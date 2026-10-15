using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;
using Dororong.Portable.Desktop;
using Dororong.Portable.Rendering;

// Windows managed-code simulation, not AppKit/macOS emulation.
var report = new Dictionary<string, object>();
var failures = new List<string>();
var warnings = new List<string>();
report["scope"] = "Distributed managed assemblies on Windows; no macOS native execution";
report["portable_sha256"] = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(SnackRenderer).Assembly.Location)));
report["core_sha256"] = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(SnackSession).Assembly.Location)));
void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
void Run(string name, Action action)
{
    try { action(); report[name] = "PASS"; }
    catch (Exception ex) { failures.Add(name + ": " + ex); report[name] = "FAIL"; }
}
void Frame(RasterImage image)
{
    Check(image.Pixels.Length == image.PixelWidth * image.PixelHeight * 4, "Frame buffer size");
    var pixels = image.Pixels.Span;
    var visible = false;
    for (var i = 0; i < pixels.Length; i += 4)
    {
        visible |= pixels[i + 3] > 0;
        Check(pixels[i] <= pixels[i + 3] && pixels[i + 1] <= pixels[i + 3] && pixels[i + 2] <= pixels[i + 3], "Invalid premultiplied pixel");
    }
    Check(visible, "Entire character disappeared");
}
Run("cold_effect_load", () =>
{
    var renderer = new SnackRenderer();
    var samples = new List<object>();
    void Measure(string name, SnackPose pose)
    {
        var allocated = GC.GetAllocatedBytesForCurrentThread(); var timer = Stopwatch.StartNew();
        var image = renderer.Render(pose); timer.Stop();
        samples.Add(new { name, milliseconds = timer.Elapsed.TotalMilliseconds, allocated_bytes = GC.GetAllocatedBytesForCurrentThread() - allocated });
        if (timer.Elapsed.TotalMilliseconds > 100) warnings.Add($"{name}: {timer.Elapsed.TotalMilliseconds:F1}ms synchronous render exceeds diagnostic 100ms stall threshold (not a Mac benchmark)");
        Frame(image);
    }
    Measure("first_chase", new(SnackPhase.Chasing, FacingDirection.Right, 0, 0, 0, 0));
    Measure("first_bite", new(SnackPhase.Eating, FacingDirection.Right, 0, 0, .001, 0));
    Measure("warm_bite", new(SnackPhase.Eating, FacingDirection.Right, 0, 0, .017, 0));
    // Evict the eating group as happens when chasing again after a meal.
    for (var i = 0; i < 8; i++) renderer.Render(new(SnackPhase.Chasing, FacingDirection.Right, 0, 0, i * .016, i * 7) { RunAmount = 1 });
    Measure("next_meal_bite", new(SnackPhase.Eating, FacingDirection.Right, 0, 0, .001, 0));
    report["effect_timings"] = samples;
});
Run("complete_meals_at_30_60_120_fps", () =>
{
    foreach (var fps in new[] { 30, 60, 120 })
    {
        var session = new SnackSession(); var renderer = new SnackRenderer(); var seen = new HashSet<SnackPhase>();
        session.Begin(FacingDirection.Right);
        for (var i = 0; i < fps * 6; i++)
        {
            var pose = session.Advance(TimeSpan.FromSeconds(1d / fps), new(true, new(100, 100)), new(100, 100), true);
            seen.Add(pose.Phase); Frame(renderer.Render(pose));
        }
        foreach (var phase in Enum.GetValues<SnackPhase>()) Check(seen.Contains(phase), $"Missing {phase} at {fps}fps");
        Check(!session.Active, "Meal did not end");
    }
});
Run("seeded_desktop_sequence", () =>
{
    var random = new Random(2151015); var brainRandom = new SeededRandom();
    var pet = new DesktopPetController(new(400, 600), brainRandom); var mapper = new MacSceneMapper();
    var phases = new HashSet<string>(); var times = new List<double>();
    for (var i = 0; i < 2400; i++)
    {
        var monitors = i % 600 < 450
            ? new DesktopMonitor[] { new(1, new(0, 0, 1440, 900)), new(2, new(-1280, -160, 1280, 800)) }
            : new DesktopMonitor[] { new(1, new(0, 0, 1440, 900)) };
        var rows = new List<MacWindowRow>();
        if (i % 320 < 240) rows.Add(new(10, 50, new(120 + i % 120, 400, 800, 300), 0, 1, true, false));
        if (i % 240 < 160) rows.Add(new(20, 60, new(200, 820, 900, 80), 20, 1, true, true));
        var scene = mapper.Build(TimeSpan.FromSeconds(i / 60d), monitors, rows, 99, 20, 101);
        var pointer = new PointD(random.Next(-1400, 1600), random.Next(-200, 1000));
        switch (i % 200)
        {
            case 0: pet.BeginSnack(); break;
            case 60: pet.CancelSnack(); break;
            case 70: pet.BeginDrag(pet.Position + new PointD(72, 50)); break;
            case 90: pet.Drop(); break;
            case 100: pet.ToggleSit(); break;
            case 120: pet.ToggleSit(); break;
            case 140: pet.SetAutomaticSleep(!pet.AutomaticSleep); break;
        }
        pet.MenuOpen = i % 180 >= 160;
        if (i % 200 == 130)
        {
            for (var y = 25; y < 120; y += 12)
            for (var x = 25; x < 120; x += 12)
                if (pet.TryBeginInteraction(pet.Position + new PointD(x, y))) goto Captured;
        }
        Captured:
        var lost = i % 311 == 310;
        var dt = TimeSpan.FromSeconds(i % 401 == 400 ? 2 : 1d / 60);
        var clock = Stopwatch.StartNew();
        pet.Advance(dt, lost ? PointerSample.Unavailable : new(true, pointer), i % 200 is >= 70 and < 90 or >= 130 and < 135,
            new(scene, i % 271 > 250 ? SceneHealth.Unavailable : SceneHealth.Fresh));
        clock.Stop(); times.Add(clock.Elapsed.TotalMilliseconds);
        Check(double.IsFinite(pet.Position.X) && double.IsFinite(pet.Position.Y), $"Nonfinite position at {i}");
        Check(Math.Abs(pet.Position.X) < 3000 && Math.Abs(pet.Position.Y) < 2000, $"Lost off desktop at {i}");
        if (lost || dt.TotalSeconds > 1) Check(!pet.Capturing && pet.Snack == SnackPhase.None, $"Input not cancelled at {i}");
        Frame(pet.Frame);
        if (i % 40 == 0) Frame(pet.ViewportFrame);
        phases.Add($"{pet.State}/{pet.Platform.Phase}/{pet.Snack}");
    }
    times.Sort();
    report["desktop_sequence"] = new { frames = times.Count, simulated_seconds = 40, seed = 2151015,
        median_ms = times[times.Count / 2], p95_ms = times[(int)(times.Count * .95)], max_ms = times[^1], states = phases.Order().ToArray() };
});
Run("coupled_desktop_feed_and_quiet_walk", () =>
{
    var pet = new DesktopPetController(new(400, 600), new SeededRandom());
    var scene = new DesktopScene(1, TimeSpan.Zero, new DesktopMonitor[] { new(1, new(0, 0, 1440, 900)) }, []);
    var seen = new HashSet<SnackPhase>(); var states = new HashSet<PetState>();
    var step = TimeSpan.FromSeconds(1d / 60);
    var distant = new PointerSample(true, new(10000, 10000));
    for (var i = 0; i < 120; i++) pet.Advance(step, distant, false, new(scene, SceneHealth.Fresh));
    pet.BeginSnack();
    for (var i = 0; i < 360; i++)
    {
        var mouth = pet.Position + new PointD(pet.Facing == FacingDirection.Right ? 53 : 91, 84);
        pet.Advance(step, new(true, mouth), false, new(scene, SceneHealth.Fresh));
        seen.Add(pet.Snack); Frame(pet.Frame);
    }
    Check(seen.Contains(SnackPhase.Eating) && seen.Contains(SnackPhase.Happy) && seen.Contains(SnackPhase.None), "Integrated feeding cycle incomplete");
    for (var i = 0; i < 1800; i++)
    {
        pet.Advance(step, distant, false, new(scene, SceneHealth.Fresh));
        states.Add(pet.State); Frame(pet.Frame);
    }
    Check(states.Contains(PetState.Walk), "Autonomous walk did not resume after meal");
    report["coupled_states"] = new { snack = seen.Select(s => s.ToString()).ToArray(), quiet = states.Select(s => s.ToString()).ToArray() };
});
report["warnings"] = warnings;
report["failures"] = failures;
var json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
Console.WriteLine(json);
if (args.Length == 1) File.WriteAllText(args[0], json + Environment.NewLine);
return failures.Count == 0 ? 0 : 1;

sealed class SeededRandom : IRandomSource
{
    private readonly Random random = new(2151015);
    public double NextUnit() => random.NextDouble();
}

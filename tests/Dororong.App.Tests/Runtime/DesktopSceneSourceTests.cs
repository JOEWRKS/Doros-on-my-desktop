using Dororong.App.Runtime;
using Dororong.App.Interop;
using Dororong.Core.Platforms;
using Xunit.Abstractions;
using System.Text.Json;

namespace Dororong.App.Tests.Runtime;

public class DesktopSceneSourceTests(ITestOutputHelper output)
{
    [Fact]
    public void Native_boundary_reads_current_monitor_metadata_without_window_content()
    {
        var before=DesktopMetadataReader.Api.GetThreadDpiAwarenessContext();
        var metadata = new DesktopMetadataReader(0).Read();
        var after=DesktopMetadataReader.Api.GetThreadDpiAwarenessContext();
        Assert.NotNull(metadata);
        Assert.Equal(before,after);
        Assert.NotEmpty(metadata.Monitors);
        Assert.All(metadata.Monitors,monitor => Assert.True(monitor.Bounds.Width > 0 && monitor.Bounds.Height > 0));
        output.WriteLine(JsonSerializer.Serialize(new { DpiContextBefore=before.ToInt64(),DpiContextAfter=after.ToInt64(),
            metadata.Monitors,metadata.Windows,metadata.Taskbars,metadata.SystemTaskbarBounds,
            Boundary="Read-only metadata. No WPF presenter was launched; cross-monitor pointer agreement unverified." }));
    }
    [Fact]
    public void Destroyed_handle_is_confirmed_absence_at_native_boundary()
    {
        Assert.Null(new DesktopMetadataReader(0).ReadWindow(0));
    }
    [Fact]
    public void Native_operation_and_numeric_error_survive_boundary_without_exception_message()
    {
        var source=new DesktopSceneSource(new Native(()=>throw new DesktopMetadataException("DwmFrame",-1)));
        Assert.Equal(SceneReadHealth.Expired,source.Read(Ms(0)).Health);
        Assert.Equal("DwmFrame:-1",source.LastFailure);
    }
    private sealed class Metadata : IDesktopMetadataReader
    {
        internal NativeDesktop? Value = new(new[] {new DesktopMonitor(1,new(0,0,1920,1080)), new DesktopMonitor(2,new(-1280,0,1280,1024))}, Array.Empty<NativeWindow>());
        public NativeDesktop? Read() => Value;
        internal void Set(params NativeWindow[] windows) => Value = new(Value!.Monitors,windows);
    }
    [Fact]
    public void Native_capture_reuses_equal_records_but_observes_changed_values_and_order()
    {
        var reader=new Metadata();var native=new DesktopSceneNative(reader);
        var first=new NativeWindow(42,7,new(0,300,500,500));
        var second=new NativeWindow(43,8,new(20,200,400,300));reader.Set(first,second);
        var a=native.Capture(Ms(0))!;
        reader.Set(first with{},second with{});var b=native.Capture(Ms(80))!;
        Assert.Same(a.Windows[0],b.Windows[0]);Assert.Same(a.Windows[1],b.Windows[1]);
        reader.Set(first with{Bounds=new(1,300,500,500)},second);
        var c=native.Capture(Ms(160))!;Assert.NotSame(b.Windows[0],c.Windows[0]);
        Assert.Equal(0,a.Windows[0].Bounds.X);Assert.Equal(1,c.Windows[0].Bounds.X);
        reader.Set(second,first);var d=native.Capture(Ms(240))!;
        Assert.Equal(43,d.Windows[0].Key.Handle);Assert.Equal(0,d.Windows[0].ZOrder);
        reader.Set(first with{IsTransientMenu=true,CanOcclude=false});
        var e=Assert.Single(native.Capture(Ms(320))!.Windows);
        Assert.True(e.IsTransientMenu);Assert.False(e.CanOcclude);
    }
    [Fact]
    public void Equal_native_metadata_does_not_allocate_every_mapped_window_again()
    {
        var reader=new Metadata();reader.Set(Enumerable.Range(1,200).Select(i=>new NativeWindow(i,7,new(i,300,500,500))).ToArray());
        var native=new DesktopSceneNative(reader);for(var i=0;i<10;i++)native.Capture(Ms(i*80));
        var start=GC.GetAllocatedBytesForCurrentThread();
        for(var i=0;i<100;i++)native.Capture(Ms(800+i*80));
        var bytes=GC.GetAllocatedBytesForCurrentThread()-start;output.WriteLine($"100 captures,200 windows: {bytes} bytes");
        Assert.InRange(bytes,1,500_000);
    }
    [Fact]
    public void Successful_absence_and_changed_pid_invalidate_handle_identity()
    {
        var reader = new Metadata(); var native = new DesktopSceneNative(reader);
        var window = new NativeWindow(42,7,new(0,300,500,500)); reader.Set(window);
        var first = native.Capture(Ms(0)); Assert.NotNull(first);
        var key = Assert.Single(first.Windows).Key;
        reader.Set(); Assert.Empty(native.Capture(Ms(80))!.Windows);
        reader.Set(window); var reappeared = Assert.Single(native.Capture(Ms(160))!.Windows).Key;
        Assert.True(reappeared.Generation > key.Generation);
        reader.Set(window with {ProcessId=8});
        var reused = Assert.Single(native.Capture(Ms(240))!.Windows).Key;
        Assert.True(reused.Generation > reappeared.Generation); Assert.Equal(8u,reused.ProcessId);
    }
    [Fact]
    public void Failed_whole_read_preserves_identity_but_invisible_is_successful_metadata()
    {
        var reader = new Metadata(); var native = new DesktopSceneNative(reader);
        var window = new NativeWindow(42,7,new(0,300,500,500)); reader.Set(window);
        var first = native.Capture(Ms(0)); Assert.NotNull(first);
        var key = Assert.Single(first.Windows).Key;
        var saved = reader.Value; reader.Value = null; Assert.Null(native.Capture(Ms(80)));
        reader.Value=saved; reader.Set(window with {Visible=false});
        var hidden=Assert.Single(native.Capture(Ms(160))!.Windows);
        Assert.False(hidden.Visible); Assert.Equal(key,hidden.Key);
    }
    [Fact]
    public void Taskbar_secondary_monitor_hides_at_two_pixels_and_requires_stable_reappearance()
    {
        var reader = new Metadata(); var native = new DesktopSceneNative(reader);
        var bar = new NativeWindow(44,9,new(-1280,1022,1280,40),Taskbar:true,HorizontalTaskbar:true);
        reader.Set(bar); var first=native.Capture(Ms(0)); Assert.NotNull(first);
        Assert.False(Assert.Single(first.Windows).CanSupport);
        reader.Set(bar with {Bounds=new(-1280,984,1280,40)});
        Assert.False(Assert.Single(native.Capture(Ms(80))!.Windows).CanSupport);
        Assert.True(Assert.Single(native.Capture(Ms(160))!.Windows).CanSupport);
        reader.Set(bar);
        Assert.False(Assert.Single(native.Capture(Ms(240))!.Windows).CanSupport);
    }
    [Fact]
    public void Metadata_order_and_occlusion_survive_non_supporting_popup()
    {
        var reader = new Metadata(); var native = new DesktopSceneNative(reader);
        reader.Set(new(7,2,new(0,0,40,40),CanSupport:false),new(8,3,new(0,30,500,400)));
        var scene=native.Capture(Ms(0)); Assert.NotNull(scene);
        Assert.False(scene.Windows[0].CanSupport); Assert.True(scene.Windows[0].CanOcclude);
        Assert.Equal(0,scene.Windows[0].ZOrder); Assert.Equal(1,scene.Windows[1].ZOrder);
    }
    private static DesktopScene Scene(long revision = 1) => new(revision, TimeSpan.Zero,
        new[] { new DesktopMonitor(1, new(0,0,1920,1080)) },
        new[] { new DesktopWindow(new(42,7,1),new(100,400,800,400),0,true,false,false,false,true,true,false,false) });
    private sealed class Native(params Func<DesktopScene?>[] reads) : IDesktopSceneNative
    {
        private int index;
        public DesktopScene? Capture(TimeSpan now) => reads[Math.Min(index++,reads.Length-1)]();
    }
    private static TimeSpan Ms(int value) => TimeSpan.FromMilliseconds(value);

    [Fact]
    public void Polling_publishes_one_capture_until_eighty_milliseconds()
    {
        var source = new DesktopSceneSource(new Native(() => Scene(1), () => Scene(2)));
        Assert.Equal(SceneReadHealth.Fresh,source.Read(Ms(0)).Health);
        foreach (var ms in new[] {16,32,48})
        {
            var read = source.Read(Ms(ms));
            Assert.Equal(SceneReadHealth.Cached,read.Health);
            Assert.Equal(1,read.Scene!.Revision);
        }
        Assert.Equal(2,source.Read(Ms(80)).Scene!.Revision);
    }
    [Fact]
    public void Two_failures_retain_support_then_recover()
    {
        var source = new DesktopSceneSource(new Native(() => Scene(), () => null, () => throw new InvalidOperationException("private title"), () => Scene(2)));
        source.Read(Ms(0));
        foreach (var ms in new[] {80,160})
        {
            var read = source.Read(Ms(ms));
            Assert.Equal(SceneReadHealth.TemporarilyUnavailable,read.Health);
            Assert.Single(read.Scene!.Windows);
        }
        Assert.DoesNotContain("private title",source.LastFailure ?? "");
        Assert.NotNull(source.LastFailure);
        Assert.Equal(SceneReadHealth.Fresh,source.Read(Ms(240)).Health);
        Assert.Equal(2,source.Read(Ms(241)).Scene!.Revision);
    }
    [Fact]
    public void Expiration_is_checked_even_between_polls()
    {
        var source = new DesktopSceneSource(new Native(() => Scene(), () => null));
        source.Read(Ms(0));
        Assert.Equal(SceneReadHealth.TemporarilyUnavailable,source.Read(Ms(480)).Health);
        var expired = source.Read(Ms(500));
        Assert.Equal(SceneReadHealth.Expired,expired.Health);
        Assert.Null(expired.Scene);
    }
    [Fact]
    public void Successful_empty_read_removes_support()
    {
        var source = new DesktopSceneSource(new Native(() => Scene(), () => Scene(2) with { Windows = Array.Empty<DesktopWindow>() }));
        source.Read(Ms(0));
        var empty = source.Read(Ms(80));
        Assert.Equal(SceneReadHealth.Fresh,empty.Health);
        Assert.Empty(empty.Scene!.Windows);
    }
    [Fact]
    public void Clock_rollback_does_not_capture_or_extend_stale_lifetime()
    {
        var source = new DesktopSceneSource(new Native(() => Scene(1), () => null));
        source.Read(Ms(100));
        Assert.Equal(SceneReadHealth.Cached,source.Read(Ms(20)).Health);
        Assert.Equal(SceneReadHealth.Expired,source.Read(Ms(600)).Health);
    }
    [Fact]
    public void Published_snapshot_is_detached_from_mutable_input()
    {
        var windows = Scene().Windows.ToList();
        var monitors = Scene().Monitors.ToList();
        var source = new DesktopSceneSource(new Native(() => Scene() with { Windows=windows, Monitors=monitors }));
        var read = source.Read(Ms(0));
        windows.Clear(); monitors.Clear();
        Assert.NotNull(read.Scene);
        Assert.Single(read.Scene!.Windows); Assert.Single(read.Scene.Monitors);
        Assert.Throws<NotSupportedException>(() => ((IList<DesktopWindow>)read.Scene.Windows).Clear());
    }
}

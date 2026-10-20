using System.Reflection;
using System.Text.Json;
using Dororong.App.Interaction;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;

if(args.Length!=1)throw new ArgumentException("Output JSON required");
var ordinary=new List<object>();
foreach(var strength in new[]{.4,1d})foreach(var distance in new[]{0d,18d,36d})
for(var ms=0;ms<=440;ms+=5){var p=HeadLandingMotion.Sample(new(0,0),distance,strength,ms,new(-1000,-1000,2000,2000),new(144,144));ordinary.Add(new{strength,distance,ms,y=p.WindowPosition.Y,compression=p.Compression});}
// Probe the existing private presentation sampler directly, not a duplicated formula.
// State-machine/height-trigger behavior is covered separately by the existing tests.
var motion=new PlatformMotion();
var flags=BindingFlags.Instance|BindingFlags.NonPublic;
typeof(PlatformMotion).GetField("extremeLanding",flags)!.SetValue(motion,true);
var set=typeof(PlatformMotion).GetMethod("Set",flags)!;
var strong=new List<object>();
for(var ms=0;ms<=1800;ms+=5){typeof(PlatformMotion).GetField("phaseSeconds",flags)!.SetValue(motion,ms/1000d);var p=(PlatformPose)set.Invoke(motion,new object[]{PlatformPhase.Landing,new PointD(0,0)})!;strong.Add(new{ms,squash=p.Squash,spread=p.LegSpread,lift=-p.Position.Y});}
Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(args[0]))!);
File.WriteAllText(args[0],JsonSerializer.Serialize(new{ordinary,strong},new JsonSerializerOptions{WriteIndented=true}));
Console.WriteLine($"Actual C# samples: {ordinary.Count} ordinary, {strong.Count} strong");

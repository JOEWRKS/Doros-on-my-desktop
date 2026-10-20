using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;
using Dororong.Core.Geometry;

namespace Dororong.App.Tests.Controls;

public sealed class HighResolutionPerformanceTests
{
    [Fact]
    public void Landing_inverse_matches_original_twenty_step_bisection(){
        var method=typeof(HighResolutionGrabRaster).GetMethod("InverseSpread",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static)!;
        foreach(var amount in new[]{.01,.2,.5,.75,1})for(var yi=0;yi<39;yi++)for(var xi=0;xi<250;xi++){
            var y=66+yi*.47;var x=20+xi*.29;
            var reach=24*amount*HighResolutionGrabRaster.Smooth((y-66)/17);
            var lo=x-reach;var hi=x+reach;
            for(var k=0;k<20;k++){var u=(lo+hi)/2;if(u+reach*(2*HighResolutionGrabRaster.Smooth((u-46)/14)-1)<x)lo=u;else hi=u;}
            var actual=(double)method.Invoke(null,new object[]{x,y,amount})!;
            Assert.InRange(Math.Abs(actual-(lo+hi)/2),0,1e-12);
        }
    }
    [Fact]
    public void Cheek_curve_fast_lookup_is_identical_to_legacy_bisection(){
        foreach(var amount in new[]{0d,.1,.5,1})for(var i=0;i<=11000;i++){
            var y=53+i/1000d;
            Assert.Equal(PolishedCheekRenderer.Edge(y,amount),HighResolutionCheekSelection.FastEdge(y,amount));
        }
    }
    [Fact]
    public void Record_source_sampler_cost_without_file_encoding_in_timing()=>CheekProductTests.Sta(()=>{
        var output=Environment.GetEnvironmentVariable("DORORONG_PERF_OUTPUT");
        if(output is null)return;
        Directory.CreateDirectory(output);var rows=new List<object>();var raster=new HighResolutionGrabRaster();
        foreach(var kind in new[]{"idle","track","cheek","grab","flutter","landing","run","eat"}){
            var times=new List<double>();
            for(var i=0;i<18;i++){
                var face=HighResolutionFacePose.Neutral;var inverse=Matrix.Identity;var progress=0d;var near=default(PointD);var spread=0d;
                switch(kind){
                    case "idle":face=face with {Breathing=i/18d};break;
                    case "track":face=face with {Head=HuntRenderer.HeadTransform(156,new(.4,.2,(i-9)*.02)),EyeX=.4,EyeY=.2};break;
                    case "cheek":face=face with {Pull=20*i/18d,EyeFollow=10,HairFollow=10};break;
                    case "grab":progress=1;inverse.RotateAt(i*2,43,32);near=new(1,-1);break;
                    case "flutter":progress=1;face=face with {FlutterPhase=i/18d};break;
                    case "landing":spread=.5+i/36d;inverse.ScaleAt(.9,1.2,48,87);break;
                    case "run":face=face with {MotionFrame=HighResolutionLocomotion.Run(1,i*66d/18)};break;
                    case "eat":face=face with {Closed=true,MouthAge=.5+i/18d};break;
                }
                var timer=Stopwatch.StartNew();var image=raster.Render(progress,inverse,new(37.5,28.25),near,default,spread,1.5,face);timer.Stop();
                if(i>=3)times.Add(timer.Elapsed.TotalMilliseconds);
                if(i==12){using var stream=File.Create(Path.Combine(output,kind+".png"));var enc=new PngBitmapEncoder();enc.Frames.Add(BitmapFrame.Create(image));enc.Save(stream);}
            }
            times.Sort();rows.Add(new{kind,mean=times.Average(),p50=times[times.Count/2],p95=times[(int)Math.Floor((times.Count-1)*.95)],max=times[^1]});
        }
        File.WriteAllText(Path.Combine(output,"timings.json"),JsonSerializer.Serialize(rows,new JsonSerializerOptions{WriteIndented=true}));
    });
}

using System.IO;
using System.Diagnostics;
using System.Text.Json;
using System.Windows;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;
using Dororong.App.Interaction;

namespace Dororong.App.Tests.Runtime;

public partial class PetLoopPlatformTests
{
    [Theory]
    [InlineData(1)] [InlineData(1.1)] [InlineData(1.25)] [InlineData(1.35)] [InlineData(1.5)]
    public void High_resolution_cancel_partial_release_and_regrab_keep_real_loop_position(double scale)=>Controls.CheekProductTests.Sta(()=>
    {
        using var h=new Harness(realPresenter:true,highResolutionScale:scale);h.Start();
        var p=h.Presenter!;
        void Press(){
            var image=p.HighResolutionImage!;Assert.NotNull(image);
            var local=p.HighResolutionSourceToPresenter(new(37.5,28.25));
            var args=p.CreateDirectPress(image,p.TranslatePoint(local,image));Assert.NotNull(args);
            h.Down=true;h.Pointer=new(true,h.Position+args!.WindowLocalPosition);h.Loop.NotifyDirectInteractionPressed(args);
        }
        var sole=p.MeasurePlatformContact()!.Value.SoleY;h.Native.Scene=Scene(y:100+sole);h.Tick(80);
        // Partial entry, release while the support has moved, then regrab.
        Press();h.Tick();h.Pointer=new(true,h.Pointer.Position+new PointD(12,-12));h.Tick();
        h.Down=false;h.Native.Scene=Scene(120,70+sole);h.Tick();
        Assert.Equal(h.Position,h.Core.Position);Assert.NotNull(h.LastPose);
        Press();var before=h.Position;h.Tick();Assert.Equal(before,h.Position);
        h.Pointer=new(true,h.Pointer.Position+new PointD(35,-40));h.Tick(80);
        h.Loop.NotifyDirectInteractionCanceled();h.Down=false;h.Pointer=Dororong.Core.Behavior.PointerSample.Unavailable;
        h.Native.Scene=Scene(windows:false);h.Tick();
        Assert.Equal(DirectInteractionTarget.None,h.Direct.Target);Assert.Equal(h.Position,h.Core.Position);
        Assert.Equal(PlatformPhase.Falling,h.LastPose?.Phase);
        for(var i=0;i<120;i++)h.Tick(20);
        Assert.Equal(PlatformPhase.Supported,h.LastPose?.Phase);
        Assert.InRange(h.Position.Y+p.MeasurePlatformContact()!.Value.SoleY,599,601);
        Assert.Equal(h.Captures,h.Releases);
    });

    [Theory]
    [InlineData(1)] [InlineData(1.1)] [InlineData(1.25)] [InlineData(1.35)] [InlineData(1.5)]
    public void High_resolution_actual_loop_grab_release_moving_and_removed_support(double scale)=>Controls.CheekProductTests.Sta(()=>
    {
        using var h=new Harness(realPresenter:true,highResolutionScale:scale);h.Start();
        var p=h.Presenter!;var rows=new List<object>();var timings=new List<double>();var rasterTimings=new List<double>();var index=0;
        var output=Environment.GetEnvironmentVariable("DORORONG_R51_OUTPUT");
        if(output is not null)Directory.CreateDirectory(output);
        void Save(string phase){
            Assert.NotNull(p.HighResolutionImage);var source=(BitmapSource)p.HighResolutionImage!.Source;
            var file=$"runtime-{scale:F2}-{index++:D3}-{phase}.png";
            if(output is not null){using var stream=File.Create(Path.Combine(output,file));var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(source));encoder.Save(stream);}
            rows.Add(new{file,phase,position=h.Position,contact=p.MeasurePlatformContact(),direct=h.Direct.Phase.ToString(),platform=h.LastPose?.Phase.ToString(),rasterizations=p.HighResolutionRasterizations,diagnostic=p.HighResolutionDiagnostic,sway=h.LastPose?.Sway,spread=h.LastPose?.LegSpread});
        }
        void Tick(string phase,int ms=20,bool save=true){var previous=p.HighResolutionRenderMilliseconds;var count=p.HighResolutionRasterizations;var timer=Stopwatch.StartNew();h.Tick(ms);timings.Add(timer.Elapsed.TotalMilliseconds);rasterTimings.Add(p.HighResolutionRenderMilliseconds-previous);Assert.Equal(h.Position,h.Core.Position);Assert.InRange(h.WritesThisTick,0,1);Assert.InRange(p.HighResolutionRasterizations-count,0,1);if(save)Save(phase);}
        var sole=p.MeasurePlatformContact()!.Value.SoleY;
        h.Native.Scene=Scene(y:100+sole);Tick("rest",80);
        h.Native.Scene=Scene(120,70+sole);Tick("support-move",80);
        Assert.InRange(h.Position.X,119.9,120.1);Assert.InRange(h.Position.Y,69.9,70.1);
        var image=p.HighResolutionImage!;var at=new Point((80+37.5)*scale,(80+28.25)*scale);
        var press=p.CreateDirectPress(image,at);Assert.NotNull(press);Assert.Equal(DirectInteractionTarget.Body,press!.Target);
        h.Down=true;h.Pointer=new(true,h.Position+press.WindowLocalPosition);h.Loop.NotifyDirectInteractionPressed(press);Tick("press");
        for(var i=0;i<12;i++){h.Pointer=new(true,h.Pointer.Position+new PointD(4,-5));Tick("entry");}
        Assert.True(h.Direct.RequiresCapture);
        for(var i=0;i<10;i++){h.Pointer=new(true,h.Pointer.Position+new PointD(i<5?8:-8,0));h.Native.Scene=Scene(windows:false);Tick("swing");}
        h.Down=false;var sawFalling=false;var sawLanding=false;
        for(var i=0;i<60;i++){
            if(i==1)h.Pointer=Dororong.Core.Behavior.PointerSample.Unavailable;
            Tick("release-fall-land",20, i<12||i%3==0);
            sawFalling|=h.LastPose?.Phase==PlatformPhase.Falling;sawLanding|=h.LastPose?.Phase==PlatformPhase.Landing;
        }
        Assert.True(sawFalling);Assert.True(sawLanding);Assert.Equal(DirectInteractionTarget.None,h.Direct.Target);
        Assert.InRange(h.Position.Y+p.MeasurePlatformContact()!.Value.SoleY,599,601);
        var landed=h.Position;h.Native.Scene=Scene(y:600);Tick("settled",80);Assert.InRange(Math.Abs(h.Position.Y-landed.Y),0,1);
        // Repeat on a new raised support; removal must start real falling.
        h.Platforms.Reset(new(100,100));h.Position=new(100,100);h.Native.Scene=Scene(y:100+p.MeasurePlatformContact()!.Value.SoleY);Tick("new-support",80);
        h.Native.Scene=Scene(windows:false);Tick("support-removed",80);Assert.Equal(PlatformPhase.Falling,h.LastPose?.Phase);
        // A genuine long fall through the same loop, not a hand-written pose.
        h.Position=new(100,0);h.Platforms.Reset(h.Position);
        h.Native.Scene=Scene(windows:false) with {Monitors=[new(1,new(0,0,800,1000))]};
        var extreme=false;var airborneExpansion=false;var firstContact=false;
        int? heldRasterizations=null;
        for(var i=0;i<160;i++){
            Tick("strong-fall-land",20,i%4==0);
            extreme|=h.LastPose is {IsExtremeLanding:true};
            if(h.LastPose is {Phase:PlatformPhase.Falling,IsExtremeLanding:true,LegSpread:>0} air){
                airborneExpansion=true;Assert.Equal(air.LegSpread,p.LastRenderedSpread);
                Assert.True(h.Position.Y+p.MeasurePlatformContact()!.Value.SoleY<1000);
            }
            if(!firstContact&&h.LastPose is {Phase:PlatformPhase.Landing,IsExtremeLanding:true} landing){
                firstContact=true;Assert.True(airborneExpansion);Assert.Equal(1,landing.LegSpread);
                Assert.Equal(1,p.LastRenderedSpread);
            }
            if(h.LastPose is {IsExtremeLanding:true,LegSpread:1,Squash:.574,Sway:0}){
                if(heldRasterizations is { } previous)Assert.Equal(previous,p.HighResolutionRasterizations);
                heldRasterizations=p.HighResolutionRasterizations;
            }else heldRasterizations=null;
        }
        Assert.True(extreme);Assert.True(firstContact);Assert.Equal(PlatformPhase.Supported,h.LastPose?.Phase);
        Assert.InRange(h.Position.Y+p.MeasurePlatformContact()!.Value.SoleY,999,1001);
        if(output is not null)File.WriteAllText(Path.Combine(output,$"runtime-{scale:F2}.json"),JsonSerializer.Serialize(new{scale,rows,meanMs=timings.Average(),maxMs=timings.Max(),timings,rasterTimings},new JsonSerializerOptions{WriteIndented=true}));
    });
}

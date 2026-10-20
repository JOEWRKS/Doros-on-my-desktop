using System.Windows;
using Dororong.App.Controls;
using Dororong.App.Interaction;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;

namespace Dororong.App.Tests.Runtime;

public partial class PetLoopPlatformTests
{
    private static void SaveReadiness(DororongPresenter p,double scale,string name){
        var output=Environment.GetEnvironmentVariable("DORORONG_R56_OUTPUT");if(output is null)return;
        System.IO.Directory.CreateDirectory(output);
        using var stream=System.IO.File.Create(System.IO.Path.Combine(output,$"review-{scale:F2}-{name}.png"));
        var encoder=new System.Windows.Media.Imaging.PngBitmapEncoder();
        encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create((System.Windows.Media.Imaging.BitmapSource)p.HighResolutionImage!.Source));encoder.Save(stream);
    }
    [Theory]
    [InlineData(1)] [InlineData(1.25)] [InlineData(1.5)]
    public void High_resolution_bottom_cue_and_release_are_reachable(double scale)=>Controls.CheekProductTests.Sta(()=>
    {
        using var h=new Harness(realPresenter:true,perchEnabled:true,highResolutionScale:scale);
        h.Native.Scene=new(2,TimeSpan.Zero,[new(1,new(0,0,800,600))],
            [new(new(2,2,2),new(0,560,800,40),0,true,false,false,false,true,true,true,true)]);
        h.Start();var p=h.Presenter!;
        var local=p.HighResolutionSourceToPresenter(new(37.5,28.25));
        var press=p.CreateDirectPress(p.HighResolutionImage!,p.TranslatePoint(local,p.HighResolutionImage!));
        Assert.NotNull(press);h.Down=true;h.Pointer=new(true,h.Position+press!.WindowLocalPosition);
        h.Loop.NotifyDirectInteractionPressed(press);h.Tick();
        h.Pointer=new(true,h.Pointer.Position+new PointD(100,0));h.Tick();h.Tick();
        h.Pointer=new(true,new(h.Pointer.Position.X,599));
        for(var i=0;i<20;i++)h.Tick();
        Assert.True(h.Direct.IsPerchReady,$"position={h.Position}; contact={h.Platforms.Contact}; grip={p.MeasureEdgePerchContact(h.Core.Facing)}");
        var frames=new HashSet<string>();
        for(var i=0;i<16;i++){
            h.Tick();Assert.True(h.Direct.IsPerchReady);SaveReadiness(p,scale,$"flutter-{i:D2}");
            var b=(System.Windows.Media.Imaging.BitmapSource)p.HighResolutionImage!.Source;
            var pixels=new byte[b.PixelWidth*b.PixelHeight*4];b.CopyPixels(pixels,b.PixelWidth*4,0);
            frames.Add(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(pixels)));
        }
        Assert.True(frames.Count>8);
        h.Down=false;h.Tick();Assert.Equal(EdgePerchPhase.Entering,h.Platforms.PerchPhase);
        for(var i=0;i<20;i++){h.Tick();SaveReadiness(p,scale,$"attach-{i:D2}");}
        Assert.Equal(EdgePerchPhase.Attached,h.Platforms.PerchPhase);
    });

    [Theory]
    [InlineData(1)] [InlineData(1.25)] [InlineData(1.5)]
    public void High_resolution_hover_departure_recovers_displayed_body(double scale)=>Controls.CheekProductTests.Sta(()=>
    {
        using var h=new Harness(realPresenter:true,highResolutionScale:scale,huntingPresentation:true);
        h.Native.Scene=Scene(windows:false);h.Start();
        for(var i=0;i<100;i++)h.Tick(20);
        Assert.Equal(PlatformPhase.Supported,h.LastPose?.Phase);var p=h.Presenter!;
        for(var i=0;i<15;i++){h.Pointer=new(true,h.Position+new PointD(72*scale,80*scale));h.Tick(20);}
        var before=HuntPose.At(p.HuntingVisualFrame/60d).Amount;Assert.True(before>.1,$"frame={p.HuntingVisualFrame}; state={h.Core.State}; position={h.Position}");
        SaveReadiness(p,scale,"hover-00");
        h.Pointer=Dororong.Core.Behavior.PointerSample.Unavailable;h.Tick(20);SaveReadiness(p,scale,"hover-01");
        var after=HuntPose.At(p.HuntingVisualFrame/60d).Amount;
        Assert.InRange(after,.01,before+.05);
        for(var i=0;i<30;i++){h.Tick(20);Assert.Equal(PlatformPhase.Supported,h.LastPose?.Phase);SaveReadiness(p,scale,$"hover-{i+2:D2}");}
        Assert.Equal(0,HuntPose.At(p.HuntingVisualFrame/60d).Amount);
    });
}

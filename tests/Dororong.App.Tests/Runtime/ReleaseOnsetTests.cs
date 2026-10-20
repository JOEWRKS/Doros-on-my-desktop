using Dororong.App.Controls;
using Dororong.App.Interaction;
using Dororong.App.Runtime;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Media.Imaging;

namespace Dororong.App.Tests.Runtime;

public class ReleaseOnsetTests(Xunit.Abstractions.ITestOutputHelper output)
{
    private sealed class Native:IDesktopSceneNative {
        public DesktopScene Capture(TimeSpan now)=>new(1,now,[new(1,new(0,0,2400,2000))],[]);
    }
    [Theory]
    [InlineData(0,1)] [InlineData(45,1)] [InlineData(-45,1)]
    [InlineData(0,1.1)] [InlineData(45,1.1)] [InlineData(-45,1.1)]
    [InlineData(0,1.25)] [InlineData(45,1.25)] [InlineData(-45,1.25)]
    [InlineData(0,1.35)] [InlineData(45,1.35)] [InlineData(-45,1.35)]
    [InlineData(0,1.5)] [InlineData(45,1.5)] [InlineData(-45,1.5)]
    public void Visible_center_follows_gravity_without_a_recovery_speed_boost(double angle,double scale)=>Run(angle,scale,false);

    [Theory]
    [InlineData(0,1)] [InlineData(45,1)] [InlineData(-45,1)]
    [InlineData(0,1.5)] [InlineData(45,1.5)] [InlineData(-45,1.5)]
    public void Regrab_preserves_the_translated_release_anchor(double angle,double scale)=>Run(angle,scale,true);

    private void Run(double angle,double scale,bool regrab)=>Controls.CheekProductTests.Sta(()=>{
        var size=144*scale;var extent=size+192;
        var p=new DororongPresenter{HighResolutionGrabEnabled=true,HighResolutionScale=scale,Width=size,Height=size};
        ((Canvas)p.Content).Width=size;((Canvas)p.Content).Height=size;
        var grid=new Grid{Width=extent,Height=extent};grid.Children.Add(p);
        p.HorizontalAlignment=HorizontalAlignment.Center;p.VerticalAlignment=VerticalAlignment.Center;
        void Layout(){grid.Measure(new(extent,extent));grid.Arrange(new(0,0,extent,extent));grid.UpdateLayout();}
        var position=new PointD(400,100);var time=TimeSpan.Zero;EventHandler? tick=null;
        var down=false;var pointer=PointerSample.Unavailable;DirectInteractionSnapshot direct=default;
        PlatformPose? pose=null;Exception? fault=null;
        var runtime=new PetPlatformRuntime(new(new Native()),_=>new(new(),new(),1,1),p.MeasurePlatformGeometry,
            (v,s)=>{pose=v;p.ApplyPlatformPose(v,s);});
        var host=new PetLoopHost(()=>new(0,0,2400,2000),()=>new(size,size),()=>new(4,4),()=>pointer,()=>down,
            ()=>position,v=>position=v,(s,d)=>{},()=>true,()=>{},(s,d,delta)=>{
                direct=d;if(d.Target==DirectInteractionTarget.Body&&d.RequiresCapture)d=d with{HeadSwingDegrees=angle};
                p.Render(s,d,delta);Layout();}){Platforms=runtime,BeginPlatformFrame=p.BeginHighResolutionPlatformFrame};
        using var loop=new PetLoop(new(()=>time,()=>{},()=>{}),new(h=>tick+=h,h=>tick-=h,()=>{},()=>{}),host,
            start=>new(BehaviorTuning.Default with{IdleMin=TimeSpan.FromMinutes(10),IdleMax=TimeSpan.FromMinutes(10)},new SeededRandomSource(1),start));
        loop.Faulted+=(_,e)=>fault=e;
        // Start uses the untimed render callback too.
        p.Render(new(PetState.Idle,position,FacingDirection.Right,0,false,null),DirectInteractionSnapshot.None);Layout();
        loop.Start();
        void Tick(){time+=TimeSpan.FromMilliseconds(16);tick?.Invoke(null,EventArgs.Empty);Assert.Null(fault);}
        for(var i=0;i<4;i++)Tick();
        var point=p.HighResolutionSourceToPresenter(new(37.5,28.25));var image=p.HighResolutionImage!;
        var press=p.CreateDirectPress(image,p.TranslatePoint(point,image));Assert.NotNull(press);Assert.Equal(DirectInteractionTarget.Body,press!.Target);
        var origin=position+press.WindowLocalPosition;pointer=new(true,origin);down=true;loop.NotifyDirectInteractionPressed(press);Tick();
        for(var i=1;i<=20;i++){pointer=new(true,origin+new PointD(0,-800d*i/20));Tick();}
        for(var i=0;i<4;i++)Tick();
        var startY=position.Y;var head0=p.HighResolutionSourceToPresenter(new(37.5,28.25)).Y+position.Y;
        var sole0=position.Y+p.MeasurePlatformGeometry()!.Value.Contact.SoleY;
        var initialGeometry=p.MeasurePlatformGeometry()!.Value;
        var previousCenter=position.Y+initialGeometry.Bounds.Y+initialGeometry.Bounds.Height/2;
        down=false;
        var export=Environment.GetEnvironmentVariable("DORORONG_RELEASE_ONSET_OUTPUT");
        if(export is not null)Directory.CreateDirectory(export);
        for(var i=1;i<=20;i++){
            var count=p.HighResolutionRasterizations;
            Tick();Assert.InRange(p.HighResolutionRasterizations-count,0,1);
            var head=p.HighResolutionSourceToPresenter(new(37.5,28.25));var g=p.MeasurePlatformGeometry()!.Value;
            output.WriteLine($"{i*16}ms {pose?.Phase} {direct.Phase} recovery={direct.ReleaseProgress:F3} host={position.Y-startY:F4} head={position.Y+head.Y-head0:F4} sole={position.Y+g.Contact.SoleY:F4}");
            Assert.Equal(PlatformPhase.Falling,pose?.Phase);
            var expectedStep=900*(Math.Pow(i*.016,2)-Math.Pow((i-1)*.016,2));
            var worldCenter=position.Y+g.Bounds.Y+g.Bounds.Height/2;
            output.WriteLine($"centerStep={worldCenter-previousCenter:F4}, gravityStep={expectedStep:F4}");
            Assert.InRange(Math.Abs(worldCenter-previousCenter-expectedStep),0,1.1);
            previousCenter=worldCenter;
            if(i<=12)Assert.Equal((i-1)*16d/180,direct.ReleaseProgress,6);
            if(export is not null&&i is 1 or 3 or 6 or 10 or 13){
                using var file=File.Create(Path.Combine(export,$"release-{scale:F2}-{angle:F0}-{i:D2}.png"));
                var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create((BitmapSource)p.HighResolutionImage!.Source));encoder.Save(file);
            }
            if(regrab&&i==6){
                var before=position+new PointD(head.X,head.Y);
                var again=p.CreateDirectPress(image,p.TranslatePoint(head,image));
                Assert.NotNull(again);Assert.Equal(DirectInteractionTarget.Body,again!.Target);
                pointer=new(true,position+again.WindowLocalPosition);down=true;
                loop.NotifyDirectInteractionPressed(again);Tick();
                var after=p.HighResolutionSourceToPresenter(new(37.5,28.25));
                Assert.InRange(Math.Abs(position.X+after.X-before.X),0,1.1);
                Assert.InRange(Math.Abs(position.Y+after.Y-before.Y),0,1.1);
                Assert.Equal(DirectInteractionPhase.BodyPending,direct.Phase);
                pointer=new(true,pointer.Position+new PointD(20,-20));Tick();
                Assert.True(direct.RequiresCapture);
                return;
            }
        }
    });
}

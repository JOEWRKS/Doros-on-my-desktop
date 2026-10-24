using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Dororong.App.Tests.Runtime;

public partial class PetLoopPlatformTests
{
    [Theory][InlineData(1,-1)][InlineData(1,1)][InlineData(1.25,-1)][InlineData(1.25,1)][InlineData(1.5,-1)][InlineData(1.5,1)]
    public void EscalationSnatchesOnlyAtActualMouthAndReturnsToFloor(double scale,int side)=>Controls.CheekProductTests.Sta(()=>{
        using var h=new Harness(realPresenter:true,snackPresentation:true,highResolutionScale:scale);
        h.Native.Scene=Scene(windows:false);h.Start();for(var i=0;i<240;i++)h.Tick();h.Loop.ToggleSnack();
        var rows=new List<string>{"frame,phase,age,x,y,mouthX,mouthY,pointerX,pointerY"};
        var frames=new List<BitmapSource>();
        for(var attempt=1;attempt<=6;attempt++){
            h.Pointer=new(true,h.Position+h.Presenter!.GetSnackMouth()+new PointD(side*100,0));h.Tick();
            for(var i=0;i<18;i++){h.Pointer=new(true,h.Position+h.Presenter.GetSnackMouth());h.Tick();}
            Assert.Equal(SnackPhase.Chasing,h.Snack.Phase);
            h.Pointer=new(true,h.Position+h.Presenter.GetSnackMouth()+new PointD(side*80,attempt==6?-180:0));h.Tick();
            if(attempt<6){
                Assert.Equal(SnackPhase.Angry,h.Snack.Phase);Assert.Equal(attempt,h.Snack.WithdrawalStreak);
                var lifted=false;var count=0;var ticks=0;
                while(h.Snack.Phase==SnackPhase.Angry&&ticks++<160){
                    if(attempt==2&&h.Snack.Age>=.18&&h.Snack.Age<=1.30){
                        Assert.True(h.Presenter.HighResolutionFace.StompReach>=3.2-1e-12);
                        Assert.True(h.Presenter.HighResolutionFace.StompCrouch>=.12-1e-12);
                    }
                    var up=h.Presenter.HighResolutionFace.StompLift>.001;
                    if(up&&!lifted)count++;lifted=up;h.Tick();
                }
                Assert.Equal(SnackAngerMotion.StampCount(attempt),count);
                Assert.Equal(SnackPhase.Chasing,h.Snack.Phase);
                Assert.InRange(ticks*.016,SnackAngerMotion.Duration(attempt)-.001,SnackAngerMotion.Duration(attempt)+.017);
                continue;
            }
            Assert.Equal(SnackPhase.Snatching,h.Snack.Phase);Assert.True(h.Snack.CarriesJewel);
            var start=h.Position;var caught=false;
            for(var frame=0;frame<110;frame++){
                // Move the jewel once mid-flight: the bite must follow the live
                // cursor, not claim success at the stale launch target.
                if(frame==6)h.Pointer=new(true,h.Pointer.Position+new PointD(side*12,-10));
                h.Tick();var mouth=h.Position+h.Presenter.GetSnackMouth();
                rows.Add($"{frame},{h.Snack.Phase},{h.Snack.Age:R},{h.Position.X:R},{h.Position.Y:R},{mouth.X:R},{mouth.Y:R},{h.Pointer.Position.X:R},{h.Pointer.Position.Y:R}");
                if(!caught&&h.Snack.Phase==SnackPhase.Eating){
                    caught=true;var error=mouth-h.Pointer.Position;
                    Assert.InRange(Math.Sqrt(error.X*error.X+error.Y*error.Y),0,8*scale);
                    Assert.True(h.Position.Y<start.Y-100);Assert.True(frame*.016<.4);
                    Assert.False(h.Snack.CarriesJewel);Assert.Equal(0,h.Snack.WithdrawalStreak);
                }
                if(Environment.GetEnvironmentVariable("DORORONG_ESCALATION_PROOF") is not null&&scale==1.5&&side==-1){
                    var visual=new DrawingVisual();using(var dc=visual.RenderOpen()){
                        dc.DrawRectangle(Brushes.LightBlue,null,new Rect(0,0,800,600));
                        if(h.Snack.CarriesJewel)dc.DrawEllipse(Brushes.DodgerBlue,new Pen(Brushes.White,1),new Point(h.Pointer.Position.X,h.Pointer.Position.Y),6,6);
                        var image=h.Presenter.HighResolutionImage!;var origin=image.TranslatePoint(new Point(),h.Presenter);
                        dc.DrawImage(image.Source,new Rect(h.Position.X+origin.X,h.Position.Y+origin.Y,image.ActualWidth,image.ActualHeight));
                        dc.DrawText(new FormattedText($"{frame*.016:F3}s {h.Snack.Phase}",System.Globalization.CultureInfo.InvariantCulture,
                            FlowDirection.LeftToRight,new Typeface("Arial"),16,Brushes.Black,1),new Point(8,8));
                    }
                    var bitmap=new RenderTargetBitmap(800,600,96,96,PixelFormats.Pbgra32);bitmap.Render(visual);frames.Add(bitmap);
                }
            }
            Assert.True(caught);Assert.Null(h.Fault);Assert.Equal(0,h.Captures);
            Assert.InRange(Math.Abs(h.Position.Y-start.Y),0,2*scale);
        }
        if(Environment.GetEnvironmentVariable("DORORONG_ESCALATION_PROOF") is {} output){
            System.IO.Directory.CreateDirectory(output);System.IO.File.WriteAllLines(System.IO.Path.Combine(output,$"snatch-{scale}-{side}.csv"),rows);
            if(frames.Count>0){
                var gif=new GifBitmapEncoder();foreach(var bitmap in frames){var metadata=new BitmapMetadata("gif");metadata.SetQuery("/grctlext/Delay",(ushort)2);
                    gif.Frames.Add(BitmapFrame.Create(bitmap,null,metadata,null));}
                using(var file=System.IO.File.Create(System.IO.Path.Combine(output,"snatch-slow-preview.gif")))gif.Save(file);
                foreach(var index in new[]{0,4,8,12,16,24,40,70,109}){var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(frames[index]));
                    using var file=System.IO.File.Create(System.IO.Path.Combine(output,$"snatch-{index:000}.png"));png.Save(file);}
            }
        }
    });

    [Theory][InlineData(0)][InlineData(1)][InlineData(2)]
    public void SnatchCanCancelOrTimeOutWithoutFalseBite(int mode){
        using var h=new Harness();h.Native.Scene=Scene(windows:false);h.Start();for(var i=0;i<240;i++)h.Tick();h.Loop.ToggleSnack();
        for(var attempt=1;attempt<=6;attempt++){
            for(var i=0;i<18;i++){h.Pointer=new(true,h.Position+new PointD(54,84));h.Tick();}
            h.Pointer=new(true,h.Position+new PointD(attempt==6?5000:154,-100));h.Tick();
            if(attempt<6)for(var i=0;i<160&&h.Snack.Phase==SnackPhase.Angry;i++)h.Tick();
        }
        Assert.Equal(SnackPhase.Snatching,h.Snack.Phase);
        if(mode==0)h.Loop.ToggleSnack();else if(mode==1)h.Pointer=PointerSample.Unavailable;
        for(var i=0;i<45;i++){h.Tick();Assert.NotEqual(SnackPhase.Eating,h.Snack.Phase);}
        Assert.Equal(mode==2?SnackPhase.Chasing:SnackPhase.None,h.Snack.Phase);
        Assert.Null(h.Fault);Assert.Equal(0,h.Captures);
    }
}

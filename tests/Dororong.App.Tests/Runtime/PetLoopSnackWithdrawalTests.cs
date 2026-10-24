using Dororong.Core.Behavior;
using Dororong.Core.Geometry;

namespace Dororong.App.Tests.Runtime;

public partial class PetLoopPlatformTests
{
    [Theory][InlineData(1,-1)][InlineData(1,1)][InlineData(1.5,-1)][InlineData(1.5,1)]
    public void ThirdStampCapturesActualLoopDisplayFrames(double scale,int side)=>Controls.CheekProductTests.Sta(()=>{
        using var h=new Harness(realPresenter:true,snackPresentation:true,highResolutionScale:scale);
        h.Native.Scene=Scene(windows:false);h.Start();for(var i=0;i<240;i++)h.Tick();
        var frames=new List<System.Windows.Media.Imaging.BitmapSource>();var states=new List<string>();
        var oracle=new Dororong.App.Controls.HighResolutionGrabRaster();
        var flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
        T Field<T>(string name)=>(T)typeof(Dororong.App.Controls.DororongPresenter).GetField(name,flags)!.GetValue(h.Presenter)!;
        for(var cycle=0;cycle<3;cycle++){
            if(!h.Loop.IsSnackActive)h.Loop.ToggleSnack();
            h.Pointer=new(true,h.Position+h.Presenter!.GetSnackMouth()+new PointD(side*150,0));h.Tick();
            for(var i=0;i<18;i++){h.Pointer=new(true,h.Position+h.Presenter!.GetSnackMouth());h.Tick();}
            h.Pointer=new(true,h.Position+h.Presenter!.GetSnackMouth()+new PointD(side*150,0));h.Tick();
            Assert.Equal(SnackPhase.Angry,h.Snack.Phase);
            for(var i=0;i<(int)Math.Ceiling(SnackAngerMotion.Duration(cycle+1)/.016);i++){
                if(cycle==2){
                    var image=(System.Windows.Media.Imaging.BitmapSource)h.Presenter.HighResolutionImage!.Source;
                    frames.Add(image.Clone());
                    states.Add($"{i},{h.Snack.Age:R},{h.Snack.Phase},{h.Presenter.HighResolutionDiagnostic},{h.Presenter.HighResolutionFace}");
                    var inverse=Field<System.Windows.Media.Matrix>("_hrForward");var recoil=Field<PointD>("_hrRecoil");
                    inverse.Translate(recoil.X/scale,recoil.Y/scale);inverse.Invert();
                    var expected=oracle.Render(Field<double>("_hrProgress"),inverse,Field<PointD>("_hrGrab"),Field<PointD>("_hrNear"),Field<PointD>("_hrFar"),
                        h.Presenter.LastRenderedSpread,scale,h.Presenter.HighResolutionFace,bodyScale:scale);
                    Assert.True(Dororong.App.Controls.PremultipliedFrame.From(expected).Pixels.AsSpan().SequenceEqual(
                        Dororong.App.Controls.PremultipliedFrame.From(image).Pixels),$"Display differs at frame{i}, size{scale}, side{side}");
                }
                h.Tick();
            }
            for(var i=0;i<4;i++)h.Tick();
        }
        Assert.Equal((int)Math.Ceiling(SnackAngerMotion.Duration(3)/.016),frames.Count);
        if(Environment.GetEnvironmentVariable("DORORONG_STOMP_PROOF") is not {} output)return;
        output=System.IO.Path.Combine(output,$"runtime-{scale}-{side}");
        System.IO.Directory.CreateDirectory(output);System.IO.File.WriteAllLines(System.IO.Path.Combine(output,"runtime-states.txt"),states);
        void Save(System.Windows.Media.DrawingVisual visual,int width,int height,string name){
            var image=new System.Windows.Media.Imaging.RenderTargetBitmap(width,height,96,96,System.Windows.Media.PixelFormats.Pbgra32);image.Render(visual);
            var png=new System.Windows.Media.Imaging.PngBitmapEncoder();png.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(image));
            using var file=System.IO.File.Create(System.IO.Path.Combine(output,name));png.Save(file);
        }
        for(var page=0;page<(int)Math.Ceiling(frames.Count/8d);page++){
            var sheet=new System.Windows.Media.DrawingVisual();using(var dc=sheet.RenderOpen()){
                dc.DrawRectangle(System.Windows.Media.Brushes.LightBlue,null,new System.Windows.Rect(0,0,1280,800));
                for(var cell=0;cell<8;cell++){
                    var i=page*8+cell;if(i>=frames.Count)break;var x=cell%4*320;var y=cell/4*400;
                    var mirrored=states[i].Contains("matrix=-1");
                    dc.DrawImage(new System.Windows.Media.Imaging.CroppedBitmap(frames[i],new System.Windows.Int32Rect((int)(80*scale),(int)(80*scale),(int)(96*scale),(int)(96*scale))),new System.Windows.Rect(x,y,192,192));
                    dc.DrawImage(new System.Windows.Media.Imaging.CroppedBitmap(frames[i],new System.Windows.Int32Rect((int)((mirrored?128:88)*scale),(int)(140*scale),(int)(40*scale),(int)(30*scale))),new System.Windows.Rect(x,y+192,280,210));
                    dc.DrawText(new System.Windows.Media.FormattedText($"{i:00} / {i*.016:F3}s",System.Globalization.CultureInfo.InvariantCulture,
                        System.Windows.FlowDirection.LeftToRight,new System.Windows.Media.Typeface("Arial"),16,System.Windows.Media.Brushes.Black,1),new System.Windows.Point(x+192,y+8));
                }
            }
            Save(sheet,1280,800,$"runtime-{page:00}.png");
        }
        var reference=Environment.GetEnvironmentVariable("DORORONG_STOMP_REFERENCE");
        if(reference is not null){
            var sheet=new System.Windows.Media.DrawingVisual();using(var dc=sheet.RenderOpen()){
                var input=new System.Windows.Media.Imaging.BitmapImage(new Uri(reference));
                dc.DrawImage(input,new System.Windows.Rect(0,0,input.PixelWidth*4,input.PixelHeight*4));
            }
            Save(sheet,812,556,"user-reference.png");
        }
    });
    [Theory][InlineData(1)][InlineData(1.25)][InlineData(1.5)]
    public void ActualSnackLoopWithdrawsRecoversAndCanFeedAgain(double scale)=>Controls.CheekProductTests.Sta(()=>{
        using var h=new Harness(realPresenter:true,snackPresentation:true,highResolutionScale:scale);
        h.Native.Scene=Scene(windows:false);h.Start();for(var i=0;i<240;i++)h.Tick();
        for(var cycle=0;cycle<3;cycle++){
            if(!h.Loop.IsSnackActive)h.Loop.ToggleSnack();
            for(var i=0;i<18;i++){h.Pointer=new(true,h.Position+h.Presenter!.GetSnackMouth());h.Tick();}
            Assert.Equal(SnackPhase.Chasing,h.Snack.Phase);
            h.Pointer=new(true,h.Position+h.Presenter!.GetSnackMouth()+new PointD(150,0));h.Tick();
            Assert.Equal(SnackPhase.Angry,h.Snack.Phase);Assert.True(h.Snack.CarriesJewel);
            Assert.True(h.Presenter.HighResolutionFace.Angry);var position=h.Position;
            Assert.False(h.Presenter.HighResolutionFace.Closed);
            Assert.Equal(cycle>=2,h.Presenter.HighResolutionFace.Furious);
            Assert.Equal(cycle+1,h.Snack.WithdrawalStreak);
            var maxLift=0d;var maxReach=0d;var maxCrouch=0d;
            for(var i=0;i<40;i++){
                h.Tick();Assert.Equal(SnackPhase.Angry,h.Snack.Phase);
                Assert.InRange(Math.Abs(h.Position.X-position.X),0,.01);Assert.InRange(Math.Abs(h.Position.Y-position.Y),0,.01);
                var face=h.Presenter.HighResolutionFace;
                maxLift=Math.Max(maxLift,face.StompLift);
                maxReach=Math.Max(maxReach,face.StompReach);maxCrouch=Math.Max(maxCrouch,face.StompCrouch);
                if(cycle>=1)Assert.Equal(Dororong.App.Controls.HighResolutionGrabRaster.StompWarp.Head(face.StompCrouch),face.Head);
                else Assert.Equal(0,face.StompLift);
            }
            if(cycle>=1){Assert.True(maxLift>(cycle==2?4.6:2.3));Assert.True(maxReach>(cycle==2?15.6:3.9));Assert.True(maxCrouch>.32);}
            for(var i=0;i<150&&h.Snack.Phase==SnackPhase.Angry;i++)h.Tick();Assert.Equal(SnackPhase.Chasing,h.Snack.Phase);Assert.False(h.Presenter.HighResolutionFace.Angry);
            Assert.Equal(0,h.Presenter.HighResolutionFace.StompLift);
        }
        for(var i=0;i<80&&h.Snack.Phase!=SnackPhase.Eating;i++){h.Pointer=new(true,h.Position+h.Presenter!.GetSnackMouth());h.Tick();}
        Assert.Equal(SnackPhase.Eating,h.Snack.Phase);Assert.False(h.Presenter!.HighResolutionFace.Angry);
        h.Pointer=new(true,new(700,20));h.Tick();Assert.Equal(SnackPhase.Eating,h.Snack.Phase);
        h.Loop.ToggleSnack();Assert.False(h.Loop.IsSnackActive);Assert.Null(h.Fault);Assert.Equal(0,h.Captures);
    });
}

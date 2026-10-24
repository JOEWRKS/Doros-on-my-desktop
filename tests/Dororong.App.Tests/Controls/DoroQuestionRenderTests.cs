using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;
using Dororong.App.Interaction;
using Dororong.Core.Behavior;

namespace Dororong.App.Tests.Controls;

public sealed class DoroQuestionRenderTests
{
    [Theory]
    [InlineData(1,false)][InlineData(1,true)][InlineData(1.25,false)]
    [InlineData(1.25,true)][InlineData(1.5,false)][InlineData(1.5,true)]
    public void Full_raise_hold_return_preserves_feet_and_reuses_one_pass_through_bubble(double scale,bool left)=>CheekProductTests.Sta(()=>{
        var p=Presenter(scale);var size=144*scale;
        var pet=new PetSnapshot(PetState.Idle,new(0,0),left?FacingDirection.Left:FacingDirection.Right,0,false,null);
        p.Render(pet,DirectInteractionSnapshot.None,TimeSpan.Zero);Layout(p,size);
        var sole=p.MeasurePlatformContact()!.Value.SoleY;
        var before=Capture(p,size);var beforePixels=Pixels(before);var children=((Canvas)p.Content).Children.Count;
        FrameworkElement? retained=null;var frames=new List<RenderTargetBitmap>();
        for(var i=0;i<=180;i++){
            var sample=pet with{DoroQuestionPhase=i/180d};p.Render(sample,DirectInteractionSnapshot.None,TimeSpan.Zero);Layout(p,size);
            var bubble=p.DoroBubble!;retained??=bubble;Assert.Same(retained,bubble);Assert.False(bubble.IsHitTestVisible);
            Assert.Equal(12*scale,Canvas.GetTop(bubble),6);
            Assert.Equal(children+1,((Canvas)p.Content).Children.Count);
            Assert.Equal(sole,p.MeasurePlatformContact()!.Value.SoleY,5);
            var bounds=bubble.TransformToAncestor(p).TransformBounds(new Rect(0,0,bubble.Width,bubble.Height));
            Assert.InRange(bounds.Left,0,size);Assert.InRange(bounds.Top,0,size);
            Assert.InRange(bounds.Right,0,size);Assert.InRange(bounds.Bottom,0,size);
            var image=Capture(p,size);var pixels=Pixels(image);
            // Lower body/feet cannot be modified by head-only tilt or speech.
            var row=(int)Math.Ceiling(104*scale)*image.PixelWidth*4;
            Assert.True(beforePixels.AsSpan(row).SequenceEqual(pixels.AsSpan(row)),$"Lower body changed frame{i}, scale{scale}, left{left}");
            if(i is 0 or 180)Assert.True(beforePixels.SequenceEqual(pixels),$"Endpoint differs frame{i}");
            frames.Add(image);
        }
        p.Render(pet,DirectInteractionSnapshot.None,TimeSpan.Zero);Assert.Equal(Visibility.Collapsed,p.DoroBubble!.Visibility);
        Assert.True(beforePixels.SequenceEqual(Pixels(Capture(p,size))));
        var output=Environment.GetEnvironmentVariable("DORORONG_DORO_PROOF");if(output is null)return;
        Directory.CreateDirectory(output);
        for(var page=0;page<6;page++){
            const int cell=216;var visual=new DrawingVisual();using(var dc=visual.RenderOpen()){
                dc.DrawRectangle(Brushes.LightBlue,null,new Rect(0,0,cell*6,cell*6));
                for(var n=0;n<36;n++){
                    var index=page*36+n;if(index>=frames.Count)break;var x=n%6*cell;var y=n/6*cell;
                    dc.DrawImage(frames[index],new Rect(x,y+16,198,198));
                    dc.DrawText(new FormattedText($"f{index:000} / {index/60d:F2}s",CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight,new Typeface("Arial"),12,Brushes.Black,1),new Point(x+3,y+2));
                }
            }Save(visual,cell*6,cell*6,Path.Combine(output,$"doro-{scale:F2}-{(left?"left":"right")}-{page:00}.png"));
        }
        var detail=new DrawingVisual();using(var dc=detail.RenderOpen()){
            dc.DrawRectangle(Brushes.WhiteSmoke,null,new Rect(0,0,900,300));
            var selected=new[]{0,90,180};for(var i=0;i<3;i++)dc.DrawImage(frames[selected[i]],new Rect(i*300,0,300,300));
        }Save(detail,900,300,Path.Combine(output,$"detail-{scale:F2}-{(left?"left":"right")}.png"));
    });

    [Theory][InlineData("snack")][InlineData("sit")][InlineData("blocked")][InlineData("press")]
    public void Conflicting_action_hides_question_immediately(string action)=>CheekProductTests.Sta(()=>{
        var p=Presenter(1);var pet=new PetSnapshot(PetState.Idle,default,FacingDirection.Right,0,false,null){DoroQuestionPhase=.5};
        p.Render(pet,DirectInteractionSnapshot.None,TimeSpan.Zero);Assert.Equal(Visibility.Visible,p.DoroBubble!.Visibility);
        if(action=="snack")p.SetSnack(new SnackPose{Phase=SnackPhase.Chasing},PointerSample.Unavailable);
        else if(action=="sit")p.SetSittingRequested(true);
        else if(action=="blocked")p.SetLocomotionBlocked(true);
        else pet=pet with{IsDirectInteractionPending=true};
        p.Render(pet,DirectInteractionSnapshot.None,TimeSpan.Zero);
        Assert.Equal(Visibility.Collapsed,p.DoroBubble.Visibility);
    });

    private static DororongPresenter Presenter(double scale){
        var p=new DororongPresenter{Width=144*scale,Height=144*scale,HighResolutionGrabEnabled=true,HighResolutionScale=scale};
        ((Canvas)p.Content).Width=p.Width;((Canvas)p.Content).Height=p.Height;return p;
    }
    private static void Layout(FrameworkElement e,double size){e.Measure(new(size,size));e.Arrange(new(0,0,size,size));e.UpdateLayout();}
    private static RenderTargetBitmap Capture(Visual p,double size){var b=new RenderTargetBitmap((int)Math.Ceiling(size),(int)Math.Ceiling(size),96,96,PixelFormats.Pbgra32);b.Render(p);return b;}
    private static byte[] Pixels(BitmapSource b){var result=new byte[b.PixelWidth*b.PixelHeight*4];b.CopyPixels(result,b.PixelWidth*4,0);return result;}
    private static void Save(Visual v,int width,int height,string path){var b=new RenderTargetBitmap(width,height,96,96,PixelFormats.Pbgra32);b.Render(v);var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(b));using var f=File.Create(path);png.Save(f);}
}

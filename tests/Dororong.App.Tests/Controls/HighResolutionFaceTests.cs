using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;
using Dororong.App.Interaction;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;

namespace Dororong.App.Tests.Controls;

public sealed class HighResolutionFaceTests
{
    [Theory]
    [InlineData(1)] [InlineData(1.1)] [InlineData(1.25)] [InlineData(1.35)] [InlineData(1.5)]
    public void Hover_tracking_cheek_press_pull_release_keep_source_scale(double scale)=>CheekProductTests.Sta(()=>
    {
        var output=Environment.GetEnvironmentVariable("DORORONG_R53_OUTPUT");if(output is not null)Directory.CreateDirectory(output);
        var rows=new List<object>();var number=0;
        foreach(var facing in new[]{FacingDirection.Right,FacingDirection.Left}){
            var p=new DororongPresenter{HighResolutionGrabEnabled=true,HighResolutionScale=scale,Width=144*scale,Height=144*scale};
            ((Canvas)p.Content).Width=p.Width;((Canvas)p.Content).Height=p.Height;
            var pet=new PetSnapshot(PetState.Idle,new(100,100),facing,0,false,null);
            void Layout(){p.Measure(new(p.Width,p.Height));p.Arrange(new(0,0,p.Width,p.Height));p.UpdateLayout();}
            byte[] Pixels(){var b=(BitmapSource)p.HighResolutionImage!.Source;var a=new byte[b.PixelWidth*b.PixelHeight*4];b.CopyPixels(a,b.PixelWidth*4,0);return a;}
            void Save(string phase){
                Assert.NotNull(p.HighResolutionImage);var im=p.HighResolutionImage!;var source=(BitmapSource)im.Source;
                Assert.Equal(256*scale,im.Width,6);Assert.Equal((int)Math.Ceiling(256*scale),source.PixelWidth);
                var pixels=Pixels();for(var y=0;y<source.PixelHeight;y++)for(var x=0;x<source.PixelWidth;x++)if(pixels[(y*source.PixelWidth+x)*4+3]>0){Assert.InRange(x,1,source.PixelWidth-2);Assert.InRange(y,1,source.PixelHeight-2);}
                var file=$"face-{scale:F2}-{number++:D3}-{facing}-{phase}.png";
                if(output is not null){using var stream=File.Create(Path.Combine(output,file));var enc=new PngBitmapEncoder();enc.Frames.Add(BitmapFrame.Create(source));enc.Save(stream);}
                rows.Add(new{file,phase,scale,facing=facing.ToString()});
            }
            p.Render(pet,DirectInteractionSnapshot.None,TimeSpan.Zero);Layout();Save("idle");
            foreach(var state in new[]{PetState.Curious,PetState.Startled,PetState.Idle}){
                p.Render(pet with {State=state},DirectInteractionSnapshot.None,TimeSpan.FromMilliseconds(16));Layout();Save(state.ToString());
            }
            // Actual tracking state update, not a hand-written head angle.
            for(var i=0;i<12;i++){
                p.UpdateHuntingWithPounce(pet,DirectInteractionSnapshot.None,new(true,pet.Position+new PointD((i<6?220:12)*scale,(i<6?25:105)*scale)),TimeSpan.FromMilliseconds(40),false);
                p.Render(pet,DirectInteractionSnapshot.None,TimeSpan.FromMilliseconds(40));Layout();Save("track");
            }
            var point=p.HighResolutionFaceToPresenter(new(16,58));var image=p.HighResolutionImage!;
            var press=p.CreateDirectPress(image,p.TranslatePoint(point,image));Assert.NotNull(press);Assert.Equal(DirectInteractionTarget.RightCheek,press!.Target);Assert.NotNull(press.CheekCapture?.HighResolution);
            var controller=new DirectInteractionController();var origin=pet.Position+press.WindowLocalPosition;controller.BeginCheekPull(press.CheekCapture!,origin);
            var before=Pixels();p.Render(pet,controller.Current,TimeSpan.Zero);Layout();Assert.Equal(before,Pixels());Save("press");
            for(var i=1;i<=5;i++){
                var unit=press.CheekCapture!.OutwardUnit;var delta=new PointD(unit.X*i*4,unit.Y*i*4);
                var direct=controller.Advance(TimeSpan.FromMilliseconds(40),new(true,origin+delta),true,PetState.Idle,PetState.Idle);
                p.Render(pet,direct,TimeSpan.FromMilliseconds(40));Layout();Save("pull");
            }
            Assert.Equal(20,controller.Current.CheekPull!.PullDips,6);Assert.NotEqual(before,Pixels());
            var held=Pixels();var release=controller.Advance(TimeSpan.Zero,new(true,origin),false,PetState.Idle,PetState.Idle);p.Render(pet,release,TimeSpan.Zero);Layout();Assert.Equal(held,Pixels());Save("release-start");
            for(var i=0;i<12;i++){var direct=controller.Advance(TimeSpan.FromMilliseconds(40),new(true,origin),false,PetState.Idle,PetState.Idle);p.Render(pet,direct,TimeSpan.FromMilliseconds(40));Layout();Save("release");}
            Assert.Equal(DirectInteractionTarget.None,controller.Current.Target);
            for(var i=0;i<8;i++){p.Render(pet,DirectInteractionSnapshot.None,TimeSpan.FromMilliseconds(40));Layout();Save("return");}Save("returned");
        }
        if(output is not null)File.WriteAllText(Path.Combine(output,$"face-{scale:F2}.json"),JsonSerializer.Serialize(rows));
    });
}

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

public sealed class IdleActivityRenderTests
{
    [Theory]
    [InlineData(1,false)][InlineData(1,true)][InlineData(1.25,false)]
    [InlineData(1.25,true)][InlineData(1.5,false)][InlineData(1.5,true)]
    public void Three_motions_full_sequence_fits_and_returns_to_baseline(double scale,bool left)=>CheekProductTests.Sta(()=>{
        var p=new DororongPresenter{Width=144*scale,Height=144*scale,HighResolutionGrabEnabled=true,HighResolutionScale=scale};
        ((Canvas)p.Content).Width=p.Width;((Canvas)p.Content).Height=p.Height;
        var pet=new PetSnapshot(PetState.Idle,default,left?FacingDirection.Left:FacingDirection.Right,0,false,null);
        var size=(int)Math.Ceiling(144*scale);
        void Layout(){p.Measure(new(size,size));p.Arrange(new(0,0,size,size));p.UpdateLayout();}
        RenderTargetBitmap Capture(){Layout();var image=new RenderTargetBitmap(size,size,96,96,PixelFormats.Pbgra32);image.Render(p);return image;}
        p.Render(pet,DirectInteractionSnapshot.None,TimeSpan.Zero);var baseline=Pixels(Capture());
        var sole=p.MeasurePlatformContact()!.Value.SoleY;
        var output=Environment.GetEnvironmentVariable("DORORONG_IDLE_PROOF");
        foreach(var kind in new[]{IdleActivityKind.Stretch,IdleActivityKind.Beg,IdleActivityKind.Groom}){
            var frames=new List<RenderTargetBitmap>();var count=(int)Math.Round(new IdleActivity(kind,0).Duration*60);
            for(var i=0;i<=count;i++){
                var sample=pet with{IdleActivity=new(kind,i/60d)};
                p.UpdateHuntingWithPounce(sample,DirectInteractionSnapshot.None,new(true,new(150,30)),TimeSpan.FromSeconds(1d/60),false);
                p.Render(sample,DirectInteractionSnapshot.None,TimeSpan.Zero);var image=Capture();var pixels=Pixels(image);
                Assert.Contains(pixels,v=>v!=0);
                for(var y=0;y<size;y++)for(var x=0;x<size;x++)if(x<2||y<2||x>=size-2||y>=size-2)
                    Assert.Equal(0,pixels[(y*size+x)*4+3]);
                // Raising the deepest front paw changes the remaining raster
                // sole. World-space anchoring is checked with the real host.
                if(kind!=IdleActivityKind.Groom)Assert.InRange(p.MeasurePlatformContact()!.Value.SoleY,sole-4*scale,sole+scale);
                if(i==0||i==count)Assert.True(baseline.SequenceEqual(pixels),$"{kind} endpoint{i} scale{scale} left{left}");
                frames.Add(image);
            }
            p.Render(pet,DirectInteractionSnapshot.None,TimeSpan.Zero);
            Assert.True(baseline.SequenceEqual(Pixels(Capture())));
            if(output is null)continue;
            Directory.CreateDirectory(output);
            for(var page=0;page*36<frames.Count;page++){
                const int cell=216;var visual=new DrawingVisual();using(var dc=visual.RenderOpen()){
                    dc.DrawRectangle(Brushes.LightBlue,null,new Rect(0,0,cell*6,cell*6));
                    for(var n=0;n<36;n++){
                        var i=page*36+n;if(i>=frames.Count)break;var x=n%6*cell;var y=n/6*cell;
                        dc.DrawImage(frames[i],new Rect(x,y+16,198,198));
                        dc.DrawText(new FormattedText($"{kind} f{i:000} {i/60d:F2}s",CultureInfo.InvariantCulture,FlowDirection.LeftToRight,new Typeface("Arial"),12,Brushes.Black,1),new Point(x+3,y+2));
                    }
                }Save(visual,cell*6,cell*6,Path.Combine(output,$"{kind}-{scale:F2}-{(left?"left":"right")}-{page:00}.png"));
            }
        }
    });
    private static byte[] Pixels(BitmapSource b){var bytes=new byte[b.PixelWidth*b.PixelHeight*4];b.CopyPixels(bytes,b.PixelWidth*4,0);return bytes;}
    private static void Save(Visual v,int width,int height,string path){var b=new RenderTargetBitmap(width,height,96,96,PixelFormats.Pbgra32);b.Render(v);var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(b));using var f=File.Create(path);png.Save(f);}
}

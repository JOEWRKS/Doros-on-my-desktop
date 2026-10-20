using System.Windows;
using Dororong.App.Controls;

namespace Dororong.App.Tests.Controls;

public sealed class HighResolutionContourTests
{
    [Fact]
    public void Display_density_is_separate_from_requested_body_size()=>CheekProductTests.Sta(()=>{
        var raster=new HighResolutionGrabRaster();
        byte[] Draw(double size){
            var image=raster.Render(0,System.Windows.Media.Matrix.Identity,new(37.5,28.25),default,default,0,1.5,bodyScale:size);
            var pixels=new byte[image.PixelWidth*image.PixelHeight*4];image.CopyPixels(pixels,image.PixelWidth*4,0);return pixels;
        }
        var atOne=Draw(1);var atLarge=Draw(1.5);
        // Same destination density and unchanged head; only body ink differs.
        Assert.Equal(atOne.AsSpan(0,190*384*4).ToArray(),atLarge.AsSpan(0,190*384*4).ToArray());
        Assert.NotEqual(atOne,atLarge);Assert.Equal(atOne,Draw(1));
    });
    [Fact]
    public void Rebound_samples_only_selected_skin_and_does_not_move_outer_ink(){
        foreach(var pull in new[]{-.1,-.5,-1,-2,-4}){
            var pose=HighResolutionFacePose.Neutral with{Pull=pull};
            for(var y=53d;y<64;y+=.125){
                var collar=HighResolutionCheekSelection.Edge(y)+1.5;
                for(var x=2d;x<35;x+=.125){
                    var q=new Point(x,y);var p=pose.Source(q);
                    if(x<collar)Assert.Equal(q,p);
                    else Assert.True(p.X>=collar-1e-8,$"{pull}: {q} -> {p}");
                }
            }
        }
    }
    [Theory]
    [InlineData(1)] [InlineData(1.1)] [InlineData(1.25)] [InlineData(1.35)] [InlineData(1.5)]
    public void Body_compensation_is_immutable_and_keeps_physical_ink_near_one_x(double scale){
        const int size=225;const double density=225d/96,width=1.295415959252971;
        var source=new float[size*size*4];
        for(var y=0;y<size;y++)for(var x=0;x<size;x++){
            // Two antialiased edges, with the right edge as white body fill.
            var left=80d;var right=left+width*density;
            var alpha=Math.Clamp(x+.5-left,0,1);var ink=Math.Clamp(right-(x-.5),0,1);
            var at=(y*size+x)*4;source[at+3]=(float)(255*alpha);
            for(var c=0;c<3;c++)source[at+c]=(float)((255-(255-58)*ink)*alpha);
        }
        var original=(float[])source.Clone();var result=HighResolutionBodyInk.AtScale(source,size,scale);
        Assert.Equal(original,source);if(scale==1)Assert.Same(source,result);
        double Measure(float[] data){double value=0;for(var x=70;x<100;x++){var at=(100*size+x)*4;value+=(data[at+3]-data[at])/(255-58);}return value/density;}
        Assert.InRange(Measure(result)*scale,Measure(source)*.8,Measure(source)*1.2);
        Assert.Same(result,HighResolutionBodyInk.AtScale(source,size,scale));
    }
}

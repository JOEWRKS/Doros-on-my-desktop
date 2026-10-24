using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;

namespace Dororong.App.Tests.Controls;

public class SnackChuckleRenderTests
{
    [Fact]
    public void OuterPawIsAnEllipticalSweepNotASmoothedRectangle(){
        var after=HighResolutionGrabRaster.ChucklePawControlPoints;
        Assert.Equal(22,after.Length);
        Assert.Equal(new Point(35,76),after[0]);
        Assert.Equal(new Point(21,79.8),after[3]);
        Assert.Equal(new Point(24.125,70.41666666666667),after[18]);
        Assert.Equal(new Point(35,70),after[21]);
        for(var i=3;i<=15;i+=3){
            var incoming=after[i]-after[i-1];var outgoing=after[i+1]-after[i];
            Assert.True(Math.Abs(Vector.CrossProduct(incoming,outgoing))<1e-9);
            Assert.True(Vector.Multiply(incoming,outgoing)>0);
        }
        static Point At(ReadOnlySpan<Point> p,int i,double t){
            var s=1-t;
            return new(s*s*s*p[i].X+3*s*s*t*p[i+1].X+3*s*t*t*p[i+2].X+t*t*t*p[i+3].X,
                s*s*s*p[i].Y+3*s*s*t*p[i+1].Y+3*s*t*t*p[i+2].Y+t*t*t*p[i+3].Y);
        }
        // Check the whole outer sweep, not only tangent direction at joins.
        // Cubic circular arcs approximate the ellipse within 0.03% radially.
        for(var segment=1;segment<=4;segment++)for(var step=0;step<=64;step++){
            var p=At(after,segment*3,step/64d);
            var x=(p.X-21)/(segment==4?4.583333333333333:9);
            var y=(p.Y-(segment==4?62:68.7))/(segment==4?4.4:11.1);
            Assert.InRange(Math.Sqrt(x*x+y*y),.9997,1.0003);
            Assert.InRange(p.X,11.99999,25.58334);
            Assert.InRange(p.Y,57.59999,79.80001);
        }
    }
    [Theory][InlineData(1)][InlineData(1.125)][InlineData(1.25)][InlineData(1.375)][InlineData(1.5)]
    public void LoweringRetracesTheSameConnectedRoundedPaw(double scale)=>CheekProductTests.Sta(()=>{
        var raster=new HighResolutionGrabRaster();
        byte[] Draw(double age)=>PremultipliedFrame.From(raster.Render(0,Matrix.Identity,new(37.5,28.25),default,default,0,scale,
            HighResolutionGrabRaster.SnackChuckle(age),bodyScale:scale)).Pixels;
        for(var step=0;step<=16;step++){
            var u=step/16d;var up=Draw(.26*u);var down=Draw(1.48+.32*(1-u));
            for(var i=0;i<up.Length;i++)Assert.True(Math.Abs(up[i]-down[i])<=1,$"Retrace step{step} byte{i} up{up[i]} down{down[i]} scale{scale}");
        }
    });
    [Theory][InlineData(1)][InlineData(1.125)][InlineData(1.25)][InlineData(1.375)][InlineData(1.5)]
    public void UprightRedTraceHasNarrowTipVerticalForearmAndClearEyes(double scale)=>CheekProductTests.Sta(()=>{
        var raster=new HighResolutionGrabRaster();var extent=(int)Math.Ceiling(256*scale);
        foreach(var age in new[]{.27,.41,.58,.69,1d,1.27,1.45}){
            var face=HighResolutionGrabRaster.SnackChuckle(age);
            byte[] Draw(HighResolutionFacePose p)=>PremultipliedFrame.From(raster.Render(0,Matrix.Identity,new(37.5,28.25),default,default,0,scale,p,bodyScale:scale)).Pixels;
            var neutral=Draw(face with {ChuckleCover=0,ChuckleBob=0});var result=Draw(face);
            for(var y=(int)Math.Ceiling(125*scale);y<(int)(136*scale);y++)for(var x=(int)Math.Ceiling(94*scale);x<(int)(128*scale);x++){
                var at=(y*extent+x)*4;Assert.True(neutral.AsSpan(at,4).SequenceEqual(result.AsSpan(at,4)),"Paw crosses eyes");
            }
            for(var y=(int)Math.Ceiling(125*scale);y<(int)(144*scale);y++)for(var x=(int)Math.Ceiling(94*scale);x<(int)(126*scale);x++){
                var at=(y*extent+x)*4;var blue=neutral[at];var green=neutral[at+1];var red=neutral[at+2];
                if(blue>green+12&&red>green+12&&Math.Abs(red-blue)<40)
                    Assert.True(neutral.AsSpan(at,4).SequenceEqual(result.AsSpan(at,4)),"Paw covers lower iris");
            }
            // Latest 516x401 red trace supersedes the rejected sideways C:
            // narrow upright tip, full vertical arm, no long lateral extension.
            for(var y=(int)Math.Ceiling((140+face.ChuckleBob)*scale);y<(int)((141.5+face.ChuckleBob)*scale);y++)
            for(var x=(int)Math.Ceiling(100*scale);x<(int)(104*scale);x++){
                var at=(y*extent+x)*4;
                Assert.True(result[at]>=225&&result[at+1]>=225&&result[at+2]>=225&&result[at+3]>=250,"Upright tip missing");
            }
            for(var y=(int)Math.Ceiling((146+face.ChuckleBob)*scale);y<(int)((155+face.ChuckleBob)*scale);y++)
            for(var x=(int)Math.Ceiling(96*scale);x<(int)(101*scale);x++){
                var at=(y*extent+x)*4;
                Assert.True(result[at]>=225&&result[at+1]>=225&&result[at+2]>=225&&result[at+3]>=250,"Vertical forearm missing");
            }
            for(var y=(int)Math.Ceiling(146*scale);y<(int)(160*scale);y++)
            for(var x=(int)Math.Ceiling(85*scale);x<(int)(90*scale);x++){
                var at=(y*extent+x)*4;
                Assert.Equal(neutral[at+3],result[at+3]);
            }
            for(var y=(int)Math.Ceiling((146+face.ChuckleBob)*scale);y<(int)((151+face.ChuckleBob)*scale);y++)
            for(var x=(int)Math.Ceiling(104*scale);x<(int)(110*scale);x++)
                Assert.True(result[(y*extent+x)*4+3]>=250,$"Inner forearm/chest pinhole age{age} x{x} y{y} alpha{result[(y*extent+x)*4+3]}");
        }
    });
    [Theory][InlineData(1)][InlineData(1.125)][InlineData(1.25)][InlineData(1.375)][InlineData(1.5)]
    public void FrameSequenceReturnsExactlyAndKeepsPlantedFeet(double scale)=>CheekProductTests.Sta(()=>{
        var raster=new HighResolutionGrabRaster();var extent=(int)Math.Ceiling(256*scale);
        BitmapSource Draw(double age)=>raster.Render(0,Matrix.Identity,new(37.5,28.25),default,default,0,scale,
            HighResolutionGrabRaster.SnackChuckle(age),bodyScale:scale);
        var baseline=PremultipliedFrame.From(Draw(0)).Pixels;
        Assert.Equal(baseline,PremultipliedFrame.From(Draw(1.8)).Pixels);
        var output=Environment.GetEnvironmentVariable("DORORONG_CHUCKLE_PROOF");
        var frames=new List<BitmapSource>();var failures=new List<string>();
        for(var frame=0;frame<=108;frame++){
            var image=Draw(frame/60d);var pixels=PremultipliedFrame.From(image).Pixels;
            for(var y=(int)Math.Ceiling(149*scale);y<(int)(152*scale);y++)for(var x=(int)Math.Ceiling(110*scale);x<(int)(114*scale);x++)
                if(pixels[(y*extent+x)*4+3]<250&&failures.Count<10)failures.Add($"Shoulder hole size{scale} frame{frame} at{x},{y}");
            // All feet other than the explicitly raised near forepaw stay put.
            for(var y=(int)Math.Ceiling(157*scale);y<extent;y++)for(var x=(int)Math.Ceiling(114*scale);x<extent;x++){
                var at=(y*extent+x)*4;
                if(!pixels.AsSpan(at,4).SequenceEqual(baseline.AsSpan(at,4))&&failures.Count<10)failures.Add($"Foot moved size{scale} frame{frame}");
            }
            if(output is not null)frames.Add(image);
        }
        if(output is null){Assert.True(failures.Count==0,string.Join("; ",failures));return;}
        Directory.CreateDirectory(output);
        for(var page=0;page<14;page++){
            var visual=new DrawingVisual();using(var dc=visual.RenderOpen()){
                dc.DrawRectangle(Brushes.LightBlue,null,new Rect(0,0,1152,576));
                for(var cell=0;cell<8;cell++){
                    var frame=page*8+cell;if(frame>=frames.Count)break;
                    var x=cell%4*288;var y=cell/4*288;
                    var offset=(int)Math.Round(80*scale);var size=(int)Math.Round(96*scale);
                    dc.DrawImage(new CroppedBitmap(frames[frame],new Int32Rect(offset,offset,size,size)),new Rect(x,y,288,288));
                    dc.DrawText(new FormattedText($"{scale*100:F1}% {frame/60d:F3}s",System.Globalization.CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight,new Typeface("Arial"),16,Brushes.Black,1),new Point(x+6,y+6));
                }
            }
            var target=new RenderTargetBitmap(1152,576,96,96,PixelFormats.Pbgra32);target.Render(visual);
            var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(target));
            using var file=File.Create(Path.Combine(output,$"chuckle-{scale:F3}-{page:00}.png"));png.Save(file);
        }
        var beforeFolder=Environment.GetEnvironmentVariable("DORORONG_CHUCKLE_BEFORE");
        if(scale==1.5&&beforeFolder is not null){
            var selected=new[]{3,32,105};var proof=new DrawingVisual();
            using(var dc=proof.RenderOpen()){
                dc.DrawRectangle(Brushes.LightBlue,null,new Rect(0,0,864,576));
                for(var col=0;col<selected.Length;col++){
                    var frame=selected[col];var page=frame/8;var cell=frame%8;
                    var before=new BitmapImage(new Uri(Path.Combine(beforeFolder,$"chuckle-1.500-{page:00}.png")));
                    var cut=new CroppedBitmap(before,new Int32Rect(cell%4*288+30,cell/4*288+165,111,90));
                    var after=new CroppedBitmap(frames[frame],new Int32Rect(135,203,56,45));
                    dc.DrawImage(cut,new Rect(col*288+10,40,268,217));
                    dc.DrawImage(after,new Rect(col*288+10,328,268,217));
                    foreach(var row in new[]{0,1})dc.DrawText(new FormattedText($"{(row==0?"BEFORE":"AFTER")} f{frame} {frame/60d:F3}s",
                        System.Globalization.CultureInfo.InvariantCulture,FlowDirection.LeftToRight,new Typeface("Arial"),17,Brushes.Black,1),new Point(col*288+10,row*288+10));
                }
            }
            var comparison=new RenderTargetBitmap(864,576,96,96,PixelFormats.Pbgra32);comparison.Render(proof);
            var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(comparison));
            using var comparisonFile=File.Create(Path.Combine(output,"joint-before-after.png"));encoder.Save(comparisonFile);
            // The user's rejection shows the opposite facing direction. Keep
            // that presentation in the visual evidence as well as the source pose.
            var facing=new DrawingVisual();
            using(var dc=facing.RenderOpen()){
                dc.DrawRectangle(Brushes.LightBlue,null,new Rect(0,0,864,576));
                var facingFrames=new[]{8,32,96};
                for(var col=0;col<3;col++){
                    var frame=facingFrames[col];var page=frame/8;var cell=frame%8;
                    var old=new BitmapImage(new Uri(Path.Combine(beforeFolder,$"chuckle-1.500-{page:00}.png")));
                    var oldCrop=new CroppedBitmap(old,new Int32Rect(cell%4*288,cell/4*288+30,288,258));
                    var current=new CroppedBitmap(frames[frame],new Int32Rect(120,135,144,129));
                    for(var row=0;row<2;row++){
                        dc.PushTransform(new ScaleTransform(-1,1,col*288+144,row*288+144));
                        dc.DrawImage(row==0?oldCrop:current,new Rect(col*288,row*288+30,288,258));
                        dc.Pop();
                        dc.DrawText(new FormattedText($"{(row==0?"BEFORE":"AFTER")} f{frame}",
                            System.Globalization.CultureInfo.InvariantCulture,FlowDirection.LeftToRight,new Typeface("Arial"),17,Brushes.Black,1),new Point(col*288+8,row*288+6));
                    }
                }
            }
            var facingTarget=new RenderTargetBitmap(864,576,96,96,PixelFormats.Pbgra32);facingTarget.Render(facing);
            var facingPng=new PngBitmapEncoder();facingPng.Frames.Add(BitmapFrame.Create(facingTarget));
            using var facingFile=File.Create(Path.Combine(output,"right-facing-before-after.png"));facingPng.Save(facingFile);
        }
        Assert.True(failures.Count==0,string.Join("; ",failures));
    });
}

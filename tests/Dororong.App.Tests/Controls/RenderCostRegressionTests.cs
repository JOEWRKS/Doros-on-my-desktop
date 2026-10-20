using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;
using Dororong.Core.Geometry;

namespace Dororong.App.Tests.Controls;

public sealed class RenderCostRegressionTests
{
    // Exported before changing the sampler, then compared to that independent
    // baseline after the optimization. No screenshot is inferred from a hash.
    [Fact]
    public void Profile_and_compare_accepted_render_samples()=>CheekProductTests.Sta(()=>{
        var output=Environment.GetEnvironmentVariable("DORORONG_COST_OUTPUT");
        if(output is null)return;
        Directory.CreateDirectory(output);
        var baseline=Environment.GetEnvironmentVariable("DORORONG_COST_BASELINE");
        using var expectedRows=baseline is null?null:JsonDocument.Parse(File.ReadAllText(Path.Combine(baseline,"costs.json")));
        var expectedHashes=expectedRows?.RootElement.EnumerateArray().ToDictionary(r=>r.GetProperty("id").GetString()!,r=>r.GetProperty("hash").GetString());
        var rows=new List<object>();
        foreach(var scale in new[]{1d,1.1,1.25,1.35,1.5}){
            var raster=new HighResolutionGrabRaster();
            var surface=new HighResolutionDisplaySurface();byte[]? livePixels=null;
            foreach(var kind in new[]{"grab","release","landing","tilted-landing","track","cheek","crouch"})for(var i=0;i<12;i++){
                var face=HighResolutionFacePose.Neutral;var inverse=Matrix.Identity;
                var progress=0d;var near=default(PointD);var far=default(PointD);var spread=0d;
                if(kind=="grab") {progress=1;inverse.RotateAt((i-6)*5,43,32);near=new(1.2,-1.4);far=new(-.7,.8);}
                if(kind=="release") {progress=1-i/11d;inverse.RotateAt((11-i)*1.5,43,32);near=new((11-i)*.1,-(11-i)*.08);}
                if(kind.Contains("landing")) {spread=1-i/12d;inverse.ScaleAt(.9,1.2,48,87);if(kind=="tilted-landing"){var h=Matrix.Identity;h.RotateAt(7,43,32);face=face with{Head=h};}}
                if(kind=="track") {face=face with{EyeX=(i-6)*.12,EyeY=(i-6)*.08};}
                if(kind=="cheek") {face=face with{Pull=20*Math.Sin(i*Math.PI/11),EyeFollow=10,HairFollow=10,EyeX=.4,EyeY=.3};}
                if(kind=="crouch") {var poseFrame=60+i*6;face=face with{BodyFrame=poseFrame,Head=HuntRenderer.HeadTransform(poseFrame,new(.4,.2,(i-6)*.015))};}
                var allocatedBefore=GC.GetTotalAllocatedBytes(true);
                var timer=Stopwatch.StartNew();
                var frame=raster.Render(progress,inverse,new(37.5,28.25),near,far,spread,scale,face);
                timer.Stop();var cost=raster.LastCost;var allocatedRender=GC.GetTotalAllocatedBytes(true)-allocatedBefore;
                var id=$"{scale:0.00}-{kind}-{i:00}";
                var pixels=new byte[frame.PixelWidth*frame.PixelHeight*4];frame.CopyPixels(pixels,frame.PixelWidth*4,0);
                var hash=Convert.ToHexString(SHA256.HashData(pixels));
                // Compare raw premultiplied bytes, not PNG's unpremultiplied
                // round trip (which can quantize translucent edge colours).
                if(expectedHashes is not null)Assert.True(expectedHashes[id]==hash,id+" pixels differ");
                livePixels=raster.RenderRetainedPixels(progress,inverse,new(37.5,28.25),near,far,spread,scale,face,scale,livePixels);
                var live=surface.Update(livePixels,scale);var displayed=new byte[pixels.Length];live.CopyPixels(displayed,frame.PixelWidth*4,0);
                Assert.True(pixels.SequenceEqual(displayed),id+" live surface differs");
                using(var stream=File.Create(Path.Combine(output,id+".png"))){var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(frame));encoder.Save(stream);}
                // Sparse geometry must remain byte-equivalent on the measured
                // occupied bottom band, including non-integer scales.
                var sparse=raster.Render(progress,inverse,new(37.5,28.25),near,far,spread,scale,face,true,2*scale);
                var retained=new byte[pixels.Length];frame.CopyPixels(retained,frame.PixelWidth*4,0);
                Assert.True(pixels.SequenceEqual(retained),id+" frozen frame changed after scratch reuse");
                var sparsePixels=new byte[pixels.Length];sparse.CopyPixels(sparsePixels,frame.PixelWidth*4,0);
                var bottom=-1;for(var y=frame.PixelHeight-1;y>=0&&bottom<0;y--)for(var x=0;x<frame.PixelWidth;x++)if(pixels[(y*frame.PixelWidth+x)*4+3]>=128){bottom=y;break;}
                if(bottom>=0)for(var y=Math.Max(0,(int)Math.Ceiling(bottom+1-2*scale));y<=bottom;y++)
                    Assert.True(pixels.AsSpan(y*frame.PixelWidth*4,frame.PixelWidth*4).SequenceEqual(sparsePixels.AsSpan(y*frame.PixelWidth*4,frame.PixelWidth*4)),id+" sole differs");
                rows.Add(new{id,scale,kind,index=i,allocatedRender,ms=timer.Elapsed.TotalMilliseconds,prepareMs=cost.Preparation,sampleMs=cost.Sampling,bitmapMs=cost.Bitmap,
                    solePrepareMs=raster.LastCost.Preparation,soleSampleMs=raster.LastCost.Sampling,hash});
            }
        }
        File.WriteAllText(Path.Combine(output,"costs.json"),JsonSerializer.Serialize(rows,new JsonSerializerOptions{WriteIndented=true}));
    });
}

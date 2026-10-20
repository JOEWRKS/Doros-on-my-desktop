using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;
using Dororong.Core.Geometry;
using Xunit.Abstractions;

namespace Dororong.App.Tests.Controls;

public sealed class SnackEffectReuseTests(ITestOutputHelper output)
{
    private static Func<double,double,float[]> OwnedRenderer(){
        var type=typeof(HighResolutionSnackEffects).GetNestedType("Buffer",BindingFlags.NonPublic);
        if(type is null)return HighResolutionSnackEffects.Render;
        var buffer=Activator.CreateInstance(type,true)!;
        return type.GetMethod("Get",BindingFlags.Instance|BindingFlags.NonPublic)!.CreateDelegate<Func<double,double,float[]>>(buffer);
    }
    [Fact]
    public void Warm_effect_sequence_reuses_pixels_without_large_array_allocation()=>CheekProductTests.Sta(()=>{
        var render=OwnedRenderer();render(-1,0);
        var before=GC.GetAllocatedBytesForCurrentThread();
        for(var i=0;i<40;i++)render(-1,i/60d);
        var allocated=GC.GetAllocatedBytesForCurrentThread()-before;
        output.WriteLine($"40 drool frames allocated {allocated} bytes.");
        Assert.True(allocated<2_000_000,$"40 frames allocated {allocated} bytes.");
    });
    [Fact]
    public void Owned_effects_match_fresh_render_and_clear_previous_content()=>CheekProductTests.Sta(()=>{
        var render=OwnedRenderer();
        foreach(var pair in new[]{(-1d,0d),(-1d,.73),(.1,-1d),(.35,-1d),(.49,-1d),(-1d,-1d),(-1d,1.2),(.0,-1d)}){
            var actual=render(pair.Item1,pair.Item2);
            Assert.Equal(HighResolutionSnackEffects.Render(pair.Item1,pair.Item2),actual);
            Assert.Equal(RasterAlphaBounds.Get(HighResolutionSnackEffects.Render(pair.Item1,pair.Item2),225),RasterAlphaBounds.Get(actual,225));
        }
    });
    [Fact]
    public void Export_effect_pixels_against_previous_product()=>CheekProductTests.Sta(()=>{
        var path=Environment.GetEnvironmentVariable("DORORONG_EFFECT_OUTPUT");if(path is null)return;
        Directory.CreateDirectory(path);
        var baseline=Environment.GetEnvironmentVariable("DORORONG_EFFECT_BASELINE");
        using var expected=baseline is null?null:JsonDocument.Parse(File.ReadAllText(Path.Combine(baseline,"pixels.json")));
        var hashes=expected?.RootElement.EnumerateArray().ToDictionary(x=>x.GetProperty("id").GetString()!,x=>x.GetProperty("hash").GetString());
        var rows=new List<object>();
        foreach(var scale in new[]{1d,1.1,1.25,1.35,1.5}){
            var raster=new HighResolutionGrabRaster();
            foreach(var kind in new[]{"drool","eat"})for(var i=0;i<12;i++){
                var face=kind=="drool"?HighResolutionFacePose.Neutral with{DroolAge=i*.13}:
                    HighResolutionFacePose.Neutral with{MouthAge=i*.043};
                var frame=raster.Render(0,Matrix.Identity,new PointD(37.5,28.25),default,default,0,scale,face);
                var pixels=new byte[frame.PixelWidth*frame.PixelHeight*4];frame.CopyPixels(pixels,frame.PixelWidth*4,0);
                var hash=Convert.ToHexString(SHA256.HashData(pixels));var id=$"{scale:0.00}-{kind}-{i:00}";
                if(hashes is not null)Assert.Equal(hashes[id],hash);
                using var stream=File.Create(Path.Combine(path,id+".png"));var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(frame));encoder.Save(stream);
                rows.Add(new{id,hash});
            }
        }
        File.WriteAllText(Path.Combine(path,"pixels.json"),JsonSerializer.Serialize(rows));
    });
}

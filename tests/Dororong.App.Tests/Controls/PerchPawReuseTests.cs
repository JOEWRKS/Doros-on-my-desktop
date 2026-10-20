using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows.Media;
using Dororong.App.Controls;

namespace Dororong.App.Tests.Controls;

public class PerchPawReuseTests
{
    [Fact]
    public void Repeated_paw_and_stroke_preparation_does_not_allocate_large_arrays(){
        var raster=new HighResolutionGrabRaster();var bank=new HighResolutionLocomotion();
        var source=bank.Get(HighResolutionLocomotion.Perch);var original=(float[])source.Clone();
        var method=typeof(HighResolutionGrabRaster).GetMethod("PerchPaw",BindingFlags.Static|BindingFlags.Instance|BindingFlags.NonPublic)!;
        var face=HighResolutionFacePose.Neutral with{MotionFrame=HighResolutionLocomotion.Perch,Paw=1,PawY=6};
        float[] Prepare(HighResolutionFacePose pose)=>(float[])method.Invoke(raster,[source,pose])!;
        var first=HighResolutionBodyInk.AtScale(Prepare(face),375,1.5);var snapshot=(float[])first.Clone();
        var before=GC.GetAllocatedBytesForCurrentThread();
        for(var i=0;i<40;i++)HighResolutionBodyInk.AtScale(Prepare(face with{Paw=i%2+1,PawY=i%11}),375,1.5);
        var allocated=GC.GetAllocatedBytesForCurrentThread()-before;
        Assert.True(allocated<65536,$"Paw preparation allocated {allocated:N0} bytes for 40 poses");
        var restored=HighResolutionBodyInk.AtScale(Prepare(face),375,1.5);
        Assert.Same(first,restored);Assert.True(snapshot.SequenceEqual(restored));
        Assert.True(original.SequenceEqual(source));
    }

    [Fact]
    public void Export_and_compare_perch_paw_pixels()=>CheekProductTests.Sta(()=>{
        var output=Environment.GetEnvironmentVariable("DORORONG_PAW_OUTPUT");
        if(output is null)return;
        var baseline=Environment.GetEnvironmentVariable("DORORONG_PAW_BASELINE");
        var expected=baseline is null?null:JsonSerializer.Deserialize<Dictionary<string,string>>(File.ReadAllText(baseline));
        var rows=new Dictionary<string,string>();
        foreach(var scale in new[]{1d,1.1,1.25,1.35,1.5}){
            var raster=new HighResolutionGrabRaster();
            foreach(var paw in new[]{1,2})for(var i=0;i<8;i++){
                var face=HighResolutionFacePose.Neutral with{MotionFrame=HighResolutionLocomotion.Perch,Paw=paw,
                    PawX=(i-4)*2,PawY=(i-3)*3,PawAngle=(i-4)*5};
                var frame=raster.Render(0,Matrix.Identity,new(37.5,28.25),default,default,0,scale,face);
                var pixels=new byte[frame.PixelWidth*frame.PixelHeight*4];frame.CopyPixels(pixels,frame.PixelWidth*4,0);
                var id=$"{scale:F2}-{paw}-{i}";var hash=Convert.ToHexString(SHA256.HashData(pixels));
                if(expected is not null)Assert.Equal(expected[id],hash);
                rows.Add(id,hash);
            }
        }
        Directory.CreateDirectory(Path.GetDirectoryName(output)!);
        File.WriteAllText(output,JsonSerializer.Serialize(rows));
    });
}

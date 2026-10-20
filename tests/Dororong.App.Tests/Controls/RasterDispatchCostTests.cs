using System.Diagnostics;
using System.Text.Json;
using System.Windows.Media;
using Dororong.App.Controls;
using Xunit.Abstractions;

namespace Dororong.App.Tests.Controls;

public sealed class RasterDispatchCostTests(ITestOutputHelper output)
{
    [Fact]
    public void Compare_serial_and_parallel_sampling_without_changing_pixels()=>CheekProductTests.Sta(()=>{
        if(Environment.GetEnvironmentVariable("DORORONG_DISPATCH_PROFILE")!="1")return;
        var results=new List<object>();
        try{
            foreach(var kind in new[]{"idle","walk","grab","landing"}){
                byte[]? expected=null;
                foreach(var serial in new[]{false,true}){
                    RasterRowScheduler.CallerOnly=serial;
                    var raster=new HighResolutionGrabRaster();var times=new List<double>();
                    byte[]? retained=null;
                    for(var i=0;i<65;i++){
                        var face=HighResolutionFacePose.Neutral with{MotionFrame=kind=="walk"?225:-1};
                        retained=raster.RenderRetainedPixels(kind=="grab"?1:0,Matrix.Identity,new(37.5,28.25),default,default,kind=="landing"?1:0,1.5,face,1.5,retained);
                        if(i>=5)times.Add(raster.LastCost.Sampling);
                    }
                    if(expected is null)expected=(byte[])retained!.Clone();else Assert.Equal(expected,retained);
                    times.Sort();results.Add(new{kind,serial,mean=times.Average(),p95=times[(int)((times.Count-1)*.95)],max=times[^1]});
                }
            }
        }finally{RasterRowScheduler.CallerOnly=false;}
        output.WriteLine(JsonSerializer.Serialize(results));
    });
}

using System.IO;
using System.Text.Json;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.App.Runtime;
using Dororong.Core.Platforms;

namespace Dororong.App.Tests.Runtime;

public class LandingFrameCaptureTests
{
    [Fact]
    public async Task Release_capture_keeps_hold_frames_then_first_fall_without_waiting_for_contact(){
        var root=Path.Combine(Path.GetTempPath(),$"dororong-release-{Guid.NewGuid():N}");
        try{
            var recorder=new LandingFrameCapture(root,Frame(0),FrameCaptureTrigger.FirstFall);
            recorder.Observe(Frame(1),Stamp(0));Assert.Equal(0,recorder.SavedFrameCount);
            recorder.Arm();
            for(var i=0;i<20;i++)recorder.Observe(Frame((byte)i),new(i*16,1,null,0,100,200));
            Assert.Equal(0,recorder.SavedFrameCount);
            recorder.Observe(Frame(20),Stamp(320));Assert.Equal(17,recorder.SavedFrameCount);
            recorder.Arm(); // A later click cannot replace the first release.
            for(var i=21;i<=70;i++)recorder.Observe(Frame((byte)i),Stamp(i*16));
            Assert.True(recorder.Complete);Assert.InRange(recorder.BufferBytes,1,LandingFrameCapture.ByteBudget);
            var folder=(await recorder.ExportAsync())!;Assert.Equal(0,recorder.BufferBytes);
            using var json=JsonDocument.Parse(File.ReadAllText(Path.Combine(folder,"frames.json")));
            Assert.Equal("FirstFall",json.RootElement.GetProperty("trigger").GetString());
            var rows=json.RootElement.GetProperty("frames");
            Assert.Equal(0,rows[16].GetProperty("relativeToTriggerMs").GetDouble());
            Assert.Equal(2,rows[16].GetProperty("stamp").GetProperty("Pose").GetProperty("Phase").GetInt32());
            Assert.Equal(JsonValueKind.Null,rows[15].GetProperty("stamp").GetProperty("Pose").ValueKind);
        }finally{if(Directory.Exists(root))Directory.Delete(root,true);}
    }
    private static BitmapSource Frame(byte value,int size=4){
        var pixels=new byte[size*size*4];for(var i=0;i<pixels.Length;i+=4){pixels[i]=value;pixels[i+3]=255;}
        var bitmap=BitmapSource.Create(size,size,96,96,PixelFormats.Pbgra32,null,pixels,size*4);bitmap.Freeze();return bitmap;
    }
    private static LandingFrameStamp Stamp(int ms,bool impact=false)=>new(ms,2,
        new(impact?PlatformPhase.Landing:PlatformPhase.Falling,new(100,ms),null,0,0){IsExtremeLanding=impact,LegSpread=impact?.5:0},impact?.5:0,4,5);

    [Fact]
    public async Task Capture_is_opt_in_bounded_ordered_and_preserves_pixels_after_source_changes(){
        var root=Path.Combine(Path.GetTempPath(),$"dororong-frames-{Guid.NewGuid():N}");
        try{
            var recorder=new LandingFrameCapture(root,Frame(0));
            recorder.Observe(Frame(99),Stamp(0,true));Assert.Equal(0,recorder.SavedFrameCount);
            recorder.Arm();
            for(var i=0;i<100;i++)recorder.Observe(Frame((byte)i),Stamp(i));
            Assert.False(Directory.Exists(root));
            recorder.Observe(Frame(100),Stamp(100,true));
            for(var i=101;i<300;i++)recorder.Observe(Frame((byte)i),Stamp(i,true));
            Assert.True(recorder.Complete);Assert.Equal(96,recorder.SavedFrameCount);
            Assert.InRange(recorder.BufferBytes,1,LandingFrameCapture.ByteBudget);
            var task=recorder.ExportAsync();Assert.Same(task,recorder.ExportAsync());
            var folder=(await task)!;Assert.Equal(0,recorder.BufferBytes);
            using var json=JsonDocument.Parse(File.ReadAllText(Path.Combine(folder,"frames.json")));
            var rows=json.RootElement.GetProperty("frames");Assert.Equal(96,rows.GetArrayLength());
            Assert.Equal(84,rows[0].GetProperty("stamp").GetProperty("Ms").GetDouble());
            Assert.Equal(0,rows[16].GetProperty("relativeToImpactMs").GetDouble());
            Assert.Equal(179,rows[95].GetProperty("stamp").GetProperty("Ms").GetDouble());
            using var stream=File.OpenRead(Path.Combine(folder,"016.png"));
            var decoded=new PngBitmapDecoder(stream,BitmapCreateOptions.PreservePixelFormat,BitmapCacheOption.OnLoad).Frames[0];
            var pixels=new byte[4*4*4];decoded.CopyPixels(pixels,16,0);Assert.Equal(100,pixels[0]);
        }finally{if(Directory.Exists(root))Directory.Delete(root,true);}
    }

    [Fact]
    public async Task No_falling_or_no_arm_does_not_claim_impact_and_resize_stops_instead_of_resampling(){
        var root=Path.Combine(Path.GetTempPath(),$"dororong-frames-{Guid.NewGuid():N}");
        try{
            var recorder=new LandingFrameCapture(root,Frame(0));recorder.Arm();
            recorder.Observe(Frame(1),Stamp(10,true));Assert.Equal(0,recorder.SavedFrameCount);
            recorder.Observe(Frame(2,8),Stamp(20));Assert.True(recorder.Complete);
            var folder=(await recorder.ExportAsync())!;
            using var json=JsonDocument.Parse(File.ReadAllText(Path.Combine(folder,"frames.json")));
            Assert.False(json.RootElement.GetProperty("triggered").GetBoolean());
            Assert.Contains("size/DPI",json.RootElement.GetProperty("error").GetString());
            Assert.Equal(0,json.RootElement.GetProperty("frames").GetArrayLength());
        }finally{if(Directory.Exists(root))Directory.Delete(root,true);}
    }

    [Fact]
    public async Task Export_failure_releases_buffers_and_repeated_arming_cannot_overwrite_triggered_clip(){
        var root=Path.Combine(Path.GetTempPath(),$"dororong-frames-{Guid.NewGuid():N}");
        File.WriteAllText(root,"block directory creation");
        try{
            var recorder=new LandingFrameCapture(root,Frame(0));recorder.Arm();
            recorder.Observe(Frame(1),Stamp(10));recorder.Observe(Frame(2),Stamp(20,true));
            recorder.Arm();Assert.Equal(2,recorder.SavedFrameCount);
            Assert.Null(await recorder.ExportAsync());Assert.Equal(0,recorder.BufferBytes);
        }finally{File.Delete(root);}
    }
}

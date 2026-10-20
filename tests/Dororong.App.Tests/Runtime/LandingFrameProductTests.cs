using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;
using Dororong.App.Interaction;
using Dororong.App.Runtime;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;

namespace Dororong.App.Tests.Runtime;

public class LandingFrameProductTests
{
    [Fact]
    public async Task Performance_capture_arms_on_body_input_and_exports_on_dispose()
    {
        var root=Path.Combine(Path.GetTempPath(),$"dororong-wired-frames-{Guid.NewGuid():N}");
        Task<string?>? export=null;
        Task? performanceExport=null;
        try{
            Controls.CheekProductTests.Sta(()=>{
                var presenter=new DororongPresenter{HighResolutionGrabEnabled=true};
                presenter.Render(new(PetState.Idle,new(100,0),FacingDirection.Right,0,false,null),DirectInteractionSnapshot.None);
                presenter.Measure(new(144,144));presenter.Arrange(new Rect(0,0,144,144));presenter.UpdateLayout();
                using var capture=new RuntimePerformanceCapture(Path.Combine(root,"performance.json"),presenter,false,landingFrameDirectory:root);
                capture.Input("Body");
                var tick=typeof(RuntimePerformanceCapture).GetMethod("OnTick",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!;
                presenter.ApplyPlatformPose(new(PlatformPhase.Falling,new(100,100),null,0,0),100);
                tick.Invoke(capture,[16d,2d]);
                presenter.ApplyPlatformPose(new PlatformPose(PlatformPhase.Landing,new(100,100),null,0,0){IsExtremeLanding=true},100);
                tick.Invoke(capture,[16d,2d]);
                capture.Dispose();export=capture.FrameExport;performanceExport=capture.CaptureExport;
            });
            var folder=await export!;Assert.NotNull(folder);
            await performanceExport!;
            using var manifest=JsonDocument.Parse(File.ReadAllText(Path.Combine(folder!,"frames.json")));
            Assert.True(manifest.RootElement.GetProperty("triggered").GetBoolean());
            Assert.Equal(2,manifest.RootElement.GetProperty("frames").GetArrayLength());
            using var perf=JsonDocument.Parse(File.ReadAllText(Path.Combine(root,"performance.json")));
            Assert.Equal(2,perf.RootElement.GetProperty("landingFrameCapture").GetProperty("SavedFrameCount").GetInt32());
        }finally{if(Directory.Exists(root))Directory.Delete(root,true);}
    }

    [Theory]
    [InlineData(1)] [InlineData(1.1)] [InlineData(1.25)] [InlineData(1.35)] [InlineData(1.5)]
    public async Task Real_presenter_frames_export_with_precontact_expansion_at_each_scale(double scale)
    {
        var keep=Environment.GetEnvironmentVariable("DORORONG_LANDING_FRAME_OUTPUT");
        var root=keep is null?Path.Combine(Path.GetTempPath(),$"dororong-product-frames-{Guid.NewGuid():N}"):Path.Combine(keep,$"scale-{scale:F2}");
        Task<string?>? export=null;
        try{
            Controls.CheekProductTests.Sta(()=>{
                var presenter=new DororongPresenter{HighResolutionGrabEnabled=true,HighResolutionScale=scale};
                var pet=new PetSnapshot(PetState.Idle,new(100,0),FacingDirection.Right,0,false,null);
                presenter.Render(pet,DirectInteractionSnapshot.None);
                presenter.Measure(new(144*scale,144*scale));presenter.Arrange(new Rect(0,0,144*scale,144*scale));presenter.UpdateLayout();
                var contact=presenter.MeasurePlatformGeometry()!.Value.Contact;
                var recorder=new LandingFrameCapture(root,(BitmapSource)presenter.HighResolutionImage!.Source);
                recorder.Arm();var motion=new PlatformMotion();var position=pet.Position;
                var floor=new PlatformSurface(new(0,0,1),1,PlatformKind.Floor,0,1000,1200,new(0,1200));
                for(var i=1;i<=200 && !recorder.Complete;i++){
                    var pose=motion.Advance(new(TimeSpan.FromMilliseconds(16),position,position,contact,[floor],false,true)
                        {Monitors=[new(1,new(0,0,1000,1000))],AnticipateExtremeLanding=true});
                    position=pose.Position;
                    presenter.BeginHighResolutionPlatformFrame();
                    presenter.Render(pet with{Position=position},DirectInteractionSnapshot.None);
                    presenter.ApplyPlatformPose(pose,contact.SoleY);
                    recorder.Observe((BitmapSource)presenter.HighResolutionImage.Source,new(i*16,0,pose,presenter.LastRenderedSpread,null,null));
                }
                Assert.True(recorder.Complete);Assert.True(recorder.SavedFrameCount>16);
                Assert.InRange(recorder.BufferBytes,1,LandingFrameCapture.ByteBudget);
                export=recorder.ExportAsync();
            });
            var folder=await export!;Assert.NotNull(folder);
            using var doc=JsonDocument.Parse(File.ReadAllText(Path.Combine(folder!,"frames.json")));
            var rows=doc.RootElement.GetProperty("frames").EnumerateArray().ToArray();
            var impact=rows.Single(r=>r.GetProperty("relativeToImpactMs").GetDouble()==0);
            Assert.Equal(1,impact.GetProperty("stamp").GetProperty("RenderedSpread").GetDouble());
            var airborne=rows.Where(r=>r.GetProperty("relativeToImpactMs").GetDouble()<0 &&
                r.GetProperty("stamp").GetProperty("RenderedSpread").GetDouble()>0).ToArray();
            Assert.NotEmpty(airborne);
            Assert.All(airborne,r=>Assert.Equal((int)PlatformPhase.Falling,r.GetProperty("stamp").GetProperty("Pose").GetProperty("Phase").GetInt32()));
            Assert.Equal(rows.Length,Directory.GetFiles(folder!,"*.png").Length);
        }finally{if(keep is null&&Directory.Exists(root))Directory.Delete(root,true);}
    }
}

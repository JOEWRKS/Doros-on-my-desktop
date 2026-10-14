using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.App.Controls;
using Dororong.Core.Platforms;

namespace Dororong.App.Tests.Runtime;

public partial class PetLoopPlatformTests
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void Happy_after_actual_eating_stays_planted_and_direct_press_interrupts(bool interrupt) => Controls.CheekProductTests.Sta(()=>
    {
        using var h=new Harness(realPresenter:true,snackPresentation:true);h.Native.Scene=Scene(windows:false);h.Start();
        for(var i=0;i<240;i++)h.Tick();
        h.Pointer=new(true,h.Position+h.Presenter!.GetSnackMouth());h.Loop.ToggleSnack();
        for(var i=0;i<210&&h.Snack.Phase!=SnackPhase.Happy;i++)
        {h.Pointer=new(true,h.Position+h.Presenter.GetSnackMouth());h.Tick();}
        Assert.Equal(SnackPhase.Happy,h.Snack.Phase);var position=h.Position;
        h.Pointer=new(true,new(700,50));
        for(var i=0;i<20;i++)
        {
            h.Tick();Assert.Equal(SnackPhase.Happy,h.Snack.Phase);
            Assert.InRange(Math.Abs(h.Position.X-position.X),0,.01);
            Assert.InRange(Math.Abs(h.Position.Y-position.Y),0,.01);
            Assert.Equal(PlatformPhase.Supported,h.LastPose!.Value.Phase);
        }
        if(interrupt){h.Press(0);Assert.False(h.Loop.IsSnackActive);}
        else
        {
            for(var i=0;i<180&&h.Snack.Phase!=SnackPhase.None;i++)h.Tick();
            Assert.False(h.Loop.IsSnackActive);Assert.Equal(SnackPhase.None,h.Snack.Phase);
        }
        h.Tick();Assert.Null(h.Fault);
    });
    [Fact]
    public void Snack_entry_releases_perch_in_place_and_falls_continuously() => Controls.CheekProductTests.Sta(()=>
    {
        using var h=AttachedHarness(advanceTicks:10);
        var attached=h.Position;h.Pointer=new(true,new(700,350));h.Loop.ToggleSnack();
        Assert.Equal(attached,h.Position);Assert.Equal(EdgePerchPhase.None,h.Platforms.PerchPhase);
        h.Tick();Assert.InRange(h.Position.Y,attached.Y,attached.Y+1);
        Assert.InRange(h.Position.X,attached.X,attached.X+3.1);
        Assert.True(h.Loop.IsSnackActive);Assert.Null(h.Fault);
    });
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void Snack_entry_follows_actual_support_instead_of_resetting_to_taskbar(bool taskbar) => Controls.CheekProductTests.Sta(()=>
    {
        using var h=new Harness(realPresenter:true,snackPresentation:true);h.Start();
        var top=taskbar?560:200;
        h.Native.Scene=taskbar ? new(2,TimeSpan.Zero,[new(1,new(0,0,800,600))],
            [new(new(2,2,2),new(0,top,800,40),0,true,false,false,false,true,true,true,true)]) : Scene();
        h.Position=new(150,top-h.Presenter!.MeasurePlatformContact()!.Value.SoleY);
        h.Tick(80);for(var i=0;i<100;i++)h.Tick();
        h.Loop.NotifySitRequested();for(var i=0;i<100;i++)h.Tick();
        h.Pointer=new(true,new(700,590));var start=h.Position;h.Loop.ToggleSnack();
        Assert.Equal(start,h.Position);
        for(var i=0;i<10;i++)h.Tick();
        if(!taskbar)h.Native.Scene=Scene(120,180);
        h.Tick(80);
        var sole=h.Position.Y+h.Presenter.MeasurePlatformContact()!.Value.SoleY;
        Assert.InRange(sole,top-(taskbar?0:20)-.01,top-(taskbar?0:20)+.01);
        Assert.Equal(PlatformPhase.Supported,h.LastPose!.Value.Phase);
    });
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void Snack_entry_keeps_window_support_during_sitting_or_standing(bool sitting) => Controls.CheekProductTests.Sta(()=>
    {
        using var h=new Harness(realPresenter:true,snackPresentation:true);h.Start();
        h.Position=new(150,200-h.Presenter!.MeasurePlatformContact()!.Value.SoleY);
        for(var i=0;i<100;i++)h.Tick();
        if(sitting){h.Loop.NotifySitRequested();for(var i=0;i<100;i++)h.Tick();}
        var initialSole=h.Position.Y+h.Presenter.MeasurePlatformContact()!.Value.SoleY;
        Assert.InRange(initialSole,199.99,200.01);
        h.Pointer=new(true,new(700,350));h.Loop.ToggleSnack();
        for(var i=0;i<25;i++)
        {
            h.Tick();var sole=h.Position.Y+h.Presenter.MeasurePlatformContact()!.Value.SoleY;
            Assert.InRange(sole,initialSole-1,initialSole+1);
        }
        Assert.Null(h.Fault);
    });
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void First_snack_with_real_presenter_starts_at_displayed_pet_not_pointer(bool sitting) => Controls.CheekProductTests.Sta(()=>
    {
        using var h=new Harness(realPresenter:true,snackPresentation:true);h.Native.Scene=Scene(windows:false);h.Start();
        for(var i=0;i<240;i++)h.Tick();
        if(sitting){h.Loop.NotifySitRequested();for(var i=0;i<100;i++)h.Tick();}
        var start=h.Position;
        h.Pointer=new(true,new(700,150));h.Loop.ToggleSnack();
        Assert.Equal(start,h.Position);
        for(var i=0;i<12;i++)
        {
            var previous=h.Position;h.Tick();
            Assert.InRange(Math.Abs(h.Position.X-previous.X),0,3.1);
            Assert.InRange(Math.Abs(h.Position.Y-previous.Y),0,2);
        }
        Assert.True(h.Position.X<start.X+38);Assert.Null(h.Fault);
    });
    [Fact]
    public void Actual_loop_eating_replays_three_visible_source_mouth_beats() => Controls.CheekProductTests.Sta(()=>
    {
        using var h=new Harness(realPresenter:true,snackPresentation:true);h.Native.Scene=Scene(windows:false);h.Start();
        for(var i=0;i<240;i++)h.Tick();
        h.Pointer=new(true,h.Position+h.Presenter!.GetSnackMouth());h.Loop.ToggleSnack();
        for(var i=0;i<80 && h.Snack.Phase!=SnackPhase.Eating;i++)
        {h.Pointer=new(true,h.Position+h.Presenter.GetSnackMouth());h.Tick();}
        Assert.Equal(SnackPhase.Eating,h.Snack.Phase);
        var source=PremultipliedFrame.From(UprightRumpSource.Standing(true)).Pixels;
        var beats=0;var wasStrong=false;
        for(var i=0;i<120 && h.Snack.Phase==SnackPhase.Eating;i++)
        {
            h.Tick();if(h.Snack.Age<=.48||h.Snack.Age>=1.55)continue;
            var image=(System.Windows.Controls.Image)h.Presenter.FindName("DororongImage");
            var pixels=PremultipliedFrame.From((System.Windows.Media.Imaging.BitmapSource)image.Source).Pixels;
            var count=0;
            for(var at=0;at<pixels.Length;at+=4)if(Math.Abs(pixels[at+2]-source[at+2])>=40)count++;
            var strong=count>=12;if(strong&&!wasStrong)beats++;wasStrong=strong;
        }
        Assert.Equal(3,beats);Assert.Null(h.Fault);
    });
    [Fact] public void Snack_chase_keeps_platform_physics_and_never_captures_mouse()
    {
        using var h=new Harness();h.Start();h.Tick(80);
        h.Pointer=new(true,new(700,300));h.Loop.ToggleSnack();var start=h.Position;
        for(var i=0;i<60;i++)h.Tick();
        Assert.True(h.Loop.IsSnackActive);Assert.True(h.Position.X>start.X+100);
        Assert.Equal(0,h.Captures);Assert.NotNull(h.LastPose);Assert.Null(h.Fault);
        h.Loop.ToggleSnack();Assert.False(h.Loop.IsSnackActive);h.Tick();Assert.Null(h.Fault);
    }
    [Fact] public void Snack_takes_over_pending_carry_and_direct_press_can_interrupt_it()
    {
        using var h=new Harness();h.Start();h.Tick();h.Press(0);h.Tick();
        h.Loop.ToggleSnack();Assert.True(h.Loop.IsSnackActive);Assert.Equal(h.Captures,h.Releases);
        h.Press(0);Assert.False(h.Loop.IsSnackActive);h.Tick();Assert.Null(h.Fault);
    }
    [Fact] public void Lost_input_retires_snack_without_leaving_carry_or_chase()
    {
        using var h=new Harness();h.Start();h.Pointer=new(true,new(700,300));h.Loop.ToggleSnack();h.Tick();
        h.Pointer=PointerSample.Unavailable;h.Tick();Assert.False(h.Loop.IsSnackActive);
        h.Tick();Assert.Null(h.Fault);Assert.Equal(0,h.Captures);
    }
    [Fact] public void Snack_jump_lands_back_on_actual_floor()
    {
        using var h=new Harness();h.Native.Scene=Scene(windows:false);h.Start();
        for(var i=0;i<180;i++)h.Tick();
        var ground=h.Position.Y;
        h.Pointer=new(true,new PointD(h.Position.X+54,h.Position.Y-200));h.Loop.ToggleSnack();
        var peak=ground;
        for(var i=0;i<85;i++){h.Tick();peak=Math.Min(peak,h.Position.Y);}
        Assert.True(peak<ground-40,$"peak={peak}, ground={ground}");
        Assert.InRange(h.Position.Y,ground-1,ground+1);Assert.Null(h.Fault);
    }
}

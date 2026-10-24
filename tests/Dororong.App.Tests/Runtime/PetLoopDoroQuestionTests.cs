using Dororong.Core.Behavior;

namespace Dororong.App.Tests.Runtime;

public partial class PetLoopPlatformTests
{
    [Theory][InlineData(false)][InlineData(true)]
    public void Minute_question_repeats_or_yields_to_sleep_in_host_loop(bool sleepEnabled)
    {
        using var h=new Harness(platformEnabled:false,doroQuestion:true);
        h.Loop.SetAutomaticSleepEnabled(sleepEnabled);h.Start();
        var starts=0;var asking=false;
        for(var i=0;i<3875;i++){
            h.Tick(80);
            if(h.Core.DoroQuestionPhase is not null&&!asking)starts++;
            asking=h.Core.DoroQuestionPhase is not null;
            if(!sleepEnabled)Assert.NotEqual(PetState.Sleep,h.Core.State);
            if(sleepEnabled&&i>=2251)Assert.Equal(PetState.Sleep,h.Core.State);
        }
        Assert.Equal(sleepEnabled?2:5,starts);
    }

    [Theory]
    [InlineData("size")]
    [InlineData("sit")]
    [InlineData("snack")]
    [InlineData("press")]
    public void Doro_question_is_canceled_by_host_interaction(string action)
    {
        using var h = new Harness(platformEnabled: false, doroQuestion: true);
        h.Start();
        var ticks = 0;
        while (h.Core.DoroQuestionPhase is null && ticks++ < 1300) h.Tick(80);
        Assert.NotNull(h.Core.DoroQuestionPhase);
        Assert.InRange(ticks * .08, 63, 68);
        var position = h.Position;
        if (action == "size") Assert.True(h.Loop.TryResize(() => {}));
        else if (action == "sit") h.Loop.NotifySitRequested();
        else if (action == "snack") h.Loop.ToggleSnack();
        else h.Press(0);
        h.Tick();
        Assert.Null(h.Core.DoroQuestionPhase);
        Assert.NotEqual(PetState.Sleep, h.Core.State);
        if (action is "size" or "sit") Assert.Equal(position, h.Position);
    }
}

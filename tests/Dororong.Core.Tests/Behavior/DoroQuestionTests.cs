using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Core.Tests.TestSupport;

namespace Dororong.Core.Tests.Behavior;

public sealed class DoroQuestionTests
{
    private static PetBrain Brain(double walkSeconds=3,double sleepSeconds=180)=>new(BehaviorTuning.Default with {
        MaxDelta=TimeSpan.FromSeconds(1),IdleMin=TimeSpan.FromSeconds(1),IdleMax=TimeSpan.FromSeconds(1),
        WalkMin=TimeSpan.FromSeconds(walkSeconds),WalkMax=TimeSpan.FromSeconds(walkSeconds),IdleToWalkProbability=0,
        SleepDelay=TimeSpan.FromSeconds(sleepSeconds),
        DoroQuestionDelay=TimeSpan.FromMinutes(1)},new SeededRandomSource(17),new(200,200));
    private static void Advance(PetBrain b,int seconds){for(var i=0;i<seconds;i++)b.Update(PetTestInput.At(1));}

    [Fact]
    public void Default_sleep_is_three_minutes_and_question_is_host_opt_in(){
        Assert.Equal(TimeSpan.FromMinutes(3),BehaviorTuning.Default.SleepDelay);
        Assert.Null(BehaviorTuning.Default.DoroQuestionDelay);
    }
    [Fact]
    public void Sixty_seconds_walk_then_question_repeats_at_two_minutes_and_sleeps_at_three(){
        var b=Brain();Advance(b,59);Assert.Equal(PetState.Idle,b.Current.State);Assert.Null(b.Current.DoroQuestionPhase);
        Advance(b,1);Assert.Equal(PetState.Walk,b.Current.State);var start=b.Current.Position;
        Advance(b,1);Assert.NotEqual(start,b.Current.Position);Assert.Null(b.Current.DoroQuestionPhase);
        Advance(b,2);Assert.Equal(PetState.Idle,b.Current.State);Assert.Equal(0,b.Current.DoroQuestionPhase);
        var planted=b.Current.Position;Advance(b,1);Assert.Equal(planted,b.Current.Position);
        Assert.Equal(1d/3,b.Current.DoroQuestionPhase!.Value,6);
        Advance(b,2);Assert.Null(b.Current.DoroQuestionPhase);
        Advance(b,54);Assert.Equal(PetState.Walk,b.Current.State);
        Advance(b,3);Assert.Equal(0,b.Current.DoroQuestionPhase);
        Advance(b,56);Assert.NotEqual(PetState.Sleep,b.Current.State);
        Advance(b,1);Assert.Equal(PetState.Sleep,b.Current.State);
        Assert.Null(b.Current.DoroQuestionPhase);Advance(b,120);Assert.Equal(PetState.Sleep,b.Current.State);
    }
    [Fact]
    public void Sleep_off_repeats_every_minute_without_interval_drifting_by_motion_duration(){
        var b=Brain();b.SetAutomaticSleepEnabled(false);var starts=new List<int>();var was=false;
        for(var i=1;i<=610;i++){
            var p=b.Update(PetTestInput.At(1));Assert.NotEqual(PetState.Sleep,p.State);
            if(p.DoroQuestionPhase is not null&&!was)starts.Add(i);
            was=p.DoroQuestionPhase is not null;
        }Assert.Equal(Enumerable.Range(1,10).Select(n=>n*60+3),starts);
    }
    [Theory][InlineData(61)][InlineData(64)][InlineData(124)]
    public void Interaction_cancels_wander_or_question_and_rearms_sixty_seconds(int at){
        var b=Brain();Advance(b,at);
        b.Update(PetTestInput.At(.1,localInteractionActive:true));Assert.Null(b.Current.DoroQuestionPhase);
        Advance(b,59);Assert.Null(b.Current.DoroQuestionPhase);
        Advance(b,1);Assert.Equal(PetState.Walk,b.Current.State);
        Advance(b,3);Assert.NotNull(b.Current.DoroQuestionPhase);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public void Suspension_pauses_but_interactive_tracking_cancels_question(bool interactive){
        var b=Brain();Advance(b,64);var before=b.Current;
        for(var i=0;i<30;i++)b.Update(PetTestInput.At(1) with{SuspendAutonomousMotion=true,TrackPointerFacing=interactive});
        if(interactive)Assert.Null(b.Current.DoroQuestionPhase);else Assert.Equal(before,b.Current);
    }
    [Fact]
    public void User_menu_activity_cancels_question_without_relocating(){
        var b=Brain();Advance(b,64);var p=b.Current.Position;b.NotifyActivity();
        Assert.Null(b.Current.DoroQuestionPhase);Assert.Equal(p,b.Current.Position);
    }
    [Fact]
    public void Play_resume_clears_question_and_restarts_both_inactivity_thresholds(){
        var b=Brain();Advance(b,64);b.ResumeAfterPlay(FacingDirection.Left);
        Assert.Null(b.Current.DoroQuestionPhase);Assert.Equal(FacingDirection.Left,b.Current.Facing);
        Advance(b,59);Assert.Null(b.Current.DoroQuestionPhase);Assert.NotEqual(PetState.Sleep,b.Current.State);
        Advance(b,4);Assert.NotNull(b.Current.DoroQuestionPhase);
    }
    [Fact]
    public void Sleep_deadline_takes_priority_over_unfinished_wander(){
        var b=Brain(walkSeconds:200);Advance(b,179);Assert.Equal(PetState.Walk,b.Current.State);
        Advance(b,1);Assert.Equal(PetState.Sleep,b.Current.State);Assert.Null(b.Current.DoroQuestionPhase);
    }
    [Fact]
    public void Sleep_deadline_clears_a_question_in_progress(){
        var b=Brain(sleepSeconds:64);Advance(b,63);Assert.NotNull(b.Current.DoroQuestionPhase);
        Advance(b,1);Assert.Equal(PetState.Sleep,b.Current.State);Assert.Null(b.Current.DoroQuestionPhase);
    }
    [Fact]
    public void Sleep_toggle_restarts_epoch_and_switches_between_repeating_and_sleeping(){
        var b=Brain();b.SetAutomaticSleepEnabled(false);Advance(b,184);Assert.NotNull(b.Current.DoroQuestionPhase);
        b.SetAutomaticSleepEnabled(true);Assert.Null(b.Current.DoroQuestionPhase);
        Advance(b,179);Assert.NotEqual(PetState.Sleep,b.Current.State);
        Advance(b,1);Assert.Equal(PetState.Sleep,b.Current.State);
        b.SetAutomaticSleepEnabled(false);Assert.Equal(PetState.Idle,b.Current.State);
        Advance(b,183);Assert.NotNull(b.Current.DoroQuestionPhase);Assert.NotEqual(PetState.Sleep,b.Current.State);
    }
    [Fact]
    public void Question_has_smooth_closed_endpoints_and_no_body_or_speed_parameter(){
        Assert.Equal(0,DoroQuestionMotion.HeadRoll(0));Assert.Equal(0,DoroQuestionMotion.HeadRoll(1));
        Assert.Equal(0,DoroQuestionMotion.BubbleOpacity(0));Assert.Equal(0,DoroQuestionMotion.BubbleOpacity(1));
        for(var i=0;i<=180;i++){
            var phase=i/180d;
            Assert.InRange(DoroQuestionMotion.HeadRoll(phase),-8*Math.PI/180,0);
            static double Smooth(double v){var t=Math.Clamp(v,0,1);return t*t*(3-2*t);}
            var previousBodywardRoll=8*Math.PI/180*Smooth(phase/.18)*(1-Smooth((phase-.76)/.24));
            Assert.Equal(-previousBodywardRoll,DoroQuestionMotion.HeadRoll(phase),12);
            Assert.InRange(DoroQuestionMotion.BubbleOpacity(i/180d),0,1);
        }
    }
}

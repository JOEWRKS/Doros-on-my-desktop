using Dororong.Core.Behavior;
using Dororong.Core.Tests.TestSupport;

namespace Dororong.Core.Tests.Behavior;

public sealed class IdleActivityTests
{
    private static PetBrain Brain()=>new(BehaviorTuning.Default with{
        IdleActivitiesEnabled=true,DoroQuestionDelay=TimeSpan.FromSeconds(60),
        IdleMin=TimeSpan.FromSeconds(1),IdleMax=TimeSpan.FromSeconds(1),IdleToWalkProbability=0,
        WalkMin=TimeSpan.FromSeconds(3),WalkMax=TimeSpan.FromSeconds(3)},new SeededRandomSource(3),new(200,200));
    private static void Advance(PetBrain brain,double seconds){
        for(var i=0;i<(int)Math.Round(seconds*100);i++)brain.Update(PetTestInput.At(.01));
    }
    [Fact] public void Default_host_has_no_new_activity(){
        Assert.False(BehaviorTuning.Default.IdleActivitiesEnabled);
        var b=new PetBrain(BehaviorTuning.Default,new SeededRandomSource(1),default);
        b.NotifyMealCompleted();Assert.False(b.Current.IdleActivityPending);
    }
    [Fact] public void Alternate_completed_meals_beg_after_existing_sequence(){
        var b=Brain();
        for(var meal=1;meal<=6;meal++){
            b.ResumeAfterPlay(FacingDirection.Left);b.NotifyMealCompleted();Advance(b,.1);
            Assert.Equal(meal%2==1?IdleActivityKind.Beg:IdleActivityKind.None,b.Current.IdleActivity.Kind);
            var p=b.Current.Position;Advance(b,2.5);Assert.Equal(p,b.Current.Position);
            Assert.Equal(IdleActivityKind.None,b.Current.IdleActivity.Kind);
        }
    }
    [Fact] public void Groom_does_not_postpone_minute_question_or_three_minute_sleep(){
        var b=Brain();Advance(b,46);Assert.Equal(IdleActivityKind.Groom,b.Current.IdleActivity.Kind);
        Advance(b,14);Assert.Equal(PetState.Walk,b.Current.State);Assert.Equal(IdleActivityKind.None,b.Current.IdleActivity.Kind);
        Advance(b,3);Assert.NotNull(b.Current.DoroQuestionPhase);
        Advance(b,57);Assert.Equal(PetState.Walk,b.Current.State);
        Advance(b,60);Assert.Equal(PetState.Sleep,b.Current.State);Assert.Equal(IdleActivityKind.None,b.Current.IdleActivity.Kind);
    }
    [Fact] public void Sleep_option_wakes_into_one_stretch(){
        var b=Brain();Advance(b,180);b.SetAutomaticSleepEnabled(false);
        Advance(b,.1);Assert.Equal(IdleActivityKind.Stretch,b.Current.IdleActivity.Kind);
        Advance(b,3);Assert.Equal(IdleActivityKind.None,b.Current.IdleActivity.Kind);
        Advance(b,20);Assert.Equal(IdleActivityKind.None,b.Current.IdleActivity.Kind);
    }
    [Fact] public void Wake_click_takes_priority_and_stretch_waits_for_release(){
        var b=Brain();Advance(b,180);
        b.Update(PetTestInput.At(.01,pointer:new(220,220),primaryDown:true,bodyPressPosition:new(220,220)));
        Assert.True(b.Current.IdleActivityPending);Assert.Equal(IdleActivityKind.None,b.Current.IdleActivity.Kind);
        b.Update(PetTestInput.At(.01,pointer:new(220,220)));
        Assert.Equal(PetState.ClickReaction,b.Current.State);
        Advance(b,.7);Assert.Equal(IdleActivityKind.Stretch,b.Current.IdleActivity.Kind);
    }
    [Fact] public void Direct_interaction_cancels_beg_and_groom(){
        var b=Brain();b.NotifyMealCompleted();Advance(b,.1);
        b.NotifyActivity();Assert.Equal(IdleActivityKind.None,b.Current.IdleActivity.Kind);
        Advance(b,46);Assert.Equal(IdleActivityKind.Groom,b.Current.IdleActivity.Kind);
        b.Update(PetTestInput.At(.01,localInteractionActive:true));
        Assert.Equal(IdleActivityKind.None,b.Current.IdleActivity.Kind);
    }
    [Fact] public void Fall_suspension_freezes_activity_clock(){
        var b=Brain();b.NotifyMealCompleted();Advance(b,.5);var before=b.Current;
        for(var i=0;i<100;i++)b.Update(PetTestInput.At(.1) with{SuspendAutonomousMotion=true});
        Assert.Equal(before,b.Current);
    }
    [Fact] public void Snack_cancel_is_not_a_completed_meal_and_success_is_one_tick(){
        var s=new SnackSession();s.Begin(FacingDirection.Right);s.Cancel();Assert.False(s.MealCompleted);
        s.Begin(FacingDirection.Right);
        for(var i=0;i<120&&!s.MealCompleted;i++)s.Advance(TimeSpan.FromSeconds(.05),new(true,new(0,0)),new(0,0),true);
        Assert.True(s.MealCompleted);Assert.False(s.Active);
        s.Advance(TimeSpan.FromSeconds(.05),new(true,new(0,0)),new(0,0),true);Assert.False(s.MealCompleted);
    }
}

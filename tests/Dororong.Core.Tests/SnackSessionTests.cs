using Dororong.Core.Behavior;
using Dororong.Core.Geometry;

namespace Dororong.Core.Tests;

public class SnackSessionTests
{
    private static readonly TimeSpan Tick = TimeSpan.FromMilliseconds(16);
    [Theory]
    [InlineData(20,50,185)]
    [InlineData(100,10,92.5)]
    public void Chase_distance_documents_existing_elapsed_cap(int milliseconds,int ticks,double expected)
    {
        var s=new SnackSession();s.Begin(FacingDirection.Right);var distance=0d;
        for(var i=0;i<ticks;i++)distance+=s.Advance(TimeSpan.FromMilliseconds(milliseconds),new(true,new(10000,50)),new(50,50),true).DeltaX;
        Assert.InRange(distance,expected-.001,expected+.001);
    }
    [Fact] public void Chases_without_proximity_limit_and_caps_speed()
    {
        var session = new SnackSession(); session.Begin(FacingDirection.Left);
        var p = session.Advance(Tick,new(true,new(10000,50)),new(50,50),true);
        Assert.Equal(FacingDirection.Right,p.Facing); Assert.InRange(p.DeltaX,2,3);
    }
    [Fact] public void Stationary_mouth_offer_eats_then_finishes_once()
    {
        var s=new SnackSession();s.Begin(FacingDirection.Right);
        for(var i=0;i<32;i++)s.Advance(Tick,new(true,new(50,50)),new(50,50),true);
        Assert.Equal(SnackPhase.Eating,s.Current.Phase);
        for(var i=0;i<300;i++)s.Advance(Tick,new(true,new(900,50)),new(50,50),true);
        Assert.False(s.Active);
    }
    [Fact] public void Moving_past_mouth_does_not_feed()
    {
        var s=new SnackSession();s.Begin(FacingDirection.Right);
        for(var i=0;i<80;i++)s.Advance(Tick,new(true,new(i%2==0?40:60,50)),new(50,50),true);
        Assert.Equal(SnackPhase.Chasing,s.Current.Phase);
    }
    [Fact] public void High_snack_jumps_with_bounded_height_and_lands()
    {
        var s=new SnackSession();s.Begin(FacingDirection.Right);var min=0d;
        for(var i=0;i<70;i++) min=Math.Min(min,s.Advance(Tick,new(true,new(50,-999)),new(50,50),true).Lift);
        Assert.InRange(min,-76,-70);Assert.Equal(0,s.Current.Lift);
    }
    [Fact] public void Falling_cannot_launch_a_jump()
    {
        var s=new SnackSession();s.Begin(FacingDirection.Right);
        for(var i=0;i<20;i++)s.Advance(Tick,new(true,new(50,-999)),new(50,50),false);
        Assert.Equal(0,s.Current.Lift);
    }
    [Fact] public void Missing_pointer_or_resume_cancels()
    {
        var s=new SnackSession();s.Begin(FacingDirection.Right);
        s.Advance(Tick,PointerSample.Unavailable,new(50,50),true);Assert.False(s.Active);
        s.Begin(FacingDirection.Right);s.Advance(TimeSpan.FromSeconds(5),new(true,new(50,50)),new(50,50),true);
        Assert.False(s.Active);
    }
    [Fact] public void Restart_clears_old_bite_and_jump()
    {
        var s=new SnackSession();s.Begin(FacingDirection.Right);
        for(var i=0;i<30;i++)s.Advance(Tick,new(true,new(50,0)),new(50,50),true);
        s.Cancel();s.Begin(FacingDirection.Left);
        Assert.Equal(0,s.Current.Lift);Assert.Equal(0,s.Current.Age);
        Assert.Equal(FacingDirection.Left,s.Current.Facing);
    }
    [Fact] public void Jump_has_grounded_anticipation_flight_and_grounded_landing()
    {
        var s=new SnackSession();s.Begin(FacingDirection.Left);var airborne=false;var landing=false;
        for(var i=0;i<65;i++)
        {
            var old=s.Current;var p=s.Advance(Tick,new(true,new(60,-200)),new(50,50),true);
            if(p.JumpAge>=0 && p.JumpAge<SnackSession.JumpPrepareSeconds)
            { Assert.Equal(0,p.Lift);Assert.Equal(0,p.DeltaX); }
            else if(p.Lift<0) airborne=true;
            else if(p.JumpAge>=SnackSession.JumpPrepareSeconds+SnackSession.JumpFlightSeconds)
            { landing=true;if(old.JumpAge>=SnackSession.JumpPrepareSeconds+SnackSession.JumpFlightSeconds)Assert.Equal(0,p.DeltaX); }
        }
        Assert.True(airborne);Assert.True(landing);
    }
    [Fact] public void Flight_ignores_cursor_reversal_and_lands_at_the_captured_endpoint()
    {
        var a=new SnackSession();var b=new SnackSession();a.Begin(FacingDirection.Right);b.Begin(FacingDirection.Right);
        var mouth=new PointD(50,50);var launch=new PointerSample(true,new(110,-50));
        a.Advance(Tick,launch,mouth,true);b.Advance(Tick,launch,mouth,true);
        double xa=0,xb=0;
        for(var i=0;i<52;i++)
        {
            var pa=a.Advance(Tick,launch,mouth,true);
            var pb=b.Advance(Tick,new(true,new(i%2==0?-900:900,-900)),mouth,true);
            xa+=pa.DeltaX;xb+=pb.DeltaX;
            Assert.Equal(pa.DeltaX,pb.DeltaX);Assert.Equal(pa.Lift,pb.Lift);
            Assert.Equal(FacingDirection.Right,pb.Facing);
        }
        Assert.InRange(xa,51.99,52.01);Assert.Equal(xa,xb);
    }
    [Fact] public void Flight_arc_meets_ground_with_zero_velocity_and_keeps_peak_height()
    {
        const double epsilon=.0001;
        Assert.InRange(Math.Abs(SnackSession.JumpLift(epsilon,76)/epsilon),0,.01);
        Assert.InRange(Math.Abs(SnackSession.JumpLift(1-epsilon,76)/epsilon),0,.01);
        Assert.Equal(-76,SnackSession.JumpLift(.5,76));
        Assert.Equal(0,SnackSession.JumpLift(0,76));Assert.Equal(0,SnackSession.JumpLift(1,76));
    }
    [Fact] public void Eating_happy_and_settling_ignore_chase_target_and_hold_ownership()
    {
        var s=new SnackSession();s.Begin(FacingDirection.Left);
        while(s.Current.Phase==SnackPhase.Chasing)s.Advance(Tick,new(true,new(50,50)),new(50,50),true);
        var phases=new List<SnackPhase>{s.Current.Phase};
        for(var i=0;i<300&&s.Active;i++)
        {
            var p=s.Advance(Tick,new(true,new(900,-900)),new(50,50),true);
            if(p.Phase!=phases[^1])phases.Add(p.Phase);
            if(!p.Active)break;
            Assert.Equal(0,p.DeltaX);Assert.Equal(0,p.Lift);Assert.Equal(FacingDirection.Left,p.Facing);
            Assert.Equal(-1,p.JumpAge);
        }
        Assert.Equal(new[]{SnackPhase.Eating,SnackPhase.Happy,SnackPhase.Settling,SnackPhase.None},phases);
        Assert.False(s.Active);
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)]
    public void Happy_is_interruptible_and_never_replays_after_cancel(int cancellation)
    {
        var s=new SnackSession();s.Begin(FacingDirection.Right);
        for(var i=0;i<180&&s.Current.Phase!=SnackPhase.Happy;i++)
            s.Advance(Tick,new(true,new(50,50)),new(50,50),true);
        Assert.Equal(SnackPhase.Happy,s.Current.Phase);
        if(cancellation==0)s.Cancel();
        else s.Advance(cancellation==1?Tick:TimeSpan.FromSeconds(2),
            cancellation==1?PointerSample.Unavailable:new(true,new(50,50)),new(50,50),true);
        Assert.False(s.Active);
        s.Begin(FacingDirection.Left);
        Assert.Equal(SnackPhase.Chasing,s.Current.Phase);Assert.Equal(0,s.Current.Age);
    }
}

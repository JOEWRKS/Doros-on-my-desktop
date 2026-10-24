using Dororong.App.Controls;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;
using Dororong.Mac.Rendering;
using Dororong.Portable.Desktop;

namespace Dororong.Mac.Tests;

public class SnackWithdrawalControllerTests
{
    [Theory][InlineData(100,1)][InlineData(125,1)][InlineData(150,1)][InlineData(100,2)][InlineData(150,2)]
    public void MacControllerUsesSameWithdrawalAndRefillPath(int size,double density)
    {
        using var c=new R104DesktopController(new(200,150),new R(),size);c.SetAutomaticSleep(false);c.Density=density;
        var p=(DororongPresenter)typeof(R104DesktopController).GetField("presenter",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!.GetValue(c)!;
        var scene=new SceneSnapshot(new(1,TimeSpan.Zero,[new(1,new(0,0,1470,956))],[]),SceneHealth.Fresh);
        var errors=new List<Exception>();c.Faulted+=errors.Add;
        void Step(PointerSample pointer)=>c.Advance(TimeSpan.FromMilliseconds(16),pointer,false,scene);
        for(var i=0;i<150;i++)Step(PointerSample.Unavailable);
        c.BeginSnack();
        for(var cycle=0;cycle<6;cycle++){
            for(var i=0;i<18;i++)Step(new(true,c.Position+p.GetSnackMouth()));
            Assert.Equal(SnackPhase.Chasing,c.Snack);
            var target=c.Position+p.GetSnackMouth()+new PointD(cycle==5?90:180,cycle==5?-180:0);
            // Autonomous walking may put the pet at the right edge before the
            // sixth offer. A real cursor on this single-monitor scene cannot
            // be at x1503 beyond its1470px edge; keep the catch target on-screen.
            if(cycle==5)target=target with{X=Math.Clamp(target.X,80,1390)};
            var away=new PointerSample(true,target);Step(away);
            if(cycle==5){
                Assert.Equal(SnackPhase.Snatching,c.Snack);
                for(var i=0;i<30&&c.Snack==SnackPhase.Snatching;i++)Step(away);
                Assert.True(c.Snack==SnackPhase.Eating,$"phase={c.Snack} pos={c.Position} mouth={c.Position+p.GetSnackMouth()} target={away.Position} platform={System.Text.Json.JsonSerializer.Serialize(c.Platform)}");var error=c.Position+p.GetSnackMouth()-away.Position;
                Assert.InRange(Math.Sqrt(error.X*error.X+error.Y*error.Y),0,8*c.Scale);
                break;
            }
            Assert.Equal(SnackPhase.Angry,c.Snack);Assert.True(p.Face.Angry);var pos=c.Position;
            Assert.False(p.Face.Closed);Assert.Equal(cycle>=2,p.Face.Furious);
            var maxLift=0d;var maxReach=0d;var maxCrouch=0d;
            for(var i=0;i<40;i++){
                Step(away);Assert.Equal(pos,c.Position);Assert.Equal(SnackPhase.Angry,c.Snack);
                maxLift=Math.Max(maxLift,p.Face.StompLift);
                maxReach=Math.Max(maxReach,p.Face.StompReach);maxCrouch=Math.Max(maxCrouch,p.Face.StompCrouch);
                if(cycle>=1)Assert.Equal(HighResolutionGrabRaster.StompWarp.Head(p.Face.StompCrouch),p.Face.Head);
                else Assert.Equal(0,p.Face.StompLift);
            }
            if(cycle>=1){Assert.True(maxLift>(cycle>=2?4.6:2.3));Assert.True(maxReach>(cycle>=2?15.6:3.9));Assert.True(maxCrouch>.32);}
            for(var i=0;i<150&&c.Snack==SnackPhase.Angry;i++)Step(away);
            Assert.Equal(SnackPhase.Chasing,c.Snack);Assert.False(p.Face.Angry);
            Assert.Equal(0,p.Face.StompLift);
        }
        for(var i=0;i<80&&c.Snack!=SnackPhase.Eating;i++)Step(new(true,c.Position+p.GetSnackMouth()));
        Assert.Equal(SnackPhase.Eating,c.Snack);Assert.False(p.Face.Angry);
        c.CancelSnack();Step(PointerSample.Unavailable);Assert.Equal(SnackPhase.None,c.Snack);Assert.Empty(errors);
    }
    private sealed class R:IRandomSource{public double NextUnit()=>.99;}
}

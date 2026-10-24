using System.Runtime.InteropServices;
using Dororong.Mac.Native;

namespace Dororong.Mac.Tests;

public sealed class OverlayFramePolicyTests
{
    [Fact] public void OnlyExplicitOwnedWindowsChangeAndRegistrationIsBounded()
    {
        var runtime=new Runtime();var policy=new OverlayFramePolicy(runtime);
        policy.Apply(10);policy.Apply(11);
        for(var i=0;i<1000;i++){policy.Apply(10);policy.Apply(11);}
        Assert.Equal(1,runtime.Created);Assert.Equal(2,runtime.Sets);
        Assert.Equal(101,runtime.GetClass(10));Assert.Equal(101,runtime.GetClass(11));
        Assert.Equal(1,runtime.GetClass(12)); // menu / unrelated window untouched
    }
    [Fact] public void UnknownClassesAndNullWindowsFailBeforeMutation()
    {
        var runtime=new Runtime();runtime.Classes[10]=2;
        var policy=new OverlayFramePolicy(runtime);
        Assert.Throws<ArgumentException>(()=>policy.Apply(0));
        Assert.Throws<InvalidOperationException>(()=>policy.Apply(10));
        Assert.Equal(0,runtime.Created);Assert.Equal(0,runtime.Sets);
    }
    [Fact] public void CallbackHasStableBlittableRectLayoutAndRoot()
    {
        Assert.Equal(32,Marshal.SizeOf<OverlayFramePolicy.Frame>());
        Assert.Same(OverlayFramePolicy.Callback,OverlayFramePolicy.Callback);
        var pointer=Marshal.GetFunctionPointerForDelegate(OverlayFramePolicy.Callback);
        Assert.NotEqual(0,pointer);
        var callback=Marshal.GetDelegateForFunctionPointer<OverlayFramePolicy.Constraint>(pointer);
        var rect=new OverlayFramePolicy.Frame(-1920,1108.5,408,408);
        Assert.Equal(rect,callback(10,20,rect,30));
    }
    public static IEnumerable<object[]> Positions()=>
        from top in new[]{-152.5,-90.377905025431,-39.651092469904,16.317104690361,27.221530259969,33,346.973473756289,749}
        from height in new[]{956d,1080d} from origin in new[]{-1920d,0d,1470d}
        from extent in new[]{408d,32d}
        select new object[]{top,height,origin,extent};
    [Theory][MemberData(nameof(Positions))]
    public void RecordedTopAndReleaseCoordinatesSurviveConstraintCallback(double top,double primaryHeight,double x,double extent)
    {
        var requested=new OverlayFramePolicy.Frame(x,primaryHeight-top-extent,extent,extent);
        // Native constraint dispatch modeled by the same callback installed in
        // production, not an assertion that AppKit was executed on this host.
        var result=OverlayFramePolicy.Callback(10,20,requested,30);
        Assert.Equal(requested,result);
        Assert.Equal(top,primaryHeight-result.Y-result.Height,8);
        if(top<33)Assert.NotEqual(33,primaryHeight-result.Y-result.Height);
    }
    private sealed class Runtime:OverlayFramePolicy.IRuntime
    {
        internal readonly Dictionary<nint,nint> Classes=new(){[10]=1,[11]=1,[12]=1};
        internal int Created,Sets;
        public nint GetClass(nint window)=>Classes[window];
        public string Name(nint type)=>type==1?"AvnWindow":"Unexpected";
        public nint CreateSubclass(nint original,OverlayFramePolicy.Constraint callback){Created++;return 101;}
        public void SetClass(nint window,nint type){Sets++;Classes[window]=type;}
    }
}

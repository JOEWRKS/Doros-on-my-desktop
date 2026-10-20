using Dororong.App.Runtime;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;

namespace Dororong.App.Tests.Runtime;

public sealed class ImmutableSnapshotTests
{
    private sealed class Native:IDesktopSceneNative{
        internal DesktopWindow[] Windows=[new(new(1,2,3),new(0,100,500,300),0,true,false,false,false,true,true,false,false)];
        internal DesktopMonitor[] Monitors=[new(1,new(0,0,1920,1080))];
        internal bool Fail;
        public DesktopScene? Capture(TimeSpan now)=>Fail?null:new((long)now.TotalMilliseconds,now,Monitors,Windows);
    }
    [Fact]
    public void Snapshots_share_only_owned_equal_geometry_keep_fresh_time_and_expire_on_failure(){
        var native=new Native();var source=new DesktopSceneSource(native);
        var a=source.Read(TimeSpan.Zero).Scene!;
        native.Windows[0]=native.Windows[0] with{};
        var b=source.Read(TimeSpan.FromMilliseconds(80)).Scene!;
        Assert.Same(a.Windows,b.Windows);Assert.Same(a.Monitors,b.Monitors);
        Assert.Equal(TimeSpan.FromMilliseconds(80),b.CapturedAt);Assert.Equal(80,b.Revision);
        native.Windows[0]=native.Windows[0] with{Bounds=new(10,200,600,300)};
        var c=source.Read(TimeSpan.FromMilliseconds(160)).Scene!;
        Assert.NotSame(b.Windows,c.Windows);Assert.Equal(0,a.Windows[0].Bounds.X);Assert.Equal(10,c.Windows[0].Bounds.X);
        native.Monitors[0]=new(2,new(-1000,0,1000,800));
        var d=source.Read(TimeSpan.FromMilliseconds(240)).Scene!;
        Assert.NotSame(c.Monitors,d.Monitors);Assert.Equal(1,a.Monitors[0].Id);
        native.Fail=true;Assert.Equal(SceneReadHealth.Expired,source.Read(TimeSpan.FromMilliseconds(800)).Health);
    }
    [Fact]
    public void Every_geometry_flag_identity_and_order_change_invalidates_reuse(){
        var a=new DesktopWindow(new(1,2,3),new(0,100,500,300),0,true,false,false,false,true,true,false,false);
        IReadOnlyList<DesktopWindow> previous=ImmutableSnapshotCollection.CopyOrReuse(new[]{a},null);
        foreach(var b in new[]{a with{Key=new(1,2,4)},a with{ZOrder=1},a with{Visible=false},a with{Minimized=true},
            a with{Cloaked=true},a with{Excluded=true},a with{CanSupport=false},a with{CanOcclude=false},
            a with{Taskbar=true},a with{HorizontalTaskbar=true},a with{IsTransientMenu=true},a with{Bounds=new(0,101,500,300)}})
            Assert.NotSame(previous,ImmutableSnapshotCollection.CopyOrReuse(new[]{b},previous));
        var pair=ImmutableSnapshotCollection.CopyOrReuse(new[]{a,a with{Key=new(9,2,3)}},null);
        Assert.NotSame(pair,ImmutableSnapshotCollection.CopyOrReuse(new[]{pair[1],pair[0]},pair));
        Assert.Empty(ImmutableSnapshotCollection.CopyOrReuse(Array.Empty<DesktopWindow>(),pair));
    }
}

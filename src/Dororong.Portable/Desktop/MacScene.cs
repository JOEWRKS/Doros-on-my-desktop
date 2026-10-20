using Dororong.Core.Geometry;
using Dororong.Core.Platforms;

namespace Dororong.Portable.Desktop;

// Quartz global display POINTS, top-left origin. Never multiply this world by
// the primary monitor's Retina scale. Backing scale belongs to rendering only.
public sealed record MacWindowRow(long Id,uint Pid,RectD Bounds,int Layer,double Alpha,bool OnScreen,bool Dock);
public sealed class MacSceneMapper
{
    private readonly Dictionary<(long,uint),long> generations=new();
    private long generation,revision;
    public DesktopScene Build(TimeSpan now,IReadOnlyList<DesktopMonitor> monitors,IReadOnlyList<MacWindowRow> rows,uint ownPid,int dockLevel,int menuLevel)
    {
        if(monitors.Count==0||monitors.Any(m=>!Valid(m.Bounds)))throw new InvalidDataException("Invalid display geometry.");
        var present=new HashSet<(long,uint)>();var windows=new List<DesktopWindow>();
        for(var z=0;z<rows.Count;z++)
        {
            var row=rows[z];
            if(row.Id<=0||row.Pid==ownPid||!row.OnScreen||!Valid(row.Bounds)||!double.IsFinite(row.Alpha)||row.Alpha<=.01)continue;
            var identity=(row.Id,row.Pid);present.Add(identity);
            if(!generations.TryGetValue(identity,out var age))generations[identity]=age=++generation;
            // Dock also owns desktop/Mission Control windows. Only a visible
            // bounded shelf adjacent to a display edge can be a Dock surface.
            var horizontal=row.Dock&&row.Layer==dockLevel&&monitors.Any(m=>
                row.Bounds.Width>row.Bounds.Height*2&&row.Bounds.Height>=8&&row.Bounds.Height<=m.Bounds.Height*.25&&
                row.Bounds.Bottom>=m.Bounds.Bottom-20&&row.Bounds.Y<m.Bounds.Bottom-7&&
                row.Bounds.Right>m.Bounds.X&&row.Bounds.X<m.Bounds.Right);
            var side=row.Dock&&row.Layer==dockLevel&&monitors.Any(m=>
                row.Bounds.Height>row.Bounds.Width*2&&row.Bounds.Width>=8&&row.Bounds.Width<=m.Bounds.Width*.25&&
                (Math.Abs(row.Bounds.X-m.Bounds.X)<=20||Math.Abs(row.Bounds.Right-m.Bounds.Right)<=20));
            var dock=horizontal||side;
            var menu=row.Layer>=menuLevel;
            // Mac policy: walk on the screen floor, visually in front of Dock.
            // Dock still occludes ordinary ledges behind it, but never owns a
            // support/perch. Menus/overlays therefore cannot delete our ground.
            // Ordinary layer-zero app windows retain support and occlusion.
            var support=!row.Dock&&row.Layer==0;
            var occlude=dock||!row.Dock&&row.Layer>=0;
            windows.Add(new(new(row.Id,row.Pid,age),row.Bounds,z,true,false,false,false,support,occlude,dock,horizontal)
                {IsTransientMenu=menu});
        }
        foreach(var key in generations.Keys.Where(k=>!present.Contains(k)).ToArray())generations.Remove(key);
        return new(++revision,now,monitors.ToArray(),windows);
    }
    public static bool Valid(RectD r)=>double.IsFinite(r.X)&&double.IsFinite(r.Y)&&double.IsFinite(r.Width)&&double.IsFinite(r.Height)&&
        double.IsFinite(r.Right)&&double.IsFinite(r.Bottom)&&r.Width>0&&r.Height>0;
    public static PointD ToCocoaTopLeft(PointD quartz,double primaryHeight)=>new(quartz.X,primaryHeight-quartz.Y);
}

public enum SceneHealth { Fresh,Cached,Unavailable,FloorFallback }
public sealed record SceneSnapshot(DesktopScene Scene,SceneHealth Health)
{
    public bool Reliable=>Health!=SceneHealth.Unavailable;
    public bool CanAcquireWindow=>Health is SceneHealth.Fresh or SceneHealth.Cached;
}

// Same80ms cadence/500ms stale-scene boundary as the Windows scene source.
// Null means a failed query; an empty successful scene means windows really left.
public sealed class SceneCache(bool background=false)
{
    private DesktopScene? scene;
    private TimeSpan? attempt;
    private TimeSpan success,highWater;
    private bool failed;
    private Task<DesktopScene?>? pending;
    public SceneSnapshot Read(TimeSpan now,Func<TimeSpan,DesktopScene?> capture,IReadOnlyList<DesktopMonitor> fallback)
    {
        now=highWater=now>highWater?now:highWater;var fresh=false;
        void Accept(DesktopScene? next)
        {
            failed=next is null;
            if(next is not null){scene=next;success=next.CapturedAt;fresh=true;}
        }
        DesktopScene? Capture(TimeSpan time){try{return capture(time);}catch{return null;}}
        if(pending is {IsCompleted:true}){Accept(pending.GetAwaiter().GetResult());pending=null;}
        if(pending is null&&(attempt is null||now-attempt.Value>=TimeSpan.FromMilliseconds(80)))
        {
            var initial=attempt is null;
            attempt=now;DesktopScene? next;
            if(background&&!initial){var requested=now;pending=Task.Run(()=>Capture(requested));}
            else{next=Capture(now);Accept(next);}
        }
        if(scene is null||now-success>=TimeSpan.FromMilliseconds(500))
        {
            var monitors=fallback.Count>0?fallback:scene?.Monitors??[];
            return new(new(0,now,monitors,[]),SceneHealth.FloorFallback);
        }
        return new(scene,fresh?SceneHealth.Fresh:failed?SceneHealth.Unavailable:SceneHealth.Cached);
    }
}

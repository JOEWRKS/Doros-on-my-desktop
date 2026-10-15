using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;
using Dororong.Portable.Rendering;

namespace Dororong.Portable.Desktop;

// Desktop integration preview. Reuses the real Windows physics engines rather
// than a cursor-following position servo or a second platform/perch algorithm.
public sealed class DesktopPetController
{
    private readonly PetBrain brain;
    private readonly PlatformMotion platform=new();
    private readonly EdgePerchRuntime perch;
    private readonly LocomotionPresentation locomotion=new();
    private readonly HuntingPresentation hunting=new();
    private readonly HeadDragPresentation head=new();
    private readonly RestPresentation rest=new();
    private readonly CheekPresentation cheek=new();
    private readonly PawPresentation paw=new();
    private readonly ClickPresentation click=new();
    private RasterImage? landingSource;private Affine landingPlacement;
    private double pendingHeadSole;
    public bool ClickReacting=>click.Active&&!click.Holding;
    private RasterImage nativeFrame=MotionRenderer.Standing();
    private Affine placement=new(1,0,0,1,24,24);
    private bool clickHeld;
    private readonly SnackSession snack=new();
    private readonly SnackRenderer snackRenderer=new();
    private readonly PerchExpressionMotion expression=new();
    private PointD? grab;
    private double flutterAge,restoreAge=.12;
    private RasterImage? oldPerch;
    private bool lastPerch;
    private double seatedClock;
    public PointD Position {get;private set;}
    public FacingDirection Facing {get;private set;}=FacingDirection.Right;
    public RasterImage Frame {get;private set;}
    public RasterImage ViewportFrame=>head.Active&&head.ViewportFrame is {} wide?wide:DesktopFrames.Pad(Frame);
    public bool Capturing=>Dragging||cheek.Active&&!cheek.Releasing||paw.Current is not null&&!paw.Releasing||clickHeld;
    public bool CheekActive=>cheek.Active;
    public double CheekPull=>cheek.Pull;
    public PerchPawSnapshot? Paw=>paw.Current;
    public PlatformPose Platform=>platform.Current;
    public EdgePerchPhase Perch=>perch.Current.Phase;
    public SnackPhase Snack=>snack.Current.Phase;
    public bool Dragging=>grab.HasValue;
    public bool Ready {get;private set;}
    public bool MenuOpen {get;set;}
    public bool Sitting {get;private set;}
    public bool AutomaticSleep=>brain.AutomaticSleepEnabled;
    public PetState State=>brain.Current.State;
    public PouncePose Pounce=>hunting.Pounce;
    public bool Preparing=>hunting.Preparing;
    public bool HeadHeld=>head.Holding;
    public double HeadStrength=>head.Strength;
    public double HeadAngle=>head.Angle;
    public void ToggleSit(){Sitting=!Sitting;if(hunting.Pounce.OffsetY<0||click.Lift<0)platform.Reset(Position);click.Reset();hunting.Reset();brain.ResumeAfterPlay(Facing);}
    public void ToggleSleep(){if(brain.AutomaticSleepEnabled)rest.PrepareWake();brain.SetAutomaticSleepEnabled(!brain.AutomaticSleepEnabled);}
    public void SetAutomaticSleep(bool enabled){if(AutomaticSleep!=enabled)ToggleSleep();}
    public DesktopPetController(PointD initial,IRandomSource random)
    {
        Position=initial;brain=new(BehaviorTuning.Default,random,initial);
        Frame=DesktopFrames.Place(MotionRenderer.Standing(),false);
        perch=new(facing=>DesktopFrames.Grip(facing==FacingDirection.Left),()=>DesktopFrames.Measure(Frame).Contact.SoleY);
        platform.Reset(initial);
    }
    public void BeginSnack()
    {
        click.Reset();landingSource=null;
        grab=null;head.Reset();cheek.Reset();paw.Reset();clickHeld=false;Sitting=false;perch.Release(Position);Ready=false;hunting.Reset();
        ResetInteractionClock();snack.Begin(Facing);snackRenderer.Render(snack.Current);
    }
    public void CancelSnack(){if(snack.Current.Lift<0)platform.Reset(Position);snack.Cancel();hunting.Reset();}
    public bool TryBeginHeadDrag(PointD pointer)
    {
        if(Classify(pointer)!=DirectInteractionTarget.Body)return false;
        var hanging=Perch!=EdgePerchPhase.None;var visible=Frame;
        pendingHeadSole=DesktopFrames.Measure(visible).Contact.SoleY;
        var asleep=State==PetState.Sleep;
        var source=hanging?DesktopFrames.PerchSource(expression.EyesClosed):nativeFrame;var map=placement;
        BeginDrag(pointer);head.Begin(pointer,Position,visible,Facing,hanging,source,map);
        if(!hanging)click.Begin(pointer,true,asleep);return true;
    }
    internal DirectInteractionTarget Classify(PointD pointer)
    {
        if(!Finite(pointer)||!Hit(pointer)||head.Active||cheek.Active||Snack!=SnackPhase.None)return DirectInteractionTarget.None;
        var local=placement.Inverse().Transform(pointer-Position);
        if(landingSource is not null)return InteractionHitMap.IsUpperHead(local)?DirectInteractionTarget.Body:DirectInteractionTarget.ClickOnly;
        if(Perch!=EdgePerchPhase.None)
        {
            if(PerchPawRenderer.HitTest(DesktopFrames.PerchSource(false),paw.Current,local) is {} right)
                return right?DirectInteractionTarget.PerchRightPaw:DirectInteractionTarget.PerchLeftPaw;
            local+=new PointD(-8,6);
            return InteractionHitMap.IsSelectedCheek(local)?DirectInteractionTarget.RightCheek:
                InteractionHitMap.IsUpperHead(local)?DirectInteractionTarget.Body:DirectInteractionTarget.ClickOnly;
        }
        if(!InteractionHitMap.InSource(local))return DirectInteractionTarget.None;
        var descriptor=click.Active?click.Descriptor:rest.Descriptor;
        if(State==PetState.Sleep||descriptor!=FrameInteractionDescriptor.Canonical)
            return descriptor.Classify(local,FacingDirection.Right,true)==DirectInteractionTarget.LeftCheek?DirectInteractionTarget.LeftCheek:
                descriptor.IsHeadOrCheek(local)?DirectInteractionTarget.Body:DirectInteractionTarget.ClickOnly;
        if(hunting.Rendered)
        {
            var h=hunting.HeadTransform.Inverse().Transform(local);
            if(InteractionHitMap.InSource(h)&&HuntFrames.Instance.Head(rest.Closed?1:0).Span[((int)h.Y*96+(int)h.X)*4+3]>0)
                return InteractionHitMap.IsSelectedCheek(h)?DirectInteractionTarget.RightCheek:
                    InteractionHitMap.IsUpperHead(h)?DirectInteractionTarget.Body:
                    FrameInteractionDescriptor.Canonical.Classify(h,FacingDirection.Right,true)==DirectInteractionTarget.LeftCheek?DirectInteractionTarget.LeftCheek:DirectInteractionTarget.None;
        }
        if(InteractionHitMap.IsSelectedCheek(local))return DirectInteractionTarget.RightCheek;
        if(InteractionHitMap.PickBody(local,nativeFrame.Pixels.Span)!=BodyRegion.None)return DirectInteractionTarget.ClickOnly;
        if(FrameInteractionDescriptor.Canonical.Classify(local,FacingDirection.Right,true)==DirectInteractionTarget.LeftCheek)return DirectInteractionTarget.LeftCheek;
        return InteractionHitMap.IsUpperHead(local)?DirectInteractionTarget.Body:DirectInteractionTarget.None;
    }
    public bool TryBeginInteraction(PointD pointer)
    {
        if(MenuOpen)return false;
        var target=Classify(pointer);
        if(target!=DirectInteractionTarget.None){if(click.Lift<0)platform.Reset(Position);click.Reset();}
        if(target==DirectInteractionTarget.Body)return TryBeginHeadDrag(pointer);
        if(target is DirectInteractionTarget.PerchLeftPaw or DirectInteractionTarget.PerchRightPaw)
        {cheek.Reset();paw.Begin(target==DirectInteractionTarget.PerchRightPaw,pointer,Facing==FacingDirection.Left);return true;}
        if(target is DirectInteractionTarget.ClickOnly or DirectInteractionTarget.LeftCheek)
        {clickHeld=true;if(target==DirectInteractionTarget.ClickOnly&&Perch==EdgePerchPhase.None)click.Begin(pointer,false,State==PetState.Sleep);ResetInteractionClock();hunting.Reset();return true;}
        if(target!=DirectInteractionTarget.RightCheek)return false;
        var attached=Perch!=EdgePerchPhase.None;
        var source=nativeFrame;var map=placement;var axis=new PointD(1,0);
        Func<double,double,double,RasterImage>? render=null;
        if(attached)
        {
            var data=new byte[36864];var raw=DesktopFrames.PerchSource(false).Pixels.Span;
            for(var y=0;y<90;y++)for(var x=8;x<100;x++)raw.Slice((y*100+x)*4,4).CopyTo(data.AsSpan(((y+6)*96+x-8)*4,4));
            source=new(96,96,data);map=new Affine(1,0,0,1,8,-6).Then(placement);
        }
        else if(hunting.Rendered){render=hunting.CaptureCheek();if(render is not null)axis=new(hunting.HeadTransform.M11,hunting.HeadTransform.M12);}
        cheek.Begin(pointer,source,map,axis,attached,Sitting,render);paw.Reset();
        ResetInteractionClock();hunting.Reset();return true;
    }
    public void BeginDrag(PointD pointer)
    {
        if(!Finite(pointer))return;
        click.Reset();landingSource=null;
        CancelSnack();head.Reset();cheek.Reset();paw.Reset();clickHeld=false;ResetInteractionClock();Sitting=false;perch.Release(Position);platform.Reset(Position);grab=pointer-Position;Ready=false;
    }
    public void Drop(){click.Reset();landingSource=null;grab=null;head.Reset();cheek.Reset();paw.Reset();clickHeld=false;hunting.Reset();perch.Release(Position);platform.Reset(Position);Ready=false;}
    public bool Hit(PointD pointer)
    {
        var wide=head.Active&&head.ViewportFrame is not null;
        var frame=wide?head.ViewportFrame!:Frame;var p=pointer-Position+(wide?new PointD(96,96):default);
        if(!Finite(p)||p.X<0||p.Y<0||p.X>=frame.PixelWidth||p.Y>=frame.PixelHeight)return false;
        var x=(int)Math.Floor(p.X);var y=(int)Math.Floor(p.Y);
        return frame.Pixels.Span[(y*frame.PixelWidth+x)*4+3]>=16;
    }
    public void Advance(TimeSpan delta,PointerSample pointer,bool primaryDown,SceneSnapshot snapshot)
    {
        var scene=snapshot.Scene;
        if(scene.Monitors.Count==0||scene.Monitors.Any(m=>!MacSceneMapper.Valid(m.Bounds)))return;
        if(!double.IsFinite(delta.TotalSeconds)||delta.TotalSeconds<0)throw new ArgumentOutOfRangeException(nameof(delta));
        if(delta.TotalSeconds>1||!pointer.IsAvailable||!Finite(pointer.Position))
        {CancelSnack();click.Reset();landingSource=null;grab=null;head.Reset();cheek.Reset();paw.Reset();clickHeld=false;Ready=false;hunting.Reset();delta=TimeSpan.Zero;}
        var dt=Math.Min(delta.TotalSeconds,.05);delta=TimeSpan.FromSeconds(dt);
        if(MenuOpen)click.Reset();
        click.Advance(dt*1000,pointer.Position,primaryDown);
        if(!primaryDown)clickHeld=false;
        var hadCheek=cheek.Active;
        if(hadCheek)cheek.Advance(dt*1000,pointer.Position,primaryDown&&!MenuOpen);
        if(paw.Current is not null)paw.Advance(dt*1000,pointer.Position,primaryDown&&!MenuOpen);
        var geometry=MeasureVisible();
        var monitor=scene.Monitors.OrderBy(m=>Distance(m.Bounds,Dragging&&pointer.IsAvailable?pointer.Position:Position+new PointD(72,72))).First();
        var bounds=monitor.Bounds;
        PointD Clamp(PointD p)=>new(Math.Clamp(p.X,bounds.X-geometry.Bounds.X,Math.Max(bounds.X-geometry.Bounds.X,bounds.Right-geometry.Bounds.Right)),
            Math.Clamp(p.Y,bounds.Y-geometry.Bounds.Y,Math.Max(bounds.Y-geometry.Bounds.Y,bounds.Bottom-geometry.Contact.SoleY)));
        var corrected=Clamp(Position);
        if(Math.Abs(corrected.X-Position.X)>.001||Math.Abs(corrected.Y-Position.Y)>.001)
        {Position=corrected;perch.Release(Position);platform.Reset(Position);}
        var surfaces=PlatformGeometry.Build(scene,0);
        var z=scene.Windows.ToDictionary(w=>w.Key,w=>w.ZOrder);
        var perchSurfaces=surfaces.Select(s=>new PerchSurface(s,z.GetValueOrDefault(s.Key,int.MaxValue))).ToArray();
        var releasedHeadThisTick=false;
        if(grab is { } offset)
        {
            if(head.Active)head.Advance(delta,pointer.Position,pointer.IsAvailable,primaryDown);
            if(head.Active&&head.Phase!=DirectInteractionPhase.BodyPending)click.Reset();
            releasedHeadThisTick=head.Releasing;
            if(primaryDown&&(!head.Active||head.Phase!=DirectInteractionPhase.BodyPending))Position=Clamp(pointer.Position-offset);
            if(primaryDown)
            {
                Ready=snapshot.CanAcquireWindow&&perch.CanBegin(Position,Facing,perchSurfaces,scene.Monitors,advertise:true);
                flutterAge=Ready?(flutterAge+dt)% .25:0;
                var source=MotionRenderer.Standing();if(Ready)source=DesktopFrames.Flutter(source,flutterAge/.25);
                Frame=head.Active?head.Render(Ready?flutterAge/.25:null):DesktopFrames.Place(source,Facing==FacingDirection.Left);
                if(click.Active&&head.Phase==DirectInteractionPhase.BodyPending)
                {
                    var pressed=click.Render(dt*1000,out var t);var pose=t.Then(new(Facing==FacingDirection.Left?-1:1,0,0,1,Facing==FacingDirection.Left?120:24,24));
                    Frame=DesktopFrames.Warp(pressed,pose);var correction=pendingHeadSole-DesktopFrames.Measure(Frame).Contact.SoleY;
                    Frame=DesktopFrames.Warp(pressed,pose.Then(new(1,0,0,1,0,correction)));head.SetPendingFrame(Frame);
                }
                brain.ApplyPlatformPosition(Position);platform.Reset(Position);return;
            }
            grab=null;Ready=false;
            if(!click.Active&&perch.TryBegin(Position,Facing,perchSurfaces,scene.Monitors,true,pointer.IsAvailable,snapshot.CanAcquireWindow))head.Reset();
        }
        if(Perch!=EdgePerchPhase.None)
        {
            var p=perch.Advance(delta,Position,perchSurfaces,scene.Monitors,snapshot.Reliable);
            if(p.Phase!=EdgePerchPhase.None)
            {
                if(!lastPerch)expression.Reset();expression.Advance(delta);lastPerch=true;
                Position=p.Position;
                placement=new Affine(Facing==FacingDirection.Left?-1:1,0,0,1,Facing==FacingDirection.Left?120:16,30)
                    .Then(new(1,0,0,expression.ScaleY,0,94*(1-expression.ScaleY)));
                Frame=cheek.Active?cheek.Render():DesktopFrames.Perch(Facing==FacingDirection.Left,expression.EyesClosed,expression.ScaleY,paw.Current);oldPerch=Frame;
                brain.ApplyPlatformPosition(Position);platform.Reset(Position);return;
            }
            Position=p.Position;platform.Reset(Position);cheek.Reset();paw.Reset();hadCheek=false;
        }
        if(lastPerch){lastPerch=false;restoreAge=0;}
        var before=Position;var proposed=Position;RasterImage image;double lift=0;var headRelease=head.Releasing;var clickTransform=Affine.Identity;
        if(click.Active)
        {image=click.Render(dt*1000,out clickTransform);lift=click.Lift;hunting.Reset();}
        else if(cheek.Active||hadCheek)
        {
            image=nativeFrame;proposed=Position+new PointD(cheek.DeltaX,0);lift=cheek.Lift;
            if(cheek.Active)Facing=cheek.Flipped?FacingDirection.Left:FacingDirection.Right;
            hunting.Reset();
        }
        else if(headRelease)
        {
            if(!releasedHeadThisTick)head.Advance(delta,pointer.Position,pointer.IsAvailable,false);
            image=MotionRenderer.Standing();hunting.Reset();
        }
        else if(snack.Active)
        {
            var mouth=snackRenderer.HeadTransform.Transform(new(29,60));
            mouth=Position+new PointD(24+(Facing==FacingDirection.Left?96-mouth.X:mouth.X),24+mouth.Y);
            var pose=snack.Advance(delta,pointer,mouth,platform.Current.Phase==PlatformPhase.Supported);
            if(pose.Active){Facing=pose.Facing;proposed=Position+new PointD(pose.DeltaX,0);lift=pose.Lift;}
            image=snackRenderer.Render(pose);hunting.Reset();
        }
        else
        {
            var relative=pointer.IsAvailable?pointer.Position-Position:(PointD?)null;
            var canTrack=platform.Current.Phase==PlatformPhase.Supported&&!MenuOpen&&!Sitting&&!primaryDown&&!clickHeld&&snapshot.Reliable;
            var previousJump=hunting.Pounce;
            hunting.Advance(dt,relative,Facing,!canTrack);
            if(previousJump.OffsetY<0&&!hunting.Pounce.IsJump)platform.Reset(Position);
            var watching=hunting.Watching;
            brain.ApplyPlatformPosition(Position);
            var area=new RectD(bounds.X-geometry.Bounds.X,bounds.Y-geometry.Bounds.Y,
                bounds.Width-geometry.Bounds.Width+144,bounds.Height-geometry.Bounds.Height+144);
            var current=brain.Update(new(delta,area,new(144,144),pointer,false,false,null,new(4,4),
                SuspendAutonomousMotion:!snapshot.Reliable||MenuOpen||watching||Sitting||clickHeld||locomotion.HoldWalking||platform.Current.Phase!=PlatformPhase.Supported,
                SurfaceBoundMotion:true,SuppressPointerReactions:true,TrackPointerFacing:watching,
                TrackingFacingOverride:hunting.Pounce.IsJump?hunting.Pounce.Direction:null));
            Facing=current.Facing;proposed=current.Position;
            if(hunting.Pounce.IsJump){proposed+=new PointD(hunting.Pounce.DeltaX,0);lift=hunting.Pounce.OffsetY;}
            var moving=Math.Abs(proposed.X-before.X)>1e-7;
            var allowed=platform.Current.Phase==PlatformPhase.Supported&&current.State is PetState.Idle or PetState.Walk;
            locomotion.Advance(dt*1000,moving&&!hunting.Preparing,proposed.X-before.X,!allowed,Sitting);
            var walk=moving?Math.Max(1d/8,locomotion.Walk):locomotion.Walk;
            if(Sitting){seatedClock=(seatedClock+dt*1000)%3200;current=current with{State=PetState.Idle,Phase=seatedClock/3200};}
            image=rest.Render(current,dt*1000,allowed,locomotion.Sit,locomotion.UsesRisePose,walk,locomotion.Distance);
            if(canTrack&&current.State!=PetState.Sleep)image=hunting.Render(image,Facing,rest.Closed,rest.Breathing,moving&&!watching?image:null);
        }
        nativeFrame=image;
        placement=new(Facing==FacingDirection.Left?-1:1,0,0,1,Facing==FacingDirection.Left?120:24,24);
        Frame=DesktopFrames.Place(image,Facing==FacingDirection.Left);
        if(click.Active){placement=clickTransform.Then(placement);Frame=DesktopFrames.Warp(image,placement);}
        if(cheek.Active)Frame=cheek.Render();
        if(head.Active&&headRelease)Frame=head.Render();
        else if(!cheek.Active){placement=placement.Then(hunting.JumpTransform(Facing==FacingDirection.Left));Frame=hunting.TransformJump(Frame,Facing==FacingDirection.Left);}
        geometry=MeasureVisible();
        var supports=PlatformGeometry.Build(scene,geometry.Contact.SoleY-geometry.Contact.VisibleTop);
        var motion=platform.Advance(new(delta,Position,proposed,geometry.Contact,supports,false,snapshot.Reliable)
            {Monitors=scene.Monitors,SupportedOffsetY=lift,SupportedLiftLimit=96});
        Position=motion.Position;brain.ApplyPlatformPosition(Position);
        if(motion.IsExtremeLanding&&!click.Active&&!cheek.Active&&!head.Active)
        {
            if(landingSource is null){landingSource=nativeFrame;landingPlacement=placement;}
            nativeFrame=landingSource;placement=landingPlacement;
            Frame=DesktopFrames.Warp(new RasterImage(160,160,ExtremeLandingFrame.Spread(landingSource.CopyPixels(),motion.LegSpread)),new Affine(1,0,0,1,-32,-32).Then(placement));
        }
        else landingSource=null;
        if(head.Active&&head.ViewportFrame is not null){head.ApplyLanding(motion);Frame=DesktopFrames.CropViewport(head.ViewportFrame!);}
        else {placement=placement.Then(DesktopFrames.LandingTransform(Frame,motion));Frame=DesktopFrames.Land(Frame,motion);}
        if(restoreAge<.12&&oldPerch is not null)
        {
            restoreAge=Math.Min(.12,restoreAge+dt);var boundary=(int)Math.Ceiling(48+70*restoreAge/.12);
            var bytes=Frame.CopyPixels();oldPerch.Pixels.Span.Slice(boundary*576).CopyTo(bytes.AsSpan(boundary*576));Frame=new(144,144,bytes);
        }
    }
    private static double Distance(RectD r,PointD p){var x=Math.Max(r.X-p.X,Math.Max(0,p.X-r.Right));var y=Math.Max(r.Y-p.Y,Math.Max(0,p.Y-r.Bottom));return x*x+y*y;}
    private (FootContact Contact,RectD Bounds) MeasureVisible()
    {
        if(!head.Active||head.ViewportFrame is not {} frame)return DesktopFrames.Measure(Frame);
        var (contact,bounds)=DesktopFrames.Measure(frame);
        return (new(contact.Left-96,contact.Right-96,contact.SoleY-96,contact.VisibleTop-96),new(bounds.X-96,bounds.Y-96,bounds.Width,bounds.Height));
    }
    private void ResetInteractionClock()
    {
        // DirectInteraction normally resets Core inactivity each held tick.
        // This portable head-only owner suspends those ticks instead.
        var enabled=brain.AutomaticSleepEnabled;
        brain.SetAutomaticSleepEnabled(!enabled);brain.SetAutomaticSleepEnabled(enabled);brain.ResumeAfterPlay(Facing);
    }
    private static bool Finite(PointD p)=>double.IsFinite(p.X)&&double.IsFinite(p.Y);
}

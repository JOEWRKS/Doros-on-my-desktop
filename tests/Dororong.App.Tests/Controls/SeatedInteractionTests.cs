using System.Windows;
using System.Windows.Media;
using System.Reflection;
using Dororong.App.Controls;

namespace Dororong.App.Tests.Controls;

public sealed class SeatedInteractionTests
{
    [Theory][InlineData(1)][InlineData(1.25)][InlineData(1.5)]
    public void Reaching_paw_preserves_original_opaque_hair_and_face(double scale){
        var raster=new HighResolutionGrabRaster();
        var source=(float[])typeof(HighResolutionGrabRaster).GetField("head",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(raster)!;
        var motion=HighResolutionLocomotion.Sit(1);var offset=raster.MotionOffset(motion);
        var head=Matrix.Identity;head.Translate(offset.X,offset.Y);
        var neutral=HighResolutionFacePose.Neutral with{Head=head,MotionFrame=motion};
        var extent=(int)Math.Ceiling(256*scale);
        foreach(var left in new[]{false,true}){
            var inverse=Matrix.Identity;if(left)inverse.ScaleAt(-1,1,48,48);
            var reference=raster.RenderRetainedPixels(0,inverse,default,default,default,0,scale,neutral,scale,null);
            var protectedPixels=new List<int>();
            for(var y=0;y<extent;y++)for(var x=0;x<extent;x++){
                var center=inverse.Transform(new Point((x+.5)/scale-80,(y+.5)/scale-80));
                if(center.X<5||center.X>33||center.Y<47||center.Y>72)continue;
                var opaque=true;
                for(var yy=0;yy<2;yy++)for(var xx=0;xx<2;xx++){
                    var q=inverse.Transform(new Point((x+(xx+.5)/2)/scale-80,(y+(yy+.5)/2)/scale-80));
                    var sx=(int)Math.Floor((q.X-offset.X)*225/96-.5);
                    var sy=(int)Math.Floor((q.Y-offset.Y)*225/96-.5);
                    // Require the whole cubic sampling stencil to be opaque.
                    for(var j=sy-1;j<=sy+2;j++)for(var i=sx-1;i<=sx+2;i++)
                        if(i<0||j<0||i>=225||j>=225||source[(j*225+i)*4+3]<255)opaque=false;
                }
                if(opaque)protectedPixels.Add((y*extent+x)*4);
            }
            Assert.True(protectedPixels.Count>40);
            byte[]? output=null;
            foreach(var angle in new[]{-1.15,-.8,-.35,0,.5})
            foreach(var frame in new[]{1,5,10,15,20,33,50,58,62}){
                var pose=SeatedInteraction.Pose(neutral,SeatedReaction.Tap,frame/60d,angle);
                output=raster.RenderRetainedPixels(0,inverse,default,default,default,0,scale,pose,scale,output);
                foreach(var at in protectedPixels)for(var c=0;c<4;c++)
                    Assert.True(reference[at+c]==output[at+c],$"Head overwritten: scale={scale},left={left},angle={angle},frame={frame},pixel={at/4}");
            }
        }
    }
    [Fact] public void Authored_paw_aims_up_level_and_down_without_stretching_body_texels(){
        foreach(var target in new[]{new Point(5,45),new Point(0,73),new Point(0,86)}){
            var angle=SeatedInteraction.PointerAngle(target);
            var face=SeatedInteraction.Pose(HighResolutionFacePose.Neutral,SeatedReaction.Tap,.33,angle);
            var paw=new HighResolutionGrabRaster.SeatedPaw(face,1);
            Assert.True(paw.Tip.X<25);
            Assert.InRange(Math.Atan2(paw.Tip.Y-73,25-paw.Tip.X)-angle,-1e-9,1e-9);
            Assert.InRange((paw.Tip-new Point(25,73)).Length,18.99,19.01);
            var warp=new HighResolutionGrabRaster.SeatedWarp(face);
            for(var y=55d;y<=90;y++)for(var x=10d;x<=40;x++)Assert.Equal(new Point(x,y),warp.Map(new(x,y)));
        }
    }
    [Fact] public void Pointer_changes_update_live_aim_smoothly(){
        var session=new SeatedInteraction();
        for(var i=0;i<20;i++)session.Advance(.02,true,false,new(5,45));
        var upper=session.AimAngle;
        session.Advance(.02,true,false,new(0,86));
        Assert.True(session.AimAngle>upper);Assert.True(session.AimAngle<SeatedInteraction.PointerAngle(new(0,86)));
        for(var i=0;i<30;i++)session.Advance(.02,true,false,new(0,86));
        Assert.InRange(session.AimAngle,.47,.51);
    }
    [Fact] public void Seated_contour_fix_is_local_and_monotone(){
        for(var frame=257;frame<=321;frame+=4)for(var y=50d;y<90;y+=.5){
            var previous=double.NegativeInfinity;
            for(var x=0d;x<96;x+=.25){
                var source=HighResolutionGrabRaster.SeatedContourSource(x,y,frame);
                Assert.True(source>previous);previous=source;
                if(y<=58||y>=73||x>=30||frame==257)Assert.Equal(x,source);
                Assert.InRange(x-source,-1e-8,2.30000001);
            }
        }
        foreach(var frame in new[]{-1,1,256,322,705,770,836})Assert.Equal(18,HighResolutionGrabRaster.SeatedContourSource(18,65,frame));
    }
    [Fact] public void Near_front_dwell_taps_but_rump_unavailable_and_standing_do_not(){
        var s=new SeatedInteraction();
        for(var i=0;i<30;i++)s.Advance(.02,true,false,new(90,65));
        Assert.Equal(SeatedReaction.None,s.Kind);
        for(var i=0;i<30;i++)s.Advance(.02,false,false,new(10,65));
        Assert.Equal(SeatedReaction.None,s.Kind);
        for(var i=0;i<8;i++)s.Advance(.02,true,false,new(10,65));
        Assert.Equal(SeatedReaction.Tap,s.Kind);
        for(var i=0;i<60;i++)s.Advance(.02,true,false,null);
        Assert.Equal(SeatedReaction.None,s.Kind);
    }
    [Fact] public void Click_has_priority_and_cooldown_survives_cancellation_and_spam(){
        var s=new SeatedInteraction();for(var i=0;i<8;i++)s.Advance(.02,true,false,new(10,65));
        s.Advance(.02,true,true,null);Assert.Equal(SeatedReaction.Shake,s.Kind);
        for(var i=0;i<25;i++)s.Advance(.02,true,i%2==0,null);
        Assert.InRange(s.Age,.49,.51);
        s.Advance(.02,false,false,null);s.Advance(.02,true,true,null);
        Assert.Equal(SeatedReaction.None,s.Kind);
        for(var i=0;i<150;i++)s.Advance(.02,true,false,null);
        s.Advance(.02,true,true,null);Assert.Equal(SeatedReaction.Shake,s.Kind);
    }
    [Fact] public void No_retrigger_during_continuous_click_state(){
        var s=new SeatedInteraction();s.Advance(.02,true,true,null);
        for(var i=0;i<250;i++)s.Advance(.02,true,true,null);
        Assert.Equal(SeatedReaction.None,s.Kind);
    }
    [Fact] public void Warps_are_invertible_and_supporting_rear_foot_is_fixed(){
        foreach(var kind in new[]{SeatedReaction.Tap,SeatedReaction.Shake})
        for(var frame=0;frame<=63;frame++){
            var face=SeatedInteraction.Pose(HighResolutionFacePose.Neutral,kind,frame/60d);
            var w=new HighResolutionGrabRaster.SeatedWarp(face);
            for(var y=50d;y<=95;y+=2)for(var x=0d;x<=90;x+=2){
                var p=w.Source(x,y);var back=w.Map(p);
                Assert.InRange(Math.Abs(back.X-x)+Math.Abs(back.Y-y),0,1e-5);
                var a=w.Map(new(x,y));var b=w.Map(new(x+.001,y));var c=w.Map(new(x,y+.001));
                var det=((b.X-a.X)*(c.Y-a.Y)-(b.Y-a.Y)*(c.X-a.X))/1e-6;
                Assert.True(det>.4);
            }
            Assert.Equal(new Point(65,90),w.Map(new(65,90)));
        }
    }
    [Theory][InlineData(false,1.05)][InlineData(true,.95)]
    public void Endpoints_are_exact(bool shake,double duration){
        var kind=shake?SeatedReaction.Shake:SeatedReaction.Tap;
        var baseline=HighResolutionFacePose.Neutral;
        Assert.Equal(baseline,SeatedInteraction.Pose(baseline,kind,0));
        Assert.Equal(baseline,SeatedInteraction.Pose(baseline,kind,duration));
    }
}

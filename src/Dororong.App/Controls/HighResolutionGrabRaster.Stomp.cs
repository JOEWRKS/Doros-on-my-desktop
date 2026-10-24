using System.Windows;
using System.Windows.Media;
using Dororong.Core.Behavior;

namespace Dororong.App.Controls;

internal sealed partial class HighResolutionGrabRaster
{
    internal static HighResolutionFacePose SnackReaction(double age,int streak)
    {
        var lift=SnackAngerMotion.PawLift(age,streak);
        var reach=SnackAngerMotion.PawReach(age,streak);
        var crouch=SnackAngerMotion.Crouch(age,streak);
        var head=streak>=2?StompWarp.Head(crouch):
            HuntRenderer.HeadTransform(156,new(0,0,SnackAngerMotion.Roll(age,streak)));
        return HighResolutionFacePose.Neutral with{Head=head,Angry=true,Furious=streak>=3,
            StompLift=lift,StompReach=reach,StompCrouch=crouch};
    }

    // Same head pivot/bob/roll as the pre-pounce pose, at <=34% depth.
    // Second reaction retains its accepted local warp. Third partitions the
    // leading foreleg and rotates it as one rigid piece about the shoulder.
    internal readonly struct StompWarp
    {
        private readonly double lift,reach,crouch;
        private readonly bool forwardSlap;
        private readonly Matrix head;
        private readonly Matrix arm,armInverse;
        private readonly double inkWidth;
        private readonly Point jointTop,jointBottom;
        private readonly double jointSlope;
        internal bool RigidArm=>forwardSlap&&Active;
        internal bool Active=>lift>0||reach>0||crouch>0;
        internal StompWarp(HighResolutionFacePose face,double bodyScale=1){
            forwardSlap=face.Furious;
            lift=Math.Clamp(face.StompLift,0,forwardSlap?4.8:2.4);
            reach=Math.Clamp(face.StompReach,0,forwardSlap?16:4);
            crouch=Math.Clamp(face.StompCrouch,0,.34);head=Head(crouch);
            var deploy=reach/16;
            arm=Matrix.Identity;
            if(forwardSlap){
                // Preserve the user's accepted larger foreleg and trajectory.
                // The silhouette extends during preparation; once deployed it
                // rotates rigidly. Rebuild ink AFTER this transform so its width
                // does not inherit the limb's longitudinal scale.
                var amount=1.5*deploy;
                const double ux=-6/7.810249675906654,uy=5/7.810249675906654;
                var a=1+amount*ux*ux;var b=amount*ux*uy;var d=1+amount*uy*uy;
                arm=new Matrix(a,b,b,d,29-a*29-b*74,74-b*29-d*74);
                arm.RotateAt(10*deploy+40*lift/4.8,29,74);
                arm.Translate(deploy,-4.7*deploy);
            }
            armInverse=arm;armInverse.Invert();
            jointTop=arm.Transform(new Point(26,70));
            jointBottom=arm.Transform(new Point(28,76.4));
            var next=arm.Transform(new Point(28.5,75.75));
            var previous=arm.Transform(new Point(27.5,77.05));
            jointSlope=(next.Y-previous.Y)/Math.Max(.1,next.X-previous.X);
            inkWidth=1.295415959252971/Math.Max(1,bodyScale);
        }
        internal Point ArmPoint(Point source)=>arm.Transform(source);
        internal Point ArmSource(Point point)=>armInverse.Transform(point);
        private static double ArmMask(double x,double y)=>
            Smooth((y-68)/2)*(1-Smooth((x-29)/3));
        internal void CompositeArm(float[] body,double x,double y,double sourceX,double sourceY,Span<double> result,Span<double> temporary){
            if(!RigidArm)return;
            var keep=1-ArmMask(sourceX,sourceY);
            for(var c=0;c<4;c++)result[c]*=keep;
            // Join FILLED silhouettes, not a line over two old outlines.
            // Reconstruct the entire foreleg exterior below, removing all
            // interior cutout ink and the old drifting diagonal repair line.
            var shoulder=ShoulderAlpha(x,y);
            for(var c=0;c<4;c++)result[c]=255*shoulder+result[c]*(1-shoulder);
            var p=ArmSource(new(x,y));var mask=ArmMask(p.X,p.Y);
            if(mask>0){
                Sample(body,375,(p.X+32)*225/96-.5,(p.Y+32)*225/96-.5,temporary);
                // Complementary cut masks partition one surface. Source-over
                // would turn two half-covered opaque pieces into 75% alpha.
                var totalAlpha=temporary[3]*mask+result[3];var normalize=totalAlpha>255?255/totalAlpha:1;
                for(var c=0;c<4;c++)result[c]=(temporary[c]*mask+result[c])*normalize;
            }
            // Re-outline the union, including the rotating cut cap. Ink that
            // used to be hidden under the head must not become an interior dot.
            var blend=Smooth((x+12)/2)*Smooth((42-x)/2)*Smooth((y-55)/2)*Smooth((94-y)/2)*Smooth(reach/3);
            if(blend<=0)return;
            var alpha=UnionAlpha(body,x,y,temporary);
            var inside=alpha;
            for(var i=0;i<8;i++){
                var angle=i*Math.PI/4;
                inside=Math.Min(inside,UnionAlpha(body,x+inkWidth*Math.Cos(angle),y+inkWidth*Math.Sin(angle),temporary));
            }
            // The transformed silhouette is already antialiased. Feeding its
            // soft coverage directly into ink adds a second broad grey ramp.
            // Narrow only that inner ink ramp; leave silhouette alpha, size,
            // stroke radius and the final destination-area sampling unchanged.
            var ink=Smooth((1-inside/255-.12)/.76);
            result[0]+=(alpha*(1-ink+ink*56/255)-result[0])*blend;
            result[1]+=(alpha*(1-ink+ink*52/255)-result[1])*blend;
            result[2]+=(alpha*(1-ink+ink*68/255)-result[2])*blend;
            result[3]+=(alpha-result[3])*blend;
        }
        private double ShoulderAlpha(double x,double y){
            var top=new Point(39,65);var bottom=new Point(39,86.2);
            var distance=Math.Min(Math.Min(Edge(jointTop,top,x,y),Edge(top,bottom,x,y)),
                Math.Min(JointDistance(x,y),Edge(jointBottom,jointTop,x,y)));
            return Math.Clamp(distance+.5,0,1)*Smooth(reach/3);
        }
        private static double Edge(Point a,Point b,double x,double y){
            var dx=b.X-a.X;var dy=b.Y-a.Y;
            return (dx*(y-a.Y)-dy*(x-a.X))/Math.Max(1e-9,Math.Sqrt(dx*dx+dy*dy));
        }
        private double JointDistance(double x,double y){
            var width=Math.Max(1,39-jointBottom.X);
            var t=Math.Clamp((x-jointBottom.X)/width,0,1);var t2=t*t;var t3=t2*t;
            var edge=(2*t3-3*t2+1)*jointBottom.Y+(t3-2*t2+t)*width*jointSlope+
                (-2*t3+3*t2)*86.2+(t3-t2)*width*.8;
            var slope=((6*t2-6*t)*jointBottom.Y+(3*t2-4*t+1)*width*jointSlope+
                (-6*t2+6*t)*86.2+(3*t2-2*t)*width*.8)/width;
            return (edge-y)/Math.Sqrt(1+slope*slope);
        }
        private double UnionAlpha(float[] body,double x,double y,Span<double> temporary){
            var p=Source(x,y);
            Sample(body,375,(p.X+32)*225/96-.5,(p.Y+32)*225/96-.5,temporary);
            var alpha=temporary[3]*(1-ArmMask(p.X,p.Y));
            var shoulder=ShoulderAlpha(x,y);alpha=255*shoulder+alpha*(1-shoulder);
            p=ArmSource(new(x,y));var mask=ArmMask(p.X,p.Y);
            Sample(body,375,(p.X+32)*225/96-.5,(p.Y+32)*225/96-.5,temporary);
            alpha=Math.Min(255,temporary[3]*mask+alpha);
            // Replace the vacated lower shoulder, not just fill over its old
            // protruding outline. Join at the moving inner edge's tangent.
            var weight=Smooth((x-jointBottom.X)/1.2)*Smooth((39-x)/2)*Smooth((y-63)/2)*Smooth(reach/3);
            var joined=Math.Clamp(JointDistance(x,y)+.5,0,1)*255;
            return alpha+(joined-alpha)*weight;
        }
        internal static Matrix Head(double amount){
            var matrix=Matrix.Identity;
            matrix.RotateAt(-.07*amount*180/Math.PI,43,48);
            matrix.Translate(-2*amount,6.3*amount);return matrix;
        }
        private double PawWeight(double x,double y)=>forwardSlap?0:
            Smooth((x-29)/9)*Smooth((54-x)/6)*Smooth((y-69)/14);
        private Point Bow(Point p){
            if(crouch==0||p.X<=8||p.X>=60||p.Y<=40||p.Y>=90)return p;
            var floor=79+6*Math.Clamp((p.X-23)/20,0,1);
            var contact=1-Smooth((p.Y-60)/(floor-60));
            var front=(1-Smooth((p.X-46)/8))*Smooth((p.X-8)/8)*Smooth((p.Y-40)/8);
            var h=head.Transform(p);return new(p.X+(h.X-p.X)*front*contact,p.Y+(h.Y-p.Y)*front*contact);
        }
        internal Point Map(Point source){
            var weight=PawWeight(source.X,source.Y);
            return Bow(new(source.X-reach*weight,source.Y-lift*weight));
        }
        internal Point Source(double x,double y){
            if(!Active||x<=(forwardSlap?-24:8)||x>=60||y<=40||y>=96)return new(x,y);
            // Undo the small crouch first, then the local forward/down paw.
            var unbowed=new Point(x,y);
            for(var i=0;i<32;i++){
                var mapped=Bow(unbowed);var dx=x-mapped.X;var dy=y-mapped.Y;
                unbowed=new(unbowed.X+dx,unbowed.Y+dy);
                if(Math.Abs(dx)+Math.Abs(dy)<1e-8)break;
            }
            if((reach==0&&lift==0)||PawWeight(unbowed.X,unbowed.Y)==0)return unbowed;
            // Solve the scalar displacement along (reach,lift), bracketed
            // in [0,1]. Unlike fixed-point iteration this remains accurate
            // at the steepest shoulder transition without excessive retries.
            double lo=0,hi=1;
            for(var i=0;i<32;i++){
                var w=(lo+hi)/2;
                if(w<PawWeight(unbowed.X+reach*w,unbowed.Y+lift*w))lo=w;else hi=w;
            }
            var weight=(lo+hi)/2;
            return new(unbowed.X+reach*weight,unbowed.Y+lift*weight);
        }
    }
}

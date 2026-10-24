using System.Windows;

namespace Dororong.App.Controls;

internal sealed partial class HighResolutionGrabRaster
{
    internal readonly struct StretchWarp(HighResolutionFacePose face)
    {
        internal bool Active=>face.Stretch!=0 || face.StretchShake!=0;
        internal Point Map(Point p){
            var front=1-Smooth((p.X-44)/18);
            var upper=1-Smooth((p.Y-59)/28);
            var paw=Smooth((p.Y-68)/10)*front;
            var neck=front*upper;
            var rump=Smooth((p.X-44)/20)*upper;
            return new(p.X-face.Stretch*(3*neck+16*paw)+face.StretchShake*.7*upper,
                p.Y+face.Stretch*(4*neck-7*rump));
        }
        internal Point Source(double x,double y){
            var p=new Point(x,y);
            if(!Active)return p;
            // Solve both coupled axes. Fixed-point iteration converged too
            // slowly where the forward paw and rising rump gradients overlap,
            // leaving dark repeated contour samples during the return.
            static double D(double t)=>t<=0||t>=1?0:6*t*(1-t);
            for(var i=0;i<12;i++){
                var q=Map(p);var dx=x-q.X;var dy=y-q.Y;
                if(Math.Abs(dx)+Math.Abs(dy)<1e-7)break;
                var front=1-Smooth((p.X-44)/18);var fx=-D((p.X-44)/18)/18;
                var upper=1-Smooth((p.Y-59)/28);var uy=-D((p.Y-59)/28)/28;
                var paw=Smooth((p.Y-68)/10);var py=D((p.Y-68)/10)/10;
                var rump=Smooth((p.X-44)/20);var rx=D((p.X-44)/20)/20;
                var a=1-face.Stretch*fx*(3*upper+16*paw);
                var b=-face.Stretch*front*(3*uy+16*py)+face.StretchShake*.7*uy;
                var c=face.Stretch*upper*(4*fx-7*rx);
                var d=1+face.Stretch*uy*(4*front-7*rump);
                var det=a*d-b*c;
                p=new(p.X+(d*dx-b*dy)/det,p.Y+(a*dy-c*dx)/det);
            }
            return p;
        }
    }
}

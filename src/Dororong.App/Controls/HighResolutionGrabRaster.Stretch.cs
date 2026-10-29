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
            // The leading paw is composited separately. This warp only bows
            // the upper body, leaving the adjacent supporting leg planted.
            var neck=front*upper;
            var rump=Smooth((p.X-44)/20)*upper;
            return new(p.X-face.Stretch*3*neck+face.StretchShake*.7*upper,
                p.Y+face.Stretch*(4*neck-7*rump));
        }
        internal Point Source(double x,double y){
            var p=new Point(x,y);
            if(!Active)return p;
            // Invert the coupled neck/rump deformation without resampling
            // the supporting foreleg into the reaching paw.
            static double D(double t)=>t<=0||t>=1?0:6*t*(1-t);
            for(var i=0;i<24;i++){
                var q=Map(p);var dx=x-q.X;var dy=y-q.Y;
                if(Math.Abs(dx)+Math.Abs(dy)<1e-7)break;
                var front=1-Smooth((p.X-44)/18);var fx=-D((p.X-44)/18)/18;
                var upper=1-Smooth((p.Y-59)/28);var uy=-D((p.Y-59)/28)/28;
                var rump=Smooth((p.X-44)/20);var rx=D((p.X-44)/20)/20;
                var a=1-face.Stretch*3*fx*upper;
                var b=-face.Stretch*3*front*uy+face.StretchShake*.7*uy;
                var c=face.Stretch*upper*(4*fx-7*rx);
                var d=1+face.Stretch*uy*(4*front-7*rump);
                var det=a*d-b*c;
                var sx=(d*dx-b*dy)/det;var sy=(a*dy-c*dx)/det;
                // Backtrack any step that crosses a transition and increases
                // residual, rather than sampling a second contour.
                var step=1d;var next=new Point(p.X+sx,p.Y+sy);
                for(var back=0;back<10;back++){
                    var mapped=Map(next);
                    if(Math.Abs(x-mapped.X)+Math.Abs(y-mapped.Y)<Math.Abs(dx)+Math.Abs(dy))break;
                    step*=.5;next=new(p.X+sx*step,p.Y+sy*step);
                }
                p=next;
            }
            return p;
        }
    }
}

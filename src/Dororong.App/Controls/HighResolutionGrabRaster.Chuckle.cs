using System.Windows;
using System.Windows.Media;

namespace Dororong.App.Controls;

internal sealed partial class HighResolutionGrabRaster
{
    // Motion adaptation of the inspected 24s+ reference: raise once, keep the
    // paw at the mouth through small upper-body pulses, then lower once.
    // These timings are authored for this 2D pet, not measured video data.
    internal static HighResolutionFacePose SnackChuckle(double age)
    {
        var cover=Smooth(age/.26)*(1-Smooth((age-1.48)/.32));
        double pulse=0;
        for(var i=0;i<4;i++) {
            var start=i switch {0=>.30,1=>.57,2=>.88,_=>1.16};
            var t=(age-start)/.23;
            if(t>0&&t<1)pulse+=Math.Pow(Math.Sin(Math.PI*t),2);
        }
        var bob=.95*pulse*cover;
        var head=Matrix.Identity;head.RotateAt(-1.2*cover+.7*pulse,43,48);
        head.Translate(0,bob);
        return HighResolutionFacePose.Neutral with {Head=head,ChuckleCover=cover,ChuckleBob=bob};
    }

    internal readonly struct ChuckleWarp
    {
        private readonly double cover,bob;
        private readonly Matrix armInverse;
        private readonly double inkWidth;
        private readonly double roundBlend,armScaleX,armScaleY;
        private readonly Matrix groomInverse;
        private readonly bool grooming;
        private readonly double seatedPawCleanup;
        private readonly bool removeSeatedBranch;
        internal bool Active=>cover>1e-9;
        internal ChuckleWarp(HighResolutionFacePose face,double bodyScale=1){
            cover=face.ChuckleCover;bob=face.ChuckleBob;
            inkWidth=1.295415959252971/Math.Max(1,bodyScale);
            var arm=Matrix.Identity;
            arm.RotateAt(75*cover,29,74);
            arm.Translate(4*cover,-5*cover+bob);
            armInverse=arm;armInverse.Invert();
            // Latest red trace is upright, not a sideways C-shaped paw.
            // Keep its narrow tip, long outside edge and descending inner line.
            armScaleX=.75+.25*cover;armScaleY=.6+.4*cover;
            roundBlend=Smooth(cover/.3);
            seatedPawCleanup=face.GroomingPaw?roundBlend:0;
            removeSeatedBranch=face.GroomingPaw;
            groomInverse=Matrix.Identity;
            grooming=face.GroomSweep!=0;
            if(grooming){
                var groom=Matrix.Identity;groom.RotateAt(18*face.GroomSweep,33,74);
                groomInverse=groom;groomInverse.Invert();
            }
        }
        private static double Mask(double x,double y)=>Smooth((y-68)/2)*(1-Smooth((x-29)/3));
        internal Point BodySource(double x,double y){
            if(!Active||y>=77)return new(x,y);
            var k=bob/22;
            return new(x,y<=55+bob?y-bob:(y-77*k)/(1-k));
        }
        internal void RemovePaw(float[] body,double x,double y,Span<double> pixel,Span<double> scratch){
            var keep=1-Mask(x,y);
            // The standing-paw cutout also crosses the seated chest/leg crease.
            // Keep that source junction opaque: the wiping overlay moves away
            // from it, exposing the partial cutout otherwise hidden by the paw.
            var seatedCrease=seatedPawCleanup*Smooth(x-Math.Max(27,27.3+.55*(y-76)))*Smooth((y-74.5)/1.5)*
                (1-Smooth((y-81)/1.5));
            keep+=(1-keep)*seatedCrease;
            // Keep the fork's top junction, not the obsolete inner foreleg
            // branch descending beside the diagonal body contour. Follow the
            // gap between those two edges instead of protecting the whole spur.
            // Like Mask above this belongs to the replaced source paw, not
            // the moving overlay. Fading it with cover resurrected the branch
            // while the paw was raising/lowering.
            if(removeSeatedBranch)
                keep*=1-Smooth(y-78)*
                    (1-Smooth((x-Math.Min(32.25,30.2+.65*(y-78)))/.55))*
                    (1-Smooth((y-86)/2));
            for(var c=0;c<4;c++)pixel[c]*=keep;
            var blend=Smooth((x-14)/2)*Smooth((40-x)/2)*Smooth((y-62)/2)*Smooth((77-y)/2)*(1-seatedCrease);
            if(blend<=0)return;
            var alpha=TorsoAlpha(body,x,y,scratch);var inside=alpha;
            for(var i=0;i<8;i++){
                var angle=i*Math.PI/4;
                inside=Math.Min(inside,TorsoAlpha(body,x+inkWidth*Math.Cos(angle),y+inkWidth*Math.Sin(angle),scratch));
            }
            var ink=Smooth((1-inside/255-.12)/.76);
            pixel[0]+=(alpha*(1-ink+ink*56/255)-pixel[0])*blend;
            pixel[1]+=(alpha*(1-ink+ink*52/255)-pixel[1])*blend;
            pixel[2]+=(alpha*(1-ink+ink*68/255)-pixel[2])*blend;
            pixel[3]+=(alpha-pixel[3])*blend;
        }
        private static double TorsoAlpha(float[] body,double x,double y,Span<double> scratch){
            Sample(body,375,(x+32)*225/96-.5,(y+32)*225/96-.5,scratch);
            var alpha=scratch[3]*(1-Mask(x,y));
            // Rounded neck-to-shoulder backing, entirely behind the head/paw.
            var t=Math.Clamp(((x-25)*10+(y-65)*9)/181,0,1);
            var dx=x-(25+10*t);var dy=y-(65+9*t);
            var fill=Math.Clamp(6.5-Math.Sqrt(dx*dx+dy*dy)+.5,0,1);
            return alpha+(255-alpha)*fill;
        }
        internal void Overlay(float[] body,double x,double y,Span<double> pixel,Span<double> scratch){
            if(!Active||(x>=34&&y>=77))return;
            if(grooming){var point=groomInverse.Transform(new Point(x,y));x=point.X;y=point.Y;}
            var p=armInverse.Transform(new Point(x,y));
            var mask=p.Y>88||p.X<12?0:Mask(p.X,p.Y);
            scratch.Clear();
            if(roundBlend>0){
                var localX=(x-(23-2*cover))/armScaleX+21;
                var localY=(y-(76-8*cover+bob))/armScaleY+68;
                var distance=FoldedPawOutline.Distance(localX,localY)*Math.Min(armScaleX,armScaleY);
                // Morph one silhouette, not two already-inked pictures. RGB
                // crossfading retained both old/new outlines during transitions.
                if(roundBlend<1)distance=StandingPawOutline.Distance(body,p.X,p.Y)*(1-roundBlend)+distance*roundBlend;
                var alphaRound=Math.Clamp(.5-distance,0,1)*255;
                var ink=Smooth((distance+inkWidth+.25)/.5);
                ink*=1-(1-roundBlend)*Smooth((pixel[3]-200)/50);
                // The arm enters the chest; do not draw a closed cap across
                // the white torso at its attachment.
                ink*=1-Smooth((localX-29)/3)*Smooth((localY-63)/2)*Smooth((pixel[3]-200)/50);
                ink*=1-Smooth((localX-24)/3)*Smooth((localY-68)/2)*(1-Smooth((localY-73)/2));
                if(y>67&&pixel[3]>=250&&pixel[0]>220&&pixel[1]>220&&pixel[2]>220)
                    ink*=Smooth((cover-.15)/.45);
                scratch[0]=alphaRound*(1-ink+ink*56/255);
                scratch[1]=alphaRound*(1-ink+ink*52/255);
                scratch[2]=alphaRound*(1-ink+ink*68/255);
                scratch[3]=alphaRound;
            }
            var alpha=scratch[3]/255;
            if(alpha<=0)return;
            if(roundBlend<1&&mask<1&&pixel[3]<255-1e-8){
                // Complementary shoulder cutouts partition one surface. A
                // source-over blend makes two half masks only 75% opaque.
                var total=pixel[3]+scratch[3];var norm=total>255?255/total:1;
                for(var c=0;c<4;c++)pixel[c]=(pixel[c]+scratch[c])*norm;
                return;
            }
            for(var c=0;c<4;c++)pixel[c]=scratch[c]+pixel[c]*(1-alpha);
        }
    }

    private static class StandingPawOutline
    {
        private const int Width=145,Height=145;
        private static float[]? field;
        private static readonly object gate=new();
        internal static double Distance(float[] body,double x,double y){
            if(field is null)lock(gate){field??=Build(body);}
            var u=(x-4)*4;var v=(y-54)*4;
            if(u<0||v<0||u>=Width-1||v>=Height-1)return 50;
            var ix=(int)u;var iy=(int)v;var fx=u-ix;var fy=v-iy;var at=iy*Width+ix;
            return (field[at]*(1-fx)+field[at+1]*fx)*(1-fy)
                +(field[at+Width]*(1-fx)+field[at+Width+1]*fx)*fy;
        }
        private static float[] Build(float[] body){
            var alpha=new float[Width*Height];Span<double> sample=stackalloc double[4];
            for(var row=0;row<Height;row++)for(var col=0;col<Width;col++){
                var x=4+col/4d;var y=54+row/4d;
                Sample(body,375,(x+32)*225/96-.5,(y+32)*225/96-.5,sample);
                alpha[row*Width+col]=(float)(sample[3]/255*Smooth((y-68)/2)*(1-Smooth((x-29)/3)));
            }
            var edge=new List<Point>();
            for(var row=1;row<Height-1;row++)for(var col=1;col<Width-1;col++){
                var at=row*Width+col;var inside=alpha[at]>=.5;
                if((alpha[at+1]>=.5)!=inside||(alpha[at+Width]>=.5)!=inside)
                    edge.Add(new(col,row));
            }
            var result=new float[alpha.Length];
            for(var row=0;row<Height;row++)for(var col=0;col<Width;col++){
                var squared=double.MaxValue;
                foreach(var p in edge){var dx=col-p.X;var dy=row-p.Y;squared=Math.Min(squared,dx*dx+dy*dy);}
                var at=row*Width+col;result[at]=(float)(Math.Sqrt(squared)/4*(alpha[at]>=.5?-1:1));
            }
            return result;
        }
    }

    internal static ReadOnlySpan<Point> ChucklePawControlPoints=>FoldedPawOutline.ControlPoints;

    private static class FoldedPawOutline
    {
        // A fixed quarter-unit signed-distance tile (~62 KiB). It is built
        // once, not cached by frame/size/age, and sampled at output resolution.
        private const int Width=145,Height=109;
        internal static readonly Point[] ControlPoints=CreateControlPoints();
        private static readonly float[] field=Build();
        internal static double Distance(double x,double y){
            var u=(x-4)*4;var v=(y-54)*4;
            if(u<0||v<0||u>=Width-1||v>=Height-1)return 100;
            var ix=(int)u;var iy=(int)v;var fx=u-ix;var fy=v-iy;var at=iy*Width+ix;
            return (field[at]*(1-fx)+field[at+1]*fx)*(1-fy)
                +(field[at+Width]*(1-fx)+field[at+Width+1]*fx)*fy;
        }
        private static Point[] CreateControlPoints(){
            // The rejected version merely aligned tangents at low corner
            // anchors, retaining a flat shelf and box-like lower outside edge.
            // Use an actual elliptical outer sweep at the same overall size,
            // then a rounded tip. Do not blur the image or change the motion.
            var controls=new List<Point>{new(35,76),new(29,75.5),new(27,79.8),new(21,79.8)};
            void Arc(double cx,double cy,double rx,double ry,double start,double end){
                var k=4d/3*Math.Tan((end-start)/4);
                controls.Add(new(cx+rx*(Math.Cos(start)-k*Math.Sin(start)),
                    cy+ry*(Math.Sin(start)+k*Math.Cos(start))));
                controls.Add(new(cx+rx*(Math.Cos(end)+k*Math.Sin(end)),
                    cy+ry*(Math.Sin(end)-k*Math.Cos(end))));
                controls.Add(new(cx+rx*Math.Cos(end),cy+ry*Math.Sin(end)));
            }
            Arc(21,68.7,9,11.1,Math.PI/2,Math.PI);
            Arc(21,68.7,9,11.1,Math.PI,Math.PI*1.25);
            Arc(21,68.7,9,11.1,Math.PI*1.25,Math.PI*1.5);
            Arc(21,62,4.583333333333333,4.4,-Math.PI/2,0);
            controls.AddRange([new(25.583333333333333,64.5),new(24.8,68),new(24.125,70.41666666666667),
                new(28,71),new(32,70),new(35,70)]);
            return controls.ToArray();
        }
        private static float[] Build(){
            var points=new List<Point>{ControlPoints[0]};
            for(var segment=0;segment<7;segment++){
                var at=segment*3;var start=ControlPoints[at];var a=ControlPoints[at+1];
                var b=ControlPoints[at+2];var end=ControlPoints[at+3];
                // Finer flattening removes polygon facets without enlarging
                // the retained distance tile or adding per-frame allocations.
                for(var i=1;i<=32;i++){
                    var t=i/32d;var s=1-t;
                    points.Add(new(s*s*s*start.X+3*s*s*t*a.X+3*s*t*t*b.X+t*t*t*end.X,
                        s*s*s*start.Y+3*s*s*t*a.Y+3*s*t*t*b.Y+t*t*t*end.Y));
                }
            }
            var result=new float[Width*Height];
            for(var row=0;row<Height;row++)for(var col=0;col<Width;col++){
                var x=4+col/4d;var y=54+row/4d;var inside=false;var squared=double.MaxValue;
                for(var i=0;i<points.Count;i++){
                    var a=points[i];var b=points[(i+1)%points.Count];
                    if((a.Y>y)!=(b.Y>y)&&x<(b.X-a.X)*(y-a.Y)/(b.Y-a.Y)+a.X)inside=!inside;
                    var dx=b.X-a.X;var dy=b.Y-a.Y;
                    var t=Math.Clamp(((x-a.X)*dx+(y-a.Y)*dy)/Math.Max(1e-9,dx*dx+dy*dy),0,1);
                    dx=x-a.X-t*dx;dy=y-a.Y-t*dy;squared=Math.Min(squared,dx*dx+dy*dy);
                }
                result[row*Width+col]=(float)(Math.Sqrt(squared)*(inside?-1:1));
            }
            return result;
        }
    }
}

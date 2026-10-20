using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Media;

namespace Dororong.App.Controls;

internal readonly record struct HighResolutionFacePose(Matrix Head, double EyeX, double EyeY,
    double Pull, double EyeFollow, double HairFollow, int BodyFrame, int MotionFrame=-1, bool Closed=false,
    int EffectFrame=-1, double MouthAge=-1, double DroolAge=-1, double Breathing=0,
    int Paw=0,double PawX=0,double PawY=0,double PawAngle=0,double FlutterPhase=-1)
{
    internal static HighResolutionFacePose Neutral => new(Matrix.Identity,0,0,0,0,0,-1);
    private static double Bump(double v,double a,double b,double c,double d) =>
        HighResolutionGrabRaster.Smooth((v-a)/(b-a))*HighResolutionGrabRaster.Smooth((d-v)/(d-c));

    // Continuous inverse lookup into the unchanged high-resolution head. The
    // accepted cheek envelope/gesture gain is reused, not its 96px raster.
    internal Point Source(Point q)=>Source(q,out _);
    internal Point Source(Point q,out bool skinDonor)
    {
        skinDonor=false;
        var x=q.X;var y=q.Y;
        var movingCheek=Pull!=0||EyeFollow!=0||HairFollow!=0;
        var chewing=MouthAge>=.48&&MouthAge<1.55;
        if(x<=-16||x>=54||y<=23||y>=71)return q;
        if(!movingCheek&&!chewing&&(EyeX==0&&EyeY==0||x<=15||x>=46||y<=43||y>=63))return q;
        if(MouthAge>=.48&&MouthAge<1.55&&x>=24&&x<=34&&y>=56&&y<=64){
            var beat=Math.Pow(Math.Sin((MouthAge-.48)/1.07*3*Math.PI),2);
            var w=Bump(x,24,26,32,34)*Bump(y,56,58,62,64);
            x=29+(x-29)/(1+.18*beat*w);y=60+(y-60)/(1-.08*beat*w)+.95*beat*w;
        }
        // Gaze is a rigid eye-layer translation in the raster compositor.
        // Never bend source coordinates: that changes the iris silhouette.
        if(Pull==0&&EyeFollow==0&&HairFollow==0)return new(x,y);
        var hair=Math.Max(Bump(x,5,14,42,50)*Bump(y,23,29,36,47),
            Math.Max(Bump(x,3,7,10,16)*Bump(y,30,41,59,69),Bump(x,44,48,50,54)*Bump(y,44,48,60,70)));
        if(x>=50&&y<44)hair=0;
        x+=.8*Math.Clamp(HairFollow/20,0,1)*hair;
        y-=.35*Math.Clamp(HairFollow/20,0,1)*hair;
        var eye=Bump(x,33,34,43,47)*Bump(y,43,47,58,63);
        x+=.6*Math.Clamp(EyeFollow/20,0,1)*eye;y-=.65*Math.Clamp(EyeFollow/20,0,1)*eye;
        // Mouth reaction remains independent of the stretched skin. The near
        // eye is kept at the hair opening below, with its captured gaze intact.
        var fraction=Math.Clamp(Pull/20,0,1);
        var vertical=HighResolutionGrabRaster.Smooth(fraction*2);
        var lateral=HighResolutionGrabRaster.Smooth(fraction*2-1);
        // Move only the observed feature footprint. A broad displacement field
        // over y40..63 also bent the bangs and sheared the cheek attachment.
        var featureSource=new Point(x+lateral,y-2*vertical);
        if(Pull>0&&x>=16&&x<31&&y>=43&&y<60&&
            !HighResolutionCheekSelection.Mouth(new(x,y))&&!HighResolutionCheekSelection.Mouth(featureSource)){
            var aperture=HighResolutionGrabRaster.GazeAperture(new(x,y));
            // The old eye selection included a diagonal strip of bangs.
            // Cleaning that strip to skin left the user's white wedge during
            // cheek pull/release. Keep the stationary hair opening in charge.
            if(aperture<1)return new(x,y);
        }
        var eyePoint=new Point(x,y);
        // A cheek-skin pull must not detach the near eye from its fixed bang
        // boundary. Preserve that eye's position and captured gaze; the mouth
        // retains its original reaction below. No copied hair/eye patch moves.
        if(Pull>0&&(HighResolutionCheekSelection.Eye(eyePoint)||
            HighResolutionGrabRaster.NearEyeFootprint(eyePoint)))return eyePoint;
        if(Pull>0&&HighResolutionCheekSelection.Mouth(featureSource))return featureSource;
        // Translate the contour collar; stretch only the inner cheek toward a
        // fixed root. No painted skin, copied bangs or added outline stroke.
        if(Pull!=0&&x<35&&y>=53&&y<64){
            var index=Math.Clamp((int)Math.Floor(y)-53,0,10);var a=CheekPullRenderer.Rows[index];var b=CheekPullRenderer.Rows[index+1];var t=y-a.Y;
            var edge=HighResolutionCheekSelection.Edge(y);
            var target=HighResolutionCheekSelection.FastEdge(y,Math.Clamp(Pull/20,0,1));
            // Keep the original contour at zero strength, including its
            // subpixel offset from the old 96px selection.
            target+=edge-(a.Edge+(b.Edge-a.Edge)*t);
            var shift=Pull>0?edge-target:Pull*.5*(a.Weight+(b.Weight-a.Weight)*t);
            target=edge-shift;
            var collar=edge+1.5;
            // Features are composited above and their vacated footprints use
            // source donors; only the cleaned interior may extend to x35.
            var root=Pull>0?35:a.Root+(b.Root-a.Root)*t;
            // The source-selected cheek starts at its transported edge. The
            // old all-x<root branch also moved the side hair outside that edge.
            if(Pull<0&&x>=collar&&x<root){
                // Match the original rebound: compress selected skin only.
                // Without the source clamp, compression sampled the outer
                // cheek/hair ink again and duplicated it inside the skin.
                x=Math.Clamp(root+(x-root)/Math.Max(.5,1+shift/(root-collar)),collar,root-.001);
            }else if(Pull>0&&x>=target-.75&&x<root){
                var y0=Math.Max(53,y-.05);var y1=Math.Min(63.99,y+.05);
                var originalTangent=(HighResolutionCheekSelection.Edge(y1)-HighResolutionCheekSelection.Edge(y0))/(y1-y0);
                var tangent=(HighResolutionCheekSelection.FastEdge(y1,fraction)-HighResolutionCheekSelection.FastEdge(y0,fraction))/(y1-y0);
                var ratio=Math.Sqrt((1+originalTangent*originalTangent)/(1+tangent*tangent));
                var moved=target+1.5/ratio;
                x=x<moved?edge+(x-target)*ratio:collar+(x-moved)*(root-collar)/(root-moved);
            }
        }
        var source=new Point(x,y);
        var clean=Pull>0?HighResolutionCheekSelection.Clean(source):source;
        // A cleanup donor is a colour lookup, not another physical eye
        // location. Never run gaze composition on this replacement skin.
        // The stretched near-cheek interior must never gain a second copy
        // of the moving eye. That eye is owned by the rigid feature above.
        skinDonor=clean!=source||(Pull>0&&source.X<32);
        return clean;
    }
}

// Coordinates observed on the immutable 225px head, expressed in logical DIP.
// These are feature selections, not a replacement face or a new art source.
internal static class HighResolutionCheekSelection
{
    // Recover the exact same 24-step bisection cell, not an approximate curve
    // table. Newton/cuberoot locates it; original Bezier comparisons resolve
    // ties and the final midpoint is bit-identical to the legacy search.
    internal static double FastEdge(double y,double amount){
        var row=Math.Clamp((int)Math.Floor(y)-53,0,10);var a=CheekPullRenderer.Rows[row];var b=CheekPullRenderer.Rows[row+1];
        var initial=a.Edge+(b.Edge-a.Edge)*(y-a.Y);var upper=y<=57.5;
        var t=upper?Math.Cbrt(Math.Max(0,(y-53)/4.5)):Math.Clamp((y-57.5)/6.5,0,1);
        if(!upper)for(var i=0;i<6;i++)t-=(57.5+10.5*t-4.5*t*t+.5*t*t*t-y)/(10.5-9*t+1.5*t*t);
        const int n=1<<24;var cell=Math.Clamp((int)Math.Floor(t*n),0,n-1);
        double Y(int k)=>upper?Bezier(53,53,53,57.5,k/(double)n):Bezier(57.5,61,63,64,k/(double)n);
        while(cell>0&&Y(cell)>=y)cell--;
        while(cell<n-1&&Y(cell+1)<y)cell++;
        var u=(cell+.5)/n;
        var target=upper?Bezier(13,7,2,2,u):Bezier(2,2,10,19,u);
        return initial+(target-initial)*amount;
    }
    private static double Bezier(double a,double b,double c,double d,double t){var s=1-t;return a*s*s*s+3*b*s*s*t+3*c*s*t*t+d*t*t*t;}
    private static readonly double[] edges=[13.65,12.75,12.25,12.05,12.15,12.45,12.9,13.7,14.6,15.8,17.8,19.6];
    private static readonly (double Y,double Left,double Right)[] eye=
    [(44.7,20.8,23.7),(45.5,20.3,23.7),(46.5,19,24.3),(47.5,19.2,24.8),
     (48.5,18.6,25.5),(49.5,18.5,26.7),(50.5,18.0,28.0),(51.5,17.9,28.9),
     (52.5,17.9,29.2),(53.5,18.2,29.0),(54.5,18.7,28.4),
     (55.5,19.6,27.6),(56.5,20.7,26.4),(57.1,22,24.9)];
    internal static double Edge(double y){var i=Math.Clamp((int)Math.Floor(y)-53,0,10);return edges[i]+(edges[i+1]-edges[i])*(y-53-i);}
    internal static bool Mouth(Point p)=>p.X>=25&&p.X<=34.5&&p.Y>=57.2&&p.Y<=61.8;
    internal static bool Feature(Point p)=>Eye(p)||Mouth(p);
    internal static bool Eye(Point p){
        for(var i=1;i<eye.Length;i++)if(p.Y>=eye[i-1].Y&&p.Y<=eye[i].Y){
            var a=eye[i-1];var b=eye[i];var t=(p.Y-a.Y)/(b.Y-a.Y);
            return p.X>=a.Left+(b.Left-a.Left)*t&&p.X<=a.Right+(b.Right-a.Right)*t;
        }
        return false;
    }
    internal static Point Clean(Point p){
        if(Eye(p))return p.Y<48?new(p.X,p.Y-3):new(p.X<23?18:30,57);
        if(p.X>=25&&p.X<=34.5&&p.Y>=57.2&&p.Y<=61.8)return new(35,p.Y);
        return p;
    }
}

internal sealed class HighResolutionHuntBodies
{
    private readonly byte[] bytes;
    private readonly (int Offset,int Length)[] entries=new (int,int)[157];
    private readonly Dictionary<int,float[]> cache=new();
    internal HighResolutionHuntBodies(){
        using var s=typeof(HighResolutionHuntBodies).Assembly.GetManifestResourceStream("Dororong.App.Assets.high-resolution-hunt-body.bank")!;
        using var memory=new MemoryStream();s.CopyTo(memory);bytes=memory.ToArray();using var r=new BinaryReader(new MemoryStream(bytes));
        if(new string(r.ReadChars(4))!="DHF1"||r.ReadInt32()!=157)throw new InvalidDataException();
        for(var i=0;i<157;i++){var n=r.ReadInt32();if(n<=0||r.BaseStream.Position+n>bytes.Length)throw new InvalidDataException();entries[i]=((int)r.BaseStream.Position,n);r.BaseStream.Position+=n;}
    }
    internal float[] Get(int frame){
        if(cache.TryGetValue(frame,out var values))return values;
        var e=entries[frame];using var s=new GZipStream(new MemoryStream(bytes,e.Offset,e.Length),CompressionMode.Decompress);
        // Borrowed only for a synchronous raster call. Keep four scratch
        // sources and their ink outputs, rather than allocating each eviction.
        if(cache.Count>=4){var expired=cache.Keys.First();values=cache[expired];cache.Remove(expired);HighResolutionBodyInk.Invalidate(values);}
        else values=new float[225*225*4];
        s.ReadExactly(System.Runtime.InteropServices.MemoryMarshal.AsBytes(values.AsSpan()));
        cache[frame]=values;return values;
    }
}

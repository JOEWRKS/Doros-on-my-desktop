using System.Windows;
using System.Windows.Media;
using Dororong.Core.Geometry;

namespace Dororong.App.Controls;

internal sealed partial class HighResolutionGrabRaster
{
    internal readonly record struct PixelRegion(int Left,int Top,int Right,int Bottom)
    {
        internal bool IsEmpty=>Left>=Right||Top>=Bottom;
    }
    private readonly record struct OwnedKey(double Progress,Matrix Inverse,PointD Grab,PointD Near,PointD Far,
        double Spread,double Scale,HighResolutionFacePose Face,double BodyScale);
    // No extra image/cache ownership: only the caller's existing frame can be
    // reused, and only while its weak identity and all nonlocal inputs match.
    private readonly WeakReference<byte[]> _ownedReference=new(null!);
    private OwnedKey? _ownedKey;
    private PixelRegion? _renderRegion;
    internal PixelRegion? LastDirtyRegion {get;private set;}
    internal long LastRenderedPixelCount {get;private set;}
    internal int OwnedFrameAllocations {get;private set;}

    private static PixelRegion? ChangedRegion(OwnedKey before,OwnedKey after,int extent)
    {
        if(before==after)return new(0,0,0,0);
        // Every unhandled transition takes the original whole-frame path.
        // In particular material lag makes the destination-to-body map nonlinear.
        if(after.Near!=default||after.Far!=default||after.Spread!=0)return null;
        var face=after.Face;
        if(face.Pull!=0||face.EyeFollow!=0||face.HairFollow!=0||face.MouthAge>=0||
            face.DroolAge>=0||face.EffectFrame>=0||face.FlutterPhase>=0||face.Paw!=0)return null;
        if(before with {Face=before.Face with {EyeX=face.EyeX,EyeY=face.EyeY,Breathing=face.Breathing}}!=after)return null;

        var forward=after.Inverse;forward.Invert();
        var left=double.PositiveInfinity;var top=double.PositiveInfinity;
        var right=double.NegativeInfinity;var bottom=double.NegativeInfinity;
        void Corner(double x,double y,bool head){
            var p=new Point(x,y);if(head)p=face.Head.Transform(p);p=forward.Transform(p);
            var px=(p.X+80)*after.Scale;var py=(p.Y+80)*after.Scale;
            left=Math.Min(left,px);right=Math.Max(right,px);top=Math.Min(top,py);bottom=Math.Max(bottom,py);
        }
        void Include(double x0,double y0,double x1,double y1,bool head){
            Corner(x0,y0,head);Corner(x1,y0,head);Corner(x0,y1,head);Corner(x1,y1,head);
        }
        // These are bounds on affected DESTINATION samples, not texture texels.
        // Breathing's predicate is x[50,84),y[49,74); rigid gaze is clipped to
        // the fixed aperture x(16,45),y[46,63]. Include margins and every pixel
        // footprint crossing the transformed rectangle. No blur or AA changes.
        if(before.Face.Breathing!=face.Breathing)Include(49,48,85,75,false);
        if(before.Face.EyeX!=face.EyeX||before.Face.EyeY!=face.EyeY)Include(15,45,46,64,true);
        if(!double.IsFinite(left)||!double.IsFinite(top)||!double.IsFinite(right)||!double.IsFinite(bottom))return null;
        return new(Math.Clamp((int)Math.Floor(left)-1,0,extent),Math.Clamp((int)Math.Floor(top)-1,0,extent),
            Math.Clamp((int)Math.Ceiling(right)+1,0,extent),Math.Clamp((int)Math.Ceiling(bottom)+1,0,extent));
    }
}

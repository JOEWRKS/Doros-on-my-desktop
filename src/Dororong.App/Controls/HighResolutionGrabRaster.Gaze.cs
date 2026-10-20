using System.Windows;

namespace Dororong.App.Controls;

internal sealed partial class HighResolutionGrabRaster
{
    private static double EyeMatte(Point p,(double Y,double Left,double Right)[] rows){
        if(p.Y<=rows[0].Y||p.Y>=rows[^1].Y)return 0;
        var i=1;while(rows[i].Y<p.Y)i++;
        var a=rows[i-1];var b=rows[i];var t=(p.Y-a.Y)/(b.Y-a.Y);
        var distance=Math.Min(Math.Min(p.X-a.Left-(b.Left-a.Left)*t,a.Right+(b.Right-a.Right)*t-p.X),
            Math.Min(p.Y-rows[0].Y,rows[^1].Y-p.Y));
        return Smooth(distance/.35);
    }
    // The stationary hair opening, not the moving eye silhouette. Its top
    // follows the original bangs; the side edge remains owned by the hair.
    private static readonly (double X,double Y)[] leftOpening=
    [(16,52.8),(18,50.4),(20,48.2),(22,48.2),(24,49),(26,50.5),(28,52.2),(30,53.5)];
    private static readonly (double X,double Y)[] rightOpening=
    [(32.5,54.5),(34,53),(36,51.5),(38,51.2),(40,51.5),(42,52.1),(44,53.1),(45,54)];
    internal static double GazeAperture(Point p){
        var rows=p.X<31?leftOpening:rightOpening;
        if(p.X<=rows[0].X||p.X>=rows[^1].X||p.Y<46||p.Y>63)return 0;
        var i=1;while(rows[i].X<p.X)i++;
        var a=rows[i-1];var b=rows[i];var top=a.Y+(b.Y-a.Y)*(p.X-a.X)/(b.X-a.X);
        var edge=p.X<31?Math.Min(p.X-16,30-p.X):Math.Min(p.X-32.5,44.2-(p.Y-54)*.19-p.X);
        return Smooth((p.Y-top)/.4)*Smooth(edge/.4);
    }
    internal static Point GazeSource(Point p,double x,double y)=>new(p.X-x,p.Y-y);
    // Whole visible eye, including its lower/side ink. The user requires the
    // eye silhouette to follow direction, not colour sliding in a fixed rim.
    // Bounds include the ink's antialias fringe; falloff lies in surrounding
    // skin. The stationary hair aperture remains a separate foreground mask.
    private static readonly (double Y,double Left,double Right)[] eyeLeft=
    [(47.5,20,24),(48.5,18.8,25.5),(49.5,18,26.5),(50.5,17.8,27.5),
     (51.5,17.2,28),(52.5,17.2,28),(53.5,17.4,28),(54.5,18.1,27.4),
     (55.5,18.9,26.4),(56.5,19.7,25.1),(57.2,21,24.5)];
    private static readonly (double Y,double Left,double Right)[] eyeRight=
    [(50.8,38,40.8),(51.5,36.4,42),(52.5,35.4,43.1),(53.5,34.8,43.8),
     (54.5,34.8,44.3),(55.5,34.8,44.3),(56.5,34.8,44.3),(57.5,35.1,44),
     (58.5,35.8,43.2),(59.5,36.8,42.3),(60.5,38,41.3),(61,39,40.4)];
    private static double EyeLayer(Point p)=>EyeMatte(p,p.X<31?eyeLeft:eyeRight);
    internal static bool NearEyeFootprint(Point p){
        if(p.X<16||p.X>30||p.Y<46.8||p.Y>58)return false;
        // Fixed ownership covers the complete gaze sweep, including zero.
        // Switching a skin donor to original skin when a moving mask barely
        // touches it caused a one-pixel pop during cheek/gaze zero crossings.
        for(var iy=-1;iy<=1;iy++)for(var ix=-1;ix<=1;ix++)
            if(EyeLayer(new(p.X+.85*ix,p.Y+.65*iy))>0)return true;
        return false;
    }
    private void ApplyRigidGaze(Point p,double dx,double dy,Span<double> original,Span<double> moved){
        var clip=GazeAperture(p);if(clip==0)return;
        var q=GazeSource(p,dx,dy);
        var oldMask=EyeLayer(p);var newMask=EyeLayer(q);if(oldMask==0&&newMask==0)return;
        Sample(head,225,q.X*225/96-.5,q.Y*225/96-.5,moved);
        Span<double> skin=stackalloc double[4];
        // Continue the original skin immediately BELOW this eye, retaining
        // its pink/white shading. A single white donor bleached the vacated rim.
        var skinX=p.X<31?Math.Clamp(p.X,17,28):Math.Clamp(p.X,35,40.5);
        var skinY=p.X<31?57.25:61.5;
        Sample(head,225,skinX*225/96-.5,skinY*225/96-.5,skin);
        // Remove the ORIGINAL full contrast (including the old rim), then
        // transport only the selected eye contrast. Do not paste a skin/hair
        // rectangle with the eye. At zero displacement the terms cancel.
        for(var c=0;c<3;c++)original[c]=Math.Clamp(original[c]+clip*(newMask*(moved[c]-skin[c])-oldMask*(original[c]-skin[c])),0,original[3]);
    }
}

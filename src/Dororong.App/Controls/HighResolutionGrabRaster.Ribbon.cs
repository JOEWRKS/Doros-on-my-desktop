namespace Dororong.App.Controls;

internal sealed partial class HighResolutionGrabRaster
{
    // The 225px head separation retained a short body-ink spur beside the
    // right ribbon (rows110–115). Clip only its exterior to the adjoining
    // ribbon contour, once when preparing each head variant. Keep premultiplied
    // channels together; the original bank and body layers remain untouched.
    private static void TrimRibbonSpur(float[] pixels)
    {
        const int size = 225;
        for (var y = 109; y <= 116; y++)
        for (var x = 162; x <= 166; x++)
        {
            var keep = 0d;
            for (var sample = 0; sample < 8; sample++)
            {
                var sy = y + (sample + .5) / 8;
                var edge = 162 + .04 * (sy - 111) * (sy - 111);
                keep += Math.Clamp(edge - x, 0, 1) / 8;
            }
            var at = (y * size + x) * 4;
            for (var c = 0; c < 4; c++) pixels[at + c] *= (float)keep;
        }
        // Remove the inward-facing remnant at the same fold. Keep alpha and
        // the exterior edge intact; this strip lies inside the opaque head.
        for (var y = 109; y <= 114; y++)
        for (var x = 158; x <= 161; x++)
        {
            var ink = 0d;
            for (var sample = 0; sample < 8; sample++)
            {
                var sy = y + (sample + .5) / 8;
                var edge = 160 + .024 * (sy - 109) * (sy - 109);
                ink += Math.Clamp(x + 1 - edge, 0, 1) / 8;
            }
            var at = (y * 225 + x) * 4;
            for (var c = 0; c < 3; c++) pixels[at+c] = (float)(255-219*ink);
        }
        ConnectRibbonFold(pixels);
    }

    // Follow the user's red line: one shallow fold from the ribbon on the
    // left into the right contour, with the old downward dangling tip removed.
    private static void ConnectRibbonFold(float[] pixels)
    {
        static double Distance(double px, double py, double ax, double ay, double bx, double by)
        {
            var dx=bx-ax; var dy=by-ay;
            var t=Math.Clamp(((px-ax)*dx+(py-ay)*dy)/(dx*dx+dy*dy),0,1);
            return Math.Sqrt(Math.Pow(px-ax-t*dx,2)+Math.Pow(py-ay-t*dy,2));
        }
        for (var y = 107; y <= 114; y++)
        for (var x = 153; x <= 161; x++)
        {
            var coverage = 0d;
            var erase = 0d;
            for (var sy = 0; sy < 8; sy++)
            for (var sx = 0; sx < 8; sx++)
            {
                var px = x + (sx + .5) / 8;
                var py = y + (sy + .5) / 8;
                var oldDistance=Distance(px,py,152.1,107.5,156.8,112.55);
                if(x<=158)erase+=Math.Clamp(2-oldDistance,0,1)/64;
                if(Distance(px,py,151.8,106.8,161.25,110.6)<.72)coverage+=1d/64;
            }
            var at = (y * 225 + x) * 4;
            for (var c = 0; c < 3; c++)
            {
                var cleared=pixels[at+c]*(1-erase)+255*erase;
                pixels[at+c]=(float)(cleared*(1-coverage)+36*coverage);
            }
        }
    }
}

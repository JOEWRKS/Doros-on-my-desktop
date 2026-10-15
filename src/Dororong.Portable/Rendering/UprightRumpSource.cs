
namespace Dororong.Portable.Rendering;

// Preserve authored ink, rather than fitting a second rear silhouette.
internal static class UprightRumpSource
{
    private static readonly RasterImage Open = Load("dororong-canonical.png");
    private static readonly RasterImage Closed = Load("dororong-closed-eyes.png");
    private static readonly byte[] Pixels = Open.CopyPixels();
    internal static RasterImage Standing(bool closed) => closed ? Closed : Open;
    internal static bool Contains(object? source) => ReferenceEquals(source,Open)||ReferenceEquals(source,Closed)||BodyBreathingFrame.Contains(source);

    private static RasterImage Load(string name) => RasterImage.Load(name,clean:true);

    internal static void Apply(byte[] composite,double amount,byte[]? joins=null)
    {
        var upright=1-Smooth(amount/.2);if(upright==0)return;
        var head=HuntFrames.Instance.Head(0).Span;
        // The entire upright body, not only a rump patch, shares the canonical
        // endpoint. Fade its authority geometrically as the crouch develops.
        // Hidden articulated neck joins remain under head-owned texels.
        for(var yy=32;yy<224;yy++)for(var xx=32;xx<224;xx++)
        {
            var x=(xx-32)/2;var y=(yy-32)/2;
            var source=(y*96+x)*4;var target=(yy*256+xx)*4;
            if(head[source+3]>0 && joins is null)continue;
            for(var c=0;c<4;c++)
            {
                var value=head[source+3]>0?joins![((y+16)*128+x+16)*4+c]:Pixels[source+c];
                composite[target+c]=(byte)Math.Round(composite[target+c]+(value-composite[target+c])*upright);
            }
        }
    }

    private static double Smooth(double value){var t=Math.Clamp(value,0,1);return t*t*(3-2*t);}
}


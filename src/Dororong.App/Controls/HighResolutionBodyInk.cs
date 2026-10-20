using System.Runtime.CompilerServices;

namespace Dororong.App.Controls;

// Display-only stroke compensation. Immutable artwork and head textures are
// never changed. A half-width retreat on either side keeps body ink close to
// its 1x physical width while the body itself continues to scale normally.
internal static class HighResolutionBodyInk
{
    private sealed class Entry(double scale,float[] pixels){
        internal double Scale=scale;
        internal readonly float[] Pixels=pixels;
        internal bool Valid=true;
    }
    private static readonly ConditionalWeakTable<float[],Entry> cache=new();
    // A reused source buffer owns its derived scratch until the next explicit
    // invalidation. Previously this discarded a large output on every frame.
    internal static void Invalidate(float[] source){
        RasterAlphaBounds.Invalidate(source);
        if(cache.TryGetValue(source,out var entry))entry.Valid=false;
    }
    internal static float[] AtScale(float[] source,int size,double scale){
        if(scale<=1)return source;
        if(cache.TryGetValue(source,out var saved)&&saved.Valid&&saved.Scale==scale)return saved.Pixels;
        var radius=.5*1.295415959252971*(1-1/scale)*(size==160?1:225d/96);
        // The eight offsets depend on scale, not on the source pixel. Retain
        // the original arithmetic/order; don't replace these with rounded
        // diagonal constants (that can move a one-byte antialias boundary).
        var offsets=new (double X,double Y)[8];
        for(var i=0;i<8;i++){var angle=i*Math.PI/4;offsets[i]=(radius*Math.Cos(angle),radius*Math.Sin(angle));}
        var output=saved?.Pixels??new float[source.Length];
        if(saved is not null){Array.Clear(output);RasterAlphaBounds.Invalidate(output);}
        (double Alpha,double Ink) Pixel(int x,int y){
            if(x<0||y<0||x>=size||y>=size)return (0,1);
            var at=(y*size+x)*4;var a=source[at+3];
            return (a,a<.001?1:1-(source[at]+source[at+1]+source[at+2])/(3*a));
        }
        (double Alpha,double Ink) Lookup(double x,double y){
            var ix=(int)Math.Floor(x);var iy=(int)Math.Floor(y);var fx=x-ix;var fy=y-iy;
            double a=0,k=0;
            for(var v=0;v<2;v++)for(var u=0;u<2;u++){
                var p=Pixel(ix+u,iy+v);var w=(u==0?1-fx:fx)*(v==0?1-fy:fy);a+=p.Alpha*w;k+=p.Ink*w;
            }
            return (a,k);
        }
        for(var y=0;y<size;y++)for(var x=0;x<size;x++){
            var at=(y*size+x)*4;var original=Pixel(x,y);if(original.Alpha==0)continue;
            if(original.Alpha==255&&original.Ink==0&&x>0&&y>0&&x<size-1&&y<size-1){
                var opaque=true;
                for(var dy=-1;dy<=1&&opaque;dy++)for(var dx=-1;dx<=1;dx++)
                    if(source[((y+dy)*size+x+dx)*4+3]!=255){opaque=false;break;}
                if(opaque){Array.Copy(source,at,output,at,4);continue;}
            }
            var alpha=original.Alpha;var ink=original.Ink;
            for(var i=0;i<8;i++){
                var p=Lookup(x+offsets[i].X,y+offsets[i].Y);
                alpha=Math.Min(alpha,p.Alpha);ink=Math.Min(ink,p.Ink);
            }
            var ratio=original.Ink>1e-6?ink/original.Ink:1;
            for(var c=0;c<3;c++)output[at+c]=(float)((1-(1-source[at+c]/original.Alpha)*ratio)*alpha);
            output[at+3]=(float)alpha;
        }
        // A mutable scratch source already owns this entry. Removing/adding
        // its weak key on every update creates tombstones and replacement
        // table containers even though the source and output are unchanged.
        if(saved is null)cache.Add(source,new(scale,output));
        else{saved.Scale=scale;saved.Valid=true;}
        return output;
    }
}

using System.IO.Compression;

namespace Dororong.Portable.Rendering;

// Only WPF-authored hidden neck/shoulder coverage, not whole animation frames.
// Continuous rolls interpolate adjacent 0.00025-radian coverage samples. The head
// retains its unquantized affine and original single-pass sampling.
internal sealed class HuntJoinBank
{
    private sealed record Patch(int X,int Y,int Width,int Height,byte[] Pixels)
    {
        internal void CopyTo(byte[] target)
        {for(var y=0;y<Height;y++)Array.Copy(Pixels,y*Width*4,target,((y+Y)*128+X)*4,Width*4);}
    }
    internal static HuntJoinBank Instance {get;}=new();
    private readonly byte[][] compressed=new byte[157*88][];
    private readonly Dictionary<int,Patch[]> cache=[];
    private readonly Queue<int> order=[];
    private readonly object gate=new();
    private HuntJoinBank()
    {
        using var stream=RasterImage.Resource("hunt-joins.bin");using var reader=new BinaryReader(stream);
        if(new string(reader.ReadChars(4))!="JOIN"||reader.ReadInt32()!=3||reader.ReadInt32()!=157||reader.ReadInt32()!=2801)
            throw new InvalidDataException("Invalid neck coverage bank.");
        if(Convert.ToHexString(reader.ReadBytes(32))!="EF1603705D1CB6073E9D0DBFF8562EAC94E8D4EC920B29A9A9FEB47319E6274C")
            throw new InvalidDataException("Unapproved neck coverage source.");
        for(var f=0;f<compressed.Length;f++)
        {
            var n=reader.ReadInt32();if(n<1||n>2_000_000)throw new InvalidDataException("Invalid coverage length.");
            compressed[f]=reader.ReadBytes(n);if(compressed[f].Length!=n)throw new EndOfStreamException();
        }
        if(stream.ReadByte()!=-1)throw new InvalidDataException("Unexpected coverage bytes.");
    }
    internal byte[] Render(int frame,double roll)
    {
        if(frame<0||frame>156||!double.IsFinite(roll)||roll<-.3500001||roll>.3500001)throw new ArgumentOutOfRangeException(nameof(roll));
        var t=Math.Clamp(roll*4000+1400,0,2800);var lo=(int)Math.Floor(t);var hi=Math.Min(2800,lo+1);var mix=t-lo;
        Patch first,second;lock(gate){first=Get(frame,lo);second=mix>1e-10?Get(frame,hi):first;}
        var result=new byte[65536];first.CopyTo(result);
        if(mix>1e-10){var next=new byte[65536];second.CopyTo(next);for(var i=0;i<result.Length;i++)result[i]=(byte)Math.Round(result[i]+(next[i]-result[i])*mix);}
        return result;
    }
    private Patch Get(int frame,int sample)
    {
        // Decode only the needed32-sample block. Whole-frame decode allocated
        // tens of megabytes on every pose change and would stall native motion.
        var block=sample/32;var key=frame*88+block;
        if(!cache.TryGetValue(key,out var patches))
        {
            patches=new Patch[Math.Min(32,2801-block*32)];using var raw=new MemoryStream(compressed[key]);using var zip=new GZipStream(raw,CompressionMode.Decompress);using var reader=new BinaryReader(zip);
            for(var i=0;i<patches.Length;i++)
            {
                var x=reader.ReadByte();var y=reader.ReadByte();var w=reader.ReadByte();var h=reader.ReadByte();
                if(w==0||h==0||x+w>128||y+h>128)throw new InvalidDataException("Invalid coverage bounds.");
                var data=new byte[w*h*4];zip.ReadExactly(data);patches[i]=new(x,y,w,h,data);
            }
            if(zip.ReadByte()!=-1)throw new InvalidDataException("Unexpected decoded coverage.");
            if(cache.Count==8)cache.Remove(order.Dequeue());cache.Add(key,patches);order.Enqueue(key);
        }
        return patches[sample%32];
    }
}

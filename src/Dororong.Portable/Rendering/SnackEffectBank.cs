using System.IO.Compression;

namespace Dororong.Portable.Rendering;

// Small transparent overlays only. Native WPF vector coverage and jewel filtering
// are reused without filtering the character or snapping the continuous clock.
internal sealed class SnackEffectBank
{
    private sealed record Patch(int X,int Y,int W,int H,byte[] Data)
    {
        internal void CopyTo(byte[] target)
        {for(var y=0;y<H;y++)Array.Copy(Data,y*W*4,target,((y+Y)*96+X)*4,W*4);}
    }
    internal static SnackEffectBank Instance {get;}=new();
    private const int BlockSize=32;
    private readonly byte[][][] compressed=new byte[590][][];
    private readonly Dictionary<int,Patch[]> cache=[];private readonly Queue<int> order=[];
    private readonly object gate=new();
    private SnackEffectBank()
    {
        using var stream=RasterImage.Resource("snack-effects.bin");using var r=new BinaryReader(stream);
        if(new string(r.ReadChars(4))!="EFCT"||r.ReadInt32()!=3||r.ReadInt32()!=590||r.ReadInt32()!=BlockSize||
            Convert.ToHexString(r.ReadBytes(32))!="EF1603705D1CB6073E9D0DBFF8562EAC94E8D4EC920B29A9A9FEB47319E6274C")throw new InvalidDataException("Invalid effect source.");
        for(var group=0;group<590;group++)
        {
            compressed[group]=new byte[(Count(group)+BlockSize-1)/BlockSize][];
            for(var block=0;block<compressed[group].Length;block++)
            {
                var n=r.ReadInt32();if(n<1||n>1_000_000)throw new InvalidDataException("Invalid effect size.");
                compressed[group][block]=r.ReadBytes(n);if(compressed[group][block].Length!=n)throw new EndOfStreamException();
            }
        }
        if(stream.ReadByte()!=-1)throw new InvalidDataException("Unexpected effect bytes.");
    }
    internal byte[] Render(bool eating,double age,int transform)
    {
        if(!double.IsFinite(age)||age<0||transform<0||transform>=589)throw new ArgumentOutOfRangeException(nameof(age));
        if(eating&&age>=.48)return new byte[36864];
        var group=eating?589:transform;
        var t=eating?age*100000:200*(1+Math.Sin(age*4));t=Math.Clamp(t,0,Count(group)-1);
        var lo=(int)Math.Floor(t);var hi=Math.Min(lo+1,Count(group)-1);var mix=t-lo;
        Patch first,second;lock(gate){first=Get(group,lo);second=mix>1e-10?Get(group,hi):first;}
        var result=new byte[36864];first.CopyTo(result);
        if(mix>1e-10){var next=new byte[36864];second.CopyTo(next);for(var i=0;i<result.Length;i++)result[i]=(byte)Math.Round(result[i]+(next[i]-result[i])*mix);}
        return result;
    }
    private static int Count(int group)=>group==589?48001:401;
    private Patch Get(int group,int sample)
    {
        // Decode only adjacent32-sample blocks, including across boundaries.
        // Whole eating-group decode stalled the UI for ~0.5s on every cache miss.
        var block=sample/BlockSize;var key=group*1501+block;
        if(!cache.TryGetValue(key,out var patches))
        {
            using var input=new MemoryStream(compressed[group][block]);using var zip=new GZipStream(input,CompressionMode.Decompress);using var r=new BinaryReader(zip);
            patches=new Patch[Math.Min(BlockSize,Count(group)-block*BlockSize)];
            for(var i=0;i<patches.Length;i++)
            {var x=r.ReadByte();var y=r.ReadByte();var w=r.ReadByte();var h=r.ReadByte();if(x+w>96||y+h>96)throw new InvalidDataException("Invalid effect bounds.");var data=new byte[w*h*4];zip.ReadExactly(data);patches[i]=new(x,y,w,h,data);}
            if(zip.ReadByte()!=-1)throw new InvalidDataException("Unexpected decoded effects.");
            if(cache.Count==8)cache.Remove(order.Dequeue());cache.Add(key,patches);order.Enqueue(key);
        }
        return patches[sample%BlockSize];
    }
}

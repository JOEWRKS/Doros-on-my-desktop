using System.Collections;
using System.IO.Compression;
using System.Reflection;
using Dororong.Portable.Rendering;

namespace Dororong.Portable.Tests;

public sealed class SnackEffectBlockTests
{
    private sealed record Patch(int X,int Y,int W,int H,byte[] Data)
    {
        public byte[] Expand()
        {
            var result=new byte[36864];
            for(var y=0;y<H;y++)Array.Copy(Data,y*W*4,result,((Y+y)*96+X)*4,W*4);
            return result;
        }
    }
    [Fact]
    public void AllGroupsAndEatingBlockBoundariesRetainOriginalInterpolation()
    {
        using var stream=typeof(SnackRenderer).Assembly.GetManifestResourceStream("snack-effects.bin")!;
        using var reader=new BinaryReader(stream);
        Assert.Equal("EFCT",new string(reader.ReadChars(4)));Assert.Equal(3,reader.ReadInt32());
        Assert.Equal(590,reader.ReadInt32());Assert.Equal(32,reader.ReadInt32());Assert.Equal(32,reader.ReadBytes(32).Length);
        for(var group=0;group<590;group++)
        {
            var count=group==589?48001:401;var patches=new List<Patch>();
            for(var offset=0;offset<count;offset+=32)
            {
                using var input=new MemoryStream(reader.ReadBytes(reader.ReadInt32()));
                using var zip=new GZipStream(input,CompressionMode.Decompress);using var raw=new BinaryReader(zip);
                for(var i=offset;i<Math.Min(offset+32,count);i++)
                {
                    var x=raw.ReadByte();var y=raw.ReadByte();var w=raw.ReadByte();var h=raw.ReadByte();
                    var data=raw.ReadBytes(w*h*4);Assert.Equal(w*h*4,data.Length);patches.Add(new(x,y,w,h,data));
                }
                Assert.Equal(-1,zip.ReadByte());
            }
            var samples=group==589
                ? Enumerable.Range(1,1500).Select(i=>i*32-.5).Concat(new[]{0d,47999.75})
                : new[]{0d,31.5,399.75,400d};
            foreach(var sample in samples)
            {
                var eating=group==589;
                var age=eating?sample/100000:(Math.Asin(sample/200-1)+2*Math.PI)/4;
                // Identical to the former v2 interpolation, including floating-point ties.
                var t=eating?age*100000:200*(1+Math.Sin(age*4));t=Math.Clamp(t,0,count-1);
                var lo=(int)Math.Floor(t);var hi=Math.Min(lo+1,count-1);var mix=t-lo;
                var expected=patches[lo].Expand();
                if(mix>1e-10)
                {
                    var next=patches[hi].Expand();
                    for(var i=0;i<expected.Length;i++)expected[i]=(byte)Math.Round(expected[i]+(next[i]-expected[i])*mix);
                }
                Assert.Equal(expected,SnackEffectBank.Instance.Render(eating,age,eating?156:group));
            }
        }
        Assert.Equal(-1,stream.ReadByte());
        Assert.All(SnackEffectBank.Instance.Render(true,.48,156),b=>Assert.Equal(0,b));
    }
    [Fact]
    public void ColdAndEvictedEatingReadsStayBounded()
    {
        var bank=(SnackEffectBank)Activator.CreateInstance(typeof(SnackEffectBank),nonPublic:true)!;
        for(var pass=0;pass<2;pass++)
        {
            for(var group=0;group<12;group++)bank.Render(false,.1,group);
            var start=GC.GetAllocatedBytesForCurrentThread();
            bank.Render(true,.000315,156); // lo31/hi32 spans two compressed blocks.
            var allocated=GC.GetAllocatedBytesForCurrentThread()-start;
            Assert.InRange(allocated,1,500_000); // v2 allocated ~15.9MB.
            var cache=(IDictionary)typeof(SnackEffectBank).GetField("cache",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(bank)!;
            Assert.InRange(cache.Count,1,8);
        }
    }
    [Fact]
    public void SharedCacheConcurrentReadersProduceSamePixels()
    {
        var bank=SnackEffectBank.Instance;var expected=bank.Render(true,.000315,156);
        Parallel.For(0,64,i=>
        {
            bank.Render(false,i*.017,i%589);
            Assert.Equal(expected,bank.Render(true,.000315,156));
        });
    }
}

using System.IO;
using System.IO.Compression;
namespace Dororong.App.Controls;

// Small index only at construction. Independent lossless blocks avoid decoding
// or creating bitmaps for preceding/unrequested frames.
internal sealed class IndexedMotionBank
{
    private readonly Func<Stream> open;
    private readonly (int Offset,int Length,int Raw)[] entries;
    private int decoded;
    internal int DecodedCount => Volatile.Read(ref decoded);
    internal IndexedMotionBank(string name,int[] sizes) : this(
        () => typeof(IndexedMotionBank).Assembly.GetManifestResourceStream("Dororong.App.Assets."+name+".indexed")
            ?? throw new InvalidDataException("Missing indexed motion bank."), sizes) { }
    internal IndexedMotionBank(Func<Stream> open,int[] sizes)
    {
        this.open=open;
        using var stream=open();using var reader=new BinaryReader(stream);
        if(!stream.CanSeek || new string(reader.ReadChars(4))!="IMBK" || reader.ReadInt32()!=1 || reader.ReadInt32()!=sizes.Length)
            throw new InvalidDataException("Invalid indexed motion header.");
        entries=new (int,int,int)[sizes.Length];long end=12L+sizes.Length*12L;
        for(var i=0;i<sizes.Length;i++)
        {
            var offset=reader.ReadInt32();var length=reader.ReadInt32();var raw=reader.ReadInt32();
            if(offset!=end || length<=0 || length>4*1024*1024 || raw!=sizes[i] || raw<=0 || raw>4*1024*1024)
                throw new InvalidDataException("Invalid indexed motion entry.");
            end=(long)offset+length;if(end>stream.Length)throw new InvalidDataException("Truncated motion bank.");
            entries[i]=(offset,length,raw);
        }
        if(end!=stream.Length)throw new InvalidDataException("Unexpected motion bank data.");
    }
    internal byte[] Read(int index)
    {
        var entry=entries[index];using var stream=open();stream.Position=entry.Offset;
        var compressed=new byte[entry.Length];stream.ReadExactly(compressed);
        using var input=new MemoryStream(compressed,false);using var gzip=new GZipStream(input,CompressionMode.Decompress);
        var raw=new byte[entry.Raw];gzip.ReadExactly(raw);
        if(gzip.ReadByte()!=-1)throw new InvalidDataException("Unexpected frame data.");
        Interlocked.Increment(ref decoded);return raw;
    }
}

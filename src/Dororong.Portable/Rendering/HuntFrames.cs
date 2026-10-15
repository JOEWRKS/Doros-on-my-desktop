using System.IO;
using System.IO.Compression;

namespace Dororong.Portable.Rendering;

internal sealed class HuntFrames
{
    internal static HuntFrames Instance { get; } = new();
    private byte[][] Heads { get; } = new byte[2][];
    private byte[][] Bodies {get;}=new byte[157][];
    internal RasterImage Body(int frame)=>new(256,256,Bodies[frame]);
    internal ReadOnlyMemory<byte> Head(int index) => Heads[index];

    private HuntFrames()
    {
        using var resource = typeof(HuntFrames).Assembly.GetManifestResourceStream("Dororong.App.Assets.hunting.pbgra.gz")
            ?? throw new InvalidDataException("Missing approved hunting layers.");
        using var gzip = new GZipStream(resource, CompressionMode.Decompress);
        using var reader = new BinaryReader(gzip);
        if (new string(reader.ReadChars(4)) != "HUNT" || reader.ReadUInt32() != 1 || reader.ReadUInt32() != 256 ||
            reader.ReadUInt32() != 256 || reader.ReadUInt32() != 157) throw new InvalidDataException("Invalid hunting bank.");
        byte[] Read(int count)
        {
            var data = reader.ReadBytes(count);
            return data.Length == count ? data : throw new InvalidDataException("Truncated hunting bank.");
        }
        // Current articulated hunting uses these authored working surfaces.
        var discard = new byte[384 * 384 * 4];
        for (var i = 0; i < 157; i++) Bodies[i]=Read(256*256*4);
        for (var i = 0; i < 2; i++) { Heads[i] = Read(96 * 96 * 4); gzip.ReadExactly(discard); }
        if (gzip.ReadByte() != -1) throw new InvalidDataException("Unexpected hunting payload.");
    }
}


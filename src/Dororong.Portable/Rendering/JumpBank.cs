using System.IO.Compression;

namespace Dororong.Portable.Rendering;

internal sealed class JumpBank
{
    private readonly RasterImage[] frames = new RasterImage[157];
    internal string BaselineHash { get; }
    internal static JumpBank Instance { get; } = new();
    private JumpBank()
    {
        using var resource=RasterImage.Resource("jump-native.pbgra.gz");
        using var gzip=new GZipStream(resource,CompressionMode.Decompress);
        using var reader=new BinaryReader(gzip);
        if(new string(reader.ReadChars(4))!="JUMP" || reader.ReadInt32()!=1 || reader.ReadInt32()!=96 || reader.ReadInt32()!=96 || reader.ReadInt32()!=157)
            throw new InvalidDataException("Invalid current jump bank.");
        var hash=new byte[32]; gzip.ReadExactly(hash); BaselineHash=Convert.ToHexString(hash);
        var pixels=new byte[96*96*4];
        for(var i=0;i<frames.Length;i++){gzip.ReadExactly(pixels);frames[i]=new(96,96,pixels);}
        if(gzip.ReadByte()!=-1)throw new InvalidDataException("Unexpected jump payload.");
    }
    internal RasterImage Frame(int frame) => frames[frame];
}

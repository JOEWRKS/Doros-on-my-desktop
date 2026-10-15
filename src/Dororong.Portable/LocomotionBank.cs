using System.IO.Compression;
using System.Text;

namespace Dororong.Portable;

/// <summary>Exact approved LOCO v2 bytes, without WPF or platform bitmap APIs.</summary>
public sealed class LocomotionBank
{
    public const int Width = 96, Height = 96, Stride = Width * 4;
    public const int EyeFrames = 65 + 8 * 32 + 65;
    public const int FrameCount = EyeFrames * 2;
    public const int FrameBytes = Stride * Height;
    private readonly byte[] pixels;
    private LocomotionBank(byte[] pixels) => this.pixels = pixels;

    public static LocomotionBank Load(Stream compressed)
    {
        ArgumentNullException.ThrowIfNull(compressed);
        using var gzip = new GZipStream(compressed, CompressionMode.Decompress, leaveOpen: true);
        using var reader = new BinaryReader(gzip, Encoding.ASCII, leaveOpen: true);
        try
        {
            if (Encoding.ASCII.GetString(reader.ReadBytes(4)) != "LOCO" ||
                reader.ReadUInt32() != 2 || reader.ReadUInt32() != Width ||
                reader.ReadUInt32() != Height || reader.ReadUInt32() != FrameCount)
                throw new InvalidDataException("Invalid approved locomotion geometry/version.");
            // Fixed maximum: never allocate based on file-provided dimensions/counts.
            var bytes = new byte[FrameCount * FrameBytes];
            gzip.ReadExactly(bytes);
            if (gzip.ReadByte() != -1) throw new InvalidDataException("Unexpected locomotion payload.");
            return new(bytes);
        }
        catch (EndOfStreamException e) { throw new InvalidDataException("Truncated locomotion bank.", e); }
    }

    public ReadOnlyMemory<byte> Frame(int index)
    {
        if (index < 0 || index >= FrameCount) throw new ArgumentOutOfRangeException(nameof(index));
        return pixels.AsMemory(index * FrameBytes, FrameBytes);
    }

    // The neutral production endpoint uses canonical.png, not bank frame zero.
    // Caller must supply that endpoint separately. Do not use this bank for current walking.
    public ReadOnlyMemory<byte> Sit(double amount, bool closed = false, bool rising = false)
    {
        if (!double.IsFinite(amount)) throw new ArgumentOutOfRangeException(nameof(amount));
        var progress = Math.Clamp(amount, 0, 1);
        var index = rising && progress > 0 && progress < 1
            ? 65 + 8 * 32 + (int)Math.Round((1 - progress) * 64)
            : (int)Math.Round(progress * 64);
        return Frame((closed ? EyeFrames : 0) + index);
    }
}

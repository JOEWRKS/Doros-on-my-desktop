using System.IO.Compression;
using System.Text;
using Dororong.Portable;

namespace Dororong.Portable.Tests;

public sealed class AssetParityTests
{
    [Fact]
    public void EveryApprovedFrameIsByteIdentical()
    {
        using var input = Resource();
        var bank = LocomotionBank.Load(input);
        Assert.True(input.CanRead); // Caller retains stream ownership.
        using var reference = new GZipStream(Resource(), CompressionMode.Decompress);
        var header = new byte[20]; reference.ReadExactly(header);
        var expected = new byte[LocomotionBank.FrameBytes];
        for (var n = 0; n < LocomotionBank.FrameCount; n++)
        {
            reference.ReadExactly(expected);
            Assert.True(expected.AsSpan().SequenceEqual(bank.Frame(n).Span), $"Frame {n}");
        }
        Assert.Equal(-1, reference.ReadByte());
    }

    [Fact]
    public void SittingAndRisingUseProductionIndices()
    {
        using var input = Resource(); var bank = LocomotionBank.Load(input);
        Assert.True(bank.Frame(64).Span.SequenceEqual(bank.Sit(1).Span));
        Assert.True(bank.Frame(LocomotionBank.EyeFrames + 32).Span.SequenceEqual(bank.Sit(.5, true).Span));
        Assert.True(bank.Frame(65 + 8 * 32 + 32).Span.SequenceEqual(bank.Sit(.5, rising: true).Span));
        Assert.Throws<ArgumentOutOfRangeException>(() => bank.Sit(double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => bank.Frame(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => bank.Frame(LocomotionBank.FrameCount));
    }

    [Theory]
    [InlineData(1, 96, 96, 772)]
    [InlineData(2, 4096, 96, 772)]
    [InlineData(2, 96, 96, int.MaxValue)]
    public void RejectsUntrustedGeometryBeforeAllocation(int version, int width, int height, int count)
    {
        using var data = Compressed(writer => Header(writer, version, width, height, count));
        Assert.Throws<InvalidDataException>(() => LocomotionBank.Load(data));
    }

    [Fact]
    public void RejectsTruncatedPayload()
    {
        using var data = Compressed(writer => Header(writer, 2, 96, 96, 772));
        Assert.Throws<InvalidDataException>(() => LocomotionBank.Load(data));
    }

    [Fact]
    public void RejectsTrailingDecompressedBytes()
    {
        using var data = Compressed(writer => {
            Header(writer, 2, 96, 96, 772);
            writer.Write(new byte[LocomotionBank.FrameCount * LocomotionBank.FrameBytes]);
            writer.Write((byte)1);
        });
        Assert.Throws<InvalidDataException>(() => LocomotionBank.Load(data));
    }

    [Fact]
    public void CanonicalCleanupPreservesConnectedInkAndInput()
    {
        var input = new byte[LocomotionBank.FrameBytes];
        void Pixel(int x, int y, byte alpha) { var i = (y * 96 + x) * 4; input[i] = 120; input[i + 3] = alpha; }
        Pixel(70, 70, 20); // Isolated artifact.
        Pixel(80, 80, 39); Pixel(81, 80, 40); // Connected antialiasing.
        Pixel(40, 40, 20); // Outside bounded cleanup.
        var clean = CanonicalPixels.Clean(input);
        Assert.Equal(0, clean[(70 * 96 + 70) * 4 + 3]);
        Assert.Equal(20, input[(70 * 96 + 70) * 4 + 3]);
        Assert.Equal(39, clean[(80 * 96 + 80) * 4 + 3]);
        Assert.Equal(20, clean[(40 * 96 + 40) * 4 + 3]);
        Assert.Throws<ArgumentException>(() => CanonicalPixels.Clean(new byte[4]));
    }

    private static Stream Resource() => typeof(AssetParityTests).Assembly.GetManifestResourceStream("approved.locomotion")!;
    private static void Header(BinaryWriter writer, int version, int width, int height, int count)
    {
        writer.Write(Encoding.ASCII.GetBytes("LOCO")); writer.Write(version);
        writer.Write(width); writer.Write(height); writer.Write(count);
    }
    private static MemoryStream Compressed(Action<BinaryWriter> fill)
    {
        var result = new MemoryStream();
        using (var gzip = new GZipStream(result, CompressionMode.Compress, leaveOpen: true))
        using (var writer = new BinaryWriter(gzip)) fill(writer);
        result.Position = 0; return result;
    }
}

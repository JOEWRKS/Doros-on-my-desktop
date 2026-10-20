using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Dororong.App.Controls;

internal sealed class HuntFrames
{
    internal static HuntFrames Instance { get; } = new();
    private BitmapSource?[] Bodies { get; } = new BitmapSource?[157];
    private byte[][] Heads { get; } = new byte[2][];
    private byte[][] EnlargedHeads { get; } = new byte[2][];
    private readonly object gate = new();
    private readonly IndexedMotionBank bank = new("hunting", Enumerable.Repeat(256*256*4,157)
        .Concat(new[]{96*96*4,384*384*4,96*96*4,384*384*4}).ToArray());
    internal int DecodedCount => bank.DecodedCount;
    internal BitmapSource Body(int index)
    {
        lock(gate)
        {
            if(Bodies[index] is {} cached)return cached;
            var image=BitmapSource.Create(256,256,96,96,PixelFormats.Pbgra32,null,bank.Read(index),1024);
            image.Freeze();return Bodies[index]=image;
        }
    }
    internal ReadOnlyMemory<byte> Head(int index)
    {lock(gate)return Heads[index]??=bank.Read(157+index*2);}
    internal void CopyEnlargedHead(int index, Span<byte> destination)
    {lock(gate)(EnlargedHeads[index]??=bank.Read(158+index*2)).AsSpan().CopyTo(destination);}
}

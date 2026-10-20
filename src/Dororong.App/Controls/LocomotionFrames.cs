using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Dororong.App.Controls;

internal static class LocomotionFrames
{
    private const int EyeFrames = 65 + 8 * 32 + 65;
    private static readonly HashSet<BitmapSource> Frames = new();
    internal static IReadOnlyList<BitmapSource> All { get; } = new LazyFrames();
    private static readonly Dictionary<(int Level,int Phase,bool Closed),BitmapSource> WalkFrames = new();

    internal static bool Contains(object? frame) { lock(Frames)return frame is BitmapSource bitmap && Frames.Contains(bitmap); }

    internal static BitmapSource Sit(double amount, bool closed = false, bool rising = false)
    {
        Validate(amount);
        var progress = Math.Clamp(amount, 0, 1);
        if(progress==0) return UprightRumpSource.Standing(closed);
        // Shared endpoint objects also preserve frozen interaction captures.
        if (rising && progress > 0 && progress < 1)
            return All[(closed ? EyeFrames : 0) + 65 + 8 * 32 + (int)Math.Round((1-progress) * 64)];
        return All[(closed ? EyeFrames : 0) + (int)Math.Round(progress * 64)];
    }

    internal static BitmapSource Walk(double amplitude, double distance, bool closed = false)
    {
        Validate(amplitude);
        Validate(distance);
        var level = (int)Math.Round(Math.Clamp(amplitude, 0, 1) * 8);
        if (level == 0) return Sit(0, closed);
        var phase = (int)Math.Round(((distance % 10 + 10) % 10) / 10 * 32) % 32;
        var key=(level,phase,closed);
        if(!WalkFrames.TryGetValue(key,out var image))
        {
            image=SnackRunFrames.Render(phase*10d/32,level/8d,walking:true,closed:closed);
            WalkFrames.Add(key,image);lock(Frames)Frames.Add(image);
        }
        return image;
    }

    private static void Validate(double value)
    {
        if (!double.IsFinite(value)) throw new ArgumentOutOfRangeException(nameof(value));
    }

    internal sealed class LazyFrames : IReadOnlyList<BitmapSource>
    {
        private readonly IndexedMotionBank bank=new("locomotion",Enumerable.Repeat(96*96*4,EyeFrames*2).ToArray());
        private readonly BitmapSource?[] frames=new BitmapSource?[EyeFrames*2];
        internal int DecodedCount=>bank.DecodedCount;
        public int Count=>frames.Length;
        public BitmapSource this[int index]
        {
            get { lock(frames)
            {
                if(frames[index] is {} cached)return cached;
                var image=BitmapSource.Create(96,96,96,96,PixelFormats.Pbgra32,null,bank.Read(index),384);
                image.Freeze();frames[index]=image;lock(Frames)Frames.Add(image);return image;
            }}
        }
        public IEnumerator<BitmapSource> GetEnumerator(){for(var i=0;i<Count;i++)yield return this[i];}
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()=>GetEnumerator();
    }
}

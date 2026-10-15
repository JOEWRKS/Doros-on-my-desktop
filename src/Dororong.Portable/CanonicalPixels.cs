namespace Dororong.Portable;

public static class CanonicalPixels
{
    // Exact bounded cleanup used by the accepted WPF UprightRumpSource.
    // Input/output are straight BGRA, not premultiplied pixels.
    public static byte[] Clean(ReadOnlySpan<byte> original)
    {
        if (original.Length != LocomotionBank.FrameBytes) throw new ArgumentException("Expected 96x96 BGRA.");
        var clean = original.ToArray();
        for (var y = 60; y < 96; y++) for (var x = 60; x < 96; x++)
        {
            var i = (y * 96 + x) * 4;
            if (original[i + 3] is 0 or >= 40) continue;
            var attached = false;
            for (var dy = -1; dy <= 1; dy++) for (var dx = -1; dx <= 1; dx++)
                if (x + dx >= 0 && x + dx < 96 && y + dy >= 0 && y + dy < 96 &&
                    original[((y + dy) * 96 + x + dx) * 4 + 3] >= 40) attached = true;
            if (!attached) Array.Clear(clean, i, 4);
        }
        return clean;
    }
}

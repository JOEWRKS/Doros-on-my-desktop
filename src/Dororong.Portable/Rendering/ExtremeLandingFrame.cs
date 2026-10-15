namespace Dororong.Portable.Rendering;

internal static class ExtremeLandingFrame
{
    internal static byte[] Spread(byte[] pixels, double amount)
    {
        var output = SurroundingPullRenderer.Pad(pixels);
        if (amount == 0) return output;
        for (var y = 67; y < 96; y++)
        {
            Array.Clear(output, (y + 32) * 640, 640);
            var reach = 24 * amount * Smooth((y - 66) / 17d);
            for (var x = -32; x < 128; x++)
            {
                // x(u) is strictly increasing: inverse search cannot fold or tear.
                var lo = x - reach; var hi = x + reach;
                for (var i = 0; i < 16; i++)
                {
                    var u = (lo + hi) * .5;
                    var mapped = u + reach * (2 * Smooth((u - 46) / 14) - 1);
                    if (mapped < x) lo = u; else hi = u;
                }
                var sx = (lo + hi) * .5; var ix = (int)Math.Floor(sx); var fraction = sx - ix;
                for (var channel = 0; channel < 4; channel++)
                {
                    var left = ix >= 0 && ix < 96 ? pixels[(y*96+ix)*4+channel] : 0;
                    var right = ix+1 >= 0 && ix+1 < 96 ? pixels[(y*96+ix+1)*4+channel] : 0;
                    output[((y+32)*160+x+32)*4+channel] = (byte)Math.Round(left*(1-fraction)+right*fraction);
                }
            }
        }
        return output;
    }
    private static double Smooth(double t) { t = Math.Clamp(t,0,1); return t*t*(3-2*t); }
}

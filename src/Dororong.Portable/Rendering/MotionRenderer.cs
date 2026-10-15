namespace Dororong.Portable.Rendering;

public static class MotionRenderer
{
    private static readonly Dictionary<(int,int,bool),RasterImage> walking = new();
    public static RasterImage Standing(bool closed=false,double breathing=0)
    {
        if(!double.IsFinite(breathing))throw new ArgumentOutOfRangeException(nameof(breathing));
        return BodyBreathingFrame.Standing(closed,breathing);
    }
    public static RasterImage Walk(double amplitude,double distance,bool closed=false)
    {
        if(!double.IsFinite(amplitude)||!double.IsFinite(distance))throw new ArgumentOutOfRangeException(nameof(amplitude));
        var level=(int)Math.Round(Math.Clamp(amplitude,0,1)*8);
        if(level==0)return UprightRumpSource.Standing(closed);
        var phase=(int)Math.Round(((distance%10+10)%10)/10*32)%32;
        var key=(level,phase,closed);
        if(!walking.TryGetValue(key,out var image))
            walking[key]=image=SnackRunFrames.Render(phase*10d/32,level/8d,walking:true,closed);
        return image;
    }
}

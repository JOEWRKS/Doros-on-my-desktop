using Dororong.Core.Behavior;

namespace Dororong.Portable.Rendering;

// Same phase boundaries and PBGRA interpolation as the Windows presenter.
public sealed class RestPresentation
{
    private static readonly RasterImage[] sleep=[HeadPullFrames.Canonical,RasterImage.Load("dororong-blink-squint.png"),RasterImage.Load("dororong-closed-eyes.png"),
        RasterImage.Load("dororong-sleep-crouch-closed.png"),RasterImage.Load("dororong-sleep-tuck-closed.png"),RasterImage.Load("dororong-sleep.png")];
    private static readonly RasterImage[] wake=[RasterImage.Load("dororong-sleep-tuck-squint.png"),RasterImage.Load("dororong-sleep-crouch-squint.png")];
    private static readonly double[] edges=[0,.0225,.045,.070,.100,.135];
    private static readonly Lazy<LocomotionBank> sit=new(()=>{using var stream=RasterImage.Resource("locomotion.pbgra.gz");return LocomotionBank.Load(stream);});
    private bool wasSleeping,settled;
    private double blink,wakeMs=180;
    public bool Closed=>blink is >=2000 and <2360;
    public double Breathing {get;private set;}
    internal FrameInteractionDescriptor Descriptor {get;private set;}=FrameInteractionDescriptor.Canonical;
    public void PrepareWake(){if(wasSleeping&&settled)wakeMs=0;}
    public RasterImage Render(PetSnapshot state,double milliseconds,bool allowed,double sitting=0,bool rising=false,double walk=0,double distance=0)
    {
        Descriptor=FrameInteractionDescriptor.Canonical;
        blink=allowed&&state.State is PetState.Idle or PetState.Walk?(blink+milliseconds)%5000:0;
        Breathing=state.State==PetState.Idle?Breath(state.Phase):0;
        if(state.State==PetState.Sleep)
        {
            if(!wasSleeping)settled=false;wasSleeping=true;
            if(state.Phase>=.135)settled=true;
            Descriptor=settled?FrameInteractionDescriptor.SettledSleep:SleepDescriptor(state.Phase);
            return settled?sleep[5]:SleepFrame(state.Phase);
        }
        wasSleeping=false;
        if(wakeMs<180){Descriptor=wakeMs<90?FrameInteractionDescriptor.SleepTuck:FrameInteractionDescriptor.SleepCrouch;var frame=wake[wakeMs<90?0:1];wakeMs=Math.Min(180,wakeMs+milliseconds);return frame;}
        if(sitting>0)return new(96,96,sit.Value.Sit(sitting,Closed,rising).Span);
        if(walk>0)return MotionRenderer.Walk(walk,distance,Closed);
        return MotionRenderer.Standing(Closed,Breathing);
    }
    internal static RasterImage SleepFrame(double phase)
    {
        for(var i=0;i<5;i++)if(phase<edges[i+1])return Blend(sleep[i],sleep[i+1],Math.Clamp((phase-edges[i])/(edges[i+1]-edges[i]),0,1));
        return sleep[5];
    }
    internal static FrameInteractionDescriptor SleepDescriptor(double phase)
    {
        FrameInteractionDescriptor[] d=[FrameInteractionDescriptor.Canonical,FrameInteractionDescriptor.Canonical,FrameInteractionDescriptor.Canonical,FrameInteractionDescriptor.SleepCrouch,FrameInteractionDescriptor.SleepTuck,FrameInteractionDescriptor.SettledSleep];
        for(var i=0;i<5;i++)if(phase<edges[i+1]){var p=Math.Clamp((phase-edges[i])/(edges[i+1]-edges[i]),0,1);return FrameInteractionDescriptor.Interpolate(d[i],d[i+1],p*p*(3-2*p));}
        return d[5];
    }
    internal static RasterImage Blend(RasterImage a,RasterImage b,double t)
    {
        var mix=t*t*(3-2*t);var data=a.CopyPixels();var other=b.Pixels.Span;
        for(var i=0;i<data.Length;i++)data[i]=(byte)Math.Round(data[i]+(other[i]-data[i])*mix,MidpointRounding.AwayFromZero);
        return new(a.PixelWidth,a.PixelHeight,data);
    }
    internal static double Breath(double phase)
    {var p=phase<.38?phase/.38:(phase-.43)/.57;return phase<.38?p*p*(3-2*p):phase<=.43?1:1-p*p*(3-2*p);}
}

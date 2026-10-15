using Dororong.Core.Geometry;
namespace Dororong.Portable.Rendering;

internal sealed class ClickPresentation
{
    private static readonly RasterImage tuck=RasterImage.Load("dororong-sleep-tuck-squint.png"),crouch=RasterImage.Load("dororong-sleep-crouch-squint.png");
    private PointD press;private bool head,waking,canceled;private double age,wakeAge;
    internal bool Active {get;private set;}
    internal bool Holding {get;private set;}
    internal FrameInteractionDescriptor Descriptor {get;private set;}=FrameInteractionDescriptor.Canonical;
    internal double Lift=>Active&&!Holding?BodyClickTransformSampler.SampleConfirmedClick(age/500).TranslationY:0;
    internal void Begin(PointD pointer,bool upperHead,bool asleep)
    {Reset();press=pointer;head=upperHead;waking=asleep;Active=Holding=true;}
    internal void Advance(double ms,PointD pointer,bool down)
    {
        if(!Active)return;
        if(Holding)
        {
            canceled|=Math.Abs(pointer.X-press.X)>=4||Math.Abs(pointer.Y-press.Y)>=4;
            if(!down){Holding=false;age=0;if(canceled)Reset();}
        }
        else {age+=ms;if(age>=500)Reset();}
    }
    internal RasterImage Render(double ms,out Affine transform)
    {
        var sample=Holding?(head?BodyClickTransformSampler.SamplePendingPress():BodyClickTransformSample.Rest):BodyClickTransformSampler.SampleConfirmedClick(age/500);
        var frame=HeadPullFrames.Canonical;
        Descriptor=FrameInteractionDescriptor.Canonical;
        if(waking)
        {
            if(wakeAge<45){frame=tuck;Descriptor=FrameInteractionDescriptor.SleepTuck;}
            else if(wakeAge<90){frame=crouch;Descriptor=FrameInteractionDescriptor.SleepCrouch;}
            wakeAge=Math.Min(90,wakeAge+ms);
        }
        const double x=.428987*96,y=.916667*96;
        transform=new(sample.ScaleX,0,0,sample.ScaleY,x*(1-sample.ScaleX),y*(1-sample.ScaleY));
        return frame;
    }
    internal void Reset(){Active=Holding=false;age=wakeAge=0;canceled=waking=false;}
}

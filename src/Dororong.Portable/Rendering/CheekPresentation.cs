using Dororong.Core.Geometry;

namespace Dororong.Portable.Rendering;

// Captures the displayed pose once; no cursor-to-window carrying path.
internal sealed class CheekPresentation
{
    private readonly FaceFollowMotion follow=new();
    private PointD press,axis;private Affine transform;private RasterImage? source;
    private Func<double,double,double,RasterImage>? render;
    private double released,age,lastHop;private bool attached,seated,spring;
    internal bool Active=>source is not null;
    internal bool Releasing {get;private set;}
    internal double Pull {get;private set;}
    internal double DeltaX {get;private set;}
    internal double Lift {get;private set;}
    internal bool Flipped=>transform.M11<0;
    internal void Begin(PointD pointer,RasterImage image,Affine placement,PointD headAxis,bool perched,bool sitting,
        Func<double,double,double,RasterImage>? renderer=null)
    {
        Reset();press=pointer;source=image;transform=placement;attached=perched;seated=sitting;render=renderer;
        SetAxis(headAxis);
    }
    private void SetAxis(PointD headAxis)
    {
        var x=headAxis.X*transform.M11+headAxis.Y*transform.M21;
        var y=headAxis.X*transform.M12+headAxis.Y*transform.M22;var length=Math.Sqrt(x*x+y*y);
        axis=new(-x/length,-y/length);
    }
    internal void Advance(double ms,PointD pointer,bool down)
    {
        DeltaX=0;if(!Active)return;
        if(!down&&!Releasing){Releasing=true;released=Pull;spring=Pull>0;age=0;follow.BeginRelease(spring);return;}
        if(Releasing)
        {
            age+=ms;var duration=spring?440:220;var t=Math.Clamp(age/duration,0,1);
            Pull=released*(spring?CheekSpringMotion.Remaining(age):1-t*t*(3-2*t));
            var hop=attached||!spring?0:-Math.Sign(axis.X)*CheekSpringMotion.Offset(age,released);
            DeltaX=hop-lastHop;lastHop=hop;Lift=attached||!spring?0:-CheekSpringMotion.Lift(age,released);
            follow.Step(Pull,ms);
            // Keep the last hop delta available on retirement; never undo it.
            if(age>=duration){source=null;render=null;Releasing=false;Pull=0;}
            return;
        }
        var delta=pointer-press;
        if(seated&&Math.Abs(delta.X)>=4&&Math.Sign(delta.X)!=Math.Sign(axis.X))
        {transform=new(-transform.M11,-transform.M12,transform.M21,transform.M22,transform.OffsetX+96*transform.M11,transform.OffsetY+96*transform.M12);axis=new(-axis.X,axis.Y);}
        Pull=Math.Clamp(delta.X*axis.X+delta.Y*axis.Y,-10,20);follow.Step(Pull,ms);
    }
    internal RasterImage Render()
    {
        var image=Pull==0?source!:render?.Invoke(Pull,follow.Eye,follow.Hair)??Deform(source!,Pull,follow.Eye,follow.Hair);
        return DesktopFrames.Warp(image,transform);
    }
    internal static RasterImage Deform(RasterImage image,double pull,double eye,double hair)
        =>FromStraight(image.PixelWidth,image.PixelHeight,OutlineCheekRenderer.Render(Straight(image),pull,eye,hair));
    internal static byte[] Straight(RasterImage image)
    {
        var data=image.CopyPixels();for(var i=0;i<data.Length;i+=4)for(var c=0;c<3;c++)
            data[i+c]=data[i+3]==0?(byte)0:(byte)Math.Clamp(Math.Round(data[i+c]*255d/data[i+3]),0,255);
        return data;
    }
    internal static RasterImage FromStraight(int width,int height,byte[] data)
    {var result=(byte[])data.Clone();for(var i=0;i<result.Length;i+=4)for(var c=0;c<3;c++)result[i+c]=(byte)((result[i+c]*result[i+3]+127)/255);return new(width,height,result);}
    internal void Reset(){source=null;render=null;Releasing=false;Pull=DeltaX=Lift=age=lastHop=0;follow.Reset();}
}

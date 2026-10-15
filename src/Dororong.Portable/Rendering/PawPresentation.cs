using Dororong.Core.Geometry;

namespace Dororong.Portable.Rendering;

internal sealed class PawPresentation
{
    internal PerchPawSnapshot? Current {get;private set;}
    internal bool Releasing {get;private set;}
    private PointD press,initial,released;private double angle,age,sign;private bool dragged,tap;
    internal void Begin(bool right,PointD pointer,bool flip)
    {
        var previous=Current is { } old&&old.Right==right?old:new PerchPawSnapshot(right,default);
        Current=previous;press=pointer;initial=previous.Offset;angle=previous.Angle;sign=flip?-1:1;
        age=0;dragged=initial!=default;tap=Releasing=false;
    }
    internal void Advance(double ms,PointD pointer,bool down)
    {
        if(Current is null)return;age+=ms;
        if(Releasing)
        {
            var t=Math.Clamp(age/(tap?600:220),0,1);if(t>=1){Reset();return;}
            var a=1-t*t*(3-2*t);Current=Current with{Offset=tap?default:new(released.X*a,released.Y*a),Angle=tap?22*Math.Sin(t*6*Math.PI)*Math.Sin(t*Math.PI):angle*a};return;
        }
        if(!down)
        {tap=!dragged&&age<=350;released=Current.Offset;if(!tap&&!dragged){Reset();return;}Releasing=true;age=0;return;}
        var d=pointer-press;dragged|=Math.Sqrt(d.X*d.X+d.Y*d.Y)>=4;
        var offset=initial+new PointD(d.X*sign,d.Y);var length=Math.Sqrt(offset.X*offset.X+offset.Y*offset.Y);var limit=length>18?18/length:1;
        Current=Current with{Offset=dragged?new(offset.X*limit,offset.Y*limit):initial};
    }
    internal void Reset(){Current=null;Releasing=false;age=0;}
}

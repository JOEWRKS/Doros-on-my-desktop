using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Dororong.App.Controls;

// One retained vector visual, no frame/size bitmap cache and no mouse interception.
internal sealed class DoroQuestionBubble : FrameworkElement
{
    private readonly DrawingGroup _left,_right;
    private bool _tailRight;
    internal DoroQuestionBubble(){
        Width=64;Height=34;IsHitTestVisible=false;
        _left=Build(false);_right=Build(true);
    }
    internal bool TailRight {set{if(_tailRight==value)return;_tailRight=value;InvalidateVisual();}}
    protected override void OnRender(DrawingContext dc)=>dc.DrawDrawing(_tailRight?_right:_left);
    private static DrawingGroup Build(bool right){
        var ink=new SolidColorBrush(Color.FromRgb(80,52,61));ink.Freeze();
        var pen=new Pen(ink,1.15){LineJoin=PenLineJoin.Round};pen.Freeze();
        var outline=new StreamGeometry();using(var g=outline.Open()){
            g.BeginFigure(new(9,1),true,true);g.LineTo(new(55,1),true,false);
            g.QuadraticBezierTo(new(63,1),new(63,9),true,false);g.LineTo(new(63,18),true,false);
            g.QuadraticBezierTo(new(63,26),new(55,26),true,false);
            if(right){g.LineTo(new(51,26),true,false);g.LineTo(new(55,32),true,false);g.LineTo(new(42,26),true,false);}
            else{g.LineTo(new(22,26),true,false);g.LineTo(new(9,32),true,false);g.LineTo(new(13,26),true,false);}
            g.LineTo(new(9,26),true,false);g.QuadraticBezierTo(new(1,26),new(1,18),true,false);
            g.LineTo(new(1,9),true,false);g.QuadraticBezierTo(new(1,1),new(9,1),true,false);
        }outline.Freeze();
        var text=new FormattedText("DORO?",CultureInfo.InvariantCulture,FlowDirection.LeftToRight,
            new Typeface(new FontFamily("Segoe UI"),FontStyles.Normal,FontWeights.SemiBold,FontStretches.Normal),13,ink,1);
        var glyph=text.BuildGeometry(new((64-text.Width)/2,(27-text.Height)/2));glyph.Freeze();
        var drawing=new DrawingGroup();using(var dc=drawing.Open()){
            dc.DrawGeometry(Brushes.White,pen,outline);dc.DrawGeometry(ink,null,glyph);
        }drawing.Freeze();return drawing;
    }
}

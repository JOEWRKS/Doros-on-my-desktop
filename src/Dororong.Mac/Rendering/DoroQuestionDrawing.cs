using Avalonia;
using Avalonia.Media;
namespace Dororong.Mac.Rendering;
// Retained vector paths, including the accepted glyph outline. No font fallback,
// bitmap cache, hit region or native window is introduced by the speech bubble.
internal sealed class DoroQuestionDrawing {
 private static readonly IBrush Ink=new SolidColorBrush(Color.FromRgb(80,52,61));
 private static readonly Pen Outline=new(Ink,1.15,lineJoin:PenLineJoin.Round);
 private readonly Geometry left=Shape(false),right=Shape(true),glyph;
 internal DoroQuestionDrawing(){
  using var s=typeof(DoroQuestionDrawing).Assembly.GetManifestResourceStream("Dororong.question-glyph")!;
  using var reader=new StreamReader(s);glyph=Geometry.Parse(reader.ReadToEnd());
 }
 private static Geometry Shape(bool right)=>Geometry.Parse("M9,1 L55,1 Q63,1 63,9 L63,18 Q63,26 55,26 "+
  (right?"L51,26 L55,32 L42,26 ":"L22,26 L9,32 L13,26 ")+"L9,26 Q1,26 1,18 L1,9 Q1,1 9,1 Z");
 internal void Draw(DrawingContext dc,Point origin,double scale,bool tailRight,double opacity){
  using var alpha=dc.PushOpacity(opacity);
  using var transform=dc.PushTransform(new Matrix(scale,0,0,scale,origin.X+(tailRight?2:78)*scale,origin.Y+12*scale));
  dc.DrawGeometry(Brushes.White,Outline,tailRight?right:left);dc.DrawGeometry(Ink,null,glyph);
 }
}

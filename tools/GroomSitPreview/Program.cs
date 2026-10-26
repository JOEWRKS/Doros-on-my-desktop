using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;
using Dororong.App.Interaction;
using Dororong.Core.Behavior;

internal static class Program
{
    // Preview-only time remapping of the packaged renderer. No application
    // source, installed binary, running process or preference is changed.
    static double Smooth(double x)=>x*x*(3-2*x);
    static double CandidateAge(double t){
        if(t>=.7)return .4+t-.7;
        var u=Math.Clamp(t/.7,0,1);var amount=u*u*u*(10+u*(-15+6*u));
        var lo=0d;var hi=1d;
        for(var i=0;i<48;i++){var mid=(lo+hi)/2;if(Smooth(mid)<amount)lo=mid;else hi=mid;}
        return .4*(lo+hi)/2;
    }
    [STAThread] static void Main(string[] args){
        var output=Path.GetFullPath(args[0]);Directory.CreateDirectory(output);
        var a=new DororongPresenter();var b=new DororongPresenter();a.SetArtSize(150);b.SetArtSize(150);
        var pet=new PetSnapshot(PetState.Idle,default,FacingDirection.Right,0,false,null);
        var records=new List<object>();
        for(var frame=0;frame<144;frame++){
            var t=Math.Clamp(frame/60d-.45,0,1.3);
            var visual=new DrawingVisual();using(var dc=visual.RenderOpen()){
                dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(241,244,247)),null,new Rect(0,0,720,340));
                dc.DrawLine(new Pen(new SolidColorBrush(Color.FromRgb(207,214,222)),1),new(360,18),new(360,320));
                for(var side=0;side<2;side++){
                    var p=side==0?a:b;var age=side==0?t:CandidateAge(t);
                    p.Render(pet with{IdleActivity=new(IdleActivityKind.Groom,age)},DirectInteractionSnapshot.None,TimeSpan.Zero);
                    p.Measure(new(p.Width,p.Height));p.Arrange(new(0,0,p.Width,p.Height));p.UpdateLayout();
                    var bitmap=new RenderTargetBitmap(216,216,96,96,PixelFormats.Pbgra32);bitmap.Render(p);
                    var crop=new CroppedBitmap(bitmap,new Int32Rect(30,30,150,150));
                    var sole=p.MeasurePlatformContact()!.Value.SoleY;
                    var top=280-(sole-30)*1.8;
                    dc.DrawImage(crop,new Rect(side*360+45,top,270,270));
                    dc.DrawLine(new Pen(new SolidColorBrush(Color.FromRgb(186,197,208)),1),new(side*360+35,281),new(side*360+325,281));
                    Label(side==0?"현재 · 0.40초":"후보 · 0.70초",side*360+34,22,22,dc);
                    Label(side==0?"기존 앉기":"시작·끝을 더 완만하게",side*360+34,55,15,dc);
                    var progress=Math.Clamp(t/(side==0?.4:.7),0,1);
                    dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(218,226,234)),null,new Rect(side*360+35,304,290,4));
                    dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(102,126,167)),null,new Rect(side*360+35,304,290*progress,4));
                    records.Add(new{frame,side,time=t,sourceAge=age,motionFrame=p.HighResolutionFace.MotionFrame,sole});
                }
            }
            var result=new RenderTargetBitmap(720,340,96,96,PixelFormats.Pbgra32);result.Render(visual);
            var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(result));
            using var stream=File.Create(Path.Combine(output,$"{frame:000}.png"));png.Save(stream);
        }
        File.WriteAllText(Path.Combine(output,"samples.json"),JsonSerializer.Serialize(records,new JsonSerializerOptions{WriteIndented=true}));
        Console.WriteLine($"144 preview frames, 60fps. Product unchanged: {output}");
    }
    static void Label(string text,double x,double y,double size,DrawingContext dc)=>dc.DrawText(
        new FormattedText(text,CultureInfo.GetCultureInfo("ko-KR"),FlowDirection.LeftToRight,new Typeface("Malgun Gothic"),size,Brushes.DarkSlateGray,1),new(x,y));
}

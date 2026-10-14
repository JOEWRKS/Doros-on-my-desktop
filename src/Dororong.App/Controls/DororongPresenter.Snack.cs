using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;

namespace Dororong.App.Controls;

public partial class DororongPresenter
{
    private SnackPose _snackPose;
    private PointerSample _snackPointer;
    private SnackRenderer? _snackRenderer;
    private Matrix _snackHead = Matrix.Identity;
    private double _snackRecoveryAge = 2.6;
    internal void SetSnack(SnackPose pose, PointerSample pointer)
    {
        if (pose.Active != _snackPose.Active)
        {
            // Carry crouch depth into the authored recovery, not a ghosted
            // crossfade or an immediate jump to the upright snack frame.
            _snackRecoveryAge = SnackRenderer.RecoveryAge(HuntPose.At(_huntVisualFrame/60d).Amount);
            ResetHunting();
            _postBodyClickIdleFacing = _postCheekIdleFacing = _postHeadLandingIdleFacing = null;
        }
        _snackPose = pose; _snackPointer = pointer;
    }

    internal PointD GetSnackMouth()
    {
        var p = _snackHead.Transform(new Point(29,60));
        p = DororongImage.TransformToAncestor(this).Transform(p);
        return new(p.X,p.Y);
    }

    private void ApplySnack(PetSnapshot snapshot)
    {
        if (!_snackPose.Active) return;
        var renderer = _snackRenderer ??= new();
        DororongImage.Source = renderer.Render(_snackPose,default,_snackRecoveryAge);
        _snackHead = renderer.HeadTransform;
        BodyScaleTransform.ScaleX = snapshot.Facing == FacingDirection.Left ? -1 : 1;
        BodyScaleTransform.ScaleY = ImageBreathingScaleTransform.ScaleX = ImageBreathingScaleTransform.ScaleY = 1;
        BodyRotateTransform.Angle = 0;
        BodyTranslateTransform.X = BodyTranslateTransform.Y = 0;
    }

    public event EventHandler? SnackRequested;
    public event EventHandler? StartupToggleRequested;
    public event EventHandler? ProductMenuOpened;
    internal void UpdateProductMenu(bool snackActive, bool startupEnabled)
    {
        SnackMenuItem.Header = snackActive ? "간식 취소" : "간식 주기";
        StartupMenuItem.IsChecked = startupEnabled;
    }
    private void OnSnackClicked(object sender, RoutedEventArgs e) => SnackRequested?.Invoke(this,EventArgs.Empty);
    private void OnStartupClicked(object sender, RoutedEventArgs e) => StartupToggleRequested?.Invoke(this,EventArgs.Empty);
    private void OnProductMenuOpened(object sender, RoutedEventArgs e) => ProductMenuOpened?.Invoke(this,EventArgs.Empty);
}

// Original closed-mouth pixels are animated locally. The opening/tongue is a
// temporary layer; no replacement omega mouth or whole-character resampling.
internal sealed class SnackRenderer
{
    private readonly HuntRenderer hunt = new();
    private static readonly BitmapSource Jewel = LoadJewel();
    private static BitmapSource LoadJewel()
    {
        var source=new BitmapImage(new Uri("pack://application:,,,/Dororong.App;component/Assets/snack-jewel.png"));
        source.Freeze();return source;
    }
    private readonly Dictionary<(int,int),BitmapSource> runs = new();
    private readonly Dictionary<int,BitmapSource> jumps = new();
    internal Matrix HeadTransform { get; private set; } = Matrix.Identity;
    internal static double RecoveryAge(double amount)
    {
        // Invert the existing authored crouch recovery, retaining pose depth.
        double lo=2.05,hi=2.6;
        for(var i=0;i<20;i++){var mid=(lo+hi)/2;if(HuntPose.At(mid).Amount>amount)lo=mid;else hi=mid;}
        return (lo+hi)/2;
    }
    internal static int JumpFrame(double age)
    {
        double seconds;
        if(age<SnackSession.JumpPrepareSeconds)
            seconds=.25+.45*Smooth(age/SnackSession.JumpPrepareSeconds);
        else if(age<SnackSession.JumpPrepareSeconds+SnackSession.JumpFlightSeconds)
        {
            var u=(age-SnackSession.JumpPrepareSeconds)/SnackSession.JumpFlightSeconds;
            // Push out of the hunting crouch early, extend during flight.
            seconds=2.05+.55*Smooth(u/.65);
        }
        else
        {
            // The hunting crouch also reaches the forepaws forward. Replaying
            // it as landing compression creates an unwanted second paw jab.
            // Flight already recovers to the planted pose before touchdown.
            seconds=2.6;
        }
        return (int)Math.Clamp(Math.Round(seconds*60),0,156);
    }
    internal BitmapSource Render(SnackPose pose, HuntGazePose gaze, double entryRecoveryAge=2.6)
    {
        if(pose.Phase==SnackPhase.Happy)
        {
            // Same single-sampled head and hidden neck backing as accepted
            // upright tracking. No body scaling, foot movement or mouth redraw.
            var happyGaze=new HuntGazePose(0,0,HappyRoll(pose.Age));
            HeadTransform=HuntRenderer.HeadTransform(156,happyGaze);
            return hunt.Render(156,happyGaze,closed:true);
        }
        var eating = pose.Phase == SnackPhase.Eating;
        BitmapSource source;
        HeadTransform=Matrix.Identity;
        var frame=pose.JumpAge>=0 ? JumpFrame(pose.JumpAge) : 156;
        if(pose.Phase==SnackPhase.Chasing && pose.Age<.24 && entryRecoveryAge<2.59)
            frame=(int)Math.Round(Math.Min(2.6,entryRecoveryAge+pose.Age*.55/.24)*60);
        if(frame<156)
        {
            if(!jumps.TryGetValue(frame,out source!))
                jumps[frame]=source=hunt.Render(frame,default,crisp:true);
            HeadTransform=HuntRenderer.HeadTransform(frame,default);
        }
        else if(pose.Phase==SnackPhase.Chasing && pose.RunAmount>0 && pose.JumpAge<0)
        {
            var phase=(int)Math.Round((pose.Distance%SnackRunFrames.Stride)/SnackRunFrames.Stride*48)%48;
            var level=(int)Math.Round(pose.RunAmount*8);
            var key=(phase,level);
            if(!runs.TryGetValue(key,out source!))
                runs[key]=source=SnackRunFrames.Render(phase*SnackRunFrames.Stride/48,level/8d);
            HeadTransform=new Matrix(1,0,0,1,0,SnackRunFrames.Bob(phase/48d,level/8d));
        }
        else source=UprightRumpSource.Standing(eating || pose.Phase==SnackPhase.Settling && pose.Age<.16);
        if(pose.Phase==SnackPhase.Settling) return source;
        if(eating)source=SnackMouthFrame.Chew(source,pose.Age);
        var visual = new DrawingVisual();
        // Paint effects separately at native resolution, then source-over onto
        // copied pixels. Every unaffected face/hair/body pixel stays exact.
        using(var dc=visual.RenderOpen())
        {
            dc.PushTransform(new MatrixTransform(HeadTransform));
            if(eating)
            {
                var pen=new Pen(new SolidColorBrush(Color.FromRgb(80,52,61)),.85)
                    {StartLineCap=PenLineCap.Round,EndLineCap=PenLineCap.Round};
                var open=Smooth(pose.Age/.14)*(1-Smooth((pose.Age-.30)/.18));
                if(open>.01)
                {
                    // Preserve the authored smiling upper lip. Open a rounded
                    // lower jaw underneath it, like the supplied figure photo.
                    var g=new StreamGeometry();using(var c=g.Open())
                    {
                        c.BeginFigure(new Point(27,60),true,true);
                        c.QuadraticBezierTo(new Point(29,60.6),new Point(31.2,60),false,false);
                        c.BezierTo(new Point(31,60+open*4.2),new Point(27.2,60+open*4.2),new Point(27,60),true,false);
                    }
                    dc.DrawGeometry(new SolidColorBrush(Color.FromRgb(109,57,72)),pen,g);
                    dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(235,145,170)),null,new Point(29.2,60+open*2),1.1,open*.75);
                }
                if(pose.Age<.34)
                {
                    var swallow=Smooth((pose.Age-.12)/.22);var size=10*(1-swallow)+1;
                    dc.PushOpacity(1-swallow);
                    dc.DrawImage(Jewel,new Rect(21+8*swallow-size/2,58-size/2,size,size));
                    dc.Pop();
                }
            }
            else
            {
                var y=67+4*(.5+.5*Math.Sin(pose.Age*4));
                var drool=new Pen(new SolidColorBrush(Color.FromRgb(108,185,222)),1.3){StartLineCap=PenLineCap.Round,EndLineCap=PenLineCap.Round};
                var g=new StreamGeometry();using(var c=g.Open())
                {c.BeginFigure(new Point(30.8,61.5),false,false);c.BezierTo(new Point(32,65),new Point(30,67),new Point(31,y),true,false);}
                dc.DrawGeometry(null,drool,g);
                dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(174,226,242)),new Pen(Brushes.SteelBlue,.45),new Point(31,y+1),1.3,2);
                dc.DrawLine(new Pen(Brushes.White,.45),new Point(30.8,y),new Point(30.8,y+1.5));
            }
            dc.Pop();
        }
        var overlay=new RenderTargetBitmap(96,96,96,96,PixelFormats.Pbgra32);overlay.Render(visual);
        var ink=PremultipliedFrame.From(overlay).Pixels;
        var pixels=PremultipliedFrame.From(source).Pixels;
        for(var i=0;i<pixels.Length;i+=4)
        {
            var coverage=ink[i+3]/255d;
            for(var c=0;c<4;c++)pixels[i+c]=(byte)Math.Min(255,Math.Round(ink[i+c]+pixels[i+c]*(1-coverage)));
        }
        var bitmap=BitmapSource.Create(96,96,96,96,PixelFormats.Pbgra32,null,pixels,384);
        bitmap.Freeze();return bitmap;
    }
    internal static double HappyRoll(double age)
    {
        // Long continuous sways rather than quick tips and held accents.
        // Zero speed at each boundary; the closed-eye neutral endpoints exactly
        // match the last chew and the existing quiet settling phase.
        ReadOnlySpan<double> times=[0,.06,.46,1.10,1.64,2.10,SnackSession.HappySeconds];
        ReadOnlySpan<double> degrees=[0,0,-8,8,-4,0,0];
        if(!double.IsFinite(age)||age<=0||age>=SnackSession.HappySeconds)return 0;
        for(var i=1;i<times.Length;i++)
            if(age<=times[i])
            {
                var u=Smooth((age-times[i-1])/(times[i]-times[i-1]));
                return (degrees[i-1]+(degrees[i]-degrees[i-1])*u)*Math.PI/180;
            }
        return 0;
    }
    private static double Smooth(double u){u=Math.Clamp(u,0,1);return u*u*(3-2*u);}
}

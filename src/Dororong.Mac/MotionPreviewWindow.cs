using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Portable.Rendering;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Dororong.Mac;

internal enum PreviewMode { Idle, Walk, Track, Snack }

// Deliberately bounded test stage, not a replacement menu or native desktop
// integration. No global input hooks, OS window movement or permission prompts.
internal sealed class MotionPreviewWindow : Window
{
    private readonly Canvas stage = new() { Width=760, Height=340, Background=new SolidColorBrush(Color.Parse("#202830")), ClipToBounds=true };
    private readonly Image pet = new() { Width=96, Height=96, Stretch=Stretch.None, IsHitTestVisible=false };
    private readonly Image jewel = new() { Width=22, Height=22, IsHitTestVisible=false, IsVisible=false };
    private readonly WriteableBitmap frame = new(new PixelSize(96,96),new Vector(96,96),PixelFormat.Bgra8888,AlphaFormat.Premul);
    private readonly Bitmap jewelBitmap;
    private readonly SnackSession snack = new();
    private readonly SnackRenderer snackRenderer = new();
    private readonly UprightTracking tracking = new();
    private readonly LocomotionPresentation locomotion = new();
    private readonly DispatcherTimer timer = new() { Interval=TimeSpan.FromMilliseconds(16) };
    private readonly Stopwatch clock = Stopwatch.StartNew();
    private readonly Cursor snackCursor = new(StandardCursorType.None);
    private readonly TextBlock status = new() { Margin=new Thickness(12,6) };
    private TimeSpan last;
    private PreviewMode mode;
    private PointD? pointer;
    private double x=330, y=205, lift;
    private FacingDirection facing=FacingDirection.Right;
    private bool disposed;
    private bool awaitingSnackPointer;
    internal double PetX => x;
    internal SnackPhase SnackPhase => snack.Current.Phase;
    internal MotionPreviewWindow()
    {
        Title="Dororong — motion parity lab (not a release)";
        Width=790;Height=465;CanResize=false;
        var root=new StackPanel();
        root.Children.Add(new TextBlock{Text="Windows 모션 이식 검사 · 이 창 안에서만 조작 / 실제 Mac 검증 전",Margin=new Thickness(12)});
        var buttons=new StackPanel{Orientation=Orientation.Horizontal,Spacing=8,Margin=new Thickness(12,0)};
        foreach(var (label,value) in new[]{("평상시",PreviewMode.Idle),("걷기",PreviewMode.Walk),("마우스 추적",PreviewMode.Track),("간식 / 다시 시작",PreviewMode.Snack)})
        {
            var button=new Button{Content=label};button.Click+=(_,_)=>SetMode(value);buttons.Children.Add(button);
        }
        root.Children.Add(buttons);root.Children.Add(status);root.Children.Add(stage);Content=root;
        using(var stream=typeof(MotionPreviewWindow).Assembly.GetManifestResourceStream("Dororong.jewel")!) jewelBitmap=new Bitmap(stream);
        jewel.Source=jewelBitmap;pet.Source=frame;stage.Children.Add(pet);stage.Children.Add(jewel);
        RenderOptions.SetBitmapInterpolationMode(pet,BitmapInterpolationMode.None);
        stage.PointerMoved+=(_,e)=>{var p=e.GetPosition(stage);SetPointer(new(p.X,p.Y));};
        stage.PointerExited+=(_,_)=>SetPointer(null);
        timer.Tick+=Tick;
        Opened+=(_,_)=>{last=clock.Elapsed;timer.Start();};
        Closed+=(_,_)=>{disposed=true;timer.Stop();timer.Tick-=Tick;snack.Cancel();stage.Cursor=null;
            pet.Source=null;jewel.Source=null;frame.Dispose();jewelBitmap.Dispose();snackCursor.Dispose();};
        SetMode(PreviewMode.Idle);Advance(0);
    }
    internal void SetPointer(PointD? position)
    {
        pointer=position;
        if(position.HasValue)awaitingSnackPointer=false;
    }
    internal void SetMode(PreviewMode value)
    {
        snack.Cancel();tracking.Reset();locomotion.Advance(0,false,0,true);mode=value;
        awaitingSnackPointer=value==PreviewMode.Snack&&!pointer.HasValue;
        if(value==PreviewMode.Snack)
        {
            snack.Begin(facing); // Position is deliberately not touched.
            snackRenderer.Render(snack.Current); // Reset captured mouth transform too.
        }
        lift=0;stage.Cursor=value==PreviewMode.Snack?snackCursor:null;
        status.Text=value==PreviewMode.Snack?"커서를 움직이면 쫓기 · 위로 들면 점프 · 입에 잠시 두면 먹기 · 창 밖 이동은 취소":"검사 모드: "+value;
    }
    private void Tick(object? sender,EventArgs e)
    {
        var now=clock.Elapsed;var dt=(now-last).TotalSeconds;last=now;Advance(dt);
    }
    internal void Advance(double seconds)
    {
        if(disposed)return;
        var dt=Math.Clamp(seconds,0,.05);RasterImage rendered;
        if(mode==PreviewMode.Snack&&snack.Active&&awaitingSnackPointer)
            rendered=MotionRenderer.Standing();
        else if(mode==PreviewMode.Snack&&snack.Active)
        {
            var mouth=snackRenderer.HeadTransform.Transform(new(29,60));
            mouth=new(x+(facing==FacingDirection.Left?96-mouth.X:mouth.X),y+lift+mouth.Y);
            var sample=pointer is { } p?new PointerSample(true,p):PointerSample.Unavailable;
            var pose=snack.Advance(TimeSpan.FromSeconds(seconds),sample,mouth,true);
            if(pose.Active)
            {
                x=Math.Clamp(x+pose.DeltaX,0,664);lift=pose.Lift;facing=pose.Facing;
                rendered=snackRenderer.Render(pose);tracking.Advance(dt,null,false,blocked:true);
            }
            else{SetMode(PreviewMode.Idle);rendered=MotionRenderer.Standing();}
        }
        else
        {
            lift=0;double travel=0;
            if(mode==PreviewMode.Walk)
            {
                if(x<=5)facing=FacingDirection.Right;if(x>=659)facing=FacingDirection.Left;
                travel=(facing==FacingDirection.Right?1:-1)*21*dt;x=Math.Clamp(x+travel,0,664);
            }
            locomotion.Advance(dt*1000,mode==PreviewMode.Walk,travel,false);
            rendered=mode==PreviewMode.Walk?MotionRenderer.Walk(Math.Max(1d/8,locomotion.Walk),locomotion.Distance):MotionRenderer.Standing();
            if(mode==PreviewMode.Track)
            {
                PointD? relative=pointer is { } p?new(p.X-x,p.Y-y):null;
                tracking.Advance(dt,relative,facing==FacingDirection.Left);rendered=tracking.Render(rendered);
            }
        }
        var bytes=rendered.Pixels.ToArray();
        using(var target=frame.Lock())for(var row=0;row<96;row++)Marshal.Copy(bytes,row*384,target.Address+row*target.RowBytes,384);
        pet.RenderTransform=new ScaleTransform(facing==FacingDirection.Left?-1:1,1);
        pet.RenderTransformOrigin=RelativePoint.Center;
        Canvas.SetLeft(pet,Math.Round(x));Canvas.SetTop(pet,Math.Round(y+lift));pet.InvalidateVisual();
        jewel.IsVisible=snack.Active&&snack.Current.Phase==SnackPhase.Chasing&&pointer.HasValue;
        if(pointer is { } point){Canvas.SetLeft(jewel,point.X-11);Canvas.SetTop(jewel,point.Y-11);}
    }
}

using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;
using Dororong.Core.Behavior;
using Dororong.App.Interop;
using System.Windows.Interop;
using System.Windows.Threading;

class Program
{
    [STAThread] static void Main(string[] args)
    {
        _=new Application();var render=new SnackRenderer();var visual=new DrawingVisual();
        if(args[0]=="--happy")
        {
            var folder=Path.GetFullPath(args[1]);Directory.CreateDirectory(folder);
            RenderOptions.SetBitmapScalingMode(visual,BitmapScalingMode.NearestNeighbor);
            var previewFrames=(int)Math.Ceiling((SnackSession.EatingSeconds+SnackSession.HappySeconds+SnackSession.SettleSeconds+.5)*60);
            for(var n=0;n<previewFrames;n++)
            {
                var t=n/60d;
                var phase=t<SnackSession.EatingSeconds?SnackPhase.Eating:
                    t<SnackSession.EatingSeconds+SnackSession.HappySeconds?SnackPhase.Happy:SnackPhase.Settling;
                var age=t-(phase==SnackPhase.Eating?0:phase==SnackPhase.Happy?SnackSession.EatingSeconds:SnackSession.EatingSeconds+SnackSession.HappySeconds);
                var source=render.Render(new(phase,FacingDirection.Left,0,0,age,0),default);
                using(var dc=visual.RenderOpen())
                {
                    dc.DrawRectangle(Brushes.White,null,new Rect(0,0,480,288));
                    dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(18,20,28)),null,new Rect(480,0,480,288));
                    for(var side=0;side<2;side++)
                    {
                        dc.PushTransform(new TranslateTransform(side*480,0));
                        if(side==1)dc.PushTransform(new MatrixTransform(-1,0,0,1,480,0));
                        dc.DrawImage(source,new Rect(0,96,96,96));
                        dc.DrawImage(source,new Rect(144,0,288,288));
                        if(side==1)dc.Pop();dc.Pop();
                    }
                }
                var output=new RenderTargetBitmap(960,288,96,96,PixelFormats.Pbgra32);output.Render(visual);
                Save(output,Path.Combine(folder,$"{n:000}.png"));
            }
            // Whole gesture sampled evenly on light/dark for seam review.
            using(var dc=visual.RenderOpen())
            {
                dc.DrawRectangle(Brushes.White,null,new Rect(0,0,1536,3072));
                for(var n=0;n<=60;n++)
                {
                    var source=render.Render(new(SnackPhase.Happy,FacingDirection.Left,0,0,n*SnackSession.HappySeconds/60,0),default);
                    var x=n%8*192;var y=n/8*384;
                    dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(18,20,28)),null,new Rect(x,y+192,192,192));
                    dc.DrawImage(source,new Rect(x,y,192,192));dc.DrawImage(source,new Rect(x,y+192,192,192));
                }
            }
            var sheet=new RenderTargetBitmap(1536,3072,96,96,PixelFormats.Pbgra32);sheet.Render(visual);
            Save(sheet,Path.Combine(folder,"contact-sheet.png"));Console.WriteLine(folder);return;
        }
        if(args[0]=="--gaze-sweep")
        {
            var folder=Path.GetFullPath(args[1]);Directory.CreateDirectory(folder);
            var renderer=new HuntRenderer();byte[]? previous=null,older=null;
            double curvature=0,detail=0;long samples=0,detailSamples=0;
            var times=new List<double>();
            for(var n=0;n<=240;n++)
            {
                var roll=-.3+n*.0025;var watch=System.Diagnostics.Stopwatch.StartNew();
                var frame=renderer.Render(156,new(.4,.3,roll));watch.Stop();if(n>5)times.Add(watch.Elapsed.TotalMilliseconds);
                var pixels=PremultipliedFrame.From(frame).Pixels;
                for(var y=16;y<62;y++)for(var x=10;x<80;x++)
                {
                    var at=(y*96+x)*4;
                    for(var c=0;c<3;c++)
                    {
                        if(older is not null){curvature+=Math.Abs(pixels[at+c]-2*previous![at+c]+older[at+c]);samples++;}
                        detail+=Math.Pow(pixels[at+c+4]-pixels[at+c],2)+Math.Pow(pixels[at+c+384]-pixels[at+c],2);detailSamples++;
                    }
                }
                older=previous;previous=pixels;
                File.WriteAllBytes(Path.Combine(folder,$"{n:000}.pbgra"),pixels);
                RenderOptions.SetBitmapScalingMode(visual,BitmapScalingMode.NearestNeighbor);
                using(var dc=visual.RenderOpen())
                {
                    dc.DrawRectangle(Brushes.White,null,new Rect(0,0,480,384));
                    dc.DrawImage(frame,new Rect(0,144,96,96));dc.DrawImage(frame,new Rect(96,0,384,384));
                }
                var sweepProof=new RenderTargetBitmap(480,384,96,96,PixelFormats.Pbgra32);sweepProof.Render(visual);
                Save(sweepProof,Path.Combine(folder,$"{n:000}.png"));
            }
            times.Sort();var summary=System.Text.Json.JsonSerializer.Serialize(new{temporalCurvature=curvature/samples,spatialGradient=detail/detailSamples,medianMs=times[times.Count/2],p95Ms=times[(int)(times.Count*.95)]});
            File.WriteAllText(Path.Combine(folder,"metrics.json"),summary);Console.WriteLine(summary);return;
        }
        if(args[0]=="--mouth-metrics")
        {
            var source=UprightRumpSource.Standing(true);var pixels=PremultipliedFrame.From(source).Pixels;
            for(var y=54;y<=66;y++)Console.WriteLine($"{y}: "+string.Join(" ",Enumerable.Range(22,15).Select(x=>$"{pixels[(y*96+x)*4+2],3}")));
            for(var n=0;n<=12;n++)
            {
                var age=.48+n*1.07/12;var mouth=PremultipliedFrame.From(SnackMouthFrame.Chew(source,age)).Pixels;
                var strong=0;var delta=0;
                for(var i=0;i<pixels.Length;i+=4){var d=Math.Abs(pixels[i+2]-mouth[i+2]);delta+=d;if(d>=40)strong++;}
                Console.WriteLine($"age={age:F3} changedBy40={strong} sumRedDelta={delta}");
            }
            return;
        }
        if(args[0]=="--source-motion")
        {
            var h=new HuntRenderer();var sources=new List<BitmapSource>();
            foreach(var roll in new[]{-.349,-.22,-.1,0,.1,.22,.349})sources.Add(h.Render(156,new(.4,.3,roll)));
            foreach(var frame in new[]{14,15,16,20,152,155,156})sources.Add(h.Render(frame,default));
            for(var n=0;n<7;n++)sources.Add(LocomotionFrames.Walk(1,n*10d/7));
            for(var n=0;n<7;n++)sources.Add(BodyBreathingFrame.Standing(false,n/6d));
            for(var n=0;n<7;n++)sources.Add(LocomotionFrames.Sit(n/6d));
            RenderOptions.SetBitmapScalingMode(visual,BitmapScalingMode.NearestNeighbor);
            using(var dc=visual.RenderOpen())
            {
                for(var row=0;row<5;row++)for(var col=0;col<7;col++)
                {
                    var x=col*192;var y=row*384;
                    dc.DrawRectangle(Brushes.White,null,new Rect(x,y,192,192));
                    dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(29,40,49)),null,new Rect(x,y+192,192,192));
                    dc.DrawImage(sources[row*7+col],new Rect(x,y,192,192));
                    dc.DrawImage(sources[row*7+col],new Rect(x,y+192,192,192));
                }
            }
            var output=new RenderTargetBitmap(1344,1920,96,96,PixelFormats.Pbgra32);output.Render(visual);
            Save(output,Path.GetFullPath(args[1]));return;
        }
        if(args[0]=="--consistency")
        {
            var h=new HuntRenderer();
            var afterWalk=new DororongPresenter();
            var standing=new PetSnapshot(PetState.Idle,new(100,100),FacingDirection.Left,0,false,null);
            afterWalk.RenderDesktop(standing,Dororong.App.Interaction.DirectInteractionSnapshot.None,TimeSpan.FromMilliseconds(16));
            for(var n=1;n<=8;n++)afterWalk.RenderDesktop(standing with {State=PetState.Walk,Position=new(100-n*2,100)},Dororong.App.Interaction.DirectInteractionSnapshot.None,TimeSpan.FromMilliseconds(32));
            for(var n=0;n<40;n++)afterWalk.RenderDesktop(standing with {Position=new(84,100)},Dororong.App.Interaction.DirectInteractionSnapshot.None,TimeSpan.FromMilliseconds(16));
            var settledSource=(BitmapSource)((System.Windows.Controls.Image)afterWalk.FindName("DororongImage")).Source;
            var sources=new (string Name,BitmapSource Image)[]{
                ("standing",UprightRumpSource.Standing(false)),
                ("tracking-neutral-linear",h.Render(156,default)),
                ("tracking-neutral-nearest",h.Render(156,default,crisp:true)),
                ("locomotion-neutral",LocomotionFrames.Walk(0,0)),
                ("snack-neutral",render.Render(new(SnackPhase.Chasing,FacingDirection.Left,0,0,1,0),default)),
                ("walk-active",LocomotionFrames.Walk(1,0)),
                ("actual-idle-after-walk",settledSource)};
            var baseline=PremultipliedFrame.From(sources[0].Image).Pixels;
            var results=new List<object>();
            foreach(var (name,source) in sources)
            {
                var pixels=PremultipliedFrame.From(source).Pixels;
                int bodyChanged=0,maskChanged=0,area=0;double detail=0;int count=0;
                for(var y=73;y<90;y++)for(var x=13;x<83;x++)
                {
                    var at=(y*96+x)*4;
                    if(Enumerable.Range(0,4).Any(c=>pixels[at+c]!=baseline[at+c]))bodyChanged++;
                    if((pixels[at+3]>=128)!=(baseline[at+3]>=128))maskChanged++;
                    if(pixels[at+3]>=128)area++;
                }
                double L(int x,int y){var at=(y*96+x)*4;return (pixels[at]+pixels[at+1]+pixels[at+2])/3d;}
                for(var y=21;y<54;y++)for(var x=18;x<62;x++)
                {var lap=4*L(x,y)-L(x-1,y)-L(x+1,y)-L(x,y-1)-L(x,y+1);detail+=lap*lap;count++;}
                results.Add(new{name,bodyChanged,maskChanged,bodyArea=area,headDetail=detail/count});
            }
            RenderOptions.SetBitmapScalingMode(visual,BitmapScalingMode.NearestNeighbor);
            using(var dc=visual.RenderOpen())
            {
                dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(29,40,49)),null,new Rect(0,0,sources.Length*192,384));
                for(var i=0;i<sources.Length;i++)
                {
                    dc.DrawImage(sources[i].Image,new Rect(i*192+48,0,96,96));
                    dc.DrawImage(sources[i].Image,new Rect(i*192,128,192,192));
                }
            }
            var consistencyProof=new RenderTargetBitmap(sources.Length*192,384,96,96,PixelFormats.Pbgra32);consistencyProof.Render(visual);
            Save(consistencyProof,Path.GetFullPath(args[1]));Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(results));return;
        }
        if(args[0]=="--mouth")
        {
            RenderOptions.SetBitmapScalingMode(visual,BitmapScalingMode.NearestNeighbor);
            using(var dc=visual.RenderOpen())
            {
                dc.DrawRectangle(Brushes.White,null,new Rect(0,0,960,192));
                var ages=new[]{-1d,.32,.48,.64,.76,1.6};
                for(var i=0;i<ages.Length;i++)
                {
                    var source=ages[i]<0?UprightRumpSource.Standing(true):render.Render(new(SnackPhase.Eating,FacingDirection.Left,0,0,ages[i],0),default);
                    dc.PushClip(new RectangleGeometry(new Rect(i*160,0,160,192)));
                    dc.DrawImage(source,new Rect(i*160-20*8,-47*8,96*8,96*8));dc.Pop();
                }
            }
            var mouthProof=new RenderTargetBitmap(960,192,96,96,PixelFormats.Pbgra32);mouthProof.Render(visual);
            Save(mouthProof,Path.GetFullPath(args[1]));return;
        }
        if(args[0]=="--breathing")
        {
            var samples=new List<BitmapSource>();var metrics=new List<object>();
            foreach(var phase in new[]{0d,.19,.4,.7})
            {
                var p=new DororongPresenter();p.Measure(new Size(144,144));p.Arrange(new Rect(0,0,144,144));
                p.RenderDesktop(new(PetState.Idle,new(100,100),FacingDirection.Left,phase,false,null),Dororong.App.Interaction.DirectInteractionSnapshot.None,TimeSpan.Zero);
                p.UpdateLayout();
                BitmapSource Capture(){var b=new RenderTargetBitmap(144,144,96,96,PixelFormats.Pbgra32);b.Render(p);b.Freeze();return b;}
                var transform=(ScaleTransform)p.FindName("ImageBreathingScaleTransform");
                var sx=transform.ScaleX;var sy=transform.ScaleY;
                var breathing=Capture();samples.Add(breathing);
                transform.ScaleX=transform.ScaleY=1;p.UpdateLayout();
                var fixedScale=Capture();samples.Add(fixedScale);
                var a=PremultipliedFrame.From(breathing).Pixels;var b=PremultipliedFrame.From(fixedScale).Pixels;
                double Energy(byte[] data)
                {
                    double sum=0;var count=0;
                    double L(int x,int y){var at=(y*144+x)*4;return (data[at]+data[at+1]+data[at+2])/3d;}
                    for(var y=45;y<78;y++)for(var x=42;x<86;x++)
                    {var lap=4*L(x,y)-L(x-1,y)-L(x+1,y)-L(x,y-1)-L(x,y+1);sum+=lap*lap;count++;}
                    return sum/count;
                }
                metrics.Add(new{phase,sx,sy,breathingDetail=Energy(a),fixedDetail=Energy(b),changedBytes=a.Where((v,i)=>v!=b[i]).Count()});
            }
            RenderOptions.SetBitmapScalingMode(visual,BitmapScalingMode.NearestNeighbor);
            using(var dc=visual.RenderOpen())
            {
                dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(29,40,49)),null,new Rect(0,0,1152,576));
                for(var i=0;i<4;i++)for(var row=0;row<2;row++)dc.DrawImage(samples[i*2+row],new Rect(i*288,row*288,288,288));
            }
            var breathingProof=new RenderTargetBitmap(1152,576,96,96,PixelFormats.Pbgra32);breathingProof.Render(visual);
            Save(breathingProof,Path.GetFullPath(args[1]));Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(metrics));return;
        }
        if(args[0]=="--transition")
        {
            var presenter=new DororongPresenter();
            var standing=new PetSnapshot(PetState.Idle,new(100,100),FacingDirection.Left,0,false,null);
            var tick=TimeSpan.FromMilliseconds(16);
            void Step(PointerSample pointer)
            {
                presenter.UpdateHuntingWithPounce(standing,Dororong.App.Interaction.DirectInteractionSnapshot.None,pointer,tick,false);
                presenter.RenderDesktop(standing,Dororong.App.Interaction.DirectInteractionSnapshot.None,tick);
            }
            for(var i=0;i<60;i++)Step(new(true,new(230,181)));
            using(var dc=visual.RenderOpen())
            {
                dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(29,40,49)),null,new Rect(0,0,1152,432));
                for(var i=0;i<24;i++)
                {
                    dc.DrawImage((BitmapSource)((System.Windows.Controls.Image)presenter.FindName("DororongImage")).Source,new Rect((i%8)*144,(i/8)*144,144,144));
                    Step(PointerSample.Unavailable);
                }
            }
            var bitmap=new RenderTargetBitmap(1152,432,96,96,PixelFormats.Pbgra32);bitmap.Render(visual);
            Save(bitmap,Path.GetFullPath(args[1]));Console.WriteLine(args[1]);return;
        }
        if(args[0]=="--motion")
        {
            var folder=Path.GetFullPath(args[1]);Directory.CreateDirectory(folder);
            var run=new SnackRenderer();var jump=new SnackRenderer();var eat=new SnackRenderer();
            for(var i=0;i<180;i++)
            {
                var t=i/60d;var a=t%1.4;
                var jumpAge=a<SnackSession.JumpSeconds?a:-1;
                var flight=(jumpAge-SnackSession.JumpPrepareSeconds)/SnackSession.JumpFlightSeconds;
                var lift=jumpAge>=0?SnackSession.JumpLift(flight,60):0;
                var cycle=t%(SnackSession.EatingSeconds+SnackSession.HappySeconds+SnackSession.SettleSeconds);
                var phase=cycle<SnackSession.EatingSeconds?SnackPhase.Eating:
                    cycle<SnackSession.EatingSeconds+SnackSession.HappySeconds?SnackPhase.Happy:SnackPhase.Settling;
                var age=cycle-(phase==SnackPhase.Eating?0:phase==SnackPhase.Happy?SnackSession.EatingSeconds:SnackSession.EatingSeconds+SnackSession.HappySeconds);
                var frames=new[]{
                    run.Render(new(SnackPhase.Chasing,FacingDirection.Left,3,0,t,t*185){RunAmount=1},default),
                    jump.Render(new(SnackPhase.Chasing,FacingDirection.Left,0,lift,t,0){JumpAge=jumpAge},default),
                    eat.Render(new(phase,FacingDirection.Left,0,0,age,0),default)};
                using(var dc=visual.RenderOpen())
                {
                    dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(29,40,49)),null,new Rect(0,0,576,240));
                    for(var n=0;n<3;n++)dc.DrawImage(frames[n],new Rect(n*192,48+(n==1?lift:0),192,192));
                }
                var bitmap=new RenderTargetBitmap(576,240,96,96,PixelFormats.Pbgra32);bitmap.Render(visual);
                Save(bitmap,Path.Combine(folder,$"{i:000}.png"));
            }
            Console.WriteLine(folder);return;
        }
        if(args[0]=="--native-smoke")
        {
            var errors=new List<string>();
            var window=new Dororong.App.MainWindow((code,error)=>{if(error is not null)errors.Add(error.ToString());});
            var timer=new DispatcherTimer {Interval=TimeSpan.FromMilliseconds(350)};var step=0;
            timer.Tick+=(_,_)=>
            {
                try
                {
                    if(step++==0)
                    {
                        window.ToggleSnack();
                        if(!window.IsSnackActive)throw new Exception("Snack did not start");
                    }
                    else
                    {
                        if(window.IsSnackActive)window.ToggleSnack();
                        if(window.IsSnackActive)throw new Exception("Snack did not cancel");
                        timer.Stop();window.Close();
                    }
                }
                catch(Exception ex){errors.Add(ex.ToString());timer.Stop();window.Close();}
            };
            window.Loaded+=(_,_)=>timer.Start();
            Application.Current.Run(window);
            if(errors.Count>0)throw new Exception(string.Join("\n",errors));
            Console.WriteLine("PASS: real MainWindow/loop snack start, cancel and close; no diagnostic errors");return;
        }
        if(args[0]=="--benchmark")
        {
            var times=new List<double>();
            for(var i=0;i<150;i++)
            {
                var watch=System.Diagnostics.Stopwatch.StartNew();
                render.Render(new(SnackPhase.Chasing,FacingDirection.Left,2.9,0,i*.016,i*2.9){RunAmount=1},default);
                watch.Stop();if(i>=10)times.Add(watch.Elapsed.TotalMilliseconds);
            }
            times.Sort();Console.WriteLine($"render ms median={times[times.Count/2]:F2}, p95={times[(int)(times.Count*.95)]:F2}");return;
        }
        using(var dc=visual.RenderOpen())
        {
            dc.DrawRectangle(Brushes.White,null,new Rect(0,0,1152,576));
            dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(29,40,49)),null,new Rect(0,288,1152,288));
            for(var row=0;row<3;row++)for(var i=0;i<8;i++)
            {
                var eating=row==2;
                var frame=render.Render(new(eating?SnackPhase.Eating:SnackPhase.Chasing,FacingDirection.Left,
                    eating?0:2.9,0,eating?i*.22:i*.12,i*SnackRunFrames.Stride/8)
                    {RunAmount=row==0?1:0,JumpAge=row==1?i*SnackSession.JumpSeconds/7:-1},default);
                dc.DrawImage(frame,new Rect(i*144,row*192,144,144));
            }
        }
        var proof=new RenderTargetBitmap(1152,576,96,96,PixelFormats.Pbgra32);proof.Render(visual);
        var path=Path.GetFullPath(args[0]);Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(proof));using var stream=File.Create(path);encoder.Save(stream);
        Console.WriteLine(path);
    }
    static void Save(BitmapSource bitmap,string path)
    {
        var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var file=File.Create(path);encoder.Save(file);
    }
}

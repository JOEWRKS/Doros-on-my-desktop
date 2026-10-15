using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using P = Dororong.Portable.Rendering;
using W = Dororong.App.Controls;

public sealed class RendererParityTests
{
    [Fact]
    public void CompareAgainstAcceptedWindowsBinary()
    {
        Exception? error=null;
        var thread=new Thread(()=>{try{Compare();}catch(Exception ex){error=ex;}});
        thread.SetApartmentState(ApartmentState.STA);thread.Start();thread.Join();
        if(error is not null)System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error).Throw();
    }
    private static byte[] Pixels(BitmapSource source)
    {
        var frame=new FormatConvertedBitmap(source,PixelFormats.Pbgra32,null,0);
        var data=new byte[frame.PixelWidth*frame.PixelHeight*4];frame.CopyPixels(data,frame.PixelWidth*4,0);return data;
    }
    private static void Compare()
    {
        _=new System.Windows.Application();
        var baseline=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(W.DororongPresenter).Assembly.Location)));
        Assert.Equal("EF1603705D1CB6073E9D0DBFF8562EAC94E8D4EC920B29A9A9FEB47319E6274C",baseline);
        Assert.Equal(baseline,P.JumpBank.Instance.BaselineHash);
        RefinementParity.Run();
        var wgaze=new W.HuntGaze();var pgaze=new P.HuntGaze();
        var wloco=new W.LocomotionPresentation();var ploco=new P.LocomotionPresentation();
        for(var tick=0;tick<240;tick++)
        {
            PointD? target=tick<90?new(130,-45):null;var flip=tick>45;var active=tick<90;
            var wg=wgaze.Advance(.016,target,flip,active);var pg=pgaze.Advance(.016,target,flip,active);
            Assert.Equal(wg.EyeX,pg.EyeX);Assert.Equal(wg.EyeY,pg.EyeY);Assert.Equal(wg.Roll,pg.Roll);
            wloco.Advance(16,tick<70,.336,false,tick is >=70 and <150);
            ploco.Advance(16,tick<70,.336,false,tick is >=70 and <150);
            Assert.Equal(wloco.Walk,ploco.Walk);Assert.Equal(wloco.Sit,ploco.Sit);
            Assert.Equal(wloco.Distance,ploco.Distance);Assert.Equal(wloco.UsesRisePose,ploco.UsesRisePose);
        }
        var exact=0;
        void Exact(BitmapSource windows,P.RasterImage portable,string label)
        {
            var expected=Pixels(windows);var actual=portable.Pixels.ToArray();
            var index=Enumerable.Range(0,expected.Length).FirstOrDefault(i=>expected[i]!=actual[i],-1);
            Assert.True(index<0,$"{label}: first difference at {index}, expected {(index<0?0:expected[index])}, got {(index<0?0:actual[index])}");exact++;
        }
        foreach(var closed in new[]{false,true})
        {
            for(var breath=0;breath<=32;breath++)Exact(W.BodyBreathingFrame.Standing(closed,breath/32d),P.MotionRenderer.Standing(closed,breath/32d),$"breath {closed}/{breath}");
            for(var level=0;level<=8;level++)for(var phase=0;phase<32;phase++)
                Exact(W.LocomotionFrames.Walk(level/8d,phase*10d/32,closed),P.MotionRenderer.Walk(level/8d,phase*10d/32,closed),$"walk {closed}/{level}/{phase}");
        }
        for(var level=0;level<=8;level++)for(var phase=0;phase<48;phase++)
            Exact(W.SnackRunFrames.Render(phase*66d/48,level/8d),P.SnackRunFrames.Render(phase*66d/48,level/8d),$"run {level}/{phase}");
        var hunt=new W.HuntRenderer();
        for(var frame=0;frame<=156;frame++)Exact(hunt.Render(frame,default,crisp:true),P.JumpBank.Instance.Frame(frame),$"jump {frame}");
        for(var i=0;i<=170;i++)
        {
            var age=i/100d;
            Exact(W.SnackMouthFrame.Chew(W.UprightRumpSource.Standing(true),age),P.SnackMouthFrame.Chew(P.MotionRenderer.Standing(true),age),$"chew {age}");
            Assert.Equal(W.SnackRenderer.HappyRoll(age),P.SnackRenderer.HappyRoll(age));
            Assert.Equal(W.SnackRenderer.JumpFrame(age),P.SnackRenderer.JumpFrame(age));
        }
        Exact(W.PerchExpressionFrames.Open,P.DesktopFrames.PerchSource(false),"perch open");
        Exact(W.PerchExpressionFrames.Closed,P.DesktopFrames.PerchSource(true),"perch closed");
        for(var phase=0;phase<16;phase++)
        {
            var data=W.ForelegFlutterFrame.Render(W.PremultipliedFrame.From(W.UprightRumpSource.Standing(false)),false,phase/16d,null);
            Assert.Equal(data,P.ForelegFlutterFrame.Render(P.MotionRenderer.Standing(),false,phase/16d,null));exact++;
        }
        var wExpression=new W.PerchExpressionMotion();var pExpression=new P.PerchExpressionMotion();
        for(var tick=0;tick<350;tick++)
        {
            wExpression.Advance(TimeSpan.FromMilliseconds(16));pExpression.Advance(TimeSpan.FromMilliseconds(16));
            Assert.Equal(wExpression.ScaleY,pExpression.ScaleY);Assert.Equal(wExpression.EyesClosed,pExpression.EyesClosed);
        }
        foreach(var flip in new[]{false,true})
        {
            var contact=new W.PlatformContactPresentation();
            var transform=new MatrixTransform(flip?new Matrix(-1,0,0,1,120,24):new Matrix(1,0,0,1,24,24));
            var measured=contact.Measure(W.UprightRumpSource.Standing(false),transform);
            Assert.Equal(measured,P.DesktopFrames.Measure(P.DesktopFrames.Place(P.MotionRenderer.Standing(),flip)).Contact);
        }
        // Continuous eye deformation and bounded cubic rotation must match byte-for-byte.
        var eye=new W.HuntEyes();var sampling=new W.HuntHeadSampling();
        var pEye=new P.HuntEyes();var pSampling=new P.HuntHeadSampling();
        var tracking=new P.TrackingRenderer();var metrics=new List<object>();
        foreach(var closed in new[]{false,true})foreach(var roll in new[]{-.34,-.17,0,.17,.34})
        foreach(var x in new[]{-.85,0,.85})
        {
            var gaze=new W.HuntGazePose(x,.4,roll);var pg=new P.HuntGazePose(x,.4,roll);
            Exact(sampling.Render(eye.Paint(x,.4,closed),W.HuntRenderer.HeadTransform(156,gaze)),
                pSampling.Render(pEye.Paint(x,.4,closed),P.TrackingRenderer.HeadTransform(pg)),$"head {closed}/{roll}/{x}");
            metrics.Add(Difference(Pixels(hunt.Render(156,gaze,closed)),tracking.Render(pg,closed).Pixels.ToArray(),$"tracking {closed}/{roll}/{x}"));
            Exact(hunt.Render(156,gaze,closed),tracking.Render(pg,closed),$"upright composite {closed}/{roll}/{x}");
        }
        var snack=new W.SnackRenderer();var pSnack=new P.SnackRenderer();
        foreach(var phase in new[]{SnackPhase.Chasing,SnackPhase.Eating,SnackPhase.Happy,SnackPhase.Settling})
        foreach(var age in new[]{0,.08,.22,.34,.55,.95,1.4,1.65,2.1})
        {
            var pose=new SnackPose(phase,FacingDirection.Right,0,0,age,20){RunAmount=phase==SnackPhase.Chasing?1:0};
            metrics.Add(Difference(Pixels(snack.Render(pose,default)),pSnack.Render(pose).Pixels.ToArray(),$"snack {phase}/{age}"));
        }
        foreach(var jumpAge in new[]{0,.08,.17,.18,.24,.35,.5,.65,.66,.75,.86})
        {
            var pose=new SnackPose(SnackPhase.Chasing,FacingDirection.Right,0,0,.8,20){JumpAge=jumpAge};
            var portable=pSnack.Render(pose);
            metrics.Add(Difference(Pixels(snack.Render(pose,default)),portable.Pixels.ToArray(),$"snack jump/{jumpAge}",pSnack.HeadTransform));
        }
        InteractionParity.Run(Exact);
        DirectParity.Run(Exact);
        SaveComparison(hunt,tracking,snack,pSnack);
        File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"parity-report.json"),JsonSerializer.Serialize(new{baseline,exactFrames=exact,stateTicks=240,metrics},new JsonSerializerOptions{WriteIndented=true}));
    }
    private static object Difference(byte[] a,byte[] b,string label,P.Affine? effectTransform=null)
    {
        var count=0;var max=0;long sum=0;var outside=0;
        for(var i=0;i<a.Length;i+=4)
        {
            var changed=false;
            for(var c=0;c<4;c++){var d=Math.Abs(a[i+c]-b[i+c]);max=Math.Max(max,d);sum+=d;changed|=d>0;}
            if(changed)
            {
                count++;var y=i/384;
                // Crouch carries drool below Y74; exclude only its transformed
                // local effect bounds, not the rest of the legs/feet.
                var p=(effectTransform??P.Affine.Identity).Inverse().Transform(new(i/4%96+.5,y+.5));
                var movedDrool=effectTransform.HasValue&&p.X>=28&&p.X<=35&&p.Y>=60&&p.Y<=76;
                if(y>74&&!movedDrool)outside++;
            }
        }
        Assert.True(outside==0,$"{label}: {outside} differences outside vector region.");
        Assert.True(count<=64,$"{label}: {count} differing pixels exceeds vector-only budget.");
        Assert.True(max<=12,$"{label}: channel delta {max} exceeds edge budget.");
        Assert.True(sum/(double)a.Length<=.015,$"{label}: average channel error exceeds .015/255.");
        return new{label,pixels=count,maxChannelDelta=max,meanChannelDelta=sum/(double)a.Length};
    }
    private static void SaveComparison(W.HuntRenderer hunt,P.TrackingRenderer tracking,W.SnackRenderer snack,P.SnackRenderer portable)
    {
        var poses=new List<(string,BitmapSource,P.RasterImage)>
        {
            ("WALK",W.LocomotionFrames.Walk(1,2),P.MotionRenderer.Walk(1,2)),
            ("RUN",W.SnackRunFrames.Render(24,1),P.SnackRunFrames.Render(24,1)),
            ("TRACK",hunt.Render(156,new(.8,.4,.34)),tracking.Render(new(.8,.4,.34)))
        };
        foreach(var (label,phase,age,jump) in new[]{("JUMP",SnackPhase.Chasing,.4,.3),("BITE",SnackPhase.Eating,.22,-1d),("CHEW",SnackPhase.Eating,.7,-1d),("HAPPY",SnackPhase.Happy,1.1,-1d)})
        {
            var pose=new SnackPose(phase,FacingDirection.Right,0,0,age,20){JumpAge=jump};
            poses.Add((label,snack.Render(pose,default),portable.Render(pose)));
        }
        using var sheet=new SkiaSharp.SKBitmap(520,poses.Count*150+35);
        using var canvas=new SkiaSharp.SKCanvas(sheet);canvas.Clear(new SkiaSharp.SKColor(240,238,232));
        using var ink=new SkiaSharp.SKPaint{Color=SkiaSharp.SKColors.Black,TextSize=15,IsAntialias=true};
        using var nearest=new SkiaSharp.SKPaint{FilterQuality=SkiaSharp.SKFilterQuality.None};
        canvas.DrawText("WINDOWS",115,24,ink);canvas.DrawText("PORTABLE",320,24,ink);
        for(var i=0;i<poses.Count;i++)
        {
            var (label,w,p)=poses[i];canvas.DrawText(label,8,35+i*150+70,ink);
            using var wb=new P.RasterImage(96,96,Pixels(w)).ToBitmap();using var pb=p.ToBitmap();
            canvas.DrawBitmap(wb,SkiaSharp.SKRect.Create(100,35+i*150,144,144),nearest);
            canvas.DrawBitmap(pb,SkiaSharp.SKRect.Create(305,35+i*150,144,144),nearest);
        }
        using var image=SkiaSharp.SKImage.FromBitmap(sheet);using var data=image.Encode(SkiaSharp.SKEncodedImageFormat.Png,100);
        using var output=File.Create(Path.Combine(AppContext.BaseDirectory,"motion-parity.png"));data.SaveTo(output);
    }
}

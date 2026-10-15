using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.Core.Behavior;
using P=Dororong.Portable.Rendering;
using W=Dororong.App.Controls;

internal static class RefinementParity
{
    internal static void Run()
    {
        var rows=new List<object>();var proof=new List<(string,byte[],P.RasterImage)>();var failures=new List<string>();
        void Compare(string label,BitmapSource w,P.RasterImage p,int cap,double meanCap)
        {
            var expected=Pixels(w);var actual=p.CopyPixels();var max=0;long sum=0;var changed=0;
            for(var i=0;i<expected.Length;i++){var d=Math.Abs(expected[i]-actual[i]);max=Math.Max(max,d);sum+=d;if(d>0)changed++;}
            var mean=sum/(double)expected.Length;rows.Add(new{label,max,mean,changed});
            // Persist failure metrics too; never turn a broad old tolerance into proof.
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"refinement-parity.json"),JsonSerializer.Serialize(rows,new JsonSerializerOptions{WriteIndented=true}));
            if(max>cap||mean>meanCap)failures.Add($"{label}: max {max}, mean {mean}, changed channels {changed}");
            for(var i=0;i<actual.Length;i+=4)Assert.True(actual[i]<=actual[i+3]&&actual[i+1]<=actual[i+3]&&actual[i+2]<=actual[i+3],label+" invalid premultiplication");
        }
        var wh=new W.HuntRenderer();var ph=new P.HuntingRenderer();
        for(var f=0;f<=156;f++)
        {
            var roll=.349*Math.Sin(f*1.719+.217);var gaze=new W.HuntGazePose(.85*Math.Cos(f*.43),.65*Math.Sin(f*.63),roll);var closed=f%2==0;
            var pull=f%3==0?20:f%3==1?-10:0;
            var walk=f%3==0?W.LocomotionFrames.Walk(.5,2,closed):null;
            var pw=walk is null?null:P.MotionRenderer.Walk(.5,2,closed);
            var w=wh.Render(f,gaze,closed,pull,7,4,walkingFrame:walk,breathing:.731);
            var p=ph.Render(f,new(gaze.EyeX,gaze.EyeY,roll),closed,.731,pw,pull,7,4);
            Compare($"continuous-hunt-{f}",w,p,8,.02);
            if(f is 0 or 16 or 24 or 42 or 75 or 130 or 150)proof.Add(($"FRAME {f}",Pixels(w),p));
        }
        var ws=new W.SnackRenderer();var ps=new P.SnackRenderer();
        for(var i=0;i<=240;i++)
        {
            var age=i*.002013;var pose=new SnackPose(SnackPhase.Eating,FacingDirection.Right,0,0,age,0);
            var w=ws.Render(pose,default);var p=ps.Render(pose);Compare($"bite-{i}",w,p,3,.003);
            if(i is 80 or 130)proof.Add(($"BITE {i}",Pixels(w),p));
        }
        for(var level=0;level<=8;level++)for(var phase=0;phase<48;phase++)
        {
            var pose=new SnackPose(SnackPhase.Chasing,FacingDirection.Right,0,0,.317+phase*.0331,phase*66d/48){RunAmount=level/8d};
            Compare($"run-drool-{level}-{phase}",ws.Render(pose,default),ps.Render(pose),8,.01);
        }
        for(var i=0;i<87;i++)
        {
            var pose=new SnackPose(SnackPhase.Chasing,FacingDirection.Right,0,0,.3181+i*.0317,20){JumpAge=i*.01};
            Compare($"jump-drool-{i}",ws.Render(pose,default),ps.Render(pose),8,.01);
        }
        for(var i=0;i<24;i++)
        {
            var source=P.HeadPullFrames.Sample(i/23d);var a=.5*Math.Sin(i*.713);var c=Math.Cos(a);var s=Math.Sin(a);var flip=i%2==0?-1:1;
            var t=new P.Affine(flip*c,flip*s,-s,c,flip<0?210.213:119.173,104.327);const int size=336;
            var visual=new DrawingVisual();RenderOptions.SetBitmapScalingMode(visual,BitmapScalingMode.Linear);
            var b=BitmapSource.Create(96,96,96,96,PixelFormats.Pbgra32,null,source.CopyPixels(),384);
            using(var dc=visual.RenderOpen()){dc.PushTransform(new MatrixTransform(t.M11,t.M12,t.M21,t.M22,t.OffsetX,t.OffsetY));dc.DrawImage(b,new Rect(0,0,96,96));dc.Pop();}
            var expected=new RenderTargetBitmap(size,size,96,96,PixelFormats.Pbgra32);expected.Render(visual);
            Compare($"swing-composite-{i}",expected,P.DesktopFrames.Warp(source,t,size),3,.006);
        }
        Save(proof);
        Assert.True(failures.Count==0,string.Join("\n",failures));
        var before=GC.GetAllocatedBytesForCurrentThread();var times=new List<double>();
        for(var i=20;i<100;i++)
        {var clock=System.Diagnostics.Stopwatch.StartNew();ph.Render(i,new(.4,-.3,.21731));times.Add(clock.Elapsed.TotalMilliseconds);}
        var bytes=(GC.GetAllocatedBytesForCurrentThread()-before)/80d;times.Sort();
        File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"refinement-performance.json"),JsonSerializer.Serialize(new{frames=80,medianMs=times[40],p95Ms=times[76],allocatedBytesPerFrame=bytes,scope="Windows CPU renderer only; not Mac timing"}));
        Assert.True(bytes<8_000_000,$"Full-bank decode regression: {bytes} bytes/frame");
    }
    private static byte[] Pixels(BitmapSource image)
    {var data=new byte[image.PixelWidth*image.PixelHeight*4];image.CopyPixels(data,image.PixelWidth*4,0);return data;}
    private static void Save(List<(string Label,byte[] W,P.RasterImage P)> frames)
    {
        using var bitmap=new SkiaSharp.SKBitmap(1040,frames.Count*430+40);using var canvas=new SkiaSharp.SKCanvas(bitmap);canvas.Clear(new SkiaSharp.SKColor(239,237,230));
        using var ink=new SkiaSharp.SKPaint{TextSize=16,Color=SkiaSharp.SKColors.Black};using var paint=new SkiaSharp.SKPaint{FilterQuality=SkiaSharp.SKFilterQuality.None};
        canvas.DrawText("1x + 4x INTEGER / WINDOWS LEFT, PORTABLE RIGHT",20,23,ink);
        for(var i=0;i<frames.Count;i++)
        {
            var f=frames[i];var y=40+i*430;canvas.DrawText(f.Label,8,y+17,ink);
            using var w=new P.RasterImage(96,96,f.W).ToBitmap();using var p=f.P.ToBitmap();
            using var bg=new SkiaSharp.SKPaint{Color=new(30,40,48)};canvas.DrawRect(200,y,840,420,bg);
            canvas.DrawBitmap(w,0,y+30,paint);canvas.DrawBitmap(p,96,y+30,paint);
            canvas.DrawBitmap(w,SkiaSharp.SKRect.Create(220,y+24,384,384),paint);canvas.DrawBitmap(p,SkiaSharp.SKRect.Create(640,y+24,384,384),paint);
        }
        using var image=SkiaSharp.SKImage.FromBitmap(bitmap);using var png=image.Encode(SkiaSharp.SKEncodedImageFormat.Png,100);using var output=File.Create(Path.Combine(AppContext.BaseDirectory,"refinement-proof.png"));png.SaveTo(output);
    }
}

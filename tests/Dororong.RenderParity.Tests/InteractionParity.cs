using System.IO;
using System.Text.Json;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using P=Dororong.Portable.Rendering;
using W=Dororong.App.Controls;
using I=Dororong.App.Interaction;

internal static class InteractionParity
{
    internal static void Run(Action<BitmapSource,P.RasterImage,string> exact)
    {
        var wPounce=new W.PounceSession();var pPounce=new P.PounceSession();var wh=new W.HuntingSession();var ph=new P.HuntingSession();
        var ws=new I.HeadSwingSession();var ps=new P.HeadSwingSession();
        for(var i=0;i<600;i++)
        {
            var near=i is <50 or >=70 and <400;var blocked=i is >=500;var x=50*Math.Sin(i*.07);
            var w=wPounce.Advance(.016,near,x,FacingDirection.Right,blocked);var p=pPounce.Advance(.016,near,x,FacingDirection.Right,blocked);
            Assert.Equal((int)w.Phase,(int)p.Phase);Assert.Equal(w.Direction,p.Direction);
            Assert.Equal(new[]{w.DeltaX,w.OffsetY,w.Age,w.TrackingRemaining,w.AngleRadians,w.ScaleX,w.ScaleY},new[]{p.DeltaX,p.OffsetY,p.Age,p.TrackingRemaining,p.AngleRadians,p.ScaleX,p.ScaleY});
            wh.Advance(.016,near,blocked);ph.Advance(.016,near,blocked);Assert.Equal(wh.FrameAgeSeconds,ph.FrameAgeSeconds);Assert.Equal(wh.IsActive,ph.IsActive);
            var pointer=new PointD(x*10,400);var settle=i>=550;var release=settle?(i-550)/50d:0;
            Assert.Equal(ws.Advance(TimeSpan.FromMilliseconds(16),pointer,true,settle?I.DirectInteractionPhase.BodyDragSettle:I.DirectInteractionPhase.BodyDragHold,release),
                ps.Advance(TimeSpan.FromMilliseconds(16),pointer,true,settle?P.DirectInteractionPhase.BodyDragSettle:P.DirectInteractionPhase.BodyDragHold,release));
        }
        BitmapSource Load(string name)=>new BitmapImage(new Uri("pack://application:,,,/Dororong.App;component/Assets/"+name+".png"));
        var canonical=Load("dororong-canonical");var head=new W.SuppliedBodyDragFrames(W.PremultipliedFrame.From(canonical));
        for(var i=0;i<=224;i++)
        {
            var p=i/224d;exact(head.Sample(p),P.HeadPullFrames.Sample(p),$"pull {i}");
            Assert.Equal(W.HeadPullAnchoring.RegistrationPoint(head.Sample(p)),P.HeadPullAnchoring.RegistrationPoint(P.HeadPullFrames.Sample(p)));
        }
        foreach(var strength in new[]{.13,.5,1d})
        {
            var recover=head.CreateRecovery(head.Sample(strength),canonical);var port=P.HeadPullFrames.Recovery(P.HeadPullFrames.Sample(strength),strength);
            for(var i=0;i<=18;i++)exact(recover(i/18d),port(i/18d),$"release {strength}/{i}");
        }
        var names=new[]{"dororong-canonical","dororong-blink-squint","dororong-closed-eyes","dororong-sleep-crouch-closed","dororong-sleep-tuck-closed","dororong-sleep"};
        var sources=names.Select(Load).ToArray();var edges=new[]{0,.0225,.045,.070,.100,.135};
        for(var i=0;i<5;i++)for(var j=0;j<=10;j++)
        {var sequence=new W.PremultipliedFrameSequence([W.PremultipliedFrame.From(sources[i]),W.PremultipliedFrame.From(sources[i+1])]);var phase=edges[i]+(edges[i+1]-edges[i])*j/10d;var p=(phase-edges[i])/(edges[i+1]-edges[i]);exact(sequence.Sample(p),P.RestPresentation.SleepFrame(phase),$"sleep {i}/{j}");}
        var renderer=new W.HuntRenderer();var portRenderer=new P.HuntingRenderer();var metrics=new List<object>();
        var comparisons=new List<(string,byte[],P.RasterImage)>();
        foreach(var frame in new[]{16,24,42,75,96,130,150})foreach(var closed in new[]{false,true})foreach(var roll in new[]{-.34,.17,.34})
        {
            var gaze=new W.HuntGazePose(.4,-.3,roll);var p=portRenderer.Render(frame,new(.4,-.3,roll),closed);
            var w=Pixels(renderer.Render(frame,gaze,closed));var actual=p.CopyPixels();var changed=0;var max=0;long sum=0;
            for(var i=0;i<w.Length;i+=4){var diff=false;for(var c=0;c<4;c++){var d=Math.Abs(w[i+c]-actual[i+c]);sum+=d;max=Math.Max(max,d);diff|=d>0;}if(diff)changed++;}
            metrics.Add(new{frame,closed,roll,changed,max,mean=sum/(double)w.Length});
            Assert.True(max<=4&&sum/(double)w.Length<=.02,$"Hunt raster drift {frame}/{closed}/{roll}: max={max}, mean={sum/(double)w.Length}");
            if(roll==.17)comparisons.Add(($"HUNT {frame} {closed}",w,p));
        }
        foreach(var strength in new[]{0,.5,1d})comparisons.Add(($"PULL {strength}",Pixels(head.Sample(strength)),P.HeadPullFrames.Sample(strength)));
        foreach(var strength in new[]{.2,.7,1d})foreach(var direction in new[]{-1,1})
        {
            var w=Pixels(head.Sample(strength));var p=P.HeadPullFrames.Sample(strength).CopyPixels();
            var point=new PointD(34,25);var follow=new PointD(direction*12,3);var far=new PointD(direction*8,1);
            Assert.Equal(W.SurroundingPullRenderer.Head(w,point,follow,far),P.SurroundingPullRenderer.Head(p,point,follow,far));
        }
        Save(comparisons);
        File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"interaction-parity.json"),JsonSerializer.Serialize(metrics,new JsonSerializerOptions{WriteIndented=true}));
    }
    private static byte[] Pixels(BitmapSource image)
    {var source=new FormatConvertedBitmap(image,PixelFormats.Pbgra32,null,0);var pixels=new byte[source.PixelWidth*source.PixelHeight*4];source.CopyPixels(pixels,source.PixelWidth*4,0);return pixels;}
    private static void Save(List<(string Label,byte[] Windows,P.RasterImage Portable)> poses)
    {
        using var bitmap=new SkiaSharp.SKBitmap(540,poses.Count*130+30);using var canvas=new SkiaSharp.SKCanvas(bitmap);canvas.Clear(new SkiaSharp.SKColor(235,233,227));
        using var ink=new SkiaSharp.SKPaint{TextSize=13,Color=SkiaSharp.SKColors.Black};using var paint=new SkiaSharp.SKPaint{FilterQuality=SkiaSharp.SKFilterQuality.None};
        canvas.DrawText("WINDOWS / PORTABLE",170,20,ink);
        for(var i=0;i<poses.Count;i++){var p=poses[i];canvas.DrawText(p.Label,4,80+i*130,ink);using var w=new P.RasterImage(96,96,p.Windows).ToBitmap();using var m=p.Portable.ToBitmap();canvas.DrawBitmap(w,SkiaSharp.SKRect.Create(150,30+i*130,120,120),paint);canvas.DrawBitmap(m,SkiaSharp.SKRect.Create(350,30+i*130,120,120),paint);}
        using var image=SkiaSharp.SKImage.FromBitmap(bitmap);using var data=image.Encode(SkiaSharp.SKEncodedImageFormat.Png,100);using var output=File.Create(Path.Combine(AppContext.BaseDirectory,"interaction-parity.png"));data.SaveTo(output);
    }
}

using System.IO;
using System.Text.Json;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using P=Dororong.Portable.Rendering;
using W=Dororong.App.Controls;
using I=Dororong.App.Interaction;

internal static class DirectParity
{
    internal static void Run(Action<BitmapSource,P.RasterImage,string> exact)
    {
        for(var i=0;i<=500;i++)
        {
            var w=W.BodyClickTransformSampler.SampleConfirmedClick(i/500d);var p=P.BodyClickTransformSampler.SampleConfirmedClick(i/500d);
            Assert.Equal(w.ScaleX,p.ScaleX);Assert.Equal(w.ScaleY,p.ScaleY);Assert.Equal(w.TranslationY,p.TranslationY);
        }
        var spread=typeof(W.DororongPresenter).Assembly.GetType("Dororong.App.Controls.ExtremeLandingPresentation")!.GetMethod("Spread",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static)!;
        var canonical=P.HeadPullFrames.Canonical.CopyPixels();
        for(var i=0;i<=100;i++)Assert.Equal((byte[])spread.Invoke(null,new object[]{canonical,i/100d})!,P.ExtremeLandingFrame.Spread(canonical,i/100d));
        var source=W.PerchExpressionFrames.RegisteredOpen();
        foreach(var pull in new[]{-10d,-5,0,1,3,8,15,20})foreach(var follow in new[]{0d,7,20})
            Assert.Equal(W.OutlineCheekRenderer.Render(source,pull,follow,follow*.7),P.OutlineCheekRenderer.Render(source,pull,follow,follow*.7));
        for(var ms=0;ms<=500;ms++)
        {
            Assert.Equal(I.CheekSpringMotion.Remaining(ms),P.CheekSpringMotion.Remaining(ms));
            foreach(var pull in new[]{0d,1,5,15,20})
            {Assert.Equal(I.CheekSpringMotion.Offset(ms,pull),P.CheekSpringMotion.Offset(ms,pull));Assert.Equal(I.CheekSpringMotion.Lift(ms,pull),P.CheekSpringMotion.Lift(ms,pull));}
        }
        var wfollow=new I.FaceFollowMotion();var pfollow=new P.FaceFollowMotion();
        for(var i=0;i<100;i++)
        {
            if(i==30){wfollow.BeginRelease(true);pfollow.BeginRelease(true);}
            wfollow.Step(20,16);pfollow.Step(20,16);Assert.Equal(wfollow.Eye,pfollow.Eye);Assert.Equal(wfollow.Hair,pfollow.Hair);
        }
        for(var y=0;y<96;y++)for(var x=0;x<96;x++)
        {
            PointD point=new(x+.25,y+.25);
            Assert.Equal(I.InteractionHitMap.IsUpperHead(point),P.InteractionHitMap.IsUpperHead(point));
            Assert.Equal(I.InteractionHitMap.IsSelectedCheek(point),P.InteractionHitMap.IsSelectedCheek(point));
        }
        var weyes=new W.HuntEyes();var peyes=new P.HuntEyes();
        foreach(var closed in new[]{false,true})foreach(var pull in new[]{-10d,1,5,20})
            exact(weyes.Paint(.4,-.3,closed,pull,7,4),peyes.Paint(.4,-.3,closed,pull,7,4),$"cheek head {closed}/{pull}");
        var pawMetrics=new List<object>();
        var sheetFrames=new List<(string Label,byte[] Expected,P.RasterImage Actual)>();
        var huntMetrics=new List<object>();var whunt=new W.HuntRenderer();var phunt=new P.HuntingRenderer();
        foreach(var frame in new[]{0,16,42,75,150})foreach(var pull in new[]{-10d,5,20})
        {
            var expected=Pixels(whunt.Render(frame,new(.4,-.3,.17),false,pull,7,4));
            var actual=phunt.Render(frame,new(.4,-.3,.17),cheekPull:pull,eyePull:7,hairPull:4);
            var diffs=expected.Zip(actual.CopyPixels(),(a,b)=>Math.Abs(a-b)).ToArray();
            var max=diffs.Max();var mean=diffs.Average();Assert.True(max<=4&&mean<=.02,$"Cheek hunt {frame}/{pull}: {max}/{mean}");
            huntMetrics.Add(new{frame,pull,max,mean});if(pull==20)sheetFrames.Add(($"HUNT {frame}",expected,actual));
        }
        foreach(var right in new[]{false,true})foreach(var flip in new[]{false,true})foreach(var drag in new[]{false,true})
        {
            var w=new I.DirectInteractionController();var p=new P.PawPresentation();var origin=new PointD(300,400);
            var target=right?I.DirectInteractionTarget.PerchRightPaw:I.DirectInteractionTarget.PerchLeftPaw;
            w.BeginPerchPaw(target,origin);w.SetPressContext(flip?FacingDirection.Left:FacingDirection.Right,false);p.Begin(right,origin,flip);
            for(var i=0;i<60;i++)
            {
                var pointer=new PointerSample(true,origin+(drag?new PointD(20,35):default));var down=i<10;
                var state=w.Advance(TimeSpan.FromMilliseconds(16),pointer,down,PetState.Idle,PetState.Idle);p.Advance(16,pointer.Position,down);
                Assert.Equal(state.PawPull?.Offset,p.Current?.Offset);Assert.Equal(state.PawPull?.Angle,p.Current?.Angle);
            }
        }
        foreach(var closed in new[]{false,true})foreach(var right in new[]{false,true})foreach(var offset in new[]{new PointD(0,0),new(0,18),new(12,12),new(-12,-12)})foreach(var angle in new[]{-22d,0,22})
        {
            var ws=closed?W.PerchExpressionFrames.Closed:W.PerchExpressionFrames.Open;var ps=P.DesktopFrames.PerchSource(closed);
            var wp=new I.PerchPawSnapshot(right,offset,angle);var pp=new P.PerchPawSnapshot(right,offset,angle);
            var a=Pixels(W.PerchPawRenderer.Render(ws,wp));var b=P.PerchPawRenderer.Render(ps,pp).CopyPixels();
            var max=a.Zip(b,(x,y)=>Math.Abs(x-y)).Max();exact(W.PerchPawRenderer.Render(ws,wp),P.PerchPawRenderer.Render(ps,pp),$"paw {closed}/{right}/{offset}/{angle}");
            pawMetrics.Add(new{closed,right,offset,angle,max});
            if(!closed&&right&&angle==0)sheetFrames.Add(($"PAW {offset}",a,P.PerchPawRenderer.Render(ps,pp)));
            for(var y=63;y<100;y++)for(var x=20;x<80;x++)
                Assert.Equal(W.PerchPawRenderer.HitTest(ws,wp,new(x+.2,y+.4)),P.PerchPawRenderer.HitTest(ps,pp,new(x+.2,y+.4)));
        }
        using(var sheet=new SkiaSharp.SKBitmap(420,150*sheetFrames.Count))
        using(var canvas=new SkiaSharp.SKCanvas(sheet))
        using(var text=new SkiaSharp.SKPaint{Color=SkiaSharp.SKColors.White,TextSize=13,IsAntialias=true})
        {
            canvas.Clear(new SkiaSharp.SKColor(32,40,48));var y=0;
            foreach(var item in sheetFrames)
            {
                canvas.DrawText(item.Label+"   Windows | Portable",12,y+18,text);
                using var w=new P.RasterImage(item.Actual.PixelWidth,item.Actual.PixelHeight,item.Expected).ToBitmap();using var p=item.Actual.ToBitmap();
                canvas.DrawBitmap(w,new SkiaSharp.SKRect(25,y+25,145,y+145));canvas.DrawBitmap(p,new SkiaSharp.SKRect(235,y+25,355,y+145));y+=150;
            }
            using var image=SkiaSharp.SKImage.FromBitmap(sheet);using var png=image.Encode(SkiaSharp.SKEncodedImageFormat.Png,100);using var output=File.Create(Path.Combine(AppContext.BaseDirectory,"direct-parity.png"));png.SaveTo(output);
        }
        File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"direct-parity.json"),JsonSerializer.Serialize(new{cheekRasters=24,headFrames=8,springSamples=501,pawStateTicks=480,hitGrid=9216,pawMetrics,huntMetrics},new JsonSerializerOptions{WriteIndented=true}));
    }
    private static byte[] Pixels(BitmapSource image)
    {var result=new byte[image.PixelWidth*image.PixelHeight*4];new FormatConvertedBitmap(image,PixelFormats.Pbgra32,null,0).CopyPixels(result,image.PixelWidth*4,0);return result;}
}

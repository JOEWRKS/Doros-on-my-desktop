using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;
using Dororong.Core.Behavior;

namespace Dororong.App.Tests.Controls;

public class SnackStompRenderTests
{
    [Theory][InlineData(1)][InlineData(1.25)][InlineData(1.5)]
    public void ShoulderIsFilledWithoutNeckToArmpitDivider(double scale)=>CheekProductTests.Sta(()=>{
        var raster=new HighResolutionGrabRaster();
        // This interior patch is below the chin and above the repaired outer
        // shoulder in both lifted/contact poses. The rejected seam crossed it.
        foreach(var age in new[]{.1,.19,.24,.28,.51,.60,1d}){
            var image=raster.Render(0,Matrix.Identity,default,default,default,0,scale,
                HighResolutionGrabRaster.SnackReaction(age,5),bodyScale:scale);
            var pixels=PremultipliedFrame.From(image).Pixels;
            for(var y=(int)Math.Ceiling(154*scale);y<(int)Math.Floor(157*scale);y++)
                for(var x=(int)Math.Ceiling(114*scale);x<(int)Math.Floor(118*scale);x++){
                    var at=(y*image.PixelWidth+x)*4;
                    Assert.True(pixels[at+3]>=250,$"Shoulder hole {age}/{scale}/{x},{y}");
                    Assert.True(pixels[at]>=235&&pixels[at+1]>=235&&pixels[at+2]>=235,
                        $"Unwanted neck-to-armpit divider {age}/{scale}/{x},{y}");
                }
        }
    });
    [Theory][InlineData(1)][InlineData(1.125)][InlineData(1.25)][InlineData(1.375)][InlineData(1.5)]
    public void SecondContinuesSameCadenceWithoutReturningToNeutralBetweenBeats(double scale)=>CheekProductTests.Sta(()=>{
        var raster=new HighResolutionGrabRaster();
        BitmapSource Draw(double age)=>raster.Render(0,Matrix.Identity,new(37.5,28.25),default,default,0,scale,
            HighResolutionGrabRaster.SnackReaction(age,2),bodyScale:scale);
        foreach(var age in new[]{.19,.24,.28})for(var beat=1;beat<4;beat++){
            var a=PremultipliedFrame.From(Draw(age)).Pixels;var b=PremultipliedFrame.From(Draw(age+.32*beat)).Pixels;
            for(var i=0;i<a.Length;i++)Assert.InRange(Math.Abs(a[i]-b[i]),0,1);
        }
        var neutral=PremultipliedFrame.From(Draw(0)).Pixels;
        Assert.Equal(neutral,PremultipliedFrame.From(Draw(1.54)).Pixels);
        for(var frame=11;frame<=78;frame++)Assert.False(neutral.AsSpan().SequenceEqual(PremultipliedFrame.From(Draw(frame/60d)).Pixels));
        if(scale==1.5&&Environment.GetEnvironmentVariable("DORORONG_STOMP_PROOF") is {} output){
            Directory.CreateDirectory(output);var gif=new GifBitmapEncoder();
            var sheet=new DrawingVisual();using(var dc=sheet.RenderOpen()){
                dc.DrawRectangle(Brushes.LightBlue,null,new Rect(0,0,1152,576));
                for(var cell=0;cell<8;cell++){
                    var age=.60+.05*cell;var x=cell%4*288;var y=cell/4*288;
                    dc.DrawImage(new CroppedBitmap(Draw(age),new Int32Rect(120,120,144,144)),new Rect(x,y,288,288));
                    dc.DrawText(new FormattedText($"{age:F2}s",System.Globalization.CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight,new Typeface("Arial"),16,Brushes.Black,1),new Point(x+8,y+8));
                }
            }
            var sheetImage=new RenderTargetBitmap(1152,576,96,96,PixelFormats.Pbgra32);sheetImage.Render(sheet);
            var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(sheetImage));
            using(var sheetFile=File.Create(Path.Combine(output,"second-middle-no-reset.png")))png.Save(sheetFile);
            for(var frame=0;frame<=52;frame++){
                var image=Draw(frame*.03);var visual=new DrawingVisual();using(var dc=visual.RenderOpen()){
                    dc.DrawRectangle(Brushes.LightBlue,null,new Rect(0,0,288,288));
                    dc.DrawImage(new CroppedBitmap(image,new Int32Rect(120,120,144,144)),new Rect(0,0,288,288));
                }
                var target=new RenderTargetBitmap(288,288,96,96,PixelFormats.Pbgra32);target.Render(visual);
                var metadata=new BitmapMetadata("gif");metadata.SetQuery("/grctlext/Delay",(ushort)3);
                gif.Frames.Add(BitmapFrame.Create(target,null,metadata,null));
            }
            using var file=File.Create(Path.Combine(output,"second-continuous.gif"));gif.Save(file);
        }
    });
    [Theory][InlineData(1)][InlineData(1.125)][InlineData(1.25)][InlineData(1.375)][InlineData(1.5)]
    public void EscalatedHitsReuseTheSameCleanPawPixels(double scale)=>CheekProductTests.Sta(()=>{
        var raster=new HighResolutionGrabRaster();
        byte[] Draw(double age,int streak)=>PremultipliedFrame.From(raster.Render(0,Matrix.Identity,new(37.5,28.25),default,default,0,scale,
            HighResolutionGrabRaster.SnackReaction(age,streak),bodyScale:scale)).Pixels;
        foreach(var age in new[]{.19,.24,.28}){
            var expected=Draw(age,3);
            for(var streak=3;streak<=5;streak++)for(var hit=0;hit<streak;hit++)
                Assert.True(expected.AsSpan().SequenceEqual(Draw(age+.32*hit,streak)),$"Paw changed at size{scale}, streak{streak}, hit{hit}, age{age}");
        }
    });
    [Theory][InlineData(1)][InlineData(1.125)][InlineData(1.25)][InlineData(1.375)][InlineData(1.5)]
    public void EveryFrameKeepsInteriorOpaqueAndOtherFeetUnchanged(double scale)=>CheekProductTests.Sta(()=>{
        var raster=new HighResolutionGrabRaster();var extent=(int)Math.Ceiling(256*scale);
        byte[] Draw(double age)=>PremultipliedFrame.From(raster.Render(0,Matrix.Identity,new(37.5,28.25),default,default,0,scale,
            HighResolutionGrabRaster.SnackReaction(age,3),bodyScale:scale)).Pixels;
        var baseline=Draw(0);
        var last=(int)Math.Ceiling(SnackAngerMotion.Duration(3)*60);
        for(var frame=0;frame<=last;frame++){
            var pixels=Draw(frame/60d);
            if(frame==0||frame==last)Assert.Equal(baseline,pixels);
            for(var y=(int)Math.Ceiling(157*scale);y<extent;y++)for(var x=(int)Math.Ceiling(140*scale);x<extent;x++){
                var at=(y*extent+x)*4;Assert.True(pixels.AsSpan(at,4).SequenceEqual(baseline.AsSpan(at,4)), $"Rear foot frame{frame} size{scale}");
            }
            // Y70..72 is below the accepted bobbing chin (Y68 crosses its
            // legitimate outline at impact). Test torso, not facial ink.
            for(var y=(int)Math.Ceiling(150*scale);y<(int)(152*scale);y++)for(var x=(int)Math.Ceiling(112*scale);x<(int)(115*scale);x++){
                var at=(y*extent+x)*4;
                Assert.True(pixels[at+3]>=250,$"Collar hole frame{frame} size{scale} at{x},{y}: {pixels[at+3]}");
                Assert.True(pixels[at]>=220&&pixels[at+1]>=220&&pixels[at+2]>=220,$"Interior ink frame{frame} size{scale} at{x},{y}");
            }
        }
    });

    [Fact] public void ContactContourHasNoCutMaskStep()=>CheekProductTests.Sta(()=>{
        var raster=new HighResolutionGrabRaster();
        foreach(var age in new[]{.28,.60}){
            var p=PremultipliedFrame.From(raster.Render(0,Matrix.Identity,new(37.5,28.25),default,default,0,4,
                HighResolutionGrabRaster.SnackReaction(age,3),bodyScale:1.5)).Pixels;
            double? previous=null;
            for(var x=25d;x<=34;x+=.5){var edge=86d;
                for(var y=68d;y<86;y+=.25)if(p[((int)((y+80)*4)*1024+(int)((x+80)*4))*4+3]<128){edge=y;break;}
                if(previous is {} last)Assert.True(Math.Abs(edge-last)<=1,$"Contour jump {x}: {last}->{edge}");previous=edge;
            }
        }
    });
    [Fact] public void RootJoinDoesNotIntroduceContourStepIntoOriginalValley()=>CheekProductTests.Sta(()=>{
        var raster=new HighResolutionGrabRaster();
        foreach(var age in new[]{.28,.60}){
            var pixels=PremultipliedFrame.From(raster.Render(0,Matrix.Identity,new(37.5,28.25),default,default,0,4,
                HighResolutionGrabRaster.SnackReaction(age,3),bodyScale:1.5)).Pixels;
            double? previous=null,previousSlope=null;
            // The restored large arm joins across the same region as the
            // accepted-size candidate. Its contour must not gain a second kink.
            for(var x=27d;x<=37;x+=.5){
                var edge=90d;for(var y=68d;y<90;y+=.25)
                    if(pixels[((int)((y+80)*4)*1024+(int)((x+80)*4))*4+3]<128){edge=y;break;}
                if(previous is {} last){
                    var slope=edge-last;
                    // Quarter-DIP sampling permits one quantization step.
                    if(previousSlope is {} lastSlope)Assert.InRange(Math.Abs(slope-lastSlope),0,.25);
                    previousSlope=slope;
                }
                previous=edge;
            }
        }
    });
    [Fact] public void InspectEverySixtiethSecondOfShoulder()=>CheekProductTests.Sta(()=>{
        var output=Environment.GetEnvironmentVariable("DORORONG_STOMP_PROOF");
        var raster=new HighResolutionGrabRaster();
        if(output is not null){
            Directory.CreateDirectory(output);
            var body=(float[])typeof(HighResolutionGrabRaster).GetMethod("Body",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!.Invoke(raster,[0d])!;
            var edges=new List<string>();
            for(var x=18d;x<=44;x+=.5){var edge=95d;
                for(var y=70d;y<95;y+=.1){var at=((int)((y+32)*225/96)*375+(int)((x+32)*225/96))*4;if(body[at+3]<128){edge=y;break;}}
                edges.Add($"{x:F2},{edge:F2}");}
            File.WriteAllLines(Path.Combine(output,"source-body-boundary.csv"),edges);
            var p=PremultipliedFrame.From(raster.Render(0,Matrix.Identity,new(37.5,28.25),default,default,0,4,
                HighResolutionGrabRaster.SnackReaction(.28,3),bodyScale:1.5)).Pixels;
            var rows=new List<string>();
            for(var x=24d;x<=40;x+=.5){var edge=86d;
                for(var y=68d;y<86;y+=.25)if(p[((int)((y+80)*4)*1024+(int)((x+80)*4))*4+3]<128){edge=y;break;}
                rows.Add($"{x:F2},{edge:F2}");}
            File.WriteAllLines(Path.Combine(output,"contact-boundary.csv"),rows);
        }
        var last=(int)Math.Ceiling(SnackAngerMotion.Duration(3)*60);
        for(var page=0;page<(int)Math.Ceiling((last+1)/8d);page++){
            var sheet=new DrawingVisual();using(var dc=sheet.RenderOpen()){
                dc.DrawRectangle(Brushes.LightGray,null,new Rect(0,0,1280,800));
                for(var cell=0;cell<8;cell++){
                    var frame=page*8+cell;if(frame>last)break;
                    var image=raster.Render(0,Matrix.Identity,new(37.5,28.25),default,default,0,1.5,
                        HighResolutionGrabRaster.SnackReaction(frame/60d,3),bodyScale:1.5);
                    var x=cell%4*320;var y=cell/4*400;
                    dc.DrawImage(new CroppedBitmap(image,new Int32Rect(120,120,144,144)),new Rect(x,y,192,192));
                    dc.DrawImage(new CroppedBitmap(image,new Int32Rect(132,210,48,36)),new Rect(x,y+192,256,192));
                    dc.DrawText(new FormattedText($"{frame:00} / {frame/60d:F3}s",System.Globalization.CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight,new Typeface("Arial"),16,Brushes.Black,1),new Point(x+192,y+8));
                }
            }
            if(output is not null){Directory.CreateDirectory(output);
                var target=new RenderTargetBitmap(1280,800,96,96,PixelFormats.Pbgra32);target.Render(sheet);
                var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(target));
                using var file=File.Create(Path.Combine(output,$"frames-{page:00}.png"));png.Save(file);
            }
        }
    });
    [Theory][InlineData(false)][InlineData(true)]
    public void InterruptedLiftRecoversWithoutLeavingPawRaised(bool furious)=>CheekProductTests.Sta(()=>{
        var presenter=new DororongPresenter();
        var method=typeof(DororongPresenter).GetMethod("RecoverCapturedPose",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!;
        var lift=furious?4.8:2.4;var reach=furious?16:4;
        var captured=HighResolutionFacePose.Neutral with{Furious=furious,StompLift=lift,StompReach=reach,StompCrouch=.34};
        foreach(var u in new[]{0d,.25,.5,.75,1}){
            var recovered=(HighResolutionFacePose)method.Invoke(presenter,[captured,u])!;
            Assert.Equal(lift*(1-u),recovered.StompLift);
            Assert.Equal(reach*(1-u),recovered.StompReach);
            Assert.Equal(.34*(1-u),recovered.StompCrouch);
        }
    });

    [Theory][InlineData(false)][InlineData(true)]
    public void FacingFlipKeepsUnaffectedPixelsAndRestoresExactContact(bool flipped)=>CheekProductTests.Sta(()=>{
        var raster=new HighResolutionGrabRaster();var inverse=Matrix.Identity;
        if(flipped)inverse.ScaleAt(-1,1,48,48);
        byte[] Draw(double age)=>PremultipliedFrame.From(raster.Render(0,inverse,new(37.5,28.25),default,default,0,1.5,
            HighResolutionGrabRaster.SnackReaction(age,3),bodyScale:1.5)).Pixels;
        var reference=Draw(0);var lifted=Draw(.28);var changed=0;
        for(var y=0;y<384;y++)for(var x=0;x<384;x++){
            var at=(y*384+x)*4;if(reference.AsSpan(at,4).SequenceEqual(lifted.AsSpan(at,4)))continue;
            changed++;var p=inverse.Transform(new Point(x/1.5-80,y/1.5-80));
            Assert.False(p.X>=60&&p.Y>=77,$"Rear feet moved: {p.X},{p.Y}");
        }
        Assert.True(changed>10);Assert.Equal(reference,Draw(0));
    });

    [Fact] public void ForwardReachBowsOnTwoImpactsAndReturnsExactly()=>CheekProductTests.Sta(()=>{
        var sheet=new DrawingVisual();using var draw=sheet.RenderOpen();
        draw.DrawRectangle(Brushes.White,null,new Rect(0,0,1440,800));
        var sizes=new[]{1d,1.125,1.25,1.375,1.5};
        var end=SnackAngerMotion.Duration(3);var ages=new[]{0d,.12,.19,.28,.51,.60,.83,.92,end};
        for(var row=0;row<sizes.Length;row++){
            var scale=sizes[row];var raster=new HighResolutionGrabRaster();
            BitmapSource Frame(double age)=>raster.Render(0,Matrix.Identity,new(37.5,28.25),default,default,0,scale,
                HighResolutionGrabRaster.SnackReaction(age,3),bodyScale:scale);
            var baseline=PremultipliedFrame.From(Frame(0)).Pixels;
            var extent=(int)Math.Ceiling(256*scale);
            for(var col=0;col<ages.Length;col++){
                var age=ages[col];var image=Frame(age);var pixels=PremultipliedFrame.From(image).Pixels;var changed=0;
                for(var y=0;y<extent;y++)for(var x=0;x<extent;x++){
                    var at=(y*extent+x)*4;
                    if(pixels.AsSpan(at,4).SequenceEqual(baseline.AsSpan(at,4)))continue;
                    changed++;var lx=x/scale-80;var ly=y/scale-80;
                    Assert.False(lx>=60&&ly>=77,$"Rear foot moved: {lx},{ly}");
                }
                if(age==0||age>=end)Assert.Equal(baseline,pixels);
                else Assert.True(changed>10);
                var crop=new CroppedBitmap(image,new Int32Rect((int)(80*scale),(int)(80*scale),(int)(96*scale),(int)(96*scale)));
                draw.DrawImage(crop,new Rect(col*160,row*160,144,144));
            }
            if(row==4&&Environment.GetEnvironmentVariable("DORORONG_STOMP_PROOF") is {} folder){
                Directory.CreateDirectory(folder);var gif=new GifBitmapEncoder();
                var comparison=new DrawingVisual();using(var dc=comparison.RenderOpen()){
                    dc.DrawRectangle(Brushes.White,null,new Rect(0,0,1152,576));
                    foreach(var streak in new[]{2,3}){
                        var contact=raster.Render(0,Matrix.Identity,new(37.5,28.25),default,default,0,scale,
                            HighResolutionGrabRaster.SnackReaction(.28,streak),bodyScale:scale);
                        dc.DrawImage(new CroppedBitmap(contact,new Int32Rect(120,120,144,144)),new Rect((streak-2)*576,0,576,576));
                    }
                }
                var comparisonImage=new RenderTargetBitmap(1152,576,96,96,PixelFormats.Pbgra32);comparisonImage.Render(comparison);
                var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(comparisonImage));
                using(var comparisonFile=File.Create(Path.Combine(folder,"second-third-contact.png")))png.Save(comparisonFile);
                for(var i=0;i<=Math.Ceiling(end/.025);i++){
                    var image=Frame(i*.025);
                    var crop=new CroppedBitmap(image,new Int32Rect(120,120,144,144));
                    var visual=new DrawingVisual();using(var dc=visual.RenderOpen()){
                        dc.DrawRectangle(Brushes.White,null,new Rect(0,0,288,288));
                        dc.DrawImage(crop,new Rect(0,0,288,288));
                    }
                    var frame=new RenderTargetBitmap(288,288,96,96,PixelFormats.Pbgra32);frame.Render(visual);
                    var metadata=new BitmapMetadata("gif");metadata.SetQuery("/grctlext/Delay",(ushort)(i%2==0?2:3));
                    gif.Frames.Add(BitmapFrame.Create(frame,null,metadata,null));
                }
                using var file=File.Create(Path.Combine(folder,"stomp.gif"));gif.Save(file);
            }
        }
        draw.Close();
        if(Environment.GetEnvironmentVariable("DORORONG_STOMP_PROOF") is {} output){
            Directory.CreateDirectory(output);var image=new RenderTargetBitmap(1440,800,96,96,PixelFormats.Pbgra32);image.Render(sheet);
            var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(image));
            using var file=File.Create(Path.Combine(output,"stomp-five-sizes.png"));encoder.Save(file);
        }
    });

    [Theory][InlineData(2)]
    public void ForwardMappingIsInvertibleAndOtherSolesStayPlanted(int streak)
    {
        foreach(var originalAge in new[]{.12,.19,.24,.28,.36,.51,.60,.76}){
            var age=originalAge;
            var warp=new HighResolutionGrabRaster.StompWarp(HighResolutionGrabRaster.SnackReaction(age,streak));
            foreach(var anchor in new[]{streak==3?new Point(43,85):new Point(23,79),new Point(66,82),new Point(54.5,78.5)})
                Assert.Equal(anchor,warp.Map(anchor));
            var tip=streak==3?new Point(23,79):new Point(43,85);
            var toe=warp.Map(tip);Assert.True(toe.X<tip.X);
            if(originalAge==.28||originalAge==.60){Assert.Equal(tip.Y,toe.Y,8);if(streak==3)Assert.True(toe.X<=7.01);}
            for(var y=45;y<=90;y+=2)for(var x=12;x<=70;x+=2){
                var mapped=warp.Map(new(x,y));var source=warp.Source(mapped.X,mapped.Y);
                var error=Math.Abs(source.X-x)+Math.Abs(source.Y-y);
                Assert.True(error<=1e-5,$"Inverse error={error} age={age} source={x},{y} mapped={mapped} recovered={source}");
            }
        }
    }

    [Fact] public void ThirdArmRotatesRigidlyAroundFixedShoulderDuringBothHits()
    {
        var shoulder=new Point(29,74);var tip=new Point(23,79);
        double? length=null;Point? pivot=null;
        foreach(var age in new[]{.19,.23,.28,.32,.36,.45,.51,.55,.60,.64}){
            var warp=new HighResolutionGrabRaster.StompWarp(HighResolutionGrabRaster.SnackReaction(age,3));
            var root=warp.ArmPoint(shoulder);var toe=warp.ArmPoint(tip);
            pivot??=root;Assert.Equal(pivot.Value.X,root.X,9);Assert.Equal(pivot.Value.Y,root.Y,9);
            var distance=Math.Sqrt(Math.Pow(toe.X-root.X,2)+Math.Pow(toe.Y-root.Y,2));
            length??=distance;Assert.Equal(length.Value,distance,9);
            foreach(var p in new[]{shoulder,tip,new Point(17,74),new Point(28,76)}){
                var restored=warp.ArmSource(warp.ArmPoint(p));
                Assert.Equal(p.X,restored.X,9);Assert.Equal(p.Y,restored.Y,9);
            }
        }
        var high=new HighResolutionGrabRaster.StompWarp(HighResolutionGrabRaster.SnackReaction(.19,3)).ArmPoint(tip);
        var contact=new HighResolutionGrabRaster.StompWarp(HighResolutionGrabRaster.SnackReaction(.28,3)).ArmPoint(tip);
        Assert.True(contact.Y-high.Y>9);Assert.InRange(contact.Y,78.5,79.5);
    }

    [Fact] public void ThirdArmKeepsAcceptedLargeSizeAndTrajectoryIncludingPreparationAndReturn()
    {
        // User accepts the larger arm, not the previous original-length fix.
        // Pin its geometry independently from the reconstructed outline width.
        for(var frame=0;frame<=Math.Ceiling(SnackAngerMotion.Duration(3)*60);frame++){
            var face=HighResolutionGrabRaster.SnackReaction(frame/60d,3);
            var warp=new HighResolutionGrabRaster.StompWarp(face);
            var deploy=face.StompReach/16;
            var angle=(10*deploy+40*face.StompLift/4.8)*Math.PI/180;
            foreach(var point in new[]{new Point(16,70),new Point(18,77.7),new Point(23,82.4),new Point(28,76.4),new Point(29,74)}){
                var dx=point.X-29;var dy=point.Y-74;
                var along=(-6*dx+5*dy)/Math.Sqrt(61);
                dx+=1.5*deploy*along*(-6/Math.Sqrt(61));dy+=1.5*deploy*along*(5/Math.Sqrt(61));
                var expected=new Point(29+deploy+dx*Math.Cos(angle)-dy*Math.Sin(angle),74-4.7*deploy+dx*Math.Sin(angle)+dy*Math.Cos(angle));
                var actual=warp.ArmPoint(point);
                Assert.Equal(expected.X,actual.X,9);Assert.Equal(expected.Y,actual.Y,9);
            }
        }
    }

    [Theory][InlineData(1)][InlineData(1.125)][InlineData(1.25)][InlineData(1.375)][InlineData(1.5)]
    public void LargeForepawInkDoesNotStretchWithItsSilhouette(double scale)=>CheekProductTests.Sta(()=>{
        var raster=new HighResolutionGrabRaster();
        foreach(var age in new[]{.19,.24,.28,.51,.55,.60}){
            var face=HighResolutionGrabRaster.SnackReaction(age,3);var warp=new HighResolutionGrabRaster.StompWarp(face);
            var pixels=PremultipliedFrame.From(raster.Render(0,Matrix.Identity,new(37.5,28.25),default,default,0,4,face,bodyScale:scale)).Pixels;
            // Sample across the leading forepaw edge along its transformed
            // normal. This part formerly kept its axially stretched source ink.
            var edge=warp.ArmPoint(new(19,79.4));var zero=warp.ArmPoint(new(0,0));
            var u=warp.ArmPoint(new(1,0));var v=warp.ArmPoint(new(0,1));
            var nx=(v.Y-zero.Y)*(-.9)-(u.Y-zero.Y)*.5;
            var ny=-(v.X-zero.X)*(-.9)+(u.X-zero.X)*.5;
            var length=Math.Sqrt(nx*nx+ny*ny);nx/=length;ny/=length;
            var inkIntegral=0d;
            for(var d=-5d;d<=5;d+=.125){
                var x=(int)Math.Floor((edge.X+d*nx+80)*4);var y=(int)Math.Floor((edge.Y+d*ny+80)*4);
                var at=(y*1024+x)*4;var alpha=pixels[at+3]/255d;
                if(alpha==0)continue;
                var ink=Math.Clamp((1-pixels[at]/(255*alpha))/(1-56d/255),0,1);
                inkIntegral+=ink*alpha*.125;
            }
            Assert.InRange(inkIntegral*scale,.7,1.85);
        }
    });
}

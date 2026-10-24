using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;
using Dororong.Core.Behavior;

namespace Dororong.App.Tests.Controls;

public class SnackAngerRenderTests
{
    [Fact] public void AuthoredFarBrowIsNotDoubledAndBothBridgeFoldsExist()=>CheekProductTests.Sta(()=>{
        var r=new HighResolutionGrabRaster();
        var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
        var source=(float[])typeof(HighResolutionGrabRaster).GetField("head",flags)!.GetValue(r)!;
        foreach(var furious in new[]{false,true}){
            r.Render(0,Matrix.Identity,new(37.5,28.25),default,default,0,1,
                HighResolutionFacePose.Neutral with {Angry=true,Furious=furious},bodyScale:1);
            var expression=(float[])typeof(HighResolutionGrabRaster).GetField("angryHead",flags)!.GetValue(r)!;
            for(var y=114;y<=122;y++)for(var x=81;x<=105;x++)for(var c=0;c<4;c++)
                Assert.Equal(source[(y*225+x)*4+c],expression[(y*225+x)*4+c]);
            if(furious){
                // Curved muzzle folds must be continuous rather than isolated
                // dots. They remain separate from each other and the mouth.
                foreach(var box in new[]{(L:64,R:67),(L:73,R:77)}){
                    var ink=new HashSet<(int X,int Y)>();
                    for(var y=133;y<=136;y++)for(var x=box.L;x<=box.R;x++)
                        if(expression[(y*225+x)*4]<200)ink.Add((x,y));
                    Assert.True(ink.Count>=4,$"Missing curved muzzle fold: {box}");
                    var visited=new HashSet<(int X,int Y)>();
                    var pending=new Queue<(int X,int Y)>();pending.Enqueue(ink.First());
                    while(pending.TryDequeue(out var p)){
                        if(!visited.Add(p))continue;
                        for(var dy=-1;dy<=1;dy++)for(var dx=-1;dx<=1;dx++){
                            var next=(p.X+dx,p.Y+dy);
                            if(ink.Contains(next)&&!visited.Contains(next))pending.Enqueue(next);
                        }
                    }
                    Assert.Equal(ink.Count,visited.Count);
                    Assert.True(ink.Max(p=>p.Y)-ink.Min(p=>p.Y)>=2);
                }
                for(var y=133;y<=136;y++)for(var x=69;x<=71;x++)for(var c=0;c<3;c++)
                    Assert.True(expression[(y*225+x)*4+c]>220,$"Muzzle folds merged: {x},{y}");
                // Test below the two folds, not beneath the middle channel:
                // the unchanged omega's central peak reaches that middle row.
                foreach(var x in new[]{65,66,67,74,75})for(var c=0;c<3;c++)
                    Assert.True(expression[(137*225+x)*4+c]>220,$"Muzzle fold touches mouth: {x}");
                // User's red circle: the old rising connector must be gone,
                // not merely covered by a second glabellar line above it.
                for(var y=132;y<=134;y++)for(var x=60;x<=63;x++)for(var c=0;c<3;c++)
                    Assert.True(expression[(y*225+x)*4+c]>220,$"Residual ink in deleted connector: {x},{y}");
                // Each newly authored short fold must add ink in its own skin
                // region; checking only a Furious flag did not catch omission.
                foreach(var box in new[]{(L:64,R:67,T:124,B:128),(L:74,R:79,T:124,B:130)}){
                    var added=0;
                    for(var y=box.T;y<=box.B;y++)for(var x=box.L;x<=box.R;x++){
                        var at=(y*225+x)*4;
                        if(source[at]-expression[at]>25&&expression[at]<160)added++;
                    }
                    Assert.True(added>=2,$"Missing bridge fold: {box}, ink={added}");
                }
            }
        }
    });

    [Fact] public void NewExpressionIsLocalAndRestoresExactSourceAtFiveSizes()=>CheekProductTests.Sta(()=>{
        var sheet=new DrawingVisual();using var dc=sheet.RenderOpen();
        dc.DrawRectangle(Brushes.White,null,new Rect(0,0,1200,1000));
        var sizes=new[]{1d,1.125,1.25,1.375,1.5};
        var irisErrors=new List<string>();
        for(var row=0;row<sizes.Length;row++){
            var size=sizes[row];var r=new HighResolutionGrabRaster();
            BitmapSource Frame(bool angry,bool furious,double age){
                var matrix=Matrix.Identity;if(angry)matrix.RotateAt(SnackAngerMotion.Roll(age)*180/Math.PI,37.5,28.25);
                return r.Render(0,Matrix.Identity,new(37.5,28.25),default,default,0,size,
                    HighResolutionFacePose.Neutral with {Angry=angry,Furious=furious,Head=matrix},bodyScale:size);
            }
            var neutral=PremultipliedFrame.From(Frame(false,false,0)).Pixels;
            byte[]? firstStage=null;
            foreach(var furious in new[]{false,true,false}){
            var angry=PremultipliedFrame.From(Frame(true,furious,0)).Pixels;
            if(!furious){firstStage??=angry;Assert.Equal(firstStage,angry);}
            var extent=(int)Math.Ceiling(256*size);var changed=0;
            for(var y=0;y<extent;y++)for(var x=0;x<extent;x++){
                var at=(y*extent+x)*4;Assert.Equal(neutral[at+3],angry[at+3]);
                if(neutral.AsSpan(at,4).SequenceEqual(angry.AsSpan(at,4)))continue;
                changed++;var lx=x/size-80;var ly=y/size-80;
                Assert.True(lx>=15&&lx<=53&&ly>=29&&ly<=67,$"Changed outside face: {lx},{ly}");
                // Third-stage user reference deliberately changes the lower
                // lids. Preserve the upper iris colour/highlight, not that band.
                if(lx>=21&&lx<=24&&ly>=51.5&&ly<=52)irisErrors.Add($"Upper near iris changed {lx},{ly}, furious={furious}, size={size}");
                if(lx>=37&&lx<=41&&ly>=54&&ly<=57)irisErrors.Add($"Upper far iris changed {lx},{ly}, furious={furious}, size={size}");
                Assert.False(lx>=42&&ly>=59,"Far eye / hair seam changed");
                if(!furious){
                    Assert.False(lx>=20&&lx<=26&&ly>=52&&ly<=55,"First-stage near iris changed");
                    Assert.False(lx>=36&&lx<=42&&ly>=54&&ly<=59,"First-stage far iris changed");
                }
            }
            Assert.True(changed>10);
            }
            var ages=new[]{0d,.1,.25,.45,.7,.9};
            for(var col=0;col<ages.Length;col++){
                var active=col!=0&&col!=5;var bitmap=Frame(active,col>=3,ages[col]);
                var crop=new CroppedBitmap(bitmap,new Int32Rect((int)(80*size),(int)(80*size),(int)(96*size),(int)(96*size)));
                dc.DrawImage(crop,new Rect(col*200,row*200,192,192));
            }
            Assert.Equal(neutral,PremultipliedFrame.From(Frame(false,false,0)).Pixels);
            if(row==4&&Environment.GetEnvironmentVariable("DORORONG_SNACK_PROOF") is {} detailPath){
                Directory.CreateDirectory(detailPath);
                SaveDetail(false,"neutral-face.png");
                SaveDetail(true,"furious-face.png");
                void SaveDetail(bool angry,string name){
                var face=new CroppedBitmap(Frame(angry,angry,0),new Int32Rect((int)(95*size),(int)(123*size),(int)(36*size),(int)(30*size)));
                var view=new DrawingVisual();using(var draw=view.RenderOpen())draw.DrawImage(face,new Rect(0,0,432,360));
                var detail=new RenderTargetBitmap(432,360,96,96,PixelFormats.Pbgra32);detail.Render(view);
                var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(detail));using var file=File.Create(Path.Combine(detailPath,name));png.Save(file);
                }
            }
        }
        dc.Close();
        if(Environment.GetEnvironmentVariable("DORORONG_SNACK_PROOF") is {} path){
            Directory.CreateDirectory(path);var image=new RenderTargetBitmap(1200,1000,96,96,PixelFormats.Pbgra32);image.Render(sheet);
            var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(image));using var file=File.Create(Path.Combine(path,"anger-five-sizes.png"));png.Save(file);
            var preview=new DrawingVisual();using(var draw=preview.RenderOpen()){
                var columns=new[]{0,1,3};for(var i=0;i<columns.Length;i++)
                    draw.DrawImage(new CroppedBitmap(image,new Int32Rect(columns[i]*200,400,200,200)),new Rect(i*200,0,200,200));
            }
            var small=new RenderTargetBitmap(600,200,96,96,PixelFormats.Pbgra32);small.Render(preview);
            var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(small));using var strip=File.Create(Path.Combine(path,"expression-preview.png"));encoder.Save(strip);
        }
        Assert.True(irisErrors.Count==0,string.Join("\n",irisErrors.Take(15)));
    });
}

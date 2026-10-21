// r104 sampler unchanged except fixed WPF paw texture import.
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Dororong.App.Controls;

internal sealed partial class HighResolutionGrabRaster
{
    // Source-resolution cutouts. The separate immutable head covers these
    // layers afterwards. Rotation is sampled only at destination resolution.
    private sealed class FlutterLayers
    {
        private sealed record Arm(Point Root, Point[] Mask);
        private static readonly Arm[] hanging=
        // Registered to the accepted r33 body, not the old image08 coordinates.
        [new(new(27,67),[new(20,64),new(35,64),new(36,71),new(35,77),new(32,83),new(29,85),new(25,84),new(21,80),new(20,73)]),
         new(new(43,69),[new(35,67),new(51,67),new(51,73),new(49,80),new(46,84),new(42,85),new(37,83),new(35,77)])];
        private static readonly Arm[] canonical=
        [new(new(22,71),[new(16,70),new(29,70),new(29,79),new(27,83),new(22,84),new(18,81),new(16,76)]),
         new(new(39,77),[new(32,76),new(50,76),new(50,85),new(47,89),new(41,90),new(35,86),new(32,81)])];
        private readonly float[] plate;
        private readonly (float[] Pixels,Point Root,double Cos,double Sin)[] layers;
        private float[]? scaledPlate;
        private float[][]? scaledArms;
        internal void SetScale(double scale){
            scaledPlate=HighResolutionBodyInk.AtScale(plate,375,scale);
            scaledArms=layers.Select(a=>HighResolutionBodyInk.AtScale(a.Pixels,375,scale)).ToArray();
        }
        internal FlutterLayers(float[] source,bool held)
        {
            plate=(float[])source.Clone();var arms=held?hanging:canonical;
            layers=new (float[],Point,double,double)[arms.Length];
            if(held){
                // Partition along the accepted r33 forepaw curves, not boxes
                // containing neighbouring torso ink and white fringe.
                for(var y=0;y<375;y++)for(var x=0;x<375;x++){
                    var lx=(x+.5)*96/225-32;var ly=(y+.5)*96/225-32;
                    if(lx>=52||ly<55||ly>=85)continue;
                    var at=(y*375+x)*4;var donor=((int)((85+32)*225/96)*375+x)*4;
                    Array.Copy(source,donor,plate,at,4);
                }
                layers[0]=(PawTexture(false),hanging[0].Root,1,0);
                layers[1]=(PawTexture(true),hanging[1].Root,1,0);
                return;
            }
            for(var i=0;i<arms.Length;i++){
                var arm=arms[i];var pixels=new float[source.Length];
                for(var y=0;y<375;y++)for(var x=0;x<375;x++){
                    var lx=(x+.5)*96/225-32;var ly=(y+.5)*96/225-32;
                    if(!Inside(arm.Mask,lx,ly))continue;
                    var at=(y*375+x)*4;Array.Copy(source,at,pixels,at,4);
                    if(ly>arm.Root.Y+2)Array.Clear(plate,at,4);
                }
                layers[i]=(pixels,arm.Root,1,0);
            }
        }
        private static float[] PawTexture(bool far)=>Dororong.Raster.FixedPaws.Load(far);
        internal void SetPhase(double phase){
            for(var i=0;i<layers.Length;i++){
                var angle=(52+13*Math.Sin(phase*Math.PI*2-i*Math.PI))*Math.PI/180;
                layers[i]=(layers[i].Pixels,layers[i].Root,Math.Cos(angle),Math.Sin(angle));
            }
        }
        internal void SampleAt(double x,double y,Span<double> result,Span<double> temporary)
        {
            Sample(scaledPlate??plate,375,(x+32)*225/96-.5,(y+32)*225/96-.5,result);
            for(var i=0;i<layers.Length;i++){
                var layer=layers[i];
                var dx=x-layer.Root.X;var dy=y-layer.Root.Y;
                var sx=layer.Root.X+layer.Cos*dx+layer.Sin*dy;
                var sy=layer.Root.Y-layer.Sin*dx+layer.Cos*dy;
                Sample(scaledArms?[i]??layer.Pixels,375,(sx+32)*225/96-.5,(sy+32)*225/96-.5,temporary);
                var remaining=1-temporary[3]/255;
                for(var c=0;c<4;c++)result[c]=temporary[c]+result[c]*remaining;
            }
        }
        private static bool Inside(Point[] polygon,double x,double y){
            var inside=false;
            for(int i=0,j=polygon.Length-1;i<polygon.Length;j=i++){
                var a=polygon[i];var b=polygon[j];
                if((a.Y>y)!=(b.Y>y)&&x<(b.X-a.X)*(y-a.Y)/(b.Y-a.Y)+a.X)inside=!inside;
            }
            return inside;
        }
    }
}

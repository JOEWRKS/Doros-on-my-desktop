// Mac-only r104 sampler copy. Arithmetic is identical to the Windows source;
// only retained-region clearing and destination-loop clipping are added here.
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.Core.Geometry;

namespace Dororong.App.Controls;

// Approved immutable r43/r49 inputs, sampled once at the destination resolution.
// This bank is opt-in until the other high-resolution interactions are migrated.
internal sealed partial class HighResolutionGrabRaster
{
    internal (double Preparation, double Sampling, double Bitmap) LastCost { get; private set; }
    private readonly byte[] bank;
    private readonly (int Offset, int Length)[] entries;
    private readonly Dictionary<int,float[]> cache = new();
    private readonly Queue<int> cacheOrder = new();
    private readonly Dictionary<double,float[]> spreadCache=new();
    private readonly float[] head, nativeBody;
    private HighResolutionHuntBodies? huntBodies;
    private HighResolutionLocomotion? locomotion;
    private HighResolutionHuntNeck.Buffer? neckBuffer;
    private HighResolutionSnackEffects.Buffer? snackBuffer;
    private FlutterLayers? flutterLayers;
    private int flutterKey=-1;
    // BitmapSource.Create owns its copy. The UI-thread raster retains only
    // this scratch buffer, not a fresh large managed array for every draw and
    // every sparse sole query. Clear the whole buffer before changing bounds.
    private byte[] outputBuffer=[];
    private float[]? perchPawBuffer;
    private float[]? closedHead;
    internal int Steps { get; }
    internal PointD Center { get; }
    internal Point MotionOffset(int frame)=>(locomotion??=new()).Head(frame);
    internal HighResolutionGrabRaster()
    {
        using var stream=typeof(HighResolutionGrabRaster).Assembly.GetManifestResourceStream("Dororong.App.Assets.high-resolution-grab.bank")
            ?? throw new InvalidOperationException("High-resolution grab bank is missing.");
        using var memory=new MemoryStream();stream.CopyTo(memory);bank=memory.ToArray();
        using var reader=new BinaryReader(new MemoryStream(bank));
        if(new string(reader.ReadChars(4))!="DHR1")throw new InvalidDataException("Grab bank version.");
        Steps=reader.ReadInt32();if(Steps!=224)throw new InvalidDataException("Grab bank count.");
        entries=new (int,int)[Steps+3];
        for(var i=0;i<entries.Length;i++){var n=reader.ReadInt32();var at=checked((int)reader.BaseStream.Position);if(n<=0||at+n>bank.Length)throw new InvalidDataException();entries[i]=(at,n);reader.BaseStream.Position+=n;}
        head=Read(0,225);nativeBody=Read(1,160);
        double x=0,y=0,count=0;for(var i=0;i<225*225;i++)if(head[i*4+3]>=128&&head[i*4+2]>head[i*4]+12&&head[i*4]>head[i*4+1]+12){x+=i%225;y+=i/225;count++;}
        Center=new(x/count*96/225,y/count*96/225);
    }
    private float[] Read(int index,int size,float[]? reuse=null)
    {
        var e=entries[index];using var gz=new GZipStream(new MemoryStream(bank,e.Offset,e.Length),CompressionMode.Decompress);
        var values=reuse??new float[size*size*4];HighResolutionBodyInk.Invalidate(values);gz.ReadExactly(MemoryMarshal.AsBytes(values.AsSpan()));if(gz.ReadByte()!=-1)throw new InvalidDataException();return values;
    }
    private float[] Body(double progress)
    {
        var key=(int)Math.Floor(Math.Clamp(progress,0,1)*Steps+.5);
        if(cache.TryGetValue(key,out var b))return b;
        float[]? reuse=null;if(cache.Count>=4){var expired=cacheOrder.Dequeue();reuse=cache[expired];cache.Remove(expired);}
        b=Read(key+2,375,reuse);cache[key]=b;cacheOrder.Enqueue(key);return b;
    }
    internal static double Smooth(double t){t=Math.Clamp(t,0,1);return t*t*(3-2*t);}
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static double InverseSpread(double x,double y,double amount)
    {
        if(amount==0)return x;
        var reach=24*amount*Smooth((y-66)/17);if(reach==0)return x;
        // Outside the transition the same 20 bisections have a closed result.
        // Keep their residual (rather than snapping to the analytic root).
        if(x+reach<=46)return x+reach-reach/1048576;
        if(x-reach>=60)return x-reach+reach/1048576;
        // Locate the original binary-search cell, not an approximate inverse.
        // Newton only proposes a cell. Reconstruct its endpoints using the
        // original midpoint operations and verify both monotone comparisons;
        // ambiguous boundaries fall back to the original search below.
        var estimate=53d;
        for(var i=0;i<6;i++){
            var t=Math.Clamp((estimate-46)/14,0,1);
            estimate-=(estimate+reach*(2*t*t*(3-2*t)-1)-x)/(1+12*reach*t*(1-t)/14);
        }
        var cell=Math.Clamp((int)Math.Floor((estimate-(x-reach))/(2*reach)*1048576),0,1048575);
        var candidateLo=x-reach;var candidateHi=x+reach;
        for(var bit=19;bit>=0;bit--){var mid=(candidateLo+candidateHi)/2;if((cell&(1<<bit))==0)candidateHi=mid;else candidateLo=mid;}
        if(candidateLo+reach*(2*Smooth((candidateLo-46)/14)-1)<x&&
            candidateHi+reach*(2*Smooth((candidateHi-46)/14)-1)>=x)return(candidateLo+candidateHi)/2;
        var lo=x-reach;var hi=x+reach;
        for(var i=0;i<20;i++){
            var u=(lo+hi)/2;
            var lower=u+reach*(2*Smooth((u-46)/14)-1)<x;
            if(lower)lo=u;else hi=u;
        }
        return (lo+hi)/2;
    }
    private static void Weights(double t,Span<double> w){var t2=t*t;var t3=t2*t;w[0]=-.5*t+t2-.5*t3;w[1]=1-2.5*t2+1.5*t3;w[2]=.5*t+2*t2-1.5*t3;w[3]=-.5*t2+.5*t3;}
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static void Sample(float[] data,int size,double x,double y,Span<double> result,bool cubic=true)
    {
        var ix=(int)Math.Floor(x);var iy=(int)Math.Floor(y);var fx=x-ix;var fy=y-iy;
        if(ix+1<0||iy+1<0||ix>=size||iy>=size){result.Clear();return;}
        if(ix>=0&&iy>=0&&ix+1<size&&iy+1<size){
            var p=(iy*size+ix)*4;var pixel=Vector128.LoadUnsafe(ref data[p]);
            var uniform=Vector128.EqualsAll(pixel,Vector128.LoadUnsafe(ref data[p+4]))&&
                Vector128.EqualsAll(pixel,Vector128.LoadUnsafe(ref data[p+size*4]))&&
                Vector128.EqualsAll(pixel,Vector128.LoadUnsafe(ref data[p+(size+1)*4]));
            // The bounded cubic clamps to these four taps. Uniform taps are
            // therefore exactly constant, even if outer cubic taps differ.
            if(uniform){for(var c=0;c<4;c++)result[c]=data[p+c];return;}
        }
        if(!cubic){
            var linear=Vector256<double>.Zero;
            for(var yy=0;yy<2;yy++)for(var xx=0;xx<2;xx++){
                var sx=ix+xx;var sy=iy+yy;
                if(sx<0||sy<0||sx>=size||sy>=size)continue;
                var at=(sy*size+sx)*4;
                var v=Vector256.Create((double)data[at],data[at+1],data[at+2],data[at+3]);
                var weight=(xx==0?1-fx:fx)*(yy==0?1-fy:fy);
                linear=Vector256.Add(linear,Vector256.Multiply(v,Vector256.Create(weight)));
            }
            var alphaLinear=Math.Clamp(linear.GetElement(3),0,255);
            Vector256.Min(Vector256.Max(linear,Vector256<double>.Zero),Vector256.Create(alphaLinear)).CopyTo(result);
            return;
        }
        Span<double> wx=stackalloc double[4],wy=stackalloc double[4];Weights(fx,wx);Weights(fy,wy);
        var sum=Vector256<double>.Zero;var low=Vector256.Create(255d);var high=Vector256<double>.Zero;
        for(var yy=cubic?0:1;yy<(cubic?4:3);yy++)for(var xx=cubic?0:1;xx<(cubic?4:3);xx++){
            var sx=ix+xx-1;var sy=iy+yy-1;var v=Vector256<double>.Zero;
            if(sx>=0&&sy>=0&&sx<size&&sy<size){var at=(sy*size+sx)*4;v=Vector256.Create((double)data[at],data[at+1],data[at+2],data[at+3]);}
            var weight=cubic?wx[xx]*wy[yy]:(xx==1?1-fx:fx)*(yy==1?1-fy:fy);
            sum=Vector256.Add(sum,Vector256.Multiply(v,Vector256.Create(weight)));
            if(xx is 1 or 2 && yy is 1 or 2){low=Vector256.Min(low,v);high=Vector256.Max(high,v);}
        }
        if(cubic)sum=Vector256.Min(Vector256.Max(sum,low),high);
        var alpha=Math.Clamp(sum.GetElement(3),0,255);
        Vector256.Min(Vector256.Max(sum,Vector256<double>.Zero),Vector256.Create(alpha)).CopyTo(result);
    }
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static bool Uniform(float[] data,int size,double x0,double y0,double x1,double y1,Span<double> value)
    {
        var left=(int)Math.Floor(x0);var right=(int)Math.Floor(x1)+1;
        var top=(int)Math.Floor(y0);var bottom=(int)Math.Floor(y1)+1;
        var first=true;
        for(var y=top;y<=bottom;y++)for(var x=left;x<=right;x++){
            var at=(y*size+x)*4;var inside=x>=0&&y>=0&&x<size&&y<size;
            for(var c=0;c<4;c++){var v=inside?data[at+c]:0;if(first)value[c]=v;else if(value[c]!=v)return false;}
            first=false;
        }
        return true;
    }
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    internal BitmapSource Render(double progress, Matrix inverse, PointD grab, PointD near, PointD far,
        double spread,double scale,HighResolutionFacePose? facePose=null,bool soleOnly=false,double contactBandPixels=2,double? bodyScale=null)
    {
        var bytes=RenderPixels(progress,inverse,grab,near,far,spread,scale,facePose,soleOnly,contactBandPixels,bodyScale);
        var started=System.Diagnostics.Stopwatch.GetTimestamp();var extent=(int)Math.Ceiling(256*scale);
        var bitmap=BitmapSource.Create(extent,extent,96*scale,96*scale,PixelFormats.Pbgra32,null,bytes,extent*4);bitmap.Freeze();
        LastCost=(LastCost.Preparation,LastCost.Sampling,System.Diagnostics.Stopwatch.GetElapsedTime(started).TotalMilliseconds);
        return bitmap;
    }
    // Borrowed until the next render; capacity may exceed the current extent.
    // Synchronous geometry reads only extent*extent pixels, not spare capacity.
    internal byte[] RenderSolePixels(double progress,Matrix inverse,PointD grab,PointD near,PointD far,
        double spread,double scale,HighResolutionFacePose face,double band,double bodyScale)=>
        RenderPixels(progress,inverse,grab,near,far,spread,scale,face,true,band,bodyScale);

    internal byte[] RenderRetainedPixels(double progress,Matrix inverse,PointD grab,PointD near,PointD far,
        double spread,double scale,HighResolutionFacePose face,double bodyScale,byte[]? reuse,bool preserveCapacity=false)
    {
        var pixels=RenderPixels(progress,inverse,grab,near,far,spread,scale,face,false,2,bodyScale);
        var extent=(int)Math.Ceiling(256*scale);
        var length=checked(extent*extent*4);
        // The presenter may retain high-water storage across size changes.
        // Only extent*extent pixels belong to this frame; consumers use that
        // explicit extent, never infer geometry from the buffer capacity.
        var retained=reuse is not null&&(reuse.Length==length||(preserveCapacity&&reuse.Length>=length))?reuse:new byte[length];
        Array.Copy(pixels,retained,length);return retained;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private byte[] RenderPixels(double progress,Matrix inverse,PointD grab,PointD near,PointD far,
        double spread,double scale,HighResolutionFacePose? facePose,bool soleOnly,double contactBandPixels,double? bodyScale)
    {
        var started=System.Diagnostics.Stopwatch.GetTimestamp();
        var inkScale=bodyScale??scale;
        var body=Body(progress);var extent=(int)Math.Ceiling(256*scale);
        var length=extent*extent*4;
        // One high-water scratch, not one new LOH allocation for every slider
        // step. Grow geometrically so a first upward sweep is bounded too.
        if(outputBuffer.Length<length)outputBuffer=new byte[Math.Max(length,outputBuffer.Length*2)];
        else if(_renderRegion is {} dirty){
            for(var y=dirty.Top;y<dirty.Bottom;y++)Array.Clear(outputBuffer,(y*extent+dirty.Left)*4,(dirty.Right-dirty.Left)*4);
        }else Array.Clear(outputBuffer,0,length);
        var bytes=outputBuffer;
        var face=facePose??HighResolutionFacePose.Neutral;
        var stomp=new StompWarp(face,inkScale);
        var chuckle=new ChuckleWarp(face,inkScale);
        var stretch=new StretchWarp(face);
        var seated=new SeatedWarp(face);
        var seatedPaw=new SeatedPaw(face,inkScale);
        if(seatedPaw.Active)seatedBodyDistance??=new SeatedBodyDistance((locomotion??=new()).BorrowBody(321,1));
        var facePin=SurroundingPullRenderer.HeadFacePin(Center,grab);
        var faceInverse=face.Head;
        faceInverse.Invert();
        var activeHead=face.Angry?AngryHead(face.Furious):face.Closed?ClosedHead():head;
        var preparedMotion=face.BodyFrame<0&&face.MotionFrame>=0&&!(face.MotionFrame==HighResolutionLocomotion.Perch&&face.Paw!=0);
        var posedBody=face.BodyFrame>=0?(huntBodies??=new()).Get(face.BodyFrame):face.MotionFrame>=0?
            (preparedMotion?(locomotion??=new()).BorrowBody(face.MotionFrame,inkScale):(locomotion??=new()).Get(face.MotionFrame)):null;
        FlutterLayers? flutter=null;
        if(face.FlutterPhase>=0&&posedBody is null){
            var key=(int)Math.Floor(Math.Clamp(progress,0,1)*Steps+.5);
            if(flutterLayers is null||flutterKey!=key){flutterLayers=new(body,progress>=.5);flutterKey=key;}
            flutter=flutterLayers;flutter.SetPhase(face.FlutterPhase);
        }
        if(posedBody is not null&&face.MotionFrame==HighResolutionLocomotion.Perch&&face.Paw!=0)
            posedBody=PerchPaw(posedBody,face);
        var paddedPaw=posedBody is not null&&ReferenceEquals(posedBody,perchPawBuffer);
        var posedSize=paddedPaw?375:225;
        var posedOffset=paddedPaw?32:0;
        body=HighResolutionBodyInk.AtScale(body,375,inkScale);
        if(posedBody is not null&&!preparedMotion)posedBody=HighResolutionBodyInk.AtScale(posedBody,posedSize,inkScale);
        flutter?.SetScale(inkScale);
        var neck=face.BodyFrame>=0?(neckBuffer??=new()).Get(face):null;
        if(neck is not null)neck=HighResolutionBodyInk.AtScale(neck,225,inkScale);
        var effect=face.EffectFrame>=0?(locomotion??=new()).BorrowEffect(face.EffectFrame):null;
        var snack=face.MouthAge is >=0 and <.48||face.DroolAge>=0?(snackBuffer??=new()).Get(face.MouthAge,face.DroolAge):null;
        // Conservative bounds measured from the immutable body/head, expanded
        // for cubic taps, lag and maximum monotone paw displacement.
        double left=double.PositiveInfinity,right=double.NegativeInfinity,topBound=double.PositiveInfinity,bottom=double.NegativeInfinity;
        void Bounds(float[] data,int size,double offset,Matrix? transform=null,double margin=0){
            var box=RasterAlphaBounds.Get(data,size);
            var l=box.Left;var r=box.Right;var t=box.Top;var b=box.Bottom;
            if(l>r||t>b)return;
            l=l*96/225-offset-margin;r=r*96/225-offset+margin;t=t*96/225-offset-margin;b=b*96/225-offset+margin;
            foreach(var corner in new[]{new Point(l,t),new Point(r,t),new Point(l,b),new Point(r,b)}){
                var p=transform?.Transform(corner)??corner;left=Math.Min(left,p.X);right=Math.Max(right,p.X);topBound=Math.Min(topBound,p.Y);bottom=Math.Max(bottom,p.Y);
            }
        }
        Bounds(posedBody??body,posedBody is null?375:posedSize,posedBody is null?32:posedOffset,margin:seatedPaw.Active?24:seated.Active?10:stretch.Active?12:stomp.Active?Math.Max(6,face.StompReach+2):face.Breathing>0?1:0);
        Bounds(activeHead,225,0,face.Head,face.Pull!=0||face.EyeFollow!=0||face.HairFollow!=0?14:2);
        if(effect is not null)Bounds(effect,225,0,face.Head);
        if(snack is not null)Bounds(snack,225,0,face.Head);
        if(neck is not null)Bounds(neck,225,0);
        left-=4+24*spread;right+=4+24*spread;topBound-=4;bottom+=4;
        // Bound the destination work before supersampling. The original loop
        // sampled the entire transparent 256px gutter (64 times for paw rows).
        // Affine extrema are at the rectangle corners; one output pixel is a
        // conservative margin for all subpixel sample locations.
        var forward=inverse;forward.Invert();
        var corners=new[]{new Point(left,topBound),new Point(right,topBound),new Point(left,bottom),new Point(right,bottom)}.Select(forward.Transform).ToArray();
        var minX=Math.Clamp((int)Math.Floor((corners.Min(p=>p.X)+80)*scale)-1,0,extent);
        var maxX=Math.Clamp((int)Math.Ceiling((corners.Max(p=>p.X)+80)*scale)+1,0,extent);
        var minY=Math.Clamp((int)Math.Floor((corners.Min(p=>p.Y)+80)*scale)-1,0,extent);
        var maxY=Math.Clamp((int)Math.Ceiling((corners.Max(p=>p.Y)+80)*scale)+1,0,extent);
        if(_renderRegion is {} region){
            minX=Math.Max(minX,region.Left);maxX=Math.Min(maxX,region.Right);
            minY=Math.Max(minY,region.Top);maxY=Math.Min(maxY,region.Bottom);
        }
        if(!soleOnly)LastRenderedPixelCount=(long)Math.Max(0,maxX-minX)*Math.Max(0,maxY-minY);
        float[]? low=null;
        if(spread>0 && !spreadCache.TryGetValue(spread,out low)){
            if(spreadCache.Count>=4){var expired=spreadCache.Keys.First();low=spreadCache[expired];spreadCache.Remove(expired);HighResolutionBodyInk.Invalidate(low);}
            else low=new float[160*160*4];
            Span<double> tmp=stackalloc double[4];
            for(var y=0;y<160;y++)for(var x=0;x<160;x++){Sample(nativeBody,160,InverseSpread(x-32,y-32,spread)+32,y,tmp,false);for(var c=0;c<4;c++)low[(y*160+x)*4+c]=(float)tmp[c];}
            spreadCache[spread]=low;
        }
        if(low is not null)low=HighResolutionBodyInk.AtScale(low,160,inkScale);
        // Rows are independent. Bound CPU parallelism; never touch WPF objects
        // on workers. The byte output is identical to the serial sampler.
        void Row(int py,int fromX,int toX){
        Span<double> total=stackalloc double[4],a=stackalloc double[4],b=stackalloc double[4],l=stackalloc double[4];
        var rowTop=inverse.Transform(new Point(48,py/scale-80));
        var rowCoverage=spread>0&&rowTop.Y>=74&&rowTop.Y<=112?8:2;
        for(var px=fromX;px<toX;px++){
            // Refine only the compressed lower-body footprint as in r49.
            var coverage=rowCoverage;
            if(face.Pull>0&&coverage==2){
                var contour=faceInverse.Transform(inverse.Transform(new Point((px+.5)/scale-80,(py+.5)/scale-80)));
                if(contour.Y>=53&&contour.Y<64){
                    var edge=HighResolutionCheekSelection.FastEdge(contour.Y,Math.Clamp(face.Pull/20,0,1));
                    // More area samples only along the exposed stretched
                    // contour. Do not blur, widen, or resample the fixed hair.
                    if(Math.Abs(contour.X-edge)<2&&contour.X<HighResolutionCheekSelection.Edge(contour.Y)-1)coverage=4;
                }
            }
            // Bounded cubic is constant whenever every central tap in this
            // pixel's footprint is constant. This skips interior work only;
            // a varying contour always retains the full 8x8 coverage path.
            if(coverage==8&&!stretch.Active&&posedBody is null&&neck is null&&effect is null&&snack is null&&flutter is null){
                var p0=inverse.Transform(new Point(px/scale-80,py/scale-80));
                var p1=inverse.Transform(new Point((px+1)/scale-80,py/scale-80));
                var p2=inverse.Transform(new Point(px/scale-80,(py+1)/scale-80));
                var p3=inverse.Transform(new Point((px+1)/scale-80,(py+1)/scale-80));
                var x0=Math.Min(Math.Min(p0.X,p1.X),Math.Min(p2.X,p3.X));var x1=Math.Max(Math.Max(p0.X,p1.X),Math.Max(p2.X,p3.X));
                var y0=Math.Min(Math.Min(p0.Y,p1.Y),Math.Min(p2.Y,p3.Y));var y1=Math.Max(Math.Max(p0.Y,p1.Y),Math.Max(p2.Y,p3.Y));
                // Head tilt doesn't disable constant-footprint coverage: test
                // its actual inverse-transformed rectangle. Face features end
                // at local Y71. Body breathing ends at Y74; material lag is
                // exactly zero at world Y84 (not merely "small enough").
                var h0=faceInverse.Transform(p0);var h1=faceInverse.Transform(p1);
                var h2=faceInverse.Transform(p2);var h3=faceInverse.Transform(p3);
                var hx0=Math.Min(Math.Min(h0.X,h1.X),Math.Min(h2.X,h3.X));var hx1=Math.Max(Math.Max(h0.X,h1.X),Math.Max(h2.X,h3.X));
                var hy0=Math.Min(Math.Min(h0.Y,h1.Y),Math.Min(h2.Y,h3.Y));var hy1=Math.Max(Math.Max(h0.Y,h1.Y),Math.Max(h2.Y,h3.Y));
                if(y0>=76&&hy0>=71&&((near==default&&far==default)||y0>=84)&&
                    Uniform(activeHead,225,hx0*225/96-.5,hy0*225/96-.5,hx1*225/96-.5,hy1*225/96-.5,b)&&
                    low is not null&&Uniform(low,160,x0+32-.5,y0+32-.5,x1+32-.5,y1+32-.5,l)){
                    var bx0=Math.Min(InverseSpread(x0,y0,spread),InverseSpread(x0,y1,spread));
                    var bx1=Math.Max(InverseSpread(x1,y0,spread),InverseSpread(x1,y1,spread));
                    if(spread==1||Uniform(body,375,(bx0+32)*225/96-.5,(y0+32)*225/96-.5,(bx1+32)*225/96-.5,(y1+32)*225/96-.5,a)){
                        var mix=Smooth(spread);var constantAt=(py*extent+px)*4;
                        for(var c=0;c<4;c++){var v=spread==1?l[c]:a[c]+(l[c]-a[c])*mix;
                            bytes[constantAt+c]=(byte)Math.Clamp(Math.Floor(b[c]+v*(1-b[3]/255)+.5),0,255);}
                        continue;
                    }
                }
            }
            total.Clear();for(var yy=0;yy<coverage;yy++)for(var xx=0;xx<coverage;xx++){
                var q=inverse.Transform(new Point((px+(xx+.5)/coverage)/scale-80,(py+(yy+.5)/coverage)/scale-80));
                if(q.X<left||q.X>right||q.Y<topBound||q.Y>bottom)continue;
                var uv=new PointD(q.X,q.Y);
                if(near!=default||far!=default)for(var k=0;k<8;k++){
                    var d=SurroundingPullRenderer.HeadDisplacement(uv,Center,grab,near,far,facePin);var next=new PointD(q.X-d.X,q.Y-d.Y);
                    var done=Math.Abs(next.X-uv.X)+Math.Abs(next.Y-uv.Y)<1e-8;uv=next;if(done)break;}
                var headLocal=faceInverse.Transform(new Point(uv.X,uv.Y));
                var headPoint=face.Source(headLocal,out var skinDonor);
                Sample(activeHead,225,headPoint.X*225/96-.5,headPoint.Y*225/96-.5,b);
                if(!skinDonor&&!face.Closed&&(face.EyeX!=0||face.EyeY!=0))ApplyRigidGaze(headPoint,face.EyeX,face.EyeY,b,l);
                if(snack is not null){
                    Sample(snack,225,headLocal.X*225/96-.5,headLocal.Y*225/96-.5,l);
                    var clear=1-l[3]/255;for(var c=0;c<4;c++)b[c]=l[c]+b[c]*clear;
                }
                if(effect is not null){
                    Sample(effect,225,headLocal.X*225/96-.5,headLocal.Y*225/96-.5,l);
                    var clear=1-l[3]/255;for(var c=0;c<4;c++)b[c]=l[c]+b[c]*clear;
                }
                if(b[3]>=255&&!chuckle.Active){for(var c=0;c<4;c++)total[c]+=b[c]/(coverage*coverage);continue;}
                // Approved r35 left attachment occlusion: reuse body texels,
                // only behind an independently raised head.
                var bodyY=face.Head.M12>0&&uv.X>=18&&uv.X<29&&uv.Y>=57&&uv.Y<66.5?66.5:uv.Y;
                var bodyX=uv.X;
                if(stretch.Active){var source=stretch.Source(bodyX,bodyY);bodyX=source.X;bodyY=source.Y;}
                if(seated.Active){var source=seated.Source(bodyX,bodyY);bodyX=source.X;bodyY=source.Y;}
                if(chuckle.Active){var source=chuckle.BodySource(bodyX,bodyY);bodyX=source.X;bodyY=source.Y;}
                if(stomp.Active){var source=stomp.Source(bodyX,bodyY);bodyX=source.X;bodyY=source.Y;}
                if(face.Breathing>0&&bodyX>=50&&bodyX<84&&bodyY>=49&&bodyY<74){
                    var w=Smooth((bodyX-50)/13)*Math.Pow(Math.Sin(Math.PI*(bodyY-49)/25),2)*face.Breathing;
                    bodyX-=.4*w;bodyY+=.22*w;
                }
                if(posedBody is not null){
                    bodyX=SeatedContourSource(bodyX,bodyY,face.MotionFrame);
                    Sample(posedBody,posedSize,(bodyX+posedOffset)*225/96-.5,(bodyY+posedOffset)*225/96-.5,a);
                }
                else if(flutter is not null)flutter.SampleAt(bodyX,bodyY,a,l);
                else Sample(body,375,(InverseSpread(bodyX,bodyY,spread)+32)*225/96-.5,(bodyY+32)*225/96-.5,a);
                if(seatedPaw.Active)seatedPaw.Composite(seatedBodyDistance!,uv.X,uv.Y,a);
                if(stomp.RigidArm)stomp.CompositeArm(body,uv.X,uv.Y,bodyX,bodyY,a,l);
                if(chuckle.Active)chuckle.RemovePaw(body,bodyX,bodyY,a,l);
                if(neck is not null&&a[3]<255){Sample(neck,225,uv.X*225/96-.5,uv.Y*225/96-.5,l);var clear=1-a[3]/255;for(var c=0;c<4;c++)a[c]+=l[c]*clear;}
                var mix=Smooth((uv.Y-74)/2)*Smooth(spread);
                if(low is not null&&mix>0){Sample(low,160,uv.X+32-.5,uv.Y+32-.5,l,false);for(var c=0;c<4;c++)a[c]+=(l[c]-a[c])*mix;}
                if(chuckle.Active){
                    var clear=1-b[3]/255;
                    for(var c=0;c<4;c++)a[c]=b[c]+a[c]*clear;
                    chuckle.Overlay(body,uv.X,uv.Y,a,l);
                    for(var c=0;c<4;c++)total[c]+=a[c]/(coverage*coverage);
                }else for(var c=0;c<4;c++)total[c]+=(b[c]+a[c]*(1-b[3]/255))/(coverage*coverage);
            }
            var at=(py*extent+px)*4;for(var c=0;c<4;c++)bytes[at+c]=(byte)Math.Clamp(Math.Floor(total[c]+.5),0,255);
        }
        }
        var prepared=System.Diagnostics.Stopwatch.GetTimestamp();
        if(soleOnly){
            // Geometry uses the very same output samples/alpha threshold. Work
            // upward until the occupied bottom and its complete 2-DIP band are
            // known; never rasterize the face merely to find the soles.
            var sole=-1;
            for(var py=maxY-1;py>=minY;py--){
                if(sole>=0 && py+1<=sole-contactBandPixels+1e-9)break;
                // An eight-row batch can sample seven expensive 8x8 contour
                // rows above the required band. Query one row at a time;
                // split only its expensive 8x8 pixel work across the existing
                // bounded workers. Samples/threshold/stop row are unchanged.
                var rowTop=inverse.Transform(new Point(48,py/scale-80));
                if(spread>0&&rowTop.Y>=74&&rowTop.Y<=112&&maxX-minX>=32)
                    RasterRowScheduler.For(minX,maxX,px=>Row(py,px,px+1));
                else Row(py,minX,maxX);
                if(sole<0)for(var px=minX;px<maxX;px++)
                    if(bytes[(py*extent+px)*4+3]>=128){sole=py+1;break;}
            }
        }else RasterRowScheduler.For(minY,maxY,py=>Row(py,minX,maxX));
        var sampled=System.Diagnostics.Stopwatch.GetTimestamp();
        LastCost=(System.Diagnostics.Stopwatch.GetElapsedTime(started,prepared).TotalMilliseconds,
            System.Diagnostics.Stopwatch.GetElapsedTime(prepared,sampled).TotalMilliseconds,
            0);
        return bytes;
    }
    private float[] ClosedHead(){
        if(closedHead is not null)return closedHead;
        var source=(locomotion??=new()).Get(0);
        closedHead=(float[])source.Clone();
        // Five native texels retain the open upper-lid spur over the pink
        // fringe in the closed bank. Continue the adjoining fringe texels;
        // leave the lid, hair edge/tip, mouth and immutable bank untouched.
        Array.Copy(source,(106*225+45)*4,closedHead,(108*225+44)*4,3*4);
        Array.Copy(source,(110*225+44)*4,closedHead,(109*225+44)*4,2*4);
        closedHead=HighResolutionClosedInk.Prepare(closedHead);
        return closedHead;
    }
    private float[] PerchPaw(float[] source,HighResolutionFacePose face){
        // The synchronous raster owns this scratch. All workers finish before
        // the next preparation; never mutate the immutable motion-bank source.
        // Keep the same texel density; maximum extension exceeds logical Y96.
        // A 32-DIP gutter carries the complete tip without changing its gain.
        const int size=375,pad=75;
        var output=perchPawBuffer??=new float[size*size*4];
        HighResolutionBodyInk.Invalidate(output);Array.Clear(output);
        for(var y=0;y<225;y++)Array.Copy(source,y*225*4,output,((y+pad)*size+pad)*4,225*4);
        if(face.PawX==0&&face.PawY==0&&face.PawAngle==0)return output;
        // Include the outer stroke and the sloping armpit (ends at X53).
        // The old X19..51 rectangle sliced that authored contour.
        var right=face.Paw==2;var left=right?35:17;var end=right?54:35;
        var stretch=Math.Clamp(1+face.PawY/14,.6,2.3);
        var bend=face.PawX+Math.Tan(face.PawAngle*Math.PI/180)*14;
        bend=right?Math.Max(-2.5,bend):Math.Min(2.5,bend);
        Span<double> sample=stackalloc double[4];
        for(var y=pad;y<size;y++)for(var x=0;x<size;x++){
            var lx=(x-pad+.5)*96/225;var ly=(y-pad+.5)*96/225;
            if(ly<70)continue;
            var at=(y*size+x)*4;
            if(lx>=left&&lx<end)Array.Clear(output,at,4);
            // The opposite paw remains in front during an inward pull.
            var sourceX=x-pad;var sourceY=y-pad;
            if(sourceX>=0&&sourceX<225&&sourceY<225&&
               (right?lx>=17&&lx<35:lx>=35&&lx<54)&&source[(sourceY*225+sourceX)*4+3]>0)continue;
            // Zero displacement at the first affected row: no jump between
            // the stationary shoulder and the clicked paw's shear.
            var sy=(ly-70)/stretch+70;var sx=lx-bend*Math.Clamp((sy-70)/14,0,1);
            if(sx<left||sx>=end||sy<70||sy>=85)continue;
            Sample(source,225,sx*225/96-.5,sy*225/96-.5,sample);
            var clear=1-sample[3]/255;for(var c=0;c<4;c++)output[at+c]=(float)(sample[c]+output[at+c]*clear);
        }
        return output;
    }
}

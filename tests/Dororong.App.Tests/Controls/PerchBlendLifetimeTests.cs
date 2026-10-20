using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;

namespace Dororong.App.Tests.Controls;

public sealed class PerchBlendLifetimeTests
{
    [Theory]
    [InlineData(1)] [InlineData(1.1)] [InlineData(1.25)] [InlineData(1.35)] [InlineData(1.5)]
    public void Reused_blend_matches_original_clip_and_handles_aliased_inputs(double scale)=>CheekProductTests.Sta(()=>{
        var size=(int)Math.Ceiling(256*scale);var stride=size*4;
        var a=new byte[stride*size];var b=new byte[a.Length];
        for(var i=0;i<a.Length;i++){a[i]=(byte)(i%251);b[i]=(byte)(i%239);}
        BitmapSource Image(byte[] bytes)=>BitmapSource.Create(size,size,96*scale,96*scale,PixelFormats.Pbgra32,null,bytes,stride);
        var first=Image(a);var second=Image(b);var blend=new HighResolutionPerchBlend();
        BitmapSource? identity=null;byte[]? buffer=null;
        for(var i=0;i<30;i++){
            var current=i%3==0&&identity is not null?identity:second;
            var previous=i%5==0?current:first;
            var age=i%8*15d;
            var expected=new byte[a.Length];current.CopyPixels(expected,stride,0);
            var old=new byte[a.Length];previous.CopyPixels(old,stride,0);
            var row=Math.Clamp((int)Math.Ceiling((104+70*age/120)/256*size),0,size);
            Buffer.BlockCopy(old,row*stride,expected,row*stride,(size-row)*stride);
            var actual=blend.Update(previous,current,age);
            if(identity is not null){Assert.Same(identity,actual);Assert.Same(buffer,blend.Pixels);}
            identity=actual;buffer=blend.Pixels;
            Assert.Equal(expected,blend.Pixels);
            var copied=new byte[a.Length];actual.CopyPixels(copied,stride,0);Assert.Equal(expected,copied);
        }
    });
}

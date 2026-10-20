using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;

namespace Dororong.App.Tests.Controls;

public sealed class SnackImageRetentionTests
{
    internal static BitmapSource Original()
    {
        var image=new BitmapImage(new Uri("pack://application:,,,/Dororong.App;component/Assets/snack-jewel.png"));
        image.Freeze();return image;
    }
    internal static byte[] Draw(ImageSource source,int extent,double size,double offset)
    {
        var visual=new DrawingVisual();
        using(var dc=visual.RenderOpen())dc.DrawImage(source,new Rect(offset,offset,size,size));
        var target=new RenderTargetBitmap(extent,extent,96,96,PixelFormats.Pbgra32);
        target.Render(visual);var pixels=new byte[extent*extent*4];target.CopyPixels(pixels,extent*4,0);
        return pixels;
    }
    [Fact]
    public void Shared_snack_is_detached_and_preserves_full_source_pixels()=>CheekProductTests.Sta(()=>{
        var original=Original();
        var actual=SnackImage.Source;
        Assert.Same(actual,SnackImage.Source);
        Assert.True(actual.IsFrozen);
        Assert.IsNotType<BitmapImage>(actual);
        Assert.IsNotType<FormatConvertedBitmap>(actual);
        Assert.Equal(original.PixelWidth,actual.PixelWidth);
        Assert.Equal(original.PixelHeight,actual.PixelHeight);
        var expected=new FormatConvertedBitmap(original,PixelFormats.Pbgra32,null,0);
        var a=new byte[actual.PixelWidth*actual.PixelHeight*4];var b=new byte[a.Length];
        actual.CopyPixels(a,actual.PixelWidth*4,0);expected.CopyPixels(b,actual.PixelWidth*4,0);
        Assert.Equal(b,a);
    });
    [Theory]
    [InlineData(1)] [InlineData(1.1)] [InlineData(1.25)] [InlineData(1.35)] [InlineData(1.5)]
    [InlineData(2)] [InlineData(2.5)] [InlineData(3)]
    public void Cursor_and_chewing_pixels_match_original(double scale)=>CheekProductTests.Sta(()=>{
        var original=Original();
        foreach(var size in new[]{36d,32d,23.4375,12.125,2.34375})
        foreach(var offset in new[]{0d,.125,.5,.875}){
            var extent=(int)Math.Ceiling((size+2)*scale);
            Assert.Equal(Draw(original,extent,size*scale,offset),Draw(SnackImage.Source,extent,size*scale,offset));
        }
    });

    [Fact]
    public void Repeated_menu_templates_share_the_detached_snack_source()=>CheekProductTests.Sta(()=>{
        var resources=new ResourceDictionary{
            Source=new Uri("/Dororong.App;component/Product/PetMenuResources.xaml",UriKind.Relative)};
        var template=(ControlTemplate)resources["SnackTemplate"];
        static Image? Find(DependencyObject node){
            if(node is Image image)return image;
            for(var i=0;i<VisualTreeHelper.GetChildrenCount(node);i++){
                if(Find(VisualTreeHelper.GetChild(node,i)) is {} found)return found;
            }
            return null;
        }
        for(var i=0;i<5;i++){
            var icon=Find((DependencyObject)template.LoadContent());
            Assert.NotNull(icon);
            Assert.Same(SnackImage.Source,icon.Source);
            Assert.Equal(36,icon.Width);
            Assert.Equal(BitmapScalingMode.HighQuality,RenderOptions.GetBitmapScalingMode(icon));
        }
    });
}

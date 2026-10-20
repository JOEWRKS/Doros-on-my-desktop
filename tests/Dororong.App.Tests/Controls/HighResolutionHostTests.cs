using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;
using Dororong.App.Interaction;
using Dororong.App.Interop;
using Dororong.App.Runtime;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;

namespace Dororong.App.Tests.Controls;

public sealed class HighResolutionHostTests
{
    [Theory]
    [InlineData(1)] [InlineData(1.25)] [InlineData(1.5)]
    public void Legacy_visible_head_press_seeds_high_resolution_pin(double scale)=>CheekProductTests.Sta(()=>
    {
        var p=new DororongPresenter{HighResolutionGrabEnabled=true,HighResolutionScale=scale,Width=144*scale,Height=144*scale};
        var pet=new PetSnapshot(PetState.Walk,new(100,100),FacingDirection.Right,0,false,null);
        p.Render(pet,DirectInteractionSnapshot.None,TimeSpan.Zero);p.Measure(new(p.Width,p.Height));p.Arrange(new(0,0,p.Width,p.Height));p.UpdateLayout();
        var image=(AlphaHitTestImage)p.FindName("DororongImage");
        var press=p.CreateDirectPress(image,new(37.5,28.25));Assert.NotNull(press);Assert.Equal(DirectInteractionTarget.Body,press!.Target);
        var origin=pet.Position+press.WindowLocalPosition;
        p.Render(pet,new(DirectInteractionTarget.Body,DirectInteractionPhase.BodyPending,origin,origin,0,0,false){PressFacing=press.PressFacing},TimeSpan.Zero);
        var pinned=p.HighResolutionSourceToPresenter(new(37.5,28.25));
        Assert.InRange(Math.Abs(pinned.X-press.WindowLocalPosition.X),0,.000001);
        Assert.InRange(Math.Abs(pinned.Y-press.WindowLocalPosition.Y),0,.000001);
    });

    [Theory]
    [InlineData(1)] [InlineData(1.1)] [InlineData(1.25)] [InlineData(1.35)] [InlineData(1.5)]
    public void Production_viewport_capture_mirror_extreme_angles_and_regrab(double scale)=>CheekProductTests.Sta(()=>
    {
        var p=new DororongPresenter {HighResolutionGrabEnabled=true,HighResolutionScale=scale,Width=144*scale,Height=144*scale};
        ((Canvas)p.Content).Width=p.Width;((Canvas)p.Content).Height=p.Height;
        var root=new Grid();root.Children.Add(p);
        var window=new Window {Content=root,WindowStyle=WindowStyle.None,AllowsTransparency=true,Opacity=0,ShowActivated=false,ShowInTaskbar=false};
        var output=Environment.GetEnvironmentVariable("DORORONG_R52_OUTPUT");var rows=new List<object>();var index=0;
        if(output is not null)Directory.CreateDirectory(output);
        try{
            window.Show(); // Transparent test HWND only; no synthetic desktop mouse input.
            var factory=typeof(PetLoop).GetMethod("CreateProductionRuntime",BindingFlags.NonPublic|BindingFlags.Static)!;
            var runtime=factory.Invoke(null,new object[]{window,p,new DesktopInput()})!;
            var host=(PetLoopHost)runtime.GetType().GetProperty("Host")!.GetValue(runtime)!;
            Assert.Equal(new SizeD(144*scale,144*scale),host.GetPetSize());
            host.SetWindowPosition(new(300,200));Assert.Equal(new PointD(300,200),host.GetWindowPosition());
            void Layout(){root.Measure(new(window.Width,window.Height));root.Arrange(new(0,0,window.Width,window.Height));root.UpdateLayout();}
            void Save(string phase){
                var image=p.HighResolutionImage!;var source=(BitmapSource)image.Source;
                var pixels=new byte[source.PixelWidth*source.PixelHeight*4];source.CopyPixels(pixels,source.PixelWidth*4,0);
                for(var y=0;y<source.PixelHeight;y++)for(var x=0;x<source.PixelWidth;x++)if(pixels[(y*source.PixelWidth+x)*4+3]>0){
                    Assert.InRange(x,1,source.PixelWidth-2);Assert.InRange(y,1,source.PixelHeight-2);
                    var point=image.TranslatePoint(new((x+.5)*image.ActualWidth/source.PixelWidth,(y+.5)*image.ActualHeight/source.PixelHeight),root);
                    Assert.InRange(point.X,0,root.ActualWidth);Assert.InRange(point.Y,0,root.ActualHeight);
                }
                var file=$"host-{scale:F2}-{index++:D3}-{phase}.png";
                if(output is not null){using var stream=File.Create(Path.Combine(output,file));var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(source));encoder.Save(stream);}
                rows.Add(new{file,phase,scale});
            }
            foreach(var facing in new[]{FacingDirection.Left,FacingDirection.Right}){
                // Retire the previous direct ownership before changing facing.
                var pet=new PetSnapshot(PetState.Walk,new(300,200),facing,0,false,null);
                p.Render(pet,DirectInteractionSnapshot.None,TimeSpan.Zero);Layout();
                pet=pet with {State=PetState.Idle};p.Render(pet,DirectInteractionSnapshot.None,TimeSpan.Zero);Layout();
                foreach(var grab in new[]{new Point(37.5,28.25),new Point(37.5,35)}){
                    var image=p.HighResolutionImage!;var local=p.HighResolutionSourceToPresenter(grab);
                    var press=p.CreateDirectPress(image,p.TranslatePoint(local,image));Assert.NotNull(press);
                    Assert.Equal(DirectInteractionTarget.Body,press!.Target);
                    var origin=pet.Position+press.WindowLocalPosition;
                    var direct=new DirectInteractionSnapshot(DirectInteractionTarget.Body,DirectInteractionPhase.BodyDragHold,origin,origin,1,0,true){PressFacing=facing};
                    foreach(var angle in new[]{-88d,-45,0,45,88}){
                        p.Render(pet,direct with {HeadSwingDegrees=angle},TimeSpan.Zero);Layout();
                        var pinned=p.HighResolutionSourceToPresenter(grab);
                        Assert.InRange((pinned-local).Length,0,.000001);Save($"{facing}-angle-{angle}");
                    }
                    // Same screen press origin must still create a fresh capture.
                    var before=((BitmapSource)p.HighResolutionImage!.Source);var bodyBefore=p.HighResolutionDiagnostic;
                    var a=new byte[before.PixelWidth*before.PixelHeight*4];before.CopyPixels(a,before.PixelWidth*4,0);
                    var again=p.CreateDirectPress(p.HighResolutionImage,p.TranslatePoint(p.HighResolutionSourceToPresenter(grab),p.HighResolutionImage));Assert.NotNull(again);
                    p.Render(pet,direct with {Phase=DirectInteractionPhase.BodyPending,Strength=0,HeadSwingDegrees=0},TimeSpan.Zero);Layout();
                    var after=(BitmapSource)p.HighResolutionImage.Source;
                    var b=new byte[a.Length];after.CopyPixels(b,after.PixelWidth*4,0);
                    Assert.Equal(a,b);Save($"{facing}-regrab");
                    p.Render(pet,direct with {Phase=DirectInteractionPhase.BodyDragSettle,ReleaseProgress=1},TimeSpan.Zero);Layout();Save($"{facing}-released");
                }
            }
            Assert.True(host.SamplePointer().IsAvailable);
            if(output is not null)File.WriteAllText(Path.Combine(output,$"host-{scale:F2}.json"),JsonSerializer.Serialize(rows));
        }finally{window.Close();}
    });
}

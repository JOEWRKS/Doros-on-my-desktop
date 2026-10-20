using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;
using Dororong.App.Interaction;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;

namespace Dororong.App.Tests.Controls;

public sealed class HighResolutionLocomotionTests
{
    [Theory]
    [InlineData(1)] [InlineData(1.25)] [InlineData(1.5)]
    public void Walk_sit_rise_and_blink_use_source_resolution_parts(double scale)=>CheekProductTests.Sta(()=>{
        var output=Environment.GetEnvironmentVariable("DORORONG_R54_MOTION_OUTPUT");
        if(output is not null)Directory.CreateDirectory(output);
        var p=new DororongPresenter{HighResolutionGrabEnabled=true,HighResolutionScale=scale,Width=144*scale,Height=144*scale};
        ((Canvas)p.Content).Width=p.Width;((Canvas)p.Content).Height=p.Height;
        var pet=new PetSnapshot(PetState.Idle,new(100,100),FacingDirection.Right,0,false,null);
        var rows=new List<object>();var index=0;
        void Render(string phase,double ms){
            p.Render(pet,DirectInteractionSnapshot.None,TimeSpan.FromMilliseconds(ms));
            p.Measure(new(p.Width,p.Height));p.Arrange(new(0,0,p.Width,p.Height));p.UpdateLayout();
            Assert.NotNull(p.HighResolutionImage);
            var image=p.HighResolutionImage!;var b=(BitmapSource)image.Source;
            Assert.Equal((int)Math.Ceiling(256*scale),b.PixelWidth);
            var filename=$"motion-{scale:F2}-{index++:D3}-{phase}.png";
            if(output is not null){using var s=File.Create(Path.Combine(output,filename));var enc=new PngBitmapEncoder();enc.Frames.Add(BitmapFrame.Create(b));enc.Save(s);}
            rows.Add(new{filename,phase,scale});
        }
        Render("idle",0);
        pet=pet with {State=PetState.Walk};
        for(var i=0;i<32;i++){pet=pet with {Position=pet.Position+new PointD(10d/32,0)};Render("walk",40);}
        pet=pet with {State=PetState.Idle};
        for(var i=0;i<6;i++)Render("walk-stop",40);
        p.SetSittingRequested(true);
        for(var i=0;i<17;i++)Render("sit",1050d/16);
        // The displayed pose, not a hidden 96px image, supplies the capture.
        var head=p.HighResolutionFaceToPresenter(new(37.5,28.25));
        var press=p.CreateDirectPress(p.HighResolutionImage!,p.TranslatePoint(head,p.HighResolutionImage!));
        Assert.NotNull(press);Assert.Equal(DirectInteractionTarget.Body,press!.Target);
        p.SetSittingRequested(false);
        for(var i=0;i<17;i++)Render("rise",650d/16);
        for(var i=0;i<20;i++)Render("blink",100);
        if(output is not null)File.WriteAllText(Path.Combine(output,$"motion-{scale:F2}.json"),JsonSerializer.Serialize(rows));
    });
}

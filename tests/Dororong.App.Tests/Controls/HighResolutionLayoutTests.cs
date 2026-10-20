using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Dororong.App.Controls;
using Dororong.App.Interaction;
using Dororong.Core.Behavior;

namespace Dororong.App.Tests.Controls;

public sealed class HighResolutionLayoutTests
{
    [Theory]
    [InlineData(1)] [InlineData(1.1)] [InlineData(1.25)] [InlineData(1.35)] [InlineData(1.5)]
    public void Valid_live_image_does_not_force_layout_but_resizing_still_does(double scale)=>CheekProductTests.Sta(()=>{
        var p=new DororongPresenter{HighResolutionGrabEnabled=true,HighResolutionScale=scale,Width=144*scale,Height=144*scale};
        ((Canvas)p.Content).Width=p.Width;((Canvas)p.Content).Height=p.Height;
        var pet=new PetSnapshot(PetState.Idle,new(100,100),FacingDirection.Right,0,false,null);
        p.Render(pet,DirectInteractionSnapshot.None,TimeSpan.Zero);
        p.Measure(new(p.Width,p.Height));p.Arrange(new(0,0,p.Width,p.Height));p.UpdateLayout();
        var image=p.HighResolutionImage!;
        Assert.False(DororongPresenter.NeedsHighResolutionLayout(image));
        var flushes=p.HighResolutionLayoutFlushes;
        for(var i=0;i<20;i++)p.Render(pet,DirectInteractionSnapshot.None,TimeSpan.Zero);
        Assert.Equal(flushes,p.HighResolutionLayoutFlushes);
        var point=p.HighResolutionFaceToPresenter(new(37.5,28.25));
        Assert.NotNull(p.CreateDirectPress(image,p.TranslatePoint(point,image)));
        image.Width+=1;
        Assert.True(DororongPresenter.NeedsHighResolutionLayout(image));
        p.Measure(new(p.Width,p.Height));p.Arrange(new(0,0,p.Width,p.Height));p.UpdateLayout();
        Assert.False(DororongPresenter.NeedsHighResolutionLayout(image));
    });
}

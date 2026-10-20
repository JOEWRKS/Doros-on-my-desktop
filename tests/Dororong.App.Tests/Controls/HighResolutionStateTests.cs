using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;
using Dororong.App.Interaction;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;

namespace Dororong.App.Tests.Controls;

public sealed class HighResolutionStateTests
{
    [Theory]
    [InlineData(1)] [InlineData(1.25)] [InlineData(1.5)]
    public void Sleep_snacks_click_and_attention_never_use_legacy_frames(double scale)=>CheekProductTests.Sta(()=>{
        var output=Environment.GetEnvironmentVariable("DORORONG_R55_OUTPUT");
        if(output is not null)Directory.CreateDirectory(output);
        var p=new DororongPresenter{HighResolutionGrabEnabled=true,HighResolutionScale=scale,Width=144*scale,Height=144*scale};
        ((Canvas)p.Content).Width=p.Width;((Canvas)p.Content).Height=p.Height;
        var pet=new PetSnapshot(PetState.Idle,new(100,100),FacingDirection.Right,0,false,null);
        var index=0;
        byte[] Render(string name,DirectInteractionSnapshot? direct=null){
            p.Render(pet,direct??DirectInteractionSnapshot.None,TimeSpan.FromMilliseconds(16));
            p.Measure(new(p.Width,p.Height));p.Arrange(new(0,0,p.Width,p.Height));p.UpdateLayout();
            Assert.NotNull(p.HighResolutionImage);
            var b=(BitmapSource)p.HighResolutionImage!.Source;Assert.Equal((int)Math.Ceiling(256*scale),b.PixelWidth);
            var pixels=new byte[b.PixelWidth*b.PixelHeight*4];b.CopyPixels(pixels,b.PixelWidth*4,0);
            if(output is not null){using var s=File.Create(Path.Combine(output,$"state-{scale:F2}-{index:D3}-{name}.png"));var enc=new PngBitmapEncoder();enc.Frames.Add(BitmapFrame.Create(b));enc.Save(s);}
            index++;return pixels;
        }
        Render("idle");
        var body=p.HighResolutionSourceToPresenter(new(60,62));
        var press=p.CreateDirectPress(p.HighResolutionImage!,p.TranslatePoint(body,p.HighResolutionImage!));
        Assert.NotNull(press);Assert.Equal(DirectInteractionTarget.ClickOnly,press!.Target);
        foreach(var state in new[]{PetState.Curious,PetState.Startled,PetState.ClickReaction}){
            pet=pet with {State=state};for(var i=0;i<=10;i++){pet=pet with {Phase=i/10d};Render(state.ToString());}
        }
        pet=pet with {State=PetState.Sleep};
        for(var i=0;i<=27;i++){pet=pet with {Phase=i*.005};Render("sleep");}
        pet=pet with {State=PetState.Idle,Phase=0};for(var i=0;i<14;i++)Render("wake");
        for(var i=0;i<48;i++){
            p.SetSnack(new(SnackPhase.Chasing,FacingDirection.Right,0,0,1,i*66d/48){RunAmount=1},default);Render("run");
        }
        for(var i=0;i<=20;i++){
            p.SetSnack(new(SnackPhase.Chasing,FacingDirection.Right,0,0,2,0){JumpAge=i*.04},default);Render("jump");
        }
        var chewing=new HashSet<string>();
        for(var i=0;i<=34;i++){
            p.SetSnack(new(SnackPhase.Eating,FacingDirection.Right,0,0,i*.05,0),default);
            var pixels=Render("eat");if(i>=10&&i<=30)chewing.Add(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(pixels)));
        }
        Assert.True(chewing.Count>8,"Chew must animate, not remain a closed-mouth still.");
        for(var i=0;i<=27;i++){
            p.SetSnack(new(SnackPhase.Happy,FacingDirection.Right,0,0,i*.08,0),default);Render("happy");
        }
        for(var i=0;i<=8;i++){
            p.SetSnack(new(SnackPhase.Settling,FacingDirection.Right,0,0,i*.04,0),default);Render("settle");
        }
        p.SetSnack(default,default);Render("returned");
        for(var i=0;i<21;i++){
            p.ApplyEdgePerch(EdgePerchPhase.Attached,FacingDirection.Right,TimeSpan.FromMilliseconds(16),false);
            Render("perch",DirectInteractionSnapshot.None with {PawPull=new(false,default)});
        }
        foreach(var right in new[]{false,true}){
            var point=p.HighResolutionSourceToPresenter(new(right?42:26,78));
            var pawPress=p.CreateDirectPress(p.HighResolutionImage!,p.TranslatePoint(point,p.HighResolutionImage!));
            Assert.NotNull(pawPress);Assert.Equal(right?DirectInteractionTarget.PerchRightPaw:DirectInteractionTarget.PerchLeftPaw,pawPress!.Target);
            for(var i=0;i<=12;i++){
                var amount=Math.Sin(i*Math.PI/12);
                Render(right?"right-paw":"left-paw",DirectInteractionSnapshot.None with {PawPull=new(right,new(right?12*amount:-12*amount,12*amount),10*amount)});
            }
        }
        p.ApplyEdgePerch(EdgePerchPhase.None,FacingDirection.Right,TimeSpan.Zero,true);Render("perch-return");
    });
}

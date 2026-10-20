using System.Windows;
using System.Windows.Interop;
using Dororong.App.Controls;
using Dororong.App.Runtime;
using Dororong.Core.Geometry;

namespace Dororong.App.Tests.Controls;

public sealed class WindowMoveCostTests(Xunit.Abstractions.ITestOutputHelper output)
{
    [Fact]
    public void Subpixel_targets_keep_logical_motion_without_repeated_native_moves()=>CheekProductTests.Sta(()=>{
        var presenter=new DororongPresenter();
        var window=new Window{Content=presenter,WindowStyle=WindowStyle.None,AllowsTransparency=true,Opacity=0,ShowActivated=false,ShowInTaskbar=false};
        try{
            var viewport=new PetWindowViewport(window,presenter);window.Show();
            var source=HwndSource.FromHwnd(new WindowInteropHelper(window).Handle)!;
            var moves=0;
            nint Hook(nint h,int message,nint w,nint l,ref bool handled){if(message==0x0047)moves++;return 0;}
            source.AddHook(Hook);
            viewport.SetPosition(new(396,296));moves=0;
            for(var i=1;i<20;i++){
                var point=new PointD(396+i*.01,296+i*.01);viewport.SetPosition(point);
                Assert.Equal(point,viewport.GetPosition());
            }
            output.WriteLine($"Native position notifications for19same-pixel targets: {moves}");
            Assert.Equal(0,moves);
            viewport.SetPosition(new(397,297));Assert.True(moves>0);
            window.Left=500;Assert.Equal(new PointD(596,window.Top+96),viewport.GetPosition());
            source.RemoveHook(Hook);
        }finally{window.Close();}
    });
}

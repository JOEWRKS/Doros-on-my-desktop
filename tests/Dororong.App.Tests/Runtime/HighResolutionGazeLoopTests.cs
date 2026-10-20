using System.IO;
using System.Text.Json;
using System.Windows.Media.Imaging;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;

namespace Dororong.App.Tests.Runtime;

public partial class PetLoopPlatformTests
{
    [Theory]
    [InlineData(1)] [InlineData(1.25)] [InlineData(1.5)]
    public void High_resolution_gaze_entry_and_departure_frames(double scale)=>Controls.CheekProductTests.Sta(()=>{
        using var h=new Harness(realPresenter:true,highResolutionScale:scale,huntingPresentation:true);
        h.Native.Scene=Scene(windows:false);h.Start();for(var i=0;i<100;i++)h.Tick(20);
        var output=Environment.GetEnvironmentVariable("DORORONG_R60_OUTPUT");
        if(output is not null)Directory.CreateDirectory(output);
        var rows=new List<object>();var p=h.Presenter!;
        for(var side=0;side<2;side++)for(var i=0;i<48;i++){
            var entering=i<18;
            h.Pointer=entering?new(true,h.Position+new PointD((side==0?64:80)*scale,68*scale)):PointerSample.Unavailable;
            h.Tick(20);Assert.NotNull(p.HighResolutionImage);Assert.Equal(256*scale,p.HighResolutionImage!.Width,6);
            var file=$"loop-{scale:F2}-{side}-{i:D2}-{(entering?"enter":"return")}.png";
            if(output is not null){using var stream=File.Create(Path.Combine(output,file));var enc=new PngBitmapEncoder();enc.Frames.Add(BitmapFrame.Create((BitmapSource)p.HighResolutionImage.Source));enc.Save(stream);}
            rows.Add(new{file,scale,side,index=i,entering,frame=p.HuntingVisualFrame,state=h.Core.State.ToString()});
        }
        if(output is not null)File.WriteAllText(Path.Combine(output,$"loop-{scale:F2}.json"),JsonSerializer.Serialize(rows));
    });
}

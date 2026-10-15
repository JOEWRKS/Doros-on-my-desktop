using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Portable.Desktop;
using Dororong.Portable.Rendering;
namespace Dororong.Portable.Tests;
public class ClickSettingsTests
{
    [Fact]
    public void ClickThresholdIsStickyAndWakeUses45msFrames()
    {
        var click=new ClickPresentation();click.Begin(default,true,true);
        Assert.Equal(RasterImage.Load("dororong-sleep-tuck-squint.png").CopyPixels(),click.Render(45,out var pending).CopyPixels());
        Assert.Equal(.975,pending.M22);Assert.Equal(RasterImage.Load("dororong-sleep-crouch-squint.png").CopyPixels(),click.Render(45,out _).CopyPixels());
        Assert.Equal(HeadPullFrames.Canonical.CopyPixels(),click.Render(16,out _).CopyPixels());
        click.Advance(16,default,false);click.Advance(250,default,false);Assert.Equal(-12,click.Lift);click.Advance(250,default,false);Assert.False(click.Active);
        click.Begin(default,false,false);click.Advance(16,new(4,0),true);click.Advance(16,default,true);click.Advance(16,default,false);Assert.False(click.Active);
    }
    [Fact]
    public void SleepSettingsSurviveRestartAndPreserveUnknownAndCorruptData()
    {
        var folder=Path.Combine(Path.GetTempPath(),"dororong-settings-test-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);var path=Path.Combine(folder,"settings.json");
        try
        {
            var settings=new BehaviorSettings(path);Assert.True(settings.AutomaticSleepEnabled);settings.SetAutomaticSleepEnabled(false);Assert.False(new BehaviorSettings(path).AutomaticSleepEnabled);
            File.WriteAllText(path,"{\"version\":1,\"automaticSleepEnabled\":false,\"future\":42}");settings.SetAutomaticSleepEnabled(true);Assert.Contains("42",File.ReadAllText(path));
            File.WriteAllText(path,"{broken");Assert.Throws<IOException>(()=>settings.SetAutomaticSleepEnabled(false));Assert.Equal("{broken",File.ReadAllText(path));Assert.Single(Directory.GetFiles(folder));
        }
        finally{File.Delete(path);Directory.Delete(folder);}
    }
    [Fact]
    public void ExtremeSpreadNeverChangesHeadAndZeroIsPaddedOriginal()
    {
        var source=HeadPullFrames.Canonical.CopyPixels();var zero=ExtremeLandingFrame.Spread(source,0);var spread=ExtremeLandingFrame.Spread(source,1);
        for(var y=0;y<96;y++)Assert.Equal(source.AsSpan(y*384,384).ToArray(),zero.AsSpan(((y+32)*160+32)*4,384).ToArray());
        for(var y=0;y<99;y++)Assert.Equal(zero.AsSpan(y*640,640).ToArray(),spread.AsSpan(y*640,640).ToArray());
        Assert.NotEqual(zero,spread);
    }
}

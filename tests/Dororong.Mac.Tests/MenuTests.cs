using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Input;
using Dororong.Mac.Product;
namespace Dororong.Mac.Tests;
public class MenuTests
{
    [AvaloniaTheory]
    [InlineData(false,false,true)] [InlineData(true,false,true)] [InlineData(true,true,false)]
    public void B2MenuRendersRealStatesAndCommands(bool tray,bool snackActive,bool sleepEnabled)
    {
        var snack=0;var dismissed=0;var sleepCalls=0;
        using var panel=new PetMenuPanel(tray,snackActive,sleepEnabled,()=>snack++,()=>{},_=>{sleepCalls++;return false;},()=>{},()=>{});
        panel.Dismiss+=()=>dismissed++;
        var window=new Window{SizeToContent=SizeToContent.WidthAndHeight,Content=panel};
        try
        {
            window.Show();panel.FocusFirst();Assert.True(panel.SnackButton.IsFocused);
            Assert.Equal(288,panel.Bounds.Width);Assert.False(panel.StartupButton.IsEnabled);
            Assert.Equal(sleepEnabled,panel.SleepButton.IsChecked);
            Assert.Equal(tray?4:4,panel.Actions.Count);
            using var frame=window.CaptureRenderedFrame();Assert.NotNull(frame);frame.Save(Path.Combine(AppContext.BaseDirectory,$"menu-{tray}-{snackActive}-{sleepEnabled}.png"));
            panel.SnackButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Assert.Equal(1,snack);Assert.Equal(1,dismissed);
            panel.SleepButton.IsChecked=!sleepEnabled;panel.SleepButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal(sleepEnabled,panel.SleepButton.IsChecked);Assert.Equal(1,sleepCalls);Assert.Equal(1,dismissed);Assert.True(panel.Error.IsVisible);
            window.UpdateLayout();using var errorFrame=window.CaptureRenderedFrame();Assert.NotNull(errorFrame);errorFrame.Save(Path.Combine(AppContext.BaseDirectory,$"menu-error-{tray}.png"));
        }
        finally{window.Close();}
    }
    [AvaloniaFact]
    public void KeyboardCyclesAndActivatesAndEscapeDismisses()
    {
        var snack=0;var dismiss=0;var saved=false;
        using var panel=new PetMenuPanel(false,false,false,()=>snack++,()=>{},value=>{saved=value;return true;},()=>{},()=>{});
        panel.Dismiss+=()=>dismiss++;
        var window=new Window{SizeToContent=SizeToContent.WidthAndHeight,Content=panel};
        void Key(Key key){var physical=key switch{Avalonia.Input.Key.Up=>PhysicalKey.ArrowUp,Avalonia.Input.Key.Down=>PhysicalKey.ArrowDown,Avalonia.Input.Key.Enter=>PhysicalKey.Enter,Avalonia.Input.Key.Space=>PhysicalKey.Space,_=>PhysicalKey.Escape};window.KeyPressQwerty(physical,RawInputModifiers.None);window.KeyReleaseQwerty(physical,RawInputModifiers.None);}
        try
        {
            window.Show();panel.FocusFirst();Key(Avalonia.Input.Key.Up);Assert.True(panel.Actions[^1].IsFocused);Key(Avalonia.Input.Key.Down);Assert.True(panel.SnackButton.IsFocused);
            Key(Avalonia.Input.Key.Enter);Assert.Equal(1,snack);Key(Avalonia.Input.Key.Down);Key(Avalonia.Input.Key.Down);Assert.True(panel.SleepButton.IsFocused);
            Key(Avalonia.Input.Key.Space);Assert.True(saved);Assert.Equal(2,dismiss);Key(Avalonia.Input.Key.Escape);Assert.Equal(3,dismiss);
        }
        finally{window.Close();}
    }
    [AvaloniaFact]
    public void PopupIsSingletonAndClosesWithoutChangingPetTarget()
    {
        var target=new Button{Content="pet target",Width=144,Height=144};var window=new Window{Width=600,Height=500,Content=target};
        using var host=new PetMenuHost();var created=0;var notifications=new List<bool>();host.OpenChanged+=notifications.Add;
        PetMenuPanel Create(){created++;return new(false,false,true,()=>{},()=>{},_=>true,()=>{},()=>{});}
        try
        {
            window.Show();var bounds=target.Bounds;host.Open(target,Create);Assert.True(host.IsOpen);host.Open(target,Create);Assert.Equal(1,created);Assert.Equal(bounds,target.Bounds);
            window.MouseDown(new(590,490),MouseButton.Left,RawInputModifiers.None);window.MouseUp(new(590,490),MouseButton.Left,RawInputModifiers.None);
            Assert.False(host.IsOpen);Assert.Equal(new[]{true,false},notifications);host.Open(target,Create);Assert.Equal(2,created);host.Dispose();Assert.False(host.IsOpen);
        }
        finally{window.Close();}
    }
}

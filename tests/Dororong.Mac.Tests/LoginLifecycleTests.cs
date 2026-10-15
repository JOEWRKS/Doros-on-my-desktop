using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Dororong.Mac.Native;
using Dororong.Mac.Product;
namespace Dororong.Mac.Tests;
public class LoginLifecycleTests
{
    private sealed class Service:ILoginService
    {
        internal LoginState State=LoginState.NotRegistered;internal bool Approval,Fail;internal int Registers,Unregisters,Settings;
        public LoginSnapshot Read()=>new(State);
        public void Register(){Registers++;if(Fail)throw new IOException();State=Approval?LoginState.RequiresApproval:LoginState.Enabled;}
        public void Unregister(){Unregisters++;if(Fail)throw new IOException();State=LoginState.NotRegistered;}
        public void OpenSettings()=>Settings++;
    }
    [Fact]
    public void StartupReadsOsStateAndDoesNotTreatApprovalAsEnabled()
    {
        var service=new Service();var startup=new LoginStartup(service);Assert.False(startup.Read().Requested);Assert.Equal(0,service.Registers);
        Assert.Equal(LoginState.Enabled,startup.Set(true).State);startup.Set(true);Assert.Equal(1,service.Registers);
        startup.Set(false);Assert.Equal(1,service.Unregisters);service.Approval=true;
        Assert.Equal(LoginState.RequiresApproval,startup.Set(true).State);Assert.Equal(1,service.Settings);startup.Set(false);Assert.Equal(LoginState.NotRegistered,service.State);
        service.Fail=true;Assert.Throws<IOException>(()=>startup.Set(true));Assert.Equal(LoginState.NotRegistered,startup.Read().State);
        service.State=LoginState.Unavailable;Assert.Throws<InvalidOperationException>(()=>startup.Set(true));
    }
    [Fact]
    public void NativeCallsAreNotAttemptedOnWindows()
    {
        if(OperatingSystem.IsMacOS())return;
        Assert.False(MacLoginService.IsBundledApp());Assert.Equal(LoginState.Unavailable,new MacLoginService().Read().State);
        Assert.Throws<PlatformNotSupportedException>(()=>new StatusItem([],()=>{},_=>{}));
    }
    [AvaloniaTheory]
    [InlineData(LoginState.NotRegistered)] [InlineData(LoginState.Enabled)] [InlineData(LoginState.RequiresApproval)] [InlineData(LoginState.NotFound)]
    internal void MenuShowsLiveStartupAndFailureWithoutLosingOtherActions(LoginState initial)
    {
        var service=new Service{State=initial};var startup=new LoginStartup(service);var dismissed=0;
        using var panel=new PetMenuPanel(true,false,true,()=>{},()=>{},_=>true,()=>{},()=>{},startup.Read,startup.Set);panel.Dismiss+=()=>dismissed++;
        var w=new Window{Content=panel,SizeToContent=SizeToContent.WidthAndHeight};
        try
        {
            w.Show();Assert.True(panel.StartupButton.IsEnabled);Assert.Equal(5,panel.Actions.Count);
            var toggle=Assert.IsType<ToggleButton>(panel.StartupButton);Assert.Equal(startup.Read().Requested,toggle.IsChecked);
            using var frame=w.CaptureRenderedFrame();Assert.NotNull(frame);frame.Save(Path.Combine(AppContext.BaseDirectory,$"login-{initial}.png"));
            service.Fail=true;toggle.IsChecked=!startup.Read().Requested;toggle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Assert.Equal(0,dismissed);Assert.True(panel.Error.IsVisible);Assert.Equal(startup.Read().Requested,toggle.IsChecked);
            w.UpdateLayout();using var error=w.CaptureRenderedFrame();Assert.NotNull(error);error.Save(Path.Combine(AppContext.BaseDirectory,$"login-error-{initial}.png"));
        }
        finally{w.Close();}
    }
    [AvaloniaFact]
    public void UnavailableAfterFailureDisablesAndRemovesStartupFromKeyboardCycle()
    {
        var current=new LoginSnapshot(LoginState.NotRegistered);
        using var panel=new PetMenuPanel(true,false,true,()=>{},()=>{},_=>true,()=>{},()=>{},()=>current,_=>{current=new(LoginState.Unavailable,"OS 상태 확인 실패");throw new IOException();});
        var w=new Window{Content=panel,SizeToContent=SizeToContent.WidthAndHeight};
        try
        {
            w.Show();panel.StartupButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Assert.False(panel.StartupButton.IsEnabled);Assert.DoesNotContain(panel.StartupButton,panel.Actions);
            Assert.Contains("OS 상태 확인 실패",Avalonia.Automation.AutomationProperties.GetName(panel.StartupButton));Assert.True(panel.SleepButton.IsFocused);
        }
        finally{w.Close();}
    }
    [AvaloniaFact]
    public void GrowingErrorRepositionsStatusMenuAfterLayout()
    {
        using var host=new StatusMenuHost();PetMenuPanel? panel=null;Window? window=null;var heights=new List<double>();
        host.Open(()=>panel=new(true,false,true,()=>{},()=>{},_=>true,()=>{},()=>{}),w=>{window=w;heights.Add(w.Bounds.Height);});
        Assert.NotNull(panel);Assert.NotNull(window);using var beforeFrame=window.CaptureRenderedFrame();var before=window.Bounds.Height;
        panel.Error.Text="자동 실행 변경 실패. 시스템 설정의 로그인 항목과 앱 경로를 확인해 주세요.";panel.Error.IsVisible=true;window.UpdateLayout();
        using var afterFrame=window.CaptureRenderedFrame();Assert.True(window.Bounds.Height>before,$"height {before} -> {window.Bounds.Height}; open={host.IsOpen}");Assert.True(heights.Count>=2,$"position calls={heights.Count}");Assert.Equal(window.Bounds.Height,heights[^1]);
    }
    [AvaloniaFact]
    public void StatusHostReusesMenuAndReleasesItOnExit()
    {
        using var host=new StatusMenuHost();var created=0;var positioned=0;var state=new List<bool>();host.OpenChanged+=state.Add;
        PetMenuPanel Create(){created++;return new(true,false,true,()=>{},()=>{},_=>true,()=>{},()=>{});}
        host.Open(Create,w=>{positioned++;Assert.Equal(288,w.Bounds.Width);});host.Open(Create,_=>positioned++);
        Assert.Equal(1,created);Assert.Equal(1,positioned);Assert.True(host.IsOpen);host.Dispose();Assert.False(host.IsOpen);Assert.Equal(new[]{true,false},state);
    }
    [Fact]
    public void StatusPositionUsesPointCoordinatesAndClampsNegativeScreens()
    {
        var result=StatusMenuHost.Clamp(new(-1,0),new(288,390),new(-1920,0,1920,1080));Assert.Equal(new Point(-288,4),result);
        Assert.Equal(new Point(-1920,690),StatusMenuHost.Clamp(new(-1919,1079),new(288,390),new(-1920,0,1920,1080)));
    }
}

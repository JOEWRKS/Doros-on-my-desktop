using System.Runtime.InteropServices;
using Dororong.Mac.Product;
namespace Dororong.Mac.Native;

internal sealed class MacLoginService:ILoginService
{
    internal const string BundleIdentifier="com.joewrks.dororong";
    // Retain framework for process lifetime; Objective-C class references depend on it.
    private static nint framework;
    internal static bool IsBundledApp()
    {
        if(!OperatingSystem.IsMacOS())return false;
        var bundle=Cocoa.Get(Cocoa.Class("NSBundle"),"mainBundle");var path=Cocoa.Text(Cocoa.Get(bundle,"bundlePath"));
        var executable=Cocoa.Text(Cocoa.Get(bundle,"executablePath"));var identity=Cocoa.Text(Cocoa.Get(bundle,"bundleIdentifier"));
        return path is not null&&path.EndsWith(".app",StringComparison.OrdinalIgnoreCase)&&identity==BundleIdentifier&&
            executable is not null&&Path.GetFullPath(executable)==Path.GetFullPath(Environment.ProcessPath??"")&&
            executable.StartsWith(path+"/Contents/MacOS/",StringComparison.Ordinal);
    }
    public LoginSnapshot Read()
    {
        if(!OperatingSystem.IsMacOSVersionAtLeast(13))return new(LoginState.Unavailable,"macOS 13 이상 필요");
        if(!IsBundledApp())return new(LoginState.Unavailable,".app 설치 후 사용 가능");
        var raw=(long)Cocoa.Get(Service(),"status");return raw is >=0 and <=3?new((LoginState)raw):new(LoginState.Unavailable,"OS 상태 확인 실패");
    }
    private static nint Service()
    {
        Cocoa.RequireMainThread();
        if(framework==0)framework=NativeLibrary.Load("/System/Library/Frameworks/ServiceManagement.framework/ServiceManagement");
        var cls=Cocoa.Class("SMAppService");if(cls==0)throw new PlatformNotSupportedException("SMAppService unavailable.");
        var service=Cocoa.Get(cls,"mainAppService");return service!=0?service:throw new InvalidOperationException("Main app service unavailable.");
    }
    public void Register()=>Change("registerAndReturnError:");
    public void Unregister()=>Change("unregisterAndReturnError:");
    private void Change(string selector)
    {
        if(!Read().Available)throw new InvalidOperationException("Use an installed .app on macOS 13 or newer.");
        if(!Cocoa.Error(Service(),Cocoa.Sel(selector),out _))throw new IOException("macOS가 자동 실행 변경을 승인하지 않았습니다. 시스템 설정의 로그인 항목을 확인해 주세요.");
    }
    public void OpenSettings(){_=Service();Cocoa.Get(Cocoa.Class("SMAppService"),"openSystemSettingsLoginItems");}
}

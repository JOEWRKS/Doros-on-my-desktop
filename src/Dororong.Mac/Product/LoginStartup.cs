namespace Dororong.Mac.Product;

internal enum LoginState{Unavailable=-1,NotRegistered=0,Enabled=1,RequiresApproval=2,NotFound=3}
internal readonly record struct LoginSnapshot(LoginState State,string? Reason=null)
{
    internal bool Requested=>State is LoginState.Enabled or LoginState.RequiresApproval;
    internal bool Available=>State!=LoginState.Unavailable;
    internal string Label=>State switch{LoginState.Enabled=>"켜짐",LoginState.NotRegistered=>"꺼짐",LoginState.RequiresApproval=>"시스템 승인 필요",LoginState.NotFound=>"앱 경로 확인 필요",_=>Reason??"사용할 수 없음"};
}
internal interface ILoginService
{
    LoginSnapshot Read();void Register();void Unregister();void OpenSettings();
}
internal sealed class LoginStartup(ILoginService service)
{
    internal LoginSnapshot Read()=>service.Read();
    internal LoginSnapshot Set(bool enabled)
    {
        var before=Read();if(!before.Available)throw new InvalidOperationException(before.Reason);
        if(enabled&&before.State==LoginState.Enabled||!enabled&&before.State==LoginState.NotRegistered)return before;
        if(enabled)service.Register();else service.Unregister();
        var after=Read();
        if(enabled&&after.State==LoginState.RequiresApproval){service.OpenSettings();return after;}
        if(after.State!=(enabled?LoginState.Enabled:LoginState.NotRegistered))throw new IOException("자동 실행 등록 상태를 확인하지 못했습니다. 메뉴를 다시 열어 확인해 주세요.");
        return after;
    }
}

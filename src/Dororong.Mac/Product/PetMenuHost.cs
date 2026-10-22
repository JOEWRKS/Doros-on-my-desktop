using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
namespace Dororong.Mac.Product;

// A single light-dismiss popup. Repeated requests keep the current menu open
// rather than close/reopen it; OS menu-bar anchoring is a separate native port.
internal sealed class PetMenuHost:IDisposable
{
    private readonly Popup popup=new(){IsLightDismissEnabled=true,Placement=PlacementMode.Pointer};
    private PetMenuPanel? panel;
    internal bool IsOpen=>popup.IsOpen;
    internal event Action<bool>? OpenChanged;
    internal PetMenuHost()
    {
        popup.Opened+=(_,_)=>{var opened=panel;OpenChanged?.Invoke(true);Dispatcher.UIThread.Post(()=>{if(popup.IsOpen&&ReferenceEquals(panel,opened))opened?.FocusFirst();});};
        popup.Closed+=(_,_)=>{ReleaseContent();OpenChanged?.Invoke(false);};
    }
    internal void Open(Control target,Func<PetMenuPanel> create)
    {
        if(popup.IsOpen)return;
        panel=create();panel.Dismiss+=Close;
        try
        {
            // PlacementTarget positions a popup but does not supply its logical
            // theme/resource parent. Native controls such as Slider need both.
            ((ISetLogicalParent)popup).SetParent(target);
            popup.PlacementTarget=target;popup.Child=panel;popup.IsOpen=true;
        }
        catch{Close();ReleaseContent();throw;}
    }
    internal void Close()=>popup.IsOpen=false;
    private void ReleaseContent()
    {
        var released=panel;panel=null;
        if(released is not null)released.Dismiss-=Close;
        popup.Child=null;popup.PlacementTarget=null;
        ((ISetLogicalParent)popup).SetParent(null);
        released?.Dispose();
    }
    public void Dispose(){Close();ReleaseContent();}
}

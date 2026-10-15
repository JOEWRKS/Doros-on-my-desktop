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
        popup.Opened+=(_,_)=>{OpenChanged?.Invoke(true);Dispatcher.UIThread.Post(()=>panel?.FocusFirst());};
        popup.Closed+=(_,_)=>{OpenChanged?.Invoke(false);popup.Child=null;panel?.Dispose();panel=null;};
    }
    internal void Open(Control target,Func<PetMenuPanel> create)
    {
        if(popup.IsOpen)return;
        panel=create();panel.Dismiss+=Close;popup.PlacementTarget=target;popup.Child=panel;popup.IsOpen=true;
    }
    internal void Close()=>popup.IsOpen=false;
    public void Dispose(){Close();popup.Child=null;panel?.Dispose();panel=null;popup.PlacementTarget=null;}
}

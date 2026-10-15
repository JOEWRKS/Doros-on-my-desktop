using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
namespace Dororong.Mac.Product;

// Status-item clicks occur outside Avalonia's pointer stream. A dedicated menu
// window is positioned through Cocoa in screen points, not stale Mouse placement.
internal sealed class StatusMenuHost:IDisposable
{
    private Window? window;private PetMenuPanel? panel;private long deactivatedAt;
    internal bool IsOpen=>window is not null;
    internal event Action<bool>? OpenChanged;
    internal event Action<Exception>? Failed;
    internal void Open(Func<PetMenuPanel> create,Action<Window> position,Action? activateNative=null)
    {
        if(window is not null)return;
        // Cocoa may deactivate the old menu before delivering the same icon click.
        // Do not recreate it during that mouse-up dispatch (the old flicker bug).
        if(deactivatedAt!=0&&Stopwatch.GetElapsedTime(deactivatedAt).TotalMilliseconds<150)return;
        panel=create();panel.Dismiss+=Close;
        var current=new Window{Content=panel,Width=288,SizeToContent=SizeToContent.Height,CanResize=false,ShowInTaskbar=false,ShowActivated=false,
            SystemDecorations=SystemDecorations.None,Topmost=true,Background=Brushes.Transparent,TransparencyLevelHint=[WindowTransparencyLevel.Transparent]};
        window=current;
        current.Deactivated+=(_,_)=>{deactivatedAt=Stopwatch.GetTimestamp();Close();};
        current.Closed+=(_,_)=>{if(window!=current)return;window=null;current.Content=null;panel?.Dispose();panel=null;OpenChanged?.Invoke(false);};
        var shown=false;
        current.SizeChanged+=(_,_)=>{if(!shown||window!=current)return;try{position(current);}catch(Exception ex){Close();Failed?.Invoke(ex);}};
        try{current.Show();current.UpdateLayout();position(current);shown=true;activateNative?.Invoke();current.Activate();if(window!=current)return;panel?.FocusFirst();OpenChanged?.Invoke(true);}
        catch{Close();throw;}
    }
    internal void Close()=>window?.Close();
    internal static Point Clamp(Point pointer,Size size,Rect monitor)=>new(
        Math.Clamp(pointer.X-size.Width/2,monitor.Left,Math.Max(monitor.Left,monitor.Right-size.Width)),
        Math.Clamp(pointer.Y+4,monitor.Top,Math.Max(monitor.Top,monitor.Bottom-size.Height)));
    public void Dispose()=>Close();
}

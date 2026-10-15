using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using PathShape=Avalonia.Controls.Shapes.Path;

namespace Dororong.Mac.Product;

// B2 port: shared component for character and menu-bar surfaces. Native host
// lifecycle is verified separately; this component never transforms the pet.
internal sealed class PetMenuPanel:UserControl,IDisposable
{
    private static readonly FontFamily Font=new("avares://Dororong.Mac/Assets/Fonts#Pretendard");
    private readonly Bitmap head=Load("head"),jewel=Load("jewel");
    private readonly List<Button> actions=[];
    internal IReadOnlyList<Button> Actions=>actions;
    internal Button SnackButton {get;}
    internal ToggleButton SleepButton {get;}
    internal Button StartupButton {get;}
    internal TextBlock Error {get;}=new(){IsVisible=false,TextWrapping=TextWrapping.Wrap,FontSize=11,Foreground=new SolidColorBrush(Color.Parse("#8D2338")),Margin=new(11,5)};
    internal event Action? Dismiss;
    internal PetMenuPanel(bool tray,bool snackActive,bool sleepEnabled,Action snack,Action sit,Func<bool,bool> sleep,Action logs,Action exit,Func<LoginSnapshot>? startupRead=null,Func<bool,LoginSnapshot>? startupSet=null)
    {
        FontFamily=Font;Width=288;
        var layout=new StackPanel();
        var header=new Grid{Height=54,ColumnDefinitions=new("*,30"),ClipToBounds=true};
        var facets=new PathShape{Data=Geometry.Parse("M100,0 L170,54 240,0 Z M185,0 L278,54 278,15 250,0 Z"),Fill=Brush("#0BFFFFFF"),IsHitTestVisible=false};Grid.SetColumnSpan(facets,2);header.Children.Add(facets);
        var title=new StackPanel{Orientation=Orientation.Horizontal,Spacing=7,VerticalAlignment=VerticalAlignment.Center,Margin=new(16,0,0,3)};
        title.Children.Add(new Image{Source=head,Width=26,Height=26});title.Children.Add(Text("도로롱이 내 컴에?!",19,FontWeight.Bold,"#FFFFFF"));header.Children.Add(title);
        var close=MakeButton(new PathShape{Data=Geometry.Parse("M0,0 L10,10 M10,0 L0,10"),Stroke=Brushes.White,StrokeThickness=1.2,Width=11,Height=11},32,Brushes.Transparent);
        close.Width=30;close.Margin=new(0,0,6,3);close.Focusable=false;AutomationProperties.SetName(close,"메뉴 닫기");close.Click+=(_,_)=>Dismiss?.Invoke();Grid.SetColumn(close,1);header.Children.Add(close);layout.Children.Add(header);
        var rows=new StackPanel();
        var snackGrid=new Grid{ColumnDefinitions=new("36,*,18"),Margin=new(11,0)};
        snackGrid.Children.Add(new Image{Source=jewel,Width=36,Height=36});
        var snackLabel=Text(snackActive?"간식 취소":"간식 주기",16,FontWeight.Bold,"#123D52");snackLabel.Margin=new(10,0);Grid.SetColumn(snackLabel,1);snackGrid.Children.Add(snackLabel);
        Control arrow=snackActive?new Border{Width=10,Height=10,Background=Brush("#557F92")} : Icon("M2,9 L15,9 M10,4 L15,9 10,14");Grid.SetColumn(arrow,2);snackGrid.Children.Add(arrow);
        var snackLayers=new Grid();snackLayers.Children.Add(new Border{Margin=new(3),BorderThickness=new(1),BorderBrush=Brush("#50FFFFFF"),CornerRadius=new(2),IsHitTestVisible=false});snackLayers.Children.Add(new PathShape{Data=Geometry.Parse("M0,30 L30,0 60,30 30,60 Z"),Fill=Brush("#12FFFFFF"),Width=60,HorizontalAlignment=HorizontalAlignment.Right,IsHitTestVisible=false});snackLayers.Children.Add(snackGrid);
        SnackButton=MakeButton(snackLayers,62,Gradient(snackActive?"#F5FAFC":"#45CAF4",snackActive?"#E3F0F6":"#63D8F7"),snackActive?"#99B9CA":"#209BC7");
        SnackButton.Margin=new(0,0,0,7);AutomationProperties.SetName(SnackButton,snackActive?"간식 취소":"간식 주기");Wire(SnackButton,snack);rows.Children.Add(SnackButton);
        if(!tray){rows.Children.Add(Row("앉아","M5,4 L5,14 19,14 M5,20 L5,14 M19,14 L19,20 M9,9 L19,9 19,14",sit));rows.Children.Add(Separator());}
        SleepButton=new ToggleButton{IsChecked=sleepEnabled};
        var sleepRow=SettingContent("자동 눕기",sleepEnabled?"켜짐":"꺼짐",sleepEnabled);
        ConfigureButton(SleepButton,sleepRow,54,Brushes.Transparent,null);AutomationProperties.SetName(SleepButton,"자동 눕기");actions.Add(SleepButton);
        SleepButton.Click+=(_,_)=>
        {
            // ToggleButton changes before Click; restore on persistence failure.
            var requested=SleepButton.IsChecked==true;
            if(!sleep(requested)){SleepButton.IsChecked=!requested;Error.Text="설정을 저장하지 못했습니다. 폴더 권한을 확인한 뒤 다시 시도해 주세요.";Error.IsVisible=true;return;}
            Dismiss?.Invoke();
        };
        rows.Children.Add(SleepButton);
        var login=startupRead?.Invoke()??new LoginSnapshot(LoginState.Unavailable,"연결되지 않음");
        var startup=new ToggleButton{IsChecked=login.Requested};StartupButton=startup;
        ConfigureButton(startup,SettingContent("macOS 로그인 시 실행",login.Label,login.Requested),54,Brushes.Transparent,null);
        void RefreshLogin()
        {
            startup.IsChecked=login.Requested;startup.Content=SettingContent("macOS 로그인 시 실행",login.Label,login.Requested);
            startup.IsEnabled=login.Available&&startupSet is not null;AutomationProperties.SetName(startup,"macOS 로그인 시 실행 — "+login.Label);
            ToolTip.SetTip(startup,login.State==LoginState.RequiresApproval?"시스템 설정 > 일반 > 로그인 항목에서 허용해 주세요. 다시 누르면 등록을 해제합니다.":login.Reason??"현재 사용자의 로그인 시 실행 여부");
            actions.Remove(startup);if(startup.IsEnabled)actions.Insert(actions.IndexOf(SleepButton)+1,startup);
        }
        RefreshLogin();rows.Children.Add(startup);
        startup.Click+=(_,_)=>
        {
            try{_=startupSet!(startup.IsChecked==true);Dismiss?.Invoke();}
            catch(Exception ex) when(ex is IOException or InvalidOperationException or UnauthorizedAccessException)
            {
                try{login=startupRead!();}catch{login=new(LoginState.Unavailable,"OS 상태 확인 실패");}
                RefreshLogin();if(!startup.IsEnabled)SleepButton.Focus(NavigationMethod.Directional);
                Error.Text="자동 실행을 변경하지 못했습니다. 시스템 설정의 로그인 항목과 앱 경로를 확인해 주세요.";Error.IsVisible=true;
            }
        };
        if(tray){rows.Children.Add(Separator());rows.Children.Add(Row("로그 폴더 열기","M3,7 L3,4 9,4 12,7 21,7 21,20 3,20 Z M3,10 L21,10",logs));}
        rows.Children.Add(Row("종료","M12,2 L12,11 M7,5 A9,9 0 1 0 17,5",exit));
        rows.Children.Add(Error);AutomationProperties.SetName(Error,"설정 저장 오류");
        var inset=new Grid();inset.Children.Add(new PathShape{Data=Geometry.Parse("M140,80 L210,150 140,220 70,150 Z"),Stroke=Brush("#0CB5C7D4"),StrokeThickness=7,IsHitTestVisible=false});inset.Children.Add(rows);
        layout.Children.Add(new Border{Background=Brush("#FCFDFE"),BorderBrush=Brush("#DCE4EA"),BorderThickness=new(1,1,1,2),CornerRadius=new(5),Padding=new(9,11,9,6),Child=inset});
        if(tray)
        {
            var footer=new Grid{ColumnDefinitions=new("*,Auto"),Margin=new(11,10,11,5)};
            footer.Children.Add(Text("DORO'S ON MY DESKTOP?!",9,FontWeight.Normal,"#566B7A"));var version=Text("v0.1.3",11,FontWeight.Normal,"#536C7E");Grid.SetColumn(version,1);footer.Children.Add(version);layout.Children.Add(footer);
        }
        Content=new Border{BorderBrush=Brush("#7894AA"),BorderThickness=new(1),CornerRadius=new(6),Padding=new(4),Background=new LinearGradientBrush{StartPoint=new(0,0,RelativeUnit.Relative),EndPoint=new(0,1,RelativeUnit.Relative),GradientStops=[new(Color.Parse("#248ED3"),0),new(Color.Parse("#39A4DF"),.26),new(Color.Parse("#E8EEF2"),.27),new(Color.Parse("#F0F3F5"),1)]},Child=layout};
        KeyDown+=(_,e)=>
        {
            if(e.Key==Key.Escape){Dismiss?.Invoke();e.Handled=true;}
            else if(e.Key is Key.Down or Key.Up)
            {var index=actions.FindIndex(b=>b.IsFocused);var next=(index+(e.Key==Key.Down?1:-1)+actions.Count)%actions.Count;actions[next].Focus(NavigationMethod.Directional);e.Handled=true;}
        };
    }
    internal void FocusFirst()=>SnackButton.Focus(NavigationMethod.Directional);
    private void Wire(Button b,Action action){actions.Add(b);b.Click+=(_,_)=>{Dismiss?.Invoke();action();};}
    private Button Row(string title,string path,Action action)
    {
        var row=new Grid{ColumnDefinitions=new("18,*"),Margin=new(11,0)};row.Children.Add(Icon(path));var label=Text(title,14,FontWeight.SemiBold,"#354049");label.Margin=new(10,0,0,0);Grid.SetColumn(label,1);row.Children.Add(label);
        var b=MakeButton(row,44,Brushes.Transparent);AutomationProperties.SetName(b,title);Wire(b,action);return b;
    }
    private static Control SettingContent(string title,string state,bool enabled)
    {
        var row=new Grid{ColumnDefinitions=new("18,*,42"),Margin=new(11,0)};
        row.Children.Add(Icon(title=="자동 눕기"?"M16,3 A9,9 0 1 0 21,16 A10,10 0 0 1 16,3 Z":"M3,4 L21,4 21,17 3,17 Z M8,21 L16,21 M12,17 L12,21 M10,11 L15,11 M13,9 L15,11 13,13"));
        var labels=new StackPanel{VerticalAlignment=VerticalAlignment.Center,Spacing=3,Margin=new(10,0,4,0)};labels.Children.Add(Text(title,12,FontWeight.SemiBold,"#354049"));labels.Children.Add(Text(state,11,FontWeight.Normal,"#667785"));Grid.SetColumn(labels,1);row.Children.Add(labels);
        var indicator=new Border{Width=42,Height=23,CornerRadius=new(3),BorderThickness=new(1),BorderBrush=Brush(enabled?"#267D9E":"#85949E"),Background=Brush(enabled?"#19A9DB":"#8999A4"),Padding=new(2),Child=new Border{Width=16,Height=16,CornerRadius=new(2),Background=Brushes.White,HorizontalAlignment=enabled?HorizontalAlignment.Right:HorizontalAlignment.Left}};
        Grid.SetColumn(indicator,2);row.Children.Add(indicator);return row;
    }
    private static TextBlock Text(string s,double size,FontWeight weight,string color)=>new(){Text=s,FontSize=size,FontWeight=weight,Foreground=Brush(color),VerticalAlignment=VerticalAlignment.Center};
    private static PathShape Icon(string path)=>new(){Data=Geometry.Parse(path),Stroke=Brush("#526677"),StrokeThickness=1.6,Width=18,Height=18,Stretch=Stretch.Uniform,VerticalAlignment=VerticalAlignment.Center};
    private static Control Separator()=>new Border{Height=11,Padding=new(4,5),Child=new Border{Background=Brush("#D7E1E8"),Height=1}};
    private static Button MakeButton(Control content,double height,IBrush background,string? outline=null){var b=new Button();ConfigureButton(b,content,height,background,outline);return b;}
    private static void ConfigureButton(Button b,Control content,double height,IBrush background,string? outline)
    {
        b.Theme=new Avalonia.Styling.ControlTheme(b.GetType());b.FocusAdorner=null;b.Content=content;b.Height=height;b.Padding=new(0);b.HorizontalAlignment=HorizontalAlignment.Stretch;b.HorizontalContentAlignment=HorizontalAlignment.Stretch;b.VerticalContentAlignment=VerticalAlignment.Stretch;
        b.Template=new FuncControlTemplate<Button>((button,_)=>
        {
            var presenter=new Avalonia.Controls.Presenters.ContentPresenter{Name="PART_ContentPresenter",Content=button.Content,HorizontalContentAlignment=HorizontalAlignment.Stretch,VerticalContentAlignment=VerticalAlignment.Stretch};
            presenter.Bind(Avalonia.Controls.Presenters.ContentPresenter.ContentProperty,button.GetObservable(ContentControl.ContentProperty));
            _.Register("PART_ContentPresenter",presenter);
            var face=new Border{Child=presenter,CornerRadius=new(4),Background=background,BorderBrush=outline is null?Brushes.Transparent:Brush(outline),BorderThickness=outline is null?new(2):new(1,1,1,2)};
            void Refresh(){face.Background=button.IsPressed?Brush("#D3E7F4"):button.IsPointerOver&&outline is null?Brush("#E8F2F8"):background;face.BorderBrush=button.IsFocused?Brush("#055BA6"):outline is null?Brushes.Transparent:Brush(outline);face.Opacity=button.IsEnabled?1:.65;}
            button.PropertyChanged+=(_,e)=>{if(e.Property==IsPointerOverProperty||e.Property==IsFocusedProperty||e.Property==IsEnabledProperty||e.Property==Button.IsPressedProperty)Refresh();};Refresh();return face;
        });
    }
    private static SolidColorBrush Brush(string s)=>new(Color.Parse(s));
    private static LinearGradientBrush Gradient(string top,string bottom)=>new(){StartPoint=new(0,0,RelativeUnit.Relative),EndPoint=new(0,1,RelativeUnit.Relative),GradientStops=[new(Color.Parse(top),0),new(Color.Parse(bottom),1)]};
    private static Bitmap Load(string key){using var stream=typeof(PetMenuPanel).Assembly.GetManifestResourceStream("Dororong."+key)!;return new(stream);}
    public void Dispose(){head.Dispose();jewel.Dispose();}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.App;
using Dororong.App.Controls;
using Dororong.App.Interaction;
using Dororong.App.Product;
using Dororong.Core.Behavior;

internal static class Program
{
    [STAThread] static void Main(string[] args)
    {
        new Application { ShutdownMode=ShutdownMode.OnExplicitShutdown };
        if (args.Contains("--graphics")) { GraphicsReplay.Run(Path.GetFullPath(args[0])); return; }
        if (args.Contains("--viewport")) { ViewportProof.Run(Path.GetFullPath(args[0])); return; }
        if (args.Contains("--native")) { NativeResize.Run(Path.GetFullPath(args[0])); return; }
        if (args.Contains("--stress")) { Stress.Run(Path.GetFullPath(args[0])); return; }
        var output = Path.GetFullPath(args[0]); Directory.CreateDirectory(output);
        var evidence = new List<object>();
        // Render the shipped menu construction paths; no registry writes or
        // singleton app launch, and no synthetic desktop mouse input.
        var owner = new MainWindow();
        var presenter = (DororongPresenter)owner.FindName("Presenter");
        var character = (PetContextMenu)((Canvas)presenter.FindName("BodyGroup")).ContextMenu;
        using var forms = new System.Windows.Forms.ContextMenuStrip();
        foreach (var (text, tag) in new[] { ("간식 주기","snack"), ("자동 눕기","sleep"), ("Windows 로그인 시 실행","startup"), ("로그 폴더 열기","logs"), ("종료","exit") })
            forms.Items.Add(new System.Windows.Forms.ToolStripMenuItem(text) { Tag = tag });
        using var bridge = new StyledTrayMenuBridge(forms, owner, () => { });
        var tray = bridge.CreateMenu();
        foreach (var (name, menu) in new[] { ("character", character), ("tray", tray) })
        {
            var actual = menu.Items.OfType<PetSizeMenuItem>().Single();
            var at = menu.Items.IndexOf(actual);
            var percent = 125; var saves = 0;
            var slider = new PetSizeMenuItem(() => percent, () => true, n => { percent = n; return true; }, () => saves++);
            menu.Items.RemoveAt(at); menu.Items.Insert(at, slider);
            foreach (var dpi in new[] { 96, 144, 192 })
            {
                menu.Measure(new Size(800,1200)); menu.Arrange(new Rect(menu.DesiredSize)); menu.UpdateLayout();
                Save($"{name}-{dpi}", menu, dpi);
                evidence.Add(new { target=name, dpi, width=menu.ActualWidth, sizeRow=slider.ActualHeight, sliderWidth=slider.Slider.ActualWidth });
            }
            var source = new HwndSource(new HwndSourceParameters("Size slider key route") { Width = 1, Height = 1, WindowStyle = 0 });
            try
            {
                // Routed key-event exercise only; not a claim of OS-input automation.
                foreach (var (key, expected) in new[] { (Key.Home,100), (Key.Right,101), (Key.PageUp,111), (Key.End,150), (Key.Left,149), (Key.PageDown,139) })
                {
                    slider.Slider.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, key) { RoutedEvent = Keyboard.KeyDownEvent });
                    slider.Flush();
                    if (percent != expected) throw new Exception($"{name} {key}: {percent} != {expected}");
                    evidence.Add(new { target=name, key=key.ToString(), percent });
                }
                slider.Finish(); if (saves != 1) throw new Exception("save count");
            }
            finally { source.Dispose(); }
            menu.Items.RemoveAt(at); menu.Items.Insert(at, actual);
            menu.Measure(new Size(800,1200)); menu.Arrange(new Rect(menu.DesiredSize)); menu.UpdateLayout();
            Save($"{name}-busy",menu,96);
        }
        var sheet = new DrawingVisual();
        using (var dc=sheet.RenderOpen())
        {
            dc.DrawRectangle(Brushes.White,null,new Rect(0,0,1250,420));
            var index=0;
            foreach (var percent in new[] {100,110,125,140,150})
            {
                var p = new DororongPresenter(); p.SetArtSize(150);
                var pet = new PetSnapshot(PetState.Idle,new(0,0),FacingDirection.Right,0,false,null);
                p.Render(pet,DirectInteractionSnapshot.None,TimeSpan.Zero); Layout(p);
                p.SetArtSize(percent); p.Render(pet,DirectInteractionSnapshot.None,TimeSpan.Zero); Layout(p);
                var b=(BitmapSource)p.HighResolutionImage!.Source;
                var fresh=new DororongPresenter(); fresh.SetArtSize(percent);fresh.Render(pet,DirectInteractionSnapshot.None,TimeSpan.Zero);Layout(fresh);
                var reference=(BitmapSource)fresh.HighResolutionImage!.Source;
                if(Hash(b)!=Hash(reference))throw new Exception($"resized/fresh pixel mismatch {percent}");
                SaveBitmap($"pet-{percent}",b);
                dc.DrawImage(b,new Rect(index++*250,20,b.PixelWidth,b.PixelHeight));
                evidence.Add(new { percent, pixels=b.PixelWidth, pixelHash=Hash(b), sameAsFresh=true });
            }
        }
        var shot=new RenderTargetBitmap(1250,420,96,96,PixelFormats.Pbgra32);shot.Render(sheet);SaveBitmap("sizes",shot);
        owner.Close();
        File.WriteAllText(Path.Combine(output,"evidence.json"),JsonSerializer.Serialize(evidence,new JsonSerializerOptions{WriteIndented=true}));
        Console.WriteLine($"Saved {evidence.Count} observations to {output}");
        void Save(string name,FrameworkElement element,int dpi)
        {
            var b=new RenderTargetBitmap((int)Math.Ceiling(element.ActualWidth*dpi/96),(int)Math.Ceiling(element.ActualHeight*dpi/96),dpi,dpi,PixelFormats.Pbgra32);
            b.Render(element);SaveBitmap(name,b);
        }
        void SaveBitmap(string name,BitmapSource bitmap)
        {
            using var stream=File.Create(Path.Combine(output,name+".png"));var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(bitmap));png.Save(stream);
        }
    }
    static void Layout(FrameworkElement p){p.Measure(new Size(p.Width,p.Height));p.Arrange(new Rect(0,0,p.Width,p.Height));p.UpdateLayout();}
    static string Hash(BitmapSource b){var bytes=new byte[b.PixelWidth*b.PixelHeight*4];b.CopyPixels(bytes,b.PixelWidth*4,0);return Convert.ToHexString(SHA256.HashData(bytes));}
}

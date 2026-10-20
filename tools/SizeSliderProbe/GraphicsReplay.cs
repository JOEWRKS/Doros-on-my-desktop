using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using Dororong.App.Controls;
using Dororong.App.Product;

// Diagnostic process only: published r104 renderer, no desktop input/settings.
internal static class GraphicsReplay
{
    [StructLayout(LayoutKind.Sequential)] struct Mbi {
        public ulong Address, AllocationBase; public uint AllocationProtect, Alignment;
        public ulong Bytes; public uint State, Protect, Type, Alignment2;
    }
    [DllImport("kernel32.dll")] static extern UIntPtr VirtualQuery(IntPtr address,out Mbi info,UIntPtr size);
    [DllImport("user32.dll")] static extern uint GetGuiResources(IntPtr process,uint flags);
    static object[] Regions() {
        ulong address=0;
        var list=new System.Collections.Generic.List<object>();
        while(address<0x7FFFFFFFFFFF) {
            if(VirtualQuery((IntPtr)address,out var r,(UIntPtr)48)==UIntPtr.Zero)break;
            if(r.State==0x1000 && (r.Protect&0x400)!=0)
                list.Add(new {address=r.Address,allocationBase=r.AllocationBase,bytes=r.Bytes});
            var next=r.Address+r.Bytes;if(next<=address)throw new Exception("Invalid region.");
            address=next;
        }
        return list.ToArray();
    }
    internal static void Run(string output)
    {
        Directory.CreateDirectory(output);
        var path=Path.Combine(output,"samples.jsonl");
        if(File.Exists(path))throw new IOException("Preserve previous evidence.");
        using var writer=new StreamWriter(new FileStream(path,FileMode.CreateNew,FileAccess.Write,FileShare.Read)){AutoFlush=true};
        using var process=Process.GetCurrentProcess();
        var hash=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(HighResolutionGrabRaster).Assembly.Location)));
        if(hash!="374666338E0AA082F84B49B8EAD1E4BF0397F0CD08658BC24F336F78F882B322")throw new Exception("Wrong product DLL.");
        File.WriteAllText(Path.Combine(output,"run.json"),JsonSerializer.Serialize(new{
            pid=process.Id,startUTC=process.StartTime.ToUniversalTime(),hash,
            mode="Isolated HWND component replay; not actual user input, no forced GC/trim/settings"}));
        var clock=Stopwatch.StartNew();
        var raster=new HighResolutionGrabRaster();
        var surface=new HighResolutionDisplaySurface();
        byte[]? pixels=null;
        var image=new Image{Width=216,Height=216,Stretch=Stretch.Fill};
        RenderOptions.SetBitmapScalingMode(image,BitmapScalingMode.NearestNeighbor);
        var root=new Grid{Background=Brushes.Transparent};
        root.Children.Add(image);
        root.Children.Add(new TextBlock{Text="그래픽 메모리 진단 · 조작 불필요",VerticalAlignment=VerticalAlignment.Bottom,
            Background=Brushes.White,Foreground=Brushes.Black});
        var window=new Window{Width=408,Height=408,Content=root,WindowStyle=WindowStyle.None,
            AllowsTransparency=true,Background=Brushes.Transparent,ShowActivated=false,ShowInTaskbar=false};
        var menu=new PetContextMenu{Placement=PlacementMode.Relative,PlacementTarget=root};
        menu.Items.Add(new PetSizeMenuItem(()=>150,()=>true,_=>true,()=>{}));
        window.Show();
        var dpi=VisualTreeHelper.GetDpi(window);
        var area=System.Windows.Forms.Screen.FromPoint(System.Windows.Forms.Cursor.Position).WorkingArea;
        window.Left=area.Left/dpi.DpiScaleX+20;window.Top=area.Top/dpi.DpiScaleY+20;
        var phases=new[]{("warm",100),("menu",100),("resize-first",1000),("rest-first",100),
            ("resize-repeat",1000),("rest-repeat",100),("flutter-first",100),("flutter-repeat",300),
            ("rest-final",200),("closed-window",200)};
        int phase=0,frame=0;
        void Snapshot(string boundary) {
            process.Refresh();
            writer.WriteLine(JsonSerializer.Serialize(new{utc=DateTime.UtcNow,ms=clock.Elapsed.TotalMilliseconds,
                phase=phases[phase].Item1,frame,boundary,privateBytes=process.PrivateMemorySize64,
                managed=GC.GetTotalMemory(false),allocated=GC.GetTotalAllocatedBytes(false),
                gen2=GC.CollectionCount(2),gdi=GetGuiResources(process.Handle,0),user=GetGuiResources(process.Handle,1),
                handles=process.HandleCount,regions=Regions()}));
        }
        var timer=new DispatcherTimer(DispatcherPriority.Background){Interval=TimeSpan.FromMilliseconds(20)};
        timer.Tick+=(_,_)=>{
            try {
                var name=phases[phase].Item1;
                if(frame==0) {
                    if(name=="closed-window"){menu.IsOpen=false;image.Source=null;window.Close();}
                    Snapshot("start");
                }
                menu.IsOpen=name=="menu" && frame%40<20;
                if(name!="closed-window") {
                    var scale=name.StartsWith("resize")?(100+(frame%100<=50?frame%100:100-frame%100))/100d:1.5;
                    var flutter=name.StartsWith("flutter");
                    var face=HighResolutionFacePose.Neutral with {FlutterPhase=flutter?frame/30d:-1};
                    pixels=raster.RenderRetainedPixels(flutter?1:0,Matrix.Identity,new(0,0),new(0,0),new(0,0),
                        0,scale,face,1,pixels,preserveCapacity:true);
                    image.Source=surface.Update(pixels,scale);
                    image.Width=image.Height=144*scale;
                }
                if(frame%25==0)Snapshot("sample");
                if(++frame>=phases[phase].Item2) {
                    Snapshot("end");
                    if(++phase>=phases.Length) {
                        timer.Stop();
                        File.WriteAllText(Path.Combine(output,"completion.json"),JsonSerializer.Serialize(new{success=true,seconds=clock.Elapsed.TotalSeconds}));
                        Application.Current.Shutdown();return;
                    }
                    frame=0;
                }
                if(clock.Elapsed.TotalSeconds>150)throw new TimeoutException("Bounded replay exceeded150s.");
            } catch(Exception ex) {
                timer.Stop();menu.IsOpen=false;window.Close();
                File.WriteAllText(Path.Combine(output,"error.txt"),ex.ToString());
                Application.Current.Shutdown();
            }
        };
        timer.Start();Application.Current.Run();
        GC.KeepAlive(raster);GC.KeepAlive(surface);
    }
}

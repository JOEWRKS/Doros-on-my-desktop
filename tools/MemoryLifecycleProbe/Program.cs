using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Dororong.App.Controls;
using Dororong.App.Product;
using Dororong.App.Interaction;
using Dororong.Core.Behavior;
using Dororong.Core.Platforms;

internal static class Program
{
    static string output="";
    static readonly Stopwatch clock=Stopwatch.StartNew();
    [STAThread] static int Main(string[] args)
    {
        output=Path.GetFullPath(args[0]);Directory.CreateDirectory(output);
        if(File.Exists(Path.Combine(output,"samples.jsonl")))throw new IOException("Preserve previous run.");
        var app=new Application {ShutdownMode=ShutdownMode.OnExplicitShutdown};
        File.WriteAllText(Path.Combine(output,"run.json"),JsonSerializer.Serialize(new{
            pid=Environment.ProcessId,startUtc=Process.GetCurrentProcess().StartTime.ToUniversalTime(),
            appHash=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(HighResolutionGrabRaster).Assembly.Location))),
            mode=args[1],forcedGC=false,syntheticDesktopInput=false}));
        try {
            if(args[1]=="neck") Neck();
            else if(args[1]=="perch") Perch();
            else Gpu(app,args.Length>2?int.Parse(args[2]):8);
            File.WriteAllText(Path.Combine(output,"completion.json"),JsonSerializer.Serialize(new{success=true,seconds=clock.Elapsed.TotalSeconds}));
            return 0;
        }catch(Exception ex){File.WriteAllText(Path.Combine(output,"error.txt"),ex.ToString());return 1;}
        finally{app.Shutdown();}
    }
    static void Neck()
    {
        var buffer=new HighResolutionHuntNeck.Buffer();
        var hashes=new List<object>();
        Snapshot("before",0);
        for(var cycle=0;cycle<10;cycle++){
            var allocated=GC.GetAllocatedBytesForCurrentThread();
            for(var frame=0;frame<157;frame++){
                var face=HighResolutionFacePose.Neutral with {
                    BodyFrame=frame,Head=HuntRenderer.HeadTransform(frame,new(.2,-.3,.12*Math.Sin(frame*.1)))};
                var pixels=buffer.Get(face);
                if(cycle==0)hashes.Add(new{frame,hash=Convert.ToHexString(SHA256.HashData(MemoryMarshal.AsBytes(pixels.AsSpan())))});
            }
            Snapshot("neck",cycle+1,GC.GetAllocatedBytesForCurrentThread()-allocated);
        }
        File.WriteAllText(Path.Combine(output,"hashes.json"),JsonSerializer.Serialize(hashes));
        GC.KeepAlive(buffer);
    }
    static void Perch()
    {
        var hashes=new List<object>();
        foreach(var scale in new[]{1,1.1,1.25,1.35,1.5}){
            var p=new DororongPresenter{HighResolutionGrabEnabled=true,HighResolutionScale=scale,Width=144*scale,Height=144*scale};
            ((Canvas)p.Content).Width=p.Width;((Canvas)p.Content).Height=p.Height;
            var pet=new PetSnapshot(PetState.Idle,new(100,100),FacingDirection.Right,0,false,null);
            var delta=TimeSpan.FromMilliseconds(16);
            p.Render(pet,DirectInteractionSnapshot.None,delta);
            p.Measure(new(p.Width,p.Height));p.Arrange(new(0,0,p.Width,p.Height));p.UpdateLayout();
            var size=((BitmapSource)p.HighResolutionImage!.Source).PixelWidth;
            var bytes=new byte[size*size*4];
            for(var cycle=0;cycle<10;cycle++){
                var allocated=GC.GetAllocatedBytesForCurrentThread();
                foreach(var facing in new[]{FacingDirection.Right,FacingDirection.Left}){
                    pet=pet with{Facing=facing};
                    for(var frame=0;frame<20;frame++){
                        p.Render(pet,DirectInteractionSnapshot.None,delta);
                        p.ApplyEdgePerch(frame<10?EdgePerchPhase.Attached:EdgePerchPhase.None,facing,delta,false);
                        var b=(BitmapSource)p.HighResolutionImage!.Source;
                        b.CopyPixels(bytes,b.PixelWidth*4,0);
                        if(cycle==0)hashes.Add(new{scale,facing=(int)facing,frame,hash=Convert.ToHexString(SHA256.HashData(bytes))});
                    }
                }
                Snapshot($"perch-{scale}",cycle+1,GC.GetAllocatedBytesForCurrentThread()-allocated);
            }
        }
        File.WriteAllText(Path.Combine(output,"hashes.json"),JsonSerializer.Serialize(hashes));
    }
    static void Gpu(Application app,int cycles)
    {
        var raster=new HighResolutionGrabRaster();
        var surface=new HighResolutionDisplaySurface();
        var image=new Image{Width=384,Height=384,Stretch=Stretch.Fill};
        RenderOptions.SetBitmapScalingMode(image,BitmapScalingMode.NearestNeighbor);
        var snack=new Image{Width=32,Height=32,HorizontalAlignment=HorizontalAlignment.Right,VerticalAlignment=VerticalAlignment.Top};
        var snackTemplate=new MenuItem{Header="간식"};
        var menu=new PetContextMenu{Placement=PlacementMode.Relative,HorizontalOffset=30,VerticalOffset=40};
        snackTemplate.Template=(ControlTemplate)menu.Resources["SnackTemplate"];
        menu.Items.Add(snackTemplate);
        menu.Items.Add(new MenuItem{Header="진단용 메뉴 — 입력 불필요"});
        var root=new Grid{Background=Brushes.Transparent};
        root.Children.Add(image);root.Children.Add(snack);
        root.Children.Add(new TextBlock{Text="메모리 자동 재생 검사 · 입력 불필요",VerticalAlignment=VerticalAlignment.Bottom,Background=Brushes.White,Foreground=Brushes.Black});
        var window=new Window{Title="도로롱 메모리 진단",Width=400,Height=420,WindowStyle=WindowStyle.None,
            AllowsTransparency=true,Background=Brushes.Transparent,ShowActivated=false,ShowInTaskbar=false,Content=root};
        menu.PlacementTarget=root;
        window.Show();
        var dpi=VisualTreeHelper.GetDpi(window);
        var area=System.Windows.Forms.Screen.FromPoint(System.Windows.Forms.Cursor.Position).WorkingArea;
        window.Left=area.Left/dpi.DpiScaleX+20;window.Top=area.Top/dpi.DpiScaleY+20;
        byte[]? pixels=null;
        var timer=new DispatcherTimer(DispatcherPriority.Background){Interval=TimeSpan.FromMilliseconds(30)};
        int frame=0,cycle=0;
        var hashes=new List<object>();
        Snapshot("before",0);
        timer.Tick+=(_,_)=>{
            try{
                const int phases=10,steps=12;
                var phase=frame/steps;var i=frame%steps;var t=i/(double)(steps-1);
                var face=HighResolutionFacePose.Neutral;
                var inverse=Matrix.Identity;double progress=0,spread=0;
                switch(phase){
                    case 0:face=face with{Breathing=t};break;
                    case 1:face=face with{MotionFrame=HighResolutionLocomotion.Walk(1,t*10)};break;
                    case 2:progress=t;inverse.RotateAt(-40+80*t,43,32);break;
                    case 3:spread=t;inverse.ScaleAt(1,.6+.4*t,48,87);break;
                    case 4:face=face with{BodyFrame=(int)(t*156),Head=HuntRenderer.HeadTransform((int)(t*156),new(.2,.4,.1))};break;
                    case 5:face=face with{Pull=20*Math.Sin(t*Math.PI),EyeFollow=8,HairFollow=8};break;
                    case 6:face=face with{MotionFrame=HighResolutionLocomotion.Run(1,t*66),DroolAge=t};break;
                    case 7:face=face with{MouthAge=t*1.6,Closed=true};break;
                    case 8:progress=1;face=face with{FlutterPhase=t};break;
                    case 9:face=face with{MotionFrame=HighResolutionLocomotion.Perch,Paw=1,PawY=8*t};break;
                }
                if(frame==0){snack.Source=SnackImage.Source;menu.IsOpen=true;}
                if(frame==steps){menu.IsOpen=false;snack.Visibility=Visibility.Hidden;}
                if(frame==6*steps)snack.Visibility=Visibility.Visible;
                if(frame==8*steps)snack.Visibility=Visibility.Hidden;
                pixels=raster.RenderRetainedPixels(progress,inverse,new(37.5,28.25),default,default,spread,
                    1.5*dpi.DpiScaleX,face,1.5,pixels);
                image.Source=surface.Update(pixels,1.5*dpi.DpiScaleX);
                if(cycle==0)hashes.Add(new{frame,hash=Convert.ToHexString(SHA256.HashData(pixels))});
                frame++;
                if(frame==phases*steps){
                    Snapshot("cycle",++cycle);frame=0;
                    if(cycle==cycles){
                        timer.Stop();menu.IsOpen=false;
                        File.WriteAllText(Path.Combine(output,"hashes.json"),JsonSerializer.Serialize(hashes));
                        window.Close();app.Shutdown();
                    }
                }
            }catch(Exception ex){File.WriteAllText(Path.Combine(output,"error.txt"),ex.ToString());timer.Stop();window.Close();app.Shutdown(1);}
        };
        timer.Start();app.Run();
        if(File.Exists(Path.Combine(output,"error.txt")))throw new Exception("Probe failed; see error.txt.");
    }
    static void Snapshot(string phase,int cycle,long allocated=0)
    {
        using var process=Process.GetCurrentProcess();
        var info=GC.GetGCMemoryInfo();
        var row=new{phase,cycle,utc=DateTime.UtcNow,seconds=clock.Elapsed.TotalSeconds,
            @private=process.PrivateMemorySize64,workingSet=process.WorkingSet64,handles=process.HandleCount,
            gdi=GetGuiResources(process.Handle,0),user=GetGuiResources(process.Handle,1),
            managed=GC.GetTotalMemory(false),gcCommitted=info.TotalCommittedBytes,
            gc0=GC.CollectionCount(0),gc1=GC.CollectionCount(1),gc2=GC.CollectionCount(2),
            allocated,writeCombine=WriteCombine(process.Handle)};
        File.AppendAllText(Path.Combine(output,"samples.jsonl"),JsonSerializer.Serialize(row)+Environment.NewLine);
        Console.WriteLine(JsonSerializer.Serialize(row));
    }
    static ulong WriteCombine(IntPtr handle){
        ulong address=0,total=0;
        while(address<0x7FFFFFFFFFFF && VirtualQueryEx(handle,(UIntPtr)address,out var m,(UIntPtr)48)!=UIntPtr.Zero){
            if(m.State==0x1000 && (m.Protect&0x400)!=0)total+=m.RegionSize;
            var next=m.BaseAddress+m.RegionSize;if(next<=address)break;address=next;
        }
        return total;
    }
    [StructLayout(LayoutKind.Sequential)] struct Mbi {
        public ulong BaseAddress,AllocationBase;public uint AllocationProtect,Alignment;
        public ulong RegionSize;public uint State,Protect,Type,Alignment2;
    }
    [DllImport("kernel32.dll")] static extern UIntPtr VirtualQueryEx(IntPtr process,UIntPtr address,out Mbi info,UIntPtr length);
    [DllImport("user32.dll")] static extern uint GetGuiResources(IntPtr process,uint flags);
}

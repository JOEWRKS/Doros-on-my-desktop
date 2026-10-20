using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Dororong.App.Controls;
using Dororong.App.Interaction;
using Dororong.App.Runtime;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;

// Transparent, real HWND but deterministic component replay, not OS user input.
internal static class NativeResize
{
    [DllImport("user32.dll")] static extern uint GetGuiResources(IntPtr process, uint flag);
    [StructLayout(LayoutKind.Sequential)] struct Region
    {
        public ulong BaseAddress, AllocationBase;
        public uint AllocationProtect, Alignment;
        public ulong RegionSize;
        public uint State, Protect, Type, Alignment2;
    }
    [DllImport("kernel32.dll")] static extern UIntPtr VirtualQuery(IntPtr address,out Region info,UIntPtr size);
    static ulong WriteCombineBytes()
    {
        ulong address=0,total=0;
        while(address<0x7FFFFFFFFFFF) {
            if(VirtualQuery((IntPtr)address,out var r,(UIntPtr)48)==UIntPtr.Zero)break;
            if(r.State==0x1000&&(r.Protect&0x400)!=0)total+=r.RegionSize;
            var next=r.BaseAddress+r.RegionSize;if(next<=address)break;address=next;
        }
        return total;
    }
    internal static void Run(string output)
    {
        Directory.CreateDirectory(output);
        var presenter = new DororongPresenter();
        presenter.SetArtSize(150);
        var root = new Grid(); root.Children.Add(presenter);
        var window = new Window { Content=root, WindowStyle=WindowStyle.None, AllowsTransparency=true,
            Background=System.Windows.Media.Brushes.Transparent, ShowActivated=false, ShowInTaskbar=false,
            Left=300, Top=300 };
        var viewport=new PetWindowViewport(window,presenter);
        var position=new PointD(400,400);
        var pet=new PetSnapshot(PetState.Idle,position,FacingDirection.Right,0,false,null);
        var rows=new List<object>();
        var samples=new List<object>();
        var clock=Stopwatch.StartNew();
        using var process=Process.GetCurrentProcess();
        var step=0;
        var timer=new DispatcherTimer(DispatcherPriority.Background) { Interval=TimeSpan.FromMilliseconds(20) };
        window.Show();
        viewport.SetPosition(position);
        presenter.Render(pet,DirectInteractionSnapshot.None,TimeSpan.Zero);
        window.UpdateLayout();
        timer.Tick+=(_,_) =>
        {
            if(step>=600)
            {
                timer.Stop();
                File.WriteAllText(Path.Combine(output,"native.json"),JsonSerializer.Serialize(new {
                    mode="Transparent HWND deterministic resize replay; not desktop input, no forced GC",
                    seconds=clock.Elapsed.TotalSeconds,rows,samples
                },new JsonSerializerOptions{WriteIndented=true}));
                window.Close(); Application.Current.Shutdown(); return;
            }
            var n=step%100;
            var percent=n<=50?100+n:200-n;
            var started=Stopwatch.GetTimestamp();
            var contact=presenter.MeasurePlatformContact()!.Value;
            var before=new PointD((contact.Left+contact.Right)/2,contact.SoleY);
            var t0=Stopwatch.GetTimestamp();
            presenter.SetArtSize(percent);
            var t1=Stopwatch.GetTimestamp();
            viewport.SetPosition(position);
            var t2=Stopwatch.GetTimestamp();
            var layout=presenter.HighResolutionLayoutMilliseconds;
            var sampling=presenter.HighResolutionSamplingMilliseconds;
            var preparation=presenter.HighResolutionPreparationMilliseconds;
            var bitmap=presenter.HighResolutionBitmapMilliseconds;
            presenter.Render(pet with {Position=position},DirectInteractionSnapshot.None,TimeSpan.Zero);
            var t3=Stopwatch.GetTimestamp();
            contact=presenter.MeasurePlatformContact()!.Value;
            var after=new PointD((contact.Left+contact.Right)/2,contact.SoleY);
            var t4=Stopwatch.GetTimestamp();
            var offset=before-after;
            position+=offset; presenter.RebaseSizePresentation(offset);
            viewport.SetPosition(position);
            var t5=Stopwatch.GetTimestamp();
            double Ms(long a,long b)=>Stopwatch.GetElapsedTime(a,b).TotalMilliseconds;
            rows.Add(new{step,percent,total=Ms(started,t5),before=Ms(started,t0),size=Ms(t0,t1),
                native=Ms(t1,t2),render=Ms(t2,t3),contact=Ms(t3,t4),move=Ms(t4,t5),
                layout=presenter.HighResolutionLayoutMilliseconds-layout,
                sampling=presenter.HighResolutionSamplingMilliseconds-sampling,
                preparation=presenter.HighResolutionPreparationMilliseconds-preparation,
                bitmap=presenter.HighResolutionBitmapMilliseconds-bitmap});
            if(step%50==0 || step==599)
            {
                process.Refresh();
                samples.Add(new{step,ms=clock.Elapsed.TotalMilliseconds,privateBytes=process.PrivateMemorySize64,
                    managed=GC.GetTotalMemory(false),allocated=GC.GetTotalAllocatedBytes(false),
                    writeCombineBytes=WriteCombineBytes(),gcFragmentedBytes=GC.GetGCMemoryInfo().FragmentedBytes,
                    gdi=GetGuiResources(process.Handle,0),user=GetGuiResources(process.Handle,1),
                    handles=process.HandleCount,gen2=GC.CollectionCount(2)});
            }
            step++;
        };
        timer.Start(); Application.Current.Run();
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;
using Dororong.App.Interaction;
using Dororong.Core.Behavior;

internal static class Stress
{
    [DllImport("user32.dll")] static extern uint GetGuiResources(IntPtr process, uint flag);
    internal static void Run(string output)
    {
        Directory.CreateDirectory(output);
        var p=new DororongPresenter();
        var pet=new PetSnapshot(PetState.Idle,new(0,0),FacingDirection.Right,0,false,null);
        var costs=new List<double>();var rounds=new List<object>();
        var clock=Stopwatch.StartNew();var allocated=GC.GetTotalAllocatedBytes(true);
        using var proc=Process.GetCurrentProcess();
        for(var round=0;round<20;round++)
        {
            var first=costs.Count;var startAlloc=GC.GetTotalAllocatedBytes(true);
            for(var step=0;step<100;step++)
            {
                var percent=step<=50?100+step:200-step;
                var began=Stopwatch.GetTimestamp();
                p.SetArtSize(percent);
                p.Render(pet,DirectInteractionSnapshot.None,TimeSpan.Zero);
                p.Measure(new Size(p.Width,p.Height));p.Arrange(new Rect(0,0,p.Width,p.Height));p.UpdateLayout();
                _=p.MeasurePlatformContact();
                costs.Add(Stopwatch.GetElapsedTime(began).TotalMilliseconds);
            }
            proc.Refresh();
            rounds.Add(new{round,ms=clock.Elapsed.TotalMilliseconds,privateBytes=proc.PrivateMemorySize64,
                managed=GC.GetTotalMemory(false),allocatedBytes=GC.GetTotalAllocatedBytes(true)-startAlloc,
                gdi=GetGuiResources(proc.Handle,0),user=GetGuiResources(proc.Handle,1),handles=proc.HandleCount,
                gen2=GC.CollectionCount(2),median=Quantile(costs.Skip(first),.5),p95=Quantile(costs.Skip(first),.95)});
        }
        File.WriteAllText(Path.Combine(output,"stress.json"),JsonSerializer.Serialize(new{
            mode="Published DLL, source-quality presenter resize/sole loop; synthetic, no desktop input or screen FPS proof",
            totalMs=clock.Elapsed.TotalMilliseconds,totalAllocated=GC.GetTotalAllocatedBytes(true)-allocated,
            count=costs.Count,p50=Quantile(costs,.5),p95=Quantile(costs,.95),p99=Quantile(costs,.99),max=costs.Max(),rounds
        },new JsonSerializerOptions{WriteIndented=true}));
        Console.WriteLine($"Stress {costs.Count} frames: p50={Quantile(costs,.5):F2}ms, p95={Quantile(costs,.95):F2}ms");
    }
    static double Quantile(IEnumerable<double> source,double q){var v=source.OrderBy(n=>n).ToArray();return v[(int)((v.Length-1)*q)];}
}

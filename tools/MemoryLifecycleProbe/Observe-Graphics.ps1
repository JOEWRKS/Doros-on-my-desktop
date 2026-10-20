param(
    [Parameter(Mandatory)][int]$TargetPid,
    [Parameter(Mandatory)][long]$ExpectedStartTicks,
    [Parameter(Mandatory)][string]$OutputDirectory,
    [ValidateRange(10,3600)][int]$Seconds=600,
    [ValidateRange(1,60)][int]$IntervalSeconds=5
)
$ErrorActionPreference='Stop'
$outputRoot=(Resolve-Path -LiteralPath $OutputDirectory).Path
$samplePath=Join-Path $outputRoot 'samples.jsonl'
if(Test-Path -LiteralPath $samplePath){throw 'Preserve existing samples.'}
Add-Type @'
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
public static class GraphicsObservation {
 [StructLayout(LayoutKind.Sequential)] public struct MBI {
  public ulong BaseAddress, AllocationBase; public uint AllocationProtect, Alignment;
  public ulong RegionSize; public uint State, Protect, Type, Alignment2;
 }
 public sealed class Region {
  public ulong address, allocationBase, bytes;
  public uint protect, type;
 }
 [DllImport("kernel32.dll",SetLastError=true)] public static extern IntPtr OpenProcess(uint access,bool inherit,int pid);
 [DllImport("kernel32.dll",SetLastError=true)] static extern UIntPtr VirtualQueryEx(IntPtr process,UIntPtr address,out MBI info,UIntPtr size);
 [DllImport("kernel32.dll")] public static extern bool CloseHandle(IntPtr handle);
 [DllImport("user32.dll")] public static extern uint GetGuiResources(IntPtr process,uint flags);
 public static Region[] Read(IntPtr process) {
  var regions=new List<Region>();
  ulong address=0;
  while(address<0x7FFFFFFFFFFF) {
   MBI info;
   var count=VirtualQueryEx(process,new UIntPtr(address),out info,new UIntPtr((uint)Marshal.SizeOf(typeof(MBI))));
   if(count.ToUInt64()==0) {
    int error=Marshal.GetLastWin32Error();
    if(error==87) break; // End of queryable user address space.
    throw new Win32Exception(error);
   }
   if(info.State==0x1000 && (info.Protect&0x400)!=0)
    regions.Add(new Region {address=info.BaseAddress,allocationBase=info.AllocationBase,
     bytes=info.RegionSize,protect=info.Protect,type=info.Type});
   ulong next=info.BaseAddress+info.RegionSize;
   if(next<=address) throw new InvalidOperationException("Invalid region range.");
   address=next;
  }
  return regions.ToArray();
 }
}
'@
$target=Get-Process -Id $TargetPid
try {
    if($target.StartTime.ToUniversalTime().Ticks -ne $ExpectedStartTicks){throw 'Process identity changed.'}
} finally {$target.Dispose()}
$handle=[GraphicsObservation]::OpenProcess(0x400,$false,$TargetPid)
if($handle -eq [IntPtr]::Zero){throw 'Read-only query handle unavailable.'}
$clock=[Diagnostics.Stopwatch]::StartNew()
$writer=[IO.StreamWriter]::new([IO.File]::Open($samplePath,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::Read))
$writer.AutoFlush=$true
$count=0;$reason='duration_completed';$failure=$null
try {
    while($clock.Elapsed.TotalSeconds -lt $Seconds){
        $proc=Get-Process -Id $TargetPid -ErrorAction SilentlyContinue
        if(!$proc){$reason='target_exited';break}
        try {
            if($proc.StartTime.ToUniversalTime().Ticks -ne $ExpectedStartTicks){$reason='pid_reused';break}
            $scan=[Diagnostics.Stopwatch]::StartNew()
            $regions=@([GraphicsObservation]::Read($handle))
            $scan.Stop()
            $proc.Refresh()
            $writer.WriteLine(([pscustomobject]@{
                ms=$clock.Elapsed.TotalMilliseconds;utc=[DateTime]::UtcNow.ToString('o')
                pid=$TargetPid;startTicks=$ExpectedStartTicks
                privateBytes=$proc.PrivateMemorySize64;workingSetBytes=$proc.WorkingSet64
                handles=$proc.HandleCount;cpuSeconds=$proc.CPU
                gdi=[GraphicsObservation]::GetGuiResources($handle,0)
                user=[GraphicsObservation]::GetGuiResources($handle,1)
                writeCombineBytes=($regions|Measure-Object bytes -Sum).Sum
                regionScanMs=$scan.Elapsed.TotalMilliseconds;regions=$regions
            }|ConvertTo-Json -Depth 5 -Compress));$count++
        } finally {$proc.Dispose()}
        Start-Sleep -Seconds $IntervalSeconds
    }
} catch {$reason='observer_error';$failure=$_.Exception.Message}
finally {
    $writer.Dispose()
    [GraphicsObservation]::CloseHandle($handle)|Out-Null
    [pscustomobject]@{pid=$TargetPid;startTicks=$ExpectedStartTicks;reason=$reason;error=$failure;
        samples=$count;elapsedSeconds=$clock.Elapsed.TotalSeconds;endedUtc=[DateTime]::UtcNow.ToString('o')} |
        ConvertTo-Json|Set-Content -LiteralPath (Join-Path $outputRoot 'completion.json')
}

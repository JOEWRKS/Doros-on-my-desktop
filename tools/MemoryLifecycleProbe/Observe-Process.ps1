param([Parameter(Mandatory)][int]$TargetPid,[Parameter(Mandatory)][long]$ExpectedStartTicks,
    [Parameter(Mandatory)][string]$OutputDirectory,[int]$Seconds=600)
$ErrorActionPreference='Stop'
$outputRoot=(Resolve-Path -LiteralPath $OutputDirectory).Path
$samplePath=Join-Path $outputRoot 'samples.jsonl'
if(Test-Path -LiteralPath $samplePath){throw 'Preserve existing samples.'}
Add-Type @'
using System.Runtime.InteropServices;
public static class LifecycleGuiCounters {
 [DllImport("user32.dll")] public static extern uint GetGuiResources(System.IntPtr p,uint flags);
}
'@
$clock=[Diagnostics.Stopwatch]::StartNew()
$writer=[IO.StreamWriter]::new([IO.File]::Open($samplePath,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::Read))
$writer.AutoFlush=$true
$count=0;$reason='duration_completed';$failure=$null
try {
    while($clock.Elapsed.TotalSeconds-lt$Seconds){
        $proc=Get-Process -Id $TargetPid -ErrorAction SilentlyContinue
        if(!$proc){$reason='target_exited';break}
        try {
            if($proc.StartTime.ToUniversalTime().Ticks-ne$ExpectedStartTicks){$reason='pid_reused';break}
            $writer.WriteLine(([pscustomobject]@{
                ms=$clock.Elapsed.TotalMilliseconds;utc=[DateTime]::UtcNow.ToString('o')
                privateBytes=$proc.PrivateMemorySize64;workingSetBytes=$proc.WorkingSet64
                handles=$proc.HandleCount;threads=$proc.Threads.Count;cpuSeconds=$proc.CPU
                gdi=[LifecycleGuiCounters]::GetGuiResources($proc.Handle,0)
                user=[LifecycleGuiCounters]::GetGuiResources($proc.Handle,1)
            }|ConvertTo-Json -Compress));$count++
        } finally {$proc.Dispose()}
        Start-Sleep -Seconds 2
    }
} catch {$reason='observer_error';$failure=$_.Exception.Message}
finally {
    $writer.Dispose()
    [pscustomobject]@{reason=$reason;error=$failure;samples=$count;elapsedSeconds=$clock.Elapsed.TotalSeconds;endedUtc=[DateTime]::UtcNow.ToString('o')}|
        ConvertTo-Json|Set-Content -LiteralPath (Join-Path $outputRoot 'completion.json')
}

param(
    [Parameter(Mandatory=$true)][string]$BinaryDirectory,
    [Parameter(Mandatory=$true)][string]$OutputDirectory,
    [int]$Seconds=30
)
$ErrorActionPreference='Stop'
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class PlatformProbeNative {
    [DllImport("user32.dll", EntryPoint="GetWindowLongPtrW")]
    public static extern IntPtr GetWindowLongPtr(IntPtr h, int index);
    [DllImport("user32.dll")]
    public static extern bool GetLayeredWindowAttributes(IntPtr h, out uint key, out byte alpha, out uint flags);
}
'@
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$assembly=[Reflection.Assembly]::LoadFrom((Join-Path $BinaryDirectory 'Dororong.App.dll'))
$flags=[Reflection.BindingFlags]'Public,NonPublic,Instance'
$type=$assembly.GetType('Dororong.App.Interop.DesktopMetadataReader',$true)
$reader=$type.GetConstructors($flags) | Where-Object {$_.GetParameters().Count -eq 1} | Select-Object -First 1
$instance=$reader.Invoke(@([IntPtr]::Zero))
$method=$type.GetMethod('Read',$flags)
$watch=[Diagnostics.Stopwatch]::StartNew()
$samples=[Collections.Generic.List[object]]::new()
$writer=[IO.StreamWriter]::new((Join-Path $OutputDirectory 'samples.jsonl'),$false,[Text.UTF8Encoding]::new($false))
$last=$null
try {
    while($watch.Elapsed.TotalSeconds -lt $Seconds){
        $at=$watch.Elapsed.TotalMilliseconds;$query=[Diagnostics.Stopwatch]::StartNew()
        try {
            $last=$method.Invoke($instance,@())
            $queryMs=$query.Elapsed.TotalMilliseconds
            $windows=@(foreach($window in $last.Windows){
                $wt=$window.GetType();$wh=[IntPtr]$window.Handle
                [uint32]$key=0;[byte]$alpha=0;[uint32]$layerFlags=0
                $hasAlpha=[PlatformProbeNative]::GetLayeredWindowAttributes($wh,[ref]$key,[ref]$alpha,[ref]$layerFlags)
                [ordered]@{Handle=$window.Handle;ProcessId=$window.ProcessId;ClassName=$window.ClassName;
                    Bounds=$window.Bounds;Visible=$window.Visible;Excluded=$window.Excluded;Cloaked=$window.Cloaked;
                    CanSupport=$window.CanSupport;CanOcclude=$wt.GetProperty('CanOcclude',$flags).GetValue($window);
                    IsTransientMenu=$wt.GetProperty('IsTransientMenu',$flags).GetValue($window);
                    Style=[PlatformProbeNative]::GetWindowLongPtr($wh,-16).ToInt64();
                    ExtendedStyle=[PlatformProbeNative]::GetWindowLongPtr($wh,-20).ToInt64();
                    HasLayerAttributes=$hasAlpha;Alpha=$alpha;LayerFlags=$layerFlags}
            })
            $sample=[ordered]@{utc=[DateTime]::UtcNow.ToString('O');ms=$at;queryMs=$queryMs;success=$true;windows=$windows}
        } catch {
            $cause=$_.Exception.GetBaseException();$property=$cause.GetType().GetProperty('Diagnostic',$flags)
            $sample=[ordered]@{utc=[DateTime]::UtcNow.ToString('O');ms=$at;queryMs=$query.Elapsed.TotalMilliseconds;success=$false;failure=$(if($property){$property.GetValue($cause)}else{$cause.GetType().FullName})}
        }
        $writer.WriteLine(($sample | ConvertTo-Json -Depth 7 -Compress));$writer.Flush()
        $samples.Add([pscustomobject]@{ms=$at;queryMs=$sample.queryMs;success=$sample.success;failure=$sample['failure']})
        Start-Sleep -Milliseconds ([Math]::Max(1,80-[int]$query.ElapsedMilliseconds))
    }
} finally {$writer.Dispose()}
$summary=[ordered]@{elapsedMs=$watch.Elapsed.TotalMilliseconds;count=$samples.Count;failed=@($samples | Where-Object {-not $_.success}).Count;maxQueryMs=($samples | Measure-Object queryMs -Maximum).Maximum;failures=@($samples | Where-Object {-not $_.success} | Group-Object failure | Select-Object Name,Count);scope='Read-only native window geometry/class/PID metadata, no titles/content/input or pet interruption.'}
[IO.File]::WriteAllText((Join-Path $OutputDirectory 'summary.json'),($summary | ConvertTo-Json -Depth 5))
$summary | ConvertTo-Json -Depth 5
if($last){$windows | Where-Object {$_.Visible -and -not $_.Excluded -and -not $_.Cloaked} | ConvertTo-Json -Depth 5}

param([Parameter(Mandatory=$true)][string]$Path)
$capture=Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json
function Measure-Series($values){
    $ordered=@($values | Sort-Object)
    if($ordered.Count -eq 0){return $null}
    [pscustomobject]@{count=$ordered.Count;mean=($ordered|Measure-Object -Average).Average;p50=$ordered[[int][Math]::Floor(($ordered.Count-1)*.5)];p95=$ordered[[int][Math]::Floor(($ordered.Count-1)*.95)];p99=$ordered[[int][Math]::Floor(($ordered.Count-1)*.99)];max=$ordered[-1]}
}
$steady=@($capture.frames | Where-Object {$_.ms -ge 5000 -and $_.intervalMs -gt 0})
$inputs=@($capture.inputs | ForEach-Object {[pscustomobject]@{ms=$_.ms;target=$_.target}})
$transitions=@();$previous=''
foreach($sample in $capture.states){
    $label="$($sample.state.state)/$($sample.state.direct)/$($sample.state.platform.perch)"
    if($label -ne $previous){$transitions += [pscustomobject]@{ms=$sample.ms;label=$label};$previous=$label}
}
$slow=@($steady | Where-Object {$_.intervalMs -gt 50} | Sort-Object intervalMs -Descending | Select-Object -First 12)
$slowRows=@(foreach($frame in $slow){
    $state=$capture.states | Where-Object {$_.ms -le $frame.ms} | Select-Object -Last 1
    [pscustomobject]@{ms=$frame.ms;intervalMs=$frame.intervalMs;rasterMs=$frame.rasterMs;rasters=$frame.rasters;nearestPriorState=$state.state.state;nearestPriorDirect=$state.state.direct;nearestPriorPerch=$state.state.platform.perch;stateAgeMs=$frame.ms-$state.ms}
})
[pscustomobject]@{
    source=(Resolve-Path -LiteralPath $Path).Path
    inputHost=$capture.inputHost;elapsedMs=$capture.elapsedMs;cpuMs=$capture.cpuMs
    cpuSingleCorePercent=100*$capture.cpuMs/$capture.elapsedMs
    startupExcludedMs=5000
    callbackIntervalMs=Measure-Series @($steady.intervalMs)
    rasterCostPerCallbackMs=Measure-Series @($steady.rasterMs)
    callbacksOver33ms=@($steady|Where-Object {$_.intervalMs -gt 33.333}).Count
    callbacksOver50ms=@($steady|Where-Object {$_.intervalMs -gt 50}).Count
    callbacksOver100ms=@($steady|Where-Object {$_.intervalMs -gt 100}).Count
    multipleRastersCallbacks=@($steady|Where-Object {$_.rasters -gt 1}).Count
    inputEvents=$inputs;observedStateTransitions=$transitions;slowestCallbacks=$slowRows
    limitation='WPF callbacks, not hardware presentation FPS.100ms state samples do not prove visual quality or exact gesture timing. Hover is not separately identified by this capture.'
} | ConvertTo-Json -Depth 8

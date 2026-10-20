param([Parameter(Mandatory)][string]$Directory)
$ErrorActionPreference='Stop'
$root=(Resolve-Path -LiteralPath $Directory).Path
$run=Get-Content "$root/run.json" -Raw|ConvertFrom-Json
$done=Get-Content "$root/completion.json" -Raw|ConvertFrom-Json
$rows=@(Get-Content "$root/samples.jsonl"|ForEach-Object{$_|ConvertFrom-Json})
$runtime=Get-Content "$root/runtime.json" -Raw|ConvertFrom-Json
function Q($values,$p) {
    $a=@($values|Sort-Object)
    if(!$a.Count){return $null}
    return $a[[int][Math]::Floor(($a.Count-1)*$p)]
}
function Stats($values) {
    $a=@($values)
    return @{count=$a.Count;min=Q $a 0;p50=Q $a .5;p95=Q $a .95;p99=Q $a .99;max=Q $a 1}
}
$sizes=@($runtime.inputs|Where-Object{$_.target -like 'size:*'})
$bursts=[Collections.Generic.List[object]]::new()
$current=[Collections.Generic.List[object]]::new()
function Close-Burst {
    if(!$current.Count){return}
    $values=@($current|ForEach-Object{[int]($_.target.Split(':')[1])})
    $direction=0;$reversals=0
    for($j=1;$j-lt$values.Count;$j++){
        $next=[Math]::Sign($values[$j]-$values[$j-1])
        if($next-ne0){if($direction-ne0-and$next-ne$direction){$reversals++};$direction=$next}
    }
    $bursts.Add(@{firstMs=$current[0].ms;lastMs=$current[$current.Count-1].ms;count=$current.Count;
        min=($values|Measure-Object -Minimum).Minimum;max=($values|Measure-Object -Maximum).Maximum;
        directionReversals=$reversals;applyWorkMs=Stats @($current|ForEach-Object workMs)})
    $current.Clear()
}
foreach($item in $sizes){
    if($current.Count-and($item.ms-$current[$current.Count-1].ms)-gt500){Close-Burst}
    $current.Add($item)
}
Close-Burst
$parts=@(for($i=0;$i -lt [Math]::Ceiling($run.externalSeconds/30);$i++){
    $part=@($rows|Where-Object{$_.ms-ge$i*30000-and$_.ms-lt($i+1)*30000})
    @{fromSecond=$i*30;count=$part.Count;privateBytes=Stats $part.privateBytes;
        gdi=Stats $part.gdi;user=Stats $part.user;handles=Stats $part.handles}
})
$gaps=@(for($i=1;$i-lt$rows.Count;$i++){$rows[$i].ms-$rows[$i-1].ms})
$wallGaps=@(for($i=1;$i-lt$rows.Count;$i++){([DateTime]$rows[$i].utc-[DateTime]$rows[$i-1].utc).TotalMilliseconds})
$proc=Get-Process -Id $run.pid -ErrorAction SilentlyContinue
$identity=$proc-and$proc.StartTime.ToUniversalTime().Ticks-eq$run.startTicks
$activeTicks=@()
if($sizes.Count) {
    $activeTicks=@($runtime.ticks|Where-Object{$_.ms-ge($sizes[0].ms-50)-and$_.ms-le($sizes[-1].ms+50)})
}
$summary=@{
    checkedUTC=[DateTime]::UtcNow.ToString('o');sameProcess=[bool]$identity;completion=$done
    sampleCount=$rows.Count;maxGapMs=Q $gaps 1;maxWallGapMs=Q $wallGaps 1
    internalSeconds=$run.internalSeconds;externalSeconds=$run.externalSeconds
    sizeApplyCount=$sizes.Count;sizeValues=@($sizes.target|Sort-Object -Unique)
    sizeBursts=$bursts;burstGapThresholdMs=500
    firstApply=$sizes|Select-Object -First 1;lastApply=$sizes|Select-Object -Last 1
    applyWorkMs=Stats $sizes.workMs
    activeTickIntervalMs=Stats $activeTicks.intervalMs;activeTickWorkMs=Stats $activeTicks.workMs
    allTickIntervalMs=Stats $runtime.ticks.intervalMs;windows=$parts
    first=$rows[0];last=$rows[-1]
    limitations=@('Size events represent successful applications, not raw mouse samples or displayed-frame latency.',
        'Callback/update timing is not compositor/GPU display FPS. Tick work excludes separate resize apply work.',
        'Bounded repeated input and subsequent idle sampling cannot establish universal or long-duration leak absence.')
}
$summary|ConvertTo-Json -Depth 9|Set-Content -LiteralPath "$root/summary-final.json"
[pscustomobject]$summary|Select-Object sameProcess,sampleCount,maxGapMs,sizeApplyCount,firstApply,lastApply,applyWorkMs,activeTickIntervalMs,activeTickWorkMs|ConvertTo-Json -Depth 5
if($proc){$proc.Dispose()}

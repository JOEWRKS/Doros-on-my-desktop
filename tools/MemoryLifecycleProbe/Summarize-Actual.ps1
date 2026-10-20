param([Parameter(Mandatory)][string]$Directory)
$ErrorActionPreference='Stop'
$root=(Resolve-Path -LiteralPath $Directory).Path
$run=Get-Content "$root/run.json" -Raw|ConvertFrom-Json
$completion=Get-Content "$root/completion.json" -Raw|ConvertFrom-Json
$rows=@(Get-Content "$root/samples.jsonl"|ForEach-Object{$_|ConvertFrom-Json})
$data=Get-Content "$root/runtime.json" -Raw|ConvertFrom-Json
function Median($values){
    $a=@($values|Sort-Object);if(!$a.Count){return $null}
    if($a.Count%2){return $a[[int][Math]::Floor($a.Count/2)]}
    ($a[$a.Count/2-1]+$a[$a.Count/2])/2
}
function Range($values){
    $m=$values|Measure-Object -Minimum -Maximum
    @{min=$m.Minimum;max=$m.Maximum;median=Median $values}
}
$windows=@(for($i=0;$i-lt10;$i++){
    $part=@($rows|Where-Object{$_.ms-ge$i*60000-and$_.ms-lt($i+1)*60000})
    @{minute=$i;count=$part.Count;private=Range $part.privateBytes;gdi=Range $part.gdi;
      user=Range $part.user;handles=Range $part.handles}
})
$gaps=@(for($i=1;$i-lt$rows.Count;$i++){$rows[$i].ms-$rows[$i-1].ms})
$wallGaps=@(for($i=1;$i-lt$rows.Count;$i++){
    ([DateTime]$rows[$i].utc-[DateTime]$rows[$i-1].utc).TotalMilliseconds
})
$proc=Get-Process -Id $run.pid -ErrorAction SilentlyContinue
$identity=$proc-and$proc.StartTime.ToUniversalTime().Ticks-eq$run.startTicks
$summary=@{
    checkedUtc=[DateTime]::UtcNow.ToString('o');sameProcess=[bool]$identity;responding=if($identity){$proc.Responding}else{$null}
    completion=$completion;sampleCount=$rows.Count;maxGapMs=($gaps|Measure-Object -Maximum).Maximum
    maxWallGapMs=($wallGaps|Measure-Object -Maximum).Maximum
    first=$rows[0];last=$rows[-1];windows=$windows
    runtime=@{
        elapsedMs=$data.elapsedMs;cpuMs=$data.cpuMs;inputCount=@($data.inputs).Count;inputs=$data.inputs
        gen0=($data.frames.gen0|Measure-Object -Sum).Sum;gen1=($data.frames.gen1|Measure-Object -Sum).Sum
        gen2=($data.frames.gen2|Measure-Object -Sum).Sum;allocatedBytes=($data.frames.allocatedBytes|Measure-Object -Sum).Sum
        ticksOver50=@($data.ticks|Where-Object intervalMs -gt 50).Count
        maxTickMs=($data.ticks.intervalMs|Measure-Object -Maximum).Maximum
        callbacksOver50=@($data.frames|Where-Object intervalMs -gt 50).Count
        maxCallbackMs=($data.frames.intervalMs|Measure-Object -Maximum).Maximum
        directStates=@($data.states.state.direct|Group-Object|Select-Object Name,Count)
        perchStates=@($data.states.state.platform.perch|Group-Object|Select-Object Name,Count)
        lastMemory=$data.memory[-1];memoryErrors=@($data.memory|Where-Object error).Count
    }
    limitations=@('Internal input logging ends at 300 seconds; external memory sampling ends at 600 seconds.',
        'User completion report does not establish exact counts, timing or visual correctness.',
        'Different workloads and diagnostic retention prohibit whole-process savings attribution.')
}
$summary|ConvertTo-Json -Depth 9|Set-Content "$root/summary-final.json"
$summary|ConvertTo-Json -Depth 9
if($proc){$proc.Dispose()}

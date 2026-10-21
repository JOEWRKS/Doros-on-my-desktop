param([Parameter(Mandatory=$true)][string]$Directory)
$ErrorActionPreference='Stop'
$run=Get-Content -LiteralPath (Join-Path $Directory 'run.json') -Raw | ConvertFrom-Json
$rows=@(Get-Content -LiteralPath (Join-Path $Directory 'samples.jsonl') | ForEach-Object {$_ | ConvertFrom-Json})
$completionPath=Join-Path $Directory 'completion.json'
$completion=if(Test-Path -LiteralPath $completionPath){Get-Content -LiteralPath $completionPath -Raw | ConvertFrom-Json}else{$null}
function Median($values) {
 $sorted=@($values | Sort-Object);if(!$sorted.Count){return $null}
 $mid=[int][Math]::Floor($sorted.Count/2)
 if($sorted.Count%2){return [double]$sorted[$mid]}
 return ([double]$sorted[$mid-1]+[double]$sorted[$mid])/2
}
function Stats($set) {
 if(!$set.Count){return $null}
 $result=[ordered]@{samples=$set.Count;fromSeconds=$set[0].elapsedSeconds;toSeconds=$set[-1].elapsedSeconds}
 foreach($key in @('privateBytes','workingSet','managedEstimate','heapAtLastGc','fragmentedAtLastGc','committedAtLastGc','handles','threads','ownedAllocations','scratchBytes','bodyCache','spreadCache','soleCache')) {
  $v=@($set | ForEach-Object {$_.$key});$result[$key]=@{min=($v | Measure-Object -Minimum).Minimum;median=Median $v;max=($v | Measure-Object -Maximum).Maximum}
 }
 $result['gcIndexes']=@($set.gcIndex | Select-Object -Unique)
 return $result
}
$maxMono=0.0;$maxWall=0.0;$maxDivergence=0.0
for($i=1;$i -lt $rows.Count;$i++){
 $mono=$rows[$i].elapsedSeconds-$rows[$i-1].elapsedSeconds
 $wall=([DateTimeOffset]$rows[$i].utc-[DateTimeOffset]$rows[$i-1].utc).TotalSeconds
 $maxMono=[Math]::Max($maxMono,$mono);$maxWall=[Math]::Max($maxWall,$wall)
 $maxDivergence=[Math]::Max($maxDivergence,[Math]::Abs($wall-$mono))
}
$windows=@();foreach($group in ($rows | Group-Object { [int][Math]::Floor($_.elapsedSeconds/1200) })) {
 $windows+=@{window20Minutes=[int]$group.Name;stats=Stats @($group.Group)}
}
$cycles=@();foreach($group in ($rows | Where-Object cyclePhase -eq 'autonomous' | Group-Object cycle)){
 $cycles+=@{cycle=[long]$group.Name;stats=Stats @($group.Group)}
}
$live=Get-Process -Id $run.pid -ErrorAction SilentlyContinue
$identity=if($live){$live.StartTime.ToUniversalTime().Ticks -eq $run.startTicks}else{$false}
$postWarmup=@($rows | Where-Object phase -eq 'repeated')
$postWarmupStart=if($postWarmup.Count){$postWarmup[0].elapsedSeconds}else{[double]::PositiveInfinity}
[ordered]@{
 mode=$run.mode;nativeMac='NOT_TESTED';pid=$run.pid;currentlySameProcess=$identity;run=$run;completion=$completion
 samples=$rows.Count;maxSampleMonotonicSeconds=$maxMono;maxSampleWallSeconds=$maxWall;maxClockDivergenceSeconds=$maxDivergence
 first10MinutesAfterWarmup=Stats @($postWarmup | Where-Object {$_.elapsedSeconds -lt $postWarmupStart+600})
 last10MinutesOfRepeats=Stats @($rows | Where-Object {$_.phase -eq 'repeated'} | Select-Object -Last 60)
 recovery=Stats @($rows | Where-Object phase -eq 'recovery')
 windows20Minutes=$windows;cycleTailStats=$cycles;lastSample=$rows[-1]
 interpretation='No automatic leak verdict. Compare same-phase late windows and natural GC history; last-GC heap is not current live bytes. Check gaps, workload completion, buffer/cache counts. Native macOS/GPU excluded.'
} | ConvertTo-Json -Depth 12

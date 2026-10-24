param([Parameter(Mandatory)][string]$Before,[Parameter(Mandatory)][string]$After,[Parameter(Mandatory)][string]$Output)
$ErrorActionPreference='Stop'
if(Test-Path -LiteralPath $Output){throw 'Fresh output required'}
$beforeData=Get-Content -LiteralPath $Before -Raw|ConvertFrom-Json
$afterData=Get-Content -LiteralPath $After -Raw|ConvertFrom-Json
function Stats($values){
    $sorted=@($values|Sort-Object)
    if(!$sorted.Count){throw 'Missing measured frames'}
    [ordered]@{count=$sorted.Count;mean=($sorted|Measure-Object -Average).Average;median=$sorted[[int][math]::Floor($sorted.Count/2)];p95=$sorted[[int][math]::Floor($sorted.Count*.95)];max=$sorted[-1]}
}
if($beforeData.results.Count -ne $afterData.results.Count){throw 'Scenario mismatch'}
$results=for($s=0;$s -lt $beforeData.results.Count;$s++){
    $b=$beforeData.results[$s];$a=$afterData.results[$s]
    if($b.size -ne $a.size -or $b.density -ne $a.density -or $b.rows.Count -ne $a.rows.Count){throw 'Scenario/length mismatch'}
    $beforeMs=@();$afterMs=@();$beforeCpu=0.;$afterCpu=0.;$mismatches=0;$spread=0
    for($i=0;$i -lt $a.rows.Count;$i++){
        $br=$b.rows[$i];$ar=$a.rows[$i]
        if($br.hash -ne $ar.hash -or $br.geometry -ne $ar.geometry -or
            ($br.position|ConvertTo-Json -Compress) -ne ($ar.position|ConvertTo-Json -Compress) -or
            ($br.platform|ConvertTo-Json -Depth 12 -Compress) -ne ($ar.platform|ConvertTo-Json -Depth 12 -Compress)){$mismatches++}
        if($br.platform.IsExtremeLanding){$spread++}
        # Predeclared selection: warm cycles with a changed spread frame, not
        # a threshold chosen from timing results. Static hold frames excluded.
        if($br.cycle -gt 0 -and $br.platform.LegSpread -gt 0 -and $i -gt 0 -and $br.hash -ne $b.rows[$i-1].hash){
            $beforeMs+=$br.ms;$afterMs+=$ar.ms;$beforeCpu+=$br.cpuMs;$afterCpu+=$ar.cpuMs
        }
    }
    if(!$spread){throw 'No extreme landing exercised'}
    [ordered]@{size=$b.size;density=$b.density;frames=$a.rows.Count;mismatches=$mismatches;extremeFrames=$spread;
        beforeMs=(Stats $beforeMs);afterMs=(Stats $afterMs);beforeCpuMs=$beforeCpu;afterCpuMs=$afterCpu}
}
$report=[ordered]@{nativeMac='NOT_TESTED';before=$Before;after=$After;results=@($results)}
$report|ConvertTo-Json -Depth 10|Set-Content -LiteralPath $Output -Encoding utf8
$report|ConvertTo-Json -Depth 10
if(@($results|Where-Object {$_.mismatches -ne 0}).Count){throw 'Pixel/position/geometry/state mismatch'}

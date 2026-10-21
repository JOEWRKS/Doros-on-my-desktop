param([Parameter(Mandatory=$true)][string]$Directory)
$ErrorActionPreference='Stop'
function Median($values) { $v=@($values | Sort-Object); $v[[int][Math]::Floor($v.Count/2)] }
$runs=@()
$checked=0
foreach($trial in 2..4) {
    $before=Get-Content -LiteralPath (Join-Path $Directory "baseline-full$trial.json") -Raw | ConvertFrom-Json
    $after=Get-Content -LiteralPath (Join-Path $Directory "candidate-full$trial.json") -Raw | ConvertFrom-Json
    if($before.results.Count -ne $after.results.Count){throw 'Scenario count changed'}
    for($i=0;$i -lt $before.results.Count;$i++) {
        $a=$before.results[$i];$b=$after.results[$i]
        if($a.scenario -ne $b.scenario -or $a.size -ne $b.size -or $a.density -ne $b.density -or $a.frames -ne $b.frames){throw 'Scenario/frame count changed'}
        if(($a.rows | ConvertTo-Json -Depth 10 -Compress) -cne ($b.rows | ConvertTo-Json -Depth 10 -Compress)){throw "Pixels/positions/geometry changed: trial$trial/$i"}
        $checked+=$a.rows.Count
        $runs+=[pscustomobject]@{scenario=$a.scenario;size=$a.size;density=$a.density;before=$a;after=$b}
    }
}
$summary=foreach($group in ($runs | Group-Object scenario,size,density)) {
    $g=$group.Group
    [pscustomobject]@{scenario=$g[0].scenario;size=$g[0].size;density=$g[0].density;
        beforeTickMs=Median $g.before.tickMedian;afterTickMs=Median $g.after.tickMedian;
        beforeCpuMs=Median $g.before.cpuMs;afterCpuMs=Median $g.after.cpuMs;
        beforeGeometry1000Ms=Median $g.before.repeatedGeometry1000Ms;afterGeometry1000Ms=Median $g.after.repeatedGeometry1000Ms}
}
[pscustomobject]@{matchedFrames=$checked;nativeMac='NOT_TESTED';
    note='Three sequential host pairs. CPU includes hashing/probe overhead; not Activity Monitor CPU or native upload.';results=$summary} | ConvertTo-Json -Depth 8

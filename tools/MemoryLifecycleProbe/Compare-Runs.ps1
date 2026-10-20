param([Parameter(Mandatory)][string]$BeforeRoot,[Parameter(Mandatory)][string]$AfterRoot,[Parameter(Mandatory)][string]$OutputPath)
$ErrorActionPreference='Stop'
function Read-Rows($root,$name) {
    $completion=Get-Content -LiteralPath "$root/$name/completion.json" -Raw|ConvertFrom-Json
    if(-not $completion.success){throw "Incomplete run: $root/$name"}
    @(Get-Content -LiteralPath "$root/$name/samples.jsonl"|ForEach-Object{$_|ConvertFrom-Json})
}
function Median($items) {
    $sorted=@($items|Sort-Object);if(-not $sorted.Count){throw 'Empty sample'}
    if($sorted.Count%2){return $sorted[[int][Math]::Floor($sorted.Count/2)]}
    ($sorted[$sorted.Count/2-1]+$sorted[$sorted.Count/2])/2
}
function Range($rows,$field) {
    $m=$rows|Measure-Object -Property $field -Minimum -Maximum
    @{min=$m.Minimum;max=$m.Maximum;first=$rows[0].$field;last=$rows[-1].$field}
}
function Hashes($a,$b,$keys) {
    $left=@(Get-Content -LiteralPath $a -Raw|ConvertFrom-Json)
    $right=@(Get-Content -LiteralPath $b -Raw|ConvertFrom-Json)
    $differences=@(Compare-Object $left $right -Property $keys)
    if($left.Count-ne$right.Count-or$differences.Count){throw "Pixel mismatch: $a vs $b"}
    @{frames=$left.Count;mismatches=0}
}
$neckBefore=Read-Rows $BeforeRoot 'neck-before'
$neckAfter=Read-Rows $AfterRoot 'neck-final'
$perchBefore=Read-Rows $BeforeRoot 'perch-before-buffered'
$perchAfter=Read-Rows $AfterRoot 'perch-after'
$gpuBefore=Read-Rows $BeforeRoot 'gpu-32cycles'
$gpuAfter=Read-Rows $AfterRoot 'gpu-32cycles'
$perch=@(foreach($group in ($perchBefore|Group-Object phase)){
    $before=@($group.Group|Where-Object cycle -gt 1)
    $after=@($perchAfter|Where-Object{$_.phase-eq$group.Name-and$_.cycle-gt 1})
    @{phase=$group.Name;beforeMedianAllocation=Median $before.allocated;afterMedianAllocation=Median $after.allocated}
})
$gpu=@(foreach($run in @(@{name='r97';rows=$gpuBefore},@{name='r98';rows=$gpuAfter})){
    $rows=@($run.rows|Where-Object cycle -ge 8)
    @{name=$run.name;warmCycles='8..32';private=Range $rows 'private';gdi=Range $rows 'gdi';
      user=Range $rows 'user';handles=Range $rows 'handles';writeCombine=Range $rows 'writeCombine';
      managed=Range $rows 'managed';gc2=Range $rows 'gc2'}
})
$result=@{
    neck=@{beforeFullGC=$neckBefore[-1].gc2-$neckBefore[0].gc2;afterFullGC=$neckAfter[-1].gc2-$neckAfter[0].gc2;
        pixels=Hashes "$BeforeRoot/neck-before/hashes.json" "$AfterRoot/neck-final/hashes.json" @('frame','hash')}
    perch=@{allocation=$perch;pixels=Hashes "$BeforeRoot/perch-before-buffered/hashes.json" "$AfterRoot/perch-after/hashes.json" @('scale','facing','frame','hash')}
    gpu=@{runs=$gpu;pixels=Hashes "$BeforeRoot/gpu-32cycles/hashes.json" "$AfterRoot/gpu-32cycles/hashes.json" @('frame','hash')}
    limitations=@('Synthetic state replay through published product assemblies, not desktop mouse-input acceptance.',
        'No forced GC/working-set trim. Warmed pools and garbage awaiting collection are not classified as live leaks.',
        'Different cold process/resource state prevents attributing whole-process private differences to one change.',
        'Bounded repeated workload, not multi-hour or other-hardware validation.')
}
$result|ConvertTo-Json -Depth 10|Set-Content -LiteralPath $OutputPath
$result|ConvertTo-Json -Depth 10

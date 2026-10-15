Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Exercise the real validator comparison, without publishing or launching the pet.
$validator = Join-Path $PSScriptRoot 'Dororong.ProductPackage.Tests.ps1'
$tokens = $null; $parseErrors = $null
$ast = [Management.Automation.Language.Parser]::ParseFile($validator, [ref]$tokens, [ref]$parseErrors)
if ($parseErrors.Count) { throw "Validator parse failed: $parseErrors" }
$blocks = @($ast.FindAll({ param($node)
    $node -is [Management.Automation.Language.TryStatementAst] -and
    $node.Body.Extent.Text -match 'Expand-Archive -LiteralPath \$archive'
}, $true))
if ($blocks.Count -ne 1) { throw 'Expected exactly one archive comparison block.' }
$statements = @($blocks[0].Body.Statements)
if ($statements.Count -ne 5 -or $statements[0].Extent.Text -notmatch '^Expand-Archive') {
    throw 'Archive comparison layout changed; update this boundary test explicitly.'
}
$compare = [scriptblock]::Create(($statements[1..4].Extent.Text -join "`n"))
function Assert-Equal($Expected, $Actual, [string]$Message) {
    if ($Expected -ne $Actual) { throw "$Message Expected '$Expected', observed '$Actual'." }
}
function Get-Hash([string]$Path) { (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash }
function Check([bool]$Condition, [string]$Message) { if (-not $Condition) { throw $Message } }

$testRoot = Join-Path ([IO.Path]::GetTempPath()) ('dororong-parity-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $testRoot | Out-Null
$passed = 0
function Add-File([string]$Root, [string]$Relative, [string]$Value, [bool]$Hidden = $false) {
    $path = Join-Path $Root $Relative
    New-Item -ItemType Directory -Path (Split-Path -Parent $path) -Force | Out-Null
    [IO.File]::WriteAllText($path, $Value)
    if ($Hidden) { [IO.File]::SetAttributes($path, [IO.FileAttributes]::Hidden) }
}
function Case([string]$Name, [scriptblock]$Arrange, [bool]$Reject) {
    $caseRoot = Join-Path $testRoot $Name
    $packageRoot = Join-Path $caseRoot 'package'
    $roundTrip = Join-Path $caseRoot 'expanded'
    Add-File $packageRoot 'normal.txt' 'same'
    Add-File $roundTrip 'normal.txt' 'same'
    & $Arrange $packageRoot $roundTrip
    $failure = $null
    try { & $compare } catch { $failure = $_.Exception.Message }
    if ($Reject) {
        Check ($null -ne $failure -and $failure.StartsWith('Archive round-trip changed')) "$Name failed to reject through parity: $failure"
    } else { Check ($null -eq $failure) "$Name rejected legitimate parity: $failure" }
    $script:passed++
    Write-Output "PASS: $Name"
}
try {
    Case 'ordinary-match' {} $false
    Case 'hidden-package-extra' { param($p,$r) Add-File $p 'extra.txt' 'extra' $true } $true
    Case 'hidden-archive-extra' { param($p,$r) Add-File $r 'extra.txt' 'extra' $true } $true
    Case 'hidden-content-mismatch' {
        param($p,$r)
        Add-File $p 'same-name.txt' 'one' $true
        Add-File $r 'same-name.txt' 'two' $true
    } $true
    Case 'hidden-path-mismatch' {
        param($p,$r)
        Add-File $p 'one.txt' 'same' $true
        Add-File $r 'two.txt' 'same' $true
    } $true
    Case 'hidden-directory-extra' {
        param($p,$r)
        Add-File $p 'hidden-dir/extra.txt' 'extra'
        [IO.File]::SetAttributes((Join-Path $p 'hidden-dir'), [IO.FileAttributes]::Directory -bor [IO.FileAttributes]::Hidden)
    } $true
    Case 'matching-hidden-files' {
        param($p,$r)
        Add-File $p 'nested/same.txt' 'same' $true
        Add-File $r 'nested/same.txt' 'same' $true
    } $false
    Case 'attribute-only-difference' {
        param($p,$r)
        Add-File $p 'same.txt' 'same' $true
        Add-File $r 'same.txt' 'same'
    } $false
} finally {
    $resolved = (Resolve-Path -LiteralPath $testRoot).Path
    $parent = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\')
    if ([IO.Path]::GetDirectoryName($resolved) -ne $parent -or
        [IO.Path]::GetFileName($resolved) -notmatch '^dororong-parity-[0-9a-f]{32}$') {
        throw "Refusing unexpected fixture cleanup: $resolved"
    }
    Remove-Item -LiteralPath $resolved -Recurse -Force
}
Write-Output "PACKAGE PARITY PASS: $passed cases; validator comparison, no app launch."

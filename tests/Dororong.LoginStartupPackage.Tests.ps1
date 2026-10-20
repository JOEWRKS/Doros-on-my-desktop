param(
    [Parameter(Mandatory)][string]$ExecutablePath,
    [Parameter(Mandatory)][string]$EvidencePath
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
function Check([bool]$condition, [string]$message) { if (-not $condition) { throw $message } }
$executable = (Resolve-Path -LiteralPath $ExecutablePath).Path
Check (-not (Test-Path -LiteralPath $EvidencePath)) 'Refusing to overwrite evidence.'
Check (@(Get-Process Dororong,Dororong.App -ErrorAction SilentlyContinue).Count -eq 0) 'Exit the existing pet via its tray first.'

# Reuse only the declared native harness, not the package script's entry point.
$tokens=$null; $errors=$null
$ast=[Management.Automation.Language.Parser]::ParseFile((Join-Path $PSScriptRoot 'Dororong.ProductPackage.Tests.ps1'),[ref]$tokens,[ref]$errors)
Check ($errors.Count -eq 0) 'Package harness parse failed.'
$definitions=@($ast.FindAll({param($node) $node -is [Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -eq 'Add-NativeDesktopHarness'},$false))
Check ($definitions.Count -eq 1) 'Expected one shared native harness.'
. ([scriptblock]::Create($definitions[0].Extent.Text))
Add-NativeDesktopHarness
$desktop=$null
$owned=[Collections.Generic.List[int]]::new()
try {
    $desktop=[Dororong.PackageTests.NativeDesktopHarness]::Create('DororongLoginTest-'+[guid]::NewGuid().ToString('N'))
    $clock=[Diagnostics.Stopwatch]::StartNew()
    $automatic=$desktop.Start($executable,$true); $owned.Add($automatic)
    do {
        Check ($desktop.IsRunning($automatic)) 'Automatic launch exited before displaying the pet.'
        $visible=$desktop.VisibleWindows($automatic)
        if ($visible -gt 0) { break }
        Check ($clock.Elapsed.TotalSeconds -lt 35) 'Automatic launch never displayed a window.'
        Start-Sleep -Milliseconds 50
    } while ($true)
    $automaticMs=$clock.Elapsed.TotalMilliseconds
    Check ($automaticMs -ge 20000) 'Automatic launch displayed before the 20-second delay.'
    Check ($desktop.CloseWindows($automatic) -gt 0) 'Cannot close automatic test window.'
    Check ($desktop.WaitForExit($automatic,10000) -eq 0) 'Automatic test did not exit normally.'

    $pending=$desktop.Start($executable,$true); $owned.Add($pending)
    Start-Sleep -Milliseconds 1500
    Check ($desktop.IsRunning($pending) -and $desktop.VisibleWindows($pending) -eq 0) 'Pending login launch displayed early.'
    $clock.Restart()
    $manual=$desktop.Start($executable); $owned.Add($manual)
    do {
        Check ($desktop.IsRunning($manual)) 'Manual launch exited instead of taking ownership.'
        if ($desktop.VisibleWindows($manual) -gt 0) { break }
        Check ($clock.Elapsed.TotalSeconds -lt 8) 'Manual launch was delayed.'
        Start-Sleep -Milliseconds 50
    } while ($true)
    $manualMs=$clock.Elapsed.TotalMilliseconds
    Check ($desktop.WaitForExit($pending,5000) -eq 0) 'Manual launch did not cancel the pending login process.'
    $duplicate=$desktop.Start($executable); $owned.Add($duplicate)
    Check ($desktop.WaitForExit($duplicate,10000) -eq 0) 'Duplicate launch failed.'
    Check ($desktop.IsRunning($manual)) 'Duplicate launch stopped the owner.'
    Check ($desktop.CloseWindows($manual) -gt 0) 'Cannot close manual test window.'
    Check ($desktop.WaitForExit($manual,10000) -eq 0) 'Manual test did not exit normally.'
    [ordered]@{
        scope='Actual packaged processes on a private Windows desktop; not a login/boot measurement'
        executable=$executable; appSha256=(Get-FileHash (Join-Path (Split-Path $executable) 'Dororong.App.dll')).Hash
        automaticFirstVisibleMs=$automaticMs; manualFirstVisibleMs=$manualMs
        pendingLoginCanceled=$true; duplicateExit=0; normalCloseExit=0
        timestampUtc=[datetime]::UtcNow.ToString('o')
    } | ConvertTo-Json | Set-Content -LiteralPath $EvidencePath -Encoding utf8NoBOM
    Write-Output "LOGIN NATIVE PASS: automatic window ${automaticMs}ms; manual override ${manualMs}ms; pending login canceled; duplicate and normal exit passed."
} finally {
    if ($null -ne $desktop) {
        foreach ($processId in $owned) {
            if ($desktop.IsRunning($processId)) { $desktop.Terminate($processId); [void]$desktop.WaitForExit($processId,10000) }
        }
        $desktop.Dispose()
    }
}

param([Parameter(Mandatory)][string]$ExecutablePath)
$ErrorActionPreference='Stop'
# Explicit delivery smoke only. Never close a pre-existing pet or change settings.
if (Get-Process Dororong,Dororong.App -ErrorAction SilentlyContinue) {
    throw 'A pet is already running; refusing an overlapping delivery smoke.'
}
$testPath=Join-Path $PSScriptRoot '../tests/Dororong.ProductPackage.Tests.ps1'
$tokens=$null; $errors=$null
$ast=[Management.Automation.Language.Parser]::ParseFile($testPath,[ref]$tokens,[ref]$errors)
if ($errors.Count) { throw 'Cannot parse existing package harness.' }
foreach($name in @('Assert-True','Assert-Equal','Get-Hash','Add-NativeDesktopHarness','Invoke-NativeSmoke')) {
    $function=$ast.Find({param($node) $node -is [Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -eq $name},$true)
    if ($null -eq $function) { throw "Missing harness function $name" }
    $definition=$function.Extent.Text
    if($name -eq 'Add-NativeDesktopHarness') {
        $definition=$definition.Replace('var desktop = CreateDesktopW(name, IntPtr.Zero, IntPtr.Zero, 0, access, IntPtr.Zero);',
            'var desktop = OpenInputDesktop(0, false, access);')
        $definition=$definition.Replace('startup.lpDesktop = name;', 'startup.lpDesktop = null;')
        $definition=$definition.Replace('private const uint WmClose',
            '[DllImport("user32.dll", SetLastError = true)] private static extern IntPtr OpenInputDesktop(uint flags, bool inherit, uint access); private const uint WmClose')
    }
    # Definitions come only from the tracked local harness, not external input.
    . ([scriptblock]::Create($definition))
}
$result=@(Invoke-NativeSmoke ([IO.Path]::GetFullPath($ExecutablePath)))
$result | ForEach-Object { $_ -replace 'on an isolated desktop','on the active desktop' }
if($result[-1] -ne 'PASS') { throw 'Active desktop delivery smoke did not pass.' }

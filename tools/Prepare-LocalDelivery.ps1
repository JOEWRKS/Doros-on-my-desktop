param(
    [Parameter(Mandatory)][string]$InstallerDirectory,
    [Parameter(Mandatory)][string]$OutputPath
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = Split-Path -Parent $PSScriptRoot
$source = (Resolve-Path -LiteralPath $InstallerDirectory).Path
$destination = [IO.Path]::GetFullPath($OutputPath)
$deliveryRoot = [IO.Path]::GetFullPath((Join-Path $root 'artifacts/delivery'))
if ((Split-Path -Parent $destination) -ine $deliveryRoot) { throw 'Use a direct artifacts/delivery child.' }
if (Test-Path -LiteralPath $destination) { throw 'Refusing to overwrite an existing delivery.' }
foreach ($location in @($source,$destination)) {
    $part=$location
    while ($part) {
        if ((Test-Path -LiteralPath $part) -and
            ((Get-Item -LiteralPath $part -Force).Attributes -band [IO.FileAttributes]::ReparsePoint)) {
            throw "Reparse point is not allowed: $part"
        }
        $parent=Split-Path -Parent $part
        if ($parent -eq $part) { break }
        $part=$parent
    }
}
$evidence=Get-Content -LiteralPath (Join-Path $source 'build-evidence.json') -Raw | ConvertFrom-Json
$name='Doros-on-my-desktop-Setup-0.1.3-win-x64.exe'
$setup=Join-Path $source $name
if ($evidence.version -cne '0.1.3' -or $evidence.compilerExitCode -ne 0 -or
    $evidence.validatorExitCode -ne 0 -or
    (Get-FileHash -LiteralPath $setup).Hash -cne $evidence.installerSha256) {
    throw 'Installer evidence/hash validation failed.'
}
New-Item -ItemType Directory -Path $destination -Force | Out-Null
Copy-Item -LiteralPath $setup -Destination (Join-Path $destination $name)
Copy-Item -LiteralPath (Join-Path $root 'docs/INSTALLER-README.txt') -Destination (Join-Path $destination 'README.txt')
$lines=foreach ($file in @($name,'README.txt')) {
    '{0}  {1}' -f (Get-FileHash -LiteralPath (Join-Path $destination $file)).Hash,$file
}
[IO.File]::WriteAllLines((Join-Path $destination 'SHA256SUMS.txt'),[string[]]$lines,[Text.UTF8Encoding]::new($false))
if ((Get-FileHash -LiteralPath (Join-Path $destination $name)).Hash -cne $evidence.installerSha256 -or
    @(Get-ChildItem -LiteralPath $destination -Force).Count -ne 3) {
    throw 'Delivery copy verification failed; retained for inspection.'
}
Write-Output "LOCAL DELIVERY PASS: $destination (Setup, README, SHA256SUMS). Not uploaded or published."

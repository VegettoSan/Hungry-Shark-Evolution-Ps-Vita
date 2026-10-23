param(
    [Parameter(Mandatory=$true)][string]$OutDir,
    [string]$Apk,
    [string]$Bundle
)
$ErrorActionPreference = 'Stop'
$target = [IO.Path]::GetFullPath($OutDir)
if (Test-Path -LiteralPath $target) { throw 'Choose a new test directory.' }
New-Item -ItemType Directory -Path $target | Out-Null
$compiler = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
$test = Join-Path $target 'ReleaseTests.exe'
$argsList = @('/nologo', '/target:exe', '/platform:x64', '/optimize+', '/utf8output', '/main:ReleaseTests',
    ('/out:' + $test), '/r:System.IO.Compression.dll', '/r:System.IO.Compression.FileSystem.dll',
    '/r:System.Web.Extensions.dll', '/r:System.Windows.Forms.dll',
    (Join-Path $PSScriptRoot 'HSERebuild.cs'), (Join-Path $PSScriptRoot 'ReleaseTests.cs'))
& $compiler @argsList
if ($LASTEXITCODE -ne 0) { throw 'Test compilation failed.' }
$fixture = Join-Path $target 'fixtures'
if ($Apk -and $Bundle) { & $test $fixture $Apk $Bundle } else { & $test $fixture }
if ($LASTEXITCODE -ne 0) { throw 'Native tests failed.' }

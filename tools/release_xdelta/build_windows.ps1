param([Parameter(Mandatory=$true)][string]$OutDir)
$ErrorActionPreference = 'Stop'
$target = [IO.Path]::GetFullPath($OutDir)
if (Test-Path -LiteralPath $target) { throw 'Choose a new output directory; no overwrite.' }
$compiler = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) { throw '.NET Framework 4.x compiler missing.' }
New-Item -ItemType Directory -Path $target | Out-Null
$argsList = @('/nologo', '/target:exe', '/platform:x64', '/optimize+', '/utf8output',
    ('/out:' + (Join-Path $target 'HSERebuild.exe')), '/r:System.IO.Compression.dll',
    '/r:System.IO.Compression.FileSystem.dll', '/r:System.Web.Extensions.dll', '/r:System.Windows.Forms.dll',
    (Join-Path $PSScriptRoot 'HSERebuild.cs'))
& $compiler @argsList
if ($LASTEXITCODE -ne 0) { throw 'Extractor compilation failed.' }
Get-FileHash -LiteralPath (Join-Path $target 'HSERebuild.exe') -Algorithm SHA256

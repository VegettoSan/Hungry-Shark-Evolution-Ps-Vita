# Locate local portable tools without running or downloading any programs.
param([switch]$NoWrite)
$ErrorActionPreference="Stop"
$repo=(Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$root=Join-Path $repo "Tool Codex"
$specs=@(
    @("AssetRipper","AssetRipper*.exe","P01 REQUIRED"),
    @("ILSpy","ILSpy.exe","P01 REQUIRED"),
    @("dnSpyEx","dnSpy.exe","Optional"),
    @("UABEA","UABEA*.exe","Optional"),
    @("AssetStudio","AssetStudioGUI.exe","Optional"),
    @("xdelta3","xdelta3.exe","P12 optional"),
    @("UnityScript2CSharp","*.exe","P05 optional"),
    @("UnityDataTools","UnityDataTool*.exe","Optional"),
    @("JADX","jadx-gui*.bat","Optional"),
    @("Apktool","apktool*.jar","Optional"),
    @("Ghidra","ghidraRun.bat","Optional"),
    @("Vita3K","Vita3K.exe","Optional"),
    @("xdelta3-legacy","xdelta3.exe","Optional"),
    @("FFmpeg","ffmpeg.exe","Optional"),
    @("Blender","blender.exe","Optional"),
    @("Audacity","audacity.exe","Optional")
)
$lines=New-Object System.Collections.Generic.List[string]
$lines.Add("Tool Codex scan: paths only, not evidence of runtime compatibility.")
$lines.Add("Repository: $repo")
$lines.Add("Local tools directory: $root")
$lines.Add("")
if(Test-Path -LiteralPath $root -PathType Container){
    foreach($entry in $specs){
        $dir=Join-Path $root $entry[0];$hit=$null
        if(Test-Path -LiteralPath $dir -PathType Container){
            $folders=@($dir)
            $folders+=@(Get-ChildItem -LiteralPath $dir -Directory -ErrorAction SilentlyContinue|ForEach-Object{$_.FullName})
            foreach($f in $folders){
                $found=Get-ChildItem -Path (Join-Path $f $entry[1]) -File -ErrorAction SilentlyContinue|Select-Object -First 1
                if($found){$hit=$found.FullName;break}
            }
        }
        if($hit){$lines.Add("[FOUND] $($entry[0]) [$($entry[2])] $hit")}
        else{$lines.Add("[MISSING] $($entry[0]) [$($entry[2])] $dir")}
    }
}else{$lines.Add("[MISSING] Tool Codex folder - create it in the local clone")}
$lines.Add("")
$lines.Add("Global commands (these are installed OUTSIDE Tool Codex):")
foreach($name in @("py","python","git","7z","java","dotnet")){
    $c=Get-Command $name -ErrorAction SilentlyContinue|Select-Object -First 1
    if($c){$lines.Add("[FOUND] $name : $($c.Source)")}else{$lines.Add("[NOT IN PATH] $name")}
}
$lines.Add("")
$lines.Add("Unity 5.6.4p3 / 2018.2.19f1 and PSP2 module are not scanned; verify in the actual Unity editors.")
$lines.Add("Check phase gates separately: py -3 tools\check_phase_gates.py")
$lines|ForEach-Object{Write-Output $_}
if((Test-Path -LiteralPath $root -PathType Container) -and -not $NoWrite){
    $file=Join-Path $root "_tool_inventory.txt"
    $lines|Out-File -LiteralPath $file -Encoding utf8
    Write-Output "Saved ignored local report: $file"
}

#requires -Version 7.0
param(
    [Parameter(Mandatory=$true)][string]$AssemblyPath,
    [string]$BeforeAssembly,
    [string]$AfterAssembly,
    [string]$Dnlib='Tool Codex/dnSpyEx/bin/dnlib.dll'
)
# Read-only, local metadata checks. Does not emit game code or load the game assembly.
$ErrorActionPreference='Stop'
$null=[Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $Dnlib))
function Require($value,[string]$message) { if (!$value) { throw $message } }
function Calls($method) {
    @($method.Body.Instructions | Where-Object {$_.Operand -is [dnlib.DotNet.IMethod]} | ForEach-Object {$_.Operand.FullName})
}
function BodyKey($method) {
    if (!$method.HasBody) { return '<no body>' }
    $instructions=$method.Body.Instructions
    $parts=foreach($instruction in $instructions) {
        $operand=$instruction.Operand
        if ($operand -is [dnlib.DotNet.Emit.Instruction]) { $operand='branch:'+ $instructions.IndexOf($operand) }
        elseif ($operand -is [dnlib.DotNet.Emit.Instruction[]]) { $operand='switch:'+ (($operand | ForEach-Object {$instructions.IndexOf($_)}) -join ',') }
        elseif ($operand -is [dnlib.DotNet.Emit.Local]) { $operand='local:'+ $operand.Index }
        elseif ($operand -is [dnlib.DotNet.Parameter]) { $operand='arg:'+ $operand.Index }
        elseif ($operand -is [dnlib.DotNet.IMethod] -or $operand -is [dnlib.DotNet.IField] -or $operand -is [dnlib.DotNet.ITypeDefOrRef]) { $operand=$operand.FullName }
        $instruction.OpCode.Name+':'+[string]$operand
    }
    $locals=($method.Body.Variables | ForEach-Object {$_.Type.FullName}) -join ','
    $handlers=foreach($handler in $method.Body.ExceptionHandlers) {
        [string]$handler.HandlerType+':'+$instructions.IndexOf($handler.TryStart)+':'+$instructions.IndexOf($handler.TryEnd)+':'+$instructions.IndexOf($handler.HandlerStart)+':'+$instructions.IndexOf($handler.HandlerEnd)+':'+[string]$handler.CatchType
    }
    $locals+'|'+($parts -join '|')+'|'+($handlers -join '|')
}
$module=[dnlib.DotNet.ModuleDefMD]::Load((Resolve-Path -LiteralPath $AssemblyPath))
try {
    $types=@($module.GetTypes())
    $camera=$types | Where-Object FullName -eq 'GameCamera'
    Require ($null -ne $camera) 'GameCamera is missing'
    foreach($name in @('ApplyUpdate','UpdateFOV')) {
        $method=$camera.Methods | Where-Object Name -eq $name
        Require ($null -ne $method -and $method.HasBody) "Missing camera method: $name"
        $calls=@(Calls $method)
        Require (@($calls | Where-Object {$_ -eq 'System.Void UnityEngine.GeometryUtility::CalculateFrustumPlanes(UnityEngine.Camera,UnityEngine.Plane[])'}).Count -eq 1) "Reusable camera frustum call missing: $name"
        Require (!($calls -match 'UnityEngine.Plane\[\] UnityEngine.GeometryUtility::CalculateFrustumPlanes')) "Allocating frustum overload remains: $name"
    }
    $rocks=$types | Where-Object FullName -eq 'RocksExplosion'
    $rockStart=$rocks.Methods | Where-Object Name -eq 'Start'
    $rockCalls=@(Calls $rockStart)
    Require ($rockCalls -contains 'UnityEngine.Mesh UnityEngine.MeshFilter::get_sharedMesh()') 'Shared mesh read missing'
    Require (!($rockCalls -contains 'UnityEngine.Mesh UnityEngine.MeshFilter::get_mesh()')) 'Implicit rock mesh cloning remains'
    $water=$types | Where-Object FullName -eq 'FGOL.Water'
    $waterUpdate=$water.Methods | Where-Object Name -eq 'Update'
    Require (@((Calls $waterUpdate) | Where-Object {$_ -eq 'UnityEngine.Camera UnityEngine.Camera::get_main()'}).Count -eq 1) 'Water must query the current main camera once per update'
    Require (!($types | Where-Object {$_.Name -in @('HSEIntegralRoute','HSEIntegralAudit','HSEEditorFrameSample','HSEEditorRegression')})) 'Temporary Editor diagnostics leaked into player sources'
    $changed=@();$compared=0
    if ($BeforeAssembly -or $AfterAssembly) {
        Require ($BeforeAssembly -and $AfterAssembly) 'Supply both comparison assemblies'
        $before=[dnlib.DotNet.ModuleDefMD]::Load((Resolve-Path -LiteralPath $BeforeAssembly))
        $after=[dnlib.DotNet.ModuleDefMD]::Load((Resolve-Path -LiteralPath $AfterAssembly))
        try {
            $methods=[System.Collections.Generic.Dictionary[string,object]]::new([StringComparer]::Ordinal)
            foreach($type in $before.GetTypes()){foreach($method in $type.Methods){$methods[$method.FullName]=$method}}
            foreach($type in $after.GetTypes()){foreach($method in $type.Methods){
                Require ($methods.ContainsKey($method.FullName)) 'Unexpected added method'
                $compared++
                if ((BodyKey $method) -cne (BodyKey $methods[$method.FullName])) {$changed += $type.FullName+'::'+$method.Name}
                [void]$methods.Remove($method.FullName)
            }}
            Require ($methods.Count -eq 0) 'Unexpected removed method'
            $expected=@('GameCamera::.ctor','GameCamera::ApplyUpdate','GameCamera::UpdateFOV','RocksExplosion::Start','FGOL.Water::Update')
            Require ((($changed | Sort-Object) -join '|') -eq (($expected | Sort-Object) -join '|')) 'Unexpected method body changes; inspect private evidence'
        } finally { $before.Dispose();$after.Dispose() }
    }
    [pscustomobject]@{result='PASS';bufferedFrustum=$true;rockMeshCloning=$false;waterMainCameraQueries=1;temporaryPlayerDiagnostics=$false;comparedMethods=$compared;changedMethodBodies=$changed;hardware='UNTESTED'} | ConvertTo-Json -Depth 4
} finally { $module.Dispose() }

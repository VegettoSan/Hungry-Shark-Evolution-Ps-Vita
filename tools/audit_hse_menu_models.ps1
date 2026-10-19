#requires -Version 7.0
param([Parameter(Mandatory=$true)][string]$AssemblyPath,[string]$Dnlib='Tool Codex/dnSpyEx/bin/dnlib.dll')
# Read-only inspection of a locally supplied PSP2 game assembly. No code is executed.
$ErrorActionPreference='Stop'
$null=[Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $Dnlib))
function Need($ok,[string]$message){if(!$ok){throw $message}}
function Calls($m){@($m.Body.Instructions|Where-Object {$_.Operand -is [dnlib.DotNet.IMethod]}|ForEach-Object {$_.Operand.FullName})}
$module=[dnlib.DotNet.ModuleDefMD]::Load((Resolve-Path -LiteralPath $AssemblyPath))
try{
 $types=@($module.GetTypes());$gui=$types|Where-Object FullName -eq 'GUIShark';$create=$gui.Methods|Where-Object Name -eq 'CreateAttachPoints'
 Need ((Calls $create)-match 'HSEVitaMenuAnchors::Get') 'Anchor metadata path missing'
 Need (!((Calls $create)-match '::get_modelPrefab|::Instantiate')) 'Full player prefab still loaded/cloned by each selector'
 $anchors=$types|Where-Object FullName -eq 'HSEVitaMenuAnchors';Need ($null -ne $anchors) 'Anchor helper missing'
 $entry=$anchors.NestedTypes|Where-Object Name -eq 'Entry'
 Need (@($entry.Fields|Where-Object {$_.FieldType.FullName -match '^UnityEngine\.(GameObject|Transform|Material|Mesh|Object)$'}).Count -eq 0) 'Cache pins native resources'
 Need (@($anchors.Fields|Where-Object {$_.IsStatic -and $_.FieldType.FullName -eq 'HSEVitaMenuAnchors/Entry[]'}).Count -eq 1) 'Fixed anchor entry array missing'
 $get=$anchors.Methods|Where-Object Name -eq 'Get'
 Need ((Calls $get)-match 'Player::InitAttachPoint') 'Original attachment initialization no longer used on cache miss'
 $lab=$types|Where-Object FullName -eq 'LabCrane'
 $init=$lab.Methods|Where-Object Name -eq 'Initialise';$replace=$lab.Methods|Where-Object Name -eq 'ReplaceShark';$prepare=$lab.Methods|Where-Object Name -eq 'PrepareShark'
 Need (!((Calls $init)-match 'GUIManager::InstantiatePrefab')) 'All-nine eager model construction remains'
 Need ((Calls $replace)-match 'LabCrane::PrepareShark') 'On-demand preparation missing'
 Need (@((Calls $prepare)|Where-Object {$_ -match 'GUIManager::InstantiatePrefab'}).Count -eq 1) 'Unexpected special model creation sites'
 Need (@($prepare.Body.Instructions|Where-Object {$_.OpCode.Code.ToString() -eq 'Ldc_I4_5'}).Count -ge 1) 'Five-model bound missing'
 Need (@((Calls $prepare)|Where-Object {$_ -match 'UnityEngine.Object::Destroy'}).Count -eq 2) 'Evicted model/material cleanup missing'
 $destroy=$lab.Methods|Where-Object Name -eq 'OnDestroy';Need ((Calls $destroy)-match 'ReleasePreparedMaterials') 'Menu material cleanup missing'
 Need (!($types|Where-Object {$_.Name -in @('HSEMenuTiming','HSEMenuAudit','HSEEditorRegression','HSEEditorFrameSample')})) 'Temporary diagnostics leaked into player'
 [pscustomobject]@{result='PASS';anchorMetadataOnly=$true;originalPoseInitialization=$true;specialsOnDemand=$true;retainedModelLimit=5;temporaryDiagnostics=$false;hardware='UNTESTED'}|ConvertTo-Json
}finally{$module.Dispose()}

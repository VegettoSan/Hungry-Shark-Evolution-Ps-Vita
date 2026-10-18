#requires -Version 7.0
param([Parameter(Mandatory=$true)][string]$AssemblyPath,[string]$Dnlib='Tool Codex/dnSpyEx/bin/dnlib.dll')
# Local read-only metadata audit; never executes the recovered game assembly.
$ErrorActionPreference='Stop'
$null=[Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $Dnlib))
function Need($ok,[string]$reason){if(!$ok){throw $reason}}
function Calls($method){@($method.Body.Instructions | Where-Object {$_.Operand -is [dnlib.DotNet.IMethod]} | ForEach-Object {$_.Operand.FullName})}
$module=[dnlib.DotNet.ModuleDefMD]::Load((Resolve-Path -LiteralPath $AssemblyPath))
try {
    $types=@($module.GetTypes())
    $status=$types | Where-Object FullName -eq 'HSEVitaD10Status'
    Need ($null -ne $status) 'Save startup observer type missing'
    Need (!($status.Methods | Where-Object {$_.Name -in @('OnGUI','Update')})) 'D10 overlay rendering or timer still compiled'
    $boot=$status.Methods | Where-Object Name -eq 'Boot'
    $calls=@(Calls $boot)
    Need ($calls -match 'HSEVitaSaveDiagnostics::ObserveStartup') 'Existing startup save preservation/checks removed'
    Need (!($calls -match 'GameObject::.ctor|AddComponent|DontDestroyOnLoad|UnityEngine.GUI::')) 'D10 startup still creates a visual object'
    Need (!($types | Where-Object Name -eq 'HSEVitaInputDiagnostic')) 'Input diagnostic overlay still compiled'
    $input=$types | Where-Object FullName -eq 'HSEVitaInput'
    $inputCalls=@(Calls ($input.Methods | Where-Object Name -eq 'Bootstrap'))
    Need ($inputCalls -match 'AddComponent<FGOLControlManager>') 'Original controller manager bootstrap missing'
    Need ($inputCalls -match 'AddComponent<HSEVitaNavigation>') 'Controller navigation bootstrap missing'
    Need (!($inputCalls -match 'HSEVitaInputDiagnostic')) 'Input diagnostic still created'
    $overlays=@($types | Where-Object { $_.Name -match '^HSEVita.*(?:Status|Probe)$|^HSEStartupTrace$' } | Where-Object {$_.Methods | Where-Object Name -eq 'OnGUI'})
    Need ($overlays.Count -eq 0) 'Another port diagnostic overlay is compiled'
    Need (!($module.GetAssemblyRefs() | Where-Object Name -like 'UnityEditor*')) 'Editor dependency in runtime'
    [pscustomobject]@{result='PASS';D10OverlayCompiled=$false;D10VisualObjectCreated=$false;inputOverlayCompiled=$false;startupSaveChecksRetained=$true;controllerBootstrapRetained=$true;otherPortDiagnosticOverlays=$overlays.Count;physicalDisplay='UNTESTED'} | ConvertTo-Json
} finally {$module.Dispose()}

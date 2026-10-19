#requires -Version 7.0
param(
    [Parameter(Mandatory=$true)][string]$AssemblyPath,
    [string]$Dnlib = 'Tool Codex/dnSpyEx/bin/dnlib.dll'
)
# Read-only audit of a user-supplied private PSP2 assembly; executes no game code.
$ErrorActionPreference = 'Stop'
$null = [Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $Dnlib))
function Need($ok, [string]$message) { if (!$ok) { throw $message } }
function Calls($method) {
    @($method.Body.Instructions | Where-Object { $_.Operand -is [dnlib.DotNet.IMethod] } |
        ForEach-Object { $_.Operand.FullName })
}
$module = [dnlib.DotNet.ModuleDefMD]::Load((Resolve-Path -LiteralPath $AssemblyPath))
try {
    $types = @($module.GetTypes())
    $store = $types | Where-Object FullName -eq 'StoreMenuSheet'
    $refresh = $store.Methods | Where-Object Name -eq 'RefreshGameItemSlots'
    Need ((Calls $refresh) -match 'HSEVitaStoreDescriptions::GetEffects') 'UI metadata lookup missing'
    Need ((Calls $refresh) -match 'HSEVitaStoreDescriptions::IsAllZero') 'Trail visibility contract missing'
    $helper = $types | Where-Object FullName -eq 'HSEVitaStoreDescriptions'
    Need ($null -ne $helper) 'Description helper missing'
    $get = $helper.Methods | Where-Object Name -eq 'GetEffects'
    Need ((Calls $get) -match 'GameplayEffects::LoadRemoteConfiguration') 'Original remote overrides missing'
    Need ((Calls $get) -match 'ShopManager::GetGameplayEffectsForShopItem') 'Original fallback missing'
    $load = $helper.Methods | Where-Object Name -eq 'Load'
    Need ([int](($helper.Fields | Where-Object Name -eq 'MaximumEntries').Constant.Value) -eq 512) 'Entry bound changed'
    Need ([int](($helper.Fields | Where-Object Name -eq 'MaximumJsonBytes').Constant.Value) -eq 524288) 'JSON bound changed'
    Need ((Calls $load) -match 'JsonUtility::FromJson') 'Private metadata decoding missing'
    Need ((Calls $load) -match 'Resources::UnloadAsset') 'Source TextAsset release missing'
    Need ((Calls $load) -match 'StringComparer::get_OrdinalIgnoreCase') 'Resources path case contract missing'
    Need (!((Calls $load) -match '::Instantiate|::GetAccessoryPrefab')) 'Description loading still constructs models'
    $entry = $helper.NestedTypes | Where-Object Name -eq 'Entry'
    Need (@($entry.Fields | Where-Object { $_.Name -eq 'hasTrail' }).Count -eq 1) 'Trail-presence metadata missing'
    Need (!($types | Where-Object Name -in @('HSEMenuTiming','HSETransitionObserver','HSEGameplayCaptureFrames','HSEEditorRegression'))) 'Diagnostic code leaked into player'
    $callers = @()
    foreach ($type in $types) {
        foreach ($method in $type.Methods) {
            if (!$method.HasBody) { continue }
            if ((Calls $method) -match 'HSEVitaStoreDescriptions::GetEffects|HSEVitaStoreDescriptions::IsAllZero') {
                $callers += $method.FullName
            }
        }
    }
    Need ($callers.Count -eq 1 -and $callers[0] -match 'StoreMenuSheet::RefreshGameItemSlots') 'UI copies are being used outside store descriptions'
    [pscustomobject]@{ result='PASS'; uiOnly=$true; boundedCatalog=$true;
        remoteOverrides=$true; originalFallback=$true; sourceTextReleased=$true;
        diagnosticsInPlayer=$false; hardware='UNTESTED' } | ConvertTo-Json
} finally { $module.Dispose() }

#requires -Version 7.0
param(
    [Parameter(Mandatory=$true)][string]$AssemblyPath,
    [string]$Dnlib='Tool Codex/dnSpyEx/bin/dnlib.dll'
)
# Read-only metadata checks against the locally supplied compiled game assembly.
# No game sources, binaries, SDK files or resource dumps are emitted.
$ErrorActionPreference='Stop'
$null=[Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $Dnlib))
function Require($value,[string]$message) { if (!$value) { throw $message } }
function Calls($method) {
    @($method.Body.Instructions | Where-Object {$_.Operand -is [dnlib.DotNet.IMethod]} | ForEach-Object {$_.Operand.FullName})
}
$module=[dnlib.DotNet.ModuleDefMD]::Load((Resolve-Path -LiteralPath $AssemblyPath))
try {
    $types=@($module.GetTypes())
    $collector=$types | Where-Object FullName -eq 'AnalyticsFPSHeatmapRunner'
    Require ($null -ne $collector) 'Telemetry interface missing'
    $awake=$collector.Methods | Where-Object Name -eq 'Awake'
    Require (@((Calls $awake) | Where-Object {$_ -match 'RollingAverage::\.ctor|Heatmap::\.ctor'}).Count -eq 0) 'Unused telemetry allocations remain'
    Require (@((Calls $awake) | Where-Object {$_ -match 'set_enabled\(System.Boolean\)'}).Count -eq 1) 'Telemetry disable missing'
    $update=$collector.Methods | Where-Object Name -eq 'Update'
    Require (@(Calls $update).Count -eq 0) 'Telemetry Update still performs work'
    $start=$collector.NestedTypes | Where-Object {$_.Name -like '<Start>*'}
    $move=$start.Methods | Where-Object Name -eq 'MoveNext'
    Require ($null -ne $move) 'Telemetry iterator missing'
    Require (@((Calls $move) | Where-Object {$_ -match 'StartHeatmap|WaitForSeconds|set_enabled'}).Count -eq 0) 'Telemetry coroutine still starts sampling'
    $particles=$types | Where-Object FullName -eq 'ParticleManager'
    foreach($name in @('SpawnAttached','EmitBatchedParticles')) {
        $method=$particles.Methods | Where-Object Name -eq $name
        Require ($null -ne $method) "Particle entry point missing: $name"
        Require (@((Calls $method) | Where-Object {$_ -match '::ToString\(|::StartsWith\('}).Count -eq 0) "Particle name conversion remains: $name"
        Require (@((Calls $method) | Where-Object {$_ -match 'IsUserAChild'}).Count -eq 1) "Age policy changed: $name"
    }
    $ads=$types | Where-Object FullName -eq 'HSEAdManager'
    $init=$ads.Methods | Where-Object Name -eq 'Initialize'
    Require (@((Calls $init) | Where-Object {$_ -match 'Resources::|TextAsset::'}).Count -eq 0) 'Unused mobile ad configuration is still loaded'
    Require (@((Calls $init) | Where-Object {$_ -match 'AdManager::\.ctor'}).Count -eq 1) 'Ad session/getter contract manager missing'
    $startAds=$ads.Methods | Where-Object Name -eq 'StartEngine'
    Require (@(Calls $startAds).Count -eq 0) 'Mobile ad engine is reachable from PSP2 startup'
    $analytics=$types | Where-Object FullName -eq 'FGOLAnalytics'
    $analyticsInit=$analytics.Methods | Where-Object Name -eq 'Initialise'
    Require (@((Calls $analyticsInit) | Where-Object {$_ -match 'FGOLAnalyticsDNA::|FGOLAnalyticsAppsFlyer::'}).Count -eq 0) 'Mobile analytics sender is created'
    $facebook=$types | Where-Object FullName -eq 'FGOL.Facebook.FGOLFacebookFacade'
    $access=$facebook.Methods | Where-Object Name -eq 'GetAccessForCurrentPlatform'
    Require (@((Calls $access) | Where-Object {$_ -match 'DisabledFBAccess::\.ctor'}).Count -eq 1) 'Unavailable Facebook provider missing'
    Require (!($module.GetAssemblyRefs() | Where-Object Name -eq 'Facebook.Unity')) 'Facebook runtime assembly reference remains'
    $iap=$types | Where-Object FullName -eq 'FGOL.IAP.InAppPurchase'
    $iapInit=$iap.Methods | Where-Object Name -eq 'Initialize'
    Require (@((Calls $iapInit) | Where-Object {$_ -match 'HSEVitaUnavailableIAPProvider::\.ctor'}).Count -eq 1) 'Unavailable IAP provider missing'
    $dna=$types | Where-Object FullName -eq 'DNAManager'
    $dnaInit=$dna.Methods | Where-Object Name -eq 'Initializate'
    Require (@(Calls $dnaInit).Count -eq 0) 'DNA native initialization or save listener remains'
    $loader=$types | Where-Object FullName -eq 'GUITextureLoader'
    $load=$loader.Methods | Where-Object Name -eq 'LoadTexture'
    Require (@((Calls $load) | Where-Object {$_ -match 'Material::\.ctor'}).Count -eq 1) 'Unexpected material construction sites'
    Require (@((Calls $load) | Where-Object {$_ -match 'Material::set_shader'}).Count -eq 1) 'Existing icon material reuse missing'
    Require (@((Calls $load) | Where-Object {$_ -match 'MakePixelPerfect|CR_RefreshOnLoad'}).Count -eq 2) 'Icon refresh/size behavior missing'
    Require (!($types | Where-Object {$_.Name -in @('HSEResourceAudit','HSEEditorRegression','HSEEditorFrameSample','HSEIntegralRoute')})) 'Temporary diagnostics leaked into player assembly'
    [pscustomobject]@{result='PASS';telemetrySampling=$false;particleNameConversions=$false;adConfigResourceLoad=$false;iconMaterialReuse=$true;mobileSendersCreated=$false;facebookUnavailable=$true;iapUnavailable=$true;dnaNativeInit=$false;hardware='UNTESTED'} | ConvertTo-Json
} finally { $module.Dispose() }

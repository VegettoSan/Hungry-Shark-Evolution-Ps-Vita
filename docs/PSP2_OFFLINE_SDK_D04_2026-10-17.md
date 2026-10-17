# D04: Facebook incompatible y servicios móviles fuera de Vita

Fecha: 2026-10-17 UTC / 16 de octubre local. Encargo expreso del propietario de retirar Facebook y servicios innecesarios del destino Vita; no cambia P00 ACTIVE ni aprueba fases.

## Causa demostrada del bloqueo actual

**Actualización posterior del propietario:** D04 supera el bloqueo Facebook, completa LaunchFrontEnd/Init y llega a Master_low. Se observa fallo FMOD y crash durante carga. El [seguimiento D05](PSP2_LOAD_AUDIO_INPUT_D05_2026-10-17.md) documenta esos problemas independientes; no constituye aprobación de gameplay.

**VERIFIED por captura de la Vita suministrada por el propietario:** D03 muestra `target=60 vsync=1 Update/s=60,1`, `persistent OK` y `ux0 DirectoryNotFoundException`. Las últimas etapas son `08 Ads init > 09 GameCenter init > 10 Social init > 11 Facebook init`. Error: **`NotSupportedException: The facebook sdk does not support this platform`**. Hay fondo original visible, sin botones del menú. SHA-256 de la captura privada: `55bfd03760d30f4199e96389f4f4d46d1c7ad957a4a0ca355768fef451520b87`.

**VERIFIED estático:** `King.Initializate_Start()` llama a `Facebook_Initialize()` antes de `CloudSaveFacade.Instance.Load(true)`. Esa función llama a `FGOLFacebookFacade.Initialise()`, cuya fábrica `GetAccessForCurrentPlatform()` devolvía incondicionalmente `OfficialFBAccess`. `OfficialFBAccess.Initialise()` llama a `Facebook.Unity.FB.Init`. En la DLL instalada `Facebook.Unity.dll`, el método FB.Init contiene exactamente el mensaje observado. La excepción aborta esa corrutina antes de solicitar la carga que conduce a LaunchFrontEnd. No es evidencia de otro fallo de shaders; tampoco demuestra que no haya otros problemas posteriores.

El contador de la captura confirma el límite observado en esa pantalla, **no 60 FPS de gameplay ni la cadencia de presentación física**. El acceso directo System.IO a ux0 de D03 falló en hardware: D04 retira ese intento y conserva la ruta persistente y la franja/TTY. La copia binaria recibida anteriormente no se reinterpreta como log de texto.

## Corrección por destino

Las condiciones `UNITY_PSP2` incluyen el editor con PSP2 seleccionado para poder probar estos contratos sin SDK móvil. Las ramas de otros destinos permanecen. No se usa IsConsole como sustituto global: PlatformUtils conserva su selección original de calidad, dispositivo e input.

| Servicio / punto de entrada | D04 PSP2 | Conservado / contrato |
| --- | --- | --- |
| Facebook: FGOLFacebookFacade.GetAccessForCurrentPlatform / FacebookEnabledOnPlatform | Selecciona el **DisabledFBAccess ya recuperado**; capacidad de Facebook falsa | Init/login/token responden Failed; sin permisos, sesión o bonus social simulados |
| OfficialFBAccess y cuatro PluginImporter | Clase fuera de compilación PSP2; exclusión PSP2 de Facebook.Unity, Android, Canvas e IOS | Fuentes/DLL/meta originales respaldadas; GUID preservados y otros destinos conservados. Facebook.Unity.Settings queda para la identidad del recurso original de ajustes, sin referencia del juego al runtime Facebook |
| Google Play: PlayGamesClientFactory.GetPlatformPlayGamesClient | Selecciona DummyClient existente en vez de NativeClient/AndroidClient | Autenticación del cliente devuelve false; no es una integración PSN ni se conceden logros online. Managers/datos locales de logros siguen presentes |
| Compras: InAppPurchase.Initialize | [Proveedor Vita no disponible](../tools/unity/HSEVitaUnavailableIAP.cs), sin construir GooglePlayGamesIAPProvider | Cinco operaciones completan con fallo, arrays vacíos o recibo nulo. No se usa DummyIAPProvider, que simula compras exitosas. Identificadores y transacciones/historial originales conservados |
| Verificación de compras | Proveedor no disponible para las tres rutas de verificación | Devuelve AwaitingVerification: no valida ni invalida recibos por ausencia del servicio; sin peticiones a Google/Ubisoft/Houston |
| Anuncios: HSEAdManager.AreAdsAllowed / StartEngine | Failure y motor sin arrancar | CanShow continúa falso; AllowAd/ShowAd usan el callback original de fallo. No recompensas falsas ni silenciamiento de audio al intentar anuncios |
| Analytics: FGOLAnalytics.Initialise | Lista de emisores vacía; no crea DNA/AppsFlyer | Contadores/estadísticas locales originales siguen calculándose. Las protecciones Android previas de DNAManager/AppsFlyer se conservan, sin repetir reparaciones |
| CloudSaveFacade | Capacidad de nube falsa, usuario existente carga con LocalLoad, autenticación/ping no disponibles | **SaveGameManager.Save() original sigue antes del límite de subida**. Mismo usuario, fichero, serialización y progreso; no se resetean preferencias. Login explícito de nube no cambia identidad ni flags; no upload/sync |
| RemoteConfigurationFacade.LoadRemoteConfig | Completa callback false sin invocar proveedores remotos | Conserva carga local/caché y sus datos; no sustituye parámetros del juego por un JSON inventado |
| LiveEventsFacade | IsDisabled verdadero y sin petición de reloj remoto | El contrato existente RequestActiveEvent completa false/null; sin premios online ficticios. Datos persistidos conservados |
| DeepLink / IronSource / SDK DNA y AppsFlyer | Protecciones Android existentes auditadas | No se sustituyen otra vez. Adaptadores/tipos conservados para evitar romper referencias |

Esto retira los runtimes de Facebook del player y desactiva las entradas de servicios móviles examinadas. **No significa que se haya eliminado toda clase auxiliar de SDK de Assembly-CSharp/firstpass.** Permanecen tipos recuperados y dependencias compartidas que no deben borrarse indiscriminadamente. No se ha prometido una implementación de tiendas, nube, PSN, anuncios o servicios online. No se modifica la progresión offline ni se desbloquea contenido comprado.

## Pruebas y límites

[HSEPSP2OfflineAudit.cs](../tools/unity/HSEPSP2OfflineAudit.cs) usa la API SetExcludeFromAnyPlatform(BuildTarget.PSP2, true), cuya presencia se verificó en UnityEditor.dll de **2018.2.19f1**. Los cuatro importadores conservaron Any/Editor/Android/Windows/iOS respecto de su estado previo; su única exclusión intencional nueva es PSP2. No se descargó ni actualizó un SDK.

**VERIFIED, contratos nativos dentro del editor / PSP2:** fábrica Facebook devuelve DisabledFBAccess; init/login/token completan una vez con fallo; sin sesión/permisos/token. Proveedor de compras: cinco callbacks de fallo, inventario/transacciones vacíos, recibo nulo e identidad de producto intacta; verificador conserva AwaitingVerification. El ensamblado del juego no referencia Facebook.Unity. Estas pruebas no ejecutan escenas, edad, red, tiendas ni guardados. **No equivalen a gameplay en hardware.**

D04 conserva temporalmente los marcadores acotados de King y el registro de excepciones de D03 para verificar el siguiente paso en consola. Solo los mensajes esperados de tienda no disponible se anotan como EXPECTED_UNAVAILABLE y no ocupan la primera excepción de la franja; no se suprimen otras excepciones. Sin A/B/C, cambios de cámara o materiales. El límite de FPS D03 permanece.

**VERIFIED build PSP2 / Mono2x:** Succeeded, **0 errores / 294 advertencias**, 2 min 42,20 s; totalSize 223.967.524 bytes. Ocho advertencias nuevas CS0162 corresponden a las salidas PSP2 antes de los cuerpos de nube/eventos/configuración que se conservan para otros destinos; no son errores nuevos de shader. La inspección IL verifica que esas rutas del player no llaman a los servicios, y que SaveGameManager.Save sigue presente. No se deshabilitaron diagnósticos del compilador.

**VERIFIED player real:** las cuatro DLL Facebook runtime ausentes; OfficialFBAccess fuera de compilación y sin referencia Facebook.Unity desde el juego. Fábrica Google sin construcción de NativeClient/AndroidClient; IAP sin construcción del proveedor Google/verificador Ubisoft; Analytics sin emisores DNA/AppsFlyer; guardado local presente sin llamadas Upload/Authenticate, corrutina de carga sin Authenticate/CloudLoad; StartEngine sin corrutina de anuncios; configuración remota sin invocar proveedores. Comprobación IL PASS, no ejecución en Vita. Censo de recursos: 720 texturas, **cero BC7**, preservando la corrección anterior.

**VERIFIED paquete privado:** `Reports/OfflineSDK_20261017/Build/HSE_Offline_D04_60fps_HGSKE1178.vpk`, **117.016.991 bytes**, SHA-256 **`efa25b239a8621f0cb1c064099d2a1ad79cb9a46127da39a7e7131252fd942c6`**. CRC y comparación exacta de miembros runtime con el player pasan; solo SFO cambia por identidad/etiqueta de prueba. No se publica ni despliega automáticamente.

## Evidencia privada y reversión

`Reports/OfflineSDK_20261017/` conserva BeforeManifest.json de Assets/ProjectSettings, Before de las fuentes/meta, Changes.json con hashes, captura privada D03_device.png, PluginExclusions.txt, Contracts.txt, NativeIL.json, BuildResult/PackageReceipt, Editor_D04_build.log e IntegrityAfter.json. **VERIFIED:** entre 19.615 archivos iniciales solo cambiaron los nueve scripts previstos y cuatro metas de exclusión; único añadido permanente: proveedor IAP y su meta. GUID de los plugins preservados; ninguna escena/settings/textura/malla/lightmap original cambió ni falta. Los 19.591 archivos de la referencia 5.6 siguen idénticos, sin añadidos. Otras 18 comparaciones estáticas de tokens confirman que las ramas de esos nueve scripts para Windows Editor y Android conservan su contenido; no son pruebas runtime de esos destinos. Fuentes recuperadas, captura, SDK y VPK no se incorporan al repositorio público.

Para revertir únicamente D04: restaurar los nueve scripts y cuatro metas de PluginImporter desde Before y retirar el proveedor HSEVitaUnavailableIAP.cs con su meta. Mantener las correcciones anteriores de caché/shader y FPS. **El auditor, constructor, recurso y marcadores temporales ya se retiraron del proyecto después del build**; sus copias y el VPK permiten repetirlo. King.cs quedó idéntico a su estado previo. Ninguna escena, textura, modelo, lightmap, GUID, save o preferencia requiere borrado/restablecimiento.

## Prueba en consola

1. Instalar D04 sobre HGSKE1178 sin desinstalar ni borrar guardados. D03 se conserva como archivo de respaldo local.
2. Observar el paso de `11 Facebook unavailable (Vita)` a `12 Save load`, callback 13, LaunchFrontEnd 14 e Init completed 15. Si queda el fondo, registrar la última etapa y el error nuevo; no atribuirlo de nuevo a Facebook sin leerlo.
3. Comprobar botones/selector originales, entrada al juego, modelo/escenario/HUD, movimiento, boost, pausa y regreso. La franja es temporal; no reemplaza la UI original.
4. Comprobar que anuncios/compras/login no bloquean ni conceden premios; cerrar y volver a abrir para comprobar persistencia del progreso. No resetear edad, opciones o saves para forzar pruebas.

**Pendiente de hardware:** D04 y el recorrido posterior al bloqueo Facebook. Audio, estabilidad, memoria, todas las pantallas y lightmaps pendientes mantienen el alcance de los informes anteriores.

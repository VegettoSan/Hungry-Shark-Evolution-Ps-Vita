# Pantalla negra PSP2: caché de texturas y prueba D02

Fecha: **2026-10-17 UTC / 16 de octubre local**. Base Git `b13c603`. Encargo expreso del propietario: diagnosticar/reparar la copia PSP2 existente y generar un VPK privado de prueba. **P00 ACTIVE; ninguna aprobación ni transición de fase.**

## Hallazgo y alcance

**Actualización del propietario posterior a D02:** en **Original** se ven el fondo y el diálogo original de edad, operable con controles y táctil. Después de confirmar queda **solo el fondo**, sin botones del menú. Esto confirma render parcial en hardware según el propietario; no confirma el menú completo ni gameplay. El resultado sustituye el estado pendiente de hardware de D02 descrito históricamente al final de este informe. La corrección de caché/variante queda respaldada por esa recuperación visual, pero no se aisló cuál de las dos explica cada elemento. Véase [seguimiento del arranque y límite de FPS](PSP2_STARTUP_D03_2026-10-17.md).

**VERIFIED:** el último VPK local anterior, SHA-256 `a9f2e0f5464c39c0c755d37bee8b903cd224ccc1bbf0411bcb7de223cd9a7b7d`, contiene **671 texturas BC7, cinco DXT1 y cuatro RGBA32** en sus archivos `resources.assets` y `sharedassets*.assets`. Entre las BC7 están todos los atlas `GUIAtlas_*`, `menu-backdrop`, `caustics` y `water-midFE`. Se inspeccionaron los objetos con AssetStudio 0.16.47 ya instalado; UnityDataTool 2.4.0 no admite estos SerializedFiles v17 y se descartó para esta lectura. No se reextrajo el APK. **UNTESTED:** igualdad de ese hash con la instalación física descrita por el propietario; no se recibió un log/hash del dispositivo.

**VERIFIED:** había una discrepancia entre la caché importada y la selección del módulo PSP2. `TextureImporter.GetAutomaticFormat("PSP2")` devuelve DXT1 para `menu-backdrop.png` y DXT5 para los atlas/cáusticas, mientras `Texture2D.format` devuelve BC7. Una reimportación `ImportAssetOptions.ForceUpdate`, **sin cambiar opciones**, produce DXT1/DXT5 respectivamente. Los cuatro `.meta` permanecen idénticos. La misma operación dirigida se completó para las 667 texturas restantes: dos fallbacks NPOT observados antes del lote final de 665. Los **671 `.meta` no cambiaron**.

**INFERRED:** esa caché de escritorio empaquetada para PSP2 es una explicación fuerte de que se vean partículas con textura RGBA32 y no los atlas/modelos con BC7. **No se ha demostrado todavía en hardware que sea la única causa de la pantalla negra.** El VPK nuevo y el log del player permiten comprobarlo.

**VERIFIED:** los shaders representativos `Unlit/Transparent Colored` y sus tres variantes de clipping, `Depth`, `HSE/RimLit`, `HSE/ActionLineCaustics`, `HSE/BackgroundFish` y `Custom/PostProcess/Distortion` sí contienen programas `ConsoleVS/ConsoleFS` para PSP2 en el VPK anterior. RimLit y ActionLineCaustics contienen ocho variantes de vértice y fragmento. Eso descarta su ausencia general del paquete, no una incompatibilidad de ejecución ni toda posible variante ausente.

**VERIFIED:** `Unlit/Transparent Colored (Packed)` conserva programas GLES y produce `Pass '' has no vertex shader` / `Setting to default shader`. No se encontró ningún material del VPK que lo use; no se modificó por conjetura. Las reconstrucciones Cg examinadas se identifican expresamente como traducciones de programas GLES; no son las fuentes originales.

**VERIFIED (nuevo build D01):** dos errores PSP2 de `HSE/ActionLineCaustics_BlendTextures`, `Program 'frag'/'vert', Missing_original_shader_variant at line 1722 (on psp2)`, corresponden a `UNITY_PASS_FORWARDBASE LIGHTMAP_ON HIGH_END LIGHTMAP_OFF`. Dos pragmas independientes generaban estados imposibles que ninguna de las ocho ramas recuperadas admite. Se sustituyó **solo ese par** por `#pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON`, como ya hacía el shader base reparado. Se conservaron cuerpos, propiedades, estados, fallback, GUID/meta y el `#error` de protección. D02 compila con cero errores y contiene ocho programas ConsoleVS y ocho ConsoleFS de ese shader. No se atribuye por sí solo a este error el fallo de toda la UI.

## Línea base y rutas

**VERIFIED:** los `ProjectVersion.txt` de `HSE642_Unity2018_ReadyToOpen` y `HSE642_Unity56_ReadyToOpen` indican **2018.2.19f1** y **5.6.4p3**. El módulo instalado registra `432399 / 06990f28ba00`, PSP2 está disponible y el backend de scripts es Mono2x. La referencia 5.6 se mantuvo de solo lectura.

**VERIFIED (configuración):** la primera escena habilitada es `Assets/WIP/SplashLoaderScene.unity`; `SplashLoader.loadNextScene()` carga `MainMenu` aditivamente y destruye la cámara/GUITexture de splash tras el fundido. El manifiesto incluye las 18 escenas recuperadas. **UNTESTED:** la secuencia efectivamente recorrida en la Vita de esta prueba hasta recibir su log.

| Elemento | Visibilidad en Vita | Renderer/ruta | Cámara / destino | Material / shader | Textura / keywords |
| --- | --- | --- | --- | --- | --- |
| Partícula original identificada `BackgroundCamera/sfx_part_seamotion` | Sí, según propietario; identidad exacta del efecto visible pendiente de log | ParticleSystemRenderer, billboard | `BackdropCamera`, capa 0, pantalla | `Default-Particle` / `Particles/Alpha Blended Premultiply` | RGBA32 64×64; programas PSP2 presentes |
| Recuadro amarillo | Sí, según propietario | IMGUI, cuatro `GUI.DrawTexture` en `HSEVitaNavigation.OnGUI()` | Coordenadas calculadas con cámara NGUI; dibujo IMGUI posterior | Material interno IMGUI; no se inventa una asignación | `Texture2D.whiteTexture` |
| UI original | No correctamente, según propietario | NGUI `UIPanel` → `UIDrawCall` → MeshRenderer; no Canvas en la escena examinada | `GUIRoot/Camera`, depth 0, máscara 393216, pantalla | `GUIAtlas_SD/HD*`; `Unlit/Transparent Colored`, clipping dinámico; pase `Depth` en los paneles indicados | Atlas BC7 en VPK anterior; UV0 y color de vértice; variantes de clipping en Resources |
| Modelo `PF_EnemySharkReef` | No, según informe general del propietario; este objeto concreto sin confirmar | SkinnedMeshRenderer en prefab original | Cámara efectiva pendiente de player | `reef-sharkB.mat` / `HSE/RimLit` | `reef-sharkB.png`; HIGH_END/LOW_END elegidos en runtime |
| Escenario representativo `menu-backdrop` | No, según propietario | MeshRenderer, 11.000 vértices, un submesh, capa 29 | `BackgroundCamera`, depth −2, máscara 536870912, pantalla | `menu-backdrop.mat` / `HSE/ActionLineCaustics` | Fondo y cáusticas BC7 en VPK anterior; lightmapIndex −1 en editor |

**VERIFIED (editor):** Built-in/Forward, Gamma, calidad Fastest/0, `masterTextureLimit=1`. `BackdropCamera` tiene depth −1 y máscara 205553665. Las tres cámaras examinadas tienen viewport completo y `targetTexture=null`. No había `PostProcess_Distortion` activo en ellas durante esa lectura. `PlatformUtils.Awake()` selecciona Fastest/SD y `deviceType` activa HIGH_END/LOW_END; no se debe inferir la calidad/keyword efectiva de Vita a partir del archivo de settings. El índice PSP2 serializado es 2 aunque solo existen dos niveles; no se cambió sin medición de ejecución.

**VERIFIED:** `UIDrawCall.UpdateMaterials()` conserva el atlas y busca por nombre HardClip/AlphaClip/SoftClip; el contrato usa UV0/color de vértices y, donde corresponde, rango/sharpness de clipping. Los materiales examinados mantienen `_MainTex` y el shader esperado. No se cambiaron Shader.Find, keywords de runtime, inclusión global, cámaras, materiales originales ni código de gameplay. No se encontraron indicios en esta ruta de bundles externos usados para los atlas examinados.

## Corrección y pruebas

La corrección es **regenerar exclusivamente la caché BC7 con PSP2 activo**. [Herramienta reproducible](../tools/unity/HSEPSP2TextureCacheRepair.cs): audita cada TextureImporter, comprueba el formato PSP2 automático, respalda su `.meta`, reimporta y exige igualdad de hashes. No asigna formatos, tamaños, sRGB, mipmaps ni opciones de alfa. La documentación **instalada** `PSVitaTipsForMaximizingAvailableMemory.html`, apartado Texture Size, explica que una textura NPOT comprimida requiere un fallback/copia; se conservaron los fallbacks RGB24/RGBA32 elegidos por el importador. No se hizo una conversión general a RGBA32/PVRTC.

| Prueba | Cambio / pregunta | Resultado observado | Decisión |
| --- | --- | --- | --- |
| VPK anterior | Leer programas y texturas realmente empaquetados | BC7 en atlas/modelos; programas PSP2 presentes en shaders seleccionados | Investigar importación antes de sustituir shaders |
| Cuatro texturas | ForceUpdate, sin cambiar settings | BC7 → DXT1/DXT5; cuatro meta idénticos | Reimportar solo los otros BC7 |
| Primeros intentos del lote | Validación demasiado estricta de formato esperado | Se detuvo tras NPOT RGBA32 y luego RGB24; ambos meta idénticos | Preservar intentos; aceptar solo fallback NPOT documentado |
| Lote final | 665 BC7 restantes | 373 DXT1, 288 DXT5, cuatro RGBA32; 665 meta idénticos | Compilar y verificar archivo resultante |
| Controles de diagnóstico | Quad con material UI magenta y material opaco cian, cámara/RT controlados | Lectura de píxeles PASS en editor, exactamente magenta/cian | Controles listos para prueba independiente en Vita |
| Build D01 | Caché corregida + sonda | Estado Succeeded, **dos errores de shader**, 288 advertencias; no entregado como VPK | Corregir únicamente la combinación de lightmap demostrada |
| Build D02 | Añade el pragma excluyente | **Succeeded, cero errores, 286 advertencias**, 2 min 3,66 s | Empaquetar prueba; hardware pendiente |
| Inspección D02 | Leer los archivos generados y comparar el VPK con ellos | **Cero BC7**; atlas/cáusticas DXT5, fondo DXT1; shaders de control y juego incluidos | Probar Original primero en consola |

Los controles del editor se ejecutaron con Direct3D11 `Emulated (PSVita)`: **no ejecutan el backend gráfico de la consola**.

La sonda [HSERenderProbe.cs](../tools/unity/HSERenderProbe.cs) solo se activa con `Resources/HSE_RenderDiagnostics.txt`. Conserva la escena original y permite volver a Original tras cada prueba. Registra BOOT, escenas, tres capturas acotadas por cambio de escena, hasta doce errores distintos, cámaras, canvases, renderers representativos, materiales/keywords/isSupported, texturas, calidad/API y lightmaps/probes. Usa `UnityEngine.Debug.Log` explícito: el `Debug` propio del juego puede suprimir mensajes. También intenta un archivo `HSE_Render_<UTC>.txt` en la ruta real de `Application.persistentDataPath`; si falla, lo muestra y mantiene TTY. Esa escritura en Vita sigue **UNTESTED**. No consulta `Texture2D.isReadable`, ausente en esta API.

En D02 los botones táctiles inferiores permiten: **Original**, **A: sin efecto** (solo PostProcess_Distortion activo), **B: UI sólida** (un UIDrawCall magenta sin atlas/color de vértices), **B: atlas** (mismo elemento con atlas/color original), **C: malla** (una malla original cian). Son controles condicionales para usar solo si Original sigue negro. Los materiales de control están referenciados individualmente; no se incluyen globalmente todos los shaders. La prueba B no conserva las máscaras de clipping: sirve para aislar geometría/atlas en un único drawcall, no para sustituir la UI de producción.

## Iluminación eliminada: inventario dirigido

| Recurso anterior | GUID | Estado / consumidores encontrados |
| --- | --- | --- |
| `WIP/MainMenu/Lightmap-0_comp_light.texture2D` | `800a3dd3ee7fd164995d8718db5f2e25` | Falta; RGB24 512×512, diez mips; referencia textual en `Resources/guiprefabs/SecretMenu.prefab` |
| `WIP/MainMenu/Lightmap-1_comp_light.texture2D` | `e303e84edd06f794b925bf1189873868` | Falta; RGB24 32×32, seis mips; sin referencia textual encontrada |
| `WIP/Master_spawn_high/Lightmap-0_comp_light.texture2D` | `b4e0850998c9a034ea6a15f87c4f14fa` | Falta; ETC_RGB4 2048×2048, doce mips; referenciada por los seis LightingData de Master/spawn low/medium/high |
| `WIP/titanic/Lightmap-0_comp_light.texture2D` | `1879921331a262a45b2157f79ec1b23f` | ETC_RGB4 512×512, diez mips; remapeo anterior a PNG ya documentado para Titanic/VolcanoIsland |
| `WIP/MainMenu/LightingData.asset` anterior | `e873f64598a7b31419fca2dac94275d2` | Es un **LightProbes vacío**, sin posiciones/coeficientes; no es por su nombre un horneado recuperable |
| `WIP/titanic/LightingData.asset` anterior | `10de1715eca4a9d4c9e786fbd7b27eea` | **Movido**, no eliminado: mismo GUID en `WIP/ttitanic/LightingData.asset` |

Se recuperaron copias exactas de los cuatro originales y sus `.meta`, más el asset vacío de probes y el `LightingData_0.asset` anterior, en el informe privado `RecoveredLightingOriginals/`, fuera de `Assets`. El escaneo textual **no cubre referencias dentro de assets binarios**. En la escena MainMenu examinada hay dos slots de lightmap nulos y probes nulos. El `LightingData_0.asset` anterior, GUID `949764cb84d6b504da1e3fd2e6ec0d7f`, demuestra el orden original: slot 0 → `800a3...`, slot 1 → `e303...`; ambos sin dirección ni shadowmask. Su versión actual es binaria. Además, el asset recuperado anterior contiene `sh[11]: NaN` y un array vacío de renderers lightmapped: **no procede restaurarlo entero sin validar sus datos**, ni asumir un horneado/probes completos por su nombre. Los archivos `HSE_PSP2_*` de las 06:53–07:22 UTC describen una reparación previa de Titanic, no el estado completo ni una causa demostrada del menú negro.

**Decisión:** conservar/restaurar datos y referencias, con conversión dirigida de las dos ETC1 y verificación de codificación/canales antes de activarlas; remapear únicamente los slots demostrados, sin trasplantar los coeficientes dañados. **No rebake** como primer paso: existen las texturas fuente y no se ha demostrado necesidad. No se borró iluminación ni se regeneró. Su restauración activa queda separada del VPK D02 para aislar las correcciones prioritarias. Audio no modificado; la recuperación 5.6 ya documentó silencio por una opción interna.

## Evidencia privada y reversión

La copia 2018 conserva `Reports/BlackScreen_20261017/`: manifiestos de 19.611/19.591 archivos antes de intervenir, respaldo del Editor.log, cuatro reimports dirigidos, planes/resultados/primeros intentos, `.meta` previos, control por lectura de píxeles, recursos de iluminación recuperados y recibos de build. Los inventarios del VPK anterior quedan en el espacio local ignorado `Tool Codex/HSEBlackScreen/`. No se publica código/recursos del juego, players ni SDK.

La reimportación no cambia archivos autorales que haya que revertir. La comparación de los **19.611 archivos** iniciales de 2018 muestra **un único cambio**: el par de pragmas de `HSE_ActionLineCaustics_BlendTextures.shader`. Los **19.591 archivos** de la referencia 5.6 siguen idénticos, sin archivos nuevos ni ausentes. La copia anterior del shader/meta y sus hashes están en `Before_BlendShader/`; restituir ese shader revierte la corrección de variantes. [Aplicador reproducible](../tools/fix_hse_lightmap_variants.py).

El VPK anterior se conserva intacto como baseline. Después de compilar D02 se trasladaron los siete archivos de la sonda/auditor/controles y sus `.meta` a `DiagnosticFiles/` fuera de Assets. **No queda instrumentación activa en el proyecto de trabajo.** Permanece únicamente la herramienta de editor para reimportar caché PSP2. Los controles del VPK de diagnóstico no son una corrección de producción.

El empaquetado de prueba usa [port_identity.json](../config/port_identity.json). [Script de empaquetado/identidad](../tools/set_vpk_test_identity.py): empaqueta los mismos directorios de runtime que el VPK anterior, convierte el nombre del SELF a `eboot.bin` sin alterar sus bytes y modifica solo `sce_sys/param.sfo`. Verifica CRC y compara todos los demás miembros byte por byte con el player generado. Omite símbolos y configuración/rutas del host. Su CONTENT_ID es un marcador local de diagnóstico; no se registra como identificador comercial ni aprobación de distribución. El VPK anterior usa `ABCD12345`; D02 usa `HGSKE1178`, por lo que debe probarse como título separado sin desinstalar el anterior. No se copiaron ni borraron perfiles/guardados.

## Artefacto y comprobación en Vita

VPK privado: `HSE642_Unity2018_ReadyToOpen/Reports/BlackScreen_20261017/Build/HSE_Render_D02_HGSKE1178.vpk`, **117.194.348 bytes** (111,77 MiB), 217 miembros, CRC válido. SHA-256: **`1a5fa5a2fc1fb9b5c84ac2807ab6f11c3a4cd716336971bb22a97099e5fa1982`**. El player contiene la bandera D02 y la clase de sonda. El censo de sus resources/sharedassets contiene 380 DXT1, 292 DXT5, 13 RGBA32, un RGB24, 33 ARGB4444 y un Alpha8; **ningún BC7**. Los controles IMGUI añaden dependencias de skin; ese total no representa memoria residente ni medición de VRAM.

1. Instalar D02 como `HGSKE1178` y abrir **Hungry Shark Evolution Vita**. Conservar el título anterior.
2. Dejar **Original** seleccionado. Esperar 25–30 segundos y comprobar splash, atlas/texto/botones del menú, fondo, selector de tiburones y una entrada a gameplay. Confirmar también que siguen los controles positivos existentes.
3. Si Original vuelve a ser negro, usar **A** una vez; volver a Original. Después **B: UI sólida** y **B: atlas**, anotando si aparece geometría magenta y luego imagen del atlas; volver a Original. Usar **C: malla** y anotar qué objeto cian aparece. No combinar modos. Si no existe un componente/objeto adecuado, el log dice que no cambió nada.
4. Recuperar `HSE_Render_<UTC>.txt` del almacenamiento persistente del título, si la escritura funcionó; la sonda registra la ruta devuelta realmente por Unity y conserva mensajes `[HSE_RENDER]` en TTY. Si la escritura falla, aparece el error en la franja inferior. Una foto de Original y de la primera prueba que cambie la imagen permite elegir el siguiente experimento.

**Estado final:** correcciones estáticas revisadas, recompilación PSP2 y empaquetado verificados. **UNTESTED en Vita:** instalación/arranque de D02, render correcto, ruta/escritura del log, comportamiento de sus controles temporales, audio y memoria. El propietario confirmó síntomas anteriores; no se declara solucionada la pantalla negra sin esa prueba física.

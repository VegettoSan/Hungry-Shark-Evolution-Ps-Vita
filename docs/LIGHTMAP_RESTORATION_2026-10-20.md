# Restauración de lightmaps — 20 de octubre de 2026

Encargo del propietario del **20 de octubre local / 21 UTC**, sobre `main`, base
`4054ce2`. Excepción específica para investigar y reparar la copia Unity 2018;
no aprueba fases. P03 continúa ACTIVE, con 3/13 aprobadas. Entrada de referencia:
APK 6.4.2 SHA-256
`52b7d8754ab8fec75c3b656a0dc5ec8dc849565adc402487a3ac093f6a481f7a`.

**VERIFIED:** se recuperaron cuatro atlas, conservando GUID, fileID, dimensiones,
mips, índices y escalas originales. No se hizo ningún bake. Los dos RGB24 son
idénticos a la referencia; los dos ETC1 se decodificaron y recomprimieron a DXT1
con herramientas ya instaladas. No se sustituyó ningún shader, material o malla.

**VERIFIED:** el laboratorio usa el prefab `SecretMenu`, no una escena de
iluminación independiente. Recuperar su textura era necesario, pero Unity 2018
retenía además `CombinedDirectional` sin textura direccional y mostraba las
paredes blancas. La prueba A/B cambió solamente el modo global y recuperó las
paredes azules, las luces rojas y sus detalles. El helper existente
`PrefabLightmapData.Awake` ahora conserva el modo Non-Directional al anexar atlas
cuando ninguna entrada contiene una textura direccional. Respeta datos
direccionales reales; no cambia el algoritmo de anexado, renderizadores, índices
o coordenadas. Dos entradas sucesivas al laboratorio validaron la corrección
automática, sin ejecutar el comando temporal que cambiaba el modo.

**VERIFIED:** Unity 5 original se usó exclusivamente mediante lecturas. Una
comparación con su editor se ejecutó en una **copia mínima independiente** de
cuatro assets; nunca se abrió el proyecto de referencia. Sus 19.591 archivos
de `Assets` y `ProjectSettings` se comprobaron por SHA-256.

## Escenas y resultados

En la tabla, «N» significa atlas **Non-Directional realmente recuperado**, no
que la casilla Baked GI por sí sola pruebe un bake. En las 18 escenas la casilla
Baked GI estaba activada; solamente diez tenían referencias a atlas. No había
texturas direccionales ni shadowmasks en esos datos. «Modo alineado» significa
un único dígito modificado en `m_LightmapsBakeMode`, respaldado por el modo 0 de
los atlas; no se cambiaron todas las escenas indiscriminadamente.

| Escena | Iluminación original recuperada | Estado anterior | Acción realizada | Estado final comprobado |
| --- | --- | --- | --- | --- |
| MainMenu | Dos atlas N: RGB24 512² y 32²; ambiente Flat negro | Dos texturas ausentes; SH ambiental con NaN | Recuperación exacta, SH negro finito y modo alineado | Dos atlas resueltos; escena sin renderizadores baked directos |
| MainMenu / laboratorio `SecretMenu` | Atlas 512² por `PrefabLightmapData`; 18 asignaciones | Atlas principal ausente; al recuperarlo, modo direccional incompatible en runtime | Recuperación del atlas y guard mínimo en el helper original | Laboratorio iluminado en Play Mode Medium; dos entradas/salidas; material `lab` y shader Mobile/Diffuse conservados |
| Master_high | Un atlas N compartido de 2048²; 537 asignaciones | Textura ausente | Recuperación ETC1→DXT1 y modo alineado | 537 asignaciones, cero referencias de textura inválidas al reabrir |
| Master_medium | Mismo atlas; 366 asignaciones | Textura ausente | Misma recuperación y modo alineado | 366 asignaciones resueltas; carga y movimiento en Play Mode Medium |
| Master_low | Mismo atlas; 236 asignaciones | Textura ausente | Misma recuperación y modo alineado | 236 asignaciones resueltas al reabrir |
| Master_spawn_high | Referencia al atlas compartido; sin asignaciones propias | Textura compartida ausente | Recuperación compartida y modo alineado | Referencia resuelta; sin asignaciones inventadas |
| Master_spawn_medium | Igual | Igual | Igual | Referencia resuelta; probado como parte del gameplay Medium |
| Master_spawn_low | Igual | Igual | Igual | Referencia resuelta |
| Alien_world | Referencia al mismo atlas; sin asignaciones propias | Textura ausente | Recuperación compartida y modo alineado | Referencia resuelta; iluminación específica no reconstruida por conjetura |
| titanic | Atlas N compartido de 512²; sin asignaciones propias | Textura ausente; SH no finito | ETC1→DXT1, SH negro y modo alineado | Atlas resuelto; carga aditiva durante gameplay Medium |
| VolcanoIsland | Mismo atlas 512²; 105 asignaciones; una luz Baked | Referencia a conversión PNG nula; SH no finito | Referencia canónica recuperada, SH negro y modo alineado | 105 asignaciones, cero texturas inválidas; render representativo comparado |
| SplashLoaderScene | Sin atlas | Ajuste de bake direccional en el arranque aditivo | Modo alineado con MainMenu | Arranque probado; no se generó iluminación nueva |
| Map_high | Sin atlas | Sin pérdidas baked demostradas | Auditoría; sin cambios | Conservada |
| Map_medium | Sin atlas | Igual | Auditoría; sin cambios | Conservada |
| Map_low | Sin atlas | Igual | Auditoría; sin cambios | Conservada |
| ArticWorld | Sin atlas; flag Realtime GI; probes exportadas vacías | Sin bake reutilizable demostrado | Auditoría; sin cambios | Conservada; GI original completa no demostrada |
| SharknyataCave | Sin atlas; flag Realtime GI | Igual | Auditoría; sin cambios | Conservada |
| Empty | Sin atlas | Sin pérdidas baked demostradas | Auditoría; sin cambios | Conservada |
| Effects | Sin atlas | Igual | Auditoría; sin cambios | Conservada |

**VERIFIED:** las 1.244 parejas de ID de renderizador / malla de Master
high/medium/low y Volcano coinciden con la referencia, no solamente con un tipo
de objeto del mismo número. Las coordenadas y la geometría no se modificaron.

**VERIFIED:** el prefab del laboratorio tiene una segunda entrada de textura
nula también en Unity 5. Afecta tres renderizadores HSE/RimLit; ese shader
recuperado no lee lightmaps. Se conservó la entrada original, sin asignarle a
ciegas el segundo atlas de MainMenu. Las otras quince asignaciones tienen atlas
válidos; el contrato comprueba por separado las referencias necesarias para los
shaders que las consumen. Los duplicados de las ranuras del helper apuntan al
mismo objeto Texture2D, no a copias adicionales del atlas.

## Fidelidad, shaders y configuración

**VERIFIED:** los 18 bloques originales `RenderSettings`/`LightmapSettings`
coincidían inicialmente entre las copias. Se conservaron intensidades, colores,
rangos, sombras, Mixed Lighting, ambiente, skyboxes, banderas Static,
LightmapParameters, resoluciones de bake, AO, bounces y orden de escenas. El
modo del editor se alineó únicamente en los diez consumidores de atlas N y el
arranque aditivo. Auto Generate sigue desactivado: workflow `OnDemand`.

**VERIFIED:** los `LightingData.asset` cargaban y su comprobación nativa de
validez pasaba. No se trasplantaron desde Unity 5: se modificaron mediante la
serialización nativa de Unity 2018 solamente el enlace de Volcano y los SH no
finitos de MainMenu, Titanic y Volcano. El ambiente original de esas escenas es
Flat negro, intensidad cero; los 27 coeficientes se reconstruyeron como cero,
sin reinterpretar artísticamente la iluminación. Los diffs JSON privados
comprueban que las matrices y el resto de datos permanecen iguales.

**VERIFIED:** los shaders personalizados de superficies baked mantienen sus
ramas originales `LIGHTMAP_ON`, UV secundarias y lectura dLDR. La captura del
laboratorio usa su shader Mobile/Diffuse existente. No se editaron shaders ni
materiales para corregir el blanco. Unity utiliza codificaciones de lightmap
según plataforma; por ello no se convirtió automáticamente un atlas dLDR en una
textura RGBM ni se cambió su gamma por conjetura. [Documentación Unity 2018.2](https://docs.unity3d.com/2018.2/Documentation/Manual/Lightmaps-TechnicalInformation.html).

**VERIFIED:** comparación aislada de malla, material y atlas originales del
laboratorio, Unity 5.6.4p3 frente a Unity 2018.2.19f1, Gamma, 960×544, Fastest
(perfil Medium), mip limit 1: error medio RGB **0,245 %**, RMS **0,559 %**.
Es una comparación de una pieza, no de todo el juego Android.

**VERIFIED:** ETC1→DXT1 preserva todos los mips. Error del nivel base:

| Atlas | Resolución / mips | Error medio RGB | RMS | Máximo por canal |
| --- | --- | ---: | ---: | ---: |
| Master compartido | 2048² / 12 | 0,0591 % | 0,3015 % | 18,43 % |
| Titanic / Volcano | 512² / 10 | 0,3083 % | 0,6939 % | 16,47 % |

Los máximos localizados impiden llamar a la recompresión «sin pérdida». El peor
RMS entre todos los mips fue 0,891 % y 1,458 %, respectivamente. Un render
representativo con el shader actual de Master_medium tuvo error medio 0,041 %
y RMS 0,133 %; la pieza de Volcano comparada produjo el mismo resultado. No
demuestra paridad completa de zonas, animaciones o cámaras.

## Memoria, rendimiento y PSP2

**VERIFIED / estimación estática:** los cuatro payloads completos con mips suman
4.023.662 bytes (**3,84 MiB**): 2.796.216 + 174.776 bytes DXT1 y 1.048.575 +
4.095 bytes RGB24. Ese es almacenamiento de texels, no una medición de RAM o
CDRAM de Vita. Las consultas de memoria de textura del editor suman
aproximadamente **4,88 MiB**, incluyendo su sobrecoste. La residencia simultánea,
alineación del driver, carga temporal y efecto del mip limit deben medirse en
hardware. No se prometen 3,84 MiB como límite de memoria de proceso.

**VERIFIED:** el player final limpio contiene cuatro atlas adicionales,
724 texturas en total; los atlas son dos RGB24 y dos DXT1, sin ETC ni BC7. El
atlas de gameplay conserva 2048²; no se bajó resolución para esconder su coste.
La configuración estable de memoria, audio, calidad, controles, shaders y las
optimizaciones anteriores se conserva mediante comparación de archivos.

**VERIFIED, alcance limitado:** una muestra posterior de Editor Medium tiene
153 frames, media 39,53 ms y P95 47,71 ms; media 79,39 draw calls/batches. No
existe un recorrido previo emparejado y el editor/profiler añaden trabajo. La
memoria administrada de proceso osciló durante la muestra, incluyendo trabajo
del editor y carga. Estos tiempos no son tiempo de CPU ocupada ni de GPU y no
permiten atribuir ganancia/regresión de FPS a la restauración.

**UNTESTED:** FPS, CPU/GPU, RAM/CDRAM y estabilidad físicos del nuevo player.
No se apagaron luces dinámicas, no se modificó gameplay ni se afirmó una mejora
de rendimiento. Se mantienen los 32 componentes de luz de cada Master y la luz
de Volcano. La traducción completa del antiguo `LightBakingOutput` sigue sin
demostrarse; no se cambiaron esos flags para ahorrar trabajo a ciegas.

## Pruebas y límites

| Prueba | Resultado y alcance |
| --- | --- |
| Auditoría de 18 escenas y reapertura | **PASS:** atlas asignados, LightingData válido; matrices conservadas |
| Identidad renderizador/malla | **PASS:** 1.244 parejas coincidentes |
| Decodificación / recompresión independiente | **PASS:** todos los mips, dimensiones y GUID conservados; error cuantificado |
| Referencia Unity 5 | **PASS en copia aislada:** cuatro assets; proyecto original nunca abierto |
| Laboratorio A/B | **PASS:** solo cambiar modo recupera sus colores y luces |
| Laboratorio con corrección definitiva | **PASS:** modo Single automático, entrada/salida y segunda entrada; sin cambiar shader o material |
| Contrato del helper | **PASS:** conserva anexado, referencias, índice y ST; respeta atlas direccional real |
| Gameplay Medium | **PASS parcial:** Master_medium + Master_spawn_medium + Effects + Titanic, atlas DXT1, movimiento x≈0,3→22,1, pausa/reanudación |
| Primer build PSP2 tras recuperar atlas | **PASS:** 18 escenas, 0 errores / 296 advertencias, 2:22,657 |
| Build final en sesión gráfica usada para ensayos | **FAIL / sin resultado:** cierre nativo D3D11 `TexturesD3D11::UploadTextureSubData2D`; causa no demostrada. No fue un crash de player/Vita |
| Build limpio final sin interfaz | **PASS:** Unity 2018.2.19f1, PSP2 / Mono2x, 18 escenas, 0 errores / 295 advertencias, 2:01,559; salida nativa inspeccionada; empaquetado privado posterior documentado al final |
| Contratos en DLL del player final | **PASS:** guard de lightmaps compilado, clasificación Medium, selección de calidad y limitador originales conservados |
| Vita física | **UNTESTED:** no se instaló ni publicó ningún paquete |

**VERIFIED:** se conservaron los intentos fallidos. La primera lectura GPU de
texels no era exacta y se descartó; se usó el decodificador CPU instalado de
AssetStudio 0.16.47. Unity 5 no importaba `.texture2D` en la copia mínima: la
misma textura/meta se cargó allí con extensión nativa `.asset`, sin tocar la
fuente. Se corrigieron preparación de diagnósticos y colisiones de solicitud
antes de aceptar sus resultados. Errores anteriores de IAP offline y avisos de
limpieza del juego permanecen diferenciados de las pruebas de iluminación.

**VERIFIED:** las pruebas provocaron escritura incidental del offset animado
de `Underwater.mat`, `runInBackground` y campos serializados por el editor en
QualitySettings. Se archivaron los diffs y se restauraron los archivos exactos
del respaldo antes y después del build limpio. Los ocho archivos de guardado y las
preferencias se respaldaron y restauraron; el guardado de pruebas se conserva
aparte para recuperación.

**UNTESTED / pendiente heredado:** 34 renderizadores mapeados de Master_medium
y los 105 de Volcano no tienen UV2 en sus mallas recuperadas. Sus mallas y
asignaciones coinciden con Unity 5; no se inventaron UV ni se hizo un bake que
invalidase el atlas. Las piezas y zonas correspondientes requieren comparación
amplia con Android/Vita. Los LightProbes exportados están vacíos y no hay
ReflectionProbe components ni cubemaps baked en estas escenas. Las banderas
Realtime GI de ArticWorld, SharknyataCave, Titanic y Volcano no prueban una GI
realtime recuperada: los mappings Enlighten exportados están vacíos.

## Archivos, respaldo y reproducción

Todo el contenido del juego permanece **privado** en el proyecto Unity 2018.
Se añadieron cuatro texturas con sus `.meta` originales y el LightProbes vacío
original de MainMenu con su `.meta`. Se modificaron once escenas por un dígito,
tres LightingData y solamente `PrefabLightmapData.cs` del runtime. Unity creó
también `Assets/WIP/titanic.meta` para la carpeta recuperada: no sustituye ningún
GUID existente. Se añadió el auditor dentro de `Assets/Editor` con su meta.
El archivo de
Titanic continúa en la carpeta preexistente `ttitanic`; no se renombró. Los
shaders, demás fuentes, mallas, audio y ajustes de proyecto quedan sujetos al
comprobante final de integridad: **19.660 archivos originales de usuario, quince
modificados, ninguno ausente**; once entradas de assets/metas recuperados y dos
del auditor. Todos los demás archivos originales coinciden exactamente.

Los tres datos existentes modificados son
`Assets/WIP/MainMenu/LightingData_0.asset`,
`Assets/WIP/ttitanic/LightingData.asset` y
`Assets/WIP/VolcanoIsland/LightingData.asset`. El único cambio de runtime está en
`Assets/Plugins/Assembly-CSharp-firstpass/PrefabLightmapData.cs`. Las once escenas
modificadas figuran en la tabla; son las diez marcadas «modo alineado» y
SplashLoaderScene. El inventario exacto con hashes está en el
[comprobante sin contenido propietario](evidence/LIGHTMAP_RESTORATION_2026-10-21.json).

Respaldo completo e informes privados: `Reports/LightingRestoration_20261020/`.
`Before/` contiene los authored inputs; `Unity56Before.json` y
`Unity2018Before.json` sus hashes. `BeforeModeAlignment/` y `PrefabModeRepair/`
guardan respaldos específicos. `LightingDataRepair/`, `CPUConversion/`,
`RendererIdentityCheck.json`, `VisualFixtures/`, `LabEngineComparison.json` y
`PlayRegression/` conservan el detalle. `IntegrityFinal.json` y
`Unity56IntegrityAfter.json` cierran la comprobación. `CleanBuild/Result.txt`,
`NativeTextureCensus.json`, `NativePrefabGuard.json` y `NativeMediumAudit.json`
registran la salida final. Los diagnósticos temporales se archivaron fuera de
Assets; solamente permanece el auditor de lectura. No subir esos directorios a GitHub.

Herramientas públicas, sin assets recuperados:

- `tools/unity/HSELightingRestorationAudit.cs`: auditor Editor de solo lectura,
  preserva escenas abiertas y rechaza cambios sin guardar.
- `tools/restore_hse_lightmap_modes.py`: alinea únicamente los modos probados,
  con vista previa y respaldo verificado antes de `--apply`.
- `tools/fix_hse_prefab_lightmap_mode.py`: parche mínimo del helper privado;
  exige versión de proyecto y hash de entrada auditado.

Para revertir, cerrar Unity y restaurar **solo** los quince archivos modificados
desde sus respaldos; retirar las diez entradas nuevas de textura/probe y metas
según el inventario final. La meta de carpeta nueva y el auditor se gestionan
por separado, comprobando primero que la carpeta no contiene trabajo posterior.
No restaurar todo el proyecto sobre trabajo posterior.
No hay ramas nuevas, commits de contenido propietario ni cambios de fases.

En PS Vita física, con **Medium y relojes habituales**, comprobar arranque →
Evolución → laboratorio; paredes azules y puntos rojos, sin negro/blanco/pink;
recorrer tiburones especiales, salir y repetir después de un arranque en frío.
Después repetir la misma ruta de gameplay antes/después, cuevas, portales,
Titanic y Volcano cuando estén disponibles; medir FPS/frametime y memoria, y
comprobar audio, controles y guardado. La aprobación física queda pendiente del
propietario.

## VPK privado solicitado después de la restauración

**VERIFIED:** el propietario pidió expresamente un VPK para probarlo, el
20 de octubre local / 21 UTC. Se empaquetó el player final existente sin otro
build ni cambios de proyecto:
`Reports/LightingRestoration_20261020/CleanBuild/HSE_Lighting_Restored_HGSKE1178.vpk`.
Tamaño **135.759.203 bytes**, 213 entradas. SHA-256:
`488a66e4e0682f6aaa00b573277e101cb10c6f6c142003b3b7543072e33d36cc`.

**VERIFIED:** CRC y comparación independiente de cada entrada con su archivo
nativo pasan. La identidad HGSKE1178, artwork y flags de memoria coinciden con
el paquete anterior; solamente se normaliza su metadata SFO de identidad. No
incluye savedata, símbolos ni helpers del host. La DLL principal de gameplay
conserva el SHA-256 del player de optimizaciones anterior
`f59d15c7502a77bc082cf33cd88f521e051960aa605ea3bf46e87e9d11dc65a1`;
el guard de iluminación está en la DLL firstpass. El VPK anterior permanece
idéntico. `CleanBuild/PackageReceipt.json` e
`IndependentPackageVerification.json` registran la entrega. No se instaló ni
publicó el VPK; la prueba física queda pendiente.

**VERIFIED, precisión sobre el log del build:** el preflight de empaquetado
detectó doce líneas CS2001 de un intento de compilación de la DLL del Editor:
Unity retenía seis nombres de herramientas temporales archivadas en sus inputs
de compilación. Se conservaron y clasificaron expresamente esos diagnósticos;
no se eliminó el log ni se ignoraron errores desconocidos. Los bloques de
compilación de las DLL runtime pasan, no hay errores de shader y el BuildReport
nativo indica Succeeded / cero errores. Esa cifra no significa que todo el log
carezca de mensajes de error del Editor.

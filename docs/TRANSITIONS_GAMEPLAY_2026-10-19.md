# Evolución → Jugar y captura de gameplay — 2026-10-19

**Resultado:** reducción comprobada del trabajo de entrada a tienda en Editor y
compilación PSP2 correcta. **UNTESTED:** reducción de los ~10 segundos físicos,
eliminación de la traba de regreso y 30 FPS sostenidos en Vita sin overclock.

Esta petición del propietario autoriza el diagnóstico y las correcciones privadas
de Unity 2018 como excepción específica. Se trabajó en `main`, sin ramas nuevas.
P03 sigue ACTIVE; P00–P02 APPROVED, P04–P12 LOCKED. No se modificó el ledger.

## 1. Diagnóstico y cambio aplicado

**VERIFIED, código y Editor:** `UpgradeMenuSheet.ButtonHandler`, botón
`NextButton`/Jugar, abre **StoreMenuSheet: tienda de equipamiento**, mediante
`GUIMenu.AddMenu`. No carga una escena de gameplay en esta transición. Se ejecutan
reconstrucción del catálogo, creación del menú/modelo, botones y filas, zoom,
`UnloadUnusedAssets`, GC explícito y animaciones originales. La partida empieza
después, desde `StoreMenuSheet.ButtonHandler`, botón `PlayButton`.

El coste principal descubierto no era una reconstrucción de Canvas: esta pantalla
tiene cero Canvas y utiliza NGUI. Para escribir las descripciones de sus 49 filas,
40 consultas a `ShopManager.GetAccessoryPrefab` cargaban prefabs completos con sus
dependencias. Una captura desglosada atribuye **447,4 ms** a esas consultas dentro
de **535,6 ms** de filas; modelo/preparación de tienda ~28 ms y GC ~9 ms.
Profiler registra lectura, integración y AwakeFromLoad de recursos. Sus tiempos
inclusivos se solapan: no deben sumarse. La solicitud de descarga no representa
todo el coste posterior de `UnloadUnusedAssets`.

**INFERRED para Vita:** esas cargas síncronas son una explicación fuerte de una
parte importante de la espera física. **No se ha demostrado que expliquen los
10 segundos completos.** Se conservan las animaciones de 0,3/0,5 s y el polling
original; no se encontró una espera fija de diez segundos en esta ruta.

Se añadió un catálogo **solo para descripciones UI**, generado desde los prefabs
privados: 115 entradas, **134.469 bytes JSON**. Contiene copias de datos y el booleano
de presencia de trail, sin referencias a prefabs, meshes, materiales o texturas.
Conserva los métodos originales de configuración remota y texto localizado.
El equipamiento y gameplay siguen consultando los prefabs originales.

Las rutas respetan la comparación de mayúsculas de Resources. Si faltan datos se
advierte y se usa el acceso original; el prebuild PSP2 rechaza un catálogo ausente,
inválido o con dependencias modificadas. Límites: 512 entradas/512 KiB JSON; no son
un presupuesto de RAM. El TextAsset se libera después de leerlo; las copias están
acotadas por el catálogo y se reinician antes de una nueva sesión.

## 2. Comparación reproducible

**VERIFIED Editor, Unity 2018.2.19f1, tres ciclos por condición, misma sesión/perfil.**
Medianas; el observador espera fin del tween raíz y frames renderizados. La primera
entrada del helper carga sus datos; no es una prueba de arranque frío de Vita/OS.

| Medida | Control original | Optimizado |
| --- | ---: | ---: |
| Acción Jugar / trabajo síncrono | 591,2 ms | 134,8 ms |
| Hasta tienda preparada tras animación | 1123,2 ms | 665,6 ms |
| Pico de frame durante entrada | 605,4 ms | 147,1 ms |
| Acción de regreso a Evolución | 87,5 ms | 81,8 ms |
| Regreso tras tween raíz | 426,7 ms | 422,2 ms |
| Pico de frame de regreso | 99,1 ms | 98,4 ms |

Las filas pasan de 467,9–597,5 ms a 64,5–79,2 ms. Al terminar ese bloque,
Unity tiene aproximadamente **9 MB menos asignados**; es un punto de muestreo
del asignador del Editor, **no** memoria máxima, libre ni RAM/VRAM de Vita.

**Regreso:** se sigue destruyendo/reconstruyendo Evolución, preparando su jerarquía
de UI/modelos y ejecutando las limpiezas originales. Sus constructores medidos
consumen decenas de ms. La comparación no demuestra eliminar la traba de regreso;
queda pendiente su atribución temporal física. No se introdujo una caché de menús
vivos ni se omitió GC para aparentar una mejora.

## 3. Gameplay y límites de la captura

**VERIFIED:** se utilizó el lector lógico de control de pruebas ya existente,
temporal, sin cambiar movimiento ni alimentación. Hubo desplazamiento horizontal
y vertical y alimentación real. La captura final contiene 589 frames/~32,9 s,
contador de comidas **1→11**, posiciones x=0,13–48,77/y=−23,04–−4,78 y 26
colecciones GC. Se comprobaron pausa y reanudación.

El perfil corto previo sitúa `SpawnerManager.Update` por delante de otros scripts
(~59,7 ms acumulados/299 llamadas, máximo 2,14 ms); después aparecen Player,
peces/cardúmenes, renderizado, actualizaciones de bounds, física y partículas.
Se revisaron modelos, animaciones y consultas de colisión. Varias optimizaciones
PSP2 previas no se ejecutan en la misma rama del Editor; se verificaron en el player.
No se cambió IA, spawning, física, animaciones, densidad, shaders ni calidad.

**No hay comparación válida de FPS de gameplay antes/después:** cambian recorrido,
posición inicial, carga y resolución del Game View. La captura final da P50=53,15,
P95=69,72 y P99=79,62 ms, pero incluye condiciones del Editor/Profiler. No demuestra
ganancia ni pérdida en Vita. `Gfx.ProcessCommands` incluye esperas; no prueba coste
GPU ni CPU ocupada. El Recorder de PlayerLoop no produjo muestras completadas:
se marca no disponible, sin sustituirlo por cero. El raw permite inspeccionar hilos.

La ruta automática no cubre todo el mapa, especies, zonas densas ni sesiones largas.
No se añadió otra optimización de gameplay sin una comparación representativa.

## 4. Validaciones y preservación

- **PASS:** las mismas 49 filas en seis ciclos: identidad, orden, posición, textos,
  precios y rutas de iconos. 115 efectos comparados con datos originales, texto
  localizado y predicado de visibilidad; sin mutar prefabs. Las filas desaparecen al
  regresar. Esto no sustituye un soak test de memoria ni todas las combinaciones.
- **PASS:** C#4/Mono antes/final, PSP2 0 errores/271 advertencias y Editor 0/269.
  Sin nuevas firmas de advertencias. Comparación de 31.731 métodos: solo cambia
  el cuerpo existente `StoreMenuSheet.RefreshGameItemSlots`; helper nuevo aparte.
- **PASS build nativo:** PSP2/Mono2x, 18 escenas, 2:13,079, 0 errores/296 advertencias.
  Auditorías de descripción, modelos, recursos, frustum/rocas/agua, calidad,
  pantalla limpia y guardado D08/D09/D10. Control original rechazado por el auditor.
- **PASS integridad:** de 19.650 archivos originales, solo cambia StoreMenuSheet.
  Se añaden helper, generador, captura y JSON privados con metas. Ajustes y parches
  previos conservan hashes. Ocho archivos del perfil y el material Underwater
  animado se restauraron exactamente, conservando las salidas de prueba aparte.
  VPK anterior intacta; no se creó ni publicó una VPK nueva.
- **UNTESTED:** desbloqueos/compras confirmados, todo el audio/efectos, selección de
  cada tiburón, controles físicos, cold save Vita y estabilidad prolongada. El código
  de compra/progresión/guardado permanece intacto; no se repitió toda esa matriz.

Intentos inválidos conservados: entrada inicial del arnés a partida en vez de
Evolución; comparación que aún usaba la ruta de prefabs; diferencia trail/rutas
corregida antes del PASS; APIs de Profiler ausentes corregidas; un batch de auditorías
con argumentos incorrectos invalidado y repetido correctamente. Una captura larga
solo dejó 300 frames en el historial cargado y se sustituyó por perfiles cortos.
No se usan esos intentos para proclamar mejoras.

## 5. Prueba manual que falta

El capturador está instalado en el proyecto privado y el Profiler está abierto.
Se comprobó su captura y parada. No requiere introducir automatización de gameplay.

1. Abre `Assets/WIP/SplashLoaderScene.unity`, entra en Play Mode e inicia partida.
   Espera a terminar la carga. Mantén el mismo tamaño/calidad del Game View en cada tanda.
2. En Profiler deja **Record** y **Deep Profile** apagados antes de F8; comprueba
   CPU Usage, Rendering, Memory y Physics en **Add Profiler**. CPU Timeline permite
   revisar Main Thread/Render Thread, GC, animaciones, cargas y callbacks.
3. Pulsa **F8** o `HSE > Profiling > Start gameplay capture`.
4. Durante 60–90 s mueve el tiburón, cambia de zona, come varias presas y atraviesa
   zonas con muchas entidades/efectos. Pulsa **F9** para parar **antes de salir de Play**.
5. Repite tres recorridos comparables. Anota tiburón, zonas y acciones de cada tanda.
   Entrega cada carpeta `Reports/GameplayCapture_<UTC>/`: `unity.raw`, `frames.csv`
   y `availability.txt`. Para cargar raw usa **Load** en Unity Profiler.

La captura escribe CSV al parar y limita el buffer a 20.000 frames; el raw sí se
escribe durante la prueba. Para tandas largas divide capturas. Los campos −1 no
están disponibles. Memoria nativa libre, VRAM y GPU requieren herramientas del
hardware; los totales del Editor no las sustituyen.

La comparación física definitiva debe medir primera entrada y repeticiones de
Evolución→tienda→Evolución, memoria máxima y gameplay con **frecuencias stock**.
No se dispone de una Vita controlada en esta sesión. El player privado queda en
`Reports/Transitions_20261019/Build/NativePlayer/`; no equivale a validación física.

[Recibo saneado](evidence/TRANSITIONS_GAMEPLAY_2026-10-19.json).
Las fuentes/datos/logs originales permanecen privados fuera de Git.
El [preparador](../tools/prepare_hse_store_descriptions.py) reproduce la adaptación
sobre un proyecto suministrado por el propietario: comprueba por defecto, y con
`--apply` respalda antes de editar. El JSON se genera privadamente desde el menú
HSE del Editor; no se incluye contenido del juego en el commit.
Herramientas y adaptación reproducible: commit `2a52d28`; este informe y su recibo
se registran en el commit documental siguiente. No se realizó push.

## Seguimiento físico del propietario

**2026-10-19 — VERIFIED por reporte explícito del propietario:** después de recibir
la VPK de transiciones, informa que los menús mejoraron y ya no tardan tanto,
aunque persisten pequeñas esperas. Es confirmación cualitativa en Vita, no una
medición independiente de Codex. Referencia entregada: SHA-256
`4ac7df928ec49b8c25ce59a558ece534b2ca25789f0c601c6676eceb6b48dc81`.

No se proporcionaron hash instalado, frecuencias, tiempos por transición ni FPS
de gameplay. La magnitud física, el regreso específico y los 30 FPS a frecuencias
stock siguen pendientes. Se conserva esta compilación como referencia para las
esperas residuales; sin nuevo código, build, paquete ni aprobación de fase.

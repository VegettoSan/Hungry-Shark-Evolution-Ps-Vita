# Profiling interactivo — preparación y sesiones

Fecha local: 2026-10-19; preparación registrada 2026-10-20 UTC. Petición específica
del propietario en este chat: diagnóstico interactivo y optimización privada de
Unity 2018 sobre `main`. Es una excepción de alcance; P03 permanece ACTIVE,
P04–P12 LOCKED y no se aprueba ninguna fase. No se ha probado una Vita en esta sesión.

## Estado de la investigación

**VERIFIED:** Unity 2018.2.19f1 abierto, destino PSP2, backend Editor Direct3D11.
Acceso puntual a ventana y archivos; no observación continua de la interacción.
Los controles siguen a cargo del propietario. No se inició gameplay automático.
**UNTESTED:** baseline manual representativo, optimizaciones nuevas y comparativa
antes/después. Los 20–30 FPS físicos sin overclock son un reporte del propietario.

**VERIFIED, preparación:** checkpoint privado de 19.658 archivos authored,
copias de los fuentes tocados, metas, material animado y perfil existente.
Comparación posterior: cuatro archivos existentes modificados —capturador Editor,
PlatformUtils, GUIResolutionSwitchController y App—, dos helpers Editor nuevos
con sus metas; ningún archivo ausente, ningún ProjectSettings cambiado. Los tres
fuentes runtime solo tienen hooks temporales bajo UNITY_EDITOR y SessionState;
el código del player PSP2 conserva sus ramas previas. No son optimizaciones.

## Calidad efectiva

| Parámetro | Resultado observado en el menú preparado |
| --- | --- |
| Contenido | `medium`, seleccionado antes de cargar el contenido |
| Perfil Unity | `Fastest`; no existe un perfil llamado Medium |
| Shader keywords | `LOW_END` activo; `HIGH_END` inactivo |
| Atlas | SD, según la ruta original para la resolución de Vita |
| Render target Game View | 960 × 544; la escala de presentación puede ser menor |
| Sombras / distancia configurada | Desactivadas / 15 |
| Antialiasing / anisotropía | 0 / desactivada |
| Mipmaps | masterTextureLimit = 1 |
| LOD bias / máximo nivel LOD | 0,3 / 0 |
| Soft particles | Desactivadas |
| VSync / targetFrameRate | 1 / 60, presentación actual del port; objetivo de optimización: 30 estables |
| Cámaras observadas en menú | Forward, HDR desactivado; clips conservados, sin ajuste global |

El perfil guardado PSP2 señala un índice fuera de los dos perfiles presentes;
el juego selecciona Fastest explícitamente en PlatformUtils.Awake. La clasificación
del PC y Screen.currentResolution tampoco representan la Vita: por eso los hooks
Editor fuerzan medium y SD durante esta sesión. El App original fija 30 en Editor;
se reproduce temporalmente la presentación PSP2 ya existente, sin cambiar el player.
El calculador de memoria conserva su lógica; no se usa la RAM del PC como presupuesto Vita.

**Límites:** no se ha demostrado paridad total de importadores, GPU, shaders,
postprocesado y todas las cámaras/zonas. Los clips, componentes de cámara y estados
efectivos se registran al inicio/final de cada captura. Varias optimizaciones PSP2
previas están condicionadas a !UNITY_EDITOR: una captura Editor no demuestra su
beneficio en el player. Una cámara allowMSAA=true no implica MSAA efectivo con AA=0.

## Captura y reproducción

Instalar juntos en Assets/Editor del proyecto privado suministrado por el dueño:
[HSEGameplayCapture](../tools/unity/HSEGameplayCapture.cs),
[HSEInteractiveProfiling](../tools/unity/HSEInteractiveProfiling.cs) y
[HSEProfileExport](../tools/unity/HSEProfileExport.cs). No entran al player.
La preparación temporal real y sus copias exactas quedan en el checkpoint privado;
no se distribuyen fuentes ni recursos del juego.

- `HSE > Profiling > Prepare Vita Medium session` selecciona Game View y módulos.
  Los hooks privados deben estar instalados para medium/atlas/presentación.
- F8 inicia una captura; F9 la detiene, también desde el menú HSE. Otro binario
  Profiler activo se rechaza para no reemplazar su salida. La grabación ordinaria
  de la ventana y los Recorders se preservan/restauran.
- CSV buffered de hasta 20.000 frames; parada automática a los cinco minutos.
  Raw continuo dividido cada 240 muestras para el límite de historial de 300.
  No se escriben logs propios por frame. Control por archivo: máximo dos sondeos/s,
  con lectura solo al cambiar su fecha de modificación.
- Capturas privadas en `Reports/InteractiveProfiling/<nombre>_<UTC>/`:
  frames.csv, segment_*.raw, availability.txt y settings_start/end.txt.
  No mover ni borrar estos directorios entre sesiones. No se agregan a Git.
- Tras parar Play, el comando privado `export|<carpeta>` carga cada raw y exporta
  coverage.csv, estadísticas por frame y totales inclusivos por marcador/hilo.
  No suma tiempos inclusivos como si fueran independientes. El tiempo de frame
  nativo es duración común, no CPU ocupada por cada hilo.
- [Analizador CSV](../tools/analyze_hse_capture.py): media, P50/P95/P99, máximos,
  GC, memoria, alimentación, posición y contadores. Conserva distribución completa
  y otra excluyendo los dos frames marcados después de cada rotación/inicio.
  Las capturas raw y su cobertura deben examinarse junto con el CSV.

El Recorder PlayerLoop no entrega muestras completadas y se informa no disponible;
los hilos se analizan desde el raw. GC.Alloc del Recorder cuenta eventos, no bytes.
Las estadísticas nativas incluyen GC Allocated, memoria, draw calls y física.
Los formatos legibles de Unity pueden estar redondeados. RAM/VRAM física y GPU PSP2
no se deducen del asignador del Editor. Las esperas de Gfx.ProcessCommands no son GPU ocupada.

La [documentación oficial de Unity 2018.2](https://docs.unity3d.com/2018.2/Documentation/Manual/ProfilerWindow.html)
explica el historial limitado, la carga de binarios y el elevado coste de Deep Profile;
se usa instrumentación ordinaria y Deep Profile apagado.

## Comprobaciones técnicas y fallos conservados

**VERIFIED, autocheck final en escena vacía:** 520 filas CSV; tres segmentos con
241/241/41 frames nativos legibles, 15 hilos, estadísticas de memoria/GC disponibles.
No es una prueba de rendimiento del juego ni prueba funcional del gameplay.
El frame adicional de los segmentos es una frontera nativa; no duplicar sus
estadísticas al combinar segmentos sin reconciliar sus tiempos/IDs.

Los intentos previos quedan privados y excluidos de comparativas: APIs de áreas
ausentes en Profiler 2018.2, namespace corregido, solicitud repetida tras domain
reload, lectura simultánea del archivo de control, comando enviado antes de refresh
y primer raw reducido a dos frames por Record de la ventana sin sincronizar.
Correcciones: API de charts inspeccionada en el Editor instalado, ID en SessionState,
sondeo acotado con reintento de IO, actualización normal de assets y sincronización
explícita de ProfilerDriver.enabled. La repetición final verifica la cobertura.
Error IAP unavailable y aviso de limpieza observados en arranque/salida: precedentes
conocidos conservados; no se ocultaron para producir un PASS global.

## Sesiones manuales 1 y 2 — completadas

**VERIFIED:** inicio y final confirmados expresamente por el propietario en ambas
sesiones. Primera: tiburón por defecto, todos los menús, desplazamiento/alimentación,
muerte y regreso; trabas en menú y gameplay reportadas por el dueño. Su captura
`Baseline_Gameplay_20261020T031703760Z` tiene 9.091 filas / 300,006 segundos,
38 raw legibles. La parada automática ocurrió en gameplay; muerte/regreso quedan
fuera de esa grabación, aunque el dueño los confirmó.

Segunda: `Baseline_Gameplay_20261020T034343741Z`, nombre conservado por el selector
tras domain reload; corresponde a **Gameplay_FunctionBreakdown**, no a otra línea
base sin cambios. 12.409 filas / 212,439 segundos, 52 raw legibles; 142 presas
comidas y regreso/menús incluidos. El dueño confirmó ausencia de fallos nuevos en
colores, daño, alimentación, turbo y pausa; murió y volvió al menú. No equivale a
la matriz completa de tiburones/equipamiento.

La segunda usa Game View maximizada. El primer segmento registra también el
Editor para diagnóstico, y se excluye del resumen ordinario. La primera mantuvo
Scene View visible. Cambian recorrido, densidad y pasos de física por frame:
**NO son una comparación A/B de FPS**. Excluir dos muestras de cada rotación
permite separar instrumentación; se conservan también las distribuciones completas.

| Observación Editor, gameplay | Sesión 1, segmentos 18–37 | Sesión 2, segmentos 1–40 |
| --- | ---: | ---: |
| Filas CSV / sin fronteras | 4.771 / 4.731 | 9.600 / 9.520 |
| Frame medio / mediana (ms), sin fronteras | 39,910 / 39,511 | 16,576 / 16,567 |
| P95 / P99 / máximo (ms), sin fronteras | 49,270 / 54,892 / 134,284 | 17,201 / 17,538 / 52,061 |
| EditorOverhead inclusivo (ms/Update de Player) | 33,541 | 2,317 |
| BehaviourUpdate scripts inclusivo (ms/Update) | 0,895 | 0,837 |
| Física FixedUpdate inclusivo (ms/Update) | 0,577 | 0,291 |
| Camera.Render CPU inclusivo (ms/Update) | 0,700 | 0,565 |
| Draw calls / batches medios | 50,10 / 49,78 | 44,69 / 44,45 |

Son mediciones del proceso Editor Direct3D11, con esperas/presentación y coste del
Profiler. No convertirlas en FPS PSP2 ni atribuir la diferencia al tinte. Los tiempos
inclusivos anidados no se suman. GC.CollectionCount del proceso incluye Editor:
712 colecciones en la ventana de sesión 1 frente a 11 muestras GC.Collect en el
árbol del juego. La segunda tiene 26 colecciones del proceso / 21 muestras en
la ventana seleccionada. Tampoco es una reducción A/B atribuible a un parche.

**Diagnóstico de gameplay:** SpawnerManager.Update es la mayor ruta de script
medida (0,168/0,179 ms por Update, picos 7,105/7,758 ms); Player.Update
0,154/0,158 ms. En sesión 2, CanSpawn ocupa 0,125 ms/Update (487.197 llamadas);
UpdateHealth 0,079 ms, tint 0,013 ms. Las consultas GetComponent de CanSpawn ya
tienen caché PSP2 previa, ausente en Editor: conservar esa mejora y no extrapolar
su coste Editor al player. En un segmento de 240 frames, CanSpawn registra
5.136.340 bytes de GC; esta cifra necesita distinguir Editor/null wrappers antes
de proponer otra caché. UpdateHealth registra 82 bytes/frame en esa muestra;
GetComponent puede tener comportamiento Editor específico. No se ha alterado
salud, IA, frecuencia de spawn, física, controles ni animaciones.

AIGenericFish.Update totaliza 0,049/0,039 ms por Update de Player; existen arrays
OverlapSphere en IA y Shoal. NonAlloc exige prueba de orden, empates y overflow;
no se ha sustituido especulativamente. Hay aproximadamente 103 callbacks de
RocksExplosion y 105 de BreakWallController/frame, unos 0,025 y 0,030 ms. Desactivar
componentes afecta sus ciclos de vida y exige pruebas dedicadas; no se ha hecho.
Pooling global tampoco está justificado por el Instantiate observado en gameplay.

**Menús/cargas:** la segunda incluye picos CSV de 1.722,921 y 1.528,168 ms
después del gameplay. Los árboles atribuyen el primero a integración/carga y
Instantiate de vuelta al menú; el segundo a botón/carga/Instantiate, con
Animation.RebuildInternalState anidado. Otro frame de regreso destruye 11.389
objetos; UnloadScene alcanza 359,313 ms en el árbol. Los caches de modelos PSP2
previos no se ejecutan en Editor: estos datos no prueban regresión de la mejora
reportada en Vita ni autorizan rehacerla. Las capturas conservan carga inicial,
menús y regreso completos; no se han filtrado los picos para anunciar una mejora.

**Render/memoria:** Camera.Render mide CPU, no el coste de shaders/overdraw/GPU
PSP2. El render thread incluye esperas Gfx.WaitForCommands. El asignador del Editor
alcanzó varios GB; no es RAM/VRAM disponible en Vita ni prueba aislada de fuga del
player. Se mantienen recursos y calidad. No hay validación nueva en hardware.

## Correcciones demostradas y controles

1. **Tinte, VERIFIED:** reutilización local del Material en cada bucle, sin caché
   persistente ni cambios de shaders/matemáticas/orden. Lecturas material por renderer:
   sin tinte 5→2; con tinte 7→2. [Preparador](../tools/prepare_hse_player_tint.py)
   conserva implementación original de otros players y comparación Editor.
   [Contrato](../tools/unity/HSEPlayerTintContract.cs): 72 comparaciones exactas,
   1/4 renderers, accesorios que aceptan/rechazan tinte, hielo/veneno/invulnerabilidad,
   negro, decaimiento y reemplazo de material. Cinco rondas alternas / 10.000 llamadas:
   mediana sin tinte 7,833→6,633 ms (1 renderer), 29,126→24,369 ms (4);
   veneno 13,497→10,874 y 43,878→35,170 ms. Ahorro 15–20% de esa función aislada,
   **no del frame**. La sesión 2 usó el nuevo camino, sin fallos nuevos reportados.
   Un primer intento con colisión C# CS0136 se corrigió; no entra en comparativas.
2. **Tiempos por área, VERIFIED en contrato; gameplay pendiente:** la sesión 2
   registra nueve GC.Alloc de 20 bytes/frame en GameLogic.Update (43.200 bytes /
   240 frames). [Contrato aislado](../tools/unity/HSEAreaTimeContract.cs) reproduce
   exactamente 1.080.000 bytes / 6.000 actualizaciones con el diccionario original,
   cero eventos dentro del marcador corregido. 10.000 comparaciones exactas PASS
   incluyendo primeras visitas, regresos, delta cero/negativo, enum desconocido y
   crecimiento del diccionario. [Preparador](../tools/prepare_hse_area_times.py)
   instala [comparador tipado](../tools/unity/HSEVitaAreaComparer.cs) y elimina una
   búsqueda redundante. Primera visita continúa en cero, misma suma float y claves;
   no cambia persistencia ni analytics. Las rondas cronometradas del fixture tienen
   Profiler activo: no utilizarlas como benchmark de CPU sin instrumentación.

Ambas correcciones compilan con C#4/Mono Unity 2018.2: PSP2 0 errores/271 warnings,
Editor 0/269, mismos recuentos de los controles originales. Tokens de otras
plataformas (ninguna/Android/iOS/Standalone) idénticos en los métodos modificados.
Esto verifica compilación de scripts, no build de un VPK ni FPS Vita.

Evidencia privada: `Reports/InteractiveProfiling/TintTrial/` (Before/receipt,
contracts, benchmark, compilaciones/paridad) y `AreaTimeTrial/` (equivalentes más
dictionary.raw y exportación de GC). Los archivos raw originales permanecen intactos.
La exportación adicional `detail|carpeta|NNNN` conserva rutas, muestras >10 ms y
bytes de GC extraídos de tooltip con contador explícito de tamaños no interpretados.

## Sesión 3 — validación manual completada

**VERIFIED:** inicio/final expresos; el dueño confirmó alimentación, daño, turbo,
pausa, muerte y regreso sin fallos nuevos. `Optimized_Gameplay_20261020T035846645Z`
contiene 14.700 filas / 251,912 segundos / 62 raw legibles, 186 presas comidas.
Las rutas GC de tres intervalos separados (0010/0030/0040, 720 frames de gameplay)
contienen **cero eventos bajo GameLogic.Update**, frente a 9 eventos/180 bytes por
frame antes de corregir el comparador. Es validación directa de la asignación
evitada en Editor. El fixture aislado establece la causa y equivalencia exacta.

| Observación Editor, segmentos 1–40 | Sesión 2, enum original | Sesión 3, enum corregido |
| --- | ---: | ---: |
| Filas sin fronteras | 9.520 | 9.520 |
| Frame medio / mediana (ms) | 16,576 / 16,567 | 16,579 / 16,567 |
| P95 / P99 (ms) | 17,201 / 17,538 | 17,183 / 17,496 |
| Peor frame gameplay, sin fronteras (ms) | 52,061 | 45,586 |
| Todos los scripts inclusivos (ms/Update) | 0,837 | 0,804 |
| GameLogic.Update GC del intervalo 0010, 240 frames | 43.200 bytes / 2.160 eventos | 0 / 0 |
| GC.Collect en árbol del juego / máximo (ms) | 21 / 7,323 | 19 / 6,976 |
| Draw calls medios | 44,69 | 44,18 |
| Mono usado, sesión completa (MiB, mínimo–máximo) | 20,91–35,19 | 21,25–35,13 |
| Pico observado al volver al menú (ms/frame, no duración total) | 1.722,921 | 1.866,859 |

Los recorridos y entidades difieren (157.488 frente a 126.615 callbacks IA en esas
ventanas); no atribuir diferencias globales de CPU, GC, memoria o máximos al parche.
**No se demuestra mejora general de frame time ni eliminación de picos.** El peor
frame de toda la tercera sesión es 1.866,859 ms al volver a menús; la navegación
posterior conserva picos de 1.555,136/985,119/928,853 ms. No se afirma una nueva
mejora de menús/cargas; las optimizaciones PSP2 anteriores siguen conservadas.

Se considera demostrado el ahorro acotado de llamadas de material y boxing, no la
meta de 30 FPS físicos. El siguiente diagnóstico que puede decidir una mejora
grande requiere capturas del player PSP2 y separación CPU/GPU en la consola sin
overclock. Repetir las mismas rutas en el PC no resuelve esa incertidumbre.

## Priorización y límites prácticos

| Área | Evidencia | Impacto/riesgo y decisión |
| --- | --- | --- |
| CPU/GPU PSP2 | Editor no reproduce backend ni relojes de Vita | Máxima prioridad de diagnóstico; beneficio y cuello real **UNTESTED** en dispositivo |
| Spawner/activación | Mayor coste de scripts y picos; caché de componentes PSP2 ya existe | Potencial alto; preservar semántica CanSpawn/LockableSpawner y caché anterior; sin nuevo cambio especulativo |
| GC enum | 180 bytes/frame eliminados; contrato y gameplay | Ahorro pequeño pero determinista, riesgo bajo; implementado |
| Tinte | Menos getters, 72 contratos, benchmark y sesiones | Ahorro pequeño de CPU, riesgo bajo; implementado |
| Callbacks de rocas/paredes | ~208 callbacks/frame; ~0,055 ms combinados en Editor | Potencial a medir en PSP2; riesgo de Start/Play/respawn/control; conservados |
| Arrays de IA/Shoal | OverlapSphere asigna, IA acotada/intercalada existente | NonAlloc/pooling sólo con límites/overflow/orden y memoria probados; conservados |
| UI/menús | NGUI UIPanel/instancias/animación; cachés Vita ausentes en Editor | Preservar parches anteriores. Módulo UI/Canvas no representa por sí solo todo NGUI |

Ninguna corrección elimina contenido, efectos, animaciones o presas. No se cambian
pasos de física, controles, salud, guardado, shaders ni recursos gráficos. El budget
de memoria disponible para Unity PSP2 y la presión real de GPU siguen **UNTESTED**.

## Fuentes finales y restauración aplicada

La comparación Editor A/B y los marcadores se retiraron tras la tercera sesión.
Los preparadores admiten `--production-only`: cambios sólo bajo
`UNITY_PSP2 && !UNITY_EDITOR`; Editor y otros players recuperan los métodos
originales. La variante PSP2 conserva el mismo código optimizado probado en Editor.
Sólo quedan dos fuentes existentes modificados (Player/UpdateTint y GameLogic,
inicialización/actualización del diccionario) y el pequeño comparador nuevo.
Los checkpoints del ensayo y fuentes anteriores están conservados.

Se restauraron App/PlatformUtils/GUIResolutionSwitchController, SpawnerManager,
capturador previo, calidad/keywords/presentación, selección Game View y charts.
Los helpers de esta tanda y sus metas se archivaron en Reports, fuera de Assets.
El perfil posterior del dueño se respaldó **sin restaurar SaveBefore sobre su
progreso nuevo**. ProjectSettings y recursos se verifican por hash al finalizar.

## Validación final, paquete y pendientes físicos

**VERIFIED:** compilación final original/corregida C#4 con Mono de Unity: PSP2
0 errores/271 avisos y Editor 0/269 en ambos controles. Tokens originales de Editor,
Editor+PSP2, Android, iOS, Standalone y sin símbolos coinciden para los dos fuentes.
Build nativo limpio: **18 escenas, BuildOptions.None, Mono2x/AOT PSP2, 0 errores,
296 avisos, 2:01,801**. No se interpreta el éxito del build como prueba en consola.

La comparación de 31.733 métodos frente al player anterior cambia solamente
`Player.UpdateTint`, `GameLogic.Start` y `GameLogic.Update`; agrega constructor,
Equals y GetHashCode del comparador, sin métodos retirados. Ocho auditorías de
descripciones UI, recursos, modelos de menú, optimizaciones previas, pantalla
limpia, calidad medium y guardado CPU/IO PASS sobre la DLL nativa. Su hash coincide
entre ambas construcciones; la procedencia de los controles reutilizados está
registrada. El antiguo auditor de HSEVitaPerformanceProbe es **NOT_APPLICABLE**:
requiere una sonda ausente también en el player anterior limpio, no una regresión
ni motivo para agregar HUD/logs de diagnóstico al paquete.

Se conservan dos intentos no usados como entrega: el control C# que coincidió con
la retirada temporal de DLLs Editor durante el build (repetición final PASS), y el
primer build que serializó estado temporal de dos prefabs de misión/dos materiales
y normalizó QualitySettings. Se respaldaron las variantes observadas y se
restauraron **exactamente los hashes iniciales** desde copias verificadas; el build
limpio posterior conserva esos recursos/configuraciones. No se arreglaron los
hashes cambiando la referencia del checkpoint. El verificador final cuenta 19.658
originales, dos fuentes modificados, cero faltantes y sólo comparador+meta nuevos;
todos los ProjectSettings/recursos originales coinciden. Los ocho archivos del
perfil posterior del dueño siguen idénticos; SaveBefore no se impuso sobre ellos.

El propietario solicitó expresamente un VPK privado tras terminar las sesiones.
Se creó `HSE_Gameplay_Profiled_HGSKE1178.vpk`: **135.097.531 bytes, 213 entradas**,
SHA-256 `c8f934129afd03034f538060c477b865107cba4fb24cb0c63a394369d1f8af02`.
CRC, igualdad exacta con el runtime limpio salvo SFO canónico, identidad HGSKE1178,
arte y campos de memoria/identidad previos PASS; sin savedata. Una comprobación
posterior del wrapper reutilizó una variable del empaquetador y falló; se corrigió
el aislamiento y se repitió la verificación independiente sobre el mismo VPK, sin
reescribirlo. El VPK anterior de transiciones conserva su SHA-256 original.

Entrega privada bajo `Reports/InteractiveProfiling/NativeValidationClean/Build/`;
no se agrega el paquete ni contenidos originales a Git. [Recibo público sanitizado](evidence/INTERACTIVE_PROFILING_2026-10-20.json).
La petición autoriza esta prueba privada; no publicación, instalación automática
ni aprobación de fases. P03 ACTIVE / 3 de 13 aprobadas, sin ramas nuevas.

Prueba física siguiente: respaldar la partida antes de actualizar el título
existente; usar relojes normales y repetir tiburón/ruta comparables, alimentación,
turbo/daño, zonas densas, pausa y muerte/regreso. Registrar frame times/FPS y memoria
del player, distinguiendo CPU/render/GPU y primeras cargas de repeticiones. Quedan
**UNTESTED** en Vita los 30 FPS sostenidos, los bytes de GC evitados por frame,
RAM/VRAM/picos, tiempos de carga y la matriz completa de tiburones/audio/input/save.
No se afirma que estas dos mejoras pequeñas resuelvan las caídas de 20–30 FPS.

## Restauración al acabar la investigación

Parar F9/Play y usar `HSE > Profiling > Restore session preview`: recupera calidad,
VSync, límite de presentación, keywords, selección Game View y módulos previos,
y apaga el opt-in de SessionState. Restaurar después los tres fuentes temporales
y el capturador anterior desde `Reports/InteractiveProfiling/Before/`, comprobando
sus hashes y preservando cualquier edición posterior. Archivar los helpers nuevos
y metas fuera de Assets. Las salidas del perfil de prueba deben guardarse por
separado antes de restaurar el perfil original; nunca sobreescribir progreso nuevo
del propietario sin distinguirlo de los datos de las pruebas.

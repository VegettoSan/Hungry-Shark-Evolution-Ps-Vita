# Auditoría y optimización de rendimiento — 2026-10-18

**Resultado:** tres fuentes del proyecto privado real corregidas y verificadas en
Unity **2018.2.19f1**. Se reduce trabajo y memoria demostrables; **no se demuestra
todavía una mejora de FPS en Vita**. Los **23–40 FPS sin overclock** son la referencia
comunicada por el propietario. Rama existente `main`, base `92f1f84`; excepción
específica de este encargo, **P03 ACTIVE / 3 de 13 aprobadas**, sin cambiar gates.

[Recibo de hashes y resultados](evidence/PERFORMANCE_INTEGRAL_2026-10-18.json).
Fuentes recuperadas, diffs, backups y perfiles se conservan privados en el proyecto,
`Reports/PerformanceIntegral_20261018/`.

## A. Hallazgos y prioridad

| Sistema | Evidencia y decisión |
| --- | --- |
| Mallas de rocas | **VERIFIED:** 103 `RocksExplosion`, 414 filtros asociados; 162 filtros con mallas instanciadas, 3.399.800 bytes de copias únicas según el Profiler del Editor. `Start` solo necesita leer vértices. Prioridad: evitar esas copias. |
| Cámara | **VERIFIED:** `ApplyUpdate` y `UpdateFOV` usan la variante de frustum que devuelve un arreglo nuevo. Captura previa: 299 asignaciones de 136 bytes bajo `GameCamera.LateUpdate` en 299 llamadas. La API instalada permite sobrescribir seis planos existentes. |
| Agua | **VERIFIED:** `Water.Update` consulta `Camera.main` repetidamente, incluso dentro de sus bucles. Es suficiente consultar la cámara actual una vez en cada invocación; no hace falta una caché persistente. |
| CPU de entidades | **VERIFIED Editor:** entre los scripts registrados destaca `SpawnerManager.Update`: 82,39 ms acumulados / 299 llamadas en una captura quieta, con un pico de 13,21 ms. `Player.Update`: 54,29 ms acumulados. Son tiempos inclusivos de Editor, no costes Vita. El manager ya usa cuadrícula, deduplicación por check ID, bounds y estados de activación. No se alteró su frecuencia ni elegibilidad. |
| Asignaciones de spawn | **VERIFIED:** hay asignaciones bajo el manager en Editor; la rama PSP2 ya incorpora la caché de componente ausente de D08 y el Editor usa otra rama. No se atribuyen automáticamente esas asignaciones a Vita ni se repite la reparación anterior. |
| Partículas/pooling | **VERIFIED estático:** existen listas de emisores reutilizables en `ParticleManager`, `BatchedParticleManager` y poolers. Las entidades tienen callbacks, estados e identidades que impedirían un pool global seguro sin contratos específicos. No se reconstruyen estos sistemas. |
| Texturas | **VERIFIED:** 680 importadores, dos legibles por CPU, 386 con mipmaps, ninguna fuente PNG mayor de 2048 px ni duplicados byte a byte. Censo nativo de 679 Texture2D: 379 DXT1, 291 DXT5, dos RGB24, siete RGBA32; cero BC7. El importador restante corresponde a un Cubemap. No justifica cambiar formatos, mipmaps o tamaños globalmente. |
| GPU/materiales | **UNTESTED en Vita:** coste GPU, overdraw, ancho de banda y cambios de estado durante alimentación/cámara alejada. Los materiales de agua y paredes contienen estado por instancia; compartirlos indiscriminadamente cambiaría efectos. Shaders, calidad, geometría y batching se conservan. |
| Cargas | **VERIFIED estático:** ya hay cargas asíncronas de escenas, preparación de menús y limpieza explícita en transiciones. No hay prueba de que sus `UnloadUnusedAssets`/GC sean redundantes. No se eliminan ni se introducen hilos para acceder a Unity. |

## B. Cambios aplicados

Rutas relativas al **proyecto privado Unity**, no fuentes distribuidas en Git:

- `Assets/Scripts/Assembly-CSharp/GameCamera.cs`: un buffer de seis planos por
  cámara, reutilizado en los dos puntos de cálculo. Conserva el mismo cálculo,
  frecuencia y culling; no cambia encuadre, zoom ni reglas de activación.
- `Assets/Scripts/Assembly-CSharp/RocksExplosion.cs`: lectura de `sharedMesh`
  en `Start`, conservando la copia de vértices y sus transformaciones. No se
  escriben datos compartidos ni se cambia `Play`, cantidades o fuerzas de rocas.
- `Assets/Scripts/Assembly-CSharp/FGOL/Water.cs`: referencia local a `Camera.main`
  dentro de `Update`. Sigue buscando la cámara cada frame, sin referencias obsoletas
  entre escenas; conserva bucles, posiciones, materiales y apariencia.

**VERIFIED integridad:** entre 19.648 archivos originales, únicamente esas tres
fuentes cambian; ningún archivo añadido o eliminado tras retirar diagnósticos.
Todos los ProjectSettings, escenas, prefabs, shaders, texturas, importadores y metas
originales conservan sus hashes. Ocho archivos de perfil restaurados byte a byte;
12 VPK anteriores intactas. No se toca Unity 5.6.

## C. Pruebas y mediciones

| Prueba | Resultado observado |
| --- | --- |
| Contrato de frustum | **PASS:** 1.000 cámaras con perspectiva/ortográfica, poses, FOV y aspect variables; planos exactamente iguales y mismas decisiones de culling. |
| 10.000 consultas de frustum, cinco rondas iniciales | Variante original: 1.564.672–1.568.768 bytes de delta administrado y 9,77–12,44 ms; buffer: cero bytes y 9,16–9,90 ms. Microbenchmark de Editor, no ahorro de frame Vita. |
| Contrato real de `RocksExplosion.Start` | **PASS:** mismos puntos transformados, fuente inalterada y ninguna copia de mesh en el fixture. Escena posterior: mismos 103 componentes/414 filtros, **cero filtros con clones** frente a 162 antes. Los 3,24 MiB anteriores son memoria del Editor, no ahorro de VRAM Vita medido. |
| Consultas de cámara | Cinco rondas, mismo resultado acumulado: 30.000 consultas repetidas, 2,19–2,73 ms; 10.000 consultas con variable local, 1,33–1,57 ms. Ahorro pequeño, sin inferencia de FPS. |
| Compilación completa C#4 / PSP2 sin Editor | Antes/después: **0 errores / 271 advertencias**, ninguna firma nueva. Perfil Editor posterior: 0 errores / 269 advertencias. No es un build del player. |
| Comparación CIL | **PASS:** 31.726 métodos comparados; solo cambian `GameCamera` constructor/`ApplyUpdate`/`UpdateFOV`, `RocksExplosion.Start` y `FGOL.Water.Update`. Control negativo rechaza el ensamblado anterior. [Auditor reproducible](../tools/audit_hse_integral_performance.ps1), PowerShell 7 con dnlib local. |
| Gameplay | **VERIFIED Play Mode:** arranque, menú, carga, movimiento por input lógico original, alimentación/colisiones, salud, pausa/reanudación, muerte natural a salud cero, popup, Evolución/tienda y segunda entrada al juego. El contador llegó a 18 consumos en la sesión posterior. No es una matriz exhaustiva de enemigos, tiburones o escenas. |
| Guardado | Inicio con el perfil existente y flujo de fin de partida ejercitados. Fixture del codec actual bajo Mono del editor: PSP2 conserva datos ficticios en cuatro culturas, copias/reload y rechazo de checksum/truncación/versión incorrectos. El control Editor conserva el comportamiento cultural original, incluidas sus limitaciones conocidas. **No se acredita save frío físico en esta ronda**; fuentes de save intactas y perfil real restaurado. |

Se incorporó la petición adicional de medir **movimiento y alimentación**. Secuencia
de 28 s: derecha 8 s, abajo 3 s, izquierda 8 s, arriba 3 s y derecha 6 s, con
intensidad lógica 0,8, sin teletransportar ni generar presas artificiales. Captura
en buffer; se excluyen los primeros tres segundos de los resúmenes:

| Editor DX11 | Ruta anterior | Ruta posterior |
| --- | ---: | ---: |
| Consumos totales de la captura | 10 | 14 |
| Frame mediano / P95 | 5,20 / 8,08 ms | 4,90 / 7,88 ms |
| Pico de frame | 20,50 ms | 20,26 ms |
| Pico en frames con incremento de consumos | 7,59 ms | 7,92 ms |
| Batches medios | 65,25 | 111,23 |
| GC muestreados después de 3 s | 5 | 6 |

**No es un A/B de FPS:** cambian posición inicial, salud, spawn, carga visible y
planificación/foco del Editor. La Game view era 300×205, con contenido desktop high,
no el medium ni la resolución del player Vita. La primera ruta mezcló carga y cap
restablecido; se retuvo como parcial y se corrigió el capturador para esperar gameplay.
Estos valores **no prueban una mejora global ni reproducen las caídas físicas**.
El Profiler también registró un GC de 12,58 ms en una captura posterior; no se
atribuye a una única causa ni se oculta mediante un límite de FPS.

No se capturaron nuevas excepciones C# de producción en las rutas probadas.
Persisten el diagnóstico conocido de IAP no disponible y mensajes de limpieza al
salir de Play. El fixture que demuestra `.mesh` produjo su aviso esperado en modo
edición y destruyó la copia. Fallos del diagnóstico quedan conservados: editores
duplicados cerrados, bloqueo transitorio del archivo de comandos corregido, y el
comparador CIL cambió a claves sensibles a mayúsculas. Los cambios de fuentes se
escribieron tras fallar un comando de parada; se completó la parada antes de las
pruebas posteriores. No se presenta esa secuencia como una parada inicial exitosa.

**VPK final no generada:** la preparación se detuvo antes de `BuildPipeline` porque
`PlayerSettings.PSVita.liveAreaPath` devuelve vacío, aunque el archivo de settings
contiene `PSP2System/BetterWinds/contents`. No se omitió esa validación ni se alteraron
ajustes para sortearla. Driver y diagnósticos propios archivados fuera de Assets;
Unity queda en MainMenu, fuera de Play y con preview desactivado.

## D. Siguiente comprobación física

1. Resolver de forma separada la discrepancia de LiveArea para producir un paquete
   con la configuración existente; comparar contra la VPK realmente instalada.
2. Misma Vita, shark, perfil, ruta/cámara y calidad, **frecuencias originales**.
   Registrar vídeo y frametimes/FPS al permanecer quieto, nadar, comer grupos de
   presas, activar turbo, alejar cámara y cruzar zonas. Separar primer uso de repeticiones.
3. Recopilar CPU principal/GPU cuando estén disponibles, GC/allocations, RAM y
   memoria gráfica; P50/P95/P99 y cantidad de frames >33,33/50/100 ms. La correlación
   con alimentación necesita esos datos para decidir entre spawn, física, partículas,
   transparencia y carga. Priorizar el coste dominante medido antes de introducir pooling.
4. Repetir paredes/rocas, agua/culling, pausa, muerte/reintento y varias cargas;
   comprobar memoria, render, sonido, save frío y sesión de 15–30 minutos.

Objetivo sigue siendo aproximadamente **30 FPS sostenidos sin overclock**. No se
modifica el límite original para afirmar estabilidad ni se certifica rendimiento de
consola a partir de este Editor.

## Seguimiento: VPK terminada por petición del propietario

**2026-10-18 — VERIFIED:** el bloqueo anterior queda resuelto. La ruta LiveArea
estaba vacía en memoria del Editor, mientras memoria/potencia/icono estaban correctos.
Se restableció mediante el setter nativo el mismo valor ya guardado; la verificación
original pasó y el archivo de ProjectSettings conservó su hash.

Build PSP2/Mono2x de 18 escenas: **0 errores / 296 advertencias**, 2:08,956, sin
nuevas firmas C# ni errores de shader. Auditorías nativas de las tres optimizaciones,
medium, save y navegación **PASS**. VPK privada de **135.088.469 bytes**, 213 entradas,
CRC y equivalencia con los archivos del player verificados; identidad HGSKE1178,
memoria y BetterWinds conservados. [Recibo](evidence/PERFORMANCE_VPK_2026-10-18.json).

Archivo privado: `Reports/PerformanceVPK_20261018/Build/HSE_Performance_Optimized_HGSKE1178.vpk`.
SHA-256: `44f93a771810f2a62d5e305af9ba9c621c70581a01e2813cc7b49deab23b0fbb`.

Código y ProjectSettings intactos respecto al checkpoint de este build; 12 VPK
anteriores conservadas. Se observó además un cambio de hash de `Underwater.mat`
después de compilar, conservado y copiado en `ObservedBuildDrift`. La reserialización
por Editor/build es **INFERRED**; el checkpoint tenía hashes, sin los bytes anteriores
del material, por lo que no se inventó una restauración. Revisar el agua al probar.
Driver retirado de Assets. **UNTESTED en Vita:** FPS, guardado frío y regresión física;
los resultados del Editor no se convierten en una afirmación de rendimiento real.

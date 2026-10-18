# Correcciones de navegación y rendimiento en Unity 2018

Fecha local del encargo: **2026-10-17**; evidencias: **2026-10-18 UTC**.
Base Git `ff92e69`, rama existente `main`, Unity **2018.2.19f1**.
Excepción específica solicitada por el propietario; **P03 ACTIVE / 3 de 13 aprobadas**, sin modificar gates.
Proyecto recuperado, backups, diffs y capturas del Profiler permanecen privados.

## 1. Diagnóstico

| Sistema | Hallazgo y evidencia |
| --- | --- |
| Laboratorio táctil | **VERIFIED:** tras desaparecer el último toque, `GameInput.CurrentControlMethod` vuelve a joystick; `TouchControls.UpdateTouchControls` retorna y conserva `released`. `LabCrane.MonitorSwipe` volvía a consumir ese gesto. Una liberación inyectada cambió repetidamente la selección durante varios segundos: recibos `b18`–`b24`. |
| Mando del laboratorio | **VERIFIED:** `LabCrane` solo consultaba su ruta de mando para Android TV; el adaptador Vita rechazaba el dummy sin collider. D-Pad no cambiaba la selección y el foco terminaba en otro botón (`b14`–`b17`). |
| Final de transición | **VERIFIED:** al alcanzar directamente el destino del tween, `StateMonitor` podía pasar a reposo antes de `UpdateSlide`, omitiendo el wrap. Se observaron posiciones fuera del anillo. Prueba corregida: 60 pasos deterministas con destinos completos, ambos sentidos, dentro de límites. |
| Modelos del laboratorio | **VERIFIED estático:** los modelos no seleccionados quedaban sin padre y el siguiente `Initialise` reemplazaba sus referencias. Ahora los nueve pertenecen al menú; inventarios de Editor confirman 9 → 0 → 9 al cerrar/reabrir. |
| Evolución | **VERIFIED:** una carga fría de `GUIPrefabs/UpgradeMenu` consumió 98,39 ms; instanciarlo, 6,38 ms. Constructor completo: 162,72 ms. La carga síncrona era una parte relevante, no la única. |
| Pausa | **VERIFIED:** apertura original 33,45 ms, de los cuales 24,96 ms estaban en construir contenido de opciones. El cierre medido fue 4,01 ms. La pila original ya controla `timeScale`; no se encontró evidencia para sustituirla. |
| Asignaciones | **VERIFIED:** `CheckTouchState` obtenía repetidamente copias de `Input.touches`, incluido un recorrido cuyo resultado no se utilizaba. `RocksExplosion.Start` copiaba `mesh.vertices` dentro del bucle por vértice. |
| CPU de gameplay | **VERIFIED en Editor:** predominó `WaitForTargetFPS`. Entre scripts medidos destacó `SpawnerManager.Update` (~0,27 ms por llamada en la captura inicial). Esto **no identifica el cuello de botella real de Vita**. No se cambiaron spawn, IA, física ni calidad. |

## 2. Cambios aplicados al proyecto privado

- `LabCrane.cs`: consumir y limpiar cada gesto una vez; ignorar gestos iniciados durante movimiento; conservar umbrales originales de 50 px / 0,4 s; corregir wrap antes de declarar reposo; propiedad de modelos inactivos y conservación de su escala mundial; simulación de ratón solo bajo `UNITY_EDITOR`.
- `HSEVitaNavigation.cs`: reconocer exclusivamente el dummy del laboratorio activo y enviar izquierda/derecha a `AttemptSlide`. FGOL sigue gestionando zona muerta, repetición, confirmación y regreso. No se añaden ejes ni otro lector de mando.
- `GameInput.cs`: consultar `touchCount` una vez y usar `GetTouch`; conservar orden, índice lógico, fases y actualización de posiciones del algoritmo original.
- `RocksExplosion.cs`: obtener los vértices una vez por malla. Conservar cada transformación y el contenido del efecto.
- `MainMenuSheet.cs` / `GUIManager.cs` y nuevo `HSEVitaMenuResources.cs`: preparar el prefab de Evolución durante la carga del menú principal y el de opciones durante la creación del HUD. Se conserva **como máximo un prefab fuente**, nunca una instancia viva; se libera la referencia al comenzar carga/descarga de partida. Cada pantalla sigue usando sus constructores, callbacks y destrucción originales.

La preparación desplaza trabajo a las cargas existentes; **no elimina ese coste del arranque**.
No se suprimieron las limpiezas originales `UnloadUnusedAssets`/`GC.Collect`: la prueba aislada de unload no justificó considerarlas redundantes.

## 3. Mediciones

Tiempos de CPU del Editor; no son latencia total con animaciones ni resultados de Vita.

| Prueba | Antes | Después | Alcance |
| --- | ---: | ---: | --- |
| Acción de entrada fría a Evolución | 167,67 ms | 64,44–66,08 ms | Carga preparada antes de la interacción; cuatro sesiones posteriores observadas |
| Primera apertura de pausa | 33,45 ms | 18,61 ms | Muestra por método; aperturas posteriores 12,54–13,38 ms, sin referencia repetida equivalente |
| Cierre/reanudación | 4,01 ms | 3,43–5,83 ms | Sin mejora demostrada; lógica original conservada |
| 10.000 consultas táctiles, cero toques | 708.608–712.704 bytes; 1,81–2,43 ms | 0 bytes; 0,60–1,36 ms | Cinco rondas; microbenchmark, no bytes de GC por frame de gameplay |
| 400 vértices de prueba | 3.276.800 bytes | 8.192 bytes | Tres rondas; todos los vértices transformados idénticos |

**Limitación observada:** una reapertura posterior de Evolución aún tardó **337,70 ms**; la preparación de un prefab no elimina la reconstrucción de modelos, accesorios y UI. La entrada al laboratorio mantiene una construcción síncrona de unos **2,4 s** al crear sus nueve modelos. No se implantó un pool global ni se omitieron inicializaciones para ocultar estos costes.

| Captura de gameplay de ~12 s | Antes | Después |
| --- | ---: | ---: |
| Frames | 360 | 359 |
| FPS calculados del tiempo medio | 29,94 | 29,87 |
| Frame medio | 33,40 ms | 33,47 ms |
| Percentil 95 | 34,13 ms | 33,86 ms |
| Pico | 93,80 ms | 119,19 ms |
| Recolecciones GC observadas | 0 | 0 |
| Eventos `GC.Alloc` medios/frame | 498,91 | 498,92 |
| Batches/draw calls medios | 89,24 | 110,94 |

**No es un A/B controlado de FPS:** las capturas quedaron limitadas cerca de 30 FPS y difieren en objetos/renderizado y momento de simulación. El propio diagnóstico genera asignaciones. Los eventos `GC.Alloc` no representan bytes/frame. **No se demuestra mejora ni regresión atribuible de FPS con estas capturas**, ni rendimiento validado en consola. Los picos siguen pendientes de medición reproducible.

## 4. Pruebas ejecutadas y límites

**VERIFIED en Unity Editor Play Mode:**

- Inicio desde `SplashLoaderScene`, menú principal, Evolución, selección normal y laboratorio; múltiples entradas/salidas.
- D-Pad anterior/siguiente; stick a 0,2 sin movimiento y 0,8 con navegación; pulsación mantenida y cambio rápido de dirección; liberación de entrada sin movimiento indefinido.
- Gestos cortos inyectados en el lector original, repetición de `released`, alternancia mando/gesto y gesto iniciado durante transición. Cada gesto aceptado produce un paso y se estabiliza.
- X sobre la acción original abre el diálogo; Círculo lo cancela y vuelve. El dummy es foco de navegación; bajar al botón conserva el flujo original de confirmación, sin compra de prueba.
- 60 pasos deterministas de tween completado en ambas direcciones, selección válida y tres grúas dentro del anillo. Escalas mundiales de los tres modelos coinciden con `GetSharkScale` antes/después de veinte pasos de la última sesión.
- Inventario de los nueve modelos: cero tras cerrar y nueve tras reabrir, sin acumulación.
- Gameplay, entrada proporcional y movimiento; pausa/reanudación repetida por API original, START y Círculo. Posición e input bloqueados en pausa; `timeScale` vuelve a 1. Regreso por resultados, tienda y segunda entrada a partida.
- Comparación de vértices y microbenchmark táctil; capturas CPU/rendering/physics/GC del Profiler de Unity 2018.

**PARTIAL / UNTESTED:**

- Se ejecutó un drag real de ratón que no cambió de tiburón; no se acredita con él un swipe corto aceptado. Los gestos cortos aceptados se verificaron mediante inyección en `TouchControls`. Falta repetir swipe corto con ratón manual y touch real.
- Sin joystick físico ni multitáctil real; no se validan números de botones, signos de ejes ni calibración física de Vita.
- No se ejecutó la matriz exhaustiva de alimentación/enemigos, muerte natural, todos los desbloqueos, accesorios ni paridad Android. No se certifica fidelidad global por los checks de escala.
- No hay soak largo, audio de pausa verificado auditivamente, save frío ni medición GPU de Vita.

No se capturaron nuevas excepciones C# de producción durante estas rutas. Permanecen mensajes conocidos de IAP no disponible y limpieza al cerrar Play; no se declara una consola globalmente limpia. Los fallos del diagnóstico se conservaron: API `Sample` ausente en el NGUI recuperado y `SendMessage("Update")` invocando otros componentes; la prueba final usa el destino real del tween e invoca solo `LabCrane.Update`.

## 5. Compatibilidad e integridad

**VERIFIED:** compilación con herramientas y referencias del editor instalado, C# 4 / símbolos PSP2 sin `UNITY_EDITOR`: **0 errores, 271 advertencias**, mismas firmas que la compilación previa con las mismas opciones. Es comprobación de fuente, **no build de player**.

Seis fuentes existentes modificadas y un helper con su meta añadido entre **19.644 archivos iniciales**. Todos los demás archivos originales, escenas, prefabs, materiales, importadores y **todos los ProjectSettings** conservan sus hashes; no falta ninguno. Cinco archivos persistentes restaurados byte a byte. Herramientas temporales retiradas de `Assets`, escena `MainMenu` restaurada, fuera de Play y preview desactivado. No se tocó Unity 5.6 ni se creó un VPK.

Pruebas y helper usan APIs disponibles en Unity 2018; el ratón de laboratorio y la instrumentación quedan separados por Editor. **UNTESTED en PSP2:** coste/residencia real del prefab preparado, picos y efectos de cargarlo antes durante el arranque.

## 6. Siguiente comprobación en Vita

1. D-Pad/stick, X/Círculo, soltar y alternar con touch; gestos rápidos, largos, cancelados y repetidos.
2. Reabrir laboratorio/Evolución varias veces; comprobar modelos, accesorios, memoria y transiciones con frames largos.
3. START/reanudar repetidamente; movimiento bloqueado, animación UI y audio correctos; volver y entrar de nuevo.
4. Capturar mismo shark/ruta/cámara/configuración antes/después: CPU/GPU, FPS/frame picos, RAM/VRAM y GC. Después, soak y save frío.

## 7. Git y evidencias

Se permaneció en `main`, sin crear ramas ni sobrescribir los archivos locales previos.
Por `AGENTS.md`, las seis fuentes recuperadas y sus diffs **no se incorporan al repositorio público**. Los commits de este encargo contienen la autorización/resultados sanitizados, diagnóstico original y helper original de recursos; las correcciones están aplicadas en el proyecto privado actual.

Evidencia privada: `Reports/EditorRegression_20261018/`: manifiestos antes/después, backups, diffs privados, recibos `b*`/`a*`/`c*`/`d*`/`e*`, capturas `.raw`, `FrameSummary.json`, `CompileResult.json`, `WarningDelta.json` y `SaveRestore.json`.
Resumen público de hashes: [recibo](evidence/EDITOR_NAV_PERF_2026-10-18.json). [Registro de experimentos](EXPERIMENTS.md).

# Estado de la recuperación Unity 5.6 — 15 de octubre de 2026

**Registro formal: P00 ACTIVE; P01–P12 LOCKED; 0/13 fases aprobadas.** [Ledger](PHASE_GATES.json) · [Plan y criterios](PORTING_PLAN.md) · [Experimentos](EXPERIMENTS.md).

**VERIFIED:** existe una recuperación privada que abre en Unity **5.6.4p3**, con compilaciones de código y pruebas de shaders realizadas. El propietario informa que entra en gameplay, mueve el tiburón con teclado y ratón y, después de las últimas correcciones, «parece estar todo bien». También confirma que ve el mosasaurio y los demás modelos. Es evidencia de una prueba breve del propietario; no equivale a completar la matriz de gameplay, disponer de un ejecutable PC probado o aprobar una fase.

Este documento actualiza el estado actual. [OFFLINE_RECOVERY_2026-10-15.md](OFFLINE_RECOVERY_2026-10-15.md) conserva el resultado histórico **anterior** a instalar/probar el editor. Las filas antiguas NOT TESTED siguen describiendo su momento; los experimentos posteriores aportan las comprobaciones nuevas.

## Archivos privados y trazabilidad

Los datos del juego permanecen fuera del repositorio, en el espacio privado del propietario. Se conservan:

| Carpeta relativa al espacio privado | Función |
| --- | --- |
| `Original_Data_Extracted/Data` | Datos originales reconstruidos y ensamblados Mono; fuente de investigación |
| `HSE642_AssetRipper_RawExport/ExportedProject` | Exportación original intacta; no abrirla para trabajar ni sobrescribirla |
| `HSE642_Unity56_ReadyToOpen` | Única copia Unity de trabajo |
| `Checkpoints/01_*.json` a `18_*.json` | Integridad, operaciones terminadas y ubicación de sus evidencias |
| `Reports/` | Informes, inventarios, fuentes de referencia, respaldos, diffs y resultados de pruebas privados |

Las rutas absolutas, código recuperado, programas de shaders, assets y logs completos no se incorporan a Git. Los informes de cada operación conservan sus respaldos y diffs. No se ha publicado contenido propietario ni generado un VPK.

## Recuperación y reparaciones realizadas

APK de referencia: `com.fgol.HungrySharkEvolution`, **6.4.2 / 228**, SHA-256 `52b7d8754ab8fec75c3b656a0dc5ec8dc849565adc402487a3ac093f6a481f7a`.

| Área | Resultado y límite de la evidencia | Checkpoint / experimentos |
| --- | --- | --- |
| Integridad y extracción | **VERIFIED:** hash, CRC y Unity 5.6.4p3/Mono; 2.295 archivos lógicos, 86 fragmentos reconstruidos en 15 grupos, 18 archivos levelN y 21 ensamblados. Integridad revalidada sin repetir la extracción | 01–02; OFF-001–002 |
| Exportación | **VERIFIED:** AssetRipper 2.0.0 importó Data y Managed conjuntamente. Copias original y de trabajo separadas. Manifiestos de 18.790 archivos raw y 2.295 extraídos preservados en las verificaciones de integridad | 03–04, 08; OFF-003, OFF-007 |
| Código C# y UnityScript | **VERIFIED:** 2.692 identidades de tipos de nivel superior representadas. Descompilación automática guardada en privado. Corregidos colisión HeatMap/Heatmap, metadatos ausentes y errores de descompilación con evidencia IL; no se reconstruyó manualmente la lógica del juego | 05–08; OFF-004–006 |
| CS0104 Google Play Games | **VERIFIED:** alias de `GooglePlayGames.Native.Cwrapper.Types` en ConversionUtils y NativeClient. Los tres errores informados quedaron corregidos; las compilaciones posteriores cubren las fuentes resultantes | 09; GPG-001 |
| CS0165 y Array | **VERIFIED:** flujo de lectura de ByteReader y capturas de XMLParser restaurados sin inicializaciones arbitrarias; cinco casts originales cualificados como `System.Array` en los tres SM_*. Regresión contra Mono original y compilación aislada pasan | 10; PAR-001–002 |
| NGUI Depth | **VERIFIED:** reconstrucción ShaderLab/Cg basada en GLES original, conservando regla de alpha/depth y GUID. No se desactivó depthPass. Su reparación inicial estática no debe confundirse con validación visual de todas las interfaces | 11; NGUI-001–002 |
| Servicios Android en Windows | **VERIFIED:** límites de plataforma en FGOLNativeBinding, PlatformUtils, DNAManager, FGOLAnalyticsDNA y AppsFlyer; después DeepLink e IronSource. Compilaciones y contratos aislados pasan; DeepLink devuelve ausencia e IronSource Failure sin recompensa simulada. Google Play aún puede advertir falta de autenticación | 12–13; PCSRV-001–004, SH15-005 |
| Primera recuperación de shaders y cámara | **COMPILA_EN_UNITY:** 15 shaders solicitados y HSE/BackgroundFish; 44 variantes probadas con ShaderUtil/SetPass, 88 etapas D3D aisladas. Se recuperaron operaciones de GLES y estados, conservando identidades. No son sustituciones genéricas | 13; SH15-001–004 |
| Materiales InternalErrorShader | **VERIFIED:** 56 materiales restaurados, incluido GodRay → Custom/GodRays. Se corrigió el tipo externo 2→3 tras pasar a ShaderImporter, conservando GUID/fileID y propiedades. El censo nativo de ese checkpoint terminó con cero materiales persistentes en InternalErrorShader | 14; MATERR-001–005 |
| Checkbox y memoria PSS | **VERIFIED:** adaptador tipado para la notificación UICheckbox al Unlock(bool) original; PSS limitado a Android. Pruebas nativas pasan y el propietario informa que desaparecen esos errores y entra al gameplay. No se inventó una medición de memoria PC | 15; CHECKPSS-001–004 |
| Movimiento Windows y fondo | **VERIFIED:** reutilización de las rutas originales teclado/ratón y ejes existentes, con alcance Windows. Propietario confirma movimiento. HSE/Background conserva su pase opaco original: se recuperaron textura/fog y conversión de profundidad; no se hizo transparente para ocultar el fondo blanco. Prueba de render aislada pasa | 16; INPUTBG-001–004 |
| Turbo | **VERIFIED:** Espacio o Button0/Ctrl izquierdo alimentan la ruta de acción original en Windows. Contratos aislados e IL pasan, sin cambiar UpdateBoost ni física. **UNTESTED:** el observador no capturó una tecla de boost mantenida; el informe general positivo del propietario no sustituye esa medición específica | 17; TURBOGFX-003–005 |
| Agua, frenesí y shaders adicionales | **COMPILA_EN_UNITY:** otros 28 shaders, 78 variantes y 156 etapas D3D aisladas; incluye HSE/Water, la familia SphereMap usada por Gold y los seis NGUI solicitados. 389 materiales enlazan su shader esperado; 419 campos de referencia corregidos. Pruebas de altura del agua y seis efectos NGUI pasan | 17; TURBOGFX-001–005 |
| Inicialización y cierre | **VERIFIED:** diagnósticos de gizmos y destrucción toleran managers aún no creados o ya destruidos. Diez fuentes afectadas; comparación IL conserva CanSpawn, UpdateBoost, física, estadísticas y lógica de spawning examinadas. Contratos de cierre sin propietarios pasan | 17; TURBOGFX-003–005 |
| Tiburones y modelos | **VERIFIED:** 20/20 prefabs jugables intactos; 2.957 mallas y sus meta idénticas al raw, coincidentes con el recuento de objetos del juego. 1.167 prefabs presentes; 981 clips, 12 controladores y 37 avatares intactos. Sin referencias ausentes en las dependencias examinadas. Mosasaurus/Mr Snappy tiene modelo de gameplay y selector; el propietario confirma verlo | 18; MODELS-001 |

Los recuentos de materiales de distintos checkpoints describen operaciones diferentes y no deben sumarse como un número de materiales únicos. El censo actual de fuentes contiene **46 shaders con nombres internos únicos** recuperados como `.shader`; eso tampoco significa que todos hayan pasado una comparación visual exhaustiva. Los nombres internos se determinaron mediante los datos originales, no normalizando los nombres de archivo solicitados.

GameCamera: la línea 298 requería **HSE/BackgroundFish**. **VERIFIED:** Shader.Find y el constructor de Material funcionan en la comprobación nativa, incluida la prueba antes de cargar expresamente el asset. **UNTESTED por el agente:** regresión completa GameCamera/CheckBossCam/TriggerBossCam/FogController durante todos los estados del juego. El propietario sí informa entrada a gameplay; no se ha desactivado la cámara para ocultar errores.

## Resultados que siguen abiertos

| Pendiente | Evidencia actual / condición de cierre |
| --- | --- |
| Comparación exacta del dorado | **FAIL conservado:** el primer diagnóstico del checkpoint 17 comparó filtrados CPU/GPU diferentes y difirió 0,051, superior a su tolerancia. También liberó una RenderTexture activa. El diagnóstico corregido compiló, pero no se ejecutó antes de retirarlo. No se cambió el shader para forzar el resultado. Falta comparación controlada y referencia visual Android |
| Aserción `m_AssetToPrefab.empty()` | Observada históricamente, sin prefab/GUID causal identificado; no apareció en el último intervalo limitado examinado. No se declara resuelta. Solo investigar profundamente si se reproduce, guardando contexto; ningún prefab fue eliminado indiscriminadamente |
| Referencias y serialización globales | El inventario inicial dejó 55 referencias uGUI pendientes de resolución nativa, ocho scripts nulos en seis prefabs y un mismatch de layout de MissionAchievementsManager. No existe un cierre global posterior de esos tres hallazgos. La auditoría limpia de tiburones no demuestra que todas las escenas estén libres de ellos |
| Preloads y datos de iluminación | Dos entradas nulas de precarga y avisos de datos de iluminación requieren diagnóstico propio si persisten. No se han reconstruido ni descartado por conjetura |
| Compilación/build | Hay compilaciones completas de fuentes con herramientas Unity en cuatro perfiles y pruebas dentro del editor. **UNTESTED:** build nativo PC completo, arranque del ejecutable y prueba independiente de Editor |
| Gameplay y fidelidad | Falta matriz registrada de alimentación, hambre/salud, muerte/reintento, audio, UI, guardado/carga tras cerrar el proceso, perfil nuevo y continuado; comparación con Android. No se afirma paridad de todas las animaciones, shaders o mecánicas |
| Estabilidad | Pruebas breves del propietario y ventanas limitadas del log no equivalen a una sesión larga sin errores. Falta prueba repetible, incluyendo carga, pausa/reintento y cierre |
| Otros motores y hardware | Unity 2018.2, PSP2, rendimiento y PS Vita física **UNTESTED**. No se inició migración ni adaptación Vita |

Los diagnósticos temporales de las intervenciones terminadas fueron retirados. Se conservaron copias y resultados privados. Las comprobaciones de IL se limitan a los tipos/métodos examinados; no son una demostración matemática de equivalencia de todo el juego.

## Matriz mínima para la siguiente validación

Esta tabla organiza la evidencia pendiente; no registra pruebas nuevas realizadas al actualizar la documentación.

| Caso | Evidencia disponible | Prueba que falta registrar |
| --- | --- | --- |
| Consola e importación | Compilaciones de fuentes y pruebas nativas de editor en distintos checkpoints | Importación actual, cero errores de compilación sin explicar y clasificación de avisos/referencias de la escena acordada |
| Inicio y carga | Propietario informa menú → gameplay | Repetir desde el ejecutable PC offline, guardando log y versión de build |
| Movimiento y cámara | Propietario confirma teclado/ratón; rutas originales conservadas | WASD/flechas, arrastre, seguimiento, pausas y transiciones; comparación Android |
| Turbo | Contratos de acción Windows pasan; última prueba general positiva del propietario | Mantener Espacio/Ctrl, observar consumo/recarga y efecto original en gameplay; registrar resultado específico |
| Comida, hambre y muerte | Lógica examinada conservada; sin matriz de ejecución completa | Comer, daño/hambre, muerte, reintento y retorno a menú |
| Gráficos y modelos | Shaders/materiales probados según los checkpoints; modelos conservados y vistos por el propietario | Comparación de agua, cáusticas, transparencias, escudos, frenesí dorado y animaciones con Android |
| Sonido e interfaz | Assets exportados; parte de la interfaz vista durante las pruebas | Música, efectos, pausa, menús y cambio de tiburón sin errores |
| Guardado y carga | No se alteró el formato ni se pretende simular servicios/progreso | Perfil nuevo y continuado, cerrar completamente el proceso y verificar persistencia; preservar el perfil actual |
| Servicios no disponibles | Contratos aislados evitan JNI/recompensas simuladas | Inicio offline del ejecutable y exclusiones claras de publicidad, compras, enlaces y autenticación |
| Repetibilidad | Copia raw y checkpoints conservados | Importación/build limpios y repetir la matriz; archivar base Unity 5.6 con recibo y aceptación P04 |

## Fase real y siguiente paso

| Fase | Estado formal | Avance técnico que puede revisarse |
| --- | --- | --- |
| P00 — fuente y entorno | **ACTIVE** | APK y editor 5.6.4p3 verificados en las pruebas realizadas. Falta cerrar la referencia Android, evidencia de ejecución del proyecto PC vacío exigida por el plan, inventario de entorno objetivo y aprobación formal |
| P01 — recuperación | **LOCKED** | Extracción, exportación, código, inventarios e integridad preparados. Revisar los hallazgos no resueltos y su clasificación cuando P00 quede aprobada |
| P02 — restauración en editor | **LOCKED** | Gran parte del trabajo técnico realizado por encargos expresos del propietario: fuentes compilan, shaders probados y gameplay accesible. Falta el cierre documentado de importación/consola, escena mínima acordada y componentes esenciales |
| P03 — gameplay PC | **LOCKED** | Prueba breve en Editor informada por el propietario. Falta ejecutable nativo y matriz completa comparada con Android |
| P04 — base estable | **LOCKED** | Falta repetición desde importación/build limpios, matriz de regresión y archivo reproducible aceptado |
| P05–P12 | **LOCKED** | No se han ejecutado ni se habilitan por estas reparaciones |

**Siguiente transición formal posible: P00 → P01**, después de aportar lo que falta de P00 y su aprobación explícita. El trabajo ya preparado puede presentarse para revisar P01 y luego P02 en orden, sin repetir una extracción íntegra ni dar por aprobados los hallazgos.

**Siguiente objetivo técnico recomendado: validar la base Unity 5.6 hasta P03/P04.** Primero registrar consola/importación actuales y las referencias pendientes, completar la referencia Android y después, con las fases correspondientes habilitadas, generar/probar el ejecutable PC y congelar la base. Los checkpoints permiten continuar las reparaciones Unity 5.6 expresamente autorizadas mientras se reúne esa evidencia; no autorizan saltar a otra fase.

No corresponde migrar aún a Unity 2018.2.19f1. P05 exige antes P04 aprobada y la confirmación del propietario **«Unity 5.6.4p3 native baseline accepted»**. Cada transición requiere dos referencias de evidencia y un enlace de aprobación del propietario en una issue/PR del repositorio, con usuario y fecha UTC, tal como exige el plan. Pedir este resumen, confirmar que se ve un modelo o indicar que una prueba breve va bien no constituye esa aprobación.

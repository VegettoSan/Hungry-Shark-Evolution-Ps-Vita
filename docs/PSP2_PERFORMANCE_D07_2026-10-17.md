# D07: CPU, caídas con zoom y pausas de interfaz

Encargo expreso de seguimiento del propietario sobre D06. P00 sigue ACTIVE; ningún resultado o petición se registra como aprobación de fase.

## Evidencia y límites

**VERIFIED por informe del propietario:** D06 ronda 30 FPS, baja hasta 11 en algunas zonas al alejar la cámara y presenta bloqueos breves al abrir pausa/cambiar menús. Informa memoria libre LPDDR 48,0 MiB, CDRAM 83,2 y PHYSCONT 26,0, y PSVShell muestra CPU 97 % / 51 % / 38 % / 22 %. No se recibió aún log con tiempos por hilo, frecuencia de CPU/GPU o duración exacta de cada bloqueo. No se da por confirmada paridad visual completa por el solo informe de rendimiento.

**INFERRED:** la muestra de memoria tiene margen y no reproduce el agotamiento inmediato del dump D04. No descarta picos ni fragmentación en otro instante. Un núcleo casi saturado es compatible con un camino crítico de CPU; los cuatro porcentajes no identifican qué hilo lo ocupa ni demuestran un reparto incorrecto. Scripts Unity, envío de dibujos, física, driver y sistema pueden contribuir. No equivale a una lectura de GPU.

**VERIFIED local:** el dump D04 ya identificó UnityGfxDeviceWorker, por tanto existe trabajo de render en un hilo auxiliar del player. gpuSkinning está habilitado. Activar tareas arbitrarias para MonoBehaviours o activar graphicsJobs sin soporte PSP2 demostrado no constituye una corrección válida. D07 registra SystemInfo.graphicsMultiThreaded y processorCount efectivos, sin modificar clocks, power mode, threading, física o frame cap.

Master_medium contiene 4.926 MeshRenderer frente a 1.686 de Master_low, con flags estáticos recuperados sin marcar batching. Esto no mide dibujos simultáneos ni prueba por sí solo que batching sea la solución. El manual **instalado** PSVitaProjectSettings.html documenta static/dynamic batching y sus costes variables. No se marcan objetos estáticos masivamente: antes se necesita identificar geometría inmóvil, shaders elegibles, coste de memoria y medida física. La memoria libre no es un permiso para duplicar sin medida toda la geometría.

## Menús: ruta real

**VERIFIED estático:** InGameGUI.SwitchInGameOverlayState(Pause) → GetPausePopup → GUIManager.GetPopup/CreatePopup → GUIMenu.AddPopup → GUIPopup.GetOptionsContent → OptionMenuSheet. Esta ruta instancia y prepara Popup/OptionsMenu; GUIManager.InstantiatePrefab hace Resources.Load, Instantiate, rebindeo de mensajes y controles. BaseMenuSheet.ButtonSetup añade componentes de partículas, recorre controles/colliders; OptionMenuSheet busca y configura widgets. Las búsquedas recursivas conservan su comportamiento. Esto identifica candidatos, todavía no atribuye sus tiempos en Vita.

Los GC.Collect explícitos de GUIPopup encontrados están en acciones StartGame/elección de controles; los UnloadUnusedAssets de GUIManager corresponden a cargas/transiciones. No se eliminan ni se culpa a esos métodos de abrir pausa sin una medida. Las animaciones de menú y el retardo de selección de 0,2 s tampoco prueban que todo el frame se bloquee.

## Prueba D07

Misma clasificación medium, texturas límite 1, escenas, shaders, sonidos HEVAG, memoria ampliada y límite 60 de D06. **Sin corrección de rendimiento de producción aún**: faltan medidas para elegirla. El constructor usa las mismas opciones de release, sin Deep Profiling ni cambio a un player development que altere la comparación.

[HSEVitaPerformanceProbe.cs](../tools/unity/HSEVitaPerformanceProbe.cs) mantiene un búfer máximo de 160 eventos, cuatro scopes de tipo prefab y tiempos totales de menús/overlay. Las etiquetas se limitan a las primeras llamadas y pocas recurrencias lentas. Registra intervalos Update, peor intervalo en ventanas de dos segundos, cantidad >50 ms y cambios de GC.CollectionCount(0). Un GC correlacionado no mide por sí solo su duración ni bytes asignados. Los intervalos Update no son tiempos exclusivos de CPU ni presentaciones físicas.

La sonda no escribe ni usa Debug.Log cada frame: guarda el informe solo al pulsar **Save diagnostic report** fuera del gameplay activo. El acceso de archivo se excluye del siguiente agregado de tiempos; un botón de diagnóstico puede introducir su propio coste. GUIStyle se crea una vez, el texto se actualiza dos veces por segundo y se dibuja en Repaint. D06 construía estilo/texto en cada evento OnGUI; se evita ese coste del propio instrumento sin atribuirle todas las caídas.

Recorder y FrameTimingManager existen en las assemblies instaladas de 2018.2. Se intentan los contadores Camera.Render, BehaviourUpdate, GC.Collect y tiempos de frame. Cero muestras, datos ausentes o excepción se etiquetan **unavailable**, nunca 0 ms ni prueba de GPU libre. El manual instalado PSVitaProfiling.html exige development build para el profiler conectado: D07 no afirma sustituir ese perfil completo. No se promete soporte de cada contador en este release PSP2.

El botón táctil **A/B: camera draw OFF** conserva la máscara y pone cullingMask=0 solo en GameLogic.gameCamera.UnityCamera durante HUD activo. No desactiva GameCamera ni cambia su posición, zoom, físicas, spawners, materiales, IA o texturas. Quedan las demás cámaras/efectos: no es un apagado absoluto de GPU. Se restaura con el mismo botón y automáticamente al pausar, cambiar escena/estado o retirar la sonda. Es un control de diagnóstico que oculta dibujo, no un modo de juego ni una optimización final.

**PASS fixture de editor:** [HSEPSP2PerformanceAudit.cs](../tools/unity/HSEPSP2PerformanceAudit.cs) comprueba restauración exacta de máscara en cámara genérica inactiva, limpieza del estado A/B y evento de timing en búfer. No instancia la UI de edad ni toca partidas. Veinte comparaciones de ramas conservan los tokens previos en editor PSP2, Android, iOS y sin símbolos para los cinco archivos instrumentados.

## Pasos físicos y siguiente decisión

**VERIFIED compilación PSP2 Mono2x:** Succeeded, **0 errores / 296 advertencias**, 2 min 3,41 s; tamaño reportado 246.762.580 bytes. Mismas advertencias C# que D06; WarningDelta no contiene errores de shader. Auditoría [audit_hse_vita_performance.ps1](../tools/audit_hse_vita_performance.ps1) confirma scopes en los siete métodos previstos, control solo por cullingMask y restauración, y ausencia de la sonda D06. NativeQuality confirma medium; SDK offline PASS, 302 HEVAG/tres PCM, 720 texturas sin BC7. No es verificación de FPS.

**VERIFIED paquete privado:** `Reports/Performance_D07_20261017/Build/HSE_Performance_D07_Medium_HGSKE1178.vpk`, **134.926.353 bytes**, SHA-256 **`fb26c5f8fb3db7a9d4a05399327bc31c738933e12b3f6a240b5ce4d53c0d2eb2`**. 213 entradas, CRC/runtime-byte checks PASS, ATTRIBUTE2=0xc mantiene ampliación. No publicado ni instalado automáticamente.

**PASS reversión/integridad:** después de empaquetar se restauran los cinco archivos instrumentados desde Before y se guardan copias exactas instrumentadas, constructor, fixture, sonda/recurso y metas en DiagnosticFiles fuera de Assets. Los 19.619 archivos iniciales de 2018 y los 19.591 de referencia 5.6 siguen idénticos, sin nuevos ni ausentes. VPK D05/D06 mantienen sus hashes. El proyecto de trabajo conserva el ajuste medium D06 y no deja temporizadores de diagnóstico activos. El VPK D07 sí contiene la sonda temporal para esta prueba. Evidencia privada: BeforeManifest, Before, BranchEquivalence, FixtureResult, BuildResult, NativePerformance/NativeQuality/NativeSDKContract, Audio/Textures, WarningDelta, PackageReceipt e IntegrityAfter.

1. Usar la misma zona/tiburón, reloj y condiciones de D06. Dejar estabilizar la carga y observar NORMAL durante varios segundos en el punto donde bajaba a 11 FPS.
2. Pulsar A/B para ocultar dibujo de esa cámara y esperar unos segundos, manteniendo posición y zoom lo más parecidos posible. Registrar FPS, CPU de PSVShell y la franja. Restaurar dibujo con el mismo botón. Una mejora grande implica coste importante de ese camino de render, todavía sin separar envío CPU y GPU; poca mejora deja abiertos scripts, otras cámaras y efectos.
3. Abrir/cerrar pausa y las mismas pantallas del menú dos veces. Comparar primera y segunda apertura. La franja muestra la última operación de menú y su máximo; el informe separa carga/instanciación/bind/buttons y totales anidados, que no deben sumarse entre sí.
4. En pausa o menú, pulsar Save diagnostic report. Conservar la franja y el archivo `HSE_Perf_D07_*.txt` creado mediante Application.persistentDataPath, la ruta de log que ya funcionó. No inventar una ruta ux0 directa ni borrar saves.

**Seguimiento VERIFIED por informe del propietario:** ocultar dibujo añadió aproximadamente 5–10 FPS, pero la imagen quedó quieta. Hubo una pequeña mejora con la otra prueba; el informe guardado no resultó legible y fue borrado. Sin el archivo no puede atribuirse su formato ni recuperarse el perfil. Ocultar la cámara no es una optimización utilizable; esa opción se retira de D08. La mejora sugiere coste de ese camino de dibujo, todavía sin separar CPU/GPU ni explicar por completo las caídas. Coste por operación, soporte de contadores y estabilidad prolongada siguen **UNTESTED**. Véase [D08: guardado y CPU](PSP2_SAVE_CPU_D08_2026-10-17.md). D05/D06/D07 se conservan como puntos de recuperación.

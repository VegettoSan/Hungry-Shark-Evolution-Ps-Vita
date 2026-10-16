# Auditoría puntual de ejecución Unity 2018.2.19f1

Encargo expreso del propietario: diagnosticar/reparar la copia independiente de migración conservando Unity 5.6. **P00 ACTIVE; 0/13 fases aprobadas.** No se modifica el registro de fases ni se acredita paridad completa de gameplay.

## Evidencia antes de reparar

**VERIFIED:** se preservaron Editor.log de Unity 2018.2.19f1 (`06990f28ba00`) y Editor-prev.log, identificado como Unity 5.6.4p3. El análisis completo de las 103.684 líneas del primero selecciona **1.035 mensajes diagnósticos y 43 firmas normalizadas**, con ejemplos y primera/última línea. Estos recuentos no son el total exacto de entradas de la consola; se excluyen mensajes informativos y 97 ocurrencias históricas de errores de compilación en transcripciones duplicadas. No existen marcas horarias por cada mensaje: no se inventaron timestamps.

| Cadena dominante | Repeticiones |
| --- | ---: |
| NullReferenceException en AnimControl.Update | 896 |
| NullReferenceException en AnimControl.ApplyRateScale | 38 |
| NullReferenceException en AnimControl.RandomizeFrame | 3 |
| Otros mensajes de estados/clips no encontrados | 58 |

**VERIFIED:** los avisos de claves de animación ausentes aparecen antes de las excepciones. Las fuentes de AnimControl antes de la reparación son idénticas a Unity 5.6. No se demostró un fallo previo de un manager compartido como causa de esta cadena.

## Causa y cambio mínimo

**VERIFIED mediante consulta nativa en la sesión existente:** los diez clips legacy del Reef están presentes y sus referencias resuelven. Unity 2018 los expone bajo los nombres de archivo exportados, mientras el controlador conserva los nombres originales de los estados. Las diez consultas originales fallan antes de reparar. El contenido fuente de los 981 clips coincide por SHA-256 con la copia 5.6; en 804 casos el nombre de archivo difiere del nombre original serializado. **INFERRED:** el tratamiento del nombre al importar explica la diferencia entre motores; no se instrumentó el importador ni se atribuye este cambio al API Updater.

La reparación restaura exclusivamente las claves `AnimationState.name` durante la inicialización de AnimControl, usando una tabla exacta derivada de los nombres serializados originales. No se eliminan sufijos por heurística, no se sustituyen clips ni se cambian sus curvas. Una prueba nativa aislada conserva la misma instancia de estado, velocidad y cantidad de clips al renombrarlo. [Unity documenta el nombre del estado como propiedad de AnimationState](https://docs.unity3d.com/kr/current/ScriptReference/AnimationState.html); la compatibilidad con este editor se comprobó localmente, no se dedujo de la documentación actual.

Cambios privados permanentes: una llamada condicionada a la existencia del componente Animation en `AnimControl.cs`, más el helper generado `HSE2018LegacyAnimationNames.cs` y su nuevo meta. El helper no modifica tasas, pesos, eventos, transiciones ni reglas de gameplay. No añade campos serializados al controlador.

## Validación y ensayo fallido conservado

**PASS CLI:** Mono mcs 5.11.0.0 del editor 2018.2.19f1, opciones/dependencias de la sesión con salida privada: cero errores y 262 advertencias. La sesión existente también recompiló y recargó el ensamblado antes de probar. No se inició otra instancia ni un build de player/PSP2.

**FAIL parcial, primer ensayo:** la llamada inicial del adaptador también alcanzaba controles sin Animation y produjo 44 excepciones nuevas. Se detuvo la prueba, se conservó su log y se limitó únicamente la nueva llamada a componentes existentes. La ruta original restante permanece intacta; no se añadió un retorno ni un try/catch para ocultarla.

**PASS limitado, segundo ensayo corregido:** menú → gameplay Reef → salida de Play Mode, HUD observado durante unos 30 segundos. El observador registró 36 muestras con jugador durante aproximadamente 36 segundos, hasta **22 controladores inicializados y cero estados activos ausentes**. Se observaron estados swim/bite y avance del tiempo de animación. Un arrastre breve produjo un pequeño cambio de posición/orientación; no se declara validación completa del movimiento.

En esa ventana, las **937 excepciones previas y los 58 mensajes asociados de animación pasan a cero**. Permanecen ocho avisos Missing Script y dos avisos de iluminación. Son ventanas distintas en duración/contenido; no se presenta una tasa comparable ni una demostración de ausencia de errores futuros.

## Pendientes clasificados

| Categoría | Hallazgo / límite |
| --- | --- |
| A — migración | Causa de nombres de estados corregida y probada en ejecución breve. No se cambiaron ajustes de runtime. |
| B — referencias | Ocho componentes con m_Script nulo en seis prefabs, idénticos a 5.6. Incluyen los avisos ElectroShockAbility presentes en ambos logs. Sin identidad original demostrada para restaurarlos; no se borraron componentes ni inventaron remaps. |
| C — servicios/red | Sin JNI ni excepciones de servicios Android en el segundo ensayo. Un fallo HTTPS de descarga de configuración apareció en el primero; gameplay continuó. No se alteró TLS ni se simuló una respuesta. |
| D — gráficos | Dos avisos Realtime GI distintos entre escenas aditivas persisten. El log histórico contiene 28 avisos shader unsupported/fallback de vídeo, AR, VR y NGUI; no reaparecen en la segunda ventana. Esto no prueba fidelidad visual global. |
| E — gameplay | Inicialización Animation y claves activas observadas. Turbo mantenido, alimentación, hambre/muerte, progresión, todos los tiburones, audio y persistencia tras reiniciar requieren pruebas completas. |
| F — editor | Dos ReflectionProbeAnchorManager durante descarga/importación de jerarquías previa; ninguno al entrar/salir del segundo ensayo. No hay objeto causal reproducible; no se modificaron probes, prefabs ni escenas. |

## Integridad y evidencia privada

**VERIFIED:** manifiestos completos antes/después de Assets y ProjectSettings: **19.591 archivos Unity 5.6 sin modificaciones, adiciones ni eliminaciones**. En Unity 2018, solo cambia AnimControl y se añaden el helper y su meta; ningún archivo preexistente desaparece y ningún meta existente cambia. El diagnóstico Editor temporal fue retirado, conservando fuente/meta en Reports. Se preservó una copia de los datos persistentes antes de las pruebas; no se editó el formato ni se probó una regresión completa de guardado.

Los respaldos, SHA-256 individuales, tabla de nombres, diffs y logs completos permanecen en el proyecto privado, bajo `Reports/RuntimeAudit_20261016/`. `Informe_final.md` incluye las métricas, diez firmas más frecuentes, clasificación, archivos y prueba siguiente. `grouped.json`, `Attempt2Analysis/grouped.json`, `Metrics.json`, `ProjectIntegrityDiff.json`, `repair_receipt.json` y los logs nativos permiten reproducir las conclusiones sin publicar contenido propietario.

Siguiente prueba del propietario: conservar el log; repetir 2–3 minutos con Reef y otro tiburón, giros, turbo sostenido, comida, daño/hambre y muerte/reintento; comparar cámara/HUD, animaciones, audio y efectos; comprobar persistencia con perfil respaldado. Investigar cualquier firma nueva desde su primera causa, sin borrar Missing Script. No se utilizó PS Vita física ni se generó VPK.

# Recursos y trabajo runtime PSP2 — 2026-10-18

**Resultado:** cuatro correcciones pequeñas en el proyecto privado Unity
**2018.2.19f1**, rama existente `main`. Excepción puntual autorizada por el encargo;
P03 ACTIVE / 3 de 13 aprobadas, sin transición de fases. Los **23–40 FPS** proceden
del propietario; esta ronda no mide una mejora física. Código recuperado, backups,
diffs y perfiles permanecen privados en `Reports/ResourcePerformance_20261018/`.

## Diagnóstico y cambios

Las cuatro correcciones son **categoría 1** para las reducciones objetivas observadas
en código/Editor y **categoría 2** al aplicarlas a los recursos limitados de Vita.
La magnitud de mejora de FPS, picos y RAM/VRAM física sigue siendo **categoría 3**,
pendiente de medición; no se presupone a partir del PC.

| Fuente privada | Evidencia y corrección PSP2 | Clasificación |
| --- | --- | --- |
| `AnalyticsFPSHeatmapRunner.cs` | `GameLogic` crea el componente en cada partida aunque `FGOLAnalytics.Initialise` no construye remitentes en PSP2. El Profiler previo registra 253 `Update` y una entrada de corrutina. Ahora queda deshabilitado, sin `RollingAverage`, mapa ni espera inicial. Conserva la interfaz consultada al terminar: estadísticas indisponibles `-1`, mapa nulo. Los contadores de progreso de `FGOLAnalytics` siguen ejecutándose. | **VERIFIED:** trabajo eliminado; beneficio temporal Vita pendiente. |
| `GUITextureLoader.cs` | `LoadTexture` podía crear materiales sucesivos sin destruir los anteriores; `NewGamePromo` llama explícitamente además del ciclo de activación. Ahora reutiliza textura/material únicamente mientras pertenece al mismo loader y ruta. Cambiar ruta o desactivar conserva `Cleanup`, destrucción y descarga originales. Sigue reasignando UI, tamaño y refresh; no añade caché global. | **VERIFIED:** 16 llamadas crean 1 material frente a 16; menor presión de memoria técnicamente fundada para Vita. |
| `ParticleManager.cs` | El filtro de edad convertía el ID a texto y comprobaba el prefijo `blood` en emisión/efectos adjuntos. PSP2 compara directamente los dos IDs existentes. Se conserva la consulta de edad, emisión, pooling y efectos. | **VERIFIED:** misma decisión para 156 casos; elimina conversión/boxing de esas rutas. |
| `HSEAdManager.cs` | El motor ya tenía `StartEngine` vacío en PSP2, pero `Initialize` cargaba/descargaba su TextAsset local de configuración. Se omite solo esa carga. Se mantiene `AdManager` para sesiones/getters y el resultado de fallo de anuncios. La configuración local compartida todavía puede proporcionar JSON; no se atribuye su desaparición. | **VERIFIED estático/binario:** una ruta de carga eliminada; ahorro pequeño de arranque, sin estimación inventada. |

Las ramas no PSP2 conservan sus tokens ejecutables: **24 comparaciones PASS**.
No se alteran shaders, materiales de escena, texturas/importadores, calidad, físicas,
spawn, IA, animación, controles ni codec de guardado.

## Servicios encontrados: presencia frente a ejecución

| Sistema | Estado revisado |
| --- | --- |
| JNI / AndroidJava / bindings iOS | Existen fuentes y wrappers. `IsAndroidPlayer` retorna falso en PSP2; la inicialización JNI retorna antes de construir Java. No son por sí solos carga runtime. Se conservan rutas locales usadas por el juego. |
| Google Play / compras | Fuentes presentes. La fábrica PSP2 selecciona el proveedor IAP no disponible; no se construye el proveedor Google en esa raíz. Se conserva inventario local y validación pendiente; nada simula compras correctas. |
| Facebook/social | Fábrica PSP2 usa `DisabledFBAccess`; sin referencia al runtime Facebook en el ensamblado. No se borran interfaces ni funciones locales de progresión. |
| IronSource/publicidad | Motor sin arranque ni corrutina periódica en PSP2. Contratos de permitir/mostrar anuncio devuelven fallo una vez, sin recompensa. Solo se elimina la carga local indicada arriba. |
| DNA/Ubisoft/AppsFlyer | Sin remitentes PSP2; `DNAManager.Initializate` no inicializa SDK ni añade listener de guardado. Se preservan identidad, metadatos y contadores que también usa gameplay. El recolector de FPS era una tarea activa residual: corregido. |
| Cloud save/configuración/contenido remoto | Se conservan guardado e información local. Las exclusiones PSP2 existentes evitan autenticación/upload y la carga de proveedores remotos; configuración responde indisponible con callback. Se conservan sus contratos, sin nuevo gestor. |
| Push / Firebase / Google Mobile Ads / permisos | Hay tipos de notificaciones del SDK Ubi: su presencia no prueba un proceso push activo. No se encontraron referencias C# a Firebase, GoogleMobileAds ni `UnityEngine.Android.Permission`. No se eliminan tipos por nombre ni se afirma una auditoría de cada posible entrada nativa. |

## Validación y límites

- **PASS Editor:** arranque con perfil existente, entrada/salida de Evolución,
  selección mako/reef, laboratorio, 20 pasos completos de carrusel, liberación de
  nueve modelos al cerrar; tienda/personalización y segunda partida. El laboratorio
  se abrió por su constructor original desde el diagnóstico, sin desbloquear progreso.
- **PASS Play Mode:** movimiento con input lógico original, dos consumos,
  pausa/reanudación, salud/posición detenidas en pausa, muerte natural y regreso a
  Evolución/tienda. Tres fuentes de audio reproduciéndose al activar su control;
  estado de mute inicial restaurado. No constituye prueba física de controles/audio.
- **PASS recursos:** cambio de ruta, limpieza y recarga; anuncios fallan una vez.
  Microbenchmark de 10.000 predicados: conversión original ~1,6–1,8 MiB de delta
  administrado; comparación directa cero. Mismos 446 resultados positivos. Es un
  fixture del Editor, no ahorro medido por frame ni RAM/VRAM de consola.
- **PASS Profiler:** captura posterior con 299 `GameLogic.Update`, cero eventos del
  recolector/corrutina. El componente real está deshabilitado y hay cero remitentes.
  Las capturas globales tienen carga/posición/spawn/salud distintos: no permiten
  atribuir diferencias de FPS, P95, picos ni memoria al parche.
- **PASS C#4/Mono:** PSP2 antes/después 0 errores / 271 advertencias; Editor
  0 / 269. Comparación de 31.720 métodos: diez cuerpos cambian exclusivamente en
  las cuatro clases previstas, sin métodos añadidos/eliminados.
- **PASS codec actual con datos ficticios:** PSP2 en cuatro culturas, copia/relectura,
  checksum, truncación y versión. El perfil Editor reproduce su comportamiento
  cultural previo, incluida corrupción esperada fuera de en-US. Guardados reales
  y preferencias no se usan en ese fixture. No se afirma una prueba fría física.
- Una compilación de control sin PSP2 falla antes y después por la misma visibilidad
  preexistente de `Facebook.Unity.LoginResult`. No se corrige fuera de alcance;
  la preservación de esas ramas se verifica por tokens, no por un build Android/iOS.
- No aparecen nuevas excepciones de producción en las rutas probadas. Persisten
  IAP no disponible y avisos de limpieza. El fixture previo fuera de Play produjo
  el aviso esperado de `Destroy`; no se suprime ni se presenta como fallo de gameplay.

**PASS build nativo:** PSP2/Mono2x, 18 escenas, **0 errores / 296 advertencias**,
2:26,671. Auditorías del player de recursos/servicios, optimizaciones previas,
medium, guardado y ausencia de overlays PASS; el control negativo rechaza el
ensamblado anterior. No se publica ni se instala el player.

**VERIFIED integridad:** de 19.648 archivos originales, solo cambian las cuatro
fuentes indicadas; sin adiciones/pérdidas finales. Metas, escenas, texturas, shaders
y ProjectSettings conservan hashes. Se detectó un cambio incidental del offset X
de `Underwater.mat` durante Play/build: se conserva la variante observada y se
restauran los bytes iniciales desde un backup privado previo cuyo SHA-256 coincide
exactamente con el checkpoint. La causa por serialización del Editor es **INFERRED**;
no se atribuye al parche ni se altera el shader. Ocho archivos de perfil restaurados
byte a byte; resultados de prueba conservados. Diagnósticos propios retirados;
Editor en MainMenu fuera de Play, simulación de mando desactivada.

## Pendientes concretos

Evolución aún tiene entradas/reconstrucciones de ~329–380 ms y el laboratorio
~2,46 s; tienda ~850 ms en muestras de Editor. Los constructores instancian UI,
modelos y accesorios y fuerzan limpiezas. Estas muestras no son un A/B controlado
ni equivalen a tiempos Vita. No se eliminan `GC.Collect`/`UnloadUnusedAssets` sin
medir su necesidad de memoria. Preparar todos los tiburones ampliaría la residencia.

Siguiente diagnóstico: separar en Vita construcción de attach points/modelos,
UI, descarga/GC y primer uso frente a repetición; capturar RAM/VRAM máxima antes
de decidir reutilización adicional. Para gameplay: misma ruta/shark/cámara/perfil,
frecuencias originales, alimentación de grupos, P50/P95/P99 y frames >33/50/100 ms;
separar CPU/GPU y probar 15–30 minutos, guardado frío y suspend/resume.

[Auditor binario reproducible](../tools/audit_hse_resource_runtime.ps1) y
[recibo sanitizado](evidence/RESOURCE_RUNTIME_2026-10-18.json). Las fuentes privadas
no se distribuyen mediante Git; los commits públicos contienen documentación y auditor.

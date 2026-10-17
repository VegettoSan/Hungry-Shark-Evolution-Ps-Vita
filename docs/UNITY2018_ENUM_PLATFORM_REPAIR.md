# MemDump y condiciones NGUI — 2026-10-16 UTC

Intervención expresamente solicitada por el propietario el 15 de octubre (Colombia), limitada a la copia privada Unity 2018.2.19f1. Sin aprobación de fases: P00 ACTIVE, 0/13 aprobadas. El propietario confirma funcionamiento de Unity 5.6; esta intervención no vuelve a probar su gameplay ni modifica esa copia.

## MemDump: causa y corrección

**VERIFIED:** se examinó el switch completo de `GetBitsPerPixel`. Las líneas 83 y 85 de la copia 2018 usaban ambas `TextureFormat.ETC_RGB4` y ambas devolvían 4. Son miembros de un enum, no constantes numéricas escritas en el switch. El compilador informa CS0152 por el valor repetido 34.

**VERIFIED:** la copia 5.6 y el código original recuperado distinguen ETC_RGB4 → 4 bits, ATC_RGB4 → 4 bits y ATC_RGBA8 → 8 bits. La lectura de metadatos de los ensamblados instalados, mediante dnlib local, confirma:

| Miembro | Unity 5.6.4p3 | Unity 2018.2.19f1 |
| --- | ---: | ---: |
| ETC_RGB4 | 34 | 34 |
| ATC_RGB4 | 35 | 35 |
| ATC_RGBA8 | 36 | 36 |
| ETC2_RGBA8 | 47 | 47 |

**VERIFIED:** frente a la copia 5.6, el código 2018 había sustituido ATC_RGB4 por ETC_RGB4 y ATC_RGBA8 por ETC2_RGBA8. El log registra CS0619 con esas sustituciones `UnityUpgradable` y una ejecución posterior de `ScriptUpdater.exe`. **INFERRED:** el API Updater realizó las sustituciones observadas; no se capturó cada escritura del proceso. Los valores del enum no cambiaron.

Se eliminó únicamente la segunda pareja duplicada `case ETC_RGB4 / return 4`, manteniendo una sola rama equivalente. Se conserva la rama ya migrada ETC2_RGBA8 → 8 y todos los demás casos, default y cálculo de memoria. No se cambian valores de enumeraciones ni se inventan casts para conservar formatos ATC retirados. La conservación semántica se refiere a los formatos migrados/soportados: no se afirma equivalencia numérica para valores ATC históricos 35/36.

## Cuatro condiciones NGUI

**VERIFIED:** `NGUIMath.ApplyHalfPixelOffset` (dos sobrecargas), `UIAnchor.OnEnable` y `UIPanel.UpdateDrawcalls` incluían la alternativa WindowsWebPlayer junto a WindowsPlayer y WindowsEditor. UIAnchor guarda el resultado en `mIsWindows`, usado por su ajuste de medio píxel; UIPanel ajusta el centro de su región de clipping. Las dos sobrecargas NGUIMath aplican el mismo ajuste, una de ellas condicionado por la paridad de la escala.

Se retiró únicamente la alternativa `WindowsWebPlayer` de esas cuatro expresiones OR. No se sustituyó por WindowsPlayer, que ya estaba presente. Permanecen literalmente las alternativas WindowsPlayer/WindowsEditor y los cuerpos de cálculo de posición, escala, anclaje y clipping. Para cualquier plataforma distinta del WebPlayer retirado, la condición produce el mismo resultado. No cambian firmas, campos ni identidades serializadas.

## Verificación y alcance

**PASS (VERIFIED, compilación CLI de Assembly-CSharp):** Mono mcs 5.11.0.0 de Unity 2018.2.19f1, usando el response file más reciente de la sesión (`UnityTempFile-d7019f58466e1f44cb464e35fa658799`), modificando únicamente `-out` hacia el informe privado. Resultado: **exit 0, cero errores, 262 advertencias**. No aparecieron errores nuevos. Esta configuración existente incluye C#4, UNITY_EDITOR_WIN y UNITY_PSP2; no se cambió el target, no se ejecutó el pipeline de build ni se generó un player/VPK. Se usan las dependencias ya compiladas de la sesión; esto no equivale a un build limpio de todos los ensamblados ni a validación de ejecución.

**VERIFIED:** solo se escribieron las cuatro fuentes de la copia 2018. La comparación exacta contra los respaldos confirma el alcance mínimo descrito. Los hashes de las cuatro fuentes correspondientes de Unity 5.6 y de los cuatro `.meta` 2018 permanecen iguales. No se escribieron escenas, prefabs, shaders, ajustes ni guardados.

Evidencia exclusivamente privada, dentro de la copia 2018: `MigrationDiagnostics/EnumsPlatforms_20261016T044317Z/`. Contiene cuatro respaldos `*.before.cs`, `receipt.json` con SHA-256 antes/después, `TextureFormat_metadata.json`, `verification.txt`, `compile.rsp` y `compile.log`. No se publica código recuperado ni contenido propietario.

## ReflectionProbeAnchorManager

**VERIFIED:** la revisión del Editor.log tras esta compilación CLI muestra solo las dos ocurrencias antiguas, durante descarga de archivos serializados/importación, en Transform.cpp:1704–1705. No hay una recarga nativa posterior registrada que permita comprobar el resultado tras estas correcciones. **UNTESTED:** persistencia tras recompilar/recargar en el editor. No se declara resuelto ni se atribuye a estos scripts; no se alteraron probes, escenas, prefabs ni componentes.

Siguiente comprobación: dejar que la sesión existente complete su recompilación normal y revisar la consola y una recarga normal de escena. Si reaparece la advertencia, capturar contexto y objeto causal antes de intervenir. Gameplay, fidelidad visual, ejecutable PC y PS Vita física no probados en esta operación.

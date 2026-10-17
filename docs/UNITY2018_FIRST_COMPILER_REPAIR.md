# Primera reparación puntual Unity 2018.2 — 2026-10-16 UTC

Encargo explícito del propietario del 15 de octubre (hora de Colombia): reparar exclusivamente los seis CS1525 de la copia independiente ya migrada. Esta autorización puntual no aprueba P00–P12, no modifica el ledger y no acredita paridad de gameplay. El propietario informa gameplay, gráficos, audio y guardado confirmados en Unity 5.6; no se repitieron esas pruebas en esta intervención.

## Diagnóstico y modificación

**VERIFIED:** el ProjectVersion de la copia y el log del editor identifican Unity 2018.2.19f1 (`06990f28ba00`). La línea de inicio del log apunta a la copia de migración, distinta de la copia 5.6. Las tres expresiones afectadas coinciden con las de la copia 5.6 examinada en modo lectura.

**VERIFIED:** Mono C# compiler 5.11.0.0, incluido en este editor, rechaza las tres llamadas a `ToAngleAxis` sobre una multiplicación parentizada de cuaterniones. La reproducción con los tres respaldos y el response file real produce exactamente seis CS1525. Es una incompatibilidad del análisis sintáctico de estas expresiones con este compilador; no se encontró una característica moderna de C# que exija actualizar el runtime. **INFERRED:** la ambigüedad de análisis de identificadores y `*` explica que el parser espere una coma y rechace el punto; no se instrumentó internamente el parser.

Se cambió una sentencia en cada archivo: `AIParagliderBoat.cs:152`, `MovementControl.cs:660` y `Util.cs:488`. Primero se asigna el mismo producto a un `Quaternion` local llamado `rotationDeltaForAngleAxis`, después se invoca su `ToAngleAxis` con los mismos argumentos `out`. Ambos operandos se evalúan una vez y en el mismo orden; se conservan el producto, la llamada y todos sus consumidores. No cambian firmas, campos serializados, tipos públicos ni operaciones de gameplay. Los `.meta` conservan sus hashes. Los hashes de las tres fuentes 5.6 son iguales antes y después; ninguna operación de escritura se dirigió a ese proyecto.

**VERIFIED (configuración efectiva):** el response file del editor contiene `-langversion:4`, `NET_2_0_SUBSET` y referencias `MonoBleedingEdge/lib/mono/unity`, correspondientes al perfil heredado .NET 3.5 / API .NET 2.0 Subset. `ProjectSettings.asset` contiene `apiCompatibilityLevel: 1` y no serializa explícitamente `scriptingRuntimeVersion`. **UNTESTED:** lectura directa del desplegable o consulta nativa de `PlayerSettings.scriptingRuntimeVersion`. No se modificaron ajustes. [Referencia Unity 2018.2 sobre perfiles](https://docs.unity.cn/2018.2/Documentation/Manual/dotnetProfileSupport.html).

## Comprobación posterior

**PASS para los seis CS1525; FAIL para compilación completa.** Se ejecutó el compilador del editor con el response file real de Assembly-CSharp, cambiando únicamente el destino de salida para conservar el resultado en el informe privado. La compilación posterior no emite CS1525 y termina con **6 errores CS0619 y 165 warnings**:

| Fuente | Diagnóstico nuevo |
| --- | --- |
| `MemDump.cs:85` | `TextureFormat.ATC_RGB4` obsoleto con error |
| `MemDump.cs:87` | `TextureFormat.ATC_RGBA8` obsoleto con error |
| `NGUIMath.cs:160,171` | `RuntimePlatform.WindowsWebPlayer` eliminado |
| `UIAnchor.cs:60` | `RuntimePlatform.WindowsWebPlayer` eliminado |
| `UIPanel.cs:639` | `RuntimePlatform.WindowsWebPlayer` eliminado |

No se corrigieron estas APIs en este encargo de sintaxis. No se inició otro editor sobre el proyecto abierto, no hubo build PC completo, prueba de gameplay, PSP2, VPK ni PS Vita física.

Respaldos, hashes SHA-256 individuales, response files y logs propietarios están exclusivamente en `MigrationDiagnostics/CS1525_20261016T043759Z/` dentro de la copia privada Unity 2018. `receipt.json` identifica los tres inputs y outputs. `before-corrected-compile.log` reproduce los seis errores originales; `compile.log` contiene los diagnósticos posteriores. El primer intento auxiliar de reproducción falló por rutas sin comillas (CS2001); se conserva como `before-compile.log`. Se corrigieron las rutas del probe sin cambiar fuentes para obtener la reproducción válida.

## ReflectionProbeAnchorManager

**VERIFIED:** el log anterior a la reparación contiene ambos mensajes nativos al descargar 829 archivos serializados durante la importación/actualización de assets. Proceden de `Runtime/Transform/Transform.cpp:1704–1705`, sin objeto/GUID causal identificado. No son diagnósticos del compilador C#.

**UNTESTED:** persistencia después de una compilación completa satisfactoria; esta sigue bloqueada por CS0619. No se atribuyen a los tres scripts ni se declaran resueltos. Si se reproducen tras resolver la compilación, registrar contexto y objeto afectado antes de modificar probes, iluminación, escenas o materiales.

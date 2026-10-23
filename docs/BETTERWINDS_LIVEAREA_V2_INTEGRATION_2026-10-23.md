# BetterWinds LiveArea v2 — integración privada (2026-10-23 UTC)

El propietario autoriza expresamente esta actualización de presentación, su build
PSP2 y VPK privado en `main`. Excepción específica de alcance; P03 continúa ACTIVE,
P04–P12 LOCKED. No se modifican fases. Crédito de diseño:
[BetterWinds](https://github.com/BetterWinds). Las imágenes y los binarios permanecen
privados; no se presupone permiso de redistribución del arte del juego.

## Entradas y respaldo

**VERIFIED:** ZIP preparado SHA-256
`2c907da6bdb6b73d2bb3ad41e6d9cb55fc66917d95ea596b54c57e89cd9dc22a`.
Los originales coinciden con el [inventario v2](BETTERWINDS_LIVEAREA_V2_2026-10-21.md).
Icono/startup coinciden con v1 y sus fuentes instaladas no se reemplazaron.
Los documentos y scripts del ZIP no se ejecutaron ni se trataron como instrucciones.

Respaldo privado: `Reports/BetterWindsV2_20261023/Before`, originales bajo
`originals/`, manifiesto de 19.676 archivos y hashes de 19 VPK anteriores.
El instalador guarda v2 en una carpeta ignorada separada y conserva v1.

## Configuración nativa y compatibilidad

**VERIFIED:** Unity 2018.2.19f1 / PSP2 / Mono2x, 18 escenas existentes,
`BuildOptions.None`. Se reutiliza la integración nativa:

| Entrada privada | Ajuste/salida |
| --- | --- |
| `PSP2System/BetterWinds/contents` | `PlayerSettings.PSVita.liveAreaPath`; background/gate independientes vacíos |
| `Assets/Editor/BetterWinds/icon0.png` | Mismo icono PSP2 de 128 px |
| `Assets/Editor/BetterWinds/pic0.png` | Campo nativo `psp2Splashimage`, utilizado por la interfaz PSP2 instalada |
| `PSP2System/BetterWinds/manual/001.png` | `PlayerSettings.PSVita.manualPath = PSP2System/BetterWinds/manual` |

Solo cambian los campos manual y splash de `ProjectSettings.asset`. La extensión
PSP2 instalada copia las páginas PNG a `sce_sys/manual`, e incluye el directorio
al empaquetar. La documentación local `PSVitaProjectSettings.html` describe el
manual como una serie de PNG; no se añadió XML del manual ni un bit SFO supuesto.
La [API de Unity](https://docs.unity.cn/540/Documentation/ScriptReference/PlayerSettings.PSVita-manualPath.html)
expone esa configuración. **UNTESTED:** aparición/apertura real del manual en Vita.
La [guía de Sony](https://manuals.playstation.net/document/en/psvita/basic/screens.html)
identifica el botón de ayuda/manual de la aplicación en LiveArea.

**VERIFIED incompatibilidad y corrección:** Publishing Tools 2.31,
`psp2pubcmd file_verify --iformat psp2_larea_xml template.xml`, rechaza
`psmobile` con `Invalid style (psmobile)` y advierte sobre LF. El derivado final
usa el estilo anterior `a1`, corrige `bg.png` a `bg0.png` y usa CRLF:
SHA-256 `24a187fa56fb63e0ba68530073e8df84544308ef7c9756687cbd31e36c1edb51`.
El mismo validador devuelve cero errores/advertencias para ese XML. `a1` coloca
el gate centrado: comprobar la composición con el nuevo fondo en hardware.
El [autor de livearea-specs](https://github.com/Hammerill/livearea-specs) describe
`psmobile` en otros entornos; eso no invalida el rechazo del toolchain instalado.
Original y XML psmobile construido se conservan para comparación.

**VERIFIED:** se conserva el fondo original indexado de 180.020 bytes, sin pérdida
ni uso de la copia cuantizada de 103.628 bytes. Unity lo copia intacto. No se
estableció una restricción de 128 KiB aplicable a este player; el verificador XML
no demuestra restricciones de decodificación PNG en la consola. **UNTESTED:**
aceptación y calidad del fondo en el firmware físico.

**VERIFIED:** manual byte-idéntico. PIC0 se importa a 960×544, sin mipmaps ni
compresión de textura, y el exportador nativo vuelve a cuantizar la paleta:
206.161 bytes, SHA-256 `b8a350dfb61640bc018a5a47ffd1bbc4767338befccadfd1fa478defceea378c`.
Inspección visual conserva composición; 518.929 píxeles difieren, máximos RGB
13/11/17, alfa idéntico. No se confunde igualdad visual aproximada con igualdad
de bytes. El verificador acepta solo este derivado revisado o el original.

## Build y comparación estricta

**VERIFIED build:** Succeeded, 0 errores / 295 advertencias, 3:38.532, 18 escenas.
No se ejecutó Play Mode. Las advertencias heredadas se conservan; los mensajes
Hidden/FrameDebugger del dispositivo gráfico nulo no prueban un fallo PSP2.
PIC0, manual, fondo e inicio salen por el mecanismo nativo, sin postprocesador
permanente de arte. El XML a1 se aplicó después de compilar al origen y a la copia
nativa: la extensión solo copia este XML, por lo que no se repitió el build de
código para un archivo declarativo. Los builds futuros lo copian desde el origen.

El primer candidato de build se conserva por separado. El verificador estricto
**lo rechazó** por cinco diferencias adicionales respecto al último VPK de
iluminación: `MonoAssembliesPSP2.suprx`, `globalgamemanagers` y
`sharedassets4/14/15.assets`. Tamaños iguales; inspección AssetStudio identifica
BuildSettings/RuntimeInitializeOnLoadManager y tres objetos Mesh. No se demuestra
que esas diferencias sean inofensivas y no se amplía la allowlist para aceptarlas.
Ninguna fuente de gameplay ni escena/shader/material del proyecto cambió.

Para aislar esta actualización se genera el VPK final con el runtime exacto del
VPK de iluminación ya verificado y los recursos de presentación del nuevo build.
Se reutiliza la convención ZIP privada existente, sin modificar ejecutable,
contenido del juego o SFO. No se añade un segundo sistema permanente de build.
El arte nuevo y ajustes nativos quedan instalados para posteriores builds Unity.
El resultado del build y el resultado del paquete final se documentan por separado.

## Prueba física y reversión

**UNTESTED hardware:** no instalación, manual, apariencia, lanzamiento/reanudación,
partida ni persistencia en consola ejecutados por Codex. Probar en HGSKE1178:

1. Conservar respaldo del progreso y el VPK anterior; actualizar la aplicación existente.
2. Revisar burbuja, fondo y gate centrado, recortes/colores y botón Inicio.
3. Abrir Ayuda/manual y verificar la página 001; regresar y lanzar/reanudar.
4. Verificar PIC0 durante arranque, menú, audio y una partida breve; cerrar y reabrir conservando progreso.

Reversión privada: restaurar solo manual/splash anteriores desde el respaldo de
settings, y archivos BetterWinds cambiados; preservar cualquier edición posterior.
No reemplazar settings completos si han cambiado otros campos. Los VPK anteriores
no se sobrescriben. Unity 5.6 no se abrió ni modificó.

## Resultado final verificado

**VERIFIED:** `Build/HSE_BetterWinds_v2_PresentationOnly_HGSKE1178.vpk`,
136.185.607 bytes, 214 entradas, SHA-256
`3e3dffc4a175f8e11ca769c1f0cec69d5cb79b1095fcbccf312f9a4c58df9db3`.
CRC ZIP, decodificación de todas las imágenes PNG, rutas únicas/seguras,
referencias/hash XML e identidad HGSKE1178 pasan. Solo difieren los cuatro
miembros de presentación: fondo, XML, PIC0 y nueva página de manual. Todo el
runtime y `param.sfo` son byte-idénticos al VPK de iluminación SHA-256
`488a66e4e0682f6aaa00b573277e101cb10c6f6c142003b3b7543072e33d36cc`.
Sin savedata; los 19 VPK previos conservan sus hashes.

**VERIFIED integridad de proyecto:** 19.676 archivos originales examinados,
ninguno desaparecido. Cuatro archivos cambian: helper Editor, ProjectSettings,
fondo y XML. Se añaden PIC0+meta y página manual. Código de gameplay, shaders,
escenas, controles, guardado y rendimiento no cambian. El driver temporal fue
archivado fuera de Assets; el helper reusable queda instalado. Sin cambios a Unity 5.6.

**VERIFIED herramientas:** `--version v1` mantiene las comprobaciones históricas;
`--version v2` revisa PIC0/manual/XML/hashes y rechaza cambios fuera de presentación.
Instalador acepta v1 y ambos formatos de entrada v2; rechaza perfil equivocado.
Genera `native-template.xml` separado, conservando el XML original. Ocho controles
negativos del verificador pasan: manual ausente, PIC0 viejo, XML erróneo, runtime
alterado/eliminado, traversal, colisión de mayúsculas y PNG dañado. Validación real
del VPK v1 anterior también pasa. Sin errores C# ni shader en el nuevo build.

[Recibo público sanitizado](evidence/BETTERWINDS_LIVEAREA_V2_2026-10-23.json).
Evidencia detallada privada: Build.log/BuildResult, Configuration, BeforeManifest,
ProjectSettings.diff, PackageDiff, NativePNG, PackageReceipt, Integrity y
AdditionalChecks bajo `Reports/BetterWindsV2_20261023`.

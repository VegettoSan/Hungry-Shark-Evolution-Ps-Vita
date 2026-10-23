# Checklist de publicación v1.0 — Hungry Shark Evolution Vita

**Estado: BLOQUEADO PARA SUBIR BINARIOS.** Hay un candidato ZIP privado validado, pero falta resolver hallazgos de privacidad y revisar permisos y avisos. Esta lista no sustituye el [plan y las fases formales](PORTING_PLAN.md); P03 permanece ACTIVE y P12 LOCKED. Alcance autorizado ahora: preparación de documentación de lanzamiento en `main`, sin crear ramas, tags ni Releases.

## Impedimentos que deben cerrarse antes de subir un ZIP

- [ ] **Privacidad:** eliminar de manera reproducible las **dos cadenas de rutas absolutas privadas** registradas en `Media/Modules/MonoAssembliesPSP2.suprx`, o registrar una decisión explícita y consciente del propietario de divulgarlas. No copiar valores de las rutas a Issues, logs, documentación ni adjuntos públicos. Toda alteración del módulo requiere recompilar o reemplazar con una referencia autorizada y **repetir la comparación y la matriz de Vita**: los hashes actuales dejarán de representar el binario nuevo.
- [ ] **Distribución y licencias:** documentar el alcance verificable de la autorización para redistribuir la base PSP2 (incluidos `eboot.bin`, `Media/Modules/`, `sce_module/`, ensamblados, LiveArea) y los **literales del parche xDelta**, además de las obligaciones de licencia de terceros. La declaración del propietario de tener documentación está registrada, pero los documentos no fueron auditados de manera independiente. Una delta pequeña no equivale a contenido libre de derechos.
- [ ] **Avisos de terceros:** comprobar y completar los avisos exigibles a Unity/Mono/Sony y librerías integradas, cuando apliquen. Mantener licencia xdelta3 Apache-2.0 y atribución a BetterWinds.
- [ ] **Paquete libre de secretos:** revisar el ZIP exacto y sus miembros en busca de rutas locales, tokens, claves o recursos restringidos. El informe solo descarta patrones concretos; no demuestra ausencia general.

## Verificación técnica final

- [x] APK de referencia comprobado con tamaño 104296014 bytes y SHA-256 `52b7d8754ab8fec75c3b656a0dc5ec8dc849565adc402487a3ac093f6a481f7a` (comprobado también contra el APK aportado en esta conversación).
- [x] Según los recibos, extractor nativo reconstruyó los **214 miembros internos** byte por byte a partir del APK y se validaron ZIP CRC, hashes y salida VPK; ver [recibo](evidence/RELEASE_PACKAGE_2026-10-23.json).
- [x] Según testimonio del propietario, la matriz solicitada de instalación, gameplay, audio, menú, guardado, reinicio y reinstalación pasó en PS Vita. **OWNER-REPORTED**, no ejecutada por este asistente.
- [ ] Si se modifica `HSEVita_Stub.vpk`, `HSEVita_Data.xdelta`, manifiesto o EXE, **recrear** el paquete y repetir la reconstrucción, SHA-256 del VPK, CRC y comparación de todos los archivos. Repetir las pruebas relevantes en Vita para todo cambio binario.
- [ ] Probar manualmente en Windows **arrastrar APK al EXE** y **selector de archivo**; los tests automáticos no los sustituyen.
- [ ] Probar en un PC Windows 10 x64 limpio con .NET Framework 4.8, espacios y rutas Unicode, sin SDK/Python y con los 2 GB libres indicados.
- [ ] Probar error por APK incorrecto y cancelación; verificar que no queda VPK parcial, y que la salida existente no se sobrescribe.
- [ ] Confirmar que el ZIP nuevo contiene `HSERebuild.exe`, `xdelta3decode.exe`, `HSEVita_Stub.vpk`, `HSEVita_Data.xdelta`, `HSEVita_Release.json`, `README_ES_EN.md`, `SHA256SUMS.txt`, `xdelta3-LICENSE.txt` y avisos aplicables.
- [ ] Registrar SHA-256, tamaño y contenidos del **ZIP realmente subido**, no confundir el anterior `HSEVita_v1.0_Windows.zip` con `HSEVita_v1.0_Windows_README.zip`. Ambos son candidatos **locales** y sus hashes difieren.
- [ ] Validar contra el ZIP final los checksums publicados y el hash resultante del VPK (el candidato documenta `03f752c91d936765c20d9b7494deae319486763f4bf4f4005dbff45353fe2f9b`; cambiar si el nuevo VPK difiere).

## Pasos de GitHub al cerrar los impedimentos

- [ ] Confirmar que los archivos autorizados no incluyen APK, VPK reconstruido completo ni datos originales aportados separadamente por usuarios.
- [ ] Actualizar el estado del README y usar las [notas v1.0](RELEASE_V1_0_GITHUB_NOTES.md) revisadas; no anunciar resultados de FPS sin medición.
- [ ] Publicar con tag `v1.0` **solo después de validar todo lo anterior**. Adjuntar exclusivamente los assets autorizados y el `SHA256SUMS.txt` correspondiente.
- [ ] Descargar de nuevo el ZIP desde la Release pública y repetir el cálculo de SHA-256; contrastar con el recibo antes de anunciar la URL.
- [ ] Mantener `docs/PHASE_GATES.json` intacto salvo que exista una aprobación formal independiente para cambiar sus fases.

**Referencia:** [preparación y hallazgos](RELEASE_V1_0_PREPARATION_2026-10-23.md), [recibo local](evidence/RELEASE_PACKAGE_2026-10-23.json), [guía ES/EN](RELEASE_V1_0_README_ES_EN.md). Este checklist documenta trabajo pendiente y **no autoriza por sí mismo la publicación**.

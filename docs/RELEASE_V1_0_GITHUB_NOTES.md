# Hungry Shark Evolution Vita v1.0 — Release notes (borrador / draft)

> **NO PUBLICAR BINARIOS TODAVÍA.** Este archivo prepara el texto de la Release, pero los artefactos privados aún no están autorizados para subirse. Consultar la [lista de comprobación](RELEASE_V1_0_PUBLICATION_CHECKLIST.md) antes de crear o publicar la Release. No existe descarga pública confirmada al 23 de octubre de 2026.

## Español

¡Hungry Shark Evolution llega a PS Vita! La versión 1.0 es un port nativo creado a partir de la versión original de Android **6.4.2 (build 228)**, usando Unity con soporte PSP2.

**Incluye:** gameplay, menús y evolución; tiburones especiales; controles adaptados a PS Vita y entrada táctil; audio y guardado/carga. La matriz de instalación y funcionamiento fue **confirmada por el propietario en una PS Vita real**. No se publica una cifra de FPS estables ni una garantía de compatibilidad en todas las consolas.

**Requisitos de reconstrucción del VPK:** PC con Windows de 64 bits, .NET Framework 4.8, 2 GB libres y tu propio APK original de **Hungry Shark Evolution 6.4.2 (228)**, de **104296014 bytes** y SHA-256:

```text
52b7d8754ab8fec75c3b656a0dc5ec8dc849565adc402487a3ac093f6a481f7a
```

Cuando se autorice la distribución, descarga el ZIP de la Release, descomprímelo por completo y arrastra el APK a `HSERebuild.exe` (o selecciónalo desde el programa). Espera a **PASS**, pasa el VPK **completo reconstruido** a la Vita e instálalo con VitaShell. **Nunca instales `HSEVita_Stub.vpk` por separado.** Antes de actualizar, respalda los datos de guardado.

**Title ID:** `HGSKE1178`. **Salida normal del extractor:** `HungrySharkEvolutionVita_1.0.vpk`.

**Controles:** stick izquierdo/cruceta para movimiento y menús; **R** turbo; **X** confirmar; **O** volver; **START** pausa; **cuadrado** misiones; **triángulo** mapa; controles táctiles disponibles. Consulta la [guía completa ES/EN](RELEASE_V1_0_README_ES_EN.md).

**Limitaciones y comprobaciones pendientes:** el rendimiento puede variar según la escena y la configuración de Vita; no se publican cifras de rendimiento no medidas. La selección manual del APK/arrastrar y soltar y la compatibilidad en un Windows 10 limpio no tienen una prueba manual documentada. La reconstrucción y comprobación de 214 entradas del VPK sí fueron verificadas por herramientas en Windows. El juego original y sus recursos no vienen en forma de APK ni de VPK completo.

**Créditos:** port por **VegettoSan**; arte de LiveArea por [BetterWinds](https://github.com/BetterWinds). *Hungry Shark Evolution* pertenece a sus respectivos titulares; proyecto comunitario no afiliado con Ubisoft. xdelta3 3.2.2 por Joshua MacDonald, licencia Apache-2.0; conservar los avisos de terceros.

## English

**Hungry Shark Evolution Vita v1.0** is a native PS Vita port based on the original Android **6.4.2 (build 228)** release. It includes core gameplay, evolution menus, special sharks, Vita controls/touch input, audio and saving/loading. The owner reports PASS for the requested hardware test matrix; stable FPS and all-device compatibility have not been certified.

After distribution clearance, use the supplied Windows x64/.NET Framework 4.8 reconstruction utility with **your own unmodified 6.4.2 (228) APK** (104296014 bytes; exact SHA-256 above). Extract the whole release ZIP, drag the APK onto `HSERebuild.exe`, wait for **PASS**, and install only the **complete reconstructed VPK** using VitaShell. Never install `HSEVita_Stub.vpk` alone. Back up saves before updating. Title ID: `HGSKE1178`.

No original APK or fully assembled VPK is distributed. A clean Windows 10 manual test and manual drag-and-drop/file picker testing remain unrecorded; performance varies by scene. See the [bilingual user guide](RELEASE_V1_0_README_ES_EN.md) and [technical validation](RELEASE_V1_0_PREPARATION_2026-10-23.md).

**Credits:** VegettoSan (port), [BetterWinds](https://github.com/BetterWinds) (LiveArea art), Joshua MacDonald (xdelta3). All original game rights remain with their respective owners. Not affiliated with Ubisoft.

---

**Nota para quien publica / Maintainer note:** No adjuntar este texto como prueba de autorización. Antes de liberar, solucionar privacidad, licencias y permisos; volver a medir SHA-256 del ZIP **exacto** que se cargará; validar el paquete final (no reutilizar hashes antiguos si se vuelve a empaquetar). La revisión local más reciente registrada es `HSEVita_v1.0_Windows_README.zip`, SHA-256 `318198f01361ec1df01fdc27618638d88bb365e34a0c51e68263d8b7bdf4b30d`; **no afirmar que ese archivo se ha subido**.

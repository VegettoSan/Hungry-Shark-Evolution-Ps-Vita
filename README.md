# Hungry Shark Evolution — PS Vita Port

Community research and development repository for a **native PlayStation Vita port** of *Hungry Shark Evolution*, preserving original gameplay logic and Unity assets wherever technically possible.

> **Estado (2026-10-23):** **OWNER-REPORTED PASS:** el propietario validó el VPK reconstruido en PS Vita, incluyendo gameplay, audio, menús, evolución, tiburones especiales, pausa, guardado/carga, reinicio en frío, actualización y reinstalación. **VERIFIED:** el extractor Windows reconstruye los 214 archivos internos idénticos byte por byte al VPK de referencia. El paquete v1.0 está preparado localmente; aún no se ha publicado una Release en GitHub. Véanse [preparación y límites](docs/RELEASE_V1_0_PREPARATION_2026-10-23.md) y [recibo del paquete](docs/evidence/RELEASE_PACKAGE_2026-10-23.json). Las aprobaciones formales siguen **P00–P02 APPROVED; P03 ACTIVE; P04–P12 LOCKED**; la validación del paquete no modifica el [ledger](docs/PHASE_GATES.json).

## Uso del paquete v1.0 / Getting started

**Necesitas:** Windows de 64 bits con .NET Framework 4.8, al menos **2 GB libres**
y tu propio APK de **Hungry Shark Evolution 6.4.2, build 228**. No necesitas
instalar Python, Unity ni un SDK para reconstruir el VPK.

El APK exacto tiene **104296014 bytes** y SHA-256:

```text
52b7d8754ab8fec75c3b656a0dc5ec8dc849565adc402487a3ac093f6a481f7a
```

1. Usa `HSEVita_v1.0_Windows*.zip` proporcionado por el propietario. Cuando se
   publique, estará en [Releases](https://github.com/VegettoSan/Hungry-Shark-Evolution-Ps-Vita/releases).
   Extrae **todo el ZIP** a una carpeta local escribible.
2. Mantén juntos `HSERebuild.exe`, `xdelta3decode.exe`, `HSEVita_Stub.vpk`,
   `HSEVita_Data.xdelta` y `HSEVita_Release.json`.
3. Arrastra tu APK sobre `HSERebuild.exe`, o abre el EXE y selecciona el APK.
   Espera al mensaje **PASS**. El VPK se genera junto al APK con el nombre
   `HungrySharkEvolutionVita_1.0.vpk`.
4. Respalda tu progreso antes de actualizar. Transfiere **el VPK completo
   reconstruido** a la Vita e instálalo con VitaShell. Title ID: **`HGSKE1178`**;
   ubicación estándar de la app: `ux0:app/HGSKE1178/`.

**No instales `HSEVita_Stub.vpk`: es una base incompleta.** El paquete no incluye
el APK ni el VPK completo; el extractor necesita el APK exacto del usuario.
No copies los datos Android por encima del runtime PSP2.

Para elegir otra ubicación de salida, abre PowerShell en la carpeta del paquete:

```powershell
.\HSERebuild.exe "C:\MisJuegos\HSE_6.4.2.apk" --out "C:\Salida\HSEVita_1.0.vpk"
```

El extractor comprueba los hashes y **no sobrescribe archivos existentes**. Si
el destino existe, elige otro nombre con `--out`. El VPK generado por el EXE
incluido tiene SHA-256:

```text
03f752c91d936765c20d9b7494deae319486763f4bf4f4005dbff45353fe2f9b
```

Si el APK es rechazado, comprueba su tamaño y SHA-256. Si faltan componentes o
falla un checksum, vuelve a extraer el ZIP completo. Para comprobar un archivo:
`Get-FileHash -Algorithm SHA256`, comparando con `SHA256SUMS.txt` del paquete.
Consulta la [guía completa en español e inglés](docs/RELEASE_V1_0_README_ES_EN.md)
para instalación, uso, cancelación y solución de problemas.

**English:** extract the complete package, launch `HSERebuild.exe` or drop your
exact 6.4.2 (228) APK onto it, and wait for **PASS**. Install only the complete
reconstructed VPK with VitaShell. Never install the incomplete stub by itself.
See the [English instructions](docs/RELEASE_V1_0_README_ES_EN.md#english).

## Controles de PS Vita / PS Vita controls

| Control | Español | English |
| --- | --- | --- |
| Stick izquierdo / Left stick | Mover el tiburón; navegar por menús | Move the shark; navigate menus |
| Cruceta / D-pad | Movimiento alternativo; navegar y cambiar selección | Alternative movement; navigate and change selection |
| R, mantenido / Hold R | Turbo durante la partida | Boost during gameplay |
| START | Pausar/reanudar | Pause/resume |
| ✕ / Cross | Confirmar o activar la opción seleccionada | Confirm or activate the selected option |
| ○ / Circle | Volver/cancelar | Back/cancel |
| □ / Square | Abrir/cerrar misiones durante la partida | Toggle missions during gameplay |
| △ / Triangle | Abrir/cerrar mapa durante la partida | Toggle map during gameplay |
| Pantalla táctil / Touchscreen | Controles táctiles originales y botones en pantalla | Original touch controls and on-screen buttons |

Misiones y mapa están sujetos a su disponibilidad en el estado actual del juego.
El turbo consume la energía original: suelta R para dejar de solicitarlo. La
pantalla táctil conserva prioridad mientras hay un toque activo.

**Evidencia:** configuración **VERIFIED** en el perfil PSP2, Input Manager y
consumidores de entrada del proyecto; funcionamiento general en Vita
**OWNER-REPORTED PASS**. Esta actualización de documentación no añade una nueva
prueba física botón por botón ni modifica los controles. Contexto histórico en
[la integración del mando](docs/VITA_CONTROLLER_INTEGRATION_2026-10-16.md).

## Reference build

| Item | Audited result |
| --- | --- |
| Game | Hungry Shark Evolution **6.4.2**, Android versionCode **228** |
| Original Unity player | **5.6.4p3** (official editor released November 20, 2017) |
| Backend | **Mono**, not IL2CPP |
| Main assembly | `Assembly-CSharp.dll` (4,958,208 bytes) |
| Other assemblies | `Assembly-CSharp-firstpass.dll`, `Assembly-UnityScript.dll`, `Assembly-UnityScript-firstpass.dll` |
| Serialized scenes | `level0` through `level17` (18 files; not necessarily 18 gameplay levels) |
| Android architecture | `armeabi-v7a` |
| Candidate Vita editor | **Unity 2018.2.19f1 PSP2 / Mono2x — native HSE VPK builds and owner-reported hardware tests documented** |

The newer 14.5.0 package uses Unity 6 / IL2CPP and is not our initial porting baseline.

## Vita identity and game data layout

**Official port name: Hungry Shark Evolution Vita** · **Title ID: `HGSKE1178`**. The intended application directory is `ux0:app/HGSKE1178/`, subject to actual PSP2 packaging tests. The APK's original Unity `Data/` is **developer recovery input**, NOT a verified copy-and-play PSP2 asset pack. Read [APK extraction and Vita data/app/save layout](docs/DATA_EXTRACTION_AND_LAYOUT.md) and [canonical identity](config/port_identity.json). The Vita identity is used by privately tested Unity 2018 PSP2 VPKs; the **formal phase gates for later work are not yet approved**. See the 2026-10-17 retrospective audit.

## NON-NEGOTIABLE development order

**6.4.2 APK → recover Unity 5.6.4p3 project → get original gameplay working in Unity 5.6.4p3 on PC → explicitly approve the original-engine baseline → migrate a separate copy to Unity 2018.2 → prove gameplay again on PC → verify PSP2 toolchain on actual Vita → port and test actual game on Vita.**

This is the **formal acceptance order**. Private Unity 2018/PSP2 experiments already happened under direct owner requests; they are documented without inventing retroactive full gate passes. Agents must not perform new phase-skipping work without a fresh, scoped owner authorization and **must not create branches without prior authorization**. Formal phase approvals are maintained in [`docs/PHASE_GATES.json`](docs/PHASE_GATES.json), validated by a [CI checker](tools/check_phase_gates.py). Each phase needs evidence **and an explicit owner approval**; automated validation cannot substitute for hardware testing or human review.

**Read the [full start-to-finish gated plan](docs/PORTING_PLAN.md) before doing any work.**

## Repository index

- [Detailed sequential project plan](docs/PORTING_PLAN.md)
- [Current phase approval ledger](docs/PHASE_GATES.json)
- **[Retrospective audit of P00–P12 and physical Vita evidence (2026-10-17)](docs/PHASE_AUDIT_2026-10-17.md)**
- **[Historical Unity 5.6 recovery report from October 15](docs/RECOVERY_STATUS_2026-10-15.md)**
- **[Unity 5.6 confirmed systems, freeze checklist and Unity 2018 handoff](docs/UNITY56_BASELINE_AND_UNITY2018_HANDOFF.md)**
- [Codex task: stage private Unity 5.6 baseline and evidence (no migration yet)](docs/CODEX_FREEZE_UNITY56_PREP.md)
- [Codex future P05/P06 migration instructions (locked until P04 approval)](docs/CODEX_UNITY2018_MIGRATION_PREP.md)
- [Rules for Codex, AI agents and contributors](AGENTS.md)
- [Halo 2D PSP2 evidence / Unity 2018.2.19f1](docs/HALO2D_PSP2_REFERENCE.md)
- [PS Vita shader research, visual parity and 30/60 FPS targets](docs/SHADERS_PSVITA_REFERENCE.md)
- [Developer vs. user APK extraction and Vita app data storage](docs/DATA_EXTRACTION_AND_LAYOUT.md)
- [xdelta3 research: Predators, Pocket Mortys and future HSE delivery](docs/XDELTA_UNITY_PORTS_RESEARCH.md)
- [Set up ignored local Tool Codex and official download links](docs/TOOL_CODEX_LOCAL_SETUP.md)
- [Codex unattended local first-run task (P00 → approved P01)](docs/CODEX_LOCAL_RECOVERY_TASK.md)
- **[Codex offline Unity 5.6 project staging (scoped owner authorization; no Unity installed)](docs/CODEX_UNITY56_OFFLINE_EXPORT_PROMPT.md)**
- [Technical audit and reproducible APK identity](docs/TECHNICAL_AUDIT.md)
- [Experiment history: PASS / FAIL / NOT TESTED](docs/EXPERIMENTS.md)
- **[Complete toolchain: current downloads, dependencies and install order](docs/TOOLS.md)**

## Local source preparation

Requires Python 3 and a **user-supplied** copy of the original APK.

Windows: drag the APK onto [`tools/ARRASTRAR_HUNGRY_SHARK_6.4.2.bat`](tools/ARRASTRAR_HUNGRY_SHARK_6.4.2.bat).

Cross-platform:
```shell
python tools/Preparar_Hungry_Shark_Evolution_6.4.2.py path/to/game.apk local-work/HSE642
python tools/check_phase_gates.py
```

This prepares split Unity `Data/` files, including `Data/Managed/`, **not** a complete Unity editor project. AssetRipper and script recovery are separate steps.

## Legal and reproducibility notes

*Hungry Shark Evolution* and original game code/assets belong to their rights holders. **Do not distribute** APK/XAPK/OBB, decompiled code, extracted game assets, Sony/Unity restricted SDKs, credentials or unauthorized VPKs. Work from individually obtained lawful inputs; the repository contains only public tools, technical reports, rules and scripts. Not affiliated with Ubisoft or the game's developers.

Individual source/editor test results are documented with their limits. Full runtime, parity and performance claims require the corresponding platform tests and gate evidence; a successful editor probe does not establish standalone PC or Vita gameplay.

## LiveArea artwork credits

The PS Vita LiveArea graphics and bubble icon for this port were contributed by **[BetterWinds](https://github.com/BetterWinds)**. Thank you for the artwork. This acknowledgement covers the LiveArea contribution, not ownership of *Hungry Shark Evolution* or its underlying characters, logos and artwork, which remain with their respective rights holders.

See the [LiveArea asset manifest](assets/livearea/BetterWinds/README.md) and [Codex integration instructions](docs/CODEX_BETTERWINDS_LIVEAREA.md). The owner-provided binary source package is handled separately from the public documentation and is not included in this commit.

**BetterWinds LiveArea v2:** los nuevos `pic0.png`, manual y presentación se integraron en el VPK nativo; consulta el [informe de integración del 2026-10-23](docs/BETTERWINDS_LIVEAREA_V2_INTEGRATION_2026-10-23.md) y la [validación del paquete](docs/RELEASE_V1_0_PREPARATION_2026-10-23.md). El inventario original permanece en [la entrega v2](docs/BETTERWINDS_LIVEAREA_V2_2026-10-21.md).

# Hungry Shark Evolution — PS Vita Port

Community research and development repository for a **native PlayStation Vita port** of *Hungry Shark Evolution*, preserving original gameplay logic and Unity assets wherever technically possible.

> **Status (2026-10-17): P00–P02 APPROVED; P03 ACTIVE; P04–P12 LOCKED** tras [auditoría retrospectiva](docs/PHASE_AUDIT_2026-10-17.md) y [autorización registrada](https://github.com/VegettoSan/Hungry-Shark-Evolution-Ps-Vita/issues/2). Unity 5.6.4p3 funciona en Editor según informes del propietario; la copia Unity 2018.2.19f1 compila y existen **VPK nativos PSP2 probados en una PS Vita real**. D05 llegó a gameplay con sonido (40–58 FPS reportados); D06 experimentó caídas a ~11 FPS. **El guardado persistente en Vita sigue fallando o reportándose corrupto** y no se han completado todas las pruebas de controles, muerte/reintento, paridad, estabilidad o release. Los logros técnicos de P05–P11 no equivalen a aprobar esas fases formalmente.

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

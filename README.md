# Hungry Shark Evolution — PS Vita Port

Community research and development repository for a **native PlayStation Vita port** of *Hungry Shark Evolution*, preserving original gameplay logic and Unity assets wherever technically possible.

> **Status (2026-10-14):** static APK audit complete; **P00 (original editor/environment validation) ACTIVE**. No Unity editor reconstruction, PC gameplay or Vita VPK has yet been proven.

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
| Candidate Vita editor | **Unity 2018.2 + an independently verified PSP2 build target** |

The newer 14.5.0 package uses Unity 6 / IL2CPP and is not our initial porting baseline.

## NON-NEGOTIABLE development order

**6.4.2 APK → recover Unity 5.6.4p3 project → get original gameplay working in Unity 5.6.4p3 on PC → explicitly approve the original-engine baseline → migrate a separate copy to Unity 2018.2 → prove gameplay again on PC → verify PSP2 toolchain on actual Vita → port and test actual game on Vita.**

Agents **MUST NOT** jump ahead. Formal phase approvals are maintained in [`docs/PHASE_GATES.json`](docs/PHASE_GATES.json), validated by a [CI checker](tools/check_phase_gates.py). Each phase needs evidence **and an explicit owner approval**; automated validation cannot substitute for hardware testing or human review.

**Read the [full start-to-finish gated plan](docs/PORTING_PLAN.md) before doing any work.**

## Repository index

- [Detailed sequential project plan](docs/PORTING_PLAN.md)
- [Current phase approval ledger](docs/PHASE_GATES.json)
- [Rules for Codex, AI agents and contributors](AGENTS.md)
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

No game runtime or performance claim is verified until its documented gate has been tested on the indicated platform.

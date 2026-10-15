# Hungry Shark Evolution — PS Vita Port

Community research and development repository for a **native PlayStation Vita port** of *Hungry Shark Evolution*, preserving the original gameplay logic and Unity assets wherever technically possible.

> **Status (2026-10-14): research / asset-recovery preparation. No playable PS Vita build or VPK has been produced or tested.**

## Chosen reference build

| Item | Audited result |
| --- | --- |
| Game | Hungry Shark Evolution **6.4.2**, Android versionCode **228** |
| Unity player | **5.6.4p3**, confirmed from Unity serialized-file headers |
| Script backend | **Mono**, not IL2CPP |
| Main assembly | `Assembly-CSharp.dll` (4,958,208 bytes) |
| Other game assemblies | `Assembly-CSharp-firstpass.dll`, `Assembly-UnityScript.dll`, `Assembly-UnityScript-firstpass.dll` |
| Scene serialized files | `level0` through `level17` (18 files; **not** necessarily 18 gameplay levels) |
| Architecture | Android `armeabi-v7a` |
| Candidate Vita editor | Unity **2018.2** with a working, legitimately obtained PSP2 build target |

The newer **14.5.0** Android package was also investigated, but uses Unity 6 / IL2CPP and is a much harder initial candidate. **6.4.2 is the working baseline, not a confirmed port.**

## Approach

1. Audit a legally obtained local APK and reconstruct split Unity data files.
2. Import Unity data **together with Managed assemblies** into [AssetRipper](https://github.com/AssetRipper/AssetRipper) and assess script / GUID / scene recovery.
3. Examine the existing C# assemblies with [ILSpy](https://github.com/icsharpcode/ILSpy); preserve code behavior and serialized contracts where possible.
4. Rebuild an export in Unity 2018.2 **on a separate copy**, resolve platform dependencies, then test a minimal scene.
5. Generate a Vita-native player with Unity PSP2; add controls, offline services, save handling, renderer fixes and performance work incrementally.

The Android `libunity.so` is **not** a drop-in Vita runtime. This project does not claim that an APK can simply be imported and recompiled.

## Documentation

- [Technical audit and reproducible evidence](docs/TECHNICAL_AUDIT.md)
- [Porting plan, milestones and success gates](docs/PORTING_PLAN.md)
- [Experiment log: proven / failed / not attempted](docs/EXPERIMENTS.md)
- [Tools and authoritative references](docs/TOOLS.md)
- [Rules for AI agents and contributors](AGENTS.md)

## Local extraction (Windows / Python)

Requires Python 3 and your **own** APK of version 6.4.2.

- On Windows: drag the APK onto [`tools/ARRASTRAR_HUNGRY_SHARK_6.4.2.bat`](tools/ARRASTRAR_HUNGRY_SHARK_6.4.2.bat).
- Cross-platform: `python tools/Preparar_Hungry_Shark_Evolution_6.4.2.py path/to/game.apk output_directory`
- The tool creates `output_directory/Data/` and combines `.splitN` fragments in numeric order. **It does not recover the original Unity editor project.**
- Keep `Data/Managed/` available during asset import. Record the APK SHA-256 to ensure test results are comparable.

## Copyright and distribution

*Hungry Shark Evolution*, its code, art, audio, and other game assets belong to their respective owners. **No APK, extracted game DLLs, Unity assets, proprietary SDKs, or paid platform files are distributed here.** Contributors must use their own lawfully obtained input and respect applicable rights and licenses. Do not upload extracted content in commits, issues, logs, or releases. No affiliation with Ubisoft or the game's developers is implied.

**No release should be described as working until it is tested on physical PS Vita hardware.**

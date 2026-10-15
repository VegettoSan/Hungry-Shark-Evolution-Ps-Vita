# Halo 2D PSP2 reference — verified GitHub inspection

**Reviewed:** 2026-10-14 · **Source repository:** [VegettoSan/Halo2D_PsVita](https://github.com/VegettoSan/Halo2D_PsVita) · **Inspection:** static GitHub files + published release metadata. No remote host / local Windows installations were accessed and no PSP2 compilation was executed during this review.

## Confirmed findings

| Evidence | Observation | Relevance to Hungry Shark |
| --- | --- | --- |
| [ProjectVersion.txt](https://github.com/VegettoSan/Halo2D_PsVita/blob/main/Halo%202D/ProjectSettings/ProjectVersion.txt) | **`m_EditorVersion: 2018.2.19f1`** | **Preferred migration editor after P04 approval.** Supersedes the previously proposed 2018.2.21f1. |
| [Official Unity 2018.2.19f1 release](https://unity.com/releases/editor/whats-new/2018.2.19f1) | Unity confirms editor version, **Dec 7, 2018**, changeset **06990f28ba00** | Exact official version reference, not a guessed Unity build |
| [ProjectSettings.asset](https://github.com/VegettoSan/Halo2D_PsVita/blob/main/Halo%202D/ProjectSettings/ProjectSettings.asset) | Has PS Vita-specific settings: `psp2PowerMode`, `psp2MemoryExpansionMode`, `psp2SaveDataQuota`, `psp2LiveArea*`; includes `PSP2` icon target. **Contains potentially sensitive packaging information; DO NOT reproduce those values.** | Shows a genuinely PSP2-configured Unity project; **not** a reusable template for secret/platform fields |
| [Unity PSP2 player cache](https://github.com/VegettoSan/Halo2D_PsVita/tree/main/Halo%202D/Library/PlayerDataCache/PSP2) | `Data/Managed/UnityEngine.PSVitaModule.dll`, `level0`, `level1`, `globalgamemanagers` and other PSP2 build-cache files | Strong historical evidence that the project underwent Unity PSP2 build processing. Cached outputs should **not** be copied into another project |
| [Release 1.76](https://github.com/VegettoSan/Halo2D_PsVita/releases/tag/1.76) | Published `Halo2D.vpk` (52,679,023 bytes), released July 27, 2024 | Real released Vita artifact; does not independently confirm that the user's *current* PC editor/module can still build |
| [EditorBuildSettings.asset](https://github.com/VegettoSan/Halo2D_PsVita/blob/main/Halo%202D/ProjectSettings/EditorBuildSettings.asset) | Enabled `Assets/Scenes/MainMenu.unity` and `Assets/Scenes/Game.unity` | Demonstrates scene-order configuration only; do not reuse Halo's scene order for Hungry Shark |
| [GraphicsSettings.asset](https://github.com/VegettoSan/Halo2D_PsVita/blob/main/Halo%202D/ProjectSettings/GraphicsSettings.asset) | Built-in render pipeline (`m_CustomRenderPipeline: {fileID: 0}`), forward/default rendering | A useful comparison for a conservative Vita graphics setup, **not proof** HSE materials/shaders will work |
| [VitaInputManager.cs](https://github.com/VegettoSan/Halo2D_PsVita/blob/main/Halo%202D/Assets/Scripts/Input/VitaInputManager.cs) and [VitaInputModule.cs](https://github.com/VegettoSan/Halo2D_PsVita/blob/main/Halo%202D/Assets/InputModules/VitaInputModule.cs) | Input uses old Unity Input Manager axes/buttons and custom EventSystem module; `UNITY_PSP2` preprocessor test is present | Possible **P10 input reference**, NOT something to transplant into HSE before native gameplay passes |
| [Sony Vita input README](https://github.com/VegettoSan/Halo2D_PsVita/blob/main/Halo%202D/Assets/SonyExamples/Vita/Input/README.txt) | Notes an axis dead-zone recommendation of 0.25 | For future input testing, **re-check actual controller profiles instead of copying numbers** |

## Important limitations and cautions

1. **Halo 2D proves a historically functional 2018.2.19f1 PSP2-oriented project and has a released VPK.** It does **not prove** Unity 2018.2.19f1 + PSP2 is present, correctly licensed or still builds on the **current Windows installation**. P00 can inventory that installation; P07 still requires its own controlled physical-Vita smoke test.
2. This is a Unity-authored 2D game, **not** a reconstruction of a Unity 5.6 Android APK. It validates *workflow and configuration patterns*, **not** the success of AssetRipper, Mono restoration or HSE's gameplay migration.
3. The repo tracks `Library/`, build-cache assemblies, PSP2 plugin example files and potential sensitive Sony packaging data. **Never copy those binaries, credentials, keystones, or platform-dependent settings** to Hungry Shark. Examine sensitive material privately; if credentials are authentic, revoke/rotate and remove them from public history as appropriate. Deleting a file in a new commit alone does not erase old public Git objects.
4. **Potential input issue** noted in static review: in `VitaInputManager.cs`, the Square-button release branch calls `OnCrossUp` rather than `OnSquareUp`. Do not reuse the controller verbatim without a test-driven review.
5. The `Library/PlayerDataCache/PSP2` assembly presence can help verify which Unity platform assemblies existed historically but these are vendor/player-specific output, not a distribution or build dependency to commit.

## Approved reuse strategy

**Now, P00:** log that **the preferred Unity 2018.2 editor version is 2018.2.19f1** based on file evidence. On the actual PC, record the path, `Help > About` value, installed PSP2 module visibility and any legitimate build target access. Do **not** migrate HSE or mark P00 approved from GitHub evidence alone.

**Only after P04:** migrate a **frozen, working HSE Unity 5.6.4p3 PC baseline** to a **separate Unity 2018.2.19f1 clone**. Use Halo 2D as a reference for platform configuration, not game code or copyrighted content.

**Only in P07:** create an independent Unity 2018.2.19f1 PSP2 smoke test on real Vita and record logs/success. **P08–P10** may then consult graphics, input and package layout patterns, reimplementing only game-specific adapters with tests.

## Evidence classification

- **VERIFIED (repository):** editor version, PSP2 settings and cache, VPK release, scene list, script/API symbols.
- **UNVERIFIED (current PC):** installed editor file hashes; actual PSP2 module; current account/toolchain availability; HSE performance and compatibility.
- **UNTESTED:** any migration, Unity 2018 desktop gameplay, PSP2 build of Hungry Shark, copied input adaptation.

For the authoritative gate state, consult [PHASE_GATES.json](PHASE_GATES.json). This reference is **not** a gate approval.

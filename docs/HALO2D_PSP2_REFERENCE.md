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
| [VitaInputManager.cs](https://github.com/VegettoSan/Halo2D_PsVita/blob/main/Halo%202D/Assets/Scripts/Input/VitaInputManager.cs) and [VitaInputModule.cs](https://github.com/VegettoSan/Halo2D_PsVita/blob/main/Halo%202D/Assets/InputModules/VitaInputModule.cs) | Input uses old Unity Input Manager axes/buttons and custom EventSystem module; `UNITY_PSP2` preprocessor test is present | Possible **P10 auxiliary code reference**; prefer the **standard Input Manager asset/settings** above over this script. No early gameplay integration |
| [Sony Vita input README](https://github.com/VegettoSan/Halo2D_PsVita/blob/main/Halo%202D/Assets/SonyExamples/Vita/Input/README.txt) | Notes an axis dead-zone recommendation of 0.25 | For future input testing, **re-check actual controller profiles instead of copying numbers** |

## Reusing Halo 2D's **standard Unity Input Manager** (verified, P10 reference only)

The project owner explicitly authorized reusing the **Project Settings > Input Manager** bindings from Halo 2D, as they are standard Unity legacy input settings, not a custom runtime requirement.

**Source:** [`Halo 2D/ProjectSettings/InputManager.asset`](https://github.com/VegettoSan/Halo2D_PsVita/blob/main/Halo%202D/ProjectSettings/InputManager.asset), created with **Unity 2018.2.19f1**. Static inspection: **24 `m_Axes` entries** (15 distinct names), with multiple entries under the same name for alternative devices. This is separate from `VitaInputManager.cs`.

| Unity legacy name(s) | Settings observed in Halo 2D | Recommended reuse |
| --- | --- | --- |
| `Horizontal`, `Vertical` | Keyboard arrows/A-D/W-S; duplicated button mappings: horizontal **joystick 11 / 9**, vertical **10 / 8**; plus analog axes **0 and 1**. Axis vertical has `invert: 1`. | Preserve HSE's existing movement axis names first. On 2018.2 Vita target, merge only required compatible entries. Confirm analog sign on hardware. |
| `PR Horizontal`, `PR Vertical` | `type: 2` (Joystick Axis), axes **0/1**, `dead: 0.19`, `invert: 0/1`; `PR Horizontal` sensitivity `0.5`. | Separate, optional analog aliases; do not presume `PR` means right stick. Test before binding. |
| `PR X`, `PR Y` | `type: 2`, axes **0/1**, `dead: 0.19`, no inversion. | Optional aliases sharing axes 0/1; not proven right-stick axes. |
| `Fire1`, `Fire2`, `Fire3`, `Jump` | Duplicated keyboard/mouse bindings and `joystick button 0/1/2/3` respectively. | Candidate actions for gameplay only after physical Vita button inspection and native-gameplay parity. |
| `Submit`, `Cancel` | `Submit`: keyboard Return/Enter/Space and joystick **0**; `Cancel`: Escape and joystick **1**. | Natural starting point for UI navigation, but verify HSE menus/touch behavior. |
| `Mouse X`, `Mouse Y`, `Mouse ScrollWheel` | Standard mouse mappings retained. | Preserve existing desktop fallback rather than replacing HSE's settings. |

**Mapping caveats (important):**

1. **`type: 0`** is Key or Mouse Button; **`type: 2`** is Joystick Axis. The numeric button/axis mapping above is from *Halo's file*, **not a newly measured hardware button map** for Hungry Shark. Test each physical Vita control. In the committed file, there is no separate confirmed right-stick axis definition.
2. The committed `InputManager.asset` does **not** define named mappings `Cross`, `Square`, `Triangle`, `Circle`, `LTRIG`, `RTRIG`, `Start`, `Select`, `Left Stick Horizontal`, `Left Stick Vertical`, `Right Stick Horizontal`, `Right Stick Vertical`, even though Halo's separate `VitaInputManager.cs` reads some of those names. **Do not copy that script as a functioning mapping by assumption.**
3. Halo's actual analog dead zone is `0.19`. The bundled [Sony Input example README](https://github.com/VegettoSan/Halo2D_PsVita/blob/main/Halo%202D/Assets/SonyExamples/Vita/Input/README.txt) recommends `0.25`; **measure and follow target hardware/platform requirements before finalizing**. Do not silently overwrite tuned controls.
4. For Unity 2018.2, use the built-in **Edit > Project Settings > Input** entries and standard `Input.GetAxis`, `Input.GetButton`, `Input.GetButtonDown`; no third-party input manager is required solely to read these mappings. However, adding axes alone does **not** make Hungry Shark use them; route actions through the existing gameplay input consumers, not a replacement gameplay controller.
5. The complete Halo `InputManager.asset` may be copied **to a private staging copy** for inspection, but must **not replace** the recovered HSE `ProjectSettings/InputManager.asset` wholesale. Merge only the necessary axis definitions after backing up HSE's controls and testing keyboard/mouse/touch and Vita behaviors. Preserve original HSE axes, duplicate-name alternatives and GUID-independent project settings.
6. **Phase discipline:** documenting the candidate settings during P00 does **not** authorize modifying the HSE game or applying Vita controls now. The actual mapping and physical hardware acceptance are in **P10**, after the approved P09 gameplay gate. Physical button mapping and touchscreen input must be tested with a clean and resumed Vita session, including menus, feeding and movement; compare against the frozen P06 PC behavior.

**Goal for P10:** reuse Unity's standard input configuration to the maximum extent possible; add a small adapter only if the original HSE input path cannot consume the platform-independent axis/button names as-is.


## Important limitations and cautions

1. **Halo 2D proves a historically functional 2018.2.19f1 PSP2-oriented project and has a released VPK.** It does **not prove** Unity 2018.2.19f1 + PSP2 is present, correctly licensed or still builds on the **current Windows installation**. P00 can inventory that installation; P07 still requires its own controlled physical-Vita smoke test.
2. This is a Unity-authored 2D game, **not** a reconstruction of a Unity 5.6 Android APK. It validates *workflow and configuration patterns*, **not** the success of AssetRipper, Mono restoration or HSE's gameplay migration.
3. The repo tracks `Library/`, build-cache assemblies, PSP2 plugin example files and potential sensitive Sony packaging data. **Never copy those binaries, credentials, keystones, or platform-dependent settings** to Hungry Shark. Examine sensitive material privately; if credentials are authentic, revoke/rotate and remove them from public history as appropriate. Deleting a file in a new commit alone does not erase old public Git objects.
4. **Potential input issue** noted in static review: in `VitaInputManager.cs`, the Square-button release branch calls `OnCrossUp` rather than `OnSquareUp`. Do not reuse the controller verbatim without a test-driven review.
5. The `Library/PlayerDataCache/PSP2` assembly presence can help verify which Unity platform assemblies existed historically but these are vendor/player-specific output, not a distribution or build dependency to commit.

## Shader/rendering reference

Halo 2D's Unity 2018.2.19f1 **Built-in Render Pipeline** and PSP2 build configuration provide a useful historical *render pipeline* reference, **not proof** that any particular Hungry Shark shader is compatible. PSP2 has its own Unity shader compiler; consult [candidate optimizations and hardware validation methodology](SHADERS_PSVITA_REFERENCE.md). Keep original HSE visual effects until validated, and benchmark any changes on hardware.

## Approved reuse strategy

**Now, P00:** log that **the preferred Unity 2018.2 editor version is 2018.2.19f1** based on file evidence. On the actual PC, record the path, `Help > About` value, installed PSP2 module visibility and any legitimate build target access. Do **not** migrate HSE or mark P00 approved from GitHub evidence alone.

**Only after P04:** migrate a **frozen, working HSE Unity 5.6.4p3 PC baseline** to a **separate Unity 2018.2.19f1 clone**. Use Halo 2D as a reference for platform configuration, not game code or copyrighted content.

**Only in P07:** create an independent Unity 2018.2.19f1 PSP2 smoke test on real Vita and record logs/success. **P08–P10** may then consult graphics, input and package layout patterns, reimplementing only game-specific adapters with tests.

## Evidence classification

- **VERIFIED (repository):** editor version, PSP2 settings and cache, VPK release, scene list, script/API symbols.
- **UNVERIFIED (current PC):** installed editor file hashes; actual PSP2 module; current account/toolchain availability; HSE performance and compatibility.
- **UNTESTED:** any migration, Unity 2018 desktop gameplay, PSP2 build of Hungry Shark, copied input adaptation.

For the authoritative gate state, consult [PHASE_GATES.json](PHASE_GATES.json). This reference is **not** a gate approval.

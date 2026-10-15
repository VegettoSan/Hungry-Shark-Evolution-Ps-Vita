# Game data extraction and PS Vita application layout

**Declared port identity:** Title ID **`HGSKE1178`**; display name **Hungry Shark Evolution Vita**. Source: Hungry Shark Evolution Android **6.4.2** (versionCode 228), Unity **5.6.4p3 Mono**. Intended target: Unity **2018.2.19f1 PSP2**. See [the canonical identity](../config/port_identity.json).

**Status: RESEARCH ONLY (P00).** We have no confirmed Hungry Shark PSP2 VPK, no measured game-data loader, no finished end-user asset subset or extraction package. Do NOT implement later phases before owner-approved phase gates.

## 1. Confirmed APK source inventory

The following was independently recounted directly from the user-supplied APK's ZIP directory. No Unity editor/runtime reconstruction was performed in this audit.

| Source section | Files / entries | Uncompressed size | Developer role |
| --- | ---: | ---: | --- |
| Full APK | 3,073 ZIP entries | 350,933,570 bytes | Verified original input, NOT Vita payload |
| `assets/bin/Data/` | 2,366 ZIP entries; **2,295 logical files after rejoining fragments** | **311,224,301 bytes** | Complete original Unity serialized data for project recovery |
| Hex-named objects at Unity Data root | 1,928 filenames of 32 hex characters | Included above | Preserve exact names and dependencies |
| `Data/*.resource` | 305 entries | 6,104,000 bytes | Sidecar resources; preserve paired names |
| `Data/Managed/` | 21 assemblies | 9,338,880 bytes | Required for Mono script reference/type reconstruction |
| `level0` to `level17` | 18 logical scene files (29 ZIP entries including fragments) | 18,860,352 bytes | Scene metadata; NOT necessarily 18 playable levels |
| `*.splitN` | 86 fragments forming 15 whole files | Included above | Reconstruct in ascending numeric order |
| `lib/armeabi-v7a/*.so` | 7 Android libraries | 28,603,940 bytes | Android-specific: NOT part of an ordinary PSP2 runtime |
| `assets/controllers.json` | 1 small file | 181 bytes | Reference-only Android input metadata |
| `assets/api_key.txt` and `assets/msdk` | Android-specific extras | Outside Unity Data | Do not distribute automatically; service/key sensitivity |

**2,295 = 2,366 ZIP Data entries – 86 fragments + 15 joined files.** These are **disk archive sizes**, not RAM/VRAM consumption or final VPK size. Sampling all 1,928 hex-name roots found no `UnityFS`/`UnityRaw`/`UnityWeb` signatures: do NOT assume every hex-named file is a self-contained cross-platform AssetBundle.

Expected APK SHA-256: `52b7d8754ab8fec75c3b656a0dc5ec8dc849565adc402487a3ac093f6a481f7a`. No OBB was packaged in this particular APK. Additional download/cache requirements, if any, remain UNTESTED.

## 2. Two very different extractors: developer vs. end user

**Developer recovery — planned P01 after approval:** run [our existing local ZIP extractor](../tools/Preparar_Hungry_Shark_Evolution_6.4.2.py) using a user-owned APK, reconstruct all 15 split groups, preserve the **complete** `Data` tree and `Managed` assemblies, then open with AssetRipper and recover a **private Unity 5.6.4p3** editor project. We should **not** discard any serialized object or resource based on size/extension because dependency links are not yet mapped. This extractor is NOT a final user-game-data installer.

**End-user extraction — design only, P12:** after successful Unity 5.6 PC, Unity 2018 PC, and physical Vita tests, determine exactly which original assets can be legally used and are loadable by the final Vita runtime. A user-owned APK extractor will then emit only an approved, tested subset plus a manifest, into a PC staging directory for copying to the installed Vita app. Do not promise this flow will work until proven.

**Critical incompatibility:** original Android `assets/bin/Data/globalgamemanagers`, `levelN`, `sharedassetsN.assets`, assemblies or any Android AssetBundles are NOT interchangeable with Unity 2018.2 **PSP2-generated** serialized runtime files. AssetBundles can contain platform-specific shaders and texture formats. Do not overwrite PSP2 build files with Android ones just because both filenames look similar. A raw unzipper alone cannot generally regenerate Vita-compatible Unity content.

If a working build requires whole Unity scenes/assets compiled for PSP2, a final public end-user converter may need a genuine platform conversion path. **A proprietary PSP2 editor/module is not an acceptable hidden dependency for a universal public extractor**. Document this as a possible blocker and do not invent a loader.

### xdelta3 in other Unity port distribution workflows

Real releases from [Predators](https://github.com/AJ170/Predators-PSVita-Port) and [Pocket Mortys](https://github.com/celticg/AS-Pocket-Mortys-vita) use **a user-owned Android OBB or APK + precomputed xdelta3 patch → Vita game-data ZIP**, then copy content to the installed `ux0:app/<TITLEID>/`. This demonstrates a user-friendly packaging pattern **without requiring the user to run Unity**. It does **not** demonstrate automatic Android Unity → PSP2 conversion. Mechanistically, producing such a delta requires a prepared target archive; **how exactly each author generated their target archive is not published** in either repository. See [full binary-patch research, size/version evidence, legal caveats and gated experiment plan](XDELTA_UNITY_PORTS_RESEARCH.md).

For Hungry Shark, treat **stub VPK + xdelta** as a **candidate P12 distribution option**, alongside a conversion script. It only becomes viable after the real target PSP2 output has passed gameplay and content tests, and patch redistribution rights have been assessed. Do not assume that the original Android `Data` files can be dropped into the Vita `Media` directory.

## 3. Chosen location and path probes

| Meaning | Path/API | Confidence |
| --- | --- | --- |
| App installed location | **`ux0:app/HGSKE1178/`** | Standard title install convention and our chosen Title ID |
| Runtime app mount | **`app0:`** | Standard PS Vita app mount; verify the exact paths with the Unity player |
| Built-in Unity data | `Application.dataPath` | **PSP2 result UNKNOWN until P07 real-device test** |
| Streaming assets | `Application.streamingAssetsPath` | **PSP2 result UNKNOWN until P07**; do not guess `Media/Data/` |
| User-provided extra data | Proposed dedicated subfolder beneath the installed title, e.g. `app0:/GameData/` | **Hypothesis only; directory name not yet committed**. Confirm in P08 with an ordinary harmless file |
| Saves / progress | `Application.persistentDataPath` or supported Vita save API | **Exact PSP2 writable location UNKNOWN**; must not use the installed read-only application directory for saves |
| `ux0:data/` | Often used by other homebrew loaders | **Not our default for Unity application assets**, but not globally forbidden if a measured need arises |

The PS Vita convention for application files is `ux0:app/<TITLEID>/`, with runtime `app0:`. **App files and save files are different**; do not use `ux0:app` for user progress. VPK reinstall/upgrade can replace installed application content and end-user data placed inside it; our installation docs must specifically test and warn about preservation.

The actual **Unity-generated PSP2 VPK internal file tree** will be examined at P08. Halo 2D `Library/PlayerDataCache/PSP2/Data` is historical build cache, **not proof** of the final VPK relative folder structure. Probe all three Unity path APIs with a tiny signed-off PSP2 smoke test, not by extrapolating from Android or PC.

## 4. Final extractor contract (design, not ready for release)

- **Input:** user-selected original APK 6.4.2, verified package/versionCode 228/hash and engine version. Do not silently accept the Unity 6 / IL2CPP release or a different modified APK.
- **Output:** private staging folder on the PC. The eventual manifest includes each verified input relative path, output relative path, conversion operation, expected SHA-256, byte size and required/optional flag.
- **Exclusions:** no native Android `.so`, signing material, keys, entire original Mono DLLs by default, Android service caches, or guessed Unity platform bundles.
- **Completeness:** dependency closure must cover the menus, sharks, levels, audio, UI, saves and any optional content claimed to work. The precise **minimal** list comes from P08–P12 **runtime tests**, not from this ZIP inventory.
- **Safety:** protect against duplicate/escaping ZIP paths, corrupt chunks, case collisions, mismatched versions, partial extraction and write failures. Make a clean staging directory, verify checksums and give clear missing-data diagnostics.
- **Installation:** install an appropriately permitted VPK; copy only tested user-owned output to the *exact validated relative app path* under `ux0:app/HGSKE1178/`. Never overwrite `eboot.bin`, `sce_sys` or Unity-generated PSP2 runtime files. Reinstall/update retention must be tested.
- **Distribution:** no copyrighted APK or extracted proprietary content in GitHub/Release. The legal right to distribute compiled player/game resources must be verified; an empty launcher VPK does not guarantee APK assets are loadable.

A *candidate* future manifest schema (NOT real files):

```json
{
  "schema_version": 1,
  "title_id": "HGSKE1178",
  "source_version": "6.4.2",
  "recipe_version": "PENDING_P08",
  "files": [
    {
      "source_path": "VERIFIED_APK_RELATIVE_PATH",
      "destination_relative_path": "ONLY_AFTER_PSP2_PATH_TEST",
      "operation": "VALIDATED_COPY_OR_CONVERSION",
      "sha256": "EXPECTED_OUTPUT_HASH",
      "required": true
    }
  ]
}
```

## 5. Phase gates for data and identity

- **P00 CURRENT:** research APK structure and Vita paths; define identity in JSON; do not edit game source or declare Unity recovery successful.
- **P01:** complete extraction/import, preserve the entire original Unity graph and note additional file loads.
- **P02–P04:** restore faithful Unity 5.6.4p3 PC gameplay and test dependency usage. Do not purge assets to reduce size.
- **P05–P06:** after approval, configure Unity 2018.2.19f1 display name **Hungry Shark Evolution Vita**, and plan PSP2 Title ID **HGSKE1178** without copying Halo's packaging credentials; validate desktop parity.
- **P07:** test the actual Unity PSP2 install/build toolchain and log the runtime data, streaming and persistent path values.
- **P08:** examine the real generated PSP2 app file tree, test application reads, and choose an exact verified extra-data subdirectory *only if needed*.
- **P09–P11:** validate representative/full gameplay resources, performance and save persistence; 30 FPS target, optional 60 if feasible.
- **P12:** implement, test and approve public end-user extractor/install guide **only after** confirming a real Vita loader, compatible content conversion, proper distribution rights and installation/reinstall behavior.

## Research sources

- [PS Vita filesystem app0:, ux0:app, savedata](https://www.psdevwiki.com/vita/index.php/Files_on_the_PS_Vita)
- [Vita3K installed title directories](https://vita3k.org/quickstart.html)
- [Unity historical AssetBundle platform-incompatibility FAQ](https://docs.unity3d.com/es/530/Manual/AssetBundleFAQ.html)
- [Unity historical AssetBundle API and shaders/textures caveats](https://docs.unity3d.com/es/2017.4/ScriptReference/AssetBundle.html)
- [Unity dataPath runtime platform differences](https://docs.unity.cn/2018.3/Documentation/ScriptReference/Application-dataPath.html)
- [Unity StreamingAssets read-only deployment paths](https://docs.unity3d.com/es/2020.1/Manual/StreamingAssets.html)
- [Unity persistentDataPath use for saves](https://docs.unity.cn/ScriptReference/Application-persistentDataPath.html)
- [Halo 2D PSP2 reference](HALO2D_PSP2_REFERENCE.md)

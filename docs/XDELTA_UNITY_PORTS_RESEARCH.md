# xdelta3 in Unity PS Vita ports — Predators, Pocket Mortys, Hungry Shark

**Researched 2026-10-14.** **REFERENCE / NOT AN IMPLEMENTED EXTRACTOR.** P00 research is permitted; no Vita data patch/converter, built Hungry Shark VPK, successful Android→PSP2 scene transplantation, or end-user installer has been tested. Respect [phase gates](PHASE_GATES.json).

## 1. What the original authors actually publish

Directly inspected primary GitHub READMEs, repository trees and release asset metadata. Both repos' default branches contain **only README.md** (no export source, patch-creation script or verified bill of materials). Therefore we can confirm **the user-facing commands and inputs**, **not** what precise bytes/Unity assets were rewritten inside the generated data ZIP.

| Port (primary sources) | Source needed from user | xdelta patch / actual release size | Stub VPK / release size | Output, Vita destination |
| --- | --- | --- | --- | --- |
| [**Predators — AJ170**](https://github.com/AJ170/Predators-PSVita-Port) · [1.0 release](https://github.com/AJ170/Predators-PSVita-Port/releases/tag/1.0), published **2026-10-10** | **Predators Android 1.5.1 OBB**, specifically `main.21.com.fde.predators.obb` | `PredatorsData.xdelta`, **123,402,587 bytes** | `PREDATORS-FinalBuild_v1.0-stub.vpk`, **14,860,020 bytes** | Patch produces `PredatorsData.zip`; extract the **`Media/`** folder to **`ux0:app/PRED42069/`** |
| [**Pocket Mortys — celticg**](https://github.com/celticg/AS-Pocket-Mortys-vita) · [v0.9 release](https://github.com/celticg/AS-Pocket-Mortys-vita/releases/tag/v0.9), published **2026-08-22** | **Pocket Mortys Android 1.94 APK**, exact required build/contents and hash; README **does not state the hash value** | `PocketMortysData.xdelta`, **77,051,044 bytes** | `PocketMortys_Stub.vpk`, **16,814,806 bytes** | Patch produces `PocketMortysData.zip`; extract the ZIP's contents into **`ux0:app/POMO00137/`** |

Pocket Mortys **explicitly says** it was built with **Unity 2018.2.19f1 PSP2** (also the editor version in our [Halo 2D repository](HALO2D_PSP2_REFERENCE.md)). **Predators' README does not identify its exact Unity version or exporter; do not assert it is Unity 2018.2 without evidence.** The developer credits another researcher for decompilation, but neither release includes a source-rebuild tutorial.

**Save distinction:** Pocket Mortys explicitly documents saves under `ux0:data/PocketMortys/`, separate from `ux0:app/POMO00137/`. This is an **observed design in that particular port**, **not proof** Unity's `Application.persistentDataPath` resolves identically in Hungry Shark's eventual PSP2 runtime.

### Commands *as documented by authors*

**Predators (requires original 1.5.1 OBB):**
```powershell
xdelta3.exe -d -B 536870912 -s main.21.com.fde.predators.obb PredatorsData.xdelta PredatorsData.zip
```

**Pocket Mortys (requires original 1.94 APK):**
```powershell
xdelta3.exe -d -B 134217728 -s Pocket_Mortys_1.94.apk PocketMortysData.xdelta PocketMortysData.zip
```

These commands only reconstruct an archive with the matching original. They **do not decompile Unity, run an APK on PSP2, or convert asset formats at user installation time**. `-B` is the xdelta **source window/cache size**, expressed in bytes: **512 MiB** for Predators, **128 MiB** for Pocket Mortys. It is **not** a shader/Unity conversion flag, target system RAM requirement, or decompressed output size. [xdelta3 official CLI](https://github.com/jmacd/xdelta/blob/main/site/docs/commandline.md).

### Why stub VPK + .xdelta?

One plausible explanation is that the developer **already prepared/rebuilt** a PSP2-friendly data package on their PC, then applied a binary delta between the user's original APK/OBB and that package. The user reproduces the precomputed result without needing their own Unity PSP2 SDK/editor. This is exactly what xdelta's generic encode/decode algorithm enables. However, **the exact Predators/Pocket Mortys target ZIP contents and patch-generation steps remain unverified**: source repos show README only. Do **not** claim they 'automatically convert Unity Android scenes to Vita' or that the patch contains only art/textures.

## 2. What xdelta actually does — mechanism and limits

xdelta3 implements VCDIFF/RFC 3284 binary differential encoding. It can reconstruct an output using a supplied source file and patch. It does **not care** whether the input is APK, OBB, ZIP, Unity data, images or unrelated bytes.

```text
Creator (private, after a successful target Vita data build):
  ORIGINAL_ANDROID_APK/OBB (SOURCE) + PREPARED_VITA_DATA.ZIP (TARGET)
         xdelta3 -e -s SOURCE TARGET PATCH
                 ↓
              PATCH.xdelta

End user (no Unity installation required):
  ORIGINAL_ANDROID_APK/OBB (SOURCE) + PATCH.xdelta
         xdelta3 -d -s SOURCE PATCH DATA.zip
                 ↓
             DATA.zip  → extract validated files → ux0:app/<TITLEID>/...
```

The model **may** allow distributing an efficient difference of the newly rebuilt PSP2 data relative to the source Android package. But a delta can include **literal bytes copied from the target**, including substantial material not present in the source. A large patch can encode effectively a whole archive even with an unrelated source. **Therefore a patch is not automatically a legal, copyright-free release** or proof that its users actually own the game. Independent redistribution rights/legal assessment remains required.

Delta efficiency depends on byte-level similarity of **the exact complete input bytes** and target. Raw compressed APK/OBB versus a freshly compressed PSP2 output may have little similarity and therefore create a **large patch**. Neither observed project supplies a benchmark that proves xdelta substantially saves disk space compared with the final target ZIP. We cannot infer target ZIP sizes from patch sizes.

### xdelta3 Windows download — verified official GitHub release

- **[xdelta3 v3.2.2 maintained release](https://github.com/jmacd/xdelta/releases/tag/v3.2.2)**, published **2026-10-09**, license Apache-2.0 for this maintained branch. **[Windows x64 binaries](https://github.com/jmacd/xdelta/releases/download/v3.2.2/xdelta3-3.2.2-windows-x86_64.zip)**, reported GitHub size **188,145 bytes**. Contains standalone compiled tool; verify provenance and hash locally.
- **[Historic v3.1.0 GPL releases](https://github.com/jmacd/xdelta-gpl/releases/tag/v3.1.0)** and **[Windows x64](https://github.com/jmacd/xdelta-gpl/releases/download/v3.1.0/xdelta3-3.1.0-x86_64.exe.zip)**, release published **2016-01-08**. Source linked by both older ports. Keep it available **only if compatibility testing of their old patches needs it**.
- The maintained 3.2.x series adds **BLAKE3 armor** (whole-source/target checksums by default in certain builds), while legacy 3.1.x does not. **Do not assume patches generated with 3.2.2 decode under 3.1.0 or vice versa without an actual matrix test**. Pin the build tested for end users, and always validate input SHA-256 and output checksum independently. [Versioned docs](https://github.com/jmacd/xdelta/blob/main/site/docs/armor.md).
- No xdelta binary or patch has been added to this repository. Merely preloading this tool is allowed during P00, but it is **not required for original-game Unity 5.6 reconstruction**.

## 3. Impact on our Hungry Shark 6.4.2 workflow

**This significantly improves our *candidate end-user delivery design*, but does not change the original-engine recovery and port stages.**

1. **P00–P04 (no change):** audit original owned APK and restore game in Unity 5.6.4p3 PC; preserve full 2,295 logical Unity data files and all Mono assemblies in a private development source tree, not the public Git repo.
2. **P05–P06:** migrate a clone to Unity 2018.2.19f1 and confirm the full actual game works on PC; **the planned app name is Hungry Shark Evolution Vita**, Title ID **HGSKE1178**.
3. **P07–P08:** perform real PSP2 smoke test and first game build, then **inspect actual Unity PSP2 output and final VPK**. Determine whether the stub VPK can contain only a permitted/redistributable loader and which correctly built game data may be supplied afterwards under **`ux0:app/HGSKE1178/`**. Do not hardcode a guessed `Media/` folder: verify Unity's file access and actual package structure on hardware.
4. **P09–P11:** validate playable real-device data, menu, sharks, effects, offline services, saves, 30 FPS main goal and 60 FPS stretch; freeze exact known-good **Vita target data ZIP** with a manifest/checksums. Avoid changing target ZIP while testing an xdelta patch.
5. **P12 (owner-approved only):** evaluate a **stub VPK + xdelta** workflow as a candidate alongside a deterministic local converter. Generate a patch from the exact original HSE APK **only against a proven prepared target ZIP**, if legally redistributable; test Windows x64 decode, hashes, ZIP extraction, install, reinstall, upgrades, missing/wrong APK and no leaked game data in published artifacts.

### Illustrative future commands — not a runnable HSE patch yet

```powershell
# DEVELOPERS ONLY after building + verifying the target Vita data ZIP:
xdelta3.exe -e -B 268435456 -s HungrySharkEvolution_6.4.2.apk HungrySharkVitaData.zip HungrySharkVitaData.xdelta

# END USERS ONLY after a real patch exists and has passed tests:
xdelta3.exe -d -B 268435456 -s HungrySharkEvolution_6.4.2.apk HungrySharkVitaData.xdelta HungrySharkVitaData.zip
```

The `-B 268435456` (256 MiB) here is an **illustrative tuning value, not a requirement**; final choice is benchmarked against the exact APK/window sizes and available PC RAM. `HungrySharkVitaData.zip` and `HungrySharkVitaData.xdelta` **DO NOT EXIST**; those are suggested future names, not an authorized file list. No guessed PSP2 `Media` output, saves path, or content-ID included.

### Design decisions to test: direct APK delta versus normalized asset package

| Option | Potential advantage | Important caveat |
| --- | --- | --- |
| Patch directly from exact Android 6.4.2 APK to target PSP2 data ZIP | Simple 1-file user input, as Pocket Mortys; no Unity installation for user | May produce huge deltas because ZIP compression and ordering break byte similarity; exact input SHA-256 required |
| Patch against a **deterministically extracted/normalized source** generated locally from owned APK | May increase reuse of unchanged asset bytes and reduce patch size; multiple files can be hashed | Adds preprocessing, dependency manifest and disk space; must benchmark actual output and verify that no non-owned source is embedded |
| Deterministic **local conversion script** without binary delta | Easier to audit distributable output, transparency over transforms | Cannot generate proprietary PSP2 Unity player/bundles from Android serialized assets without a functioning licensed editor/loader pipeline |
| Stub VPK + raw portable assets | Smallest possible transformation when assets really are cross-platform | Unity serialized Android assets, shaders and texture formats may not be portable; **requires measured loader support** |

**Recommendation:** once P11 passes, test **direct APK delta** first for user simplicity, then compare archive size, creation time, cross-version risk and release rights against normalized-source and deterministic script approaches. No choice is finalized today.

## 4. Mandatory safeguards before a future public patch

- **Input identity:** verify full SHA-256 `52b7d8754ab8fec75c3b656a0dc5ec8dc849565adc402487a3ac093f6a481f7a`, Android package, versionCode 228, Unity 5.6.4p3. Same reported version without same hash is a **separate untested input**, not automatically compatible.
- **Reproducibility:** output ZIP built deterministically (fixed file list/order, timestamps, compression level and metadata). Record editor, PSP2 module and source build/version. Fix the original source and target **exact bytes** before creating the delta.
- **Patch verification:** decode using both chosen official xdelta build and a clean Windows PC, validate target ZIP SHA-256, ZIP integrity, entry-by-entry destination paths, no executable overwrite, no `../` paths/symlinks and full file completeness.
- **Security/legal:** never publish original APK/OBB, decompiled game code, rebuilt PSP2 Unity runtime, assets, leaked/licensed SDK content, or a binary patch that effectively distributes copyright-protected content without authorization. **A VCDIFF patch is a container, not a copyright exemption.**
- **User experience:** give a simple Windows drag-drop tool only if it passes; report wrong APK, insufficient space, checksum mismatch, patch mismatch, failed ZIP extraction and reinstallation safety. Stage on PC then copy to the exact tested `ux0:app/HGSKE1178/` subtree, not guessed `ux0:data`.
- **Status honesty:** no patch is working until it actually decodes from owned source and the resulting content is successfully loaded by a real Vita installation.

## 5. Source links

**Primary author README / exact releases:**
- [Predators repository](https://github.com/AJ170/Predators-PSVita-Port) / [1.0 release](https://github.com/AJ170/Predators-PSVita-Port/releases/tag/1.0).
- [Pocket Mortys repository](https://github.com/celticg/AS-Pocket-Mortys-vita) / [0.9 release](https://github.com/celticg/AS-Pocket-Mortys-vita/releases/tag/v0.9).

**Tool implementation and format:**
- [Original author's xdelta project](https://github.com/jmacd/xdelta), [CLI reference and -B/-s meanings](https://github.com/jmacd/xdelta/blob/main/site/docs/commandline.md), [BLAKE3 armor](https://github.com/jmacd/xdelta/blob/main/site/docs/armor.md).
- [xdelta 3.2.2](https://github.com/jmacd/xdelta/releases/tag/v3.2.2) and [historical 3.1.0](https://github.com/jmacd/xdelta-gpl/releases/tag/v3.1.0).
- [Unity 2018.2.19f1 editor](https://unity.com/releases/editor/whats-new/2018.2.19f1), [platform-specific AssetBundle FAQ](https://docs.unity3d.com/es/530/Manual/AssetBundleFAQ.html), [UnityDataTools Player build format](https://github.com/Unity-Technologies/UnityDataTools/blob/main/Documentation/playerbuild-format.md).

**Evidence labels:** **VERIFIED:** primary release commands, source extension/version, delta/stub file names and sizes, destination path, Pocket Mortys Unity 2018.2.19f1 statement, xdelta algorithm/version. **INFERRED:** authors' hidden workflow likely precomputed target PSP2 output. **NOT VERIFIED:** exact target ZIP contents, patch build toolchain, xdelta 3.2.2 vs 3.1.0 cross-compatibility on their files, existence of a usable HSE Vita data ZIP.

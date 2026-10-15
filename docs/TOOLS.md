# Toolchain and download checklist — Hungry Shark Evolution PS Vita

**Verified against vendor/GitHub pages on 2026-10-14.** This is the **authoritative tools list**; do not create a second download list. Upstream "latest" releases can change, so record the **actual installed version and SHA-256** in the experiment record. Exact game/editor versions are pinned, irrespective of newer releases.

**Current gate P00:** tools may be downloaded and inventoried, but **P01/P02+ implementation is LOCKED** until [PHASE_GATES.json](PHASE_GATES.json) is correctly advanced after owner approval. Installing software is not proof of game recovery or Vita compatibility.

## 1. Install now: original-engine baseline (P00)

| Item / Windows x64 download | Version (2026-10-14) | Priority | Why and how to confirm |
| --- | --- | --- | --- |
| **[Unity 5.6.4p3 official page](https://unity.com/releases/editor/whats-new/5.6.4p3)** · [direct official Windows x64 installer](https://download.unity3d.com/download_unity/11e8f66cfca7/Windows64EditorInstaller/UnitySetup64-5.6.4p3.exe) | **5.6.4p3 REQUIRED, not latest** (2017-11-20) | **P00 REQUIRED** | Original Android player's Unity version. Install as a separate editor, start directly if Unity Hub's legacy licensing flow fails. Open a **new blank PC project**, enter Play Mode, confirm `Help > About`. |
| **[Python official downloads](https://www.python.org/downloads/release/python-3150/)** | **3.15.0** latest stable (2026-10-09); 3.14.x also suitable | **P00 REQUIRED** | Local stdlib APK extractor and phase validator. Check `py -3 --version` or `python --version`. Modern Python does not determine the Unity scripting runtime. |
| **[7-Zip Windows](https://www.7-zip.org/)** | **26.04** | Recommended | Examine APK (ZIP); our included Python extractor handles `.splitN` reconstruction. `7z i` if CLI added to PATH. |
| **[Git for Windows](https://git-scm.com/install/windows)** | **2.56.0(2)** | **P00 REQUIRED for contributor workflow** | Clone/docs/checkpoints; `git --version`. Do not commit source APK, extracted DLLs or assets. |
| **[Unity Hub](https://unity.com/download)** · [archived editor instructions](https://docs.unity.com/en-us/hub/add-editor) | Current Hub; NOT a replacement for the pinned editors | Optional helper | Manage editor installs. With a Unity 5.x editor, launching its own `Unity.exe` may avoid legacy Hub activation issues. |
| **Android device / original installed 6.4.2** | **Exact 6.4.2 build** | **P00 REQUIRED reference** | Observe baseline menu, gameplay, audio, controls, save and errors on Android. Do not assume the original game remains network-functional. |
| **[Android SDK Platform-Tools (ADB)](https://developer.android.com/tools/releases/platform-tools)** | Latest official platform-tools (rolling download) | Optional P00 | `adb version`, `adb devices`; screenshots and `adb logcat` where available. Install Android Studio **only if** platform-tools prove insufficient. |

**Unity 5.x installation warning:** Unity notes pre-2017.1 Editors do not share Hub sign-in state; in some configurations they may hang at splash if opened from Hub. Launch the original Editor directly and use valid licensing — **never bypass activation**. [Unity legacy install guide](https://docs.unity.cn/2021.1/Documentation/Manual/GettingStartedInstallingHub.html).

## 2. Download/prestage for P01–P04: recover assets and original gameplay

| Tool and verified download | Latest confirmed | Required when | Usage / limitations |
| --- | --- | --- | --- |
| **[AssetRipper releases](https://github.com/AssetRipper/AssetRipper/releases/tag/2.0.0)** · [Windows x64 download selector](https://assetripper.github.io/AssetRipper/articles/Downloads.html) | **2.0.0** stable | **P01 primary tool** | Rebuild Unity project-oriented assets, scenes, MonoScript linkage. Import **all extracted Unity Data files PLUS original `Data/Managed` assemblies** in one import. Capture version/settings/error log. Export does NOT guarantee original working project/source. |
| **[ILSpy releases](https://github.com/icsharpcode/ILSpy/releases/tag/v11.1)** | **11.1** | **P01 essential** | Inspect C# DLLs, signatures, namespaces, serialized fields, decompile only what's required locally. Current desktop/CLI may need [.NET 10 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) according to release package; verify downloaded asset requirements. |
| **[dnSpyEx releases](https://github.com/dnSpyEx/dnSpy/releases/tag/v6.6.0)** · [Windows 64-bit archive](https://github.com/dnSpyEx/dnSpy/releases/download/v6.6.0/dnSpy-net-win64.zip) | **6.6.0** | Strongly recommended | Second perspective on Mono assemblies, CIL and behavior. Debugging original Android Unity runtime is **not** guaranteed just by installing dnSpyEx. Download only from dnSpyEx, not abandoned original forks. |
| **[UABEA releases](https://github.com/nesrak1/UABEA/releases/tag/v8)** | **v8** last stable original branch | Conditional fallback | Inspect/edit individual `.assets` or bundles and dependencies if AssetRipper mismaps. This is **not** the primary project reconstruction tool. Requires [.NET 6 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) and [Visual C++ Redistributable](https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist). |
| **[UABEANext latest Windows nightly](https://nightly.link/nesrak1/UABEANext/workflows/build-windows/master/uabea-windows.zip)** · [source](https://github.com/nesrak1/UABEANext) | **Nightly / no stable GitHub Release** | Fallback only | Experimental successor; save workflow/build SHA, keep v8 separately. Do not confuse nightly with validated stable. |
| **[AssetStudio archived releases](https://github.com/Perfare/AssetStudio/releases/tag/v0.16.47)** | **v0.16.47** last archived release | Fallback only | Extract/view particular textures, models, audio; repo archived/read-only. Not needed when AssetRipper works. |
| **[Unity built-in Console / Profiler / Frame Debugger](https://docs.unity3d.com/560/Documentation/Manual/Profiler.html)** | **Bundled in Unity 5.6** | P02–P04 essential | Compilation errors, allocations, frame timing; capture reproducible scenes and logs. No separate installation. |
| **[Visual Studio Community](https://visualstudio.microsoft.com/vs/community/)** or [Visual Studio Code](https://code.visualstudio.com/Download) | Current IDE (no forced major version) | P02+ recommended | C# editing/diff and debugging; compile using **the project's Unity version**, not modern .NET SDK rules. Install one IDE, not several. Visual Studio's "Game development with Unity" workload may help, but debugger integration with 5.6 is not guaranteed. |

**Mono recovery invariant:** [AssetRipper's Common Issues](https://github.com/AssetRipper/AssetRipper/blob/master/docs/articles/CommonIssues.md) specifies that Mono scripts are in the **assemblies**, not stored directly in asset bundles. Confirm the import log says `Files use the 'Mono' scripting backend`; otherwise stop and correct the import. Reconcile each MonoBehaviour with original assembly/class/namespace, GUID, fileID and serialized fields.

**Do not install Cpp2IL or Il2CppDumper for this 6.4.2 baseline:** this APK has **Mono** managed assemblies, not IL2CPP. These would be relevant only to a separate IL2CPP build (such as 14.5.0), which is NOT the current milestone. Similarly, you do **not** need VitaGL, GL4ES or FalsoJNI when developing through a native **Unity PSP2 player**.

## 3. Compatibility bridge for UnityScript (conditional within P05)

The game contains both `Assembly-UnityScript.dll` and `Assembly-UnityScript-firstpass.dll`. Even after recovered C# works in **5.6.4p3**, migration can fail from language/runtime/API differences.

- **[Official UnityScript-to-C# converter](https://github.com/Unity-Technologies/unityscript2csharp)**; it documents use in **Unity 2017.3 through 2018.1.x**, with **2018.1.x preferred**. This is historical tooling, **not an actively updated modern converter**. It operates on UnityScript **source files**; assembled `.dll` binaries alone are not a drop-in conversion input. Do **not** run it until actual script source recovery and needs are established.
- **[Unity 2018.1 archive](https://unity.com/releases/editor/archive)** — **optional, temporary bridge ONLY IF THE CONVERTER REQUIRES IT**. A bridge editor does not authorize skipping the original Unity 5.6 desktop baseline or the final 2018.2 desktop parity gate.
- Unity's official [2018.2 announcement](https://unity.com/blog/engine-platform/2018-2-now-available) declares the **end of UnityScript support**; the historical compiler removal timeline has nuances, so validate actual behavior in the selected editor rather than assume recovered UnityScript assemblies remain functional.
- If original assemblies resolve but source conversion is impossible, produce an evidence-backed migration plan for equivalent C# behavior and preserve the serialized public API. Owner approval is still required at the current gate.

## Halo 2D reference (verified)

[Halo2D_PsVita](https://github.com/VegettoSan/Halo2D_PsVita) was authored in **Unity 2018.2.19f1**, contains historical PSP2 build cache and has a published VPK. This makes **2018.2.19f1 the preferred target**, replacing the earlier untested 2018.2.21f1 recommendation. The public **[2018.2.19f1 Unity release page](https://unity.com/releases/editor/whats-new/2018.2.19f1)** is the authoritative download location if reinstallation is needed. **Do not transplant cached Unity players, Sony examples, keystones, passwords or private platform files from the Halo project.** See [detailed evidence and limitations](HALO2D_PSP2_REFERENCE.md).

## 4. Reserve for P05–P06: Unity 2018.2 desktop migration

| Tool | Required version / link | Check |
| --- | --- | --- |
| **Unity 2018.2.19f1** | [Official version page](https://unity.com/releases/editor/whats-new/2018.2.19f1) | **Evidence-backed target from [Halo 2D](HALO2D_PSP2_REFERENCE.md) (2018.2.19f1)**, installed **side by side**, not in place of 5.6.4p3. Do not upgrade the only working original project. |
| Built-in Unity compatibility tools | Editor Console, API Updater, UnityYAMLMerge, Profiler, Frame Debugger | Preserve GUID/fileID/MonoBehaviour data; fix C# API changes and scene references with regressions. |
| PSP2 build-target availability check | Official editor build modules / developer installation, when authorized | **Inspect P00**, but formal **build smoke test P07**. An editor installer may omit PSP2, even if the Unity documentation contains `BuildTarget.PSP2`. |

In [Unity 2018.2 scripting restrictions](https://docs.unity.cn/2018.2/Documentation/Manual/ScriptingRestrictions.html), Vita has both Mono and IL2CPP entries and **IL2CPP is AOT**. Verify backend offered by the actual PSP2 module. Audit reflection, generics, dynamic code generation, native plugins and managed runtime differences. **Having Mono on Android does not prove every feature works on PSP2.**

## 5. Android / native-plugin troubleshooting (install only when indicated)

| Tool | Latest confirmed / downloads | Use |
| --- | --- | --- |
| **[JADX](https://github.com/skylot/jadx/releases/tag/v1.5.6)** | **1.5.6** (choose `jadx-gui-1.5.6-with-jre-win.zip` on Windows to avoid manual Java install) | Inspect Android Java/Dex `UnityPlayerActivity`, ads, Google services and custom bridge callbacks. Not a C# decompiler. |
| **[Apktool](https://github.com/iBotPeaches/Apktool/releases/tag/v3.0.3)** · [install guide](https://apktool.org/docs/install/) | **3.0.3**; requires Java | Decode Android manifest/resources/Smali where JADX misses behavior. Not needed simply to unzip APK. |
| **[Ghidra](https://github.com/NationalSecurityAgency/ghidra/releases/tag/Ghidra_12.1.4_build)** | **12.1.4** | Only if evidence shows essential logic trapped in ARM Android `.so` plugins. Native Android ABI code is not the Unity PSP2 runtime; this is a research/adapter task, not direct recompilation. |
| **[Temurin JDK 21](https://adoptium.net/temurin/releases/?version=21)** | Current patched JDK 21 LTS | Java dependency for Ghidra/Apktool/JADX CLI. Do not install separately if a suitable bundled Java runtime already covers the selected tool. |

## PS Vita shader reference archive (for P05–P11; not a P00 dependency)

**[Official Unity 2018.2.19f1 Built-in Shaders ZIP](https://download.unity3d.com/download_unity/06990f28ba00/builtin_shaders-2018.2.19f1.zip)** is the **exact historical Unity 2018.2.19f1 shader-source reference**. Unity [lists a Shaders archive for that editor](https://unity.com/releases/editor/whats-new/2018.2.19f1). Use it to inspect compatible ShaderLab/Cg include patterns; **do not copy all shaders or assume PSP2 performance**. The binary URL was not downloaded/tested during this research. See [the source-backed research and 30/60 FPS test methodology](SHADERS_PSVITA_REFERENCE.md). Shader review is non-binding; gameplay correctness and gate order remain mandatory.

## xdelta3: optional end-user data patch tool (P12, research allowed in P00)

[**xdelta3 v3.2.2 official release**](https://github.com/jmacd/xdelta/releases/tag/v3.2.2), 2026-10-09 · [**Windows x64 direct ZIP**](https://github.com/jmacd/xdelta/releases/download/v3.2.2/xdelta3-3.2.2-windows-x86_64.zip). Maintained Apache-2.0 version. [**xdelta3 3.1.0 historical Windows x64**](https://github.com/jmacd/xdelta-gpl/releases/download/v3.1.0/xdelta3-3.1.0-x86_64.exe.zip) is a fallback for older port patches if needed; test actual compatibility, don't assume. [Detailed Predators/Pocket Mortys case study](XDELTA_UNITY_PORTS_RESEARCH.md).

xdelta3 creates/applies **binary differences** (`xdelta3 -e -s original target.zip patch.xdelta` / `xdelta3 -d -s original patch.xdelta target.zip`), **not** Unity project recovery or Android→PSP2 conversion. Used only after producing a valid target file by other means. The `-B` parameter is **source cache/window bytes**, not a conversion flag. You may download it now, but **do not generate/share a Hungry Shark asset patch** while P12 remains locked. A delta may itself contain protected bytes; review distribution rights.

## 6. Media repair and platform optimization (P02–P11, only as needed)

| Tool | Latest verified / download | Purpose |
| --- | --- | --- |
| **[Blender LTS](https://www.blender.org/download/lts/)** | 5.2.2 LTS (2026-09-15), or 4.5.14 LTS for conservative workflows | Inspect/repair **specific failed** mesh, UV, rig, animation exports. Preserve original importer/model data; not required for mass scene recovery. |
| **[FFmpeg](https://ffmpeg.org/download.html)** | 9.0.2 (2026-09-18) | Inspect/transcode specific incompatible audio/video formats. Save original channels, sample rates and looping semantics. |
| **[Audacity](https://www.audacityteam.org/download/)** | 4.0.1 (2026-09-30) | Optional manual audio inspection/repair, never bulk-convert without tests. |
| **Unity built-in Texture Importer, Profiler, Frame Debugger** | Version matching current Unity editor | Primary fixes for PSP2 texture memory, shaders, draw calls and measured frame timings. |
| **[Vita3K](https://github.com/Vita3K/Vita3K/releases)** | `continuous` build (rolling) | Optional emulator smoke checks; **physical Vita tests remain mandatory**. |
| **[VitaShell](https://github.com/TheOfficialFloW/VitaShell/releases)** | Latest release from maintainer | Local deployment/transfer and log retrieval on a homebrew-enabled Vita where appropriate. |
| **[VitaSDK / vdpm](https://github.com/vitasdk/vdpm)** · [current release-channel documentation](https://vitasdk.org/migration/) | VitaSDK 2026.08 release channel documented | **Optional** for helper homebrew, diagnostics and unrelated loader research. It does **not** replace Unity's proprietary PSP2 player/module or Sony platform development resources. Do not replace an existing working WSL VitaSDK just to follow a newer channel. |

## 7. CRITICAL ACCESS BLOCKER — legitimate Unity PSP2 toolchain (P07)

- **[PlayStation Partners](https://partners.playstation.net/)** is the official channel for platform partner registration and development access.
- Unity's publicly archived **2018.2 Windows editor is not proof** that the associated PSP2 build target, compatible Unity runtime, signing, licensed platform tools and deployment path are installed or obtainable in the current environment.
- Do not download redistributed Sony SDKs, leaked Unity console modules, keys, signatures or other restricted binaries from unofficial sources. Do not ask agents to find such files.
- Record **YES / NO / UNKNOWN** for: PSP2 option visible; matching module installed; target platform dependencies satisfied; authorized build and deployment access. For P00, just inspect/inventory; if UNKNOWN, note it as a major risk. P07 must produce **a real physical-device proof** or remain BLOCKED.
- Some homebrew-developed games use a previously acquired PSP2-capable Unity installation; that does **not** by itself prove the *currently installed* 2018.2.19f1 editor still has a compatible PSP2 module. A different PSP2-capable 2018.2 patch, if proven necessary, requires documented selection and approval before migrating.

## 8. Install order for Windows x64 — minimal duplicate downloads

**P00 (now):** install **Unity 5.6.4p3 → Python → Git → 7-Zip**. Use an existing IDE if available. Verify the editor can create and run a **blank Windows PC project**, document original Android observations. Merely downloading tools does not approve a gate.

**Prestage but don't use out of phase:** AssetRipper 2.0.0, ILSpy 11.1, dnSpyEx 6.6.0 and Unity **2018.2.19f1**, already reported installed and evidenced by [Halo 2D](HALO2D_PSP2_REFERENCE.md); no new editor download needed; optionally store UABEA v8 as a fallback. These are the only extra downloads worth preparing immediately to avoid repetitive agent searches.

**Only if tests require them:** UnityScript converter + Unity 2018.1 bridge; JADX/Apktool/JDK; AssetStudio/UABEANext; Ghidra; Blender/FFmpeg; Vita3K. No Cpp2IL for this Mono version.

Suggested local layout (outside Git, editable to user preference):

```text
HSEVita-Local/
  Installers/                 # trusted installers & archive downloads
  Tools/                      # AssetRipper, ILSpy, dnSpyEx, optional tools
  UnityProjects/
    Original_5.6_RECOVERED/   # never overwrite this source snapshot
    Original_5.6_WORKING/     # original-engine fixes, P02-P04
    Port_2018.2_WORKING/      # create ONLY after P04 approval
  PrivateInputs/              # user-provided APK, NEVER commit
  PrivateExports/             # copyrighted export, NEVER commit
  PrivateLogs/                # full logs; publish only sanitized excerpts
```

## 9. Exact verification commands (no downloads)

Windows terminal:

```powershell
py -3 --version
git --version
7z i
dotnet --info   # only if needed by a downloaded .NET tool
java -version   # only for Java-based tools
Get-FileHash 'path\to\DownloadedTool.zip' -Algorithm SHA256
```

In both Unity editors, record `Help > About` version, install directory and available build modules. In AssetRipper capture **app version, loading log and Mono detection**. In ILSpy open `Assembly-CSharp.dll` and verify game types are readable; do not assume exported source recompiles. Use [`docs/EXPERIMENTS.md`](EXPERIMENTS.md) for per-tool success or failure, and never mark a later phase approved just because the tool launches.

### References and versioning policy

Official Unity [5.6.4p3 notes](https://unity.com/releases/editor/whats-new/5.6.4p3) · [2018.2.19f1 notes](https://unity.com/releases/editor/whats-new/2018.2.19f1) · [AssetRipper import caveats](https://github.com/AssetRipper/AssetRipper/blob/master/docs/articles/CommonIssues.md) · [UnityScript converter's actual support window](https://github.com/Unity-Technologies/unityscript2csharp) · [Unity 2018.2 console/AOT scripting restrictions](https://docs.unity.cn/2018.2/Documentation/Manual/ScriptingRestrictions.html).

**Version fields mean "latest stable verified on 2026-10-14", not an eternal latest.** Follow upstream release pages before downloading later; don't silently replace the pinned Unity versions.

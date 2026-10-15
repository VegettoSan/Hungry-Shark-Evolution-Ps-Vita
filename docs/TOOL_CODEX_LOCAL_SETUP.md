# Tool Codex — complete local Windows x64 download map

**2026-10-14:** verified against author release assets. The cloned repository's folder `Tool Codex/` is **ignored** by Git and is deliberately *not* a GitHub directory. **Unity is installed outside the repo**. Unity 2018.2.19f1 already exists with the user's Halo 2D project; do not download it again. Unity 5.6.4p3 must be installed separately.

**[Technical phases](PORTING_PLAN.md)** stay authoritative. Tool installation ≠ permission to start P01/P05/P12.

## A. Put extracted portable ZIPs directly in `Tool Codex/`

| Folder | Exact tool and official binary download | Verified version | Purpose and priority |
| --- | --- | --- | --- |
| `AssetRipper/` | [AssetRipper_win_x64.zip](https://github.com/AssetRipper/AssetRipper/releases/download/2.0.0/AssetRipper_win_x64.zip) | **2.0.0** | **Primary P01**, asset/project recovery, Mono type mapping |
| `ILSpy/` | [ILSpy_windows_selfcontained_11.1.0.9782-x64.zip](https://github.com/icsharpcode/ILSpy/releases/download/v11.1/ILSpy_windows_selfcontained_11.1.0.9782-x64.zip) | **11.1**, portable self-contained | **Primary P01**, inspect C# managed assemblies; no separate .NET runtime needed |
| `dnSpyEx/` | [dnSpy-net-win64.zip](https://github.com/dnSpyEx/dnSpy/releases/download/v6.6.0/dnSpy-net-win64.zip) | **6.6.0**, Windows x64 bundled runtime | Supplementary assembly inspection, P01–P03 |
| `UABEA/` | [uabea-windows.zip](https://github.com/nesrak1/UABEA/releases/download/v8/uabea-windows.zip) | **v8** | Unity asset editor/viewer, fallback only |
| `AssetStudio/` | [AssetStudio.net472.v0.16.47.zip](https://github.com/Perfare/AssetStudio/releases/download/v0.16.47/AssetStudio.net472.v0.16.47.zip) | **0.16.47** (archived) | Legacy Unity asset viewer, fallback; .NET Framework 4.7.2 |
| `xdelta3/` | [xdelta3-3.2.2-windows-x86_64.zip](https://github.com/jmacd/xdelta/releases/download/v3.2.2/xdelta3-3.2.2-windows-x86_64.zip) | **3.2.2** | Candidate **P12** patch delivery; not Android→PSP2 conversion |
| `UnityScript2CSharp/` | [UnityScript2CSharp_1.0.8052.30790.zip](https://github.com/Unity-Technologies/unityscript2csharp/releases/download/v1.0.8052.30790/UnityScript2CSharp_1.0.8052.30790.zip) | **1.0.8052.30790** | Conditional P05. Converts UnityScript **source**, not .dll binaries |
| `UnityDataTools/` | [UnityDataTool-windows-x64.zip](https://github.com/Unity-Technologies/UnityDataTools/releases/download/v2.4.0/UnityDataTool-windows-x64.zip) | **2.4.0** | Optional inspection of Unity player data. Compatibility with original Unity 5.6 / PSP2 **untested** |
| `JADX/` | [jadx-gui-1.5.6-with-jre-win.zip](https://github.com/skylot/jadx/releases/download/v1.5.6/jadx-gui-1.5.6-with-jre-win.zip) | **1.5.6** | Android Java/Dex plugins; embedded Java runtime |
| `Apktool/` | [apktool_3.0.3.jar](https://github.com/iBotPeaches/Apktool/releases/download/v3.0.3/apktool_3.0.3.jar) | **3.0.3** | Android manifest, Smali/resources; needs installed Java |
| `Ghidra/` | [ghidra_12.1.4_PUBLIC_20260921.zip](https://github.com/NationalSecurityAgency/ghidra/releases/download/Ghidra_12.1.4_build/ghidra_12.1.4_PUBLIC_20260921.zip) | **12.1.4** | Only if native Android plugins are essential; **requires JDK 25 x64**, not JDK 21 |
| `Vita3K/` | [windows-latest.zip](https://github.com/Vita3K/Vita3K/releases/download/continuous/windows-latest.zip) | Continuous / rolling | Optional emulator; physical PS Vita is required to validate the port |
| `xdelta3-legacy/` | [xdelta3-3.1.0-x86_64.exe.zip](https://github.com/jmacd/xdelta-gpl/releases/download/v3.1.0/xdelta3-3.1.0-x86_64.exe.zip) | **3.1.0** | Historical fallback for old port patches; **not required** with our selected 3.2.2 |
| `Shaders-Unity2018/` | [builtin_shaders-2018.2.19f1.zip](https://download.unity3d.com/download_unity/06990f28ba00/builtin_shaders-2018.2.19f1.zip) | Exact Unity 2018.2.19f1 | **Reference shader source, not a program**; future Vita shader research |
| `FFmpeg/` | [Official download](https://ffmpeg.org/download.html) (use linked Windows binary builds) | **9.0.2** | Optional audio/video conversion only when needed |
| `Blender/` | [Blender portable ZIP](https://www.blender.org/download/) | **5.2.2 LTS** | Optional model/animation repair, large download |
| `Audacity/` | [Audacity official download](https://www.audacityteam.org/download/) | Check current build | Optional manual audio repair |

**Do not confuse tool prep with usage:** during P00 inspect only prerequisites. During approved P01, start with AssetRipper + ILSpy. Only run alternative tools if there is a measured or documented reason, to limit Codex model usage.

## B. Installed system-wide and **outside** `Tool Codex`

| Program / location | Official download | Notes |
| --- | --- | --- |
| **Unity 5.6.4p3** (exact pinned original engine) | [Official version](https://unity.com/releases/editor/whats-new/5.6.4p3) · [Windows x64 installer](https://download.unity3d.com/download_unity/11e8f66cfca7/Windows64EditorInstaller/UnitySetup64-5.6.4p3.exe) | Install and activate with legitimate license. Do not put into `Tool Codex` |
| **Unity 2018.2.19f1** | **Already installed by user** | Reuse Halo 2D installation and authorized PSP2 build module; do NOT replace with 2018.2.21 |
| **Python 3.15.0** | [Python official Windows installer](https://www.python.org/downloads/release/python-3150/) | Existing compatible Python 3.14 is fine; no need to replace; `py -3 --version` |
| **Git for Windows** | [git-scm.com Windows](https://git-scm.com/install/windows) | Existing Git installation is fine |
| **7-Zip 26.04** | [7-zip.org official](https://www.7-zip.org/download.html) | Existing 7-Zip installation is fine |
| **VS Code** | [code.visualstudio.com](https://code.visualstudio.com/Download) | Optional if Visual Studio or another editor is already available |
| **Temurin JDK 25 x64** | [Adoptium Temurin 25](https://adoptium.net/temurin/releases/?version=25) | Required **only if using Ghidra 12.1.4**, usable for Apktool; corrected from earlier JDK 21 recommendation |
| **.NET runtimes / Visual C++ redistributable** | [.NET downloads](https://dotnet.microsoft.com/en-us/download/dotnet) · [Microsoft VC++](https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist) | Install ONLY if a specific program complains; .NET 6 legacy versions are out of support |
| **VitaShell** | [VitaShell v2.02 VPK](https://github.com/TheOfficialFloW/VitaShell/releases/download/v2.02/VitaShell.vpk) | On the **PS Vita**, not in Windows `Tool Codex` |
| **VitaSDK / vdpm** | [vdpm v0.1.6](https://github.com/vitasdk/vdpm/releases/tag/v0.1.6) | Already available in WSL; optional native utilities only; **not** Unity PSP2 SDK |
| **Sony / Unity PSP2 tools** | [PlayStation Partners](https://partners.playstation.net/) | Restricted distribution, require authorized access; never put vendor secrets or copyrighted platform binaries in public Git |

Do **not** install Cpp2IL or Il2CppDumper for this **Unity 5.6.4p3 Mono** APK, and do not fetch VitaGL/GL4ES merely to run Unity PSP2.

## C. Recommended local layout

```text
Hungry-Shark-Evolution-Ps-Vita/
  README.md
  AGENTS.md
  .gitignore
  config/
  docs/
  tools/
    check_tool_codex.ps1
  Tool Codex/                      # ignored, LOCAL only
    AssetRipper/
    ILSpy/
    dnSpyEx/
    UABEA/
    AssetStudio/
    xdelta3/
    UnityScript2CSharp/
    UnityDataTools/
    JADX/
    Apktool/
    Ghidra/
    Vita3K/
    xdelta3-legacy/
    Shaders-Unity2018/
    FFmpeg/
    Blender/
    Audacity/
    _tool_inventory.txt           # ignored generated report
```

Extract files, don't leave only the downloaded ZIP. Avoid extra folder nesting. Unity editors are installed normally **outside this folder**. Original **APK, extracted game data, project exports, SDK and proprietary files** belong in a **separate private workspace OUTSIDE the Git clone**.

Git ignore rule is **`/Tool Codex/`**: it excludes the entire root-level tool directory from version control, not just the ZIP archives. Git does not delete ignored files from disk.

## D. Offline validation before launching Codex

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File ".\tools\check_tool_codex.ps1"
git check-ignore -v "Tool Codex\AssetRipper\sample.zip"
py -3 .\tools\check_phase_gates.py
```

The first command creates **`Tool Codex\_tool_inventory.txt`** with detected portable executables and global tools; it performs **no downloads** or third-party code execution. Give Codex that inventory rather than asking it to search or infer tool paths. Its presence alone does not prove CLI/editor compatibility.

### Codex instruction

> Leer primero `docs/TOOL_CODEX_LOCAL_SETUP.md` y `Tool Codex/_tool_inventory.txt`. Reutilizar las herramientas locales de esa carpeta y no buscar ni instalar versiones alternativas sin un error justificado. Reducir logs a lo esencial. Mantener las fases de `docs/PHASE_GATES.json` y no publicar activos propietarios del APK.

Tool versions and direct GitHub asset filenames were verified 2026-10-14. Windows ZIP tools were not downloaded or executed here; check extracted executables on the actual machine before claiming PASS.

<div align="center">

# 🦈 Hungry Shark Evolution Vita

### The ocean is yours. Now on PlayStation Vita.

An unofficial native PlayStation Vita port of **Hungry Shark Evolution**, based on the original Android version **6.4.2 (build 228)**.

[![Release](https://img.shields.io/badge/Release-1.0-00b8b5?style=for-the-badge)](https://github.com/VegettoSan/Hungry-Shark-Evolution-Ps-Vita/releases/tag/01.00)
![Platform](https://img.shields.io/badge/Platform-PlayStation%20Vita-1261a0?style=for-the-badge)
![Game](https://img.shields.io/badge/Game-Hungry%20Shark%20Evolution-183850?style=for-the-badge)

**[Download v1.0](https://github.com/VegettoSan/Hungry-Shark-Evolution-Ps-Vita/releases/tag/01.00) · [Installation](#-installation) · [Controls](#-controls) · [Troubleshooting](#-troubleshooting) · [Report an Issue](https://github.com/VegettoSan/Hungry-Shark-Evolution-Ps-Vita/issues/new/choose)**

</div>

---

## 🌊 About the Port

Swim, hunt, survive, and evolve on **PlayStation Vita**. This port brings the classic *Hungry Shark Evolution* experience to the handheld, with Vita controls and touchscreen support.

| Feature | Description |
| :-- | :-- |
| 🦈 **Original gameplay** | Explore the ocean, eat prey, complete missions, and evolve your sharks |
| 🎮 **Vita controls** | Analog stick, D-pad, face buttons, and touchscreen |
| 🔊 **Audio** | In-game music and sound effects |
| 💾 **Progress** | Save and load your game |
| 🐋 **Shark selection** | Evolution menus and special sharks |
| 📱 **LiveArea** | Custom PlayStation Vita LiveArea artwork |

**Port version:** `1.0` &nbsp;·&nbsp; **Title ID:** `HGSKE1178` &nbsp;·&nbsp; **Source version:** Android `6.4.2 (228)`

## 📦 Download

### [⬇️ Download Hungry Shark Evolution Vita v1.0](https://github.com/VegettoSan/Hungry-Shark-Evolution-Ps-Vita/releases/download/01.00/HSEVita_v1.0_Windows.zip)

**Release page:** [Hungry Shark Evolution 01.00](https://github.com/VegettoSan/Hungry-Shark-Evolution-Ps-Vita/releases/tag/01.00)

The download contains a **Windows VPK reconstruction tool**. Use it with your own unmodified copy of the supported Android APK to generate the complete PS Vita VPK.

> [!NOTE]
> The original Android APK and the fully reconstructed VPK are **not included**. The release package requires your own exact copy of *Hungry Shark Evolution 6.4.2 (build 228)*. You do **not** need to install Unity, Python, or a console SDK.

<details>
<summary><strong>Verify the downloaded ZIP (SHA-256)</strong></summary>

```text
89aea8f6747101b0b2bffcdca2bcdfff5d29786d36f63fa7c2867aba05a1821a
```

File: `HSEVita_v1.0_Windows.zip`

On Windows, check it with:

```powershell
Get-FileHash ".\HSEVita_v1.0_Windows.zip" -Algorithm SHA256
```

</details>

## 📋 Requirements

| | Requirement |
| :-- | :-- |
| **Console** | PlayStation Vita able to install VPK files using VitaShell |
| **Required Vita plugin** | **[CapUnlocker](https://github.com/GrapheneCt/CapUnlocker)** (`ur0:tai/CapUnlocker.skprx`) — must be enabled before launching the game |
| **Computer** | 64-bit Windows |
| **Runtime** | .NET Framework 4.8 |
| **Disk space** | At least 2 GB of free space for reconstruction |
| **Game files** | An unmodified **Hungry Shark Evolution 6.4.2 (build 228)** Android APK |

> [!IMPORTANT]
> **CapUnlocker is required to run v1.0.** Without it, the game may open and then immediately close. Install `CapUnlocker.skprx` to `ur0:tai/`, add `ur0:tai/CapUnlocker.skprx` under `*KERNEL` in the active taiHEN `config.txt`, then **fully reboot your PS Vita**. See the [CapUnlocker setup guide](docs/CAPUNLOCKER_SETUP.md) for details. This also applies to the complete game installed from **PSVitaAlive Store**.

<details>
<summary><strong>How to verify your APK</strong></summary>

Only the exact supported APK will work.

| Property | Expected value |
| :-- | :-- |
| Android package | `com.fgol.HungrySharkEvolution` |
| Version | `6.4.2` |
| Build / versionCode | `228` |
| File size | `104296014` bytes |
| SHA-256 | `52b7d8754ab8fec75c3b656a0dc5ec8dc849565adc402487a3ac093f6a481f7a` |

Run this in PowerShell, replacing the file path:

```powershell
Get-FileHash "C:\Games\HungrySharkEvolution.apk" -Algorithm SHA256
```

Different or modified APKs are not supported, even if they show the same version number.

</details>

## 🚀 Installation

1. **Download** [`HSEVita_v1.0_Windows.zip`](https://github.com/VegettoSan/Hungry-Shark-Evolution-Ps-Vita/releases/download/01.00/HSEVita_v1.0_Windows.zip) from the [v1.0 release](https://github.com/VegettoSan/Hungry-Shark-Evolution-Ps-Vita/releases/tag/01.00).
2. **Extract the entire ZIP** into a writable folder on your Windows PC. Keep the included files together.
3. **Drag your original Hungry Shark Evolution 6.4.2 (228) APK** onto `HSERebuild.exe`. Alternatively, open the program and select the APK.
4. **Wait for `PASS`**. The tool checks the input and reconstructed content, then creates `HungrySharkEvolutionVita_1.0.vpk` next to your APK.
5. **Transfer the generated VPK** to your PlayStation Vita and install it with **VitaShell**.
6. **Confirm CapUnlocker is installed and enabled**, then reboot your PS Vita if you just configured the plugin. Launch the game from LiveArea.

> [!IMPORTANT]
> **Install the completed `HungrySharkEvolutionVita_1.0.vpk` only.** Do not install `HSEVita_Stub.vpk` on its own: it is an incomplete package. **Back up your saved progress before updating or reinstalling.**

### Optional: Choose a custom output location

Open PowerShell in the extracted release folder:

```powershell
.\HSERebuild.exe "C:\Games\HSE_6.4.2.apk" --out "C:\Output\HungrySharkEvolutionVita_1.0.vpk"
```

The tool will not overwrite an existing output file. For additional details, see the [complete installation guide](docs/RELEASE_V1_0_README_ES_EN.md#english).

## 🎮 Controls

| PS Vita input | Action |
| :--: | :-- |
| **Left analog stick** | Move your shark / navigate menus |
| **D-pad** | Alternative movement / menu navigation |
| **R (hold)** | Boost |
| **✕ (Cross)** | Confirm / select |
| **○ (Circle)** | Back / cancel |
| **□ (Square)** | Toggle missions |
| **△ (Triangle)** | Toggle map |
| **START** | Pause / resume |
| **Touchscreen** | Touch controls and on-screen interactions |

Boost uses the game's energy system. The map and mission functions are available when supported by the current game state.

**Required vs. optional:** CapUnlocker is a **startup requirement**; overclocking via PSVshell is **optional** and recommended only for smoother performance.

## ⚡ Performance Recommendation

For **smoother gameplay and more consistent frame rates**, **overclocking your PS Vita is recommended**, especially in more demanding areas of the game.

A PS Vita clock-control plugin, such as **PSVshell**, can be used to adjust performance settings. Results may vary depending on the scene and your console. Higher clock speeds can increase battery consumption and heat, so use stable settings appropriate for your device.

## 🛠️ Troubleshooting

| Problem | Solution |
| :-- | :-- |
| **The APK is rejected** | Verify the exact version, file size, and SHA-256 shown above. |
| **A required file is missing** | Extract the complete release ZIP again, keeping its files together. |
| **Checksum verification fails** | Re-extract the original download and check its SHA-256. |
| **The VPK cannot be created** | Check for available disk space and write permissions. |
| **An output VPK already exists** | Move your previous VPK or choose another name using `--out`. |
| **Game opens and immediately closes** | Make sure **CapUnlocker** is installed at `ur0:tai/CapUnlocker.skprx`, loaded under `*KERNEL` in the active taiHEN configuration, and that the Vita has been rebooted. See the [setup guide](docs/CAPUNLOCKER_SETUP.md). |
| **The game does not launch** | Confirm that you installed the completed VPK rather than `HSEVita_Stub.vpk`, and check the required CapUnlocker plugin. |

Need help? [**Open an issue**](https://github.com/VegettoSan/Hungry-Shark-Evolution-Ps-Vita/issues/new/choose) with your Vita model, firmware, a short description, and steps to reproduce the problem. **Do not upload APKs, VPKs, proprietary game files, or personal information.**

---

## ❤️ Credits

| Contribution | Contributor |
| :-- | :-- |
| **PS Vita port** | [VegettoSan](https://github.com/VegettoSan) |
| **LiveArea artwork** | [BetterWinds](https://github.com/BetterWinds) |
| **xdelta3 utility** | [Joshua MacDonald](https://github.com/jmacd/xdelta) |

*Hungry Shark Evolution* and its original characters, assets, and trademarks belong to their respective rights holders. This is an **unofficial, independent community project** and is not affiliated with or endorsed by Ubisoft or the original developers. Use only lawfully obtained game copies and respect applicable licenses.

<div align="center">

---

**Hungry Shark Evolution Vita · Version 1.0**

[**Download**](https://github.com/VegettoSan/Hungry-Shark-Evolution-Ps-Vita/releases/tag/01.00) · [**Report an Issue**](https://github.com/VegettoSan/Hungry-Shark-Evolution-Ps-Vita/issues) · [**Installation Guide**](docs/RELEASE_V1_0_README_ES_EN.md#english)

</div>

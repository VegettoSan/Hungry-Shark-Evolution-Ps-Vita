# Hungry Shark Evolution Vita — Version 1.0

The first public release of **Hungry Shark Evolution Vita**, an unofficial native PlayStation Vita port based on the original **Hungry Shark Evolution 6.4.2 (Android build 228)**.

Swim, hunt, evolve, and explore the ocean with adapted PS Vita controls, touchscreen support, original gameplay, audio, and save/load functionality.

### What's included

- Native PlayStation Vita version with original shark gameplay and progression
- Shark selection, evolution menus, and special sharks
- Analog stick, D-pad, button controls, and touchscreen support
- Game audio and save/load functionality
- Custom PS Vita LiveArea presentation, with artwork by [BetterWinds](https://github.com/BetterWinds)
- Windows reconstruction utility to generate your VPK from your own supported Android APK

### Requirements

- A PlayStation Vita able to install VPKs through VitaShell
- **Required kernel plugin: [CapUnlocker](https://github.com/GrapheneCt/CapUnlocker)** installed and active as `ur0:tai/CapUnlocker.skprx` under `*KERNEL`
- **Windows 64-bit** with **.NET Framework 4.8**
- **At least 2 GB of free disk space**
- Your own **unmodified Hungry Shark Evolution 6.4.2 (build 228) APK**

**Required APK SHA-256:**
```text
52b7d8754ab8fec75c3b656a0dc5ec8dc849565adc402487a3ac093f6a481f7a
```

**Required APK size:** `104296014` bytes.

### Required PS Vita Plugin — CapUnlocker

**CapUnlocker is mandatory for Hungry Shark Evolution Vita v1.0.** Without it, the game may launch and immediately close.

1. Download [CapUnlocker](https://github.com/GrapheneCt/CapUnlocker) from its official project and copy `CapUnlocker.skprx` to `ur0:tai/` on your Vita.
2. In the active taiHEN configuration (`ur0:tai/config.txt` on the standard setup), ensure the plugin is listed under the existing `*KERNEL` section:

   ```ini
   *KERNEL
   ur0:tai/CapUnlocker.skprx
   ```

3. Save the configuration and **fully reboot** the console before launching the game.

The plugin is also required if you install the complete game from **PSVitaAlive Store**. **Overclocking with PSVshell is recommended for smoother gameplay but is not mandatory.** See the [full setup guide](https://github.com/VegettoSan/Hungry-Shark-Evolution-Ps-Vita/blob/main/docs/CAPUNLOCKER_SETUP.md).

### How to install

1. Download **`HSEVita_v1.0_Windows.zip`** from the Assets section below.
2. Extract **all files** into a writable folder on your Windows PC.
3. Drag your original **6.4.2 (228)** APK onto **`HSERebuild.exe`**, or open the program and select it.
4. Wait for the **`PASS`** message. The tool generates **`HungrySharkEvolutionVita_1.0.vpk`** next to your APK.
5. Transfer the completed VPK to your Vita and install it using **VitaShell**.
6. Confirm **CapUnlocker is enabled**, reboot the Vita if needed, and launch the game.

> [!IMPORTANT]
> **Do not install `HSEVita_Stub.vpk`**. It is an incomplete base used by the reconstruction process. Install **only the finished VPK** produced by the utility. Back up your save data before updating or reinstalling.

The download **does not include the original APK or a complete ready-to-install VPK**. You do not need to install Python, Unity, or any SDK.

### PS Vita controls

| Input | Action |
| :-- | :-- |
| Left stick / D-pad | Move your shark and navigate menus |
| Hold **R** | Boost |
| **✕** | Confirm / select |
| **○** | Back / cancel |
| **□** | Missions |
| **△** | Map |
| **START** | Pause / resume |
| Touchscreen | Touch controls and on-screen interactions |

### File verification

**Release ZIP SHA-256:**
```text
89aea8f6747101b0b2bffcdca2bcdfff5d29786d36f63fa7c2867aba05a1821a
```

**Expected reconstructed VPK SHA-256:**
```text
03f752c91d936765c20d9b7494deae319486763f4bf4f4005dbff45353fe2f9b
```

If the game opens and then closes, first check that **CapUnlocker.skprx** is enabled in the active `*KERNEL` section and reboot the Vita. If your APK is rejected, check the exact version and hash. If a component is missing or verification fails, extract the entire ZIP again. The reconstruction tool will not overwrite an existing output VPK.

### Credits

- **Port:** [VegettoSan](https://github.com/VegettoSan)
- **LiveArea artwork:** [BetterWinds](https://github.com/BetterWinds)
- **xdelta3:** [Joshua MacDonald](https://github.com/jmacd/xdelta)

For more information, see the [README](https://github.com/VegettoSan/Hungry-Shark-Evolution-Ps-Vita#readme) or [report an issue](https://github.com/VegettoSan/Hungry-Shark-Evolution-Ps-Vita/issues/new/choose).

*Hungry Shark Evolution and its original content belong to their respective rights holders. This unofficial community port is not affiliated with or endorsed by Ubisoft or the original developers.*

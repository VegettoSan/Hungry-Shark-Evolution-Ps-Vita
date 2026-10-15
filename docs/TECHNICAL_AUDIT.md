# APK technical audit — Hungry Shark Evolution 6.4.2

**Audit date:** 2026-10-14. **Evidence type:** direct static examination of the user-supplied APK and managed assembly metadata. **No Vita build/runtime test.**

## Reproducible input

| Field | Value |
| --- | --- |
| Android package | `com.fgol.HungrySharkEvolution` |
| APK versionName / versionCode | `6.4.2` / `228` |
| Android minSdk / targetSdk | 16 / 28 |
| APK size | 104,296,014 bytes |
| SHA-256 | `52b7d8754ab8fec75c3b656a0dc5ec8dc849565adc402487a3ac093f6a481f7a` |
| ZIP CRC check | PASS |
| OpenGL ES requirement | 2.0 (Android manifest) |
| Unity version | **5.6.4p3** |
| Version evidence | `assets/bin/Data/globalgamemanagers` and `level0`, Unity serialized-file header |
| Backend evidence | `lib/armeabi-v7a/libmono.so`; `assets/bin/Data/Managed/*.dll`; no `libil2cpp.so` |
| OBB inside APK | Not present |

This SHA identifies **this exact sample**; a different APK with the same version string may differ.

## Contents and recovery implications

- APK archive: **3,073 entries**, approximately **350,933,570 bytes uncompressed**.
- `assets/bin/Data/`: **2,366 entries**, **311,224,301 bytes** uncompressed. This is **disk data, not runtime RAM usage**.
- `Data/Managed/`: **21 files**, **9,338,880 bytes**.
- Scene serialized files: `level0`–`level17`. Do not equate this count with playable levels.
- **86** `.splitN` fragments belong to **15** logical files; reconstruct in **numeric** order, not lexical order.

### Principal managed assemblies

| Assembly | Size (bytes) | TypeDef | MethodDef | Methods with RVA |
| --- | ---: | ---: | ---: | ---: |
| `Assembly-CSharp.dll` | 4,958,208 | 3,798 | 31,671 | 27,367 |
| `Assembly-CSharp-firstpass.dll` | 237,568 | 219 | 1,731 | 1,636 |
| `Assembly-UnityScript.dll` | 19,456 | 24 | 95 | 95 |
| `Assembly-UnityScript-firstpass.dll` | 46,080 | 44 | 139 | 139 |

Type/Method counts include third-party libraries and generated code; **not** a count of custom gameplay scripts. RVA counts show method bodies in the CLR metadata; they do **not** imply source recovery or clean recompilation.

Examples of observed types: `GameLogic`, `SharkStats`, `GameCamera`, `SpawnerManager`, `ShopManager`, `FGOL.Save.SaveGameManager`, `FGOLControllerMapper`, `SoundPlayer`. Assemblies have not yet been proven compatible with Unity 2018.2. Legacy UnityScript is an explicit migration risk.

### Native Android libraries (`armeabi-v7a`)

| Library | Size (bytes) |
| --- | ---: |
| `libmain.so` | 19,404 |
| `libgpg.so` | 1,684,372 |
| `libunity.so` | 18,275,088 |
| `libubiservices.so` | 3,876,440 |
| `libadcolony.so` | 38,244 |
| `libmono.so` | 3,774,904 |
| `libjs.so` | 935,488 |

Despite ARMv7, `libunity.so` depends on Android APIs/libraries (including `libandroid.so`, `libEGL.so` and `libGLESv2.so`). The Vita-native route instead requires the PSP2 Unity player and corresponding platform code.

## Comparison with later Android version

An independently examined 14.5.0 XAPK uses **Unity 6000.0.68f1 + IL2CPP**, with `libil2cpp.so` and metadata rather than the comparable Mono gameplay assemblies. It is retained as a research reference, **not** this repo's initial source baseline.

## Confirmed vs unconfirmed

**Verified in static audit:** APK integrity, package/version, Unity version, Mono/assemblies, file inventory, fragment layout, key managed metadata, native library list. The local Python extractor was exercised against the supplied sample and produced a reconstructed `Data/` directory.

**Not yet tested:** AssetRipper export fidelity, scene and MonoBehaviour GUID resolution, recovered code compilation, UnityScript migration, offline service isolation, Unity 2018.2 PSP2 build, Vita FPS/memory, actual gameplay. No VPK exists.

Source APK bytes and the full asset inventory are **intentionally not committed**. Use the local tool and compare your sample hash.

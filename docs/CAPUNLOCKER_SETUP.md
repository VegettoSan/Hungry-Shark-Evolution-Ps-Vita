# Required PS Vita plugin: CapUnlocker (Hungry Shark Evolution Vita v1.0)

**Status: REQUIRED for the current v1.0 build.** The port owner observed that the game starts and then closes shortly afterwards **without CapUnlocker**. With the plugin enabled, the game launches. This is an **owner-reported hardware dependency**, not a diagnosis of the exact engine function causing the crash.

CapUnlocker is distinct from an overclock plugin: **CapUnlocker is required to launch; PSVshell/overclocking is optional and only recommended for smoother gameplay.** The requirement applies to both the GitHub reconstructed VPK and the complete game installed from **PSVitaAlive Store**.

## Download

Official upstream project: **[GrapheneCt/CapUnlocker](https://github.com/GrapheneCt/CapUnlocker)** (MIT license).

CapUnlocker unlocks additional game-app capabilities on PS Vita, including the fourth CPU core, certain virtual machine thread-manager functions (`ForVM` / `ForMono`), and thread attributes. These capabilities are a **possible explanation** for the port's dependency, but the exact reason has not been isolated.

## Installation (taiHEN)

1. Obtain `CapUnlocker.skprx` from the official upstream project or a trusted plugin manager. Do not rename the file.
2. Open VitaShell, copy the plugin to **`ur0:tai/CapUnlocker.skprx`**.
3. Identify which taiHEN `config.txt` is actually active on your Vita (typically `ur0:tai/config.txt`; some installations prioritize an existing `ux0:tai/config.txt`). Back up that configuration before editing it.
4. Under the existing **`*KERNEL`** section, add the following line once, leaving all your other plugin entries intact:

   ```ini
   *KERNEL
   ur0:tai/CapUnlocker.skprx
   ```

5. Save `config.txt` and **fully reboot** the Vita. Re-enable your homebrew environment as appropriate for your setup.
6. Start **Hungry Shark Evolution Vita** from LiveArea.

**Do not create a second `*KERNEL` header** if it already exists. Add only the plugin path beneath that header.

## Troubleshooting

**Symptom:** The game opens and closes shortly afterwards, returning to LiveArea.

- Check that `ur0:tai/CapUnlocker.skprx` actually exists, is not a zero-byte file, and matches the configured path.
- Confirm `ur0:tai/CapUnlocker.skprx` appears under `*KERNEL` in the **active** taiHEN configuration. If `ux0:tai/config.txt` is active, changing only `ur0:tai/config.txt` might have no effect.
- Perform a **full reboot**; merely closing the game or refreshing LiveArea is not enough for a kernel plugin.
- Try again. If the issue persists with CapUnlocker loaded, report your Vita model, firmware, installed plugins and reproduction steps at [GitHub Issues](https://github.com/VegettoSan/Hungry-Shark-Evolution-Ps-Vita/issues/new/choose). Do **not** post private paths, APK/VPK files, secrets or save data.

**Developer note:** To determine precisely why this port requires CapUnlocker, compare sanitized startup logs with and without the plugin enabled on real hardware. Do not assume that a particular Mono API, core affinity call or thread attribute is the cause without a matching error trace. No game/runtime binary changes are included with this documentation.

---

## Español: requisito e instalación rápida

**Hungry Shark Evolution Vita v1.0 necesita CapUnlocker para iniciar correctamente.** Sin él, el propietario observó que el juego abre y se cierra enseguida. Este requisito también se aplica a quienes instalaron el juego completo desde **PSVitaAlive Store**.

1. Consigue `CapUnlocker.skprx` del [repositorio oficial](https://github.com/GrapheneCt/CapUnlocker) o de un gestor de plugins confiable.
2. Copia el archivo a `ur0:tai/CapUnlocker.skprx` mediante VitaShell.
3. Haz una copia de seguridad del `config.txt` **activo** de taiHEN y añade `ur0:tai/CapUnlocker.skprx` debajo de la sección `*KERNEL`, sin borrar el resto de entradas.
4. **Reinicia completamente la consola** y vuelve a abrir el juego.

**Importante:** CapUnlocker es **obligatorio**; el overclock mediante PSVshell sigue siendo **opcional** y solo se recomienda para mejorar la fluidez.

Fuentes: [CapUnlocker upstream](https://github.com/GrapheneCt/CapUnlocker) · [PS Vita plugin installation](https://consolemods.org/wiki/Vita:Installing_Plugins).

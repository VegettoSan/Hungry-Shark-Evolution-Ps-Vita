# Hungry Shark Evolution Vita 1.0

## Español

Este paquete reconstruye el VPK en tu PC usando tu propio APK de **Hungry Shark
Evolution 6.4.2, build 228**. No necesitas instalar Python, Unity ni un SDK.

Necesitas Windows de 64 bits con .NET Framework 4.8 y al menos **2 GB libres**.
El APK exacto tiene **104296014 bytes** y este SHA-256:

```text
52b7d8754ab8fec75c3b656a0dc5ec8dc849565adc402487a3ac093f6a481f7a
```

Una versión diferente o modificada no se acepta aunque indique 6.4.2. La
herramienta no descarga el juego. El APK y el VPK completo no están incluidos.

**Plugin obligatorio de PS Vita:** instala [CapUnlocker](https://github.com/GrapheneCt/CapUnlocker), coloca `CapUnlocker.skprx` en `ur0:tai/`, añade `ur0:tai/CapUnlocker.skprx` bajo `*KERNEL` en el `config.txt` activo de taiHEN y **reinicia completamente la consola**. Sin el plugin, se ha observado que el juego abre y se cierra enseguida. También es obligatorio si instalaste el juego completo desde **PSVitaAlive Store**. Consulta la [guía del plugin](CAPUNLOCKER_SETUP.md). El overclock es opcional.

1. Extrae **todo el ZIP** en una carpeta local escribible.
2. Mantén juntos el EXE, decoder, base, parche y manifiesto. **No instales
   `HSEVita_Stub.vpk`: es una base incompleta.**
3. Arrastra tu APK sobre `HSERebuild.exe` o abre el EXE y selecciona el APK.
4. Espera a que aparezca **PASS**. La salida predeterminada es
   `HungrySharkEvolutionVita_1.0.vpk`, junto a tu APK. La herramienta verifica
   los componentes y todos los archivos internos antes de guardar el resultado.
5. Respalda tu progreso antes de actualizar. Transfiere el VPK completo
   reconstruido a la Vita e instálalo con VitaShell. Title ID: **HGSKE1178**.
   La ubicación estándar de instalación es `ux0:app/HGSKE1178/`.

Si quieres elegir otra ubicación o nombre de salida, abre PowerShell en la carpeta
del paquete y ejecuta:

```powershell
.\HSERebuild.exe "C:\MisJuegos\HSE_6.4.2.apk" --out "C:\Salida\HSEVita_1.0.vpk"
```

No reemplaza archivos existentes. Si el destino ya existe, elige otro nombre.
No copies los archivos de datos Android sobre la instalación PSP2.

El SHA-256 del VPK reconstruido con el EXE incluido es:

```text
03f752c91d936765c20d9b7494deae319486763f4bf4f4005dbff45353fe2f9b
```

Para comprobar un archivo del paquete, usa `Get-FileHash -Algorithm SHA256` y
compara con `SHA256SUMS.txt`.

**Problemas habituales**

- El juego abre y se cierra: comprueba que **CapUnlocker** esté cargado bajo `*KERNEL` y reinicia la PS Vita. Comprueba también cuál es el `config.txt` activo.
- APK incorrecto: comprueba el tamaño y SHA-256 indicados arriba.
- Falta un componente o no coincide su checksum: extrae otra vez todo el ZIP.
- Decoder no reconocido: conserva el `xdelta3decode.exe` incluido; no lo sustituyas.
- Espacio insuficiente o acceso denegado: usa una carpeta escribible en otro disco.
- Salida existente: cambia `--out` o mueve tu VPK anterior sin borrarlo.
- Ctrl+C cancela y limpia el temporal. Un corte eléctrico o cierre forzado puede
  dejar una carpeta `.hse-rebuild-*` con `HSE_TEMP.txt` junto al destino. Elimina
  sólo esa carpeta cuando hayas confirmado que la herramienta terminó.

## English

This package reconstructs the VPK on your PC from your own **Hungry Shark Evolution
6.4.2, build 228** APK. No Python, Unity or SDK installation is required.

Requirements: 64-bit Windows with .NET Framework 4.8 and **2 GB free disk space**.
The exact APK is **104296014 bytes**, SHA-256:

```text
52b7d8754ab8fec75c3b656a0dc5ec8dc849565adc402487a3ac093f6a481f7a
```

Other or modified APKs are rejected, even if they report version 6.4.2. The tool
does not download the game. The APK and complete VPK are not included.

**Required PS Vita plugin:** install [CapUnlocker](https://github.com/GrapheneCt/CapUnlocker). Copy `CapUnlocker.skprx` to `ur0:tai/`, add `ur0:tai/CapUnlocker.skprx` under `*KERNEL` in the active taiHEN `config.txt`, and **fully reboot the Vita**. Without this plugin, the game has been observed to launch and then close. This also applies to complete installs from **PSVitaAlive Store**. See the [plugin setup guide](CAPUNLOCKER_SETUP.md). Overclocking is optional.

1. Extract the **entire ZIP** into a writable local folder.
2. Keep the EXE, decoder, base, patch and manifest together. **Do not install
   `HSEVita_Stub.vpk`: it is an incomplete base.**
3. Drag your APK onto `HSERebuild.exe`, or launch the EXE and select the APK.
4. Wait for **PASS**. Default output: `HungrySharkEvolutionVita_1.0.vpk` beside
   your APK. Components and every internal member are checked before completion.
5. Back up progress before updating. Transfer the reconstructed complete VPK to
   the Vita and install it using VitaShell. Title ID: **HGSKE1178**. Standard app
   install location: `ux0:app/HGSKE1178/`.

To choose another output path, open PowerShell in the package directory:

```powershell
.\HSERebuild.exe "C:\MyGames\HSE_6.4.2.apk" --out "C:\Output\HSEVita_1.0.vpk"
```

Existing output files are never replaced; choose another name. Do not overwrite
the PSP2 installation with Android data files.

The VPK reconstructed by the included EXE has SHA-256:

```text
03f752c91d936765c20d9b7494deae319486763f4bf4f4005dbff45353fe2f9b
```

Use `Get-FileHash -Algorithm SHA256` and compare with `SHA256SUMS.txt` to check
package components.

**Troubleshooting**

- Game opens and immediately closes: confirm that **CapUnlocker** is actually loaded from `*KERNEL`, then reboot and try again. Check which taiHEN configuration is active.
- Wrong APK: compare its size and full SHA-256 above.
- Missing component/checksum mismatch: extract the complete ZIP again.
- Unknown decoder: keep the included pinned `xdelta3decode.exe`.
- Low space/access denied: choose a writable folder on another disk.
- Output already exists: change `--out` or move your previous VPK without deleting it.
- Ctrl+C cancels and cleans temporary files. Power loss/forced termination can
  leave a `.hse-rebuild-*` sibling containing `HSE_TEMP.txt`. Remove only that
  directory after confirming the tool has stopped.

## Controles de PS Vita / PS Vita controls

| Control | Español | English |
| --- | --- | --- |
| Stick izquierdo / Left stick | Mover el tiburón; navegar por menús | Move the shark; navigate menus |
| Cruceta / D-pad | Movimiento alternativo; navegar y cambiar selección | Alternative movement; navigate and change selection |
| R, mantenido / Hold R | Turbo durante la partida | Boost during gameplay |
| START | Pausar/reanudar | Pause/resume |
| ✕ / Cross | Confirmar o activar la opción seleccionada | Confirm or activate the selected option |
| ○ / Circle | Volver/cancelar | Back/cancel |
| □ / Square | Abrir/cerrar misiones durante la partida | Toggle missions during gameplay |
| △ / Triangle | Abrir/cerrar mapa durante la partida | Toggle map during gameplay |
| Pantalla táctil / Touchscreen | Controles táctiles originales y botones en pantalla | Original touch controls and on-screen buttons |

Misiones y mapa dependen de su disponibilidad en el estado actual del juego.
El turbo consume la energía original; suelta R para dejar de solicitarlo.
Mientras hay un toque activo, se da prioridad a la entrada táctil.

Missions and map depend on availability in the current game state. Boost uses the
original energy system; release R to stop requesting it. Active touches take
priority over controller movement.

## Credits / Créditos

Port: **VegettoSan**. LiveArea artwork: **[BetterWinds](https://github.com/BetterWinds)**.
Hungry Shark Evolution and its original content belong to their respective rights
holders. xdelta3 by Joshua MacDonald, version 3.2.2; see `xdelta3-LICENSE.txt` and
`THIRD_PARTY_NOTICES.txt`.

# Hungry Shark Evolution Vita 1.0 — ES / EN (borrador privado)

**Candidato privado; no publicado.** El propietario confirma permisos y matriz
física PASS. Queda pendiente excluir las rutas privadas encontradas en la base.
No redistribuir base, parche, APK ni VPK. Ver [expediente](RELEASE_V1_0_PREPARATION_2026-10-23.md).

## Español

**Requisito añadido después de las pruebas originales:** el propietario comprobó que el port v1.0 necesita **CapUnlocker** (`ur0:tai/CapUnlocker.skprx`) cargado bajo `*KERNEL` en la configuración activa de taiHEN. **Sin el plugin, el juego abre y se cierra.** Instálalo, reinicia completamente la consola y consulta la [guía de configuración](CAPUNLOCKER_SETUP.md). Esto también afecta a la instalación completa desde PSVitaAlive Store; el overclock es opcional.


Usa tu propio APK **Hungry Shark Evolution 6.4.2, build228**, 104296014 bytes:

```text
SHA-256: 52b7d8754ab8fec75c3b656a0dc5ec8dc849565adc402487a3ac093f6a481f7a
```

APKs modificados u otra versión se rechazan. La herramienta no descarga el juego.
Windows x64/.NET Framework4.8, sin instalar Python, Unity o SDK. Pruebas automáticas
en Windows familia build26200; Windows10 y PC limpio pendientes. Reserva 2GB libres.

1. Extrae todo el paquete a una carpeta local escribible. Mantén juntos
   `HSERebuild.exe`, `xdelta3decode.exe`, `HSEVita_Stub.vpk`, `HSEVita_Data.xdelta`
   y `HSEVita_Release.json`. **No instales la base incompleta.**
2. Comprueba `SHA256SUMS.txt` con PowerShell `Get-FileHash -Algorithm SHA256`.
   En una futura Release coteja con los hashes publicados por el propietario;
   un checksum no acredita derechos.
3. Arrastra el APK al EXE o abre el EXE para elegirlo. Interfaces implementadas,
   recorrido manual pendiente. Alternativa automatizada probada:

   ```powershell
   .\HSERebuild.exe "C:\MisJuegos\HSE_6.4.2.apk" --out "C:\Salida\HSEVita_1.0.vpk"
   ```

4. Salida predeterminada: `HungrySharkEvolutionVita_1.0.vpk` junto al APK. Si existe,
   elige otro `--out`; nunca se reemplaza. Se verifican todos los archivos internos
   y se muestra el SHA del VPK final.
5. Para prueba privada, respalda el progreso con tu método habitual. Transfiere e
   instala **el VPK completo reconstruido** mediante VitaShell. Title ID `HGSKE1178`,
   ubicación estándar de app `ux0:app/HGSKE1178/`. No copies datos Android sobre
   runtime PSP2. Esta tarea no establece la ruta física del guardado ni un
   procedimiento validado de actualizar/reinstalar: conserva backups.
6. Registra instalación, arranque, sonido, menús, evolución, especiales, gameplay,
   pausa, guardado/carga, reinicio frío, actualización y reinstalación conservando
   progreso. Anota modelo, firmware, hash instalado y PASS/FAIL/NOT TESTED por caso.

APK incorrecto: coteja SHA completo. Componente ausente/dañado: vuelve a extraer el
paquete verificado. Decoder desconocido: usa el binario exacto fijado. Falta espacio:
elige otro disco y deja el margen indicado. Acceso denegado: carpeta escribible.
Salida existente: cambia `--out`. Ctrl+C cancela y limpia. Corte eléctrico/cierre
forzado puede dejar `.hse-rebuild-*` con `HSE_TEMP.txt` junto al destino; elimina
sólo esa carpeta tras confirmar que terminó el proceso. Recuperación automática
ante cortes no validada. No borres progreso para investigar sin respaldarlo.

LiveArea: [BetterWinds](https://github.com/BetterWinds). Hashes/tamaños de candidatos
en el expediente; aún no existen hashes de assets de una Release publicada.

## English

**Requirement discovered after the original tests:** the owner observed that v1.0 requires **CapUnlocker** (`ur0:tai/CapUnlocker.skprx`) loaded under `*KERNEL` in the active taiHEN configuration. **Without the plugin the game starts and closes.** Install it, fully reboot the Vita and follow the [setup guide](CAPUNLOCKER_SETUP.md). This also applies to complete installations from PSVitaAlive Store; overclocking is optional.


**Private candidate; not published.** The owner confirms rights and PASS for the
requested Vita matrix. Private build paths in the base still need resolution.
Do not redistribute the stub, patch, APK or VPKs yet.

Supply your own **Hungry Shark Evolution 6.4.2, build228** APK: 104296014 bytes,
SHA-256 `52b7d8754ab8fec75c3b656a0dc5ec8dc849565adc402487a3ac093f6a481f7a`.
Modified/other APKs are rejected. The tool does not download the game.
Windows x64/.NET Framework4.8; no Python, Unity or SDK install. Automated testing
used Windows build family26200; Windows10 and a clean PC remain untested. Allow
2GB free disk space.

1. Extract the whole package into a writable local folder. Keep `HSERebuild.exe`,
   `xdelta3decode.exe`, `HSEVita_Stub.vpk`, `HSEVita_Data.xdelta` and
   `HSEVita_Release.json` together. **Do not install the incomplete stub.**
2. Verify `SHA256SUMS.txt` with PowerShell `Get-FileHash -Algorithm SHA256`.
   For a future Release compare with the maintainer's published hashes.
3. Drag the APK onto the EXE or launch it to select the APK. Native interfaces
   are implemented but still need manual testing. Tested automated alternative:

   ```powershell
   .\HSERebuild.exe "C:\MyGames\HSE_6.4.2.apk" --out "C:\Output\HSEVita_1.0.vpk"
   ```

4. Default output: `HungrySharkEvolutionVita_1.0.vpk` beside the APK. Existing files
   are never replaced; choose a different `--out`. Every internal file is checked
   and the final VPK SHA-256 is displayed.
5. For private Vita testing, back up progress using your established method.
   Transfer/install the **complete reconstructed VPK** with VitaShell. Title ID
   `HGSKE1178`, standard app location `ux0:app/HGSKE1178/`. Do not overwrite PSP2
   runtime data with Android files. This task has not established the save path
   or a validated update/reinstall procedure; preserve backups.
6. Record install, boot, sound, menus, evolution, special sharks, gameplay, pause,
   save/load, cold restart and progress retention on update/reinstall. Include
   device, firmware, installed hash and individual PASS/FAIL/NOT TESTED results.

Wrong APK: compare the exact hash. Missing/damaged component: re-extract the
verified package. Unknown decoder: use the exact pinned binary. Low space: choose
another disk with the displayed margin. Access denied: use a writable folder.
Existing output: change `--out`. Ctrl+C cancels and cleans. Power loss/forced close
may leave a `.hse-rebuild-*` sibling with `HSE_TEMP.txt`; remove only that directory
after confirming the tool stopped. Automatic power-loss recovery is unvalidated.
Preserve progress before investigating failures.

LiveArea artwork: [BetterWinds](https://github.com/BetterWinds). Measured candidate
hashes/sizes are in the preparation receipt; no public Release asset hashes exist.

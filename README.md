<div align="center">

# 🦈 Hungry Shark Evolution Vita

**Hungry Shark Evolution, now on PlayStation Vita.**

A native PS Vita port of the classic shark survival game, adapted from the Android release.

[![Version](https://img.shields.io/badge/Version-1.0-00b6bd?style=for-the-badge)](https://github.com/VegettoSan/Hungry-Shark-Evolution-Ps-Vita/releases)
![Platform](https://img.shields.io/badge/Platform-PS%20Vita-1857a4?style=for-the-badge)
![Game](https://img.shields.io/badge/Game-HSE%206.4.2-173b58?style=for-the-badge)

[**Releases**](https://github.com/VegettoSan/Hungry-Shark-Evolution-Ps-Vita/releases) · [**Instalación**](#-instalación) · [**Controles**](#-controles) · [**English**](#english) · [**Reportar un error**](https://github.com/VegettoSan/Hungry-Shark-Evolution-Ps-Vita/issues/new/choose)

</div>

---

## 🌊 Sobre el port

**Hungry Shark Evolution Vita** lleva la experiencia de *Hungry Shark Evolution* a PlayStation Vita, manteniendo el gameplay y el contenido de la versión **6.4.2 (build 228)**.

| | Características |
| :-- | :-- |
| 🎮 | Gameplay, exploración, alimentación y evolución de tiburones |
| 🕹️ | Controles adaptados a PS Vita, además de soporte táctil |
| 🔊 | Música y efectos de sonido del juego |
| 💾 | Guardado y carga de progreso |
| 🦈 | Menús de evolución y tiburones especiales |
| 📱 | Integración con LiveArea de PS Vita |

> **Versión 1.0** · **Title ID:** `HGSKE1178` · **Plataforma:** PlayStation Vita

## 📦 Descarga

Consulta la sección [**GitHub Releases**](https://github.com/VegettoSan/Hungry-Shark-Evolution-Ps-Vita/releases) para obtener el paquete de la versión 1.0 **cuando esté publicado**.

El port utiliza un **reconstructor para Windows**: a partir de tu propia copia del APK original de Android y los archivos del paquete, genera el VPK que instalarás en tu PS Vita.

**El APK original y el VPK completo no están incluidos en la descarga.** No necesitas Unity, Python ni un SDK para utilizar el reconstructor.

## 📋 Requisitos

| Requisito | Detalle |
| :-- | :-- |
| Consola | PlayStation Vita con posibilidad de instalar VPK mediante VitaShell |
| PC | Windows de 64 bits |
| Software | .NET Framework 4.8 |
| Espacio libre | Al menos 2 GB para la reconstrucción |
| Juego original | **Hungry Shark Evolution 6.4.2**, build **228**, APK sin modificar |

<details>
<summary><strong>🔎 Comprobar que tienes el APK correcto</strong></summary>

El reconstructor acepta el APK original exacto de la versión **6.4.2 (228)**, con estos datos:

| Campo | Valor |
| :-- | :-- |
| Paquete Android | `com.fgol.HungrySharkEvolution` |
| Tamaño | `104296014` bytes |
| SHA-256 | `52b7d8754ab8fec75c3b656a0dc5ec8dc849565adc402487a3ac093f6a481f7a` |

Puedes comprobar el hash desde PowerShell:

```powershell
Get-FileHash "C:\Ruta\HungrySharkEvolution.apk" -Algorithm SHA256
```

Otras versiones, APK modificados o archivos con un hash diferente no son compatibles con este paquete.

</details>

## 🚀 Instalación

1. **Descarga** el paquete de la versión 1.0 desde [Releases](https://github.com/VegettoSan/Hungry-Shark-Evolution-Ps-Vita/releases), cuando esté disponible.
2. **Extrae el ZIP completo** en una carpeta de tu PC. No muevas ni borres los archivos que acompañan al reconstructor.
3. **Arrastra el APK original 6.4.2 (228)** sobre `HSERebuild.exe`. También puedes abrir el programa y seleccionar el APK.
4. **Espera a que aparezca `PASS`**. Se generará el archivo `HungrySharkEvolutionVita_1.0.vpk` junto al APK.
5. **Copia el VPK generado a tu PS Vita** e instálalo con **VitaShell**.

> [!IMPORTANT]
> Instala únicamente **`HungrySharkEvolutionVita_1.0.vpk`**, el archivo completo creado por el reconstructor. **No instales `HSEVita_Stub.vpk`**: es una base incompleta. Si ya tenías una versión instalada, respalda tu progreso antes de actualizar.

**¿Prefieres indicar la carpeta de salida?** Abre PowerShell en la carpeta del paquete:

```powershell
.\HSERebuild.exe "C:\MisJuegos\HSE_6.4.2.apk" --out "C:\Salida\HSEVita_1.0.vpk"
```

El reconstructor no sobrescribe un archivo de salida existente. Para más detalles, consulta la [guía de instalación completa (ES/EN)](docs/RELEASE_V1_0_README_ES_EN.md).

## 🎮 Controles

| Botón | Función |
| :--: | :-- |
| **Stick izquierdo** | Mover el tiburón y navegar por los menús |
| **Cruceta** | Movimiento alternativo y navegación |
| **R** (mantener) | Turbo |
| **✕** | Confirmar / seleccionar |
| **○** | Volver / cancelar |
| **□** | Mostrar u ocultar misiones |
| **△** | Mostrar u ocultar el mapa |
| **START** | Pausar / reanudar |
| **Pantalla táctil** | Interacción táctil con el juego y los menús |

El turbo consume la energía del juego. Las funciones de mapa y misiones dependen del estado de la partida.

## 🛠️ Solución de problemas

| Problema | Qué hacer |
| :-- | :-- |
| El programa rechaza el APK | Comprueba que sea la versión **6.4.2 (228)** y verifica su SHA-256. |
| Faltan archivos o hay errores de checksum | Extrae **todo el ZIP** de nuevo y mantén sus componentes juntos. |
| El programa no puede crear el VPK | Comprueba el espacio disponible y los permisos de escritura de la carpeta. |
| El VPK ya existe | Elige otro nombre con `--out` o guarda una copia del VPK anterior. |
| La Vita no inicia correctamente el juego | Comprueba que instalaste el **VPK final**, no el archivo `Stub`. |

¿Encontraste otro problema? Puedes [**abrir un Issue**](https://github.com/VegettoSan/Hungry-Shark-Evolution-Ps-Vita/issues/new/choose) e indicar qué ocurrió, tu modelo de PS Vita y los pasos para reproducirlo. No adjuntes APKs, VPKs ni datos personales.

---

## English

**Hungry Shark Evolution Vita** is a native PlayStation Vita port based on the original Android game, **version 6.4.2 (build 228)**. It includes gameplay, evolution and special-shark menus, audio, save/load support, Vita button controls and touchscreen input.

### Download and installation

Visit [**GitHub Releases**](https://github.com/VegettoSan/Hungry-Shark-Evolution-Ps-Vita/releases) to get the v1.0 package **once published**.

You need a **64-bit Windows PC**, **.NET Framework 4.8**, at least **2 GB of free space**, a PS Vita capable of installing VPKs through VitaShell, and your **own unmodified Hungry Shark Evolution 6.4.2 (228) APK**.

1. Download and **fully extract** the release ZIP.
2. Drag your original APK onto **`HSERebuild.exe`**, or launch the tool and select the APK.
3. Wait until it displays **`PASS`**. The generated file is **`HungrySharkEvolutionVita_1.0.vpk`**.
4. Transfer the **complete rebuilt VPK** to your PS Vita and install it using **VitaShell**.

**Do not install `HSEVita_Stub.vpk` on its own.** It is incomplete. Back up your save data before updating.

**Required APK SHA-256:**

```text
52b7d8754ab8fec75c3b656a0dc5ec8dc849565adc402487a3ac093f6a481f7a
```

**Controls:** Left stick or D-pad to move/navigate; hold **R** to boost; **✕** confirm; **○** back; **□** missions; **△** map; **START** pause/resume; touchscreen supported.

For detailed troubleshooting and custom output paths, read the [**full English installation guide**](docs/RELEASE_V1_0_README_ES_EN.md#english). Report issues [here](https://github.com/VegettoSan/Hungry-Shark-Evolution-Ps-Vita/issues/new/choose).

---

## ❤️ Créditos / Credits

- **Port y adaptación / Port & adaptation:** [VegettoSan](https://github.com/VegettoSan)
- **Arte de LiveArea / LiveArea artwork:** [BetterWinds](https://github.com/BetterWinds)
- **Reconstrucción mediante xdelta3 / xdelta3 utility:** [Joshua MacDonald](https://github.com/jmacd/xdelta)

*Hungry Shark Evolution* y su contenido original pertenecen a sus respectivos titulares. Este es un proyecto comunitario independiente, no afiliado ni respaldado por Ubisoft o los desarrolladores originales. Utiliza únicamente copias del juego obtenidas legítimamente y respeta las licencias aplicables.

*Hungry Shark Evolution and its original content belong to their respective rights holders. This is an independent community project, not affiliated with or endorsed by Ubisoft or the original developers. Use only lawfully obtained game copies and respect all applicable licenses.*

<div align="center">

**Hungry Shark Evolution Vita · v1.0**

[Releases](https://github.com/VegettoSan/Hungry-Shark-Evolution-Ps-Vita/releases) · [Issues](https://github.com/VegettoSan/Hungry-Shark-Evolution-Ps-Vita/issues) · [Installation guide](docs/RELEASE_V1_0_README_ES_EN.md)

</div>

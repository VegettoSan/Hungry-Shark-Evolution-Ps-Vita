# Codex: offline Unity 5.6.4p3 project export — user-authorized preparation

> **Explicit owner instruction (2026-10-14):** Prepare as much of the *Hungry Shark Evolution 6.4.2* Unity **5.6.4p3** project as realistically possible **without installing or running Unity 5.6**. The user will test/open it later. The owner specifically authorizes this **one-time offline preparatory extraction, AssetRipper export, and safe static repair**, while keeping the formal phase gate **P00 ACTIVE**. This is NOT owner approval of P00, P01 or P02. It is NOT permission to migrate to 2018.2 or to publish original game files.

## Copy-paste into local Codex (Spanish)

**Objetivo prioritario:** entrégame una carpeta privada `HSE642_Unity56_ReadyToOpen` con el proyecto de Unity 5.6.4p3 más completo que puedas dejar **sin tener Unity 5.6 instalado**. Debe incluir el resultado de AssetRipper, escenas, prefabs, materiales, texturas, sonido, animaciones, metadatos y las referencias/scripts reconstruidos hasta donde sean verificables de forma estática. **No me pidas instalar Unity 5.6 para avanzar en esta ejecución.** Yo probaré la apertura y la compilación después.

Trabaja EN WINDOWS LOCAL con el clon del repositorio `Hungry-Shark-Evolution-Ps-Vita`, usa las utilidades ya descomprimidas en `Tool Codex/` y una carpeta de salida privada **fuera del clon**. El APK 6.4.2 debe existir en mi disco local; si no conoces su ruta, búscalo en las rutas de entrada que yo te haya dado, no en Internet. Si no está accesible, termina con un bloqueo preciso; nunca lo descargues de fuentes no verificadas ni sustituyas otra versión.

**Rutas de entrada (rellenar por el usuario):**
- APK original: `<RUTA_ABSOLUTA_AL_APK_6_4_2>`
- Carpeta privada de salida fuera del repositorio: `<RUTA_ABSOLUTA_A_HSEVita-Local>`

### 0. Comprobaciones obligatorias (breves, sin búsquedas redundantes)

1. Lee `AGENTS.md`, `docs/PHASE_GATES.json`, `docs/PORTING_PLAN.md`, `docs/TECHNICAL_AUDIT.md`, `docs/TOOL_CODEX_LOCAL_SETUP.md` y este documento. Ejecuta `py -3 tools/check_phase_gates.py` y `powershell -NoProfile -ExecutionPolicy Bypass -File tools/check_tool_codex.ps1`.
2. Comprueba que el APK es `com.fgol.HungrySharkEvolution`, versión 6.4.2/228, **Unity 5.6.4p3 Mono**, y SHA-256 exacto:
   `52b7d8754ab8fec75c3b656a0dc5ec8dc849565adc402487a3ac093f6a481f7a`.
   Registra integridad ZIP/CRC. No modifiques el APK ni publiques su contenido.
3. Detecta los ejecutables existentes de AssetRipper 2.0.0, ILSpy 11.1, dnSpyEx y Python usando `Tool Codex/_tool_inventory.txt`. **No descargues versiones distintas**, no ejecutes Unity 2018.2 ni intentes instalar Unity 5.6 en este trabajo.

### 1. Extracción completa y reproducible

1. Usa **una vez** el extractor existente `tools/Preparar_Hungry_Shark_Evolution_6.4.2.py` con un directorio privado vacío `Original_Data_Extracted`. Preserva el árbol `Data/` completo, no solo assets con nombres familiares.
2. Verifica **86 fragmentos `.splitN`**, **15 archivos reconstruidos**, **18 `level0`–`level17`**, todos los archivos `Data/Managed` (21 entradas), archivos laterales `.resource` y nombres originales. Espera **2.295 archivos lógicos** en `Data/`. **No presupongas 18 niveles jugables**.
3. Crea manifiestos en formato TXT/CSV/JSON compacto con ruta relativa, tamaño y hash o muestras calculadas eficientemente. No hagas volcado de assets binarios a la conversación. Compara el recuento con el informe existente y explica desviaciones; conserva logs detallados solo de manera privada.

### 2. Exportación offline con AssetRipper

1. Importa los archivos serializados recuperados y **las DLL de `Data/Managed` juntas**, de modo que el lector pueda resolver tipos Mono. Verifica que detecte **Mono**, no IL2CPP.
2. Usa las opciones reales de la **versión instalada** de AssetRipper. Si ofrece CLI consulta ayuda real; si usa servidor local/interfaz GUI, úsala solo cuando la ejecución local lo permita. **No inventes comandos/opciones** ni supongas que solo copiar archivos `Data` crea un proyecto.
3. Exporta el proyecto entero al directorio privado `HSE642_AssetRipper_RawExport` y **preserva esta salida sin modificaciones** como referencia.
4. Registra en el informe cantidad de escenas, prefabs, materiales, shaders, animaciones, audio, texturas y scripts exportados, así como los **Missing Script**, GUID/fileID rotos y errores. Obtén los números con un script sobre disco, sin imprimir miles de rutas al modelo.

### 3. Dejar la copia de Unity lo más preparada posible sin ejecutarlo

1. Copia la exportación a `HSE642_Unity56_ReadyToOpen`. La copia de trabajo debe contener como mínimo `Assets/` y `ProjectSettings/`. Revisa los archivos `.meta`, escenas, referencias de MonoBehaviour, GUID duplicados, rutas y `EditorBuildSettings.asset`. **No cambies a Unity 2018** ni crees una configuración de proyecto ficticia.
2. Si AssetRipper exporta como versión superior o no genera un proyecto legítimo para **Unity 5.6.4p3**, documenta el formato exacto y evita falsear `ProjectVersion.txt` para simular compatibilidad. Prepara copias separadas y cambios justificados de serialización **solo donde sea comprobable**, preservando `.meta` y los GUID. Si queda pendiente validación del editor, decláralo.
3. Inspecciona **`Assembly-CSharp.dll`**, `Assembly-CSharp-firstpass.dll`, `Assembly-UnityScript.dll` y `Assembly-UnityScript-firstpass.dll` con ILSpy/dnSpyEx. Genera inventario de namespaces, clases de gameplay, campos serializados, dependencias y errores de decompilación. El código fuente descompilado completo puede generarse **en disco de forma automatizada si realmente ayuda**; no lo envíes a la conversación ni a GitHub. No inventes fuentes a partir de nombres; identifica qué scripts de AssetRipper ya tienen código y cuáles son referencias vacías.
4. Para recuperar funcionalidad, realiza **solo correcciones estáticas de alta confianza** en la copia de trabajo: script/GUID vinculados por evidencia, nombres de ensamblado/tipo, correcciones de rutas, archivo duplicado o falta de referencia demostrable. Todo cambio debe tener respaldo en metadatos originales, diff reversible y explicación. No reescribas `GameLogic`, `SharkStats`, `SpawnerManager`, `FGOL.Save`, sistemas de tienda/salvado/IA ni alteres comportamiento de juego por suposiciones.
5. **UnityScript** es un riesgo especial. Su convertidor histórico requiere código fuente y un Unity puente compatible; no lo ejecutes sobre DLL binarias como si fueran fuentes. Si la restauración exige un paso en el editor o conversión manual, clasifícalo como PENDIENTE y describe un camino concreto.
6. Si detectas servicios Android (Google Play, anuncios, Ubisoft, JNI) que podrían impedir arrancar el juego en PC, **inventaría los puntos de entrada** y propone adaptadores mínimos; no elimines clases ni apliques stubs globales que cambien la jugabilidad. Para codecs, plugins, shaders y texturas, prepara un listado de riesgos, no modificaciones especulativas.
7. Si es viable sin ejecutar Unity, genera un **validador local estático** que confirme que el proyecto contiene los directorios esperados, assets `.meta` y los GUID de referencia, detecte conflictos y exporte un resumen (no millones de líneas).
8. No publiques ni copies archivos de Halo 2D, datos licenciados de Sony, Unity PSP2 ni binarios propietarios en el repositorio. Mantén las salidas de juego **fuera del clon**; en GitHub pueden quedar exclusivamente scripts genéricos, documentación técnica saneada y el estado real de las pruebas.

### 4. Resultados verificables que debes dejar en disco

```text
<RUTA_PRIVADA_A_HSEVita-Local>/
  Original_Data_Extracted/           # Fuente íntegra, sin cambios
    Data/
      Managed/
      level0 ... level17
  HSE642_AssetRipper_RawExport/      # Exportación intacta, nunca retocarla
    Assets/
    ProjectSettings/
  HSE642_Unity56_ReadyToOpen/        # Proyecto que intentaré abrir luego
    Assets/
    ProjectSettings/
  Reports/
    APK_and_Tool_Identity.txt
    Source_Manifest.json
    AssetRipper_Export_Summary.md
    Mono_Scripts_and_Dependencies.md
    Project_Static_Validation.md
    Unity56_First_Open_Instructions.md
    Known_Blockers.md
  Logs/
    extraction.log
    assetripper.log
```

Si alguna herramienta no produce alguno de esos directorios, no lo fabriques para aparentar éxito; **reporta el bloqueo real y sigue con las tareas independientes posibles**. Conserva un procedimiento reproducible y resultados exactos.

En `Unity56_First_Open_Instructions.md`, indícame cómo abrir el proyecto con Unity 5.6.4p3, qué comprobar en la consola, qué escena abrir primero y qué errores concretos debemos diagnosticar, todo basado en lo extraído y no en suposiciones.

### 5. Control de coste, autonomía y condiciones de cierre

- Trabajo local con herramientas reales, no análisis binario a mano con LLM: usa Python/PowerShell para archivos, diffs, estadísticas y hashing. Ejecuta una extracción/exportación completa como máximo una vez salvo error reproducible corregido; no repitas procesos enormes sin una razón.
- **No gastar tiempo en buscadores o descargas de alternativas** si Tool Codex ya funciona; buscar algo extra solo cuando haya una falla concreta y documentada.
- Divide el trabajo en checkpoints: APK verificado → Data extraída → AssetRipper exportado → scripts inventariados → proyecto estático preparado → reporte. Guarda cada resultado para reanudar sin repetir las etapas correctas si la sesión termina.
- Si un programa queda bloqueado esperando GUI, ofrece pasos de interacción precisos o usa un método automatizado admitido oficialmente. No afirmes que AssetRipper exportó si no existe una exportación comprobable.
- **Límites de fases:** esta es una **excepción expresa y limitada de preparación offline**, NO un pase de fase. Deja `P00 ACTIVE`, `P01 LOCKED` y `P02 LOCKED`; no edites `docs/PHASE_GATES.json`. No instales Unity 5.6, no abras Unity 2018, no hagas VPK, xdelta final, controles PSP2 ni optimización de shaders.
- Al terminar, **NO uses "PROBADO EN UNITY"**. Usa etiquetas honestas: `EXTRACTED`, `EXPORTED`, `STATIC_CHECK_PASS`, `EDITOR_NOT_TESTED`, `COMPILE_NOT_TESTED`, `GAMEPLAY_NOT_TESTED`.
- Entrega un resumen final **máximo 25 líneas** con ubicación privada de `HSE642_Unity56_ReadyToOpen`, pruebas comprobadas, problemas pendientes, archivos más importantes y el paso exacto que deberá hacer el usuario al instalar Unity 5.6.4p3.

**Primera prioridad:** producir la carpeta real `HSE642_Unity56_ReadyToOpen`, no redactar largos informes. Si la fase de exportación no puede completarse, entregar el mejor resultado verificado y un diagnóstico conciso, sin inventar éxito.

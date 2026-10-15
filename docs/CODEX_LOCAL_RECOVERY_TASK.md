# Codex local assignment — Hungry Shark Evolution 6.4.2 → Unity 5.6.4p3

**Tooling first:** use ignored `Tool Codex/` folder in the Windows clone, read [download/folder map](TOOL_CODEX_LOCAL_SETUP.md), run `tools/check_tool_codex.ps1`, and read its `Tool Codex/_tool_inventory.txt` report before asking for tools or browsing. The two Unity editors remain outside the repo.

**Owner-authorized offline-only exception (2026-10-14):** [CODEX_UNITY56_OFFLINE_EXPORT_PROMPT.md](CODEX_UNITY56_OFFLINE_EXPORT_PROMPT.md) explicitly authorizes private extraction and AssetRipper project staging during P00 **even though Unity 5.6 is not installed**, with no phase approval. **Use that specialized document for this exact request.** This general task retains its normal stage gate for all other work.

**Purpose:** give Codex a bounded, reproducible task for preparing the user's **own Windows PC** for restoring HSE 6.4.2 in original Unity 5.6.4p3, with minimal model-token waste.

**Identity/data policy:** the app will be named **Hungry Shark Evolution Vita**, Title ID **HGSKE1178** (see [identity JSON](../config/port_identity.json)). Consult [APK-to-Vita data design](DATA_EXTRACTION_AND_LAYOUT.md). An entire Android Unity Data extraction is appropriate for developer research once P01 is approved; it is NOT evidence that raw Android Unity serialized files can be deployed into a PSP2 VPK, and it is NOT the final public extractor.

**xdelta note:** our [Predators / Pocket Mortys reference](XDELTA_UNITY_PORTS_RESEARCH.md) documents optional **P12** user-data patching. Codex must **NOT** waste P00/P01 tokens generating an xdelta to the original Android Unity files or claiming that it solves Unity engine porting; a working target PSP2 data ZIP does not yet exist.

**Run mode:** **LOCAL WINDOWS** using Codex desktop/CLI opened in this cloned repository. **Do not use Codex Cloud** unless the owner has specifically and lawfully provided the private APK and necessary editor/tool access to that isolated environment. An APK attached to a different ChatGPT Project is **not automatically a file on this PC or a Codex cloud worker**.

**Before starting:** user should save their **owned** 6.4.2 APK in a private local folder outside this repository. Provide its actual full path in the first message. Do not attach/share the copyrighted APK to GitHub, tickets or logs. Leave the Windows PC powered and awake while local tasks operate; cloud tasks differ.

## Paste into Codex (replace paths)

> Trabaja EN ESTE REPOSITORIO LOCAL como agente de preparación del port Hungry Shark Evolution 6.4.2 a PS Vita. Lee `AGENTS.md`, `docs/PORTING_PLAN.md`, `docs/PHASE_GATES.json`, `docs/TECHNICAL_AUDIT.md`, `docs/TOOLS.md` y `docs/EXPERIMENTS.md`, en ese orden. Ejecuta `python tools/check_phase_gates.py` al comenzar. Usa GPT-6.1 Sol High solo para decisiones y bloqueos reales: usa Python/PowerShell y logs para el trabajo masivo sin enviar los assets, DLL o dumps enormes al contexto del modelo.
>
> Ruta de entrada (APK de mi equipo): `<RUTA_COMPLETA_APK>`
>
> Ruta local de trabajo privada FUERA del repositorio: `<RUTA_PRIVADA_DE_TRABAJO>`
>
> **TAREA P00:** verifica el APK sin modificarlo, SHA-256 esperado `52b7d8754ab8fec75c3b656a0dc5ec8dc849565adc402487a3ac093f6a481f7a`, ZIP CRC, 6.4.2/versionCode 228 y Unity 5.6.4p3/Mono; reutiliza el inventario documentado antes de repetir investigaciones. Comprueba si Unity 5.6.4p3 está instalado, cuál es su ruta y si puede abrir una escena vacía en Windows (usa el editor solo con la licencia válida disponible). Identifica las rutas de Python, AssetRipper, ILSpy y Git **sin descargar alternativas si ya funcionan**. Documenta el resultado y cualquier bloqueo con versión, ruta, comando, evidencia sanitizada y siguiente acción en `docs/EXPERIMENTS.md`; deja comprobantes privados detallados en la carpeta local. Nunca commitees el APK, DLL, assets ni datos del SDK.
>
> **BLOQUEO OBLIGATORIO:** revisa el estado en `docs/PHASE_GATES.json`. Si P00 NO está APPROVED por el propietario con el registro requerido, **NO empieces P01** (extracción/exportación AssetRipper); limita esta ejecución a P00 y preparación del entorno. No apruebes fases ni inventes resultados. Si falta una herramienta esencial o acceso interactivo/licencia, documenta exactamente la falta y termina con un resumen breve. No solicites al usuario que permanezca presente ni prometas completar pruebas que requieren su intervención.
>
> **CUANDO P01 ESTÉ ACTIVA DESPUÉS DE APROBACIÓN EXPLÍCITA:** sin cambiar ni subir el APK original, ejecuta **una sola vez** `tools/Preparar_Hungry_Shark_Evolution_6.4.2.py` con el archivo local y un directorio privado `Original_Data_Extracted`; comprueba las 15 reconstrucciones `.splitN`, archivos `level0`–`level17` y `Data/Managed/Assembly-CSharp.dll`, `Assembly-CSharp-firstpass.dll` y `Assembly-UnityScript*.dll`. No copies ensamblados propietarios al repositorio.
>
> Instala/usa **la versión YA identificada de AssetRipper**, sin probar forks ni buscar 20 alternativas. Importa los Unity serialized files junto con sus assemblies Mono **en una misma sesión**, documenta ajustes y errores y realiza la exportación de proyecto Unity al directorio privado `HSE642_Unity56_AssetRipper_Export`. No inventes banderas CLI: comprueba `--help` del ejecutable instalado, o utiliza la interfaz GUI/web de la herramienta si esa versión no trae CLI. Si la GUI exige interacción que no está disponible, no simules el resultado; prepara instrucciones y detente con un diagnóstico preciso.
>
> Verifica en el export `Assets/`, `ProjectSettings/`, `ProjectVersion.txt` y la correspondencia de scripts con escenas/prefabs. Si la exportación resulta ser de un editor distinto a Unity 5.6.4p3, documéntalo y evalúa una copia de migración compatible SIN manipular el campo de versión para aparentar éxito. Usa ILSpy **solo para examinar los tipos o DLL implicadas en errores**; no descompiles el juego completo salvo que haya necesidad demostrable, no vuelques 30 mil métodos al contexto. Evalúa UnityScript como riesgo especial. Separa el proyecto `HSE642_Unity56_AssetRipper_Export` (inmutable) del proyecto de trabajo `HSE642_Unity56_Working`.
>
> **No alteres el gameplay, shaders, controles, saves ni motor** para esconder errores. **P02 también sigue bloqueada** hasta aprobación de P01; en P01 solo se prepara y valida la recuperación, no se declara un juego funcional. Avisa claramente si está solo exportado, abre en Unity o compila/juega realmente.
>
> Minimiza uso: usa la documentación existente como fuente principal; evita investigaciones web amplias, OCR, informes duplicados, lectura de binarios a texto, impresiones de logs completos y reintentos sin cambio. Repite una misma operación fallida como máximo una vez tras una corrección concreta. Guarda logs completos localmente; en GitHub solo notas técnicas breves sin datos propietarios. Realiza commits pequeños exclusivamente de documentación/herramientas públicas y deja el árbol Git limpio. Cierra con un resumen de **máximo 20 líneas**, rutas privadas de los resultados, pruebas PASS/FAIL/NOT TESTED y la siguiente fase bloqueada.

## Reality checks and hard stops

| Condition | Expected action |
| --- | --- |
| P00 ACTIVE / AWAITING_APPROVAL / BLOCKED | **Only P00**; no P01 extraction or AssetRipper project export |
| Missing APK on the Windows machine or hash mismatch | **Stop**; state exact path/mismatch, never substitute the 14.5.0 XAPK or a modded APK silently |
| Missing Unity 5.6.4p3 or no legitimate working license | **Stop** with install/activation blocker, without claiming a working editor |
| AssetRipper missing during P00 | Record path/version or absence; downloading can be authorized as preparatory tooling; **no P01 export yet** |
| P01 ACTIVE with owner-approved P00 | Execute extraction, import and project export **locally**, then compare evidence to P01 gate |
| P01 incomplete or missing-script references | Do not advance to P02; report exact count/errors and corrective plan |
| P01 complete with proof but no owner approval | Set **AWAITING_APPROVAL** if applicable, not APPROVED; owner must review |
| Unity 5.6 project opens but gameplay untested | Label **EXPORT / OPEN ONLY**, not stable/fully playable |
| Public GitHub upload contains game binaries/source or platform secret | **Cancel upload** and move private data outside repo; inspect Git history when necessary |

## Canonical commands (Windows PowerShell examples)

These are **examples**, not commands that run themselves. Replace the absolute APK and private output paths:

```powershell
# P00: read-only integrity + environment check
Get-FileHash "C:\PrivateInputs\HungrySharkEvolution_6.4.2.apk" -Algorithm SHA256
py -3 --version
git --version
py -3 .\tools\check_phase_gates.py

# P01 ONLY after explicit owner approval and a verified phase transition:
py -3 .\tools\Preparar_Hungry_Shark_Evolution_6.4.2.py "C:\PrivateInputs\HungrySharkEvolution_6.4.2.apk" "C:\HSEVita-Local\Original_Data_Extracted"
```

### Expected local outputs after approved P01

```text
C:\HSEVita-Local\
  Original_Data_Extracted\Data\
    Managed\
    level0 ... level17
    globalgamemanagers
  HSE642_Unity56_AssetRipper_Export\  # private, read-only project export
    Assets\
    ProjectSettings\
  HSE642_Unity56_Working\             # separate copy; editing only in approved P02
  Logs\
    00_environment.txt
    01_extract.txt
    01_assetripper.txt
```

We **cannot promise** AssetRipper produces a fully working Unity 5.6.4p3 project from an APK. It may require P02 script reconstruction, reference repair, dependency stubbing and UnityScript migration. The original-game assembly DLLs are not the original editor C# sources.

## Plus usage / long-running task facts (as of 2026-10-14)

- Codex/Work Plus may have both **5-hour** and **weekly** allowances. You cannot select to spend the weekly allowance *instead of* the 5-hour gate. For actual reset and remaining budget, inspect **Settings → Usage**, Codex `/status`, and where available `/usage weekly`. [OpenAI current help](https://help.openai.com/en/articles/20001516-managing-usage-with-gpt-6-astra-in-work-and-codex) · [Codex usage](https://help.openai.com/en/articles/11369540-using-codex-with-your-chatgpt-plan).
- The use meter measures work including reasoning, tools and context. **High reasoning on trivial ZIP/IO operations wastes allowance**. Run shell/Python for deterministic operations and use Sol High for diagnosis and judgment; optionally use a lower effort/model for simple preparation tasks.
- **Local Codex** can use the installed Windows Unity and private local APK, but depends on the PC staying awake. **Codex Cloud** can run with the PC asleep, but will not inherit locally installed Unity/PSP2 or the APK; a remote-only run is therefore not equivalent for this task. [OpenAI Work/Codex documentation](https://help.openai.com/en/articles/20001275-chatgpt-work-and-codex).
- Codex may finish an active turn after a window limit is reached, subject to restrictions, but this is **not guaranteed and cannot be used as a quota bypass**. Do not ask Codex to intentionally loop to burn weekly allowance. Stop when meaningful progress ends.


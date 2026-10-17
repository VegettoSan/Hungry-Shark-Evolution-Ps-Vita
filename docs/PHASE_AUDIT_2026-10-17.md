# Auditoría retrospectiva de fases — 2026-10-17

**Proyecto:** Hungry Shark Evolution 6.4.2 → PS Vita, Unity 5.6.4p3 → Unity 2018.2.19f1 PSP2. **Tipo:** verificación documental de registros públicos y pruebas comunicadas por el propietario, NO nueva ejecución de Unity ni de la consola. **Sin ramas nuevas, sin publicación de código/recursos recuperados ni VPK.**

## Contexto y autoridad

El 17 de octubre de 2026 el propietario, VegettoSan, autorizó expresamente en esta conversación que se audite el trabajo realizado y se marquen como terminadas las fases **que realmente estén listas**. No autorizó fingir pruebas que no constan, aprobar todas las fases por adelantado ni ocultar defectos. La aprobación formal de P00–P02 se registra junto a esta auditoría; P03 permanece ACTIVE. La aprobación es retrospectiva y no convierte los avances de fases posteriores en pruebas cumplidas en orden histórico.

Distinciones: **VERIFIED** = evidencia estática, compilación o prueba instrumentada registrada; **OWNER-REPORTED** = resultado físico o de editor comunicado por el propietario; **PENDING** = criterio sin resultado reproducible suficiente. Los archivos privados indicados en los reportes **no se han inspeccionado de nuevo** durante esta auditoría de GitHub.

## Matriz de las 13 fases

| Fase | Dictamen | Evidencia observada y límite |
| --- | --- | --- |
| **P00** fuente y entorno | **APPROVED retrospectivo; equivalencia explícita** | Identidad SHA-256/CRC, Unity 5.6.4p3 y editor 2018.2.19f1 documentados; proyecto real abrió y ejecutó Play Mode (prueba más exigente que un proyecto vacío). Inventario de herramientas/PSP2. **No existe recibo de una escena vacía independiente ni matriz Android completa**; la referencia Android detallada queda como requisito de P03. Se acepta la sustitución acotada del smoke test, **no** se afirma que se ejecutó el test vacío. |
| **P01** recuperación | **APPROVED** | 2.295 ficheros lógicos, 15 conjuntos split, 21 ensamblados, 18 escenas serializadas; AssetRipper 2.0.0 con Mono, exportación, inventarios, trazabilidad GUID/fileID, scripts y hallazgos dañados documentados. No implica fidelidad total del export. |
| **P02** editor 5.6 | **APPROVED con pendientes de referencias registrados** | Compilaciones del editor/ensamblados y shaders sin errores C# bloqueantes en pruebas documentadas; SplashLoader, menú y gameplay se han ejecutado. **55 referencias uGUI, 8 scripts nulos en seis prefabs, desajuste de MissionAchievementsManager e iluminación** siguen abiertos; la aprobación reconoce los componentes esenciales ejercitados, **no** declara limpiadas todas las escenas y prefabs. Cualquier hallazgo que afecte el loop principal debe repararse durante P03. |
| **P03** gameplay PC | **ACTIVE / parcialmente observado** | Propietario confirma movimiento, menú, gráficos, audio y relectura del guardado en Unity 5.6. Falta matriz completa registrada de alimentación, hambre/vida, muerte, reintento, boost, comparación con Android, perfil nuevo/continuado y regresión reproducible. EXE Windows opcional según autorización previa. |
| **P04** baseline 5.6 | **LOCKED / no aprobado** | No existe prueba registrada de reimportación limpia y matriz repetida de gameplay con backup **inmutable y hashes**; confirmación de guardado tras reabrir no sustituye esa congelación ni aceptación literal. |
| **P05** migración 2018 | **LOCKED formalmente / migración técnica realizada** | Copia separada Unity 2018.2.19f1, reparaciones de compilación/animación, cero errores de compilación en auditorías. Faltan los gates previos y cierre de referencias/regresión. |
| **P06** paridad PC 2018 | **LOCKED / pruebas parciales** | Menú y Reef con controles/animaciones durante ventanas limitadas de Editor, sin matriz de equivalencia contra la base Unity 5.6. |
| **P07** smoke PSP2 independiente | **LOCKED / herramienta operativa pero criterio independiente pendiente** | Builds Mono2x y VPK nativos de HSE compilados y ejecutados en Vita; no se ha registrado **una escena independiente** que verifique todas las rutas, suspend/resume, input y audio placeholder según el gate. |
| **P08** boot y menú Vita | **LOCKED formalmente / hito técnico observado** | El propietario probó D02–D06 en Vita; fondos y menú originales cargan, D05 llega a gameplay y audio, problemas de BC7/Facebook/FMOD tratados. La experiencia de arranque ha avanzado; no reemplaza los gates históricos sin cerrar. |
| **P09** core gameplay Vita | **LOCKED formalmente / jugable parcialmente** | Propietario confirma gameplay y 40–58 FPS en D05; no constan pruebas repetibles de alimentación, salud, muerte y reintento con comparación P06. |
| **P10** controles, audio, saves, lifecycle | **LOCKED / parcial con fallo conocido** | Audio audible tras conversión HEVAG; navegación provisional. **Guardado se sigue perdiendo o se informa corrupto**; D08/D09 tienen builds/fixtures PC, pero no evidencia de cold-reload Vita correcto. Falta matriz física completa, touch y suspensión/reanudación. |
| **P11** rendimiento/estabilidad | **LOCKED / experimentos parciales** | D06 informado ~30 FPS y descensos ~11 al alejar cámara; pausas en menú. D07 instrumentación y D08 microoptimizaciones sin benchmarks completos repetibles ni sesión larga sin crash. |
| **P12** release | **LOCKED / pendiente** | Sin matriz final, estrategia legal validada de distribución de datos, extractor de usuario o release aprobado. |

## Fuentes auditables

- P00: [APK y ambiente](TECHNICAL_AUDIT.md), [reporte de recuperación](RECOVERY_STATUS_2026-10-15.md), [experimentos OFF-001 y OWNER-BASE](EXPERIMENTS.md).
- P01: [recuperación y manifiestos](RECOVERY_STATUS_2026-10-15.md), [AssetRipper OFF-002/003/004/006](EXPERIMENTS.md).
- P02: [restauración y pendientes](RECOVERY_STATUS_2026-10-15.md), [experimentos PCSRV/SH15/OWNER-BASE](EXPERIMENTS.md).
- P03–P04: [pruebas y requisitos de congelación](UNITY56_BASELINE_AND_UNITY2018_HANDOFF.md), [plan](PORTING_PLAN.md).
- P05–P06: [compilación 2018](UNITY2018_ENUM_PLATFORM_REPAIR.md), [auditoría animación](UNITY2018_RUNTIME_AUDIT_2026-10-16.md), [controles](VITA_CONTROLLER_INTEGRATION_2026-10-16.md).
- P07–P09: [pantalla negra y paquete D02](PSP2_BLACK_SCREEN_2026-10-17.md), [D04 SDK](PSP2_OFFLINE_SDK_D04_2026-10-17.md), [D05 gameplay y audio](PSP2_LOAD_AUDIO_INPUT_D05_2026-10-17.md), [D06 medium](PSP2_MEDIUM_D06_2026-10-17.md).
- P10–P11: [D07](PSP2_PERFORMANCE_D07_2026-10-17.md), [D08 save/CPU](PSP2_SAVE_CPU_D08_2026-10-17.md), [D09 copy](PSP2_SAVE_IO_D09_2026-10-17.md) y los posteriores mensajes del propietario sobre corrupción en Vita.
- P12: [criterios del plan](PORTING_PLAN.md).

## Próximo trabajo formal y técnico

**Formal:** P03 ACTIVE; capturar una matriz mínima reproducible del loop 5.6 en Editor, incluyendo carga/perfil nuevo, movimiento/turbo, alimentación/hambre/salud, muerte/reintento, sonidos y guardado frío, con comparativa Android y clasificación de los pendientes P02. P04 exige congelar la base 5.6 y aceptación literal del propietario. Una aprobación retroactiva de P00–P02 **no** certifica P03–P12 ni afirma cumplimiento cronológico de las fases ya ejecutadas por encargos directos.

**Técnico en paralelo por solicitudes expresas del propietario, sin aprobación de gate:** preservar VPK D05 funcional como referencia; investigar la corrupción del save local PSP2 y comparar datos reales de `.sav` / `.temp` sin renombrar o sobrescribir los originales a ciegas; después medir rendimiento D06/D07 en escenas equivalentes y terminar mapa de controles/lifecycle. La autorización para correcciones puntuales no equivale a aprobar fases ni a crear ramas nuevas.

Este archivo es una evaluación honesta del estado, no un log de nuevos test de consola ni de pruebas que no se realizaron.

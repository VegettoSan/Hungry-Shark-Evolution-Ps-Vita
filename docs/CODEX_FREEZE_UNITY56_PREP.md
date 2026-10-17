# Codex task — preserve Unity 5.6 baseline, prepare handoff (NO MIGRATION)

**Historical P00 preparation task.** P00–P02 are now approved (2026-10-17), P03 is ACTIVE. The immutable baseline required for P04 still needs a clean documented full PC gameplay regression and explicit approval. Do not treat this old task as authority for new branches or phase transition. See [current audit](PHASE_AUDIT_2026-10-17.md).


**Authorized now:** inventory, create a private **candidate backup**, reproduce owner-confirmed tests where possible, prepare sanitized evidence/checklists. **Not authorized now:** claiming P04 acceptance, transitioning phase states without owner approval, opening Unity 2018, compiling PSP2/VPK, committing game assets.

## Paste into Codex local

Trabaja dentro del clon local de `VegettoSan/Hungry-Shark-Evolution-Ps-Vita` y con los recursos privados en `G:\PsVita\My Ports\HSEVita-Local`. Lee `AGENTS.md`, `docs/PORTING_PLAN.md`, `docs/PHASE_GATES.json`, `docs/RECOVERY_STATUS_2026-10-15.md` y `docs/UNITY56_BASELINE_AND_UNITY2018_HANDOFF.md`. **Objetivo: dejar documentada, respaldada y verificable la base funcional Unity 5.6.4p3 SIN iniciar aún la migración a Unity 2018.**

El propietario confirmó en chat que el gameplay, shaders, gráficos y guardado funcionan; antes ya había confirmado audio y movimiento. Registra esto como **OWNER-CONFIRMED** indicando que el propietario confirmó también **recuperar el guardado tras cerrar y reabrir**. Esa confirmación no equivale a un log de prueba independiente en una copia limpia, sesiones largas, comparación exhaustiva con Android ni un ejecutable PC. **No hace falta producir un EXE de Windows**: el plan ahora permite matriz completa en Unity Editor Play Mode.

1. Comprueba estado Git y fases con `py -3 tools/check_phase_gates.py`; jamás alteres el ledger para simular aprobaciones.
2. Reutiliza informes y checkpoints existentes; no repitas AssetRipper ni la extracción. Localiza el proyecto Unity 5.6 actual y cierra o coordina con el usuario si el editor está abierto. **Nunca trabajes sobre un proyecto que otro Unity tenga abierto**.
3. Genera una **copia privada independiente** etiquetada `CANDIDATE` en `Baselines/`, nunca `FROZEN` hasta aprobación de P04. Preserva `Assets`, `ProjectSettings`, archivos `.meta`, `ProjectVersion` y demás insumos del proyecto. Excluye cachés reconstruibles `Library`/`Temp`/`Obj`/`Logs` y archivos de IDE. No uses `/MIR`, `/PURGE`, no sobrescribas backups existentes. Conserva intactos tanto la exportación raw como el proyecto actual.
4. Genera inventario JSON privado con SHA-256 de archivos fuente y proyecto, escenas/orden, shader/material/prefabs, GUID duplicados, referencia a checkpoints 01–19, estado de referencias pendientes (55 uGUI, ocho null scripts/seis prefabs, MissionAchievementsManager), aserción de prefabs y test incompleto del shader dorado.
5. Prepara una matriz real en Markdown privado de Play Mode: inicio, menú, movimiento/cámara, turbo mantenido, comer, daño/hambre, muerte/reinicio, UI, efectos de agua, gráficos, audio y guardar/cerrar/reabrir/cargar; preserva evidencia sin datos propietarios en Git. Marca casos nuevos `NOT TESTED` hasta que haya prueba. No elimines pendientes sin diagnóstico.
6. Asegura que el Editor 5.6 y una escena vacía realmente funcionan, y documenta referencia Android; si no hay pruebas, anótalas faltantes. No pruebes cambios que puedan sobrescribir el save activo sin hacer su respaldo.
7. Documenta solo observaciones sanitizadas en `docs/EXPERIMENTS.md` y entrega al propietario una tabla breve de condiciones ya satisfechas vs. faltantes. No declares P00–P04 aprobadas por tu cuenta.
8. Deja un informe de **cómo continuar P00→P01→P02→P03→P04** en orden y **qué aprobación literal se necesita** para habilitar P05. Si falta evidencia, detente en esa fase sin alterar las otras.
9. No migres Unity, no toques la copia `FROZEN` y no prepares VPK. Evita investigaciones web generales, nuevos shaders o ediciones de código si no hay error concreto. Prioriza reportes compactos, decisiones con evidencia y bajo consumo de tokens.

Entrega: ubicación exacta de `CANDIDATE`, inventario/reporte y matriz privada, pendientes, estado comprobable del ledger. **El siguiente encargo para Unity 2018 solo se ejecutará tras la aprobación formal P04**.

# BetterWinds LiveArea v2 — 2026-10-21 (owner-provided input)

**Status: REVIEWED / STAGED LOCALLY; NOT YET INTEGRATED OR TESTED ON PSP2.** This note documents the second owner-supplied ZIP `SharkEVO.zip`. It does **not** supersede historical evidence in `docs/BETTERWINDS_LIVEAREA_2026-10-18.md`. No executable, VPK or private Unity project was changed in this repository task.

**Artwork credit:** [BetterWinds](https://github.com/BetterWinds) contributed LiveArea artwork for this port. Underlying *Hungry Shark Evolution* art and trademarks belong to their respective rights holders. Binary artwork remains excluded from this public repository pending confirmation of redistribution permissions; owner supplies it privately.

## Verified input inventory

| Path in owner-supplied `SharkEVO.zip` | Size (bytes) | Dimensions / format | SHA-256 |
| --- | ---: | --- | --- |
| `bg0.png` | 180020 | 840×500 / palette PNG | `c5b2704c3fc91bb7d35cf46ec41941ed1c9d02f2d8a0dd90926b40aabb612e07` |
| `icon0.png` | 30695 | 128×128 / RGBA PNG | `81d4755193278c3cde2d821398d300d01c3a6b9d5e3df868db9f9fb70c83c1c5` |
| `startup.png` | 3274 | 280×158 / palette PNG | `293fff5b6e8abc7923ed2d9f3b1bb92b233843c7d6416fa5b657b8b2730a4f4d` |
| `pic0.png` | 223813 | 960×544 / palette PNG | `403dc5de774542a200bbc744133cff29e5151faf9679ac77d956824bd4edc732` |
| `Manual/001.png` | 217126 | 960×544 / palette PNG | `e8a05c546a44d6b2dd598a04f3838d879a45ac5fe6b39ecc2c33c6e3b07e7e6f` |
| `template.xml` | 250 | XML, style `psmobile` | `bd7469fe19dbc88e0688ac16b3fd717c866689e2b1e35964594fff44813d1b35` |

**Comparison with v1:** icon and startup are byte-identical to v1. Background differs; `pic0.png` and `Manual/001.png` are newly supplied. The XML changes the LiveArea style from the previously proven `a1` to `psmobile` (gate location differs); this must be confirmed visually on PS Vita.

## Important correction and build staging

The supplied XML says `<image>bg.png</image>` although the archive contains `bg0.png`. A prepared *derived* `template.xml` corrects this reference to `bg0.png` while retaining `psmobile`, `startup.png` and all other structure. Corrected XML SHA-256: `59e615040c5edccc3d2497452be70d27a99627222114637d58616d4579e1c2dc`.

The original palette background is 180020 bytes; it exceeds the 128-KiB limit documented by some community PS Vita packaging guides. A prepared **build-only** derived indexed background (same 840×500 resolution, 103628 bytes, SHA-256 `67debd07588d8573f01be88f42d0df0915802483d49cdcf9011444b888d58523`) was made using Pillow RGB→256-color FASTOCTREE quantization and optimized PNG encoding. This is **not** byte-identical; inspect image quality on actual hardware before selecting the derived copy. The original stays intact. No claim is made that size limit or visual fidelity has been verified on physical Vita.

Expected final VPK system paths (not yet demonstrated):

```text
sce_sys/icon0.png
sce_sys/pic0.png
sce_sys/manual/001.png
sce_sys/livearea/contents/bg0.png
sce_sys/livearea/contents/startup.png
sce_sys/livearea/contents/template.xml
```

The separate owner-delivered `SharkEVO_LiveArea_v2_PS_Vita_preparado.zip` contains the full original file set under `originals/` plus this derived VPK path tree under `staging/`. It is **not a VPK** and must not be shipped or committed to this public repository. It contains no executable or PSP2 identity data. `sce_sys/manual/001.png` is a documented manual path, but packing the image alone does **not** establish that the manual entry will appear in the LiveArea.

## Codex integration task (existing `main` only)

1. Read `AGENTS.md`, `docs/CODEX_BETTERWINDS_LIVEAREA.md` and the October 18 integration report. Work in the **current** Unity 2018.2.19f1 PSP2 private project, not Unity 5.6. **Do not create branches.** Preserve existing uncommitted work, Title ID `HGSKE1178`, gameplay, save data, runtime code and phase gates.
2. Back up prior native LiveArea artwork, `ProjectSettings.asset`, and a previous working VPK **without resetting/rebasing**. Inspect the actual current build pipeline before changing it.
3. Use the **v2** privately supplied assets from `SharkEVO.zip` or the separately prepared staging ZIP; do not use the old `tools/install_betterwinds_livearea.py` unmodified because it requires the three **v1** hashes and has no v2 manual/pic0 support. Do not overwrite the original supplied assets.
4. Keep the existing native Unity PSP2 LiveArea configuration where compatible. Update the expected background digest in `tools/unity/HSEBetterWindsLiveArea.cs` according to the **actual staged input**; account for Unity's documented native icon conversion. The old `tools/verify_betterwinds_vpk.py` assumes the v1 background, `a1` XML, no manual, and unchanged old `pic0`; add a **v2-specific** verification path or extend it without breaking historical v1 evidence.
5. Ensure all six system paths above appear in the **final** VPK if supported by the existing Unity/PSP2 packager. Unity's custom LiveArea path may only copy `*.png` and `*.xml` under its contents directory, **not automatically** the top-level pic0 and manual; inspect output. If a minimal post-build copy is necessary, do it safely and without touching eboot, game content, SFO identity, saves or any other files.
6. Compare the final ZIP CRC, duplicate/case-colliding paths, native XML refs, PNG sizes/formats/hashes, `param.sfo` title/identity, game payload and the previous VPK; changes to `pic0.png` are **intentional in v2**, unlike the v1 verifier. Validate that manual support is actually enabled. Only then perform physical Vita LiveArea, manual/help, gate, icon, startup and loading-background inspection.
7. Preserve BetterWinds credit in public docs; do not publish the owner-supplied binary art without permission. Report **VERIFIED** versus **NOT TESTED**, with hashes, changed files, rollback steps and a small commit to existing `main` when the local/remote histories safely agree. No unrelated runtime/performance/save work and no phase approval.

**Current verification:** ZIP content parsed, five PNGs decoded at expected dimensions, original hashes recorded, and corrected XML structure verified. Unity import/build/VPK packaging and hardware visual behavior: **NOT TESTED**.

## Seguimiento de integración — 2026-10-23 UTC

La revisión/staging anterior es histórica. Véase el [informe de integración y sus límites](BETTERWINDS_LIVEAREA_V2_INTEGRATION_2026-10-23.md) para la configuración nativa, el rechazo PSP2 de psmobile y el paquete privado de prueba. Las fases y los derechos de distribución no cambian.

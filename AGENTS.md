# AGENTS.md — instructions for Codex, AI agents and contributors

These rules apply to the entire repository. Read `README.md`, `docs/TECHNICAL_AUDIT.md`, `docs/PORTING_PLAN.md`, and `docs/EXPERIMENTS.md` **before** modifying code or claiming a milestone.

## Objective and constraints

- Target a **native PS Vita build** of Hungry Shark Evolution **6.4.2**. Prefer recovery and faithful adaptation of the existing Unity/Mono gameplay over rewriting mechanics or introducing a different game engine.
- Known source player: **Unity 5.6.4p3 / Mono**. Candidate build player: **Unity 2018.2 with PSP2 module**. Their compatibility has NOT been demonstrated.
- Work incrementally. Do not change fundamental gameplay, save format, shark stats, spawning, physics, AI, progression or scene order without concrete evidence and a focused test.
- Do **not** attempt to embed the original Android `libunity.so` as the first Vita strategy. Android native libraries depend on Android OS / graphics / system APIs.
- Preserve scene / prefab / script identity and references (GUID, fileID, type name, namespace, serialized fields) whenever recovering content. Track all intentional remaps.
- Keep legacy UnityScript assemblies visible in audits. **Do not assume** their contents can be compiled unchanged in Unity 2018.2; validate or convert with tests.
- Separate Android-only services (ads, purchases, Google Play, Ubisoft) behind narrowly scoped adapters. Offline gameplay must not silently depend on network responses. Preserve contracts, do not blanket-delete classes.

## Evidence first

1. Clearly label each statement **VERIFIED** (directly observed), **INFERRED** (reasoned from evidence), or **UNTESTED**. Do not promote inferred results to verified.
2. Every meaningful experiment adds a row to `docs/EXPERIMENTS.md` with date, input hash, tool/editor version, command or settings, outcome, evidence log, and next step. "Not tested" is different from "failed".
3. Before a potentially destructive change, save a checkpoint or separate branch. Use small, topical commits with understandable messages.
4. Run the smallest relevant validation (Python test, import, compile, device runtime) before declaring success. If no physical Vita was used, say so explicitly.
5. Prefer reproducible scripts and short markdown notes over large generated inventories. Do not check in dumps, decompiled game code, exports, or duplicate reports.
6. Avoid unattended broad refactors, speculative compatibility hacks, disabling error checks, or deleting missing scripts just to hide build errors.
7. When blocked, document the exact blocker and evidence rather than inventing success or substituting an unrelated game prototype.

## Distribution and security

- **Never commit or publish** original APK/XAPK/OBB, game code or DLLs, extracted textures/audio/models/scenes, Unity proprietary players, Sony SDKs, credentials, personal paths, or unauthorized assets.
- Developer machines may keep proprietary inputs in ignored local folders. Public scripts must require user-supplied game data.
- Do not add a GitHub Actions workflow that fetches copyrighted APKs, private SDKs, or proprietary assets.
- Inspect any generated log before sharing for embedded data or identifying local paths.
- Only publish a VPK after verifying the distribution rights and documenting real-device test results.

## Definition of progress

- **Audit complete:** facts and hashes recorded.
- **Assets recovered:** export verified, scenes and MonoBehaviour links assessed, errors documented.
- **Editor prototype:** clean controlled Unity 2018.2 compile and a real scene test.
- **Vita prototype:** built with PSP2, boots and renders on real hardware with logs.
- **Playable:** controls, gameplay, save, audio and stability validated on hardware.

Current confirmed stage: **audit complete; recovery/build/device stages remain untested**.

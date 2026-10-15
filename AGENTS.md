# AGENTS.md — mandatory rules for Codex, AI agents and contributors

These rules apply to the **entire repository**.

## Required reading before any change

1. Read [`docs/PORTING_PLAN.md`](docs/PORTING_PLAN.md) **in full** and [`docs/PHASE_GATES.json`](docs/PHASE_GATES.json).
2. Read [`docs/TECHNICAL_AUDIT.md`](docs/TECHNICAL_AUDIT.md) and [`docs/EXPERIMENTS.md`](docs/EXPERIMENTS.md).
3. Read [`docs/TOOLS.md`](docs/TOOLS.md) and [`docs/HALO2D_PSP2_REFERENCE.md`](docs/HALO2D_PSP2_REFERENCE.md) for the **verified, phase-scoped downloads and compatibility caveats**. Do not auto-download a substitute or new editor just because it is newer. **Unity 5.6.4p3 is the source editor and Unity 2018.2.19f1 (verified in Halo 2D) is the preferred target; current PSP2 must be verified independently.**
4. Run `python tools/check_phase_gates.py`. If it fails, **STOP** and correct the ledger (without fabricating approval).
5. Only perform work allowed by the **single ACTIVE phase**. If the phase is AWAITING_APPROVAL or BLOCKED, focus exclusively on its proof or corrective work. **Never implement a LOCKED phase.**

## Mandatory engine progression

- **Unity 5.6.4p3 is the original engine**; validate the native Unity 5.6 PC executable and preserve an approved stable baseline **before** creating a Unity 2018.2 migration clone.
- **Unity 2018.2 PC gameplay must pass equivalent tests** before PSP2 port implementation.
- **Physical PS Vita gameplay** must pass independent tests; a Unity test scene or a VPK merely installing is not equivalent to a working port.
- Unity 5.6.4p3 is a 2017 editor, **earlier than Unity 2018.2**. Existing Mono DLLs and UnityScript code do not guarantee full source or working script references.
- Use a native Unity PSP2 player; Android `libunity.so` is not portable unchanged.

## Approval authority and evidence

- Only the repo owner **VegettoSan** may approve stage transitions **explicitly**. An agent, an automated build or a green CI check is **not** the owner.
- Record two independent evidence references, the owner's approval reference (GitHub issue/PR link), UTC date and owner ID in `docs/PHASE_GATES.json`. Next phase becomes ACTIVE **only after** the previous is APPROVED. Run `python tools/check_phase_gates.py` after each gate change.
- Never infer "approval" from silence, no errors, or an assistant's own confidence. A FAIL or missing proof is BLOCKED/ACTIVE as appropriate, never APPROVED.
- Review experimental results and phase scope with owner; document failed alternatives without secretly switching scope.
- CI checks ledger shape/order; it **does not** independently validate the honesty of evidence or actual hardware behavior. Require manual GitHub branch-protection status checks and maintainer review to guard merges.

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

Current confirmed stage: **P00 ACTIVE, formal approval pending; Unity recovery/build/device stages remain untested**.

# AGENTS.md — mandatory rules for Codex, AI agents and contributors

These rules apply to the **entire repository**.

## Required reading before any change

1. Read [`docs/PORTING_PLAN.md`](docs/PORTING_PLAN.md) **in full** and [`docs/PHASE_GATES.json`](docs/PHASE_GATES.json).
2. Read [`docs/TECHNICAL_AUDIT.md`](docs/TECHNICAL_AUDIT.md) and [`docs/EXPERIMENTS.md`](docs/EXPERIMENTS.md).
3. Read [`docs/TOOLS.md`](docs/TOOLS.md) and [`docs/HALO2D_PSP2_REFERENCE.md`](docs/HALO2D_PSP2_REFERENCE.md) for the **verified, phase-scoped downloads and compatibility caveats**. Do not auto-download a substitute or new editor just because it is newer. **Unity 5.6.4p3 is the source editor and Unity 2018.2.19f1 (verified in Halo 2D) is the preferred target; current PSP2 must be verified independently.**
4. Before local Windows work, read [docs/TOOL_CODEX_LOCAL_SETUP.md](docs/TOOL_CODEX_LOCAL_SETUP.md), run `tools/check_tool_codex.ps1`, and consult ignored `Tool Codex/_tool_inventory.txt`. Portable executables live inside ignored `Tool Codex/` at the repository root; Unity 5.6 and 2018.2 are separately installed. Use local tools instead of unnecessary new downloads.
5. For the owner's explicitly authorized **Unity 5.6 offline project preparation without the editor installed**, follow [docs/CODEX_UNITY56_OFFLINE_EXPORT_PROMPT.md](docs/CODEX_UNITY56_OFFLINE_EXPORT_PROMPT.md). This **one-time P00 preparatory exception** permits private local extraction, AssetRipper export and evidence-backed **static-only** repair without approving P00/P01/P02. See the scoped policy in [PORTING_PLAN](docs/PORTING_PLAN.md#p00-scoped-offline-recovery-staging-authorization-owner-request-2026-10-14). For other tasks, [docs/CODEX_LOCAL_RECOVERY_TASK.md](docs/CODEX_LOCAL_RECOVERY_TASK.md) still applies.
6. Run `python tools/check_phase_gates.py`. If it fails, **STOP** and correct the ledger (without fabricating approval).
7. Only perform work allowed by the **single ACTIVE phase**, except the explicitly owner-authorized **P00 offline static recovery staging** detailed above, which **does not approve or transition any locked phase**. If a phase is AWAITING_APPROVAL or BLOCKED, focus on its proof/corrective work. Never treat preparatory staging as a passed gate.

## Branch creation and authorization — mandatory

- **Only the repository owner, VegettoSan, may explicitly authorize the creation of a new branch, before it is created.** This restriction applies to Codex, all other AI agents, bots, GitHub Actions, tools and human contributors alike, for both local and remote branches.
- **Never create, publish, duplicate, rename or automatically generate a branch without that prior, task-specific owner authorization.** This includes `codex/*`, `feature/*`, temporary, recovery, PR, and worktree branches. A pull-request workflow, a technical need, permission to edit code, or access to GitHub **is not permission to create a branch**.
- Work only with the existing branch explicitly designated by the owner for the task. If a new branch seems necessary or no write target has been authorized, **STOP and request permission first**; do not silently choose a branch or use automation that creates one.
- Explicit authorization for one merge or one task is **not** standing authorization to create branches in later work. Preserve this rule in all subsequent edits and instructions.

## Identity, original APK data and Vita storage

- User-approved title ID **`HGSKE1178`** and display name **Hungry Shark Evolution Vita**: use [config/port_identity.json](config/port_identity.json) as canonical values for the future Unity 2018 PSP2 settings, extractor and docs. Never reuse Halo 2D title, content IDs or secrets.
- Read [docs/DATA_EXTRACTION_AND_LAYOUT.md](docs/DATA_EXTRACTION_AND_LAYOUT.md) before packaging or extraction work. Standard app installation is `ux0:app/<TITLEID>/` (runtime `app0:`); saved progress requires its separately verified persistent-data path, not the installed app directory.
- **xdelta3 is an optional candidate for P12 release data delivery**, based on [Predators / Pocket Mortys research](docs/XDELTA_UNITY_PORTS_RESEARCH.md); it is **NOT** an APK-to-Vita converter or a way to bypass original-engine recovery. Neither the full Vita target ZIP nor a safe-to-publish HSE patch exists yet. Inspect source/target hashes and copyright implications; only pursue this after the PSP2 gameplay and content gates pass.
- **Developer full extraction and final end-user minimal extraction are different products.** In approved P01 preserve all reconstructed Unity `Data/` and Mono assemblies; do not copy original Android serialized files into the Unity PSP2 runtime and claim success.
- Exact PSP2 data subtree and final end-user data set are **UNTESTED** pending real device tests (P07–P12). Do not invent paths or a converter; research/documentation in P00 does not unlock implementation.

## Mandatory engine progression

- **Unity 5.6.4p3 is the original engine**; validate the full original gameplay matrix on PC via **Unity Editor Play Mode** (owner-approved alternative to a standalone Windows EXE), verify cold save/load and preserve an **approved** stable baseline **before** creating a Unity 2018.2 migration clone. Read [freeze/handoff](docs/UNITY56_BASELINE_AND_UNITY2018_HANDOFF.md) and [current prep task](docs/CODEX_FREEZE_UNITY56_PREP.md).
- **Unity 2018.2 PC gameplay must pass equivalent tests** (editor-based desktop regression is acceptable) before PSP2 port implementation. The [P05/P06 task](docs/CODEX_UNITY2018_MIGRATION_PREP.md) remains **locked until P04 explicit approval**.
- **Physical PS Vita gameplay** must pass independent tests; a Unity test scene or a VPK merely installing is not equivalent to a working port.
- Unity 5.6.4p3 is a 2017 editor, **earlier than Unity 2018.2**. Existing Mono DLLs and UnityScript code do not guarantee full source or working script references.
- Use a native Unity PSP2 player; Android `libunity.so` is not portable unchanged.
- **Shader optimization reference:** [docs/SHADERS_PSVITA_REFERENCE.md](docs/SHADERS_PSVITA_REFERENCE.md) provides candidates and benchmarking methods, **NOT mandatory shader substitutions**. Target **30 FPS stable in gameplay**, investigate **60 FPS if measurements permit**. P01–P04 preserve original-engine appearance; P05–P06 require Unity 2018.2 visual parity; P07–P08 handle actual PSP2 shader compatibility; P11 performs measured optimization. Do not implement later phases early or claim FPS absent Vita measurements.
- **P10 input policy:** reuse/merge the **legacy Unity Input Manager mappings** from [Halo 2D's ProjectSettings/InputManager.asset](https://github.com/VegettoSan/Halo2D_PsVita/blob/main/Halo%202D/ProjectSettings/InputManager.asset), not an unverified custom input rewrite. See [measured source mappings and caveats](docs/HALO2D_PSP2_REFERENCE.md#reusing-halo-2ds-standard-unity-input-manager-verified-p10-reference-only). Preserve the recovered HSE settings; never overwrite wholesale. The actual input integration remains **LOCKED** until P09 approval.

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
3. Before a potentially destructive change, save a local checkpoint or backup. Create a separate branch **only if the owner explicitly authorizes that branch beforehand**, per the mandatory branch rule above. Use small, topical commits with understandable messages.
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

Current formal stage: **P00 ACTIVE, 0/13 phases approved**. Focused owner-requested Unity 5.6 recovery, source compilation and editor checks have evidence; the owner reports brief gameplay and model visibility. A standalone PC build, complete regression/Android comparison and stable baseline remain unverified. Unity 2018/PSP2/device stages remain untested. Read [the current recovery status](docs/RECOVERY_STATUS_2026-10-15.md) before treating historical offline-only status as current; no acceptance criteria or phase approvals are changed.

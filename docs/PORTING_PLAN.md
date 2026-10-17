# End-to-end gated development plan — Hungry Shark Evolution PS Vita

**Authoritative phase ledger:** [`PHASE_GATES.json`](PHASE_GATES.json) · **Validation:** `python tools/check_phase_gates.py`

**Status after retrospective owner-authorized audit (2026-10-17):** **P00, P01 and P02 APPROVED; P03 ACTIVE; P04–P12 LOCKED**. See [the audit](PHASE_AUDIT_2026-10-17.md), [formal ledger](PHASE_GATES.json) and [owner-authorization record](https://github.com/VegettoSan/Hungry-Shark-Evolution-Ps-Vita/issues/2). Private Unity 2018/PSP2 milestones have been observed **ahead of their formal approvals**; their execution is documented but does not automatically close gates.

**Historical recovery update (2026-10-15):** the owner confirmed gameplay, graphics, sound and save/reload in the Unity 5.6.4p3 editor. An EXE Windows standalone is optional, but the full P03/P04/P06 acceptance matrices remain required. **Subsequent evidence (2026-10-16/17):** Unity 2018.2.19f1 PSP2 builds and owner-reported Vita gameplay/audio are documented, as are corrupt/lost Vita saves and severe FPS drops. Read the [dated retrospective audit](PHASE_AUDIT_2026-10-17.md) rather than interpreting the original October 15 notes as current status.

**Declared PS Vita identity:** `HGSKE1178` / **Hungry Shark Evolution Vita** ([single-source JSON](../config/port_identity.json)); see [APK user-data and filesystem study](DATA_EXTRACTION_AND_LAYOUT.md). This declaration does not approve later Unity/PSP2 phase work.

## Engine order (MUST NOT be reversed)

1. **Original game 6.4.2** is a **Unity 5.6.4p3 / Mono** Android build. Unity officially released 5.6.4p3 **2017-11-20**, before the 2018 release family.
2. **First**, rebuild/recover a Unity 5.6.4p3 editor project, repair references, make core **desktop gameplay work**, and freeze a reproducible baseline. Recovery is not equal to having original Unity source.
3. **Only after approved Unity 5.6.4p3 gameplay**, clone that project and migrate **the clone** into **Unity 2018.2** (candidate editor 2018.2.19f1, subject to verifying PSP2 installation). Prove desktop gameplay parity again.
4. **Only after approved Unity 2018.2 desktop gameplay**, build with a working **PSP2 Unity player/module** and test on physical PS Vita. Having an old public editor does *not* grant PSP2 build support.

Official Unity references: [5.6.4p3](https://unity.com/releases/editor/whats-new/5.6.4p3), [2018.2.19f1](https://unity.com/releases/editor/whats-new/2018.2.19f1), [2018.2 release](https://unity.com/blog/engine-platform/2018-2-now-available). Unity's PSP2 build target is marked unsupported **starting at 2018.3**; **this does not guarantee PSP2 is installed in 2018.2**. [UnityCsReference evidence](https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/BaseClass.cs).

**UnityScript caution:** the APK includes `Assembly-UnityScript*.dll`. Existing compiled assemblies, recovered `.js` sources, conversion tools and Unity 2018.2 compatibility have different constraints. Inventory what each component actually does, preserve serialized identities and migrate only as needed. Do **not** claim automatic UnityScript conversion succeeds.

## Retrospective acceptance details (2026-10-17)

The owner authorized this phase audit and approval of sufficiently demonstrated work. **P00:** successful Play Mode execution of the recovered real game in Unity 5.6 was accepted as a *stronger equivalent* to the older blank-project smoke check; no claim is made that a blank-scene test actually occurred. The detailed Android gameplay reference matrix is still owed at **P03**. **P01:** extraction/export inventory and known unresolved objects are documented rather than silently called restored. **P02:** main menu/gameplay compile and essential components are demonstrated while 55 uGUI references, eight null scripts in six prefabs and a layout mismatch remain to be triaged, never described as a globally clean project. Owner-authorization record: [issue #2](https://github.com/VegettoSan/Hungry-Shark-Evolution-Ps-Vita/issues/2); evidence: [phase audit](PHASE_AUDIT_2026-10-17.md).

These limited acceptances do not waive any remaining acceptance conditions of P03–P12. Earlier Unity 2018/PSP2 tests were **owner-requested experiments executed before chronological phase closure**; retain that historical truth. New deviations need fresh owner authorization and must be logged.

## Hard stop / gate approval protocol

- One active phase maximum. No parallel implementation in later stages, no "skip", no pre-emptive downgrade to another engine, no fictitious PASS.
- `ACTIVE` (work permitted) → `AWAITING_APPROVAL` (tests/evidence ready) → `APPROVED` **only when the repository owner expressly accepts the documented result**. A failure sets `BLOCKED`, and the active phase remains blocked until a documented corrective test.
- For `APPROVED`, record **both** reproducible, sanitized evidence references (minimum two), an owner approval reference (GitHub issue/PR/comment URL), `approved_by: "VegettoSan"`, and UTC `approved_at`; preserve the previous experiment entry.
- Once **and only once** phase N is `APPROVED`, phase N+1 becomes `ACTIVE`. The next phases remain `LOCKED`.
- CI runs `tools/check_phase_gates.py` and checks a valid sequential ledger; it cannot independently verify real gameplay or that a GitHub URL truly represents consent. **Human review is mandatory.** For enforceable merge restrictions, set branch protection/rulesets to require the `phase-gates` status check and maintainer review.
- Every attempt (including FAIL/BLOCKED) goes into [`EXPERIMENTS.md`](EXPERIMENTS.md) with precise tool version, source hash, setting, observed result and evidence. Logs must not contain copyrighted assets.
- **Owner-approved validation-format change (2026-10-15):** the owner explicitly considers a Windows standalone executable unnecessary for this Vita port if the original Unity engine plays properly inside the editor; PC-level P03/P04 and P06 may therefore use documented **Unity Editor Play Mode** as an alternative to building a Windows EXE. This does **not** waive a complete gameplay matrix, cold-start save/load, clean reproduction, Android comparison, logs, owner approval or real hardware PSP2 testing. See [handoff](UNITY56_BASELINE_AND_UNITY2018_HANDOFF.md).
- **Other exceptions:** if a phase is impossible, do NOT auto-approve. Present its evidence and proposed, narrowly scoped acceptance-criteria change to the owner; only the owner can authorize revising this plan before re-running that phase. Never weaken gates to avoid work.

### Evidence package for closing each gate

Record (a) commit SHA + source input hash, (b) editor/AssetRipper/SDK build version, (c) commands and settings, (d) raw observed PASS/FAIL results plus sanitized log, (e) screenshot/video link or structured test transcript where appropriate, (f) regression comparison, and (g) explicit owner approval link. Use `docs/EXPERIMENTS.md` for short logs and optional linked issue attachments that do not distribute game content.

## Milestones, exact order

### P00 — Reproducible source and original editor setup (APPROVED retrospectively)

**Do:** Verify owned source 6.4.2 APK hash against [audit](TECHNICAL_AUDIT.md), ZIP CRC and `5.6.4p3` header. Install **exact Unity 5.6.4p3** from official archive, verify editor starts and a blank desktop project runs. Locally reproduce Android version gameplay for a reference matrix (menu, load, movement, feeding, health/death, save, audio); capture notes/screenshots privately. Check current Unity **2018.2.19f1** + PSP2 toolchain against the historical [Halo 2D evidence](HALO2D_PSP2_REFERENCE.md); record this environment inventory **without attempting a migration**, and record the complete [tool installation/inventory checklist](TOOLS.md). Do not infer PSP2 availability from a standard public Unity installer.

**Acceptance:** APK hash and Unity version verified; editor version screen/log and PC blank scene execution demonstrated; baseline gameplay observations documented; environment constraints recorded. **Static APK audit alone does not close the gate.**

### P00 scoped offline recovery staging authorization (owner request 2026-10-14)

The repository owner expressly requested **an unattended local preparation of the original Unity 5.6.4p3 project BEFORE installing Unity 5.6**, as described in [`docs/CODEX_UNITY56_OFFLINE_EXPORT_PROMPT.md`](CODEX_UNITY56_OFFLINE_EXPORT_PROMPT.md). This is a **limited preparatory experiment during P00**, not approval or transition of P00/P01/P02. It permits *offline-only* APK hash/CRC checks, private full extraction, AssetRipper project export, managed-assembly/static reference inventory, and safe reversible **static fixes** in a private copy. **No Unity Editor execution, verified compilation/gameplay claim, altered mechanics, shader optimization, Unity 2018 migration, PSP2 code, user-release extractor or VPK.**

Codex may carry out the preparatory extraction despite P01's normal gate **solely because the owner explicitly authorized that scope**; recorded stage state remains P00 ACTIVE, P01/P02 LOCKED and no gate may be marked APPROVED. Do not use this exception to claim later deliverables PASS or circumvent the signed-off editor/device criteria. When Unity 5.6 becomes available, the owner must still satisfy the P00 validation gate, explicitly approve it, and then validate P01's already prepared evidence normally. Never auto-promote stages. Private outputs remain outside Git.

### Follow-up owner requests: focused Unity 5.6 repairs (2026-10-15)

After installing Unity 5.6.4p3, the owner expressly requested repairs for compiler diagnostics, NGUI, desktop Android-service boundaries, shader/material recovery, camera initialization, Windows input/turbo, startup/cleanup exceptions and model completeness. These requests authorized those private repairs and targeted editor checks, with backups and diffs. They superseded the initial offline-only restriction **for those requested operations**, without approving P00/P01/P02 or authorizing migration, PSP2 work, release packaging or speculative gameplay changes.

The [recovery status](RECOVERY_STATUS_2026-10-15.md) and [experiments](EXPERIMENTS.md) record what was actually observed, including failed or incomplete probes. The original offline checkpoint remains historical. Existing preparatory results may be reviewed when their phase opens; they do not replace its acceptance tests or explicit owner approval.

### P01 — Extract everything needed for recovery (APPROVED retrospectively)

**Do:** Run our `tools/Preparar_Hungry_Shark_Evolution_6.4.2.py` locally, verify reconstructed 15 `.splitN` groups, preserve original `Data/Managed` assemblies. Import Unity serialized files **with managed assemblies** into AssetRipper as a single source set. Export into disposable Unity 5.6 project. Use ILSpy on recovered game scripts and note types, field names, scene `MonoBehaviour` links, GUID/fileID mapping, 18 serialized `levelN` files, textures, animation, shaders, audio, prefabs and dependencies.

**Acceptance:** complete inventory, AssetRipper log including Mono backend detection, export result, count/list of unresolved script references and damaged assets, and a reproducible import recipe. Do not claim complete project recovery merely because export succeeded.

### P02 — Restore buildable Unity 5.6.4p3 editor project (APPROVED retrospectively, open references tracked)

**Do:** Open **only** recovered baseline in exact Unity 5.6.4p3. Reconstruct necessary C# and UnityScript-dependent code without changing game mechanics. Resolve serialized script references by verified type/assembly/GUID; repair scenes/prefabs, scene index, materials and dependencies. Wrap Android-specific APIs minimally; avoid accidental game logic removal. Keep original recovery snapshot read-only; maintain changes as a separate working project.

**Acceptance:** editor imports, an agreed minimal scene loads, **zero unaccounted compile errors**, and essential scene components have verified references rather than `Missing Script`. Noncritical unresolved items must be explicitly inventoried, never silently discarded.

### P03 — Restore actual Unity 5.6.4p3 PC gameplay (ACTIVE)

**Do:** Verify start → menu → start game → player shark moves → prey/food interaction → hunger/health progression → death/retry → UI → audio → save/load; test a fresh profile and a continued profile. Compare movement, timing and behavior with original Android baseline; only platform-specific services may be adapted. Desktop validation may run entirely in **Unity 5.6.4p3 editor Play Mode**, without producing a standalone Windows executable, per the owner's documented decision. **A screenshot by itself is not sufficient**: record the complete scenario matrix, editor settings, sanitized console/video or structured test transcript, save persisted across a restart, and Android comparison.

**Acceptance:** an end-to-end **offline core gameplay loop** operates on desktop without fatal errors; each reference-matrix case has recorded PASS and comparison evidence. Offline store/ads not required but their exclusions must be documented.

### P04 — Freeze faithful original-engine reference (LOCKED)

**Do:** Repeat P03 after a clean import into a **separate project copy** and a fresh save, with editor Play Mode test receipt as the approved alternative to a standalone PC build. Confirm save after closing/reopening the editor/game; catalog original-versus-PC divergences, eliminated hacks and known non-core exclusions. Archive a private immutable Unity 5.6 source-project snapshot, its manifest/hashes, sanitized editor/test receipt and regression checklist. Do not change the 5.6 baseline after approval; create a separate migration clone **only when P05 becomes ACTIVE**.

**Acceptance:** repeatable stable PC session, no regression to core mechanics, explicit owner confirmation **"Unity 5.6.4p3 native baseline accepted"**. **P05 is forbidden before this gate.**

### P05 — Migration clone to Unity 2018.2 (LOCKED)

**Do:** Copy (never upgrade in place) the approved Unity 5.6 project. Use the declared application name **Hungry Shark Evolution Vita** and prepare **HGSKE1178** for later PSP2 package identity, without copying Halo 2D credentials/content identifiers. Prefer the verified Halo 2D **2018.2.19f1** editor (do not force 2018.2.21f1); pin its exact version and identify the actually working PSP2 target module locally. Handle engine API changes, assemblies, serialization and UnityScript migration; keep GUID/fileID mappings intact. Inventory recovered shader/material names and attempt faithful shader compilation/visual parity before proposing simplifications; the [PS Vita shader research](SHADERS_PSVITA_REFERENCE.md) is **reference, not automatic rewrite criteria**. The official UnityScript-to-C# converter documents a Unity 2017.3–2018.1.x support window; any temporary 2018.1 bridge is **conditional** and must not bypass the approved original-engine baseline. Compile a representative test scene on PC and audit all warnings/errors.

**Acceptance:** native Unity 2018.2 project imports and compiles with zero unaccounted errors, scenes/sprite/prefab references verified, differences documented. **Merely opening in 2018.2 is not acceptance.**

### P06 — Prove Unity 2018.2 PC gameplay parity (LOCKED)

**Do:** Re-run the complete frozen P03/P04 regression suite in **Unity 2018.2 editor Play Mode** (or optionally a PC standalone build), compare original-engine behaviors, saves, HUD, touch simulation, collisions, animation, sound and scene changes. Include side-by-side shader/material effects and underwater transparency using the [rendering reference](SHADERS_PSVITA_REFERENCE.md). Fix and repeat any regression **before** starting Vita work.

**Acceptance:** same approved offline gameplay scenarios run on PC with documented equivalence (allowing explained rendering differences); repeated documented desktop Editor Play Mode session **or** optional PC executable demonstrated. Owner explicitly approves 2018.2 desktop parity. **P07 forbidden until approved.**

### P07 — Independent native PSP2 toolchain smoke test (LOCKED)

**Do:** Using the confirmed Unity 2018.2 installation, build a minimal **independent** test scene to PSP2. Log `Application.dataPath`, `Application.streamingAssetsPath`, `Application.persistentDataPath` and check `app0:` read-only access on physical Vita to determine real PSP2 layout. Verify device executable packaging, actual PS Vita launch, GPU first frame, input, audio placeholder, app suspend/resume and logging. Do not confuse custom homebrew VitaSDK with the proprietary Unity PSP2 module.

**Acceptance:** photo/log from **physical Vita** showing Unity PSP2 sample running; toolchain/platform version recorded. If the module cannot legally be obtained or used, **BLOCKED**, not assumed solved.

### P08 — Hungry Shark boot/UI on physical Vita (LOCKED)

**Do:** Inspect actual VPK built files under installed app `ux0:app/HGSKE1178/`, and verify an additional user-data relative path only if a loader is necessary. Do not overwrite Unity-generated PSP2 player data with original Android serialized files. Build the first real recovered game scene for PSP2; load Unity resources with monitored RAM/VRAM. Validate real **PSP2 shader compilation and material correctness**, including pink/missing effects, alpha, depth, lighting and water, and fix shader/material API and startup platform dependencies carefully. Preserve camera bounds and correct 960×544 presentation and menu controls.

**Acceptance:** stable boot → title/menu → transition to gameplay scene on physical hardware with logs and screenshots; no unrelated placeholder passed off as the game.

### P09 — Core gameplay on physical Vita (LOCKED)

**Do:** Restore shark locomotion, collision, spawn, food/feeding, health, death/retry, camera and HUD. Run a comparable short gameplay segment using **a minimal, tested Vita input path** (for example an already existing compatible legacy axis) to prove gameplay; preserve mechanics verified on PC. **This is not the P10 complete control integration**: the full standardized Halo 2D Input Manager merge, all button aliases, dead-zone calibration and touchscreen regression testing remain P10.

**Acceptance:** repeatable playable core offline loop on Vita without crash; video/logs and comparison with P06 supplied.

### P10 — Controls/audio/save and Vita lifecycle (LOCKED)

**Do:** Prefer **Halo 2D's standard Unity 2018.2.19f1 Input Manager bindings** as documented in [the verified mapping](HALO2D_PSP2_REFERENCE.md#reusing-halo-2ds-standard-unity-input-manager-verified-p10-reference-only). Back up HSE's project settings and **merge only necessary alternative axis/button entries**; never replace HSE InputManager wholesale, assume physical button numbers, or import Halo's custom VitaInputManager script unreviewed. Preserve original HSE keyboard/mouse/touch behavior, use built-in `Input.GetAxis/GetButton` where its original consumers support them, and validate direction/dead zone and duplicates on physical Vita. Map sticks/D-pad/face buttons and optional touch to existing game input paths, without rewriting controllers. Validate audio, music and effects, virtual keyboard only if required, saves through Vita-compatible storage, cold launch, suspend/resume and clean exit. Preserve save semantics; record Android-only feature exclusions.

**Acceptance:** physical controller test matrix shows joystick axes including invert/dead zone, D-pad, action/menu buttons, touch and desktop fallback correctly mapped without gameplay changes; zero missing-axis exceptions and no broken original input. Controls and audio matrix PASS; save survives process restart; suspend/resume verified; gameplay still passes regression tests.

### P11 — Performance, memory and long-run reliability (LOCKED)

**Do:** Aim for **30 FPS stable in real gameplay (33.33 ms/frame)** as the primary design goal; **60 FPS (16.67 ms/frame)** is an optional stretch goal only when feasible. Neither target is a demonstrated result. Measure real Vita FPS, stutter, allocations, scene transitions, RAM/VRAM and GPU faults using baseline and repeatable scenes. Apply **one measured optimization at a time** (textures, LOD, draw calls, shader choices, pooled data), using the [optional shader trials and A/B methodology](SHADERS_PSVITA_REFERENCE.md) when relevant, with before/after logs and gameplay regression checks. Never turn off correctness checks merely to gain frames.

**Acceptance:** attempt the **30 FPS stable gameplay** goal, investigate **60 FPS** if benchmarks justify it, and use an **owner-approved, evidence-based** FPS/stability acceptance threshold after collecting a real-device baseline. Both numerical targets are *design goals*, not measured facts or blanket shader rules. Requirements may only change via owner-approved plan revision; all compromises documented, no fatal crashes or gameplay regressions.

### P12 — Release readiness and handoff (LOCKED)

**Do:** After obtaining a real tested content dependency manifest and verifying legitimate conversion/distribution, evaluate **either a deterministic end-user APK converter or stub VPK + xdelta3** ([Predators / Pocket Mortys reference](XDELTA_UNITY_PORTS_RESEARCH.md)); only implement a user-owned APK data extractor **if the runtime supports separately provided assets**, and no chosen patch bypasses ownership/redistribution rights; verify output checksums, correct app-path staging, missing-data reporting and reinstall data preservation. Never advertise Android binary Unity Data as PSP2-compatible without tests. Perform clean reproducible build on a known environment, full physical Vita regression, install/uninstall/save tests, version documentation, troubleshooting, issue template, tested limitations and attribution. Define lawful distribution: repository scripts and documentation may be shared; proprietary game data and restricted SDK/runtime require legal authorization. If rights are not established, ship no unauthorized proprietary binary/VPK; prefer a legally compliant end-user conversion workflow where permissible.

**Acceptance:** owner explicitly signs release checklist, physical Vita evidence exists, copyright/license constraints satisfied, release notes accurate. If xdelta is selected, test exact original APK SHA-256, patched output archive SHA-256, archive integrity, Windows compatibility, app-relative copy path, save preservation on update and legality of patch content. **Not a production release until all checks pass.**

## Platform acceptance matrix

| Scenario | Android reference P00 | Unity 5.6 PC P03–04 | Unity 2018.2 PC P06 | Vita P09–12 |
| --- | --- | --- | --- | --- |
| Start and menu | Record | PASS | PASS | PASS |
| Gameplay loads | Record | PASS | PASS | PASS |
| Shark movement/camera | Record | PASS | PASS | PASS |
| Eat prey / health / death | Record | PASS | PASS | PASS |
| Retry / UI / progression | Record | PASS | PASS | PASS |
| Sound and music | Record | PASS | PASS | PASS |
| Save and reload | Record | PASS | PASS | PASS |
| Offline without advertising/purchases | Record | Document exclusions | PASS | PASS |
| Vita controls + suspend/resume | n/a | n/a | Simulation | PASS |

This plan is intentionally strict: if any phase is blocked, continue **within that phase**, gather evidence and obtain explicit owner approval before advancing.

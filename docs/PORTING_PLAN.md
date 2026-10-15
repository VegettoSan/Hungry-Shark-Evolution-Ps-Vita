# End-to-end gated development plan — Hungry Shark Evolution PS Vita

**Authoritative phase ledger:** [`PHASE_GATES.json`](PHASE_GATES.json) · **Validation:** `python tools/check_phase_gates.py`

**Status:** `P00 ACTIVE`; **all subsequent phases LOCKED**. Prior APK static research is evidence, **not** user approval of P00.

## Engine order (MUST NOT be reversed)

1. **Original game 6.4.2** is a **Unity 5.6.4p3 / Mono** Android build. Unity officially released 5.6.4p3 **2017-11-20**, before the 2018 release family.
2. **First**, rebuild/recover a Unity 5.6.4p3 editor project, repair references, make core **desktop gameplay work**, and freeze a reproducible baseline. Recovery is not equal to having original Unity source.
3. **Only after approved Unity 5.6.4p3 gameplay**, clone that project and migrate **the clone** into **Unity 2018.2** (candidate editor 2018.2.19f1, subject to verifying PSP2 installation). Prove desktop gameplay parity again.
4. **Only after approved Unity 2018.2 desktop gameplay**, build with a working **PSP2 Unity player/module** and test on physical PS Vita. Having an old public editor does *not* grant PSP2 build support.

Official Unity references: [5.6.4p3](https://unity.com/releases/editor/whats-new/5.6.4p3), [2018.2.19f1](https://unity.com/releases/editor/whats-new/2018.2.19f1), [2018.2 release](https://unity.com/blog/engine-platform/2018-2-now-available). Unity's PSP2 build target is marked unsupported **starting at 2018.3**; **this does not guarantee PSP2 is installed in 2018.2**. [UnityCsReference evidence](https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/BaseClass.cs).

**UnityScript caution:** the APK includes `Assembly-UnityScript*.dll`. Existing compiled assemblies, recovered `.js` sources, conversion tools and Unity 2018.2 compatibility have different constraints. Inventory what each component actually does, preserve serialized identities and migrate only as needed. Do **not** claim automatic UnityScript conversion succeeds.

## Hard stop / gate approval protocol

- One active phase maximum. No parallel implementation in later stages, no "skip", no pre-emptive downgrade to another engine, no fictitious PASS.
- `ACTIVE` (work permitted) → `AWAITING_APPROVAL` (tests/evidence ready) → `APPROVED` **only when the repository owner expressly accepts the documented result**. A failure sets `BLOCKED`, and the active phase remains blocked until a documented corrective test.
- For `APPROVED`, record **both** reproducible, sanitized evidence references (minimum two), an owner approval reference (GitHub issue/PR/comment URL), `approved_by: "VegettoSan"`, and UTC `approved_at`; preserve the previous experiment entry.
- Once **and only once** phase N is `APPROVED`, phase N+1 becomes `ACTIVE`. The next phases remain `LOCKED`.
- CI runs `tools/check_phase_gates.py` and checks a valid sequential ledger; it cannot independently verify real gameplay or that a GitHub URL truly represents consent. **Human review is mandatory.** For enforceable merge restrictions, set branch protection/rulesets to require the `phase-gates` status check and maintainer review.
- Every attempt (including FAIL/BLOCKED) goes into [`EXPERIMENTS.md`](EXPERIMENTS.md) with precise tool version, source hash, setting, observed result and evidence. Logs must not contain copyrighted assets.
- **Exceptions:** if a phase is impossible, do NOT auto-approve. Present its evidence and proposed, narrowly scoped acceptance-criteria change to the owner; only the owner can authorize revising this plan before re-running that phase. Never weaken gates to avoid work.

### Evidence package for closing each gate

Record (a) commit SHA + source input hash, (b) editor/AssetRipper/SDK build version, (c) commands and settings, (d) raw observed PASS/FAIL results plus sanitized log, (e) screenshot/video link or structured test transcript where appropriate, (f) regression comparison, and (g) explicit owner approval link. Use `docs/EXPERIMENTS.md` for short logs and optional linked issue attachments that do not distribute game content.

## Milestones, exact order

### P00 — Reproducible source and original editor setup (ACTIVE)

**Do:** Verify owned source 6.4.2 APK hash against [audit](TECHNICAL_AUDIT.md), ZIP CRC and `5.6.4p3` header. Install **exact Unity 5.6.4p3** from official archive, verify editor starts and a blank desktop project runs. Locally reproduce Android version gameplay for a reference matrix (menu, load, movement, feeding, health/death, save, audio); capture notes/screenshots privately. Check current Unity **2018.2.19f1** + PSP2 toolchain against the historical [Halo 2D evidence](HALO2D_PSP2_REFERENCE.md); record this environment inventory **without attempting a migration**, and record the complete [tool installation/inventory checklist](TOOLS.md). Do not infer PSP2 availability from a standard public Unity installer.

**Acceptance:** APK hash and Unity version verified; editor version screen/log and PC blank scene execution demonstrated; baseline gameplay observations documented; environment constraints recorded. **Static APK audit alone does not close the gate.**

### P01 — Extract everything needed for recovery (LOCKED)

**Do:** Run our `tools/Preparar_Hungry_Shark_Evolution_6.4.2.py` locally, verify reconstructed 15 `.splitN` groups, preserve original `Data/Managed` assemblies. Import Unity serialized files **with managed assemblies** into AssetRipper as a single source set. Export into disposable Unity 5.6 project. Use ILSpy on recovered game scripts and note types, field names, scene `MonoBehaviour` links, GUID/fileID mapping, 18 serialized `levelN` files, textures, animation, shaders, audio, prefabs and dependencies.

**Acceptance:** complete inventory, AssetRipper log including Mono backend detection, export result, count/list of unresolved script references and damaged assets, and a reproducible import recipe. Do not claim complete project recovery merely because export succeeded.

### P02 — Restore buildable Unity 5.6.4p3 editor project (LOCKED)

**Do:** Open **only** recovered baseline in exact Unity 5.6.4p3. Reconstruct necessary C# and UnityScript-dependent code without changing game mechanics. Resolve serialized script references by verified type/assembly/GUID; repair scenes/prefabs, scene index, materials and dependencies. Wrap Android-specific APIs minimally; avoid accidental game logic removal. Keep original recovery snapshot read-only; maintain changes as a separate working project.

**Acceptance:** editor imports, an agreed minimal scene loads, **zero unaccounted compile errors**, and essential scene components have verified references rather than `Missing Script`. Noncritical unresolved items must be explicitly inventoried, never silently discarded.

### P03 — Restore actual Unity 5.6.4p3 PC gameplay (LOCKED)

**Do:** Verify start → menu → start game → player shark moves → prey/food interaction → hunger/health progression → death/retry → UI → audio → save/load; test a fresh profile and a continued profile. Compare movement, timing and behavior with original Android baseline; only platform-specific services may be adapted. Produce a PC executable, not just a Unity editor screenshot.

**Acceptance:** an end-to-end **offline core gameplay loop** operates on desktop without fatal errors; each reference-matrix case has recorded PASS and comparison evidence. Offline store/ads not required but their exclusions must be documented.

### P04 — Freeze faithful original-engine reference (LOCKED)

**Do:** Repeat P03 after clean import/build and fresh save. Catalog original-versus-PC divergences, eliminated hacks and known non-core exclusions. Archive local clean working Unity 5.6 project and sanitized build receipt, lock tests and commit checkpoint. Do not change the 5.6 baseline after approval; create a separate migration clone.

**Acceptance:** repeatable stable PC session, no regression to core mechanics, explicit owner confirmation **"Unity 5.6.4p3 native baseline accepted"**. **P05 is forbidden before this gate.**

### P05 — Migration clone to Unity 2018.2 (LOCKED)

**Do:** Copy (never upgrade in place) the approved Unity 5.6 project. Prefer the verified Halo 2D **2018.2.19f1** editor (do not force 2018.2.21f1); pin its exact version and identify the actually working PSP2 target module locally. Handle engine API changes, assemblies, serialization and UnityScript migration; keep GUID/fileID mappings intact. The official UnityScript-to-C# converter documents a Unity 2017.3–2018.1.x support window; any temporary 2018.1 bridge is **conditional** and must not bypass the approved original-engine baseline. Compile a representative test scene on PC and audit all warnings/errors.

**Acceptance:** native Unity 2018.2 project imports and compiles with zero unaccounted errors, scenes/sprite/prefab references verified, differences documented. **Merely opening in 2018.2 is not acceptance.**

### P06 — Prove Unity 2018.2 PC gameplay parity (LOCKED)

**Do:** Re-run the complete frozen P03/P04 regression suite in the 2018.2 desktop build, compare original-engine behaviors, saves, HUD, touch simulation, collisions, animation, sound and scene changes. Fix and repeat any regression **before** starting Vita work.

**Acceptance:** same approved offline gameplay scenarios run on PC with documented equivalence (allowing explained rendering differences); real PC executable demonstrated. Owner explicitly approves 2018.2 desktop parity. **P07 forbidden until approved.**

### P07 — Independent native PSP2 toolchain smoke test (LOCKED)

**Do:** Using the confirmed Unity 2018.2 installation, build a minimal **independent** test scene to PSP2. Verify device executable packaging, actual PS Vita launch, GPU first frame, input, audio placeholder, app suspend/resume and logging. Do not confuse custom homebrew VitaSDK with the proprietary Unity PSP2 module.

**Acceptance:** photo/log from **physical Vita** showing Unity PSP2 sample running; toolchain/platform version recorded. If the module cannot legally be obtained or used, **BLOCKED**, not assumed solved.

### P08 — Hungry Shark boot/UI on physical Vita (LOCKED)

**Do:** Build the first real recovered game scene for PSP2; load Unity resources with monitored RAM/VRAM. Fix shader/material API and startup platform dependencies carefully. Preserve camera bounds and correct 960×544 presentation and menu controls.

**Acceptance:** stable boot → title/menu → transition to gameplay scene on physical hardware with logs and screenshots; no unrelated placeholder passed off as the game.

### P09 — Core gameplay on physical Vita (LOCKED)

**Do:** Restore shark locomotion, collision, spawn, food/feeding, health, death/retry, camera and HUD. Run a comparable short gameplay segment using **a minimal, tested Vita input path** (for example an already existing compatible legacy axis) to prove gameplay; preserve mechanics verified on PC. **This is not the P10 complete control integration**: the full standardized Halo 2D Input Manager merge, all button aliases, dead-zone calibration and touchscreen regression testing remain P10.

**Acceptance:** repeatable playable core offline loop on Vita without crash; video/logs and comparison with P06 supplied.

### P10 — Controls/audio/save and Vita lifecycle (LOCKED)

**Do:** Prefer **Halo 2D's standard Unity 2018.2.19f1 Input Manager bindings** as documented in [the verified mapping](HALO2D_PSP2_REFERENCE.md#reusing-halo-2ds-standard-unity-input-manager-verified-p10-reference-only). Back up HSE's project settings and **merge only necessary alternative axis/button entries**; never replace HSE InputManager wholesale, assume physical button numbers, or import Halo's custom VitaInputManager script unreviewed. Preserve original HSE keyboard/mouse/touch behavior, use built-in `Input.GetAxis/GetButton` where its original consumers support them, and validate direction/dead zone and duplicates on physical Vita. Map sticks/D-pad/face buttons and optional touch to existing game input paths, without rewriting controllers. Validate audio, music and effects, virtual keyboard only if required, saves through Vita-compatible storage, cold launch, suspend/resume and clean exit. Preserve save semantics; record Android-only feature exclusions.

**Acceptance:** physical controller test matrix shows joystick axes including invert/dead zone, D-pad, action/menu buttons, touch and desktop fallback correctly mapped without gameplay changes; zero missing-axis exceptions and no broken original input. Controls and audio matrix PASS; save survives process restart; suspend/resume verified; gameplay still passes regression tests.

### P11 — Performance, memory and long-run reliability (LOCKED)

**Do:** Measure real Vita FPS, stutter, allocations, scene transitions, RAM/VRAM and GPU faults using baseline and repeatable scenes. Apply **one measured optimization at a time** (textures, LOD, draw calls, shader choices, pooled data), with before/after logs and gameplay regression checks. Never turn off correctness checks merely to gain frames.

**Acceptance:** owner-approved device-specific FPS/stability target defined from **measured baseline**, met in multiple test sessions; no fatal crashes or gameplay regressions, all compromises documented. Do not invent a target number before hardware measurements.

### P12 — Release readiness and handoff (LOCKED)

**Do:** Perform clean reproducible build on a known environment, full physical Vita regression, install/uninstall/save tests, version documentation, troubleshooting, issue template, tested limitations and attribution. Define lawful distribution: repository scripts and documentation may be shared; proprietary game data and restricted SDK/runtime require legal authorization. If rights are not established, ship no unauthorized proprietary binary/VPK; prefer a legally compliant end-user conversion workflow where permissible.

**Acceptance:** owner explicitly signs release checklist, physical Vita evidence exists, copyright/license constraints satisfied, release notes accurate. **Not a production release until all checks pass.**

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

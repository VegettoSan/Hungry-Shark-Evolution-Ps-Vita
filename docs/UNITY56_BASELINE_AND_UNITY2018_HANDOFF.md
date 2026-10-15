# Unity 5.6 accepted-technical-results register and 2018 handoff

**Date: 2026-10-15.** This is a **preparation and handoff checklist**, not proof of a completed baseline, gate approval or Unity 2018 conversion. The exact engine baseline remains *Hungry Shark Evolution 6.4.2*, Android package `com.fgol.HungrySharkEvolution` v228, Unity **5.6.4p3 / Mono** and source APK SHA-256 `52b7d8754ab8fec75c3b656a0dc5ec8dc849565adc402487a3ac093f6a481f7a`.

## Explicit owner feedback and evidence precision

In the 2026-10-15 conversation, the repository owner expressly confirmed:

| Item | Present evidence | Do not claim without further tests |
| --- | --- | --- |
| Core gameplay | **OWNER-CONFIRMED:** gameplay works in Unity **5.6.4p3** editor (prior reports include menu/gameplay and movement by keyboard/mouse) | Every gameplay state, Android behavioral parity, standalone PC executable, long-running stability |
| Shaders and graphics | **OWNER-CONFIRMED:** shaders and graphics look/function correctly during the tested session; previous editor shader/material probes also exist | All variant/scene parity, exact gold-frenzy color comparison, PSP2 shader support |
| Sound | **OWNER-CONFIRMED:** game audio plays after toggling the original in-game audio option; a Unity editor audio probe captured nonzero signal | All audio variations, option persistence after cold restart |
| Save and reload | **OWNER-CONFIRMED:** saved progress was recovered after closing and reopening the Unity 5.6 game/editor, as explicitly confirmed by the owner. | Independently logged reproducibility, separate profiles and PS Vita storage remain untested |
| Models | **OWNER-CONFIRMED:** sharks including Mosasaurus visibly appear, with reference inventory from Codex | All animations, rig/state permutations |
| Compilation | Unity 5.6 code compiles in documented editor and isolated checks | Fresh-copy clean import with classified console warnings, public distribution or full standalone Windows build |

Evidence labels are intentionally distinct: **OWNER-CONFIRMED** is a real direct report from the user, **VERIFIED** requires a preserved test/log receipt, **UNTESTED** has no recorded reproducible run. Do not expand the user's claims beyond these boundaries. The prior [recovery report](RECOVERY_STATUS_2026-10-15.md) and [experiment ledger](EXPERIMENTS.md) remain authoritative for detailed technical traces.

## Owner decision: editor-based PC validation is acceptable

The owner does **not** want a Windows standalone executable solely to serve as a gate to porting for PS Vita. For **P03/P04**, a **repeatable, documented, end-to-end offline gameplay session in Unity 5.6.4p3 editor Play Mode**, with a clean reimport/restart and a full scenario matrix, may fulfill the desktop baseline requirements. A Windows executable is **optional**, not claimed to exist.

Apply the **same editor-based alternative to P06** in Unity 2018.2.19f1 for parity, *before* PSP2 is attempted. This only changes **delivery format**, not the required fidelity, repeatability, gameplay scenarios, evidence, owner acceptance, phase order or PSP2 physical test. The plan was updated accordingly at the owner's request. If critical behaviors differ between Play Mode and player, record a risk; the first actual PSP2 executable must still run on hardware in P07+.

## Remaining evidence before freezing Unity 5.6

- [ ] **P00:** confirm the exact editor version and run a blank Unity 5.6 PC scene, register the Android reference gameplay matrix and target editor/PSP2 tool availability; explicitly approve P00 with evidence.
- [ ] **P01:** audit source/export proofs, 18 scenes, managed type/script mapping, noncritical missing references; explicitly approve P01.
- [ ] **P02:** fresh editor import, check console for unaccounted C# errors, test agreed `SplashLoaderScene` and `MainMenu`, triage remaining **55 uGUI references**, **8 null scripts across 6 prefabs** and **MissionAchievementsManager layout mismatch** as fixed or documented noncritical issues; explicitly approve P02.
- [ ] **P03:** run the documented Play Mode matrix (menu → loading → movement/camera → food → health/hunger → death/restart → UI/audio → save/load); record individual PASS/FAIL, Android comparison notes and excluded online features. Observe boost explicitly with a held input. **No Windows EXE required.** Explicitly approve P03.
- [ ] **P04:** repeat on a **separate clean-copy import** and a fresh save, then close and relaunch editor/game to reproduce the owner-confirmed save persistence with a logged test; observe exceptions/warnings, compare visual states and audio, preserve sanitized logs/screenshots. Preserve the project as an immutable source-of-truth backup, verify contents/hashes and obtain the owner's literal acceptance **"Unity 5.6.4p3 native baseline accepted"** in an issue/PR comment. Approve P04 only with required owner approval link and references.
- [ ] If present: investigate `m_AssetToPrefab.empty()` with the exact reproducible prefab/import context, not destructive prefab repair. Gold-frenzy shader v2 comparison is still untested; preserve prior FAIL until a comparable test closes it. Missing preloads/lighting messages may need a documented classification.

Owner's confirmation of save persistence after reopening is positive functional evidence, but a second documented clean-project reproduction, the complete scenario matrix and formal phase approval remain outstanding. Do not mark any item PASS merely because the game entered Play Mode.

## Private immutable freeze layout (no copyrighted data in Git)

Private workspace (not a Git directory):

```text
G:\PsVita\My Ports\HSEVita-Local\
  Original_Data_Extracted\                      # unchanged original Data and Mono assemblies
  HSE642_AssetRipper_RawExport\                 # untouched source export
  HSE642_Unity56_ReadyToOpen\                   # current Unity 5.6 working copy
  Baselines\
    HSE642_Unity56_FROZEN\                      # immutable approved 5.6 baseline, when signed off
    HSE642_Unity2018_MIGRATION\                 # independent copy, P05 only
  Reports\
    Unity56_Baseline_Freeze_Manifest.json
    Unity56_Editor_Gameplay_Matrix.md
    Unity56_Final_Console_Sanitized.txt
    Unity56_Handoff_Receipt.md
```

**Do not create or open the Unity 2018 migration copy while P05 is locked.** The directory layout is a plan, not evidence those files exist. A private snapshot may be staged for backup without treating it as an approved P04 baseline; label it `CANDIDATE` until the gate is passed.

Freeze instructions for local Codex:

1. Close Unity (no simultaneous Unity processes touching the same project). Work exclusively on the private working copy; source export and APK stay intact.
2. Create a **separate** backup/copy including `Assets/` and all `.meta`, `ProjectSettings/`, and other actual project input directories if present. Do **not** copy volatile `Library/`, `Temp/`, `Obj/`, `Logs/` or IDE caches as canonical source. Never use destructive `robocopy /MIR`, `/PURGE`, overwrite an existing frozen backup, or move/rename the active project.
3. Record timestamp UTC, Unity editor path/version, hash and size for project-authored files, shader/material counts, scene order, `ProjectSettings/ProjectVersion.txt`, total .meta/GUID consistency and references to pre-existing checkpoints. Keep generated JSON/manifests **private** (paths and proprietary asset names must not leak into GitHub).
4. The frozen backup is **never modified** by the 2018 editor. Place the eventual migration in a **third, separate directory** only after P04 has explicit owner approval. Do not modify the source editor's input assets or original Mono APK assemblies.
5. Document the acceptance matrix and a change inventory. GitHub receives sanitized **documentation only**, never exported Unity assets, decompiled sources, licensed Unity/PSP2 binaries or credentials.

## Locked Unity 2018.2.19f1 handoff — when P05 is ACTIVE

- Read [Halo 2D PSP2 environment reference](HALO2D_PSP2_REFERENCE.md), [shader reference](SHADERS_PSVITA_REFERENCE.md), [porting plan](PORTING_PLAN.md) and [Codex P05 instructions](CODEX_UNITY2018_MIGRATION_PREP.md).
- Verify exact Unity **2018.2.19f1** is installed, and independently assess whether its PSP2 build module and lawful platform dependencies actually work; the existence of Halo 2D history does not prove current local PSP2 readiness.
- **Copy** the approved `HSE642_Unity56_FROZEN` to `HSE642_Unity2018_MIGRATION`. Ensure source GUID/fileID links, scenes, NGUI/ui, C#/UnityScript deserialized type identities, materials, render queues, effects and original local saves are inventoried.
- Upgrade **the copy** in 2018.2, capture compiler/import changes, handle API and shader differences minimally, avoid rewriting game systems or deleting Android compatibility layers without evidence.
- In P06, repeat the same gameplay+save+graphics matrix **within the 2018.2 editor if no standalone EXE is desired**. Only after approval proceed to independent native PSP2 tests (P07) and actual Vita game boot (P08).
- No assumption that Android Unity serialized `Data` can be placed directly inside PSP2 `app0:`. Final user-owned-APK extraction/patch decisions remain P12.

## Gate administration (not done automatically)

Current [PHASE_GATES.json](PHASE_GATES.json) remains **P00 ACTIVE, P01–P12 LOCKED, 0 approved**. When the acceptance conditions of **each phase** have real evidence, record at least **two** clean references, an explicit owner approval linked to an issue/PR comment, UTC time, then update only the relevant phase and next phase in order. Run `python tools/check_phase_gates.py`, review CI and preserve prior logs. A chat instruction to document or prepare does not constitute owner approval of **"Unity 5.6.4p3 native baseline accepted"**.

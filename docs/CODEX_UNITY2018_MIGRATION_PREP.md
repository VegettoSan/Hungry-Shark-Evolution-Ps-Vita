# Codex P05/P06 — native Unity 2018.2.19f1 migration (LOCKED UNTIL P04 APPROVED)

**Do not execute this task yet.** The complete P00→P04 approval chain is required, including the owner's explicit P04 acceptance `Unity 5.6.4p3 native baseline accepted`. Read `docs/PHASE_GATES.json` and run `python tools/check_phase_gates.py` first. If P05 is not `ACTIVE`, **STOP after reporting what is missing**. A user saying "prepare the migration" or confirming that save works does not approve P04.

## Instructions to use AFTER unlock

1. Consult [handoff checklist](UNITY56_BASELINE_AND_UNITY2018_HANDOFF.md), [porting gates](PORTING_PLAN.md), [recovery status](RECOVERY_STATUS_2026-10-15.md), [Halo 2D PSP2 reference](HALO2D_PSP2_REFERENCE.md) and [shaders](SHADERS_PSVITA_REFERENCE.md).
2. Verify immutable private snapshot `HSE642_Unity56_FROZEN`, its SHA-256 manifest, original 5.6.4p3 editor recipe, clean matrix and known noncritical issues. Never change this original-engine source of truth.
3. Confirm exact **Unity 2018.2.19f1** (from user's existing Halo 2D workflow); independently verify PSP2 support/library and any restricted platform requirements without copying secrets. **P07 hardware toolchain verification is separate**.
4. Copy frozen source to `HSE642_Unity2018_MIGRATION` in another private folder, preserving all `Assets`, `ProjectSettings`, `.meta`, GUIDs, fileIDs, build-scene order, sprite/mesh/material serialization, C# and the recovered UnityScript type identities. Record pre-upgrade inventory and hashes. No use of raw Android APK `Data` as a replacement for native Unity target serialization.
5. Open **only the copied project** with 2018.2.19f1, record importer/compiler errors as an evidence-led list. Fix minimum required API changes, UnityScript support/translation, plugin import settings, NGUI, shader variants and material references. Preserve Android and Windows behavior. **No speculative gameplay engine rewrite**, no automatic replacement of recovered shaders by Standard/URP.
6. Capture exact shader names, Blend/Depth/Alpha/Stencil/fog/camera/caustics/water in 2018's Built-in pipeline; compare representative screenshots/states with the signed-off Unity 5.6 reference. Inventory all compile/import errors, file diffs and intentional platform changes.
7. In P06 after P05 approval, repeat the *same offline gameplay regression matrix* on PC **in Unity 2018 editor Play Mode** if a standalone EXE is unnecessary. Include save after complete close/reopen, input, camera, boosts, death/retry, UI and audio; compare to P04. Evidence must be sufficient; editor-only is an accepted **format alternative**, not an automatic PASS.
8. Do not build a VPK, alter Vita controls or run game on Vita until P07/P08 are ACTIVE and the proper PSP2 module/hardware are verified. Keep proprietary project files outside Git.

Record sanitized receipts in docs/checkpoints, and leave `docs/PHASE_GATES.json` unchanged unless there is real owner approval for the particular active stage.

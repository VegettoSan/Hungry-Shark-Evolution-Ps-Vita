# Experiment log

Use **PASS** only for reproducibly observed success; **FAIL** for an actually attempted failed test; **NOT TESTED** for a plan or untried hypothesis. Date refers to when the evidence was recorded. Static APK inspection is not Vita runtime testing.

| Date (UTC) | ID | Input / configuration | Result | Evidence / next action |
| --- | --- | --- | --- | --- |
| 2026-10-14 | AUD-001 | HSE 6.4.2 / v228 APK, SHA-256 `52b7d875...`, ZIP/manifest/Unity headers | **PASS (static)** | 5.6.4p3, Mono, 18 `levelN` serialized files, assembly metadata; see [audit](TECHNICAL_AUDIT.md) |
| 2026-10-14 | EXT-001 | Python local extractor, original 6.4.2 APK | **PASS (local)** | Reassembled 15 split groups and preserved `Managed`; does **not** prove project recovery |
| 2026-10-14 | CMP-001 | Later HSE 14.5.0 XAPK, Unity 6 / IL2CPP | **PASS (static comparison)** | Poorer first candidate for a Unity 2018.2 Vita workflow; not a runtime failure |
| — | IMP-001 | AssetRipper import/export with Mono DLLs | **NOT TESTED** | Capture AssetRipper version, log, missing scripts, asset references |
| — | ORIG-001 | Unity 5.6.4p3 editor reconstruction, import, PC gameplay | **NOT TESTED** | Exact source-editor build plus original engine PC executable required before migration |\n| — | EDT-001 | Unity 2018.2 editor compilation **after** P04 approval | **NOT TESTED** | Capture editor/build-target version, compile errors and fixes |
| — | VIT-001 | Real PSP2 native build and hardware test | **NOT TESTED** | Capture VPK/build logs, real hardware behavior and FPS |

## Gated stage state\n\nCurrent first incomplete gate: **P00 ACTIVE**, not formally approved. See [the ledger](PHASE_GATES.json) and [the end-to-end plan](PORTING_PLAN.md). Previously recorded static PASS does not authorize moving directly to AssetRipper or Unity 2018.\n\n## Append-only record template

```text
ID:
UTC date:
Goal / hypothesis:
Base commit:
Input SHA-256 (never attach APK):
Tool/editor version and platform:
Exact command/settings:
Observed result: PASS / FAIL / PARTIAL / NOT TESTED
Evidence: local log path or sanitized excerpts; no copyrighted data
Regression check:
Next action:
```

Failed ideas stay recorded and must not be silently retried without a new reason or variable. Do not label an *untried* loader/port strategy as failed or successful.

## Owner-authorized P00 offline staging — 2026-10-15 UTC

All rows below use APK SHA-256 `52b7d8754ab8fec75c3b656a0dc5ec8dc849565adc402487a3ac093f6a481f7a`. This is the scoped [offline preparation exception](CODEX_UNITY56_OFFLINE_EXPORT_PROMPT.md), not a phase transition. Full logs, manifests, code and exports remain private outside Git. See the [sanitized result](OFFLINE_RECOVERY_2026-10-15.md).

| Date (UTC) | ID | Tool / command or settings | Observed result | Private evidence / next action |
| --- | --- | --- | --- | --- |
| 2026-10-15 | OFF-001 | Python 3.14; SHA-256, ZIP `testzip`, binary Android manifest and Unity header | **PASS (VERIFIED static)**: package/version 6.4.2/228, CRC, 5.6.4p3 Mono | `Reports/APK_and_Tool_Identity.txt`, `Checkpoints/01_apk_verified.json` |
| 2026-10-15 | OFF-002 | Existing Python extractor; empty `Original_Data_Extracted` destination | **PASS (EXTRACTED)**: 2295 logical files, 86 fragments / 15 groups, 21 managed DLLs, 18 levelN; all reconstructed hashes match | `Logs/extraction.log`, `Reports/Source_Manifest.json`; extraction not repeated |
| 2026-10-15 | OFF-003 | AssetRipper 2.0.0 (`1ac666f`); documented local API, Data including Managed; target 5.6.4p3, C#4/Hybrid/Level2, shader YAML, unreadable assets retained | **PARTIAL (EXPORTED)**: 18 scenes, 1167 prefabs, 533 materials, 981 clips, 305 audio files; Mono/21 assemblies detected. HeatMap metadata collision and one MonoBehaviour layout mismatch recorded | `Logs/assetripper.log`, settings/OpenAPI and raw manifest; raw export preserved unchanged |
| 2026-10-15 | OFF-004 | dnSpyEx 6.6.0 CLI, four Assembly-* DLLs; ILSpy 11.1.0.9782; dnlib metadata | **PASS (VERIFIED static inventory)**: complete automatic decompilation saved privately; 2692/2692 top-level type identities accounted for, legacy UnityScript visible | `Recovered_Code_dnSpy`, `Reports/Mono_Metadata.json`, `Type_Coverage.json`; method equivalence untested |
| 2026-10-15 | OFF-005 | Separate working copy; original MonoScript/CIL evidence; reversible metadata and decompiler repairs | **PARTIAL (STATIC_PREPARED)**: HeatMap references restored, missing metadata completed, duplicate assembly attributes removed; 49 event field accesses, 5 overload calls and 1 constructor type restored to original IL semantics | `Reports/Static_Repairs.json`, `Static_Code_Repairs.json`, original IL evidence, backups and private diff. Constructor proof was found in its nested coroutine before completing that repair |
| 2026-10-15 | OFF-006 | Generic Python GUID/fileID validator; Roslyn SDK 10.0.201, C#4 syntax and metadata-only semantic analysis, no game assembly emitted | **STATIC_CHECK_ISSUES**: no missing metadata/duplicate GUIDs/broken local or YAML-target fileIDs; 18 scene paths/GUIDs match. 55 uGUI references and 8 null scripts unresolved. Syntax: 0 errors. Semantic probe: 0 firstpass / 2 main diagnostics | `Reports/Working_Static_Validation.json`, `Working_CSharp_Syntax.json`, `Working_CSharp_Semantic.json`; APK references are stripped, so this is not Unity compilation. Validator fixture PASS for missing/resolved scripts and duplicate GUID detection |
| 2026-10-15 | OFF-007 | Final SHA-256 comparison and handoff checkpoint | **PASS (VERIFIED integrity)**: all 2295 extracted files and 18790 raw export files unchanged; working copy and first-open instructions exist | `Checkpoints/08_offline_complete.json`; **EDITOR_NOT_TESTED / COMPILE_NOT_TESTED / GAMEPLAY_NOT_TESTED**. P00 ACTIVE; P01/P02 LOCKED; no physical Vita |

## Focused owner-requested Google Play Games ambiguity repair

| Date (UTC) | ID | Input / tool / exact change | Observed result | Private evidence / next action |
| --- | --- | --- | --- | --- |
| 2026-10-15 | GPG-001 | Existing private working project from APK SHA-256 `52b7d8754ab8fec75c3b656a0dc5ec8dc849565adc402487a3ac093f6a481f7a`; owner-reported Unity 5.6.4p3 CS0104 at ConversionUtils:80 and NativeClient:203/444; Python 3.14 added `using Types = GooglePlayGames.Native.Cwrapper.Types;` once in each file | **VERIFIED (static change only):** original wrapper declares DataSource, MultiplayerEvent and AuthOperation; alias binds their 10 existing references across both files. Backups precede both writes; removing the alias reproduces original bytes; both meta hashes unchanged. **UNITY RECOMPILE RESULT NOT VERIFIED** | `Checkpoints/09_gpg_types_cs0104.json` locates private before-copies, diff and SHA-256 receipt. Files saved for editor recompilation; inspect the new Unity console before claiming success. No DLL, prefab/assertion, SimpleJSON or gameplay changes; phase ledger unchanged |

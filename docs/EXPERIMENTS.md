# Experiment log

Use **PASS** only for reproducibly observed success; **FAIL** for an actually attempted failed test; **NOT TESTED** for a plan or untried hypothesis. Date refers to when the evidence was recorded. Static APK inspection is not Vita runtime testing.

| Date (UTC) | ID | Input / configuration | Result | Evidence / next action |
| --- | --- | --- | --- | --- |
| 2026-10-14 | AUD-001 | HSE 6.4.2 / v228 APK, SHA-256 `52b7d875...`, ZIP/manifest/Unity headers | **PASS (static)** | 5.6.4p3, Mono, 18 `levelN` serialized files, assembly metadata; see [audit](TECHNICAL_AUDIT.md) |
| 2026-10-14 | EXT-001 | Python local extractor, original 6.4.2 APK | **PASS (local)** | Reassembled 15 split groups and preserved `Managed`; does **not** prove project recovery |
| 2026-10-14 | CMP-001 | Later HSE 14.5.0 XAPK, Unity 6 / IL2CPP | **PASS (static comparison)** | Poorer first candidate for a Unity 2018.2 Vita workflow; not a runtime failure |
| — | IMP-001 | AssetRipper import/export with Mono DLLs | **NOT TESTED** | Capture AssetRipper version, log, missing scripts, asset references |
| — | EDT-001 | Unity 2018.2 editor compilation | **NOT TESTED** | Capture editor/build-target version, compile errors and fixes |
| — | VIT-001 | Real PSP2 native build and hardware test | **NOT TESTED** | Capture VPK/build logs, real hardware behavior and FPS |

## Append-only record template

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

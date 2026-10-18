# BetterWinds LiveArea integration — 2026-10-18 UTC

Local date: 2026-10-17. The owner's explicit request and
[bounded task](CODEX_BETTERWINDS_LIVEAREA.md) authorize this LiveArea-only
exception. Work uses existing `main`; no branch or phase transition.
Artwork contribution: **[BetterWinds](https://github.com/BetterWinds)**.
Underlying game artwork/trademarks retain their original owners.

## Inputs and implementation

**VERIFIED:** supplied `BetterWinds_HSE_LiveArea_ReadyForCodex.zip` SHA-256
`ea3425f6738a61e972e36681395b2797de548350c0a1e83aafa7baa9f525a82a`.
Only its three PNG payloads were isolated in a local three-file ZIP and passed
`tools/install_betterwinds_livearea.py`. Embedded documentation and scripts were
not executed or treated as user instructions. Originals match the
[reviewed SHA-256 manifest](../assets/livearea/BetterWinds/README.md): background
840×500 and startup 280×158 are indexed PNGs; icon 128×128 is RGBA.
Source images remain ignored pending public redistribution rights.

**VERIFIED:** the installed Unity 2018.2.19f1 PSP2 extension copies a custom
`liveAreaPath` directory with `*.xml;*.png` and bypasses independent background/gate
fallbacks when that copy succeeds. Existing project settings had all three paths
empty and no PSP2 icon. D10's LiveArea used `bg0.png` and `default_gate.png`.

The private project now uses native saved settings:

| Setting/input | Value |
| --- | --- |
| `PlayerSettings.PSVita.liveAreaPath` | `PSP2System/BetterWinds/contents` |
| `liveAreaBackroundPath`, `liveAreaGatePath` | Empty, avoiding competing inputs |
| PSP2 icon slot | 128×128 texture at `Assets/Editor/BetterWinds/icon0.png` |
| Custom contents | Original `bg0.png`, `startup.png`, current native `template.xml` |
| Template | Existing `a1` / `01.03`; only gate filename changed to `startup.png` |

The checked-in minimal `01.00` XML remains a reference. The actual native `01.03`
template had no additional links/frames to preserve. `startup.png` is the system
launch gate. Existing `pic0.png` is retained. The reusable editor helper
[HSEBetterWindsLiveArea.cs](../tools/unity/HSEBetterWindsLiveArea.cs) checks source
hashes, sets native configuration and saves assets. No LiveArea postprocessor or
runtime/gameplay change is required. Saved paths and icon GUID persist for future
builds; keep the referenced private files and their metadata.

## Validation and rollback

Private evidence is under the existing Unity 2018 project's
`Reports/BetterWinds_20261018/`: before-manifest, settings backup, input hash,
configuration receipt, build log/result, package receipt and integrity report.
The checkpoint inventories 19,639 authored files and nine previous VPKs.

**VERIFIED preparation controls:** old generic D10 VPK is rejected by the new
read-only [VPK verifier](../tools/verify_betterwinds_vpk.py). A competing batch
editor invocation refused the already-open project; the existing editor session
was refreshed instead. An initial checkpoint attempt stopped on an old directory
named `.vpk`; restricting the archive census to files corrected the collector
before installing assets. No previous build was replaced.

**VERIFIED build:** native PSP2/Mono2x, original enabled scenes and build options,
Succeeded, **0 errors / 296 warnings**, **2:12.318**. Build log has no C# compiler
errors or shader errors. This is not a gameplay/performance regression test.

**VERIFIED final VPK:** `Build/HSE_BetterWinds_HGSKE1178.vpk` within that report
directory, **135,085,568 bytes**, **213 entries**, SHA-256
`a6e40f075f1c08a72942382708e6df3b9bab9d3481fc618dd4e0e8d093ac78fb`.
CRC, unique/case-safe paths, template references and required dimensions PASS.
Exactly `bg0.png`, `startup.png`, `template.xml` occupy LiveArea contents; no stale
`default_gate.png`. Background/gate hashes equal originals. Unity's native icon
export quantizes RGBA to indexed PNG: **10,916 bytes**, SHA-256
`28bafa4298a4aa7e00ce30df69d082710e1553d201bcdeacb81fb5c5cf9fb775`.
The initial strict source-hash assertion caught this expected engine conversion;
it was not silently accepted. Source and generated icon were visually inspected:
same composition/dimensions, palette quantization visible at pixel level;
16,266 pixels differ, maximum RGB deltas 58/33/47, alpha unchanged. The verifier
now accepts only this reviewed native derivative or the byte-exact original.
No manual conversion was necessary; original source/staging images are intact.

The existing private player-directory-to-VPK convention is retained. Native SELF
and all runtime entries equal the native output byte-for-byte; only the standard
SFO identity metadata is normalized to the **exact existing D10** TITLE_ID,
TITLE, STITLE and CONTENT_ID values. No new identity/content ID was invented.
The final title is `HGSKE1178` / Hungry Shark Evolution Vita; prior full content
ID, ATTRIBUTE2 and `pic0.png` are unchanged relative to D10. No savedata directory
is included in the VPK.

**VERIFIED integrity:** all 19,639 preexisting authored files checked; the only
changed original file is `ProjectSettings/ProjectSettings.asset`, whose diff
contains only the PSP2 icon slot and custom LiveArea path. Existing game code,
input, save code, scenes, shaders, other settings and all nine earlier VPK hashes
are unchanged. Unity 5.6 was not opened or modified. The temporary build driver
was archived outside Assets; the reusable helper and saved native settings remain.

**VERIFIED additional checks:** the final player's native D10 save audit passes
original header/MD5/AES/CLZF2, scoped invariant culture/finally, exact reads,
four temporal/commit/reopen checks and D09 copy retention. An initial attempt
under Windows PowerShell could not load the local dnlib type; running the same
audit in the current PowerShell runtime passed. No save source was changed.
Verifier negative fixtures reject stale gate artwork, damaged PNG bytes, wrong
template references, case-colliding entries and a wrong title. The phase validator
remains P03 / 3 approvals; the ledger is unchanged.

Public changes are the PNG ignore rule, reproducible editor configuration helper,
read-only VPK verifier, manifest instructions, this report and experiment record.
Original game art, Unity project, VPKs, SDK files, local backups and detailed logs
remain private. README's BetterWinds acknowledgement is preserved.

**UNTESTED physical Vita:** no installation, bubble/LiveArea visual inspection,
launch/resume or hardware gameplay test was performed in this task. Package
verification does not establish physical behavior or approve any locked phase.

Rollback: preserve any subsequent edits, restore only the old PSP2 icon and
LiveArea fields from `ProjectSettings.before.asset`, then archive the added
`PSP2System/BetterWinds`, `Assets/Editor/BetterWinds` and
`Assets/Editor/HSEBetterWindsLiveArea.cs` with their metadata. If other settings
have since changed, do not replace `ProjectSettings.asset` wholesale. D10 and the
other prior VPKs remain the comparison artifacts. Public rollback can revert the
topical LiveArea commit on existing `main`; no reset/force-push is needed.

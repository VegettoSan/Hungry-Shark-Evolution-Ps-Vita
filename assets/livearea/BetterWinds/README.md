# Hungry Shark Evolution Vita — LiveArea artwork

**LiveArea artwork contributed by [BetterWinds](https://github.com/BetterWinds).** Thanks for the PS Vita artwork and icon design. This credit refers to the LiveArea asset contribution; *Hungry Shark Evolution*, related characters, logos and original game art remain property of their respective rights holders.

**Source archive:** `SharkEVOassets.zip` provided by the project's owner (not committed here). The three original assets were inspected unchanged:

| Source file | PNG dimensions | SHA-256 | VPK destination |
| --- | --- | --- | --- |
| `bg0.png` | 840 × 500 | `452a71826ec9613c29773a4f29bebdc88c3dfe9299313048939d6a2b1152f77c` | `sce_sys/livearea/contents/bg0.png` |
| `startup.png` | 280 × 158 | `293fff5b6e8abc7923ed2d9f3b1bb92b233843c7d6416fa5b657b8b2730a4f4d` | `sce_sys/livearea/contents/startup.png` |
| `icon0.png` | 128 × 128 | `81d4755193278c3cde2d821398d300d01c3a6b9d5e3df868db9f9fb70c83c1c5` | `sce_sys/icon0.png` |

The repository includes a minimal `template.xml` reference for the two LiveArea images; **this template was prepared for integration and is not part of BetterWinds' original ZIP**. The ZIP does not include `pic0.png` (the full-screen background), so keep any existing packaging input rather than inventing a fourth artist asset.

## Local source installation

Place the owner-supplied ZIP anywhere accessible on the development machine and run from repository root:

```sh
python tools/install_betterwinds_livearea.py path/to/SharkEVOassets.zip
```

The installer checks exact file identities, dimensions and safe names, then writes the three PNGs next to this README. It does **not** update Unity settings or build any VPK. Continue with [Codex's integration task](../../../docs/CODEX_BETTERWINDS_LIVEAREA.md).

**Public repository caveat:** This repository currently excludes unauthorized extracted game art. Do not publish any image if the required permission/rights for public redistribution are not established; a local developer copy is not, by itself, permission to redistribute.

## Native Unity 2018.2 PSP2 configuration

The owner-requested integration uses a private project folder
`PSP2System/BetterWinds/contents/` with the original `bg0.png`, `startup.png`
and the current native template (preserve its format/functionality; update the gate
reference to `startup.png`). Keep `icon0.png` unchanged at
`Assets/Editor/BetterWinds/icon0.png`. System artwork stays outside runtime scenes.

Copy [HSEBetterWindsLiveArea.cs](../../../tools/unity/HSEBetterWindsLiveArea.cs)
into the private project's `Assets/Editor/`, then run
**HSE > PSP2 > Configure BetterWinds LiveArea** in Unity **2018.2.19f1**.
Back up `ProjectSettings.asset` first. The helper validates source hashes and sets
the native PSP2 icon and `PlayerSettings.PSVita.liveAreaPath`; it saves the
settings and clears competing independent background/gate paths. No build
postprocessor is installed. Existing `pic0.png`, identity and game settings are
preserved. Future builds consume the saved settings and referenced files; keep
their `.meta` files and GUIDs with the private project.

After the existing private VPK packaging step, run:

```sh
python tools/verify_betterwinds_vpk.py path/to/final.vpk --previous-vpk path/to/previous.vpk
```

This checks CRC, duplicate paths, reviewed PNG bytes/dimensions, native template
references, canonical title, unchanged previous package identity/memory flag and
`pic0.png`. It does not install or certify physical Vita behavior. See the
[integration report](../../../docs/BETTERWINDS_LIVEAREA_2026-10-18.md) for observed results.

**Native icon conversion:** Unity exports the original RGBA icon as a quantized
indexed PNG (128×128), SHA-256
`28bafa4298a4aa7e00ce30df69d082710e1553d201bcdeacb81fb5c5cf9fb775`.
The verifier accepts this explicitly reviewed native result as well as the exact
original. Background/gate retain their source hashes. No manual PNG conversion,
VPK artwork replacement or modification of the source originals was performed.

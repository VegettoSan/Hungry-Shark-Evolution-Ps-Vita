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
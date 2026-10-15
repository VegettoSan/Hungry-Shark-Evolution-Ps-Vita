# Porting plan — stages and acceptance gates

**Principle:** preserve the original gameplay and use the **native Unity PSP2 runtime**, rather than recreate the game or assume Android Unity libraries are Vita compatible. **Every stage has a measurable gate.**

## Stage 0 — freeze the source (DONE: static audit only)

- Verify SHA-256 against [technical audit](TECHNICAL_AUDIT.md).
- Keep original APK read-only, outside the public repository.
- Preserve 5.6.4p3 and 6.4.2/versionCode 228 in all audit reports.
- Local extraction tool can reconstruct `.splitN` Unity files without downloading any other game content.

**Gate:** repeatable local extraction and consistent metadata.

## Stage 1 — asset and scene recovery (NOT TESTED)

- Use the provided Python/BAT tool to extract `Data/`.
- Import serialized data and `Data/Managed` assemblies **together** into AssetRipper.
- Export to a disposable local Unity project. Record tool version, errors, missing MonoScript mappings, scene and prefab links, shader replacements and success/failure counts.
- Inspect two or three small representative scenes; preserve original identifiers/serialized fields.
- Use ILSpy to inspect game assemblies. Document external service entry points and runtime-critical systems.

**Gate:** at least one exported scene with inspected, working script references; list all unresolved references. Merely exporting files is not sufficient.

## Stage 2 — Unity editor prototype (NOT TESTED)

- Work on a **copy** using Unity 2018.2 and an actually working PSP2 module; preserve the recovery baseline.
- Try PC/editor compilation first; identify Unity 5.6 to 2018.2 API differences.
- Audit `Assembly-UnityScript*` carefully; do not assume it compiles unchanged.
- Keep full names, serialized types/fields, object assignments and execution order; perform deliberate mappings only with evidence.
- Replace Android-only plugins with platform-isolated offline stubs/adapters **without breaking callers**. Do not fake network-dependent progression.
- Fix incompatible shaders using built-in/legacy pipeline and PSP2-supported effects, beginning with a simple test scene.

**Gate:** deterministic scene load and gameplay-system initialization in the target editor, with a tracked list of remaining failures.

## Stage 3 — first physical PS Vita build (NOT TESTED)

- Build a minimal genuine PSP2 player. Confirm deployment, logs, Unity startup and first frame.
- Then progressively enable scene assets, touch/controller input, audio, camera, spawn and shark navigation.
- Measure boot/menu/play time, FPS, heap and graphics usage and crashes on real hardware.
- Avoid bulk texture compression or shader removal until a measured bottleneck demands it.

**Gate:** real-console screenshot/log and reproducible shark movement in a recovered scene. An empty Unity Vita test does **not** count as port gameplay.

## Stage 4 — playable offline milestone (NOT TESTED)

- Verify sharks, prey, feeding/health, UI, progression, save/load, sound, scene transitions and exit/resume.
- Document unsupported online store, purchases, ads and external services.
- Track regressions between changes; release only with tested limitations and appropriate distribution rights.

## Biggest risks

1. Loss of MonoBehaviour GUID/fileID/type associations during extraction/export.
2. Unity 5.6 to 2018.2 API, serialization and UnityScript differences.
3. Android plugins and online services hidden in startup/gameplay.
4. PSP2 rendering and memory limits with original textures/shaders.
5. Editor installation/PSP2 SDK access, which the repository does **not** supply.

If a gate fails, log evidence in [EXPERIMENTS.md](EXPERIMENTS.md) before trying another strategy.

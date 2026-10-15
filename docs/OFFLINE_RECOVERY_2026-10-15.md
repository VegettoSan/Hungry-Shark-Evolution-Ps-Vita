# Offline recovery preparation — 2026-10-15 UTC

> **Historical checkpoint, before editor testing.** Subsequent owner-requested repairs and actual Unity 5.6.4p3 checks are summarized in [the current recovery status](RECOVERY_STATUS_2026-10-15.md). Statements below such as EDITOR_NOT_TESTED describe this earlier operation, not the current project. Its inventories and outstanding findings remain evidence unless a later experiment explicitly closes them.

This work used the owner's [P00 offline staging authorization](CODEX_UNITY56_OFFLINE_EXPORT_PROMPT.md). **P00 remains ACTIVE; all later phases remain LOCKED.** No Unity editor, Unity 2018 migration, PSP2 implementation or device test was performed. No proprietary output was added to Git.

**VERIFIED:** the exact audited APK passed SHA-256, CRC, package/version and Unity 5.6.4p3 Mono checks. The existing extractor produced 2295 logical Data files, including 15 numerically reconstructed groups from 86 fragments, 18 levelN files and 21 assemblies. Each joined output was checked against the original fragments.

**EXPORTED:** AssetRipper 2.0.0 loaded Data and Managed together, detected Mono and 21 assemblies, and exported a project with its own generated `ProjectVersion.txt` set to 5.6.4p3. The raw directory contains `ExportedProject` and `AuxiliaryFiles`. A separate `HSE642_Unity56_ReadyToOpen` copy contains Assets and ProjectSettings. Outputs reside in the private sibling workspace `HSEVita-Local`, outside the clone.

The working copy contains 18 scenes, 1167 prefabs, 533 materials, 2957 mesh YAML assets, 981 animation clips, 305 audio files, 678 PNG images, 4 Texture2D YAML assets and 47 shader YAML assets. These are exported-file counts, not fidelity or gameplay results. Shader source was not recovered; Android GPU data and original Data remain preserved. No shader replacement was applied.

**VERIFIED static recovery:** ILSpy 11.1 and dnSpyEx 6.6 produced/inspected real managed source. All 2692 top-level type identities from the four original C#/UnityScript assemblies are present. Full code, fields, dependency inventories and logs are private. UnityScript was decompiled to C#; its original assemblies remain preserved and its runtime behavior is untested.

Reversible repairs were limited to demonstrated export problems: separate case-colliding HeatMap/Heatmap types with both GUIDs preserved; complete absent script/folder metadata with explicitly new deterministic recovery GUIDs; remove duplicate AssemblyVersion attributes caused by C# compilation grouping; restore 49 erroneous event reads/ref accesses to their declared backing fields; restore five optional-argument calls to the exact four arguments in original IL; qualify one constructor with its original namespace. Original method arguments, event synchronization and public contracts remain represented. Runtime equivalence still requires testing.

The [generic static validator](../tools/validate_recovered_unity_project.py) checks metadata, GUIDs, exported YAML fileIDs, scripts and Build Settings. **VERIFIED:** no missing metadata, duplicate GUIDs, broken local/YAML-target fileIDs or scene path/GUID mismatches remain. All 18 Build Settings entries retain their recovered order. A fixture confirmed missing/resolved script and duplicate-GUID detection.

**STATIC_CHECK_ISSUES:** 55 references to the uGUI GUID need editor resolution; eight null script references remain in six prefabs. AssetRipper also reported a `MissionAchievementsManager` layout mismatch; that exported component lacks its instance fields, so defaults must not be accepted as faithful recovery. No components were deleted or assigned guessed scripts.

C#4 syntax analysis found zero errors. A separate Roslyn semantic probe emitted no game assemblies and found zero firstpass and two main diagnostics. The latter concern incomplete CreateAssetMenu attribute accessors in the stripped Android UnityEngine metadata; **INFERRED:** editor references may resolve them. This probe used installed full .NET 2.0/3.5 references after confirming APK BCL limitations. It is not the Unity compiler or a PC build.

**UNTESTED:** actual Unity 5.6 import/compilation, uGUI and Boo/UnityScript references, shader rendering, Android-native service isolation, serialized values, save behavior, gameplay and hardware. The private `Reports/Unity56_First_Open_Instructions.md` directs the owner to open `Assets/WIP/SplashLoaderScene.unity` first after accounting for compiler errors.

Final hash validation confirmed all 2295 extracted files and 18790 raw export files unchanged. Private checkpoints, repair backups and diffs support resuming without another extraction/export. See [experiment records OFF-001–007](EXPERIMENTS.md).

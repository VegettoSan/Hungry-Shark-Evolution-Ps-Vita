# Tools and external references

Only direct project links and specific reasons to use them are maintained here. **No installer binaries or vendor SDKs are mirrored.** Note version and hashes when conducting reproducible experiments.

| Tool / reference | Use |
| --- | --- |
| [Unity 5.6.4p3 official editor](https://unity.com/releases/editor/whats-new/5.6.4p3) | **Original-engine reconstruction and desktop gameplay validation before any migration** |\n| [Unity 2018.2.21f1 official editor](https://unity.com/releases/editor/whats-new/2018.2.21f1) | Candidate later editor **after** Unity 5.6 PC baseline approval; ensure matching PSP2 module is actually available |\n| [Unity download archive](https://unity.com/releases/editor/archive) | Official historical editor installers via Unity Hub / manual archive |\n| [AssetRipper](https://github.com/AssetRipper/AssetRipper) | Import Unity serialized assets/scenes; attempt project recovery and mapping of MonoScript dependencies |
| [AssetRipper common issues](https://github.com/AssetRipper/AssetRipper/blob/master/docs/articles/CommonIssues.md) | Import **assemblies together with the serialized files** so Mono scripts can be identified |
| [ILSpy](https://github.com/icsharpcode/ILSpy) | Inspect original managed C# assemblies; identify call sites, types, fields and service dependencies |
| [Unity PSP2 support deprecation reference](https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/BaseClass.cs) | PSP2 is marked no longer supported starting Unity 2018.3; investigate Unity 2018.2 with available PSP2 build tooling |
| [Unity 2018.2 release notes](https://unity.com/blog/engine-platform/2018-2-now-available) | Platform/version context; UnityScript deprecation is a migration risk |
| [Unity 2018 LTS upgrade manual](https://docs.unity.cn/ru/Manual/UpgradeGuide2018LTS.html) | Changes in later versions; do not infer that every change applies identically to 2018.2 |
| [UnityScript conversion reference](https://github.com/Unity-Technologies/unityscript2csharp) | Research a possible C# migration if legacy scripts block compilation; test semantics carefully |

## Local reproducible preparation

```bash
python tools/Preparar_Hungry_Shark_Evolution_6.4.2.py /path/to/HungryShark.apk local-work/HSE642
```

Windows: drag your APK onto `tools/ARRASTRAR_HUNGRY_SHARK_6.4.2.bat`. On success, import the resulting `Data/` folder **with** its `Managed/` subfolder into AssetRipper.

The extractor uses Python 3 standard library only. It does not require Codex or a network connection, does not decompile Mono, and does not reconstruct a ready-to-build Unity project.

### Mandatory phase order\n\nDo **not** open the recovered project directly in Unity 2018.2 as the first validation. See [gated plan](PORTING_PLAN.md). Recover, fix and run in **Unity 5.6.4p3 PC first**, then migrate a **clone** to Unity 2018.2 after explicit approval. Legacy UnityScript interoperability must be audited and may require conversion.\n\n### Install/compatibility caution

The publicly available Unity editor alone does not guarantee that a **PSP2 build target / SDK** is installed or usable. Establish the actual local build capability before spending time on mass asset migrations. Do not commit private or vendor-restricted components.

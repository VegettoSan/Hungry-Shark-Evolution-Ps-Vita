# PS Vita shaders and rendering — research and experiment reference

**Research date:** 2026-10-14 · **Target:** Hungry Shark Evolution 6.4.2, original Unity 5.6.4p3 → Unity **2018.2.19f1** (Halo 2D reference) → native PSP2 runtime.

> **REFERENCE / NOT A RULE:** Techniques below are candidate experiments, not mandatory material replacements. Do not change a shader purely because it is called 'mobile' or because an optimization sounds plausible. Preserve the original appearance as far as practical, establish a baseline and measure each modification on real Vita hardware. Phase gates remain binding; this document does not unlock later work.

## 1. Evidence from Unity 2018.2

- Unity 2018.2 explicitly offers **`UnityEngine.Rendering.ShaderCompilerPlatform.Vita`** for PlayStation Vita. The platform is **not** equivalent to GLES2 even though the original Android APK declares OpenGL ES 2.0. Shader compilation must be validated using the **PSP2 target compiler**. [Unity 2018.2 scripting API](https://docs.unity.cn/2018.2/Documentation/ScriptReference/Rendering.ShaderCompilerPlatform.html).
- Built-in shader macro **`SHADER_API_PSP2`** identifies PlayStation Vita. Unity also defines **`SHADER_API_MOBILE`** on PSP2. They can be used to author conservative, explicit platform paths. `SHADER_API_GLES` should **not** be assumed on Vita. [Unity 2018.2 platform macros](https://docs.unity.cn/ja/2018.2/Manual/SL-BuiltinMacros.html).
- `#pragma only_renderers psp2` and `#pragma exclude_renderers psp2` exist in historical Unity shader-compiler directives. Avoid applying either indiscriminately: they change where shaders compile and can remove needed desktop/test variants. [Unity 2018.2 shader programs](https://docs.unity.cn/cn/2018.2/Manual/SL-ShaderPrograms.html).
- `#pragma target 2.0` can be a **conservative experiment** for very simple vertex/fragment shaders; it restricts available features (e.g. interpolators, derivatives, texture LOD). **Do not assert that all Vita shaders must use shader model 2.0** or that upgrading target ensures compatibility. Check actual compiler errors and hardware rendering. [Unity 5.6 shader compile targets](https://docs.unity.cn/560/Documentation/Manual/SL-ShaderCompileTargets.html).
- This workflow uses **Built-in Render Pipeline / ShaderLab / Cg-HLSL**, not Unity URP, Shader Graph, or a VitaGL/GL4ES Android loader. The historical Unity 2018.2 LWRP was preview technology; no official evidence was found that switching to LWRP is supported or beneficial in this specific PSP2 Unity module. [Unity 2018.2 release](https://unity.com/blog/engine-platform/2018-2-now-available).
- Underwater alpha blending, overdraw, multi-pass lighting, shadow passes, texture reads, render-to-texture effects, shader math and frequent material/state changes are **possible bottlenecks**, not audited findings for HSE. See [Unity mobile optimization](https://docs.unity3d.com/es/2018.3/Manual/MobileOptimisation.html), [Unity 2018.2 rendering profiler](https://docs.unity.cn/2018.2/Documentation/Manual/ProfilerRendering.html).
- Unity 2018.2 introduced **scriptable shader variant stripping**; this can reduce build time and size, not necessarily GPU frame time. Be careful about variants activated by runtime C# code (fog, lights, quality settings). Stripping an actually-used variant can cause incorrect rendering. [Unity developer explanation, 2018](https://unity.com/blog/engine-platform/stripping-scriptable-shader-variants).

## 2. Official shader reference downloads

| Download | Verified use | Caution |
| --- | --- | --- |
| **[Unity 2018.2.19f1 built-in shaders ZIP — official Unity CDN](https://download.unity3d.com/download_unity/06990f28ba00/builtin_shaders-2018.2.19f1.zip)** | Exact target editor version (Halo 2D), reference built-in shader source/CG includes, compare Mobile/Unlit/Legacy/Standard implementation and fallback behavior | The Unity release page lists a **Shaders** archive with changeset `06990f28ba00`. The CDN binary was not downloaded/validated during this research; confirm archive integrity locally before using. Do not import every built-in shader into the game. |
| **[Unity 2018.2.19f1 official editor page](https://unity.com/releases/editor/whats-new/2018.2.19f1)** | Confirm exact editor, release changeset, companion components | Preferred to another Unity 2018.2 patch until local PSP2 module is checked |
| **[Unity 5.6.4p3 original editor](https://unity.com/releases/editor/whats-new/5.6.4p3)** | Reconstruct/record original rendering first | The community built-in shader index lists 5.6.4f1 rather than 5.6.4p3; do not silently assume those are byte-identical. Inspect the *installed exact 5.6.4p3* editor's built-ins as primary reference. |

## 3. When shader work is allowed (phase discipline)

| Phase | Action | Gate/evidence |
| --- | --- | --- |
| **P01–P02** | **Inventory only:** extract/export material/shader identifiers, original names, serialized properties, textures, keywords, render queue, blend/depth/culling/pass setup; restore correct Unity 5.6 references. | Record missing shaders and mappings. Avoid graphical redesign/optimization. |
| **P03–P04** | Reproduce **original Unity 5.6.4p3 desktop** rendering and game behavior. Freeze visual screenshots plus performance stats as reference. | Owner approves faithful original-engine PC baseline before migration. |
| **P05** | After P04 owner approval, **clone** for Unity 2018.2.19f1, compile shaders, solve migration/serialization errors and broken references as narrowly as possible. | Every replacement mapped to original shader/material and visually inspected. |
| **P06** | Compare Unity 2018.2 PC screenshots and critical effects against frozen Unity 5.6 PC baseline; validate real gameplay before PSP2. | Owner approves visual/gameplay parity. Desktop FPS does not predict Vita FPS. |
| **P07–P08** | Verify PSP2 compiler on a tiny independent test, then compatibility of actual HSE game shaders/rendering on device: magenta/invisible assets, incorrect alpha, shadows, lighting, texture orientation. **Correctness first.** | Real hardware logs, screenshots and regression tests. |
| **P09–P10** | Preserve successful Vita gameplay; fix shader bugs if they prevent a previously approved loop. Collect data, no broad unmeasured material rewrite. | Existing gameplay and controls still pass. |
| **P11** | Main A/B optimization phase **after real-Vita baseline**. Benchmark and replace only proven bottlenecks, testing visuals and gameplay after each change. | Owner-approved measurements and performance criteria; **30 FPS primary goal**, **60 FPS optional stretch goal**. |
| **P12** | Verify visual regression across menus and representative scenes on physical Vita; document intentional quality differences and legal distribution rules. | Release readiness requires previous approved gates. |

**Important:** Reference research in P00 is allowed; writing Vita-specific game shaders or applying them while P05/P08/P11 remain LOCKED is **not** authorized by this document.

## 4. Candidate shader strategies (hypotheses only)

| Material/effect class to identify in recovered game | Candidate A/B trial | Fidelity/technical pitfalls |
| --- | --- | --- |
| Background/flat surfaces, distant decorative images | Original vs **Unlit/Texture** | Loss of global tint, depth/fog and effects; transparent materials may need a separate alpha blend implementation |
| Shark, fish, edible enemies, interactive objects | Original (often complex lit) vs **Mobile/Diffuse** or **Mobile/VertexLit** only when visually acceptable | Normal maps, directional specular, animated texture properties, colors, skinning and wet/shiny surface style may change |
| Water overlay, underwater color/fog, light shafts | Original vs custom **single-pass** variant, simplify pixel math and texture samples | Water motion, distortion, depth sorting, alpha composition and scene atmosphere must survive |
| Bubbles, splashes, plankton, particle systems | Original vs simpler **Particles/Alpha Blended** or additive pass; fewer transparent screen pixels | Changing alpha clip to alpha blend may **increase overdraw**; particle geometry and blend order matter |
| Dynamic lighting/specular/reflection | Original vs selective vertex lighting/one pixel light/precomputed highlight | Critical shark/readability and water highlights must remain identifiable |
| UI/text/hit overlays | Preserve original UI shader; test simpler unlit/transparent only with evidence | Font softness, sprites, hit flashes, sorting, masks and menu readability |
| Full-screen distortions, post FX, render textures | Keep initially; test disabling **one effect at a time** or lowering intermediate render target resolution | Can break water appearance, transitions, UI or device render passes; do not automatically disable all post FX |

### Practical shader-level ideas to investigate

- **Simplify fragment-stage work:** fewer dependent texture reads, repeated sampling, branches and expensive functions; where justified move calculations into a vertex shader or offline data. Check approximation artifacts in motion.
- **Preserve material properties:** if replacing a shader, map `_MainTex`, tints, normal maps, UV tiling/offset, emission, alpha/blending, render queues and shader keywords. A missing or silently renamed property can make gameplay graphics wrong despite successful compilation.
- **Be cautious with alpha:** `Blend SrcAlpha OneMinusSrcAlpha`, additive and `clip()` have different fill-rate, depth and sorting implications. Transparency in an underwater game is important; benchmark both paths, **never assume alpha test or alpha blend is categorically faster**.
- **Reduce passes and overdraw** where an actual frame capture proves they dominate; share stable materials where possible, without destroying color/animation distinctions between sharks.
- **Test precision** (`float` to `half`/`fixed`) on Vita, not PC alone; older hardware precision rules and rounding can differ. Avoid affecting positions/UVs unless measured error is acceptable.
- **Remove unneeded variants only after tracing actual runtime keywords**, fog/light conditions, dynamically chosen sharks and menus. The engine may require a variant not present in the initial scene.
- **Avoid unconditional switches to LWRP/URP** or adding a new renderer just for perceived performance; the original is Unity 5.6 legacy, and Halo 2D uses the built-in pipeline.

### Platform-conditional shader sketch (documentation only; not HSE code)

```c
// Inside a ShaderLab CGPROGRAM/HLSLPROGRAM block in a future test shader:
#if defined(SHADER_API_PSP2)
    // Candidate reduced fragment path, if independently benchmarked on Vita.
#else
    // Preserve baseline desktop shader path.
#endif
```

This merely illustrates macro detection; it does **not** prove the underlying shader code compiles on PSP2. Use separate shader names/copy-on-write for A/B testing if serialized material identity must be preserved.

## 5. Performance goals and rigorous benchmark protocol

**Primary design goal:** **30 FPS stable in real gameplay** (frame budget **33.33 ms**). **Optional stretch:** **60 FPS** (frame budget **16.67 ms**). Both refer to the **entire rendered frame** (including scripts, physics, GPU, streaming and present), not a standalone GPU shader benchmark. Neither performance level has yet been demonstrated for HSE on Vita.

These goals are **targets, not fabricated guarantees nor blanket shader rules**. P11's acceptance thresholds are confirmed with the owner only after real hardware measurements. Do not lower scene quality or original gameplay logic in advance just to report a nominal FPS.

### Exact A/B steps for each shader experiment

1. **Baseline A:** same real Vita unit and clock profile (record stock/overclock explicitly), same fixed scene/camera/shark/save, resolution and VSync/target frame-rate settings. Record built game commit, editor PSP2 runtime, shader/material name and variant.
2. **Test B:** change only **one shader or one isolated material group**, keeping all other graphics/quality settings and gameplay identical. Preserve a ready rollback.
3. **Warm up and repeat:** for a comparable scene, e.g. 10 s warm-up + three 120 s measurements at representative menu, sparse gameplay and visually dense gameplay points **when runtime instrumentation supports it**. Use consistent controlled inputs where possible. These are suggested reproducibility settings, not a substitute for practical field testing.
4. **Record CPU and GPU evidence separately where available:** average, median, 95th/99th-percentile frame time, longest stalls, FPS distribution, Batches, SetPass, draw calls, triangles, texture memory, total memory and crashes. Keep a raw data log; not every GPU counter is supported on Vita.
5. **Visual comparison:** capture equivalent frames and short movement clips (shark, UI, layered water, bubbles, effects), confirm no pink shaders, blending/z-fighting/flicker, missing normal/specular, missing textures or broken animations.
6. **Go/no-go:** keep candidate **only if repeatably faster or more compatible without unacceptable visual/gameplay regression**. Otherwise **revert B**, log the failed result, move to another hypothesis. A change with ~0 improvement should not be retained as an unproven optimization.

### Diagnose the bottleneck before altering shaders

- **CPU-bound indicator:** scripts/physics/spawning/sorting/set-pass dominates, GPU headroom remains. Shader arithmetic reduction may do little; investigate safe CPU/material batching work instead.
- **GPU/fill-rate-bound indicator:** frame time improves substantially when **temporarily lowering internal render resolution** or removing a large transparent layer; examine fragment/pixel work and overdraw. Treat this as a diagnostic A/B, not a permanent downgrade.
- **Memory/bandwidth-bound indicator:** large gains from reducing texture quality or stalled streaming, with texture memory pressure; inspect compression/mipmaps/loads, not just shader instruction count.
- **Variant/shader-loading-bound indicator:** spikes coincide with first-time material/variant appearance or scene transitions; inspect shader loading/variant lists before adding global preloads.

**Instrumentation caveat:** [Unity 2018.2 GPU Profiler documentation](https://docs.unity.cn/2018.2/Documentation/Manual/ProfilerGPU.html) does **not list PlayStation Vita as supported for remote GPU timings**. Do not invent Vita GPU-millisecond measurements. Use available on-device frame timing plus CPU and rendering counters, PSP2 diagnostics legitimately accessible to the developer, and controlled A/B changes. Unity's [2018.2 Rendering Profiler](https://docs.unity.cn/2018.2/Documentation/Manual/ProfilerRendering.html) documents **Batches / SetPass / Triangles / Vertices**. Logs or tools may have overhead; do a final non-profiled session.

### A/B experiment record template (append to EXPERIMENTS.md)

```text
TEST ID / date UTC / phase gate:
Game source SHA256 / baseline commit / editor & PSP2 module version:
Hardware, clock profile, output resolution, vsync & Application.targetFrameRate:
Scene, shark, game state, duration, number of runs:
Original shader + material name/serialized properties:
Candidate shader + exactly one change:
Compilation on PC / PSP2 shader compiler (PASS/FAIL):
Visual validation: opaque, alpha, UV, lighting, FX, UI:
CPU/GPU diagnostics available? (state unsupported metrics explicitly):
Frame ms average / median / P95 / P99, 30 FPS or 60 FPS target attained?:
RAM/texture memory, batches, SetPass, any crashes:
Decision KEEP/REVERT/RETEST and link to sanitized evidence:
Owner approval only if closing an actual phase gate:
```

## 6. What we do NOT know yet

- **No verified per-material HSE 6.4.2 shader inventory yet.** Static APK Unity version/Mono audit does not establish exact imported shader names or whether particular HSE effects compile for PSP2.
- **No Unity 5.6 native PC rendering test, no Unity 2018.2 migration, no PSP2 shader compiler log and no real-Vita frame measurement exist yet.** Do not describe shader suggestions as confirmed improvements.
- **Halo 2D's Unity 2018.2.19f1 PSP2 artifacts prove a historical project**, not HSE material compatibility or current toolkit viability. See [Halo reference](HALO2D_PSP2_REFERENCE.md).

## 7. Source references (historical where possible)

- [Unity 2018.2 Vita shader compiler enum](https://docs.unity.cn/2018.2/Documentation/ScriptReference/Rendering.ShaderCompilerPlatform.html)
- [Unity 2018.2 macros: SHADER_API_PSP2, SHADER_API_MOBILE](https://docs.unity.cn/ja/2018.2/Manual/SL-BuiltinMacros.html)
- [Unity 2018.2 #pragma renderers / psp2](https://docs.unity.cn/cn/2018.2/Manual/SL-ShaderPrograms.html)
- [Unity 5.6 supported shader compilation targets](https://docs.unity.cn/560/Documentation/Manual/SL-ShaderCompileTargets.html)
- [Unity 2018.2 stripping variants](https://unity.com/blog/engine-platform/stripping-scriptable-shader-variants)
- [Unity 2018.2 Rendering Profiler](https://docs.unity.cn/2018.2/Documentation/Manual/ProfilerRendering.html)
- [Unity 2018.2 GPU Profiler support matrix](https://docs.unity.cn/2018.2/Documentation/Manual/ProfilerGPU.html)
- [Unity built-in shader performance and mobile categories](https://docs.unity3d.com/es/2018.3/Manual/MobileOptimisation.html)
- [Unity original shader package / version page](https://unity.com/releases/editor/whats-new/2018.2.19f1)
- [Halo 2D PSP2 repository](https://github.com/VegettoSan/Halo2D_PsVita)

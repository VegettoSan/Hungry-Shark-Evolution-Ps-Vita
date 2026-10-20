// Editor-only comparison of the owner's original and optimized private UpdateTint.
#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class HSEPlayerTintContract
{
    const BindingFlags Fields = BindingFlags.Instance | BindingFlags.NonPublic;
    static FieldInfo Field(Type t, string name) { return t.GetField(name, Fields); }
    static void Set(object o, string name, object value) { Field(o.GetType(), name).SetValue(o, value); }
    static string Float(float f) { return f.ToString("R", System.Globalization.CultureInfo.InvariantCulture); }
    sealed class Fixture : IDisposable
    {
        public object player;
        public Action tick;
        public List<Renderer> renderers = new List<Renderer>();
        List<GameObject> objects = new List<GameObject>();
        List<Material> materials = new List<Material>();
        public Fixture(int count, bool withAccessories)
        {
            Shader shader = Shader.Find("HSE/RimLit");
            if (shader == null) throw new Exception("Fixture shader absent");
            player = FormatterServices.GetUninitializedObject(typeof(FGOL.Player));
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("Temporary tint fixture"); objects.Add(go);
                var renderer = go.AddComponent<MeshRenderer>(); var mat = new Material(shader); materials.Add(mat);
                renderer.sharedMaterial = mat; renderers.Add(renderer);
                var instance = renderer.material; if (instance != mat) materials.Add(instance);
                if (!instance.HasProperty("_Tint") || !instance.HasProperty("_TintIntensity")) throw new Exception("Fixture tint properties absent");
                instance.SetColor("_Tint", new Color(.1f * i, .3f, .7f, 1)); instance.SetFloat("_TintIntensity", .1f * (i + 1));
            }
            Set(player, "m_modelRenderers", renderers);
            var accessories = new List<Accessory>();
            if (withAccessories)
            {
                var go = new GameObject("Temporary accessory tint fixture"); objects.Add(go);
                var renderer = go.AddComponent<MeshRenderer>(); var mat = new Material(shader); materials.Add(mat); renderer.sharedMaterial = mat;
                var instance = renderer.material; if (instance != mat) materials.Add(instance);
                var accessory = go.AddComponent<Accessory>(); Set(accessory, "m_modelRenderer", renderer); accessories.Add(accessory);
                // Keep an opt-out accessory in the same ordered list.
                var ignored = go.AddComponent<Accessory>(); ignored.m_canTint = false; accessories.Add(ignored);
            }
            Set(player, "m_cachedAccessories", accessories);
            tick = (Action)Delegate.CreateDelegate(typeof(Action), player, typeof(FGOL.Player).GetMethod("UpdateTint", Fields));
        }
        public void State(int scenario)
        {
            Set(player, "m_healthPower", 0f); Set(player, "m_icedTime", scenario == 1 ? 1f : 0f);
            Set(player, "m_poisonTime", scenario == 2 ? 1f : 0f);
            Set(player, "m_poisonInvulnerabilityTimer", scenario == 3 ? 1f : 0f);
            Set(player, "m_damageTintBlend", scenario == 0 ? 0f : .75f);
            Set(player, "m_damageTint", scenario == 4 ? Color.black : Color.red);
        }
        public string Snapshot()
        {
            var b = new StringBuilder(); b.Append(Float((float)Field(typeof(FGOL.Player), "m_damageTintBlend").GetValue(player)));
            foreach (GameObject go in objects)
            {
                var mat = go.GetComponent<MeshRenderer>().material; var c = mat.GetColor("_Tint");
                b.Append('|').Append(Float(c.r)).Append(',').Append(Float(c.g)).Append(',').Append(Float(c.b)).Append(',').Append(Float(c.a)).Append(',').Append(Float(mat.GetFloat("_TintIntensity"))).Append(',').Append(mat.IsKeywordEnabled("TINT_ON"));
            }
            return b.ToString();
        }
        public void ReplaceMaterial()
        {
            var replacement = new Material(renderers[0].sharedMaterial); materials.Add(replacement);
            renderers[0].sharedMaterial = replacement;
            var instance = renderers[0].material; if (instance != replacement) materials.Add(instance);
        }
        public void Dispose()
        {
            foreach (var go in objects) UnityEngine.Object.DestroyImmediate(go);
            foreach (var mat in materials) if (mat != null) UnityEngine.Object.DestroyImmediate(mat);
        }
    }
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new Exception("Run fixture outside Play.");
        string root = Path.GetFullPath(Path.Combine(Application.dataPath, "../Reports/InteractiveProfiling/TintTrial")); Directory.CreateDirectory(root);
        bool previous = HSEHotpathTrial.Optimized;
        var report = new StringBuilder(); var bench = new StringBuilder("renderers,scenario,round,optimized,calls,elapsed_ms\n"); int cases = 0;
        try
        {
            foreach (int count in new int[] { 1, 4 })
            foreach (bool accessories in new bool[] { false, true })
            foreach (int scenario in new int[] { 0, 1, 2, 3, 4, 5 })
            using (var original = new Fixture(count, accessories))
            using (var optimized = new Fixture(count, accessories))
            {
                original.State(scenario); optimized.State(scenario);
                for (int step = 0; step < 3; step++)
                {
                    if (step == 1) { original.ReplaceMaterial(); optimized.ReplaceMaterial(); }
                    HSEHotpathTrial.Optimized = false; original.tick(); HSEHotpathTrial.Optimized = true; optimized.tick();
                    if (original.Snapshot() != optimized.Snapshot()) throw new Exception("Tint mismatch count=" + count + " accessories=" + accessories + " scenario=" + scenario + " step=" + step);
                    cases++;
                }
            }
            foreach (int count in new int[] { 1, 4 })
            foreach (int scenario in new int[] { 0, 2 })
            using (var f = new Fixture(count, false))
            {
                for (int round = 0; round < 5; round++)
                for (int pass = 0; pass < 2; pass++)
                {
                    HSEHotpathTrial.Optimized = (pass == 1) ^ (round % 2 == 1); f.State(scenario);
                    for (int i = 0; i < 100; i++) f.tick();
                    var timer = System.Diagnostics.Stopwatch.StartNew(); for (int i = 0; i < 10000; i++) f.tick(); timer.Stop();
                    bench.Append(count).Append(',').Append(scenario).Append(',').Append(round).Append(',').Append(HSEHotpathTrial.Optimized).Append(",10000,").Append(timer.Elapsed.TotalMilliseconds.ToString("R", System.Globalization.CultureInfo.InvariantCulture)).AppendLine();
                }
            }
            report.AppendLine("PASS exact material/color/intensity/keyword/player blend comparisons=" + cases);
            report.AppendLine("Covers zero tint, ice, poison, invulnerability, black tint, decay; 1/4 model renderers; opt-in/opt-out accessories; material reassignment.");
            report.AppendLine("No gameplay, stats, saves or visual rendering proof; startup/hunger/death still require manual regression.");
            File.WriteAllText(Path.Combine(root, "contracts.txt"), report.ToString()); File.WriteAllText(Path.Combine(root, "benchmark.csv"), bench.ToString());
        }
        finally { HSEHotpathTrial.Optimized = previous; }
    }
}
#endif

// Isolated dictionary contract; never touches GameLogic, player state or saves.
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Profiling;
public static class HSEAreaTimeContract
{
    static string root;
    static int frame;
    static bool previousEnabled, previousDriver, previousBinary, previousEditor;
    static string previousLog;
    static StringBuilder report;
    static void Step(Dictionary<AreaTransitionManager.Area, float> map, AreaTransitionManager.Area area, float delta, bool optimized)
    {
        if (optimized)
        {
            float time;
            map[area] = map.TryGetValue(area, out time) ? time + delta : 0f;
        }
        else if (!map.ContainsKey(area)) map[area] = 0f;
        else map[area] += delta;
    }
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || Profiler.enableBinaryLog) throw new Exception("Stop Play/binary capture first.");
        if (Enum.GetUnderlyingType(typeof(AreaTransitionManager.Area)) != typeof(int)) throw new Exception("Expected Int32 enum.");
        var a = new Dictionary<AreaTransitionManager.Area, float>();
        var b = new Dictionary<AreaTransitionManager.Area, float>(new HSEVitaAreaComparer());
        int tests = 0;
        // Include new areas, repeated visits, zero/negative delta, unknown enum keys and collisions/resizes.
        for (int i = 0; i < 10000; i++)
        {
            var area = (AreaTransitionManager.Area)(i % 71 - 35); float delta = (i % 13 - 2) * .016f;
            Step(a, area, delta, false); Step(b, area, delta, true);
            if (a.Count != b.Count || a[area] != b[area]) throw new Exception("Area-time dictionary mismatch");
            tests++;
        }
        root = Path.GetFullPath(Path.Combine(Application.dataPath, "../Reports/InteractiveProfiling/AreaTimeTrial")); Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "contracts.txt"), "PASS " + tests + " exact dictionary comparisons; Int32 enum\n");
        previousEnabled = Profiler.enabled; previousDriver = ProfilerDriver.enabled; previousBinary = Profiler.enableBinaryLog; previousLog = Profiler.logFile; previousEditor = ProfilerDriver.profileEditor;
        report = new StringBuilder("round,optimized,calls,elapsed_ms\n"); frame = 0;
        ProfilerDriver.profileEditor = true; Profiler.logFile = Path.Combine(root, "dictionary.raw"); Profiler.enableBinaryLog = true; ProfilerDriver.enabled = true; Profiler.enabled = true;
        EditorApplication.update += Tick;
    }
    static void Tick()
    {
        try
        {
            if (++frame > 7) { Finish(); return; }
            if (frame == 1) return;
            for (int pass = 0; pass < 2; pass++)
            {
                bool optimized = ((frame + pass) % 2) == 0;
                var map = optimized ? new Dictionary<AreaTransitionManager.Area, float>(new HSEVitaAreaComparer()) : new Dictionary<AreaTransitionManager.Area, float>();
                var area = (AreaTransitionManager.Area)0; map[area] = 0f;
                // Warm up outside the measured marker, keeping maps at identical sizes.
                Step(map, area, .016f, optimized);
                var watch = System.Diagnostics.Stopwatch.StartNew();
                Profiler.BeginSample(optimized ? "HSE.AreaTime.Optimized" : "HSE.AreaTime.Original");
                for (int i = 0; i < 1000; i++) Step(map, area, .016f, optimized);
                Profiler.EndSample(); watch.Stop();
                report.Append(frame).Append(',').Append(optimized).Append(',').Append(1000).Append(',').Append(watch.Elapsed.TotalMilliseconds.ToString("R", System.Globalization.CultureInfo.InvariantCulture)).AppendLine();
            }
        }
        catch { Finish(); throw; }
    }
    static void Finish()
    {
        EditorApplication.update -= Tick;
        Profiler.enabled = false; Profiler.enableBinaryLog = false;
        Profiler.logFile = previousLog; Profiler.enableBinaryLog = previousBinary; Profiler.enabled = previousEnabled; ProfilerDriver.enabled = previousDriver; ProfilerDriver.profileEditor = previousEditor;
        File.WriteAllText(Path.Combine(root, "benchmark.csv"), report.ToString());
    }
}
#endif

// Install in Assets/Editor. Unity 2018.2; no gameplay changes or player inclusion.
#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEditor;
using UnityEditorInternal;

public static class HSEGameplayCapture
{
    static HSEGameplayCaptureFrames capture;
    static bool wasEnabled, wasBinary, wasDeep, wasDriverEnabled;
    static bool wasProfileEditor;
    public static bool DiagnoseEditorFirstSegment;
    static string previousLog;
    public static bool Running { get { return capture != null; } }
    public static string Label = "Baseline_Gameplay";
    public static string LastDirectory;
    static int segment;

    [MenuItem("HSE/Profiling/Start gameplay capture _F8")]
    public static void Start()
    {
        if (!EditorApplication.isPlaying) throw new InvalidOperationException("Enter Play Mode first.");
        if (capture != null) throw new InvalidOperationException("A gameplay capture is already running.");
        if (Profiler.enableBinaryLog)
            throw new InvalidOperationException("Stop the existing binary Profiler recording before starting this capture.");
        string directory = Path.GetFullPath(Path.Combine(Application.dataPath,
            "../Reports/InteractiveProfiling/" + Label + "_" + DateTime.UtcNow.ToString("yyyyMMddTHHmmssfffZ")));
        Directory.CreateDirectory(directory);
        wasEnabled = Profiler.enabled; wasBinary = Profiler.enableBinaryLog;
        wasDriverEnabled = ProfilerDriver.enabled;
        previousLog = Profiler.logFile; wasDeep = ProfilerDriver.deepProfiling;
        wasProfileEditor = ProfilerDriver.profileEditor;
        if (DiagnoseEditorFirstSegment) ProfilerDriver.profileEditor = true;
        ProfilerDriver.deepProfiling = false;
        LastDirectory = directory; segment = 0;
        ProfilerDriver.enabled = true;
        Profiler.logFile = Path.Combine(directory, "segment_0000.raw");
        Profiler.enableBinaryLog = true; Profiler.enabled = true;
        GameObject go = new GameObject("Editor only bounded gameplay capture");
        UnityEngine.Object.DontDestroyOnLoad(go);
        capture = go.AddComponent<HSEGameplayCaptureFrames>();
        capture.Begin(directory);
        UnityEngine.Debug.Log("Capture started. Move, change area and eat prey; F9 stops and writes the buffered CSV. " + directory);
    }

    // Keep each raw below the verified 300-frame import history of this editor.
    // Rollover frames are marked in the CSV for exclusion from matched timings.
    public static void NextSegment()
    {
        Profiler.enabled = false; Profiler.enableBinaryLog = false;
        segment++;
        if (DiagnoseEditorFirstSegment) ProfilerDriver.profileEditor = false;
        Profiler.logFile = Path.Combine(LastDirectory, "segment_" + segment.ToString("D4") + ".raw");
        Profiler.enableBinaryLog = true; Profiler.enabled = true;
    }

    [MenuItem("HSE/Profiling/Stop gameplay capture _F9")]
    public static void Stop()
    {
        if (capture == null) return;
        var current = capture; capture = null;
        Profiler.enabled = false; Profiler.enableBinaryLog = false;
        ProfilerDriver.enabled = false;
        try { current.Finish(); }
        finally
        {
            Profiler.logFile = previousLog; Profiler.enableBinaryLog = wasBinary;
            ProfilerDriver.deepProfiling = wasDeep;
            ProfilerDriver.profileEditor = wasProfileEditor;
            ProfilerDriver.enabled = wasDriverEnabled;
            Profiler.enabled = wasEnabled;
            UnityEngine.Object.Destroy(current.gameObject);
        }
    }
}

public sealed class HSEGameplayCaptureFrames : MonoBehaviour
{
    const int Capacity = 20000;
    struct Row
    {
        public int frame, eats, gc, batches, draws, triangles, instances, destroys, boundary, segment, gcAllocSamples;
        public float seconds, ms, x, y, health;
        public long mainNs, renderNs, physicsNs, mono, allocated, reserved;
    }
    Row[] rows;
    int count, lastGC;
    float started;
    string directory;
    Recorder main, render, physics, instances, destroys, gcAlloc;
    readonly List<Recorder> owned = new List<Recorder>();
    readonly List<bool> prior = new List<bool>();
    Recorder Use(string name)
    {
        Recorder r = Recorder.Get(name);
        if (r == null || !r.isValid) return null;
        owned.Add(r); prior.Add(r.enabled); r.enabled = true; return r;
    }
    public void Begin(string path)
    {
        directory = path; rows = new Row[Capacity]; started = Time.realtimeSinceStartup;
        lastGC = GC.CollectionCount(0);
        main = Use("PlayerLoop"); render = Use("Gfx.ProcessCommands"); physics = Use("Physics.Processing");
        instances = Use("Instantiate"); destroys = Use("Destroy");
        gcAlloc = Use("GC.Alloc");
        File.WriteAllText(Path.Combine(path, "availability.txt"),
            "Unity=" + Application.unityVersion + " API=" + SystemInfo.graphicsDeviceType +
            " resolution=" + Screen.width + "x" + Screen.height + " quality=" + QualitySettings.GetQualityLevel() +
            " vSync=" + QualitySettings.vSyncCount + " targetFPS=" + Application.targetFrameRate +
            "\n-1 means unavailable. Recorder times are CPU scopes; frame_ms is wall frame time." +
            "\nGfx.ProcessCommands may include waits; it is not busy CPU time or GPU timing." +
            "\nA valid Recorder can still produce no completed samples; inspect CPU Main/Render Thread in unity.raw." +
            "\nallocated/reserved are Unity allocator totals, not free native RAM/VRAM." +
            "\nCSV excludes Deep Profiling. unity.raw has call trees, GC, animation, loading and physics markers." +
            "\nPlayerLoop=" + (main != null) + " Gfx.ProcessCommands=" + (render != null) +
            " Physics.Processing=" + (physics != null) + " Instantiate=" + (instances != null) + " Destroy=" + (destroys != null));
        File.WriteAllText(Path.Combine(path, "settings_start.txt"), HSEInteractiveProfiling.Describe());
    }
    static long Ns(Recorder r) { return r == null || r.sampleBlockCount == 0 ? -1 : r.elapsedNanoseconds; }
    void LateUpdate()
    {
        if (rows == null) return;
        var player = GameLogic.player;
        rows[count] = new Row {
            frame = Time.frameCount, seconds = Time.realtimeSinceStartup - started, ms = Time.unscaledDeltaTime * 1000,
            eats = GameLogic.numEats, gc = GC.CollectionCount(0) - lastGC,
            x = player == null ? float.NaN : player.position.x, y = player == null ? float.NaN : player.position.y,
            health = player == null ? float.NaN : player.healthPower,
            batches = UnityStats.batches, draws = UnityStats.drawCalls, triangles = UnityStats.triangles,
            instances = instances == null ? -1 : instances.sampleBlockCount,
            destroys = destroys == null ? -1 : destroys.sampleBlockCount,
            mainNs = Ns(main), renderNs = Ns(render), physicsNs = Ns(physics),
            mono = Profiler.GetMonoUsedSizeLong(), allocated = Profiler.GetTotalAllocatedMemoryLong(),
            reserved = Profiler.GetTotalReservedMemoryLong(),
            segment = count / 240, boundary = count % 240 < 2 ? 1 : 0,
            gcAllocSamples = gcAlloc == null ? -1 : gcAlloc.sampleBlockCount
        };
        count++;
        lastGC = GC.CollectionCount(0);
        if (count == Capacity || Time.realtimeSinceStartup - started >= 300) HSEGameplayCapture.Stop();
        else if (count % 240 == 0) HSEGameplayCapture.NextSegment();
    }
    public void Finish()
    {
        if (rows == null) return;
        for (int i = 0; i < owned.Count; i++) owned[i].enabled = prior[i];
        var b = new StringBuilder("frame,seconds,frame_ms,eats,x,y,health,gc,batches,draws,triangles,instantiate_samples,destroy_samples,main_ns,render_ns,physics_ns,mono_bytes,allocated_bytes,reserved_bytes,segment,capture_boundary,gc_alloc_samples\n");
        for (int i = 0; i < count; i++)
        {
            Row r = rows[i];
            b.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,
                "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21}\n",
                r.frame,r.seconds,r.ms,r.eats,r.x,r.y,r.health,r.gc,r.batches,r.draws,r.triangles,
                r.instances,r.destroys,r.mainNs,r.renderNs,r.physicsNs,r.mono,r.allocated,r.reserved,r.segment,r.boundary,r.gcAllocSamples);
        }
        File.WriteAllText(Path.Combine(directory, "frames.csv"), b.ToString()); rows = null;
        File.WriteAllText(Path.Combine(directory, "settings_end.txt"), HSEInteractiveProfiling.Describe());
        UnityEngine.Debug.Log("Capture saved: " + directory);
    }
    void OnDisable() { if (rows != null) HSEGameplayCapture.Stop(); }
}
#endif

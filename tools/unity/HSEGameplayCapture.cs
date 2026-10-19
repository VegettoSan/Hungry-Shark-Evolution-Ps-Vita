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
    static bool wasEnabled, wasBinary, wasDeep;
    static string previousLog;

    [MenuItem("HSE/Profiling/Start gameplay capture _F8")]
    public static void Start()
    {
        if (!EditorApplication.isPlaying) throw new InvalidOperationException("Enter Play Mode first.");
        if (capture != null) throw new InvalidOperationException("A gameplay capture is already running.");
        if (Profiler.enabled || Profiler.enableBinaryLog)
            throw new InvalidOperationException("Stop the existing Profiler recording before starting this capture.");
        string directory = Path.GetFullPath(Path.Combine(Application.dataPath,
            "../Reports/GameplayCapture_" + DateTime.UtcNow.ToString("yyyyMMddTHHmmssfffZ")));
        Directory.CreateDirectory(directory);
        wasEnabled = Profiler.enabled; wasBinary = Profiler.enableBinaryLog;
        previousLog = Profiler.logFile; wasDeep = ProfilerDriver.deepProfiling;
        ProfilerDriver.deepProfiling = false;
        Profiler.logFile = Path.Combine(directory, "unity.raw");
        Profiler.enableBinaryLog = true; Profiler.enabled = true;
        GameObject go = new GameObject("Editor only bounded gameplay capture");
        UnityEngine.Object.DontDestroyOnLoad(go);
        capture = go.AddComponent<HSEGameplayCaptureFrames>();
        capture.Begin(directory);
        UnityEngine.Debug.Log("Capture started. Move, change area and eat prey; F9 stops and writes the buffered CSV. " + directory);
    }

    [MenuItem("HSE/Profiling/Stop gameplay capture _F9")]
    public static void Stop()
    {
        if (capture == null) return;
        var current = capture; capture = null;
        Profiler.enabled = false; Profiler.enableBinaryLog = false;
        try { current.Finish(); }
        finally
        {
            Profiler.logFile = previousLog; Profiler.enableBinaryLog = wasBinary;
            ProfilerDriver.deepProfiling = wasDeep;
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
        public int frame, eats, gc, batches, draws, triangles, instances, destroys;
        public float seconds, ms, x, y, health;
        public long mainNs, renderNs, physicsNs, mono, allocated, reserved;
    }
    Row[] rows;
    int count, lastGC;
    float started;
    string directory;
    Recorder main, render, physics, instances, destroys;
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
    }
    static long Ns(Recorder r) { return r == null || r.sampleBlockCount == 0 ? -1 : r.elapsedNanoseconds; }
    void LateUpdate()
    {
        if (rows == null) return;
        var player = GameLogic.player;
        rows[count++] = new Row {
            frame = Time.frameCount, seconds = Time.realtimeSinceStartup - started, ms = Time.unscaledDeltaTime * 1000,
            eats = GameLogic.numEats, gc = GC.CollectionCount(0) - lastGC,
            x = player == null ? float.NaN : player.position.x, y = player == null ? float.NaN : player.position.y,
            health = player == null ? float.NaN : player.healthPower,
            batches = UnityStats.batches, draws = UnityStats.drawCalls, triangles = UnityStats.triangles,
            instances = instances == null ? -1 : instances.sampleBlockCount,
            destroys = destroys == null ? -1 : destroys.sampleBlockCount,
            mainNs = Ns(main), renderNs = Ns(render), physicsNs = Ns(physics),
            mono = Profiler.GetMonoUsedSizeLong(), allocated = Profiler.GetTotalAllocatedMemoryLong(),
            reserved = Profiler.GetTotalReservedMemoryLong()
        };
        lastGC = GC.CollectionCount(0);
        if (count == Capacity) HSEGameplayCapture.Stop();
    }
    public void Finish()
    {
        if (rows == null) return;
        for (int i = 0; i < owned.Count; i++) owned[i].enabled = prior[i];
        var b = new StringBuilder("frame,seconds,frame_ms,eats,x,y,health,gc,batches,draws,triangles,instantiate_samples,destroy_samples,main_ns,render_ns,physics_ns,mono_bytes,allocated_bytes,reserved_bytes\n");
        for (int i = 0; i < count; i++)
        {
            Row r = rows[i];
            b.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,
                "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18}\n",
                r.frame,r.seconds,r.ms,r.eats,r.x,r.y,r.health,r.gc,r.batches,r.draws,r.triangles,
                r.instances,r.destroys,r.mainNs,r.renderNs,r.physicsNs,r.mono,r.allocated,r.reserved);
        }
        File.WriteAllText(Path.Combine(directory, "frames.csv"), b.ToString()); rows = null;
        UnityEngine.Debug.Log("Capture saved: " + directory);
    }
    void OnDisable() { if (rows != null) HSEGameplayCapture.Stop(); }
}
#endif

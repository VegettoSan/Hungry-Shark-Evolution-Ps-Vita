// Temporary Editor-only A/B selector and CPU sample. No player code or per-frame logs.
#if UNITY_EDITOR
using System;
using UnityEngine.Profiling;
public static class HSEHotpathTrial
{
    public static bool Optimized = true;
    public static Scope Tint() { return new Scope(Optimized ? "HSE.PlayerTint.Optimized" : "HSE.PlayerTint.Original"); }
    public static Scope Measure(string label) { return new Scope(label); }
    public struct Scope : IDisposable
    {
        bool active;
        public Scope(string label) { active = Profiler.enabled; if (active) Profiler.BeginSample(label); }
        public void Dispose() { if (active) Profiler.EndSample(); }
    }
}
#endif

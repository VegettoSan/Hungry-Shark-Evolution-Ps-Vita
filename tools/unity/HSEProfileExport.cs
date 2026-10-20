// Offline analysis only, after stopping Play. No per-frame reflection during gameplay.
#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;

public static class HSEProfileExport
{
    static readonly BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    static string Quote(object s) { return "\"" + Convert.ToString(s, System.Globalization.CultureInfo.InvariantCulture).Replace("\"", "\"\"") + "\""; }
    static object Get(Type t, object obj, string name) { return t.GetProperty(name, Flags).GetValue(obj, null); }
    public static void Detail(string name, string segment)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new Exception("Stop Play first.");
        if (Path.GetFileName(name) != name || !System.Text.RegularExpressions.Regex.IsMatch(segment, "^[0-9]{4}$")) throw new Exception("Invalid capture/segment.");
        string root = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "../Reports/InteractiveProfiling/" + name));
        Type driver = typeof(EditorApplication).Assembly.GetType("UnityEditorInternal.ProfilerDriver", true);
        Type iterator = typeof(EditorApplication).Assembly.GetType("UnityEditorInternal.ProfilerFrameDataIterator", true);
        if (!(bool)driver.GetMethod("LoadProfile", Flags).Invoke(null, new object[] { Path.Combine(root, "segment_" + segment + ".raw"), false })) throw new Exception("Cannot load raw.");
        int first = (int)Get(driver, null, "firstFrameIndex"), end = (int)Get(driver, null, "lastFrameIndex");
        var values = new Dictionary<string, double[]>(); var info = new Dictionary<string, string>();
        var spikes = new StringBuilder("native_frame,start_ms,duration_ms,path,info\n");
        object it = Activator.CreateInstance(iterator);
        try
        {
            for (int frame = first + 1; frame <= end; frame++)
            {
                iterator.GetMethod("SetRoot", Flags).Invoke(it, new object[] { frame, 0 });
                if (((string)iterator.GetMethod("GetThreadName", Flags).Invoke(it, null)).IndexOf("Main", StringComparison.OrdinalIgnoreCase) < 0) throw new Exception("Expected Main Thread at index zero.");
                while ((bool)iterator.GetMethod("Next", Flags).Invoke(it, new object[] { true }))
                {
                    string label = (string)Get(iterator, it, "name");
                    double ms = Convert.ToDouble(Get(iterator, it, "durationMS"));
                    if (label != "GC.Alloc" && ms < 10) continue;
                    string path = (string)Get(iterator, it, "path"), tooltip = (string)Get(iterator, it, "extraTooltipInfo");
                    if (label == "GC.Alloc")
                    {
                        double[] v; if (!values.TryGetValue(path, out v)) { v = new double[4]; values.Add(path, v); info.Add(path, tooltip); }
                        v[0]++; v[1] += ms;
                        long bytes;
                        if (tooltip != null && tooltip.StartsWith("size: ") && long.TryParse(tooltip.Substring(6).Trim(), out bytes)) v[2] += bytes;
                        else v[3]++;
                    }
                    else spikes.Append(frame).Append(',').Append(Convert.ToString(Get(iterator, it, "startTimeMS"), System.Globalization.CultureInfo.InvariantCulture)).Append(',').Append(ms.ToString("R", System.Globalization.CultureInfo.InvariantCulture)).Append(',').Append(Quote(path)).Append(',').Append(Quote(tooltip)).AppendLine();
                }
            }
        }
        finally { ((IDisposable)it).Dispose(); }
        var allocation = new StringBuilder("path,events,total_ms,bytes,unparsed_size_events,first_sample_info\n");
        foreach (var pair in values) allocation.Append(Quote(pair.Key)).Append(',').Append(pair.Value[0]).Append(',').Append(pair.Value[1].ToString("R", System.Globalization.CultureInfo.InvariantCulture)).Append(',').Append(pair.Value[2]).Append(',').Append(pair.Value[3]).Append(',').Append(Quote(info[pair.Key])).AppendLine();
        File.WriteAllText(Path.Combine(root, "segment_" + segment + "_allocations.csv"), allocation.ToString());
        File.WriteAllText(Path.Combine(root, "segment_" + segment + "_spikes.csv"), spikes.ToString());
    }
    public static void Run(string name)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new Exception("Stop Play before offline export.");
        if (Path.GetFileName(name) != name) throw new Exception("Capture folder name only.");
        string root = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "../Reports/InteractiveProfiling/" + name));
        Type driver = typeof(EditorApplication).Assembly.GetType("UnityEditorInternal.ProfilerDriver", true);
        Type iterator = typeof(EditorApplication).Assembly.GetType("UnityEditorInternal.ProfilerFrameDataIterator", true);
        string[] properties = (string[])driver.GetMethod("GetAllStatisticsProperties", Flags).Invoke(null, null);
        File.WriteAllLines(Path.Combine(root, "statistics_properties.txt"), properties);
        var selected = new List<string>(); var ids = new List<int>();
        foreach (string s in properties)
            if (!s.StartsWith("Selected") && !selected.Contains(s))
            { selected.Add(s); ids.Add((int)driver.GetMethod("GetStatisticsIdentifier", Flags).Invoke(null, new object[] { s })); }
        var coverage = new StringBuilder("segment,native_frames,threads,max_main_frame_ms\n");
        foreach (string raw in Directory.GetFiles(root, "segment_*.raw"))
        {
            bool ok = (bool)driver.GetMethod("LoadProfile", Flags).Invoke(null, new object[] { raw, false });
            if (!ok) throw new Exception("Cannot read " + Path.GetFileName(raw));
            int first = (int)Get(driver, null, "firstFrameIndex"), end = (int)Get(driver, null, "lastFrameIndex");
            var frames = new StringBuilder("native_frame,thread,native_wall_frame_ms,samples\n");
            var stats = new StringBuilder("native_frame"); foreach (string s in selected) stats.Append(',').Append(Quote(s)); stats.AppendLine();
            var totals = new Dictionary<string, double[]>(); int maxThreads = 0; double maxMain = 0;
            object it = Activator.CreateInstance(iterator);
            try
            {
                for (int frame = first; frame <= end; frame++)
                {
                    int threads = (int)iterator.GetMethod("GetThreadCount", Flags).Invoke(it, new object[] { frame }); maxThreads = Math.Max(maxThreads, threads);
                    for (int thread = 0; thread < threads; thread++)
                    {
                        iterator.GetMethod("SetRoot", Flags).Invoke(it, new object[] { frame, thread });
                        string threadName = (string)iterator.GetMethod("GetThreadName", Flags).Invoke(it, null);
                        double frameMs = Convert.ToDouble(Get(iterator, it, "frameTimeMS")); int count = 0;
                        if (threadName.IndexOf("Main", StringComparison.OrdinalIgnoreCase) >= 0) maxMain = Math.Max(maxMain, frameMs);
                        while ((bool)iterator.GetMethod("Next", Flags).Invoke(it, new object[] { true }))
                        {
                            string label = threadName + "\t" + Get(iterator, it, "name");
                            double ms = Convert.ToDouble(Get(iterator, it, "durationMS")); double[] value;
                            if (!totals.TryGetValue(label, out value)) { value = new double[3]; totals.Add(label, value); }
                            value[0] += ms; value[1]++; value[2] = Math.Max(value[2], ms); count++;
                        }
                        frames.Append(frame).Append(',').Append(Quote(threadName)).Append(',').Append(frameMs.ToString("R", System.Globalization.CultureInfo.InvariantCulture)).Append(',').Append(count).AppendLine();
                    }
                    stats.Append(frame);
                    for (int i = 0; i < ids.Count; i++) stats.Append(',').Append(Quote(driver.GetMethod("GetFormattedStatisticsValue", Flags).Invoke(null, new object[] { frame, ids[i] })));
                    stats.AppendLine();
                }
            }
            finally { ((IDisposable)it).Dispose(); }
            string stem = Path.Combine(root, Path.GetFileNameWithoutExtension(raw));
            File.WriteAllText(stem + "_threads.csv", frames.ToString()); File.WriteAllText(stem + "_statistics.csv", stats.ToString());
            var names = new List<string>(totals.Keys); names.Sort((a,b) => totals[b][0].CompareTo(totals[a][0]));
            var aggregate = new StringBuilder("thread\tmarker\ttotal_inclusive_ms\tcount\tmax_ms\n");
            foreach (string label in names) { var t = totals[label]; aggregate.Append(label).Append('\t').Append(t[0].ToString("R", System.Globalization.CultureInfo.InvariantCulture)).Append('\t').Append(t[1]).Append('\t').Append(t[2].ToString("R", System.Globalization.CultureInfo.InvariantCulture)).AppendLine(); }
            File.WriteAllText(stem + "_markers.tsv", aggregate.ToString());
            coverage.Append(Path.GetFileName(raw)).Append(',').Append(end-first+1).Append(',').Append(maxThreads).Append(',').Append(maxMain.ToString("R", System.Globalization.CultureInfo.InvariantCulture)).AppendLine();
        }
        File.WriteAllText(Path.Combine(root, "coverage.csv"), coverage.ToString());
    }
}
#endif

// Unity 2018.2 only. Install with HSEGameplayCapture in Assets/Editor.
// Control files and all captures stay in the owner's private project Reports/.
#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using Turnip;

[InitializeOnLoad]
public static class HSEInteractiveProfiling
{
    const string Key = "HSE.InteractiveProfiling";
    static string Root { get { return Path.GetFullPath(Path.Combine(Application.dataPath, "../Reports/InteractiveProfiling")); } }
    static string last;
    static double deadline;
    static double nextPoll;
    static DateTime requestTime;
    static int smokeStart;
    static readonly StringBuilder errors = new StringBuilder();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void BootPresentation()
    {
        if (!SessionState.GetBool(Key, false)) return;
        ApplyPresentation(); SceneManager.sceneLoaded += ScenePresentation;
    }
    static void ScenePresentation(Scene s, LoadSceneMode mode) { ApplyPresentation(); }
    static void ApplyPresentation() { QualitySettings.vSyncCount = 1; Application.targetFrameRate = 60; }
    static HSEInteractiveProfiling()
    {
        EditorApplication.update += Tick;
        EditorApplication.playModeStateChanged += State;
        Application.logMessageReceived += Log;
    }
    static void Log(string message, string stack, LogType type)
    {
        if ((type == LogType.Exception || type == LogType.Error || type == LogType.Assert) && errors.Length < 131072)
            errors.AppendLine(type + " frame=" + Time.frameCount + " " + message + "\n" + stack);
    }
    static void State(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode) HSEGameplayCapture.Stop();
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            RestoreRuntime();
            if (SessionState.GetBool(Key + ".Smoke", false))
            {
                SessionState.SetBool(Key + ".Smoke", false);
                EditorSceneManager.OpenScene(SessionState.GetString(Key + ".Scene", "Assets/WIP/SplashLoaderScene.unity"));
                File.WriteAllText(Path.Combine(Root, "smoke_finished.txt"), "EMPTY SCENE ONLY; no gameplay/save test\n" + Describe());
            }
        }
    }
    static void RestoreRuntime()
    {
        if (!SessionState.GetBool(Key + ".Saved", false)) return;
        QualitySettings.SetQualityLevel(SessionState.GetInt(Key + ".Quality", 0), true);
        QualitySettings.vSyncCount = SessionState.GetInt(Key + ".Vsync", 0);
        Application.targetFrameRate = SessionState.GetInt(Key + ".Target", -1);
        QualitySettings.masterTextureLimit = SessionState.GetInt(Key + ".Mip", 1);
        QualitySettings.antiAliasing = SessionState.GetInt(Key + ".AA", 0);
        if (SessionState.GetBool(Key + ".Low", false)) Shader.EnableKeyword("LOW_END"); else Shader.DisableKeyword("LOW_END");
        if (SessionState.GetBool(Key + ".High", false)) Shader.EnableKeyword("HIGH_END"); else Shader.DisableKeyword("HIGH_END");
    }
    [MenuItem("HSE/Profiling/Prepare Vita Medium session")]
    public static void Prepare()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new Exception("Stop Play before preparation.");
        foreach (var s in EditorSceneManager.GetSceneManagerSetup())
            if (SceneManager.GetSceneByPath(s.path).isDirty) throw new Exception("Unsaved scene; preserve owner changes first.");
        if (!SessionState.GetBool(Key + ".Saved", false))
        {
            SessionState.SetBool(Key + ".Saved", true);
            SessionState.SetInt(Key + ".Quality", QualitySettings.GetQualityLevel());
            SessionState.SetInt(Key + ".Vsync", QualitySettings.vSyncCount);
            SessionState.SetInt(Key + ".Target", Application.targetFrameRate);
            SessionState.SetInt(Key + ".Mip", QualitySettings.masterTextureLimit);
            SessionState.SetInt(Key + ".AA", QualitySettings.antiAliasing);
            SessionState.SetBool(Key + ".Low", Shader.IsKeywordEnabled("LOW_END"));
            SessionState.SetBool(Key + ".High", Shader.IsKeywordEnabled("HIGH_END"));
        }
        SetGameView();
        SessionState.SetBool(Key, true);
        Profiler.enabled = false; Profiler.enableBinaryLog = false; ProfilerDriver.enabled = false; ProfilerDriver.deepProfiling = false;
        ProfilerDriver.profileEditor = false;
        ConfigureCharts(false);
        Profiler.enabled = false; ProfilerDriver.enabled = false;
        Directory.CreateDirectory(Root);
        EditorSceneManager.OpenScene("Assets/WIP/SplashLoaderScene.unity");
        File.WriteAllText(Path.Combine(Root, "prepared.txt"), Describe());
    }
    static void SetGameView()
    {
        Type viewType = typeof(Editor).Assembly.GetType("UnityEditor.GameView", true);
        var view = EditorWindow.GetWindow(viewType);
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        var index = viewType.GetProperty("selectedSizeIndex", flags);
        if (!SessionState.GetBool(Key + ".GameViewSaved", false))
        {
            SessionState.SetInt(Key + ".GameViewIndex", (int)index.GetValue(view, null));
            SessionState.SetBool(Key + ".GameViewMaximized", view.maximized);
            SessionState.SetBool(Key + ".GameViewSaved", true);
        }
        Type sizes = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes", true);
        object singleton = sizes.BaseType.GetProperty("instance", flags).GetValue(null, null);
        object groupType = sizes.GetProperty("currentGroupType", flags).GetValue(singleton, null);
        object group = sizes.GetMethod("GetGroup", flags).Invoke(singleton, new object[] { groupType });
        Type gt = group.GetType(); int selected = -1;
        int total = (int)gt.GetMethod("GetTotalCount", flags).Invoke(group, null);
        for (int i = 0; i < total; i++)
        {
            object size = gt.GetMethod("GetGameViewSize", flags).Invoke(group, new object[] { i });
            if ((int)size.GetType().GetProperty("width", flags).GetValue(size, null) == 960 &&
                (int)size.GetType().GetProperty("height", flags).GetValue(size, null) == 544) { selected = i; break; }
        }
        if (selected < 0)
        {
            Type sizeType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize", true);
            Type mode = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizeType", true);
            object size = Activator.CreateInstance(sizeType, flags, null, new object[] { Enum.Parse(mode, "FixedResolution"), 960, 544, "HSE Vita Profiling" }, null);
            gt.GetMethod("AddCustomSize", flags).Invoke(group, new object[] { size }); selected = total;
        }
        index.SetValue(view, selected, null); view.Repaint();
    }
    [MenuItem("HSE/Profiling/Restore session preview")]
    public static void Restore()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new Exception("Stop Play first.");
        RestoreRuntime(); SessionState.SetBool(Key, false);
        ConfigureCharts(true);
        if (SessionState.GetBool(Key + ".GameViewSaved", false))
        {
            Type t = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
            t.GetProperty("selectedSizeIndex", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(EditorWindow.GetWindow(t), SessionState.GetInt(Key + ".GameViewIndex", 0), null);
            EditorWindow.GetWindow(t).maximized = SessionState.GetBool(Key + ".GameViewMaximized", false);
        }
        SessionState.SetBool(Key + ".Saved", false); SessionState.SetBool(Key + ".GameViewSaved", false);
    }
    static void ConfigureCharts(bool restore)
    {
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        Type t = typeof(Editor).Assembly.GetType("UnityEditor.ProfilerWindow", true);
        var window = EditorWindow.GetWindow(t);
        t.GetMethod("InitializeIfNeeded", flags).Invoke(window, null);
        var charts = (Array)t.GetField("m_Charts", flags).GetValue(window);
        var report = new StringBuilder();
        foreach (object chart in charts)
        {
            if (chart == null) continue;
            Type ct = chart.GetType(); string area = ct.GetField("m_Area", flags).GetValue(chart).ToString();
            var active = ct.GetProperty("active", flags);
            string chartKey = Key + ".Chart." + area;
            if (!SessionState.GetBool(chartKey + ".Saved", false)) { SessionState.SetBool(chartKey, (bool)active.GetValue(chart, null)); SessionState.SetBool(chartKey + ".Saved", true); }
            bool enabled = restore ? SessionState.GetBool(chartKey, false) : area == "CPU" || area == "Rendering" || area == "Memory" || area == "Physics" || area == "UI";
            active.SetValue(chart, enabled, null);
            report.AppendLine(area + "=" + active.GetValue(chart, null));
            if (restore) SessionState.SetBool(chartKey + ".Saved", false);
        }
        Directory.CreateDirectory(Root); File.WriteAllText(Path.Combine(Root, restore ? "charts_restored.txt" : "charts.txt"), report.ToString());
        window.Repaint();
    }
    public static string Describe()
    {
        var b = new StringBuilder();
        b.AppendLine("Unity=" + Application.unityVersion + " target=" + EditorUserBuildSettings.activeBuildTarget + " playing=" + EditorApplication.isPlaying + " prepared=" + SessionState.GetBool(Key, false));
        b.AppendLine("resolution=" + Screen.width + "x" + Screen.height + " API=" + SystemInfo.graphicsDeviceType + " quality=" + QualitySettings.names[QualitySettings.GetQualityLevel()]);
        b.AppendLine("shadows=" + QualitySettings.shadows + " shadowDistance=" + QualitySettings.shadowDistance + " AA=" + QualitySettings.antiAliasing + " mip=" + QualitySettings.masterTextureLimit + " anisotropic=" + QualitySettings.anisotropicFiltering);
        b.AppendLine("LOD=" + QualitySettings.lodBias + " maxLOD=" + QualitySettings.maximumLODLevel + " vSync=" + QualitySettings.vSyncCount + " targetFPS=" + Application.targetFrameRate + " softParticles=" + QualitySettings.softParticles);
        b.AppendLine("LOW_END=" + Shader.IsKeywordEnabled("LOW_END") + " HIGH_END=" + Shader.IsKeywordEnabled("HIGH_END") + " recording=" + HSEGameplayCapture.Running + " nativeRecord=" + Profiler.enabled + " binary=" + Profiler.enableBinaryLog + " deep=" + ProfilerDriver.deepProfiling + " profileEditor=" + ProfilerDriver.profileEditor);
        if (EditorApplication.isPlaying && King._Instance != null)
            b.AppendLine("deviceType=" + MonoSingleton<PlatformUtils>.Instance.deviceType + " state=" + King._Instance.CurrentGameState + " atlas=" + (GUIResolutionSwitchController.Instance == null ? "unavailable" : GUIResolutionSwitchController.Instance.quality.ToString()));
        for (int i = 0; i < SceneManager.sceneCount; i++) b.AppendLine("scene=" + SceneManager.GetSceneAt(i).name);
        foreach (Camera c in Camera.allCameras) b.AppendLine("camera=" + c.name + " clip=" + c.nearClipPlane + "/" + c.farClipPlane + " path=" + c.actualRenderingPath + " HDR=" + c.allowHDR + " MSAA=" + c.allowMSAA);
        return b.ToString();
    }
    static void Smoke()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new Exception("Stop Play first.");
        var setup = EditorSceneManager.GetSceneManagerSetup();
        if (setup.Length != 1 || SceneManager.GetActiveScene().isDirty) throw new Exception("Smoke requires one saved scene.");
        SessionState.SetString(Key + ".Scene", setup[0].path);
        SessionState.SetBool(Key + ".Smoke", true);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorApplication.isPlaying = true;
    }
    static void Tick()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        if (SessionState.GetBool(Key + ".Smoke", false) && EditorApplication.isPlaying && Time.frameCount >= 60)
        {
            if (!HSEGameplayCapture.Running && deadline == 0)
            {
                HSEGameplayCapture.Label = "Capture_SelfTest_EmptyScene";
                Application.targetFrameRate = 120;
                Profiler.enabled = false; Profiler.enableBinaryLog = false; ProfilerDriver.enabled = false;
                try { HSEGameplayCapture.Start(); smokeStart = Time.frameCount; deadline = EditorApplication.timeSinceStartup + 30; }
                catch (Exception e) { SessionState.SetBool(Key + ".Smoke", false); File.WriteAllText(Path.Combine(Root, "smoke_failed.txt"), e.ToString()); EditorApplication.isPlaying = false; }
            }
            if (deadline != 0 && (Time.frameCount - smokeStart >= 520 || EditorApplication.timeSinceStartup > deadline))
            {
                HSEGameplayCapture.Stop(); deadline = 0; EditorApplication.isPlaying = false;
            }
        }
        if (EditorApplication.timeSinceStartup < nextPoll) return;
        nextPoll = EditorApplication.timeSinceStartup + .5;
        string request = Path.Combine(Root, "request.txt");
        if (!File.Exists(request)) return;
        DateTime modified = File.GetLastWriteTimeUtc(request); if (modified == requestTime) return;
        string value; try { value = File.ReadAllText(request).Trim(); } catch (IOException) { return; }
        requestTime = modified;
        if (value == last || value == SessionState.GetString(Key + ".LastRequest", "")) return;
        last = value; SessionState.SetString(Key + ".LastRequest", value);
        var p = value.Split('|'); string id = p[0];
        if (!System.Text.RegularExpressions.Regex.IsMatch(id, "^[a-zA-Z0-9_]+$")) return;
        try
        {
            switch (p[1])
            {
                case "prepare": Prepare(); break;
                case "status": break;
                case "smoke": Smoke(); break;
                case "boot":
                    if (!SessionState.GetBool(Key, false) || EditorApplication.isPlayingOrWillChangePlaymode) throw new Exception("Prepare outside Play first.");
                    EditorApplication.isPlaying = true; break;
                case "endplay": HSEGameplayCapture.Stop(); EditorApplication.isPlaying = false; break;
                case "export": HSEProfileExport.Run(p[2]); break;
                case "detail": HSEProfileExport.Detail(p[2], p[3]); break;
                case "tint_contract": typeof(HSEInteractiveProfiling).Assembly.GetType("HSEPlayerTintContract", true).GetMethod("Run").Invoke(null, null); break;
                case "area_contract": typeof(HSEInteractiveProfiling).Assembly.GetType("HSEAreaTimeContract", true).GetMethod("Run").Invoke(null, null); break;
                case "diagnostic": HSEGameplayCapture.DiagnoseEditorFirstSegment = true; break;
                case "label":
                    if (!System.Text.RegularExpressions.Regex.IsMatch(p[2], "^[a-zA-Z0-9_]+$")) throw new Exception("Invalid label");
                    HSEGameplayCapture.Label = p[2]; HSEGameplayCapture.DiagnoseEditorFirstSegment = false; break;
                case "gameview":
                    var gameview = EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView", true));
                    gameview.maximized = true; gameview.Focus(); break;
                case "trial":
                    typeof(GameLogic).Assembly.GetType("HSEHotpathTrial", true).GetField("Optimized").SetValue(null, p[2] == "optimized"); break;
                case "api":
                    var api = new StringBuilder();
                    foreach (Type t in typeof(Editor).Assembly.GetTypes())
                        if (t.Name == "ProfilerWindow" || t.Name == "ProfilerChart" || t.Name == "ProfilerFrameDataIterator") { api.AppendLine(t.FullName); foreach (var m in t.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)) api.AppendLine(m.ToString()); }
                    File.WriteAllText(Path.Combine(Root, "profiler_api.txt"), api.ToString()); break;
                case "start":
                    if (p.Length > 2) { if (!System.Text.RegularExpressions.Regex.IsMatch(p[2], "^[a-zA-Z0-9_]+$")) throw new Exception("Invalid label"); HSEGameplayCapture.Label = p[2]; }
                    HSEGameplayCapture.Start(); break;
                case "stop": HSEGameplayCapture.Stop(); break;
                case "restore": Restore(); break;
                default: throw new Exception("Unsupported profiling command");
            }
            File.WriteAllText(Path.Combine(Root, id + ".txt"), Describe());
        }
        catch (Exception e) { File.WriteAllText(Path.Combine(Root, id + "_FAILED.txt"), e.ToString()); }
        if (errors.Length > 0) { File.WriteAllText(Path.Combine(Root, "errors.txt"), errors.ToString()); errors.Length = 0; }
    }
}
#endif

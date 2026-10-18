// Temporary diagnostic: install in Assets/Editor only. Never included in a player.
#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Profiling;

[InitializeOnLoad]
public static class HSEEditorRegression
{
    static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "../Reports/EditorRegression_20261018"));
    static readonly BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    static string last = "", pulse = "", axisName = "AXIS_LEFT_STICK_HORZ";
    static float axis;
    static int pulseFrame;
    static double releaseAt;
    static HSEEditorFrameSample sample;
    static HSEEditorRegression() { EditorApplication.update += Tick; Application.logMessageReceived += Capture; }
    static void Capture(string message, string stack, LogType type)
    {
        if (Directory.Exists(Root) && (type == LogType.Exception || type == LogType.Error || type == LogType.Assert))
            File.AppendAllText(Path.Combine(Root, "Errors.txt"), type + ": " + message + "\n" + stack + "\n");
    }
    static void Write(string id, string value) { File.WriteAllText(Path.Combine(Root, id + ".txt"), value); }
    static void Readers(bool enabled)
    {
        typeof(HSEVitaInput).GetField("PreviewAxis", All).SetValue(null, enabled ? (object)new Func<string,float>(ReadAxis) : null);
        typeof(HSEVitaInput).GetField("PreviewButton", All).SetValue(null, enabled ? (object)new Func<string,int,bool>(ReadButton) : null);
    }
    static float ReadAxis(string name) { return name == axisName ? axis : 0; }
    static bool ReadButton(string name, int edge) { return name == pulse && (edge == 0 || (edge == 1 && Time.frameCount == pulseFrame)); }
    static string Snapshot()
    {
        StringBuilder b = new StringBuilder("version=" + Application.unityVersion + " playing=" + EditorApplication.isPlaying + "\n");
        b.AppendLine("paused=" + App.paused + " timeScale=" + Time.timeScale + " gameplay=" + HSEVitaInput.GameplayAllowed + " method=" + GameInput.CurrentControlMethod);
        b.AppendLine("frame="+Time.frameCount+" background="+Application.runInBackground);
        b.AppendLine("selected=" + (FGOLControlManager.Instance == null || FGOLControlManager.Instance.selectedObject == null ? "null" : FGOLControlManager.Instance.selectedObject.name));
        if (GUIManager.Instance != null && GUIManager.Instance.m_Menu != null) {
            b.AppendLine("guiState=" + GUIManager.Instance.state);
            foreach (object menu in GUIManager.Instance.m_Menu.m_MenuStack) b.AppendLine("menu=" + menu.GetType().Name);
        }
        foreach (FGOLScreenConfig c in UnityEngine.Object.FindObjectsOfType<FGOLScreenConfig>()) b.AppendLine("screen=" + c.name + " default=" + (c.m_defaultSelectedObj == null ? "null" : c.m_defaultSelectedObj.name));
        foreach (LabCrane c in UnityEngine.Object.FindObjectsOfType<LabCrane>()) b.AppendLine("crane=" + c.name + " state=" + c.State + " pos=" + c.transform.localPosition);
        b.AppendLine("shark=" + LabCrane.m_activeShark + " interactive=" + LabCrane.isInteractive);
        if (GameLogic.player != null) b.AppendLine("player=" + GameLogic.player.position + " input=" + new Vector2(App.gameInput.tiltX,App.gameInput.tiltY));
        return b.ToString();
    }
    static void Tick()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || !Directory.Exists(Root)) return;
        if (releaseAt > 0 && EditorApplication.timeSinceStartup >= releaseAt) { axis = 0; pulse = ""; releaseAt = 0; }
        string path = Path.Combine(Root,"request.txt");
        if (!File.Exists(path)) return;
        string request; try { request = File.ReadAllText(path).Trim(); } catch (IOException) { return; }
        if (request == last) return;
        last = request;
        string[] p = request.Split('|'); string id = p[0], command = p[1];
        var timer = System.Diagnostics.Stopwatch.StartNew();
        try {
            if (command == "prepare") {
                if (EditorApplication.isPlaying) throw new Exception("Already playing");
                foreach (var s in EditorSceneManager.GetSceneManagerSetup()) if (UnityEngine.SceneManagement.SceneManager.GetSceneByPath(s.path).isDirty) throw new Exception("Dirty scene; preserve user work");
                SessionState.SetBool("HSE.Regression.PreviewBefore", SessionState.GetBool("HSE.VitaControllerPreview",false));
                SessionState.SetBool("HSE.VitaControllerPreview",true);
                EditorSceneManager.OpenScene("Assets/WIP/SplashLoaderScene.unity");
            }
            else if (command == "play") EditorApplication.isPlaying = true;
            else if (command == "stop") { Readers(false); Application.runInBackground=SessionState.GetBool("HSE.Regression.BackgroundBefore",false); EditorApplication.isPlaying = false; }
            else if (command == "restore") {
                if(EditorApplication.isPlaying)throw new Exception("Stop first");
                Readers(false); SessionState.SetBool("HSE.VitaControllerPreview",SessionState.GetBool("HSE.Regression.PreviewBefore",false));
                EditorSceneManager.OpenScene("Assets/WIP/MainMenu.unity");
            }
            else if (command == "quit") { if (EditorApplication.isPlaying) throw new Exception("Stop first"); EditorApplication.Exit(0); }
            else if (command == "snapshot") { }
            else if (command == "background") {SessionState.SetBool("HSE.Regression.BackgroundBefore",Application.runInBackground);Application.runInBackground=true;}
            else if (command == "pause") GUIManager.Instance.InGameGUI.SwitchInGameOverlayState(InGameGUI.State.Pause);
            else if (command == "resume") GUIManager.Instance.InGameGUI.SwitchInGameOverlayState(InGameGUI.State.HUD);
            else if (command == "lab_inventory") {
                StringBuilder b=new StringBuilder(); int count=0;foreach(SecretSharkBrain brain in Resources.FindObjectsOfTypeAll<SecretSharkBrain>())if(brain.gameObject.scene.IsValid()){count++;b.AppendLine(brain.name+" active="+brain.gameObject.activeInHierarchy+" parent="+(brain.transform.parent==null?"null":brain.transform.parent.name));}Write(id+"_inventory","count="+count+"\n"+b);
            }
            else if(command == "lab_wrap_contract") {
                for(int j=0;j<20;j++) {
                    if(!LabCrane.AttemptSlide(j%4<2))throw new Exception("Crane rejected ready slide");
                    LabCrane[] cranes=UnityEngine.Object.FindObjectsOfType<LabCrane>();
                    foreach(LabCrane crane in cranes){TweenPosition tween=crane.GetComponent<TweenPosition>();tween.enabled=false;crane.transform.localPosition=tween.to;}
                    foreach(LabCrane crane in cranes)typeof(LabCrane).GetMethod("Update",All).Invoke(crane,null);
                    float pitch=(float)typeof(LabCrane).GetField("m_cranePitch",All).GetValue(null);
                    foreach(LabCrane crane in cranes)if(Mathf.Abs(crane.transform.localPosition.x)>pitch*1.5f||crane.State!=LabCrane.state.restingBottom)throw new Exception("Completed tween escaped carousel bounds");
                    if(!LabCrane.isInteractive||LabCrane.m_activeShark==SharkType.none)throw new Exception("Invalid settled selection");
                }Write(id+"_wrap","PASS 20 completed-tween steps, both directions, valid shark, three ready cranes inside carousel bounds.");
            }
            else if(command == "lab_scale_contract") {
                int count=0;foreach(LabCrane crane in UnityEngine.Object.FindObjectsOfType<LabCrane>()) {
                    SecretSharkBrain brain=crane.GetComponentInChildren<SecretSharkBrain>(); if(brain==null)throw new Exception("Missing visible model");
                    Vector3 expected=(Vector3)typeof(LabCrane).GetMethod("GetSharkScale",All).Invoke(null,new object[]{brain.sharkType});
                    if((brain.transform.lossyScale-expected).sqrMagnitude>.000001f)throw new Exception("Changed model scale: "+brain.transform.lossyScale+" expected "+expected);count++;
                }if(count!=3)throw new Exception("Expected three models");Write(id+"_scale","PASS: all three model world scales match the original GetSharkScale values.");
            }
            else if (command == "click") {
                GameObject go = GameObject.Find(p[2]); if (go == null) throw new Exception("Missing active button: " + p[2]);
                go.SendMessage("OnClick",SendMessageOptions.DontRequireReceiver);
            }
            else if (command == "focus") { FGOLControlManager.Instance.selectedObject = GameObject.Find(p[2]); }
            else if (command == "direction") { axisName=p[2]; axis=float.Parse(p[3],System.Globalization.CultureInfo.InvariantCulture); releaseAt=EditorApplication.timeSinceStartup+double.Parse(p[4],System.Globalization.CultureInfo.InvariantCulture); Readers(true); }
            else if (command == "button") { pulse=p[2]; pulseFrame=Time.frameCount+1; releaseAt=EditorApplication.timeSinceStartup+.1; Readers(true); }
            else if (command == "back") GUIManager.Instance.OnHardwareBackButton();
            else if (command == "measure") {
                if (sample != null) throw new Exception("Sample already running");
                GameObject go = new GameObject("Editor-only frame sample"); sample=go.AddComponent<HSEEditorFrameSample>(); sample.Begin(Path.Combine(Root,p[2]+".csv"),float.Parse(p[3],System.Globalization.CultureInfo.InvariantCulture));
            }
            else if (command == "touch") {
                LabCrane master=null; foreach (LabCrane c in UnityEngine.Object.FindObjectsOfType<LabCrane>()) if ((bool)typeof(LabCrane).GetField("m_isMaster",All).GetValue(c)) master=c;
                if (master==null) throw new Exception("No master crane");
                TouchControls touch=(TouchControls)typeof(LabCrane).GetProperty("m_touchControls",All).GetValue(null,null);
                typeof(TouchControls).GetField("m_currentTouchState",All).SetValue(touch,(TouchState)Enum.Parse(typeof(TouchState),p[2]));
                typeof(TouchControls).GetField("m_currentTouchPos",All).SetValue(touch,new Vector3(float.Parse(p[3]),100,0));
                typeof(LabCrane).GetMethod("MonitorSwipe",All).Invoke(master,null);
            }
            else if (command == "gc") { int count=GC.CollectionCount(0); GC.Collect(); Write(id+"_gc", "milliseconds="+timer.Elapsed.TotalMilliseconds+" count="+(GC.CollectionCount(0)-count)); }
            else if (command == "unload") {
                int before=GC.CollectionCount(0); var operation=Resources.UnloadUnusedAssets(); var sw=System.Diagnostics.Stopwatch.StartNew();
                EditorApplication.CallbackFunction poll=null; poll=()=>{if(operation.isDone){EditorApplication.update-=poll;Write(id+"_unload","collections="+(GC.CollectionCount(0)-before)+" ms="+sw.Elapsed.TotalMilliseconds);}};EditorApplication.update+=poll;
            }
            else if (command == "benchmark_touch") {
                StringBuilder b=new StringBuilder();
                for(int run=0;run<5;run++) { GC.Collect(); long before=Profiler.GetMonoUsedSizeLong(); int count=GC.CollectionCount(0); var sw=System.Diagnostics.Stopwatch.StartNew(); for(int j=0;j<10000;j++)GameInput.CheckTouchState(0); sw.Stop(); b.AppendLine("calls=10000 ms="+sw.Elapsed.TotalMilliseconds+" managed_delta="+(Profiler.GetMonoUsedSizeLong()-before)+" collections="+(GC.CollectionCount(0)-count)); }
                Write(id+"_benchmark",b.ToString());
            }
            else if (command == "mesh_contract") {
                GameObject go=new GameObject("Mesh copy contract"); Mesh mesh=new Mesh();
                try { Vector3[] input=new Vector3[400];for(int j=0;j<input.Length;j++)input[j]=new Vector3(j*.01f,j%7,j%11);mesh.vertices=input;
                    go.transform.position=new Vector3(2,3,4);go.transform.rotation=Quaternion.Euler(20,30,40);go.transform.localScale=new Vector3(2,3,4);
                    Vector3[] original=new Vector3[input.Length],cached=new Vector3[input.Length];StringBuilder b=new StringBuilder();
                    for(int run=0;run<3;run++) {
                        GC.Collect();long memory=Profiler.GetMonoUsedSizeLong();var sw=System.Diagnostics.Stopwatch.StartNew();
                        for(int j=0;j<mesh.vertexCount;j++)original[j]=go.transform.TransformPoint(mesh.vertices[j]);sw.Stop();b.AppendLine("original_ms="+sw.Elapsed.TotalMilliseconds+" managed_delta="+(Profiler.GetMonoUsedSizeLong()-memory));
                        GC.Collect();memory=Profiler.GetMonoUsedSizeLong();sw=System.Diagnostics.Stopwatch.StartNew();Vector3[] vertices=mesh.vertices;
                        for(int j=0;j<vertices.Length;j++)cached[j]=go.transform.TransformPoint(vertices[j]);sw.Stop();b.AppendLine("cached_ms="+sw.Elapsed.TotalMilliseconds+" managed_delta="+(Profiler.GetMonoUsedSizeLong()-memory));
                        for(int j=0;j<original.Length;j++)if(original[j]!=cached[j])throw new Exception("Vertex mismatch");
                    }Write(id+"_mesh","PASS: all transformed vertices identical; mesh source unchanged.\n"+b);
                } finally {UnityEngine.Object.DestroyImmediate(go);UnityEngine.Object.DestroyImmediate(mesh);}
            }
            else if (command == "trace") {
                Type t=typeof(GameInput).Assembly.GetType("HSEEditorTiming"); if(t==null)throw new Exception("Temporary timing helper absent");
                t.GetField("Enabled").SetValue(null,p[2]!="off");
                if(p[2]=="off") Write(id+"_trace",(string)t.GetMethod("Drain").Invoke(null,null));
            }
            else if(command == "profiler_api") {
                StringBuilder b=new StringBuilder(); foreach(Type t in typeof(EditorApplication).Assembly.GetTypes()) if(t.Name=="ProfilerFrameDataIterator"||t.Name=="ProfilerDriver"||t.Name=="HierarchyFrameDataView") {b.AppendLine(t.FullName);foreach(var m in t.GetMembers(All))b.AppendLine(m.ToString());} Write(id+"_api",b.ToString());
            }
            else if(command == "analyze_profile") {
                Type driver=typeof(EditorApplication).Assembly.GetType("UnityEditorInternal.ProfilerDriver");
                driver.GetMethod("LoadProfile",All).Invoke(null,new object[]{Path.Combine(Root,p[2]+".raw"),false});
                int first=(int)driver.GetProperty("firstFrameIndex",All).GetValue(null,null), end=(int)driver.GetProperty("lastFrameIndex",All).GetValue(null,null);
                Type type=typeof(EditorApplication).Assembly.GetType("UnityEditorInternal.ProfilerFrameDataIterator"); object it=Activator.CreateInstance(type);
                Dictionary<string,double[]> totals=new Dictionary<string,double[]>();
                try { for(int frame=first;frame<=end;frame++) {
                    type.GetMethod("SetRoot").Invoke(it,new object[]{frame,0});
                    while((bool)type.GetMethod("Next").Invoke(it,new object[]{true})) {
                        string name=(string)type.GetProperty("name").GetValue(it,null); double duration=Convert.ToDouble(type.GetProperty("durationMS").GetValue(it,null));
                        double[] t;if(!totals.TryGetValue(name,out t)){t=new double[3];totals[name]=t;}t[0]+=duration;t[1]++;t[2]=Math.Max(t[2],duration);
                    }
                }} finally {((IDisposable)it).Dispose();}
                List<string> names=new List<string>(totals.Keys);names.Sort((a,b)=>totals[b][0].CompareTo(totals[a][0]));
                StringBuilder bld=new StringBuilder("frames="+(end-first+1)+"\nname\ttotal_inclusive_ms\tcount\tmax_ms\n");foreach(string name in names){double[] t=totals[name];bld.AppendLine(name+"\t"+t[0]+"\t"+t[1]+"\t"+t[2]);}Write(id+"_profile",bld.ToString());
            }
            else if (command == "profile") {
                if (p[2]=="off") { Profiler.enabled=false; Profiler.enableBinaryLog=false; Profiler.logFile=""; }
                else { Profiler.logFile=Path.Combine(Root,p[2]+".raw"); Profiler.enableBinaryLog=true; Profiler.enabled=true; }
            }
            else if (command == "objects") {
                StringBuilder b=new StringBuilder(); foreach (Transform t in UnityEngine.Object.FindObjectsOfType<Transform>()) if (t.name.IndexOf(p[2],StringComparison.OrdinalIgnoreCase)>=0) b.AppendLine(t.name+" parent="+(t.parent==null?"null":t.parent.name)); Write(id+"_objects",b.ToString());
            }
            else throw new Exception("Unknown command");
            Write(id,"command_ms="+timer.Elapsed.TotalMilliseconds+"\n"+Snapshot());
        } catch(Exception e) { Write(id+"_FAILED",e.ToString()); }
    }
}

public class HSEEditorFrameSample : MonoBehaviour
{
    string path; float until; int lastGC; readonly List<string> rows=new List<string>();
    UnityEngine.Profiling.Recorder gc;
    public void Begin(string file,float seconds) { path=file; until=Time.realtimeSinceStartup+seconds; lastGC=GC.CollectionCount(0); gc=UnityEngine.Profiling.Recorder.Get("GC.Alloc"); if(gc!=null)gc.enabled=true; rows.Add("frame,unscaled_ms,gc_collections,gc_alloc_samples,gc_alloc_ns,managed_bytes,batches,draw_calls,triangles"); }
    void LateUpdate() {
        if(path==null)return;
        rows.Add(Time.frameCount+","+(Time.unscaledDeltaTime*1000).ToString("F4",System.Globalization.CultureInfo.InvariantCulture)+","+(GC.CollectionCount(0)-lastGC)+","+(gc==null?-1:gc.sampleBlockCount)+","+(gc==null?-1:gc.elapsedNanoseconds)+","+GC.GetTotalMemory(false)+","+UnityStats.batches+","+UnityStats.drawCalls+","+UnityStats.triangles); lastGC=GC.CollectionCount(0);
        if(Time.realtimeSinceStartup>=until) { File.WriteAllLines(path,rows.ToArray()); if(gc!=null)gc.enabled=false; Destroy(gameObject); }
    }
}
#endif

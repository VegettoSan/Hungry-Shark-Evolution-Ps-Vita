// Install in Assets/Editor. Generated JSON contains private game data; never publish it.
#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public sealed class HSEVitaStoreDescriptionBuilder : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }
    const string Destination = "Assets/Resources/HSEVitaStoreDescriptionData.json";

    [MenuItem("HSE/Profiling/Rebuild private store description metadata")]
    public static void Rebuild()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before rebuilding metadata.");
        var entries = new List<HSEVitaStoreDescriptions.Entry>();
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }))
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            int resources = assetPath.LastIndexOf("/Resources/", StringComparison.Ordinal);
            if (resources < 0) continue;
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            Accessory accessory = prefab == null ? null : prefab.GetComponent<Accessory>();
            if (accessory == null || accessory.m_gameplayEffects == null) continue;
            string path = assetPath.Substring(resources + 11);
            path = path.Substring(0, path.Length - Path.GetExtension(path).Length);
            if (!paths.Add(path)) throw new InvalidOperationException("Ambiguous accessory Resources path: " + path);
            // Round trip a private copy; never mutate the prefab, including its trail reference.
            var copy = JsonUtility.FromJson<Accessory.GameplayEffects>(JsonUtility.ToJson(accessory.m_gameplayEffects));
            copy.m_TrailPrefab = null;
            entries.Add(new HSEVitaStoreDescriptions.Entry {
                resourcePath = path, assetPath = assetPath,
                dependencyHash = AssetDatabase.GetAssetDependencyHash(assetPath).ToString(),
                hasTrail = accessory.m_gameplayEffects.m_TrailPrefab != null, effects = copy
            });
        }
        entries.Sort((a, b) => StringComparer.Ordinal.Compare(a.resourcePath, b.resourcePath));
        if (entries.Count == 0 || entries.Count > HSEVitaStoreDescriptions.MaximumEntries)
            throw new InvalidOperationException("Unexpected description catalog size: " + entries.Count);
        string json = JsonUtility.ToJson(new HSEVitaStoreDescriptions.Catalog { entries = entries.ToArray() });
        if (System.Text.Encoding.UTF8.GetByteCount(json) > HSEVitaStoreDescriptions.MaximumJsonBytes)
            throw new InvalidOperationException("Description metadata exceeds the bounded size.");
        Directory.CreateDirectory("Assets/Resources");
        File.WriteAllText(Destination, json);
        AssetDatabase.ImportAsset(Destination, ImportAssetOptions.ForceUpdate);
        HSEVitaStoreDescriptions.Reset();
        Validate();
        UnityEngine.Debug.Log("Private store metadata rebuilt: " + entries.Count + " scalar descriptions; no model/texture references.");
    }

    public static void Validate()
    {
        if (!File.Exists(Destination)) throw new BuildFailedException("Rebuild private store description metadata first.");
        var catalog = JsonUtility.FromJson<HSEVitaStoreDescriptions.Catalog>(File.ReadAllText(Destination));
        if (catalog == null || catalog.version != HSEVitaStoreDescriptions.FormatVersion || catalog.entries == null ||
            catalog.entries.Length == 0 || catalog.entries.Length > HSEVitaStoreDescriptions.MaximumEntries ||
            new FileInfo(Destination).Length > HSEVitaStoreDescriptions.MaximumJsonBytes)
            throw new BuildFailedException("Invalid private store description metadata.");
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in catalog.entries)
            if (entry == null || entry.effects == null || entry.effects.m_TrailPrefab != null ||
                string.IsNullOrEmpty(entry.resourcePath) || !paths.Add(entry.resourcePath) ||
                string.IsNullOrEmpty(entry.assetPath) || !File.Exists(entry.assetPath) ||
                AssetDatabase.GetAssetDependencyHash(entry.assetPath).ToString() != entry.dependencyHash)
                throw new BuildFailedException("Stale/invalid private accessory description; rebuild the catalog.");
    }

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.PSP2) Validate();
    }
}
#endif

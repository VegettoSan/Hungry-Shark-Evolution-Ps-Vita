// UI-only metadata. Never use these copies for equipping or gameplay effects.
#if UNITY_EDITOR || UNITY_PSP2
using System;
using System.Collections.Generic;
using UnityEngine;

public static class HSEVitaStoreDescriptions
{
    public const string ResourceName = "HSEVitaStoreDescriptionData";
    public const int FormatVersion = 1;
    public const int MaximumEntries = 512;
    public const int MaximumJsonBytes = 512 * 1024;

    [Serializable]
    public sealed class Entry
    {
        public string resourcePath;
        public string assetPath;
        public string dependencyHash;
        public bool hasTrail;
        public Accessory.GameplayEffects effects;
    }

    [Serializable]
    public sealed class Catalog
    {
        public int version = FormatVersion;
        public Entry[] entries;
    }

    static Dictionary<string, Entry> descriptions;
    static bool attempted, warned;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Reset()
    {
        descriptions = null;
        attempted = warned = false;
    }

    static void Load()
    {
        if (attempted) return;
        attempted = true;
        TextAsset data = Resources.Load<TextAsset>(ResourceName);
        if (data == null) return;
        try
        {
            if (data.bytes.Length > MaximumJsonBytes) return;
            Catalog catalog = JsonUtility.FromJson<Catalog>(data.text);
            if (catalog == null || catalog.version != FormatVersion || catalog.entries == null ||
                catalog.entries.Length > MaximumEntries) return;
            // Resources paths are case insensitive; exported folders and item paths differ in case.
            var result = new Dictionary<string, Entry>(catalog.entries.Length, StringComparer.OrdinalIgnoreCase);
            foreach (Entry entry in catalog.entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.resourcePath) || entry.effects == null ||
                    entry.effects.m_TrailPrefab != null || result.ContainsKey(entry.resourcePath)) return;
                result.Add(entry.resourcePath, entry);
            }
            descriptions = result;
        }
        catch (ArgumentException) { descriptions = null; }
        finally { Resources.UnloadAsset(data); }
    }

    public static Accessory.GameplayEffects GetEffects(ShopManager.ShopItem item)
    {
        if (!(item.IsEquipable || item.IsSwitchable || item.itemtype == ItemType.Antidote ||
              item.itemtype == ItemType.FlakJacket)) return null;
        Load();
        Entry entry;
        if (descriptions != null && descriptions.TryGetValue(item.PrefabResourcePath, out entry))
        {
            // Keep the same override timing and localization methods as the original UI.
            entry.effects.LoadRemoteConfiguration(RemoteConfigurationFacade.Instance, "SM." + item.id + ".");
            return entry.effects;
        }
        if (!warned)
        {
            warned = true;
            UnityEngine.Debug.LogWarning("Store description metadata unavailable: using original prefab lookup. Rebuild the description catalog before a PSP2 build.");
        }
        return ShopManager.GetGameplayEffectsForShopItem(item);
    }

    public static bool IsAllZero(ShopManager.ShopItem item, Accessory.GameplayEffects effects)
    {
        Entry entry;
        // The original predicate also checks the trail object. Retain that boolean,
        // without loading/retaining a trail prefab merely to decide UI visibility.
        return effects.IsAllZero() && !(descriptions != null &&
            descriptions.TryGetValue(item.PrefabResourcePath, out entry) &&
            object.ReferenceEquals(entry.effects, effects) && entry.hasTrail);
    }
}
#endif

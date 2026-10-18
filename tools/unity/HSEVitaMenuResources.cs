using UnityEngine;

// Keep at most one source prefab, never a live menu instance. Call preparation only
// during the existing front-end/game loading sequence, before the screen is interactive.
public static class HSEVitaMenuResources
{
    private static GUIManager owner;
    private static GameObject prepared;
    private static string preparedPath;

    public static void PrepareEvolution(GUIManager source)
    {
        Prepare(source, "GUIPrefabs/UpgradeMenu");
    }

    public static void PreparePause(GUIManager source)
    {
        // Vita retains the original mobile OptionsMenu path in OptionMenuSheet.
        Prepare(source, "GUIPrefabs/OptionsMenu");
    }

    private static void Prepare(GUIManager source, string path)
    {
        if (!HSEVitaInput.Active) return;
        if (owner == source && preparedPath == path && prepared != null) return;
        Release();
        owner = source;
        preparedPath = path;
        prepared = Resources.Load<GameObject>(path);
    }

    public static GameObject Load(string path)
    {
        if (HSEVitaInput.Active && owner != null && prepared != null && path == preparedPath) return prepared;
        return Resources.Load(path) as GameObject;
    }

    public static void Release()
    {
        prepared = null;
        preparedPath = null;
        owner = null;
    }
}

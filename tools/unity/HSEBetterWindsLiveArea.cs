// Unity 2018.2 editor only. Install in Assets/Editor of the private PSP2 project.
using System;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

public static class HSEBetterWindsLiveArea
{
    public const string Root = "PSP2System/BetterWinds";
    const string Icon = "Assets/Editor/BetterWinds/icon0.png";

    static void Check(string path, string expected)
    {
        using (var sha = SHA256.Create())
            if (BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant() != expected)
                throw new InvalidOperationException("BetterWinds source hash mismatch: " + path);
    }

    [MenuItem("HSE/PSP2/Configure BetterWinds LiveArea")]
    public static void Configure()
    {
        if (Application.unityVersion != "2018.2.19f1") throw new InvalidOperationException("Use the pinned editor");
        Check(Root + "/contents/bg0.png", "452a71826ec9613c29773a4f29bebdc88c3dfe9299313048939d6a2b1152f77c");
        Check(Root + "/contents/startup.png", "293fff5b6e8abc7923ed2d9f3b1bb92b233843c7d6416fa5b657b8b2730a4f4d");
        Check(Icon, "81d4755193278c3cde2d821398d300d01c3a6b9d5e3df868db9f9fb70c83c1c5");
        AssetDatabase.ImportAsset(Icon, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(Icon);
        importer.textureType = TextureImporterType.Default;
        importer.mipmapEnabled = false;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(Icon);
        if (texture == null || texture.width != 128 || texture.height != 128) throw new InvalidOperationException("Invalid icon");
        int[] sizes = PlayerSettings.GetIconSizesForTargetGroup(BuildTargetGroup.PSP2);
        if (sizes.Length != 1 || sizes[0] != 128) throw new InvalidOperationException("Unexpected native PSP2 icon slots");
        PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.PSP2, new[] { texture });
        PlayerSettings.PSVita.liveAreaPath = Root + "/contents";
        PlayerSettings.PSVita.liveAreaBackroundPath = "";
        PlayerSettings.PSVita.liveAreaGatePath = "";
        AssetDatabase.SaveAssets();
        Verify();
    }

    public static void Verify()
    {
        if (PlayerSettings.PSVita.liveAreaPath != Root + "/contents" ||
            PlayerSettings.PSVita.liveAreaBackroundPath != "" || PlayerSettings.PSVita.liveAreaGatePath != "")
            throw new InvalidOperationException("Competing or missing LiveArea settings");
        var icons = PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.PSP2);
        if (icons.Length != 1 || AssetDatabase.GetAssetPath(icons[0]) != Icon)
            throw new InvalidOperationException("PSP2 icon setting missing");
    }
}

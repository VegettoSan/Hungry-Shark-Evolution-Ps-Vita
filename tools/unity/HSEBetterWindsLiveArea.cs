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
    const string Pic = "Assets/Editor/BetterWinds/pic0.png";

    [MenuItem("HSE/PSP2/Configure BetterWinds LiveArea v2")]
    public static void ConfigureV2()
    {
        Check(Root + "/contents/bg0.png", "c5b2704c3fc91bb7d35cf46ec41941ed1c9d02f2d8a0dd90926b40aabb612e07");
        // Installed PSP2 Publishing Tools 2.31 rejects psmobile; reviewed a1/CRLF derivative.
        Check(Root + "/contents/template.xml", "24a187fa56fb63e0ba68530073e8df84544308ef7c9756687cbd31e36c1edb51");
        Check(Root + "/manual/001.png", "e8a05c546a44d6b2dd598a04f3838d879a45ac5fe6b39ecc2c33c6e3b07e7e6f");
        Check(Pic, "403dc5de774542a200bbc744133cff29e5151faf9679ac77d956824bd4edc732");
        ConfigureImages(true);
        PlayerSettings.PSVita.manualPath = Root + "/manual";
        // Same serialized native field used by the installed PSP2 publishing UI.
        var settings = new SerializedObject(Resources.FindObjectsOfTypeAll<PlayerSettings>()[0]);
        var splash = settings.FindProperty("psp2Splashimage");
        if (splash == null) throw new InvalidOperationException("Native PSP2 splash setting unavailable");
        splash.objectReferenceValue = AssetDatabase.LoadAssetAtPath<Texture2D>(Pic);
        settings.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssets();
        VerifyV2();
    }

    public static void VerifyV2()
    {
        Verify();
        if (PlayerSettings.PSVita.manualPath != Root + "/manual") throw new InvalidOperationException("Manual path missing");
        var settings = new SerializedObject(Resources.FindObjectsOfTypeAll<PlayerSettings>()[0]);
        if (AssetDatabase.GetAssetPath(settings.FindProperty("psp2Splashimage").objectReferenceValue) != Pic)
            throw new InvalidOperationException("Native PIC0 setting missing");
    }

    static void Check(string path, string expected)
    {
        using (var sha = SHA256.Create())
            if (BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant() != expected)
                throw new InvalidOperationException("BetterWinds source hash mismatch: " + path);
    }

    [MenuItem("HSE/PSP2/Configure BetterWinds LiveArea")]
    public static void Configure()
    {
        Check(Root + "/contents/bg0.png", "452a71826ec9613c29773a4f29bebdc88c3dfe9299313048939d6a2b1152f77c");
        ConfigureImages(false);
    }

    static void ConfigureImages(bool v2)
    {
        if (Application.unityVersion != "2018.2.19f1") throw new InvalidOperationException("Use the pinned editor");
        Check(Root + "/contents/startup.png", "293fff5b6e8abc7923ed2d9f3b1bb92b233843c7d6416fa5b657b8b2730a4f4d");
        Check(Icon, "81d4755193278c3cde2d821398d300d01c3a6b9d5e3df868db9f9fb70c83c1c5");
        if (v2)
        {
            AssetDatabase.ImportAsset(Pic, ImportAssetOptions.ForceSynchronousImport);
            var picImporter = (TextureImporter)AssetImporter.GetAtPath(Pic);
            picImporter.textureType = TextureImporterType.Default;
            picImporter.mipmapEnabled = false;
            picImporter.npotScale = TextureImporterNPOTScale.None;
            picImporter.maxTextureSize = 1024;
            picImporter.textureCompression = TextureImporterCompression.Uncompressed;
            picImporter.SaveAndReimport();
            var pic = AssetDatabase.LoadAssetAtPath<Texture2D>(Pic);
            if (pic == null || pic.width != 960 || pic.height != 544) throw new InvalidOperationException("Invalid PIC0");
        }
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

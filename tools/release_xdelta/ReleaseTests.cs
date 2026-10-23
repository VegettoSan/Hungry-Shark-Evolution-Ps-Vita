using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web.Script.Serialization;

public static class ReleaseTests
{
    static int passed;
    static void Pass(string name) { passed++; Console.WriteLine("PASS " + name); }
    static void Reject(string name, Action action)
    {
        bool failed = false;
        try { action(); } catch (Exception) { failed = true; }
        if (!failed) throw new Exception("Accepted invalid case: " + name);
        Pass(name);
    }
    static void Zip(string path, Dictionary<string, string> files)
    {
        using (var z = ZipFile.Open(path, ZipArchiveMode.Create))
        foreach (var r in files) using (var w = new StreamWriter(z.CreateEntry(r.Key).Open())) w.Write(r.Value);
    }
    public static int Main(string[] args)
    {
        string work = Path.GetFullPath(args[0]); Directory.CreateDirectory(work);
        foreach (var name in new[] { "../x", "a/../b", "a/./b", "a//b", "a\\b", "/x", "C:/x", "NUL.txt", "a. /b", "a./b", "a\u0001b", "a*/b" })
            Reject("path " + name, () => HSERebuild.SafeName(name));
        string conflict = Path.Combine(work, "conflict.zip");
        Zip(conflict, new Dictionary<string, string> { { "a", "1" }, { "a/b", "2" } });
        Reject("file prefix collision", () => { using (var z = ZipFile.OpenRead(conflict)) HSERebuild.Entries(z); });
        string collision = Path.Combine(work, "case.zip");
        Zip(collision, new Dictionary<string, string> { { "A", "1" }, { "a", "2" } });
        Reject("case collision", () => { using (var z = ZipFile.OpenRead(collision)) HSERebuild.Entries(z); });
        string source = Path.Combine(work, "split.apk"), normalized = Path.Combine(work, "source.bin");
        Zip(source, new Dictionary<string, string> { { "assets/bin/Data/b.split1", "2" }, { "assets/bin/Data/b.split0", "1" },
            { "assets/bin/Data/A", "A" }, { "assets/bin/Data/a", "a" } });
        // Exact source rejects case collisions even though the canonical logical order is ordinal.
        Reject("normalized source unsafe case collision", () => HSERebuild.Normalize(source, normalized));
        source = Path.Combine(work, "split-safe.apk"); normalized = Path.Combine(work, "safe.bin");
        Zip(source, new Dictionary<string, string> { { "assets/bin/Data/b.split1", "2" }, { "assets/bin/Data/b.split0", "1" }, { "assets/bin/Data/a", "a" } });
        HSERebuild.Normalize(source, normalized);
        if (File.ReadAllText(normalized) != "a12") throw new Exception("split concatenation mismatch"); Pass("numeric split normalization");
        source = Path.Combine(work, "gap.apk"); normalized = Path.Combine(work, "gap.bin");
        Zip(source, new Dictionary<string, string> { { "assets/bin/Data/b.split0", "1" }, { "assets/bin/Data/b.split2", "3" } });
        Reject("split gap", () => HSERebuild.Normalize(source, normalized));
        string a = Path.Combine(work, "a.vpk"), b = Path.Combine(work, "b.vpk");
        Zip(a, new Dictionary<string, string> { { "file", "a" } }); Zip(b, new Dictionary<string, string> { { "file", "b" } });
        Reject("byte difference", () => HSERebuild.Compare(a, b));
        if (HSERebuild.Compare(a, a) != 1) throw new Exception(); Pass("equal bytes");
        if (args.Length >= 3)
        {
            string apk = args[1], bundle = args[2]; HSERebuild.Progress = text => { };
            string output = Path.Combine(work, "protected.vpk"); File.WriteAllText(output, "DO NOT OVERWRITE");
            Reject("overwrite rejected", () => HSERebuild.Rebuild(apk, bundle, output));
            if (File.ReadAllText(output) != "DO NOT OVERWRITE") throw new Exception();
            Reject("wrong APK", () => HSERebuild.Rebuild(a, bundle, Path.Combine(work, "bad.vpk")));
            string wrongHash = Path.Combine(work, "same-size.apk");
            using (var f = new FileStream(wrongHash, FileMode.CreateNew)) f.SetLength(104296014);
            Reject("wrong APK SHA at correct size", () => HSERebuild.Rebuild(wrongHash, bundle, Path.Combine(work, "hash.vpk")));
            var free = HSERebuild.Available; HSERebuild.Available = path => 0;
            Reject("low free space", () => HSERebuild.Rebuild(apk, bundle, Path.Combine(work, "low.vpk")));
            HSERebuild.Available = free;
            Reject("missing manifest", () => HSERebuild.Rebuild(apk, work, Path.Combine(work, "missing.vpk")));
            string broken = Path.Combine(work, "broken"); Directory.CreateDirectory(broken);
            foreach (var name in new[] { "HSEVita_Release.json", "HSEVita_Stub.vpk", "HSEVita_Data.xdelta", "xdelta3decode.exe" })
                File.Copy(Path.Combine(bundle, name), Path.Combine(broken, name));
            string decoder = Path.Combine(broken, "xdelta3decode.exe"); File.WriteAllText(decoder, "NOT AN EXECUTABLE");
            Reject("unverified decoder never executed", () => HSERebuild.Rebuild(apk, broken, Path.Combine(work, "decoder.vpk")));
            File.Copy(Path.Combine(bundle, "xdelta3decode.exe"), decoder, true);
            string patchFile = Path.Combine(broken, "HSEVita_Data.xdelta");
            File.Move(patchFile, patchFile + ".hold");
            Reject("missing patch", () => HSERebuild.Rebuild(apk, broken, Path.Combine(work, "missing-patch.vpk")));
            File.Move(patchFile + ".hold", patchFile);
            File.WriteAllText(Path.Combine(broken, "HSEVita_Data.xdelta"), "BROKEN");
            Reject("corrupt patch", () => HSERebuild.Rebuild(apk, broken, Path.Combine(work, "patch.vpk")));
            File.Copy(Path.Combine(bundle, "HSEVita_Data.xdelta"), Path.Combine(broken, "HSEVita_Data.xdelta"), true);
            string manifestPath = Path.Combine(broken, "HSEVita_Release.json");
            var json = new JavaScriptSerializer { MaxJsonLength = 8 * 1024 * 1024 };
            string originalManifest = File.ReadAllText(manifestPath);
            var m = json.Deserialize<Dictionary<string, object>>(originalManifest);
            m["title_id"] = "INVALID"; File.WriteAllText(manifestPath, json.Serialize(m));
            Reject("wrong title manifest", () => HSERebuild.Rebuild(apk, broken, Path.Combine(work, "title.vpk")));
            m["title_id"] = "HGSKE1178";
            var receipts = ((System.Collections.IEnumerable)m["files"]).Cast<Dictionary<string, object>>().ToArray();
            string oldName = (string)receipts[0]["name"]; receipts[0]["name"] = "../escape";
            File.WriteAllText(manifestPath, json.Serialize(m));
            Reject("manifest path traversal", () => HSERebuild.Rebuild(apk, broken, Path.Combine(work, "traversal.vpk")));
            receipts[0]["name"] = oldName;
            m["data_sha256"] = new string('0', 64); File.WriteAllText(manifestPath, json.Serialize(m));
            Reject("decoded checksum failure and cleanup", () => HSERebuild.Rebuild(apk, broken, Path.Combine(work, "decoded.vpk")));
            File.WriteAllText(manifestPath, originalManifest);
            HSERebuild.Progress = text => { if (text.StartsWith("[4/7]")) HSERebuild.Cancelled = true; };
            Reject("cancel during decoder and cleanup", () => HSERebuild.Rebuild(apk, bundle, Path.Combine(work, "cancel.vpk")));
            HSERebuild.Cancelled = false;
            if (Directory.GetDirectories(work, ".hse-rebuild-*").Length != 0) throw new Exception("Temporary files leaked");
            foreach (var name in new[] { "bad", "low", "missing", "decoder", "patch", "decoded", "cancel" })
                if (File.Exists(Path.Combine(work, name + ".vpk"))) throw new Exception("Partial output leaked");
            Pass("no leaked temporaries or partial outputs");
        }
        Console.WriteLine("PASS " + passed + " native checks"); return 0;
    }
}

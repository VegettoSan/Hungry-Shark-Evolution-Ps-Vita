// Standalone Windows .NET Framework 4.8 console app with native file picker.
// No Python, Unity, SDK, game code or assets are embedded.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;

public static class HSERebuild
{
    const string ApkHash = "52b7d8754ab8fec75c3b656a0dc5ec8dc849565adc402487a3ac093f6a481f7a";
    const string DecoderHash = "67f84255bd009a65b1252784f8b3ebc72b5907766bc366c571e8b85fa7002a06";
    const long ApkBytes = 104296014;
    public static volatile bool Cancelled;
    public static Action<string> Progress = Console.WriteLine;
    public static Func<string, long> Available = p => new DriveInfo(Path.GetPathRoot(Path.GetFullPath(p))).AvailableFreeSpace;
    static readonly JavaScriptSerializer Json = new JavaScriptSerializer { MaxJsonLength = 8 * 1024 * 1024 };

    static void CheckCancel() { if (Cancelled) throw new OperationCanceledException(); }
    static Exception Error(string es, string en) { return new InvalidDataException(es + " / " + en); }
    public static string Hash(Stream input)
    {
        using (var h = SHA256.Create())
        {
            var buffer = new byte[1024 * 1024]; int n;
            while ((n = input.Read(buffer, 0, buffer.Length)) > 0)
            { CheckCancel(); h.TransformBlock(buffer, 0, n, null, 0); }
            h.TransformFinalBlock(new byte[0], 0, 0);
            return BitConverter.ToString(h.Hash).Replace("-", "").ToLowerInvariant();
        }
    }
    public static string HashFile(string path) { using (var f = File.OpenRead(path)) return Hash(f); }
    static string S(Dictionary<string, object> d, string k) { return (string)d[k]; }
    static long N(Dictionary<string, object> d, string k) { return Convert.ToInt64(d[k]); }
    static object[] A(Dictionary<string, object> d, string k) { return ((IEnumerable)d[k]).Cast<object>().ToArray(); }
    static void CheckHash(string path, string expected)
    {
        if (!Regex.IsMatch(expected, "^[0-9a-f]{64}$") || HashFile(path) != expected)
            throw Error("Checksum incorrecto: " + Path.GetFileName(path), "Checksum mismatch");
    }
    public static string SafeName(string name)
    {
        if (String.IsNullOrEmpty(name) || name.StartsWith("/") || name.EndsWith("/") ||
            name.Any(c => c < 32 || "\\:<>\"|?*".Contains(c)))
            throw Error("Ruta ZIP insegura", "Unsafe ZIP path");
        foreach (var part in name.Split('/'))
            if (part == "" || part == "." || part == ".." || part.EndsWith(".") || part.EndsWith(" ") ||
                Regex.IsMatch(part, @"^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(\..*)?$", RegexOptions.IgnoreCase))
                throw Error("Ruta ZIP insegura", "Unsafe ZIP path");
        return name;
    }
    static string Flat(string name)
    {
        SafeName(name);
        if (name.Contains('/')) throw Error("Nombre de componente invalido", "Invalid component filename");
        return name;
    }
    public static Dictionary<string, ZipArchiveEntry> Entries(ZipArchive archive)
    {
        var result = new Dictionary<string, ZipArchiveEntry>(StringComparer.Ordinal);
        var folded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in archive.Entries)
        {
            if (e.FullName.EndsWith("/")) { SafeName(e.FullName.TrimEnd('/')); continue; }
            var name = SafeName(e.FullName);
            int type = (e.ExternalAttributes >> 16) & 0xF000;
            if (type != 0 && type != 0x8000 || !folded.Add(name))
                throw Error("Entrada ZIP duplicada o especial", "Duplicate or special ZIP entry");
            result.Add(name, e);
        }
        foreach (var name in result.Keys)
        {
            int i = name.IndexOf('/');
            while (i >= 0)
            {
                if (folded.Contains(name.Substring(0, i))) throw Error("Colision de rutas ZIP", "ZIP prefix collision");
                i = name.IndexOf('/', i + 1);
            }
        }
        return result;
    }
    static void Copy(Stream input, Stream output)
    {
        var buffer = new byte[1024 * 1024]; int n;
        while ((n = input.Read(buffer, 0, buffer.Length)) > 0)
        { CheckCancel(); output.Write(buffer, 0, n); }
    }
    public static void Normalize(string apk, string output)
    {
        using (var z = ZipFile.OpenRead(apk))
        using (var stream = new FileStream(output, FileMode.CreateNew, FileAccess.Write))
        {
            var groups = new SortedDictionary<string, SortedDictionary<int, ZipArchiveEntry>>(StringComparer.Ordinal);
            foreach (var pair in Entries(z))
            {
                if (!pair.Key.StartsWith("assets/bin/Data/", StringComparison.Ordinal)) continue;
                var m = Regex.Match(pair.Key, @"^(.+)\.split(\d+)$");
                string key = m.Success ? m.Groups[1].Value : pair.Key;
                int index = m.Success ? Int32.Parse(m.Groups[2].Value) : -1;
                if (!groups.ContainsKey(key)) groups.Add(key, new SortedDictionary<int, ZipArchiveEntry>());
                groups[key].Add(index, pair.Value);
            }
            foreach (var group in groups.Values)
            {
                if (!(group.Count == 1 && group.ContainsKey(-1)) && !group.Keys.SequenceEqual(Enumerable.Range(0, group.Count)))
                    throw Error("Fragmentos APK incompletos", "Non-contiguous APK split group");
                foreach (var part in group.Values) using (var input = part.Open()) Copy(input, stream);
            }
        }
    }
    static string Quote(string text)
    {
        // CommandLineToArgvW quoting; no shell is involved.
        var b = new StringBuilder("\""); int slashes = 0;
        foreach (char c in text)
        {
            if (c == '\\') { slashes++; continue; }
            b.Append('\\', c == '"' ? slashes * 2 + 1 : slashes);
            b.Append(c); slashes = 0;
        }
        b.Append('\\', slashes * 2); b.Append('"'); return b.ToString();
    }
    static void Decode(string decoder, string source, string patch, string output)
    {
        var info = new ProcessStartInfo(decoder, "-d -D -R -B 536870912 -s " + Quote(source) + " " + Quote(patch) + " " + Quote(output))
        { UseShellExecute = false, CreateNoWindow = true, RedirectStandardError = true, RedirectStandardOutput = true };
        info.EnvironmentVariables.Remove("XDELTA");
        using (var p = new Process { StartInfo = info })
        {
            var errors = new StringBuilder();
            p.ErrorDataReceived += (sender, e) => { if (e.Data != null && errors.Length < 4096) errors.AppendLine(e.Data); };
            p.OutputDataReceived += (sender, e) => { };
            p.Start(); p.BeginErrorReadLine(); p.BeginOutputReadLine();
            try
            {
                while (!p.WaitForExit(200)) CheckCancel();
                p.WaitForExit(); CheckCancel();
                if (p.ExitCode != 0) throw Error("xdelta3 fallo (" + p.ExitCode + ")", "xdelta3 failed: " + errors);
            }
            finally { if (!p.HasExited) { p.Kill(); p.WaitForExit(); } }
        }
    }
    public static int Compare(string original, string rebuilt)
    {
        using (var a = ZipFile.OpenRead(original)) using (var b = ZipFile.OpenRead(rebuilt))
        {
            var aa = Entries(a); var bb = Entries(b);
            if (!new HashSet<string>(aa.Keys).SetEquals(bb.Keys)) throw Error("Lista diferente", "Entry sets differ");
            foreach (var name in aa.Keys)
            using (var x = aa[name].Open()) using (var y = bb[name].Open())
            {
                if (aa[name].Length != bb[name].Length) throw Error("Tamano diferente", "Member size differs");
                var left = new byte[1024 * 1024]; var right = new byte[left.Length];
                while (true)
                {
                    CheckCancel(); int n = x.Read(left, 0, left.Length), total = 0;
                    while (total < n) { int read = y.Read(right, total, n - total); if (read == 0) break; total += read; }
                    if (n != total) throw Error("Bytes diferentes", "Member bytes differ");
                    for (int i = 0; i < n; i++) if (left[i] != right[i]) throw Error("Bytes diferentes", "Member bytes differ");
                    if (n == 0) { if (y.ReadByte() != -1) throw new InvalidDataException(); break; }
                }
            }
            return aa.Count;
        }
    }
    public static void Rebuild(string apk, string bundle, string output)
    {
        apk = Path.GetFullPath(apk); bundle = Path.GetFullPath(bundle); output = Path.GetFullPath(output);
        if (File.Exists(output) || Directory.Exists(output)) throw Error("El destino ya existe", "Output already exists; will not overwrite");
        Progress("[1/7] Verificando APK / Checking APK...");
        if (new FileInfo(apk).Length != ApkBytes) throw Error("APK incorrecto: se requiere 6.4.2 (228)", "Wrong APK: exact 6.4.2 (228) required");
        CheckHash(apk, ApkHash);
        string jsonFile = Path.Combine(bundle, "HSEVita_Release.json");
        if (new FileInfo(jsonFile).Length > 8 * 1024 * 1024) throw new InvalidDataException("Manifest too large");
        var manifest = Json.Deserialize<Dictionary<string, object>>(File.ReadAllText(jsonFile));
        if (N(manifest, "format") != 2 || S(manifest, "title_id") != "HGSKE1178" || S(manifest, "apk_sha256") != ApkHash ||
            S(manifest, "xdelta_version") != "3.2.2" || S(manifest, "xdelta_decoder_sha256") != DecoderHash ||
            N(manifest, "apk_size") != ApkBytes || N(manifest, "source_window") != 536870912)
            throw Error("Manifiesto incompatible", "Incompatible release manifest");
        string mode = S(manifest, "source_mode");
        if (mode != "apk" && mode != "unity-data-v1") throw new InvalidDataException("Unsupported source mode");
        var records = A(manifest, "files").Cast<Dictionary<string, object>>().ToArray();
        var order = A(manifest, "entry_order").Cast<string>().ToArray();
        var payload = new HashSet<string>(A(manifest, "payload_paths").Cast<string>(), StringComparer.Ordinal);
        if (records.Length == 0 || records.Length > 10000 || order.Length != records.Length ||
            new HashSet<string>(order).Count != order.Length || records.Select(r => S(r, "name")).Distinct().Count() != records.Length ||
            !new HashSet<string>(order).SetEquals(records.Select(r => S(r, "name"))) ||
            !payload.IsSubsetOf(order) || payload.Contains("eboot.bin") || payload.Contains("sce_sys/param.sfo") ||
            !order.Contains("eboot.bin") || !order.Contains("sce_sys/param.sfo"))
            throw new InvalidDataException("Invalid entry manifest");
        long total = 0;
        foreach (var record in records)
        {
            SafeName(S(record, "name")); long size = N(record, "bytes");
            if (size < 0 || !Regex.IsMatch(S(record, "sha256"), "^[0-9a-f]{64}$")) throw new InvalidDataException("Invalid file receipt");
            total = checked(total + size);
        }
        if (total != N(manifest, "uncompressed_bytes") || total > 8L * 1024 * 1024 * 1024 || N(manifest, "source_bytes") < 0 ||
            N(manifest, "source_bytes") > 1024L * 1024 * 1024 || N(manifest, "data_bytes") < 0 || N(manifest, "data_bytes") > total * 2 + 1048576)
            throw new InvalidDataException("Invalid size limits");
        string stub = Path.Combine(bundle, Flat(S(manifest, "stub_filename")));
        string patch = Path.Combine(bundle, Flat(S(manifest, "patch_filename")));
        string decoder = Path.Combine(bundle, "xdelta3decode.exe");
        Progress("[2/7] Verificando componentes / Checking release components...");
        CheckHash(stub, S(manifest, "stub_sha256")); CheckHash(patch, S(manifest, "patch_sha256")); CheckHash(decoder, DecoderHash);
        string parent = Path.GetDirectoryName(output);
        Directory.CreateDirectory(parent);
        long required = checked(total * 3 + N(manifest, "source_bytes") + 128L * 1024 * 1024);
        if (Available(parent) < required) throw Error("Espacio insuficiente; necesita " + required + " bytes", "Insufficient free space");
        string work = Path.Combine(parent, ".hse-rebuild-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(work);
        try
        {
            // Marker explains any orphan directory after power loss/forced task termination.
            File.WriteAllText(Path.Combine(work, "HSE_TEMP.txt"), "HSE reconstruction temporary files. Safe to remove after the tool exits.\n");
            Progress("[3/7] Preparando datos del APK / Preparing source data...");
            string source = apk;
            if (mode == "unity-data-v1") { source = Path.Combine(work, "source.bin"); Normalize(apk, source); }
            if (new FileInfo(source).Length != N(manifest, "source_bytes")) throw new InvalidDataException("Source length mismatch");
            CheckHash(source, S(manifest, "source_sha256"));
            string data = Path.Combine(work, "payload.zip");
            Progress("[4/7] Reconstruyendo datos / Decoding data (Ctrl+C cancels)...");
            Decode(decoder, source, patch, data);
            if (new FileInfo(data).Length != N(manifest, "data_bytes")) throw new InvalidDataException("Payload length mismatch");
            CheckHash(data, S(manifest, "data_sha256"));
            string final = Path.Combine(work, "reconstructed.vpk");
            Progress("[5/7] Montando VPK / Assembling VPK...");
            using (var b = ZipFile.OpenRead(stub)) using (var d = ZipFile.OpenRead(data))
            using (var dest = new FileStream(final, FileMode.CreateNew))
            using (var z = new ZipArchive(dest, ZipArchiveMode.Create))
            {
                var bb = Entries(b); var dd = Entries(d);
                if (bb.Keys.Intersect(dd.Keys).Any() || !payload.SetEquals(dd.Keys) ||
                    !new HashSet<string>(bb.Keys.Concat(dd.Keys)).SetEquals(order)) throw new InvalidDataException("Component entry sets mismatch");
                foreach (var name in order)
                {
                    var input = payload.Contains(name) ? dd[name] : bb[name];
                    var receipt = records.First(r => S(r, "name") == name);
                    if (input.Length != N(receipt, "bytes")) throw new InvalidDataException("Member length mismatch");
                    var e = z.CreateEntry(name, CompressionLevel.Optimal);
                    e.LastWriteTime = new DateTimeOffset(1980, 1, 1, 0, 0, 0, TimeSpan.Zero);
                    e.ExternalAttributes = 0x1A4 << 16;
                    using (var reader = input.Open()) using (var writer = e.Open()) Copy(reader, writer);
                }
            }
            Progress("[6/7] Verificando todos los archivos / Verifying every file...");
            using (var z = ZipFile.OpenRead(final))
            {
                var entries = Entries(z);
                foreach (var r in records)
                using (var reader = entries[S(r, "name")].Open())
                    if (Hash(reader) != S(r, "sha256")) throw Error("Archivo interno danado", "Internal member checksum mismatch");
            }
            string checksum = HashFile(final); CheckCancel();
            File.Move(final, output); // Atomic same-volume publish; fails if destination appeared.
            Progress("[7/7] PASS: " + output + "\nSHA-256: " + checksum + "\nCredito / Artwork credit: BetterWinds");
        }
        finally
        {
            // Only this invocation's freshly created sibling directory is removed.
            if (Path.GetDirectoryName(work) != parent || !Path.GetFileName(work).StartsWith(".hse-rebuild-")) throw new InvalidOperationException();
            Directory.Delete(work, true);
        }
    }
    [STAThread]
    public static int Main(string[] args)
    {
        bool picker = args.Length == 0;
        bool interactive = args.Length <= 1;
        Console.CancelKeyPress += (sender, e) => { e.Cancel = true; Cancelled = true; };
        try
        {
            if (args.Length == 3 && args[0] == "--compare")
            { Console.WriteLine("PASS: byte-identical members: " + Compare(args[1], args[2])); return 0; }
            string apk, bundle = AppDomain.CurrentDomain.BaseDirectory, output = null;
            if (picker)
            {
                using (var d = new OpenFileDialog { Filter = "Hungry Shark Evolution 6.4.2 (*.apk)|*.apk", Title = "Selecciona tu APK / Select your APK" })
                { if (d.ShowDialog() != DialogResult.OK) return 130; apk = d.FileName; }
            }
            else
            {
                apk = args[0];
                for (int i = 1; i < args.Length; i += 2)
                {
                    if (i + 1 >= args.Length) throw new ArgumentException("Usage: HSERebuild.exe APK [--bundle DIR] [--out FILE]");
                    if (args[i] == "--bundle") bundle = args[i + 1];
                    else if (args[i] == "--out") output = args[i + 1]; else throw new ArgumentException("Unknown option");
                }
            }
            if (output == null) output = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(apk)), "HungrySharkEvolutionVita_1.0.vpk");
            Rebuild(apk, bundle, output);
            if (interactive) MessageBox.Show("PASS: VPK reconstruido / VPK rebuilt\n" + output, "Hungry Shark Evolution Vita");
            return 0;
        }
        catch (Exception e)
        {
            string message = e is OperationCanceledException ? "Cancelado / Cancelled" : "Error / Error: " + e.Message + "\nComprueba APK, componentes, espacio y permisos. / Check APK, bundle, free space and permissions.";
            Console.Error.WriteLine(message);
            if (interactive) MessageBox.Show(message, "Hungry Shark Evolution Vita");
            return e is OperationCanceledException ? 130 : 1;
        }
    }
}

"""Reduce repeated same-loop Renderer.material reads in an owner-supplied project.

Dry-run by default. Preserve the original non-PSP2 player implementation; add an
Editor-only A/B path. No original game source or data is distributed here.
"""
import argparse
import hashlib
import json
from pathlib import Path
import shutil


def prepare(data, trial=True):
    text = data.decode("utf-8-sig").replace("\r\n", "\n")
    if "HSE tint material local reuse" in text:
        raise ValueError("Already adapted; do not overwrite")
    signature = "private void UpdateTint()"
    if text.count(signature) != 1:
        raise ValueError("Expected exactly one UpdateTint method")
    start = text.index("{", text.index(signature)) + 1
    end = start
    depth = 1
    while depth:
        depth += (text[end] == "{") - (text[end] == "}")
        end += 1
    original = text[start:end - 1]
    if "#" in original or original.count(".material") != 11:
        raise ValueError("Unexpected source; inspect rather than guessing")
    optimized = original
    for suffix in ("", "2", "3", "4"):
        name = "renderer" + suffix
        declaration = "Renderer " + name + " = m_modelRenderers["
        position = optimized.index(declaration)
        finish = optimized.index(";", position) + 1
        optimized = optimized[:finish] + "\n\t\t\t\t\tMaterial hseTintMaterial = " + name + ".material;" + optimized[finish:]
        # The newly introduced assignment is deliberately excluded.
        tail = finish + len("\n\t\t\t\t\tMaterial hseTintMaterial = " + name + ".material;")
        optimized = optimized[:tail] + optimized[tail:].replace(name + ".material.", "hseTintMaterial.")
    replacement = """
#if UNITY_EDITOR
            using (HSEHotpathTrial.Tint()) {
                if (!HSEHotpathTrial.Optimized) {
""" + original + """
                    return;
                }
                {
""" + optimized + """
                }
            }
#elif UNITY_PSP2
            // HSE tint material local reuse; no material retained across loops/frames.
""" + optimized + """
#else
""" + original + """
#endif
        """
    if not trial:
        replacement = '\n#if UNITY_PSP2 && !UNITY_EDITOR\n            // HSE tint material local reuse; no material retained across loops/frames.\n' + optimized + '\n#else\n' + original + '\n#endif\n        '
    result = text[:start] + replacement + text[end - 1:]
    newline = "\r\n" if b"\r\n" in data else "\n"
    bom = b"\xef\xbb\xbf" if data.startswith(b"\xef\xbb\xbf") else b""
    return bom + result.replace("\n", newline).encode()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("project", type=Path)
    parser.add_argument("--checkpoint", type=Path)
    parser.add_argument("--apply", action="store_true")
    parser.add_argument("--production-only", action="store_true", help="PSP2 player only; preserve original Editor and omit the A/B helper")
    args = parser.parse_args()
    project = args.project.resolve()
    if "2018.2.19f1" not in (project / "ProjectSettings/ProjectVersion.txt").read_text():
        raise ValueError("Expected Unity 2018.2.19f1")
    relative = Path("Assets/Scripts/Assembly-CSharp/FGOL/Player.cs")
    source = project / relative
    before = source.read_bytes()
    after = prepare(before, trial=not args.production_only)
    receipt = {"file": relative.as_posix(), "before_sha256": hashlib.sha256(before).hexdigest(),
               "after_sha256": hashlib.sha256(after).hexdigest(), "scope": "PSP2 only; Editor original" if args.production_only else "PSP2; Editor A/B; other players original",
               "change": "same-loop material lookup reuse, no persistent cache or shader/math change", "applied": args.apply}
    if args.apply:
        if not args.checkpoint:
            raise ValueError("--checkpoint is required with --apply")
        args.checkpoint.mkdir(parents=True, exist_ok=False)
        backup = args.checkpoint / relative
        backup.parent.mkdir(parents=True)
        shutil.copy2(source, backup)
        shutil.copy2(str(source) + ".meta", str(backup) + ".meta")
        helper = Path(__file__).parent / "unity/HSEHotpathTrial.cs"
        target = project / "Assets/Scripts/Assembly-CSharp/HSEHotpathTrial.cs"
        if not args.production_only and target.exists():
            raise ValueError("Trial helper already exists; inspect first")
        if not args.production_only:
            shutil.copy2(helper, target)
        source.write_bytes(after)
        (args.checkpoint / "receipt.json").write_text(json.dumps(receipt, indent=2) + "\n")
    print(json.dumps(receipt, indent=2))


if __name__ == "__main__":
    main()

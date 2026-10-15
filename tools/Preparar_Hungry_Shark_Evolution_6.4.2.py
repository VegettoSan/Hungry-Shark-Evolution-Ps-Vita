#!/usr/bin/env python3
"""Prepare Unity 5.6 Data from a user-provided APK. Does NOT recreate source project.

Usage: python Preparar_Hungry_Shark_Evolution_6.4.2.py game.apk output_dir
"""
import argparse
import re
import shutil
import sys
import zipfile
from collections import defaultdict
from pathlib import Path

PREFIX = "assets/bin/Data/"
SPLIT = re.compile(r"^(.+)\.split([0-9]+)$")


def copy_entry(zf, name, destination, mode="wb"):
    destination.parent.mkdir(parents=True, exist_ok=True)
    with zf.open(name, "r") as src, destination.open(mode) as dst:
        shutil.copyfileobj(src, dst, 1024 * 1024)


def prepare(apk, output):
    apk, output = Path(apk), Path(output)
    if not apk.is_file():
        raise FileNotFoundError(apk)
    output.mkdir(parents=True, exist_ok=True)
    with zipfile.ZipFile(apk) as zf:
        all_names = [n for n in zf.namelist() if n.startswith(PREFIX) and not n.endswith("/")]
        singles, parts = [], defaultdict(dict)
        for name in all_names:
            relative = name[len(PREFIX):]
            if Path(relative).is_absolute() or ".." in Path(relative).parts:
                raise ValueError(f"Suspicious path in APK: {relative}")
            match = SPLIT.fullmatch(relative)
            if match:
                base, number = match.group(1), int(match.group(2))
                if number in parts[base]:
                    raise ValueError(f"Repeated fragment: {relative}")
                parts[base][number] = name
            else:
                singles.append((relative, name))
        for base, fragments in parts.items():
            indices = sorted(fragments)
            if indices != list(range(len(indices))):
                raise ValueError(f"Missing fragments for {base}: {indices}")
            if (output / "Data" / base).exists() or any(a == base for a, _ in singles):
                raise FileExistsError(f"Output collides with another file: {base}")
        print(f"Regular files: {len(singles)} | split groups: {len(parts)}")
        for i, (relative, name) in enumerate(singles, 1):
            copy_entry(zf, name, output / "Data" / relative)
            if i % 400 == 0:
                print(f"  Extracted {i}/{len(singles)} files...")
        for base, fragments in sorted(parts.items()):
            dest = output / "Data" / base
            for number, name in sorted(fragments.items()):
                copy_entry(zf, name, dest, "ab" if number else "wb")
            print(f"  Reconstructed {base} ({len(fragments)} fragments; {dest.stat().st_size} bytes)")
        notes = output / "INSTRUCTIONS.txt"
        notes.write_text(
            "Hungry Shark Evolution 6.4.2 - Unity 5.6.4p3 / Mono\n"
            "Data contains reconstructed Unity files; Managed is under Data/Managed.\n"
            "Open Data with AssetRipper. This is NOT the original Unity editor project.\n"
            "Work on copies; do not publish proprietary game data.\n",
            encoding="utf-8",
        )
    print("Preparation complete:", output.resolve())


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("apk", help="Path to an owned source APK")
    parser.add_argument("output", nargs="?", default="HungryShark642_Extracted")
    args = parser.parse_args()
    try:
        prepare(args.apk, args.output)
    except Exception as ex:
        print("ERROR:", ex, file=sys.stderr)
        sys.exit(1)

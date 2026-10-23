#!/usr/bin/env python3
"""Install owner-supplied BetterWinds PS Vita LiveArea PNGs, without modifying them."""

from __future__ import annotations

import argparse
import hashlib
import os
from pathlib import Path
import struct
import tempfile
import zipfile

DEST = Path(__file__).resolve().parents[1] / "assets" / "livearea" / "BetterWinds"
EXPECTED = {
    "bg0.png": ((840, 500), "452a71826ec9613c29773a4f29bebdc88c3dfe9299313048939d6a2b1152f77c"),
    "icon0.png": ((128, 128), "81d4755193278c3cde2d821398d300d01c3a6b9d5e3df868db9f9fb70c83c1c5"),
    "startup.png": ((280, 158), "293fff5b6e8abc7923ed2d9f3b1bb92b233843c7d6416fa5b657b8b2730a4f4d"),
}
V2_EXPECTED = dict(EXPECTED, **{
    "bg0.png": ((840, 500), "c5b2704c3fc91bb7d35cf46ec41941ed1c9d02f2d8a0dd90926b40aabb612e07"),
    "pic0.png": ((960, 544), "403dc5de774542a200bbc744133cff29e5151faf9679ac77d956824bd4edc732"),
    "Manual/001.png": ((960, 544), "e8a05c546a44d6b2dd598a04f3838d879a45ac5fe6b39ecc2c33c6e3b07e7e6f"),
})
V2_XML_SHA = "bd7469fe19dbc88e0688ac16b3fd717c866689e2b1e35964594fff44813d1b35"
V2_FIXED_XML_SHA = "59e615040c5edccc3d2497452be70d27a99627222114637d58616d4579e1c2dc"
V2_NATIVE_XML_SHA = "24a187fa56fb63e0ba68530073e8df84544308ef7c9756687cbd31e36c1edb51"
V2_SMALL_BG_SHA = "67debd07588d8573f01be88f42d0df0915802483d49cdcf9011444b888d58523"
PREPARED_SHA = "2c907da6bdb6b73d2bb3ad41e6d9cb55fc66917d95ea596b54c57e89cd9dc22a"


def verify_png(name: str, data: bytes, version="v1") -> None:
    size, sha = (V2_EXPECTED if version == "v2" else EXPECTED)[name]
    if hashlib.sha256(data).hexdigest() != sha:
        raise ValueError(f"{name}: SHA-256 does not match the reviewed original")
    if not data.startswith(b"\x89PNG\r\n\x1a\n") or data[12:16] != b"IHDR":
        raise ValueError(f"{name}: not a valid PNG header")
    if len(data) < 24 or struct.unpack(">II", data[16:24]) != size:
        raise ValueError(f"{name}: unexpected dimensions; expected {size}")


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("zip_path", type=Path, help="Original owner-supplied SharkEVOassets.zip")
    parser.add_argument("--verify-only", action="store_true", help="Validate ZIP without installing anything")
    parser.add_argument("--version", choices=("v1", "v2"), default="v1")
    args = parser.parse_args()
    expected = V2_EXPECTED if args.version == "v2" else EXPECTED
    with zipfile.ZipFile(args.zip_path) as src:
        names = [i.filename for i in src.infolist() if not i.is_dir()]
        if len(names) != len(set(names)) or len(names) != len({n.casefold() for n in names}):
            raise ValueError("Duplicate/case-colliding ZIP entries")
        prefix = ""
        if args.version == "v2" and "originals/bg0.png" in names:
            if hashlib.sha256(args.zip_path.read_bytes()).hexdigest() != PREPARED_SHA:
                raise ValueError("Unreviewed prepared ZIP; use the reviewed original six-file ZIP")
            prefix = "originals/"
        elif set(names) != set(expected) | ({"template.xml"} if args.version == "v2" else set()):
            raise ValueError("Unexpected ZIP content for selected artwork version")
        payload = {name: src.read(prefix + name) for name in expected}
        if args.version == "v2":
            xml = src.read(prefix + "template.xml")
            if hashlib.sha256(xml).hexdigest() != V2_XML_SHA:
                raise ValueError("Unexpected original v2 XML")
            payload["template.xml"] = xml
            # Preserve the supplied XML; emit a separately named, reviewed derivative.
            native = xml.decode('utf-8').replace('\r\n', '\n')
            native = native.replace('\n\n<livearea', '\n<livearea')
            native = native.replace('style="psmobile"', 'style="a1"').replace('>bg.png<', '>bg0.png<')
            native = native.replace('\n', '\r\n').encode('utf-8')
            if hashlib.sha256(native).hexdigest() != V2_NATIVE_XML_SHA:
                raise ValueError("Unexpected native v2 XML derivation")
            payload['native-template.xml'] = native
    for name, data in payload.items():
        if name.endswith(".png"):
            verify_png(name, data, args.version)
        print(f"VERIFIED {name} ({len(data)} bytes)")
    if args.verify_only:
        return
    dest = DEST / "v2" if args.version == "v2" else DEST
    dest.mkdir(parents=True, exist_ok=True)
    conflicts = [name for name, data in payload.items() if (dest / name).exists() and (dest / name).read_bytes() != data]
    if conflicts:
        raise FileExistsError("Refusing to overwrite changed files: " + ", ".join(conflicts))
    for name, data in payload.items():
        target = dest / name
        target.parent.mkdir(parents=True, exist_ok=True)
        if target.exists():
            continue
        with tempfile.NamedTemporaryFile(dir=target.parent, prefix=".livearea-", delete=False) as tmp:
            tmp.write(data)
            tmp_path = Path(tmp.name)
        try:
            os.replace(tmp_path, target)
        finally:
            tmp_path.unlink(missing_ok=True)
        print(f"INSTALLED {target}")
    print("Done: source PNGs installed unchanged. Unity/VPK integration is a separate step.")


if __name__ == "__main__":
    main()

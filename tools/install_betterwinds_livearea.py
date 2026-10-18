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


def verify_png(name: str, data: bytes) -> None:
    size, sha = EXPECTED[name]
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
    args = parser.parse_args()
    with zipfile.ZipFile(args.zip_path) as src:
        names = [i.filename for i in src.infolist() if not i.is_dir()]
        if len(names) != len(EXPECTED) or set(names) != set(EXPECTED):
            raise ValueError("Unexpected ZIP content; expected only bg0.png, icon0.png and startup.png")
        payload = {name: src.read(name) for name in EXPECTED}
    for name, data in payload.items():
        verify_png(name, data)
        print(f"VERIFIED {name} ({len(data)} bytes)")
    if args.verify_only:
        return
    DEST.mkdir(parents=True, exist_ok=True)
    conflicts = [name for name, data in payload.items() if (DEST / name).exists() and (DEST / name).read_bytes() != data]
    if conflicts:
        raise FileExistsError("Refusing to overwrite changed files: " + ", ".join(conflicts))
    for name, data in payload.items():
        target = DEST / name
        if target.exists():
            continue
        with tempfile.NamedTemporaryFile(dir=DEST, prefix=".livearea-", delete=False) as tmp:
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
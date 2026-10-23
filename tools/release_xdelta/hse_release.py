#!/usr/bin/env python3
"""HSE Vita v1.0 developer packaging and end-user reconstruction prototype.

Requires a separately obtained xdelta3 3.2.x binary. The script is not a game
converter and does not confer redistribution rights on any build artifact.
"""
import argparse
import hashlib
import json
import os
import shutil
import stat
import subprocess
import sys
import tempfile
import re
import struct
import zipfile
from collections import Counter
from pathlib import Path, PurePosixPath

APK_SHA256 = "52b7d8754ab8fec75c3b656a0dc5ec8dc849565adc402487a3ac093f6a481f7a"
APK_SIZE = 104296014
TITLE_ID = "HGSKE1178"
VERSION = "1.0"
BASE_NAME = "HSEVita_Stub.vpk"
PATCH_NAME = "HSEVita_Data.xdelta"
MANIFEST_NAME = "HSEVita_Release.json"
NORMALIZED_TIMESTAMP = (1980, 1, 1, 0, 0, 0)
PROTECTED = {"eboot.bin", "sce_sys/param.sfo"}
XDELTA_HASHES = {
    "encoder": "732980d52f9220dc81817ccdd0c295eadce8a0048a24c2a771a73f791ac824e9",
    "decoder": "67f84255bd009a65b1252784f8b3ebc72b5907766bc366c571e8b85fa7002a06",
}
SOURCE_WINDOW = 536870912


def sha256(path):
    h = hashlib.sha256()
    with open(path, "rb") as stream:
        for chunk in iter(lambda: stream.read(4 * 1024 * 1024), b""):
            h.update(chunk)
    return h.hexdigest()


def valid_name(name):
    if (not name or name.endswith("/") or "\\" in name or "\x00" in name
            or name.startswith("/") or ":" in name or name.startswith("./")):
        raise ValueError("Unsafe or directory entry: " + repr(name))
    parts = name.split("/")
    if (any(part in (".", "..", "") for part in parts) or "/./" in name or "//" in name
            or any(ord(c) < 32 or c in '<>"|?*' for c in name)
            or any(p.endswith((".", " ")) for p in parts)
            or any(re.fullmatch(r"(?i)(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(\..*)?", p) for p in parts)):
        raise ValueError("Unsafe path: " + repr(name))
    return name


def validate_zip(zf, required=None):
    entries = {}
    folded = set()
    for info in zf.infolist():
        if info.is_dir():
            continue
        name = valid_name(info.filename)
        if info.flag_bits & 1:
            raise ValueError("Encrypted ZIP member: " + name)
        typ = stat.S_IFMT(info.external_attr >> 16)
        if typ not in (0, stat.S_IFREG):
            raise ValueError("Special ZIP member: " + name)
        if name in entries or name.casefold() in folded:
            raise ValueError("Duplicate/case-colliding ZIP path: " + name)
        entries[name] = info
        folded.add(name.casefold())
    for name in entries:
        parts = name.split("/")
        if any("/".join(parts[:i]).casefold() in folded for i in range(1, len(parts))):
            raise ValueError("ZIP file/directory prefix collision")
    if required:
        missing = sorted(required - entries.keys())
        if missing:
            raise ValueError("Missing mandatory VPK entries: " + ", ".join(missing))
    return entries


def validate_apk(path):
    path = Path(path)
    if not path.is_file():
        raise ValueError("APK missing: " + str(path))
    if path.stat().st_size != APK_SIZE or sha256(path) != APK_SHA256:
        raise ValueError("Wrong APK. Exact Hungry Shark Evolution 6.4.2 (228) SHA-256 is required.")
    with zipfile.ZipFile(path) as zf:
        validate_zip(zf)
        for required in ("assets/bin/Data/globalgamemanagers", "assets/bin/Data/Managed/Assembly-CSharp.dll"):
            if required not in zf.namelist():
                raise ValueError("Expected original APK file missing: " + required)
    return path


def normalized_info(name, compression=zipfile.ZIP_DEFLATED):
    info = zipfile.ZipInfo(name, NORMALIZED_TIMESTAMP)
    info.compress_type = compression
    info.create_system = 3
    info.external_attr = 0o100644 << 16
    return info


def copy_member(src, name, dest, compression=zipfile.ZIP_DEFLATED):
    with src.open(name, "r") as reader, dest.open(normalized_info(name, compression), "w", force_zip64=True) as writer:
        shutil.copyfileobj(reader, writer, 1024 * 1024)


def archive_selected(source_vpk, out_file, names, order, compression=zipfile.ZIP_DEFLATED):
    with zipfile.ZipFile(source_vpk) as src, zipfile.ZipFile(out_file, "w", allowZip64=True, compresslevel=6) as dst:
        src_entries = validate_zip(src)
        for name in order:
            if name in names:
                if name not in src_entries:
                    raise ValueError("Selected file absent: " + name)
                copy_member(src, name, dst, compression)


def combine(base_zip, data_zip, out_vpk, order, payload_names):
    with zipfile.ZipFile(base_zip) as stub, zipfile.ZipFile(data_zip) as data, zipfile.ZipFile(out_vpk, "w", allowZip64=True, compresslevel=6) as output:
        base_entries = validate_zip(stub, PROTECTED)
        data_entries = validate_zip(data)
        payload_names = set(payload_names)
        if set(base_entries) & set(data_entries):
            raise ValueError("Stub and payload overlap")
        if set(data_entries) != payload_names:
            raise ValueError("Payload ZIP file list does not match manifest")
        if set(base_entries) | set(data_entries) != set(order) or len(set(order)) != len(order):
            raise ValueError("VPK entry list is incomplete or inconsistent")
        for name in order:
            copy_member(data if name in payload_names else stub, name, output)


def run_xdelta(executable, args):
    if sha256(executable) not in XDELTA_HASHES.values():
        raise ValueError("Unrecognized xdelta3 binary / binario xdelta3 no verificado")
    command = [str(executable), *args]
    environment = os.environ.copy()
    environment.pop("XDELTA", None)
    try:
        subprocess.run(command, check=True, env=environment)
    except FileNotFoundError as exc:
        raise ValueError("xdelta3 executable not found: " + str(executable)) from exc
    except subprocess.CalledProcessError as exc:
        raise ValueError("xdelta3 failed with exit code " + str(exc.returncode)) from exc


def unity_data_groups(archive):
    """Canonical unity-data-v1 source: concatenated logical Data bytes in ordinal order.

    No ZIP headers, paths or delimiters; split chunks joined numerically. This source
    is private and is always regenerated from the exact hashed user APK.
    """
    groups = {}
    for name in validate_zip(archive):
        if not name.startswith("assets/bin/Data/"):
            continue
        match = re.fullmatch(r"(.+)\.split(\d+)", name)
        logical = match[1] if match else name
        groups.setdefault(logical, []).append((int(match[2]) if match else -1, name))
    for name in sorted(groups):
        chunks = sorted(groups[name])
        indices = [i for i, _ in chunks]
        if indices != [-1] and indices != list(range(len(chunks))):
            raise ValueError("Non-contiguous or conflicting APK split group")
        yield name, [p for _, p in chunks]


def make_source(apk, out):
    with zipfile.ZipFile(apk) as z, open(out, "xb") as stream:
        for _, parts in unity_data_groups(z):
            for part in parts:
                with z.open(part) as reader:
                    shutil.copyfileobj(reader, stream, 1024 * 1024)


def file_receipts(archive):
    result = []
    for name, info in validate_zip(archive, PROTECTED).items():
        with archive.open(name) as stream:
            h = hashlib.file_digest(stream, "sha256").hexdigest()
        result.append({"name": name, "bytes": info.file_size, "sha256": h})
    return result


def sfo_identity(raw):
    if len(raw) < 20 or raw[:4] != b"\0PSF":
        raise ValueError("Invalid param.sfo header")
    _, _, keys, data, count = struct.unpack_from("<5I", raw)
    fields = {}
    for i in range(count):
        key, fmt, length, maximum, offset = struct.unpack_from("<HHIII", raw, 20 + 16 * i)
        name = raw[keys + key:].split(b"\0", 1)[0].decode("utf-8")
        value = raw[data + offset:data + offset + length]
        if len(value) != length or length > maximum:
            raise ValueError("Invalid param.sfo bounds")
        fields[name] = int.from_bytes(value, "little") if fmt == 0x404 else value.rstrip(b"\0").decode("utf-8")
    if fields.get("TITLE_ID") != TITLE_ID:
        raise ValueError("VPK Title ID mismatch")
    return {k: fields.get(k) for k in ("TITLE_ID", "TITLE", "APP_VER", "VERSION", "CATEGORY")}


def compare_vpks(original, reconstructed):
    with zipfile.ZipFile(original) as a, zipfile.ZipFile(reconstructed) as b:
        aa, bb = validate_zip(a, PROTECTED), validate_zip(b, PROTECTED)
        if set(aa) != set(bb):
            raise ValueError("Reassembled entry set mismatch")
        for name in aa:
            with a.open(name) as x, b.open(name) as y:
                while True:
                    left, right = x.read(1024 * 1024), y.read(1024 * 1024)
                    if left != right:
                        raise ValueError("Reassembled VPK bytes differ: " + name)
                    if not left:
                        break
        return len(aa)


def read_list(path):
    rows = [row.strip() for row in Path(path).read_text(encoding="utf-8-sig").splitlines()]
    names = [valid_name(row) for row in rows if row and not row.startswith("#")]
    if not names or len(names) != len(set(names)):
        raise ValueError("The paths file needs unique, explicit data paths (no wildcards)")
    if set(names) & PROTECTED:
        raise ValueError("Essential VPK files cannot be moved to payload")
    return set(names)


def command_audit(args):
    apk = validate_apk(args.apk)
    result = {"source_apk_sha256": APK_SHA256, "source_apk_size": apk.stat().st_size,
              "title_id": TITLE_ID, "vpk": None}
    print("PASS: exact HSE 6.4.2 (228) APK SHA-256 confirmed")
    if args.vpk:
        apk_hashes = {}
        with zipfile.ZipFile(apk) as source:
            if source.testzip() is not None:
                raise ValueError("APK CRC failure")
            for name in validate_zip(source):
                with source.open(name) as stream:
                    digest = hashlib.file_digest(stream, "sha256").hexdigest()
                apk_hashes.setdefault(digest, []).append([name])
            for _, parts in unity_data_groups(source):
                if len(parts) > 1:
                    digest = hashlib.sha256()
                    for part in parts:
                        with source.open(part) as stream:
                            for chunk in iter(lambda: stream.read(1024 * 1024), b""):
                                digest.update(chunk)
                    apk_hashes.setdefault(digest.hexdigest(), []).append(parts)
        with zipfile.ZipFile(args.vpk) as vpk:
            entries = validate_zip(vpk, PROTECTED)
            identity = sfo_identity(vpk.read("sce_sys/param.sfo"))
            if vpk.testzip() is not None:
                raise ValueError("VPK CRC failure")
            buckets = Counter(name.split("/")[0] for name in entries)
            receipts = file_receipts(vpk)
            for receipt in receipts:
                receipt["apk_exact_matches"] = apk_hashes.get(receipt["sha256"], [])
            result["vpk"] = {"sha256": sha256(args.vpk), "file_count": len(entries),
                             "uncompressed_bytes": sum(info.file_size for info in entries.values()),
                             "top_level_entry_counts": dict(buckets),
                             "sfo_identity": identity, "files": receipts,
                             "exact_reusable_files": sum(bool(r["apk_exact_matches"]) for r in receipts)}
            print(f"PASS: VPK has {len(entries)} files, title expected: {TITLE_ID}")
            for key, count in buckets.most_common():
                print(f"  {key}: {count} files")
    if args.report:
        Path(args.report).parent.mkdir(parents=True, exist_ok=True)
        Path(args.report).write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8")
        print("Audit report:", args.report)


def command_author(args):
    apk = validate_apk(args.apk)
    output = Path(args.out)
    if output.exists():
        raise ValueError("Output directory already exists: " + str(output))
    with zipfile.ZipFile(args.vpk) as src:
        entries = validate_zip(src, PROTECTED)
        order = list(entries)
        sfo_identity(src.read("sce_sys/param.sfo"))
        receipts = file_receipts(src)
    selected = read_list(args.data_paths)
    missing = selected - set(order)
    if missing:
        raise ValueError("Paths not found in VPK: " + ", ".join(sorted(missing)[:15]))
    print(f"Input VPK files: {len(order)}; explicit data files: {len(selected)}")
    print("Reminder: a patch or stub may still redistribute restricted material. Review before publishing.")
    output.parent.mkdir(parents=True, exist_ok=True)
    required = sum(r["bytes"] for r in receipts) * 4 + APK_SIZE * 4 + 128 * 1024 * 1024
    if shutil.disk_usage(output.parent).free < required:
        raise ValueError("Insufficient disk space / espacio insuficiente")
    with tempfile.TemporaryDirectory(prefix="hse_author_", dir=output.parent) as temporary:
        candidate = Path(temporary) / "candidate"
        candidate.mkdir()
        base_path = candidate / BASE_NAME
        patch_path = candidate / PATCH_NAME
        content = Path(temporary) / "payload.zip"
        restored = Path(temporary) / "restored.zip"
        full = Path(temporary) / "assembled.vpk"
        archive_selected(args.vpk, base_path, set(order) - selected, order)
        compression = zipfile.ZIP_STORED if args.payload_compression == "stored" else zipfile.ZIP_DEFLATED
        archive_selected(args.vpk, content, selected, order, compression)
        source = apk
        if args.source_mode == "unity-data-v1":
            source = Path(temporary) / "source.bin"
            make_source(apk, source)
        run_xdelta(args.xdelta, ["-e", "-D", "-A", "-S", "none", "-B", str(SOURCE_WINDOW), "-s", str(source), str(content), str(patch_path)])
        run_xdelta(args.decoder, ["-d", "-D", "-R", "-B", str(SOURCE_WINDOW), "-s", str(source), str(patch_path), str(restored)])
        if sha256(content) != sha256(restored):
            raise ValueError("xdelta roundtrip mismatch")
        combine(base_path, restored, full, order, selected)
        compare_vpks(args.vpk, full)
        manifest = {"format": 2, "title_id": TITLE_ID, "release_version": VERSION,
                    "distribution_status": "PRIVATE_NOT_CLEARED",
                    "apk_sha256": APK_SHA256, "apk_size": APK_SIZE,
                    "source_mode": args.source_mode, "source_sha256": sha256(source), "source_bytes": source.stat().st_size,
                    "xdelta_version": "3.2.2", "xdelta_decoder_sha256": XDELTA_HASHES["decoder"],
                    "source_window": SOURCE_WINDOW, "files": receipts,
                    "stub_filename": BASE_NAME, "stub_sha256": sha256(base_path),
                    "patch_filename": PATCH_NAME, "patch_sha256": sha256(patch_path),
                    "data_sha256": sha256(content), "data_bytes": content.stat().st_size,
                    "output_vpk_sha256": sha256(full), "uncompressed_bytes": sum(r["bytes"] for r in receipts),
                    "original_developer_vpk_sha256": sha256(args.vpk),
                    "entry_order": order, "payload_paths": [p for p in order if p in selected]}
        (candidate / MANIFEST_NAME).write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
        os.rename(candidate, output)
    print("PASS: local reconstruction and per-file byte comparison successful")
    print("Created developer-only release candidates in:", output)
    print("DO NOT PUBLISH without rights audit and a physical PS Vita test")


def command_user(args):
    apk = validate_apk(args.apk)
    bundle = Path(args.bundle)
    manifest = json.loads((bundle / MANIFEST_NAME).read_text(encoding="utf-8"))
    if (manifest.get("format") != 2 or manifest.get("title_id") != TITLE_ID or
            manifest.get("apk_sha256") != APK_SHA256):
        raise ValueError("Release manifest is not valid for this source and Title ID")
    for key in ("stub_filename", "patch_filename"):
        valid_name(manifest[key])
        if PurePosixPath(manifest[key]).name != manifest[key]:
            raise ValueError("Manifest artifact must be a flat filename")
    stub = bundle / manifest["stub_filename"]
    patch = bundle / manifest["patch_filename"]
    for path, key in ((stub, "stub_sha256"), (patch, "patch_sha256")):
        if not path.is_file() or sha256(path) != manifest[key]:
            raise ValueError("Missing/damaged release component: " + str(path))
    output = Path(args.out)
    if output.exists():
        raise ValueError("Output already exists; will not overwrite: " + str(output))
    output.parent.mkdir(parents=True, exist_ok=True)
    if shutil.disk_usage(output.parent).free < manifest["uncompressed_bytes"] * 3 + manifest["source_bytes"] + 128 * 1024 * 1024:
        raise ValueError("Insufficient disk space / espacio insuficiente")
    with tempfile.TemporaryDirectory(prefix="hse_rebuild_", dir=output.parent) as temporary:
        payload = Path(temporary) / "payload.zip"
        assembled = Path(temporary) / "final.vpk"
        source = apk
        if manifest["source_mode"] == "unity-data-v1":
            source = Path(temporary) / "source.bin"
            make_source(apk, source)
        elif manifest["source_mode"] != "apk":
            raise ValueError("Unsupported source mode")
        if sha256(source) != manifest["source_sha256"]:
            raise ValueError("Normalized source hash mismatch")
        run_xdelta(args.xdelta, ["-d", "-D", "-R", "-B", str(SOURCE_WINDOW), "-s", str(source), str(patch), str(payload)])
        if sha256(payload) != manifest["data_sha256"]:
            raise ValueError("Reconstructed data hash mismatch")
        combine(stub, payload, assembled, manifest["entry_order"], manifest["payload_paths"])
        if sha256(assembled) != manifest["output_vpk_sha256"]:
            raise ValueError("Final VPK hash mismatch")
        with zipfile.ZipFile(assembled) as vpk:
            if vpk.testzip() is not None:
                raise ValueError("Final VPK ZIP CRC check failed")
        # On Windows rename fails if destination appeared while reconstructing.
        if os.name == "nt":
            os.rename(assembled, output)
        else:
            os.link(assembled, output)
    print("PASS:", output, "| SHA-256:", sha256(output))
    print("Install this locally reconstructed VPK on PS Vita (hardware validation required).")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    subs = parser.add_subparsers(dest="command", required=True)
    a = subs.add_parser("audit", help="Validate exact original APK, optionally examine a local VPK")
    a.add_argument("--apk", required=True)
    a.add_argument("--vpk")
    a.add_argument("--report")
    a.set_defaults(func=command_audit)
    b = subs.add_parser("author", help="Developer only: make a stub + delta from an explicit VPK file list")
    b.add_argument("--apk", required=True)
    b.add_argument("--vpk", required=True)
    b.add_argument("--data-paths", required=True)
    b.add_argument("--xdelta", required=True)
    b.add_argument("--decoder", required=True)
    b.add_argument("--source-mode", choices=("apk", "unity-data-v1"), default="apk")
    b.add_argument("--payload-compression", choices=("stored", "deflated"), default="stored")
    b.add_argument("--out", required=True)
    b.set_defaults(func=command_author)
    c = subs.add_parser("user", help="User: rebuild complete VPK locally with original APK")
    c.add_argument("--apk", required=True)
    c.add_argument("--bundle", required=True)
    c.add_argument("--xdelta", required=True)
    c.add_argument("--out", required=True)
    c.set_defaults(func=command_user)
    d = subs.add_parser("compare", help="Compare every VPK member byte for byte")
    d.add_argument("--original", required=True)
    d.add_argument("--reconstructed", required=True)
    d.set_defaults(func=lambda a: print("PASS: identical files:", compare_vpks(a.original, a.reconstructed)))
    args = parser.parse_args()
    try:
        args.func(args)
    except (OSError, ValueError, KeyError, zipfile.BadZipFile) as ex:
        print("ERROR:", ex, file=sys.stderr)
        return 1
    except KeyboardInterrupt:
        print("Cancelled / Cancelado; temporary files cleaned", file=sys.stderr)
        return 130
    return 0


if __name__ == "__main__":
    sys.exit(main())

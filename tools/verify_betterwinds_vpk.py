#!/usr/bin/env python3
"""Read-only verification of BetterWinds artwork in a final private PSP2 VPK."""
import argparse
import hashlib
import json
from pathlib import Path, PurePosixPath
import struct
import xml.etree.ElementTree as ET
import zipfile

from install_betterwinds_livearea import EXPECTED, verify_png

# Reviewed output of the installed Unity 2018.2.19f1 native PSP2 icon exporter.
# It quantizes the original RGBA icon to an indexed PNG; sources stay untouched.
NATIVE_ICON_SHA256 = '28bafa4298a4aa7e00ce30df69d082710e1553d201bcdeacb81fb5c5cf9fb775'

def read_sfo(data):
    magic, _, keys, payload, count = struct.unpack_from('<5I', data)
    if magic != 0x46535000:
        raise ValueError('Invalid SFO')
    fields = {}
    for index in range(count):
        keyoff, kind, size, capacity, offset = struct.unpack_from('<HHIII', data, 20 + 16 * index)
        key = data[keys + keyoff:].split(b'\0', 1)[0].decode('ascii')
        value = data[payload + offset:payload + offset + size]
        fields[key] = value.rstrip(b'\0').decode('utf-8') if kind == 0x204 else value.hex()
    return fields


def verify(vpk, original_vpk=None):
    identity = json.loads((Path(__file__).resolve().parents[1] / 'config/port_identity.json').read_text())
    with zipfile.ZipFile(vpk) as archive:
        names = archive.namelist()
        if len(names) != len(set(names)) or len(names) != len({n.casefold() for n in names}):
            raise ValueError('Duplicate or case-colliding entries')
        if any('\\' in n or PurePosixPath(n).is_absolute() or '..' in PurePosixPath(n).parts for n in names):
            raise ValueError('Unsafe archive path')
        if archive.testzip() is not None:
            raise ValueError('CRC failure')
        contents = 'sce_sys/livearea/contents/'
        if {n for n in names if n.startswith(contents) and not n.endswith('/')} != {
                contents + n for n in ('bg0.png', 'startup.png', 'template.xml')}:
            raise ValueError('Unexpected/missing LiveArea files, possibly stale artwork')
        images = {}
        for name in EXPECTED:
            path = 'sce_sys/icon0.png' if name == 'icon0.png' else contents + name
            data = archive.read(path)
            digest = hashlib.sha256(data).hexdigest()
            if name == 'icon0.png' and digest == NATIVE_ICON_SHA256:
                if (not data.startswith(b'\x89PNG\r\n\x1a\n') or
                        struct.unpack('>II', data[16:24]) != (128, 128) or data[25] != 3):
                    raise ValueError('Unexpected native indexed icon format')
            else:
                verify_png(name, data)
            images[path] = digest
        template = ET.fromstring(archive.read(contents + 'template.xml'))
        if (template.tag != 'livearea' or template.get('style') != 'a1' or
                template.findtext('livearea-background/image') != 'bg0.png' or
                template.findtext('gate/startup-image') != 'startup.png'):
            raise ValueError('Unexpected LiveArea template references/style')
        sfo = read_sfo(archive.read('sce_sys/param.sfo'))
        if sfo['TITLE_ID'] != identity['title_id'] or sfo['TITLE'] != identity['display_name']:
            raise ValueError('Noncanonical title identity')
        if original_vpk:
            with zipfile.ZipFile(original_vpk) as previous:
                old = read_sfo(previous.read('sce_sys/param.sfo'))
                for key in ('TITLE_ID', 'TITLE', 'STITLE', 'CONTENT_ID', 'ATTRIBUTE2'):
                    if sfo[key] != old[key]:
                        raise ValueError('Existing identity/memory field changed: ' + key)
                if archive.read('sce_sys/pic0.png') != previous.read('sce_sys/pic0.png'):
                    raise ValueError('Existing pic0 changed')
        with vpk.open('rb') as stream:
            digest = hashlib.file_digest(stream, 'sha256').hexdigest()
        return {'sha256': digest,
                'bytes': vpk.stat().st_size, 'entries': len(names), 'images': images,
                'title_id': sfo['TITLE_ID'], 'title': sfo['TITLE'],
                'template_format': template.get('format-ver'),
                'icon_conversion': 'Unity native RGBA to indexed PNG' if images['sce_sys/icon0.png'] == NATIVE_ICON_SHA256 else 'none',
                'hardware': 'NOT TESTED'}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('vpk', type=Path)
    parser.add_argument('--previous-vpk', type=Path, help='Check identity, memory flag and pic0 preservation')
    args = parser.parse_args()
    print(json.dumps(verify(args.vpk, args.previous_vpk), indent=2))


if __name__ == '__main__':
    main()

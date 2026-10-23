#!/usr/bin/env python3
"""Read-only verification of BetterWinds artwork in a final private PSP2 VPK."""
import argparse
import hashlib
import json
from pathlib import Path, PurePosixPath
import struct
import xml.etree.ElementTree as ET
import zipfile

from install_betterwinds_livearea import EXPECTED, V2_EXPECTED, V2_NATIVE_XML_SHA, V2_SMALL_BG_SHA, verify_png

# Reviewed output of the installed Unity 2018.2.19f1 native PSP2 icon exporter.
# It quantizes the original RGBA icon to an indexed PNG; sources stay untouched.
NATIVE_ICON_SHA256 = '28bafa4298a4aa7e00ce30df69d082710e1553d201bcdeacb81fb5c5cf9fb775'
NATIVE_V2_PIC_SHA256 = 'b8a350dfb61640bc018a5a47ffd1bbc4767338befccadfd1fa478defceea378c'

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


def verify(vpk, original_vpk=None, version='v1'):
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
        expected = V2_EXPECTED if version == 'v2' else EXPECTED
        for name in expected:
            path = ('sce_sys/manual/001.png' if name == 'Manual/001.png' else
                    'sce_sys/' + name if name in ('icon0.png', 'pic0.png') else contents + name)
            data = archive.read(path)
            digest = hashlib.sha256(data).hexdigest()
            if ((name == 'icon0.png' and digest == NATIVE_ICON_SHA256) or
                    (version == 'v2' and name == 'pic0.png' and digest == NATIVE_V2_PIC_SHA256)):
                if (not data.startswith(b'\x89PNG\r\n\x1a\n') or
                        struct.unpack('>II', data[16:24]) != expected[name][0] or data[25] != 3):
                    raise ValueError('Unexpected native indexed presentation format')
            elif version == 'v2' and name == 'bg0.png' and digest == V2_SMALL_BG_SHA:
                if struct.unpack('>II', data[16:24]) != (840, 500) or data[25] != 3:
                    raise ValueError('Unexpected prepared background format')
            else:
                verify_png(name, data, version)
            images[path] = digest
        template = ET.fromstring(archive.read(contents + 'template.xml'))
        if (template.tag != 'livearea' or template.get('style') != 'a1' or
                template.findtext('livearea-background/image') != 'bg0.png' or
                template.findtext('gate/startup-image') != 'startup.png'):
            raise ValueError('Unexpected LiveArea template references/style')
        if version == 'v2':
            if hashlib.sha256(archive.read(contents + 'template.xml')).hexdigest() != V2_NATIVE_XML_SHA:
                raise ValueError('Unreviewed v2 template')
            if {n for n in names if n.startswith('sce_sys/manual/') and not n.endswith('/')} != {'sce_sys/manual/001.png'}:
                raise ValueError('Unexpected manual pages')
        sfo = read_sfo(archive.read('sce_sys/param.sfo'))
        if sfo['TITLE_ID'] != identity['title_id'] or sfo['TITLE'] != identity['display_name']:
            raise ValueError('Noncanonical title identity')
        if original_vpk:
            with zipfile.ZipFile(original_vpk) as previous:
                old = read_sfo(previous.read('sce_sys/param.sfo'))
                for key in ('TITLE_ID', 'TITLE', 'STITLE', 'CONTENT_ID', 'ATTRIBUTE2'):
                    if sfo[key] != old[key]:
                        raise ValueError('Existing identity/memory field changed: ' + key)
                if version == 'v1' and archive.read('sce_sys/pic0.png') != previous.read('sce_sys/pic0.png'):
                    raise ValueError('Existing pic0 changed')
                if version == 'v2':
                    old_names = {n for n in previous.namelist() if not n.endswith('/')}
                    new_names = {n for n in names if not n.endswith('/')}
                    allowed = {contents + 'bg0.png', contents + 'template.xml', 'sce_sys/pic0.png', 'sce_sys/manual/001.png'}
                    changed = old_names ^ new_names
                    changed |= {n for n in old_names & new_names if archive.read(n) != previous.read(n)}
                    if changed - allowed:
                        raise ValueError('Files outside presentation scope changed: ' + ', '.join(sorted(changed - allowed)))
        with vpk.open('rb') as stream:
            digest = hashlib.file_digest(stream, 'sha256').hexdigest()
        return {'sha256': digest, 'artwork_version': version,
                'bytes': vpk.stat().st_size, 'entries': len(names), 'images': images,
                'title_id': sfo['TITLE_ID'], 'title': sfo['TITLE'],
                'template_format': template.get('format-ver'),
                'template_style': template.get('style'),
                'pic0_conversion': 'Unity native palette export' if version == 'v2' and images['sce_sys/pic0.png'] == NATIVE_V2_PIC_SHA256 else 'none',
                'icon_conversion': 'Unity native RGBA to indexed PNG' if images['sce_sys/icon0.png'] == NATIVE_ICON_SHA256 else 'none',
                'hardware': 'NOT TESTED'}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('vpk', type=Path)
    parser.add_argument('--previous-vpk', type=Path, help='Check identity, memory flag and pic0 preservation')
    parser.add_argument('--version', choices=('v1', 'v2'), default='v1')
    args = parser.parse_args()
    print(json.dumps(verify(args.vpk, args.previous_vpk, args.version), indent=2))


if __name__ == '__main__':
    main()

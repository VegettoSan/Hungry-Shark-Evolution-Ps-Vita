"""Package already verified release components without rebuilding or altering the game.

This creates local artifacts only; it never uploads files or approves phase gates.
Compiler-path findings and distribution permissions remain a maintainer decision.
"""
import argparse
import json
import shutil
import zipfile
from pathlib import Path

import hse_release as h

CORE = ('HSERebuild.exe', 'HSEVita_Stub.vpk', 'HSEVita_Data.xdelta',
        'HSEVita_Release.json', 'xdelta3decode.exe', 'xdelta3-LICENSE.txt')


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('bundle', 'receipt', 'guide', 'out', 'archive'):
        parser.add_argument('--' + name, required=True)
    args = parser.parse_args()
    bundle, output, archive = map(Path, (args.bundle, args.out, args.archive))
    if output.exists() or archive.exists():
        raise ValueError('Choose new output paths; existing files will not be replaced')
    receipt = json.loads(Path(args.receipt).read_text(encoding='utf-8'))
    expected = {r['name']: r for r in receipt['candidate_artifacts']}
    for name in CORE:
        path = bundle / name
        if path.stat().st_size != expected[name]['bytes'] or h.sha256(path) != expected[name]['sha256']:
            raise ValueError('Previously verified component changed: ' + name)
    manifest = json.loads((bundle / h.MANIFEST_NAME).read_text(encoding='utf-8'))
    if manifest['original_developer_vpk_sha256'] != receipt['original_vpk']['sha256']:
        raise ValueError('Original reference mismatch')
    with zipfile.ZipFile(bundle / h.BASE_NAME) as base:
        h.validate_zip(base, h.PROTECTED)
        h.sfo_identity(base.read('sce_sys/param.sfo'))
        if base.testzip() is not None:
            raise ValueError('Base VPK CRC failure')
    output.mkdir(parents=True)
    for name in CORE:
        if name != h.MANIFEST_NAME:
            shutil.copyfile(bundle / name, output / name)
    # Status metadata only. All decoder inputs, entry receipts, hashes and ordering stay exact.
    manifest['distribution_status'] = 'OWNER_VALIDATED_LOCAL_RELEASE_PACKAGE'
    (output / h.MANIFEST_NAME).write_text(json.dumps(manifest, indent=2) + '\n', encoding='utf-8')
    shutil.copyfile(args.guide, output / 'README_ES_EN.md')
    notices = (
        'xdelta3 3.2.2 by Joshua MacDonald. Apache License 2.0.\n'
        'Official source and binary provenance: https://github.com/jmacd/xdelta/releases/tag/v3.2.2\n'
        'License: https://github.com/jmacd/xdelta/blob/v3.2.2/xdelta3/LICENSE\n'
        'The included decoder is unmodified; its SHA-256 is pinned in the extractor.\n'
        'LiveArea artwork credit: BetterWinds, https://github.com/BetterWinds\n'
        'This notice does not replace notices or permissions required for game/runtime components.\n'
    )
    (output / 'THIRD_PARTY_NOTICES.txt').write_text(notices, encoding='utf-8')
    maintainers = (
        'LOCAL RELEASE PACKAGE / PAQUETE LOCAL PARA RELEASE v1.0\n\n'
        'The owner reports the complete requested Vita matrix PASS and states that\n'
        'all redistribution documents are in order. These are owner attestations;\n'
        'the underlying documents were not independently inspected.\n\n'
        'Known privacy finding: two absolute build-path strings remain in\n'
        'Media/Modules/MonoAssembliesPSP2.suprx; a third-party SDK build path\n'
        'also exists in Facebook.Unity.Settings.dll. Their actual values are\n'
        'not copied into these notes. Native game/runtime bytes are unchanged.\n'
        'Local packaging does not by itself resolve the earlier public-upload\n'
        'restriction on private paths or approve a formal project phase.\n\n'
        'No APK or complete reconstructed VPK is included. No upload is performed.\n'
        'Windows 10/clean-PC and forced-power-loss tests remain unrecorded.\n'
        'Mbed TLS embedded test keys were matched to upstream public test data.\n'
    )
    (output / 'MAINTAINER_NOTES.txt').write_text(maintainers, encoding='utf-8')
    files = [{'name': p.name, 'bytes': p.stat().st_size, 'sha256': h.sha256(p)}
             for p in sorted(output.iterdir())]
    (output / 'SHA256SUMS.txt').write_text(''.join(r['sha256'] + '  ' + r['name'] + '\n' for r in files), encoding='utf-8')
    archive.parent.mkdir(parents=True, exist_ok=True)
    with zipfile.ZipFile(archive, 'x', compression=zipfile.ZIP_DEFLATED, compresslevel=9) as z:
        for p in sorted(output.iterdir()):
            info = h.normalized_info(p.name)
            with p.open('rb') as source, z.open(info, 'w', force_zip64=True) as target:
                shutil.copyfileobj(source, target, 1024 * 1024)
    with zipfile.ZipFile(archive) as z:
        if z.testzip() is not None:
            raise ValueError('Release ZIP CRC failed')
        if set(z.namelist()) != {p.name for p in output.iterdir()}:
            raise ValueError('Release ZIP membership mismatch')
    digest = h.sha256(archive)
    checksum = archive.with_name(archive.name + '.sha256')
    with checksum.open('x', encoding='utf-8') as f:
        f.write(digest + '  ' + archive.name + '\n')
    result = {'status': 'LOCAL_PACKAGE_CREATED_NOT_UPLOADED',
              'archive': {'name': archive.name, 'bytes': archive.stat().st_size, 'sha256': digest},
              'files': files, 'game_binary_changes': 0, 'phase_changes': 0,
              'privacy': 'Known compiler paths retained; see MAINTAINER_NOTES.txt'}
    archive.with_suffix('.receipt.json').write_text(json.dumps(result, indent=2) + '\n', encoding='utf-8')
    print(json.dumps(result, indent=2))


if __name__ == '__main__':
    main()

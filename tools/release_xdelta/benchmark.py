"""Private Windows comparison; outputs contain proprietary data. Never upload them."""
import argparse
import json
import subprocess
import sys
import time
from pathlib import Path

import hse_release as h


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for key in ('apk', 'vpk', 'data-paths', 'xdelta', 'decoder', 'out'):
        parser.add_argument('--' + key, required=True)
    args = parser.parse_args()
    output = Path(args.out)
    output.mkdir(parents=True, exist_ok=True)
    results = []
    for mode, compression in [('apk', 'deflated'), ('apk', 'stored'), ('unity-data-v1', 'stored')]:
        label = mode + '-' + compression
        dest = output / label
        start = time.monotonic()
        command = [sys.executable, str(Path(h.__file__)), 'author', '--apk', args.apk,
                   '--vpk', args.vpk, '--data-paths', args.data_paths, '--xdelta', args.xdelta,
                   '--decoder', args.decoder, '--out', str(dest), '--source-mode', mode,
                   '--payload-compression', compression]
        with (output / (label + '.log')).open('w', encoding='utf-8') as log:
            subprocess.run(command, check=True, stdout=log, stderr=subprocess.STDOUT)
        manifest = json.loads((dest / h.MANIFEST_NAME).read_text())
        row = {'variant': label, 'author_roundtrip_compare_seconds': round(time.monotonic() - start, 3),
               'source_bytes': manifest['source_bytes'], 'data_bytes': manifest['data_bytes'],
               'stub_bytes': (dest / h.BASE_NAME).stat().st_size,
               'patch_bytes': (dest / h.PATCH_NAME).stat().st_size,
               'patch_sha256': h.sha256(dest / h.PATCH_NAME),
               'status': 'PASS', 'rights': 'NOT CLEARED', 'hardware': 'NOT TESTED'}
        results.append(row)
        (output / 'benchmark.json').write_text(json.dumps(results, indent=2) + '\n')
        print(json.dumps(row), flush=True)


if __name__ == '__main__':
    main()

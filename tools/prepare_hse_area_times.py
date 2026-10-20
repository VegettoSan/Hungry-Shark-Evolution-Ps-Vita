"""Avoid legacy Mono enum boxing for runtime area-time accounting; dry-run by default."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil


def prepare(data, include_editor=True):
    text = data.decode('utf-8-sig').replace('\r\n', '\n')
    if 'HSE area-time typed equality' in text:
        raise ValueError('Already adapted')
    constructor = 'm_areaGameTimes = new Dictionary<AreaTransitionManager.Area, float>();'
    start = text.index('if (!m_areaGameTimes.ContainsKey(currentArea))')
    end = text.index('\n\t\tif (m_liveEventSession', start)
    original = text[start:end]
    if text.count(constructor) != 1 or original.count('m_areaGameTimes[') != 2:
        raise ValueError('Unexpected source; inspect manually')
    optimized = '''float hseAreaTime;
        m_areaGameTimes[currentArea] = m_areaGameTimes.TryGetValue(currentArea, out hseAreaTime) ? hseAreaTime + Time.deltaTime : 0f;'''
    replacement = '''#if UNITY_PSP2 || UNITY_EDITOR
        // HSE area-time typed equality; first visit still records zero.
        ''' + optimized + '''
#else
        ''' + original + '''
#endif'''
    text = text[:start] + replacement + text[end:]
    text = text.replace(constructor, '''#if UNITY_PSP2 || UNITY_EDITOR
        m_areaGameTimes = new Dictionary<AreaTransitionManager.Area, float>(new HSEVitaAreaComparer());
#else
        ''' + constructor + '''
#endif''')
    if not include_editor:
        text = text.replace('#if UNITY_PSP2 || UNITY_EDITOR', '#if UNITY_PSP2 && !UNITY_EDITOR')
    newline = '\r\n' if b'\r\n' in data else '\n'
    bom = b'\xef\xbb\xbf' if data.startswith(b'\xef\xbb\xbf') else b''
    return bom + text.replace('\n', newline).encode()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('project', type=Path)
    parser.add_argument('--checkpoint', type=Path)
    parser.add_argument('--apply', action='store_true')
    parser.add_argument('--production-only', action='store_true', help='PSP2 player only; original Editor path')
    args = parser.parse_args()
    project = args.project.resolve()
    if '2018.2.19f1' not in (project/'ProjectSettings/ProjectVersion.txt').read_text():
        raise ValueError('Expected Unity 2018.2.19f1')
    relative = Path('Assets/Scripts/Assembly-CSharp/GameLogic.cs')
    source = project/relative
    before = source.read_bytes()
    after = prepare(before, include_editor=not args.production_only)
    helper = Path(__file__).parent/'unity/HSEVitaAreaComparer.cs'
    target = project/'Assets/Scripts/Assembly-CSharp/HSEVitaAreaComparer.cs'
    if target.exists() and target.read_bytes() != helper.read_bytes():
        raise ValueError('Different comparer already exists')
    receipt = {'file': relative.as_posix(), 'before_sha256': hashlib.sha256(before).hexdigest(),
               'after_sha256': hashlib.sha256(after).hexdigest(), 'applied': args.apply,
               'scope': ('PSP2 only; Editor original' if args.production_only else 'PSP2 and Editor')+'; runtime dictionary only',
               'contract': 'same Int32 enum equality, first visit zero, same float addition, no save-format change'}
    if args.apply:
        if not args.checkpoint:
            raise ValueError('--checkpoint required')
        args.checkpoint.mkdir(parents=True, exist_ok=False)
        backup = args.checkpoint/relative
        backup.parent.mkdir(parents=True)
        shutil.copy2(source, backup)
        shutil.copy2(str(source)+'.meta', str(backup)+'.meta')
        shutil.copy2(helper, target)
        source.write_bytes(after)
        (args.checkpoint/'receipt.json').write_text(json.dumps(receipt, indent=2)+'\n')
    print(json.dumps(receipt, indent=2))


if __name__ == '__main__':
    main()

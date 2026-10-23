"""Portable structural tests. Real Windows/xdelta tests are separate and explicit."""
import io
import tempfile
import unittest
import zipfile
from pathlib import Path

import hse_release as h
from inspect_delta import inspect


class ArchiveTests(unittest.TestCase):
    def test_vcdiff_accounting(self):
        # Generated from synthetic abcdef repeats + NEW, with real -S none xdelta.
        raw = bytes.fromhex('d6c3c4000005aa180016ac4400030702f4768eb54e455713aa18042382290003')
        with tempfile.TemporaryDirectory() as directory:
            patch = Path(directory) / 'fixture.vcdiff'; patch.write_bytes(raw)
            result = inspect(patch)
            self.assertEqual(result['instructions'], dict(source_copy_bytes=5400,
                             target_copy_bytes=297, add_bytes=3, run_bytes=0))
            patch.write_bytes(raw[:-1])
            with self.assertRaises(ValueError): inspect(patch)

    def test_unsafe_paths(self):
        for name in ('../foo', 'a/../b', '/root', 'C:/x', 'a\\x', 'foo//bar',
                     'a/./b', 'a/.', 'a./b', 'a /b', 'CON.txt', 'a\x01', 'x*/b'):
            with self.subTest(name=name), self.assertRaises(ValueError):
                h.valid_name(name)

    def test_prefix_case_and_symlink(self):
        for names in (('A', 'a'), ('a', 'a/b')):
            stream = io.BytesIO()
            with zipfile.ZipFile(stream, 'w') as z:
                for n in names:
                    z.writestr(n, b'test')
            with zipfile.ZipFile(stream) as z, self.assertRaises(ValueError):
                h.validate_zip(z)
        stream = io.BytesIO()
        with zipfile.ZipFile(stream, 'w') as z:
            info = zipfile.ZipInfo('link'); info.external_attr = 0o120777 << 16
            z.writestr(info, 'target')
        with zipfile.ZipFile(stream) as z, self.assertRaises(ValueError):
            h.validate_zip(z)

    def test_split_normalization(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory); apk = root / 'source.apk'; target = root / 'source.bin'
            with zipfile.ZipFile(apk, 'w') as z:
                z.writestr('assets/bin/Data/b.split1', b'2')
                z.writestr('assets/bin/Data/b.split0', b'1')
                z.writestr('assets/bin/Data/a', b'a')
            h.make_source(apk, target)
            self.assertEqual(target.read_bytes(), b'a12')
            with zipfile.ZipFile(apk, 'a') as z:
                z.writestr('assets/bin/Data/c.split1', b'gap')
            with zipfile.ZipFile(apk) as z, self.assertRaises(ValueError):
                list(h.unity_data_groups(z))

    def test_byte_compare_and_complete_entry_set(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory); a, b = root / 'a.vpk', root / 'b.vpk'
            def write(path, data):
                with zipfile.ZipFile(path, 'w') as z:
                    for name, content in data.items():
                        z.writestr(name, content)
            data = {'eboot.bin': b'fixture only', 'sce_sys/param.sfo': b'fixture', 'path': b'abc'}
            write(a, data); write(b, data)
            self.assertEqual(h.compare_vpks(a, b), 3)
            data['path'] = b'abd'; write(b, data)
            with self.assertRaises(ValueError): h.compare_vpks(a, b)
            data['path'] = b'abc'; data['unexpected'] = b'x'; write(b, data)
            with self.assertRaises(ValueError): h.compare_vpks(a, b)


if __name__ == '__main__':
    unittest.main()

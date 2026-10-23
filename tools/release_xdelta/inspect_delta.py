"""Account RFC3284 default-table/-S none windows without copying literal bytes.

This is an accounting reader, not an alternative decoder. Reject unsupported
formats. Real xdelta decode + byte comparisons remain mandatory.
"""
import argparse
import json
import io
from pathlib import Path

import hse_release as h


def integer(stream):
    value = 0
    for _ in range(10):
        raw = stream.read(1)
        if not raw:
            raise ValueError('Truncated integer')
        value = value * 128 + (raw[0] & 127)
        if raw[0] < 128:
            return value
    raise ValueError('Oversized integer')


def byte(stream):
    raw = stream.read(1)
    if not raw:
        raise ValueError('Truncated byte')
    return raw[0]


def code_table():
    table = [[('RUN', 0, 0)]] + [[('ADD', n, 0)] for n in range(18)]
    for mode in range(9):
        table += [[('COPY', n, mode)] for n in [0, *range(4, 19)]]
    for mode in range(6):
        for add in range(1, 5):
            for copy in range(4, 7):
                table.append([('ADD', add, 0), ('COPY', copy, mode)])
    for mode in range(6, 9):
        for add in range(1, 5):
            table.append([('ADD', add, 0), ('COPY', 4, mode)])
    table += [[('COPY', 4, mode), ('ADD', 1, 0)] for mode in range(9)]
    assert len(table) == 256
    return table


def inspect(patch):
    counts = dict(source_copy_bytes=0, target_copy_bytes=0, add_bytes=0, run_bytes=0)
    target = windows = literal_section_bytes = 0
    table = code_table()
    with open(patch, 'rb') as stream:
        if stream.read(5) != b'\xd6\xc3\xc4\x00\x00':
            raise ValueError('Only unarmored VCDIFF v0, no app header/secondary/custom table supported')
        while raw := stream.read(1):
            indicator = raw[0]
            if indicator & ~7 or indicator & 3 == 3:
                raise ValueError('Unsupported window indicator')
            source_length = 0
            if indicator & 3:
                source_length = integer(stream); integer(stream)  # segment position
            length = integer(stream); end = stream.tell() + length
            size = integer(stream)
            if byte(stream) != 0:
                raise ValueError('Secondary-compressed sections unsupported')
            data_len, inst_len, addr_len = [integer(stream) for _ in range(3)]
            if indicator & 4:  # xdelta Adler32 extension
                if len(stream.read(4)) != 4:
                    raise ValueError('Truncated checksum')
            if end != stream.tell() + data_len + inst_len + addr_len:
                raise ValueError('Invalid window length')
            stream.seek(data_len, 1)  # never retain/export literal data
            instructions, addresses = io.BytesIO(stream.read(inst_len)), io.BytesIO(stream.read(addr_len))
            if stream.tell() != end or len(instructions.getbuffer()) != inst_len or len(addresses.getbuffer()) != addr_len:
                raise ValueError('Truncated sections')
            near, same, slot, written, used_data = [0] * 4, [0] * 768, 0, 0, 0
            while instructions.tell() < inst_len:
                for kind, n, mode in table[byte(instructions)]:
                    if n == 0:
                        n = integer(instructions)
                    if n <= 0 or written + n > size:
                        raise ValueError('Invalid instruction size')
                    if kind == 'ADD':
                        counts['add_bytes'] += n; used_data += n
                    elif kind == 'RUN':
                        counts['run_bytes'] += n; used_data += 1
                    else:
                        here = source_length + written
                        if mode == 0:
                            address = integer(addresses)
                        elif mode == 1:
                            address = here - integer(addresses)
                        elif mode < 6:
                            address = near[mode - 2] + integer(addresses)
                        else:
                            address = same[(mode - 6) * 256 + byte(addresses)]
                        if not 0 <= address < here:
                            raise ValueError('Invalid COPY address')
                        if address < source_length and address + n > source_length:
                            raise ValueError('COPY crosses source boundary')
                        key = 'source_copy_bytes' if address < source_length and indicator & 1 else 'target_copy_bytes'
                        counts[key] += n
                        near[slot] = address; slot = (slot + 1) % 4; same[address % 768] = address
                    written += n
            if written != size or used_data != data_len or addresses.tell() != addr_len:
                raise ValueError('Incomplete instruction/data/address accounting')
            target += size; windows += 1; literal_section_bytes += data_len
    if sum(counts.values()) != target or target == 0:
        raise ValueError('Instruction accounting incomplete')
    return {'instructions': counts, 'target_bytes': target, 'windows': windows,
            'literal_section_bytes': literal_section_bytes,
            'warning': 'COPY counts are output bytes, not unique bytes or redistribution permission.'}


if __name__ == '__main__':
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument('--patch', required=True)
    p.add_argument('--out', required=True)
    a = p.parse_args()
    Path(a.out).write_text(json.dumps(inspect(a.patch), indent=2) + '\n')

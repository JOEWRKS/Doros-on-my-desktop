"""Lossless container conversion only: no pixel, pose, order or timing changes."""
import gzip
import struct
from pathlib import Path
assets = Path(__file__).resolve().parents[1] / 'src/Dororong.App/Assets'
def convert(name, header, sizes):
    raw = gzip.decompress((assets / (name + '.pbgra.gz')).read_bytes())
    if raw[:20] != header or len(raw) != 20 + sum(sizes):
        raise ValueError('Unexpected approved bank: ' + name)
    blocks, cursor = [], 20
    for size in sizes:
        frame = raw[cursor:cursor+size]
        block = gzip.compress(frame, compresslevel=9, mtime=0)
        assert gzip.decompress(block) == frame
        blocks.append(block)
        cursor += size
    offset, table = 12 + len(blocks) * 12, []
    for block, size in zip(blocks, sizes):
        table.append(struct.pack('<III', offset, len(block), size))
        offset += len(block)
    result = b'IMBK' + struct.pack('<II', 1, len(blocks)) + b''.join(table) + b''.join(blocks)
    (assets / (name + '.indexed')).write_bytes(result)
    print(name, len(blocks), len(raw), 'raw bytes ->', len(result), 'indexed bytes; exact pixels')
convert('hunting', b'HUNT' + struct.pack('<IIII', 1, 256, 256, 157),
        [256*256*4]*157 + [96*96*4, 384*384*4]*2)
convert('locomotion', b'LOCO' + struct.pack('<IIII', 2, 96, 96, 772), [96*96*4]*772)

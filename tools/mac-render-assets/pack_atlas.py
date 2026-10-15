"""Losslessly reblock a derived JOIN v2 bank; only accepts a NEW output file."""
import argparse
import gzip
import io
from pathlib import Path
import struct


def exact(stream, size):
    value = stream.read(size)
    if len(value) != size:
        raise ValueError("Truncated bank")
    return value


def number(stream):
    return struct.unpack("<i", exact(stream, 4))[0]


def pack(source, destination):
    with source.open("rb") as src, destination.open("xb") as dst:
        if exact(src, 16) != b"JOIN" + struct.pack("<iii", 2, 157, 2801):
            raise ValueError("Wrong bank version")
        baseline = exact(src, 32)
        if baseline.hex().upper() != "EF1603705D1CB6073E9D0DBFF8562EAC94E8D4EC920B29A9A9FEB47319E6274C":
            raise ValueError("Unapproved source")
        dst.write(b"JOIN" + struct.pack("<iii", 3, 157, 2801) + baseline)
        for frame in range(157):
            size = number(src)
            if not 0 < size < 2_000_000:
                raise ValueError("Invalid chunk size")
            raw = io.BytesIO(gzip.decompress(exact(src, size)))
            for start in range(0, 2801, 32):
                block = bytearray()
                for _ in range(start, min(start + 32, 2801)):
                    header = exact(raw, 4)
                    x, y, w, h = header
                    if x+w > 128 or y+h > 128:
                        raise ValueError("Invalid patch bounds")
                    block.extend(header)
                    block.extend(exact(raw, w*h*4))
                compressed = gzip.compress(block, compresslevel=9, mtime=0)
                if gzip.decompress(compressed) != block:
                    raise ValueError("Repacking changed pixels")
                dst.write(struct.pack("<i", len(compressed)) + compressed)
            if raw.read(1):
                raise ValueError("Extra decoded bytes")
        if src.read(1):
            raise ValueError("Extra bank bytes")
    print(f"439757 patches repacked byte-exact: {destination.stat().st_size} bytes")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("source", type=Path)
    parser.add_argument("destination", type=Path)
    args = parser.parse_args()
    pack(args.source, args.destination)

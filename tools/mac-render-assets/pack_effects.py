"""Losslessly reblock EFCT v2 into bounded32-sample EFCT v3 chunks."""
import argparse
import gzip
import hashlib
import io
import json
from pathlib import Path
import struct

BASELINE = bytes.fromhex("EF1603705D1CB6073E9D0DBFF8562EAC94E8D4EC920B29A9A9FEB47319E6274C")
BLOCK = 32


def exact(stream, size):
    data = stream.read(size)
    if len(data) != size:
        raise ValueError("Truncated effect bank")
    return data


def integer(stream):
    return struct.unpack("<i", exact(stream, 4))[0]


def pack(source, destination):
    before, after = hashlib.sha256(), hashlib.sha256()
    samples, chunks = 0, 0
    with source.open("rb") as src, destination.open("xb") as dst:
        if exact(src, 12) != b"EFCT" + struct.pack("<ii", 2, 590) or exact(src, 32) != BASELINE:
            raise ValueError("Wrong effect source/version")
        dst.write(b"EFCT" + struct.pack("<iii", 3, 590, BLOCK) + BASELINE)
        for group in range(590):
            size = integer(src)
            if not 0 < size <= 1_000_000:
                raise ValueError("Invalid compressed size")
            raw = io.BytesIO(gzip.decompress(exact(src, size)))
            count = integer(raw)
            if count != (48001 if group == 589 else 401):
                raise ValueError("Wrong sample count")
            for start in range(0, count, BLOCK):
                block = bytearray()
                for _ in range(start, min(start + BLOCK, count)):
                    header = exact(raw, 4)
                    x, y, w, h = header
                    if x + w > 96 or y + h > 96:
                        raise ValueError("Invalid patch bounds")
                    block.extend(header)
                    block.extend(exact(raw, w * h * 4))
                    samples += 1
                before.update(block)
                compressed = gzip.compress(block, compresslevel=9, mtime=0)
                decoded = gzip.decompress(compressed)
                if decoded != block:
                    raise ValueError("Changed effect bytes")
                after.update(decoded)
                dst.write(struct.pack("<i", len(compressed)) + compressed)
                chunks += 1
            if raw.read(1):
                raise ValueError("Unexpected sample bytes")
        if src.read(1):
            raise ValueError("Unexpected bank bytes")
    result = {"samples": samples, "chunks": chunks, "block_size": BLOCK,
              "original_patch_sha256": before.hexdigest(), "reblocked_patch_sha256": after.hexdigest(),
              "bytes": destination.stat().st_size, "sha256": hashlib.sha256(destination.read_bytes()).hexdigest()}
    assert result["original_patch_sha256"] == result["reblocked_patch_sha256"]
    return result


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("source", type=Path)
    parser.add_argument("destination", type=Path)
    parser.add_argument("--report", type=Path)
    args = parser.parse_args()
    report = json.dumps(pack(args.source, args.destination), indent=2) + "\n"
    if args.report:
        with args.report.open("x", encoding="utf-8") as output:
            output.write(report)
    print(report)

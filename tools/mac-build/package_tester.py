"""Build an arm64 tester ZIP on Windows without dropping Unix execute bits.

Only fresh output directories are accepted. No installers, uploads, certificates,
login changes or security-policy overrides. rcodesign supplies ad-hoc integrity,
NOT Developer ID identity or notarization. Native acceptance remains separate.
"""
import argparse
import hashlib
import json
import os
from pathlib import Path, PurePosixPath
import plistlib
import re
import shutil
import stat
import struct
import subprocess
import zipfile

ROOT = Path(__file__).resolve().parents[2]
APP = "Doro Mac Test.app"
RUNTIME = "8.0.32"
ARM64 = 0x0100000C


def sha(data):
    return hashlib.sha256(data).hexdigest()


def macho_cpus(data):
    if data[:4] == b"\xcf\xfa\xed\xfe":
        return [struct.unpack_from("<I", data, 4)[0]]
    if data[:4] in (b"\xca\xfe\xba\xbe", b"\xca\xfe\xba\xbf"):
        count = struct.unpack_from(">I", data, 4)[0]
        stride = 32 if data[:4] == b"\xca\xfe\xba\xbf" else 20
        if count > 32 or len(data) < 8 + count * stride:
            raise ValueError("Invalid universal Mach-O")
        return [struct.unpack_from(">I", data, 8+i*stride)[0] for i in range(count)]
    return []


def ico_to_icns(data):
    # Container conversion only: preserve approved PNG bytes; no redraw/resampling.
    reserved, kind, count = struct.unpack_from("<HHH", data)
    if reserved != 0 or kind != 1:
        raise ValueError("Expected ICO")
    types = {16: b"icp4", 32: b"icp5", 64: b"icp6", 128: b"ic07", 256: b"ic08"}
    chunks = []
    for i in range(count):
        w, h, _, _, _, _, size, offset = struct.unpack_from("<BBBBHHII", data, 6+i*16)
        w, h = w or 256, h or 256
        if w not in types or w != h:
            continue
        png = data[offset:offset+size]
        if len(png) != size or png[:8] != b"\x89PNG\r\n\x1a\n" or struct.unpack_from(">II", png, 16) != (w, h):
            raise ValueError("Invalid ICO PNG")
        chunks.append(types[w]+struct.pack(">I", size+8)+png)
    if len(chunks) != 5:
        raise ValueError("Missing approved icon sizes")
    body = b"".join(chunks)
    return b"icns"+struct.pack(">I", len(body)+8)+body


def validate_publish(publish):
    required = ["Dororong.Mac", "Dororong.Mac.dll", "Dororong.Core.dll",
                "Dororong.Portable.dll", "Dororong.Raster.dll", "Dororong.Mac.deps.json",
                "Dororong.Mac.runtimeconfig.json", "libhostfxr.dylib",
                "libcoreclr.dylib", "libAvaloniaNative.dylib", "OFL-Pretendard.txt"]
    if any(not (publish/name).is_file() for name in required):
        raise ValueError("Incomplete self-contained publish")
    config = json.loads((publish/"Dororong.Mac.runtimeconfig.json").read_text())
    if config["runtimeOptions"]["includedFrameworks"] != [{"name": "Microsoft.NETCore.App", "version": RUNTIME}]:
        raise ValueError("Unexpected bundled runtime")
    deps = json.loads((publish/"Dororong.Mac.deps.json").read_text())
    if not deps["runtimeTarget"]["name"].endswith("/osx-arm64"):
        raise ValueError("Expected arm64 dependencies")
    files = []
    for file in sorted(publish.iterdir()):
        if file.is_symlink() or not file.is_file():
            raise ValueError("Unexpected directory/link: "+file.name)
        if file.name == "Info.plist" or file.suffix == ".pdb":
            continue
        executable = file.name in ("Dororong.Mac", "createdump")
        if file.name.startswith(".") or (not executable and file.suffix not in (".dll", ".dylib", ".json", ".txt")):
            raise ValueError("Unexpected publish file: "+file.name)
        if file.suffix in (".json", ".txt") and file.name not in ("Dororong.Mac.runtimeconfig.json", "Dororong.Mac.deps.json", "OFL-Pretendard.txt"):
            raise ValueError("Unexpected data file: "+file.name)
        cpus = macho_cpus(file.read_bytes())
        if (cpus or file.suffix == ".dylib" or executable) and ARM64 not in cpus:
            raise ValueError("Missing arm64: "+file.name)
        files.append(file)
    return files


def write_zip(tree, output):
    with zipfile.ZipFile(output, "x", compression=zipfile.ZIP_DEFLATED, compresslevel=6) as archive:
        for file in sorted(tree.rglob("*")):
            if file.is_symlink():
                raise ValueError("Links are not permitted")
            rel = file.relative_to(tree).as_posix()
            isdir = file.is_dir()
            data = b"" if isdir else file.read_bytes()
            executable = not isdir and (bool(macho_cpus(data)) or file.name == "화면 경계 진단.command")
            mode = (stat.S_IFDIR | 0o755) if isdir else (stat.S_IFREG | (0o755 if executable else 0o644))
            info = zipfile.ZipInfo(rel+("/" if isdir else ""))
            info.create_system = 3
            info.external_attr = mode << 16 | (0x10 if isdir else 0)
            info.compress_type = zipfile.ZIP_DEFLATED
            archive.writestr(info, data)


def verify_zip(path, tree):
    with zipfile.ZipFile(path) as archive:
        if archive.testzip() is not None:
            raise ValueError("ZIP CRC failure")
        names = [entry.filename for entry in archive.infolist()]
        if len(set(names)) != len(names):
            raise ValueError("Duplicate entries")
        actual = set()
        for entry in archive.infolist():
            rel = PurePosixPath(entry.filename)
            if rel.is_absolute() or ".." in rel.parts or "\\" in entry.filename:
                raise ValueError("Unsafe ZIP path")
            if entry.is_dir():
                continue
            data = archive.read(entry)
            mode = entry.external_attr >> 16
            if entry.create_system != 3 or not stat.S_ISREG(mode):
                raise ValueError("Invalid ZIP platform/mode")
            if (macho_cpus(data) or rel.name == "화면 경계 진단.command") and mode & 0o111 != 0o111:
                raise ValueError("Lost execute permission")
            if data != tree.joinpath(*rel.parts).read_bytes():
                raise ValueError("ZIP payload mismatch")
            actual.add(entry.filename)
        expected = {p.relative_to(tree).as_posix() for p in tree.rglob("*") if p.is_file()}
        if actual != expected:
            raise ValueError("ZIP inventory mismatch")
        return len(actual)


def run_signer(signer, args, log):
    # Ignore user signing profiles/environment. This task only authorizes ad-hoc
    # integrity signing, not use of personal certificates or remote services.
    env = {k: v for k, v in os.environ.items() if not k.startswith("RCODESIGN_")}
    proc = subprocess.run([str(signer), "--config-file", "/dev/null", *args],
                          capture_output=True, env=env, cwd=ROOT, timeout=180)
    log.write_bytes(proc.stdout+proc.stderr)
    if proc.returncode:
        raise RuntimeError("Signing/verification failed; see "+str(log))


def code_directories(data, external=None):
    """Check ad-hoc page/special-slot hashes, NOT Apple policy/trust.

    Narrow parser for this packager's 64-bit little-endian Mach-O output; fail
    closed except SHA1/SHA256 (legacy universal slices carry both). Each slice
    and every CodeDirectory are independently checked.
    """
    if data[:4] in (b"\xca\xfe\xba\xbe", b"\xca\xfe\xba\xbf"):
        result = []
        count = struct.unpack_from(">I", data, 4)[0]
        wide = data[:4] == b"\xca\xfe\xba\xbf"
        macho_cpus(data)  # bounds
        for i in range(count):
            entry = 8+i*(32 if wide else 20)
            offset, size = struct.unpack_from(">QQ" if wide else ">II", data, entry+8)
            result.extend(code_directories(data[offset:offset+size], external))
        return result
    if data[:4] != b"\xcf\xfa\xed\xfe":
        raise ValueError("Unsupported Mach-O signature format")
    commands = struct.unpack_from("<I", data, 16)[0]
    offset, signature = 32, None
    for _ in range(commands):
        cmd, size = struct.unpack_from("<II", data, offset)
        if size < 8 or offset+size > len(data):
            raise ValueError("Invalid load command")
        if cmd == 0x1D:
            start, length = struct.unpack_from("<II", data, offset+8)
            signature = data[start:start+length]
        offset += size
    if signature is None or len(signature) < 12:
        raise ValueError("Missing code signature")
    magic, length, count = struct.unpack_from(">III", signature)
    if magic != 0xFADE0CC0 or length > len(signature):
        raise ValueError("Invalid signature container")
    blobs = {}
    for i in range(count):
        slot, at = struct.unpack_from(">II", signature, 12+i*8)
        size = struct.unpack_from(">I", signature, at+4)[0]
        if at+size > length:
            raise ValueError("Invalid signature slot")
        blobs[slot] = signature[at:at+size]
    results = []
    for slot, cd in blobs.items():
        if slot == 0 or 0x1000 <= slot <= 0x1005:
            results.append(check_directory(cd, data, blobs, external))
    if not results:
        raise ValueError("Missing CodeDirectory")
    return results


def check_directory(cd, data, blobs, external):
    magic, size, version, flags, hashes, identity, special, pages, limit = struct.unpack_from(">9I", cd)
    hash_size, hash_type, platform, page_power = struct.unpack_from("4B", cd, 36)
    digest = {1: hashlib.sha1, 2: hashlib.sha256}.get(hash_type)
    if magic != 0xFADE0C02 or size != len(cd) or not flags & 2 or digest is None or hash_size != digest().digest_size or page_power > 20:
        raise ValueError("Unsupported ad-hoc CodeDirectory")
    page_size = 1 << page_power
    if limit > len(data) or pages != (limit+page_size-1)//page_size:
        raise ValueError("Invalid code page count")
    for i in range(pages):
        expected = cd[hashes+i*hash_size:hashes+(i+1)*hash_size]
        if digest(data[i*page_size:min(limit,(i+1)*page_size)]).digest() != expected:
            raise ValueError("Code page digest mismatch")
    slots = dict(blobs)
    slots.update(external or {})
    for i in external or {}:
        if i > special or cd[hashes-i*hash_size:hashes-(i-1)*hash_size] != digest(external[i]).digest():
            raise ValueError("Required external seal missing: "+str(i))
    for i in range(1, special+1):
        expected = cd[hashes-i*hash_size:hashes-(i-1)*hash_size]
        if expected == bytes(hash_size):
            continue
        if i not in slots or digest(slots[i]).digest() != expected:
            raise ValueError("Special-slot digest mismatch: "+str(i))
    return digest(cd).digest()[:20]


def seal_managed_resources(bundle):
    # rcodesign 0.29 treats MacOS/* as nested code and omits managed DLL/JSON.
    # Explicit resource rules/seals cover them, then the main binary is re-signed
    # against this exact CodeResources. Native code remains nested cdhash entries.
    contents = bundle/"Contents"
    seal = contents/"_CodeSignature/CodeResources"
    resources = plistlib.loads(seal.read_bytes())
    for file in sorted((contents/"MacOS").iterdir()):
        data = file.read_bytes()
        if macho_cpus(data):
            continue
        rel = file.relative_to(contents).as_posix()
        resources["files2"][rel] = {"hash2": hashlib.sha256(data).digest()}
        resources["rules2"]["^"+re.escape(rel)+"$"] = {"weight": 30.0}
    seal.write_bytes(plistlib.dumps(resources))


def verify_bundle(bundle):
    contents = bundle/"Contents"
    info = (contents/"Info.plist").read_bytes()
    seal = (contents/"_CodeSignature/CodeResources").read_bytes()
    resources = plistlib.loads(seal)
    main = "MacOS/"+plistlib.loads(info)["CFBundleExecutable"]
    checked = 0
    for file in contents.rglob("*"):
        if not file.is_file():
            continue
        rel = file.relative_to(contents).as_posix()
        if rel in ("Info.plist", "PkgInfo", "_CodeSignature/CodeResources"):
            continue
        data = file.read_bytes()
        if rel == main:
            code_directories(data, {1: info, 3: seal})
            checked += 1
        elif macho_cpus(data):
            if resources["files2"][rel]["cdhash"] not in code_directories(data):
                raise ValueError("Nested code seal mismatch")
            checked += 1
        elif resources["files2"][rel]["hash2"] != hashlib.sha256(data).digest():
            raise ValueError("Resource seal mismatch: "+rel)
    return checked


def build(publish, output, signer, notices, nuget):
    files = validate_publish(publish)
    if output.exists() or output.is_symlink():
        raise ValueError("Use a fresh output directory; existing files are preserved")
    if output == publish or publish in output.parents or output in publish.parents:
        raise ValueError("Output and publish must be separate")
    output.mkdir(parents=True)
    tree = output/"package"
    bundle = tree/APP
    macos = bundle/"Contents/MacOS"
    resources = bundle/"Contents/Resources"
    macos.mkdir(parents=True)
    resources.mkdir()
    for file in files:
        shutil.copyfile(file, macos/file.name)
    info = plistlib.loads((ROOT/"src/Dororong.Mac/Packaging/Info.plist").read_bytes())
    info.update(CFBundleName="Doro Mac Test", CFBundleDisplayName="도로롱이 내 컴에?! — Mac 테스트",
                CFBundleVersion="104.11", CFBundleIconFile="Dororong.icns",
                CFBundleInfoDictionaryVersion="6.0", LSMinimumSystemVersion="13.0")
    (bundle/"Contents/Info.plist").write_bytes(plistlib.dumps(info))
    (resources/"Dororong.icns").write_bytes(ico_to_icns((ROOT/"src/Dororong.App/Assets/dororong.ico").read_bytes()))
    (bundle/"Contents/PkgInfo").write_bytes(b"APPL????")
    licenses = resources/"Licenses"
    licenses.mkdir()
    shutil.copyfile(ROOT/"LICENSE", licenses/"Dororong-code-MIT.txt")
    shutil.copyfile(ROOT/"src/Dororong.Raster/THIRD-PARTY-WPF.txt", licenses/"WPF-coverage-MIT.txt")
    shutil.copyfile(ROOT/"src/Dororong.App/Assets/Fonts/OFL-Pretendard.txt", licenses/"Pretendard-OFL.txt")
    for name in ("Avalonia-LICENSE.txt", "MicroCom-LICENSE.txt"):
        shutil.copyfile(notices/name, licenses/name)
    for package, version in (("microsoft.netcore.app.runtime.osx-arm64", RUNTIME),
                             ("skiasharp.nativeassets.macos", "2.88.9"),
                             ("harfbuzzsharp.nativeassets.macos", "8.3.1.1")):
        found = [p for p in (nuget/package/version).iterdir() if p.is_file() and ("license" in p.name.lower() or "notices" in p.name.lower())]
        if len(found) < 2:
            raise ValueError("Missing third party notices: "+package)
        for p in found:
            shutil.copyfile(p, licenses/(package+"-"+p.name))
    shutil.copyfile(ROOT/"tools/mac-build/TESTER-README.txt", tree/"먼저 읽어주세요.txt")
    shutil.copyfile(ROOT/"tools/mac-build/TESTER-README.txt", resources/"TESTER-README.txt")
    shutil.copyfile(ROOT/"tools/mac-build/boundary-diagnostic.command", tree/"화면 경계 진단.command")
    logs = output/"verification"
    logs.mkdir()
    run_signer(signer, ["sign", "--timestamp-url", "none", str(bundle)], logs/"adhoc-sign.log")
    seal_managed_resources(bundle)
    run_signer(signer, ["sign", "--timestamp-url", "none", "--binary-identifier", info["CFBundleIdentifier"],
                       "--info-plist-file", os.path.relpath(bundle/"Contents/Info.plist", ROOT),
                       "--code-resources-file", os.path.relpath(bundle/"Contents/_CodeSignature/CodeResources", ROOT),
                       str(macos/"Dororong.Mac")], logs/"main-resource-sign.log")
    checked = verify_bundle(bundle)
    run_signer(signer, ["print-signature-info", str(bundle)], logs/"signature-info.log")
    # All metadata/assets must be finalized before signing. Nothing in the bundle
    # is modified after this point. Manifest/report live outside it.
    manifest = {p.relative_to(tree).as_posix(): sha(p.read_bytes()) for p in sorted(tree.rglob("*")) if p.is_file()}
    (tree/"FILE-HASHES.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2)+"\n", encoding="utf-8")
    archive = output/"Doro-Mac-arm64-r104-test11.zip"
    write_zip(tree, archive)
    count = verify_zip(archive, tree)
    digest = sha(archive.read_bytes())
    (output/(archive.name+".sha256")).write_text(digest+"  "+archive.name+"\n", encoding="ascii")
    report = {"artifact": archive.name, "sha256": digest, "bytes": archive.stat().st_size,
              "files": count, "native_code_hashes_checked": checked, "runtime": RUNTIME,
              "verification_scope": "CodeDirectory page/special-slot hashes and SHA256 resource seals, not Apple codesign or trust policy",
              "architecture": "arm64", "tester_minimum_macos": "13.0",
              "signature": "ad-hoc integrity only; no Developer ID", "notarized": False,
              "zip_execute_bits": "verified", "mac_native_execution": "UNVERIFIED",
              "gatekeeper_download_launch": "UNVERIFIED"}
    (output/"verification.json").write_text(json.dumps(report, indent=2)+"\n", encoding="utf-8")
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ("publish", "output", "signer", "notices", "nuget"):
        parser.add_argument("--"+name, type=Path, required=True)
    args = parser.parse_args()
    build(**{k: v.resolve() for k, v in vars(args).items()})

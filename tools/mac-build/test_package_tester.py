import importlib.util
import hashlib
import json
from pathlib import Path
import plistlib
import stat
import struct
import tempfile
import unittest
import zipfile

spec = importlib.util.spec_from_file_location("pack", Path(__file__).with_name("package_tester.py"))
pack = importlib.util.module_from_spec(spec)
spec.loader.exec_module(pack)


class PackageTests(unittest.TestCase):
    def directory(self):
        data = b"code"
        info = b"info"
        hashes = 44+32
        cd = struct.pack(">9I4BI", 0xFADE0C02, hashes+32, 0x20000, 2, hashes, 0, 1, 1, len(data), 32, 2, 0, 12, 0)
        cd += hashlib.sha256(info).digest()+hashlib.sha256(data).digest()
        return cd, data, info

    def test_code_page_and_external_slot_hashes(self):
        cd, data, info = self.directory()
        self.assertEqual(pack.check_directory(cd, data, {}, {1: info}), hashlib.sha256(cd).digest()[:20])
        with self.assertRaisesRegex(ValueError, "Code page"):
            pack.check_directory(cd, b"evil", {}, {1: info})
        with self.assertRaisesRegex(ValueError, "external seal"):
            pack.check_directory(cd, data, {}, {1: b"changed"})

    def test_required_seal_cannot_be_silently_omitted(self):
        cd, data, info = self.directory()
        cd = cd[:44]+bytes(32)+cd[76:]
        with self.assertRaisesRegex(ValueError, "external seal"):
            pack.check_directory(cd, data, {}, {1: info})

    def test_icon_reuses_exact_pngs(self):
        ico = (pack.ROOT/"src/Dororong.App/Assets/dororong.ico").read_bytes()
        icns = pack.ico_to_icns(ico)
        self.assertEqual(icns[:4], b"icns")
        self.assertEqual(struct.unpack_from(">I", icns, 4)[0], len(icns))
        offset, count = 8, 0
        while offset < len(icns):
            size = struct.unpack_from(">I", icns, offset+4)[0]
            self.assertIn(icns[offset+8:offset+size], ico)
            count += 1
            offset += size
        self.assertEqual(count, 5)

    def test_icon_rejects_other_container(self):
        with self.assertRaises(ValueError):
            pack.ico_to_icns(b"badbad")

    def test_macho_cpu_and_fat_cpu(self):
        self.assertEqual(pack.macho_cpus(b"\xcf\xfa\xed\xfe"+struct.pack("<I", pack.ARM64)), [pack.ARM64])
        fat = b"\xca\xfe\xba\xbe"+struct.pack(">I", 1)+struct.pack(">IIIII", pack.ARM64, 0, 0, 0, 0)
        self.assertEqual(pack.macho_cpus(fat), [pack.ARM64])
        with self.assertRaises(ValueError):
            pack.macho_cpus(b"\xca\xfe\xba\xbe"+struct.pack(">I", 40))

    def test_zip_preserves_execute_bits_and_non_ascii(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            tree = root/"tree"
            exe = tree/pack.APP/"Contents/MacOS/Dororong.Mac"
            exe.parent.mkdir(parents=True)
            exe.write_bytes(b"\xcf\xfa\xed\xfe"+struct.pack("<I", pack.ARM64))
            (tree/"먼저 읽어주세요.txt").write_text("테스트", encoding="utf-8")
            (tree/"화면 경계 진단.command").write_text("#!/bin/bash\nexit 0\n", encoding="utf-8")
            archive = root/"test.zip"
            pack.write_zip(tree, archive)
            self.assertEqual(pack.verify_zip(archive, tree), 3)
            with zipfile.ZipFile(archive) as z:
                info = z.getinfo(pack.APP+"/Contents/MacOS/Dororong.Mac")
                self.assertEqual(info.external_attr >> 16, stat.S_IFREG | 0o755)
                self.assertEqual(z.getinfo("화면 경계 진단.command").external_attr >> 16, stat.S_IFREG | 0o755)
            with self.assertRaises(FileExistsError):
                pack.write_zip(tree, archive)

    def test_incomplete_publish_fails_without_output(self):
        with tempfile.TemporaryDirectory() as tmp:
            with self.assertRaisesRegex(ValueError, "Incomplete"):
                pack.validate_publish(Path(tmp))

    def fixture(self, root):
        names = ["Dororong.Mac", "Dororong.Mac.dll", "Dororong.Core.dll", "Dororong.Portable.dll", "Dororong.Raster.dll",
                 "libhostfxr.dylib", "libcoreclr.dylib", "libAvaloniaNative.dylib", "OFL-Pretendard.txt"]
        for name in names:
            (root/name).write_bytes(b"\xcf\xfa\xed\xfe"+struct.pack("<I", pack.ARM64) if name.endswith(".dylib") or name == "Dororong.Mac" else b"fixture")
        (root/"Dororong.Mac.runtimeconfig.json").write_text(json.dumps({"runtimeOptions": {"includedFrameworks": [{"name": "Microsoft.NETCore.App", "version": pack.RUNTIME}]}}))
        (root/"Dororong.Mac.deps.json").write_text(json.dumps({"runtimeTarget": {"name": ".NETCoreApp,Version=v8.0/osx-arm64"}}))

    def test_wrong_architecture_rejected(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            self.fixture(root)
            self.assertEqual(len(pack.validate_publish(root)), 11)
            (root/"Dororong.Mac").write_bytes(b"\xcf\xfa\xed\xfe"+struct.pack("<I", 0x01000007))
            with self.assertRaisesRegex(ValueError, "arm64"):
                pack.validate_publish(root)

    def test_private_or_unexpected_files_rejected(self):
        for name in (".env", "settings.json", "signing.p12", "helper.exe"):
            with self.subTest(name=name), tempfile.TemporaryDirectory() as tmp:
                root = Path(tmp)
                self.fixture(root)
                (root/name).write_bytes(b"never ship")
                with self.assertRaisesRegex(ValueError, "Unexpected"):
                    pack.validate_publish(root)

    def test_symbols_excluded(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            self.fixture(root)
            (root/"Dororong.Mac.pdb").write_bytes(b"debug paths")
            self.assertNotIn("Dororong.Mac.pdb", [p.name for p in pack.validate_publish(root)])

    def test_zip_tamper_rejected(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            tree = root/"tree"
            tree.mkdir()
            (tree/"payload.txt").write_bytes(b"old")
            pack.write_zip(tree, root/"test.zip")
            (tree/"payload.txt").write_bytes(b"changed")
            with self.assertRaisesRegex(ValueError, "mismatch"):
                pack.verify_zip(root/"test.zip", tree)


if __name__ == "__main__":
    unittest.main()

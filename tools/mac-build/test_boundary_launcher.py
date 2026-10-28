"""Execute the launcher with stubbed macOS tools; NOT native Mac validation."""
import os
from pathlib import Path
import shutil
import subprocess
import tempfile
import unittest

SOURCE = Path(__file__).with_name('boundary-diagnostic.command')
BASH = shutil.which('bash') or next((str(p) for p in [Path('C:/Program Files/Git/bin/bash.exe')] if p.exists()), None)

@unittest.skipUnless(BASH, 'bash unavailable')
class LauncherTests(unittest.TestCase):
    def run_case(self, location='sibling', mode='complete', version='104.10'):
        with tempfile.TemporaryDirectory(prefix='doro launcher ') as directory:
            root=Path(directory); home=root/'user home'; scripts=root/'download'; system=root/'Applications'; stubs=root/'bin'
            for p in (home,scripts,system,stubs): p.mkdir()
            def write(path,text):
                path.parent.mkdir(parents=True,exist_ok=True);path.write_text(text,encoding='utf-8',newline='\n');path.chmod(0o755)
            locations={'sibling':[scripts], 'moved':[system], 'user':[home/'Applications'], 'ambiguous':[system,home/'Applications'], 'missing':[]}
            for base in locations[location]:
                app=base/'Doro Mac Test.app'
                write(app/'Contents/MacOS/Dororong.Mac','#!/bin/bash\nexit 0\n')
                write(app/'Contents/Info.plist',version+'\n')
            write(stubs/'plist','#!/bin/bash\ncat "$3"\n')
            write(stubs/'uuid',"#!/bin/bash\necho 'f70a60ef-649a-444f-bbd3-665cf082b8fe'\n")
            write(stubs/'sleep','#!/bin/bash\nexit 0\n')
            write(stubs/'open', '''#!/bin/bash
[ "$1" = '-R' ] && exit 0
[ "$MODE" = 'open-fail' ] && exit 1
[ "$MODE" = 'no-start' ] && exit 0
log="$HOME/Library/Application Support/JOEWRKS/Dororong/Logs/boundary-f70a60ef-649a-444f-bbd3-665cf082b8fe.jsonl"
mkdir -p "$(dirname "$log")"
[ "$MODE" = 'already-running' ] && { printf '%s\n' 'boundary-error: already-running' > "${log%.jsonl}.stderr.txt"; exit 0; }
[ "$MODE" = 'writer-fail' ] && { printf '%s\n' 'boundary-error: log-write IOException' > "${log%.jsonl}.stderr.txt"; exit 0; }
printf '%s\n' '{"kind":"start"}' > "$log"
if [ "$MODE" = 'complete' ]; then printf '%s\n' '{"kind":"end","reason":"duration-complete"}' >> "$log"; fi
if [ "$MODE" = 'early' ]; then printf '%s\n' '{"kind":"end","reason":"disposed-before-deadline"}' >> "$log"; fi
''')
            text=SOURCE.read_text(encoding='utf-8').replace('/usr/libexec/PlistBuddy',str(stubs/'plist').replace('\\','/'))
            # Absolute tool paths in production; quote fixture paths with spaces.
            text=text.replace(str(stubs/'plist').replace('\\','/'),'"'+(stubs/'plist').as_posix()+'"')
            text=text.replace('/usr/bin/uuidgen','"'+(stubs/'uuid').as_posix()+'"').replace('/usr/bin/open','"'+(stubs/'open').as_posix()+'"')
            text=text.replace("'/Applications/Doro Mac Test.app'", "'"+(system/'Doro Mac Test.app').as_posix()+"'")
            text=text.replace('sleep 1','"'+(stubs/'sleep').as_posix()+'" 1')
            script=scripts/'diagnose.command';write(script,text)
            env=dict(os.environ,HOME=home.as_posix(),MODE=mode)
            return subprocess.run([BASH,'--noprofile','--norc',script.as_posix()],env=env,capture_output=True,text=True,encoding='utf-8',timeout=60)
    def test_existing_and_moved_app_report_verified_completion(self):
        for location in ('sibling','moved','user'):
            with self.subTest(location=location):
                result=self.run_case(location)
                self.assertEqual(result.returncode,0,result.stderr)
                self.assertIn('기록 시작 확인',result.stdout);self.assertIn('완료를 파일에서 확인',result.stdout)
    def test_missing_wrong_version_and_ambiguous_apps_do_not_claim_start(self):
        for args in [('missing','complete','104.10'),('sibling','complete','104.8'),('ambiguous','complete','104.10')]:
            with self.subTest(args=args):
                result=self.run_case(*args);self.assertNotEqual(result.returncode,0)
                self.assertNotIn('기록 시작 확인',result.stdout)
    def test_failed_launch_missing_log_early_exit_and_no_end_are_not_complete(self):
        for mode in ('open-fail','no-start','early','timeout','already-running','writer-fail'):
            with self.subTest(mode=mode):
                result=self.run_case(mode=mode);self.assertNotEqual(result.returncode,0)
                self.assertNotIn('완료를 파일에서 확인',result.stdout)
                self.assertTrue(result.stderr)

if __name__=='__main__': unittest.main()

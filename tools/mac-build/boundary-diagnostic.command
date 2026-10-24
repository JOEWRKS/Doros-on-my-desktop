#!/bin/bash
set -eu
SCRIPT_DIR="$(cd -- "$(dirname -- "$0")" && pwd)"
version='104.9'
valid_app() {
  [ -x "$1/Contents/MacOS/Dororong.Mac" ] &&
  [ "$(/usr/libexec/PlistBuddy -c 'Print :CFBundleVersion' "$1/Contents/Info.plist" 2>/dev/null)" = "$version" ]
}
APP=''
if valid_app "$SCRIPT_DIR/Doro Mac Test.app"; then
  APP="$SCRIPT_DIR/Doro Mac Test.app"
else
  for candidate in '/Applications/Doro Mac Test.app' "$HOME/Applications/Doro Mac Test.app"; do
    if valid_app "$candidate"; then
      if [ -n "$APP" ]; then
        printf '%s\n' '실행 중단: test09 앱이 여러 위치에 있습니다. 사용할 앱과 이 .command를 같은 폴더에 두세요.' >&2
        exit 1
      fi
      APP="$candidate"
    fi
  done
fi
if [ -z "$APP" ]; then
  printf '%s\n' '실행 실패: test09의 Doro Mac Test.app을 찾지 못했습니다. 앱과 이 .command를 같은 폴더에 두세요. 진단은 시작되지 않았습니다.' >&2
  exit 1
fi
session=$(/usr/bin/uuidgen | tr '[:upper:]' '[:lower:]')
LOG="$HOME/Library/Application Support/JOEWRKS/Dororong/Logs/boundary-$session.jsonl"
umask 077
if ! mkdir -p "$(dirname "$LOG")"; then
  printf '%s\n' '실행 중단: 로그 폴더를 만들 수 없습니다. 앱은 켜지 않았습니다.' "폴더: $(dirname "$LOG")" >&2
  exit 1
fi
OUT="${LOG%.jsonl}.stdout.txt"
ERR="${LOG%.jsonl}.stderr.txt"
if ! (set -C; : > "$OUT" && : > "$ERR"); then
  printf '%s\n' '실행 중단: 진단 출력 파일을 만들 수 없습니다. 앱은 켜지 않았습니다.' "폴더: $(dirname "$LOG")" >&2
  exit 1
fi
launch_failure() {
  if grep -q 'boundary-error:' "$ERR"; then
    cat "$ERR" >&2
    printf '%s\n' '진단 실패. 다시 실행하지 말고 아래 오류 파일을 전달하세요.' "오류 파일: $ERR" >&2
    /usr/bin/open -R "$ERR" || true
    return 0
  fi
  return 1
}
printf '%s\n' "실행할 앱: $APP" '이 커맨드가 캐릭터를 직접 켭니다. 앱을 따로 켜지 마세요.' '기존 도로롱이 실행 중이면 먼저 메뉴에서 종료해야 합니다.' '화면·창 제목·키 입력은 수집하지 않습니다. 기록 시작/완료를 이 창에서 확인합니다.'
printf '%s\n' "시작 오류 파일: $ERR"
if ! /usr/bin/open -n "$APP" --stdout "$OUT" --stderr "$ERR" --args --boundary-diagnostics "--boundary-session=$session"; then
  printf '%s\n' '앱 실행 요청 실패. 진단 시작은 확인되지 않았습니다.' >&2
  exit 1
fi
for ((i=0;i<30;i++)); do
  if launch_failure; then exit 1; fi
  [ -s "$LOG" ] && break
  sleep 1
done
if [ ! -s "$LOG" ]; then
  cat "$ERR" >&2
  printf '%s\n' '기록 시작 미확인. 같은 실행을 반복하지 말고 이 메시지와 아래 오류 파일을 전달하세요. 빈 오류 파일도 성공을 뜻하지 않습니다.' "오류 파일: $ERR" "예상 파일: $LOG" >&2
  /usr/bin/open -R "$ERR" || true
  exit 1
fi
printf '%s\n' '기록 시작 확인. 지금 위로 끌기·놓기를 해주세요. 캐릭터는 켜두세요.' "로그: $LOG"
for ((i=0;i<120;i++)); do
  if launch_failure; then exit 1; fi
  if grep -q '"kind":"end"' "$LOG"; then
    if grep -q '"reason":"duration-complete"' "$LOG"; then
      printf '%s\n' '90초 기록 완료를 파일에서 확인했습니다. 캐릭터는 계속 실행됩니다.' "보낼 파일: $LOG"
      /usr/bin/open -R "$LOG" || true
      exit 0
    fi
    printf '%s\n' '90초 전에 기록이 종료됐습니다. 완료가 아닙니다. 생성된 파일은 보존됩니다.' "파일: $LOG" >&2
    exit 1
  fi
  sleep 1
done
printf '%s\n' '기록 완료 미확인: 대기 시간 초과. 앱은 강제 종료하지 않았습니다. 같은 실행을 반복하지 말고 현재 파일/메시지를 전달하세요.' "파일: $LOG" >&2
exit 1

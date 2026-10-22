#!/bin/bash
set -eu
SCRIPT_DIR="$(cd -- "$(dirname -- "$0")" && pwd)"
printf '%s\n' '기존 도로롱은 먼저 메뉴에서 종료해 주세요.' '이 실행은 90초 동안 창 위치·화면 배치·커서 위치·창 경계 숫자만 로컬에 기록합니다.' '화면 이미지·창 제목·키 입력은 수집하지 않습니다. 앱을 일시 정지하지 않습니다.' '90초 동안 문제가 나는 화면 위쪽 드래그/창 걸치기를 해 주세요. 앱은 켜두세요.' '90초 뒤 로그: ~/Library/Application Support/JOEWRKS/Dororong/Logs/boundary-*.jsonl' '중복 실행 중이면 기록이 시작되지 않습니다. 새 로그 생성 여부를 확인해 주세요.'
open -n "$SCRIPT_DIR/Doro Mac Test.app" --args --boundary-diagnostics

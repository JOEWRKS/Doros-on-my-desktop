#!/bin/bash
# Developer assembly only. Run on a Mac; never changes Gatekeeper/signing or login items.
set -euo pipefail
if [[ $(uname -s) != Darwin ]]; then echo 'Run on macOS.' >&2; exit 2; fi
if [[ $# != 2 ]]; then echo 'Usage: bash assemble-probe.sh /absolute/published-folder /absolute/new-output-directory' >&2; exit 2; fi
publish=$1
output=$2
case "$publish:$output" in /*:/*) ;; *) echo 'Both paths must be absolute.' >&2; exit 2;; esac
if [[ ! -f "$publish/Dororong.Mac" || ! -f "$publish/Dororong.Mac.dll" || ! -f "$publish/Info.plist" ]]; then echo 'Incomplete published payload.' >&2; exit 2; fi
if [[ -e "$output" || -L "$output" ]]; then echo 'Use a new output directory; existing files are never replaced.' >&2; exit 2; fi
app="$output/Dororong Preview.app"
mkdir -p "$app/Contents/MacOS"
ditto "$publish" "$app/Contents/MacOS"
cp "$publish/Info.plist" "$app/Contents/Info.plist"
chmod u+x "$app/Contents/MacOS/Dororong.Mac"
plutil -lint "$app/Contents/Info.plist"
echo "Created developer probe: $app"
echo 'Not signed or notarized. Keep OS protections enabled. Login startup requires macOS 13+ and OS approval.'

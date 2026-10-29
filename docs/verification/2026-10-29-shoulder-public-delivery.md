# Accepted shoulder build downloads and images — 2026-10-29

User requested direct downloads for both platforms and animation images again.
Published existing exact candidates; no product rebuild, restart or installation.
Existing releases/latest designation retained. Both new releases are prereleases.

## Source / direct links

- Windows tag `windows-shoulder-20261029` resolves to
  `0951ad4fc0bcc67ac6f1a8f9b488fd684289553f`.
- Mac tag `macos-r104-test11` resolves to
  `1a5aedf5b0b511549f0598b04c6bd46ef41c98a1`.
- [Windows portable ZIP](https://github.com/JOEWRKS/Doros-on-my-desktop/releases/download/windows-shoulder-20261029/Doros-on-my-desktop-Portable-0.1.3-win-x64.zip)
- [Mac Apple Silicon test11](https://github.com/JOEWRKS/Doros-on-my-desktop/releases/download/macos-r104-test11/Doro-Mac-arm64-r104-test11.zip)
- [Latest motion images](https://github.com/JOEWRKS/Doros-on-my-desktop/releases/download/windows-shoulder-20261029/Doro-motion-images-accepted-20261029.zip)

## Verified artifacts

|Artifact|Bytes|SHA256|
|---|---:|---|
|Windows portable|99584208|`3d777a60fa34bb9f26c478dcbaee4379e0798c92fe059d5bbc8af6bdfbe54144`|
|Mac test11|97926327|`0d0ca46dcb4cd8e3475f027b6b95ad9f0e1db056fff4254ab47d30358110a24a`|
|Images ZIP|8074255|`6631200844f1d86fbbd732f7243cb3ccfbc64d40fce5a2f820bc6b47da7ce0d7`|

Both platform ZIPs, images ZIP, two standalone PNG sheets and both SHA256SUMS
downloaded anonymously and matched exact local sizes/hashes. Upload metadata
also matched. Evidence: `artifacts/release-shoulder-20261029/plan.json`,
`*-published.json`, `anonymous-verification.json`, `image-package-check.json`.
Credentials were obtained via existing Git credential helper and never saved in
these evidence files. Connected GitHub tools lacked release-asset upload support;
the previously verified API script was reused with fresh tags and pinned assets.

## Images / limits

44PNG+3GIF+README. Old overview containing superseded stretch/stamp imagery is
excluded. Two current accepted contact sheets and24keyframes copied unchanged
from the verified v5 render; unchanged seated/DORO images retained from the prior
handoff. These are rendered diagnostic previews, not real Mac screenshots.
GIF preview holds/target markers are documented. Both current contact sheets
were directly viewed again before publishing. No new artwork modification.

Windows download is portable, not an updated installer. Existing installed app,
running PID38720 and user settings unchanged. Quit old app before running the new
portable; for Mac, Finder-extract and open Doro Mac Test.app from Applications.
Existing signature/security guidance remains in the package README.
No new tests implied by publication; Mac M2/native acceptance remains NOT_TESTED
as recorded in [test11 verification](2026-10-29-mac-test11-shoulder.md).

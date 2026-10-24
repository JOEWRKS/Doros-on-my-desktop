# Second reaction reassignment / third forward slap — 2026-10-23

## Authority and scope

User likes the previous small forward stamp and explicitly moves that motion to the
second withdrawal. The new drawing asks for a leading forepaw reaching well ahead of
the face and hitting the floor twice on the third withdrawal. Drawing is a pose guide,
not authorization to replace accepted face ink or original art. User closed the pet and
authorized launching the new candidate directly after checks.

## Implementation

- First reaction retains its head shake. Second uses the prior 2.4DIP lift, 3.2→4DIP
  reach and 12→34% small bow with the same .28/.60s contacts. Its mild face remains mild.
- Third uses the leading foreleg (sole23,79), up to16DIP forward reach and4.8DIP lift;
  original furious face and small bow retained. Both contacts and .9s reaction unchanged.
- Leading-leg displacement tapers to zero at the shoulder and toward the other legs.
  Horizontal weight is non-increasing: extending it stretches the arm without folding
  a narrow interior transition. Inverse lookup samples immutable artwork once. No added
  sprite, frame array, bitmap cache or art-asset modification. Bounds include full reach.
- Other three sole anchors stay put; stationary world position, eating threshold and
  return/recovery rules unchanged. Windows and shared raster/controller code agree.
- Legacy96px preview still does not implement this high-resolution deformation. Native
  Mac appearance, actual mouse response and long-duration memory are not certified here.

## Evidence

Artifacts: `artifacts/snack-front-slap-20261023/`.

- `core.trx`:20/20; two lifts at both levels, retained second curve, third limits, reset/offer.
- `windows-first.trx`:11/11; two facings, other soles, numerical inverse grid within1e-5DIP,
  5sizes100–150%, exact return, accepted face checks and code-input real presenter loop.
- `raster.trx`:10/10,400frames across sizes/densities; retained/full pixels identical,
  output allocation count1 and same expression buffer. This is not a memory soak.
- `mac-host.trx`:5/5, Windows-hosted controller code-input checks, not native Mac.
- Saved `proof/stomp-five-sizes.png` inspected: extended leading forepaw, attachment,
  two-contact sequence and return. GIF is a saved-frame animation, not desktop capture.
- Additional second/third enlarged proof and furious interruption test initially failed
  to compile because a test stream variable shadowed `file`; renamed it. No product failure
  was hidden or tolerance relaxed. Final rerun/delivery below.

## Delivery

- `windows-final.trx`:12/12, including both mild/furious interruption recoveries.
- Opened `proof/second-third-contact.png`: second small stamp beside third extended leading
  foreleg. Connection and other feet visually intact in the saved enlarged contact render.
- `git diff --check` exit0. Separate self-contained Windows candidate packaged:
  `artifacts/product-shell/candidate-snack-front-slap-20261023/runtime/Dororong.exe`.
- Tested and candidate DLL SHA256 matched:
  App `97128C52E5F604A5E25B4B5C6FA0B2FEC76E4586F10EFBE32F2E366EE692438E`;
  Core `D2AA55AEBC1C6C94C1E54887DB647863D80616E5AD8D024B0DCA55A6949A1A15`.
  ZIP SHA256 `618974AABC698E18341D1AF17D2EB35487B2F781B68BA02240D5097A4C0C0C0E`.
- No prior pet process; launched authorized candidate2026-10-23 20:11:11 KST, PID50704.
  Same-path process alive/responding after3s. No automatic mouse input or process termination.

Actual new motion acceptance remains pending. Installation, settings, original artwork
and unrelated Mac work are untouched. No public upload or native Mac package in this turn.

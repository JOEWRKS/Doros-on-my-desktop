# Grooming armpit residual-line diagnosis — 2026-10-25

> Subsequent user rejection: the first cleanup erased the required armpit crease.
> Its earlier visual approval was incorrect. See [restoration and corrected
> validation](2026-10-25-groom-crease-restoration.md); the history below is not
> acceptance of the rejected package.

## Finding

User crop shows an unwanted spur near the raised paw attachment. Reproduced in
the grooming-feature package: old seated foreleg contour remains below the lifted
paw, not an intentional new crease or proof of a temporal compositor trail.
At the diagnosis stage, product code, art, timing, settings and running app were
not modified. The subsequent authorized correction is recorded below.

## Exact target and visual scope

- Feature package: `artifacts/product-shell/candidate-idle-activities-20261024/runtime/`.
  App SHA256 `5B2A091B6843D1FC8B22C25028761BA1CBFC7B23D16F675B007080BDDCE6264B`.
- Actual current PID75276 instead runs `C:/Users/tjdwo/Downloads/Doros-on-my-desktop-r104-win-x64/Dororong.exe`.
  App SHA256 `109FBA197CAC5FFDD795F43C25353CEEB59A4982D87450E66058D1B967965A81`,
  Core `442434058DE3FA8C95C6DA14AF399C1135E98D8F9C329B7266C8973CCBBF45A2`.
  Attempt to compile the probe against this target found no PetSnapshot.IdleActivity
  or IdleActivityKind API. It is not the grooming-feature binary. No live grooming
  claim is made for that process and it was not restarted or replaced.
- Separate WPF process rendered486 frames:150% f24..204 at60Hz both directions,
  100/125% every sixth frame over that range both directions. No injected user input.
- Visually opened150% right pages01/02/08 and left01 plus100/125% right00;
  these show the spur during raise/hold/wipe/lower (e.g.0.733–1.383s,3.067–3.317s).
  Not every generated frame or both smaller-scale mirrored sequences was visually
  inspected. Selected crops are nearest-neighbor enlarged solely for inspection.

## Confirmed pixel ownership

`HighResolutionGrabRaster.Chuckle.cs` RemovePaw uses a feathered mask ending at
authored x32; only x<=29 is fully removed once y>=70. Its torso repair blend ends
at y77. The seated original leg has ink around x30.5..31.8,y78.3..83.8, so its
boundary survives below the repaired torso. The new rounded-paw Overlay does not
cover those pixels when the paw is raised (ChuckleCover=1,GroomSweep=0,as at0.8s).

Isolated calls to the packaged RemovePaw/Overlay on actual seated-body texels
verify this: at(30.9333,78.2933), original alpha255 becomes181.213 after removal,
and remains181.213 after Overlay. At(31.36,82.9867), alpha252.439 becomes222.875
and stays unchanged after Overlay. Thus the visible branch is retained original
ink. These are source-texel ownership checks, not final screen-pixel alpha values.

## Evidence / remaining

`artifacts/groom-armpit-20261025/feature-candidate/`: crop sheets, selected full
frames, `source-line-trace.json`. Repro tool `tools/GroomArmpitProbe/` references
the exact packaged assemblies; no production build or source patch.

At the diagnosis stage correction remained unimplemented: remove only the obsolete grooming paw contour
while preserving accepted sitting body outline, rounded raised paw and chuckle.
Previous frame-bounds/endpoints tests did not assert absence of this internal
contour; their passes are not evidence that this region was clean.

## Bounded correction — 2026-10-25

User subsequently authorized cleanup only and requested raise/wipe/lower frame
review at multiple scales. The accepted geometry and speed are the baseline.

### Implementation

- `HighResolutionFacePose.cs`: opt-in `GroomingPaw`, default false.
- `DororongPresenter.IdleActivities.cs`: set the flag for grooming; all existing
  sitting/raise/wipe/lower clocks and geometry expressions remain unchanged.
- `HighResolutionGrabRaster.Chuckle.cs`: remove the obsolete seated foreleg ink
  only when that flag is set. Cleanup follows the existing cover blend and uses
  a diagonal boundary below authored y77, ending before x33 and fading by y88.
  This preserves the adjacent leg and avoids applying seated cleanup to chuckle.
- No source artwork, contour path, motion duration, Core source, Mac code,
  persistent settings, installed package or running process was edited.
  Product cleanup adds value fields/math, not image buffers or caches.

### Rejected attempts and final verification

- `fix-v1`: a broad mask passed initial numerical tests but was rejected on
  enlarged visual inspection because it opened a body/paw connection gap.
- `fix-v2`: narrowed diagonal mask looked better; the far-body test still failed
  at x33 (35/36). The final cap was tightened from32.5 to32.25, ending at33.
- Final `tests/groom-final.trx`:36 passed,0 failed/skipped, covering new residual
  source-pixel and neighboring-body checks plus related idle/chuckle regressions.
- `baseline-full` and `final`:229 frames each at60Hz over0..3.8s,100/125/150%
  and both facings, total1,374 frames. Actual WPF presenter rendered in a separate
  probe process; no desktop mouse input or running-pet control was used.
- Every final frame compared against the same PNG encode/decode baseline path.
  Changes outside the mirrored cleanup ROI x51..59,y97..114 normalized canvas
  coordinates:0. Maximum changed pixels per frame:33/57/88 at100/125/150%.
  All1,374 measured sole coordinates equal baseline; frame0/228 changes:0.
- Every distinct armpit crop was opened and visually reviewed:629 crops in36
  sheets, pages00..05 for all six size/facing groups. Unique counts:105 each,
  except125% Right104. Exact crop hashes map repeated frames to these reviewed
  representatives. Reviewed raise, wiping excursions, lowering and seated exit;
  final cleanup removes the hanging branch without the first attempt's open
  notch. The existing crossfade into/out of the original seated paw is retained.
- Full150% Right frame70 was also inspected against baseline in context.
  Enlargement is only for review, not new artwork or smoothing.
- `existing-final.json` versus the accepted feature's
  `artifacts/idle-activities-20261024/candidate-motion-hashes.json`:396/396 exact
  image hashes and sole positions unchanged, including chuckle and anger motions.

All evidence above is under `artifacts/groom-armpit-20261025/`. Probe source is
`tools/GroomArmpitProbe/`; new regression tests are `GroomArmpitTests.cs`.

### Candidate and limits

Separate self-contained Windows candidate:
`artifacts/product-shell/candidate-groom-armpit-20261025/`.
ZIP `Doros-on-my-desktop-Portable-0.1.3-win-x64.zip`,99,579,383 bytes;
SHA256 `8CEC664C50A21ACF2586D6AE84C11EBF9A24C53F6491766DD1172573C1E65EB4`.
Published App/Core hashes exactly match the binaries used by final tests/probes:

- App: `08D060B70C61B70F334C67DE43B9EF39162E3C7D65EB31D01F222D6E6B8AB52B`
- Core: `46358E2D9EC0452DA6BEE40C64BFF0A20E7EE7819D1103A1D091BBE9D18ED29A`

Core source was unchanged; rebuilt Core binary identity is reported separately
from the older baseline package. Current Downloads r104 PID75276 remained
responsive at the same path/start time. No launch/restart, installation, commit
or push was performed. Live desktop visual acceptance and nativeMac execution
remain untested. This correction does not address the separate taskbar issue.

## User-requested launch — 2026-10-25

After the user reported exiting the previous app and requested launch, verified
no Dororong/Dororong.App process remained. Candidate App SHA256 matched the tested
hash above. Started the candidate runtime/Dororong.exe normally at21:18:30.0619145
KST,PID80624; WaitForInputIdle returned true, HasExited=false, Responding=true.
This confirms process startup, not visual acceptance of grooming. No automatic
interaction, settings edit, installed-file replacement or further code change.

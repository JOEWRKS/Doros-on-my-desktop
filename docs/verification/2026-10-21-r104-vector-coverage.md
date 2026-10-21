# r104 Mac test02 — neck/drool software coverage

## Scope and result · 2026-10-21

User requested the remaining neck attachment and drool edge differences be fixed,
with accepted r104 art/quality/motion/speed and Windows unchanged, then a new
Apple Silicon test app. This supersedes the **neck/drool** open gate in the
[test01 record](2026-10-21-r104-source-and-mac-port.md#connected-r104-apple-silicon-test-candidate--2026-10-21).
Native Mac acceptance is still open, not replaced by these tests.

The tested neck/drool overlays now match the accepted r104 DLL exactly: 4,102
source-resolution cases and 350 composed frames at five sizes / two densities.
No source asset, anchor, stroke width, head transform, effect age, animation curve,
behavior speed, slider logic or Windows source was changed. No existing pet was
restarted, paused, manipulated or terminated during this work.

## Confirmed cause and fix

The old adapter delegated vector coverage and widening to Skia. WPF's software
RenderTargetBitmap instead uses 28.4 fixed-point curve flattening, an 8×8 coverage
grid, per-primitive color rounding, and tangent-based stroke widening. Those are
different algorithms even when given identical path controls and line widths.

Initial isolation used WPF-exported widened contours: implementing the reference
coverage made rectangles, triangle, drool curve and ellipse pixels exact. Widening
the same continuous paths on the portable side exposed a second discrepancy at
sharp neck turns: WPF refines the outer rail and adds inner-rail gap tiles. The
first broad run had 216 neck failures; adding those rules removed them. Drool
geometry, colors, continuous sine phase and the head matrix remain unchanged.

`src/Dororong.Raster/MilCoverage.cs` is a narrow smooth-overlay implementation,
not a general replacement WPF UI or arbitrary path-stroker. `DrawingAdapter`
uses it for the linked, unchanged neck implementation and for dynamic drool.
Non-authored eating fallback explicitly keeps the previous Skia backend; the
accepted fixed paw/eating banks are unchanged.

Algorithm adaptations are attributed to .NET Foundation and Contributors under
MIT, including the [pinned upstream source](https://github.com/dotnet/wpf/tree/65d15adece1df9a73e3d1abc11f8bbf99e9fc7a9/src/Microsoft.DotNet.Wpf/src/WpfGfx).
`THIRD-PARTY-WPF.txt` is included in the app's license directory. No platform WPF
DLL or Windows-native renderer is shipped to macOS.

## Evidence

All commands below ran on the **Windows host**, not a Mac. Artifact root:
`artifacts/macos/r104-vector-20261021/`.

| Check | Observed result | Evidence |
| --- | --- | --- |
| Hash-checked accepted DLL | SHA256 `374666338E0AA082F84B49B8EAD1E4BF0397F0CD08658BC24F336F78F882B322` | `raw-final.json`, `composed-final/summary.json` |
| Raw neck overlays | 2,099 exact; all 157 body frames × 7 gaze angles, plus 1,000 seeded random frame/angle pairs | `raw-final.json` |
| Raw drool overlays | 2,003 exact; 60Hz ages, arbitrary phase/extrema, long ages, 1,000 seeded random ages | `raw-final.json` |
| Composed neck/drool | 350 exact; 100/112.5/125/137.5/150%, densities 1× and 2× | `composed-final/summary.json` |
| Existing 70-frame regression | 65 exact; same 5 non-60Hz eating diagnostic differences remain, maximum channel error 28; exit 2, **not overall PASS** | `regression-final/summary.json`, `regression-final.log` |
| Raster tests | 46 passed, 0 failed/skipped; 9 WPF golden cases and scratch reuse added | `tests/raster-final-46.trx` |
| Mac headless tests | 33 passed, 0 failed/skipped | `tests/mac-final.trx` |
| Packager tests | 11 passed, 0 failed | `python -B -m unittest discover -s tools/mac-build -p test_package_tester.py -v` |
| Independent primitive probe | 6 exact fill/stroke cases, exit 0 | `dotnet run --project tools/MacVectorCoverage -c Release` |

Source-resolution comparison precedes the accepted sampler, so coverage errors
cannot be hidden behind the character. Composed tests then check actual sampler
output, including source masking and density/size registration. Two representative
768px saved frames (neck frame60/gaze20°, drool age0.7) were also viewed; this is
saved-frame inspection only, not native or interactive viewing. Sampled equality
is not a proof over every possible floating-point input or native backend.

The five remaining diagnostic differences use `MouthAge=.18`, not an age emitted
by the coordinator, which rounds eating age to 60Hz. The existing 103-frame eating
bank and runtime-age regression remain exact. This task does not silently change
the probe to treat the non-60Hz fallback as passing.

Reproduce:

```powershell
dotnet run --project tools/MacVectorCoverage -c Release -- ACCEPTED_DLL NEW_RAW_JSON
dotnet run --project tools/MacR104Parity -c Release -- ACCEPTED_DLL NEW_DIRECTORY vectors
dotnet run --project tools/MacR104Parity -c Release -- ACCEPTED_DLL NEW_REGRESSION_DIRECTORY
```

## Allocation and timing boundary

Scratch rows, intersections, flattened contours, tangent samples and reverse rails
are owned by each overlay and reused at their high-water capacity. There is no
cache keyed by time, gaze, frame or size. Existing 225² source float/byte buffers
remain reused; native path handles are disposed. The scratch regression repeats
the same 120-position cycle five times without retained-capacity growth.

A bounded host benchmark measured 2,000 changing neck/drool pairs after 200 warmup
pairs, including reflection overhead and without forced GC or working-set trim:

| Candidate | Allocated bytes/pair | Median | p95 | p99 | Max |
| --- | ---: | ---: | ---: | ---: | ---: |
| test01 Raster | 16,704.812 | 0.2881ms | 0.5094ms | 0.7101ms | 2.5812ms |
| published test02 Raster | 9,835.908 | 0.3634ms | 0.7800ms | 1.0526ms | 5.0254ms |

Both reused output buffer identities. Evidence: `benchmark-test01.json` and
`benchmark-published.json`. This confirms lower measured temporary managed
allocation in this probe, **not** faster rendering, leak freedom, native pacing,
or native memory acceptance. The reference-compatible calculation took longer
in this host measurement; accepted animation clocks/speeds were not changed.

## Exact test02 deliverable

`artifacts/macos/r104-test02-20261021/candidate/Doro-Mac-arm64-r104-test02.zip`

- 99,170,711 bytes, SHA256:
  `07d48b0a779b33a7216183b01867e43c1f53cdbc3a1b1586fa54aba384e5840d`.
- Self-contained osx-arm64 / .NET8.0.32; bundle version104.2; macOS13+ test prerequisite.
- 236 files, 18 native code hash checks; CodeDirectory page/special-slot hashes,
  SHA256 resources, ZIP CRC/content and executable bits verified by the packager.
- Published Raster DLL equals the tested host DLL:
  `72E3B365DAB0DDFF8C1723DD588AEA37F8D29A20516324DF156EB808910FEB4A`.
- Published Mac DLL:
  `5D9B842C85040F779C223C4DB8290636BA921133D63C704397BD27826E59AD0E`.
- Package evidence: `candidate/verification.json`, ZIP SHA256 and bundled file hashes.
- Ad-hoc integrity signature only; no Developer ID/notarization. This is not
  Apple's codesign verification, Gatekeeper acceptance or a safety guarantee.
- Prior test01 packages were preserved; no signed bundle edited after signing.
- `git diff --name-only 3813312 -- src/Dororong.App src/Dororong.Core` is empty.
  Windows accepted DLL hash above is unchanged. No installer/settings modified.
- Source changes remain local on `feature/macos-r104-arm64`; no new push/release.

## Remaining gates

**NOT TESTED:** actual Apple Silicon launch/security prompts/exit, Retina input,
windows/Dock/Spaces ordering, interactive transitions, native frame pacing and
long-running memory. No Mac is attached here. The next step is testing this exact
test02 ZIP on the user's Mac, not repeating the closed Windows memory tests.
Non-60Hz eating diagnostic fallback remains as recorded above. Full Mac parity
and user acceptance are not declared complete.

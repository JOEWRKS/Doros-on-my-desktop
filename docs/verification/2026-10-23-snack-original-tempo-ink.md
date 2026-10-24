# Second reaction original tempo / stamping ink — 2026-10-23

## User authority and cause

- Later correction: user rejects the two whole .9s clips with a midpoint reset. See
  `2026-10-23-snack-continuous-second.md`; the repetition below is historical, not accepted.

- User rejects the changed second motion: extend its duration, not its cadence.
  User reports the other new motions normal, but asks to inspect blurry stamping ink.
- The previous change multiplied the entire second timeline by1.5. That altered
  lift, descent and crouch speed; it was the assistant's misinterpretation, not approval.
- Restore the original .9s two-beat waveform and repeat it twice, total1.8s. This
  interpretation was announced. First reaction and third/fourth/fifth3/4/5 strikes,
  sixth cursor snatch, large arm geometry and accepted facial artwork are retained.

## Changes

- `SnackAngerMotion`: second uses original cycle age and two repetitions, without
  scaling time. The previous second1.35s slowdown is superseded. Other streaks unchanged.
- `HighResolutionGrabRaster.Stomp`: reconstructed ink previously used soft transformed
  silhouette coverage directly as its inner colour ramp; the stretched coverage plus
  destination sampling produced a broad grey transition. Narrow that colour ramp with
  Smooth((coverage-.12)/.76), leaving the alpha/silhouette, ink radius, colour endpoints,
  arm matrix, join, destination sampling and original art unchanged. No new buffer/cache.
- Tests pin original waveform at900 points in both cycles, repeated pixels at five sizes,
  contact/outline/other-foot invariants and retained/full-frame agreement.

## Verification

Evidence: `artifacts/snack-tempo-ink-20261023/`.

- Core final61/61; Windows final105/105, including actual WPF loop code-input sequences,
  second four beats, third/fourth/fifth counts, sixth bite/return/cancel and display frames.
- Raster10/10:900 full/retained frame comparisons across size/density cases including
  second repeat; owned frame allocation remains1 in this bounded test. Mac controller
  host5/5 on Windows. These are NOT native Mac or desktop mouse acceptance.
- Isolated archived renderer comparison1/1:1,195 frames at100/112.5/125/137.5/150%.
  All alpha bytes unchanged; first/second rendering RGB unchanged at identical poses;
  third55,341 RGB pixel instances change only within the paw reconstruction region.
  Comparison uses current Core motion for both renderers; it isolates ink, not historic
  tempo. Original tempo is independently checked against the explicit old waveform.
- All75 third-reaction saved frames opened in10 sheets (`attempt1/frames-00..09.png`);
  before/after five-size comparison, final five-size overview and actual loop contact/
  lift sheets inspected. No new gap or shoulder step observed in these saved images.
  Actual perceived sharpness remains user acceptance, not inferred from pixel equality.
- Attempt failures retained: initial exact-decimal rounding assertion failed on a
  ~5e-17 subtraction difference, corrected to explicit1e-12 tolerance; all61 then passed.
  Mixing archived same-name WPF assembly and live presenter tests in one process caused
  two XAML resource lookup failures. Archived check rerun alone passed; harness moved
  to evidence folder, clean-process105 production tests passed without runtime changes.
- No forced GC/trim, heap/trace pause, desktop automatic input, process killing,
  settings/install changes, public push or Mac packaging. Long-memory not tested here.

## Delivery

- Fresh candidate: `artifacts/product-shell/candidate-snack-tempo-ink-20261023/`.
- Publish completed and tested App/Core hashes matched:
  App `F695DE39A07CC2D4D73CA0ADD7A79BC5607B3E8F53425E399D6D09BE7ED2E558`;
  Core `C3AF63ED922615500B56B92C79AA6FD6D8514DA038D8A857F033140AEA8D6E22`;
  ZIP `B48869E9B5C368BCA9FF9594AF263F3767180F5C3E8BB826C668F71AC37456AF`.
- User answered “ㄱㄱ” to the replacement request. Confirmed no existing pet, checked
  hashes again and announced launch. PID5680 started at22:56:41KST, same-path alive/
  responding after3s. No automatic input. Startup is not animation/outline acceptance.
- Next is actual second-cadence and outline acceptance. Other accepted motions are
  preserved; this is not a new broad memory or performance campaign.

# Withdrawal escalation: longer second / 3–5 strikes / sixth snatch

## Authority and scope

- Follow-up: user reports other motions normal but rejects the second slowed cadence
  and asks for sharper stamping ink. See `2026-10-23-snack-original-tempo-ink.md`;
  the second1.5x slowdown below is historical and superseded, not accepted behavior.

- User accepts the preceding large-clean arm candidate: “그래 이정도면 인정해”.
  This closes that arm-size/outline acceptance, not unrelated native-Mac validation.
- New request: prolong second foot-stamping; third/fourth/fifth strike3/4/5 times;
  sixth jumps very quickly to the mouse jewel and catches/eats it like a dog.
- Second interpreted as1.5x duration (announced), keeping two strikes. Accepted face,
  large forepaw geometry/outline and original art retained. Only requested reaction
  timings/counts and sixth movement change. Settings/install/public release untouched.

## Implementation

- SnackAngerMotion: second .9→1.35s; third1.22s/fourth1.54s/fifth1.86s. Added strikes
  reuse the same .32s-separated stroke waveform and existing arm raster. First reaction
  remains .9s head shake. Later counts retain the accepted furious expression.
- SnackSession: sixth recognized withdrawal enters Snatching.40ms preparation and200ms
  live-target flight reuse the existing jump pose. Existing mouth opens during flight.
- Both production PetLoop paths move the rendered mouth toward the target, clamp to the
  screen and confirm consumption only when the actual rendered mouth is within8*scale.
  No additional .5s dwell for this forced bite. Successful eating resets the streak and
  ordinary gravity/landing resumes. Native Mac cursor includes the new carrying phase.
- Unreachable target times out at .65s to chasing/streak5 without false eating. Cancel,
  unavailable input and lock/resume cancel the reaction. Ordinary offers remain .5s.
- No new rendering cache or frame buffer introduced. No forced GC/trim, desktop input,
  process termination or pause diagnostics performed.

## Evidence

Root: `artifacts/snack-escalation-20261023/`.

| Final result | Scope |
| --- | --- |
| core-first.trx60/60 | Counts, durations, existing offers, snatch contact/cancel/timeout |
| windows-final.trx82/82 | Snack render regressions including five-size full sequences |
| windows-runtime-final.trx13/13 | Actual WPF loop with code inputs, capture, snatch/landing/cancel |
| repeated-paw-pixels.trx5/5 | 100–150% added-hit pixels exactly match corresponding first stroke |
| raster-final.trx10/10 | 600 full/retained frame comparisons, owned frame reuse |
| mac-host-final.trx5/5 | Mac controller on Windows through sixth bite; NOT native macOS |
| contact-proof.trx1/1 | Corrected second/third proof samples both at contact time |

- Runtime test six combinations: scale1/1.25/1.5 and left/right target. Live target moves
  once in flight; bite occurs in<.4s, height gain>100, rendered mouth contact validated,
  cursor stops carrying, landing returns to starting floor within2*scale. Sample bite
  around .2s. This is actual production-loop execution with code inputs, not real mouse
  interaction or measured desktop responsiveness.
- Runtime third-reaction capture308 frames across four configurations. Final third-hit
  and recovery sheets (`final/runtime-1.5--1/runtime-06..09.png`) opened and inspected;
  no new shoulder split observed in those saved frames. Five-size overview and earlier
  jump frames inspected; final snatch012/016/040 inspected after diagnostic marker fix.
- Snatch preview GIF is deliberately slower (20ms/frame versus16ms simulated step);
  it is not native desktop video. Diagnostic jewel marker now disappears after catch.
- Initial Mac host tests5/5 failed: fixture placed a target at x1503 outside1470px scene
  after autonomous walking. Actual mouth clamped at x1445 and correctly did not eat.
  Diagnostic failure retained. Corrected fixture to an on-screen target (80..1390),
  not runtime constraints; final5/5 passed. Separate unreachable tests verify no false bite.

## Delivery and remaining acceptance

- Candidate: `artifacts/product-shell/candidate-snack-escalation-20261023/runtime/Dororong.exe`.
- App SHA256 `E8869823DEE22E431E60AE446058F33C5C966B3C516F14DEFB5E36C479D3425F`.
- Core SHA256 `ABDB0F37DD73A4155F9D787038E7CFAFA261986550137721190172D5BE31A4BE`.
- ZIP SHA256 `FEBF8714A027E7B1E7CC202946F03253F4EF3153E23502292D12162AB8510BA9`.
- Publish succeeded and matched tested win-x64 App/Core binaries. Existing large-clean
  PID67896 still running at final check. Asked user to close; did not kill or launch a
  competing app. New candidate is packaged, NOT yet launched or visually accepted.
- Remaining: user verifies longer second,3/4/5 strikes and fast sixth catch. Actual Mac,
  native desktop timing, cross-monitor snatch and long-duration memory are NOT_TESTED.
  Shared Mac source/controller checks do not mean a new Mac test app was built here.

### Authorized launch follow-up

- User answered “ㄱㄱ” to the close/replacement request. No existing Dororong process
  at inspection and immediately before launch; App/Core hashes matched those above.
- Announced launch, started candidate PID44512 at2026-10-23T22:42:50.5461934+09:00.
  Same-path process alive/responding after3s. No desktop input or forced termination.
- Prior not-launched status is superseded; actual reaction appearance/feel acceptance
  remains pending. Installation/settings untouched.

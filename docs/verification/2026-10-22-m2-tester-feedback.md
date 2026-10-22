# M2 tester feedback — received 2026-10-22

## Source / environment

User relayed a tester report; no raw performance log, recording, executable hash
or direct agent access to the Mac. Exact test ZIP/build not specified. Do not
assign this to test05 or clear its specific popup verification gate yet.

Reported environment: M2 eight cores (4 performance +4 efficiency),16GB RAM,
OS `Tahoe 26.6.2`, internal display `1470x956`. OS string and display dimensions
are recorded verbatim, not independently verified; physical pixels vs scaled
logical desktop dimensions unspecified. Main report says approximately3 hours,
memory observation says approximately5 hours; do not turn this into one timed run.

## Positive observations (tester report only)

- CPU60–112% in one-core reporting convention; idle/no direct interaction felt
  stable and used comparatively little CPU. No per-state samples or before/after
  trace, so no quantified idle optimization or cause claim.
- Memory around200MB even after approximately5 hours; no apparent accumulation
  reported. Metric type, initial value and slope unknown; not universal leak-free.
- Animation showed no residual lines/stutter; intended motions worked.
-100–150% resizing showed no visually apparent issue.
- Walks in front of Dock; floor height did not change.
- Automatic sleep OFF persisted after exit/restart.
- Launching twice still produced one pet.

## Open failures

1. Cannot raise pet into roughly the upper fifth of internal screen. Tester plans
   another environment comparison. Exact drag action, cutoff coordinate/size and
   whether whole overlay or visible character stops are unspecified.
2. Clamshell with external monitor: pet does not mount window boundaries. External
   display topology/resolution/scaling, target app/window type, fullscreen/Spaces,
   and whether startup occurred before or after closing lid are unspecified.

Read-only source review: QuartzDesktop converts Quartz top-left to Cocoa using
main-display height; monitors refresh at80ms. R104DesktopController supplies the
first monitor to its work-area callback; platform mapping separately uses all
monitors. Window support depends on layer-zero rows and fresh/cached scene data.
These are relevant investigation paths, not confirmed causes. No permission
change, coordinate patch or user setting workaround was made.

## Next / scope

Prioritize these two display/window-boundary defects if implementation is
authorized; do not repeat accepted art/animation or generic memory soaks merely
because no universal leak proof exists. Verify exact test build and external
monitor details before making build-specific acceptance claims. Native popup
close/reopen, mixed-display and original CPU-root-cause gates are not all cleared
by this general report. No product changes or app interruption this turn.

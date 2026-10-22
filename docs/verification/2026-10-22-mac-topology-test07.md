# Mac display-transition combinations / test07 — 2026-10-22

## Scope and result

User authorized layout/density/clamshell combination testing and fixes for
reproduced defects, preserving accepted artwork/quality/motion/speed and Windows.
This follows test06's diagnostic-only investigation; it is not a native M2 run.

Confirmed and fixed: old monitor/window coordinates survived a topology change in
two successive scene caches. A delayed asynchronous result could also publish the
removed display again. These are reproducible source defects, not proof that they
explain the tester's persistent external-window failure or single-screen top cutoff.
Both reported native symptoms still require actual Mac recheck. No claim of every
possible display configuration, Mac support completion or leak-free behavior.

## Before/after evidence

All artifacts under `artifacts/macos/topology-20261022`.

- `before-cache.trx`:19/19 fail with old code. Current monitor enumeration changes
  at16ms but returned scene still contains the old monitor/windows. Includes a
  delayed background query crossing the removal of the internal display.
- `before-controller.trx`:108/108 fail. Even with a correct fresh fallback scene,
  the downstream controller's separate80ms cache still supplies the old monitor
  on the first changed frame.
- `after-cache.trx`:37/37 scoped desktop/cache cases pass.
- `after-controller.trx`:113/113 original boundary/controller cases pass.
- `matrix-expanded.trx`:128/128 pass after expanded end-to-end window checks.
- `mac-final.trx`:180/180 Mac host tests pass.
- `portable-final.trx`:98/98 Portable tests pass. After adding same-ID resolution
  and stable-cadence tests and making asynchronous completion deterministic,
  `cache-final.trx`:42/42 focused transition cases pass.
- Package unit tests11/11 pass; archive CRC/inventory/execute bits and18 native
  code/resource seals verify. ARM64 self-contained publish succeeds.

## Tested combinations and limits

- Outer scene cache: external widths1280/1920/2560; left/right/above/below,
  external-primary with internal repositioned, external-only clamshell topology.
  Failed/mismatched queries, fresh recovery, reopening internal layout, delayed
  old query completion, same-ID resolution changes and unchanged read cadence.
- Actual R104 controller: sizes100/125/150 x density1/2 x three widths x six
  replacement-layout labels =108 cases. These replace the internal screen with
  an external scene at left/right/above/below/origin; primary/clamshell both use
  origin(0,0) in this controller fixture (not two distinct native OS transitions).
  Assertions: first-frame active monitor matches new geometry and pet origin plus
  visible bounds lies within new screen range. Density changes during transition.
- 18 actual-controller scenarios: three sizes x two densities x three external
  origins, after starting on the internal panel. Code-driven head grab/release
  acquires a normal window, follows its20x35-point move, and detaches on close.
- Existing internal1470x956 upper drag cases remain passing. Quartz/Cocoa inverse
  coordinate tests include negative/above-screen points and primary heights720,
  956,1080,1440. These do not emulate or override AppKit constraints.
- All inputs are scripted controller calls/synthetic desktop scenes, not actual
  mouse input, physical lid-close, mixed-monitor Retina upload or window metadata.

## Fixes and preservation

1. Mac `SceneCache` records a bounded current topology and generation. Changed
   monitor values immediately discard old scene support. Pending old-generation
   queries remain single-flight but cannot restore obsolete coordinates. Only a
   matching fresh scene restores window support. Missing queries use current
   screen floors; existing80ms/500ms timing remains for stable layouts.
2. Mac R104 adapter invalidates its synchronous downstream scene cache at topology
   changes, before the next tick. Added a guarded synchronous invalidation method
   in the Mac-only Raster runtime extraction, not Windows' scene source.
3. No physics/animation/renderer/asset changes. No global cache clearing every
   tick; unchanged monitor values retain the existing query cadence. Additional
   state is one topology array/generation, not an ever-growing history/task list.
   This is not a new long-run memory verification claim.

`MacCpuProbe` frozen test04 host vs current host: `pixels-before.json` and
`pixels-after.json` have identical2880 pixel-hash/position/contact-geometry rows
across12 stable-layout autonomous/idle/seated,size100/150,density1/2 scenarios.
This is one preservation pair, not a CPU performance comparison (runs overlapped).
No need to repeat the previous two-hour soak for these bounded scene-cache changes.

Windows App/Core sources have no diff from this task. The separate Windows
size-label worktree's existing changes were left alone; no Windows app restarted.
Core rebuilt bytes differ from earlier package, but635 method bodies compare
identically and Core sources are unchanged. Renderer/simulation unchanged-source
claim is not a claim that entire Raster binary is identical: the scene-cache API
is in that Mac-only assembly.

## Candidate and remaining native check

Local `candidate/Doro-Mac-arm64-r104-test07.zip`,99,189,064bytes,
SHA2560A9856E687B55963863931F5342E6188896227983494417505E5C58AFED1D071.
237files, version104.7, arm64/.NET8.0.32/macOS13+. Ad-hoc integrity only, no
Developer ID/notarization/public upload. Prior test05 popup fix and opt-in test06
90second diagnostic retained; normal launch does not enable recording.

Candidate Core/Portable/Raster hashes exactly match final host-test dependencies:

- Core484F70AA75627AAF7DB296E51B20BFAF779B7D7379A252CC3DAF80C6013A6C26
- Portable33C8864794C23581A39B83EA553B7739FAAC24D3BE47820ADD1DEBDFBF30C872
- RasterA07665F24D0BDAE3260B891B4016648AF6A4A30711E3921F4515E5803EC451E1

Host/ARM64 Mac IL491 method bodies and12,930,200 embedded-resource bytes match;
this is not native execution. Actual M2 launch/Gatekeeper, top drag and external
window support after lid closure remain NOT_TESTED by the agent.

Next: launch test07 normally on the affected Mac and recheck only top drag and
ordinary window-edge placement after clamshell transition. If failure remains,
use included optional90second boundary logger; do not widen collision ranges,
remove occlusion checks or change accepted motion based on guesses.

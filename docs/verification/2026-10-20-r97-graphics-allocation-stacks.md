# r97: live allocation stacks identify the large write-combine regions

## Target and preserved scope

Same published r97 App DLL SHA256
`C90E62313A08BDB16A0A17DF32EB3D7278D2B4CE947ABD74FE1C63CFBBBA2E00`.
PID38088 started UTC2026-10-20T07:50:18.8097533Z, transparent1.5x, ordinary
user process. Only the separate bounded ETW collector was elevated with user
approval. Internal performance recorder disabled. No product/source artwork,
quality, accepted shape/motion/speed, configuration or installation change.
No forced GC, trim, heap dump or automated input.

## Recorded phases and user evidence

1. `artifacts/native-allocation-20261020-r97-startup`: direct kernel ETW from
   UTC07:50:11.7226839 through07:52:30.7546165, before app launch through about
   two minutes of runtime. ETL saved and parsed, EventsLost0. User later
   explicitly said they had not interacted: this is startup/idle evidence,
   not repeated-input verification.
2. `artifacts/native-allocation-20261020-r97-actions`: same process, ETW
   UTC07:53:26.2129149–07:55:34.7360863, EventsLost0. User confirmed they mixed
   the requested interactions rather than separating them. Exact action counts,
   completed meals and individual input timestamps are not instrumented.
   This confirms mixed user operation, not agent direct visual inspection.
3. User requested at16:57–16:58KST to repeat mixed actions for about30seconds.
   Only passive counters and VirtualQueryEx snapshots, no further heavy trace.
   Result is recorded below when available.

Each trace start/stop was announced, including possible diagnostic stalls.
These elevated trace CPU/timing observations are not ordinary-render performance
acceptance. Traces were stopped; no recurring monitor or power action.

## Address-correlated evidence, not pointer-neighborhood inference

Read-only VirtualQueryEx snapshots identify committed PAGE_WRITECOMBINE memory.
The very same allocation bases occur in this PID's VirtualMem/Alloc events with
kernel/user stacks. Microsoft matching PDBs for wpfgfx_cor3 and d3d9 resolve the
WPF functions. Source: `graphics-allocation-summary.json` and the full
`allocations.symbolized-events.jsonl` under the actions/startup folders.

| Allocation base | Idle snapshot07:52:38 UTC | After mixed phase07:55:43 UTC | Increase |
| --- | ---: | ---: | ---: |
| 0x2bdfe420000 | 4,386,816B | 10,350,592B | 5,963,776B |
| 0x2be02420000 | 14,913,536B | 33,423,360B | 18,509,824B |

Combined increase24,473,600B (~23.34MiB). This is these two regions, not total
process growth or application live-object bytes. They were unchanged between
two startup snapshots at07:51:16 and07:52:38UTC.

Their startup reservation paths contain
`CD3DDeviceManager::CreateNewDevice → d3d9 → nvd3dumx → dxgmms2/dxgkrnl`.
Their growth paths during the mixed-operation recording include:

- First region:5,242,880B requested commit through
  `CD3DDeviceLevel1::UpdateSurface → CHwBitmapColorSource::PushTheSourceBitsToVideoMemory`;
  another720,896B through `CreateRenderTargetUntracked →
  CD3DSurface::ReadIntoSysMemBuffer → PresentWithGDI`.
- Second region:6,553,600B through `CreateTexture →
  CHwBitmapColorSource::CreateTexture`, plus intermediate layer/render targets,
  blur/drop-shadow effects, glyph/gradient resources and presentation resources.
  The menu XAML does declare DropShadowEffect, consistent with these stacks;
  exact managed visual/bitmap identity cannot be recovered from this
  kernel-only stack record. Do not claim the6.55MB texture is conclusively the
  snack merely from its size.

This establishes graphics rendering/resource allocation for these particular
regions, not decoder/managed-array retention. It does **not** establish an
NVIDIA bug, memory leak, or that all retained bytes are still live resources.
Driver suballocation/pool reclamation is not fully visible as VirtualFree events.
Reserve sizes must not be counted as committed memory. Commit requests may
overlap/recommit; snapshot deltas, not summed allocation volume, are the retained
byte figures. Old r95 addresses belong to another process and are not identical
objects; this is new r97 evidence for the same class of memory.

## Current conclusion

The previously unknown large regions now have an observed originating rendering
path. No safe product defect has yet been demonstrated in that path. Do not
remove shadows, reduce source resolution, switch software rendering, reset the
GPU, or clear working sets just to lower a counter. Same-workload reuse and
long-duration retention remain separate validation requirements.

## Additional passive observation (not a new ETW trace)

User's mixed-operation report was accepted as valid without requiring separate
action ordering. A further approximately30-second mixed repeat was requested;
as of this handoff no explicit completion response has arrived for that
additional round. Do not invent its action count or precise boundaries.

Snapshots:

- UTC07:58:05: first region12,513,280B; second33,423,360B.
- UTC07:59:29: first33,091,584B; second33,554,432B; a new write-combine
  allocation base0x2be07f60000 has2,555,904B.
- UTC08:00:46: same three committed sizes.

The additional two snapshots total69,201,920B, compared with45,936,640B before
the repeat request. Thus another23,265,280B (~22.19MiB) is retained in this
memory class. The new region and these later allocations occurred **outside**
ETW capture: no originating stack attribution for the second increase. This
does not establish sustained leakage, but prevents declaring all increase
bounded/fully explained by the first observed resources.

Passive70-second recorder saved35samples (UTC07:58:20–07:59:30), same PID/start
identity checked each time, responding throughout. Private ranged260,177,920–
321,232,896B; first268,206,080B/last321,232,896B. These gross changes include
managed collection/allocation and cannot be equated with the native snapshot
delta. The two later equal snapshots only support stability at those two
instants, not continuous or hours-long stability.

Remaining: correlate the later allocations with creation stacks across a warm
repeat, separate graphics pool capacity from live resource retention, then
change only a confirmed application cause. No speculative product patch and no
leak-resolution pass. App left running; bounded recorders ended.

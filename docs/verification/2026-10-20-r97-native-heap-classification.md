# Remaining native memory — offline classification, allocation stacks pending

## Scope and preservation

User approved continuing investigation after r97 menu/image-sharing verification. Read AGENTS/TASKS/ROADMAP and prior native evidence. Current pet PID30432/start2026-10-20T07:01:38.1943424Z remained running/responding. Initial read in this turn private265940992B (~253.62MiB),below earlier measured end268.45MiB; a single later sample is not a continuous soak or no-leak proof.

No new pet dump/live attachment,auto input,forcedGC,working-set trim,restart,exit,trace,settings/install/art/motion/speed changes in this turn. All native-debugger sessions used saved dump files via `-z`,not live PID attachment. No new product patch because no new actionable application allocation cause was proven.

## Tools and evidence

Local diagnostic files under `artifacts/native-debugger-20261020/`.

Downloaded Microsoft.Debugging.Platform.DbgEng NuGet20260319.1511.0,which lacks the required native heap extension. Then fetched Microsoft's WinDbg appinstaller and bundle1.2610.1001.0,extracted x64 tool binaries without installing/registering the app. Extracted cdb10.0.29661.1004 signature valid,Microsoft Corporation. Microsoft symbols cached locally; no application dumps uploaded. No systemwide debugger/IFEO setting enabled.

First native attempt used forward slashes in symbol-cache path and lacked explicit ext load; it failed,not evidence. Retried with Windows path and explicit `winext/ext.dll`,successfully obtained native heap/address data. CDB reports missing automaticSOS and incomplete dump miscellaneous state; managed evidence remains from prior successful dotnet-dump/SOS,not this CDB startup. One heap header is unreadable/invalid in each saved heap dump: **not a diagnosed application heap corruption**,and not grounds for treating summaries as complete coverage.

Files:

- `before-heap-address.txt`:r95 pre-interaction saved dump.
- `after-heap-address.txt`:same r95 process post-three-hour dump.
- `r97-heap-address.txt`:r97 post-repetition saved dump. The two r95 addresses queried in this cross-process run are free/unrelated; these address lines are NOT evidence about r97. Only its valid own heap row is used.
- `r95-native-owner-probe.txt`:partial pointer/symbol exploration,interrupted during slow symbol resolution;not a full successful scan.
- `r95-owner-raw.txt`,`r95-owner-symbol.txt`:subsequent completed offline probes.

## New native heap result

The same readable r95 default heap `15892580000`:

| Snapshot | Committed KiB | Free KiB reported by debugger |
| --- | ---: | ---: |
| Before repeated interactions | 35,288 | 17,337 |
| After three-hour observation | 48,952 | 28,989 |
| Difference | +13,664 | +11,652 |

Thus committed heap increased13.34375MiB,while reported free space increased11.378906MiB. This identifies a significant part of **this heap's** growth as allocator free space,not all live image/object retention. Committed-minus-free difference grows2,012KiB (~1.96MiB);this is not an exact object census and includes allocator overhead. Do not add these figures to raw native pixel matches/GC sizes as independent totals.

r97 defaultheap`2bc04fe0000`:committed48,060KiB/free26,905KiB. This supports that free heap space is still material in r97,but different workload/uptime/process prevents an A/B saving claim. No HeapCompact/EmptyWorkingSet/GC was invoked to manufacture a lower figure.

## Large unknown regions narrowed, not fully attributed

Both oldr95 allocation bases`199327b0000` and`199367b0000` have MEM_PRIVATE/MEM_COMMIT with **PAGE_READWRITE|PAGE_WRITECOMBINE**,outside debugger-classified NT heaps. Prior aggregate committed growth26.125MiB and8.75MiB is unchanged. `!address` describes one contiguous region at a time; for the first base it shows2.840MiB even though other committed subregions exist. Do not replace aggregate totals with that one region.

`19928e40ab0` is inside a busy LFH allocation and points to`199327d0000`. Nearby data includes an NVIDIA `nvd3dumx` function-pointer table and also a pointer resolved to `coreclr!ILMarshaler::vftable`. This mixed structure is **not an unambiguous resource ownership proof**;old pointer bytes can remain within a reused busy block. Another pointer source is also a busy LFH block but surrounding PresentationCore pointers fall partly outside its user payload. Prior neighborhood hints must not be promoted to a confirmed owner.

Graphics/driver allocation remains a hypothesis. NVIDIA private symbols were not available from Microsoft's public symbol server. These snapshots contain no allocation-time stack history for these regions. They cannot identify which application's call caused the original growth or whether later cycles would allocate additional regions.

## Next evidence and authority boundary

`whoami /priv`:current token has no SeSystemProfilePrivilege; no elevation attempted. `wpr -status`:not recording. To capture a new allocation's originating stack, propose a bounded **VirtualAllocation** system trace,starting before the same r97's launch/first-use sequence. This needs administrator approval and a coordinated restart/repetition;it has not been started. Stored trace can include system process/module metadata,will stay local,and must be filtered to the pet during analysis. Announce immediately before collection/stop because diagnostic work can cause stalls. Do not silently enable persistent heap tracing,change renderer/GPU mode,clear caches,or update drivers.

Outcome:additional memory is partly identified as retained free NT-heap space;large write-combined allocations remain unowned. No extra application fix or full memory-resolution pass. r97 accepted source/motion remains unchanged;long-run and initial native allocation attribution remain open.

References used for diagnostic capability:
[Microsoft debugger package](https://www.nuget.org/packages/Microsoft.Debugging.Platform.DbgEng),
[official WinDbg download](https://learn.microsoft.com/en-us/windows-hardware/drivers/debugger/),
[VirtualAllocation capture and administrator requirement](https://learn.microsoft.com/en-us/windows/apps/develop/performance/disk-memory).

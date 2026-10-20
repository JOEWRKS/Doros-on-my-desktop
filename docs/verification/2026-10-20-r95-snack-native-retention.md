# r95 native retention diagnosis — snack pixels identified, full delta still open

## Scope

User asked to investigate memory, and explicitly excluded the small release hitch because they report it also exists in the original. This supersedes its previous deferred status; it is not a newly fixed or independently verified motion issue. No production implementation was requested in this follow-up.

Analyzed the saved pre-interaction and post-three-hour r95 dumps from [the soak](2026-10-19-r95-three-hour-soak.md). No live attachment, heap capture, GC, working-set trim, input, app restart, installation/settings change, or shutdown was performed. Current installed process PID15080 started 2026-10-20 14:45:59, path `C:/Users/tjdwo/AppData/Local/Programs/JOEWRKS/Dororong/Dororong.exe`, file version0.1.3.0/SHA256 `147327F2BA4F204BE651AB5D07E6145472FB9F014734DFBB77BBA6A81648B882`. It is a different execution from the measured r95 process; its sparse current counters are NOT a comparable r95 run or proof of a leak.

## New direct evidence

`src/Dororong.App/Assets/snack-jewel.png` is 1254×1254 RGBA8, noninterlaced. One decoded four-channel image contains **6,290,064 bytes**.

Offline `artifacts/memory-native-20261020/match-jewel.cjs` decompresses PNG IDAT, reverses each row filter, constructs BGRA and premultiplied BGRA variants, locates a nontransparent anchor in captured dump memory, and then compares EVERY byte across the minidump memory ranges. These are whole-image matches, not resemblance based on address/region size or a tiny sample.

| Post-soak virtual address | Format | Compared bytes | Equal bytes |
| --- | --- | ---: | ---: |
| `1993b3fe080` | BGRA | 6,290,064 | 6,290,064 |
| `1993bb3a080` | BGRA | 6,290,064 | 6,290,064 |
| `1993c141040` | premultiplied BGRA, rounded | 6,290,064 | 6,290,064 |

All three also match every nonzero byte (964,610 / 964,610 / 964,227 respectively). Pre-interaction dump has **no such match**. Raw results: `jewel-before.json`, `jewel-after.json`. Thus **18,870,192 bytes (~18.00MiB) of native pixel payload** are identified as snack art. This is not three abandoned managed BitmapImage objects, and not proof that repeated uses add three more copies every time.

## Owner and code path

Saved SOS results in `snack-owner.txt`, `snack-native-handles.txt`, `snack-cache-chain.txt`, `native-pixel-links.txt`:

- Live `SnackCursorWindow` `0158965b2ff8` → its Image → BitmapImage `0158965b3718`.
- The BitmapImage URI is exactly `.../Assets/snack-jewel.png`; `_pixelWidth/_pixelHeight=1254`, `_decodePixelWidth/_decodePixelHeight=0`, `_cacheOption=0`.
- `_finalSource` retains CachedBitmap `0158965867c0`, which retains BitmapFrameDecode `0158965b3b98` and PngBitmapDecoder `0158965b3a48`. Its bitmap sizes are also1254×1254.
- BitmapImage/CachedBitmap WIC safe handles share native handle `1992de858b8`. Reading that native object shows 1254×1254 dimensions, stride `0x1398`=5016, and pointer `1993bb3a080`, one of the full pixel matches. Associated `SafeMILHandleMemoryPressure` records6,290,064 bytes.
- The other exact pixel copies are associated with native WIC/converted-image references, but their complete ownership chains and destruction behavior are not fully attributed here.
- The broad safe-handle enumeration reports4,366 objects, but **only48 are live**. Do not misreport the unfiltered count as4,366 retained native bitmap leaks.

Source: `Controls/SnackCursorWindow.cs` constructs a32×32-DIP window but loads BitmapImage at source resolution without DecodePixel dimensions. Its inactive path calls `Hide`, preserving the window/image. `Controls/HighResolutionSnackEffects.cs` also keeps a frozen static Jewel loaded at full source resolution for a small chewing overlay. `Controls/DororongPresenter.Snack.cs` has a corresponding legacy loader. URI caching can share objects; three call sites must NOT be equated with the three native payloads.

**Diagnosis:** first-use full-resolution snack decoding/caching and conversion causes a significant native retention step. The code sizes the display without bounding decoded source resolution, and retains the resulting image chain while the cursor window is hidden. This explains an identified portion of memory remaining after snack interactions. It does not establish ongoing unbounded leakage.

## Remaining attribution and limits

The original private-byte rise was59.75MiB. Two other large native allocation-base commit increases total34.875MiB (26.125+8.75). Sparse neighboring references include graphics modules, but are clues, not owner/call-stack proof. Do not label all remaining memory as GPU cache. Managed-live delta2.06MiB, GC commit delta4.96MiB, image payloads, and native allocation changes are different measures; do not add/subtract them as independent accounts without checking overlap.

`inspect-dump.cjs` records region byte fingerprints and bounded nearby pointer clues. Initial execution assumed Memory64List and failed explicitly; support for the actual MemoryList stream was added before successful analysis. Regions can have uncommitted gaps, so reference membership uses actual committed ranges, not base+sum(committed). Whole-pixel matching is independent of these heuristic pointer scans.

No new actual runtime or visual regression test was performed. Previous latter-two-hour plateau is still valid, and full initial native attribution remains open. No product fix or memory-saving amount has been verified.

## Proposed next implementation, not applied

Make the snack image loading policy explicit and shared. Bound decoded resolution to the required cursor/effect output across supported DPI/scales; avoid retaining the original decoder chain unnecessarily. Preserve the on-disk source and accepted character motion/appearance/speed. Any decode-size/cache-policy change must first be compared with the existing small displayed images at each supported scale; it is not automatically pixel-identical. Re-measure first use and repeated snack cycles to verify retained-memory reduction and absence of repeat accumulation. Investigate remaining native growth separately if it persists.

Only diagnostic scripts/output and verification ledgers changed during this diagnosis. No power action or background monitor was restarted.

Reference consulted for offline-debugger capability only: [Microsoft OpenDumpFile API](https://learn.microsoft.com/en-us/windows-hardware/drivers/ddi/dbgeng/nf-dbgeng-idebugclient-opendumpfile). No native debugger attachment was performed.

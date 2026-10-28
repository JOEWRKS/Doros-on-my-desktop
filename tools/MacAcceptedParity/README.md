# Accepted Windows / Mac test10 comparison

The Windows probe references the immutable accepted package, not a rebuilt App.
The Mac probe references the current Raster project. Separate processes produce
4,848 active-buffer SHA256 records: 100/125/150%, both facings, every60Hz seated,
groom/stretch/beg/question frame and sparse existing anger/chuckle samples.
Retina density2 checkpoints supplement the density1 sequences. This is managed
Windows-host arithmetic verification, not native Apple Silicon rendering.

From the repository root:

```
dotnet build tools/MacAcceptedParity/Probe.csproj -c Release -p:Baseline=true -p:ProductRoot=ABSOLUTE_ACCEPTED_RUNTIME -o FRESH_BASELINE_PROBE
dotnet FRESH_BASELINE_PROBE/Dororong.App.Tests.dll FRESH_BASELINE_OUTPUT
dotnet build tools/MacAcceptedParity/Probe.csproj -c Release -o FRESH_MAC_PROBE
dotnet FRESH_MAC_PROBE/Dororong.Raster.Tests.dll FRESH_MAC_OUTPUT
```

Compare complete `frames.json` records, including identity/count, not just a subset.
Selected raw BGRA images are review aids; they are not all-frame visual inspection.
The Windows exporter also saves the accepted DORO? glyph outline for the retained
Avalonia vector drawing. The bubble's native composition remains a Mac check.

`dotnet FRESH_MAC_PROBE/Dororong.Raster.Tests.dll FRESH_MEMORY_DIR memory` runs a
bounded300wall-second renderer-only test:60seconds warmup, repeated size/density/
pose changes to240seconds,60seconds recovery. Samples stream to disk; no forced
GC, trim, native input, desktop app launch or heap dump. Examine sample gaps,
first-versus-late repeated windows, natural-GC counters and ownership capacities.
Counters for unexercised sole/spread/native upload paths are not evidence of pass.
`completion.json` must exist and say completed. No automatic leak verdict is made.

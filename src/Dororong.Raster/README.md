# r104 portable renderer and headless interaction coordinator

This `net8.0` library compiles the accepted Windows CPU pixel sampler and source
banks directly. Windows does not reference it. Internal geometry and Skia drawing
adapters isolate the few WPF-only overlays; they are not an emulation of WPF UI.

`R104Raster.Render` is synchronous, single-caller and accepts optional caller-owned
high-water storage. Use only `Extent(size, displayScale)^2 * 4` bytes. Product size
(1–1.5) and native pixel density are separate so Retina does not thicken the body
ink or change motion speed.

Run the portable tests:

```
dotnet test tests/Dororong.Raster.Tests -c Release
```

On Windows, compare against the exact accepted r104 DLL:

```
dotnet run --project tools/MacR104Parity -c Release -- ACCEPTED_DLL NEW_OUTPUT
```

The probe verifies the accepted SHA256 and reports source-pixel mismatches
separately from vector-backend coverage differences. Exit 1 means source-pixel
mismatches; exit 2 means a vector coverage gate remains open. Even exit zero is
only the sampled renderer comparison, never all-frame or actual Mac acceptance.

`RuntimePort` contains Mac-only extractions of accepted PetLoop and high-resolution
presentation methods. Input controllers, platform/perch engines and behavior
clocks compile from unchanged source. Native WPF factories and display services
are replaced with Mac host adapters, not used at runtime. DesktopSession now
connects this coordinator to the fixed-capacity surface and continuous slider.

Fixed paw textures and the 103 possible 60Hz eating-effect frames are exported
from the hash-checked accepted DLL by `tools/MacR104FixedEffects`. Only occupied
eating rectangles are stored; no resize or new artwork is involved. The two
compressed banks together are under 9KB. Neck/drool remain dynamic paths with
the original geometry; their edge coverage differs between the raster backends.
Non-60Hz eating ages (not emitted by the coordinator) use the dynamic fallback.

Runtime geometry uses the same alpha-128 silhouette and lower 2-DIP support band.
Retina density changes refresh the raster before any geometry read; hit/display
mapping uses the active extent, including fractional-size ceil rounding. One
high-water display buffer and 24 bounded sole results are retained. No unbounded
size cache, forced GC or native window resizing is introduced.

Actual Apple Silicon launch, native interaction, Retina/window behavior, rendering
latency and long-running memory are NOT validated by these Windows-hosted tests.
See `docs/verification/2026-10-21-r104-source-and-mac-port.md` for the exact bundle.

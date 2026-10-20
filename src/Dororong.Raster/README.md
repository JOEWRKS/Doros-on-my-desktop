# r104 portable renderer foundation — not the Mac product

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

Still required: vector difference review/correction, accepted state/input and
landing orchestration, Mac host/continuous-slider integration, arm64 application
packaging, and actual Mac execution. The older `Dororong.Mac` still uses its old
renderer; do not call that application an r104 port.

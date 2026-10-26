# Groom armpit frame probe

Windows WPF offscreen renderer; does not start/control the desktop pet.
References product DLLs from `ProductRoot`. Use a fresh process for each build.

```powershell
dotnet run --project tools/GroomArmpitProbe/Probe.csproj -c Release -p:ProductRoot=D:/JOEWRKS/DororongDesktopPet/artifacts/product-shell/candidate-idle-activities-20261024/runtime -- artifacts/groom-armpit-20261025/baseline-full
dotnet run --project tools/GroomArmpitProbe/Probe.csproj -c Release -p:ProductRoot=D:/JOEWRKS/DororongDesktopPet/tests/Dororong.App.Tests/bin/Release/net8.0-windows/win-x64 -- artifacts/groom-armpit-20261025/final artifacts/groom-armpit-20261025/baseline-full
```

Generates229 frames per size/facing at60Hz, raw PNGs and chronological local crop
contact sheets with no duplicate-frame omission. `frames.json` retains every
frame and its crop hash. Comparison
allows only the diagnosed mirrored armpit ROI and throws on changes elsewhere.
Compare sole coordinates/endpoints and review the sheets separately; the probe
does not make an automated aesthetic judgment. The optional `trace` mode is the
original diagnostic without the grooming cleanup flag, not proof of the fix.
An optional third argument `audit` compares existing raw PNGs without rerendering:
it rejects changes to the protected upper armpit (mirrored canvas x51..60,y97..105)
and records the bounds of every changed frame in `crease-audit.json`.

See `docs/verification/2026-10-25-groom-armpit-line.md` for exact evidence/limits.

For bounded diagnosis, `GROOM_PROBE_FRAMES=24,48,70` selects only those frames;
unset it for the required full229-frame run. Partial-run sheets do not use the
normal20-consecutive-frame page ranges. `junction-trace` as the second argument
records actual seated source texels before removal and after removal/overlay at
maximum wipe, using the target binary. It creates no desktop window or input.
The earlier `audit` mode protects the prior upper-crease pixels exactly; it is
not the completion criterion for the later opacity-restoration correction, which
intentionally restores those pixels. See the2026-10-25 grooming-junction report.

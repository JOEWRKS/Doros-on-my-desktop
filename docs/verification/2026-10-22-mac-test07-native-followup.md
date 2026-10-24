# test07 native feedback follow-up — 2026-10-22

Later evidence: user supplied two native logs. The missing-native-evidence status
below is superseded by [the analysis and guidance incident report](2026-10-22-mac-boundary-analysis-and-guidance-incident.md).
Native requested/actual placement mismatch is now established; no repair yet.

User reports the internal-screen upper fifth still blocks upward movement and
falling stutters. External/clamshell was not tested. This is relayed native
feedback, not agent observation. No timing trace, boundary file, size, or precise
stutter phase was supplied. Do not label stutter a proven test07 regression or
the same previously accepted Windows release hitch.

The earlier stale-topology cache fixes remain evidenced host defects, but did
not resolve the reported upper-screen symptom. Native repair acceptance FAILED
for that symptom; external behavior remains NOT_TESTED.

Read-only follow-up inspected DesktopSession.Tick, R104DesktopController.Advance,
QuartzDesktop.Move/MoveIfChanged/PlacementEvidence, and BoundaryDiagnostic.
Normal placement sends setFrameTopLeftPoint and caches requested Cocoa position;
only opt-in diagnostics read the actual NSWindow frame. Therefore native clamp
versus model restriction remains unconfirmed. Tick performs controller/render,
pixel upload and native placement sequentially; no per-stage timing evidence
identifies which causes falling stutter.

No source, timing, motion, render, Windows, package or release changes. No new
synthetic matrix or memory soak: these cannot supply missing native evidence.

Next: use test07's existing bounded 90-second boundary launcher on the internal
screen, try upward drag and release, then send only the new boundary JSONL.
It records geometry at 500ms cadence, not a frame-time profile, and must not be
used to claim short falling stutters are diagnosed or cleared. Clarify whether
the hitch occurs just after release, throughout descent, or at ground contact.
If stage timing is needed after geometry analysis, request a scoped diagnostic
change instead of repeating the same geometry recording. No external test needed
to investigate this internal-screen failure. Native launcher itself remains
unverified by the agent; report an execution error rather than bypass security.

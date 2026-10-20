# r68 native motion audit — capture blocked

User requested actual-execution inspection of hover departure, grab/tilted
release/landing, and perch transitions, preserving the accepted r67 appearance.
After confirming the previous app exit, launched the SAME r67 Release executable
at1.5x with `--interaction-test-window` and an explicit local performance log.
Diagnostic PID at launch:76060. No source code, artwork, settings or installer
changes. The opaque diagnostic host is not equivalent to normal transparent UI.

Native window enumeration returned `Dororong interaction test`, id396970.
Screenshot capture failed with `FrameArrived timed out: timed out waiting on
channel`. The user requested a pause because they were doing other work; stopped
the pending inspection and sent no pointer input. The user subsequently asked
to proceed on the current monitor. Interpreted this as the monitor containing
Dororong, with no other-monitor window interaction. Re-enumerated and selected
the same returned window; one screenshot recovery attempt failed identically.

No hover, drag, release, landing or perch gesture was performed by the agent.
No visual/native-input PASS is recorded. Process/window discovery is not visual
inspection, and a60-second callback log alone cannot certify these gestures or
display FPS. The diagnostic instance has not been deliberately closed or moved;
normal transparent restoration remains pending. Need working native capture or
user-supplied actual-operation recording to inspect the requested transitions.

## Subsequent user report and restoration

User replied "문제없", then "다음". Record no issue reported in the user's
checked operations; exact scenarios, durations and scale coverage were not
enumerated. This does not establish agent-performed visual/native input PASS.
No Dororong process remained at the next check. Relaunched the unchanged accepted
r67 executable in normal transparent1.5x mode, without diagnostic-host flag.
Next remaining checks: ordinary motions, sleep/wake, snacks/body click and grabs
from those states; scale variation and sustained performance remain open.

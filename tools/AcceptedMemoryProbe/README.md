# Accepted-build memory probe

This helper loads the exact accepted Windows product DLLs. It does not rebuild
or modify the product. The expected App SHA is pinned in Program.cs.

- Isolated transparent WPF HWND using real presenter, size menu and display path.
- Snapshot/pointer inputs are generated in this helper, not injected into Windows.
- Does not claim full PetLoop, real user clicks, real Mac, or all behaviors covered.
- Samples every5wall seconds: process identity, UTC/monotonic time, loop gaps,
  private/managed/last-natural-GC values, GUI/handle/thread counts, bounded buffer
  capacities and VirtualQuery write-combine regions. Those regions are not VRAM.
- Streams samples/events; does not retain a growing frame list or screenshot log.
- No forced GC, trim, heap dump, debugger attach, settings writes or live-pet control.
- Stops only its own diagnostic window after duration or failure. Existing evidence
  is never overwritten. Closing the diagnostic window reports an incomplete run.

Build once against the fixed candidate, then leave frozen-probe unchanged while
it is running. `Program.cs output-directory duration-seconds recovery-seconds`
uses the final recovery interval for idle display without scripted interaction.
The execution command is the generated `Dororong.App.Tests.exe`, not the source.

Current qualified run:150seconds/15recovery; measured run900seconds/180recovery.
Warmup is the first180seconds in analyze.py. Later windows are180–360,360–540,
540–720 and720–900. Do not compare the cold-start minimum with the final maximum
and call that a leak. Total allocated bytes measure activity, not ownership.

`analyze.py artifacts/accepted-memory-20261026` reads both actual (component)
and live (separate read-only observer) records, and writes summary.json. Review
normal completion, matching identities, required observed paths, timestamps and
all retention metrics before concluding. IN_PROGRESS is never a stability pass.

The UI replay covers seated tap/shake, grooming, stretch, begging, anger/chuckle/
eating/sleep and continuous100–150percent resizing with a real popup. Triggering
the six-withdrawal sequence, real mouse input and the whole desktop platform loop
are not exercised by direct snapshot injection. Existing interaction tests remain
separate evidence. Buffer capacity fields may alias the displayed/cache buffer;
do not sum them as a unique allocation census.

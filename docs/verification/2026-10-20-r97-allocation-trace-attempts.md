# r97 allocation tracing — collection path verified, attribution incomplete

## Scope and consent

User approved an elevated, approximately two-minute allocation recording around
the same r97 launch, including local system process/module metadata. No production
code, artwork, quality, accepted motion/speed, settings or installation changed.
No forced GC, working-set trim, permanent heap tracing or automatic input.
Start/stop and possible diagnostic stalls were announced.

## WPR attempts

- First folder: `artifacts/native-allocation-20261020-r97`.
  PID40300 started UTC07:36:51.7907253. Last responding sample at105.23s.
  A pending exit question confused the user into closing the newly launched app.
  User explicitly confirmed no repeated interactions. This is user termination,
  not a crash or a complete repetition test. No final ETL/completion record.
- Separate retry: `artifacts/native-allocation-20261020-r97-retry`.
  PID31300 started UTC07:41:20.0037797; App DLL SHA256
  `C90E62313A08BDB16A0A17DF32EB3D7278D2B4CE947ABD74FE1C63CFBBBA2E00`.
  Same transparent1.5x executable, internal performance recorder off.
  External observer completed62samples through123.54s. Last private260,263,936B.
  Startup sample is before runtime initialization, not an established baseline.
  No reliable input count or repeated-meal completion claim; user has not
  confirmed the retry actions.
- Retry stop returned `-2147417850 / 0x80010106`,
  `Cannot change thread mode after it is set`, profile RunningProfile.
  Final ETL absent. Thus ordinary counters survived but originating allocation
  stacks did not. Do not declare app memory fixed or performance passed.
- Five-second WPR preflight with `-skipPdbGen` failed with the same error.
  Exact internal cause remains unproven. Earlier commentary identifying symbol
  generation as the cause was too definite and was corrected in chat.
  [Microsoft's diagnostic script](https://github.com/microsoft/MSO-Scripts/blob/main/src/INCLUDE.ps1)
  documents this error as known during NGEN/embedded PDB generation; this is a
  possible mechanism, not a verified diagnosis of this machine.
- WPR subsequently reports not recording. No OS restart, installation/registry
  change, or app termination was performed to work around this.

## Direct ETW collection preflight

Diagnostic-only analyzer/collector:
`artifacts/native-allocation-20261020-r97/analyzer/Program.cs`.
Uses installed Microsoft.Diagnostics.Tracing.TraceEvent3.1.23, a unique named
session, Process/Thread/ImageLoad/VirtualAlloc kernel events and VirtualAlloc
stack capture. Bounded duration, own-session disposal, raw local ETL; no WPR
merge/symbol-generation step. Build succeeded with0warnings/0errors.

Evidence folder `artifacts/native-allocation-20261020-r97-direct-preflight`.
Collector begin UTC07:46:57.1344217; completion07:47:05.3994312 (five-second
capture plus initialization/stop). Saved20,840,448B ETL and successfully parsed
offline. PID31300:4VirtualMem/Alloc events with stacks,4VirtualMem/Free events
without stacks. TraceLog EventsLost=0. Modules and native addresses resolve;
managed generated-code names remain unresolved in this kernel-only recording.
The stack's incidental driver/module presence is not ownership/leak proof.

This verifies the collection path only. It begins after startup and does not
attribute the older growing write-combine regions or validate repeated-input
memory retention. It is not a long-run pass. No product patch based on conjecture.

## Handoff

At16:47:45KST PID31300 still responding; WPR not recording and no matching
DororongAllocation session listed. App remains running. Next requires a clearly
coordinated user tray exit/readiness, then the proven direct collector before
the same r97 launch and explicit user repetition confirmation. Do not leave an
exit question pending while launching the replacement. Stop alert immediately
before ending capture. Initial allocation ownership and long-run retention
remain incomplete.

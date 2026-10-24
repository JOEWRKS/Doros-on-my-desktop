# Three approved motion concepts — 2026-10-24

User chose suggestions3/5/6: wake stretch, occasionally ask for another jewel,
and seated idle grooming. This authorizes implementation, not visual acceptance.

## Local implementation decisions

- Stretch: after waking, wait for the initiating click/carry to finish, then a
  2.8-second forward forepaw stretch / rump lift / small shake. Direct interaction
  can interrupt. Do not change the existing sleep entry or180-second threshold.
- Beg: first and alternate successfully completed meals, after all existing
  eating/happy/settle or sixth-snatch chuckle.2.4seconds, lip-smack then slightly
  raised forepaw and gaze toward cursor. Cancellation/withdrawal is not a meal.
  This deterministic occasional cadence does not consume the walking RNG.
- Groom: after45seconds of unoccupied autonomous time, then90seconds between
  grooming opportunities, wait for Idle; sit, raise once, wipe three times, lower
  once and stand.3.8seconds. No hunger/status/persistence/rewards added.
- Existing60-second DORO wander/question and180-second sleep take precedence;
  new autonomous activities do not reset or extend their inactivity clock.
- Grab/cheek/snack/menu sit/airborne platform ownership has priority. Keep original
  art banks, accepted question/anger/stomp/snatch/chuckle and saved preferences.
- Windows high-resolution host opts in. Other hosts remain opt-out; no nativeMac
  feature or execution claim. Separate candidate, no installer/release replacement.

## Verification boundary

Core state transitions, production WPF frame sequences at100/125/150% both ways,
host platform anchoring and bounded retained buffers are checked separately.
Headless generated input is not physical user operation or live desktop acceptance.
User visual acceptance remains pending until review of the candidate.

No JOEFLOW state migration: this established repository has no product-definition
state.json; existing TASKS/ROADMAP/spec remain the handoff records. No V2 exact
manifest approval/SemanticClosure is claimed by this implementation record.

# Windows size label investigation — 2026-10-22

User clarified that the clipped-looking `크기` label is on Windows, not a Mac
error dialog. Diagnosis only; no product change/rebuild/restart authorized here.

## Reproduction and cause

- Existing r101 `proof/character-96.png` and `character-192.png` already show
  the malformed-looking first syllable. Earlier menu review missed this detail.
- Standalone WPF diagnostic references the accepted r104 App DLL at
  `artifacts/size-storage-20261020-r104/release/Dororong.App.dll`.
  Output: `artifacts/size-label-audit-20261022/{menu,variants}.png`.
  It instantiates the shared size item without launching the pet, native popup,
  registry writes, synthetic input or attaching to a running application.
- Menu312 DIP; item258x76; header236x64; title236x14. Title Clip and layout clip
  are null. Its style is null; effective font is bundled Pretendard, Normal12,
  Display formatting. The implicit MenuItem style's SemiBold does not apply to
  the derived PetSizeMenuItem. Font family is inherited from the parent menu.
- Standalone Normal12/Display text reproduces the lost/merged-looking lower
  stroke of `크`, so the screenshot's glyph defect does not require menu clipping.
  Same font/size/weight with Ideal formatting preserves the stroke; 14px,
  SemiBold12 and Malgun Gothic12 comparisons also look distinct from the defect.
  This identifies the small-text formatting/style combination, not an exception
  or missing slider, as the reproduced problem.

## Limits / next

No product fix or release was made. A focused size-label text formatting/style
correction is the proposed next action if authorized. Validate at normal and
scaled DPI; retain menu width, slider behavior and all pet rendering unchanged.
User supplied a small crop, so full-menu clipping elsewhere is not ruled out.
Actual user popup, monitor DPI and tray-versus-character entry remain unknown.
Diagnostic PNG output is not an actual desktop popup acceptance test.

## Authorized correction follow-up

User then authorized the title-only correction and a scale-checked Windows build.
Implemented in the existing Windows release worktree, not in this Mac branch:
`D:/JOEWRKS/.worktrees/DororongDesktopPet-windows-overlay-release`.
Its same-named verification document owns full evidence and package identity.
Local Ideal formatting only;19 scoped tests passed,20 rendered menu/DPI/state
comparisons have zero out-of-title changes. Five-scale title crops inspected.
Candidate ZIP470 entries checked. Existing PID30148 left running; actual popup,
mixed-monitor transitions and user acceptance remain unverified. No public upload.

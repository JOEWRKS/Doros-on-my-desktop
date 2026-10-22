# Mac missing slider diagnosis — 2026-10-22

User screenshot shows character-menu size label100% but no track/thumb. This
request asks why; no Mac product fix, packaging, restart or Windows change here.

## Evidence

- PetMenuPanel creates a real Slider; FluentTheme is loaded by ProbeApp.Initialize
  and its DLL exists in the test04 publish/package. Resize callback is supplied
  by DesktopPreviewWindow. Existing menu tests embed the panel in a Window;
  popup test checks opening/dismissal, not slider visuals or template.
- PetMenuHost owns a standalone Popup. Open sets PlacementTarget and Child but
  does not connect the popup's logical parent to the target. Control styles/theme
  inheritance are missing on that path in the headless host.
- Added diagnostic contrasts direct Window vs actual PetMenuHost in Light/Dark.
  Direct Window both: slider236x50, valid template/Track/thumb. Dark mode alone
  does not reproduce absence. PetMenuHost both: absent theme/template and zero
  slider bounds. Diagnostic's whole panel also has zero desired height in this
  headless path, so this is NOT a pixel-exact reproduction of the native screenshot.
- In test fixture only, ISetLogicalParent.SetParent(target) on the same Popup
  before Open restores inherited theme and slider236x50 with20x20 thumb in both
  modes. No product patch needed to run this controlled causal comparison.
- `artifacts/macos/size-slider-visibility-20261022/mac-slider-parent-diagnostic.trx`:
  4 passed /2 failed: original popup fails both, direct and test-attached cases
  pass. This is defect evidence, NOT a passing product suite. A subsequent image
  capture encountered zero-size bitmap errors in the already-failing cases;
  diagnostic capture now guards zero height. Native rendering was not tested.

## Conclusion / boundary

Confirmed host-level missing logical/style-parent connection on the character
popup path; it explains loss of the theme-dependent slider while manually styled
menu buttons do not rely on Fluent templates. Native screenshot is consistent,
but exact macOS rendering and actual executable version were not instrumented.
Recommended scoped repair is attach/detach popup ownership safely, then verify
real popup slider rendering/input, reopen/close/lifetime and tray route. Do not
claim the Mac defect fixed until implemented and checked. No change to character
art/motion/renderer/size behavior is needed by this diagnosis.

## Authorized implementation follow-up

User subsequently authorized the scoped repair and test app. Implemented and
host-verified in [test05 report](2026-10-22-mac-popup-test05.md). That supersedes
the no-product-change status above for the new candidate only. Original failing
diagnostic is retained as cause evidence; actual Mac acceptance remains open.

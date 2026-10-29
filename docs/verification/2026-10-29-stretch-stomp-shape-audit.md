# Stretch / stomp shape feedback — 2026-10-29

User questioned the delivered overview's stretch image and missing-looking stomp
shoulder. This is a diagnosis of the current artifact, not a corrected candidate.

## Identity and reproduction

- Image1 is `Stretch-60`, one second into the2.8second wake stretch. Its maximum
  stretch envelope holds from0.75 to1.35seconds; this is not a single-frame glitch.
- Image2 is `Angry5-60`, one second into the fifth jewel-withdrawal reaction.
  At that instant reach16/lift0/crouch0.12; it is the lowered, extended-arm pose.
- Exact immutable accepted Windows App SHA256
  `B09899C12C1844B9F08F5EC4C67A3B189C33D2ADC01D65D02E8C55517E306DE0`
  was loaded into an isolated probe, producing12 key phases for each motion.
  Both24-panel outputs were visually inspected. Not an every-frame/new-size test.
- Existing Windows/Mac parity records were re-read for the exact150% source-facing
  frames. Stretch hash `749951E7882F1DECDB2901CDA9A1D0A6A043C2AD0433B4243634D82C90CD99DB`;
  Angry5 hash `866E0BDD7F8C1408276B3CA6D2753D58B0E55D3ADFD82A683B5F6628692600FE`.
  Both match between baseline and final-parity. The visual issues therefore already
  exist in the Windows reference; parity does not establish visual correctness.

## Confirmed rendering paths and visual findings

1. `HighResolutionGrabRaster.Stretch.cs` applies a broad position-based body warp:
   x displacement includes `16*paw`, where paw uses y68–78 and x44–62 falloffs.
   This affects the whole front/lower body rather than an independently drawn limb.
   The front foot becomes a small forward stub and the neighboring lower contour
   forms a broad flattened lobe. The inverse solver's earlier numeric correction
   prevents repeated samples but does not solve this silhouette/design issue.
2. `HighResolutionGrabRaster.Stomp.cs` removes the original foreleg region with
   ArmMask, transforms the limb, adds ShoulderAlpha, then re-outlines UnionAlpha.
   JointDistance replaces the lower shoulder contour up to x39. Original internal
   ink is intentionally removed during this process. The rendered arm is attached
   to the body, but shoulder volume/separation is lost under the overlaid head,
   giving the user-reported missing-shoulder appearance. At full reach the arm
   pivot(29,74) is translated to(30,69.3); the repair is defined in that displaced
   geometry rather than preserving the original shoulder silhouette.

These are current visual defects/feedback, not a native Mac-only regression or a
wrong preview file. Previous representative review and numeric checks missed them.
Do not describe the overview as visually defect-free because its hashes match.

## Scope and remaining work

Evidence: `artifacts/motion-shape-audit-20261029/` contains probe source, exact
referenced DLL, `proof/poses.json`, `proof/Stretch.png`, `proof/Angry5.png`.
No source correction, installed-app replacement, live input, settings change,
commit/push or release replacement was made during this requested check.
Open correction targets: isolate a recognizable forepaw silhouette during stretch;
retain a rounded shoulder connection during stamp while preserving accepted arm
length, trajectory, cadence, counts, expression and other motions. A corrected
candidate needs raise/contact/return inspection and Windows/Mac parity again.

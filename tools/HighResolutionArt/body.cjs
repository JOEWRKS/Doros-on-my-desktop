'use strict';
const reference=require('../PreviewLocomotion/reference-parts.cjs');
// Only the high-resolution candidate opts into this source225 contour. Keep
// the accepted96 product rig immutable. The hind paw is a folded, tapering
// triangle, not a round capsule attached to a separate ankle bulge.
const artwork=structuredClone(reference.art);
const scaled=path=>path.map(([op,...q])=>[op,...q.map(v=>v*96/225)]);
const hindEdge=scaled([
 ['M',138.1,173.1],['C',139.1,173.8,137.8,177.2,139.1,181.2],
 ['C',141.2,188.5,148.2,197.3,153,197.1],
 ['C',158.5,196.8,165.7,191.5,165.2,186.2],
 ['C',164.9,183.3,162.8,181,163.7,175.4],
 ['C',164,172.9,165.2,170.8,166.5,169]
]);
artwork.nearHind={
 fill:[['M',59,59],['C',59,55,68,55,70,59],['C',72,64,72,69,166.5*96/225,169*96/225],
  // Reverse the measured contour so the upper hidden skin closes the same
  // visible source path used by the outline.
  ['C',165.2*96/225,170.8*96/225,164*96/225,172.9*96/225,163.7*96/225,175.4*96/225],
  ['C',162.8*96/225,181*96/225,164.9*96/225,183.3*96/225,165.2*96/225,186.2*96/225],
  ['C',165.7*96/225,191.5*96/225,158.5*96/225,196.8*96/225,153*96/225,197.1*96/225],
  ['C',148.2*96/225,197.3*96/225,141.2*96/225,188.5*96/225,139.1*96/225,181.2*96/225],
  ['C',137.8*96/225,177.2*96/225,139.1*96/225,173.8*96/225,138.1*96/225,173.1*96/225],
  ['C',58.5,69,58.5,62,59,59],['Z']],
 // This edge starts inside the haunch. Clip its socket section against the
 // haunch; treating the complete path as foreground ink leaves an internal
 // second ankle line. The exposed-skin pass still inks the actual silhouette.
 // Only the proximal socket is haunch-occluded. The distal folded paw stays
 // in front of the haunch during sitting; do not erase its whole boundary.
 ink:[scaled([
  ['M',139.1,181.2],['C',141.2,188.5,148.2,197.3,153,197.1],
  ['C',158.5,196.8,165.7,191.5,165.2,186.2],
  ['C',164.9,183.3,162.8,181,163.7,175.4]
 ])],
 rootInk:[scaled([['M',138.1,173.1],['C',139.1,173.8,137.8,177.2,139.1,181.2]]),
  scaled([['M',163.7,175.4],['C',164,172.9,165.2,170.8,166.5,169]])]
};
module.exports={artwork,hindEdge};

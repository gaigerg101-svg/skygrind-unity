// PG3 (GG 2026-10-28: "jump height needs to be lower"): the ULTRAKILL profile's jump (design.ultra71.set) from 5.07 m to 4.0 m, apexTime cut so gravity stays the
// same (g = 2h / t^2 = 20.58 m/s^2 before and after: t = sqrt(2 x 4.0 / 20.58) = 0.6235 s), so the fall feels the same and only the height drops. The double jump
// (v0 x doubleJumpMult 0.9), the wall jump and the dash jump scale with v0, so they drop with it. The web's jump (PortExport/config.json) is untouched: the golden
// vectors and the replays never load the profile. Edits both config copies and MovementTuning71Tests' assertion.
// Usage: node Tools/pg3/jump-lower.cjs
'use strict';
const fs = require('fs');
const path = require('path');
const root = path.resolve(__dirname, '..', '..');
const H = '4.0', T = '0.6235';
for (const rel of ['Design/unity-config.json', 'Assets/StreamingAssets/Design/unity-config.json']) {
  const p = path.join(root, rel);
  let s = fs.readFileSync(p, 'utf8');
  const re = /"jump\.height":5\.07,"jump\.apexTime":0\.702/;
  if (!re.test(s)) { console.error(rel + ': ultra71 jump not found as expected'); process.exit(1); }
  s = s.replace(re, '"jump.height":' + H + ',"jump.apexTime":' + T);
  s = s.replace('"_why": "GG 2026-10-26: movement almost a 1:1 copy of ULTRAKILL.', '"_why": "GG 2026-10-28: the jump lower (5.07 -> 4.0 m, apexTime 0.702 -> 0.6235 so gravity stays 20.58). GG 2026-10-26: movement almost a 1:1 copy of ULTRAKILL.');
  JSON.parse(s);
  fs.writeFileSync(p, s);
  console.log(rel + ': jump.height ' + H + ', apexTime ' + T);
}
const tp = path.join(root, 'Assets/Tests/EditMode/Game/MovementTuning71Tests.cs');
let t = fs.readFileSync(tp, 'utf8');
t = t.replace('Assert.That(h, Is.EqualTo(5.07).Within(1e-9));', 'Assert.That(h, Is.EqualTo(4.0).Within(1e-9)); // GG 2026-10-28: the jump lower (was 5.07)');
t = t.replace('Assert.That(c.movement.jump.apexTime, Is.EqualTo(0.702).Within(1e-9));', 'Assert.That(c.movement.jump.apexTime, Is.EqualTo(0.6235).Within(1e-9));');
fs.writeFileSync(tp, t);
console.log('MovementTuning71Tests: 4.0 / 0.6235');

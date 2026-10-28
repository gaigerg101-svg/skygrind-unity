// PG3 (GG 2026-10-28: "can we add them to the game now?"): turns the tripo74 row ON in both config copies (Design/ and the Player's StreamingAssets copy).
// The flip is GG's call in chat, not a blind-spotter verdict; the evidence is the pg3-tripo2 kill12 frames and the Editor renders named in the commit.
// Usage: node Tools/pg3/tripo-on.cjs
'use strict';
const fs = require('fs');
const path = require('path');
const root = path.resolve(__dirname, '..', '..');
const why = 'ON (PG3): GG asked for the Tripo models in the game (2026-10-28 15:10); pg3-tripo2 kill12 frames show the KESTREL facing forward and textured, Editor renders show the six enemies facing +Z';
for (const rel of ['Design/unity-config.json', 'Assets/StreamingAssets/Design/unity-config.json']) {
  const p = path.join(root, rel);
  const txt = fs.readFileSync(p, 'utf8');
  const re = /("tripo74": \{\s*"enabled": )false(,\s*"enabled_why": )"[^"]*"/;
  if (!re.test(txt)) { console.error(rel + ': tripo74 row not found as expected'); process.exit(1); }
  fs.writeFileSync(p, txt.replace(re, (m, a, b) => a + 'true' + b + JSON.stringify(why)));
  console.log(rel + ': tripo74 ON');
}

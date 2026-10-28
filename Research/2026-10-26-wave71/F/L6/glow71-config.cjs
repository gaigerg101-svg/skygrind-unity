// NIGHT71 builder F (L6): design.n71.glow71's numbers block, in both config copies (byte-identical). Touches ONLY the glow71 line of design.n71
// (the shared commit's one-line row): its `enabled` stays false; a "numbers" object is appended inside the same line. Idempotent, CRLF-safe,
// so the lead can re-apply it after a config conflict (L-049 / L-065).
// Usage (from the tree's root): node Research/2026-10-26-wave71/F/L6/glow71-config.cjs [--dry]
'use strict';
const fs = require('fs');
const path = require('path');

const dry = process.argv.includes('--dry');
const files = ['Design/unity-config.json', 'Assets/StreamingAssets/Design/unity-config.json'];
const key = '      "glow71": {';
const head = '      "glow71": {"enabled": false, "enabled_why": "OFF until GG\'s keep (NIGHT71: ON only in the play build)", "_why": "L6 (T-113): the PULSE\'s own gun glow"';
const numbers = {
  _why: "Builder F (NIGHT71 L6; Game/Glow71.cs, Viewmodel.Charge35.cs). With the row on (gunglow50 on), the PULSE gauntlet gets gunglow50's four charge steps ON TOP of its body (the body mesh's bounds' top), from its back toward the knuckles, in the player's cyan; they light with the PULSE's pips while its BLAST is off cooldown. NIGHT50's PULSE steps ran inside the forearm block and read alike at 0 % and 75 % (DECIDE50 D-8), so the PULSE was left out. m in the viewmodel's rig space (the gauntlet: forearm 0.086 x 0.078 x 0.15 m, its top plate 0.06 m wide, top at y 0.045). View only: no sim read, no tape bit. The web has no such numbers: its gauntlet had no charge steps.",
  segW: 0.036,
  segW_why: "m: each step's width across the top (x): under the top plate's 0.06 m, near gunglow50's 0.038 m barrel steps",
  segH: 0.01,
  segH_why: 'm: each step\'s height (y): a strip on the top face, not a block',
  lift: 0.002,
  lift_why: "m above the body's top (no z-fight with the top plate)",
  span: 0.8,
  span_why: "the share of the body's length the four steps run, from its back cap toward the knuckles",
  gapShare: 0.3,
  gapShare_why: "each slot's share left dark between steps (gunglow50's G50GapShare)",
};
const body = JSON.stringify(numbers).replace(/":/g, '": ').replace(/,"/g, ', "');
const line = `${head}, "numbers": ${body}},`;

const out = [];
for (const rel of files) {
  const p = path.join(process.cwd(), rel);
  const lines = fs.readFileSync(p, 'utf8').split('\n');
  const at = lines.findIndex((l) => l.startsWith(key));
  if (at < 0 || lines.findIndex((l, i) => i > at && l.startsWith(key)) >= 0) throw new Error(`${rel}: the glow71 line is missing or not unique`);
  const cr = lines[at].endsWith('\r') ? '\r' : '';
  const was = cr ? lines[at].slice(0, -1) : lines[at];
  if (!was.startsWith(head)) throw new Error(`${rel}: the glow71 line is not the shared commit's: ${was.slice(0, 160)}`);
  if (!was.endsWith('},') && !was.endsWith('}')) throw new Error(`${rel}: the glow71 line does not end with "}," or "}"`);
  const comma = was.endsWith('},') ? ',' : '';
  lines[at] = `${head}, "numbers": ${body}}${comma}` + cr;
  out.push([p, lines.join('\n')]);
}
if (out[0][1] !== out[1][1]) throw new Error('the two copies would differ (they must be byte-identical)');
JSON.parse(out[0][1]);
if (dry) console.log(line);
else for (const [p, t] of out) fs.writeFileSync(p, t);
console.log(`${dry ? 'dry: ' : ''}glow71 numbers: 5 numbers in ${files.length} copies`);

// NIGHT30 builder B (round 5): the boss lines for the Glacier, the Wyrm and the Loom (item 12, through builder S's one budget) and THE LOOM's SWEEP numbers, in Design/unity-config.json
// by text insertion (keeps the line endings and the layout; idempotent per row). Run from the repo root: node Tools/night/n30b-config-lines-sweep.cjs
const fs = require('fs');
const path = 'Design/unity-config.json';
let text = fs.readFileSync(path, 'utf8');
const eol = text.includes('\r\n') ? '\r\n' : '\n';
const n30 = text.indexOf('"n30": {');

// insert `rows` (each "key": value) after the line that ends with `afterKeyLine` inside the block that starts at `blockHead` (after n30)
function insertAfter(blockHead, afterRegex, rows, indent, skipIfKey) {
  const at = text.indexOf(blockHead, n30);
  if (at < 0) throw new Error('no ' + blockHead);
  const end = text.indexOf(eol + indent.slice(2) + '}', at);
  if (text.slice(at, end).includes('"' + skipIfKey + '"')) { console.log(skipIfKey + ': already present'); return; }
  const m = afterRegex.exec(text.slice(at, end));
  if (!m) throw new Error('anchor not found in ' + blockHead);
  const lineEnd = at + m.index + m[0].length;
  let ins = '';
  for (const r of rows) ins += ',' + eol + indent + r;
  text = text.slice(0, lineEnd) + ins + text.slice(lineEnd);
  console.log(skipIfKey + ': added');
}

insertAfter('"glacier30": {', /"windowSecondsGlide_why": "[^"]*"/, [
  `"lineVolley": "STAY CLOSE. DASH SIDEWAYS FROM SHARDS."`,
  `"lineVolley_why": "The Glacier's teaching line on its first homing-volley tells (item 12: a boss explains its rule in at most two short lines on the first tell; the lead's words, through SurvivalSim.BossLine30's budget)."`,
], '        ', 'lineVolley');
insertAfter('"wyrm30": {', /"lateSeconds_why": "[^"]*"/, [
  `"linePunch": "PUNCH THE GLOWING HEAD."`,
  `"linePunch_why": "The Wyrm's teaching line on its first punchable windows (item 12; through SurvivalSim.BossLine30's budget)."`,
], '        ', 'linePunch');
insertAfter('"loom": {', /"legThick_why": "[^"]*"/, [
  `"lineStart": "HANG ON A LEG. NAIL THE JOINTS."`,
  `"lineStart_why": "The Loom's teaching line when its fight starts (item 12; through SurvivalSim.BossLine30's budget)."`,
  `"sweepFirstSeconds": 8`, `"sweepFirstSeconds_why": "The first SWEEP can come this long after the intro."`,
  `"sweepEverySeconds": 8`, `"sweepEverySeconds_why": "After a sweep the next waits this long; it only tells at a player standing on the deck (what stops a player standing there forever: DESIGN30 D)."`,
  `"sweepTellSeconds": 1.2`, `"sweepTellSeconds_why": "The leg's red mark across the deck before it sweeps (DESIGN30 D: a 1.2 s tell)."`,
  `"sweepLiveSeconds": 0.6`, `"sweepLiveSeconds_why": "The sweep crosses the deck in this long: a player on the deck with his feet low in it is hit once."`,
  `"sweepHeight": 1.2`, `"sweepHeight_why": "The sweep's height over the deck (m): a jump (about 3 m high) clears it; a grapple off the deck does too."`,
  `"sweepDamage": 20`, `"sweepDamage_why": "The sweep's one hit (x the fight's damage scale): a fifth of a 100 HP player; nothing kills in under 0.5 s."`,
  `"sweepKnock": 9`, `"sweepKnock_why": "It knocks a hit player outward at this speed (m/s), toward the deck's edge and the lava: the deck is no place to stand."`,
  `"sweepUp": 5`, `"sweepUp_why": "...and up at this speed (m/s), so the knock is an arc, not a slide."`,
], '          ', 'lineStart');
fs.writeFileSync(path, text);

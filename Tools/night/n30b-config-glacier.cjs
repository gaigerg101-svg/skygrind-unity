// NIGHT30 builder B: adds THE GLACIER REBUILT's numbers (design.n30.glacier30) to Design/unity-config.json by text insertion after the block's "enabled" line
// (keeps the line endings and the layout; idempotent: does nothing when "farMeters" is already in the block). Run from the repo root: node Tools/night/n30b-config-glacier.cjs
const fs = require('fs');
const path = 'Design/unity-config.json';
let text = fs.readFileSync(path, 'utf8');
const eol = text.includes('\r\n') ? '\r\n' : '\n';
const at = text.indexOf('"glacier30": {', text.indexOf('"n30": {'));
if (at < 0) throw new Error('no glacier30 block');
const end = text.indexOf(eol + '      }', at);
if (text.slice(at, end).includes('"farMeters"')) { console.log('glacier30 numbers already present'); process.exit(0); }
const m = /"enabled": (true|false)/.exec(text.slice(at));
if (!m || at + m.index > end) throw new Error('no enabled line in glacier30');
const lineEnd = at + m.index + m[0].length;
const I = '        ', J = '          ';
const rows = [
  `"farMeters": 22`, `"farMeters_why": "Distance is punished (DESIGN30 C2): a player farther than this (straight line, chest to the boss) fills the patience gauge. 22 m is DESIGN30's first pick; the pilot's out-of-reach line checks it."`,
  `"hideSeconds": 3`, `"hideSeconds_why": "Out of the boss's sight (the world's ray from the boss to the player's chest blocked: the island, the bank's lip) this long also fills the gauge (DESIGN30 C2)."`,
  `"fillSeconds": 4`, `"fillSeconds_why": "The gauge fills over this many seconds of far / hidden (DESIGN30 C2: 4 s), then the homing volley."`,
  `"drainMult": 2`, `"drainMult_why": "Back in reach the gauge drains this many times faster than it fills (closing in is rewarded quickly)."`,
  `"volleyTellSeconds": 0.8`, `"volleyTellSeconds_why": "The homing volley's tell (DESIGN30 C2: 0.8 s; the boss's tells 0.8-1.5 s)."`,
  `"volleyCount": 5`, `"volleyCount_why": "Shards in a homing volley, fanned over volleySpreadDeg."`,
  `"volleySpeed": 13`, `"volleySpeed_why": "The shards' speed (m/s): slower than a dash (a sideways dash out-turns them), faster than a run."`,
  `"turnDegPerSecond": 70`, `"turnDegPerSecond_why": "How fast a shard turns toward the player (deg/s): a straight run is caught, a sideways dash is not (DESIGN30 C2: a sideways dash dodges)."`,
  `"volleyLife": 3`, `"volleyLife_why": "A shard's life (s); it also breaks on the world (the island, the walls)."`,
  `"volleyDamage": 10`, `"volleyDamage_why": "Damage per shard (five shards: a volley that all lands costs half of a 100 HP player; none lethal under 0.5 s alone)."`,
  `"volleyRadius": 0.5`, `"volleyRadius_why": "A shard's hit radius (m) against the player's capsule."`,
  `"volleySpreadDeg": 18`, `"volleySpreadDeg_why": "The volley's fan half-angle (deg) at the throw."`,
  `"sweepEverySeconds": 9`, `"sweepEverySeconds_why": "A lane-wide frost sweep aimed at the player every 9 s (DESIGN30 C2), whatever the distance: nowhere on the ice is out of reach."`,
  `"sweepFirstSeconds": 6`, `"sweepFirstSeconds_why": "The first sweep comes this long into the fight (the opening chase reads first)."`,
  `"sweepTellSeconds": 1`, `"sweepTellSeconds_why": "The sweep's band is marked this long before it goes live (tells 1-1.5 s)."`,
  `"sweepLiveSeconds": 0.5`, `"sweepLiveSeconds_why": "The band is live this long: one hit (sweepDamage) and the frost's bite (design.n29.boss2.frostSlow) to a player in it."`,
  `"sweepWidth": 4`, `"sweepWidth_why": "The band's width across the rink (m), centred on the player's lateral position when the tell starts: a glide sideways out of it dodges."`,
  `"sweepHalfLength": 8`, `"sweepHalfLength_why": "The band reaches this far either side of the player along the lap (m): braking or racing ahead 8 m also dodges."`,
  `"sweepDamage": 12`, `"sweepDamage_why": "The sweep's one hit."`,
  `"sweepHeight": 2.2`, `"sweepHeight_why": "The band reaches this high above the floor under the player (m): a jump over it dodges too."`,
  `"rink": {`,
];
const rink = [
  `"_why": "THE GLACIER's closed rink rebuilt bigger (DESIGN30 C2: two to three times the 28 x 52 rink): numbers here replace design.n28.rink's (Rink28Params.From(cfg, over)); the outer half-width is islandRadius + laneWidth + bankRun + lip = 22 m and the length straight + 2 x 22 = 70 m: 44 x 70 (2.2x the area). The banks stay inside the closed rim and the boss's reach (the volley, the sweep)."`,
  `"straight": 26`, `"straight_why": "The straights' length (m): 70 m end to end."`,
  `"islandRadius": 6`, `"islandRadius_why": "The central island (m): a bigger sight blocker, so hiding behind it is a choice the gauge punishes."`,
  `"islandHeight": 3.5`, `"islandHeight_why": "The island is taller than a standing player's eye (it blocks the boss's sight)."`,
  `"laneWidth": 11`, `"laneWidth_why": "The flat ice lane (m): room to glide and to dodge a 4 m sweep band sideways."`,
  `"centreRadius": 11.5`, `"centreRadius_why": "The lap's centre line (the island's edge + half the lane): where the boss runs."`,
  `"bankRun": 4.6`, `"bankRun_why": "The bank's run (m) at the rink's 28 deg: a 2.4 m speed ramp inside the rim."`,
  `"doorWidth": 5`, `"doorWidth_why": "The island's doors (m): the cut stays."`,
  `"rimHeight": 8`, `"rimHeight_why": "The rim wall up to the lid (m)."`,
];
let ins = '';
for (const r of rows) ins += ',' + eol + I + r;
ins = ins.replace(/,(\r?\n) {8}"rink": \{$/, '');
ins += ',' + eol + I + '"rink": {';
ins += rink.map((r, i) => eol + J + r + (i < rink.length - 1 ? ',' : '')).join('');
ins += eol + I + '}';
text = text.slice(0, lineEnd) + ins + text.slice(lineEnd);
fs.writeFileSync(path, text);
console.log('glacier30: added', (rows.length - 1) / 2, 'numbers and the rink');

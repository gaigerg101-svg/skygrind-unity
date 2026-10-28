// NIGHT30 builder B: adds THE LANTERN RING's numbers (design.n30.lantern30) to Design/unity-config.json by text insertion after the block's "enabled" line
// (keeps CRLF and the layout; idempotent: does nothing when "leadSeconds" is already in the block). Run from the repo root: node Tools/night/n30b-config-lantern.cjs
const fs = require('fs');
const path = 'Design/unity-config.json';
let text = fs.readFileSync(path, 'utf8');
const eol = text.includes('\r\n') ? '\r\n' : '\n';
const head = '"lantern30": {';
const at = text.indexOf(head, text.indexOf('"n30": {'));
if (at < 0) throw new Error('no lantern30 block');
const end = text.indexOf(eol + '      }', at);
const block = text.slice(at, end);
if (block.includes('"leadSeconds"')) { console.log('lantern30 numbers already present'); process.exit(0); }
const en = text.indexOf('"enabled": false', at);
if (en < 0 || en > end) throw new Error('no enabled line in lantern30');
const lineEnd = en + '"enabled": false'.length;
const I = '        ';
const J = '          ';
const rows = [
  `"tellSeconds": 1.2`,
  `"tellSeconds_why": "The floor's tell before it goes live (DESIGN30 C1: R1's 1-1.5 s tell; Lantern29's 0.9 s read too short in GG's frames): replaces design.n29.boss1.tellSeconds while this row is on (the cycle's rest shortens by the same amount)."`,
  `"leadSeconds": 0.6`,
  `"leadSeconds_why": "The fan orbs LEAD the rider: at the moment they fire they turn to the player's position leadSeconds ahead (position + velocity x leadSeconds), so circling at a steady speed is not safe (R1: the strafe circle is a named failure). 0.6 s is DESIGN30's first pick; the rider bot's kill band checks it."`,
  `"sweepEvery": 2`,
  `"sweepEvery_why": "Every sweepEvery-th live phase a SWEEP marks one wall panel ahead of the rider (DESIGN30 C1: every second live phase)."`,
  `"sweepAheadPanels": 2`,
  `"sweepAheadPanels_why": "How many panels ahead of the rider (in its direction round the ring) the sweep marks: a rider at 10-12 m/s covers about two 5.9 m panels in the 1.2 s tell, so the burn lands where it would be; it must jump the seam or drop to a platform."`,
  `"sweepTellSeconds": 1.2`,
  `"sweepTellSeconds_why": "The sweep's red-orange mark before it burns (tells 1-1.5 s, never over 2 s: NIGHT30's boss rule)."`,
  `"sweepBurnSeconds": 1.5`,
  `"sweepBurnSeconds_why": "How long the marked panel burns (DESIGN30 C1)."`,
  `"sweepDamagePerSecond": 30`,
  `"sweepDamagePerSecond_why": "Damage per second to a player within sweepReach of the burning panel, in the hazardFloor27 tick (0.25 s: 7.5 a tick, at most 45 over the whole burn): nothing kills in under 0.5 s."`,
  `"sweepReach": 1.5`,
  `"sweepReach_why": "How far from the burning panel's face the burn reaches (a rider's capsule is 0.35 m from the wall; 1.5 m catches a rider and a player hugging it, not the platform's front edge 2.4 m out)."`,
  `"platformScale": 0`,
  `"platformScale_why": "The share of a hit that lands from a shooter standing on a platform (DESIGN30 I-3: the platforms are a refuge from the lava, not a firing spot: 0 = no damage)."`,
  `"lineSeconds": 3`,
  `"lineSeconds_why": "How long a teaching line stays up (the Lesson / boss-line code reads Lantern30.LineNow; event bossLine30)."`,
  `"lineMaxShows": 2`,
  `"lineMaxShows_why": "Each teaching line shows at most this many times per fight (NIGHT30 item 4)."`,
  `"lineTell": "GET ABOVE THE LAVA"`,
  `"lineTell_why": "The line on the floor's tell (GG's own words: 'including guidance like get above the lava')."`,
  `"lineCore": "RIDE THE WALL. SHOOT THE CORE."`,
  `"lineCore_why": "The line when the core opens (DESIGN30 C1)."`,
  `"arena": {`,
];
const arena = [
  `"_why": "THE LANTERN RING (Core/Enemies/Lantern30Arena.cs, a triangle soup like Rink28Arena, at design.n27.boss1.room.origin): a closed ring of flat wall panels (the Mover's ride turns at most 30 deg per ride; seam30 measured a 22.5 deg and a 30 deg seam: the same ride survives both but bleeds to the 8 m/s ride floor, so neither is clean and the ring keeps DESIGN30 I-4's 16 panels), a closed roof, three platforms against the wall, three heat vents. Replaces the NIGHT27 square courtyard's boxes while design.n30.lantern30 is on."`,
  `"panels": 16`,
  `"panels_why": "16 flat panels of 22.5 deg (DESIGN30 I-4; seam30: a 30 deg seam is not clean either)."`,
  `"apothem": 15`,
  `"apothem_why": "Centre to a panel's face: 30 m across (DESIGN30 C1: about 30 m), a panel 5.97 m wide."`,
  `"height": 14`,
  `"height_why": "Floor to the closed roof (DESIGN30 C1: 14 m tall)."`,
  `"platformTop": 2.4`,
  `"platformTop_why": "The platforms' top (DESIGN30 C1: 2.4 m; a running jump reaches about 3.15 m, so a player on the floor can climb one)."`,
  `"platformDepth": 2.4`,
  `"platformDepth_why": "How far a platform reaches out from its panel (DESIGN30 I-5: against the wall; a rider drops onto one from the lane)."`,
  `"platformInset": 0.6`,
  `"platformInset_why": "The gap between a platform's ends and its panel's seams (the platform stays on one flat panel)."`,
  `"platformPanels": [2, 7, 13]`,
  `"platformPanels_why": "The three platform panels at about the ring's thirds, none at the south entry (the player enters at panel 4's seam, 90 deg)."`,
  `"laneLow": 2`,
  `"laneLow_why": "The wall-ride lane strip's bottom on each panel (DESIGN30 C1: y 2-6; a view strip: the whole panel is rideable)."`,
  `"laneHigh": 6`,
  `"laneHigh_why": "The lane strip's top."`,
  `"ventPanels": [0, 5, 10]`,
  `"ventPanels_why": "The three heat vents' directions (a vent sits in front of each of these panels): the view glows them through the floor's tell (they never glow for nothing: DESIGN30 B f12 question 6)."`,
  `"ventRadius": 9`,
  `"ventRadius_why": "How far from the centre the vents sit on the floor."`,
];
let ins = '';
for (const r of rows) ins += ',' + eol + I + r.replace(/^"arena": \{$/, '"arena": {');
// the arena object's rows (one level deeper), then close it
ins = ins.replace(/,\r?\n {8}"arena": \{$/, '');
ins += ',' + eol + I + '"arena": {';
ins += arena.map((r, i) => eol + J + r + (i < arena.length - 1 ? ',' : '')).join('');
ins += eol + I + '}';
text = text.slice(0, lineEnd) + ins + text.slice(lineEnd);
fs.writeFileSync(path, text);
console.log('lantern30: added', rows.length - 1, 'rows and the arena block');

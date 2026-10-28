#!/usr/bin/env node
// NIGHT72's shared commit (FORGE72, 2026-10-27; a rewrite of n71-rows.cjs; the lead runs it at the bearings, BEFORE the builders'
// worktrees, and commits it alone with Rows72.cs; builders branch from that sha). It pre-places every row the slate may build, so no two
// builders add the `design.n72` block or edit the FrameRows line (L-049 / L-065):
//   - `design.n72` in BOTH config copies (Design/unity-config.json, Assets/StreamingAssets/Design/unity-config.json; byte-identical after),
//     inserted as text just before the `"n71": {` block (the files keep their format), each row
//     {"enabled": false, "enabled_why": "OFF until the lead's ON flip (NIGHT72 D-1: ...)", "_why": "<slice>: <what>"};
//   - the view rows appended to SameRun63Math.FrameRows (Assets/Scripts/Game/SameRun63Math.N63D.cs), so -skygrindSameRun63 pairs work.
// View rows only (no `sim`, no tape bit): NightRows.ViewOn reads `design.n72.<row>.enabled` by the row's name; the game is unchanged with
// every row OFF. A builder fills its own row's numbers inside its block; it never adds or removes a row. NIGHT72 turns rows ON in the
// repo (DECISIONS D-1) with n72-freeze-rows.cjs, one commit per row, after the row's pair, SPOT and guards.
//   node Tools/night/n72-rows.cjs [--dry] [--root <dir>]
'use strict';
const fs = require('fs');
const path = require('path');
const argv = process.argv.slice(2);
const ROOT = path.resolve(argv.includes('--root') ? argv[argv.indexOf('--root') + 1] : path.join(__dirname, '..', '..'));
const dry = argv.includes('--dry');
const CFG = ['Design/unity-config.json', 'Assets/StreamingAssets/Design/unity-config.json'];
const FR = 'Assets/Scripts/Game/SameRun63Math.N63D.cs';

const ROWS = [
  ['calm72', 'L4 (gg-proxy #10; GG 2026-10-06 "that weird border", 2026-10-11 the PS2 overlay): no noise border at full health; the low-health edge thin and only under 30 % health', true],
  ['lit72', 'L3 (gg-proxy #1; GG 2026-10-26 "the boss rooms are very dark"): the boss arenas and level 5 lifted inside each boss\'s own hue', true],
  ['fix72', 'L2 (GG 2026-10-26 "there\'s flickers"): the flicker sources the meter names, fixed (render settings only)', true],
  ['palette72', 'C2 (gg-proxy #2; GG 2026-10-26 "colors ... I just haven\'t seen it"): each level\'s floor / wall / trim values apart; red-orange only on danger', true],
  ['decor72', 'C3 (gg-proxy #8; GG 2026-10-20 "lack strong color direction"): stripes and blocks with no job removed or made into route / ride marks', true],
  ['body72', 'B1 (gg-proxy #4; GG 2026-10-22 "boss animations are too rigid"): THE KILN WYRM restyled inside the pill language (presence, spring follow-through, a tell glow; no new creature design: CLAUDE.md rule 7), render only, inside BossMotion63\'s hitbox guard', true],
  ['spring72', 'B4 (GG 2026-10-26 "animations ... I just haven\'t seen it"): spring secondary motion on bosses and enemies (windup, flinch, idle, death); the rest silhouette unchanged (the pill language, CLAUDE.md rule 7)', true],
  ['bossfx72', 'B2 (gg-proxy #3; GG 2026-10-20 "the guns and effects during boss fights should follow proper color theory"): boss-fight effects off the crosshair, each in its role colour', true],
  ['btext72', 'B3 (gg-proxy #5; T-155): boss text right (the hand you hold, STAGGER only when staggered) and readable over beams', true],
  ['goal72', 'N3 (gg-proxy #6; GG 2026-10-26 "OK now you have to go up"): the room\'s one line at the thing it names, its marker, stuck hints that retire on success', true],
  ['cam72', 'N4 (GG 2026-10-26 ULTRAKILL "almost a one to one copy"): the ULTRAKILL camera roll, dash FOV and slide eye drop (view only)', true],
];

const WHY_OFF = "OFF until the lead's ON flip (NIGHT72 D-1: its same-build pair, a SPOT naming the change, the next train's guards)";
// one multi-line block per row (FORGE72 red team 8): the lead's flips (n72-freeze-rows.cjs, n72-off-row.cjs) edit one row's lines only;
// builders keep their numbers in UnityTuning.N72<letter>.cs, so no builder pick touches the config
const rowsJson = (indent) => ROWS.map(([id, why]) => indent + '  "' + id + '": {\n' + indent + '    "enabled": false,\n' + indent + '    "enabled_why": ' + JSON.stringify(WHY_OFF) + ',\n' + indent + '    "_why": ' + JSON.stringify(why) + '\n' + indent + '  }').join(',\n');
const block = (indent) => indent + '"n72": {\n' + indent + '  "_why": ' + JSON.stringify("NIGHT72 (FORGE72, GG 2026-10-26 evening): this night's rows, all VIEW rows (no sim, no tape bit), pre-placed by Tools/night/n72-rows.cjs so builders never add the block. ON in the repo by the lead (D-1); off: -skygrindNo<Row>72 or -skygrindRows72 <row>:0 (Rows72.cs).") + ',\n' + rowsJson(indent) + '\n' + indent + '},\n';

const out = [];
for (const rel of CFG) {
  const f = path.join(ROOT, rel);
  const s = fs.readFileSync(f, 'utf8');
  if (/"n72": \{/.test(s)) { console.log('n72-rows: ' + rel + ' already holds "n72" (refused: run once)'); process.exit(1); }
  const m = s.match(/^( *)"n71": \{/m);
  if (!m || (s.match(/^ *"n71": \{/gm) || []).length !== 1) { console.log('n72-rows: ' + rel + ': no single "n71": { line'); process.exit(1); }
  const t = s.slice(0, m.index) + block(m[1]).replace(/\n/g, s.includes('\r\n') ? '\r\n' : '\n') + s.slice(m.index);
  const j = JSON.parse(t);
  for (const [id] of ROWS) if (j.design.n72[id].enabled !== false) { console.log('n72-rows: ' + id + ' not false'); process.exit(1); }
  out.push([f, t]);
}
if (out[0][1] !== out[1][1]) {
  const a = fs.readFileSync(path.join(ROOT, CFG[0]), 'utf8'), b = fs.readFileSync(path.join(ROOT, CFG[1]), 'utf8');
  console.log('n72-rows: the two config copies ' + (a === b ? 'were identical but differ after (refused)' : 'already differ (refused: fix that first)')); process.exit(1);
}
const frf = path.join(ROOT, FR);
const fr = fs.readFileSync(frf, 'utf8');
const anchor = '"marks71", "glow71" };';
if (fr.split(anchor).length !== 2) { console.log('n72-rows: the FrameRows anchor is not found exactly once in ' + FR); process.exit(1); }
const views = ROWS.filter((r) => r[2]).map((r) => '"' + r[0] + '"');
const fr2 = fr.replace(anchor, '"marks71", "glow71", ' + views.join(', ') + ' };');
if (dry) { console.log('n72-rows --dry: would add design.n72 (' + ROWS.length + ' rows, all OFF) to both copies and ' + views.length + ' FrameRows'); process.exit(0); }
for (const [f, t] of out) fs.writeFileSync(f, t);
fs.writeFileSync(frf, fr2);
console.log('n72-rows: design.n72 (' + ROWS.length + ' rows, all OFF) in both config copies (byte-identical); FrameRows + ' + views.join(' '));
console.log('commit it with Rows72.cs: git commit -m "NIGHT72 shared commit: design.n72 placeholders, FrameRows, Rows72 switch" -- ' + CFG.join(' ') + ' ' + FR + ' Assets/Scripts/Game/Rows72.cs Assets/Scripts/Game/Rows72.cs.meta <its test and the boot line>');

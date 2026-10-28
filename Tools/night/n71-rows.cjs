#!/usr/bin/env node
// NIGHT71's shared commit (FORGE71 red team finding 2; the lead runs it at the bearings, BEFORE the builders' worktrees, and commits it
// alone; builders branch from that sha). It pre-places every row the slate and the leftovers may build, so no two builders add the
// `design.n71` block or edit the FrameRows line (FORGE64 redteam 11; L-049 / L-065):
//   - `design.n71` in BOTH config copies (Design/unity-config.json, Assets/StreamingAssets/Design/unity-config.json; byte-identical after),
//     inserted as text just before the `"n67": {` block (the files keep their format), each row
//     {"enabled": false, "enabled_why": "OFF until GG's keep (NIGHT71: ON only in the play build)", "_why": "<slice>: <what>"};
//   - the view rows appended to SameRun63Math.FrameRows (Assets/Scripts/Game/SameRun63Math.N63D.cs), so -skygrindSameRun63 pairs work.
// View rows only (no `sim`, no tape bit): NightRows.ViewOn reads `design.n71.<row>.enabled` by the row's name; the game is unchanged with
// every row OFF. A builder fills its own row's numbers inside its block; it never adds or removes a row.
//   node Tools/night/n71-rows.cjs [--dry] [--root <dir>]     (--root: a scratch copy for the forge's test)
'use strict';
const fs = require('fs');
const path = require('path');
const argv = process.argv.slice(2);
const ROOT = path.resolve(argv.includes('--root') ? argv[argv.indexOf('--root') + 1] : path.join(__dirname, '..', '..'));
const dry = argv.includes('--dry');
const CFG = ['Design/unity-config.json', 'Assets/StreamingAssets/Design/unity-config.json'];
const FR = 'Assets/Scripts/Game/SameRun63Math.N63D.cs';

const ROWS = [
  ['voice71', 'B1 (T-155): one order at a time on THE CROWN (call40 hidden while the fight line or bar draws the window order)', true],
  ['words71', 'B2 (T-155, DECIDE63 D-4): the orders name the hand you hold', true],
  ['wyrm71', 'B3 (T-154): a wyrm motion that reads (BossMotion63 wyrm numbers; render only)', true],
  ['cyan71', 'B4 (T-153, GG\'s DECIDE64 call): boss weak points in bone while closed, cyan while open (play build A / B only)', true],
  ['kicks71', 'F3 (T-77): feel46 kicks bigger and rarer', true],
  ['pace71', 'L2 (T-152a): the frame spike F1 names, fixed (render side)', true],
  ['wash71', 'L4 (T-105): the kill wash f3-f4 source', true],
  ['marks71', 'L5 (T-135): kill55 marks drawn near-black', true],
  ['glow71', 'L6 (T-113): the PULSE\'s own gun glow', true],
  ['room71', 'L1: a room start for the play build (level select left / right; DOORFREE on)', false],
  ['reshape71', 'L8 moonshot: the arena reshapes at wave 3 (a level event in whole steps; play build only)', false],
];

const rowsJson = (indent) => ROWS.map(([id, why]) => indent + '  "' + id + '": {"enabled": false, "enabled_why": "OFF until GG\'s keep (NIGHT71: ON only in the play build)", "_why": ' + JSON.stringify(why) + '}').join(',\n');
const block = (indent) => indent + '"n71": {\n' + indent + '  "_why": ' + JSON.stringify("NIGHT71 (GG 2026-10-26: \"Bots guard, I judge\"): this night's rows, all VIEW rows (no sim, no tape bit), pre-placed by Tools/night/n71-rows.cjs so builders never add the block; each OFF in the repo until GG's keep (DECIDE71), ON only in Builds/n71-play") + ',\n' + rowsJson(indent) + '\n' + indent + '},\n';

let out = [];
for (const rel of CFG) {
  const f = path.join(ROOT, rel);
  const s = fs.readFileSync(f, 'utf8');
  if (/"n71": \{/.test(s)) { console.log('n71-rows: ' + rel + ' already holds "n71" (refused: run once)'); process.exit(1); }
  const m = s.match(/^( *)"n67": \{/m);
  if (!m || (s.match(/^ *"n67": \{/gm) || []).length !== 1) { console.log('n71-rows: ' + rel + ': no single "n67": { line'); process.exit(1); }
  const t = s.slice(0, m.index) + block(m[1]).replace(/\n/g, s.includes('\r\n') ? '\r\n' : '\n') + s.slice(m.index);
  const j = JSON.parse(t);
  for (const [id] of ROWS) if (j.design.n71[id].enabled !== false) { console.log('n71-rows: ' + id + ' not false'); process.exit(1); }
  out.push([f, t]);
}
if (out[0][1] !== out[1][1]) {
  const a = fs.readFileSync(path.join(ROOT, CFG[0]), 'utf8'), b = fs.readFileSync(path.join(ROOT, CFG[1]), 'utf8');
  console.log('n71-rows: the two config copies ' + (a === b ? 'were identical but differ after (refused)' : 'already differ (refused: fix that first)')); process.exit(1);
}
const frf = path.join(ROOT, FR);
const fr = fs.readFileSync(frf, 'utf8');
const anchor = '"order64", "fightLine64" };';
if (fr.split(anchor).length !== 2) { console.log('n71-rows: the FrameRows anchor is not found exactly once in ' + FR); process.exit(1); }
const views = ROWS.filter((r) => r[2]).map((r) => '"' + r[0] + '"');
const fr2 = fr.replace(anchor, '"order64", "fightLine64", ' + views.join(', ') + ' };');
if (dry) { console.log('n71-rows --dry: would add design.n71 (' + ROWS.length + ' rows, all OFF) to both copies and ' + views.length + ' FrameRows'); process.exit(0); }
for (const [f, t] of out) fs.writeFileSync(f, t);
fs.writeFileSync(frf, fr2);
console.log('n71-rows: design.n71 (' + ROWS.length + ' rows, all OFF) in both config copies (byte-identical); FrameRows + ' + views.join(' '));
console.log('commit it alone: git commit -m "NIGHT71 shared commit: design.n71 placeholders and FrameRows" -- ' + CFG.join(' ') + ' ' + FR);

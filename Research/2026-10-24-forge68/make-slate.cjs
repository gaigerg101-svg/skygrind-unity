// FORGE68 (2026-10-24): writes Research/2026-10-24-forge68/slate-seed.json from NIGHT67's seed (the forges' method: a slate that ran a
// whole night, with the night's names changed), then FORGE68's changes: the instruments first (I1-I4), the M2 rule on pooled seeds 1-20
// with a STYLE rule that can be met (levels-data.md item 2), the level review scheduled, six rooms (levels-data.md section 3), PC always.
//   node Research/2026-10-24-forge68/make-slate.cjs
'use strict';
const fs = require('fs');
const path = require('path');
const ROOT = path.join(__dirname, '..', '..');
const old = JSON.parse(fs.readFileSync(path.join(ROOT, 'Research/2026-10-24-forge67b/slate-seed.json'), 'utf8'));
const ren = (s) => s.replace(/\bn67-(?!iceline|kiln|spire|foundry)/g, 'n68-').replace(/Logs\/n67\b/g, 'Logs/n68').replace(/NIGHT67/g, 'NIGHT68');
const blank = { state: 'todo', passes: false, verdict: null, spot: null, passSha: null, passFiles: null };

const ROOM = old.ROOM.map(ren);
// M2: pooled seeds 1-20 (history.md: the +0.20 bar sits inside 10-seed noise; g7 1.56 on 1-10, 1.24 on 11-20) and a STYLE rule that can be met
const m2 = ROOM.findIndex((l) => l.startsWith('M2 VERB'));
ROOM[m2] = "M2 VERB: skip v2 on the room's verb, POOLED over seeds 1-20 (the median of 20; the per-set ratios printed beside it), prints a ratio >= the room's mainBar = max(1.25, its red's pooled ratio + 0.20) with >= 14 of 20 seeds exercised and the refused run's refusal fired (`refused <verb> N` with N > 0 on every exercised seed: the ride refusal leaked on 10 / 10 in g8 and kb3c at NIGHT67); STYLE above DULL on the verb-aware run >= min(refusal + 15, max(refusal, 95)) (FORGE68 and its red team 10: the +15 rule alone was unreachable on kb3 80.1 % and v6 88.6 %, levels-data.md item 2, and a step at 80 % would reward a more stylish refusal); a ride room whose ratio is in [mainBar, mainBar + 0.10) is UNSURE and does not PASS M2; on the GATE pack's fresh seeds 21-30 (never tuned on) the median ratio >= 1.25 and >= 7 of 10 exercised (the verb still the faster line on seeds the builder never saw)";
const gate = ROOM.findIndex((l) => l.startsWith('a GATE needs'));
ROOM[gate] = ROOM[gate].replace('skip v2 also on seeds 11-20 (the GATE pack\'s fresh skip)', 'skip v2 also on fresh seeds 21-30 (the GATE pack\'s fresh skip, or the r1 pre-run it takes by --fresh; a pack whose fresh line names seed 11 is refused by the critic)');
if (!/seeds 21-30/.test(ROOM[gate])) throw new Error('ROOM gate line: the seeds 11-20 phrase did not apply');
ROOM[gate] = ROOM[gate].replace('and that round read by a level review (the GATE critic writes it when none did)', 'and that round read by a level review written at its named time (NIGHT68.md section 4: a fresh reviewer starts the moment the round ends; NIGHT67 never wrote one, so no room could have passed)');

const tool = (id, title, why, red, doneWhen) => Object.assign({ id, owner: 'T' }, blank, { title, why, row: 'none (tools: Tools/SimHarness, harness only)', red, doneWhen, visible: 'n/a (tools)', spotPose: null });
const room = (id, owner, level, title, why, red) => Object.assign({ id, owner }, blank, { level, verb: null, mainBar: null, title, why, row: 'slot ' + id + ' of n67.' + level + '67', red, doneWhen: ['the ROOM list'], visible: "the room's verb piece and its high plane read at a glance", spotPose: "the GATE pack's iso pair" });
const pair = 'a planted pair saved under Logs/n68/I<n>/ (RED on the t0 sha\'s DLL, GREEN on the fix), the whole-level walk on all five shipped levels with --off67 EQUAL to Logs/n68/flow-base-a0.txt unless the slice names the change, and one critic PASS (Logs/n68/verdict-I<n>.md)';

const slices = [
  tool('I1', 'The ride pilot rides the line65 face only, and the ride refusal holds (by 2:05; the largest, last)',
    "NIGHT67-REPORT.md section 7 (a): the verb-aware pilot rides any face within 2.5 m to its far end (g8: 92-97 rides vs 55-59), so a ride wall on the fighting floor costs time; history.md: the refused run still rode on 10 / 10 seeds in g8 and kb3c.",
    'NIGHT67 r3: g8 0.77, kb3c 1.06 (Logs/n67/r3-<L>/verbs-skip.txt)',
    ['the verb-aware ride run rides only the room\'s line65 face (a planted room with a line65 face and a second ride face: rides on the second face 0 after, > 0 before)', 'the refused ride run rides 0 times on every seed and prints `refused ride N` with N > 0 (g8 and kb3c at the t0 sha: rides > 0 on the refused run, the RED)', pair]),
  tool('I2', 'The launch pilot and the launch trigger target only bodies the punch does not kill (by 1:10)',
    "levels-data.md item 3: the trigger takes any husk-shaped body (Harness.N65.VerbPilot.cs:197) and punch damage 10 equals a swarmer's 10 hp: 5 of 10 bodies in v2, 6 of 13 in v6 die before any throw.",
    'NIGHT67 r3: v2 1.01, v6 0.96, kb3 1.02; kb3 M3 RED (target dead by the punch)',
    ['the launch pilot and the skip test\'s launch trigger pick only a body whose hp is above the punch\'s damage and print the target\'s kind and hp; a planted swarmer-only room prints `no launch target` (not a kill)', 'shipped kb3: M3 reads `in a pit yes` or `in lava yes` on a launched body that survived the punch', pair]),
  tool('I3', 'The walk bot reads lava by height (by 0:45; first)',
    'NIGHT67-REPORT.md section 7 (c): n46flow freezes wherever a point 1.5 m ahead is lava in plan at any height (Harness.N46.Flow.cs:570): a deck over lava reads as lava (v2 stall 24.7 vs 19.0).',
    'NIGHT67 r3 v2: stall A 24.7 % vs ceiling 19.0 %',
    ['a planted deck 2 m over lava: the walk bot crosses it (stall at the deck 0 after, > 0 before); a planted lava floor at foot height still refused', 'the whole-level walk on the five shipped levels: any change from flow-base-a0 is listed per level and judged by I3\'s critic (the shipped levels have decks over lava: a change is expected and must be named, not hidden); the flow base re-made after I3 lands, kept as flow-base-i3.txt', 'one critic PASS (Logs/n68/verdict-I3.md)']),
  tool('I4', 'Skip v2 pools seeds 1-20, prints the refusal count, and takes the new STYLE rule; every red re-made on I1-I4 (the tool by 1:35; the reds in r1\'s chain)',
    'history.md: the +0.20 bar sits inside 10-seed noise (per-seed 0.67-4.30; g7 1.56 then 1.24); levels-data.md item 2: the +15 STYLE rule cannot be met when the refusal is at 80-89 %.',
    'the NIGHT67 skip v2 (seeds 1-10 per run; STYLE +15 absolute)',
    ['`--seeds 20` prints the pooled median, each set\'s median beside it, `exercised N / 20`, `refused <verb> N` per seed and the ROOM list\'s M2 verdict line; a planted case each for the STYLE rule\'s two branches',
     'n68-round-verbs.sh runs skip v2 on seeds 1-20 pooled; n68-gate-pack.cjs\'s fresh skip runs `--seed 21 --seeds 10` under its timeout (its header and :234 say 11-20 today) and takes `--fresh <file>` (the r1 pre-run) only when that file\'s header room-file sha256 equals the round\'s room-files record; n68-red-bars.cjs reads the pooled ratio; a dry run of each on one room saved, the fresh one\'s command line printing `--seed 21`', 'every room of the slate re-red on I1-I4 (Logs/n68/red-<L>/<id>/), bars and ceilings written by n68-red-bars.cjs from the files, committed before r1', 'one critic PASS for I4 with the reds (Logs/n68/verdict-I4.md)']),
  room('kb3c', 'S', 'kiln', '3.3c THE TAP RUN (ride)', 'levels-data.md rank 1: every static item holds at r3 (lint 6/6, M3 GREEN), 1.06 vs 1.25; I1 removes the ride cost; then the tower ramp 7 -> 14 m (S.md).', 'NIGHT67 r3 kb3c (main: Design/levels/n67/kiln/rooms/kb3c.json); re-red on I1-I4'),
  room('kb2', 'S', 'kiln', '3.2 (parry: the THUNDERCLAP is the only gun)', 'levels-data.md rank 3: the parry room g7 could not be, because here the only gun is the THUNDERCLAP (the KESTREL comes at kb2b); GG 2026-10-12: "an understanding of parry mechanics".', 'the shipped kb2 (Logs/n68/red-kiln/kb2)'),
  room('kb3d', 'S', 'kiln', '3.3d (launch)', 'levels-data.md rank 6: a 480 m2 launch room whose lava has never killed the walk bot.', 'the shipped kb3d (Logs/n68/red-kiln/kb3d)'),
  room('g8', 'R', 'iceline', '2.8 THE STAMPEDE (ride)', 'levels-data.md rank 2: I1 removes its 14 s ride cost (47.4 vs 33.2 s); it still needs the ride as a shortcut between pockets (research.md: ride walls cross gaps, never on the fighting floor) and its M3 fails on a lone 150 hp charger.', 'NIGHT67 r3 g8; re-red on I1-I4'),
  room('v2', 'R', 'spire', '5.2 THE NAVE (launch), rebuilt from its blank rect', 'levels-data.md rank 5; gg-proxy-close.md: v2 was the shipped room with a patch, against GG 2026-10-09 "completely from scratch"; I3 first for its stall guard.', 'the shipped v2; re-red on I1-I4'),
  room('kb3', 'R', 'kiln', '3.3 THE PRESS HALL (launch), the hazard on the throw\'s line', 'levels-data.md item 7 and NIGHT67-REPORT section 7: every guard held at r3 (1.02 vs 1.35); the STYLE rule I4 changes is what made it unreachable; a hazard only where the verb throws is the untested idea.', 'NIGHT67 r3 kb3; re-red on I1-I4'),
];
// FORGE68 call (GG 2026-10-24 "you make the calls", DECIDE68 Q1 (a)): g7 with the game's existing room rule `limit: ["meleeOnly"]`
// (Campaign24.cs:35, 299; a field gen-slots.cjs accepts, gen-slots.cjs:110; shipped levels level2 / level07 / level10 use it)
const g7 = room('g7', 'R', 'iceline', '2.7 THE MIX (parry), melee only', "NIGHT67: 1.32 vs 1.39, every other line PASS; R's finding: the spit, the KESTREL and the returned glob share one aim line, so geometry cannot lift it (NIGHT67-REPORT.md section 1). The game's own melee-only room rule removes the gun's answer: parry becomes the ranged answer (research.md section 1: 'the other answer is weaker').", 'r3');
g7.doneWhen = [
  'the room file carries `"limit": ["meleeOnly"]` and a `limitReason` the player reads on entry (both existing room fields); nothing else of the rule changes',
  'the probe by 1:00 (R): a 1-seed planted parry skip on the assembled level with the limit: globs returned > 0 on the verb run. If the parry pilot cannot parry with the gauntlet in hand (code-scout.md (d)), T adapts it after I1 (a fix with its planted pair and a critic) and g7 enters at r2; if that is not GREEN by 5:30, g7 is SHORT with the probe line',
  'the ROOM list',
];
slices.push(g7);
// red team 12: a red is always the shipped room on I1-I4 (the round may start from NIGHT67's r3 file)
for (const e of slices) if (e.level) e.red = 'the shipped ' + e.id + ' on I1-I4 (Logs/n68/red-' + e.level + '/' + e.id + '/)' + (/r3/.test(e.red) ? '; round 1 starts from NIGHT67\'s r3 file on main' : '');
const lv = old.slices.find((e) => e.id === 'LV'), pc = old.slices.find((e) => e.id === 'PC');
const LV = JSON.parse(ren(JSON.stringify(lv)).replace('named n68-freeze', 'named n68-rc1').replace('equals Logs/n68/flow-base-a0.txt', 'equals the flow base current at the flip (Logs/n68/flow-base-i3.txt or later)'));
if (!/n68-rc1/.test(JSON.stringify(LV)) || !/flow-base-i3/.test(JSON.stringify(LV))) throw new Error('LV: a red team 8 / 12 edit did not apply');
const PC = JSON.parse(ren(JSON.stringify(pc)));
PC.title = "GG's play card: a play build of the night's best room per level, PASSed or not, labelled";
PC.why = 'GG has not played any room built after EXT53 (his last reaction 2026-10-20); two nights without a PASS gave him nothing to play (history.md). A labelled build of the best candidates lets his eyes, not the bots, judge.';
PC.doneWhen = [
  "NIGHT68-REPORT.md section 2: a play build (per level `node Tools/night/gen-slots.cjs --level <L> --slots=<id>,<id> --run --out=Logs/n68/play/<L>`, the ids taking their latest room files and the rest the base; then `bash Tools/night/n65-play-build.sh` with its `--config` / `--add-level n67.<L>67` options per its header, a data swap on the newest verified freeze build; hard links, never a full copy; only with 8 GB free) holding, per rebuilt level, every PASSed room and otherwise the night's best room by M2, each named on the card PASS or NOT PASSED with its numbers; its boot run once (`-skygrindSmoke` or a reel) or 'not run' with the reason; what to try in each room, one question per level, about five minutes",
  'DECIDE68.md holds the questions for his answers; the shipped game unchanged by PC (the rows stay as LV left them)'
];
const seed = {
  _why: "NIGHT68's slate (FORGE68, 2026-10-24; GG: 'follow same methods to build prompt for overnight, 8 hours'). Thirteen slices: I1-I4 (the instruments first: NIGHT67-REPORT.md section 7, history.md, levels-data.md), seven rooms on three levels (builder S: level 3 kb3c kb2 kb3d; builder R: level 2 g8, level 5 v2, level 3 kb3, and level 2 g7 melee only behind its probe: the forge's call at GG's 'you make the calls'), LV (branch A / A-lean only) and PC (always a labelled play build). A doneWhen is written before any measurement and never loosened; only the lead edits this file. Each room's verb, mainBar and ceilings are written by the lead from its red on I1-I4 before its first round is judged.",
  ROOM,
  slices: [...slices, LV, PC],
};
const out = path.join(__dirname, 'slate-seed.json');
if (fs.existsSync(out) && !process.argv.includes('--force')) { console.log('exists, not overwritten: ' + out); process.exit(1); }
fs.writeFileSync(out, JSON.stringify(seed, null, 2) + '\n');
console.log('slate-seed.json: ' + seed.slices.length + ' slices (' + seed.slices.map((e) => e.id).join(' ') + '), ROOM ' + ROOM.length + ' lines');

// FORGE70 (2026-10-25): writes Research/2026-10-25-forge70/slate-seed.json from NIGHT69's final slate (Logs/n69/slate.json: the rooms' bars and
// ceilings from the NIGHT68 reds, which Y2's re-run showed still stand) with FORGE70's changes written BEFORE any measurement: M3 on 20 probe
// seeds with the burst counted (DECIDE70 D-3, DECIDE69 Q-A), the GATE round run by the queue runner (Tools/night/n70-runner.sh), the card's
// launch target (spike.md), the verb-aware run as the faster of the plain and the committed pilot once Z3 PASSes (symmetric with the refusal),
// three tool slices (Z1-Z3), two launch rooms (kb3 R, v2 S), g8 parked (DECIDE69 Q-C), LV, PC (Builds/n70-play at the end).
//   node Research/2026-10-25-forge70/make-slate.cjs [--force]
'use strict';
const fs = require('fs');
const path = require('path');
const ROOT = path.join(__dirname, '..', '..');
const old = JSON.parse(fs.readFileSync(path.join(ROOT, 'Logs/n69/slate.json'), 'utf8'));
const ren = (s) => s.replace(/\bn69-(?!iceline|kiln|spire|foundry)/g, 'n70-').replace(/Logs\/n69\b/g, 'Logs/n70').replace(/NIGHT69/g, 'NIGHT70');
const blank = { state: 'todo', passes: false, verdict: null, spot: null, passSha: null, passFiles: null };

const ROOM = old.ROOM.map(ren);
const at = (p) => { const i = ROOM.findIndex((l) => l.startsWith(p)); if (i < 0) throw new Error('ROOM line not found: ' + p); return i; };
// the card: the launch target and how fresh bodies reach punch range (spike.md; scout.md A; design-research.md R1-R5, R7)
const c = at('an area card first');
ROOM[c] = ROOM[c] + "; **a launch card also states (FORGE70, spike.md): the target launches per verb-aware run (at least the red's median + 2: kb3 red 4, v2 red 2) and how the room brings FRESH husk-shaped bodies (unshot, above the 30 hp launch bound) inside 5.5 m of the player (short sight lines, a corner or a door 6-10 m away: design-research.md R7), and where each throw ends (a wall 1-12 m past the punch spot on the approach line, or a pit whose near rim is 0.5-6.9 m out, 2.5-6.5 m long, no body within 1.1 m of the line before it: scout.md A, design-research.md R1-R4)**";
const g = at('a GATE needs');
ROOM[g] = ROOM[g].replace('one ALONE main-tree round on the passed tools', 'one ALONE main-tree round on the passed tools, run as a job of the queue runner (`Tools/night/n70-runner.sh`; the job `bash Logs/n70/room-chain.sh <n> <id>`)')
  + '; **a GATE round starts after Z1-Z3 are each PASSed or SHORT (the tools at its start are the ones it is judged on: its slate-at-start.json); an earlier round is evidence only**';
const m2 = at('M2 VERB');
ROOM[m2] = ROOM[m2] + "; **FORGE70: once Z3 (the committed pilot) PASSes, the verb-aware run is the FASTER of the plain verb-aware run and the committed one, as the refusal is the faster of the refused-in run and the walker (symmetric; written before any measurement); the reds and bars are then re-made on that tool before any GATE round**";
const m3 = at('M3:');
ROOM[m3] = "M3: the room's verb pilot over 20 seeds (FORGE70 D-3, DECIDE69 Q-A; one seed was one sample: v2 s1b's crypt caught 15 of 20 probes where a1's seed 1 read FAIL): launch: `bash Tools/night/n70-throws.sh <the round's level> <id> <round>/throws-<id>.txt 20` (room-chain.sh runs it) prints `M3 candidates (D-3) N / 20`: a launched body that ends in a pit or lava, OR whose burst kills >= 2 (the flier included), on GREEN probes; PASS at N >= 10; ride: a kill from the wall (`n65verb ride --shoot`); parry: a returned glob kills a spitter; slide: the slide line's kill; pierce: >= 2 bodies";

const tool = (id, title, why, red, doneWhen) => Object.assign({ id, owner: 'T' }, blank, { title, why, row: 'none (tools: Tools/SimHarness, Tools/night)', red, doneWhen, visible: 'n/a (tools)', spotPose: null });
const room = (id, owner, level, verb, title, why, red) => {
  const o = old.slices.find((e) => e.id === id);
  return Object.assign({ id, owner }, blank, { level, verb, mainBar: o.mainBar, redRatio: o.redRatio, redSkip: o.redSkip.replace('Logs/n68/', 'Logs/n70/'), deathsAMax: o.deathsAMax, deathsBMax: o.deathsBMax, stallAMax: o.stallAMax, redNote: o.redNote,
    title, why, row: 'slot ' + id + ' of n67.' + level + '67', red, doneWhen: ['the ROOM list'], visible: "the room's verb piece reads at a glance", spotPose: "the GATE pack's iso pair" });
};
const pair = "a planted pair saved under Logs/n70/<slice>/ (RED on the t0 sha's code, GREEN on the fix), the five-level walk unchanged (flow-base compare) and one critic PASS (Logs/n70/verdict-<slice>.md)";
const slices = [
  tool('Z1', 'M3 on 20 seeds, the burst counted: the readers read the throw chart (node / bash only; by 0:40)',
    "NIGHT69: M3 was one probe on seed 1 and the burst counted for nothing; a launched husk always dies in its own burst (60 dmg, 3.5 m, the flier included: Launch36.cs:211-229), so a pit end needs a throw that hits nothing (scout.md A, B); the forge's 20-seed spike (spike.md): v2 s1b 20 / 20 candidates where a1 read FAIL, shipped kb3 7 / 20.",
    "NIGHT69 a1: `n65-verbs launch M3: kb3 FAIL` and v2 FAIL on one seed each (Logs/n69/r1-kiln/verbs.md, r1-spire/verbs.md)",
    ["both readers (Tools/night/n65-verbs.sh's launch M3 and Tools/night/n70-gate-pack.cjs launchM3) and Tools/night/n70-next.cjs's M3 piece read the round's throws-<id>.txt summary (`M3 candidates (D-3) N / 20`, PASS at N >= 10; no file: FAIL 'no throw chart'); the ride / parry / slide / pierce M3 unchanged; n70-m3-cases.sh gains the planted cases (a 14 / 20 chart PASS, a 7 / 20 FAIL, a missing chart FAIL, a RED probe's burst never counted); the probe itself unchanged", pair]),
  tool('Z2', 'The throw chart in skip v2 (print-only): every launch\'s start, end and outcome, and the falls counted (by 1:10)',
    "No line prints a skip run's landings (Harness.N65.Verb.cs:985 keeps kill bursts only) and skip v2 prints `falls 0` because it subscribes to stageRespawn only (:977) while the quench return emits fallReturn43 (Campaign24.cs:1038; n46flow counts it: Harness.N46.Flow.cs:233-234): NIGHT69's kb3 flow fell 52-78 times while every skip seed read falls 0 (scout.md A, E).",
    'none: no such line; falls 0 on every skip seed of Logs/n69/r1-kiln/verbs-skip.txt',
    ["a NEW line kind `n65verb skip launchend: room <id> | seed N | from (x, z) to (x, y, z) thrown D m | end <wall|body|timeout|landed> | kills K | in pit yes/no | in lava yes/no` per launch of the verb-aware run (not `launchkill`: the area lint and the deco checker read that), and each seed line's `falls` counting fallReturn43; every other line byte-identical (a 3-seed kb3 run before / after, the diff saved); room-chain.sh prints the median launches and launch ends per run from it", pair]),
  tool('Z3', 'THE COMMITTED PILOT: a verb-aware run that holds fire on launchable bodies so the verb is CHOSEN, not left over (by 1:40 or in Act 2)',
    "The verb-aware pilot shoots every husk it sees and launches only an unshot husk-shaped body above the 30 hp bound within 5.5 m and 50 deg (Harness.N65.VerbPilot.cs:212, :299-347): 2-4 launches a run against about 23 bodies (spike.md), so a room can only show what the gun left over. A player who plays the verb lets husks come. The M2 question is 'is the verb the better way', so the verb-aware side may play the verb on purpose, as the refusal side is the faster of two refusals.",
    'the plain verb-aware run: kb3 r1d 2.5 launches a run (Logs/n69/r1-kiln/verbs-skip.txt), v2 s1b 2',
    ["`n65verb skip ... --commit` (and skip v2's own second verb-aware run when the room's verb is launch): the inner autopilot does not fire at a husk-shaped body with hp above the launch bound within 12 m while a launch episode can start on it (the pilot closes and punches instead); everything else as the plain run; the M2 VERB line prints both verb-aware runs, uses the FASTER per seed, and prints which; the refused-in and walker runs byte-identical to the plain build's (the planted pair diffs them); on the shipped kb3 and v2 the committed run's launches per run printed beside the plain run's", "the reds (kb3, v2) re-made on the Z3 tool and their bars re-written (`node Tools/night/n70-red-bars.cjs --write`) BEFORE any GATE round, the change in each red's ratio printed (a red that rises raises its bar)", pair]),
  room('kb3', 'R', 'kiln', 'launch', '3.3 THE PRESS HALL (launch): the fresh body at the corner, the throw on the line',
    "NIGHT69 a1 1.08 vs 1.33 (red 1.13; fresh 1.23; nine layouts 0.94-1.12): the verb happened 2.5 times a run (red 4) because the bodies met the pilot on the far floor and were shot first (review-level.md; spike.md); M3 under D-3 already 14 / 20 on r1d, so the throw is not the blocker; the room check lost R5 / I4 (the press, the vent) and the flow fell into the quench 52-78 times.",
    'the shipped kb3 (Logs/n70/red-kiln/kb3/, bar 1.33; re-made on Z3 if it PASSes)'),
  room('v2', 'S', 'spire', 'launch', '5.2 THE NAVE (launch): the crypt that catches, the bodies that come',
    "NIGHT69 a1 0.99 vs 1.25 (red 0.87; fresh 0.94): the crypt caught the probe's throw on 15 of 20 seeds (spike.md) but the verb happened twice a run; the clear time was set by whether W3's TANK fell into the crypt (26-37 s) or was shot (47-76 s: S.md); the pads unused (deco FAIL); keep the TANK (D-10: enemy kinds are GG's).",
    'the shipped v2 (Logs/n70/red-spire/v2/, bar 1.25; re-made on Z3 if it PASSes)'),
];
const LV = JSON.parse(ren(JSON.stringify(old.slices.find((e) => e.id === 'LV'))));
Object.assign(LV, blank); delete LV.shortWhy;
const PC = JSON.parse(ren(JSON.stringify(old.slices.find((e) => e.id === 'PC'))));
Object.assign(PC, blank); delete PC.shortWhy;
PC.title = "GG's play card at the end: Builds/n70-play with the night's best kb3 and v2 (and NIGHT69's g8), each labelled PASS or NOT PASSED";
PC.why = "Builds/n69-play (NIGHT69: kb3 r1d, g8 r1f, v2 s1b, every room NOT PASSED) is GG's current build and has no reaction yet; NIGHT70 makes a NEW play build at the end (never writes n69-play), so GG can compare them.";
PC.doneWhen = [
  "by the close: per level `node Tools/night/gen-slots.cjs --level <L> --slots=<id> --run --out=Logs/n70/play/<L>` (kiln kb3, spire v2: the best of the night's rounds by M2; iceline g8 = NIGHT69's r1f), then `bash Tools/night/n65-play-build.sh n66-freeze n70-play Logs/n70/play/kiln/n67-kiln.json,Logs/n70/play/iceline/n67-iceline.json,Logs/n70/play/spire/n67-spire.json --config Design/unity-config.json --add-level n67.kiln67 n67.iceline67 n67.spire67`; one direct `Builds/n70-play/SKYGRIND.exe -skygrindSmoke -logFile Logs/n70/play-smoke.log` (the guard's disk rule covers the scripts only; it loads none of the rebuilt levels: say so); the tracked slots and rows never change for PC; Logs/ is gitignored: `git add -f`",
  "NIGHT70-REPORT.md section 2 names each room PASS or NOT PASSED with its numbers (M2, fresh, launches per run, M3 N / 20), what to try in it (about five minutes), what changed from Builds/n69-play, and the boot line; DECIDE70.md holds the questions; one critic PASS (Logs/n70/verdict-PC.md)"
];
const seed = {
  _why: "NIGHT70's slate (FORGE70, 2026-10-25; GG: 'can we use the same 2 methods to prompt another 8 hour overnight prompt'). Seven slices: Z1-Z3 (M3 on 20 seeds with the burst, the throw chart and the falls, the committed pilot), two launch rooms measured ALONE (kb3 R, v2 S; g8 parked: DECIDE69 Q-C has no honest payer under the walker rule), LV and PC (Builds/n70-play at the end). A doneWhen is written before any measurement and never loosened after one.",
  ROOM,
  slices: [...slices, LV, PC],
};
const out = path.join(__dirname, 'slate-seed.json');
if (fs.existsSync(out) && !process.argv.includes('--force')) { console.log('exists, not overwritten: ' + out); process.exit(1); }
fs.writeFileSync(out, JSON.stringify(seed, null, 2) + '\n');
console.log('slate-seed.json: ' + seed.slices.length + ' slices (' + seed.slices.map((e) => e.id).join(' ') + '), ROOM ' + ROOM.length + ' lines');

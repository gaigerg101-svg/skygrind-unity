// FORGE69 (2026-10-25): writes Research/2026-10-25-forge69/slate-seed.json from NIGHT68's final slate (Logs/n68/slate.json: the rooms' bars and
// ceilings, written by n68-red-bars.cjs from the final reds, 122f83ad) with FORGE69's changes written BEFORE any measurement: the STYLE
// rule (verb STYLE >= refusal STYLE - 5), the GATE round ALONE, a patch-share rule, three tool slices (Y1-Y3), three rooms, PC always,
// LV (branch C: short).   node Research/2026-10-25-forge69/make-slate.cjs [--force]
'use strict';
const fs = require('fs');
const path = require('path');
const ROOT = path.join(__dirname, '..', '..');
const old = JSON.parse(fs.readFileSync(path.join(ROOT, 'Logs/n68/slate.json'), 'utf8'));
const ren = (s) => s.replace(/\bn68-(?!iceline|kiln|spire|foundry)/g, 'n69-').replace(/Logs\/n68\b/g, 'Logs/n69').replace(/NIGHT68/g, 'NIGHT69');
const blank = { state: 'todo', passes: false, verdict: null, spot: null, passSha: null, passFiles: null };

const ROOM = old.ROOM.map(ren);
const at = (p) => { const i = ROOM.findIndex((l) => l.startsWith(p)); if (i < 0) throw new Error('ROOM line not found: ' + p); return i; };
// the GATE round: ALONE (only the measured room new in its level), the level review at its named time
const g = at('a GATE needs');
ROOM[g] = "a GATE needs M1-M4 and every GUARD held on one ALONE main-tree round on the passed tools (`L67=<L> ROOMS=<id> SLOTS=<id> bash Tools/night/n69-round.sh r<n>-<L>`, which Logs/n69/room-chain.sh runs: only this room new in its level, the others old, so it is measured against its red like for like: NIGHT68 saw one room file read 0.10 differently beside three other new rooms), skip v2 also on fresh seeds 21-30 (the GATE pack's fresh skip, or the alone round's pre-run it takes by --fresh; a pack whose fresh line names seed 11 is refused), the room file unchanged since that round, and that round read by a level review written at its named time (a fresh reviewer starts the moment the round ends: a round without one holds no GATE); a room without such a round is SHORT";
// M2: the STYLE rule becomes 'verb-aware STYLE no lower than the refusal's STYLE minus 5 points' (written before any measurement; GG's DECIDE68 Q8 unanswered: the forge's call)
const m2 = at('M2 VERB');
ROOM[m2] = "M2 VERB: skip v2 on the room's verb, POOLED over seeds 1-20 (the median over the exercised seeds; the per-set ratios printed beside it), prints a ratio >= the room's mainBar (written from its red: max(1.25, red + 0.20)) with >= 14 of 20 seeds exercised, the refusal fired and the refused run NOT using the verb on any exercised seed; **STYLE: the verb-aware run's STYLE above DULL >= the refusal's - 5 points** (FORGE69: NIGHT68's min(refusal + 15, max(refusal, 95)) asked 91-94 % on levels 3 / 5 where the verb runs reach 79-82 %: unreachable; the new rule says the verb must not make the run duller; it is written here, before any measurement); a ride room whose ratio is in [mainBar, mainBar + 0.10) is UNSURE and does not PASS M2 (so a ride room needs 1.35); on the fresh seeds 21-30 the median ratio >= 1.25 and >= 7 of 10 exercised";
// M4: the patch share
const m4 = at('M4:');
ROOM[m4] = ROOM[m4] + "; **a rebuilt room keeps at most 25 % of the shipped room's pieces** (`node Tools/night/n69-patch-share.cjs --level <L> <id>`: geometry-equal boxes, ramps, pads and pits of Design/levels/n67/<L>/rooms-base/<id>.json: gg-proxy's #1 'the old room with a patch'; inside the slot's fixed rect and doors), and it keeps every verb and every pad / ride face the shipped room offered unless the card says what replaces it (nothing the player had is lost: air65's flights handed on)";

const tool = (id, title, why, red, doneWhen) => Object.assign({ id, owner: 'T' }, blank, { title, why, row: 'none (tools: Tools/SimHarness, Tools/night)', red, doneWhen, visible: 'n/a (tools)', spotPose: null });
const room = (id, owner, level, verb, title, why, red) => {
  const o = old.slices.find((e) => e.id === id);
  return Object.assign({ id, owner }, blank, { level, verb, mainBar: o.mainBar, redRatio: o.redRatio, redSkip: o.redSkip, deathsAMax: o.deathsAMax, deathsBMax: o.deathsBMax, stallAMax: o.stallAMax, redNote: o.redNote,
    title, why, row: 'slot ' + id + ' of n67.' + level + '67', red, doneWhen: ['the ROOM list'], visible: "the room's verb piece reads at a glance", spotPose: "the GATE pack's iso pair" });
};
const pair = "a planted pair saved under Logs/n69/<slice>/ (RED on the t0 sha's code, GREEN on the fix), the five-level walk unchanged (flow-base compare) and one critic PASS (Logs/n69/verdict-<slice>.md)";
const slices = [
  tool('Y1', 'The STYLE rule: verb-aware STYLE >= the refusal\'s - 5 (by 0:20)',
    "NIGHT68: the +15 rule asked 91-94 % on levels 3 / 5 (Harness.N68.Skip.cs:10-12 V68StyleNeed; Harness.N65.Verb.cs:1223-1230, :1203-1207, :1081); under the new rule every recorded room passes STYLE and M2 rests on the ratio (scout.md X6).",
    'NIGHT68: `n65verb skip --style-cases` 8 / 8 on the old rule',
    ['V68StyleNeed, the M2 VERB line text, the comments and the planted `--style-cases` carry the new rule (verb 69.0 vs refusal 74.0 PASS; 68.9 vs 74.0 FAIL; NaN UNSURE; the old rule printed beside each); no ratio, bar or exercised count changes (the reds stand: n69-red-bars.cjs on the NIGHT68 reds prints the same bars)', pair]),
  tool('Y2', 'The M3 reader and probe: a pit counts only for a LAUNCHED body (by 1:00; it changes no ratio input, so round a1 does not wait for it)',
    "verdict-I2-r2.md: the probe sets inPit / inLava from the TARGET on any step whether or not it flew (Harness.N65.Verb.cs:326-339) and the readers grep 'in a pit YES' on any launch line (Tools/night/n65-verbs.sh:19-22, n69-gate-pack.cjs:188-192): kb3's RED probe printed 'M3: kb3 PASS' (Logs/n68/r2-kiln/verbs-launch.txt:21 / :23).",
    "NIGHT68 r2: a RED launch probe printed 'M3: kb3 PASS'",
    ['the probe keeps the first launched body id (Launch36.cs:133 emits it), tracks THAT body, sets inPit / inLava only from it and only when launchEv >= 1, prints "launched body <kind> #<id> (the target: yes / no)" and "in a pit n/a (nothing launched)" otherwise; both readers pass M3 only on a `: GREEN` line with `launch36 [1-9]` and a YES', 'RED: the saved r2 line through today\'s readers (kb3 PASS); GREEN: the same line through the fixed readers (kb3 FAIL: nothing launched); the control v2 (Logs/n68/r2-spire/verbs-launch.txt:11: GREEN, launch36 1, in a pit YES) stays PASS; then ONE red re-run on the n69 harness (`ROOMS=kb3 bash Tools/night/n69-red.sh 3 n57-kiln ...` + its skip) reads the same pooled ratio as Logs/n68/red-kiln/kb3/skip.txt (1.13): the reds stand', pair]),
  tool('Y3', 'The patch-share tool (by 0:45; node only: the builders take it by path from branch n69-T at once, the lead picks it to main between rounds)',
    "gg-proxy NIGHT68 #1: four of seven rooms kept 5-9 of the old room's blocks; no tool counts kept pieces (the reviewer used python); scout.md X5 counted: kb3d 73 %, kb3 56 %, kb3c 44 %, kb2 21 %, g8 8 %, g7 0 %, v2 0 %.",
    'none: no tool',
    ['`node Tools/night/n69-patch-share.cjs --level <L> <id>` prints the row (pieces kept / total, boxes kept / total, rect / spawn / exit same, waves same) and a last line `n69-patch-share: PASS|PATCH <id> (<share> % kept)` against 25 %; geometry-equal: boxes min / max, ramps from / to / width / rise, pads at / target, pits rect / floorY / lava; the job / why text ignored; reads Design/levels/n67/<L>/rooms-base/<id>.json (the shipped room) and rooms/<id>.json; planted: a copy of rooms-base as the room (100 %, PATCH) and the shipped g7 / v2 (0 %, PASS); its table equals the scout\'s for the seven NIGHT68 rooms', pair]),
  room('kb3', 'R', 'kiln', 'launch', '3.3 THE PRESS HALL (launch), rebuilt from the blank rect',
    "scout.md B rank 1: the one room where the verb clearly pays (verb-aware 44.5-46.9 s vs refused 57-58 s; 79-89 launches), gap -0.11 alone (1.22 vs 1.33); its 1.5 m tanks never end a body (pitDropM 1.5): a pit of 2 m or more on line65 adds a kill the refusal cannot get; 56 % of the shipped room kept (gg-proxy #1); the four presses' job (the charger's stun) gone (gg-proxy #4).",
    'the shipped kb3 (Logs/n68/red-kiln/kb3/, bar 1.33)'),
  room('v2', 'S', 'spire', 'launch', '5.2 THE NAVE (launch), the pit deeper and the ride face and pads back',
    "scout.md B rank 2: already from the blank rect (0 % kept), floor bar 1.25, measured +0.22 (1.09 vs 0.87 red, pre-I2-fix); a crypt deeper than 1.5 m on the throw line; the shipped ride face and two pads back (air65 0 / 0 vs 3 / 3; nothing the player had lost); stall A 21.5 vs 19.0.",
    'the shipped v2 (Logs/n68/red-spire/v2/, bar 1.25)'),
  room('g8', 'S', 'iceline', 'ride', '2.8 THE STAMPEDE (ride): the pocket only the ride reaches',
    "scout.md B rank 3: already a rebuild (8 % kept), one untested idea aimed at the measured cause (the clear is kill-bound: every body comes to the near bank): R.md's W4 spitters behind a parapet facing the near bank, 12 m by ride vs about 28 m round; weakest: a ride room needs 1.35 (the UNSURE band) and the red 0.63 is held down by the ride fallback pilot.",
    'the shipped g8 (Logs/n68/red-iceline/g8/, bar 1.25)'),
];
const LV = JSON.parse(ren(JSON.stringify(old.slices.find((e) => e.id === 'LV'))).replace('named n69-freeze', 'named n69-rc1').replace('equals Logs/n69/flow-base-a0.txt', 'equals the flow base current at the flip (Logs/n69/flow-base.txt)'));
Object.assign(LV, blank);
const PC = JSON.parse(ren(JSON.stringify(old.slices.find((e) => e.id === 'PC'))));
Object.assign(PC, blank);
PC.title = "GG's play card, FIRST at t0 and updated at the end: a labelled play build of the best room per level";
PC.why = "GG has played nothing built after EXT53; NIGHT67 and NIGHT68 gave him no build (a usage stop ate NIGHT68's PC). So PC-1 is made at 0:15 from the files as they stand (NIGHT68's finals: kb3 r1d, v2 r1f, g8 r1f), before any other work, and PC-2 updates it from what the night improved; each room on the card is PASS or NOT PASSED with its numbers.";
PC.doneWhen = [
  "PC-1 by 0:25: per level `node Tools/night/gen-slots.cjs --level <L> --slots=<id> --run --out=Logs/n69/play/<L>` (kiln kb3, iceline g8, spire v2), then `bash Tools/night/n65-play-build.sh n66-freeze n69-play Logs/n69/play/kiln/n67-kiln.json,Logs/n69/play/iceline/n67-iceline.json,Logs/n69/play/spire/n67-spire.json --config Design/unity-config.json --add-level n67.kiln67 n67.iceline67 n67.spire67` (ONE comma-joined file argument; hard links: about 40 MB written); PC-2 by 2:20: the same with `--update` and the night's better files (the best of a1 / a2 by M2); the tracked slots and rows never change for PC; Logs/ is gitignored: `git add -f`",
  "NIGHT69-REPORT.md section 2 names each room PASS or NOT PASSED with its numbers, what to try in it (about five minutes), and whether the build was booted (a direct `Builds/n69-play/SKYGRIND.exe -skygrindSmoke` is the guard's call: the guard refuses the scripts under 12 GB; the lead says which in the log) or 'not run' with the reason; DECIDE69.md holds the questions"
];
const seed = {
  _why: "NIGHT69's slate (FORGE69, 2026-10-25; GG: 'follow those 2 same prompting methods ... a detailed 4 hour long prompt to build'). Eight slices: Y1-Y3 (the instruments' last three: the STYLE rule, the M3 reader, the patch share), three rooms measured ALONE (kb3 R; v2 and g8 S), LV (branch C: short) and PC (first at t0, updated at the end). A doneWhen is written before any measurement and never loosened; only the lead edits this file. The rooms' bars and ceilings are NIGHT68's final reds' (the reds stand: Y2's one re-run proves it).",
  ROOM,
  slices: [...slices, LV, PC],
};
const out = path.join(__dirname, 'slate-seed.json');
if (fs.existsSync(out) && !process.argv.includes('--force')) { console.log('exists, not overwritten: ' + out); process.exit(1); }
fs.writeFileSync(out, JSON.stringify(seed, null, 2) + '\n');
console.log('slate-seed.json: ' + seed.slices.length + ' slices (' + seed.slices.map((e) => e.id).join(' ') + '), ROOM ' + ROOM.length + ' lines');

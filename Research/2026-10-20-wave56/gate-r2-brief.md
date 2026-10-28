# NIGHT56 gate r2: the evidence for the batched critic (the lead's, 02:0x)

Build `Builds/n56-r2` from port/stage3 af050c9b (r1's code + the builders' first slices: L 0e3e6024 606d2c1f b4f3b543 93644530; G 0881ea58
bda83337 54cd7acb; V 174e2b7d 145fd17d 283cdc5f bf2dbee0 7afdb2ea). Its copy's config has ON: design.n55 grade55, wick55, step55, decor55,
kill55, brief55, clear55 and design.n56 rank56, colour56 (ssao55 off: closed DONE-no-defect at r1). An OFF side runs with `-skygrindOff55`
(NIGHT54's game) or `-skygrindOff56` (NIGHT55's game, rooms aside: rooms are level data and change on both sides). All paths are under
`C:\Users\gaige\dev\SKYGRIND-unity\`. Reel lines: `Logs/feel/n56-r2/summary47.txt`; full lines in `Logs/player/n47-reel-n56-r2-<side>.log`.
The r1 gate (Research/2026-10-20-wave56/gate-r1.md) is the previous verdict on the NIGHT55 rows; judge r2's evidence fresh.
Rubric and output: `Research/2026-10-20-wave56/critic-gate.md`. Write `Research/2026-10-20-wave56/gate-r2.md`. Images: only those listed
(at most 30 in all). The slates: NIGHT56 `Logs/n56/slate.json`, NIGHT55 `Logs/n55/slate.json` (doneWhen + notes per id).

Bot numbers are labelled bot. The lead's own bot runs on HEAD af050c9b: `Logs/n56/bot-r2/` (n56-room.py table + planted, identity, n46flow
x10 on the n52 levels with the style column), summary `Logs/n56/bot-r2.out`. Baselines at u3-n55 (rows off): `Logs/n56/flow-base.txt` /
`flow-base.out`, `Logs/n56/identity-base.txt`, `Logs/n56/wyrm-base.txt`.

## A. NIGHT56 entries

### A1. P0-pick (the lead's pick of NIGHT55's branches)
- doneWhen (Logs/n56/slate.json): every NIGHT55 branch commit picked (HANDOFF order) or skipped with a DECIDE line; gates GREEN; n56-r1 built.
- Evidence: `NIGHT56-LOG.md` (sections "A0 bearings", "P0: n56-r1"); `git log --oneline e7d5b59b..6b1c00c9` (the 13 picks + the shared
  commit); HANDOFF.md's NIGHT55 block (the list to pick); `DECIDE56.md` is untracked but readable at the repo root (D-3: 8288aec2 skipped,
  it is 3b71c610 already on the branch; D-4 the one conflict). Gate lines are in NIGHT56-LOG.md; `Builds/n56-r1/SKYGRIND.exe` exists.
- Rubric lines 2 and 5 (frames, not basic) do not apply to a pick: score them n/a, not FAIL.

### A2. L1 the room checker (Tools/night/n56-room.py)
- doneWhen: see the slate (R2 0 dead-end nodes, R5 a foothold, R8 every solid's job from the plans' job / job36, in view per opening;
  planted case FAILs a copy of one NIGHT52 room; prints a table over the 18 rooms).
- Evidence: `Logs/n56/bot-r2/room-table.txt` (the table on HEAD), `Logs/n56/bot-r2/room-planted.txt` (planted cases; the lead's run printed
  "planted exit 0" = every planted case FAILed). The RED table before L2 is in L's commit 0e3e6024's message (`git log -1 0e3e6024`). You may
  run `python -I Tools/night/n56-room.py planted` yourself. Note: L1 is a tool; the R5 / R2 rows it prints FAIL in many rooms: that is the
  rooms' state, not L1's (L1 passes if it measures; the rooms are L2 / L3's).
- No frames.

### A3. L2 spawns in view (level data: per-wave waveNodes43)
- doneWhen: n56-room.py in-view PASS on >= 15 of 18 rooms; I7 still PASS on every level; the a156 meter (distance AND line of sight) on
  foundry / iceline / kiln entry frames agrees with n56-room.py on >= 80 % of rooms; n46flow on the n52 levels: stall <= 25 % on foundry /
  iceline / kiln and no clear time worse than Logs/n56/flow-base.txt by more than 10 %; verbs37 x5 nothing lost.
- Evidence: `Logs/n56/bot-r2/room-table.txt` (IN VIEW line, I7 table); a156 sides l2f / l2i / l2k / l2s / l2v (the `[reel] a156:` lines);
  run `python -I Tools/night/n56-room.py agree Logs/player/n47-reel-n56-r2-l2f.log Logs/player/n47-reel-n56-r2-l2i.log
  Logs/player/n47-reel-n56-r2-l2k.log` yourself; flow (bot) `Logs/n56/bot-r2/flow.out` SUMMARY lines vs `Logs/n56/flow-base.out`;
  verbs37 sides vf / vi / vk / vs / vp (`verbs37: PASS: total` lines). Builder L's proposal for the stall line (the baselines are already over
  25 %: foundry 26.2, iceline 32.0, kiln 29.2): "no worse than the baseline + 3 points per level"; judge the ORIGINAL line and say what
  the proposal would give; the lead's call D-16 keeps the original as the gate.
- Frames (6): after / before for three rooms, the entry camera (frames52.entry) both sides:
  `Logs/feel/n56-r2/l2f/a156/shot-N29-L56-n52-foundry-a3-entry.png` vs `Logs/feel/n56-r1/d1off/a452/shot-N29-K52-n52-foundry-a3-entry.png`;
  `Logs/feel/n56-r2/l2i/a156/shot-N29-L56-n52-iceline-g2-entry.png` vs `Logs/feel/n56-r1/d2off/a452/shot-N29-K52-n52-iceline-g2-entry.png`;
  `Logs/feel/n56-r2/l2k/a156/shot-N29-L56-n52-kiln-kb2-entry.png` vs `Logs/feel/n56-r1/d3off/a452/shot-N29-K52-n52-kiln-kb2-entry.png`
  (n56-r1 = NIGHT55's rooms; the r1 a452 lines in Logs/feel/n56-r1/summary47.txt print "alive N, in view N" per entry frame).

### A4. G1 rank56 (sim row design.n56.rank56; ships OFF)
- doneWhen: see the slate ((bot) DULL share falls >= 30 points vs the baseline and a combat room reaches SAVAGE; StyleSim's golden records
  EQUAL off; golden / fingerprints EQUAL off; a256 shows the rank leaving DULL on a combat frame).
- Evidence: the style column in `Logs/n56/bot-r2/flow.out` (`STYLE:` / `PAYOFFS:` lines, rows off = the baseline); builder G's own row-on
  run is NOT evidence: if no row-on run by the lead exists, say "not evidenced". The gates (NIGHT56-LOG.md r2: golden, fingerprints). a256
  was not built (no FeelReel.N56.Play.cs on HEAD). The doneWhen's first line is the entry's honest test: rank56 scales payoffs, not style gain.
- No frames.

### A5. G2 kill55's fix (= NIGHT55 P1-kill)
- doneWhen (NIGHT56 G2): a355 GREEN (chain >= 3 kills inside 1 s, hit mark >= 5 % next frame); a blind pair (both orders) picks ON as "the
  kill hits harder" 2 of 2. NIGHT55 P1-kill's: the 3rd and later kills inside 1 s hold >= 0.08 s (inside the cap), the 1st 0.05 s; >= 8 debris
  per kill alive >= 1.5 s; a hit husk's body pixels change >= 5 % next frame; a354 jitter <= 0.191; notes: the hold cap move54's 0.12 s; a354
  camera-held total over 4 kill freezes <= 0.150 s off a chain and printed on a chain.
- Evidence: sides k2on / k2off (a355), m2on / m2off (a354, n52-foundry; m2off is -skygrindOff55 = no kill55) in summary47.txt / the Player logs.
  The fix (G's bda83337): chainGapS 0.45 -> 0.55 (the KESTREL's live interval 0.483 s), so a354's kills (0.483-0.492 s apart) now chain.
- Blind pair (4 images, the key held by the lead): `Logs/n56/pairs-r2-kill/pair-1-1A.jpg` + `pair-1-1B.jpg` (order 1), then `pair-1-2A.jpg`
  + `pair-1-2B.jpg` (order 2): the frame 1.5 s after a chain of kills. Question per order: "which frame shows the kill hitting harder?"
- Frame (1): `Logs/feel/n56-r2/k2on/a355/shot-N55-K-hit-next.jpg` (a hit husk, the frame after the shot).

### A6. V1 colour56 (view row; each level a KEY and an ACCENT; P0-decor folds into it, D-18)
- doneWhen: per level on a452 entry / mid frames (n56-colour.py): the key or accent at S >= 0.35 on >= 3 % of the frame; the accents
  pairwise >= 40 deg apart; sky pixels inside 5-70 deg at S > 0.35 = 0 %; nothing in the world above the tells' saturation; a blind pair both
  orders picks ON as "which has more colour direction" 2 of 2 (2 levels); walls keep value structure (a whole-frame tint FAILs); keys / accents
  never cyan, red-orange, yellow or magenta.
- Evidence: sides v1f / v1i / v1k / v1s / v1v (on) and v1foff / v1koff (-skygrindOff56), scenario a356; run
  `python -I Tools/night/n56-colour.py Logs/feel/n56-r2/<side>/a356 --kinds entry,mid` yourself on each (it prints the keys' / accents'
  hues and gaps, cover, sky, sat, tint per frame and per level).
- Blind pairs (8 images): `Logs/n56/pairs-r2-colour/pair-1-*` (foundry b1 entry) and `pair-2-*` (kiln kb2 entry), orders 1 and 2 as
  above. Question per order: "which frame has more colour direction (a level colour you would remember)?"
- P0-decor (NIGHT55, decor55): its fix is V1 (D-18: colour56 owns decor55's string course); judge P0-decor's doneWhen lines (accent share
  up >= 3x vs off, the accent >= 20 deg from the role hues, usable cyan the brightest class) on the same a356 frames where you can, else
  "not evidenced".

### A7. V4 the boss palettes' second round (= NIGHT55 P0-wickGrade, P0-hotWall)
- doneWhen (NIGHT56 V4): THE CROWN tells kept >= 0.8; THE WYRM's turn inside 15-55 deg; THE WICK's hot wall the most saturated region while it
  burns; its ride panels carry the cyan ride cue. NIGHT55 P0-wickGrade / P0-hotWall: as in Logs/n55/slate.json (and the r1 brief).
- Evidence: sides b1on / b1off (THE WICK), b2on / b2off (THE CROWN), scenario a155. THE WYRM has no a155 room (say "not evidenced").
- Frames (3): `Logs/feel/n56-r2/b1on/a155/shot-N55-wick-burn.jpg`, `Logs/feel/n56-r2/b2on/a155/shot-N55-crown37-p1.jpg`,
  `Logs/feel/n56-r2/b2off/a155/shot-N55-crown37-p1.jpg`.

## B. NIGHT55 entries re-gated on r2 (Logs/n55/slate.json for the full doneWhen / notes)

### B1. P0-smoke
- Evidence (new since r1): `Logs/n56/smoke-steal-n56-r1.out` (the steal now takes focus: "took True"), `Logs/player/n56-smoke-steal-n56-r1.log`
  (`[smoke] ... focus LOST` lines, the "input summary: focus changes" line, DONE). Run on n56-r1 with the shipping config (every row off);
  three builder agents were running (dotnet only). The focus-pause test outside -skygrindSmoke: Smoke55Tests 5 / 5 passed in the live
  Editor at 01:26 (NIGHT56-LOG.md; the test file Assets/Tests/EditMode/Game/Smoke55Tests.cs). No frames.

### B2. P2-clear (clear55)
- Evidence: a455 sides c55f / c55foff (foundry), c55k / c55koff (kiln), c55s / c55soff (span), c55v / c55voff (spire) on r2, plus r1's iceline
  (Logs/feel/n56-r1, c55on / c55off). The lines print the held frames, the hold on the loop's clock and the card text.
- Frames (4): `Logs/feel/n56-r2/c55f/a455/shot-N29-C55-n52-foundry-<room>-hold.png` (the first such file), the same for c55k (kiln), c55s
  (span), c55v (spire): list the folder and open one hold frame each.

### B3. P0-t125
- Evidence: side t1 (a152, n52-kiln): its "fire events" (THUNDERCLAP) line in `Logs/player/n47-reel-n56-r2-t1.log`; side t2 (a252, n52-spire)
  in summary47.txt. a152's own verdict line is RED for an unrelated hud52 item (the lesson chip at 8 s): judge P0-t125's line only.

### B4. P0-t99
- Evidence: the long reel `Logs/player/n47-reel-n56-r2-t99.log` (one Player run of verbs37 then a452 on n52-foundry: its `done:` line with
  the frames written and failed writes) and `Logs/n56/reel-t99.run`. Its frames are NOT pruned: `Logs/feel/n56-r2/t99/verbs37/` and
  `.../t99/a452/` (each phase numbers its own folder from f0000; count the files and check the indices yourself: the lead counted
  1800 files f0000-f1799 and 1007 files f0000-f1006). The done line prints no reused-index count.

### B5. P1-teachText (brief55)
- Evidence: r2 verbs37 x5 with brief55 on: run `node Tools/night/n55k-teach.cjs Logs/feel/n56-r2/vf/verbs37/telemetry.jsonl foundry
  Logs/feel/n56-r2/vi/verbs37/telemetry.jsonl iceline Logs/feel/n56-r2/vk/verbs37/telemetry.jsonl kiln Logs/feel/n56-r2/vs/verbs37/telemetry.jsonl
  span Logs/feel/n56-r2/vp/verbs37/telemetry.jsonl spire`. (The overlap fix was assigned at 01:25; it is not in r2 unless the lead says so.)

### B6. P1-doorDark
- Evidence: `python -I Tools/night/n55-r-flat.py Logs/feel/n56-r2/vf/verbs37 Logs/feel/n56-r2/vi/verbs37 Logs/feel/n56-r2/vk/verbs37
  Logs/feel/n56-r2/vs/verbs37 Logs/feel/n56-r2/vp/verbs37` (the kept frames: every 50th). No frames.

### B7. P0-g7g5 (iceline g7 / g5 re-cuts, level data)
- doneWhen: see Logs/n55/slate.json (identity g7 / g5 I1 < 60 %, nothing worse, contract 14/14 each, >= 1 piece with a named job; verbs37
  iceline 21/21; walk bot 10/10 on iceline; a fresh review of g7 / g5 frames names the piece and its job).
- Evidence: `Logs/n56/bot-r2/identity.txt` (g7, g5 rows), the vi verbs37 line (r2), the flow table's iceline rows in `Logs/n56/bot-r2/flow.out`
  (GREEN n/10 per room). The contract 14/14 print is not in the evidence (say so). The pieces and their jobs: `Research/2026-10-20-wave55/R.md`.
- Frames (2): `Logs/feel/n56-r2/l2i/a156/shot-N29-L56-n52-iceline-g7-entry.png`, `.../shot-N29-L56-n52-iceline-g5-entry.png`.

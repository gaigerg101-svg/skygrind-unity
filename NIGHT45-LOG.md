# NIGHT45 log (the arena identity night; NIGHT45.md; Research/2026-10-17-wave45/)

## G0 (19:05-19:10)
- 19:05:22 t0 (GG's /goal). Hard cap 21:05. No new words from GG since the forge; DECIDE43-45 unanswered (defaults stand).
- Premise: the Editor stopped (status playMode stopped); C: 26 GB free (`df`, > 20 GB: no deletion beyond tonight's own frames);
  only GG's four files dirty.
- 19:07 `node Tools/night/n45-levels.cjs`: design.n45 (foundry45 / iceline45 / kiln45 / look45 off), maps.list + the three
  placeholders (byte-identical copies), N45Levels.cs, N45View.cs, FeelReel.N45.cs, N45SharedTests, nine patched files. Added the
  three StreamingAssets level .meta files (mkmeta.py) and Tools/night/n45-gates.sh (n43-gates with --off45).
- 19:08 gates GREEN: config copies identical; golden 39 sets / 48094 records, 22 / 22 exact, 0 mismatches; fingerprint EQUAL plain
  and --off45; the Editor compile 0 errors, 0 console errors. N45SharedTests 2/2, Lesson25Tests 10/10 in the Editor.
- 19:09 shared commit bc0c8f74. 19:10 builders A1, A2, A3, S1, C1 launched (worktrees from bc0c8f74; cards end ~20:20).
  integrate.sh's map set (Logs/night45-picked.txt).

## P0 the suite (19:10-)
- 19:09:47 Core started (`n42-suite.sh n45 Core`, Logs/n42-suite-n45.txt), then the Game batches NIGHT44 missed (Logs/n42-suite-n45g.txt).
- 19:15:06 **Core 1114 / 1114 passed, 0 failed** (21 letter batches, 5 min 19 s). Game MapP..Z started 19:15:06.
- 19:25:12 **Game MapP Me Mo N1 N2 N3 P R Sa Se Sh Sk Sl Sm Sp St Ta Tel Tex To Tr V W Z: 389 / 389 passed, 0 failed** (T-70: with
  NIGHT44's A-M9 / MapC / N4 / Lesson / C / U the whole suite has now run; no NIGHT43-44 failure to fix).
- 19:25 C1 picked (0af8eea5..3664de62: T-62, RETIRED.md, the harvest 40-44, ROOMS L4 / L5). 19:27 the Editor compile 0 errors; **Lesson
  6 / 6** (was 5 / 6: Lesson25TextTests' prompt now 6 words): T-62 closed.
- 19:28 the smoke control on n44-r1 (NIGHT=n45, Logs/player/n45-smoke-n44-r1-ctl45.log). 19:31 **"DONE: 4 step(s) failed"**: virtual
  pad, virtual pad menus, pad only, grind22 (all four read the pad: "A played False", "the d-pad reached the MODE row False"); every
  other step PASS (tape EQUAL 485 steps, autopilot 30 / 30 hashes, attract 12 / 12, drydock 60 / 60). Cause found: `Get-PnpDevice
  -PresentOnly` lists a **"Pro Controller"** (a real pad present; memory batch-build-and-editor-stall: a real gamepad breaks the smoke's
  pad steps). The lead cannot unplug it: tonight's smoke is judged against this control (the same four pad steps fail, nothing else).

## Round 1 (19:33-19:46)
- 19:33 A3 report: THE RING's fight is in the worm's own arena (Campaign24.Enter), so the card is THE KILN'S HEART for wyrm45-arena;
  K1's 30 boxes kept in order (crumble 27 / 28, keep 23 valid), three slag plates added (each never reached by the worm in 3 fights,
  closest 1.9-2.0 m); 24 of k5's 76 props kept with a reason, 52 cut (34 generic trim, 18 never seen). Harness before / after (3 seeds):
  killed 3 / 3 both, 134.3 / 185.5 / 161.1 s, identical; the bar valid every live step; **the bot band 30-60 s missed both (median
  161.1 s): not an arena number (the worm's HP / legs; next night)**.
- 19:37 A3 picked (bfcb588f..6a90f643; ROOMS.md conflict with C1: both appended sections, kept both by hand). S1 picked
  (889c51a4..0af5b30f: Core/NightRows.cs unwired, tests, docs/NIGHT-ROWS.md, rows-switch-on.cjs for NIGHT46). Duplicate-key check: 0
  in unity-config, n45-kiln, wyrm45-arena. 19:39 gates GREEN (golden 0 / 22, fingerprint EQUAL plain and --off45, the Editor 0 errors);
  NightRows tests 14 / 14 and N45Shared 2 / 2 in the Editor (the new files seen).
- 19:40 build **n45-r1** (0af5b30f; DLLs 19:40:40; drift only GG's two ProjectSettings files). kiln45 on in its copy.
- 19:41 reel a345 (n45-kiln): **RED on the crumble only**: wyrm45-arena built (33 boxes), look43 slag plates 4 / 4, the kiln mouth 1,
  perch lips 8, ride strips 3, hot zones 11; the crumble's tell drawn (60 frames) but fallen 0 / 2: the crumble-tell frame shows the bar at
  ~75 %, over the crumble mark, so the perches were restored (the reel's hp cheat, not the arena); 0 NRE / 0 exceptions. Looked at:
  entrance (THE KILN MOUTH's ring on the west wall, the hot-floor grid, slag plates orange), mid (the ring overhead, the yellow
  column), the crumble tell (the red-orange perch). Seen: level 3's lesson card "STEP 1 · PRESS SPACE TO JUMP" stays up over the boss
  fight in all three frames (stale card or the reel's teleport: not checked). 19:45 A3 resumed to fix the reel's cheat.

## Round 2 and the final round (19:46-20:09)
- 19:42-19:46 verbs37 on n45-r1: level 4 n40-span 30 / 30, level 5 n41-spire 30 / 30 (0 NRE).
- 19:46 A2 report (THE THAW LOCK: the coal grate of three lava pits split by two 3 m bridges, THE BOILER as the landmark, wave 3 from the
  door side; cut after the walk: the ice apron (the bot slid in 250-430 times), the 0.9 m kerb (12 deaths)); walk g7 10 / 10, median
  66.3 s, 2 deaths, 0 burns (n43: 10 / 10, 61.8 s); g3 not started. Picked clean (3df2b2d1..6bd7b6b6). Gates GREEN 19:48; build
  n45-r2 19:49 (not used: A1 landed first).
- 19:49 A1 report (THE SPILL: a tipped ladle's slag river wall to wall, two 3 m bridges, the crane rail beside them, the ladle as the
  landmark, wave 3 from the near bank); walk b1 10 / 10, median 63.7 s, 10 deaths (n43's b1: 51.2 s, 0); the rail crosses 20 / 20, the
  rim jump-off 10 / 20; arena42's grinder and cover gone from b1 (DECIDE45 D-6). A3 round 2 (0f641f82): the reel's crumble was undone by
  bosses33's still armor (half of a drop handed back while the player stands still); the reel now moves first.
- 19:50 A1 picked (ROOMS conflict with A3's section: one heading kept, both cards kept) + A3's fix. Duplicate keys 0; copies identical;
  gates GREEN 19:51; build **n45-r3** 19:52 (DLLs 19:52:38); foundry45 / iceline45 / kiln45 on in its copy.
- 19:53-19:55 reels on n45-r3, all GREEN, 0 NRE: a145 4 / 4 frames (alive 3 at wave 2, wave 3 from the near bank); a245 8 frames, the
  grate burned and killed a placed swarmer; a345 the crumble fell 2 / 2. Looked at each entrance + mid in colour and greyscale
  (Logs/shots/n45/sheet-a1/a2/a3.png): the ladle's lit lip and lamp, THE BOILER ahead of the door, the KILN MOUTH ring; mid-fight a
  spitter's glob off the rim, the grate field with the tank beyond, the perch tell then fallen. Still seen: a345's stale "STEP 1" card
  (A3: the reel teleports past k5, so kb1's card stays up: a reel artifact).
- 19:56 the freeze c44fda35: foundry45, iceline45, kiln45 ON (n45-rows-on.cjs; the build's copy already equal); gates GREEN.
- 19:56-20:00 verbs37 on n45-r3: **L1 n45-foundry 26 / 26, L2 n45-iceline 20 / 20, L3 n45-kiln 25 / 25**; 20:05-20:08 **L4 30 / 30,
  L5 30 / 30**; 0 NRE everywhere. 12377 numbered frames of tonight deleted (C: 16 -> 20 GB).
- 20:00 the smoke on n45-r3 (rows on): "DONE: 4 step(s) failed": the same four pad steps as the n44-r1 control (the Pro Controller
  present), every other step PASS (rounds, buys, the box, PROTOCOL, EXFIL, tape EQUAL, autopilot 30 / 30, drydock 60 / 60, ghost),
  0 NRE. n45-r3 not promoted (the smoke not all passed); see n45-r4 below.
- 20:05 leftovers: A1 the KNOT YARD card (p3 -> THE TIE-OFF, docs only; picked 9e319d62); A3 look45 (off) until 20:35.

## The last round on n45-r4 (20:09-20:27)
- 20:09 A3's look45 picked (658130fc: Game/Look45.cs + 6 lines in Look43View.cs; the row off). Gates GREEN 20:12; build **n45-r4**
  20:12 (DLLs 20:12:51; the three rows on in its copy, as in the repo; look45 off).
- 20:13-20:15 a145 / a245 with look45 off then on: all GREEN, 0 NRE; b1's entrance off vs on differs in 1012 px and nothing is
  visible on the sheet (Logs/shots/n45/look45-b1.png): **look45 stays OFF** (no screen change measured; T-74).
- 20:16-20:21 verbs37 on n45-r4: **26 / 26, 20 / 20, 25 / 25, 30 / 30, 30 / 30**, 0 NRE.
- 20:22 the smoke on n45-r4 (idle 197 s at the start): **"DONE: all steps passed"** (227 s, 94 PASS lines, 0 NRE). The "Pro
  Controller" is still listed by Get-PnpDevice after it: the earlier pad failures' cause is not checked (T-73). **Play build: n45-r4.**
- 20:26 Play mode's TMP rewrite of Archivo-ExtraBold SDF.asset (_ScaleRatioA / C only) read and restored. Close: the report, HANDOFF,
  THREADS (T-58 / T-62 / T-70 closed; T-72-T-76 new), ledger wave-45, CORE's state line; night-context close: "every SHORT and open item
  landed"; 56 old untracked prompts moved into `nights/`; the stray `Logs$TAG-tests.*` deleted.

## Close table (20:27, `date`)
| card | state | measured | date |
|---|---|---|---|
| G0 | DONE | shared commit bc0c8f74 (n45-levels.cjs); gates GREEN (golden 0 / 22, fingerprint EQUAL plain and --off45, Editor 0 errors); N45Shared 2 / 2, Lesson25Tests 10 / 10 | 19:09 |
| P0 | DONE | Core 1114 / 1114; Game MapP-Z 389 / 389; Lesson 6 / 6 after T-62; the smoke control on n44-r1 failed the four pad steps only | 19:31 |
| A1 | DONE | b1 -> THE SPILL ON: card first; walk b1 10 / 10 (63.7 s, 10 deaths vs 0 on n43); a145 GREEN 4 / 4 on n45-r3 and r4; frames looked at (colour + grey); + KNOT YARD card for NIGHT46 | 20:08 |
| A2 | DONE (g7) / SHORT (g3) | g7 -> THE THAW LOCK ON: walk g7 10 / 10 (66.3 s); a245 GREEN, the grate killed a swarmer; frames looked at. g3 THE SWARM not started (cut first by the card's order) | 19:57 |
| A3 | DONE | THE KILN'S HEART ON: wyrm45-arena, 3 plates never reached, 52 / 76 props cut; a345 GREEN, the crumble 2 / 2; look45 built, kept OFF (1012 px, nothing visible). SHORT inside: the worm's bot fight 161 s median vs 30-60 s (T-72, not the arena) | 20:15 |
| S1 | DONE | NightRows unwired + docs/NIGHT-ROWS.md; NightRows tests 14 / 14 in the Editor; fingerprint EQUAL plain and --off45 | 19:40 |
| C1 | DONE | RETIRED.md (12 rows + 6 GG decides), T-62 (5 places), harvest 40-44, ROOMS L4 / L5 | 19:27 |
| verbs 1-5 | DONE | n45-r4: 26 / 26, 20 / 20, 25 / 25, 30 / 30, 30 / 30, 0 NRE (n45-r3 the same) | 20:21 |
| the smoke | DONE | n45-r4 "DONE: all steps passed", 0 NRE (n44-r1 control and n45-r3 failed the four pad steps: cause not checked) | 20:26 |
| memory | DONE | ledger wave-45, THREADS w45, CORE state line; night-context close "every SHORT and open item landed" | 20:27 |

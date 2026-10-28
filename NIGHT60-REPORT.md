# NIGHT60 report: the overnight run (2026-10-21 23:05 to 2026-10-22 ~07:50 MST)

For GG. The night ran your "Overnight Run" template as NIGHT60.md: the lead plus two builders, every change verified before it counted,
every row turned on only by the shipping rule (NIGHT57.md section 0) with a fresh critic. The usage limit stopped everything from about
03:00 to 04:01; the work was committed and resumed.

## The short version
- **Three rows turned ON tonight:** kill55 (the kill wound now draws near-black), rim56 (enemies lifted over the world), and focus60 (a new
  fix: the game now pauses when its window loses the focus; before tonight a Player never paused on alt-tab).
- **Level 3 (THE KILN) got three fixes** in its shipped data: the respawn in THE CLINKER BRIDGES faces the fight, a dead-end pocket at the
  head of THE TAP RUN is opened, and THE BLOW PIPE pad no longer sits on the exit line.
- **Not shipped, with the numbers:** yellow56 (no colour rule separates a spitter's parry tell from spire's world yellow: a mask is needed,
  proposal P1), the exit lamp lamp60 (it draws, but barely changes the door: 2-14 %), level 4's room split (two contract items are your
  calls), idle60 (a real cause found for iceline's idle drones, but a guard failed: on its branch only).
- **The play build: Builds/n60-freeze** (the repo at ed5624dd; its config is the repo's): the final gates all GREEN, verbs37 27 / 29 / 31 / 31
  / 31, the smoke with two planted focus steals "227.9s DONE: all steps passed".

## The first thing to look at
Play Builds/n60-freeze on level 3 (THE KILN) and alt-tab once mid-fight: the game should stop on the pause screen. Then look at an
enemy up close with rim56 on: it reads brighter against the world; tell us if it reads too flat (D-14 measured it, but a person decides).

## PLAY notes (three lines each)
- **kill55, the kill wound (ON).** Kill an enemy with a hit that leaves a mark and watch the body before it breaks.
  The wound is now a near-black hole with cracks (it used to be a red-orange smudge the x-ray drew over).
  The wound is large (about a fifth of the body half a second in) and covers the hit's spark for a few frames.
- **rim56, enemies over the world (ON).** Walk into any level-1 or level-3 room with enemies in it.
  Enemies have a brighter lit side and a thin warm edge; the world's accent strips glow at about half their old brightness.
  Rooms read a little darker; up close a few bodies may look flatter (measured: the median body barely changes).
- **focus60, the focus pause (ON).** Start a run, then alt-tab or click another window during a fight.
  The game stops on the pause screen; Esc, START, B or RESUME carries on.
  Any pop-up that takes the window's focus will pause a fight too (that is the rule's point; say if it annoys you).
- **Level 3: THE CLINKER BRIDGES respawn.** Die in the second room.
  You come back facing down the middle channel with wave 1 ahead (before: facing a wall 2-4 m away, 0 of 5 enemies in view).
- **Level 3: THE TAP RUN's pocket and THE BLOW PIPE.** Fight in the fourth room and leave through its exit.
  The corner between the tap hood and the run wall is open on foot now; the blow pipe pad sits on the furnace top, not in front of the door.
  Your call: reaching the fettlers' walk by pad now takes two launches from the furnace top (the far end lost its way up).

## What shipped (by task; one line per commit that changed the game, its data or a tool)
- B2 kill55: cd05c630 the cause (the enemy x-ray, queue 3005, drew over the marks); 3524e84e test first; 284a4783 the marks draw at queue 3006;
  3e396a2e ON by the rule (marks at the flash end 0.42 % off -> 15.29 % on; critic PASS gate-b2.md).
- B7 rim56: 3f5c9509 cause; c658cdba / 2771de00 the string course takes the accent dim; e6e11996 / 5cddef27 lit gain 1.3; 5d51b7be /
  4d6a6f62 / 4669d2be lit gain 1.1; ba80cfee ON by the rule (bodies over the world 75.7 -> 90.1 %; critic PASS gate-b7.md; flow EQUAL).
- B9 / B18 focus60: b43e1490 / b016ca87 the Player focus probe; ff57b3c5 test first; c6a6e196 PausesOnFocusLoss behind the row; 0d3589c8 the
  row; 224d8338 a comment fix; 228e860b the probe's steal timing (L-115); f923bf57 ON by the rule (DONE True on / False off; critic PASS gate-b18.md).
- B13 33ef339f kb2c's respawn yaw 45 -> -10 and wave 1's nodes 2 m south (seen 0 -> 5 of 5 from the spawn; critic PASS gate-b13.md).
- B14 612530f0 / bdb2ec0b kb3c's pocket opened (NO PATH 4774 lines -> 0 over 30 seeds; deaths 8 -> 0; critic PASS gate-b14.md).
- B15 8419921a the blow pipe onto the furnace top (put-ats 0 / 1 / 1 -> 0 / 0 / 0; critic PASS gate-b15.md).
- B3 (instrumentation kept; yellow56 OFF): 076310a5 / 89803e51 spitters and swarmers report to the probe, its cap 8 -> 32; dee75beb the
  yellow meter; f6133f7e n60-build-num.cjs; e65d4275 the meter's fold bound (B16).
- B8 (lamp60 OFF): 563795e2 / db599f4c the lamp behind the row design.n60.lamp60 (was a compile-time constant); 9fec7fb5 / 7ce08c33 the
  clear shot; e51de119 the lamp meter; 51e4b04f / 338cfa30 the lamp at the door's face, not inside its 4 m walls.
- Tools and reels: a1e6216c the night's gates / reels / prune; 5e64a858 a157's spawn shots (B1); bd39383d the flow geometry off the
  spawn's yaw (B12); ea370eaa the prune fix (L-113); be31dac3 the flow baseline re-made after level 3's changes.
- B4 (level 4, OFF): abd32586 THE GAP NEST's slot split in gen-n58-span.cjs (contract 11 -> 12 of 14; the shipped level 4 unchanged).
- Notes and measures: ae193fcf (B6 kb3c's cause), 2026a6f4 (B24 the PULSE pips' hue), 10adcfc8 (B21 T-91 not reproduced), 7a2d31cd /
  1ba724c1 / 6b8b5834 (the audit), 18d173b0 (proposals P2 / P3), 3e9050ee (critics, the review and the evidence).

## Blocked, and why
- **B3 yellow56** (3 attempts): at every keep bound either the MAIN or the halo guard fails, because a spitter's parry tell and spire's world
  yellow share hue, saturation and value. Next: a stencil mask on the tells (P1, L).
- **B8 lamp60** (2 attempts, ~3 h): the clear shots are real clears now and the lamp stands in front of the wall, but its own gain on the door
  is 1.02-1.14x; 4 of 5 rooms pass the 1.5x MAIN with the lamp, 2 of 5 already pass without it. Next: a brighter or larger lamp, or a MAIN
  written as the lamp's own gain.
- **B4 / B5 level 4** (span58 OFF, D-8): THE GAP NEST's split reaches 12 of 14; the two left are yours (below).
- **B10 idle60** (branch n60-b10, D-13): the cause of iceline's idle enemies is drones frozen mid-climb (campaign drones never get NIGHT20's
  climb rules); the fix halves the drones' idle (32.6 -> 5.5 %) but the MAIN missed (15.6 / 18.4 % vs <= 10 %) and deaths rose 6 -> 9 on one set.

## Your calls
1. Level 4, THE GAP NEST: a ride wall that does not cross the gap (or waive that item for this card), and a flyer room above the 16 m
   height band (or change how drones treat a roof, or lower the far deck) (NIGHT60-LOG Blocked; DECIDE60 D-8).
2. THE BLOW PIPE: the pad now launches from the furnace top (two launches to the walk); keep, or find it another floor spot (gate-b15.md).
3. The PULSE gauntlet's HUD pips are pink (hue 332, 2 deg outside the meters' RIVAL magenta): keep, or move them to cyan (V-b24.md).
4. The 16 px text rule: every text is >= 20 px as a font size; if you mean the glyph height, the 20-22 px texts need a measure (audit-V A-V4).
5. yellow56 by a tells mask (P1, a render-pipeline change) or leave it OFF.

## Assumptions to confirm (DECIDE60.md, untracked)
D-10 t0 = the run's first `date` (23:05:36); D-11 who took what; D-12 B9 re-sized to M; D-13 idle60 kept on its branch; D-14 rim56 kept ON
after the review's computed terminator collapse (measured 0.50 -> 0.52 instead); D-15 five should-fix items from the code review wait for
NIGHT61 (the biggest: focus60 does not yet exclude the Player tours, so a focus loss can pause -skygrindShots / Strip / Tile / Hitch and the
DoorFree29 / LevelSelect29 checks: run them with -skygrindNoFocus60 until it is fixed; THREADS T-146).

## Proposals waiting for you (NIGHT60-PROPOSALS.md)
P1 (L) yellow56's tells mask; P2 (L) the pilots that climb THE WICK's stack and ride a wall in a Player (T-59 / T-66); P3 (M) a reel step
that takes a round-end card and a MODS draft card on the virtual pad (no run has ever taken a card on a pad, though the code allows it).

## The close (NIGHT60.md section 8)
- The final check command, 07:06-07:07 (Logs/n60/gates-final.txt): config copies identical; golden 39 vector sets, 22 replays (22 exact),
  0 failing, 0 mismatches; fingerprints plain / --off57 / --off58 / --off59 / --off60 EQUAL; the Editor "compilationFailed":false,
  "consoleErrors":0; `gates: all GREEN 07:07:42`.
- Builds/n60-freeze from ed5624dd (07:08; rows ON: kill55, rim56, focus60 and every earlier ON row; OFF: yellow56, lamp60, span58).
- verbs37 x5 (07:09-07:13, Logs/feel/n60-freeze/summary47.txt): foundry 27 / 27, iceline 29 / 29, kiln 31 / 31, span 31 / 31, spire 31 / 31;
  RED rows none; 0 NREs, 0 exceptions in all five.
- The smoke (07:14-07:18, Logs/n56/smoke-steal-n60-freeze.out): two planted focus steals took the foreground (+61 s, +201 s);
  `[smoke] 227.9s DONE: all steps passed`.
- B11, the Game EditMode suite in the live Editor (07:18-07:33, Logs/n41-suite-n60.txt): every class letter but M, 1006 / 1006 passed,
  0 failed. Game M, one class per batch (Logs/n41-suite-n60m.txt): M9Fade25Tests 4 / 4, then **MapChoiceTests TIMEOUT after 606 s**: the
  first time the class that has hung Game M since NIGHT41 is named; the other 9 M classes were not run (logged TIMEOUT, not waited out:
  the runner was stopped by its PID at 07:47; the Editor was never killed and may still be running that class: if it does not answer when
  you sit down, give it a minute or restart it yourself; THREADS T-147).

## Lessons added
L-113 (a copied prune script's pattern skipped tonight's side names), L-114 (a builder's heredoc patch), L-115 (never time a planted Player
action off the Player's log: it lands late).

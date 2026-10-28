# DECIDE59 (NIGHT59: SHIP WHAT IS READY; the forge wrote D-1..D-4, 2026-10-21 ~20:40-21:00; the lead adds D-5 on)

## D-1 The slate (the forge)
From NIGHT58-LOG's close table and its EXT section: the three entries one step from shipping (L0 cut 4, THE GAP NEST, yellow56's halo)
plus one deeper fix (kill55's marks). Rejected: rim56 and press57 (their next step is a cause hunt, not a ship: leftovers); a new level
slot or new features (NIGHT58 attempted 8 entries and shipped none: the run's minutes go to verdicts, not to more half-built entries).

## D-2 The forge experiment: rolling one-row critics
NIGHT58's one batched critic died twice and left every row without a verdict; the EXT's one-row critic (gate-x-brief.md) took 17 min and
returned a usable FAIL. Rolling critics, at most two alive at once, each started when its entry's Player evidence lands. Rejected: one
batched critic per build (25-47 min, a single point of failure); a critic per builder slice before Player evidence (it checks evidence).

## D-3 The forge's spike: THE GAP NEST's 61 falls are not a room bug (20:38)
`Logs/n58/ext-L/flow-n52-span-s1.txt`: n52-span already logs falls p1 10, p3 55, p5 42 per 10 seeds (the span's respawnY recovery is the
bot's normal on void rooms). So L1's falls GUARD is p2 within the level's void rooms' range (p3 52-55, p5 42-50), not "falls down".
Rejected: requiring falls 0 (no void room of the level meets it); dropping the guard (the drones could knock the player off: research
check 3, Research/2026-10-21-wave59/void-rooms.md).

## D-4 The forge's red team (a fresh reviewer, 20:44-20:58; 10 findings, all applied)
V2's fix went the wrong way (a higher vFull dims the halo more): split by saturation (sKeep). b2 moved to ~0:55 with reels and critics
entry by entry (a 1:05 b2 left no verdict by the cutoff); the cutoff 1:35 -> 1:40 and the freeze at 1:40 exactly. G1's MAIN regains the
a355 ladder (NIGHT57.md section 0 line 5). The halo meter committed as n59-v-halo.py (gate-x's halo3.py is not in the repo). L0 re-runs
its bot sets on the tree (the size-skip rule had no source). The off side's a157 reel on n58-freeze (it had none). The reel command named
(n56-reels.sh: prune loop, 6 GB floor). The wave59 folder committed before spawns (worktrees branch from HEAD). /goal needs a date
>= 1:55. Rejected: three critics at once (L-003's limit).

## D-5 A0 (21:11-21:19): the base held; the Editor was found in Play mode
`git tag -l` printed u3-n58; 04d8f2f3 in the log; `git apply --check L0-cut4.patch` printed nothing; C: 26 GB; the Editor ready. The first
build (21:17) was REFUSED: the Editor sat in Play mode (not started by this run). `bash Tools/unity-loop.sh stop` (Play mode only; the
Editor was not touched otherwise), then the build at 21:19. Rejected: building in Play mode (CLAUDE.md: never recompile in Play mode).

## D-6 L0 shipped into level 3's data (21:52)
Tonight's tree reproduced cut 4 bit for bit (n46flow seeds 1-30: put-at 1 / 2 / 5, deaths 1 / 7 / 1); Builds/n59-b1 verbs37 on n57-kiln 31/31,
a157 GREEN 5/5 with in view equal to the off side at every door; critic L0: PASS (gate-l0.md). Committed 4fdcf8e6. The critic's two
caveats go to the PLAY note and the threads, not to a hold: (1) no frame from the new spawn (a157 frames each room at its fixed entry pose),
(2) stall up 7-21 % relative on the same seeds, mostly in kb3c (a room the cut did not change), inside the cap set before the numbers.
Rejected: holding L0 for a spawn-view reel (no such reel exists; the rule's four lines are met).

## D-7 G spawned while L, V and the L0 critic ran (21:33)
Three builders plus one critic is L-003's limit, not past it; G had the longest Player path (a258 + a355) and needed the b2 slot.

## D-8 G1 kill55 stays OFF by its numbers; no critic (22:15)
b2 a258: marks at the flash end 0.46 % on, 0.46 % off, 0.46 % with NIGHT58's draw (-skygrindOldMarks59); want >= 5 %. The a355 ladder: ok on,
NO off (kill55's hold works). The rule's MAIN is both, so OFF. The probe (n59-g-probe.py): on is darker than off by > 0.12 on 5.89 % of
the body rect at the flash end, median V 0.33 (off 0.58): the marks are drawn and visible as a darkening but not near-black (the MAIN counts
V < 0.12). Next: the mark's own colour to near-black (a material number), re-measured. Rejected: redefining the MAIN after the numbers (L-100).

## D-9 V2 yellow56 stays OFF by its guard; no critic (22:19)
b2 a358: MAIN frames over 0.5 % non-parry yellow 11 / 12 off -> 0 / 12 on (met); GUARD the foundry a5 parry halo keeps 56.9 % of its yellow on
(90.4 % off) against >= 80 % written before the run: FAIL (NIGHT58: 20.1 %). The spire orbs 15.42 -> 17.29 % (kept). Off side vs NIGHT58: five
frames equal within 0.04 %, b1-mid (bodies in view) game-state noise; Cap57Pass reads the cap only when yellow56 is on (line 158), so the
keep bound cannot change the row-off game. Rejected: sKeep 0.75 (builder V's simulation: the halo 84 % but the MAIN over 0.5 % on two frames;
the simulation over-read the halo by 14 points tonight); lowering the guard.

## D-10 L1 THE GAP NEST stays out of NEW; span58 OFF (22:02)
Builder L's three cuts: p2's contract 7/14 -> 11/14, still FAIL on size (36 x 50 x 22 m; band 20-32 m long, 10-16 m high), density (0.72
pieces / 100 m2; >= 2.5) and the ride wall (none: the card keeps the pull as the only fast way across). The put-ats were all in p3 (not p2)
and are 0 / 0 after cut 1. Next: split the 50 m slot into a room <= 32 m plus an approach link (gen-n57-kiln.cjs's slot DE did that).
Rejected: a ride wall against the card (the card's spatial law is the pull; GG's call); shrinking the contract's bands.

## D-11 The freeze early (22:22, not 22:51)
After D-8 / D-9 / D-10 no entry awaited a verdict and no row flips, so n59-freeze-rows.cjs is not needed (L0 is data, already committed).
The freeze built from HEAD 83720e82 (gates GREEN 22:07:45 at that commit); the close keeps the 1:55 floor. Rejected: a b3 for a late fix
(no entry had a fix in hand whose numbers could pass before 1:40).

## D-12 GG's mid-run request: NIGHT60 from the "Overnight Run" template (22:3x)
GG: "dont follow this, take this but make it reated to this project, plan next night prompt using this as a base, output should be next
copty pastable goal pormpt". Read as: do not run the pasted template now; adapt it into the next night's prompt and hand back its /goal.
Done in the close's wait for the 1:55 floor (the NIGHT59 close unchanged): NIGHT60.md, NIGHT60-BACKLOG.md, DECIDE60.md. Adaptations
that change the template's meaning are logged in DECIDE60 (the branch, the revert command, the file names, the check command).
Rejected: starting the overnight run now (GG said "dont follow this"); stopping NIGHT59's close (the goal still needs its table and tag).

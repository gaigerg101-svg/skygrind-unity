# NIGHT66 builder S notes (b1 THE KNOCKOUT YARD, a8 THE SPILLWAY, a4 THE SPITTING GALLERY)

Tree: .claude/worktrees/agent-aeb9fc116ba05ef4d, branch n66-S from the t0 sha e37083cf. t0 = 2026-10-23 22:26:11 MST. Branch C (sim only:
no build, no Player). C: 8 GB free at 22:29 (`df -BG /c`).

## Verb plans (22:29-22:45)
Read: the builder brief, CLAUDE.md, PRINCIPLES.md, VERBS.md, NIGHT66.md, Logs/n66/slate.json, the forge's n65-data.md, research.md,
history.md section 3, DECISIONS.md, redteam.md findings 3 and 9, my NIGHT65 notes (Research/2026-10-23-wave65/S.md), the three room files
at t0, Logs/n65/gg-proxy-final.md, and the code the plans rest on:
- the verb-aware pilot's triggers (Tools/SimHarness/Harness.N65.VerbPilot.cs:177-246): one verb per room; pierce = two or more bodies within
  2 m of one line through a body in clear sight; parry = a glob within 0.9 x orb.reflectRange (1.8 m) of the eye;
- Launch36.cs:96: only husk-shaped bodies fly (never a spitter, a stand-in or a tank);
- Spitter.cs:175-176 (a tell needs ai.los) and :452-479 (the return homes straight on AimPoint and bursts on the first wall);
- config: spitter hp 60, minRange 6, maxRange 24, retreatRange 5, globDamage 12, puddle 3 m / 10 dps / 4 s, tell 0.6 s, cooldown 2.6 s;
  husk hp 40, aimHeight 1.0; punch.parryHeal 15; launch36 burst 60 in 3.5 m; pierce33 24 bodies; director.camp 10 m / 8 s, style drain x2;
- level 1's enemy kinds today: husk, swarmer (a3), spitter (a4, a5, b1); no charger or tank (D-12 keeps it so).

Calls I made (logged, not asked):
- **a8:** its verb65 is "pierce", so every skip v2 run walks; "the pierce from the ride" cannot be M1's. The plan makes the pierce line level
  down the lane and keeps P1's ride as the movement line and the flank. The charger stays GG's (D-12).
- **a4:** no wall passes the glob and stops the gun (the return flies the gun's own line), so the lever is economy and the burst (THE PAIR),
  and the ride only if its I2 >= 1.3. A first idea, a 1 m lip on the gallery's north edge, was dropped before writing: P5 THE FENCE already
  covers z -99.5..-106 at 4.2 m and a lip on z -106..-108 would block the gallery's drop beside d4 (R2).
- **The colliderless props:** a prop stays only if it is a pickup, a light's fixture (PRINCIPLES 7), a role-colour read of a danger or the
  exit (rule 8, PRINCIPLES 3), or the look of a collider box; the rest are cut in round 1 (b1 10, a8 10, a4 5).
- **The lead's T1a note (22:39):** tour65 WARNs a4 line65 6/8 (inside P3) and a8 frames65 `exit` (inside P8's wedge): both fixes are in the
  plans' round-1 lists.

## Round 1 (22:41-22:49, the usage stop 22:46-00:41, then 00:42-01:2x)
**Every skip line below is v1, not MAIN** (skip v2 is on n66-T, not yet in my tree). The ratio is my arithmetic on each file's seed lines
(Tools: my scratch `skipsum.cjs`): the median over the exercised seeds of refused / verb-aware, the v1 refused run being the plain pilot.
Each lever was measured alone against the t0 files on the same tool and seeds (`n65verb skip --verbs65 --seeds 10`, n46flow seeds 1-10);
every output opens with its command, HEAD's sha, the room file's blob and the time. The loop: my scratch `sgen.sh` (gen-n65-foundry.cjs
--slots=latest --run, levelgen --walk 0 ERROR every time) and `sloop.sh` (serial, one dotnet job at a time).

**Found while building:** Campaign24.SpawnWave (Campaign24.cs:910-927) picks each enemy's node at random from its wave's node list, so one
enemy can be placed on one spot only as its own wave's single node; and gen-n65-foundry.cjs refuses a `drop` key n52's row does not have.

| room | lever (folder) | ratio v1 (t0) | clear on / off s | STYLE on / off | walk bot A: deaths, stall | kept? |
|---|---|---|---|---|---|---|
| b1 | t0 (r1/t0) | 1.68 | 26.5 / 44.0 | 70.4 / 32.4 | 0, 31.8 | - |
| b1 | A: GATE FM closed + the prop cut (r1/A) | 1.48 | 29.8 / 41.4 | 69.8 / 27.7 | **1**, 29.8 | no: the join reverted (the cut kept) |
| b1 | C: wave 1 four husks (r1/C) | 1.20 | 30.5 / 35.5 | 63.8 / 55.6 | 0, 27.6 | no: reverted |
| a8 | t0 (r1/t0) | 0.73 | 22.2 / 14.5 | 49.8 / 47.5 | 0, 45.8 | - |
| a8 | A: THE HEAD SPITTER (wave 2 at the lane head) + the cut + the tour pose (r1/A) | 0.77 | 20.1 / 14.6 | 52.5 / 45.4 | 0, 41.0 | yes |
| a8 | A + B: wave 1 six husks (r1/B) | 0.83 | 18.6 / 15.5 | 52.4 / 46.5 | 0, 41.6 | yes |
| a4 | t0 (r1/t0) | 1.22 | 21.9 / 33.0 | 34.5 / 19.4 | 0, 25.3 | - |
| a4 | A: THE PAIR (a third wave-2 spitter, a high node) + the cut + the tour line (r1/A) | 1.23 | 23.7 / 30.5 | 42.8 / 14.1 | 0, 24.8 | no: flat ratio, and its full set (r1/final-pair) put a walk-bot death on seeds 11-20 (flow B deaths 1; both reds 0 / 0): reverted |
| a4 | A + B: THE FLUE RIDE (r1/B) | 1.31 | 26.4 / 25.6 | 30.3 / 16.3 | **1**, 25.4 | no: I2 1.12 FAIL (walk 9.0 / ride 8.0 m) and a walk-bot death: cut |

- **b1's prop cut changes no sim number:** at the round's files (t0 geometry, 10 props cut) the skip's seed lines and flow A are identical to
  t0's (`diff`, b1/r1/final against b1/r1/t0). Round 1 moved no M1 line in b1: both levers lowered the ratio and were reverted.

### Round 1's files and their full sets (`<id>/r1/final/`: room check, flows A / B / V, skip v1, the verb pilot, pads, air65, iso)
| room | what changed since t0 | skip v1 ratio (t0) | STYLE on / off | verb pilot | walk bot A / B: clears, deaths | stall V (t0) | room check | air65 |
|---|---|---|---|---|---|---|---|---|
| b1 | 10 colliderless props cut | 1.68 (1.68: seed lines identical) | 70.4 / 32.4 | launch GREEN by its own rule but 1 kill from line65[0] (bursts 1, hits 1): **M3's ">= 2 kills" not met** (as t0) | 10 / 10, 0 / 0 | 24.6 (24.6) | PASS | 59 hand-offs, 3 of 4 flights, 0 dead ends (= t0) |
| a8 | wave 2 at the lane head (THE HEAD SPITTER), wave 1 six husks, 10 props cut, frames65 `exit` moved | **0.83 (0.73)** | 52.4 / 46.5 (49.8 / 47.5) | pierce GREEN, **4 bodies, 4 kills** (t0: 3) | 10 / 10, 0 / 0 | 37.5 (37.5) | PASS (I2 1.92 / 1.64) | 4 hand-offs, dead ends 2 / 4 (R2, D4) (= t0) |
| a4 | line65's drop past P3's edge (the tour WARN), 5 props cut | 1.22 (1.22: seed lines identical) | 34.5 / 19.4 | parry GREEN, a second victim no | 10 / 10, 0 / 0 | 25.5 (25.5) | FAIL IN VIEW held (as t0, by construction) | 3 hand-offs, 0 dead ends (= t0) |

- **M1 moved in a8 only** (0.73 -> 0.83, v1, not MAIN), far under any bar of 1.25 or more. b1 and a4 end round 1 on their t0 numbers: every
  lever there was measured and reverted. **N10 clock:** b1 and a4 have had one round with no M1 move.
- **The guards** are read against my t0 runs (NIGHT65's final rooms on the same tools and seeds, the red65 stand-in) and Logs/n65/red (the
  n52 rooms): Logs/n66/red and red65 do not exist yet. All three hold clears 10 / 10 and deaths 0 / 0 on both seed sets; stall on flow V
  equals t0 in all three. a8's walk-bot stall on seeds 11-20 rose 42.9 -> 51.3 % (not a guard by the ROOM list; noted).
- **M4:** no box, ramp or pad added or moved in the round's files (the two geometry levers, GATE FM's close and THE FLUE RIDE, were reverted);
  n66-deco.cjs is not in my tree (T1c), so M4 is not checked. The round changed enemies, a pose, a line and cut dressing: honestly, no new
  piece with a job yet.
- **Not checked:** skip v2 (not in my tree), bodies per charged shot in the skip runs, spitter positions at the verb's moment, Player frames.

### The verb-plan critic (Logs/n66/plan-critic.md), answered by name
- **b1 B, "a job none of the four deco tests can see; placed where the bot stood it fits the bot":** accepted. B was not built in round 1.
  Round 2 builds it only where it blocks a wave-2 spitter's sight of a walk-line sample (test b) or where the verb-aware dash-in touches it
  (a P16-style sill), read on n66-deco; otherwise it is not added. **b1 A** (the critic's M4 note on the joined P12 / P13): moot, A was
  reverted on its numbers (ratio 1.68 -> 1.48, a walk-bot death).
- **b1 / a8 / a4 cuts, "let the SPOT pair show the gates, the lane and the parry line still read" (b1-cueCanyon, a8-cueLane, a4-cueStair):**
  the cuts stand in round 1; the iso pictures in r1/final are the evidence; if the SPOT pair or a reviewer says the line no longer reads,
  the one cue comes back (a reversible room-file edit).
- **a8, "measure bodies per charged shot before counting the file":** accepted, and not measurable in round 1: the per-shot body count is
  skip v2's print (n66-T 59ae6fed), not in my tree yet. The staged pilot from line65[0] hits 3 bodies (r1/final/verb-pierce.txt). Round 2
  opens with v2's bodies per shot; if the median is under 4, the file's channel at the ledge's drop is narrowed to single file before any
  more husks (the critic's fix). Round 1's six-husk lever B stands only on v1's number until then.
- **a8, "the head spitter steps off the line to its 10 m ring slot" (K3, Spitter.cs:186-197, read):** accepted: the node only places its
  spawn; where it stands at each charged shot is not printed by any tool I have (NOT CHECKED). The lever is judged on the skip alone, not
  on the claim that it stays on the line.
- **a8, "the camp rule is not verb-specific":** accepted; the plan's camp-rule reason for M2 is withdrawn (the pierce pilot holds the mouth
  too). M2 is read from the skip's STYLE columns only.
- **a8, "THE TAILRACE PAD moved fails M4; fill the pit and cut its stair in the same round wave 2 leaves the stair":** accepted. Wave 2 left
  the stair in round 1 (lever A); round 2 fills the pit and cuts the stair, judged on n66-deco and the guards, not on air65.
- **a4, "THE FLUE RIDE fails all four M4 tests; its gate 'M1 does not fall' is not the slate's rule":** accepted, and it is already out: it
  failed I2 (1.12) and brought a walk-bot death in round 1 (r1/B) and was cut. A later ride comes only as a free-standing fin that cuts a
  spitter's sight of a walk-line sample (test b), or goes to GG's play card.
- **a4, THE PAIR, "two spitters on the 10 m ring sit 4.5 m apart, beyond the 3 m burst: print the spacing":** accepted; no tool of mine
  prints the pair's spacing at the parry. The parry pilot prints "a second victim yes / no" for its one return (r1/final/verb-parry.txt),
  the nearest check I have. A is judged on "every gun kill is 3 taps under fire" and M2 alone unless a print shows a second victim.

## Round 2 (01:13-03:xx; skip v2 from here, `--bars` the main tree's Logs/n66/slate.json: b1 1.53, a8 1.25, a4 1.42)
Tools: main's T1a / T1b files at b2ae560a, T's fixes at 2658225d (a pierce seed counts as exercised only with >= 2 bodies), the
decoration checker at f3a2336e (`n66-deco.cjs --base e37083cf <room> --touches <skip v2>`, in my loop as `deco.txt`). Each lever alone
against the round-1 file; every output headed with the command, HEAD's sha, the room file's blob and the time.

**A tool finding for T (twice):** `n65verb skip` sat idle after printing its whole output on two a8 files with stand-ins (PID 64460,
01:41-01:50, a8/r2/B; PID 69936, 02:01-02:11, a8/r2/C; 0 CPU over 2-3 s each time); I stopped both by their PIDs and my loop now runs
every dotnet call under `timeout 900`. The lead's round tool will hang the same way on an a8 with stand-ins. Not checked: why.
**An instrument finding for T:** the verb-aware pierce trigger counts a second body within 2.0 m of the line (Harness.N65.VerbPilot.cs:199-216), the
shot needs it within a body's radius (0.4 m). In a8 the pilot charged 4-9 times a seed and hit 1 body on most charges (every a8 skip file
of round 2), so each cycle (0.9 s charge + 2.5 s cooldown) is time the verb-aware run loses: the instrument charges where a player would
tap. The critic named it in round 1 ("it will charge at pairs the ray cannot pierce"). I cannot fix it (the tools are T's).

| room | lever (folder) | M1 v2 (bar) | exercised | M2: STYLE on vs faster refusal | walk bot A / B deaths | kept? |
|---|---|---|---|---|---|---|
| a8 | round-1 file, v2 (a8/r2/v2-r1) | 0.84 (1.25) | 10 / 10 | 54.3 vs 47.0: FAIL | 0 / 0 | - (bodies per charged shot 0 or 1 on 42 of 46) |
| a8 | A: P15 THE SLOT CHEEK, a 1.7 m single-file slot (a8/r2/A; 1.4 m was refused by levelgen: no free 1 m AI-grid cell, a pit soft-lock) | 0.75 | 10 / 10 | 54.1 vs 47.0: FAIL | 0 / 0 | no: worse, bodies per shot unchanged, room check IN VIEW held FAIL (the cheek hides wave 1): reverted |
| a8 | B: THE TALLY, three static stand-ins in depth on the hatch ledge (a8/r2/B; before T's exercised fix) | 0.90 | 10 / 10 (old rule) | 56.7 vs 25.2: **PASS** | 0 / 0 | measured; C replaces it |
| a8 | C: THE TALLY on the spawn's ray across the pit (a8/r2/C; T's fix 1) | 0.89 | 8 / 10 | 71.5 vs 48.5: **PASS** | 0 / 0 | yes: M2 now PASSes, M1 +0.05; pierces through >= 2 bodies 11 in 10 seeds |
| a8 | C again on T's fix 2 (a8/r2/C2) | 0.89 | 8 / 10 | 71.5 vs 48.5: PASS | - | the same seed lines as C |
| b1 | round-1 file + line65 facing the file, v2 (b1/r2/line) | 1.33 (1.53) | 10 / 10 | 67.5 vs 33.4: PASS | 0 (A) | yes: the staged launch from line65[0] now **hits 3, kills 3 (M3 met)** |
| b1 | B: P20 GATE FM's PLUG, round 1's A re-judged on v2 (b1/r2/B) | **1.15** | 9 / 10 | 68.6 vs 42.2: PASS | 0 (A) | no: the plug passes the decoration checker (touched 4 / 10, blocks 11 sights) but M1 falls 1.33 -> 1.15: reverted (v2 confirms round 1's v1 revert) |
| a4 | B: A + P10 THE BELLOWS FIN, a 4 m fin out of the gallery's face at z -100, its south face a ride onto the deck (a4/r2/B) | 1.33 | **6 / 10: UNSURE** | 32.5 vs 17.8: FAIL | 0 / 0 | the fin: I2 **6.03** (walk 24.1 m / ride 4.0 m), decoration checker PASS (touched 7 / 10, blocks 22 of 51 sights), air65 rides 1, hand-offs 6, dead ends 0 / 3; it also cuts the floor spitter's sight, so fewer globs are parried |
| b1 | A: P19 THE KNOCK BLOCK in the canyon (b1/r2/A) | 1.37 (printed against the then-default 1.25) | 7 / 10 | 63.9 vs 35.3: PASS | **1 / 1** | no: touched 0 / 10, no launch kill on it, the staged launch unchanged (1 kill), a walk-bot death each set: reverted |
| a4 | A: P5 THE FENCE removed (a4/r2/A) | **1.28** (1.22) | 9 / 10 | 19.8 vs 21.7: FAIL (was PASS) | 0 / 0 | yes, as the base of B / C: the walk bot's clear 33.7 / 26.6 -> 20.4 / 25.3 s |
| b1 | C2: the review's one change, wave 3's east pair (8, -178.5), (3, -178) moved west to (-6, -177.5), (-2, -177.5) (b1/r2/C2) | 1.21 | 7 / 10 | 67.0 vs 34.3: PASS | 0 (A) | no: M1 1.33 -> 1.21 (the all-seeds medians moved the right way, 26.5 -> 24.9 s with the launch, 39.8 -> 41.8 s refused): reverted |
| a8 | D: the review's fix for THE TAILRACE, P6 THE TAILRACE PAD to the pit's south-west (-2.5, -2.3, -348.5), apex 4 (a8/r2/D) | 0.75 | 7 / 10 | 78.0 vs 74.9: FAIL | 0 (A) | no: the pad touched 5 / 10, but M1 0.89 -> 0.75: reverted |
| a4 | C: B + wave 2's second spitter on THE GALLERY at (-3, 3, -103) (a4/r2/C; the review's (-3, 3, -99.5) was refused by levelgen's spawn31, 9.0 m inside the entry's view cone against 12 m: a4/r2/C-refused/gen.txt) | **1.57: M1 PASS** (1.42) | 7 / 10 | 29.5 vs 9.8: **PASS** | 0 (A), B below | **yes** |

| b1 | D: wave 1 a husk leading four swarmers (R's finding: a punch on a winding-up husk parries, a swarmer always launches) (b1/r2/D) | 1.15 | 8 / 10 | 73.8 vs 43.4: PASS | 0 / 0 | no: M1 1.33 -> 1.15 (the sill's 3-kill bursts on 4 seeds, but the refusals clear faster): reverted |

### Round 2's files (committed with this note) and their numbers
| room | the file against round 1 | M1 v2 (bar) | M2 | M3 (staged pilot) | M4 (n66-deco) | walk bot A / B: clears, deaths (red / red65) | stall V (red / red65) | room check | air65 |
|---|---|---|---|---|---|---|---|---|---|
| b1 | line65's first segment faces wave 1's file (no geometry change) | **1.33 FAIL (1.53)**, 10 / 10 (b1/r2/line) | 67.5 vs 33.4 PASS | **3 kills: MET** (was 1) | 0 pieces added or moved: PASS | 10 / 10 and 0 / 0 (1 / 1; 0 / 0) (b1/r1/final, same geometry) | 24.6 (33.8 / 24.6) | PASS | 59 hand-offs, dead ends 0 / 9 |
| a8 | THE TALLY: three static stand-ins on the north apron in file on the spawn's ray; line65 from the door; wave 1's ledge nodes as round 1 | **0.89 FAIL (1.25)**, 8 / 10 (a8/r2/C, C2) | 71.5 vs 48.5 **PASS** (was FAIL) | 3 bodies, stand-ins 3 / 3: MET | 0 pieces (stand-ins are not judged): PASS | 10 / 10 and 0 / 0 (0 / 0; 0 / 0) | 23.1 (42.3 / 37.5) | PASS | 4 hand-offs, dead ends 2 / 4 (R2, D4), as red65 |
| a4 | P5 THE FENCE removed; P10 THE BELLOWS FIN added; wave 2's second spitter on the gallery (-3, 3, -103) | **1.57 PASS (1.42)**, 7 / 10 (a4/r2/C) | 29.5 vs 9.8 **PASS** | parried, the spitter killed: MET | THE BELLOWS FIN PASS (touched 7 / 10, blocks 24 of 51 sights) | 10 / 10; deaths 0 / **1** (0 / 0; 0 / 0): **GUARD FAIL** (seed 13: a 60.7 s fight, stall 57.4 %, flow-B-per-seed.txt) | 30.7 (31.2 / 25.5) | FAIL IN VIEW held, as both reds (one wave-1 spitter); I2 6.03 PASS | rides 1, hand-offs 6, dead ends 0 / 3 (red65: rides 0, 3) |

### The r1 level-design review (Logs/n66/r1/review-level.md), BLOCKING lines answered by name
- **b1, "decoration-only round (r1 moved no piece)" and "re-judge the reverted tries on v2":** re-judged on v2: GATE FM closed 1.15 (b1/r2/B),
  and three new pieces or packs (THE KNOCK BLOCK 1.37 on 7 / 10 with deaths 1 / 1; wave 3 west 1.21; a swarmer wave 1 1.15) all measured
  and reverted. Round 2 again keeps no new piece in b1: every piece I could add lowered M1 below the room NIGHT65 left. N10: two rounds with
  no M1 move; the redesign or the slot is the lead's call. M3 is met (the line's direction, 3 kills).
- **b1, "the single-kill launches come from wave 3's east pair, which skips the gates":** the review's one change was built and measured
  (b1/r2/C2): M1 1.21. Reverted.
- **a8, "on fix 2's rule the pierce fired on only 4 / 10 seeds, so M1 is really UNSURE":** on fix 2 the final file reads 8 / 10 exercised,
  11 charged shots through 2 or more bodies, M1 0.89: a FAIL with its refusal exercised, no longer UNSURE.
- **a8, "THE TAILRACE lost its last enemy job: fill it or give it one":** its fix (the pad to the pit's south-west, a flank onto the
  ledge) was built and measured (a8/r2/D): M1 0.89 -> 0.75; reverted. The fill was not tried (time). Open; for round 3.
- **a8, "flow B stall 51.3 % above both reds":** at the round-2 file the walk bot's stall reads 24.1 / 28.8 % (A / B): fixed.
- **a4, "decoration-only round":** round 2 adds P10 THE BELLOWS FIN (decoration checker PASS) and removes P5 THE FENCE, and moves a spitter.
- **a4, "no ride (PRINCIPLES 4; THE FLUE RIDE not built)":** THE FLUE RIDE was built in round 1 and cut at I2 1.12 (a4/r1/B, as the review's
  fix allows). Round 2 builds P10 THE BELLOWS FIN instead: a free-standing fin whose south face rides 4 m onto the gallery, I2 6.03.
- **a4, "the ratio follows parries per seed" and "plays slow":** the floor is now in the spitters' fire (the fence gone) and all three
  spitters stand on the climb (the review's own change): M1 1.57. The walk bot's clear reads 33.0 / 22.7 s (red65 33.7 / 26.6, the
  shipped room 15.1 / 15.2): still slow against the old room.

**Not checked:** why skip v2 sits idle after its output on a8 with stand-ins; where a4's seed-13 walker stalled and died; Player frames;
the iso pictures read by anyone but me (a4/r2/C/iso.png, a8/r2/C/iso.png, b1/r1/final/iso.png for b1's geometry).

## Round 3 (02:57-04:0x; the last; the lead's DECIDE66 D-3: b1's slot is dropped for tonight, it ends SHORT as NIGHT65 left it)
**a4 first: the walk bot's death on seed 13** (the round-2 file, flow B; both reds 0 / 0, a GUARD FAIL):
- Where (a4/r3/trace-seed13.txt, `n46flow --first 13 --per-seed --trace`): 47.9 s into the room, both gallery spitters alive, the feet at
  (0.9, 0.5, -97.2), at P8 THE BELLOWS PAD in the pocket between THE TELL BLOCK and THE BELLOWS FIN: a long fight (60.7 s, stall 57.4 %).
- Tried, each on seed 13 then the full set:
  - **no pad** (a4/r3/A-nopad): seed 13 lives; **M1 1.87, 10 / 10 exercised, M2 PASS, deaths 0 / 0**; but stall on flow V **57.4 %**
    against the reds' 25.5 / 31.2: every pilot sits on the stair with no second way up (the walker's clear 56.9 / 37.0 s). A GUARD FAIL:
    the pad stays.
  - **the pad's landing 1 m nearer the stair** (a4/r3/V1, seed 13 only): the walker still dies, now at the fin's east end (2.0, 0, -100.4)
    at 44.9 s: the landing is not the cause; the walker is worn down by two gallery spitters it cannot parry.
  - **a health pickup on the walk line at the fin's east end** (a4/r3/V2; b1 and a8 already have one, a4 had none): seed 13 lives (57.0 s).

- **b1's M3** (a launch burst of 2 or more kills): the staged launch from the sill at five yaws (b1/r2/pose): facing north (yaw -30, as
  line65 had it) the husk flies 6.8 m to THE LADLE and kills 1; facing the file wave 1 makes along the slag wall's north face (yaw -60
  / -90) it bursts on the next body, hits 3, kills 3; from the rail pose 3 kills. line65's first segment now faces that file (yaw about
  -60, inside GATE W). In the v2 verb-aware runs, real 2-5 kill bursts land on walls and on THE ANVIL (b1/r2/A, the launchkill lines).

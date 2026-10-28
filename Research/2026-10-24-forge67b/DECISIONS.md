# DECISIONS (FORGE67b, 2026-10-24 ~08:00-09:30; GG asked for the next 8-hour prompt with area design across many levels as the focus)

GG, verbatim (2026-10-24): "need to build a another detailed 8 hour build prompt following same methods, area deisgn need t be better in
alot of levels thats the mian focus". NIGHT67 has not run, so this forge rewrites NIGHT67 (its first plan, level 1's rooms, is kept as
Research/2026-10-24-forge67/NIGHT67-v1.md). The method is FORGE66's: four scouts in this folder (history.md, levels-data.md, research.md,
code-scout.md), these decisions, a red team (redteam.md), the slate seed, the builder brief, GOAL.txt, the n67 tools. Grounding: the repo;
Google Drive, Gmail and Calendar were tried for SKYGRIND material and all three sessions had expired; Linear is not connected.

## D-1 Eight hours; two room builders, a tools builder for the first 2:15, one review slot; built around the usage limit
NIGHT66 lost 4:35 to two stops (a window lasted 2:20 under its load). Agents: R and S (rooms), T (tools, 0:05-2:15, then on call), one
review slot: four running until T ends, three after. GG starts the night right after a reset (DECIDE67). Every slice commits its state.

## D-2 The disk decides the branch at t0 (A >= 23 GB, A-lean 18-23, C 7-18, under 7 no rooms)
C: 14 GB at the forge: branch C (sim only) unless GG frees it. In branch C every changed level is "not checked in a Player" (L-043).

## D-3 The focus: three levels, eight rooms, the worst first
By the data (levels-data.md section 3) and GG's words (history.md section 2):
- **Level 2 THE ICE LINE (n52-iceline), builder R: g1 TANK ALONE, g4 THE REACH, g3 THE SWARM, g8 THE STAMPEDE** (four of the campaign's
  six weakest rooms; GG 2026-10-13: "Nearly all rooms ... randomly placed").
- **Level 3 THE KILN (n57-kiln), builder S: kb2 (the first room: GG 2026-10-16 "The first room is confusing") and kb3 THE PRESS HALL**
  (gg-proxy's worst frames; GG 2026-10-13 level 3 "just isn't good"). n57-kiln has never been played by GG.
- **Level 5 THE SPIRE (n52-spire), builder S: v6 THE CHOIR and v2 THE NAVE** (the level's two flagged rooms; GG 2026-10-18 "the level
  doesn't make sense").
Not tonight: level 1 (three nights already; its rebuild waits for GG's play of Builds/n65-play-latest), level 4 (three fight rooms; its
complaint is the boss arena), bosses, links as rooms (research change 1: a leftover, below).

## D-4 The area instruments: a static area lint (MAIN) beside the verb test (MAIN), the walk bot a guard
The history's worst corner is judging rooms with an instrument that cannot see the design (history.md section 4). Area design is geometry,
so the first MAIN is **an area lint** over the room file and the level JSON (research.md section 1: six of its principles are static):
P1 the high plane, P4 every tall solid has a job, P5 the engagement band, P9 two climb routes per plane, P11 ranged up high, P17 one ride
look. The second MAIN is **skip v2 on the room's verb**, planted from its card (the verb pilot performs ride, slide, launch, pierce and
parry only, and the room's level must own it: pierce only where the revolver is owned). The decoration checker judges every piece of a
rebuilt room (its base the room file's first commit), and a round counts only by a double-or-half change (research P18).

## D-5 Tools first: T1 the multi-level pipeline (by 1:30), T2 the area lint (by 2:15)
code-scout.md section 3: a level table (Tools/night/levels67.json) and a generic slot assembler (Tools/night/gen-slots.cjs --level <L>,
about 650 lines from gen-n65-foundry.cjs), proved by `--check` (every slot old equals the shipped level) on each level and a byte-equal
cross-check on level 1; three bugs a copy would ship (span's / spire's base levelgen ERROR rows, the openings overlay, the surfaces list);
the round tools taking LEVEL; the shared commit (the lead's): design.n67.iceline67 / kiln67 / spire67 OFF, placeholder level files, the
map list, look49's list and UnityTuning.N53's Polish53IcelineLevels. Rooms keep their rect and doors (the slot model): "smaller" is done
inside the rect.

## D-6 A room's GATE: one held main-tree round, a second seed set, the area lint and a level review
As FORGE67's red team 2 (the sim is deterministic): M1-M4 and every GUARD on one round, skip v2 also on seeds 11-20, the room file unchanged
since, that round read by a level review; a blind spotter; a skygrind-critic PASS. The GUARDS are no worse than the shipped room on the
T1 tools (numeric ceilings in the slate).

## D-7 The rows and the play build
One level row per rebuilt level (design.n67.<level>67, levelFrom the shipped level, levelTo n67-<level>), each with its slots file
(Design/levels/n67/<level>/slots67.json). LV flips them only in branch A / A-lean. In branch C the play build is a data swap on
Builds/n66-freeze with every PASSed room's level (one `n65-play-build.sh` call per level, chained into one folder by T2's
`--update`, or one play build per level).

## D-8 The close and the goal (FORGE67 final, generalised)
n67-close-check.cjs reads every level row and its slots file (`--rows`, default level 1's and the three new: one ROW line per row; planted
cases 26-27) and the n65-foundry verbs37 side only when level 1's row is ON. GOAL.txt: the script's last line is the verdict.

## D-9 Leftovers (never past 6:00), ranked
(1) fodder in one link per rebuilt level (research change 1; a link clears only at alive 0: fodder the pass kills); (2) level 1's a4 GATE
(NIGHT67 v1's carry-over; the charge rule unjudged); (3) level 4's p3 / p5.

## The red team's amendments (redteam.md findings 1-14; they supersede the decisions above where they differ)
- **R-1 (1):** the close-check's default rows are level 1's plus every design.n67 row that exists in the config; the shared-commit script
  writes every slots67.json with every slot old (planted case 28: a row without its slots file fails).
- **R-2 (2), amends D-3:** level 2's g1-g4 cannot be measured (the gauntlet alone: the punch drops, the ride pilot counts only gun
  kills); R takes level 2's g7, g8 (after the KESTREL) and level 5's v2. A 1-seed verb probe per room comes first; a card's verb must fire.
- **R-3 (3), amends D-4:** the lint reads the round's folder and the touches; rules with nothing to judge print N/A; P1's size half is
  replaced by OPEN (no open floor square over 12 x 12 m with no piece: gg-proxy's "empty boxes"), since the long-axis rule passed 25 / 25;
  P9 counts planes 6 m or wider; P11 applies to TOWER / GALLERY / SPLIT LEVEL cards only; M4 needs at least one added piece with a job.
- **R-4 (4), amends D-1:** two work blocks: block 1 to the likely stop near 2:20 (the reds, cards, T1a / T1b, round 1, r1 chained by 2:05
  to compute through the stop), block 2 after the reset; at most two judged rounds.
- **R-5 (5), amends D-5:** T1 split: T1a by 0:50 (gen-slots, the shared-commit script), T1b by 1:45 (the round tools on L67, the lint, the
  charge-rule re-run), T1c by the first GATE (the GATE tools, the multi-level play build); builders sync T1a by path; a T1a fail on a level
  makes only that level's rooms SHORT.
- **R-6 (6):** rebuilt rooms carry verb65 (= the slate's verb) and line65, never area67; the GATE pack refuses a verb mismatch.
- **R-7 (7), amends D-3:** kb2 swapped for kb3c (the kiln's one measured fault: its ride skippable, 0.81); GG's level-3 words were about the
  kiln before NIGHT57; no room gets smaller tonight (the slot keeps the rect: a room can lose empty floor, not size).
- **R-8 (8):** critics batched; level reviews only for rounds with a GATE candidate; each round reads a slate snapshot; rule 6 bans slate
  writes during a round.
- **R-9 (9):** the link-fodder leftover dropped (links are kept bit for bit by the slot model; an in-place edit would ship ungated).
- **R-10 (10):** T1b's critic also reads d25ea7d8 with T's after-runs re-run on main; no pierce room before it PASSes.
- **R-11 (11):** the branch-C play build goes on n67-freeze when the close built it (n66-freeze lacks n67-iceline's colour grade, a code list).
- **R-12 (12):** LV's rows written in full; rc1 judged with the rows ON in a config copy before the flip; the reels' lines name the level
  each loaded.
- **R-13 (13):** n65-slot-set.cjs takes `--slots=<file>` (T1b makes it refuse an unknown option or a room not in the file).
- **R-14 (14):** launch's M3 reads "in a pit yes" / "in lava yes"; the flow base, the reds and the rounds run in series; a memory hold that
  does not clear in 20 minutes holds only its heavy job; the new level variable is `L67`; the builders' guards run on their assembled
  level; a verb change re-runs its red.

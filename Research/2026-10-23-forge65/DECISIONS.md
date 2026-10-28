# DECISIONS (FORGE65, 2026-10-23 ~13:09-13:50, right after NIGHT64's close; GG was not asked: he asked for the prompt)

GG, verbatim (2026-10-23 ~13:08): "ok we need to plan detaile dbuild prompt for next 7 hours 6.5 min 7 hard stop. follow same methods on
finding what to focus on, dont run into corners or repeat past mistakes, i wnat real progress and itterations and not just small polishes,
so please plan resrecah and output (spend 20-30 mins planning prompt) and give detailed goal prompt following best methods on opus 5.5 and
how to get best ouputs". Sources: four forge scouts in this folder (history.md, research-rooms.md, code-scout.md, research-run-methods.md),
docs/memory, Logs/n64/gg-proxy.md, NIGHT64-REPORT.md, the red team (redteam.md).

## D-1 Seven hours, 6.5 minimum, 7:00 hard stop, held by the guard's clock
Tools/night/n65-clock.cjs (forge65-make-n65.cjs): BUILD 0:00-6:15, FREEZE 6:15-7:00 (the close), CAP 7:00, stale after 10 h. The close
starts between 6:00 and 6:15 and "NIGHT65 COMPLETE" is printed at 6:30 or later (GG's minimum) and before 7:00 (his hard stop). NIGHT64's
close took 16 minutes in series (rc-first): 45 minutes is room for a level night's heavier close. Rejected: a close at 7:00 (NIGHT63 lost its
whole close window to the usage limit).

## D-2 The focus: THE ROOMS, on one level, rebuilt from scratch for flight
gg-proxy ranks the rooms #1 three nights running (Logs/n64/gg-proxy.md); GG's most repeated ask (history.md section 1: "every single level
needs a complete arena overhaul completely from scratch"; "rooms that really incentivize mechanics"; "open arenas with a roof ... one might
encourage the dash jump another wall riding ... can't be too exploitable"). Tonight: **level 1 THE FOUNDRY rebuilt as n65-foundry behind
design.n65.foundry65** (levelFrom n52-foundry, levelTo n65-foundry; NIGHT57's recipe), **six room slots** (a2 KESTREL, a3 RIDE, a5 CRUCIBLE,
b1 THE YARD, b2 THE GANTRY, a8 PIERCE); each slot holds the n52 room until its rebuilt room passes its GATE, so a SHORT room never costs the
level. Kept bit for bit: a1 INTAKE (start), a4 PARRY, a6 REST, a7 THE FLUE, a9 THE STACK (THE WICK). Level 1 because GG sees it first in
every build, its ride section is where "awful and really difficult and not fun" came from (2026-10-14), its data is the emptiest (68 boxes, 8
props, 7 pads, 0 grapple nodes for 11 rooms). Rejected: movement (blocked on GG's eye63 A / B; "don't change the core mechanics"); bosses
(w63-w64 did two nights; their shapes are GG's); two levels (w52's 18 rooms in one night: "none would satisfy GG"); re-cuts in place ("don't
preserve anything").

## D-3 The verdict is play-like evidence and GG's play card, never the bot
No room is "done" on the walk bot, the contract or the identity checks: those are GUARDS (the corner of w52-w56, history.md section 2). A
room's MAIN (fixed in its card before any number): its forced verb is on the room's main line (the validator's routes and a Player frame of
the line), two fresh persona reviews per round on its frames and plan picture with no blocking finding by the last round, a blind SPOT on the
old vs new entry frame, and a GATE critic's PASS. The real judge is GG: the night ends with a 5-minute play card (new vs -skygrindOff65) plus
eye63's X / Y in the same sitting.

## D-4 The contract is re-banded on purpose, before any room is measured
n52-room-check.py bans what GG asked for (a long side 20-32 m, a 17 m ride wall: code-scout.md section 7). Tonight's band, written in A1
before any measurement (L-100, L-103): a long side 14-32 m (tight rooms and open arenas both: GG 2026-10-13), height by the card, a ride
wall >= 10 m joining two planes (research rule 15; one ride at 8 m/s is 9.6 m), at most 3 planes >= 1 m apart (rule 2), >= 2.5 pieces per
100 m2 kept, every piece with a job (R8). verbs37's 25-30 m wall and 14 m lane are kept by b2 THE GANTRY's long wall or another slot's
(checked by verbs37 foundry on every build).

## D-5 Rounds, not polishes: at least three per room, batched by build
Each round: the builder edits its room file -> the sim loop in its worktree (levelgen, the room checker, n46flow --rooms, the re-banded
contract, the plan PNG; about 5 minutes) -> commit -> the lead picks, assembles, builds (about 1 minute) -> the Player reel of every room's
card poses (tour65) + a157 + verbs37 foundry -> two persona reviewers on all rooms of the round (gg-proxy and a level-design reviewer with the
research rules), batched -> findings to the builders -> the next round adds or moves pieces with jobs (never cuts only). Rounds at about
1:45 (R1), 2:55 (R2), 4:05 (R3), 5:10 (R4, stretch); GATEs 5:10-5:55.

## D-6 Three builders and one review slot; builders own room FILES, the lead owns the generated level
Builder T: the assembler gen-n65-foundry.cjs (room files Design/levels/n65/rooms/<id>.json in slots, --check that all-old equals n52-foundry,
levelgen, --finish), the slot tool n65-slot-set.cjs (a PASS's sha checked), then the tour65 reel (each room's card poses framed with wave 1
live), then the links. Builders R (a2, a3, a5) and S (b1, b2, a8): cards first, then rounds. Builders commit only their room files, cards,
notes and evidence; the lead regenerates the 2.5-5 MB level JSON after the picks (no two builders ever touch one file). At most four
subagents alive (three builders + the review slot), as NIGHT63-64 ran.

## D-7 Dressing with a job and a colour, from the kit that already exists
Kit33Props (about 45 kinds) and PropsView29 are ON in the code but the ON levels carry 4-18 props (foundry 8; NIGHT33's kiln had 332). Each
rebuilt room gets dressing where it does a job (a landmark, a route cue, a light's fixture, a hazard read), coloured by the role table (rule
8: cyan player-usable, red-orange danger, violet-white boundary). GG: "decorative elements ... lack strong color direction" (2026-10-20);
"don't just randomly plce stuff it needs intension and personality" (2026-10-17). Every prop passes R8 (a job and a why).

## D-8 Nothing lost: the room ids stay, the systems keyed to them are re-checked
Room ids keep their names (a2 .. b2) because the walk bot (Walk36, Walk38Foundry, Walk43Foundry: a3, a5, b2, a8), reel hooks
(FeelReel.N42.First: b2) and Arena42 (a5's grinder and cover) key on them; each round's n46flow on the room says whether the bot's room
script still clears it. Look52TierLevels names "n52-foundry" (UnityTuning.U52.cs:150): A1 adds "n65-foundry". teach35 lessons, job36 /
verb36 and the room's card stay on the row (VERBS.md: nothing lost).

## D-9 The forge experiment: building share
w53 spent 53 % building and every room night since checked more than it built. Hypothesis: with the assembler early and the reviews batched
per round, at least 70 % of the builders' minutes are building. Measure: each builder's notes tag their minutes BUILD / CHECK; the report
prints the share, and the rounds actually reached per room.

## D-11 The red team's 15 findings (redteam.md), all applied (this file's D-2 .. D-10 read with them)
(1) the GATE window over-full at 5:10-6:00 where NIGHT63's usage limit hit: R4 dropped; each builder's set GATEd as soon as its R3 reviews
land (~4:10 / ~4:30); rc1 after the first slots, rc2 by 5:40; **always two play builds** (the verified freeze and n65-play-latest, every room's
newest, labelled unreviewed). (2) T spawned at 0:05 from HEAD (T1 touches no A1 file); a9's finish step (gen-n52-foundry.cjs:429-473) kept
in T1. (3) the room file is the whole row + its pieces + lights / props + grapple + teach35, with a per-key table. (4) slots65.json is the
lead's, written only by n65-slot-set; room rows read "slot <id> of n65.foundry65"; only LV flips the row. (5) the bot opt-out `bot43: false`
(T1). (6) the contract is not run (D-4 below rewritten); the checks are n59-l-room.py, levelgen --walk, n46flow, hold52 --pads, the ride
pilot. (7) b2 out (kept: verbs37's wall and lane, the fall return), a4 PARRY in; cards fit their rects (the card gate rejects one that does
not). (8) red frames via tour65 on rc0 (fallback frames36.mid); cards from Logs/feel/n64-freeze/vf and plan PNGs. (9) MAIN is measured play
(air %, STYLE, pads touched, the ride pilot, the line's sequence) and "cannot be skipped: not checked" without a pilot. (10) BLOCKING must
name a piece, a rule or quote, and a one-round fix; after R3 the critic decides. (11) the night needs 20 GB at t0 (DECIDE65), a junction for
the red set, one round build kept. (12) no builder gates unless C# changed; builders pipeline rounds; D-9 measured from commits and diffs.
(13) every literal "n52-foundry" in config and code gets "n65-foundry" (look49's list at Design/unity-config.json:8168 too). (14) GOAL.txt's
rules moved under "Working rules, not conditions of done"; the CAP branch accepts a stale clock. (15) the red runs both seed sets.
**D-4 rewritten:** the room contract is not run tonight (its dwell / hold items need tooling the night does not have; its size band fights
GG's tight rooms; it is the corner that went green while GG disliked). **D-9 rewritten:** measured from the room files' commits and diffs
(pieces with jobs added per room per round, rounds reached), not self-tagged minutes.

## D-10 Disk is the first risk
C: had 14 GB free at the forge (the reel guard refuses a set under 12 GB). Builds/ holds about 30 builds of about 380 MB. The night prunes
its own reel frames at once (n65-prune) and deletes its own superseded GATE builds (keeping the last two and every RC); older nights' builds
are GG's call (T-149, DECIDE65). Branch C (no Player) if C: is under 12 GB at the bearings.

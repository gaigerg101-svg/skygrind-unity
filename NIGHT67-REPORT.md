# NIGHT67 REPORT: THE AREAS (8 hours in two work blocks; t0 2026-10-24 10:36:42; branch C, sim only)

Every claim names its command or file; anything not checked says "not checked"; every rebuilt level is **not checked in a Player**
(branch C: C: 11.7 GB free at t0, 4.8 GB at 2:19, 9.4 GB at 5:04). A usage stop ate 2:29-5:04 (2:35). The log is NIGHT67-LOG.md.

## 1. What is still not good
- **No room passed its GATE: 0 of 6.** The closest is g7 THE MIX (parry): skip v2 `M1 PASS 1.56 vs bar 1.39` on seeds 1-10 in
  r2-iceline, but the GATE pack's fresh seeds 11-20 read `M1 FAIL ... 1.24 vs bar 1.39` (Logs/n67/gate/g7/verbs65-skip.txt: seeds 14
  at 0.80 and 19 at 0.96); R's later versions pooled at 1.30-1.32 over 20 seeds (r3-iceline: `ratio 1.32 vs bar 1.39`). R traced why
  geometry cannot lift it: a spitter fires only with a clear line to the eye (HuskBrain.cs:816-822), the KESTREL aims at the same point
  and a returned glob homes on it (Spitter.cs:455-458), so cover that stops the gun also stops the spit. With the KESTREL in hand the
  refusing player simply shoots the spitters; parry saves only the gun's time.
- **The verb still does not pay in five of six rooms** (skip v2 ratio, the faster refusal over the verb-aware run, against the bar):
  g8 ride 0.77 vs 1.25, v2 launch 1.01 vs 1.25, kb3 launch 1.08 vs 1.35, kb3c ride 0.93 vs 1.25, v6 launch 1.06 vs 1.33 (r1 / r2 on main,
  Logs/n67/r<n>-<L>/verbs-skip.txt). Ride rooms are the worst: the verb-aware pilot rides every face it passes (g8: 92-97 wall rides
  against 55-59), so a ride wall along the fighting floor costs time instead of saving it.
- **Lava with no job came back and killed the bot** (history.md corner 11, again): S's first cut put lava beside the fight in kb3 and v6
  and the walk bot died 19 / 13 and 15 / 12 times over 20 seeds (S.md r1; S's tree). Every lava strip became a dry pit by r1d. v2 kept a
  lava crypt and its stall guard fails (24.7 % vs 19.0 %): the walk bot freezes wherever a point 1.5 m ahead is lava in plan
  (Harness.N46.Flow.cs:570), even under the bell and the beam.
- **g7 lost its air play to win its lint:** its ride walls were cut (P17 N/A) and air65 now reads 1 air piece, 0 hand-offs, dead ends 1/1
  (Logs/n67/r2-iceline/air.txt) where R's card cites the shipped g7 at 15 air pieces and 95 hand-offs. The ride lives on in g8; whether a
  player misses it in g7 is not checked.
- **The instruments measured the wrong game until 1:33.** skip v2 and the walk bot put the player into a room without the earlier rooms'
  weapon awards: every iceline run held the gauntlet only (shots 0) while a player holds the KESTREL there, and kiln ran without the
  revolver. The reds were re-run (Logs/n67/red-<L>-pre-awards/ keeps the old ones); g7's bar moved 1.25 -> 1.39 and its ceilings changed.
  Four harness fixes landed tonight (the parry refusal, the ride pilot's gun, the put-in loadout, the launch pilot): each is a place the
  instruments could not see the design.
- **The area lint is new and partly untested on real rooms:** P1 is read strictly (every node of the last wave's set 3.5 m up), P4 was
  never shown passing a shipped room, P5 / P11 first ran on a real room tonight, and a rule can go N/A by removing what it judges
  (Logs/n67/verdict-T1b.md problems 3-6).
- **The disk:** C: fell from 11.7 to 4.8 GB in 2:19 because Windows grew the pagefile with the machine's memory commit (DECIDE67 D-1);
  the builders stopped at the 5 GB line for about ten minutes before the usage stop.
- **gg-proxy's ranking on the final rooms** (Logs/n67/gg-proxy-close.md, from GG's recorded words, worst first): (1) in all six rooms
  the verb is not the faster way through; (2) floor-to-roof pillars and slabs still stand in five of six (v6's card said its west lane
  pier was removed: the final room still has it; the lint's P4 counts a touch as a job, and a touch is not intent); (3) three of six (v2,
  kb3c, v6) are the shipped room with a patch, not rebuilt from scratch (plan.png red vs r3: the same ramps, island, strips): against GG's
  2026-10-09 order; (4) the dry pits read as black holes in the floor (g8, kb3, v6: "VOID" on the plans), which GG named on 2026-10-09;
  v2's lava square has no shown purpose; (5) g7 is bare: "3 boxes (0 ride), 1 ramps, 0 pads, 0 grapple" (its iso header); (6) v6's loft
  has one way up, a perch to camp on.
- No person has judged any room built after EXT53; tonight's rooms are a tested guess until GG plays them.

## 2. GG's play card (about five minutes)
- **No play build tonight: no room PASSed its GATE**, so every slot in Design/levels/n67/<level>/slots67.json stays `old` and the three
  rows design.n67.iceline67 / kiln67 / spire67 stay OFF. The shipped game is unchanged (NIGHT64's rows; n66-freeze is still the
  verified build). PC is SHORT for that reason.
- **What there is to look at instead (files, not a build):** the six rebuilt rooms' room files (Design/levels/n67/<level>/rooms/<id>.json)
  with their cards (Research/2026-10-24-wave67/cards/<id>.md) and the sim's iso pictures (Logs/n67/r3-<level>/<id>/iso.png). Not checked
  in a Player.
- **Still yours from before (about five minutes each):** Builds/n65-play-latest (level 1's rebuilt rooms: better, worse or the same?),
  eye63's blind X / Y (Builds/n64-abx, n64-aby).
- **One question per level, for DECIDE67:**
  - Level 2 (g7): should a parry room take the gun away (the spitters out of the gun's reach some other way, or a parry-only wave), or is
    "parry saves the gun's time" enough for you?
  - Level 3 (kb3c): is a ride wall whose landing is a 3.5 m tower holding the drones the kind of room you want, even at 1.06 (not yet
    faster than walking)?
  - Level 5 (v2 / v6): lava beside the fight kills the walk bot and a dry pit slows it: do you want hazards in the fighting space at all?

## 3. Tools proven
- **T1a PASS** (Logs/n67/verdict-T1a.md): `Tools/night/gen-slots.cjs --level <iceline|kiln|spire>` reproduces n52-iceline, n57-kiln and
  n52-spire with every slot old; level 1 byte-identical to gen-n65-foundry.cjs; `n67-a1.cjs` made the shared commit (16116dea: three rows
  OFF, placeholder levels, slots67.json all old; gates all GREEN, Logs/n67/gates-a1.txt).
- **T1b PASS** (Logs/n67/verdict-T1b.md): the round tools on `L67`, a slate snapshot per round, n65-slot-set.cjs `--slots=`, the area
  lint n67-area.cjs (25 planted cases), the charge rule d25ea7d8 PASS (8 / 8 re-runs identical: a pierce room may run), skip v2's parry
  refusal fixed (globs parried by the refused run 12 -> 0).
- **T1c** (the GATE pack and deco on L67, the multi-level play build): round 1 FAIL (Logs/n67/verdict-T1c.md: no "room file unchanged"
  check); fixed (06732afa); round 2: (pending: Logs/n67/verdict-T1c-r2.md).
- **The put-in loadout fix** (a527ac13; the verb pilots 5adbca3b): measured on main: the whole-level walk unchanged (the re-made flow base
  `diff` EQUAL to flow-base-a0.txt), level 1 unchanged (T's a8 / a4 / a5 runs identical), the --flow gate all GREEN after it
  (Logs/n67/gates-flow-r2.txt).
- **The launch pilot** (c305a2ab): switches off the gauntlet before the dash-punch; shipped kb3 launch RED -> GREEN.
- **The lead's n67-red-bars.cjs:** each room's bar and ceilings read from its red files (Logs/n67/red-bars.txt), not typed.

## 4. What failed, with numbers
Skip v2 M1 is the faster refusal's clear time over the verb-aware run's (median of 10 seeds); the bar is max(1.25, the red's + 0.20) on
the fixed loadout (Logs/n67/red-bars.txt). The lint is n67-area.cjs (the shipped room: Logs/n67/area-baseline.txt). r3 is the builders'
final files on main (Logs/n67/r3-<level>/: verbs-skip.txt, area-<id>.txt, deco-<id>.txt, verbs.md, table.md).

| room (verb) | red | bar | r1 | r2 | r3 (final) | lint shipped -> r3 | deco r3 | M3 r3 | guards r3 vs ceilings | why it is not a PASS |
|---|---|---|---|---|---|---|---|---|---|---|
| g7 (parry) | 1.19 | 1.39 | 1.43 | 1.56; 11-20 **1.24** | 1.32 | FAIL 3/5 -> PASS 4/4 | PASS 4/4 | GREEN | deaths 1 / 3 (1 / 3), stall 19.0 (29.3) | M2 on seeds 11-20; the gun |
| g8 (ride) | 0.84 | 1.25 | 0.79 | 0.77 | 0.77 | FAIL 2/4 -> PASS 6/6 | PASS 5/5 | RED (5 shots, 0 kills) | 0 / 0, 21.4 | M2; the pilot rides every face (92 vs 55 rides) |
| v2 (launch) | 0.87 | 1.25 | 1.01 | (r1 file) | 1.01 (7/10) | FAIL 2/4 -> PASS 4/4 | PASS 9/9 | RED (launch36 0) | 0 / 0 (0 / 0), **stall 24.7 (19.0)** | M2; stall (lava in plan under the bell) |
| kb3 (launch) | 1.15 | 1.35 | 1.08 | (in S's tree) | 1.02 | FAIL 1/3 -> PASS 4/4 | PASS 7/7 | RED (target dead by the punch) | 0 / 0 (0 / 2), 21.8 (22.7) | M2; M3 |
| kb3c (ride) | 0.91 | 1.25 | 0.93 | (in S's tree) | 1.06 | FAIL 2/5 -> PASS 6/6 | **FAIL 8/9** | GREEN | **deaths A 1 (0)**, 30.2 (33.0) | M2; a death; the tower ramp touched 2 / 10 |
| v6 (launch) | 1.13 | 1.33 | 1.06 | (in S's tree) | 0.96 (9/10) | FAIL 1/4 -> FAIL 4/5 (P9) | PASS 1/1 | pilot GREEN, slate M3 FAIL (in a pit no, in lava no) | 0 / 0, **stall 19.6 (18.9)** | M2: ~21 launches per 10 seeds whatever the room |

- **What did work:** the area lint went from FAIL on all six shipped rooms to PASS on five of the six rebuilt ones (r3), with
  pieces that the decoration checker sees doing a job (touched, sight, launch kill); the walk bot's deaths fell in every room that kept
  hazards out of the fighting floor (g8 3 / 1 -> 0 / 0 on the pre-fix ceilings; 1 / 3 -> 0 / 0 on the fixed loadout).
- **Lava:** S's first kb3 / v6 put lava beside the fight: walk-bot deaths 19 / 13 and 15 / 12 over 20 seeds (S.md); dry pits fixed the
  deaths and raised stall (kb3 24.0 %, v6 25.5 %).
- **The instruments' own failures tonight** (each fixed and judged): the parry refusal leaked (12 / 12 / 12 globs parried); the put-in
  loadout lacked earlier rooms' awards (iceline gauntlet-only); the ride pilot held the gauntlet (0 shots); the launch pilot punched with
  the gauntlet in hand; the GATE pack lacked a room-file and a level-review check (T1c FAIL twice, PASS on round 3).
- **The usage stop:** 2:29-5:04 (2:35 lost); the disk: 11.7 -> 4.8 GB by 2:19 (the pagefile), the builders stopped at 5 GB.

## 5. Not checked
- Every rebuilt level in a Player (branch C); verbs37 on the rebuilt levels; any picture a player would see (the iso pictures are the
  sim's own drawings).
- skip v2 on seeds 11-20 for every room but g7.
- Whether a player misses g7's cut ride walls.

## 6. Your calls (DECIDE67.md)
- D-1: the disk and the pagefile: closing the two AssetImportWorkers (PIDs 51936, 49588) and capping the pagefile are yours.
- D-2: the builders worked design-only under 5 GB (reverse it with one word).
- D-3: the instruments' put-in loadout fixed and every red re-run before r1 (g7's bar 1.25 -> 1.39).
- The questions for your answers are at the end of DECIDE67.md (g7's parry rule, kb3c's tower, hazards in the fight, the disk).
- Open from before: the play of Builds/n65-play-latest; eye63's X / Y; DECIDE65 D-5.

## 7. The next prompt's seed
- **Fix the pilots before the rooms** (L-150 again): (a) the verb-aware ride pilot rides any face within 2.5 m to its far end, so every
  ride wall on the fighting floor costs time (g8: 92-97 rides vs 55-59): make it ride the room's line65 face (a planted pair); (b) the
  launch pilot's punch kills a 10 hp body outright and S saw it kill a husk at 1.6 m in kb3: target only bodies that survive, and say so;
  (c) the walk bot freezes wherever a point 1.5 m ahead is lava in plan, at any height (Harness.N46.Flow.cs:570): a deck over lava reads as
  lava. Each of these is an instrument that cannot see the design.
- **The parry room needs a rule, not geometry** (R's finding, g7): cover cannot separate the spit, the gun and the returned glob. A
  parry-only wave or a spitter the gun cannot kill (a shield that only a returned glob breaks) is a new enemy rule: GG's call.
- **The rooms that came closest:** g7 (1.30-1.32 vs 1.39; every other line PASS), kb3c (1.06 vs 1.25; lint 6/6, M3 GREEN), kb3 (every
  guard and static check held; 1.02 vs 1.35). Start from them, not from the shipped rooms.
- **Hazards:** lava beside the fight killed the walk bot in both rooms that tried it; a hazard only where the verb throws (kb3: beyond the
  press gate on the throw's line) is the untested one.
- **The disk before t0** (DECIDE67 D-1): the pagefile ate 7 GB in two hours; free 25 GB first or the night is sim-only again.
- **Level 2's g1-g4** still wait for a gauntlet pilot (the campaign's weakest rooms).

## 8. The close lines and every ROW line
- ROW: n65.foundry65 enabled=false; SLOTS: a2=old a3=old a4=old a5=old b1=old a8=old b2=old
- ROW: n67.iceline67 enabled=false; SLOTS: g1=old g2=old g3=old g4=old g5=old g6=old g7=old g8=old
- ROW: n67.kiln67 enabled=false; SLOTS: kb2=old kb2c=old kb3=old kb3c=old kb3d=old
- ROW: n67.spire67 enabled=false; SLOTS: v2=old v5=old v6=old
- The slate: `slate: 11 slices: 3 PASS (passes true), 8 SHORT, 0 BLOCKED, 0 in progress, 0 todo` (`node Tools/night/n67-slate.cjs`).
- The close started at 7:00 (Logs/n67/close-start.txt). The freeze build, verbs37 and the smoke were each refused by the guard: "SKYGRIND
  guard blocked this (disk): 11.7 GB free on the project drive, under 12 GB" (NIGHT67-LOG.md "## Blocked"). No play build (no PASS).
- The final gates: "gates: all GREEN 17:48:50" (Logs/n67/gates-flow-close.txt: flow --off67 EQUAL to the flow base, the config copies
  identical, golden 0 mismatches over 39 vector sets and 22 exact replays, every fingerprint plain and --off57..--off67 EQUAL, the Editor
  0 errors).
- The tag u3-n67 is made by `node Tools/night/n67-close-check.cjs --tag` after `--pre` PASSes; the clock stops at 7:30 or later; the
  final check prints the verdict.

# DECIDE53: the calls GG did not answer (NIGHT53, THE SECOND PASS; go 07:39)

Each call with the rejected alternative. GG reverses any of these; nothing here is his word.

- **D-1 Rooms are edited in place in the n52 levels; the shared commit has one view row** (07:41, 4d4be1c7). Design/levels/n52/*.plan.json are
  edited through scripts that assert the room's prior state (NIGHT52's own review fixes did this); tag u3-n52 and Builds/n52-freeze are the
  comparison side. design.n53 holds polish53 only (U's items, numbers placeholders firstShot / killWash / scorchCore / icelineGrade /
  killPop), so `-skygrindOff53` gives NIGHT52's look and the rooms' own off side is u3-n52's build. Rejected: n53 level ids (every generator
  copied and every level-id check re-plumbed, L-074 / L-077, for 3.5 h).
- **D-2 The openings (I7) live in a sidecar, Design/levels/n52/openings53.json, not in each plan's room row** (08:05). Builder K edits the
  kiln / span / foundry plans in place from 07:48; a field added to 18 room rows in those files would collide with K's commits at every
  pick, and the regen scripts were not checked to carry an unknown room field into the level files (nothing reads it there). Each opening
  is read from the room's identity card (docs/ROOMS.md) and its card line is kept in the file's `_cards`. Rejected: the plan field
  NIGHT53.md section 2 names (same data, a merge risk tonight).
- **D-3 T-126 fixed in the test that leaks** (07:54, d748867d): Lesson25Tests.TheOrder_FollowsTheBlocks saves and restores every static it sets.
  Proven in one Editor domain: unpinned pair RED (N52SharedTests 3/4), fixed pair GREEN (4/4); N52SharedTests' pin stays (NIGHT53.md).
  Rejected: a [TearDown] reset in every shared test (treats the symptom in N places).
- **D-4 I5's "central mass" is the occupied core, not the largest box** (08:00). The first definition (the largest single box within 20 % of the
  centre with a free ring) found 2 ring rooms in 18 at u3-n52 and FAILed no level, while the reviews saw four rooms share the shape: the
  mass is often a pit plus its decks (kb3's pour, g7's thaw lock). Now: the central 40 % x 40 % core >= 30 % occupied (solids or pits) and the
  2.5 m band round it >= 70 % walkable. At u3-n52 kiln FAILs (kb2, kb2c, kb3) and iceline has g3 / g7 / g8 (see the baseline). Rejected:
  keeping the first definition (its planted case did not FAIL: L-034 says it is not trusted).
- **D-5 The retry spawn in I4 is the room's spawn** (08:00): the plans have no separate retry field; a checkpoint door respawns the player at
  the room's spawn (kb3's vent was 1.6 m in front of it). The pad's height counts: a pad more than 1.5 m above or below the line is not on it.
- **D-6 I4 skips a walk line that ends at the pad** (08:12): such a line is the pad's own approach proof (a3's and a5's deck pads are walked up
  to by their proofs: 0.5 / 0.7 m "off" lines that end there). The threshold stays the spec's (centre within 2.0 m: the 1 m trigger,
  jumpPad.radius in PortExport/config.json, within 1 m of the line). The planted cases still FAIL (kb3 spawn line, p1 mid-line, a3 0.15 m).
  The baseline (Logs/n53/identity-p0.md) has I4 FAILs in 12 of 18 rooms, most 1.0-1.6 m off the walk pilot's circuit tours; p1's well pad
  [-1, 0, 99.5] is still 0.29 m off line 1 after NIGHT52's D-30 fix (only its second pad moved). Rejected: loosening to 1.0 m (a walker
  drifting 1 m off the pilot's line steps on the trigger) and counting approach lines (they would fail every deck pad by construction).
- **D-7 I1 at 60 % stands although 13 of 17 framed rooms FAIL it** (08:13): NIGHT52 built most rooms symmetric about the door's axis (b1 99 %,
  g2 100 %, g7 99 %, v2 98 %, p3 95 %); that is the template read the reviews named. The loop cannot re-cut 13 rooms in 75 minutes: the
  queue takes them by the reviews' order (g4, g3, then the worst overlaps), and the close reports the count. Rejected: raising the bar to
  pass the baseline (the check would stop measuring what GG called sameness).
- **D-8 I2 / I3 on the level file's navGrid, the worst ring decides I3** (08:16, 5105e60f). The bot's grid (NavGridBuilder's export, already in
  every level file) gives walker paths with no new package: DotRecast (RESEARCH B-1) was not needed. I2 takes a room's best ride (one
  shortcut is enough); I3 takes p1's worst ring (a teach room fails when any ring's tier is a short walk: the lesson can be skipped there).
  Rejected for I3: the best ring (p1 passes on a hook-only ledge while its terrace is walked: the reviews' complaint).
- **D-9 A room change is kept when it passes what it set out to fix, loses nothing the room passed, and the rest stays as before** (08:24).
  Section 5's "keep (all PASS)" read literally would revert g4's mirror fix (I1 77 -> 59 % PASS) because I2 stayed FAIL (1.18, it was 1.18
  before): I2 fails in 10 of 18 rooms at u3-n52, and its fix is a room re-cut each. The close table counts the identity FAILs left per room.
  Tried for g4's I2 and reverted: cutting the east bridge (I2 1.22 and the gallery ramp's foot unreached: the contract's tier item FAIL).
  Rejected: reverting a room whose own target passed because an older, unrelated FAIL remains (nothing would be kept).
- **D-10 g3's hopper narrows (4 x 6 m, still to the roof) instead of stopping at ~6 m** (08:25): a 6 m top is a fourth standing height (the
  contract asks exactly 3) and no walker reaches it (a 0-hit perch, the hold probe's FAIL), or it needs a ramp that the room has no space
  for beside the chute. The chute's cheeks move to the hopper's west face (the review's "opens west into the lane").
- **D-11 I2 never walks on lava, and its planted case moves from a3 to g4 @u3-n52** (08:50). Builder K found the walk crossing lava pits
  (the grid links every lava surface): a lava pit's surface is now no walk (a dry pit such as g4's canal stays: a fall costs a jump out).
  Re-measured, a3 @u3-n52 reads walk 26.1 m round the pit vs the 18 m ride = 1.45: the final review's "walking round is ~1 m longer" is
  not reproduced on the grid, so a3 cannot be I2's planted FAIL; g4 @u3-n52 (1.18: both lock walls beside the bridge-and-ramp walk) is,
  and it FAILs. The 18 rooms after the fix (Logs/n53/identity-lava.md): I2 FAIL kb2c 1.07, a5 1.08, g3 1.28, g4 1.18, g5 1.26, g7 1.26,
  g8 1.10, v2 1.17; kb3 now PASS 1.66 (K's inlet). Rejected: keeping the lava walk (it measured rides as no shortcut when the walk was
  through lava); shipping I2 OFF for want of a3's FAIL (the check FAILs real NIGHT52 rooms; only the review's estimate was off).
- **D-12 I4's pads moved where a 2 m-clear spot exists; the rest answered** (09:05). g7's return hoist moved 1.3 m east (I4 PASS, its pad
  probe PASS). g8's cross hoist: the only clear spot in lane W ([-9.6, 0, -228.5]) failed its pad probe (the throw to the far gallery
  missed): reverted, I4 FAIL stays (1.50 m). Spire: v2's bell hoist sits in the 2.5 m lane between the east aisle deck and the bell, v2's
  aisle hoist and v5's gallery hoist on 2-3 m walkways, v5's chamber hoist's clear spot is the chamber ramp's foot, v6's bell hoist 1 m from
  the east ride wall: none has a spot 2 m off its line without re-cutting the walkway. Rejected: moving a pad onto a ramp or against a
  ride wall to pass the number; cutting the walk proofs (they prove the walkways are walkable).
- **D-13 The reviews of g2 / g3 / g4 / kb3 (09:00): "none would satisfy GG"; what is taken** (both reviewers agree on the mirror and the
  empty door half). Taken: g2's mirror (cycle 5: the east pillar cut, the GG reviewer's flank side; I1 100 % is the worst in the level);
  kb3's east stubs cut and its east plateau's south 4 m (sent to builder K, its plan). Queued, not tonight (each a room re-cut beyond the
  loop's 25 minutes, and changing a wall moves a door and its link): g4 / g2's south floor 5-6 m shorter (GG: "smaller and more action
  packed"); g4's NW / NE blocks joined by a 6.5 m ledge (P-rank); g3's hopper against the north wall (P-rank) vs its chute east (GG): the two
  reviewers disagree, the checks (I1 21 %, I5 PASS) pass as it stands; g4's dry canal as lava (GG; it is ice in the level's fiction and a
  dry pit is the lesson's safe fall). Rejected: taking every finding as an order (the checks decide) and taking none.
- **D-14 g2's mirror is not cut tonight; build n53-r2 at 09:10 instead** (09:07). g2 is mirrored in every large piece (two gantries, two
  ramps each side, two ride panels, two springs); the GG reviewer's cut (the east pillar, 2 x 2 m) moves I1 by a few points, and the
  P-rank's (the east panel cut, the west panel moved onto the pillar's face) is a re-cut with its walk proofs and nodes: more than the
  loop's last 30 minutes with a build, U's proofs and K's a3 still to run before the 09:39 freeze. g2 ships as NIGHT52 built it, I1 100 %,
  the first item of NIGHT54's room queue. Rejected: the pillar-only cut (a change that passes nothing).
- **D-15 polish53 ships ON with three items; T-122 and T-111 stay at NIGHT52's fallback** (09:27). The row's check is per item: a Player
  line RED off and GREEN on. On n53-r2: the kill pop GREEN on foundry and iceline (pop yes, peak x1.32; off: pop NO); T-120 GREEN on iceline
  (ON-floor +0.072, ON-OFF +0.212; foundry -0.012, short of the floor); T-116 GREEN on iceline (lift x1.14 applied, sat 19.5 %; the look
  meter's 4/4 needs a349's combat frame, not run). T-122: the KESTREL's first shot smooth with the prewarm on 4 of 4 runs (11.4-13.7 ms vs
  18.4-26.4 off), the THUNDERCLAP's still a hitch (15.8-25.4 ms vs 28.6-38.1 off): never GREEN, firstShot.on 0. T-111: ON peak +0.327 /
  +0.294 vs the +0.25 target (OFF +0.425 / +0.324): never GREEN, killWash.on 0. Rejected: shipping T-122 for its KESTREL half (a measured
  gain, but the row ships an item on its GREEN, D-27); the whole row OFF (three items are proven).
- **D-16 Builder K's calls, kept** (09:25): a3's far floor is a 2.5 m pouring platform, not +1.5 m (at 1.5 m the lanes' ends stay inside the
  walkers' 2.2 m jump, so walking round stayed the short way); p1's start moved 5 m into the yard (the walk pilot pinned itself in the start
  passage: dwell 2.45 s); a3's retry spawn 4 m further in (dwell 2.30 s on try 1). Each re-passed the contract. Said plainly: from p1's new
  start 1 of 3 wave-1 enemies is in view (was 3 / 3). Rejected: NIGHT53.md's +1.5 m (the check it was meant to pass would still FAIL).

## EXT53 (GG 2026-10-20 ~10:44: "continue this prompt for another 2 hours ... minimum 2 hours, hard cap at 3 hours ... clean up fix bugs and get creative"; go 10:47)
- **D-17 D-12 was wrong for spire, span and kiln's pads: a searched spot exists** (10:55). Tools/night/e53-padspot.py scans the room on a
  0.25 m grid for the nearest spot that passes I4 and keeps 0.6 m from solids, off ramps, on a nav node at the pad's height, with a throw
  no longer than 1.15 x today's; it found spots 0.5-3.5 m from v2's two pads, v5's two, v6's, p5's and kb2's (kb2c: none). My 09:05 answer
  ("no spot 2 m off its line") was a manual read of the plan and missed them. Moved by Tools/night/e53-loop-pads.cjs; the pad probe decides.
  Rejected: keeping D-12 (an answer the tool disproves).
  Result (11:10, Logs/n53/e53-spire-contract.txt): kept v2's two pads, v5's gallery hoist, v6's bell hoist (15/15 each, every pad lands
  within 0.07 m; I4 PASS on v2 and v6); reverted v5's chamber hoist (landed 4.77 m short), kb2's east pad (did not land), p5's (its room's
  dwell past the grace).
- **D-18 EXT53's work: three builders and the lead** (10:47). M: the mirrors (g2 by both reviews, b1 with foundry's opening clash, then g8,
  v2); N: something new, RIDE KILL (rideKill53: a kill on or just off a ride wall refunds a dash charge with a callout), GG's w35 "the rooms
  still don't incentivize wall-riding"; B: the bugs (the AutopilotTests allocation flake since NIGHT14, the THUNDERCLAP's first-shot hitch,
  a152's THUNDERCLAP that never fires, kiln's short teach line); the lead: the pads (W), integration, the proofs, the freeze. Rejected: more
  builders (the spend limit stops every agent at once, L-003; the shared disk is at ~29 GB); a fourth room builder (one plan per owner).
- **D-19 A fourth EXT53 builder, K2, for kiln kb2c and span p3** (11:26): with M on iceline / foundry and the lead's pad lane done, two
  template rooms on plans nobody else owns had no owner (kb2c: I1 72 %, I2 1.07, a pad with no clear spot; p3: I1 95 %). 55 minutes to the
  builders' last commit. Rejected: the lead re-cutting them in loop cycles (L-092: a re-cut is a builder's round).
- **D-20 EXT53's full batch suite runs at 11:52 on 762ea622, not after the freeze build** (11:52): it takes ~36 min on this machine (2190 s at
  NIGHT53's close), and the freeze needs ~25 min of reels with nothing beside them (L-005) and the smoke, inside the 13:47 cap. The commits
  after 762ea622 (the builders' last fixes) get their own test classes in the Editor at the freeze. Rejected: the suite after the reels (it
  would end past the cap); the suite beside the reels (a153's frame times and the smoke's focus are load-sensitive).
- **D-21 firstShot ON at the EXT53 freeze** (12:05): T-122 went GREEN 2/2 on foundry and iceline on n53-r3 once builder B found the
  THUNDERCLAP's own cause (fx47's three sprite sheets, ~240 materials / meshes and ~2.7 MB built inside the first shot's frame; the
  KESTREL never triggers them): THUNDERCLAP worst 9.0 / 5.3 ms, KESTREL 7.9 / 7.5 ms, OFF 23-34 ms. D-15's condition is met. Rejected:
  waiting for the freeze build's re-proof before the config flip (the freeze build carries it and a153 re-proves it there).
- **D-22 The EXT53 review's pattern answered, not re-cut** (12:00): "each room breaks its mirror by deleting pieces from one side; none adds a
  piece that has a job" (g2 / b1 / g8 / v2). The concrete bug it found (g2's gantry in the wall) is fixed; its geometry asks (g2's h10 column
  to a 2 m plinth, b1's free-standing wall across the slag, g8's centre wall over the void, v2's ride moved into the SE) are NIGHT54's queue:
  each is a re-cut with its contract, and the builders' last commit was 12:20. Rejected: re-cutting after the builders stopped (the freeze
  build was due; a room changed after its review ships with that said).
- **D-23 RIDE KILL (rideKill53) ships ON** (12:20): a253 GREEN on / RED off on e53-freeze with the word measured on screen (3566 cyan px
  vs 127 before) and seen in the frame; verbs37 27 / 21 / 31 / 31 / 31 with it on; golden 22 / 22 exact, fingerprints plain / --off52 /
  --off53 EQUAL with it on (the fingerprint's run has no ride kill, so it cannot see the row: a known blind spot of that gate); the sim
  tests pin on / off / foot / grace. No human has played it: the boost refund is the reel's and the harness's. Rejected: shipping it OFF
  for want of a playtest (every NIGHT row ships on its proof; GG turns it off with -skygrindOff53 if it feels wrong).
- **D-24 One more builder after the freeze, on foundry a5, shipped only if it fully verifies by ~13:15** (12:26): GG asked for at least 2 h
  of building (to 12:47) and not to stop; a5 is the worst room left (I1 87 %, I2 1.08, I4 FAIL). If it lands by 13:00 it gets the foundry
  regen, the contract, verbs37 + a452 on foundry and the smoke on a re-made build; otherwise it stays on its branch for NIGHT54.
  Rejected: stopping at the freeze (GG: "I just dont wnat it to stop"); re-opening every room (no time to verify).


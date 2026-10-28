# NIGHT55 builder R (rooms): notes (round 1, 17:58-20:2x)

Worktree branch `worktree-agent-adc56f0a78ec83566`, based at 23a88bf1. No Editor, Player, build or reel was run: every number below is a
harness / script number from the worktree's level files. Rooms are edited in place (DECIDE55 D-4): `--off55` does not undo them; the
comparison side is Builds/n54-freeze2.

## Commits
| sha | entry | what |
|---|---|---|
| df811aff | P0-g7g5 | iceline g7 THE THAW LOCK II: the west the walkers' lock, the east the riders' firebox |
| 0a82414b | P0-g7g5 | iceline g5 THE ICEHOUSE: the west the pour, the east the crane |
| edfee264 | P2-clear | Clear55.cs (rules), design.n55.clear55.numbers.holdS 0.5 (row off), N55RoomClearTests (6 tests) |
| 876dc97e | P2-clear | the hold hook, the HUD33 card (UiClear55.cs), the a455 reel (FeelReel.N55.Room.cs), UnityTuning.N55R.cs |
| e25d89bb | P1-doorDark | slice 1: kiln kb2b THE STACK lit below its 32 m roof; Tools/night/n55-r-flat.py (the flat-frame meter) |

## P0-g7g5 (done to its computed contract; the critic's review and the Player reels are the lead's)
| room | I1 before -> after | I2 before -> after | I4 | contract | walk bot (n46flow n52-iceline, 10 seeds) |
|---|---|---|---|---|---|
| g7 | 99 -> 52 % | 1.22 / 1.26 -> 1.22 / 1.75 | PASS (4.0 / 4.3 m) | C 14/14 PASS; dwell 0.65 -> 0.05 s; clears 10/10 / 10/10; hold 14 spots min 27; pads 2/2 | GREEN 10/10; g7 clear 46.8 s (NIGHT53 cand 59.1), deaths 0 (2) |
| g5 | 76 -> 50 % | 1.07 / 1.11 / 1.25 / 1.26 -> 1.12 / 4.66 / 1.25 / 2.85 | PASS (2.5 / 7.5 m) | C 14/14 PASS; dwell 0.60 -> 0.80 s (its band 0.6 s); clears 10/10 / 10/10; hold 11 spots min 11; pads 3/3 | GREEN 10/10 with both rooms; g5 clear 37.7 s (NIGHT53 cand 33.1), deaths 0 |
Every added / moved piece, its job and its probe contact: Research/2026-10-20-wave55/R-jobs.md. Two first cuts failed a probe and were
fixed before the commit (both written into the scripts' whys): g7's return hoist in the keeper's yard (its arc hit the conveyor: miss
24.2 m), g5's runway ramp foot against the south wall (levelgen: "not reached by walkers").
The slate's sharper check "verbs37 iceline 21/21 on n55-rK" and "a fresh review names the piece and its job": not run (the lead's).
docs/ROOMS.md's g7 / g5 cards are not updated (not this builder's file); the scripts' header comments are the card text for the lead.

## P0-mirrors (contract number met; most rooms not re-cut)
- Identity table FAIL items, the 18 rooms: **11 -> 7** (the doneWhen asks <= 7). The slate's "14" is EXT53's mid-night count; HEAD 23a88bf1
  read 11 (a5 and v5 were fixed at EXT53). Left: kb2 I4, a3 I1 69 %, g3 I2 1.28, g4 I2 1.18, g8 I2 1.08 / 1.10, v2 I2 1.17, p5 I4.
- kb2 I4 tried and reverted: e53-padspot.py's nearest spot [5, 0, -89.5] (2.0 m off the line) fails the pad probe ("landed no", miss
  1.85 m: the launch hits the crucible's east face 1 m away; NIGHT53's probe of that spot failed the same way); the passing spot [5.5]
  is 1.5 m from the line. No x is both >= ~1.1 m off the crucible and 2 m off the line at x 7: the fix is a lane re-cut (the walk
  line's lane moved east), not a pad move.
- g3 (1.28): not re-cut: the lane beside the panel is the rider's lane too; a piece that lengthens the walk blocks the ride (no cheap
  piece found). g4 / g8 / v2: the walkers cross g4's dry canal anywhere beside the ride (D-9's bridge cut gave 1.22); v2's walkers drop
  off the aisle deck and jump onto the north ramp's side beside the ride: each needs a re-cut with its contract (~40 min each).
- The EXT53 review's asked pieces (g2's h10 column, b1's wall across the slag, g8's centre wall, v2's ride into the SE): not built.

## P1-doorDark (slice 1 only; not measured on frames)
- **The meter** `python -I Tools/night/n55-r-flat.py <frames dir> ...`: world-area luma std per frame (the HUD corners and the gun
  masked), FLAT < 0.03. RED on n54-freeze verbs37 x5: **13 flat frames** (vk 2: f0850 0.0128, f0950 0.0144; vf 2: f0500, f0950;
  vi 3: f0850-f0950; vp 2: f0900, f0950; vs 4: f0900 0.0181, f0950, f1550, f1600; per-file lines in its output).
- **Diagnosis**: nearly all are verbs37's wall steps (frames 850-950: wall ride, wall jump, bounce) with the camera at a wall up close on
  every level; "DOOR" on the kiln frame is the HUD's zone plate for the link kb2b THE STACK, not a door. In kb2b the cause is the light:
  a 32 m roof whose ceiling panels (31.95 m, range 14) light nothing below ~18 m, so through d3 the floor and G2a's 12 m cliff face
  render in the fog colour (f1000: the face dark violet).
- **Slice 1 (e25d89bb)**: kb2b gets four white lamps that reach the floor (two on the side liners at the cliff's foot, a floor pool
  before the face, one at the door landing); the room's one accent stays amber (the first cut used amber: LevelValidate's "one accent
  per room" ERROR). Level report 0 errors; walk bot n46flow n52-kiln GREEN 10/10.
- **Not done**: the other four levels' flat frames (each is the camera at whichever wall the reel's wall steps use: foundry a2, iceline
  g1, span p1, spire v1 per the reel logs); doors38's curtains themselves were not touched (door47's pink fix intact).
- Proposal (sharper contract): count flat frames per reel with n55-r-flat.py, and judge doors on a door-crossing frame, not on the wall
  steps; the verbs37 wall steps may also need their camera turned off the wall (a reel change, builder S's harness).

## P2-clear (built, row off; built by a fork of this builder in the same worktree)
- At a combat room's clear (Campaign24's roomClear, kind room): a 0.5 s hold (design.n55.clear55.numbers.holdS; the slate's 0.4-0.6 s)
  and a card "ROOM CLEAR · UNDER PAR / 31.2 S · PAR 50 S" for 2.6 s in HUD33's label styles (24 px label, 32 px number; cyan within par),
  190 px under the reticle. Par = the level file's campaign.rooms[].par (the plan's room row), carried on the roomClear event; all 23
  combat rooms of the five n52 levels have a par > 0.
- **A change from the brief**: the hold does not go through the sim-tier hitstop: design.impact.budget.perSecond 0.2 would cut a 0.5 s
  request to 0.2 s. It returns share 0 on the loop's share line in ImpactBeginFrame (beside Arena36's last-kill slow): the same freeze
  (no sim step, the views stop), one per clear, none with HIT FREEZE off; move54's kill cap untouched.
- Call-site lines in shared files: SurvivalGame.Impact.cs:46 `share = Clear55Frame(frameDt, share);`, Ui.cs:429 `BuildClear55();`,
  Ui.cs:1097 `RenderClear55(frameDt, f == Flow.Playing);`.
- Checked: dn-check-wt.sh 0 errors in all 4 projects; Clear55's logic outside Unity 30/30. Not run: the EditMode tests
  (Assets/Tests/EditMode/Game/N55RoomClearTests.cs: the root EditMode assembly cannot see Game code), the a455 reel.

## For the lead
- Reels (with the build from these commits): verbs37 + a452 on n52-iceline (g7, g5) and n52-kiln (kb2b's lamps); a455 per level with
  clear55 forced on in the build copy (`c55on,<level>,a455`) and off (`c55off,<level>,a455,-skygrindOff55`); then
  `python -I Tools/night/n55-r-flat.py Logs/feel/<tag>/vk/verbs37` (and the other four) for the flat count.
- Frames for the critic: a452 g7 entry / mid / high (the firebox, the stoker's bridge, the keeper's tower from the quay; high = the
  tower) and g5 entry / mid / high (the crane runway over the ring; the pour chute), each beside Builds/n54-freeze2's; kb2b from d3 and
  at the cliff face (vk verbs37 f0950 vs n54-freeze's); a455's `shot-N29-C55-<level>-<room>-hold.png` / `-card.png`.

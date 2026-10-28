# DECIDE52 (the lead's calls for NIGHT52; GG will not answer tonight; each call names the alternative it rejected)

Untracked, like NIGHT52.md. Builders read it; only the lead writes it.

- **D-1 The clock.** Kickoff 22:29:46 MST 2026-10-19 (the first printed `date`). Freeze by 04:59 (6:30), close by 05:59 (7:30), hard cap
  06:29 (8:00), minimum 05:29 (7:00). Rejected: counting from GG's message time (~22:00): the goal counts from the kickoff's printed date.
- **D-2 What counts as a piece** (n52-room-check.py): every box, ramp, pad and grapple row with the room's zone, the room row's grappleNodes
  and pads, and every prop with `collider: true`. A pickup or light prop without a collider is not geometry. Rejected: counting every prop
  (a health pickup asks for no verb by its shape; it would let a room pass density with decorations).
- **D-3 The bot is n46flow's walk pilot.** At P0 NIGHT24's `campaign <level> --seeds 10` reached no room of n47-kiln or n48-foundry (every
  room NaN, cleared 0/10: Logs/n52/base/campaign-*.txt); the n46flow walk pilot clears every room 10/10 (Logs/n52/flow-p0). So the contract's
  "the bot clears the room 10/10" reads n46flow's per-room row (clears k/n, deaths). First-try is not printed per room by n46flow: deaths
  per 10 seeds are logged instead. Rejected: repairing the old campaign bot tonight (a pilot project, not a room).
- **D-4 A tier** is the floor plus each cluster of walkable tops (deck / slab boxes, ramp tops) >= 1.0 m high, a cluster being tops within
  1.5 m of its lowest; pit floors are hazards, not tiers. Rejected: every distinct top height (a 0.5 m step would count as a plane).
- **D-5 Enemy types:** the room check passes 2-4 kinds in a room (A15: 2 regular, 3 complex, 4 brawl); LevelValidate29 already caps a
  wave at 2 kinds. Rejected: exactly 2-3 per room (kiln's kb2 and kb3 already carry 4 across three waves, one role per wave pair).
- **D-6 kiln47's beats on n52-kiln.** SurvivalSim.N47.Kiln.cs keyed THE GLIMPSE / PASS / CAMEO on the raw id "n47-kiln"; turning kiln52 on
  would have lost them (PRINCIPLES 1). The shared commit adds SurvivalSim.N52LevelFrom and keeps the beats on n52-kiln. Rejected: leaving it
  for builder K (a sim file outside K's ownership; the shared commit is where hooks go, L-049).
- **D-7 The look49 grade per level** lists level ids (PostFx.Look49Listed, raw compare): the shared commit adds n52-foundry and n52-spire beside
  n48-foundry and n47-spire, so a swapped level keeps its NIGHT50 grade. Rejected: a PostFx code change to resolve the levelFrom (a render file
  builder U may touch tonight).
- **D-8 Evidence lives in Logs/ (git-ignored),** as every night before: the commit carries the code and the log text; the frame and output
  paths are local. Rejected: committing the frames (GBs a night).
- **D-9 Parked for GG:** a TrenchBroom / CSG blockout pipeline (Scopa, Chisel; MIT) instead of hand-written plan rows (NIGHT52.md section 5).
  Not tonight: a pipeline change.
- **D-10 Dwell grace.** The contract's "dwell no worse" passes at <= the P0 room's dwell + 0.5 s (n46flow's median over 10 seeds; the meter's
  own seeds 1-10 vs 11-20 band at P0 is 1.6 s on the level, so 0.5 s per room is stricter than the noise). Rejected: a strict <= (a 0.1 s
  wobble of one median would fail a room that is better on every other number).
- **D-12 The loop takes combat rooms only, and starts early.** Teach rooms (job36 teach), reward rooms (iceline g6 THE ARMOURY, the KESTREL's
  return) and boss rooms are exempt from the contract (NIGHT52.md section 2), so the seeded queue's g6 and g1 (teach) and span p1 (teach) are
  skipped; the queue is iceline g7 (0.52), g5 (0.77), g4 (0.83), g8 (0.97), spire v5 (0.95), v6 (1.02), span p3 (1.56), p5 (1.70), then foundry's
  and kiln's leftovers. P0 finished at 0:22 and builders report at ~1:45, so the lead's loop starts at 0:22 instead of 4:30 (building share).
  Rejected: rebuilding a teach room to the contract (a one-verb lesson room is small and sparse on purpose: A-7) and idling until 4:30.
- **D-13 The dwell item uses both seed sets and the room's own noise** (changed at 23:55, after spire v6's second dwell miss; logged as a
  rule change made after seeing a fail). n52-room-check.py takes `--flow-before A,B --flow A,B` (seeds 1-10 and 11-20) and passes when the
  candidate's mean dwell is no worse than the P0 mean by more than max(0.5 s, the room's own P0 seed-set gap). That is the flow table's own
  noise rule (its band = the base's seeds 1-10 vs 11-20), applied per room. v6: P0 2.8 / 4.5 s (gap 1.7 s), candidate 4.7 / 5.9 s: +1.65 s,
  inside the room's own noise. Rejected: reverting v6 under the twice-missed rule (the measured change is smaller than the meter's own
  seed-to-seed spread for that room), and keeping D-10's flat 0.5 s (it reads noise as a fail in a room whose two P0 seed sets differ by 1.7 s).
- **D-14 The iceline persona reviews (Logs/n52/review/; the checks decided).** P-rank runner: (1) g7's coal bunkers are a keep behind the
  fire -> the hold probe gave the bunker tops 51 / 65 hits in 20 s (not a camping spot): the bunkers stay; the door-side half of the claim
  was right in another form (the walk pilot's isolated door dwell rose), answered by two quay nodes (fix 3). (2) g5's crane is a dead end ->
  fixed: the shelf hoist turned round (crane -> shelf). (3) g5's short ice walls cut the door chute off the side lanes -> fixed: cut to their
  north 4 m. (4) g8's east sand tower sits in the signal box ramp's run-out -> fixed: moved to lane E's door end. (5) g8's column tops are a
  walker-proof skyway -> **confirmed by hold52: 0 hits on all three column tops**; fixed: the columns run to the roof. GG persona: (1) g8's dry
  pit reads as a fall hole, make it lava -> rejected: the pit is the charger trap the lesson names ("Lead one into the pit."): it holds a
  charger for the punish from behind; lava would kill it and remove the punish (the verb). (2) g8's identical columns, step their heights
  into a hop line -> rejected: the hold probe showed column tops are perches no walker reaches (A12); the columns went to the roof instead.
  (3) the ice stack / boiler have no identity -> partly fixed: a frost lamp on the ice stack's door face (the boiler already has the
  grate's orange pool and its gauge lamp). (4) g5's door-side posts are random pillars -> fixed: cut (the room's east side came in 1 m and
  a west hoist was added so the density holds). (5) the lesson banner stays up the whole room -> not a room: the lesson rows show until
  done (Lesson25); logged as leftover 5 (teach lines that wait for the verb). The names (THE MIX etc.) stay as the rooms' card names: the
  identity titles live in docs/ROOMS.md (as NIGHT45's THE THAW LOCK did).
- **D-15 The room contract's flow items are measured per room in isolation** (Tools/night/n52-room-flow.sh: n46flow --rooms <id>, base with
  --off52, seeds 1-10 and 11-20). The chained run carries the pilot's state from room to room: g7's dwell moved 2.9 -> 10.0 s on seeds 1-10
  when only g5 had changed. The isolated runs found what the chained runs hid: the pilot capped in g7 on 3 of 20 seeds and in g8 on 1 of 20,
  stuck in pockets the rebuild had made (fix 2 closed them). Rejected: the chained run alone (it hides a stuck room behind an earlier room's
  state, and moves numbers of rooms nobody touched).
- **D-16 The kiln / foundry / spire GG-persona review (Logs/n52/review/; 15 findings).** Kiln: (1) "SPACE to jump" on level 3's frames ->
  not a room: the reel runs on a fresh profile, and teach35's lines show once per save (a real player saw it on level 1); answered, no change.
  (2) kb2c's "grapple nodes on 0.4 m pads" -> kb2c has no grapple nodes: the 0.4 m decks are K's burrow-safe metal plates (the burrower cannot
  surface through metal); the two flanking the spawn narrowing the entry -> to K (move them off the spawn's line). (3) kb2c's three lone flues
  read random -> to K: each must show its verb in the frame (the north flue's wall-jump to the loft stays; the lone south-east one goes or
  joins a structure). (4) kb3's yellow wash / orange shape -> not a room: kiln47's cameo beat on n52-kiln (D-6's fix working): the worm
  breaching beside the camera and its floor tell; to K: check the tell's area in the smaller room leaves a safe stand. (5) kb2's "tipped
  crucible" reads as stacked boxes -> to K: a sloped lip (a ramp-wedge spout) on the crucible's east box toward the lava and the ride wall.
  Foundry: (1) a5's crucible blocks the door's view of the north half -> rejected: the central mass is the contract's circuit centre and it
  breaks the door's line on purpose (the door landing is not a firing position: a5's door dwell 3.2 -> 0.0 s). (2) b1 repeats a5's kit (the
  stepped centrepiece, the door cheeks, the side ramps) -> to F: give THE SPILL its own centrepiece (a tipped ladle pouring into the river) and
  drop b1's door cheeks if its dwell holds. (3) b1's bridges at the river's ends make the ride walls optional -> to F: one bridge becomes river
  (the ride is that side's crossing), the other stays the walkers' route. (4) the health pickup reads as a cyan canister under small text ->
  to U (a pickup view: the health silhouette). (5) the cyan beams on rings "mark the spawns" -> rejected: they are the jump pads (cyan =
  player-usable, rule 8); the enemies stood near them in the frames because the pads sit on the circuits. Spire v6: (1) the rings are flat cyan
  discs with a red-orange line -> not tonight's room (the ring and rope look is NIGHT31-47's view; red-orange on a rope breaks rule 8: logged
  for U / a later night). (2) every tier has a ramp, the rings are never needed -> to F (who owns spire now): drop the island's north ramp
  and hang a ring over the island -> loft gap so the rope is the short way up (keep the density with the ring). (3) the east ride wall runs over
  plain floor -> to F: a lava strip in the east lane beside the island so the ride is the crossing there (walker routes kept). (4) one olive
  green on every surface -> the grade (look49 on spire, T-112); not a room. (5) the rood screens and bell posts "say nothing" -> rejected for
  geometry (each has its LOS / cover job); the organ and bells as visible props are a look item (logged).
  **The P-rank runner on kiln / foundry / spire (12 findings; all sent to the owner as fixes except where noted):** kb2's health box narrows
  the east lane to 1.5 m (an out-and-back) -> K: cut it, health to the lane's lava edge. kb2c's NE ramp stops 1.5 m short of the ride wall (a
  pocket) -> K. kb3's twin ride walls split the room and hide wave 1 (1/9 in view) -> K: the east one to a 1 m lip. kb3's jut and pillar slots
  -> K. kb3's west block is a 14 m dead end (the contract allows 6 m) -> K: a north ramp. a5's door cheeks leave a 2.5 m strip and the ride ends
  in a pocket -> F: cheeks 2 m deep. a5's pillar blocks the door's view, lower it to 1.2 m -> rejected again (the circuit's centre; both
  reviewers named it: the half that is real, wave 1 not in view 0/4, -> F: nodes out of its shadow). a5's 5 m gantry with the health is a camp
  -> hold52 gave that top 9 hits (not 0: not a camp by the probe), but the health moves to the riskiest spot (F). b1's cheeks end both rides in
  pockets with spawn nodes -> F. b1's centre block (2 m ledge, health on its lip) -> F. v6's rood screens and posts fence the entry third -> F:
  the screens go (the posts stay). v6's loft with the health is a camp -> F: the health to the floor. Three rooms (kb2, a5, v6) put the
  health on or beside the top tier: A20 says the riskiest spot, so all three move. The unusable frames (kb3 mid / high: the cameo; v6 mid:
  a wall) -> their frames52 spots to the owners.
- **D-17 One more view row, look52** (d062cccc; the lead's second config commit of the night, after the shared one): builder U's round 5 needs a
  switch for the health pickup's silhouette and the ring / rope colours (the GG review's findings), and neither belongs to hud52 / skin52 /
  guns52. Rejected: hiding them under skin52 (an enemy-surface row: its off switch would carry unrelated changes).
- **D-18 kiln's kb3 ships as n47's (the fallback).** THE MOULD LINE passed every contract item but the isolated door dwell (1.90 s against
  1.05 + 0.5; six tries: the spawn twice, the apron, flank nodes, a noride door wall, the ramps north, noride liners at d5). The rule
  (NIGHT52.md section 1: a room that misses twice goes back) is applied; the room stays in git (`gen-n52-kiln.cjs --rooms=kb2,kb2c,kb3`).
  Rejected: widening the dwell grace for one room after six misses (D-13 already used the room's own seed band: 0.1 s here).
- **D-19 The smoke and the paired controller.** At 01:33 `Get-PnpDevice -PresentOnly` lists a Bluetooth "Pro Controller" (Status OK), the
  device NIGHT49-51 suspected for the smoke's four failing pad steps (T-114). The lead does not disable or unpair a device on GG's machine
  (an outward, owner's call); the freeze runs the smoke as it is and logs the device's state. Rejected: Disable-PnpDevice for the run.
- **D-20 The iceline / span grade lives in look52's numbers, not look49's list** (6571cdb4). U's round 6 asked for n47-iceline / n47-span
  in design.n49.look49.numbers.levels; that list is NIGHT49's row, which -skygrindOff52 does not cover, so NIGHT51's game would have changed
  under the off switch. The ids sit in design.n52.look52.numbers.gradeLevels, read only while look52 is on. Rejected: editing look49's list.
- **D-21 The span / spire v5 / v2 persona reviews (GG 10 findings, P-rank 9).** All concrete geometry findings went to the owners as fixes:
  K (span): p3's spawns all near the exit (a shooting gallery) -> nodes onto the decks and the void block; p3's west ride over solid floor ->
  onto the void's west face; p3's random posts -> the cable's tie-off posts; p3's east ride wall turning the 4.2 m ledge into a trench -> capped
  1 m over the ledge; p3's void column blocking the anchors' line -> deleted, its anchor floating; p5 repeats p3's kit -> the spool drum; p5's
  four columns splitting the floor strips -> onto the void lip; p5's west ramp slot -> 2 m wide. F (spire): v5's stuck seed 17 (chained) first;
  v5's ride over walkable floor -> onto the newel's face; v5's one wave of 4 -> a second wave; v5's entry columns (a slot + a pocket) -> 2.5 m
  tops; v5's ride landing on its own ramp -> the ramp 2.5 m east; v5's 26 m2 loop -> the east block meets the newel; v2's ride beside the
  walkable aisle -> a gap in the aisle; v2's bell reads as a cube -> a bell shape (same footprint) if time; v2's 1 m crossing at the exit
  columns -> door jambs; v2's central block hides wave 1 and is the camp -> its south 3 m cut. Answered without change: the red-orange "bell
  rope" is the enemy spawn beam (correct, rule 8; U checked: the rope is cyan in config); ramp feet 2 m from an end wall (a 90 deg turn in a
  2 m pocket) stay: the pocket rule is slots under 1.5 m.
- **D-22 The polish fallbacks the checks chose on n52-r15** (02:48, a53b1e48; written into this file at the close, the log carried it):
  skin52.numbers.scorchGlow = 0 (the scorch core still read darker than bare floor at 0.5 s: rise -0.036 kiln / -0.122 spire against
  >= +0.08, so NIGHT51's mark ships under skin52; a252 then judges what ships); look52.numbers.gradeLevels = [] (iceline's lifted grade
  reached 3/4 on the look meter, saturated 10-30 no; span had already met 4/4 with no lift, 681213ee, D-20's follow-up); look52's tiers
  ship on foundry only (spire RED). Rejected: keeping the lifted grade on iceline at 3/4 (the meter's own band says no); a hotter core
  value guessed without a frame that shows it.
- **D-23 The midpoint GG-persona review (12 rooms) and the second P-rank pass (12 findings), 03:15.** Fixes went to the owners with a 04:20
  deadline; each fixed room re-passes the whole contract or the fix is dropped. K (kiln, span): p5's island camp -> a north bridge through
  the h14 gap, the south posts cut; p3's west deck walkers can't reach -> a north ramp, the straight bridge's middle 6 m cut (the rings carry
  the gap); kb2's lesson "SHOOT DOWN. FLY" lands nowhere -> a shot-jump ledge at ~8 m over the lava, the two posts by the spawn lane cut, the
  short east ramp checked (a pad if it stops below the lip); kb2c's health by the spawn -> onto the d4.5 block. F (foundry, spire): v5's w12
  block in front of the door -> thinned to a 1 m wall on the deck's edge, v5's and v2's health off the one-ramp d5 corners (the camp) -> onto
  the floor; b1's single lava crossing -> a second one at the west end, its corner ramps widened to 3 m, the mirror broken; a5's second health
  cross cut, the slot at the h10 column opened to 3 m; v6's six rings -> the circuit's only. The lead (iceline, fix 5): g7's bunker ramps faced
  the wall (two U-turns from the door) -> turned to rise from beside the door onto the bunkers (the review's ramp-for-bunker swap rejected:
  19 pieces = 2.47 per 100 m2, under the contract, and wave 3's six nodes in the air); g8's inner lanes 2.1-2.3 m beside THE PIT -> the two
  east columns 1 m out (the charge stripe's lane; the west two and the spawn row's two measured blocking the validator's walk / leaving a 0.6
  m slot, kept). Answered without change: a5's h10 column stays a column, not a plinth (the circuit's centre, rejected twice before); g8's
  void trench stays void (the charger trap: the lava swap was rejected at the loop); g5's stack shares g7's template but is g5's third tier
  (cutting it to a cover block loses the plane: rejected); the yellow discs / saucers in iceline / spire frames are spitter globs (parryable
  yellow, rule 8; their size on screen at close range is T-117, enemy design is GG's); the teach banners ("SPACE to jump", "G an enemy to yank
  it") are the teach rows on a fresh profile. Rejected: taking every persona finding as an order (the contract decides) and taking none (the
  freeze would ship rooms with known camps).
- **D-24 The loop takes iceline g4 THE REACH (0.83) although D-12 exempts teach rooms** (03:35). NIGHT52.md section 5's queue names g6, g1,
  g4 by density; D-12 exempted them from the contract's items, not from the loop. With the queue's combat rooms done and an hour before the
  freeze, the lead rebuilt g4 (Tools/night/n52-loop-g4.cjs) keeping its lesson and waves (drones only: the contract's types item exempt) and
  holding every other item: 2.64 per 100 m2 (19 pieces), 3 planes, two 17.5 m lock walls, 3 pads, health, clears 10/10 / 10/10, dwell 0.40 ->
  0.80 (grace 0.5), hold min 4 hits (Logs/n52/loop/contract-g4.txt, 13/14 with the types item the one FAIL). Kept. Rejected: stopping the
  loop at the end of the combat list (section 10: a list that ran out is not a stop); adding a second enemy type to pass the item (the room
  teaches the drone's orb; A-7 one lesson).
- **D-25 The last loop cycle (03:45-04:40): K on kiln kb3 (T-119, the door dwell by trace), F on span p1 HOOK YARD (a teach room in the
  contract's spirit, like g4); the lead verifies r19 and prepares the freeze.** Read and not taken by the lead: iceline g1 TANK ALONE (the
  level's first lesson: the charger's run down an open axis breaking on a ride panel is the lesson; pieces in that lane would teach less);
  iceline g3 THE SWARM was first declined (03:45: 11 more pieces with jobs in 45 minutes beside the freeze prep), then TAKEN at 03:47 when the
  clock showed g4's whole cycle had taken 6 minutes of wall time and r19's reels run unattended: rect 26 -> 24 m wide (the trestle 1 m in),
  roof 9 -> 10 m, a 4 m SE loft by its ramp, a 2 m tipple head off the trestle, two spill walls at the chute (the swarm packed into the west
  lane: the lesson's place), the ride panel to 18 m, two hoists; reverted if it misses the contract twice (it passed 14/14 on the second
  try). Then iceline g2 THE SHIELD (1.19) read at 03:58 and left: the flank lesson is an open floor with two pillars; 2.5 per 100 m2 needs 9
  more pieces and the room has no ranged enemy, so covers would be filler (GG w35 "random pillars that serve no purpose"); NIGHT53. Rejected:
  ending the loop at 03:45 (section 10); padding g2 to the number.
- **D-26 Foundry a8 PIERCE stays NIGHT46's** (F round 8, 04:12): its dry pit is the lesson (the pierce down a line of husks from the perch),
  and its walk is hard-coded in Walk43Foundry.cs, so a rebuild needs a bot change built and checked with it: not by the freeze. F's proposal
  (grow to x -10..10, galleries at 2.5 / 5 m each by its own ramp, P2 an 18 m ride, health on the perch, the walk's targets moved) goes to
  NIGHT53. Rejected: a plan-only rebuild that leaves the scripted walk pointing at moved pieces.
- **D-27 A feature is not shipped on an unproven Player line** (U's leftover 5, teach lines that wait for their verb): r19 / r20 show the
  line waiting (up at 8 s) but the reel's shot never fired in a152's context, so "cleared by the verb" was never seen. U has to 04:30 for a real
  shot; otherwise 2194211f and its reel commits are reverted before the freeze build (L-014: RED once and GREEN once). Rejected: shipping it
  on the EditMode table alone (a stuck teach line for 20 s is a visible regression if the listener misses in play).
- **D-28 The final GG-persona review (a3, kb3, g3, g4, p1; 04:25): "none of the five would satisfy GG as built; a3 is the closest".** Its
  findings are design-level, not contract failures, and each cheap fix (cut a ramp, a wall, the mirror's second half) drops its room under
  2.5 per 100 m2; they go to NIGHT53 as the rooms' second pass: a3's ride is no shortcut (walking round is ~1 m longer: raise the far floor);
  kb3's west ride runs beside a walkable bank (cut the bank with a lava inlet) and its floor-to-roof posts read as random pillars; g3's hopper
  is a giant box and the chute opens toward the door, not into the west lane; g4 is a mirror down the door's axis with the drone parked on
  it, and its canal does not read as a drop from the door; p1's south ramp makes the grapple optional in the room that teaches it; four rooms
  share one shape (a ring round a central mass). Rejected: changing rooms after the freeze began; dismissing the review because the numbers
  pass (the numbers are the walk pilot's, T-118).
- **D-29 The freeze starts at 04:27, not 04:59.** Every builder reported its last loop room (K g2 14/14, F p1 14/14 and a8 SHORT, U's
  proof moved to a252) and the queue's remaining rooms each need a bot or lesson change (g1, g6, a2 / a4 / a8, the links). Starting early
  buys the freeze's full batch suite (~17 min), the smoke, the flow table on five levels and every Player proof without crowding the close.
  Rejected: one more lead room to 04:59 (a room built after the reviews would ship unreviewed and squeeze the suite and the smoke).
- **D-30 The freeze re-opened at 04:36 for the final P-rank review's first-minute bugs** (10 findings on a3, kb3, g3, g4, p1). Taken, as
  coordinate fixes that each re-pass the contract, with the freeze build re-made and its whole reel set run once on it: kb3's blast vent
  1.6 m in front of the retry spawn (every respawn launched you onto the spitters' ladle: K); p1's well pad mid-lane (most laps round the
  drum launched you: F); a3's hoist pad 0.15 m off the entry floor's line into the east lane (F); g4's east ride ending in a dead-end pocket
  (the canal hoist moved into it) and its west ride landing half in a slot beside the NW ramp (the ramp flush: the lead, fix 6). Answered
  without change (NIGHT53's second pass, with D-28): a3's 1.5 m slot at the ring's NE turn (cut the ladle step's south 1.5 m); kb3's north
  crossing (the ride's end, the loop's turn and the exit in one 3 m strip: move the mould line 2 m south); g3's spill-wall pocket (cut the
  east spill wall: drops g3 to 2.38) and wave 2's nodes spread on three sides (the swarm never packs: nodes at the west lane's north end);
  p1's well-panel ride line crossed by the railing end, the engine deck's face and the hoist engine (a run-up block, the engine 1.5 m west).
  Also noted: the hold probe saw wave 1 only in a3 / g3 / g4 / kb3 (T-118); kb3's and g4's high frames are a drone at point blank.
  Rejected: shipping the pad misfires (a player meets them in the first minute); re-opening for every finding (four need a room re-cut).
- **D-31 The polish rows ship ON with their fallbacks; the U row closes SHORT** (05:26). On n52-freeze: hud52 a152 GREEN / RED; the teach
  lines' verb wait proven in a252 on spire (up at 8 s, a real KESTREL shot heard, cleared by the verb), so D-27's condition is met and it
  ships (kiln's line ended before 8 s: the queue rule likely, not checked); skin52 8/8 on spire with the scorch at NIGHT51's mark; guns52's
  machined guns 2/2, its muzzle-light timing 1/2 (a long first-shot frame each run, a different gun each time: T-122, not a lost verb: the
  off side runs as long); look52's tiers GREEN on foundry. Also at the freeze: span's verbs37 row 9 went RED on the first freeze build (p1's
  rebuild left an 11 m floored wall for the reel's pick) and F lengthened p1's chain wall (bee5387f: 31 / 31) before the final build.
  Rejected: guns52 OFF for the timing item (the guns' look is proven; the timing is a measurement on hitch frames).
- **D-11 The hold probe** (builder K's `hold52`): the player stands 20 s on each tier's best spot with the room's waves live and counts hits;
  a 0-hit spot is a camping spot and fails the room. Rejected: judging camping from the SVG alone (GG w29-w30 "you can camp" came from play).

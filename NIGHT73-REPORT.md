# NIGHT73-REPORT.md: MOTION (2026-10-27 evening, a 3.5 h session continuing NIGHT72; clock t0 17:49:52, cap 3:30)

## 1. What is still not good (ranked; gg-proxy's close look on n73-freeze, Logs/n73/gg-proxy-close.md, against its start ranking)
1. **The rooms are big empty boxes that do not ask you to move** (foundry-a2: three pills in a row on an open floor; foundry-b1 "11 LEFT" with no enemy
   in view; GG, seven times, 2026-10-09 to 10-26). Same as at the start; not an evening's work.
2. **Bosses wear the player's cyan, and THE CROWN's room is olive, magenta, brown, red and cyan at once** (Logs/n73/look/bosses-d.png). Better on the
   kiln only (kiln73); the weak points' colour language is GG's call (DECIDE73).
3. **You still cannot tell what a boss is or where to hit it:** core73 removed the wyrm's see-through box, but its open head is a flat tilted slab with
   three squares; THE CROWN says "OPEN 2.9" with no target in view.
4. **The speckled border after a hit is still there.** calm72 made the red vignette a thin rim (48 px), but Juice38's hurt "TV static" (GG's own
   2026-10-13 ask) still speckles about 200 px into the screen; the edge meter did not count it, gg-proxy saw it (Logs/n73/look/edge-windup-right.png).
5. **Flicker is down, not gone.** fix72 cut the still-camera flicker 42-87 % on every level, but the meter still reads 0.96-9.1 % per level against
   its 0.010 % bar. What is left is mostly intended motion the meter cannot tell from flicker (jump-pad particle streams, lava, enemy glows, the
   gun's idle sway); a moving-camera flicker (SSAO crawl, bloom on thin strips) is not measured at all.
6. **The movement's ULTRAKILL feel is unproven:** cam72 is ON, but no frame shows feel and GG has not played it.
7. **The boss kill does not land.** finish55's punch-in (8 deg) and letterbox bars were invisible three times, the third with the wyrm filling
   57 % of the frame: the beat needs a change a glance sees.
8. **Enemies still look rigid to a glance.** spring72's 17 % flinch squash was not visible over the game's own 15 % hit pop.
9. **Dark corners of boss rooms:** THE CROWN's ceiling centre is near black (900-1300, 150-450); THE ICEBREAKER's arena is 19.8 % near-black at idle
   (lit72's aim 15 %; a 0.18 lift reached 14.6 % but no viewer could see it, so it was not shipped).
10. goal72's line is ON but no frame tonight shows a cleared room with it in play (the stuck72 reel did); gg-proxy could not confirm it from the tour.

## 2. GG's play card in short
PLAY73.md. Six changes ON in the repo's config (the game GG plays) on top of everything NIGHT72 turned ON: **kiln73** (the wyrm's arena out of the
danger orange), **core73** (the wyrm's open weak point without the see-through box), **calm72** (the red border a thin rim, every cue kept),
**goal72** (the room's one line, verb first, at the exit), **cam72** (the ULTRAKILL camera: the higher slide eye, strafe tilt, the dash FOV),
**fix72** (less flicker). Each has its off switch.

## 3. What went ON (each with its pair and SPOT; verdicts in Logs/n73/verdict-*.md)
- kiln73 (builder K): danger share on the wyrm's five boss63 shots 83.1 % -> 7.9 % (bar 35 %), dark 2.5 %, mean luma 67.3 -> 72.7. SPOT: "The whole
  arena in A is washed in orange and red, while in B the walls, ceiling and floor are cool purple and violet; only the lava strip and the yellow
  beams are still orange in B." Cause: THE WYRM's two boss grades (BossGrade54's pushed split toning clamped blue; BossGrade55 amber), lit72 the amplifier.
- goal72: every line opens with a capitalised verb (new test); stuck72 GREEN with "GO to the exit" / "GO AHEAD to the exit" / "FOLLOW the arrow to
  the exit", retired on progress. SPOT: "A shows a boxed DOOR OPEN banner, and B shows a cyan arrow with GO to the exit in its place."
- cam72: the slide strip (eye 1.28 m ON vs 0.90 m OFF). SPOT: "A's camera sits much lower than B's ... In B the camera looks down onto the beige
  platform top." The dash strip was NOTHING OBVIOUS (fov 71.3 ON vs 85.8 OFF).
- fix72: L1's still72 flicker meter, first run in a Player: mean flip share OFF -> ON spire 5.92 -> 3.26 %, foundry 9.11 -> 2.82 % (its a8 pose is a
  moving-camera artefact, not counted), iceline 13.31 -> 6.13 %, kiln 9.83 -> 3.77 %, span 7.50 -> 0.96 %; no pose worse. No SPOT (a still cannot
  show a flicker): the lead's call, DECIDE73.
- calm72 (reworked by K: every cue kept on the thin frame): the deepest red edge 48 px ON vs 102 px OFF (bar 60), the hurt flash drawn thin on all 20
  hits. SPOT: "A has a much thicker red-orange haze around the screen edges ... In B the red is only a thin frame, and you can see the purple walls right
  up to the edges." Guard on n73-e PASS. Correction at the close: the 48 px is the vignette rim; Juice38's hurt static still speckles ~200 px in
  (section 1 item 4).
- core73 (B, round 2): the 1.45x translucent shell gone, the head kept cyan with a glow on its face (71 px inside the head's 89 px). SPOT: "A has a large
  see-through bright blue box around the cyan block in the middle of the screen, and B doesn't have it." Guard on n73-h PASS. Round 1 was refused on
  the lead's look (it hid where to shoot).
- The night's review (skygrind-reviewer, Logs/n73/review.md): 0 must-fix, 5 should-fix; S1-S4 fixed by K (76462c9d7: kiln73 no longer touches the
  EXPOSED edge; calm72's HUD search and kept sprites per HUD), re-checked on n73-h (kiln73 7.4 %, calm72 48 px).
- The combined guard on n73-c with the first four ON: verbs37 27/27, 29/29, 30/30, 31/31, 31/31; the smoke DONE (Logs/n73/guard-combined-c.md). Gates on
  1ad6da714: golden 0 mismatches, 22/22 replays exact, fingerprints EQUAL plain and --off57..--off67 (Logs/n73/gates-1.txt).

## 4. What failed or stayed OFF (with numbers)
- finish55: three strip rounds NOTHING OBVIOUS (fov punch 69.9, then 66.2 deg; round 3 with B's framing, the wyrm 57 % of the frame).
- spring72: the new spring73 reel (B) and a 17 % flinch squash (was 8 %); the close crops at hit + 1 NOTHING OBVIOUS.
- lit72 at A 0.18: THE ICEBREAKER 19.8 -> 14.6 % dark, NOTHING OBVIOUS; kept at 0.12.
- calm72 as built (NIGHT72): never paired: it removed the hurt flash and the EXPOSED edge above 30 % health (cues lost); the rework went ON.
- core73 round 1: refused on the lead's look (the head body-orange, the glow above it).

## 5. Not checked
A moving-camera flicker meter; a human's boss-kill frame; THE CROWN's ceiling (gg-proxy's dark patch at 900-1300, 150-450); the bosses' cyan;
glide braking on ice (NIGHT72's open point); level rows (GG's play-build rooms) still never paired.

## 6. Your calls
DECIDE73.md: fix72 ON without a SPOT; the wyrm's lost sulphur / umber grade and its hidden EXPOSED edge; cam72's narrower dash FOV; bosses in cyan.

## 7. NIGHT74's seed
- A moving-camera flicker meter (motion-compensated, or the same path flown twice and differenced), so fix72's rest and the SSAO crawl are measured.
- finish55 with a change a glance sees (an impact flash frame, a held view, thick bars), judged on B's framed bosskill72 (it already frames the wyrm).
- The review's S5-level notes (Logs/n73/review.md, 14 notes) and the calm72 "HUD rebuilt" path seen in a Player (not checked tonight).
- The colour language for boss weak points (GG's answer to DECIDE73), then the other bosses' arenas.
- The rooms (gg-proxy #1): GG's play-build rooms (the LV rows) paired at last.

## 8. The close lines
- Slate (Logs/n73/slate.json): 8 slices: PASS M2 cam72, M3 goal72, M6 fix72, K1 kiln73, K2 calm72, B4 core73; SHORT M4 finish55, M5 spring72.
- Flips (one commit each, both config copies byte-identical, Tools/night/n72-freeze-rows.cjs --slate=Logs/n73/slate.json): e426f0010 cam72, cde263567
  goal72, 13cd89ce2 fix72, 5fe5dbf1b kiln73, 823303f53 calm72, 77f8321b3 core73.
- Freeze build n73-freeze at 5ad6c89bf (the repo's config, all six ON): verbs37 n52-foundry 27/27, n52-iceline 29/29, n57-kiln 30/30, n52-spire 31/31,
  n52-span 31/31, all PASS, NRE 0 (Logs/feel/n73-freeze/summary47.txt); boss63 GREEN on the wyrm and THE CROWN; tour65 on n52-foundry RED as on n72-play
  (no line65 frames on that map; pre-existing); the smoke DONE: all steps passed (Logs/player/n73-freeze-smoke.log).
- Gates on 5ad6c89bf (Logs/n73/gates-close.txt): config copies identical; golden 39 vector sets, 0 mismatches, 22/22 replays exact; fingerprints EQUAL
  plain and --off57..--off67: "gates: all GREEN 19:47:22". No level file changed tonight, so the flow gate was not re-run.
- Play build: Builds/n73-play (a hard-link copy of n73-freeze), its smoke DONE: all steps passed (Logs/player/n73-play-smoke.log).
- Builds tonight: n73-a .. n73-h, n73-freeze, n73-play. Agents: builders K (4 jobs) and B (4 jobs), 11 spotter runs, gg-proxy twice, one reviewer.
- Estimate vs actual: the NIGHT72 close took 17:01-17:50 (most of it the clock's wait); NIGHT73's slices ran 17:50-19:52; the longest gap with no
  agent and no Player job: the 17:20-17:49 wait for NIGHT72's clock.

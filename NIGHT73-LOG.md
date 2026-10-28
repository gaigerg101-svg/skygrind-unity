# NIGHT73-LOG.md: MOTION (a 3.5 h session continuing NIGHT72; prompt NIGHT73.md, untracked)

## Bearings (17:01-17:50, before the n73 clock)
- 17:01 HEAD 970f99946 (NIGHT72's close commit) on port/stage3. NIGHT72's close was unfinished: no close-start, no u3-n72, the clock running
  (6:43 of 7:30), NIGHT72-REPORT.md untracked, HANDOFF / CORE / the log uncommitted. C: 434 GB free; commit free 93.8 GB.
- No Unity Editor was running. 17:08 `unity open`; it waited on "Recovering Scene Backups" (a crashed earlier Editor). The lead chose Yes
  (copy and preserve the backups in Assets/_Recovery/; nothing discarded). 17:13 the scripts compiled (goal72's edit inside).
- 17:03-17:08 NIGHT72's close sides re-run on n72-freeze under the names its close check reads: all PASS (NIGHT72-LOG.md).
- Written before the clock (untracked or uncommitted until the close of NIGHT72 is tagged): NIGHT73.md; Tools/night/n73-clock.cjs (FREEZE 3:00,
  CAP 3:30), n73-reels.sh (--keep: no prune), n73-prune.sh (named sides), n73-strip.py (peek / strip), n73-pair.cjs; the goal72 wording
  (Goal72Math.cs stage 2) and the test Goal72_EveryText_OpensWithACapitalisedVerb (N72NTests.cs). The test is red on the old lines by
  construction ("The exit is [AHEAD]" does not open with "["); not run against the old code.
- The lead's look at n72-play's frames (Logs/n73/look/play-foundry.png, play-wyrm.png, play-crown.png): THE KILN WYRM's arena is red-orange
  wall to wall; Tools/night/n73-hue.py: danger share 92.5 / 94.8 / 88.1 / 44.2 / 95.8 % on the five boss63 shots (lit72 OFF on n72-t2:
  77-87 %; the kiln's ordinary rooms 19-34 %; a foundry room 2.4 %). The room light is pale violet (0xcfc6ff), not the cause. -> builder K.
  THE CROWN's ring around the crosshair is the boss's own body (not an effect). The spire arena's magenta-pink walls: DECIDE72's open call.
- 17:49:36 n72 clock stopped (7:30:16); `n72-close-check.cjs`: every check PASS but clock ("want 7:00 to 7:30": stopped 16 s past 7:30):
  CLOSE-CHECK n72: MINIMUM PASS. Tag u3-n72 -> d56a97c0a.

## The session (n73 clock t0 17:49:52; FREEZE 20:49, CAP 21:19)
- 17:50 shared commit bbc60fe7f: design.n73.kiln73 OFF in both copies (byte-identical) + FrameRows; the goal72 lines; the n73 tools.
  Tests: N72NTests 15/15 (with the new verb test); Rows72 6/7 then 7/7: ViewRowSwitchedBothWays assumed lit72 OFF, which NIGHT72's flip made
  stale; it now reads the config's state before the switch.
- 17:51 worktree n73-K (bbc60fe7f) and builder K spawned on the kiln arena (Research/2026-10-27-wave73/brief-K.md; bar: ON danger share
  mean at or under 35 %, dark share at or under 15 %).
- 17:52 build n73-a (bbc60fe7f): exit 0. ProjectSettings drift: UnityConnectSettings / EditorBuildSettings only (GG's; not staged).
- 17:53 reel set a1 (--keep: every frame): cam72 boost / slide on n52-foundry, finish55 bosskill72 and spring72 kill12 on n57-kiln, ON vs OFF.
  All sides NRE 0, exceptions 0 (Logs/feel/n73-a/summary47.txt).
- The motion strips (n73-strip.py: six frames evenly spaced, 640x360 each, no labels) and the blind spotter (Logs/n73/spot/<pair>/spot.md):
  - cam72 boost f0014-f0044: telemetry ON fov 71.3 at the dash, then 76.7 and roll -2.3; OFF fov 85.8 (the old wide punch). SPOT: NOTHING OBVIOUS.
  - cam72 slide f0020-f0050: ON eye 1.28 m, roll -5 then -1.8, fov 74 -> 83; OFF eye 0.90 m, roll +4, fov 87. SPOT names it: "A's camera sits
    much lower than B's ... In B the camera looks down onto the beige platform top" (A = OFF, B = ON).
  - finish55 bosskill72 f0522-f0642: ON fov 74 -> 69.9 at the dying beat and the letterbox bars; OFF fov 73.0. SPOT: NOTHING OBVIOUS. The lead's
    look: the top bar is dark on a dark ceiling and the 3.5 deg punch is gone by the second frame. -> Finish55FovDeg 3.5 -> 8, recover 0.5 -> 0.8 s
    (UnityTuning.N72B.cs), for a retake on the next build.
  - spring72 kill12: no pair. The scenario kills its husk with one shot at 12 m, so the flinch spring never fires ("flinch squash peak 0.000",
    Logs/player/n47-reel-n73-a-spon.log); only the 2.2 % idle breath runs, a few pixels at 12 m. SHORT on this instrument.
- goal72 stuck72 (set a2): ON GREEN with the stage lines "GO to the exit" (0 s), "GO AHEAD to the exit" (10 s), "FOLLOW the arrow to the exit"
  (20 s), retired when the player closed in (Logs/player/n47-reel-n73-a-sfon.log); OFF RED (the row off). The stage-2 shot shows the cyan verb
  and the arrow (Logs/n73/look/goal-stage2-crop.png). Telemetry's ui text stays "GO to the exit" in all 1501 frames: the probe's text is
  stale, the screen is right (a meter quirk, logged, not fixed). Pair f0950 ON vs OFF: SPOT names it: "A shows a boxed DOOR OPEN banner, and B
  shows a cyan arrow with GO to the exit in its place" (A = OFF, B = ON).
- Flicker (set a2; L1's still72 meter, first run in a Player; n52-spire, 3 poses x 60 still frames; Logs/n73/flicker/<side>.txt):
  - plain s1: flip share 7.14 / 8.19 / 2.42 % (mean 5.92 %), every pose RED against the meter's 0.010 % bar;
  - planted s1p: "plant rect 1024 of 1024 px flip" on every pose (the chain sees a planted flicker);
  - fix72 ON s1f: 2.49 / 6.30 / 0.97 % (mean 3.26 %): down 65 / 23 / 60 %.
  - The heatmap (Logs/n73/look/flicker-v5.png): a speckle over every wall (the grain fix72 targets), plus blobs that are intended motion (the
    health pickup's pulse, an enemy's glow, the gun's idle sway). The bar of 0.010 % cannot be met while those animate: the meter does not
    separate animation from flicker. Not checked: a moving camera.
- 18:03 the combined guard on n73-a (cam72 + goal72 + finish55 + spring72 ON): verbs37 27/27, 29/29, 30/30, 31/31, 31/31 PASS. The a2 set's prune
  thinned set a1's kept frames (the strips were already cut); n73-reels.sh now prunes only its own sides.
- 18:04 build n73-b (54523019a: Finish55FovDeg 8). finish55 retake: fov 74 -> 66.2; SPOT NOTHING OBVIOUS again: M4 SHORT (two rounds; the kill frame
  has no subject: an orange wall and a lit box, the wyrm out of view). Logs/n73/verdict-finish55.md.
- 18:05-18:12 fix72 flicker on the other four levels (Logs/n73/flicker/): mean OFF -> ON foundry 9.11 -> 2.82 % (without a8, a moving-camera
  artefact: 48.0 / 40.5 %), iceline 13.31 -> 6.13, kiln 9.83 -> 3.77, span 7.50 -> 0.96; no pose worse. Logs/n73/verdict-fix72.md.
- 18:06 gg-proxy's start ranking (Logs/n73/gg-proxy-start.md): 1 rooms are big empty boxes (7 asks; not an evening), 2 colours: the boss is its
  room's colour (the wyrm; THE CROWN in the player's cyan), 3 flicker on a still camera, 4 the bosses cannot be read, 5 the movement not ULTRAKILL yet.
- 18:09 builder K's report: the kiln orange is THE WYRM's two boss grades (BossGrade54's pushed split toning clamps blue; BossGrade55's amber), lit72
  the amplifier. kiln73 picked (1bb72d292, ca1288c65, 4f94c0637), compile clean; Kiln73Tests 5/6 then 6/6 after K's ea6e5744e (a slip in the cause
  test's hue arithmetic).
- 18:15 build n73-c; boss63 kiln ON vs OFF: danger share mean 7.9 % vs 83.1 % (bar 35 %), dark 2.5 %; SPOT names it (the arena cool purple and violet,
  only the lava and the yellow beams orange). The speckled border on two ON shots is the HURT edge (calm72 line "drawn hurt 0.54"), kept.
- 18:17-18:26 the combined guard on n73-c with cam72 + goal72 + fix72 + kiln73 ON: verbs37 27/27, 29/29, 30/30, 31/31, 31/31 PASS, NRE 0; the smoke
  DONE: all steps passed with `[rows72] cam72=1 goal72=1 fix72=1 kiln73=1` (Logs/player/n73-smoke-rows.log; focus changes 0). Logs/n73/guard-combined-c.md.
- FLIPS (one commit each, both copies byte-identical, n72-freeze-rows.cjs --slate=Logs/n73/slate.json, every PASS bound to passSha 4f94c0637):
  e426f0010 cam72, cde263567 goal72, 13cd89ce2 fix72, 5fe5dbf1b kiln73.
- 18:19 builder B's report (spring72 flinch 8 -> 17 % squash, kick -4.5 -> -15.5 at 7 Hz z 0.35; a new reel scenario spring73; n73-springcut.py).
  Picked (221c8cd39, a34794881, de2d59639); Spring72Tests 13/13. Build n73-d (with the four flips): spring73 reads RED on its own counter ("hits landed
  0 of 5") while the telemetry has the five hits and the kill on both sides (EQUAL); sent back to B. Crop pair at the hit + 1 frames: SPOT NOTHING
  OBVIOUS. M5 SHORT: OFF already pops 15 % per hit (crunch36), the spring adds little a glance tells.
- 18:31 boss63 on the other four bosses (n73-d, the repo's config): danger share mean foundry THE WICK 0.5 %, iceline THE ICEBREAKER 0.9 %, span
  THE LOOM 0.2 %, spire THE CROWN 8.6 %; dark share at idle 2.7 / 19.8 / 6.0 / 3.9 % (the iceline boss over the 15 % aim). The lead's look
  (Logs/n73/look/bosses-d.png): THE WICK, THE LOOM and THE CROWN are drawn in cyan, the player's colour (rule 8; thread T-153; cyan71 failed at
  NIGHT71): GG's call, in DECIDE73.
- calm72 (built at NIGHT72, never paired) read before any pair: above 30 % health it disables the hurt flash, the damage vignette and the EXPOSED
  edge: the hit cue and the anti-camp cue would be lost (PRINCIPLES rule 1). Sent to K to rework: every cue kept, on the thin frame, EXPOSED's
  pulse halved.
- 18:38-18:42 lit72 on THE ICEBREAKER: lit72:0 47.9 % dark, the shipped 0.12 19.8 %, A 0.18 14.6 %; A 0.18 on the other four bosses brighter, nothing
  blown (Logs/n73/look/lit018.png). SPOT on the iceline pair: NOTHING OBVIOUS: lit72 stays at 0.12 (the 15 % aim missed by 4.8 points, reported).
- 18:42 gates on 1ad6da714 (Logs/n73/gates-1.txt): config copies identical; golden 39 vector sets, 0 mismatches, 22/22 replays exact; fingerprints EQUAL
  plain and --off57..--off67: "gates: all GREEN".
- The flicker leftovers read: after fix72 the biggest blobs are the jump-pad beams' rising particle streams (Pad27StreamRate 12/s; the beam's pulse is
  a 1.7 s sine), the lava, the health pickup's 90 deg/s spin and enemies' glows: motion, which a still-camera meter counts as flicker.
- 18:45 builder K's calm72 rework picked (29b03e131, 5b0e3486a); Calm72Tests 6/6, Kiln73Tests 6/6. Build n73-e. boss63 kiln ON vs OFF: the deepest red
  edge (Tools/night/n73-edge.py, K's meter) 27.2 px mean / 48 px deepest ON vs 54.8 / 102 OFF (bar 60); "[calm72] hit N ... hurt 0.51 thin 58 x 32 px" on
  all 20 hits; SPOT names it ("A has a much thicker red-orange haze ... In B the red is only a thin frame"). Guard on n73-e with calm72:1: verbs37
  27/27, 29/29, 30/30, 31/31, 31/31 PASS; the smoke DONE (Logs/n73/guard-calm72-e.md). FLIP 823303f53 calm72 (passSha 1c82903b4).
- 18:57 builder B's bosskill72 framing picked (e32464b36). Build n73-f: "framing FRAMED (57 % ON / 50 % OFF)"; the finish55 strip SPOT NOTHING OBVIOUS a
  third time: M4 stays SHORT (Logs/n73/verdict-finish55.md).
- 19:00 skygrind-reviewer started on u3-n72..HEAD (Logs/n73/review.md). design.n73.core73 placed OFF (6b47bda82, Tools/night/n73-row-add.cjs);
  builder B on core73 (the wyrm's open weak point: a lit core in the body instead of a flat translucent cyan box; gg-proxy #4).
- 19:13 core73 picked (cc27d0494); Core73Tests 4/4; build n73-g; boss63 kiln: ON "drawn as core ... 77 px across, today's box 134 x 134 px", OFF "drawn
  as box". The lead's look on the stagger pair: ON's head reads body-orange and the cyan core is a faint haze above it: the shoot-here cue weakened, B4
  SHORT, no SPOT taken (Logs/n73/verdict-core73.md). Round 2 sent to B: keep the head cyan, drop only the 1.45x shell.
- 19:14 skygrind-reviewer (Logs/n73/review.md): 0 must-fix, 5 should-fix, 14 notes; nothing changes a drawn frame with every new row OFF. S1: with
  calm72 OFF and kiln73 ON, kiln73's K2 hides EXPOSED in the wyrm's arena (calm72's off switch no longer gives back the NIGHT72 game); S2: calm72's HUD
  search cap of 40 never resets across level loads (after ~40 loads the thick frame returns with the row ON); S3: the restore puts the previous
  scene's sprite on the new HUD; S4 / S5 timing. S1-S3 sent to K.
- On n73-g (calm72 ON from the config) the border measures 27.4 px mean, 48 px deepest: the downscaled sheet's haze is the CRT grain on the thin rim.
- 19:20 memory harvest committed (52c54b5b7): ledger wave-73, L-157, T-159 (w73's SHORT items; its w71 list moved to ledger wave-71), FEATURES w72 / w73.
- 19:21 K's review fixes picked (76462c9d7: S1 kiln73's EXPOSED hide retired; S2 calm72's HUD search per HUD; S3 kept sprites per HUD; S4 the swell
  age on the loop's clock); Calm72Tests 8/8, Kiln73Tests 5/5. Gates on c41e42f82: all GREEN 19:22:49 (Logs/n73/gates-2.txt).
- 19:24 core73 round 2 picked (0014c6c95: the head kept cyan, only the 1.45x shell dropped, the glow on the head's face); Core73Tests 12/12; build n73-h.
  "[core73] ... 71 px across, inside the head's box on screen: yes; the head's box = 89 x 89 px, today's shell = 134 x 134 px | head lit cyan (kept)".
  SPOT names it ("A has a large see-through bright blue box around the cyan block ... B doesn't have it"; A = OFF). The same build's plain side re-checks
  K's fix: kiln73 7.4 %, calm72 48 px, the hurt drawn thin. Guard with core73:1: verbs37 five sides PASS, the smoke DONE. FLIP 77f8321b3 core73.
- 19:35 code freeze. Build n73-freeze (5ad6c89bf): verbs37 27/27, 29/29, 30/30, 31/31, 31/31 PASS; boss63 GREEN (wyrm, crown37); tour65 RED as on n72-play
  (no line65 frames on n52-foundry: pre-existing); the smoke DONE. Gates on 5ad6c89bf all GREEN 19:47:22. Builds/n73-play = cp -al of the freeze; its
  smoke DONE 19:51. gg-proxy's close look started (Logs/n73/gg-proxy-close.md).

## Close table
| slice | state | evidence | where it goes next |
|---|---|---|---|
| M2 cam72 | PASS, ON (e426f0010) | Logs/n73/verdict-cam72.md | GG's A / B on the dash FOV (DECIDE73) |
| M3 goal72 | PASS, ON (cde263567) | Logs/n73/verdict-goal72.md | done |
| M6 fix72 | PASS, ON (13cd89ce2), no SPOT possible | Logs/n73/verdict-fix72.md | a moving-camera meter (THREADS T-159) |
| K1 kiln73 | PASS, ON (5fe5dbf1b) | Logs/n73/verdict-kiln73.md | GG on the wyrm's identity colour (DECIDE73) |
| K2 calm72 | PASS, ON (823303f53) | Logs/n73/verdict-calm72.md | GG on the thin EXPOSED edge (DECIDE73) |
| B4 core73 | PASS, ON (77f8321b3) | Logs/n73/verdict-core73.md | done |
| M4 finish55 | SHORT (three rounds NOTHING OBVIOUS) | Logs/n73/verdict-finish55.md | THREADS T-159; NIGHT73-REPORT section 7 |
| M5 spring72 | SHORT (NOTHING OBVIOUS over crunch36) | Logs/n73/spot/spring73/spot.md | THREADS T-159 |
| lit72 at 0.18 | not shipped (NOTHING OBVIOUS) | Logs/n73/spot/lit018/spot.md | THREADS T-159 |
| the bosses in cyan | GG's call | Logs/n73/look/bosses-d.png | DECIDE73; THREADS T-153 |
| the review's 14 notes | open | Logs/n73/review.md | NIGHT73-REPORT section 7 |

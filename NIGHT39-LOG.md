# NIGHT39 log (the lead): the feel night

Prompt: NIGHT39.md (untracked). Box 5:00, hard cap 5:20. t0 = 14:29 local (2026-10-13), GG's /goal.

## U3-52.1 Start
- Editor ready, play mode stopped. Free memory 13 GB. Base 540eea86 (port/stage3, after u3-n38). GG's four files modified, untouched.

## G0 (b) the ghost weapon (14:29-14:45): FIXED; the harness proof SHORT
- Cause, proven by the code path: n35-kiln's room k1 has `"award":"thunderclap"` (a display name). On the room's clear,
  Campaign24.cs:850-853 calls `world.AwardWeapon(r.award)` -> Loadout.AddWeapon (Combat.cs:943), which adds ANY string to `owned`.
  Combat fires by id (`"revolver"`, `"shotgun"`, `"nailgun"`, ...: Combat.cs:648-903): no branch for "thunderclap", so the slot never fires.
  Every kiln level since n29 carries the same award; n38-kiln and the tutorial use `shotgun`.
- Harness: `n38 limits n35-kiln k1` shows the award is NOT given on entry (owned revolver, gauntlet in k1); `walk29 n35-kiln` cannot take
  that level name (a file-path error), so the clear was not driven in the harness. **The harness proof of the ghost is SHORT**; the
  proof is the code path above.
- Fix: `Tools/night/n39-ghost-fix.cjs`: "thunderclap" -> "shotgun" in n35-kiln (both copies, byte-identical). A data fix, deliberately NOT
  behind --off39 (a bug). Fingerprint EQUAL after the fix. Older kiln files (n29-n34) are not in the campaign order: left as they are.

## The shared commit (14:29-14:40): dd013f7b
- N39Blocks (p39, ride39, boss39), design.n39 (p39, ride39, boss39, slow39; every row off), --off39 / -skygrindOff39 (+ N29Fixes),
  tape 22, SurvivalSim.N39.cs, harness partials (N39Pistol / Ride / Slow / Boss / Views), the test chain. Golden 0 mismatches (22/22);
  fingerprint EQUAL plain and --off39; compile clean; N39 2/2, N38 3/3, N37 3/3, N36 3/3, Tape 14/14, Lesson25 10/10.
- NIGHT39-BRIEF.md written; the seven builders launched at 14:41 (0:12): K4, P1, R1, F1, B1, E, V.

## G0 (a) the campaign boot (14:45-15:02): PROVEN
- `n33-check.sh ... -skygrindMap n38-span -skygrindReelOnly verbs37` alone: the SURVIVAL attract again (56 lines): `-skygrindReelOnly` only
  filters scenarios; the reel itself needs `-skygrindReel <dir>`.
- **The working boot:** `NIGHT=n39 bash Tools/night/reel.sh Builds/<tag> <tag> <side> <timeout> -skygrindMap <level> -skygrindReelOnly verbs37`
  (reel.sh adds `-skygrindReel <dir>`). On n38-r1: `[reel] reel on 'n38-span'`, `verbs37: start level n38-span (span), owned [revolver,
  gauntlet, shotgun]`, 0 NullReference.
- **C1 row 2, the first verbs on a NIGHT38 level (the before):** n38-span 28/30, RED rows 9 (wall ride: "WallRideStart 0 ... on a 30 m wall
  20.0 m high", room p1's open floor) and 11 (the jump-off: no ride, so no boost). Hypothesis, unverified: the reel's wall on n38-span is
  tagged `noride` (O2 tagged void side walls noride). Asked R1 for a harness check; the lead decides the fix.

- **The rest of the before (15:09):** n38-foundry 25/26, RED row 9 (two rides ended on `corner` after 0.12 s and 0.45 s on the reel's
  30 m wall in a2; sent to R1); n38-spire 26/30, RED rows 17 / 19 / 22 / 23 ("no grapple node in clear sight of a floor spot on this
  level": C2's rebuilt rooms carry no nodes and the gate rooms turn the grapple off, so the reel has nowhere to grapple; not a lost verb,
  not proven either). 0 NullReference on both. C1 row 1 (the three boots) DONE by this path; row 2 has its before.

## C1 row 3, round 1 frames (15:10-15:20)
- No build spent: `Tools/night/n39-build-rows.cjs n38-r1 on n38.doors38 n38.hud38 n38.juice38` turns the view rows on in the BUILD's
  config copy only (the repo's config untouched). Reel: `level36` on n38-foundry: PASS 11/11 rooms, 33/33 frames, 0 NullReference;
  1417 frames in Logs/feel/n38-r1/views-foundry/level36/.
- Looked at f0300 (1.2 KESTREL), f0800 (1.6 THE YARD), f1200 (1.10 PIERCE):
  - doors38: the open curtain reads cyan with scanlines (f0300 clear; f0800 only a thin sliver at distance). In f1200 (enemies alive)
    the far curtain reads PALE WHITE / PINK, not red-orange (D-2: locked = red-orange): the state colour or the door type is wrong.
  - hud38: the three-segment bar shows under the crosshair (three cyan dashes) but is very thin and small for "easy to tell".
  - juice38: no boss entrance or hit in these frames.
  - Sent to V (round 2 fixes); nothing turned on yet.

## V round 1 (merged c43c80b2, 15:40)
- C1 row 8 DONE: AutopilotTests.PerStep_AllocatesNothing measures only SampleMove / SampleCombat (outside sim.Step, where NIGHT38's code
  runs); the headless alloc probe: 0 B in 43,200 windows at HEAD, --off38 and u3-n37 (bit-identical); the same red appears in the
  NIGHT17 / 18 / 21 / 33 / 38 suites: pre-existing and load-related. The assert unchanged; a red now prints the step and the call.
- C1 row 9 DONE: MapChoiceTests split into one case per map (48 maps headless in 46.2 s; n38 put the single case at ~247 s in the suite).
- doors38: the f1200 door is the boss door (d10 into THE STACK): a locked boss door now draws red-orange inside its violet frame;
  distant open curtains brighter (up to 2.2x by 40 m), a frame >= 4 px beyond 16 m. hud38: cells 17x4 -> 31x10 px.
- Lead's checks on the merged tree: fingerprint EQUAL plain and --off39; compile clean (0 console errors); AutopilotTests 9/9,
  MapChoiceTests 50/50 in the Editor. Frames to retake after the next build.

## R1 round 1 (merged 0b6c7117, 15:57)
- ride39 built, row OFF: design.n39.ride39 speedBuff 0.15, exitBoost 1, chainCap 16, upLow 4.6, upHigh 10. Finding: wallride35's
  chainCap never held a chain (the 8 m/s jump-off push became along speed on the next ride); new rule capJumpOff (Ride35.Boost39, one
  ternary inside Mover.cs's wallride35 jump-off). Chain speed over six walls: before 10.4 / 15.2 / 19.1 / 20.7 / 22.2 / 23.6 m/s; after
  9.2 / 13.2 / 16.0 / 16.1 / 16.1 / 16.2. Jump-off rise 0.89 -> 1.09 m (pitch 0), 1.72 -> 2.08 m (+45).
- THE GANTRY with ride39 on: gates GREEN 2/2 (+32 %, +26 %), the walk RED 0/5 (the pilot stalls in gate 2's pit): ride39 stays OFF.
- The verbs REDs explained (`n39ride verbs9`): n38-span's reel wall is box 5, a NORIDE liner (with noride ignored a ride holds 2.3 s);
  n38-foundry's reel spot drops into THE GANTRY's pit 1. Both the reel's wall pick (Wall35), not a lost verb: sent to V.
- Lead's checks on the merged tree: golden 0 mismatches (22/22); fingerprint EQUAL plain and --off39; compile clean (0 console
  errors); Ride39Tests 3/3. R1 round 2: the GANTRY walk under ride39, the noride texture, ride38's a5.

## F1 round 1 (merged e0f043fa, 16:10): G0 (c) DONE
- `n39slow signal`: the slowAim27 signal fires on 100 % of rides (KESTREL and THUNDERCLAP; rows as configured, ride38 on, wallgrip36
  off, wallride35 off; HOLD and TAP). The cause of "it feels removed" is the 0.75 scale: a 3.99 sim-s ride lasts 5.32 real s at 0.75,
  13.15 real s at slow39's 0.3. This machine's saved settings: slowMo true, reducedMotion false (written 15:12, maybe by a lead
  Player run): GG's own settings are a question for the report.
- slow39 (design.n39.slow39, OFF until the frames): scale 0.3, ease in 0.15 s / out 0.25 s, the whip latch in the same condition, the
  parry's slow window (0.25 for 0.3 s after the 0.25 s freeze, back over 0.35 s); the parry streak on world time, wider, brighter.
  VERBS 9, 11, 20 (slow), 40 (emphasised). Fingerprint EQUAL plain and --off39 after the merge.

## Build n39-r1 (16:14-16:17): Succeeded, 0 errors, 53 s; from the tree with V round 1, R1 round 1, F1 round 1. The TMP font
rewrite (_ScaleRatio) restored. V round 2 merged after the build (5659ce9f): the verbs reel's wall pick (VerbsWall39.cs) skips noride
faces and pits: `n39views wall` rows 9-11 PASS on n38-span (p5), n38-foundry (a7), n38-spire (v6) and on ten other levels; the Player
proof waits for the next build.

## Frames on n39-r1 (16:18-16:25; slow39 + doors38 + hud38 + juice38 on in the build's copy only)
- level36 on n38-foundry: PASS 11/11, 0 NullReference (V's door / bar fixes NOT in this build: they landed in round 1, merged before
  the build: in). slowworld: RED: the min scale 0.300 (the reel still expects NIGHT27's 0.75: expected), 4397 frames slowed while not
  riding + aiming beyond the reel's 0.10 s ease-out, 2 of 13 control frames under 1. f0900: a strong full-screen RGB split + tint and
  the bigger charge bar under the crosshair; whether it is slow39's cue or juice38's hurt static is F1's round-2 question.

## E round 1 (merged 1f5fa063, 16:20): the ice judge and the two-pull grapple
- iceline G1 (skate) RED: dash jump + blast reaches 57.28 m dry vs the skate's 57.27 m; with pistolOnly 37.47 vs 37.45: no skate-only
  gap exists on flat ice with today's glide (for GG: G1 is a flow beat). x6 (the finale's two-pull grapple) +15 %, RED on 35 cheats;
  PASS with a what-if pistolOnly. Selftest 10/10.
- **span38 RED 0/3** on the current checker (the boost x3 crosses each 30.5 m gap; RED since NIGHT38's round-4 boost sequence, after
  the row went on). The lead's own run: RED 0/3. **span38 turned OFF** (9b836034); E owns tonight's fix (a room LIMIT on p2 / p4 that
  keeps the grapple, or a ceiling).

## P1 round 1 (merged d48b0218, 16:27)
- p39 (OFF): a press released within tapS 0.15 s fires the tap on the release; held past it, no tap, the charge; INVERSUS's aim
  correction (full to 0.11 s, none by 0.19 s); the KESTREL only (THE THUNDERCLAP keeps tap36). Added latency = the click length (40-140
  ms clicks fire 42-142 ms after the press). Heavier: fire interval x1.2 (48 -> 58 steps), Pistol39View (kick, 0.32 s settle, hammer,
  bloom, the cannon), hitstop 0.05 s body / 0.1 s charged. The crosshair spin on the KESTREL (shows with hud38 on). Outside its files:
  Combat.FireRevolver `getAim()` -> `ShotAim39()` (the same unless a corrected tap fires); one hook line in UiHud38.
- Lead's checks on the merged tree: golden 0 mismatches (22/22), fingerprint EQUAL plain and --off39, compile clean, Pistol39Tests 4/4.
  p39 on would turn the verbs reel's rows 25 / 27 and N36.Guns' tap36 lines RED: P1 round 2 makes them read the row. The tap DPS -17 %:
  B1 measures bands with p39 off and on.

## K4 round 1 (merged 6548c678, 16:35): level 3 rebuilt, kiln38 ON
- k5 loads `Design/maps/wyrm26-arena.json` through design.n26.bosses.wyrm26.arena (half 30), not the hard-coded arena. New arena
  `Design/maps/wyrm39-arena.json` (gen-n39-wyrm-arena.cjs): wyrm26's shell (half 30, height 30) and paths kept, new perches / ride slabs /
  stacks 2.9 m clear of every worm route; the worm's AI untouched. The lead ran K4's `Tools/night/n39k-wyrm-hook.cjs` (read first: two
  anchored single-line call sites in SurvivalSim.Wyrm24.cs; a level without `wyrmArena` keeps wyrm26's arena).
- The level: every gate a CLIMB (a 9.5 m noride cliff: above the blast's 7.75 m + 10 %, under the THUNDERCLAP climb's 13.36 m), gates only
  in flight rooms; rooms THE CHARGE, THE FLUE (G1), POUR HALL (boost through the pack onto the crucible), THE STACK (G2a / G2b), CASTING
  FLOOR, THE SPOUT (G3), REST, k5. The award `shotgun`.
- Checks: the lead's own `n38 gates n38-kiln` GREEN 4/4 (owns pulse); K4: G1 +49 %, G2a +69 %, G2b +55 %, G3 +49 %; walk 10/10 seeds
  (median 197.4 s; the bot dies 8x in kb2, 2x in kb3 over the 10); levelgen 0; n36-jobs GREEN; fingerprint EQUAL plain and --off39 with
  the hook. K4's `n38kiln blastwall` probe: the blast + a wall jump off the cliff face lands on no gate (0 / 864 each) but lifts the feet
  to 11.03 m: sent to E to add to the checker; kiln38 goes off if it lands.
- Note: kiln38 is a NIGHT38 level row, so `--off39` keeps it on; `--off38` gives the old level 3. Rule slip: K4 used one `node -e` on its
  own generator.

## P1 round 2 (merged 334ebb8c): the reel checks read p39
- FeelReel.N35.Verbs rows 25 / 27 and FeelReel.N36.Guns' KESTREL lines expect p39's behaviour when the run's p39 is live, tap36's when
  not; `n39reels` GREEN 8/8 (p39 on: row 25 +12 steps, row 27 0 normal shots, TAP +7 steps, HOLD 0 press shots; off: the tap36 values).

## R1 round 2 (merged df77d625) and ride39 ON (74d8a1d6, 16:40)
- THE GANTRY under ride39: the walk pilot's re-press 30 -> 40 steps under the ceiling + gate 2 taken from L1; no geometry changed; gates
  GREEN 2/2, walk b1..b2 10/10 (on and off). NoRide39View built (dark plates + vertical fins; neither cyan nor red-orange), its look
  OFF (design.n39.ride39.noRideLook); level 1 has no noride surfaces (kiln 12, span 7, spire 8, finale 9, iceline 2). ride38's a5
  deaths: SHORT (not started). Open: `n36move walk` sticks in room a1 on n37-/n38-foundry even with --off38 / --off39 (pre-existing).
- **ride39 turned ON**: the lead's gates with it on: n38-foundry GREEN 2/2, n38-kiln GREEN 4/4, n38-spire GREEN 4/4; fingerprint --off39
  EQUAL. VERBS rows 9 / 11 (changed by ride39) get their Player proof on n39-r2.

## B1 round 1 (merged d30eabb9): every boss in the 30-60 s bot band with boss39 forced on (row OFF until the look)
- Bot-to-person factor 2 (the w31 / w32 record: bot 15-22 s was "~40 s person"). Before -> after p50 (bot, 10 seeds, all killed):
  WICK total 34.3 -> 44.7 s; STAG 20.9 -> 37.9; KILN WYRM 20.5 -> 40.3 (pool x2.75; AI unchanged; measured in the old arena: re-measure
  in wyrm39-arena asked); LOOM 21.6 -> 45.4 (stagger and rest 2.5 s; rest 2 s killed only 4/10); CROWN 21.9 -> 36.5 (gems 8 -> 20);
  Stag31 (level 6) 19.6 -> 44.4. Brain levers first; health where the pool decides the legs. No Boss37.cs change; design.n37.* untouched.
- p39 column: unchanged on wick / stag / loom / crown (p39 live 10/10); not live in the wyrm and Stag31 rigs (unchecked there).
- Lead's checks after the merge: the config copies identical, fingerprint EQUAL plain and --off39, compile clean.

## Build n39-r2 (16:48, 0 errors) and its Player runs (16:50-16:53; p39, slow39, boss39, doors38, hud38, juice38 on in the build's copy)
- verbs37 n38-foundry **26/26** (rows 9 / 11 PASS with ride39 on and V's wall pick: the Player proof; rows 25 / 27 PASS with p39 on).
- verbs37 n38-kiln 29/30: row 13 (slide bounce) RED "slid False ... wall 26 m with floors" in kb1: sent to K4 (the room or the reel).
- verbs37 n38-spire 26/30: rows 17 / 19 / 22 / 23 RED "no grapple node in clear sight of a floor spot" (as on n38-r1: the reel has
  nowhere to grapple on this level; not a lost verb, not proven either).
- level36 frames n38-kiln RED: 7/8 rooms entered, 2/3 arena rooms locked: sent to K4. 0 NullReference in every run.

## F1 round 2 (merged debb4815): the slowworld RED explained
- THE LANTERN's own NIGHT29 floor slow (design.n29.boss1.slowScale 0.55) slows the world while the floor is live: the reel judged
  WorldScale, so the reel has been RED in that room since NIGHT29 whatever slow39 does (headless copy: 4658 leak frames, 4640 the floor;
  the other 18 slow39's 0.25 s ease-out vs the reel's 0.10 s). Fix: the reel judges the row's own share (SurvivalGame.SlowAimShare) and
  reads slow39's scale / eases. n39-r1's f0900 is juice38's hurt static on the lava floor, not slow39's cue.

## E round 2 (merged be71c3a3): span fixed, kiln RED
- n38-span: LIMIT pistolOnly ("PULL ACROSS") on the flight rooms p2 / p4: blast 0, boost 0, the grapple fires (`n38 limits`); gates
  GREEN 3/3 (+31 %), walk 10/10. **span38 ON** (16a1fdbd; the lead's own run GREEN 3/3). VERBS: the STITCHER, THUNDERCLAP and gauntlet
  are set aside inside p2 / p4 only (weapons back at the door).
- The blast + wall-jump tries (K4's move) added: **n38-kiln RED 0/4** (the blast tops 8.46 m, a wall jump off the noride face carries
  the feet to 11.31 m; best try lands 1.83 m onto the ledge). **kiln38 OFF** (16a1fdbd); K4 fixes the geometry (a higher cliff or a
  face leaning > ~17.5 deg).
- For GG (DECIDE39's carried row): with today's ice tuning no gap on flat ice needs skating (a pumped skater reaches the lip at ~17 m/s,
  slower than the dash's 24 m/s): level 2's G1 is a flow beat.

## B1 round 2 (merged): the worm in its new arena
- `boss39 kiln` (n38-kiln's k5, wyrm39-arena): x2.75 gave 26.7 s (RED) -> pool mult x4: p50 43.0 / p90 43.0 s (pool 5620; NIGHT25 was 8580;
  AI unchanged). `boss39 finale` (Stag31): 37.5 s. p39 live in 10/10 on both level rigs: +7.4 s worm, +5.5 s Stag31; in band either way.
  `boss39 all` GREEN 8 rigs. boss39 stays OFF until the CROWN frame (20 gems).

## V round 3 (merged a02bdd7c): the damage static
- Juice39Hurt.cs: a damage-over-time tick (hazard / acid / vent / deadline, within 1.0 s of a burst, or the same source within 0.75 s)
  draws static 0.12 with no shake and raises `hurt38Tick`; a real hit keeps NIGHT38's burst (`hurt38Static`). `n39views hurt` PASS:
  a 3 s lava stand 6 bursts -> 1; a 0.25 s hazard floor 12 -> 0; husk hits 2 s apart 3 -> 3.

## Build n39-r3 (17:03, 0 errors) and its Player runs (17:05-17:12; p39, slow39, boss39, doors38, hud38, juice38, crown38 on in the copy)
- slowworld with slow39: PASS (own share 0.30 = design; 860 riding + aiming frames; 0 slowed outside riding + aiming past the ease-out;
  control 0 of 10 under 1; ease-in 0.13 s mean). With -skygrindOff39: PASS (0.75). The frame f1300 shows the cue: the scene washed blue
  and desaturated, the edges darkened.
- crown37 on n38-spire with boss39 + crown38: GREEN (every tell, 14 passes, 6 windows, the shatter and the kill frames; gems left 0). The
  pass frame shows the ring's flash; the 20 gems are not countable in it.
- verbs37 n38-span with span38's limits: **30/30**.
- V round 4: n39-r1's f1200 door was OPEN (the level36 reel opens every door, SetDoorFree29) so violet was right; a locked doorway was not
  seen on screen tonight (a verbs37 boot's first frames show d1 open cyan): doors38 stays OFF. Doors38View now logs each state change.

## Rows decided (17:14-17:20)
- ON: hud38 (1fa53574), slow39, boss39, p39, crown38 (aa3d0105); already ON: ride39, foundry38, span38, spire38.
- OFF: doors38 (no locked doorway seen), juice38 (no boss entrance frame looked at; the DOT fix not retaken), kiln38 (gates RED, K4
  fixing), iceline38 (G1 a flow beat: GG), finale38 (GG), stag38 (iceline off), ride38 (a5 not started).
- p39's heavier / cannon frames were not looked at: the behaviour is proven (verbs rows 25 / 27, n39reels), the look is not.

## K4 round 2 (merged 72b82f13) and the freeze (17:25-17:41)
- K4: the climbs raised to 12 m (a leaning face lost: the blast and dash jump ride up a slope); gates GREEN 4/4 +34 % (the best non-boost
  try stops 101 % short); walk 10/10 (CASTING FLOOR's panels moved inside the lava). **kiln38 ON** (the lead's gates GREEN 4/4).
- Freeze commit `92eb280a`: golden 0 mismatches 22/22, fingerprint EQUAL plain and --off39, compile clean. Build **n39-r4**: 0 errors.
- **The freeze suite: SHORT.** Claude Code stopped the batch suite for low memory (it started at 6.7 GB free, under the 8 GB rule). Three
  NIGHT38 Players from 13:10-13:13 (n38-r1, `-skygrindMap n38-*`, the n33-check boots) were still running: identified by command line and
  start time and closed with the orphaned batch Unity (8.6 GB free after). Not restarted (the stop notice says not to).
- **Freeze verbs on n39-r4 (the shipped config), 0 NullReference each:** n38-foundry 26/26; n37-iceline 30/30; n38-kiln 29/30 (row 13: the
  reel's 0.02 s press under one frame on the low preset: K4's `n38kiln slidewall` passes it); n38-span 30/30; n38-spire 26/30 (the
  grapple rows 17 / 19 / 22 / 23: no grapple node on the level); n35-finale 30/30. No Player left running after the run.

## Close table (17:45; box 5:00, cap 5:20)
| card | status | measured reason |
|---|---|---|
| G0 premise | DONE (d SHORT) | (a) the campaign boot proven; (b) the ghost weapon fixed (the code path; the harness drive of the clear SHORT); (c) the signal fires 100 %, the cause the 0.75 scale; (d) the Editor's 13:01 NullReferences not reproduced in any compile tonight, cause not found. |
| K4 level 3 | DONE (ON) | n38-kiln: 12 m boost climbs GREEN 4/4 +34 %, walk 10/10, verbs 29/30 (row 13 the reel's frame window); a new worm arena (same half-size); the climb takes three shots + the double (hard for a person: GG to judge). |
| P1 pistol | DONE (ON); the look SHORT | p39: the hold charges, a click fires on the release with the aim correction; verbs rows 25 / 27 PASS in the Player; n39reels 8/8. The heavier feel / cannon / spin not looked at on screen. |
| R1 ride | DONE (ride39 ON); 2 parts SHORT | the chain held at 16 m/s, the jump-off higher; THE GANTRY gates + walk with it on; the no-ride texture built, OFF, not seen; ride38's a5 deaths not started. |
| F1 slow + parry | DONE (ON) | slow39 0.30, seen on screen (f1300), slowworld PASS both ways; the whip latch and the parry window in the harness only. |
| B1 boss length | DONE (ON) | every boss 37-45 s bot (factor 2 -> 1-2 min person), brain levers first; the CROWN fight GREEN in a Player with it on. |
| E2 checker | DONE | the ice judge (G1 RED: no skate-only gap), the two-pull grapple, span's LIMIT fix (3/3), the blast + wall-jump tries (caught kiln). |
| V views + tests | DONE; doors / juice SHORT | C1 rows 8, 9 DONE; the bar bigger (hud38 ON); the verbs reel's wall pick; the DOT static fix; a locked door and the boss entrance never seen on screen. |
| L0 lead | DONE; the suite SHORT | the shared commit, every integration checked (golden / fingerprint / compile / tests), 4 builds (cap 6), 20 reels (cap 20), the freeze verbs on six levels; the suite stopped for low memory. |
| C1-1 level boots | DONE | reel.sh + -skygrindMap + -skygrindReelOnly verbs37 on every level. |
| C1-2 verbs | DONE | the before (n38-r1) and the after (n39-r4) on the six levels, above. |
| C1-3 NIGHT38 view frames | SHORT | hud38 and crown38 ON after frames; doors38 (no locked doorway on screen), juice38 (no entrance frame), stag38 (iceline off) and loom38 not looked at. |
| C1-4 iceline38 | SHORT (GG) | E's ice judge: no skate-only gap on flat ice; G1 a flow beat by default (DECIDE39). |
| C1-5 finale38 | SHORT (GG) | x6 RED (35 cheats), PASS with a what-if pistolOnly; waits on GG. |
| C1-6 ride38 a5 | SHORT | not started (R1's time went to THE GANTRY). |
| C1-7 kiln38 | DONE | replaced by K4's level 3, ON. |
| C1-8 Autopilot alloc | DONE | pre-existing (0 B headless at HEAD, --off38, u3-n37; seen in five earlier suites); the assert now names the step. |
| C1-9 MapChoice timeout | DONE | split per map (50/50 in the Editor). |
| C1-10 Editor NRE / Play mode | SHORT | not reproduced; no unexplained Play mode tonight; cause not found. |
| C1-11 NIGHT38 docs + harvest | DONE | NIGHT38-PROOF, PORT-STATUS (NIGHT38 and NIGHT39 events), FEEL-PLAYTEST (NIGHT39's), the memory harvest of w38 + w39 (7b811bbb). |
| C1-12 the node -e lesson | DONE | in the harvest; the HANDOFF edit tonight went through the Edit tool. K4 used one `node -e` on its own generator (logged). |

## C1 row 11 (14:45-14:50): NIGHT38-PROOF.md and PORT-STATUS's NIGHT38 events written (ca1adeb0). FEEL-PLAYTEST and the memory harvest
at the close.

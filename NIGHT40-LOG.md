# NIGHT40 log (the lead's record; NIGHT40.md is the prompt, untracked)

t0 = Tue 2026-10-13 23:11 (GG set the /goal in the forge session). Box: build to 4:20, round 4 4:20-5:00, test 5:00-5:45, close
5:45-6:00; hard cap 6:00 = 05:11.

## G0 (23:11-)
- (a) Editor: stopped, not playing (heartbeat 06:11Z). Free memory 17.7 GB. **Disk C: 18 GB free** (GG plans to free space later, D-0):
  the night keeps the 10 GB floor; the lead deletes its own reel frames after each verdict. HEAD e323ea3e (the forge commit). GG's four
  modified files untouched.
- (b) **Shared commit `4fa44abe` (23:28)**: N40Blocks (10 bits), design.n40 (14 rows, all off; kiln40 startSet [shotgun], iceline40
  [gauntlet], loom40 dropWeapons [nailgun], cut40 cut [n35-finale]), --off40 / -skygrindOff40, tape 23, SurvivalSim.N40.cs (partials, the
  level rows' start sets / awards / drops through N31UnlocksStep, cached per level: no per-step allocation), Curriculum25.N40Levels,
  Campaign24.waveReady40, the n38 walk pilots answer for n40 ids, harness partials. Golden 39 sets / 22 replays 0 mismatches; fingerprint
  EQUAL plain and --off40; Editor compile 0 errors; N40Shared 3/3, N39Shared 2/2, N38Shared 3/3, N37Shared 3/3, TapeTests 7/7.
- (d) NIGHT40-BRIEF.md written (untracked). **Nine builders launched 23:31** (S1, W1, H1, R2, L2, B2, B4, B5, E3), worktrees from 4fa44abe.
- (c) The before reels on n39-r4 started 23:33 (Tools/night/n40-reels.sh): g0kiln (n38-kiln verbs37), g0loom (loom37), g0crown (crown37).
  The crown37 reel has one pilot (no casual flag in the reel): B5's casual run is the harness's.
- (c) results (23:34-23:40, 0 NRE / 0 exceptions each; C: **128 GB free** by 23:34: GG freed space): g0kiln verbs37 29/30 (row 13 slide
  bounce RED: NIGHT39's known reel frame window); g0loom loom37 GREEN (6 tears incl. 4 pulls, killed 63.6 s, the pilot uses 49 nails);
  g0crown crown37 GREEN (14 passes, 6 windows, killed; the fight is fought with the TORCH). Frames looked at: g0kiln f0010 / f0700 /
  f1400 (no rods in kb1-kb2; the three hud38 cells), g0loom f0400 (the LOOM's pale threads: boss geometry), g0crown f0500 (two thin pale
  poles under a floating platform: a rods40 candidate; no text says how to hurt THE CROWN). Paths sent to S1, B4, B5.
- dn-check-wt.sh tested with a planted error (L-034): the plant's `error CS0103` counted 1. Scratch copy deleted.
- **The permission classifier blocks builders' `git merge port/stage3`** ("Git Destructive", H1 at ~00:00). Not routed around: builders
  skip the merge, the lead cherry-picks (integrate.sh) and resolves conflicts on port/stage3.

## Round 1 integrations
- 00:06 H1 476ce077 (arc40: one 90 deg arc under the reticle, 44 px, 7 px; charge fill eased, cooldown drain, the gauntlet's pips as
  segments and its 15 s line; off = hud38's three cells) + the lead's UiRail34 line dce875e6 (the bend meter follows the grapple row).
  Gates GREEN (golden 0 / 22, fingerprint EQUAL plain and --off40, compile 0 errors), Hud40Tests 6/6. arc40 stays OFF until frames.
- 00:21 W1 1a8f07b0 (charge40: KESTREL FULL 0.9 s, THUNDERCLAP 1.2 s; the THUNDERCLAP's ground hold charges with no volley first, a
  ground click fires on release 58 ms after the press for a 60 ms click (p39's KESTREL: 58), an air press fires on the press step (0 ms
  added, boost vy +9.74); one volley for a 3-step press; n38 gates kiln 4/4 with it on and off). Gates GREEN.
- 00:33 the lead's verbs reel change (rows 27 / 30 hold FULL + 0.25 s with charge40; row 30 wants 0 volleys for a ground hold).
- 00:33 R2 49b66963 / 3e648173 (ride40: every ride holds: 0.000 m lost over 3 s vs a tap ride's 0.547 m in 1.2 s then a fall; ends on
  jump / crouch / away / the wall's end / a landing, still riding at 30 s with no input. ride38's old a5 2 -> 9 does not reproduce on
  n38-foundry (deaths over 5 seeds in a5: 249 / 23 / 15 for off / ride38 / ride40). The attach audit's before: 606 rides on
  levels 1-5, 480 on a face under 2 m or with ground within 1 m). Gates GREEN.
- 00:38 L2 e12fe97b / f720e893 / f5585009 (n40-iceline's beat sheet g1-g8, gr, x9 THE BRAWL; the generator; awards revolver in g6;
  LevelValidate29 reads costs for warden / drone / charger: kept; walk 10/10 to g3; n40ice level: gauntlet only at the start, the
  revolver in g6, the three kinds spawn). B4 f41064f2 / 58939fce / a22c628c (loom40: a camper pulled THE LOOM 43.7 m out of o3 over 160 s
  in 10 seeds; with the clamp 0; the death fall lands; glide 36-40 -> eased 18 m/s; n40-span without the STITCHER, p1 "4.1 HOOK YARD",
  gates 3/3; kill times now 28.9-107.1 s: round 3's band). The lead's Walk29Pilot Ice40 line (4e1bf46e). Gates GREEN.
- 00:42 B2 1dba6f86 (THE ICEBREAKER's brain behind brawl40: the haymaker 1.0 s RED tell + 1.2 s stuck, guns x1 at 4 m -> x0.2 at 12 m,
  the gauntlet full + stagger; band p50 44.6 s 10/10, 0 frames outside; pistol from range x3.8 the mixed time) + the lead's Boss25 hooks.
- 00:43 B5 c9f5a674 / 3919f807 / 36b26582 / 0e1a7920 (THE CROWN's defect list D1-D8 with lines: nothing teaches the rule, the idle ring
  drawn as danger, the ring cutting into the core 5-10 % of steps, the ring 5 m away fills the screen, falls cost 20 hp (skilled 4.7
  falls a fight), 91-140 silent blocked hits a fight; BossLine40 and the entry line; n40-spire with v10's prompt). Gates GREEN.
- 00:45 E3 cc686907 / 6760c918 (a campaign wave spawns all three kinds: no gate; foes40's DRONE: 1.0 s yellow glow, 12 m/s orb, 0.60x
  approach, a parried orb kills it 5/5; **found: with the gauntlet in hand the swing reflected nothing, 0 / 251** (the gauntlet's swing now
  parries with foes40 on). W1 9291552d / bae2295b (pump40: the pump ends 0.42 s after the shot, was 0.75; the pistol's rotations inside
  75 % of the interval: the crosshair quarter turn ends 0.34 s, was 0.60 late; the slug's look). Gates GREEN. Build n40-r1 started 00:47.
- 00:49 **build n40-r1** (bae2295b). Reels 00:51-00:56 with arc40, charge40, pump40, ride40, foes40, loom40, crown40, iceline40 on in the
  build copy (n39-build-rows.cjs): r1kiln verbs37 n38-kiln 29/30 (row 13 as NIGHT39), r1ride verbs37 n38-foundry 26/26 (ride40 on: rows
  9 / 11 PASS), 0 NRE each. **r1ice / r1loom / r1crown booted SURVIVAL**: the n40 ids were not in design.maps.list (the shared commit
  missed it; the campaign loads levels through the list too). Fixed 01:00 (n40-maps-list.cjs); the three reels re-run on n40-r2.
  Reels used 8 / 38 (3 of them wasted on the boot).
- Frames looked at: r1kiln f1300 (THUNDERCLAP, the arc at rest, cyan), f1600 (KESTREL at rest), f1750 (KESTREL CHARGING: the arc fills
  bone). **arc40 iteration:** it reads as one curved bar but is small next to the crosshair: H1 enlarges it ~1.4x with a dark
  under-stroke for pale floors; ON after the next frame.
- 01:03 W1 57a56d52 / dd0bc2bc (pump40 reads -skygrindOff40 at each use: the viewmodel is built before PlayerChecks sets ForceOff; the
  four fullSeconds reels). 01:07 H1 05e30384 (the KESTREL spin on W1's rhythm: a tap's quarter turn + snap ends 0.34 s, the cannon's
  1.82 s; the STITCHER on the arc) Hud40Tests 9/9. 01:10 S1 7b6ebd17 / 438e549d (start40: spawn -> first attack 0.00 s on levels 2 and 4
  before, 5.6-5.8 s with the row; after a retry 0.01-1.01 -> 5.0-6.7 s; kiln40: n40-kiln with the THUNDERCLAP alone, the KESTREL in
  kb2b, the gauntlet in kb3, the boost chain line by line through teach35; gates 4/4, walk 10/10). UiTeach35's kiln40 line end accepted.

## Round 2 integrations (01:18-01:46)
- H1 99f8687e (the arc 1.4x: radius 60 px, 10 px, 104 deg, a 1.5 px dark stroke; the grapple row to -76 px). L2 c36066d4 / d89e0d6e (every
  room g1-gr, x9 on B2's contract; the walk 10/10 to x9's door with iceline40 + foes40 on; g4 fails with foes40 off: iceline40 needs
  foes40) + the lead's SampleCombat line 57a48da3. B5 d37fe8c3 / e47b0f91 / 8e19fd95 / ae3b3244 (the ring out of the core 9.9 % -> 0.0 %,
  8 m, clamped, the anti-hide flies at 30 m/s; falls 20 -> 5 hp; cyan ring, GEMS n / 20, OPEN: SHOOT IT, BLOCKED, the blade decal; band
  skilled p50 39.8 / casual 51.9 s, 10/10). B2 4df07b74 / 7df9d71b / aa5e1cb9 / ece31914 (slam, block, rush, phase 2's chain, the boss
  lines, Brawl40View, the casual pilot; skilled p50 47.9 s, casual p50 52.5 s 10/10, pistol from range x7.2; the fight on n40-iceline x9
  through the campaign hook) + the lead's view / reel lines. B4 f7878a50 / 9ae4153c (the grapple tear, the practice knee, the boss lines,
  no-ride phases 5 -> 10 of 14 walls with a 1 s blink; the pilot 4/10 killed: round 3). E3 f37663e7 / f56c843c (TTK: WARDEN x1.91, CHARGER
  x2.11 PASS, DRONE: the KESTREL kills it in 0.5 s, DECIDE40 D-13; Foes40Combat: the walk bot clears each kind 5/5) + the lead's two
  dispatch lines. R2 74283e0c / dcb03d3d (the attach asks for intent: 606 rides / 480 short or low -> 220 / 0; THE GANTRY walk 10/10;
  every ON level's gates GREEN with the endless ride and the climb move; b2-g1 crossed by one ride, D-14; the near-miss reach stays 0.5 m:
  0.25 broke THE GANTRY 0/3). S1 3f17df2e / ed3399e4 (rods40: 5-14 pieces per level inside the play volume -> 0, mostly pole lamps;
  cut40: 6 slots / 5 playable). Gates GREEN after each.
- Build **n40-r2** (01:30). Reels r2crown GREEN (n40-spire), r2kiln 24/28 (rows 25-27: the KESTREL not owned at n40-kiln's start by GG's
  D-2: the reel now SKIPs them on n40 levels, 09ed78fa), r2ice level36 10/10 rooms entered, r2loom GREEN (n40-span, clamp, killed
  29.4 s); 0 NRE each. Frames looked at: r2crown f0060 (v10's cyan rehearsal ring + "L-SHIFT + SPACE: THROUGH THE RING"), f0700,
  walkin-b (the entry line "FLY THROUGH THE RING: L-SHIFT DASH + SPACE JUMP", GEMS 20 / 20), window ("OPEN: SHOOT IT", GEMS 18 / 20);
  r2kiln f0030 ("JUMP SPACE.", the THUNDERCLAP alone, the bigger arc), f0400 ("In the air, LOOK STRAIGHT DOWN.").
- **Rows ON:** crown40 (fe069909), arc40 + charge40 (the verbs rows 27 / 30 PASS in the Player; the slug's look not yet framed).
  kiln40's iteration: the chain's wording (S1 round 3). Build **n40-r3** 01:46; its reels running.

## Round 3 (01:46-)
- Build **n40-r3** (01:46; every NIGHT40 row on in its copy but arc40 / charge40 / crown40, already on). Reels: r3brawl **RED** (the reel
  began THE ICEBREAKER with walk-in but left the player in g1 "2.1 TANK ALONE" with the KESTREL: pilot swings 0, still 6716 steps; the
  boss line cut at ~52 chars; a SURVIVAL "ROUND 1 IN 88" under the bar; the slam tell frame aimed at the sky) -> B2; r3loom **RED** (the
  grapple tear works in the Player: 4 latches, 0 nails; the pilot falls 4x, not killed) -> B4 round 3; r3ride verbs37 n38-foundry 26/26
  (ride40's attach rules); r3ice verbs37 n40-iceline 20/24 (rows 24 / 40-42: the F punch needs a gun, D-2 -> the reel SKIPs them with the
  reason, fdadc900); r3rods level36 n40-spire 11/11 rooms; LevelSelect29 (cut40 on): five playable levels cycle, 0 exceptions. 0 NRE in
  every run. Reels used 17 / 38.
- Frames looked at: r3brawl arena / line / tell-brawlSlam; r3rods v9-b, v10-b, v11-b (no pole in the play volume); r3ride f0080; r3ice
  f0020 (g1 empty at the start, the gauntlet alone, its 5 pips on the arc), f0400 ("BIG ONES FALL TO F": F is the punch, dead with the
  gauntlet in hand -> L2 round 3 rewrites every room's prompt).
- **Rows ON 02:00:** rods40, cut40, ride40, start40 (1b5eee48). With crown40, arc40, charge40: 7 / 14. S1's kiln40 chain wording
  (341c9be8, "BOOST n/5 · ...") is in the repo, not yet on a frame. Waiting: kiln40 (the wording frame), pump40 (a frame), iceline40 +
  foes40 + brawl40 (THE ICEBREAKER fought in the Player), loom40 (B4's band), models40 (E3).
- 02:05 r3kiln2 (n40-r3 with the repo config copied in, kiln40 + pump40 on): f0030 "BOOST 1/5 · PRESS SPACE TO JUMP" renders (the
  separator too) -> **kiln40 ON** (02:11). Row 9 RED there: the reel picked a 2.5 m-high wall that ride40's intent rule refuses by design
  (a platform face) -> the reel's ride wall must stand 4 m tall with ride40 live (VerbsWall39, 02:10; n39views wall 3/3; L-056).
- 02:11 L2 a913fab4 (every room's prompt names the button that does its job; g5 / g7 panels moved so the decks show from the door;
  prompts 8/8 bound; walk 10/10). E3 ba8efce4 / b0fb1888 / 899ed08c (models40 for all seven kinds; Models40Tests 3/3; lineup40; the walk
  bot clears all of n40-iceline 10/10 with Foes40Combat; the two-at-a-time attack cap SHORT: up to 5 committed, > 2 for 3.5 % of fight
  time) + the lead's lineup40 dispatch d4717648. 02:17 B4 52791c04 / f2a268df / f258101a (band: both pilots 10/10, skilled 49-52 s,
  casual 52-63 s, 0 deaths; the fight's own defects fixed: only the lit knee hookable, the hook bites and lets go, an empty meter always
  opens the window, a forced YELLOW spool that turns 90 deg/s, a tear heals 20; the tell glow + a pose per state; n40-span walk 10/10;
  the floor sweep cue SHORT: no sweep attack exists). 02:19 B2 28972a4b / 15b4c4f3 / fba57297 (the reel's cause: walk-in left the player
  in g1 and the run was SURVIVAL; fixed: a CAMPAIGN run, the player at x9's spawn; the line shortened; phase 2's floes; skilled p50
  48.6 s, casual p50 63.8 s, 10/10). Gates GREEN after each.
- Build **n40-r4a** (02:16): lineup40 GREEN (7/7 placed), level36 n40-iceline 10/10 rooms, 0 NRE. Frames: lineup-12 (every kind's new
  parts); g4-b / g2-a (L2's prompts read). **The blind silhouette test: a fresh reader named 7/7** (the bar 6/7); weak pairs husk /
  spitter and tank / charger -> E3's one iteration. Rows on 8 / 14.

## Round 4 in the Player (02:27-03:00)
- 02:27 E3 3cc74c0b (the SPITTER a dome + pods, the CHARGER low and long with horns; the lineup's part count after the views draw);
  Models40Tests 3/3. 02:32 L2 864def74 (a view-only ice tint in SurvivalBuilder; **not seen in the Player**: v2 f0300 shows the same
  tan floor -> L2 has 12 minutes, else SHORT).
- Build **n40-r4** (864def74, 02:34; 5 / 7 builds) with foes40, iceline40, brawl40, loom40, models40, pump40 on in its copy. Reels
  02:35-02:45 (26 / 38 used), **0 NRE / 0 exceptions in every run**:
  | reel | result |
  |---|---|
  | r4brawl (n40-iceline, brawl40) | beaten at 47.5 s of fight (the reel's judge read Alive during the 1.2 s Dying: fixed, 3684ff54); tells haymaker 1.00 / slam 0.90 / block 0.75 / rush 0.65 s; 4 windows; phase 2 + 6 floes (RED and water frames); 0 frames outside |
  | r4loom (n40-span, loom37) | GREEN: killed 35.9 s by the grapple tear (4 latches, 0 nails), 0 pilot falls |
  | r4lineup (lineup40) | GREEN, 7 / 7 placed, 15 parts drawn |
  | r4crown (n40-spire) | GREEN with every NIGHT40 row on |
  | verbs: L1 n38-foundry / L2 n40-iceline / L3 n40-kiln / L4 n40-span / L5 n40-spire | 26/26 / 20/20 / 24/25 (row 13: NIGHT39's reel frame window) / 30/30 / 26/30 (rows 17 / 19 / 22 / 23: no grapple node on level 5, as NIGHT39) |
  | LevelSelect29 / smoke | 0 exceptions / "[smoke] 203.4s DONE: all steps passed" |
- Frames looked at: r4brawl tell-brawlHaymaker (the jointed brute; the line "GET IN CLOSE: LEFT CLICK THE GAUNTLET HITS FULL" fits),
  floes (phase 2, the red-orange tell glow); r4loom fight-leg2 ("OPEN: SHOOT THE CORE. EVERY GUN COUNTS.", the striped no-ride walls),
  fight-tell-spool ("HOOKED. NOW G PULL AWAY TO A THREAD."); v2 f0300 ("LEFT CLICK: ONE SWING FELLS A TANK"). The second blind
  silhouette test on n40-r4: 7/7 (the CHARGER now high confidence; husk / spitter the weakest pair).
- **Rows ON:** loom40, iceline40, foes40, brawl40 (3684ff54), models40 (6d43b55a): **13 / 14**. **pump40 OFF**: built (the harness: the
  pump ends 0.42 s after the shot, was 0.75), never on a frame tonight (the telemetry carries no trigger to find the volley's frame).
- Freeze candidate 6d43b55a: gates GREEN (golden 0 / 22, fingerprint EQUAL plain and --off40, compile 0 errors). The batch suite started
  03:01 alone (free memory 8.3 GB).

## The freeze and the test hour (03:00-)
- 03:03 L2 aa424094 (the tint was drawn but the warm room light kept ~0.9 / 0.7 / 0.5 of R / G / B: bluer colours). **The freeze:
  aa424094**, gates GREEN. 03:01-03:08 the batch suite at 6d43b55a: **stopped by Claude Code for low memory**, no results written; its
  orphaned batch Unity (pid 151376, `-batchmode -runTests` on SKYGRIND-unity-batch, started 02:59) closed by the lead -> 8.3 GB free.
  Not restarted (the notice).
- Build **n40-r5** (aa424094, 03:10; 6 / 7 builds; the repo's own config, 13 rows on). f5brawl **GREEN** (THE ICEBREAKER beaten at 47.5 s
  of fight, 4 tells, 4 windows, phase 2 + floes, 0 outside); f5v2 verbs37 n40-iceline 20/20; LevelSelect29 0 exceptions; 0 NRE in each.
  Reels used 28 / 38. f5v2 f0300: the floor a cooler, paler grey (no longer tan), not yet "ice".
- 03:15-03:26 the freeze smoke "[smoke] 204.1s DONE: all steps passed"; Editor tests at the freeze: Lesson25 9/10 -> 10/10 (the test
  forces NIGHT40 off like NIGHT30-38, L-027, ba63ddeb), N40Shared 3/3, Hud40 9/9, Models40 3/3, Tape 7/7, Level5 3/3, ZeroAlloc 6/6,
  Autopilot 9/9, MapChoice 54/54. The tree: only GG's four files modified.

## Close table (03:30; box 6:00 = 05:11)
| card | status | measured reason |
|---|---|---|
| G0 premise | DONE | the shared commit at 0:17 (gates GREEN), nine builders at 0:20, before frames on n39-r4, VERBS marked, dn-check proven with a plant; **missed**: the n40 ids in design.maps.list (three r1 reels booted SURVIVAL; fixed 01:00) |
| S1 start + level 3 + sweep | DONE (start40, kiln40, rods40, cut40 ON) | spawn -> first attack 0.00 -> 5.6-5.8 s (levels 2, 4); level 3 the THUNDERCLAP alone, the boost in five numbered lines (framed, iterated once); rods 5-14 per level -> 0 (framed on level 5); LevelSelect29: five playable. Level 6's files stay until GG says delete (D-4) |
| W1 weapons | DONE (charge40 ON); pump40 SHORT (OFF) | FULL 0.9 / 1.2 s; a ground hold charges with no shot first (Player row 30 PASS), the click 58 ms, the air 0 ms; the pistol's rotations inside 75 % (harness); **pump40 never on a frame** (the telemetry has no trigger to find the volley); the slug's look not framed |
| H1 the arc | DONE (arc40 ON) | one arc under the crosshair for every weapon, framed, enlarged once (60 px, a dark stroke); the KESTREL spin on W1's rhythm (harness: 0.34 s), not judged on a frame |
| R2 the ride | DONE (ride40 ON) | 0.000 m lost over 3 s; attaches on short / low faces 480 / 606 -> 0 / 220; every ON level's gates GREEN with the endless ride; Player rows 9 / 11 PASS on levels 1, 3, 4; the reach 0.25 m failed THE GANTRY (kept 0.5); b2-g1 one ride (D-14) |
| L2 level 2 | DONE (iceline40 ON); the ice look SHORT | n40-iceline from a beat sheet, the gauntlet's curriculum g1-g8 + x9, the KESTREL back in g6; walk 10/10 to x9; verbs 20/20 in the Player; prompts framed and rewritten once; the tint drew but the warm light leaves a pale grey floor, not ice |
| B2 the brawler | DONE (brawl40 ON) | THE ICEBREAKER: beaten in the Player at 47.5 s of fight (freeze reel GREEN), 4 tells 0.65-1.00 s, phase 2's floes; bot p50 48.6 s, casual 63.8 s, 10/10; pistol from range x7.2; framed (the brute, the line) |
| B4 THE LOOM | DONE (loom40 ON); the floor sweep SHORT | stays in o3 (43.7 m out -> 0); the grapple tear (Player: 4 latches, 0 nails, killed 35.9 s); no-ride walls 5 -> 10 of 14 by phase, framed; both pilots 10/10 (49-63 s); the STITCHER out of level 4 (n40-span, verbs 30/30); no sweep attack exists for the cue |
| B5 THE CROWN | DONE (crown40 ON) | the entry line, v10's prompt, the cyan ring, GEMS n / 20, OPEN / BLOCKED (framed); the ring out of the core 9.9 % -> 0 %; falls 20 -> 5 hp; skilled 39.8 s / casual 51.9 s, 10/10; GREEN on the freeze build's round-4 copy |
| E3 roster + looks | DONE (foes40, models40 ON); the attack cap SHORT | the DRONE (yellow orb, 1.0 s glow, parry kills it), the WARDEN (punch or flank) and the CHARGER (dodge, its back) in the campaign; TTK: WARDEN x1.91, CHARGER x2.11, the DRONE two answers (D-13); the walk bot clears n40-iceline 10/10; models for seven kinds, blind naming 7/7 twice; at most two attackers at once: SHORT (up to 5 committed, > 2 for 3.5 % of the time) |
| L0 lead | DONE; the suite SHORT | every integration gated (golden 0 / 22, fingerprint EQUAL plain and --off40, compile 0 errors); 6 / 7 builds, 28 / 38 reels, 0 NRE in every Player run; the freeze smoke DONE; 13 / 14 rows ON; **the batch suite stopped by Claude Code for low memory** (no results; targeted Editor tests above instead) |
- (e) docs/VERBS.md: the NIGHT40 section (rows 9, 11, 25 / 27, 28, 29 / 30, 32, 45, 46, 47 and the new counters) written before any row.

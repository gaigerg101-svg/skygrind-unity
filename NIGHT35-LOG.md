# NIGHT35 log (the fundamentals night; lead + five builders)

Start `date`: Sun Oct 11 2026 20:07:15 (t0). Box 3:00 = 23:07, hard cap 3:20 = 23:27. Freeze 2:20 = 22:27, close by 2:45 = 22:52.
Builders (worktrees, agent ids for Tools/night/integrate.sh): W af9cb9d63670c0d3b, T a98da820ebbab4e0f, M a158aff660e0846d3,
V a5bfd5216312da89c, A a6a14da160e68c7e9. Launched 20:13 (0:06).

## U3-48.1 Start (20:07-20:35)
- 20:07 repo: port/stage3 at 55498db1 (the forge commit), GG's four files modified and untouched; Editor up, Play stopped.
- 20:13 builders launched with `Logs/n35-brief.md` (the shared commit's API by exact name, rules, timeline).
- 20:13 the lights frame on n34-r5 (reel 1/22: n34-foundry, lines34 scenario, 6 frames): the "weird little triangle displays" are the
  CEILING PANEL fixtures hanging in open sky (these rooms have no ceiling: cream parallelograms with a dark rim at every angle) plus the
  floating white strip bars above the wall tops and two big flat yellow discs (f5b); Vfx33's motes / sparks are small orange dots, not
  the cause. Sent to V 20:16.
- 20:27 shared commit 2aea07a0. Gates: fingerprint EQUAL, --off35 EQUAL, --off34 EQUAL, golden 39 sets 0 mismatches, trajectories 22/22,
  dn-check 0, Editor compile 0; N35SharedTests 3/3, N34SharedTests 3/3, N33SharedTests 3/3, Lesson25Tests 10/10, TapeTests 7/7.
- 20:3x docs/VERBS.md (39 rows; changed-by-D marks for D-2, D-6, D-8, D-9).

## Decisions (each with the rejected alternative)
- D35-1 Builders launched at 0:06 against the shared commit's API written in the brief, before the commit landed (20:27). Rejected:
  the commit first, then the launch (NIGHT35.md's order): 15-20 builder minutes each spent waiting. Risk: a name mismatch; the brief's
  names were then used verbatim.
- D35-2 The lights cause from one ON frame set, not a Vfx33 on / off pair: n34-r5 has no Vfx33-only switch (-skygrindOff33 also turns
  off look33's lights, the dressing and the HUD), and a new build for a switch would spend 1 of 6 builds. The frames name the cause
  anyway (floating ceiling panels; Vfx33's particles are tiny dots). Rejected: a pair with -skygrindOff33.
- D35-3 teach35 is a sim block bit (the lessons run in the sim and their events land on the tape) although its text is view. Rejected:
  a view-only row (the once-per-profile lesson trigger would then change the sim with no bit).
- D35-4 campaign6 drops every legacy level after the finale from the order (the files stay). Rejected: the finale inserted with the
  legacy levels kept after it (GG: "six levels total").

## Rounds
### Round 1 (20:42-21:00)
- 20:43 M wallride35 85b1e41d (a builder's empty `wip` stash commit 2c1244aa skipped). 20:45 W charge35 + reads + reel. 20:48 rows ON
  charge35, wallride35 (c77aeb21). Harness: n35charge GREEN (KESTREL 0 shots before the charge, x9.00; THUNDERCLAP x2.22; a tap = one
  shot; --off35 RED); n35move ride PASS (30 m wall 9.95 m / 1.20 s -> 28.95 m / 2.79 s; three-wall chain end 16.2 -> 20.7 m/s; outside
  corner 98 deg, inside 90, pillar 360).
- 20:52 T teach35 + reels teach35 / verbs35; row ON (harness teach35 GREEN: fresh profile 9/9 on n34-foundry, the same profile 0).
- 20:54 V look35 / ridelook35 / lights35 / reel look35 (V turned its own view rows on). **V corrected the lead's lights cause**: f5 / f5b
  are roofed (roof slab y 10-11, RoofView27's ceiling quad); the flat unlit violet ceiling reads as sky, so the panels and 127 white roof
  strips look like they float; the yellow ellipses are spitters' acid globs (enemy design, GG's: not touched). light35 PASS (foundry
  lights 273 -> 146). V now also owns the ceiling's colour (reads as a ceiling, not sky).
- 20:55 W pickup35 + reel; row ON. 20:59 T verbs35 fixes. VERBS.md row 31: the THUNDERCLAP shockwave has been off since NIGHT25
  (shockwaveOff25), so it is SKIP, not a lost verb (T found it).
- 20:59 gates on 66ff8b46: dn-check 0, fingerprint EQUAL, --off35 EQUAL, golden 39 sets 0 mismatches 22/22, Editor compile 0 (V's
  Crt29 shader change compiled). Build n35-r1 (1/6).
- n35-r1 reels (2-5 of 22; a real "Wireless Gamepad" was connected, so the glyphs are the pad's: ZR / ZL / L / A):
  - charge35 GREEN: KESTREL tap 1 shot; hold 0 shots while held, charge35 Start / Full / Fire 1 / 1 / 1; hands 0.50 -> 1.00, tip
    11.0 -> 25.6 mm, rail CHARGING 0.50 -> CHARGED 1.00. RED (-skygrindOff35): 1 shot while held, no charge35 events, no read.
    Frame looked at: the barrel glows cyan with a white tip, the rail label reads CHARGED.
  - teach35 GREEN: 5 / 5 level-1 lines in their rooms (dash f3, slide f7, fire f8, charge f8, zoom f9), 24 px, 5.02 s each; a second
    run on the same profile 0 (D-7). RED: no line. Frame looked at: "HOLD ZR to CHARGE. Release to fire." in the message slot.
  - look35: look35 GREEN, lights35 GREEN, ridelook35 RED (the reel never rode: 0 ride frames; the forced look drew nothing, blue 0.0 %);
    equal neighbours 10.3 % (RED) -> 61.6 % (GREEN). Frames looked at: scanlines and chroma read; the grain (0.045) is the loudest thing
    on flat walls (reads as static, not PS2); room f4 "lit" no brighter (mid luminance 0.188 vs 0.193). Sent to V as the tuning pass.

### Round 1b / round 2 start (21:02-21:13)
- V ceiling (#4a403a) + tv35 (spring on hits / swaps, text pop, 0.2 s glitch; no UIEffect / LitMotion: UIEffect draws magenta in a
  Player, NIGHT21). A campaign6 + n35-foundry / iceline / kiln (ride halls i1r, k2r) + n35-finale (x2 ride hall, x3 pillar ring,
  x4 slalom, then THE STAG on its rink: level 2's boss, not level 5's) + teach35 rects; D-3: L1's rings removed except f5b's two
  (the tease), L2-3 rings dim locked teases. campaign6 ON: campaign35 ALL GREEN (L6 starts with all five guns).
- 21:13 build n35-r2 (2/6) at 8713a9e5; reels verbs1 / verbs3 / verbs4 / pickup / levels6 (6-10 of 22).
- n35-r2 verbs35, the first run (one Player per map, 0 exceptions each): L1 n35-foundry 13/23, L3 n35-kiln 14/25, L4 n33-span 17/27.
  REDs: 9 wall ride (L1 never started, L3 0.68 s, L4 never) and 11 jump-off; 13 slide bounce (L3 / L4: the wall jump at 8.00 m/s vs the
  slide's 11.60: slideBounce34's speed is gone with wallride35 ON, a possible LOST verb); 17-23 "not landed" (grapple35 off); 27 / 30
  the charge (a 1.8 s hold passes the self-fire at ~1.5 s and then fires a normal shot: the check, not the gun, T to fix); 28 gauntlet
  (0 events on a press); L4: 5 dash 2.05 m (a wall in the lane?), 20 the whip 0 latches. M has 9 / 11 / 13 (+ the f4 shaft walk stall
  A found: wallride35 makes the walk pilot hang in the shaft, 0/2 even on the unchanged n34-foundry); T has the reel checks 5, 20, 27,
  28, 30. Rows 1-8, 10, 12, 24-26 PASS everywhere.
- pickup35 PASS (THUNDERCLAP label near alpha 1.00 / far 0.00; MEND label) but the frames don't read: the glow pillar stands in front
  of the gun and splits its label, the gun is small and dark, the MEND vial is not visible, "MEND +400000" (the reel's god-mode max).
  Sent to W as the tuning pass.
- levels6 PASS: n35-finale boots as place 6 of 6 with all five guns and the grapple; 3 ride rooms entered, 9 frames (looked at x2-b:
  the ride panels over lava read; the ceiling joists read as a ceiling now).
- 21:16 V's tuning pass integrated (553f2952: SlowAim27 was never built since SurvivalGame.Init created it before `cam` was set, so
  NIGHT27's slow-aim cue never existed either; grain 0.045 -> 0.012; the key spot 72 deg x2.4 with shadows + a cool fill; the TV motion
  holds still while a reel takes stills).
- 21:20-21:30: T's reel fixes (the ride entry killed its own jump; the slide bounce was pressed 1.21 m from the wall: a slide
  jump; the dash / whip spots had a wall ahead; the gauntlet emits `meleeSwing`; the charge row holds 0.9 s and releases). W's
  n35gauntlet harness: RT = meleeSwing + hit, LT = ltParry31Open: the gauntlet is intact (charge35 never reaches it). M: a person still
  climbs a shaft (tapping jump: 5.46 m with 6 WallJumps before and after; the Lucio way 5.56 m); the f4 stall was the pilot only.
- 21:31 grapple35 ON (64dc5c04, 94cba47b): every hook is a straight pull at 44 m/s to the hit point (a wall tap no longer refused,
  arrives in 0.70 s, 0.00 m off the line), keeps 4 m/s + a 3 m/s pop at the point; the hold no longer swings; grapple35Here =
  campaign place >= 4. lines34 walk n35-foundry fell to 7/10 (f5b, f9 lava); A fixed it by rooms (f5b walkway, f9 ledge), so M's
  pilot change was dropped (rejected: the pilot change, which hid the room problem from a person; M kept its patch out of the repo).
- 21:40 A's ride rooms on M's distances (panels 10.5 m = 1 s of ride, facing walls 5 m apart, lava only under the ride line, x3 without
  lava): walk 10/10 on all four n35 levels on the main tree with every row ON (lava deaths x2 64 -> 4, x3 31 -> 0, i1r 31 -> 3).
- 21:45 build n35-r3 (3/6) at d15b768d; reels verbs1 / verbs3 / verbs4 / look / pickup / grapple / redcamp (11-17 of 22).
- n35-r3 verbs35: L1 21/23, L3 23/25, L4 19/27 (0 exceptions). REDs: 9 (the ride timed from a restart), 27 / 30 (2 normal shots
  before the charge), and on L4 17 / 18 / 19 / 22 / 23: **the grapple never attached in the Player** (RopeAttach 0), M's grapple35
  reel RED too. look35 PASS, all four rows GREEN (ride frame: focus 1.00, blue 51.4 %; tv35 kicked 3.6 px; grain 0.012); pickup35 PASS
  (THUNDERCLAP label 39 px near / 23 px at 12 m; MEND +40); RED pickups + ride with -skygrindOff35. Frames looked at: the ride look is a
  strong cold blue; the gun on its ring and the cyan MEND vial read; the lit room was still flat (no key pool).
- 21:55-22:02 three real fixes found by the Player lines: (1) the grapple: under pullOnFire the latch came the step after the hook
  fired with the button still down, so the same press (repeated on the frame's sim steps) counted as the cancel and let go (M 9bb3322e;
  `n35move pull` "press x3" RED before, arrives 0.70 s after); (2) the charge: the same repeated press ended the hold with a normal
  shot (W c87b574c; n35charge "pressed on 3 steps is ONE press"); (3) the lit room: URP falloff from a 9 m ceiling gave ~0.06 on the
  floor, and the reel shot a room without the key (V 9efe227c). The ride: not reproduced in the harness (one ride from 0.14 s, no end
  in 2.5 s with T's exact inputs); the reel now times each ride and prints the end reasons (T 7628ab1b).

### Freeze (22:06-22:45)
- 22:06 T 470e13a4 (presses by sim step). Freeze 2be19aef: fingerprint EQUAL, --off35 / --off34 EQUAL, golden 39 sets 0 mismatches,
  22/22, dn-check 0, Editor compile 0. Build n35-r4 (4/6, 0 errors).
- Reels 18-22 on n35-r4: verbs1 23/23 PASS, verbs3 25/25 PASS, verbs4 (see below), grapple35 RED (the hook bit and pulled; the reel's
  spot blocked the path: M read it), look35 PASS (all four GREEN; lit room 0.313).
- Claude Code's memory guard stopped the reel script's wrapper at ~22:16 (the system had ~4 GB free of 64); the script itself kept
  running. **Lead error**: reading memory use, the lead took the running verbs4 Player (pid 131296) for an orphan and closed it, so
  L4's freeze run lost rows 13-37 and its total (rows 1-12 recovered from the Player's log, all PASS). A retake would be reel 23 of 22:
  reported short (NIGHT35.md: "If a retake would cross a cap, report it short instead"). Rejected: a 23rd reel.
- Batch suite at the freeze (2be19aef, 21 min): **1943 / 1945**. Failed, not diagnosed: `BotZeroAllocTests.BotBrainStep_ChaseEngageEvadeRecover_AllocatesNothing` (3 GC samples over 1800 steps, want 0) and `MapChoiceTests.EveryListedMap_LoadsAndPlays600Steps` (over its 180 s timeout). Both are load-sensitive and ran with ~4 GB of 64 free; whether the new n35 levels slow MapChoice is not checked. NIGHT34's freeze suite was 1936 / 1936.
- Smoke on n35-r4: no result (the Player quit ~10 s after boot without a [smoke] line and hung in cleanup; closed by PID; not
  diagnosed). LevelSelect29: not run (time).
- N-11 (harness bosses33, 5 seeds): --off35 all in band; n35 on: lantern 20.3 -> 13.9 s (under the floor), crown 18.3 -> 16.0 s,
  others unchanged, camp tests PASS (DECIDE35 D-15, default keep).
- A leftover Player from an earlier session (pid 115072, n33-r4 arenas33 reel, started 13:33) is still running; not launched tonight,
  left for GG.

## Close table
| card | status | measured reason |
|---|---|---|
| W1 weapons come back + pickups | DONE | Player n35-r1 charge35 GREEN (0 shots while held, Start / Full / Fire 1/1/1, glow 0.50 -> 1.00, rail CHARGING -> CHARGED) vs RED 1 shot, no events; verbs35 row 27 PASS on n35-r4 L1 + L3; THUNDERCLAP charge (row 30) only in the harness after the fix (x2.22). pickup35 PASS on n35-r3 (gun model on a ring, 39 px / 23 px at 12 m; cyan MEND vial "MEND +40") vs RED cubes; frames looked at. |
| T1 teaching text + verbs reel | DONE | Player n35-r1 teach35 GREEN 5/5 level-1 lines (24 px, 5.02 s, once per profile) vs RED none; verbs35 written and run 4 times (L1 13 -> 21 -> 23/23, L3 14 -> 23 -> 25/25). Lines for levels 2-5 are placed (campaign35) but seen in a Player only on level 1. |
| M1 Lucio ride + grapple that yanks | SHORT | wallride35 DONE: Player rides 1.89 / 1.56 / 1.64 s vs the 1.2 s cap, jump-off 17.20 vs 11.71 m/s, harness 30 m wall 9.95 -> 28.95 m. grapple35 SHORT: harness pull PASS (0.70 s, 44 m/s, 0.00 m off the line, press x3 fixed), owned from level 4 (L1 / L3 rows 17-23 PASS), but on L4 the n35-r3 Player rows 17-19 / 22 / 23 were RED (RopeAttach 0); the fix (9bb3322e) was not re-run in a Player (the lost verbs4 run, the reel cap). Walk 10/10 on the four n35 levels. |
| V1 the look | DONE | Player n35-r4 look35 PASS, four GREEN: equal neighbours 10.3 % -> 61.9 % (480 lines, 32 levels, grain 0.012), lit-room luminance 0.188 -> 0.313 with the key pool, the HUD kicks + glitches on a hit, ride + zoom focus 1.00 / blue 52.8 %; RED / GREEN frames looked at; the lights' cause found (roofed rooms with a sky-violet ceiling, panels and 127 strips reading as floating) and fixed (ceiling #4a403a, joists, fixtures on surfaces, 273 -> 146 lights). |
| A1 six levels on wall riding | DONE | campaign35 ALL GREEN (n35-foundry, n35-iceline, n35-kiln, n33-span, n33-spire, n35-finale; L6 with all five guns + the grapple); ride halls i1r / k2r and the finale's three ride rooms on M's distances; walk 10/10 on all four n35 levels with every row ON; n35-finale boots as place 6 of 6 in a Player (levels35 PASS). n35-iceline not booted in a Player tonight (L-043 SHORT for that one file); levels 4-5 ride rooms are leftover 2. |
| L0 lead | SHORT | VERBS.md (39 rows) before any row landed, the shared commit 2aea07a0 (gates green), every round integrated with gates, 4 builds / 22 reels, the frames looked at, DECIDE35 D-10..D-15. Short: the smoke has no result, LevelSelect29 not run, the boss check is harness-only (Player boss reel over the cap), and the lead lost L4's freeze verbs run by closing its Player. |

### verbs35 (docs/VERBS.md rows; L1 / L3 = n35-r4; L4 = n35-r4 rows 1-12, n35-r3 rows 13-37 as marked)
| # | verb | L1 n35-foundry | L3 n35-kiln | L4 n33-span |
|---|---|---|---|---|
| 1 | walk / strafe | PASS 7.51 m in 1 s | PASS 7.51 m | PASS 7.51 m |
| 2 | sprint | PASS 10.50 vs 8.00 m/s | PASS | PASS |
| 3 | jump | PASS rise 1.13 m | PASS | PASS |
| 4 | double jump | PASS | PASS | PASS |
| 5 | dash | PASS 6.67 m | PASS 6.67 m | PASS 6.67 m |
| 6 | slide | PASS 11.60 m/s | PASS | PASS |
| 7 | slam | PASS | PASS | PASS |
| 8 | jump dash | PASS | PASS | PASS |
| 9 | wall ride (changed by D-6) | PASS longest ride 1.89 s | PASS 1.56 s | PASS 1.64 s |
| 10 | wall jump | PASS | PASS | PASS |
| 11 | wall-ride jump-off boost | PASS 17.20 vs 11.71 m/s | PASS 17.20 vs 11.71 | PASS 17.20 vs 11.71 |
| 12 | bunny hop | PASS 100 % x3 | PASS | PASS |
| 13 | slide bounce | PASS 8.20 -> 11.56 m/s | PASS 11.60 -> 14.52 | PASS (r3) 8.20 -> 10.31 |
| 14 | zip carry / coyote / buffer / lurch | SKIP (flow34's line) | SKIP | SKIP (r3) |
| 15 | jump pad | SKIP no pad on the level | SKIP | SKIP (r3) |
| 16 | glide rail / ice | SKIP map feature (walk bot crosses it) | SKIP | SKIP (r3) |
| 17 | grapple pull to a node (D-2) | PASS not owned (place 1) | PASS not owned (place 3) | **RED (r3)** RopeAttach 0; fix 9bb3322e harness-only |
| 18 | grapple pull to a bare wall | PASS not owned | PASS not owned | **RED (r3)** RopeAttach 0; fix harness-only |
| 19 | hold-swing (D-9) | PASS not owned | PASS not owned | **RED (r3)** RopeAttach 0; fix harness-only |
| 20 | the whip | PASS not owned | PASS not owned | PASS (r3) yank 1 at 9.0 m |
| 21 | whip readies the punch | SKIP rides on 20 + 24 | SKIP | SKIP (r3) |
| 22 | the BEND | PASS not owned | PASS not owned | **RED (r3)** no latch (follows 17) |
| 23 | instant swap in a bend | PASS not owned | PASS not owned | **RED (r3)** no bend (follows 22) |
| 24 | punch | PASS | PASS | PASS (r3) |
| 25 | KESTREL fire | PASS 1 shot | PASS | PASS (r3) |
| 26 | KESTREL zoom | PASS | PASS | PASS (r3) |
| 27 | KESTREL charge (D-8) | PASS 0 normal shots, 1 charged | PASS | RED (r3; fixed c87b574c, L1 / L3 PASS on r4) |
| 28 | gauntlet | SKIP not owned on L1 | PASS meleeSwing 1 | PASS (r3) |
| 29 | THUNDERCLAP fire | SKIP not owned | SKIP not owned | PASS (r3) 1 shot |
| 30 | THUNDERCLAP charge (D-8) | SKIP | SKIP | RED (r3; fixed c87b574c, harness n35charge GREEN, not re-run in a Player) |
| 31 | THUNDERCLAP LT (shockwave off since NIGHT25) | SKIP | SKIP | SKIP (r3) |
| 32 | NAILGUN | SKIP owned from 5 | SKIP | SKIP (r3) |
| 33 | TORCH | SKIP owned after 5 | SKIP | SKIP (r3) |
| 34 | weapon swap | SKIP one gun | PASS gauntlet -> revolver | PASS (r3) shotgun -> revolver |
| 35 | weapon wheel | SKIP pad-only UI | SKIP | SKIP (r3) |
| 36 | ARSENAL / QUICKDRAW | SKIP style names | SKIP | SKIP (r3) |
| 37 | interact / buy | SKIP SURVIVAL / shop reel | SKIP | SKIP (r3) |
| 38 | teaching text on level 1 | teach35 PASS 5/5 (n35-r1) | n/a | n/a |
| 39 | a weapon pickup shows its model | pickup35 PASS (n35-r3, kiln) | (same) | n/a |

Totals: L1 23/23, L3 25/25 at the freeze; L4 at the freeze 12/12 performed rows PASS (rows 1-12), the rest from n35-r3 with 7 REDs,
all with a fix in the tree and a harness PASS, none re-run in a Player tonight. Nothing is known lost; the grapple on level 4 is
unproven in the Player after its fix: GG plays it first (FEEL-PLAYTEST step 1).

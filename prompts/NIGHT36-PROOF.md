# NIGHT36 proof (freeze f53ad261, Player build Builds/n36-r4, 2026-10-12 18:44-18:53)

Every line below was measured on the freeze commit or read from the n36-r4 Player logs (Logs/feel/n36-r4/summary.txt, Logs/player/n36-reel-n36-r4-*.log).
"Not checked" says so.

## Gates at the freeze (f53ad261)
- dn-check 0 errors; Editor compile 0 errors (18:40).
- fingerprint EQUAL; `--off36` EQUAL; `--off35` EQUAL.
- golden: 39 vector sets (48094 records), 0 mismatches; 22 trajectory replays, 22 exact.
- harness: `n36tap` GREEN, `n36core` GREEN, `n36crunch` GREEN, `n36move grip` PASS, `boss36 all` GREEN (band36 p10 20.1 s, p50 20.1 s, p90 20.2 s;
  want p50 15-22 s), `n36move walk --seeds 10` 10/10 (M, 18:33; median 160.2 s; a5 deaths 2), n36-jobs GREEN (L; selftest catches 3 planted errors).
- Batch suite at f53ad261 (18:55-19:10, 894 s): 1955 / 1956 passed. The one failure, DeterminismTests.Core_CallsNoSystemMathTranscendental:
  `Lantern36.cs:381` used System.Math.Asin (B's fan aim). Fixed after the freeze in 026eb43b (JsMath.Asin): DeterminismTests 1/1 in the
  Editor, fingerprint / --off36 EQUAL, golden 0 mismatches 22/22, boss36 all GREEN (p50 20.1 s). The play build n36-r4 is the freeze
  (it still has Math.Asin: a last-bit difference in the fan's pitch only, boss36 on).
- All 14 design.n36 rows ON: tap36, crunch36, tells36, parry36, launch36, heal36, wallgrip36, walkin36, boss36, muzzle36, fist36, text36,
  bossmodel36, level36. FingerprintBaseline.N36OnPath stays false (the composed fingerprint runs with every NIGHT36 block off, as NIGHT35).

## Player reels on n36-r4 (8 invocations; 22 / 22 for the night). 0 NullReference, 0 exceptions in all 8 Player logs.
| reel | result | measured |
|---|---|---|
| level36 | PASS | n36-foundry: 8/8 rooms, 24/24 frames, 4/4 arena rooms locked, spawn bubbles 3/3, red doors 4, own room title 8/8, ride trim panels 4 |
| boss36 | GREEN | the begin moved the feet 0.000 m; tells 3/3 (floor 1.20 s, fan 1.20 s, sweep 1.20 s, camp 1.00 s); 1 parry, 1 crack, 1 stun, 2 punishes; core cyan at the stun |
| tap36 | GREEN | the shot 1 frame after the press; a hold: 1 press shot + 1 charged on release; CHARGING only from 0.35 s; tracer start to the muzzle flash 0.00 cm |
| crunch36 | GREEN | body hit freeze 18.0 ms, stagger 11.9°, twist 8.0°, squash 0.15, hammer 38°; punch freeze 100 ms, stagger 21.9° |
| fist36 | GREEN | lunge 1.93x the old jab, guard weight 1.00, 1 parry pose |
| parry36 | RED | "no spitter placed on the lane (5 m)": the reel's new open-lane setup could not place its spitter in the Player (the r3 run placed it; see verbs36 row 40 for the parry itself) |
| launch36 | RED | "husks not placed": the same setup failure (verbs36 row 41 proves the launch in the same build) |
| heal36 | GREEN | a kill at 2.5 m: +10 (50 -> 61); none at 12 m |
| wallgrip36 | GREEN | tap ride rose +0.02 m, hold +0.05 m; the "rising" case met the wall at vy 0.0 (the reel still cannot reproduce GG's fast entry) |
| text36 | GREEN (r3, unchanged since) | centre 0.620 down, 32 px, the room name keeps the top slot; plate #0B0A1A 68 % (M a55d81d8) |
| verbs36 | 26/27, RED row 40 | see below |
| RED side (-skygrindOff36 on n35-foundry) | as expected | tap36 RED (the shot on release; CHARGING at 0.1 s), crunch36 RED (no body freeze, punch 60 ms), parry36 RED (no tell, no parry), text36 RED (24 px in the top slot), wallgrip36 GREEN: the reel's slow entries do not climb on the old path either (harness: 5.68 m -> 0.05 m) |
| RED boss (-skygrindOff36) | not a valid RED | the boss36 reel builds THE LANTERN on Boss36 itself (hall placed at 2400,0,0 with boss36 False), so it does not show NIGHT35's teleport fight; the walk-in's RED is verbs36 row 43's measure |

## verbs36 on n36-foundry (one Player; docs/VERBS.md rows)
| # | verb | result | measured |
|---|---|---|---|
| 1 | walk / strafe | PASS | 7.51 m in 1 s (top 8.00 m/s) |
| 2 | sprint | PASS | 10.50 m/s vs 8.00 |
| 3 | jump | PASS | Jump 1, rise 1.13 m |
| 4 | double jump | PASS | DoubleJump 1 |
| 5 | dash | PASS | 6.67 m in about 0.3 s |
| 6 | slide | PASS | top 11.60 m/s |
| 7 | slam | PASS | Slam 1 |
| 8 | jump dash | PASS | DashJump + BoostJump 1 |
| 9 | wall ride (changed by wallgrip36) | PASS | 1.48 s ride; rose at most 0.05 m above the attach point (want <= 1.5) |
| 10 | wall jump | PASS | WallJump 1 |
| 11 | wall-ride jump-off boost | PASS | 17.20 vs 11.71 m/s |
| 12 | bunny hop | PASS | 3 hops, 100 % speed kept |
| 13 | slide bounce | PASS | 11.60 -> 14.52 m/s |
| 14 | zip carry / wall coyote / buffer / lurch | SKIP | rides on 12-13 (VERBS.md) |
| 15 | jump pad | SKIP | no jump pad on this level |
| 16 | glide rail / ice glide | SKIP | map feature (VERBS.md) |
| 17-20, 22, 23 | grapple pull node / wall, hold-swing, whip, bend, swap in a bend | PASS | not owned on level 1 (D-2: from level 4), as designed |
| 21 | the whip readies the punch | SKIP | rides on 20 + 24 |
| 24 | punch | PASS | punch 1 |
| 25 | KESTREL fire (changed by tap36) | PASS | 1 shot on a 0.1 s click, 1 sim step after the press frame |
| 26 | KESTREL zoom | PASS | zoom on while LT held |
| 27 | KESTREL charge (changed by tap36) | PASS | 1 normal shot, then 1 charged on release |
| 28-31 | gauntlet blast, THUNDERCLAP fire / charge / LT | SKIP | not owned on level 1 (the unlock rows) |
| 32-37 | NAILGUN, TORCH, swap, wheel, ARSENAL / QUICKDRAW, buy | SKIP | not owned or not performed (VERBS.md) |
| 38 | teaching text on level 1 | PASS (text36 reel) | the line in the lower-middle slot |
| 39 | a weapon pickup shows its model | not checked tonight | (pickup35's reel not run; unchanged since n35-r4) |
| 40 | parry | RED | the parry fired (parry36 1), the glob returned and burst, the spitter DIED, health 50 -> 66; RED only because the dash-refill sub-check could not show: "the dash spent one before the punch: NO" (pips full at the punch) |
| 41 | launch | PASS | launch36Burst 1, 2 kills from one dash-punch |
| 42 | heal by aggression | PASS | +11 at 2.5 m; 0 at 12 m |
| 43 | the boss walk-in | PASS | entered a8 at 0.9 s, the largest one-frame jump of the feet 0.1 m (a Respawn is a jump), feet in the boss room at the end |
| total | | 26/27 | RED rows [40] (the dash-refill sub-check) |

## Frames the lead looked at (n36-r1..r4)
- tap36 press / release: the flash at the barrel on the shot's own frame; the charged beam from the barrel. PASS.
- text36: below the crosshair at 32 px; after M's pass a dark plate with a cyan edge. PASS.
- crunch36: r2 head-on tilt barely read -> F added flash + squash + twist (r3/r4 lines: twist 8°, squash 0.15).
- heal36: the cyan +10 reads.
- launch36 flight (r2): the husk airborne off the punch.
- level 1 (r3, r4): a1 calm with the text plate; a3-b both ride panels with cyan edges, lava between, two husks on the bridge; a4-a the
  lone spitter on its perch under the prompt; a5-b a husk and the panels, the lava only glimpsed (the view is not the best one).
- THE LANTERN (r3 -> r4): r3 small, core unreadable, stun a slump -> r4 1.4x, the core yellow in the open cage on the fan tell; the stun
  frame still does not show the cyan core (the cage tilts, plates open): an open item.
- fist36 (r3 -> r4): r3 a cyan smear (afterimages) -> r4 a solid dark fist with no smear; from behind it still reads as a box (knuckles not
  visible): an open item.

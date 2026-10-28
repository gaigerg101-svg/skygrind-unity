# NIGHT40-PROOF: every number from tonight's runs (2026-10-13 23:11 - 2026-10-14)

Freeze: port/stage3 aa424094, Player build `Builds/n40-r5` (the repo's config: 13 of 14 NIGHT40 rows ON, pump40 OFF). `-skygrindOff40` /
`--off40` = NIGHT39's game. Every gate below was run by the lead on port/stage3; builders' numbers are their harness runs (named).

## Gates (at every integration; the last at aa424094)
- Golden: 39 vector sets (48094 records), 22 replays exact, 0 mismatches.
- Fingerprint: EQUAL to the baseline plain and with `--off40` (FingerprintBaseline.N40OnPath false: the composed fingerprint runs with
  every NIGHT40 block off; no new baseline recorded tonight).
- Editor compile: 0 errors. Tests run in the Editor tonight: N40SharedTests 3/3, N39SharedTests 2/2, N38SharedTests 3/3, N37SharedTests
  3/3, TapeTests 7/7, Hud40Tests 9/9, Models40Tests 3/3 (the freeze's targeted list: see the close).
- **The batch suite: SHORT.** Started 03:01 at 6d43b55a with 8.3 GB free; Claude Code stopped it for low memory (no results written); its
  orphaned batch Unity (pid 151376, identified by its command line and start time) was closed by the lead; not restarted (the notice).

## The Player (reel results; 0 NullReference / 0 exceptions in every run tonight)
| build | run | result |
|---|---|---|
| n39-r4 (before) | verbs37 n38-kiln / loom37 n38-span / crown37 n38-spire | 29/30 / GREEN (49 nails) / GREEN |
| n40-r1 | verbs37 n38-kiln (arc40 + charge40 on) / n38-foundry (ride40 on) | 29/30 / 26/26 |
| n40-r2 | crown37 n40-spire / verbs37 n40-kiln / level36 n40-iceline / loom37 n40-span | GREEN / 24/28 (25-27: the KESTREL not owned at the start, D-2; the reel now SKIPs) / 10/10 rooms / GREEN |
| n40-r3 | brawl40 / loom37 / verbs37 n38-foundry / verbs37 n40-iceline / level36 n40-spire / LevelSelect29 | RED (the reel left the player in g1) / RED (pilot falls) / 26/26 / 20/24 (24, 40-42: the punch needs a gun, D-2; the reel now SKIPs) / 11/11 / 5 playable, 0 exceptions |
| n40-r4a | lineup40 / level36 n40-iceline | GREEN 7/7 / 10/10 |
| n40-r4 | brawl40 / loom37 / lineup40 / crown37 / verbs L1-L5 / LevelSelect29 / smoke | beaten 47.5 s / GREEN 35.9 s / GREEN 15 parts / GREEN / 26/26, 20/20, 24/25, 30/30, 26/30 / 0 exceptions / DONE 203.4 s |
| n40-r5 (freeze) | see the close | |

## Per card (the measures)
- **start40:** spawn -> first enemy able to attack, levels 2 and 4: 0.00 s -> 5.6 / 5.8 s; after a retry 0.01-1.01 -> 5.0-6.7 s (S1, n40start).
- **kiln40:** n40-kiln gates 4/4, walk 10/10; owned: shotgun at 0.02 s, + revolver entering kb2b, + gauntlet entering kb3 (S1).
- **charge40:** FULL KESTREL 108 steps = 0.9 s (was 0.5), THUNDERCLAP 144 = 1.2 s (was 0.7); ground clicks 40-140 ms fire at 42-142 ms (the
  60 ms click at 58 ms, p39's KESTREL 58); air press 0 ms added, boost vy +9.74; a 3-step press = one volley; n38 gates kiln 4/4 on and
  off (W1, n40weapons). The Player: row 27 PASS (n40-r1), row 30 PASS: a 1.5 s ground hold, 0 volleys before the slug (n40-r2, n40-r3).
- **The pistol's rhythm:** the crosshair's quarter turn + snap ends 0.34 s after a tap (was 0.60, past the next shot at 0.483 s); the gun's
  kick ends 0.30 s (was 0.35); the cannon turn 1.82 s of a 2.5 s cooldown (was 2.62) (W1, n40weapons rhythm; H1's spin).
- **pump40 (OFF):** the pump ends 0.42 s after the shot (was 0.75), the kick dies by 0.16 s (was 0.23) (W1); no frame.
- **arc40:** 90 deg / 44 px -> (iteration) 104 deg / 60 px / 10 px + a 1.5 px dark stroke; Hud40Tests 9/9 (H1).
- **ride40:** height lost over 3 s: a tap ride 0.547 m in 1.2 s then a fall -> 0.000 m; a hold ride 0.265 -> 0.000 m; the attach audit over
  levels 1-5: 606 rides / 480 on a face under 2 m or with ground within 1 m -> 220 / 0; THE GANTRY walk 10/10; gates with the endless
  ride + the climb move: n38-foundry 2/2, n38-kiln 4/4, n38-span 3/3, n38-spire 4/4, n40-span 3/3, n40-spire 4/4; b2-g1 now crossed by
  one ride (+156 %, D-14) (R2).
- **rods40:** collider-less pieces in the play volume per level (foundry / iceline / kiln / span / spire): 8 / 14 / 12 / 5 / 11 -> 0 (S1).
- **cut40:** the level select 10 slots / 6 playable -> 6 / 5 (S1); LevelSelect29 in the Player cycles five levels, 0 exceptions.
- **crown40:** the ring inside the core 9.9 % / 5.4 % of steps (skilled / casual) -> 0.0 %; falls cost 20 -> 5 hp, deaths per 10 fights
  9 -> 1 (skilled), 20 -> 6 (casual); the ring 5 -> 8 m; band skilled p50 39.8 s, casual p50 51.9 s, both 10/10 (B5).
- **loom40:** a camping player pulled THE LOOM up to 43.7 m out of o3 (160 s outside over 10 seeds) -> 0; band both pilots 10/10: skilled
  49.2-51.8 s, casual 52.0-62.8 s, 0 deaths; n40-span walk 10/10; no-ride walls 5 -> 10 of 14 by phase (B4).
- **iceline40 + foes40 + brawl40:** the walk to x9's door 10/10 (L2; 10/10 through all rooms with Foes40Combat, E3); the TTK matrix:
  WARDEN gauntlet x1.91, CHARGER back x2.11 (PASS), DRONE: the KESTREL 0.5 s vs the parry 4.3 s (D-13); THE ICEBREAKER: skilled p50
  48.6 s, casual p50 63.8 s, 10/10; the pistol from range x7.2 the mixed fight (B2); in the Player beaten at 47.5 s of fight (n40-r4).
- **models40:** renderers per model within 2x (Models40Tests 3/3); blind silhouette naming 7/7 twice (n40-r4a, n40-r4).

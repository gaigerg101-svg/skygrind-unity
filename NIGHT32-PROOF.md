# NIGHT32 PROOF (the lead; every line from a run this session; Player = a batch Development Player build, reels via Tools/night/n32-reels.sh)

Freeze build **n32-r5** at 2a07c9fd (every NIGHT32 row ON except balance32), plus the freeze fix df98da47 (Curriculum25 level-5 insert; no Player change on the campaign order in use). RED = the same build with `-skygrindOff32` (NIGHT31's game).

## Bosses (GG: "defeat in like a minute", then "bosses still too tanky!")

| boss | NIGHT31 Player (RED, -skygrindOff32 on n32-r5) | NIGHT32 Player n32-r5 (GREEN) | harness 20 seeds p50 |
|---|---|---|---|
| THE LANTERN (level 1) | KILLED 25.1 s | KILLED 30.1 s, 0 deaths | 29.0 s |
| THE STAG (level 2) | KILLED 115.4 s | KILLED 26.6 s, 1 death | 27.8 s |
| THE KILN WYRM (level 3) | NOT killed in 240 s (31 % left) | KILLED 29.2 s, 0 deaths | 28.6 s |
| THE LOOM (level 4) | (no kill reel in NIGHT31) | KILLED 28.0 s, 1 death | 29.9 s |
| THE CROWN (level 5, new) | - | KILLED 39.3 s, 1 death (n32-r4; reel RED only by its strict tell-frame rule) | 28.5 s |

- Hits under 25 % of full damage: NIGHT31 36-94 % of hits (harness) -> NIGHT32 0 % (harness and the bosses32 reel).
- The Stag's open back: frame `Logs/feel/n32-r5/bosses/bosses32/shot-N29-N32-stag-back.png` LOOKED AT: a large glowing back plate and an outlined body (NIGHT31: a thin cyan line on a black box, mid-frame luminance 0.074).
- Camp (real max health, Player n32-r5 camp32): 12 of 12 campers lose on the Lantern, Stag and Wyrm; THE LOOM 2 of 3: the deck-corner shotgun camper died at 7.4 s but dealt 63 % of the bar first (rule: under 40 %): **SHORT**. THE CROWN (harness): 4 of 4 campers die at ~10.5 s.

## The grapple (GG: "you can just spam the grapple")

- Player n32-r5 grapple32 on n30-span, 20 s of tap spam through the real input: GREEN 60 presses -> 6 hooks (0.0 / 0.8 / 4 / 8 / 12 / 16 s); RED (-skygrindOff32) 33 presses -> 33 hooks. Pips seen full / one / empty / refilling (frames in the reel; 1.5x after round 2).
- Harness grapple32 (20 s): level-4 tap hooks 38 -> 6, mean speed 18.9 -> 7.7 m/s; cancel top speed 50.1 -> 25.3 m/s.

## The round ring (GG: "it needs to be a circle")

- Player ring32 PASS (n32-r1 and n32-r5): 64 facets drawn every frame; frames LOOKED AT (n32-r1 shot ring-middle-east, ring-wallride-2): the wall and roof rim curve smoothly. Harness ring32: a full wall-ride lap keeps 3.2-5.6 % less mean speed than the 16 flat panels; each seam 95 % vs 73 % (DECIDE32 D-5).

## Levels

- Level 4 rebuilt (n32-span): Player span32 PASS (9 rooms framed; ring-ride-ring route GREEN on 2 charges); frame s4 LOOKED AT (lava channel, islands, gantry). Harness walk 10/10 seeds; deaths 39 -> 23 after S round 2.
- Level 5 THE SPIRE (n32-spire): Player level5 PASS (9 rooms, four guns at the start, the level select shows 5 playable levels; frames select + v8 LOOKED AT). Harness walk 10/10; crown32walk: enter v9 from v8, THE CROWN dies, the level ends.
- New arenas: Player arenas32 PASS on n32-foundry (f9b, f10), n32-kiln (k2, k2b), n32-iceline (i6, i6b); frame k2 LOOKED AT (a gantry with a spitter, a ramp up).
- Unlocks: harness unlocks31 GREEN for all 9 level files (n30 and n32), n32-spire starts with all four guns (the TORCH comes with the level clear).

## Freeze checks

- Player smoke n32-r4 (every row ON but the final boss tells): `200.0s DONE: all steps passed`.
- LevelSelect29 n32-r4: 20 presses, 20 PASS, 0 FAIL, 0 NRE (5 playable levels).
- DoorFree29 n32-r4: PASS.
- Suite run 1 (batch, 2a07c9fd): 1876 / 1877, the red Lesson25Tests.TheOrder_FollowsTheBlocks (a real bug: LEVEL 5 appended to an order without level 4), fixed in the freeze fix; suite run 2 (batch, df98da47): 1877 / 1877 passed.
- Gates at every integration: fingerprint EQUAL and --off32 EQUAL, Editor compile 0, golden 37/37, trajectories 22/22. Every reel 0 NRE. Frame writes fail in long reels (hundreds per reel; disk C 93 % full): not traced.

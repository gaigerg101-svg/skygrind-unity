# IDEAS-POOL.md (wave 24 forge, 2026-10-08): 26 cards, diverged through the lenses, then scored against GG's words

Score = owner fit (does a GG sentence ask for it: 3 = said twice or "I want", 2 = said once, 1 = implied, 0 = ours) + build fit (fits 6-7 h with proof: 3 easy .. 0 no) - risk (0-3).
Verdict: **IN** (this night), **NEXT** (seeded for NIGHT25, designed here), **PARK** (IDEAS.md). `GG:` quotes are his words (GG-REQUEST.md).

## Pillar A: the linear campaign (his "change of heart": "linear boomer shooter ... focus on the boss fights similar to ULTRAKILL")
| id | idea | lens | GG words | cost | verdict |
| --- | --- | --- | --- | --- | --- |
| A1 | **LAYER** = a level: 3 ROOMS (closed arenas) + 2 traversal links + a BOSS room; the run is a list of rooms in `Design/levels/<id>.json` | genre's best (ULTRAKILL layers) | "linear ... a few arenas" | 1 layer slice | **IN** (2.5 h block) |
| A2 | **ROOM** = a box-brush room, doors that LOCK until its encounter script clears, spawn nodes, an encounter script (wave list of enemy kinds, a count, a timer-free clear), checkpoint at the door | genre (Doom Eternal arenas) | "closed rooms type movement" | core of A1 | **IN** |
| A3 | **WEAPON EARNING**: start KESTREL (pistol) + gauntlet; the THUNDERCLAP is awarded at the end of room 2 (a pedestal pickup, a name card); the STITCHER (nailgun) at the end of layer 2 (NEXT) | genre | "start out with a pistol ... earn a shotgun" | small | **IN** |
| A4 | **LIMIT ROOMS**: a room flag strips weapons or a movement verb for that room only (PISTOL ONLY, NO DASH, NO SLIDE, NO JUMP, MELEE ONLY), announced by a banner, restored at the door; clearing it pays a style bonus | genre (ULTRAKILL challenge levels, Titanfall stripped sections) | "limited weapons ... lose weapons or lose movement" | small after A2 | **IN** (2 flags: PISTOL ONLY, NO DASH; the others NEXT) |
| A5 | **V2 SPIKE**: the layer's room 3 before the boss is a hard mirror fight (RIVAL PRIME: the web had one, `PortExport/maps/spire-duel-navgrid.json` is its nav graph), ULTRAKILL's V2 difficulty spike | "feel like V2 where it is a difficulty spike" | 2 | big (a new enemy brain) | **NEXT** (designed in DESIGN24-bosses.md) |
| A6 | Arcade modes stay: SURVIVAL and THE GRIND on TITLE's MODE row beside CAMPAIGN; the DAILY and WEEKLY go | his reversal | "let's keep these game modes" + "remove the daily and weekly" | removal | **IN** |
| A7 | Level select + per-level rank (existing grade / style meter) + par time; a per-LEVEL ghost (existing ghost tech, not the DAILY's) | genre (Neon White, ULTRAKILL ranks) | "mastery of the movement" | medium | **NEXT** (rank per layer **IN**: reuse the stamp) |
| A8 | Closed-room lighting kit: one accent colour per room family (cyan safe, red-orange danger only on enemies / hazards), guide light toward the exit door, dusk skybox only outdoors; emissive trim | lens: what a player shows a friend | "better lighting ... what's actually shown" | medium | **IN** (kit + 2 looks; art pass later) |
| A9 | Traversal links: a jump-pad hall, a grapple-node shaft, a slide tunnel, a wall-kick shaft (the Mover has wall jumps; NO new Mover verbs: rule 3) | genre | "a little bit of platforming or wall riding" | medium | **IN** (3 link kits) |
| A10 | Level authoring pipeline: a JSON room list + a code builder that generates collision (ThreeOctree boxes, like `Build_drydock.cs`) and the render boxes; a validator; ONE command builds the layer | tech lens | "study 3D arena building" | the risky premise | **IN** (spike S5) |

## Pillar B: bosses ("a crazy worm boss", "chasing the boss", three bosses, two made)
| id | idea | lens | GG words | cost | verdict |
| --- | --- | --- | --- | --- | --- |
| B1 | **WORM** ("THE KILN WYRM": a 40-segment boss that slithers FAST on a spline, burrows through walls and floor, coils the arena; head + segment weak points; phases) | song-as-level / Terraria's Eater of Worlds / The Destroyer in 3D | "crazy worm boss ... slithers really fast" x2 | 1.5-2 h | **IN** (boss 3) |
| B2 | **HIGHWAY CHASE** ("THE OUTRUNNER": an endless road; you chase the boss; hazards, gaps, car roofs, jump pads; distance meter; not left-right only) | other-genres lens (Sonic chase, Burnout, Crash boulder) | "endless highway ... you're chasing the boss ... Sonic" | big (scrolling world) | **NEXT** (NIGHT25: full design in DESIGN24-bosses.md) |
| B3 | **MONOLITH** port: the web's first boss (12 m stacked pillars, core weak point, 6000 HP, 3 phases; `skygrind/src/enemies/boss.js`, 695 lines): "the one already made" | what exists | "I've already made two" | 1.5 h | **NEXT** (default for "two made" is BULWARK + MONOLITH; BULWARK is built and off: turn it on in the layer) |
| B4 | HIVE and ORBITER ports (web bosses 2 and 3) for layers 2 and 3 | what exists | "the old game had plenty of bosses" | 1.5 h each | **NEXT** |
| B5 | Boss kit: name card slam, health bar with 3 segments, phase beats, finisher slow-mo (BossKit.cs exists) shared by all | what exists | | free | **IN** |
| B6 | A boss that TEACHES the move (the worm punishes standing still; the highway demands the slide; the V2 demands the dash) | genre lens | "mastery of the movement" | design rule | **IN** (a rule for every boss) |

## Pillar C: the first-list fixes and builds (each a switch, each a contract)
| id | idea | lens | GG words | cost | verdict |
| --- | --- | --- | --- | --- | --- |
| C1 | **CHARGE FIST**: the PULSE GAUNTLET charges by HITS and KILLS only (never time); 3 levels; a punch with charge FLICKS the player through the air in the aim direction; small levels are a small hop, FULL (about 25 kills) is the big launch; spent on use | his idea | "charge it up by hitting enemies ... flick air movement boost ... balanced unless fully charged" | medium | **IN** |
| C2 | **RETICLE ARC**: a thin curved bar under the crosshair that fills as the held ability recharges, plus dash pips; not in the way (small, 40 % alpha at rest, flashes when ready) | his idea | "under your reticle ... curved bar ... pill that shows dashes" | small | **IN** (replaces the ARCLINE chip's location) |
| C3 | **BIG PICK**: PICK A MOD becomes rare: every 5 rounds in SURVIVAL (and at each layer end in the CAMPAIGN); 3 BIG cards from one pool: FREE MYSTERY BOX SPIN, +1000 POINTS, WEAPON PACK (the held gun gets a tier), a PERK, FULL HEAL + ARMOR, FULL FIST CHARGE; no reroll, no small percentages | his idea | "sparing ... every five or 10 rounds ... big buck ... free mystery box spin or 1000 points" | small | **IN** |
| C4 | **Y SWITCHES WEAPONS**: pad Y = NEXT WEAPON (tap) as the v2 default had it; RB keeps SPECIAL + ARCLINE; the layout migrates with a test | his ask | "the Y ... should also be able to switch weapons" | small | **IN** |
| C5 | **SHOTGUN RATE**: the THUNDERCLAP "fires a little too fast": measure the real interval on a Player (web `fireInterval` 0.9 s, `pumpDelay` 0.45) and any Unity-side scale (stamina / perks / MOVE flags), then set it from the research table, behind `design.weapons.shotgun.*` | his ask | "shotgun is not balanced it just fires a little too fast" | tiny | **IN** |
| C6 | **REMOVE the INSTANT REPLAY** (view, prompt, ring, config row, tests, the smoke / reel scenarios) | his ask | "remove the replay feature" | removal | **IN** |
| C7 | **REMOVE the DAILY and the WEEKLY** (modes, TITLE rows, `Daily.cs`, `Weekly20.cs`, RESULTS lines, the DAILY ghost, the TITLE CODE field's daily link; keep a seed-code RETRY SAME SEED) | his ask | "remove the daily and weekly day mode" | removal, wide blast radius | **IN** |
| C8 | **SWITCH LAG**: find the hitch when switching screens / modes / weapons with a measure (Profiler markers, frame time per switch, the Player log), then fix the top cause (prewarm TMP, canvases, shaders, first-use loads) | his report | "so laggy switching between" | investigation | **IN** (P1) |

## Pillar D: beyond his ideas (smarter, bigger picture)
| id | idea | lens | why | cost | verdict |
| --- | --- | --- | --- | --- | --- |
| D1 | **RETRY ROOM**: death restarts the ROOM in under 1.5 s with the same seed (a boomer shooter lives on fast retries); the DOWN screen is one button | genre | the whole loop | small | **IN** |
| D2 | **SKULL-STYLE OPTIONAL CHALLENGES** per room (no damage, finish in par time, kill a mini-boss with the fist): pay rank, no gating | genre (ULTRAKILL secrets/challenges) | replaces the DAILY's replay value | small after A2 | **NEXT** |
| D3 | **BOSS RUSH** from the layer's three bosses (the web had one: "all five bosses down in 47.6 s" bot run) | what exists | cheap replay value | small | **NEXT** |
| D4 | **Per-level ghost** and Steam leaderboard keys per level (replaces the DAILY's leaderboards plan, D-06) | Steam | Stage U6 | medium | **NEXT** |
| D5 | **Arena director**: encounter scripts reuse Director20's squad logic with explicit room-local spawn nodes; enemy budget per room tuned by a headless bot run ("balance is measurable headlessly", L-012) | lessons | proof | medium | **IN** (bot clears room 1-3: a number) |
| D6 | **Scripted camera beats** for boss intros (the pull-back on the worm rising, the road opening) using the existing slow-mo / FOV tools | polish lens | feel | small | **PARK** (art and polish come later: his words) |
| D7 | **Wall-run / rail grind** as new Mover verbs | NIGHT22 out-of-scope | rule 3 (movement stays exact) and GG says "maybe a little bit of platforming or wall riding": use the EXISTING wall jump / kick / slide / grapple only | big | **PARK** |

Picks: A1-A4, A6, A8-A10, B1, B5, B6, C1-C8, D1, D5 = **IN**; B2, B3, B4, A5, A7, D2-D4 = designed and seeded for NIGHT25-26; one moonshot behind a switch: **B2 (the highway) as a prototype stub** only if the night finishes early (see NIGHT24.md leftovers).

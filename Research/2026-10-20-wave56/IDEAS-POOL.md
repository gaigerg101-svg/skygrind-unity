# NIGHT56 idea pool (wave-forge S2, 2026-10-20 23:1x; the lenses in ~/.claude/skills/wave-forge/lenses.md)

GG's ask (verbatim): "what should overnight prmpt focus on to imporve levels specifically, gameplay, eniemies, colors, ect".
Our data first: NIGHT55's three critics (Research/2026-10-20-wave55/critic-*.md), Logs/n55/OPEN.md (91 lines), the slate's 25 SHORT
entries, and the builders' unpicked round-2 commits (HANDOFF NIGHT55 block). GG owns enemy LOOKS (CLAUDE.md rule 7): enemy cards here
are behaviour, encounters and lighting only.

Score: owner fit x3, player impact x3, fits the game x2, feasible in the box x2, provable x1, novelty x1 (/60).

| id | lens | card (the player's words) | owner hook | build / files | proof | score |
|---|---|---|---|---|---|---|
| D-0 | debt | NIGHT55's built work reaches the game: grade55 r2, wick55, step55 / ssao55 / decor55, kill55 r2, brief55 teach text, clear55, doorDark | NIGHT55 "finish and continue anything not finished" | cherry-pick the 5 branches (HANDOFF order), gates, n56-r1, gate critics | each row's a1xx proof GREEN + a fresh critic PASS | 55 |
| L-1 | owner fantasy | "the room fight starts in my face: I walk in and 4 enemies are already coming, the next wave lands before I finish this one" | GG w35 "random rooms", veteran #4 (1-4 of 7-14 visible) | sim row `wave56`: spawn at loop nodes the entry sees, next wave when 2 remain | a452 entry frame >= 4 visible, >= 2 within 15 m; flow table stall <= 15 % | 52 |
| L-2 | debt | the seven rooms that still fail identity (a3, g3, g4, g8, v2, kb2, p5) get pieces with jobs, not cuts | GG "random pillars that serve no purpose"; T-127..T-131 | level data in place; n53-identity.py; padspot | identity FAIL rooms 7 -> <= 2; each added piece used by the bot | 50 |
| L-3 | genre best | one SHOWCASE room per level rebuilt from scratch to a researched recipe (a loop arena with height tiers and a pad cut-through) | GG 2026-10-09 "complete arena overhaul ... from scratch", "ULTRAKILL rooms" | a new room in each plan from levels.md's recipes | contract 14/14 + identity PASS + a fresh GG-persona review "would satisfy GG" | 46 |
| L-4 | debt | doors and links are lit; ceiling lights hang from fixtures | ggproxy #6, #9; GG w32 / w37 | doorDark r2, fixtures (decor row) | 0 flat frames in verbs37 x5; 0 floating quads | 42 |
| L-5 | first-time | the surface I ride reads as usable: THE WICK's climb panels and every ride wall carry the cyan ride cue; nothing decorative is danger red | firsttime #2; rule 8 | view row on ride walls + THE WICK's stripes | ride walls: cyan cue on 100 % of ride faces in a452 frames; red-orange off-tell <= 2 % | 45 |
| G-1 | other genre | "my style rank fills, and A-rank kills heal more and refund more boost; the rank-up punches the screen" | veteran #7; ggproxy #4 (DULL in 2772 / 2772 frames); OWNER w41 "feel aweosme to combo" | view + sim row `rank56`: rank scales heal36 / ride-kill refund; rank-up beat | a pilot reaches A in 20 s; >= 30 % of combat at B+; golden EQUAL off | 50 |
| G-2 | debt | kills crunch: the hold grows in a chain, shards stay, enemies crack when hit | GG "kills not crunchy" (w16) | kill55 r2 (K's branch) | a355 GREEN + blind pair | 49 |
| G-3 | genre best | the camera shows my speed (FOV with speed), no edge washes | veteran #9, ggproxy #10 | fov55 (C) | FOV-speed corr >= 0.9; edges luma <= 0.05 | 40 |
| G-4 | debt | THE WICK is a punish-the-window fight | veteran #1 | wick55 (B's branch) | n47human window share >= 50 %, a155 FIGHT MODE punch inside a window | 47 |
| G-5 | first-time | every boss says what it wants once, in one place, with my button | firsttime #1, #7, #8; GG w35 | bossText55 + brief55 | first 3 s: 0 px text on the boss rect; one verb line per boss | 46 |
| E-1 | genre best | enemies come from where I can see them, with a tell, and they climb to me | E-6, GG NIGHT20 "a route enemies can climb" | spawn tells at loop nodes (with L-1) | spawns in view 80 %+; climb route used per room | 44 |
| E-2 | genre best | at most two enemies wind up at me at once, but they never stand still | DOOM tokens; tokens41 exists | tune tokens41 + an "idle enemies advance" rule (sim row) | idle-in-place time per enemy <= 20 % (bot) | 38 |
| E-3 | colours x enemies | enemies are the warmest, brightest thing in every frame (a warm rim light) | E-3; ggproxy #10 "red jelly beans" | view: rim / emissive band (no model change) | enemy rect S and V above the room's p90 | 40 |
| E-4 | debt | THE WYRM's fights end in 1-3 minutes | GG 2026-10-13 | wyrm55 (not built) | n47human p90 <= 180 s | 42 |
| C-1 | owner fantasy | each level has ONE colour you remember (the foundry is brass and soot, the ice line cobalt and white) | GG NIGHT54 "lack strong color direction", NIGHT49 "way too bland" | decor55 r2 + colors.md scripts | accent 3-8 % of wall pixels; blind pair "has a colour" 2 / 2 | 48 |
| C-2 | debt | THE CROWN and THE WYRM palettes right (yellow-red-orange worm) | GG NIGHT47 worm theme | grade55 r2 + wyrmTurn | a155 GREEN both | 41 |
| C-3 | first-time | yellow means parry and nothing else | firsttime #4 | audit + recolour non-parry yellow (gun plates, gems, floor trims) | yellow pixels outside parry tells <= 0.5 % per frame | 40 |
| M-1 | moonshot | "THE DUSK CLOCK": the sun sinks as you clear rooms; the last room is neon dusk | IDEAS C-14 (53/60) | view row, sky / fog per room cleared | per-room sky luminance falls monotonically | 36 |
| X-1 | constraint "no new UI" | the room-clear beat uses only the world: lights come on in the next room when you clear | ggproxy #9 | clear55 alt | next room lit on clear frame | 34 |
| X-2 | tech | per-room route reels by a pilot that rides and takes pads (T-118) | T-118 | a reel scenario | each showcase room has a ride-route frame strip | 33 |

**S5 spike (L-1, 23:1x):** the wave overlap already exists and is ON: design.n46.overlap46 (sim, `bodiesLeft` 2: the next wave releases
while <= 2 bodies stand; Campaign24.cs:852-866 `waveRelease46`, SurvivalSim.N46.Flow.cs:134). So the veteran's "1-4 of 7-14 visible" is a
SPAWN PLACEMENT problem: each wave spawns at `r.WaveNodes43(w)` / `waveNodes33` / `r.nodes` (Campaign24.cs:913), i.e. level data per room.
L-1 becomes: re-place each combat room's wave nodes onto the loop within the entry's view (<= 15 m for >= 2 of them), a level-data edit
with a checker, no new sim row. Cheaper and safer than planned (no fingerprint risk).

Runners-up to docs/memory/IDEAS.md at the close: M-1 (moonshot slot taken by L-3), X-1, X-2, E-2.

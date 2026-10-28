# Wave 34 research index (forge 2026-10-11; four web-only subagents, about 4-6 minutes each)

Files (each claim carries its URL there; claims taken only from a search summary are marked so in the file):
- research/slowmo.md: player-triggered slow motion in Superhot, Max Payne, F.E.A.R., Ghostrunner, Severed Steel, Katana Zero, Sifu,
  Doom Eternal; asymmetric time; overlays; a fixed-step implementation; spam risks.
- research/swap-hud.md: ULTRAKILL's weapon HUD and swap logic, Doom Eternal's quick switch and wheel, pad conventions, melee hitboxes.
- research/movement-oss.md: open-source movement code with licences, numbers, the ranked top 8 patterns.
- research/arenas.md: ULTRAKILL, Doom, Neon White, Titanfall arena rules; readability; the ten rules.
- SPIKE.md: the bend's code path in SKYGRIND (read with file:line).

## What the research changed in the plan
1. **The world slows, the player does not** (slowmo.md section 1: F.E.A.R. states it, Max Payne does it). So the bend's target is the
   split path, with the global path first as the safe slice (SPIKE.md).
2. **Pay for slow motion with aggression, not time** (slowmo.md section 6: kill-refilled meters fixed spamming in Max Payne 2 and Severed
   Steel; Doom Eternal's free wheel slowdown became an exploit). Matches GG's long-cooldown rule (w19).
3. **No game's time-scale value was found** (slowmo.md section 3). 0.25 is a starting choice, to be judged on frames; logged as such.
4. **ULTRAKILL does not show the full arsenal on its HUD** (swap-hud.md section 1: the current weapon's icon in its variant colour; all
   weapons only in a held wheel that slows time). GG's rail is therefore a new design: ULTRAKILL's coloured current icon plus an always-on
   strip. Per-weapon cooldowns that tick while holstered are what make swap-cancelling work (swap-hud.md section 2).
5. **Arena metrics are not published** (arenas.md section 4): spacings must come from SKYGRIND's own measured jump / dash / ride / grapple
   distances. Readability means low detail and one colour per meaning (ULTRAKILL devs via 80.lv, Neon White via Game Developer).
6. **Copyable movement code exists only for generic patterns** (movement-oss.md: Celeste Player.cs MIT, UnitySourceMovement MIT,
   dot-player-controller, Godot-Quake-wallrun Apache-2.0); nothing licensed reproduces Titanfall or ULTRAKILL. SKYGRIND already has
   coyote 0.1 s, buffer 0.1 s, wall ride, wall jump, slide-jump carry (PortExport/config.json movement): the builder greps each pattern.
7. **Melee reach:** inflate the enemy's hurtbox, keep the player's small, test with spheres (swap-hud.md section 6; no ULTRAKILL numbers).

## Table B: the bend's starting numbers (design.n34.bend34 / bendLook)
| number | start | source |
|---|---|---|
| world share `scale` | 0.25 | not found in any game (slowmo.md 3); the forge's choice, judged by frames f1-f4 |
| ease in `inS` / out `outS` | 0.08 s / 0.3 s | slowmo.md 9 (0.3 s ramp out); 2024 paper: 0.2-0.4 s slow motion reads heavy (slowmo.md 2) |
| length `holdS`, `killExtendS`, `maxS` | 2.5 s, +0.5 s per kill, 5 s | slowmo.md 9 (3 s, +0.5 per kill, cap 6 s), shortened for small rooms |
| short bend, every latch | 1.0 s, free (the grapple's charges limit it) | the red team's finding 13: GG's "the whole point" is real-time swaps on the grapple; a meter-only bend leaves most grapples plain |
| meter refill (full bend) | kills only, 0.25 a kill, x2 at rank S+; starts full; about 15 s of fighting from empty | slowmo.md 1, 6 (Max Payne 2, Severed Steel); 0.25 and x2 the forge's choice |
| entry | a FOV thump, no freeze by default | ULTRAKILL's 0.25 s parry freeze (slowmo.md 2) is for parries; a freeze on every grapple breaks flow |
| overlay | world desaturated, enemies red-orange, usable cyan, a step deeper per kill | slowmo.md 9; Max Payne 2's deepening read (slowmo.md 5); CLAUDE.md rule 8 palette |
| swaps | instant inside a bend; cooldowns tick holstered | swap-hud.md 2, 9 |

## Table A: arena rules (research/arenas.md's ten, applied)
1 one-look readability; 2 two or three floors in a small room; 3 movement props between floors plus a climbable enemy route to every
floor; 4 rewards on a different floor from the threat; 5 an air-time multiplier; 6 weapon freshness; 7 an obvious route that scores
badly; 8 one colour per meaning; 9 enemies as route markers; 10 special movement surfaces in a few spots only.

## Table M: movement patterns, ranked (research/movement-oss.md)
1 buffer and coyote on every surface (walls, ledges, slides); 2 no ground friction on a buffered landing jump; 3 a clamped slide boost
with a re-arm wait; 4 wall-ride gravity in phases (zero, then a ramp); 5 a capped up-plus-away wall jump; 6 one air steering lurch shortly
after a jump; 7 a grapple that adds to velocity on release; 8 corner correction, hang at the jump peak.

## Not found / not checked (said once, here)
Any game's slow-motion time scale and ramps; Doom Eternal's wheel slowdown factor; swap and draw times in seconds; ULTRAKILL melee
hitbox sizes; Titanfall wall-run spacing; Quake 3 loop metrics; GDC movement talks for Titanfall 2 / Doom Eternal. REA (the Reverse
Engineer Anything skill) was not used: its `rea` MCP server timed out on connect in the forge session.

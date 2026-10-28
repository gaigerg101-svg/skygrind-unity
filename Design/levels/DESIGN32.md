# DESIGN32: bosses in a minute, a grapple with charges, a round ring, level 4 again, level 5

GG, 2026-10-10 (Research/2026-10-10-wave32/GG-REQUEST.md): "the bosses have way too much health ... you can barely hit them ... you
should defeat [them] in like a minute ... you can just spam the grapple ... the arenas are so boring ... the 5th level ... the 4th level
completely overhauled again ... new arenas on existing levels ... the first boss fight arena ... needs to be a circle."

Evidence: Research/2026-10-10-wave32/A1-boss-audit.md (every boss's damage gates with file:line), A2-level-pipeline.md (how levels are
made, why the ring is 16-sided), M1 (methods: target bands, seeded sweeps, degenerate bots, look at frames), M2 (boss and grapple
numbers from shipped games), M3 (arena patterns, ten arena sketches), REVIEW-frames.md (the lead's look at the NIGHT31 Player frames).

## Rules for every card (from M1 / M2; GG's words decide over them)

1. **A target band first, then the number.** Each card names the band and the measurement that proves it (a harness sweep table over
   seeds: p10 / p50 / p90; and a Player reel line). One variable per commit where a number is tuned; the table goes in the commit message.
2. **Degenerate bots.** Every boss gets a camper (still, corner, high spot, far side) and every movement verb gets a spammer. A
   degenerate strategy that wins or is safe is a RED line.
3. **Look at it.** Every new or rebuilt room gets a top-down render (`node Tools/night/level-svg.cjs <plan> <out.svg>`, then
   `"/c/Program Files/Google/Chrome/Application/chrome.exe" --headless --disable-gpu --screenshot=<out.png> --window-size=1600,1200 <out.svg>`)
   that the builder opens and judges against: a loop (no dead end), two or more heights, a hazard edge, an enemy climb route, and at most
   24 m across for a combat room. The lead adds eye-height Player frames.
4. **Overrides, never edits of old rows.** A NIGHT32 number lives under its `design.n32.<row>` and is read only while that row is on, so
   `--off32` stays NIGHT31's game bit for bit (fingerprint EQUAL and `--off32` EQUAL at every commit).

## B: the four bosses die in about a minute (design.n32.bosses32; view row bossMarks)

**Band:** the Player pilot kills each boss in 45-75 s with the weapons its level has (level 1 KESTREL; 2 + PULSE; 3 + THUNDERCLAP; 4 +
NAILGUN); a seeded harness sweep (at least 20 seeds) has p50 in 45-75 s and p90 at most 100 s; a "pistol + fist only" bot still kills
each in at most 150 s (no hard lock on one weapon).

**Damage rules (all four):**
- Every hit from every weapon in every state deals at least 25 % of its full damage (A1: guns did 1-6 % outside the windows).
- No state where the boss takes zero damage for more than 1.5 s (a phase change only). No position the player can stand on where the
  boss cannot be damaged (A1: Lantern platforms 0 %, Loom knees out of reach, Wyrm buried pieces) - GG's "sometimes they can't even take
  damage in areas".
- The identity mechanic stays, as a bonus: an open window deals x2.5 and lasts at least 3 s and at least the gauntlet's cooldown + 0.75 s
  (A1: the Stag's and Wyrm's windows were shorter than the 2.5 s swing cooldown: one punch per window).
- Hurtboxes at least 1.3x the drawn model; the weak spot at least 2x the size it has now if it is under 1 m.
- HP from `HP = target 60 s x sustained DPS of the level's kit x 0.6 hit rate x 0.75 uptime`, then tuned on the sweep.

**Starting numbers (A1 section 5; each as an override under n32.bosses32, never an edit of the n27 / n29 / n30 / n31 key):**
- LANTERN: pool multiplier 0.285 -> 0.6, chip 0.06 -> 0.25 (and its extra x0.6 cut -> 1.0), floor share 0.5 -> 1.0, platform share 0 ->
  0.25, core open 4 -> 6 s, wall-ride grace 0.6 -> 1.2 s, finisher gun share 0.4 -> 1.0, and the 66 % / 33 % clamps must not eat a hit.
- STAG: pool 1800 -> 1300, gun share 0.05 -> 0.25, back window -> 3 s, crystal regrow 15 -> 8 s; find why the island-top and far campers
  took 11 shard hits and lost 0 % (NIGHT31-PROOF) and make both spots lose.
- WYRM: pool multiplier 0.7 -> 0.25, gun share 0.03 -> 0.3, stagger 2.5 -> 4 s, aggro cooldown 6 -> 5 s; FIX the slug x2 that never
  applies (Wyrm31.cs:107 looks for "slug" in the weapon name; the slug fires as "shotgun", ChargeShots27.cs:277).
- LOOM: pool 2400 -> 1200, other guns 0.01 -> 0.25, nailgun away from joints 0.12 -> 0.35, full-damage reach 6 -> 7.5 m, hang 3 -> 4 s;
  the hips hang (about 5 m over the lava, or a shorter rope there) and every joint is reachable.

**bossMarks (view):** each boss's damage spot glows (cyan-white: player-usable), and an open window is unmistakable (the frames showed
the Stag, Wyrm and Loom as dark boxes with no "hit here"). New file `Game/BossMarks32.cs`; Boss31View.cs only for a hook line.

**Proof:** harness `boss32 <boss> --seeds 20` (p10 / p50 / p90, deaths, % of hits at full damage) and `camp32 <boss>` (every camper
loses; the boss takes damage from every camper spot that can see it), before (`--off32`) and after. The lead runs the Player reels
`bosses32` and `camp32`.

## C: level 5's boss, THE CROWN (design.n32.boss5)

Built to the B rules from the start. A blocky crowned core (cubes, like the web's models; red-orange danger, never magenta) hovering
over a TRUE CIRCLE arena (radius about 18 m, 64 wall facets, floor a disc, built from triangles the way `Core/Enemies/Lantern30Arena.cs`
builds the Lantern ring: view and collider from the same triangles). Kill band 50-75 s with all four weapons.
- Always hittable body (x1.0) plus three cyan weak plates (x2.5) that face different ways, so the player must circle.
- Beat 1 (100-55 %): a sweeping beam rotates round the ring at waist height: jump or slide it; orbs the gauntlet can parry back.
- Beat 2 (55-20 %): floor sectors drop to lava in a rotating pattern (M3 "Drop Ring"); grapple rings appear over the rim, so the limited
  grapple matters; the core drops lower (easy to hit).
- Beat 3 (20-0 %): it charges across the ring like the Stag; a wall slam stuns it 3.5 s with its plates open (x2.5).
- Anti-camp: standing still 2.5 s marks the spot and a mortar lands 1 s later; a player on the rim wall-ride is not "still".
- At most 1.5 s unhittable at each beat change. Damage to the player is fair: every attack has a 0.6 s+ tell.
C owns `Core/Enemies/Crown32.cs`, `Core/Enemies/Crown32Arena.cs`, `Game/SurvivalSim.Boss32.cs` (the boss5 part), `Game/Crown32View.cs`,
harness `boss32 crown`, reel `boss5`. The boss room id and plan boss id are agreed with F: the plan's boss room is `v9`, boss id
`crown`; C hands the lead the one routing line for `SurvivalSim.Boss25.cs` (like Tools/night/n31-stag-wire.cjs).

## G: the grapple has charges; the balance pass; the HUD errors (grapple32, balance32; views grapplePips, hud32)

**Grapple (D-1 defaults):** 2 charges; a spent charge comes back after 4 s (one at a time); at least 0.8 s between two pulls; a non-boss
kill gives one charge back; landing does not refill; a press with no target spends nothing; tap-pull and hold-swing both spend one; a
jump or dash during a pull keeps at most 20 m/s (A1: a cancel kept 44 m/s and skipped the 12 m/s cap: the endless chain); tap-pull targets
rings, boss joints and big enemies only, not bare walls (A1: any wall within 60 m was a target); range 40 m. No edit of Mover.cs: the
press is cleared in `GrappleTap31.Apply` / `SurvivalSim.Grapple31.cs` before the Mover reads it.
**Proof:** a harness grapple-spam bot (press every frame toward the nearest target for 20 s): NIGHT31 pulls / 20 s and mean speed vs
NIGHT32's (at most 2 pulls, then one per 4 s; plus kills); the level-4 chain still completes on S's level. `grapplePips`: two cyan pips by
the reticle, refilling.
**Balance (balance32):** a harness table of time-to-kill for each weapon against each common enemy and each level room's clear time
(bot), before / after; fix only outliers (a weapon that is never worth using, a room that takes over 3x the median), each change measured.
**hud32:** no round countdown during a boss fight; the LT label never cut ("CHARGIN"); a room's lesson line clears when the room changes.

## A: the round ring and new arenas on levels 1-3 (design.n32.arenas32)

**The ring (GG: "it needs to be a circle"):** keep the Lantern's 16 logical sections (vents, sweep panel, platforms, wall panels: the fight
rules) and draw AND collide 64 facets (`facetsPerPanel` 4 under n32.arenas32); the floor a disc; the refuge platforms curved to the wall.
Files: `Core/Enemies/Lantern30Arena.cs`, `Game/Boss30View.cs`, `Game/LavaView30.cs`, `Game/Bot/LanternPilot30.cs`, and only the panel
geometry lines of `Game/Boss31View.cs` (208-212). Extend the harness `seam30` wall-ride lap test to 64 facets: the speed kept over a full
lap must be at least NIGHT31's 16-panel value (A2: 61 % kept per corner at 16), Mover.cs untouched. The lead captures frames.
**New arenas:** each of levels 1-3 gets one NEW arena room and its most boring room rebuilt (judged on the top-down render and the room's
size and height count), from M3's list (Turntable, Drop Ring, Half-Pipe, Gantry Cross, Chimney, Crucible Tiers, Vent Crawl) or better.
Each: at most 24 m across, two or more heights, a hazard edge, an enemy climb route, a loop. New files `n32-foundry` / `n32-iceline` /
`n32-kiln` (generators `Tools/night/gen-n32-*.cjs` built on the n30 generators; the n30 files stay as they are), named in
`design.n32.arenas32.levelSwap`. Spawn rule (no spawn within 6 m of an entry), lesson lines at most 6 words, pickup sizes as plain doubles.

## S: level 4 rebuilt (design.n32.span32)

From scratch (A2: today 8 boxes in a straight 235 m line, every door on x = 0). 8-10 rooms, each at most 24 m across except the boss deck;
turns and doors off-centre; two or more heights per room; lava edges (no bottomless pits). Built around the LIMITED grapple: routes need a
charge plan (pull, wall-ride, pull; a kill gives one back), never three pulls in a row without ground, a ride or a kill. Keep: the NAILGUN
pickup in the first room (pickup, award, both weapon tables, the reel entry), the grapple teaching order (one ring, then two with a ride
between, then a moving fight with rings), the Loom deck room with its `loomLegs` and `deckBreak31` rows, and room ids s1-s8 where a room
keeps its job (bots and reels script against them). New `Design/levels/n32/span.plan.json` -> `n32-span.json` via
`Tools/night/gen-n32-span.cjs`; `design.n32.span32.levelSwap` {"n30-span": "n32-span"}. Proof: the walk bot clears it on 10 seeds, the
grapple chain bot completes every ring route with the 2-charge rule ON, top-down render looked at.

## F: level 5, THE SPIRE (design.n32.level5)

The finale (M3 section 4): a climb up a dusk tower that remixes each earlier level's verb, one per room (a lava foundry room, an ice glide
room, a kiln vent room, a grapple ring room), then a GAUNTLET room that combines them, then THE CROWN's ring (C) at the top. 8-10 rooms,
the same room rules as A. All four weapons owned from the start; level 5 earns the TORCH if it works in the campaign today (the old
unlock table's n 5), else nothing (say which in the commit). Id `n32-spire`; `design.n32.level5.id` = "n32-spire"; a map row "LEVEL 5";
playable level-select slots 4 -> 5 (`UnityTuning.Hud30.cs:39`, `UiLevelSelect26.cs:67`) while the row is on; a weapon row with n 5 in
`design.n31.unlocks.levels.rows` and `Design/levels/unlocks.json`; lighting / props entries; every list in A2's level-5 checklist. The
boss room is `v9` with plan boss id `crown`, a flat 44 x 44 m space C's arena fills (model it on n30-foundry's f12 for the Lantern).
Until C's boss lands, `v9` must still clear (a placeholder wave) so the level can be walked end to end.

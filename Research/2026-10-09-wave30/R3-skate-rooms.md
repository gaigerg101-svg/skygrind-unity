# R3: Skate surfaces, combat room craft, one-mechanic levels (wave 30 research)

Web-only research, 2026-10-09, about 15 minutes. All web content was treated as data. Quotes are under 15 words.
Anything not opened directly is marked "(search summary)" or "not checked". Unit notes: Quake/Source units are
roughly inches (1 u ~ 2.54 cm); ULTRAKILL "u" are Unity metres.

---

## 1. Momentum surfaces that encourage skating without locking the player

### Numbers found

| Game / source | Value | Notes |
|---|---|---|
| Quake 1 (`sv_friction`, `sv_accelerate`, `sv_stopspeed`) | 4 / 10 / 100 | (search summary) [QdQ console](https://quake.speeddemosarchive.com/quake/qdq/articles/console.html), [ezQuake physics vars](https://www.quaddicted.com/files/webarchive/ezquake.sourceforge.net_20121129/docs/index.html%3Fvars-serverside-physics.html) |
| Quake 3 (`pm_friction`, `pm_stopspeed`, `pm_maxspeed`) | 6 / 100 / 300 | (search summary) [q3df groundboost wiki](https://q3df.org/wiki?p=8), [Quake 2 pmove.c](https://q2pb.sourceforge.net/quake2_html/5799.html) |
| Quake friction formula | `control = max(speed, stopspeed); drop = control * friction * dt` | Friction is proportional to speed above `stopspeed`, so it is an exponential decay: at friction 4, speed halves in ~0.17 s on the ground. A "skate" surface sets friction near 0 (ULTRAKILL gasoline, below). |
| Source (CS / CS:S / CS:GO) `sv_friction` | 4 / 4 / 5.2 | (search summary) [VDC sv_friction](https://developer.valvesoftware.com/wiki/Sv_friction) (page returned 403) |
| ULTRAKILL walk / slide / dash / slam | 16.5 / 24 / 49.5 / 100 u/s | [ULTRAKILL wiki, Movement](https://ultrakill.wiki.gg/wiki/Movement) |
| ULTRAKILL slide | enters at old speed, decays by friction back to 24; cap 72 u/s (3x base) | same page: "progressively lose their momentum to friction until they reach 24" |
| ULTRAKILL gasoline floor | "removes friction" up to ~1100 u/s | same page. This is the closest existing analogue to a skate floor: a surface tag, not a mode. |
| ULTRAKILL slopes | steeper than ~57.6 deg prevent acceleration | same page |
| ULTRAKILL slide-hop practical max | ~70 u/s; dash-jump ~49; ULTRABOOST ~140 | (search summary) [ULTRAKILL wiki SSJ](https://ultrakill.wiki.gg/wiki/SSJ) |
| Titanfall-style slide (student recreation, NOT Respawn's numbers) | slide boost 180-380 u/s on entry, boost cooldown 1.5 s, braking 200 -> 400 u/s^2 ramped over 3 s, slope force 400 u/s^2, slide steer 1.8 rad/s, fatigue 4 levels, hard cap 7000 u/s | [UCSD CSE125 group2 TitanfallConstants.hpp](https://cse125.ucsd.edu/2026/cse125g2/docs/TitanfallConstants_8hpp_source.html). Useful as a worked design, not as truth about TF2. |
| Apex slide | flat: ~1 s before deceleration; downhill: longer, "even turn into acceleration at steep angles"; sprint 7.59 m/s | (search summary) [Apex wiki Sliding](https://apexlegends.wiki.gg/wiki/Sliding), [Apex fandom Movement](https://apexlegends.fandom.com/wiki/Movement) |
| Lucio wall ride (Overwatch) | +30 % move speed while wall riding; no speed loss in flight above 5.5 m/s | (search summary) [Overwatch wiki Lucio](https://overwatch.fandom.com/wiki/L%C3%BAcio), [Wall Ride](https://overwatch.fandom.com/wiki/Wall_Ride). Attach control (hold jump) not checked in this pass. |
| Tribes skiing | no public engine numbers found; origin is a physics bug | "jumping is how players deal with friction, while gravity provides speed" (search summary of [Tribes wiki Skiing](https://tribes.fandom.com/wiki/Skiing), page returned 402). Players exceeded 100 km/h ([PC Gamer](https://www.pcgamer.com/au/great-moments-in-pc-gaming-learning-to-ski-in-tribes), search summary). Became a deliberate feature in Tribes 2 ([Wikipedia](https://en.wikipedia.org/wiki/Starsiege:_Tribes)). |

### Patterns (how speed-skating feel is built)

1. **Friction is a surface property, not a mode.** ULTRAKILL gasoline and Tribes slopes keep every normal verb live
   (jump, dash, shoot, steer); they just remove the brake. No button to enter, no button to leave.
2. **Gravity is the accelerator; the player is the steering.** Tribes, Apex and the TF-style slide all add speed only
   from slope (TF recreation: 400 u/s^2 times slope). Flat skate floors need either near-zero friction (keep what you
   brought) or a rhythmic "pump" input to add speed.
3. **Pumping = timed input that pays.** Tribes: tap jump on the downslope. Titanfall slide-hop: jump on landing to dodge
   ground friction ([Titanfall wiki Slide Hop](https://titanfall.fandom.com/wiki/Slide_Hop), search summary). The
   reward is "lose nothing", not "gain a lot": the player keeps speed by doing the rhythm.
4. **Speed caps are soft and layered.** ULTRAKILL slide decays to 24 and caps at 3x base (72); gasoline is
   frictionless up to ~1100. TF recreation: boost on entry, then braking that ramps up over 3 s, plus "fatigue" that
   kills the entry boost if spammed. A cap that bites gradually feels like drag, not a wall.
5. **Steering stays responsive by limiting turn rate, not by removing it.** The TF recreation caps heading change at
   1.8 rad/s (about 103 deg/s) in a slide: you can always turn, just in arcs. Air-strafe gains speed in turns in
   Quake/Titanfall (search summary), so turning is a skill, not a cost.
6. **Boost meters are earned in-flow.** SSX: tricks fill boost; full meter = infinite boost while you keep it up; you
   cannot boost in the air, so the ground is the fast lane ([SSX notes, TrueAchievements](https://www.trueachievements.com/game/SSX/walkthrough/2), search summary). Sonic Unleashed: boost spends ring
   energy; one version uses chunked gauges usable only on a full chunk ([Sonic Retro Boost](https://info.sonicretro.org/Boost), search summary).
7. **Rail entry speed is preserved, not replaced.** Jet Set Radio: grind speed "depends on how fast they were" before
   the rail, then slowly decays ([JSR wiki Grinding](https://jetsetradio.fandom.com/wiki/Grinding), search summary; page 402).
   SSX: grinding keeps Tricky active. Rails are a way to keep speed through a section, not a separate faster mode.
8. **Snap vs soft magnetism.** Tony Hawk: press grind near a rail and you "magnet" to it; indie devs call
   "press the button, magnet towards the rail" the winning formula ([itch.io thread](https://itch.io/post/15654276),
   search summary). The restriction people dislike is a rail that grabs without a press or will not let go; the assist
   people like is a button-gated pull over a small radius with a free jump-off at any time. JSR's stated intent:
   rails let you enjoy speed "while still being aware of their surroundings" (search summary of JSR wiki).
9. **Rhythm-locked rails.** Hi-Fi Rush grapple lines and rails ride along the music; platforms move on beats
   ([GGRecon review](https://ggrecon.com/reviews/hi-fi-rush-review), search summary). A rail is a calm stretch where
   aim is free.

---

## 2. Combat room layout craft

### Sources and what they say

- **Level Design Book, layout typology** ([book.leveldesignbook.com/process/layout/typology](https://book.leveldesignbook.com/process/layout/typology)):
  - Arena: "an open area with several lanes of circulation."
  - Combat bowl / "ring around the rosie": small open area with full cover (pillar) in the middle, "resulting in a
    spinning/rotating flow." This is the ring route.
  - Single-player arenas need gating so players cannot exit early or camp a chokepoint.
- **Level Design Book, verticality** ([.../flow/verticality](https://book.leveldesignbook.com/process/layout/flow/verticality)):
  - Cap at three floor planes: "A fourth plane doesn't add new dynamics."
  - One-way drops force forward flow; ramps, stairs and ladders need reserved space.
  - Gamepad players struggle with steep verticality: use "discrete floor planes that remain relatively flat."
    (SKYGRIND is pad-first: this applies directly.)
  - The Q3DM17 sniper perch is the named example of a height advantage.
- **Level Design Book, cover** ([.../combat/cover](https://book.leveldesignbook.com/process/combat/cover)):
  - "Low cover is better than tall cover" (keeps flanks visible). "Less cover is better."
  - Metrics (Uncharted 4): high cover >= 1.75 m, low cover 1.0-1.25 m, not cover <= 0.5 m.
  - Narrow cover pushes movement; wide cover gives reliable safety. Freestanding cover allows 360 deg play.
  - Open arenas suit boss fights and setpieces.
- **Level Design Book, encounters** ([.../combat/encounter](https://book.leveldesignbook.com/process/combat/encounter)):
  - "Give the player an opportunity to survey the arena before the fight" (a foothold / vista).
  - 5+ enemy types turn to chaos; players "focus on a small subset."
  - Telegraphs ~2-3 s anticipation (God of War example).
- **DOOM Eternal** ([Adam Saltsman, "Design of Doom Eternal"](https://blog.adamatomic.com/post/613311014289768448/design-of-doom-eternal)):
  - Two tall-arena types: **jungle gyms** (distinct floors, separated levels) and **canyons** (open, fewer divisions).
  - Jungle gyms "allow you to jump from skirmish to skirmish"; more obstacles reduce ranged chip damage.
  - Canyons strain focus: little cover, fewer mobility hooks.
  - id's arenas use monkey bars, boost pads, teleporters as modular parts; Hugo Martin steers resupply to come from
    enemies rather than litter ([GameSpot](https://www.gamespot.com/articles/why-doom-eternal-incorporates-puzzle-platforming/1100-6473287/), search summary). Pickups lure players to new places like Mario coins ([Game Developer](https://www.gamedeveloper.com/design/the-dark-souls-design-of-doom-eternal), search summary).
- **Quake deathmatch** (search summary of several pages incl. [mariteaux Quake DM maps](https://mariteaux.somnolescent.net/modding/quake/deathmatch/), [Unreal Wiki Mapping For DM](https://unrealarchive.org/wikis/unreal-wiki/Legacy:Mapping_For_DM.html)):
  - A loop is a path through every major pickup "without ever having to stop or turn 180 degrees."
  - Power positions are defined by sightlines (can the rail perch see the red armour?).
  - Jump pads and teleporters "reset the fight" vertically. Pickup sounds reveal positions.
  - Not checked: real top-down images of DM6 / Q3DM6 (no image pages opened).
- **ULTRAKILL rooms** (wiki, search summary): the 2-4 arena is "a large, open room" with a stairway, two centre pillars
  and a red pedestal that triggers the fight ([2-4 wiki](https://ultrakill.wiki.gg/wiki/2-4)). Named arenas carry one
  strong identity element (a room of columns over a flesh floor; four Cerberus statues at the corners). Not checked:
  any Hakita interview on room craft.
- **Dusk:** no source found in this pass (not checked).

### Checklist: a blocky room that is not boring

1. **Hero object:** one unmistakable centre piece (pillar, statue, tower) that also does a job (full cover for the ring
   route). Visible from every entrance.
2. **Three height levels, not four:** floor, mid (1-2 jumps), high perch. Each level flat enough for a pad.
3. **Ring route:** at least one loop around the hero object with no 180 deg turn; pickups on the loop.
4. **2+ ways up and 2+ ways down:** a ramp, a jump pad / climb route, and one-way drops. Enemies need a climb route too
   (GG: "There always really needs to be a route that enemies can climb up").
5. **Cover mix:** mostly low cover (1.0-1.25 m), a little full cover (>= 1.75 m), freestanding pieces in the middle,
   directional cover at edges. Less is better.
6. **Object variety:** at least 3 distinct silhouettes (column, low wall, ramp/block, rail) plus one dynamic element
   (pad, moving platform, hazard).
7. **Sightlines:** one long lane (power position), several short broken lanes; the perch sees a reward but is exposed.
8. **Lit landmarks:** each corner or side readable by a light or colour cue so the player knows facing after a fast turn.
9. **Foothold:** an entrance that previews the room before the fight; gates so the fight cannot be skipped.
10. **Choke and open:** one tight connector (risk, short cut) and one open area (crowd kiting).
11. **Few enemy types per room** (under 5), with telegraphs of 2-3 s for big attacks.

---

## 3. A level built around one weapon / one mechanic, with a boss that tests it

1. **Hades, Tartarus spike traps -> Megaera.** Tartarus spike traps deal low damage to Zagreus and high damage to
   enemies; you push or lure enemies onto them. Megaera's arena is a rectangle lined with pillars, "with Spike Traps in
   between the pillars", so the biome's trick (use the floor against them) is the boss's best tool
   ([Prima Games, Furies](https://primagames.com/gaming/hades-boss-guide-furies-megaera-alecto-and-tisiphone), [Hades wiki Tartarus](https://breezewiki.discard.no/hades/wiki/Tartarus), search summaries).
2. **ULTRAKILL Layer 1 -> V2 (1-4).** Layer 1 teaches slide, dash, jump, the Piercer revolver and core-eject shotgun.
   V2 is a mirror: "able to slide, jump, dash, and use the Piercer Revolver"; the wiki calls it a "test of all the
   player's abilities." V2 telegraphs its movement plan by wing colour (yellow run, blue circle, red rush, green retreat)
   ([ULTRAKILL wiki V2](https://ultrakill.wiki.gg/wiki/V2), [V2 second encounter](https://ultrakill.fandom.com/wiki/V2_(Second_Encounter))).
   Arena: a water-filled chamber, open.
3. **Neon White, cards -> Neon Green with the Book of Life.** Every level is a card-routing puzzle (each card is a gun
   and a discard movement verb). The final fight hands you the Book of Life card and turns the fight into
   teleport routing: teleport to crystals around a tower, then to Green's own projectiles to bridge gaps
   ([TheGamer mission 12](https://www.thegamer.com/neon-white-every-mission-12-hand-of-god-shortcut-location-guide/), [Wikipedia](https://en.wikipedia.org/wiki/Neon_White), search summaries).
   The boss's attacks become the route.

Bonus: **Hi-Fi Rush** bosses attack on the beat and include rhythm-matching parry duels, the same rhythm the stage's
platforms and rails run on ([GGRecon](https://ggrecon.com/reviews/hi-fi-rush-review), search summary).
ULTRAKILL 2-4 (Minos) was checked but no source tied it to a Layer 2 mechanic; not used.

**Pattern:** the boss either (a) mirrors the player's kit (V2), (b) fights on the level's hazard so the hazard is the
player's weapon (Megaera), or (c) turns its own attacks into the level's traversal targets (Neon Green).

---

## What SKYGRIND should take

1. **Make "skate" a floor tag, not a mode:** a surface with near-zero ground friction (ULTRAKILL gasoline style) where
   every verb stays live; no enter / exit button. Friction value goes in a Unity-side config file (CLAUDE rule 5).
2. **Speed from slope and rhythm, not from a button:** downhill adds speed (TF-style slope force); a timed jump or slide
   on landing preserves speed (Tribes / slide-hop). Flat skate floors keep what you brought.
3. **Soft, layered cap:** decay toward a cruise speed above a soft cap (ULTRAKILL slide: 3x base), braking that ramps
   in over ~3 s, plus a hard cap far above. Never a wall.
4. **Turn-rate limit, not steering loss:** cap heading change on skate floors (~1.8 rad/s in the TF recreation) so turns
   are arcs; keep air-strafe gains so turning is a skill.
5. **Rails: button-gated magnetism, free release.** Press to pull onto a rail within a small radius; entry speed is kept;
   jump off at any time; rails never grab without input. Aim stays free on the rail.
6. **Earned boost if any:** fill from kills / style in flow, ground-only, chunked (SSX / Sonic). Any crowd-control or
   movement verb on it keeps GG's 15 s+ cooldown rule.
7. **Every arena: hero object + ring route + three flat floor planes** (pad-friendly), 2+ ways up including an enemy climb
   route, one-way drops, mostly low cover (1.0-1.25 m equivalents).
8. **Lit landmarks per side and a foothold vista** so a fast-turning player keeps orientation and can read the room
   before the wave starts.
9. **Pickups on the loop, resupply from kills** (DOOM): place refills to pull players across the skate floor and onto
   rails, not in corners.
10. **One mechanic per map, tested by its boss:** pick one of the three patterns: a mirror boss that skates (V2), a boss
    fought on the skate floor whose hazards hurt it more than you (Megaera), or a boss whose projectiles / pieces are
    rail or teleport targets (Neon Green).

---

Not checked this pass: Dusk room craft; real top-down DM6 / Q3DM6 images; DOOM Eternal GDC talk text; Tribes engine
constants; Lucio attach rules; official Titanfall 2 numbers (the TF table is a student recreation).

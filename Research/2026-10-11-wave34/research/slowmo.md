# Player-triggered slow motion: what the best action games do (wave 34 research)

Web research, 2026-10-11, about 30 tool calls. Some sources were read only through search-result summaries; those are
marked "(search summary)". Fandom wikis (Max Payne, Katana Zero) returned HTTP 402, doomwiki returned 403, and the
ULTRAKILL wiki mirror was denied, so several exact numbers are "not found".

## Answer

The games that make slow motion feel heavy all slow the *world* while the player keeps aiming and reacting at normal
speed (F.E.A.R. says this outright), and they pay for it with a meter that refills through aggression, not waiting:
kills in Max Payne 2 and Severed Steel, being shot at in Max Payne 3, landing hits in Sifu. F.E.A.R. and Katana Zero
refill on their own over time. Published numbers are rare. The only hard ones found are ULTRAKILL's 0.25 s major
hitstop (parry), Severed Steel's 10-20 s default bullet-time meter, Max Payne 3's shootdodge at one sixth of the
multiplayer meter, and lab results of 0.1-0.2 s for hitstop and 0.2-0.4 s for slow-motion impact beats. No game's
world time-scale value (for example 0.25x) was found in any source. For SKYGRIND that points to: world-only slow,
player at full rate, a short meter (seconds, not tens of seconds) refilled by kills, a long cooldown on the grapple
trigger, a hard hitstop-style entry beat, and a world-step skip pattern inside the fixed 120 Hz loop so timers stay in
whole steps.

## 1. Games x numbers

| Game | Mechanic | Time scale | Ramp in / out | Max duration | Meter / cooldown, refill | Source |
|---|---|---|---|---|---|---|
| Superhot | Time runs normally only when you move, shoot, turn, jump or pick up; otherwise slowed to a crawl | not found | not found | unlimited (core rule) | none | [Wikipedia: Superhot](https://en.wikipedia.org/wiki/Superhot) |
| Superhot VR | Only deliberate body movement advances time; head turns and small twitches do not | not found | not found | unlimited | none | [Wikipedia: Superhot](https://en.wikipedia.org/wiki/Superhot) |
| Max Payne 1 | Bullet Time: first slow motion where the player keeps aiming and shooting | fixed single level (not quantified) | not found | hourglass meter | hourglass | [Wikipedia: Bullet time](https://en.wikipedia.org/wiki/Bullet_time) |
| Max Payne 2 | Hourglass drains; each refill to the top by kills turns it brighter yellow, world slows further and Max moves faster; maxed reload triggers a 360 spin with deeper slow | levels deepen with kills (not quantified) | not found | until hourglass empties | kills refill it mid-use | [GamesFirst MP2](https://gamesfirst.com/articles/2003/max-payne-ii-the-fall-of-max-payne/), [Giant Bomb MP2](https://giantbomb.com/wiki/Games/Max_Payne_2_The_Fall_of_Max_Payne) (search summary) |
| Max Payne 3 | Bullet time from a HUD meter; shootdodge dive | not found | not found | meter | SP: fills when enemies shoot at you (fast with 5+ shooters). MP: shootdodge costs half a segment = 1/6 of the meter; fills from kills, looting, denying streaks | [TrueAchievements MP3](https://www.trueachievements.com/max-payne-3-walkthrough.htm?page=2), [GamesRadar MP3 MP guide](https://gamesradar.com/max-payne-3-multiplayer-guide/3) (search summary) |
| F.E.A.R. | Reflex time: world slows, player aims and reacts at normal speed | not found | not found | meter | refills slowly on its own when not in use; reflex injections raise the cap | [Wikipedia: F.E.A.R.](https://en.wikipedia.org/wiki/F.E.A.R._(video_game)) |
| Ghostrunner | Sensory Boost: hold Shift / LB; in the air it slows time "for a few seconds" and lets you strafe sideways to dodge | not found | not found | "a few seconds" | upgrades extend boost time | [GodisaGeek tips](https://godisageek.com/2020/10/ghostrunner-beginners-tips/), [Select Button review](https://selectbutton.com/reviews/ghostrunner-review) (search summary) |
| Severed Steel | Left trigger bullet time; unlimited while doing a stunt | not found | not found | 10-20 s default | kills refill the meter | [Finger Guns review](https://fingerguns.net/reviews/2021/10/03/severed-steel-review-pc-running-hot/), [bit-tech review](https://bit-tech.net/reviews/severed-steel-review/1/) (search summary) |
| Katana Zero | Chronos slow motion to read and deflect bullets | not found | not found | "a few seconds" | meter refills gradually over time | [USgamer review](https://www.usgamer.net/articles/katana-zero-review), [GameLuster review](https://gameluster.com/review-katana-zero-an-instant-kill-adrenaline-rush/) (search summary) |
| Mirror's Edge | Reaction Time: bullet time to pick a route "without losing momentum" | not found | not found | not found | not found | [Shacknews preview](https://www.shacknews.com/article/53527/new-mirrors-edge-details-and) (search summary) |
| Sifu | Focus: hold LT, time slows to a crawl, pick a body part, spend a segment | not found | not found | while held (not quantified) | segmented gauge fills by landing blows | [Pro Game Guides](https://progameguides.com/sifu/what-does-focus-do-in-sifu-explained/) (search summary) |
| Doom Eternal | Time slows while the weapon wheel is open; a "BFG weapon wheel slowdown" exploit has its own wiki page | not found | not found | while held | none | [doomwiki: BFG weapon wheel slowdown](https://doomwiki.org/wiki/BFG_weapon_wheel_slowdown) (title only, page 403), [Prima guide](https://primagames.com/eguides/doom-eguide/basics/combat) (search summary) |
| ULTRAKILL | Hitstop: time and music stop briefly; major hitstop (parry, interruption) = 0.25 s freeze plus white screen flash | 0 (freeze) | instant | 0.25 s (major) | event-driven, no meter | [ULTRAKILL wiki: Parry](https://ultrakill.wiki.gg/wiki/Parry), [Hitstop mirror](https://breezewiki.discard.no/ultrakill/wiki/Hitstop) (search summary) |
| Titanfall 2 | No general slow motion found; "Effect and Cause" swaps between two time periods instead | n/a | n/a | n/a | n/a | [Rolling Stone](https://www.rollingstone.com/culture/culture-news/titanfall-2-everything-we-know-about-blockbuster-sci-fi-game-sequel-110080/) (search summary) |
| Neon White, Hotline Miami, Bionic Commando, Just Cause, Sekiro | not checked or not found in budget | | | | | |
| Grapple + slow motion | Indie only: HookShot (grapple, air dash, time slow), Swift Swing (slow mo on limited stamina), SUPERHOOK (Superhot rules plus a hook) | not found | not found | not found | Swift Swing: stamina | [HookShot](https://elimcardle.itch.io/hookshot), [Swift Swing](https://cerealpine.itch.io/swift-swing), [SUPERHOOK](https://games.softpedia.com/get/Freeware-Games/SUPERHOOK.shtml) (search summary) |

No AAA game was found where firing a grapple itself triggers slow motion. That is open ground, not a proven pattern.

## 2. Asymmetric time (player vs world)

- F.E.A.R.: the world slows "while still allowing the player to aim and react at normal speeds".
  [Wikipedia: F.E.A.R.](https://en.wikipedia.org/wiki/F.E.A.R._(video_game))
- Max Payne 1 is credited as the first slow motion where the player keeps aiming and shooting control.
  [Wikipedia: Bullet time](https://en.wikipedia.org/wiki/Bullet_time)
- Max Payne 2 scales the player *up* as the world slows deeper: deeper bullet-time levels let Max move faster, and
  reloads are instant. [GamesFirst MP2](https://gamesfirst.com/articles/2003/max-payne-ii-the-fall-of-max-payne/) (search summary)
- Superhot scales everything including the player's own bullets, but the player's input is the clock: turning,
  shooting and moving all advance time. VR removes head turns from the clock so looking is free.
  [Wikipedia: Superhot](https://en.wikipedia.org/wiki/Superhot)
- Mouse / stick look: no source found that documents look-speed handling. Max Payne, F.E.A.R. and Superhot VR all keep
  the camera at full rate by design (above); our inference is that look must never be scaled.
- Unity's own model: `timeScale` changes the step reported to `Update` / `FixedUpdate`; `unscaledDeltaTime` keeps UI
  running. [Unity Manual: Time scale](https://docs.unity3d.com/Manual/time-scale.html). Not usable for our sim (rule 4: one
  fixed loop of our own), but it shows the standard split: world on scaled time, player and UI on unscaled.

## 3. Screen language

- Superhot reads by colour alone: white world, red enemies, black usable objects.
  [Wikipedia: Superhot](https://en.wikipedia.org/wiki/Superhot)
- Max Payne 2 shows depth in the meter: the hourglass turns brighter yellow at each deeper level.
  [GamesFirst MP2](https://gamesfirst.com/articles/2003/max-payne-ii-the-fall-of-max-payne/) (search summary)
- ULTRAKILL telegraphs a heavy beat with a full freeze, a white flash and the music stopping.
  [ULTRAKILL wiki: Parry](https://ultrakill.wiki.gg/wiki/Parry) (search summary)
- Desaturation, vignette, chromatic aberration, radial blur, FOV change: no primary source with numbers found in budget.
- Sound pitch: noted only (GG owns sound, rule 7). The ULTRAKILL music stop above is the one documented example.
- Entry vs exit: no source documents ramp durations. The research on impact effects found 0.1-0.2 s hitstop and
  0.2-0.4 s slow motion as preferred lengths for a blow-off hit.
  [SciTePress 2024 paper](https://www.scitepress.org/Papers/2024/124614/124614.pdf) (search summary)

## 4. Implementation in a fixed 120 Hz loop (engineering reasoning, not sourced)

- Do not scale `dt`. Our movement goldens and 16 trajectory replays are bit-exact at dt = 1/120 (CLAUDE.md rule 3);
  a fractional dt breaks them and breaks whole-step timers (rule 4).
- Scale by *step skipping* per entity group: the player group runs every sim step; the world group (enemies,
  projectiles, spawners, hazards, boss timers) runs on step k only when a world-step accumulator crosses 1. With world
  scale s = 1/N (0.5, 0.333, 0.25, 0.2) this is an exact integer pattern: one world step every N sim steps. Every
  timer stays in whole steps and the run is deterministic and replayable.
- Visual smoothness for slowed actors: render interpolation already exists; interpolate the world group on its own
  alpha (world steps elapsed fraction), or slowed enemies will stutter at 120/N Hz.
- Ramps: change N on whole steps along a fixed table (for example 1, 2, 3, 4) so the ramp is data, not a curve.
- Hitstop interaction: keep hitstop as a full freeze of both groups that sits above slow motion; the slow-motion
  meter does not drain during hitstop steps; hitstop length is not multiplied by N.
- Open source: "Quite Cold", a 2D Superhot-like, is listed as MIT-licensed code
  ([itch.io](https://prox276.itch.io/quite-cold), search summary; licence file not checked). Unity's docs pattern
  above is reference only. No MIT / Apache / BSD 3D implementation verified.

## 5. Design risks and the rules that fixed them

- Spam / trivial combat: meters that cap use (F.E.A.R., Katana Zero, Max Payne). The best-documented fix is refill
  by aggression: Severed Steel "if you want to slow things down, you can't afford to slow down" (kills refill), Max
  Payne 2 deepens slow motion on kills, Max Payne 3 fills when shot at.
  [Finger Guns](https://fingerguns.net/reviews/2021/10/03/severed-steel-review-pc-running-hot/) (search summary)
- Flow killer: Superhot makes the whole game slow motion; that works as a puzzle shooter, not inside a speed loop.
  Mirror's Edge pitches Reaction Time as keeping momentum. [Shacknews](https://www.shacknews.com/article/53527/new-mirrors-edge-details-and) (search summary)
- Exploit: Doom Eternal's wheel slowdown became a named BFG exploit, so an uncosted slow-mo menu gets abused.
  [doomwiki](https://doomwiki.org/wiki/BFG_weapon_wheel_slowdown) (page not read, 403)
- Too long: Severed Steel's 10-20 s plus stunt-unlimited lets players spend whole levels in slow motion
  ([bit-tech](https://bit-tech.net/reviews/severed-steel-review/1/), search summary).
- Project rule: GG wants every movement / crowd-control verb on a long cooldown, "every 15 seconds or even longer"
  (docs/memory/CORE.md, NIGHT20). A grapple-triggered slow motion is such a verb.

## Recommended starting numbers for SKYGRIND (all to go in a Unity-side config file, rule 5)

1. Trigger: grapple fire only, cooldown 15 s from the end of the slow motion. Source: GG's rule above; Ghostrunner
   ties its slow motion to an air verb ([GodisaGeek](https://godisageek.com/2020/10/ghostrunner-beginners-tips/)).
2. Entry beat: 0.25 s full freeze (30 steps) with a flash, then slow motion. Source: ULTRAKILL major hitstop 0.25 s
   ([wiki](https://ultrakill.wiki.gg/wiki/Parry)).
3. World scale: 1/4 (one world step per 4 sim steps). No game value found; chosen as a "heavy" step-exact value. Tune
   blind A/B between 1/3, 1/4 and 1/5 (L-001).
4. Ramp in: 0 (the freeze is the ramp). Ramp out: 0.3 s (36 steps) through N = 3, 2, 1. Source: 0.2-0.4 s
   slow-motion beat preference ([SciTePress](https://www.scitepress.org/Papers/2024/124614/124614.pdf)).
5. Duration: 3 s of real time (360 steps) per trigger. Source: Ghostrunner and Katana Zero "a few seconds"
   ([GodisaGeek](https://godisageek.com/2020/10/ghostrunner-beginners-tips/),
   [USgamer](https://www.usgamer.net/articles/katana-zero-review)); not Severed Steel's 10-20 s, which reviewers say
   covers whole levels.
6. Extension by kills: each kill during slow motion adds 0.5 s (60 steps), cap 6 s total. Source: kill refill in
   Severed Steel and Max Payne 2 ([Finger Guns](https://fingerguns.net/reviews/2021/10/03/severed-steel-review-pc-running-hot/),
   [GamesFirst](https://gamesfirst.com/articles/2003/max-payne-ii-the-fall-of-max-payne/)).
7. Player rate: camera, own movement, fire rate and weapon swap all at full rate. Source: F.E.A.R. reflex time
   ([Wikipedia](https://en.wikipedia.org/wiki/F.E.A.R._(video_game))); Max Payne 2 instant reload as the combo hook.
8. Overlay: one strong colour read, not a stack of effects; world desaturated, enemies kept red-orange, player-usable
   things cyan (our rule 8). Source: Superhot's three-colour read ([Wikipedia](https://en.wikipedia.org/wiki/Superhot)).
   A deepening tint per kill, like Max Payne 2's brighter hourglass, shows the extension.
9. Bosses: slow motion must not stack with STILL ARMOR camping fixes; judge the boss band in the Player (L-029 / L-040).

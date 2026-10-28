# Scout: boss weak-point readability and the kill beat (web only, 2026-10-27)

Gaps: no GDC talk found stating "one colour = shootable"; no published weak-point pulse rate, on-screen size, or boss-kill slow-motion factor for any game named. "Search excerpt" = seen in a search result, page not fetched. Blog numbers are tagged unverified.

## Q1. Weak point readable at a glance

**R1. The weak point is a physical part on the silhouette, never only an overlay.**
- DOOM Eternal's are mounted parts: Arachnotron turret, Pinky tail, Revenant shoulder launchers ([Thumbsticks](https://www.thumbsticks.com/how-to-exploit-demon-weaknesses-doom-eternal/)).
- Horizon: the Watcher's eye is an instant kill; scorpion designs were dropped because "the legs were so thin you barely had any surface to hit" ([PlayStation Blog](https://blog.playstation.com/2017/02/24/the-making-of-horizon-zero-dawns-machines/)). Minimum target surface is a design input.
- A combat designer: a weak point "should be displayed in an obvious way", and keep spikes off it, since players avoid threatening shapes ([Game Developer](https://www.gamedeveloper.com/design/opinion-boss-design---tips-from-a-combat-designer)).

**R2. One state, one colour, held for the whole game. Hues are not universal.**
- DOOM Eternal: stagger flashes blue, glory-kill range orange (search excerpt, [Twinfinite](https://twinfinite.net/guides/doom-eternal-glory-kills/)); [Shacknews](https://shacknews.com/article/117039/how-to-do-glory-kills-doom-eternal) only says staggered enemies "start glowing".
- ULTRAKILL: yellow flash = parryable, blue = not ([wiki](https://ultrakill.wiki.gg/wiki/Parry)). Blue means the opposite of DOOM's, so consistency inside one game is the rule.
- Cuphead: only pink can be parried ([wiki](https://cuphead.wiki.gg/wiki/Parry)); reviewers flagged colour-blind failure ([Giant Bomb](https://www.giantbomb.com/reviews/cuphead-review/1900-771/), search excerpt). Add a second channel: shape or motion.

**R3. Keep the colour channel quiet.** Horizon's team avoided "overloading the machines with blinking lights and colours during combat" so status reads at a glance ([Wikipedia](https://en.wikipedia.org/wiki/Machines_(Horizon))). Returnal sets bright high-contrast action against a much darker world ([Sixth Axis](https://www.thesixthaxis.com/?p=375524)). Frame test: the weak point is the brightest saturated pixels in frame, and nothing else shares its hue.

**R4. Pair exposure with a timed state change.**
- DMC4's frog tongue exists only in its vulnerability window.
- The window should be at least one full combo, plus travel time.
- Telegraph: 1-1.5 s sweet spot, 2 s max, before the player must count (all [Game Developer](https://www.gamedeveloper.com/design/opinion-boss-design---tips-from-a-combat-designer)).
- Frame test: an exposed frame and a closed frame differ in shape and colour.

**R5. The hit itself reads differently on a weak point.** Monster Hunter Wilds damage numbers: white = weak, yellow = weak point, red = wound ([Game8](https://game8.co/games/Monster-Hunter-Wilds/archives/500006)).
**R6. Not found:** pulse rate, pixel size, contrast ratio. Claim no number; A/B it.

## Q2. Kill beat that lands in a glance

**K1. Kill hit-stop sits at the top of the observed range.**
- Smash Ultimate hitlag is capped at 30 frames (0.5 s at 60 fps); a 15% hit gives 15 frames ([SmashWiki](https://www.ssbwiki.com/Hitlag)).
- ULTRAKILL "major" hitstops, parries among them, freeze 0.25 s, and the camera still moves ([wiki mirror](https://breezewiki.discard.no/ultrakill/wiki/Hitstop), search excerpt).
- Street Fighter V hit pause: 8/12/15 frames for light/medium/hard ([de Heras](https://threadreaderapp.com/thread/1385104062157254660.html)).
- Unverified blog tiers: 2-3 / 4-6 / 6-10 frames ([FreeGameSprites](https://freegamesprites.com/ar/news/game-feel-2d-combat-juice-tricks)).
- Build: kill freeze 0.25-0.5 s (30-60 steps at 120 Hz), other hits well below. Both attacker and victim freeze equally; there is a cap ([Sakurai via Source Gaming](https://sourcegaming.info/2015/11/11/thoughts-on-hitstop-sakurais-famitsu-column-vol-490-1/)).

**K2. Freeze, then slow, not one or the other.**
- Dead Cells crits: 1-frame freeze, then "a slow down of a few tenths of a second", blood spray, impact sound ([Game Developer](https://www.gamedeveloper.com/design/designing-each-of-the-50-weapons-in-i-dead-cells-i-to-feel-distinctive)).
- Bayonetta Witch Time slowdown is about 0.25 s ([de Heras](https://threadreaderapp.com/thread/1385104062157254660.html)).
- Smash KO slow motion lasts "about a second" ([SmashWiki](https://ssbwiki.com/Slow_motion)). No factor is published.
- Build: 1-3 frame freeze, then 0.25-1 s slow. The slow factor is yours to tune.

**K3. Make the frame legible: screen colour, radial sparks, zoom.** Smash's Special Zoom flips the background blue with sparks pointing at the impact, slows time and zooms to the impact; Finish Zoom does the same on red ([SmashWiki](https://www.ssbwiki.com/Finish_Zoom)). Both are readable in one still.

**K4. Hold the camera on the impact.** Zoom to the point of impact (Smash, above); keep the camera free during the freeze (ULTRAKILL, above). An unverified blog claims half-second boss-death pauses with a zoom ([gamedo-live](https://gamedo-live.pages.dev/news/game-feel-juice-guide-indie-devs)).

**K5. One large shake spike, linear decay.** Nuclear Throne's pistol: 6 px camera kick, +4 shake, fast decay ([infovore excerpt of the RPS interview](https://infovore.org/?p=5275), search excerpt). Trauma model: shake = trauma squared, trauma decays linearly ([Eiserloh GDC 2016](https://gdcvault.com/play/1023146/Math-for-Game-Programmers-Juicing); [Bevy port](https://bevy.org/examples/camera/2d-screen-shake/), search excerpt). Peak value is builder's choice.

**K6. Reserve the full stack for the kill.** Sakurai warns that too much hitstop slows the game (Source Gaming, above).
**K7. Not found:** letterbox timings, "BOSS DEFEATED" card timings, white-flash frames from a primary source (blogs say 1-3 frames, unverified).

## 3-frame strip a builder can test
1. Impact: freeze on, 1-3 frame white flash, sparks pointing in, screen tint.
2. +0.25-0.5 s: slow motion, zoom on impact, shake decaying.
3. +1 s: particle burst, corpse held in frame, card last.

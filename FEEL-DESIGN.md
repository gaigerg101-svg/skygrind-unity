# FEEL-DESIGN: SKYGRIND's feel, judged and redesigned (NIGHT16, U3-29.2)

Written 2026-10-05 from 14:42 by the NIGHT16 lead (anchor: `u3-n15` + the shared commit `390cdf3`). The process is the
web's NIGHT5 (`docs/FEEL-TARGETS.md` in the web repo): instrument first, write targets as numbers, change one thing,
measure again, look frame by frame, say what is still wrong.

**Tags:** **[M]** measured tonight by the instrument named; **[C]** read from the code or config (file:line); **[S]**
seen in a capture (Logs/shots/...); **[T]** a target, not yet measured. Every number below is one of the four.

## 1. What is wrong now (ranked by what a player feels in the first minute)

| # | Finding | Evidence | Feel in the first minute |
| --- | --- | --- | --- |
| 1 | **The slide is free speed.** A slide press snaps the speed to `slide.floorSpeed` 11.6 m/s (1.45 x `ground.walkSpeed` 8) at once; on flat ground a held slide never drops below it; release and press again pays the burst again. | [C] `Mover.StartSlide` (Mover.cs ~813: `max(floorSpeed x slideSpeedMult, speed)`), `StepSlide` (~896-903: friction only above the floor, then `speed = max(floor, speed)`); [C] config `slide.friction` 1.5, `floorSpeed` 11.6. Measured [M: SimHarness spam, the BEFORE half]: held 10 s 1.45 x a walk; tapped every 0.15-2.0 s 1.23-1.32 x, on the plane, the arena and DRYDOCK (section 3.1). | Holding C everywhere is the best play; the slide stops being a choice. GG felt it first. |
| 2 | **Slide-hops multiply with no falloff.** Every slide-jump x `slide.jumpHorizontalMult` 1.2 up to `jumpBoostMaxSpeed` 24. | [C] `MovementMath.SlideJumpSpeed` (MovementMath.cs:151-155), Mover.cs ~748-754. Measured [M: spam]: a chain from walk speed reaches the 24.0 m/s cap, 2.59-2.90 x a walk on flat ground. | A mashed hop chain reaches the cap on flat ground: the skill line (a well-timed first hop) and mashing pay the same. |
| 3 | **The parries are inverted.** A PERFECT PARRY freezes 0.15 s, a plain parry 0.25 s. | [C] `design.parry.perfectHitstop` 0.15; `design.ultra.parryHitstop` 0.25 (Ultra.cs ~40); SurvivalSim.cs ~288. | The better parry feels like less. The one moment GG named ("its like ultrakill where the game will almost pause") is backwards. |
| 4 | **The impact layer is thin.** Freezes exist only for a player kill (0.04 s), a melee hit above momentum 0.6 (0.045 s) and a parry. A headshot, a punch that connects, a rocket's direct hit and the round's last kill have none of their own; nothing ever slows. | [C] SurvivalSim.cs ~272-289; config `feel.hitstopTime` 0.04, `feel.momentumHitstop` 0.045 at `momentumHitstopAt` 0.6. | Hits land the same whatever you did: a headshot reads like a body shot. |
| 5 | **The kill freeze has no budget.** 0.04 s on every player kill, no cooldown, no cap. | [C] SurvivalSim.cs ~274-277. NIGHT15's DRYDOCK AUTOPILOT: 99 kills in 300 s [M: `SimHarness newmap`, NIGHT15-LOG] = 0.8 s frozen per minute on average; the ledger (section 3.2) [M: SimHarness impact]: 126 kills in 300 s on the arena, 5.0 s frozen, with no cap on a burst. | In a horde, a burst of kills is a burst of 40 ms stops: a stutter, not a punch. |
| 6 | **Every swap is the same drop-and-raise.** 0.25 s; the gun is down for 15 steps of 30 (0.125 s), fire opens at step 26 (0.217 s); 0.092 s of visible raise before it can fire, then 0.033 s more. No per-gun draw; the holster drops straight down. | [C] Combat.cs StepSwap ~899-925 (total `StepsFor(0.25)` = 30, lower `round(30 x 0.5)` = 15), CombatMath.SwapCanFire (fire when (t - 15) / 15 >= 0.7: t >= 25.5); Viewmodel.cs ~701-717. | Six guns, one gesture: nothing tells you which gun arrived or that it's ready. |
| 7 | **No fist.** The quick punch (F) shows nothing on the left: the web's `viewmodel.arm` isn't ported. | [C] Viewmodel.cs header ("Not ported: the left arm's punch"); PORT-STATUS Player row. | A punch that kills shows no hand: the most physical verb has no picture. |
| 8 | **The cooldown-ready glint and `weaponReady` aren't ported; nor is the web's FOV kick stack** (a multikill widens 1.5 deg, a melee hit narrows 1.6 deg, a rank-up widens). | [C] Viewmodel.cs header; CameraFeel.cs:10; the web's player.js :166-212, viewmodel.js ~850. | Slow guns give no "ready" cue; multikills don't breathe. |
| 9 | **The hotbar cuts names mid-word** ("KESTR", "PULSE"). | [S] Logs/shots/n16-before/n15-drydock-hub.png; [C] UiHotbar.cs:170-177 (`HudHotbarGlyphChars` 5). | Reads as a bug in the first second of every run. |
| 10 | **The style plate greets a new run with "D / DULL"** and an empty bar, mid-right, before any style exists. | [S] n15-drydock-hub.png; [C] Ui.cs ~320. | A big zero-state plate whose first word to a new player is DULL. |
| 11 | **Two first-run toasts, two type treatments** (condensed vs wide-tracked), nearly touching. | [S] n15-drydock-hub.png (top left). | Looks unfinished. |
| 12 | **TITLE's MAP group has no sub-line** (SURVIVAL, DAILY, HEAT each have one). | [S] Logs/shots/n16-before/n15-title.png; HANDOFF section 6. | The one new choice on TITLE is the one not explained. |
| 13 | **The pad wheel's disc lets world labels show through; nothing teaches hold-Y.** | HANDOFF section 6 (NIGHT15's capture n15-pad-wheel.png). | Pad players don't find the wheel; when they do, it's noisy. |
| 14 | **Nothing near the crosshair says what movement is spent.** The 3 dash pips sit bottom-left under the HP bar. | [S] n15-drydock-hub.png (three cyan bars bottom-left); [C] config `hud.crosshairDashSpread`. | A limit you can't see feels like a bug: the slide charge will need a place the eye already is. |
| 15 | **A headshot's hitmarker differs by size only** (`hud.headshotScale`), not by shape. | [C] config `hud.headshotScale`; hud.js grammar (Ui.cs). | Colour- and size-only cues fail at speed and for colour-blind players. |
| 16 | **Menus cut, never fade.** TITLE to the run, DOWN to RESULTS, MODS open: hard cuts. | [C] Ui.cs screen switching (`shownFlow`), no CanvasGroup fades. | Fine for speed, but RESULTS lands like a glitch. |

**Judged and left alone (with the number):**
- **Dash spam:** 3 pips, `dash.regenPerSec` 0.7, a dash is 24 m/s for 0.25 s (6 m) [C]. Mashing it can't beat walking for
  long: after the first 3 pips, one dash per 1.43 s. Whether mashing beats the skilled dash-jump (21.2 m) is the spam
  table's [pending: `SimHarness spam`]; NIGHT16.md rule (e): change only if it does.
- **The wall-jump cap:** 3 per airtime (`wallJump.maxPerAirtime`) [C], refreshed by landing; a wall-jump chain needs two
  walls and timing. Not spam today [pending: the spam table's wall-jump chain row].
- **The slam-boost:** a slam is `slam.speed` 48 m/s down, the boost jump within `boostJump.window` 0.15 s of landing
  (x1.8) [C]: it's a timing skill already; the spam table checks a mashed chain.
- **The camera's landing and slide:** the web's NIGHT5 tuned them (landing dip back to 0 with no second dip, the slide drop
  with no overshoot: the web's FEEL-TARGETS "Movement camera" table) and the port is line for line (PORT-STATUS NIGHT12)
  [C]. No change.
- **The buy prompts:** hud.js's buyLabel with FREE / OWNED (NIGHT11) [C]; not judged tonight beyond the tour (no defect
  seen).

## 2. The research (tagged)

A read-only research agent, 14:38-14:49 (30 min cap). **VERIFIED** = read in the cited source (quote given there);
**UNVERIFIED** = not confirmed. The ULTRAKILL wikis are community-written (wiki.gg is the "Official ULTRAKILL Wiki",
fandom the older fan wiki), pulled through the MediaWiki API.

**2.1 ULTRAKILL's freezes and slow-downs**
- Hitstop stops time and the music. VERIFIED: ultrakill.fandom.com/wiki/Hitstop.
- A **major hitstop is 0.25 s**: parries / interruptions, punching a cannonball, shooting a core / rocket / cannonball,
  collecting soul orbs, the Jackhammer on cores and rockets. VERIFIED: the same page, citing the game's
  `ParryFlash()`.
- A **minor hitstop is "less than a quarter of a second"** (no number): Feedbacker / Knuckleblaster punches, piercing rail
  shots, Alternate Revolver hits, rockets hitting an enemy, punched projectiles. VERIFIED: the same page.
- **The Jackhammer scales with V1's speed:** green ~0.05 s, yellow 0.2 s, red 0.5 s; a projectile launch freezes 0.2 s.
  VERIFIED: ultrakill.wiki.gg/wiki/Impact_Hammer; ultrakill.fandom.com/wiki/Jackhammer.
- Enemy deaths and the Whiplash pull appear in no hitstop list. UNVERIFIED (not checked further).
- **The look stays live during a hitstop, and it was made so on purpose.** VERIFIED: fandom Hitstop ("The player can
  change the view during the hitstop"); wiki.gg changelog Patch 15c ("Camera can be rotated during a hitstop").
- **Inputs act during a hitstop.** VERIFIED: weapon switches, coin throws, hammer pumps and punches (wiki.gg Revolver;
  fandom Jackhammer: "During the full speed hitstop, V1 can still punch; this is likely unintended").
- **The level timer pauses during a hitstop.** VERIFIED: wiki.gg Ranking.
- Whether the **viewmodel freezes** with the world: not found. UNVERIFIED.
- **Slow-mo** is documented only for the weapon wheel (time slows while it's open) and an "arena end slowdown". VERIFIED:
  wiki.gg Weapon_Wheel; the Patch 7 changelog. No factor or duration found. **Slow-mo after a parry:** not found
  (UNVERIFIED; probably none).
- **A parry freezes and flashes the screen white**; an option ("Parry Screen Flash", on by default, added in Patch 14)
  turns the flash off but **not the freeze**; some punched projectiles freeze without a flash. VERIFIED: wiki.gg Parrying,
  Options, Arms. Players asked for a hitstop-off option on Steam; no developer reply. VERIFIED (Steam thread
  7434949789832999894). There is a Screenshake slider and a per-action rumble slider. VERIFIED: wiki.gg Options.

**2.2 Hitstop theory**
- **Sakurai** (Famitsu vol. 490, sourcegaming.info): both sides freeze for the same time, longer for more damage;
  slow-mo is "a separate concept"; free-for-alls keep hitstop short (a third player strikes the frozen pair); projectiles
  get less; there is a cap; the struck character vibrates, starting large and shrinking; the attacker keeps creeping
  forward imperceptibly. VERIFIED.
- **SmashWiki** (ssbwiki.com/Hitlag): Ultimate's hitlag is `floor((d x 0.65 + 6) x ...)` frames (10 % damage: 12 frames,
  0.2 s); the cap is 30 frames (0.5 s) from Brawl on; only the pair freezes, other objects keep moving; Ultimate cuts hitlag
  above 10 frames by player count. VERIFIED (community wiki).
- **Vlambeer** (Jan Willem Nijman, "The Art of Screenshake", 2013, ~18:02-18:50): "when I hit an enemy, the game will
  pause for 20 milliseconds or something ... And also, when they die"; Nuclear Throne needed an option to turn the shake
  off. VERIFIED (machine transcript). A blog's "0.2 s" is wrong.
- **Eiserloh** (GDC 2016 slides): trauma in [0, 1], +0.2 / +0.5 per hit, linear decay, shake = trauma^2 or ^3, rotation
  only in 3D, Perlin noise (works through pause and slow-mo), "Camera shake is like salt". VERIFIED. (The port already
  has fx/trauma.js: exponent 2, decay 2.5 / s.)

**2.3 Keeping the slide a skill**
- **Titanfall 2:** the slide's start boost is on a **2 s timer**, "preventing players from spamming slides". VERIFIED
  (secondary: avidachievers.com speedrun guide). Titanfall 1 (Respawn, gamedeveloper.com): the slide "gives you a little
  boost when you start it" and enables slide-hopping. VERIFIED.
- **Apex Legends:** "Slide itself has no timer. But the Slideboost has a **2 sec cooldown, from the last time you entered a
  Slide**"; the boost is capped; jump height fatigue after landing (17 units for 0.15 s, back to 55 at 0.8 s). VERIFIED
  (apexmovement.tech, community).
- **Call of Duty:** MWIII's launch added a delay after sliding to the Tac Sprint refresh; a week later it was cut by 75 %
  ("the previous delay was too aggressive ... without making slide and Tac Sprint repetition the only viable choice"); BO6
  S02 / S03 trimmed the slide's initial speed and duration (max speed scale 1.6 -> 1.55, max time 0.7 -> 0.65 s). VERIFIED
  (callofduty.com patch notes). MW2019 / MWII slide-cancel history: UNVERIFIED.
- **ULTRAKILL:** the slide is a constant 24 u/s with zero acceleration; a faster entry keeps its speed and **decays by
  friction back to 24**; stamina doesn't regenerate while sliding; the Super Slide Jump needs a jump within 3 frames of the
  slide ending (capped at 100 u/s in Patch 15). VERIFIED (ultrakill.wiki.gg Movement).
- **Neon White:** movement is paid for with the gun card (discard it for a dash). VERIFIED (Engadget, Ben Esposito).

**2.4 Weapon switches**
- DOOM Eternal: no swap-time number and no id quote on animation lock found (UNVERIFIED); update 1.06 (2020-08-04) added
  a quick-switch radial. VERIFIED (gamerevolution.com summary).
- ULTRAKILL: "shotgun cancelling now takes 12 frames instead of 8" (Prelude v1.02); the Alternate Revolver draws fast but
  plays a slower draw "if the gun has been fired within the past 2 seconds" (Patch 5). VERIFIED (wiki.gg changelogs).

**2.5 Impact frames and first-person hands** (thin)
- Impact frames: "Usually monochromatic or otherwise chromatically stylized drawings hidden within sequences". VERIFIED
  (sakugabooru glossary). Hi-Fi Rush / Sifu frame counts: not found (UNVERIFIED).
- Mirror's Edge (Tobias Dahl): "animate what you perceive and not exactly how things move in real life". VERIFIED (awn.com).
- Titanfall 1: sliding cants the gun ~45 deg down; wall-running, the free hand "caressing the wall". VERIFIED
  (gamedeveloper.com interview).

**2.6 Accessibility**
- WCAG 2.3.1: nothing flashes more than three times in any one second (or stays below the thresholds). VERIFIED (w3.org).
- XAG 118: failures are > ~3 flashes a second over >= ~20 % of the screen; a saturated red flash has a stricter rule.
  XAG 117: avoid camera shake / bobbing / motion blur or give an off option. VERIFIED (learn.microsoft.com).
- Game Accessibility Guidelines: name the effect ("screen flash effects"), don't label options "epilepsy safe". VERIFIED.
- No guideline names hit-pause or slow-mo; the nearest precedents are ULTRAKILL's flash-off option and XAG 117's off
  switches. (Not found.)

**2.7 What the research changed in section 3**
- **The look stays live** in every freeze and slow-mo (ULTRAKILL made it so on purpose): confirmed as designed.
- **The viewmodel's animation freezes with the world; its look sway follows the live look** (no source either way; the
  web's split already does this: the look sway on frame time, every other layer on WorldDt). Decided.
- **The kill tier stays the web's 0.04 s** (Vlambeer's ~20 ms on a hit and a death; ULTRAKILL's minor hitstops are under
  0.25 s); the budget is stricter than the research's suggestion (ours: 0.12 s a second; the agent's: 0.25 s with the parry
  exempt), because a horde round kills faster than any of those games.
- **The parry tier is ULTRAKILL's 0.25 s**; the perfect parry's 0.30 s + slow-mo tail is ours (no source has a slow-mo after
  a parry: it's a design choice, behind SLOW-MO).
- **The screen flash:** white, never red (XAG 118's red rule, and red-orange is the enemies' colour), at most 3 a second,
  on the parry tier and the headshot kill only, x FLASH INTENSITY (0 = off, as ULTRAKILL's option, while the freeze stays).
- **The slide charge:** Apex / Titanfall 2 time the boost **2.0 s from the last slide entry**; our starting rule (1.0 s
  after the slide ends) is about the same for a short slide and longer for a held one; the spam table decides between them
  (sent to builder M).
- **The flat-ground decay** follows ULTRAKILL's idea (a faster entry decays back to the base) but to a walk, not to a
  constant slide speed, because SKYGRIND's slide floor is 1.45 x the walk (ULTRAKILL's slide is its own base).
- **A free hand on the wall during a wall-ride** (Titanfall) joins the polish queue for the left arm.

From earlier nights' research (already tagged there):
- ULTRAKILL: a major hitstop of 0.25 s on a parry; the Jackhammer 0.2 / 0.5 s by speed; options: screenshake 0-200 %, a
  flash toggle, game speed 50-100 % (ZOMBIES-ULTRAKILL-RESEARCH.md 2.2 item 6, [wiki]).
- Sakurai: hitstop scales with the impact and is capped (DESIGN-RESEARCH.md row 4, [R], sourcegaming.info Famitsu vol.
  490).

## 3. The design (targets as numbers, each with its instrument)

### 3.1 Movement: the slide becomes a skill (design.move, the one gameplay change; each rule has a flag and a LAB row)

| Rule | Starting values | Why |
| --- | --- | --- |
| Slide charge | one charge; back `rechargeTime` 1.0 s after the slide ends; an uncharged slide keeps its entry speed (no burst, never a loss); the press always slides | the burst becomes a decision (Titanfall 2 / Apex's slide boost cooldown: section 2) |
| Flat-ground decay | after `sustainTime` 0.6 s the floor eases to `ground.walkSpeed` over `decayTime` 0.5 s; downhill untouched | a held slide stops being a stance; ramps still pay |
| Slide-hop falloff | each hop within `chainWindow` 1.0 s of the last landing: x 1.2, 1.1, 1.0, then 1.0; the 24 cap stays | the first well-timed hop pays in full; mashing doesn't compound |

**Retuned by the instrument (builder M, `274cf29`):** `slideCharge.rechargeTime` 1.0 -> **2.0** s (with 1.0 a slide
timed to the recharge reached 1.19 x walk); `hopFalloff.step` 0.1 -> **0.15** (multipliers 1.2, 1.05, 1.0: with 0.1 the
chain peaked at 15.3 m/s); `sustainTime` stays 0.6 (0.3 meets every row but cuts the first slide-jump to 7.87 m, -12 %,
breaking (d)). The charge counts **after the slide ends** (measured against Apex / Titanfall 2's "2 s from the entry",
`spam --charge-from-entry 2.0`: taps every 2.0 s gave 1.19 x, so that semantics fails (a)).

The spam table [M: `dotnet run --project Tools/SimHarness -c Release -- spam [map] [--off]`, builder M 14:40-15:00; "x
walk" = the mean speed over the window against walking's; the plane 10 s windows, the arena / DRYDOCK cut at their longest
clear flat straight, 70 / 65 m]:

| Row | Plane BEFORE | Plane AFTER | Arena BEFORE / AFTER | DRYDOCK BEFORE / AFTER |
| --- | --- | --- | --- | --- |
| slide tapped every 0.15 s | 1.32 x (peak 11.6, end 10.5) | 1.00 x | 1.24-1.32 / 1.01-1.06 (taps 0.15 .. 2.0 s) | 1.25-1.32 / 1.01-1.06 |
| tapped every 0.3 s | 1.28 x | 1.01 x | (above) | (above) |
| tapped every 0.5 s | 1.25 x | 1.01 x | (above) | (above) |
| tapped every 1.0 / 1.6 / 2.0 s | 1.24 / 1.24 / 1.23 x | 1.02 / 1.03 / 1.04 x | (above) | (above) |
| slide held 10 s | 1.45 x (11.6 / 11.6) | 1.04 x (end 8.0 m/s) | 1.45 / 1.06 | 1.45 / 1.06 |
| slide-hop chain from walk speed | 2.90 x, peak 24.0 | 1.82 x, peak 14.6 | 2.62 (24.0) / 1.81 (14.6) | 2.59 (24.0) / 1.81 (14.6) |
| a slide timed to the recharge (slide 0.6 / 1.1 s, walk the recharge) | n/a | 1.11 / 1.14 x | - / 1.14 | - / 1.13 |
| one slide-jump, then bunny hops | 1.74 x | 1.74 x | | |
| dash mashed / dash on each pip / dash-jump on 2 pips | 1.40 / 1.35 / 1.86 x | unchanged | | |
| slam-boost chain | 0.21 x | 0.21 x | | |
| wall-jump chain (a 7 m canyon) | 1.07 x | 1.07 x | | |

| Target | Instrument | Before | After |
| --- | --- | --- | --- |
| (a) slide tapped every 0.15 / 0.3 / 0.5 s, 10 s on flat ground: <= 1.1 x a walk's distance | `SimHarness spam` | 1.25-1.32 x [M] | **met**: 1.00-1.06 x on every world, taps 0.15 .. 2.0 s [M]. Not met for a slide *timed* to the 2 s recharge (1.11-1.14 x: one burst per 2.6-3.1 s at 0.4 inputs a second: a rhythm, not spam): **a question for GG** (a 3 s recharge reads 1.09 / 1.12) |
| (b) slide held 10 s on flat ground: <= 1.1 x | `SimHarness spam` | 1.45 x [M] | **met**: 1.04-1.06 x [M] |
| (c) a slide-hop chain from walk speed peaks <= 15 m/s | `SimHarness spam` | peak 24.0 m/s [M] | **met**: peak 14.6 m/s on every world [M] |
| (d) skilled lines within 5 %: dash-jump 21.2 m, wall-ride climb 7.6 m, first slide-jump 8.9 m, DRYDOCK slipway 28.8 m/s, LAUNCH RAIL 3 of 6 | `spam`'s skilled section (the web's recordings and the verbs scenarios with the rules on); `SimHarness verbs` | as listed [M, NIGHT15] | **met, all unchanged**: 21.20 m, 7.61 m, 8.93 m, 28.81 m/s, 3 of 6 [M] |
| (e) dash / wall-jump / slam: change only if mashing beats skill | `SimHarness spam` | dash mashed 1.40 x < dash-jump 1.86 x; wall-jump mashed 1.07 x; slam-boost 0.21 x [M] | **no change**: mashing never beats the skilled line [M] |
| The AUTOPILOT still clears DRYDOCK | `SimHarness newmap Design/maps/drydock.json 300` | NIGHT15: round 6, cleared 5, 99 kills [M] | `newmap PASS ... round 8 / 8, cleared 7, 148 kills ... 60 of 60 hashes equal` [M, rules on] |
| Feedback: the charge visible where the eye is | the HUD at the crosshair (U3-29.7), `slideCharged`, the tick's pulse on return | none | built (`7bcf0f6`); [pending: capture] |

### 3.2 Impact: tiered freezes, a little slow-mo, a budget (design.impact; Game layer only)

| Moment | Freeze (s) | Slow-mo |
| --- | --- | --- |
| Kill (the web's) | 0.04 | |
| Heavy body hit (rocket direct, rail, shotgun 6+ pellets in one body) | 0.03 | |
| Headshot | 0.05 | |
| Headshot kill | 0.08 | |
| Punch or gauntlet hit (the momentum hit's 0.045 a floor) | 0.06 | |
| Punch kill | 0.09 | |
| Parry | 0.25 | |
| PERFECT PARRY | 0.30 | 0.35 x for 0.25 s |
| The round's last kill | (its kill tier) | 0.3 x for 0.5 s, easing back over 0.3 s |
| A multi-kill of 3+ from one shot or blast | (its kill tier) | 0.5 x for 0.25 s |

| Target | Instrument | Result |
| --- | --- | --- |
| Frozen time <= 0.12 s in any rolling second, <= 6 % of real time over a minute, on both maps' 300 s AUTOPILOT runs | `SimHarness impact` (the ledger) | **met** [M, builder I]: arena 126 kills: kill 109 x (4360 ms, 2 skipped), headshotKill 11 x (760 ms), 4 cut to the budget; 997.9 ms frozen per minute (1.66 %); worst rolling second 120.0 ms (the cap binds), worst minute 1400 ms; 6 last-kill slow-mos. DRYDOCK 99 kills: kill 91 x (3640 ms), heavyHit 1 x (30 ms), headshotKill 4 x (240 ms), 2 cut; 766.2 ms per minute (1.28 %); worst second 120.0 ms, worst minute 1480 ms; 5 slow-mos. `impact PASS: the budget holds on 2 map(s)` |
| Before tonight (the web's 0.04 s per kill, no cap) | the same ledger's runs | [M]: the arena's 126 kills = 5.04 s frozen in 300 s (1.0 s a minute) with nothing capping a burst: the worst second was uncapped (the cap now cuts 4 requests on the arena, 2 on DRYDOCK) |
| A kill freeze at most every 0.1 s; requests over budget skipped and logged | ImpactRulesTests; the ledger's skipped column | **met**: ImpactRulesTests 15 / 15 in the Editor (15:00); skipped / cut columns above |
| Slow-mos never stack; at most one start per 3 s; none while DOWN, paused or in a menu; none with REDUCED MOTION or SLOW-MO off | ImpactRulesTests | **met** (the tests); the AUTOPILOT never punched, parried or multi-killed in the ledger runs: those tiers are covered by the tests only |
| No eaten input: a jump pressed and released inside a freeze still jumps; the mouse look stays live | the latch test; the strip | **met**: ImpactLatchTests 1 / 1 in the Editor (15:00); the look: the loop polls the devices before ImpactBeginFrame |
| A hard cap on one freeze | design.impact.budget.maxFreeze | 0.5 s (Smash's 30-frame cap, section 2.2) |
| **After the review** (finding 4: a 3-hitch chop on automatic headshots and in a horde; finding 8: parry chains): no second with 3 freeze starts; a horde's kills never swallow the moment after them; a parry chain under 30 % frozen | `SimHarness impact` (the rhythm line); ImpactRulesTests 24 / 24 | `design.impact.cooldown.any` **0.5 s** between any two freeze starts (0.25 s still let 3 fit in a second: measured by builder I); a strictly higher tier may start inside it (once per gap, kill < heavyHit < headshot < headshotKill < punchHit < punchKill); the non-kill headshot / heavy tiers on a shot's first hit only; parries cooldown 1.25 s, a repeat inside it the web's 0.06 s (budgeted). Ledger (15:57): arena 1031.7 ms frozen / min (1.72 %), worst second 120.0 ms, at most 2 starts in any second, the shortest gap 0.437 s (a bypass); DRYDOCK 1074.5 ms / min (1.79 %), 120.0 ms, 2 starts, 0.500 s; `impact PASS`. **For GG:** 0.5 s means at most two kill freezes a second in a horde: the rest of a burst lands without a stop |
| What a freeze shows: the world stopped, the struck enemy's flash held, the hitmarker on the first frame, a 1-2 deg FOV punch-in (non-melee moments; melee keeps the web's 1.6 deg lunge), a low-alpha flash on parry / perfect parry / headshot kill only (x FLASH INTENSITY, <= 3 a second), a rumble pulse | strips: headshot kill, punch kill, parry, last kill | build 1's strips (`Logs/shots/n16-b1-strips/`): the freeze held (HELD runs: headshot kill ~80 ms, punch kill all 78 ms seen, perfect parry 92 ms of 300 seen), the struck husk white through it, the headshot kill's brightening on frame 1, the parry's white flash. **Wrong:** the left fist hidden through a punch's freeze (V's fix) |
| The composed fingerprint EQUAL with every impact feature at FULL | `-skygrindFingerprint` | **met**: build 1 `EQUAL to the baseline` (the automation runs at FULL) |
| The viewmodel freezes with the world (it runs on WorldDt: the web's split) | [C] Viewmodel.cs header; the strip | decided: freezes (section 2 asks what ULTRAKILL does) |

### 3.3 The hands

| Target | Instrument | Result |
| --- | --- | --- |
| Every gun at its fire-ready pose (within 0.5 deg and 2 mm of hold) by step 26 of the 30-step swap (the sim's fire gate) | HandsTests | **met** (`EveryGunIsFireReadyByTheStepTheSimOpensFire`, 6 guns at x1 and QUICK's x2: step 13 of 15): HandsTests 19 / 19 in the Editor (15:03). Before: the web's curve left the gun ~6.1 deg and 53 mm low on the fire step [M, builder V]; the raise is remapped to be at hold on it |
| A shot mid-draw cancels the flourish the same frame | HandsTests | **met** (`AShotMidDrawCancelsTheFlourishTheSameFrame`; a swap out mid-flourish doesn't pop: `ASwapOutMidFlourishDoesNotPop`, red at 7.00 deg first) |
| No viewmodel pixel inside the centred 10 % x 10 % box on any draw frame | HandsTests | **met** for the 6 kit meshes and the arm's punch / wall poses (every triangle projected at viewmodel.fovDeg 54, 16:9, 144 fps; a control proves the projection live); not covered: the gems, the muzzle flash, the sweep band |
| Five signature draws (KESTREL cylinder spin + flick; THUNDERCLAP pump rack; COMET heavy lift + shoulder settle; LONGWAVE charge sweep; GAUNTLET fist clench + gem flare); the holster tips to the hip | strips (`-skygrindStrip swap`, `draw`) | **seen** in build 2's `n16-strip-swap.png` / `n16-strip-draw.png` (2x crops): the holster tips to the right hip; KESTREL flicks and settles; THUNDERCLAP's pump shifts through the settle; COMET rises rolled onto the shoulder and seats by ~+330 ms; LONGWAVE's sweep runs bright along the barrel; the GAUNTLET's gem flares white. **Wrong:** the GAUNTLET sits low under the HUD's weapon plate, so its clench barely reads (its hold pose is the web's `viewmodel.kitOffsets`) |
| The left arm's punch from the lower left (the web's viewmodel.arm), knuckles flash on a parry (white on a perfect one) | strips (`punch`, `parry`) | **seen** after the fix (`c1e301e`): build 2's punch and parry strips: the fist in from frame 3, full by frame 6 (+43.5 ms), held through the freeze. Build 1 showed none (the arm froze at age 0) |
| The glint + `weaponReady` when a slow gun's cycle ends | the bus; a strip | [pending] |
| Zero allocation per frame; +4 viewmodel draw calls at most at rest | the test; the frame debugger counts | **met**: `TheDrawFrameAllocatesNothing` (Unity's AllocatingGCMemory, Editor) passes; no draw added at rest (the arm, the knuckle plate and the band draw only while they show) [C, builder V] |

### 3.4 The UI

| Target | Instrument | Result |
| --- | --- | --- |
| The 3 dash pips and the slide charge as cyan ticks under the crosshair, shown while spent, faded 1 s after all are full | FeelHudTests; a capture | **met**: FeelHudTests 12 / 12 (the show / hold / fade clock); build 1's slide strip (a 2x crop: the 3 pip ticks and the longer slide tick, dimmer once spent). Still wrong: faint at 1080p |
| A headshot marker distinct by shape | a capture | built (a hollow diamond at the marker's centre, `7bcf0f6`); [not yet seen in a strip: the freeze strip's framing hides the centre] |
| The wheel's disc on a dim backdrop; TITLE's MAP sub-line; a hold-Y hint for pad players | captures | MAP sub-line **met** (build 1's TITLE: "A SHIP'S RIBS, RAMPS AND A MAST"); the backdrop and the hint built (`cd1abbd`, `1e1809d`); [no pad capture yet] |
| The hotbar never cuts a word; the style plate hidden until the first award; one toast treatment | captures (the n15 tour's three worst) | **met** (build 1's hub: KESTREL / PULSE; one toast type, a 14 px gap; the rank hidden: its empty skewed box fixed after, `cb14e82`) |
| TITLE to the run, DOWN to RESULTS, MODS open: 0.15-0.25 s fades that never delay input | a capture pair; a press during a fade acts at once | built (`7bcf0f6`: CanvasGroup alpha only, every screen live from its first frame); [not yet seen in a strip] |

## 4. What we will not do, and why

- **No animation lock, no eaten input, no added latency** (pillar 1): every draw, freeze and slow-mo follows the sim's
  timers and never gates them; a press inside a freeze reaches the next step through the input latches; the look stays
  live. A draw that wants to hold the gun down longer than the sim's swap is cut, not the sim stretched.
- **No new gameplay numbers beyond the slide** (rule 4): `weapons.swapTime`, the guns' damage and timers, the enemies, the
  rounds stay. The freezes are time and pictures only; the slow-mo changes how many 120 Hz steps run per real second,
  never what a step does.
- **No gore that stays** (D-29): an enemy's effects are its hit flash and the dissolve.
- **No new packages, no Animator controllers** (D-30, rule 6): the viewmodel stays code-driven springs and curves.
- **No audio** (GG's): each new moment emits an event for GG's audio map instead (`impactFreeze`, `slowMo`,
  `slideCharged`, `weaponDraw`, the web's `weaponReady`).
- **No enemy design changes** (GG's): no new enemy reactions to freezes beyond the existing flash.
- **No freeze on the player being hurt**: a stop when you're hit takes control away (pillar 1); the hurt has its
  vignette and arc.
- **No slow-mo on every kill or in menus**: slow-mo is rare by design (at most one per 3 s) or it stops reading as a moment.

## 5. The ranked polish queue

Score = impact (1-5, what a player feels) x confidence (0.5-1) / cost (S 1, M 2, L 3). Seeded from NIGHT16.md U3-29.8,
re-ranked by the audit. Taken in rank order after the builders land, one commit each.

| Rank | Entry | Impact | Confidence | Cost | Score | Owner / note |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | The multikill FOV punch + the melee lunge + the rank-up widen (the web's fovKick stack) | 3 | 1.0 | S | 3.0 | builder I (assigned 14:41) |
| 2 | The struck enemy held white through the freeze, then the dissolve | 4 | 0.75 | S | 3.0 | lead: EnemyViews' flash under WorldDt 0 (check first: it may already hold) |
| 3 | The dash pip's refill blink (the web's `hud.pipFlashTime`) on the crosshair ticks and the HP plate | 3 | 0.9 | S | 2.7 | lead / U's HUD |
| 4 | Landing dust ring scaled by fall speed | 3 | 0.8 | S | 2.4 | lead: MovementFx (land puffs exist: scale them) |
| 5 | Slide sparks / dust from the heels scaled by speed | 3 | 0.8 | S | 2.4 | lead: MovementFx |
| 6 | A quick-swap flick (two swaps in 0.4 s skip the second holster visually) | 3 | 0.7 | S | 2.1 | V's follow-up |
| 7 | A cyan flick at the feet / on the viewmodel when the slide charge returns | 3 | 0.7 | S | 2.1 | after M; V or lead |
| 8 | Menu focus movement and a button press scale (UiFeel) | 2 | 0.8 | S | 1.6 | lead |
| 9 | The round-clear beat: the slow-mo + the banner + a soft vignette | 4 | 0.75 | M | 1.5 | lead after I lands |
| 10 | A dash-start smear frame (x EFFECTS INTENSITY) | 2 | 0.6 | S | 1.2 | lead: MovementFx / PostFx |
| 11 | Wall-ride sparks at the contact point | 2 | 0.6 | S | 1.2 | exists (MovementFx port-only sparks, NIGHT12): check, maybe drop |
| 12 | A hold-progress ring on BUY | 3 | 0.7 | M | 1.05 | lead (Ui) |
| 13 | Shotgun shells ejected on the pump | 2 | 0.5 | S | 1.0 | exists (WeaponFx port-only shells, NIGHT12): check timing vs the pump, maybe drop |
| 14 | An idle flourish (8 s without input in a break: the revolver spins, the pump is checked; never in a round) | 2 | 0.7 | M | 0.7 | V's follow-up |
| 15 | A perfect parry's one-frame "impact frame" in an inverted dusk palette (x FLASH INTENSITY, <= 3 a second; judged against pillar 3's readability first) | 3 | 0.4 | M | 0.6 | after the research on Hi-Fi Rush / Sifu |
| 16 | The left hand on the wall during a wall-ride (Titanfall, section 2.5), pushing off on a wall-jump; a slam wind-up | 2 | 0.6 | M | 0.6 | V's movement layers |

Each entry taken gets a commit, its strip / capture / measured line and a two-line review in NIGHT16-LOG.md; each left
gets its reason at 5:00.

**The queue's state** (NIGHT16-LOG.md has each entry's evidence and review):

| # | Entry | State | Commit / evidence |
| --- | --- | --- | --- |
| 1 | The web's FOV kick stack | taken (builder I) | `1977084`; ImpactFrames ports player.js :166-212 |
| 2 | The struck enemy held white through the freeze | exists by construction | EnemyViews' flash on the world dt (0 in a freeze); seen in the freeze strips (b1, b3) |
| 3 | The pip refill blink (the web's `hud.pipFlashTime`) | taken (builder U) | `7bcf0f6`; FeelHudTests |
| 4 | Landing dust ring by fall speed | taken (the lead) | `d3a1b29`, `9a9669c` (alpha); still not clearly seen: build 4's land strip (re-aimed `3f54cf7`) lands on a SHOCK PLATE pad whose pale disc fills the frame |
| 5 | Slide heel dust by speed | taken (the lead) | `d3a1b29`; behind the camera in first person by design |
| 6 | Quick-swap flick | taken (builder V) | `715d9e9`; HandsTests (no pop, fire-ready, centre box) |
| 7 | The slide charge's return cue | taken (builder U: the tick's pulse) | `7bcf0f6`; a cyan burst at the feet left (out of a first-person view) |
| 8 | Menu focus slide + press dip | taken (the lead) | `8d45924`, `c83955a`, `3868feb` (the dip fixed: finding 11) |
| 9 | The round-clear beat (slow-mo + banner + vignette) | taken (I: the slow-mo; the lead: the vignette) | `8c8fd91`, `cb14e82` (0.07); the MODS draft after it `3868feb`; the slowmo strip (b1) |
| 10 | A dash-start smear frame | left | the dash already has three cues (the FOV kick, the edge tint, the chroma pulse) |
| 11 | Wall-ride contact sparks | exists | MovementFx (NIGHT12, port-only) |
| 12 | A hold ring on BUY | exists | the web's prompt bar filled by `buyables.Progress` (Ui.cs:1244) |
| 13 | Shells on the pump | exists | WeaponFx (NIGHT12) ejects one at `weapons.shotgun.pumpDelay` |
| 14 | Idle flourish | taken (builder V) | `e31aae9`; HandsTests |
| 15 | A perfect parry's inverted impact frame | left | inverting the dusk palette maps cyan toward red (danger only, CLAUDE.md rule 8); a second full-screen flash; one frame is 7 ms at 144 fps |
| 16 | The hand on the wall, the push-off, the slam wind-up | taken (builder V) | `1c683c3`; HandsTests |
| 17 | The slide charge's first-time hint (added by the audit) | taken (the lead) | `3e318c1` |
| 18 | The crosshair ticks legible (added from build 1's strips) | taken (the lead) | `3b212d2` (4 px, a 0.3 track) |
| 19 | Zone signs hidden behind PAUSED (added from build 3's UI pass) | taken (the lead) | `8b709f1` |
| 20 | The held slide's crawl after the decay (added from the review of the rules) | **left for GG** | a second Core rule would move the fingerprint again (NIGHT16 rule 4: once); FEEL-PLAYTEST.md question 1 |
| 21 | The GAUNTLET under the HUD's weapon plate (added from build 2's swap strip) | **left for GG** | its hold pose is the web's `viewmodel.kitOffsets`; raise it or move the plate: FEEL-PLAYTEST.md question 5 |
| 22 | RESULTS' BEST MOMENTS lists a LONGEST SWING a run can't have yet (build 3's UI pass) | left | the web's runMoments line, ported as is |
| 23 | The rank-up camera kick (added from the sweep of the web's feel not yet ported) | taken (the lead) | `2cad6fc`; CameraFeelTests 2 / 2 (written with the code, not seen red first) |
| 24 | The barrier-touch FOV pinch (the same sweep) | left | the web's `barrierTouch` comes from barriers.js, which isn't ported |
| 25 | The crosshair's spread on a dash and after a THUNDERCLAP shot (the same sweep) | taken (the lead) | `3a431b3`; Play check 1: the left arm -18.6 px at 14 ms after a shot, -13.0 (rest) at 200 ms |
| 26 | The rank-up pop: the letter's scale, the name's slide (the same sweep) | taken (the lead) | `2ec7b82`; Play check 1: scale 1.318 (first frame) -> 0.929 at 132 ms -> 1.000 at 255 ms; the name +24.6 -> -3.5 -> 0 px. The name's fade and skew aren't ported |
| 27 | The momentum hit's hold spring (the same sweep) | taken (builder V) | `538ea24`; HandsTests 28 / 33 at the tests-first `3f07413`, 33 / 33 after |
| 28 | The forge flourish on the gun in hand (the same sweep) | taken (builder V) | `538ea24`; the same HandsTests run |
| 29 | The rank-up screen-edge glow (hud.js :183 / :475-476 / :676-677) | left | it glows in the rank's colour: S is #ff4a1c (red-orange: enemies and danger only) and SS #ff2e88 (magenta: RIVAL only), so an S rank-up would light the screen's edge like the hurt vignette (CLAUDE.md rule 8). A neutral colour is GG's call |
| 30 | The "+60" points pops at the crosshair (hud.js :524-535 / :918-932; never ported) | taken (the lead) | `70c102e`; Play check 1: '+66' #19D3E6 22 px, '+100 ROUND' #FFE6A8 30 px, rising 41 px over 0.72 s, alpha 0.17 at 754 ms, none live at 950 ms |
| 31 | The damage vignette (hud.js .hud-damage; never ported) | taken (the lead) | `894a47a`; Play check 2: at 40 % HP the alpha 0.13 at 101 ms, 0.24 at 610 ms (falling as regen began), 0.00 1.2 s after full HP |
| 32 | EXPOSED's pulsing edge (hud.js .hud-exposed-edge; never ported) | taken (the lead) | `eb1963b`; Play check 2: Exposed 128 of 128 frames, alpha 0.00-0.70, #FF280A, sliced; 0.00 1 s after the camp let go |
| 33 | The buy prompt's poor / denied states (hud.js .hud-buy.poor / .denied) | taken (the lead) | `eb1963b`; Play check 2: THUNDERCLAP 750 with 0 points: the prompt #FF9A82, the bar back #FF4A1C; denied: 0 / 2 / 4 / 2 px every ~25 ms, still from 405 ms |
| 34 | Regen: the HP fill's sheen and the REGEN tag (hud.js .hud-hp-fill.regen / .hud-regen-tag) | taken (the lead) | `eb1963b`; Play check 2: the sheen's u 0.158 -> -0.496 then wrapping (a 0.8 s period), the tag 'REGEN' #8FF3FF; both off at full HP |
| 35 | The move row: the speed readout and the wall-jump ticks (hud.js :127-133 / :284 / :329 / :907-912; never ported) | taken (builder U) | `cad255e` red (MoveRowTests 0 / 3), `8dde864` green (3 / 3); seen in build 4's land strip: "0.0 M/S" under the stamina segments, inside the HP plate |
| 36 | The ADS HUD: the ring, the LONGWAVE's scope, the rangefinder, the arms fading (hud.js :74-82 / :752-770; never ported) | taken (builder V) | `418e8f3` red (UiAdsTests 0 / 6), `66dbdea` green (6 / 6; the Ui.cs hook conflict with #35 resolved by keeping both); not seen on screen |
| 37 | The crosshair tag: PRIMED, CROSS / the combo, SWAP (hud.js .hud-tag; never ported) | taken (the lead) | `0140eec`; Editor compile clean; not seen on screen |
| 38 | The charge edge: the LONGWAVE's charge tints the screen's edge (hud.js .hud-charge-edge; never ported) | taken (the lead) | `0140eec`; Editor compile clean; not seen on screen |
| 39 | The style multiplier chip, "x1.2 AIR" by the rank (hud.js .hud-mult) | left | its place is under the rank bar, where the award feed's box also reaches; no capture left to check the overlap |
| 40 | HUD STYLE: the web's default ATTACHED makes the bottom-right block ride the gun's sway (hud.js :818-826, viewmodel.js :930-932); the port has no such row and is always CLASSIC | **left for the next session** | `SettingsView.hudStyle` is loaded and never used, and the row isn't in UiSettings' `Shown` (builder U's sweep). Porting it moves the points and the weapon block (both layers) as one: a mount per layer, each with its own nested Canvas (re-batching), the roll about the block's centre, the Viewmodel's sway + bob exposed. Nothing could run it tonight (Play checks and captures spent) |
| 41 | Particle bursts at the web's sizes (section 6) | taken (builder V) | `0f43369` red (FxGatesTests 0 / 3), `6c334ac` green; blasts / crusher dust 1.6 x, gun and nail sparks 0.35-0.4 (were 1), the impact ring 1.1; not seen on screen |
| 42 | STEADY's softer kick and punch (x `perks.fx.recoil`) | taken (V) | `5a03771`; HandsTests 33 / 34 -> 34 / 34. STEADY isn't buyable in the port yet (`SurvivalSim.cs` :503): no visible change today |
| 43 | REDUCED MOTION stops the port-only enemy flinch / vibration and the THUNDERCLAP shells | taken (V) | `9b2ab75`; FxGatesTests (the gate) |
| 44 | A THUNDERCLAP full miss: no sparks or light in mid-air | taken (V) | `675fd36`; FxGatesTests (the gate: the shotgun, "world", the end at `weapons.shotgun.range`); Game layer only |
| 45 | The spawn beams pulse each on their own (a MaterialPropertyBlock per beam) | taken (V) | `04d6589`; dotnet build only (rendering) |
| 46 | The award feed: "+ NAME n" in the rank's colour; zero-point tricks shown dimmed | taken (builder U) | `216a804` red (SmallUiTests 0 / 6), `2b2e4c5` green |
| 47 | The rank bar's pulse (`StyleMath.RankPulseRate`), off with REDUCED MOTION | taken (U) | `3d3c192`; SmallUiTests |
| 48 | R restarts from PAUSED (menus.js :786-790) | taken (U) | `57d4033`; SmallUiTests (`RestartRule.For`); the same path as PAUSED's RESTART button |
| 49 | RESULTS: "RETRY (R)" (the live key; "RETRY" on a pad) | taken (U) | `ae1cb73`; SmallUiTests |
| 50 | The particle cap by quality (160 on LOW; the oldest recycled first, as particles.js) | taken (V) | `0a94016` red (FxGatesTests 3 / 7), `a89b82a` green (7 / 7) |
| 51 | The impact ring's burst: round(18 x the preset's impact detail) cubes (was scaled by the momentum share: near-threshold waves threw ~0) | taken (V) | `d8f8f01`; FxGatesTests |
| 52 | The melee hit's family sparks (6; the GAUNTLET's cyan, a punch's warm `colors.spark`) | taken (V) | `e6386ea`; FxGatesTests |
| 53 | Swarmer / spitter death bursts at mid-body (husks unchanged) | taken (V) | `d51cc70`; FxGatesTests |
| 54 | A 0.15 s fade-in on every new run and every return to TITLE (the "in" half only: nothing waits) | taken (U) | `89d58db` red (SmallUi2Tests 0 / 3), `dc8935b` green (3 / 3) |
| 55 / 57 | The focused button's label cyan; disabled buttons at 35 % and skipped by the focus (REROLL first) | **ready, not integrated** | `u-small2-n16` `91caa6b`: it changes UiNav's focus, which the Player smoke's virtual-pad steps drive; the final build is the only one left, with no Play check to try it first |
| 58 | OVERLOAD's picture (a stand-in for the web's lightning): volt bursts on the target and each chained enemy | taken (V) | `8403d8f` red (FxGatesTests 7 / 9), `e89647d` green (9 / 9); the numbers are the STITCHER detonate's burst (projectiles.js :306); no flash (D-71) |
| 59 | The muzzle light at the drawn muzzle (viewmodel.js :942-947 muzzleWorld) | taken (V) | `dfc354d`; FxGatesTests (the muzzle lines up on screen) |
| 60 | The mod chips in the perk row ("VAM×2" in the tag's colour) | taken (U) | `233e2e2` red (ModChipTests 0 / 2), `213bdb1` green; rule 8: the fire tag (0xff8c1a) falls back to the text colour |
| 61 | RESULTS' "ROUND N BEATS YOUR BEST (k MORE)" | left | RESULTS is UiResults' tally by default (not Ui.cs' text), and k has no source at RESULTS (the records already hold this run): keep the previous best at DOWN, then one line in the tally (U's note) |
| 56 | PAUSED's YOUR KEYS lists the pad after a pad input | **ready, not integrated** | `u-small2-n16` `99db5e1`: the smoke's pad step drives PAUSED; U flags the card's bottom row may sit near its edge |

## 6. The web's feel still missing from the port (NIGHT16's read-only sweeps, 17:35-17:48)

Two sweeps of the web (read only) against the port at `a427318`:
- builder U: the UI, 43 rows (the HUD rows its own, the menu rows by a read-only research agent, two spot-checked);
- builder V: feel outside the HUD, 32 rows (some by two read-only explorers, the biggest claims re-checked in the code).

Nothing was run. Below are their top rows by impact / cost (S 1, M 2, L 3); both full tables (75 rows, with HUD
STYLE's plan) are in `Reports/NIGHT16-web-gaps.md`. Rows taken tonight after the sweep are marked with their entry.

| Web (file:line) | What the player sees | Port | Impact | Cost | Notes |
| --- | --- | --- | --- | --- | --- |
| fx/particles.js :61 / :135 | Bursts at the web's sizes (blasts 1.6, sparks 0.35-0.4) | the size is dropped (`SurvivalGame.Init.cs` :126 / :133): blasts small, sparks 2.5-3 x too big | 3 | S | taken: #41 (builder V) |
| combat/effects.js :74-96; config `flashShapes` | A muzzle star per gun, in its family's colour | a plain quad, one colour, for every gun | 3 | S | strobes 12 / s on the STITCHER: with the flash rule below |
| (WCAG; port-wide) | Flashes outside tonight's FlashGate: the muzzle, the kill pop, the rail flash, the enemy's white flash | ungated (the web gates none) | 3 | S | GG: which go through `game.flashGate` / FLASH INTENSITY |
| menus.js :198-202, :221 | Mouse hover: the label turns cyan, an edge lights, the button slides 8 px | a colour tint only; no pointer-enter anywhere | 4 | S | a select-on-hover component reusing UiNav's frame |
| tooltips.js :111-122 | A hold key lists YOUR PERKS / YOUR MODS | absent; the key is already read (CombatReader.cs :145) | 3 | S | |
| menus.js :455 | SETTINGS: each row's description under it | one line, the selected row's (mouse players see it only after a click) | 3 | S | |
| menus.js :283, :1146-1154 | A 0.15 s fade on every restart and quit | only TITLE -> run has the curtain | 3 | S | |
| hud.js :185 / :607-612 | The "x1.2 AIR" style chip | absent | 3 | S | entry 39 (its place) |
| hud.js :183 / :676-677 | The rank-up's edge glow | absent | 3 | S | entry 29 (rule 8: GG) |
| player.js :124 / :160 | STEADY's softer kick (x0.7) | computed (`Perks.cs` :63), read by nothing | 2 | S | taken: #42 (V) |
| (port-only motion) | The enemy flinch and the THUNDERCLAP shells ignore REDUCED MOTION | gaps | 2 | S | taken: #43 (V) |
| effects.js :172-189 | A THUNDERCLAP full miss shows no sparks | sparks and a light in mid-air (a port bug) | 2 | S | taken: #44 (V) |
| (port bug) | Spawn beams pulse each on their own | one shared material: every beam shows the last beam's pulse | 1 | S | taken: #45 (V) |
| hud.js :487 / :450-456 | Zero-point tricks dimmed; scoring lines in the rank's colour | dropped / white | 2 | S | taken: #46 (U) |
| hud.js :179-182 / :602 | The rank bar's pulse, faster at higher ranks | absent (`StyleMath.RankPulseRate` ported) | 2 | S | taken: #47 (U) |
| menus.js :786-790 | R restarts from PAUSED | refused outside DOWN / RESULTS | 2 | S | taken: #48 (U) |
| menus.js :1037 | RESULTS: "RETRY (R)" | "RESTART" | 1 | S | taken: #49 (U) |
| main.js :2210-2218; world/*.js | The world answers the rank: neon brightens, a pulse runs along the trims, the horizon warms at S+ | only the bloom follows the rank | 4 | M | rule 8 at S+ (the rank colours): GG |
| tooltips.js :93-109; tooltipText.js | Cards at a perk machine / a wall buy: effect, numbers, price | the prompt is NAME · PRICE only | 4 | M | you can't learn a perk before paying |
| fx/barrier.js :333-400; player.js :203-206 | Touching / shooting a field: a hex flash, ripples, a small FOV pinch | absent | 3 | M | through FlashGate; the pinch off with reduced motion |
| gunFx.js :96-102 | OVERLOAD's lightning burst | the combo fires (`Arsenal.cs` :413-434) with no picture | 3 | M | needs the lightning pool (fx/arcs.js) |
| hud.js :818-826 | HUD STYLE: ATTACHED | absent (no row; always CLASSIC) | 3 | M | entry 40 |
| powerUpHud.js | Power-up chips with draining bars | one centred text line | 3 | M | rule 8 for their colours |
| menus.js :446-448 | SETTINGS sliders | -/+ only (sensitivity: 56 clicks end to end) | 3 | M | |
| config.js :2292-2308 | Rows HUD STYLE, ZOOM toggle, SHAPE CUES, STYLE AURA, SHOW FPS | loaded, not shown, not read | 3 | M | ZOOM toggle and SHAPE CUES are accessibility |
| fx/aura.js | The style aura on the held gun | absent | 4 | L | rule 8 (the rank colours): GG first |

**Gated on gameplay the port doesn't have yet** (they come with it): the other 12 guns' projectiles, fire and rigs; the
ARCLINE's rope in Survival; bosses, affixes and RIVAL's telegraphs; trials; Duel's orbs; STATIC's arcs; the boss bar and
card.

**Cross-cutting (V's notes):** (1) palette rule 8 blocks every web effect that uses the rank colours at S / SS (SAVAGE
`0xff4a1c` is the danger red-orange, SKYGRIND `0xff2e88` is RIVAL's magenta): the aura, the world's S+ tint, the swap
dissolve's edge, the top rank's shimmer; a cyan-family rank ramp for player-side effects is one answer (GG). (2)
`Core/Combat.cs` allocates a new ShotEvent / HitEvent / MeleeEvent per shot, hit or kill (`:765`, `:894`, `:955`, ...),
as does `Arsenal.cs` :275-349: about 12 a second on the STITCHER, against PORT-RULES 6 (no allocation per step). A Core
change: golden and the fingerprint gate it.

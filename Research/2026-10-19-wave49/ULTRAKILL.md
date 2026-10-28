# ULTRAKILL: visual identity and feedback juice, in buildable terms

Research for wave 49, 2026-10-19. Web only; no local code read. Each claim carries its source and a mark:
**V** = Verified (I fetched and read the page), **R** = Reported (search snippet, forum, or a page I could not open).
Wiki pages are community-maintained (ultrakill.wiki.gg is the official-community wiki). Where no source was found I say
"not found" and do not guess.

## 0. The design stance behind the look

- Hakita: "One of the big benefits of the retro visual style is that visuals are very easy to read at a glance." Levels
  keep "the geometry of a level is simple and clean" so the player can read a room at high speed. V,
  https://80.lv/articles/ultrakill-devs-on-the-game-s-mechanics-npc-ai-and-early-access-experience/
- The guiding principle was "focusing completely on the game feel above all else"; enemy AI is "kept purposefully simple
  to make the enemies predictable and easy to keep track of." V, same 80.lv page.
- Hakita on Hell in other shooters: "a pretty dull standardized interpretation of it with a primarily orange color
  scheme, brimstone and fire". The prelude plays on that cliche on purpose. V,
  https://intothebluesky.com/2020/12/19/interview-with-arsi-hakita-patala/
- A New Blood developer on Steam: "The graphics are PS1 style." V,
  https://steamcommunity.com/app/1229490/discussions/0/3002177675804624601
- Built in Unity; "retro-style graphics reminiscent of video games from the early PlayStation consoles." ULTRA_REVAMP
  (Feb 2025) gave "new graphics for most layers". V, https://en.wikipedia.org/wiki/Ultrakill
- ULTRA_REVAMP: "lighting has been reworked", "Environments boast greater detail", "some models have been reworked from
  the ground up". V, https://readwrite.com/ultrakill-sees-graphics-overhaul-in-ultra-revamp-update/ . Destructoid's
  subtitle: "Way blockier, but also way nicer"; the devs used the time to "REVAMP a lot of the visuals from the previous
  layers to bring them up to par artistically". V,
  https://www.destructoid.com/ultrakill-gets-a-massive-technical-and-visual-revamp-in-new-update/

## 1. Colour and palette per layer

What the sources document is per-level material and light, not a stated per-layer palette. No developer statement or wiki
page gives a layer's dominant hue. Facts found:

- 1-1 (Limbo): rooms "encased in screen walls displaying a false sky"; water and foliage "holographic and artificial";
  stone, torch-lit halls, lit stained glass, a red carpet. V, https://ultrakill.wiki.gg/wiki/1-1:_Heart_of_the_Sunrise
- 2-1 (Lust): the first tower is "illuminated with cold white lamps". V, https://ultrakill.wiki.gg/wiki/2-1
- 4-1 (Greed): "windswept desert", a pyramid "eclipsing the sun", arenas "lit by bonfires and torches", lava streams. V,
  https://ultrakill.wiki.gg/wiki/4-1
- 6-1 (Heresy): a dark upside-down city above "a fiery-red skyline". R (search snippet of the fandom 6-1 page).
- 7-1 (Violence): "a bright white courtyard engulfed by a thick blinding fog"; the screen "will briefly flash white" on
  entry; below it a dark shaft lit only by white orbs. Music titles "The World Looks White" / "The World Looks Red". V,
  https://ultrakill.wiki.gg/wiki/7-1
- Hex codes seen online for "Lust / Gluttony / Greed / Wrath" come from a generic seven-sins palette site, not the game.
  Do not use them. (R, color-name.com, flagged by my search.)

Colour coding against the environment, documented:

- Enemy tells: a yellow spark = parryable, a blue spark = not parryable, each with its own sound. Melee shows a
  "4-point parry spark", ranged an 8-point spark. V, https://ultrakill.wiki.gg/wiki/Parry . Virtues glow pink when
  parryable (R, search snippet of https://ultrakill.wiki.gg/wiki/Virtue ). Players say projectile colour signals speed,
  not parryability (R, Steam threads in the same search).
- Radiant (buffed) enemies: "a scrolling rainbow texture over their bodies, as well as a scrolling rainbow outline". V,
  https://ultrakill.wiki.gg/wiki/Radiance
- The player's kit uses one variant colour set: Blue #3fe8ff, Green #44ff44, Red #ff3d3d, Gold #ffe03d. V,
  https://ultrakill.wiki.gg/wiki/HUD . Arms follow it: Feedbacker "blue-and-black", Knuckleblaster "red-and-grey",
  Whiplash "green-and-grey". V, https://ultrakill.wiki.gg/wiki/Arms . Nailgun barrels carry lights "corresponding to the
  weapon's variant color". V, https://ultrakill.wiki.gg/wiki/Nailgun
- Player HUD colours: health #ff0000, soft damage #ff6300, hard damage #595959, overheal #0ff000; stamina full #0ddff0,
  charging #03899c, empty #ff0000. V, https://ultrakill.wiki.gg/wiki/HUD
- Blood: the Steam page sells it as "Soak yourself in their blood to regain health". V,
  https://store.steampowered.com/app/1229490/ULTRAKILL/ . Blood colour itself: not found in a source.
- Readability assist: "Enemy Silhouettes" gives thicker outlines or full coloured silhouettes, 1-100 m or always, 1-10 px;
  enemies hidden by fire particles or Virtue beams get outlines even with it off. V, https://ultrakill.wiki.gg/wiki/Assists

## 2. Lighting and fog

- How lighting is computed (vertex-lit, baked, realtime point lights from projectiles): not found in any primary source.
  Modding pages and dev posts I searched say nothing on it. Treat any "it is vertex-lit" claim as unverified.
- Light sources the wiki names are diegetic and placed: torches, bonfires, lit stained glass, cold white lamps, white orbs
  held by statues (V, the level pages above). Nailgun barrels "glow slightly orange" while spinning (V, Nailgun page).
- Fog is used per area as a mood tool: 7-1's white blinding fog (V, 7-1 page). The Cyber Grind exposes fog as data:
  Dynamic (end 0-500), Static (adds a start distance), or off, with a colour option. V,
  https://ultrakill.wiki.gg/wiki/The_Cyber_Grind . Its grid glow is a grayscale mask, "the closer a pixel is to white, the
  more intense its glow is", global intensity 0-10 (V, same page).

## 3. Post-processing: what is on by default

The PSX Graphics block in Options, with wiki-listed defaults. V, https://ultrakill.wiki.gg/wiki/Options

- Downscaling: Off by default, 720p down to 36p.
- Dithering: 100% default, range 0-500%.
- Texture Warping (affine wobble): 0% default, 0-100%.
- Vertex Warping (vertex snapping): None default; Light, Medium, Heavy, Very Heavy, "Modern Art".
- Color Compression (colour-depth reduction): Medium default, None to "Indie Art Game". A player's reading: "It reduces the
  number of colors that can be rendered." (R, https://steamcommunity.com/app/1229490/discussions/0/3004425113435273400 )
- Custom Color Palette: off; a palette-texture menu, user palettes go in a Palettes folder.
- Also: Gamma 1.0 (0-2), FOV 105 (45-160), "Use Fallback Shaders", and Performance toggles (simpler explosions, fire,
  spawn effects; no environmental particles, hit particles, heat waves).

The defaults depend on a first-boot choice. The intro asks "old" or "new" look; Hakita: "go in the PSX section of the
Graphics settings and disable everything except Color Compression" to get the PC look. V,
https://steamcommunity.com/app/1229490/discussions/1/3044983510167265625 . So colour compression is part of even the
"clean" look, and the wiki's defaults (dither and compression on, warping and downscale off) read as that clean path.
Texture filtering (point vs bilinear) and bloom: not listed on the Options page, not found elsewhere.

## 4. Hit and kill feedback

- Hitstop: "brief freezes of time and music". Major = 0.25 s (parries, interruptions, punching a cannonball, shooting a
  Core Eject grenade or a rocket with hitscan); minor = under 0.25 s (Feedbacker / Knuckleblaster punches, rockets on
  enemies); some exceed it (a charged Jackhammer). Hitstops do not advance the level timer, and major Assists' game speed
  does not change them. R (search snippets of the fandom Hitstop page; the mirror blocked my fetch),
  https://breezewiki.discard.no/ultrakill/wiki/Hitstop
- Parry: the game "briefly freeze[s] and flash[es] the screen white", restores health up to the hard-damage limit and
  refills stamina; a parried projectile speeds up, explodes, and hits harder. V, https://ultrakill.wiki.gg/wiki/Parry
- Punching your own projectiles: pellets and drills get hitstop and a flash; saws get hitstop only; coins get neither. V,
  https://ultrakill.wiki.gg/wiki/Arms
- Options: Screenshake 0-200%, default 100%. Parry Screen Flash on by default; off removes the flash "but keeps the freeze
  frame". Controller rumble has its own "parry hitstop" slider. V, https://ultrakill.wiki.gg/wiki/Options
- Blood as healing: heal by "attacking an enemy at close range" (no radius given). Per hit: punch 10 / 20 / 30, hitscan
  10 / 20 / 30 (body, limb, head), shotgun pellet 3, nail 1, gib 120, parry 999. Hard damage = 35% of damage taken, a
  temporary health ceiling that drains at 15/s after a 1 s+ cooldown. V, https://ultrakill.wiki.gg/wiki/Health .
  Guides add the blood must be "fresh and pouring" and you stand under it. R,
  https://gamerjournalist.com/how-to-heal-in-ultrakill/
- Style meter: D, C, B, A, S, SS, SSS, U at 200 / 300 / 400 / 500 / 700 / 850 / 1000 / 1500 points stacked, passive
  decay 15/s at D up to 120/s at U. Bonuses list as "+ Kill", "+ Headshot" (80), "+ Parry" (100). Movement multiplier up
  to 3x while sliding or airborne. Weapon Freshness per variant: FRESH 1.5x, USED 1.0x, STALE 0.5x, DULL 0x; each use
  -0.7, others +0.5, unheld +0.1/s. Taking damage drops one rank. V, https://ultrakill.wiki.gg/wiki/Style
- Hakita wants style to be "more about positive feedback (giving bonuses for changing things up) rather than negative
  feedback". V, intothebluesky 2020 interview above. On audio (GG's domain, noted only): "It feels like everything's
  always clipping". V, https://intothebluesky.com/2021/10/05/follow-up-interview-with-hakita/

## 5. How the effects stay consistent

Documented mechanisms, not developer statements of intent:

- One colour table. Four variant colours drive the HUD icons, the arms, the gun lights, and even geometry: "The color of
  the Sawed-On Shotgun's chainsaw is determined by the color of the red variation of weapons in the HUD." V,
  https://ultrakill.wiki.gg/wiki/Shotgun and HUD page.
- One reward bundle per skill event. A parry is always freeze + white flash + heal + stamina; projectile boosting reuses
  the same visual without the heal (V, Parry page; R, search snippet). The flash and the freeze are separate switches.
- Systems feed each other. Style rank speeds hard-damage decay (S 40/s, SS 50/s, SSS 60/s, U instant). V, Health and
  Style pages. So the meter, healing and aggression read as one loop.
- One tell grammar. Yellow vs blue spark, 4-point vs 8-point shape, each with a sound. V, Parry page.
- One global slider per effect family (screenshake, flash, rumble, silhouettes, PSX block), so a player tunes intensity
  without breaking the relationships. V, Options and Assists pages.
- Charge states share two colours: Railcannon full #3fe8ff, charging #ff0000 (V, HUD page); the Core Eject's bar "turning
  red" while charging (V, Shotgun page).

## 6. Weapon charge: where it shows and how readable it is

- Railcannon: one shared 16 s recharge. HUD: "a large lightning bolt icon next to the player's weapon icon", cyan when
  full, red while charging (toggle "Railcannon Meter", on by default). On the gun: two prongs with four neon screws each
  that "deactivate after firing the weapon and reactivate one at a time" as charge returns, a hum that "increases its
  pitch based on the charge level", and "a distinct sound effect" when done. V, https://ultrakill.wiki.gg/wiki/Railcannon
- Piercer revolver: hold alt fire "over the course of half a second"; release early cancels; 2.5 s cooldown. Charge shows
  on "a small weapon UI directly above the handle indicating weapon variant and current state"; no separate meter is
  documented. Sharpshooter shows charge only by the gun spinning (0.3 s = one bounce, 2.5 s = three). V,
  https://ultrakill.wiki.gg/wiki/Revolver
- Shotgun: Core Eject distance by hold time, bar on the weapon UI turns red; Pump Charge has no meter, the third pump
  starts "an alarm sound". V, Shotgun page. Nailgun magnets warn by "a rapidly increasing alert sound and flash". V,
  Nailgun page.
- Crosshair HUD: health and stamina arcs around the crosshair (None to "Extra Thicc"), fading about 1 s after a change. V,
  HUD page.
- Readability, my judgment from the above (not measured): the long-cooldown weapon gets three redundant channels (HUD
  icon, on-model segment count, rising pitch); short charges (0.5 s Piercer) get only the on-gun screen and the animation,
  because they are learned by timing, not watched.

## What to steal for a dusk-palette movement shooter

1. A single variant colour table that every surface reads: HUD icon, on-gun screen, emissive strips, trail colour. In
   SKYGRIND that table lives in UnityTuning or config, and every player effect stays in the cyan family by rule.
   Source: https://ultrakill.wiki.gg/wiki/HUD , https://ultrakill.wiki.gg/wiki/Shotgun
2. Tells coded by shape and brightness, not only hue: 4-point spark for melee, 8-point for ranged, plus a two-state code
   for parryable or not. With red-orange reserved for enemies, do the parry code inside that family (bright pale-gold
   core vs dark ember) and let the spark shape carry the rest. Source: https://ultrakill.wiki.gg/wiki/Parry
3. Two hitstop tiers in whole sim steps: major 0.25 s (30 steps at 120 Hz) for parries and interrupts, minor shorter for
   punches and rocket hits, never counted on the run timer and immune to slow-motion scaling. Source (Reported):
   https://breezewiki.discard.no/ultrakill/wiki/Hitstop
4. A fixed parry bundle (freeze + one white flash frame + heal to the cap + stamina refill), with the flash on its own
   accessibility toggle separate from the freeze, and screenshake as a 0-200% slider. Source:
   https://ultrakill.wiki.gg/wiki/Parry , https://ultrakill.wiki.gg/wiki/Options
5. Colour quantization plus ordered dither as the baseline look, vertex and texture warping as opt-in extras. This keeps
   the identity without hurting readability. Source: https://ultrakill.wiki.gg/wiki/Options ,
   https://steamcommunity.com/app/1229490/discussions/1/3044983510167265625
6. Long-cooldown weapons show charge diegetically in segments: N lights on the model that relight one at a time, mirrored
   by a two-state HUD icon next to the weapon icon. Source: https://ultrakill.wiki.gg/wiki/Railcannon
7. Let the score meter feed survival: a higher rank drains the temporary damage ceiling faster, so playing loud is also
   playing safe. Source: https://ultrakill.wiki.gg/wiki/Health , https://ultrakill.wiki.gg/wiki/Style
8. Fog as a per-area data row (mode, start, end, colour) rather than one global value; one area can go white-out on
   entry with a single flash. Source: https://ultrakill.wiki.gg/wiki/The_Cyber_Grind , https://ultrakill.wiki.gg/wiki/7-1

# Tutorial text, UI art direction, blurred backdrops, environment and enemy readability (wave 43, for G6-G8 G21 G24 G29)

Web + GitHub research, 2026-10-16. Adds to wave 35 `teach.md` (ULTRAKILL 0-1 text pattern, pad glyphs) and `look.md`
(UIEffect, LitMotion, TMPEffects, KinoGlitch), wave 33 `U1-hud.md`, wave 38 `uk-hud-feel.md` (ULTRAKILL HUD sway, hurt
screen). "[code]" = read in a decompiled ULTRAKILL source mirror on GitHub (numbers and behaviour only, nothing copied);
"[fetched]" = page read; "[snippet]" = search summary only.

## A. The centred tutorial message (G7)

1. **ULTRAKILL's HudMessage is one shared panel, filled by trigger volumes.** A `HudMessage` component in the level calls
   the singleton `HudMessageReceiver`: text = `message` + the player's CURRENT binding for an input action + `message2`;
   `$` in the string becomes a line break; the panel's image and text are enabled, a sound plays, and the panel hides
   after `timerTime` (default 5 s) when timed, or on leaving the trigger (`deactiveOnTriggerExit`) or on disable. A
   `playerPref` key makes a message show once per save ("hideShotgunPopup", "secretMissionPopup"). [code]
   https://github.com/teker821/ULTRAKILL/blob/HEAD/HudMessage.cs , `HudMessageReceiver.cs` (default `Invoke("Done", 5f)`)
   - for SKYGRIND: one prompt panel, many trigger rows: text with a `{input}` token resolved from the live binding and
     device (glyph), shown while the player is inside the lesson volume, gone when they leave or do the verb, once per save.
2. **Its open animation is a 2-axis "CRT" pop.** `HudOpenEffect.Reset` sets the panel's scale to 0.05 x 0.05, then grows
   X first, then Y, each at `(target - s + 0.1) * 30` per UNSCALED second (so it also plays during slow motion). [code]
   https://github.com/teker821/ULTRAKILL/blob/HEAD/HudOpenEffect.cs . Derived: each axis takes ln(10.5)/30 = 0.078 s,
   about 0.16 s for the whole pop.
   - for SKYGRIND: a 0.15-0.2 s horizontal-then-vertical open in unscaled time, with the open sound event, is the
     "prominent but not permanent" look G7 describes. On-screen size / position of ULTRAKILL's panel lives in the prefab:
     not checked.
3. **Level / boss name cards type in.** `LevelNamePopup`: the layer line types at 0.01 s per letter, waits 0.5 s, then
   the name types at 0.01 s per letter and holds 3 s. [code] https://github.com/teker821/ULTRAKILL/blob/HEAD/LevelNamePopup.cs
   - for SKYGRIND: arena titles and boss cards: ~10-20 letters type in 0.1-0.2 s, hold 2-3 s, never pause the sim.
4. **Text pattern (wave 35):** VERB IN CAPS + the bound input + at most one rule ("JUMP while near a WALL to WALL JUMP.
   (Max. 3 times)"). Doom Eternal's repeated pausing pop-ups are the counter-example (see encounters file, item 7).

## B. UI art direction (G6, G24)

5. **Persona 5 (art director Masayoshi Sudou, Famitsu):** each game's UI colour follows its main palette (P3 teal, P4
   yellow, P5 crimson), tested against black and white text; "I tend to not add gradations" (flat fills); iterate "until
   I feel 100% satisfied". [snippet] https://segabits.com/?p=54918
   - for SKYGRIND: flat fills of 3 colours + 1 accent: dusk navy panels, pale violet-white text, cyan for player state,
     red-orange only for danger; no gradients on frames (gradients only inside meters).
6. **Hierarchy by role (Persona 5 analysis):** core (main actions), supportive (party HP/SP, enemy name), extraneous
   (comments); the hovered item gets bigger AND gains colour; button colours match the controller's face buttons.
   [fetched] https://mechanicsofmagic.com/?p=7280
   - for SKYGRIND: score / shop screens: one hero number (rank or total) at 2-3x the size of anything else, a supportive
     band (time, kills, style), the rest small; the selected shop card scales to ~1.08 and lights its cyan edge.
7. **Sakurai: "Each UI has a different aim"** (expand the world, add personality, ease play, tie things together); P5's
   busy UI is praised for presentation even though it slows new players. [fetched]
   https://sourcegaming.info/2016/11/24/the-aims-of-ui-sakurais-famitsu-column-vol-515/
   - for SKYGRIND: HUD aims at "ease play" (minimal, corner-tucked, fast); full-screen menus aim at "personality" (angled
     panels, motion, the game's own type) since nothing is at stake while they are open.
8. **Blurred backdrop in URP (G24): Bluecadet UI Blur** (MIT, `com.bluecadet.uiblur` 1.0.2, `"unity": "6000.3"`; our
   editor is 6000.3.7f1). A Render Graph renderer feature renders a Kawase dual-filter blur (down then up, up to 6 passes)
   of the camera image into a global texture `_UIBlurTexture`; a UI shader samples it by screen UV. Defaults: `blurScale`
   4, `resolutionScale` 0.5, `renderPassEvent` AfterRenderingTransparents. Install pinned by Git URL
   `https://github.com/bluecadet/unity-packages.git?path=Packages/com.bluecadet.uiblur#com.bluecadet.uiblur@1.0.2`
   (tag 1.0.2 = commit b3885a4, 2026-08-07; 1.0.0 = 2026-05-19; both older than two weeks). [fetched + GitHub API]
   https://unity.bluecadet.com/uiblur/ ; licence from the GitHub API: MIT. Unity tracks an "incorrect partial blur when
   using rendergraph in urp" bug. https://issuetracker-mig.prd.it.unity3d.com/issues/incorrect-partial-blur-when-using-rendergraph-in-urp
   - for SKYGRIND: blur + darken (multiply by ~0.55 navy) behind the shop and score panels; works with our camera stack
     only if the feature runs on the WORLD camera's renderer (not checked); a Screen Space Overlay canvas can still sample
     the global texture (not checked). Fallback with no package: freeze one frame to a RenderTexture at menu open and
     blur it once (a static backdrop costs nothing per frame).
9. **Health bar that reads as designed (derived from ULTRAKILL + Hades):** ULTRAKILL shows a dead "hard damage" segment
   that regrows (wave 42); its boss bar keeps a damage after-image that waits 0.15 s then drains (bosses file); Hades
   shows armour as a separate yellow bar over health, outlined in the enemy colour (bosses file).
   - for SKYGRIND: health as a segmented, slanted bar (e.g. 10 cells) in a frame with one notch per 25 %, a white
     after-image chunk that waits 0.15 s then drains, a cyan heal sweep, and a red-orange pulse of the FRAME (not the fill)
     on hit. Low health: the frame breathes, never a full-screen tint.

## C. Environment and enemy readability (G8, G21, G29)

10. **TF2's three reads: colour (team) -> silhouette (class) -> detail concentrated at the chest**; characters graded dark
    at the feet to contrasting torso; rim highlights separate them from the background. **Left 4 Dead:** fog lightened
    because darker fog lost "that silhouette read"; colour correction desaturates everything below a threshold while
    "health packs, blood, exit points" stay saturated; vignette in the top corners only; lights mark safe areas and routes.
    [fetched] https://www.gamedeveloper.com/art/in-depth-how-valve-makes-art-to-enhance-gameplay
    - for SKYGRIND: the saturation budget belongs to gameplay: environment desaturated mid-low value (navy / violet
      dusk), enemies the most saturated warm objects on screen, pickups and interactables the only cyan emitters, exits
      lit. Decoration may glow only in the environment's own cool hue at low intensity.
11. **Darktide: fake rim light that fades in with distance** so far enemies stay visible; head / shoulder area carries the
    most perceived difference between enemy types; back attachments change the silhouette from front and side; variants
    must not drift from their layer's look. [fetched] https://darktide.gameslantern.com/news/darktide-101-enemy-character-art-dev-blog
    - for SKYGRIND (G21): keep the pill body for all; put the class in the TOP third: visor shape (slit = gunner, round =
      flyer, chevron = charger, plate = shield), plus one back part (fins, tank, antenna). A distance-scaled emissive rim
      in yellow-orange keeps them readable against dusk sky.
12. **Small detail must support the big read.** "Small details should add visual interest to your major silhouette reads,
    not distort them" (Riot character artist). [fetched] https://80.lv/articles/character-design-shape-language-and-readability/ ;
    Overwatch art values include "big, bold silhouettes". [snippet] https://creators.vice.com/en_us/article/heres-what-its-like-to-art-direct-overwatch
13. **Realistic lighting lowers enemy contrast.** Clint Hocking: lighting direction and ambient occlusion reduce contrast
    and make enemies harder to see. [snippet] https://gamedev.net/news/3499-splinter-cell-designer-says-modern-lighting-has-made-stealth-games-harder-to/ ;
    an indie note: when floors and walls are "about as bright as ... enemies and projectiles", hazards stop standing out.
    [snippet] https://itch.io/post/10227840
    - for SKYGRIND: enemies get emissive visors that ignore scene lighting; check readability with a value-only (greyscale)
      screenshot of each arena: enemies and hazards must be the top 10 % of values or the most saturated hue.
14. **Neon White: limited palette for legibility at speed.** [snippet] https://gonintendo.com/comments/3622

**Art framework for SKYGRIND (derived from 10-14 and CLAUDE.md rule 8):**
- Value: sky brightest, environment mid-dark, enemies + hazards + interactables carry the emissive.
- Hue ownership: cyan = the player can use / stand on / ride; red-orange-yellow = enemy or danger (lava fits); magenta =
  RIVAL (and, if GG rules so, the empowered-impact flash); pale violet-white = boundary.
- Ridable walls: one consistent cyan edge strip + texture rhythm along the ride direction; non-ridable walls never have it.
- Hazards move (scrolling lava, pulsing grates) and light the geometry around them from below.
- One landmark per arena visible from its entrance (a tower, a beam, the boss's silhouette) to show where "forward" is (G11).
- Decoration must tell one of: route, threat, world (factory / foundry / ice line), never "a neon strip because it was empty".

## Not found
Primary sources on Hi-Fi Rush, Hades or Neon White UI design; ULTRAKILL's HudMessage on-screen size / font size (prefab).

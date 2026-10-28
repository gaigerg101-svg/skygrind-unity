# R2: the gritty retro look and a buzzing, readable HUD (wave 30 research)

Web-only research, 2026-10-09, about 15 minutes. Every claim carries its URL. Quotes are under 15 words. Where a
page did not say something, this file says "not checked" or "not stated". Cost notes marked **(reasoning)** are my
estimates from how the technique works, not measured on SKYGRIND.

---

## 1. The PS2 / ULTRAKILL / boomer-shooter look

### 1.1 The parts and what produces each

| Part | How it is made | Source |
|---|---|---|
| Low internal resolution, point upscale | Render to a small target, upscale nearest-neighbour; or quantise screen UVs in a post pass: `uv = floor(uv * W) / W` | URP: https://docs.unity3d.com/6000.0/Documentation/Manual/urp/universalrp-asset.html ; Kodrin URP-PSX `Pixelation.shader`: https://raw.githubusercontent.com/Kodrin/URP-PSX/master/URP-PSX/Assets/Shaders/Pixelation.shader |
| Vertex snapping / jitter | In the vertex shader, divide by w, floor x/y to a grid of `snap_res`, multiply by w again | MenacingMecha `psx_base.gdshaderinc`: https://raw.githubusercontent.com/MenacingMecha/godot-psx-style-demo/master/shaders/psx_base.gdshaderinc |
| Affine texture warp | Interpolate UVs without perspective correction (`POSITION /= abs(POSITION.w)` in that Godot include) | same file |
| Colour quantisation | `floor(c * N) / N` (Kodrin) or `round(c * N + dither) / N` (MenacingMecha) | Kodrin shader above; https://raw.githubusercontent.com/MenacingMecha/godot-psx-style-demo/master/shaders/pp_band-dither.gdshader |
| Ordered dithering | Add a 4x4 Bayer-style threshold before quantising | Kodrin `DitheringPatterns.cginc`: https://raw.githubusercontent.com/Kodrin/URP-PSX/master/URP-PSX/Assets/Shaders/cginc/DitheringPatterns.cginc |
| Limited texture resolution | Point filtering + lower mips / small textures ("resolution limiter ... lower mip levels") | Daniel Ilett's PSX Shaders Pro page (commercial, named only as a reference): https://danielilett.itch.io/psx-shaders-pro-for-unity-urp |
| Fog | Distance fog hides draw distance; URSC has `fog_start_distance` / `fog_end_distance` globals | https://github.com/Zorochase/ultimate-retro-shader-collection |
| No AA | Implied by point upscale; URP's nearest-neighbour filter is not combinable with post (below) | URP doc above |

A 15-bit target is "32 (32768 colors in total)" per channel step count, with Bayer ordered dithering
(Prototype: Odysseus, Unity URP): https://itch.io/devlog/849811/vfx-breakdown-lets-go-retro.amp

**Unity gotcha:** URP's own point upscale: "The Nearest-Neighbour filter doesn't work if Post-processing is enabled."
https://docs.unity3d.com/6000.0/Documentation/Manual/urp/universalrp-asset.html . SKYGRIND uses post (bloom, etc.), so
the pixelation must be a full-screen pass (Kodrin's UV-floor approach) or a manual low-res render texture, not
Render Scale + Nearest. (Whether SKYGRIND's current renderer has post on: not checked in this research.)

### 1.2 What ULTRAKILL itself ships (Options, PSX section)

From https://ultrakill.wiki.gg/wiki/Options :
- **Downscaling:** "Set to Off by default, ranges from 720p to 36p" (36p is labelled "Absurd").
- **Dithering:** "Set to 100% by default, ranges from 0% to 500%".
- **Texture Warping (affine):** "Set to 0% by default, can scale from 0% to 100%".
- **Vertex Warping:** None, Light, Medium, Heavy, Very Heavy, "Modern Art" (default: not stated on the page).
- **Color Compression:** "Set to Medium by default", from None up to "Indie Art Game".
- **Color Palette:** custom palettes, plus built-ins (Gamebot Color, Noir, Pink and Purple, Rustic, ...).
- Gamma 1.0 default (0.0 to 2.0); FOV "Set to 105 by default, ranges from 45 to 160".

So ULTRAKILL's **defaults** are: full resolution, no affine warp, medium colour compression with dithering at 100%.
The heavy PSX look is opt-in. Players report affine warp makes floors "excessively distorted" and vertex warping can
make water vanish: https://steamcommunity.com/app/1229490/discussions/0/3004425113435273400 (search summary; thread
text not opened). Texture filtering option in ULTRAKILL: not checked (not on the wiki Options page as fetched).

### 1.3 Open shaders, licences, default values

| Project | Licence | Defaults read in source |
|---|---|---|
| Kodrin / URP-PSX (Unity URP renderer features + Shader Graph) https://github.com/Kodrin/URP-PSX | MIT | `widthPixelation = 512`, `heightPixelation = 512`, `colorPrecision = 32` (Pixelation.cs); `ditherScale = 2`, `ditherStrength = 1`, `ditherThreshold = 512`, `patternIndex = 0` (Dithering.cs); fog `fogDensity 1`, `fogDistance 10`, `fogNear 1`, `fogFar 100`, `noiseStrength 0.05` (Fog.cs). Vertex jitter is a Shader Graph subgraph `VertexJitter.shadersubgraph` (values not read). |
| Math-Man / URP-PSX fork (Unity 2022.3, URP 14; adds affine + CRT) https://github.com/Math-Man/URP-PSX | MIT (also "use for whatever you want") | README states no numbers. |
| RubenTipparach / HPSXURP Shader Graph Tools https://github.com/RubenTipparach/HPSXURP-Shader-Graph-Tools | MIT | README states no numbers; "Vertex jitter (and resolution control)", "Texture scaling + point filtering". |
| MenacingMecha / godot-psx-style-demo https://github.com/MenacingMecha/godot-psx-style-demo | MIT | `precision_multiplier` 1.0 (0 to 1) scales the vertex snap grid; post `col_depth = 15.0`, `dither_banding = true`. Suggested PS1 resolutions 256x240, 320x240, 512x240 (README, historical, not defaults). |
| Zorochase / Ultimate Retro Shader Collection (Godot) https://github.com/Zorochase/ultimate-retro-shader-collection | MIT | `vertex_snap_intensity` int 0 to 2; `affine_texture_mapping` bool; `texture_filtering` bool (N64 3-point); fog start/end (0 = off); `texture_lod_halve_distance`. Numeric defaults not stated. |

Ports to Unity are fine under MIT (keep the notice). The Godot ones are for maths only.

### 1.4 What each part costs (reasoning, not measured)

- **Pixelation pass (UV floor):** one extra full-screen blit; cheap. A true low-res render target is *cheaper*
  than full-res (fewer fragments), so the "look" can pay for itself.
- **Colour quantise + 4x4 dither:** a few ALU ops in the same pass; near free if merged with pixelation.
- **Vertex snap:** a floor per vertex; negligible. It must stay view-only: it never touches `Skygrind.Core`, the
  web collider or any ray (CLAUDE.md rule 4).
- **Affine warp:** free on the GPU but costly in *readability*: big floor quads swim. ULTRAKILL ships it at 0%.
- **Point-filtered small textures:** saves memory and bandwidth.
- **Fog:** cheap per fragment; it also hides draw distance.
- The real risk is **readability**, not speed: quantising can collapse SKYGRIND's cyan / red-orange / magenta /
  violet-white role colours into one band. Quantise *after* the palette is fixed and check each role colour survives.

---

## 2. A HUD that buzzes like an old TV but stays readable

What was found:
- **ULTRAKILL:** HUD types "Standard, None, Classic Color, or Classic White"; background opacity "Set to 50% by
  default"; toggles for Weapon Icon and Railcannon Meter. https://ultrakill.wiki.gg/wiki/Options
- **Cruelty Squad:** a search summary said health is "a pulsing glob that changes color" with damage; the two
  articles I opened (gamedeveloper.com, vice.com) did not say this, so **unconfirmed**. Kallio on the look: "I simply
  tried to make something that looks good to me."
  https://www.gamedeveloper.com/design/examining-amoral-world-big-business-cruelty-squad
- **Mullet Madjack:** the health is the timer: kill an enemy every 10 seconds or die (search summary of
  https://timeextension.com/news/2024/05/mullet-madjack-is-a-new-retro-fps-inspired-by-classic-80s-and-90s-anime ;
  CRT / VHS HUD details: the Creative Bloq article was paywalled, not checked).
- **Prodeus:** "modern rendering techniques" for a retro look; "resolution options (in order to get a proper
  modern-day look)"; "various HUD" setups.
  https://www.dsogaming.com/news/kickstarter-campaing-launched-for-prodeus-old-school-first-person-retro-shooter-using-modern-rendering-techniques/
- **Signalis:** CRT-inspired visuals "constantly invade the screen" (search summary); its HUD specifics: not checked.
- **Math-Man URP-PSX** has a CRT pass with "screen bending, vignette, scan lines, noise, and chromatic aberration"
  (MIT): https://github.com/Math-Man/URP-PSX

**Safety limits for any flicker** (Game Accessibility Guidelines,
https://gameaccessibilityguidelines.com/avoid-flickering-images-and-repetitive-patterns/ ):
- avoid "More than three flashes in a single second, covering 25%+ of the screen";
- avoid flashing sequences that last "more than 5 seconds";
- moving high-contrast stripes over 25% of the screen, static over 40%, are triggers (scanlines over the whole
  screen count);
- name the setting "screen flash effects" or "effects intensity", never "epilepsy safe".

Patterns that fit these limits (design reasoning, sources above for the limits):
1. **Per-element flicker, not screen flicker:** each HUD panel gets its own small alpha noise (for example 0.92 to
   1.0, changing at 10 to 15 Hz). The change is small, so it is not a "flash"; the text never drops below its contrast.
2. **Scanlines on the UI layer only**, alpha about 0.1 to 0.15, static, at most 1 line per 2 to 3 UI pixels, and
   only *inside* HUD panels (well under 40% of the screen).
3. **Chromatic edge:** offset R and B copies of the panel *border / glyph outline* by 1 to 2 px, never the
   numbers' fill. Increase it for about 150 ms on a state change, then let it settle.
4. **Glow pulse on state change only:** one pulse (up, then decay over about 200 to 300 ms) when a value changes
   state (ready, low, hit). A constant pulse becomes noise.
5. **One "TV off" global, settings-backed:** "effects intensity" scales 1 to 4 at once (0 = clean HUD), the same
   way ULTRAKILL exposes Classic / None HUD types.

---

## 3. Making weapon states readable (charging / cooling down / ready)

What the reference games do:
- **ULTRAKILL railcannon:** "a shared 16-second cooldown"; ready is "indicated by a distinct sound effect and a
  marker on the HUD"; "a large lightning bolt icon next to the player's weapon icon". https://ultrakill.wiki.gg/wiki/Electric
  The HUD page reports it is cyan when full and red while charging (wiki summary): https://ultrakill.wiki.gg/wiki/HUD
  (Red is the opposite of SKYGRIND's rules; SKYGRIND would use dim cyan, not red, for "charging".)
- **ULTRAKILL stamina:** cyan when full, a darker teal while recharging, red when empty (same HUD page summary).
- **ULTRAKILL arms:** "Punch Exhaustion, which is displayed as a meter where the arm icon is"; the Whiplash "has its
  own separate cooldown of 0.5 seconds". https://ultrakill.wiki.gg/wiki/Arms
- **ULTRAKILL coins:** "4 seconds per coin, one coin at a time" (https://ultrakill.wiki.gg/wiki/Revolver); how the
  HUD shows coins: not stated on the wiki pages read.
- **DOOM Eternal:** cooldowns are long (frag 30 s, Flame Belch 25 s; chainsaw 3 fuel pips, one refills in 20 s, per
  doomwiki search summaries: https://doomwiki.org/wiki/Frag_grenade_(Doom_Eternal),
  https://doomwiki.org/wiki/Flame_belch, https://doomwiki.org/wiki/Chainsaw_(Doom_Eternal); doomwiki blocked the page
  fetch). Update 1.14 added the "Racecar" HUD: a "Reticle Ability Indicator" places those icons near the reticle,
  so players see "chainsaw fuel, Blood Punches, Flame Belch, and melee charges".
  https://www.gamerevolution.com/?p=676748
- **Celia Hodent (The Gamer's Brain / Epic):** her framework has two pillars, usability and "engage-ability";
  the usability pillar adapts Nielsen's heuristics with game vocabulary.
  https://www.designative.info/2019/03/28/the-ux-of-fortnite-celia-hodent/ ;
  https://elitegamedevelopers.substack.com/p/egd-084-celia-hodent-game-ux . Her exact named heuristics ("signs and
  feedback", "form follows function", "minimum workload") are widely attributed to her book but **not confirmed** on
  any page I could open in time; the GDC talk is https://www.gdcvault.com/play/1020867/Developing-UX-Practices-at-Epic
  (not watched).
- GDC HUD talks on cooldown radials: none found with usable text in the time; not checked.

Patterns these support:
- **Redundant signs:** ready is said three ways at once: shape (icon fills or changes form), colour (dim to full),
  and sound (ULTRAKILL's ready noise plays "even if it is not currently equipped", per the search summary of
  https://ultrakill.wiki.gg/wiki/Weapons ). SKYGRIND: GG owns sound, so the port emits the event and leaves the noise
  to him.
- **Put long-cooldown state near the reticle** (DOOM's Racecar HUD), because the eye lives there.
- **Charging rises in pitch / fill continuously; ready is a single discrete event** (railcannon hum "increases its
  pitch based on the charge level", https://ultrakill.wiki.gg/wiki/Electric).
- **Pips for counted charges** (chainsaw fuel, coins), **a fill for one timer** (railcannon bolt).

---

## What SKYGRIND should take (10)

1. **A merged "retro pass"** (one full-screen pass after bloom): UV-floor pixelation + colour quantise + 4x4 ordered
   dither, ported from Kodrin URP-PSX (MIT). Not URP Render Scale + Nearest, which "doesn't work" with post.
2. **Start at ULTRAKILL-like defaults:** pixel grid about 640x360 at 1080p (between Kodrin's 512 and ULTRAKILL's
   720p option; a value to tune, not a source value), colour steps 32 per channel (Kodrin `colorPrecision 32` and
   the 15-bit "32" figure), dither strength 1, dither scale 2.
3. **Affine warp off by default** (ULTRAKILL: 0%); vertex snap at a light level, view-only, never in Core.
4. **Exempt the HUD and the reticle** from the retro pass (render them after it) so 16 px text stays crisp.
5. **Guard the role colours:** after quantising, the cyan / red-orange / magenta / pale violet-white must stay
   distinct; add a test that runs the quantise function on the four palette colours and checks they don't merge.
6. **Expose the look as player options** like ULTRAKILL's PSX section (pixel size, colour compression, dithering,
   vertex warp), with a "clean" preset; names in `UnityTuning.cs`, prefs keys there too.
7. **HUD buzz per panel, not per screen:** alpha noise 0.92 to 1.0, static UI scanlines at about 0.12 alpha inside
   panels only, 1 to 2 px chromatic offset on borders; all under one "effects intensity" setting (0 to 1).
8. **Flash budget:** never more than 3 flashes per second over 25% of the screen, no flashing over 5 s; a ready flash
   is one 200 to 300 ms pulse on the icon, not the screen.
9. **Weapon state = shape + colour + event:** dim cyan fill while cooling, full cyan + one pulse at ready, a
   web-named ready event for GG's sound; pips for counted charges, a single fill for one timer; never red for
   "charging" (red-orange is enemies and danger only).
10. **Reticle-side cooldown chips** for the long-cooldown verbs (THUNDERCLAP LT shockwave at 15 s+, gust), DOOM
    Racecar style, as a toggle, so pad players read cooldowns without looking away.

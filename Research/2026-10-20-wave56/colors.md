# Colour identity per level without breaking the colour roles (wave 56 research)

Date: 2026-10-20. Web research only, about 12 minutes. Sources were WebSearch and WebFetch.

**Sharp question:** how do stylized action games give each level a strong colour identity and still keep their
gameplay colour roles?

**Tags:**
- **Verified:** I fetched the page and read the quoted text myself.
- **Reported:** the claim comes from a search summary or a secondary write-up. I did not read the primary source.
- **Not checked / not found:** I searched and found no developer source.

**The short answer, from the sources that hold up:** the world gets one key hue per level, and that hue lives at
low-to-mid saturation, mostly in the fog, the sky and the large surfaces. Gameplay hues are reserved, more saturated,
brighter in value and few in pixels. Valve's L4D grade pushed everything under a saturation threshold toward grey and
let only the gameplay items stay coloured. TF2 used warm and cool to separate the factions. Valorant keeps material
values close and puts each map's colour into landmarks. Mirror's Edge keeps a sparse world and gives one hue to the
usable things.

## Assumed role bands (replace these with the project's real role hexes; I did not check them against the palette file)

| Role | Hue band (HSV degrees) | Note |
|---|---|---|
| red-orange: enemies and danger | 5-35 | |
| yellow: parryable | 45-70 | |
| cyan: player-usable | 170-200 | |
| magenta: the rival | 285-330 | |
| pale violet-white: the boundary | about 250-280 | V >= 0.85, S <= 0.15 |
| **Free for the world** | 85-150 (green), 205-280 (blue to violet) | violet at V <= 0.6, so it never reads as the boundary |
| **Also free** | any neutral at S < 0.15; warm greys at 20-45 | warm greys only at S <= 0.2 and V <= 0.6 |

**Caution bands:**
- 150-165 sits next to cyan.
- 35-45 (amber) sits between red-orange and yellow.
- 330-355 (rose) sits between magenta and red-orange.

**Measure on the final frame,** after the low resolution, the dither and the colour steps. Quantisation can push a
low-S world pixel into a reserved band.

**Measurement setup for every method below** (a suggestion; I did not check whether the project has a role-ID pass):
1. Capture the 1920x1080 Player frame.
2. Capture a matching role mask from a debug pass where each renderer writes its role ID: world, enemy, tell (parry),
   usable (cyan), rival, boundary, HUD.
3. "World pixels" means mask == world.
4. Hue, S and V are HSV of the final sRGB. Luminance contrast uses the WCAG formula (method 6).

## Methods (11)

### 1. A colour script: one emotional key per level, planned as a sequence before the levels are lit (Pixar)

- **The claims:**
  - Lou Romano describes the colour script as conveying "the emotional tone of each scene ... with pure abstract
    shapes and color".
  - In his Up script, the opening is saturated, the film goes grey after Ellie dies, and the colour returns.
  - Adventure scenes are bright; danger scenes go dark.
- **Sources (Reported):**
  - https://www.moma.org/audio/playlist/192/2575
  - https://www.slashfilm.com/503975/cool-stuff-lou-romanos-color-script-for-pixars-up/
- **Related, Gris (Reported):** five levels, each with "a different predominant color". https://hypercritic.org/?p=1753
- **Measure:**
  - Take 3 frames per level.
  - Compute the circular mean hue and the median S and V of the world pixels at S >= 0.12.
  - The levels' mean hues sit >= 40 deg apart, or differ by >= 0.15 in median S or V where two levels share a hue
    sector. (These thresholds are mine; no source gives them.)
  - Print the result as one swatch strip per level, in campaign order, so the arc can be read.

### 2. A gamut mask per level that cuts the reserved hue sectors out of the world (James Gurney)

- **The claims:**
  - A gamut mask is "a geometric overlay placed over the color wheel to limit our color choices".
  - Changing the mask's side lengths makes some colours dominant and others subservient.
- **Sources (Reported):**
  - https://linesandcolors.com/2011/09/19/james-gurney-on-gamut-masking/
  - https://www.sessions.edu/notes-on-design/color-series-gamut-masks/
- **Measure:**
  - Among world pixels at S >= 0.25, the share inside the four reserved bands is <= 2 %.
  - Inside the cyan band and the magenta band it is 0 %.
  - The level's mask is written as a hue range plus a max S, and checked per frame.

### 3. A saturation threshold: the world sits under a cap and only the gameplay items stay saturated (Valve, Left 4 Dead)

- **The claims:**
  - "It simplifies the palette, but we still wanted health packs, blood, exit points to pop. We did that with a
    saturation threshold. Anything below that moves towards gray."
  - Valve's TF2 team ranked readability in three levels: team colour first, then silhouette, then detail.
- **Source (Verified):** https://www.gamedeveloper.com/art/in-depth-how-valve-makes-art-to-enhance-gameplay
- **Measure:**
  - The 95th percentile S of world pixels is <= 0.55.
  - No world pixel exceeds S 0.8.
  - The median S of tell pixels (enemy, parry, usable, rival) is >= 0.75.
  - The tell-to-world saturation gap (median tells minus world p95) is >= 0.2.

### 4. Warm and cool split by faction; shadows go cool and never black (Valve, TF2)

- **The claims:**
  - Red team: "predominantly warm colors - some grays, but they're warm as well".
  - Blue team: "cooler colors, and industrial materials such as concrete and steel".
  - Lighting goes "warm to cool - cool in the shadows, but never going black".
  - A rim highlight pops the character, because otherwise "the shadowed side gets lost into the background".
- **Source (Verified):** https://www.gamedeveloper.com/art/in-depth-how-valve-makes-art-to-enhance-gameplay
- **For SKYGRIND:** the enemies own warm, so the world's shadows and fog go cool.
- **Measure:**
  - The mean hue of world pixels at V < 0.3 sits in 200-280 deg.
  - No world pixel at V < 0.3 has S > 0.35 inside 5-70 deg.
  - The enemy's mean hue is >= 60 deg from the mean hue of its 16 px ring of world pixels.

### 5. Value first: the actors are the brightest and highest-contrast thing; the world stays in a narrow value band (DOTA 2 and Valorant)

- **The claims:**
  - DOTA 2: "The ground uses darker grays than the heroes or units ... the heroes and units are the brightest and
    highest areas of contrast."
  - The same article warns that "if everything is drawing attention, nothing will".
  - Valorant: "our materials are similar in value and there isn't too much contrast or darkness".
  - Valorant again: added colours "aren't too dark, especially in interior spaces".
- **Sources (Verified):**
  - https://www.gamedeveloper.com/design/how-to-reduce-visual-confusion-in-your-game
  - https://playvalorant.com/en-us/news/dev/the-art-of-valorant-map-environments/
- **Measure:**
  - On a greyscale frame, the interquartile range of world luminance is <= 0.25 (in 0-1 relative luminance).
  - The world's 10th percentile V is >= 0.12, so it is not crushed.
  - Each enemy's median luminance sits outside the world's middle 50 %.

### 6. A contrast floor for every tell: 3:1 against the pixels next to it (WCAG non-text contrast)

- **The claim:** "a contrast ratio of at least 3:1 against adjacent color(s)" for graphical objects needed to
  understand the content.
- **Formulas:**
  - Ratio = (L1 + 0.05) / (L2 + 0.05).
  - L = 0.2126 R + 0.7152 G + 0.0722 B, on linearised sRGB.
- **Source (Verified):** https://www.w3.org/WAI/WCAG21/Understanding/non-text-contrast.html
- **Measure:** for every enemy, parry-yellow tell, cyan usable and the boundary, the median luminance of the object
  against the median of its 16 px ring is >= 3:1. Report the worst object per frame.

### 7. An accent budget per frame: about 60 / 30 / 10 for the world, with the role hues as a separate small tier

- **The claim:** about 60 % dominant, about 30 % secondary and about 10 % accent. It is an interior and UI design rule
  of thumb, not a game-specific source.
- **Sources (Reported):**
  - https://www.freecodecamp.org/news/the-60-30-10-rule-in-design/
  - https://uxplanet.org/the-60-30-10-rule-a-foolproof-way-to-choose-colors-for-your-ui-design-d15625e56d25
- **Measure:**
  - Run k-means with k = 3 on world pixels at S >= 0.12. Then:
    - the largest cluster (the level key) covers 50-70 % of them;
    - the level accent (the smallest cluster, S >= 0.35) covers 3-10 % of the world pixels.
  - Role hues together cover <= 8 % of the frame in calm play.

### 8. Landmarks carry the level colour; the colour helps you find your way and is not decor noise (Hyper Light Drifter and Valorant)

- **The claims:**
  - Alx Preston: "Each environment uses a striking and specific palette to highlight landmarks."
  - Valorant uses "colors as a way to help distinguish certain areas and structures", for example "the yellow shipping
    container on B-site".
- **Sources (Verified):**
  - https://www.gameinformer.com/games/hyper_light_drifter/b/macintosh/archive/2014/03/14/developer-covers-key-aspects-hyper-light-drifter.aspx
  - the Valorant URL in method 5
- **Related (Reported):** reviews describe HLD's regions as violet and blue in one zone and orange and red in
  another. https://games.gg/de/hyper-light-drifter/
- **Measure:**
  - One landmark mass per room in the level accent, at S 0.4-0.65, covering 1-5 % of the frame.
  - It is visible (not masked) from every room entry. Check it with one capture per entry.

### 9. A "runner vision" hue: a sparse world, and one hue that only usable things wear (Mirror's Edge)

- **The claims:**
  - The interviewer: "the color is so sparse, but then there's a bright splash of red or yellow."
  - Nick Channon: "the red itself is what we call 'runner vision'"; the red elements are things "she can use to her
    advantage".
  - Runner vision can be turned off on harder settings (Reported, Wikipedia).
- **Sources:**
  - Verified: https://www.gamedeveloper.com/design/the-philosophy-of-faith-a-mirror-s-edge-interview
  - Reported: https://en.wikipedia.org/wiki/Mirror%27s_Edge
- **For SKYGRIND:** cyan already is SKYGRIND's runner vision.
- **Measure:**
  - Cyan-band pixels (170-200 deg, S >= 0.4) outside the usable mask are 0 % of the frame.
  - Every usable object in view shows >= 50 cyan-band pixels at 1080p.

### 10. Fog and sky carry the level hue; lighten the fog for silhouettes (Valve, Left 4 Dead)

- **The claim:** with "a more accurate, darker fog" the silhouettes suffered, so Valve lightened it: "people can see
  things much better and it's much more dramatic."
- **Source (Verified):** the Valve URL in method 3.
- **Fog as the level's colour carrier** is a general technique. I did not find a developer source that states it: the
  Firewatch fog talk is behind GDC Vault (not checked).
- **Measure:**
  - Far-field pixels (depth > 60 % of the fog end, from a depth capture) take the level key hue, within ±15 deg, at
    S 0.2-0.45.
  - Every enemy in front of the fog keeps >= 3:1 luminance contrast against it (method 6).

### 11. A neutral ground for gameplay colour to land on (Splatoon); and a warning about cues that fight the grade (Doom: The Dark Ages)

- **Splatoon (Reported):**
  - Stages start from "a place ... that has an origin of grey-toned neutral color and could be vandalized with colors"
    (the Famitsu interview via search).
  - The prototype rabbits were neutral so the inks would contrast.
  - Sources:
    - https://nintendowire.com/news/2018/03/21/splatoon-prototype-shown-off-gdc/
    - https://kotaku.com/splatoon-was-originally-a-game-about-rabbits-1823960606
- **Doom: The Dark Ages (Verified):**
  - A critic found purple outlines and neon-green parry light out of place against a muted grey-brown world.
  - The game already lets players recolour the parry light with RGBA sliders and turn off the Daze highlights.
  - Source: https://buttonmusher.substack.com/p/doom-the-dark-ages-has-a-colour-problem
- **Measure:**
  - The median S of world pixels is <= 0.3, so tells land on near-neutral ground.
  - Each role hue is kept as a named constant in one place, so an accessibility remap can swap it (code check, not a
    frame check).

## Hue distance numbers (Reported)

- A just-noticeable difference is about ΔE*ab 2.3 in CIE76.
  https://colour.readthedocs.io/en/latest/generated/colour.difference.JND_CIE1976.html
- CIEDE2000 is about 1 for a just-noticeable difference (calculator documentation): https://metricgate.com/docs/delta-e-ciede2000/
- No source tied a ΔE value to telling categories apart.
- **Proposed (not sourced):** the level accent is ΔE2000 >= 25 from the nearest role colour. The world key is
  ΔE2000 >= 20 from any role colour at matched lightness.

## Not found / not checked

- ULTRAKILL per-layer palettes: no developer source found. Hakita, as reported by 80.lv, says the simple and clean
  geometry makes a room "easy to understand" at a glance.
  https://80.lv/articles/ultrakill-devs-on-the-game-s-mechanics-npc-ai-and-early-access-experience/
- Hades per-biome palettes and its telegraph colour: no primary source found. The pages found were content farms.
- Jet Set Radio per-district palettes: not found.
- Overwatch's GDC 2017 art talk: behind GDC Vault, not checked.
- The TF2 NPAR paper PDF did not parse. Its claims above come from Valve's 2008 interview instead.

## A suggested colour script per level (the world avoids cyan, red-orange, yellow and magenta)

**The dusk-sky risk:** a dusk sky's orange band sits in the red-orange danger band. Keep each level's warm horizon at
S <= 0.35, below the horizon line's top 10 % of the frame, or turn it rose-violet. Measure: sky pixels inside 5-70 deg
at S > 0.35 are 0 %.

- **FOUNDRY: oxide green on black iron.**
  - Soot-iron walls: H 210-220, S 0.1-0.2, V 0.2-0.4.
  - A verdigris and patina green key (H 125-140, S 0.35-0.5) on pipes, cranes and landmarks.
  - Fog: a deep green-grey (H 140, S 0.2).
  - The only orange on screen is molten metal, and it is always a hazard (danger red-orange by role).
- **ICE LINE: ultramarine night ice.**
  - Walls and fog in ultramarine: H 215-225, S 0.35-0.5, V 0.3-0.55.
  - Snow is a warm grey (S <= 0.08), never violet-white, because violet-white is the boundary.
  - Accent: pine green (H 120-135, S 0.4) at 3-5 %.
  - The ice reads by value (white highlights), never by a cyan glow.
- **KILN: cobalt glaze on bone ash.**
  - A warm umber and ash neutral key: H 25-40, S <= 0.18, V 0.35-0.75. This is the brightest level by value.
  - Accent: cobalt glaze (H 222-230, S 0.55-0.65) at 3-6 % on pots and landmarks.
  - Fog: a pale ash haze (S <= 0.1).
  - Fire appears only in hazard vents.
- **SPAN: an indigo-violet open sky.**
  - The sky and fog carry the key: a violet to indigo gradient, H 258-272, S 0.3-0.5, V <= 0.6, kept below the
    boundary's pale violet-white (which sits at V >= 0.85, S <= 0.15).
  - The girders are cool neutral steel (S <= 0.1).
  - Accent: signal-green lamps (H 130-145, S 0.55) at 2-4 % along the route.
- **SPIRE: achromatic marble and soot (the colour-starved climax).**
  - The world is S <= 0.08: white stone and near-black. The sky gradient keeps a little deep blue (H 215-235,
    S <= 0.35).
  - The role colours are the only saturated pixels on screen, the "runner vision" endpoint.
  - The value contrast is the highest of the five levels, still under method 5's cap on the world.
- **Finale (not asked):** it could cycle each level's key in turn as the colour script's resolution.

**Keys in order:** green (about 132), blue (about 220), warm neutral (S <= 0.18), violet (about 265),
achromatic (S <= 0.08).
- The chromatic keys are pairwise >= 40 deg apart in hue. The closest pair is ICE LINE and SPAN at about 45 deg.
- The two neutral keys differ from each other by >= 0.1 in S and by their value range.
- None of the five keys sits inside a reserved band. These are design proposals, not measured on a frame.

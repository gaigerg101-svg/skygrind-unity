# UNIFY: one visual style across SKYGRIND (Unity 6 URP)

Research only, web sources, 2026-10-19. No local code was read. Each claim carries a URL and a mark:
Verified means the page was fetched and the claim read on it; Reported means it came from a search excerpt, a
third-party summary or a forum, and was not confirmed on a primary page. Proposals (numbers or rules this file
suggests) are marked Proposal and are not measured. The Unity manual pages fetched were served as Unity 6.3 to 6.6;
the project is on 6000.3.7f1, so recheck any property that looks different in the Editor.

## The short answer

Unify in this order: one set of colour tokens as data, one global Volume profile that every level shares, a small
per-level override drawn from a colour script, an emissive intensity ladder tied to the Bloom threshold, and one
shared impact kit that every hand-coded effect calls. Then measure frames (luma, saturation, hue per role) before and
after, and judge with blind pairs. Post-processing alone cannot unify a game whose surfaces and effects pick their own
colours; the tokens and the emissive ladder do most of the work, the Volume makes it read as one camera.

## 1. The style guide as data

What the studios do:
- Valve (TF2, L4D): each team has "a general set of colors per teams"; the red team's world uses predominantly warm
  colours and the blue team's cooler ones; L4D's colour correction keeps health packs, blood and exits vivid through "a
  saturation threshold" while everything below it moves toward grey; L4D darkens only the top corners to focus play to
  the centre. Verified: https://www.gamedeveloper.com/art/in-depth-how-valve-makes-art-to-enhance-gameplay
- Riot (League clarity): "The amount of attention an ability grabs should match its level of importance"; noise
  "should be kept minimal"; established shape language carries across skins; things must be clear on every map.
  Verified: https://www.leagueoflegends.com/en-gb/news/dev/clarity-in-league/
- Riot (VALORANT, Moby Francke): lighting, characters, weapons and skills must be "clear no matter what hardware";
  art should "complement but not overshadow the gameplay". Verified:
  https://inverse.com/gaming/valorant-art-style-interview-moby-francke
- Blues and Bullets (GDC 2016): a three-colour palette (white, black, red) run by an "Attention Pyramid" that shaped
  scene design, colour and lighting. Verified (session description): https://gdcvault.com/play/1022986/Art-of-Blues-and-Bullets
- Pixar colour scripts (Ralph Eggleston, from Toy Story): a short version of the film in colour and light; per a quoted
  Lasseter remark, colours got more intense as emotion rose and muted when sombre. Reported:
  https://www.awn.com/animationworld/ralph-eggleston-talks-inside-out and https://hyperallergic.com/610771/the-art-of-pixar-chronicle-books/
- Games use colour scripts too: Insomniac's Jason Hickey covers "Art Directing the colour script" for Miles Morales
  (GDC 2021). Verified (session description): https://gdcvault.com/play/1027294/Art-Direction-Summit-Collaborative-Art

Proposal: one tokens file, loaded once, that every view, material builder, VFX and HUD script reads. Per CLAUDE.md rule
5 these are Unity-only render numbers, so they belong in or behind `UnityTuning.cs` with a comment each. Shape:
- Roles (from CLAUDE.md rule 8): `env` (dusk structure, low saturation), `player` (cyan), `danger` (red-orange, enemies
  and their attacks only), `rival` (magenta), `boundary` (pale violet-white), `pickup` (one hue not used by any other
  role; pick it once), `ui` (text, panels, accents, which reuse player / danger hues for meaning only).
- Each role gets a base hue, an allowed hue window (for example plus or minus 12 degrees), a saturation band and a
  value band in linear luma. Example budgets: env saturation 0.10 to 0.35 and luma 0.03 to 0.35; role colours
  saturation 0.70 to 1.0; nothing in `env` may sit inside a role's hue window at high saturation.
- A value hierarchy, after the Valve and Riot rules above: background darkest and greyest, enemies a clear step brighter
  or more saturated than the wall behind them, player-usable things brightest in their own hue, impacts the only
  things allowed to clip.
- A colour script per level: one row per level with sky top, horizon, fog colour, ambient colour and intensity, a
  temperature shift (warm or cool, small), the level's single accent if any, and the emotional beat (opening, climb,
  boss). Role hues never change between levels; only the environment moves.

## 2. URP post-processing with no custom shader

How Volumes work (Unity manual):
- Every URP scene has two global defaults: the project Default Volume Profile (Project Settings > Graphics > URP) and
  the active URP Asset's Volume Profile; they have the lowest priority and are evaluated only on scene load or quality
  change, not every frame. Verified: https://docs.unity3d.com/6000.0/Documentation/Manual/urp/Volumes.html
- Volume component: Global affects every camera; Local uses a collider with Blend Distance; Weight multiplies the
  contribution; when several Volumes set the same property, the highest Priority wins. Verified:
  https://docs.unity3d.com/Manual/urp/volume-component-reference.html
- Pitfall: script changes to a profile assigned as the Default or Quality profile do nothing, because those values are
  cached. Fix: put a Global Volume in the scene and script that, or call
  `VolumeManager.instance.OnVolumeProfileChanged(profile)`, which costs extra work. Verified:
  https://docs.unity3d.com/Manual/urp/volumes-troubleshooting.html
- URP Asset: Grading Mode High Dynamic Range grades before tonemapping (movie-style precision), Low Dynamic Range after;
  LUT Size default 32; Volume Update Mode Every Frame or Via Scripting. Verified:
  https://docs.unity3d.com/Manual/urp/universalrp-asset.html

The overrides:
- Tonemapping: Neutral remaps range "with minimal impact on color hue and saturation"; ACES is more cinematic and
  higher contrast and does shift hue and saturation. Verified:
  https://docs.unity3d.com/6000.0/Documentation/Manual/urp/post-processing-tonemapping.html
  Proposal: use Neutral. Role colours (cyan versus red-orange) are the game's meaning; ACES bending bright saturated hues
  works against the hue windows above. Grade the dusk look with the controls below instead.
- Color Adjustments: Post Exposure in EV, applied after HDR effects and before tonemapping; Contrast; Color Filter
  (multiplies the render); Hue Shift; Saturation. Verified: https://docs.unity3d.com/Manual/urp/Post-Processing-Color-Adjustments.html
  Proposal: never use Hue Shift or a strong Color Filter per level, since both move role hues; use Saturation to push
  the world back.
- Shadows Midtones Highlights: three trackballs with lightness sliders plus shadow and highlight limits. Verified:
  https://docs.unity3d.com/Manual/urp/Post-Processing-Shadows-Midtones-Highlights.html
  Lift Gamma Gain: lift for dark tones, gamma for mids (power function), gain for highlights. Verified:
  https://docs.unity3d.com/Manual/urp/Post-Processing-Lift-Gamma-Gain.html
  Proposal: the global dusk grade lives here (cool violet shadows, warm highlights), identical in every level.
- Color Lookup: a LUT texture plus Contribution (blend with the ungraded image). Verified:
  https://docs.unity3d.com/Manual/urp/post-processing-color-lookup.html. The LUT must match the URP Asset's LUT size
  (32), and the texture should be imported with sRGB off and no compression. Reported:
  https://gamedevelopmentwiki.readthedocs.io/projects/unitywiki/en/latest/game_architecture/accessibility.html and
  https://learn.unity.com/course/unity-color-lookup/tutorial/applying-color-lookup
  Proposal: skip LUTs in the first pass; the trackballs are data an agent can set by script, a LUT needs an image tool.
- Bloom: Threshold is gamma-space brightness, default 0.9; Intensity 0 to 1 (0 is off); Scatter default 0.7; Clamp
  default 65472; High Quality Filtering costs more; Downscale (Quarter recommended for cost) and Max Iterations are under
  Advanced Properties. Verified: https://docs.unity3d.com/6000.0/Documentation/Manual/urp/post-processing-bloom.html
- Vignette: Color, Center, Intensity, Smoothness (default 0.2), Rounded. Verified:
  https://docs.unity3d.com/Manual/urp/post-processing-vignette.html

Cameras, stacking and Unity 6 pitfalls:
- In a stack, apply post-processing only to the last camera; URP then runs it once and every camera in the stack gets
  the same grade. Verified: https://docs.unity3d.com/Manual/urp/cameras/camera-stacking-concepts.html
- Camera component: the Post Processing checkbox, Volume Mask (which Volumes affect the camera), Volume Trigger, and
  HDR Rendering; Overlay cameras have no Output or Stack section and no anti-aliasing. Verified:
  https://docs.unity3d.com/Manual/urp/camera-component-reference.html
- Post Processing left on an Overlay camera can apply Bloom a second time. Reported (low-reliability blog):
  https://bugnet.io/blog/fix-unity-urp-postprocess-bloom-double-applied
- A Screen Space - Overlay canvas is not post-processed; Screen Space - Camera is. Reported (old forum thread):
  https://discussions.unity.com/t/how-to-post-process-after-screenspace-overlay-canvas/674743
  Proposal: keep the HUD out of Bloom and tonemapping so 16 px text keeps its edges; check this in a Player frame.
- Unity 6.3: Render Graph Compatibility Mode is hidden behind the `URP_COMPATIBILITY_MODE` define and will be removed;
  passes with only `Execute` and no `RecordRenderGraph` do not run. Verified (Unity forum post, 2025-07-17):
  https://discussions.unity.com/t/render-graph-updates-in-unity-6-3/1668122. Built-in Volume overrides need no
  migration; this matters only for custom renderer features.
- Open camera-stack and Render Graph bugs exist in 6000.0 / 6000.1 (gizmos, overlay errors); none found for
  post-processing in 6000.3. Reported: https://issuetracker.unity3d.com/product/unity/issues/guid/UUM-83013

## 3. Lighting consistency

- Fog and ambient are per scene in the Lighting window: Environment Lighting Source (Skybox, Gradient or Color) with an
  Intensity Multiplier 0 to 8; Fog Color, Mode (Linear, Exponential, Exponential Squared), Density, Start and End; fog
  is not available in the Deferred path. Verified: https://docs.unity3d.com/Manual/lighting-window.html
  Proposal: since campaign levels load into one scene, set fog colour, fog density and a Gradient ambient (sky,
  horizon, ground) per level from the colour script row at level load. The script API for this (`RenderSettings`) was
  not checked in this pass.
- L4D lightened its fog because heavy fog hid the silhouettes players needed. Verified:
  https://www.gamedeveloper.com/art/in-depth-how-valve-makes-art-to-enhance-gameplay
  Proposal: fog colour equals the horizon colour of that level, and fog may never reach an enemy at combat range below
  the enemy-to-background contrast budget.
- Light limits: the Forward path allows 9 lights per object (1 main plus 8 additional) and 256 additional lights per
  camera on desktop (32 on mobile). Verified: https://docs.unity3d.com/Manual/urp/lighting/light-limits-in-urp.html
  Forward+ removes the per-object cap but keeps a per-camera cap, and ignores the URP Asset's per-object limit and
  reflection probe blending. Verified: https://docs.unity3d.com/6000.0/Documentation/Manual/urp/rendering/forward-rendering-paths.html
  The Forward+ per-camera figure is 256 on desktop, and raising it lengthens Lit shader compile time. Reported:
  https://docs.unity3d.com/Manual/urp/rendering/forward-plus-rendering-path-limitations.html
  Proposal: lights are a budget per level (one sun, plus a fixed count of role lights); emissive surfaces do the rest.
- Emissive ladder: on the HDR colour picker each positive Intensity step gives twice the light of the previous one.
  Reported (search excerpt of the manual): https://docs.unity3d.com/2022.1/Documentation/Manual/HDRColorPicker.html
  Proposal: four tiers in stops above a base colour, used by every material and effect: T0 world trim (0, under the
  Bloom threshold, no glow), T1 player-usable and pickups (+1), T2 enemy telegraph and danger (+2), T3 impact flash
  only, a few frames (+3). Bloom threshold and intensity are global and never set per level, so the same tier glows the
  same everywhere.

## 4. VFX consistency rules

What the sources say:
- Riot clarity: attention matches importance; projectiles show direction even when still by blurring the tail.
  Verified: https://www.leagueoflegends.com/en-gb/news/dev/clarity-in-league/
- A Riot VFX artist on League missiles: heals are green and must not feel threatening; damaging spells use sharper
  shapes; spells that do more damage are bigger, brighter and more threatening; higher-impact spells get more
  attention-grabbing timing; each spell is rated 1 to 10 for importance; "if everything is important, nothing is
  important". Verified (community post by a Riot artist):
  https://realtimevfx.com/t/anatomy-of-a-league-of-legends-missile-part-2-what-are-we-communicating-with-our-vfx/13062
- Riot's 2017 VFX style guide organised the work into gameplay, value, colour, shape and timing. Reported (the PDF now
  redirects): https://nexus.leagueoflegends.com/en-us/2017/10/dev-leagues-vfx-style-guide/

Proposal, the rules for SKYGRIND's hand-coded C# effects:
- One shape language: player effects are clean lines, rings and capsules (cyan); enemy effects are sharp wedges and
  shards (red-orange), matching the pill language of the enemy models; the boundary is soft planes only.
- One timing set, three curves shared by every effect: snap-in (peak in 1 to 2 sim steps), hold (a fixed number of
  steps per tier), ease-out fade (quadratic). Timers in whole steps through `stepsFor` (CLAUDE.md rule 4).
- Colour temperature by damage tier: low damage leans to the role's cool edge, high damage to its hot edge with a
  white-hot core; the hue stays inside the role window.
- A shared impact kit: flash quad, spark burst, ring, decal, each with tier and role parameters; every hit, kill and
  parry calls it instead of building its own. Hitstop and shake stay where the sim already drives them.
- An importance rating (1 to 10) per effect, and a cap on total effect pixels on screen per frame.

## 5. Checking the result objectively

- Relative luminance from linear RGB: Y = 0.2126 R + 0.7152 G + 0.0722 B; gamma-encoded values must be linearised
  first. Verified: https://en.wikipedia.org/wiki/Relative_luminance
- URP Rendering Debugger: Material Validation (albedo pixels under a min luminance draw red, over a max draw blue) and
  Pixel Validation (highlight values outside a range, per RGB luminance or channel), plus lighting feature toggles
  (emission, ambient, main light). Verified: https://docs.unity3d.com/Manual/urp/features/rendering-debugger-reference.html
- Squint or greyscale test: blur or desaturate a frame; if the focal point cannot be found, the player will not find it;
  most readability failures are value failures. Reported (informal sources):
  https://bugnet.io/blog/readable-game-art-why-clarity-beats-detail and https://www.wayline.io/learn/color-palettes/1

Proposal, the meter (a script over captured Player frames, the same camera spots per level):
- Luma histogram: mean, 5th and 95th percentile, and the share of pixels above the Bloom threshold. Target: every level
  inside one shared band, and the bloom share near zero outside impacts.
- Saturation by role: mean saturation of `env`-coloured pixels versus pixels inside each role's hue window.
- Hue count: bucket hue into 24 bins for pixels with saturation above 0.4; count bins over 1 percent of the frame.
  Target: only role bins plus the level's one accent. Any other bin is an effect or material that bypassed the tokens.
- Contrast check: luma ratio between an enemy and the wall behind it at combat range, per level.
- Squint score: blur the frame, then measure whether enemies and the player's targets remain the local luma peaks.
- Blind pairs: old versus new frames at the same spot, side order shuffled, judged without labels (the project's own
  L-001 rule, docs/memory). A number moving is the proof; a reviewer's adjective is not.

## A 4-hour application plan: the order of changes for the biggest visible gain at the lowest risk

Nothing below touches `Skygrind.Core` or `Mover.cs`; the sim, golden vectors and replays stay as they are.
1. 0:00 to 0:30. Baseline. Capture the same 3 to 4 spots per level in a Player and run the meter. Without this, no
   later step can be shown to help.
2. 0:30 to 1:00. Tokens. Write the role, hue window, saturation and value bands and the emissive tiers into the tokens
   file behind `UnityTuning.cs`. No visual change yet; this is the single source the rest reads.
3. 1:00 to 1:40. One global Volume profile, built by an editor script, used by every level: Neutral tonemapping, HDR
   grading, the dusk grade in Shadows Midtones Highlights, a world Saturation pull, one Bloom threshold and intensity,
   a light Vignette. Post Processing on the last camera of the stack only; confirm the HUD is untouched in a Player
   frame. Biggest visible gain, lowest risk: it is data, it is one asset, and it reverts in one line.
4. 1:40 to 2:30. The emissive ladder. Route every hand-coded material and VFX colour through a token lookup by role and
   tier. This removes the stray hues the hue count found. Medium risk (many call sites); one commit per system.
5. 2:30 to 3:00. The colour script. One row per level: fog colour and density, Gradient ambient, a small temperature
   shift in a per-level Global Volume of higher priority than the shared one. Role hues stay fixed.
6. 3:00 to 3:30. The impact kit. Move hit, kill and parry effects onto the shared kit with tier and role parameters
   and the three timing curves. Leave anything sim-driven (hitstop, shake) where it is.
7. 3:30 to 4:00. Proof. Re-run the meter on the same spots, boot every changed level in a Player (L-043), show blind
   pairs, and record every number as measured or "not checked". Restore the TMP fallback asset before each commit, and
   commit each step with its explicit paths and `.meta` files.

If the time runs short, steps 1, 3 and 7 alone still give one camera look and a measured before and after; steps 4 and
6 are where the game stops looking assembled from parts.

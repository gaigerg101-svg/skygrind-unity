# Scout: procedural animation, VFX colour language, telegraphs (forge72, 2026-10-27)

Web research only; no local files were read. Grades: [P] official doc or the developer's own words, [S] second-hand summary of a talk,
[C] community post or blog (weak), [D] my derivation or suggestion (no source gives the number). "unread" = I could not fetch the page text.

## Ranking for primitive-built bosses (highest impact first)
1. Spring-driven secondary motion on every part (root drives, parts lag, overshoot, settle). One small class gives wobble, follow-through, look-at lag, flinch, death.
2. A three-beat attack: pull-back (anticipation), hold, snap. The tell must be visible at least 16 frames (267 ms at 60 fps) before damage.
3. One hue per meaning, unchanged across phases, plus a rare flash cue for "your window" (ULTRAKILL's rule: the flash is scarce so it stays readable).
4. One hit stack on the hit event: hitstop scaled to damage, flash, knockback, squash.
5. Volume-preserving squash/stretch and idle breathing (cheap, always on, fixes "rigid").
6. Verlet or FABRIK chains for tails, tentacles, legs.
7. Batcher-safe Unity plumbing: per-instance materials, not MaterialPropertyBlock.
8. Automated checks from transform logs and masked frames (section 5).

## 1. Procedural animation that reads as alive
- Second-order system (t3ssel8r): k1 = z/(pi f), k2 = 1/(2 pi f)^2, k3 = r z/(2 pi f). Step: y += dt*yd; yd += dt*(x + k3*xd - y - k1*yd)/k2s,
  with k2s = max(k2, dt^2/2 + dt*k1/2, dt*k1) for stability. [P, code] https://gist.github.com/Andicraft/ab605dda924a0321e755abf8f573ebd7
- Meaning of the three knobs: f in Hz (higher is faster); z = 0 never stops, 0 < z < 1 rings, z = 1 critical, z > 1 sluggish; r < 1 slow start,
  r = 1 immediate, r > 1 overshoots. [P, same gist] Negative r = pull-back before moving (anticipation): I recall this from the video, not re-verified [D].
  Video (page text unread): https://www.youtube.com/watch?v=KPoeNZZ6H4s ; solver is semi-implicit Euler, a large dt blows it up, eigenvalues bound the safe step:
  https://hackaday.com/2022/06/30/a-math-based-personality-for-games/ [S]
- Overshoot of the pure spring (r = 0): PO = 100*exp(-pi z / sqrt(1 - z^2)); I computed z 0.3 = 37 %, z 0.5 = 16 %, z 0.7 = 4.6 %. [D from]
  https://en.wikipedia.org/wiki/Damping_ratio . A boss that reads "rigid" is often z >= 1 on every part; a measurable fault (section 5).
- Start values to A/B (no source gives any): f 1-3 Hz for body and limb follow, 4-8 Hz for flinch and eyes; z 0.3-0.5 jelly/tail, 0.6-0.8 armour plates,
  1.0 aim and camera-like; r 1.5-2 for punchy snaps. [D] Orange Duck's page also says it gives no numeric recommendations.
- Halflife form, exact for any dt: critical spring x = e^(-y dt)(j0 + j1 dt) + c; damping d = 4 ln2 / halflife; stiffness s = (2 pi f)^2;
  damping ratio = d / (2 sqrt(s)); two chained springs each at halflife/2 give an S-curve ease. [P] https://theorangeduck.com/page/spring-roll-call
- Look-at with lag: value = lerp(target, value, exp2(-rate*dt)); rate 1 halves the remaining gap every second. [P]
  https://www.gamedeveloper.com/programming/improved-lerp-smoothing- ; same topic, Freya Holmer's talk (blog unfinished): https://acegikmo.substack.com/p/lerp-smoothing-is-broken
  Use slerp with that factor for head and eye aim [D].
- Principles, as definitions: squash and stretch keep volume constant; loose parts keep moving after the body stops, at different rates; secondary action
  should reinforce the main action, not compete. [P] https://en.wikipedia.org/wiki/Twelve_basic_principles_of_animation
- Squash that keeps volume [D]: stretch s on the long axis, 1/sqrt(s) on the other two (s = 1.3 gives 0.877). Blender's Stretch To binds X and Z to a square root
  (snippet) https://docs.blender.org/manual/en/latest/animation/constraints/tracking/stretch_to.html ; Pixar's squetch blends the preservation amount 0..1 (snippet)
  https://patents.google.com/patent/US7221379 . Drive s from a spring, not a sine.
- Hit stack, one real example (Nuclear Throne pistol, Nijman): camera kick 6 px, shake +4 decaying fast, weapon kick 2, hit pause about 10-20 ms,
  enemy knockback 3 px per frame along the bullet, muzzle flash 1 frame. [S] https://infovore.org/tag/feel/ ; the talk is "The Art of Screenshake":
  https://kenney.nl/learn/must-see-videos-for-indie-developers
- Hitstop scaled to damage (Smash): Ultimate floor(floor(floor((d*0.65+6)*h*e*s)*p)*c), cap 30 frames; Melee floor(d/3+3), cap 20; the victim is held on the first
  frame of its flinch, shaking. 15 damage = 15 frames in Ultimate, 8 in Melee [D from the formulas]. [P] https://www.ssbwiki.com/Hitlag
  Rule: bigger hit, longer freeze; the kill hit gets the longest.
- Blog numbers [C]: hit stop 0.05-0.1 s, flash 50-100 ms (30 ms unnoticed, 150 ms looks blurry), shake 2-3 px weak and 8-10 px strong, 20-30 particles,
  freeze animation state but not the whole world. https://eastondev.com/blog/en/posts/dev/20260521-game-feedback-feel/ . It disagrees with Vlambeer's 10-20 ms: A/B both.
- "Juice it or lose it" (Jonasson, Purho, GDC Europe 2012): tween, stretch and squeeze objects on events so they feel alive. [S]
  https://www.gamedeveloper.com/design/video-is-your-game-juicy-enough-
- Rain World: creatures are physics points under constraints (kelp segments; lizard torso, skull, limbs, tail; long-legs core plus tentacles). [S]
  https://handandmachine.org/classes/computational_fabrication/2024/08/26/introduction-logan-sullivan/ . Slugcat body = two round segments at a fixed separation,
  limbs and tail procedural; Jakobsson: "I never really set out to do procedural animation"; each creature has its own script. [P]
  https://unity.com/blog/exploring-procedural-design-rain-world . The GDC slides were not found.
- Overgrowth: Rosen, GDC 2014, few keyframes plus code that offloads repetitive animation work (video not watched, no numbers). [S]
  https://gamedeveloper.com/design/video-an-indie-approach-to-procedural-animation
- Chains and tentacles. Verlet: x += x - oldx + a*dt^2; a distance constraint moves both ends by 0.5*diff; 3-4 relaxation passes for rigid, 1 for cloth. [P]
  https://www.gamedeveloper.com/programming/advanced-character-physics . FABRIK: backward pass pins tip to target, forward pass pins root, 2-4 passes typical. [P]
  https://docs.vulkan.org/tutorial/latest/Advanced_glTF/Procedural_Animation_IK/03_fabrik.html (paper: https://www.andreasaristidou.com/FABRIK , unread).
- Procedural legs: step when the foot can no longer reach its target, and stagger legs so they never all step together. [C]
  https://assetstore-fallback.unity.com/packages/tools/animation/procedural-spider-267789
- Not found: any Toyful / Hi-Fi Rush talk on enemy timing.

## 2. One consistent VFX colour language
- ULTRAKILL: yellow flash = parryable, blue flash = unparryable; melee parry spark 4-point, ranged 8-point; some attacks (Hell Orb barrages) have none. [P-wiki]
  https://ultrakill.wiki.gg/wiki/Parry . Hakita: the flash is added to some attacks "for visual and audio aid" when it is hard to tell they are attacking. [P]
  https://steamcommunity.com/app/1229490/discussions/0/2951502108448095102 . A player relays that too many flashes would devalue a flash. [C]
  https://steamcommunity.com/app/1229490/discussions/2/3734079155590153286 . I found NO source for "red = unparryable" in ULTRAKILL; the wiki says blue.
  Style: retro visuals "very easy to read at a glance", AI kept simple and predictable. [P] https://80.lv/articles/ultrakill-devs-on-the-game-s-mechanics-npc-ai-and-early-access-experience/
- Doom Eternal (Hugo Martin): "It's all about communication, which is key in a game like Doom"; burning demons drop bright-green armour; glory-kill health is a
  "soft, baby blue"; each ammo type has its own colour; items are bolder because the levels are busier. [P] https://shacknews.com/article/116635/hell-razer-the-making-of-doom-eternal?page=5
  Weak points are described as extra-damage areas; I found no statement that they are colour-coded. [C] https://doomwiki.org/wiki/Destructible_demons
- Riot's League VFX guide (via a summary, official page redirected, unread) [S]: primary elements get high colour value, clear silhouette, high opacity, sharp focal point;
  secondary ones lower. No pure 0 % black or 100 % white; the highest value is only the flash. One dominant hue; the complement is desaturated and limited to small highlights.
  Effect size matches gameplay weight. Anticipation, impact, dissipation, with impact fully active in the first quarter of the lifetime. Leading edge brightest.
  https://www.vfxapprentice.com/blog/10-league-of-legends-vfx-design-tips . No numeric saturation or value bands exist in any source I found.
- Overwatch (community only) [C]: red and blue chosen so harmful red effects differ from harmless multicolour ones
  https://us.forums.blizzard.com/en/overwatch/t/overwatch-league-visual-effects-in-game/593284 ; colourblind options recoloured UI and outlines but ability effects stayed red
  https://eu.forums.blizzard.com/en/overwatch/t/colour-blind-accessibility-issues/30890 ; critique: silhouettes read, teams not distinct enough
  https://80.lv/articles/comparing-team-fortress-2-and-overwatch-art-direction . Lesson [D]: give every colour meaning a second channel (shape, size, motion).
- Valorant: no primary VFX rules found; only a job post naming timing, colour, composition, motion, readability. https://80.lv/jobs/senior-vfx-artist-valorant-riot-games
- Splatoon: inks chosen bright to contrast neutral characters. [S] https://gonintendo.com/stories/305120-nintendo-s-splatoon-panel-at-gdc-2018-another-round-of-developm
  Splatoon 3's neon night Splatfest made blue ink hard to see; red and green inks were shifted toward orange and yellow-green. [C]
  https://automaton-media.com/en/news/20221112-16667/ . Lesson [D]: glow changes the hue you see, so measure after bloom.
- Telegraph colour audit [C]: one primary shape per attack, 3-5 shape families per boss, colour meaning stable across phases (phases may raise saturation or pulse rate,
  red never changes meaning), colour never the only differentiator. https://gamineai.com/blog/enemy-telegraph-shape-language-top-down-boss-fights-fast-visual-consistency-audit-2026
- Only hard numeric floor: graphics need 3:1 against adjacent colour; ratio = (L1+0.05)/(L2+0.05), L = 0.2126R + 0.7152G + 0.0722B on linearised sRGB. [P]
  https://www.w3.org/WAI/WCAG22/Understanding/non-text-contrast.html

## 3. Telegraph timing for fast bosses
- Human limits: simple visual reaction about 190 ms; choice reaction grows as a + b*log2(n+1) (Hick), so more attack types need longer tells. [P]
  https://en.wikipedia.org/wiki/Mental_chronometry . Fighting-game community: average 265 ms is about 16 frames at 60 fps; startup of 16+ frames is reactable "quite consistently";
  15 is "virtually unreactable", 19 close to the limit, 27 easy alone but hard amid other threats; if the first 20 frames show little movement it is impossible to react,
  so the early tell matters more than total length. [C] https://ki.infil.net/reaction.html
- Wind-up ladders [C]: 450-650 ms low punishment, 650-900 ms medium, 900 ms and up high; similar-punishment attacks must keep similar wind-ups or players stop trusting the tell.
  https://gamineai.com/blog/enemy-telegraph-shape-language-top-down-boss-fights-fast-visual-consistency-audit-2026 . A wiki table: dash thrust 0.6 s, sweeping cleave 0.8-1.0 s,
  heavy slam 1.2-1.8 s, tracking beam 2.0 s; phase 3 telegraphs "shortened slightly". https://buildaboss.top/en/guide/build-a-boss-beginner-guide . No authoritative FPS-boss numbers found.
- Cues without audio: raised blade, glowing eye; high-contrast floor indicators (dark busy floors hide red decals) [C] buildaboss link above; charge-up particles gathering at the
  weapon, layered animation plus VFX; fairness test: a hit after a clear telegraph is "not because they didn't know what they were being asked to do". [P]
  https://gamedeveloper.com/design/enemy-attacks-and-telegraphing . Over-long exaggerated wind-ups make players dodge early and eat a delayed follow-up. [C, snippet]
  https://www.wayline.io/blog/the-art-of-the-fakeout-combat-design
- Proposal for this game [D]: tell onset is a large pose change (pull-back of 10 % of body length or 20 degrees) within the first 6 frames; colour/flash peak at 60-70 % of the
  wind-up; light attacks 16-30 frames, medium 0.45-0.65 s, unavoidable-damage attacks 0.9 s or more; the recovery pose carries the cyan "your window" cue.

## 4. Cheap Unity techniques
- MaterialPropertyBlock "is not compatible with SRP Batcher" and likely drops performance in URP. [P]
  https://docs.unity.com/en-us/engine/current/script-reference/unityengine/materialpropertyblock . The Batcher groups by shader variant: "You can still use as many different
  materials with the same shader as you want". [P] https://docs.unity.com/en-us/engine/current/manual/analysis/graphics-performance-profiling/in-urp/reduce-draw-calls-urp/srpbatcher/srpbatcher
  So the flash is a per-enemy material instance made at spawn, with `_Flash` declared in UnityPerMaterial (custom shaders must declare material properties there, snippet)
  https://docs.unity.cn/2022.3/Documentation/Manual/SRPBatcher.html [D]. MPB/Batcher support is only "under consideration": https://activation.unity3d.com/roadmap/75-material-property-block-srp-batcher-compatibility
- Vertex wobble: Shader Graph object-space sine with amplitude, frequency, speed; a gradient mask keeps one end still (their flag keeps its left side fixed, grass keeps its base fixed). [P]
  https://unity.com/blog/engine-platform/creating-animated-materials-with-shader-graph . On capsules mask by object-space Y so feet stay pinned; drive amplitude from a script spring so a hit
  wobble decays [D]. It moves no collider; the game's C# collider is unaffected.
- Spring-driven transforms: the section 1 spring, one struct per channel (scale, euler, position), stepped in the fixed sim loop.
- Emission pulses: URP Bloom Threshold is gamma-space, Intensity 0-1 and default 0. [P] https://docs.unity3d.com/6000.0/Documentation/Manual/urp/post-processing-bloom.html
  Pulse HDR emission with a decaying spring; cap the peak below where bloom washes the hue to white (check in section 5) [D].
- Decals: the URP Decal Projector needs the Decal Shader Graph material, does not project on transparent surfaces, and "does not support the SRP Batcher by design"; use GPU instancing or an atlas. [P]
  https://docs.unity3d.com/6000.0/Documentation/Manual/urp/renderer-feature-decal.html . A flat unlit ring quad is cheaper for ground telegraphs [D].

## 5. Measurable checks (all [D]; building blocks cited)
Transform log (per sim step, per part: position, rotation, scale):
1. Idle alive: RMS displacement of each part from its rest pose over 10 s idle must exceed a floor calibrated on today's build; near 0 = rigid.
2. Tell: in the frames before each attack's damage frame, the tell part's speed or silhouette bounds change must exceed idle RMS by a set ratio, and the first frame over the
   threshold must be at least 16 frames before damage (https://ki.infil.net/reaction.html).
3. Anticipation: dot(pull-back velocity, strike velocity) < 0 for at least k frames before strike peak.
4. Follow-through: a child's settle time after its parent stops must be above 0, and its measured overshoot must match the PO formula for its configured z (https://en.wikipedia.org/wiki/Damping_ratio);
   flags parts accidentally set overdamped.
5. Timing ladder: wind-up per attack falls in its tier band, and same-tier attacks agree within about 15 %.
6. Colour registry: dump base and emission colour of every boss, VFX and telegraph material per state, convert to HSV or LCh, assert hue is inside its meaning's band
   (cyan player, red-orange enemy, magenta rival only, pale violet-white boundary) and unchanged across phases (rule from the gamineai audit above).
Frames (render the boss on an object-ID or layer mask so the arena does not pollute the numbers):
7. Motion read: Quantity-of-Motion-style score = mean absolute frame difference inside the mask per mask area (concept: EyesWeb QoM from silhouette motion images; exact formula
   unverified) https://casapaganini.unige.it/node/8 ; or dense optical-flow magnitude, https://docs.opencv.org/4.x/dc/d6b/group__video__track.html (unread, 403). Require tell-window score >= k * idle score.
8. Hue family on the post-bloom frame: convert to HSV (OpenCV hue 0-179, https://docs.opencv.org/4.x/df/d9d/tutorial_py_colorspaces.html , unread) and require at least X % of masked pixels in the
   allowed band; also require near 0 % danger hue on non-enemy pixels. Compare pre- and post-bloom (Splatoon lesson).
9. Distinctness: CIELAB difference between two meanings' pixel clusters must be far above one JND (CIE76 about 2.3) https://en.wikipedia.org/wiki/Color_difference ; the "far above" figure
   is unsourced, calibrate on today's frames. Boundary vs neighbour must pass 3:1 (W3C link above).
10. Danger reads first: in the tell window the highest contrast-times-area cluster must belong to the tell.

## Gaps (searched, nothing usable)
Valorant and Overwatch primary VFX rules; Toyful / Hi-Fi Rush; Rain World and Overgrowth slide contents; negative-r wording in t3ssel8r's video; colour-coded Doom weak points;
ULTRAKILL red un-parryable; any numeric saturation or value hierarchy. Most telegraph numbers are blogs: playtest, do not treat as standards.

# FORGE64 research: role colour under grading, boss readability, one HUD order (2026-10-23)

Scout: web only (WebSearch / WebFetch, about 15 min). No repo file was read except the brief. "Not verified" means I looked and
could not confirm it from a primary page. "Inference" means my own reasoning from the cited facts, not a source's claim.

## 1. Keeping a role colour constant under per-scene grading (Unity 6 URP)

Why the cyan turns spring green (facts, then inference):
- URP's Color Adjustments **Color Filter** "multiply[s] the render and tint[s] the result"; **Post Exposure** is applied "after the HDR effect and before tonemapping" (https://docs.unity.com/en-us/engine/6000.3/manual/post-processing-and-full-screen-effects/urp/effect-list/color-adjustments.md).
- **Split Toning** "tints different areas of the image based on luminance values" (shadows vs highlights, with a Balance slider) (https://docs.unity.com/en-us/engine/6000.3/manual/post-processing-and-full-screen-effects/urp/effect-list/split-toning.md).
- Inference: a multiply filter and a highlight tint touch every pixel drawn before the grade, at any emissive intensity. A brighter (HDR) cyan
  lands deeper in the highlights, where the sulphur tint is strongest, so "more emission" makes the drift worse, not better.
- **Tonemapping:** Neutral gives "range-remapping with minimal impact on color hue & saturation"; ACES "has an effect on actual color hue &
  saturation" and URP then "does all the grading operations in the ACES color spaces" (https://docs.unity.com/en-us/engine/6000.3/manual/post-processing-and-full-screen-effects/urp/effect-list/tonemapping.md).
  Bright saturated blues drifting to violet under ACES is a known complaint (https://discussions.unity.com/t/unitys-aces-implementation-is-outdated/1567683; https://github.com/godotengine/godot/pull/52476).

Where grading happens in the frame (URP source, master branch):
- `ColorGradingLutPass` is queued at `RenderPassEvent.BeforeRenderingPrePasses` (it bakes the LUT early); `FinalBlitPass` is at
  `AfterRendering + 1` (https://raw.githubusercontent.com/Unity-Technologies/Graphics/master/Packages/com.unity.render-pipelines.universal/Runtime/UniversalRenderer.cs).
- In the Render Graph path, `m_PostProcess.RenderPostProcessing(...)` (the main post / uber pass) is recorded first, then the custom passes at
  `AfterRenderingPostProcessing` (only if `hasPassesAfterPostProcessing`), then `RenderFinalPostProcessing` (FXAA, upscaling; "FinalPost pass is
  guaranteed to execute after user authored passes"), then the final blit (https://raw.githubusercontent.com/Unity-Technologies/Graphics/master/Packages/com.unity.render-pipelines.universal/Runtime/UniversalRendererRenderGraph.cs).
- Conflict: the `RenderPassEvent` comment (value 600) says AfterRenderingPostProcessing runs "after rendering post-processing effects but before
  final blit, post-processing AA effects and color grading" (https://raw.githubusercontent.com/Unity-Technologies/Graphics/master/Packages/com.unity.render-pipelines.universal/Runtime/Passes/ScriptableRenderPass.cs;
  same wording in https://docs.unity3d.com/Manual/urp/customize/custom-pass-injection-points.html). The source order says the uber pass (which
  applies the LUT) runs before it. **Not verified which is true in 6000.3** (I did not read the `PostProcess` class): a Player frame with the grade
  on vs off must decide it (rule R8).

Mechanisms, cost and risk:
| # | Mechanism (API names) | What it buys | Costs / risks (sourced) |
|---|---|---|---|
| A | **Render Objects** renderer feature (`RenderObjects`): Event = `AfterRenderingPostProcessing`, Filters > Layer Mask = a role layer, Overrides > Material = unlit role material, Depth Test = LessEqual, Write Depth off (https://docs.unity3d.com/Manual/urp/renderer-features/renderer-feature-render-objects.html) | Role marks drawn after the grade, no code | Depth bug UUM-88979 (no depth bound at this event; fixed 2022.3.24f1) (https://issuetracker.unity.com/issues/15074/renderobjects-does-not-have-any-depth-texture-bound-when-event-is-set-to-afterrenderingpostprocessing); a second report closed "by design": depth "can be jittered from Post-processing and not be available" (search-result summary of issuetracker-mig, page 502 when fetched: not verified); objects after post still got TAA in one thread (https://discussions.unity.com/t/urp-render-after-taa/947670); Shader override mode "is less performant and isn't compatible with the SRP Batcher". Inference: these pixels get no bloom, and FXAA (FinalPost) still runs over them. |
| B | Custom `ScriptableRendererFeature` + `ScriptableRenderPass` with `renderPassEvent = AfterRenderingPostProcessing`, drawing a renderer list in `RecordRenderGraph` (https://docs.unity.com/en-us/engine/6000.3/manual/render-pipelines/universal-render-pipeline/customizing-urp/render-graph/write-render-pass) | Same as A, plus a per-boss role table in code | The draw-objects page (render-graph-draw-objects-in-a-pass) failed to load (DNS), so exact calls (`CreateRendererList`, `SetRenderAttachment`, `SetRenderAttachmentDepth`, `UniversalResourceData.activeColorTexture`) are **not verified** here. `AddUnsafePass` exists but "URP can't optimize those passes" (search summary of the unsafe-pass page). |
| C | **Overlay camera** with Post Processing off, its own Culling Mask, optional **Clear Depth** ("clear depth from previous camera") (https://docs.unity.com/en-us/engine/6000.0/manual/cameras/urp/camera-components-reference/camera-component-reference.md) | Role layer composited after the base camera | Unity: "You should only apply post-processing to the last camera in the stack" (https://docs.unity.com/en-us/engine/6000.0/manual/cameras/urp/multiple/camera-stacking-concepts.md); whether base-with-PP + overlay-without-PP draws the overlay after the grade in Unity 6: **not verified**. Forum reports 1-1.5 ms per overlay camera, 2.5-3.5 ms with empty overlays (https://discussions.unity.com/t/reducing-rendering-cost-of-secondary-camera/913279). Highest cost, most moving parts. |
| D | Grading mask (stencil exemption inside the post shader) | Exact exemption | URP has no switch; HDRP users report "you need to modify HDRP package" (https://discussions.unity.com/t/how-to-exclude-post-processing-effects-from-certain-objects/860152); Unreal's version writes "the unchanged scene color" where a stencil matches (https://answers.unrealengine.com/questions/119242/view.html). Too big for 3 h. |
| E | Per-scene **role table**: per boss grade, author the role emissive so the graded result lands in band (inference) | Zero render risk, keeps bloom | Must be re-measured whenever a grade changes; still not exact across luminance (split toning is luminance-keyed). |
| F | Limit the grade: lower Split Toning highlight saturation / Color Filter strength on the boss volumes, or Neutral instead of ACES | Fastest | Changes the boss mood; a look change GG may want to see. |

## 2. Enemy and boss readability in fast action games

- **Riot, clarity's "Unbreakable Vows":** "gameplay should be clearly conveyed", "always preserve hierarchy", "noise should be kept minimal"; "the
  amount of attention an ability grabs should match its level of importance"; a skin must be "at least as clear as the base skin" and "clear no
  matter which map you're on" (https://www.leagueoflegends.com/en-us/news/dev/clarity-in-league/). Inference: a boss's theme grade is a "skin" and
  must not change what a role colour means. The page gives no colour or contrast numbers.
- **Riot VFX style guide (2017):** five areas, "gameplay, value, color, shape, and timing"; primary elements get "a high value range, strong contrast
  in value or saturation, a clear silhouette" (search summary of https://nexus.leagueoflegends.com/wp-content/uploads/2017/10/VFX_Styleguide_final_public_hidpjqwx7lqyx0pjj3ss.pdf, PDF not opened).
  Secondary summary: colour meanings stay fixed ("healing ... always ... greener", "frost ... always more blue"); environments sit at "a low, muted
  saturation baseline", characters get "a wider, higher saturation ceiling" (https://www.vfxapprentice.com/blog/10-league-of-legends-vfx-design-tips).
- **Valve, TF2 (Moby Francke):** teams read by colour first ("differentiate the characters through the colors alone"), class by silhouette ("very
  unique shapes"), detail last; detail concentrated "up towards the chest", a value gradient "from the dark bottoms to the contrasting upper torso",
  rim light to separate figures, silhouettes checked in black and grey; no numbers (https://gamedeveloper.com/art/in-depth-how-valve-makes-art-to-enhance-gameplay).
  The NPAR 2007 paper (https://cdn.akamai.steamstatic.com/apps/valve/2007/NPAR07_IllustrativeRenderingInTeamFortress2.pdf) downloaded but its text
  could not be extracted: not verified.
- **ULTRAKILL (Hakita):** "visuals are very easy to read at a glance"; "as soon as you look at a room it's easy to understand it with no details or
  decorations distracting or obfuscating it"; AI "purposefully simple to make the enemies predictable" (https://80.lv/articles/ultrakill-devs-on-the-game-s-mechanics-npc-ai-and-early-access-experience/).
- **DOOM Eternal:** destructible weak points (Revenant rockets, Mancubus arm cannons, Arachnotron turret) as tactical targets
  (https://www.redbull.com/gb-en/doom-eternal-all-you-need-to-know-tips; https://www.gamestar.de/artikel/doom-eternal-daemonenzerstoerung-soll-kaempfe-taktischer-machen,3333370.html).
  A single weak-point colour across all demons: not verified.
- **Destiny:** yellow damage numbers mean a critical (precision) hit, white normal (guide, not Bungie: https://www.trueachievements.com/game/Destiny-2/walkthrough/2).
- **Overwatch:** enemy red aura/nametag, ally blue (https://blog.prototypr.io/how-overwatch-leagues-ui-is-an-esports-game-changer-9db218c0d466);
  "effects stay red and blue for enemy and allied" is a forum reply, not an official statement (https://us.forums.blizzard.com/en/overwatch/t/overwatch-league-visual-effects-in-game/593284).
- **VALORANT:** the enemy highlight colour is a player setting (red default, purple, two yellows for colour-vision types) (https://www.thespike.gg/news/how-to-change-enemy-color-in-valorant/3318).
- **Returnal:** bioluminescent enemies "tie in nicely to gameplay readability"; "bright high contrast ... set against a darker, bleaker background" (https://www.thesixthaxis.com/?p=375524).
- **Accessibility numbers a meter can use:** WCAG 1.4.11: at least **3:1** contrast for graphical objects against adjacent colours
  (https://dequeuniversity.com/resources/wcag2.1/1.4.11-non-text-contrast). Xbox XAG 103: "aim to have no cue depend on color alone";
  "Color-conveyed critical content is also expressed through shape, pattern, icon, or text label"; ally vs enemy marker colours player-configurable
  (https://devdocs.xbox.com/gaming/accessibility/xbox-accessibility-guidelines/103).
- **Hue separation in degrees:** no published threshold found; palette tools maximise the minimum pairwise CIEDE2000 distance and treat any floor as
  a design choice (https://www.theoj.org/joss-papers/joss.08936/10.21105.joss.08936.pdf). Any degree band below is a project rule, not a standard.
- **Boss size on screen / arena framing numbers:** none found. Only qualitative: God of War moved the camera closer on a small foe so hits felt
  as big as giant bosses (https://80.lv/articles/behind-the-scenes-of-god-of-wars-first-baldur-fight); big bosses fail when the camera shows
  "ankles" (player forum, anecdotal: https://steamcommunity.com/app/1245620/discussions/0/4343240316586917334). Screen-share band: not verified.

## 3. HUD instruction priority (one order at a time)

- Attention is the bottleneck: Hodent's Fortnite UX talk lists "attention is scarce" and "we can hardly pay attention to tutorial texts when we are
  busy with the game's combat" (search snippet of https://interaction19.ixda.org/download/talk-the-ux-of-fortnite-celia-hodent.pdf; PDF too large to open).
- Riot's hierarchy rule (section 2) applies to UI too (inference): the most important order gets the most attention, once.
- A documented Unreal prompt manager: prompts shown **one at a time** from a queue; a new high-priority prompt clears queued prompts of its class
  but "always preserves the current on-screen prompt"; an "add if type does not exist" mode drops a prompt already showing or queued (dedupe)
  (https://gamesbyhyper.com/docs/ui/information-prompt/). Its HUD: when entries claim the same slot index, "the entry with the higher Priority
  wins" (https://gamesbyhyper.com/docs/ui/hud/).
- Pattern (inference from the above): one **order slot** owned by one arbiter; every widget asks the arbiter instead of printing; orders keyed by
  id, so the boss-bar subtitle and the centre callout cannot both show order X. A shipped AAA game documenting this: not verified.

## 4. What a 3-hour run can fix, ranked by value per hour

| Rank | Problem | Fix | Time | Risk |
|---|---|---|---|---|
| 1 | (e) one order printed twice | Single order arbiter: dedupe by order key; the boss-bar line hides while the centre callout shows | 0.5-0.75 h | Low; check every HUD state that used either widget (tutorial, boss intro) |
| 2 | (b) danger-red line on the exit door | Recolour that one line / move it off the door on that level | 0.25-0.5 h | Low; re-run that level's gates and a reel |
| 3 | (c) bosses wearing player cyan | Palette table: no cyan-band material on boss parts unless the part is a player-usable weak point | 0.75-1 h | Medium: boss colours are enemy design, which GG owns (project rule 7); frame it as the colour rule, list each changed part |
| 4 | (a) weak-point cyan goes 158 under the grade | Cheapest: option E (per-boss compensation) or F (soften the highlight tint); exact: option A (Render Objects at AfterRenderingPostProcessing on a role layer) | E/F 0.5-1 h; A 1.5-2 h | E/F low; A medium (depth/TAA issues, no bloom on marks, ordering comment conflict: needs a Player frame grade-on/off) |
| 5 | (d) boss unreadable in 11/18 frames | Per named case: cap the ring's screen coverage/alpha, give the flat-square boss depth/rim, remove the wire-in-sphere layering; blind re-read | 1.5 h+ for 2-3 cases | High: judged blind, likely partial; do last |

## Rules a prompt can use

- R1 Role hue: player cyan on the final Player frame lies at hue 180-186 on every level and boss frame (158 fails). Measured on pixels, not on the material.
- R2 Cyan is yours: inside each boss's screen mask, cyan-band pixels = 0 except on parts the player can use (weak points); list those parts by name.
- R3 Red never on yours: red-orange-band pixels inside a player-usable object's screen box (exit doors, pads) = 0.
- R4 Contrast: a role mark vs its adjacent pixels has luminance contrast at least 3:1 (WCAG 1.4.11) on every boss frame.
- R5 Not colour alone: every role cue also has a shape or icon (XAG 103); test: the greyscale frame still shows the weak point (Valve's grey-silhouette check).
- R6 Grade-proof: toggling the boss grade off/on moves the role mark's hue by at most 3 degrees (proposed tolerance, not a standard).
- R7 One emission rule: raising a role colour's emission is not a fix under a highlight split-tone; the fix is drawn after the grade or compensated per grade.
- R8 Order of passes: before relying on AfterRenderingPostProcessing, one Player frame proves the mark there is ungraded (the URP comment and source disagree).
- R9 One order slot: across every frame of a boss fight, an order key is visible in at most 1 widget (duplicates = 0).
- R10 Hierarchy: while a centre order shows, no second order text is visible anywhere on the HUD.
- R11 Boss readable: blind reads of the boss frames reach at least 15 of 18 "I can see the boss and its weak point" (was 7 of 18); target, not a sourced number.
- R12 Boss in frame: the boss mask is visible on every boss frame and no single boss effect (ring, shell) covers more than 40 % of the frame (proposed, not verified).

# NIGHT49 builder brief (the lead's; read it whole, then NIGHT49.md sections 0-4, DECIDE49.md, and your G row)

Untracked, like NIGHT49.md. **GG's words are at the top of C:\Users\gaige\dev\SKYGRIND-unity\NIGHT49.md: read them twice; they outrank
everything.** Every call is made in C:\Users\gaige\dev\SKYGRIND-unity\DECIDE49.md (read it by absolute path; you never write it).
Four builders. Your letter, your row:

| you | owns | row (pre-placed, off) | rows | first read |
|---|---|---|---|---|
| H | the charge element and the HUD's colours: UiHud38 (arc40 / meter42 re-gated), UiHud33 (the reticle), UiRail34 (the BEND), UiHud47 panel colours, UnityTuning.H49.cs (sizes only) | `charge49` (view) | G1 | Research/2026-10-19-wave49/CODE-FACTS.md section 1; ULTRAKILL.md "weapon charge"; DOOM.md HUD |
| V | the environment grade and colour script: UnityTuning.V49.cs (per-level values), Look33 / Look41 per theme, PostFx.ApplyTheme33, fog, ambient, key and fill, the stone vertex colours | `look49` (view) | G2, G3, G4 (environment) | CODE-FACTS sections 2-4; UNIFY.md; SPIKE.md; LOOK-NOW.md |
| L | the level lights in their roles: LightKit29's fixture colour map, each level's landmark lit as its room's strongest light, LightPlan33's emissive fixture tiers; UnityTuning.L49.cs | `lights49` (view) | G2, G3, G4 (lights) | CODE-FACTS sections 3-4; the level JSON (pads, grappleNodes, zoneParts) |
| X | one impact kit: UnityTuning.Fx49.cs (tiers light / heavy / critical x owners player cyan-white, gauntlet pink, enemy red-orange, parry yellow; sizes and timings in whole sim steps); every hit, kill, parry, blast, flame and torch reads it | `fx49` (view) | G5 | CODE-FACTS section 5; UNIFY.md VFX; ULTRAKILL.md hitstop / parry |

## 0. The clock
t0 = 11:44 MST on 2026-10-19 (by `date`). Round 1 ends when you report (~12:05 to 13:00). The lead builds `n49-r1` at ~13:00 and
`n49-r2` at ~14:30. The freeze is at ~14:50: nothing after it. **Work through your whole list without stopping. One report per round.**
A SHORT with a measured reason beats a late GREEN. **Commit every slice** (the spend limit has stopped every builder at once before).

## 1. Who you are
One of four builders on SKYGRIND (Unity 6000.3.7f1, URP; a first-person movement shooter at dusk; pad-first for Steam and Switch). You
work in your own git worktree, a branch from `port/stage3` at the NIGHT49 shared commit (the lead tells you its sha). The lead alone drives
the Unity Editor, builds, Players, reels, the suite and git on `port/stage3`. You own only your files (the table above; your own new
files named with your id).

## 2. Hard rules (CLAUDE.md, restated)
- **Never** run the Unity Editor (no `unity-loop.sh`, no `unity` commands, no Play), a build, a Player, a reel, `run_tests`, `unity test`,
  the batch suite. Your checks: `dotnet run --project Tools/CoreHarness -c Release` (golden), `dotnet run --project Tools/SimHarness -c
  Release -- <cmd>` (from your worktree root), `bash Tools/night/dn-check-wt.sh <scratchDir>` (a directory of YOUR OWN under your
  scratchpad). The lead runs every Player check: name the frames you need (reel phase, level, room, moment) in your report.
- **Git:** never commit to `port/stage3` or `master`, never push; `git merge` / `git rebase` are blocked: the lead cherry-picks your
  commits; read port/stage3 with `git show port/stage3:<path>`. Commit in YOUR branch with explicit paths (+ a `.meta` for every new file
  under Assets/: `fileFormatVersion: 2` / `guid: <fresh 32-hex>`), never `git add -A` / `.`. One commit per slice. Never touch GG's four
  files (ProjectSettings/UnityConnectSettings.asset, ProjectSettings/EditorBuildSettings.asset,
  Assets/ProjectSettings/UIEffectProjectSettings.asset, Assets/Scenes/Arena.unity), **never** HANDOFF.md, NIGHT49-LOG.md, DECIDE49.md,
  NIGHT49.md, NIGHT49-BRIEF.md. Trailer: `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- **Builders cannot message each other.** Hand-offs go in your report; the lead passes them on.
- **Movement exact:** you touch no `Skygrind.Core` movement code tonight. Your work is view only.
- **Sim parity:** before each code commit, `fingerprint`, `fingerprint --off48` and `fingerprint --off49` all EQUAL.
- **The one palette:** `UnityTuning.Look49.cs` (the lead's; the role tokens) is the only colour table. Your file holds sizes and timings
  only. To change a token, write the proposal in your report; the lead decides. Never edit the web's `colors` / `fx` blocks in
  `PortExport/config.json`.
- **Rows:** your row is pre-placed in `design.n49` (enabled false). Read it with `NightRows.ViewOn(cfg, "<row>")`. You never set
  `enabled: true`. Numbers go in `design.n49.<row>.numbers` with a `_why` per number, patched by a `.cjs` written with the Write tool (never
  `node -e`, never a heredoc), touching ONLY your row. Render numbers go in `Assets/Scripts/Game/UnityTuning.<Name>49.cs`, each with a
  comment. Never edit a row of design.n27 to design.n48.
- **Look (rule 8):** cyan = player-usable; red-orange = enemies and danger; yellow = punch it / parryable; magenta = RIVAL only; pale
  violet-white = the boundary; the gauntlet pink stays apart from RIVAL's magenta. UI text >= 16 px at 1080p; the theme fonts. The level
  accents stay >= 20 deg of hue from every role token (Look49AccentMinHueGapDeg) and below the roles' saturation. No effect over the whole
  screen; never over the reticle's centre box (the 18 px box). No new full-screen shader (DECIDE47 D-13); URP's built-in Volume overrides
  are allowed. A new material or shader goes in Resources; its Player frames are checked for magenta.
- **GG owns enemy design (rule 7):** no change to an enemy's or a boss's model, shape or colours tonight.
- **Randomness:** only from `Mulberry32` in sim code; view randomness from a separate seeded cosmetic generator. No `UnityEngine.Random`
  in sim code. No Unity API in MonoBehaviour field initializers (L-035).
- **Nothing lost (PRINCIPLES 1):** a change that touches a verb names its VERBS.md row in your commit and report.
- **No audio.** New gameplay events get web-style names, listed in your report.
- **Feel is seen (L-001):** write your PROOF first: the numbers and the frames before / after. You never write GREEN; the lead does, after
  looking at the frames and the blind pair.
- **Docs** with the Write / Edit tools only; write only what was measured or read, otherwise "not checked".
- **Disk:** keep scratch output under your scratchpad.

## 3. Your row's proof (the lead runs the Player; you name the frames)
- **Reels:** your reel phase is in `FeelReel.N49.cs` (a149 H, a249 X, a349 V, a449 L), filled by your own partial file
  `FeelReel.N49.<Name>.cs` (Charge / Fx / Look / Lights). A reel that presses a button uses `sim.UseInput(g.playerInput)` and prints
  `fires` / `acted` (NIGHT48's lesson: no action = NO READING).
- **Blind pairs:** the lead runs them (3 fresh judges each); you give the two images' paths and the question.
- **Harness commands:** your own `Tools/SimHarness/Harness.N49.<Name>.cs` with a `[ModuleInitializer]`.

## 4. Your G rows
- **H (G1):** one element under the reticle for every charge source: KESTREL charge, THUNDERCLAP slug, the PULSE LT blast, the BEND, and
  the STITCHER heat (it was arc40's); never two at once; in the active weapon's colour from Look49's tokens; filling with the charge; one
  2-frame white flash at full; a cooldown as a dim refill; hidden when the weapon has no charge or heat. The label stays in the weapon
  panel. Build two candidates: (a) a thin arc, (b) DOOM's segmented pips. The lead picks one on round 1's frames.
  Acceptance (the lead's reel a149): each source at 0 / 50 / 100 % and its cooldown at 1920x1080 and 1280x720. Your line prints the
  element's nearest pixel to the screen centre (>= 18 px at 1080p: outside the reticle box), its height (>= 4 px at 1080p), and that it is
  hidden with no charge. The blind pair: the picked candidate vs NIGHT46's arc40: "which charge display is clearer and better placed?"
- **V (G2-G4, environment):** per level one dominant environment hue on a dark base; the saturation below the roles'; >= 20 deg from every
  role hue (L1's lava stays the danger: its walls must not be the danger hue); Neutral tonemapping; ShadowsMidtonesHighlights toward the
  level hue; a contrast lift; fog colour + density and the trilight ambient per level; floors darker than walls, walls darker than the sky;
  the HUD outside the grade (the HUD's cyan reads exact on every level's frame).
  Acceptance: the look meter (`python -I Tools/night/n49-lookmeter.py <label>=<png> ...`) on the same five frames per level before and
  after: a combat frame dark >= 35 %, mid <= 55 %, saturated 10-30 %, hue bins 3-5 (hypotheses; V logs any change); 0 magenta pixels.
  The blind sheet: five rows (one per level), old vs new in each row; 3 judges per row: "which looks more like a polished, high-contrast
  action game?"
- **L (G2-G4, lights):** first the test: a fixture is "usable" when it is within 1.5 m of a pad, a grapple node, or a ride / climb surface
  in the level JSON (L names the fields it uses and logs the count per level). Then cyan only on usable fixtures; white only on the
  boundary (pale violet-white); every other fixture takes the level accent from Look49's tokens or a dim neutral; each level's landmark (the
  WICK's furnace, the KILN's core, the LOOM's warp, the SPIRE's column, level 2's) lit as the room's strongest light; emissive tiers that
  agree with V's bloom threshold.
  Acceptance: per level the fixture counts by colour before -> after (cyan off every non-usable fixture: 0; white off every non-boundary
  fixture: 0); the look meter's cyan share; the blind sheet (five rows, old vs new, 3 judges): "which arena reads more clearly: where to go
  and what to use?"
- **X (G5):** UnityTuning.Fx49.cs: the tiers x owners table (size, brightness, shape; colours from Look49) and one timing table (rise / hold /
  fade in whole sim steps; the hitstop stays the sim's); every effect file reads it with fx49 on. More damage = bigger, brighter, sharper
  (UNIFY.md); "if everything is important, nothing is important".
  Acceptance: reel a249: one contact sheet per state (fx49 off / on), each effect at its peak (KESTREL tap and charge, THUNDERCLAP shot /
  slug / boost, PULSE punch / blast, the fist, the TORCH, a kill, a parry, an enemy hit on the player, a boss weak point); your line prints
  per effect its tier, owner colour and peak frame. The wash meter GREEN (no frame's WASH box > +0.25 over f-2 in the middle 20 % of the
  screen: 384 x 216 px at 1080p). The blind pair of the two sheets: "which set of effects looks like one game?"

## 5. Reports (each round)
Under 300 words: each G row's state (not started / building / BUILT / SHORT) with its measured number (BUILT / MEASURED, never GREEN: the
lead writes GREEN); every commit sha; every check with its measured result (golden, fingerprints, dn-check); the frames the lead must take
(reel phase + what); the proposals for tokens; hand-offs; new event names. Last line exactly:
`N49-BUILDER <id>: round <r> · rows BUILT <list> SHORT <list> · commits <shas>`

# NIGHT49.md: one look for the whole game. The charge bar back under the reticle, one palette and colour script across all five levels, one impact kit for every effect, the level lights in their roles, in the ULTRAKILL / DOOM spirit; NIGHT48's freeze first (lead + four builders; 4 h, hard cap 4 h from GG's go)

GG, 2026-10-19 (~11:20, after NIGHT48's close), verbatim. **This text outranks everything below it. Read it twice.**
> The charge bar under the AIM reticle needs to come back I don't know why it's gone also we just need a visual polish all across the
> game whether it's lighting whether it's design whether the arenas it's just the colors and stuff are way too bland The effects are
> inconsistent it all needs to be unified so we need to research and figure out the best way to not only make it unified but actually
> apply those changes across the entire game Umm and again I'm i'm very inspired by Ultra Kill and Doom and those types of games I wanted
> to have more of that effect so we gotta research how to do it and craft the next detailed prompt after you plan this next long prompt
> which will go for 4 hours

**The charge bar is a reversal.** On 2026-10-18 GG wrote "Remove the strange blue bar under the aim reticle and the charged attack indicator
because it's unclear and misplaced" (NIGHT47 G19; hud47 hid arc40 / meter42). It comes back tonight, and the 10-18 complaint is the spec:
clear and in place, not the old blue arc as it was.

**GG will not answer during the night.** Every call goes in `DECIDE49.md` (untracked; the Write tool; never a builder's file). Base:
`port/stage3` at `u3-n48` (a8fe95cd); check it equals HEAD, else base on HEAD and log it. t0 = GG's go. The lead writes `date` on every
log line. Research is done: `Research/2026-10-19-wave49/` (GG-REQUEST, LOOK-NOW + look-now.png, CODE-FACTS, ULTRAKILL, DOOM, UNIFY,
SPIKE + lookmeter.py + montage.py, SHORTLIST, FORGE-NOTES with the reviewer's fixes). Read CODE-FACTS and SPIKE before G0.

Rows tonight (G1-G6 = GG's asks): G1 the charge bar (H); G2 lighting, G3 arenas, G4 colours (V and L); G5 one effects family (X); G6
the ULTRAKILL / DOOM effect (all four, judged together at the close); plus P0's NIGHT48 rows.

## 0. The rules NIGHT47 and NIGHT48 taught (hard rules tonight)
| lesson | rule |
|---|---|
| NIGHT48 closed without its freeze; every n48 row stayed OFF | **P0 is NIGHT48's freeze**; the polish is judged on a build with the n48 rows that pass ON |
| The usage limit stopped NIGHT47 for 3 h | a STOP POINT line at the top of NIGHT49-LOG.md and HANDOFF.md at every build and every 30 min, with the close table so far, committed |
| Rows looked GREEN to a builder and RED on frames (the worm's body, the swarmer at 20 m, the LOOM saucer) | no row closes without the lead's look at its Player frames, the path in the log; builders report BUILT / MEASURED, only the lead writes GREEN |
| A meter read zero for two builds: the reel's pad had lost focus (n48-r3 / r4) | every reel that presses uses the n33 recipe (IgnoreFocus, the pad enabled before each press) and prints `fires` / `acted`; no action = NO READING |
| The recorder overwrote f1799.jpg (20-110 failed writes a long reel) | the lead fixes FeelReel.cs Capture in the shared commit (skip a frame whose index did not advance) |
| Blind pairs decided four looks and retired one (the drone, three losses) | every look ends in a blind pair (3 fresh judges, the key read after the votes); a new look that loses twice is retired |
| A config row without `"sim": true` never registers (loomReach48) | only the lead's shared script writes rows and their flags |
| The smoke fails when a window takes focus; the flow table and the suite flake beside heavy jobs (L-005, memory) | the smoke, the batch suite and the flow table run ONCE, in the quiet freeze window, with every builder stopped |

## 1. Facts (from CODE-FACTS.md, SPIKE.md and the reviewer, 2026-10-19; re-check them)
- **The charge bar:** arc40 (UiHud38.Arc40.cs; built UiHud38.cs:177) + meter42 (UiHud38.Meter42.cs) are hidden at UiHud38.cs:339
  (`if (hud47Built) show = false;`); hud33's dot arcs at UiHud33.cs:720. The data is computed every frame at UiHud38.cs:243-284 (KESTREL /
  THUNDERCLAP `ChargeShots27`, the slug, the PULSE `pulse27`, and the STITCHER heat at :270-275). The BEND meter is NOT in UiHud38: it is
  UiRail34.cs:356-387 (`sim.Bend34Meter`), moved to the weapon panel by UiHud47.cs:173-191. The reticle: UiHud33.cs:690-753; its ticks end
  18 px out at 1080p (the Hud38BarY comment).
- **The look:** the main Volume is built in code (Look.cs:141-190: Bloom, Tonemapping, ShadowsMidtonesHighlights, ColorAdjustments,
  Vignette; ChromaticAberration / FilmGrain 0 at rest); PostFx.cs drives it; a per-theme grade exists (PostFx.ApplyTheme33,
  UnityTuning.Look33.cs:54-62: bloom, exposure, saturation, fog, trilight ambient; the key light UnityTuning.Look41.cs:12). The theme is
  picked from the level name (LightPlan33.cs:120-129). An overlay viewmodel camera sits in the stack (Viewmodel.cs:213-227). Stone.shader
  has no emissive term; its albedo is the vertex colour (pillarTop / pillarSide).
- **The level lights:** fixtures carry `{id, room, kind, at, normal, size, color, range, intensity}`; colours in use: cyan 1604, white
  1219, amber 311, green 137, orange 136. LightKit29.cs:97-107 maps cyan -> usable cyan, **white -> the boundary colour (roof27.boundaryHex)**,
  green / amber / warm to their own colours, orange -> enemy. So 1604 cyan fixtures say "usable" and 1219 white ones say "boundary" almost
  everywhere: both break rule 8. Nothing in a fixture says whether it is on a usable surface (the test is section 4 L's first task).
- **Effects:** no shared impact kit, no colour / timing table: each effect sets its own in its own UnityTuning.*.cs (Fx47View*,
  GunFx.*, HitSpark, ImpactFx, KillConfirm, KillFeel42, Tell36View, ImpactFrames). The enemy rim exists (Enemy.shader:5-26, `_RimColor`
  from the web's fx.enemyRimColor).
- **The baseline** (SPIKE.md, n48-r5 frames): most rooms 61-86 % mid values with 3-8 % saturated pixels; L1's arena the opposite (48 %
  saturated, 2 hue bins, 2 % dark); L3 the strongest (63 % dark, 27 % saturated).
- **Research** (every claim with its URL in the files): ULTRAKILL drives HUD, arms and gun lights from ONE colour table; tells by shape
  and brightness (yellow = parryable); the Railcannon shows its charge on the HUD and on the gun. DOOM (Hugo Martin): "if you have
  something to tell me, tell me loud and tell me fast"; one colour per resource; the crosshair shows the weapon mod's charge (segmented);
  the HUD is drawn after the tonemap. UNIFY: tokens and emissive tiers unify more than post; Neutral tonemapping keeps role hues (ACES
  bends them); script a scene Volume, not the default profile; only the last stacked camera runs post; the environment never takes a
  role's hue at high saturation.
- **Two boxes, in pixels at 1080p:** the RETICLE box = 18 px from the screen centre (the ticks' end); the WASH box = the middle 20 % of
  the screen, 384 x 216 px (FeelReel.N31.Grapple.cs:301).

## 2. Hard rules (CLAUDE.md and NIGHT48.md section 4)
- Git: never master, never push / merge, never `gh auth switch`; builders in worktrees, the lead cherry-picks; explicit paths + `.meta`;
  never stage GG's four files; the TMP fallback asset restored before a commit; night prompts and DECIDE49.md untracked.
- Movement exact: golden 0 / 22 exact at every Core commit; fingerprint plain / --off48 / --off49 EQUAL. Tonight's builder work is view
  only; a sim change needs the lead.
- Numbers: render numbers in `Assets/Scripts/Game/UnityTuning.*49.cs`, each with a comment. **One palette:** the role tokens live in
  UnityTuning.Look49.cs (seeded by the lead at G0); H, X and L read them and hold only their own sizes and timings. Never edit the web's
  `colors` / `fx` blocks in PortExport/config.json; a change to a web colour goes through a Unity-side override in UnityTuning.
- Look (rule 8): cyan = player-usable; red-orange = enemies and danger; yellow = parryable; magenta = RIVAL only; pale violet-white =
  the boundary; UI text >= 16 px at 1080p; the theme fonts. **Level accents stay >= 20 deg of hue away from every role hue and below the
  roles' saturation.** No new full-screen shader (DECIDE47 D-13); URP built-in Volume overrides are allowed; a new material or shader goes
  in Resources and its Player frames are checked for magenta.
- GG owns enemy design (rule 7): no change to an enemy's or boss's model, shape or colours tonight.
- Nothing lost: verbs37 on levels 1-5 after rows go ON; no gun lighter (DECIDE47 D-14). Docs with the Write / Edit tools only.

## 3. P0, the lead: NIGHT48's freeze (T-89, T-96, T-97)
1. **In the shared commit (G0):** the drone split (the lead's one-line gate in EnemyViews.Drone.cs: the drone's foes48 change behind
   `drone49`, off (named drone49, not foes48drone: NightRows.ViewOn reads the night from the row's name; DECIDE49 D-2); the warden plate and the swarmer stay in foes48), checked by a948 (the drone draws its NIGHT47 look with foes48 ON).
2. **The baseline flip, its own commit:** `RowsBaseline.OnPathThrough` 47 -> 48 (SurvivalSim.Rows.cs), the fingerprint measured plain /
   --off47 / --off48 / --off49 before and after (as D-17 did), the hashes' change explained in the message.
3. **The rows-on script:** `Tools/night/n49-rows-on.cjs` (written from n47-rows-on.cjs; the name became n49 because the one script serves P0 and the quiet freeze): it takes `<reasons.json>` with rows from design.n47, design.n48 or design.n49 (the night is found from the config), has `--dry`, writes both config copies byte-identical.
4. **The candidates and the bar:** a VIEW row goes ON when its frames or its blind pair are GREEN in NIGHT48-LOG's CLOSE TABLE: climbTop48
   (G1), wickRideParry47 / wickIntro47 / wickFinale47 (G9), foes48 (G12, swarmer; the drone part stays off), blast48 (G20), loomHull48
   (G36). A SIM or NUMBERS row goes ON when its meter is GREEN on the n49-r0 build and verbs37 passes: guns48 (G24: the a748 ladder),
   wyrmTell48 + wyrmDive48 (G31, G34: n47human --boss wyrm, 40 seeds, the band), kilnBoost48 (G33). finale47: ON if the lead's look at
   its NIGHT47 frames agrees with F's numbers. stampede47: the lead takes its a247 frame in P0; ON only with that frame and verbs37.
   **Stay OFF:** loomReach48 (D-37), flame48 (until its a548 frames are retaken and looked at: the leftovers), the drone part.
5. **The quick gates in P0:** dn-check 0; golden 0 / 22; fingerprints EQUAL; n45rows GREEN; the build `n49-r0` (batch); verbs37 levels
   1-5 (the five Players run serially). The smoke, the batch suite and the flow table wait for the quiet window (section 5).
6. A row that fails goes back OFF with its reason in DECIDE49; the rows that pass are tonight's base for every builder's frames.

## 4. Builders (four; one owner per file; each prompt: "work through your whole list without stopping; one report per round")
| id | owns | row (pre-placed, off) | first read |
|---|---|---|---|
| H | the charge element and the HUD's colours: UiHud38 (arc40 / meter42 re-gated), UiHud33 (the reticle), UiRail34 (the BEND), UiHud47 panel colours, UnityTuning.H49.cs (sizes only) | `charge49` (view) | CODE-FACTS 1; ULTRAKILL.md "weapon charge"; DOOM.md HUD |
| V | the environment grade and colour script: UnityTuning.Look49.cs (fills the tokens' per-level values), Look33 / Look41 per theme, PostFx.ApplyTheme33, the fog, ambient, key and fill, the stone vertex colours | `look49` (view) | CODE-FACTS 2-4; UNIFY.md; SPIKE.md; LOOK-NOW.md |
| L | the level lights in their roles: LightKit29's fixture colour map (cyan only on usable surfaces, white off the boundary colour except on the boundary), each level's landmark lit as its light source, LightPlan33's emissive fixture tiers | `lights49` (view) | CODE-FACTS 3-4; the level JSON (pads, grappleNodes, zoneParts) |
| X | one impact kit: UnityTuning.Fx49.cs (tiers light / heavy / critical x owners player cyan-white, gauntlet pink, enemy red-orange, parry yellow; sizes and timings in whole sim steps); every hit, kill, parry, blast, flame and torch reads it | `fx49` (view) | CODE-FACTS 5; UNIFY.md VFX; ULTRAKILL.md hitstop / parry |

**The builder brief:** NIGHT49-BRIEF.md = NIGHT48-BRIEF.md with these substitutions, all of them: n48 -> n49 in every row, file and reel
name; NIGHT48.md -> NIGHT49.md; DECIDE48 -> DECIDE49; NIGHT48-LOG -> NIGHT49-LOG; "GG's words at the top of NIGHT47.md" -> "at the top of
NIGHT49.md"; "--off48 is NIGHT47's game" -> "--off49 is NIGHT48's game (with the n48 rows P0 turned ON)"; the fingerprint checks add
`--off49`; "never edit design.n27-n47" -> "never edit design.n27-n48"; the table of builders -> section 4's; plus section 0 and the one-palette rule.

### H: the charge element (G1)
- **What:** one element under the reticle for every charge source: KESTREL charge, THUNDERCLAP slug, the PULSE LT blast, the BEND, and the
  STITCHER heat (it was in arc40); never two at once; in the active weapon's colour from Look49's tokens; filling with the charge; one
  2-frame white flash at full; a cooldown as a dim refill; hidden when the weapon has no charge or heat. The label stays in the weapon
  panel. H builds two candidates (a thin arc; DOOM's segmented pips) and the lead picks one by eye on round 1's frames.
- **Acceptance:** reel a149 frames each source at 0 / 50 / 100 % and its cooldown at 1920x1080 and 1280x720; the line prints the
  element's nearest pixel to the screen centre (>= 18 px at 1080p: outside the reticle box), its height (>= 4 px at 1080p) and that it is
  hidden with no charge; ONE blind pair, the picked candidate vs NIGHT46's arc40: "which charge display is clearer and better placed?".

### V: one grade and colour script (G2, G3, G4)
- **What:** per level one dominant environment hue on a dark base, its saturation below the roles' and >= 20 deg from every role hue (L1's
  lava stays the danger; its walls must not be the danger hue); Neutral tonemapping; ShadowsMidtonesHighlights toward the level hue; a
  contrast lift; fog colour + density and the trilight ambient per level; floors darker than walls, walls darker than the sky; the HUD
  outside the grade (check the HUD's cyan reads exact on every level's frame).
- **Acceptance:** the look meter (`python -I Research/2026-10-19-wave49/lookmeter.py <label>=<png> ...`, copied to
  Tools/night/n49-lookmeter.py at G0) on the same five frames per level before and after: a combat frame dark >= 35 %, mid <= 55 %,
  saturated 10-30 %, hue bins 3-5 (hypotheses; V logs any change); 0 magenta pixels; ONE blind sheet: five rows (one per level), old vs
  new in each row, 3 judges answer per row "which looks more like a polished, high-contrast action game?".

### L: the level lights in their roles (G2, G3, G4)
- **What:** first the test: a fixture is "usable" when it is within 1.5 m of a pad, a grapple node, or a ride / climb surface in the level
  JSON (L names the fields it uses and logs the count per level). Then cyan only on usable fixtures; white only on the boundary (the
  pale violet-white rule); every other fixture takes the level accent from Look49's tokens or a dim neutral; each level's landmark (the
  WICK's furnace, the KILN's core, the LOOM's warp, the SPIRE's column, level 2's) lit as the room's strongest light; emissive tiers that
  agree with V's bloom threshold.
- **Acceptance:** per level the fixture counts by colour before -> after (cyan off every non-usable fixture: 0; white off every
  non-boundary fixture: 0); the look meter's cyan share; ONE blind sheet (five rows, old vs new, 3 judges): "which arena reads more
  clearly: where to go and what to use?".

### X: one impact kit (G5)
- **What:** UnityTuning.Fx49.cs: the tiers x owners table (size, brightness, shape; colours from Look49) and one timing table (rise /
  hold / fade in whole sim steps; the hitstop stays the sim's); every effect file reads it with fx49 on. More damage = bigger, brighter,
  sharper (UNIFY.md); "if everything is important, nothing is important".
- **Acceptance:** reel a249: one contact sheet per state (fx49 off / on), each effect at its peak (KESTREL tap and charge, THUNDERCLAP shot
  / slug / boost, PULSE punch / blast, the fist, the TORCH, a kill, a parry, an enemy hit on the player, a boss weak point); the line prints
  per effect its tier, owner colour and peak frame; the wash meter GREEN (no frame's WASH box > +0.25 over f-2); ONE blind pair of the two
  sheets: "which set of effects looks like one game?".

## 5. The lead's clock (by `date`; t0 = GG's go)
| time | lead | builders |
|---|---|---|
| 0:00-0:20 | G0: the shared commit (`Tools/night/n49-shared.cjs` from n48-shared.cjs, dry run first): design.n49 with charge49, look49, lights49, fx49, drone49 (all `enabled false`, flags written); Look49's role tokens seeded (UNIFY.md's role bands); the drone gate; SurvivalSim.N49.cs, FeelReel.N49.cs (a149 H, a249 X, a349 V, a449 L), N49SharedTests.cs; the FeelReel.cs Capture fix; n49-lookmeter.py; NIGHT49-BRIEF.md; spawn H / V / L / X; the first STOP POINT | spawned ~0:20 |
| 0:20-1:05 | P0 steps 2-6 (the flip, rows-on, the quick gates, n49-r0, verbs37 x5) | round 1 |
| 1:05-1:45 | integrate round 1, build `n49-r1`; ONE reel script (all builders' reels + the baseline frames on the same build); the lookmeter; look at every frame; H's candidate picked | (report) |
| 1:45-2:35 | the blind pairs (H 3, V 3, L 3, X 3 judges = 12); round 2 orders from the frames and the votes | round 2 |
| 2:35-3:10 | integrate round 2, build `n49-r2`; one reel script; look; re-pair only a look that changed (<= 6 judges) | (report, then STOP) |
| 3:10-3:40 | **the quiet freeze, every builder stopped:** `n49-rows-on.cjs` (the rows whose frames + pairs + meters are GREEN); the build; verbs37 x5; the smoke; the batch suite (re-run only failing files); the n48 flow table | stopped |
| 3:40-4:00 | the close (starts by 3:40 whatever is unfinished): NIGHT49-LOG close table, NIGHT49-REPORT, HANDOFF, DECIDE49, THREADS, tag `u3-n49` | |

Judges tonight: about 18 (12 in the pairs + up to 6 re-pairs). Cut order (depth only): the 720p twin for non-text rows, a re-pair, the
leftovers. **Never cut:** P0's gates, the frames each row needs, the four pairs, verbs37, the smoke, the batch suite, the STOP POINTs, the close.

**Leftovers (in order, for a builder whose list is done):** H: the HUD's other colours onto Look49's tokens (the counter, the health
bar's frame, the weapon panel). V: the sky / ceiling gradient per level. L: the lava lights onto Look49's danger token. X: the boss weak
point and the tell halos onto the kit. The lead: retake flame48's a548 frames (T-95) and look; the a148 pad pass for the crack prompt
(T-98); `wave-memory harvest 47` (T-102).

## 6. Done when
- P0: the n48 rows that passed are ON (verbs37 1-5 PASS on n49-r0); each row left OFF has its reason in DECIDE49; the baseline flip
  committed with its hashes explained.
- G1 (H): the element on frames for every source at both sizes, its line GREEN, its pair won or tied.
- G2-G4 (V, L): every level's frames inside the lookmeter targets (or a logged reason); the fixture counts at 0 off-role; 0 magenta; V's
  and L's sheets won or tied on at least 4 of 5 levels.
- G5 (X): every effect reads the kit with fx49 on; the wash meter GREEN; the pair won or tied.
- G6: the close's before / after sheet of one frame per level (n48-r5 vs n49-r2) in the report.
- The freeze: the smoke DONE (0 NRE), the batch suite green, the flow table with no level worse past its band. `git status`: GG's four
  files and untracked prompts only; tag u3-n49.

## 7. The report (NIGHT49-REPORT.md, GG's page)
Outcome first: what GG should look at first (the charge element, one frame per level before / after, the effects sheet), each number from
tonight's runs, what is ON by default and how to switch it off (`-skygrindOff49`; `-skygrindOff48` for NIGHT48's rows), what lost its
pair or stayed SHORT and why, what was not verified. The forge experiment's number: the share of G1-G6 closed with both a meter number
and a pair result.

## 8. The closing reminder
- P0 before polish: a look judged on a build whose n48 rows are OFF is judged on the wrong game.
- One palette: Look49's tokens are the only colour table; every other file holds sizes and timings.
- A reel that presses must prove it pressed; the lead looks at every frame and writes its path; a blind pair decides a look.

## Kickoff (GG pastes this into a fresh Claude Code session in `C:\Users\gaige\dev\SKYGRIND-unity`)
```
/effort high
You are the lead of NIGHT49. Read C:\Users\gaige\dev\SKYGRIND-unity\NIGHT49.md whole (GG's words are at its top), then CLAUDE.md,
HANDOFF.md, NIGHT48-LOG.md's CLOSE TABLE, Research/2026-10-19-wave49/CODE-FACTS.md, SPIKE.md and FORGE-NOTES.md, and run the night
exactly as written. Four hours, hard cap 4 h from my go. I will not answer during the night: decide every call in DECIDE49.md and keep
going. P0 (NIGHT48's freeze) comes first. Write a STOP POINT line at every build. Start with `date`.
```

## /goal (paste after the kickoff; under 1,800 characters)
```
/goal Met when NIGHT49-LOG.md's close table is printed (G1-G6 and every P0 row, each GREEN, SHORT, or OFF with its reason and a frame path where one exists) and tag u3-n49 exists, or when 4 h have passed since the kickoff by `date` (the close starts by 3:40 whatever is unfinished). Run NIGHT49 nonstop as the lead: never ask me; decide every call in DECIDE49.md. Order: G0 shared commit (the drone gate, Look49's tokens, the Capture fix), then P0 (the baseline flip, the n48 rows on, verbs37 1-5) while builders H (charge element), V (grade and colour script), L (level lights in their roles), X (impact kit) build. Every look ends in a blind pair (3 fresh judges, key read after); the lead looks at every frame and logs its path. The smoke, the batch suite and the flow table run once in the quiet freeze window with the builders stopped. A STOP POINT line with the close table so far at the top of NIGHT49-LOG.md and HANDOFF.md at every build and every 30 min, committed. Commit every slice. Close with the close table, NIGHT49-REPORT.md, HANDOFF, DECIDE49, THREADS and tag u3-n49.
```

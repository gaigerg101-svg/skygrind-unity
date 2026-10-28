# NIGHT12.md: SKYGRIND in Unity, stage 3 night 4 (the look, the feel and the meta core), a BUILDING night, no human in the loop, at most 12 h

You are Claude Code working alone in the Unity project `C:\Users\gaige\dev\SKYGRIND-unity` (Unity 6000.3.7f1, URP 17.3).
GG starts this run and leaves; the Editor is open on this project. Nobody answers questions and nobody playtests.

**Read these files:**
- **this file:** tonight's job
- **`CLAUDE.md`:** the rules that always hold (read it first)
- **`NEXT-WAVE-RESEARCH.md`:** why tonight looks like this (sections 0-5); the evidence is in `Research/2026-10-03-*.md`
  (retention, polish, unity: read the sections each item cites, not all 1,640 lines)
- **`PORT-RULES.md`:** how to translate a web module (tonight's ports are views and small meta modules)
- **`HANDOFF.md`:** where things stand (anchor `u3-n11`; read its update note first)
- **`tasks.json`:** tonight's items are U3-25.1 .. U3-25.12; U3-25 is the umbrella and its acceptance holds
- **`NIGHT11-LOG.md`:** how the last night ran, its slips

**Run length: up to 12 hours from your first `date`, or what GG says when he starts it.** Stop earlier only when the
definition of done (section 2) is met.

GG, verbatim (2026-10-03, after NIGHT11): "great lets see what other features or things we are missing that should be
apart of it, resrecah what is at core of best selling and most played video games what polish, features ect make them
stand out and have players coming back, how can we take that and add more to the game that will be good, strat with
resrecah and create next goal prompt, I want way more building than testing (remember this is on unity!)". Before that:
"want this game to be like cod zombies meets ultrakill as the main, but other games should be inspired".

**Why tonight looks like this (NEXT-WAVE-RESEARCH.md section 0):**
- The best sellers keep players with runs that always pay, unlocks shown as locks, difficulty rungs, a daily, mastery
  tracks and a first session that teaches one thing at a time; SKYGRIND carries nothing across runs but a best round.
- Their polish is in the gun's life, kills that read, a lit world with depth and ceremonies; SKYGRIND's gun is bolted to
  the camera, its map can't receive SSAO or point lights, its kills are cube bursts and its RESULTS is a wall of text.
- **Most of both is already designed and tuned in the frozen web game** (`config.json`: `post`, `lightPool`, `motes`,
  `blobShadows`, `fx.killConfirm`, `fx.enemyRim*`, `viewmodel.*`; `challenges.js`, `unlocks.js`, `mutators.js`,
  `daily.js`, `seeds.js`, `finishes.js`, tips), and URP has the built-ins. So tonight is building: views and small Core
  state, little new testing. The POWER guns move to NIGHT13 (`NIGHT13.md`).

**Goal (the morning):** GG starts the game and it looks and feels like a shipped game: a living dusk title in the theme
fonts; a lit map with depth (SSAO, light pools, motes, a graded dusk, fog that falls away); a gun that breathes, bobs and
leans; enemies that glow at their edges, flash when hit and dissolve when they die, with a kill confirm on every kill;
dust when he lands, rings when he slams, sparks on a wall ride; the round number burning in like CoD's; a RESULTS screen
that counts up, slams his grade and shows what he's close to unlocking. He opens CONTRACTS and sees 30-40 goals as locks;
his first one unlocks in round 1 or 2; he picks HEAT 1 or today's DAILY on the run setup; his guns earn mastery tiers and
finishes; a first run teaches one system per round. With every new flag off, one player plays exactly the web's game.

## Facts already measured (2026-10-03; re-check them, don't trust them blindly)

- **The baseline (`u3-n11`, tag on `1128c2e`):** `tests: 437 / 437 passed, 0 failed`, `replays: 22 / 22`, `golden tests:
  90 / 90`; harness `golden total: 34 vector sets (31481 records), 22 replays (22 exact); 0 failing, 0 mismatches`; the
  Player's smoke `[smoke] 86.3s DONE: all steps passed`; bench median 2.47 ms, worst 1% 5.55 ms (Balanced, AC).
- **The look now** [V Research/unity section 1]: `Shaders/Stone.shader` (the whole map) has no `_SCREEN_SPACE_OCCLUSION`
  keyword and no additional-light loop (URP 17.3 names it `_CLUSTER_LIGHT_LOOP`, not `_FORWARD_PLUS`), so the SSAO
  renderer feature (After Opaque off, DepthNormals source) most likely runs without reaching the map and no point light
  would light it; `Look.cs` sets bloom, ACES, exposure and a hand-typed 0.22 vignette; the opaque texture is enabled and
  unused; `Particles` (EnemyViews.cs) draws one GameObject + MPB per cube; Forward+ and Render Graph are on.
- **The web's tuned numbers** [V config.json]: `post` (bloom*, vignette*, chroma Hurt / Dash / Rank / Speed / Decay /
  Modulation, `grade`, `godRays`, `lowHpAt`, `lowHpDarkness`, `quality`), `lightPool` (max, steadyNear / Far, blast,
  boss, machine), `motes` (max, box, wind, size, colours, opacity, seed), `blobShadows`, `ao`, `fx` (killConfirm,
  enemyRimColor / Power / Strength, popTime / Intensity / Distance, landPuff*, slamRing*, spark*, streak*, flash*,
  flashLightIntensity / Distance, impactFlare, ghost*), `viewmodel` (breath*, bob*, sway*, strafeRollDeg, slideDipY,
  slideRollDeg, wallTiltDeg, airLiftY, dashPull*, landDipScale, cylinderStepDeg / TurnTime, pump*, afterimages).
  The web modules: `fx/killConfirm.js` 97 lines, `fx/dissolve.js` 38, `fx/rimGlow.js` 40, `fx/xray.js`, `fx/motes.js`
  105, `fx/lightPool.js`, `fx/blobShadows.js`, `fx/post.js`, `fx/grade.js`; `modes/challenges.js` 132 (24 badges,
  `config.challenges.list`), `modes/unlocks.js` 46, `modes/mutators.js` 125 (lowGravity, glassCannon, speedDemon,
  noRegen), `modes/daily.js` 63 (`config.daily` salt + mutators), `modes/seeds.js` 33, `modes/finishes.js` 45
  (`config.weapons.finishes`: factory, graphite, arctic, brass, mirror, protocol, skylight), `ui/tips.js` 41. Their web
  tests: `tests/runModes.test.js` :22-40 (mutators), `tests/night6.test.js` (DAILY :151-175, the PROTOCOL finish
  :93-102), plus any test importing finishes / challenges / unlocks (grep `tests/`).
- **Not ported in Unity:** every module above; `Viewmodel.cs` says "not ported: bob, sway, breath, the slide / wall
  tilts, trauma"; `CameraFeel.cs` says the dash edge tint and the aim-lock FOV pinch are not ported; RESULTS is a single
  TMP body (`Ui.cs`); records live in PlayerPrefs (`Records.StorageKey`).

## 0. Tonight's rules (on top of CLAUDE.md, which is in force)

1. **Branch:** keep working on `port/stage3` (from `u3-n11`). Tag `u3-n12` at the end.
2. **Building first.** At least 80% of the night goes to features. Testing is about 15%: the gates CLAUDE.md requires,
   unit tests for the new Core state, the ported web tests, captures for the looks, one smoke and one Player check at the
   close. No new golden vector sets tonight (the ported modules are views and small meta state; the web tests are their
   oracle). Don't re-verify what a compile, a test or a capture already showed.
3. **Remember this is Unity.** Use what URP 17 / Unity 6 gives (section 3): hand-written HLSL in `Assets/Shaders`,
   Forward+ point lights, Volume overrides, the SSAO feature, `Graphics.RenderMeshInstanced`, render passes injected by
   script (Render Graph), TextMeshPro font assets made by an editor script. No VFX Graph / Shader Graph assets (editor-
   window authoring), no Cinemachine (it would fight the golden-checked trauma shake), no new packages (D-30).
4. **Render-only stays render-only.** Every look and feel item is a view: it reads the sim and never changes it; it runs
   on the loop's `worldDt` (hitstop and pause apply); its randomness comes from its own Mulberry32 seeded per run (never
   a run stream, never `UnityEngine.Random`); it allocates nothing per frame after warm-up (PORT-RULES section 6).
5. **Port where the web has it, design where it doesn't.** The web's FX and meta modules are ported faithfully (their
   numbers from `config.json`, their tests ported). New design (the PAYOFF strip, HEAT rungs as a ladder, MASTERY tracks,
   the extra contracts, the first-run script, the round ceremony, the RESULTS tally, the living title) goes behind
   `design.*` flags with a `_why` citing `NEXT-WAVE-RESEARCH.md` / `Research/`; defaults ON (they don't change a run's
   rules) except the HEAT rungs, which are opt-in by choice on the run setup. Unity-only render numbers go in
   `UnityTuning.cs` with a comment.
6. **The meta's state lives in one JSON profile** (`Application.persistentDataPath/skygrind-profile.json`, versioned, a
   corrupt or missing file starts empty), not PlayerPrefs: records (migrated once from PlayerPrefs), unlock flags,
   contract progress, mastery, best grade per rung, the DAILY's streak. Settings stay where they are tonight (N16).
   A LAB run and the smoke's runs (`noRecords`) never change the profile.
7. **GG owns sound, looks and enemy design** (CLAUDE.md rule 7): enemy models stay as they are (the rim, flash, dissolve
   and x-ray are the web's own looks); no gibs (D-29); no audio code: every ceremony and FX fires on a named event GG can
   bind (port-only names listed in PORT-STATUS). The dusk palette holds: cyan = player-usable (kill confirm, guides,
   unlock highlights), red-orange = enemies and danger, magenta = RIVAL only, gold for points and grades.
8. **Movement stays bit-exact; golden + 22 replays at every Core commit** (CLAUDE.md rule 3). No item names `Mover.cs`.
9. **No questions, no waiting.** Pick the safe default, log it with the rejected alternative, continue. A status line in
   `NIGHT12-LOG.md` at the end of every item, with times from `date`.
10. **Agents:** this night is wide. Run 5-6 background builders split by file ownership (memory `night-parallel-agents`):
    a shared-types commit first (the profile API, RunStats' new fields, the event names); Core meta in a git worktree
    (harness-compiled, cherry-picked); Game builders on named files only (compiled by you). You alone drive the Editor,
    take captures and commit. Evals idempotent (memory `play-check-eval-recipe`).

## 1. Start (U3-25.1, 10 min)

1. Read the files listed at the top, CLAUDE.md first.
2. HANDOFF section 1, rows 1-5 and 8-9; `RECEIVED` in `NIGHT12-LOG.md`. The Player rows (6-7) only if a row 1-5
   differs (they passed at the u3-n11 close).
3. HANDOFF section 5: GG's answers (D-13, D-18, D-25..D-33); log them.
4. Load `unity:unity-cli` before driving the Editor and `unity:ui` before the UI items.

## 2. Definition of done (all measurable)

1. **U3-25.2, light and lens:** `Stone.shader` receives SSAO and point lights; `post.js` on the URP Volume (the split-
   tone grade, chroma pulses on hurt / dash / rank / speed, the low-HP volume, grain by preset, the vignette from config:
   the 0.22 literal gone); fog that thins with distance; `lightPool` (steady pools at the machines, the box, the relays,
   the radio; pops on blasts) plus the muzzle-flash point light (`fx.flashLight*`) and kill / impact pops (`fx.pop*`);
   blob shadows; motes (one instanced mesh). Captures: the map before / after from the same spot.
2. **U3-25.3, the enemy shader:** one `SKYGRIND/Enemy` HLSL shader on every enemy view: the rim with a distance boost
   (`fx.enemyRim*`), the hit flash, the death dissolve (dissolve.js), an x-ray pass (xray.js, `ZTest Greater`, behind a
   setting). Capture: a husk at 30 m, a dissolve mid-death.
3. **U3-25.4, hits, kills and movement FX:** an instanced particle renderer replaces the GameObject cube pool (one draw
   per material); kill confirm (killConfirm.js); a hitmarker grammar (body / head / kill / elite) and the victim's
   decaying vibration; the impact flare; a tracer per pellet; shells; land puffs, slam rings, wall-ride sparks, the dash
   edge tint, speed streaks (the `fx.*` numbers). Capture: a THUNDERCLAP kill.
4. **U3-25.5, the viewmodel's life:** bob, sway, breath, strafe / slide / wall tilts, the air lift, the dash pull, the
   land dip, trauma's viewmodel share, the KESTREL's cylinder step, the THUNDERCLAP's pump, afterimages (all from
   `viewmodel.*`). Capture: mid-slide and mid-wall-ride.
5. **U3-25.6, ceremonies and type:** the round ceremony (the round number flashes at the clear; the new one burns in at
   the centre and flies to its corner; tally strokes for rounds 1-5; port-only events `roundCeremony`); RESULTS as a
   staged tally (the outcome, the round, three counting cards, the grade slam, NEW BEST as a stamp, the score line;
   any key completes it; three type sizes); HUD motion (the HP bar's damage ghost, the points roll, the web's panel skew);
   the theme fonts (Barlow Semi Condensed + Chakra Petch, OFL TTFs fetched once into `Assets/Fonts` with their licence
   files, TMP font assets made by an editor script; every text at least 16 px at 1080p); a living title (a slow camera
   path over the map at dusk with motes and pulsing strips, input live within 1 s). Captures: TITLE, a round burn-in,
   RESULTS mid-tally.
6. **U3-25.7, the profile and the contracts:** `Core/Profile.cs` (the JSON profile, rule 6) with a round-trip test and a
   PlayerPrefs records migration; challenges.js (24) and unlocks.js ported (their web tests ported), plus port-only
   contracts to 30-40 in all (`design.contracts`, `_why`: Research/retention P3), each unlocking variety, never stats
   (the web's locked MODS by flag, finishes, HEAT rungs, the DAILY); a CONTRACTS screen on TITLE (progress bars; locked
   ones visible as locks); a toast on completion (port-only event `contractDone`); the first unlock reachable in round
   1-2. Tests: progress, completion, the unlock it grants, a LAB / noRecords run changes nothing.
7. **U3-25.8, the PAYOFF strip and the grade chase:** DOWN and RESULTS show the personal-best deltas, the 3 nearest
   contracts with their bars, mastery XP gained, the NEXT UNLOCK; a best grade per HEAT rung on TITLE; a "P" grade on an
   exfil at S grade with an SS peak and no down (`design.grade.p`). Test: the P rule.
8. **U3-25.9, HEAT and DAILY:** mutators.js ported (runModes.test.js's mutator tests ported); HEAT rungs 1-3 =
   GLASS CANNON, SPEED DEMON, NO REGEN (`design.heat`: a rung adds its rule and a score multiplier, shows a badge on the
   grade; an EXFIL on rung N unlocks N+1; LOW GRAVITY stays a fun toggle, not a rung); daily.js and seeds.js ported
   (night6.test.js's DAILY tests ported): a local DAILY (the date seed, its twist, the first finished run counts, the
   streak), the seed code on RESULTS, RETRY SAME SEED; a run setup on TITLE (SURVIVAL / DAILY, the HEAT rung). Tests: the
   ported ones, the rung unlock, the DAILY's counted run. Records and boards per rung stay local (Steam is N16).
9. **U3-25.10, MASTERY and finishes:** finishes.js ported (a finish recolours a gun's neutral body tones, never the
   family trim or the gem: BlockyKit materials); a MASTERY track per gun, 3 tiers tonight (`design.mastery`, `_why`:
   Research/retention P4: counts in the tens, teaching the verbs: kills, FRESH or air kills, S-rank kills), each tier a
   finish; a WEAPONS screen on TITLE (tiers, bars, the finish in use); the finish shows on the viewmodel and the wall.
   Tests: tier progress from kill events, a finish unlocked and applied. Capture: a finished gun.
10. **U3-25.11, first-run onboarding:** tips ported (`ui/tips.js`, the web's tip texts); a first-run script
    (`design.onboarding`: the first run only, skippable in SETTINGS): one system per round (R1 move and shoot, R2 dash /
    slide / style, R3 a wall buy and points, R5 perks, R6 the power and the PROTOCOL), a guide pulse (a light pool) on
    the next thing to learn, nothing new punished in its first round (no rule change: only text, lights and order of
    tips). Test: the script's round table.
11. **U3-25.12, the close:** section 7, every line.
12. **Invariants at the end:** `tests: N / N passed, 0 failed` with N above 437; every golden test passes (90 or more);
    replays 22 / 22; the Editor and Player smokes DONE with the two new steps; the Player's `-skygrindGolden` 0
    mismatches; the bench's worst 1% at most 6.9 ms (else the costliest effect moves to a quality tier, logged);
    `git status --short` shows only `UnityConnectSettings.asset` (if modified) and untracked NIGHT7..NIGHT13.md.

## 3. The Unity toolbox for tonight (Research/unity sections 0, 2, 4)

- **Shaders:** hand-written HLSL in `Assets/Shaders` (Stone fix: `#pragma multi_compile _ _SCREEN_SPACE_OCCLUSION`, the
  URP 17.3 cluster light loop for additional lights; the Enemy shader; an instanced unlit particle shader; a motes vertex
  shader; a dissolve with a noise texture made in code). Shaders built in code need `Shader.Find`-safe names and must be
  in the build (Always Included or referenced by a material asset the scene holds: check the Player).
- **Post:** URP Volume overrides from code (`ColorAdjustments`, `SplitToning`, `ChromaticAberration`, `Vignette`,
  `FilmGrain`, `Tonemapping`, `Bloom`); pulses driven per frame by a view.
- **Lights:** realtime point lights pooled (Forward+ is on), shadows off on pops; intensity from config.
- **Draws:** `Graphics.RenderMeshInstanced` with `RenderParams` and a reused matrix array; no per-object GameObjects.
- **Passes:** only if a pass is needed (the x-ray can be a second shader pass instead): inject by script with
  `RenderPipelineManager.beginCameraRendering` + `EnqueuePass`, Render Graph only; never edit `PC_Renderer.asset` by
  hand while the Editor is live.
- **UI motion:** code tweens on unscaled frame time (the HUD's own easing helpers); TMP font assets via
  `TMP_FontAsset.CreateFontAsset` in an editor script, saved as assets.
- **Captures:** `ScreenCapture.CaptureScreenshot` inside the smoke / a check coroutine (memory note), moved to
  `Logs/shots/n12-*.png`.

## 4. Notes (decided; don't re-open them)

- **One currency (points).** Unlocks are variety, never stats (the web's own rule in `unlocks.js`).
- **Contracts never expire** (no FOMO: Research/retention section 4).
- **The HEAT ladder is opt-in;** the base game never gets harder.
- **The DAILY is local** until the Steam layer (N16); its streak lives in the profile.
- **No gibs, no damage numbers by default** (clutter; D-29); damage numbers may ship as an off-by-default setting if
  time allows.

## 5. Testing budget (small on purpose)

- Per Game item: a compile; a capture when the item is visual. Per Core item: its unit tests and the ported web tests,
  seen red once; the full suite + golden + replays at each Core commit (CLAUDE.md rule 3).
- **Caps:** Play checks at most 3 (one look pass with captures mid-night, one Editor smoke before the build, one
  spare); captures at most 12; Player builds 1 (the close). **Caps are caps.**
- **The smoke gets two steps only:** a contract completes in a run and its unlock shows in the profile; a DAILY run
  starts on today's seed with HEAT 1 stamped on its RESULTS. Render items need no smoke step.
- One heavy job at a time (a build, a bench, the suite).

## 6. Order of work and budgets

| Order | Item | Budget (lead) | Who builds |
|---|---|---|---|
| 1 | U3-25.1 verify | 10 min | lead |
| 2 | shared types (profile API, RunStats fields, event names) + agent launch | 30 min | lead |
| 3 | U3-25.2 light and lens | 75 min | agent A (Shaders/Stone, Look.cs, new LightAndLens.cs) |
| 4 | U3-25.3 the enemy shader | 60 min | agent B (Shaders/Enemy, EnemyViews.cs views) |
| 5 | U3-25.4 hits, kills, movement FX | 75 min | agent B / C (Particles, new HitFx.cs, MovementFx.cs, WeaponFx.cs) |
| 6 | U3-25.5 the viewmodel's life | 45 min | agent C (Viewmodel.cs, CameraFeel.cs) |
| 7 | U3-25.6 ceremonies and type | 90 min | agent D (Ui.cs + partials, MysteryBoxView.cs, Editor font script, the title) |
| 8 | U3-25.7 the profile and contracts | 90 min | agent E (Core worktree: Profile, Contracts, Unlocks) + lead (screens) |
| 9 | U3-25.8 the PAYOFF strip, grade chase | 40 min | lead / agent F (new UiMeta*.cs) |
| 10 | U3-25.9 HEAT and DAILY | 75 min | agent E (Heat, Daily, Seeds) + agent F (run setup) |
| 11 | U3-25.10 MASTERY and finishes | 60 min | agent E (Mastery, Finishes) + agent C (finish materials) |
| 12 | U3-25.11 first-run onboarding | 45 min | agent F (tips, the script's views) |
| 13 | U3-25.12 close | 50 min | lead |

- **An item 25% over its budget:** ship what works behind its flag, mark it `in_progress` with what remains, move on,
  come back after the others.
- **The close starts by hour 10:30** (scale it with the run's length) whatever is left.
- **Cut order** (only if the run is cut short): onboarding, then MASTERY's WEAPONS screen (keep the tracks), then the
  living title (keep the fonts), then x-ray, then shells.
- **Never cut:** U3-25.1, .2, .4, .5, .7, .12, and the rule that the flags off give the web's game.

## 7. Close (U3-25.12): the handoff loop

1. **The invariants, results in the log.** Rebuild the Player (Release; scenes Survival then Arena), then `git diff
   --stat` and the restores (Mobile_RPAsset, the TMP fallback asset); INV-smoke (with the two new steps);
   `-skygrindGolden`; one bench (`bash Tools/survival-bench.sh 60`, the plan polled), compared with u3-n11's.
2. **`tasks.json`:** every item touched has a status and evidence (a commit hash, an output line, a time, a capture).
   Never delete an item. U3-25 passes when its acceptance is met.
3. **Docs:** PORT-STATUS.md (FX, UI, Survival, modes, the events: the web's and the port-only ones), PORT-PLAN.md
   section 2 ticks (Look, UI, Modes: daily / mutators / challenges / finishes), NEXT-WAVE-RESEARCH.md section 4 (what
   shipped), PORT-RULES.md (a rule learned twice).
4. **Rewrite `HANDOFF.md`** with the `handoff` skill, in its own structure (anchor `u3-n12`, the verify block with
   tonight's values, next actions = NIGHT13 (the POWER guns), decisions, risks, dead ends, invisible state, a changelog
   line).
5. **`NIGHT12-REPORT.md` for GG (one page):** what shipped with commits, tonight's numbers, the captures, what is partial
   and why, a 10-minute play checklist, one question each: the look (too dark? too busy?), the gun's life, the kill
   confirm and dissolve, the round ceremony, RESULTS, CONTRACTS (enough? the first unlock fast enough?), HEAT, the DAILY,
   MASTERY, the first run.
6. **Refresh `NIGHT13.md`** (untracked: the POWER guns) against the code tonight changed (the baseline, the profile in
   save-and-quit's snapshot list, the finishes the new guns need).
7. **Promote any lesson seen twice to memory**; propose CLAUDE.md changes in HANDOFF's pending decisions.
8. **Finish:** tag `u3-n12` once on the close commit; `git status --short` as in section 2.12; nothing pushed, nothing
   merged.

## GG's standing instruction (verbatim, from NIGHT8)

A standing instruction from the user, the person you are working for. It is about how your turns end. A message with no
tool call in it ends your turn, and the work stops there until you are asked to continue. The user has seen you end turns
in four ways while work they asked for was still owed, and does not want any of them. One: a long summary of what was
done that closes by announcing the next step and has no tool call, so the next thing never starts. Two: an offer to carry
on with something unless the user would prefer otherwise, which stops to wait for an answer the user was not going to
give. Three: a list of decisions for the user when, by your own account, none of them blocks the rest of the work. Four:
deciding that this is a good place to report, because the turn has been long or a milestone is done. Status notes are
welcome, and so are your recommendations on open decisions, but put them in the same message as your next tool call and
carry on with whatever does not depend on the user's answer. If you notice yourself inviting the user to redirect you or
offering to wait, delete it and do the next thing. The stops the user does want are the ones where nothing can move
without them, or where the thing blocking you is deliberately protected from you. This does not override the need for
confirmation on risky or destructive actions.

## Closing reminder

- **Build.** GG asked for way more building than testing: views and features first, the gates CLAUDE.md requires,
  nothing more.
- **Render-only stays render-only;** the flags off give the web's game; golden + replays at every Core commit.
- **Write only what was measured.** Every number in the log, report and tasks.json comes from an output line tonight.
- **The web repo is read only.** Caps are caps.

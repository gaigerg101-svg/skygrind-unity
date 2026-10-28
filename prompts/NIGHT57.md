# NIGHT57.md: SHIP AND DEEPEN. Ship what is built, rebuild level 3 from scratch, make standing still lose (lead + three builders + batched critics; at least 4 h, hard cap 5 h; red-teamed 2026-10-21 09:2x, fixes applied)

GG, 2026-10-21, verbatim (this outranks everything below it):
> i cant play it now so i need you ti figure out, study game deisgn check doucments and see where we are missing, use skills, look at github

The lead's four questions (2026-10-21, verbatim): "1. Shipping rule: a feature ships when it beats the old version on its main number and
isn't worse on its guard numbers. Your play is the veto. Yes or no? 2. Enemy behaviour: may I change how enemies attack (delayed aim,
attack-token cap), given that it changes enemy design? Yes or no? 3. Level 3 rebuild: do you want THE KILN rebuilt from scratch to 5-6
rooms, or patched? 4. Editor: please restart Unity." GG's answers, verbatim:
> 1. you decide  2. yes  3. yes  4. will do write pormpt now

Then: "once finished writing prompt (continue t tak eyour time, start it immediatley, building)".
[reading: 1 = the shipping rule (section 0) is the lead's call; 2 = enemy ATTACK BEHAVIOUR on existing kinds may change (looks and new
kinds stay GG's, rule 7); 3 = rebuild level 3 THE KILN from scratch to 5-6 combat rooms; 4 = done: the Editor answered at 09:13.]

**GG will not answer during the run.** Decide every call in `DECIDE57.md` (untracked; D-1..D-6 are already written) with its reason and the
alternative rejected, and keep going. If GG writes, his words come first. Base: `port/stage3` HEAD (2371852a or newer; tag `u3-n56` =
`git rev-parse u3-n56^{commit}`, the play build Builds/n56-freeze). t0 = the first `date` you print. Print `date` and the elapsed time at the
start of every turn. Research for this night: `Research/2026-10-21-wave57/GAPS.md` (read all of it), research/design.md, research/github.md.
Do not redo it. Re-check every number below before you trust it.

## 0. Why the night is shaped this way (GAPS.md sections 1-3)
- **Built work does not reach the player.** NIGHT56 built ~12 rows and shipped 1 ON; NIGHT55 gated 1 of 26. The rule that held them off:
  a row shipped only if every line of its critic passed, so a 2.4 px overlap kept a whole feature off.
- **Levels 3-5 have three combat rooms each and the roster falls back** (level 1: 6 rooms / 3 kinds, level 2: 8 / 7, level 3: 3 / 6,
  level 4: 3 / 3, level 5: 3 / 5; counted from Design/levels/n52/*.plan.json). Level 3 is GG's named problem (2026-10-13: "just isn't
  good ... encourage far more mechanics, have a new arena, and keep the worm AI the same").
- **Standing still wins:** the walk bot stalls 25-37 % of combat time (bot). The research (design.md lessons 2, 4) says pressure the
  still player and cap who attacks; tokens56 is built and OFF.
- **The judge is uncalibrated:** every verdict since 2026-10-20 14:05 is a bot's or a critic role-playing GG, never checked against GG.

**THE NEW SHIPPING RULE (D-1; the forge experiment, section 8).** A row ships ON when ALL hold:
1. its MAIN number (named in its slate entry BEFORE any number is seen: Logs/n57/slate.json is fixed at the shared commit) beats the A0
   baseline (the same measure, the row off, at HEAD) by more than its noise: for bot numbers, more than the gap between seeds 1-10 and
   seeds 11-20 of the baseline (deterministic runs make +0.1 meaningless), or the entry's stated minimum;
2. each GUARD number is no worse than the baseline by more than its band (bot numbers: 10 % relative, or outside the seed spread; frame
   meters: the planted-case tolerance);
3. nothing lost (verbs37 x5), gates EQUAL with the row off (golden 0, replays exact, fingerprints, and `n57-gates.sh --flow`);
4. the row-on side is measured at HEAD on tonight's build (NIGHT56's numbers only order the work); the batched critic confirms the
   evidence is real and the comparison fair (right build, row really on, frames show what the numbers say) and judges the eye-only
   lines. A line the baseline already fails never blocks a row (L-100) unless the row makes it worse beyond its band. A near miss on a
   target that is not a guard is noted, not blocking. GG's play is the veto: every ON row has a PLAY note and an off switch in the report.
5. a MAIN that is an AI critic's blind preference (kill55, colour56) counts only if P0-calib PASSED; otherwise its measured fallback is
   the MAIN (kill55: hit marks >= 5 % of the body the next frame AND the chain hold ladder on a355; colour56: the key or accent at
   S >= 0.35 on >= 3 % of the frame per level, the accents >= 40 deg apart).

## 1. Facts already measured (re-check them)
- **Done by the lead before t0 (2026-10-21 09:13-09:35):** the Editor answers (autotick on); disk 34 GB free; the baselines at HEAD
  2371852a: `Logs/n57/flow-base.txt` (n46flow, n52 levels, 10 seeds; kiln clear 92.2 s, stall foundry 27.7 / iceline 36.5 / kiln 25.5 %,
  deaths 21 / 9 / 8 / 0 / 0) and `Logs/n57/wyrm-base.txt` (n47human wyrm on n52-kiln, 40 seeds: 38/40, median 122.6 s, p90 256.9 s,
  deaths 68); both equal NIGHT56's numbers (the harness is deterministic). The shared commit's script `Tools/night/n57-shared.cjs` ran:
  design.n57 (press57 sim, kiln57 level row), the maps.list row for n57-kiln, the placeholder `Design/levels/n57-kiln.json` (+ Player
  copy: a byte copy of n52-kiln, only its name changed), `SurvivalSim.N57.cs` (kiln47's glimpse / pass / cameo beats follow the chain
  n57 -> n52 -> n47), `FeelReel.N57.cs`, `N57SharedTests` (3/3), N52 / N50 shared tests 4/4, 3/3; `n57-gates.sh` GREEN at 09:30 (golden
  0 mismatches, 22/22 replays, fingerprints plain / --off56 / --off57 EQUAL, Editor compile clean). `n57-gates.sh --flow` adds the
  campaign check (n46flow --off57 = flow-base line for line). Committed as the shared commit (`git log --oneline -3`).
- Play build `Builds/n56-freeze` (u3-n56): clear55 ON; verbs37 27 / 21 / 31 / 31 / 31; the smoke DONE; golden 0, fingerprints EQUAL.
- The shelf (Logs/n56/slate.json, NIGHT56-REPORT.md "Built tonight, OFF"; the A0 lead re-reads each against the rule):
  - wyrm55 (bot, n52-kiln, 40 seeds): 38/40 -> 40/40 killed, median 122.6 -> 86.8 s, p90 256.9 -> 178.3 s, deaths 68 -> 32.
  - tokens56 (bot): attack starts inside the sim cone 44.6 -> 84.3 %; per-enemy idle > 20 % (iceline 21.4 %) (Logs/n56/bot-r4-tokens).
  - kill55 (Player a355): chain holds work; blind judge ON "hits harder" both orders; hit marks 0.04 % of the body; a354 jitter 0.230 (limit 0.191).
  - colour56: blind ON "more colour direction" both orders (foundry, kiln); the world out-saturates the tells on 28 / 36 frames.
  - brief55: every line passes; a card 2.4 px over the room counter for 1 frame on kiln / spire.
  - grade55: THE WICK grade + hot wall pass; THE CROWN keeps 0.68 of its tells; THE WICK's ride panels 0 % cyan.
  - wick55 (bot): damage in the windows 6 -> 57 %, median 46 -> 58 s, 7 deaths / 40; the Player pilot cannot climb the stack (T-59).
  - rank56: DULL share 50.5 -> 50.6 % (unmoved); the ride-kill refund pays 0; N56RankTests' RowOff fails in the Editor (T-137).
  - rim56: 1 of 30 enemies brighter than the world; yellow56: spire up to 24 % non-parry yellow.
- Level 3 now (n52-kiln, Design/levels/n52/kiln.plan.json): rooms kb1 / kb1b (links), kb2 POUR HALL, kb2b (link), kb2c THE ASH BED, kb3
  THE MOULD LINE, kb3b / kb4 (links), k5 THE KILN'S HEART (the boss room). 33 enemies; bot clear 92.2 s; generator chain
  `Tools/night/gen-n52-kiln.cjs` + `n53-k-kb3*.cjs` (its header has the run order). `Design/levels/n52/kiln.json` is untracked (levelgen's
  intermediate; the game reads `Design/levels/n52-kiln.json` + the StreamingAssets copy).
- The Editor: on 2026-10-21 a run_tests batch (Game M) wedged the Pipeline (status bar "Executing IPostBuildCleanup for:
  Unity.PerformanceTesting.Editor.TestRunBuilder"; Logs/n41-suite-n56-freeze.txt). GG restarts it before the run. `Tools/night/n41-suite.sh`
  has an UNCOMMITTED change (waits for an idle Editor, never re-runs a timed-out batch, a `class` mode); commit it after its first good run.
- Archived builds with GG's verdicts (section 4 P0-calib): Builds/n35-r4 + Logs/feel/n35-r* ("the gameplay just feels awful", w35),
  n37-r7 + Logs/feel/n37-r* ("Gameplay feels a little bit better"), n41-r7 (the level 1 wall ride "awful and relaly difficult"), n42-r4
  (the ride "clunky and buggy", the UI "too generic and cheap"). OWNER.md holds the full quotes.

## 2. Scope
- **In:** the shelf (NIGHT55-56's OFF rows), level 3 rebuilt from scratch (rooms, waves, spawns, pieces), enemy ATTACK BEHAVIOUR (GG 2:
  yes) on existing kinds, kill feedback, the colour budget, the critic's calibration, the Game M test hang, an as-shipped rooms table.
- **Out (do not start):** enemy looks / models / new enemy kinds (GG's; proposal page only), audio, level 6, the UI's style, the wall
  ride's speed, THE KILN WYRM's AI (GG: "keep the worm AI the same"; wyrm55's encounter numbers are in), new verbs, new modes, the 182 old
  worktrees, GG's four modified files. New ideas go to docs/memory/IDEAS.md.

## 3. Roles and the clock (t0 = your first `date`; close table no earlier than 4:00; hard cap 5:00)
| when | who | what |
|---|---|---|
| 0:00-0:10 | lead | A0: `date`, `bash Tools/unity-loop.sh status`, git (the shared commit is HEAD), `df`; no `n41-suite` / SKYGRIND.exe left running |
| 0:10-0:25 | lead | spawn L, G, V ONE AT A TIME (worktrees; `df` between) |
| 0:25-1:20 | lead | P0-calib (outside the repo, section 5); P0-shelf's order from NIGHT56's numbers; n57-r0: a build with every shelf row ON in the copy, its reels = the row-on side at HEAD |
| 0:25-3:30 | L, G, V | their entries, one slice at a time, commit every slice |
| ~1:30 / ~2:40 / ~3:15 | lead | each build: cherry-pick the builders' new commits (`git cherry-pick`; builders cannot merge), `n57-gates.sh --flow`, `bash Tools/night/n41-build.sh n57-rK`, rows on in the copy (`node Tools/night/n46-build-rows.cjs`), reels, ONE batched critic. r3 (3:15) is L's level 3 only |
| after the r1 gate | lead | **checkpoint:** verbs37 x5 on n57-r1 with the rows that shipped (NIGHT56: 4 min); if it passes, tag `u3-n57-r1` (the fallback play build). No smoke while builders run (L-099: focus) |
| 3:30 | builders | last commits; builders stop |
| 3:30-4:10 | lead | freeze: `n57-freeze` from HEAD, verbs37 x5, the suite (`n41-suite.sh n57 Core`, then `... n57 Game ABCDEFGHIJKL`, then `... n57 Game NPRSTUVWZ class`; NOT Game M), the smoke with nobody at the machine (`n56-smoke-steal.sh` method), tag `u3-n57` |
| 4:10-5:00 | lead | the close (section 7); Game M (leftover X1) only after the tag, never before it |
(NIGHT56 measured: a Player build ~1 min, verbs37 x5 ~4 min, the smoke ~4 min, a batched critic 15-25 min; the usage limit hit at 3 h 12
min with three builders: the checkpoint tag after r1 is the night's insurance.)

## 4. The shared commit and the baselines (lead)
- **Baselines at HEAD with every n56 / n57 row off: DONE (section 1).** The commands that ran (neither has `--help`; n47human has no
  `--out`): `dotnet run --project Tools/SimHarness -c Release -- n46flow --level n52-foundry,n52-iceline,n52-kiln,n52-span,n52-spire
  --seeds 10 --out Logs/n57/flow-base.txt` (NOT `--level all`); `dotnet run --project Tools/SimHarness -c Release -- n47human --boss wyrm
  --level n52-kiln --seeds 40 > Logs/n57/wyrm-base.txt 2>&1`. For seed-split noise bands, run the same with `--first 11` (n46flow /
  n47human both take it) only when an entry's MAIN is within 2x of its threshold.
- **The shared commit: DONE (section 1)** by `Tools/night/n57-shared.cjs` from `Logs/n57/rows-def.json`: `press57` (sim, G), `kiln57`
  (a level row: `levelFrom` n52-kiln, `levelTo` n57-kiln; the maps.list row and a placeholder level so ON never ends the campaign).
  NIGHT55-56 rows keep their names. `FeelReel.N57.cs` (a157 L, a257 G, a357 V), `N57SharedTests`, `SurvivalSim.N57.cs`,
  `Tools/night/n57-gates.sh [--flow]`. `-skygrindOff57` = NIGHT56's game.
- **Owners:** L owns level 3's plan / generator / ROOMS.md level 3 section and clear55 / doors on level 3; G owns tokens56, press57,
  kill55, wyrm55, wick55, rank56 (sim + their views); V owns colour56, rim56, yellow56, grade55, brief55 (view rows). Every spawn names its
  files.
- `Research/2026-10-21-wave57/builder-brief.md` (NIGHT56's brief with the n57 names, the files, the shipping rule, and "after every commit
  the default SimHarness build prints the fingerprint EQUAL"); commit it with the shared commit. Builders read this file by its absolute path
  `C:\Users\gaige\dev\SKYGRIND-unity\NIGHT57.md` (untracked).
- `Logs/n57/slate.json` (force-add): one entry per slice, `passes: false`, each with its MAIN and GUARD numbers named. Only the lead flips
  `passes`, by the rule in section 0 and the critic's confirmation.

## 5. The slate (contracts; builders build to the doneWhen AND the notes)

### P0. The lead (0:35-1:20)
- **P0-shelf:** apply section 0's rule to each NIGHT55-56 row. NIGHT56's numbers (section 1) only ORDER the work; the row-on side is
  re-measured at HEAD (wyrm55: `n47human ... --rows wyrm55` beside wyrm-base; view rows: n57-r0's reels). Expected (re-check):
  wyrm55 ON (every number better); grade55 ON for THE WICK only (THE CROWN's grade stays off: its tells are a guard);
  the rest go to their builder with the ONE guard or main number that holds them (kill55 jitter, colour56 tells, brief55 overlap, tokens56
  guards to measure, wick55 deaths 2 -> 7 vs wick54, rank56 main unmoved, rim56 / yellow56 main missed). DECIDE57 a line per row.
- **P0-calib (one critic, <= 24 images, BLIND):** a subagent spawned in this repo loads CLAUDE.md -> CORE.md and the auto-memory, which
  quote GG's verdicts, so it is not blind. Run it OUTSIDE the repo: copy the frames, renamed A1..An (the key in `Logs/n57/calib-key.json`,
  never in that folder), into a new scratch folder `<scratchpad>/calib57/`, write the rubric there, and run `claude -p` with that folder
  as the working directory (no `--add-dir` to the repo). The critic gets, blind and unlabelled, matched frames from n35-r* and n37-r* (the
  same reel scenarios where both exist, e.g. verbs / a level's entry), and n41-r7 / n42-r4's level 1 ride frames, plus the rubric. It must
  (a) say which build a player would call better (GG: n37 over n35) in BOTH orders, and (b) name, before it sees GG's words, the complaint
  GG made about each (the ride on n41-r7 / n42-r4; the UI on n42-r4). Score in `Research/2026-10-21-wave57/calib.md`. **PASS = (a) right in
  both orders and (b) 2 of 3 named.** A passing critic's rubric and prompt become tonight's gate critic. A failing one: the gate critic
  judges only evidence and eye-only lines (rule 4), never "would GG like it"; log it as a thread.

### L. LEVEL 3 FROM SCRATCH (builder L; the biggest share of the night: GG 3: yes)
Read: DESIGN-RULES A (A-1 every piece a reason, A-2 one loop, A-9 THE CARD FIRST) and R (R-1 the 30 s problem, R-2 the role curve, R-3 the
budget, R-4 placement); docs/ROOMS.md's level 3 cards (kb2 / kb2c / kb3 / k5); Research/2026-10-20-wave56/levels.md (R1-R15, THE RING /
THE GYM / THE CHINA SHOP); GAPS.md section 2 rows 1, 4, 9; design.md lessons 1, 4, 9 (Neon White: "every card has to have an obvious
purpose"; Hakita: difficulty by the enemy mix).
- **L0 the checkers read n57 (first, <= 30 min; the red team: they hard-code n52):** `n56-room.py` builds its path as `'n52-' + lv +
  '.json'` (line ~72): add a `--prefix` (default n52); I7 reads `Design/levels/n52/openings53.json` (the 18 n52 rooms): add
  `Design/levels/n57/openings57.json` for the new rooms; `n52-contract-all.sh` / `n53-contract-all.sh` compare the n52 level: give them
  the n57 level. A planted case per tool still FAILS. New files only (`Tools/night/n57-l-*.py` / `.sh`) or one flag per old tool.
- **L1 the cards (<= 20 min):** a new docs/ROOMS.md section "Level 3 as n57-kiln": the level's arc in one paragraph (its verb, the
  THUNDERCLAP, taught in room 1, its order of roles), then an A-9 identity card per combat room, 5 or 6 rooms, each with a 30 s problem no
  other room in the game has (grep ROOMS.md), and the role curve: one new role per room at most (R-3), the burrower and charger introduced
  before they combine, the last combat room a combine room that foreshadows THE KILN WYRM (E-7). k5 THE KILN'S HEART stays (the boss room
  and the worm unchanged). **The worm's lead-in beats** (SurvivalSim.N47.Kiln.cs: the glimpse in room `kb1`, the pass in room `kb2`, the
  cameo) key on those room ids: keep `kb1` (the start link) and give the first new combat room the id `kb2`, or move the beats in a new
  partial with one call-site line; doneWhen: the beats fire on n57-kiln as on n52-kiln (a harness or a157 line printing glimpse / pass /
  cameo counts on both).
- **The slots:** n57-kiln has 5-6 combat-room SLOTS in order. A slot holds a NEW room (from scratch) once that room passes L3's checks;
  until then it holds one of n52's kb2 / kb2c / kb3 (bit for bit), so the level always plays end to end.
- **L2 the generator:** `Tools/night/gen-n57-kiln.cjs` writes `Design/levels/n57/kiln.plan.json` FROM SCRATCH for the combat rooms (no
  n52 box copied into them; links, the start and k5 may be kept bit for bit and asserted), every piece with its `job` naming a card field,
  then levelgen and --finish to `Design/levels/n57-kiln.json` + the StreamingAssets copy (the run order in the file header, as gen-n52-kiln.cjs).
  The row `kiln57` swaps n52-kiln -> n57-kiln; off = n52-kiln bit for bit.
- **L3 the rooms, one at a time (commit each):** doneWhen per room: n56-room.py PASS (no dead ends, a foothold, every solid's job, first
  wave in view); n53-identity.py PASS; the contract 14/14 (L0's n57 form); walk bot 10/10. Then per level: MAIN = >= 5 combat rooms of
  which >= 3 are NEW and pass (n52-kiln: 3 rooms, 0 new), and n46flow kiln clear 150-240 s (bot; it is 92.2 s); GUARDS = stall <= the
  n52-kiln baseline (25.5 %) plus its band, deaths inside the flow table's band (8), verbs37 level 3 nothing lost (31), the worm's beats
  fire; the batched critic gets a 4-frame strip per room (entry, mid, high, the loop) against its card and GG's dislikes ("random lava
  pits random pillars that serve no purpose", "teleport into these random rooms").
- If time runs out: kiln57 ships ON only if MAIN holds (>= 3 new rooms passing, >= 5 rooms); else OFF, SHORT with the measured reason,
  the passing rooms kept behind the row for the next night. L's last rooms reach a critic at r3 (3:15), not after.

### G. ENEMY PRESSURE and KILLS (builder G; sim rows: golden 0, replays exact, fingerprints EQUAL with every n56 / n57 row off)
Read: Research/2026-10-20-wave56/enemies.md, design.md lessons 2, 3, 5; DESIGN-RULES E-4, E-5, C-1, C-2.
- **G1 tokens56 to ON:** measure its guards against flow-base (deaths per level in band, clear times <= +10 %); fix per-enemy idle where
  it hurts (target, not a guard). MAIN = attack starts inside the sim cone (44.6 % baseline).
- **G2 press57 (new sim row; GG 2: yes): standing still loses.** On EXISTING kinds only (no new enemy, no model change): (a) a delayed
  attack aimed where the player stood (ULTRAKILL's Virtue, ~1-3 s delay; design.md lesson 2), given to the spitter's or heavy's existing
  attack with its own red-orange ground tell (E-4: >= 0.8 s) on the spot; (b) if tokens56 does not already, ranged aim worsens with the
  player's speed. Numbers in the row (config, never hand-typed). MAIN = the bot's stall share on foundry / iceline / kiln (25-37 %
  baseline) down by >= 5 points; GUARDS = deaths per level in band, clear time <= +10 %, a moving player's hits taken not up. A frame per
  tell in a257 (the critic checks it reads in one frame).
- **G3 kill55's guard:** a354 jitter back under 0.191 with kill55 on; hit marks you can see (>= 5 % of the body the next frame) inside
  the medium-juice band (design.md lesson 3: extreme juice scores worse); then ON by the rule (its main, the blind pair, already won).
- **G4 the rest of G's shelf, in this order, one attempt each:** wick55 (deaths back to wick54's band; or the T-59 pilot climbs), rank56
  (it must move the DULL share or it is retired, with T-137's test fixed or explained).

### V. THE COLOUR BUDGET and THE NEAR MISSES (builder V; view rows only)
Read: Research/2026-10-20-wave56/colors.md, V.md; design.md lesson 7 (Valve: the world under a saturation cap, gameplay the brightest);
github.md (adobe/leonardo, DaltonLens: Apache-2.0 / MIT; study, port only with the licence logged).
- **V1 colour56 to ON: cap the world first.** A world saturation cap (per level, in colour56's numbers) so the tells stay the most
  saturated thing, THEN the per-level key / accent under it. MAIN = the blind pair (already won; re-run both orders on the new frames);
  GUARD = frames where the world out-saturates the tells 28 / 36 -> <= the n52 baseline's count (`n56-colour.py`); a greyscale check
  (enemies and hazards in the top values, E-3) and a deuteranopia simulation of 4 frames (cyan vs red-orange still apart).
- **V2 brief55:** the 2.4 px overlap gone (kiln, spire), then ON.
- **V3 grade55 THE CROWN:** tells kept >= the baseline's (no grade) count; THE WICK's ride panels carry the cyan ride cue.
- **V4 rim56 / yellow56:** one attempt each at their main numbers; else retired with the reason.

### Seeded leftovers (only after every P0 / L / G / V entry has had its first attempt; in this order)
1. docs/ROOMS.md "As shipped" table, generated (`Tools/night/n57-rooms-table.cjs` from Design/levels/n52 + n57 plans: level, room, kind,
   par, waves, roles, the card's title); ROOMS.md's older sections stay below it. (lead)
2. A 4th-6th new kiln room if L3 finished early. (L)
3. The door / link lamps on spire's f1550 (NIGHT56: still dark; level data in place, as NIGHT56's lamps: MAIN = verbs37's flat frames
   5 -> fewer, GUARD = nothing lost). (lead)
4. **Only after the tag `u3-n57`:** the Game M hang: `bash Tools/night/n41-suite.sh m57 Game M class`, ONE class per batch (the runner
   waits for an idle Editor and never re-runs a timeout); the class that never returns gets a thread and a DECIDE line. If the Editor
   wedges, stop: GG restarts it (never kill it). (lead)

## 6. The gate (one batched critic per build; the rubric of Research/2026-10-20-wave55/critic-gate.md + section 0's rule)
- After each build's reels, ONE fresh critic (P0-calib's prompt if it passed) gets every entry with new evidence: MAIN / GUARD numbers
  with their baselines, raw log paths, <= 4 frames per entry (described before the numbers), the planted-fail lines. It writes
  `Research/2026-10-21-wave57/gate-rK.md`: per entry, "evidence real" yes / no, each guard held yes / no, each eye-only line PASS / FAIL.
  Bot numbers are labelled "bot". At most 30 images per critic; split past that.
- The lead flips `passes` by section 0's rule. A tie on a blind pair is not a win.

## 7. The close (4:10-5:00; the close table no earlier than 4:00 by `date`)
`NIGHT57-LOG.md` close table (every slate entry: state ON / OFF / SHORT, MAIN and GUARD before / after, frame path, row, how to switch it
off; every NIGHT55-56 shelf row's final state), `NIGHT57-REPORT.md` for GG in plain words (what changed in level 3 room by room, what is
now ON and the one-line PLAY note per row with its off switch, P0-calib's result, what is SHORT and why), HANDOFF.md (a new top block),
DECIDE57.md, `wave-memory` (harvest w57; THREADS under its 60-line cap), the forge experiment scored (section 8). Commit with explicit
paths. Nothing pushed, nothing merged.

## 8. The forge experiment (scored at the close)
Forge experiment: the shipping rule (section 0) replaces "every critic line passes". Hypothesis: built rows reach the player. Measure in
NIGHT57-REPORT: rows ON at the close / rows with a gate verdict (NIGHT56: 1 of ~12), and any ON row a later GG play reverses (scored at
the next harvest).

## 9. Builder spawn template (every builder)
```
You are builder <L|G|V> on NIGHT57 of SKYGRIND (Unity 6000.3.7f1, URP; a pad-first movement shooter at dusk), C:\Users\gaige\dev\SKYGRIND-unity.
Objective: <one sentence>.  Read first: CLAUDE.md, docs/PRINCIPLES.md, docs/DESIGN-RULES.md, C:\Users\gaige\dev\SKYGRIND-unity\NIGHT57.md (untracked: by this path) sections 0, 2, 5, 6, 10,
Research/2026-10-21-wave57/builder-brief.md, GAPS.md, and your research file(s).  Your entries: <ids, MAIN, GUARDS, doneWhen + notes>.  Your files: <paths>.
You work in your own git worktree branched from port/stage3 at the NIGHT57 shared commit; the lead integrates by cherry-pick.
You may not: run the Unity Editor, a build, a Player, a reel or run_tests, push, merge, or commit outside your branch.
After EVERY commit: `dotnet run --project Tools/SimHarness -c Release -- fingerprint` (no -p:Extra) builds and prints EQUAL; after every
commit that touches Assets/Scripts/Core: `dotnet run --project Tools/CoreHarness -c Release` prints "0 failing, 0 mismatches" (22 replays
exact) and `... fingerprint --off56` / `--off57` print EQUAL. A campaign-only sim row (tokens56, press57) also: n46flow with `--off57` on
the n52 levels equals Logs/n57/flow-base.txt (wall times aside).
Output: commits (explicit paths + .meta), notes in Research/2026-10-21-wave57/<letter>.md, a report under 300 words with every sha, every
number before / after (MAIN and GUARDS), the reel specs. Last line: N57-<letter>: round <r> · built <ids> · commits <shas>.
```

## 10. Hard rules (CLAUDE.md restated; nothing here overrides it)
- Never commit to `master`, push, merge or `gh auth switch`. Explicit paths plus `.meta`; never `git add -A` / `.`. Never stage GG's four
  files (ProjectSettings/UnityConnectSettings.asset, ProjectSettings/EditorBuildSettings.asset, Assets/ProjectSettings/UIEffectProjectSettings.asset,
  Assets/Scenes/Arena.unity) or the Play-mode font rewrites (the four `Assets/Resources/Fonts/*SDF.asset` files are GG's uncommitted
  edits: never stage, never `git checkout --` them); restore the TMP fallback font asset before commits.
- Only the lead drives the Editor, builds, Players, reels, the suite and git on `port/stage3`. `editor_focus` before Play checks; never
  recompile in Play mode; never `unity test`; one Play session or build at a time. Close every Player by its PID. Check for leftover
  `n41-suite` processes at the start (2026-10-21: one outlived its session).
- Movement exact: golden 0 mismatches, replays exact at every Core commit with tonight's rows off; fingerprints plain / --off56 / --off57
  EQUAL before each code commit. Mover.cs only where an entry names it (none tonight).
- Numbers from config through `GameConfig`; Unity-only numbers in UnityTuning partials with a comment; config edits by a `.cjs` that
  asserts nothing else changed; Mulberry32 only in sim code; no allocation per sim step. No audio. No enemy looks, no new enemy kinds.
- A tool or harness file must build in the DEFAULT SimHarness project. Any new view that copies the stone material passes vertex colours.
- Docs with the Write / Edit tools only. Write only what was measured or read; otherwise "not checked". Delete only your own output.

## 11. If something goes wrong
- **Usage limit or 429 stops the lead too.** Everything is committed; `u3-n57-r1` is the fallback. At the resume: after 3:45, skip to the
  freeze; after 5:00, no new building: write the close table at once from what is gated. Resume builders by SendMessage, not new agents.
- **The Editor does not answer at A0** (GG did not restart, or it wedges again): never kill it; build from the batch copy
  `C:\Users\gaige\dev\SKYGRIND-unity-batch` (memory: batch-build-and-editor-stall), run the suite batch-mode there, and say so in the report.
- **Disk under 12 GB:** prune your own frames; under 6 GB no new reels.
- **A row breaks an old verb:** off, logged, SHORT. Never ship a lost verb (PRINCIPLES 1).

## 12. Closing reminder
- Ship what is built: a row that beats its baseline and holds its guards goes ON tonight, with an off switch and a PLAY note.
- Level 3 is built from its cards, not patched: a piece no card field asks for is cut.
- Caps are caps: at 5:00, close with what is gated.

## Kickoff (paste into a fresh Claude Code session in `C:\Users\gaige\dev\SKYGRIND-unity`, after restarting the Unity Editor)
```
Read CLAUDE.md, HANDOFF.md, then NIGHT57.md and DECIDE57.md, and run the night exactly as written as the lead. At least 4 hours, hard cap
5 hours from your first `date`. I will not answer: decide every call in DECIDE57.md and keep going. Start with `date`, then A0.
```

## /goal (paste after the kickoff)
```
/goal Met when this conversation shows NIGHT57-LOG.md's close table printed in full (every slate entry ON, OFF or SHORT, with MAIN and GUARD numbers before / after or "n/a", a frame path where one exists, every NIGHT55-56 shelf row's final state) and `git tag -l u3-n57` printing exactly u3-n57; or when a printed `date` shows 5 h 30 min since the first `date` (start the close by 4:10 whatever is unfinished; past 5 h no new building). Print `date` and elapsed time at the start of every turn. Run NIGHT57 nonstop as the lead; never ask me; decide every call in DECIDE57.md. Order: A0 baselines, the shared commit, builders L (level 3 from scratch), G (enemy pressure, kills), V (colour budget, near misses) one at a time; P0-shelf and P0-calib while they build. Ship rows by NIGHT57.md section 0's rule, never by "every line passed". One batched critic per build; checkpoint tag u3-n57-r1 after the r1 gate. Commit every slice with explicit paths. Close with the close table, NIGHT57-REPORT.md, HANDOFF, DECIDE57, wave-memory harvest, and tag u3-n57.
```

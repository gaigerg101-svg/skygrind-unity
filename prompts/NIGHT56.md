# NIGHT56.md: LEVELS FIRST. Rooms that make you move, enemies that come at you, a style rank that pays, one colour per level (lead + three builders + batched critics; at least 7 h, hard cap 7 h 30 min)

GG, 2026-10-20 ~23:08, verbatim (this outranks everything below it):
> what should overnight prmpt focus on to imporve levels specifically, gameplay, eniemies, colors, ect, resrecah plan for next 20 mins and build out 7 hour night prompt

**GG will not answer during the run.** Decide every call in `DECIDE56.md` (untracked) with its reason and the alternative rejected, and
keep going. If GG writes, his words come first. Base: `port/stage3` at tag `u3-n55` (check `git rev-parse u3-n55` against HEAD; if HEAD
is newer, base on HEAD and log why). t0 = the first `date` you print. Print `date` and the elapsed time at the start of every turn.
Research for this night: `Research/2026-10-20-wave56/` (levels.md, enemies.md, colors.md, gameplay.md, IDEAS-POOL.md). Read it; do not
redo it. Re-check every number below before you trust it.

## 0. Why the night is shaped this way

**What GG will notice in the first minute** (NIGHT55's three critics, Research/2026-10-20-wave55/critic-*.md, all read from Player frames):
- Rooms read as big symmetric halls; the counter says 7-14 enemies, the entry frame shows 1-4, most far away (veteran #4, ggproxy #1).
- Kills are a flat 3-frame pause on an undamaged pill; the style rank reads DULL in 2772 / 2772 frames and pays nothing (veteran #2, #7).
- The decor has no colour direction (on vs off cannot be told apart); yellow and red show up on things that are not parry / danger
  (ggproxy #5, firsttime #2, #4). THE WICK's ride walls are painted in the danger colour.
- No boss says what it wants; text lands on the boss (firsttime #1, #7, #8).

**What NIGHT55 taught about running a night (the forge's one change, see section 8):** five Opus builders plus six research / critic
agents hit the usage limit at 2 h 28 min and every agent stopped at once. Commit-every-slice held (nothing was lost), but 25 of 26
entries never reached a gate. Tonight: **at most three builders, one batched critic per build, image-heavy work capped.** A slice
counts when a fresh critic passes it, not when it is built.

**The spike already done (wave-forge S5):** the next wave already releases while <= 2 bodies stand (design.n46.overlap46, ON;
Campaign24.cs:852-866). The "1-4 of 7-14 visible" problem is WHERE waves spawn: `r.WaveNodes43(w)` / `waveNodes33` / `r.nodes`
(Campaign24.cs:913), i.e. level data. Fixing it is a level-data job with a checker, no new sim row.

## 1. Facts already measured (re-check them)
- Play build `Builds/n55-freeze` (u3-n55, c1a1a273): every design.n55 row OFF; verbs37 27 / 21 / 31 / 31 / 31; the smoke DONE all steps.
- Gates at u3-n55: golden 0 mismatches, 22/22 replays exact, fingerprints plain / --off54 / --off55 EQUAL (`bash Tools/night/n55-gates.sh`).
- NIGHT55's unpicked work, on branches (HANDOFF.md's NIGHT55 block has the exact order and the one compile pairing):
  B `worktree-agent-a760a198bdb92ba37` (wick55 bot 6 -> 57 % damage in windows; grade55 r2; a155 FIGHT MODE), C `worktree-agent-a390df1feffc64bd9`
  (ssao55, step55 reel fix, decor55: pick 5fe9b6cc with 283f3c41), K `worktree-agent-a5f6051c74748e579` (kill55 r2, brief55 teach text),
  R `worktree-agent-adc56f0a78ec83566` (clear55, doorDark slice 1).
- NIGHT55 measured GREEN on n55-r1 but never gated: the focus-proof smoke, iceline g7 / g5 (I1 99 -> 52 %, 76 -> 50 %), T-125 (0 -> 6
  fires), THE WICK's grade (walls 149 / 140 / 122 deg from danger). RED on n55-r1: THE CROWN's grade (tells 282 / 533 px kept), THE
  WICK's hot wall (burn S 0.48 vs walls 0.74), a355's chain (input), a255's step (input).
- Rooms still failing the identity checks (Logs/n55/identity-r1.txt): foundry a3 (I1 69 %), iceline g3 / g4 / g8 and spire v2 (I2 1.08-1.28),
  kiln kb2 and span p5 (I4 pads on walk lines). `python -I Tools/night/n53-identity.py table kiln:kb2,kb2c,kb3 foundry:a3,a5,b1 iceline:g2,g3,g4,g5,g7,g8 spire:v2,v5,v6 span:p1,p3,p5`.
- Flow (the walk pilot, not a person: Logs/n52/flow-freeze/table.md + NIGHT53): stall 25-36 % on foundry / iceline / kiln, door waits
  11-14 s, iceline clear 221 s.
- THE KILN WYRM (n47human, NIGHT49): median 122.6 s, p90 256.9 s, 68 deaths in 40 fights; not re-measured since.

## 2. Scope
- **In:** the campaign's combat rooms (levels 1-5), wave spawn placement, enemy BEHAVIOUR (tokens, accuracy vs speed, advance rules),
  the style rank's payoff, kill feedback, each level's colour key, NIGHT55's built rows.
- **Out (do not start):** enemy looks / models / new enemy kinds (GG's: CLAUDE.md rule 7; write a proposal page only), audio, level 6,
  the UI's style (new elements in the existing styles are fine), the wall ride's speed (GG 2026-10-13 "you gain speed too quickly"),
  the 182 old worktrees, GG's four modified files. New ideas go to docs/memory/IDEAS.md, never into code.

## 3. Roles and the clock (t0 = your first `date`; close table no earlier than 6:50; hard cap 7:30)
| when | who | what |
|---|---|---|
| 0:00-0:30 | lead | A0 bearings (git, `df` > 12 GB, HANDOFF NIGHT55, this file, the research); the BASELINES at u3-n55 (section 4, last bullet: ~12 min, never beside a Player) |
| 0:30-1:20 | lead | **P0-pick FIRST:** cherry-pick NIGHT55's branches in HANDOFF's order (builders cannot merge: their worktrees must start from the picked code); gates; THEN the shared commit (section 4); then spawn L, G, V ONE AT A TIME with `df` between |
| 1:20-2:40 | lead | P0's build `n56-r1`, reels for every NIGHT55 row (verbs37 x5 waits for r2, which runs it anyway), ONE batched critic |
| 1:20-5:00 | L, G, V | their entries, one slice at a time, commit every slice |
| ~2:40 / ~3:50 / ~5:10 | lead | builds n56-r1 (P0) / r2 / r3 (`bash Tools/night/n41-build.sh n56-rK`), rows on in the copy (`node Tools/night/n46-build-rows.cjs`), reels (`n56-reels.sh` from n55-reels.sh, its floor 6 GB; a SKIPPED reel marks its entry RED, never "passed"), then the batched critic |
| after the r2 gate | lead | **checkpoint freeze:** verbs37 x5 + the smoke on n56-r2 with the rows that passed; if both pass, tag `u3-n56-r2` (the fallback play build if a limit ends the night) |
| 5:00 | builders | last commits (r3 picks at 5:10); builders stop |
| 6:00-6:45 | lead | freeze: its OWN build `n56-freeze` from HEAD (the repo's config with the ON rows), verbs37 x5, the suite batches tonight's rows touched, the smoke with nobody at the machine (no agent running, idle >= 60 s), tag `u3-n56` |
| 6:45-7:30 | lead | the close (section 7) |

## 4. The shared commit (lead, after P0-pick and before any builder starts; model `Tools/night/n55-shared.cjs` + `n55-addrows.cjs`)
- `design.n56` rows, all OFF, each with an empty `numbers` object, from `Logs/n56/rows-def.json`, ordered by builder (G's rows, then V's):
  `rank56` (sim, G), `tokens56` (sim, G), `rim56` (view, V), `colour56` (view, V), `yellow56` (view, V). Each builder writes its numbers
  with ONE writer script of its own. Rooms (L) are level data in place (as NIGHT53-55, D-4 of DECIDE55): no row; the comparison side is
  `Builds/n55-freeze`.
- **Owners of NIGHT55's files:** V owns builder B's grade55 files, `FeelReel.N55.Boss.cs` (a155) and `Tools/night/n55-b-rows.cjs`; G owns
  THE WICK's fight (wick55) and puts any FIGHT MODE change in a NEW partial file; G owns kill55's fix (the hold ladder lives in kill55
  only: no second ladder row); L owns clear55 and doorDark. Every spawn names its file list.
- `FeelReel.N56.cs`: reel phases a156 (L), a256 (G), a356 (V); `N56SharedTests`; `Tools/night/n56-gates.sh` (fingerprints plain /
  --off55 / --off56). `-skygrindOff56` = NIGHT55's game (rooms aside). Write `Tools/night/n56-shared.cjs` from n55-shared.cjs: its guard
  "the last design key" is now `n55`, its anchors are `PhaseN55` / `N55ProofBody` in FeelReel.N27.cs.
- `Research/2026-10-20-wave56/builder-brief.md`: NIGHT55's brief with the n56 names, the three builders' files, and the line "after every
  commit the default SimHarness build prints the fingerprint EQUAL". **Commit the research folder (levels, enemies, colors, gameplay,
  IDEAS-POOL, redteam, builder-brief) in the shared commit:** builders work in worktrees and NIGHT56.md is untracked (rule 1), so they
  read it by its absolute path `C:\Users\gaige\dev\SKYGRIND-unity\NIGHT56.md`.
- **The a156 room meter (numbers, not images):** per a452 entry frame, print the enemies inside the camera frustum, the nearest and
  second-nearest distances, the spawn tells up, and the room id (from FeelProbe's enemy rects, FeelProbe.cs:116). L2 and the critic read
  this line; no critic counts enemies by eye.
- **The colour meter's method (no role-ID render pass: out of scope):** world = the frame minus the HUD rects minus FeelProbe's enemy
  rects minus the sky (above the horizon row); tells = the danger / parry hue bands inside the boss / enemy rects. V writes it once
  (`Tools/night/n56-colour.py <frame dir>`), with a planted case.
- **Baselines at u3-n55 (A0, the lead; numbers every "before" is read against):** `dotnet run --project Tools/SimHarness -c Release --
  n46flow --level n52-foundry,n52-iceline,n52-kiln,n52-span,n52-spire --seeds 10 --out Logs/n56/flow-base.txt` (NOT `--level all`: that
  runs the old n45 / n40 / n41 levels, Harness.N46.Flow.cs:62); `n47human --boss wyrm --level n52-kiln --seeds 40` (the default level is
  n46-kiln, Harness.N47.Human.cs:110); a style column for n46flow if G1 needs it (G adds it; baseline with every n56 row off).
- `Logs/n56/slate.json` (force-add: Logs is gitignored): one entry per slice below, `passes: false`. Only the lead flips `passes`, only on
  a critic's PASS. An entry with two failed attempts becomes SHORT with the measured reason and the best attempt behind its row.

## 5. The slate (contracts; builders build to the doneWhen AND the notes)

### P0. NIGHT55's debt (lead, 0:30-2:40: the pick before the builders spawn, then its build, reels and critic)
- **P0-pick:** every NIGHT55 branch commit picked (HANDOFF order) or skipped with a DECIDE line; gates GREEN; n56-r1 built.
- **P0-gate55:** one batched critic on n56-r1 for NIGHT55's rows with these reels: a155 (THE WICK, THE CROWN; and FIGHT MODE), a255
  (kiln step, foundry SSAO + decor), a355 (kill chain), a455 (room clear), the smoke with planted steals (`bash Tools/night/n55-smoke-steal.sh
  n56-r1`); brief55's teach text waits for r2's verbs37 x5 (`node Tools/night/n55k-teach.cjs`, which arrives with K's pick). Each NIGHT55
  row turns ON only on its critic PASS; a RED row gets ONE fix by the builder who owns its area tonight (L rooms / doors, G kills / boss
  text / WICK, V grade / decor / SSAO). NIGHT55's slate entries close in Logs/n55/slate.json with tonight's result. NIGHT55's pick of 6
  commits took 18 min: budget ~45 min for 13 here; if the critic has not answered 20 min after its reels, the lead goes on and reads it
  when it lands; nothing NIGHT55 built turns ON without it.

### L. LEVELS (builder L; the biggest share of the night: GG said "levels specifically")
Read levels.md (rules R1-R15, recipes THE RING / THE GYM / THE CHINA SHOP), DESIGN-RULES A / R, docs/ROOMS.md.
**Order L1 > L2 > L3 > L4.** L4 (the moonshot) starts only if L2 PASSED at the r2 gate; otherwise it is SHORT ("conditional on L2").
- **L1 the room checker (first, 30 min):** `Tools/night/n56-room.py` adds to n53-identity.py, per combat room: R2 no dead ends (the walk
  graph incl. ride / pad / drop edges: 0 dead-end nodes), R5 a foothold, R8 every solid's job read from the plans' EXISTING `job` /
  `job36` fields (no new tags file: the plans already carry them), and "in view" per the room's OPENING (Design/levels/n52/openings53.json):
  held / vista rooms: >= 4 enemies or spawn tells visible from the foothold's eye height at any distance; ambushed / ambusher rooms: the
  first group within 15 m by midfield; spawns 8-15 m from the entry door, beside the foothold (not behind it), none with a line into the
  entry corridor. Planted case: it must FAIL a copy of one NIGHT52 room (L-091). Prints a table over the 18 rooms.
- **L2 spawns in view (the first-minute fix):** add per-wave `waveNodes43` entries (do NOT move `r.nodes`: highNodes merge into it,
  Campaign24.cs:323-324) so rooms pass "in view"; doneWhen: n56-room.py in-view PASS on >= 15 of 18 rooms; I7 (the openings) still PASS
  on every level; the a156 meter (it prints distance AND line of sight, not a452's yaw cone) on foundry / iceline / kiln entry frames
  agrees with n56-room.py on >= 80 % of rooms; n46flow on the n52 levels (the A0 command) stall <= 25 % on foundry / iceline / kiln and no
  clear time worse than Logs/n56/flow-base.txt by more than 10 %; verbs37 x5 nothing lost.
- **L3 the seven failing rooms, by ADDING pieces with jobs** (T-131: cuts alone break mirrors and give nothing): a3, g3, g4, g8, v2, kb2,
  p5. doneWhen: identity FAIL rooms 7 -> <= 2; each added piece carries its `job` in the plan and a bot contact count > 0 (ride / pad /
  cover); contract 14/14 per room; walk bot 10/10 per touched level; the batched critic's GG-persona line per room (one critic per build
  covers them all; no per-room agents: the usage limit).
- **L4 one showcase room (the moonshot, capped at 90 min, only after L2 PASS):** a room NOT in L3's list, rebuilt from scratch to THE RING
  or THE GYM recipe: the worst-reviewed of g2 / kb3 / p1 (NIGHT53's reviews: "would not satisfy GG") (GG 2026-10-09: "complete arena
  overhaul ... from scratch"). doneWhen: n56-room.py all PASS, identity PASS, contract 14/14, verbs37 on its level nothing lost, a strip of
  4 frames (entry, mid, high, the loop) judged by the batched critic against the recipe and as GG would. If it is not better on the
  critic's pair, it reverts.

### G. GAMEPLAY and ENEMY BEHAVIOUR (builder G; sim rows: golden 0 mismatches, 22 replays exact, fingerprints EQUAL with every n56 row off)
Read gameplay.md (the three proposals), enemies.md (M1 tokens, M2 accuracy vs speed, M6 push-forward, M10 waves on kills).
- **G1 rank56, the rank pays where it does not yet (sim):** the game's ranks are DULL, COOL, BOLD, AIRBORNE, SAVAGE, SKYGRIND
  (PortExport/config.json ~208-233; StyleSim.cs). What ALREADY ships (Style.cs:4-5): the rank multiplies points (points.rankMult
  [1, 1.1, 1.25, 1.5, 1.75, 2]), regen and stamina; a hit costs style (style.damagePenalty 30); movement states multiply style
  (style.stateMult 1.2); the rank-up slam exists (UiStyleMeter.cs:263-342). rank56 adds ONLY: the rank also multiplies heal36's
  close-kill heal and RIDE KILL's boost refund (DULL x1.0 ... SKYGRIND x2.0, the mapping in the row's numbers, read from the same rank
  index as points.rankMult). Measured with a style column added to
  n46flow (n52 levels) and a StyleSim profile, baselined at u3-n55 with the row off (the "~100 % DULL" seen on verbs37 reels is not a
  combat baseline). doneWhen (bot): the share of combat time at DULL falls by >= 30 points vs the baseline and a combat room reaches
  SAVAGE; StyleSim's golden records EQUAL with the row off (its pace targets, "varied: SAVAGE in 35-60 s", are not broken with it off);
  golden / fingerprints EQUAL off; a256 shows the rank leaving DULL on a combat frame. **Ships OFF for GG to try** (DECIDE56: GG
  2026-10-20 "don't change the core mechanics"; heal / refund scaling is his call): a PLAY note in the report.
- **G2 kill55's fix (NIGHT55's row, owned by G tonight; no new row):** P0's critic result on a355 decides the fix; the hold ladder lives
  in kill55 only (move54's 0.12 s cap; one hold per event, the largest; gameplay.md's 0.25 s parry hold is NOT adopted: it breaks the
  cap). doneWhen: a355 GREEN (chain >= 3 kills inside 1 s, hit mark >= 5 % next frame); a blind pair (Tools/night/n46-blind-pairs.cjs,
  both orders) picks ON as "the kill hits harder" 2 of 2.
- **G3 tokens56 (sim): enemies come at you from where you can see them.** The two-wind-up cap already ships (design tokens41, ON, cap
  2): tokens56 adds only three rules: tokens go to enemies "on screen" first, where on screen is a SIM cone from config `camera.fovDeg`
  (74) around the sim's look yaw / pitch, never the player's FOV setting (rule 4: tapes and replays must not depend on a viewer setting); ranged accuracy falls with the player's speed
  (DOOM 2016, enemies.md M2); idle enemies advance to a loop node in view. doneWhen (bot): >= 80 % of attack starts from enemies inside the
  sim cone; ranged hit rate at dash speed <= 50 % of standing; per-enemy idle-in-place time <= 20 %; deaths per level inside the flow
  table's own band against Logs/n56/flow-base.txt (span and spire have ~0-1 deaths per 20 walks: a percentage gate is noise);
  golden / fingerprints EQUAL off.
- **G4 THE KILN WYRM's tail (NIGHT55 P0-wyrm, never built):** wick54's method on wyrm55 (encounter numbers only; every attack pinned by a
  test). doneWhen: `n47human --boss wyrm --level n52-kiln --seeds 40` vs the A0 baseline on the same level: 40 / 40 killed, median 60-120 s,
  p90 <= 180 s, deaths <= 10.

### V. COLOURS and READABILITY (builder V; view rows only)
Read colors.md (the saturation cap, value first, 3:1 contrast floor, warm enemies / cool world, the per-level keys and the dusk-sky risk).
- **V1 colour56, MORE colour direction per level (GG: "way too bland" three times: w16, 2026-10-19, 2026-10-20).** Each level gets a
  KEY (its fog / sky / landmark hue, any coverage) and an ACCENT (1-5 % of the frame, on landmarks and decor), both saturated, never
  cyan, red-orange, yellow or magenta. No level has a neutral key: colors.md's warm-neutral kiln and achromatic spire are rejected (kiln is
  the worm's level, GG NIGHT47: "the same yellow-red-orange color theme": the kiln key goes to the warm side by S and value, not hue, and
  its danger tells stay the most saturated). Foundry: colors.md's oxide-green key wins over decor55's 72 deg accent (72 sits next to the
  parry yellow). doneWhen per level on a452 entry / mid frames (n56-colour.py): the key or accent at S >= 0.35 on >= 3 % of the frame;
  the accents pairwise >= 40 deg apart; sky pixels inside 5-70 deg at S > 0.35 = 0 %; nothing in the world above the tells' saturation;
  a blind pair both orders picks ON as "which has more colour direction" 2 of 2 (pairs for 2 levels per build: the image cap). GG's eye
  decides the keys (DECIDE56 rows). **The trap GG already named:** one hue over every surface reads as a tint (T-112 "the grade's teal
  shift"; spire v6 "one olive-green on every surface"): walls keep value structure; a whole-frame tint is a FAIL (rubric line 4).
- **V2 rim56, enemies the warmest and brightest thing:** a warm rim / emissive band on enemies (lighting, no model change: rule 7).
  doneWhen: on a452 mid frames the enemy rects' mean V and S above the world's p90 on >= 90 % of enemies in view; red-orange stays theirs.
- **V3 yellow56, yellow means parry only:** non-parry yellow on the guns (the THUNDERCLAP / TORCH plates) and the floor trims moved off
  the parry band. Boss and enemy parts in yellow (THE CROWN's gems) are GG's design: listed in DECIDE56, not changed. doneWhen
  (n56-colour.py): yellow (45-70 deg, S > 0.5) pixels outside parry tells and outside enemy / boss rects <= 0.5 % per frame on verbs37 x5.
- **V4 the boss palettes' second round (NIGHT55 B's r2 is on its branch):** THE CROWN tells kept >= 0.8; THE WYRM's turn inside 15-55 deg
  (GG NIGHT47: "the same yellow-red-orange color theme"); THE WICK's hot wall the most saturated region while it burns; its ride panels
  carry the cyan ride cue (firsttime #2).

### Seeded leftovers (only after every P0 / L / G / V entry has had its first attempt; in this order)
1. Doors and links lit (NIGHT55 P1-doorDark; R's slice 1 arrives in P0): verbs37 x5 0 frames with luma std < 0.03 (L, else V).
2. NIGHT55's P1s never started: boss text placement (bossText55), FOV with speed (fov55), floating lights (fixtures), edge washes.
3. finish55 (the boss-kill freeze), brief55's boss brief card (one verb line per boss), speed55.
4. A second showcase room (the other of a3 / g4) if L4 passed.
(Weapon freshness is NOT a leftover: it already ships as design.freshness / Freshness19; a second row would double-pay style.)

## 6. The gate (one batched critic per build; NIGHT55's rubric, Research/2026-10-20-wave55/critic-gate.md)
- After each build's reels, ONE fresh critic subagent gets every entry that has new evidence: the doneWhen + notes, the raw log paths,
  <= 4 frames per entry (described before the numbers), and the planted-fail lines. It writes `Research/2026-10-20-wave56/gate-rK.md`
  with PASS / FAIL per line per entry. Approving a miss is the failure it is there to prevent. Bot numbers are labelled "bot".
- At most 30 images per critic (NIGHT55's critics hit the image limit at 26-36); past that, split the gate into two critics. Numbers
  come from meters (a156, n56-colour.py, the a1xx lines), so images are for what only the eye can judge. Blind pairs are shown in both orders.
- The lead flips `passes` only on PASS. A slice GG asked for by name stays ON on a tie, and the tie goes in the report.

## 7. The close (6:45-7:30; the close table no earlier than 6:50 by `date`)
`NIGHT56-LOG.md` close table (every slate entry: state, numbers before / after, frame path, row, how to switch it off; every NIGHT55
entry's final state), `NIGHT56-REPORT.md` for GG in plain words (what a player sees in the first minute of each level now, the frames to
look at, what is SHORT and why, GG's calls: each level's colour key, the showcase room, rank56's multipliers), HANDOFF.md (a new top block),
DECIDE56.md, `wave-memory` (harvest w55 + w56; THREADS consolidated under its 60-line cap), docs/memory/IDEAS.md (the pool's runners-up),
the forge experiment scored (section 8). Commit with explicit paths. Nothing pushed, nothing merged.

## 8. The forge experiment (scored at the close)
Forge experiment: at most three builders and one batched critic per build (NIGHT55: five builders + per-entry critics hit the usage limit
at 2 h 28 min, 1 of 26 entries gated). Hypothesis: more entries reach a gate before any cap. Measure in NIGHT56-REPORT: entries gated /
entries attempted, and the time of the first gate (NIGHT55: 1 / 26, first gate 18:16 = 1 h 10 min).

## 9. Builder spawn template (every builder; models NIGHT55's Research/2026-10-20-wave55/builder-brief.md: copy it to wave56 with the
n56 names and use it)
```
You are builder <L|G|V> on NIGHT56 of SKYGRIND (Unity 6000.3.7f1, URP; a pad-first movement shooter at dusk), C:\Users\gaige\dev\SKYGRIND-unity.
Objective: <one sentence>.  Read first: CLAUDE.md, docs/PRINCIPLES.md, docs/DESIGN-RULES.md, C:\Users\gaige\dev\SKYGRIND-unity\NIGHT56.md (untracked: by this path) sections 0, 2, 5, 6, 10,
Research/2026-10-20-wave56/builder-brief.md, and your research file(s).  Your entries: <ids, doneWhen + notes>.  Your files: <paths>.
You may not: run the Unity Editor, a build, a Player, a reel or run_tests, push, merge, or commit outside your branch.
After EVERY commit: `dotnet run --project Tools/SimHarness -c Release -- fingerprint` (no -p:Extra) builds and prints EQUAL.
Output: commits (explicit paths + .meta), notes in Research/2026-10-20-wave56/<letter>.md, a report under 300 words with every sha, every
number before / after, the reel specs. Last line: N56-<letter>: round <r> · built <ids> · commits <shas>.
```

## 10. Hard rules (CLAUDE.md restated; nothing here overrides it)
- Never commit to `master`, push, merge or `gh auth switch`. Explicit paths plus `.meta`; never `git add -A` / `.`. Never stage GG's four
  files (ProjectSettings/UnityConnectSettings.asset, ProjectSettings/EditorBuildSettings.asset, Assets/ProjectSettings/UIEffectProjectSettings.asset,
  Assets/Scenes/Arena.unity) or the Play-mode font rewrites; restore the TMP fallback font asset before commits.
- Only the lead drives the Editor, builds, Players, reels, the suite and git on `port/stage3`. `editor_focus` before Play checks; never
  recompile in Play mode; never `unity test`; one Play session or build at a time. Close every Player by its PID.
- Movement exact: golden 0 mismatches, 22 replays exact at every Core commit with tonight's rows off; fingerprints plain / --off55 /
  --off56 EQUAL before each code commit. Mover.cs only where an entry names it (none tonight).
- Numbers from config through `GameConfig`; Unity-only numbers in UnityTuning partials with a comment; config edits by a `.cjs` that
  asserts nothing else changed; Mulberry32 only in sim code; no allocation per sim step. No audio. No enemy looks.
- Any new view that copies the stone material passes vertex colours or uses `Light54.StoneOrLit`.
- A tool or harness file must build in the DEFAULT SimHarness project (NIGHT55: C's -p:Extra file broke the fingerprint gate).
- Docs with the Write / Edit tools only. Write only what was measured or read; otherwise "not checked". Delete only your own output.

## 11. If something goes wrong
- **Usage limit or 429: it stops the lead too** (NIGHT55: every agent stopped at 19:33, resumed 22:01). Everything is committed and the
  checkpoint tag `u3-n56-r2` is the fallback play build. At the resume: after 6:30, skip to the freeze; after 7:30, no new building and no
  freeze: write the close table at once from what is gated (the 6:50 floor and "at least 7 h" are waived; the cap still holds for building). If time is left, resume each builder by
  SendMessage (their work and context survive), not by a new agent. Keep the agent count low all night: 3 builders + 1 critic per build.
- **Disk under 12 GB:** prune your own frames (`n55-prune.sh` copied to n56-prune.sh); under 6 GB no new reels.
- **The Editor stalls:** batch-mode builds from `C:\Users\gaige\dev\SKYGRIND-unity-batch`.
- **A row breaks an old verb:** turn it off, log it, SHORT with the reason. Never ship a lost verb (PRINCIPLES 1).
- **The machine is slow** (NIGHT55: a 30 s `date`): stop your own polling loops; poll at >= 60 s; one heavy job at a time.

## 12. Closing reminder (the rules nights like this have dropped)
- Levels first: the first gate after P0 is L2 (spawns in view); the first thing GG sees in a room is the fight, not an empty hall.
- A slice counts when a fresh critic passes it on Player frames, not when it compiles. Bot numbers are labelled "bot".
- Caps are caps: at the hard cap, close with what is gated; never build past it.

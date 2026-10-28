# NIGHT63.md: THE BOSS NIGHT (8 hours, 7 minimum): bosses that move like they mean it, one colour language for every effect, NIGHT62's debt

GG, 2026-10-22 ~22:11, verbatim: "plan out next long detail night prompt 8 hours 7 minimum, follow detailed methods we used and pull form online
to get best cresuolts, be honets and critcal on where we need to improve, i would lead towards bosses, effect colores everything doenst feel
alligned, boss animations are too rigid to start, this is one of many many things the night prompt hsould focus on, create it now, 30 mins to
build". The ask behind every night (FORGE61): "need to see real results and it needs to be honest and critical where things are not good."

Written by the forge FORGE63 (30-minute box, during NIGHT62's close). Evidence and calls: `Research/2026-10-22-forge63/` (DECISIONS, two web
research files, redteam.md, GOAL.txt, slate-seed.json). Order of authority: the org rules, GG's words above, CLAUDE.md, this file,
HANDOFF.md, the logs. Read this whole file first and again after any compaction or resume. The /goal is
`Research/2026-10-22-forge63/GOAL.txt`. **The night ends when section 9's close is done and the last message is `NIGHT63 COMPLETE`; the
close starts no earlier than 7:00 and ends by 8:00.**

## Where we are, honestly (read before planning)
- **NIGHT62 shipped 1 of 5.** Of NIGHT61's five built features only the guns without yellow passed (critic PASS, 0 yellow on the viewmodel
  in four poses). Exit frame: its SPOT pair came from two reel runs that drift (an enemy 15.3 m vs 15.2 m away), so a hint line differed and
  the blind spotter named it (L-129). Text one-line-at-a-time and lights on fixtures: the spotter saw NOTHING OBVIOUS: the changes were too
  small to notice. Boss rule line: it works but rises at 2.45 s, after the name card, and its own reel checks 1.0 s (L-131). **Lesson: build
  changes a player notices, and judge them on clean pairs.**
- **The game's worst things now** (gg-proxy on NIGHT62's frames, Logs/n62/gg-proxy.md): empty boxy rooms (asked 7 times), choppy movement and
  a clunky wall ride (7), the fight hidden (12-23 LEFT, 1-5 enemies on screen), text stacked on the room name, bosses never saying what they
  want, dark flat views, the style meter stuck at DULL, enemies reading as red glow balls, kills that don't satisfy, colours without direction.
- **The bosses** (GG tonight: "boss animations are too rigid"): the boss views move their parts as rigid copies of the sim (Wick37View,
  Crown37View, Brawl40View, Ice41View, WyrmView; a grep for spring / damping / anticipation / flinch finds 0-9 uses per file, 0 in
  Boss37Kit.View.cs). No boss has a wind-up pose, a follow-through, a hit flinch or idle life.
- **Effect colours** (GG: "everything doenst feel alligned"; also 2026-10-19: "The effects are inconsistent it all needs to be unified"): the
  rules exist (CLAUDE.md rule 8: cyan player-usable, red-orange enemies and danger only, magenta RIVAL only, pale violet-white the boundary;
  yellow56's parry tells) but effects pick colours one at a time; gg-proxy names lime floor stripes, a danger-red line on spire's exit (since
  at least NIGHT60) and yellow edge lines.
- **Not tonight, said plainly:** the rooms (#1) and the movement feel (#2) are the two oldest complaints and tonight leads with bosses at
  GG's word; the rooms need a level night, the movement gets one measurement (M1). GG has not played since 2026-10-20.

## 0. Rules at a glance (all of CLAUDE.md's hard rules apply; the guard hook enforces 1-4)
1. **The clock** (`node Tools/night/n63-clock.cjs`, every turn; the guard's `clock` rule): BUILD 0:00-7:00; FREEZE 7:00-8:00 = the close (only
   builds, reel sets and the smoke tagged `n63-freeze` start; no test run; no new agent); CAP 8:00. **The close never starts before 7:00**: when
   the slate runs dry, take the next leftover (section 3). Never restart t0; never edit `Logs/clock.json` (only `stop` at the end).
2. **Disk:** the guard refuses a build or reel set under 12 GB; `n63-reels.sh` prunes beside every set; builders delete each `dn-check-wt.sh`
   scratch (about 681 MB) after the check.
3. **Docs and code only with Write / Edit** (rule 9): no heredocs, `node -e` / `python -c` writes, `sed -i`, redirects into docs or code.
4. **Git and the machine:** explicit paths plus `.meta`; never GG's four files or the four TMP fonts; no push, merge, rebase, master, `reset
   --hard`, `stash`, `commit -a` / `--amend`, `branch -D`, `unity test`, killing Unity.exe, a synchronous test run that reaches
   MapChoiceTests, a background `&` ending an `&&` chain (`node .claude/hooks/guard.cjs --selftest`: 110 / 110).
5. **Memory** (the PC ran out of virtual memory on 2026-10-22, L-128): one Player at a time; at most five agents alive: three builders plus
   two slots shared by the GATEs, the spotter and gg-proxy; close every Player you start by its PID.
6. **Background, not foreground:** every reel set, gate run, build, smoke, test batch, GATE and gg-proxy runs in the background; wait on the
   completion notice, never a foreground `sleep` (L-122).
7. **Evidence:** nothing turns ON without a Player frame, a SPOT line and a critic PASS. **SPOT first** (L-130): a row's SPOT pair (or its own
   proof reel) runs before the rest of its evidence; NOTHING OBVIOUS or another difference ends the row for this build, no critic.
8. **Visible by design** (FORGE63's experiment, D-6, re-based by the red team): each of tonight's new visible slices (B1, B2, B3, C1) names its
   SPOT pose, and on that pose's pair at least 41,472 px change (2 % of 1920 x 1080; `python -I Tools/night/n62-gun-yellow.py --pair <on>
   <off> --rect 0,0,1,1` prints the count); under it, the slice goes back to its builder once for a bigger change before its SPOT. NIGHT62's
   debt rows (P1, P2) are exempt: their doneWhen predate the rule (NIGHT62's only PASS changed 20,439 px).
9. **The usage limit** (L-003, L-098): commit every step; resume agents by SendMessage; the clock does not pause.

## 1. Bearings (steps 1-3 every session; 4-7 the first session only)
1. First session: `node Tools/night/n63-clock.cjs` first: if it prints `clock n62` (NIGHT62's clock, not stopped), run `node
   Tools/night/n62-clock.cjs stop` and log the line; then `node Tools/night/n63-clock.cjs start`. Every session: `node
   Tools/night/n63-clock.cjs`, `git status --short`, `git log --oneline -10`, `git tag -l u3-n62` (if it prints nothing, log it under Blocked
   and go on; never tag NIGHT62's work). Write t0 at the top of `NIGHT63-LOG.md` (tracked). Read the log, the slate (`node
   Tools/night/n63-slate.cjs`), `DECIDE63.md` (untracked; seeded by the forge; one D-id per call you make for GG).
2. **The Editor:** `timeout 60 bash Tools/unity-loop.sh status` ("playMode": "stopped"), then `timeout 45 unity command set_autotick --enable
   true --interval_ms 16 --caller plugin --skill unity-cli`. **Memory:** `powershell -NoProfile -Command '$o=Get-CimInstance
   Win32_OperatingSystem; "{0:N1} GB commit free" -f ($o.FreeVirtualMemory/1MB)'`.
3. **The branch:** **A** (12 GB free, the Editor answers, 20 GB commit free): the plan. **A-lean** (under 20 GB commit): two builders, one
   GATE slot. **C** (under 12 GB, or the Editor deaf twice 10 minutes apart): no Player; builders build OFF and gated, the report opens with it.
4. `mkdir -p Logs/n63 && cp Research/2026-10-22-forge63/slate-seed.json Logs/n63/slate.json`; `bash Tools/night/n63-gates.sh` (background;
   GREEN is the baseline), then `bash Tools/night/n63-gates.sh --flow-base` (background, never beside another gate run).
5. **A1 (the lead, ~20 min):** the `design.n63` block with a `_why` in both config copies (byte-identical) and the rows the slate names
   (bossMotion63, bossTell63, bossColours63, colours63), all OFF; `N63SharedTests` modelled on `N61SharedTests`; one D-id in DECIDE63 for
   yellow's role (the forge's default: "parry = a yellow role in the table, used only by the parry tell"); `bash Tools/night/n63-gates.sh`
   (its fingerprints include --off63) GREEN; commit. Builders start from this commit (L-049, L-065).
6. **gg-proxy's start check** (background): it ranks NIGHT62's freeze frames (`Logs/feel/n62-freeze/v*/verbs37`) into `Logs/n63/gg-proxy-start.md`.
7. Spawn builders D, V and G with `Research/2026-10-22-forge63/builder-brief.md` and their first slices (section 3). Log each under "Agents".

## 2. The slate and the visible rule
`Logs/n63/slate.json` (seeded; fields as NIGHT62's: id, title, why, owner, row, red, doneWhen, visible, spotPose, passes, state, verdict, spot).
Only the lead edits it and commits it with `git add -f`. A doneWhen is written before any measurement and never loosened.
**The visible rule.** Two frames from the same build and pose differing only by the slice's one-row switch; **once D1 lands, from one run
(the same-run toggle) for every row D1 covers, never from two runs** (L-129); a row D1 does not cover is judged on a two-run pair and the
report says so. In a build that carries several rows, each SPOT pair has only its own row toggled (`n46-build-rows.cjs` on / off for the
rest). `node Tools/night/n63-spot-pair.cjs <id> <on> <off>`, a fresh `skygrind-spotter` (in a free slot) with the two paths and nothing else,
its line in `Logs/n63/spot/<id>.txt`, the mapping logged after the answer. One SPOT per slice per build; it must name the slice's `visible`
change. Bug, test and tooling slices are exempt.

## 3. Order of work and owners
- **D (tooling, first): D1 SAME-RUN PAIRS** (the red team's scope): `NightRows.ViewOn` consults a live reel override
  (`NightRows.ReelOverride`) that the reel sets, and the rows read every frame re-read it every frame (exitFrame61, bossMotion63, bossTell63,
  bossColours63, colours63); a UI row decided at an event (bossRule61) is judged OFF as "the same frame with the row's UI root hidden". At each
  shot the reel holds the sim and every unscaled-time clock, captures with the row on, again with it off, then restores; the off captures go to
  `<side>-off/` with the same file names, so n60-v-lamp.py, `n62-gun-yellow.py --pair` and n63-spot-pair.cjs work unchanged. doneWhen: a planted
  no-op row gives 0 changed px; a planted 10 x 10 px change exactly 100; one planted case per row it claims changes that row's frame; the reel
  prints a "toggle fired" line per shot; the sim lines and fingerprints identical with the option on and off. Then **D: C1 THE COLOUR TABLE**
  (below), then **D: B3 BOSS COLOURS** (it reads C1's table), then leftovers.
- **G: P2 THE BOSS RULE LINE ON TIME first** (NIGHT62 P2): the line rises with the name card, at or before 1.0 s (BossRule61.cs /
  UiBossRule61.cs; the row stays design.n61.bossRule61), test first; the lead's boss61 reels must read GREEN on all five bosses. Then **G: B2
  BOSS TELLS**, once V's `BossAttackClock63` and `boss63` reel are picked: measure each boss attack's visible cue-to-hit lead and cue hue on
  `boss63` first (the sim's own tells already run 0.55-1.5 s in config), then fix only **the attacks whose cue lead is under 300 ms or whose cue
  is not in the danger hue** (view only, design.n63.bossTell63, its own file attached by one hook line); a parryable attack's parry tell is
  untouched whether its row is on or off.
- **V: B1 BOSS MOTION first** (GG's "too rigid"). **Its first deliverable (about 40 minutes) is the proof:** `BossAttackClock63` (a read-only view
  of each boss's sim attack state: attack id, phase, steps to the hit) and a `boss63` reel scenario extending boss61's that holds 20-30 s after
  the rise, fires at the weak point and prints `[boss63] attack <id> windup <ms> hit <ms>`, `flinch` and `stagger` lines on the reel's fixed
  clock (G reuses both for B2). Then one shared view-only layer (`BossMotion63`) for the five campaign bosses, design.n63.bossMotion63,
  numbers in config: second-order (damped spring) follow on each part instead of a rigid copy; an anticipation pose before each attack (driven
  by the sim's attack state), follow-through and overshoot after it, a hit flinch on each damage event, a slump while staggered, idle breathing.
  The sim, the hitboxes and the fingerprints never change. Proof: a boss reel prints each part's offset from the rigid pose (rms per second) on
  vs off and the wind-up's start before each hit; MAIN: on > 0 on every boss, off = 0; wind-up visible >= 250 ms before the hit; GUARD: no part
  more than its config limit from its rigid pose, the hit frames unchanged; SPOT on a same-run pair at a flinch or the stagger slump (B2's
  SPOT takes a cue, so the two rows never share a pose). The research
  (`Research/2026-10-22-forge63/research-boss-animation.md`): a second-order spring per part (t3ssel8r: k1 = z/(pi f), k2 = 1/(2 pi f)^2,
  k3 = r z/(2 pi f); start at f 1.5-3 Hz, z 0.4-0.7, r 1-2, and r -0.5 to -1 for a wind-up dip), a volume-keeping squash on hit peaking at
  about 40 ms, the telegraph pose driven by the sim's own wind-up timer (15 frames, 250 ms, for a learned attack; 30, 500 ms, for a new one),
  idle breathing muted during a wind-up, every view offset capped so the drawn body never leaves its collider, and no second hit-freeze (the
  sim already has hitstop). The wyrm's segments that are hitboxes stay in the sim. Then V takes the leftovers.
- **The lead:** A1; **builds on demand**: a build starts whenever a pick's gates are GREEN and no Player runs (about 2 minutes each), with b1 no
  later than 2:00, b2 no later than 4:00, b3 no later than 6:00, and the close's freeze; P1 (NIGHT62's exit frame) re-SPOT on a same-run pair in
  the first build after D1 is picked, ON by the rule; **M1 THE CAMERA'S CHOP** (measurement only, the red team's scope): from verbs37 telemetry
  (fixed clock, no yaw), the camera's position, pitch and its heading from the x / z path, frame to frame against a smoothed path, a number per
  level labelled "fixed-clock camera path; real frame pacing not checked"; and from one `-skygrindBench 60` Player run (no FeelClock) the frame
  times, the pacing half; a cause or "none found", and a proposal in DECIDE63; every pick, gate run, reel set, SPOT and GATE.
- **C1 THE COLOUR TABLE (D, after D1):** an audit tool that lists every effect colour in `Assets/Scripts/Game` not read from config's role
  colours (hex and Color literals, with file:line); the top offenders routed through the role table behind design.n63.colours63, including
  gg-proxy's lime floor stripes, the danger-red line on spire's exit and the yellow edge lines; a frame meter (with planted cases) of
  off-palette pixel share per level before and after; SPOT on the pair with the biggest change. The research
  (`Research/2026-10-22-forge63/research-effect-colours.md`): one hue, one meaning, bosses included (ULTRAKILL's parry flash is the same on
  every enemy); a saturation ladder (the world muted, the UI controlled, effects the highest); no pure black or white in an effect's body; an
  effect peaks in its first quarter of life; the danger warning gets the strongest edge contrast; one table every effect reads; and no studio
  publishes per-role numbers, so the bands come from our own frames, measured first.
- **Leftovers, ranked (about twice the building time; take the next whenever a list above is waiting; the close never starts before 7:00):**
  (1) the w62 and w63 memory harvest (a builder card, L-079); (2) text61 (NIGHT62 P3) re-judged on a two-run pair with the hint pinned (a reel
  argument that fixes the hint line; its off state is a history the same-run toggle cannot recreate), else left for GG; (3) T-146's three items
  as code; (4) R1's small fixes (Logs/n62/R1.md N1, N3, N5, N11, N12); (5) fixture61 made visible (a bigger lit housing), only if GG answered
  "keep" in DECIDE62; (6) S7 kiln's presses as machines; (7) C2 the enemy glow up close. **Not tonight:** MapChoiceTests (P8): it can wedge the
  live Editor every build needs (T-147; a batch-mode run on a copy, a later night).

## 4. One slice, start to finish
1. The builder builds test-first behind its OFF row, compiles (`bash Tools/night/dn-check-wt.sh <scratch>`), deletes the scratch, runs `bash
   Tools/night/n63-gates.sh --no-editor`, commits each step, writes notes to `Research/2026-10-22-wave63/<letter>.md` with the exact Player
   steps (the build-rows line, the reel sides, the SPOT pose, the meter with its planted case, the proof line) and reports its own shas.
2. The lead picks by `git cherry-pick <first>^..<last>` with the builder's own shas (L-117), never during an Editor compile; the tests RED on
   the test-first commit's run and GREEN; `bash Tools/night/n63-gates.sh`; the full diff read.
3. In the next build: `node Tools/night/n46-build-rows.cjs <tag> on <rows>`; **the SPOT pair first**, then the rest of the reels and meters;
   then one fresh `skygrind-critic` per row (a one-row brief naming the slate `Logs/n63/slate.json`, the id, "your first line exactly
   `<id>: PASS` / `FAIL <reason>` / `UNSURE <reason>`", the verdict file `Logs/n63/verdict-<id>.md`, every frame and meter output, verbs37 x5 on
   that build, the shas; about 3 minutes, L-132). PASS: the slate, `node Tools/night/n63-freeze-rows.cjs "<night.row>=<reason>"`, commit; the
   flow gate once per build, after its flips. FAIL: OFF, one fix round if the critic names a cause and the clock allows, else SHORT.

## 5. Delegation
Three builders (D, V, G) and two slots shared by the GATEs, the spotter and gg-proxy (at the start and at 6:30): at most five agents.
Builders on UI or HUD work load `unity:ui` first; only the theme fonts. Resume a stopped agent by SendMessage, never a fresh copy.

## 6. The clock (t0 = the clock's start)
| from | to | what |
|---|---|---|
| 0:00 | 0:40 | bearings, the branch, the baseline gates and flow base, A1, gg-proxy's start check, builders spawned |
| 0:40 | 2:00 | builders on D1, P2, B1's clock and reel; the lead on M1; a build as soon as a pick is ready (b1 no later than 2:00) |
| 2:00 | 4:00 | builds on demand: P1's same-run re-SPOT, the boss rule line on five bosses, B1's layer; builders on C1, B2, B1 (b2 by 4:00) |
| 4:00 | 6:00 | builds on demand: SPOTs first, GATEs, flips; builders on B3, B2, leftovers (b3 by 6:00) |
| 6:00 | 6:30 | the last picks and their build; the freeze of picks at 6:30 (a fix to a picked slice only); builders write notes and stop |
| 6:30 | 7:00 | the last GATEs; gg-proxy ranks the newest build's frames; the report, HANDOFF and the harvest drafted; leftovers until 7:00 |
| 7:00 | 8:00 | **the close** (section 9), never before 7:00 |

## 7. Usage limit and resume
State on disk: `Logs/n63/slate.json`, `NIGHT63-LOG.md`, each builder's branch and notes. After a stop: bearings 1-3, the slate, resume each
agent by SendMessage, log the gap. A slice in progress at 6:30 is SHORT (or BLOCKED with its cause) with what was measured.

## 8. Honesty
Every claim names its command or frame; anything not checked says "not checked"; a failed slice is reported failed with its number; tests are
never edited to pass; critics report every problem and the lead filters with a logged reason. Mistakes become LESSONS.md rows from L-133.

## 9. The close (7:00-8:00) and the report
- **The close (boxes add up to about 45 minutes).** (a) Wait for a GATE due within 5 minutes, never past 7:10; no flip after (a). (b) `bash
  Tools/night/n63-gates.sh --flow` (background, about 7 minutes), shown. (c) `bash Tools/night/n41-build.sh n63-freeze` from HEAD and `git
  diff --stat ProjectSettings`. (d) `bash Tools/night/n63-reels.sh n63-freeze vf,n52-foundry,verbs37 vi,n52-iceline,verbs37 vk,n57-kiln,verbs37
  vs,n52-span,verbs37 vp,n52-spire,verbs37` (background). (e) With no agent alive, `bash Tools/night/n56-smoke-steal.sh n63-freeze`
  (background; "DONE: all steps passed"). (f) `node Tools/night/n63-slate.cjs`, nothing in progress. (g) Finish `NIGHT63-REPORT.md`,
  HANDOFF.md, LESSONS rows and the harvest (ledger/wave-62.md and wave-63.md, THREADS <= 60 lines, FEATURES, CORE). (h) Each ON row's
  `enabled` printed in both config copies and matched to the report (L-107); commit with explicit paths; `git tag u3-n63`; `git tag -l u3-n63`;
  `git status --short NIGHT63-REPORT.md NIGHT63-LOG.md HANDOFF.md docs/memory/CORE.md` printing nothing; `node Tools/night/n63-clock.cjs
  stop`; the final message `NIGHT63 COMPLETE`, with no background job or agent running. At 8:00 whatever is left is logged under Blocked, then (h).
- **NIGHT63-REPORT.md, in this order:** (1) What is still not good (gg-proxy's 6:30 ranking on the newest build's frames and the critics, each
  with its frame; the freeze frames come after 7:00, when no agent may start, so they are not ranked); (2) what you can see tonight: each row ON, its frame, its switch, a three-line PLAY note (the bosses first); (3) bugs fixed, tests
  and tools proven; (4) what failed, with numbers; (5) not checked; (6) your calls (DECIDE63.md); (7) the next prompt's seed (estimate vs
  actual per slate entry, leftovers ranked, aged threads, three research questions, the commands that worked, the forge experiment's numbers:
  the SPOT pass rate and each visible slice's changed-pixel share); (8) the close lines.

**Closing reminder (the rules nights like this one dropped):** never start the close before 7:00, take a leftover instead; judge every
visible slice on a same-run pair and run its SPOT first; a change too small to see goes back to its builder before its GATE; every long job
and agent in the background; cherry-pick from the builder's own shas and copy row names from the slate.

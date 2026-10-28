# NIGHT64.md: THE ALIGNED NIGHT (3 hours): one colour means one thing, one order at a time, the wyrm's weak point

GG, 2026-10-23 ~08:30, verbatim: "is prompt fully finsihed or do we need to handoff and continue, create handoff and use that, resrecah,
follow best methods and create next prompt that will go for 3 hours". The ask it continues (2026-10-22 ~22:11): "i would lead towards
bosses, effect colores everything doenst feel alligned, boss animations are too rigid to start". The ask behind every night (FORGE61):
"need to see real results and it needs to be honest and critical where things are not good."

Written by the forge FORGE64 (Research/2026-10-23-forge64/: DECISIONS, two web scouts, a code scout, redteam.md (17 findings, all applied),
GOAL.txt, slate-seed.json, builder-brief.md). Order of authority: the org rules, GG's words above, CLAUDE.md, this file, HANDOFF.md, the logs.
Read this whole file first and again after any compaction or resume. The /goal is `Research/2026-10-23-forge64/GOAL.txt`. **The night ends
when section 9's close is done and the last message is `NIGHT64 COMPLETE`; the close starts between 2:00 and 2:15 and ends by 3:00.**

## Where we are, honestly (read before planning)
- **NIGHT63 shipped six rows** (bossMotion63, bossTell63, bossRule61, text61, text63, nearFade63; tag u3-n63 on 0ec78408; Builds/n63-freeze
  verified 07:55-08:17: verbs37 x5 27 / 29 / 31 / 31 / 31, the smoke "DONE: all steps passed", flow "gates: all GREEN 08:17:31"). **None of
  its colour rows shipped:** bossColours63 (the wyrm's weak point lime, hue 158.5) and colours63 (2 of 5 levels) are SHORT.
- **What GG would see** (gg-proxy on the shipped frames, Logs/n63/gg-proxy.md, 36 frames seen): (1) the rooms still empty boxes, 12 of 16
  level frames; (2) choppy movement (no frame shows it; eye63 waits for GG's A / B); (3) 11 of 18 boss shots show no readable boss, and 4
  print the same order twice at once; (4) a red-orange line down every spire exit (5 of 5 spire frames, named since NIGHT60), the bosses in
  the player's cyan, the wyrm lime; (5) floating lamps; (6) the fight hidden; (7) kills; (8) the style meter at D, the score 500.
  **Nothing on the level frames got better last night.**
- **Tonight's target, said plainly:** C4 (the red line down the spire exits), U1 (the order printed twice) and L1 (GG's eye63 kit); B3 (the
  wyrm's weak point) if its tuning loop lands in time. All view only, each pinned to file:line in Research/2026-10-23-forge64/code-scout.md.
  **Not tonight:** the rooms (#1: an 8-hour level night); the bosses' shapes (the Crown's 6 m ring, the Loom's cyan knee: enemy design, GG's,
  CLAUDE.md rule 7); the lesson under a "DOOR" title (the forge's U2, cut: the red team found "DOOR" is a room title text61 already waits for,
  redteam.md 5). DECIDE64 asks GG the order of the next long nights.
- **NIGHT63's lessons this prompt is built on:** the usage limit stopped everything 05:23-07:41 and the whole close window was lost (L-003):
  tonight keeps a verified release candidate at all times and starts the close by T-45; a fix after a PASS shipped unreviewed and lost a lesson
  (T63c): a PASS binds to a sha; another session's runaway job took commit free to 1.0 GB (L-146): read the processes before a heavy job.

## 0. Rules at a glance (all of CLAUDE.md's hard rules apply; the guard hook enforces 1-4)
1. **The clock** (`node Tools/night/n64-clock.cjs`, every turn; the guard's `clock` rule): BUILD 0:00-2:15; FREEZE 2:15-3:00 = the close (only
   builds, reel sets and the smoke tagged `n64-freeze`; no test run; no new agent); CAP 3:00. **The close starts between 2:00 and 2:15**; a dry
   slate before 2:00 takes a leftover (section 3). Never restart t0; never edit `Logs/clock.json` (only `stop` at the end).
2. **Disk:** the guard refuses a build or reel set under 12 GB; `n64-reels.sh` prunes beside every set; side names lowercase.
3. **Docs and code only with Write / Edit** (rule 9): no heredocs, `node -e` / `python -c` writes, `sed -i`, redirects into docs or code.
4. **Git and the machine:** explicit paths plus `.meta`; never GG's four files or the four TMP fonts; no push, merge, rebase, master, `reset
   --hard`, `stash`, `commit -a` / `--amend`, `branch -D`, `unity test`, killing Unity.exe, a background `&` ending an `&&` chain.
5. **Memory and agents:** one Player at a time; **at most three subagents alive: builders D and G and one review slot** (the spotter, a critic
   or gg-proxy, in turn), plus the lead. **Before every build, reel set, smoke or gate run** read commit free and the top processes
   (`powershell -NoProfile -Command 'Get-Process | Sort-Object WorkingSet64 -Descending | Select-Object -First 6
   Id,ProcessName,@{n="MB";e={[int]($_.WorkingSet64/1MB)}}'`): under 15 GB commit free, hold the job and log what is using it (L-146).
6. **Background, not foreground:** every reel set, gate run, build, smoke, test batch, GATE and gg-proxy runs in the background; wait on the
   completion notice, never a foreground `sleep`. Never two dotnet runs at once (a gate run and a flow base share Tools/SimHarness's obj tree).
7. **Evidence:** nothing turns ON without a Player frame, a SPOT line and a critic PASS. **SPOT first:** a row's same-run pair (D1's
   `-skygrindSameRun63 <night.row>`, residue 0-16 px, stated) runs before the rest of its evidence; NOTHING OBVIOUS or another difference ends
   the row for that build, no critic.
8. **Visible:** each slice names its SPOT pose and its SPOT must name the slice's `visible` change. (B3's pose already changes 187,052 px on
   NIGHT63's code, n63-b11's same-run b3k stagger pair, so the 2 % bar is met; C4's line and U1's text are thin by nature: the SPOT naming them is the bar.)
9. **A PASS binds to a sha** (FORGE64 DECISIONS D-6): at a PASS the lead sets the slice's `passSha` (the full sha the critic judged) and
   `passFiles` (the union of `git show --name-only --format= <sha>` over the slice's own picked shas, never a pick range);
   `n64-freeze-rows.cjs` refuses the flip if those files (config copies aside) or the row's own config block changed since. Flip every PASS of
   one build in one call. Any later change to a passed row goes through a GATE again or does not ship.
10. **Reviewers see evidence, not the story:** a critic's brief holds the slate entry, the frames, the meter outputs, the diff and the sha;
    no builder notes, no lead reading; at most 20 images per reviewer (contact sheets of four: `python -I Tools/night/n63-sheet.py`).
11. **The usage limit** (L-003): commit every step; resume agents by SendMessage; the report is drafted and committed by 1:30 and again by
    2:05; after any stop, the first turn past 2:00 runs the minimum close (section 7) before anything else.

## 1. Bearings (steps 1-3 every session; 4-8 the first session only)
1. First session: `node Tools/night/n64-clock.cjs start` (NIGHT63's clock is stopped). Every session: `node Tools/night/n64-clock.cjs`,
   `git status --short`, `git log --oneline -5`; **run HANDOFF.md's "Verify on arrival" table** and log each line; `claude --version` (one
   line). Write t0 at the top of `NIGHT64-LOG.md` (tracked). Read the log, the slate (`node Tools/night/n64-slate.cjs`), `DECIDE64.md`
   (untracked; one D-id per call you make for GG).
2. **The Editor:** `timeout 60 bash Tools/unity-loop.sh status` ("playMode": "stopped"), then `timeout 45 unity command set_autotick --enable
   true --interval_ms 16 --caller plugin --skill unity-cli`. **Memory:** `powershell -NoProfile -Command '$o=Get-CimInstance
   Win32_OperatingSystem; "{0:N1} GB commit free" -f ($o.FreeVirtualMemory/1MB)'` and rule 5's process list.
3. **The branch:** **A** (12 GB disk, the Editor answers, 20 GB commit free): the plan. **A-lean** (15-20 GB commit): one builder, D, takes C4
   then U1; B3 becomes a leftover. **C** (under 12 GB disk, or the Editor deaf twice 10 minutes apart): no Player; builders build OFF and
   gated, the report opens with it.
4. `mkdir -p Logs/n64 && cp Research/2026-10-23-forge64/slate-seed.json Logs/n64/slate.json`; write **Logs/n64/rc.json** with the Write tool:
   `{"rc": [{"name": "rc0", "build": "Builds/n63-freeze", "sha": "<git rev-parse u3-n63>", "verified": "2026-10-23 08:17:31: verbs37 x5
   27/29/31/31/31, the smoke DONE, flow GREEN"}]}`. Then `bash Tools/night/n64-gates.sh --flow-base` (background, ~5-10 min; NIGHT63's 08:17:31
   run is tonight's baseline for the plain gates).
5. Spawn builders **D** (C4, then B3) and **G** (U1) in the background, each in its own worktree with
   `Research/2026-10-23-forge64/builder-brief.md`, told t0 and to branch only once port/stage3's head is the A1 commit; they read while A1
   lands. Log each under "Agents".
6. **A1 (the lead, 20-25 min):** the `design.n64` block with a `_why` in both config copies (byte-identical) and the rows goal64, order64 and
   fightLine64 (leftover 2's), all OFF, by a row script modelled on Tools/night/n63-a1-rows.cjs; **add "goal64", "order64" and
   "fightLine64" to SameRun63Math's FrameRows** (Assets/Scripts/Game/SameRun63Math.N63D.cs:27, so the same-run toggle claims them and no two
   builders edit that line, redteam.md 11); `N64SharedTests` modelled on `N63SharedTests`; after the flow base, `bash Tools/night/n64-gates.sh`
   GREEN; `node Tools/night/n64-freeze-rows.cjs --selftest` (PLANTED GREEN); commit.
7. **The red set on rc0** (right after A1, background, one Player): `bash Tools/night/n64-reels.sh n64-red cr,n52-spire,a157,-skygrindClearShots60`
   (C4's red: a157 has never run on n52-spire; if it comes back RED or stalls, C4's pair falls back to a two-run verbs37 n52-spire pair at
   f0400 with `-skygrindNoGoal64`, and the report says so). Send the frame paths to D. Then **L1** (section 3).
8. **At 0:45:** rewrite HANDOFF.md's NIGHT64 section (the anchor sha, rc.json's last line, the slate line, the next step) so a stop leaves a
   handoff; refresh it at each RC (the I-PASS shape, research-run-methods.md rule 14).

## 2. The slate and the visible rule
`Logs/n64/slate.json` (fields as NIGHT63's, plus passSha and passFiles). Only the lead edits it (`node Tools/night/n64-slate-set.cjs <id>
<field>=<value>`; sha fields stay strings) and commits it with `git add -f`. A doneWhen is written before any measurement and never loosened.
**The pair:** two frames from the same build and pose differing only by the slice's row: same-run for C4, U1 and B3 (`-skygrindSameRun63
n64.goal64`, `n64.order64`, `n63.bossColours63`). In a build that carries several rows, each pair toggles only its own. `node
Tools/night/n64-spot-pair.cjs <id> <on> <off> <tag>`, a fresh `skygrind-spotter` in the review slot with the two paths and nothing else, its
line in `Logs/n64/spot/<id>.txt`, the mapping logged after the answer. One SPOT per slice per build. L1 is exempt (a kit for GG's own judgement).

## 3. Order of work and owners
- **D: C4 THE EXIT THAT READS DANGER first** (about 20 lines; report by 0:55): THE CROWN's goal column (Route47View.cs:113-135; its
  through-wall pass Column47.shader:98-106) draws in the colour table's goal role behind design.n64.goal64, read every frame; split the one
  colour Route47View.cs:58 gives the column and the window shafts; read the role with `Colours63Math.From(cfg)`, never `Colours63.Route`
  (it returns its input while colours63 is off); the shafts keep their red-gold (GG's call). **Then D: B3's first candidate** (report by
  1:05, so it rides b1): the weak point's own rim colour aimed bluer for the wyrm's stagger (code-scout option 1), the punch shell routed
  (Boss30View.cs:482), `n63-b3-roles.py --hue <rect>` with its planted cases, rule (c) fixed; grade54's own numbers untouched. B3 is a tuning
  loop: b1 measures the head's hue; if it is short of 175-200, D's one fix round aims by the measured gap and rides b2.
- **G: U1 ONE ORDER AT A TIME** (report by 1:05): while hud47's fight line shows an order the bar's hint stays hidden, and shows it after the
  line fades (UiBossBar43.cs:209-215, UiHud47.Boss.cs:30 / :146-170, UiStyle33.cs:77-78), written after UiBossRule61's every-frame write
  (UiBossRule61.cs:120-150), behind design.n64.order64, with a counter that prints "would-duplicate" and "duplicate shown" frames per boss in
  one run. **Then G: leftover 2** (by 1:45), **then leftover 1** (by 2:05). Builders write notes and stop by 2:10.
- **The lead:** bearings, A1, the red set; **L1 the eye63 kit** (two builds n64-abx / n64-aby from A1's sha; `node
  Tools/night/n46-build-rows.cjs <tag> on n63.eye63` on the one a coin flip picks; the mapping written to Logs/n64/eye63-ab.mapping.txt before
  either reel runs; verbs37 n52-foundry on each; `node Tools/night/n63-m1-chop.cjs --pair n64-abx n64-aby`, its direction kept out of the
  report); picks; builds; SPOTs; GATEs; flips; **rc1**; the report.
- **Leftovers, ranked (take the next whenever a list above is waiting; never past 2:10):** (1) the w64 memory harvest draft (G, a card of its
  own, L-079); (2) the room name shown as the fight line mid-fight (gg-proxy: "THE STACK" in bf/wick-windup, "THE SPIRE" in
  bp/crown37-idle) behind design.n64.fightLine64; (3) gg-proxy on rc1's frames, at most 20 images (contact sheets), for the report's section 1;
  (4) a bench with VSYNC on (M1's proposal 1: `bash Tools/survival-bench.sh 60 <build> <log> play`, the game's own VSYNC kept), measurement
  only. **Not tonight:** the rooms; U2 re-scoped for a later night ("a room title that arrives over a standing card takes its place, and the
  card resumes": text63's queue, redteam.md 5); the boss63 reel's pose (it would move every boss row's evidence mid-night); MapChoiceTests.

## 4. One slice, start to finish
1. **Contract first:** the builder's RED commit message names its proof command and the output it expects with the row off and on; the lead
   checks them against the doneWhen before the GREEN lands. Then test-first behind the OFF row, `bash Tools/night/dn-check-wt.sh <scratch>`,
   the scratch deleted, `bash Tools/night/n64-gates.sh --no-editor`, a commit per step, notes in `Research/2026-10-23-wave64/<letter>.md`.
2. The lead picks by `git cherry-pick` with the builder's own shas, RED commits first (the Editor RED), then the rest (GREEN), never during an
   Editor compile; `bash Tools/night/n64-gates.sh`; the full diff read.
3. **The GATE build** (`node Tools/night/n46-build-rows.cjs <tag> on <rows>`): **the SPOT pairs first**, then each slice's proof and **verbs37
   x1 on the level it touches** (spire for C4, kiln for U1 and B3). One fresh `skygrind-critic` per slice in the review slot (rule 10's
   brief: the slate path and id, "your first line exactly `<id>: PASS` / `FAIL <reason>` / `UNSURE <reason>`", the verdict file
   `Logs/n64/verdict-<id>.md`, the frames, the meter outputs, the verbs37 x1 line and rc0's x5, the diff, the sha judged; about 3-5 minutes).
   PASS: passSha and passFiles, the slate, then every PASS of the build in one `node Tools/night/n64-freeze-rows.cjs "<night.row>=<reason>" ...`,
   commit. FAIL: OFF, one fix round if the critic names a cause and it can be picked by 1:50, else SHORT; a fix round's result goes to a
   round-2 critic, never to the builder's own meter alone (L-145).
4. **rc1** = a fresh build from HEAD after b1's flips (no `n46-build-rows` edit: its copy is the repo's config): verbs37 x5 (the smoke too,
   only at a moment no agent is alive; else it waits for the freeze); its line in rc.json. **If rc1's verbs37 x5 or smoke fails, the rows
   flipped since the last RC go back OFF in one commit, the failure is logged, and the earlier RC stays the play build.**

## 5. Delegation
Two builders (D, G) and one review slot (the spotter, the critics, gg-proxy in turn): at most three subagents. Builders on UI or HUD work
load `unity:ui` first; only the theme fonts. Resume a stopped agent by SendMessage, never a fresh copy.

## 6. The clock (t0 = the clock's start)
| from | to | what |
|---|---|---|
| 0:00 | 0:30 | bearings, HANDOFF's verify table, the flow base (background), builders spawned (reading), A1, the red set on rc0 |
| 0:30 | 1:05 | builders on C4 then B3's candidate (D) and U1 (G); the lead: L1 (two builds, two reels), the report's skeleton, HANDOFF at 0:45 |
| 1:05 | 1:45 | picks C4, U1, B3; b1: SPOTs first, proofs, verbs37 x1, GATEs; one flip call; **rc1** (verbs37 x5) right after; G on leftover 2 |
| 1:45 | 2:10 | fix rounds picked by 1:50 into b2: SPOT, GATE round 2, flip by 2:10; G's harvest draft; the report committed by 2:05 |
| 2:00 | 3:00 | **the close** (section 9), started by 2:15 at the latest |

## 7. Usage limit, resume and the minimum close
State on disk: `Logs/n64/slate.json`, `Logs/n64/rc.json`, `NIGHT64-LOG.md`, each builder's branch and notes. After a stop: bearings 1-3, the
slate, resume each agent by SendMessage, log the gap. **The minimum close** (10 minutes, no agent, no Player; it runs first after any stop
past 2:00, and its steps are the close's last steps anyway): (1) the clock line; (2) every slice still in progress set SHORT with what was
measured; `node Tools/night/n64-slate.cjs` with 0 in progress; (3) NIGHT64-REPORT.md committed with what is known, "not checked" for the rest,
and its section 8 (the close lines); (4) HANDOFF.md's NIGHT64 section with rc.json's last verified sha as the play build; (5) commit with
explicit paths, then `git tag u3-n64` on that commit; (6) `git status --short NIGHT64-REPORT.md NIGHT64-LOG.md HANDOFF.md docs/memory/CORE.md`
printing nothing; (7) `node Tools/night/n64-clock.cjs stop`.

## 8. Honesty
Every claim names its command or frame; anything not checked says "not checked"; a failed slice is reported failed with its number; tests are
never edited to pass; critics report every problem and the lead filters with a logged reason. Mistakes become LESSONS.md rows (LESSONS.md is
at its 150-line cap: merge or retire to make room, as the wave-memory skill's consolidate mode says).

## 9. The close (started between 2:00 and 2:15, done by 3:00) and the report
- **The close (in series, about 30 minutes of heavy steps; NIGHT63's after-close ran these in series GREEN).** (a) Wait for a GATE due within
  5 minutes, never past 2:20; no flip after (a). (b) `bash Tools/night/n41-build.sh n64-freeze` from HEAD and `git diff --stat ProjectSettings`.
  If nothing was picked after rc1, say so with `git diff --stat <rc1 sha> HEAD -- Assets Design PortExport Packages` (empty) and take rc1's
  verbs37 x5 and smoke as the freeze's. (c) `bash Tools/night/n64-reels.sh n64-freeze vf,n52-foundry,verbs37 vi,n52-iceline,verbs37
  vk,n57-kiln,verbs37 vs,n52-span,verbs37 vp,n52-spire,verbs37` (background). (d) With no agent alive, `bash Tools/night/n56-smoke-steal.sh
  n64-freeze` (background; "DONE: all steps passed"). (e) `bash Tools/night/n64-gates.sh --flow` (background, ~10-12 min). (f) `node
  Tools/night/n64-slate.cjs`, nothing in progress. (g) Finish `NIGHT64-REPORT.md`, HANDOFF.md, LESSONS rows and the harvest (ledger/
  wave-64.md, THREADS <= 60 lines, FEATURES, CORE <= 40). (h) Each ON row's `enabled` printed in both config copies and matched to the report;
  the minimum close's steps (5)-(7); the final message `NIGHT64 COMPLETE`, with no background job or agent running. At 3:00 whatever is left
  is logged under Blocked, then (h).
- **NIGHT64-REPORT.md, in this order:** (1) what is still not good (gg-proxy's ranking if leftover 3 ran, else this morning's, with what
  tonight changed in it, and the critics' points, each with its frame); (2) what you can see tonight: each row ON, its frame, its switch, a
  three-line PLAY note, **and the eye63 play card** (which builds, what to do, the one question; never which build has it); (3) bugs fixed,
  tests and tools proven; (4) what failed, with numbers; (5) not checked; (6) your calls (DECIDE64.md); (7) the next prompt's seed (plan vs
  actual per slate entry, leftovers ranked, aged threads, three research questions, the commands that worked, the forge experiment's numbers:
  rc.json's timeline and the close's length); (8) the close lines.

**Closing reminder:** keep a verified RC at all times; the close by 2:15; judge every visible slice on its pair and run its SPOT first; a PASS
binds to its sha; every long job and agent in the background; read the processes before a heavy job; copy row names from the slate.

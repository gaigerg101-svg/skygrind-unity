# NIGHT62.md: THE PROOF SPRINT (2 hours, hard cap): NIGHT61's five built features in front of a Player, ON what passes, the truth

GG, 2026-10-22 ~20:35, verbatim: "follow similar methods from building other prompts and create a 2 hour build prompt hard cap and follow best methods".
The ask it carries forward (FORGE61, verbatim): "need to see real results and it needs to be honest and critical where things are not good."

Written by the forge (FORGE62b, the same evening; FORGE62's 6-hour draft is kept untracked as `NIGHT62-6h.md`). Evidence and calls:
`Research/2026-10-22-forge62/` (RETRO, RESEARCH, DECISIONS D-1..D-24, redteam-2h.md, GOAL.txt, slate-seed-2h.json). Order of authority: the
org rules, GG's words above, CLAUDE.md, this file, HANDOFF.md, the logs. Read this whole file before anything, and again after any compaction
or resume. The /goal is `Research/2026-10-22-forge62/GOAL.txt`. **The run ends when section 8's close is done and the last message is
`NIGHT62 COMPLETE`, by 2:00 after t0.**

**Why tonight is different.** NIGHT61 built five visible features that GG never saw: no Player could be built (C: under 12 GB) and the
Editor never answered. Tonight both hold (the forge, 20:44-20:53: C: 37.8 GB free; gates all GREEN with the Editor compile; a test class ran
in the Editor in 7 s). So the 85 minutes before the close turn that code into frames and verdicts, and what passes ships in a build GG can
play tonight. No new slice is built (D-15): the one the 6-hour draft gave a builder (S6, a wider view at speed) already exists and works
(NIGHT60's verbs37 telemetry: 74 deg standing, a median of 82-83 deg at 20-26 m/s), and a slice started now could not be judged before 2:00.

## 0. Rules at a glance (all of CLAUDE.md's hard rules apply; the guard hook enforces 1-4)
1. **The clock is the cap, and the guard holds it** (D-14). `node Tools/night/n62-clock.cjs` prints the date, `h:mm / 2:00`, the phase and
   C:'s free GB; print it at the start of every turn. BUILD 0:00-1:30: anything. FREEZE 1:30-2:00: only the close's heavy steps (a build,
   reel set or smoke tagged `n62-freeze`); no new agent, no test run. CAP from 2:00: no build, reel, smoke, test run or agent starts. A clock
   refusal is logged under Blocked with its line, then go on. Never restart t0; never edit or delete `Logs/clock.json` (only `stop` at the end).
2. **Disk** (L-104, L-119): the guard refuses a build or reel set under 12 GB; `n62-reels.sh` prunes in a loop beside every set and skips a
   side under 10 GB. No disk watcher runs (a background job that never ends would keep the /goal from being checked); the clock line shows it.
3. **Docs and code only with Write / Edit** (rule 9, L-114): the guard blocks heredocs, `node -e` / `python -c` writes, `sed -i`, redirects
   into docs or code.
4. **Git and the machine** (rules 1, 2, 10; L-047): explicit paths plus `.meta`; never GG's four files or the four TMP fonts. The guard also
   blocks push, merge, rebase, master, `reset --hard`, `checkout -- .`, `clean -f`, `stash`, `commit -a` / `--amend`, `branch -D`, tag
   deletes, `unity test`, killing Unity.exe, a synchronous test run that reaches MapChoiceTests, and a background `&` that ends an `&&` chain
   (`node .claude/hooks/guard.cjs --selftest`: 110 / 110 as planted). If it blocks you, do it the allowed way; never script around it.
5. **Memory** (the PC ran out of virtual memory at 17:32 today and froze; no bluescreen): one Player at a time; at most four background
   agents (three GATEs and gg-proxy); the spotter runs in the foreground, one at a time; close every Player you start by its PID.
6. **Background, not foreground** (red team finding 2): every reel set, the flow base, the gates, the smoke, the Editor test batch, every GATE
   and gg-proxy start in the background (the Bash tool's `run_in_background`, the Agent tool's background mode); wait on their completion
   notices, never a foreground `sleep` (L-122). The Bash tool stops a foreground command at 2 minutes by default and 10 at most, and a killed
   set leaves its Player running. The focus probe starts only after a reel set's completion notice (it takes the foreground).
7. **Evidence** (L-014, L-107, L-108): nothing turns ON without a Player frame, a SPOT line and a critic PASS; no "done" without the command or
   frame behind it. `node Tools/night/n62-slate.cjs` refuses `passes: true` without a "<id>: PASS" verdict and, for a visible slice, a SPOT.
8. **The usage limit** (L-003, L-098): commit every step; the clock keeps running through a stop (section 7).

## 1. Bearings (steps 1-3 every session, including after a compaction or a resume; 4-5 the first session only)
1. First session only: `node Tools/night/n62-clock.cjs start` (t0). Every session: `node Tools/night/n62-clock.cjs`, `pwd`, `git status
   --short`, `git log --oneline -8`, `git tag -l u3-n61` (must print). Write t0 and the clock line at the top of `NIGHT62-LOG.md` (tracked; the
   progress file). Read the log, the slate (`node Tools/night/n62-slate.cjs`, after step 4), `DECIDE62.md` (untracked; the forge seeded it:
   add one D-id per call you make for GG, with the alternative rejected).
2. **The Editor:** `timeout 60 bash Tools/unity-loop.sh status` must print `"playMode": "stopped"`; then `timeout 45 unity command
   set_autotick --enable true --interval_ms 16 --caller plugin --skill unity-cli` (it keeps the Editor ticking unfocused; the forge got
   "already enabled"). **Memory:** `powershell -NoProfile -Command '$o=Get-CimInstance Win32_OperatingSystem; "{0:N1} GB commit free" -f
   ($o.FreeVirtualMemory/1MB)'` (the forge read 46-48 GB).
3. **The branch, logged:** **A** (12 GB free, the Editor answers, 20 GB commit free or more): the whole plan. **A-lean** (all but the memory):
   the same plan with two GATEs alive at most, not three. **C** (under 12 GB, or the Editor deaf at two checks 10 minutes apart): no Player;
   prove what needs none (R1's answers, T-146's read-only checks), print `NIGHT62 LIST EXHAUSTED`, go to the close; the report opens with it.
   Re-check at each phase boundary and move up when allowed.
4. `mkdir -p Logs/n62 && cp Research/2026-10-22-forge62/slate-seed-2h.json Logs/n62/slate.json`; `bash Tools/night/n62-gates.sh` in the
   background (1 min 27 s in the forge; GREEN is the baseline; if not, fix it for 15 minutes at most, then Blocked and go on); when it is
   done, `bash Tools/night/n62-gates.sh --flow-base` in the background (about 5 minutes; never beside another n62-gates.sh: two SimHarness runs
   lock each other's build output).
5. Build P0 (section 3 step 1) as soon as the baseline gates are GREEN. No builder is spawned tonight (D-15).

## 2. The slate and the visible rule
`Logs/n62/slate.json` (8 entries, FORGE62's verbatim; only the set and order changed): id, title, why, owner, row, `red`, `doneWhen` (hard
thresholds written before any measurement, never loosened after), `visible`, `passes`, `state` (todo / building / measuring / critic / pass /
short / blocked), `verdict` (the verdict file's path), `spot` (the SPOT file's path). Only the lead edits it (Edit tool), prints it with the
slate tool after each change, and commits it with `git add -f Logs/n62/slate.json`.
**The visible rule.** A player-facing slice turns ON only if GG would notice it in normal play. Take two frames from the same build and pose
that differ only by the slice's one-row switch (`-skygrindNo<Row>61`). Copy them to `Logs/n62/spot/<6 random letters>/A.png` and `B.png` in a
coin-flip order, spawn a fresh `skygrind-spotter` with those two paths and nothing else, write its one line to `Logs/n62/spot/<id>.txt`; log
the coin flip and the mapping after the answer. One SPOT per slice per build, except P4, whose doneWhen asks for two pairs. It must name the
slice's `visible` change: "NOTHING OBVIOUS" or another difference keeps the row OFF, whatever its numbers, and that row needs no critic and no
more reels (L-109; the SPOT comes first for P1, P2 and P4 so their other reels are spent only on a row that can still ship).

## 3. The plan, in order (t0 = the clock's start; a reel takes 63-121 s, NIGHT60, RESEARCH.md section 5 and redteam-2h.md)
1. **P0 build, ~0:08 (the lead):** `bash Tools/night/n41-build.sh n62-p0` (33-76 s), `git diff --stat ProjectSettings` (only GG's files),
   `node Tools/night/n46-build-rows.cjs n62-p0 on n61.gunYellow61 n61.exitFrame61 n61.text61 n61.bossRule61 n61.fixture61`. Then start the
   Editor test batch (step 3) in the background: the Editor is free while the Players run.
2. **P0 reels, ~0:10-1:05, one set at a time, each `bash Tools/night/n62-reels.sh n62-p0 <sides>` in the background, in this order:**
   - **verbs37 x5 first** (every GATE's "nothing lost" line needs it, skygrind-critic.md rule 3): `vf,n52-foundry,verbs37
     vi,n52-iceline,verbs37 vk,n57-kiln,verbs37 vs,n52-span,verbs37 vp,n52-spire,verbs37`: 27 / 29 / 31 / 31 / 31, RED rows none, 0 NREs.
     Its vs and vp sides are P5's span and spire on sides, and its logs are P3's outside-level-1 proof.
   - **P5** `c3on-k,n57-kiln,a157 c3off-k,n57-kiln,a157,-skygrindNoGunYellow61 c3off-s,n52-span,verbs37,-skygrindNoGunYellow61
     c3off-p,n52-spire,verbs37,-skygrindNoGunYellow61`. Meter: `python -I Tools/night/n62-gun-yellow.py --pair <c3on-k kb2c entry png>
     <c3off-k kb2c entry png>` (the shot `shot-N29-L57-n57-kiln-kb2c-entry.png`; planted: `... planted`, 9 / 9) gives the yellow among the
     recoloured pixels (on must be 0) and the yellow left in the gun's corner with its box (open the frame there: world or gun); the span and
     spire poses (verbs37 f0600, f0300, f0900) by `python -I Tools/night/n56-colour.py <side dir>`'s yellowGunPct, vs / vp against c3off-s /
     c3off-p. Proof: `grep "\[gunyellow61\]" Logs/player/n47-reel-n62-p0-*.log`. SPOT: the kb2c entry pair, c3on-k vs c3off-k.
   - **P1, SPOT first:** `s1on,n57-kiln,a157,-skygrindClearShots60 s1spot,n57-kiln,a157,-skygrindClearShots60,-skygrindNoExitFrame61`; SPOT on
     the kb3 clear pair (s1on vs s1spot: the one-row switch). Only if it names the door or the exit, the side
     `s1off,n57-kiln,a157,-skygrindClearShots60,-skygrindNoExit58` and the meter `python -I Tools/night/n60-v-lamp.py Logs/feel/n62-p0/s1on/a157 Logs/feel/n62-p0/s1off/a157`: P1's MAIN
     is its **OWN GAIN** line (door luma on / off >= 1.5x in >= 4 of 5 rooms; the red is 0 / 5), never the LAMP summary line above it (lamp60's
     old MAIN, which the red already passes 4 / 5); planted: `... planted` (8 cases, GREEN). Proof: `grep -c "\[exit61\] door lit: room"` on
     s1on (8) and s1off (0).
   - **P3** `t61off,n57-kiln,a157,-skygrindNoText61 s4on-f,n52-foundry,a357` (its on side is c3on-k; s4on-f holds level 1's a357 entry frames
     for its doneWhen line 3 and is P4's foundry on side). Count the centre text blocks on the five kiln entry frames (open them); `grep
     "\[text61\]"` on c3on-k and on the verbs37 logs. SPOT: the kb2c entry pair, c3on-k vs t61off.
   - **P2, SPOT first:** `b61k,n57-kiln,boss61 b61koff,n57-kiln,boss61,-skygrindNoBossRule61`; SPOT on `shot-N30-B61-wyrm-1000.png`, b61k vs
     b61koff. Only if it names a line of text or the rule: `b61f,n52-foundry,boss61 b61i,n52-iceline,boss61 b61s,n52-span,boss61
     b61p,n52-spire,boss61`; summary47 wants `GREEN boss61 <boss>` x5 and `OFF boss61 wyrm` with shows 0.
   - **The focus probe (P6), here, after P2's completion notice:** `bash Tools/night/n60-focus-probe.sh n62-p0 on`, `... off
     -skygrindNoFocus60`, `... planted -skygrindFocusProbe60NoPause`, one at a time (each takes the foreground once).
   - **P4, SPOT first:** `s4on-i,n52-iceline,a357 s4off-i,n52-iceline,a357,-skygrindNoFixture61` (iceline holds the floating panel
     gg-proxy named); SPOT on its entry pair. Only if it names the lamps or fixtures: `s4off-f,n52-foundry,a357,-skygrindNoFixture61`, then
     `-k` n57-kiln, `-s` n52-span, `-p` n52-spire as on / off pairs, then the flicker guard `s4on-lf,n52-foundry,a154
     s4off-lf,n52-foundry,a154,-skygrindNoFixture61` and the same with `-li` n52-iceline and `-lk` n57-kiln; the second SPOT pair from a level
     whose entry frame shows a key panel. MAIN: `grep "\[fixture61\] mounts on"` per level (10 / 10 / 11 / 8 / 10, unmounted 0) and the a357
     entry frames counted by eye against the red (49).
   Every side name is lower case with hyphens (n62-prune.sh's pattern, L-113). After each set: its meter with its planted case, its proof
   lines, one on frame and one off frame opened by you (Read on the PNG), its SPOT, then its GATE (section 4). Each builder's notes hold the
   detail: Research/2026-10-22-wave61/V-C3.md (P5), V.md (P1), G.md (P2, P3), V-S4.md (P4).
3. **The Editor test batch (P6 / P7), in the background from ~0:10:** one statement, one class per call, in sequence, into
   `Logs/n62/editor-tests.txt`: `N61` (the eleven N61 classes), `FocusAutomation61Tests`, then D1's eleven: Ui43Tests, Hud30Tests,
   FirstFifteenVerbsTests, Arcline22Tests, Health42Tests, Style42Tests, Stamp22Tests, RunSnapshotTests, FingerprintTests, MenusTests,
   MenuMotion18Tests (each `bash Tools/unity-loop.sh tests <name>`; N61SharedTests took 7 s in the forge). A P7 class that fails: the batch
   behind it is reverted with `git revert` and its entry goes back on the list. P6 and P7 have no critic: you write `Logs/n62/verdict-P6.md`
   and `verdict-P7.md`, first line `P6: PASS` only when every doneWhen line has its printed line (P6's last one is the close's smoke).
4. **gg-proxy once, in the background, about 0:20-0:35:** it ranks the newest frames (the P0 build's verbs37 and c3on-k a157, all five rows
   on) into `Logs/n62/gg-proxy.md`, the report's section 1; say in the report which frames it ranked.
5. **Leftovers, ranked** (whenever the list above is waiting on an agent or a reel): the report's skeleton (by about 0:45); R1's one-line
   answers to the review's notes N1-N12 (`Logs/n61/review-n61.md`) into `Logs/n62/R1.md`, no code; T-146's three read-only checks. P8
   (MapChoiceTests alone) waits for a later night: it holds the Editor up to 40 minutes.
**When behind, cut from the end:** P4's flicker guard, then the rest of P4, then the focus probe; never the close. A cut row is SHORT with what
was measured. A row a critic FAILs stays OFF, SHORT with the lever the critic names (no fix round tonight: D-24). No sim row turns ON tonight.

## 4. One GATE, start to finish
1. **A GATE** is one fresh `skygrind-critic` per entry, in the background, started once verbs37 x5, the row's own set and its SPOT have landed
   (L-109: 17-23 minutes). Its brief, one row only: the slate `Logs/n62/slate.json` and the id (P1-P5), "your first line exactly `P1: PASS` /
   `P1: FAIL <reason>` / `P1: UNSURE <reason>`" (with the row's id), the verdict file `Logs/n62/verdict-<id>.md`, the frames, the meter
   outputs and their planted cases, the SPOT file, the proof lines, verbs37 x5 on n62-p0 (`Logs/feel/n62-p0/summary47.txt`), and the shas
   (NIGHT61's commits for the row). At most three GATEs alive (two on branch A-lean).
2. PASS: `passes: true`, `state: pass`, `verdict` and `spot` set, `node Tools/night/n62-freeze-rows.cjs "n61.<row>=<reason>"` (copy the row
   name from the slate, L-121), commit. The flow gate runs once, in the close (all five are view rows). FAIL / UNSURE: the row stays OFF;
   `state: short` with the number. A critic or spotter silent for 30 minutes is asked by SendMessage first (L-120), then replaced once.
3. Log every agent (name and id) under "Agents" in `NIGHT62-LOG.md` when it starts, and each slate entry's minutes (estimate vs actual) in its
   progress row: the next prompt sizes from them.

## 5. Delegation
No builder (D-15). Three GATE slots (two on A-lean) and gg-proxy in the background, the spotter in the foreground: never more than four
background agents (the project allows 6; the machine ran out of memory today). Every subagent writes its result to a file and returns a short
pointer. Resume a stopped agent with SendMessage (L-098), never a fresh copy. No new agent after 1:30 (the guard refuses it).

## 6. The clock (print `node Tools/night/n62-clock.cjs` at the start of every turn)
| from | to | what |
|---|---|---|
| 0:00 | 0:10 | bearings, the branch, the baseline gates, the flow base (background), the P0 build, the Editor test batch started |
| 0:10 | 1:05 | the reel sets in order (verbs37, P5, P1, P3, P2, the focus probe, P4); SPOTs and GATEs as each row lands; gg-proxy at 0:20-0:35 |
| 0:45 | 1:25 | GATEs land and their rows flip ON; the report's skeleton, then its numbers; R1 and T-146 while you wait |
| 1:25 | 2:00 | **the close** (section 8); from 1:30 the guard lets only n62-freeze builds, reels and the smoke start |
When every slate entry and leftover is DONE, SHORT or BLOCKED before 1:25, print `NIGHT62 LIST EXHAUSTED` with each id and its state and start
the close; never wait for the clock.

## 7. Usage limit and resume
Every step is committed; the state lives in `Logs/n62/slate.json` (force-added) and `NIGHT62-LOG.md`. After a stop or a compaction: bearings
steps 1-3, the slate, resume each agent by SendMessage, log the gap with its times. The clock does not pause: after 1:25 go straight to the
close; after 2:00 commit what is done, log the rest under Blocked, then section 8 step (g). A row in progress at the close becomes SHORT (or
BLOCKED, with the precondition that stopped it) with what was measured.

## 8. Honesty, the close and the report
- Every claim names its command or frame; anything not checked says "not checked"; a failed slice is reported as failed with its number;
  tests are never edited to pass; critics report every problem and the lead filters with a logged reason. Every mistake becomes a LESSONS.md
  row from L-129 (`| L-NNN | rule | what happened, file, commit | scope | new | w62 |`).
- **The close (by 1:25, or at LIST EXHAUSTED; about 31 minutes).** A step that fails or runs past its box is logged under Blocked with its
  reason and the next step starts. (a) Wait for a GATE due within 5 minutes, never past 1:32, and flip its row if it passes; no row is flipped
  after (a) (a later PASS stays OFF and goes to DECIDE62.md as "PASS after the freeze"). (b) `bash Tools/night/n62-gates.sh --flow` in the
  background (the flow against the base, then every gate; about 7 minutes), shown; finish `NIGHT62-REPORT.md`, HANDOFF.md and the LESSONS
  rows while it runs (the memory harvest is the next forge's, D-24). (c) `bash Tools/night/n41-build.sh n62-freeze` from HEAD and `git diff
  --stat ProjectSettings` (2). (d) Only if (c) ended by 1:45: `bash Tools/night/n62-reels.sh n62-freeze vf,n52-foundry,verbs37
  vi,n52-iceline,verbs37 vk,n57-kiln,verbs37 vs,n52-span,verbs37 vp,n52-spire,verbs37` in the background (7); else Blocked (clock). (e) With no
  agent alive (wait until 1:50 at most, then run it and say one was alive), `bash Tools/night/n56-smoke-steal.sh n62-freeze` in the background
  ("DONE: all steps passed"; its planted steals take the foreground for 3 s twice) (5); then P6's verdict. (f) `node Tools/night/n62-slate.cjs`,
  shown, nothing in progress (1). (g) Each ON row's `enabled` in both config copies printed and matched to the report (L-107), commit with
  explicit paths, `git tag u3-n62`, then `git tag -l u3-n62` (it must print u3-n62), `git status --short NIGHT62-REPORT.md NIGHT62-LOG.md
  HANDOFF.md docs/memory/CORE.md` printing nothing, `node Tools/night/n62-clock.cjs stop`, then the final message `NIGHT62 COMPLETE` (3), with
  no background job or agent still running. At 2:00, whatever is left is logged under Blocked, then (g).
- **NIGHT62-REPORT.md, in this order:** (1) **What is still not good**: the 10 worst things in the game now, from gg-proxy's ranking and the
  critics, each with its frame; (2) what you can see tonight: each row ON, its frame, its switch and a three-line PLAY note; (3) bugs fixed,
  tests proven and dead code checked, with evidence; (4) what failed, with its numbers; (5) not checked; (6) your calls (DECIDE62.md: the rows
  that stayed OFF, each row turned ON as a keep or drop row, the parked threads, the disk); (7) **the next prompt's seed**: each slate entry's
  estimate vs actual minutes and the ratio, the leftovers ranked, the threads aged two nights or more, three sharp research questions, the
  commands that worked exactly as run, and the forge experiment's numbers (the close's start and end against t0, every clock refusal in
  Logs/guard-blocks.log); (8) the close lines.

**Closing reminder (the rules short runs like this one dropped):** the clock is the cap and the guard holds it, so start the close by 1:25
whatever is left; verbs37 runs first and every long job runs in the background; nothing turns ON without a frame, a SPOT and a critic, and a
row whose SPOT says NOTHING OBVIOUS gets no more reels and no critic; one Player at a time; copy row names from the slate.

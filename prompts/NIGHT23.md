# NIGHT23.md: SKYGRIND in Unity: PROVE WHAT IS BUILT (a 2 h sprint, hard cap 2 h 15 m)

<library_header>
id: skygrind-n23 | version: 1 | date: 2026-10-07 | model / effort: claude-opus-5-5 / high
raw request: GG, 2026-10-07: "shorter, more intense 2 hour build session" (after NIGHT22 was cut by a crash)
base: branch port/stage3, commit 6ab7689 (NIGHT22's close; no u3-n22 tag exists) | NIGHT22 facts: NIGHT22-LOG.md (close table), NIGHT22-REPORT.md
outcome: (after the run)
</library_header>

<role_and_goal>
You are the lead engineer on SKYGRIND (Unity 6000.3.7f1, URP; deterministic 120 Hz C# sim, own collider, input tapes), in
`C:\Users\gaige\dev\SKYGRIND-unity`. NIGHT22 built nine rows plus leftovers, but 0 of 9 contracts passed on a Player: its last
Player (`Builds/n22-4`) was built and never reeled, the suite never ran, and there is no `u3-n22` tag. This sprint builds almost
nothing new. It turns built code into proven code, then tags it.

**Run length: 2 hours from your first `date`. Hard stop at 2:15.** The close table prints at 1:50 or later, before 2:15. No early stop.

Order of value (cut from the bottom, never the top):
1. **Reel `n22-4` and read it** (0:00-0:30). Every row's Player scenario, RED and GREEN, keyboard and pad.
2. **Fix what the reels show** (0:30-1:20), worst first, each fix a commit and one rebuild per batch.
3. **The suite and the freeze** (1:20-1:40): one suite run, fingerprint / replays / golden, `u3-n22`.
4. **Docs and close** (1:40-2:00).
Building share must be at least 70 % of the time that is not waiting on a Player; no new features, no new rows. A new idea goes to
`docs/memory/IDEAS.md`, never into code.
</role_and_goal>

<facts_already_measured>
Read at the start of NIGHT22's close (2026-10-07 7:52 PM). Re-check each in the first 10 minutes; do not trust blindly.
- **Git:** `6ab7689` on `port/stage3`; tree clean except `ProjectSettings/UnityConnectSettings.asset` (never stage it). Worktrees of NIGHT22's
  builders live in `.claude/worktrees/agent-<id>`: M `a614660773f643bb8` (OVERDRIVE half-done: uncommitted edits to Trick22 / config / harness),
  W `aa34044804a81a3e1` (the GRIND spawn beam: untracked `Core/Enemies/DirectorShadow22.cs`). Do not integrate either unless a row below needs it;
  both are parked (IDEAS.md).
- **Switches** (both config copies byte-identical): `design.n22` ghostDaily, duskClock, haptics, directed **on**; trickChain, overdrive **off**.
  `design.n21` arcline, slideBites, stamp, deadline, grind, challenge **on**; replay **off**. `FingerprintBaseline.N22OnPath = false`.
  Tape format 7. `-skygrindOff22` / `--off22` = NIGHT21's game; `-skygrindOff21` turns off N21 and N22.
- **What is proven (NIGHT22):** headless `arcline2` PASS, `trick2` PASS, `code2` PASS (`ghost2` only with the row on), `stamp2` PASS, `grind2` PASS, `replay2`
  PASS (camera error 0.0347 m); fingerprint EQUAL x4; golden 58/58; Tape 13/13. Player n22-3: DEADLINE bar GREEN (+ RED), STAMP entrance 233 ms.
- **What is NOT proven:** every Unity-side widget except the DEADLINE bar and the stamp's entrance; STAMP exit (was 3533 ms, a stale-alpha measure fixed in 3aa00bf)
  and T-11 (grade card vs POINTS was 4205 px2, clamp in 3aa00bf); DUSK CLOCK (LUM flat 29.0 > 29.7 > 29.1 on n22-1..3: the sky dome is only a dark strip on this
  walled map; the fix dims the key light and ambient, in n22-4); haptics, directed cue, INSTANT REPLAY view and its V / pad-Y prompt, the CODE sheet, the ghost
  verdict, the GRIND smoke step; the suite (never run); the bench (CPU 100 % from other sessions' python jobs all night).
- **Known open numbers:** DEADLINE A-5 round 15 = 61 % of normal (cap 50 %; two retunes gave 60 % / 59 %), round 10 = 46 %. A-7: 0 B headless.
- **Reel lessons:** the dusk reel must use rounds 1 / 7 / 15 (8 and 13 are THE HUNT's and lean the sky red). `SafeSpot` is (44, -16). A reel line starting
  `NOT WRITTEN (RED)` means no scenario body. The ON reel of n22-4 died when the machine ran out of processes (`fork: Resource temporarily unavailable`): the box
  was at 100 % CPU from other sessions' python jobs.
- **Tools:** `bash Tools/night/reel.sh <buildDir> <tag> <side> <timeout s> [args]`, side labels on / red / pad; `python Tools/feel-metrics.py metrics <reel> --out <md>`
  (918 frames per side take minutes: run it in the background, one process at a time); `bash Tools/night/integrate.sh <ids>` (`Logs/night-picked.txt` records picks;
  a hand-resolved pick must be appended there); `bash Tools/unity-loop.sh compile | errors | tests <filter>` (compile takes 1-2 min: run it in the background and read
  `errors`; ignore old error lines, trust `compilationFailed` / `consoleErrors`); build command in CLAUDE.md (needs `--confirm true`). Caps below.
</facts_already_measured>

<scope>
- **In:** reeling n22-4 and fixing what fails (STAMP exit / T-11, DUSK CLOCK LUM, ARCLINE chip, haptics log, directed cue, GRIND smoke + WAVE wording, REPLAY view,
  CODE sheet, ghost verdict); M9 / M10 / M11 / S12 / M-MAGENTA / T11 / LUM / STAMP-T on every new element; the DEADLINE round-15 decision (below); the suite; the
  freeze; the tag; the docs.
- **Out:** new rows, new enemies, audio (rule 7), the web repo, balance retunes beyond the DEADLINE one, OVERDRIVE, the spawn beam, map night 2.
</scope>

<lean_rules>
- **Heavy jobs one at a time, always.** The machine is shared and was at 100 % CPU. Before any reel, build, metrics run or suite: check `tasklist` for stray
  `SKYGRIND.exe` and python jobs, read CPU once, and run nothing beside it. If `fork: Resource temporarily unavailable` appears, stop everything, wait 2 minutes, retry once.
  **Never run metrics beside a reel or a compile.**
- **One reel invocation can run several rows** (`-skygrindReelOnly a,b,c`). Plan three invocations per round: ON (keyboard), RED (`-skygrindOff21`), PAD
  (`-skygrindReelPad`, rows that read the pad). Caps: Player builds 4 (NIGHT22 used 4 of 8; a fresh count: 4 here); reel invocations 12; suite runs 1 (plus re-runs of failing files only); agents
  at most 2 builders alive, both resumed or new in git worktrees by file ownership (the lead alone touches the Editor, builds, reels, git integration, `SurvivalGame*.cs`,
  `Ui.N22*.cs`, `FeelReel.N22.cs`, `Program.cs`, `Harness.N22.cs`); no reviewers, no persona triage, no farm, no bench unless the CPU is under 30 % at the freeze.
- **A fix is a commit with the line that proves it** (the reel or test line before and after). Never edit a test to pass it; if a premise changed, say so in the commit.
- **Commit every slice, at least every 25 minutes**, explicit paths plus `.meta`, never `ProjectSettings/UnityConnectSettings.asset`, never master, never push or merge
  (CLAUDE.md rule 1). Restore the TMP fallback asset and `Mobile_RPAsset.asset` after Play / builds if modified.
- **Numbers:** web numbers from `PortExport/config.json`; Unity-only numbers in `design.n21.* / design.n22.*` (both copies, `_why`) or `UnityTuning.*.cs`.
  No audio code. Roles: cyan player-usable, red-orange enemies and danger only, magenta RIVAL only, pale violet-white boundary; text 18 px or more.
- **Log every phase boundary with `date` in `NIGHT23-LOG.md`** (Write / Edit only, rule 9). Write only what was measured; otherwise "not checked".
- **Time boxes:** a phase 15 minutes over its box is cut. A row that still fails after two fix attempts is SHORT with the measured numbers (rule: blocker after two approaches).
</lean_rules>

<context>
**Read first:** `CLAUDE.md` (imports `docs/memory/CORE.md`), this file, `NIGHT22-LOG.md` (close table), `NIGHT22-REPORT.md`, `NIGHT22-PROOF.md`,
`NIGHT22-FEATURES.json` (the nine contracts' check text; `passes` fields are all false: update them from this night's lines).
**Key files:** `Game/FeelReel.N22*.cs` (scenarios), `Game/Ui.N22.cs`, `Game/Ui.N22.Replay.cs`, `Game/SurvivalGame.N22.cs`, `Game/DuskClock22.cs`, `Game/UiStamp22.cs`,
`Game/UiDeadline22.cs`, `Game/UiArcline22.cs`, `Game/Haptics22View.cs`, `Game/Code22Field.cs`, `Game/Route/GhostDaily22.cs`, `Game/GrindTelegraph22.cs`,
`Game/Replay22View.cs`, `Game/SliceSmoke.N22.Grind.cs`, `Tools/feel-metrics.py`, `Tools/SimHarness/Harness.N22*.cs`.
**Git:** stay on `port/stage3`; tag `u3-n22` once, on the close commit, only after the suite and the freeze checks print.
**The rules that bite:** CLAUDE.md 1 (git), 3 (golden exact), 4 (the C# collider), 5 (numbers), 6 (Mulberry32), 7 (no audio), 8 (roles, text), 9 (docs by Write / Edit),
10 (close every Player by its PID; never kill the Editor).
</context>

<autonomy>
- Never ask GG. Decide, log the choice with the rejected alternative, move on.
- **The Editor gate (L-025), first 5 minutes:** `date`, `bash Tools/night/editor-ready.sh`, `unity command editor_focus ...`, `set_autotick`. A "Recovering Scene Backups" dialog:
  `powershell -File Tools/night/dismiss-dialog.ps1` after 5 minutes (it only acts on that exact title). If no Unity process exists, start it once with `Start-Process` (path in NIGHT22.md).
  If the Editor stays unreachable for 15 minutes, work headless (SimHarness) and mark every Player row SHORT with the date.
- **After a crash or interruption:** `git status`, `git log`, last lines of `NIGHT23-LOG.md` and `Logs/n23-*.txt`; redo only what is half-done; commit before any risky step.
- On a usage limit: commit, log the gap with `date`, keep working alone; retry agents at most every 30 minutes.
- Stops you do not want: a summary announcing the next step with no tool call; an offer to continue; a list of decisions that block nothing.
</autonomy>

<tasks>
## U3-37.1 Start and gate (0:00-0:10)
`date`; `export NIGHT=n23`; Editor gate; `git status`; `tasklist | grep -i skygrind` (close strays by PID); CPU reading logged (`powershell (Get-CimInstance Win32_Processor).LoadPercentage`);
add U3-37 and U3-37.1 .. U3-37.8 to `tasks.json` (single-line items after the last U3-36 line, CRLF kept; see how NIGHT22 did it); compile once in the background and read `errors`; run `tests N22SharedTests`.

## U3-37.2 Reel n22-4 (0:10-0:35)
One build is already there (`Builds/n22-4`, built from 5f932f2 + the dusk light change). Run, alone and in this order, each to `Logs/n23-*.txt`:
1. ON: `-skygrindReelOnly arcline2,stamp,deadline,grind,replay,code,ghostdaily,dusk,haptics,directed` (side `on`, timeout 900).
2. RED: the same ids with `-skygrindOff21` (side `red`).
3. PAD: `-skygrindReelPad -skygrindReelOnly arcline2,haptics,stamp,code,replay` (side `pad`).
Then, one process at a time in the background: `feel-metrics.py metrics` for `on`, then `pad`; read RED lines from the reel result lines (they print the evidence), run metrics on `red` only for
rows whose RED needs a number. Record each row's RED and GREEN result line (and the build, the date) in `NIGHT22-PROOF.md`. A scenario that cannot show RED is fixed before its row counts.

## U3-37.3 Triage (0:35-0:45)
Write a table in `NIGHT23-LOG.md`: row, scenario line, failing check, owner (lead or builder), fix size (S / M / L). Fix order: (a) anything that crashes or throws, (b) magenta pixels, (c) text under 18 px or
a pad-pass keyboard word (S12), (d) the nine contracts' own checks, (e) M9 pop-ins, (f) M11 roles. Cut rows whose fix is L and whose contract is not close: they are SHORT with the numbers.

## U3-37.4 Fix round 1 (0:45-1:10), rebuild once, reel only the changed rows
Likely work, from NIGHT22's open numbers (confirm against the reels first):
- STAMP exit and T-11: exit must read 150-200 ms and the grade card must not intersect POINTS (T11 meter 0 frames); entrance 200-300 ms (STAMP-T).
- DUSK CLOCK: LUM thirds must fall by at least 3.0 over rounds 1 / 7 / 15 with the light dimming; if the light dim makes the map unreadable (read the frames), tune `design.n22.duskClock.lightScale / ambientScale` (both copies, `_why`) and say what you saw. The look must stay violet / cyan, never magenta or red-orange.
- ARCLINE chip on the Player: three looks differ (READY / COOLDOWN seconds / NO TARGET at 40 % alpha, none red), the pad press of Y yanks (`arclinePull 1`), peak speed 20 m/s or more.
- GRIND: smoke step reaches wave 2 via the pad on the MODE row; no ROUND wording; RESULTS shows best wave; records survive a restart.
- REPLAY: VIEW offered at DOWN, plays at half speed, returns to RESULTS; the prompt text is 18 px and not in the central 40 % (S14). Turn `design.n21.replay` on only if the contract passes.
- CODE sheet and ghost verdict: the CODE item is reachable from the pad, shows the typed code, RESULTS shows the code line; the ghost verdict reads AHEAD / BEHIND with seconds (the DAILY needs a first finish: the scenario must set that up and print it).
- Haptics: the reel's haptics log has distinct lines for shockwave, attach, kill, DOWN on the pad pass; VIBRATION off logs none.
- Directed cue: the hunt / beacon step list shows, current step cyan, never overlapping the round banner (a rect check).
Use at most two resumed builders (S for Stamp / Deadline / Results, M for Arcline / Haptics / Directed) on a worktree each, each told the exact failing line; the lead integrates, compiles, builds and reels.

## U3-37.5 DEADLINE round 15 (a decision, not a build; 15 minutes)
The measure is 61 % at round 15 (cap 50 %). Decide in the log, with the rejected alternative: (A) keep 18 s / 5 s and record the contract at round 10 only, 46 %, naming the change;
(B) let a burn kill at round 12 and over (a rule change, needs a harness number before and after). Default if no time: A, the DEADLINE row SHORT-but-honest. Never retune silently.

## U3-37.6 Fix round 2 (1:10-1:20)
Second and last rebuild. Reel only rows still failing. After this, no more code changes except a failing-test fix.

## U3-37.7 Freeze and verify (1:20-1:45), nothing else running
1. `FingerprintBaseline.N22OnPath` stays false unless a sim-changing default is on (none is: trickChain / overdrive off, directed render-only). Confirm: fingerprint EQUAL, `--off22` EQUAL, `--off21` EQUAL to N20, `--off19` EQUAL to N18, golden 0 mismatches, `n22shared`, `n21shared`.
2. The suite once (`bash Tools/unity-loop.sh tests`, async; nothing else running; budget 15 minutes). Re-run failing files alone twice before calling a flake; replays 22/22, golden 97/97.
3. A final build (build 4 at the latest) from the frozen HEAD; one smoke alone (`-skygrindSmoke`; 200 s; it pauses when a window takes focus); the three fingerprint lines on the Player.
4. Restore `LiberationSans SDF - Fallback.asset` and `Mobile_RPAsset.asset` if modified; `git diff --stat ProjectSettings` and understand every line.
5. Bench: only if CPU is under 30 %: three runs, median (A-6). Otherwise "not checked" with the readings.

## U3-37.8 Close (1:45-2:00)
`NIGHT23-LOG.md` close table (below), `NIGHT23-REPORT.md` (one page for GG: per row what he can do, its switch, the contract's number, what felt worse, what is not verified),
`NIGHT22-FEATURES.json` `passes` / `evidence` from this night's lines, `NIGHT22-PROOF.md`, `tasks.json`, `HANDOFF.md` (a NIGHT23 block: use the `handoff` skill), `docs/memory`
(wave-memory harvest: what the crash cost and the shared-machine lesson), `FEEL-PLAYTEST.md` (10 minutes, pad first, per row what to look at). Tag `u3-n22` on the close commit; `date`; print the table; "NIGHT23 CLOSED".
</tasks>

<definition_of_done>
| Item | DONE when (from this session's output) |
| --- | --- |
| U3-37.1 | Editor gate logged, CPU logged, tasks.json parses, N22SharedTests green |
| U3-37.2 | three reel invocations ran on n22-4; every row has a RED and a GREEN-or-failing result line in NIGHT22-PROOF.md |
| U3-37.3 | the triage table is in the log |
| U3-37.4 / .6 | each fixed row has a before and after line; rebuilt at most twice; rows that failed twice are SHORT with numbers |
| U3-37.5 | the DEADLINE decision is logged with the rejected alternative |
| U3-37.7 | fingerprint x4, golden, suite totals, final build, smoke result printed; or SHORT with the measured blocker |
| U3-37.8 | the docs written, `u3-n22` tagged, the close table printed, "NIGHT23 CLOSED" |
A row is SHORT with a measured reason: a cap, a GG decision, a blocker after two approaches, the time limit once the clock reads 2:15.
</definition_of_done>

<final_report>
`NIGHT23-REPORT.md`, one page. Outcome first: the contracts that now pass on a Player out of 9 (NIGHT22: 0). Then each row: what GG can do, its switch, the contract's number,
what is still wrong. Then what is not verified and the DECIDE questions (DEADLINE round 15; keep replay on?). Every number comes from a run this session.
</final_report>

<closing_reminder>
- **One heavy job at a time.** The machine is shared; a crash and a fork failure already cost NIGHT22 its close.
- **Reel before you fix, fix before you build, build twice at most.**
- **No new features.** Prove, fix, freeze, tag.
- Commit every slice. Never push, merge or commit to master; never stage `UnityConnectSettings.asset`.
- Every message ends with `N23-STATUS: <d> done, <s> short, <l> left (of 8) · build <n>/4 · passes <p>/9 · <h:mm> / 2:15`.
</closing_reminder>

## The kickoff

Launch `claude --model claude-opus-5-5 --append-system-prompt-file PROMPT-STOPS.md`, run `/effort high`, then paste:

```text
/goal Met only when a `date` reads 1:50 or later after the first `date` AND NIGHT23.md's close table is printed with every row U3-37.1 .. U3-37.8 DONE (its done-when evidence from this session) or SHORT (a measured reason: a cap, a GG decision, a blocker after two approaches, or the time limit once a `date` reads 2:15), followed by "NIGHT23 CLOSED"; SHORT rows count as met; at 2:15 the run stops whatever the table says. Run NIGHT23.md in C:\Users\gaige\dev\SKYGRIND-unity: `date` first, then the Editor gate, read CLAUDE.md, NIGHT23.md, NIGHT22-LOG.md and NIGHT22-REPORT.md, reel Builds/n22-4 first, fix what the reels show, freeze, run the suite once, tag u3-n22. One heavy job at a time; at most 2 builders in worktrees; commit every slice; never push, merge or commit to master; the web repo is read only; never ask me questions. After a crash: git status, git log and the last log lines first. End every message with the N23-STATUS line.
```

# NIGHT60.md: OVERNIGHT RUN on SKYGRIND (about 8 h unattended; hard cap 8 h 30 min)

GG, 2026-10-21 ~22:3x, verbatim (this outranks everything below it):
> dont follow this, take this but make it reated to this project, plan next night prompt using this as a base, output should be next
> copty pastable goal pormpt

The base is the "Overnight Run" template GG pasted with that message. Its words are kept where they fit this project and adapted where
CLAUDE.md or a measured lesson says otherwise; each adaptation that changes its meaning is a D-id in `DECIDE60.md` (untracked; D-1..D-7
written by the forge). Order of authority: the org rules, GG's words above, CLAUDE.md, this file, HANDOFF.md, the logs.

You are working unattended for about 8 hours. Nobody will answer questions until morning. Read this whole file before doing anything,
and re-read it after any context compaction or resume.

The goal of tonight is not "do as much as possible." It is "leave the project clearly better, with every change verified, and a clean
report of what happened." Ten verified improvements beat thirty unverified ones.

## 0. Start of every session (including after resume or compaction)
1. Read `docs/memory/LESSONS.md` fully (`docs/memory/CORE.md` loads with CLAUDE.md). These are real mistakes from past nights. Treat each
   row as a rule.
2. Read `NIGHT60-LOG.md` to see what is done, in flight, and blocked (create it at the first session: start time, Progress, Assumptions,
   Blocked, Unverified).
3. Read `NIGHT60-BACKLOG.md`.
4. If NIGHT60-LOG.md has no start time, get the current time from the shell (`date`) and write it at the top: that is t0 (the same
   moment as this conversation's first `date`, which the /goal counts from).
5. At the first session only, BEFORE step 6, the base and the night's tools (L-107: a close can claim what the repo does not have):
   - `git tag -l u3-n59` prints u3-n59 and HEAD contains it; `df -h /c` shows >= 15 GB free.
   - The Editor answers: `bash Tools/unity-loop.sh status` returns and shows `"playMode": "stopped"` (NIGHT59 found it playing: `bash
     Tools/unity-loop.sh stop`). If it times out, it is unfocused (memory: unity-editor-unfocused-autotick): idle >= 45 s, the AttachThreadInput
     foreground moment, then `unity command set_autotick --enable true --interval_ms 16 --caller plugin --skill unity-cli`. Never kill it.
     After two failures: gates with `--no-editor`, builds in batch mode from `C:\Users\gaige\dev\SKYGRIND-unity-batch`, logged.
   - Make three night tools and commit them: `Tools/night/n60-prune.sh` from n59-prune.sh (n60-* tags); `Tools/night/n60-reels.sh` from
     n56-reels.sh with n60-prune.sh in its prune loop (n56-reels.sh's loop calls n56-prune.sh, which refuses every non-n56 tag, so nothing
     would prune); `Tools/night/n60-gates.sh` from n58-gates.sh with `cd "$(git rev-parse --show-toplevel)"` in place of the fixed path
     (so a worktree checks itself), `fingerprint --off59` and `--off60` added (both parse and print EQUAL: checked 2026-10-21 22:34), a FAIL
     when the Editor's `consoleErrors` is not 0 (n58's only prints it), and `--flow` comparing n46flow `--off60` on n52-foundry, n52-iceline,
     n57-kiln, n52-span, n52-spire with a baseline `Logs/n60/flow-base.txt` you make now (~5 min), its outputs under Logs/n60/.
6. Run the check command (section 9) to see the baseline. If it already fails, fixing that is task one.

## 1. Ground rules
- Work only on `port/stage3` (CLAUDE.md; DECIDE60 D-2). Never push, force push, merge, rebase, rewrite history, commit to master or
  `gh auth switch`.
- Never touch anything under Off-limits (section 9).
- Never delete files outside the repo. Inside the repo, only delete files you created tonight (scratch files; your own reel frames through
  `n60-prune.sh`). Earlier nights' builds, worktrees and evidence stay; any other deletion is a proposal for GG (NIGHT60-PROPOSALS.md;
  CLAUDE.md rule 10).
- One task at a time per worker: plan, build, verify, commit, log. Then the next one. The workers are the lead plus at most two builders in
  git worktrees (the lead integrates by cherry-pick) and at most two fresh-context reviewers alive at once (DECIDE60 D-4; L-003: the usage
  limit stops every agent at once). Builders never run the Editor, a build, a Player, a reel or the suite: in their worktree they run only
  `bash Tools/night/n60-gates.sh --no-editor` and the sim tools, and commit a task's test before its fix. The lead runs the full gates, the
  EditMode test RED then GREEN, and the Player evidence after each cherry-pick, and only then marks the task done.
- One task per commit, with explicit paths plus their `.meta` files (never `git add -A` or `.`). Message format:
  `NIGHT60 <backlog id>: <what> (MAIN before -> after)`, trailer `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`. Before a commit,
  restore Play mode's rewrite of the TMP fallback asset (CLAUDE.md rule 1).
- Do not edit a test just to make it pass. If a test is genuinely wrong (a later ON row changed what it pins), fix it and log why in
  NIGHT60-LOG.md (NIGHT58's N52SharedTests fix, ea053e54, is the model).
- Do not refactor or restyle working code that no task asks you to touch.
- When you would normally ask GG a question, pick the more conservative, easily reversible option and log it as a D-id in DECIDE60.md
  (the reason and the alternative rejected) and one line under "Assumptions" in NIGHT60-LOG.md.
- New player-facing behaviour lives behind a design row, OFF by default (`design.n60.<row>`; `-skygrindOff60` and a one-row switch give the
  old game back, fingerprints EQUAL), with a three-line PLAY note and a DECIDE row. A row turns ON only by NIGHT57.md section 0's rule
  (read it by its absolute path `C:\Users\gaige\dev\SKYGRIND-unity\NIGHT57.md`).

## 2. Picking work (in this order)
1. A failing baseline check.
2. Items in NIGHT60-BACKLOG.md, top to bottom (an item whose "only after" is not met waits; say so in the log).
3. When the backlog is empty, run an audit pass over the project: (a) every Player log of tonight's reels and smokes (NRE count,
   exceptions); (b) `bash Tools/unity-loop.sh errors`; (c) the EditMode Core suite (`bash Tools/night/n41-suite.sh n60 Core`, ~13 min, never
   beside a build or a reel; the Game suite runs only in the close, section 8, because Game M once wedged the Editor); (d) THREADS.md rows
   aged two waves or more; (e) FEATURES.md rows OFF with
   a written next step; (f) docs/VERBS.md rows without a check; (g) the frame time (`Builds/<tag>/SKYGRIND.exe -skygrindBench 60`, the power
   plan polled before and after, median of 3); (h) UI text >= 16 px at 1080p and the dusk palette (CLAUDE.md rule 8) on reel frames;
   (i) pad-first: every verb reachable on the pad. Write each finding to NIGHT60-BACKLOG.md under "Proposed by Claude", with a one-line
   reason and a size (S, M, L).
4. Build S and M items from that list if they fit the project's stated purpose (CLAUDE.md, docs/PRINCIPLES.md, docs/memory/OWNER.md's wants).
   New features are fine at S or M size, behind a row.
5. Do not build L items. Write a short spec for each one in `NIGHT60-PROPOSALS.md` instead so GG can decide in the morning. Never, at any
   size: audio, enemy looks or new enemy kinds (GG's), level 6, the web repo.

## 3. Before writing code
- For anything touching more than about 5 files, write a short plan in NIGHT60-LOG.md first.
- If you are using a Unity, URP, Input System or TMP API you are not certain about, check the real docs (context7 for Unity 6000.3, or the
  installed package source under Library/PackageCache) before writing code. Do not guess function signatures or config keys. Note the
  source in NIGHT60-LOG.md.
- Read the surrounding code first and match its existing patterns: night partials (`X.N60<letter>.cs`), UnityTuning partials with one
  comment per number, `NightRows.ViewOn(cfg, "<row>")`, one writer script per row set (`Tools/night/n60-<letter>-rows.cjs`; both config
  copies byte-identical), every gameplay number from config (CLAUDE.md rule 5).
- Read a number's `_why` before choosing which way to move it (L-110).
- After adding a `.cs` file, compile and check the new type is in Library/ScriptAssemblies; if the compile says "up_to_date", force
  ImportAsset + RequestScriptCompilation (memory: pick-new-file-stale-editor) before any build.

## 4. Definition of verified
A task is done only when all of these are true:
- The check command passes.
- Logic changes have a test that would have failed before the change: an EditMode test seen RED, then GREEN
  (`bash Tools/unity-loop.sh tests <ClassName>`). A Skygrind.Core change also keeps golden at 0 mismatches and 22 of 22 replays exact.
- Anything the player sees or plays was actually run and looked at, not assumed: a Player build (`n41-build.sh`), its reel
  (`n60-reels.sh`), a meter that reads the frame's pixels with a planted case (L-093, L-108), and you open at least one frame on and one
  off (the Read tool on the PNG). A level change also gets n46flow on seeds 1-10 and 11-20 against its baseline, the room contract
  (`n59-l-contract.sh`) and verbs37 on that level (L-090). The Editor never loads a campaign level (L-043).
- You re-read your own full diff (`git show <sha>`): no debug prints, no commented-out code, no stray TODOs you added, no unrelated
  changes, no gameplay number typed by hand.
- For M-size tasks: spawn a subagent with fresh context, give it the task description and the diff, ask it to find problems. Fix what it
  finds or log why you disagree. For a row that would turn ON: the one-row critic on its Player evidence (L-109; model brief
  Research/2026-10-21-wave59/gate-l0-brief.md; first line `<id>: PASS | FAIL <reason> | UNSURE <reason>`). A row whose own MAIN misses
  needs no one-row critic (it stays OFF with its numbers), but committed M-size code still gets the fresh-context diff review.
If you cannot verify something, it is not done. Mark it "unverified" in NIGHT60-LOG.md (a row stays OFF) and move on.

## 5. When stuck
- After 3 real attempts or about 45 minutes on one task, stop.
- Undo only your own work: `git revert <your sha>` (a new commit), or, for uncommitted work, restore your own files by explicit path
  after reading their diff. Never `git reset --hard`, `git checkout -- .` or `git clean`: GG's uncommitted edits live in this tree (L-047;
  DECIDE60 D-3).
- Log it under "Blocked" in NIGHT60-LOG.md: what you tried, what failed, your best guess at the cause.
- Move to the next task. Do not loop on the same failure.
- Tools that hang: the Editor does not answer -> never kill it; build in batch mode from `C:\Users\gaige\dev\SKYGRIND-unity-batch` and say
  so. A Player you launched: close it by its PID. Never two builds, two Players or a build beside the suite (L-005).

## 6. Learning from mistakes
Any time something goes wrong (you broke a test, made a wrong assumption, had to revert, misused a tool, rushed a step), append one row
to `docs/memory/LESSONS.md` in its table format, the next id from L-113:
`| L-NNN | <the rule for next time> | <what happened and why: the date, the file or commit> | <scope> | new | w60 |`
Keep rules concrete and checkable ("run the Player smoke with no agent running: a window taking focus pauses it"), not vague ("be more
careful"). This file carries over to future nights, so write it for a future version of you.

## 7. Pace
You have hours. There is no bonus for speed. The moment you think "this is small, I can skip verifying it" is exactly when to verify it.
If you notice you are rushing, stop, re-read section 4, and slow down. Commit each finished task at once: the usage limit can stop every
agent at any minute (L-003), and a committed task survives it. Measured costs (NIGHT59): a Player build 1-3 min; one reel ~40 s; verbs37
x5 ~6 min; the smoke ~5 min; n46flow three seed sets in parallel ~3 min; a room contract ~3 min; a one-row critic 17-23 min.

## 8. Ending the night
Stop starting new tasks when either: 8 hours have passed since t0, or the backlog (after an audit pass) has no S or M items left. No new
M task after 6 h 30 min and no new S task after 7 h 30 min; at 8 h commit what is verified, log the rest as "unverified", and close. The
close takes about 45 minutes (NIGHT59: the gates to the smoke alone took 27 min); the /goal's clock escape is 8 h 30 min.
1. Run the full check command one final time and show the output. Build `n60-freeze` from HEAD (rows as the repo's config), run verbs37 x5
   (`bash Tools/night/n60-reels.sh n60-freeze vf,n52-foundry,verbs37 vi,n52-iceline,verbs37 vk,n57-kiln,verbs37 vs,n52-span,verbs37
   vp,n52-spire,verbs37`; NIGHT59: 27 / 29 / 31 / 31 / 31; span58 stays OFF tonight, so n52-span is the shipped level 4) and the smoke
   (`bash Tools/night/n56-smoke-steal.sh n60-freeze`, no agent running: "DONE: all steps passed"); show those lines. A failure is logged
   under Blocked and named in the report, never hidden. Only now, with the Editor no longer needed, B11 (the Game suite):
   `bash Tools/night/n41-suite.sh n60 Game ABCDEFGHIJKLNOPQRSTUVWXYZ`, then `bash Tools/night/n41-suite.sh n60m Game M class`; a batch that
   times out is logged TIMEOUT, not waited out past the close. B11 never counts toward "no S or M item left".
2. Consolidate docs/memory/LESSONS.md (merge duplicates, retire a contradicted row with the reason, keep it under 150 lines).
3. Write `NIGHT60-REPORT.md` for GG in plain words:
   - What shipped (one line per commit)
   - What is blocked and why
   - Assumptions you made that GG should confirm (DECIDE60 ids)
   - L-size proposals waiting for him (NIGHT60-PROPOSALS.md)
   - The first thing he should look at in the morning, and a three-line PLAY note for each change he can feel
4. Rewrite HANDOFF.md; wave-memory harvest 60 (the ledger, THREADS under 60 lines, FEATURES, CORE); print `git tag -l` and each ON row's
   `enabled` and match them to the report (L-107); commit with explicit paths; tag `u3-n60`.
5. Print the line `OVERNIGHT COMPLETE` as your final message.

## 9. Project-specific
- **Check command:** `bash Tools/night/n60-gates.sh` (made at the first session, section 0 step 5: both config copies identical;
  CoreHarness golden "0 failing, 0 mismatches" with 22 of 22 replays exact; `fingerprint` plain / --off57 / --off58 / --off59 / --off60
  EQUAL; the Editor compile with "compilationFailed": false and 0 console errors). Before a row turns ON, NIGHT57.md section 0's flow gate
  is `bash Tools/night/n60-gates.sh --flow` tonight. After any change the player can see, also the Player smoke (section 8).
- **How to run the app:** the Editor is open on this project: `bash Tools/unity-loop.sh status | compile | errors | tests <filter> | play |
  stop` (never recompile in Play mode; never `unity test`, it starts a second Editor). A Player: `bash Tools/night/n41-build.sh <tag>` ->
  `Builds/<tag>/SKYGRIND.exe` (it refuses in Play mode or under 10 GB free); a row on in that build only:
  `node Tools/night/n46-build-rows.cjs <tag> on <night.row>`; reels: `bash Tools/night/n60-reels.sh <tag> <side>,<level>,<scenario>[,<arg>]`
  (a campaign level boots only through a reel; it prunes beside the set), then `bash Tools/night/n60-prune.sh <tag>`; the bot pilot:
  `dotnet run --project Tools/SimHarness -c Release -- n46flow --level <id> --seeds 10 [--first 11]`.
- **Off-limits files or folders:** ProjectSettings/UnityConnectSettings.asset, ProjectSettings/EditorBuildSettings.asset,
  Assets/ProjectSettings/UIEffectProjectSettings.asset, Assets/Scenes/Arena.unity and the four TMP font assets in Assets/Resources/Fonts
  (GG's uncommitted edits: never stage, never checkout); the web repo `C:\Users\gaige\dev\skygrind` (read-only); Packages/ (no package or
  lockfile changes); Assets/Scripts/Core/Mover.cs unless a task names it; any audio (code, AudioSource, sound files); enemy models and new
  enemy kinds; .unity / .prefab YAML by hand while the Editor is live; earlier nights' Builds/ and .claude/worktrees.
- **What this project is for, in one sentence:** SKYGRIND is a first-person movement shooter at dusk (CoD Zombies rounds x ULTRAKILL
  movement), built only in Unity 6000.3 URP for Steam and hopefully Switch (pad-first), owned by GG.

## /goal (paste this alone in a fresh Claude Code session in `C:\Users\gaige\dev\SKYGRIND-unity`; one line, under 1,800 characters)
```
/goal Met when this conversation shows NIGHT60-REPORT.md written, the final `bash Tools/night/n60-gates.sh` output, the n60-freeze smoke line (or its failure logged under Blocked), `git tag -l u3-n60` printing exactly u3-n60, and `OVERNIGHT COMPLETE` as the final message; start that close when a printed `date` shows 8 h since this conversation's first `date`, or earlier if NIGHT60-BACKLOG.md has no S or M item left after an audit pass; or met when a printed `date` shows 8 h 30 min since that first `date` and NIGHT60-REPORT.md is written with what is done; or at 10 h whatever is done. Read NIGHT60.md in full before anything and again after any compaction or resume; run its section 0 every session. Print `date` and elapsed time at the start of every turn. Never ask me: take the conservative, reversible option and log it in DECIDE60.md. One task at a time per worker (the lead plus at most two builders): plan, build, verify, commit, log; no new M task after 6 h 30, no new S task after 7 h 30. Verified means n60-gates GREEN, a test seen RED then GREEN for logic, a Player reel plus a pixel meter with a planted case plus a frame you looked at for anything the player sees, your full diff re-read, and a fresh-context reviewer for M code; rows turn ON only by NIGHT57.md section 0's rule; span58 stays OFF. Stuck after 3 tries or 45 min: git revert your own commits (never reset --hard), log Blocked, move on. Every mistake becomes a row in docs/memory/LESSONS.md. Commit each task with explicit paths; never push, merge, touch master or GG's files.
```

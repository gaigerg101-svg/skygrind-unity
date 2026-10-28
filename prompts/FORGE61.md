# FORGE61.md: write the next overnight run (NIGHT61) so it builds real, visible results, polishes, kills dead code, avoids the old mistakes, and tells GG the truth

GG, 2026-10-22: "research best ways to prompt and get better results, skills whatever ... so it can create a powerful goal and loop prompt
to build and not run into mistakes, polish, kill dead code, but need to see real results and it needs to be honest and critical where
things are not good."

**This session builds nothing in the game.** You are the forge. You study the last nights, research how to run long unattended Claude
Code work well, install the guardrails, and write `NIGHT61.md` plus the exact `/goal` line GG will paste. GG starts NIGHT61 himself in a
fresh session. Budget: about 60-90 minutes. Don't ask GG questions; decide and write each call in `Research/<date>-forge61/DECISIONS.md`.

**Run length for NIGHT61: 6 h minimum, 7 h hard cap.** (GG: change these two numbers here if you want another length.)

## 1. Get your bearings from the files (the files are the memory)

`date`, `pwd`, `git status`, `git log --oneline u3-n54..HEAD`. Read CLAUDE.md, HANDOFF.md, docs/memory/CORE.md, THREADS.md, LESSONS.md
(all of it: the L-rows are what went wrong before), docs/PRINCIPLES.md, docs/DESIGN-RULES.md, nights/PROMPT-FORGE.md, nights/PROMPT-STOPS.md,
NIGHT55.md through NIGHT60.md, and their LOG and REPORT files, NIGHT60-BACKLOG.md and NIGHT60-PROPOSALS.md. NIGHT60.md is the newest
"Overnight Run" template instance: keep what worked in it.

## 2. The honest retro of nights 55-60 (write `Research/<date>-forge61/RETRO.md`; numbers only from the logs)

Answer each with evidence (file and line, or a measured number):
1. **What did GG actually get to see?** For every row turned ON in nights 55-60: would a player notice it in the first minute of the
   level it touches? Mark each VISIBLE, SUBTLE (needs a side-by-side to spot) or INVISIBLE (only a metric moved). Count the hours spent
   on each class. Example of SUBTLE from NIGHT60: lamp60 changed the door's brightness by 2-14 %.
2. **Repeated mistakes.** Every L-row added in nights 55-60 (for example L-113 the prune pattern and disk at 9.7 GB, L-114 a heredoc
   patch against CLAUDE.md rule 9, L-115 timing off a late log), the usage-limit stops, the MapChoiceTests hang (seen since NIGHT41),
   focus60 pausing the tools. For each: did a written rule stop it next time? If a rule was written and broken again, it needs a hook,
   not another sentence.
3. **Where the hours went.** Setup, tools and harness work, measuring, building game changes, waiting on builds and reels, the close.
4. **What the critics and reports said was still bad and never got fixed.**
5. **What was claimed and not proven** (any "done" without a Player frame or a measured number behind it).

## 3. Research (write `Research/<date>-forge61/RESEARCH.md`; Verified = you read the page; give the URL)

Read these first, then go wider (GitHub, engineering blogs, talks) only where they leave a gap:
- Claude Code: best practices (`code.claude.com/docs/en/best-practices`), `/goal` (`/docs/en/goal`), `/loop` and scheduled tasks
  (`/docs/en/scheduled-tasks`), hooks guide and reference (`/docs/en/hooks-guide`, `/docs/en/hooks`), subagents (`/docs/en/sub-agents`),
  skills (`/docs/en/skills`), worktrees, environment variables (subagent caps).
- Prompting: "Prompting Claude Opus 5" and "Prompting best practices" on platform.claude.com (scope, over-verification, delegation rules,
  test gaming, state files, effort).
- Anthropic engineering: "Effective harnesses for long-running agents", "Harness design for long-running apps", "How we built our
  multi-agent research system".
- Dead code in Unity C#: the Roslyn rules IDE0051 / IDE0052 (and how to make them report on build), the Unity Code Coverage package
  (`com.unity.testtools.codecoverage`) for finding code no test or smoke run reaches, and how other Unity teams find unused scripts,
  shaders and assets. Note which tools need a package added (GG's OK goes in DECISIONS as a proposal, not an install).

What the forge already found (check each against the pages, don't trust this list):
- A check Claude can run (tests, a build, a screenshot compared to a target) is what lets a run finish correctly unattended. Show
  evidence, not claims. Fix root causes, never suppress the error.
- `/goal`: after each turn a separate small model judges the condition from what the transcript shows; it does not run commands or read
  files, so the condition must name things the transcript will print. Up to 4,000 characters. It pauses on a usage limit and can resume.
  A goal with no tool use for several turns is stopped.
- `/loop` fires only between turns while the session is idle, so it is not a heartbeat for a busy `/goal` run. For disk or health
  watching use a background script or the Monitor tool.
- Hooks are deterministic; CLAUDE.md is advice. A PreToolUse hook can block a command; a Stop hook can refuse to let a turn end until a
  check passes. Rules that keep getting broken belong in hooks.
- Opus 5: give the full task up front; don't add "double-check" or "verify again" lines (it already checks; extra verification wastes
  tokens); give explicit delegation rules and keep spawn counts low; it can widen scope, so state the scope; for reviews, ask it to
  report everything and filter in a separate pass instead of "only report high severity" (that makes it report less).
- Long runs: a JSON feature list with a pass flag that only flips on evidence; a progress file; git commits per slice; one feature at a
  time; tests are never edited to pass ("tests verify correctness, they don't define the solution"; if a test is wrong, say so).
- A separate evaluator is easier to make skeptical than a builder is to make self-critical, but out of the box it talks itself into
  approving. It needs hard thresholds, few-shot examples of real failures, and tuning against its own past verdicts.

## 4. Install the guardrails (repo files, not game code; commit them with explicit paths)

Turn the rules that keep breaking into things that cannot break. For each one, read the hooks docs for the exact JSON, then **test it
with a planted violation (L-034)**: show the blocked command and the hook's message in the transcript.
- **PreToolUse blocks:** `git push`, `git add -A` / `git add .`, `gh auth switch`, commits to master, staging GG's four files
  (ProjectSettings/UnityConnectSettings.asset, ProjectSettings/EditorBuildSettings.asset, Assets/ProjectSettings/UIEffectProjectSettings.asset,
  Assets/Scenes/Arena.unity), heredocs or `node -e` that write into docs or code (CLAUDE.md rule 9, L-114), `unity test`, and deleting
  `.claude/worktrees` (GG's call).
- **Disk:** a check that refuses to start a reel or a build under 12 GB free (L-104, L-113), plus a background watcher the night starts.
- **Custom subagents in `.claude/agents/`** (model opus, read-only tools where they only judge):
  - `skygrind-critic`: judges one finished slice from its contract, the before / after frames and the numbers only, never the builder's
    report. It gets 3 few-shot cases from the real logs: a true pass, a SUBTLE change that should fail the visibility test (lamp60), and
    a pilot-read-as-game failure.
  - `gg-proxy`: plays GG from his own words only (docs/memory/OWNER.md and the verbatim blocks in the NIGHT prompts). It ranks what is
    still bad on the newest frames, and gives no praise.
  - `dead-code-auditor`: finds dead code with evidence and proposes deletions in batches.
- **Update the two project skills only where the retro shows a gap** (`skygrind-idea-lab`, `skygrind-retro`).

## 5. Write NIGHT61.md (the run) and the `/goal` line

NIGHT61.md must carry all of this, kept as short as it can be while staying complete (long files get their rules ignored):
1. **Bearings first:** pwd, git log, the progress file, the slate, then one basic end-to-end check of the current build before any new
   work (the smoke, or verbs37 on one level).
2. **The slate:** `Logs/n61/slate.json`, one entry per slice: id, title, why (source: retro line, critic file, GG quote), owner, row,
   `red` (the before), `doneWhen` (hard thresholds, written before any measurement), `visible` (what a player sees in the first minute,
   with the frame that shows it), `passes: false`, state. Only the lead flips `passes`, and only on the critic's pass.
3. **The visible rule:** a slice can turn ON only if GG would notice it in normal play. In a side-by-side of before / after frames, a
   critic who wasn't told what changed must spot the difference within 5 seconds. Changes that only move a metric are allowed for bugs
   and dead code, and go in the report under their own heading.
4. **Order of work:** the honest state check, then fixes for what GG has asked for again and again, then the critics' top problems,
   then polish, then the dead-code pass, then the beyond list. Ranked by what a player sees, not by what is easy to measure.
5. **The dead-code pass:** dead means proven unreached: no references (search + IDE0051 / IDE0052 if they can run), not used by any ON
   row, not needed by the `--off` chain of the last two nights, no test or smoke run reaches it. Delete in small batches. After each
   batch: compile, golden 0 mismatches, 22 replays exact, fingerprints EQUAL, the touched test batches, verbs37. Each batch is one commit
   with a ledger line in `Logs/n61/DEADCODE.md` (what, evidence, lines removed). OFF rows that were never turned on and are older than
   five nights go on a list for GG with a one-line reason each. Nothing GG decides gets deleted.
6. **Honesty:** every claim names the command or frame behind it; anything not checked says "not checked"; a failed slice is reported as
   failed with its number; tests are never edited to pass; the critic reports every problem and the lead filters afterwards. The report
   opens with **"What is still not good"**: the 10 worst things in the game right now, from gg-proxy and the critics, with frames,
   whether or not tonight touched them.
7. **Delegation:** at most four builders plus the critics, worktrees created one at a time with a disk check between; set the subagent
   caps through the environment variables the docs name; subagents write results to files and return a short pointer; no subagent to
   "double-check" a builder (the critic gate is the one independent check).
8. **The clock** from section 0's run length, with integrations and builds at fixed times, a freeze, and a close that cannot start
   before the minimum and must finish by the cap. The usage-limit plan: everything committed per slice, and a resume path from the slate
   and the progress file.
9. **All of CLAUDE.md's hard rules**, by reference plus the five most-broken ones restated (with a note on which are now hooks).

**The `/goal` line** (paste-ready, under 4,000 characters): it names only things the transcript will print (`date` and elapsed time each
turn; the slate's pass / SHORT count; the gate lines; the tag; the committed close files), includes the minimum and the cap, and says
the close is required at the cap. Put it in `Research/<date>-forge61/GOAL.txt`.

## 6. Attack the prompt before GG runs it

1. Spawn one fresh subagent with only NIGHT61.md and CLAUDE.md. It lists every place NIGHT61 is ambiguous, unmeasurable, in conflict
   with CLAUDE.md, rewards gaming a number over a visible result, or can't be done in the time. Fix what it finds that matters.
2. Dry-run the first 15 minutes: every path, script and command NIGHT61 names exists and runs (`--help` or a no-op where it would
   change state). Fix every miss.
3. Check the `/goal` condition: could the evaluator mark it met without the work being done? Could it never be met? Fix both.

## 7. Hand-off to GG (your last message, short, plain words)

- The three lines GG types: `cd C:\Users\gaige\dev\SKYGRIND-unity`, the `claude` start command with `--dangerously-skip-permissions` and
  the model, and the `/goal` line (or where it is).
- What changed in the guardrails (hooks, agents), each with its tested result.
- The honest part: what NIGHT61 still can't guarantee, and anything that needs GG's OK first (a package install, deleting the old
  worktrees, the OFF-row list).
NIGHT61.md stays untracked (night prompts always do). Commit the hooks, agents, skills and the forge's Research folder with explicit
paths. Never push.

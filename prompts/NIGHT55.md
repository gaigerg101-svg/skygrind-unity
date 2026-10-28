# NIGHT55.md: the catch-up and push night. Finish everything left open, then research, iterate and prove the game forward (lead + up to five builders + independent critics; at least 4 h, hard cap 4 h 30 min)

GG, 2026-10-20, verbatim (this outranks everything below it):
> have it finish and continue anything that it wasn't able to finish then also simultaneously spin up subagents following the best methods
> using Opus 5.5 in terms of just getting the best results skills everything ... build ... for the next four hours nonstop basically as
> soon as I submit this prompt in claude code I want it to be able to catch up where it left off Research find areas to iterate and prove
> build not just basic things and then do that for the next 4 hours minimum 4.5 hours hard cap also go well beyond this prompt ... think
> of what has been proven to work and follow that in creating the prompt that will order you know the research and just how everything
> goes with the building and iterating

**GG will not answer during the run.** Decide every call in `DECIDE55.md` (untracked) with its reason and keep going. If GG writes, his
words come first. Base: `port/stage3` at `u3-n54` (check `git rev-parse u3-n54` against HEAD; if HEAD is newer, base on HEAD and log
why). t0 = the first `date` you print. Print `date` and the elapsed time at the start of every turn: the `/goal` evaluator reads them.

## 0. Why the night is shaped this way (methods that have been shown to work; follow them, they are the plan)

| method | source | what it means tonight |
|---|---|---|
| A fresh session gets its bearings from files: pwd, git log, the progress file, the feature list, then a basic end-to-end check before new work | Anthropic, "Effective harnesses for long-running agents" | Phase A0 below. The files are the memory: HANDOFF, the logs, docs/memory |
| A feature list in JSON with a `passes` flag per item; agents only flip the flag after verifying, never rewrite the list; one feature at a time; commit each | same | `Logs/n55/slate.json` (section 2). Only the lead flips `passes`, and only after the critic's verdict |
| Planner, generator, evaluator as separate roles; the evaluator tests the running thing like a user, with hard thresholds per criterion; a standalone evaluator is easier to make skeptical than a builder is to make self-critical; out of the box it talks itself into approving, so tune it | Anthropic, "Harness design for long-running apps" | The lead plans, builders build, a separate critic judges from Player frames and numbers, never from the builder's report. A miss on any threshold fails the slice |
| Sprint contracts: before building, the builder writes what it will build and how success is checked; the evaluator agrees first | same | Each slice starts with a contract in the slate (done-when + RED proof) that the lead accepts before code |
| Delegation briefs need an objective, an output format, the tools and sources to use, and boundaries; vague briefs caused duplicated work and gaps. Launch 3-5 subagents in parallel. Subagents write results to files and return a short reference | Anthropic, "How we built our multi-agent research system" | Every spawn prompt follows section 5's template. Research and critic agents run in parallel. Everything lands in files under `Research/2026-10-20-wave55/` and `Logs/n55/` |
| LLM judging works best with one judge, one rubric, a 0-1 score and a pass / fail per criterion, on cases with a clear right answer; judge the end state, not the path | same | The critic rubric in section 4. Blind A/B pairs (`Tools/night/n46-blind-pairs.cjs`) shown in both orders (A/B then B/A) so position bias cancels |
| Measure, then change; feel is done when the screen measurably changes (L-001); RED once, GREEN once (L-014) | this repo's LESSONS | No slice without a before and an after |
| Simplest harness that works; add parts only when needed | Anthropic, harness design | At most five builders. Fewer is fine if the slate is small |

## 1. Phase A: catch up, research, plan (0:00-0:50)

**A0, bearings (lead, 0:00-0:15).** `date`, `pwd`, `git status`, `git log --oneline u3-n47..HEAD | head -80`, `df -h` (C: must have more
than 12 GB; else run `Tools/night/n54-prune.sh` and delete only your own old frames). Read: CLAUDE.md, HANDOFF.md (the NIGHT54 and EXT53
blocks), NIGHT54-REPORT.md, DECIDE54.md, docs/memory/CORE.md, THREADS.md, the top of LESSONS.md, docs/PRINCIPLES.md, docs/DESIGN-RULES.md,
Logs/n47/ASKS.md, NIGHT53-REPORT.md (THE MIRRORS rows, DECIDE53 D-22). Then one basic end-to-end check before any new work: boot
`Builds/n54-freeze2/SKYGRIND.exe -skygrindSmoke` with nobody at the machine and note the result (it was not clean at NIGHT54's close).

**A1, the open ledger (one research subagent, the "auditor", in parallel with A2-A4).** It writes `Logs/n55/OPEN.md`: every unfinished
item with its source line and its last measured number. At least:
- NIGHT54 "not done": THE WICK's and THE CROWN's palettes (next try: grade shadows only, measure the walls, not the whole frame; D-16),
  the smoke not clean, the step-down camera smoothing unmeasured, SSAO noise, decor54 too subtle on screen.
- NIGHT47's G9 extras still OFF: wickRideParry47, wickIntro47, wickFinale47.
- THE MIRRORS (drafted, replaced on NIGHT54; rows in NIGHT53-REPORT.md and DECIDE53 D-22).
- Every SHORT row in the close tables of NIGHT47 to NIGHT54 and EXT53.
- THREADS aged 2+ that are not marked "GG decides".
- GG-only items are listed under "GG's calls" and not touched: T-18, T-20 (UnityConnectSettings), T-57 level 6, the 179 old agent
  worktrees in `.claude/worktrees` (~34 GB), GG's four modified files.

**A2, the critic panel (three role-play subagents in parallel, read-only, Opus 5.5).** Each gets the newest Player frames and reels
(`Logs/feel/` for n54-freeze2 and the newest of each level and boss; Read the images) and the play reports, and plays ONE role:
- **GG-proxy:** built only from GG's own words in docs/memory/OWNER.md and the NIGHT4x-5x prompts' verbatim blocks. Judges what GG has
  asked for again and again and is still not on screen.
- **First-time player:** has never seen the game; judges what is unclear, unreadable or confusing in the first 10 minutes and in each boss.
- **Movement-shooter veteran** (thousands of hours of ULTRAKILL, Doom Eternal, Titanfall 2): judges flow, feel, weight, arena design and
  boss quality against those games.
Each writes `Research/2026-10-20-wave55/critic-<role>.md`: its 10 worst problems, ranked, each with the frame path it saw it in and what
"fixed" would look like on a frame. No praise section.

**A3, research (two subagents in parallel, using the `skygrind-idea-lab` skill).** (a) For the top 10 items from A1 + A2: how shipped
games and published talks solved the same problem (GDC Vault, developer posts, wikis; Verified / Reported marked; numbers only when the
page gives them). (b) **GitHub:** for each technical item (camera smoothing, SSAO noise, color grading by region, procedural animation,
VFX), find 2-3 well-regarded open-source Unity / URP projects that solve it; read how they do it, then write the method in our own code.
Code is copied only under MIT / BSD / Apache / Unlicense / CC0 with the license added to `THIRD-PARTY.md`; never GPL, never a new package
without the lead's OK and a DECIDE line. Output: `Research/2026-10-20-wave55/web.md` and `github.md`.

**A4, the beyond list (the lead, with `skygrind-idea-lab`'s five lenses).** 5-8 ideas that go past every list above, ranked against
docs/PRINCIPLES.md and GG's taste. Bigger than polish: new boss beats, new uses of existing verbs, a room that teaches a verb by itself,
a finisher, a combo reward. Each with how it would be proven.

**A5, the slate and the pre-mortem (lead, 0:35-0:50).** Write `Logs/n55/slate.json`: one entry per slice, ordered P0 (finish OPEN), P1
(the critics' top problems), P2 (the beyond list). Each entry: `id`, `title`, `source` (OPEN line / critic file / idea), `owner`, `row`
(`design.n55.<row>`), `red` (the before measurement or frame), `doneWhen` (numbers and named frames, hard thresholds), `passes: false`,
`state`, `notes`. Then a pre-mortem: one subagent reads the slate cold and writes the 5 likeliest ways tonight fails (disk, the Editor
stalling, a row breaking an old verb, the spend limit, a pilot read as the game) and the lead adds a guard line for each in the log.

## 2. Phase B: build rounds (0:50-3:30)

- **Builders:** up to five, Opus 5.5, each in its own git worktree from `port/stage3` at the shared commit. Create worktrees ONE at a time
  and check `df` between them (NIGHT54: four at once took C: to 256 MB free). Each builder owns a set of slate entries and only their
  files. Spawn prompts follow section 5.
- **The shared commit (lead, before builders start):** place the `design.n55` rows (all off) and any hooks the slate needs, modeled on
  `Tools/night/n46-shared.cjs`; reel routing `Game/FeelReel.N55.cs`; `-skygrindOff55` / `--off55` = NIGHT54's game.
- **Each slice:** contract (the slate entry, accepted by the lead) -> RED proof -> build -> GREEN proof -> commit -> report. One slice at a
  time per builder. Commit every slice (the spend limit has stopped every agent at once before: L-003).
- **The critic gate:** after each integration build, a FRESH critic subagent (never the builder, never one that saw the builder's report)
  gets only: the slate entry's `doneWhen`, the before and after frames, the numbers. It scores each criterion 0-1 with pass / fail against
  the threshold, and is told that approving a miss is the failure it is there to prevent. The lead flips `passes: true` only on its pass.
  For looks and feel, blind A/B pairs shown in both orders; a slice GG asked for by name stays ON on a tie and the tie goes in the report.
- **Integrations and builds:** ~1:45 (`n55-r1`), ~2:40 (`n55-r2`), ~3:30 (`n55-r3`, the candidate): `bash Tools/night/n41-build.sh
  n55-rK`, rows ON in the build's copy with `node Tools/night/n46-build-rows.cjs`, reels with an `n55-reels.sh` copied from the newest
  reels script, then `Tools/night/n54-prune.sh` (a proof reel writes ~840 MB).
- **Order:** every P0 entry gets a first attempt before any P2 entry starts. An entry with two failed attempts becomes SHORT with the
  measured reason and the best attempt committed behind its row. "Not reached" is not a state.
- **Beyond (from ~2:40):** builders that finish their P0 / P1 entries take P2 entries; at least 3 P2 entries reach a Player frame tonight.

## 3. Phase C: freeze and close (3:30-4:30; the close table no earlier than 4:00 by `date`, hard cap 4:30)

- 3:30-4:00: Player fixes only; the ON calls (gates GREEN, verbs37 on levels 1-5, the critic's pass, the frames show it); `n55-rows-on`
  script in the repo; the suite batches touched by tonight's rows; the smoke with nobody at the machine (run it once more if only pad steps
  fail); tag `u3-n55`. The play build becomes `Builds/n55-freeze/` only if verbs37 and the smoke pass; otherwise it stays n54-freeze2.
- 4:00-4:30: `NIGHT55-LOG.md` close table (every slate entry: state, numbers before / after, frame path, row, how to switch it off; every
  OPEN.md line accounted for), `NIGHT55-REPORT.md` for GG in plain words (what changes for a player, the frames to look at, what is SHORT
  and why, GG's calls waiting for him), HANDOFF.md (a new top block), DECIDE55.md, `wave-memory` (THREADS consolidated), and the
  `skygrind-retro` skill. Commit with explicit paths. Nothing pushed, nothing merged.

## 4. The critic rubric (one judge, one rubric, scored 0-1, pass / fail per line)

1. The `doneWhen` numbers are met (each threshold read from a log line, not a claim).
2. The before / after frames show the change at 1280x720 and 1920x1080 to someone who was not told what changed.
3. Nothing was lost: the VERBS.md rows the slice touches still pass (verbs37 output), and `--off55` gives NIGHT54's game.
4. The look rules hold: cyan player-usable, red-orange enemies and danger, yellow parryable, magenta RIVAL, pale violet-white boundary; UI
   text at least 16 px at 1080p (bigger where NIGHT47's spec set it); no effect over the whole screen.
5. It is not "basic": it changes what a player sees or does in a way GG would notice in the first minute of the level or boss it touches.
A slice passes only if every line passes.

## 5. Spawn template (every subagent; vague briefs caused duplicated work and gaps)

```
You are <role> on NIGHT55 of SKYGRIND (Unity 6000.3.7f1, URP; a pad-first movement shooter at dusk), C:\Users\gaige\dev\SKYGRIND-unity.
Objective: <one sentence>.
Read first: CLAUDE.md, docs/PRINCIPLES.md, docs/DESIGN-RULES.md, NIGHT55.md sections 0, 4, 6, then: <files>.
Your entries: <slate ids with their doneWhen>.  Your files: <paths>; touch nothing else.
Tools and sources: <harness commands / skills / sites>.  You may not: run the Unity Editor, a build, a Player, a reel or run_tests
(builders), push, merge, or commit outside your branch.
Output: commits on your branch (explicit paths + .meta), notes in Research/2026-10-20-wave55/<id>.md, and a report under 300 words:
each entry's state, every sha, every measured number before / after, the frames the lead must take. Last line:
N55-<id>: round <r> · passes <ids> · SHORT <ids> · commits <shas>.
```
Critics and researchers are read-only and write only their one file.

## 6. Hard rules (CLAUDE.md, restated; nothing here overrides it)

- Never commit to `master`, push, merge into master or `gh auth switch`. Explicit paths plus `.meta`, never `git add -A` / `.`. Never stage
  GG's four files (ProjectSettings/UnityConnectSettings.asset, ProjectSettings/EditorBuildSettings.asset,
  Assets/ProjectSettings/UIEffectProjectSettings.asset, Assets/Scenes/Arena.unity). Restore the TMP fallback font asset before commits.
  Trailer: `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`. Night prompts stay untracked; NIGHT55-LOG.md is tracked.
- Only the lead drives the Editor, builds, Players, reels, the suite and git on `port/stage3`. `editor_focus` before Play checks; never
  recompile in Play mode; never `unity test`; one Play session or build at a time; batch builds from
  `C:\Users\gaige\dev\SKYGRIND-unity-batch` when the live Editor stalls. Close every Player by its PID.
- Movement exact: golden 0 mismatches, 22 replays exact at every Core commit with tonight's rows off; fingerprints plain / `--off54` /
  `--off55` EQUAL before each code commit. Mover.cs only where a slate entry names it, behind its row.
- Numbers from config through `GameConfig`; Unity-only numbers in UnityTuning with a comment; patches by `.cjs` scripts written with the
  Write tool. Randomness from `Mulberry32` in sim code only. No allocation per sim step.
- No audio code or sound files (GG's). New gameplay events get web-style names in docs/AUDIO-HOOKS.md.
- Any new view that copies the stone material passes vertex colors or uses `Light54.StoneOrLit` (Stone.shader ignores `_BaseColor`:
  NIGHT54's find).
- Docs with the Write / Edit tools only. Write only what was measured or read; otherwise "not checked".
- Delete only your own temporary output. Never the old worktrees in `.claude/worktrees` (GG's call).

## 7. If something goes wrong

- **Usage limit or 429:** everything is committed already. The lead continues alone in this order: integrate what is committed, build,
  the ON calls, the smoke, the close, then open slate entries in order. If the run is paused by a usage limit, it resumes from
  `Logs/n55/slate.json` and NIGHT55-LOG.md when the limit resets; re-read them first.
- **Disk under 12 GB:** prune frames; under 6 GB no new reels.
- **The Editor stalls:** batch-mode builds from the batch checkout (NIGHT46).
- **A row breaks an old verb:** turn it off, log it, make it SHORT with the reason. Never ship a lost verb (PRINCIPLES 1).

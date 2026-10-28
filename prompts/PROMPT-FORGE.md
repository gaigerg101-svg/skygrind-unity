# PROMPT-FORGE: the master prompt that writes SKYGRIND's next build wave

> **Runs through the `wave-forge` skill (2026-10-07).** The skill holds the stages: recall, diverge, research,
> converge, spike, draft, attack, close; plus its lenses, red-team and Opus 5.5 notes. Memory is `wave-memory`
> (`docs/memory/`). This file keeps SKYGRIND's own brief (section 0) and the lines every SKYGRIND wave prompt must carry
> (section 5). The first forge (wave 21, 2026-10-07) produced `NIGHT21.md` and `Research/2026-10-07-wave21/`: start the
> next one from that set's shape.

**What this is:** a session you run *before* a night. It builds nothing in the game. It learns from the nights so far,
researches, invents and ranks features, then writes the next wave prompt (`NIGHTn.md`) and its kickoff. You run the
wave afterwards, in a fresh session, with the kickoff it prints.

**Why it's a separate session:** the playbook's record (`~/.claude/skills/game-prompt-playbook/PLAYBOOK.md` §2-3) shows
two things. Waves whose research was done *before* the run were building within minutes and met their done-whens.
Waves that ran research or "creative freedom" *inside* the run overran or were stopped. So the creativity happens here,
and the wave prompt stays short and decided.

**How to run it:** a fresh Claude Code session in `C:\Users\gaige\dev\SKYGRIND-unity`, Opus 5.5, `/effort high`. Paste
section 8's `/goal`. Budget: about 60-90 minutes. It asks you nothing until the end.

---

## 0. The brief (GG, 2026-10-07; verbatim in the playbook's library)

- **The wave is a building wave:** new features that fit SKYGRIND, researched and innovative. It is not another
  balance-polish night.
- **The wave's clock:** at least 5 hours (the close table prints at 5:00 or later from the wave's first `date`) and at
  most 6 hours (the hard cap).
- **Every new feature ships behind its own switch** (a `design.adds.<row>` or a new block, off = the previous night's
  game). GG judges feel and looks by playing; the switch lets him keep or drop it without a revert.
- **Out of bounds, always** (CLAUDE.md rules 2, 3, 7): audio, new enemy *designs* (a new kind is a one-page proposal),
  web-repo edits, `Mover.cs` without a named task. Everything else is open: weapons, movement verbs, modes, map content,
  progression, UI, effects, the meta loop, onboarding, accessibility, the Steam Deck.

## 1. Re-enter from the files (fresh context: the files are the memory)

Run `date`. Then read, in this order, and note what each says that matters for the next wave:
1. `CLAUDE.md` (always true), `HANDOFF.md`'s top block (state, open defects in order).
2. The last night's `NIGHTn-LOG.md` close table and `NIGHTn-REPORT.md` (GG's page), `FEEL-PLAYTEST.md`.
3. `LEARNINGS.md` and `IDEAS.md` (created by the `skygrind-retro` skill; if they don't exist yet, step 2 creates them).
4. Any `GG-NOTES*.md`, and GG's own words in the last three night prompts' first sections (his reactions are the best
   predictor of what he keeps).
5. Queued prompts: `NIGHT21.md` (map night 2, deferred four times), `QUEUE-*.md`. They're candidates, not orders.
6. `tasks.json` (open rows), `git log --oneline -30`, the `Research/` index.
7. The playbook: `PLAYBOOK.md` §2-§4, `templates/wave-prompt.md`, `templates/kickoff.md`, `OUTCOMES.md`'s SKYGRIND rows.

## 2. Retro: learn from what was built (the `skygrind-retro` skill)

Invoke the `skygrind-retro` skill on the last night. It writes `LEARNINGS.md` (process lessons with evidence) and
`IDEAS.md` (the feature backlog), and fills the playbook's OUTCOMES row. The forge reads its output; it doesn't redo it.
**Why:** NIGHT19 and NIGHT20 both lost hours to the same causes (the spend limit with six agents, heavy jobs side by
side). A lesson that isn't written where the next prompt is built gets learned again.

## 3. Research and invent (the `skygrind-idea-lab` skill)

Invoke `skygrind-idea-lab`. It runs a 25-minute research pass (two research subagents in parallel at most, web only,
no personal data in any request), then generates and ranks ideas against SKYGRIND's pillars and GG's taste, and writes
`Research/<date>-wave-<n>.md`. Read its ranked slate before choosing.

## 4. Choose the wave (decide; don't ask)

Pick **one theme** and from the slate:
- **3-5 headline features:** each a player-visible moment GG can feel in the first 10 minutes of play, each with a
  measurable done-when on a Player build, and each behind a switch.
- **The previous night's debt as P0** (the playbook's rule): HANDOFF's open defects, at most 45 minutes. Tonight that's
  the magenta rank letter first.
- **A short beyond list:** 3-6 rows, only from `IDEAS.md` rows GG already liked or that extend a headline.
- **A NOT-this-wave list:** everything else stays in `IDEAS.md`.

If two themes tie, write the stronger as the prompt and the other as one paragraph in GG's summary. A queued prompt
that loses (say map night 2) stays in the queue: if it holds the number you need, rename it to
`QUEUE-<topic>-v<k>.md` first, content unchanged.

## 5. Write the wave prompt (playbook template, SKYGRIND specifics)

Start from `templates/wave-prompt.md` and fill every variable from the files (re-read them; never from memory). Keep it
**under about 350 lines**. Put long evidence in the research file and link to it.

What this project's wave prompts must carry. Each line comes from a measured night:
- **The clock:** the first `date`; phase boxes with `date` at each boundary. Freeze at 4:50; the close 4:50-5:45; the
  table at 5:00 or later, hard cap 6:00. NIGHT20's close took 40 minutes on a quiet machine and ran 3-5x slow on a busy
  one, so plan the close's heavy jobs one at a time.
- **Builders:** at most 3 alive, split by file ownership, each with a queue; the lead builds too. Commit every slice.
  **Why:** six builders hit the account's spend limit at ~3 h on NIGHT20 and on NIGHT19. Give a usage-stop rule (log
  the gap, keep building alone, retry at most every 30 minutes).
- **Done-whens:** measured on a Player build where the feature is player-facing (a reel scenario, a harness judge, a
  smoke step). A stand-in or a placeholder is SHORT, never DONE. "Tests verify, they don't define": never edit a test
  to pass.
- **Judges and reviewers:** reuse the existing judges (THE RANGE, THE CHASE, THE CLIMB, THE MARATHON, the reels and
  feel-metrics) wherever they measure the new thing. Each new feature gets one reel scenario. Blind reviewers **veto
  defects; they don't approve looks**: round 4's reviewers caught the magenta letter, while pairwise wins never
  predicted GG (playbook §3.4). Give each reviewer a budget: "25 minutes, about 80 tool calls" (7-9 minutes in
  practice, against 73 without one).
- **Caps** (each with "report it short instead"): builds, reels, farm batches (one at a time, never beside a bench or
  the suite), bench runs (alone, after checking `Get-Process` CPU: other sessions' jobs ran during NIGHT20's), reviewers.
- **Gates at every Core commit:** golden 0 mismatches / 22 exact, fingerprint EQUAL (FingerprintBaselineN20) and
  `--off19` EQUAL; a new block off = the previous night's fingerprint. If the wave changes the on path, it records one
  new baseline at the close with the reason.
- **Stops:** the one you want (the close table, every row DONE or SHORT with a measured reason) and the early ones you
  don't. Launch the wave with `--append-system-prompt-file PROMPT-STOPS.md`: Anthropic's Opus 5.5 "Unattended
  agentic runs" paragraph, verbatim, which names the four early stops to avoid.
- **A status line** ending every message with elapsed time against the cap (`h:mm / 6:00`). Opus 5.5 paces itself to a
  stated budget.
- **Calm wording:** say what to do and why; at most two emphasized rules; positive instructions over prohibitions.
- **The report spec:** GG's page (`NIGHTn-REPORT.md`): what he can now do in the game, per feature, with the switch's
  name and a 10-minute pad-first walk (`FEEL-PLAYTEST.md`); numbers only from that night's runs.
- **The close:** the playbook's close table; the `handoff` skill; `skygrind-retro` at the very end, so the next forge
  starts from fresh learnings.

## 6. Red-team the draft (one fresh subagent, 10 minutes)

Hand a fresh subagent the draft prompt, `LEARNINGS.md` and the playbook's §4 checklist. Ask for gaps, not style:
- Which past failure would this prompt repeat?
- Is any done-when unmeasurable, or measurable only by a judge's taste?
- Does the clock math close before 6:00?
- Can the `/goal` become impossible?
- Is any line in the prompt contradicted by `CLAUDE.md`?

Fix what it finds. Log what you rejected and why.

## 7. Deliver

1. `NIGHTn.md` (untracked, like every night prompt) and its `/goal` (end condition first, under 1,800 characters: a
   pasted goal was once cut at ~2,640).
2. The kickoff (`templates/kickoff.md` filled in) printed in the reply, with the launch line: `claude --model
   claude-opus-5-5 --append-system-prompt-file PROMPT-STOPS.md`, `/effort` as the prompt says.
3. Committed, tracked: `LEARNINGS.md`, `IDEAS.md`, `Research/<date>-wave-<n>.md`, and a `tasks.json` row set for the
   wave (written before any code, as every night).
4. The playbook: the library entry's Outcome ("forge ran; wave prompt written"), and a note in `PLAYBOOK.md` if this
   forge taught something.
5. **The reply to GG** (short): the theme in one sentence; the headline features, each with its switch and the moment
   he'll feel; what was cut and why; anything only GG decides, such as installing a plugin or picking between two themes.

## 8. The /goal to paste

```text
/goal Met when PROMPT-FORGE.md's step 7 is done: NIGHTn.md written (red-teamed, under ~350 lines, a 5-6 h wave of new
features behind switches), LEARNINGS.md / IDEAS.md / the research file committed, and the kickoff printed; or 100
minutes after the first `date`, with what's missing listed. Work in C:\Users\gaige\dev\SKYGRIND-unity: run `date`, then
follow PROMPT-FORGE.md steps 1-7 in order, using the skygrind-retro, skygrind-idea-lab and game-prompt-playbook skills.
Build nothing in the game; never push, merge or commit to master; the web repo is read only. Ask nothing until the
reply; log choices with the rejected alternative.
```

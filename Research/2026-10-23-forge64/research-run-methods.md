# FORGE64 research: run methods for a 3-hour autonomous night (2026-10-23)

Web research, about 15 min. In the repo I read only NIGHT63.md and Research/2026-10-22-forge63/redteam.md. Each claim cites [Sn]; the URLs are at the
end. "(excerpt)" = seen only in a search-result summary, not fetched. "Not verified" = could not confirm. Section 2's timings are estimates; nothing ran.

## 1. Best practice for long-running autonomous coding agents (2025-2026)
**Progress files, init script, one feature at a time.**
- An initializer writes a JSON feature list (all failing, a `passes` field), `claude-progress.txt`, `init.sh` and a first commit; later sessions
  only flip `passes` ("It is unacceptable to remove or edit tests"); JSON because the model overwrites it less than Markdown [S1].
- Every session's bearings: pwd, git log, progress notes, pick the top unfinished feature, run init.sh and an end-to-end check *before* new work,
  then one feature, a commit and a notes update. Failure modes: done too early, a broken environment, unverified "passes", time lost working out
  how to run the app [S1].
- Notes outside the context (NOTES.md, to-do files) suit "iterative development with clear milestones" [S4]. The C-compiler agents kept progress
  files and a doc of failed approaches [S6]. OpenAI: a docs directory (maps, execution plans, specs) is the agents' source of truth, enforced by
  linters / CI [S21]; the primary post returned 403, so its details are not verified.
**Verification before "done".**
- "Claude stops when the work looks done. Without a check it can run, 'looks done' is the only signal available." Show evidence (command and
  output); gate the stop with /goal, a Stop hook or a verification subagent so "the agent doing the work isn't the one grading it" [S7].
- End-to-end test tools "dramatically improved performance" [S1]. The verifier must be "nearly perfect, otherwise Claude will solve the wrong
  problem"; a CI gate was added once new work kept breaking old work [S6]. Stop conditions and ground truth at each step [S5]. /goal's
  evaluator runs no commands and judges only the transcript; a time clause bounds it [S11].
**Surviving a stop or a usage limit.**
- Claude Code 2.1.234 or later, interactive, claude.ai subscription: after a usage limit it waits in the open session and continues at the reset,
  on by default (`autoContinueAtUsageLimit`) [S10].
- Its limits [S10]: it still asks for permissions ("the task can stop on a prompt while you're away"); it re-arms at most twice in a row; it does
  not resend the last message; exiting or /clear ends the wait; it doesn't start on its own for a reset over 24 h away or in agent-team teammate
  sessions; background sessions and `-p` runs never get it.
- A /goal shows "Goal paused" at a limit and resumes with the auto-continue; --continue / --resume restore it [S11].
- Hooks [S9]: `SessionStart` (startup | resume | clear | compact | fork) can inject `additionalContext` (as facts, not commands); `StopFailure`
  fires on API-error turn ends (matcher `rate_limit`), for logging only; `PreCompact` can block compaction.
- Subagents resume by `SendMessage` with full history and survive a restart of the same session; one you stopped doesn't auto-resume [S8].
  /resume doesn't restore in-process agent-team teammates [S14]. Whether a background subagent continues after a reset: not verified.
- Pro / Max: a five-hour session limit plus a weekly limit, shared by Claude and Claude Code; a cut-off step may finish as "wrapping up" [S16].
  The status line gets `rate_limits.five_hour.used_percentage` / `.resets_at` (and the seven-day pair) [S13]. Anthropic's research system
  resumes "from where the agent was when the errors occurred", with retries, regular checkpoints and the plan in external memory [S3].
**Sizing work to a time box.**
- METR: Claude 3.7 Sonnet's 50 % horizon was 50-59 min, doubling about every 7 months; the 80 % horizon is about 5x shorter (15 vs 59 min);
  success under 10 % on tasks over about 4 h [S19]. The 80 % estimates depend on modelling choices [S20, excerpt]. Size every slice that must
  land well under the 50 % horizon: reliability, not capability, is the limit.
- Agents can't tell time: the C-compiler harness prints little, logs to files and offers a `--fast` 1 % / 10 % sample [S6]. Teams: a task too
  large means teammates "work too long without check-ins"; "three focused teammates often outperform five scattered ones" [S14].
- A 3 h 50 min harness run (Opus 4.6, $124.70): build rounds 2 h 07 and 1 h 02, the planner $0.46, QA about $3-4 a round; per-sprint review
  became one pass at the end once the model could build alone [S2].
**How many parallel agents pay off.**
- Multi-agent runs use about 15x chat's tokens (one agent about 4x). They pay off on parallel, high-value work; coding has "fewer truly
  parallelizable tasks than research" [S3].
- "Actions carry implicit decisions, and conflicting decisions carry bad results" [S18]. With 16 agents on one task, "every agent would hit the
  same bug ... and then overwrite each other's changes" until an oracle split the work [S6]. Start with 3-5 teammates; tokens scale about
  linearly with team size; never two agents on one file [S14][S12].
**Keeping the reviewer independent.**
- "Agents reliably skew positive when grading their own work"; a standalone skeptical evaluator is "far more tractable". Builder and evaluator
  agreed a sprint contract (what done looks like and how it is checked) before any code [S2].
- A fresh-subagent reviewer "sees only the diff and the criteria you give it, not the reasoning"; tell it to flag only correctness / requirement
  gaps [S7]. LLM judges prefer their own outputs, in step with self-recognition [S25]; one call scoring 0.0-1.0 plus pass/fail was the most
  consistent judge [S3]. Code review: a new push dismisses a stale approval ("Dismiss stale pull request approvals when new commits are
  pushed") [S24].
- Images: over 20 in one API request bring a stricter per-image size limit, and earlier turns' images count because they are resent; keep 20 or
  fewer, or each at most 2000 px a side; the cap is 100 per request on 200k-context models [S17]. Which limit refused gg-proxy's reads on
  NIGHT63: not verified.

## 2. Scheduling a 3-hour run so the verification and the close survive an interruption
- Keep the software deployable ahead of new features [S22]; ship unfinished work dark behind toggles and test both states [S23]; each timebox
  gives a usable increment [S30, excerpt].
- NIGHT63 did the opposite: no close before 7:00, and the freeze build, five reels, the smoke and the flow gate were all left for 7:00-8:00
  (NIGHT63.md section 0 rule 1, section 9). The 05:23-07:41 stop then took the whole close (commit c1b2ec43).
- Checkpoint interval, roughly: Young's sqrt(2·C·M) [S29, excerpt]. C (one full verification) is about 15-20 min (NIGHT63's boxes: a build about
  2, verbs37 x5 about 5, the flow gate about 7); M (time between interruptions) is a few hours, not measured. That gives one verified checkpoint
  every 1.2-1.7 h: at the start, mid-run and before T-45.

| when | what |
|---|---|
| 0:00-0:15 | bearings by init script; read the usage window (rule 5); start the rc0 build from the start sha (background) |
| 0:15-0:45 | rc0 verified (verbs37 x5, smoke, flow) while builders start; HANDOFF stub; tag rc0. **The minimum close now works.** |
| 0:45-1:40 | slices and picks; builds on demand with fast checks (verbs37 x1 plus the row's proof) |
| ~1:40 | rc1: full verification of the rows that passed; re-point the RC |
| 1:40-2:10 | last picks (picks freeze at 2:10); the last critic |
| 2:10-2:30 | rc2 build and full verification (background); if it fails, rc1 ships |
| 2:30-2:45 | report, handoff, harvest, tag; the minimum close is done by 2:45 |
| 2:45-3:00 | slack for an overrun, a limit stop or a deaf Editor |

- **Minimum viable close (MVC)**, scripted, about 10 min, the lead alone, no agent or Player: tag the last verified RC, fill HANDOFF from its
  verify table, log Blocked, stop the clock, commit. Run it at T-45, or sooner if the usage window will run out before the cap or the Editor is
  deaf twice. "Never close before 7:00" becomes "never later than T-45; leftovers only after the MVC".

## 3. The handoff: what the next session needs on arrival
- I-PASS (Illness severity, Patient summary, Action list, Situation awareness and contingency, Synthesis by receiver), as a bundle with training
  and tools in 9 hospitals: medical errors 24.5 -> 18.8 per 100 admissions (-23 %), preventable adverse events 4.7 -> 3.3 (-30 %) [S26, excerpt].
- SRE: read the last shift's handoff at the start, send one at the end, and hand off incident command explicitly with an acknowledgment [S27,
  excerpt]. A reset handoff carries "the previous agent's state and the next steps" [S2]. Amp's handoff (a goal, a drafted prompt, the relevant
  files) replaces lossy compaction [S28]. Progress file plus git log [S1].
- **HANDOFF sections, in the I-PASS shape:** (I) shippable? the anchor: tag, sha, build folder, verification time; (P) each row ON / OFF / SHORT
  with one evidence path; (A) the next three actions, with files; (S) contingencies ("if the Editor is deaf / disk under 12 GB / the window runs
  out, then ..."), the D-ids for GG, calls made with reasons; the invisible state (GG's uncommitted files and the font drift, `Logs/clock.json`,
  live branches and worktrees, untracked prompts, PIDs at close, disk and commit free, the usage reset time, anything only in the lead's context);
  (S) a verify table (command | exact expected output | last actual | when) that the next session runs first, logging match or mismatch.
  Write it at 0:45, update it at each RC; the close only finalizes it.

## 4. Fourteen rules for the 3-hour prompt: (a) the team now, (b) add or change, (c) source
1. **RC first.** (a) Baseline gates at 0:00-0:40, the freeze build only at the close (NIGHT63 section 1.4, section 9). (b) Build and fully
   verify rc0 from the start sha by 0:45; tag `u3-n64-rc0`. (c) [S22] [S1]
2. **Rolling RCs, fast vs full checks.** (a) Builds on demand; verbs37 x5 on every critic build. (b) A pick gets verbs37 x1 plus its proof; the
   full set only on the RC builds (about 1:40, 2:10); `Logs/n64/rc.json` holds the last verified sha. (c) [S6] [S29]
3. **A scripted MVC at T-45.** (a) A 7:00-8:00 close of about 45 min, never before 7:00. (b) `n64-mvc.sh`, 10 min or less, no agent or Player,
   done by 2:45; the full close is a bonus. (c) [S1] [S3]
4. **Auto-continue armed.** (a) Commit every step, resume by SendMessage (section 0 rule 9, section 7); whether auto-continue was on: not checked.
   (b) Bearings print the version (2.1.234 or later) and `autoContinueAtUsageLimit`; allowlist every night command; the lead is interactive, not
   `-p`; /goal stays set. (c) [S10] [S11]
5. **Budget the usage window like the disk.** (a) Clock, disk and commit gates only. (b) A status-line script writes `rate_limits.five_hour` to
   `Logs/n64/usage.json` and the clock prints it. Start only under about 30 % used, or with the reset inside the run (the forge burns the same
   window); if 100 % is projected before 2:45, go to one builder and run the MVC. (c) [S13] [S16] [S12]
6. **Resume from disk by hook.** (a) Bearings 1-3 by hand. (b) A `SessionStart` hook (startup | resume | compact) injects the clock, the slate
   summary, the RC sha and the log's last `NEXT:` line; a `StopFailure` hook on `rate_limit` logs the time. (c) [S9] [S1] [S4]
7. **At most four agents.** (a) Three builders plus two slots: five (section 0 rule 5). (b) Two builders on disjoint files and one review slot used
   in turn (spotter, critic, gg-proxy); a third builder only if under 50 % of the window is used at 0:45. (c) [S14] [S3] [S18] [S6]
8. **A slice reaches its first pick in 45 min or less.** (a) The forge sizes slices; the red team put B1 at 3:00-3:30. (b) A slice not picked
   by 1:45 is SHORT; split anything bigger. (c) [S19] [S14]
9. **One feature at a time, contract first.** (a) Test first; the slate JSON; doneWhen never loosened. (b) The builder writes its proof command
   and expected output before any code, and the lead checks them against doneWhen. (c) [S2] [S1]
10. **A PASS binds to a sha.** (a) A PASS flips the row; NIGHT63 shipped a post-PASS fix unreviewed and it regressed. (b) The slate stores
    `passSha` and the row's files; the guard refuses a flip or an RC tag while `git diff passSha..HEAD -- <files>` is non-empty. Re-review, or
    ship passSha. (c) [S24] [S7]
11. **The reviewer sees evidence, not the story.** (a) A blind spotter before the critic; one-row briefs. (b) The brief holds the doneWhen, the
    frames, the meter outputs and the diff; no builder notes or lead commentary; "flag only gaps against doneWhen". (c) [S7] [S2] [S25] [S3]
12. **An image budget.** (a) gg-proxy saw 36 frames; then its reads were refused at a request limit. (b) At most 20 images per reviewer; four
    frames per contact sheet, each sheet at most 2000 px a side; numbers first; split big rankings across fresh agents. (c) [S17]
13. **One heavy job, behind a memory guard.** (a) One Player, a commit-free check, everything long in the background. (b) Before each heavy job,
    also check other Claude or Unity processes; re-run a memory-killed job once, then log it. `CLAUDE_CODE_TOOL_MEMORY_LIMIT` is Linux / WSL
    only, so on Windows the guard has to do this. (c) [S15]
14. **Write the handoff continuously, in the I-PASS shape.** (a) HANDOFF is rewritten at the close. (b) A stub at 0:45 with the anchor and the
    verify table, updated at each RC; the next session runs the verify table first. (c) [S26] [S27] [S2] [S28]

## Sources
- [S1] https://www.anthropic.com/engineering/effective-harnesses-for-long-running-agents · [S2] https://www.anthropic.com/engineering/harness-design-long-running-apps
- [S3] https://www.anthropic.com/engineering/multi-agent-research-system · [S4] https://www.anthropic.com/engineering/effective-context-engineering-for-ai-agents
- [S5] https://www.anthropic.com/engineering/building-effective-agents · [S6] https://www.anthropic.com/engineering/building-c-compiler
- [S7] https://code.claude.com/docs/en/best-practices · [S8] https://code.claude.com/docs/en/sub-agents · [S9] https://code.claude.com/docs/en/hooks
- [S10] https://code.claude.com/docs/en/interactive-mode ("Wait for a usage limit to reset") · [S11] https://code.claude.com/docs/en/goal
- [S12] https://code.claude.com/docs/en/costs · [S13] https://code.claude.com/docs/en/statusline · [S14] https://code.claude.com/docs/en/agent-teams
- [S15] https://code.claude.com/docs/en/whats-new/2026-w33 · [S16] https://support.claude.com/en/articles/11145838-use-claude-code-with-your-pro-or-max-plan
- [S17] https://platform.claude.com/docs/en/build-with-claude/vision · [S18] https://cognition.com/blog/dont-build-multi-agents
- [S19] https://metr.org/blog/2025-03-19-measuring-ai-ability-to-complete-long-tasks/ and https://arxiv.org/html/2503.14499v1 (the 80 % figure)
- [S20] https://metr.org/notes/2026-03-20-impact-of-modelling-assumptions-on-time-horizon-results/ (excerpt)
- [S21] https://www.infoq.com/news/2026/02/openai-harness-engineering-codex (primary https://openai.com/index/harness-engineering/: 403, not verified)
- [S22] https://martinfowler.com/bliki/ContinuousDelivery.html · [S23] https://martinfowler.com/articles/feature-toggles.html · [S25] https://arxiv.org/abs/2404.13076
- [S24] https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches/about-protected-branches
- [S26] https://pubmed.ncbi.nlm.nih.gov/25372088/ (Starmer et al., NEJM 2014; excerpt; nejm.org returned 403) · [S28] https://ampcode.com/news/handoff
- [S27] https://sre.google/workbook/on-call/ and https://sre.google/sre-book/managing-incidents/ (excerpt)
- [S29] https://arxiv.org/pdf/1302.3752 (Young / Daly; excerpt) · [S30] https://scrumguides.org/scrum-guide-2017.html (excerpt)

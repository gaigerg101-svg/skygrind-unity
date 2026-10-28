# Running a 7-hour /goal night well on Opus 5.5 (FORGE65; a claude-code-guide agent on the official docs, 2026-10-23 ~13:15)

Sources: code.claude.com/docs/en/{goal, model-config, context-window, sub-agents, worktrees, workflows, errors, interactive-mode, costs, headless,
permission-modes, hooks-guide, best-practices}; platform.claude.com/docs/en/build-with-claude/{effort, prompt-engineering/prompting-claude-opus-5-5,
prompting-claude-opus-5, claude-prompting-best-practices}. "Not documented" where the docs are silent. The lead's NIGHT64 experience is
marked (w64).

## 1. /goal (the stop condition)
- A session-scoped Stop hook judged by a small, fast model after each turn, from the transcript alone: it runs no tools and reads no files,
  so every check must be **printed** in the conversation. Up to 4,000 characters. Best: one measurable end state, the checks named with
  their expected output, constraints, and a bound (the docs' example: "or stop after N turns ... list what is still open").
- Pitfalls: an unverifiable condition is never met (or judged impossible and cleared); several turns in a row with no tool use stop the
  loop; `/clear` removes the goal; a new `/goal` replaces it. While a background shell or subagent runs at a turn's end, evaluation waits;
  check-ins after 30 min, 1 h, then every 2 h (w64: the check-ins arrived and were answered).
- A wall-clock bound is not a documented clause: print the night's own clock (n65-clock.cjs) and name its line in the condition (w64 did).
- Not documented: whether a goal survives compaction. So the goal tells the session to re-read the night's plan after any compaction.

## 2. Effort
- Opus 5.5's default is medium; high where verification matters; xhigh for deeper reasoning at higher spend ("tasks over 30 minutes" in
  the platform table); max shows diminishing returns. Effort is not a budget; Opus 5.5 thinks more per turn at xhigh than Opus 5 did.
- GG set xhigh (2026-10-23). The lead's advice: keep xhigh for the lead session (planning, judging, integrating); NIGHT63's 8-hour run hit
  the usage limit at 5:23, so the plan keeps a verified RC at all times and a minimum close; if limits bite, high is the lever.

## 3. Compaction
- Auto-compaction near 967K tokens on 1M-context models. Survives: the system prompt, the project-root CLAUDE.md, auto memory, up to five
  recently read / edited files, skill bodies (capped). Lost: the rest is summarized.
- Robustness: state on disk (NIGHT65-LOG.md, Logs/n65/slate.json, rc.json, commits), the goal's "re-read NIGHT65.md after any compaction",
  and every agent's id written in the log (SendMessage resumes them).

## 4. Subagents and worktrees
- Background by default; a fresh subagent starts with only its prompt: put the task, paths, constraints and the report format in it.
- **A subagent in .claude/worktrees/ does not load CLAUDE.md** from its worktree: the builder brief must say "read CLAUDE.md first" (it does).
- Worktrees branch from the default branch unless `worktree.baseRef` is "head": builders branch explicitly from the sha the lead sends (w64
  did; it worked).
- Opus 5 guidance: delegate only large, independent, parallel work; do not use subagents to re-verify your own work (the night's critics
  are independent judges with their own evidence, which is the exception the night needs).

## 5. Usage limits
- An interactive session on a claude.ai subscription **auto-continues** after a usage-limit reset (autoContinueAtUsageLimit, default on;
  re-arms at most twice in a row); `-p` and background sessions do not wait. Run the night interactively.

## 6. Prompting Opus 5.5 for long agentic work
- Give it checks it can run and read; ask for evidence (the command and its output), not assertions; drop "verify again" wording (it causes
  over-verification); constrain scope; ask for plain reports of what failed; treat a text-only end of turn as a report, not proof of done;
  plan first, then execute from the plan file; commit each item so a stop loses at most one.
- Hooks: a Stop hook is overridden after eight consecutive blocks with no tool call between them.

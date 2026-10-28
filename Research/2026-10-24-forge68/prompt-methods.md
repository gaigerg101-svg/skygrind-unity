# Prompt methods for an 8-hour Opus 5.5 night (FORGE68 prompt-method scout, 2026-10-24)

Question: the current best methods to get the best results from a long, multi-hour agentic prompt on Claude Opus 5.5 in Claude Code.
Labels: [A#] Anthropic official doc or engineering post; [R#] research preprint (peer review not checked); [C#] community, unverified;
[L] read or measured on this machine in this session. URLs are in the source list at the end. Grounding: Google Drive and Linear were
searched (nothing relevant); the local note `~/.claude/skills/wave-forge/opus-5-5.md` (snapshot 2026-10-07) was read, not edited.

## 1. What changed or is newly documented since the 2026-10-07 snapshot

**Effort and thinking**
- Opus 5.5 defaults to `medium`; thinking is always on (`disabled` returns a 400), so effort is the one depth control [A2, A3]. At one
  level Opus 5.5 thinks more per turn than Opus 5, most at `xhigh` / `max`: "Reserve `xhigh` and `max` for work where you've measured a
  quality gain"; at `medium` it matched or beat Opus 5 at `high` on coding in Anthropic's tests [A1]. The generic effort table still lists
  `xhigh` for "Long-running agentic and coding tasks (over 30 minutes) with token budgets in the millions" [A2].
- **Correction to the snapshot:** in Claude Code on Opus 5.5 (API key or subscription) changing effort mid-session keeps the cache [A14].
  A top-level `effortLevel` in user settings "doesn't count for Opus 5.5"; `modelSettings`, `CLAUDE_CODE_EFFORT_LEVEL`, `--effort`,
  `/effort` do [A11]. [L] This machine's `modelSettings` sets `claude-opus-5-5` to `xhigh` (the lead runs xhigh); the project's critic,
  reviewer and gg-proxy agents carry `effort: high` in frontmatter.
- New per-agent control: the Agent tool takes an `effort` parameter (v2.1.292) [A20]; skill / subagent frontmatter `effort` overrides the
  session level but not the env var [A10, A11]. `ultrathink` deepens one turn; "think", "think hard" are not keywords [A11]. `ultracode`
  (new) has Claude plan dynamic workflows for every substantive task, at higher usage [A16].
- Thinking-instruction removal still holds, now with a named refusal: prompts that push the model to reproduce its reasoning in the reply
  "might be declined with the `reasoning_extraction` category" [A1]. The chat line "treat that answer as done" is not for agentic tasks
  "where a later step can reveal a mistake in an earlier one" [A1].
- Task budgets (an advisory token countdown) exist in the API but "are not supported on Claude Code" [A4]; pacing stays the clock line.

**Unattended runs and /goal**
- The guide now gives the harness recipe: treat a text-only end of turn "as a report rather than as proof the task is done"; keep the parts
  in a checklist file; have "a separate, smaller model check the conversation" against the stated condition; stop after two or three
  automatic continuations of one task; wait for running background work. The four-stop paragraph goes "at the end of your system prompt
  from the first request" [A1].
- `/goal` is that recipe: a session prompt Stop hook whose small fast model gets the condition and the conversation, "doesn't run commands
  or read files", and returns not met / met / impossible [A8]. Condition up to 4,000 characters; add a turn or time clause; several turns
  with no tool use stop the loop with the goal still set [A8]. Stop hooks are overridden after 8 blocks in a row
  (`CLAUDE_CODE_STOP_HOOK_BLOCK_CAP`) [A9]; v2.1.294 judges Stop prompt hooks "written as instructions" better [A20].
- Background work defers evaluation; a check-in comes after 30 min of waiting, then at doubling intervals capped at 4x
  (`CLAUDE_CODE_GOAL_CHECKIN_MINUTES`), at most three idle check-ins per goal between prompts [A8]. A usage limit pauses the goal and work
  resumes when the session's automatic continue fires; auth, credit, unclearable context-overflow and model-unavailable errors clear it;
  a resume restores the goal but resets its turn count, timer and token baseline [A8].

**Usage limits**
- In an open interactive session on a claude.ai subscription Claude Code waits and continues after the reset (v2.1.234+); it re-arms at
  most twice in a row, and the wait does not restart after an exit [A15]. Workflows pause at the limit (v2.1.271+), not in `-p`, background
  or teammate sessions; teammate sessions never start the wait on their own [A15, A16]. [L] NIGHT67's stop read "individual spend limit
  ... resets 3:40pm" and work resumed at 5:04 (= the reset); whether auto-continue covers spend limits too is undocumented: not checked.

**Compaction and memory**
- 1M-window models auto-compact "at about 967K tokens by default"; `/autocompact`, `autoCompactWindow`, `CLAUDE_CODE_AUTO_COMPACT_WINDOW`
  (100K-1M) move it; subagents got their own `autoCompactWindow` frontmatter (v2.1.296) [A11, A20].
- **Idle compaction (v2.1.299, 2026-10-14):** a 200K+ conversation idle about 54 min is compacted before its one-hour cache expires;
  `"idleCompaction": false` turns it off [A20, A21]. A lead waiting on background builders or on a usage reset is idle.
- What survives compaction [A12]: project-root CLAUDE.md and auto memory are re-read from disk; up to five recently modified files come
  back; invoked skill bodies return capped at 5,000 tokens each and 25,000 total, oldest dropped; background tasks keep running;
  `SessionStart` hooks with matcher `compact` add their output. Text "given only in conversation" is summarized away [A13]: **a night
  prompt pasted or read as a message is not re-injected.**
- v2.1.293 fixed Claude "treating its own last actions before a context compaction as done after it, and retracting or redoing finished
  work"; since v2.1.299 `SendMessage` names "given before the last compaction still need the agent ID" [A20].
- [L] The n66-n67 lead transcript (session b545c660) has 4 auto compactions at 763K-971K tokens, summaries of 5.7K-35.7K tokens, 51-74 s
  each; one fired at 763K, under the threshold (cause not checked; idle compaction fits). Installed Claude Code is 2.1.299, so 2.1.304's
  "compaction to send Claude the skill list again" and 2.1.305's workflow-resume fix are not on this machine [A20].
- CLAUDE.md: under 200 lines; contradictions resolve arbitrarily; mid-session edits apply only after `/clear`, `/compact` or a restart
  [A13, A14]. New: `/doctor prompt-audit` reports instructions "written for older models", dead references and conflicts [A13].

**Subagents, teams, workflows, advisor**
- Subagents start in the background; fork mode is on by default and while on the lead "can't wait for a subagent" in interactive sessions
  [A10, A21]. A subagent gets CLAUDE.md, git status and preloaded skills, never the conversation: "If a rule must ... restate it in the
  prompt you give Claude when delegating" [A10]. `maxTurns` returns a resumable partial; `SendMessage` resumes with full history; the stall
  timeout is 10 min (`CLAUDE_ASYNC_AGENT_STALL_TIMEOUT_MS`) [A10, A21]. Subagent requests use a 5-minute cache even on a subscription
  unless `subagentPromptCacheTtl` is `1h` [A14]: a builder resumed after a long sim re-reads its context uncached.
- The Opus 5 guide "remain[s] a reasonable starting point" for 5.5 [A1]: Opus 5 "delegates to subagents more readily", and Anthropic's
  damping text says not to "use subagents to verify or double-check your own work" [A6]. Agent teams stay experimental and are not
  restored on resume [A17].
- Dynamic workflows hold the plan in a script, resume within a session, and run adversarial cross-checks [A16]. The advisor tool
  (experimental, Anthropic API) lets an Opus 5.5 main consult an Opus or Fable advisor that reads the whole conversation, typically before
  committing to an approach, when stuck and before declaring done; it counts toward usage [A18].

## 2. Ten testable rules for the night prompt

1. **One end condition, judged from one printed line, said three times.** The /goal names one script's last line plus a time clause (its
   evaluator reads only the transcript [A8]); the prompt's opening lines say the same; the final paragraph repeats it beside the stop text.
   Test: GOAL.txt under 4,000 characters names exactly one verdict line, and the close script prints that line last.
2. **Survive compaction by mechanism, not by "re-read this file".** A `SessionStart` hook (matcher `compact`) prints a CORE block of at
   most 60 lines: end condition, rules at a glance, clock, state-file path, live agent IDs [A9, A12]; rules that must persist all night
   live in CLAUDE.md, which is re-read from disk [A13]. Test: after every compact_boundary the hook text is in the transcript and the next
   tool call reads the state file.
3. **State in a JSON file the lead updates; the log stays prose.** Rows: slice, owner, agent ID, status, evidence path, commit ("Use
   structured formats for state data" [A5]; JSON is "less likely" to be "inappropriately" changed than Markdown [A24]); agent IDs live
   there because names die at compaction [A20]. Commit every slice; on "Usage limit reset · continuing automatically" [A15] the first acts
   are the clock, `git log -1`, the state file. Test: every resume line in the log names the state file before any new work.
4. **Every rule carries its reason; every hard rule is a hook.** "Claude can generalize from the explanation" [A5]; instructions are
   advisory, hooks deterministic [A13, A19]; "CRITICAL: You MUST" overtriggers on current models [A5]; "If you emphasize many lines, none
   of them stands out" [A19]. Test: a grep finds no NEVER / MUST without a reason clause or a named hook.
5. **Data first, the task last, the stop paragraph at the very end.** Long material above the instructions improves answers "by up to 30
   percent" on multi-document inputs [A5]; the unattended-run paragraph sits at the end from the first request [A1], adapted to name this
   night's wanted stops (guard refusal, disk floor, a GG-only decision). Test: section order; the four-stop paragraph is the last block.
6. **Plain sentences and a glossary.** Long runs grow private shorthand; Anthropic asks for final summaries "for a reader who didn't see any
   of that", no arrow chains or made-up labels [A7, written for Fable 5, untested on 5.5]; match written reports' length to the need
   [A6]. Define each coined label (GATE, MAIN, r1 ...) once. Test: a fresh subagent given only the prompt answers five questions (what
   ends the night, who owns which files, what to do at a usage stop, what a GATE is, when to stop a builder).
7. **Examples where format or judgment matter; rules where scope matters.** 3-5 diverse examples in `<example>` tags are "one of the most
   reliable ways to steer" format [A5]; positive examples beat "what not to do" for communication style [A6]; a critic calibrated with
   scored few-shot examples drifted less [A23]; canonical examples beat a "laundry list of edge cases" [A25]; one evocative phrase
   ("museum quality") made outputs converge [A23]. Test: the critic brief holds one past PASS and one past FAIL with their evidence lines.
8. **Pace with time, cap with hooks.** A line `elapsed Ns / budget Ns` at the end of each message lets Opus 5.5 pace and parallelize; the
   budget is advisory, set a little above the target, and under time pressure the model "might search and verify a little less" [A1];
   unaided agents are time-blind [A27]. Test: every clock / hook print carries the line; slices land within 15 min of plan.
9. **Briefs are contracts.** Objective, output format and file path, tools and sources, owned files and what not to touch [A26], the
   verdict line to print, done-when, the CLAUDE.md rules restated [A10]; return a file path plus a 1-2K-token summary [A25] (files avoid
   the "game of telephone" [A26]); set `effort` per role [A10, A20]. Test: a script checks each brief for the seven parts before spawning.
10. **Verification external and specific; claims audited.** Delete generic "double-check / verify with a subagent" lines (over-verification
   on Opus 5 [A6]); keep fresh-context critics only where the model is not reliable alone, judging files and Player output against hard
   thresholds [A23]. An in-band judge still accepted cycles that were 44 % real regressions when success lived outside the transcript
   [R1]. Add "Before reporting progress, audit each claim against a tool result from this session" [A7: "nearly eliminated fabricated
   status reports" on Fable 5; untested on 5.5]. Test: every DONE row in the state file cites a file and a sha.

## 3. Anti-patterns to delete

- "Think step by step", "write out your reasoning", "think carefully first": effort is the control; reasoning-in-reply can be refused [A1].
- "Double-check your work", "re-verify before responding", "use a subagent to verify": cost without quality on Opus 5 [A6].
- Walls of CRITICAL / MUST / NEVER and "if in doubt, use X": overtriggering [A5]; many emphasized lines cancel out [A19].
- Edge-case laundry lists in place of examples [A25]; adjective bans ("avoid a generic look") in place of named patterns [A1].
- "Only report high-severity issues" to a reviewer: followed literally, so it reports less; ask for everything, filter after [A6].
- A compaction defence that is only a sentence ("re-read this file after compaction"): conversation-only text is summarized [A13].
- A remaining-token or context countdown shown to the model: it can "suggest a new session ... or trim its own work" [A7, Fable 5]; show
  time, not tokens.
- Turn endings that offer, ask or promise ("Want me to...", "I'll now run X") while work is owed: name them as unwanted [A1, A7].
- Leaning on API-only controls inside Claude Code: task budgets [A4]; the per-message effort beta is API-side [A2].
- Unscoped "investigate X" in the lead's own context [A19]; raw logs into context instead of a few lines, details in files, and errors
  tagged `ERROR` with the reason on the same line [A27].
- Effort carried over by habit: Anthropic says run a sweep; 5.5 at `xhigh` is not 5 at `xhigh` [A1, A2].
- Old anti-laziness text ("be thorough", "use tools aggressively"): current models overtrigger on it [A5].

## 4. Three "go beyond" experiments (the forge picks one)

**E1. Compaction-proof core (state JSON + SessionStart(compact) hook + an earlier compact window).** Hypothesis: the lead loses rules and
in-flight state at its about four nightly compactions (971K to 5.7-35.7K tokens [L]) and spends each window's last hours in a near-full
context, where recall degrades [A19, A25]; a hook re-injecting a 60-line CORE plus the state JSON, with the lead's window at 400-500K,
re-orients it faster and cleaner. Build: a SessionStart matcher `compact` that runs a `core.cjs` printing CORE, the state rows and agent
IDs (a settings change: GG's call); `/autocompact 500k` at t0. Measure next night: compactions (compact_boundary count), minutes from each
compaction to the next productive tool call, guard refusals and rule slips in the 30 min after vs before, builders resumed by ID with no
search. Baseline: n67's transcript, counted the same way. Cost: more compactions at 51-74 s each.

**E2. Grounded-claims Stop hook (an out-of-band mid-run self-audit).** Hypothesis: status and report lines overclaim when success lives
outside the transcript (the R1 agent "claimed improvement in every cycle" while 56 % had zero or negative measured change); a command Stop
hook that checks the state JSON (each DONE / PASS row points at an existing file, a sha in `git log`, and the verdict line inside that
file) and blocks the turn end with the unbacked rows drives unbacked claims to zero. Build: a command hook, not the experimental agent type
[A9]; honour `stop_hook_active`; at most 2 blocks per turn under the 8-block cap [A9]. Measure: unbacked rows caught per night, critic
overturns of lead claims, report claims without an evidence pointer (scripted count; n67's report as baseline).

**E3. Effort by role against the usage stop.** Hypothesis: the largest measured loss is the stop (n67: 2:29 to 5:04 [L]; NIGHT67.md puts
NIGHT66's windows at about 2:20 of work); builders' long sim loops at the session's `xhigh` burn usage the lead's decisions need. Lead
`xhigh`, builders `high` via the Agent tool's `effort` [A20], critics `high`, spotters `medium` moves the first stop later with the same
MAIN results ([A1]: 5.5 at `medium` matched 5 at `high` on coding; [C1] reports medium plus capped subagents as practice, unverified).
Measure: clock time of the first stop, the `/usage` breakdown at it, slices committed per block, critic PASS rate vs n66-n67. One night
is one sample: log it as such.

Also considered: the advisor tool at verdict points (experimental; adds usage) [A18]; a dynamic workflow for batched critics with
adversarial cross-checks (pauses at usage limits) [A16]; `subagentPromptCacheTtl: 1h` for builders resumed after long sims [A14] (its
effect on subscription usage is undocumented: not checked); updating Claude Code past 2.1.299 before the night (GG's call) [A20].

## Sources
- A1 platform.claude.com/docs/en/build-with-claude/prompt-engineering/prompting-claude-opus-5-5 · A2 .../effort
- A3 platform.claude.com/docs/en/models/opus-5-5/whats-new-opus-5-5 · A4 platform.claude.com/docs/en/build-with-claude/task-budgets
- A5 platform.claude.com/docs/en/build-with-claude/prompt-engineering/claude-prompting-best-practices
- A6 .../prompt-engineering/prompting-claude-opus-5 · A7 .../prompt-engineering/prompting-claude-fable-5 (Fable 5 guidance)
- A8 code.claude.com/docs/en/goal · A9 code.claude.com/docs/en/hooks-guide · A10 code.claude.com/docs/en/sub-agents
- A11 code.claude.com/docs/en/model-config · A12 code.claude.com/docs/en/context-window · A13 code.claude.com/docs/en/memory
- A14 code.claude.com/docs/en/prompt-caching · A15 code.claude.com/docs/en/interactive-mode · A16 code.claude.com/docs/en/workflows
- A17 code.claude.com/docs/en/agent-teams · A18 code.claude.com/docs/en/advisor · A19 code.claude.com/docs/en/best-practices
- A20 code.claude.com/docs/en/changelog (2.1.288-2.1.310, 2026-10-02 to 10-23) · A21 code.claude.com/docs/en/env-vars
- A23 www.anthropic.com/engineering/harness-design-long-running-apps (2026-03-24)
- A24 www.anthropic.com/engineering/effective-harnesses-for-long-running-agents
- A25 www.anthropic.com/engineering/effective-context-engineering-for-ai-agents
- A26 www.anthropic.com/engineering/multi-agent-research-system · A27 www.anthropic.com/engineering/building-c-compiler
- R1 arxiv.org/abs/2607.25152 (Park, Choi; v2 2026-09-29; preprint) · C1 www.dsebastien.net/2026-10-04-claude-opus-5-5-give-it-
  the-whole-task-keep-your-hand-on-the-brake/ (community, mostly second-hand Hacker News claims)
All URLs are https://; "..." stands for platform.claude.com/docs/en/build-with-claude. Fetched 2026-10-24.

# /goal runs: the evaluator, Stop hooks, background waits and long-run limits

## Answer (10 lines)

1. `/goal` is a session-scoped prompt-based Stop hook. When each turn ends, the small fast model gets the condition plus the conversation so far and returns not met, met or impossible. It runs no tools and reads no files, so it judges only what the transcript shows. Verified: [goal#how-evaluation-works](https://code.claude.com/docs/en/goal#how-evaluation-works), [goal#write-an-effective-condition](https://code.claude.com/docs/en/goal#write-an-effective-condition)
2. A turn that ends with a subagent or background shell command still running is not evaluated. The next turn that ends with nothing running is, and finished work comes back to Claude as a new turn without a prompt (fixed in 2.1.143). Verified: [goal#background-work-defers-evaluation](https://code.claude.com/docs/en/goal#background-work-defers-evaluation), [changelog](https://code.claude.com/docs/en/changelog)
3. While background work keeps the goal waiting, check-ins fall due at 30 min, 1 h after that, then every 2 h (`CLAUDE_CODE_GOAL_CHECKIN_MINUTES`). Interactive sessions start at most three idle check-ins per goal between prompts; `-p` delivers check-ins only at turn ends. Verified: [goal#background-work-defers-evaluation](https://code.claude.com/docs/en/goal#background-work-defers-evaluation)
4. A message typed mid-turn is queued and handed to Claude after the running tool calls, inside the same turn, so the evaluator judges at the end of that longer turn. Stop hooks don't run after a user interrupt (Esc), so that turn is not evaluated (that /goal consequence is Inferred). Verified: [interactive-mode#when-claude-code-sends-what-you-queued](https://code.claude.com/docs/en/interactive-mode#when-claude-code-sends-what-you-queued), [hooks#stop](https://code.claude.com/docs/en/hooks#stop)
5. "X as the final message" is unreliable once the run keeps working after X. The verdict is a small model's reading of the whole conversation at the first turn that ends with nothing running, and by then X may no longer be last. Stop background work, print X last, or gate on a command Stop hook. Inferred from [goal#how-evaluation-works](https://code.claude.com/docs/en/goal#how-evaluation-works) and [goal#background-work-defers-evaluation](https://code.claude.com/docs/en/goal#background-work-defers-evaluation)
6. Plain Stop hooks still fire at every turn end with work in flight. Their input carries `background_tasks` and `session_crons`, so a script can tell "done" from "waiting to be woken". `decision: "block"` (or exit 2) feeds the reason back as Claude's next instruction, capped at 8 blocks in a row without a tool call. Verified: [hooks#stop-input](https://code.claude.com/docs/en/hooks#stop-input), [hooks#stop-decision-control](https://code.claude.com/docs/en/hooks#stop-decision-control)
7. To wait without polling: background Bash and background subagents (completion notifications), Monitor (streams each output line; 5 min default and 30 min max per watch, then re-arm), goal check-ins, and `asyncRewake` hooks (these wake an idle session). `/loop` and cron fire only between turns while idle, and missed fires don't each catch up. Verified: [tools-reference#monitor-tool](https://code.claude.com/docs/en/tools-reference#monitor-tool), [scheduled-tasks#limitations](https://code.claude.com/docs/en/scheduled-tasks#limitations), [hooks#run-hooks-in-the-background](https://code.claude.com/docs/en/hooks#run-hooks-in-the-background)
8. Compaction keeps background commands and subagents running, re-injects project-root CLAUDE.md and auto memory, re-reads up to five recent files and summarizes the rest. An overflow that auto-compaction can't clear ends the goal. Verified: [context-window#what-survives-compaction](https://code.claude.com/docs/en/context-window#what-survives-compaction), [goal#errors-you-have-to-fix-clear-the-goal](https://code.claude.com/docs/en/goal#errors-you-have-to-fix-clear-the-goal)
9. A claude.ai usage limit pauses the goal (`Goal paused`). An open interactive session resumes at the reset on its own, but not in `-p` or background sessions, not for resets more than 24 h away, and it re-arms at most twice in a row. Background subagents that hit a limit fail and report back. Verified: [goal#other-errors-retry-or-pause-the-goal](https://code.claude.com/docs/en/goal#other-errors-retry-or-pause-the-goal), [interactive-mode#wait-for-a-usage-limit-to-reset](https://code.claude.com/docs/en/interactive-mode#wait-for-a-usage-limit-to-reset), [sub-agents#api-errors-in-subagents](https://code.claude.com/docs/en/sub-agents#api-errors-in-subagents)
10. Defaults: 20 running subagents (`CLAUDE_CODE_MAX_CONCURRENT_SUBAGENTS`), 3 nesting layers (`CLAUDE_CODE_MAX_SUBAGENT_SPAWN_DEPTH`), 16 workflow agents, no cap on the total. Background commands have no time limit in a terminal session. Unattended runs (`-p`, SDK, CI, cloud) default to 30 min (10 min in a text-prompt `-p` run), 2 h max. Verified: [sub-agents#concurrent-subagent-limit](https://code.claude.com/docs/en/sub-agents#concurrent-subagent-limit), [sub-agents#let-subagents-spawn-their-own-subagents](https://code.claude.com/docs/en/sub-agents#let-subagents-spawn-their-own-subagents), [tools-reference#time-limit-for-background-commands](https://code.claude.com/docs/en/tools-reference#time-limit-for-background-commands)

**How to read this file:**

- **Read:** 2026-10-22, from the official docs at code.claude.com/docs. Each page was read as raw Markdown, by adding `.md` to the URL.
- **Changelog:** the newest entry when read was 2.1.309, dated 2026-10-22.
- **Installed Claude Code version:** not checked.
- **Verified** means the cited page says it and I read it there. **Inferred** means my reasoning from the cited pages; they don't state it.
- **Web requests:** plain GETs of public pages, with no personal data or local paths.

---

## 1. How the `/goal` evaluator decides

### 1.1 What it is and what it reads

- **What it wraps:** `/goal` is "a wrapper around a session-scoped prompt-based Stop hook". Each time Claude finishes a turn, Claude Code sends "the condition and the conversation so far" to the configured small fast model. Verified: [goal#how-evaluation-works](https://code.claude.com/docs/en/goal#how-evaluation-works)
- **Verdicts:** there are three, each with a short reason:
  - **Not yet met:** Claude keeps working and takes the reason as guidance.
  - **Met:** the goal clears and an achieved entry is recorded.
  - **Impossible:** the goal clears and a failed entry is recorded with the reason.

  Verified: [goal#how-evaluation-works](https://code.claude.com/docs/en/goal#how-evaluation-works)
- **What it judges:** what Claude has surfaced in the conversation. "It doesn't run commands or read files independently"; a test condition works "because Claude runs the tests and the result appears in the transcript for the evaluator to read". Verified: [goal#write-an-effective-condition](https://code.claude.com/docs/en/goal#write-an-effective-condition)
- **No tools:** the evaluator "does not call tools, so it can only judge what Claude has already surfaced in the conversation". Verified: [goal#evaluation-model-and-cost](https://code.claude.com/docs/en/goal#evaluation-model-and-cost)
- **Model:** it runs on the session's provider. `ANTHROPIC_DEFAULT_HAIKU_MODEL` changes the evaluator model. The same variable also re-points the `haiku` alias and background work such as conversation summarization. Verified: [goal#evaluation-model-and-cost](https://code.claude.com/docs/en/goal#evaluation-model-and-cost)
- **Subagent work:** the parent conversation sees only a subagent's final result, not its intermediate tool calls. Verified: [tools-reference#agent-tool-behavior](https://code.claude.com/docs/en/tools-reference#agent-tool-behavior). So the evaluator sees a builder's work only through the report it returns. Inferred.
- **Condition format:**
  - At most 4,000 characters.
  - Suggested parts: one measurable end state, a stated check, and the constraints that matter.
  - To bound the run, add a turn or time clause such as `or stop after 20 turns`. "Claude reports progress against that clause each turn and the evaluator judges it from the conversation."

  Verified: [goal#write-an-effective-condition](https://code.claude.com/docs/en/goal#write-an-effective-condition)
- **Where verdicts show:** the transcript shows each verdict, and Ctrl+O shows its reason. `/goal` with no argument shows the condition, elapsed time, turns evaluated, token spend and the latest reason. Verified: [goal#set-a-goal](https://code.claude.com/docs/en/goal#set-a-goal), [goal#check-status](https://code.claude.com/docs/en/goal#check-status)
- **Setting and clearing:**
  - One goal per session; a new `/goal <condition>` replaces the active one.
  - `/goal clear` removes it, and so do the aliases `stop`, `off`, `reset`, `none` and `cancel`.
  - `/clear` also removes the goal.

  Verified: [goal#set-a-goal](https://code.claude.com/docs/en/goal#set-a-goal), [goal#clear-a-goal](https://code.claude.com/docs/en/goal#clear-a-goal)

### 1.2 When it runs

- **After each turn:** "After each turn, a model checks whether the condition holds." Verified: [goal](https://code.claude.com/docs/en/goal)
- **What a turn is:** "One complete response from Claude ... begins when you send a message and ends when Claude finishes responding, with any number of tool calls in between. Stop hooks fire at the end of each turn." Verified: [glossary#turn](https://code.claude.com/docs/en/glossary#turn)
- **First turn:** setting a goal starts a turn at once, with the condition as the directive. Verified: [goal#set-a-goal](https://code.claude.com/docs/en/goal#set-a-goal)
- **Next turn:** it starts when the previous turn finishes. In an interactive session it can also start when an idle check-in or an automatic retry comes due. Verified: [goal#compare-ways-to-keep-a-session-running](https://code.claude.com/docs/en/goal#compare-ways-to-keep-a-session-running)
- **Turns Stop hooks skip:** a Stop hook "does not run if the stoppage occurred due to a user interrupt", and API errors fire `StopFailure` instead. Verified: [hooks#stop](https://code.claude.com/docs/en/hooks#stop). So an interrupted turn gets no evaluation, and an API-error turn follows the failure rules in 1.5. Inferred.
- **Permissions:** a goal doesn't change the permission mode. "To let goal turns run unattended, run `/goal` in auto mode." Verified: [goal#set-a-goal](https://code.claude.com/docs/en/goal#set-a-goal)

### 1.3 Background work defers evaluation

- **The rule:** "If a subagent or a background shell command is still running when a turn ends, Claude Code skips the evaluation for that turn. It evaluates at the end of the next turn that finishes with no background work running. When the background work finishes, Claude Code delivers the result to Claude as a new turn, so you don't have to prompt." Verified: [goal#background-work-defers-evaluation](https://code.claude.com/docs/en/goal#background-work-defers-evaluation)
- **Since:** 2.1.143 (2026-05-15), "Fixed `/goal` evaluator firing while background shells or delegated subagents are still running". Verified: [changelog](https://code.claude.com/docs/en/changelog)
- **Consequence:** the evaluator can't declare a goal met or impossible while a subagent or background shell command is still running. Inferred from the rule. The goal can still clear on an unrecoverable error or on `/goal clear` (1.5).
- **Coverage:**
  - The rule names only subagents and background shell commands.
  - Not documented: whether a running Monitor watch, a workflow, a teammate or a pending `/loop` wakeup also defers evaluation.
  - The Stop-hook `background_tasks` list does carry types `monitor`, `workflow` and `teammate`. Verified: [hooks#stop-input](https://code.claude.com/docs/en/hooks#stop-input)
- **Check-in schedule:**
  - The first check-in is due once background work has kept the goal waiting 30 minutes.
  - Claude Code lists the running tasks and asks Claude to read their output, keep waiting if they progress, and fix or stop stuck ones.
  - Later intervals double, up to 4x the first: "with the default, 1 hour after the first check-in, then every 2 hours".

  Verified: [goal#background-work-defers-evaluation](https://code.claude.com/docs/en/goal#background-work-defers-evaluation)
- **Check-in delivery:**
  - At the end of the next turn that finishes with work still running. This is the only way in `-p`.
  - In an interactive session, Claude Code also starts a turn on its own while idle. If the work stopped without reporting, it asks Claude to continue toward the goal.
  - At most three idle check-ins per goal between your prompts; the third says idle check-ins are paused until another prompt.

  Verified: [goal#background-work-defers-evaluation](https://code.claude.com/docs/en/goal#background-work-defers-evaluation). 2.1.246: "your next message allows three more". Verified: [changelog](https://code.claude.com/docs/en/changelog)
- **Unattended timeline:**
  - With defaults, idle check-ins land about 30 min, 1 h 30 and 3 h 30 after the wait starts.
  - After the third, the session waits for the work's own completion turn or for a prompt.
  - A job that hangs silently past about 3.5 h therefore parks the run.

  Inferred from the schedule.
- **`CLAUDE_CODE_GOAL_CHECKIN_MINUTES`:**
  - Default `30`; whole minutes up to `10080`.
  - `0` turns off check-ins and automatic retries.

  Verified: [env-vars](https://code.claude.com/docs/en/env-vars), [goal#other-errors-retry-or-pause-the-goal](https://code.claude.com/docs/en/goal#other-errors-retry-or-pause-the-goal)
- **Cost:** each idle check-in is a new turn that sends the full context. Verified: [costs#why-usage-climbs-in-a-long-session](https://code.claude.com/docs/en/costs#why-usage-climbs-in-a-long-session)

### 1.4 The stall rule

- **The rule:** "If Claude keeps answering the evaluator without making progress (no tool use for several turns in a row), Claude Code stops the loop, prints a warning, and returns control to you with the goal still set. Evaluation resumes after your next prompt." The page points to the Stop-hook block cap as "the underlying mechanism". Verified: [goal#how-evaluation-works](https://code.claude.com/docs/en/goal#how-evaluation-works)
- **The block cap:**
  - It is 8 consecutive continuations, and the count resets each time Claude calls a tool.
  - `CLAUDE_CODE_STOP_HOOK_BLOCK_CAP` raises it; `0` disables it.

  Verified: [hooks#stop-input](https://code.claude.com/docs/en/hooks#stop-input), [env-vars](https://code.claude.com/docs/en/env-vars)
- **What follows:**
  - "Several" is presumably that cap, 8 by default.
  - A lead that waits by answering in text alone can trip it; one tool call per turn resets the count.
  - Not documented: whether check-in turns without a tool call count toward it.

  Inferred.

### 1.5 When a turn fails, and resume

- **Clears the goal:** four errors you have to fix. The warning starts `Goal cleared after an unrecoverable error` and ends `Run /goal again to continue`.
  - An authentication failure, when Claude Code manages its own credentials.
  - An exhausted credit balance.
  - A context overflow that auto-compaction couldn't clear.
  - A model that isn't available.

  Verified: [goal#errors-you-have-to-fix-clear-the-goal](https://code.claude.com/docs/en/goal#errors-you-have-to-fix-clear-the-goal)
- **Other errors keep the goal.** In an interactive session on 2.1.269 or later:
  - **Retry:** for an overloaded server or a dropped connection, a `Goal still active` notice shows the wait. After three automatic retries the goal pauses.
  - **Pause:** for an API rate limit, a claude.ai usage limit, or "a hook that ended the turn", a `Goal paused` notice names the cause. If the session is waiting to continue automatically at a usage-limit reset, Claude resumes the goal then.
  - "Send a message at any time to start the next turn immediately."

  Verified: [goal#other-errors-retry-or-pause-the-goal](https://code.claude.com/docs/en/goal#other-errors-retry-or-pause-the-goal)
- **Resume:**
  - Resume restores a goal that was still active, on every route; the `claude --resume` picker does so from 2.1.239.
  - Turn count, timer and token-spend baseline reset.
  - Achieved or cleared goals are not restored.

  Verified: [goal#resume-with-an-active-goal](https://code.claude.com/docs/en/goal#resume-with-an-active-goal)
- **`-p`:**
  - `claude -p "/goal ..."` runs the loop to completion in one invocation.
  - Default text output prints only when the loop ends, so use `--output-format stream-json --verbose` to watch it.
  - Ctrl+C stops it.

  Verified: [goal#run-non-interactively](https://code.claude.com/docs/en/goal#run-non-interactively). Not documented: what a paused goal does in `-p`, where no one can send the message that resumes it.
- **Availability:** the same workspace-trust rule as settings hooks applies. `/goal` is unavailable when `disableAllHooks` is true or managed settings set `allowManagedHooksOnly`. Verified: [goal#requirements](https://code.claude.com/docs/en/goal#requirements)

### 1.6 A user message sent in the middle of a turn

- **Queued messages:**
  - Pressing Enter while Claude works queues the message instead of interrupting.
  - "if you queue a message while Claude is running tool calls, Claude Code passes it to Claude as soon as those tool calls finish, within the same turn."
  - Messages still queued at turn end go out automatically, in order.
  - Commands and `!` shell commands are held until the turn ends.

  Verified: [interactive-mode#when-claude-code-sends-what-you-queued](https://code.claude.com/docs/en/interactive-mode#when-claude-code-sends-what-you-queued)
- **Ctrl+Enter (2.1.275+):**
  - It sends what's queued at once.
  - If Claude is running shell commands, subagents or other work that can move to the background, that work moves there and Claude reads the messages in the same turn. Otherwise the turn is interrupted.
  - Esc interrupts and sends the queue right away.

  Verified: [interactive-mode#when-claude-code-sends-what-you-queued](https://code.claude.com/docs/en/interactive-mode#when-claude-code-sends-what-you-queued)
- **Effects on the goal.** All Inferred from the lines above:
  - A mid-turn message lengthens the current turn. The evaluator later reads it as part of "the conversation so far", and the turn's last message becomes Claude's reply to it.
  - Ctrl+Enter during a shell command or subagent moves that work to the background, which defers evaluation (1.3).
  - Esc fires no Stop hook, so that turn gets no evaluation.
- **A message never clears the goal:** only met, impossible, an unrecoverable error, `/goal clear` or `/clear` do. Verified: [goal#how-evaluation-works](https://code.claude.com/docs/en/goal#how-evaluation-works), [goal#clear-a-goal](https://code.claude.com/docs/en/goal#clear-a-goal)
- **Your next prompt also:**
  - Re-arms three idle check-ins. Verified: [changelog](https://code.claude.com/docs/en/changelog) (2.1.246)
  - Restarts evaluation after a stall stop. Verified: [goal#how-evaluation-works](https://code.claude.com/docs/en/goal#how-evaluation-works)
- **Typing `/goal clear` mid-turn:** the docs name only `/status`, `/model`, `/effort` and `/fast` as running on send, so it is probably held until the turn ends. Inferred; not documented for `/goal`. Source: [interactive-mode#when-claude-code-sends-what-you-queued](https://code.claude.com/docs/en/interactive-mode#when-claude-code-sends-what-you-queued)

### 1.7 Is "X as the final message" reliable when the run keeps working after X?

No, not on its own. Inferred. Each row below builds on a Verified mechanic.

| When X is printed | What the docs say happens next | Effect on "X as the final message" (Inferred) |
| :- | :- | :- |
| As the last output of a turn, with nothing in the background | The evaluator runs at that turn end. Verified: [goal#how-evaluation-works](https://code.claude.com/docs/en/goal#how-evaluation-works) | Likely met, but it is still a small model's reading |
| Mid-turn, followed by more tool calls or text | The turn is evaluated once, at its end, on the whole conversation. Verified: same page | X is no longer last. A strict reading says not met and the run goes on. A loose reading says met because X appeared earlier. Either can happen |
| While a subagent or background shell command runs | That turn is skipped. The result arrives as a new turn, and the first turn that ends with nothing running is evaluated. Verified: [goal#background-work-defers-evaluation](https://code.claude.com/docs/en/goal#background-work-defers-evaluation) | That later turn usually ends with Claude's reaction to the result, not X, so it is not met until X is printed again |
| Before a compaction | The conversation is summarized, and early detail can be lost. Verified: [how-claude-code-works#when-context-fills-up](https://code.claude.com/docs/en/how-claude-code-works#when-context-fills-up) | X may survive only as summary text |
| With a time clause (`or 12 h`) | The evaluator judges the clause from what Claude reports each turn. Verified: [goal#write-an-effective-condition](https://code.claude.com/docs/en/goal#write-an-effective-condition) | The evaluator has no clock, so Claude must print elapsed time every turn |
| When a user message arrives mid-turn | It is delivered after the current tool calls, in the same turn. Verified: [interactive-mode#when-claude-code-sends-what-you-queued](https://code.claude.com/docs/en/interactive-mode#when-claude-code-sends-what-you-queued) | The turn now ends with Claude's reply, not X |

What holds up better. Every item is Inferred, from the Verified mechanics above:

1. Finish or stop every background command, Monitor and subagent before printing X. Otherwise the turn that prints X is never evaluated.
2. Make X the last output of its turn, with no tool call or text after it.
3. Write the condition as a state plus its proof, not as a position. For example: "a turn has printed the close table with every row DONE or SHORT, after showing that no background task is running". Avoid "the final message is the close table".
4. If the condition has a time clause, have Claude print the elapsed time each turn.
5. For a deterministic gate, use a command Stop hook. It reads `last_assistant_message`, which holds "the text content of Claude's final response", and `background_tasks`. Verified fields: [hooks#stop-input](https://code.claude.com/docs/en/hooks#stop-input). The hook allows the stop while tasks are in flight, blocks with a reason when nothing runs and X isn't last, and allows the stop once it is. Watch the 8-block cap (2.4). `/goal` and a Stop hook both fire after every turn. Verified: [goal#compare-ways-to-keep-a-session-running](https://code.claude.com/docs/en/goal#compare-ways-to-keep-a-session-running)

---

## 2. Stop hooks while background work runs, and how a blocked stop continues the run

### 2.1 When Stop fires, and what it receives

- **When:** Stop "runs when the main Claude Code agent has finished responding". It doesn't run on a user interrupt, and API errors fire `StopFailure` instead, which has no decision control. Verified: [hooks#stop](https://code.claude.com/docs/en/hooks#stop), [hooks#stopfailure](https://code.claude.com/docs/en/hooks#stopfailure)
- **Background work doesn't stop it firing:**
  - The input carries `stop_hook_active`, `last_assistant_message`, `background_tasks` and `session_crons`.
  - The two arrays "let hooks distinguish 'session is done' from 'session is paused waiting for background work to wake it back up'".
  - The documented sample input shows a Stop event with an in-flight `shell` task.

  Verified: [hooks#stop-input](https://code.claude.com/docs/en/hooks#stop-input). Only `/goal` is documented to skip evaluation while work runs (1.3).
- **`background_tasks` entries:**
  - Fields: `id`, `type`, `status`, `description`, plus `command` for shell tasks and `agent_type` for subagents.
  - Types: `shell`, `subagent`, `monitor`, `workflow`, `teammate`, `cloud session`, `MCP task`.
- **`session_crons` entries:** they come from `CronCreate`, `ScheduleWakeup` and `/loop`.
- **When the arrays are present:** whenever the task registry is reachable; they are empty when nothing is in flight or scheduled.

  Verified: [hooks#stop-input](https://code.claude.com/docs/en/hooks#stop-input). The arrays were added in 2.1.145 (2026-05-19). Verified: [changelog](https://code.claude.com/docs/en/changelog)
- **Transcript lag:** the transcript file is written asynchronously and may lag. Hooks that need the turn's final text should use `last_assistant_message`. Verified: [hooks#common-input-fields](https://code.claude.com/docs/en/hooks#common-input-fields), [hooks#stop-input](https://code.claude.com/docs/en/hooks#stop-input)

### 2.2 How a block continues the run

- **Command hooks:** `"decision": "block"` "prevents Claude from stopping", and `reason` is required and "tells Claude why it should continue". Exiting 2 routes stderr the same way. `hookSpecificOutput.additionalContext` also keeps the conversation going; it is labelled `Stop hook feedback` instead of a hook error, under the same loop protections. Verified: [hooks#stop-decision-control](https://code.claude.com/docs/en/hooks#stop-decision-control)
- **Prompt hooks (what `/goal` uses):**
  - The model returns `{ok, reason, impossible}`.
  - On Stop, `ok: false` means "the reason is fed back to Claude as its next instruction and the turn continues". With `impossible: true` the stop is allowed instead.
  - Default timeout 30 s; the default model is the background-functionality model.

  Verified: [hooks#how-prompt-based-hooks-work](https://code.claude.com/docs/en/hooks#how-prompt-based-hooks-work), [hooks#response-schema](https://code.claude.com/docs/en/hooks#response-schema)
- **Agent hooks (experimental):**
  - They spawn a subagent with Read, Grep and Glob, for up to 50 turns, with a 60 s default timeout.
  - They can inspect files and test output.
  - No `impossible` field.

  Verified: [hooks#agent-based-hooks](https://code.claude.com/docs/en/hooks#agent-based-hooks)

### 2.3 Async hooks that can wake the session

- **`async: true`:**
  - The hook runs in the background and can't block.
  - Its output is delivered on the next turn. "If the session is idle, the response waits until the next user interaction."
  - The exception: an `asyncRewake` hook that exits 2 "wakes Claude immediately even when the session is idle".

  Verified: [hooks#run-hooks-in-the-background](https://code.claude.com/docs/en/hooks#run-hooks-in-the-background)
- **In `-p`:** async hooks still running at teardown are killed. Verified: same section.

### 2.4 Loop protection

- **`stop_hook_active`:** it is true when Claude is already continuing because of a stop hook. Verified: [hooks#stop-input](https://code.claude.com/docs/en/hooks#stop-input)
- **The 8-block cap:** "after stop hooks have continued the turn eight times in a row, Claude Code overrides the next block and ends the turn. The count of consecutive continuations resets each time Claude calls a tool." The turn ends with a warning. Verified: [hooks#stop-input](https://code.claude.com/docs/en/hooks#stop-input), [hooks-guide#stop-hook-hits-the-block-cap](https://code.claude.com/docs/en/hooks-guide#stop-hook-hits-the-block-cap). The cap arrived in 2.1.143. Verified: [changelog](https://code.claude.com/docs/en/changelog)
- **`CLAUDE_CODE_STOP_HOOK_BLOCK_CAP`:** default 8; `0` disables the cap. Verified: [env-vars](https://code.claude.com/docs/en/env-vars)

### 2.5 Implications

- A custom Stop hook that blocks while `background_tasks` is non-empty makes Claude spin up turns instead of idling until the completion notification. Without tool calls it hits the cap. Inferred.
- So a hook should allow the stop in that state and judge only when both arrays are empty, which is what `/goal` does natively. Inferred.

---

## 3. Waiting for long background jobs without polling

| Mechanism | How the result reaches Claude | Limits | Source (Verified) |
| :- | :- | :- | :- |
| Background Bash (`run_in_background`, Ctrl+B, or auto-moved at its timeout) | A task ID returns at once, and output goes to a file Claude can Read. In a goal run, finished work arrives as a new turn | No time limit in terminal, desktop and VS Code sessions. Unattended runs (`-p`, SDK, CI, cloud): 30 min default, `timeout` up to 2 h, 10 min in a text-prompt `-p` run. Raise with `BASH_DEFAULT_TIMEOUT_MS` / `BASH_MAX_TIMEOUT_MS`. Killed past 5 GB of output, cleaned up when Claude Code exits. One started by a foreground subagent ends with that subagent | [interactive-mode#how-backgrounding-works](https://code.claude.com/docs/en/interactive-mode#how-backgrounding-works), [tools-reference#time-limit-for-background-commands](https://code.claude.com/docs/en/tools-reference#time-limit-for-background-commands), [tools-reference#when-a-background-command-stops](https://code.claude.com/docs/en/tools-reference#when-a-background-command-stops), [goal#background-work-defers-evaluation](https://code.claude.com/docs/en/goal#background-work-defers-evaluation) |
| Background subagent | "A completion notification in a later turn", marked as an automated event | 20 running by default. Permission prompts surface in the main session | [sub-agents#tools-permissions-and-results-of-background-subagents](https://code.claude.com/docs/en/sub-agents#tools-permissions-and-results-of-background-subagents), [sub-agents#concurrent-subagent-limit](https://code.claude.com/docs/en/sub-agents#concurrent-subagent-limit) |
| Monitor | Each output line, or WebSocket message, comes back as an event; "Claude interjects when an event arrives" | Each watch: 5 min default, 30 min max, 10 min max in a single-prompt `-p` run. At the deadline Claude gets one notice and can restart the watch. On Windows it needs Git Bash. Unavailable when `DISABLE_TELEMETRY` or `CLAUDE_CODE_DISABLE_NONESSENTIAL_TRAFFIC` is set, and on Bedrock, Agent Platform and Foundry. Not restarted on resume | [tools-reference#monitor-tool](https://code.claude.com/docs/en/tools-reference#monitor-tool), [scheduled-tasks#limitations](https://code.claude.com/docs/en/scheduled-tasks#limitations) |
| `/loop` on a fixed interval (cron) | The prompt fires between turns | Fires only while Claude Code is running and idle. A due prompt waits for the current turn to end, and missed fires collapse into one. Jitter: a recurring task runs up to half its gap late, at most 30 min; `*/5` runs 4 min 45 s after the previous run. 7-day expiry, 50 tasks per session. Esc and Ctrl+C don't stop a fixed loop | [scheduled-tasks#how-session-scoped-scheduled-tasks-run](https://code.claude.com/docs/en/scheduled-tasks#how-session-scoped-scheduled-tasks-run), [scheduled-tasks#jitter](https://code.claude.com/docs/en/scheduled-tasks#jitter), [scheduled-tasks#limitations](https://code.claude.com/docs/en/scheduled-tasks#limitations), [scheduled-tasks#stop-a-loop](https://code.claude.com/docs/en/scheduled-tasks#stop-a-loop) |
| Self-paced `/loop` (`ScheduleWakeup`) | Claude picks the next wakeup 1 min to 1 h out, and may use Monitor instead | Only for self-paced `/loop`. If an iteration neither reschedules nor stops, a fallback wakeup comes about 20 min later. Not restored on resume. Shows in `session_crons` | [scheduled-tasks#let-claude-choose-the-interval](https://code.claude.com/docs/en/scheduled-tasks#let-claude-choose-the-interval), [scheduled-tasks#stop-a-loop](https://code.claude.com/docs/en/scheduled-tasks#stop-a-loop), [tools-reference](https://code.claude.com/docs/en/tools-reference) |
| One-shot reminder (`CronCreate`) | A natural-language "in 45 minutes, check ..." fires once and then deletes itself | One scheduled for `:00` or `:30` can run up to 90 s early | [scheduled-tasks](https://code.claude.com/docs/en/scheduled-tasks) |
| Goal check-ins | Claude Code starts the check-in turn itself | See 1.3 | [goal#background-work-defers-evaluation](https://code.claude.com/docs/en/goal#background-work-defers-evaluation) |
| `asyncRewake` command hook | Exit 2 wakes Claude immediately, even when idle | Command hooks only, and `timeout` is enforced | [hooks#run-hooks-in-the-background](https://code.claude.com/docs/en/hooks#run-hooks-in-the-background) |
| Cross-session message; channel | A message is read "between tool calls during an active turn"; on an idle session it starts a new turn. Channels push MCP events into an open session | Channels are a research preview: Bun-based plugins, and Team and Enterprise must enable them | [cross-session-messaging](https://code.claude.com/docs/en/cross-session-messaging), [channels](https://code.claude.com/docs/en/channels) |

Which of these work while a `/goal` run is busy:

- **Background Bash and background subagents:** yes. They are the designed path; they defer evaluation and resume the goal on completion. Verified: [goal#background-work-defers-evaluation](https://code.claude.com/docs/en/goal#background-work-defers-evaluation)
- **Monitor:**
  - Its events arrive mid-conversation by design. Verified: [tools-reference#monitor-tool](https://code.claude.com/docs/en/tools-reference#monitor-tool)
  - Not documented: whether a live watch defers goal evaluation.
  - It needs re-arming at least every 30 min. Verified: same section.
- **`/loop` and cron:**
  - The scheduler queues a due task "at low priority", and it fires only between turns. Verified: [scheduled-tasks#how-session-scoped-scheduled-tasks-run](https://code.claude.com/docs/en/scheduled-tasks#how-session-scoped-scheduled-tasks-run)
  - While a goal starts turns back to back, a fire waits until the session idles, for example when the goal is parked on background work. Inferred.
- **`ScheduleWakeup`:** 2.1.299 (2026-10-14) "Fixed scheduled wakeups (ScheduleWakeup) not firing while a background task was running in Agent SDK, IDE and `claude remote-control` sessions". Verified: [changelog](https://code.claude.com/docs/en/changelog)
- **Foreground `sleep`:** a foreground command that starts with `sleep` isn't moved to the background at its timeout; it stops. Verified: [tools-reference#foreground-commands-that-move-to-the-background](https://code.claude.com/docs/en/tools-reference#foreground-commands-that-move-to-the-background)
- **`-p` runs:**
  - After its result, a `-p` run waits for background commands, subagents, workflows, Monitor watches and pending wakeups.
  - Unless a main-conversation background command is still running, it stops whatever remains after 10 min of idle waiting. `CLAUDE_CODE_PRINT_BG_WAIT_CEILING_MS` changes this, and `0` removes the cap.

  Verified: [headless#background-tasks-at-exit](https://code.claude.com/docs/en/headless#background-tasks-at-exit)
- **Seen in this research session, once, inside a subagent:** a Bash call that hit its 60 to 120 s timeout was moved to the background. Its completion notice arrived between later tool calls in the same turn. Inferred: one observation, not a doc claim. It is consistent with [tools-reference#foreground-commands-that-move-to-the-background](https://code.claude.com/docs/en/tools-reference#foreground-commands-that-move-to-the-background).

---

## 4. Documented limits that matter for a 6-hour run

### 4.1 Compaction

- **How it works:**
  - It is automatic near the limit: older tool outputs are cleared first, then the conversation is summarized.
  - "detailed instructions from early in the conversation may be lost"; put persistent rules in CLAUDE.md.
  - You can steer the summary with a "Compact Instructions" section in CLAUDE.md or `/compact <focus>`.

  Verified: [how-claude-code-works#when-context-fills-up](https://code.claude.com/docs/en/how-claude-code-works#when-context-fills-up)
- **When it triggers:**
  - With no window set, it compacts at the model's limit.
  - Native 1M-window models compact at about 967K tokens: Fable, Sonnet 5 and later, Haiku 5.5, Opus 4.7 and later.
  - Opus and Sonnet 4.6 without extended context compact at 200K. Native 1M models do too when `CLAUDE_CODE_DISABLE_1M_CONTEXT=1` is set.
  - The window is set with `/autocompact`, `autoCompactWindow`, `--autocompact` or `CLAUDE_CODE_AUTO_COMPACT_WINDOW` (100K to 1M).

  Verified: [model-config#default-auto-compact-thresholds](https://code.claude.com/docs/en/model-config#default-auto-compact-thresholds), [model-config#set-the-auto-compact-window](https://code.claude.com/docs/en/model-config#set-the-auto-compact-window)
- **What survives:**
  - The system prompt and output style still apply.
  - Project-root CLAUDE.md, unscoped rules and auto memory are re-injected from disk.
  - Skill descriptions are rebuilt (2.1.304+), and the git status and plan-mode plan are refreshed.
  - Path-scoped rules and nested CLAUDE.md files reload on demand only.
  - Up to five recently modified files are re-read; a file over 5K tokens comes back as a path reference.
  - Invoked skill bodies are re-injected, capped at 5K tokens per skill and 25K in total, oldest dropped first.
  - Background commands and background subagents "keep running. Claude Code reminds Claude which ones are still running so it doesn't start a duplicate".
  - Context that hooks added earlier is summarized; SessionStart hooks matching `compact` run.

  Verified: [context-window#what-survives-compaction](https://code.claude.com/docs/en/context-window#what-survives-compaction)
- **Subagents:** their transcripts are separate files, unaffected by main compaction. Subagents compact on their own as their context fills. Verified: [sub-agents#resume-subagents](https://code.claude.com/docs/en/sub-agents#resume-subagents), [sub-agents#auto-compaction](https://code.claude.com/docs/en/sub-agents#auto-compaction)
- **The goal across compaction:**
  - It survives compaction. Inferred: the docs clear it only for "a context overflow that auto-compaction couldn't clear". Verified: [goal#errors-you-have-to-fix-clear-the-goal](https://code.claude.com/docs/en/goal#errors-you-have-to-fix-clear-the-goal)
  - 2.1.274 (2026-09-17) fixed hook-driven sessions, such as an active `/goal`, failing with "Prompt is too long" instead of compacting. It also fixed an active `/goal` being lost when resuming a compacted session. Verified: [changelog](https://code.claude.com/docs/en/changelog)
- **What the evaluator sees after compaction:**
  - Not documented.
  - "The conversation so far" is then the summary plus later messages, so evidence printed before a compaction may survive only as summary text. Re-print the evidence in the closing turn. Inferred.
- **Thrashing:**
  - If one file or tool output refills the context right after each summary, auto-compaction stops after a few attempts with `Autocompact is thrashing`. Verified: [troubleshooting#auto-compaction-stops-with-a-thrashing-error](https://code.claude.com/docs/en/troubleshooting#auto-compaction-stops-with-a-thrashing-error)
  - In a goal turn, that is the "overflow auto-compaction couldn't clear", which clears the goal. Inferred.
- **Idle compaction:**
  - It is on by default in interactive sessions on the one-hour cache TTL with auto-compaction on, once the context is at least 200K tokens.
  - It runs about 54 min after the last request, while the computer is awake.
  - Turn it off with `idleCompaction: false` or `DISABLE_AUTO_COMPACT`.

  Verified: [settings-reference#idlecompaction](https://code.claude.com/docs/en/settings-reference#idlecompaction). A lead parked for an hour on a long build can be compacted while it waits. Inferred.

### 4.2 Usage limits

- **Limit types:**
  - The session and weekly limits are shared across all models; the Opus and Sonnet limits apply per family.
  - Usage counts against the session and weekly allowances at once: "a large workflow fanout, can exhaust the weekly allowance before the session window resets."
  - A warning such as `You've used 85% of your session limit` can appear, and the status line can show `rate_limits` fields.

  Verified: [errors#youve-hit-your-session-limit](https://code.claude.com/docs/en/errors#youve-hit-your-session-limit)
- **The goal:** it pauses (`Goal paused`) and resumes when the session's automatic continue fires. Verified: [goal#other-errors-retry-or-pause-the-goal](https://code.claude.com/docs/en/goal#other-errors-retry-or-pause-the-goal)
- **Automatic continue: when it runs.**
  - Interactive sessions signed in with a claude.ai subscription, 2.1.234+, on by default (`autoContinueAtUsageLimit`).
  - At the reset, Claude Code "prompts Claude to pick the task up where it stopped. It doesn't resend your last message."
  - Permission prompts still apply.
  - It re-arms at most twice in a row, then stops with `Automatic continue stopped after repeated usage-limit hits`.
  - If the computer slept more than about 30 min and the limit reset during the sleep, it waits for Enter.

  Verified: [interactive-mode#wait-for-a-usage-limit-to-reset](https://code.claude.com/docs/en/interactive-mode#wait-for-a-usage-limit-to-reset), [settings-reference#autocontinueatusagelimit](https://code.claude.com/docs/en/settings-reference#autocontinueatusagelimit)
- **Automatic continue: when it doesn't.**
  - It doesn't start on its own in Remote Control or agent-team teammate sessions, for a reset more than 24 h away (a weekly limit), or for an Opus or Sonnet limit while you run a model outside that family.
  - It is not offered in background sessions or `-p` runs.
  - With API keys, cloud providers or usage-based billing there is no reset to wait for.

  Verified: [interactive-mode#start-a-wait-yourself](https://code.claude.com/docs/en/interactive-mode#start-a-wait-yourself)
- **Subagents:**
  - A background subagent whose run ends on an API error ("such as a usage limit") is marked failed. Claude receives the error and the subagent's last output.
  - A foreground one returns partial output or `Agent terminated early due to an API error`.

  Verified: [sub-agents#api-errors-in-subagents](https://code.claude.com/docs/en/sub-agents#api-errors-in-subagents)
- **Workflows:** agents that hit a usage limit wait for the reset, and no new agents start. This needs 2.1.271+, an interactive session, `autoContinueAtUsageLimit` on, and a reset within 24 h; a run waits at most twice. Verified: [workflows#when-a-run-hits-your-usage-limit](https://code.claude.com/docs/en/workflows#when-a-run-hits-your-usage-limit)
- **For a 6-hour run:**
  - A session-limit reset resumes the goal, but builders running as background subagents come back failed. Resuming one by `SendMessage` keeps its history (Verified: [sub-agents#resume-subagents](https://code.claude.com/docs/en/sub-agents#resume-subagents)).
  - A weekly-limit hit with its reset days away ends unattended progress.

  Inferred.

### 4.3 Subagent concurrency and depth

- **Concurrency:**
  - With 20 subagents running, a further spawn fails with `Concurrent subagent limit reached`, and the error tells Claude not to retry.
  - `CLAUDE_CODE_MAX_CONCURRENT_SUBAGENTS` adjusts the limit; it takes a positive integer and can't disable the limit. Requires 2.1.217+.
  - Ultracode sessions are exempt.
  - "There's no limit on the total number of subagents Claude can spawn over a session."
  - A `/subtask` fork takes a slot but is never blocked. A resume takes a fresh slot without checking the limit.

  Verified: [sub-agents#concurrent-subagent-limit](https://code.claude.com/docs/en/sub-agents#concurrent-subagent-limit), [env-vars](https://code.claude.com/docs/en/env-vars)
- **Depth:**
  - The default is three layers below the main conversation. At the limit the Agent tool is withheld; a fork keeps it, but the call errors.
  - `CLAUDE_CODE_MAX_SUBAGENT_SPAWN_DEPTH`; `1` turns nesting off.
  - In an interactive session, a subagent that launches background subagents waits for their results before it finishes.
  - History: five fixed layers in 2.1.172 to 2.1.216; default 1 in 2.1.217 to 2.1.218; default 3 since 2.1.219.

  Verified: [sub-agents#let-subagents-spawn-their-own-subagents](https://code.claude.com/docs/en/sub-agents#let-subagents-spawn-their-own-subagents)
- **Foreground or background:**
  - With nothing configured, a subagent spawned from the main conversation starts in the background.
  - In interactive terminal, VS Code and desktop sessions, Claude can't wait for a subagent while fork mode is on; `CLAUDE_CODE_FORK_SUBAGENT=0` lets it wait.
  - Background subagents have a smaller tool set, and their permission prompts surface in the main session.

  Verified: [sub-agents#run-subagents-in-foreground-or-background](https://code.claude.com/docs/en/sub-agents#run-subagents-in-foreground-or-background), [sub-agents#tools-permissions-and-results-of-background-subagents](https://code.claude.com/docs/en/sub-agents#tools-permissions-and-results-of-background-subagents)
- **`maxTurns`:** it caps a subagent's turns. The result comes back marked partial and can be resumed. Verified: [sub-agents#supported-frontmatter-fields](https://code.claude.com/docs/en/sub-agents#supported-frontmatter-fields)
- **Workflows:**
  - Up to 16 concurrent agents by default, fewer with fewer CPUs.
  - `CLAUDE_CODE_WORKFLOW_MAX_CONCURRENT_AGENTS` takes 1 to 256 (2.1.269+).
  - At most 4,096 items per `parallel()` or `pipeline()` call, and 1,000 agents per run.
  - The session's concurrent-subagent limit isn't enforced for agents a workflow spawns.

  Verified: [workflows#behavior-and-limits](https://code.claude.com/docs/en/workflows#behavior-and-limits), [workflows](https://code.claude.com/docs/en/workflows)
- **Agent teams:** experimental and off by default (`CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS=1`); "There's no hard limit on the number of teammates". Verified: [agent-teams](https://code.claude.com/docs/en/agent-teams)

### 4.4 Other unattended blockers, and resume

- **Prompts that wait for a person:** permission prompts, plan approval included, "never auto-resolve on idle". `AskUserQuestion` stays open unless `askUserQuestionTimeout` is set (`60s`, `5m`, `10m`). Verified: [tools-reference#question-auto-continue-timeout](https://code.claude.com/docs/en/tools-reference#question-auto-continue-timeout)
- **On resume:**
  - Background Bash and Monitor tasks are not restarted.
  - A background subagent, Bash command or workflow that ended with the previous process shows only as a note, and Claude Code doesn't start a turn from it.
  - Unexpired cron tasks are restored; a self-paced `/loop` is not.

  Verified: [sessions#what-a-resumed-session-restores](https://code.claude.com/docs/en/sessions#what-a-resumed-session-restores), [scheduled-tasks#limitations](https://code.claude.com/docs/en/scheduled-tasks#limitations)
- **Background commands at exit:** they are cleaned up when Claude Code exits. Commands started by the main conversation or by a background subagent keep running after a final response. Verified: [interactive-mode#how-backgrounding-works](https://code.claude.com/docs/en/interactive-mode#how-backgrounding-works), [tools-reference#when-a-background-command-stops](https://code.claude.com/docs/en/tools-reference#when-a-background-command-stops)

### 4.5 Cost in a long session

- **What drives it:**
  - The full conversation goes with every request.
  - Goal check-ins, scheduled-task fires and cross-session messages each send the full context.
  - Subagents and workflows add their own requests.
  - `/compact` is itself a large request.

  Verified: [costs#why-usage-climbs-in-a-long-session](https://code.claude.com/docs/en/costs#why-usage-climbs-in-a-long-session)
- **Evaluation:** its tokens bill on the small fast model and are "typically negligible". Verified: [goal#evaluation-model-and-cost](https://code.claude.com/docs/en/goal#evaluation-model-and-cost)

---

## 5. Version gates

All rows are Verified from [changelog](https://code.claude.com/docs/en/changelog), plus the page named in the row. A dash means the date was not read.

| Version (date) | Change |
| :- | :- |
| 2.1.139 (2026-05-11) | `/goal` added; it works in interactive, `-p` and Remote Control sessions |
| 2.1.143 (2026-05-15) | The evaluator no longer fires while background shells or delegated subagents run. Stop hooks end the turn with a warning after 8 consecutive blocks |
| 2.1.145 (2026-05-19) | Stop and SubagentStop input gains `background_tasks` and `session_crons` |
| 2.1.217 / 2.1.219 (dates not read) | Concurrency and depth variables added; depth default raised to 3 in 2.1.219 ([sub-agents](https://code.claude.com/docs/en/sub-agents#let-subagents-spawn-their-own-subagents)) |
| 2.1.234 (2026-08-17) | The goal clears on unrecoverable errors; check-ins start after 30 min of background waiting; usage-limit auto-continue added ([interactive-mode](https://code.claude.com/docs/en/interactive-mode#wait-for-a-usage-limit-to-reset)) |
| 2.1.236 (-) | Idle check-ins |
| 2.1.239 (-) | Check-in back-off (30 min, then 1 h, then every 2 h) also applies at turn ends; the `--resume` picker restores the goal |
| 2.1.246 (2026-08-25) | At most three idle check-ins per goal; the next message allows three more |
| 2.1.259 (-) | Blocking Stop hooks no longer drop the model's reasoning on the turn after a block |
| 2.1.269 (2026-09-11) | The goal retries with backoff or pauses and says why, including until a usage limit resets; the workflow concurrency variable arrives ([workflows](https://code.claude.com/docs/en/workflows#behavior-and-limits)) |
| 2.1.271 (-) | Workflow agents wait at a usage limit instead of failing ([workflows](https://code.claude.com/docs/en/workflows#when-a-run-hits-your-usage-limit)) |
| 2.1.274 (2026-09-17) | Goal sessions compact instead of dying with "Prompt is too long"; the goal is kept when resuming a compacted session; Monitor's final output and exit arrive as one notification |
| 2.1.285 / 2.1.288 (-) | Background-command time limit added in 2.1.285; interactive sessions exempt from 2.1.288 ([tools-reference](https://code.claude.com/docs/en/tools-reference#time-limit-for-background-commands)) |
| 2.1.299 (2026-10-14) | `ScheduleWakeup` fires while a background task runs, in SDK, IDE and remote-control sessions |
| 2.1.309 (2026-10-22) | Newest entry when read |

Before the run, check `claude --version`. The goal behaviours above need at least 2.1.274, and the background time-limit rules match 2.1.288 and later. Inferred.

---

## 6. Not found in the docs

- Whether a running Monitor watch, workflow, teammate or pending `/loop` wakeup defers `/goal` evaluation. The goal page names only subagents and background shell commands.
- The evaluator's prompt text, and whether it weighs the last message more than earlier ones.
- Whether the evaluator reads the in-memory conversation or the transcript file, which can lag.
- What a paused goal does in `-p`, where nobody can send the message that resumes it.
- Whether check-in turns without a tool call count toward the stall rule.
- Whether `/goal clear` typed mid-turn runs at once or waits in the queue.

## Sources (all read 2026-10-22; raw Markdown at each URL plus `.md`)

- https://code.claude.com/docs/llms.txt (index)
- https://code.claude.com/docs/en/goal
- https://code.claude.com/docs/en/hooks
- https://code.claude.com/docs/en/hooks-guide
- https://code.claude.com/docs/en/interactive-mode
- https://code.claude.com/docs/en/tools-reference
- https://code.claude.com/docs/en/scheduled-tasks
- https://code.claude.com/docs/en/sub-agents
- https://code.claude.com/docs/en/agents
- https://code.claude.com/docs/en/workflows
- https://code.claude.com/docs/en/agent-teams
- https://code.claude.com/docs/en/context-window
- https://code.claude.com/docs/en/how-claude-code-works
- https://code.claude.com/docs/en/model-config
- https://code.claude.com/docs/en/settings-reference
- https://code.claude.com/docs/en/env-vars
- https://code.claude.com/docs/en/errors
- https://code.claude.com/docs/en/troubleshooting
- https://code.claude.com/docs/en/costs
- https://code.claude.com/docs/en/headless
- https://code.claude.com/docs/en/sessions
- https://code.claude.com/docs/en/glossary
- https://code.claude.com/docs/en/best-practices
- https://code.claude.com/docs/en/commands
- https://code.claude.com/docs/en/cross-session-messaging
- https://code.claude.com/docs/en/channels
- https://code.claude.com/docs/en/changelog

# Prompt methods for a 4-hour Opus 5.5 night (FORGE69 prompt-method scout, 2026-10-25)

Question: what is new since Research/2026-10-24-forge68/prompt-methods.md (not repeated), how a 4 h prompt differs from an 8 h one, how to
survive usage stops, five rules and one experiment. Labels: [A#] Anthropic doc or post, [R#] preprint, [C#] community (unverified), [L] read
or measured here. Drive, Linear and Gmail searched read-only (nothing on prompt methods); read: NIGHT68-REPORT.md 3 and 8, NIGHT66-68 logs.

## 0. The measured frame (read first)
- [L] Work per FULL 5-hour window (lead xhigh, three builders, a review slot): n66 window 2 = 2:20 (2:15-4:35), n67 = 2:25 (window from
  t0+0:04, stop 2:29, reset 15:40), n68 window 2 = 2:24 (4:00-6:24). Windows already used before t0 stopped early: n66 at 0:20, n68 at
  1:13 (reset 02:50 local = 5 h after about 21:50, an hour before t0). Every stop read "You've hit your individual spend limit ... resets X".
- So a 4 h wall-clock night that opens a fresh window stops at about 2:20 and resets at about 5:00, AFTER a 4:00 CAP: one window plus
  ~1:40 of stop. The prompt's first lines must pick: (a) 4 h wall clock = ~2:20 of agent work + what a detached chain computes during the
  stop, the close drafted before the stop; or (b) 4 h of work = two windows, ~6:40 wall clock. n68's resume at 9:01 past CAP 8:00 is
  (a)'s failure mode. Two builders instead of three may stretch the window: not checked; 2:20 is the conservative prediction.
- n68 section 8 [L]: after both stops the next useful call came at once; no compaction; no agent ID lost. The losses were the stops (5:24),
  PC eaten by the second stop, the closing gates reaped for "low memory": placing work around the stop beats faster decisions.

## 1. New since 2026-10-24
Claude Code (newest 2.1.310 of 10-23, nothing later on 10-25 [A20]; installed 2.1.299 [L]). Lines the earlier note did not use:
- 2.1.299 (installed): background-agent notifications now say when an agent "was stopped by a usage limit" [A20].
- 2.1.300: "Fixed Claude polling a background command's output file or sleeping while it runs". 2.1.304: "Fixed subagents sometimes
  forgetting their most recent work when they were messaged or resumed right after finishing" [A20]: on 2.1.299 a builder resumed by
  SendMessage after a stop can lose its last step. Updating is GG's call (DECIDE68 said not on a night).
- 2.1.301 / 2.1.305: `CLAUDE_CODE_RETRY_WATCHDOG` gained `_MAX_USAGE_LIMIT_WAIT_MS` / `_USAGE_LIMIT_RECHECK_MS`, but the watchdog "fails at
  once when a standard-speed request gets a 429 that reports a spend limit or exhausted usage credits" [A21, A29]: not for these stops.
- Seat plans: Team / seat Enterprise usage "draws from a per-seat allowance that resets on a rolling five-hour window and a weekly window",
  shared with chat and Cowork; "individual spend limit" = the request would bill to usage credits that hit an admin's limit; "When the
  message also names a plan reset time, the developer can instead wait until then" [A28, A29]. When a window starts: undocumented [A35].
- "Why usage climbs in a long session" [A28]: every request re-sends the whole conversation (cache rate); goal check-ins, subagents and
  workflows send their own; "compacting a large context is itself a large request". A lead near 900K tokens pays that on every tool call.
- Status line JSON has `rate_limits.five_hour.used_percentage` / `resets_at`, but only "for claude.ai Pro and Max subscribers, or behind a
  Claude apps gateway" [A32]; whether this org seat gets it: not checked (one statusline dump at t0 answers it).
- Memory reaper: macOS / Linux reap background shells at critical memory pressure after "idle for 30 minutes with no turn or subagent
  running"; "Windows has no memory-pressure signal, so this variable has no effect there" [A21]. Yet n68's gates were stopped at about 9:08
  with "...running low on memory" on Windows 2.1.299 [L], and an open bug (#103722, 10-20, 2.1.303, Windows 11) reports the same notice at
  44.7 / 111.3 GB commit, the reaped task's children running on to finish [C2]. Read the notice as "the wrapper died", not "the job ended".
- Hooks [A33]: Notification matchers `quota_auto_resume_fired` / `_stale` / `_disabled` (v2.1.234+); StopFailure `error: rate_limit`
  (logging only); UserPromptSubmit "also fires on turns Claude Code starts on its own"; Stop input has `background_tasks`. SessionStart
  sources are startup / resume / clear / compact / fork: **n68's SessionStart resume hook cannot fire on a usage auto-continue** (same
  session), which fits report section 8's "the hook's print was not exercised".
- Subagents: one whose run ends on a usage limit "is marked failed, and the message ... includes the subagent's last output"; "Once the
  underlying API error clears, ... resume the subagent" [A10]. Nothing restarts it for you.
- claude.com, 10-22 [A30]: "a single agent with skills frequently outperformed both the one-prompt-for-everything design and the subagent
  design"; Claude Code's system prompt for advanced models was cut "by over 80%"; subagents fit independent work returning "a compact
  result". No Anthropic engineering post since 10-13 [A34]; effort / thinking / compaction docs: no change found [A1, A11].

## 2. A 4 h prompt versus an 8 h prompt
- **One window, one schedule.** Write the night against the predicted stop, not CAP. Opus 5.5 "paces its work to finish inside the budget
  and usually finishes well before it" [A1]: give the `elapsed / budget` line the predicted stop (t0+2:10) as its budget and leave the hard
  CAP to the guard. Under time pressure it "might search and verify a little less" [A1]: keep the critics.
- **Shorter, fewer rules, front-loaded.** Instruction following drops as instructions are added and leans "towards earlier instructions"
  [R2]; "Bloated CLAUDE.md files cause Claude to ignore your actual instructions"; "If you emphasize many lines, none of them stands out"
  [A19]. NIGHT68.md: 261 lines, 12 multi-part rules [L]. For 4 h: at most 150 lines; the first 25 hold the end condition, the schedule and
  at most 8 rules; room detail in brief files builders read (progressive disclosure [A30]); hooks enforce; the four-stop paragraph last [A1].
- **Fewer stages.** Anthropic's ~4 h app run (Opus 4.6) dropped sprints for planner + three build rounds with QA, the evaluator moved "to a
  single pass at the end"; "every component in a harness encodes an assumption about what the model can't do on its own" [A23]. n68 ran 3
  rounds in 8 h [L]. For ~2:20: scripted bearings (15 min), one build round, one batched critic, PC, the close draft; more only as a chain.
- **Checkpoints at the stop, not on the hour.** Commit and STATE per slice; report skeleton and close draft committed before the predicted
  stop, so a resume past CAP only verifies and tags. Stop after 2-3 automatic continuations of one stuck task [A1].
- **Two builders, disjoint context** ([A30]: work "that needs little from the rest"); critics judge against stated thresholds and mark
  findings blocking / optional ([A19]: a gap-hunting reviewer "will usually report some, even when the work is sound").

## 3. Usage-limit stops: what is documented, and a resume-safe prompt
- Auto-continue [A15]: on by default in interactive subscription sessions (v2.1.234+); at the reset "Usage limit reset · continuing
  automatically", Claude is prompted "to pick the task up where it stopped. It doesn't resend your last message." It "re-arms the wait on
  its own at most twice in a row". The wait ends without continuing if Claude Code exits (no restart on resume), on /clear, /resume or
  /login, with the setting off, or if "a UserPromptSubmit hook that blocks the continuation prompt" fires; a permission prompt can stall the
  continued task; a PC asleep over 30 min across the reset waits for Enter (the power plan flips silently on this laptop: CLAUDE.md).
- /goal: a usage limit gives "Goal paused" and work resumes when auto-continue fires [A8]. Background subagents need SendMessage [A10];
  background shells keep running (n68's chain computed through stop 1 [L]) unless reaped (section 1). The reset time, not the night's
  clock, decides when work resumes: n68's second resume came after CAP [L].
- Resume-safe recipe: (1) a StopFailure `rate_limit` hook and a Notification `quota_auto_resume_fired` hook append times to
  Logs/n69/stops.txt; (2) a NON-blocking UserPromptSubmit hook (exit 0, `additionalContext`) prints once, when stops.txt has an unread
  resume line: clock, phase, minutes to CAP, STATE's next steps, agent IDs (firing on the continuation is implied by [A15]: not checked);
  (3) builders checkpoint per slice (the 2.1.299 forget bug); (4) phase rules (rule 3); (5) work owed by CAP launched before the stop as a
  detached, marker-writing chain. Not the retry watchdog [A21]; not third-party limit watchers on an undocumented endpoint ([C3]).

## 4. Five testable rules for the 4 h prompt
1. **Open a fresh window and schedule against its stop.** No Claude use for 5 h before t0 (the forge ends by then) or t0 = a reset time;
   the clock prints `window from HH:MM · predicted stop HH:MM (+2:20) · CAP HH:MM`; PC, the report skeleton, the close draft and the stop
   chain are committed or launched by predicted stop - 0:20. Test: each item's commit or log time is earlier than stops.txt's stop line.
2. **Resume by hook, not by memory.** The stops.txt hooks and the non-blocking UserPromptSubmit brief; the first turn after a resume reads
   the clock, `git log -1` and STATE before new work and sends one SendMessage per builder by ID. Test: stops.txt has a stop and a resume
   line; the transcript shows the brief and the three reads in that turn; every builder messaged within 5 minutes.
3. **The resume knows its phase.** Resume at or past CAP - 0:30: the pre-committed minimum close only (read the chain's outputs, report,
   tag); no new build, round or reel. Test: no build / round / reel start in the log after CAP - 0:30; close-check passes `--minimum`.
4. **Long jobs detached, ended by a marker.** Any job over 10 minutes runs outside Claude Code's process tree, writing its log and an
   `<job>.exit` marker; a "running low on memory" notice is checked against the process list and the marker before any re-run. Test: every
   long job in the log has a marker file; no job re-run while its first process was alive.
5. **The prompt fits the window.** At most 150 lines; end condition, schedule and at most 8 numbered rules in the first 25 lines, each with
   a reason or a hook; one glossary. Test: `wc -l`; a rule count; a fresh subagent given only those 25 lines answers when the night ends,
   what happens at the stop, what a resume after CAP does, who owns which files.

## 5. The go-beyond experiment
**Critique of the candidate (next action + each room's missing GATE pieces).** (a) It targets a loss n68 did not show: resume-to-action was
immediate and no room failed for a missing piece; they failed M2 VERB [L], so its effect is near zero and hard to measure. (b) A presence
list invites box-filling; n68's FAILs (I2 twice, g7leak) were critic judgments: print "presence only, not a verdict". (c) A rule-made next
action goes stale and gets followed anyway: print its inputs (STATE time, HEAD sha) and STALE past 30 minutes. (d) n68-slate.cjs, the gate
pack and STATE.md hold most of it: extend the slate script, not a fourth source of truth.
**Proposal E69, the window pacer (the candidate made window-aware).** It also prints the predicted stop (window start + 2:20, three
measured windows) and minutes left, orders must-ship items before it, and flags any not done at stop - 0:30; if the t0 statusline dump
shows `rate_limits.five_hour`, it uses `used_percentage` / `resets_at` instead [A32]. Hypothesis: every must-ship item (PC, report
skeleton, close draft, the detached stop chain) lands before the first stop; on n66 and n68 a stop ate the close or PC [L]. Measure: (1)
must-ship items before the stop, from stops.txt and git times (target 4 / 4; n68: PC and the report missed); (2) predicted minus actual
stop, minutes (calibrates the 2-builder window); (3) median minutes from an evidence file landing to the next dependent action, scripted
from the log (n68 baseline). One night is one sample. Rejected: effort by role (builders `high` via the Agent tool's `effort` [A10]) is
confounded tonight by three builders becoming two; keep it for a night with an unchanged team.

## Sources (all https://, fetched 2026-10-25)
- A1 platform.claude.com/docs/en/build-with-claude/prompt-engineering/prompting-claude-opus-5-5 · A8 code.claude.com/docs/en/goal
- A10 .../sub-agents · A11 .../model-config · A15 .../interactive-mode · A19 .../best-practices · A20 .../changelog (2.1.299-2.1.310)
- A21 .../env-vars · A28 .../costs · A29 .../errors · A32 .../statusline · A33 .../hooks ("..." = code.claude.com/docs/en)
- A23 www.anthropic.com/engineering/harness-design-long-running-apps (2026-03-24) · A34 www.anthropic.com/engineering (index)
- A30 claude.com/resources/articles/building-agents-when-to-fold-subagents-into-your-main-agent-loop (2026-10-22)
- A35 support.claude.com/en/articles/11647753 · R2 arxiv.org/abs/2507.11538 (IFScale; NeurIPS 2025)
- C2 github.com/anthropics/claude-code/issues/103722 (user report, open) · C3 pypi.org/project/claude-powernap (community tool, 0.3.2)

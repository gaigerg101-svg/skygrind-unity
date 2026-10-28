# Prompt methods for an 8-hour, two-window night (FORGE70 prompt-method scout, 2026-10-25 ~21:46 MST)

Scope: only what is new since Research/2026-10-25-forge69/prompt-methods.md (and -forge68), what this morning's note got wrong, and
NIGHT69's measured facts. Labels: [A#] official (URL in Sources), [C#] community, unverified, [L] read or measured here (file named).

## 0. The measured frame: the seat, not the night, sets the windows
- [L] Resets read back from stop messages chain at exact 5:00 steps: **02:50, 07:50, 12:50** (NIGHT68-LOG.md "reset at 02:50 local",
  "reset before 9:01" = 07:50; NIGHT69-LOG.md "your session limit resets 12:50pm"). When a session waits and auto-continues at a reset,
  the next window starts at that reset; the first window starts at the seat's first request after a reset (FORGE68 ~21:50 -> 02:50).
- [L] Every measured "2:20 window" was shared by several heavy sessions on this seat (playbook library, 2026-10-24/25 entries):
  - W1 21:50-00:03 (2:13): FORGE68 + NIGHT68 + APEX S30 + investing S9, all three nights started at 22:50.
  - W2 02:50-05:14 (2:24): the same three nights. APEX S30's pauses (00:08-02:51, 05:03-07:51) match NIGHT68's (00:03-02:50,
    05:14-07:50) within 11 min; investing S9 "two usage stops cost 5:30".
  - W3 07:50-12:05 (4:15 wall): NIGHT68's minimum close (07:51-08:01, git), FORGE69 (09:00-09:53), NIGHT69 (09:55-12:05, 2:10 of its
    own work) and APEX S31 (09:55-13:42, "one 48-minute usage pause", so ~12:02-12:50).
  - W4 12:50-17:50: NIGHT69's close (12:51-13:25), APEX S31's tail (to 13:42), investing S10 (12:52-16:40). **No stop in 5 h.**
- So 2:20 is a three-concurrent-nights figure, not what SKYGRIND gets alone (not measured); the stops' ends (the resets) are predictable.
  NIGHT69's window was not fresh (it began 07:50 with NIGHT68's close, FORGE69 inside it): this morning's rule 1 was never met.
- [L] NIGHT68 (8 h, t0 22:50:23) got 3:37 of work (1:13 + 2:24); its stops cost 2:47 + 2:37 = 5:24. CAP 8:00 fell in stop 2 and the
  resume came at 9:01: PC, the report and the full close were lost (NIGHT68-REPORT.md section 8). NIGHT69's stop was 0:45, resumed from
  STATE at once ("no agent id was needed"; no compaction; Logs/n69/STATE.md rewritten 7 times).
- [L] Tonight: FORGE70 and APEX S32's forge were both requested at ~21:44 for 8 h nights. If nothing used the seat 17:50-21:44 (not
  checked), resets fall at ~02:44 and ~07:44. With t0 ~22:45 that is NIGHT68's exact shape: block 1 of ~1:00-1:15 (shared with APEX
  S32), stop 1 to t0+4:00, block 2 of ~2:15, stop 2 to t0+9:00 (holding CAP).

## 1. New since this morning (or corrected)
- **Correction, the stop's name:** NIGHT69's stop read "session limit", not "individual spend limit"; the spend-limit variant appears
  when a plan window ran out and usage credits are capped [A2]. Both are the same 5 h window and both name a reset.
- **Correction, concurrency:** the docs say only "a per-seat allowance that resets on a rolling five-hour window and a weekly window",
  shared with chat and Cowork [A1]; no official text says whether parallel sessions drain it faster [C1]. The [L] rows show they share
  one window and stop together: concurrent sessions are the first-order term in block length.
- **A pre-stop signal and a meter:** "Before a window runs out, Claude Code can warn you ... `You've used 85% of your session limit ·
  resets 3:45pm`" [A2]; whether the model sees that line: not checked. `/usage` shows plan limits, reset times and a 24 h / 7 d
  attribution "computed from local session history on this machine" [A1], so GG can read the SKYGRIND vs APEX share after the night.
- **The weekly window:** usage counts against it at the same time, and "a single burst of heavy activity ... can exhaust the weekly
  allowance before the session window resets" [A2]. Auto-continue does not wait for a reset more than 24 h away [A3]. The seat ran ~6
  night-scale sessions on 10-24/25 [L]; weekly %: not checked. A weekly stop ends the night with no resume.
- **Auto-continue details not used before** [A3]: the wait shows `Continuing automatically at HH:MM`; it "re-arms the wait on its own
  at most twice in a row" (three sessions resuming at one reset burn the new window together); a sleep over ~30 min across the reset
  needs Enter; a permission prompt stalls the continued task; a cancel suppresses the auto-wait "for that reset window".
- **A time budget raises the burn rate:** "lowering effort reduces the work itself, whereas a budget mostly keeps more agents working in
  parallel" [A4]. On a usage-bound seat, a tight `elapsed / budget` line trades window minutes for wall minutes.
- **Subagents** [A5]: "Resuming starts a new run of the agent under the same ID"; the concurrent cap is 20
  (`CLAUDE_CODE_MAX_CONCURRENT_SUBAGENTS`); `CLAUDE_CODE_SUBAGENT_MODEL` + `_FORCE=1` puts every subagent on one model (a setting: GG's).
- **/goal** [A6]: "Goal paused" on a usage limit, resumed by the auto-continue; at most three idle check-ins per goal between prompts (two
  long detached chains can spend them); `CLAUDE_CODE_GOAL_CHECKIN_MINUTES=0` turns check-ins and retries off.
- **Idle costs** [A1]: goal check-ins, `/loop` tasks and cross-session messages each "send your full context" while idle (hold the last
  with `crossSessionInbound: hold`). **Nothing newer:** changelog 2.1.310 (10-23) [A7]; engineering 10-13 [A8]; Opus 5.5 guide [A4].
- **Peak hours:** "removing the peak hours limit reduction on Claude Code for Pro and Max" (2026-05-06) [A9] names no Team / seat plan. A
  community claim of a weekday peak at 5-11 AM PT [C1] would be 05:00-11:00 MST (until Nov 1), so Monday 05:00 could clip block 2: not
  checked.

## 2. What an 8 h two-window prompt must do that a 4 h one did not
- **Plan from the reset times, not a guessed stop.** The resets are certain (W0+5:00, W0+10:00); the stop moment depends on the other
  sessions. Each act's must-ship list is due by the worst case: block 1 = 1:00 with APEX S32 alongside.
- **CAP lands inside stop 2 in every measured shape** (t0 = forge+1:00: resets t0+4:00, t0+9:00; t0 = a reset: stop 2 ~t0+7:15, reset
  t0+10:00). So the full close (report, HANDOFF, memory, tag) is committed in block 2; after stop 2 only a scripted finish runs.
- **The stops are the night's biggest compute budget** (NIGHT68: 5:24 of stop vs 3:37 of work). Deterministic sims run detached with no
  agent. a1 (3 rooms x 20 seeds, alone) took 26:39 (10:40:37-11:07:16, NIGHT69-LOG.md); a 40-seed pool of 3 rooms x 2 candidates is
  ~1:47 and fits a 2:40 stop. It also answers L-151's noise: 20 seeds give ~0.08, 40 about 0.057 if noise scales as 1/sqrt(n) (assumed).
- **Close-ready at every act boundary.** A weekly hit or a third re-arm ends the night with no resume, so at each predicted stop the
  committed tree already passes a `--minimum` close.
- **Fewer concurrent agents while another overnight runs.** Every agent draws on the same per-seat allowance [A1]; three builders +
  critics + APEX S32 on one window repeats W1's 1:13.

## 3. Six testable rules for NIGHT70
1. **Read the window at kickoff.** GG runs `/usage` before the `/goal` line and pastes the session %, reset time and weekly %; STATE line
   1 = `W0 · R1 · R2 · weekly N% · concurrent: <sessions GG names>`. Why: stops end at Rk; a weekly stop never resumes [A2, A3]. Test:
   STATE line 1 has R1 / R2; every resume line in the log is within 5 min of R1 or R2.
2. **Two acts keyed to R1, each with a stop package.** By t0+1:00 Act 1 has the detached chain launched (with its own commits and
   markers), the builders' slices committed, and STATE and the report skeleton committed; Act 2 starts at R1. Why: n68 lost PC and the
   close to stop 2; n69's stop, with no chain running, computed nothing. Test: chain start and last pre-stop commit both precede the stop
   line; last commit to stop <= 15 min.
3. **The full close is committed by R1+2:00.** Past stop 2 or CAP only `close-check` and the clock stop run (<= 10 min). Why: CAP falls
   in stop 2 in every shape. Test: the tag time precedes stop 2's line; no build, round or agent appears in the log after CAP.
4. **Size the stop chain to the stop.** Act 1's chain runs >= 1:45 of deterministic pools (40 seeds, fresh sets) with no agent, writes
   `<job>.exit`, commits its own outputs (as n68's e61765e4 did), and is checked against the process list before any re-run. Why: 5:24
   of n68's night was stop. Test: the chain's commit times fall inside the stop interval.
5. **Budget the seat, not just the clock.** While another night shares the seat: at most 2 builders + 1 critic at once, builders at
   effort `high` [A5], and the lead's `elapsed / budget` line kept loose (budget = R1+2:00, not CAP). Why: a budget raises parallelism
   [A4] and the window is shared [L]. Test: peak live agents in the log <= 3; block 1 measured against n68's 1:13 and W2's 2:24.
6. **The first turn after a reset prints the phase.** Before any tool work it prints `phase: Act 2 | post-CAP finish`, the clock,
   `git log -1`, STATE and the chain markers, then sends one SendMessage per failed builder by ID, each told to commit before any job
   over 10 min (the 2.1.299 resume-forget bug). Why: auto-continue "doesn't resend your last message" [A3]; a resume starts a new run
   [A5]. Test: the turn after each "Usage limit reset" line holds those reads; every builder is messaged within 5 min.

## 4. The go-beyond experiment: E70, the reset-keyed two-act prompt
- **Change to the writing:** no single schedule against CAP. NIGHT70.md is written as Act 1 (t0 to R1), Act 2 (R1 to R1+2:00) and a
  Finish, each with its own must-ship list, stop package and one-screen resume brief, the times filled from GG's `/usage` line at
  kickoff (rule 1). The seed pools are written as stop work, not block work.
- **Hypothesis:** work lost at stops falls to ~0, and the stops become compute instead of dead time.
- **Numbers (log, git times, chain logs):** (1) uncommitted minutes at each stop, last commit to stop line (n68: stop 2 ate PC, the
  report, the close); (2) detached compute minutes inside the stops (n69: 0); (3) close minutes after CAP (target <= 10; n68 ran the
  whole minimum close from 9:01); (4) work minutes per block, beside `/usage`'s 24 h attribution split for SKYGRIND vs APEX [A1].
- **My critique:** (a) block length depends on APEX S32 and anything else GG runs, so (4) is confounded; only (1)-(3) test the writing,
  and one night is one sample. (b) If the seat is quiet and no stop comes, the stop package costs ~15 min for nothing; fix: Act 2 starts
  at once and the package doubles as Act 1's checkpoint. (c) Rule 1 needs GG at kickoff (one command); without it W0 falls back to the
  forge's first request (~21:44), a guess. (d) The 40-seed chain may still miss the GATE: E70 measures lost and used time, not room
  quality, so the rooms' PASS rate is not its result.
- **Rejected:** a statusline `rate_limits` pacer. The field is only for Pro / Max or behind a gateway [A10], and this is a seat plan.

## Sources (https://, fetched 2026-10-25 ~21:30-21:46; no personal data sent)
- A1 code.claude.com/docs/en/costs · A2 .../errors (#youve-hit-your-session-limit, #youve-hit-your-monthly-spend-limit) · A3
  .../interactive-mode (#wait-for-a-usage-limit-to-reset) · A5 .../sub-agents · A6 .../goal · A7 .../changelog · A10 .../statusline
- A4 platform.claude.com/docs/en/build-with-claude/prompt-engineering/prompting-claude-opus-5-5 · A8 www.anthropic.com/engineering ·
  A9 www.anthropic.com/news/higher-limits-spacex (2026-05-06) · "..." = code.claude.com/docs/en
- C1 search summary of third-party guides (www.datacamp.com/tutorial/claude-code-usage-limits, portkey.ai/blog/claude-code-limits,
  chudi.dev/blog/claude-usage-limits): window opens at the first prompt, pool shared across surfaces, the peak-hour claim
- [L] ~/.claude/skills/game-prompt-playbook/library/ 2026-10-24_apex_s30*, _investing_s9*, 2026-10-25_apex_s31*, _investing_s10*,
  _apex_s32*; NIGHT68-LOG.md; NIGHT69-LOG.md; NIGHT68/69-REPORT.md section 8; Logs/n69/STATE.md; `git log` times on 10-25

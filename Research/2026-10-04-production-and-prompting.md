# Production and prompting research (2026-10-04)

The question: what a one-owner team needs in place to keep improving SKYGRIND through a Next Fest demo and Early Access,
and how to prompt Claude Opus 5.5 for 6-12 hour nights. This builds on `game-prompt-playbook/PLAYBOOK.md` and doesn't
repeat its sections 2-4 (boxes, caps, light testing, stop rules, checklist).

How it was gathered: web fetches only, on 2026-10-04. The session's web-search budget ran out after 3 searches, so nearly
every source was fetched by its URL. Anthropic's pages (Part 2) came back as raw text, so their quotes are exact. Most
Part 1 pages came back through the fetch tool's summarizer, so treat those quotes as close paraphrase. Anything not read
this session is marked "not verified". In Sources, [W] means fetched and [S] means search snippet only.

## 0. Bottom line (ranked for this team)

1. **Pick the Next Fest now; the store page is the critical path.** Feb 2027: Feb 22 to Mar 1; register by Jan 10;
   submit demo and page by Jan 25 (Feb 11 press preview), Feb 8 at the latest; one Next Fest per game. Zukowski: momentum
   in the 2 weeks before beats the base count (r 0.61 vs 0.47); low base plus low momentum hit 3,000 fest wishlists in
   3 of 47 games. My reading: Feb only if a public page collects wishlists from this autumn; else June 2027 (not verified).
2. **Fix the worktree base before the next parallel night.** Worktree subagents branch from the default branch (`main`),
   not `port/stage3`, unless `worktree.baseRef` is `"head"`, and they take instructions from the lead, not the worktree.
3. **Make the `/goal` line always satisfiable and judgeable from the transcript.** The evaluator is the small fast model;
   it reads only the conversation, runs nothing, and can clear the goal as "impossible". Use an either-or (done, or short
   with a reason, or a `date` time) plus a fixed-format evidence line every turn.
4. **Opus 5.5 defaults to `medium` effort** (Opus 5: `high`), which Anthropic says matches Opus 5 at high on coding. Run
   nights at medium or high and compare; xhigh and max only for a measured gain; `effort: low` for log-reading subagents.
5. **Anthropic's unattended-run paragraph goes in the system prompt from turn one** (`--append-system-prompt-file`). It
   names the four early stops Opus 5.5 makes. Delete "double-check" / "verify with a subagent" lines (over-verification).
6. **Local logs now; Sentry before the first outside tester.** Player.log, a per-run JSONL of existing gameplay events,
   a "copy report" button. Sentry free: 5k errors a month, one user, 30 days; Windows native crashes via crashpad (ship
   `crashpad_handler.exe`); `SendDefaultPii` off by default. Unity Diagnostics needs GG's open cloud-services decision.
7. **Playtest in rounds of five.** Nielsen: 5 users find about 85% of usability problems; 3 rounds of 5 beat 1 of 15.
   Steam Playtest is a separate child app: no reviews of the game from it, wishlists untouched.
8. **CI with GameCI; Steam uploads to a beta branch.** IL2CPP needs a `windows-2022` runner; Windows minutes cost $0.010
   after 2,000 free a month; self-hosted is free but shares the bench laptop. Steam's default branch is set live by hand.
9. **Version the save file, with a migration test in Core, before any outside tester keeps a save.**
10. **Steam Deck Verified: text at least 9 px at 1280x800 (12 recommended), full controller access.** SKYGRIND's 16 px at
    1080p is about 11.9 px (height match) or 10.7 px (width match): legal, under the recommendation (scaler not checked).
11. **Steamworks.NET (MIT, SDK 1.64, Aug 2025) is the safe wrapper.** Unity's Platform Toolkit lists a
    `com.unity.platformtoolkit.steam` package (1.0.2: accounts, achievements, saves); its wider coverage is not verified.
12. **Stop hooks cap at 8 blocks in a row without progress.** Check `stop_hook_active`; raise with
    `CLAUDE_CODE_STOP_HOOK_BLOCK_CAP` only if needed. `/goal` is itself a prompt-based Stop hook.
13. **Steam's mandatory content survey asks about AI content "consumed by players"**; AI dev-tool "efficiency gains" are
    "not the focus". AI-written code isn't the subject; AI-made art or text on the page or in the demo would be.
14. **One page each:** pillars and a one-page GDD, an ADR-style decision log (HANDOFF's D-xx with a status), a risk list,
    and a milestone plan worked back from the fest date.

# Part 1: what a small team needs in place

## 1. Playtesting

- **Steam Playtest:** "a separate 'child' appID", shown as a section on the base game's page. Limited access (grant
  "in batches as you have capacity") or Open; testers picked "at random". A Playtest-only player "cannot review your
  actual game"; wishlists aren't affected. Valve: "a big 'feedback' button right on the main menu". End it by marking it
  "Not playable", after telling players.
- **How many:** Nielsen (2000): about 85% of problems with 5 users (found = N(1 - (1 - L)^n), L about 31%); "Spend this
  budget on 3 studies with 5 users each!"; 3-4 per group for two groups (FPS veterans, movement-shooter newcomers).
- **What to measure:** progression events (start / complete / fail, up to 3 levels) are "ideal for building funnel
  views, measuring drop off" (GameAnalytics). Slay the Spire "took a metric-driven focus early in development" through
  early access (GDC 2019). Demo play-to-wishlist median is 19.3% (Zukowski): "Do not make gameplay changes based on
  this data."
- **Not verified:** indie-shooter postmortems (no search budget left), Discord community practice, form templates.
- **First-session funnel** (my design, with PORT-STATUS.md's event names): boot, menu, settings touched (sensitivity,
  FOV), run start, first kill, first down, wave 3, wave 5, run end (wave, cause), a second run, quit point.

**For SKYGRIND:** (1) one night: a per-run JSONL in the persistent data path, a run-end summary, "open logs folder" and
"copy report" buttons (new keys in UnityTuning; no gameplay numbers change); (2) round 1 before the store page: 5 people,
a zip over Discord, one live session each, five questions (what is this game, where stuck, when wanted to quit, what felt
best, play again?), about 5 hours of GG's time; (3) round 2: 5 new players, logs only; (4) round 3: Steam Playtest,
limited, once the page is public (25-50 per batch, my suggestion). A round every 2-3 weeks on a tagged build; the
following nights take its findings as P0.

## 2. Crash and error reporting, telemetry

| Option | Windows Player coverage | Cost | Notes |
|---|---|---|---|
| Unity Diagnostics (6.2+) | "Crash reports", "Exception logs", "Telemetry" | not verified | Replaces Cloud Diagnostics ("deprecated, and will be phased out"); "must enable data collection in the Editor"; "you are responsible for collecting valid consent" |
| Sentry for Unity | C# errors; native via "the crashpad backend"; Debug.Log breadcrumbs; screenshots opt-in | Free 5k errors a month, 1 user, 30 days; Team $26 a month (annual) | Ship `crashpad_handler.exe`; symbol upload needs a token; `SendDefaultPii` / `AttachScreenshot` false; offline cache; `BeforeSend` can drop events |
| BugSplat | C# exceptions; minidumps via `UnityCrashHandler`; IL2CPP symbols; user feedback | not on page | OpenUPM or git package |
| Backtrace | Managed exceptions, crashes; Unity 2021.3+ | free trial | Windows native capture not stated: not verified |
| Local-first | `%USERPROFILE%\AppData\LocalLow\<Company>\<Product>\Player.log`; `CrashReporting.crashReportFolder` | free | The player decides what to send |

**Privacy:** the EU counts "an Internet Protocol (IP) address" and "a cookie ID" as personal data, and re-identifiable
pseudonymised data "remains personal data". So an install ID is personal data: telemetry opt-in plus a privacy policy.
Logs can hold Windows user-folder paths; scrub them before sending.

**For SKYGRIND:** (1) now: local-first logs with version and git hash in each header; (2) before the first outside round:
Sentry free with PII and screenshots off, a path-scrubbing `BeforeSend`, an opt-in toggle (about one night; Mono vs
IL2CPP symbols not checked); (3) Unity Diagnostics only if GG turns cloud services on (it uses the Unity Connect
settings the repo keeps unstaged); (4) before EA: a privacy-policy page and a data-deletion contact.

## 3. Build and release hygiene

- **GameCI:** `unity-test-runner`, `unity-builder`, activation. Personal licence secrets `UNITY_LICENSE` (.ulf),
  `UNITY_EMAIL`, `UNITY_PASSWORD`; Pro uses `UNITY_SERIAL`. "IL2CPP builds require the base operating system to match
  the build target". Cache `Library` on Assets / Packages / ProjectSettings hashes.
- **Costs:** GitHub Free: 2,000 private minutes a month, then Windows $0.010 and Linux $0.006 a minute; 10 GB cache per
  repo; self-hosted free. Unity Build Automation pricing: not verified (page 404).
- **Tests:** `Unity.exe -runTests -batchmode -projectPath <p> -testResults <xml> -testPlatform EditMode`, on CI's own
  checkout (CLAUDE.md's no-`unity test` rule is about the live Editor).
- **Steam uploads:** steamcmd `+run_app_build`; `"SetLive"` works on a beta branch, but "the 'default' branch can not be
  set live automatically". `game-ci/steam-deploy` wants a builder account with only "Edit App Metadata" and "Publish App
  Changes To Steam". Branches can be password-protected.
- **Changelog players see:** Steam Events show in the library; the last two show on the store page. Valve: "If you
  don't have a reason to post an announcement, don't post one".
- **Saves:** Auto-Cloud uses root paths, patterns and quotas; avoid "machine-specific configurations such as video
  settings". A demo save "can be stored to the full game's cloud storage". Migrations: common practice (no source).

**For SKYGRIND:** (1) a version stamp `0.<milestone>.<build>+<hash>` on the title screen, in logs, and as the Sentry
release; (2) `saveVersion` plus ordered migrations in Core, one EditMode test per migration on a fixture; (3) CI v1: the
501 tests, golden and replays on Linux per push to `port/*` (needs a GitHub remote; pushing is GG's call, rule 1);
(4) CI v2: a manual Windows build, then `-skygrindGolden` (`-skygrindSmoke` on a GPU-less runner not checked; a
self-hosted runner would spoil benches); (5) uploads to a password-protected `playtest` branch, GG promotes by hand,
with a patch-notes event from a CHANGELOG.md.

## 4. Steam readiness

- **Fee and timeline:** $100 per product; "A 21-day waiting period between when you paid the app fee and when you can
  release"; a coming-soon page public "at least two weeks"; reviews "1-5 days". Onboarding time: not verified.
- **Assets:** header 920x430, small 462x174, main 1232x706, vertical 748x896, background 1438x810 (optional), library
  capsule 600x900, library header 920x430, hero 3840x1240 (safe 860x380), logo 1280 wide and/or 720 tall. No text but the
  title on capsules. At least 5 screenshots at 1920x1080 that "should exclusively show the gameplay".
- **Trailer:** the first one should be "primarily gameplay"; "The first two valid trailers will be shown to players
  before any screenshots"; up to 1920x1080, 30/60 fps, 5,000+ Kbps; may play "without any audio" (good: no audio yet).
- **Tags:** up to 20; "top 5 tags should paint a fairly clear picture"; "some store filters prioritize the first 15".
- **Next Fest:** "ONE Next Fest" per title; base page "published and public"; demo review 3 weeks before (general page),
  Jan 25 / Feb 8 (Feb 2027 page); demo live "at least 30 minutes before the start"; keeping it up after is encouraged.
- **Wishlists (Zukowski):** Cat Mail Co. entered with 5,293 and gained about 42,000. June 2026 had "4000+" games;
  entrants with 1,000+ wishlists gained a median of about 1.2-1.5K. First games: "Base-maxx. You will fail the first
  time." His visibility-tier figures (Popular Upcoming "Silver" 7,000-10,000) came through the summarizer: read first.
- **Demo:** a separate App ID (with its own page it gets reviews); Valve weighs quality, timing ("We generally recommend
  waiting until launch") and length.
- **Steam Deck:** Verified / Playable / Unsupported / Unknown. Verified: default controller config reaches all content,
  prompts match the device, text "should never fall below 9 pixels in height at 1280x800", no launcher blockers.
- **Achievements and cloud:** 100 achievements at first (`ISteamUserStats`); Auto-Cloud needs no code.
- **SDKs:** Steamworks.NET (MIT, "100% coverage", 2025.164.1, Aug 2, 2025); Facepunch.Steamworks (MIT, Unity and
  IL2CPP, async); Unity Platform Toolkit 1.0.1 ("Account management, Achievements, Save data") plus
  `com.unity.platformtoolkit.steam` 1.0.2. Its announcement date and Steam coverage: not verified.
- **Ratings and AI:** the Content Survey "must be completed prior to submitting the game to the Review Process" and
  generates "ratings for several regional rating boards"; it asks about AI content "that ships with your game and is
  consumed by players". A separate IARC certificate for Steam: not verified. EULA and privacy-policy rules: not verified.

**For SKYGRIND:** (1) pay Steam Direct, finish onboarding; (2) draft the page: GG's capsule art in the dusk palette, 5+
gameplay screenshots, a gameplay trailer that works muted; (3) page public as early as GG accepts; (4) content survey,
AI section for shipped content only; (5) Steamworks.NET for achievements and Auto-Cloud; (6) a demo app (one map, the
first waves, same golden checks); (7) a Deck review after controller support (see `Research/2026-10-04-controller.md`).

## 5. Planning documents

- **One-page design** (Librande, GDC 2010): "why create a document with more than one page if most people only read
  the first page anyway?"
- **Decision log** (Nygard 2011): Title, Context, Decision ("We will..."), Status (proposed / accepted / deprecated /
  superseded), Consequences ("not just the 'positive' ones"); "one or two pages long"; superseded records are kept.
- **Scope** (Derek Yu 2010): "The last 10 percent is really 90 percent"; cut features; don't restart.
- **Pillars, risk lists, producer advice for tiny teams:** no source fetched (not verified); below is standard practice.

**For SKYGRIND** (GG writes the first two): `PILLARS.md` (three one-sentence pillars; every night's in-scope list cites
one); `GDD-1PAGE.md` (loop, mode, map, GG's enemies, demo slice); `DECISIONS.md` (the D-xx entries as ADRs with a
status); `RISKS.md` (top five with trigger and mitigation: the fest date slips, no art or audio pipeline, lower-end PC
performance near the 6.9 ms bar, one tester, AI-night regressions); `priority` and `demo_critical` in tasks.json.

| Date (Feb 2027 path; for June shift about 4 months) | What |
|---|---|
| Oct 2026 | Steam Direct paid; pillars and GDD; run log; playtest round 1 |
| Nov 2026 | Store page public; Sentry; round 2 |
| Dec 2026 | Demo scope frozen; Steam Playtest (round 3); CI v1 |
| Jan 10 / Jan 25 | Register / submit demo and page |
| Feb 8-22 | Momentum push (posts, streamers, festivals); fest opens Feb 22 |

## Requirements checklist

| # | Requirement | Why | Source | When |
|---|---|---|---|---|
| 1 | Choose Feb or June 2027 Next Fest | One fest per game; sets every date | Next Fest docs | now |
| 2 | Pay Steam Direct ($100), finish onboarding | Needed for a store page | Steam onboarding | now |
| 3 | GG sets `worktree.baseRef: "head"` | Worktree agents otherwise start from `main` | CC worktrees | now |
| 4 | /goal line with report-short and clock branches plus an evidence line | Evaluator sees only the transcript; can say "impossible" | CC /goal | now |
| 5 | Local run log, version stamp, "copy report" | Testers attach evidence; no privacy surface | Unity log files | now |
| 6 | PILLARS.md, one-page GDD | Scope filter for night prompts | Librande | now |
| 7 | DECISIONS.md, RISKS.md, priorities in tasks.json | Keeps nights aimed at the demo | Nygard | now |
| 8 | `saveVersion` and migration tests | Outside saves survive updates | practice (not sourced) | before round 1 |
| 9 | Playtest round 1 (5 people, live) | 5 users find most issues | Nielsen | before the demo |
| 10 | Sentry or BugSplat, opt-in, PII off, path scrub | Crashes on other PCs | Sentry docs; EU GDPR | before round 2 |
| 11 | Public store page: capsules, 5+ shots, trailer, 20 tags | Wishlist base and momentum | Steam docs; Zukowski | before the demo (early) |
| 12 | Content survey incl. AI section | Mandatory before review | Steam content survey | with the page |
| 13 | Privacy policy page | Install IDs are personal data | EU Commission | before telemetry |
| 14 | CI v1: EditMode tests on push | Catches AI-night regressions | GameCI; Unity test CLI | before the demo |
| 15 | Beta branch, steam-deploy with a builder account | Safe uploads; default stays manual | SteamPipe; steam-deploy | before the demo |
| 16 | Steam Playtest (limited) | Wider test without reviews | Steam Playtest | before the demo |
| 17 | Demo app, scope frozen | Fest requirement | Steam demos; Next Fest | before the demo |
| 18 | Demo submitted by Jan 25 (Feb path) | Press preview | Feb 2027 page | before the demo |
| 19 | Momentum plan for the last 2 weeks | Momentum beats base | Zukowski Jul 2026 | before the demo |
| 20 | Steamworks.NET achievements, Auto-Cloud | Expected; demo save carry-over | Steamworks docs | before EA |
| 21 | Controller support and Deck review | Verified badge; 9 px text | Steam Deck doc | before EA |
| 22 | Patch-notes events; CHANGELOG.md | Players see changes | Steam event tools | before EA |
| 23 | CI v2: Windows build plus golden check | Build-only failures | GameCI | before EA |
| 24 | EULA decision | Legal | not verified | before EA |

# Part 2: prompting Claude Opus 5.5 for long autonomous runs

## 2.1 What's specific to Opus 5.5 (and what it inherits from Opus 5)

- **Effort:** "Start at `medium`, the default on Claude Opus 5.5 (Claude Opus 5 defaults to `high`)"; "tends to think
  more per turn than Claude Opus 5"; "Reserve `xhigh` and `max` for work where you've measured a quality gain." The
  table's xhigh row ("over 30 minutes") is overridden: "The per-model recommendations ... override this table". `low`
  suits "Simpler tasks ... such as subagents". In Claude Code: `--effort`, `/effort`, `CLAUDE_CODE_EFFORT_LEVEL`.
- **Early stops:** "Treat a text-only end of turn as a report rather than as proof the task is done." The model "is
  responsive to instructions that name the specific kinds of early stop". Anthropic's paragraph names four: a summary
  that "closes by announcing the next step and has no tool call"; "an offer to carry on ... unless the user would prefer
  otherwise"; "a list of decisions ... none of them blocks the rest of the work"; "deciding that this is a good place to
  report". Wanted stops: "where nothing can move without them". "Add it at the end of your system prompt from the first
  request of the session" (later changes invalidate earlier thinking). Claude Code keeps the first request's system
  prompt "until the conversation is compacted", so pass it with `--append-system-prompt-file` at launch.
- **Continuations:** "stop after two or three automatic continuations on the same task ... so that a run that is
  genuinely stuck ends". Playbook form: three failed attempts on a task means mark it short.
- **Over-verification** (Opus 5): "verifies its own work without being told to"; explicit verification instructions
  "cause over-verification"; "Avoid instructing re-checks it already performs". Scope line: "Deliver what was asked, at
  the scope intended."
- **Subagents:** "Delegate ... only for large tasks that are genuinely independent and parallelizable ... do not use
  subagents to verify or double-check your own work." Caps: `CLAUDE_CODE_MAX_CONCURRENT_SUBAGENTS` (default 20),
  `CLAUDE_CODE_MAX_SUBAGENT_SPAWN_DEPTH` (default 3).
- **Emphasis:** "dial back any aggressive language"; "If you emphasize many lines, none of them stands out."
- **Time:** the model "pays close attention to information about elapsed time" (e.g. `elapsed 340s / 1200s`); "The
  budget is advisory", and under pressure it "might search and verify a little less". Playbook `date` lines are the
  in-prompt version; an elapsed-time hook for Claude Code is not verified.

## 2.2 `/goal`: how it decides, and conditions that can't trap the night

- **Mechanism:** "a wrapper around a session-scoped prompt-based Stop hook"; after each turn it sends "the condition and
  the conversation so far to your configured small fast model": Not yet met, Met, or Impossible ("clears the goal and
  records a failed entry"). "Setting a goal starts a turn immediately, with the condition itself as the directive." Run
  it in auto mode to go unattended.
- **Evaluator:** "It doesn't run commands or read files independently, so write the condition as something Claude's own
  output can demonstrate." Good conditions have "One measurable end state", "A stated check", "Constraints that matter";
  up to "4,000 characters"; add "a turn or time clause".
- **Stalls:** "no tool use for several turns in a row" stops the loop (goal kept). Running subagents or background shells
  defer evaluation; check-ins after 30 minutes (`CLAUDE_CODE_GOAL_CHECKIN_MINUTES`), "at most three idle check-ins per
  goal between your prompts". Resume keeps the condition but "resets the turn count, timer, and token-spend baseline".
- **Rules for SKYGRIND:** (1) an either-or that can always come true: done; or each open task "marked short in
  HANDOFF.md with the blocking reason and what was tried"; or "`date` shows 07:30 or later". Without these, a stuck
  golden mismatch is judged Impossible (goal cleared) or loops for hours. (2) Evidence that prints in the transcript:
  the `unity-loop.sh tests` totals line, `git status --short`, `date`; say "shown in this conversation". (3) A
  fixed-format line every turn, e.g. `GOAL-STATUS: 4/6 passing, 1 short, tests 501/501, 05:42`. (4) "Short" costs a
  named blocker (cap, GG-only decision, broken tool) and the attempt made. (5) Only 2-3 visible constraints (no commit
  on `master`, no push); the rest stay in the prompt.

## 2.3 Stop hooks: the cap and `stop_hook_active`

- "Claude Code overrides a Stop hook after it blocks eight times in a row without progress." "Parse the
  `stop_hook_active` field ... and exit early if it's `true`". "If your hook legitimately needs more than eight
  iterations to converge, raise the cap with `CLAUDE_CODE_STOP_HOOK_BLOCK_CAP`." A prompt hook's `"ok": false` feeds the
  reason back "unless the response also sets `"impossible": true`". Agent hooks "are experimental".
- Caveat: the env-vars page and the truncated hooks-reference fetch didn't show the cap; two GitHub issues (snippets
  only) describe the same 8-9 block override [S]. Don't stack a custom Stop hook on `/goal`: both fire every turn.

## 2.4 Subagents and worktrees

- **Worktrees:** subagent worktrees "branch from your repository's default branch unless `worktree.baseRef` is set to
  `"head"`"; a worktree subagent "takes the instruction files it starts with from your main conversation, not from its
  worktree". Unchanged worktrees are removed when the subagent ends; changed ones wait for the sweep.
  `.worktreeinclude` copies gitignored files (don't list Unity's `Library/`; its size was not checked).
- **Frontmatter:** `isolation: worktree`, `background`, `model`, `effort`, `maxTurns` (output "marked as partial"),
  `memory`. Background subagents get "a smaller built-in tool set"; results come "as a completion notification".
- **Briefs:** "Each subagent needs an objective, an output format, guidance on the tools and sources to use, and clear
  task boundaries." Multi-agent runs use "about 15× more tokens than chats".
- **Workflows and ultracode (new):** scripts that run many agents (default guideline under 10; 16 concurrent; 1,000 per
  run). Ultracode: "each request uses more tokens and takes longer", and it lifts the subagent limit. Keep it off on
  nights; it overrides the playbook's caps.
- **C compiler project** (16 agents, about 2,000 sessions, about $20,000): lock files in git for tasks; "new commits
  can't break existing code"; "The test harness should not print thousands of useless bytes."

## 2.5 Context over a 10-hour run

- 1M models "compact before the window fills, at about 967K tokens"; `CLAUDE_AUTOCOMPACT_PCT_OVERRIDE` only compacts
  earlier. Opus 5 stays consistent "throughout the window", yet Claude Code warns "performance degrades as it fills":
  keep verbose Unity output in subagents or summarizing scripts.
- CLAUDE.md can steer compaction: "When compacting, always preserve the full list of modified files and any test
  commands".
- State on disk: "Use structured formats for state data" (tasks.json), freeform notes (NIGHTn-LOG.md), "Use git for state
  tracking"; fresh starts sometimes beat compaction ("Review progress.txt, tests.json, and the git logs"); "It is
  unacceptable to remove or edit tests".
- Long-run failures Anthropic saw: "Prematurely declaring projects complete", "Leaving environments in buggy states".
  A separate evaluator is "far more tractable" than self-critique (summary); here that's the golden vectors, replays
  and Player smoke, not more review prompts.

## 2.6 New since mid-2026

- `/goal`: check-ins (v2.1.234+), idle check-ins capped at 3 (v2.1.246), resume on every route (v2.1.239), retry or pause
  after errors (v2.1.269). Subagent caps (v2.1.217+), depth default 3 (v2.1.219). Workflows and ultracode (v2.1.203+).
  Auto mode is the default starting mode (v2.1.283).
- Changelog (via summarizer): background Bash and PowerShell got a time limit in 2.1.285 (default 30 min, max 2 h),
  limited in 2.1.286 to unattended sessions (`-p`, SDK, CI).
- API: Opus 5.5 thinking always on; no forced tool choice; progress notes come back as thinking blocks
  (`display: "updates"`); compaction on demand in beta.
- Engineering index: harness design (Mar 24, 2026), C compiler (Feb 5, 2026), Managed Agents (Apr 8, 2026). Nothing later.

## 2.7 How to write the next night prompt and its /goal line

1. Launch with `--permission-mode auto --effort high` (or `medium`; compare across nights) and
   `--append-system-prompt-file NIGHT-RULES.txt`.
2. NIGHT-RULES.txt: the 5-8 standing rules nights have broken, Anthropic's four-early-stops paragraph verbatim, and the
   subagent line ("only for large, independent tasks; never to double-check").
3. Before a parallel night GG sets `worktree.baseRef: "head"`, or Core agents use manual `git worktree add` from
   `port/stageN`.
4. The first message is the `/goal` line naming NIGHTn.md, so the condition doubles as the kickoff.
5. NIGHTn.md under 350 lines, with acceptance commands whose output prints in the transcript.
6. Name the checks; no generic verify step. At most one end-of-night review subagent, told to "flag only gaps that affect
   correctness or the stated requirements".
7. Subagent briefs state objective, output format, tools and boundaries; log readers get `effort: low` and `maxTurns`.
8. Background commands get timeouts; the lead doesn't end a turn while waiting on output that matters.
9. No custom Stop hook alongside `/goal`; a later deterministic gate checks `stop_hook_active`.

Example (about 700 characters, under the 4,000 limit):

```text
/goal Run NIGHT14.md to its end state. Done means every task in its task list is either marked passing in tasks.json
with its acceptance command's output shown in this conversation, or marked short in HANDOFF.md with the named blocker
and the attempt made; and the last turn shows the totals line of `bash Tools/unity-loop.sh tests`, `git status --short`
and `date`. Also done: `date` shows 07:30 or later and HANDOFF.md section 0 lists what is left. Every turn ends with one
line "GOAL-STATUS: <passing>/<total> passing, <short> short, tests <n>/<n>, <HH:MM>". No commit on master and no push
at any point.
```

## Sources

Anthropic and Claude Code ([W] unless marked):
- https://platform.claude.com/docs/en/build-with-claude/prompt-engineering/ : prompting-claude-opus-5-5 ;
  prompting-claude-opus-5 ; claude-prompting-best-practices. Also .../build-with-claude/effort ;
  https://platform.claude.com/docs/en/models/opus-5-5/whats-new-opus-5-5
- https://code.claude.com/docs/en/ : goal ; hooks-guide ; hooks (truncated, not relied on) ; best-practices ; sub-agents ;
  worktrees ; workflows ; env-vars ; cli-reference ; model-config ; changelog (via summarizer)
- https://www.anthropic.com/engineering (index) and /engineering/ : effective-harnesses-for-long-running-agents ;
  effective-context-engineering-for-ai-agents ; multi-agent-research-system ; harness-design-long-running-apps ;
  building-c-compiler
- [S] https://github.com/openai/codex-plugin-cc/issues/548 ; [S] https://github.com/Yeachan-Heo/oh-my-claudecode/issues/3138

Steam and marketing ([W] unless marked):
- https://partner.steamgames.com/doc/ : features/playtest ; marketing/upcoming_events/nextfest ; .../nextfest/feb_2027 ;
  store/assets/standard ; store/assets/libraryassets ; gettingstarted/onboarding ; gettingstarted/contentsurvey ;
  store/eula (no usable content) ; store/application/branches ; store/application/demos ; store/tags ; store/trailer ;
  steamdeck/compat ; features/cloud ; features/achievements ; sdk/uploading ; marketing/event_tools
- https://howtomarketagame.com/ : blog/ ; 2026/07/14/games-that-used-momentum-for-steam-next-fest-success/ ;
  2026/06/30/nobody-plays-demos-and-that-is-ok/ ; 2026/07/13/did-ai-slop-ruin-steam-next-fest-june-2026/ ;
  2026/07/09/you-cannot-over-expose-your-game/ ; 2022/09/26/how-many-wishlists-should-i-have-when-i-launch-my-game/
- [S] https://presskit.gg/field-guides/next-fest-scheduling-registration ;
  [S] https://www.steampageanalyzer.com/blog/steam-next-fest-february-2027

Unity, crash reporting, CI, SDKs (all [W]):
- https://docs.unity.com/ugs/en-us/manual/cloud-diagnostics/manual/CrashandExceptionReporting/SettingupCrashandExceptionReporting ;
  https://docs.unity.com/en-us/cloud/developer-data/ : diagnostics ; get-started ; privacy-and-consent
- https://docs.unity3d.com/6000.3/Documentation/Manual/ : log-files.html ; WhatsNewUnity63.html ;
  https://docs.unity3d.com/Packages/com.unity.test-framework@1.4/manual/reference-command-line.html
- https://docs.unity3d.com/Packages/com.unity.platformtoolkit@1.0/manual/ : index.html ; install-platform-modules.html ;
  https://docs.unity3d.com/Packages/com.unity.platformtoolkit.steam@1.0/manual/index.html
- https://docs.sentry.io/platforms/unity/ : (root) ; native-support/ ; configuration/options/ ;
  data-management/sensitive-data/ ; https://sentry.io/pricing/
- https://docs.bugsplat.com/integrations/game-development/unity.md ;
  https://docs.saucelabs.com/error-reporting/platform-integrations/unity/setup/
- https://game.ci/docs/github/getting-started ; https://game.ci/docs/github/activation ;
  https://github.com/game-ci/steam-deploy ; https://docs.github.com/en/billing/concepts/product-billing/github-actions
- https://unity.com/products/unity-devops (no prices; unity.com/products/build-automation returned 404)
- https://steamworks.github.io/ ; https://github.com/rlabrecque/Steamworks.NET/releases ;
  https://github.com/Facepunch/Facepunch.Steamworks

Playtesting, privacy, planning (all [W]):
- https://www.nngroup.com/articles/why-you-only-need-to-test-with-5-users/ ;
  https://docs.gameanalytics.com/events-metrics-and-filtering/event-types/progression-events
- https://www.gdcvault.com/play/1025731/-Slay-the-Spire-Metrics ; https://www.gdcvault.com/play/1012356/One-Page
- https://makegames.tumblr.com/post/1136623767/finishing-a-game ;
  https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions
- https://commission.europa.eu/law/law-topic/data-protection/reform/what-personal-data_en

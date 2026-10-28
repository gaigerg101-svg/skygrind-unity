# FORGE71 research (three web-only agents, 2026-10-26 ~09:00-09:15 MST; no local files, no personal data sent)

Labels: [O] official Anthropic / Claude Code page, [P] preprint (not peer reviewed), [C] community (unverified), [D] designer or study
source. Each claim keeps the agent's URL. The agents' full reports are summarised here; where a quote came from a search excerpt the
agent said so, and it is marked (excerpt).

## R1. Long unattended Opus 5.5 runs: what changed, and how not to idle after the main plan fails

**The 2026-10-07 snapshot still holds** (https://platform.claude.com/docs/en/build-with-claude/prompt-engineering/prompting-claude-opus-5-5 [O]):
- Effort is the main control. At default medium, Opus 5.5 "matched or beat Claude Opus 5 at `high` effort". It thinks more per turn
  than Opus 5 at the same level. "Reserve `xhigh` and `max` for work where you've measured a quality gain."
- Delete "think step by step". A request to write reasoning out "may be declined".
- Name the four unwanted early stops, and name the stops you DO want.
- A time signal paces the run: an `elapsed / budget` line, with the budget set "somewhat above" the time wanted. Under time pressure
  the model "might search and verify a little less". A budget "mostly keeps more agents working in parallel".
- Mark pasted text as untrusted, and front-load one complete spec.

**Not in the snapshot:**
- Opus 5 "verifies its own work without being told to", and explicit verification lines "cause over-verification"; the sample prompt
  says "do not use subagents to verify or double-check your own work" and "keep spawn counts low"
  (https://platform.claude.com/docs/en/build-with-claude/prompt-engineering/prompting-claude-opus-5 [O]).
- In Claude Code, Opus 5.5 "starts at `medium`". A top-level `effortLevel` in settings "doesn't count for Opus 5.5": use `/effort`,
  `--effort` or `CLAUDE_CODE_EFFORT_LEVEL`. `max` "is prone to overthinking" (https://code.claude.com/docs/en/model-config [O]).
- The checklist must be a file. TaskCreate / TodoWrite are on by default only for older models
  (https://code.claude.com/docs/en/tools-reference [O]).
- "Task budgets are not supported on Claude Code" (https://platform.claude.com/docs/en/build-with-claude/task-budgets [O]). The time
  signal comes from the prompt or a hook: our n71-clock.cjs.

**Practices against early-plan-failure idle** (the agent's synthesis; the sources are mixed):
1. A ranked backlog, not one plan, deeper than the night. Anthropic's harness keeps a JSON feature list with `passes` flags and picks
   the top unfinished item (https://www.anthropic.com/engineering/effective-harnesses-for-long-running-agents [O]). One overnight
   runner found 83 % of runs had "no eligible issue" (https://www.matt-adams.co.uk/ai/security/2026/09/28/agent-github-backlog-overnight.html [C]).
2. A kill rule for each hypothesis. The workflow example stops after "two rounds in a row make no progress"
   (https://code.claude.com/docs/en/workflows [O]). "After two failed corrections, `/clear`" (https://code.claude.com/docs/en/best-practices [O]).
   "Three fix attempts, then BLOCKED" (https://github.com/igdutra/claude-overnight [C]).
3. "The main plan failed" is a named non-stop: log it, take the next backlog item. The only stops are those where "nothing can move
   without" the user [O, the Opus 5.5 page].
4. A failure log and decision notes, so pivots do not oscillate
   (https://www.anthropic.com/engineering/building-c-compiler [O]; https://developers.openai.com/blog/run-long-horizon-tasks-with-codex).
5. Gates grounded outside the transcript, and no auto-retry against a gate:
   - an agent claimed improvement in 54 of 54 cycles while 56 % moved nothing (https://arxiv.org/abs/2607.25152 [P]);
   - reward hacking grows with code size (https://arxiv.org/abs/2605.21384 [P]);
   - "the task verifier is nearly perfect, otherwise Claude will solve the wrong problem" (building-c-compiler [O]).
6. Budget the measuring. Anthropic's long-run app harness spent about 87 % building and 11 % on QA
   (https://www.anthropic.com/engineering/harness-design-long-running-apps [O]). Left alone, the model "will happily spend hours
   running tests instead of making progress": the fix was a `--fast` sample of the tests (building-c-compiler [O]).
7. Time the run from outside. Agents forecast their own time about 2.6x too high (https://arxiv.org/abs/2610.09944 [P], AgentTime; Opus
   5.5 was not tested).
8. Never wait in sleep loops: Monitor streams each line as an event; /goal skips its check while background work runs
   (https://code.claude.com/docs/en/goal [O]).
9. Delegate only independent tracks, isolated by file (the Opus 5 page [O]; https://code.claude.com/docs/en/agents [O]).
10. State on disk: each session starts from git log, the progress file and a basic test (effective-harnesses [O]).
11. One bounded final review that sees only the diff: "flag only gaps that affect correctness"; "Chasing every finding leads to
    over-engineering" (best-practices [O]).

**Claude Code facts for an overnight run** [O]:
- /goal (https://code.claude.com/docs/en/goal): the judge "doesn't run commands or read files"; the condition can be up to 4,000
  characters; a usage limit gives "Goal paused" and auto-continue resumes it.
- Stop hooks (https://code.claude.com/docs/en/hooks-guide): 8 blocks in a row with no tool call end the run
  (`CLAUDE_CODE_STOP_HOOK_BLOCK_CAP`).
- Auto-continue through a usage limit (https://code.claude.com/docs/en/interactive-mode#wait-for-a-usage-limit-to-reset): it re-arms
  at most twice in a row; after more than ~30 min of sleep it waits for Enter; a permission prompt can stall the continued task.
- Auto mode (https://code.claude.com/docs/en/permission-modes): three blocks in a row, or 20 in total, pause it and go back to prompting.
  An unattended session can then sit idle.

## R2. Removing dead code and stale flags safely (Unity 6 C#, a ~90 minute agent slice)

- **Unity reaches code no C# names:**
  - engine messages, Invoke / InvokeRepeating / StartCoroutine strings, attribute entry points, serialized fields: the
    Microsoft.Unity.Analyzers suppressors USP0003 / 0005 / 0006 / 0007 / 0008 / 0009 / 0012 / 0013 / 0020
    (https://github.com/microsoft/Microsoft.Unity.Analyzers/blob/main/doc/index.md);
  - SendMessage names (https://docs.unity3d.com/6000.3/Documentation/ScriptReference/GameObject.SendMessage.html);
  - UnityEvent persistent listeners store a method name;
  - scenes and prefabs store a script's `.meta` GUID, and deleting an attached script gives a load-time warning, not a compile error;
  - Resources.Load path strings;
  - code under undefined `#if` symbols is invisible to analyzers
    (https://docs.unity3d.com/6000.3/Documentation/Manual/platform-dependent-compilation.html).
- **The Roslyn rules IDE0051 / IDE0052 / CA1823 cover private members only.** `dotnet format style --verify-no-changes` detects and must
  never apply (https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-format). Our forge61 probe: IDE rules unreliable here
  without the Unity analyzers.
- **Flag cleanup practice:**
  - Uber Piranha: keep the decided branch, simplify, then a second pass for wrapper code; the flag's owner reviews the diff
    (https://www.uber.com/blog/piranha/).
  - LaunchDarkly's cleanup skill: "Never guess the forward value"; all OFF means keep the OFF behaviour; verify that no reference to the
    key remains (https://github.com/launchdarkly/ai-tooling/blob/main/skills/feature-flags/launchdarkly-flag-cleanup/SKILL.md).
  - FeatBit: "Do not delete just because the flag looks old"; for a flag never rolled out, "keep the fallback behavior"
    (https://www.featbit.co/feature-flag-lifecycle-management/clean-up-stale-feature-flags-with-coding-agents).
  - Toggles are "inventory which comes with a carrying cost" (https://martinfowler.com/articles/feature-toggles.html).
- **Process:**
  - small commits, each `git revert`-able (https://google.github.io/eng-practices/review/developer/small-cls.html);
  - Mikado: try the change, revert on red, do the prerequisite first (https://matthiasnoback.nl/2021/02/refactoring-the-mikado-method/);
  - golden / characterization tests read-only for the agent (https://developertoolkit.ai/en/workflows/test/characterization-tests/);
  - a grep that finds nothing does not prove code is dead, but a match is a reason to keep it
    (https://engineering.fb.com/2023/10/24/developer-tools/automating-dead-code-cleanup/).
- **Agent pitfalls:**
  - 13 of 176 ChatGPT and 9 of 137 Gemini refactorings were unsafe (https://arxiv.org/abs/2411.04444);
  - agents delete failing tests (https://arxiv.org/abs/2510.20270);
  - scope creep during cleanup (https://jonesrussell.github.io/blog/building-codebase-cleanup-skill-claude-code/).
- **The agent's procedure** (adopted, with our gates; see DECISIONS D-5):
  - 0-10 min: the baseline.
  - 10-30 min: an evidence packet per candidate.
  - 30-75 min: one flag per loop, ~12 min each: keep the OFF path, compile, grep again, run the targeted tests and the gates, commit
    the explicit paths. On red: `git checkout --` the paths, log "blocked: prerequisite", move on. Never fix forward; never edit a test
    to pass.
  - 75-90 min: no new candidate, the final gates, a fresh critic over the diff against the never-delete list.
  - The score: +3 old, +2 verdict failed, +2 <= 3 files in one assembly, +1 Game only, +1 tests pin OFF, +1 >= 50 lines, -3 a
    MonoBehaviour / serialized field, -2 simulate() / timer / RNG. Delete at >= 7, list 5-6 for GG, skip < 5.

## R3. When automated gates keep content from ever reaching a playtest

- **Bots guard, people judge.**
  - "current computational metrics should not be used in lieu of user studies" (https://ojs.aaai.org/index.php/AIIDE/article/view/12785 [D]).
  - King's bot playtester "enables designers to gauge the game experience", but "Ultimately, designers determine fun"
    (https://wnhub.io/news/Generative_AI/item-43934 [D]).
  - LLM agents track human difficulty (https://arxiv.org/abs/2410.02829 [P]) but are "jagged" on fun (https://arxiv.org/abs/2510.10930 [P]).
  - A bot built to win is not a bot that plays like a person (https://arxiv.org/abs/1903.10545 [D]).
- **Playtest cadence:**
  - Titanfall 2's action blocks: "Each week they would regroup to show and play what they made"; over 100 blocks, then the best kept
    (https://news.xbox.com/en-us/2016/09/29/building-single-player-titanfall-2/ [D]).
  - "You should playtest your level yourself every day"; Valve's Friday outside playtest
    (https://book.leveldesignbook.com/process/blockout/playtesting [D]).
  - DOOM Eternal's room size was tuned by play: "Too big, you actually feel slow. Too small, you're frustrated."
    (https://www.gamedeveloper.com/design/q-a-evolving-the-combat-design-of-id-software-s-i-doom-eternal-i- [D]).
- **Kill criteria need a state and a date** (Annie Duke, https://davidepstein.substack.com/p/annie-duke-quit). Release toggles "should
  generally not stick around much longer than a week or two" (Fowler).
- **Specification gaming:** tuning a room to beat a scripted pilot meets the letter of the rule
  (https://deepmind.google/discover/blog/specification-gaming-the-flip-side-of-ai-ingenuity/).
- **Room patterns that make a movement verb pay:**
  - A. Hazard on the approach lane: Hades' walled rooms give "extra Wall Slam damage"; make the natural fight line the throw line
    (https://kotaku.com/hades-level-design-is-less-random-than-it-seems-1845254545).
  - B. Deny the floor, open the wall or air route. Titanfall 2: walls "represent possibilities" (excerpt,
    https://www.gamesradar.com/titanfall-2-review).
  - C. A jungle gym linking skirmishes: DOOM Eternal's bars and pads join fights (https://blog.adamatomic.com/post/613311014289768448/design-of-doom-eternal, excerpt).
  - D. The resource where the verb lands: ULTRAKILL heals by close-range blood (https://en.wikipedia.org/wiki/Ultrakill).
  - E. A shortcut only the verb opens: Neon White
    (https://www.gameinformer.com/feature/2022/11/16/laying-the-smackdown).
  - One concept per room, taught, developed and twisted (https://www.gamedeveloper.com/design/the-secret-to-i-mario-i-level-design).
- **Caveats:** no ULTRAKILL level-design talk was found, and the GDC Vault was down.

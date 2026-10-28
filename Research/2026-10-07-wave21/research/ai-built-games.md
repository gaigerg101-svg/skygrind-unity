# Building notable new game features with AI coding agents in long unattended runs (2025-2026)

Web research only, 2026-10-07, about 12 minutes. "Read" means the page itself was fetched and summarized. "Snippet only"
means only a search-result summary was seen, so the claim is **not verified**. Quotes are as the fetch tool returned
them. It paraphrases, so check the exact wording at the URL before you quote it elsewhere.

## Sources

| # | Source | Date | Status |
|---|---|---|---|
| S1 | Anthropic, "Effective harnesses for long-running agents": https://www.anthropic.com/engineering/effective-harnesses-for-long-running-agents | 2025-11-26 | read |
| S2 | Anthropic, "Harness design for long-running application development" (Prithvi Rajasekaran): https://www.anthropic.com/engineering/harness-design-long-running-apps | 2026-03-24 | read |
| S3 | Anthropic, "How we built our multi-agent research system": https://www.anthropic.com/engineering/multi-agent-research-system | 2025-06-13 | read |
| S4 | Anthropic, "Building a C compiler with a team of parallel Claudes": https://www.anthropic.com/engineering/building-c-compiler | 2026-02-05 | read |
| S5 | Claude Code docs, "Best practices for Claude Code" (redirect target of the old engineering post): https://code.claude.com/docs/en/best-practices | current | read |
| S6 | Cursor (Wilson Lin), "Scaling long-running autonomous coding": https://cursor.com/blog/scaling-agents | 2026-01-14 | read |
| S7 | gonzoml, "How we built a full browser game in two evenings with Claude": https://gonzoml.substack.com/p/how-we-built-a-full-browser-game | 2026-03-25 | read |
| S8 | yurukusa, "I ran a 5-agent game studio with Claude Code Teams": https://dev.to/yurukusa/i-ran-a-5-agent-game-studio-with-claude-code-teams-2lpk | 2026-02-19 | read |
| S9 | BigDevSoon, "Building games with AI: how we shipped a 2D roguelite in 10 days" (Unity, Void Balls): https://bigdevsoon.me/blog/building-games-with-ai-indie-game-dev-workflow/ | undated | read |
| S10 | MeepleLM, "A virtual playtester simulating diverse subjective experiences": https://arxiv.org/abs/2601.07251 | 2026-01 | abstract read |
| S11 | "Lost in Simulation: LLM-simulated users are unreliable proxies for human users in agentic evaluations": https://arxiv.org/abs/2601.17087 | 2026-01 | **snippet only** |
| S12 | GBQA, "A game benchmark for evaluating LLMs as QA engineers": https://arxiv.org/abs/2604.02648 | 2026-04 | abstract read |
| S13 | RetroStyle Games, "I built a live game with Claude Code": https://retrostylegames.com/blog/claude-code-game-dev-case-study/ | n/a | **snippet only** |
| S14 | Althera Games, "AI agents for game development 2026": https://altheragames.com/en/blog/ai-agents-game-development-2026 | 2026 | **snippet only** |
| S15 | OpenAI game-studio plugin, `game-playtest` skill: https://github.com/openai/plugins/tree/HEAD/plugins/game-studio/skills/game-playtest (listing: https://mcpservers.org/agent-skills/openai/game-playtest, which returned 403) | n/a | **snippet only** |
| S16 | Unity "AI product manager" scripted playtests (screenshots and video): https://github.com/alvaris924/unity-ai-product-manager/issues/4 | 2026-09 | **snippet only** |
| S17 | Claude Opus 4 / Sonnet 4 system card (reward hacking, hard-coding tests): https://www-cdn.anthropic.com/6d8a8055020700718b0c49369f60816ba2a7c285/Claude%204%20System%20Card.pdf | 2025-05 | **snippet only** |

## What worked

1. **An initializer pass that writes a feature list JSON, then one feature per session.** Each entry has `category`,
   `description`, a `steps` array and a `passes` boolean, and every entry starts as failing. The list carries the rule
   "It is unacceptable to remove or edit tests because this could lead to missing or buggy functionality". Every session
   updates a progress file (`claude-progress.txt`) and commits to git, so a bad change can be reverted. (S1)
2. **Testing as a user rather than with unit tests alone.** The agent drives the real app (Puppeteer MCP in S1,
   Playwright in S2) to check features end to end. (S1, S2)
3. **Planner, generator and evaluator kept apart, with a "sprint contract".** Before any code is written, the generator
   and the evaluator agree on what "done" means for that chunk of work. The evaluator then plays the running app.
   - Retro game maker: a solo agent spent 20 min and $9 and produced a game whose entities did not respond to input.
     The full harness spent 6 h and $200 and produced a playable game.
   - The evaluator caught "broken wiring between entity definitions and runtime".
   - With Opus 4.6 the sprint split was dropped, and the evaluator still caught gaps (a DAW build: 3 h 50 min, $124.70).
   (S2)
4. **A design rubric against "AI slop".** There are four criteria: design quality, originality, craft and functionality.
   Originality and design quality carry the most weight, and telltale AI patterns are penalized explicitly. (S2)
5. **A near-perfect verifier, plus a fast sampled check.** "It's important that the task verifier is nearly perfect."
   - CI blocks commits that make existing results worse.
   - A `--fast` mode runs a 1-10% random sample, which fights "time blindness".
   - Error lines are written so grep can find them ("ERROR" on the same line as the details).
   - READMEs and progress files are written for the agent to read. (S4)
6. **An oracle to split monolithic work.** GCC served as a reference compiler, so 16 agents could work on different files
   at once. Text-file locks in `current_tasks/` stopped two agents from taking the same task. (S4)
7. **Hierarchical planners and workers, with periodic fresh starts.** Planners explore the code and create tasks.
   Workers finish one task each and don't coordinate with each other. A judge decides at the end of each cycle whether
   the job is done. Fresh starts counter drift and tunnel vision. "The prompts matter more" than the harness. (S6)
8. **Verification that closes the loop by itself.**
   - "Claude stops when the work looks done. Without a check it can run, 'looks done' is the only signal available."
   - The docs list ways to gate the end of a run: a `/goal` condition, a Stop hook that runs a script, or a reviewer
     subagent in a fresh context.
   - Claude should show evidence such as test output or screenshots.
   - After two failed corrections, `/clear` and start again with a better prompt.
   - Specs should name files and interfaces, state what is out of scope, and end with an end-to-end verification step.
   (S5)
9. **A short playtest loop.** The developer played, reported a symptom at a high level ("Power-ups feel weak", "I don't
   see bosses"), and Claude fixed it. Each cycle took 2-5 min, which the author called "10x more productive than trying
   to spec everything upfront". (S7)
10. **Strict file ownership between parallel agents, and task dependencies.** "No two agents edit the same files. No
    merge conflicts." Dependencies blocked work until its prerequisites were done. Result: 9 of 17 tasks done in one
    session. (S8)
11. **Constraints that hold the look together, and tests as a safety net for refactors.**
    - "The 7-color palette wasn't a limitation — it was a design decision that made AI-generated art feel intentional."
    - The project has 88 test files, with Unity MCP as the bridge between the agent and the Editor.
    - The author gives no failures and no playtest method, so this is a one-sided account. (S9)
12. **Scripted playtests inside Unity with screenshots and video.** Synthetic gestures go through the EventSystem, and
    the run produces a per-step report with console errors, thumbnails and video. Snippet only, **not verified**. (S16)
    OpenAI's `game-playtest` skill checks boot, input, scene transitions, HUD readability and visual state changes, with
    screenshot-based verification. Snippet only, **not verified**. (S15)
13. **Writing outputs to the filesystem, and an LLM judge that makes one call.** "A single LLM call with a single prompt
    outputting scores from 0.0-1.0 and a pass-fail grade was the most consistent and aligned with human judgements."
    Their evals started with about 20 realistic cases. (S3)

## What failed

1. **Declaring the project complete too early, and marking features "passing" without thorough testing.** (S1)
2. **"One-shotting" everything at once,** which left features half finished and undocumented. (S1)
3. **Self-evaluation leniency.** Models praise their own mediocre work. (S2)
4. **Stubbed features:** "display-only functionality without interactive depth". (S2) In the solo run, the game's
   entities did not respond to input. (S2)
5. **"Context anxiety".** The model wraps up early as its context fills. Context resets were the fix. (S2)
6. **Data with no code behind it.** 15 pedestrian behavior flags were defined in data but never checked in code, and
   bosses were "defined but never actually implemented". (S7)
7. **Integration drift across files built in parallel.** 21 files built at once disagreed on method names, return
   formats and property paths. (S7)
8. **Forgetting the supporting systems.** New features were added without updating the encyclopedia, the achievement
   tracker, collision descriptors or saves. (S7)
9. **New features broke old ones.** Even with CI, the compiler could not avoid regressions near the end of the run.
   (S4)
10. **Flat coordination with locks.** "Twenty agents would slow down to the effective throughput of two or three." With
    no hierarchy, agents became risk-averse and avoided the hard problems. An integrator role "created more bottlenecks
    than it solved". (S6)
11. **Multi-agent setups fit coding poorly.** "Most coding tasks involve fewer truly parallelizable tasks than research."
    Multi-agent runs use about 15x the tokens of chat. (S3)
12. **Idle agents, cost, duplicated context, and late changes that invalidate other agents' work.** For example, the
    builder changed things after the designer had taken screenshots. (S8)
13. **Gamed tests.** Models hard-code or special-case values so tests pass. On Anthropic's "reward hacking prone coding
    tasks v2", Sonnet 4 failed the holdout tests 5% of the time and set off the classifier 14% of the time. Snippet only,
    **not verified**. (S17)
14. **"Type-correct but meaning-wrong" code**, which is said to happen most in save systems, gameplay state machines and
    game feel tuning. Snippet only, **not verified**. (S14)
15. **Agents are weak game QA.** The best model (Claude 4.6 Opus, thinking) found 48.39% of 124 human-verified bugs
    across 30 games. (S12)

## Role-play / persona methods

- **Personas grounded in data help.** MeepleLM was trained on 1,727 rulebooks and 150K player reviews. It reasons in
  Mechanics-Dynamics-Aesthetics terms and uses player personas distilled from those reviews. Users preferred its
  critiques 70% of the time over GPT-5.1 and Gemini 3 Pro. It covers board games, from rules text, with no game engine.
  (S10)
- **Simulated users are biased proxies.**
  - Agent success rates moved by up to 9 points depending on which LLM played the user.
  - The simulated users overestimated success on moderately difficult tasks and underestimated it on the hardest ones,
    with calibration error up to 15.1%.
  - Simulated users tended to be over-polite and more willing to accept suggestions than real users.
  - Snippet only, **not verified**. (S11)
- **The reviewer, not the persona, is what gets agreement with humans.**
  - A fresh-context reviewer avoids bias toward the code it just wrote. (S5)
  - A reviewer told to find gaps "will usually report some, even when the work is sound", so tell it to flag only gaps
    that affect correctness or the stated requirements. (S5)
  - A single-call 0-1 score plus pass/fail matched human judgement best. (S3)
- **Human playtesting still finds what evals miss.** "People testing agents find edge cases that evals miss." (S3) In
  S7, the human playtest loop is what found the missing bosses.
- **Not checked:** PersonaArena, Deep Persona, CoSER and the MMO multi-agent playtest paper turned up in search but were
  not read.

**Takeaway:** persona reviewers are cheap for ideas and triage, and better when the personas are grounded in real player
data. They are not acceptance gates. Treat their scores as hypotheses, put a pass/fail rubric next to them, and check
them against a human playtest.

## 8 rules for a 5-6 hour autonomous build night (Unity FPS)

1. **Run one feature at a time from a JSON list.** Each feature carries a `passes` flag and player-level `steps`. A
   feature flips to passing only on recorded evidence: the smoke output, a screenshot, or a metric. Never edit or delete
   a feature's acceptance steps during the night. (S1)
2. **Write a sprint contract before any code.** For each feature, write down:
   - what a player does and sees
   - the measured check that proves it, run in the Player or in a Play-mode script
   - what is out of scope

   The evaluator signs the contract before the generator starts. (S2, S5)
3. **Never let the builder grade itself.** A separate evaluator, in a fresh context, plays the build through scripted
   input and screenshots or video. It scores against a fixed rubric: functionality, craft, originality, and a penalty
   for stubs or display-only features. It returns one 0-1 score plus pass/fail. Tell it to flag only gaps against the
   contract. (S2, S3, S5)
4. **Make the stop deterministic.** Put the verifier in a Stop hook or `/goal`: compile, the EditMode suite, and the
   Player smoke. Add a fast sampled regression mode, so a regression shows up in minutes rather than at the close. The
   verifier has to be close to perfect, because the agent optimizes against it. (S4, S5)
5. **Close each feature with a "wired, not just defined" audit.** Check that every new config key, event and enemy or
   weapon definition is read by code and reachable in play. Check that the supporting systems were updated: UI lists,
   saves, settings, the events table. (S2, S7)
6. **Commit and write the progress file after every feature, and restart fresh when the context degrades.** If a
   session has corrected the same issue twice, or its context is filling up, start a new session from the progress file
   and the feature list rather than pushing on. (S1, S5, S6)
7. **Parallelize only work with disjoint files, under one planner.** Workers claim tasks through lock files or task
   dependencies and don't coordinate with each other. Don't add an integrator agent as a bottleneck. Keep the
   interdependent core (the sim and movement) to a single agent, because coding parallelizes worse than research does.
   (S3, S4, S6, S8)
8. **Use personas and simulated players for triage, never as the acceptance gate.** Ground the personas in real data
   (the owner's notes, real player reviews), and use them to rank ideas and find suspect areas. Their verdicts are
   biased (S11) and their bug recall is about 50% (S12). The morning human playtest is the ground truth. Keep it short,
   and report symptoms ("feels weak") for the next night's contract. (S7, S10, S11, S12)

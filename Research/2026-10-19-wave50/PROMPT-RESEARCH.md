# Wave 50 research: how to prompt the night, and what the look rows need (2026-10-19, ~10 min, after NIGHT49's close)

Each line says Verified (read on the page) or Reported (a secondary source). Nothing here was measured in the game.

## A. Prompting a long unattended session
- Verified, Claude Code best practices (https://code.claude.com/docs/en/best-practices.md): "Claude stops when the work looks done. Without
  a check it can run, 'looks done' is the only signal available." Give a check that returns pass / fail; "have Claude show evidence
  rather than asserting success". A fresh-context reviewer subagent "sees only the diff and the criteria"; tell it to "flag only gaps that
  affect correctness or the stated requirements" (a reviewer told to find gaps always finds some).
- Verified, /goal (https://code.claude.com/docs/en/goal.md): the evaluator is a small fast model that "does not call tools, so it can only
  judge what Claude has already surfaced in the conversation". So the condition must name evidence the lead PRINTS (the close table, the
  `git tag -l` line, `date`). Up to 4,000 characters; a time clause bounds it; the evaluation is skipped while a subagent or background
  shell runs; a usage limit PAUSES the goal and it resumes when the limit resets (the NIGHT47 3 h stall).
- Verified, Anthropic "Effective harnesses for long-running agents"
  (https://www.anthropic.com/engineering/effective-harnesses-for-long-running-agents): two failure modes: doing too much at once, and a later
  session that "declares victory too early". The fix: a JSON feature list, every entry failing at first, with test steps and a `passes`
  flag the worker may only flip ("It is unacceptable to remove or edit tests"); JSON because the model is less likely to rewrite it than
  Markdown; a progress file + git history read at the start of each session; one feature at a time; end-to-end checks found bugs unit tests missed.
- Reported (https://academy.dair.ai/blog/autonomous-long-running-coding-agents): the goal is a contract (end state, evidence, constraints);
  a weak goal lets the model stop early or redefine success.

## B. Blind pairs with model judges
- Reported (https://arxiv.org/html/2602.02219v1, Zheng et al. 2023 cited there; https://www.alphaxiv.org/abs/2602.13110.md): position bias
  in pairwise judging is fixed by asking each judge in BOTH orders; a pick counts as a win only if it is the same both times, otherwise a TIE.
  Shuffle on every call; re-check the judge against known answers.
- Consequence for SKYGRIND: NIGHT49's judges saw each pair once. arc40's three 3-0 wins may be real, but the protocol could not tell.

## C. The look
- Reported (https://bugnet.io/blog/readable-game-art-why-clarity-beats-detail): "Most readability failures are value failures"; push the
  background back in value and saturation first; the player, threats and interactables sit in a different brightness band; "grayscale
  screenshots are the cheap audit"; shape reads through chaos, tint does not.
- Verified (https://cdn.fastly.steamstatic.com/apps/valve/2008/MIGS08_ConnectingVisualsToGameplay.pdf, Valve MIGS08, TF2): readability tied
  to gameplay; distinct silhouettes; the highest contrast where the eye goes.
- Reported (https://80.lv/articles/key-factors-in-environment-design-of-lawbreakers/): VFX, telegraphs, UI and post made enemies hard to
  parse; the team changed the environment itself; no detail without a purpose.
- Reported, ULTRAKILL wiki (https://ultrakill.fandom.com/wiki/Railcannon_(disambiguation)): the Railcannon shows charge ON THE GUN: four
  capacitors per side engage at quarter steps, a rear capacitor glows the variant colour at full; the HUD shows the recharge separately.
- Reported, Steam (https://steamcommunity.com/app/782330/discussions/0/4698886342118827973): DOOM Eternal players complained the dot
  crosshair does not show ballista charge or plasma heat, and the plasma rifle's heat glow is "hard to notice" mid-fight. So a glow on the
  gun alone is not enough: the HUD element under the reticle stays the primary read; the gun is the second.

## What this changes in NIGHT50.md
1. A JSON row list (`Tools/night/n50-rows.json`), every row `passes: false` at G0, with its check and evidence path; only the lead flips
   `passes`, and only with the evidence printed in the transcript.
2. Every pair asked in both orders per judge; consistent picks only; control pairs first.
3. V's meter adds the grayscale audit: enemies and usable surfaces in a different value band from the walls.
4. G2 (the gun) is stepped (quarters) like the Railcannon and is the SECOND read; G1 (under the reticle) stays the first.
5. The /goal names printed evidence; a fresh reviewer subagent checks the close table against the evidence (correctness gaps only).

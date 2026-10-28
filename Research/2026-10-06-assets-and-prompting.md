# Research 2026-10-06: assets we may pull in, and prompting Opus 5.5 for long runs

One read-only research agent, about 15 minutes, on 2026-10-06. It builds on `2026-10-03-polish.md`,
`2026-10-04-production-and-prompting.md` and `2026-10-06-feel-in-motion.md` and doesn't repeat them. Licences were read
from each repo's LICENSE file, source header or the GitHub API's `spdx_id` where marked **[lic]**. Everything else came
through the fetch tool's summarizer (close paraphrase) or a search snippet (**[S]**). Anything unconfirmed is marked
"(unverified)".

**What the repo has now (read this session):**
- `Packages/manifest.json` has no tween, UI-effect or VFX package.
- `Assets/ThirdParty/` doesn't exist, and there's no THIRD-PARTY / CREDITS file.
- `Assets/Fonts/` already holds six OFL families with an `OFL.txt` each: Archivo, Tourney, Big Shoulders Display, Saira,
  Chakra Petch and Barlow Semi Condensed. NIGHT17 cut them into static instances with fontTools (commit `fcdc730`).
- None of those `OFL.txt` files declares a Reserved Font Name, so cutting instances is fine.
- `git remote -v` is empty: the repo is local and private. That matters for Asset Store content (see below).
- The feel note's line "D-30 (no new packages) holds" conflicts with GG's new "don't be afraid to copy and pull
  assets". The lead should log a D-xx that supersedes D-30 before anything is imported.

## Sources

- Assets, code: github.com/mob-sakai/UIEffect, /ParticleEffectForUGUI, /SoftMaskForUGUI;
  github.com/annulusgames/LitMotion; github.com/KyryloKuzyk/PrimeTween (+ its LICENSE); dotween.demigiant.com/license.php;
  github.com/dentedpixel/LeanTween (source header); github.com/Unity-UI-Extensions/com.unity.uiextensions;
  github.com/kirevdokimov/Unity-UI-Rounded-Corners; github.com/keijiro/KinoGlitchURP, /KinoEightURP, /FlashGlitch,
  /Duotone, /URP-CameraEffectTemplate, /Kino; github.com/orangeduck/Spring-It-On; github.com/reunono/MMFeedbacksPublic
  (LICENSE.md); github.com/Unity-Technologies/VisualEffectGraph-Samples (LICENSE.md);
  unity.com/legal/licenses/unity-companion-license; unity.com/legal/as-terms
- Art: kenney.nl/assets/particle-pack, /input-prompts, /ui-pack-sci-fi; thoseawesomeguys.com/prompts/;
  game-icons.net/about.html; github.com/google/material-design-icons; api.github.com/repos/google/fonts/contents/ofl/*
- Attribution practice: wiki.creativecommons.org/wiki/Recommended_practices_for_attribution
- Prompting: platform.claude.com/docs/en/build-with-claude/prompt-engineering/claude-prompting-best-practices and
  .../prompting-claude-opus-5-5; code.claude.com/docs/en/best-practices, /scheduled-tasks; anthropic.com/engineering/
  effective-harnesses-for-long-running-agents, /building-effective-agents, /effective-context-engineering-for-ai-agents;
  ghuntley.com/ralph/; github.com/anthropics/claude-code/tree/main/plugins/ralph-wiggum; github.com/obra/superpowers

# Part 1: open-source assets and code we may legally pull in

## Candidates

| Candidate | Licence (exact) | Install route | Unity 6 / URP | Use for SKYGRIND |
|---|---|---|---|---|
| **UIEffect v5** (mob-sakai) 5.9.0 | MIT [lic] | `openupm add com.coffee.ui-effect`, or git `https://github.com/mob-sakai/UIEffect.git?path=Packages/src` | 2020.3+, URP and HDRP; TMP supported via a sample | Shiny sweeps, dissolve / burn / melt transitions, edge glow, gradients, blur, RGB shift on HUD plates, rank letters and menu focus. The single biggest "less cheap" lever for uGUI |
| **ParticleEffectForUGUI** 4.13.0 | MIT | `openupm add com.coffee.ui-particle`, or git `https://github.com/mob-sakai/ParticleEffectForUGUI.git` | 2018.3+, URP and HDRP, any canvas mode | Particle bursts inside the canvas (rank-up, points pop, kill ladder, menu select), maskable, with no extra camera or RenderTexture |
| **SoftMaskForUGUI** 3.2.0 | MIT | `openupm add com.coffee.softmask-for-ugui`, or git `...SoftMaskForUGUI.git?path=Packages/src` | 2019.4+, uGUI 2.0 (Unity 6), URP | Soft-edged panels and scroll fades, TMP-aware |
| **LitMotion** | MIT [lic] (pushed 2026-09-18) | git `https://github.com/annulusgames/LitMotion.git?path=src/LitMotion/Assets/LitMotion` | 2021.3+ | Zero-alloc tweens with TMP per-character bindings (`BindToTMPCharColor`, `BindToTMPCharPosition`): text reveals, punches, shakes |
| **PrimeTween** | Custom (`spdx_id` NOASSERTION) [lic]. You may "use PrimeTween and modify its source code in free and commercial products distributed in binary format" and redistribute it "internally within a team"; you may not "distribute PrimeTween's source code ... inside derivative products" | npm `com.kyrylokuzyk.primetween`, or the Asset Store | not stated (2020.1+ mentioned) | A fine tween library for a shipped binary, but not OSI. LitMotion is the cleaner pick |
| **DOTween** (free) | Custom: "freely use DOTween in both commercial and non-commercial projects"; modified versions may not be distributed. Pro: every person on the project needs a licence | Asset Store / zip | not checked | Usable; no reason to choose it over LitMotion |
| **LeanTween** | MIT (in `LeanTween.cs`'s header; Penner easings under BSD) [lic] | zip into `Assets/ThirdParty/LeanTween/` | not stated; last push 2021-12 | Stale. Skip |
| **Unity UI Extensions** 3.0 | BSD-3-Clause | `openupm add com.unity.uiextensions` | "Unity 6 ready" | `UICornerCut` (cut-corner sci-fi plates), `UILineRenderer`, `UICircle` (radial meters), Gradient, Curved Text, Shine |
| **Unity-UI-Rounded-Corners** | MIT | git `https://github.com/kirevdokimov/Unity-UI-Rounded-Corners.git` | 2019.3+; URP not stated | A per-corner radius `Image`. Low value: rounded corners don't fit the HUD's skewed language |
| **KinoGlitchURP** (Keijiro) | Unlicense [lic] | scoped registry "Keijiro", `jp.keijiro.kino-glitch.universal` | "Unity 6000.0 or later", URP | Analog / digital glitch passes for the hurt, down and RIVAL moments. Use sparingly: glitch can itself read as cheesy |
| **FlashGlitch** (Keijiro) | Unlicense | copy `Packages/jp.keijiro.flashglitch` into `Packages/` | 6000.0+, URP | Triggerable coloured flashes that fade, built for rhythm sync: kill and rank-up screen flashes driven by sim events |
| **KinoEightURP** / **Duotone** | Unlicense [lic for Duotone] | Keijiro registry, `jp.keijiro.kino.post-processing.eight.universal` | 6.0+, URP | Retro palette / dither: polish E9 (an optional retro toggle) only |
| **URP-CameraEffectTemplate** (Keijiro) | **No LICENSE file** (`spdx_id` absent) [lic] | none | Render Graph pass plus a camera controller | Study only. With no licence, all rights are reserved by default |
| **Kino** (HDRP, Keijiro) | not checked; last push 2020-11 | none | HDRP | Not for URP |
| **Kenney Particle Pack** | CC0 ("no attribution required") | zip into `Assets/ThirdParty/Kenney-Particles/` | n/a (PNGs) | 80 PNGs at 512 x 512: sparks, flares, smoke, muzzle and trace sprites for HitSpark, muzzle flash and kill bursts. Whether they are white and tintable to the palette: (unverified) |
| **Kenney UI Pack Sci-Fi** v2.0 | CC0 | zip | n/a | 130 files (buttons, panels, sliders). Reference or base for 9-slice plates; re-tint to the palette |
| **Kenney Input Prompts** v1.4 | CC0 | zip | n/a | 1,500+ files including Steam Deck, Xbox, PS1-5, Switch 1/2, keyboard and mouse; spritesheets and fonts. Glyphs for controller support and Deck Verified |
| **Xelu's Controller Prompts** | CC0 ("completely free to use in any personal or commercial project") | zip | n/a | An alternative glyph set. Platforms not read (in its Readme) |
| **game-icons.net** | CC BY 3.0. Attribution per icon author: "Icons made by {author}. Available on https://game-icons.net" | SVG / PNG export | n/a | 4,000+ icons for perks, buys and gadgets. Needs a credits line per author (Lorc, Delapouite, ...) |
| **Material Symbols** | Apache 2.0 ("attribution ... appreciated but not mandatory") | SVG / variable font from `google/material-design-icons` (`symbols/`) | n/a | Settings and system icons (gear, back, controller) |
| **Spring-It-On** (Daniel Holden) | MIT [lic] | port the C into a Core / Game helper; keep the MIT notice | n/a (C, raylib) | Critically damped springs with a half-life parameter, double and timed springs, inertialization: viewmodel sway / recoil and UI springs that settle without wobble |
| **Showerss/unity-first-person-controller** | MIT [lic] (pushed 2026-09-21) | read only | "Unity 6", CharacterController | Head bob, strafe tilt, sprint FOV. Copy camera-feel code only: its mover is CharacterController, which rule 4 forbids for gameplay |
| **jamesarndt82/first-person-controller** | MIT [lic] | read only | URP, new Input System | Head bob with stance reduction. Same caveat |
| **Linko-3D/First-Person-Controller-FPS** | MIT (code); models and sounds need attribution | read only | Godot / GDScript | Recoil, sway, bob and shake logic to port as ideas |
| **IronWarrior/UnityCameraShake** | Unlicense [lic] | read only | old (2019) | Trauma-shake reference (SKYGRIND already has one) |
| **MMFeedbacksPublic** | **CC BY-NC 4.0**, plus "You cannot sell MMFeedbacks or its parts, redistribute this code" [lic] | none | old | **Don't copy.** Non-commercial |
| **VFX Graph samples** (Unity) | Unity Companion License [lic]: use only "in connection with ... a valid Unity ... engine software license"; not "to develop a competing product" | git clone; needs `com.unity.visualeffectgraph` (not installed) | 2024-11 | Legal to copy into a Unity game. Needs the VFX Graph package; prefer the Particle System |
| **UniversalRenderingExamples** (Unity) | no LICENSE.md at root (404) | none | | Study only (unverified licence) |
| **Unity Particle Pack** 3.1 (Unity Technologies, Asset Store) | Standard Asset Store EULA | Asset Store | URP on 6000.3 per its listing [S] (unverified) | Reference muzzle flashes and explosions; re-tint to the palette |
| **Cartoon FX Remaster Free** (Jean Moreno) | Standard Asset Store EULA [S] | Asset Store | URP listed [S] | A punchy cartoon style; probably off-tone for the dusk look (opinion) |

**What the Asset Store EULA means here:**
- "Incorporate the Asset, together with substantial, original content ..., into an electronic application" is allowed.
- Editor Extensions are "single seat ... on a maximum of 2 computers".
- A public repo isn't addressed. The repo has no remote today, so vendoring is fine until it goes public. A public repo
  would need these assets git-ignored (inference).
- Source: https://unity.com/legal/as-terms

**Fonts:**
- Already vendored: the six families above.
- On Google Fonts' GitHub (`ofl/`, each with `OFL.txt`):
  - static TTFs: Rajdhani (5 weights), Chakra Petch, Barlow Condensed, Share Tech Mono (Regular only)
  - variable only: Oxanium, Teko, Orbitron, Exo 2, Saira, Space Grotesk, JetBrains Mono
  - source: https://api.github.com/repos/google/fonts/contents/ofl/rajdhani (and the same path per family)
- TMP has no documented support for variable-font axes (unverified; the Font Asset Creator docs list SDF and bitmap
  modes only: https://docs.unity3d.com/Packages/com.unity.textmeshpro@4.0/manual/FontAssetsCreator.html). Keep NIGHT17's
  route: cut static instances with fontTools (fcdc730).
- **The one gap:** a monospaced / tabular face for HUD numerals that mustn't jitter as they count. Share Tech Mono (static)
  or a JetBrains Mono instance.
- Every candidate is a clean geometric face that bakes well to SDF. Very thin weights (ExtraLight) need a larger atlas
  point size (common TMP practice, unverified).

## Vendoring: how to do it

1. **Check the licence before import.** Read the LICENSE file or the source header, not the README badge. GitHub's
   `spdx_id` is a quick check (`curl https://api.github.com/repos/<o>/<r>`). `NOASSERTION` or no licence means read the
   text (PrimeTween) or don't copy (URP-CameraEffectTemplate, UniversalRenderingExamples).
2. **Packages:**
   - Pin UPM git URLs to a tag (`...git?path=Packages/src#5.9.0`), or add the OpenUPM scoped registry with an exact
     version.
   - The `manifest.json` / `packages-lock.json` change is the commit (rule 1: explicit paths).
   - Raw art goes in `Assets/ThirdParty/<Name>/` with its `LICENSE` / `OFL.txt` / `License.txt` beside it, as the font
     folders already do.
3. **A root `THIRD-PARTY.md`**, one row per item:
   - name, version or commit, URL, the SPDX id (MIT, BSD-3-Clause, Unlicense, CC0-1.0, CC-BY-3.0, OFL-1.1, Apache-2.0,
     LicenseRef-Unity-Companion, LicenseRef-Asset-Store-EULA), path in the repo, used for, and the date checked
   - for CC BY, the TASL fields (Title, Author, Source, License with a link):
     https://wiki.creativecommons.org/wiki/Recommended_practices_for_attribution
   - example: `"Crossed swords" by Lorc, https://game-icons.net, CC BY 3.0 (https://creativecommons.org/licenses/by/3.0/)`
4. **Ship the notices.** MIT, BSD and OFL require the licence text to travel with copies. The Player build needs a
   CREDITS screen or a `licenses.txt` beside the exe (the MIT / BSD notice clauses; OFL 1.1). The REUSE convention
   (`LICENSES/` folder, SPDX headers) is an option (unverified: https://reuse.software/spec/ didn't load).
5. **Rules that still apply:**
   - No audio files from any pack (rule 7).
   - Re-tint every texture to the palette roles, kept as tokens in `UnityTuning`.
   - Enemy models stay the game's own.
   - VFX randomness comes from a render-side stream, never the run's `Mulberry32` (polish note, pitfalls).

## Ranked top 8 pulls

1. **UIEffect (MIT):** shiny sweeps, dissolve transitions and edge glow on HUD plates, rank letters and menu focus. It
   fixes "flat TMP text on a flat panel", the core of "cheap".
2. **LitMotion (MIT):** zero-alloc tweens with TMP per-character animation. It replaces hand-rolled lerps with staggered
   text reveals and punches (RESULTS tally, round burn-in).
3. **ParticleEffectForUGUI (MIT):** real particle bursts in the canvas for rank-up, points and kill-ladder moments,
   instead of scaled sprites.
4. **Kenney Particle Pack (CC0):** textured sparks, flares, smoke and traces for hit sparks, muzzle flashes and kill
   bursts, instead of untextured quads (what HitSpark draws now: not checked).
5. **Spring-It-On's damped springs (MIT, port the math):** half-life springs for viewmodel sway / recoil and UI motion.
   They settle without the rubbery overshoot that reads as cheesy.
6. **Unity UI Extensions (BSD-3):** `UICornerCut`, `UILineRenderer` and `UICircle` for cut-corner, skewed sci-fi plates
   and radial meters, instead of default sprites.
7. **FlashGlitch / KinoGlitchURP (Unlicense, Unity 6 URP):** short, event-driven screen flashes for hurt, down and
   rank-up, built on Render Graph. Small doses only.
8. **Kenney Input Prompts (CC0):** device-matched glyphs (Steam Deck included) for prompts and settings, ahead of
   controller support and Deck Verified.

Runners-up:
- SoftMaskForUGUI (soft panel edges)
- game-icons.net (perk and buy icons; per-author credits)
- Material Symbols (settings icons)
- Share Tech Mono (tabular HUD numerals)

# Part 2: prompting Opus 5.5 for long autonomous runs

The playbook and the earlier notes cover a lot already:
- effort (`medium` default)
- the four-early-stops paragraph and `--append-system-prompt-file`
- over-verification and subagent damping
- `/goal`'s transcript-only evaluator and the Stop-hook cap
- `worktree.baseRef`
- "every status claim cites a tool result"
- separate calibrated judges

The checklist below adds what's new or still missing. "In the prompts?" was judged by reading NIGHT19.md (and
grepping NIGHT18.md) this session:
- **Yes** = already there.
- **Partly** = something close is there.
- **No** = not found.

| # | Technique | Source | In the prompts? |
|---|---|---|---|
| 1 | **Tell it compaction is coming.** "Your context window will be automatically compacted ... do not stop tasks early due to token budget concerns. As you approach your token budget limit, save your current progress and state to memory before the context window refreshes." | https://platform.claude.com/docs/en/build-with-claude/prompt-engineering/claude-prompting-best-practices (Context awareness) | **No** ("compact" isn't in NIGHT18 / NIGHT19) |
| 2 | **A re-entry routine after compaction or a fresh window:** "Call pwd", "Review progress.txt, tests.json, and the git logs", "Manually run through a fundamental integration test before moving on to implementing new features." For SKYGRIND: re-read tasks.json, the log tail, `git log -5`, then `unity-loop.sh tests` before new work. Also add CLAUDE.md's "When compacting, always preserve the full list of modified files and any test commands". | same page (Workflows across multiple context windows); https://code.claude.com/docs/en/best-practices (Manage context) | **Partly** (U3-32.1 reads HANDOFF; there's no post-compaction re-entry step and no smoke before new work) |
| 3 | **Name the "cheap" patterns to avoid, then iterate.** "A general instruction such as 'avoid a generic AI look' mostly swaps one default for another"; Opus 5.5 "responds well to instructions that name specific patterns to avoid ... check which styles the first result used instead, and extend the list". For the UI: list the exact tells GG and the reviewers named. | https://platform.claude.com/docs/en/build-with-claude/prompt-engineering/prompting-claude-opus-5-5 (Frontend design defaults) | **No** (the prompts give targets and metrics, not a banned-pattern list) |
| 4 | **Don't game the check.** "Implement a solution that works correctly for all valid inputs, not just the test cases ... Tests are there to verify correctness, not to define the solution ... if any of the tests are incorrect, please inform me rather than working around them." This matters now that nights are judged on feel metrics (pixel diffs, sizes). | best-practices page (Avoid focusing on passing tests and hardcoding) | **No** |
| 5 | **Deterministic gates as hooks, not prose.** "Unlike CLAUDE.md instructions which are advisory, hooks are deterministic." Candidates: a PreToolUse hook that blocks `git add -A` / `.`, staging `UnityConnectSettings.asset`, commits on `master`, and `unity test`. | https://code.claude.com/docs/en/best-practices (Set up hooks); poka-yoke: https://www.anthropic.com/engineering/building-effective-agents | **No** (no `.claude/settings.json` in the project; user-level hooks not checked) |
| 6 | **Search before assuming it isn't built.** "Before making changes search codebase (don't assume not implemented)". Parallel builders duplicate helpers otherwise. | https://ghuntley.com/ralph/ | **No** |
| 7 | **No placeholders.** "DO NOT IMPLEMENT PLACEHOLDER OR SIMPLE IMPLEMENTATIONS. WE WANT FULL IMPLEMENTATIONS." In SKYGRIND terms: a stand-in is SHORT, never DONE. | https://ghuntley.com/ralph/ | **Partly** (done-when contracts; no explicit rule) |
| 8 | **Two-stage review per builder task:** spec compliance first, then code quality, in fresh subagents. Plans give "exact file paths, complete code, verification steps", with tasks of 2-5 minutes. | https://github.com/obra/superpowers | **Partly** (blind A/B reviewers judge feel; a per-merge spec check isn't named) |
| 9 | **A diff reviewer against the plan:** "review the ... diff against PLAN.md. Check that every requirement is implemented ... and nothing outside the task's scope changed. Report gaps, not style preferences." | https://code.claude.com/docs/en/best-practices (Add an adversarial review step) | **Partly** (reviewers 1) |
| 10 | **Hand-back size cap:** subagents return "a condensed, distilled summary ... (often 1,000-2,000 tokens)"; results go to files or commits. | https://www.anthropic.com/engineering/effective-context-engineering-for-ai-agents | **Partly** (not checked in the builder briefs) |
| 11 | **Just-in-time context:** keep paths and queries, load on demand; clear old raw tool output (route Unity logs through summarizing scripts). | same | **Partly** (unity-loop.sh summarises; not stated as a rule) |
| 12 | **Spend the whole budget:** "it's encouraged to spend your entire output context working on the task - just make sure you don't run out of context with significant uncommitted work." Commit per feature before long steps. | best-practices page (Encourage complete usage of context) | **Partly** (one commit per feature is a rule; the "uncommitted work" link isn't) |
| 13 | **Give the reason with each rule.** "Claude is smart enough to generalize from the explanation"; plain "Use X when ..." over "CRITICAL: You MUST". Use emphasis on one line only: "If you emphasize many lines, none of them stands out." | best-practices page (Add context; Tool usage); https://code.claude.com/docs/en/best-practices | **Partly** (NIGHT19 bolds most rule heads) |
| 14 | **Act, don't suggest:** the `<default_to_action>` block ("implement changes rather than only suggesting them ... using tools to discover any missing details instead of guessing"). | best-practices page (Tool usage) | **Yes** in effect (directive items, "no questions, no waiting") |
| 15 | **Parallel tool calls:** the `<use_parallel_tool_calls>` block. Mind "Run bash commands in parallel (which can even bottleneck system performance)", which matters on the bench laptop. | best-practices page (Optimize parallel tool calling) | **No** (low value; add the bench caveat if used) |
| 16 | **Look around before changing things:** "Before taking any action, explore broadly with tool calls ... including ones the task does not explicitly mention". Useful in builder briefs that touch several systems. | Opus 5.5 page (Explore context) | **Partly** ("Read these files" lists them) |
| 17 | **Wait on long jobs with an event stream, not polling:** the Monitor tool "streams each output line back, which avoids polling altogether". For `build_status`, Player smoke and long suites. | https://code.claude.com/docs/en/scheduled-tasks | **No** (the prompts poll) |
| 18 | **A one-item-per-iteration loop as a fallback harness:** `/ralph-loop "<prompt>" --max-iterations <n> --completion-promise "<text>"`, plus "After 15 iterations, if not complete: document what's blocking, list attempted solutions". Same idea as `/goal` with SHORT. Don't stack it on `/goal`: both are Stop hooks. | https://github.com/anthropics/claude-code/tree/main/plugins/ralph-wiggum | **Yes** in effect (`/goal` with DONE / SHORT) |
| 19 | **An evaluator-optimizer loop needs clear criteria and a stopping condition** ("such as a maximum number of iterations"). | https://www.anthropic.com/engineering/building-effective-agents | **Yes** (done-when plus caps) |
| 20 | **A time signal against a budget** (`elapsed 340s / 1200s`). It speeds up teams; it's "advisory", and under pressure the model may "verify a little less", so keep a hard stop. | Opus 5.5 page (Time signals) | **Yes** (NIGHT18: "3:10 / 6:00" in every status line; NIGHT19: time from `date`) |

**Not found:** an Opus 5.5-specific long-run guide beyond the prompting page, and Opus 5.5 behaviour changes beyond
effort, refusals, progress updates, visual inputs and frontend defaults. The page says Opus 5.5 "sustains long-running
autonomous work better than Claude Opus 5" and that "existing Claude Opus 5 prompts should perform well without
changes". awesome-claude-code's README didn't load (no entries read).

**Most likely missing, in order:**
- **#1 + #2:** the compaction paragraph plus a re-entry routine with a smoke before new work. Whether a 5-5.5 h run
  reaches the about-967K compaction point: not checked. Nothing in NIGHT19 says what to re-read if it does.
- **#3:** a named list of banned "cheap" UI patterns, iterated. It's the documented lever for generic-looking output.
- **#4 + #5:** an anti-gaming line for metric-judged feel items, and hooks for the git rules that CLAUDE.md now holds
  only in prose.

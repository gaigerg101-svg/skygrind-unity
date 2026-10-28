# NIGHT52 research (the forge, 2026-10-19 22:03-22:35 MST; three web-only agents + our own data)

Every outside claim carries its URL; UNVERIFIED marks what an agent could not open. Our own data comes first.

## 0. Our data (measured 2026-10-19 on port/stage3 d9484485)
- **Room density** (`python -I Research/2026-10-19-wave52/density-baseline.py`, output in density-baseline.txt): pieces (boxes, ramps and pads
  counted by `zone`, plus the room rows' `grappleNodes` and `pads`) per 100 m2 of each combat room. Kiln kb2 36 x 44 m 0.44, kb2c 28 x 30 m
  0.60, kb3 40 x 46 m 0.33, boss k5 64 x 64 m 0.00; iceline g1-g8 0.00-1.10; spire v2 1.67, v5 0.95, v6 1.02; span p1 1.39, p3 1.56, p5 1.70.
  **No jump pad in any n47 plan** (top level or room rows); grapple anchors live in room rows (span 22, spire 5). Props (kiln props45 / props46)
  are not counted: P0 decides whether a prop with a collider counts. **Foundry's geometry is `Design/levels/n46/foundry.plan.json`**
  (levelgen -> n46-foundry; gen-n47-foundry.cjs and gen-n48-foundry.cjs only add data to room a9): a2 14 x 20 m 0.71, a3 16 x 26 m 0.48,
  a4 8 x 19 m 1.97, a5 20 x 24 m 1.04, b1 36 x 40 m 0.42, a8 14 x 22 m 1.95, boss a9 28 x 28 m 0.00. Kiln's chain is n46 plan -> n47 plan ->
  levelgen -> `node Tools/night/gen-n47-kiln.cjs --finish` (the worm arena). Found by the forge's red-team reviewer.
- Layer 1 (NIGHT24) rooms were 52 x 48 to 56 x 56 m (docs/LEVEL-DESIGN-NOTES.md).
- Movement (PortExport/config.json via docs/ROOMS.md M-5): walk 8 m/s; verbs37 foundry: a wall ride 1.48 s on a 20 m wall at ~11.7 m/s; the
  boost lands 4 m up (7.3 m of a person's 8.1); the double jump 3.17 m; the corner wall jump 3.5 m (kiln.plan.json gate S1).
- The pipeline: one plan file per level (`Design/levels/n47/<level>.plan.json`) -> `dotnet run --project Tools/SimHarness -c Release --
  levelgen <plan> <map> [--walk]` (the report checks spots, routes, every tier reachable by walkers, pits) -> `campaign <plan> --seeds 10
  --rooms k` (clear rate and medians per room). A rebuilt level ships as a new id behind a level row (NIGHT48: gen-n48-foundry.cjs, n48-shared.cjs).
- NIGHT51 made the surfaces read (stone51 etc.); the rooms' layout is unchanged since NIGHT47 / 48.

## A. Arena design rules with numbers (agent 1)
1. Room scale in seconds of travel: Quake small / medium / large 256 / 512 / 1024 u ~ 6.5 / 13 / 26 m, 0.8 / 1.6 / 3.2 s at run speed (unit
   conversion the agent's). https://book.leveldesignbook.com/process/blockout/metrics/quake
2. Range bands (TF2): close <= ~4.9 m, medium <= ~19.5 m, long ~39 m (conv.). A small room's longest sightline under the mid band weakens
   hitscan camping. https://book.leveldesignbook.com/process/blockout/metrics
3. Drops mostly 256 u (~5 m), few 320 u, almost none 512 u. Same page.
4. **At most 3 floor planes per area**; a 4th adds nothing. https://book.leveldesignbook.com/process/layout/flow/verticality
5. Upward flow must be built (ramps, pads, climb walls): "downward flow is heavier than upward flow"; Q3DM17 has about a dozen pads. Same page.
6. **Every arena needs a circuit** ("combat bowl": a pillar in the middle to circle; a one-way drop forces a direction).
   https://book.leveldesignbook.com/process/layout/typology
7. **1-3 lanes per space**; more is a maze. https://book.leveldesignbook.com/process/layout/flow/circulation
8. Two arena shapes to mix: jungle gyms (distinct floors, hop between skirmishes) and canyons (open, continuous). The source page returned 403
   (snippets only). https://blog.adamatomic.com/post/613311014289768448/design-of-doom-eternal
9. DOOM Eternal arenas are mostly asymmetric, with vantage points for player and enemies. https://critpoints.net/2020/06/07/doom-eternal-reviews-ft-s-g-s
10. Cover: full >= 1.75 m, low 1.0-1.25 m, <= 0.5 m is not cover. https://book.leveldesignbook.com/process/combat/cover
11. **A movement shooter denies static cover**: DOOM's AI seeks open spots with a view of the player; pillars break line of sight, not camps.
    https://80.lv/articles/cyber-demons-the-ai-of-doom
12. **No safe ledges** (Hugo Martin: "get them off those ledges and don't let them kite enemies"): every perch reachable by at least one enemy
    type. https://mp1st.com/news/id-software-people-will-be-shocked-at-how-deep-doom-eternal-multiplayer-is
13. Vary sightlines: many deep ones overwhelm, a few shallow ones leave nothing to plan. https://book.leveldesignbook.com/process/combat/balance
14. Gate the entrance (one-way drop or lock) so the fight can't be pulled into a chokepoint. https://book.leveldesignbook.com/process/combat/encounter
15. Enemy types per fight: 1 tutorial, 2 regular, 3 complex, 4 brawl, 5+ slaughter. Same page.
16. Hakita (ULTRAKILL): difficulty is "the amount of enemies and their composition"; one tricky enemy in the right spot; low-count breathers.
    https://intothebluesky.com/2021/10/05/follow-up-interview-with-hakita/
17. Resources from enemies, pickups as breadcrumbs through the arena ("part of natural movement and not a detour").
    https://gdcvault.com/play/1024940/Embracing-Push-Forward-Combat-in ; https://www.gamedeveloper.com/design/design-notes-doom-2016-
18. Don't spawn behind a gamepad player (a 180 on a stick is slow). https://book.leveldesignbook.com/process/layout/flow/verticality
19. The 30-second encounter loop (enter, plan, execute). https://www.engadget.com/2011-07-14-half-minute-halo-an-interview-with-jaime-griesemer.html
20. Best pickup at the riskiest spot (Quake timers UNVERIFIED). https://icculus.org/pipermail/gtkradiant/2002-March/002265.html
21. Wall-run surfaces: size to run speed x run time (no official Titanfall 2 sizes found). https://titanfall.fandom.com/wiki/Enhanced_Parkour_Kit
**Common mistakes:** safe spots; waist-high crates that reward standing; one flat plane or more than 3; resources off the fight; one-note
enemy mixes.

## B. Open-source to copy (agent 2; licences checked on the GitHub API 2026-10-19 unless marked)
| use tonight for | source | licence | what to take |
|---|---|---|---|
| U hud52 animation | https://github.com/Luca3317/TMPEffects | MIT | tag-driven TMP shake / wave / pop for the rank and kill text |
| U hud52 tweens | https://github.com/annulusgames/LitMotion | MIT (stars per an aggregator) | allocation-free RectTransform / colour tweens for meters and punches |
| U hud52 effects | https://github.com/mob-sakai/UIEffect | MIT | shiny / dissolve / gradient on uGUI (the project already carries its settings asset) |
| U skin52 / ink | https://github.com/chrisnolet/QuickOutline | MIT | smoothed normals baked into a UV channel (fixes ink51's gaps on flat-shaded corners) |
| U skin52 shader base | https://github.com/ColinLeung-NiloCat/UnityURPToonLitShaderExample | MIT | the URP toon lit skeleton: ramp lighting for the enemy body |
| shader templates | https://github.com/Cyanilux/URP_ShaderCodeTemplates | CC0 | URP HLSL templates (no attribution needed) |
| a world ink pass (leftover) | https://github.com/CristianQiu/Unity-URP-Outline | MIT | render-graph outline feature (6000.3+) |
| gibs (leftover) | https://github.com/DavidArayan/ezy-slice | MIT | plane slicing that keeps UVs: real cuts of the pill models at a kill |
| camera kicks (reference) | https://github.com/gasgiant/Camera-Shake | MIT | additive Perlin / kick shakes |
| level blockout (NOT tonight: a pipeline change) | https://github.com/radiatoryang/scopa , https://github.com/RadicalCSG/com.chisel | MIT | TrenchBroom .map import / in-Editor CSG; park in DECIDE52 as a later spike |
| enemy climb links (reference only) | https://github.com/ikpil/DotRecast | zlib | pure C# Recast off-mesh links; the game keeps its own nav grid |
**Avoid:** Xonotic / DarkPlaces (GPL), zzamjak ToonShading (GPL-3.0), unlicensed repos (WiggleWizard q3 movement, PhilS94), NavMeshAgent-based
AI (the no-NavMesh rule), Feel / Nova (paid). Log every port in docs/THIRD-PARTY.md with its licence.

## C. Prompting a 7-8 h run (agent 3)
1. Feature list as JSON, every item failing; only `passes` may change ("It is unacceptable to remove or edit tests").
   https://www.anthropic.com/engineering/effective-harnesses-for-long-running-agents
2. The complete spec up front, then let it run. https://platform.claude.com/docs/en/build-with-claude/prompt-engineering/prompting-claude-opus-5
3. Name the four unwanted early stops (a summary announcing the next step, an offer to continue, decisions that block nothing, stopping at a
   milestone). https://platform.claude.com/docs/en/build-with-claude/prompt-engineering/prompting-claude-opus-5-5
4. `/goal` reads only the transcript: its condition must be something the run prints (a time clause on printed `date`).
   https://code.claude.com/docs/en/goal
5. Tell it about compaction: save progress to disk, don't stop for token budget.
   https://platform.claude.com/docs/en/build-with-claude/prompt-engineering/claude-prompting-best-practices
6. Claude "will happily spend hours running tests instead of making progress": fast samples, one grep-able ERROR line.
   https://www.anthropic.com/engineering/building-c-compiler
7. No "double-check" lines (over-verification on Opus 5); one evaluator near the end, correctness gaps only.
   https://www.anthropic.com/engineering/harness-design-long-running-apps ; https://code.claude.com/docs/en/best-practices
8. **Roleplay:** a one-sentence role sets focus and tone; expert personas gave no significant accuracy gain (Wharton "Playing Pretend",
   2025; an EMNLP 2025 study: "positive or non-significant"). So tonight's personas are critics that propose, and checks decide.
   https://papers.ssrn.com/abstract=5879722
9. An overnight game run with parallel subagents, each delivering a before / after screenshot (UNVERIFIED summary of a video).
   https://daily.dev/posts/i-let-claude-improve-my-game-while-i-slept-vwmrfeyvk

## D. What the forge took from the playbook (game-prompt-playbook OUTCOMES.md)
- A minimum run length needs an elapsed-time goal clause AND a seeded leftovers list, or the run ends at its list (SLAPSTICK 43-45 ended
  near 2 h of 7).
- Size caps to the loop, not the list (SLAPSTICK 52: BUILD took 1:00 of a 3:30 box).
- Commit every slice; a usage stop can eat hours (BEAT RALLY 05, 07; SKYGRIND NIGHT19).
- Measuring is the bottleneck: one heavy job at a time, meters on a frozen copy (BEAT RALLY 08).

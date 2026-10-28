# M1: What works for AI-built games (research, 2026-10-10)

Scope: web research only. Sources were read via fetch or search snippets; "(snippet)" means I saw only the search summary. Anything marked "inferred" is my reasoning, not a source claim. Few sources give hard numbers; the numbers below that are not cited are proposals for SKYGRIND, labelled as such.

## (a) Do this tonight: 12 items, ranked by expected impact

1. **Write target bands before touching any number** (proposal): boss kill time 40-80 s for a median run on a fresh pistol-level loadout, 25-45 s for a good run; basic grunt 1-3 s; grapple uptime under 25 percent of a round. Put them in `Research/` or `docs/` as a table the sim must satisfy.
2. **Build a headless balance sweep on the existing sim harness**: for each boss and each weapon, run N=200 seeded bot fights, print kill-time p10/p50/p90, hit rate, damage taken. Fail the check if p50 is outside the band. godot-autosim does exactly this shape (thousands of seeded headless runs, win rates, JSON report, regression assertions): https://godotengine.org/asset-library/asset/5002
3. **Cut boss HP to hit the band, one variable per commit.** Change only `bossHealth` (or only a hit-window), rerun the sweep, record the table in the log. Same for the grapple.
4. **Make the grapple a limited verb**: a cooldown plus a distance- or use-based lockout. Titanfall's grapple is 5 s; Apex Pathfinder scales cooldown by distance travelled (10 s minimum, 35 s maximum) (snippet, https://apexlegends.wiki.gg/wiki/Pathfinder). Doom Eternal's creative director says restricting the meathook to enemies is "what will make the meathook feel smart" (snippet, https://www.pcgamesn.com/doom-eternal-intelligent-design). GG's own rule (CORE.md): no movement verb without a long cooldown.
5. **Adopt a "fun metric on screen" rule**: every prototype shows the one number being judged as debug text (prototype-fast skill, step 5): https://claudeskills.info/skills/gamedev-skills/awesome-gamedev-agent-skills/prototype-fast/
6. **Replace each arena with a 3-minute greybox, judged before any dressing.** Derive metrics from the mover first (jump height and distance, grapple range), block out in primitives, then play: https://claudeskills.info/skills/gamedev-skills/awesome-gamedev-agent-skills/level-design/
7. **Render a top-down map plus two eye-height frames per arena** and have the agent judge them against a checklist (verticality, sightlines, three routes, cover density) before GG sees it. Flat levels come from top-down paper design that forgets the third dimension (snippet, https://gamedev.dou.ua/blogs/level-design-basics-part-2/).
8. **Start a playtest-feedback database**: GG's spoken reactions become rows (title, detail, priority, category, done) that Claude dedupes and lists. https://www.jackfranklin.co.uk/blog/claude-code-ai-feedback-skill/
9. **Run a degenerate-strategy bot** (spam grapple; camp a corner; only pistol) against every boss and level. Fix any bot that wins with one verb. This is the "dominance / unkillability" check of the ags balance-check skill: https://skillsmp.com/creators/lordkuper/agentic-game-studio/claude-skills-ags-balance-check
10. **Feed GG's words back as the spec, not an adjective list**: "say what's wrong, not how to fix it" worked best in a 2-5 minute playtest loop: https://gonzoml.substack.com/p/how-we-built-a-full-browser-game
11. **Audit data against code**: every config flag must be referenced by code (that project found 15 flags defined but never checked). Cheap grep-based test over `config.json` keys.
12. **Freeze new features until items 1-3 are green.** A one-night "no new verbs" rule; the failures below come from adding variety without identity.

## (b) Findings by topic

### 1. Workflows that produce fun
- Code generation is not the bottleneck. "Game design ... still depends on a person testing, judging, and iterating." One 10,400-line AI-built 3D game shipped with reversed controls, a missing back button, a pickup bug and untested balance; playtesting found all of it within five minutes; the AI itself listed balance as untested: https://www.mindstudio.ai/blog/librarian-2-vibe-coded-3d-game/
- Play-fix cycles of 2-5 minutes over dozens of rounds beat upfront specs ("10x more productive"). Balance there was iterative (economy changed 5 times, one weapon 3 times), driven by play observation. Power-ups felt better as dramatic effects than +20 percent: https://gonzoml.substack.com/p/how-we-built-a-full-browser-game
- Prototype-fast procedure: one binary question, a hard timebox (30-90 min per mechanic), greybox everything else, instrument the metric, then watch an uncoached player. Keep if a fresh player does the core verb unprompted within about 30 s and repeats it unasked; kill or retune if the verb needs explaining: https://claudeskills.info/skills/gamedev-skills/awesome-gamedev-agent-skills/prototype-fast/
- Reference-game teardown: I found no source with boss HP or duration numbers for DOOM Eternal or ULTRAKILL (search returned none). Do the teardown from the web build's own recorded runs, or measure by video: not checked.

### 2. Balancing with automation
- Automated bots report time-to-kill, deaths, time to checkpoints, damage per second, win rates by playstyle (snippet, https://arxiv.org/pdf/2304.08699 and the RaidEnv boss-raid benchmark, https://arxiv.org/pdf/2307.01676). Agents play in minutes what testers need days for.
- Monte Carlo is valid when rules are formal; one dice roguelike developer ran millions of simulated combinations per chapter (snippet, see https://www.thepromptindex.com/skill/1361-game-balancing). SKYGRIND's fixed 120 Hz deterministic sim and Mulberry32 are exactly that precondition (inferred).
- godot-autosim: seeded, headless, thousands of runs, JSON report, assertions for balance regression in CI (link above). It needs "a game adapter and a bot strategy": SKYGRIND has both in its harness (inferred).
- ags balance-check output format worth copying: HEALTHY / CONCERNS / CRITICAL, an outliers table, degenerate-strategy list; fix, rerun, repeat. It publishes no numeric bands, so the bands in (a)1 are mine.
- Heatmaps: kill and death maps binned by position, subtract for a balance map, normalise to [-1,1]; the source says it needs millions of rows, so impractical early: https://www.gameanalytics.com/blog/balance-and-flow-maps. Inferred for us: bot-run heatmaps of grapple attach points and boss-camp positions are cheap because bots generate the volume.
- Grapple spam: see (a)4 for Titanfall, Apex and Doom Eternal restraint.

### 3. Visual review loops
- Claude Code opened a browser game, took screenshots and caught a camera clipping through a ceiling and a dominant mechanic (https://www.mindstudio.ai/blog/librarian-2-vibe-coded-3d-game/). Counterpoint: screenshots "consumed a lot of tokens and was fairly hit or miss", and static frames miss feel (snippet, Franklin / mindstudio threads). So use frames for geometry and readability, numbers for feel.
- Level metric checks that are not visual: a flood-fill reachability test from spawn to goal and to every upgrade; a pacing curve of beats scored 0-1 that must be a sawtooth, not flat-high; "SAFE_GAP = 70 percent of max jump distance, HARD_GAP = 95 percent" (level-design skill above). Inferred: for SKYGRIND use the real Mover jump and grapple numbers.
- Inferred: a fixed camera set per arena (top-down orthographic, entrance, centre, boss spot) rendered by the harness gives before/after frames without Play mode.

### 4. Unity tooling (see (c))
Facts only; details in (c).

### 5. Failure modes and countermeasures
- **Feature soup**: models "describe games ... but [are] terrible at committing to one"; variety with "almost no identity". Countermeasures: human-locked core verb, camera, input and failure states; AI limited to bounded variation; config schema gating output: https://dev.to/seagamesai/my-first-ai-generated-game-looked-great-on-paper-and-boring-on-screen-5eo4
- **Looks right, plays wrong**: enemies stuck in corners, difficulty spikes, unexplained damage (same source). Countermeasure: sim sweeps plus degenerate bots.
- **Ghost features**: 15 data flags never checked in code, a score read after reset, state shared between two abilities: https://gonzoml.substack.com/p/how-we-built-a-full-browser-game. Countermeasure: data-to-code audit tests.
- **Entangled architecture and style chaos**: keep system design human-owned: https://eastondev.com/blog/en/posts/ai/20260523-ai-game-dev-judgment/
- **Untested numbers**: the AI self-reports balance as untested (mindstudio). Countermeasure: band plus sweep as a gate, not a note.
- **Flat boxes**: forgetting the third dimension in top-down layout (level design link above). Countermeasure: metrics first, blockout, play, then dress.

## (c) Tooling recommendation

**Landscape (2026):**
- Unity's own in-Editor MCP server (`com.unity.ai.assistant`) is deprecated; MCP moved to the Unity CLI (`unity pipeline install`, `unity mcp configure <client>`); older projects use community MCP for Unity: https://mcpplaygroundonline.com/blog/unity-mcp-server-guide.md. SKYGRIND already runs on this CLI route (project CLAUDE.md).
- CoplayDev MCP for Unity (MIT): `manage_scene` (hierarchy, screenshot), `read_console`, `run_tests`, `manage_profiler`, `manage_graphics`, `execute_code`, custom tools via `[McpTool]` (same guide). Needs Python 3.10+ and `uv`: https://sorceress.games/blog/compare-the-best-ai-for-unity-coding-honest-2026-pick
- IvanMurzak Unity-MCP: 70+ tools, Roslyn runtime C# execution, EditMode/PlayMode tests, console, profiler, screenshots, any C# method as a tool: https://github.com/IvanMurzak/Unity-MCP
- Coplay (paid) and Coplay MCP: 86 tools, "visual feedback/verification", profiling; MCP "free for now": https://coplay.dev/blog/coplay-vs-coplay-mcp-vs-unity-mcp. The comparison page does not detail play-mode or capture tools.
- Gameplay-testing servers: Playcaller (screenshot, game-view pixel read, key press, tap, drag, playmode, wait, set game-view size, `game_query` into the running game) https://glama.ai/mcp/servers/takashicompany/playcaller; Unity Code MCP (`play_unity_game` unpauses, applies Input System actions, collects logs, pauses; game-view screenshot) (snippet, https://mcpservers.org/es/servers/signal-loop/unitycodemcpserver).
- Caveats from the same guide: domain reloads break in-flight calls, the Editor must stay open, `execute_code` runs arbitrary C#.

**Honest verdict:** none of these lets an agent judge fun. All of them are variants of what the project's CLI already has (eval, capture_game_view, logs, tests). Swapping plugins will not fix balance; the sweep and bands will (inferred).

**Capabilities that would add value over eval + capture_game_view:**
1. **Paused, stepped play** (Unity Code MCP `play_unity_game` pattern): run N sim steps with scripted input, pause, capture. Gives deterministic before/after frames of one moment (a boss hit window, a grapple pull) (inferred use).
2. **A game-state query tool** (Playcaller `game_query`; CoplayDev/IvanMurzak custom tools): expose `SurvivalGame` state as JSON: boss HP, grapple state, player velocity. Read numbers beside each frame.
3. **Frame sequences** at fixed 120 Hz step counts rather than single captures, to compare before/after tuning.
4. **Profiler tools** (`manage_profiler`) for frame-time claims, replacing manual bench runs (the Editor must still be idle; CLAUDE.md L-005).

**How to use it:** keep the current CLI as the base. Do the balance work in the headless harness (no Editor, so no focus stalls). Reserve the Editor for one capture set per arena and one Play check per boss. If adding anything, add custom `[McpTool]`-style commands to the existing CLI for state JSON and stepped capture, not a second bridge (inferred; avoids two Editor connections, a hard rule in CLAUDE.md).

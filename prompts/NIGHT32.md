# NIGHT32.md: bosses in a minute, a grapple with charges, a round ring, level 4 again, level 5 (lead + six builders)

GG, 2026-10-10 18:00 (verbatim in Research/2026-10-10-wave32/GG-REQUEST.md), after playing NIGHT31: the bosses have far too much health
and you can barely hit them (defeat them in about a minute); you can spam the grapple; the arenas are boring; make level 5; overhaul level 4
again; add new arenas on existing levels; the first boss arena must be a circle; do it with the best methods for making games with AI;
"spin up multiple subagents". The leftover list of NIGHT31 is NIGHT32.draft.md (outranked by this).

Base: port/stage3 at 201317d6 (shared commits cc751f22 + 201317d6 on top of u3-n31). t0 = 17:58 (GG's message). Research and audits
(0:00-0:20) are in Research/2026-10-10-wave32/. Design: Design/levels/DESIGN32.md (the cards B, C, G, A, S, F and the four rules).
Defaults: DECIDE32.md.

<method>
The methods tonight (M1), in force for every builder and the lead:
1. A target band before a number; a seeded harness sweep (p10 / p50 / p90) proves it; the table goes in the commit message.
2. Degenerate bots (campers, spammers) against every boss and verb: a degenerate strategy that wins or is safe is RED.
3. Look at it: a top-down render of every new room (level-svg + headless Chrome PNG, opened and judged), eye-height Player frames by the
   lead each round.
4. One variable per tuning commit; the Player decides over the harness (L-029, L-040).
5. Small slices, committed: every finished slice is a commit (L-003: the spend limit stops every agent at once).
</method>

<builders>
Six builders in git worktrees (`isolation: "worktree"`), launched at about 0:35 with DESIGN32.md. Each merges `port/stage3` before starting
and before every report, runs `bash Tools/night/dn-check-wt.sh <its scratch dir>/chk` before each commit (0 errors; prove the checker red
once, L-034), runs the harness gates `dotnet run --project Tools/SimHarness -c Release -- fingerprint` and `... fingerprint --off32` (both
EQUAL) before each commit that touches sim code, never touches the Unity Editor, a build or a Player, and RESUMES by SendMessage each round.

| Builder | Owns (new files unless stated) | Card |
| --- | --- | --- |
| **B** bosses | `Core/Enemies/{Lantern31,Stag31,Wyrm31,Loom31}.cs` (numbers, gates, hurtboxes), `Game/SurvivalSim.Boss32.cs` (bosses32 part), `Game/BossMarks32.cs`, `Game/Bot/*Pilot*` for the four bosses (not LanternPilot30's geometry), `Tools/SimHarness/Harness.N32.Boss.cs` (`boss32`, `camp32`), `Game/FeelReel.N32.Bosses.cs` (`bosses32`, `camp32`), rows `design.n32.bosses32` and `bossMarks` | B |
| **C** crown | `Core/Enemies/Crown32.cs`, `Core/Enemies/Crown32Arena.cs`, `Game/SurvivalSim.Crown32.cs` (implements N32CrownStartRun / N32CrownStep, shared commit 3), `Game/Crown32View.cs`, its pilot, harness `crown32` in `Harness.N32.Crown.cs` (the N32Crown partial), reel `boss5` in `FeelReel.N32.Crown.cs` (N32CrownBody), row `design.n32.boss5`. The routing line for `SurvivalSim.Boss25.cs` goes to the lead as a `.cjs` | C |
| **G** grapple, balance, HUD | `Core/GrappleTap31.cs`, `Core/GrappleAim.cs`, `Game/SurvivalSim.Grapple31.cs`, `Game/SurvivalSim.Grapple32.cs`, `Game/SurvivalSim.Balance32.cs`, the grapple pips view, the HUD fixes (the boss-fight countdown, the LT label, the lesson clear: the Ui files they live in), `Harness.N32.Grapple.cs`, `FeelReel.N32.Grapple.cs`, rows `grapple32`, `balance32`, `grapplePips`, `hud32` | G |
| **A** arenas | `Core/Enemies/Lantern30Arena.cs`, `Game/Boss30View.cs`, `Game/LavaView30.cs`, `Game/Bot/LanternPilot30.cs`, `Game/Boss31View.cs` lines 208-212 only, `Tools/night/gen-n32-{foundry,iceline,kiln}.cjs`, `Design/levels/n32/{foundry,iceline,kiln}.plan.json`, `Design/levels/n32-{foundry,iceline,kiln}.json`, `Harness.N32.Arenas.cs` (incl. the 64-facet `seam30` lap), `FeelReel.N32.Arenas.cs`, row `arenas32` | A |
| **S** span | `Tools/night/gen-n32-span.cjs`, `Design/levels/n32/span.plan.json`, `Design/levels/n32-span.json`, `Harness.N32.Span.cs`, `FeelReel.N32.Span.cs`, row `span32` | S |
| **F** finale | `Tools/night/gen-n32-spire.cjs`, `Design/levels/n32/spire.plan.json`, `Design/levels/n32-spire.json`, the level-5 registration (map row, level-select slots under the row, `design.n31.unlocks.levels.rows` n 5, `Design/levels/unlocks.json` n 5, lighting / props entries), `Harness.N32.Finale.cs`, `FeelReel.N32.Finale.cs`, row `level5` | F |

Shared rules: each builder edits only inside its own `design.n32.<row>` objects in `Design/unity-config.json` (plus F's n 5 unlock row),
never a value under n24-n31 (overrides only, read while the n32 row is on: `--off32` stays NIGHT31's game). `Assets/StreamingAssets` copies
by `bash Tools/night/n24-sync.sh` in the same commit. Typed level lists in older harness / reel files (A2 risk 1) are the lead's: name the
line. **The lead alone** touches `SurvivalSim*.cs` other than the new files above, `SurvivalGame*.cs`, `FeelReel.N27.cs`,
`Tools/SimHarness/Program.cs`, `RunTape.cs`, `FingerprintBaseline*.cs`, `Core/Curriculum25.cs`, the config merge, every build and reel.
</builders>

<lean_rules>
- **Proof:** a row a player sees is DONE only with a Player frame looked at and a Player number from tonight; RED once (--off32 /
  -skygrindOff32), GREEN once. A harness line alone is "SHORT: harness only".
- **Golden at every Core commit (rule 3):** fingerprint + --off32 + golden + trajectories; `Mover.cs` never edited; JsMath, Mulberry32 only.
- **Config:** Unity-only numbers in `design.n32.<row>` with a `_why`; patch scripts are `.cjs` files written with the Write tool; no Unity
  API in MonoBehaviour field initializers (L-035); boxed numbers in level rows are doubles (L-043: PropsView29's float).
- **Caps:** Player builds 10; reel invocations 24; suite runs 2 (batch); builders 6 alive.
- **Heavy jobs one at a time** (L-005): never a reel, build or suite beside another; builders' dotnet runs are small.
- **Commits:** every finished slice, explicit paths plus `.meta` files; never `ProjectSettings/UnityConnectSettings.asset`,
  `Assets/Scenes/Arena.unity`, `Assets/ProjectSettings/UIEffectProjectSettings.asset`, `ProjectSettings/EditorBuildSettings.asset`; never
  master, never push; restore the TMP font assets Play mode rewrites; nothing deleted; night prompts untracked, logs tracked.
</lean_rules>

<tasks>
## U3-45.1 Research, review, shared (0:00-0:35) DONE before launch
Five subagents (three web, two audits), the lead's frame review, shared commits cc751f22 and 201317d6, DESIGN32.md, DECIDE32.md.
## U3-45.2 Build rounds (0:35-4:15): integrate at about 1:35, 2:30, 3:25
Each round: `Tools/night/integrate.sh` (tonight's ids), sync, Editor compile, dn-check 0, fingerprint + --off32 EQUAL, golden +
trajectories when Core changed, ONE batch Player build, ONE reel invocation for what landed, frames looked at, builders resumed with the
Player's exact lines. Order of landing: G's charges and B's four retunes in round 1 (GG's first two complaints); A's ring in round 1-2;
S's level 4 and F's level 5 walkable in round 2; C's boss in round 2-3; arenas in round 2-3; balance32 and hud32 any round.
## U3-45.3 Freeze (4:15) and verification (4:15-5:00)
`--off32` fingerprint EQUAL; one batch suite alone; the Player smoke with rows ON; LevelSelect29 (now with 5 levels if level5 is on) and
DoorFree29; the boss kill-time and camp tables in the Player; frames of the ring, each new arena, level 4 and level 5; NIGHT32-PROOF.md.
## U3-45.4 Docs and the close (5:00-5:40)
NIGHT32-LOG.md close table, NIGHT32-REPORT.md (one page for GG), FEEL-PLAYTEST.md, HANDOFF.md block, PORT-STATUS.md events,
docs/memory harvest 32, NIGHT33.md, tag `u3-n32`. Hard cap 6:00 (23:58).
</tasks>

<closing_reminder>
- Every boss: a moving pilot kills it in 45-75 s with its level's weapons, every camper loses, and no spot the player can stand on is
  "can't hurt it from here".
- The grapple: a spam bot gets 2 pulls, then one per 4 s (plus kills).
- Switches and exactness: every sim change behind `design.n32`; golden 0 mismatches; fingerprint EQUAL with the rows off; never `Mover.cs`.
- Every lead message ends with `N32-STATUS: round <r> · builders <alive>/6 · rows on <n>/10 · <h:mm> / 6:00`.
</closing_reminder>

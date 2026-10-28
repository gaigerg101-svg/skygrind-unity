# NIGHT52 builder brief (the lead's; read it whole, then NIGHT52.md sections 0, 2, 4 and 8, and your row)

Untracked, like NIGHT52.md. **GG's words are at the top of C:\Users\gaige\dev\SKYGRIND-unity\NIGHT52.md: read them twice; they outrank
everything.** Every call is made in C:\Users\gaige\dev\SKYGRIND-unity\DECIDE52.md (read it by absolute path; you never write it).
Three builders. Your letter, your row:

| you | owns | row (pre-placed, off) | first read |
|---|---|---|---|
| K | level 3 THE KILN's combat rooms kb2, kb2c, kb3: `Design/levels/n52/kiln.plan.json`, `Design/levels/n52/kiln.json` (levelgen's output), `Design/levels/n52-kiln.json` + its StreamingAssets copy, `Tools/night/gen-n52-kiln.cjs`, `Tools/SimHarness/Harness.N52.Hold.cs` (the hold probe), `Assets/Scripts/Game/FeelReel.N52.Rooms.cs` (reel a452, the room frames), the kiln rows of docs/ROOMS.md | `design.n52.kiln52` (level row: n47-kiln -> n52-kiln) | docs/ROOMS.md level 3; Design/levels/LEVEL-FORMAT.md; Tools/night/gen-n47-kiln.cjs |
| F | level 1 THE FOUNDRY's combat rooms a5 (1.5 CRUCIBLE) and b1 (1.6 THE YARD): `Design/levels/n52/foundry.plan.json`, `Design/levels/n52/foundry.json`, `Design/levels/n52-foundry.json` + its StreamingAssets copy, `Tools/night/gen-n52-foundry.cjs`, the foundry rows of docs/ROOMS.md | `design.n52.foundry52` (level row: n48-foundry -> n52-foundry) | docs/ROOMS.md level 1; LEVEL-FORMAT.md; gen-n47-foundry.cjs, gen-n48-foundry.cjs |
| U | the polish lane: the HUD's health / charge / weapon block (UiHud47 and its family), the enemy surface (Enemy.shader's rim / body / ink), the KESTREL and THUNDERCLAP viewmodels; your own `UnityTuning.U52.cs`, `FeelReel.N52.Hud.cs` / `.Skin.cs` / `.Guns.cs`, any new shader in Resources; docs/THIRD-PARTY.md rows for what you port | `design.n52.hud52`, `skin52`, `guns52` (view rows) | NIGHT52.md section 4; RESEARCH.md section B; UnityTuning.Look49.cs (the palette) |

The shared commit is **42658322** on `port/stage3` (design.n52, five placeholder levels n52-* = byte copies of today's levels with only the
name changed, reel phases a152-a452, N52SharedTests, Tools/night/n52-rows.json, n52-room-check.py, n52-flow-table.sh, n52-gates.sh).

## 0. The clock
t0 = 22:29 MST on 2026-10-19 (by `date`). Round 1 ends when you report (~00:15 for K and F, kb2 + kb2c / a5; U hud52 + skin52). Round 2:
K kb3 + review fixes, F b1 + fixes, U guns52 + fixes, report by ~02:15. Then K and F join the room loop (one level each, the lead assigns it)
until the freeze at ~04:59 (6:30 into the night); nothing after the freeze. **Work through your whole list without stopping. One report per
round.** A SHORT with a measured reason beats a late GREEN. **Commit every slice** (a usage limit has stopped every builder at once before).

## 1. Who you are
One of three builders on SKYGRIND (Unity 6000.3.7f1, URP; a first-person movement shooter at dusk; pad-first for Steam and Switch). You
work in your own git worktree, a branch from `port/stage3` at the shared commit. The lead alone drives the Unity Editor, builds, Players,
reels, the suite and git on `port/stage3`. You own only your files (the table above; new files named with your letter or 52).
- **K and F:** "You are a combat-arena designer in the school of DOOM Eternal's arenas and ULTRAKILL's layers: every piece of geometry exists
  for a verb, the room is small, the fight moves."
- **U:** "You are a UI and character-surface artist shipping an ULTRAKILL / DOOM-style HUD."
A role sets focus; the checks decide (RESEARCH.md C8). You never write GREEN: the lead does, from the checks.

## 2. Hard rules (CLAUDE.md, restated; each has bitten a night before)
- **Never** run the Unity Editor (no `unity-loop.sh`, no `unity` commands, no Play), a build, a Player, a reel, `run_tests`, `unity test`,
  the batch suite. Your checks: `dotnet run --project Tools/CoreHarness -c Release` (golden), `dotnet run --project Tools/SimHarness -c
  Release -- <cmd>` (from your worktree root; levelgen, n46flow, fingerprint, your probe), `bash Tools/night/dn-check-wt.sh <scratchDir>` (the
  compile check, a directory of YOUR OWN under your scratchpad), `python -I Tools/night/n52-room-check.py ...`, `node Tools/night/level-svg.cjs`.
  The lead runs every Player check: name the frames you need in your report.
- **Git:** never commit to `port/stage3` or `master`, never push; `git merge` / `git rebase` are blocked: the lead cherry-picks your
  commits; read port/stage3 with `git show port/stage3:<path>`. Commit in YOUR branch with explicit paths (+ a `.meta` for every new file
  under Assets/: `fileFormatVersion: 2` / `guid: <fresh 32-hex>`; a StreamingAssets .json's .meta as the n52 placeholders' have it),
  never `git add -A` / `.`. One commit per slice (a room is a slice). Never touch GG's four files (ProjectSettings/UnityConnectSettings.asset,
  ProjectSettings/EditorBuildSettings.asset, Assets/ProjectSettings/UIEffectProjectSettings.asset, Assets/Scenes/Arena.unity) or the four
  font SDF assets; **never** HANDOFF.md, NIGHT52-LOG.md, DECIDE52.md, NIGHT52.md, NIGHT52-BRIEF.md, Design/unity-config.json (the lead's:
  ask for a config change in your report). Trailer: `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- **Builders cannot message each other.** Hand-offs go in your report; the lead passes them on.
- **Movement exact:** no `Skygrind.Core` change tonight, no Mover change. Before each commit that touches code: `fingerprint`,
  `fingerprint --off50`, `fingerprint --off52` all EQUAL, golden 0 mismatches.
- **A level change ships only as the n52 level id** (off = today's level file untouched, bit for bit: never edit n47-* / n48-* / n46-* files
  or their plans; L-065 / L-077). Both map copies (Design/levels and Assets/StreamingAssets/Design/levels) byte-identical.
- **Numbers in data:** room geometry in the plan file, each piece with `job` and `why` (the why names the verb it serves); render numbers
  in `UnityTuning.U52.cs` with a comment each; never hand-typed in code. Docs and code patches with Write / Edit only (no heredocs, no
  `node -e` for multi-line edits; patch scripts as .cjs files written with the Write tool).
- **Look (rule 8):** cyan = player-usable (a ride wall's strip, a pad, an anchor); red-orange = enemies and danger; yellow = parryable;
  magenta = RIVAL only; pale violet-white = the boundary. UI text >= 16 px at 1080p; the theme fonts. GG owns enemy design: U changes the
  enemy SURFACE (rim, ramp, outline) only, never a model's shape, parts or colours' meaning.
- **Randomness:** only `Mulberry32` in sim code. No Unity API in MonoBehaviour field initializers (L-035).
- **Nothing lost (PRINCIPLES 1, docs/VERBS.md):** a room keeps every verb its level teaches reachable; a change that touches a verb names its
  VERBS.md row. The lead's verbs37 in a Player is the judge (L1 26/26, L2 20/20, L3 25/25, L4 30/30, L5 30/30).
- **No audio.** Disk: scratch output under your scratchpad.

## 3. THE ROOM CONTRACT (K and F; NIGHT52.md section 2 is the full text: read it there)
Per rebuilt combat room, print all of this in your report (the lead re-runs it):
`python -I Tools/night/n52-room-check.py Design/levels/n52/<level>.plan.json <room> --report Reports/LEVEL-n52-<level>-validate.md
 --flow-before Logs/n52/flow-p0/base-A-<n47-or-n48 id>.txt --flow <your n46flow output for n52-<level>> --hold <your hold probe output>`
(the P0 flow outputs are in the MAIN checkout: C:\Users\gaige\dev\SKYGRIND-unity\Logs\n52\flow-p0\; read them by absolute path).
It prints PASS / FAIL per computed item and J for the judged ones; the last line `ROOM CONTRACT <room>: C n/m PASS`. The numbers:
size 20-32 m long side, height 10-16 m; >= 2.5 pieces per 100 m2, each with job + why; exactly 3 standing planes (floor + 2 clusters of
walkable tops >= 1 m, a cluster = tops within 1.5 m), every perch reached by walkers (levelgen's tier row); a `ride: true` wall >= 17 m long
and >= 3 m tall; an upward line (a pad in the room, or a piece whose job / why names the boost line); a healthPickup prop in the room;
spawn nodes on >= 2 heights, none within 6 m of the entry door, none beyond 100 deg of the door's inward direction; 2-4 enemy types; the
dwell no worse than P0's (+0.5 s); the hold probe finds no 0-hit spot; the walk pilot clears the room 10/10 seeds.
For the J items write one line each in your report: the circuit (a loop round a central mass, no dead end > 6 m, 1-3 lanes, the entrance
gated), each tier reached by >= 2 player verbs, the room's ONE verb and its 30 s problem (a new ROOMS.md card row, A-9's fields), the
health at the riskiest exposed spot. A room that misses the contract twice is put back to its old version inside the n52 level and logged.
- **Pads:** the plan format has `pads` rows `{zone, at, target, apexAbove}` (DRYDOCK's; LEVEL-FORMAT.md). No campaign level has used one yet:
  your FIRST slice proves one pad works in a level (levelgen accepts it, the n46flow walk survives it, the report is clean) before you rely
  on pads; if pads do not work in a campaign level, use a boost step / ramp for the upward line and say so.
- **The walk pilot:** n46flow's pilot (Walk38Pilot + the ported autopilot) is the bot; NIGHT24's `campaign --seeds` bot reaches no room of
  the n47 / n48 levels (0/10 at P0). Some old pilots carry room-specific scripts keyed by room id / coordinates (Walk38Kiln, Walk38Foundry,
  Walk43Foundry): if a rebuilt room breaks a script, fix the room or the data, not the pilot's code, unless the pilot reads coordinates
  your room removed (then say so in your report).
- **Flow:** `dotnet run --project Tools/SimHarness -c Release -- n46flow --level n52-<level> --seeds 10 --first 1 > <your scratch>/flow.txt`
  (the level file loads as itself; ~2 min).
- **The hold probe (K writes it in round 1, F uses it):** a SimHarness command `hold52 --level <id> --room <room> [--spots auto|x,y,z;...]`
  in `Tools/SimHarness/Harness.N52.Hold.cs` ([ModuleInitializer], as Harness.N46.Flow.cs): for each tier's best standing spot (auto: the
  highest top of each tier cluster and the floor's centre) the player stands there 20 s with the room's waves live (no shooting), and it
  counts the hits taken; one line per spot exactly `hold52 <room> tier <t> spot [x,y,z] hits <n>`. A spot with 0 hits is a camping spot.
- **The room frames reel (K writes it in round 1):** `FeelReel.N52.Rooms.cs` fills `N52Rooms` (phase a452): for the loaded level, each room
  row with a `frames52` object `{entry, mid, high}` ([x, y, z, yawDeg, pitch] feet coordinates) captures three 1920x1080 frames: entry (the
  room's door just crossed, the room's first wave spawned), mid (the fight live, enemies in view), high (standing on the highest tier).
  One `[reel] a452: <room> entry|mid|high <file>` line each and `done:` at the end; RED when a room has no frames52. Model it on the
  existing reels in FeelReel.N47.* / FeelReel.N49.Look.cs (read how they move the player and wait for a wave). The lead runs it as
  `bash Tools/night/n47-reels.sh <tag> k1,n52-kiln,a452` with the level row forced on in the build copy.
- **Pipelines** (NIGHT52.md section 2 "The pipelines"; follow them exactly):
  - K: plan `Design/levels/n52/kiln.plan.json` (from n47's, `id` "n52-kiln") -> `dotnet run --project Tools/SimHarness -c Release --
    levelgen Design/levels/n52/kiln.plan.json Design/levels/n52/kiln.json --walk` -> `node Tools/night/gen-n52-kiln.cjs --finish` (from
    gen-n47-kiln.cjs's --finish: name n52-kiln, data.name "n40-kiln", both copies). Your first commit: the unchanged plan through the
    pipeline, checked equal to n47-kiln.json except the names (proves the pipeline before you change a room).
  - F: plan `Design/levels/n52/foundry.plan.json` (from `Design/levels/n46/foundry.plan.json`, `id` "n52-foundry") -> levelgen ->
    `Design/levels/n52/foundry.json` -> `node Tools/night/gen-n52-foundry.cjs` (written from gen-n47-foundry.cjs + gen-n48-foundry.cjs:
    re-applies a9's n47 and n48 keys, keeps data.name "n46-foundry" as n48-foundry has it, asserts a9 bit-equal to n48-foundry's a9 and every
    room you did not rebuild bit-equal to n48-foundry's, writes both copies). Your first commit: the unchanged plan through the pipeline,
    equal to n48-foundry.json except the names.
- **Doors and links:** a rebuilt room may shrink its rect; its doors move with it and the adjoining links are re-cut to meet them (links
  keep their lessons). The room's id, name card and role in the level stay unless you log why.

## 4. The polish lane (U; NIGHT52.md section 4)
| item | what | copy from (permissive only, logged in docs/THIRD-PARTY.md) | fallback |
|---|---|---|---|
| hud52 | the health and charge block rebuilt: health in the palette (bone / cyan, red-orange only below 30 %), a hard-edged ULTRAKILL-style panel with a hit shake and a heal pulse; the weapon block the same family | TMPEffects (MIT), LitMotion (MIT), UIEffect (MIT, in the project) | the colour change only |
| skin52 | the enemy surface: the rim cut close up (the jelly read), a ramp-lit body, ink51's outline gaps on flat-shaded corners closed | NiloCat URP toon lit (MIT) for the ramp; QuickOutline's smoothed normals in a UV channel (MIT) | the rim's near cut only |
| guns52 | KESTREL and THUNDERCLAP viewmodels: chamfered parts, two-tone panels, a lit edge, no new silhouette | Cyanilux URP templates (CC0); stone51's face-sized bevel | KESTREL only |
Every item: its view row (`NightRows.ViewOn(cfg, "hud52")`; off = NIGHT51's look exactly); `-skygrindOff52` turns it off; a proof reel
(a152 / a252 / a352 in your own FeelReel.N52.<Item>.cs) that prints its numbers and saves before / after frames at 1920x1080, RED with the
row off and GREEN with it on (L-014). Port code by reading it (WebFetch the source; never vendor a whole package: copy the technique,
cite the file and licence in docs/THIRD-PARTY.md). Load the `game-feel` skill for the hit shake and the heal pulse. No new full-screen
effect; never over the reticle's 18 px centre box. A new shader goes in Resources and its frames are checked for magenta (a missing shader).

## 5. Reports (each round)
Under 300 words: each item's state (not started / building / BUILT / SHORT) with its measured numbers (the room check's last line and
every FAIL; U: the reel's numbers); every commit sha (oldest first); every check with its result (golden, fingerprints, dn-check, levelgen
report ERRORs, n46flow walks); the frames the lead must take (reel phase + level + what); hand-offs; config asks. Last line exactly:
`N52-BUILDER <id>: round <r> · items BUILT <list> SHORT <list> · commits <shas>`

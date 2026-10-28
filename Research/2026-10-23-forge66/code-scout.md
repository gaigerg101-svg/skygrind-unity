# FORGE66 code scout: the n65 toolchain, the Player path at low disk, shipping the level row (2026-10-23, read-only)

Read from the tree at e184ec15 (port/stage3; u3-n65 = ee2e4839) and measured with read-only commands (`df`, `du`, `ls`, `git`, one
`Get-CimInstance`) at about 21:05. Nothing was built, run or edited. "Not checked" means I did not settle it. Sources: the repo only.

**Now:** C: **8.3 GB free** (`df -BM /c`: 8344 M), commit free 19 GB. The review's S1-S4 are all still open at HEAD (lines below unchanged).
The review's N6 is closed: every room file now carries verb65 (a2 / a8 pierce, a3 ride, a4 parry, a5 launch+slide, b1 launch; all `bot43: false`).

## 1. The toolchain as it stands

**The level's own tools (keep their n65 names: they name the level, not the night)**
- **gen-n65-foundry.cjs** `[--slots=latest|old|a2,a5|<slots65.json>] [--run] [--finish] [--check] [--export] [--dry-plan]`. It assembles
  n52's plan with six slot files and refuses unknown keys (:143), drops with no reason (:151) and moved rects (:144). `--check` proves
  all-old = n52-foundry apart from its names (:368-469).
  - **S1 is open:** `finish()` writes both SHIPPED copies for any slot set (:330, OUTS :39).
  - **New finding:** `--run` calls `finish()` (:511) *before* it exits on a levelgen ERROR (:513), so a level with errors is still written
    into the shipped files. foundry.assembly.json stamps `at: new Date()` (:263), so every run dirties a tracked file; levelgen rewrites
    the tracked Reports/LEVEL-n65-foundry-validate.md.
  - **NIGHT66:** add `--out=<dir>`. Write OUTS only when the spec is SLOTS_FILE (:493); anything else goes to `--out` (default
    Logs/n66/latest/). Move the error exit above `finish()`. Drop the timestamp, or leave it out of the tracked file.
- **n65-slot-set.cjs** `<room> new|old <reason> [--slate=] [--slots=] [--dry] | --print | --selftest`: the only writer of slots65.json; it
  binds a slot to its passSha and blob, and its working-tree diff (:52) is the right check. Self-test 5 / 5. **Trap:** the default slate is
  `Logs/n65/slate.json` (:60), so NIGHT66 must pass `--slate=Logs/n66/slate.json`. SLOTS is hard-coded (:17), fine for level 1.
- **n65-iso.cjs** `<plan|level> <room> <out.png> [--no-card-marks] [--plant-box] [--plant-strip]`: fixed cameras per slot, identical bytes
  for identical input. The 4:58 colour fix is in: floor strips / cyan props in the cyan role (:113-115), cyan lights as strips (:267-273).
  No gap open. The header (:445) names the room, so it goes blind only through **n65-blind.py** (crops rows 60-1070, refuses anything but
  1600x1100, :9-19). Use both as they are.
- **n65-air.cjs / n65-air-rooms.sh**: a static floor-free hand-off graph built from the config's movement numbers; its planted pair is
  GREEN. By design it has no obstacle test and no air control: a guard, not a verdict. Use as is.
- **n65verb (Harness.N65.Verb.cs, VerbPilot.cs)** `launch|pierce|parry|skip --level --room [--pose] [--wave n] [--target x,y,z] [--slide]
  [--verbs65] [--seeds] [--refuse]`. The 4:58 `--wave` / `--target` are in (:174-196).
  - **The skip test's open gap:** a seed counts as "exercised" when the refused run refused a press **or** the allowed run used the verb
    (:518). The two runs are different pilots: the allowed run is the verb-aware pilot (:441-443), the refused run the plain pilot through
    V65Filter (:440, :516). So a4's r4 GREEN read "presses refused 0, the verb used 7 time(s)" (NIGHT65-LOG.md:251): the verb-aware pilot
    against a walker that never tries the verb.
  - **NIGHT66:** wrap the filter round the verb-aware pilot (V65Filter takes any IPlayerInput, :387; "none" passes through), run both seeds
    with `verbAware` (:516), and make the rule `y.refused > 0 &&` (:518).
  - **The red is stale:** Research/2026-10-23-wave65/T/verb-skip-verbs65-red.txt (16:08) predates the exercise rule (71f6577a, 17:59), and
    n65-gate-pack.cjs still reads it (:102-105). Re-take the red on n52-foundry first.
- **n65-verbs.sh / n65-round-verbs.sh**: a pilot per room, then verbs.md. round-verbs hard-codes which room gets which verb (:15-19) and
  writes to Logs/n65 (:8). NIGHT66: read each room's verb65 from the level, and write to Logs/n66.

**The lead's round tools (night-named: regenerate as n66)**
- **n65-red.sh** `[JOBS] [LEVEL] [OUTDIR]`: room check, n46flow on seeds A / B, hold52 pads, the plan PNG, the ride pilot (a3 only, :30). It
  runs the prebuilt DLL and never builds it (:9-10): after a harness pick, rebuild first (a gates run does) or the round is stale.
- **n65-round.sh** `<tag>`: assembles `latest` into the shipped files (:19), runs red, iso, verbs, air and the table, then puts the shipped
  files back (:32). A stop between :19 and :32 leaves the latest level in the tracked shipped files: S1's live hazard. NIGHT66: use
  `gen --slots=latest --out=Logs/n66/<tag>`, and point red, iso and verbs at that file (they all take a level path).
- **n65-table.cjs** `<dir> [reddir]`: walk-bot columns only (:50). Add the skip verdict, the verb pilots and air hand-offs: MAIN moves to
  those (NIGHT65-REPORT.md section 7).
- **n65-gate-pack.cjs** `<id> <round> [--logs --plan --level --out --key --sha --blind --no-skip]`: the GATE pack; it now reads every round's
  reviews (:19). It hard-codes NIGHT65's red verbs (:102-105) and cards (:162): add `--red-verbs` / `--cards`. It has no Player input: add
  `--player Logs/feel/<tag>/<side>` (the room's tour65 PNGs and verbs37 line), plus a SPOT pair taken from the tour65 entry pose (rc0 against
  the GATE build) through n66-spot-pair.cjs, not iso65.
- **n65-d9.cjs** `[rooms]`: walks every commit on the branch (:35). Add `--since <sha>`, or NIGHT65's 49 commits count again.
- **n65-play-build.sh** `<src> <dst> <level> [--config] [--add-level] [rows]`: a hard-link copy that replaces only the config and the level
  (:27-29) and checks the source's shas. It refuses an existing dst (:18), so the 4:50 update was a hand rm + cp. Add `--update`, which
  unlinks and replaces in place after checking the link count is 1.
- **n65-refresh.cs** lists four files by hand (:3). NIGHT66: read `Logs/n66/added-cs.txt`, written from
  `git diff --name-only --diff-filter=A <pre> HEAD -- '*.cs'`.

**The Player-side tools**
- **tour65** (FeelReel.N65.Tour.cs, Tour65.cs) runs through `n6x-reels.sh <tag> tour,n65-foundry,tour65[,-skygrindTourPlant65=a2:x/y/z/yaw/pitch]`.
  **S3 is open:** `green` (:124) ignores `unframed` (:63) and `warns` (:98), and `framed == poses` is always true; Tour65.Solids reads only
  collision.boxes (:119-127, review N3). **NIGHT66, before the code build:** `green = ... && unframed.Count == 0 && warns == 0`; drop the
  tautological term; add wedges to Solids. It has never run in a Player.
- **The bot opt-out:** S4 is open. Walk36.cs:48 is reachable only under `Level36` (Walk29Pilot.cs:203), and Walk43Foundry.cs:281 sits behind
  Walk38Foundry.cs:92. Harmless; fix the comments and N65BotOptOutTests' claim.

**The night tools** (n65-clock, gates, reels, prune, slate*, spot-pair, freeze-rows): see section 4. One more finding: the gates' `--flow`
list is hard-coded without n65-foundry (n65-gates.sh:14). With the row ON, no gate runs the shipped level 1 (review N5).

## 2. The Player path at low disk (measured, not run)

| item | on disk | source |
|---|---|---|
| a Player build | **382 MB** (StreamingAssets 227 MB, of it levels 192 MB) | `du Builds/n64-freeze`; n63-b1 383, n64-rc1 382 |
| one reel side, raw | at most **1800 JPGs** at ~0.46 MB = **~0.83 GB** | ProbeMaxFrames 1800 (UnityTuning.FeelAB.cs:64; FeelProbe.Record.cs:198); f0000.jpg 456 KB; "1800 frames written" (n64-rc2 summary47.txt:41) |
| one verbs37 side, pruned | **~37 MB** (36 JPGs + telemetry 20 MB) | n64-freeze/vf 37 MB, vi 33 MB; a 5-level set 175 MB |
| tour65 on n65-foundry, pruned | **~0.14 GB**: 6 fight rooms, 27 frames65 + 48 line65 = **75 PNG** at ~1.3 MB (the pruner keeps them) + 36 JPGs + telemetry | room files; a157 PNGs 1.28-1.30 MB; n65-prune.sh:18 deletes only f*.jpg |
| the smoke | ~28 KB of log | Logs/player/n56-smoke-steal-n64-freeze.log |
| Library / Temp | 5.9 GB (Bee 2.6 GB, BurstCache 345 MB, ShaderCache 105, ScriptAssemblies 57, PlayerDataCache 50) / 44 MB | `du`; growth per build **not measured**; +0.3 GB over NIGHT65's first 2.5 h (DECIDE65:89) |
| a builder worktree | 709 / 716 / 719 MB (NIGHT65's three) | `du .claude/worktrees/agent-*` |
| NIGHT64's close (build + verbs37 x5 + smoke) | 14.6 -> 13.2 GB, the forge in between: **at most 1.4 GB** | NIGHT64-LOG.md:386; NIGHT65-LOG.md:3-7 |
| NIGHT65, branch C, 6 h | 13.2 -> 8.3 GB (**4.9 GB**: 2.1 worktrees, about 1.5 not found) | NIGHT65-LOG.md:7, :287; DECIDE65 D-6 |

**Pruning.** n65-reels.sh starts n65-prune.sh in a loop (:13), which prunes every *finished* side each 20 s (n65-prune.sh:16-24) and the
whole set at the end (:28). So the peak is the running side raw (~0.83 GB), plus the previous side until the next tick. A re-run of a
scenario deletes its own folder first (FeelReel.cs:241-242).

**Why a hard-link play build cannot carry a code change.**
- The build is Mono: the game code is `SKYGRIND_Data/Managed/Skygrind.Core.dll` (1.96 MB) and `Skygrind.Game.dll` (5.9 MB), both with link
  count 2: the same bytes as n64-freeze.
- n65-play-build.sh swaps only `StreamingAssets/Design/unity-config.json` and one level file (:27-29). That works because the Player reads
  config and levels from StreamingAssets at boot (SurvivalGame.Init.cs:24-29); code is not read from there.
- A code change needs the Player pipeline: DLLs compiled with player defines (the Editor's Library/ScriptAssemblies are compiled for the
  Editor), the serialized scenes and type data (level0 / level1, sharedassets, RuntimeInitializeOnLoads.json, ScriptingAssemblies.json),
  and `unity command build` (CLAUDE.md). Writing a DLL into the copy would also write through the shared link into n64-freeze.
- Hence n65-play-latest lacks look52's tiers (code: UnityTuning.U52.cs:150) and has no tour65 phase (review-n65.md:144-148). What a
  hard-link copy *can* carry is data: room files, slots, config rows, which is every round of a data-only night.

**The smallest verified Player check of n65-foundry.**
- The steps: one build from HEAD's code (tour65 and the S3 fix need it), then `tour,n65-foundry,tour65` and `v,n65-foundry,verbs37` in one
  set, pruned, then the smoke.
- Writes: ~0.38 GB build + ~0.18 GB reels after the prune, plus Library growth (unmeasured; at most ~0.85 by NIGHT64's close). The transient
  peak is up to ~1.7 GB during the set.
- **The binding constraint is the floor, not the writes.** The guard refuses a build, reel set or smoke under 12 GB at its start
  (guard.cjs:27, :239-242); the build script's own floor is 10 GB (n41-build.sh:9), and the reel script skips a side under 10 GB
  (n65-reels.sh:18-19). The smoke, the last step, starts at about S - 1.4 GB, so **S >= 13.5 GB** at the build.
- **Today the floor cannot be met:** 8.3 GB is 5.2 GB short, and nothing the night owns closes the gap. Its own output is Logs/n65 (26 MB)
  and n65-play-latest (4 MB of non-shared files); CLAUDE.md rule 10 and the guard (:190, :216-224) leave the rest to GG. Lowering
  `SKYGRIND_GUARD_MIN_FREE_GB` is a configuration change for GG, and it still leaves the 10 GB script floors.
- What GG can free: 21 superseded builds, ~8.0 GB (n63-b1..b13, n63-freeze, n64-red, n64-b1..b3, n64-rc1..rc3: DECIDE65:8-9; real copies,
  not links; keep n64-freeze, because n65-play-latest shares its bytes); NIGHT65's three worktrees, 2.1 GB; 232 older worktrees.

**What NIGHT66 needs free at t0** (estimates built from the rows above):

| branch | how the night spends disk | needs at t0 |
|---|---|---|
| **A: Player GATEs** | floor 12 + 3 worktrees 2.1 + drift ~1.7 per 8 h + Library ~0.85 + 3 builds (b0 code build, rc1, freeze) 1.15 + rounds by data swap (0 build bytes; 4 x ~0.18 reels) + red + close (verbs37 x6 0.22) ~1.1 + one side's peak 0.8 | **20 GB** |
| A-lean (2 builders, same swaps) | as A, minus one worktree and some reels | **18 GB** |
| **C: sim only** | no guard floor (sim steps are not "heavy", guard.cjs:239-241); NIGHT65's rate: 2.1 GB of worktrees up front + ~0.47 GB/h | **>= 10 GB** to end the 8 h with ~4 GB; at 8.3 it ends near 2.7 GB (pagefile and compile scratch share C:, L-119, L-128) |

If GG frees the 21 builds, C: reaches about 16.3 GB: A-lean, not A. With NIGHT65's worktrees freed too it reaches about 18.4 GB.

## 3. Shipping the level row (LV)

**What n65-freeze-rows.cjs binds** (S2, still open): the slice must have passes true, state pass, a verdict whose first line is "<id>: PASS"
(:47-50) and a passSha / passFiles (:55). Then it runs `git diff --name-only <passSha> HEAD -- passFiles`, minus the two configs (:58-60),
and compares the row's config block at passSha (:61-62). For foundry65 the block never changes, so the level files and slots65.json are
unguarded unless they are in passFiles, and LV's passFiles is null today. And :59 compares commits only: the latest assembly left in the
*working tree* (S1) passes. n65-slot-set.cjs:52 gets this right.

**The fix in n66-freeze-rows.cjs:** (1) when the HEAD row has `levelTo`, append `Design/levels/<levelTo>.json`, its StreamingAssets copy
and `Design/levels/n65/slots65.json` to the compared files; (2) diff those against the working tree (`git diff --name-only <sha> -- ...`,
no HEAD); (3) require the two level copies byte-identical, and the level's `generatedBy` to name `slots Design/levels/n65/slots65.json`
(gen-n65-foundry.cjs:310); (4) add a planted case to `--selftest` (:25-42).

**From a room PASS to the row ON:**
1. **The room's GATE on a Player build.** Set the slice: `node Tools/night/n66-slate-set.cjs <id> state=pass passes=true passSha=<judged sha>
   passFiles=[...] verdict=... spot=...`. A player-facing slice needs a SPOT file whose SPOT line names a change (n65-slate.cjs:28-31).
2. `node Tools/night/n65-slot-set.cjs <id> new --slate=Logs/n66/slate.json`. It refuses a room file changed since passSha (:52).
3. `node Tools/night/gen-n65-foundry.cjs --run` (slots65.json by default, :493). Check "levelgen --walk: 0 ERROR" **and** exit 0 (until
   S1's order fix the files are already written); `--check` must still print all PASS; `git status --short Design/levels
   Assets/StreamingAssets/Design/levels Reports` should list only the level files.
4. **One commit, explicit paths:** slots65.json; both n65-foundry.json copies; Design/levels/n65/{foundry.plan.json, foundry.json,
   foundry.assembly.json, openings65.json}; Reports/LEVEL-n65-foundry-validate.md; the slate (`git add -f`).
5. **LV's own GATE** (NIGHT65.md:53-56: the row flips only by LV, never by a room). LV's passFiles = the level files + slots65.json.
   - `bash Tools/night/n66-gates.sh --flow` GREEN. The flow gate is vacuous for this row, so also save `n46flow --level n65-foundry
     --seeds 10`, with seeds 11-20, against the red's band (LV doneWhen 2).
   - Build `bash Tools/night/n41-build.sh n66-lv` from that commit, then `node Tools/night/n46-build-rows.cjs n66-lv on n65.foundry65`.
   - Reels: `bash Tools/night/n66-reels.sh n66-lv vn,n65-foundry,verbs37 tour,n65-foundry,tour65 vo,n52-foundry,verbs37,-skygrindOff65`:
     verbs37 27 / 27 and NRE 0 per side (n65-reels.sh:25); then the look49 / Look52 pair (doneWhen 3) and the LV critic's "LV: PASS".
   - With -skygrindMap the file is picked by id (L-062), so the OFF side proves only that n52-foundry still boots; the order swap itself is
     pinned by N65SharedTests. Which Player reel prints the campaign's level-1 id: not determined.
6. `node Tools/night/n66-freeze-rows.cjs n65.foundry65="<measured reason, 20+ chars>"` (both copies, :95), gates GREEN, commit. The OFF
   switch stays **-skygrindOff65**: `--off66` / `-skygrindOff66` do not cover a night-65 row (NightRows.cs:12, :158).
7. **rc1** from the flip commit (the repo config is ON, so no build-rows): `n66-reels.sh n66-rc1 vf,n65-foundry,verbs37 vi,n52-iceline,verbs37
   vk,n57-kiln,verbs37 vs,n52-span,verbs37 vp,n52-spire,verbs37` must read 27 / 29 / 31 / 31 / 31, then `bash Tools/night/n56-smoke-steal.sh
   n66-rc1` DONE (LV doneWhen 4); else revert the flip commit. The freeze is rc1's code, tagged n66-freeze.

## 4. What to generate for NIGHT66 (Tools/night/forge66-make-n66.cjs, modelled on forge65-make-n65.cjs)

**Keep forge65's rules:** never overwrite an existing file (forge65-make-n65.cjs:39), chmod the .sh files (:43), print each file's leftover
"65" lines (:44-45).

**Change the rename.** forge65's `/n64-/g` (:17) would rename the level's own names inside the round scripts (gen-n65-foundry, n65-foundry,
n65-slot-set, n65-iso, n65-verbs, n65-air-rooms, n65-table, n65-gate-pack, n65-blind, Design/levels/n65). Rename by an allow-list instead:
- `\bn65-(clock|freeze-rows|gates|prune|reels|slate-set|slate-add|slate|spot-pair|round-verbs|round|red|play-build|refresh)\b` -> `n66-$1`;
- `Logs/n65` -> `Logs/n66`; `n65-*` -> `n66-*` (n65-prune.sh:9's tag rule); `NIGHT65` -> `NIGHT66`; `flow-off65` / `--off65` ->
  `flow-off66` / `--off66`.

Keep `-skygrindOff65`, `n65verb`, `tour65`, `slots65`, `foundry65`, `verb65` and `bot43`. The expected leftover "65" lines are exactly those.

**FILES** (with the special edits):

| file | what to change |
|---|---|
| n65-clock.cjs | **an 8-hour box:** DEF (:13) -> `night: 'n66', freezeMin: 435, capMin: 480, staleMin: 720, closeTags: ['n66-freeze']`; texts "h:mm / 8:00", "BUILD (0:00-7:15) ... FREEZE (7:15-8:00)", "CAP (8:00 on)" (:2-8). staleMin 600 leaves only 2 h of usage stop past an 8 h cap (the guard ignores a stale clock, guard.cjs:91). |
| n65-gates.sh | fingerprints (:32) -> `... "--off64" "--off65" "--off66"`; flow (:24-27) -> --off66 / flow-off66; levels (:14): add n65-foundry in the commit that flips LV, re-basing the flow as :11 says |
| n65-freeze-rows.cjs | LABEL -> NIGHT66 (:11); the S2 fix (section 3) |
| n65-prune.sh, n65-reels.sh, n65-slate.cjs, n65-slate-set.cjs, n65-slate-add.cjs, n65-spot-pair.cjs | the rename only (Logs/n66 defaults: n65-slate.cjs:11, n65-slate-set.cjs:8, n65-slate-add.cjs:9) |
| n65-round.sh, n65-round-verbs.sh, n65-red.sh | the rename, plus the S1 change (`--out`, never the shipped files) and verbs taken from verb65 |
| n65-play-build.sh | the rename, plus `--update` |
| n65-refresh.cs | the list-file version (section 1) |

**Reused as they are** (after the named fixes): gen-n65-foundry.cjs (S1), n65-slot-set.cjs (`--slate=`), n65-iso.cjs, n65-blind.py,
n65-air.cjs, n65-air-rooms.sh, n65-verbs.sh, n65-save.cjs, n65-flow-cmp.cjs, n65-iso-proof.cjs, n65-table.cjs (verb columns),
n65-gate-pack.cjs (options), n65-d9.cjs (`--since`), the n65verb harness (the skip pairing), tour65 (S3).

**Order on the night:** (1) the code fixes (S3, the skip pairing, S4's comments), gates, then **one code build n66-b0**; (2) every round as a
data swap, `n66-play-build.sh n66-b0 n66-r<k> Logs/n66/<round>/n65-foundry.json`: about 0 bytes and no S1 flip, because the round's level
never touches the tracked files; (3) re-take the red with the fixed skip rule (rc0 = n64-freeze by a hard-link copy named n66-red, so the
pruner accepts the tag).

## 5. Risks in the code a night could hit

1. **The round's level flip:** n65-round.sh:19 -> :32, round-verbs `--latest` :10 -> :22, `--run` writing before its error exit
   (gen-n65-foundry.cjs:511-513), freeze-rows blind to the working tree (:59). A build started while a round runs ships whatever
   StreamingAssets holds: never run a round beside a build. Never `git add Design/levels/` after a round without `git status` (the
   assembly's timestamp, :263, is always dirty).
2. **The Editor's stale import after a pick that adds a .cs** (memory "pick-new-file-stale-editor"; NIGHT65-LOG.md:93-94): the compile gate
   says up_to_date and the Player build fails on script layout. Import, refresh and request a compile (n65-refresh.cs:4-6), then gate.
   Builders never open the Editor, so a new file's `.meta` comes from the lead's import (Tools/night/mkmeta.py exists) and is committed with it.
3. **Library growth:** 5.9 GB now (Bee 2.6 GB); per-build growth not measured. Library is a protected folder (guard.cjs:21-22). Read `df`
   before each build; the clock prints it every turn (n65-clock.cjs:61).
4. **The TMP fallback asset:** Editor Play mode rewrites `Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset`;
   restore it by that exact path before a commit (the guard allows it: guard.cjs:331's case). The four `Assets/Resources/Fonts/*SDF.asset`
   files are GG's and blocked (guard.cjs:14-19). A build also rewrites the URP Mobile asset (memory); n41-build.sh:18 prints the drift.
5. **Hard links and junctions:** n46-build-rows.cjs writes in place (:17). On a hard-link copy, or on NIGHT65.md:40's junction, it writes
   n64-freeze's config unless the file was unlinked first; n65-play-build.sh:27 unlinks, a hand edit does not. Deleting n64-freeze frees no
   shared bytes while a copy of it exists.
6. **The floors disagree:** the guard 12 GB (guard.cjs:27), the build script 10 (n41-build.sh:9), a reel side 10 (n65-reels.sh:19; a SKIPPED
   side reads RED). The guard's refusal text still names n62-prune.sh (guard.cjs:242).
7. **The clock's FREEZE:** only heavy commands whose line contains a closeTag pass (guard.cjs:250-252). Name every close build, reel set and
   smoke n66-freeze, and run rc builds before 7:15.
8. **Pilots that measure the wrong thing:** the skip test (section 1); the ride pilot on a3 only (n65-red.sh:30), where NIGHT65's red "GREEN"
   rode a2's wall (NIGHT65-LOG.md:61-62); tour65's GREEN (S3). A meter must fail on what it was built to see (L-136).
9. **The usual:** MapChoiceTests run synchronously wedges the Editor (guard.cjs:198-213: async only); commit free is 19 GB now (the rule is
   >= 15 GB before a heavy job; watch other sessions' bursts, L-146); two dotnet runs in one tree at once break the harness DLL
   (n65-red.sh:5-6).

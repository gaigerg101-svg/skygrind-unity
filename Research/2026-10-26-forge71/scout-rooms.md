# Scout: the room pipeline, the guards, the play build, the unplayed rooms (FORGE71, read-only Explore agent, 2026-10-26 ~10:00)

Disk: 70 GB free (the 12 GB guard no longer blocks). **There is no room select:** a person picks a level; inside a level the rooms run
in order, and DOORFREE (hold RB 3 s / F8: every door opens, waves spawn on entry, the run is UNRANKED; DoorFree29.cs:1-8) is the only skip.

## 1. Authoring and the guards (commands exactly as NIGHT69 / NIGHT70 ran them)
- **Authoring:**
  - The level table is Tools/night/levels67.json (iceline g1-g8; kiln kb2 kb2c kb3 kb3c kb3d; spire v2 v5 v6; foundry a2-a5 b1 b2 a8).
  - A builder edits only `Design/levels/n67/<L>/rooms/<id>.json` (level 1: `Design/levels/n65/rooms/`). `rooms-base/` holds the shipped
    rooms (never edited).
  - `slots67.json` / `slots65.json` are written only by n65-slot-set.cjs; every slot is old today.
  - `gen-slots.cjs --out=<dir>` writes nothing tracked; `--room-file=<id>:<path>` takes one room file from anywhere (gen-slots.cjs:672-680).
  - Level 1 has its own tool: `node Tools/night/gen-n65-foundry.cjs --run --slots=latest --out=<dir>`.

| # | guard | command | runtime |
|---|---|---|---|
| G0 | harness build | `dotnet build Tools/SimHarness -c Release -nologo -v q` | 12-13 s |
| G1 | levelgen walk (reachable, walkers reach every tier, pits recoverable) | `node Tools/night/gen-slots.cjs --level <L> --slots=<id> --run --out=<dir>` | 6-21 s |
| G2 | skip v2 (M2 printed; makes the touch table G4 / G5 read) | `bash Tools/night/n65-verbs.sh skip <dir>/n67-<L>.json <id> --seeds 20 --bars C:/Users/gaige/dev/SKYGRIND-unity/Logs/n70/slate.json` | ~3.2 min |
| G3 | room check + walk A / B / V + pads (+ ride) | `LEVEL_DIR=<dir> ROOMS=<id> bash Tools/night/n70-red.sh 2 n67-<L> <ev>` | 76-120 s |
| G4 | area lint | `node Tools/night/n70-area.cjs --level <L> --dir <dir> --touches <skip.txt> <id>` | seconds |
| G5 | decoration | `L67=<L> node Tools/night/n70-deco.cjs <id> --touches <skip.txt> --file <room file>` | seconds |
| G6 | patch share | `node Tools/night/n70-patch-share.cjs --level <L> <id>` (`--room <file>` for a branch file) | seconds |
| G7 | throw chart (launch rooms; info) | `bash Tools/night/n70-throws.sh <dir>/n67-<L>.json <id> <ev>/throws.txt 20` | 43-75 s |

- **G3's inside** (n70-red.sh:51, :58-61):
  - `python -I Tools/night/n59-l-room.py n67-<L> room <id> --dir <dir>`
  - `SimHarness n46flow --level n67-<L> --rooms <id> --seeds 10 --first 1|11 [--no-geo] --file <dir>/n67-<L>.json`
  - `--verbs65`, `hold52 --pads`, `n53ridekill ride` in ride rooms.
- **Slate trap:** for a room not in Logs/n70/slate.json, n70-red stops with MISMATCH (n70-verbs-of.cjs:7). Pass `SLATE=Logs/n68/slate.json`
  (g7 g8 kb2 kb3 kb3c kb3d v2) or `SLATE=Logs/n67/slate.json` (v6).
- **Main tree:**
  - A room chain on main is 7.4-10.5 min per room.
  - The gates `bash Tools/night/n71-gates.sh --no-editor --flow` take 8-10 min; `--flow-base` takes ~4 min.
  - Levelgen on spire always prints ONE ERROR, the base room v11's pit soft-lock. "DID NOT reach the last exit" in the generator's quick
    walk proves nothing; n46flow is the walk guard.

## 2. The play build and booting a level
- **The build:** `bash Tools/night/n65-play-build.sh <src> <dst> <lvl.json>[,...] [--config <file>] [--add-level] [--update] <night.row>...`
  makes hard links and turns rows ON with n46-build-rows.cjs. It writes ~40 MB and takes seconds.
- **Builds/n66-freeze:** no n67 rows, no n67 levels, and it lacks the iceline grade (UnityTuning.N53.cs:29). Build a fresh n71-freeze:
  `bash Tools/night/n41-build.sh n71-freeze` (the Editor open and stopped, >= 10 GB free; ~2 min on NIGHT62).
- **Booting a campaign level needs reel.sh** (`-skygrindMap` alone boots SURVIVAL, L-062):
  `bash Tools/night/n71-reels.sh <tag> vk,n67-kiln,verbs37`.
  - verbs37 runs 50-55 s a level, but does not walk the rooms. Run it on the play build (rows ON); with a row OFF it reads RED on rows 17-23.
  - tour65 enters every fight room with wave 1 live and frames each pose (FeelReel.N65.Tour.cs:4-19). It has never run in a Player
    (runtime unknown; 420 s cap).
  - The smoke `Builds/<b>/SKYGRIND.exe -skygrindSmoke -logFile <f>` takes 233 s and loads no rebuilt level.
- **A person's path:** TITLE > PLAY (CAMPAIGN) > the level select; all five slots start (`sequential` false). The rows swap the slots:
  1 n65-foundry, 2 n67-iceline, 3 n67-kiln, 5 n67-spire. `-skygrindOff67` boots the shipped levels for a side-by-side.

## 3. The unplayed rooms and their last guard results
| room (level) | file | room check | area | deco | walk (red) | guard FAIL? |
|---|---|---|---|---|---|---|
| level 1: a2 a3 a4 a5 b1 a8 | Design/levels/n65/rooms/ (current files never measured; a3 a4 a5 a8 FORGE67 carry-overs) | Logs/n66/r2/table.md on older files: a4 IN VIEW held | never | 6/6 (old) | a5 deaths 1/4 (red 0/2) | measure first |
| g7 THE MIX (2) | main 70dfbd5ee | R5 FAIL (red too) | PASS | PASS | stall A 54.0 % (red 29.3) | walk stall |
| g8 THE STAMPEDE (2) | main 38a92116b (r1f) | R2 + R5 (red too); I2 1.17 < 1.3 | PASS | PASS | 1/0 (1/3); **air65 1 hand-off vs the shipped 3** | **a verb hand-off lost** |
| kb2 THE CHARGING FLOOR (3) | main aa3433260 | PASS | FAIL 5/6 (P11) | PASS | 0/0 | area (label) |
| kb3 d2 (3) | n70-R bfe1529f (blob 389856ba) | PASS | PASS 4/4 | FAIL (door kerb 4/20, need 6) | 10/10, deaths 0; stall 24.5 vs ceiling 22.7 | deco / stall (labels) |
| kb3c THE TAP RUN (3) | main ee7efd023 | PASS | PASS 6/6 | PASS | 0/1 (0/2), ride GREEN | none |
| kb3d THE CRACKED FLOOR (3) | main aa3433260 | **R2 dead end (red PASS)** | FAIL 3/4 | PASS | 0/0 | **reachability** |
| v2 s1b (5) | main deda5825f | R5 (red too) | PASS | FAIL 2 | 0/0 | deco (label) |
| v2 t16 (5) | n70-S dfa1ed865 (blob 6bbcce61) | R5 + I2 (same as red) | FAIL 3/4 (open square 12.5 m vs 12) | PASS 7/7 | 10/10, deaths 0, stall 17-19 % | area (label) |
| v6 THE CHOIR (5) | main f8ea361cc | R5 (red too) | FAIL 4/5 (P9 enemy climb tops) | PASS | 0/0 | area (label) |

The kb3 d2 / v2 t16 level files in the builder trees are stale: regenerate from the room files.

## 4. GG's words and the rules that encode them
| GG's words | rules |
|---|---|
| "smaller and more action packed like ... Ultra Kilikow" | area OPEN <= 12 m (n70-area.cjs:13-15); patch share <= 25 % kept |
| "random pillars that serve no purpose"; "cluttered" | deco (n70-deco.cjs:1-10); area P4; room check R8; DESIGN-RULES.md:87-88 |
| "random lava pits" | no lava or pit on the walk; a hazard only on the throw line; LevelValidate24.cs:12-13 |
| "a route that enemies can climb up" | LevelValidate24.cs:8-11 rows 4-5 (code :219-258); area P9; PRINCIPLES.md:28 |
| "incentivize wall-riding" | room check I2 / I4 / I1 / R5 / IN VIEW; area P5 / P17 |

## 5. The scout's first-play-build plan (60 min): the base for slate item P0 (NIGHT71.md uses it with the forge's spike corrections)
1. Bearings: disk, `bash Tools/unity-loop.sh status`, git.
2. `bash Tools/night/n41-build.sh n71-freeze`.
3. The level files under Logs/n71/play/:
   - `git show n70-R:Design/levels/n67/kiln/rooms/kb3.json > Logs/n71/play/kb3-d2.json` and the same from dfa1ed865 for v2;
   - `gen-n65-foundry --slots=latest`;
   - `gen-slots` for iceline (g7 g8), kiln (kb2 kb3c + `--room-file=kb3:...`) and spire (v6 + `--room-file=v2:...`).
4. The guards: n70-red.sh per level with SLATE= for each slot's slate, plus the area lint per room.
5. `n65-play-build.sh n71-freeze n71-play <four level files> --config Design/unity-config.json --add-level n65.foundry65 n67.iceline67
   n67.kiln67 n67.spire67`.
6. In the Player: `n71-reels.sh n71-play vn,n65-foundry,verbs37 vi,n67-iceline,verbs37 vk,n67-kiln,verbs37 vp,n67-spire,verbs37`, the
   smoke on n71-play, and tour65 on kiln and spire (first run ever).
7. The card. 8. Commit with `git add -f`; never commit a generated level file.

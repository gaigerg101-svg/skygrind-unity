# FORGE67b code scout: rebuilding and judging rooms in several levels in one night

Written 2026-10-24 ~08:20 at eaa4f5f4 (port/stage3), read only (no dotnet, build, Editor or Player run). Grounding: the repo only
(Google Drive, Gmail and Linear were not searched: this game's material lives in the repo, as FORGE67's DECISIONS.md:6 says). GG's
focus for the night (2026-10-24, verbatim): "area deisgn need t be better in alot of levels". Every number below was read or measured
with a read-only command; anything else says "not checked".

## 1. The levels and their pipelines

The campaign's five levels, as the close's reels name them (NIGHT67.md:164-165), and the level rows from `design.*.levelFrom/levelTo`
in Design/unity-config.json (read with a node one-liner):

| level | shipped id (row) | plan read by the next generator | generator of the shipped file | fight rooms (kind room) | boss | levelgen ERROR rows of the shipped file |
|---|---|---|---|---|---|---|
| 1 THE FOUNDRY | n52-foundry (n52.foundry52 ON; n65.foundry65 OFF -> n65-foundry) | Design/levels/n52/foundry.plan.json | gen-n52-foundry.cjs; the slot pipeline gen-n65-foundry.cjs | a2 a3 a4 a5 b1 a8 (+ link b2 as a slot) | a9 wick | 0 (Reports/LEVEL-n52-foundry-validate.md) |
| 2 THE ICE LINE | n52-iceline (n52.iceline52 ON) | Design/levels/n52/iceline.plan.json | levelgen only (its generatedBy); the plan is patched by night scripts (e.g. Tools/night/n56-l-doors3.py, commit 9aedb84d) | g1 g2 g3 g4 g5 g6 g7 g8 (8; g6 the armoury, 0 boxes) | x9 brawl40 | 0 (report untracked) |
| 3 THE KILN | n57-kiln (n57.kiln57 ON) | Design/levels/n57/kiln.plan.json | gen-n57-kiln.cjs (its rooms built in code, from n52/kiln.plan.json, itself gen-n52-kiln.cjs's) | kb2 kb2c kb3 kb3c kb3d (5) | k5 wyrm | 0 (LEVEL-n57-kiln-validate.md:5) |
| 4 THE SPAN | n52-span (n52.span52 ON; n58.span58 OFF -> n58-span, gen-n58-span.cjs, `NEW = []` at :35) | Design/levels/n52/span.plan.json (written by gen-n52-span.cjs) | levelgen only (its generatedBy) | p1 p3 p5 (3) | o3 loom37 | **22** (LEVEL-n52-span-validate.md:5: pit-recovery soft-locks in p4 / p5, the route) |
| 5 THE SPIRE | n52-spire (n52.spire52 ON) | Design/levels/n52/spire.plan.json | levelgen only; patched like iceline | v2 v5 v6 (3) | v11 crown37 | **1** (v11 pit recovery; report untracked) |

26 rebuildable rooms in all (7 foundry slots, 8 + 5 + 3 + 3). Room ids are unique across the campaign (a*/b*, g*/gr/x9, kb*/k5,
p*/o*, v*), so one slate keyed by room id still works for several levels.

Per-level facts a generic tool must carry (read with Research-only scratch scripts over the five plans):
- **Lists.** Every plan has zone-keyed boxes / ramps / pads / pits (foundry also tunnels, grapple) and room-keyed lights / props.
  Iceline, span and spire also carry a room-keyed `surfaces` list (iceline g4's ice floor). Foundry alone has `teach35`. gen-n65's LISTS
  (gen-n65-foundry.cjs:59-61) lack `surfaces`: a rebuilt g4 would keep a stale ice rect.
- **Row keys.** gen-n65's KNOWN list (gen-n65-foundry.cjs:73-85) accepts every fight-room row key of the other four plans; only kiln's boss
  row has an unlisted key (`wyrmArena`, k5: not a slot). Spire's top-level `gates38` names links only (v3, v4, v7): no fight slot is affected.
- **The finish.** Foundry re-applies a9's n47 / n48 keys and `data.name "n46-foundry"` (gen-n65-foundry.cjs:287-332). Kiln sets
  `data.name "n40-kiln"` and generatedBy only (gen-n57-kiln.cjs:30-37). Iceline, span and spire ship levelgen's output as it is
  (data.name = the level id; gen-n58-span.cjs:216 set its own). A generic finish keeps the base level's data.name.
- **Verbs.** owns38: foundry / iceline / spire jump double dash slide ride walljump; kiln adds boost; span adds boost and grapple. The fight
  rooms' verb36 lines: iceline the punch, the shield stun, the LT blast, the orb parry, the dodge-into-column stun; kiln the THUNDERCLAP
  boost, the burrower, the charger into a press, the ride; span the pull, the swing, the whip; spire the TORCH, the grapple pull up the
  ropes, the orb parry.

**How a level row swaps a level: one row per level.** NightRows.SwapLevels (Assets/Scripts/Core/NightRows.cs:269-278) walks every
`design.nNN.<row>` holding levelFrom / levelTo, sorted by night then name (:335), and replaces levelFrom with levelTo in the campaign
order (called last in Curriculum25.Order, Curriculum25.cs:39). Rows chain: `design.n67.kiln67 {levelFrom n57-kiln, levelTo n67-kiln}`
runs after n57.kiln57 (n52-kiln -> n57-kiln). NightRows.BaseId follows the chain back for older systems (:283-296); `-skygrindOff67`
(or older) turns the row off (:274). Several rebuilt levels = several rows, each flipped on its own.

What adding a level id needs beyond the row (the template is Tools/night/n65-a1.cjs, NIGHT65's shared commit): the row OFF in both
config copies, a `design.maps.list` entry (an unlisted id boots the arena: N65SharedTests.cs:40), the id beside its base in look49's
`levels` list when the base is there (today `["n48-foundry","n47-spire","n52-foundry","n65-foundry","n52-spire"]`: spire needs
n67-spire), placeholder level files in Design/levels and Assets/StreamingAssets/Design/levels, and pinned tests (N65SharedTests.cs:23-71
is the model). Two literal-id lists in Game code: UnityTuning.N53.Polish53IcelineLevels (UnityTuning.N53.cs:29, polish53 is ON: a
rebuilt iceline loses its grade unless the new id is added) and Look52TierLevels (UnityTuning.U52.cs:150, foundry only). Exit58 / Exit63 /
ExitFrame61 hard-code n57-kiln (Exit58.N58V.cs:22, Exit63.N63V.cs:33, ExitFrame61.N61V.cs:32) but their rows (lamp60, exit63,
exitFrame61) are OFF: nothing shipped is lost there.

The walk bot on rebuilt rooms of other levels: the bot43 opt-out (Walk36.cs:143-150, BotRows65 :168-183) covers only level 1's id scripts
(Walk36, Walk38Foundry, Walk43Foundry). The other levels' scripts key on level data or on old ids that are not fight rooms: kiln's gates
are boxes with a `gate` object (Walk38Kiln.cs:33-45), span's on the row's `pull38.gates` (Walk38Span.cs:36-48) plus hop lists for the
links o1 / o2 (:134-138), spire's on `meter38` rows (Walk38Spire.cs:34-45) plus the links v9 / v10, iceline's Walk40Iceline only on the id
"n40-iceline" exactly (Walk40Iceline.cs:30). So a rebuilt fight room on n67-kiln / -span / -spire / -iceline needs no bot code change.

## 2. Every hard-coded level-1 assumption (file:line)

`git grep -c "n65-foundry|foundry65|Design/levels/n65|slots65"` over Tools and Assets: gen-n65-foundry.cjs 41, n65-a1.cjs 22,
N65SharedTests.cs 18, n67-gate-pack.cjs 16, n65-slot-set.cjs 14, n67-round.sh 11, n67-close-check.cjs 8, n67-round-verbs.sh 5,
n67-red.sh 5, n67-deco.cjs 4, n67-table.cjs 2, n65-iso.cjs 2 (comments), n59-l-room.py 2 (comments), n67-freeze-rows.cjs 1 (selftest).

**The assembler, Tools/night/gen-n65-foundry.cjs**
- :42-48 PLAN_IN n52/foundry.plan.json, N52 n52-foundry.json, DIR Design/levels/n65, rooms/ and rooms-n52/, slots65.json, the outputs
  foundry.plan.json / foundry.json / foundry.assembly.json and OUTS n65-foundry.json (both copies); :52 REPORT LEVEL-n65-foundry-validate.md;
  :54 ID n65-foundry; :57 SLOTS a2 a3 a4 a5 b1 b2 a8; :58 KEPT a1 a6 a7 a9; :59-61 LISTS (no `surfaces`); :65 openings65.json.
- :102 the export's _why text; :233-240 the guards (generic, given SLOTS); :242 the plan's _why "NIGHT65 (row foundry65 ...)".
- :266-275 openingsOf writes the key `foundry` only, and only new slots' openings: n59-l-room.py merges openings53.json with the prefix's
  file only (n59-l-room.py:49-62), so an n67-kiln built this way would read n52's openings for n57's rooms (kb2c n52 "ambushed" against
  n57's "vista", openings57.json) and none for kb3c / kb3d. A generic tool must seed its openings file with the base level's openings.
- :287-332 finishLevel: a9's keys, data.name n46-foundry, KEPT, the lists checked (:324: no `surfaces`).
- :343-359 finish: refuses ANY `| ERROR |` row (:348-349); :570-576 `--run` refuses any levelgen ERROR. Span (22) and spire (1) can never
  pass either: the rule must become "no ERROR row beyond the base level's own" (by its check name).
- :365 shipShas: the two n65-foundry paths.
- :403-505 --check: the planted cases name a2 (:445, :466), a5 (:457, :474-492), a8 (:458), b1 (:459), a3 (:460), a4 (:461, its rect
  [-5,-108,5,-89]); :551-553 --plant-error lifts a2's spawn.

**The round, Tools/night/n67-round.sh**: :21 ROOMS from Design/levels/n65/slots65.json; :25 and :33 gen-n65-foundry.cjs; :36
LEVEL-n65-foundry-validate.md; :37 `n67-red.sh 6 n65-foundry`; :41 `$out/foundry.plan.json` for iso; :44 `$out/n65-foundry.json`;
:45 n65-air-rooms.sh called without rooms (its default a2,a3,a4,a5,b1,a8, n65-air-rooms.sh:5); :47 Logs/n67/red65.

**Tools/night/n67-red.sh**: :12 LEVEL default n65-foundry; :13 the plan path is generic (`n52-*` -> Design/levels/n52/<x>.plan.json, else
Design/levels/<px>/<x>.plan.json); :15 ROOMS from slots65.json; :38 the ride pilot only in a3 and b2.

**Tools/night/n67-round-verbs.sh**: :12 ROOMS from slots65.json; :15 the level default n65-foundry.json; :16 foundry.assembly.json; :17
gen-n65-foundry; :22-26 the verb per room is fixed (pierce a2,a8; launch a5,b1; launch --slide a5; parry a4; ride --shoot a3); :28 the
skip with Logs/n67/slate.json. **Tools/night/n65-verbs.sh**: :6 default rooms; :7 a `dotnet build` on every call (so it cannot be run in
parallel in one tree); :9-18 the rooms run one after another.

**Tools/night/n67-table.cjs**: :11 red default Logs/n67/red; :18 foundry.assembly.json; :71 the ride column only for a3 / b2.

**Tools/night/n67-gate-pack.cjs**: :36 LEVEL default Design/levels/n65-foundry.json; :39 RED_PLAN Design/levels/n52/foundry.plan.json; :57
the round's saved files named n65-foundry.json / foundry.plan.json; :69-75 the scratch assembly (gen-n65-foundry.cjs, n52's plan,
slots65.json, rooms/ and rooms-n52/); :115 the red folders `red` / `red65`; :126-129 the scratch levelgen of n65 paths; :139 the slate
Logs/n67/slate.json; :180 the key's text "the red (n52-foundry)"; :186 the plan path Research/2026-10-23-wave66/plans/<id>.md; :194-198
rooms-n52/ against rooms/. Options already exist for --logs, --plan, --level, --out, --key, --slate (:36-39, :138).

**Tools/night/n67-deco.cjs**: :139, :167, :268 Design/levels/n65/rooms/<id>.json (the `--file` option covers the current file at :167, not
the base path read at that sha).

**Tools/night/n65-slot-set.cjs**: :19 SLOTS; :23 roomPath n65/rooms; :28 the file's _why; :61 slots65.json; :62 the default slate
Logs/n65/slate.json (NIGHT67 passes --slate); :110-115 its self-test runs gen-n65-foundry.cjs.

**Tools/night/n67-close-check.cjs**: :54 one `--level-row` (n65.foundry65) and one `--slots` file; :100 FROZEN holds that one slots file;
:119-120 the Player sides: vn only when that one row is ON; :136-142 one `ROW: ... SLOTS: ...` line.

**Tools/night/n67-freeze-rows.cjs**: already takes several `<night.row>=<reason>` pairs (:6, :57-76, :81-88); levelFilesMissing
(:21-28) is generic but accepts any `Design/levels/<x>/slotsNN.json` (:26), not the row's own; :47 a selftest names n65-foundry.

**Tools/night/n67-gates.sh**: :16 the flow levels are the five shipped ids; :34 the fingerprint offs --off57..--off67. A new row OFF changes
neither: n46flow loads a level by id, not through the campaign order (so this gate never sees a rebuilt level's rooms either).

**Tools/night/n65-play-build.sh**: :18 refuses an existing destination and :21-29 replaces one level file: a play build with three new
levels needs a loop over level files (or three calls into three folders).

**Already level-agnostic** (no change needed): the verb pilot (Harness.N65.VerbPilot.cs:105-136 reads `verb65`, ride faces and tunnels
from any level file); skip v2 (Harness.N65.Verb.cs:822-941, `--level <file> --room <id>`; :94-95 / :824-825 are only defaults); n46flow
`--level <id> --rooms --file --verbs65 --plant-verb65` (Harness.N46.Flow.cs:102-111, :167); n65tour (Harness.N65.Tour.cs:37); n59-l-room.py
for any `<px>-<level>` with `--dir` (:35-42, :189-193; the n52 checkers cover all five levels: n53-identity.py:437); n65-iso.cjs (:38-52:
a level file is drawn from `<level>.plan.json` beside it); n65-air.cjs; hold52; n53ridekill; FeelReel.N65.Tour.cs (any -skygrindMap);
n67-reels.sh.

## 3. The generalisation plan (files in order, sizes, proofs, cuts)

The design: **one slot assembler for any level, driven by a table**, level 1 left on its proven tool. New ids n67-kiln, n67-span,
n67-spire, n67-iceline; level 1 keeps n65-foundry and foundry65. Layout per level `<L>` (kiln, span, spire, iceline):
`Design/levels/n67/<L>.plan.json`, `n67/<L>.json`, `n67/<L>.assembly.json`, `n67/openings67.json` (one key per level: n59-l-room.py's
naming, :35-42), `n67/<L>/slots67.json` (matches freeze-rows' `slots\d+\.json` at :26), `n67/<L>/rooms/<id>.json`,
`n67/<L>/rooms-base/<id>.json`; the level `Design/levels/n67-<L>.json` and its StreamingAssets copy; the round under `Logs/n67/<L>/<tag>/`
with the reds `Logs/n67/<L>/red/` (so the GATE pack's `--logs` and its `r*` / review scan work per level unchanged).

| # | file | change | size | proof it works | cut? |
|---|---|---|---|---|---|
| 1 | Tools/night/levels67.json (new) | per level: base level id and file, PLAN_IN, the new id, the px dir, SLOTS (fight rooms), KEPT (computed: every other room), zone / room lists (+ surfaces), finish kind (foundry-a9 / data-name), the planned verb per slot (for the reds' `--plant-verb65`), the row key | ~80 lines | read by #2 and #5-#9; its `foundry` entry points at n65's paths | no |
| 2 | Tools/night/gen-slots.cjs (new; gen-n65-foundry.cjs parameterised) | `--level <L>`; export / plan / --run / --out / --finish / --check / --room-file / --dry-plan as gen-n65 has; ERROR rule baseline-relative (the base plan's levelgen report, made by --check, saved as `n67/<L>/base-errors.txt`); openings seeded from the base level's (openings57.json for kiln); finish keeps the base's data.name; planted cases on the level's first two slots with boxes | ~650 lines (583 today) | **`node Tools/night/gen-slots.cjs --level kiln --check`**: every slot old -> levelgen --walk + finish -> the bytes ARE n57-kiln.json apart from name / _why / generatedBy; the same for span (22 ERROR rows, none new), spire (1), iceline. Planted: a box moved 1 m prints the diff; an unknown key, a drop with no reason, a piece keyed to another room, a rect change, a new piece with no job: each refused; one slot rebuilt changes only its own rows through levelgen; a spawn lifted 40 m adds an ERROR row and is refused while span's 22 base rows pass. Cross-check: `--level foundry --slots=old --out=<a>` and `gen-n65-foundry.cjs --slots=old --out=<b>` write byte-identical levels. ~3 levelgen runs per level (~12 s each: NIGHT66 r2 "assembled ... (12 s)") | per-level copies of gen-n65 (~20 min each) are faster for ONE extra level; from two levels on, the table wins |
| 3 | Tools/night/n67-a1-levels.cjs (new; n65-a1.cjs's recipe) | the lead's shared commit: `design.n67` with kiln67 / span67 / spire67 / iceline67 (levelFrom = the shipped id, enabled false), the maps.list rows, n67-spire in look49's levels, the placeholder level files (base bytes, names changed); asserts nothing else changed, copies identical; then `gen-slots --export` per level | ~150 lines | the run's own assert; `node -p` the rows; the gates (fingerprint --off67 EQUAL, flow --off67 EQUAL: the rows are OFF) | only the levels the night takes |
| 3b | Assets/Tests/EditMode/N67SharedTests.cs (+ UnityTuning.N53.cs:29 when iceline is taken) | N65SharedTests per row: a row, off, listed, files exist, order unchanged with the rows off, BaseId equal | ~90 lines + 1 | the Editor run (the lead only; CLAUDE.md workflow) | cut to a node check of the same facts when the Editor is not free; the Polish53 line is not cuttable if iceline ships (nothing lost) |
| 4 | Tools/night/n67-round.sh | `LEVEL=<L>` (default foundry: today's behaviour): gen-slots or gen-n65 by level, ROOMS from the level's slots file, the report / plan / level names from the table, n65-air-rooms.sh given $ROOMS, the tables against `Logs/n67/<L>/red` | ~25 lines changed | `LEVEL=kiln bash Tools/night/n67-round.sh red --shipped`: the red of n57-kiln's five rooms; tracked-status.txt equal before and after | no |
| 5 | Tools/night/n67-red.sh | the ride pilot in every room whose verb (row verb65 or the table's) is ride, not a3 / b2 | ~5 lines | the red's ride.txt files | no |
| 6 | Tools/night/n67-round-verbs.sh | each room's pilot by its verb65 (read from the round's level file), the reds with `--plant-verb65 <id>:<table verb>`; **the skip in a job pool of the built DLL** (n67-red.sh:40-45's loop, `timeout $N65_VERBS_TIMEOUT` each; never n65-verbs.sh in parallel: its :7 builds) | ~40 lines | the sim is deterministic: one room's pooled skip output equals its serial n65-verbs.sh output line for line, wall-time lines aside; a planted hang at N65_VERBS_TIMEOUT=60 ends with its TIMEOUT line | the pool is cuttable (rounds then serial, see section 5's budget) |
| 7 | Tools/night/n67-table.cjs | the assembly name `*.assembly.json`; the ride column wherever ride.txt exists | ~8 lines | a table of the kiln red | no |
| 8 | Tools/night/n67-deco.cjs | `--rooms-dir <dir>` for the current and the base path (:139, :167, :268) | ~6 lines | its self-test on one kiln room (B+/B-, C+, A-, D-/D+) | no (M4) |
| 9 | Tools/night/n65-slot-set.cjs | `--level <L>`: SLOTS, roomPath and the slots file from the table | ~25 lines | its self-test (PASS sets the slot; a changed file refused; no PASS refused) on kiln | no |
| 10 | Tools/night/n67-gate-pack.cjs | `--red-plan`, `--rooms-dir`, `--plans <dir>`; the round's saved level / plan found by `*.assembly.json`; the red folder names from `--logs` | ~30 lines | a pack on the kiln red as its own "round" (`spot-A/B.png`, verbs.md, air.md written; the key outside) | can wait to the first GATE (~2:00) |
| 11 | Tools/night/n67-close-check.cjs | `--level-row` repeatable, each with its slots file; FROZEN gets every slots file; one ROW line per row; a Player side per ON row (as vn) | ~40 lines + 3 selftest cases | `--selftest` (25 cases today, n67-close-check.cjs header): + two rows one ON, a row ON without its side, a slots file committed in the close | can wait to ~4:00, must land before the close |
| 12 | Tools/night/n67-freeze-rows.cjs | the passFiles' slots file must be the row's own (`Design/levels/n67/<L>/slots67.json`) | ~5 lines + 1 case | `--selftest` (5 cases today) | cuttable (the PASS still binds the level files) |
| 13 | Tools/night/n65-play-build.sh | several `<level.json>` in one call | ~10 lines | a hard-link play build from n66-freeze with two added levels, the source's sha256s unchanged (its own check, :31-33) | cuttable (one level per build folder) |

**What a tools builder delivers in the first 60-90 minutes** (estimates, not measured): #1 + #2 with `--check` PASS on kiln and one
of span / spire (0:05-1:00); #4-#7 so a per-level round runs (1:00-1:30); the lead runs #3 meanwhile (it is the lead's config commit).
First per-level red at about 1:30, first per-level round at about 2:00. #8-#10 by the first GATE; #11-#13 by 4:00.

**Cut outright for the night: new verb pilots.** The pilot performs launch, launch+slide, pierce, ride, parry, slide only
(Harness.N65.VerbPilot.cs:6-16); skip v2 counts uses for those (Harness.N65.Verb.cs:874). Kiln's boost, span's pull / swing / whip, spire's
torch and grapple, iceline's blast and stun have no pilot: such a room's M1 reads `REFUSED: verb65 '<x>' is not a verb this pilot
performs` (VerbPilot.cs:78) or, with no verb65, `M1 UNSURE ... no verb65` (Verb.cs:859-864). A new verb needs a trigger and episode in
V65VerbPilot, a refusal in V65Filter (Verb.cs:518-545), a use count in V65ClearRun / UsesOf and planted proofs: several hundred lines and
a critic per verb, not a 90-minute job. The night's rooms should be carded on verbs the pilot knows (ride exists in every level's
owns38; parry where orbs or globs fly; launch where the gauntlet is in hand), or their M1 is "not checked".

**Also out of the slot model:** a slot cannot change its id, kind or rect, nor the doors (gen-n65-foundry.cjs:86-87, :239). GG's
"rooms a lot smaller" fits only inside the rect; a split like n58's p2 (a link + a door + a room: gen-n58-span.cjs:17-18) needs a
per-level doors override owned by the lead: not built.

## 4. What a builder can measure on a non-level-1 room on day one, with no new code

All run the built DLL (`dotnet Temp/SimHarness/bin/Release/net10.0/SimHarness.dll ...`) or node / python on the SHIPPED level file:
- skip v2 with a planted verb: `n65verb skip --level Design/levels/n57-kiln.json --room kb3c --plant-verb65 kb3c:ride --seeds 10 --bar 1.25`
  (the plant also sets bot43 false in memory: VerbPilot.cs:51-62). This is the room's red for any card whose verb the pilot knows.
- the stall guard on the verb-aware pilot: `n46flow --level n57-kiln --rooms kb3c --seeds 10 --first 1 --verbs65 --plant-verb65 kb3c:ride`;
  flow A / B the same without the plant (and `--no-geo` for B, as n67-red.sh:33-36).
- the verb pilots: `n65verb ride --level Design/levels/n57-kiln.json --room kb3c [--shoot]`, `n65verb parry ...` (needs a spitter's glob:
  VerbPilot.cs:14; whether iceline g4's or spire v6's flyer orbs count is not checked), `n65verb launch ...` (whether each level's
  loadout gives the dash punch in that room is not checked; iceline's KESTREL returns only in g6, so pierce is out in g1-g5).
- `python -I Tools/night/n59-l-room.py n57-kiln room kb3c` (and `n52-span table`, `n52-spire table`, `n52-iceline table`): the room checker
  and the identity checks; `hold52 --level n57-kiln --room kb3c --pads`; `n53ridekill ride --level Design/levels/n57-kiln.json --room kb3c`;
  `n65tour --level n57-kiln`; `n65bot --level n57-kiln --rooms kb2`.
- the pictures: `node Tools/night/n65-iso.cjs Design/levels/n57/kiln.plan.json kb3c out.png` (also n52/span, n52/spire, n52/iceline plans);
  `node Tools/night/n65-air.cjs Design/levels/n57/kiln.plan.json kb3c`.
- a hand-made trial level without the slot tool: a plan copy edited in a scratch folder, `dotnet run --project Tools/SimHarness -c Release
  -- levelgen <plan> <out> --walk --report <md>` (gen-n65-foundry.cjs:372), and the checks above with `--file <out>` / `--dir`; nothing in it is
  guarded (no KNOWN keys, no rect lock, no finish): fit for a builder's probe, never for a GATE.
Not measurable on day one: n67-deco.cjs (room files only), the GATE pack, the slot set, the close-check rows.

## 5. Risks and the time budget

**Disk.** `df -BG /c`: 14 GB free (08:10). NIGHT67's bearings (DECIDE67 D-1, NIGHT67.md:36-38): branch C, 7-18 GB, sim only. The guard
refuses every build, reel set, smoke and batch test under 12 GB (.claude/hooks/guard.cjs:27, :239-242). Sim rounds are small: Logs/n66
is 24 MB for a whole night; a round's level copy 2.5-5.7 MB (n52-span.json 5.0 MB); the harness build 15 MB. Builds/ holds 257 entries;
.claude/worktrees 213 (about 0.7 GB each: Research/2026-10-24-forge67/DECIDE67-v1.md:15; one `du` timed out at 60 s, not measured). The
guard refuses removing worktrees (guard.cjs:190, :220): freeing disk is GG's. n65-play-build.sh is not in the guard's heavy list (its name
does not match `n\d+-build`, guard.cjs:239) and writes hard links (n65-play-build.sh:3-4, :26): GG's play build of rebuilt levels is
possible in branch C.

**Memory.** At 08:13: commit 80.8 of 97.6 GB, **16.8 GB free**, against NIGHT67's hold line of 15 GB (NIGHT67.md:44). The Unity Editor
(PID 21008, 7.1 GB private) and its two AssetImportWorker6 / 7 (PIDs 49588, 51936, `-batchMode` children of the Editor since 2026-10-22
23:24, 7.0 GB private each) hold about 21 GB; the workers are the "two stray Unity processes". The guard would allow stopping a
batch-mode Unity by PID (guard.cjs:233); CLAUDE.md rule 10 forbids killing the Editor: whether to stop the workers is GG's call. One
SimHarness DLL is about 0.2 GB (PID 70464's working set): a 6-job pool adds about 1.2 GB. PID 70464 (an n46flow on n65-foundry started
08:12 with a `--file` in this forge session's scratchpad) is another FORGE67b agent's job, not this scout's.

**The usage limit.** NIGHT66 lost 1:55 (0:20-2:15) and 2:40 (4:35-7:15) to two stops (NIGHT66-LOG.md:50, :260). More levels mean more
agents and more tokens per hour: a stop is more likely, not less. Every tool slice must commit on its own; the per-level rounds must be
resumable (each writes its own folder and refuses an existing one: n67-round.sh:19).

**The guard's other rules that bite.** No new agent after the freeze (guard.cjs:301); a background `&` after `&&` refused (:298); no
`git worktree remove` (:190); killing a non-batch Unity refused (:228, :233).

**Other risks found.**
- Whether today's levelgen reproduces n57-kiln / n52-span / n52-spire / n52-iceline bit for bit is not checked: #2's `--check` is the
  first run that proves it (it was proven for foundry: `gen-n65-foundry --check: all PASS (levelgen runs: 3)`, Logs/n66/t2-check-main.txt). A difference means that level's red must be measured on the
  regenerated file, said so in the log.
- The openings overlay (section 2) gives n67-kiln's old rooms the wrong openings unless the generator seeds n57's.
- Span's 22 and spire's 1 base ERROR rows block today's refusal rule outright (gen-n65-foundry.cjs:348-349, :571-576).
- Each new `design.maps.list` row adds a case to MapChoiceTests.ListedMap_LoadsAndPlays600Steps (MapChoiceTests.cs:39-45; 82 maps listed
  now), the Game M class that already times out (the auto-memory's editor-suite note).
- The mainBar rule (max(1.25, red65 + 0.20): NIGHT67.md:14-15) has no red65 outside level 1: a forge decision is needed (the shipped room
  with its card's verb planted is the obvious red).
- The binding constraint is the review slot, not compute: one review slot (NIGHT67.md:43), a critic 17-23 min (L-109). Several levels per
  round means one combined level review per round across levels, or rounds that outrun their reviews.

**Time per round** (NIGHT66 r2, Logs/n66/r2-run.txt and verbs-skip.txt: gen 12 s; n66-red 25 jobs in 214 s at 6 parallel, about 51 s
a job; the four pilot files about 43 s; skip v2 serial at 155-193 s a room, 1060 s for six; the round 1348 s). Per room about 36 s of the
red's pooled time plus about 177 s of skip, serial. Estimated (not measured) per level:

| level (rooms) | serial skip, as today | skip in the 6-job pool (#6) |
|---|---|---|
| foundry a2 a3 a4 a5 a8 (5) | ~19 min | ~6 min |
| kiln (5) | ~19 min | ~6 min |
| iceline (8) | ~30 min | ~9 min |
| span (3) | ~12 min | ~4 min |
| spire (3) | ~12 min | ~4 min |
| all 24 | ~90 min | ~30 min |

The GATE pack's fresh skip (seeds 11-20) adds about 3 min a room. CPU: 12 cores / 24 threads (Ryzen AI 9 HX 370), power plan
Performance at 08:13; builders run 2 jobs each in their trees (builder-brief.md step 5), so a 6-job lead pool plus two builders is 10
processes. Taking every level every round does not fit an 8-hour night with one review slot; two or three levels, about 12 rooms, does
(a round of about 12-15 min pooled, one combined review of about 25 min).

Housekeeping: this scout's own `du -sh Builds` (PID 69344) outlived its stopped shell and was stopped by its PID (the TaskStop-orphans lesson, NIGHT52).

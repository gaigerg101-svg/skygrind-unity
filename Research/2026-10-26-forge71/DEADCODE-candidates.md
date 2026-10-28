# FORGE71: dead-code candidates and old OFF rows (dead-code auditor, 2026-10-26; read-only pass, nothing deleted)

Tree: port/stage3 at 20f07e72e (NIGHT70's close). Base list: Logs/n61/DEADCODE-candidates.md (NIGHT61 deleted batches 1-7, 11-36, 1b
and 2b; Logs/n61/DEADCODE.md). NIGHT71 deletes from this list, one batch or one row per commit.

- **Part A (code):** 30 batches, about 465 lines.
  - 4 new batches (N1-N4), found by the lead finder in Game / Editor.
  - NIGHT61's leftovers re-verified against today's tree: 8, 9, 10, 37-50 (with one new symbol in 40 and one in 50), 51 (two new
    symbols), and Core 52-59.
- **Part B (old OFF rows):** 23 rows scored.
  - DELETE: 4 rows, about 540 lines: flow46, drone49, shift46, teach42. All four are view rows with no tape bit.
  - CANDIDATE for GG: 6 rows.
  - SKIP: 13 rows.
- **Read this before Part B:** 11 of the 23 rows are named in DECIDE pages older than the ones the forge grepped. DECIDE45 D-3 is
  GG's open choice for six of these rows: "kept in config (old tapes replay through them)" or "delete them and their code (old tapes
  then break)". See section B.0.

---

## How each proof was run (shared by every entry; per-entry results are in the tables)

- **Lead finder.** `scratchpad/idx.py` builds a word-token index. It covers every text file (.cs .unity .prefab .asset .json .sh .cjs
  .py .mjs .js .asmdef .txt .csv .tsv .yaml .shader .hlsl .uss .uxml .bat .ps1; not .md, not .meta) under Assets/, Tools/, Design/,
  PortExport/ and Packages/, untracked files included: 3,754 files and 75,886 tokens. `scratchpad/scan.py` then lists every declaration
  in Assets/Scripts/Game, Assets/Editor, Assets/Scripts/HudFx20 and Assets/Scripts/Bench whose name has a total count <= 1. It found
  133 leads. After removing array-literal noise and NIGHT61's known names, 25 were new. Each was checked by hand.
- **(a) No references.**
  - `python -I q.py idx.pkl <names>`: count 1 means the declaration only. The count is printed per symbol below.
  - `git grep -nw` gave the line numbers.
  - The ad-hoc eval scripts were checked with
    `grep -rIlwE "<all Part A names>" Logs Research nights Reports Tools --include=*.cs --include=*.sh --include=*.cjs --include=*.py --include=*.mjs --include=*.js --include=*.txt`.
    It printed nothing.
  - UnityEvent targets: `grep -rhoI "m_MethodName: [A-Za-z_]\+" Assets --include=*.unity --include=*.prefab` printed nothing.
  - Script GUIDs: N37Fixes.cs.meta has guid ff8418be726c3f73a92e8e94a3172d1f, and
    `grep -rlI <guid> Assets --include=*.unity --include=*.prefab --include=*.asset` printed nothing. No other batch removes a type
    that has its own .cs.meta.
  - Reflection: Core/RunSnapshot.cs reads instance fields reflectively (`GetFields(Inst)`, :435) on Core objects only. No Part A batch
    removes a snapshotted field, except NIGHT61's Core batches 52 and 53, whose notes stand.
- **(b) ON rows.**
  - Design/unity-config.json was read with python json.
  - `cmp Design/unity-config.json Assets/StreamingAssets/Design/unity-config.json` printed nothing: the two copies are identical, so
    every config line number below holds for both.
  - None of the Part A symbols is named by any code, so no row can reach it.
- **(c) The off chain.**
  - Tools/night/n71-gates.sh (the newest gates script) runs `--off57 --off58 --off59 --off60 --off61 --off62 --off63 --off64 --off65
    --off66 --off67`.
  - The off switches work by night (NightRows / N<nn>Blocks.ForceOff); none of them names a symbol. The index count of 1 covers
    Tools/SimHarness and Tools/night.
- **(d) Tests and smoke.** Index hits in Assets/Tests, SliceSmoke*.cs, PlayerChecks*.cs and FeelReel*.cs are 0 for every Part A symbol.
  The exceptions are the two tests that test only the batch 8 and 9 code; they are named in those batches.
- **Not run:**
  - the scratch-copy compiler build (section 5.2);
  - `dotnet format --verify-no-changes`;
  - Unity.
  
  Write-only fields and unused locals were not searched again: CS0414 / CS0219 come from a compile, and this pass did not compile.

---

## PART A: code

### Batch N1: UnityTuning, the NIGHT17 stamina-bar aliases and the Hud17 values they alone read (6 symbols, 2 files, 6 lines) (low)

| file:line | symbol | (a) count | (b) | (c) | (d) | removes |
|---|---|---|---|---|---|---|
| Assets/Scripts/Game/UnityTuning.CameraMove.cs:109 | `StaminaBarWidthN17` | 1 | none | 0 | 0 | line 109 |
| UnityTuning.CameraMove.cs:110 | `StaminaBarHeightN17` | 1 | none | 0 | 0 | line 110 |
| UnityTuning.CameraMove.cs:111 | `StaminaBarYN17` | 1 | none | 0 | 0 | line 111 |
| Assets/Scripts/Game/UnityTuning.Hud17.cs:115 | `Hud17StaminaWidth` (cascade: count 2, the declaration and CameraMove:109) | 2 | none | 0 | 0 | line 115 |
| UnityTuning.Hud17.cs:116 | `Hud17StaminaHeight` (cascade: CameraMove:110 only) | 2 | none | 0 | 0 | line 116 |
| UnityTuning.Hud17.cs:114 | `Hud17StaminaY` (cascade: CameraMove:111 only) | 2 | none | 0 | 0 | line 114 |

- **Re-run:** the compile gate (dn-check), Hud17Tests / StaminaBar tests if any (not checked which exist).
- **Risk:** low. These are constants. The comment at CameraMove:104-108 still describes StaminaTrailHold / StaminaTrailRate, which stay.

### Batch N2: UnityTuning, the points-pop literals and a glacier spike width (6 symbols, 2 files, 8 lines) (low)

| file:line | symbol | (a) | (b) | (c) | (d) | removes |
|---|---|---|---|---|---|---|
| Assets/Scripts/Game/UnityTuning.FeelPolish.cs:61 | `PointsPopBigSize` | 1 | none | 0 | 0 | line 61 |
| UnityTuning.FeelPolish.cs:62 | `PointsPopY`, `PointsPopBigY` (one line) | 1, 1 | none | 0 | 0 | line 62 |
| UnityTuning.FeelPolish.cs:63 | `PointsPopScaleRate`, `PointsPopFadeRate` (one line) | 1, 1 | none | 0 | 0 | line 63 |
| Assets/Scripts/Game/UnityTuning.Glacier26.cs:12 | `GlacierSpikeWidth` (and its summary, line 11) | 1 | n26 glacier rows (no reader) | 0 | 0 | lines 11-12 |

- **Also:** the hud.js comment lines 58-60, which describe only these five values. Line 57 (the section header) and line 64 onwards
  (the label's box) stay.
- **Re-run:** the compile gate; UiPointsPops' test class if one exists (not checked).
- **Risk:** low. The live points pops read config.points and other constants (not traced).

### Batch N3: UnityTuning, three more unused values (3 symbols, 2 files, 2 lines + 1 rewrite) (low)

| file:line | symbol | (a) | removes |
|---|---|---|---|
| Assets/Scripts/Game/UnityTuning.Hud17.cs:133 | `Hud17SwapNameH` | 1 | line 133 |
| UnityTuning.Hud17.cs:134 | `Hud17CardSwapFrom` | 1 | line 134 |
| Assets/Scripts/Game/UnityTuning.Crown37.cs:26 | `Crown37BeamGain` (it shares the line with `Crown37ShardSize`, which is alive: count 2, used in Crown37View.Crown.cs) | 1 | line 26 becomes `public const float Crown37ShardSize = 0.5f;`, and the summary on line 25 drops "the beam cut's beam (m wide, gain)" |

- **Proofs:** (b) none, (c) 0, (d) 0 for all three.
- **Re-run:** the compile gate.
- **Risk:** low.

### Batch N4: N37Fixes.cs, a whole static class nothing names (1 type + 3 members, 1 file + .meta, 26 lines) (low-medium)

| file:line | symbol | (a) | (b) | (c) | (d) | removes |
|---|---|---|---|---|---|---|
| Assets/Scripts/Game/N37Fixes.cs:7 | `N37Fixes` (static class: `On`, `ViewRowOn`, the private `Off(string[])`) | 1 (`N37Fixes` occurs only at its declaration) | its rows are read through other classes' ViewRowOn | the class reads `-skygrindOff29`..`-skygrindOff37`, outside the gates' `--off57..--off67` | 0 | the whole file and N37Fixes.cs.meta |

- **Why it looks dead:**
  - `ViewRowOn` has a count of 121, but those calls are qualified by other classes. Nothing writes `N37Fixes.`, and a `using static`
    would name the class.
  - A static class cannot be a component.
  - Its GUID ff8418be... is in no scene, prefab or asset.
  - Last touched f79cf1c71 (2026-10-12, NIGHT37's shared commit).
- **Re-run:** the compile gate; the Player smoke at the next build (the `-skygrindOff37` path has no other reader in this class).
- **Risk:** low-medium. It deletes a file, and its header documents how `-skygrindOff37` behaves; the lead may want that note kept
  somewhere.

### Batches 37-50 (NIGHT61's unused public Game read-outs): re-verified, still dead (49 + 2 symbols, about 117 lines)

Every symbol in the table at Logs/n61/DEADCODE-candidates.md:544-594 still has an index count of 1 today: the q.py output for all 49
names printed 1, and `git grep -nw` found only the declaration. The NIGHT61 file:line and spans hold, except:

- **BossMarks32.cs:** `CampRingShown` is now at **line 227** (was 221). It removes lines 226-227: the summary "What is drawn now (the
  reel counts frames)" and the property. `GlobsShown` on line 228 stays.
- **Batch 40 adds** `assistBack` (Assets/Scripts/Game/SurvivalGame.cs:51, a private string field). Its only other refs are the two in
  `ToggleAssist` (SurvivalGame.Flow.cs:56-57), so it goes with `ToggleAssist` (SurvivalGame.Flow.cs:49-59). That makes 1 more line.
  SurvivalGame is a MonoBehaviour, but the field is private and not `[SerializeField]`, so it is not serialized. RunSnapshot is Core
  and never touches SurvivalGame.
- **Batch 50 adds** `NearHidden63` (Assets/Scripts/Game/Brawl40View.N63V.cs:40, public read-out "For the reels"; count 1). It removes
  lines 39-40. Its row, n63.bossMotion63, is ON, and nothing reads this getter. The field `nearHidden63` stays (count 3, live).
- **Not dead:** `KioskDebugIdle` (batch 40) forwards to `Kiosk.DebugIdle`, which KioskTests calls, so `Kiosk.DebugIdle` stays alive;
  only the SurvivalGame forwarder goes. `RouteCopySeedNow` forwards to `CopyRouteSeed` (count 5, live).

Re-run per batch: as in NIGHT61's table, plus fingerprints plain and `--off57`..`--off67` (no hash should move).

Risk: low-medium. No caller anywhere, but a later eval may want a public read-out back.

### Batch 51: ShotTour.RunReel and the constants it frees (8 symbols, 3 files, about 86 lines) (medium)

| file:line | symbol | (a) | removes |
|---|---|---|---|
| Assets/Scripts/Game/ShotTour.cs:109 | `RunReel(SurvivalGame, string, List<string>)` (NIGHT17's screens reel; ShotTour's live entries are `Run` / `RunUi`) | 1 | lines 104-179 (its summary, the body, the blank after it) |
| Assets/Scripts/Game/UnityTuning.Reel.cs:45 | `ReelScreenSettle` (cascade) | 2, both in RunReel | line 45 |
| UnityTuning.Reel.cs:46 | `ReelFightTime` (**new cascade**: all 3 uses are in RunReel at 151, 155 and 163) | 4, all in RunReel | line 46, and the comment on line 44 that introduces both |
| Assets/Scripts/Game/UnityTuning.Ui.cs:55-59 | `TitleBandWidth`, `TitleBandAlpha`, `TitleBandSolid`, `TitleShadeHeight`, `TitleShadeAlpha` (freed when NIGHT61 batch 7 deleted TitleView's Ui block; they **moved from 58-62 to 55-59**) | 1 each | lines 53-59 (the two-line comment and the five constants) |

- **Kept:** `ReelSettingsTabs` (count 4: FeelReel.cs and UiKit.cs also use it) and `LabTabId` (count 16).
- **Re-run:** the compile gate.
- **Risk:** medium for RunReel (75 lines of a NIGHT17 reel; no reel script names it, since the index count includes Tools/ and the
  Logs / Research scripts printed nothing). Low for the constants.

### Batch 8: UiHotbar.cs, the hotbar short-name path + its only test (5 symbols + 1 test, 3 files, about 62 lines) (medium)

Re-verified today. The lines shifted by about 2 since NIGHT61:

| file:line | symbol | (a) | removes |
|---|---|---|---|
| Assets/Scripts/Game/UiHotbar.cs:271 | `ShortName(string)` | 1 | lines 266-280 |
| UiHotbar.cs:82 | `ShortNames` | 3, all in ShortName | line 82 |
| UiHotbar.cs:42 | `HotbarLabel` (public static class; its only production caller is ShortName:276) | 11 (UiHotbar + FeelHudTests) | lines 41-54 |
| Assets/Scripts/Game/UnityTuning.FeelHud.cs:44 | `HotbarShortNames` (read only by HotbarLabel.Pick) | 3 | lines 40-54 (the comment, the dictionary, then `HotbarLabelPad` and the blank line) |
| UnityTuning.FeelHud.cs:53 | `HotbarLabelPad` (used only in ShortName) | 2 | (inside 40-54) |
| Assets/Tests/EditMode/Game/FeelHudTests.cs:103 | test `AHotbarLabelIsNeverACutWord` (tests only HotbarLabel) | 1 | lines 102-118, and "/ HotbarLabel" in the header on line 4 |

- **Proofs:** (b) none, (c) 0, (d) the test is the dead code's own.
- **Re-run:** FeelHudTests (the rest of it); the compile gate.
- **Risk:** medium. HotbarLabel is public and tested; it looks dead only because ShortName is dead.

### Batch 9: UiHotbar.cs, the hold-Y wheel hint's render path + its only test (7 symbols + 1 test, 3 files, about 39 lines) (medium)

| file:line | symbol | (a) | removes |
|---|---|---|---|
| UiHotbar.cs:581 | `WheelHintRender(int, bool)` | 1 | lines 580-596 |
| UiHotbar.cs:85 / 86 / 87 | `wheelHintDone`, `wheelHintShown`, `wheelHintSet` | 3 / 3 / 4, all in WheelHintRender | lines 85-87 |
| UiHotbar.cs:35 | `HotbarWheelHint` (public static class; its only production caller is WheelHintRender:586) | 10 | lines 34-40 |
| UnityTuning.FeelHud.cs:72 / 73 | `WheelHintMinGuns`, `WheelHintFormat` | 3 / 2 | lines 72-73 |
| FeelHudTests.cs:93 | test `TheWheelHintTeachesPadPlayersWithThreeGuns` | 1 | lines 92-101; "HotbarWheelHint /" on line 4; the section comment on line 90 if batch 8 also goes |

- **Kept:** `wheelHintText` (UiHotbar.cs:84, built inactive at 239-243) and `WheelHintGap`. Removing those changes the UI hierarchy,
  so that is a follow-up.
- **Re-run:** FeelHudTests; the compile gate; the smoke's pad steps at the next build.
- **Risk:** medium. NIGHT17 kept the rule on purpose ("its rule (HotbarWheelHint) and text stay built, never shown", UiHotbar.cs:356-358).

### Batch 10: UiHotbar.cs, the special hint's old render path (9 symbols, 3 files, about 68 lines) (medium)

| file:line | symbol | (a) | removes |
|---|---|---|---|
| UiHotbar.cs:700 | `RenderSpecialHint(string, bool)` | 1 | lines 699-717 |
| UiHotbar.cs:723 | `SpecialHintText(string, int)` | 3 (2 here + the comment in Hud20.cs:27) | lines 718-757 |
| UiHotbar.cs:94-97 | `shownSpecialWeapon`, `shownSpecialMode`, `specialOn`, `specialHints` (the field; the config key `ui.specialHints` is the web's and stays) | 4 / 3 / 3 / 7 in UiHotbar | lines 94-97, and the resets on lines 250-251 |
| Assets/Scripts/Game/UnityTuning.Hud20.cs:27 | `Hud20ShockWord` | 2 | line 27 |
| Assets/Scripts/Game/UnityTuning.Hotbar.cs:42 | `HudSpecialDivider` (**new cascade**: its only use is UiHotbar:752) | 2 | line 42 |
| UnityTuning.Hotbar.cs:51 | `HudPadSpecial` (**new cascade**: its only use is UiHotbar:727) | 2 | line 51 |

- **Kept:**
  - `specialText` (built at 192-195 and inactive; a hierarchy follow-up);
  - `HudSpecialRmb`, `Hud20ShockGun` and `HudPadAltPlayStation` (UiCooldown20 reads them);
  - `SpecialHint20.ZoomWord`: after this batch only Hud20Tests calls it, so it becomes test-only. That is a follow-up; do not delete
    a tested helper in this batch.
- **Re-run:** Hud20Tests, FeelHudTests; the compile gate.
- **Risk:** medium. This is a 39-line port of main.js specialHint; nothing renders it since NIGHT17 ("the teaching tier: gone at rest").

### Batches 52-59 (Core, last): re-verified, still dead (24 symbols, about 51 lines) (medium)

- **Re-verified today:**
  - Every Core symbol in NIGHT61's batches 52-59 (Logs/n61/DEADCODE-candidates.md:615-669) has the same file:line today:
    `grep -rnwE` in Assets/Scripts/Core printed each at its NIGHT61 line, with no other occurrence.
  - Each still has an index count of 1. `slammed` in Stag31 is still declared on 117, written on 482 and 536, and never read.
- **Files:** each file is Unity-only or a Unity-side boss, not a web port. NIGHT61's note on Wyrm.cs stands: the lead confirms it is
  the Unity KILN WYRM before batch 57.
- **Re-run per Core batch:** golden 0 mismatches, every trajectory replay bit-exact, fingerprints plain and `--off57`..`--off67`
  (Tools/night/n71-gates.sh), and the test classes NIGHT61 named.
- **Risk:** medium. These are Core commits under rule 3. Batches 52 and 53 remove a field from a snapshotted enemy, which changes
  RunSnapshot's field set.

---

## PART B: old OFF rows (NIGHT49 or older)

### B.0 Before deleting any row: DECIDE pages the forge did not grep

The forge grepped THREADS.md, HANDOFF.md, DECIDE6*.md, DECIDE70.md and OWNER.md. I also grepped docs/memory/ledger/*.md,
NIGHT46-49-LOG.md and DECIDE42-49.md. DECIDE2x / DECIDE3x were **not checked**. These DECIDE items name candidate rows:

- **DECIDE45 D-3** (balance32, ride38, doors38 / juice38, stag38, pump40, boost42 / parry42).
  - Options, quoted from the page: "listed in docs/RETIRED.md, kept in config (old tapes replay through them)", or "delete them and
    their code (old tapes then break)".
  - This is GG's question about exactly this pass. GG's 2026-10-26 "Dead code + old OFF rows" may answer it, but the lead should
    confirm that before deleting any of these six. None of the six is ranked DELETE below.
- DECIDE42 D-9 (boost42), D-11 (teach42), D-12 (arena42); DECIDE45 D-6 (arena42); DECIDE46 N-2 (shift46, cited in the
  Shift46View.cs header) and its retired table (reticle42); DECIDE49 D-2 (drone49).
- NIGHT61's list marks n41.juice41 "OFF; GG decides".

The four DELETE rows (flow46, drone49, shift46, teach42) are all **view rows with no tape bit**:

- N46SharedTests.cs:97 asserts "is a view row, no tape bit". teach42 / reticle42 are N42View rows, and N42Blocks.cs:5 says
  "no bit". drone49 is N49 ViewRows.
- So deleting them cannot break an old tape or move a fingerprint.
- Three of them (drone49, shift46, teach42) appear in a DECIDE item that recorded an OFF verdict. If the lead reads "a DECIDE question
  names it" to include decided items, those three drop to CANDIDATE; flow46 stays DELETE.

Scoring terms (from the brief):

- +3 last touched >= 10 waves ago: every row's sites were last touched at NIGHT54 or earlier, so every row gets +3. The newest touch
  found was 3288a81ee, 2026-10-20, "NIGHT54 L light54", on WyrmVisual26.cs and Shift46View.cs.
- +2 recorded verdict failed / superseded; +2 all refs in one assembly and <= 3 files (production code; tests counted apart);
  +1 Game/Editor only; +1 tests pin the OFF path; +1 removes >= 50 lines.
- -3 a MonoBehaviour / ScriptableObject / serialized field deleted; -2 touches simulate() order, a timer or the RNG.

Every row's `enabled` was read with python json on Design/unity-config.json: all 23 are `false`. The copies are byte-identical (cmp
printed nothing), so the config line spans below apply to both. The off chain: no gates script or harness `--offNN` path names any
row (`scratchpad/rows.py` lists every hit in Assets/ and Tools/; the Tools hits are the rows' own night scripts under Tools/night and
their harness commands, listed per row).

### B.1 DELETE (score >= 7)

#### flow46 (NIGHT46): 7 = +3 old, +0 verdict (NIGHT46-LOG only "shift46 / flow46 off"), +2 one file, +1 Game only, +1 tests pin OFF, +0 lines

- **Code sites:** none. The row has no reader: the only production mention is a comment, SurvivalSim.N46.cs:6 ("flow46, feel46) are
  data only"). Nothing calls `NightRows.ViewOn(..., "flow46")` (rows.py: 6 hits in 3 files, the other two being Tools/night/n46-shared.cjs
  and the test).
- **Removes:**
  - the config key, Design/unity-config.json:7086-7090, and the same lines in Assets/StreamingAssets/Design/unity-config.json
    (5 + 5 lines);
  - `"flow46", ` from N46SharedTests.cs:18;
  - the word flow46 from the comment SurvivalSim.N46.cs:5-6.
- **OFF behaviour kept:** nothing changes (no reader).
- **Tests:** N46SharedTests; fingerprints plain and `--off57..--off67` (expected EQUAL); the compile gate.
- **Risk:** low.

#### drone49 (NIGHT49): 9 = +3 old, +2 verdict (NIGHT49-LOG: "OFF | the drone keeps its NIGHT47 look (the old drone won three pairs)"), +2 one file, +1 Game only, +1 tests pin OFF, +0 lines

- **Code site:** Assets/Scripts/Game/EnemyViews.Drone.cs:286, the term `&& !(Foes48.On(game.cfg) && NightRows.ViewOn(game.cfg, "drone49"))`.
  With the row off, ViewOn is false, so the argument is `f47`.
- **Removes:**
  - the term, so line 286 passes `f47`; the comment on line 285 is rewritten;
  - the config key, Design/unity-config.json:8251-8256 and the StreamingAssets copy (6 + 6);
  - `, "drone49"` from N49SharedTests.cs:14.
- **Check first:** that `Foes48.On` has no side effect. It is called here only inside the dropped term; not checked.
- **OFF behaviour kept:** the NIGHT47 drone band whenever f47 holds.
- **Tests:** N49SharedTests; EnemyViews test classes if any (not checked); the compile gate.
- **Risk:** low. Named in DECIDE49 D-2 (decided OFF).

#### shift46 (NIGHT46): 8 = +3 old, +2 verdict (ledger/wave-46.md: "SHORT: shift46 (the state-change light) built, blind pairs 2 of 5 agreed, OFF"), +0 files (5), +1 Game only, +1 tests pin OFF, +1 lines (about 425)

- **Code sites:**
  - Assets/Scripts/Game/Shift46View.cs (203 lines; a plain class, built by `new Shift46View(g)`, not a MonoBehaviour);
  - Assets/Scripts/Game/UnityTuning.Shift46.cs (66 lines);
  - Assets/Scripts/Game/FeelReel.N46.Shift.cs (144 lines; the a546 shift46 scenario);
  - the hooks in Feel46View.cs: line 80 (field), 117-118 (Dispose / new) and 221 (`shift46?.Render`);
  - FeelReel.N46.Feel.cs:42, which picks the scenario through `Shift46View.ReelFlag` (`-skygrindShift46`). No Tools script passes
    that flag: `grep -rn "skygrindShift46\|Shift46" Tools --include=*.sh --include=*.cjs -l` printed nothing.
- **Removes:**
  - the three files and their .meta;
  - the four hook lines in Feel46View;
  - FeelReel.N46.Feel.cs:42 becomes `body = Feel46Scenario(...)`;
  - the config key, Design/unity-config.json:7081-7085 and the copy (5 + 5);
  - `"shift46", ` from N46SharedTests.cs:18;
  - the mentions in Tools/night/gen-n46/n47-*.cjs (old generators, left as history; not run).
- **OFF behaviour kept:** Feel46View without the fixtures. The row's own header says the fixtures are not drawn with it off.
- **Tests:** N46SharedTests; Feel46 test classes if any (not checked); the compile gate; the a546 reel at the next build (feel46's
  half).
- **Risk:** medium (three files). Named in DECIDE46 N-2.

#### teach42 (NIGHT42): 7 = +3 old, +2 verdict (DECIDE42 D-11 / ledger/wave-42.md: "teach42 OFF (2 lines in 60 s with and without it)"; superseded by n43.teach43), +0 files, +1 Game only, +0 tests, +1 lines (about 95)

- **Code sites:**
  - Assets/Scripts/Game/UiTeach42.cs (71 lines, a partial of Ui: `Teach42Hold`, `Teach42Lesson`, `Teach42Started` / `Teach42Held`,
    and the reads of `design.n42.teach42.windowS / maxLines / skip`);
  - the hooks UiHud33.cs:772-773;
  - FeelReel.N42.First.cs:39 and :92 (the first42 reel prints `Ui.Teach42Started` / `Teach42Held`, and :91 prints the row);
  - comments: N42View.cs:1, N42Blocks.cs:5, UiPinHint63.cs:2, FeelReel.N52.Hud.cs:344, Harness.N42.Level.cs:1.
- **Removes:**
  - UiTeach42.cs and its .meta;
  - UiHud33.cs:772 becomes `string teach = Teach35Want(frameDt, Teach35SlotNow);`, and 773 is dropped (the comment says
    "off: unchanged");
  - FeelReel.N42.First's teach42 print terms;
  - the config key, Design/unity-config.json:6425-6435 and the copy (11 + 11).
- **Before the edit:** read UiTeach42.cs to confirm that `Teach42Hold` is false and `Teach42Lesson` returns `lesson` unchanged while
  `t42Read == 0`. I did not read the body; the claim rests on the comment on UiHud33.cs:773.
- **OFF behaviour kept:** teach35's line and the lesson line, unchanged.
- **Tests:** the compile gate; the first42 reel at the next build (its line loses the teach42 terms); Teach35 tests if any (not checked).
- **Risk:** medium (a UI hook in the HUD's teaching channel). Named in DECIDE42 D-11 (decided OFF).

### B.2 CANDIDATE for GG (score 5-6, or 7 with GG's own words)

| row | score (terms) | what deleting means | why not DELETE |
|---|---|---|---|
| n42.reticle42 | 7 (+3, +2 "not shown faster" on a frame and retired for reticle43 / hud43 (DECIDE46), +0 (UiHud38.Reticle42.cs, UnityTuning.Reticle42.cs, the UiHud38.cs:290 hook, FeelReel.N42.Weapons.cs), +1 Game, +0, +1) | the two Reticle42 files, the hook on line 290, and the config key at 6405-6409 x2 | its header quotes GG (G8: "animation for redical for all weapons needs to spin faster ..."); by the forge's parry42 rule, GG's words are GG's call |
| n22.overdrive | 6 (+3, +0 never judged, +2 one file, +0 Core, +1 N22SharedTests, +0) | never built: no code beyond the N22Blocks bit. Delete the `overdrive = cfg.Design(...)` line (N22Blocks.cs:38) and the config key at 1082-1085 x2; **keep the bit** as a retired field (the tape byte, like `retired0`) | a Core commit for 4 config lines; rule 3 gates |
| n32.balance32 | 6 (+3, +2 superseded (RETIRED.md), +0 (N32Blocks, FeelReel.N32.Grapple, FeelReel.N27 PhaseN32G, Harness.N32.Grapple's balance32 command, about 70 lines), +0 Core, +0, +1) | it has no sim code ("balance32 has no sim change yet", FeelReel.N32.Grapple.cs:37). Delete the harness command, the reel line, the config key at 3226-3229 x2 and the From() read; keep the bit | named in DECIDE45 D-3 |
| n38.iceline38 | 6 (+3, +2 superseded by iceline40 / iceline43, +0 (N38Blocks, Core Curriculum25.cs, Walk38Iceline.cs, Harness.N38.Iceline), +0 Core, +0, +1) | a level row: the level swap in Curriculum25 (Core), the n38-iceline map data, and the config key at 5243-5248 x2 | a Core level swap plus map data; not traced whether a tape stores the level id |
| n41.juice41 | 6 (+3, +0, +2 Juice38.cs:129-132 only, +1 Game, +0, +0) | Juice38.cs:131-132: `iter` goes, and `J(k)` reads `config.Num(p + k)`; delete the config key at 6125-6129 x2 | NIGHT61's list marks it "GG decides"; it is the iteration numbers of a live (ON through juice41On) juice |
| n24.prewarm | 5 (+3, +2 ledger/wave-24.md "prewarm made no gain and is off", superseded by Polish53 / FirstShot53, +0, +0 Core bit, +0, +0) | only Prewarm24.Start / Run (the ON pass) and SurvivalGame.N24.cs:12. **Keep:** `Prewarm24.WakeScreens` / `Prewarm24Screens` (the live n25.hitchFix's Prewarm25.cs uses them), the key `design.n24.prewarm.glyphs` (Prewarm25.cs:43 reads it), BigPick24Tests' WakeScreens tests (:79-110, a shared helper's tests) and the N24 bit 8 (N24SharedTests:95 pins it) | the pieces are shared with a live row; less than 50 lines go |

### B.3 SKIP (score < 5, or a hard rule)

| row | score / rule | reason |
|---|---|---|
| n38.ride38 | rule 3 | 8 hits in Mover.cs; movement is never on the list. Also DECIDE45 D-3 |
| n42.boost42 | rule 3 | named in Mover.cs, and its Core Boost42.Params is boost43's live shape ("boost42's shape with boost43's numbers", RETIRED.md; FeelReel.N42.Wall uses it). DECIDE42 D-9, DECIDE45 D-3 |
| n27.chargeShots27 | about 1 | 90 hits in 32 files, including the web-ported Combat.cs. Name collision: ChargeShots27.cs is live through n31.charge / Pierce33. A row name is not a file |
| n38.doors38 / n38.juice38 / n40.pump40 | load-bearing | RETIRED.md: their code is ON through n41.doors41On / juice41On / pump41On (N41View.cs; Juice38.cs:128 reads `design.n38.juice38.*`, which is live). Removing "the ON branch" removes live code. At most, the `|| row` term and the `enabled` key could go; not scored. DECIDE45 D-3 |
| n22.trickChain | 2 (+3, +0 "not judged", +0 (Core Trick22, N22Blocks, Game SurvivalSim.Trick22, the harness, Trick22Tests), +0, +0, +1, -2) | a sim step inside simulate() (`trick22.Step()`, SurvivalSim.Trick22.cs:49) with step timers (`StepsFor`); Core |
| n38.stag38 | 4 (+3, +2, +0, +0, +0, +1, -2) | Core Stag37Arena lane-wall geometry (colliders in the sim); DECIDE45 D-3. Its dead read-out `Stag37View.Read38.Arrow38Shown` (Stag37View.Read38.cs:23, count 1) goes only with the row |
| n48.kilnBoost48 | 3 (+3, +0 (NIGHT48-LOG: "GREEN (numbers) ... kilnBoost48 OFF": not a failure), +0, +0 Core, +1, +1, -2) | a damage multiplier in Core Wyrm.cs:1332 (sim); it shares SurvivalSim.N48.Brawl.cs with the excluded wyrmTell48 / wyrmDive48 |
| n42.arena42 | 1 (+3, +2, +0, +0, +0, +1, -3, -2) | the Arena42View MonoBehaviour; the grinder damages in the sim; DECIDE42 D-12, DECIDE45 D-6 |
| n26.wormVisual | 2 (+3, +0, +0, +1, +0, +1, -3) | the WormBossParts Resources asset + the Editor's WormBossSetup prefab builder; the Tripo models are enemy art (rule 7: GG's). Its dead private helpers `WyrmVisual26.SetArrays` (:560), `SetAura` (:569) and the `mpbHead` / `mpbBody` declarators (:44) have count 1 and go only with the row |
| n48.lineup48 | 3 (+3, +0, +0, +1, +1, +1, -3) | the Lineup48Rig MonoBehaviour (AddComponent in the a948 reel); a reel-only switch, not game code |
| n28.rink | 1 (+3, +0 "not judged", +0, +0 Core, +0, +1, -2) | load-bearing numbers: `design.n28.rink.*` is read by Core Rink28Arena.Params (Rink28Arena.cs:32-44), and the live glacier rows build on it (Glacier28.cs:20 and 188; Rink28Arena.cs:48: "NIGHT30 B (design.n30.glacier30): design.n28.rink with every number found under `over`"); a Core sim arena. Only `Rink28Arena.Enabled` (the `enabled` read, :32) is row-only; not worth a Core commit |

---

## Rejected near-misses (symbol: what keeps it alive)

- `BuildStamp.LogStamp` (BuildStamp.cs:73): `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]`.
- `MapMarker.PlaceFromValues` (MapAuthoring/MapMarker.cs:22): `[ContextMenu("Place from values")]`.
- `WormBossSetup.BuildPrefab`, `MapMenu.ExportKeepNav`, `SpikeSetup.CreateArenaScene` / `CreateEnemiesScene`,
  `SurvivalSetup.CreateSurvivalScene` / `AddMaterials`: `[MenuItem]` (Assets/Editor; scenes are built through these, CLAUDE.md).
- `SpikeSetup`, `MapMenu` (static classes, count 1): they host `[MenuItem]` methods.
- `Cap57Pass.YellowArgs` (Render/Cap57Pass.cs:172): n56.yellow56, an OFF row from NIGHT55 on (excluded).
- `SurvivalSim.N48.Brawl.WyrmTell48On` (:54, count 1): the code of n48.wyrmTell48, which a thread names (excluded).
- `Stag37View.Read38.Arrow38Shown`, `WyrmVisual26.SetArrays` / `SetAura` / `mpbHead` / `mpbBody`: dead inside SKIP rows (stag38,
  wormVisual); they go only with their row.
- `Kiosk.DebugIdle`: KioskTests calls it (only the SurvivalGame forwarder `KioskDebugIdle` is dead, batch 40).
- `SpecialHint20.ZoomWord`: Hud20Tests calls it (after batch 10 it is test-only: a follow-up, not this list).
- `ReelSettingsTabs` (FeelReel.cs, UiKit.cs), `LabTabId` (16 refs), `CopyRouteSeed` (5), `assistBack` before batch 40: alive.
- `Crown37ShardSize` (Crown37View.Crown.cs): alive; only its line-mate `Crown37BeamGain` goes (N3).
- `ViewRowOn` (121 refs): alive in the other N<nn>Fixes / view classes; only N37Fixes' copy is dead (N4).
- `wheelHintText`, `specialText`: still built (inactive); removing them changes the UI hierarchy (follow-ups after 9 / 10).
- n42.parry42 and every row from NIGHT55 on, design.n65 / n67 rows, and the thread-named rows the brief lists: excluded by the brief.
- The array-literal "declarations" the scanner flagged (BulwarkFx20.cs:154-159, Enemy43View.Burrow.cs:285-290, UnityTuning.Models40 /
  Models43 rows, Viewmodel.Ports20.cs:65-66): data rows, not symbols.

## Not checked

- The scratch-copy compile (CS0414 / CS0219 / CS0169: CS0169 is silenced by `NoWarn 0169`), `dotnet format --verify-no-changes`, and
  Unity. No new write-only field or unused local was searched.
- The scanner reads the first name of each declaration line and one-line signatures. Shared declarators were checked by hand only
  where a lead pointed at them (N2, N3).
- DECIDE2x / DECIDE3x pages, for Part B row names.
- Whether `Foes48.On` (drone49) has side effects. The bodies of UiTeach42.Teach42Hold / Teach42Lesson with the row off (teach42).
  Which test classes exist for UiPointsPops, StaminaBar, Feel46 and EnemyViews.
- Whether a tape stores the level id that iceline38 swaps.
- Exact line counts for the Part B removals beyond the file sizes given (`wc -l`) and the config spans (python).

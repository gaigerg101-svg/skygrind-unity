# NIGHT61 builder D: B1, focus60 never pauses an automation run (the Player tours)

Branch `n61-B1`, from bda4f11a (port/stage3 when this worktree was made). port/stage3 has since gained 7334a3f5 (A1) and fdf4d336 (the
slate); neither touches a file of mine. Row design.n60.focus60 (ON) is untouched in both config copies. This is a bug slice, so it is
exempt from the visible rule (no SPOT, no critic). Not touched: Core, Mover.cs, the golden vectors, any config row.

## Commits (in order)
1. **0d978dbe**, test first. It adds `Assets/Tests/EditMode/Game/FocusAutomation61Tests.cs` and its .meta (guid
   4da1be0291a7470393c5db2993585e50), plus the stub `SurvivalGame.MayPauseOnFocusLoss(string[] args) => true`. The stub gives the OLD
   answer: every run may pause. With it every automation case fails, so the test is RED for the right reason.
2. **16419271**, the change, in two files:
   - `SurvivalGame.N60L.cs`: the predicate is filled in, and `FocusPause60On` adds `|| !MayPauseOnFocusLoss(args)`. NIGHT60's
     -skygrindOff60 / -skygrindNoFocus60 / -skygrindBench tests are kept exactly as written.
   - `PlayerChecks.cs`: `PlayerChecks.FocusAutomation61Modes`, the mode flags, sits beside `Attach`.
3. This notes file.

## The red (read from the code at bda4f11a; no Player run)
- **The rule.** `SurvivalGame.N60L.cs:37` turns focus60 off only for `-skygrindOff60` (or older), `-skygrindNoFocus60` and
  `-skygrindBench`. The call at `SurvivalGame.cs:297` also skips the attract and `FeelReel.Running`. `FeelReel.Running` is set only by
  `FeelReel.Run` (`FeelReel.cs:63/121`).
- **Exposed: runs that play live under PlayerChecks with focus60 on.**
  - The tours: ShotTour `-skygrindShots` (`ShotTour.cs:85/127/203` StartRun), FeelStrip `-skygrindStrip` (`FeelStrip.cs:441`),
    StyleTile `-skygrindTile` (`StyleTile.cs:30`), HitchTour24 `-skygrindHitch` (`HitchTour24.cs:39`).
  - The NIGHT29 checks: DoorFree29Check (`DoorFree29Check.cs:26`) and LevelSelect29Check (a real PLAY press, `LevelSelect29Check.cs:49`).
  - The smoke (`SliceSmoke.cs:114/325/376/455`).
- **Shielded:**
  - The reel, by `FeelReel.Running`.
  - The bench, by its flag.
  - Golden, fingerprint, farm and replay run synchronously in one frame, so they never reach a Playing frame.
- **Measured in a Player, for the smoke only.** NIGHT60's close steal smoke (`Logs/player/n56-smoke-steal-n60-freeze.log`) shows the
  pause and the guard resolving it:
  - `53.1s n55 guard: a pause the smoke did not make (the cursor lock dropped: the window lost the focus) resolved`
  - summary: `focus changes 1 ... resolved: dropped cursor lock 1`

  In a standalone Player the lock never drops on a focus loss (B9's finding). So that pause was focus60's, even though Smoke55
  classified it as LockDropped.
- **Not measured:** a tour paused in a Player. No tour has run with a planted steal.

## The rule now (`MayPauseOnFocusLoss`, in Attach's order)
1. A null command line: the run may pause (a player's game).
2. `-skygrindDoorFree29` or `-skygrindLevelSelect29`: no pause. These are Attach's first two modes; with either one the probe never starts.
3. `FocusProbe60.Requested` (`-skygrindFocusProbe60` or the planted `-skygrindFocusProbe60NoPause`): the run may pause. This is the probe's
   own run, and it measures this very pause.
4. Any flag in `PlayerChecks.FocusAutomation61Modes`: no pause. The list is `-skygrindSmoke`, `-skygrindShots`, `-skygrindReel`,
   `-skygrindTile`, `-skygrindStrip`, `-skygrindHitch`, `-skygrindGolden`, `-skygrindFingerprint`, `-skygrindFarm`, `-skygrindReplay`,
   `-skygrindBench`.
5. Anything else: the run may pause. Switches and options that start no mode leave a player's game:
   - row switches: `-skygrindOffNN`, `-skygrindNoExit58`
   - a map: `-skygrindMap` alone (L-062: it boots SURVIVAL)
   - other switches: `-skygrindDev`, `-skygrindKiosk`
   - a reel option without `-skygrindReel`
   The row switches stay FocusPause60On's own rules. A Web build's `?skygrindSmoke` / `?skygrindBench` are not read (no Web build is made,
   and NIGHT60's rule did not read them either).

## Every PlayerChecks flag, and its test case
| flag (Attach order) | what it starts | test cases (FocusAutomation61Tests) | predicate |
|---|---|---|---|
| -skygrindOff19 .. -skygrindOff27, the -skygrindOffNN prefix | row switches, no mode | -skygrindOff61 (a player's game); -skygrindOff29 beside -skygrindLevelSelect29 | may pause (FocusPause60On's -skygrindOff60-or-older rule decides) |
| -skygrindDoorFree29 (+ its -skygrindNoHold29) | DoorFree29Check | 2 cases; also with the probe's flag | no pause |
| -skygrindLevelSelect29 | LevelSelect29Check | 2 cases; also with the probe's flag | no pause |
| -skygrindFocusProbe60 / -skygrindFocusProbe60NoPause | the probe (excepted) | 3 cases; also with -skygrindSmoke (the probe wins) | may pause |
| -skygrindSmoke (+ -skygrindPadShot) | the slice smoke | 2 cases | no pause |
| -skygrindShots (+ -skygrindShotsUi, -skygrindMap) | ShotTour | 2 cases | no pause |
| -skygrindReel (+ -skygrindReelOnly, -skygrindReelPad; reel.sh's -skygrindMap, -skygrindClearShots60, -skygrindNoExit58) | FeelReel | 2 cases | no pause |
| -skygrindTile | StyleTile | 1 | no pause |
| -skygrindStrip | FeelStrip | 1 | no pause |
| -skygrindHitch | HitchTour24 | 1 | no pause |
| -skygrindGolden, -skygrindFingerprint, -skygrindFarm, -skygrindReplay | synchronous checks | 1 each | no pause |
| -skygrindBench (+ -skygrindFpsCap, -skygrindProfile) | the bench | 3 cases | no pause (its NIGHT60 test kept too) |

`EveryPlayerChecksFlag_IsClassified` scans PlayerChecks.cs for its `"-skygrind..."` literals and finds 26. Each one is either a row switch,
one of the six options, or one of the 11 modes, and each mode must read no pause. A mode added to Attach as a literal fails this test until
its flag joins the list. The scan's limit: a dispatch through another class's `Requested`, as DoorFree29Check, LevelSelect29Check and
FocusProbe60 do, is outside it. Those three are pinned by explicit cases.

## Checks
- **Compile.** `bash Tools/night/dn-check-wt.sh <scratch>/chk-D` gave 0 errors in all four projects (Skygrind.Game,
  Skygrind.Game.Tests, Skygrind.Core.Tests, Assembly-CSharp-Editor) three times: on bda4f11a before any change, on the test-first state,
  and on the change.
- **Gates.** `bash Tools/night/n61-gates.sh --no-editor` was all GREEN at 11:52:46 on the change's content:
  - config: the two copies identical
  - golden: 39 vector sets (48094 records), 22 replays (22 exact), 0 mismatches
  - fingerprint: plain and --off57 / --off58 / --off59 / --off60 / --off61 all EQUAL

  Re-run on the final HEAD: see the report line.
- **Scratch run, outside Unity.** This is not the EditMode test and not a Player. A scratch .NET 8 harness loaded the dotnet-built
  `Skygrind.Game.dll` by reflection. It evaluated `MayPauseOnFocusLoss` on the test file's own `[TestCase]` lines, the `[Test]` methods'
  cases and the PlayerChecks.cs scan:
  - stub build (0d978dbe): 33 fail and 15 pass. Every automation assertion failed; every "may still pause" assertion passed.
  - change build (16419271): 48 pass and 0 fail.

  The harness and its output were scratch only and were deleted.

## Not checked
- **The EditMode test** `FocusAutomation61Tests`, RED on 0d978dbe and GREEN on 16419271: not checked, because the Editor does not
  answer. Filter: `FocusAutomation61Tests`. The class uses `Application.dataPath` for the scan, as Level5Tests does.
- **doneWhen 2, the focus probe** (`n60-focus-probe.sh`): not checked, because no Player runs tonight for builders. From the code, all
  three cases should keep NIGHT60's results:
  - `-skygrindFocusProbe60`: MayPause is true, so the row reads on and the line says `DONE: paused on the focus loss True`.
  - with `-skygrindNoFocus60`: the kept rule turns the row off, so False.
  - with the planted `-skygrindFocusProbe60NoPause`: MayPause is true, the game pauses and the probe sets it back, so False.
- **The Editor compile gate:** not run (`--no-editor`).
- **A tour (-skygrindShots / -skygrindStrip / ...) with a planted steal in a Player:** not checked. No script for it exists.

## For the lead
- **Player commands, when a Player is allowed.** Build-rows line: none (focus60 is already ON in the repo, and no row is added).
  - `bash Tools/night/n60-focus-probe.sh <tag> on` should print `[focus60] start: focus60 row on` and `DONE: ... True`.
  - `bash Tools/night/n60-focus-probe.sh <tag> nofocus -skygrindNoFocus60` should print row off and `False`.
  - `bash Tools/night/n60-focus-probe.sh <tag> planted -skygrindFocusProbe60NoPause` should print `False`.
  - Each run must show `focus losses >= 1`; MISSED means no verdict.
- **The smoke as B1's Player line.** It costs nothing extra, because the close already runs it:
  - Command: `bash Tools/night/n56-smoke-steal.sh <tag>`.
  - Expected: `DONE: all steps passed`, and in the Player log's `n55 input summary`: `focus changes >= 1` with `dropped cursor lock 0`.
  - Red: n60-freeze read `dropped cursor lock 1` at 53.1 s (`Logs/player/n56-smoke-steal-n60-freeze.log`).
  - `focus changes 0` means the steal missed, and the run gives no verdict.
- **The pick.**
  - The test is a new .cs file. Memory pick-new-file-stale-editor applies: if the recompile says up_to_date, force
    ImportAsset + RequestScriptCompilation.
  - Touched test classes: FocusAutomation61Tests (new). The rule's old tests N60FocusPauseTests, N60FocusProbeTests and N56FocusPauseTests
    are unchanged; batch filter `N60Focus`.
- **Stale text, not touched (config is off limits to me).** focus60's `_why`, in both config copies, still reads "not in the attract, a
  reel or -skygrindBench; the smoke's SmokeGuard55 resolves it". After B1 every PlayerChecks run but the probe is excepted, the smoke
  included. No code reads that text.
- **Other builders tonight.** A slice that adds a PlayerChecks mode (a new probe) must add its flag to
  `PlayerChecks.FocusAutomation61Modes`. If it dispatches before the focus probe, it goes in the predicate's NIGHT29 line instead.
  Otherwise focus60 can pause that probe's run.

# D1: the dead-code pass

The input is the auditor's `Logs/n61/DEADCODE-candidates.md` (main tree). The lead's order: batches 1-7 and 11-36; batches 8-10, 37-51
and 52-59 are skipped tonight. Branch `n61-D1`, from n61-B1 at 0ab87de8. Each batch commit deletes only the listed lines.

How each line below was measured:
- **Re-grep:** word matches (ripgrep) over Assets/, Tools/ and Design/, run just before the deletion.
- **GUIDs:** no batch removes a type or a file, so no script GUID can be orphaned. The UnityEvent check found 0 `m_MethodName` targets
  in every .unity, .prefab and .asset under Assets.
- **Compile:** `dn-check-wt.sh`, as errors in Skygrind.Game / Skygrind.Game.Tests / Skygrind.Core.Tests / Assembly-CSharp-Editor.

- **Batch 1** (Ui.cs, UiArcline22.cs, UiHud36Text.cs):
  - Symbols: `assistLabels`, `AssistTexts`, `AppendHex2`, `FeedRankHex`, `feedRankHex`, `feedTextHex`, `arc22LabelColor`, `h36Lower`.
  - Removed: 23 lines, 0 added.
  - Re-grep: Assets 12 hits, all in the declarations or FeedRankHex's own cascade (`feedRankHex` 3, `feedTextHex` 3, each other symbol 1);
    Tools 0; Design 0.
  - Ui.cs:383 was a standalone statement, and `text` is still read at Ui.cs:371 and 381.
  - Compile: 0 / 0 / 0 / 0.
  - Follow-up, not on the list: `shownRank = -1` on Ui.cs:85 is now unreferenced, since its one read was inside FeedRankHex. Its
    warning was not checked. The list's span also leaves a double blank line where AssistTexts and AppendHex2 were.
- **Batch 2** (UiHud33.cs, UiMeta.cs, UiNav.cs):
  - Symbols: `h33StatusRead` (the declarator only: `bool h33HpRed, h33StatusRead;` became `bool h33HpRed;`), `MutatorName`,
    `ModName`, `MutatorNames`, `ModNames`, `NoNav`.
  - Removed: 23 lines, plus the one-line declarator edit.
  - Re-grep: Assets 8 matching lines, all in the declarations or their own cascades (`MutatorNames` / `ModNames` only inside their
    getters); Tools 0; Design 0.
  - Compile: 0 / 0 / 0 / 0.
  - Kept, as the list says: `navSkip` (UiNav.cs), now never filled but still read. Batch 2b below removes it on the lead's word.
- **Batch 1b** (Ui.cs; the lead's yes to batch 1's follow-up):
  - Symbol: `Ui.shownRank`, the field and its only write (its `= -1` initializer). Ui.cs:84, after batch 1, became
    `int shownHp = int.MinValue, shownRoundA = int.MinValue, shownRoundB = int.MinValue;`.
  - Removed: 0 lines, since this is a declarator edit on a shared line.
  - Re-grep: Assets 15 hits. The 1 in Game is this declarator. The other 14 are different symbols: StyleCore's own private field
    `shownRank` (Core/StyleCore.cs) and a parameter in StyleMath.StepShownRank (Core/Style.cs). Tools 0; Design 0.
  - No read was left after batch 1 removed FeedRankHex.
  - RunSnapshot (Core, no engine references) snapshots Core types only, so it cannot reach `Ui`.
  - Compile: 0 / 0 / 0 / 0.
- **Batch 2b** (UiNav.cs; the lead's follow-up to batch 2):
  - Symbol: `navSkip`. Removed the field (UiNav.cs:124) and rewrote the condition in Relink: `s.IsActive() && s.IsInteractable() &&
    !navSkip.Contains(s)` became `s.IsActive() && s.IsInteractable()`.
  - Same behaviour: batch 2 removed NoNav, the only writer, so the set was always empty and `!Contains` always true. `HashSet.Contains`
    has no side effects.
  - Removed: 1 line, plus the one edited condition.
  - Re-grep before the edit: Assets 2 hits (the field, the condition); Tools 0; Design 0; plus 1 mention in this notes file.
  - Compile: 0 / 0 / 0 / 0.
- **Batch 3** (FeelReel.N22.cs, FeelReel.N26.Bosses2.cs, FeelReel.N26.Glacier.cs):
  - Symbols: `N22GhostBody` (a partial declaration with no body and no call), `GlacierScenario` (its doc comment and body, lines 15-69;
    the live `glacier` entry `GlacierDriveScenario` stays), and the `died = 0,` declarator (CS0219).
  - Removed: 56 lines, plus the one-line declarator edit.
  - Re-grep: Assets 2 hits, the two declarations; `died` 1 hit in its own file; Tools 0; Design 0.
  - Compile: 0 / 0 / 0 / 0.
  - The listed span leaves a double blank line in Bosses2.cs.
- **Batch 4** (FeelReel.N29.Bosses.cs, FeelReel.N37.Crown.cs, FeelReel.N49.Look.cs):
  - Symbols: the local `pitchL` (the `, pitchL = 0` declarator), the local `legStep` (the `legStep = -1,` declarator), and the
    second copy of the local constants `ur` / `ug` / `ub` (FeelReel.N49.Look.cs:338, in GrayAuditV50, lines 308-422).
  - Removed: 1 line, plus two one-line declarator edits.
  - Re-grep:
    - In their own files: `pitchL` and `legStep` 1 hit each (the declarator).
    - `ur` / `ug` / `ub`: 3 lines in the file. Lines 51 and 61 are MeterV49's own copy, which is used; line 338 is unused in
      GrayAuditV50.
    - Elsewhere in Assets: 61 other matching lines, all `pitchL` fields or locals of other pilots. Tools: 5 files, all the harnesses' own locals or a
      Python variable. Design 0. A local cannot be referenced outside its method.
  - Compile: 0 / 0 / 0 / 0.
- **Batch 5** (FeelReel.N54.Move.cs, Bot/Walk38Foundry.cs, SurvivalSim.Wick37Pilot.cs):
  - Symbols: the local `camVyAtFeet` (its declarator, and its write in the max-feet branch); `Walk38Pilot`'s private write-only fields
    `done38`, `phaseStep` and `pitTries` (their declarators, their resets in Reset38, and the `done38 = false;` line in Chain);
    `Wick37Pilot.jumpHold` (its declarator and `if (jumpPress) jumpHold = 0;`).
  - Removed: 2 lines, plus 7 edited lines.
  - Every removed write assigns a constant, so no side effect is lost. `jumpPress` is still read at Wick37Pilot.cs:357.
  - Re-grep: Assets 7 lines for the first four names, all in their own files; Tools 0; Design 0. `jumpHold`: 2 lines in
    Wick37Pilot (the non-partial class's own field); every other `jumpHold` is another pilot's own field.
  - RunSnapshot never copies these fields: it saves fixed systems, and the sim's own section writes `input.Yaw` / `Pitch`, never the
    pilot object (SurvivalSim.Save.cs). That also settles the list's open question for batch 6: SurvivalSim's fields are never
    copied by reflection.
  - Compile: 0 / 0 / 0 / 0.
- **Batch 6** (UiWeapons.N27.cs, Loom37View.Look47.cs, SurvivalSim.Grind21.cs):
  - Symbols: the write-only fields `w27LitShown` (its declarator, and its reset in OnNewRun), `Loom37View.lastHp` (the
    `, lastHp = -1f` declarator) and `SurvivalSim.grindRecordsLoaded` (its declaration and its two constant writes; the
    `Records21.Load` call between them stays).
  - Removed: 4 lines, plus 2 edited lines. The list's header says 3, but its own rows add up to 4.
  - Re-grep: Assets 14 lines. 6 are these symbols, all in their own files; the other 8 are separate `lastHp` locals or fields
    (SandboxMarathon 5, WyrmVisual26 3). Tools 3 lines, a harness's own `lastHp` local. Design 0. (Corrected in the batch 7 commit:
    the first version of this line said 13 and 7.)
  - Compile: 0 / 0 / 0 / 0. SimHarness also compiles `SurvivalSim*.cs` and `Bot/*.cs`, so I built it too, which covers batches 5 and
    6: `dotnet build Tools/SimHarness -c Release` succeeded. Its 3 warnings are unrelated and predate this pass.
- **Batch 7** (TitleView.cs):
  - What went: the dead `partial class Ui` block, lines 118-172 (the NIGHT12 title backdrop, unwired since NIGHT17's menus). Its symbols:
    `BuildTitleBackdrop`, `RenderTitle`, `GradientSprite`, `Brighten`, `wordmarkClock`, `shownBreath`.
  - Kept: the TitleView class with its .cs.meta, and the namespace brace.
  - Removed: 55 lines.
  - Re-grep: Assets 14 lines, all inside the block (lines 121-171); Tools 0; Design 0.
  - The live title breath stays in UiMenus.cs. The constants this block freed (TitleBand* / TitleShade*) are batch 51, skipped tonight.
  - Compile: 0 / 0 / 0 / 0.

Batches 11-36 are unused UnityTuning constants (all under Assets/Scripts/Game/). For every batch below, unless its line says
otherwise:
- **Re-grep:** one combined word search over Assets/, Tools/ and Design/ finds exactly 1 hit per symbol, its own declaration. The
  combined glob was proved to reach Tools/ and Design/.
- **"for GG" check:** no constant belongs to an OFF row on the auditor's "for GG" list. I read each constant's comment and its file's
  header.
- **Comments left:** a deleted constant's section comment stays where the list did not name it, even when it now introduces nothing
  ("orphan comment" below).

- **Batch 11** (UnityTuning.CameraMove.cs, UnityTuning.Controls.cs):
  - Symbols: `SprintFovDegN17`, `SprintFovEaseN17`, `LinesStartSpread`, `StaminaBarWidthN18` / `StaminaBarHeightN18` / `StaminaBarYN18`
    (three declarator edits; the N17 twins stay), `SettingsDescY`, `SettingsDescHeight`, `NavFocusEdge`, `NavFocusFillAlpha`.
  - Removed: 7 lines, plus 3 declarator edits. Re-grep: 10 hits, one per symbol. Compile: 0 / 0 / 0 / 0.
  - Orphan comment: Controls.cs's "SETTINGS' description line" header (2 lines).
- **Batch 12** (UnityTuning.Crown37.cs, UnityTuning.Crown47.cs, UnityTuning.Crunch.cs):
  - Symbols: `Crown37BeamWidth` (a declarator edit), `Crown47FireDropM` (with its doc line), `ChunkHeadPop`, `ChunkHeadPopShot`,
    `CrunchNotchDepth`.
  - Removed: 5 lines, plus 1 declarator edit. Re-grep: 5 hits, one per symbol. Compile: 0 / 0 / 0 / 0.
  - "for GG": Crown47.cs's header assigns it to `design.n47.crown47` (ON), not the OFF `n48.flame48`.
  - The doc on Crown37.cs:25 still says "m wide".
- **Batch 13** (UnityTuning.Enemy.cs, UnityTuning.F47.cs, UnityTuning.Feel46.cs):
  - Symbols: `EnemyFlinchPush`, `EnemyFlinchIn`, `EnemyFlinchOut` (EnemyFlinchSquash stays), `Shield47DownYawDeg`, `Static46TexSize`
    (with its doc line).
  - Removed: 6 lines. Re-grep: 5 hits, one per symbol. Compile: 0 / 0 / 0 / 0.
  - "for GG": Shield47 is F47.cs's "foes47" section, and Static46 is `design.n46.feel46`. Neither is on the list.
  - SimHarness can also compile Feel46.cs alone (`-p:Extra`); no Tools file names `Static46TexSize`.
- **Batch 14** (UnityTuning.FeelHud.cs, UnityTuning.FeelPolish.cs, UnityTuning.FeelRows.cs):
  - Symbols: `MapSubLines` (the dictionary, 5 lines), `MapSubLineFallback`, `StripStampPad`, `SlideChargeHintFormat`, `PointsPopBigHex`,
    `LabNoteText`, `LabNoteHeight`, and three declarator edits: `PointsPopSize`, `PointsPopBigAt`, `PointsPopScaleAdd` (their
    siblings stay).
  - Removed: 11 lines, plus 3 declarator edits. Re-grep: 10 hits, one per symbol. Compile: 0 / 0 / 0 / 0.
  - Orphan comments: FeelHud.cs's "TITLE's MAP sub-line" header (3 lines), FeelPolish.cs's "slide charge's first-time hint" header
    (4 lines), FeelRows.cs's "LAB tab's note" comment (2 lines, now the class's last lines).
- **Batch 15** (UnityTuning.Fx20.cs, UnityTuning.Fx49.cs, UnityTuning.FxFixes.cs):
  - Symbols: `TelegraphParry`, `Fx49SparkSpeedPerLen` (with its doc line), `ParticleBoundsScale`.
  - Removed: 4 lines. Re-grep: 3 hits, one per symbol. Compile: 0 / 0 / 0 / 0.
  - "for GG": Fx49 is `design.n49.fx49`; the list's n49 rows are charge49 and drone49.
  - Orphan comment: FxFixes.cs's "polish #41" comment (2 lines).
- **Batch 16** (UnityTuning.Glacier26.cs):
  - Symbol: `GlacierSpikeEvery` (a declarator edit; `GlacierSpikeWidth` stays).
  - Removed: 0 lines, 1 declarator edit. Re-grep: 1 hit. Compile: 0 / 0 / 0 / 0.
  - The doc above it still names "spike boxes per metre".
- **Batch 17** (UnityTuning.Hotbar.cs, NIGHT13's hotbar layout):
  - Symbols: `HudWeaponPadTop` and `HudWeaponNameToSlot` (declarator edits), `HudWeaponRowGap`, `HudHotbarCellAlpha`, `HudHotbarOnAlpha`,
    `HudHotbarGlyphChars`, `HudHotbarGlyphColor`, `HudHotbarStaleAlpha`, `HudHotbarMorePad`, `HudMeterBackAlpha`.
  - Removed: 8 lines, plus 2 declarator edits. Re-grep: 10 hits, one per symbol. Compile: 0 / 0 / 0 / 0.
- **Batch 18** (UnityTuning.Hud17.cs, NIGHT17's HUD):
  - Symbols: `Hud17HpBarY`, `Hud17HpTrackAlpha`, `Hud17PopMidAt`, `Hud17PopBigAt`, `Hud17CalloutFrom`, `Hud17CalloutIn`,
    `Hud17StaminaGap`, `Hud17StaminaHollowAlpha`, `Hud17StaminaDimAlpha`, `Hud17StaminaHold`.
  - Removed: 10 lines. Re-grep: 10 hits, one per symbol. Compile: 0 / 0 / 0 / 0.
  - `Hud17PopBigAt`'s comment said "kept for the tier test's names", but no test names it, as the list says.
- **Batch 19** (UnityTuning.Hud17.cs, UnityTuning.Hud29.cs):
  - Symbols: `Hud17StaminaFade`, `Hud17StaminaFlash`, `Hud17StaminaFloorTick`, `Hud17CardGap`, `Hud17SwapNameY`, `Hud17SwapNameFrom`,
    `Hud17CardSwapTime`, `Hud17RoundRestLabelGap`, and two declarator edits: `Hud17SwapNamePadX` (Hud17SwapNameH stays) and
    `Hud29TagPadX` (Hud29TagPadY stays).
  - Removed: 8 lines, plus 2 declarator edits. Re-grep: 10 hits, one per symbol. Compile: 0 / 0 / 0 / 0.
- **Batch 20** (UnityTuning.Hud38.cs):
  - Symbol: `Hud38SnapOvershoot`.
  - Removed: 1 line. Re-grep: 1 hit. Compile: 0 / 0 / 0 / 0.
  - "for GG": the file is `design.n38.hud38`; the list's n38 rows are ride38 / iceline38 / finale38 / stag38 / doors38 / juice38.
  - SimHarness compiles this file, so I built it too: `dotnet build Tools/SimHarness -c Release`, 0 errors, succeeded.
- **Batch 21** (UnityTuning.Impressions.cs, NIGHT13's first-impression UI):
  - Symbols: `PointsLabelGap`, `StaleTip`, `ContractBadgeReward`, `ContractDormantLine`, `LessonTag`, `TitlePlayY`, `ModeLineSurvival`,
    `ModeLineHeat`, `SetupSubGap`, `PauseColumnX`.
  - Removed: 10 lines. Re-grep: 10 hits, one per symbol. Compile: 0 / 0 / 0 / 0.
  - Orphan comment: S10's "one short line under each run-setup choice" comment (2 lines). (Corrected in the batch 22 commit: the
    first version of this line named the S7 header, which still introduces ContractFooterGap.)
- **Batch 22** (UnityTuning.Impressions.cs, the PAUSED card's layout):
  - Symbols: `PauseTopY`, `PauseHeadY`, `PauseCardX`, `PauseCardTop`, `PauseCardWidth`, `PauseCardPad`, `PauseCardHeadHeight`,
    `PauseKeyLine`, `PauseKeyWidth`, `PauseKeyGap`.
  - Removed: 10 lines. Re-grep: 10 hits, one per symbol. Compile: 0 / 0 / 0 / 0.
  - Orphan comment: the S12 PAUSED header (4 lines), now above PauseKeysSkip alone.
  - From this batch on, chk-D is deleted after every compile (the lead: C: at 4.7 GB). After batch 22's: 4.8 GB free (`df -h /c`).
- **Batch 23** (UnityTuning.Impressions.cs, UnityTuning.Kit33.cs, UnityTuning.Loom37.cs):
  - Symbols: `PauseFixedTitle`, `PauseFixedKeys` (the array, 8 lines), `TitleControls`, `Kit33HeroFlatDepth`, `Loom37SpinHex`.
  - Removed: 12 lines. Re-grep: 5 hits, one per symbol. Compile: 0 / 0 / 0 / 0. SimHarness also compiles Kit33.cs, so I built it:
    0 errors, succeeded.
  - "for GG": Kit33 is `design.n33.kit33` and Loom37 is `design.n37.loom37`; neither is on the list.
  - Orphan comments: the fixed-keys comment (3 lines, now the class's last lines) and the TITLE controls-line comment (3 lines).
    Loom37.cs:19's doc still says "and its shut colour".
  - Disk after: 4.7 GB.
- **Batch 24** (UnityTuning.Maps.cs, UnityTuning.Menus17.cs):
  - Symbols: `MapButtonWidth`, `MapLabelWidth`, `MapRowLabel`, `ControlsTabId`, `DownRetryLine`, `PromptPunchRange`.
  - Removed: 6 lines. Re-grep: 6 hits, one per symbol. Compile: 0 / 0 / 0 / 0. Disk after: 4.7 GB.
- **Batch 25** (UnityTuning.Meta.cs):
  - Symbols: `MetaHeadHeight`, `SetupInfoAbove`, `SetupInfoWidth`, `SetupModeWidth`, `SetupButtonHeight`, `SetupChipWidth`,
    `SetupHeatLabelWidth`, `SetupGap`, `SetupLineHeight`, `RowBackAlpha`.
  - Removed: 10 lines. Re-grep: 10 hits, one per symbol. Compile: 0 / 0 / 0 / 0. Disk after: 4.7 GB.
- **Batch 26** (UnityTuning.Meta.cs, UnityTuning.Recap.cs, UnityTuning.Route.cs):
  - Symbols: `BarBackAlpha`, `FaintAlpha`, `WeaponRowHeight`, `PayoffX`, `HintFontSize`, `RecapTagUnderHead`, `RoutePlateRight`,
    `RouteButtonsTop`.
  - Removed: 8 lines. Re-grep: 8 hits, one per symbol. Compile: 0 / 0 / 0 / 0. Disk after: 4.6 GB.
- **Batch 27** (UnityTuning.StaminaBar.cs, UnityTuning.Tell36.cs, UnityTuning.U52.cs):
  - Symbols: `StaminaSpentFlash`, `StaminaEmptyPulse`, `Tell36PoseLean`, `Guns52ReelBoxMovePx`.
  - Removed: 4 lines. Re-grep: 4 hits, one per symbol. Compile: 0 / 0 / 0 / 0. Disk after: 4.6 GB.
- **Batches 28-36:** one re-grep covered all 85 symbols before batch 28: 85 hits, one per symbol, each at its listed line.
- **Batch 28** (UnityTuning.Ui.cs, the title and the round counter):
  - Symbols: `WordmarkY`, `TaglineY`, `TaglineSpacing`, `RoundCounterSize`, `RoundMarkHeight`, `RoundNumeralBox`, `RoundCounterBottom`,
    `RoundLabelGap`, `RoundLabelHeight`, `TallyStrokeWidth`.
  - Removed: 10 lines. Compile: 0 / 0 / 0 / 0. Disk after: 4.6 GB.
  - Orphan comment: the tagline comment (2 lines).
- **Batch 29** (UnityTuning.Ui.cs, the tally strokes and the burn-in):
  - Symbols: `TallyStrokeHeight`, `TallyStrokeGap`, `TallyTiltDeg`, `TallySlashLength`, `TallySlashDeg`, `TallyStrokeTime`,
    `RoundBurnTime`, `RoundBurnHold`, `RoundFlyTime`, `RoundBurnScale`.
  - Removed: 10 lines. Compile: 0 / 0 / 0 / 0. Disk after: 4.6 GB.
  - Orphan comment: the tally strokes comment (2 lines).
- **Batch 30** (UnityTuning.Ui.cs, the end of the burn-in, the points punch, the plates):
  - Symbols: `RoundBurnPop`, `RoundBurnRise`, `RoundCoolTime`, `PointsPunchTime`, `PointsPlatePadLeft`, `PointsPlatePadRight`,
    `RankPlatePadRight`, `RankPlateY`, `RankPlateWidth`, `RankPlateHeight`.
  - Removed: 10 lines. Compile: 0 / 0 / 0 / 0. Disk after: 4.6 GB.
  - Orphan comments: the burn-in comment (4 lines) and the rank box comment (2 lines).
- **Batch 31** (UnityTuning.Ui.cs, RESULTS' column and tally rows):
  - Symbols: `ResultsColumnX`, `ResultsColumnWidth`, `ResultsBigSize`, `TallyOutcomeY`, `TallyOutcomeSpacing`, `TallyRowY`, `TallyRoundX`,
    `TallyRoundWidth`, `TallyLabelHeight`, `TallyLabelSpacing`.
  - Removed: 10 lines. Compile: 0 / 0 / 0 / 0. Disk after: 4.6 GB.
  - Orphan comment: the column comment (2 lines).
- **Batch 32** (UnityTuning.Ui.cs, the tally's grade, cards and stamp):
  - Symbols: `TallyGradeX`, `TallyGradeBox`, `TallyHeatGap`, `TallyCardsY`, `TallyCardWidth`, `TallyCardHeight`, `TallyCardSpacing`,
    `TallyCardPad`, `TallyStampY`, `TallyStampWidth`.
  - Removed: 10 lines; TallyHeatWidth / TallyHeatHeight between them stay. Compile: 0 / 0 / 0 / 0. Disk after: 4.6 GB.
- **Batch 33** (UnityTuning.Ui.cs, the tally's stamp, details, buttons and grade ink):
  - Symbols: `TallyStampHeight`, `TallyStampBorder`, `TallyStampLineGap`, `TallyDetailsY`, `TallyDetailsHeight`, `TallyLineY`,
    `TallyRestartY`, `TallyButtonGap`, `GradeInk` (a static readonly Color), `GradeSkewDeg`.
  - Removed: 10 lines; TallyStampTiltDeg / TallyDetailsLineSpacing stay. Compile: 0 / 0 / 0 / 0. Disk after: 4.6 GB.
  - Orphan comment: the `.grade` comment (1 line).
- **Batch 34** (UnityTuning.Ui.cs, the tally's beats):
  - Symbols: `DangerDimAlpha`, `TallyRoundAt`, `TallyCardAt`, `TallyCardGap`, `TallyCardTime`, `TallyGradeAt`, `TallySlamFrom`,
    `TallySlamTime`, `TallyShakeTime`, `TallyShakeHz`.
  - Removed: 10 lines. Compile: 0 / 0 / 0 / 0. Disk after: 4.6 GB.
  - Orphan comment: the `--danger-dim` comment (1 line).
- **Batch 35** (UnityTuning.Ui.cs, UnityTuning.Ui43.cs):
  - Symbols: `TallyShakePx`, `TallyBestAt`, `TallyLineAt`, `TallyDoneAt`, `TallyFadeTime`, `Screens43FocusScale`, `Screens43RankPx`.
  - Removed: 7 lines. Compile: 0 / 0 / 0 / 0. Disk after: 4.6 GB.
  - The fade comment above TallySkipGuard still names "fades in over this many s".
- **Batch 36** (UnityTuning.cs):
  - Symbols: `BloomScatter`, `OverlayRefresh`, `UsableDimAlpha`, `StaminaSegmentHeight`, `StaminaSegmentGap`, `SettingsWidth`,
    `SettingsRowHeight`, `SettingsTabWidth`.
  - Removed: 8 lines. Compile: 0 / 0 / 0 / 0. Disk after: 4.6 GB.
  - Orphan comments: the overlay refresh (1 line), `--usable-dim` (2), stamina segments (1) and SETTINGS layout (1) comments.

## D1 totals and what is still owed
- **Totals:** 35 commits on `n61-D1` (e7ffdc6c .. 0918e196): batches 1-7 with 1b and 2b, then 11-36.
  - `git diff --shortstat 0ab87de8..HEAD -- Assets`: 52 files, 27 insertions (rewritten declarator lines), 397 deletions. Net 370 lines
    removed, equal to the per-batch sum above.
  - No file under Assets/Scripts/Core, no Mover.cs and no test was touched (`git diff --name-only` filter: 0).
- **Skipped, as the lead said:** batches 8-10, 37-51 and 52-59. No listed entry had to be skipped for a re-grep hit or a "for GG" row:
  every re-grep found only the symbol's own declaration and cascade, and no constant in 11-36 belongs to an OFF row on the list.
- **Gates**, once, after the last batch (`bash Tools/night/n61-gates.sh --no-editor` on 0918e196): `gates: all GREEN 13:51:15`.
  - Config copies identical; golden 39 sets (48094 records), 22 replays exact, 0 mismatches; fingerprint plain and --off57..--off61 EQUAL.
  - The fingerprint runs rebuild SimHarness, which compiles SurvivalSim*.cs, Bot/*.cs and UnityTuning.Hud38.cs / Kit33.cs. So batches
    5, 6, 20 and 23 also built there, and the sim's hashes did not move.
- **Not checked (for the lead):**
  - the Editor compile of the picks;
  - the touched test batches the auditor names: batch 1 Ui43Tests, Hud30Tests, FirstFifteenVerbsTests, Arcline22Tests; batch 2 Health42Tests,
    Style42Tests; batches 3-4 Stamp22Tests; batch 6 RunSnapshotTests, FingerprintTests; batch 7 MenusTests, MenuMotion18Tests; 11-36 the
    Editor compile only;
  - verbs37 at the next build;
  - the smoke's TITLE step after batch 7.
- **Follow-ups, not done (each would edit lines the list did not name):**
  - the orphan comments listed per batch;
  - two double blank lines (Ui.cs where AssistTexts / AppendHex2 were; FeelReel.N26.Bosses2.cs);
  - doc comments that still name a removed value (Crown37.cs:25, Glacier26.cs's spike doc, Loom37.cs:19, the tally fade comment).
- **Disk:** chk-D was deleted after every compile from batch 22 on, and is deleted now. Free space on C: was 4.6 GB at the end
  (`df -h /c`), never under 4 GB.

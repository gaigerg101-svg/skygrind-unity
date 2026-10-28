# Audit: controller, rebinding, hold / toggle, frame pacing, quality presets (web → Unity)

2026-10-04. A read-only audit for planning. Web: `C:\Users\gaige\dev\skygrind` at `p17-night6` (branch `night6`). Unity:
`port/stage3` at `u3-n12` (07f7885). I ran, built and tested nothing. Line numbers are as read today. Anything I didn't
read is marked "not checked".

## 1. Bottom line

- **Unity has keyboard and mouse only, and every key is hard-coded.** Movement keys are at `InputReader.cs:50-54` and
  `:65-71`; combat keys at `CombatReader.cs:26-37` and `:55-59`. The only gamepad read in the project is the RESULTS tally
  skip (`UiResults.cs:368-369`). There is no rebinding, no pad, no hold / toggle and no CONTROLS screen
  (`PORT-STATUS.md:35`, `:37`, `:49`).
- **The data is already exported.** `PortExport/config.json` has:
  - the `gamepad` block (8 keys)
  - all 41 web settings, including the 8 pad keys, `slideMode` / `arclineMode`, `fpsCap`, `quality` and `renderScale`
  - `input.bindings` (18 actions) with their labels
  - `ui.controlGroups` / `mouseControls` / `techniques` / `hudLegend`
  - `ui.notices.padOn` / `padOdd`
  - `quality.presets` / `dynamic`

  The store already loads and saves every pad key (`Settings.cs:195`). They are only hidden (`UiSettings.cs:16-26`) and
  not applied (`GameSettings.cs:78-94`). config.json lacks only the web's **module constants** (section 5).
- **The pure parts port cleanly to Core:**
  - `gamepad.js` (249 lines, all pure)
  - the pure half of `bindings.js` (`:8-48`)
  - `holdToggle.js` (49), `framePace.js` (25) and `qualityMath.js` (67)
  - the pure helpers in `menus.js`

  That is **47 web test cases to port**: gamepad 20, bindings 6, holdToggle 6, framePace 4, quality 6, menuFocus 3 of 5,
  prompts 2 of 3.
- **Replays and golden vectors never see this layer.** The 16 + 6 replays build their InputFrames from recorded rows
  (`PortData.cs:249`, `:295`), not through `InputReader`. "With no pad, the frame is bit-identical" is a live-game
  property, kept by three defaults: `mergePad` does nothing for no pad or a pad at rest, HOLD does nothing, and the
  breakaway cap defaults to +∞. The only Core edit to existing code is the aim assist's cap (`AimAssist.cs:567`).
- **Most of the Unity-only work is in three places:**
  - **uGUI pad navigation.** Buttons are built in code with the default navigation and no visible selected state
    (`Ui.cs:586-602`, `UiSettings.cs:154-168`). Only RESULTS ever selects a button (`Ui.cs:868`).
  - **CONTROLS with key capture.**
  - **A web key-code ↔ Unity `Key` table.** Today's `KeyOf` turns `ShiftLeft`, the dash default, into `Key.None`
    (`SurvivalGame.cs:672-676` against `Keyboard.cs:567` `LeftShift`).
- **Frame pacing is already mapped:** FPS CAP drives `targetFrameRate`, and VSYNC is Unity-only (`SurvivalGame.cs:662-666`).
  **Quality is half there.** Bloom, SMAA, grain, lights and motes follow the preset (`LightAndLens.cs:49-60`, `:82-89`).
  Still missing: the AUTO benchmark, dynamic resolution, pixel ratio, shadows and the particle cap (`LightAndLens.cs:8-9`).
- **Size: about 2,300 lines in total:**

  | Part | Lines |
  |---|---|
  | Core | ~650 |
  | Core tests | ~600 |
  | Game | ~550 |
  | UI | ~550 |

  That fits one night as four worktree agents split by file ownership: Core input, Core quality, Game glue, UI. This
  matches `PORT-PLAN.md:128` (N19, ~2,300) and `tasks.json:471-473` (U3-22).

## 2. The web's input stack

| File (lines) | Pure | Browser-bound | Tests (cases) |
|---|---|---|---|
| `src/input/gamepad.js` (249) | All of it (it takes Gamepad-like objects, `:1-6`); the parts are listed below | none | `tests/gamepad.test.js` (20) |
| `src/input/bindings.js` (106) | `UNBINDABLE` `:8`, `keyName` `:11-17`, `fillKeys` `:25-27`, `rebind` (a key in use swaps) `:34-41`, `conflicts` `:44-48` | `createBindings` `:53-106`: localStorage `skygrind.bindings.v1` (`:6`, `:57`, `:67`); a saved map is checked against UNBINDABLE and conflicts, else the defaults (`:59-60`) | `bindings.test.js` (6); `prompts.test.js` (3) |
| `src/input/holdToggle.js` (49) | All: `createToggleState` `:10-12`; `applyToggles` `:22-49` | none | `holdToggle.test.js` (6) |
| `src/input/keyboardMouse.js` (411) | none | Everything; the parts are listed below | none |
| `src/core/framePace.js` (25) | `paceFrame` `:18-25` (`SLACK_MS` 1 ms, `:9`) | none | `framePace.test.js` (4) |
| `src/fx/qualityMath.js` (67) | `percentile` `:8-15`, `keepingUp` `:21-23`, `pickQuality` `:26-30`, `presetPixelRatio` `:33-36`, `stepRenderScale` `:45-67` (Math.round at `:53`, `:58`, `:63`) | none | `quality.test.js` (6) |
| `src/fx/quality.js` (120) | The state machine: Float32Array ring `:14-15`, `startBenchmark` `:68-73`, `update` `:86-117` | `window.devicePixelRatio` `:57`; the post / particles callbacks `:30-36` | none |
| `src/ui/settings.js` (68) | `migrateSettings` `:19-32` | localStorage `:39-68` | **Already ported** (`Core/Settings.cs:68-86`, `:179-282`; `SettingsTests.cs`, 6 tests) |
| `src/ui/menus.js` (1168) | `controlRows` `:101-110`, `primaryIndex` `:119-123`, `tabDefaults` `:132-134`, `keyGrid` `:137-140` | The rest; listed below | `menuFocus.test.js` (5) |

**gamepad.js, its parts:**
- the tables: `PAD_BUTTONS` `:9-11`, `DEFAULT_PAD_LAYOUT` `:19-36`, `FRAME_BUTTONS` `:39`, `PAD_GLYPHS` `:42-45`,
  `TRIGGER_DOWN` 0.35 `:48`
- `buttonDown` `:51-55`; `radialDeadzone` `:62-68` (Math.hypot, rescaled); `responseCurve` `:71-74` (Math.sign)
- `readSticks` `:83-96`: up is −1 on the stick, so `moveZ = -my`, and the yaw is `-lookX·yawRate·sensX`
- `heldActions` `:102-108` (a new Set on every call); `pressedSince` `:111-115`; `buttonFor` `:118-121`
- `padActivity` `:124-128` (skips button 16)
- `menuNav` `:138-166`: vertical wins; a held direction repeats after `delay`, then every `every`
- `mergePad` `:180-189`: an axis adds and clamps only when it isn't zero; buttons OR in; presses latch
- `PAD_ACTION_LABELS` `:192-196`; `padRows` `:199-203`
- `rebindPad` `:213-224`: the two buttons swap their whole action lists; `readPadLayout` `:227-242`: a stored layout
  missing an action in `REQUIRED` (`:206`) falls back to the default
- `rawPresses` `:245-249`

**holdToggle.js:** SLIDE toggle (`:23-27`): a press flips it and a jump clears it. ARCLINE toggle (`:29-47`) reads the
rope phase; the press that lets go is swallowed (`:35`).

**keyboardMouse.js, its browser-bound parts:**
- DOM listeners `:146-243`; pointer lock `:87-114`
- `sample()` `:286-302`: latches are consumed per step; the pad merges only when it is connected and in game (`:299`);
  pad latches clear after the merge (`:300`)
- `pollPad` `:344-409`, once a frame:
  - `navigator.getGamepads()`, the first pad connected (`:345-347`)
  - the device switch (`:365`); the sticks from the settings (`:368-371`); menu navigation (`:372`)
  - `input:padButton` (`:373-375`)
  - out of game: everything clears (`:380-385`)
  - look at frame rate × `lookScale` × dt (`:391-396`)
  - the commands: weapon cycle and quick swap, lockOn, xray, perkList, pause (`:397-408`)
- the device rules: keydown (`:160`), mousedown (`:204`), a mouse move over 2 px (`:220`)
- `rumble` `:317-329`: `vibrationActuator`, only with VIBRATION on and the pad in use

**menus.js, its browser-bound parts:**
- CONTROLS `:505-558`: key capture `:548-558`; pad capture `:515-522`; the GAMEPAD table only while a pad is connected
  `:534-537`
- keyboard navigation `:756-791`: arrows and Tab move, left / right switch the settings tab, Esc goes back, the RESTART
  key
- the pad helpers: `padTargets` / `padFocusOn` / `padChange` `:888-919`
- `padInput` `:932-968`: the focus walks every control; left / right change sliders and lists; A presses; B goes back
  or resumes; START resumes
- `show()` focuses `primaryIndex` `:1013`; the pause key list follows the device `:1001-1003`; the CSS focus ring on the
  pad `:204-207`

**What each test covers:**
- **gamepad.test.js:**
  - dead zone and curve `:19-43`
  - sticks: direction, separate X / Y sensitivity, invert Y, a centred stick reads zero `:47-65`
  - the table: coverage, triggers past 0.35, edges, custom layouts `:69-93`
  - hot-swap activity and menu repeat / confirm / back `:97-120`
  - **the keyboard path untouched: no pad, a pad at rest, a pad adding on top** `:132-154`
  - remap and stored-layout checks; raw presses `:158-182`
- **bindings.test.js:** no default conflicts `:8`; a free key moves `:12`; swaps `:19`; Ctrl / Meta / Esc refused `:26`;
  the RESTART swap `:29`; key names `:40`.
- **prompts.test.js:** `fillKeys` `:12`; the BUY prompt `:17`; Training signs `:25` (needs the Training map: not ported).
- **holdToggle.test.js:** HOLD leaves the frame alone `:15`; slide toggle `:26`; a jump clears it `:37`; ARCLINE release
  `:46`; re-hook after the rope is gone `:58`; a missed hook `:68`.
- **framePace.test.js:** OFF `:19`; holds the cap `:23`; no catch-up burst `:31`; cap ≥ display `:37`.
- **quality.test.js:** percentile `:9`; AUTO picks `:21`; pixel ratios `:29`; an undone drop and the lockout `:39`;
  repeated drops `:51`; hold times, floor and climb `:58`.
- **menuFocus.test.js:** `primaryIndex` `:7`, `:12`; `readLastPlay` `:20` (out of scope); `tabDefaults` `:34` (already
  ported as `SettingsTests.cs:67`); `keyGrid` `:48`.

**How main.js wires it:**
- bindings and the pad-layout store `skygrind.padLayout.v1`: `:185-205`
- the menus' hooks (`promptKey`, `getPadRows`, `padBinds`): `:228-250`
- the aim assist per device: SNAP MODE (PAD) and the breakaway cap `yawRate × padSensX × breakawayShare` (`:377-379`);
  the stick also gets the look friction (`:383`)
- toggles on your own input only (`:1796-1811`), reset each run (`:853`)
- pad pause / resume without the mouse (`:1312-1329`); `padConnected` / `device` / rumble (`:1331-1340`)
- `keyLabel`, a pad glyph or a key (`:1432-1440`); the cursor hidden for a pad game (`:2178-2184`)
- the FPS CAP (`:2431-2434`)
- `pollPad` before the steps, with menu input outside play (`:2444-2447`)
- quality: `update` `:2469`, `startBenchmark` on TITLE `:1176`, `resetWindow` `:1372`, `applyPreset` `:1654-1680`

**config.js:**

| What | Lines |
|---|---|
| `gamepad` | `:1263-1269` |
| pad settings | `:2159-2166` |
| `slideMode` / `arclineMode` | `:2150-2151` |
| `fpsCap` | `:2154` |
| settings rows | `:2286-2287`, `:2294`, `:2307-2318` |
| tabs | `:2187-2190` |
| `quality` | `:2413-2443` |
| `input` | `:1182-1194` |

**Docs:**
- `docs/GAMEPAD.md` (102 lines): the layout `:16-35`; the settings `:49-57`; SNAP as the pad default, the 80% breakaway
  cap and look friction on the stick `:61-64`; hot-swap `:68-76`; caveats `:80-92`.
- `docs/controls.md` (47 lines) is the printable keyboard card.

## 3. Unity today

**Input flow.** `SurvivalGame.Update` (`:971-985`) runs in Playing and Down only (`:977`):
1. It sets `input.sensMult` to ADS × `assist.LookScale` (`:979`).
2. It calls `InputReader.Poll` (`:980`) and `CombatReader.Poll` (`:981`).
3. If the cursor came unlocked, it pauses (`:983`).
4. `HotKeys` (`:1382-1412`) reads F3 / F8 / F9, the restart and lock-on keys from config (`:601-602`), Esc, and the
   draft's 1-3.

Then it computes `frameDt` (`:992`) and runs the accumulator (`:999-1010`). Each `SimStep` (`:1163`) does
`input.Sample` (`:1167`; `forcedMove` `:1169-1175`), `mover.Step` (`:1180`), `combatReader.Sample` (`:1191`), and the
assist turning the look (`:1198-1200`).

**The readers:**
- **`InputReader.cs` (83 lines):** mouse look every frame (`:44-49`), using the **float** `Mathf.Deg2Rad` (`:46`). The
  keys are hard-coded (`:50-54`, `:65-71`); latches clear per step (`:73`); `Locked` is the cursor lock state (`:20`); a
  click locks and Esc unlocks (`:28-43`).
- **`CombatReader.cs` (67):** left / right mouse, B, F, E, digits 1-9, the wheel, Q (`:26-37`); `Sample` `:48-65`.
- **Other keyboard reads:** the X-ray key in `EnemyViews.cs:124`, `:323`; the tally skip in `UiResults.cs:362-369`. The
  Arena scene drives the same `InputReader` (`ArenaBootstrap.cs:105`, `:138`, `:169`).

**Frame structs.**
- `InputFrame` (`Mover.cs:25-37`): `moveX`, `moveZ`, `yaw`, `pitch`, `jump`, `dash`, `slide`, `grapple`, `reel`, each a
  `Button {held, pressed}` (`:11-15`), plus `grappleTarget`.
- `CombatInput` (`Combat.cs:154-162`): `fire`, `altFire`, `special`, `punch`, `interact`, `weapon`.

The web keeps all of this in one frame (`mover.js:37-53`), so `MergePad` must write both structs. Analog walking is
already exact: the wish speed scales by `min(1, hypot)` (`Mover.cs:338`). The rope phase that holdToggle needs is public
(`Mover.cs:95`, `:139`).

**Settings.** The store holds **49 values**:
- the 41 `config.settings` keys
- VSYNC, the FIRST-RUN GUIDE and 6 LAB toggles (`GameSettings.cs:25-34`)

SETTINGS shows **29 rows**: 21 of the web's 41, plus VSYNC, the guide and the 6 LAB toggles (`UiSettings.cs:16-26`;
`UiOnboarding.cs:23`). `PORT-PLAN.md:102` says "22 of 41", which probably counts VSYNC. A tab shows only when it has a
shown row (`UiSettings.cs:66-69`), so **the GAMEPAD tab is hidden**. Not shown:
- `slideMode`, `arclineMode`, `adsMode`, `renderScale`, `showFps`
- the 8 pad keys
- `uiScale`, `gunFinish`, `hudStyle`, `shapeCues`, `auraIntensity`, `showGhost`, `duelRounds`

Rows are a `-` and a `+` button around a value text (`UiSettings.cs:104-106`); `Nudge` and `Cycle` exist
(`Settings.cs:108-123`).

**UI navigation.**
- The EventSystem and an `InputSystemUIInputModule` are made at run time (`Ui.cs:191-192`) with no actions assigned, so
  `OnEnable` assigns `DefaultInputActions` (`InputSystemUIInputModule.cs:1645-1647`, `:1541-1557`).
- In those defaults:
  - Navigate is both pad sticks, the d-pad, WASD and the arrows.
  - Submit is any `{Submit}` usage: pad south, Enter (`Gamepad.cs:118`, `Keyboard.cs:55`).
  - Cancel is pad east or Esc (`Gamepad.cs:121`, `Keyboard.cs:53`).
- The module repeats a held direction after 0.5 s, then every 0.1 s (`:950`, `:955`); the web uses 0.35 / 0.12
  (`config.gamepad`).
- `UnityEngine.UI.Button`s are added in code with the default (Automatic) navigation and no `selectedColor`
  (`Ui.cs:591-597`, `UiSettings.cs:158-163`, `UiMods.cs:28`). Pad focus therefore has no visible state.
- Selection is set only on RESULTS (`Ui.cs:868`, `UiResults.cs:186`, `:301`), so today a pad's A presses RESTART there
  and nowhere else.

**Cursor.** It locks on StartRun and Resume (`SurvivalGame.cs:835-836`, `:965-966`) and frees on ToTitle, EndRun and
Pause (`:725-726`, `:908-909`, `:957-958`). Every reader gives a neutral frame while unlocked (`InputReader.cs:28`,
`:61`; `CombatReader.cs:25`, `:53`).

**Active input handling.**
- `activeInputHandler: 1`, the Input System only (`ProjectSettings.asset:930`); `com.unity.inputsystem` 1.18.0
  (`manifest.json:11`).
- The project-wide actions are the template's `Assets/InputSystem_Actions.inputactions` (`EditorBuildSettings.asset:22`,
  guid 052faaac…). No script uses them.
- There is no InputSettings asset, so the defaults apply (`InputSettings.cs:743`, `:752-753`, `:758`):
  - events are processed in dynamic update
  - sticks get a dead zone of 0.125-0.925
  - buttons count as pressed at 0.5

**Aim assist.**
- The mode comes in through a `Func<string>` (`AimAssist.cs:96`, `:105`), fed by `settings.aimAssist`
  (`SurvivalGame.cs:730`).
- The breakaway uses `p.breakaway` alone (`AimAssist.cs:567`). The file header lists "the gamepad's own mode and
  breakaway cap" as not ported (`:20-21`).
- Friction exists (`LookScale` `:512`) and already scales the mouse (`SurvivalGame.cs:979`): reuse it for the stick.

**Frame cap and quality.**
- `ApplyFrameCap` (`SurvivalGame.cs:662-666`) sets vSyncCount to 1 or 0, and targetFrameRate to the cap only when vsync
  is off and the cap is over 0 (else −1). The VSYNC row is `GameSettings.cs:29` (default off, `UnityTuning.cs:60`).
- Quality: `LightAndLens.cs:29-30`, `:78-89`. AUTO draws HIGH (`:84`).
- `PC_RPAsset` is fixed at render scale 1, a 2048 shadow map and a 60 m shadow distance (`PC_RPAsset.asset:29`, `:46`,
  `:57`).
- There are two quality levels, Mobile and PC; PC is current, and vSyncCount is 0 on both (`QualitySettings.asset:7`,
  `:10`, `:64`, `:86`, `:104`).

## 4. The port plan

| # | Piece | Web source | Unity target | Size | Check |
|---|---|---|---|---|---|
| 1 | Core: the pad maths, table, menu navigation, MergePad, remap | `gamepad.js:1-249` | `Core/Gamepad.cs` (new). A `PadState` struct (17 buttons as {pressed, value}, 4 axes, the web's standard indices); actions as an enum plus a bitmask (no Set per poll; edges in layout order, PORT-RULES §4); `JsMath.Hypot` / `Min` / `Max`; a JS `Math.sign` (one is private at `CombatMath.cs:279`) | ~320 | `gamepad.test.js`'s 20 cases (`:132-154` compared bit for bit with `DoubleToInt64Bits`); a new `vectors/gamepad.json`; a ZeroAllocTests line for `MergePad` |
| 2 | Core: hold / toggle | `holdToggle.js:1-49` | `Core/HoldToggle.cs` (new); the rope phase as `RopePhase` | ~70 | `holdToggle.test.js` (6); `vectors/holdToggle.json` (U3-22 asks for both sets) |
| 3 | Core: bindings table and store | `bindings.js:8-48`, `:53-106` | `Core/Bindings.cs` (new): a store like `SettingsStore`, keeping the web codes and the key `skygrind.bindings.v1` | ~170 | `bindings.test.js` (6), `prompts.test.js:12`, `:17` (needs a Core `buyLabel`, or skip `:17`) |
| 4 | Core: frame pace | `framePace.js:18-25` | `Core/FramePace.cs` (optional; see section 7) | ~35 | `framePace.test.js` (4) |
| 5 | Core: quality maths and state | `qualityMath.js:8-67`, `quality.js:12-120` (DPR passed in) | `Core/QualityMath.cs`, `Core/Quality.cs` (new); the Float32Array ring stored as float (PORT-RULES §2) | ~220 | `quality.test.js` (6); `vectors/quality.json` |
| 6 | Core: menu helpers | `menus.js:101-110`, `:119-123`, `:137-140` | `Core/MenusMath.cs` (new); `keyGrid` returns rows, not HTML | ~60 | `menuFocus.test.js:7`, `:12`, `:48` |
| 7 | Core: the pad's breakaway cap | `aimAssist.js:39`, `:434` | `AimAssist.cs`: an optional `Func<double>` cap (default +∞); `:567` becomes `JsMath.Min(p.breakaway, cap())` | ~8 | AssistGoldenTests; **all golden and 22 replays at this commit** |
| 8 | Game: key codes | `KeyboardEvent.code` | `Game/KeyCodes.cs` (new): a table both ways (`ShiftLeft`↔`LeftShift`, `ArrowUp`↔`UpArrow`, `Equal`↔`Equals`, ...); replaces `SurvivalGame.KeyOf` `:672-676` | ~90 | A Game test: every `config.input.bindings` default maps to a Key that isn't None |
| 9 | Game: the readers on live bindings | `keyboardMouse.js:131-143`, `:286-302` | `InputReader.cs:50-54`, `:65-71`; `CombatReader.cs:26-37`, `:55-59`; `HotKeys` `:1396-1398`; `EnemyViews.cs:124` | ~120 | The Editor smoke is unchanged; a Play check rebinds jump |
| 10 | Game: the pad reader | `keyboardMouse.js:68-78`, `:317-329`, `:344-409` | `Game/PadReader.cs` (new): fills `PadState` from `Gamepad.current` (raw sticks, y negated); the device switch; look × `sensMult` × frameDt; latches to the next step; commands go to the slot request, `assist.ToggleLock`, the X-ray toggle and Pause; the `input:*` events (`AUDIO-HOOKS.md:157-158`) | ~230 | A Play check on a virtual pad (`InputSystem.AddDevice<Gamepad>()` + `InputState.Change`; no new package) |
| 11 | Game: SurvivalGame wiring | `main.js:378-379`, `:853`, `:1312-1340`, `:1807-1811`, `:2444-2447` | `SurvivalGame.cs`: poll in every flow (`:977` is Playing / Down only); frameDt before Poll (`:992`); `ApplyToggles` between Sample and Step (`:1167-1180`, not on `forcedMove`), reset at StartRun; mode and cap per device; rumble on hurt, kill and parry | ~90 | Smoke, plus the virtual-pad Play check |
| 12 | Game: settings applied | `settings.js` (live `config.settings`) | `GameSettings.Apply` `:78-94` | ~20 | `SettingsTests` round trip |
| 13 | UI: rows shown | `config.js:2286-2287`, `:2311-2318` | `UiSettings.cs:16-26` Shown += `slideMode`, `arclineMode`, the 8 pad keys (the GAMEPAD tab appears) | ~10 | A capture |
| 14 | UI: CONTROLS | `menus.js:505-558`, `:396`, `:402` | `Game/UiControls.cs` (new): the `controlRows` groups; key capture (poll `Keyboard.allKeys`; Esc cancels; the swap and "CAN NOT BE BOUND" notes); the GAMEPAD table + RESET PAD with a pad connected; the binding press swallowed (`:514`, `:934-937`); a CONTROLS button on TITLE and PAUSED | ~320 | A capture; a Play check of a rebind and a conflict swap |
| 15 | UI: pad navigation | `menus.js:885-968`, `:1013`, `:1001-1003`, `:204-207` | `Game/UiNav.cs` (new): one Selectable per settings row (left / right call `Nudge` / `Cycle`); explicit vertical navigation; a cyan selected state; `primaryIndex` selected when each screen opens; B = back / resume, START = resume; the pause key list per device | ~260 | Virtual-pad Play check: open SETTINGS, change a slider, back |
| 16 | UI: prompts | `main.js:1432-1440`, `bindings.js:25-27` | The hard-coded title line `Ui.cs:346`; `:362-366`; `"HOLD E"` `:962`; `SurvivalGame.KeyLabel` `:218-225` | ~60 | A capture on each device |
| 17 | Game: quality | `quality.js`, `main.js:1176`, `:1372`, `:1654-1680` | A Game driver on a runtime copy of `PC_RPAsset` (section 7) | ~160 | The bench's preset is logged (section 6) |

**Keeping "no pad = the keyboard's frame, bit for bit".**
1. Add no field to `InputFrame` and don't touch `Mover.cs` (CLAUDE.md rule 3).
2. `MergePad` returns early for no pad and changes an axis only when the pad's value isn't zero
   (`gamepad.js:181-183`). Port `gamepad.test.js:132-145` with a bitwise compare of every field of both structs.
3. HOLD is the default and a no-op (`holdToggle.test.js:15-24`).
4. The device starts as keyboard, so the assist mode stays `aimAssist`. The cap defaults to +∞, and
   `JsMath.Min(x, +∞)` is `x`.
5. Pad look is added only when the rate isn't zero (`keyboardMouse.js:391`).

The golden checks and the replays don't pass through the readers (`PortData.cs:249`, `:295`), so only item 7 of the
plan touches what they check: run them at that commit.

## 5. Unity-only additions (the web can't have these)

**Device APIs for the browser-only parts:**

| Web | Unity |
|---|---|
| The first connected pad (`keyboardMouse.js:345-347`) | `Gamepad.current` (the last used); `InputSystem.onDeviceChange` (`InputSystem.cs:1120`) for `input:padConnected` |
| `mapping === 'standard'` (`:362`) | Any `Gamepad` layout is already standard. An unknown HID pad appears as a `Joystick`, not a `Gamepad`: show `padOdd` for that |
| `buttons[i].value` | `ButtonControl.ReadValue()`, with the web's 0.35 (`gamepad.js:48`), not `isPressed` (press point 0.5, `InputSettings.cs:758`) |
| `axes[0..3]` | `leftStick` / `rightStick.ReadUnprocessedValue()` (`InputControl.cs:1528`), because the layout adds `stickDeadzone` (`Gamepad.cs:138`, `:149`). Negate y: up is + in Unity, −1 on the web (`gamepad.js:87`) |
| `vibrationActuator` dual-rumble (`keyboardMouse.js:323`) | `SetMotorSpeeds(low = strong, high = weak)` (`Gamepad.cs:799`), plus a frame-time timer that sets 0 after `ms`. Stop it on pause, focus loss and quit |
| Pointer lock needs a click (`keyboardMouse.js:87-114`; `main.js:1265-1267`, `:1777`, `:2178`) | `Cursor.lockState` can be set at any time. Keep the cursor locked while a pad plays, and log that the web's unlocked pad play was simplified |
| Rebind capture (`rawPresses` `gamepad.js:245`; keydown `menus.js:548`) | `rawPresses` on `PadState`; for keys, `Keyboard.current.allKeys` with `wasPressedThisFrame` while waiting (or `InputSystem.onAnyButtonPress`, `InputSystem.cs:2449`, which allocates per subscription) |
| The last-used device (`keyboardMouse.js:74-78`, `:160`, `:204`, `:220`) | Keep the web's rules (a key, a click or a move over 2 px means keyboard; `padActivity` over 0.5 means pad). Don't use PlayerInput control schemes |

- **Steam Deck.** Under Steam Input the Deck's controls should appear as a virtual Xbox pad (`XInputControllerWindows`,
  `XInputController.cs:51`, under Proton). The A / B / X / Y set fits it. **Not checked** on a Deck. Steam's own glyphs
  need Steamworks, and D-30 allows no new packages until N16.
- **Glyph sets.** The web has one neutral set (`gamepad.js:42-45`). Unity can tell the device types apart:
  - Xbox: `XInputController`
  - PlayStation: `DualShockGamepad` (`DualShockGamepad.cs:15`), `DualSenseGamepadHID` (`DualShockGamepadHID.cs:337`)
  - Nintendo: `SwitchProControllerHID` (`SwitchProControllerHID.cs:150`)

  Two catches:
  - **The PlayStation shapes (✕ ○ △) aren't in the static TMP atlases' charset** (`Ui.cs:6-11`, `HANDOFF.md:231`). Use
    words (CROSS / CIRCLE / SQUARE / TRIANGLE) or sprites. The tables go in `UnityTuning.Ui.cs`.
  - **On a Switch Pro pad, Unity flips confirm and back.** Its bottom button is `buttonSouth` with Back / Cancel usages
    (`SwitchProControllerHID.cs:53`), so the default UI Submit and Cancel swap. The web goes by position
    (`GAMEPAD.md:80-84`).
- **uGUI.**
  - Repeat: either set `moveRepeatDelay` / `moveRepeatRate` from `gamepad.menuDelay` / `menuEvery`, or drive selection
    from the ported `menuNav` and clear the module's `move` action.
  - Take WASD out of UI Navigate (the web navigates with arrows and Tab only, `menus.js:770-775`).
  - Give every Selectable a visible selected state.
- **What `config.json` lacks:** only the web's module constants:
  - `PAD_BUTTONS`, `DEFAULT_PAD_LAYOUT` (16 buttons), `FRAME_BUTTONS`, `PAD_GLYPHS`, `PAD_ACTION_LABELS` (17),
    `REQUIRED`, `TRIGGER_DOWN` 0.35 (`gamepad.js:9-48`, `:192-206`)
  - `UNBINDABLE` and the key names (`bindings.js:8`, `:15`)
  - `SLACK_MS` (`framePace.js:9`)
  - the 2 px mouse threshold (`keyboardMouse.js:220`)
  - `PAD_MOVE` (`main.js:1432`)

  Either port them as cited C# constants (Core already keeps named constants with comments, e.g. `EnemyCrowd.cs:157-160`;
  CLAUDE.md rule 5 makes it a lead call), or export them into a new untracked `port-export-p21/` with the vectors.
- **Unity-only numbers:**
  - In `UnityTuning`: the glyph texts; the pixel-ratio map (there's no `devicePixelRatio`; section 7); the storage key
    `skygrind.padLayout.v1` (`main.js:188`, keep the web's name).
  - In `Design/unity-config.json` with a `_why`, if GG wants them: F3 / F8 / F9 added to UNBINDABLE (the port's debug
    keys, `SurvivalGame.cs:1386-1395`).

## 6. Risks and gotchas

1. **HIGH, a double dead zone.** `Gamepad.leftStick.ReadValue()` already applies 0.125-0.925 (`InputSettings.cs:752-753`).
   Add the web's 0.15-0.95 on top and the stick feels dead and off. Read unprocessed values.
2. **HIGH, `KeyOf("ShiftLeft")` is `Key.None`** (`SurvivalGame.cs:672-676`; Unity calls it `LeftShift`,
   `Keyboard.cs:567`). Generalizing the readers on `KeyOf` silently unbinds dash. Build the table (plan item 8) first.
3. **HIGH, pad menus get no input.** Input is polled only in Playing / Down (`SurvivalGame.cs:977`); the web polls in
   every state (`main.js:2444-2447`). TITLE, PAUSED, SETTINGS and RESULTS need the pad poll.
4. **The cursor-lock gate.** Every reader is neutral while unlocked (`InputReader.cs:28`, `:61`; `CombatReader.cs:25`)
   and unlocking pauses (`SurvivalGame.cs:983`). A pad Pause / Resume must go through `Pause()` / `Resume()`
   (`:952-967`), which already lock and free the cursor. Whether a gamepad in the Editor ignores Game-view focus: **not
   checked**.
5. **The update mode against the 120 Hz loop.** Events are processed in dynamic update before `Update`
   (`InputSettings.cs:743`), so there is one pad sample per rendered frame, latched into the coming steps: the web's
   model (`keyboardMouse.js:283-301`). Keep it:
   - Don't switch to fixed update, and don't read the pad in `SimStep`.
   - Take edges from the ported `pressedSince` against the last poll, not `wasPressedThisFrame`.
   - A press and release inside one frame is lost on both, as on the web.
6. **Determinism and allocations.** In Core, use doubles, `JsMath.Hypot` and `deg * (Math.PI / 180)` for the
   `degToRad` yaw / pitch rates. Not `Mathf.Deg2Rad`, a float, which `InputReader.cs:46` already uses: about 1e-8 rad
   off the web at 89°. Allocate nothing per frame or step (PORT-RULES §6): `gamepad.js:103` builds a Set on every poll,
   so use bitmasks.
7. **The breakaway cap is a Core change** (`AimAssist.cs:567`): run golden and the 22 replays at that commit.
8. **The bench.** The worst 1% is at the 6.9 ms bar (`HANDOFF.md:176-177`). Two things matter:
   - AUTO and dynamic resolution would change what `-skygrindBench` measures. Pin HIGH, no dynamic scale under the
     bench, or log the preset.
   - Pad polling adds a little per frame; `SKYGRIND.R.Input` measures it.
9. **The URP asset is tracked.** Setting `renderScale` or the shadow properties on `PC_RPAsset` in Play mode writes the
   file. Assign an `Instantiate`d copy through `QualitySettings.renderPipeline`. `supportsMainLightShadows` has only an
   internal setter (`UniversalRenderPipelineAsset.cs:1304-1307`), so turn shadows off with the light or with
   `shadowDistance` 0.
10. **Hold / toggle scope.** It applies to your own input only (`main.js:1807`), so `forcedMove` and the smoke bypass
    it. Reset the latches on StartRun (`main.js:853`).
11. **Rumble persists.** `SetMotorSpeeds` stays on until reset: stop it on pause, focus loss and quit.
12. **No real pad in automation.** A virtual `Gamepad` covers the smoke. "A full run on a gamepad" (`tasks.json:472`)
    needs GG with a real pad.
13. **The CONTROLS rows name features the port lacks:** F2 photo mode, the BOUNCER grenade jump, the TAB wheel, the
    perk list (`config.ui.mouseControls` / `techniques`). Port them as they are and log it, or filter them (a lead call).

## 7. framePace.js and quality.js

**framePace.js** (`:1-6`, `:18-25`).
- What it does: under a cap, it skips display refreshes until a deadline that steps by exactly 1 / cap from the last
  one, with 1 ms of slack. After a stall it restarts from now instead of catching up. A skipped refresh does nothing at
  all (`main.js:2433-2434`).
- **In Unity, `Application.targetFrameRate` does this job, and it's already wired** (`SurvivalGame.cs:662-666`; the
  FPS CAP row is shown, `UiSettings.cs:19`).
- The difference: the browser always vsyncs and the cap skips vsynced refreshes. Unity's VSYNC is a separate Unity-only
  toggle, off by default, and the cap is ignored while it's on (`GameSettings.cs:29`).
- Recommendation: mark `framePace.js` as superseded in PORT-STATUS. Port `paceFrame` and its 4 tests (~35 lines) only
  if the module count matters. Whether Unity's docs advise leaving `targetFrameRate` at −1 on Web: **not checked**.

**quality.js / qualityMath.js.**
- **The presets** (`config.js:2414-2423`) set: pixel ratio, particle cap, aura, impact, telegraph, AO, motes, sheen,
  point lights and grain. The shadows are `config.js:2486-2491`; the post settings `config.js:2467-2471`.
- **AUTO** runs once per session from TITLE (`main.js:1176`; `quality.js:68-73`, `:91-103`):
  1. It renders the title orbit at HIGH.
  2. It warms up for 0.6 s.
  3. For 3 s it records frame times (the raw dt, clamped to 0.25 s, in ms) in a 240-slot ring.
  4. It picks from the p95 and the median (`qualityMath.js:21-30`; `config.js:2425-2431`):
     - **HIGH** if the p95 is at most 12.5 ms, or if the frames keep up with vsync (median at most 17.5 ms, p95 at most
       1.12 × the median)
     - **MEDIUM** if the p95 is at most 25 ms
     - **LOW** otherwise
  5. It emits `qualityPicked`.
- **Dynamic resolution** works only in play (`quality.js:105-116`; `qualityMath.js:45-67`). Every 0.25 s, once there
  are at least 30 samples:
  - A p95 over 18 ms for 2 s lowers the scale by 0.1 (to 0.6 at least).
  - If a drop doesn't bring the p95 under 0.92 × what it was, the drop is undone and the scale holds for 20 s (the CPU
    is the limit).
  - A p95 under 13 ms, or keeping up with vsync, for 4 s raises it by 0.1.
  - Each change emits `renderScale`.
- The final pixel ratio is preset × `settings.renderScale` × the dynamic scale (`quality.js:56-58`).

**How each part maps to Unity:**

| Web part | Unity |
|---|---|
| Bloom, bloomScale, SMAA, grain, lights, motes | Done (`LightAndLens.cs:49-60`, `:82-89`) |
| Pixel ratio × render scale × dynamic | The URP `renderScale` on a runtime asset copy (`UniversalRenderPipelineAsset.cs:1118-1121`). The desktop has no DPR. On a DPR-1 screen the web's MEDIUM renders at 1.25 (above HIGH's 1; `qualityMath.js:33-36`). The Unity map is a GG decision (UnityTuning) |
| Shadows (enabled, map size, range) | `mainLightShadowmapResolution` (`:1319-1322`), `shadowDistance` (`:1462-1465`), off through the light. The web's range is a box around the player; URP uses cascades, so it's approximate (log it) |
| Particle cap, aura, impact, telegraph, AO, sheen | No consumer in Unity (grep: no use of `particleCap`, `pixelRatio` or `lights.shadows` in `Assets/Scripts`) |
| Quality levels | Keep one (PC) and drive the copy. Three levels with three URP assets would add files and the asset-rewrite problem (D-09) |
| vSyncCount, targetFrameRate | Done (`SurvivalGame.cs:662-666`) |

With VSYNC off (the default), frames never sit on a refresh, so "keeping up" rarely applies and a fast machine is picked
by the 12.5 ms p95 rule.

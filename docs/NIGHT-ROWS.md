# NIGHT-ROWS: sim rows as data (from NIGHT46 on)

Written by NIGHT45 builder S1 (NIGHT45.md card S1, GG's J5: "lets cut fluff and old stuff"). Until NIGHT45, every night that changed the
sim added an `N4xBlocks` struct, a `SurvivalSim.N4x.cs`, a tape format bump, an `AsFormatNN` link, an `N4xOnPath` constant, a
`-skygrindOffNN` line in three places, and rewrote seven shared tests' format pins. NIGHT43's shared commit touched 52 files. From NIGHT46
on, a sim row is a line of config, and the registry reads it.

Status on NIGHT45: **built, tested, UNWIRED**. Nothing in the running game reads it; the tape is still format 26; the play build is
unchanged. Switching it on is a one-time step at NIGHT46's G0 (section 1). After that, a night's shared commit is at most three files
(section 2).

## What exists (NIGHT45, all S1's files)

| File | What it is |
| --- | --- |
| `Assets/Scripts/Core/NightRows.cs` | The registry (`NightRows`), a run's flags (`NightRowState`), the tape record (`NightRowsTape`), the off switch, the level rows and the view rows |
| `Assets/Scripts/Game/SurvivalSim.Rows.cs` | The sim side: `rowsRegistry`, `rowsOverride`, `rows`; `RowsInit` / `RowsStartRun` / `RowsStep` (no caller yet); the per-night hooks `Night46StartRun` ... `Night57Step`; the fingerprint's `RowsFingerprintEnter` / `Exit`; `RowsBaseline.OnPathThrough = 45` |
| `Assets/Scripts/Game/Tape/TapeRows.cs` | The record inside a tape: `Write`, `Read` (length-prefixed), `Replay` (a missing row is a `TapeFormatException` line) |
| `Assets/Tests/EditMode/NightRowsTests.cs` | 11 Core tests (registry, order, off switch, checker, the real config, the record, old records, refusals, `Through`, level and view rows) |
| `Assets/Tests/EditMode/Game/NightRowsTapeTests.cs` | 3 Game tests (the record in a tape stream, the refusal as a tape line, RunTape at format 26 now and 27 after the switch-on) and `AsFormat26`, the test chain's last link |
| `Tools/night/rows-switch-on.cjs` | The one-time switch-on: 30 anchored edits in 19 files (`--dry` first) |
| `Tools/SimHarness/Harness.N45.Rows.cs` | `n45rows [check] [--expect-wired]` and `n45rows selftest` |

## The rules

1. **A sim row** is `design.nNN.<name>` with `"sim": true`, for NN >= 46. Its name ends with its night (`wall46` in `design.n46`).
   It has a bool `enabled` (the design default) and a `_why`. A row without `sim` is a view row or a level row, never a tape bit.
2. **The registry** reads every night's sim rows once, at `SurvivalSim` Init: nights ascending, then names by ordinal (never the JSON's key
   order). A row's index is its id for the run. Sim code resolves ids once in its StartRun (`rowsRegistry.IndexOf("wall46")`) and reads
   `rows.On(id)` per step: an array read, no allocation. A row the config lacks has id -1 and reads off (the older game).
3. **The off switch**: `-skygrindOffNN` (Player) / `--offNN` (harness) turns off every row of night NN and later (the smallest NN given
   wins). `-skygrindOff47` is NIGHT46's game. Every older switch (`-skygrindOff19` ... `-skygrindOff45`) turns them all off, as each older
   night's switch did for the blocks. View rows (`NightRows.ViewOn`) and level rows (`NightRows.SwapLevels`) obey the same switch.
4. **The tape record** (format 27, once): per night, its number, its row names, then the bits. A replay sets each named row this build
   knows to its recorded bit. A row the tape does not name replays OFF, so every older tape (formats 1-26, or a record made before a row
   existed) replays the new rows off. A row the tape ran ON that this build lacks is refused with a tape line (the replay could not be the
   run). So: **never rename or delete a row that ever ran ON**; retire it by setting `enabled: false` and keep it in config (docs/RETIRED.md).
5. **Order in the sim**: `RowsStep` runs right after `N43Step`, nights ascending (46, 47, ...); inside a night, the order its
   `SurvivalSim.NNN.cs` calls its builders' partials. This is the web's `simulate()` order with each night appended, as N40Step-N43Step are.
6. **The composed fingerprint**: rows of nights up to `RowsBaseline.OnPathThrough` run as configured; later nights' rows run off. The freeze
   that turns a night's rows ON by default moves that one number to the night and re-records the baseline (the old per-night `N4xOnPath`).
7. **Level rows** (NIGHT46+): `{ "enabled", "levelFrom", "levelTo" }`, no `sim`. `NightRows.SwapLevels` swaps them into the campaign order
   after `N45Levels.Swap`; `NightRows.BaseId` maps a level back to its `levelFrom` (chained, unconditional, allocation-free after the first
   call), asked first by `N45Levels.BaseId`, so every older id check still sees its level (the NIGHT45 forge's lesson).

## 1. The one-time switch-on (NIGHT46 G0, once; the lead)

Measured as a dry run on NIGHT45 in S1's worktree (Research/2026-10-17-wave45/S1.md): applied, every gate below GREEN, then restored.

1. `node Tools/night/rows-switch-on.cjs --dry` must print `30 edit(s) planned (dry run, nothing written), 0 already in, 19 file(s)`.
   A `MISS` line means a lead file changed since NIGHT45: fix the anchor in the script, never edit around it.
2. `node Tools/night/rows-switch-on.cjs` prints `30 edit(s) applied`. A second run prints `30 already in`.
3. Gates, in order:
   - `bash Tools/night/dn-check.sh` (or `dn-check-wt.sh <scratch>`): 0 errors.
   - `dotnet run --project Tools/CoreHarness -c Release`: golden 0 mismatches, 22 / 22 exact.
   - `dotnet run --project Tools/SimHarness -c Release -- fingerprint`, then `-- fingerprint --off45`, then `-- fingerprint --off46`: all
     EQUAL (no row exists yet, so the switch-on changes no hash).
   - `dotnet run --project Tools/SimHarness -c Release -- n45rows --expect-wired`: GREEN (WIRED, format 27, probe, tape EQUAL and refused).
   - `dotnet run --project Tools/SimHarness -c Release -- tape27`: EQUAL (an older night's rows still replay at format 27).
   - `dotnet run --project Tools/SimHarness -c Release -p:Extra=../../Assets/Tests/EditMode/NightRowsTests.cs -- n45rows selftest` and the
     same with `-p:Extra=../../Assets/Tests/EditMode/Game/NightRowsTapeTests.cs`: 11 / 11 and 3 / 3.
   - The Editor compile, then the EditMode batches `N3` / `N4` (N37-N43SharedTests: their format pins are now 27), `NightRows`, `Lesson25`.
     **Not run by S1** (builders never run the Editor).
4. Commit the 19 files in one commit ("NIGHT46 rows switch-on"), explicit paths.

The 19 files and what each gains (one or two lines each):

| File | Change |
| --- | --- |
| `Assets/Scripts/Game/SurvivalSim.cs` | `RowsInit();` after `N43Init();` |
| `Assets/Scripts/Game/SurvivalSim.Run.cs` | `RowsStartRun();` after `N43StartRun();` · `RowsStep(step);` after `N43Step(step);` · `RowsFingerprintEnter();` / `RowsFingerprintExit();` beside the N43OnPath save / restore |
| `Assets/Scripts/Game/Tape/TapeScript.cs` | `sim.RowsFingerprintEnter();` / `sim.RowsFingerprintExit();` beside the N43OnPath save / restore |
| `Assets/Scripts/Game/Tape/RunTape.cs` | `FormatVersion = 27`; `public byte[] rows;`; `TapeRows.Write(w, rows);` after `n43`; `t.rows = version >= 27 ? TapeRows.Read(bytes, r) : null;` |
| `Assets/Scripts/Game/Tape/TapeRecorder.cs` | `tape.rows = sim.RowsTapeRecord();` |
| `Assets/Scripts/Game/Tape/TapeReplay.cs` | `sim.rowsOverride = TapeRows.Replay(sim.rowsRegistry, tape.rows);` |
| `Assets/Scripts/Game/PlayerChecks.cs` | `NightRows.ForceOffFrom = NightRows.OffFrom(args, "-skygrindOff");` |
| `Tools/SimHarness/Program.cs` | `NightRows.ForceOffFrom = NightRows.OffFrom(args, "--off");` |
| `Assets/Scripts/Game/N29Fixes.cs` | `-skygrindOff46` and later boot like a player's game (`OffSwitchNight(a, ...) < FirstNight`) |
| `Assets/Scripts/Core/Curriculum25.cs` | `NightRows.SwapLevels(cfg, res);` after `N45Levels.Swap` |
| `Assets/Scripts/Core/N45Levels.cs` | `BaseId` asks `NightRows.BaseId` first |
| `Assets/Tests/EditMode/Game/N37SharedTests.cs` | the format pin 26 -> 27; `N37TestBaseline` also sets `NightRows.ForceOffFrom = 46` (and restores it) |
| `Assets/Tests/EditMode/Game/N38` ... `N42SharedTests.cs` (5 files) | the format pin 26 -> 27 |
| `Assets/Tests/EditMode/Game/N43SharedTests.cs` | the format pin 26 -> 27; `AsFormat25` calls `NightRowsTapeTests.AsFormat26` first |
| `Assets/Tests/EditMode/Lesson25Tests.cs` | `TheOrder_FollowsTheBlocks` sets `NightRows.ForceOffFrom = 46` (it pins the older orders) |

This is the last format bump and the last rewrite of the shared tests' pins for a row.

## 2. A night's shared commit from NIGHT46 on (at most 3 files)

- [ ] **1. `Design/unity-config.json`**: the `design.nNN` block, patched by a `.cjs` (never by hand):
  - `"_why"`: the night's request, verbatim source.
  - each sim row: `"<topic>NN": { "sim": true, "enabled": false, "_why": "...", <its numbers> }` (the name ends with NN).
  - each view row: `"<topic>NN": { "enabled": false, "_why": "..." }`; read with `NightRows.ViewOn(cfg, "<topic>NN")` (no NNNView.cs).
  - each level row: `"<topic>NN": { "enabled": false, "levelFrom": "...", "levelTo": "nNN-...", "_why": "..." }`, plus its id in
    `design.maps.list` (same file). The level file itself is its builder's (the generator writes it and its StreamingAssets copy).
- [ ] **2. `Assets/StreamingAssets/Design/unity-config.json`**: a byte copy (`cmp` the two).
- [ ] **3. `Assets/Scripts/Game/SurvivalSim.NNN.cs`** (+ its `.meta`; `NNN` = `N46` for NIGHT46, `NN` = `46`): the night's hooks, for
  builders to fill in their own partial files (`SurvivalSim.N46.Move.cs` ...):

```csharp
// NIGHTNN (the shared commit): the night's sim rows (design.nNN.*, sim: true; Core/NightRows.cs). Off: -skygrindOffNN / --offNN.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        int idWallNN = -1, idPourNN = -1; // one id per row, resolved once a run

        partial void NightNNStartRun()
        {
            idWallNN = rowsRegistry.IndexOf("wallNN");
            idPourNN = rowsRegistry.IndexOf("pourNN");
            NNNMoveStartRun(); // builder M
            NNNLevelStartRun(); // builder L
        }

        partial void NightNNStep(double dt)
        {
            NNNMoveStep(dt);
            NNNLevelStep(dt);
        }

        partial void NNNMoveStartRun();
        partial void NNNMoveStep(double dt); // reads rows.On(idWallNN)
        partial void NNNLevelStartRun();
        partial void NNNLevelStep(double dt);
    }
}
```

Then the gates: `n45rows` (the checker: 0 problems; the night's rows listed), golden 0 / 22, fingerprint EQUAL plain and `--offNN`
(rows of night NN are after `RowsBaseline.OnPathThrough`, so off in the composed fingerprint), dn-check 0.

**No longer part of a night**: an `NNNBlocks.cs`; a tape format bump; an `AsFormat` link; the seven format pins; an `NNNOnPath` constant;
the `-skygrindOffNN` lines in PlayerChecks, Program.cs and N29Fixes; a `ForceOff` line in N37TestBaseline and Lesson25; a Curriculum25
line; an `NNNView.cs`. A harness or a reel forces rows with `sim.rowsOverride = sim.rowsRegistry.Defaults().Set("wallNN", true)`.

**At the night's freeze** (rows turned ON by default): `RowsBaseline.OnPathThrough = NN` in `SurvivalSim.Rows.cs` and the baseline
re-recorded, in its own commit (as the `N4xOnPath` flips were).

**Night 58 and later**: `SurvivalSim.Rows.cs` pre-declares hooks for nights 46-57; a later night adds its two `partial void NightNNStartRun();
partial void NightNNStep(double dt);` declarations and their two calls there (a fourth file, once).

## Not checked (NIGHT45)

- The Unity EditMode run of `NightRowsTests` / `NightRowsTapeTests` (run headless on the harness's mini NUnit only).
- After the switch-on: the Game shared tests N37-N43 and Lesson25 in Unity (they read `UnityEngine.Application`); a Player boot with
  `-skygrindOff46` / `-skygrindOff47`; a Player tape recorded at format 27 and replayed.
- `NightRows.ViewOn` in a view (no NIGHT46 view row exists yet).

# D1 flow46: delete design.n46.flow46 (builder D, NIGHT71, 2026-10-26 ~11:15 MST, t0+0:29; base 106dd56e7)

Source: Research/2026-10-26-forge71/DEADCODE-candidates.md Part B.1 (score 7). Slate Logs/n71/slate.json D1.

## Never-delete checks (run at 106dd56e7, before the edit)
- `grep -n -i 'flow46\|drone49\|teach42' HANDOFF.md docs/memory/THREADS.md docs/memory/OWNER.md` (main tree): no hit.
- `grep -l -i 'flow46\|drone49\|teach42' DECIDE*.md` (main tree, untracked pages, read only): DECIDE42, DECIDE43, DECIDE49. None names flow46.
- No `-skygrind*` flag, audio event, scene / prefab GUID, serialized field, test golden or replay names flow46.

## Proofs at the head (106dd56e7)
- `git grep -n flow46 -- . ':!*.md'`: 6 hits in 4 files: both config copies (the block, lines 7086-7090), N46SharedTests.cs:18 (the
  ViewRows list), the comment SurvivalSim.N46.cs:6, and Tools/night/n46-shared.cjs (NIGHT46's shared-commit generator: history, not run;
  left, as D-5 rejects deleting Tools/night scripts).
- No reader: nothing calls `NightRows.ViewOn(..., "flow46")`. The row's `enabled` is false in both copies.

## What was removed
- the config block `design.n46.flow46` (5 lines) from Design/unity-config.json and Assets/StreamingAssets/Design/unity-config.json;
  `cmp` of the two copies prints nothing; both parse as JSON.
- `"flow46", ` from N46SharedTests.ViewRows (the test reads each listed row from the config; the row no longer exists). No test removed.
- the word flow46 from the comment SurvivalSim.N46.cs:6.

OFF behaviour kept: nothing changes (the row had no reader).

## Checks after the edit (Research/2026-10-26-wave71/D/flow46-checks.txt)
- `bash Tools/night/dn-check-wt.sh <scratch>/dn`: 0 errors on the four projects.
- `bash Tools/night/n71-gates.sh --no-editor`: config copies identical; golden 39 sets, 22 replays exact, 0 failing, 0 mismatches;
  fingerprint plain and --off57 .. --off67 EQUAL; "gates: all GREEN 11:14:52".
- Re-grep after the edit: `git grep -n flow46 -- . ':!Research' ':!*.md'`: only Tools/night/n46-shared.cjs (history, above).
- Test class touched: N46SharedTests (Skygrind.Core.Tests): the lead runs it in the Editor (not run here: no Unity for D).

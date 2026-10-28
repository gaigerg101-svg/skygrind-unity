# D1 drone49: delete design.n49.drone49 (builder D, NIGHT71, 2026-10-26 ~11:23 MST, t0+0:37; base 4da846d8e)

Source: Research/2026-10-26-forge71/DEADCODE-candidates.md Part B.1 (score 9). Slate Logs/n71/slate.json D1.

## Never-delete checks
- HANDOFF.md, docs/memory/THREADS.md, OWNER.md (main tree): no hit for drone49.
- DECIDE49.md (main tree, untracked, read only) D-2: "`design.n49.drone49` (enabled false). The drone keeps its NIGHT47 look with foes48
  on ... Reason: the NIGHT48 blind pairs chose the old drone three times; keep the old look until a fourth look wins." D-20 (the close):
  "OFF with reasons: ... drone49". A decided OFF verdict, not an open question (NIGHT71.md 5.3; DECISIONS D-9 item 4). No later
  DECIDE page (50-70) names drone49 (`grep -l drone49 DECIDE*.md`: DECIDE49 only).
- Not in SameRun63Math.FrameRows, no `-skygrind*` flag, no reel script, no scene / prefab, no serialized field.

## The read before the edit: Foes48.On (Assets/Scripts/Game/EnemyViews.Foes48.cs:29-35)
```
public static bool On(GameConfig cfg)
{
    if (Force.HasValue) return Force.Value;
    if (cfg == null) return false;
    if (!ReferenceEquals(cfg, readFor)) { readFor = cfg; on = NightRows.ViewOn(cfg, "foes48"); }
    return on;
}
```
Its only side effect is a memo of the foes48 read per config object. Four other call sites read the same memo
(Enemy40View.cs:173, EnemyViews.N20.cs:171 / 173 / 174, EnemyViews.cs:724), so dropping the drone's call changes no state anyone reads.

## OFF behaviour kept
The old argument was `f47 && !(Foes48.On(game.cfg) && NightRows.ViewOn(game.cfg, "drone49"))`. With drone49 off, ViewOn is false, so
the term is `f47 && !(x && false)` = `f47` for any foes48 value. The new line passes `f47`. The comment on line 285 is rewritten.

## What was removed
- the config block `design.n49.drone49` (the last row of n49: 6 lines + the comma before it) from both copies; `cmp` clean; JSON parses;
  design.n49 keeps _why, charge49, look49, lights49, fx49.
- the ON term in EnemyViews.Drone.cs:286 (and the comment on 285 rewritten).
- `"drone49"` from N49SharedTests.ViewRows (the test reads each listed row from the config). No test removed.

## Checks (Research/2026-10-26-wave71/D/drone49-checks.txt)
- dn-check: 0 errors on the four projects.
- n71-gates --no-editor: config identical; golden 0 failing, 0 mismatches, 22 replays exact; fingerprints plain and --off57 .. --off67
  EQUAL; "gates: all GREEN 11:22:01".
- Re-grep `git grep -n drone49 -- . ':!Research' ':!*.md'`: left as history: foes48's enabled_why text (config line 8113, both copies:
  NIGHT48's recorded verdict, "The drone part is drone49 and stays OFF") and Tools/night/n49-shared.cjs (NIGHT49's shared-commit
  generator, not run). No code reads the key.
- Test class touched: N49SharedTests (Skygrind.Core.Tests): the lead runs it in the Editor.

# NIGHT38 report for GG: the flight night (short close)

**Play:** `Builds/n38-r1/SKYGRIND.exe` (built from `01c5c032`). Three campaign levels have their new rooms on: level 1 (`n38-foundry`:
THE YARD and THE GANTRY added), level 4 (`n38-span`: grapple rings, two flight rooms, THE LOOM on the new level), level 5 (`n38-spire`:
eight new rooms, the gate meter, the jump-dash gates). **I did not get a person-level look at any of them.** The Player smoke passes,
but I did not boot the three campaign levels in a Player: my boot attempts landed on the attract, not on the level. So treat the rooms
as built and bot-walked, not played.

## What you asked for, and where it stands
| ask | state |
|---|---|
| wall rides you stay on, aim up / down to drift | built (ride38: held rides keep height, look pitch drifts). **Off in config.** Its walk on level 1 has a death regression (2 -> 9 deaths in one room). |
| doors you can see (cyan open, orange locked) | built (doors38, view). **Off.** No frames. |
| every level overhauled for flight | level 1 (ON), level 4 (ON), level 5 (ON). Level 2 (iceline): two gates pass; the skate gap is not judged (no ice in the checker). Level 3 (kiln): gates RED. Level 6 (finale): gates RED, and it needs your call (below). |
| shotgun-boost rooms, far dash jumps, grapple gaps | level 3's boost gates are not proven (blast and dash beat them). Level 5's dash-jump gates are proven with its room limits. Level 4's grapple gaps are proven. |
| HUD: charge bars under the crosshair, reload spin | built (hud38, view). **Off.** No frames. |
| boss entrance shake and flicker; damage static | built (juice38, view). **Off.** No frames. |
| THE STAG's walls and read pass | stag38 built, **off**. |
| the LOOM and the CROWN polish | loom38 built (view); crown38 built (view). Both **off** or unseen. |

## Decisions for you
1. **Level 6 (THE FINALE)'s gates:** every verb is owned on that level, so a gate that only a dash jump crosses can be skipped by the
   boost or the grapple. I did not change the rule. Options: take the boost and grapple away at its gates (as the rooms with limits
   already do), or relax the rule for the exam level.
2. **Level 2's skate gap:** the checker cannot judge ice, so the gap is a flow beat, not a gate. Accept it, or tell me how to judge ice.
3. **ride38:** the walk on level 1 dies more with the ride on (room a5, 2 -> 9). Look at that room before turning ride38 on.

## Not done
- Player boots of the three campaign levels (the boot path is open).
- The verbs run on the six levels.
- Frames for doors38, hud38, juice38, loom38, crown38, and the level rooms.
- The kiln's gate proof.
- Three test failures: one fixed (Lesson25's order pin; 10 of 10 now). Two open: an allocation in the autopilot's per-step test (1 over 17,748
  steps) and a 180 s timeout in the every-map test (flaky since NIGHT35).
- Once, the Editor was found in Play mode during the close (origin not identified); stopped with `unity-loop.sh stop`.

Details: NIGHT38-LOG.md (the close table and the integration record).

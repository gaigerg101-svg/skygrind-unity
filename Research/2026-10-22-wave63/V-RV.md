# NIGHT63 builder V: the review's S1 and S8 (branch n63-RV from port/stage3 at a86453e8)

These two items come from Logs/n63/review-n63.md. Both are in my code.

Shas:
- `d884141c`: test first (RED).
- `60482a36`: GREEN.
- This notes commit.

Cherry-pick: `git cherry-pick d884141c^..<notes sha>`.

## S1: THE ICEBREAKER drawn on the floor during its leap (bossMotion63 on)
- **The bug, as the review read it:** `Brawl40View.N63V.cs` DrawMotion63 wrote `bodyRoot.position = fu + m.offset + ...`. Render42 runs
  before it in Brawl40View.Render, so this dropped what Render42 had placed:
  - the leap's height: `bodyRoot.position = fu + up * air42`, "the sim's hit spheres do";
  - Punch42's recoil.
- **The fix:** the hook reads `bodyRoot.position` and `bodyRoot.rotation` first, as the view placed them this frame, the same way the Wick's
  and the Crown's hooks take theirs. It then only adds:
  - the position: `Placed63(rigid, m)` = rigid + the layer's offset + the sink's lift back (the knee drop takes the sink, as before);
  - the rotation: the sway about the line to the player, times the view's own rotation.
- **Tests:**
  - `Placed63` keeps a 2.5 m leap's height exactly, and a layer at rest leaves a recoiled placement exactly.
  - A source scan checks that the hook reads the transform before it writes it, never rebuilds the position from `fu`, and runs after
    Render42.

## S8: boss63's walk-in on the wall clock
- **The bug:** the 3 s window for the room's own entry and the 4 s walk were wall seconds, copied from boss61.
- **The fix:** both windows run on `FeelClock.Now`, the reel's fixed clock, with `wallEnd` as the safety cap.
- **Test:** a source scan of the walk-in. No window may use `Time.realtimeSinceStartup - t0`, and both conditions must read
  `FeelClock.Now - t0 < 3.0 / 4.0 && Time.realtimeSinceStartup < wallEnd`.

## Checks
- RED, measured outside Unity with my scratch runner on the dn-check build: 3 / 3 fail.
- GREEN: N63ReviewTests 3 / 3 pass.
- The engine-free tests of N63BossMotionTests (7), N63NearHideTests (3) and N63BossClockTests (6) still pass. Their tests that call the
  engine cannot run outside Unity: not checked.
- dn-check: 0 errors on both commits. Gates: all GREEN 01:46:30.

## Not checked
- A Player frame of the Icebreaker's leap with bossMotion63 on.
- A boss63 run timed on the new clock.
- S5 (focus60's `locked &&`): the lead's focus probe on n63-b7 decides it.

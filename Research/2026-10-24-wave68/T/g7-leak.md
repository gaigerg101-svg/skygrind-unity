# g7: the parry-swing leak closed (builder T, NIGHT68; the lead's reversal at 4:50)

r2's g7 (melee only) read M2 VERB FAIL on the leak alone (Logs/n68/r2-iceline/verbs-skip.txt): pooled 1.35 vs bar 1.32, STYLE 29.9 vs 23.8,
and "the refused run used the verb on 2 exercised seed(s) (8, 12)". The leak needs the glove in hand, so no red is touched (g7's red is the
shipped gun room).

## The cause (`--trace-leak`, c70838aa; Logs/n68/g7leak/r2-seed8-red.txt, r2-seed12-red.txt)
On both seeds, in both refused runs, the same thing happened:
- A swing passed the filter 15 steps before the reflect, because no glob alive then was catchable on its predicted path.
- The swing sent nothing back, so it opened parry42's orb buffer.
- The buffer caught a glob 15 steps later: `parry42Buffer(gauntlet, parry)` and parry36's buffer.

## The fix (Tools/SimHarness/Harness.N68.Glob.cs V68GlobCatchable; the refusal otherwise as narrow as before)
- A gauntlet press is also refused when a spitter is in its spit tell, its volley leaves inside the buffer (stateSteps <= bufferS + 1),
  and it stands within reach + spitter.globSpeed x (the buffer left after the volley + parry43's window).
  - reach is the largest catch volume, plus the glob's radius and the 0.5 m margin.
  - The glob is born inside the buffer, so the press-time test cannot see it.
  - Spitters farther away, or whose tell ends after the buffer, do not stop the swing.
- The glob's path now falls under the globs' own gravity: MovementMath.GravityFor(jump.height, jump.apexTime), as SpitterAcid's
  constructor sets it. Before, it was a straight line.

## The pair (r2's level, Logs/n68/r2-iceline/n67-iceline.json, its row melee only; bars from r2's slate snapshot)
- Seeds 8 and 12, RED (`r2-seed8-red.txt`, `r2-seed12-red.txt`, the tip before the fix): the refused-in run and the walker each sent 1 glob
  back (LEAK). The leak trace names the buffer path.
- Seeds 8 and 12, GREEN (`r2-seed8-green.txt`, `r2-seed12-green.txt`):
  - globs parried 0 in both refused runs, and no leak trace line.
  - Gauntlet presses refused: 98 and 88.
  - The seed ratios: 8 is 1.60 -> 1.16, 12 is 1.47 -> 2.82. The refused runs' routes changed with the extra refusals.
- Seeds 1-20 pooled:
  - RED: r2's own run on main's harness (Logs/n68/r2-iceline/verbs-skip.txt; the parry room's code there equals this tip's before the fix).
    Pooled 1.35 (sets 1.32 / 1.42), 20 / 20 exercised. Leak on 2 seeds (8, 12): globs parried 2 / 2 in the refused runs. Presses refused
    1054 / 999. M2 VERB FAIL on the leak.
  - GREEN (`r2-seeds1-20-green.txt`, with --trace-leak: 0 trace lines):
    - Pooled 1.35 vs mainBar 1.32 (sets 1-10 1.26, 11-20 1.55), 20 / 20 exercised.
    - "The refused run used the verb on 0 exercised seed(s)"; globs parried 57 / 0 / 0.
    - Presses refused 1460 / 1400 (from 1054 / 999). STYLE 29.9 % vs need 22.8 %.
    - M2 VERB PASS.
  - Not a GATE verdict: these are seeds 1-20 on r2's level, run in this tree, not a main-tree round.

## The whole-level walk
- `flow-g7leak-diff.txt`: EQUAL to the I2 fix round's walk on all five levels; against flow-base-a0 only I3's named n52-iceline change.

Not checked:
- An LT press's own window (0.25 s) against the 0.15 s buffer the test looks over (open.md item 11).
- The extra refusals' cost: the refused-in run's median clear moved 39.4 -> 39.9 s.

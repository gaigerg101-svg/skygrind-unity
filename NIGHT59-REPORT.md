# NIGHT59 report for GG (2026-10-21, 21:11 - 23:1x MST; tag u3-n59; play build `Builds/n59-freeze`)

## In one line
Level 3's exits fix shipped. The other three things tried tonight (level 4's new room, the hit marks, the yellow clean-up) each got
closer but missed a number written down before the run, so they stay switched off.

## What changed in the game
- **Level 3 (THE KILN), room 3.2c THE CLINKER BRIDGES.** You now start that room 2 m left of the door, facing the near bridge
  (south-west), instead of 7.5 m off to the side by the iron apron. The far bridge has an arch under it and the slag step moved 2.5 m
  east, so the walk to the exit is no longer walled off; the same arch idea is in 3.3 THE PRESS HALL's gallery. Measured with the test
  bot over 30 runs: it got stuck and had to be moved on 1 / 2 / 5 times per 10 runs (before: 14 / 15 / 14); it died 9 times in both
  versions; every verb check on the level still passes (31 / 31). A reviewer checked the evidence and passed it.

## PLAY note (the one change you can feel)
1. Play level 3 to the second room after the first link (THE CLINKER BRIDGES).
2. On entering, you face the near bridge (the raised walkway on the left), not straight down the room; the fight starts to your left.
3. Tell us: is the first view clear about where the enemies come from, and is the way to the exit (past the far bridge) easy to find?

## What stayed off, and why (each with its number)
- **Level 4's new room THE GAP NEST** (drones over a gap you grapple across): built and tuned in three cuts, but the room is 50 m long
  where the room rules want at most 32 m, so it passes 11 of 14 checks. Next: split it into a smaller room plus a short approach.
- **Hit marks on kills (kill55):** the marks are drawn and darken the enemy, but the measure wants near-black marks on 5 % of the body
  right after the white kill flash; it reads 0.46 %. Next: draw the marks darker.
- **Yellow clean-up (yellow56):** it now removes stray yellow from every test frame (11 of 12 frames had it, now 0), but the parry halo,
  which must stay yellow, keeps only 57 % of its yellow (the bar is 80 %). Next: teach the meter the halo's own edge.

## Checks on the play build
`Builds/n59-freeze` (built from the tagged code): the automatic smoke test passed every step (229 s, with two planted focus losses);
every verb check passed on all five levels (27 / 29 / 31 / 31 / 31, the same as the last two nights); the movement and simulation checks
are exact (0 mismatches, every replay identical). The switched-off work above is in the code; with it off the game simulates exactly as
before (checked), and the two new drawing paths (the marks, the yellow keep) only run when their switch is on (read in the code).

## Your calls
1. Play level 3's second room and tell us how the new start feels (the PLAY note above).
2. Level 4: is it fine to split THE GAP NEST into a room plus a short approach, or do you want the long gap kept and the room rules bent?

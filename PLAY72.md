# PLAY72.md: what is ON in Builds/n72-play (NIGHT72)

Builds/n72-play is the freeze build (Builds/n72-freeze, HEAD 9350c8631) with the repo's config. Every change below is ON by default and
keeps an off switch: `-skygrindRows72=<row>:0` (any row), or `-skygrindNo<Row>72` for the night's rows. Tick keep or drop.

| row | the change | where to see it | off switch | keep | drop |
|---|---|---|---|---|---|
| colours63 | danger red-orange taken off non-danger decor (the spire's dark-red stripes and accent moved to hue 40) | level 5 (spire), any room's walls and pillars | `-skygrindRows72=colours63:0` | [ ] | [ ] |
| goal64 | the route beam over the exit is pale blue-white instead of orange | level 5 (spire), the beam above the crosshair in the first rooms | `-skygrindRows72=goal64:0` | [ ] | [ ] |
| palette72 | each level's floor, walls and trim in their own colour families (spire: violet floor, teal walls) | every level; most visible on level 5 | `-skygrindNoPalette72` | [ ] | [ ] |
| decor72 | wall bands kept only where they have a job; route chevrons in the level's accent on every ramp | ramps on every level (spire: pale bone chevrons) | `-skygrindNoDecor72` | [ ] | [ ] |
| lit72 | the boss arenas lifted inside each boss's own hue (the kiln's dark share 57 % to 5 %) | the KILN WYRM's arena (level 3 boss) | `-skygrindNoLit72` | [ ] | [ ] |
| bossfx72 | THE CROWN's ring thinner, the gem bursts kept off the crosshair | the CROWN fight (level 5 boss) | `-skygrindNoBossfx72` | [ ] | [ ] |
| body72 | the wyrm's body lit in the danger red-orange on its wind-up tell (pill language kept) | the KILN WYRM fight | `-skygrindNoBody72` | [ ] | [ ] |
| btext72 | the boss line is one short order: "SWAP gauntlet, then SWING LEFT CLICK"; STAGGER only when staggered | any boss fight's hint line | `-skygrindNoBtext72` | [ ] | [ ] |

Also in this build (not a row, a value): the movement profile's ground friction and accel 31.07 (ULTRAKILL's walk stop), with ice and mud
multipliers that keep their stops (ice slides 4.77 m as before). To compare, there is no single switch: it is design.ultra71.set.

Measured but OFF (no blind SPOT on a still): spring72 (springs on enemies), cam72 (dash FOV, slide eye drop, strafe roll), finish55 (the boss
kill's 6 degree kick). Turn one on to try it: `-skygrindRows72=spring72:1` / `cam72:1` / `finish55:1`.

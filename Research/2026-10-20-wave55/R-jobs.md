# NIGHT55 builder R: every piece added or moved in the re-cut rooms, its job, and the bot / probe contact that shows it is used

How the contacts were measured (all on the worktree's level files, no Player):
- **pad probe**: `hold52 --level n52-<level> --room <r> --pads` launches each pad and prints where it lands (`launched yes ... miss <m> PASS`).
- **hold probe**: `hold52 --perches --seeds 3` stands a held player on every reachable top and counts the enemy hits it takes in the room's
  waves (`hold52 <room> tier <t> spot [x,y,z] hits <n>`): hits > 0 = the enemies engage a player standing on that piece.
- **walker reach**: the level report (Reports/LEVEL-n52-<level>-validate.md, "enemies reach every tier"): every raised top reached by walkers
  from the floor spawn (walk and drop links only).
- **walk bot**: n46flow (Walk38Pilot + the ported autopilot), 10 seeds; its death lines print the pilot's feet.
- Not measured: a per-piece count of the walk pilot's own contacts. n46flow's `--trace` prints the pilot's position only on a link
  detour (Harness.N46.Flow.cs:515-516), not during a room's fight, so a per-piece pilot count needs a new harness print (not built: the
  harness is not this builder's file). The pilot never rides a wall or wall-jumps, so ride pieces have no bot count by construction.

## iceline g7 THE THAW LOCK II (Tools/night/n55-r-g7.cjs)
| piece | added / moved | job | contact |
|---|---|---|---|
| THE FIREBOX (east pit 3.5 -> 7.5 m wide) | re-cut | no walker crosses the east; the east ride (and the coal hoist) is the quick way over: I2 1.26 -> 1.75 | walk bot 10/10 clears; I2 walk 20.1 m vs ride 11.5 m on the nav grid |
| THE STOKER'S BRIDGE (x 1.5..4.5, against the boiler) | moved (the east bridge) | the walkers' east crossing with fire on both sides: the punch spot (the card's verb) | hold probe tier 0 spot [2,0,-200] (on the bridge): 79 hits; walk bot seed 10 died on it at (4.3, 0, -198.7) |
| THE KEEPER'S TOWER (4 m, 3.5 x 6.5 m, at the firebox's far edge) | moved (the keeper's house) | the spitter's stand over the fire, 2 m past the east ride's end | hold probe tier 2 spot [10,4,-208]: 42 hits; coal hoist lands on it (miss 0.1 m); walker reach PASS |
| THE COAL CONVEYOR (4 m over 8 m ramp) | moved (the house ramp) | the walkers' climb onto the tower (enemies reach every tier) | walker reach PASS (5 raised tops, 12 nodes) |
| THE COAL STAGE (2 m, 4 x 3 m) | **added** | the riders' run-up straight onto the east sluice wall; wave 3's pack comes off its lip | hold probe tier 1 spot [10,2,-189.5]: 64 hits; walker reach PASS |
| THE STOKER'S POST (1.2 x 4 m) | **added** (the east sluice post moved) | the corner the pack rounds off the bridge; breaks the tower's line onto the bridge | hold probe perch [5,4,-204] (its top): 36 hits |
| the east mooring post | moved | on the tower's line to the quay's east half | hold probe perch [4.5,3.5,-192]: 40 hits |
| the keeper's hut | moved (onto the tower) | the tower spitter's shield | hold probe perch [11,6,-210]: 56 hits |
| THE COAL HOIST pad | moved off the bridge's mouth (I4) | quay -> over the firebox -> the tower | pad probe: launched, landed [10.02,4,-206.6], miss 0.1 m PASS |
| THE RETURN HOIST pad | moved off the conveyor | the firebox's far lip -> the east bunker (wave 3's turn) | pad probe: launched, landed [9.01,2,-185.88], miss 0.12 m PASS (in the keeper's yard its arc hit the conveyor: miss 24.2 m, FAIL, so moved) |
| the east sluice wall (ride) | re-cut 18 -> 11.5 m | the only quick way over 7.5 m of fire | no bot count (the pilot does not ride); I2 1.75 |
| the east bunker ramp | re-cut 4 -> 2.5 m wide | wave 3 single file into the glove | walker reach PASS |
| the east coal island | **cut** | it would stand on the new bridge | - |

## iceline g5 THE ICEHOUSE (Tools/night/n55-r-g5.cjs)
| piece | added / moved | job | contact |
|---|---|---|---|
| THE CRANE RUNWAY (4.5 m, 3.5 x 10 m along the east wall over the ring) | moved (the crane deck, the shelf's twin in the SE corner) | the drones' perch over the warden's ring in the middle of the fight; the east ride's landing | hold probe tier 2 spot [8,4.5,-143]: 24 hits; ice hoist lands on it (miss 0.04 m); walker reach PASS (after the first cut's ramp foot against the wall failed it: "not reached by walkers", moved 1 m) |
| the east ride wall (ride, 18 -> 9.5 m, into the runway) | re-cut | the shortcut onto the drones' perch: walk 44.3 m vs ride 9.5 m (I2 1.11 -> 4.66) | no bot count (the pilot does not ride) |
| THE RUNWAY RAMP (4.5 m over 9 m from the far SE corner) | moved | the warden's climb after you (enemies reach every tier) | walker reach PASS (2 raised tops, 9 nodes) |
| THE CRANE HOOK (1.2 x 3.5 m at the runway ramp's foot) | **added** (the far east post moved) | the corner the warden circles; the break on the runway's line to the exit | hold probe perch [4,3.5,-156] (its top): 23 hits |
| THE POUR CHUTE (the shelf's ramp, rising west into the shelf's east face) | moved off the west lane | the second pack pours down it into the ring behind the stack (the blast's target); the west lane is the ride's | walk proof 1 re-pointed up it (levelgen walks it); walker reach PASS |
| the crane cab, the crane hoist | moved onto the runway | the drones' cover; runway -> shelf (the high circuit) | pad probe: crane hoist landed [-7.6,2,-153.05], miss 0.11 m PASS; cab top perch [9.5,6.5,-143]: 21 hits |
| the ice hoist | re-aimed onto the runway | floor -> the drones' perch | pad probe: landed [8.03,4.5,-141.53], miss 0.04 m PASS |

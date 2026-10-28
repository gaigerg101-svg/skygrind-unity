# NIGHT43 report for GG: the cohesion night (your thirty asks from n42-r4, levels 1-3)

**Play build: `Builds/n43-r5/SKYGRIND.exe`** (frozen 19:54; the smoke PASS; verbs 26/26, 20/20, 25/25, 30/30, 30/30). `-skygrindOff43` gives
NIGHT42's game. Nine builders worked from 16:25; the monthly spend limit stopped all of them from 17:15 to 18:50 (about 1.6 h lost), so a few
things are SHORT, below. Every ask's fate: NIGHT43-LOG.md. The rules you asked for: **docs/DESIGN-RULES.md** (numbered rules for movement,
combat feedback, enemies, encounters, arenas, tutorials, progression, UI, art, bosses, plus a checklist for every new room) and
**docs/ROOMS.md** (what each room of levels 1-3 teaches, needs, which enemy roles, its 30-second problem). PRINCIPLES.md points to both, so
every session loads them.

## What we found (the causes behind your words)
- "After a certain number of jumps the movement stops working": wall jumps were capped at 3 per airtime and only a landing refilled them.
- The door that never opens: a room counted every enemy in the whole level, and an enemy knocked into a dry pit never died.
- The floating enemies in the ride-and-shoot room: three "stand-ins" spawned over the pit when you walked in.
- The three-wall climb: the walls were 7 m apart and overlapped by only 1 m, so your speed carried you past the next one.
- B (slide) just after a ride was a slam that set you to 48 m/s straight down. On your pad B is also jump: both now behave.
- The shotgun jump: a shot pressed on the ground fired on the ground (no boost), and letting go of jump halved any boost.
- The orb parry: the window was about 0.11 s, measured from your eye.
- The pink at the boss door: its violet light plus the entrance flicker's red / blue split; the "brick wall": a freeze at the door and a
  12.4 m/s speed clamp in the shaft.

## Your asks, one line each (ON = in the build)
| ask | what changed | checked | state |
|---|---|---|---|
| G3 / G13 wall riding | a wall jump off a new wall refills the count; one "same wall" rule for every lock; a wider attach, a softer snap, a longer grace; B after a ride keeps your speed; a dash into a wall attaches | chimney climb by presses 5.6 -> 13.5 m; B keeps 16 m/s; near misses attach 0 -> 8 of 12; THE WICK fight 10/10 for both bots | **ON** |
| G1 THE GANTRY | over lava now: gate 1 one long wall to learn the hold, gate 2 a chain with a dash gap; a fall costs 10 health and puts you back on that gate's lip | scripted people: gate 1 100 % first try, gate 2 90 % first / 100 % in two; flights 0.43 s median | **ON** |
| G2 THE FLUE | a 3.5 m chimney from the door, walls continuous, the deck drops forward one way | held 87.5 %, taps 75 %, presses straight across 100 % (was 45 % / 0 %) | **ON** |
| G4 ride-and-shoot (PIERCE) | no floating enemies: you ride, land on a perch, then the husks arrive with spawn tells | the frames (the hatch, the counter "3 LEFT") | **ON** |
| G5 doors + counter | a room counts only its own enemies; pit / stuck bodies end; a lesson can't hold a door; "N LEFT" in the top-left corner | doors open on every kill path 17 -> 64 of 64 rooms | **ON** |
| G6 / G24 the UI | one visual language (slanted panels, type roles, colour tokens): the health bar (cells, after-image, frame pulse), the style meter, the shop, results, pause, title and level select over a blurred game | ui43 frames after two iterations | **ON** |
| G7 tutorials | a centre card above the crosshair with your button's glyph, an open animation, never truncated | frames | **ON** |
| G8 / G29 the look | lit exits that brighten when a room clears, lava glow from below, one ride-strip rule (the old strips were drawn mirrored, away from their walls), landmarks | frames (warm share 19 -> 33 % in 1.3 RIDE) | **ON** |
| G10 the boss door | a cyan door, no red / blue split, no freeze, the shaft's speed eased (worst loss 5.6 -> 0.7 m/s) | the door frame | **ON** |
| G11 backtracking | doors lock behind you; THE FLUE's deck drops forward; the gantry's way back is over lava | per-room table in ROOMS.md | **ON** |
| G14 boss readability | the bar stays for the whole fight: HP, LOCKED (with what to do), OPEN (with a timer), STAGGER, the leg; for every boss | bar valid every live step | **ON** |
| G15 the intro | the camera eases onto THE WICK as it rises, letterbox, the shaft lights come on in order; you keep control; 1 s on a retry | frames (the reveal) | **ON** |
| G16 phases | each leg change is a beat (recoil, the slag drops, a free crack); leg 2 and 3 add one variation each; a hit while OPEN makes it flinch | bands 47-57 s bot | **ON** |
| G17 gauntlet | 1.8 -> 1.25 s | THE ICEBREAKER still 10/10 in band | **ON** |
| G18 parry | caught up to 5.6 m out (was 1.9 m); a buffered swing; the same rules for spitter globs | an orb parried at 3.6 m in the Player | **ON** |
| G19 FUSION | a gauntlet hit at speed (dash, jump-off) does x2, throws and stuns; a hot-pink burst where cyan and orange meet, the word, a crack of rumble; level 2's gun-again room has a dash lane for it | FUSION at 22.8 m/s for 955 damage in the Player | **ON** |
| G20 combat feel | gun hit sparks per weapon, the charged slug throws and blasts (3 kills instead of 1), the TORCH sets enemies burning with a real flame stream, rumble per weapon, the cyan wash on the charged release fixed | frames | **ON** |
| G21 enemies | every kind a pill with its own visor and back part (slit, lens, chevron brow, shield plate), darker bodies so they read | the line-up frames; greyscale measured | **ON** |
| G22 stuck enemies | a stuck enemy crouches and hops to a free spot, on screen too | longest still 6.3 -> 2.7 s | **ON** |
| G23 mini-ICEBREAKERs | level 2 has 4 chargers before THE BRAWL (was 2): one by a wall, one with an escort, then two | harness | **ON** |
| G25 / G26 level 3 start | the ramp is gone: three boost steps with cyan lips, taught small then big; jump + shot within 0.15 s either way boosts in full | landing step 1 36 -> 82 %, step 2 13 -> 54 %; boost 1.5-5 -> 6.3-8 m | **ON** |
| G27 compositions | drones and chargers enter level 3 by role; at most 3 roles at once before level 4 | per-room table | **ON** |
| G28 worms | the BURROWER: hunts your footsteps under the floor, a 0.8 s ring tell, bursts out; hit it in its 0.5 s window for a counter; safe in the air or on raised plates; a new room THE ASH BED teaches it | tell / emerge / counter in the Player | **ON** |
| G28 THE RING | dressed as a kiln: iron plates with glowing seams where the worm surfaces, a molten core, a hanging crucible, THE KILN MOUTH | frames | look ON; **geometry SHORT** (the worm's paths leave little room) |
| G9 / G12 rules | docs/DESIGN-RULES.md, docs/ROOMS.md | | DONE |
| sound (G15, G19, G20) | yours: new event names only (fusion43, slug43, burn43, burrowTell43 / Emerge43 / Counter43, bossLeg43, bossOpen43, roomClear43, fallReturn43) | | for you |

## Not done / open
- THE RING's geometry is only lightly changed (one slag plate, the landmark); a real rebuild needs the worm's paths moved.
- THE WICK's fight reel still never kills it in a Player (the harness kills it 10/10 after the climb pilot's fix): T-59 narrowed, not closed.
- The movement rows were judged on harness numbers and the verbs check; their own reel's pilot never reached a wall in the Player.
- The full test suite was not run tonight (only the NIGHT43 tests, the shared tests and Campaign24: all green).

## What to try first
Level 1 from the start: the dash trench, THE GANTRY (fall in once), THE FLUE, PIERCE, THE WICK's reveal and bar. Then level 2's gun-again
room (dash into the husk with the gauntlet) and level 3's first room and THE ASH BED. Your calls are in DECIDE43.md (D-1..D-9).

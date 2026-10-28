# NIGHT46 report: the flow night (2026-10-17 22:56 - 2026-10-18)

**Outcome.** You told me to decide every call and that you couldn't test. Every call is on DECIDE46.md. The night measured where the
game makes you stop, fixed the worst causes and rebuilt level 4 and level 5's fight rooms from identity cards plus a flow plan. It
checked all of it with one flow meter against NIGHT45's game. **The new play build is `Builds/n46-r3/SKYGRIND.exe`.** It passed verbs37
on all five levels (26 / 20 / 25 / 30 / 30) and the full smoke ("DONE: all steps passed", 0 errors). The whole test suite ran: Core 1127 / 1129
(the 2 were a leak between two test classes, fixed and re-run), Game 1046 / 1046.

## What changes for a player
- **A death no longer replays the waves you already won.** You restart at the wave you died in.
- **The arena doesn't go empty between waves.** The next wave arrives while one or two enemies still stand, from where you're heading.
  It holds back when that would stack a big wave on you or bring it in at your door (THE THAW LOCK's wave 3).
- **Teaching rooms hold you at most 8 s** with nothing to fight (was up to 20 s).
- **A burrower stuck under the floor no longer keeps the door shut** (it happened in 5 of 10 bot runs on level 3).
- **Boss intros take 1 s** instead of 2.
- **THE KILN WYRM: you walk into its arena through the door at full speed.** The teleport that dropped your speed to zero is gone, and
  the arena is visible through the boss door. Bot fight: median 38 s (was 161 s), longest 85 s (was 300 s), killed 40 / 40. The bot
  still takes damage in every fight, and the crumble falls every time.
- **Level 3's three cliff doors** now have a cyan ride panel into each climb, so you hit the cliff riding instead of standing.
- **Off-screen attacks are warned:** a red-orange chevron at the screen edge points at an enemy winding up where you can't see it. In
  the bot test it warned before 14 of 14 hits from out of view (the Survival arena) and 5 of 5 (DRYDOCK).
- **Taking damage no longer fills the whole screen with static**; it stays in an edge band (your w30 rule).

## The rebuilt rooms (identity card first, then a flow plan checked against the research, then geometry)
- **Level 4, p1 THE HOOK YARD.** A walled cable yard round a 5.5 m cable drum; you pull to the great hook's ring over the hoist well.
  Frame: `Logs/feel/n46-r3/a146/a146/shot-N30-A146-p1-entrance.png`.
- **Level 4, p3 THE TIE-OFF.** The bridge's tie-off: two strafe decks over a void, the KNOT hung from the roof under an amber lamp.
  Kills give pulls between the anchorages. *Tell a friend:* "you pull between two anchor towers over a drop, and every kill gives you
  another pull." Frame: `shot-N30-A146-p3-entrance.png` (wave 1 and the KNOT in view).
- **Level 4, p5 THE SPOOL HALL.** The great spool stands alone in the void on two gangways. Its spitters guard the health, and the
  whip pulls them off. Frame: `shot-N30-A146-p5-entrance.png`.
- **Every level 4 void now has a lip return** (5 health in a fight room, 10 on a link). Before, a fall sent you back to the start of
  the link for 25 % of your health.
- **Level 5, v2 THE FALLEN BELL.** A bell came down through the nave's vault; you drop in from a lip and fight round it. The health sits
  on the bell's shoulder. Frame: `Logs/feel/n46-r3/a246/a246/shot-N30-A246-v2-lip.png`. It reads as a stepped monument; whether it reads
  as a bell is for your eye.
- **Level 5, v6 THE ROPE CHOIR.** Islands: wave 1 on the central island in view from the door, the organ loft and stalls as the top
  tier, a ride wall up the east side. Frame: `shot-N30-A246-v6-door.png`.
- **Level 5's four identical belfry gates are now three, each with its own job.**
- **Level 2, g3 THE TIPPLE** (was THE SWARM). A coal hopper the swarm pours from, a trestle with a one-way drop, half walls. Frame:
  `Logs/feel/n46-r3/a346i/a346/shot-N30-A346-g3-entrance.png`.
- **Smaller fixes on levels 1 and 2:**
  - a3's lava now leaves a walk round both sides.
  - b1's wave 1 stands where you can see it from the door.
  - a8's lesson starts at the first ride wall.
  - b2's 104 m lava gantry has a small fight.
  - g4 has a second crossing.
  - g8's charger is in view from the door.

## The flow table (the meter's bot, seeds 1-20, NIGHT45's game -> tonight's; Logs/feel/n46-flow/freeze/table.md)
| level | runs finished | dead time (nothing to fight) | clear time | worse anywhere? |
|---|---|---|---|---|
| 1 | 16 -> 20 / 20 | 53.3 -> 30.2 s | 210.6 -> 137.9 s | no |
| 2 | 20 -> 20 | 36.1 -> 32.4 s | 196.5 -> 184.8 s | no |
| 3 | 7 -> 19 / 20 | 26.1 -> 12.1 s | 218.5 -> 123 s | no |
| 4 | 20 -> 20 | 15.6 -> 6.2 s | 127 -> 96.3 s | no |
| 5 | 20 -> 20 | 11.4 -> 10.4 s | 191 -> 163.1 s | no |

## The research it rests on (Research/2026-10-17-wave46/)
- **The flow audit:** where the player stops in levels 1-5, ranked.
- **Arena layout:** 28 findings, mainly id's GDC 2018 talk on Doom's arenas, read in full. Small and dense beats big, no single round
  loop, cover you can jump over, see the arena and wave 1 before dropping in, and change the arena between waves, never mid-fight.
- **Feel and flow:** 32 findings: Celeste's coyote time, hitstop, trauma-based screen shake, Left 4 Dead's pacing, and the metrics the
  flow meter now uses.

## Not done or not verified
- **The hit / kill camera kicks are built but OFF.** In the blind A/B test they read the same as before.
- **The swap's 15-step gun-up and presses during a freeze** are next night's (they need sim timing).
- **THE KILN WYRM's length in a human's hands** is not measured (bot only).
- **The bell** is built from boxes; a real prop model would read better.
- **g3's own stall / dwell targets** were not met by the bot.
- **The state-change light (`shift46`) is built but OFF.** A room's lamp turns hot on its last wave and cyan when it clears; the
  furnace flares at the crumble. In the blind test the judges agreed on only 2 of 5 pairs: the v2 lamp change didn't show, and v6's
  added light lands at the screen centre.
- **Pad aim assist and the level-select previews** were framed (a546) but not judged.
- **Old disk:** I deleted the numbered reel frames of nights 42 and older (652,227 files) when free space hit 9 GB; summaries and named
  shots are kept, as DECIDE46 D-1 allowed. C: now has ~210 GB free.

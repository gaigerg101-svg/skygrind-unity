# a2: the verb pilots and the skip test

The round (r2):

from r2/verbs-pierce.txt:
    n65verb pierce: level n65-foundry (Design/levels/n65-foundry.json), room a2 "1.2 KESTREL", seed 1; pose (-3.8, 2.2, -41.3) yaw -36.7 from line65[0]; off switch none
    n65verb pierce: GREEN | room a2 | the charged shot hit 3 bodies, kills 3, stand-ins killed 3/3 | aim line65's first segment | pose line65[0]
from r2/verbs-skip.txt:
    n65verb skip: level Design/levels/n65-foundry.json, room a2, verb65 pierce, refuse pierce, seeds 1..3, cap 120.0 s; pilot Walk38Pilot + n46flow's lava rule + the ported autopilot (normal), not invulnerable
      seed 1: verb allowed: cleared yes in 20.1 s, deaths 0, falls 0, charged shots 4, STYLE above DULL 52.8 %, top BOLD | pierce refused: cleared yes in 19.9 s, deaths 0, falls 0, charged shots 0, STYLE above DULL 39.6 %, top COOL (presses refused 1044)
      seed 2: verb allowed: cleared yes in 28.0 s, deaths 0, falls 0, charged shots 5, STYLE above DULL 66.8 %, top BOLD | pierce refused: cleared yes in 24.6 s, deaths 0, falls 0, charged shots 0, STYLE above DULL 27.7 %, top COOL (presses refused 1064)
      seed 3: verb allowed: cleared yes in 22.4 s, deaths 0, falls 0, charged shots 3, STYLE above DULL 50.2 %, top BOLD | pierce refused: cleared yes in 20.7 s, deaths 0, falls 0, charged shots 0, STYLE above DULL 46.7 %, top BOLD (presses refused 980)
    n65verb skip: RED | room a2, verb65 pierce, refuse pierce | verb allowed: cleared 3/3, median 22.4 s, deaths 0, the verb used 12 time(s), STYLE above DULL 57.5 % | refused: cleared 3/3, median 20.7 s, deaths 0, STYLE above DULL 37.4 % | the room clears as well without the verb: it can be skipped (by this pilot)

The red (n52-foundry):

from Research/2026-10-23-wave65/T/verb-launch-red.txt:
    n65verb launch: level n52-foundry (Design/levels/n52-foundry.json), room a2 "1.2 KESTREL", seed 1; pose (0.0, 0.0, -37.0) yaw 0.0 from the room's spawn (no line65); off switch none
    n65verb launch: RED | no target: no husk-shaped body alive in a2 that launch36 can throw (alive: husk stand-in, husk stand-in, husk stand-in; stand-ins and tanks never fly)
from Research/2026-10-23-wave65/T/verb-parry-red.txt:
    n65verb parry: level n52-foundry (Design/levels/n52-foundry.json), room a2 "1.2 KESTREL", seed 1; pose (0.0, 0.0, -37.0) yaw 0.0 from the room's spawn (no line65); off switch none
    n65verb parry: RED | no target: no spitter in a2 in any wave within 40 s (6 bodies of earlier waves killed outright; alive now: husk, husk, husk)
from Research/2026-10-23-wave65/T/verb-pierce-red.txt:
    n65verb pierce: level n52-foundry (Design/levels/n52-foundry.json), room a2 "1.2 KESTREL", seed 1; pose (0.0, 0.0, -37.0) yaw 0.0 from the room's spawn (no line65); off switch none
    n65verb pierce: GREEN | room a2 | the charged shot hit 3 bodies, kills 3, stand-ins killed 3/3 | aim the nearest body, tracked (husk, a stand-in #1 at (0.0, 0.1, -44.0)) | pose the room's spawn (no line65)
from Research/2026-10-23-wave65/T/verb-skip-red.txt:
    n65verb skip: level Design/levels/n52-foundry.json, room a2, refuse pierce, seeds 1..5, cap 120.0 s; pilot Walk38Pilot + n46flow's lava rule + the ported autopilot (normal), not invulnerable
      seed 1: verb allowed: cleared yes in 5.2 s, deaths 0, falls 0, charged shots 1 | pierce refused: cleared yes in 13.0 s, deaths 0, falls 0, charged shots 0 (presses refused 752)
      seed 2: verb allowed: cleared yes in 11.1 s, deaths 0, falls 0, charged shots 2 | pierce refused: cleared yes in 11.1 s, deaths 0, falls 0, charged shots 0 (presses refused 611)
      seed 3: verb allowed: cleared yes in 15.4 s, deaths 0, falls 0, charged shots 2 | pierce refused: cleared yes in 12.5 s, deaths 0, falls 0, charged shots 0 (presses refused 685)
      seed 4: verb allowed: cleared yes in 14.9 s, deaths 0, falls 0, charged shots 4 | pierce refused: cleared yes in 9.1 s, deaths 0, falls 0, charged shots 0 (presses refused 464)
      seed 5: verb allowed: cleared yes in 14.9 s, deaths 0, falls 0, charged shots 4 | pierce refused: cleared yes in 9.1 s, deaths 0, falls 0, charged shots 0 (presses refused 493)
    n65verb skip: RED | room a2, refuse pierce | verb allowed: cleared 5/5, median 14.9 s, deaths 0, the verb used 13 time(s) | refused: cleared 5/5, median 11.1 s, deaths 0 | the room clears as well without the verb: it can be skipped (by this pilot)
from Research/2026-10-23-wave65/T/verb-skip-verbs65-red.txt:
    n65verb skip: level Design/levels/n52-foundry.json, room a2, verb65 pierce (planted, bot43 false), refuse pierce, seeds 1..5, cap 120.0 s; pilot Walk38Pilot + n46flow's lava rule + the ported autopilot (normal) + the verb-aware layer (--verbs65) in the allowed run, not invulnerable
      seed 1: verb-aware: cleared yes in 12.2 s, deaths 0, falls 0, charged shots 4, STYLE above DULL 64.5 %, top COOL, trigger held 683 steps | pierce refused: cleared yes in 13.0 s, deaths 0, falls 0, charged shots 0, STYLE above DULL 81.8 %, top BOLD (presses refused 752)
      seed 2: verb-aware: cleared yes in 15.6 s, deaths 0, falls 0, charged shots 5, STYLE above DULL 65.0 %, top COOL, trigger held 1156 steps | pierce refused: cleared yes in 11.1 s, deaths 0, falls 0, charged shots 0, STYLE above DULL 69.9 %, top COOL (presses refused 611)
      seed 3: verb-aware: cleared yes in 10.4 s, deaths 0, falls 0, charged shots 3, STYLE above DULL 58.1 %, top COOL, trigger held 680 steps | pierce refused: cleared yes in 12.5 s, deaths 0, falls 0, charged shots 0, STYLE above DULL 81.1 %, top BOLD (presses refused 685)
      seed 4: verb-aware: cleared yes in 13.6 s, deaths 0, falls 0, charged shots 4, STYLE above DULL 37.3 %, top COOL, trigger held 877 steps | pierce refused: cleared yes in 9.1 s, deaths 0, falls 0, charged shots 0, STYLE above DULL 74.2 %, top BOLD (presses refused 464)
      seed 5: verb-aware: cleared yes in 12.2 s, deaths 0, falls 0, charged shots 4, STYLE above DULL 70.7 %, top BOLD, trigger held 865 steps | pierce refused: cleared yes in 9.1 s, deaths 0, falls 0, charged shots 0, STYLE above DULL 79.5 %, top BOLD (presses refused 493)
    n65verb skip: RED | room a2, verb65 pierce, refuse pierce | verb-aware: cleared 5/5, median 12.2 s, deaths 0, the verb used 20 time(s), STYLE above DULL 59.0 %, trigger held 4261 steps | refused: cleared 5/5, median 11.1 s, deaths 0, STYLE above DULL 77.6 % | the room clears as well without the verb: it can be skipped (by this pilot)

The round's row (its plan at d602157a): verb65 pierce; the --verbs65 skip on the round's level (verbs65-skip.txt):

    n65verb skip: level Design/levels/n65-foundry.json, room a2, verb65 pierce, refuse pierce, seeds 1..5, cap 120.0 s; pilot Walk38Pilot + n46flow's lava rule + the ported autopilot (normal) + the verb-aware layer (--verbs65) in the allowed run, not invulnerable
      seed 1: verb-aware: cleared yes in 23.7 s, deaths 0, falls 0, charged shots 7, STYLE above DULL 62.0 %, top AIRBORNE, trigger held 1871 steps | pierce refused: cleared yes in 19.9 s, deaths 0, falls 0, charged shots 0, STYLE above DULL 39.6 %, top COOL (presses refused 1044)
      seed 2: verb-aware: cleared yes in 13.5 s, deaths 0, falls 0, charged shots 4, STYLE above DULL 93.1 %, top BOLD, trigger held 984 steps | pierce refused: cleared yes in 24.6 s, deaths 0, falls 0, charged shots 0, STYLE above DULL 27.7 %, top COOL (presses refused 1064)
      seed 3: verb-aware: cleared yes in 20.0 s, deaths 0, falls 0, charged shots 6, STYLE above DULL 39.4 %, top AIRBORNE, trigger held 1459 steps | pierce refused: cleared yes in 20.7 s, deaths 0, falls 0, charged shots 0, STYLE above DULL 46.7 %, top BOLD (presses refused 980)
      seed 4: verb-aware: cleared yes in 22.9 s, deaths 0, falls 0, charged shots 7, STYLE above DULL 63.3 %, top BOLD, trigger held 1315 steps | pierce refused: cleared yes in 18.3 s, deaths 0, falls 0, charged shots 0, STYLE above DULL 36.5 %, top COOL (presses refused 958)
      seed 5: verb-aware: cleared yes in 12.8 s, deaths 0, falls 0, charged shots 3, STYLE above DULL 92.8 %, top BOLD, trigger held 663 steps | pierce refused: cleared yes in 22.7 s, deaths 0, falls 0, charged shots 0, STYLE above DULL 40.3 %, top COOL (presses refused 1161)
    n65verb skip: RED | room a2, verb65 pierce, refuse pierce | verb-aware: cleared 5/5, median 20.0 s, deaths 0, the verb used 27 time(s), STYLE above DULL 66.2 %, trigger held 6292 steps | refused: cleared 5/5, median 20.7 s, deaths 0, STYLE above DULL 37.8 % | the room clears as well without the verb: it can be skipped (by this pilot)

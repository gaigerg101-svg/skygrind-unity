# NIGHT11 report: the run's shape (2026-10-03, 2:07 PM - about 5:15 PM; your 8-hour cap, finished early)

**For GG.** A Survival run now has a shape: a first goal (the power switch), special rounds (HOLD, GOLD RUSH, and swarm
rounds that end in a drop), a main quest (THE PROTOCOL), traps, and an ending you choose (exfil at round 11). RESULTS
grades the run. Everything below passed its tests and a scripted run in the built game; **nobody has played it yet**.

## What shipped (tag `u3-n11`; commits in `NIGHT11-LOG.md`)

| What | Kind | Commits |
|---|---|---|
| **Objective rounds**: HOLD (rounds 6, 14, 22 ...: stand in a 4 m ring for 30 s; the round can't clear until held; points x1.5 inside, +50 x round) and GOLD RUSH (7, 11, 19 ...: kills x2, chains of 3 in 3 s pay a bonus) | web port, golden | `9b41520`, `c25a3e4`, `24b5b55`, `d1f7ff7` |
| **THE PROTOCOL**: four relays on the movement showpieces wake at round 6; light them (hold E); the SURGE; clearing it forges every gun you own free and pays 2500; a hidden fifth relay | web port | `4728ba2` |
| **Traps**: SHOCK PLATE 750 (LANDING), CRUSHER 1000, GRAVITY FAN 900, ARC PYLON 1250 (SLAB GARDENS) | web port, golden | `71373c8`, `29d495f` |
| **The run's grade**: S..D, best moments, the score line on RESULTS | web port, golden | `fd94a26` |
| **The power switch** (ON): HEIGHTS' west skybridge; the traps and the relays need it | new design | `de3c622` |
| **Swarm rounds** (8, 13, 18 ...) open with a banner; their last kill drops a power-up (ON) | new design | `422e0bc` |
| **Exfil** (ON): a radio at the spawn before rounds 11, 16, 21 ...; reach the beacon in 90 s, then 35 kills in 85 s; RESULTS says EXFIL or EXFIL FAILED | new design | `77340b5`, `c99397d` |
| **ULTRA HEALTH** in the LAB (OFF): ULTRAKILL's hard damage (a grey cap on the HP bar) and a close-range blood heal | new design, LAB | `68ab3c6` |
| **Allocation debt**: the Player's managed growth roughly halved | debt | `b8d37da` |

With every new flag off, the game is the web's: the golden sets and the replays never read the design config.

## Numbers (all measured tonight)

- 437 / 437 tests, 22 / 22 replays, 90 / 90 golden; the oracle is 34 vector sets (31,481 records), 0 mismatches in the
  Editor, the harness and the built Player; the web side's `verify.mjs`: `total: 0 mismatches`.
- The Player's smoke: `DONE: all steps passed` in 86 s, including the power, a trap kill, a HOLD held (3600 / 3600
  steps), THE PROTOCOL completed and an exfil at round 11 (grade C).
- Bench (60 s, 24 husks, Balanced on AC): median 2.47 ms, worst 1% 5.55 ms, 384 fps; managed memory +54,476 KB with 5
  collections (NIGHT10: +103,916 KB, 16). The cause was the ARMORY wall's signs rebuilt every frame and a config lookup
  per step.

## Partial, and why

- THE PROTOCOL's **finishes** (cosmetic unlocks) aren't ported; the SURGE's chargers, lancers and wardens play as husks
  until the enemy roster night (N13).
- The ARC PYLON's lightning is a simple zig-zag line (the web's arc FX isn't ported).
- The power switch isn't against a wall: no HEIGHTS spot that you can walk to was (you reach it with pad 2's launch).
- Exfil's third site (round 21) is a HEIGHTS spire enemies can't reach.

## The 10-minute play checklist (debug: F9 skips to the next round, F8 opens every gate)

1. **Power switch:** buy SLAB GARDENS and HEIGHTS, find the switch (a cyan beacon stands over it) and hold E.
   *Is the spot right: findable, worth the trip?*
2. **A trap:** after the power, buy the SHOCK PLATE at LANDING's switch and lure husks onto it. *Does TRAPPED feel
   worth 750?*
3. **HOLD:** F9 to round 6 and stand in the ring. *Does the ring read, and is 30 s too long?*
4. **GOLD RUSH:** play round 7. *Do the chains feel rewarding?*
5. **THE PROTOCOL:** with the power on, at round 6 follow the guide column to the four relays. *Is the guide enough to
   find each climb?*
6. **Swarm drop:** F9 to round 8, clear it. *Does the drop feel like CoD's Max Ammo?*
7. **Exfil:** F9 to round 11's break, call it at the radio by the spawn, reach the beacon, get 35 kills. *Too hard
   solo, or too easy?*
8. **The grade:** read RESULTS after a death and after an exfil. *Is the grade fair?*
9. **ULTRA HEALTH:** turn it on in SETTINGS > LAB, play two rounds close in. *Better or worse than regen alone?*

## What I need from you (none blocks NIGHT12)

D-25 the power switch's spot, D-26 exfil's tuning, D-27 whether free buys should spend a pending REFUND, D-18 ULTRA
HEALTH as the default; details in `HANDOFF.md` section 5. Next: `NIGHT12.md`: the eight POWER guns, the hotbar and
radial, THE FORGE tiers, and save-and-quit.

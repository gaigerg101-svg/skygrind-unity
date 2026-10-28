# THE FARM: 300 AUTOPILOT runs

- **JSONL:** 300 lines in farm-easy-1.jsonl, farm-normal-1.jsonl, farm-hard-1.jsonl, farm-hard-51.jsonl; **runs in this report: 300** (equal to the line count).
- **Brain:** botBrain ×300. **Build:** SKYGRIND 0.1.0 (4e718c6-dirty) ×300. Written 2026-10-05 16:01 UTC by `Tools/farm-report.cjs`.
- **The page:** [farm-n14.html](farm-n14.html) (self-contained: the charts are inline SVG).
- Real HP, no god mode (NIGHT14 rule 6); the meta records nothing (rule 7). A "timeout" is a run alive at `design.farm.maxMinutes`.

## Observations for GG

Observations only: GG owns enemy design and the numbers. Nothing here is a change.

- easy: 100 runs reached a median round 6.0 (mean 6.70, max 14); the most common last round is 6 (42 runs, 42%). The most common cause of DOWN is HUSK (49 of 91 downs).
- normal: 100 runs reached a median round 6.0 (mean 9.52, max 26); the most common last round is 6 (44 runs, 44%). The most common cause of DOWN is HUSK (32 of 87 downs).
- hard: 100 runs reached a median round 14.0 (mean 12.66, max 26); the most common last round is 6 (42 runs, 42%). The most common cause of DOWN is ACID (31 of 81 downs).
- 41 of 300 runs hit the sim-time cap alive (a stall, not a win): round 6 ×37, round 7 ×2, round 10 ×1, round 26 ×1.
- Kills by gun: shotgun 43%, nailgun 29%, revolver 26%, shockwave 2% of 74510 kills.
- 55 of 300 runs bought no gun and no perk.
- The first perk came in round 6.3 on average (189 runs bought one).
- Points left unspent at the end: mean 24818 of a mean 32492 earned.
- 38 of the 41 timeouts ended in a HOLD round with the ring not held (objectives.js: the round can't clear until it is).
- 173 of 259 downs came in a HOLD round.

## Per difficulty

| difficulty | runs | mean round | median | max | mean sim time (s) | timeouts | exfil | mean kills | accuracy |
|---|---|---|---|---|---|---|---|---|---|
| easy | 100 | 6.70 | 6.0 | 14 | 616.5 | 9 | 0 | 124.6 | 20% |
| normal | 100 | 9.52 | 6.0 | 26 | 747.5 | 13 | 0 | 231.5 | 35% |
| hard | 100 | 12.66 | 14.0 | 26 | 1005.7 | 19 | 0 | 389.0 | 47% |

## Round reached (runs per round)

| difficulty | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 | 12 | 13 | 14 | 15 | 16 | 17 | 18 | 19 | 20 | 21 | 22 | 23 | 24 | 25 | 26 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| easy | 4 | 11 | 3 | 6 | 1 | 42 | 7 | 0 | 4 | 3 | 7 | 3 | 0 | 9 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| normal | 0 | 1 | 3 | 5 | 1 | 44 | 3 | 0 | 1 | 4 | 2 | 3 | 0 | 22 | 1 | 1 | 2 | 0 | 2 | 0 | 1 | 3 | 0 | 0 | 0 | 1 |
| hard | 0 | 0 | 0 | 0 | 0 | 42 | 1 | 0 | 1 | 0 | 3 | 0 | 0 | 25 | 0 | 1 | 0 | 0 | 1 | 1 | 2 | 20 | 0 | 0 | 0 | 3 |

## Survival curve (share of runs that reached the round)

| difficulty | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 | 12 | 13 | 14 | 15 | 16 | 17 | 18 | 19 | 20 | 21 | 22 | 23 | 24 | 25 | 26 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| easy | 100% | 96% | 85% | 82% | 76% | 75% | 33% | 26% | 26% | 22% | 19% | 12% | 9% | 9% | 0% | 0% | 0% | 0% | 0% | 0% | 0% | 0% | 0% | 0% | 0% | 0% |
| normal | 100% | 100% | 99% | 96% | 91% | 90% | 46% | 43% | 43% | 42% | 38% | 36% | 33% | 33% | 11% | 10% | 9% | 7% | 7% | 5% | 5% | 4% | 1% | 1% | 1% | 1% |
| hard | 100% | 100% | 100% | 100% | 100% | 100% | 58% | 57% | 57% | 56% | 56% | 53% | 53% | 53% | 28% | 28% | 27% | 27% | 27% | 26% | 25% | 23% | 3% | 3% | 3% | 3% |

## Causes (the recap's killer name; "timeout", "exfil")

| cause | runs | easy | normal | hard |
|---|---|---|---|---|
| HUSK | 110 | 49 | 32 | 29 |
| ACID | 71 | 16 | 24 | 31 |
| timeout | 41 | 9 | 13 | 19 |
| SWARMER | 40 | 10 | 21 | 9 |
| SPITTER GLOB | 38 | 16 | 10 | 12 |

## Kills by gun

| gun | kills | share | easy | normal | hard |
|---|---|---|---|---|---|
| shotgun | 32250 | 43% | 4519 | 9711 | 18020 |
| nailgun | 21236 | 29% | 2322 | 6088 | 12826 |
| revolver | 19422 | 26% | 5307 | 6709 | 7406 |
| shockwave | 1602 | 2% | 314 | 644 | 644 |

## Points per round (mean earned / spent over the runs that reached the round)

- **easy:** R1 678 / 0, R2 896 / 23, R3 1167 / 732, R4 1571 / 1098, R5 1904 / 1489, R6 1910 / 1036, R7 5028 / 1270, R8 2726 / 548, R9 3032 / 373, R10 3512 / 1010, R11 6715 / 1849, R12 3758 / 1283, R13 4468 / 0, R14 914 / 803
- **normal:** R1 766 / 0, R2 981 / 34, R3 1192 / 598, R4 1612 / 1210, R5 2065 / 1463, R6 2031 / 1268, R7 6440 / 1073, R8 2949 / 799, R9 3675 / 516, R10 3819 / 1383, R11 10226 / 1325, R12 4810 / 862, R13 4925 / 0, R14 3494 / 635, R15 5979 / 1038, R16 6394 / 430, R17 6404 / 358, R18 6805 / 0, R19 13934 / 971, R20 8967 / 1456, R21 8252 / 2000, R22 3814 / 250, R23 8259 / 0, R24 9628 / 0, R25 12741 / 0, R26 2021 / 0
- **hard:** R1 759 / 0, R2 893 / 19, R3 1304 / 1100, R4 1997 / 1450, R5 2494 / 1671, R6 2194 / 1301, R7 7598 / 789, R8 3104 / 708, R9 4188 / 640, R10 4774 / 1799, R11 11851 / 1382, R12 5683 / 347, R13 5046 / 494, R14 4640 / 934, R15 7055 / 1245, R16 7199 / 729, R17 7859 / 305, R18 6827 / 296, R19 19130 / 832, R20 9317 / 855, R21 9887 / 380, R22 4528 / 0, R23 8998 / 0, R24 13328 / 0, R25 13860 / 0, R26 5925 / 0

## Buy timing (perks, guns, THE FORGE, gates: the round bought)

| item | times bought | share of runs | mean round | first | last |
|---|---|---|---|---|---|
| gate:armory | 239 | 80% | 3.3 | 2 | 9 |
| gun:shotgun | 215 | 72% | 3.6 | 2 | 14 |
| gun:nailgun | 203 | 68% | 4.3 | 3 | 19 |
| perk:flip | 52 | 17% | 4.9 | 3 | 6 |
| gate:slabs | 245 | 82% | 5.5 | 4 | 7 |
| perk:slipstream | 16 | 5% | 6.8 | 5 | 21 |
| gun:rocket | 131 | 44% | 6.8 | 4 | 20 |
| gate:heights | 160 | 53% | 7.0 | 5 | 10 |
| perk:vigor | 161 | 54% | 7.0 | 4 | 19 |
| perk:quick | 86 | 29% | 10.6 | 6 | 16 |
| gate:grinder | 79 | 26% | 10.7 | 6 | 20 |
| perk:mend | 74 | 25% | 11.3 | 6 | 20 |
| perk:deadeye | 54 | 18% | 13.0 | 7 | 20 |
| gun:rail | 28 | 9% | 16.0 | 11 | 21 |
| gate:islands | 16 | 5% | 17.6 | 12 | 22 |
| perk:grip | 6 | 2% | 18.7 | 15 | 21 |

MODS taken (the draft's first card, design.autopilot.takeFirstMod): variety ×242, skydancer ×223, adrenaline ×210, bountyHunter ×201, lance ×199, scavenger ×198, bloodPrice ×181.

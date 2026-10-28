# NIGHT29-PROOF: the proof lines from tonight (each from a run this session; logs under Logs/, untracked)

Format: item | RED (the old game or the switch off) | GREEN (the new) | where seen.

| Item | RED | GREEN | Seen on |
| --- | --- | --- | --- |
| 1 level select | `-skygrindOff29`: 4 PASS / 16 FAIL of 20 real-reload presses (flow Title after 4.0 s) | 20 / 20 PASS (n29-1 on the old ids; n29-2 and n29-4 on the new ids) | Player (`-skygrindLevelSelect29`, Logs/n29-1-ls29-red.log / green.log, n29-2-ls29.log, n29-4-ls29.log) |
| 2 lights | a bad plan (floating light, 0.4 m deep light) trips `lights` | `levelgen` 0 errors on foundry / iceline / kiln; wall glow strips, ceiling panel and a framed door in the f10 still | harness + Editor still (Logs/shots/t29b-n29-foundry-9-f10.png) |
| 3 props | a loose prop trips `props` | Props29Tests 1/1; authored `props` rows (foundry 39, kiln 18) | harness; the props are NOT seen in a Player |
| 4 CRT | shader compile error (`Hash` redefined by URP): fixed | strength 0 / 0.35 / 1.0 stills at 2560x1440 looked at: visible warp, grain, vignette; HUD straight | Editor (Logs/shots/crt29-*.png); cost NOT conclusive (bench spread 3.25-7.6 ms) |
| 5 doors | n/a | `doorFit` rows PASS; door frames built from the wall opening | harness + the f10 still |
| 6 wall-ride room | `standins29 --off`: 0 stand-ins | `standins29`: a shot from the floor leaves hp 40, a shot while wall riding lands (40 -> 15); the lesson `wallRiddenAndFired`; f5 cleared by walk29 in 10 / 10 runs, median 23.3 s | harness |
| 7 boss HP | Lantern 0 of 3 killed in 300 s (old numbers) | Lantern x0.285 (after reconciliation), worm x0.70 (5:09 -> 3:40), Glacier unchanged | harness `boss29 hp` (before the Lantern retune) |
| 8 lockout | `lockout29 --off`: 422 overlaps in 10,000 random input steps | 0 overlaps (503 melee presses, 975 trigger steps dropped, 116 switches waited) | harness only (no Player pad line) |
| 9 closed rooms | n/a | `closedRoom` rows PASS; stills of f1, f5, f10, g1, k1 | harness + Editor stills |
| 10 shotgun | 63 steps (0.525 s) | 82 steps (0.683 s), pump 0.45 s unchanged | harness `shotgun29` |
| 11 unlocks + tank | `tank29 --off`: a plain 40 hp husk | 600 hp, one gauntlet hit (2592), 166 pistol shots; unlock table test: levels 1-2 nothing, 3 THUNDERCLAP | harness + Systems29Tests 16/16 |
| 12 Lantern | old Filter: 0 damage unless the face looks at the ridden wall | Player reel `lantern29`: rider KILLED 43.8 s (band 45-70: 1.2 s under, a perfect pilot); floor camper projected 137 s (3.1x); live floor glows red-orange, world slow 0.55 | Player (Logs/feel/n29-5/bosses) |
| 13 Glacier + skate | old Filter: the pistol never hurt it; the first Player pilot kill 49.6 s with frost 0 / charges 0 | Player reel `glacier29`: killed 63.6 s by the perfect pilot (band 80-120 is for a lag bot: harness median 106 s, 10 of 10), frost 1, charges 3, lane lock 85 %, arena mid-frame luminance 0.075-0.122 (was about 0.45) | Player (Logs/feel/n29-6/bosses, n29-5) |
| 14 DOORFREE | `-skygrindNoHold29`: 1 of 12 zones open, FAIL | a 3.6 s hold: 12 of 12 open, DoorFreeOn, Unranked29, a fight room still spawned its wave: PASS (n29-3 and n29-4) | Player (`-skygrindDoorFree29`); the real RB / F8 not pressed |
| level 1 | - | `levelgen` 0 errors (35 rows); walk29: 11 of 11 rooms in 10 of 10 runs, median 107.7 s (lead re-run 3 of 3) | harness, real campaign sim, bosses excluded |
| level 2 | - | `levelgen` 0 errors (24 rows); walk29 7 of 7 in 10 of 10, median 106.9 s | same |
| level 3 | - | `levelgen` 0 errors (15 rows); walk29 4 of 4 in 10 of 10, median 54.2 s | same |

Gates at the freeze: `fingerprint` EQUAL and `fingerprint --off29` EQUAL (the composed fingerprint runs with every NIGHT29 block off: `FingerprintBaseline.N29OnPath` is false, no rows-on baseline was recorded); `tape29` PASS (replay EQUAL 6 of 6, 3600 steps); golden 97 / 97 and trajectories 22 / 22 (Editor, run at several commits); `Mover.cs` untouched; one full suite (result in NIGHT29-LOG.md).

Player smoke (n29-7, rows on): 1 step failed, LONGWAVE ("kill weapons [rocket]": the new lockout makes a weapon switch wait for the rocket's 1.5 s cycle); with `-skygrindOff29` the smoke passed every step ("all steps passed", 197 s). The smoke step was changed to idle 2 s first (commit after the build) and NOT re-run in a Player (7-build cap).

Not verified by anyone: that a human can play any level; the rooms' feel; the props, hazard pulse and pickups in a Player; the CRT cost; the real RB hold; a pad line for the lockout; the five-lane arena (three lanes built); whether the tank rooms (i1, i6: 9 and 16 pilot deaths in 10 seeds) are too hard.

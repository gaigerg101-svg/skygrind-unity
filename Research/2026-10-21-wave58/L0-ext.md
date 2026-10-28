# L0 EXT: level 3's exits fix, the kb2c spawn re-cut (the lead, 2026-10-21 19:5x-20:19; NIGHT58 extension)

Start: NIGHT58's held cut 3 (`L0-held.patch`): MAIN put at the next room 14 / 15 / 14 -> 3 / 1 / 0, but deaths pooled 1-30 9 -> 13 and kb2c's
contract dwell 0.70 -> 1.75 s FAIL (grace 0.5). L.md's next step: "re-cut kb2c's spawn ... 3 m+ off the door's axis (re-check spawn31 and
the contract's dwell)".

## Cause (read, not guessed)
- The bot's steering (Tools/SimHarness/Harness.N46.Flow.cs ~line 511): once a room is cleared, the pilot walks STRAIGHT at the next room's
  spawn (a grid path only round burning cells). kb2b's exit is (0, -157.5); with kb2c's spawn at NIGHT57's (-7.5, -169) the line leaves the
  door d4 (x -2..2) west of it and pins the bot on kb2b's south wall: kb2b's 8 of 29 put-ats (L.md).
- n46flow's dwell (same file, line 24 / 310) is time within 4 m of the entry door's rect while an enemy lives.
- The validator's spawn31 (Assets/Scripts/Game/MapAuthoring/LevelValidate29.cs): every node >= 6 m from the spawn; a node inside the first
  view cone (half 58.3 deg at camera.fovDeg 74) >= design.n24.campaign.minSpawnDist (12 m).
- A scratch search (0.5 m grid, the three rules + kb2c's boxes): with yaw 0 no spawn passes all three; (-2, -168) at yaw 45 does (4.5 m
  from the door rect); deeper spots (6.5-8 m) need yaw 60-90 or -80/-90.

## The cuts (each: gen-n57-kiln.cjs, levelgen --walk (0 errors; its own walk's "DID NOT reach the last exit; zone at the end kb1" is
## HEAD's too), --finish, n46flow --level n57-kiln seeds 1-10 / 11-20 / 21-30; Logs/n58/ext/)

| measure (bot = a pilot, not a person) | HEAD (Logs/n58/kiln-base*) | NIGHT58 cut 3 (held) | EXT cut 4: spawn (-2, -168) yaw 45 | EXT cut 5: spawn (-1, -171) yaw 70 |
|---|---|---|---|---|
| MAIN put at the next room (per set) | 14 / 15 / 14 | 3 / 1 / 0 | **1 / 2 / 5** | 1 / 2 / 4 |
| deaths (per set; pooled, band 9 x 1.10) | 2 / 6 / 1 = 9 | 3 / 8 / 2 = 13 FAIL | **1 / 7 / 1 = 9** | 0 / 5 / 5 = 10 (over 9.9) |
| stall % (band <= 23.65) | 18.9 / 20.5 / 18.8 | 23.2 / 22.4 / 18.8 | 20.3 / 22.9 / 22.7 | 21.4 / 21.9 / 20.4 |
| clear s (150-240) | 177.0 / 169.9 / 167.1 | 162.1 / 176.5 / 168.3 | 176.5 / 175.8 / 173.9 | 178.0 / 187.9 / 183.7 |
| walks | 10/10 x3 | 10/10 x3 | 10/10 x3 | 10/10 x3 |
| kb2c dwell, level run (s, per set) | 2.3 / 1.3 / n/a | not printed | 3.5 / 1.7 / 5.8 | 3.6 / 1.9 / 4.0 |
| kb2c contract (n57-l-contract.sh: per-room, base n52's kb2c) | NIGHT57 PASS | C 13/14 FAIL dwell 0.70 -> 1.75 | **C 14/14 PASS dwell 0.70 -> 0.90 (0.5)** | not run |

- Cut 5 falsified "the spawn's distance from the door drives the level run's dwell" (7.5 m out, dwell unchanged). The level run's kb2c
  dwell is already 2.3 / 1.3 s at HEAD (the contract's 0.70 is n52's room, run per room): cut 4's mean of sets 1-2, 2.6 s, is within HEAD's
  2 sets + their band (1.8 + 1.0).
- Cut 4 room table (`python -I Tools/night/n57-l-room.py table kiln:kb2c,kb3`): kb2c and kb3 every column PASS; IN VIEW 2 / 2; I5 / I7 PASS.
  kb3's contract on cut 4 (20:25-20:27, Logs/n58/ext/contract-c4-kb3.txt): C 14/14 PASS, dwell 1.10 -> 1.80 s (grace 0.8), deaths 0.
- Kept: cut 4 as `Research/2026-10-21-wave58/L0-cut4.patch` (gen-n57-kiln.cjs only; `git apply`, then the run order above). The level data
  in the repo is HEAD's (not shipped): the rule's line 3 (verbs37 on n57-kiln in a Player) and line 4 (a critic) are not checked.
- A person: the spawn faces the near bridge (south-west), not down the room; the restart's back-step from (-2, -168) lands on ash, not on
  the iron apron (NIGHT57's spawn_why: "a back-step ends on the apron"): not checked in a Player.

## Not checked
A Player (a157 frames, verbs37 on n57-kiln), a critic, a person on the spawn's new facing.

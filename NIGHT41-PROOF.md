# NIGHT41-PROOF: every number behind the freeze (measured tonight; "not checked" where it was not)

Freeze: port/stage3 at the config commit df379618 (13 / 18 rows ON) + the close docs; Player build `Builds/n41-r7` (13:28, Succeeded,
0 errors; its config copy also had select41 on for the LevelSelect29 check only). Logs: Logs/feel/n41-r1..r7/summary41.txt,
Logs/n41-suite-freeze.txt, Logs/player/n41-*.log.

## Sim parity (Tools/night/n41-gates.sh at df379618, 13:47)
- Golden: 39 vector sets (48094 records), 22 replays, 22 exact; 0 failing, 0 mismatches.
- Fingerprint seed 1: EQUAL to the baseline plain and with `--off41` (FingerprintBaseline.N41OnPath false: the composed fingerprint runs
  with every NIGHT41 block off, as NIGHT37-40 did).
- Every integration (10:21, 10:43, 10:50, 11:21, 11:35, 12:03, 12:31, 12:57, 13:27) passed the same gates plus an Editor compile with 0
  errors.

## The suite (INVENTORY A6), in the live Editor, a batch per class letter (Tools/night/n41-suite.sh)
- Skygrind.Core.Tests: **1095 / 1095 passed**, 0 failed (13:09-13:22).
- Skygrind.Game.Tests: **SHORT**: A-L 527 / 528 passed (13:42-13:52; the 1 failure is NIGHT40's: Lesson25TextTests.EveryLevelPrompt_... '{grapple} HOOK THE KNEE. {grapple} PULL AWAY.' in n40-span.json is 7 words, the rule is 6; added by NIGHT40 B4 f7878a50, never caught because NIGHT40's suite never finished; an n40 file is not edited in place: the fix is an n41 text row or n41-span); M (MapChoice 58 + MapPipeline + 8 classes) and N (21 classes) each ran past run_tests' 600 s timeout twice (20 min each); P-Z: the lead's stop at 14:43 did not take (pkill matched no process), so the script kept calling run_tests until 15:03: P timed out (600 s, twice), R-Z each returned no JSON twice: no test past L produced a result, and those calls overlapped the bench runs. Next night: split M and N into smaller prefixes (Skygrind.Game.Tests.MapC, .Mo, .N3, .N4 ...).

## The verbs (docs/VERBS.md; reel verbs37 with every ON row on), before = NIGHT40's n40-r4, after = n41-r6 / n41-r7
| level | before | after |
|---|---|---|
| L1 | n38-foundry 26/26 | n41-foundry 26/26 (n41-r7) |
| L2 | n40-iceline 20/20 | n40-iceline 20/20 |
| L3 | n40-kiln 24/25 (13 RED) | n41-kiln 24/25 (13 RED: the reel's slide start, A11) |
| L4 | n40-span 30/30 | n40-span 30/30 |
| L5 | n40-spire 26/30 (17 / 19 / 22 / 23 RED) | n41-spire 30/30 |

## Rows (each: the check, the frames, the iteration)
- pump41On: weapons41 GREEN (n41-r1, again with the pad flag on n41-r6); the volley's pump phase -1 / -1 / 0.53 / -1 at +0 / 0.15 / 0.30
  / 0.45 s.
- bars41: frames n41-r1 (one bar, the 50 % tick), n41-r3 / r5 (the meter + label on all five; leg pips on THE LOOM / THE CROWN).
- look41: mean floor RGB of a fixed 500 x 80 rect, frames f0003 / f0005 / f0007 (within 0.1): L2 (150.1, 129.5, 119.3) -> (117.0, 132.7,
  162.6), B/R 0.795 -> 1.390; L3 (140.2, 98.0, 81.5) -> (140.3, 98.8, 65.6), B/R 0.581 -> 0.468; brightness within 1.2 %. Round 1 (the room
  keys only) moved no floor by more than 0.2 / 255: refuted, then the sun by theme. L1 / L4 / L5 after: not measured.
- spire41: verbs on n41-spire 30/30 twice (n41-r2, n41-r5 / r6); walk 10/10 126.2 s; gates 4/4; the pull's top y 13.8 -> 9.2.
- doors41On: doors41 frames on n41-r3 / r4 / r5 (locked, boss); the open (cyan) state on n41-r4 v2 f0005; doors38 reads the sim's zones.
- juice41On / entrance41: entrance41 GREEN on all five bosses (n41-r3 / r5), Juice38Probe shake 2.45, 2 flicker pulses; the static in
  brawl40casual (n41-r6 f0245 / f0251).
- sweep41: harness stay 10/10 (51.6 s), casual 10/10 (58.6 s) at 10 seeds (casual 19/20 at 20: the loss had no sweep hit); loom37 GREEN
  38.0 s on n41-r6, sweeps 2, frames legs 1 / 2 + the line.
- brawlLook41: frames n41-r6 b2c (slam, stuck, kill).
- foundry41: gates 2/2 with --ride40 (b2-g1 "ride chain L" +32 %, was "climb L" +156 %); the probe without W2: RED 0/2; walk b1-b2 10/10;
  level36 n41-foundry PASS 11/11.
- kiln41: `n41foes s1kiln` (= NIGHT40's `n40start kiln --start40`): 10/10 all off, 10/10 tokens41, 10/10 kiln41 + tokens41; gates 4/4.
- tokens41: `n41foes level` time above two wind-ups off -> on: n40-iceline 3.14 % -> 0 %, n40-span 0.61 -> 0, n40-spire 0.08 -> 0,
  n40-kiln 0.02 -> 0; verbs unchanged on L1-L5 with it on; row 40 PASS.
- heavy41: `n41foes heavy`: CHARGER hold 0.19 -> 0.34 s + 0.14 s hitstop; tank / WARDEN 0 -> 0.14 s hitstop; husk unchanged.
- OFF: aim41 (`n41pad real` hits per live second off -> on, weak / mid / expert pilot: THE ICEBREAKER 0.428 -> 1.891 / 0.892 -> 2.605 /
  2.284 -> 2.426, THE LOOM 0.629 -> 3.168 / 1.517 -> 3.160 / 2.130 -> 2.484; no Player frame of the lock), models41 (not judged), select41
  (LevelSelect29 PASS with it on; no frame).

## Player checks on n41-r7
- The smoke: PASS through round 11 (the power switch, SHOCK PLATE, THE PROTOCOL 31 s, the four relays, the HOLD ring 3600 / 3600, the
  exfil radio), 0 exceptions, 0 NRE.
- LevelSelect29: every press PASS, 0 exceptions, 0 NRE.
- Reels used: 49 / 50 (n41-r1 7, r2 4, r3 8, r4 7, r5 8, r6 13, r7 2); 0 NRE / 0 exceptions in all 49. Builds: 7 / 7 Players (two failed
  n41-r6 attempts made nothing).

## The bench (INVENTORY C8)
**3 runs, indicative** (n41-r7, `-skygrindBench 60 -skygrindMap n40-iceline`, 24 husks, 1920x1080, RTX 5070 Laptop, Balanced plan on AC; CPU 10-16 % before: the Editor's leftover test run, so not alone): models41 off median 3.24 / 3.34 ms, p95 5.87 / 6.72, p99 11.10 / 10.67 ms, mean 278 / 269 fps; models41 on median 3.35 ms, p95 6.25, p99 10.71 ms, 269 fps. models41 moves nothing past the off runs' own spread; GPU median 1.18 ms (the CPU main thread is the tail). Logs/player/n41-bench-n41-r7-*.log.

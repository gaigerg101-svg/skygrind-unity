# NIGHT22-PROOF.md: the proof ledger (row, scenario, RED line, GREEN line, date)

A row passes only when its contract's check printed its line on a Player build or harness THIS night. RED = the same scenario on the same
Player build with the row's switch off (`-skygrindOff21`, which also forces the N22 blocks off). Every scenario prints its preconditions
(SafeSpot (44.0, -16.0): nearest jump pad 23.7 m, nearest buy station 17.7 m, 2 guns owned). Filled at NIGHT23 (2026-10-07, 8:30-9:00 PM) from
`Logs/player/n23-reel-n23r1-{on,red,pad}.log` (Builds/n23-1) and `n23-reel-n23r2-on.log` (Builds/n23-2). NIGHT22's own reels never produced
a result (n22-4's Player threw a NullReferenceException in BuildCode22 at startup; fixed in 3af57a1).

| Row | Scenario | RED line (Player, row off) | GREEN line (Player, row on) | Passes |
| --- | --- | --- | --- | --- |
| arcline | arcline2 | `RED: design.n21.arcline is off (no ARCLINE to buy, no yank)` | keyboard and virtual pad Y: terminal bought 5500, 3 husks in a line, arclinePull 1, ropeAttach 1, peak 31.7 m/s, cooldown left 13.0 s, chip Ready / Cooldown 15 s / NoTarget (reticle alpha 0.40); M10 0, M-MAGENTA 0, S12 0 (pad) | true |
| stamp | stamp | `stamp none (roundGrade said 0x), grade card vs POINTS max overlap 30803 px2`; T11 meter 168 of 168 frames intersect | `stamp P (roundGrade said 1x, p=true 1x), entrance 233 ms, exit 183 ms, grade card overlap 0 px2 over 210 frames`; T11 meter 0 of 168; STAMP-T 5 of 5 in 200-300 ms (267 ms); M-MAGENTA 0 | true (M9: 4 pop-ins on older elements, see NIGHT23-LOG) |
| deadline | deadline | `RED: design.n21.deadline is off (a round with no bar)` | bar from frame 1, number 42 px, pulse 301 frames from 5.0 s, +N pops 3 (kills 3), burns 1, flashes 1; chase: round 10 46 % PASS, round 15 61 % FAIL (cap 50 %) | false (chase at round 15) |
| grind | grind | `SKIPPED (RED): design.n21.grind is off` | waveBreak 1x, WAVE banner 1x, telegraph 240 frames, death to RESULTS, best wave 1, `FIRST RUN ON RECORD`; harness grind2: records survive a restart, GRIND tape format 7 replays EQUAL | see NIGHT23-LOG (smoke step) |
| replay | replay | n22-4 / n23-1: `NO VIEW (RED): flow Down, offered False, clip 0 frames` (the clip was taken only inside a sim step) | n23-2: `VIEW offered, clip 'down' of 740 ring frames, began True, 1200 replay frames, ended on its own, back to Results`; harness replay2: camera error 0.0347 m worst of 3 seeds (limit 0.05); fingerprint EQUAL with the row on | true |
| challenge | code | `RunStats.mutators is [] (want both); x1/x1/x1; RESULTS line ""` | `typed 004P-PGW3-00KD: mutators [glassCannon, speedDemon], dealt x2 taken x2 pace x1.2, RESULTS "CODE 004P-PGW3-00KD"` (keyboard and pad); harness code2: same tape hash twice | true |
| ghost-daily | ghostdaily | `the ghost raced the DAILY before any DAILY finish (want none); RESULTS verdict ""` | `ghost before a finish none, after loaded, RESULTS "BEHIND YOUR GHOST BY 0.3 s (ROUND 1)"` | true |
| dusk-clock | dusk | share 0.00 / 0.00 / 0.00; LUM thirds 29.0 > 30.0 > 29.7 on n22-3 (flat; before the light fix) | share 0.00 / 0.43 / 1.00 at rounds 1 / 7 / 15 (not 8: THE HUNT's); LUM thirds 29.0 > 27.0 > 23.4 PASS; M-MAGENTA 0 on 918 frames | true (rounds 1 / 7 / 15) |
| haptics | haptics | `RED: design.n22.haptics is off` | pad pass: shockwave 3 steps, yank 2, attach 2, kill 1, down 1 (9 log lines with motor values and ms); VIBRATION off: log unchanged; keyboard pass: 0 logged | true |
| directed (leftover 1) | directed | `RED: design.n22.directed is off` | beacon round 11 step list 1 -> 3, current step cyan; pad pass shows the pad glyph; hunt not exercised (design.adds.hunt off) | true |

# NIGHT58-LOG (FINISH LEVEL 3, START LEVEL 4, LAND THE HITS; lead + builders L / G / V; t0 13:59:29 MST 2026-10-21, close by 15:44, hard cap 16:14)

## Timeline
- 13:59 t0. A0: the Editor ready (stopped, not compiling); C: 14 GB free (NIGHT58.md wants >= 20: DECIDE58 D-7).
- 14:0x **`git tag -l u3-n57` printed nothing; the repo config had kiln57 / wick55 / grade55 / colour56 OFF** (NIGHT57's close table and
  HANDOFF say ON). D-5: n57-freeze-rows.cjs with D-17's four reasons; n57-gates.sh GREEN 14:08:07 (Logs/n58/gates-d5.txt); commit
  14546f9a; tag `u3-n57` on it.
- 14:05 n57-prune.sh on n57-r1 / r2 / r3 / freeze: nothing to free (frames already every 50th). D-7: one reel set at a time, `df` first.
- 14:1x the baselines at 14546f9a (Logs/n58/): flow-base / -B (n52 foundry, iceline, span, spire; seeds 1-10 / 11-20) and kiln-base / -B
  (n57-kiln). n57-kiln: put at the next room 14 / 15, stall 18.9 / 20.5 %, deaths 2 / 6, clear 177.0 / 169.9 s. span: stall 14.5 / 13.8 %,
  deaths 0 / 2, clear 88.7 / 96.2 s. iceline idle > 20 %: 18.4 % of enemies.
- 14:12 NIGHT57's leftover suite job (Game N-Z by class, started in the NIGHT57 session) ended: 432 / 434; the 2 fails are pins fixed in
  the tree but not compiled when they ran (N55RoomClearTests, N57ColourTests); committed a424ca4d with n41-suite.sh.
- 14:20 the shared commit 7f88487c (n58-shared.cjs: span58 level row, maps.list row, placeholder n58-span.json, FeelReel.N58.cs,
  N58SharedTests, n58-gates.sh / prune / freeze-rows, the brief, slate.json). Gates GREEN 14:22:34 (Logs/n58/gates-shared.txt): flow --off58
  EQUAL to flow-base, config copies identical, golden 0 mismatches / 22 of 22 replays exact, fingerprints plain / --off57 / --off58 EQUAL,
  Editor compile clean, 0 console errors.
- 14:20-14:22 builders spawned one at a time in worktrees: L (L0 then L1), G (G1, G2, then G3), V (V1, V2, V3).
- 14:26-14:46 the Core suite (D-8): 1184 / 1186, 1 fail: N52SharedTests.TheOffSwitch_LeavesNight51sOrder (the NIGHT52 pin met kiln57
  ON since D-5: the slot holds n57-kiln, not n52-kiln); the pin fixed by the lead (a row still "swaps its level" when the slot left its
  levelFrom for a level on the same chain).
- 14:4x C: 14 -> 12 GB (the builders' dotnet builds); D-11: Logs/feel/n52-freeze (20.7 GB of raw frames) pruned to every 50th: 31 GB.
- 14:47 r1 pick: L 9517a00d (cause notes), G acd27b29 (G2 meter: press57 MISSED), V d330bab6 (V1 rim56). 14:51 gates GREEN
  (Logs/n58/gates-r1.txt); 14:51 Builds/n58-r1 (rim56 ON in its copy); 14:52-15:03 a357 on / off (-skygrindNoRim56) x 5 levels.
- 15:0x rim56 measured (n56-colour.py, Logs/n58/r1/colour-v1*.txt): bodies over the world's p90, off -> on: foundry 3 / 14 -> 6 / 14,
  n57-kiln 28 / 37 -> 26 / 36, iceline 17 / 17 -> 13 / 17, span 15 / 16 -> 15 / 16, spire 15 / 15 -> 15 / 15; overall 78 / 99 (78.8 %) ->
  75 / 98 (76.5 %): MISSED (want >= 90 %). V: the accent dim never reached the frame (env p90 V identical on / off).
- 15:01-15:09 L0 held: MAIN met (14 / 15 / 14 -> 3 / 1 / 0) but deaths pooled 1-30 9 -> 13 and kb2c's contract 13 / 14 (dwell 0.70 ->
  1.75 s, grace 0.5); D-12: the one critic reads the freeze build.
- 15:12 freeze pick: G 9a85ba45 (a258 probe) 1dd6d14b 185c252e, V cd504292 (exit lamp + a358) 208b7f99 (V2 yellow56) a6e63afc (lamp built
  but OFF: no measured MAIN), then L 8e0ad45f (L1 card + generator) 63f8dbed (L0 held notes). 15:13 Editor compile clean; 15:15
  Builds/n58-freeze (yellow56 ON in its copy) and its twin n58-freezek (kill55 ON only).

## CLOSE TABLE (16:09, 2 h 10 min after t0; the shipping rule of NIGHT57.md section 0; bot = a pilot, not a person)
| entry | state | MAIN before -> after | GUARDS before -> after | evidence | row / off switch |
|---|---|---|---|---|---|
| A0 / D-5 NIGHT57's close | DONE | u3-n57 missing; 4 rows off in the repo -> ON, tagged | gates GREEN 14:08 | Logs/n58/gates-d5.txt; 14546f9a | - |
| L0 n57-kiln exits | OFF (SHORT, held) | put at the next room 14 / 15 / 14 -> 3 / 1 / 0 (bot, seeds 1-10 / 11-20 / 21-30) | clear 177.0 / 169.9 / 167.1 -> 162.1 / 176.5 / 168.3 s; stall 18.9 / 20.5 / 18.8 -> 23.2 / 22.4 / 18.8 %; deaths 9 -> 13 pooled (MISS); kb2c contract 14/14 -> 13/14 (dwell 0.70 -> 1.75 s) | Research/2026-10-21-wave58/L.md, L0-held.patch | kiln57 (not in the data) |
| L0 in view at the door | SHORT | kb3c 3 / 6, kb3d 2 / 5 (n57-freeze a157) -> n/a (no data change shipped) | n/a | Logs/feel/n57-freeze/summary47.txt | kiln57 |
| L1 n58-span | SHORT | combat rooms 3 -> 3 (card + generator, no room); kinds 3 -> 3 | n/a | docs/ROOMS.md "Level 4 as n58-span"; Tools/night/gen-n58-span.cjs | design.n58.span58 (OFF) |
| G1 kill55 | OFF | marks at the flash end 0.41 % off -> 0.69 % on (want >= 5 %; Player a258; probe V 0.76 -> 0.53: drawn, lifted by colour / grade) | a354 jitter with kill55 firing: n/a (a354 never fires kill55) | Logs/feel/n58-freezek | design.n55.kill55 / -skygrindNoKill55 |
| G2 press57 | OFF | chosen stillness foundry 10.1 -> 7.7 / 7.9 -> 9.1, iceline 20.0 -> 20.4 / 19.3 -> 19.2, n57-kiln 6.4 -> 5.9 / 5.0 -> 4.8 % (want -5 points; bot) | deaths in spread; clear <= +2.6 %; moving HP / min down | Logs/n58/g/g2-*.txt | design.n57.press57 |
| G3 tokens56 idle | SHORT (cause only) | iceline idle > 20 %: 18.4 / 20.2 % (unchanged) | n/a | Research/2026-10-21-wave58/G.md | design.n56.tokens56 (ON) |
| V1 rim56 | OFF | over the world's p90 78 / 99 (78.8 %) -> 75 / 98 (76.5 %) (want >= 90 %) | colour56 cover 5 / 6 = 5 / 6 (foundry) | Logs/n58/r1/colour-v1*.txt | design.n56.rim56 / -skygrindNoRim56 |
| V2 yellow56 | OFF (no critic verdict: the critic died twice on API errors) | frames over 0.5 % non-parry yellow foundry 6 / 6 -> 0 / 6, spire 6 / 6 -> 0 / 6 (Player a358) | orbs on spire v5-mid 17.42 -> 15.80 %; verbs37 27 / 29 / 31 / 31 / 31 = w57 | Logs/n58/freeze/yellow-*.txt; Logs/feel/n58-freeze/summary47.txt | design.n56.yellow56 / -skygrindNoYellow56 |
| FREEZE | u3-n58 at ea053e54 | rows ON: NIGHT57's seven, none new | gates GREEN 15:18:30; verbs37 27 / 29 / 31 / 31 / 31; smoke 14 steps FAILED (582.6 s, run during a 30-min API outage; not re-run) | Logs/n58/gates-freeze.txt; Logs/n58/smoke-freeze.txt | Builds/n58-freeze (yellow56 off in its copy) |
| V3 exits lit | OFF (built, Exit58On false) | exit luma / room median: n/a (no meter by the freeze) | n/a | Exit58.N58V.cs; UnityTuning.Exit58On false | Exit58On / -skygrindNoExit58 |

## EXT (GG 2026-10-21 ~19:40: "keep building for next hour"; t0 19:40:02, the lead + one critic + builder L; DECIDE58 D-14..D-17)
- 19:43-19:47 the smoke on Builds/n58-freeze re-run (`n56-smoke-steal.sh n58-freeze`): **all steps passed, 236.9 s**, both planted steals
  took (Logs/n58/smoke-freeze-rerun.txt). NIGHT58's 14 fails (582.6 s) were the outage run; the build is as NIGHT57's smoke (232.3 s).
- 19:50-20:08 yellow56's critic (Research/2026-10-21-wave58/gate-x.md): **FAIL**: MAIN holds (0 / 12 frames over 0.5 %), but the GUARD
  breaks: foundry a5-entry's parry halo keeps 20 % of its yellow pixels on (4558 -> 918 px; the off side keeps 91 %): its V 0.84 sits on
  dimYellow58's vFull cut. yellow56 stays OFF.
- 19:55-20:19 L0 (the lead): kb2c's spawn re-cut on NIGHT58's held cut 3 (Research/2026-10-21-wave58/L0-ext.md). Cut 4 (-2, -168) yaw 45:
  put at the next room 14 / 15 / 14 -> 1 / 2 / 5, deaths pooled 9 -> 9, stall 20.3 / 22.9 / 22.7 %, kb2c contract 14/14 PASS (dwell
  0.70 -> 0.90, grace 0.5), room table PASS. Cut 5 (-1, -171) yaw 70: deaths 10, dwell unchanged (the spawn's distance is not the dwell's
  cause). Held as L0-cut4.patch (no Player verbs37, no critic); the repo's level data is HEAD's. Commit da9869b2.
- 19:51-20:20 builder L: THE GAP NEST in level 4's slot P2 (gen-n58-span.cjs slotP2; picks c5820f77 c13c145c 28a8287c): n46flow n58-span
  seeds 1-10: walks 10/10, stall 14.5 -> 13.4 %, deaths 0 -> 0, clear 88.7 -> 132.2 s (p2 now a fight), put at the next room 0 -> 3, p2
  falls 61. The lead took p2 out of NEW (56681aee: the header's room / identity / contract checks not run): n58-span.json = n52-span's
  beyond the names (--check exit 0); fingerprint plain / --off58 EQUAL.

| entry | state | MAIN before -> after | GUARDS before -> after | evidence |
|---|---|---|---|---|
| the smoke (n58-freeze) | DONE | 14 failed -> all steps passed (236.9 s) | steals took 2 / 2 | Logs/n58/smoke-freeze-rerun.txt |
| V2 yellow56 | OFF (critic FAIL) | frames over 0.5 % 12 / 12 -> 0 / 12 | foundry a5 parry halo 91 % -> 20 % of its yellow (FAIL) | gate-x.md |
| L0 kb2c spawn (cut 4) | SHORT (held patch) | put at the next room 14 / 15 / 14 -> 1 / 2 / 5 | deaths 9 -> 9; stall <= 23.65 %; kb2c contract 14/14; verbs37 / critic not checked | L0-ext.md, L0-cut4.patch |
| L1 THE GAP NEST | SHORT (built, out of NEW) | combat rooms 3 -> 4 with --slots=p2 (3 shipped) | walks 10/10; deaths 0 -> 0; put-at 0 -> 3; falls 61 | L.md "L1 EXT", Logs/n58/ext-L/ |

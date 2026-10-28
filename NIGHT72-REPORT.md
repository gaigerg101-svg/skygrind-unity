# NIGHT72-REPORT.md: SEEN (2026-10-27, t0 10:19:20, cap 7:30)

## 1. What is still not good (ranked; the lead's read, gg-proxy was not run)
1. The harness's ice is not the Player's ice. The flow base predated N5c (ice multiplier 0.25 -> 0.048281); the SimHarness keeps the web's ground friction 6 (it does not apply design.ultra71.set), so its iceline ice became five times slipperier while the Player's stayed at 1.5 /s. The close gate first read RED on iceline (Logs/n72/gates-flow-close-fail1.txt; N's diagnosis Research/2026-10-27-wave72/N/evidence/n72flow-iceline-diagnosis.txt); the base was re-made on HEAD and the gate is GREEN (16:08). The iceline flow lines now describe the harness's ice, not the game's.
2. Three rows were measured but stay OFF because a still cannot show them: spring72 (springs), cam72 (ULTRAKILL camera), finish55 (boss-kill kick). The night had no motion instrument the spotter could judge.
3. goal72 (the room's one line, GG's "OK now you have to go up") works in the reel but one stage's line ("The exit is AHEAD") has no capitalised verb: OFF.
4. The purple spire floor (palette72) is near THE RIVAL's magenta family; the kiln's lifted walls (lit72) are warm, near the danger family. Both are ON and flagged for GG.
5. The level rows (GG's play-build rooms, LV-*) were never paired: OFF. fixture61, exit63, hitzone47, reticle42, pump40, yellow56, decor55 stayed OFF with NOTHING OBVIOUS on their stills.
6. Flicker: L1's meter and L2's fix72 were built but never run in a Player; the flicker share is not measured.

## 2. GG's play card in short
Builds/n72-play (the freeze build, HEAD 9350c8631, the repo's config). ON: colours63, goal64, palette72, decor72, lit72, bossfx72, body72, btext72, and the movement profile's ground friction 31.07 with ice keeping its slide. PLAY72.md has each line, where to see it, its off switch and the keep / drop boxes.

## 3. What went ON (each with its pair and SPOT)
- colours63 (HV-colours63; C1 re-gate on t1): spire pair, SPOT "B has thin dark-red stripes ... A does not" (A = ON). Logs/n72/verdict-colours63-t1.md.
- goal64: spire row-alone pair, SPOT "the beam is orange in A and pale blue-white in B" (B = ON). Logs/n72/verdict-goal64.md.
- palette72 (C2): spire pairs t1 and t4, SPOT "olive green in A ... purple-violet and dark teal in B" (B = ON); foundry pair a cooler floor. Logs/n72/verdict-palette72.md.
- decor72 (C3 with C5's chevrons): t5 spire pair, SPOT "B has pale yellow chevron arrows painted on the ramps" (B = ON). Logs/n72/verdict-decor72.md.
- lit72 (L3): t2 kiln boss pair, dark share 57.3 % -> 5.1 % (the 15 % aim met on this frame), SPOT "B is much brighter, with an orange-lit ceiling and walls". Logs/n72/verdict-lit72-t2.md.
- bossfx72 (B2): t2 spire crown, the aim meter on five crown frames: aim 0.0 % (bar 5), coverage 6.2 % (bar 15), visible 4 of 5, danger share 7.9 %; OFF fails all four. SPOT "thin outline with spokes and sparks" vs "thick bright cyan band". Logs/n72/verdict-bossfx72-t2.md.
- body72 (B1): t3 kiln pair, changed-pixel hue meter median 25.4 deg (bar 35), yellow 1.5 % (bar 15); SPOT "a wider red-orange column under the central platform". Logs/n72/verdict-body72-t3.md.
- btext72 (B3): t4 kiln pair, SPOT "SWAP gauntlet, then SWING LEFT CLICK" vs the old ten-word line; the counter "boss lines over 6 words 0 frames". Logs/n72/verdict-btext72.md.
- Movement (N5, a value, not a row): ground friction / accel 31.07; verbs37 PASS on all five close sides of the freeze build; ice keeps its 4.766 m stop (N5c).

## 4. What failed or stayed OFF (with numbers)
spring72 (applied 61 ON vs 0 OFF, volume 1.0000; no SPOT); cam72 (dash FOV x0.976 ON vs x1.151 OFF; no SPOT); finish55 (kick 6.00 deg, FOV 3.50, bars 1.00 ON vs 2.2 deg OFF; blind spot NOTHING OBVIOUS); goal72 (stuck72 GREEN, stage 2 wording); fixture61, ssao55, exit63, hitzone47, reticle42, pump40, yellow56, decor55 (NOTHING OBVIOUS on their stills); the H4 boss group (wyrm71, voice71, words71, cyan71, bossText55: its text failed, superseded by btext72); the four LV level rows (not paired); C5's wall values (not named).
The t2 verbs37 REDs (iceline and kiln rows 40-42) were N1's retry lanes, fixed by N1b; the t3 and freeze sides are PASS.

## 5. Not checked
The flicker share on any level (L1 meter not run in a Player); the moving-camera flicker; the other bosses' lit72 (spire, foundry, iceline, span); glide braking on ice under the new accel; gg-proxy on the play frames; the skygrind-reviewer pass (REV); the play build's preview reels if not run by the close.

## 6. Your calls (DECIDE72.md)
The purple spire floor; the warm kiln walls; ice keeping its slide (done, N5c); mud's walk speed (8.49 m/s, no level has mud); the a3 ceiling lights removed in both foundries; the disk and the old worktrees.

## 7. NIGHT73's seed
- Diagnose the iceline --off67 flow difference first (the only RED gate).
- A motion instrument the spotter can judge (a short clip strip), so spring72, cam72 and finish55 can pass on what a player sees.
- goal72's stage-2 line, then ON. The LV level rows' own pairs (GG's play-build rooms).
- The flicker ranking with L1's meter and L2's fix72 in a Player.
- Estimate vs actual: the harvest took the H train plus five trains; 12 slices PASS, 38 SHORT.

## 8. The close lines
Slate: 51 slices: 12 PASS, 38 SHORT, PLAY at the close.
Close sides on Builds/n72-freeze (9350c8631): vf 27/27, vi 29/29, vk 30/30, vp 31/31, vs 31/31 PASS; the freeze smoke DONE: all steps passed (Logs/player/n56-smoke-steal-n72-freeze.log).
Gates (Logs/n72/gates-flow-close.txt, HEAD 9350c8631): flow --off67 EQUAL to the re-made base, config copies identical, golden 0 mismatches, fingerprints EQUAL plain and --off57..--off67: "gates: all GREEN 16:08:22".
Play build: Builds/n72-play (a hard-link copy of n72-freeze), its smoke DONE: all steps passed (Logs/player/n72-play-smoke.log); preview reels tf / bp / bk (Logs/feel/n72-play/).
ROW: n65.foundry65 enabled=false; SLOTS: a2=old a3=old a4=old a5=old b1=old a8=old b2=old
ROW: n67.iceline67 enabled=false; SLOTS: g1=old g2=old g3=old g4=old g5=old g6=old g7=old g8=old
ROW: n67.kiln67 enabled=false; SLOTS: kb2=old kb2c=old kb3=old kb3c=old kb3d=old
ROW: n67.spire67 enabled=false; SLOTS: v2=old v5=old v6=old
E72's numbers: rows ON in n72-play's default boot: 8 rows (colours63, goal64, palette72, decor72, lit72, bossfx72, body72, btext72), each with its SPOT above; boss dark share on the kiln 57.3 % -> 5.1 %; flicker share not measured.

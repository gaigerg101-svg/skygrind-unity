# PLAY71: GG's play card (NIGHT71; the build is Builds/n71-play, made from the freeze n71-freeze)

Written 16:35 on 2026-10-26 (5:49 after t0). Read time: 10-15 minutes. Judge by playing; the numbers below are information, not a verdict.
The earlier card for the first build (PB0) and the second (PB1, Builds/n71-play-pb1) are superseded by this one.

## What is in this build
- **Code:** HEAD of the night (the freeze n71-freeze). It includes the cleanup (the old OFF rows flow46, drone49, teach42 and about 360 dead lines), and the new code of the boss, colour and feel tracks. **Every new row is OFF** (voice71, words71, wyrm71, cyan71, kicks71, wash71, glow71, room71). They are not ON in this build.
- **Config:** the repo's config with four level rows ON: `n65.foundry65`, `n67.kiln67`, `n67.spire67`, `n67.iceline67`. Those rows swap in the rebuilt levels below.
- **Eight rebuilt rooms** (NIGHT71's aim was ten). Two on level 1 (foundry), four on level 3 (kiln), one on level 5 (spire), one on level 2 (iceline).

## Where each new room is
Start: TITLE > PLAY > the level select. Inside a level, the rooms run in order. DOORFREE (hold RB for 3 s, or F8) opens every door (unranked).

| Level | Room | Verb it asks for | What to try |
|---|---|---|---|
| 1 foundry | **1.6 THE KNOCKOUT YARD** | the launch: dash or ride into a pack at a gate and punch the front husk back into it | Launch the front husk into the pack at the gate. Is the first hit obvious? |
| 1 foundry | **1.10 THE SPILLWAY** | pierce; the ride beside the lane | Pierce the line of husks, then ride the lane. Does the lane read as a route? |
| 2 iceline | **2.4 THE ORB YARD** | parry | Parry in the yard. Is the pond the one ice floor? |
| 3 kiln | **3.2 THE CHARGING FLOOR** | parry | Parry the charge. |
| 3 kiln | **3.3 THE PRESS HALL** | launch | Launch into the press from the hall's door. |
| 3 kiln | **3.3c THE TAP RUN** | the ride | Ride the run from the entry to the tap hood. Does the ride pay? |
| 3 kiln | **3.3d THE CRACKED FLOOR** | see its card | Cross the cracked floor. |
| 5 spire | **5.6 THE CHOIR** | launch | Launch into the choir from the entry. |

## The numbers (the lead's measurements; same-day reds beside them; information, not a verdict)
- 1.6 YARD: walk deaths 0 (shipped 1); stall 31.8 % (shipped 33.8 %); pads 2 (shipped 2); air hand-offs 59 (shipped 9).
- 1.10 SPILLWAY: walk deaths 0; stall 28.1 % (shipped 42.3 %); pads 1 (shipped 0); air hand-offs 4 (shipped 2); air dead ends 2 of 4 (a label).
- 2.4 ORB YARD: walk deaths 0; stall 20.6 % (shipped 25.5 %); pads 3 (shipped 3); air hand-offs 68 (shipped 56). Its room check FAILs R5 foothold and I2 as the shipped room does.
- 3.2 CHARGING FLOOR: walk deaths 0; stall 18.9 % (shipped 18.6 %); pads 2 (2); air hand-offs 9 (9).
- 3.3 PRESS HALL: walk deaths 0; stall 24.5 % (shipped 23.1 %); pads 2 (2); air hand-offs 6 (6), dead ends 0 of 3 (shipped 1 of 4).
- 3.3c TAP RUN: walk deaths 0; stall 33.1 % (shipped 33.0 %); falls 8 in the walk bot (shipped 0; a label); pads 2 (2); air hand-offs 16 (shipped 14).
- 3.3d CRACKED FLOOR: walk deaths 0; stall 35.7 % (shipped 39.7 %); falls 35 in the walk bot (shipped 0; a label); pads 1 (1); air hand-offs 5 (5).
- 5.6 CHOIR: walk deaths 0; stall 19.3 % (shipped 18.9 %); pads 2 (2); air hand-offs 23 (shipped 18). Its room check FAILs R5 foothold as the shipped room does.

## Checks run on the build (Player checks; not GG's judgement)
- verbs37 in a Player on the same rebuilt levels (the build before this one, n71-play-pb1, same levels except the iceline row's label): n65-foundry 27/27, n67-kiln 31/31, n67-spire 31/31, n67-iceline 29/29; no red rows. The shipped-map label n52-iceline reads 25/29 RED when that row is ON; the lead traced it (R's grapple trace) to the reel's map label and an unowned-grapple gate that the row's level swap bypasses, not to a lost grapple node (the shipped and rebuilt iceline both have zero grapple nodes). Checked in a Player on the final build Builds/n71-play (made from n71-freeze, 16:31): verbs37 foundry 27/27, iceline 29/29, kiln 31/31, spire 31/31, no red rows (Logs/feel/n71-play/summary47.txt); smoke "DONE: all steps passed" (Logs/player/n71-play-smoke.log).
- The freeze n71-freeze: its shipped-level verbs37 and smoke are in Logs/feel/n71-freeze and Logs/player/n56-smoke-steal-n71-freeze.log (the close's record).
- Smoke in a Player on the earlier build n71-play-pb1: "DONE: all steps passed" (Logs/player/n71-play-pb1-smoke.log).

## Keep / drop (GG)
- 1.6 THE KNOCKOUT YARD: [ ] keep  [ ] drop  [ ] change: ____________
- 1.10 THE SPILLWAY: [ ] keep  [ ] drop  [ ] change: ____________
- 2.4 THE ORB YARD: [ ] keep  [ ] drop  [ ] change: ____________
- 3.2 THE CHARGING FLOOR: [ ] keep  [ ] drop  [ ] change: ____________
- 3.3 THE PRESS HALL: [ ] keep  [ ] drop  [ ] change: ____________
- 3.3c THE TAP RUN: [ ] keep  [ ] drop  [ ] change: ____________
- 3.3d THE CRACKED FLOOR: [ ] keep  [ ] drop  [ ] change: ____________
- 5.6 THE CHOIR: [ ] keep  [ ] drop  [ ] change: ____________

## Side by side
- `-skygrindOff65` boots every shipped level for a side-by-side (it turns the night's rows off too).
- The level rows' own switches are not checked in a Player for the final build; use `-skygrindOff65` for the comparison.

## Not in this build (each one in its own place)
- Rooms left out of this build because a Player check failed: **3.2c THE ASH FLOOR** (kb2c: with it in the kiln, verbs37 read 30/30 against the shipped 31); **5.2 THE NAVE** (v2) and **5.5 THE BELL WALLS** (v5): with both new on the spire, verbs37 reads RED on the level's first wall (rows 9 and 11, no ride at all). Their guard numbers are good; the Player checks are not. R has the numbers; NIGHT72 picks them up.
- Rooms of level 2 that failed: g3 (not in the play build: its IN VIEW check was green only after the last fix, after the level's verbs37 run), g8 (I2), g7 (stall A, I2, air hand-offs 4 against 95), g2 (the verb-aware walk's stall), g5 (the walk after its IN VIEW fix), g6 and g1 (landed in R's tree, not guarded in the main tree).
- Rows: none of the night's rows is ON. The B and F rows are on the main branch as code, OFF: voice71 (its counter is GREEN, its SPOT names the change, and its critic FAILed for want of the iceline and span verbs37 with the row alone), words71 (counter GREEN, SPOT NOTHING OBVIOUS), wyrm71 (stagger 18,991 against the 41,472 bar: SHORT), cyan71 (its role check fails), kicks71 (its probe is GREEN, its SPOT did not name the kick), wash71 (SHORT by 0.005), glow71 (RED on the PULSE check), room71 (its start works, GREEN; no SPOT frame).
- The eye63 A / B kit still waits for GG's X / Y.

## Your calls (the lead does not decide these)
- The keep / drop boxes above.
- Whether the cyan71 A / B and the voice71 pair (both OFF) are worth a play once their proofs are finished.
- The iceline row's label RED (the reel's map name under the row ON): whether to fix the reel or accept the trace.

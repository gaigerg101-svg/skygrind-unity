B2: PASS
(1) MAIN: MARKS AT THE FLASH END 15.29 % of the body ON vs 0.42 % OFF (-skygrindNoKill55, the row-off side at HEAD) and 0.44 % old draw (want >= 5 %; the noise is 0.42-0.46 % across OFF, old and NIGHT59's 0.46 %). The probe at the mark's centre reads V 0.76 -> 0.03 ON (old 0.52). The input line reads "mark queue 3006 vs the enemy x-ray's 3005 (x-ray setting on" ON, 2000 old and -1 OFF (Logs/player/n47-reel-n60-b2-g2on.log / -g2off.log / -g2old.log, line 107). The a355 line reads "ladder ok, cap ok, 3 in 1 s ok, debris ok, spaced ok" ON vs "ladder NO, cap ok, 3 in 1 s NO, debris NO" OFF (Logs/player/n47-reel-n60-b2-k2on.log / -k2off.log, line 106-107). The measure was named before any number: NIGHT58.md lines 78-79 moved it to the flash end because the next frame is 97 % white, and NIGHT60-BACKLOG.md fixed B2's MAIN at 4f52367a (23:02), before the reels (00:18). PASS.
(2) GUARDS: I re-ran n56-colour on all three sides and got the lead's TOTAL numbers exactly. ON vs OFF / old: world sat 8 vs 6 / 9 of 15; V3 yellow 13 vs 14 / 13; bodies over env p90 12 vs 12 / 11; tells out-saturated 7 vs 9 / 6; key|accent sat 15 vs 15 / 14. Cover 15/15, sky 0/15 and tint 15/15 are the same on all three. ON is inside the OFF / old spread on every count (Logs/feel/n60-b2/<side>/a258). PASS.
(3) Nothing lost: verbs37 vf / vi / vk / vs / vp read 27/27, 29/29, 31/31, 31/31, 31/31 with RED rows [] on each (Logs/feel/n60-b2/summary47.txt lines 66 / 108 / 150 / 192 / 234). That equals NIGHT59's freeze (Logs/n59/summary-freeze.txt lines 40 / 82 / 124 / 166 / 208). All five ran on Builds/n60-b2 with no off flag, so kill55 was ON. "NRE count: 0, exceptions: 0" appears in 10 of 10 reels (summary47.txt). The gates: NIGHT60-LOG.md line 36 says "Gates GREEN 00:15:00", after 284a4783 (23:56). There is no output file for that run, so I did not check it. The three B2 commits touch no Skygrind.Core file (git show --stat), so they cannot move golden / replays / fingerprints. PASS.
(4) The evidence is real and the comparison fair. Right build: all ten logs load Builds/n60-b2 (the Mono path, line 1); SKYGRIND.exe is from 00:17:00, after 284a4783; the queue-3006 line exists only with the fix. The row is really on: the build's StreamingAssets/Design/unity-config.json differs from Assets/StreamingAssets/Design/unity-config.json in exactly one key, design.n55.kill55.enabled (true vs false; my JSON diff), and the logs read "kill55 on True" ON / old and False OFF. The frames show what the numbers say (images below). N60MarkTests asserts queue > 3005 and _BaseColor kept with alpha 1 (read). I did not see it run RED / GREEN; the lead's log records 00:07:55 RED and 00:10:55 GREEN. CONFIRMED.

Meters re-run (python -I from the repo root, 2026-10-22):
- n60-v-mark.py Logs/feel/n60-b2 g2on g2off 958 551:
  - The model: under the x-ray, the hole #070303 becomes #A02B0F (V 0.63).
  - The frame's mark pixels: at the flash end 13.99 % of the rect, median #0F0804 (V 0.06), 66.4 % of them under V 0.12. At hit-after (+0.5 s): 19.64 %, V 0.04, 86.9 % (NIGHT59 read 12.9 %).
  - The planted cases print as designed: (a) the x-ray composite 0.21 % RED; (b) the own colours 16.37 % GREEN; (c) unplanted 0.21 % RED. The meter prints "meter OK".
- n56-colour.py Logs/feel/n60-b2/g2on/a258 --all-jpg --quiet: TOTAL frames 15 | V1 cover 15/15, sky 0/15, sat (whole world) 8/15, key|accent sat 15/15, tint 15/15 | V3 yellow 13/15 | body over env p90 12 (14%) | sat FAIL 7 / 15
- n56-colour.py .../g2off/a258: sat 6/15, key|accent 15/15, V3 yellow 14/15, body p90 12 (14%), sat FAIL 9 / 15; cover / sky / tint 15 / 0 / 15
- n56-colour.py .../g2old/a258: sat 9/15, key|accent 14/15, V3 yellow 13/15, body p90 11 (13%), sat FAIL 6 / 15; cover / sky / tint 15 / 0 / 15
- My own frame difference check (a scratch script): pixels changed by more than 30/255, outside a 180 x 260 px box on the mark.
  - hit-before: ON vs old 0.10 % of the whole frame; the runs match before the hit.
  - hit-next: ON vs old 0.80 %.
  - flash end: ON vs OFF 1.80 %, ON vs old 4.67 %, old vs OFF 5.55 %. The flash end lands on frame 6 for old and frame 7 for ON / OFF.
  - +0.5 s: ON vs old 0.32 %.
  - Inside the box, the 7x7 V at the centre: flash end ON 0.04, old 0.53, OFF 0.52; +0.5 s ON 0.20, old 0.63, OFF 0.64.

Images (5 reads, of at most 6):
1. g2on shot-N55-K-flash-end.jpg: a near-black, ragged hole with dark cracks on the victim's pale chest, under the crosshair. Yes, a dark wound on the body. The HUD, the three other bodies' red-orange x-ray and the debris look as in the old frame.
2. g2old shot-N55-K-flash-end.jpg: the same spot is a red-orange / dark-red smudge (a tinted square and a red crack line), with no black. The lead's read holds.
3. g2on shot-N55-K-hit-next.jpg: the victim is white under the flash, with faint grey crack lines and no dark mark (the reel reads 0.00 % next frame on every side). The cyan muzzle star is to the right of the body, not over the mark.
4. g2off shot-N55-K-hit-next.jpg: the same white flash, with a pale square at the hit point. Two things differ from ON: kill55's floor debris (left by a355's chain; present ON and old) and the room-clear time, 10.3 s vs 10.2 s ON (the same in old). Neither comes from tonight's queue change.
5. A composite of 2x crops around the mark (scratchpad crops-flashend.png): g2on flash end / g2old flash end / g2off flash end / g2on +0.5 s.
   - ON: a black hole with cracks.
   - old: a dark-red square with a red crack.
   - OFF: the hit's impact cube, a dark-red, x-ray-tinted square.
   - ON at +0.5 s: a large black wound with cracks on the red-orange body.

The known risk (sparks drawn under the mark) does not block. In these frames no see-through spark stands in front of the mark. The one thing the mark now covers is the hit's impact cube at the flash end: in old / OFF it shows as an x-ray-tinted dark-red square at the crosshair; ON it is under the hole. That lasts a few frames, at the hit point, where the hole is the intended read. The white flash, the hit-marker ticks, the muzzle star and the other bodies' x-ray look unchanged. A PLAY note for GG.

Notes (not blocking):
- The brief calls -skygrindOldMarks59 "NIGHT59's draw". The code (Kill55Damage.cs:104) and the log ("by the body copies (NIGHT58), mark queue 2000") say it is NIGHT58's body copies. NIGHT59's own Kill59Mark draw at queue 2010 is not in tonight's reels; NIGHT59-LOG.md line 34 has it at 0.46 %. Rule (1)'s baseline is the OFF side at HEAD, so the verdict stands.
- a355's overall line is RED on both sides ("damage NO": MARKS NEXT FRAME 0.00 % ON and OFF, because the next frame is 52-58 % white). The baseline fails it too (L-100), and since NIGHT58 the MAIN is measured at the flash end.
- The wound is large at +0.5 s: 21.86 % of the body (the a258 line). The geometry is unchanged; only its colour shows now. A PLAY note.
- Not checked: whether the move to queue 3006 drops the marks from the copied depth texture. Nothing depth-based shows wrong around the hole in these frames.

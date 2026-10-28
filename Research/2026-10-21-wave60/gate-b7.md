B7: PASS
(1) MAIN: bodies over the world's p90 V and S on a357 read OFF 106 / 140 (75.7 %) and ON 127 / 141 (90.1 %). That covers 5 levels and 40 shot pairs on Builds/n60-b7, with the OFF side launched with -skygrindNoRim56. My re-run of `python -I Tools/night/n60-v-rimpath.py Logs/feel/n60-b7` is byte-identical to Logs/n60/b7a3-rimpath.txt. The OFF side reads 108 / 140 (77.1 %) on n60-b4 and n60-b5 (Logs/n60/b7a1-rimpath.txt, b7a2-rimpath.txt), so the OFF noise is 2 bodies, or 1.4 points. ON beats the best OFF reading by 13.0 points (about 9x the noise) and the same-build OFF by 14.4 points. **90.1 % beats 75.7-77.1 % well beyond noise.** The slate fixed the measure before any number: Logs/n58/slate.json entry V1 was added in 7f88487c, NIGHT58's shared commit (2026-10-21 14:19).
   The margin on the ">= 90 %" target worries me as a claim, not as a verdict. 127 / 141 is 90.07 %, so 126 / 141 would read 89.4 %. One pair is not a same-pose pair: foundry b1-mid differs on 91 % of its world pixels even OFF vs OFF (n60-b5 vs n60-b7), and it alone holds the 141st body. Without that pair the totals read 106 / 139 (76.3 %) vs 125 / 139 (89.9 %). Attempt 2 (gain 1.3) read 128 / 141 (90.8 %). A re-run lands near 90 %, on either side of it. The report should say "about 90 %", not ">= 90 % met". Rule 1 asks for a win over the baseline beyond noise. Rule 4 makes a near miss on a target that is not a guard a note, not a block. PASS.
(2) GUARDS: I re-ran kiln and iceline on and off (n56-colour.py --kinds entry,mid; n57-v-cvd.py grey). Both match Logs/n60/b7a3-guards.txt exactly:
   - kiln: world over the tells 3 / 10 on both sides, on the same three frames (kb2-mid, kb2c-entry, kb2c-mid). ON's share is smaller on each: 0.07 / 2.00 / 2.79 % vs 0.14 / 2.21 / 3.74 % of the frame. Grey 7 -> 8 / 10; tint 10 / 10 on both sides.
   - iceline: world over the tells 0 / 12 on both sides. g2-entry, the frame that failed attempt 2, reads 0.00 % ON. Grey 9 -> 12 / 12. Tint 11 / 12 on both sides: g8-mid fails on the luma-IQR clause on both sides (0.055 OFF, 0.050 ON).
   - Totals from the file: world over the tells 3 / 40 on both sides (the slate's <= 3 / 36 holds). Greyscale 25 -> 36 / 40 (want >= 25). Tint 39 / 40 on both sides; the one failing frame fails on the baseline too (L-100).
   PASS.
(3) Nothing lost: verbs37 ran with rim56 ON on Builds/n60-b7. None of the five logs has -skygrindNoRim56, and each holds "rim56's accent dim on the band x0.45 (NIGHT60 B7)".
   - Results: vf7 27/27, vi7 29/29, vk7 31/31, vs7 31/31, vp7 31/31. RED rows [] and "NRE count: 0, exceptions: 0" on all five (Logs/feel/n60-b7/summary47.txt lines 227-230, 269-272, 311-314, 353-356, 395-398).
   - These equal NIGHT59's freeze (Logs/n59/summary-freeze.txt lines 39 / 81 / 123 / 165 / 207). I did not count block vk15, which ran with -skygrindNoRim56.
   - Golden and replays cannot move: the three B7 commits (2771de00, 5cddef27, 4669d2be) touch only Decor55.cs, Rim56.cs, Rim60.N60V.cs and UnityTuning.N60V.cs (all Skygrind.Game) plus V.md.
   - **The flow gate is not done.** NIGHT60-LOG.md line 90 lists it as still needed, and I found no result. The row must not flip ON until the lead's `n60-gates.sh --flow` reads EQUAL.
   PASS on the evidence that exists, with that gate as a condition.
(4) The evidence is real and the comparison fair.
   - Right build: all 15 Player logs (10 a357, 5 verbs37) load Builds/n60-b7 (the Mono path, line 1). SKYGRIND.exe is from 04:31:13, after 4669d2be (04:25:52) and before the next commit (1e45f55e, 04:32:56).
   - The row is really on: the build's StreamingAssets/Design/unity-config.json differs from the repo's Assets/StreamingAssets/Design/unity-config.json in exactly one key, design.n56.rim56.enabled (true vs false; my JSON walk).
   - The inputs fired. The five ON logs read "rim56 ON, the lit side's gain written 1.10" and "rim56's accent dim on the band x0.45 (NIGHT60 B7)". The five OFF logs read "rim56 OFF, the lit side's gain written 0.00" and "rim56's accent dim on the band no (NIGHT60 B7)".
   - Same poses: the enemy rects match within 3 px on 30 of the 39 pairs that have rects. The other 9 differ by 6 px or more, up to rects far off-screen (moving or off-screen enemies). The per-frame body counts are equal on 39 of 40 pairs; foundry b1-mid is the exception.
   - The frames show what the numbers say (images below).
   - N60RimTests RED 04:24 -> GREEN 4/4 04:27:14 is the lead's record (NIGHT60-LOG.md lines 108-109). I did not see it run.
   CONFIRMED.

What moves the MAIN (my scratch split, the 139 bodies in the 39 paired frames; scratchpad b7check.py):
- Kept 104, gained 21, lost 2, never over the line 12.
- Of the 21 gained, 16 cross only because the world's p90 fell (the accent dim). 3 cross only because the body got brighter. 1 crosses by either change alone, and 1 only by both together.
- The 2 lost bodies are on kiln (kb3-mid, kb3c-entry). Their body S fell 0.66 -> 0.64 and 0.71 -> 0.68, under the world's S p90: gain 1.1 still whitens a little.
- So at gain 1.1, rim56 works mostly by darkening the level's accent, not by brightening the enemies. The row is designed to do both, and the slate's measure counts both. But the darker accent is the change GG will see.

Meters re-run (python -I from the repo root, 2026-10-22):
- n60-v-rimpath.py Logs/feel/n60-b7: TOTAL off 106 / 140 (75.7 %), on 127 / 141 (90.1 %); per level foundry 3 / 18 -> 16 / 19, iceline 26 / 26 = 26 / 26, kiln 35 / 52 -> 43 / 52, span 21 / 22 =, spire 21 / 22 =. Identical to the lead's file.
- n56-colour.py, kiln on / off: V1 GUARD 3 / 10 = 3 / 10, tint 10 / 10 =, body over env p90 43 vs 35 of 52. n57-v-cvd grey: 8 / 10 vs 7 / 10.
- n56-colour.py, iceline on / off: V1 GUARD 0 / 12 = 0 / 12, tint 11 / 12 =, body over env p90 26 = 26 of 26. Grey: 12 / 12 vs 9 / 12.
- My world-pixel floor check (scratchpad b7noise.py). It counts pixels changed by more than 0.03, outside the enemy rects:
  - Two rim56-off runs (n60-b5 vs n60-b7) differ on 17.0 / 13.5 / 7.5 / 14.8 % of the world on foundry / iceline / span / spire (medians).
  - ON vs OFF differs on 20.6 / 17.2 / 11.2 / 17.7 %.
  - So rim56's own change to the world is a few points over the run noise, which fits the accent dim alone. Kiln was not compared: its level data changed between b5 and b7 (B15).

Images (6 reads, of at most 6; side-by-side ON | OFF composites and 2x crops made in the scratchpad):
1. foundry a5-mid ON | OFF: ON draws the violet accent strips at about half brightness, as a dark purple band on the left wall and the right diagonal. In the OFF frame a bright lilac strip runs down to the gun. The five red-orange bodies read the same or slightly warmer. Cyan strips, HUD and gun are unchanged.
2. foundry a5-mid crop, 2x: ON bodies have a brighter yellow-orange lit side and a thin near-rim edge, and their shadow side stays dark red-brown. OFF bodies are flatter and pick up a violet cast near the bright strip. Still red-orange; nothing off-palette.
3. iceline g2-entry ON | OFF (the guard's frame): ON draws the mint-green bands on the pillars and ramps as dark green. The yellow shields, the red-orange bodies and the tutorial line look the same.
4. iceline g2-entry crop, 1.53x: the yellow shields stay saturated yellow ON, if anything a little warmer than OFF. **Nothing whitens**, so attempt 3's fix holds on this frame.
5. kiln kb3d-mid ON | OFF: ON draws the blue accent bands on the right pillars and the top-left band as navy. The warm floor, the pale vertical light strips and the cyan trims are unchanged. The bodies are slightly brighter.
6. kiln kb3-mid ON | OFF: the yellow orb cluster covers the bodies on both sides and looks the same. Only the blue bands dim.
No frame looks washed or tinted, and none adds a hue (rule 8): the change is a darker accent plus a warm edge on the bodies.

Notes (not blocking):
- "key|accent sat" is not a slate guard, but it moved. Iceline goes 7 / 12 -> 6 / 12: on g3-mid the accent's S p99.5 rises 0.93 -> 0.98 against the tells' 0.95. Kiln stays at 7 / 10 on both sides. Not checked on foundry, span or spire. Darkening the accent deepens its saturation: the ON strips are deep green or navy rather than glowing.
- The world is flatter in value with rim56 on. Luma IQR falls on every frame I re-ran (iceline g2-entry 0.172 -> 0.106, kiln kb3d-mid 0.200 -> 0.147). The tint check fails a frame when IQR drops under 0.06, and iceline g8-mid already fails on both sides.
- Not checked: the flow gate (pending; the lead's step); foundry, span and spire's guard lines (taken from the lead's file); span and spire frames; whether the later commits (B8, B9, 05:20-05:33) touch rim56's files.

For GG (play notes; rim56 is a view row, no sim change):
- Each level's accent strips glow at about half their old brightness and look deeper and darker. These are the wall bands and trims in the level's own colour: foundry violet, iceline mint and kiln blue in the frames I opened. Rooms read a little darker and flatter.
- Enemies keep their red-orange. Their lit side is a touch brighter and they get a thin yellow-orange edge. Iceline's yellow shields do not wash out.
- Enemies stand out mostly because the strips stop competing with them, not because the enemies glow more. Judge whether the dimmer strips make a level feel duller: it is colour56's level identity, which you have not played yet either.
- Kiln's warm floor is unchanged and still close to the enemies' colour. 9 of 52 kiln bodies still do not clear the world's brightest tenth.
- Off switch: design.n56.rim56.enabled. It is false in the repo's Assets/StreamingAssets/Design/unity-config.json and true only in Builds/n60-b7's copy. Or launch with -skygrindNoRim56.

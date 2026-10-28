# NIGHT57 gate r3 brief (the lead; build Builds/n57-r3 from port/stage3 at 12:2x, rows ON in its copy: n55.wyrm55, n56.tokens56,
# n56.colour56, n55.brief55, n55.kill55, n55.wick55, n55.grade55, n56.rim56, n56.yellow56, n57.kiln57; template:
# Research/2026-10-21-wave57/critic-gate.md: read it first. Your taste decides nothing.)

All paths under C:\Users\gaige\dev\SKYGRIND-unity. Write Research/2026-10-21-wave57/gate-r3.md. MAIN / GUARDS: Logs/n57/slate.json.
Reel results: Logs/feel/n57-r3/summary47.txt (blocks start `== <side> <time> (map ..., scenario ...)`). Raw frames are pruned to every
50th (n57-prune.sh); named shots (shot-*) are kept. Baselines: Logs/n57/flow-base.txt, wyrm-base.txt.

## E1 L3-rooms / kiln57 (level row: level 3 as n57-kiln)
- Bot logs (builder L's, copied): Logs/n57/L/flow-n57-kiln-r3d.txt and flow-n57-kiln-r3d-B.txt (seeds 1-10 / 11-20) vs flow-base.txt's
  n52-kiln line (and L's n52 seeds 11-20 file if present in Logs/n57/L). MAIN: >= 5 combat rooms, >= 3 new passing; clear 150-240 s.
  GUARDS: stall, deaths. NOTE: r3's PLAYER build has slot C's kb3 as n52's (slot C was picked after r3 was built); the bot logs r3d are
  the final level (5 new rooms).
- Nothing lost: the `== vk57` block (verbs37 on n57-kiln) vs the `== vk` block of Logs/feel/n57-r1/summary47.txt (verbs37 on n52-kiln,
  kiln57 off). The lead reads RED rows 40 (parry) and 41 (launch) and rows 25-27 PASS -> SKIP. Confirm or refute from the lines; say
  whether the evidence shows a verb lost on n57-kiln or a reel staging difference ("open floor kb2" vs "kb2b").
- Also r3's `== vk` block (verbs37 on n52-kiln WITH kiln57 on): RED rows 17 / 19 / 22 / 23, "grapple owned True". Is that the reel loading
  a level that kiln57 swapped out of the campaign (its campaign place changes), or a real loss? r1's n52-kiln run (kiln57 off) is 31/31.
- Frames (<= 8): Logs/feel/n57-r3/l2on/a452/ (find shot-*-n57-kiln-kb2-entry/mid/high, kb2c-entry, kb3c-mid, kb3d-mid). Eye lines:
  every piece looks like it has a job (GG: "random lava pits random pillars that serve no purpose"); the first wave visible at entry.

## E2 G3-kill55
- MAIN (measured fallback): hit marks >= 5 % of the body on the next frame AND the chain hold ladder. GUARD a354 jitter <= 0.191.
- Lines: the `== k3on` / `== k3off` blocks (a355) and `== m3on` / `== m3off` (a354). The lead reads: ladder ok; MARKS NEXT FRAME 0.00 %
  (want >= 5 %): MAIN FAILS; jitter 0.191 on and off.
- Frames (2): Logs/feel/n57-r3/k3on/a355/ shot-N55-K-hit-next.* and shot-N55-K-hit-after.* (find them).

## E3 V3-grade55 (THE CROWN's tells; THE WICK's ride cue)
- MAIN THE CROWN's tells kept vs raw (NIGHT56: 0.68 kept). GUARDS THE WICK's pass kept; its ride panels carry cyan.
- Lines: `== b2on` / `== b2off` (spire, a155: TELLS, WALLS, the RED reason) and `== b1on` (foundry: RIDE CUE). Say what makes b2on RED.
- Frames (4): Logs/feel/n57-r3/b2on/a155/ crown37 p1 and p1-raw shots; Logs/feel/n57-r3/b1on/a155/ wick p2 (find them).

## E4 V1-colour56 (r2's fix: the frieze, accents and walls held under the world's luminance) and V4 rim56 / yellow56
- MAIN (measured fallback): key|accent at S >= 0.35 on >= 3 % per frame; GUARDS: world over tells, greyscale, deuteranopia.
- Run `python -I Tools/night/n56-colour.py <dir> --kinds entry,mid` on Logs/feel/n57-r3/v1on-<level>/**/a357 and v1none-<level> for the
  five levels, and `python -I Tools/night/n57-v-cvd.py grey <the five a357 dirs of a side> --kinds entry,mid` per side (r1: v1none 25/35,
  v1on 18/36). rim56: n56-colour.py's rim / body lines (want enemies above the world's p90 on >= 90 %); yellow56: its yellow per frame
  line (want <= 0.5 %). If a colour reel is missing, say "not evidenced".
- Frames (2): v1on-spire and v1none-spire shot-N56-n52-spire-v6-entry (find them). Eye line: a whole-frame tint?

## E5 D-11 wick55 (sim row; bot)
- Logs/n57/g4-wick-*.txt (MAIN damage in windows; GUARD deaths pooled over seeds 1-80; median in the bot band 30-60 s). No frames.

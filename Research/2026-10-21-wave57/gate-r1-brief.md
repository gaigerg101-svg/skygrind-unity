# NIGHT57 gate r1 brief (the lead; build Builds/n57-r1 from port/stage3 b34438ce, rows ON in its copy: n55.wyrm55, n56.tokens56,
# n56.colour56, n55.brief55; the template is Research/2026-10-21-wave57/critic-gate.md: read it first)

All paths are under C:\Users\gaige\dev\SKYGRIND-unity. Write Research/2026-10-21-wave57/gate-r1.md. The slate entries' MAIN / GUARDS:
Logs/n57/slate.json. The baselines: Logs/n57/flow-base.txt, Logs/n57/wyrm-base.txt (every n56 / n57 row off, HEAD 2371852a).

## E1 P0-shelf: wyrm55 (sim row; THE KILN WYRM's encounter numbers; bot)
- MAIN deaths per fight; GUARDS killed, median, p90 (named in NIGHT57.md section 1 before the runs).
- Logs: Logs/n57/wyrm-base.txt and Logs/n57/wyrm55-on.txt (seeds 1-40; the `| wyrm |` table line; the header names the rows forced on),
  Logs/n57/wyrm-base-41.txt and Logs/n57/wyrm55-on-41.txt (seeds 41-60, the noise band).
- No frames (a harness row). Nothing lost: verbs37 on n52-kiln with wyrm55 ON in the copy, Logs/feel/n57-r1/summary47.txt (`total 31/31`
  on n52-kiln, RED rows []).

## E2 G1-tokens56 (sim row; bot)
- MAIN attack starts inside the sim cone; GUARDS deaths per level in band, clear time <= +10 % (20 seeds, D-10).
- Logs (builder G's, committed): Logs/n57/g1-tokens-on.txt (its header: rows forced on), Logs/n57/flow-base.txt, Logs/n57/g-base20.txt,
  and the 20-seed on file named in Research/2026-10-21-wave57/G.md (read G.md only to find file names; its claims are not evidence).
- Nothing lost: verbs37 x5 with tokens56 ON: Logs/feel/n57-r1/summary47.txt, the five `verbs37: PASS: total` lines (27, 21, 31, 31, 31).

## E3 V2-brief55 (view row; the teach card clears the room counter)
- MAIN every teach line passes; GUARD 0 frames of overlap with the room counter. The checker: `node Tools/night/n55k-teach.cjs`.
- Tonight's reels ran on KEYBOARD (no pad connected; the telemetry says "device":"kbm"); NIGHT56's ran on a pad ("device":"pad").
  ON: run `node Tools/night/n55k-teach.cjs Logs/feel/n57-r1/vk/verbs37/telemetry.jsonl kiln Logs/feel/n57-r1/vp/verbs37/telemetry.jsonl spire`;
  OFF (brief55 and every n55+ row off, the same build): `... Logs/feel/n57-r1/vkoff55/verbs37/telemetry.jsonl kiln`; tonight's fix off
  (-skygrindOff57): `... vkoff/verbs37/telemetry.jsonl kiln vpoff/verbs37/telemetry.jsonl spire`. NIGHT56 (pad): Research/2026-10-20-wave56/gate-r5b.md B10.
  Judge whether the "bad" lines on keyboard are the checker (does it count "LEFT CLICK" as a glyph? as one word?) or the text.
- Frames (4): Logs/feel/n57-r1/vk/verbs37/f0100.jpg (ON) and Logs/feel/n57-r1/vkoff55/verbs37/f0100.jpg (OFF), f0300.jpg of each.

## E4 V1-colour56 (view row): the lead reads a GUARD FAIL; confirm or refute
- MAIN (measured fallback, calibration failed): key|accent at S >= 0.35 on >= 3 % per frame, accents >= 40 deg apart.
  GUARDS: world over the tells <= baseline; greyscale (enemies + hazards median luma >= world p75) not worse than baseline beyond band;
  deuteranopia keeps cyan vs red-orange apart.
- Counts: Logs/n57/r1-colour-counts.txt (from `python -I Tools/night/n56-colour.py <dir> --kinds entry,mid` per level and side:
  v1on = colour56 on, v1off = -skygrindOff57 (NIGHT56's colour56, no cap), v1none = -skygrindOff56 (no colour56)). Re-run one level yourself.
  Greyscale: `python -I Tools/night/n57-v-cvd.py grey <the five a357 dirs of a side> --kinds entry,mid` (the lead read v1on 18/36,
  v1off 19/36, v1none 25/35). CVD: `... cvd <4 entry frames>` (4/4).
- Frames (4): Logs/feel/n57-r1/v1on-spire/**/shot-N56-n52-spire-v6-entry.png and the same under v1none-spire; the same pair for
  n52-kiln kb2-entry (use `find`). Eye line: a whole-frame tint?

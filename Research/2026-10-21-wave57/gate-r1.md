# NIGHT57 gate r1 (fresh batched critic; K = 1; build Builds/n57-r1 = "SKYGRIND 0.1.0 (b34438ce-dirty)")

Rubric: Research/2026-10-21-wave57/critic-gate.md. Brief: Research/2026-10-21-wave57/gate-r1-brief.md. No builder note was used as evidence
(G.md was opened only for file names). Every number below is from a log line I read or a command I ran (python -I / node, read-only).
Bot numbers are labelled (bot). Images opened: 8 (E3 4, E4 4).

Checks common to the batch:
- The r1 build's copy `Builds/n57-r1/SKYGRIND_Data/StreamingAssets/Design/unity-config.json` (written 10:37:17) has `design.n55.wyrm55`,
  `n56.tokens56`, `n56.colour56`, `n55.brief55` (and `n55.clear55`) `enabled: true`; every other n55 / n56 / n57 row false. The repo
  config has all four false.
- Every r1 reel's `reel.json` reads `"build":"SKYGRIND 0.1.0 (b34438ce-dirty)"`, `"pad":false`; every telemetry frame reads
  `"device":"kbm"` (1800 / 1800 on vk, vp, vkoff, vpoff, vkoff55).
- `git diff --stat 2c8e3f6c b34438ce`: between the shared commit (where the harness runs were made) and r1's source no sim file changed
  (Assets/Scripts changes: Brief55, Colour56, FeelReel.N56/N57.Colour, Render/Cap57Pass, Render/TravelBlurFeature, UiBrief55,
  UnityTuning.N57V; Design/unity-config.json adds only `colour56.numbers.cap`).
- Two inputs changed under me during the review (cherry-picks at 11:32-11:43): `Tools/night/n55k-teach.cjs` (af6b2c2c) and
  `Design/unity-config.json` (17d21543). I ran the teach check with both the old and the new checker and with my own variant (same result),
  and the colour meter with `--config` set to the r1 build's copy (same kiln result as the HEAD config).

---

## E1 P0-shelf: wyrm55 (sim row; bot)

| line | verdict | evidence | reason |
|---|---|---|---|
| 1 Evidence real | PASS | Logs/n57/wyrm-base.txt:2 `rows []`; Logs/n57/wyrm55-on.txt:4 `rows [wyrm55]`; the same on wyrm-base-41.txt:2 / wyrm55-on-41.txt:2 (`seeds 41..60`); both committed in 5dfe5050 (09:45) from HEAD 2c8e3f6c; no sim change 2c8e3f6c -> b34438ce; r1 copy `wyrm55 enabled: true` | the row was forced on only in the ON runs; the code measured is r1's sim. Not checked: wyrm55 together with tokens56 (both on in r1; the harness ran wyrm55 alone) |
| 2 MAIN deaths per fight | PASS | wyrm-base.txt:45 `| wyrm | 38/40 | ... | 68 |` -> wyrm55-on.txt:47 `| wyrm | 40/40 | ... | 32 |`: 1.70 -> 0.80 per fight; seeds 41-60: wyrm-base-41.txt:25 `30` -> wyrm55-on-41.txt:25 `13`: 1.50 -> 0.65 | the baseline's own seed sets differ by 0.20 per fight; the drop is 0.90 / 0.85, over 4x that (bot). The per-seed deaths are not in the log, so the rule's 1-10 vs 11-20 split cannot be read; the 1-40 vs 41-60 split is the larger-sample substitute |
| 3 GUARDS killed, median, p90 | PASS | killed 38/40 -> 40/40 (41-60: 19/20 -> 20/20); median 122.6 -> 86.8 s (101.3 -> 66.6); p90 256.9 -> 178.3 s (228.0 -> 179.7) | each better or held on both seed sets (bot); the median now sits inside GG's "one to two minutes" (60-120 s, NIGHT56.md G4's band for this row and harness) on both sets. Near misses, not guards: the harness's own band prints `90-150 / p90 <= 180 / min >= 60: OUT` on both sides (ON median 86.8 and 66.6 are under its 90; the fastest kill 45.6 -> 27.0 s) |
| 4 Nothing lost | PASS | Logs/feel/n57-r1/summary47.txt:124 `verbs37: PASS: total 31/31 on n52-kiln; RED rows []` (r1, wyrm55 on in the copy); the off side = wyrm-base (`rows []`, NIGHT54's fight) | verbs37 does not fight the worm; that is the line the brief names |
| 5 Eye-only | PASS (no view change) | `git grep -l wyrm55 -- Assets/Scripts`: Core/Enemies/Wyrm55.cs, Game/SurvivalSim.N56.Wyrm.cs, SurvivalSim.N56.cs only; the row's numbers are poolScale, contactCooldown, phase3At, spitEvery2 / 3 (3.2 -> 6.4 s, 2.2 -> 4.4 s: fewer barrages, so fewer flashes), open | the row draws nothing new; every tell keeps its look |

**GATE P0-shelf (wyrm55): PASS**

## E2 G1-tokens56 (sim row; bot)

| line | verdict | evidence | reason |
|---|---|---|---|
| 1 Evidence real | PASS | Logs/n57/g1-tokens-on.txt:1 and g1-on20.txt:1 `rows forced on: tokens56`; flow-base.txt, g-base-s11.txt, g-base20.txt `rows forced on: none`; committed in 416c4a61 (10:19, measurement only); no sim change up to b34438ce; r1 copy `tokens56 enabled: true` | the off side prints `tokens56 thinks waited out of view 0, advances 0`; the on side 17407-37114 waits: the row really switched |
| 2 MAIN attack starts inside the sim cone | PASS | `n46flow <level> TOKENS:` lines, pooled over the five levels: seeds 1-10 1344/3005 = 44.7 % -> 2081/2449 = 85.0 %; seeds 1-20 2681/6032 = 44.4 % -> 4162/4896 = 85.0 %; baseline seeds 11-20 1337/3027 = 44.2 % | +40 points against a 0.5-point seed gap (bot); every level up (lowest: span 79.2 % at 20 seeds). Noted: attack starts fall 19 % (6032 -> 4896) |
| 3 GUARDS deaths in band, clear <= +10 % | PASS | `SUMMARY` lines. Deaths, 20 seeds: 32 / 13 / 19 / 1 / 0 -> 27 / 8 / 21 / 2 / 0 (10 seeds: 21 / 9 / 8 / 0 / 0 -> 13 / 7 / 8 / 0 / 0). Clear, 20 seeds: 116.5 / 233.3 / 91.7 / 89.0 / 140.2 -> 124.7 / 237.3 / 94.7 / 90.5 / 141.8 s = +7.0 / +1.7 / +3.3 / +1.7 / +1.1 % | kiln 19 -> 21 lies inside the baseline's own halves (8 and 11 per 10 seeds); span 1 -> 2 is at the 0-1 noise floor. The closest call: at 10 seeds iceline 220.3 -> 245.2 s (+11.3 %) and spire 135.1 -> 149.0 s (+10.3 %); the baseline's own 1-10 vs 11-20 clear differs 8.6 % (iceline) and 16.8 % (spire). I accept the 20-seed reading: it contains the 10 seeds, both sides were run at 20 with the same harness, and it shows iceline's seeds 11-20 ran faster with the row on |
| 4 Nothing lost | PASS | summary47.txt:40 / 82 / 124 / 166 / 208 `verbs37: PASS: total 27/27`, `21/21`, `31/31`, `31/31`, `31/31`, each `RED rows []` (r1, tokens56 on); my diff of flow-off57.txt vs flow-base.txt (data lines): equal | the off switch gives NIGHT56's campaign; golden / replays / fingerprints not checked by me |
| 5 Eye-only | PASS (no view change) | `git grep -l tokens56 b34438ce -- Assets/Scripts`: Core/HuskBrain(.N56).cs, Game/SurvivalSim.N56(.Tokens).cs, FeelReel.N57.cs (a reel) | no new drawing, tell or text |

Noted, not a guard: per-enemy idle over 20 % is 20.0 % on iceline at 20 seeds (`IDLE` line), at the target's edge.

**GATE G1-tokens56: PASS**

## E3 V2-brief55 (view row)

Frames, described before their numbers:
- Logs/feel/n57-r1/vk/verbs37/f0100.jpg (ON): a kiln corridor with rose bands and blue trim on the walls; the centre card "Jump SPACE, aim
  down, fire LEFT CLICK" with the key names in cyan, under the DOOR OPEN banner; nothing over the banner, the score or the health plate.
- Logs/feel/n57-r1/vkoff55/verbs37/f0100.jpg (OFF, every n55+ row off): the same spot with green trim and no rose; the card "SPACE to jump".
- Logs/feel/n57-r1/vk/verbs37/f0300.jpg (ON): the gun up; the card "Midair, aim down, then LEFT CLICK", LEFT CLICK in cyan.
- Logs/feel/n57-r1/vkoff55/verbs37/f0300.jpg (OFF): the card "In the air, look straight down" (no key named).

| line | verdict | evidence | reason |
|---|---|---|---|
| 1 Evidence real | PASS | reel.json `b34438ce-dirty`; summary47.txt:85 vk (no switch), :211 vkoff `-skygrindOff57`, :484 vkoff55 `-skygrindOff55`; r1 copy `brief55 enabled: true`; the four frames carry the lines the telemetry lists at f0 / f231 | the sides really differ: vkoff55 shows NIGHT54's text, vkoff the 1-frame overlap; the frames match the checker's text |
| 2 MAIN every teach line passes | PASS | the checker as the brief gave it (before af6b2c2c): vk kiln `lines 6, bad 5`, vp spire `lines 2, bad 1`, `RED teachText`; vkoff the same lines and counts. My variant (scratchpad teach-kbm.cjs: the original code, "LEFT CLICK" read as one key) and the updated checker (af6b2c2c) both print vk `lines 6, bad 0`, vp `lines 2, bad 0`, `GREEN teachText`; vkoff55 `lines 6, bad 3` | **the bad lines are the checker's, not the text.** Its BUTTONS list had no CLICK name, and the two-word key name counted as two words. The same authored lines on a pad (NIGHT56 gate-r5b.md B10: "Jump B, aim down, fire ZR", every line ok) are 6 words with a glyph. The ON lines equal the OFF57 lines word for word (the fix moved no line). NIGHT54's text (vkoff55) still fails 3 lines, so the checker still catches real misses. Against the row-off baseline: 3/6 ok -> 6/6 and 2/2 ok |
| 3 GUARD 0 frames over the room counter | PASS | node n55k-teach.cjs: vk kiln and vp spire `a card over the counter in 0 frames (worst 0.0 px)`; vkoff / vpoff `1 frames (worst 2.4 px) BAD`; vkoff55 `99 frames (worst 42.1 px)`. Telemetry f700-704: the counter opens at f702 on both sides; ON's Hint is at y 156 on f702, OFF57's at y 56 on f702 and 153.9 from f703 | the event happens on both sides and the ON side clears it on the frame it opens; this is measured, not "expected" |
| 4 Nothing lost | PASS | summary47.txt:124 `31/31 on n52-kiln; RED rows []`, :208 `31/31 on n52-spire`; the off sides show the older behaviour (vkoff55: NIGHT54's lines; vkoff / vpoff: the 1-frame overlap) | - |
| 5 Eye-only | PASS | the teach text rect is 37 px high (`Teach card 43/Verb/Text [707.9, 153.5, 504.2, 37]`), the hint 26.1 px: >= 16 px; key names in cyan (player-usable); my overlap scan (scratchpad hud_overlap.py, every teach / meta card against every HUD plate, all frames): ON vp none; ON vk only `Prompts over Weapon 52` (57 frames), the same 57 frames on vkoff and vkoff55 | the one remaining hit is the SLIDE prompt's 604 px-wide text rect (the word is ~100 px), and it is identical with the row off; no tint or flash belongs to this row |

**GATE V2-brief55: PASS**

## E4 V1-colour56 (view row): the lead's GUARD FAIL confirmed

Frames, described before their numbers:
- Logs/feel/n57-r1/v1on-spire/a357/v6/shot-N56-n52-spire-v6-entry.png (ON): the green-lit spire hall; three red-orange husks and a red
  shield sphere ahead; cyan edge lines; new against the OFF frame: lighter lime-olive bands and crimson-rose stripes on both walls, and a pink
  line low right where the OFF frame has a yellow one.
- Logs/feel/n57-r1/v1none-spire/a357/v6/shot-N56-n52-spire-v6-entry.png (colour56 off): the same view, darker olive walls, no stripes.
- Logs/feel/n57-r1/v1on-kiln/a357/kb2/shot-N56-n52-kiln-kb2-entry.png (ON): the tan kiln hall with dusty-rose bands and blue stripes on the
  walls; two orange husks left, an orange-red sphere and husks right; cyan ledges.
- Logs/feel/n57-r1/v1none-kiln/a357/kb2/shot-N56-n52-kiln-kb2-entry.png (colour56 off): the same view without the rose and blue bands.
In both pairs the walls get brighter with the row on; the enemies do not change.

| line | verdict | evidence | reason |
|---|---|---|---|
| 1 Evidence real | PASS | reel.json `b34438ce-dirty` (v1on / v1off / v1none spire and kiln); summary47.txt:304 `THE WORLD SATURATION CAP ON for n52-foundry (cap 0.70) ... colour56 on`, :315 v1off `-skygrindOff57 ... no cap (NIGHT56's colour56)`, the v1none run `colour56 off: no cap`; r1 copy `colour56 enabled: true`; my re-runs with `--config` = the r1 copy | the three sides are the three switches; the frames show the brighter walls the numbers say |
| 2 MAIN (measured fallback) | PASS | my run, n56-colour.py on the five v1on dirs: `V1 cover 36/36`, `palette accents pairwise: least gap 45 deg (iceline / span) -> PASS (>= 40)`; the five v1none dirs: `V1 cover 22/36` | +14 frames; the accents are apart |
| 3a GUARD world over the tells | PASS | the meter's own `V1 GUARD count` lines: v1on 0 / 0 / 1 / 0 / 0 = 1 / 36; v1off 5 / 7 / 6 / 3 / 6 = 27 / 36; v1none 6 / 8 / 6 / 5 / 6 = 31 / 36 | far under the row-off baseline. Logs/n57/r1-colour-counts.txt's "world-over-tells" column (7 / 32 / 36) is the meter's `key|accent sat` FAIL count, not its GUARD line: mislabelled, passes either way |
| 3b GUARD greyscale (E-3) | **FAIL** | `python -I Tools/night/n57-v-cvd.py grey <five dirs> --kinds entry,mid`: v1on `GREY TOTAL: 18 / 36`, v1off `19 / 36`, v1none `25 / 35 ... 1 n/a`. Paired by shot, v1none -> v1on: 8 PASS->FAIL, 0 FAIL->PASS (foundry b1-mid; iceline g3-mid, g8-entry; kiln kb2-entry; span p1-entry, p3-entry, p5-entry, p5-mid); v1none -> v1off: 7 PASS->FAIL, world p75 median +0.045, enemies+hazards median +0.009; v1off -> v1on (the cap alone): 1 flip, both medians +0.000 | the baseline is the row off (v1none), not NIGHT56's colour56 (v1off). The keys, accents and friezes lift the world's values above the enemies and hazards; the cap keeps luminance, so it cannot fix this. 8 flips in one direction against a 1-flip noise proxy is beyond any band |
| 3c GUARD deuteranopia | PASS | `n57-v-cvd.py cvd` on v1on foundry a3, iceline g2, kiln kb2, spire v6 entry: `CVD TOTAL: 4 / 4` (Delta-E 71.2 / 76.5 / 55.5 / 68.1); span p1 entry 70.8 | cyan and red-orange stay apart |
| 4 Nothing lost | PASS | summary47.txt:40-208 verbs37 27 / 21 / 31 / 31 / 31, `RED rows []` on r1 with colour56 on; the off side (v1none) shows the older palette (cover 22/36) | - |
| 5 Eye-only | PASS | `tint 36/36` on v1on (v1none 35/36); V3 yellow over 0.5 %: v1on 18/36 (worst 23.53 %) vs v1none 20/36 (worst 29.26 %); the four frames: no whole-frame wash, cyan still marks the usable edges, the enemies stay the most saturated warm thing | nearest call: the kiln key (#944a5c, rendered hue 341-342, S median 0.53) and the spire accent (#e67793, rendered median hue 351) sit between the magenta band (285-335) and the danger band. On the frames they read dusty rose / crimson, not the enemies' saturated orange-red. Spire's accent lands inside the cap's danger arc (350-70) and the grey check counts it as "hazards" (spire v2-mid hazards 3021 -> 16636 px) |

**GATE V1-colour56: FAIL (3b: greyscale guard)**

Smallest change that would pass: lower the value (not the hue or S) of the keys / accents / friezes where they raise the world's 75th
percentile luma, so that the paired grey check against v1none shows at most 1 PASS->FAIL flip. On these r1 shots that means the world p75
back to about the colour56-off level (paired median rise +0.045 -> about 0) on the 8 flipped shots listed above. Then re-run
`n57-v-cvd.py grey` on r2's five dirs of each side, read the pairs, and keep 3a / 3c / the MAIN. Lowering V keeps the MAIN (it counts
S >= 0.35, V >= 0.08). Also fix the label in r1-colour-counts.txt. Optionally move the spire accent off 351 deg so the grey check stops
counting it as a hazard.

---

opinion (uncalibrated, decides nothing): the kiln's rose bands make the hall read warmer and busier. Next to the orange husks the rose is
the colour to watch on GG's play.

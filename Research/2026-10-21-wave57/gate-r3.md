# NIGHT57 gate r3 (fresh batched critic; K = 3; entries E2-E5; E1 skipped as briefed)

Template: Research/2026-10-21-wave57/critic-gate.md. Brief: Research/2026-10-21-wave57/gate-r3-brief.md. MAIN / GUARDS: Logs/n57/slate.json.
Builder notes not read. Bot numbers are labelled "bot". My taste decides nothing.

**Build identity (applies to E2-E4):** every reel folder under Logs/feel/n57-r3/*/reel.json reads `"build":"SKYGRIND 0.1.0 (001c1fc4-dirty)"`,
started 12:24-12:45 on 2026-10-21 (001c1fc4 = the lead's "r3 built with every pick" commit; Builds/n57-r3 is dated 12:22).
**Off-flag scope (read from the logs):** `-skygrindOff55` turns off every row from NIGHT55 on, not only the n55 rows. b2off's walls are
S 0.855 (b2on: 0.607, colour56's cap on) and b2off's TORCH plate is yellow (yellow56 off). So the OFF runs are "all NIGHT55+ rows off", and
a row's own share is isolated only by the in-run twins (p1-raw, -nocap).

**Re-run by me (read-only):** `python -I Tools/night/n56-colour.py <side-level>/a357 --kinds entry,mid` on all ten colour folders,
`python -I Tools/night/n57-v-cvd.py grey <five a357 dirs> --kinds entry,mid` and `... cvd <five a357 dirs>` per side. Outputs kept in my
scratchpad only; the numbers quoted below are from those runs.

**Images opened (11 of 12):** k3on hit-next, hit-after; b2on crown37 p1, p1-raw, turn; b2off crown37 turn; b1on wick p2; v1on-spire v6-entry,
v1none-spire v6-entry; v1on-spire v5-mid; v1on-foundry a5-entry.

---

## E2 G3-kill55

Frames (described before the numbers):
- `Logs/feel/n57-r3/k3on/a355/shot-N55-K-hit-next.jpg`: the hit husk is a white silhouette over nearly its whole body (white flash); no dark crack
  or hole is visible on it; chunks of a killed husk in the air above; "ROOM CLEAR · UNDER PAR" centre text.
- `.../shot-N55-K-hit-after.jpg`: the same husk back to red-orange; at most a faint darker red patch near the crosshair; blood splats on the floor;
  no clear dark mark on the body.

| Line | Verdict | Evidence |
|---|---|---|
| 1 Evidence real | **FAIL** (half) | a355: k3on `kill55 on True ... kill55 records 10 (extended 5 ...)`, k3off `kill55 on False ... kill55 records 0` (summary47.txt lines 256 / 261), build 001c1fc4. Frames match the numbers (white flash = `the changed pixels' median H 60 S 0.00 V 0.97`). But **a354 does not evidence the row**: m3on and m3off (lines 266 / 271) print only `move54 on True`, never kill55's state, and both read the same `freezes 4 (kill 4; skipped 0; 0.200 s granted in all)`, `HOLD frames 9 (0.150 s)`: no hold was extended in the ON run, so a354 never exercised kill55. |
| 2 MAIN | **FAIL** | Measured fallback: `MARKS NEXT FRAME (changed, V < 0.12) 0.00 % of the body (want >= 5 %)` on k3on and `0.00 %` on k3off; at +0.5 s `mark pixels ... 0.02 % (want >= 1.0 %)` ON vs 0.09 % OFF; the reel's own verdict `damage NO` (RED a355). The ladder half passes: ON holds 3, 3, 5, 5, 5, 5 frames (`ladder ok`, `kills 1 -> 3 in 0.967 s`) vs OFF 3 frames each (`ladder NO`). The slate's MAIN proper (the blind pair, both orders) is not in the evidence: not evidenced. |
| 3 GUARDS | PASS (as read, hollow) | a354 jitter `0.191` ON and `0.191` OFF (<= 0.191). Hollow: see line 1, the row never fired in a354, so this guard did not test it. |
| 4 Nothing lost | PASS | r3 (every row ON): `PASS: total 27/27 on n52-foundry`, `21/21 on n52-iceline`, `31/31 on n52-span`, `31/31 on n52-spire`. Kiln's RED rows are E1's: n52-kiln rows 17/19/22/23 `no grapple node in clear sight of a floor spot` (grapple owned via the campaign place kiln57 moved), n57-kiln rows 40/41 (parry/launch staging); rows 40/41 pass on the four other levels with kill55 ON. Off side shows the older behaviour (`held 3 frames ... row off` x6). |
| 5 Eye-only | PASS | Enemies red-orange, gun and HUD cyan, no whole-frame tint, centre text overlaps no HUD element. The white body flash is on both sides (`changed next frame 97.2 %` ON, `97.5 %` OFF), so it is not the row's. |

**GATE G3-kill55: FAIL (1, 2)**
Smallest change: marks that survive the flash. The next frame is 97 % white, so no mark can show; lay the crack / hole outside the flash, or
end the flash before the next frame, so the dark (V < 0.12) pixels cover >= 5 % of the body on hit-next. Also make a354 print kill55's state
and chain its kills so that a hold is really extended, then re-read the jitter. And run the blind pair.

---

## E3 V3-grade55 (THE CROWN's tells; THE WICK's ride cue)

Frames:
- `b2on/a155/shot-N55-crown37-p1.jpg`: THE CROWN's ring is a red-orange burst with a pale ring and a yellow gem; green / ochre walls; red-orange
  sky bands on the ceiling; "THE SPIRE" title.
- `.../shot-N55-crown37-p1-raw.jpg`: almost the same frame. The ring is the same. The left wall is slightly more golden and the right wall
  slightly less dark green. I cannot see a grade difference.
- `.../shot-N55-crown37-turn.jpg` (graded) and `b2off/.../shot-N55-crown37-turn.jpg`: both turn frames have the ring white and teal and a
  large red-orange ceiling band (dusk sky) across the upper frame. In OFF the TORCH's top plate is yellow; in ON it is pale.
- `b1on/a155/shot-N55-wick-p2.jpg`: THE WICK's dark cage with a yellow core, in a violet-blue shaft; cyan trims on the upper ride panels; no
  red-orange on the walls.

| Line | Verdict | Evidence |
|---|---|---|
| 1 Evidence real | PASS | Build 001c1fc4. b2on `row grade55 on; ... graded live True (boss crown37, weight 1.00)`, b2off `row grade55 off (the RED run); ... graded live False`, b1on `row grade55 on; GRADE GREEN ... graded live True (boss wick)` (lines 308 / 313 / 318). The frames agree with `graded vs raw walls dH 6 ... dS -0.040 ... differs False` (p1 vs p1-raw look the same). |
| 2 MAIN | PASS | TELLS kept, in-run twin: `graded n 495 ... vs raw n 502` = 0.986. Before: NIGHT56 `Logs/feel/n56-r5/summary47.txt`: `graded n 362 ... vs raw n 533` = 0.68. The noise of the twin capture: b2off (no grade) `graded n 514 vs raw n 523` = 0.983, and n56-r2 `493 vs 518` = 0.95. So 495 vs 502 is "no fewer than the no-grade count" within the meter's own gap, and 0.68 -> 0.99 is beyond noise. |
| 3 GUARDS | PASS | THE WICK's pass kept: b1on `GREEN` (TELLS `graded n 608 ... vs raw n 151 ... ok`; `HOT WALL GREEN`). Ride panels carry cyan: `RIDE CUE GREEN ... pooled cyan share 13 % of 424 panel cells (>= 10 %, each shot >= 20 cells too: ok)`. The wick p2 frame shows cyan panel trims. No b1off was run: "kept" is read as b1on GREEN. |
| 4 Nothing lost | PASS | r3 verbs37 with grade55 ON: spire `31/31`, foundry `27/27`. |
| 5 Eye-only | PASS | Tells stay red-orange on THE CROWN and cyan is 5.5:1 against the graded p1. The red-orange ceiling band is in both turn frames (ON and OFF), so it is the base look, not the row. |

**What makes b2on RED:** two items in its own GRADE line. (a) `walls near danger p1 0.18 / turn 0.39 / p2 0.12 (<= 0.25: NO)`: the turn frame
is over the line. b2off is RED on the same item (`p1 0.36 / turn 0.31 / p2 0.19`), and its OFF side has every NIGHT55+ row off, so the turn's
+0.08 cannot be put on grade55 alone (p1 and p2 improved). (b) `graded vs raw walls dH 6 (>= 25) dS -0.040 (|.| >= 0.10) dV -0.017: differs
False`: on THE CROWN the grade barely changes the walls. The tells are kept because the grade is close to a no-op on spire.

**GATE V3-grade55: PASS**. Caveat for the lead: on THE CROWN the row does almost nothing (differs False). Its visible value is THE WICK's ride
cue and walls (`dH 135 ... differs True`). The turn's near-danger share is over 0.25 on both sides. It is the base look's red-orange sky band
and is still open.

---

## E4 V1-colour56 + V4 rim56 / yellow56

Frames:
- `v1on-spire/a357/v6/shot-N56-n52-spire-v6-entry.png`: a green hall. Cyan trims, rose-pink accent stripes along the walls, four red-orange husks
  on a ramp, and a pale cyan-white plate on the THUNDERCLAP. A green cast over the world.
- `v1none-spire/.../v6-entry.png`: the same view, more yellow-green. No rose stripes. The THUNDERCLAP plate is yellow-green and a yellow strip
  runs at the bottom right. Husks red-orange.
- `v1on-spire/a357/v5/shot-N56-n52-spire-v5-mid.png`: two very large yellow orbs (parryable globs) fill the right and centre and overlap the
  gun; rose stripes and cyan trims on the left.
- `v1on-foundry/a357/a5/shot-N56-n52-foundry-a5-entry.png`: a lava trench with bright orange-yellow edges, a pale yellow plate on the column's top,
  a husk at the left with a yellow-orange halo, three husks ahead, cyan trims.

Evidence real: build 001c1fc4. v1on lines `rows: colour56 ON, rim56 ON (...), yellow56 ON (gun plates moved off the yellow arc 3 ...)`,
`THE WORLD SATURATION CAP ON ... (cap 0.70)`; v1none `-skygrindOff56`, `rows: colour56 OFF, rim56 OFF ..., yellow56 OFF`, `cap 1.00` (summary47.txt
lines 426-448 and the four other level blocks). The OFF side has every n56 row off at once (colour56, rim56, yellow56, tokens56). Only the in-run
`-nocap` twins isolate colour56's cap; rim56 and yellow56 have no isolated twin. The frames agree with the numbers (the yellow plate is gone ON, and
there are rose accent stripes ON).

My n56-colour.py TOTAL lines (ON / NONE):

| level | V1 cover (key or accent >= 3 %) | world out-saturates tells (sat FAIL) | V2 body over env p90 | V3 yellow frames <= 0.5 % |
|---|---|---|---|---|
| foundry | 6/6 / 5/6 | 0/6 / 6/6 | 4/14 (29 %) / 0/14 | 4/6 / 1/6 |
| iceline | 12/12 / 0/12 | 2/12 / 8/12 | 13/17 (76 %) / 9/16 | 12/12 / 12/12 |
| kiln | 6/6 / 5/6 | 1/6 / 6/6 | 11/15 (73 %) / 14/15 | 5/6 / 4/6 |
| span | 6/6 / 6/6 | 0/6 / 5/6 | 15/16 (94 %) / 15/16 | 6/6 / 3/6 |
| spire | 6/6 / 6/6 | 0/6 / 6/6 | 15/15 (100 %) / 1/15 | 5/6 / 0/6 |
| **all** | **36/36 / 22/36** | **3/36 / 31/36** | **58/77 = 75 % / 39/76 = 51 %** | **32/36 / 20/36** |

Per-frame key|accent cover, ON: min 3.2 % (foundry), medians from 7.5 % (foundry) to 19.9 % (span).
CAP A/B (-nocap twins, ON run): world over the tells `5/6 -> 0/6` foundry, `6/12 -> 2/12` iceline, `6/6 -> 1/6` kiln, `4/6 -> 0/6` span, `5/6 -> 0/6` spire.
Greyscale (`n57-v-cvd.py grey`): ON `GREY TOTAL: 25 / 36`, NONE `GREY TOTAL: 25 / 35 ...; 1 n/a` (r1 ON was 18/36).
Deuteranopia (`n57-v-cvd.py cvd`): ON `CVD TOTAL: 36 / 36`, NONE `CVD TOTAL: 36 / 36`.

### V1-colour56

| Line | Verdict | Evidence |
|---|---|---|
| 1 Evidence real | PASS | As above. |
| 2 MAIN (measured fallback) | PASS | Key or accent at S >= 0.35 on >= 3 % of the frame: ON 36/36 frames (min 3.2 %) vs NONE 22/36 (iceline 0/12). The slate's blind pair was not given: the lead named the fallback. |
| 3 GUARDS | PASS | World out-saturates the tells: ON 3/36 vs NONE 31/36 (slate bound: <= the baseline's, 28/36 at NIGHT56). Greyscale held: 25/36 vs 25/35 (r1's 18/36 regression gone). Deuteranopia: 36/36 both sides. |
| 4 Nothing lost | PASS | r3 verbs37 (colour56 ON): foundry 27/27, iceline 21/21, span 31/31, spire 31/31. Kiln's RED rows are E1's (see E2 line 4). |
| 5 Eye-only | PASS (note) | Cyan is still the player's, husks red-orange, `tint` 35/36 ON = 35/36 NONE (spire v6-entry tint 50 % ON vs 45 % NONE, under the 60 % line). The spire accent (measured hue 350-353, S 0.48) reads rose-pink on v6-entry, 15 deg from the danger band per the palette print (`accent's 27 / 15 deg`). It is outside both role bands, but it is the closest hue to red-orange on any level: a watch item. |

**GATE V1-colour56: PASS**

### V4 rim56

| Line | Verdict | Evidence |
|---|---|---|
| 1 Evidence real | PASS (not isolated) | `rim56 ON, the lit side's gain written 2.21` vs `rim56 OFF ... 0.00`. The NONE side also has colour56 off. colour56's cap lowers the env p90 that the rim is judged against (spire env S90 0.71 ON vs 1.00 NONE), so part of the gain is the cap's. |
| 2 MAIN | **FAIL** | Enemies' body over the env p90: ON 58/77 = 75 % (want >= 90 %); foundry 4/14 = 29 %, kiln 73 %, iceline 76 %. NONE 39/76 = 51 %. |
| 3 GUARDS | PASS | Red-orange stays the enemies': husks red-orange on every frame I opened. |
| 4 Nothing lost | PASS | As colour56. |
| 5 Eye-only | PASS | No new tint; the husks read on spire v6-entry on both sides. |

**GATE V4-rim56: FAIL (2)**. Smallest change: foundry. 10 of 14 husks there are under the env p90 (env V90 0.68-0.71 on the foundry lines).
Lift the lit side on foundry until those bodies pass, or judge foundry against its own p90 after the cap. Kiln and iceline need about 3 more
husks each.

### V4 yellow56

| Line | Verdict | Evidence |
|---|---|---|
| 1 Evidence real | PASS (not isolated) | `yellow56 on: a DIM yellow (hue 40-76, value under 0.50, none from 0.75) held at S 0.45` vs `yellow56 off`. The gun plate is yellow in NONE and pale in ON. Note: S 0.45 sits just under the meter's S > 0.5 line. |
| 2 MAIN | **FAIL** | Non-parry yellow <= 0.5 % per frame (`yellow outside them`, orbs excluded): ON 2/36 frames over, `foundry-a5-entry ... yellow outside them 0.88 % FAIL` and `spire-v5-mid ... parry orbs by shape 2, yellow outside them 1.28 % FAIL`. NONE: 16/36 over (spire up to 16.36 %). By eye, spire v5-mid's yellow is all parry orbs (the remainder is their fringe: a meter artifact). foundry a5-entry's 0.88 % is real non-parry yellow: the lava trench's bright edges, the column's pale yellow top plate and a husk halo. A large gain (16 -> 2), but under the named minimum. |
| 3 GUARDS | PASS | Red-orange stays the enemies'. |
| 4 Nothing lost | PASS | As colour56. |
| 5 Eye-only | PASS | Parry orbs stay bright yellow (v5-mid). |

**GATE V4-yellow56: FAIL (2)**. Smallest change: on foundry a5, take the column top plate and the lava edge's 45-70 deg fringe off the
yellow arc (or under S 0.5), and have the orb detector fold an orb's fringe into the orb. Then re-run n56-colour.py: 0/36 frames over.

---

## E5 D-11 wick55 (sim row; every number bot)

No frames, as briefed. Logs: Logs/n57/g4-wick-{on,off,on-s41,off-s41,casual-on,casual-off}.txt (committed f029df57, 11:18 tonight).

| Line | Verdict | Evidence |
|---|---|---|
| 1 Evidence real | PASS | Headers: ON `rows [wick55]`, OFF `rows []`; the stats lines `wick55 ON; bot` / `wick55 OFF; bot`. The r3 Player build runs it live: b1on `wick55's ride-only rule suspended for the placement`. |
| 2 MAIN | PASS (with a caveat) | Bot: `dealt in windows median 6 %` OFF (seeds 1-40 and 41-80) -> `57 %` ON (both halves); casual 4 % -> 57 %. **Caveat:** the headline is mostly a change of definition. OFF `cracked plate 6 %` = the 6 % "in windows"; ON `cracked plate 6 %, P3 crack-open / bare core 50 %` sums to about the 57 %, so with the row on the bare core counts as a window (it was already 48 % / 44 % of damage OFF). Compared like for like (the state breakdown printed the same way on both sides), chip falls `51 %` -> `44 %` (1-40) and `51 %` -> `43 %` (41-80), against a 0-1 point gap between seed halves: real, but about 8 points, not 51. The parry-earned `stagger window` stays `0 %` median on both sides. |
| 3 GUARDS | PASS | Deaths pooled over seeds 1-80: OFF 2 + 9 = 11, ON 7 + 4 = 11 (equal). Median kill time in the bot band 30-60 s: ON `57.5 s` / `54.6 s` (OFF 45.5 / 47.6). Watch: casual median `60.9 s` ON (52.6 OFF) is 0.9 s over the band, but casual is not the named guard run (seeds 1-80 are the n47human files). |
| 4 Nothing lost | PASS | r3 (wick55 ON) foundry verbs37 `27/27`; b1on's a155 THE WICK `GREEN` with the row live. |
| 5 Eye-only | PASS (n/a) | Sim row. The only visible change is the existing OPEN bar state shown during the bare core. The b1on wick p2 frame (row on) shows the boss bar overlapping no other HUD element. |

**GATE D-11 wick55: PASS**. Caveat for the lead: the 6 -> 57 % mostly relabels the bare core as a window. The like-for-like shift is chip
51 -> 43-44 %, and the stagger window still earns 0 % for the bot.

---

opinion (uncalibrated): the spire accent at hue ~352 reads close to red-orange at a glance, and THE CROWN's grade is effectively off. Neither
decides anything.

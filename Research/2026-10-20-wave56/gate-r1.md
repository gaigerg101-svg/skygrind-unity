# NIGHT56 gate r1: the batched fresh critic's verdicts (build Builds/n56-r1, port/stage3 6b1c00c9)

Critic: fresh, batched; rubric `Research/2026-10-20-wave56/critic-gate.md`, evidence list `gate-r1-brief.md`, slate text
`Logs/n55/slate.json`. I read no builder report, code comment or commit message. Images opened: the 20 listed, no others. Commands
run (read-only): grep / sed / ls over the named logs, `python -I Tools/night/n55-decor-share.py Logs/feel/n56-r1/<side> --frames entry`
on all six d-sides, and PIL pixel reads on four listed pair frames and two listed kill frames. I did not look for
`Logs/n56/pairs-r1-key.json`.

Conventions: "S" is the summary file `Logs/feel/n56-r1/summary47.txt` (line numbers given). Bot numbers are labelled "bot". "N/A" marks a
rubric line that the entry's type cannot carry (a harness or reel-code entry has no frames, look or first-minute player change); N/A is
not a pass of anything, only "not applicable". Each frame was described from the image alone.

---

## 1. P0-smoke (harness: SliceSmoke)

| line | score | result | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 0 | FAIL | `Logs/player/n55-smoke-steal-n56-r1.log:193` "[smoke] 230.0s DONE: all steps passed"; `:66` start line "focused True, runInBackground True ... devices [... SwitchProControllerHID 'Wireless Gamepad' on]"; `:143` "tape: 486 steps taped (6950 B), replayed in this process: EQUAL (1 of 1 checkpoints, 486 steps)"; BUT `:192` "n55 input summary: focus changes 0, unfocused 0 of 77855 frames" while `Logs/n55/smoke-steal-n56-r1.out` says "steal at +67 s: notepad pid 132524" and "steal at +210 s: notepad pid 142304" | 0 failed is met, but the planted steals never took focus from the game (0 focus changes), so the "focus lost t=..., run continued" line the notes require does not exist: the steal was not tested. The run also had three builder agents active (brief); the doneWhen asks for none. |
| 2 Frames | - | N/A | - | harness entry; the doneWhen names no frame |
| 3 Nothing lost | 0 | FAIL | no evidence given | "outside -skygrindSmoke the game still pauses on focus loss (a test)": not evidenced. (Every smoke step PASS, pad-only steps `:238-242` included.) |
| 4 Look | - | N/A | - | harness |
| 5 Not basic | - | N/A | - | harness |

**GATE P0-smoke: FAIL (1, 3)**
Smallest change: make the planted steal really take the foreground (the smoke must print focus changes >= 2 and a "focus lost t=..."
line near 60 s and 200 s with the run continuing to DONE), and run the focus-pause test outside -skygrindSmoke; one run with no agent active.

---

## 2. P0-wickGrade (grade55: THE WICK's and THE CROWN's palettes)

Frames (described before their numbers):
- `Logs/feel/n56-r1/b1on/a155/shot-N55-wick-p1.jpg`: THE WICK's chamber with blue-slate / teal walls, a desaturated grey-beige floor, the
  yellow core behind black bars, the HUD unchanged.
- `.../b1on/a155/shot-N55-wick-p1-raw.jpg`: the same view ungraded: olive-ochre and green walls, an olive floor; the graded frame is
  plainly cooler.
- `Logs/feel/n56-r1/b2on/a155/shot-N55-crown37-p1.jpg`: THE CROWN's red-orange ring of tells, a purple-red sky, an ochre left wall, a dark
  green right wall, cyan floor edges.
- `.../b2off/a155/shot-N55-crown37-p1.jpg`: the same view with the row off; to my eye almost identical to the ON frame (the ON left wall a
  shade darker).

| line | score | result | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 0 | FAIL | THE WICK, S:3: "walls' distance p1 159 / turn 130 / p2 105 (>= 30: ok)", "graded vs raw walls dH 139 ... differs True", "TELLS ... graded n 609 S 0.973 ... the most saturated other class Body S 0.902: ok", "cyan vs the graded p1 frame 5.4:1", "WHOLE FRAME (D-16 ...) distance 157 / 130 / 95 (>= 20: ok)". THE CROWN, S:13: "walls' distance p1 79 / turn 4 / p2 166 (>= 30: NO)", "graded vs raw walls dH 15 (>= 25) dS -0.079 (\|.\| >= 0.10) ... differs False", "WHOLE FRAME (D-16, HUD in) ... distance 27 / 20 / 159 (>= 20: NO)". Masks: `ls` shows a `-mask.png` beside every b1on / b2on frame. | THE WICK meets every threshold; THE CROWN misses three (turn 4 deg from danger, graded vs raw not different, D-16). |
| 2 Frames | 0 | FAIL | the four frames above | THE WICK's change is obvious; THE CROWN's ON and OFF p1 frames cannot be told apart by someone not told what changed. (turn / p2 frames exist on disk for both bosses; they were not in my list.) |
| 3 Nothing lost | 1 | PASS | S:8 b1off "graded live False ... differs False"; S:18 b2off the same; tells kept (b1on 609 px S 0.973 vs raw 150 px S 0.903; b2on 395 of 493 px, S 0.871) | the off side is NIGHT54's look; view-only row; tells kept |
| 4 Look | 0 | FAIL | `Logs/feel/n56-r1/b2on/a155/a155-crown37.txt` "crown37-turn ... wall n 1697 H 8 S 0.643 ... nearDanger 0.31", whole frame "H 353 S 0.560" | THE CROWN's turn grades the walls into the danger hue (red-orange is danger only) and pulls the whole frame to H 353. THE WICK passes (walls 213 / 242 / 268; core, tells and HUD keep their colours, so it reads as a room palette, not a tint). Flag for the next gate: THE WICK's p2 walls H 268 S 0.559, whole frame H 277 (a155-wick.txt), a violet 23-32 deg from magenta (RIVAL); the reel does not check the RIVAL hue. |
| 5 Not basic | 0 | FAIL | frames above | THE WICK's palette is a first-minute change; THE CROWN's is not visible |

**GATE P0-wickGrade: FAIL (1, 2, 4, 5)**: every miss is THE CROWN's; THE WICK's half passes all five lines.
Smallest change: give THE CROWN a turn hue away from red (walls >= 30 deg from hue 12 at the turn, near-danger share <= 0.25) and a p1
shift big enough for dH >= 25 deg or |dS| >= 0.10 against raw; re-run a155 on n52-spire on and off.

---

## 3. P0-hotWall (grade55: THE WICK's hot walls read as a state)

Frames:
- `Logs/feel/n56-r1/b1on/a155/shot-N55-wick-burn.jpg`: THE WICK's core (red-orange, telling) at left, a red-orange floor disc, a dark slate
  wall filling the right half; no wall shows any burn.
- `.../b1on/a155/shot-N55-wick-cold.jpg`: the same angle, the core yellow, the bar reading cyan "OPEN 1.3"; the right wall looks the same
  as in the burn frame.

| line | score | result | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 0 | FAIL | S:3 "HOT WALL RED: the burn's input: burns 0 -> 0 after 8 placement(s) (angle 21 deg, y 2.6), heat drawn 0.00 on 0 quads; burn frame: hot patch S 0.222 (n 1115) vs the other walls S 0.655 (+0.15 wanted: NO), the most saturated other class Body S 0.691 (NOT the most saturated)". Met: "no wall burning: red-orange share of the world p1 0.003 / cold 0.001" vs the off run S:8 "p1 0.039 / cold 0.027" (<= 10 % and <= half); "the cold seams: hue 228 S 0.25 (144 deg from danger ...: ok)" | no burn was ever placed, so "a burning wall is the most saturated region" is not shown; the cyan cue on THE WICK's ride walls (notes) is not evidenced |
| 2 Frames | 0 | FAIL | burn / cold frames | the two frames show the same unlit wall: no burning wall on screen |
| 3 Nothing lost | 1 | PASS | S:8 b1off "the cold seams: hue 16 S 0.88 (4 deg from danger ...: NO)", red-orange share 0.039 | the off side shows the shipped striped look; view-only |
| 4 Look | 1 | PASS | S:3 seams hue 228, 144 deg from danger; red-orange 0.3 % of the world | stripes moved off the danger hue; nothing whole-frame from this slice |
| 5 Not basic | 0 | FAIL | as line 1 | the stripes' removal shows, but the "state" (a wall that burns and stands out) never appears |

**GATE P0-hotWall: FAIL (1, 2, 5)**
Smallest change: make the reel's burn land (burns >= 1 with heat drawn on > 0 quads; it tried 8 placements and got 0), then show the hot
patch S >= other walls + 0.15 and the most saturated class; print the cyan cue's share on the ride walls.

---

## 4. P1-wickWindow (wick55, sim: punish the window)

No frames given.

| line | score | result | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 0 | FAIL | bot, `Logs/n56/wick-bot.out:10` skilled-on "killed 40/40 ... kill time median 57.5 s ... deaths median 0 (total 7) ... dealt in windows median 57 %"; `:9` "hot-wall burns 0 ... of 183 hits taken (0 %)"; `:20` casual-on "killed 40/40; kill time median 60.9 s". Player: S:23 bfon "damage dealt while the bar read OPEN 40 % (>= 50 %) ... core punches 0, inside a window 0 (the first at never: no frame)"; S:187 a4on "crack-opens 0, core punches 0". Wick55Tests not run (brief). | bot thresholds met (bot); the Player proof fails (40 % < 50 %, 0 core punches; the notes want >= 1 core punch inside a window on a named frame); "every other attack's numbers unchanged (a test)" is not evidenced. Flag: the by-state medians barely move with the row (`:3` off: stagger window 0 %, cracked plate 6 %, crack-open 48 %, chip 51 %; `:9` on: 0 / 6 / 50 / 44 %) while "dealt in windows" goes 6 -> 57 %: most of the 57 % is the crack-open being counted as a window under wick55, not the bot playing differently. |
| 2 Frames | 0 | FAIL | S:23 "the first at never: no frame" | no named frame of a punch inside a window |
| 3 Nothing lost | 0 | FAIL | off = NIGHT54 met: `wick-bot.out:4` skilled-off "median 45.5 s, p90 53.7 s ... damage taken median 72 ... (total 2)" equals `NIGHT54-LOG.md:7` / `:66` (45.5 s, p90 53.7, taken 72, deaths 2), and `NIGHT56-LOG.md:26` "fingerprints plain / --off55 / --off56 EQUAL" | the other attacks' numbers (a test) are not evidenced |
| 4 Look | 0 | FAIL | none | not evidenced: no frame for this entry |
| 5 Not basic | 0 | FAIL | S:23 vs S:28: bfon and bfoff print the same fight (40 %, punches 27, ember 8, stagger windows 6, alive at 150 s, bar 40 %); S:187 vs S:192 a4on / a4off the same (core punches 0 both) | in the Player, ON and OFF are the same fight; the change (hot-wall hits 298 -> 0, median 45.5 -> 57.5 s) exists only in the bot |

**GATE P1-wickWindow: FAIL (1, 2, 3, 4, 5)**
Smallest change: give the Player pilot a core punch inside an OPEN window (bfon's pilot prints "shots 0, rides 0, punches 27" and never
lands one) so FIGHT MODE prints >= 50 % and a named frame, with OFF differing; run Wick55Tests in the Editor.

---

## 5. P0-step (step55: the step-down camera)

No frames given.

| line | score | result | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 0 | FAIL | S:33 c1k (on) "step up 0.40 m at 5.0 m ... the step CROSSED (the feet 0.225 m ...; step55 held 1) \| camera's largest one-frame height change at the step 0.067 m"; S:38 c1koff "0.090 m ... flat-ground noise 0.000 m ...: the step over 2x the noise yes"; S:43 foundry "no step line on this level"; golden / fingerprints `NIGHT56-LOG.md:26` "golden 0 mismatches, 22/22 exact; fingerprints plain / --off55 / --off56 EQUAL" | a defect is measured (off 0.090 m > 2 x 0.000 m), so DONE-no-defect does not apply; ON is 0.067 / 0.090 = 74 % of OFF (want <= 50 %); and the line was a step UP, not the step-down the doneWhen names ("43 drops over the ground snap left out: falls") |
| 2 Frames | - | N/A | - | the doneWhen names no frame |
| 3 Nothing lost | 0 | FAIL | brief: a354 not run on n56-r1; S:235 verbs37 "PASS: total 27/27 on n52-foundry" | flat-ground jitter <= 0.191 is not evidenced; fingerprints EQUAL and verbs37 27/27 hold |
| 4 Look | - | N/A | - | camera motion, no frame |
| 5 Not basic | 0 | FAIL | S:33 / S:38 | 0.090 -> 0.067 m per frame on a 0.225 m step is not shown to be something a player notices |

**GATE P0-step: FAIL (1, 3, 5)** (not DONE-no-defect: the off value 0.090 m exceeds 2x the flat noise 0.000 m)
Smallest change: hold the snap until ON <= 0.045 m on the kb2c line, find a step-down under the ground snap (or print that none exists on
any level), and run a354 for the jitter.

---

## 6. P0-ssao (ssao55: SSAO's noise without TAA)

No frames given. Changed pixels |dL| > 2 %, still camera, read from the a255 lines:

| side | room | FLOOR | OFF | ON | OFF / FLOOR | ON / OFF | lum dON-OFF |
|---|---|---|---|---|---|---|---|
| c1k S:33 | kiln kb2c | 1.73 % | 1.55 % | 1.75 % | x0.89 | x1.13 | -0.001 |
| c1k S:33 | kiln kb1 | 10.84 % | 9.39 % | 10.65 % | x0.87 | x1.13 | 0.000 |
| c2f S:43 | foundry a1 | 1.37 % | 1.57 % | 1.59 % | x1.15 | x1.01 | 0.000 |
| c2f S:43 | foundry a2 | 1.53 % | 1.79 % | 1.75 % | x1.17 | x0.98 | 0.000 |
| c1koff S:38 | kb2c / kb1 | 2.06 / 14.23 % | 2.07 / 14.09 % | 2.10 / 14.43 % | x1.00 / x0.99 | x1.02 / x1.02 | -0.001 / 0.000 |
| c2foff S:48 | a1 / a2 | 1.30 / 1.67 % | 1.60 / 1.85 % | 1.55 / 1.78 % | x1.22 / x1.11 | x0.97 / x0.96 | 0.000 / 0.000 |

Slow yaw (1 deg/s): kb2c OFF 3.58 % / ON 3.62 % (x1.01); a1 OFF 5.35 % / ON 5.37 % (x1.00). Frame times printed, but measured beside three
builder agents (not "alone"). In every room OFF is under 2x FLOOR (max x1.22), so per the notes the entry closes as no defect measured.

Caveats the lead should carry with the verdict:
- every a255 side ran on the low preset: `Logs/player/n47-reel-n56-r1-c1k.log:66` "[quality] AUTO picked low (benchmark p95 250.00 ms);
  preset in use low" (the same line in c1koff / c2f / c2foff);
- FLOOR and OFF mean luminance are equal in every room (0.369 / 0.369, 0.217 / 0.216, 0.297 / 0.296, 0.293 / 0.292), so the log does not
  show the AO darkening anything; "no defect" is shown for the measured preset only;
- the row's ON is x1.13 of OFF in both kiln rooms (more change, not less).

**GATE P0-ssao: DONE-no-defect** (on the AUTO-low preset: OFF / FLOOR x0.87-x1.22 in four rooms, both sides; luminance within 0.001)
Follow-up, not a gate miss: one a255 SSAO pass with the preset forced high before GG's next look.

---

## 7. P0-decor (decor55: the level accents made visible)

### Blind pairs (answered per order, before comparing)
Question each time: "which frame has a colour accent (a deliberate decor colour beyond the base materials)?"
- pair-1 order 1 (`pair-1-1A.png` / `pair-1-1B.png`): 1A = foundry THE YARD, ochre left wall, green right wall, cyan pads, no band.
  1B = the same view with lime bands on the left wall, the right pillar and the back walls, and the floor-edge strips turned yellow.
  **Answer: B.**
- pair-1 order 2 (`pair-1-2A.png` / `pair-1-2B.png`): 2A = the lime-banded view; 2B = the plain view. **Answer: A.**
- pair-2 order 1 (`pair-2-1A.png` / `pair-2-1B.png`): 1A = an orange-walled room with lava on the right, THUNDERCLAP in hand, a "B to
  jump" hint, no band. 1B = the same view with green diagonal bands on both walls. **Answer: B.**
- pair-2 order 2 (`pair-2-2A.png` / `pair-2-2B.png`): 2A = the green-banded view; 2B = the plain view with the hint. **Answer: A.**
- Compared afterwards: in every order I picked the banded frame, and the banded frame swapped position between the orders. The key
  (held by the lead) decides whether "banded" is ON in both pairs.

### Rubric
Meter output (my runs of `n55-decor-share.py`, entry frames):

| level | frame | ON accent share / p95 lum | OFF accent share | usable cyan p95 lum (ON) | least role gap |
|---|---|---|---|---|---|
| foundry | a3 / a5 / b1 | 2.06 % 0.516 / 1.62 % **0.888** / 1.63 % **0.870** | 0.14 / 0.35 / 0.18 % | 0.802 / 0.786 / 0.837 | 28 deg to parry |
| iceline | g2 / g3 / g4 / g5 / g7 / g8 | 4.38 % **0.716** / 1.40 % 0.553 / 2.45 % 0.583 / 3.17 % 0.619 / 2.33 % 0.625 / 3.16 % 0.390 | 0.11 / 0.10 / 0.15 / 0.18 / 0.20 / 0.03 % | 0.691 / 0.787 / 0.802 / 0.798 / 0.787 / 0.790 | 35 deg to cyan |
| kiln | kb2 / kb2c / kb3 | 1.44 % 0.599 / 2.12 % 0.539 / 1.80 % 0.610 | 0.17 / 0.16 / 0.16 % | 0.799 / 0.848 / 0.794 | 49 deg to cyan |

| line | score | result | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 0 | FAIL | meter lines, e.g. d1on "n52-foundry a5 entry \| decor55 #c0db53 (hue 72, s 0.62; least role gap 28 deg to parry; parry 28): share 1.62 %, p95 lum 0.888 \| ... usable cyan share 3.74 %, p95 lum 0.786"; b1 entry "0.870" vs "0.837"; d2on g2 entry "0.716" vs "0.691" | met: share up >= 3x on all 12 entry frames (lowest foundry a5 1.62 / 0.35 = 4.6x); gaps >= 20 deg (O-08 printed: foundry 28 deg to parry); >= 1 % absolute (lowest iceline g3 1.40 %); RED off (OFF 0.03-0.35 %). Missed: usable cyan is not the brightest class on 3 of 12 entry frames (foundry a5, b1; iceline g2). |
| 2 Frames | 1 | PASS | the 8 pair frames | the banded frame is obvious in every order (scored against the key by the lead) |
| 3 Nothing lost | 1 | PASS | S:38 / S:48 "no band (row off: NIGHT54's decor)"; OFF accent 0.03-0.35 %; S:235 verbs37 27/27 on n52-foundry with the rows on | off is NIGHT54's decor; verbs hold |
| 4 Look | 0 | FAIL | my pixel read of `pair-1-1B.png` (the banded frame): floor-edge strips left (x 0-200, y 740-775) and right (x 1740-1920, y 740-780): median hue 60, S 0.87, V 0.88 (`pair-1-1A.png` same boxes: hue 42-47, S 0.5); the parry hue is 44 (meter: hue 72 with parry gap 28; decor54 hue 41 with parry gap 3) | on foundry the accent's lit floor-edge strips render yellow, 16 deg from parry yellow (< 20; yellow is parryable). The token's 28 deg gap does not survive the lighting. The wall bands themselves read hue 83, V 0.34 (fine); kiln / iceline bands hue 139-141 (fine). |
| 5 Not basic | 1 | PASS | pair frames | lime / green bands at 2.7-3.0 m on every entry frame; visible on entry |

**GATE P0-decor: FAIL (1, 4)**. Pair answers: pair-1 order1 B, order2 A; pair-2 order1 B, order2 A.
Smallest change: drop the accent's value on the lit floor-edge strips (or leave those trims off the accent) so its p95 luminance stays
under usable cyan's on every entry frame and its rendered hue stays >= 20 deg from parry on the frame; re-run the meter on all six sides.

---

## 8. P1-kill (kill55: kills that crunch)

Frames:
- `Logs/feel/n56-r1/k1on/a355/shot-N55-K-chain.jpg`: a foundry room, three red-orange husks left of centre, a white hit burst on the right
  with dark-red chunks flying, the HUD "x3 D DULL +12 KILL", lime wall bands.
- `.../shot-N55-K-debris.jpg`: one husk left beside a cyan pillar, the floor strewn with small red chunks and dark stains, "x6".
- `.../shot-N55-K-hit-before.jpg`: four husks bunched, a cloud of chunks above them, "DOOR OPEN" and a "ROOM CLEAR · UNDER PAR 10.1 S ·
  PAR 18 S" card.
- `.../shot-N55-K-hit-next.jpg`: the same view; the targeted husk is white (hit flash) with a pale violet square on its body, a cyan
  muzzle spark.

| line | score | result | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 0 | FAIL | S:53 "kill 3: held 3 frames (0.050 s), chain 1, play 5.067 s ... rules kill 0.050 + kill55 0.000 = 0.050 s (first)" (kills 4-5 the same; "kill 6: held 0 frames"); "kill55 records 10 (extended 0, held 0, skipped 0)"; "ladder NO". Met: kill 1 0.050 s; "debris at +1.500 s: kill 1: 10 alive ... kill 6: 10 alive"; "changed next frame 97.6 %"; "cap ok"; spaced "0.150 s held over 4 kills". a354 not run (brief). | kills land 0.51 s apart (4.042 ... 6.592 s) yet every kill reads "chain 1", so the 3rd and later kills hold 0.050 s, not >= 0.08 s; jitter <= 0.191 is not evidenced |
| 2 Frames | 0 | FAIL | hit-before / hit-next; S:58 k1off "changed next frame 97.6 %" | the before / after pair shows the white hit flash, which the OFF run has too; no OFF frame shows the debris difference |
| 3 Nothing lost | 0 | FAIL | S:58 k1off "ladder NO, cap ok, debris NO, spaced ok, damage ok"; a354 not run; S:235 verbs37 27/27 | RED off holds for debris only (the damage check passes on OFF too); the a354 jitter line is not evidenced |
| 4 Look | 0 | FAIL | my pixel read of hit-next (box x 930-1000, y 525-590): median hue 315, S 0.27, V 0.83 (hit-before same box: hue 5, S 1.00); S:53 "marks laid 3, holes 3, drawn 1" vs S:58 "marks laid 0, holes 0, drawn 0" | a pale magenta-violet patch sits on the hit husk, probably kill55's mark; its hue is not printed; magenta is RIVAL only and pale violet-white is the boundary: not evidenced as rule-following. Debris is red-orange (fine). |
| 5 Not basic | 1 | PASS | debris frame; S:53 10 chunks per kill at +1.5 s | chunks that stay on the floor are a first-minute change |

**GATE P1-kill: FAIL (1, 2, 3, 4)**
Smallest change: make kill55 count the chain (kills 0.51 s apart all read "chain 1") so kills 3+ hold >= 0.08 s inside the 0.12 s cap;
print the hit mark's hue (or make it a danger-family colour); run a354; add one OFF frame of the same kill.

---

## 9. P2-clear (clear55: the room-clear beat)

Frames:
- `Logs/feel/n56-r1/c55on/a455/shot-N29-C55-n52-iceline-g2-hold.png`: iceline g2, blue walls with green bands, "DOOR OPEN" up top, "ROOM
  CLEAR · UNDER PAR / 16.3 S · PAR 30 S" under the crosshair, PULSE GAUNTLET in hand.
- `.../shot-N29-C55-n52-iceline-g2-card.png`: to the eye the same frame; a still cannot show the hold.

| line | score | result | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 1 | PASS | S:67 g2 "hold served 0.500 s on the loop's clock (band 0.4-0.6), 30 held frames ..., sim time moved during the hold False \| card 'ROOM CLEAR · UNDER PAR \| 16.3 S · PAR 30 S' (text has the time and par True) ... par source: n52-iceline.json campaign.rooms[g2].par"; g3 "0.500 s", "PAR 25 S"; S:74 c55off "holds 0", "hold served 0.000 s" | hold in band, card with time and par, par source printed, RED off; on one level (iceline) |
| 2 Frames | 0 | FAIL | the two frames; S:74 c55off "card 'ROOM CLEAR · UNDER PAR \| 16.3 S · PAR 30 S' ..., alpha 1.00" | "a frame per level": iceline only (1 of the 5 campaign levels in `NIGHT56-LOG.md:12-18`); and the OFF run shows the same card, so these frames do not show what clear55 adds (the hold) |
| 3 Nothing lost | 1 | PASS | S:74 off = no hold; S:235 verbs37 27/27 with clear55 on | older behaviour on OFF; verbs hold |
| 4 Look | 1 | PASS | S:67 "min font 24 px"; "red-orange 0.0 %, mid-frame luminance 0.203" (hold) vs "0.204" (card) | text >= 16 px; nothing whole-frame |
| 5 Not basic | 1 | PASS | S:67 the first clear at 16.3 s | a 0.5 s hold at a room's last kill lands inside the first minute |

**GATE P2-clear: FAIL (2)**
Smallest change: run a455 on and off on foundry, kiln, span and spire, with a frame per level, plus a held-frame index (or a frame pair
across the hold) that shows the freeze against the OFF run.

---

## 10. P0-t125 (reel code: a152's THUNDERCLAP taps)

| line | score | result | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 0 | FAIL | S:181 s1 (a555) "THUNDERCLAP fire events of 6 taps: before 0, after 6 (want >= 4)", the fix named ("AutopilotInput ... -> ReaderInput (the reel's forcedCombat reader, handed back as a252 does: FeelReel.N52.Skin.cs:132)") | the fix works in a555, but the doneWhen asks for a152 on the build, which was not run on n56-r1 (brief) |
| 2 Frames | - | N/A | - | reel code |
| 3 Nothing lost | 0 | FAIL | brief: a252 not run | "the KESTREL's a252 unchanged" is not evidenced |
| 4 Look | - | N/A | - | reel code |
| 5 Not basic | - | N/A | - | reel code |

**GATE P0-t125: FAIL (1, 3)**
Smallest change: run a152 (>= 4 THUNDERCLAP fire events) and a252 on the next build.

---

## 11. P0-t99 (FeelReel Capture: long reels)

| line | score | result | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 0 | FAIL | S:24, 29, 126, 149, 188, 193, 236 "[reel] done: 1800 frames written, 0 failed"; every done line in `Logs/player/n47-reel-n56-r1-*.log` is <= 1800; the last frame on disk is f1750 in bfon, bfoff, a4on, a4off, d2on and vf | no reel went past 1800 frames (they stop at exactly 1800), and the done line prints no reused-index count: the doneWhen is not reached |
| 2-5 | - | N/A | - | reel code |

**GATE P0-t99: FAIL (1)**
Smallest change: one reel run past 1800 frames (e.g. 2400) with its done line printing "0 failed" and a reused-index count of 0.

---

## Verdicts

- GATE P0-smoke: FAIL (1, 3)
- GATE P0-wickGrade: FAIL (1, 2, 4, 5) (THE CROWN only; THE WICK passes)
- GATE P0-hotWall: FAIL (1, 2, 5)
- GATE P1-wickWindow: FAIL (1, 2, 3, 4, 5)
- GATE P0-step: FAIL (1, 3, 5)
- GATE P0-ssao: DONE-no-defect (AUTO-low preset)
- GATE P0-decor: FAIL (1, 4)
- GATE P1-kill: FAIL (1, 2, 3, 4)
- GATE P2-clear: FAIL (2)
- GATE P0-t125: FAIL (1, 3)
- GATE P0-t99: FAIL (1)
- Blind pairs: pair-1 order1 B order2 A; pair-2 order1 B order2 A.

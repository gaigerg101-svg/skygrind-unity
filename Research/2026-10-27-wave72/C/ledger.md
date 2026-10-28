# Builder C ledger (COLOUR & DECOR), NIGHT72

Worktree .claude/worktrees/n72-C, branch n72-C from t0b b80afc2fa. t0 = 2026-10-27 10:19:20. Written 2026-10-27 about 11:35 (1:15 in).
Every number below names its file; anything not run says "not checked". No Editor, build, Player, reel, suite or SimHarness was run by this builder
(CLAUDE.md, the brief); the compile gate `bash Tools/night/dn-check-wt.sh <scratch>/dn` printed 0 errors on the four projects at each commit.

## Commits (in pick order)
| sha | what |
|---|---|
| e6ae56516 | the values meter `Tools/night/n72-c-values.py` (selftest PASS: 11 planted cases) and its baseline `baseline-n71-forge72.txt` |
| 9e49c8913 | C2 palette72: Palette72.cs, UnityTuning.N72C.cs, hooks in Decor54.cs / Colour56.cs / Yellow56.cs, Palette72Tests, `n72-c-calibrate.py`, calibration.txt |
| 23ce2a818 | C3 decor72: Decor72.cs (+ Decor72Marks), Decor55.cs / Colour56.cs band split, Decor72Tests, `n72-c-marks.cjs`, marks.txt |

Rows: `design.n72.palette72` and `design.n72.decor72` stay OFF in the config (the lead flips). Both are read with `NightRows.ViewOn(cfg, "<row>")` every frame.

## The meter (before any ON frame existed; never loosened after a measurement)
`Tools/night/n72-c-values.py` reads tour65 frames with FIXED masks: floor x 0.30-0.70 y 0.70-0.92, walls x 0-0.22 and 0.80-1 y 0.22-0.60; top hue = the
biggest 60-degree window of the coloured pixels (S >= 0.25, V >= 0.15), pooled per level; cyan first = p99 luma of cyan >= that of the vivid non-role hues;
blank face = the biggest flat region (16 px block means, T 14 / step 8). Two corrections were made BEFORE any ON frame (so they are not a loosening):
the first top-hue cut read single 30-degree bins (spire 42 %, split across a bin edge) and became the 60-degree window (spire 62 %: stricter);
the first blank cut read per-pixel flatness (0 % everywhere: the CRT scanlines put +-15 on every pixel) and became block means, calibrated on the
baseline's iceline g3 (34 %) and foundry a8 (24 %).

Baseline (baseline-n71-forge72.txt, sha b80afc2fa, Logs/feel/n71-forge72/{tp,tf,ti,tk,ts}/tour65 read in the main tree):
| level | floor-wall gap worst | top hue window (pooled) | blank worst |
|---|---|---|---|
| spire (3 poses) | 66.0 PASS | 60-120 deg 62 % RED | 14 % |
| foundry (6) | 8.7 RED (a8: the floor mask is on a teal box) | 30-90 deg 53 % PASS | 24 % (a8) |
| iceline (8) | 1.9 RED (g3: the camera is inside a wall, tour WARN) | 180-240 deg 86 % RED | 34 % (g3) |
| kiln (5) | 60.0 PASS | 330-30 deg 51 % PASS (it is the danger band: the floor is salmon, hue 17) | 17 % |
| span (3) | 12.1 RED (p5: the floor mask is on a ramp) | 210-270 deg 84 % RED | 19 % |
The three gap REDs are poses whose floor mask lies on a wall, a box or a ramp, not on the floor: a colour change cannot move them. They are reported, not
excused: the doneWhen reads "in every tour pose", and these three stay RED by the meter unless the ramp / box top / wall takes a value apart.

## C2 palette72 (commit 9e49c8913)
Job: floors, walls and the accent per level, from the stone's vertex colours (linear albedo: Stone.shader multiplies the light by them) and the accent pieces.
- THE FLOOR on all five levels is one warm-neutral sand (hue 36-42, S 0.25-0.27), at the level's own brightness: spire olive (hue 80) to sand, iceline blue-grey
  (222) to sand, kiln salmon (17: the danger band) to sand, span lavender (266) to sand, foundry tan (hue 50, S 0.36: the parry arc) to sand. A ramp is x1.18, a
  deck x1.10, a pit's floor x0.55 on top of that (Palette72Tests pins the classes).
- THE WALLS carry the level's own hue at their own brightness: foundry oxide green 150, iceline navy 225, kiln plum 272, span indigo 245, spire bottle green 135.
- THE ONE ACCENT: the spire's colour56 accent is a rose (0xe67793, hue 345 = the danger band's edge; the n72-h0 log shows decor54 "accent #e67793 (hue 345 ...)")
  and becomes a bone ivory 0xe8dcc0 (hue 42, S 0.17); colour56's frieze (the level's second colour) takes the accent's hue at its own luminance. The other four
  accents stay (foundry violet, iceline periwinkle, kiln blue, span indigo).
- The multipliers come from `n72-c-calibrate.py` on the baseline medians (calibration.txt: m = (target / current)^2.2): the predicted on-screen floors are
  foundry [138,125,100], iceline [105,95,77], kiln [138,124,103], span [126,115,93], spire [102,94,74] (the spire's blue is capped at x3.98: luma 105 -> 95).
  A HYPOTHESIS until a Player frame shows it (L-094): read it with `python -I Tools/night/n72-c-values.py --pair <on tour65 folder> <off tour65 folder>`.
- Same-run safe (the row is in FrameRows): Decor54.Update calls Palette72.Tick every frame; a flip writes the stored off / on colour array (exact), then the accent
  pieces; the held OFF frame is the plain stone. Yellow56Floors' rule is folded into the same pass (same function, at decor54's pass) and Yellow56Floors stands down
  on a handled mesh: with yellow56 ON the result is the same, written once.
- Evidence the stone's colours are Float32 (so a multiplier above 1 is kept): no "[decor54] ... re-toned" line in any of the 27 n72-h0 Player logs
  (Logs/player/n47-reel-n72-h0-*.log, grep -c "re-toned": total 0, checked 2026-10-27 11:38); decor54's probe compares the float it wrote with the one it reads back, which a UNorm8 mesh would never match.
  Hedge if wrong: `n72-c-calibrate.py --unorm8` lowers the spire floor to luma 72; the other levels are unchanged.
- Proof lines (L-089): `[palette72] <level> ON|off: floor x(...), wall x(...); stone vertices: floor n, slope n, deck n, pit n, wall n (k over 1.0: kept, Float32)`,
  `[palette72] <level> accent scan n: k new pieces (f frieze) of m; accent #e67793 -> #e8dcc0`, `[palette72] <level> flipped ON|off (flip n); accent pieces m (l lost)`.
- doneWhen: (1) gap >= 20 in every pose, (2) no top hue > 60 %, (3) cyan stays first: NOT CHECKED (no ON frame yet). The predicted floor / wall hues are in
  Palette72Tests (two families >= 60 degrees apart on all five levels, no role hue).

## C3 decor72 (commit 23ce2a818)
Job: the two bands that run along EVERY wall box (decor55's string course, colour56's frieze) keep a segment only where it has a job, from the level's own data:
ride (campaign.boxes ride-tagged / noted ride panels), route (within 4 m of a door rect), hazard (within 2 m of a pit or lava rect). Offline counts
(marks.txt, `node Tools/night/n72-c-marks.cjs`; the Player prints the real ones, which for the n65 / n67 rebuilt rooms differ because those files are swapped in
by the level rows at load): spire 582 of 1410 segments keep a band (ride 64, route 240, hazard 278), foundry 618 / 1428, iceline 370 / 1240, kiln n57 405 / 1362, span 671 / 1492.
No collider, level file or sim change (render objects only).
- Two objects per band (jobs / no job), drawn exactly as before while the row reads off; the no-job one is SetActive(!ViewOn) every frame (Decor72.Update).
- Proof line: `[decor72] <level> <band>: segments N, with a job K (ride r, route t, hazard h), no job M: ...`; flip line `[decor72] flipped ON|off`.
- doneWhen: the mark list printed per level (done, offline and in the Player); "the spotter names it on a pair" and "n46flow byte-equal": NOT CHECKED (no pair; the
  lead's `n72-gates.sh --flow` after the pick).

## What the lead runs (exact commands; a build that has 9e49c8913 and 23ce2a818)
Pair, two runs (ON with the switch, OFF plain), the spire and the foundry first (the stripes and the one-tint level):
`bash Tools/night/n72-reels.sh n72-<k> tpOn,n52-spire,tour65,-skygrindRows72,palette72:1+decor72:1 tpOff,n52-spire,tour65 tfOn,n52-foundry,tour65,-skygrindRows72,palette72:1+decor72:1 tfOff,n52-foundry,tour65`
(alone: `palette72:1` and `decor72:1` on their own sides, so the SPOT names each). Same-run: add `,-skygrindSameRun63,n72.palette72` (or `n72.decor72`) to a side.
Read in `Logs/player/n47-reel-<tag>-<side>.log`: the `[rows72] palette72=1` line (the input), `[palette72] ...`, `[decor72] ...` lines (above).
Then `python -I Tools/night/n72-c-values.py --pair Logs/feel/<tag>/tpOn/tour65 Logs/feel/<tag>/tpOff/tour65` (and the other levels' sides): the gap, the top hue window, cyan first, the blank face.
EditMode tests to run through the lead: `Palette72` (6 tests) and `Decor72` (9 cases); Decor54Tests / N63ColoursTests as a regression for the hooks.

## Update 2026-10-27 11:45 (1:25 in): C1, C4, C5, the train's test fixes
Commits after the three above (shas on n72-C; the lead's picks in brackets): 19423753c C1 [96add34e3], 535fd265a palette72 floor-colour fix [c8f8c3b15],
e8bcfe3f5 C5 (not picked yet), d84d45a18 the three test fixes for the train (on branch n72-C-trainfix, picked as cdb4ecc0d; 9a10e4d85 is the same fix on top of C5: skip it).
Train result the lead reported: Palette72 6 / 6 and Decor72 10 / 10 pass; before the fix 3 tests failed for stale expectations in MY tests (the cyan compare was exact, the
rose is hue 344.86 not inside 345-35, and N0b moved n52-foundry's route count 280 -> 272, which `node Tools/night/n72-c-marks.cjs` on the main tree's file also gives).

### C1 colours63 made right (19423753c; a change to a flipped row: the lead re-runs colours63's spire pair on n72-t1 and reverts it if it does not hold)
Read first (Logs/n72/pairs/colours63-alone, spot, Logs/player/n47-reel-n72-h0-r-col-sp.log): the row alone changed only the exit beam (orange to white, SPOT names it); the
left wall's dark-red stripes stayed: the log shows the rose accent routed `#CF6B84 -> #A17983` and `#5D303B -> #48363B` (the world tier kept hue 345 at S 0.25), still red on a dark wall.
Fix: Colours63.Scan finds the level accent (Decor54.ScriptFor), and when its hue lies within 15 degrees of the danger band (Colours63Math.InDangerReach) moves every accent piece
(hue within 8 degrees, S >= 0.6 of it, V >= 0.2) to hue 40 at the world tier's saturation and its own luminance (Colours63Math.WorldAt); only the spire's accent qualifies
(foundry violet 265, iceline periwinkle 229, kiln blue 222, span indigo 252 stay). The audit tool's list (`node Tools/night/n63-c1-audit.cjs`: 1031 literals, 404 hue-bearing,
203 off every role band, 183 of those "world") was re-run; the named offenders read the table as: spire stripes = this fix; the goal column's red-gold = the goal47 route (as built,
PASS alone); lava gold = the lavaGold route (as built); lime / amber kit-lamp edge lines = the world tier (as built; only the spire v6 pose was paired). NOT CHECKED: the boss arenas'
named offenders (the wyrm's lime edge lines, the crown's danger-coloured ring): no boss63 pair for colours63 exists; B2 / B1 own those rooms' colours. doneWhen "red-orange pixels
on non-danger surfaces fall >= 50 % on vs off": no frame yet; read `python -I Tools/night/n72-c-values.py --pair <on> <off>` (danger vivid in walls) on the spire.

### C4 the other colour harvest rows (no code change)
- goal64: PASS alone (verdict-goal64.md): nothing to fix; Route47View.cs untouched (the flip guard refuses a changed pass file).
- decor55: SHORT (no visible change on spire v6, numbers identical). Code read: with colour56 ON (it is, in the repo) Decor55.Accent returns colour56's accent first, TrimLum
  reads colour56's, the string course is built "for colour56" even with decor55 off, and bandKinds equals colour56's kinds: decor55 ON changes nothing while colour56 is on. It is
  redundant, not broken: keep it OFF (retire at a dead-code night). Nothing to make right.
- yellow56: SHORT on the spire v6 frame (a parry cannot show in a still). What it can change: floors in hue 30-75 above S 0.18, the TORCH / KESTREL plates, an accent in 35-80.
  Baseline yellow share (`python -I Tools/night/n72-c-yellow.py`, file `yellow-share-baseline.txt`): foundry 5 of 6 poses over 0.5 % (b1 3.43 %, a3 2.93 %), spire 3 of 3 (v2 5.28 %), iceline /
  kiln / span none. So its real pair is foundry tour65 (the tan floors, hue 50 S 0.36) and a frame with the TORCH in hand, not spire v6: `tfOn,n52-foundry,tour65,-skygrindRows72,yellow56:1`
  against plain. With palette72 ON the floors' sand (S 0.27) is made from the stone before yellow56's cap (535fd265a), so the two rows do not fight.

### C5 blank walls (e8bcfe3f5; row decor72)
Baseline blank face (baseline-n71-forge72.txt): iceline g3 34 % (the camera is inside a wall: a tour WARN, not fixable by decoration), foundry a8 24 % (a wall face 0.3 m from the camera; its side
is lit by the warm key, tan). Two changes, both view only and drawn only while decor72 reads on: (1) a wall face >= 2.5 m tall (the face height MeshKit keeps in uv3) gets a value structure
(foot x0.8, upper half rising to x1.45), composed with the palette in Palette72's stone pass (Compose: every combination of the two rows is exact); decor54's own ramp runs over a fixed
9 m band, so a 3-5 m face barely moved. (2) 1-3 route chevrons per ramp (a wedge: the one way up) pointing up the slope, in the level's accent at 0.55 of the usable cyan's luminance,
0.025 m off the slope; 9-15 wedges per level. Proof lines: `[decor72] <level> faces: n of m wall vertices ...`, `[decor72] <level> chevrons: n on r ramps`.
The doneWhen (the blank-face share falls in the named rooms on vs off) is NOT CHECKED: it needs the lead's pair; the meter is `n72-c-values.py` (blank = the biggest flat region, 16 px block means).
If a8's slab is a ramp or a wall face the camera hugs, the number may not move: then SHORT with the number.

### The leftovers (press63, look45, shift46): read, not built (each needs a Player pair; the code exists)
- press63 (Press63.cs, 220 lines; self-installed; reads the row every frame; builds on the first frame the row is on from the level's campaign boxes with job "hero prop: a drop press"):
  colours are bone / steel (no red-orange, no yellow: per the file's header; the hexes in UnityTuning.N63GS7.cs were not checked here), unlit, no collider. Pair: n57-kiln tour65 pose kb3
  (THE PRESS HALL): `tkOn,n57-kiln,tour65,-skygrindRows72,press63:1` against plain (not a FrameRow: two runs); read `[press63] presses on n57-kiln: 4 dressed ...`.
- look45 (Look45.cs, 35 lines): subtracts look43's two generic exit pylons in a room that has its own landmark prop (job landmark / hero). It removes a thing (no-job decoration, GG's ask) but the
  pylons are also the exit's marker: it needs a pair on a landmark room and the exit63 row's door read before it goes ON. Not built further.
- shift46 (Shift46View.cs): lights a fixture hot red-orange (danger) while the last wave is out and steady cyan when cleared, pulsing through the tell: a pulse is a flicker source, GG 2026-10-26
  "flickers": it should not go ON without L's flicker meter (L1) reading its room; the a546 reel (`-skygrindShift46`) is its scenario. Not built further.

## Not checked
No Player frame of any C row; palette72 / decor72 / colours63's new branch: EditMode Palette72 6 / 6 and Decor72 10 / 10 passed on the train (the lead's report) but C1's Colours72Tests and C5's added
tests were not in that run; yellow56's folded floor pass not compared with its own pass on a Player; the rebuilt-room maps (n65 / n67): marks counted offline on their StreamingAssets
files only (the level rows swap their rooms in at load); the boss arenas: palette72 / decor72 act only on a level with a colour script and campaign data.

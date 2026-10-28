# NIGHT63 builder D: B3c, B3's one fix round (Logs/n63/verdict-B3.md: GATE FAIL)

Branch n63-B3c, cut from port/stage3 ab8ade3e. These notes are in their own file so they never clash with D.md at a pick.

## Commits, in order
Cherry-pick `904d3b21^..<this notes sha>`.

1. `904d3b21` **The tests, committed RED.** Outside Unity, 4 of 11 failed.
2. `ab4d391f` **Test fixes, still RED.** Three of tonight's own hook assertions changed, with the reason in the commit:
   - B3b's wyrm wrappers become LitRim / PartRim.
   - The punch head joins them.
   - The crown's tell count is exactly 5 sites. My RED commit counted 6, but the NIGHT38 gem cubes never take the tell colour.
3. `d3e37b78` **The fix.**
4. `24950308` **The per-boss role measure** (Tools/night/n63-b3-roles.py) and its before.
5. This notes commit.

**Checks:**
- N63BossColoursTests 11 of 11 outside Unity, and all 50 tests in my scratch runner.
- dn-check 0 errors.
- Gates all GREEN 02:43:34.
- `n63-b3-roles.py --selftest`: B3 ROLES PLANTED GREEN, 11 cases.

## The GATE's four points, and what I found

### (1) THE CROWN's tell ring stays amber: 35,617 px at 35-45 deg on the tell frame
- **Cause:** the red (dodge-only) tell is already danger in the code: Tell36RedHex #FF3A14, hue 9. But every site multiplies it by an HDR
  gain: up to x2.8 on the gems, x2 on the halo ring, up to x4.2 on the ring's edges during a tell.
  - The screen clips each channel; the project's Neutral tonemap works per channel too.
  - So green 0.23 x 2.8 = 0.64 shows next to red 1.0: hue ~37, amber.
  - B3's earlier route kept the colour's brightest channel, but the views apply their gain afterwards. That is the C1 overflow again, one
    step later.
- **Fix:** a danger route is now CLIP-SAFE (`BossColours63Math.ClipSafe`).
  - Above 1, the brightest channel keeps its value, so the glow and bloom stay.
  - Every other channel stops at the role colour's own share, so the clipped colour on screen is the danger hue.
  - Within the screen's range nothing changes.
- **Where:** the crown's sites route after their gain (`BossColours63.Hot`):
  - the red tell on the blades, the NIGHT37 gems, Crown38's gem crosses, their halos and the halo ring (`crown.tell`, new, danger);
  - the ring's edges after their tell gain, phase 3's red-orange included (`crown.edge`);
  - the clapper (`crown.clap`).
- **Untouched:** the YELLOW (parryable) tell is never routed, and G's bossTell63 is not touched.
- **Pinned:** at gains 0.6-3.5x the colour on screen is in the danger band. The red is pinned as a planted case: a plain route at x2.8 shows
  ~37 deg.

### (2) THE KILN WYRM's weak point never cyan (0 cyan px among the changed px)
- **The read:** b7's stagger pair, cropped on and off. The pale square the camera sees is **BossMarks32's open-window shell**: 1.45 x the
  head's box, additive cyan-white (0.72, 1, 1), pulsing at 1.1-1.6. The lit head is a smaller tilted square inside it.
  - Both sit under **THE WYRM's grade**: design.n54.grade54, ON, wyrm on 1.
  - Its colour filter cuts blue to 0.78, and its split toning gives the highlights sulphur.
  - So both read lime: ON (190, 255, 157), OFF (182, 223, 133).
  - grade54 has its own answer for the player's effects: `Rim(c)`, the boss's rim pre-divided by its colour filter so it reads cyan after the
    grade. The wyrm's rim is (0.08, 0.86, 1) x 2.2.
- **Fix:**
  - `BossGrade54.RimAt(c)`: Rim at the colour's own brightness. A player-cyan input in a graded fight becomes rimHdr / rimGain x its
    brightest channel; anything else passes.
  - The wyrm's weak parts go through it with the row on: the open head (`LitRim`), the punch window's head and the exposed core
    (`PartRim`).
  - BossMarks32's open shell over the head is the weak cyan through the rim (`ShellRim`, `wyrm.mark`, new, weak).
  - Row off: exactly today's colours.
- **Expected, not checked:** on the stagger frame the head reads cyan.
  - My estimate: the shell (0.15, 1.5, 2.8) plus the head (0.2, 2.1, 3.9), through the filter's 0.57 on blue, the per-channel Neutral and the
    sulphur highlights. That gives about (0.35, 0.9, 0.8), hue ~175: the player band (160-205).
  - This is arithmetic, not a frame. The measure's (d) judges it on b10.

### (3) The proof line printed only a part's first routed call
- `[bossColours63] <part>: first routed ...` now says that it is the first.
- New: with `-skygrindSameRun63 n63.bossColours63`, one line per shot:
  ```
  [bossColours63] <shot> (frame N): <part> <role> #from -> #to (k calls; the last drawn) | ...
  ```
  It is taken from the held ON frame's own calls: per part, the last call that frame, since the later view's call is the one drawn.
- THE CROWN's gems come down two paths in one frame: Crown38 (#FF5A1F) and Crown40 (bone #F2EAD8). The line names the call count and the
  last.

### (4) A per-boss role measure on the boss's own pixels
`python -I Tools/night/n63-b3-roles.py --boss <on dir> <off dir> <label>`.

- **The mask:** the same-run pair's changed pixels (the parts B3 recoloured).
- **The classes:** each pixel classed ON and OFF with the table's bands.
- **The rules:**
  - (a) changed px off every role B3 routes to;
  - (b) danger -> cyan, a cyan effect;
  - (c) a weak point in two colours;
  - (d) per boss: no shot shows the weak point cyan.
  - EMPTY: B3 changed nothing.
  - The REGION: the change's box grown 15 % a side; it shows what B3 left there.
- **The bloom fringe:** the cyan's bloom fringe counts with the cyan. That is a green or blue pixel within 3 px of a cyan pixel; the table's
  player band already runs to 160 for bloom. No other role gets a fringe.
- **Planted cases:** an attack effect off the danger band, a weak point in two colours, a cyan effect, a good route, no change, the region,
  the HUD's corners, the fringe and a green part beyond it, (d), and EMPTY. All PASS.

**The BEFORE, measured here** (B3 as on n63-b5 / b7; the ON halves in `<side>/`, before the S4 layout):

| Boss (pair) | Verdict | Why |
|---|---|---|
| THE WICK (b5 b3f) | PASS | idle: cyan 8,268 + fringe 927 px, off-role 130; flinch the same; windup danger 2,755; stagger EMPTY |
| THE ICEBREAKER (b5 b3i) | EMPTY | 0 px changed on its 4 shots: its stuck fist is cyan on and off |
| THE LOOM (b5 b3s) | FAIL | stagger: cyan 5,832, blue 422 px: two colours (c), marginal (its floor 313) |
| THE KILN WYRM (b7 b3k) | FAIL | (d): no shot shows cyan (0 px); its seams / spit to danger PASS per shot |
| THE CROWN (b7 b3p) | FAIL | flinch: 263 parry + 194 amber px left in the change (a); the tell EMPTY at 77 px with the region amber 17,890 (flinch) / 22,995 (windup) |

**Expected after b10** (not checked):
- the crown's tell changes, its ring in danger;
- its flinch's parry / amber drop;
- the wyrm shows cyan, so (d) passes;
- the wick, the Icebreaker and the loom unchanged.

**The loom: FIXED** (the lead's addition, 4:14). Commits `8513efcd` (RED) and `50cd704c`.
- **Not bloom.** I read it from b5's frame (`b3s/boss63/shot-N30-B63-loom37-stagger.png` against `b3s-off`):
  - only 4 of the 422 "blue" px lie within 3 px of a cyan px;
  - only 52 of them form a 1-2 px line.
  The marked crop shows them on the knee discs' bright edges and one thin thread, ON blue where OFF they were violet / blue.
- **The real finding:** the knees B3 routes to the weak cyan read **hue 224 (blue), S 0.70, V 0.34** over all 33,607 changed px. They sit
  under the measure's vivid floor (V 0.50), so they are classed "dull"; the 422 are just their brightest px.
- **Cause:** THE LOOM's grade (grade54 loom37 on 1, filter 0.86 / 0.86 / 1) cuts red and green, so the routed cyan turns blue.
- **Fix:** every loom weak site (the knees, the nails twice, the seams) goes through the fight's grade rim (`PartRim` -> `RimAt`), as the
  wyrm's. A dim knee stays dim (RimAt keeps the routed colour's brightness).
- **Expected on b11, not checked:** the knees read in the player band and the loom's verdict PASSes.
- **The measure has a blind spot:** dull, dim weak points are not judged by colour (V < 0.5). A dim knee's hue is read here by hand
  (median of the changed dull px), not by a verdict rule.

## The exact Player steps (the lead's; I ran no Editor, build, Player or reel)
1. **Build** b10 with bossColours63 on in its copy: `node Tools/night/n46-build-rows.cjs <tag> on n63.bossColours63`.
2. **Reels:**
   ```
   bash Tools/night/n63-reels.sh <tag> b3k,n57-kiln,boss63,-skygrindSameRun63,n63.bossColours63 b3p,n52-spire,boss63,-skygrindSameRun63,n63.bossColours63 \
     b3f,n52-foundry,boss63,-skygrindSameRun63,n63.bossColours63 b3i,n52-iceline,boss63,-skygrindSameRun63,n63.bossColours63 \
     b3s,n52-span,boss63,-skygrindSameRun63,n63.bossColours63
   ```
3. **Measure:** for each side,
   `python -I Tools/night/n63-b3-roles.py --boss Logs/feel/<tag>/<side>-on/boss63 Logs/feel/<tag>/<side>-off/boss63 <boss>`.
   The ON half is in `<side>-on/` from b8 on (review S4).
4. **Proof lines:** `grep "\[bossColours63\] shot-" Logs/player/n47-reel-<tag>-<side>.log` gives what each shot's frame used. On b3p's tell
   shot, `crown.tell danger #FF3A14 x... -> #FF4A1C x...`; on b3k's stagger, `wyrm.open` and `wyrm.mark` with a blue-leaning cyan.
5. **SPOT pair:** THE CROWN's tell (`shot-N30-B63-crown37-tell.png`, `<side>-on` vs `<side>-off`) should change at the ring; the wyrm's
   stagger is the weak-point pose.

## Not checked
- Any frame of the fix: the crown's ring on screen, the wyrm's head cyan.
- Whether THE WYRM's split toning still pulls the rim's cyan out of the band on the head (the arithmetic above says no).
- THE CROWN's teal sparkle bursts on its idle / windup / tell frames (the GATE's open question: the boss's or the TORCH's).
- The wyrm frames' yellow column at the left (a parry tell or a hazard).
- The readability the GATE raised: the crown's gems are the same cyan as its pass ring, so "hit this" and "go through this" read as one
  blob. That is GG's call (D-8).
- The Editor run of N63BossColoursTests.

## n63-b11: the two FAILs read from the frames (B3 SHORT; for the next night)
**The lead's run:** Logs/n63/b3-roles-b11.txt, the role meter on the five same-run pairs.
- PASS: THE WICK, THE LOOM (stagger 48,628 px: the Loom fix holds).
- EMPTY: THE ICEBREAKER.
- FAIL: THE CROWN and THE KILN WYRM.

The reading below is mine, from the frames `Logs/feel/n63-b11/b3p-on|off` and `b3k-on|off`, by a scratch script that uses the meter's own
classes (`n63-b3-roles.py`'s `classes` and `near`).

### THE KILN WYRM, stagger (187,052 px changed): one colour, a teal on the band's edge, not two
- **The changed px ON** all sit at one hue, around 158 deg (hue p10 / p50 / p90):

  | Class | px | Hue | Mean RGB / S | Luma p50 |
  |---|---|---|---|---|
  | "player" | 36,491 | 160 / 161 / 163 | (6, 229, 161) | 0.70 |
  | "fringe" | 24,737 | 154 / 157 / 160 | | |
  | "green" | 27,240 | 152 / 157 / 159 | (25, 242, 156), S 0.95 | 0.76 |

  The meter's 160-deg band edge splits that one colour into three classes. The "green" half is the brighter half (luma 0.76 vs 0.70), and
  the two halves share the head's box (green x 781-1120 y 433-683; cyan x 808-1120 y 376-690).
- **The cause:**
  - The proof line shows `wyrm.open weak #FFD23A x2.2 -> #0854FF x6.8` and `wyrm.mark weak #B8FFFF x1.2 -> #0854FF x3.6`. That is the grade
    rim at the routed colour's brightness.
  - The input is a deep blue (hue ~220) because RimAt pre-divides the rim by the grade's colour filter: in linear about (0.82, 0.84, 0.32),
    so blue is boosted about x3.
  - That undoes the filter. It does not undo the grade's split-toning: SULPHUR highlights (0.54, 0.76, 0.3), SoftLight weighted by luma.
    On a pixel this bright (luma 0.7-0.8) the toning pulls the hue from ~185 down to ~158, and the brighter the pixel, the greener.
- **Next approach (not built):**
  - Aim the input bluer by the measured offset (~25-30 deg): the rim's hue at ~210-215 so it lands at ~185.
  - Or keep the head's luma under the split's weight, e.g. the head at x1 instead of x2.2 and the shell's glow lower.
  - Or, in grade54, a cyan exception for the split toning (a grade-side change, GG's NIGHT54 design).
  - Whichever is chosen, judge it on the same pair with the meter.
- **Not checked:**
  - Whether the player's own rim effects (Fx49's rim ring, the slug sheath) land at the same ~158 in this fight. The grade gives them the
    same rim, so it is likely, but no frame was read.
  - The exact split-toning weight at this luma (an inference from the hue / luma relation above).

### THE CROWN, tell (126,327 px) and windup (103,878): the danger holds; the (c) is the meter
- **The danger works:**
  - The tell's changed px ON are 114,460 danger (hue p50 13) against OFF 79,675 danger + 34,712 amber.
  - Amber is 34,712 -> 223: the B3c fix holds.
  - The proof line: `crown.edge danger #FFC65A x3.0 -> #FF1809 x3.0`, `crown.tell danger ... (128 calls)`.
- **The FAIL is (c), "a weak point in 2 colours (rival, rose)":** 477 rival + 832 rose = 1,309 px (1.0 % of the change). From the frame:
  - 1,282 of them lie within 3 px of a danger px (749 within 1 px).
  - The densest window (x 1120-1360, y 800-1040: the right of the frame by the TORCH's barrel) shows them as 1-2 px EDGES of pale pink /
    white diagonal light streaks crossing the now-red background.
  - Their mean RGB is ON (226, 72, 145), OFF (237, 121, 147). OFF they were mostly dull (999) or danger (240): the same streak edges over
    the old orange.
  - Where a white-pink streak blends into the red, the mix has blue above green, which is rose / magenta.
  - No weak point is drawn on the tell or windup frames: the gems take the tell colour, and `crown.gems` is not in either frame line.
- **So:** the (c) rule counted the danger's blended edges as "weak point colours". It treats every non-danger vivid family on a frame as a
  weak point's colour, with a 300-px absolute floor that a 126k-px change dwarfs. A meter fix for the next night: (c) only where a weak part is
  in the frame's line, or the families' floor relative to the whole change (2 %, as (a)). (a) itself was under its floor here: 1,599 vs
  2,527.
- **Not checked:**
  - What the pale pink / white streaks are: the crown's light beams, the TORCH's own beam, or a bloom streak.
  - Whether a viewer reads any pink at 1-2 px.

### What B3 met on b11, as I read it
- **Attack effects in the danger band:** met on THE CROWN (amber 34,712 -> 223 on the tell) and THE KILN WYRM (seams / spit to danger on
  every shot).
- **Weak points one colour:**
  - THE WICK: met (PASS).
  - THE LOOM: met (PASS).
  - THE CROWN: met where shown (idle: player 168,173, fringe 2,566).
  - THE KILN WYRM: one colour on screen, but a teal at hue ~158, on and below the cyan band's 160 edge, not the player's 186.
  - THE ICEBREAKER: unchanged (its stuck fist is cyan on and off).
- **No boss effect in cyan or magenta:** (b) danger -> cyan is 0-98 px on every shot. The rose / magenta px are the streak edges above.

# NIGHT17-REPORT: the overhaul night, for GG (one page)

**Final (the close, 2026-10-06 05:2x; tag `u3-n17`).** Build 6 passed every gate:
- the smoke's re-run: all steps passed;
- golden: 0 mismatches;
- the fingerprint: EQUAL;
- the replay: EQUAL;
- the suite: 969 / 969.

Tonight went after what you called cheap, wordy,
stale and not crunchy. You asked for massive changes rather than nudges, and that is what this night aimed at. The
proof is the reel:

- the same scenarios on the build before tonight: `Logs/shots/n17-before/`;
- each round's build: `Logs/shots/n17-r1/` .. `n17-r4/`;
- the final build: `n17-final/`.

Playing the A/B in FEEL-PLAYTEST.md takes 10 minutes.

## 1. Before / after (open these pairs)

| What | Before (n16) | After (final) |
| --- | --- | --- |
| A punch kill | `n17-before/n16-strip-punch.png` (a static white blob for 80 ms) | `n17-final/n17-strip-punchkill.png` |
| The HUD in a fight | `n17-before/n16-strip-freeze.png` (tiny text, key hints, a grey "D DULL") | `n17-final/n17-hud-fight.png` |
| PAUSED | `n17-before/n16-paused.png` (7 buttons and a 30-row key card) | `n17-final/n17-paused.png` |
| SETTINGS | `n17-before/n16-access-tab.png` (7 tabs, 50 rows) | `n17-final/n17-settings-game.png` |
| RESULTS | `n17-before/n16-results.png` (6 buttons, 4 stat lines, the raw score line) | `n17-final/n17-results.png` |
| TITLE | `n17-before/n15-title.png` | `n17-final/n17-title.png` |

## 2. The rubric, round by round (10 rows, 1-5 each, out of 50)

| Round (build) | The lead | A fresh reviewer (captures + checklist only) |
| --- | --- | --- |
| 0 (n16, before) | 17 | none |
| 1 (build 2) | 30 | 25 |
| 2 (build 3) | 35 | 23 |
| 3 (build 4) | 37 | 27 |
| 4 (build 5) | 34 | 27 |
| Two fix rounds checked in the Editor (on DRYDOCK) | not scored: checked by eye (`n17-r5e/`, `n17-r6e/`), each one's misses sent back | none |
| 5: the final reel (build 6) | 35 | none (the 4-reviewer cap is spent) |

**How to read it:**
- The lead scores higher than the reviewers, every round.
- The reviewers saw the same strongest gains: menu simplicity went from a wall to 4 items and identity became "a coherent
  system". They saw the same weakest rows: kill crunch, movement feel and motion.
- Round 4's lead score dropped because it was stricter: two rule breaks the lead had missed in round 3 (a 10 px speed
  readout, overprinted pops).
- The reviewers' total went 25, 23, 27, 27. The night improved the game a lot, but "crunchy" isn't solved: a revolver
  kill's first frame still reads like a hit's to a fresh eye.

## 3. What changed

- **Identity** (IDENTITY.md):
  - Archivo Extra Condensed Black Italic for display, Archivo for body text, and Tourney for the big numerals.
  - One 12° slant, opaque INK plates with cut corners, a cyan ladder for everything the player owns, and gold only for
    points.
  - Every token lives in one file, `UnityTuning.Theme.cs`.
- **Stamina** (design.stamina, each rule behind its own flag):
  - One bar replaces the 3 pips. VIGOR adds a second bar.
  - Sprint at 10.5 m/s, below the slide's 11.6.
  - A partial boost scales its duration by fill. Refill is 0.5 bar/s (matched to the web's pip rhythm).
  - Kills, slams and parries refill the bar.
  - Golden vectors exact; the fingerprint was re-recorded once (`964899b`).
- **Crunch:**
  - Kill grammar: the body is held white through the freeze, then thrown as its own blocks (rendered with
    `RenderMesh`; the instanced path drew nothing in a Player). A floor ring follows.
  - A crush frame on big kills, rotational trauma shake, a muzzle star per gun.
  - Each gun has its own death signature.
  - The first-kill hitch is gone (warm-up at TITLE).
- **Hands:**
  - Snappier swaps, recoil springs per gun, a sprint carry and a slide pose.
  - The style aura on the held gun.
  - A kill flourish after a multikill or a rank-up.
- **HUD:**
  - Three tiers, with HP and points as big numerals on plates.
  - A style meter on its own plate with a slam, named callouts, a decay bar and kill streaks.
  - Round start: a hero numeral. Round end: the clear on a full-width band.
  - Pickups: a banner, then a chip with a ring timer.
  - A stamina bar under the crosshair.
- **Menus:**
  - TITLE: 4 items and a 3-row setup card, under a wordmark slab.
  - PAUSED: 4 items.
  - SETTINGS: at most 5 tabs and 8 rows per tab, with sliders and a footer per row.
  - RESULTS: one card holding the round, the points, the grade plate and THE ROUTE.
  - MODS: cards with a glyph and the number as the hero.
  - Key hints are drawn as chips.
- **Beyond the ask:** the world's neon answers the rank; a rank-up streak; pickups pulled in with a flash; the spawn ring;
  the RESULTS card; the per-gun deaths; the flourish; the aura (NIGHT17-LOG.md, U3-30.10's table).

## 4. Questions for GG

1. **The world still carries hues outside the roles:** the cyan trims on buildings, the lime power-up beam, the gold
   accents on the THUNDERCLAP and STITCHER, and the COMET's lavender. Every reviewer flagged them, and they're the web's
   looks. Recolour them to the roles?
2. **The kill freeze is now longer than the web's:** `design.impact.freeze.kill` is 0.05 s against the web's 0.04 (round
   3). Round 5 may scale it with the kill count; see the log. Keep it?
3. **Stamina numbers:** refill 0.5 bar/s (NIGHT17.md proposed 0.8; M measured that 0.5 keeps the web's boost rhythm).
   The sprint is on the boost key (STAMINA KEY: ONE KEY). Do these feel right in the A/B?
4. **The spawn ring reads near white at 15 m, not DANGER red-orange.** Change it?
5. **The lesson card and tips still print ASCII "[E] BUY GUN"** instead of drawn chips. Convert them next?
6. **The Player smoke pauses when another window takes focus** (`SurvivalGame.cs:279`). This made two of tonight's first
   runs fail while agents worked. Should the smoke hold focus, or should automation runs skip the auto-pause?

# NIGHT18-REPORT: feel in motion, measured (for GG; one page)

The night ran from 08:22 on 2026-10-06 (`date`). Every number below comes from a reel's telemetry or frames
(`Tools/feel-metrics.py`), or from a blind reviewer's pick decoded with its key. Nothing here is the lead's opinion of
its own work. The full record is `NIGHT18-LOG.md`.

## 1. What changed in how we judge

NIGHT17 counted "implemented and unit-tested" as "felt". Tonight the game records itself:
- Each reel scenario (23: kills at 6 / 12 / 20 m, a headshot, a punch kill, a 5-kill blast, a hit, boost, sprint,
  slide, land, strafe, swaps, pickup, rank-up, round end, a 3 s fight, five menus) logs every rendered frame: the camera,
  the victim's screen rect against a world anchor, each effect's screen size, every UI element's scale and alpha. Frames
  are full-resolution JPGs on a fixed 1/60 s clock.
- Metrics M1-M11 come from those numbers; two fresh, blind reviewers each round pick between `FeelAB` off (NIGHT17's
  look) and on (tonight's), the second reviewer with every side swapped.

## 2. The metrics: NIGHT17 (build 1) vs tonight (final build)

From `Logs/feel/b1/FEEL-METRICS.md` (build 1) and the same build's FeelAB-off side, against
`Logs/feel/final/FEEL-METRICS.md` (build 7): **tonight 104 of 125 rows PASS, NIGHT17's look 56.**

| Metric | NIGHT17 | Final (build 7) | Target |
| --- | --- | --- | --- |
| M1 a kill's frame 1 vs a hit's | 1.97 | **3.05** | >= 3.0 |
| M2 the kill's effects at 20 m / the kill marker | n/a (not probed) / NIGHT17's red marker 89 px | **22.9 %** of the height / white, frame 1, **96 px, 367 ms** | >= 8 % / f1-2, >= 48 px, 300-400 ms |
| M3 the blocks thrown (kill12 / horde5) | 0.73 / 1.05 body heights | **1.76 / 1.59** (100 % outside the body) | >= 1.5 |
| M4 the freeze: body / headshot / punch / 5 kills | 7 / 9 / 11 / 18 steps | **7 / 11 / 13 / 18** | 6-8 / 10-13 / 11-14 / >= 18 |
| M5 a hit moves the body | 2.7 px | **12.6 px** in 50 ms, back in 67 ms, white 3 frames | >= 8 px in 50 ms, back in 200 |
| M6 boost kick / sprint / slide | +12.6 deg creeping to +16.8 / +6.4 / +4.4 deg | **+11.8** back in 300 ms / **+5.9** held / **+6.9**, 0.8 m drop, 4 deg roll | +10 +/- 2 / +5-6 / +6-8 |
| M7 strafe roll / landing dip | 1.4 deg / 0.133 m | **2.3 deg / 0.141 m**, back in 183 ms | 1.5-3 / 0.05-0.15 |
| M8 speed lines in the middle | 18 of 18 frames | **0** (and 0 frames strong under 70 % speed) | 0 |
| M9 elements that pop in (HUD / menus) | rankup 16, hud-fight 16, settings 12, paused 3 (build 1, the final rules) | **HUD 0**; menus 0 on four screens, **2 on ui-settings** (TITLE's MAP focus row, before SETTINGS opens) | 0 |
| M10 / M11 | 9 texts at 16 px / the boost's DANGER tint, NIGHT17's red marker | **0 / 0** | 0 / 0 |
| M12 cost (bench p99, 24 husks) | 17.66 / 34.32 ms | **15.62 / 13.42 ms** | <= baseline + 1 ms |

The gates on build 7: the smoke `DONE: all steps passed`; golden `0 failing, 0 mismatches` (22 exact); the fingerprint
`EQUAL`; the smoke tape's replay `EQUAL`; the suite on the frozen code in NIGHT18-LOG.md.

## 3. The blind verdicts (tonight's side picked, of the judgements)

| Item | Round 1 | Round 2 | Round 3 (Editor) | Final | Target |
| --- | --- | --- | --- | --- | --- |
| The kill (kill12, kill20, headshot12, horde5) | 1 / 8 | 8 / 8 | 6 / 8 | **8 / 8** | 6 / 8 |
| The hit (hit12) | 1 / 2 | 2 / 2 | 2 / 2 | **2 / 2** | 2 / 2 |
| Movement (boost, sprint, slide, land, strafe) | 4 / 10 | 7 / 10 | 9 / 10 | **7 / 10** | 7 / 10 |
| The HUD (rankup, roundend, pickup, hud-fight) | 6 / 8 | 4 / 8 | 4 / 8 | **5 / 8** | 6 / 8 |
| Menus (five ui-* scenarios) | 0 / 10 | 9 / 10 | 2 / 10 | **8 / 10** | 7 / 10 |
| The reviewers' 10-row rubric for tonight (of 50) | 32, 31 | 33, 33 | 31, 34 | **34, 35** | (NIGHT17: 25, 23, 27, 27) |

Still "too much" with tonight's side in it (the final pair): horde5's spark confetti after the slabs, kill6's slabs near
the camera, roundend's pops over the banner, RESULTS' grade card over the points.

What the reviewers said, in short: round 1 called tonight's first kills "a blur bloom that hides the body" and chose
NIGHT17's denser speed lines; round 2 picked every kill for "a bold outlined marker and starburst" and "chunks thrown
wider"; round 3 still preferred NIGHT17's big full-alpha slams on HUD banners and its row cascade in SETTINGS, which the
last fixes answered (the close's final pair judged them).

## 4. See it move

- `Logs/feel/final/ab-kill20.gif`, `ab-hit12.gif`, `ab-boost.gif`, `ab-ui-settings.gif`: NIGHT17 on the left, tonight
  on the right, real speed. Flipbooks: `Logs/feel/final/flip-on/`, `flip-off/`.
- In a developer build (`Builds/Stage1/SKYGRIND.exe -skygrindDev`), **F6** flips FEEL N17 / FEEL N18 live (a chip in
  the corner says which); `FEEL-PLAYTEST.md` is the 10-minute walk.

## 5. Questions only GG can answer

1. **The kill's dark disc** (frames 1-2 of a body kill darken the world inside ~340 px at 12 m, x 0.7): keep it, lower
   it, or keep it for headshots and multikills only? Reviewers liked the "halo" in round 2; it is the first thing to
   turn down if it tires you.
2. **The freeze ladder** (body kill 58 ms, headshot 96, punch 108, a 5-kill blast 150, the budget 0.2 s a second): right
   weights?
3. **Speed lines and the middle:** the reviewers kept preferring NIGHT17's long lines that reach the centre on a boost;
   tonight's rule keeps the central 60 % clear (the research's). Keep the rule?
4. **The kill marker:** white hollow wedges with an ink stroke (your KILL MARKER switch said white). Round 1's reviewers
   named NIGHT17's red X as the kill read. Keep white?
5. **FeelAB:** keep the switch after your A / B (it doubles every feel path), or retire NIGHT17's paths?
6. **The machine:** the Player sees a `Wireless Gamepad` and a Xencelabs tablet; the pad's trigger fired shots in an
   early reel. Is a controller meant to be connected? (The reel ignores both now.)

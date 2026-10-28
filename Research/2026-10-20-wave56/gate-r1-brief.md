# NIGHT56 gate r1 (P0-gate55): the evidence for the batched critic (the lead's, 01:01)

Build `Builds/n56-r1` from port/stage3 6b1c00c9 (NIGHT55's branch commits picked + NIGHT56's shared commit; no NIGHT56 builder work).
The build copy's config has these NIGHT55 rows ON: grade55, wick55, step55, ssao55, decor55, kill55, brief55, clear55. Each reel ran
twice: the ON side, and the OFF side with `-skygrindOff55` (NIGHT54's game). All paths are under `C:\Users\gaige\dev\SKYGRIND-unity\`.
Reel lines: `Logs/feel/n56-r1/summary47.txt` (cut at 2400 chars) and the full lines in `Logs/player/n47-reel-n56-r1-<side>.log`
(grep the phase name, e.g. `a155:`). Every side: NRE count 0, exceptions 0; every done line "0 failed".
Rubric and output: `Research/2026-10-20-wave56/critic-gate.md`. Write `Research/2026-10-20-wave56/gate-r1.md`. Images: open ONLY the
ones listed below (20 in all), describing each in one line before reading its numbers.

Each entry's `doneWhen` and `notes` are quoted from `Logs/n55/slate.json` (NIGHT55's slate; read it for the full text).

## 1. P0-smoke (harness: SliceSmoke)
- doneWhen: the cause printed by the smoke itself (a line naming what paused the run or dropped its presses, with focus / device / flow
  state per failing step); on the build with no agent active: DONE 0 failed, or only pad steps failed with the attached gamepad named
  (T-73) and every non-pad step PASS. Notes: a planted focus steal at ~60 s and ~200 s (the log prints focus lost t=..., run continued),
  every non-pad step PASS, the tape EQUAL at its last step; outside -skygrindSmoke the game still pauses on focus loss (a test).
- Evidence: `Logs/n55/smoke-steal-n56-r1.out` (steal times and notepad PIDs), `Logs/player/n55-smoke-steal-n56-r1.log` (the smoke's own
  log: its start line with focus / device state, the `[smoke]` lines). Shipping config (every row off). Note: three builder agents were
  running (dotnet only; none takes window focus) during this run. NIGHT55's A0 on n54-freeze2 failed 19 steps (`Logs/n55/smoke-a0.out`).
- No frames.

## 2. P0-wickGrade (design.n55.grade55: THE WICK's and THE CROWN's palettes, shadows-only grade measured on the walls)
- doneWhen: a155 row on: per boss the WALL pixels' mean hue >= 30 deg from the danger hue at p1, the turn and p2; graded vs raw differs
  (dH >= 25 deg or |dS| >= 0.10); the danger-tell pixels keep S >= 0.6 and are the most saturated class; cyan >= 4.5:1 vs the graded
  frame; RED with -skygrindOff55; frames p1 / turn / p2 per boss at 1920x1080. Notes: the wall mask comes from the render (saved beside each
  frame); the D-16 whole-frame check (mean hue >= 20 deg from danger) also runs and passes.
- Evidence: sides b1on / b1off (THE WICK, n52-foundry), b2on / b2off (THE CROWN, n52-spire): the `a155:` lines; class numbers in
  `Logs/feel/n56-r1/b1on/a155/a155-wick.txt`, `.../b2on/a155/a155-crown37.txt`.
- Frames (4): `Logs/feel/n56-r1/b1on/a155/shot-N55-wick-p1.jpg`, `.../b1on/a155/shot-N55-wick-p1-raw.jpg`,
  `Logs/feel/n56-r1/b2on/a155/shot-N55-crown37-p1.jpg`, `Logs/feel/n56-r1/b2off/a155/shot-N55-crown37-p1.jpg`.

## 3. P0-hotWall (grade55: THE WICK's hot walls read as a state, not wallpaper)
- doneWhen: on frames with no wall burning red-orange pixels <= 10 % of the frame; a burning wall is the most saturated region of its
  frame; RED off. Notes: ON <= min(10 %, half of the off run's share); a burning wall mean S >= other walls mean S + 0.15 on the same mask;
  decorative stripes >= 30 deg from the danger hue; THE WICK's ride walls should carry a usable (cyan family) cue (critic-firsttime #2).
- Evidence: the HOT WALL part of b1on's / b1off's `a155:` line.
- Frames (2): `Logs/feel/n56-r1/b1on/a155/shot-N55-wick-burn.jpg`, `.../b1on/a155/shot-N55-wick-cold.jpg`.

## 4. P1-wickWindow (design.n55.wick55, sim: THE WICK punish-the-window)
- doneWhen: n47human --boss wick --seeds 40 with wick55 on: damage inside windows >= 50 % (median); median fight <= 60 s; deaths <= 10 in
  40; hot-wall hits <= 30 % of hits taken; 40 / 40 killed; every other attack's numbers unchanged (a test); a454 GREEN in the Player; off =
  NIGHT54 bit for bit. Notes: numbers labelled bot; a casual pilot 40 / 40 median <= 90 s; a454 prints >= 1 core punch inside a window
  on a named frame.
- Evidence (bot, the lead's run on HEAD 6b1c00c9, `Tools/night/n56-wick-bot.sh`): `Logs/n56/wick/skilled-off.txt`, `skilled-on.txt`,
  `casual-off.txt`, `casual-on.txt`, summary `Logs/n56/wick-bot.out`. The pinning test is `Assets/Tests/EditMode/Wick55Tests.cs` (not run
  tonight in the Editor: say "not evidenced" if you need its result). Player: sides bfon / bfoff (a155 FIGHT MODE: -skygrindA155Fight /
  -skygrindA155FightOff) and a4on / a4off (a454, NIGHT54's WICK reel; a4off may still be running: read what is there) in summary47.txt.
  Off bit-for-bit: the fingerprint gate (`Logs/n56/fingerprint-base.txt` and NIGHT56-LOG.md's shared-commit gate lines: plain / --off55 /
  --off56 EQUAL).
- No frames.

## 5. P0-step (design.n55.step55: the step-down camera)
- doneWhen: a255 on a run line WITH a step-down (printed: the step's height and that it happened): the camera's largest per-frame vertical
  drop during the step with the row on <= 50 % of the row off's; no added lag on flat ground (jitter <= 0.191, a354's); golden /
  fingerprints EQUAL. Notes: the off value printed first and must exceed 2x the flat-ground noise, else the entry closes as no defect
  measured (DONE-no-defect with the numbers).
- Evidence: sides c1k (on) / c1koff (off), n52-kiln: the STEP part of the `a255:` line.
- No frames.

## 6. P0-ssao (design.n55.ssao55: SSAO's noise without TAA)
- doneWhen: a255 on a still camera in two rooms: the AO's frame-to-frame changed pixels (|dL| > 2 %) with the row on <= 50 % of off's;
  mean luminance within +-0.02 of off's; frames on / off at 1920x1080. Notes: the off value printed first and must exceed 2x static-scene
  noise (else DONE-no-defect); also a fixed slow yaw; the row's frame-time cost measured alone.
- Evidence: the SSAO part of the `a255:` lines of sides c1k / c1koff (kiln rooms kb2c, kb1) and c2f / c2foff (foundry rooms a1, a2).
- No frames.

## 7. P0-decor (design.n55.decor55: the level accents made visible)
- doneWhen: a452 entry frames on foundry, iceline, kiln: the level's accent hue's pixel share up >= 3x vs off (both printed), the accent
  >= 20 deg from parry yellow / danger / cyan / magenta; usable cyan stays the brightest class; RED off. Notes: includes O-08 (the foundry
  brass >= 20 deg off the parry yellow, printed); the accent covers >= 1 % of each entry frame (absolute); a blind pair both orders picks ON
  as "has a colour accent" 2 of 2.
- Evidence: sides d1on / d1off (foundry), d2on / d2off (iceline), d3on / d3off (kiln), scenario a452. The meter: `python -I
  Tools/night/n55-decor-share.py Logs/feel/n56-r1/<side> --frames entry` (run it yourself on each of the six sides; it prints per frame the
  decor55 accent's share, its p95 luminance, its least role gap, and usable cyan's share and p95 luminance).
- Blind pairs (8 images; the key is held by the lead, do not look for it): `Logs/n56/pairs-r1/pair-1-1A.png` + `pair-1-1B.png` (order 1),
  then `pair-1-2A.png` + `pair-1-2B.png` (order 2); the same for `pair-2-*`. Question for each order on its own: "which frame has a colour
  accent (a deliberate decor colour beyond the base materials)?" Answer A or B per order before comparing the orders.

## 8. P1-kill (design.n55.kill55: kills that crunch)
- doneWhen: a355 on foundry: the 3rd and later kills inside 1 s hold >= 0.08 s (inside the cap), the 1st 0.05 s; >= 8 debris pieces per
  campaign kill alive >= 1.5 s (counted); a hit husk's body pixels change >= 5 % on the next frame (before / after printed); a354 jitter <=
  0.191; frames; RED off. Notes: the hold cap stays move54's 0.12 s; a354 camera-held total over 4 kill freezes <= 0.150 s off a chain; a
  blind pair; jitter <= 0.191.
- Evidence: sides k1on / k1off, n52-foundry, the `a355:` lines (full in the Player logs). a354 was not run on n56-r1 (say so if you need it).
- Frames (4): `Logs/feel/n56-r1/k1on/a355/shot-N55-K-chain.jpg`, `.../shot-N55-K-debris.jpg`, `.../shot-N55-K-hit-before.jpg`,
  `.../shot-N55-K-hit-next.jpg`.

## 9. P2-clear (design.n55.clear55: the room-clear beat)
- doneWhen: a452 / a455 at a room's last kill: a 0.4-0.6 s hold and a card with the room's time and its par (par from data, printed with its
  source); a frame per level; RED off.
- Evidence: sides c55on / c55off, n52-iceline, the `a455:` lines (only iceline was reeled at r1: one level).
- Frames (2): `Logs/feel/n56-r1/c55on/a455/shot-N29-C55-n52-iceline-g2-hold.png`, `.../c55on/a455/shot-N29-C55-n52-iceline-g2-card.png`.

## 10. P0-t125 (reel code: a152's THUNDERCLAP taps)
- doneWhen: a152 on the build prints >= 4 THUNDERCLAP fire events from its taps and names the fix; the KESTREL's a252 unchanged.
- Evidence: side s1 (a555, n52-foundry) in summary47.txt. a152 / a252 themselves were not run on n56-r1.
- No frames.

## 11. P0-t99 (FeelReel Capture: long reels)
- doneWhen: a reel past 1800 frames on the build: 0 failed writes, no index reused (printed by the reel's done line).
- Evidence: the `done:` lines in summary47.txt (bfon / bfoff / a4on wrote 1800 frames).
- No frames.

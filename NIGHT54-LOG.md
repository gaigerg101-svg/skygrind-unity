# NIGHT54-LOG: THE LOOK PASS (2026-10-20, go 14:09 MST; plan NIGHT54.md (untracked); calls DECIDE54.md)

## Close table (16:2x; Tools/night/n54-rows.json)
| row | title | state | passes | evidence |
|---|---|---|---|---|
| P0 | baseline + shared commit | DONE | true | Logs/n54/wick-base.txt; shared commit ef971fdc; gates GREEN 14:29 |
| W | THE WICK: FLARE out, relatively easy (wick54, sim) | DONE | true | a454 FLARES 9 -> 0 (n54-r1, r2, freeze); n47human 40 seeds median 95.0 -> 45.5 s, deaths 51 -> 2, taken 189 -> 72, P2 / P3 40/40 (Logs/n54/wick-after.txt) |
| L | flicker fixes + light pass (light54) | DONE | true | a154 n54-r3: THE STACK flicker 55 -> 2, near-white 0.16 -> 0.02 %; foundry + kiln GREEN; iceline RED on the proof's input only (the gauntlet fires no shots); the white-stone bug fixed in THE STACK + 7 boss views |
| G | boss palettes, turns, guns (grade54) | SHORT | false | a254 n54-r3 GREEN ICEBREAKER / WYRM / LOOM (cyan 54-163 deg off each graded frame); THE WICK and THE CROWN failed the danger-hue check on n54-freeze2 (1-12 deg): their grades ship OFF (D-16) |
| M | the camera's smoothness (move54) | DONE | true | a354 GREEN on / RED off, foundry + iceline: held 0.250 -> 0.150 s over 4 kill freezes, jitter 0.246 -> 0.191 / 0.189; the step-down pop not measured |
| D | decor colour direction (decor54; added at 14:50, D-11) | DONE | - | [decor54] freeze: per level cyan decor 2-3 -> 0, floor trims 12-17 -> 0, wall-top rims 127-128 -> 0, lenses 39-41 moved; usable cyan the brightest; the off switch moves 0; frames: subtle (D-17) |
| Z | freeze and close | SHORT | false | Builds/n54-freeze2 (90b62818's config; its code = 87725556 + cfb5930f); verbs37 27 / 21 / 31 / 31 / 31 (= NIGHT53), NRE 0; golden 0 mismatches, fingerprints plain / --off53 / --off54 EQUAL; the smoke NOT clean (run 1 220 s: 1 failed, grind22's pad menu step with a real gamepad attached; run 2 paused by a focus change: 19 failed); the full batch suite 2277 / 2277 passed at 6462b106 (Logs/n54-close-tests.xml, 16:22-16:40); tonight's classes in the Editor too ( Wick54 4/4, N54Grade 4/4, N54Move 4/4, Decor54 5/5, N54Shared 2/2, Boss37 7/7, Lesson25 10/10) |

## Timeline
- 14:09 go (`date`: Tue Oct 20 14:09:17). GG's request in NIGHT54.md (replaces the drafted THE MIRRORS: D-2). Drive and Linear searched
  for SKYGRIND: nothing there; the repo's HANDOFF / CORE / FEATURES / THREADS read.
- 14:10 three read-only scouts (boss 1, flicker, choppiness). The Editor found in Play mode (D-10).
- 14:20 the baseline: `n47human --boss wick --seeds 40` (Logs/n54/wick-base.txt): killed 40/40, median 95.0 s, p90 105.4, min 71.0,
  damage taken median 189, deaths 51 (median 1), windows median 7.
- 14:24 the shared commit ef971fdc (design.n54: wick54 sim row; light54 / grade54 / move54 view rows, all off; FeelReel.N54.cs a154-a454;
  N54SharedTests; n54-gates / flip / freeze). dn-check 0 errors. Gates (14:29): config copies identical; golden 39 sets, 22/22 exact,
  0 mismatches; fingerprints plain / --off53 / --off54 EQUAL.
- 14:30 builders W, L started; G and M failed to create their worktrees (the disk at 256 MB free for a moment: D-9), started again
  14:33 / 14:35 one at a time (20 GB free).
- 14:38 the before frames: a147 (THE WICK's fight) on Builds/e53-freeze2: GREEN, 426 frames, NRE 0. Looked at (every 37th frame,
  scratchpad wick-before.jpg): P1 / P2 the shaft overexposed (flat cream walls, the boss lost in them; f0185 a white bloom wash), the fight's
  first frame a red noise border (f0037), P3 teal with a heavy vignette. Rooms (foundry / iceline / kiln frames from n53-freeze and
  e53-freeze2): the same cyan trims and dashes on pure decor in every level, flat walls. Sent to L (exposure) and G (palette).
- 14:50 builder D started (decor54: D-11).
- 14:54 C: at 7.0 GB free (the builders' dotnet outputs; other sessions). The lead deleted its own NIGHT53 round-build frame dumps
  (Logs/feel/n53-r1..r3 *.jpg, untracked, uncited; summaries and reel.json kept): 14 GB free. Builders told to clean their bin/obj.
- 14:59 round 1 picked (all clean): W f99e4689 074b931b, L 71be54e4 2a472275 449dcc26, G a75b6a09, M 088a1c96 f25a2f03. Gates 15:00:
  config identical; golden 0 mismatches (22/22 exact); fingerprints plain / --off53 / --off54 EQUAL; the Editor compile false / 0 errors.
- 15:01 Builds/n54-r1 (rows wick54 / light54 / grade54 / move54 forced on in its copy); the proofs on and -skygrindOff54 on n52-foundry.
- 15:09 round-1 proofs (Logs/feel/n54-r1/summary47.txt):
  - a454 W: GREEN on (FLARES 0; attacks told curtain 6, ember 17, geyser 11) / RED off (FLARES 9, burns 9, hits 9). The Player pilot
    fires no shots (punches only), so the reel reaches P2 and stops at its 120 s cap: the fight's length is the harness's number.
  - a154 L: RED on "INPUT SHORT" (a 10-shot minimum; the KESTREL fires 8 per side). THE STACK: flicker events 21 -> 10, mean luminance
    0.668 -> 0.645 (the lights were not the cause). Sent back to L.
  - a254 G: NOT WRITTEN (G's proof was not in r1).
  - a354 M: GREEN on / RED off: hold 0.150 s vs 0.250 s over 4 kill freezes (0.038 vs 0.063 s per freeze), jitter 0.191 vs 0.246;
    the step check "not checked" (no step on the line).
- 15:10-15:13 round 2 picked: W 6b363670 (the easy numbers), L 3288a81e fa189db8 0b66f039, G 4dbb4a01 f4478b79 25b0e8e5 9185a81d,
  M e24467dd 95718a73 1a96cfbd 6df447bf, D 4e016e75 (D's two commits; its config row conflicted with M's adjacent numbers: inserted by
  Tools/night/n54-pick-decor.cjs, asserting nothing else changed). Gates 15:13 GREEN (golden 0 mismatches; fingerprints x3 EQUAL; compile).
- W's harness table (W's report, e8ce760c): with wick54 on, killed 40/40, median 45.5 s, p90 53.7, taken 72, deaths 2, flares 0, P2 / P3
  40/40, every fight ended on the charge's crack-open punch; off = the baseline bit for bit.
- L's root cause (06017ad5): THE STACK draws with Stone.shader, which colours by VERTEX colour and never reads _BaseColor; Wick37View built
  white vertices and set a _BaseColor the shader ignores: the shaft, panels, ledges, cap, tunnel and THE WICK's plates all drew white (the
  cream walls of a147 and f0185's white plates). With light54 on: neutral greys by vertex colour (config), the plates on URP Lit at 0.2.
  Eleven other views copy the stone material and set a _BaseColor (Boss36View, Crown32View, Brawl40View, Loom37View, Boss30View, ...): L
  checks them next.
- 15:17 Builds/n54-r2 (rows wick54 / light54 / grade54 / move54 / decor54 forced on in its copy); 19 proof reels.
- 15:18-15:32 round-2 proofs (Logs/feel/n54-r2/summary47.txt): a454 GREEN on (FLARES 0) / RED off (9); a154 RED (r2 lacked L's input
  fix; THE STACK flicker 20 -> 15); a254 RED: THE WICK not detected (graded live False) and the grade nearly invisible on THE LOOM / THE
  CROWN (dH 3-4 deg): two G bugs (the Volume stack never updated; the boss read through the HUD's threat); a354 GREEN on / RED off on
  foundry and iceline (held 0.250 -> 0.150 s; jitter 0.246 -> 0.191 / 0.189); [decor54] cyan decor 2-3 -> 0 per level, but the frames
  showed almost no visible change (the wall dashes, lit lenses and wall-top rims still cyan): sent back to D. Disk: the r2 pruner died and
  reels hit the 10 GB floor (g2i / g2k SKIPPED); the pruner rewritten to one find -delete per side (16 GB).
- 15:23-15:33 picked: L 063658c9 3cc34b02 7ada4b9b 4ccb6103 (the white-stone fix in THE STACK + 7 boss views; the input fix), D
  dd7449b1 dd63831e cc65b03e, G 2998d8ee (the Volume stack update + per-boss state reads), D 6590953d (lenses, wall-top rims, the off switch).
- 15:35 Builds/n54-r3; a154: foundry GREEN (THE STACK flicker 55 -> 2 events, wide near-white 0.16 -> 0.02 %, mean luminance 0.237 ->
  0.119 with grade54 also on), kiln GREEN, iceline RED on input only (the gauntlet: 0 shots), the off control RED. a254: GREEN wick /
  brawl40 / wyrm / loom37, RED crown37 (dH 19); the frames: THE WICK's arena a saturated RED (H 1, S 0.80) = the danger colour, THE
  CROWN's turn red: sent back to G (87725556).
- 15:45 the lead's re-measure (Logs/n54/wick-after.txt): n47human wick 40 seeds with wick54: 40/40, median 45.5 s, p90 53.7, taken 72,
  deaths 2, flares 0, P2 / P3 40/40, picks charge 43 / curtain 65 / ember 127 / geyser 77 / ring 10 (W's numbers reproduced).
- 15:46 Editor tests: Wick54Tests 4/4, N54GradeTests 4/4, Decor54Tests 5/5, N54SharedTests 2/2, Boss37Tests 7/7; N54MoveTests 3/4
  (ConfigViewsOffAndOn: expected 0.05, was 0.085; the Player shows the view working: sent to M).
- 15:47 the freeze flip 01f3657b (wick54 / light54 / grade54 / move54 / decor54 ON); gates GREEN 15:48. 15:48 Builds/n54-freeze (its config
  = the repo's). a254 THE WICK: still "TOO CLOSE TO DANGER" (p1 H 26 S 0.77, turn H 13): G's last round (per-boss off switch asked).
- 15:49-15:58 on n54-freeze: decor54 round 2 (cyan lenses 39-41 moved, wall-top rims 127-128 -> 0, the off run moves 0); a454 GREEN
  (FLARES 0); verbs37 PASS 27/27 foundry, 21/21 iceline, 31/31 kiln, 31/31 span, 31/31 spire (RED rows none; NRE 0).
- 15:52 M's test fix 17b99903 (N54MoveTests clears the off switch a leaked earlier test left set); G's last palette round cfb5930f.
- 16:00 Builds/n54-freeze2 (config = repo). a254: THE WICK p1 H 11 / turn 24 / p2 314 (danger distance 1 / 12 / 58), THE CROWN 12 / 2 / 1:
  TOO CLOSE TO DANGER: both ungraded by numbers.<boss>.on = 0 (Tools/night/n54-grade-off.cjs, 90b62818; the build's config copied, cmp equal).
- 16:03 the smoke on n54-freeze2: 220 s, 1 failed (grind22: "the d-pad reached the MODE row True; A on it 5x picked 'survival'"; a
  Wireless Gamepad attached), NRE 0. Re-run 16:08: 509 s, 19 failed with "Paused" seen (a focus change: memory player-smoke-focus-pause).
  Not run again (the cap). N54MoveTests 4/4, N54GradeTests 4/4, Lesson25Tests 10/10 in the Editor at 16:17.
- 14:55 the Editor compile (Play mode stopped: D-10): compilationFailed false, 0 console errors; N54SharedTests 2/2.

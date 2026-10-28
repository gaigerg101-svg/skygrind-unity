# NIGHT54 REPORT (for GG; THE LOOK PASS, 2026-10-20 14:09 - 16:5x MST)

You asked (~14:05) for the visuals across the game, the lighting and flicker bugs fixed, smoother movement without changing the
mechanics, THE WICK made relatively easy without its half-arena attack, each boss its own palette with JoJo-style colour turns, the guns'
boss-fight colours by colour theory, and the UI left alone. Plan 15 minutes, build 2.5 h, hard cap 2.75 h (DECIDE54 D-1).

**Play build: `Builds/n54-freeze2/SKYGRIND.exe`** (commit 90b62818). All five rows ship ON: wick54, light54, grade54, move54, decor54.
`-skygrindOff54` gives NIGHT53's game. The comparison side is `Builds/e53-freeze2`.

**Play first:** THE WICK (level 1's boss) and any level 2-4 boss (THE ICEBREAKER, THE WYRM, THE LOOM) to see the palettes and their
turns; then run a few rooms and kill a pack to judge the camera.

## The one-line answer per ask
| you asked | what was done | measured |
|---|---|---|
| THE WICK: remove the half-arena attack | FLARE (the fire on half the shaft wall) leaves the kit in every phase, the cap too (wick54) | Player: FLARE 9 times in 120 s with the row off, 0 with it on |
| THE WICK: relatively easy, other attacks the same | HP 600 -> 330, damage outside a window 30 % -> 60 %, one attack per burst (was 2), longer rests (3 / 3.5 / 1.5 s), P2 at 55 % (was 45 %), the finish on 1 core punch (was 2); every other attack's numbers pinned unchanged by a test | the bot with its human model, 40 fights: median 95.0 s -> 45.5 s, deaths 51 -> 2, damage taken 189 -> 72, every phase still reached |
| the first boss's lighting | the cause: the shaft's shader colours by vertex colour and never read the colour it was given, so the walls, ledges and the boss's plates all drew WHITE (the cream wash you saw); now neutral greys, the boss darker than the walls; the shaft's lights x0.3-0.45, bloom x0.6 inside it (light54) | the shaft's wide frame: near-white 0.16 -> 0.02 %, mean brightness 0.237 -> 0.119 (measured on n54-r3, where THE WICK's grade was also on; the shaft as shipped, ungraded, not re-measured) |
| the flicker | one light was shared by the muzzle flash and every bullet impact (a strobe while firing); the room lights re-assigned every 0.25 s through a dark dip; the landmark light snapped 4 times a second; boss tells pulsed at 7-14 Hz; the boss entrance flickered; z-fighting at 5-12 mm (light54) | THE WICK's shaft at a still camera, 8 shots, 180 frames: flicker events (a frame-to-frame brightness jump over 3 %) 55 -> 2; changed pixels 4.9 -> 3.5 %; foundry and kiln rooms GREEN |
| the same white bug elsewhere | seven more boss views drew white the same way (THE ICEBREAKER, THE WYRM's crucible, THE LOOM, the Stag, Boss36, the Lantern ring, Crown32): their intended colours now show | frames in the close table's a254 |
| boss palettes + JoJo turns | each boss its own grade (a second Volume: colour filter, a two-tone split, saturation / contrast / darker exposure only), a 0.55 s colour turn at the fight start, each phase change and each stagger window (grade54). THE ICEBREAKER steel / ultramarine, THE WYRM umber with a sulphur-green turn, THE LOOM deep indigo, RIVAL magenta / black. **THE WICK and THE CROWN ship ungraded**: their arenas are full of orange stripes and red rings, and every grade tried put the whole frame within 1-14 deg of the danger red, where THE WICK's red tells would vanish (D-16) | a254 GREEN on THE ICEBREAKER / THE WYRM / THE LOOM: graded vs raw 5-64 deg of hue and +0.17-0.42 saturation; cyan 54-163 deg from each graded frame, luminance ratio 5.9-7.3:1 |
| guns by colour theory | in boss fights the player's hit ring, sparks, slug sheath and tracers keep cyan's hue, tuned against each palette (complementary rims, a brighter core) | cyan vs each graded frame printed in a254 |
| decor colour direction | each level its own decor accent (foundry brass, iceline cobalt, kiln copper-green, span emerald, spire citrine, finale ivory), cyan kept for what you use, walls with a value ramp (decor54) | per level: cyan decor pieces 2-3 -> 0, floor trims 12-17 -> 0, wall-top rims ~128 -> 0, 39-41 lit lenses moved; usable cyan stays the brightest. **In the frames it is subtle**: the cyan ceiling / wall-top lines are gone, but the waist-high cyan dashes stay (they run along ride walls) and the brass edges nearly vanish into the walls |
| smoother movement (view only) | kill freezes hold the camera less (gun kill 0.085 -> 0.05 s; parries and punches unchanged), slow-mo eases in, the grounded step pop smoothed, enemy turning interpolated, the pad's aim-assist pull spread across the step, AUTO resolution steps halved, VSync on by default for a new profile only (move54) | Player, 4 kill freezes on a run: camera held 0.250 s -> 0.150 s, jitter 0.246 -> 0.191 (foundry); 0.250 -> 0.150, 0.189 (iceline) |
| the UI | not touched (D-4) | - |

## Close table (NIGHT54-LOG.md has the evidence per row)
| row | state |
|---|---|
| P0 baseline + shared commit | DONE |
| W THE WICK (wick54) | DONE: every target met (FLARE 0; median 45.5 s; 2 deaths in 40 fights) |
| L flicker + light (light54) | DONE: THE STACK's flicker 55 -> 2 events at a still camera; the white-stone bug fixed in 8 boss views |
| G palettes + turns + guns (grade54) | SHORT: 4 of 6 bosses graded; THE WICK and THE CROWN off |
| M camera (move54) | DONE: camera held 40 % less per kill freeze, jitter -22 % |
| D decor (decor54) | DONE by its numbers; subtle on screen |
| Z freeze | SHORT: verbs37 x5 nothing lost, golden / fingerprints EQUAL, the full test suite 2277 / 2277 passed; the smoke not clean |

## Known issues and not done (honest)
- **No one has played this.** Every number is from the bot, the proofs and frames I looked at. THE WICK's "easy" is the bot with a human
  model (aim, reaction delay); a person may find it easier or harder.
- **THE WICK has no boss palette yet**, the boss you asked about most. Its lighting is fixed (the white walls are grey now, the boss darker
  than them, the strobes gone), but the JoJo turn is off for it. A next try: grade only the shadows (violet / indigo) and leave the warm
  highlights alone, and measure the danger hue on the walls only, not on the whole frame (the red tells count against it now).
- **The smoke is not clean.** Run 1 failed one pad-menu step (a real gamepad was attached); run 2 was paused by a window taking focus.
  Nothing tonight touches menus, so I believe both are the machine, not the build, but that is not proven.
- The full test suite passed after the tag: 2277 / 2277 (Logs/n54-close-tests.xml).
- **The step-down camera smoothing is unmeasured** (the proof's run line had no step on it). THE WYRM's tells now differ by colour only,
  not by pulse rate (the 14 Hz strobes are gone: D-14). The finish takes 1 core punch, not 2 (D-13): one number if you read it as part
  of the attack.
- SSAO's noise (no TAA) and one owner for ambient / fog were not done.
- **The disk:** C: ran out once (256 MB free) while four builders' worktrees were created; it hovered at 10-17 GB all session. 179 old
  agent worktrees under `.claude/worktrees` hold about 34 GB. Removing them is your call (a bulk delete: I did not).

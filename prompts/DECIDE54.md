# DECIDE54: the calls the lead made without GG (NIGHT54, THE LOOK PASS; 2026-10-20 from 14:09 MST)

Each call: what was decided, the alternative rejected, why. GG can reverse any of them; the row it names turns it off.

- **D-1 The clock.** GG: "a clear plan for the first fifteen minutes ... that ramps into a focused 2.5-hour build window with a hard cap
  at 2.75 hours". Read as: go 14:09; the plan 14:09-14:24; building to 16:39 (2.5 h from go); the close inside the cap 16:54 (2.75 h from
  go). Rejected: 2.5 h of building after the plan (cap 17:09), because "tight" and the cap read as the whole session's.
- **D-2 This request replaces the drafted NIGHT54 (THE MIRRORS).** The draft said GG's words replace it when he gives them. The rooms'
  identity work waits; NIGHT54 is now THE LOOK PASS.
- **D-3 "The movement needs to feel cleaner ... don't change the core mechanics".** CLAUDE.md rule 3 keeps movement exact (golden 0
  mismatches, fingerprints EQUAL). So the movement work is the VIEW: interpolation, camera effects, frame pacing, and the view bugs. No
  change to Mover.cs or the sim's movement numbers.
- **D-5 "The one where it covers half the arena before becoming vulnerable" is FLARE** (`wickFlare`: "a ring of fire wipes the half of
  the shaft wall you're riding", design.n37.wick37.attacks.flare, flareHalfDeg 90 = 180 deg; wick43 brings it back on the cap). In the code
  no attack opens the window by itself (the stagger meter does), so the read is by shape: FLARE is the only attack that covers half the
  arena. Rejected: the PLATE RING (a chest-high sweep of the whole cap, then 1.5 s bare): it covers the cap but is jumped, and GG named
  "half". If GG meant the ring, the ring's weight goes to 0 the same way (one number).
- **D-6 "Relatively easy" as numbers.** Baseline (n47human --boss wick, 40 seeds, as shipped): median 95.0 s, p90 105.4, deaths 51 in 40
  fights (median 1), damage taken median 189. Targets with wick54 on: flares 0; median <= 60 s; deaths <= 10 in 40; damage taken median
  <= 100; 40/40 killed; every phase still reached. "Keeping all the attacks exactly the same" = each other attack's id, kind, timings,
  damage, radius, legs and weight unchanged (pinned by a test); the ease comes from the encounter (pool, chip, rests, windows, gates).
- **D-7 The builders and their files** (L-049): W the Wick's sim (wick54); L the flicker fixes and the light pass (light54: LightPools,
  LightKit29, Wick37View, Juice38, PostFx); G the boss palettes, their turns and the guns' boss colours (grade54: new BossGrade54*.cs,
  one tint hook per site); M the camera (move54: SurvivalGame*, CameraFeel, the views' interpolation). Each row is off until its Player
  proof is RED off / GREEN on.
- **D-8 The palette rule holds during the JoJo turns.** CLAUDE.md rule 8 (cyan the player's, red-orange danger, magenta RIVAL) is the
  colour semantics; a boss palette shifts the arena's tone (grade, fog, ambient, its lights), never those three roles. The player's
  effects stay cyan and are tuned for contrast against each palette (colour theory: value contrast plus a complementary accent), and the
  turns are one smooth beat each, never a strobe (at most 3 flashes a second: photosensitivity).
- **D-9 Worktrees and the disk.** Four builder worktrees at once filled C: for a moment (256 MB free; two failed to create, then the
  space came back to 20 GB); G and M were started again one at a time. 179 older agent worktrees in .claude/worktrees hold about 34 GB
  (~191 MB each). They are earlier nights' temporary output, but deleting them is a bulk deletion: not done; listed for GG in the report.
- **D-10 The Editor was found in Play mode** (Survival, 14:10). The builds need it stopped; the first build stops it (as NIGHT47 did),
  nothing else in it touched.
- **D-11 A fifth builder, D (decor54, a new view row), for "decorative elements ... lack strong color direction".** The lead's read of
  the frames (foundry, iceline, kiln, THE WICK): every level spends the same cyan trims and dashes on pure decor over flat tan / teal /
  navy walls, so no level has its own colour and cyan stops meaning "usable" (rule 8). D gives each level a colour script (a wall tone
  with value structure, one decor accent that is not cyan / red-orange / magenta / violet-white) and keeps cyan on what the player uses.
  Rejected: folding it into L's light pass (L's flicker work comes first and would crowd it out).
- **D-12 Reel frames pruned as they finish** (Tools/night/n54-prune.sh: every 50th frame kept, named frames like shot-*.png kept).
  A proof reel writes ~840 MB; C: was at 12-14 GB with five builders' dotnet outputs, and the reel runner refuses under 10 GB. Only
  tonight's n54-* reel output is touched. Rejected: deleting the 179 old agent worktrees (D-9: a bulk deletion, GG's call).
- **D-13 THE WICK's finish takes 1 core punch, not 2** (wick54's `punches`, W's round C). The CRACK-OPEN CHARGE itself is unchanged
  (its speed, damage, lane, crack-open time); the number is how many times it must be punished before the finisher. Without it no round
  got under 60 s (round E 62.4 s). Kept; the row's `punches` key is the one line to remove if GG reads it as part of the attack. The
  hot wall's touch (5 hp a burn, about half the damage taken) is left as it was.
- **D-14 Boss tells keep their colour and timing, not their pulse rate.** L turned every full-range pulse above ~3 Hz into one smooth
  brighten or a 1.2 Hz swell (the 7 Hz "now" pulse on every boss tell; THE WYRM's 14 / 3.5 / 6 / 9 Hz). The Wyrm's tell kinds now differ by
  hue only, not by rate. Rejected: keeping the strobes (GG: "flickering bugs that happen constantly"; photosensitivity).
- **D-15 Rows ON at the freeze by their Player proofs, re-checked on the freeze build** (01f3657b). wick54 (a454 GREEN / RED, the
  harness re-measured by the lead), light54 (a154 GREEN foundry + kiln; iceline's RED is the proof's input: the gauntlet fires no shots),
  move54 (a354 GREEN / RED on two levels), grade54 and decor54 (their last rounds checked on the freeze build itself, since a build is ~1 min).
- **D-16 A palette may not sit on the danger hue.** The first strong WICK grade turned the arena saturated red (H 1, S 0.80): the red
  tells would have vanished. Added to a254 by G: the graded frame's mean hue >= 20 deg from the danger hue at p1, the turn and p2. A
  boss that fails it ships with its own grade off (a per-boss number), the row stays ON for the rest. Rejected: shipping a red arena
  "for JoJo" (rule 8 and readability first).
- **D-17 decor54 ships ON though it is subtle.** The frames show the cyan ceiling and wall-top lines gone (128 rims per level moved to the
  accent) and the walls' value ramp (upper / base 0.90 -> 1.64); the waist-high cyan dashes stay (they run along ride walls: usable). The
  brass foundry accent is close to the parry yellow's hue (3 deg) at half its saturation: flagged for GG's eye.
- **D-4 The UI is not touched** (GG: "the UI looks good as it is, so don't touch that"): no Ui*.cs, Hud*.cs or UiStyle file changes tonight.

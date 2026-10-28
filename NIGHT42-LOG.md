# NIGHT42-LOG (the feel night; NIGHT42.md; GG's fifteen asks G1-G15 in Research/2026-10-14-wave42/GG-REQUEST.md)

t0 = 19:00:01 local (the night's first `date`). Hard cap 01:00.

## G0 (19:00-20:50)
- 19:00 `date`; read NIGHT42.md, GG-REQUEST.md, DECIDE42.md, DECIDE41.md. **No GG answers to DECIDE42 / DECIDE41 since the forge: every
  default holds** (D-1 THE GANTRY rebuilt forgiving in n42-foundry; D-2 the swing 1.8 s, damage x6 kept; D-3 orbs only; D-4 a wall jump
  only when meant; D-5 two new attacks, bot band 40-70 s; D-6 rank letters; D-7 at most two lines in the first 60 s; D-8 a project skill).
- 19:00 state: C: 19 GB free (45 GB at 20:23); RAM 1.6 of 63 GB free (OneDrive.Sync.Service 14.9 GB, not ours); the Editor was in Play
  mode (left from an earlier session).
- 19:05 CODE-FACTS.md: the forge's read-only sweep returned late (~19:05); the lead wrote Research/2026-10-14-wave42/CODE-FACTS.md from it.
- ~19:15-20:23 **lost**: the session's connection dropped (the API unavailable); work resumed at 20:23. The clock below is compressed.
- **The Editor crashed at 20:01** on its own: "Unrecoverable D3D12 device error" (d3d12 device removed 887a0005, in Play mode; crash
  report under %TEMP%/Unity/Editor/Crashes). Not killed by the lead. Reopened at ~20:40 (`unity open .`); autotick on.
- 20:47 **the shared commit af1b680a**: N42Blocks (9 bits), design.n42 every row off (6 view rows), --off42 / -skygrindOff42, tape format
  25, maps.list n42-foundry + placeholder, Curriculum25.N42Levels, N42BaseId (asked first by N41BaseId), FeelReel.N42 dispatch, harness
  partials, the test chain. Gates (n42-gates.sh): config copies identical; golden 39 sets / 22 replays exact, 0 mismatches; fingerprint
  EQUAL plain and --off42; dn-check 0 errors (four assemblies); the Editor compile after the reopen: compilationFailed false, 0 console
  errors (20:47:56).
- 20:50 eight builders launched in worktrees from af1b680a (M1, L1, W1, F1, U1, B1, K1, X1; integrate.sh's map updated). The brief:
  NIGHT42-BRIEF.md (untracked). Revised clock: integrations ~22:00 and ~23:00; round 4 ~23:20-24:00; freeze ~00:00; the test hour
  00:00-00:45; the close 00:45-01:00.
- 20:55 docs/VERBS.md: the NIGHT42 rows marked before any row lands (9 / 11, 10 / 13, 25 / 27, 28, 29 / 30, 40, arena elements).
- 20:55 the before reels on n41-r7 (n42-reels.sh): b-v1 verbs37 n41-foundry, b-lv1 level36 n41-foundry, b-v3 verbs37 n41-kiln, b-w41
  weapons41, b-b2c brawl40casual n40-iceline, b-rank rankup, b-vit vitals.
- 21:08 **the befores (n41-r7, 0 NRE / 0 exceptions in all seven)**, measured: verbs37 n41-foundry row 11 jump-off 16.52 m/s after 11.70
  on the wall (G2's fling; row 10 wall jump PASS, row 13 slide bounce 8.20 -> 11.56); level36 n41-foundry 11/11 rooms; weapons41 GREEN
  (taps 3/3, charged 1/1, volley 1, slug 1/1); brawl40casual GREEN killed in 53.9 s (tells: haymaker 1.00, slam 0.90, block 0.75, rush
  0.65 s); rankup "NO RANK-UP in 10 s" (the scenario did not reach a rank change on n41-foundry: no style before); vitals 225 frames.
- 21:12 **looked at** (the lead): level36 b2-b (THE GANTRY): a long dark corridor, the wall faces purple-grey, "HOLD SPACE: CHAIN THE
  WALLS." in the centre slot; the no-ride band cannot be told from the wall; the pit not visible from the lip. a1-b (INTAKE, the first
  room): a room title + the arc under the crosshair; health a 64 px number + 10 slash ticks (bottom-left). weapons41 kestrel-tap2 +150 ms:
  a husk hit: the body red-orange with a glow, no flinch / flash read on the still; the style meter a small "D" with a hairline (top
  right); a "GAMEPAD CONNECTED" banner (a real pad is plugged in: L-046 pad steps). The raw f*.jpg frames deleted (named frames kept).
- 21:22-22:03 the suite on the shared commit (n42-suite.sh, the split batches): Core 1095 / 1095 (21:22-21:45); Game N4 9 / 9, N37 3 / 3,
  Lesson 5 / 6 (Lesson25TextTests: NIGHT40's 7-word level-4 prompt '{grapple} HOOK THE KNEE. {grapple} PULL AWAY.', known); Game M9 / MapC
  / MapP / Me / Mo / N1 / N2 / N3 209 / 209, every batch under 600 s (MapC the longest, 5:18): NIGHT41's M / N timeouts are gone.
- 21:35 X1 picked (52460cbf, 1e71d47d, 234f6e47): Tools/skills/night-context (brief / close), CONTEXT.md. The lead ran `brief --card W1`
  on tonight's files: it flags w40 / w41 never harvested and 12 aged threads NIGHT42 never names. Installed at the close.

## Round 1 (integration 22:05)
- Picked: W1 846d5f9c (G3 gauntlet42: the swing 1.8 s), c030e259 (G4 parry42: orb reach 1.5x, 0.1 s mercy, longer buffer); U1 5167ec85
  (meter42), 170cb30e (ui42 reel), 78b2b5ae (style42); B1 7a53567a / 9cb7071c / d094b3b8 (brawl42: SWEEP + LEAP, phase 2). M1, L1, F1,
  K1: nothing yet. Gates 22:14 GREEN (config identical; golden 0 / 22 exact; fingerprint EQUAL plain and --off42; the Editor compile 0
  errors, the new types in Skygrind.Game.dll at 22:10).
- Build n42-r1 (22:19). Rows ON in the build's copy only (n39-build-rows.cjs): gauntlet42, parry42, meter42, style42, brawl42.
- Reels on n42-r1 (0 NRE / 0 exceptions in all four): ui42 **RED** (the reel never charged: every frame REST, charge 0.00; swing frames 0;
  the counter: arc40 false FULL 897 frames of 1084, meter42 0); brawl42 GREEN (tells sweep 0.90 s x3, leap 1.10 s x2; 3 sweeps jumped, 2
  leaps escaped; 31.0 s to 50 %); brawl40casual with brawl42 ON GREEN, killed 81.6 s (before 53.9 s; tells now 5 kinds, phase 2 seen);
  verbs37 n40-iceline 20 / 20 with gauntlet42 + parry42 ON (row 28 PASS; row 40 SKIP: gauntlet only at the level's start).
- **Looked at** (the lead): B42-sweep-tell: a red-orange eye slit + red-orange reach arcs at the feet; the cocked-low arm does not read on
  the still. B42-leap-lock: the landing circle reads amber / pale orange over the warm ice floor, thin: not the danger colour. Iteration
  sent to B1 (a saturated red-orange circle with a filled disc brightening to the lock; the arm low + a shin-height arc). style42 seen in
  the same frame: "x2", "D", "DULL", "+20 QUICKDRAW" at the top right: it reads. ui42's input fix sent to U1.

## Round 2 (integration 22:47-22:53)
- Picked: K1 (grinders42, cover42, ARENA-KIT.md: 13 real images with URLs; the swing bar cut), F1 (impact42 tiers, KillFeel42, hits42;
  ebf49622 broke the Editor compile: CS1002 / CS1026 at ImpactFrames.cs:201, a comment inside rumble.Play( ; F1's d695b2fe fixed it; F1's
  "dn-check 0" in round 1 was from a hand build, not the script), W1 (charge42 + the reticle gate, reticle42, weapons42), U1 (health42,
  the ui42 input fix through forcedCombat), B1 (phase-2 double sweep, the colour iteration, the gauntlet-hit reaction on F1's event), L1
  (G1 THE GANTRY in n42-foundry), M1 (G2 wallfling42). Gates 22:56 GREEN (golden 0 / 22 exact; fingerprint EQUAL both; Editor 0 errors).
- Build n42-r2 (22:57); rows ON in its copy: gauntlet42, parry42, charge42, reticle42, meter42, style42, health42, brawl42, impact42,
  kills42, arena42, wallfling42, foundry42.
- Reels (0 NRE / 0 exceptions in all eight): **ui42 GREEN**: 754 frames, meter42 0 mismatches vs arc40 44 (worst 0.173), false FULL 0 vs
  301; fires 3, max charge 1.00; swing frames 74; style ups 2, combo 6 at the second; health chunk 3 ticks at +0.03 / +0.3, 0 at +1.0;
  heal glow 3 ticks. **weapons42 GREEN** on vs -skygrindOff42: G10 held shots through the cooldown 7 -> 1, charge42Rearm 0 -> 3; the
  reticle spin at +0 / +0.1 / +0.2 s: 0.00 / 0.16 / 0.23 -> 0.04 / 0.50 / 0.79; the parry moment missed in BOTH (the reel's timing) and
  the second charged shot not reached. **hits42 RED** (its rule "on froze longer on every moment"): KESTREL 50 = 50 ms; THUNDERCLAP 58 ->
  85; gauntlet husk 83 -> 143 + ring 0.39-0.99 m + kick 0.98 deg; gauntlet tank 123 -> 183 + ring + kick 1.47 deg. **brawl42 GREEN** (5
  tells, all answered, 0 health lost). **arena42 GREEN** (grinder: punched in from 1.6 m, ground 1; cover: 5 hits, broke, shatter 2 bodies).
  **level36 n42-foundry PASS** 11 / 11 rooms. **verbs37 n42-foundry 26 / 26** with wallfling42 + foundry42 ON (row 11 jump-off 16.52 m/s:
  a long ride, unchanged by the 0.3 s short-ride cap; rows 10 / 13 PASS).
- **Looked at** (the lead): H42-off-ghusk-050 vs H42-on-ghusk-050: off: a hit spark on the husk, nothing else; on: a double cyan ring and
  the style C COOL: much stronger, but the ring covers ~70 % of the screen at punch range (iteration sent to F1: thinner, ~0.5 alpha,
  capped radius). U42-s-up2: "x6", B "BOLD", "+40 CLOSE CALL": reads; the award line sticks out past the right edge and the letter shows a
  ghost (iteration sent to U1). U42-h-chunk: "73" with three red-orange ticks: reads. W1: the reel's parry moment iterated.
- 23:08 M1 report: wallfling42 n42move wall: a second press standing at a wall 8.0 m/s back -> 0 (double jump); a press before a landing:
  wall jump -> ground jump; strafing into a wall 7.9 -> 4.9; a short-ride jump-off 8.6 -> 5.0; wall jumps in 51 cases 40 -> 21. Gates with
  wallfling42 ON: foundry 2/2, span 3/3, spire 4/4 unchanged; **kiln 4/4 -> 1/4** (the checker owns the gauntlet on the whole level: the
  game gives it at kb3): M1 adds per-room ownership to the checker. boost42 SHORT: lift at 45 deg 7.07 -> 12.00 m/s; climbs reaching 8 m
  3.8 -> 39.1 %, 10 m 0 -> 19.9 %, 12 m 0 -> 9.1 %; with both rows the kiln 0/4 (G1's 3-shot climb hits the roof).

## Round 3 (integration 23:12; build n42-r3 23:16; rows ON in its copy: round 2's + teach42)
- Picked: F1 eaf62434 (the rings thinner / half bright / capped 9 deg), U1 86342b9c (the award line inside the edge, the ghost off), W1
  b05db3f9 (the reel's parry press at 3 m, the held rearm). Gates 23:14 GREEN. Then L1 (teach42, first42, K1's grinder + cover in b1) and
  M1 (94307921 per-room ownership in the gates checker; boost42 lift 11) picked 23:09 / 23:28.
- Reels (0 NRE / 0 exceptions in all eight): ui42 GREEN (as r2); weapons42 ON: held shots 1, rearm 1, **second charged shot True**; OFF:
  7, 0, False; the parry: no orb inside the reach at the press in both (pressed at -1.00 m: the reel found no orb); the reticle at +0.1 /
  +0.2 s on 0.19 / 0.25 vs off 0.16 / 0.23 (kind 1). hits42 **GREEN** (KESTREL: >= off by design: its 50 ms is the headshot freeze).
  brawl42 GREEN. first42 ON: 2 lines in the first 60 s ("MOVE. LOOK. SPACE JUMP." at 0.0 s, "HOLD C: SLIDE UNDER" at 28.3 s), the
  GANTRY lip / mid / g2 in 5.7 s, 2 rides, 0 falls; OFF (-skygrindOff42): also 2 lines; mid / g2 not reached in 40 s (RED: frames 5/7).
  wall42 GREEN: wall press back speed 8.00 / 8.00 / 6.86 -> 0.00 / 0.00 / 0.00 m/s at +0 / 150 / 300 ms, WallJump 2 -> 0; boost (row
  forced ON in the reel) vy 11.68 -> 16.61 at +0, rise 1.62 -> 6.88 m.
- M1 23:25: with the checker's per-room ownership the kiln with wallfling42 ON 1/4 -> 4/4 (G1 +27 %, G2a +34 %, G2b +32 %, G3 +27 %);
  every level GREEN with wallfling42; with boost42 at lift 11 the kiln 3/4 (G1 +10 %, under the checker's 10 % floor: the kb1b roof).
- **Looked at** (the lead): H42-on-ghusk-050 after the iteration: a cyan burst on the husk, the rings no longer cover the view. L42-gantry
  -mid: a narrower lane, the next walls in view, cyan step edges (vs n41-r7's dark corridor). W42-wall-on-150: facing the wall, no fling.
  W42-held-rearm: CHARGING under the KESTREL, the arc starting. B42-leap-lock after the iteration: a saturated red-orange filled disc.
  A42-grinder-1 (n42-r2): the punch's yellow parry flash fills the screen; the grinder not visible. The GANTRY frame also shows a "+0 WALL
  JUMP" award line (style42 draws a 0-point award: a nit for U1's next pass).

## Freeze decisions (23:28-23:33)
- ON (a523f935): wallfling42, foundry42, gauntlet42, charge42, meter42, style42, health42, impact42, kills42, brawl42.
- OFF with the reason: boost42 (the kiln G1 +10 % < the checker's floor at lift 11: the kb1b roof; DECIDE42 D-9), parry42 (built and
  measured: the swing inside 2.5 m 11/16 -> 16/16, the punch window 217 -> 558 ms; **never on a frame**: the reel never had an orb in reach),
  reticle42 (the tap turn 0.34 -> 0.27 s by W1's numbers, the reel's spin at +0.2 s 0.23 -> 0.25: not shown faster), teach42 (2 lines in
  the first 60 s with and without it on the reel's walk: no measured difference; the skip of "MOVE. LOOK." did not take), arena42 (the
  harness and the reel GREEN, placed in n42-foundry b1; **never seen on a frame**: the frame shows the parry flash).
- Gates 23:31 GREEN with the ten rows ON (golden 0 / 22 exact; fingerprint EQUAL plain and --off42: N42OnPath stays false). Build n42-r4
  23:32 (the freeze candidate).

## Round 4 in the Player (n42-r4, the ten rows ON in the repo config; 0 NRE / 0 exceptions in all eleven reels + the smoke)
- verbs37 (the after): level 1 n42-foundry **26 / 26**, level 2 n40-iceline 20 / 20, level 3 n41-kiln 24 / 25 (row 13 RED: the same line
  as n41-r7's before, 11.60 -> 9.43 m/s on kb2's +along slide: T-61, not NIGHT42's), level 4 n40-span 30 / 30, level 5 n41-spire 30 / 30.
- Bosses: brawl40casual GREEN killed in 48.3 s (5 tell kinds incl. the sweep); loom37 GREEN killed at 38.0 s; crown37 GREEN (4 tells,
  the kill frame); wick37 space GREEN, fight RED (alive: T-59, since NIGHT37). weapons41 GREEN (taps 3/3, charged 1/1, volley, slug);
  level36 n42-foundry PASS 11 / 11.
- **The smoke** (n42-r4, 23:50-23:53): "DONE: all steps passed" (174 s). **Freeze 23:53** at a523f935 (play build Builds/n42-r4).

## Test hour (23:53-00:33)
- The suite in the live Editor, split batches (n42-suite.sh), every batch under 600 s (the longest MapC 3:58): **Game 998 / 999** (the
  known Lesson25TextTests 7-word prompt: T-62) and **Core 1094 / 1095** -> **1095 / 1095** after dd37246a (RunSnapshotTests
  .MoverState_ListsEveryMoverField: M1's wallfling42 fields were in neither Fields nor Skipped; now saved; Core R 84 / 84 at 00:32; the fix
  is not in n42-r4). The first whole suite to finish since NIGHT40.
- Gates at dd37246a: golden 39 sets / 22 replays exact, 0 mismatches; fingerprint EQUAL plain and --off42; the Editor 0 errors.
- The context skill installed (~/.claude/skills/night-context); its close check run on tonight's files: 15 missing items -> the THREADS
  w40-w42 paragraph (T-59..T-64) written; the wave-memory harvest for 40-42 not run (time).

## Close table (00:35)
| card | state | measured |
|---|---|---|
| G0 premise | DONE (late) | the shared commit af1b680a 20:47 (planned 19:30: the connection drop 19:15-20:23 + the Editor's D3D12 crash at 20:01); CODE-FACTS written; 7 befores on n41-r7 looked at; the suite split works (M / N 209 / 209) |
| M1 movement | DONE (G2) / SHORT (G5) | wallfling42 ON: a wall press 8.00 -> 0.00 m/s back (wall42 frames), gates GREEN on every level with the checker's per-room ownership; boost42 OFF: lift at 45 deg 7.07 -> 11.00, 8 m climbs 3.8 -> 22.3 %, but the kiln G1 +10 % < the floor (kb1b roof; D-9) |
| L1 level 1 | DONE (G1) / SHORT (G6) | foundry42 ON: a person riding each wall 40.4 -> 100 %, the checker's ride tries that cross 6.8 -> ~20 %, walk 10/10, GANTRY frames lip / mid / g2; teach42 OFF: 2 lines in 60 s with and without it (D-11); K1's grinder + cover placed in b1 (walk 10/10 with arena42) |
| W1 weapons | DONE (G3, G10, G8 gate) / SHORT (G4, G8 view) | gauntlet42 ON 2.5 -> 1.8 s; charge42 ON: held shots 7 -> 1, a second charged shot False -> True; parry42 OFF: the swing 11/16 -> 16/16 in the harness, never framed (D-10); reticle42 OFF: +0.2 s spin 0.23 -> 0.25 only |
| F1 hits and kills | DONE | impact42 + kills42 ON: THUNDERCLAP 58 -> 85 ms, gauntlet husk 83 -> 143, tank 123 -> 183 ms + a ring / kick / the flinch x2.2; iterated (the ring covered ~70 % of the screen -> capped 9 deg); KESTREL kept at its 50 ms headshot freeze |
| U1 UI | DONE | meter42 ON: wrong fill 44 -> 0, false FULL 301 -> 0 over 754 frames; style42 ON (2 rank ups, combo 6; iterated: the award line, the ghost); health42 ON (the chunk at +0.3 s, gone at +1.0, the heal glow) |
| B1 THE ICEBREAKER | DONE | brawl42 ON: SWEEP (0.90 s) + LEAP (1.10 s), phase 2 double sweep; skilled 48.6 -> 55.9 s (46.2 with gauntlet42), casual deaths 10 -> 14 per 10 fights; no HP added; iterated (the amber landing circle -> red-orange disc); the gauntlet-hit reaction (chips, crack, recoil) |
| K1 arena kit | SHORT | THE GRINDER (+80 GROUND), breakable cover (+60 SHATTER), aimed pads; harness and arena42 reel GREEN; ARENA-KIT.md with 13 real images; the swing bar cut (rule 4); arena42 OFF: the grinder never seen on a frame (D-12) |
| X1 context skill | DONE | night-context brief / close (node, no deps); its close check found THE WICK dropped by the forge's HANDOFF rewrite and 15 missing items; installed at the close |
| L0 the lead | DONE | 4 builds (n42-r1..r4), 38 reels (befores 7, r1 4, r2 8, r3 8, r4 11) + the smoke (cap 50), the full suite 998 / 999 Game + 1095 / 1095 Core, golden / fingerprint at every pick; forge experiment: OFF rows whose reason is "unseen" = 2 (parry42, arena42) vs NIGHT41's 3 |

## GG's asks: each fate
| ask | fate |
|---|---|
| G1 the wall-ride section | **ON** (foundry42: THE GANTRY rebuilt forgiving in n42-foundry) |
| G2 flung back near a wall | **ON** (wallfling42) |
| G3 the gauntlet 0.7 s faster | **ON** (gauntlet42: 1.8 s) |
| G4 the drone-orb parry | OFF, built and measured (parry42: never on a frame; D-10) |
| G5 the shotgun boost | OFF, built and measured (boost42: the kiln G1 roof; D-9) |
| G6 too much instruction | OFF, built (teach42: no measured difference on the reel; D-11) |
| G7 the meter | **ON** (meter42) |
| G8 the reticle | half **ON** (charge42's wait for the cannon turn); the faster view OFF (reticle42, not shown faster) |
| G9 hits and kills | **ON** (impact42 + kills42) |
| G10 hold fire after a charge | **ON** (charge42) |
| G11 a context skill | **DONE** (night-context, installed) |
| G12 arena elements | OFF, built and placed (arena42: never seen on a frame; D-12) |
| G13 style meter + health bar | **ON** (style42 + health42) |
| G14 THE ICEBREAKER harder | **ON** (brawl42) |
| G15 the gauntlet's hit | **ON** (impact42 + kills42 + B1's boss reaction) |

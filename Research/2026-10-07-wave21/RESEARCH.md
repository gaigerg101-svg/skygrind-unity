# Wave 21 (NIGHT21) research: what to build, and why (wave-forge, 2026-10-07 07:44-08:25)

Inputs: GG's two requests of 2026-10-07 (verbatim in `docs/memory/OWNER.md` and the playbook library),
`IDEAS-POOL.md` (28 cards, 9 lenses incl. the role-play panel), and three web-only research passes:
- `research/ai-built-games.md`: 17 sources. How people building games with AI agents get real features built.
- `research/genre-features.md`: 43 sources. Which features drove excitement in 2024-2026 movement and horde shooters.
- `research/tech-oss.md`: 6 topics. Permissive open-source code and techniques for each.

Search-snippet claims are marked there as unverified.

## §0 Our data first (read 2026-10-07)
- **NIGHT20 closed 3 DONE / 9 SHORT** (`NIGHT20-LOG.md` close table). The suite is 1569 / 1569; golden 0 / 22 exact;
  FingerprintBaselineN20 recorded at `560a4e0`.
- **The top open defect:** a magenta square (missing material) where the style rank letter belongs. All 6 blind
  reviewers saw it (NIGHT20-LOG round 4 + final), and it cost tonight the HUD question 11 of 12.
- **What blind pairwise picks were worth:** they moved little (S15 26 / 56). The veto findings were what helped (the
  magenta square, words where glyphs belong). The playbook says the same across 50 waves (§3.4).
- **Six builders hit the account spend limit at ~3 h** on NIGHT19 and NIGHT20. The close's farm ran 3-5x slow beside
  the suite and another session's jobs.
- **Spike 1, instant replay.** `TapeReplay.Compose` replays the tape headless into a fresh `SurvivalSim`. The views bind
  to the live sim when they're constructed (`new Beacon20View(this, sim.beacon20)` in `SurvivalGame.Init.cs`). So a
  rendered replay is not a cheap re-sim: the playable slice is a **render-side pose recorder** (a ring buffer of the
  camera and the enemy views' poses, played back). That makes it the moonshot.
- **Spike 2, the hook.** `Mover.Impulse(dx, dy, dz)` (`Core/Mover.cs:306`) and `world.RayIntersect` exist, and the
  shockwave already pushes the player from a gun module (`Core/Guns/Shockwave.cs`). A hook is the same pattern: a ray to
  an anchor, then a pull impulse per step. Off = no effect, so golden stays exact.
- **Spike 3, THE GRIND's premise.** The arena's core is a height grid: `CoreGrid.SetHeights` (`Core/CoreGrid.cs:264`)
  and `CoreGrid.PresetHeights` (as used by `ArenaMap.cs:121`). Reshaping it between waves is the engine Cyber Grind
  needs. Whether SURVIVAL keeps its core flat by design: `ResetWorld` says so (`TapeReplay.Compose`'s comment), so THE
  GRIND lives on the ARENA map.

## §A What the outside evidence says (details and URLs in research/)
1. **AI-built features fail by being "defined but not wired" and by self-grading.**
   - A browser-game postmortem found 15 behaviour flags in data that no code read, and bosses defined but never built.
   - Anthropic's long-running harness work: a JSON feature list with a `passes` flag per feature that flips only on
     evidence; a separate evaluator with a sprint contract agreed before code; models grade their own work too kindly
     and leave features display-only.
   - Sources: anthropic.com/engineering/effective-harnesses-for-long-running-agents,
     anthropic.com/engineering/harness-design-long-running-apps, gonzoml.substack.com/p/how-we-built-a-full-browser-game.
2. **Parallelise only disjoint files, and keep the sim to one agent** (Anthropic's C-compiler post: lock files, a
   near-perfect verifier, a 1-10 % sampled fast regression mode; new features still broke old ones late).
3. **Personas are for triage, never acceptance.**
   - A persona playtester grounded in 150K reviews was preferred 70 % of the time (arXiv 2601.07251).
   - Simulated users misjudge results by up to 15.1 % calibration error (2601.17087, snippet).
   - The best model found 48 % of game bugs (2604.02648).
4. **The genre's best evidence-to-cost features:**
   - BO6 Directed mode doubled main-quest completion (4 -> 8.23 %).
   - A ghost of your best daily round plus a board, unlocked after a first clear (Neon White's devs).
   - A kill-or-die timer (Mullet MadJack, 97 % of 5k+ reviews, with a fatigue warning).
   - Opt-in curses for exclusive rewards (BO7 relics).
   - Custom-rules sliders (Doom: The Dark Ages).
   - The slide as a weapon (Turbo Overkill, 95-96 %).
   - ULTRAKILL's Cyber Grind, an endless arena that reshapes each wave with per-difficulty boards (ULTRAKILL 96 % of
     207k).
   - The P-rank.
5. **Avoid** (evidence of harm): frame-rate-dependent behaviour (RoR2 Seekers of the Storm, 34 %), hard level timers
   (Rogue Core opened Mixed), grindy paid progression (Splitgate 2 lost 80 % in 27 days).
6. **Tech:**
   - Replays from snapshots plus inputs (the Doom demo format; Overwatch's GDC 2017 replay talk).
   - Ghosts as position samples (Neon White).
   - Gyro and flick stick: GamepadMotionHelpers, MIT.
   - Unity grapple, movement and destruction repos all run on Rigidbody or PhysX, so they serve as maths references
     only (rule 4).

## §B The scored shortlist (wave-forge weights: owner fit x3, player impact x3, fits the game x2, feasible x2, provable x1, novelty x1; out of 60)
| card | feature | fit | impact | game | feasible | provable | novel | total | the evidence |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| C-27 + C-08 | THE STAMP: P-rank round grades + the rank letter as an inked stamp (fixes the magenta square) | 5 | 4 | 5 | 5 | 5 | 2 | 54 | ULTRAKILL P-rank; 6 / 6 reviewers vetoed the square |
| C-05 | THE HOOK: a grapple verb (pull to a surface, or yank a light enemy) | 5 | 5 | 5 | 3 | 4 | 3 | 53 | Titanfall 2 grapple, ULTRAKILL whiplash; GG: "new fetaures that match", speed |
| C-24' | DEADLINE: a kill-or-die timer twist round (kills refill; slide / slam kills refill most) | 4 | 4 | 5 | 5 | 5 | 3 | 52 | Mullet MadJack 97 % |
| C-13' | SLIDE BITES: the slide hits (contact damage, a gib, a style event) | 4 | 4 | 5 | 5 | 5 | 2 | 51 | Turbo Overkill 95-96 % |
| new | THE GRIND: an endless ARENA mode whose core grid reshapes every wave, a local board per HEAT | 4 | 4 | 5 | 4 | 5 | 3 | 50 | Cyber Grind (ULTRAKILL 96 % of 207k); CoreGrid exists |
| C-03 | GHOST: your best DAILY run's ghost (cyan) beside you, unlocked after a first clear, + a challenge code | 3 | 4 | 4 | 4 | 5 | 4 | 46 | Neon White gating; Trackmania |
| C-01 | INSTANT REPLAY / KILL CAM (moonshot) | 4 | 5 | 4 | 2 | 4 | 4 | 47 | Doom demos, Overwatch GDC 2017; streamers |
| C-14 | THE DUSK CLOCK | 5 | 4 | 5 | 4 | 4 | 4 | 53 | GG's "identity bland" (NIGHT16); no outside evidence found: beyond |

Runners-up go to `IDEAS.md`:
- Directed steps for THE HUNT and THE BEACON
- opt-in CURSES
- custom-rules sliders
- gyro and flick stick
- haptics
- photo mode
- supply pods
- contract doors
- perk augments
- the main quest (it belongs to the map night, `QUEUE-map-night-2-v5.md`)

## §C' After the red-team (08:00-08:10; it supersedes §C where they differ)
A fresh reviewer, given only the files, found 15 issues. What changed:
1. **THE GHOST already exists** (NIGHT14, `Game/Route/Ghost*.cs`, `design.ghost` on). It's cut from the headlines; its
   DAILY first-clear gate is beyond row 1 (about 20 min). The moonshot builds on its recorder.
2. **THE GRIND is the web's `endless.js`** (PORT-STATUS "Missing"; `config.endless`). It's a port on SURVIVAL's core
   (TITLE's "ARENA" is the default map, not `Arena.unity`), with the web's numbers and events.
3. **Switches turn on with their first playable slice.** The off side is `--off21`, and the fingerprint forces N21 off
   until the freeze. Without this, no reel can show a feature, so `passes` could never flip.
4. **Tape format 6** carries the N21 bits.
5. **The fingerprint re-record comes before the final build**, at the freeze.
6. **The close was resized:** freeze at 4:30, the suite at 4:50-5:30, farms only after it.
7. **The pre-placed partials** (`Harness.N21.<row>.cs`, `FeelReel.N21.<row>.cs`) and a config merge for the files
   everyone touches.
8. **Caps:** reels 14, persona triages 5, and a batch defined as 2+ processes or 20+ runs.
9. **The P-rank already exists** (`design.grade.p`), so the round P reuses its rule. T-11 (the RESULTS grade card over
   POINTS) is folded into THE STAMP.
10. **Every contract names its bot hook** (`chase --twist deadline`, the scripted `arcline` probe).
11. **The ARCLINE's yank cooldown is 15 s** (GG's long-cooldown rule).
12. **The spend limit is a valid SHORT reason**, with its `date`.
13. **S4's knob is dropped** (it's parked on DECIDE21).

## §C The slate (NIGHT21: "THE MOVES AND THE MOMENTS"; first draft, see §C')
- **P0 debt (45 min):** the magenta rank letter (inside THE STAMP); the bench baseline re-run alone if the machine is
  quiet (the final runs were contaminated). Parked on DECIDE (aged 2+ waves): S2 / S4 / S6, MARATHON's in-round heal,
  the crow's nest, the BULWARK bot.
- **Headlines, each a switch, a playable slice, a sprint contract and a reel scenario:** THE HOOK, SLIDE BITES (builder
  M); THE STAMP, DEADLINE (builder S); THE GRIND, GHOST (builder W).
- **Moonshot (builder W, 90-minute cap, after GHOST's recorder):** INSTANT REPLAY on DOWN and at round end.
- **Beyond (seeded leftovers, in order):** THE DUSK CLOCK; Directed steps for THE HUNT and THE BEACON; challenge codes
  on TITLE / RESULTS; haptics on kills and the shockwave; opt-in CURSES.
- **The loop's innovations** (from §A):
  - sprint contracts and `passes` flags in `NIGHT21-FEATURES.json`;
  - a wired-not-just-defined audit per feature;
  - a persona triage each round (5 personas grounded in OWNER.md and the genre research, triage only);
  - veto reviewers with budgets.
- **The forge experiment** (one change, logged in `docs/memory/ledger/wave-21.md`): sprint contracts with `passes`
  flags plus veto-only reviewers replace pairwise approval. Hypothesis: at least 4 of the 6 headlines DONE (NIGHT20:
  3 / 12 items DONE), and GG keeps at least 3 switches on. Measure: the close table and GG's DECIDE answers.

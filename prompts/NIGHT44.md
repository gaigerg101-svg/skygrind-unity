# NIGHT44.md: the finishing night: NIGHT43's open items closed on Player frames, THE RING made a place, every boss revealed like THE WICK, the FUSION and TORCH finished (lead + four builders; 2 h hard cap)

GG, 2026-10-16 ~20:05, right after NIGHT43's report, verbatim (Research/2026-10-16-wave44/GG-REQUEST.md, rows H1-H3):
> great start planning out next detailed prompt to finish whats not done and anything else you can think of, only 2 hours max

Why it matters: NIGHT43 turned 22 rows ON for GG's thirty asks, but some proof is still harness-only (the movement rows' own reel, THE
WICK's fight in a Player), THE RING was only dressed, and the whole suite was not run. GG plays the next build; what he meets first must
hold. **If GG has sent words or DECIDE43 / DECIDE44 answers since, they come first** (log how each changed a card before builders start).

Base: port/stage3 at u3-n43 plus the forge commit. Read first: CLAUDE.md, docs/PRINCIPLES.md, docs/DESIGN-RULES.md, docs/ROOMS.md,
docs/VERBS.md, HANDOFF.md, NIGHT43-REPORT.md, NIGHT43-LOG.md (round 4 + the close table), DECIDE43.md, DECIDE44.md,
Research/2026-10-16-wave44/SPIKE.md (each premise read in code with file:line) and FORGE.md (the reviewer's findings and how they were
fixed). The builder brief: C:\Users\gaige\dev\SKYGRIND-unity\NIGHT44-BRIEF.md (untracked; absolute path). t0 = GG's go.

<goal_for_the_night>
Inside two hours: the suite's priority batches run and their failures fixed or listed; THE WICK killed on Player frames by its reel
(T-59); the movement rows proven by their own reel in a Player (a chimney climbed, B after a ride, the boost apex: T-66); the BURROWER's
counter on a frame (T-68); THE RING a place GG can read (the landmark whole from the spawn, a loop, worm-proof plates where they matter, no
lava (DECIDE44 D-2), the worm's routes unchanged: T-65); every boss revealed like THE WICK (DESIGN-RULES B-3); the FUSION streaks and the
TORCH cone finished; no stale centre card after a room change (T-69). Nothing GG had is lost; `-skygrindOff44` is NIGHT43's game; the play
build stays Builds/n43-r5 until a new one passes the verbs and the smoke.
</goal_for_the_night>

<method>
1. **Clock (hard cap 2:00 by the clock; report SHORT instead of running over):** G0 0:00-0:20; builders 0:20-1:10; the suite's priority
   batches 0:20-0:48 (hard stop; nothing else heavy in the Editor beside it); integration + build ~0:50 (rows decided from THIS build's
   frames); an R2-only build ~0:35 if R2 has picks (its four reels, ~5 min); last picks + build ~1:12; the final round 1:15-1:42: verbs37
   on levels 1-5, the smoke, one proof frame per card (nothing else); freeze ~1:42; close 1:42-2:00.
   Building share: builders ~48 of their 50 minutes build; the lead's measuring is the suite, two rounds of frames and the final round.
2. **Feel is measured and seen (L-001):** each row names its number and its frames; a row turns ON only after the lead looked at its
   before / after pair on the ~0:50 build (a row first seen later stays OFF with its reason).
3. **Every sim change behind N44Blocks** (`-skygrindOff44` / `--off44` = NIGHT43's game); view changes behind design.n44.vis44; reel and
   harness fixes need no row (test code). No new level ids tonight: kiln44 makes n43-kiln load THE RING's new arena file.
4. **Four builders, one area each.** The spend limit stopped NIGHT43's nine for 1.6 h and would stop the lead too (L-003): commit every
   slice; **the lead writes the close-table skeleton and a HANDOFF note at ~0:50 and ~1:20** so a stop leaves a state GG can read.
5. **Disk:** C: had ~22 GB free at the forge (a reel ~0.65 GB, a build ~0.3 GB; the reel script skips under 10 GB and only says
   "SKIPPED"). G0 checks free space; the lead deletes each round's numbered frames (f*.jpg) once looked at (its own output), keeps shot-*.png.
   Budget: at most ~22 reels for the night.
6. **Nothing lost:** verbs37 on levels 1-5 after rows go ON (NIGHT43: fusion43 killed launch36's verb 41 indirectly), then the smoke.
7. Builders never run the Editor or a Player; the lead cherry-picks (no `git merge`), runs every reel, the suite and git. After each pick:
   the gates and a duplicate-key check of the config (NIGHT43: a builder's sync commit duplicated two rows).
</method>

<cards>
**G0 premise and shared commit (the lead, 0:00-0:20).** `date`; the Editor stopped; `df` (free space as above); GG's words / DECIDE
answers first. The shared commit: N44Blocks (kiln44, reveal44), design.n44 (+ view row vis44; kiln44 holds `arena: "Design/maps/
wyrm44-arena.json"`, no levelFrom), `--off44` / `-skygrindOff44`, tape format 27, SurvivalSim.N44 (partials: N44KilnStartRun / Step K2,
N44RevealStartRun / Step B2), **one boss-start event** emitted where every boss starts its fight (`bossKit.Start`: Brawl40.cs:197,
Crown37.Boss.cs:83, Wyrm24.cs ~148, Boss30.cs:87; name it N44Events.BossStart with a payload {boss, position}) so B2 hooks nothing else,
FeelReel.N44 dispatch (proof44 R2, ring44 K2, look44 V2, reveal44 B2), harness partials (N44Proof R2, N44Kiln K2, N44Reveal B2), the test
chain (AsFormat26; N37SharedTests / Lesson25 force NIGHT44 off). n43-shared-patch.cjs patches by exact text anchors: write
n44-shared-patch.cjs's anchors by hand from the N43 lines it added (not a find-and-replace); also copy n43-gates.sh -> n44-gates.sh
(--off44), n43-rows-on.cjs -> n44-rows-on.cjs (design.n44), integrate.sh's builder map (four ids, Logs/night44-picked.txt). Gates:
golden, fingerprint plain and --off44, the Editor compile, the shared tests.

**P0 the suite (the lead, 0:20-0:48).** Tools/night/n42-suite.sh: Core, then the Game batches NIGHT43 touched (N4*, Lesson, C*, MapC,
U*); hard stop at 0:48; every failure fixed (if NIGHT43's) or listed with its test; the batches not reached listed as SHORT (T-70 stays
open for them).

**R2 proof on Player frames (builder R2, 0:20-1:10).** H1: T-59, T-66, T-68 (T-67 stays open unless `n43level walk` passes 10/10).
Owns the reel files only: FeelReel.N37.Wick.cs, FeelReel.N43.Move.cs, FeelReel.N43.Kiln.cs, FeelReel.N44.Proof.cs, a COPY of any pilot
it changes (Walk38Pilot / Wick37Pilot stay as they are: every harness walk and gate uses them), Harness.N44.Proof.cs. No game rule.
**Harness first:** reproduce each reel's loop headless in SimHarness (`n44proof wick|move|ash`) and make it pass there, then the reel.
- T-59: the wick37 fight drives its pilot through an IPlayerInput adapter calling pilot.Step once per sim step (sim.UseInput, as
  FeelReel.N43.Foundry.cs:37 does) instead of OR-ing presses into forcedMove per frame (SPIKE 1). Acceptance: "killed True" + frames of a
  parry, a crack, the OPEN window, a leg change and the kill.
- T-66: move43's chimney at 3.5-4.0 m from floor + 3 m (THE FLUE is 3.5 m), the flip per sim step (SPIKE 2); B after a ride reached on a
  real ride. Acceptance: M43-chimney-on-top above -off-top; M43-bdrop-on keeps >= 12 m/s; boost-on apex >= 6 m.
- T-68: kiln43's ash bed fires on BurrowEmerge inside 0.5 s (SPIKE 4). Acceptance: "counters 1".
- If time: T-67's walk pilot hop (a copy of Walk38Pilot for n43-foundry's W3 -> W4), `n43level walk` b2.

**K2 THE RING (builder K2, 0:20-1:10).** H1 T-65. Owns Tools/night/gen-n44-wyrm-arena.cjs (from gen-n43-wyrm-arena.cjs, its 2.9 m
clearance rule against the worm's existing routes), Design/maps/wyrm44-arena.json, the arena pick under kiln44 (SurvivalSim.Kiln39.cs's
per-level arena choice: one hook into K2's own partial file), FeelReel.N44.Ring.cs, Harness.N44.Kiln.cs (`n44kiln wyrm` with a
`--level` / `--arena` argument; NIGHT43's `n43kiln wyrm` reads n43-kiln.json, `n43wick wyrm` reads n38-kiln.json).
- The worm's routes stay (Design/bosses/wyrm26.json untouched: GG never complained about the fight). Inside the clearance: THE KILN MOUTH
  reads whole from the spawn (today K1's 12 m ride slab hides its lower part: move the slab or the mouth), one loop around the arena with no
  dead ends, the worm-proof plates where the worm's dives and lunges land (each plate's reason: which attack it answers), every piece's
  reason in the generator (DESIGN-RULES A-1). No lava (DECIDE44 D-2: WyrmArena carries no Lava30 rule; painted lava lies). A1's
  WyrmRing43View reads the arena data and follows; K2 does not edit it.
- Acceptance: `n44kiln wyrm` killed with the bar valid, the band 30-60 s bot, before (wyrm43-arena) / after (wyrm44-arena); ring44 frames
  from the spawn and mid-fight, before / after.

**V2 visual finish (builder V2, 0:20-1:10).** H2. Owns view files under design.n44.vis44: the FUSION view (Game/Fusion43View*), the TORCH
spray view (GunFx.Spray20 / Torch43 view parts), MessageSlot33 / UiHud36Text / UiTeach43 (T-69), UnityTuning.Vis44.cs, FeelReel.N44.Look.cs.
- The FUSION's streaks: today flat outlined bars (n43-r3 on-fusion-050): tapered, curved streaks and a hot-pink / white core that reads at
  +0.05 s (the colour pending DECIDE43 D-3: say so in the report).
- The TORCH: a dense cone from the muzzle to the target at 1 s (on-torch-1000 shows a thin stream).
- T-69: on roomEnter drop the Lesson priority in MessageSlot33 and close the teach43 card (SPIKE 3; a real player rarely sees it: last).
- Proof: FeelReel.N44.Look compares **vis44 off vs on with feel43 on** at the same moments (the centre difference >= 0.02 on fusion +0.05
  and torch +1.0); a room-change frame with no stale card.

**B2 every boss's reveal (builder B2, 0:20-1:10).** H2 (DESIGN-RULES B-3; GG G15 liked THE WICK's intro). Owns Core/Reveal44.cs (B1's
wick43 look ease generalised), Game/Reveal44View.cs (letterbox, the name card, lights in order), the subscriber to the shared commit's
N44Events.BossStart (no edits in Brawl40 / Wyrm24 / Crown37 / Loom37 / Boss30), design.n44.reveal44, Harness.N44.Reveal.cs,
FeelReel.N44.Reveal.cs. THE WICK keeps wick43's reveal (reveal44 skips it).
- Each of THE ICEBREAKER, THE KILN WYRM, THE LOOM, THE CROWN: the camera's look eased toward the boss as it appears (the player keeps
  control; their own turn cancels it), letterbox, the name card, ~2.5 s the first time, ~1 s on a retry (B1.md's numbers).
- Acceptance: the yaw error to the boss under 5 deg at the reveal's end, for each boss; the bands unchanged (brawl40casual killed; THE
  KILN WYRM killed); reel frames reveal-before / 1 / 2 / 3 for THE ICEBREAKER and THE KILN WYRM.
</cards>

<builders>
| Builder | Owns |
| --- | --- |
| **R2** proof | FeelReel.N37.Wick / N43.Move / N43.Kiln, FeelReel.N44.Proof, copies of pilots, Harness.N44.Proof; no rules |
| **K2** THE RING | gen-n44-wyrm-arena.cjs, wyrm44-arena.json, kiln44's arena pick (own partial), FeelReel.N44.Ring, Harness.N44.Kiln |
| **V2** visuals | Fusion43View*, the TORCH spray view, MessageSlot33 / UiHud36Text / UiTeach43, FeelReel.N44.Look, vis44 |
| **B2** reveals | Core/Reveal44.cs, Game/Reveal44View.cs, the BossStart subscriber, FeelReel.N44.Reveal, Harness.N44.Reveal, reveal44 |
| the lead | G0, the suite, integration, every Player check, git, DECIDE44, the report |

**Hand-offs:** K2's arena -> B2's THE KILN WYRM reveal frames; R2's kiln43 fix runs on n43-kiln (kiln44 keeps the id). Builders cannot
message each other; the lead passes them on.
**Keep first if short:** the suite; T-59 (THE WICK on frames); T-66; THE RING's landmark + loop; the reveals for THE ICEBREAKER and THE
KILN WYRM; the FUSION streaks. **Cut in this order:** T-69, THE LOOM / THE CROWN reveals, the TORCH cone, T-67.
**Leftovers (approved if a card ends early, in order):** THE STEPS' step 2 (54 % landing; needs a new level id: say so before starting);
PIERCE's hatch frame without the red spawn sphere covering the husk; level 2's look pass (look43's kit on n43-iceline); the w40-w42 memory
harvest (the lead).
</builders>

<close>
Close table, one row per card: G0, P0 suite, R2, K2, V2, B2, verbs (levels 1-5), the smoke, memory. Each DONE or SHORT with its measured
reason. Then NIGHT44-REPORT.md for GG (outcome first, one line per H-row and card, what was not verified, what felt worse), HANDOFF.md,
THREADS.md (close T-59 / T-65 / T-66 / T-68 / T-69 / T-70 only where a frame or a run shows it; T-67 only if the walk passes),
night-context close. Numbers from this night's runs only.
Closing reminder: verbs on every level AFTER rows go ON; a duplicate-key check after every pick; git diff before restoring Play mode's
font / TMP rewrites (L-047); delete your own numbered frames after each verdict (disk).
</close>

/goal (paste after GG's go):
NIGHT44 is met when NIGHT44-LOG.md's close table is printed with every row DONE or SHORT (each with its measured reason), or when 2 hours
have passed since t0 (then print the table with what exists, the play build staying n43-r5 unless a new build passed verbs and the smoke).
Work NIGHT44.md in order: G0 and the shared commit, the suite's priority batches, R2 / K2 / V2 / B2 in parallel, builds at ~0:35 (R2) and
~0:50 and ~1:12, verbs37 on levels 1-5 and the smoke on the final build, the close. Never stop on a summary that announces the next step,
an offer to continue, or a list of decisions that block nothing; stop only when the table is printed or the cap is reached.

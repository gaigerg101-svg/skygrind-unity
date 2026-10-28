# NIGHT45.md: the arena identity night: three arenas GG will play next rebuilt from identity cards (not pieces placed at random), the night's machinery made lean, the old fluff cut (never anything meaningful) (lead + five builders; 2 h hard cap)

GG, 2026-10-17, verbatim (Research/2026-10-17-wave45/GG-REQUEST.md, rows J1-J6):
> what should next detailed prompt focus on, see where we are, follow best methods, dont repeat same mmistakes, reserach folllow best game deisgn pronicples and if it arena related please dont just randomly plce stuff it needs intension and personality and lets cut fluff and old stuff, dont cut anything meaningful, think before writing this prompt

**If GG has played n43-r5 / n44-r1 or answered DECIDE43-45 since, his words come first** (log how each changed a card before builders
start). Base: port/stage3 at the NIGHT45 forge commit (on u3-n44: DESIGN-RULES A-9, the audit, the research, Tools/night/n45-levels.cjs).
t0 = GG's go. The lead writes `date` on every log line and on the close table.

## Where we are (J1)
- NIGHT43 turned 22 rows ON for GG's thirty asks; NIGHT44 (one hour) added vis44 and fixed three reels. Play build: Builds/n44-r1. GG has
  not played either yet.
- On Player frames: the UI, the FUSION, the parry, the doors and counter, level 1's GANTRY / FLUE / PIERCE, level 3's STEPS and ASH BED,
  THE WICK riding all 15 panels to leg 3, the BURROWER's counter. Harness-only: the movement rows' own reel (T-66), THE WICK's kill on
  frame (T-59), the TORCH cone. Not run: Core's suite and Game MapP-Z (T-70); the smoke failed for the machine (n44-r1 and the n43-r5
  control alike).
- The arena audit (Research/2026-10-17-wave45/AUDIT.md, 27 fight rooms): NIGHT43 rebuilt links and teaching rooms, not the big fights;
  every room's "landmark" is look43's same generic exit pylons; no room has a place fantasy. Weakest the player meets next: **b1 THE YARD**
  (level 1: 3 of 8 pieces reasonless, a5's 30 s problem), **g7 THE MIX** (level 2: 16 bodies on 5 generic pieces, no hazard), **k5 THE RING**
  (level 3's boss: 0 of 76 props with a reason; geometry SHORT T-65). Levels 4 / 5's template rooms (KNOT YARD, SPOOL HALL, HOOK YARD,
  NAVE, CHOIR) are next night's (DECIDE45 D-5).
- The research (J3): Research/2026-10-17-wave45/research/arena-identity.md, folded into docs/DESIGN-RULES.md as **A-9: identity before
  geometry** (the arena identity card). Older: wave-43 encounters-arenas-web.md, wave-42 arenas-web.md.

## The mistakes this night is built not to repeat (J2; NIGHT42-44, LESSONS, the audit's section C)
| mistake (nights) | tonight's guard |
|---|---|
| the spend limit stopped every agent, the lead too (w19, w20, w42, w43) | five builders; every slice committed; the close skeleton + a HANDOFF note at ~0:45 and ~1:15 |
| the shared commit cost 15-20 min and touched 39-52 files (w42, w43) | `node Tools/night/n45-levels.cjs`: pre-written, dry-run twice by the forge (0 compile errors, golden 0 / 22, fingerprint EQUAL plain and --off45); level-only rows, no tape bump |
| a level swap silently dropped a system that checked the id (the forge's review caught it: level 3's boost lessons) | the script routes the five direct id checks through N45Levels.BaseId; builders' new checks must too (the brief) |
| rows ON on harness numbers; reel pilots broken (w43, w44) | an arena turns ON only on its own Player frames (entrance + mid-fight); reel pilots step per sim step (wave-44 SPIKE 1) |
| a new row broke an old verb indirectly (w43) | verbs37 on levels 1-5 (levels 1-3 booted as `-skygrindMap n45-*`) after the rows go ON, before the smoke |
| levels patched on copied plans; reasonless pieces (w36, w43) | A-9: the identity card first; every solid's reason in the generator; a piece no card field asks for is cut |
| sync commits duplicated JSON keys; helper names clashed; one shared line edited by three builders (w43; the review) | no sync commits; helpers prefixed; reel hooks pre-placed per builder; a duplicate-key check after every pick |
| the suite not run before the freeze (w42-w44) | P0 runs Core + the Game batches NIGHT44 missed, by their real names |
| the smoke failed for the machine (w42, w44) | GG is told at the go: the pad unplugged, nobody at the machine at ~0:40 and ~1:35 |
| the memory harvest skipped (w40-w44) | C1 runs `wave-memory harvest` 40-44 |
| disk ran low (w43, w44) | `df` at G0; numbered frames deleted after each verdict; old logs on DECIDE45 D-1 |

<method>
1. **Clock (hard cap 2:00 by the clock; report SHORT instead of running over):** G0 0:00-0:10; builders 0:10-1:15; P0 0:10-0:40;
   integration + build ~0:45 and ~1:15 (each built with the n45 rows OFF; their frames taken with the rows ON in the build's copy:
   `node Tools/night/n39-build-rows.cjs <tag> on n45.<row>`); the final round 1:15-1:42 on the ~1:15 build: the ON decision from its frames,
   `node Tools/night/n45-rows-on.cjs <reasons.json>` in the repo (config only, no rebuild: the build's copy already matches), verbs37 on
   levels 1-5, the smoke; freeze ~1:42; close 1:42-2:00.
2. **A level-only night:** arenas change level data and generators only, with systems that exist and are data: lava (Lava30, both ways),
   campaign.lights (fixtures; no triggers), props with `why` / `job`, ride panels, noride, plates, THE RING's one crumble at half health
   (crumble31: box indices, keep them valid), spawn33 tells, afterLesson43 / fallReturn43 / waveNodes43 (a wave from a new side is the
   state change data can make), burrowers. A change that needs new sim code (a light that flips, a second crumble, a pour that starts)
   goes in the card as "next night". `-skygrindOff45` / `--off45` = NIGHT44's game.
3. **Identity before geometry (A-9):** each arena builder commits its card first, into docs/ROOMS.md's new section "NIGHT45 identity
   cards" under its room's heading; then the geometry, derived from it; the generator carries each solid's reason (route / threat / world,
   V-4); the cut list filled from the walk / frames.
4. **Seen, not assumed (L-001):** an arena turns ON only after the lead looked at its entrance frame (the landmark reads), a mid-fight
   frame (the 30 s problem in play), a greyscale check (E-3), the walk clearing the rebuilt room, and its gates.
5. Builders never run the Editor or a Player; the lead cherry-picks, runs every reel, the suite and git; gates + a duplicate-key check
   after every pick. Builder rules: C:\Users\gaige\dev\SKYGRIND-unity\NIGHT45-BRIEF.md (absolute path: it is untracked).
</method>

<cards>
**G0 (the lead, 0:00-0:10).** `date`; the Editor stopped (status); `df`: tell GG the numbers at the go (no waiting: with < 20 GB and no
answer on D-1, delete only tonight's frames as they are judged); GG's words / DECIDE answers. `node Tools/night/n45-levels.cjs`, the Editor
compile (force an import if a new file is not seen), N45SharedTests + Lesson25 TheOrder, golden + fingerprint plain and --off45, commit;
n43-gates.sh -> n45-gates.sh (--off45); integrate.sh's map (five ids, Logs/night45-picked.txt); launch the builders.

**P0 the suite (the lead, 0:10-0:40).** `bash Tools/night/n42-suite.sh n45 Core`, then `... n45 Game MapP Me Mo N1 N2 N3 P R Sa Se Sh Sk
Sl Sm Sp St Ta Tel Tex To Tr V W Z` (the batches NIGHT44 did not reach, by the script's own names); fix NIGHT43-44 failures, list others.
Then the smoke control on n44-r1 (nobody at the machine, the pad unplugged).

**A1 THE YARD's identity (builder A1).** J4. b1 in n45-foundry (Tools/night/gen-n45-foundry.cjs from gen-n43-foundry.cjs: the plan copied,
only b1 rebuilt; every other room bit for bit). Today: spitters high, husks low (a5's problem), 3 of 8 pieces reasonless (arena42's
grinder / cover: arena42 stays OFF, GG's D-12; its pieces leave b1, logged in DECIDE45). The card first: level 1's verbs (the ride, the
KESTREL's pierce from a ride, the dash-punch launch into lava), a 30 s problem no other level 1 room has, a place fantasy for a foundry
yard, a landmark of its own (a lit fixture + a hero prop with job "landmark" / "hero"), lava both ways where the card asks for it, the
state change data can make (a later wave from a new side through waveNodes43 / spawn33 tells), one earned shortcut. Proof in your own
Harness.N45.Yard.cs (`--level n45-foundry`): the gates, the walk clearing b1 10/10 (T-67's GANTRY bot is not your gate), levelgen 0
errors; frames from FeelReel.N45.Yard.cs (phase a145): the entrance, mid-fight, the landmark.

**A2 THE MIX's identity (builder A2).** J4. g7 (and g3 THE SWARM if time) in n45-iceline (gen-n45-iceline.cjs from E1's gen-n43-iceline.cjs).
Today: 30 x 32 m, 16 bodies on 5 generic pieces, no hazard, nothing tied to the tank or the swarmers (R-4). The card first: level 2's verbs
(the PULSE gauntlet, the FUSION dash-in, the orb parry, the charger's wall stun), the ice line's place fantasy (what the line carried),
an ice hazard both ways only if an existing system can be placed by data (check n40-iceline's floes / slides and THE BRAWL's floes; else
"none" with the reason), a landmark of its own, R-3's budget (<= 3 roles at once). Proof in Harness.N45.Mix.cs (`--level n45-iceline`; the
iceline walk pilot matches only n40-iceline: walk g7 with a copy that asks N45Levels.BaseId), levelgen 0 errors; FeelReel.N45.Mix.cs (a245).

**A3 THE RING's home (builder A3).** J4, J5, T-65. k5 in n45-kiln (gen-n45-kiln.cjs from gen-n43-kiln.cjs) + Design/maps/wyrm45-arena.json
(from K1's wyrm43-arena via its generator: the 2.9 m clearance to the worm's routes, which stay unchanged; crumble31's box indices
rewritten to stay valid). The card first: THE KILN WYRM's home (the arena is the boss's mechanic: worm-proof plates each tied to the attack
it answers; the one crumble at half health as the state change). The 76 props: each gets its reason (route / threat / world: banners,
chains and lamps that tell the kiln's world stay, V-4) or is cut and listed; keep the KILN MOUTH, the crucible, the seams where the worm
surfaces. Proof in Harness.N45.Ring.cs (`--level n45-kiln`: the fight killed with the bar valid, band 30-60 s bot, before / after);
FeelReel.N45.Ring.cs (a345): the entrance, mid-fight, after the crumble.

**S1 rows as data, the design (builder S1).** J5 (the night's machinery). Every sim night adds an N4xBlocks struct, a SurvivalSim.N4x
file, a tape format and rewrites six older shared tests. Build the generic replacement, **without a tape bump tonight** (the bump is
NIGHT46's G0): sim rows named in config (`design.nNN.<row>.sim: true`), one registry reading every night's sim rows in order, the planned
tape record (name -> bit, old formats replaying with new rows off) written and unit-tested in isolation, an off switch "-skygrindOffNN"
for every row from night NN on; nothing wired into the running sim tonight (the play build unchanged). Acceptance: dn-check 0, golden
0 / 22, fingerprint EQUAL plain and --off45, its own unit tests, and docs/NIGHT-ROWS.md: NIGHT46's shared commit as a checklist of at
most 3 files plus the exact one-time steps to switch on.

**C1 cleanup and memory (builder C1).** J5, J2. Docs and data only; never a verb, a feature, a level a check reads, or GG's files.
- docs/RETIRED.md: the rows OFF 3+ nights that a later row superseded (balance32, ride38, doors38 / juice38, stag38, pump40, boost42 /
  parry42), each with its evidence; nothing deleted from config (old tapes replay through them); GG's open calls listed as "GG decides".
- docs/ROOMS.md: rows for levels 4 and 5; the "not checked" cells of levels 1 and 3 EXCEPT b1, g3, g7, k5 (A1-A3's).
- T-62: n40-span's 7-word prompt to 6 words in all five places (gen-n40-span.cjs:283, Design/levels/n40/span.plan.json, n40/span.json,
  Design/levels/n40-span.json + its StreamingAssets copy; cmp the copies); text only, logged as the one in-place edit of an older level.
- `wave-memory harvest` for 40-44: ledgers 40, 41, 42, 44; LESSONS L-065 on (from NIGHT40-44 logs and this prompt's mistakes table);
  CORE's state line.
The lead at the close (worktrees cannot see untracked files): move the root's old untracked night prompts (NIGHT7-NIGHT44.md, drafts,
BRIEFs, PROMPT-*, QUEUE-*) into an untracked `nights/` folder (CLAUDE.md rule 1: night prompts stay untracked; the newest stay in the
root for night-context), delete the stray `Logs$TAG-tests.log` / `.xml`, run night-context close.
</cards>

<builders>
| Builder | Owns |
| --- | --- |
| **A1** | gen-n45-foundry.cjs, Design/levels/n45-foundry* + plan, design.n45.foundry45, ROOMS.md's b1 card, FeelReel.N45.Yard, Harness.N45.Yard |
| **A2** | gen-n45-iceline.cjs, Design/levels/n45-iceline* + plan, design.n45.iceline45, ROOMS.md's g7 / g3 cards, FeelReel.N45.Mix, Harness.N45.Mix |
| **A3** | gen-n45-kiln.cjs, Design/levels/n45-kiln*, wyrm45-arena.json + its generator, design.n45.kiln45, ROOMS.md's k5 card, FeelReel.N45.Ring, Harness.N45.Ring |
| **S1** | new Core / Game registry files (unwired), their tests, docs/NIGHT-ROWS.md, Harness.N45.Rows |
| **C1** | docs/RETIRED.md, ROOMS.md's L4 / L5 rows and "not checked" cells (not b1 / g3 / g7 / k5), the five T-62 places, docs/memory/ |
| the lead | G0, P0, integration, every Player check, git, the archive move, DECIDE45, the report |
**Keep first if short:** the suite, A1, A3, A2, C1's harvest, S1. **Cut in this order:** g3, S1's tape record test, C1's level 4 / 5 rows.
**Leftovers (approved, in order):** look45 (a room's own landmark replacing look43's generic pylons: the cards name the landmarks
tonight); T-66 move43's pilot per sim step; T-59 THE WICK's reel run to the kill; the TORCH cone measured; level 4's KNOT YARD's card.
</builders>

<close>
Close table (with `date`), one row per card (G0, P0, A1, A2, A3, S1, C1, verbs 1-5, the smoke, memory), each DONE or SHORT with its
measured reason. NIGHT45-REPORT.md for GG: outcome first; each rebuilt arena as its card's title, place fantasy and tell-a-friend line with
its entrance frame's file name; what was cut (J5) and why it was not meaningful; what was not verified. HANDOFF.md, THREADS.md (close only
what a frame or a run shows), night-context close. Numbers from this night's runs only.
Closing reminder: verbs on every level after rows go ON; a duplicate-key check after every pick; `git diff` before restoring Play mode's
font / TMP rewrites (L-047); delete your own numbered frames after each verdict.
</close>

/goal (paste after GG's go):
NIGHT45 is met when NIGHT45-LOG.md's close table is printed with every row DONE or SHORT (each with its measured reason and `date`), or
when 2 hours have passed since t0 by `date` (then print the table with what exists; the play build stays n44-r1 unless a new build passed
verbs and the smoke). Work NIGHT45.md in order: G0 (n45-levels.cjs) and the builders, P0 the suite, builds at ~0:45 and ~1:15, verbs37
on levels 1-5 and the smoke, the close. Never stop on a summary that announces the next step, an offer to continue, or a list of
decisions that block nothing; stop only when the table is printed or the cap is reached.

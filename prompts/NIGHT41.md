# NIGHT41.md: the finish night: everything NIGHT40 left off or half-done, seen on screen and turned on, plus the polish around it (lead + eight builders; 5 h build, 1 h test, hard cap 6:00)

GG, 2026-10-14 ~08:40, verbatim: "ok lets make a prompt related to things that were not done or still partial, expand upon that much more
and think of other things to imporve and make better". The full list, each row with its evidence and the code facts behind it:
Research/2026-10-14-wave41/INVENTORY.md (A = NIGHT40's not-done / partial, B = older rows still OFF, C = new improvements tied to GG's
words). GG had not played n40-r5 when this was written: **if GG has sent words or DECIDE40 / DECIDE41 answers since, they come first**
(log how each changed a card before any builder starts).

Base: port/stage3 at u3-n40 (220f753c) plus the forge commit. Read first: CLAUDE.md, docs/PRINCIPLES.md, docs/VERBS.md, HANDOFF.md (the
NIGHT40 block), NIGHT40-REPORT.md, NIGHT40-LOG.md (the close table), NIGHT40-PROOF.md, DECIDE40.md, DECIDE41.md, and
Research/2026-10-14-wave41/: GG-REQUEST.md, INVENTORY.md; research from wave 40 (Research/2026-10-14-wave40/research/: boss-web.md,
enemies-web.md, feel-web.md, CODE-MAP.md). NIGHT40-BRIEF.md is the builder brief's template (untracked). t0 = GG's go.

<goal_for_the_night>
Nothing GG asked for in NIGHT40 is left half-done. The shotgun's quicker pump, the pistol's rhythm and the slug's look are seen on screen
and on. Level 2 reads as an ice canal at dusk. THE LOOM sweeps its floor so the walls and hooks matter. No more than two enemies wind up
at once. Bosses enter with a shake, a flicker and their rule; their bars show their phases; doors show whether they are open. Every new
line shows the pad's buttons on a pad, and aim assist finds a boss's weak point. Level 5 has grapple nodes, the weakest silhouettes are
fixed, enemies break apart when they die, and the new enemies appear on levels 3 and 5 too. Level 6 has a plan GG can react to. The full
test suite runs to the end. Nothing he had is lost.
</goal_for_the_night>

<method>
1. **Shape (GG's NIGHT40 words):** G0 0:00-0:30; build from the shared commit (~0:30) to 4:20; integration at about 1:30, 2:30, 3:30;
   round 4 in the Player 4:20-5:00; freeze 5:00; test hour 5:00-5:45; close 5:45-6:00; hard cap 6:00.
2. **See it first, then build:** NIGHT40's misses were mostly "never on a frame". So the first slice tonight is the proof tooling (T2):
   per-frame weapon telemetry and the reel scenarios that frame a volley, a locked door, a boss entrance and the pad's text. Every
   view row turns ON only after the lead looked at its frames and one iteration (L-001).
3. **Every change behind a switch; `--off41` / `-skygrindOff41` is NIGHT40's game.** Level edits go to new ids (n41-foundry, n41-kiln,
   n41-spire), each swapped in by its row through the hooks NIGHT40 placed (Curriculum25 levelFrom / levelTo, maps.list rows, the
   startSet / awards data). **Register every new level id in design.maps.list in the shared commit** (NIGHT40's G0 missed it).
4. **Existing systems, not new ones** (L-023): aim assist exists (Core/AimAssist.cs, pad "assist", bosses at centre mass): add the weak
   point; attack tokens exist (Core/HuskBrain.cs:31-110): add a campaign cap across pools; pad glyphs resolve per device already
   (UiLesson25 / UiTeach35 on PlayerDevices.Last): prove it on a pad reel; juice38 / doors38 are built: frame them.
5. **Nothing lost:** VERBS.md rows touched are marked first (9 / 11 on n41-foundry's band; 17-23 the grapple on level 5; 40 the parry
   under the token cap; the gauntlet's big-enemy hit). Verbs on levels 1-5 at round 1 (the before: NIGHT40's freeze verbs are it) and
   round 4 (the after). Rows 17 / 19 / 22 / 23 on level 5 go from RED (no node) to PASS.
6. **One builder per area, end to end;** builders never run the Editor or a Player (the lead does); `git merge` is blocked for builders:
   the lead cherry-picks, builders read `git show port/stage3:<path>` (memory: builder-merge-blocked).
</method>

<cards>
**G0 premise (the lead, 0:00-0:30).** `date`; the Editor (stopped, not playing); free memory and disk; `git status`; GG's words / DECIDE
answers first. The shared commit: N41Blocks (sweep41, tokens41, heavy41, aim41, foundry41, kiln41, spire41, finale41), design.n41 (+ view
rows juice41, bars41, entrance41, select41, look41, brawlLook41, models41), `--off41` / `-skygrindOff41` (N29Fixes too), tape format 24,
the maps.list rows for n41-foundry / n41-kiln / n41-spire / n41-finale, harness partials per builder, Curriculum25 rows for the n41 levels.
**The pre-placed hooks (the red-team's findings; L-021 / L-050):**
- **Old view rows turned on through n41 rows, never in place:** design.n41.juice41On / doors41On / pump41On, and the old checks become
  `old.enabled || (n41 row on && !N41 off)` (juice38 / doors38's reads of N38Blocks / their rows, Viewmodel.Pump40On), so `--off41` is
  NIGHT40's game and the n38 / n40 rows are never edited (rule: lean_rules 2).
- **heavy41:** a `Heavy41(kind, ref knock, ref hitstop)` partial hook at the gauntlet's knock-back / hitstop (Core/Combat.cs:472-473 and
  the ImpactRules hitstop), filled by E6 in a new Core/Heavy41.cs; "big" is a per-kind flag in design.n41.heavy41 (E6 never edits Brawl40).
- **Weak points:** a `WeakPoint41` (position, active) field on the boss systems (IBossSystem or a small interface) that aim41 reads; B8 fills
  it for THE LOOM and THE ICEBREAKER, P3 for THE CROWN (P3 owns SurvivalSim.Crown37*.cs / SurvivalSim.N40.Crown.cs tonight for that only).
- **Boss bars:** a "hide the old bar while bars41 is on" check in UiHud33.cs / Hud33Layout.cs / Boss37Kit.View.cs (the lead's lines).
- **Lesson25Tests.TheOrder_FollowsTheBlocks** forces N41Blocks off like N30-N40 (NIGHT40 fixed the same failure late, ba63ddeb).
- **The suite's batches proven:** one `bash Tools/unity-loop.sh tests` call with a two-class filter and its 600 s timeout, then
  Tools/night/n41-suite.sh with explicit filters by class prefix, each batch under 600 s (the Game namespace has ~142 files).
NIGHT41-BRIEF.md from NIGHT40-BRIEF.md (tape 24, the eight builders, the merge rule, builders cannot message each other: hand-offs go
through the lead). Launch at the shared commit's green.

**T2 the proof tooling (builder T2, ~150 min).** INVENTORY A1, A2, A9, A11, A14, C1's reel. Owns Game/FeelProbe.Record.cs (the frame record),
new FeelReel.N41.*.cs scenarios, the reel fixes.
- Per-frame telemetry: `fired` (the weapon id when a shot / volley / swing left this frame), `charge` (0-1 of the held weapon), `pump` (the
  viewmodel's pump phase 0-1 or -1), `spin` (the crosshair's turn 0-1), `device` (kbm / pad), written next to `vel` (FeelProbe.Record.cs
  :32, :190, :382). Additive: old readers ignore new fields.
- Scenarios: `weapons41` (in one room of a level that owns both guns: KESTREL taps at 0.5 s gaps, a full charge, a THUNDERCLAP volley, a
  ground slug; named frames at shot +0 / +0.15 / +0.30 / +0.45 s and the slug at release / +0.15 s); `doors41` (WITHOUT DOORFREE: face a
  locked doorway while its room's wave lives, then the open one, then the boss door; frames far and near); `entrance41` (each boss's room
  entered for real: frames at 0 / 0.15 / 0.3 / 0.6 / 0.9 s after bossRise); `pad41` (with -skygrindReelPad and PlayerDevices.Last = Pad:
  frames of every new NIGHT40 line: level 3's BOOST n/5, level 2's eight prompts, the three bosses' lines, OPEN / BLOCKED).
- Fixes: the DOORFREE boss reels silence room banners (A9: "4.2 FIRST GAP OPEN" over THE LOOM; "2.2 THE SHIELD OPEN" over THE ICEBREAKER);
  verbs row 13's press held for two steps (A11, L-039).
Proof: each scenario's frames exist with the telemetry fields filled; the lead names them in the log. The quick pump turns ON through
design.n41.pump41On after its frames (T2 owns that row). Every campaign boot is `reel.sh` + `-skygrindMap` + `-skygrindReelOnly` (L-062);
entrance41 runs one Player per boss (L-046).

**J2 the boss moment, doors, bars, the level select (builder J2, ~210 min).** INVENTORY B1, B2, C3, C4, C9. Owns Juice38 / Juice39Hurt /
UiJuice38, Doors38View, a new UiBossBar41, UiLevelSelect26's preview, design.n41.{juice41, bars41, entrance41, select41}.
- juice38 and doors38 ON (through juice41On / doors41On) after T2's frames and one iteration each (GG w37: the shake "when he first enters", the static "like a TV", the
  doors "a holographic barrier or a different colour"); colours: locked = red-orange (danger), open = cyan, the boss door = violet-white.
- The entrance beat (C4) for all five bosses: the name card, juice38's shake + flicker, and the boss's BossLine40 rule line in one 1.5 s
  beat, the same order every time.
- Boss bars (C3): a tick at each phase threshold and the stagger meter under the bar, all five bosses.
- The level select (C9): each slot shows the level's name, its new weapon and its boss's silhouette; slot 6 "REBUILDING", locked.
Proof: T2's door and entrance frames; LevelSelect29 PASS; a frame of each boss bar.

**K2 the levels' look (builder K2, ~180 min).** INVENTORY A3. Owns LightPlan33's light colour per theme, LightKit29.Named's theme
argument, UnityTuning.Look41.cs, ice / canal view props for n40-iceline (a view row, no level data), design.n41.look41.
- Light colour by theme (today every room light is "warm" 0xffe6c8 whatever the theme: LightPlan33.cs:361-373, 417-437): iceline cool
  (blue-white keys, pale cyan-free fill), foundry warm, kiln molten orange-amber, span dusk violet, spire high pale gold; never cyan
  (player-usable) or red-orange (danger) for room light.
- Level 2's canal: water / ice sheen and frost as view props on n40-iceline's floors and walls (L2's tint from NIGHT40 stays).
- A pixel check per level (the mean floor colour of three frames, before / after) logged as numbers.
Proof: frames of each level's first room, before / after; one iteration; look41 ON.

**B8 the bosses (builder B8, ~210 min).** INVENTORY A4, C6. Owns Loom37*.cs, SurvivalSim.N40.Loom.cs, Loom37View.N40.cs, Boss37.cs
(additive), Brawl40*.cs, their views and pilots, design.n41.{sweep41, brawlLook41}; fills WeakPoint41 for THE LOOM and THE ICEBREAKER.
- THE LOOM's floor sweep (A4): a new attack sweeping the canyon floor (a red-orange decal tell >= 0.9 s, boss_rules 1 in NIGHT40.md); the
  answer is height (a wall where riding is allowed, a ledge, a hook); in phase 2, when two thirds of the walls refuse a ride, it is the
  reason to hook. Both pilots 10/10, band 30-60 s bot; the verbs on n40-span 30/30.
- THE ICEBREAKER's look (C6): its body read pale tan like the walls (n40-r4 frames): ice-armour colours (pale blue-white plates, dark
  stone core), the fists glowing through their tells, a death that shatters into blocks (crunch's flying blocks); more poses for the slam
  and the rush.
Proof: the boss reels on the round builds; frames of the sweep's tell, the ICEBREAKER's look and its death.

**E6 the enemies (builder E6, ~210 min).** INVENTORY A5, A7, A8, C5, C7. Owns Core/HuskBrain.cs's token code (additive), Drone / Warden /
Charger.cs, EnemyViews*.cs / Enemy40View*.cs, Bot/Foes40Combat.cs, gen-n41-kiln.cjs + Design/levels/n41-kiln*, design.n41.{tokens41,
heavy41, models41, kiln41}.
- tokens41 (A5): a campaign cap across the pools: at most two enemies in a wind-up at once (tokens exist: melee 2, ranged 3, heavy 1, swarm
  3, growing per round; E3 saw up to 5 committed). Measure "> 2 winding up" share of fight time: 3.5 % -> 0; the walk 10/10 on every level.
- heavy41 (C7, through the pre-placed Heavy41 hook in a new Core/Heavy41.cs): the gauntlet on a big enemy (tank, WARDEN, CHARGER, THE ICEBREAKER) answers harder: a longer hitstop and a knock-back, so
  "the gauntlet excels against large enemies" (GG w40) is felt; numbers with a _why.
- models41 (A7, A8): the HUSK's own feature (the blind reader's weakest pair husk / spitter), re-tested blind (7/7, husk high confidence);
  a death break-apart for every kind's parts.
- kiln41 (C5): n41-kiln = n40-kiln + one CHARGER room on the kiln's open floor (the walk 10/10, the gates 4/4). The level-5 DRONE room
  is R5's file: E6 puts the placement in its report and the lead passes it to R5 (builders cannot message each other).

**P3 the pad (builder P3, ~180 min).** INVENTORY C1, C2, C10. Owns Core/AimAssist.cs (additive, behind aim41), the device resolver for
`{tokens}` (UiLesson25 / UiTeach35's resolve paths, Core/BossLine40.cs's resolve), THE CROWN's WeakPoint41 (SurvivalSim.Crown37*.cs /
SurvivalSim.N40.Crown.cs for that only), design.n41.aim41; any line's TEXT change goes in an n41 override row, never the n40 row.
- aim41 (C2): aim assist finds a boss's weak point: the CYAN core in a window, THE LOOM's lit knee, THE CROWN's ring while it is passable,
  THE ICEBREAKER's stuck fist; centre mass otherwise. The harness: hits per second on each weak point with the pad pilot, before / after.
- C1: from T2's pad41 frames, every NIGHT40 line with pad glyphs; fix any line that shows keyboard names on a pad.
- C10: the arc under the crosshair and the aim-assist's reticle on a pad do not overlap (a frame).

**R5 the levels (builder R5, ~180 min).** INVENTORY A10, A12. Owns gen-n41-spire.cjs + Design/levels/n41-spire*, gen-n41-foundry.cjs +
Design/levels/n41-foundry*, design.n41.{spire41, foundry41}.
- spire41 (A10): grapple nodes on level 5 where they serve the descent (one per descent room at least), E6's DRONE room, the gates
  re-run (L-061: the checker's grapple move), the walk 10/10; verbs rows 17 / 19 / 22 / 23 PASS on level 5 in the Player.
- foundry41 (A12, DECIDE41 D-2): a no-ride band on THE GANTRY's long b2-g1 wall so the chain of walls is needed again under the endless
  ride; the gates (b2-g1 back to "the chain") and the walk 10/10. If GG keeps the single ride, foundry41 stays OFF.

**L6 level 6's plan (builder L6, ~180 min).** INVENTORY A13. Owns Design/levels/n41/finale.beats.md, gen-n41-finale.cjs (a greybox),
Research/2026-10-14-wave41/RIVAL.md (a one-page boss proposal), design.n41.finale41 (OFF tonight).
- A beat sheet for a new level 6 (the exam: every verb from levels 1-5 in rooms that combine them, small and action-packed, a climb route for
  enemies in every room, PRINCIPLES 4), a greybox of it (walkable, the walk bot 10/10 through the rooms built), and THE RIVAL as a proposal
  (GG owns enemy design: a page, no code). finale41 stays OFF and level 6's slot stays locked: GG reacts first (DECIDE41 D-3).

**Lead L0.** G0; integration; builds per round; every reel; the frames; the verbs before / after. **The suite (A6):** run it IN the live
Editor in namespace batches (`bash Tools/unity-loop.sh tests <Namespace>` per namespace: Skygrind.Core.Tests, then Skygrind.Game.Tests
split by class lists of ~200), no second Unity process; log every batch's totals; a batch failing with no message is re-run once alone.
**The bench (C8):** `-skygrindBench 60` on n40-iceline (a survival horde on that map: what the bench measures), alone (L-005), models41
on and off, three runs each (6, the cap), medians.
</cards>

<builders>
Eight builders in worktrees from the shared commit (NIGHT40 ran nine). Rules: NIGHT41-BRIEF.md (NIGHT40-BRIEF's, updated): no `git merge`
(the lead cherry-picks), `git show port/stage3:<path>` to read; dn-check-wt (tested with a plant by the lead); fingerprint and `--off41`
EQUAL before each sim commit; golden before each Core commit; never the Editor, a build or a Player; config scripts patch only their row;
reports under 300 words naming the frames to take.

| Builder | Owns |
| --- | --- |
| **T2** proof tooling | FeelProbe.Record.cs, FeelReel.N41.*.cs, the DOORFREE banner silence, row 13's press, design.n41.pump41On |
| **J2** boss moment / doors / bars / select | Juice38*, UiJuice38, Doors38View, UiBossBar41, UiLevelSelect26 preview, design.n41.{juice41,juice41On,doors41On,bars41,entrance41,select41} |
| **K2** level look | LightPlan33 / LightKit29 theme colour, UnityTuning.Look41.cs, the iceline view props, design.n41.look41 |
| **B8** bosses | Loom37*.cs, SurvivalSim.N40.Loom.cs, Loom37View.N40.cs, Boss37.cs (additive), Brawl40*.cs + views + pilots, design.n41.{sweep41,brawlLook41} |
| **E6** enemies | HuskBrain tokens (additive), Core/Heavy41.cs, Drone / Warden / Charger.cs, EnemyViews* / Enemy40View*, Foes40Combat, n41-kiln, design.n41.{tokens41,heavy41,models41,kiln41} |
| **P3** pad | Core/AimAssist.cs (additive), the {token} resolvers, THE CROWN's WeakPoint41, design.n41.aim41 |
| **R5** levels | n41-spire, n41-foundry (generators + files), design.n41.{spire41,foundry41} |
| **L6** level 6 plan | Design/levels/n41/finale.beats.md, gen-n41-finale.cjs, RIVAL.md, design.n41.finale41 (OFF) |

**Keep first if short:** the telemetry + pump40 ON, juice38 + doors38 ON, the ice look, the token cap, the LOOM sweep, the suite to the end.
**Cut in this order:** the level select previews, the bench's second level, models41's death break-apart, kiln41, the RIVAL page.
**The spend limit (L-003):** commit every slice; on 429s the lead logs the time, integrates and builds alone, retries a builder at most
every 30 minutes.
</builders>

<lean_rules>
- Golden at every Core commit; Mover.cs untouched tonight (no card names it); JsMath; Mulberry32 seeded per run; whole-step timers.
- Numbers: gameplay in design.n41.<row> with a `_why`; render / UI in UnityTuning.*41.cs with comments; never edit n35-n40 rows or files
  in place.
- Caps (report short instead): builds 7; reels 50 by round (round 1: 10, rounds 2-3: 8 each, round 4: 16, retakes 8; K2's before / after
  level frames come from the verbs reels, not their own); the suite: the Editor batches (no batch Unity unless > 12 GB free); bench 6 runs, alone;
  builders 8; no build or reel under 10 GB free disk.
- Commits with explicit paths; never GG's four files (L-047); nothing deleted but the lead's own reel frames; no audio (event names only).
- Docs with Write / Edit only; every lead message ends with the status line.
</lean_rules>

<tasks>
## U3-54.1 Start + G0 (0:00-0:30)
`date`; GG's words first; G0; NIGHT41-LOG.md; launch the eight at the shared commit's green.
## U3-54.2 Build rounds (0:30-4:20): integrate at about 1:30, 2:30, 3:30
Round 1: T2's telemetry + weapons41 / doors41 / entrance41 / pad41 (the first build runs them on NIGHT40's rows: the frames that turn
pump40, juice38, doors38 on), K2's theme colour, E6's token cap measured, B8's sweep in the harness, P3's weak-point aim in the harness,
R5's level-5 nodes, L6's beat sheet. Round 2: every row's first frames; iterations. Round 3: rows ON after frames.
## U3-54.3 Round 4 in the Player (4:20-5:00)
The verbs on levels 1-5 (the after; level 5's grapple rows PASS); the five boss reels; lineup (models41); pad41; LevelSelect29; the smoke.
## U3-54.4 Test hour (5:00-5:45)
The suite in Editor batches to the end; golden; fingerprint `--off41` EQUAL; the bench; NIGHT41-PROOF.md.
## U3-54.5 Close (5:45-6:00)
NIGHT41-LOG close table (G0, T2, J2, K2, B8, E6, P3, R5, L6, L0: DONE or SHORT with a measured reason; and every INVENTORY row's fate);
NIGHT41-REPORT.md (GG's page: each INVENTORY row ON / OFF with how it was checked); FEEL-PLAYTEST; DECIDE41; HANDOFF; PORT-STATUS events;
tag u3-n41.
</tasks>

<leftovers>
1. Any card that closes SHORT (it carries with its reason).
2. Level 6's build from the plan, once GG has reacted (D-3); THE RIVAL's fight.
3. Survival mode with the new enemies and the arc; new weapons.
</leftovers>

<closing_reminder>
- A row is ON only after its frames were looked at and iterated once; built but unseen ships OFF with its reason (NIGHT40's pump40).
- New level ids go into design.maps.list in the shared commit; `--off41` is NIGHT40's game.
- Every lead message ends with `N41-STATUS: round <r> · builders <alive>/8 · rows on <n>/<total> · INVENTORY closed <k>/28 · <h:mm> / 6:00`.
</closing_reminder>

# NIGHT46.md: the flow night: the player never stops for the game's sake. A flow meter is the scoreboard; the code's five worst flow breaks fixed as rows; level 4 and level 5's arenas rebuilt from identity cards AND flow plans; levels 1-3 smoothed in data; THE KILN WYRM walked into and cut to the band; feel measured, then fixed on the frames (lead + six builders; 5 h 45 min hard cap)

GG, 2026-10-17 (after NIGHT45), verbatim:
> what should night prompt for 5-6 hours focus on, think hard, do good resrecah and i will cut storage a bit but there 15+ gb so it
> should be fine, you dicide answers for me, i wont be able to test so i trust you, again follow best methods and study real game deisgn
> and areana layouts, need to feel and flow better

**GG will not test tonight. Every call is already made: DECIDE46.md (read it whole; it answers DECIDE43-45 and the RETIRED "GG decides"
rows). If GG writes during the night, his words come first.** Base: port/stage3 at `u3-n45` (041c2cf0). t0 = GG's go. The lead writes
`date` on every log line and on the close table. The play build stays `Builds/n45-r4` unless a new build passes verbs and the smoke.

## Where we are
- NIGHT45 (2 h) rebuilt b1 THE SPILL, g7 THE THAW LOCK and k5 THE KILN'S HEART from identity cards (DESIGN-RULES A-9), all ON; the whole
  suite passed (Core 1114 / 1114, Game MapP-Z 389 / 389); n45-r4 passed verbs37 on levels 1-5 and the smoke. GG has played neither n44
  nor n45. Rows as data (Core/NightRows.cs) is built and unwired; tonight's G0 switches it on (DECIDE46 D-4).
- **The flow audit** (Research/2026-10-17-wave46/FLOW-AUDIT.md, read it whole): the code never freezes time, so flow is lost elsewhere.
  Its ranked breaks: (1) a death restarts the whole room from wave 1 (b1's bot 10 deaths / 10 walks, max 263 s); (2) k5 is the only boss
  that teleports, velocity 0 (Campaign24.cs:672-676; GG w35 "you just teleport into these random rooms"); (3) level 4's void falls send you
  to the link's start for 25 % HP, five doors open onto the void 6-8 m ahead; (4) teaching holds up to 20 s with nothing to fight (a2, a8);
  (5) 29 wave gaps of ~1 s, then a >= 12 m walk to the new bodies; (6) p1 / p3 / p5 / v2 / v6 / kb2c: 840-1600 m2 floors, zero designed
  ride walls, no circuit, sightlines 47-55 m; (7) three level-3 doors open onto a 12 m cliff; (8) boss reveal / phase windows 1-2 s each;
  (9) long fight-free stretches (L1 b2 104 m; L5's four identical belfry gates); (10) linear rooms a3, g4; (11) doors held by bodies that
  left the room (harness); (12) wave 1 out of sight from the door (b1, v6, kb2, g8).
- **The research** (read both whole before your card): Research/2026-10-17-wave46/arena-flow-web.md (28 findings; id's GDC 2018 "Embracing
  Push Forward Combat in DOOM" read in full; Part D is an 11-point checklist for a ~30 x 30 m wave arena) and feel-flow-web.md (32
  findings; F32 is the stall / flow metric set). Older: wave-45 arena-identity.md (A-9), wave-43 encounters-arenas-web.md, wave-42
  arenas-web.md. The points tonight is built on:
  - **Small and dense beats big** (id: speed comes "from the speed at which you pass geo"; "epic fights [don't need] epically sized arenas").
    A fight floor over ~900 m2 needs a reason on its card.
  - **No single round loop** (id: round arenas "made everyone circle"): a ground strafe loop round the landmark AND an air loop (rides /
    rails / grapple perches) that never touches the floor, broken by one one-way drop; each node offers exactly two outs.
  - **Half walls**: cover is jump-height blocks one jump mantles; no decorative props in the fight space.
  - **Commit, don't doorway-camp**: the entry is a lip that shows the arena and wave 1, then a drop in; the first foothold 5-10 m inside.
    id's four failure signs (retreat, doorway waiting, sniping, kiting) are measured by the meter (kiting is allowed: DECIDE46 N-8).
  - **Waves keyed to the fight, not the last kill** (Doom; ULTRAKILL's Cyber Grind holds dangerous enemies to 33 / 50 / 75 % of a wave dead);
    ranged enemies stand in the open on the far side; one captain per wave across from the entry; never two chaser types at once.
  - **Change the arena between waves, never mid-wave** (Titanfall 2 cut its collapsing floors: players "didn't understand").
  - **Read in one frame**: simple geometry, one colour per usable thing, the next route in view on arrival, one landmark from every tier.
  - **Feel numbers**: coyote 0.1 s (Celeste's source), hitstop 10-20 ms per hit stacked with flash + kick (Nuclear Throne), trauma-squared
    shake by rotation only in 3D (Eiserloh GDC 2016), L4D's build / peak 3-5 s / fade / relax 30-45 s, Griesemer's 3 s / 30 s / 3 min.
    Movement is exact and stays (CLAUDE.md rule 3; DECIDE46 N-5): feel work is camera, feedback, timing of view, and the arena.

## The mistakes this night is built not to repeat (LESSONS; NIGHT40-45 logs)
| mistake (nights) | tonight's guard |
|---|---|
| the spend limit stopped every agent (w19, w20, w42, w43) | every slice committed; a close skeleton + a HANDOFF note at each build; the cut order below; on a 429 the lead goes on alone with the lead-alone plan (the builds, the ON calls, the close) |
| the shared commit cost 15-20 min and touched 39-52 files (w42, w43) | G0 = rows-switch-on.cjs + `Tools/night/n46-shared.cjs`, both dry-run twice by the forge (Research/2026-10-17-wave46/FORGE.md: the exact sequence and its expected lines) |
| arenas placed piece by piece without a reason (w36, w43; GG w35 "random lava pits random pillars") | A-9 card + the flow plan (Part D's 11 points answered) committed first; **the lead's card gate at ~0:55** before any geometry |
| rows ON on harness numbers; feel "implemented + tested" (w17, w43) | ON only with the meter's numbers AND Player frames looked at; feel only by blind pairs (DECIDE46 N-4) |
| a new row broke an old verb indirectly (w43) | verbs37 on levels 1-5 after the rows go ON, before the smoke; `--off46` = NIGHT45's game |
| a level swap dropped a system that checked the id (NIGHT45 forge) | every id check asks `N45Levels.BaseId` (it asks `NightRows.BaseId` first after the switch-on) |
| the suite not run before the freeze (w42-w44) | P0 runs it after the switch-on (the tape format moves 26 -> 27) and again on the freeze's Game batches N3 / N4 / NightRows / Lesson25 |
| a builder's harness pilot read as the game (T-59, T-66, T-75) | the meter runs ONE pilot for every room (F1's), stated in each number; a pilot fault is logged as the pilot's |
| the smoke failed for the machine (w42, w44, T-73) | nobody at the machine during the smoke; the pad unplugged if GG can (tell him at the go); the smoke runs once more if only pad steps fail |
| disk low (w43, w44; 14 GB at the forge) | `df` at G0 and before each build; numbered frames deleted after each verdict; under 12 GB DECIDE46 D-1's frame cut; under 6 GB no new reels |
| the memory harvest skipped (w40-w44) / THREADS over its cap (86 lines) | the lead runs `wave-memory` at the close, consolidating THREADS |

<method>
1. **Clock (hard cap 5:45 by `date`; report SHORT instead of running over):**
   | time | the lead | builders |
   |---|---|---|
   | 0:00-0:20 | G0 | — |
   | 0:20-0:55 | P0 the suite | cards + flow plans (A1, A2, A3, B1); F1's meter; V1's feel audit |
   | ~0:55 | **the card gate** (10 min, below) | geometry starts only after it |
   | ~1:15 | F1's baseline table to every builder | |
   | ~1:50 | integrate + build r1 (rows OFF; frames with rows ON in the build's copy) | round 1 ends at its report |
   | ~3:05 | integrate + build r2 | round 2 |
   | ~4:15 | integrate + build r3 (the candidate) | fixes from the Player only |
   | 4:15-5:05 | the ON calls from r3's meter + frames, `n46-rows-on.cjs` in the repo, verbs37 levels 1-5, the smoke | |
   | 5:05 | freeze | |
   | 5:05-5:45 | close | |
   Each build: `bash Tools/night/n41-build.sh n46-rK`; rows ON in its copy with `node Tools/night/n39-build-rows.cjs n46-rK on n46.<row>`;
   reels with `bash Tools/night/n46-reels.sh` (copy n45-reels.sh with NIGHT=n46, at G0).
2. **Rows as data from tonight** (docs/NIGHT-ROWS.md): sim rows `design.n46.<row>` with `"sim": true`; a builder's code reads
   `rows.On(idXxx46)`; harness / reels force with `sim.rowsOverride = sim.rowsRegistry.Defaults().Set("<row>", true)`; off = `--off46` /
   `-skygrindOff46`. No N46Blocks, no tape bump, no older shared test edited. At the freeze: `RowsBaseline.OnPathThrough = 46` and the
   baseline re-recorded, its own commit.
3. **The flow meter is the scoreboard** (F1): `n46flow` in the SimHarness, one command per level, one pilot for every room, the same seeds
   before and after. A row or an arena turns ON only if: its gates are GREEN, the walk clears its rooms 10 / 10, its target metric improves
   by the margin its card states, no other metric of F1's table gets worse by more than F1's noise band (measured: the same build twice),
   and the lead looked at its Player frames (entrance, mid-fight, greyscale; for feel, a blind pair).
4. **Identity AND flow before geometry** (A-9 + tonight): each arena builder commits, for each room, the A-9 card under its heading in
   docs/ROOMS.md's new section "NIGHT46 identity cards", **plus a flow plan**: Part D's 11 points answered one line each with numbers (floor
   m2, tiers, the ground loop and the air loop as node lists with their two outs, the drop in, the first foothold distance, wave 1's nodes
   in view from the lip, where waves 2-3 enter relative to the player's heading, the hazard's job, the meter target it promises). Then the
   generator derives each solid from the card (its `why` names the card field and the flow plan line).
5. **The card gate (the lead, ~0:55, 10 min):** for each card: a place, not a function; one spatial law; Part D 1-11 answered with numbers;
   no single round loop; floor <= ~900 m2 or a reason; wave 1 visible from the lip; nothing placed "for flavour" inside the fight space.
   The lead answers each builder PASS or the one line to fix (SendMessage). Geometry before PASS is not integrated.
6. **Seen, not assumed (L-001):** every ON decision names its frames (file names) and its meter numbers. Builders never run the Editor or a
   Player; the lead cherry-picks (`bash Tools/night/integrate.sh`), runs every reel, the suite and git; gates + the duplicate-key check after
   every pick. Builder rules: C:\Users\gaige\dev\SKYGRIND-unity\NIGHT46-BRIEF.md (absolute path: untracked).
</method>

<cards>
**G0 (the lead, 0:00-0:20).** `date`; `df` (tell GG the number at the go); the Editor stopped (status); GG's words if any. Then
Research/2026-10-17-wave46/FORGE.md's sequence exactly: `node Tools/night/rows-switch-on.cjs --dry` then without `--dry`, its gates, the
commit "NIGHT46 rows switch-on" (19 files); `node Tools/night/n46-shared.cjs` (32 files this once: the rows, the hooks, the reel phases, four
level-id checks routed through BaseId, NightRowsTests' "0 sim rows" pin relaxed; the forge's two dry runs were byte-identical), its gates (dn-check 0, golden 0 / 22, fingerprint EQUAL
plain / `--off45` / `--off46`, `n45rows` 0 problems listing the n46 rows, `cmp` of the two configs, duplicate keys 0), the Editor compile
(force an import of each new file: L-067), EditMode `N46Shared`, `NightRows`, `N3`, `N4`, `Lesson25`; commit "NIGHT46 shared". Copy
n45-gates.sh -> n46-gates.sh (`--off46`), n45-reels.sh -> n46-reels.sh, n45-rows-on.cjs -> n46-rows-on.cjs; integrate.sh's ID map (six
builders, Logs/night46-picked.txt). Launch the builders.

**P0 the suite (the lead, 0:20-0:55).** After the switch-on the tape format is 27: `bash Tools/night/n42-suite.sh n46 Core`, then the Game
batches by NIGHT45's names (Logs/n42-suite-n45g.txt lists them). Fix a switch-on failure at once (it blocks everything); list others.
Then CLAUDE.md's two stale lines (DECIDE46: DECIDE44 D-4), their own commit.

**F1 the flow meter, then the flow rules (builder F1).** FLOW-AUDIT rows 1, 4, 5, 8; feel-flow-web F32; arena-flow-web F16.
- Round 1 (by ~1:15): `Tools/SimHarness/Harness.N46.Flow.cs`: `n46flow --level <id> [--seeds 10] [--off46] [--rows a,b]` runs F1's one
  pilot through every fight room of the level and prints per room and per level: stall share (in-combat steps under 25 % of walk speed),
  longest stall, airtime share, kill gap (median / max), nearest-enemy distance (median / p10 / p90), doorway dwell (s within 4 m of the
  entry door while enemies live), dead-time seconds (no enemy alive and no door open), deaths, retries, clear time; plus FLOW-AUDIT's
  geometry numbers ported from its scratch rasteriser (floor m2, tiers, central loop y / n, pockets, longest sightline, wave-1 nodes in view
  from the entry). The noise band: the same build twice. Write `Logs/feel/n46-flow/baseline.txt` (levels 1-5 as n45-r4 plays them) and the
  table into Research/2026-10-17-wave46/F1.md; the lead sends it to every builder.
- Round 2: the sim rows `retry46` (restart at the wave you died in: the won waves stay won; DECIDE46 N-6), `overlap46` (the next wave
  releases when 1-2 bodies remain, through the pre-placed hook; N-7), `teach46` (holds capped ~8 s; N-11), `reveal46` (boss reveal / phase
  windows ~1 s; FLOW-AUDIT row 8). The Campaign24 hooks are delegates (`retryWave46`, `waveRelease46`, `teachHold46`) set in
  `N46Campaign()` (FORGE.md). reveal46 has no pre-placed hook: the lead's ruling is that F1 may add ONE row-gated read per boss module
  (Lantern36, Crown37, Stag37, Brawl40, Boss37 ...: `if (reveal46On) seconds = n46 value`), nothing else in those files, golden 0 / 22
  and fingerprint EQUAL with the row off; it is last in F1's order and first in the cut order. Each: its numbers in its own
  `design.n46.<row>` object, its meter before / after on every level it touches, gates, `--off46` EQUAL. VERBS.md is untouched by these.

**A1 level 4's arenas (builder A1).** FLOW-AUDIT rows 3, 6. `n46-span` (gen-n46-span.cjs from gen-n40-span.cjs: the plan copied, only the
named rooms rebuilt). Cards + flow plans first for p1 HOOK YARD, p3 THE TIE-OFF (NIGHT45's card exists: re-check it against Part D) and
p5 SPOOL HALL; each a different spatial law (level 4's verb is the grapple: pull, kills give pulls, the whip). Every void gap the player
crosses gets a lip return (`fallReturn43`-style if Campaign24's reader serves rooms with `respawnY`: read Campaign24.cs:999-1029 first; else
start each void 10-12 m past the door and say so). Proof: `n46flow --level n46-span` vs the baseline (targets: stall share and longest stall
down, doorway dwell down, floors <= ~900 m2 or a card reason, a central loop y in each), the walk 10 / 10, levelgen 0 errors; reel phase
`a146` (Game/FeelReel.N46.Span.cs): each room's entrance from the lip and mid-fight. Order if short: p3, p1, p5.

**A2 level 5's arenas (builder A2).** FLOW-AUDIT rows 6, 9, 12. `n46-spire` (gen-n46-spire.cjs from the n41-spire generator). Cards + flow
plans for v2 THE NAVE and v6 THE CHOIR (level 5's verbs: the jump-dash, the TORCH, the parry against drones; GG's "rooms that incentivize
mechanics"): the aisles / heights as the air loop, wave 1 on the dais in view, the "heavy" given a real role or cut. Then the belfry gates
v3-v5 / v7: merge the identical pistol-only gates into fewer, each with its own job (FLOW-AUDIT row 9). Proof: `n46flow --level n46-spire`,
the walk 10 / 10, levelgen 0; reel phase `a246` (FeelReel.N46.Spire.cs). Order if short: v2, v6, the gates.

**A3 levels 1 and 2 smoothed (builder A3).** FLOW-AUDIT rows 4 (the data half), 9, 10, 12. `n46-foundry` (from gen-n45-foundry.cjs) and
`n46-iceline` (from gen-n45-iceline.cjs). Data only, every change named by its audit row: a3's lava 2 m short of the west wall (a walk
round both sides); b1's wave 1 at the bridges' far ends in view from the lip; a8's lesson perch on the first ride wall's end; a small fight
in b2 (the 104 m lava gantry: a few husks on its lip; card line first); g4's second crossing; g8's charger nodes in view. Then g3 THE
SWARM's card + flow plan + rebuild (NIGHT45's SHORT). Proof: `n46flow --level n46-foundry` / `n46-iceline` vs the baseline, the walk 10 /
10 on each level (Walk38Pilot: say so where it does not avoid lava, T-75), levelgen 0; reel phase `a346` (FeelReel.N46.Smooth.cs).

**B1 THE KILN WYRM: walked into, and in the band (builder B1).** FLOW-AUDIT rows 2, 7; T-72; DECIDE46 N-1, N-10. Owns `n46-kiln` and the
worm. (1) `walkin46`: the fight is entered on foot with your speed kept: either k5 rebuilt in n46-kiln at the worm arena's size (the
wyrm45 arena's plates, crumble and KILN MOUTH kept, A-9's card from NIGHT45 kept) so `BossWalkIn` holds, or velocity carried through the
respawn with the arena seen through the door; the card says which and why. (2) `wyrm46`: the worm's damage windows / stagger (DECIDE39 D-1:
stagger / legs before health), not its HP alone: bot median 30-60 s on 10 seeds (today 134-186 s, median 161 s), bot damage taken > 0 on
10 / 10 (it stays a threat), the crumble still falls at half. (3) n46-kiln's three cliff doors (kb1b / kb2b / kb3b): a ride wall or run-up
into each climb. Proof: Harness.N46.Wyrm.cs (`--level n46-kiln`), `n46flow --level n46-kiln`, the walk 10 / 10; reel phase `a446`
(FeelReel.N46.Wyrm.cs): the k5 door, the step in, mid-fight, the crumble, the kill.

**V1 feel, measured then fixed (builder V1).** feel-flow-web F1-F31. View only (no sim row; Mover.cs untouched).
- Round 1, the audit (by ~1:15), from code and the headless view math: per verb (VERBS.md rows) the input-to-view latency in steps (the
  first frame the screen changes), and what each kill / hit / landing / ride / dash changes on screen today (hitstop steps, flash, kick,
  shake model: trauma or flat, rotation or translation), against the research numbers. A table in Research/2026-10-17-wave46/V1.md: the
  five biggest gaps, each with the frame pair that would show it.
- Round 2: the view row `feel46` fixing the top gaps (expected from the research: shake as trauma-squared rotation only; hit flash + kick
  stacked on the hitstop that exists; a landing dip and FOV kick on speed if missing; an off-screen attacker cue: red-orange edge only, never
  the whole screen, GG w30); numbers in `UnityTuning.Feel46.cs`, each with a comment. Proof: blind pairs (the same reel moment, `feel46` off /
  on, labels hidden: the lead and one reviewer agent pick the stronger frame / 2 s clip; ON if both pick on in >= 4 of 5 pairs) and the
  latency table before / after. Also frame `aim41` (a pad frame pair: the lock on a weak point) and `select41` (the level select); ON only
  if the frame reads (DECIDE46).
</cards>

<builders>
| Builder | Owns |
| --- | --- |
| **F1** | Harness.N46.Flow.cs, SurvivalSim.N46.Flow.cs (retry46 / overlap46 / teach46 / reveal46), their config rows, Research/.../F1.md, Logs/feel/n46-flow/ |
| **A1** | gen-n46-span.cjs, Design/levels/n46-span* + plan, design.n46.span46, ROOMS.md's p1 / p3 / p5 cards, FeelReel.N46.Span.cs, Harness.N46.Span.cs |
| **A2** | gen-n46-spire.cjs, Design/levels/n46-spire* + plan, design.n46.spire46, ROOMS.md's v2 / v6 / gate cards, FeelReel.N46.Spire.cs, Harness.N46.Spire.cs |
| **A3** | gen-n46-foundry.cjs, gen-n46-iceline.cjs, Design/levels/n46-foundry* / n46-iceline*, design.n46.foundry46 / iceline46, ROOMS.md's g3 card + changed rows, FeelReel.N46.Smooth.cs, Harness.N46.Smooth.cs |
| **B1** | gen-n46-kiln.cjs, Design/levels/n46-kiln*, any wyrm46 arena map + generator, design.n46.kiln46 / wyrm46 / walkin46, SurvivalSim.N46.Wyrm.cs, FeelReel.N46.Wyrm.cs, Harness.N46.Wyrm.cs |
| **V1** | Game/Feel46*.cs, UnityTuning.Feel46.cs, design.n46.feel46, Research/.../V1.md, FeelReel.N46.Feel.cs |
| the lead | G0, P0, the card gate, integration, every Player check, the blind pairs, git, DECIDE46, the report, the memory |
**Keep first if short:** G0 + P0, F1's meter, B1, A1, F1's rules, A2, A3, V1's audit, V1's fixes. **Cut in this order:** g3, the belfry
gates, p5, V1's `select41`, `reveal46`. **On a 429 (the spend limit):** stop V1 and A3 first; the lead finishes alone (build, ON calls, close).
**Leftovers (approved, in order):** T-66 move43's pilot per sim step; T-59 THE WICK's reel to the kill; look45 in a room whose pylons are
in view (T-74); the light at a state change (`shift46`: reads the crumble / wave-start / room-clear events, fixtures named on the cards).
</builders>

<close>
Close table (with `date`), one row per card (G0, P0, the card gate, F1 meter, F1 rules, A1, A2, A3, B1, V1, verbs 1-5, the smoke,
memory), each DONE or SHORT with its measured reason. **The flow table**: per level, stall share / longest stall / doorway dwell / dead time
/ deaths / clear time, baseline (n45-r4's game) vs tonight's ON game, same pilot and seeds. NIGHT46-REPORT.md for GG: outcome first; what
changed for a player in one line per change (no row names); each rebuilt arena as its card's title, place fantasy and tell-a-friend line
with its entrance frame's file name; the flow table; what was not verified. HANDOFF.md; THREADS (consolidated under its cap, close only what
a run or a frame shows: T-72 if the band is met); `wave-memory` ledger 46 + CORE; night-context close.
Closing reminder: verbs on every level after rows go ON; a duplicate-key check after every pick; `git diff` before restoring Play mode's
font / TMP rewrites (L-047); delete your own numbered frames after each verdict; Players closed by PID.
</close>

/goal (paste after GG's go):
NIGHT46 is met when NIGHT46-LOG.md's close table and flow table are printed with every row DONE or SHORT (each with its measured reason and
`date`), or when 5 h 45 min have passed since t0 by `date` (then print both tables with what exists; the play build stays n45-r4 unless a new
build passed verbs and the smoke). Work NIGHT46.md in order: G0 (the switch-on and n46-shared.cjs) and the builders, P0 the suite, the card
gate, builds at ~1:50, ~3:05 and ~4:15, the ON calls, verbs37 on levels 1-5 and the smoke, the close. Never stop on a summary that
announces the next step, an offer to continue, or a list of decisions that block nothing; stop only when the tables are printed or the cap
is reached.

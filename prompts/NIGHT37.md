# NIGHT37.md: the boss night: four bosses restarted from scratch (lead + five builders, about 4 hours)

GG, 2026-10-12 ~19:25 (verbatim and a row map: Research/2026-10-12-wave37/GG-REQUEST.md): "I want all the bosses completely overhauled
and restart from scratch except the worm boss ... all the boss arenas and all the bosses their [attacks] their AI everything completely
restarted ... four unique boss fights that will in fact use the necessary mechanics of each level and their weapon ... actually good
arenas [that] encourage the gameplay movement ... all of them don't have to be a boss fight in a room like this is a movement game like
it should be a consistently moving ... battle ... you're consistently just wall riding jumping across platforming as you're killing this
boss ... get creative study game design let's not do it traditional".

GG decided (DECIDE37.md, ~19:35): **all four bosses tonight** (D-2; the lead's recommendation was one per night, so this night is built
to make breadth survivable: one builder per boss owns it end to end, a kit builder lands the shared brain first, a 4 h box); level 6's
finale will be THE RIVAL (D-1, not tonight). THE KILN WYRM is untouched.

Base: port/stage3 at u3-n36 (4da8cbce + the wave-37 research 040a5b41). Read first: CLAUDE.md, docs/PRINCIPLES.md, docs/VERBS.md,
HANDOFF.md, NIGHT36-REPORT.md, and Research/2026-10-12-wave37/: **BOSSES.md (the design: read all of it)**, research/uk-bosses.md
(ULTRAKILL's boss AI, numbers only), research/boss-web.md (patterns + GitHub, URLs), DECIDE37.md. Also Research/2026-10-12-wave36/
PLAN.md section 4 and Core/Boss36.cs (tonight's starting shape). t0 = the moment GG says go. Box 4:00, hard cap 4:20.

<goal_for_the_night>
GG plays each boss and never stands still. In level 1 he climbs a furnace shaft away from rising slag, leaping wall to wall, parrying the
WICK's embers back into its cage and piercing its core from the walls, then punches it into the slag from the cap. In level 2 he chases
THE STAG around a frozen canal, rides its slipstream, dodges its kicked shards by changing lanes, catches it and punches its legs, jumps
the breaking floes, and parries its head-on charge into the tunnel wall. In level 4 there is no floor: he crosses a canyon on the
grapple while THE LOOM cuts the threads behind him, nails its knees and tears them out by pulling away. In level 5 he falls down a
breaking spire with THE CROWN, jump-dashing through its ring to burn its gems. Every fight reads: yellow parry, red dodge, cyan your
window. Each takes a bot 15-22 s and a person about 40 s. Nothing he had is gone.
</goal_for_the_night>

<method>
1. **One builder per boss, end to end.** Each boss builder owns its arena, its two rehearsal rooms, the boss's sim and view, its bot
   pilot, its harness and its reel, and looks at its own frames through the lead. No boss waits on another boss.
2. **The kit first.** Builder K lands the shared Boss37 brain (BOSSES.md "Shared rules" 1-8) by 1:00; boss builders build arenas,
   bodies and engine pieces until then, against a stub interface K publishes in its first commit (by 0:30).
3. **Eyes close a card** (L-001, NIGHT36 method 2): a boss is DONE only on Player frames the lead looked at (the walk-in, each tell, a
   stagger break, each leg's arena change, the kill) + its harness band + one iteration on what the frames showed.
4. **Nothing lost** (PRINCIPLES 1): verbs36 on every level whose boss changed, one Player per map, at the freeze. docs/VERBS.md gains
   the boss verbs (the climb, the slipstream, the thread tear, the ring pass) as rows 44-47.
5. **Reference first:** every number starts from BOSSES.md / uk-bosses.md with a `_why`; deltas logged.
</method>

<cards>
**K1 the Boss37 kit (builder K, ~100 min).** Core/Boss37.cs, evolving Core/Boss36.cs (Boss36 stays for nothing: the four bosses move
to Boss37; Lantern36 is retired behind its row): the 3-leg structure with leg checkpoints (D-6: a fall restarts the leg, keeps the
boss's progress, costs health), the STAGGER meter + the CYAN window (D-5; Minos Prime: stamina 6 / 12), the attack budget with a forced
2 s rest and the parry refund of 5 (MinosPrime.cs:342-353, :1125), the weighted pick with pity +0.25 and no repeats, distance bands
(Gabriel.cs:74-128), the habit reader (Minotaur.cs:294-296: ground vs air over ~8 s), the anti-hide timer (3 s; GabrielBase.cs:355-363),
Tells36 for every wind-up (YELLOW / RED; CYAN for the window). A stub interface in K's first commit (by 0:30). Harness `boss37 kit`
(each rule in isolation, with numbers). Also the shared view helpers: the CYAN window glow, the stagger bar under the boss bar.

**W1 THE WICK (builder W, ~200 min).** BOSSES.md "L1 THE WICK". Replaces THE LANTERN at the end of n36-foundry: L's generator
(gen-n36-foundry.cjs, written whole last night) gains the two rehearsal rooms (a short spiral climb, a pierce-from-the-wall target) and
the 40 m stack (spiral ride panels 3 per turn, 6 m rise per turn, ledge rings at 8 m and 22 m, the cap's ring catwalk with one long
ring panel). Engine: the rising slag hazard plane (0.6 m/s while it attacks, still at rest, 30 damage + a bounce, never instant death).
The Wick: a cage of 6 plates around a flame core; ember volley (YELLOW), flare sweep (RED, the half of the shaft you ride), plate slam
(RED, ledge > 3 s), draft; damage = a charged KESTREL pierce from a wall ride through a cracked plate; leg 3 on the cap, the dive and
the launch-punch finish. Pilot: a climbing bot. Reel `wick37`.

**S1 THE STAG (builder S, ~200 min).** BOSSES.md "L2 THE STAG". A new arena after level 2's last room: the frozen canal loop (~260 m,
three 5 m skate lanes, banked turns with ride panels on the outside, straight B with a tunnel). Engine: the Stag's path follower (a
spline of nodes, its speed vs yours), the SLIPSTREAM rule (+25 % skating speed within 12 m behind it), floes on straight B in leg 2
(per-lane drops, RED a lane ahead). The Stag: shard kick (RED, change lanes), ice chunk (YELLOW, punch it back), lane cut (RED), kick
back (habit: slipstream > 4 s); damage = catch it (4 m) and punch a hind leg; stagger break = it falls and skids CYAN, a launch punch
into the canal wall; leg 3 the head-on antler charge (YELLOW, 1.5 s: the biggest parry) into the tunnel wall. Pilot: a chasing bot.
Reel `stag37`. Level 6 no longer uses THE STAG: its boss room stays as is until THE RIVAL (D-1); S checks level 6 still boots.

**O1 THE LOOM (builder O, ~200 min).** BOSSES.md "L4 THE LOOM". A new arena after level 4's last room: the canyon (120 m, three tower
pairs, a void; a fall = back to the last anchor ledge, -20 HP). Engine: thread anchors as grapple nodes, the Loom's 6 knees as moving
nodes (exists), thread cuts (nodes go dark 3 s after it passes, RED first), the web wall as a layer swap. The Loom: thread lash (RED),
spool shot (YELLOW), web wall (RED), the hide rule (one tower > 4 s: it severs that tower); damage = nail a knee, pull AWAY (or LT) to
tear it; the BEND on a latch; stagger break = the spinneret opens CYAN: whip on and unload; leg 3 the nest, its last two knees, the
fall. Pilot: a grappling bot (LoomPilot32 is the start). Reel `loom37`.

**C1 THE CROWN (builder C, ~200 min).** BOSSES.md "L5 THE CROWN". A new arena replacing level 5's crown room: the spire descent (70 m:
the summit's shatter, orbiting ring platforms at 3 heights, the gallery's broken spiral ramp with ride panels and dropping sectors, the
plaza with outer sectors dropping). Engine: orbiting platforms that carry a standing player (Devices33's mover on a ring path), sector
drops (Crown32's pattern), the ring pass test through the Crown's centre. The Crown: blade ring (RED), gem shard (YELLOW), beam cut
(RED, the next platform), habit (airborne > 6 s: low fire; one platform > 3 s: it drops); damage = jump-dash THROUGH the ring (the TORCH
burns the gems passed; 2 per pass at most); stagger break = the ring stops CYAN; leg 3 the plaza and the finishing slug / pierce through
its centre. Pilot: a descending bot. Reel `crown37`.

**Lead L0 (~40 min inside rounds).** The shared commit (N37Blocks: boss37, wick37, stag37, loom37, crown37; view rows; design.n37 all
off; `--off37`; tape 20; the level swaps for levels 1, 2, 4, 5 as `levelSwap` rows each boss builder fills; hooks in BossRoomBegin /
BossRoomSystem for the four ids; reel groups PhaseN37K/W/S/O/C + verbs37); docs/VERBS.md rows 44-47; every round's frames looked at and
turned into exact builder messages; the REA / ILSpy reads a builder asks for (the decompile is in the session scratch folder).
</cards>

<builders>
Five builders in worktrees (`isolation: "worktree"`), launched by 0:15 with this file, BOSSES.md and the research set. Each: `git merge
port/stage3` first and before every report; dn-check-wt.sh on its OWN scratch dir; fingerprint and `--off37` EQUAL before each sim
commit; CoreHarness 0 mismatches before each Core commit; never the Editor, a build or a Player; reports under 300 words; resumed by
SendMessage. Write scripts with the Write tool (no heredocs into files: CLAUDE.md rule 9).

| Builder | Owns |
| --- | --- |
| **K** kit | new Core/Boss37.cs (+ tests' harness), the shared boss view helpers (a new Game/Boss37Kit.View.cs), design.n37.boss37 |
| **W** THE WICK | gen-n36-foundry.cjs's last rooms + the stack, Design/levels/n36/*, new Core/Enemies/Wick37.cs, Game/SurvivalSim.Wick37.cs, Game/Wick37View.cs, the hazard plane, its pilot, harness, reel, design.n37.wick37 |
| **S** THE STAG | a new gen-n37-iceline-boss.cjs + its level file, new Core/Enemies/Stag37.cs, SurvivalSim.Stag37.cs, Stag37View.cs, the path follower + slipstream, its pilot, harness, reel, design.n37.stag37 |
| **O** THE LOOM | a new gen-n37-span-boss.cjs + its level file, new Core/Enemies/Loom37.cs, SurvivalSim.Loom37.cs, Loom37View.cs, thread nodes, its pilot, harness, reel, design.n37.loom37 |
| **C** THE CROWN | a new gen-n37-spire-boss.cjs + its level file, new Core/Enemies/Crown37.cs, SurvivalSim.Crown37.cs, Crown37View.cs, orbiting platforms, its pilot, harness, reel, design.n37.crown37 |

The lead alone: SurvivalSim*.cs other than builders' new partials, SurvivalGame*.cs, Campaign24, Curriculum25, Program.cs, the tape,
FingerprintBaseline, every build and reel, git. Overlaps decided now: nobody edits Core/Boss36.cs (K copies what it needs into Boss37);
nobody edits Mover.cs (no boss needs a moving wall ride: BOSSES.md "Engine work"); each boss builder owns one levelSwap row; Devices33's
mover is extended by C in a new file, not edited in place.
</builders>

<lean_rules>
- Proof: method 3. Never edit a test to pass; tests pinning a boss's old behaviour run with N37 forced off (the shared commit does it).
- Golden at every Core commit; Mover.cs untouched; JsMath (no System.Math transcendentals in Core: last night's suite caught one),
  Mulberry32, whole-step timers.
- Config: gameplay numbers in design.n37.<row> with a `_why` naming the source; render / UI numbers in UnityTuning.*37.cs.
- Every n37 row behind N37Blocks; `--off37` / `-skygrindOff37` turns all off; tape format 20.
- ULTRAKILL's decompiled code is reference only (numbers and rule shapes, cited); GitHub patterns (NPBehave MIT, CrystalAI MIT,
  BulletML BSD-2) for structure only, rewritten in Skygrind.Core with 120 Hz steps and Mulberry32.
- Caps (report it short instead): builds 8; reel invocations 26; suite 1 (batch, at the freeze); builders 5.
- Memory and Players: heavy jobs one at a time; check free memory before a build or a reel; identify a Player by command line and start
  time before closing it (L-054); no reel left running at the close.
- Commits with explicit paths; never GG's four files; nothing deleted without GG.
</lean_rules>

<tasks>
## U3-50.1 Start (0:00-0:15)
`date`; the Editor gate; `git status`; no stray Players; docs/VERBS.md rows 44-47; the shared commit; launch the five builders.
## U3-50.2 Build rounds (0:15-3:15): integrate at about 1:00, 1:45, 2:30, 3:05
Each round: integrate, gates, ONE batch build, the round's reels (each boss's reel once its fight is playable), frames LOOKED AT,
builders resumed with exact lines. Round 1 (1:00): K's kit, each boss's arena walkable in a Player with its rehearsal rooms (frames of
the space before the boss is in it). Round 2 (1:45): each boss fightable on the kit; first band numbers. Round 3 (2:30): each boss's
iteration on its frames (one pass each, at least). Round 4 (3:05): the last fixes.
## U3-50.3 Freeze (3:15) and verification (3:15-3:45)
fingerprint `--off37` EQUAL; golden; one batch suite; the four boss reels; verbs36 on levels 1, 2, 4, 5 (one Player per map); level 3
and level 6 boot; NIGHT37-PROOF.md.
## U3-50.4 Close (3:45-4:00)
NIGHT37-LOG close table (K1, W1, S1, O1, C1, L0: DONE or SHORT with a measured reason), NIGHT37-REPORT (GG's page: what to fight, what
to look for), FEEL-PLAYTEST (the four bosses), HANDOFF, PORT-STATUS events, DECIDE37, the memory harvest, tag u3-n37. Hard cap 4:20.
</tasks>

<leftovers>
1. THE RIVAL for level 6 (D-1).
2. Rebuilding levels 2, 4 and 5 from scratch on the n36 method (their bosses are new tonight; their rooms are not).
3. Riding a moving wall (a Mover row of its own, if a later boss needs it).
4. Pad aim assist on moving bosses (light, never on the core).
</leftovers>

<closing_reminder>
- A boss is DONE only on frames the lead looked at, its band, and one iteration.
- Nobody stands still in any of the four fights: if a frame shows a player standing to deal damage, that boss is not done.
- Every lead message ends with `N37-STATUS: round <r> · builders <alive>/5 · bosses <done>/4 · <h:mm> / 4:20`.
</closing_reminder>

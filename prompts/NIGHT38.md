# NIGHT38.md: the flight night: every level overhauled, walls you stay on, doors you can see (lead + eight builders, about 5 hours)

GG, 2026-10-13 ~00:10, after playing n37-r7 (verbatim and a row map: Research/2026-10-13-wave38/GG-REQUEST.md): "Gameplay feels a little
bit better"; THE STAG's design liked but "the middle walls are a little too short and clunky"; "it's just hard to really know what's
happening"; doors "not visible now and you just run through it and it's like dark" (a holographic barrier instead); THE LOOM "needs to
[polish] a little more I like it"; wall rides "you don't actually fall down you just stay on forever ... you can choose ... where you're
aiming up and down"; "I really like the layout of the platforming before the final [boss]" and want every level's overhaul to have rooms
like it: "you're forced to shotgun boost ... a far dash jump ... they can't be too exploitable"; "wide open arenas with a roof"; old rooms
"with just random boxes and lava ... randomly placed with no meaning" and "too tight" rooms where "you're encouraged to just start
shooting immediately and I don't want that"; "you should be flying through and ... killing the enemies before you enter the next room in
a stylish fashionable way"; the crosshair rotating while reloading "more of a thing", the charge readiness "similar to ultra kill ... three
bars or ... a bar under the crosshair"; the grapple indicator "too bland"; "polish the final boss"; "the screen to shake ... when he first
enters ... the worm boss digging"; "a static TV" flicker when you take damage; "go well beyond".

GG decided (DECIDE38.md, ~00:30): **everything in one long night** (D-1; the lead recommended two), doorways **cyan open / red-orange
locked** (D-2). Defaults D-3..D-6.

Base: port/stage3 at u3-n37 (d0a19739). Read first: CLAUDE.md, docs/PRINCIPLES.md, docs/VERBS.md, HANDOFF.md, NIGHT37-REPORT.md, and
Research/2026-10-13-wave38/: **PLAN.md (the plan: read all of it)**, GG-REQUEST.md, DECIDE38.md, research/arena-web.md (arena +
forced-verb design, URLs), research/uk-hud-feel.md (ULTRAKILL's HUD / shake / hurt / walls / doors: numbers only). Also NIGHT36's level
method (Design/levels/n36/foundry.beats.md, Tools/night/gen-n36-foundry.cjs, n36-lib.cjs, n36-jobs.cjs). t0 = GG's go. Box 5:00, hard
cap 5:20.

<goal_for_the_night>
GG plays any level and flies. Every room is new or a kept NIGHT36 room: open roofed arenas where enemies appear on his line so he
fights without stopping, and flight rooms where each gap only opens to the level's verb: a shotgun boost in the kiln where the walls
can't be ridden, a far dash jump on the spire under a ceiling that kills the double jump, grapple gaps over the canyon void, wall-ride
chains in the foundry. He holds a wall as long as he wants and drifts up or down by looking. He always sees where the doors are: cyan
to pass, orange when locked. Under his crosshair he can see each gun's charge fill and flash ready; the crosshair spins while it reloads.
Every boss arrives with a shake and a flicker, and a hit fills the screen with static. THE STAG's walls carry him. Nothing he had is gone.
</goal_for_the_night>

<method>
1. **Gates are proven, not eyeballed.** Builder E lands the envelope table and the gate checker FIRST (by 1:00); every level builder
   tags each gate with its intended verb and ships only gates the checker passes (the intended verb with >= 10 % margin, no single
   other verb or combo, D-6).
2. **No copied rooms.** Every level is written whole (beat sheet first, job tags, n36-jobs GREEN, the gate checker GREEN, the walk bot);
   the only kept rooms are n37-foundry's a1-a6 and the four NIGHT37 boss arenas + their rehearsal rooms (D-4).
3. **Eyes close a card** (L-001): Player frames the lead looked at + the checks + one iteration.
4. **Nothing lost:** verbs on every level (one Player per map) at the freeze; ride38 changes rows 9 / 11 on purpose (marked).
5. **Memory:** eight builders: each runs heavy harness jobs one at a time and never two `dotnet build`s at once in its worktree; the lead
   runs builds / reels / the suite serially and checks free memory first (the n37 freeze suite was stopped for low memory).
</method>

<cards>
**E1 envelopes + the gate checker (builder E, ~90 min, then gate support all night).** Measure in the real sim (SimHarness, the real
Mover and Combat) the max distance and height of every verb and combo, from flat ground and from a wall ride: jump, double jump, dash,
jump-dash, slide-hop, the wall-ride jump-off (pitch -45 / 0 / +45), wall-jump chains, the THUNDERCLAP boost (mid-air shot -aim x 10 m/s,
weapons.shotgun.jumpSpeed; chained shots), the shockwave (12 m/s), the grapple (40 m, 2 charges); write `Design/levels/envelopes38.json`
(+ the harness command that regenerates it). Then `n38 gates <plan.json>`: for every room solid tagged `gate: {verb, from, to}` it drops
the Mover at `from` and tries the intended verb and every other verb / combo the level owns: PASS only if the intended crosses with >= 10 %
margin and no other single verb or combo crosses (D-6); prints each gate's margins. A planted cheesable gate must fail (selftest). Also:
the level-format notes for `gate`, `ceiling`, `noride` (a wall the Mover must not ride: |normal.y| rule or a tagged non-ride surface:
coordinate with M if a Mover flag is needed), `flight` rooms.

**M1 ride38 + doors38 (builder M, ~150 min).** PLAN.md sections 1 and 3. Mover.cs inside the wallride35 branch only (named here):
a held ride keeps its height (no sink, a 60 s stuck guard), the pitch drifts up / down beyond +-15 deg up to ~2.5 m/s, never past the
wall's top; the timed ride unchanged. A `noride` surface flag for E's gates (walls that visibly can't be ridden: a dull, ridged material
and no cyan trim). doors38: a holographic curtain on every doorway (cyan open / red-orange locked / the boss door pale violet-white with
its sign, D-2), visible from 40 m, the next room lit behind it; the Arena36View slab orientation bug. Checks: harness (30 s hold within
0.05 m; +45 deg rises, -45 sinks, stops at the top), verbs rows 9 / 11 (changed by ride38), frames of a doorway open / locked / boss.

**H1 hud38 + juice38 (builder H, ~150 min).** PLAN.md section 4. Under the crosshair a three-segment bar per gun's charge / cooldown
(KESTREL charge, THUNDERCLAP slug, the gauntlet's BLAST pips: hidden today by hud33's values layer, UiHotbar.cs:143 / UiHud33.cs:119-124),
one colour charging, cyan full, a white flash + event `hud38Ready` on the frame it fills; the crosshair ticks rotate through each reload /
cooldown and snap square when ready (today they only turn on a kill, UiHud33.cs:672-683); the grapple's own hook icon + two charge pips.
Juice: wire `feel.trauma.boss` (in config, read by nothing); a boss entrance = hit-stop + shake (ULTRAKILL: 2-3, decays 3/s, the bigger
replaces) + a full-screen flicker (Crt29Pass's strength spiked ~0.3 s, Render/Crt29Pass.cs:130-146) + the card, for ALL bosses incl. THE
KILN WYRM; big boss moves (the worm's dig / erupt, the Stag's crash, the Loom's fall, the Crown's shatter, the Wick's dive) shake +
flicker; damage = a full-screen static / RGB-split burst for the i-frame time scaled by the hit + a small shake (UiTv35's hurt glitch is
HUD-only today). Events for GG's sounds. Checks: frames (the bar charging / full / flash; the spin mid-reload; a boss entrance; a hit).

**F1 level 1 additions + level 6 (builder F, ~200 min).** n37-foundry keeps a1-a6, then adds an OPEN ROOFED ARENA (KESTREL + ride walls,
enemies on the route) and a FLIGHT ROOM of wall-ride gates (walls farther apart than any jump, only a ride chain crosses) before THE
WICK's rehearsal rooms; level 6 (n35-finale) rebuilt whole: mixed gates (one per verb, in sequence: ride, boost, grapple, dash jump), two
open arenas, its boss room as is (the old Stag). Owns Tools/night/gen-n36-foundry.cjs's n37 variant's new rooms (coordinate: W wrote
the stack in it last night; keep the stack untouched) and a new gen-n38-finale.cjs.

**L2 level 2 + stag38 (builder S2, ~200 min).** n37-iceline's copied rooms i1a..i7 replaced by new rooms (skating, GAUNTLET, ride chains;
open roofed arenas; one flight room of skate-lane + ride gates), keeping s1-s4 (the rehearsal rooms + the canal). stag38: lane walls 5 m
tall in runs of 30 m+ (a 2.6 m wall holds only while the feet are under ~2.25 m: Mover's contact band feet+0.35..+1.45), the island's
walls rideable, a read pass (an off-screen arrow to the Stag, brighter lane warnings). Owns gen-n37-iceline-boss.cjs and Stag37*.

**L3 level 3 (builder K3, ~200 min).** n35-kiln rebuilt whole around the THUNDERCLAP and the **shotgun boost**: boost gates (`noride`
walls, ceilings under the double-jump apex, ledges only a mid-air boost reaches; chained boosts in the last gates), open roofed arenas,
THE KILN WYRM's room k5 and the Wyrm untouched (GG). A new gen-n38-kiln.cjs + level row (a levelSwap row in design.n38).

**L4 level 4 + loom38 (builder O2, ~200 min).** n37-span's copied rooms s1-s7 replaced (grapple gates over voids too long for any jump
or ride chain, open roofed arenas with nodes, the STITCHER), keeping o1-o3; loom38: one polish pass from the n37-r7 frames + its
entrance juice hook. Owns gen-n37-span-boss.cjs and Loom37*.

**L5 level 5 + crown38 (builder C2, ~200 min).** n37-spire's copied rooms v1-v8 replaced (far dash-jump gates under ceilings, a limited
dash meter at each gate so the dash jump can't be spammed gap to gap; open roofed arenas; the TORCH), keeping v9-v11; crown38: the look
pass ("make it look a little bit better": the ring's gems, the shatter, the spire) from the n37-r7 frames. Owns gen-n37-spire-boss.cjs
and Crown37*.

**Lead L0.** The shared commit (N38Blocks: ride38 + the levels' flow rows; design.n38; `--off38`; tape 21; view rows doors38, hud38,
juice38; reel groups), every round's frames, the verbs runs, the docs.
</cards>

<builders>
Eight builders in worktrees, launched by 0:15. Each: `git merge port/stage3` first and before every report; dn-check-wt.sh on its OWN
scratch dir; fingerprint and `--off38` EQUAL before each sim commit; CoreHarness 0 mismatches before each Core commit; never the Editor,
a build or a Player; reports under 300 words; scripts and docs with the Write tool; resumed by SendMessage. Level builders: every new
level reaches the Player (json + StreamingAssets copy + .meta + maps.list + unlocks row), every solid `job` + `why`, ride panels
`ride: true`, gates `gate: {...}`, n36-jobs GREEN on new rooms, E's gate checker GREEN, the walk bot through every room (M's Walk pilots:
script your own rooms in a new `Bot/Walk38<Level>.cs`), a reel `level38` frame set per room (reuse L's level36 reel with frames36).

| Builder | Owns |
| --- | --- |
| **E** envelopes + gates | Harness.N38.Gates.cs, Design/levels/envelopes38.json, Tools/night/n38-gates*, the level-format notes |
| **M** ride38 + doors38 | Core/Mover.cs (wallride35 branch only), Core/Ride35.cs / Grip36.cs, the noride flag, DoorFrames29 / Zones' view, Arena36View, a new Doors38View.cs |
| **H** hud38 + juice38 | UiHud33's reticle / rail (H alone in UiHud33 tonight), UiRail34*, HudWeapons30, a new UiHud38.cs, CameraFeel's boss / hurt hooks, Crt29Pass's strength hook, UiTv35 |
| **F** L1 additions + L6 | gen-n36-foundry.cjs's new rooms (not the stack), gen-n38-finale.cjs, their level files |
| **S2** L2 + stag38 | gen-n37-iceline-boss.cjs, Design/levels/n37/iceline*, Stag37* |
| **K3** L3 | gen-n38-kiln.cjs, Design/levels/n38/kiln*, the kiln row |
| **O2** L4 + loom38 | gen-n37-span-boss.cjs, Design/levels/n37/span*, Loom37* |
| **C2** L5 + crown38 | gen-n37-spire-boss.cjs, Design/levels/n37/spire*, Crown37* |

The lead alone: SurvivalSim*.cs other than builders' partials, SurvivalGame*.cs, Campaign24, Curriculum25, the tape, FingerprintBaseline,
every build / reel / suite, git. Nobody edits Core/Boss37.cs or the Wyrm.
</builders>

<lean_rules>
- Golden at every Core commit; Mover.cs only inside the wallride35 branch (ride38); JsMath (no System.Math transcendentals in Core),
  Mulberry32 seeded per run, whole-step timers.
- Config: gameplay numbers in design.n38.<row> with a `_why`; render / UI numbers in UnityTuning.*38.cs.
- Every n38 sim row behind N38Blocks; `--off38` / `-skygrindOff38` turns all off; tape format 21.
- Caps (report short instead): builds 9; reel invocations 30; suite 1 (batch, at the freeze, with >= 8 GB free); builders 8.
- Commits with explicit paths; never GG's four files; nothing deleted without GG; no audio (event names only).
</lean_rules>

<tasks>
## U3-51.1 Start (0:00-0:15)
`date`; Editor gate; free memory; `git status`; docs/VERBS.md rows 9 / 11 marked "changed by ride38"; the shared commit; launch the eight.
## U3-51.2 Build rounds (0:15-4:15): integrate at about 1:15, 2:15, 3:15, 4:00
Round 1: E's envelopes + gate checker, M's ride38, H's bar + spin, each level's beat sheet + first rooms. Round 2: doors38, juice38, every
level walkable with its gates GREEN. Round 3: every level's frames iterated once, the boss passes. Round 4: last fixes.
## U3-51.3 Freeze (4:15) and verification (4:15-4:45)
fingerprint `--off38` EQUAL; golden; the batch suite (>= 8 GB free); verbs on all six levels; the level reels; NIGHT38-PROOF.md.
## U3-51.4 Close (4:45-5:00)
NIGHT38-LOG close table (E1, M1, H1, F1, L2, L3, L4, L5, L0: DONE or SHORT with a measured reason), NIGHT38-REPORT, FEEL-PLAYTEST, HANDOFF,
PORT-STATUS events, DECIDE38, the memory harvest, tag u3-n38. Hard cap 5:20.
</tasks>

<leftovers>
1. THE RIVAL (level 6's boss, DECIDE37 D-1).
2. THE WICK's kill in a Player; D-7.
3. Gamepad aim-assist on moving bosses.
</leftovers>

<closing_reminder>
- A gate any single other move skips is not a gate (D-6). A room with no job is not a room.
- No copied old room survives tonight except n37-foundry's a1-a6 and the NIGHT37 boss arenas.
- Every lead message ends with `N38-STATUS: round <r> · builders <alive>/8 · levels <done>/6 · <h:mm> / 5:20`.
</closing_reminder>

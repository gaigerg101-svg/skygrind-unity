# NIGHT34.md: the flow night (lead + four builders, about 4 hours)

GG, 2026-10-11 ~13:35, after playing n33 (verbatim, screenshots and a row map: Research/2026-10-11-wave34/GG-REQUEST.md): the grapple
triggers "a dramatic slow-motion effect with a clear visual overlay" so players "switch weapons in real time and chain fluid, expressive
combos ... like that VR game where you're dodging and striking in slow motion as the world bends around you"; remove "that little diamond
indicator"; "an ULTRAKILL-style weapon display that shows your full arsenal and highlights your current weapon"; the game is "cluttered
with detail for the sake of detail" and "the arenas don't incentivize creative movement at all"; the Pulse Gauntlet needs "a slightly
larger active hitbox when striking bosses like the worm"; "the simple style we have now is fine, but the movement, UI, responsiveness,
and overall flow need to be dramatically better", built from open-source references and proven techniques "instead of reinventing".

Base: port/stage3 at the NIGHT33 close (tag u3-n33, dac509f5). Research (done in the forge, four web subagents):
Research/2026-10-11-wave34/RESEARCH.md (the index and the numbers table) over research/slowmo.md, swap-hud.md, movement-oss.md,
arenas.md; SPIKE.md (the bend's code path, read with file:line); IDEAS-POOL.md and SHORTLIST.md (scores). Defaults: DECIDE34.md.
NIGHT33's carried items: NIGHT34.draft.md (outranked by this prompt; its items are in P0 or parked below). t0 = the moment GG says go.
Box 4:00, hard cap 4:30.

<goal_for_the_night>
GG plays one loop and feels it change: grapple to a node, the world drops into a cold grey BEND where only enemies, danger and cyan keep
their colour, he swaps three guns at full speed and
kills, the bend snaps out, he lands on a wall-run line the room was built around and keeps going. Every row is behind `design.n34.<row>`;
`-skygrindOff34` gives NIGHT33's game. Visual detail goes down, not up.
</goal_for_the_night>

<method>
1. **Reference first (this night's forge experiment).** Each card starts from the cited numbers in RESEARCH.md's table, unchanged in its
   first commit, with the source in the `_why`. Later tuning is logged as a delta from the reference ("bend scale 0.25 -> 0.3: frames
   f3 / f4 read too slow"). Why: GG asked to adapt proven patterns instead of reinventing; the log shows how far we moved from them.
   A reference is the start, not the answer: GG also said (w29) "don't just do what Claude does and copy open-source stuff; actually
   study game theory, strong game design principles, and iterate properly". Each card's tuning is judged on our frames and numbers.
2. **A spec, a check, a frame.** Every row ships its automated check and a RED (`-skygrindOff34`) / GREEN Player frame pair the lead looked
   at (L-001, L-014). Adjectives are not results.
3. **Commit every slice** (L-003); the lead integrates about every 50 minutes.
4. **Less on screen.** No new HUD element without one removed or folded; the hud33 rule checker stays at 0 problems.
</method>

<cards>
Each card: its row, what GG feels, the build, the acceptance check (Player where player-facing), the minutes.

**B1 bend34: the grapple BEND (the headline; builder B, ~120 min).**
- Feel: every grapple press that latches (a node, a wall, the whip on an enemy) drops the world into a BEND: enemies, their shots and the
  player's own rockets crawl; the player's look, move, fire and swap stay at full speed. GG: "the whole point is to let players switch
  weapons in real time", so no latch is ever a plain grapple.
- Two tiers (RESEARCH.md table B; gameplay numbers in `design.n34.bend34`, the file named in the commit, each `_why` saying the web has
  no slow motion of its own and citing the source):
  - **SHORT BEND, every latch, free:** `shortS` 1.0 s. The grapple's own charges (NIGHT32) are its limit.
  - **FULL BEND, paid by the BEND METER when it is full:** `holdS` 2.5 s, +`killExtendS` 0.5 s per kill inside it, up to `maxS` 5 s.
    The meter starts full at run start and at each level start. Kills refill it (`refillPerKill` 0.25, a kill at rank S or higher
    counts double), nothing refills it over time; about 15 s of fighting from empty. The 0.25 and the rank double are the forge's
    choice (no source found), logged as such.
  - Both tiers: world share `scale` 0.25, ease in `inS` 0.08 s, ease out `outS` 0.3 s.
  - **Clocks:** every bend time above is REAL seconds. Bend34 converts with the active mode, whole steps: global `StepsFor(s x scale)`
    sim steps, split `StepsFor(s)` player steps. Hitstop sits above the bend (a share of 0 stays 0). REDUCED MOTION and the SLOW-MO
    setting off turn the time change off, never the swap rule.
- Real-time swaps (a Core change: golden gates at every commit; GoldenCombat.SwapTimeline covers it). B owns `Loadout.StepSwap`
  (Core/Combat.cs:1105-1137) behind bend34. Inside a bend the press step sets `slot` in the want branch and calls SwapIn, SwapIn19 and
  weaponRaise. Do not set `swapSteps` to 0: StepSwap's lower bound then never passes. Rules30.queuedSwitch still holds a switch while
  an action runs (keep it, said in the log). Every gun's own cooldown keeps ticking while holstered (Loadout.CooldownStepsOf: check
  each gun). Swap-cancel (design.arsenal swapCancel) and freshness stay the reward.
- Build, slice 1 (global, ~40 min, playable at round 1):
  - `Core/Bend34.cs`: pure; the tiers, the meter, the eased share, in whole steps; EditMode tests.
  - `Game/SurvivalSim.Bend34.cs`: the latch hooks from the Grapple31 / Grapple32 / Whip33 events.
  - A sim-owned `Bend34Share`, read by the loop exactly as `sim.Lantern29SlowScale` is (SPIKE.md). The lead applies the one line in
    `SlowAimFrame`.
  - The whole sim slows; look is already real time.
- Build, slice 2 (split, `design.n34.bend34.split`, capped at 75 min; if SHORT, global stays the default):
  - The player block every step; the world block on steps where `bendStep % N == 0` (N = 1 / scale).
  - Nothing is reordered (CLAUDE.md rule 4). On world steps Step() runs its normal order. On the other steps the world calls are
    skipped, `brain.Step` included, in place.
  - **Player:** input frames and latches, `mover.Step`, health, pads, assist, `loadout.Step`, the N19 / N20 / N21 arsenal hooks,
    N31WeaponsStep, N33WhipStep (its arrive reads enemy positions, which hold between world steps), N33PierceStep, N24FistStep,
    N25CombatStep, regen, buyables, style, power-ups.
  - **World:** every other call, devices included (fans, vents and platforms move on world steps, so a platform carries the player at
    the world's pace), lava timers, projectiles (the player's rockets slow too: DECIDE34 B-3), the brain, husks, bosses and the crowd.
  - The table lives in `Game/SurvivalSim.Bend34.cs` (Core cannot see Game's hooks). It is proven by a harness test per player-side hook.
  - Enemies render with a world alpha (the lead's Render line). With the row off, Step is byte-identical (fingerprint EQUAL).
- Checks:
  - harness `bend34`:
    - (a) every latch gives a short bend;
    - (b) 20 latches in 30 s with no kills give 1 full bend (the starting meter);
    - (c) the world advances 1 step in N while bending (split), or the sim's share holds 0.25 +- 0.02 (global);
    - (d) the gun in hand changes in the press step inside a bend, and with the row off the swap timeline is unchanged;
    - (e) two tape replays are EQUAL.
  - Player reel `bend34`: grapple a node in a fight with 6 husks on n34-foundry. It prints the share, the swap step, the meter tier.
    Frames f1 (latch), f2 (+0.2 s), f3 (a kill inside), f4 (exit).
- Default: the lead picks split or global from f1-f4 and the reel's numbers, leaning to split (GG: "responsiveness ... dramatically
  better"; global slows his own movement and the pull). The choice is logged with the rejected one; both stay switches.

**H1 bend overlay + rail34 + nomark34 (builder H, ~100 min).**
- Overlay (switch `design.n34.bendLook`; its render numbers in `UnityTuning.Bend34.cs`, each with a comment: CLAUDE.md rule 5):
  - Entry: a short thump (`EntryFovDeg` -4, then back over 0.15 s; no freeze by default).
  - The world desaturates by `Desat` 0.65 toward a neutral cool grey, never violet: pale violet-white is the boundary's colour (rule 8).
  - Method: Look.cs grades with one global ColorAdjustments (Look.cs:172). Add a ColorCurves hueVsSat curve that keeps the red-orange
    and cyan hues saturated. Because that curve works by hue, the dusk-orange sky would stay saturated too, so the bend also dims the
    sky's tint (Look.cs owns the sky). Only enemies, danger and cyan pop.
  - The grade deepens a step per kill inside the bend; exit restores it over the bend's `outS`.
  - No screen-border lines and no vignette bars (retired w20). The effect lives in the post grade (`PostFx.cs`, `Look.cs`), not in UI.
  - The meter is one cyan slash under the reticle, folded into hud33's grapple ticks: nothing new on screen.
- The viewmodel inside a bend: H owns `Viewmodel.Draw.cs`'s HandsSwapTimeline. Inside a bend the hands snap to the new gun over
  `SlideS` instead of the 0.25 s timeline (the sim's swap is instant).
- rail34 (`design.n34.rail34`): the ARSENAL RAIL, bottom right, replacing hud33's single gun name. Every owned weapon is a slot in one row,
  in slot order; the gun in hand is larger, cyan, named, with its LT verb ("ZL ZOOM"); the others are dim outlines. A swap slides the
  highlight to the new slot over `SlideS` 0.08 s (UnityTuning.Hud34.cs; a swap read, not a delay). A holstered gun turning ready pulses its slot once
  (HudWeapons30's READY rule); a cooling slot drains its outline (never red, never a fill: hud33's no-fill rule). Inside a bend the rail
  brightens one step. Pad: the existing RB scroll and the weapon keys, unchanged (a held-bumper wheel is a leftover, below). Text >= 16 px
  at 1080p; the hud33 rule checker 0 problems.
- nomark34 (`design.n34.nomark34`): Grapple31View's target diamond is not drawn; the node ring (GrappleNodes30) stays the only grapple
  read. Nothing replaces it. The NIGHT31 grapple reel reads `Grapple31View.MarkShown`: with nomark34 on, it reads the latch target
  without drawing it (MarkShown stays the "would show" bit), so that reel is not a regression.
- Check: reel `hud34`: frames with 5 guns owned (all 5 slots, the hand highlighted), after 4 swaps (the highlight moved each time), in a
  bend, in a boss fight; the rule checker 0; reel `bend34`'s f1-f4 for the overlay; nomark: a frame aimed at a node and at a wall with 0
  active mark renderers (counted) and the ring visible.

**A1 lines34 + calm34: arenas built for movement, with less on them (builder A, ~130 min).**
- Feel: every fight room has an obvious movement line: a wall-run wall, a grapple node and a bhop lane placed to chain into each other,
  two or three floors joined by movement props, and nothing on the floor that only decorates.
- Rules (RESEARCH.md table A, research/arenas.md's ten rules): spacings come from our own numbers (measure them first with the harness:
  jump distance, dash, a wall-ride's length at entry speed, the grapple's reach), never guessed; enemies keep a climb route to every
  floor (NIGHT20); one colour per meaning (cyan usable, red-orange danger, pale violet-white boundary); the score rewards the line, not the
  floor (C-13 below if time).
- First 20 minutes: A measures the distances itself (`Tools/SimHarness/Harness.Climb.cs` and THE CLIMB exist): jump distance, dash, a
  wall-ride's length at entry speed, the grapple's reach. It does not wait on M.
- Build, in this order: calm34 first on all five levels (`design.n34.calm34`: Kit33Props / dress33 keep only props with a collider or a
  purpose: cover, a climb, a device, a fixture that lights the room; at least 40 % fewer dressing props per level, counted), then the
  movement lines: `Tools/night/gen-n34-*.cjs` from the gen-n33 generators, `Design/levels/n34-*.json`, `design.n34.lines34.levelSwap`
  n33-* -> n34-* per level (a level not rebuilt keeps its n33 file). n34-foundry and n34-kiln to the full bar first; the other three
  are leftovers (SHORT by design if time runs out, said in the report).
- Check: harness `lines34`: per fight room, wall-run walls >= 1 at length >= the measured ride, grapple nodes >= 1 within reach of a
  floor and a wall, floors >= 2, a lane >= 3 bhop jumps long with no prop in it, a climb route per floor; the walk bot 10/10 on every
  changed level (L-041); the prop count; every changed level boots in a Player the same round (L-043: the lines34 reel boots them, inside
  the reel budget); frames of each changed level's first fight room RED / GREEN.

**M1 flow34 + reach34 + latency34 (builder M, ~110 min).**
- flow34 (`design.n34.flow34`): the patterns of RESEARCH.md table M that SKYGRIND lacks. Grep each mechanism first, not its name:
  coyote 0.1 s, jump buffer 0.1 s, wall ride (gravityMult 0.15, maxTime 1.2 s), wall jump and the slide-jump carry already exist
  (PortExport/config.json movement). At most THREE techniques tonight, in this order, the rest leftovers: (1) no friction on a buffered
  landing jump (bhop crispness), (2) buffer and coyote on walls and slides, (3) a grapple that adds to velocity on release. Each one gets
  its own sub-switch, its own harness check (THE CLIMB / THE CHASE where they measure it), and a number before / after.
  **This card names `Mover.cs`** (rule 3): edits only inside `if (flow34)` branches. New timer fields in the mover's state and the
  snapshot go in the shared commit's slots (the lead) or by a patch the lead applies. With the row off, at every commit that touches it:
  golden 0 mismatches and 22 trajectories bit-exact by `dotnet run --project Tools/CoreHarness -c Release` (every vector set and replay,
  exit 1 on any mismatch), its total line quoted in the report.
- reach34 (`design.n34.reach34`): the Pulse Gauntlet's punch and blast reach bosses with `bossMargin` m added to the boss body's radius
  (start 0.6 m; RESEARCH.md: inflate the enemy's hurtbox, sphere test). The hit frame gets the boss impact (hitstop + spark) it already
  has for a body hit. Where the code is: the worm's punch test is `SurvivalSim.Boss30.cs:282-288` (lead-only; it already has
  `design.n30.wyrm30.headReachMargin`), punch targets come from `Core/Combat.cs` PunchTargets (B owns Combat.cs tonight), the blast is
  `Core/Guns/Pulse25.cs:78` (M). M writes the two .cjs patches for the lead and B, and owns Pulse25.cs. Check: harness `reach34`: a punch
  at the worm at surface + margin - 0.05 m hits with the row on and misses off; bot boss times stay in the 15-22 s band.
- latency34 (a measure, no row): harness table of steps from a press to its first sim change for jump, dash, fire, swap, grapple, whip,
  punch, on and off. Any verb over its designed steps is fixed under flow34 or logged.

**Lead L0: P0 debt (~25 min, inside rounds).** T-44 the shop pad path re-run alone in a fresh Player (one reel); T-45 THE LOOM and THE
CROWN Player kill times (the freeze's boss reel, with reach34 on). Parked on DECIDE34 with their age: T-46 lava orange metric, T-47
hold-still tell, T-48 lava edge pressing, T-49 the allocation test, and the older T-04 / T-05 / T-06 / T-11 / T-12 / T-18 / T-20.
</cards>

<builders>
Four builders in git worktrees (`isolation: "worktree"`), launched by 0:20 with this file and the research set. Each: `git merge
port/stage3` before starting and before every report; `bash Tools/night/dn-check-wt.sh <scratch>/chk` = 0 errors before each commit
(prove it red once, L-034); `dotnet run --project Tools/SimHarness -c Release -- fingerprint` and `... --off34` EQUAL before each commit
that touches sim code; `dotnet run --project Tools/CoreHarness -c Release` 0 mismatches before each commit that touches `Assets/Scripts/Core`;
never the Unity Editor, a build or a Player; reports under 400 words; resumed by SendMessage.

| Builder | Owns | Card |
| --- | --- | --- |
| **B** bend | new `Core/Bend34.cs`, `Game/SurvivalSim.Bend34.cs` (with the hook table), `Core/Combat.cs` (StepSwap behind bend34; M's PunchTargets patch), `Tools/SimHarness/Harness.N34.Bend.cs`; reel `bend34` scenario in `FeelReel.N34.Bend.cs` | B1 |
| **H** look + HUD | `PostFx.cs`, `Look.cs` (the bend grade and sky dim only), new `Bend34View.cs`, `Viewmodel.Draw.cs` (HandsSwapTimeline in a bend), `UiHud33.cs` / `Hud33Layout.cs` (the rail), new `UiRail34.cs`, new `UnityTuning.Bend34.cs` / `UnityTuning.Hud34.cs`, `Grapple31View.cs` (the mark only), `HudWeapons30.cs` reads; reel `hud34` | H1 |
| **A** arenas | new `Tools/night/gen-n34-*.cjs`, `Design/levels/n34-*.json`, `Kit33Props.cs` / `Tools/night/dress33.cjs` (the calm filter behind calm34), `Harness.N34.Lines.cs`; reel `lines34` frames | A1 |
| **M** movement | `Core/Mover.cs` (flow34 branches only), `Core/Guns/Pulse25.cs`, `Core/Fist24.cs`, `Harness.N34.Flow.cs`, `Harness.N34.Latency.cs`, `Harness.N34.Reach.cs`; the reach34 patches for Boss30.cs (lead) and Combat.cs (B) | M1 |

Shared rules: each builder edits only its own `design.n34.<row>` objects in Design/unity-config.json (overrides of older rows only, never
their values); StreamingAssets by cp + cmp in a worktree. **The lead alone** touches `SurvivalSim*.cs` other than the new files above,
`SurvivalGame*.cs`, `Tools/SimHarness/Program.cs`, the tape, `FingerprintBaseline*.cs`, `Core/Curriculum25.cs` (the level swap), the config
merge, every build and reel. A builder names the line it needs as a .cjs patch script; the lead applies it the same round. Overlaps decided
now: B owns the bend's state and share and Combat.cs, H only reads `Bend34Share` / `Bend01`; M owns `Mover.cs`, B never edits it; A
measures its own distances.
</builders>

<lean_rules>
- **Proof:** a visual row is DONE only with a RED / GREEN Player frame pair the lead looked at; a gameplay row with a Player number. A
  stand-in is SHORT, never DONE. Tests verify, they don't define: never edit a test to pass.
- **Golden at every Core commit (rule 3):** fingerprint + `--off34` + golden 0 mismatches + 22 trajectories exact; JsMath and Mulberry32
  only; whole-step timers.
- **Config (CLAUDE.md rule 5):** new gameplay numbers in `design.n34.<row>` (Design/unity-config.json) with a `_why` naming the source
  (method 1) and why the web has no such number, the file named in the commit; render, UI layout and camera numbers in
  `UnityTuning.Bend34.cs` / `UnityTuning.Hud34.cs`, each with a comment. Patch scripts are .cjs files written with the Write tool; no
  Unity API in MonoBehaviour field initializers (L-035); boxed numbers in level rows are doubles (L-043).
- **Caps** (report it short instead): Player builds 8; reel invocations 20; suite runs 2 (batch); builders 4 alive. Reel budget:
  per round one GREEN invocation per landed card (at most 3) and ONE combined RED invocation (`-skygrindOff34`) holding every landed
  row's RED frames, so at most 12 in rounds 1-3; the freeze 6 (bend, hud, lines with the level boots, bosses with T-45, smoke-side
  checks, one spare); T-44's shop reel 1; one spare. One Player per input-driving scenario, or the hardened reel pattern of b8655b3c
  (L-046).
- **Heavy jobs one at a time** (L-005); check CPU before a reel; the lead never runs a reel beside the suite.
- **Commits:** every finished slice, explicit paths plus `.meta`; never UnityConnectSettings.asset, Arena.unity,
  UIEffectProjectSettings.asset, EditorBuildSettings.asset; read every diff before any `git checkout --` (L-047: GG's edits live in the
  tree); never master, never push; restore the TMP font asset Play mode rewrites; nothing deleted without GG.
- **REA (the Reverse Engineer Anything skill):** the forge session's `rea` server did not connect. Use REA only if its tools are present
  and DECIDE34 R-1 names targets GG allows; read numbers and structure, copy no code, log each finding with its source in RESEARCH.md.
  Without it, the night runs on the research set; that is not a blocker.
- **Usage stop:** if the spend limit stops agents, log the gap, keep building alone on B1 then H1, retry builders at most every 30 min.
</lean_rules>

<tasks>
## U3-47.1 Start (0:00-0:20)
`date`; the Editor gate (GG may be in Play mode: stop only if he is done; else the batch path); `git status` read line by line; the
shared commit: `N34Blocks` (bend34, flow34, reach34, lines34's colliders: the sim bits) in tape format 17 with `AsFormat16`, every
design.n34 row off (view rows bendLook, rail34, nomark34, calm34 included), `--off34` / `-skygrindOff34`, SurvivalSim.N34 hooks per
builder, harness partials N34<Builder>, reel groups PhaseN34<Builder>, `Curriculum25` level swap row (n33-* -> n34-*, per level; a Core
change, so CoreHarness 0 mismatches), the mover-state / snapshot slots M will need for flow34's timers; gates; launch the builders by 0:20.
While they read, in round 1: rewrite integrate.sh's ID map (B H A M) and write `Tools/night/n34-reels.sh` from n33-reels.sh (sets bend,
hud, lines, bosses, shop, and one `red` set that runs every landed row's RED frames under `-skygrindOff34`).
## U3-47.2 Build rounds (0:15-3:10): integrate at about 1:05, 1:55, 2:40
Each round: integrate, sync, Editor compile, dn-check 0, fingerprint + `--off34` EQUAL, golden + trajectories if Core changed, ONE batch
build, the round's reels by the reel budget (lean_rules), frames LOOKED AT, builders resumed with the Player's exact lines. Landing order: B's global
bend and the instant swap, H's nomark and rail skeleton, A's calm pass, M's reach34 in round 1; A's n34-foundry and n34-kiln, H's
overlay and hands, M's flow34 techniques, B's split in round 2; the default pick for the bend, the leftovers in round 3.
## U3-47.3 Freeze (3:10) and verification (3:10-3:40)
`--off34` fingerprint EQUAL; golden + trajectories; one batch suite alone; smoke; LevelSelect29; DoorFree29; the walk bot on every
changed level; bosses (pilot 15-22 s, Loom and Crown in the Player: T-45) and camp; bend34 / hud34 / lines34 reels with frames; latency34 table;
NIGHT34-PROOF.md.
## U3-47.4 Close (3:40-4:00)
NIGHT34-LOG.md close table (one row per card and P0 item: DONE / SHORT with the measured reason), NIGHT34-REPORT.md (one page for GG:
before / after frame pairs, each row's switch, the bend default and why), FEEL-PLAYTEST.md (a 10-minute pad-first walk that starts with
the bend loop), HANDOFF.md, PORT-STATUS.md events (`bend34Start`, `bend34End`, `bend34Kill`, `rail34Swap`: GG's sound hooks), DECIDE34
answers needed, NIGHT35.md draft, the wave-memory harvest, tag `u3-n34`. Hard cap 4:30.
</tasks>

<leftovers>
Approved polish, in order, for any builder whose card is DONE before 3:10 (each behind its row, each with its own check):
1. C-13 air / slide style multiplier on points and style only (ULTRAKILL's air x up to 3: research/arenas.md section 6).
2. A held-bumper weapon wheel that BENDs while held (Doom Eternal / ULTRAKILL; research/swap-hud.md 1, 4), under the bend meter's rule.
3. C-17 named combos: grapple -> bend -> two different guns -> kill stamps a combo name in hud33's one message slot.
4. The bend overlay's per-kill grade step tuned by a blind paired frame read (two reviewers, veto only).
5. flow34 techniques 4-6 of RESEARCH.md table M (wall-ride gravity in phases, one air lurch, corner correction).
6. lines34 on n34-span, n34-spire and n34-iceline, each with the full lines34 check and a Player boot.
</leftovers>

<closing_reminder>
- The bend is the night: if one card is SHORT, let it not be B1. Every latch bends (the short bend);
  the full bend is paid by kills, never by time.
- Less detail, not more: no new HUD element without one folded away; no screen-border lines; no filled HUD boxes; text >= 16 px.
- Switches and exactness: every sim change behind design.n34; golden 0 mismatches; fingerprint EQUAL with the rows off; `Mover.cs` only
  inside flow34 branches.
- Every lead message ends with `N34-STATUS: round <r> · builders <alive>/4 · rows on <n>/<total> · <h:mm> / 4:30`.
</closing_reminder>

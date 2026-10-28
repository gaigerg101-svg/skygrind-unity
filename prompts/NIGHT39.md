# NIGHT39.md: the feel night: level 3 rebuilt, a pistol that charges when held, slow motion that sells the walls (lead + seven builders, about 5 hours; NIGHT38's unfinished rows carried in C1)

GG, 2026-10-13 ~14:05, after the NIGHT38 close (verbatim and a row map: Research/2026-10-14-wave39/GG-REQUEST.md): level 3 "needs to be
completely overhauled ... encourage far more mechanics, have a new arena, and keep the worm AI the same"; the pistol "if I'm holding it
down it should intelligently prioritize the charge shot instead of firing the fast shot first", the crosshair rotation like the shotgun's,
"a cooler, more powerful-looking animation when you fire the special shot", "heavier, slower, and more impactful"; "a ghost weapon" after
the shotgun on level 3; level 1's ride rooms: "you gain speed too quickly", jump height "increased slightly", the three-wall room too fast;
"a new wall material with a clear visual texture that indicates you cannot wall ride on it"; the gauntlet's parry "emphasized more and
slowed down"; wall riding + zoom "slow down dramatically ... it needs to be extremely slow every time"; "if you grapple onto an enemy the
game should slow down with the same effect"; the worm "needs more health ... all bosses ... one to two minutes, maybe three if you're
slow"; "more polish".

**A reversal (record it, do not argue it):** w31 / w32 GG called the bosses "way too tanky" (CORE.md: 15-22 s bot, ~40 s person, "never add
health"). Tonight he asks for 1-2 minute fights. NIGHT39 follows tonight's words; DECIDE39 D-1 records the reversal and its numbers.

Base: port/stage3 at u3-n38 (07dfb1de). Read first: CLAUDE.md, docs/PRINCIPLES.md, docs/VERBS.md, HANDOFF.md (the NIGHT38 block),
NIGHT38-REPORT.md, NIGHT38-LOG.md (the close table and the integration record), Research/2026-10-14-wave39/: GG-REQUEST.md,
research/feel-web.md (slow motion, parry, charge triggers: numbers + URLs), research/arena-boss-web.md (boost arenas, boss length, no-ride
materials: numbers + URLs). NIGHT38-BRIEF.md is the builder brief's template (untracked; copy its rules section). t0 = GG's go. Box 5:00,
hard cap 5:20.

<goal_for_the_night>
GG plays level 3 and it is a new level built to fly with the THUNDERCLAP: every room has a job, boosts and rides carry him, the worm's
fight is the same AI in a new arena and lasts 1-2 minutes. He holds the pistol's trigger and it charges instead of spitting a fast shot,
its crosshair spins through each cooldown, and the charged shot looks and feels like a cannon. On a wall, aiming slows the world hard; a
grapple onto an enemy does the same; a parry freezes the moment so he sees the projectile turn. The ride builds speed calmly, the three-wall
room is reachable, and walls he cannot ride look different at a glance. Every boss lasts 1-2 minutes for a person. Nothing he had is gone.
</goal_for_the_night>

<method>
1. **Prove the premise first (G0, the lead, 0:00-0:30).** Three things NIGHT38 left unproven decide the night's checks:
   (a) the boot path to a campaign level in a Player (NIGHT38's `-skygrindMap n38-*` alone landed on the SURVIVAL attract). The likely
   answer is already in Tools/night/n37-reels.sh: its boot3 / boot6 cases run `-skygrindMap n35-kiln -skygrindReelOnly verbs37` (and the
   arena cases `-skygrindReelOnly level36`): the map flag needs a reel scenario to start a CAMPAIGN run. Prove it on n38-span (the log
   shows the level id and the reel's verbs lines), then use it for every level boot and the verbs (C1 1, 2). (`-skygrindMap` is an
   automation flag that boots SURVIVAL; the reels set CAMPAIGN themselves: FeelReel.N26.Level.cs:71.) (b) **The ghost weapon, fixed by the
   lead in G0 (not left to K4, which may end SHORT):** n35-kiln's room award is `"award":"thunderclap"`, Campaign24.cs:852 runs
   `AwardWeapon(r.award)`, and the weapon id is `shotgun`: a harness line proves the ghost (the award adds an id no weapon has), the fix
   is the id in n35-kiln (both copies) with the proof re-run; K4's new level uses `shotgun` from the start. (c) slowAim27 is enabled at
   scale 0.75 (a 25 % slow-down: "a little bit", GG's NIGHT27 words) and GG says it feels removed: measure in the sim whether its ride
   signal fires with wallride35 / wallgrip36 / ride38 on (SurvivalGame.SlowAim27), and check the two settings the harness cannot see
   (SLOW-MO on, REDUCED MOTION off: read the PlayerPrefs keys, and ask GG what his are in the report). (d) the Editor's NullReferences
   after a compile and the unexplained Play mode (C1 10): reproduce once, note the cause or SHORT.
   **G0 does not block the builders:** they launch at 0:30 whatever G0's state; only K4 needs (b), and (b) is the lead's first 15 minutes.
2. **Feel is measured on the screen** (L-001): every feel card ships Player frames the lead looked at, plus one iteration.
3. **Gates are proven** on E's checker (NIGHT38: `n38 gates`, `n38 limits`) before a level row goes on; a room with a job, or no room.
4. **Nothing lost:** VERBS.md rows the night changes are marked first (25 / 27 the pistol, 9 / 11 the ride, 40 the parry, 17 the grapple
   on an enemy); verbs on every level at the freeze.
5. **Fewer builders, smaller cards** (L-003, NIGHT38's lesson: eight builders finished but five level rows hit the same checker gaps):
   seven builders (six feature owners + V for NIGHT38's views and tests), the checker's owner (E) on call from the start.
6. **Carry-over is a card, not a footnote:** C1's twelve rows each close DONE or SHORT on tonight's table.
</method>

<cards>
**G0 premise (the lead, 0:00-0:30).** Method 1 (a)-(c). Each answer is a line in NIGHT39-LOG.md with its measured proof before any builder
uses it.

**K4 level 3 overhaul (builder K4, ~220 min).** n38-kiln (NIGHT38, gates RED 0/4) is replaced by a new level 3, same theme (the kiln,
molten metal at dusk), new arena for THE KILN WYRM (the worm's AI untouched; its health is B1's). Built around the THUNDERCLAP and the
shotgun boost, using NIGHT38's numbers: one boost lifts 6.76 m; jump + shoot down + double + shoot down reaches 13.36 m (E, `n38 gates`);
the PULSE blast reaches 7.75 m up / 29.65 m out, so boost ledges sit at 9.5-10 m (`noride`), above the blast's 8.5 m and within the boost's
reach with margin (K3's NIGHT38 measurement). Open roofed arenas where enemies spawn on the line ahead; flight rooms whose gates pass
E's checker; at least one room that rewards a boost THROUGH enemies (enemies as boost TARGETS: shoot down over them, not a hook: the whip
is owned from level 4, PRINCIPLES 4); the award id `shotgun` (G0 b). Shapes from arena-boss-web.md section 1 (URLs there): Doom Eternal's
"jungle gym" arenas; Doom 2016's asymmetric, vertical "combat chess". ULTRAKILL weakens chained rocket rides (the 4th at half power):
use that only as a LEVEL constraint (gates sized so a third boost is needed, never a verb change; no card owns a boost change tonight). No
published ledge or shaft sizes were found: E's envelopes are the numbers. The worm's arena: level 3's worm is design.n26.wyrm26, whose arena is
already config (`"arena": "Design/maps/wyrm26-arena.json"`); SurvivalSim.Wyrm24.cs hard-codes `Design/maps/wyrm-arena.json` only for the
layer-1 worm. K4 first confirms which one n35-kiln's k5 loads (a harness line): if wyrm26, the new arena is a new json plus an override
path in design.n39 behind kiln38 (no lead code); if the hard-coded one, the lead adds the hook. Wyrm.cs clamps to `arenaHalf`
(Wyrm.cs:330): keep the SAME half-size, or the AI is no longer the same. Beat sheet first (n36 method), job tags, n36-jobs GREEN, gates GREEN, the walk bot 10/10. Owns gen-n38-kiln.cjs (rewritten),
Design/levels/n38/kiln*, the kiln38 row, Bot/Walk38Kiln.cs, the new worm arena json. **Timing:** K4's level is walkable with gates GREEN
by 3:15 (round 2), so B1 can re-measure the worm in its new arena and the lead can iterate frames once.

**P1 the pistol (builder P1, ~180 min).** The KESTREL:
- **Hold prioritises the charge** (GG ask 2): tap36 fires on the press, always. Replace it with a release-time rule: a press released
  within a threshold fires the tap; a press held past it fires NO tap and charges. Starting numbers (feel-web.md, with URLs): Unity's
  tap interaction defaults to 200 ms; casual players need up to ~200 ms to decide a hold; INVERSUS measured ~150 ms of added latency from
  firing on release and hid it by correcting where the shot spawns (full correction to 110 ms, none by 190 ms); ULTRAKILL's Piercer
  puts the charge on alt-fire and an early release fires nothing. Pick the threshold in that range, measure the tap's added latency in
  the harness, and hide it with INVERSUS's spawn correction if it reads late. A new row `p39` behind N39Blocks (tap36's behaviour with
  the row off). VERBS rows 25 / 27 marked "changed by p39". **Read first why NIGHT36 reverted to fire-on-press** (Research/2026-10-12-
  wave36/REVIEW.md root cause 1: charge35 fired taps on RELEASE and GG called the pistol "not satisfying"): p39 is close to that rule, so
  a quick tap must still feel instant (the spawn correction is not optional if the measured latency is over ~100 ms). p39 is the KESTREL's
  only: the THUNDERCLAP keeps tap36.
- **Heavier, slower, more impactful** (ask 4): recoil kick, a longer settle, a hitstop on hit, a bigger muzzle; numbers in
  UnityTuning.Pistol39.cs (view) and design.n39.p39 (any fire-rate change, with its _why).
- **The crosshair spins like the shotgun's** (ask 3): H's NIGHT38 hud38 spins only through a gun's cooldown of at least 0.6 s (the
  THUNDERCLAP's pump; H's round-1 report), so the KESTREL never spins. Spin it through the KESTREL's charge and its charged shot's
  cooldown (and the tap's own cooldown if it reads well), then hud38 ON after the lead's frames.
- **A special-shot animation that looks like a cannon** (ask 3): the charged shot's beam, flash, kick and shake; one frame set.
Owns Core/Guns/Charge35.cs + its tap36 branch, the KESTREL's view (a new Pistol39View for the kick, settle and cannon shot: it calls
CameraFeel's existing API, never edits CameraFeel), UnityTuning.Pistol39.cs, design.n39.p39 (the hit's hitstop numbers included), and the
KESTREL's part of hud38's spin in UiHud38 (V owns the rest of UiHud38: coordinate through the lead).

**R1 the ride (builder R1, ~150 min).** Level 1's ride rooms (ask 8). Today's numbers (design.n35.wallride35, read 2026-10-13): every
ride starts at a floor of minSpeed 8 x (1 + speedBuff 0.3) = 10.4 m/s; each jump-off adds exitBoost 2 m/s up to chainCap 20 m/s; the
jump-off's up speed is upLow 4.12 / upHigh 9.12 m/s by look pitch. So a chain runs 10.4 -> 12.4 -> 14.4 m/s on the third wall, ~16.4 m/s
off it (F measured ~17 m/s arriving at L1). The room GG means is n38-foundry's b2 THE GANTRY (two gates, each a chain of FOUR alternating
ride walls R1-R4 / R5-R8, gaps 37 m under a 3.6 m ceiling since F's round 3; Design/levels/n38/foundry.beats.md). Levers to measure first:
speedBuff 0.3 -> ~0.15, exitBoost 2 -> ~1, chainCap 20 -> ~16, upHigh 9.12 -> a little higher (the "jump height" GG asks for).
**Where the numbers go:** a new row design.n39.ride39 (speedBuff / exitBoost / chainCap / upLow / upHigh overrides) behind an N39Blocks
bit, read in the wallride35 branch; design.n35.wallride35 stays as it is, so `--off39` gives NIGHT38's ride bit for bit and the NIGHT35-38
tests keep their numbers. **"Jump height" is the jump-off only:** the ground jump is a web number (PortExport jumpSpeed 10, golden vectors,
and every envelope K4 builds on): not changed tonight. **R1 also owns THE GANTRY's geometry** (gen-n36-foundry.cjs's b2 rooms / F's
n38f-foundry-rooms.cjs and Tools/night/n38f-config.cjs, the n38-foundry level files): a slower chain may no longer cross the 37 m gaps
(proven at today's speeds), so R1 moves the walls and gaps with the ride, re-proves both gates on E's checker and the walk (`n38f walk
foundry`), and keeps foundry38 ON only if both pass (else OFF with the reason). **The no-ride material** (ask 9): M's NIGHT38 `noride` look ("dull dark ribs",
view, off) becomes a clear texture: a distinct pattern (research: arena-boss-web.md section 3), visible at 30 m, used on every `noride`
surface on every level. Research (arena-boss-web.md section 3) found that every game marks the surfaces you CAN use (Mirror's Edge red,
Ghostrunner / Dying Light 2 yellow, Doom Eternal creased white) and none marks the ones you cannot: keep the cyan ride trim as the
primary cue and make the no-ride pattern its clear opposite (GG asked for both). Its colour is neither cyan (usable) nor red-orange
(enemies and danger only, CLAUDE.md rule 8): a dull, dark, ridged surface. Level 1 and level 3 first; every other level only if time
allows. ride38 (NIGHT38, off: a5 deaths 2 -> 9) is checked here and either fixed and turned on, or left off with the reason.
Owns Core/Mover.cs (wallride35 branch only), Ride35 / Ride38, design.n39.ride39, the noride material in a new NoRide39View (Doors38View is
V's), THE GANTRY's geometry and its generator files.

**F1 slow motion and the parry (builder F1, ~180 min).** (asks 10, 11, 13):
- **Wall ride + aim = dramatic slow motion, every time:** slowAim27's scale from 0.75 toward "extremely slow". No shipped game's time
  scale or ramp was found (feel-web.md section 1: "not found" for Titanfall 2, Ghostrunner, Max Payne, Severed Steel): the scale is a
  Unity-only design number, so start in the 0.25-0.35 range, write it in design.n39.slow39 with that _why, and tune it on Player frames
  (a still frame must show it: the cue, the FOV) before GG plays. Keep aim at real time (Max Payne: "could still aim in real time").
  Severed Steel's rule (its slow meter does not drain while wall running) argues for no drain while riding. Fix the ride signal if G0 (c)
  shows it broken.
- **A grapple onto an enemy slows the same way** (the whip latch, VERBS 20): the same slow while latched.
- **The parry, emphasised and slowed:** a hitstop and a slow window when the gauntlet returns a projectile. Numbers (feel-web.md
  section 2): ULTRAKILL's parry freezes ~0.25 s (a search snippet, the page did not load: treat as unverified) with a white flash; fighting
  games use 8 / 12 / 16 frames of hitstop for light / medium / heavy (one example game). Today's parry hitstop is already 0.25 s (the
  n38-r1 Player log's `[ultra]` line: `parryHitstop 0.25 s`), equal to ULTRAKILL's figure, so the emphasis comes from a slow window after
  the freeze and the cue, not a longer freeze; the returned projectile's trail and colour; a frame where you see it turn.
**How the slow works today (read 2026-10-13, SurvivalGame.SlowAim27.cs):** visual pacing only: a smaller dt into the accumulator, the sim
still steps at 120 Hz, the tape and the fingerprint never see it, Time.timeScale untouched. It fires on `mover.state == MoveState.WallRide
&& loadout.adsActive`, gated by the SLOW-MO setting (impactFrames.rules.slowMoOn; REDUCED MOTION turns it off). Every ride row
(wallride35, wallgrip36, ride38) still uses MoveState.WallRide, so the signal should fire: the likely reason it "feels removed" is the
mild 0.75 scale. G0 (c) proves it in the sim. So slow39 is a LOOP / view row (design.n39.slow39: scale, ease, the whip-latch condition
added to the same `target` line), no N39Blocks bit, fingerprint unaffected; the parry's slow window is the same kind (pacing), unless it
changes the sim's step count per real second in a way the tape must record (it does not: say so in the commit).
Owns, tonight, **SurvivalGame.SlowAim27.cs itself** (handed over from the lead for this night: the ride signal, the whip-latch condition
and the parry's slow window all live there), SlowAim27.cs, design.n39.slow39, the parry's view and the Impact layer's parry numbers,
Grapple31View's slow cue. Not the pistol's hitstop (P1's numbers in design.n39.p39).

**B1 boss length (builder B1, ~150 min).** (ask 12): every boss lasts 1-2 minutes for a person (B1 sets the bot band from the person
target: NIGHT32's bot runs ~2x a person's speed, so the bot band is about 30-60 s; say the factor you used). THE KILN WYRM gets more
health (GG; today: design.n24.wyrm.hp.pool 7800 x design.n25.wyrmAim.hp.mult 1, phases at 66 % / 33 %; NIGHT25 tuned it to a 4-5 min
bot kill and later nights cut it: measure today's kill time before changing anything); the four NIGHT37 bosses (THE WICK, THE STAG, THE LOOM, THE CROWN) and level 6's old Stag are tuned the same way: health,
the stagger window and the attack budget, not a sponge (arena-boss-web.md section 2: no designer published a length; the patterns
that lengthen a fight without a sponge are an armour window (Doom Eternal's Marauder takes damage only in its shield opening) and new
attacks at 75 % / 50 % health (Hades' Megaera); reviewers called Doom Eternal's bosses "bullet sponges"). Today's bands (NIGHT37-PROOF.md): THE STAG p50 20.9 s, THE LOOM 21.6 s, THE CROWN 21.9 s, THE WICK 34.3 s total (15.4 s from
the cap). Each boss's harness band printed before and after
(the NIGHT37 reels: wick37, stag37, loom37, crown37, the worm's). **Where the numbers go:** a new row design.n39.boss39 of per-boss
overrides (health, the stagger meter, the CYAN window, the attack budget, the worm's pool mult) behind an N39Blocks bit, read where each
boss reads its numbers; design.n37.* and the worm's rows stay as they are, so `--off39` and the NIGHT37 band tests keep their numbers.
**Not a sponge:** the Boss37 brain already has the levers (the stagger meter, the window, the budget): a longer fight is more stagger
cycles and attacks, health second. A new attack or an armour window is a Boss37 logic change: not tonight (DECIDE39 D-1 lists it for a
later night). The worm: its AI unchanged (GG); its pool and its phase thresholds are numbers. B1 re-measures the worm in K4's new arena
after round 2 (the arena's size changes the fight). Owns design.n39.boss39 and the boss harness bands.

**E2 the checker on call (builder E, ~90 min, then support).** K4's gates and R1's re-proven gates; the open NIGHT38 gaps: a skate gap
judged on ice (n38-iceline G1), the finale's two-pull grapple (x6). Owns Harness.N38.Gates.cs.

**C1 NIGHT38's unfinished work (the lead + builder V, all night).** NIGHT38 closed short (NIGHT38-LOG.md close table). Every row below
is carried, owned, and closes DONE or SHORT on tonight's table:
| # | NIGHT38 left | tonight | owner |
|---|---|---|---|
| 1 | level boots of n38-foundry / n38-span / n38-spire never reached the level (the attract on SURVIVAL: `-skygrindMap` without a reel) | G0 (a): `-skygrindMap <id> -skygrindReelOnly verbs37`, then boot all three + level 3 | lead |
| 2 | verbs (VERBS.md) never run on any level | the same boot runs the verbs37 reel: one Player per map, round 1 (the before) and the freeze (the after) | lead |
| 3 | no frames of doors38, hud38, juice38, stag38, loom38, crown38 (all built, all OFF) | lead takes the frames round 1; V fixes what they show round 2; each row ON after one looked-at iteration | lead + V |
| 4 | iceline38 OFF: G1 (the skate gap) not judged (no ice in the box world) | E2 builds the ice judge (a tool; it answers DECIDE39's carried row with "a judged gate" if G1 passes); if GG answers "a flow beat" first, GG's answer wins; iceline38 ON when G1-G3 pass and the walk clears | E + lead |
| 5 | finale38 OFF: the exam level owns every verb, gates 0/4 | waits on GG (DECIDE39 carried row); if GG answers, F's room limits approach (as spire's `pistolOnly`) is the default fix | lead |
| 6 | ride38 OFF: a5 deaths 2 -> 9 with the row on | R1 | R1 |
| 7 | kiln38 RED 0/4 | replaced by K4 | K4 |
| 8 | AutopilotTests.PerStep_AllocatesNothing: 1 allocation over 17,748 steps | find the allocation (a NIGHT38 per-step path: spire meter, ride38, hud38 / juice38 sim reads); fix or prove pre-existing on u3-n37 | V |
| 9 | MapChoiceTests.EveryListedMap_LoadsAndPlays600Steps: 180 s timeout (flaky since NIGHT35, more maps since NIGHT38) | time each map in the test; raise the timeout with the measured reason or split the test | V |
| 10 | the Editor logged NullReferences (SurvivalGame.Update cfg null) after the 13:00 compile, play mode stopped; later found in Play mode with no known cause | G0 (d): reproduce after a compile; find what entered Play | lead |
| 11 | not written: NIGHT38-PROOF, FEEL-PLAYTEST (n38), PORT-STATUS events (hud38Ready, boss38Entrance, spire38Meter, ...), the memory harvest | written in G0's half hour or at tonight's close (both nights in one harvest) | lead |
| 12 | lesson: a HANDOFF edit went through `node -e` (rule 9) | docs only with Write / Edit; the harvest records it | lead |

**Builder V the views (builder V, ~180 min).** Owns Doors38View, UiHud38 / hud38 in UiHud33, juice38's CameraFeel / Crt29Pass hooks,
Stag37View.Read38, the loom38 / crown38 views, the two test fixes (C1 8, 9). Works from the lead's frames (the lead sends paths); turns
nothing on (the lead does, after looking).

**Lead L0.** G0 (the ghost fix first: 0:00-0:15); the shared commit at 0:15-0:30, mirroring NIGHT38's n38-shared-patch.cjs (N39Blocks:
p39, ride39, boss39; slow39 is a loop row with no bit; design.n39; `--off39` / `-skygrindOff39`, the latter also added to
N29Fixes.IsAutomationArg (N29Fixes.cs:12; without it an `-skygrindOff39` Player boots the arena); tape 22; the Wyrm24 arena hook only
if K4 finds k5 loads the hard-coded arena); NIGHT39-BRIEF.md; launch the seven at 0:30; every round's frames (frames of a campaign level need a BUILD: the Editor
never loads one, L-043; budget builds for rounds 1, 2, 3 and the freeze); the level boots by the G0 path; verbs; the freeze; the docs.

**If the night runs short, the cut order** (keep first): the ghost fix, slow39's scale, P1's hold rule, B1's bands, K4's level. Cut in this
order: C1 rows 3 (frames of the NIGHT38 views), 9 (the MapChoice timeout), 4 (the ice judge), ride38, the noride texture beyond levels 1
and 3, the cannon-shot animation.
</cards>

<builders>
Seven builders in worktrees, launched at 0:30 whatever G0's state (only K4 needs G0 (b), which lands first). Same rules as NIGHT38 (copy NIGHT38-BRIEF.md section 2 into
NIGHT39-BRIEF.md): git merge port/stage3 first and before every report; dn-check-wt.sh; fingerprint and `--off39` EQUAL before each sim
commit; golden before each Core commit; never the Editor, a build or a Player; reports under 300 words; resumed by SendMessage. A level
row turns on only when its gates are GREEN on E's checker AND the walk clears every room (NIGHT38's bar).

| Builder | Owns |
| --- | --- |
| **K4** level 3 | gen-n38-kiln.cjs, Design/levels/n38/kiln*, the kiln38 row, Bot/Walk38Kiln.cs, the worm's new arena json |
| **P1** pistol | Core/Guns/Charge35.cs (tap36 branch), Pistol39View, UnityTuning.Pistol39.cs, design.n39.p39, the KESTREL's spin in UiHud38 |
| **R1** ride | Core/Mover.cs (wallride35 branch only), Ride35 / Ride38, design.n39.ride39, NoRide39View, THE GANTRY's geometry + its generators |
| **F1** slow + parry | SurvivalGame.SlowAim27.cs (tonight only), SlowAim27.cs, design.n39.slow39, the parry's view + Impact numbers, Grapple31View's cue |
| **B1** boss length | design.n39.boss39 (overrides; design.n37.* untouched), the boss harness bands |
| **E** checker | Harness.N38.Gates.cs, envelopes38.json |
| **V** views + tests | Doors38View, UiHud38, juice38 hooks, Stag37View.Read38, loom38 / crown38 views, AutopilotTests / MapChoiceTests fixes |

The lead alone: SurvivalSim*.cs other than builders' partials (incl. Wyrm24's arena path), SurvivalGame*.cs (except SlowAim27, F1's
tonight), CameraFeel (V's juice38 hooks aside), Campaign24, Curriculum25, n35-kiln's award fix,
the tape, FingerprintBaseline, Walk29Pilot's dispatch lines, every build / reel / suite, git. Nobody edits Core/Boss37.cs logic or the
Wyrm's AI.
</builders>

<lean_rules>
- Golden at every Core commit; Mover.cs only inside the wallride35 branch (R1); JsMath; Mulberry32 seeded per run; whole-step timers.
- Config: gameplay numbers in design.n39.<row> (or the row they tune) with a `_why`; render / UI numbers in UnityTuning.*39.cs.
- Every n39 sim row behind N39Blocks; `--off39` / `-skygrindOff39` turns all off; tape format 22.
- Caps (report short instead): builds 6; reel invocations 20; suite 1 (batch, at the freeze, >= 8 GB free); builders 7.
- Commits with explicit paths; never GG's four files; nothing deleted without GG; no audio (event names only); Play mode only by the lead
  (NIGHT38's close found the Editor in Play mode with no known cause: check `unity-loop.sh status` before every test run).
</lean_rules>

<tasks>
## U3-52.1 Start + G0 (0:00-0:30)
`date`; Editor gate (status: stopped, not playing); free memory; `git status`; G0 (a) (b) (c) answered in the log; VERBS rows marked; the
shared commit; NIGHT39-BRIEF.md; launch the seven. Then C1 11 (NIGHT38-PROOF, FEEL-PLAYTEST, PORT-STATUS events) while they build.
## U3-52.2 Build rounds (0:30-4:15): integrate at about 1:30, 2:30, 3:30
Round 1: P1's charge rule, R1's ride numbers, F1's slow scale, B1's bands measured, K4's beat sheet, E's ice judge. Round 2: K4 walkable
with gates GREEN, P1's cannon shot, F1's parry, B1's tuned bands, R1's noride material. Round 3: frames, one iteration each, rows on.
## U3-52.3 Freeze (4:15) and verification (4:15-4:45)
fingerprint `--off39` EQUAL; golden; the batch suite (AutopilotTests and MapChoiceTests GREEN or SHORT with the measured reason); the
level boots by the G0 path on every level whose row is on (1, 2 if on, 3, 4, 5); verbs on every level; the boss bands; the frames of
every view row turned on tonight (doors38, hud38, juice38, stag38, loom38, crown38, p39, slow39); NIGHT39-PROOF.md.
## U3-52.4 Close (4:45-5:00)
NIGHT39-LOG close table (G0, K4, P1, R1, F1, B1, E2, V, L0, and C1's twelve rows: DONE or SHORT with a measured reason), NIGHT39-REPORT, FEEL-PLAYTEST, HANDOFF,
PORT-STATUS events, DECIDE39, the memory harvest (NIGHT38's too: it was not run), tag u3-n39. Hard cap 5:20.
</tasks>

<leftovers>
1. Any C1 row that closes SHORT tonight (it carries again, with its measured reason).
2. THE RIVAL (level 6's boss).
3. Gamepad aim assist on moving bosses.
</leftovers>

<closing_reminder>
- Feel is done on the screen, not in the harness (L-001). A gate any single other move skips is not a gate (D-6). A room with no job is
  not a room.
- GG's reversal on boss health is tonight's rule; write the before / after band per boss.
- Every lead message ends with `N39-STATUS: round <r> · builders <alive>/7 · levels on <n>/6 · C1 <done>/12 · <h:mm> / 5:20`.
</closing_reminder>

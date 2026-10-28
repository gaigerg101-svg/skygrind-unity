# NIGHT40.md: the build night: a start you can read, one charge arc, a ride that holds, level 2 rebuilt for the gauntlet with a brawler boss, THE LOOM and THE CROWN made fair, three enemies brought into the campaign (lead + nine builders; 5 h build, 1 h test, hard cap 6:00)

GG, 2026-10-13 ~22:00, after playing n39-r4. Verbatim, with a row map (22 asks, each mapped to one card):
Research/2026-10-14-wave40/GG-REQUEST.md. In short: the first wave comes too fast; level 3's boost needs a thorough explanation ("I
love the mechanic itself"); the pistol's spin must finish before the next shot; the shotgun needs a charge shot; charges take longer, shown
by "a single curved bar under the reticle" on every weapon; the shotgun's fire animation faster; the STITCHER out of level 4 so the grapple
stands alone; THE LOOM: no-ride zones (more in phase 2), it "runs out of the map", needs animations, telegraphs and cues that push grapple,
dodge and reposition; "infinite wall-riding", and no snapping onto stairs and platforms; level 2's rooms rebuilt to teach the gauntlet, no
pistol at its start; more enemy types (one with deflectable projectiles); boss 2 a brawler; the white rods you clip through; more detailed
enemy models; THE CROWN: "even I don't know how to damage it"; level 6 deleted (rebuilt later); "five hours of overnight building and one
hour of testing, with a maximum of six hours". **GG's answers before the go (2026-10-13 ~23:00):** level 3 starts with the shotgun, and
"the pistol and gauntlet" are unlocked later in the level (DECIDE40 D-2); GG frees disk space before the run (D-0).

**GG's calls that override standing rules (record them, do not argue them; GG-REQUEST.md lists them):** enemy kinds and enemy models
(CLAUDE.md rule 7 gave enemy design to GG: he now asks for them); the STITCHER, the start weapons of levels 2 and 3, and level 6 taken out
(PRINCIPLES 1: "unless GG says to remove it"); a new boss-2 brain and a smarter LOOM (NIGHT39 froze Boss37 logic). THE KILN WYRM's AI stays
frozen (not named). Boss length stays DECIDE39 D-1 (1-2 min person, bot band 30-60 s).

Base: port/stage3 at u3-n39 (a659d7d0) plus the forge commit. Read first: CLAUDE.md, docs/PRINCIPLES.md, docs/VERBS.md, HANDOFF.md (the
NIGHT39 block), NIGHT39-REPORT.md, NIGHT39-LOG.md (the close table), DECIDE40.md, and Research/2026-10-14-wave40/: GG-REQUEST.md,
research/CODE-MAP.md (every file:line and number this prompt uses, read 2026-10-13; re-check), research/boss-web.md, enemies-web.md,
feel-web.md (numbers with URLs), FORGE.md (the red-team's findings and what was changed). NIGHT39-BRIEF.md is the builder brief's template
(untracked). t0 = GG's go.

<goal_for_the_night>
GG starts each level and has a moment to breathe before the first enemies come; level 3 hands him the shotgun alone and teaches the boost
step by step. Every weapon shows its charge in one arc under the crosshair; the shotgun's charged slug is something he finds and wants;
the pistol's spin and the shotgun's pump finish inside their fire rhythm. A wall ride holds until he leaves it, and stairs never grab him.
Level 2 is new: it starts with the gauntlet alone, teaches what it is good and bad at, hands back the pistol mid-level, and ends with a
brawler boss that rewards fighting up close. THE LOOM stays in its canyon, tells every attack, and is beaten with the grapple; some walls
refuse a ride, more in phase 2. THE CROWN tells him how to hurt it. The DRONE, the WARDEN and the CHARGER join the campaign and make
switching weapons matter; every enemy reads at a glance. Level 6 is gone from the game. Nothing else he had is lost.
</goal_for_the_night>

<method>
1. **The night's shape (GG's words):** G0 0:00-0:30; build from the shared commit (~0:30) to 4:20; round 4 in the Player 4:20-5:00; the
   test hour 5:00-5:45; the close 5:45-6:00; hard cap 6:00. Integration rounds at about 1:30, 2:30, 3:30, 4:20.
2. **Every change behind a switch, and `--off40` stays NIGHT39's game** (L-009): sim rows behind N40Blocks bits; view rows behind
   design.n40 / UnityTuning flags. **Level edits go to new level ids** (n40-iceline, n40-kiln, n40-span, n40-spire), each swapped in by
   its row; the n38 / n37 files are never edited, so `fingerprint --off40` stays EQUAL. A row turns ON only after the lead has looked at
   its Player frames once and iterated once (L-001, PRINCIPLES 8); a row built but unseen ships OFF with its reason.
3. **Root causes are in the code map, not guesses:** THE LOOM's anti-hide writes `stations[leg].z = player.z - 6` with no clamp
   (Loom37.cs:425-430); the first wave spawns on the first fight step (Campaign24.cs:546-579, gapLeft 0); the attach has no wall-height,
   ground-clearance or input test (Mover.cs:1417-1846); THE CROWN's ring pass is explained nowhere; the shotgun's charge exists but a hold
   fires a pellet shot first and nothing shows the charge. Each card proves its root cause with one harness line before fixing it.
4. **Build on what the port already has** (L-023, L-024): the DRONE (Core/Enemies/Drone.cs: hovers 4-8 m, keeps 12-18 m, charges 0.4 s,
   an orb every 2 s, a punch reflects it), the WARDEN (Warden.cs: a front 120 deg shield, a punch stuns it 1 s, flanks and hits from above
   go through, a 0.8 s heavy tell) and the CHARGER (Charger.cs: a 0.8 s tell with a ground line, a 22 m/s charge, stunned 1.5 s on a wall
   with its back x3) are web ports that no campaign level 1-5 uses. ride38 (NIGHT38, OFF) is the infinite ride. bossLine30 (Lantern30.cs)
   is the boss line.
5. **Levels and bosses are judged in the Player** (L-029, L-043, L-062): a campaign level boots only through
   `NIGHT=n40 bash Tools/night/reel.sh Builds/<tag> <tag> <side> <timeout> -skygrindMap <level> -skygrindReelOnly <scenario>`. Builders
   never run a Player: the lead runs every reel and sends the frames' paths.
6. **Nothing lost:** VERBS.md rows the night changes are marked first: 9 / 11 (ride40), 25 / 27 / 29 / 30 (charge40), 28 (iceline40: the
   gauntlet from level 2's start), 32 (loom40: the STITCHER leaves the campaign), 45 (brawl40: THE STAG leaves level 2), 46 (loom40: the
   tear by the grapple), 47 (crown40: the pass stays, now taught); new rows for the brawler's counter and the three enemies' counters.
   The NIGHT39 freeze verbs on n39-r4 (NIGHT39-LOG:201-203) are the "before"; the "after" runs in round 4. The lead owns
   FeelReel.N35.Verbs.cs: builders send the check change their row needs (p39 turned rows RED in NIGHT39 until the reel learned it).
7. **One builder per boss, end to end** (L-060): arena, AI, model, pilots (skilled and casual, L-058), reel scenario, frames via the lead.
</method>

<boss_rules>
Shared by B2, B4 and B5 (research/boss-web.md, URLs there): the design principles of movement-centric fights, made checkable.
1. **Tell every attack:** wind-up light 0.45-0.65 s, medium 0.65-0.9 s, heavy >= 0.9 s (a secondary source: a start, not a law); a
   red-orange glow on the attacking part, a ground decal for an area attack; a parryable projectile is YELLOW (the n36 tells' colour).
2. **Every attack has a movement answer** (dash, jump, ride, grapple) and most leave a punish window after (~1 s: Minos Prime's stuck
   fist; the Marauder's falter, "a couple of seconds").
3. **Standing still is punished by an attack you can see** (an area attack on the player's spot, a rush when far), never by a stat.
4. **The boss stays in its arena:** every position clamped to the arena rect minus a margin; check: 0 frames outside over 10 seeds.
5. **The damage rule is taught on entry** (one line with the bound buttons, generalised from bossLine30), and every hit answers: a hit
   that counts shows a cyan spark and a counter; a hit that does nothing shows a spark and "BLOCKED".
6. **Two phases** (ULTRAKILL splits health about evenly: Minos 65 + 65, Gabriel 50 + 50); phase 2 changes the arena or the attack set,
   announced by a 1-2 s transition.
7. **Length:** 1-2 min for a person; bot band 30-60 s (factor 2, DECIDE39 D-1); the casual pilot finishes 10/10.
8. **Proof in the Player** with slow39 and every verb on (L-048): the boss reel with both pilots, a frame per attack's tell.
</boss_rules>

<cards>
**G0 premise (the lead, 0:00-0:30).** (a) `date`, `bash Tools/unity-loop.sh status` (stopped, not playing), free memory, free disk (GG
frees space first; at the forge C: had 22 GB; a build is 275 MB, a reel's frames ~0.6 GB), `git status`. (b) **The shared commit,
first** (builders launch the minute it is green: golden, fingerprint plain and `--off40` EQUAL, Editor compile): N40Blocks (start40,
kiln40, cut40, charge40, ride40, iceline40, brawl40, loom40, crown40, foes40), each reading `design.n40.<row>.enabled` (all false);
design.n40 (+ view rows arc40, pump40, rods40, models40); `--off40` / `-skygrindOff40` (also in N29Fixes.IsAutomationArg); tape format
23; **the pre-placed hooks** (L-021, L-050): Curriculum25's N40 level swaps (each level row's `levelSwap`) and cut40's order cut;
`N40StartSet` (each level row's config `startSet` / `awards: [{room, weapon}]` / `dropWeapons`, applied by Unlocks31.StartFor and the
room-entry award path); Campaign24's first-wave hold (a flag S1's partial sets); Walk29Pilot dispatch lines mapping each n40 level id to
its n38 pilot; declaration-only partials per builder; harness partials `N40<Owner>`. (c) Background on n39-r4 (the before frames, 4
reels): verbs37 on n38-kiln (the start, the rods), loom37 (the LOOM's path), crown37 with each pilot (B5's bug list). (d)
NIGHT40-BRIEF.md from NIGHT39-BRIEF.md (rules copied, N39 -> N40, tape 23, the nine builders, the frame paths rule). (e) VERBS rows
marked (method 6).

**S1 the start, level 3 and the sweep (builder S1, ~230 min).** Asks 1, 2, 14 (level 3), 18, 21.
- **start40, the start grace:** the first wave of each level, and the wave after a respawn, waits until the player moves 6 m from the
  spawn point OR 4 s pass (whichever first); the enemies then appear behind the spawns33 tell (red-orange column) and cannot attack for
  1.0 s. Doom Eternal locks an arena when the player crosses in (enemies-web.md section 5); no published grace number was found, so 6 m /
  4 s / 1.0 s are hypotheses in design.n40.start40 with a `_why`. Proof: a harness line per level 1-5 "spawn -> first enemy able to attack"
  (before: ~0 s on levels 3 and 4), and a Player frame of level 3's first second (an empty room, the first lesson).
- **kiln40, level 3 (n40-kiln from n38-kiln, a new id):** starts with the THUNDERCLAP alone; the KESTREL and the gauntlet are unlocked by
  entering zones later in the level (GG's answer, D-2), placed where the rooms need them. The boost taught as a chain in the opening room,
  each step shown when the last is done (ULTRAKILL's HudMessage, PRINCIPLES 2): JUMP {jump} -> LOOK STRAIGHT DOWN -> FIRE {fire}: THE BLAST
  THROWS YOU UP -> FIRE AGAIN AT THE TOP -> JUMP AGAIN IN THE AIR FOR THE LAST METRES; then one line on the charged slug (W1's rule). Text
  >= 16 px with the bound buttons; each line held until done (a 20 s fallback). Proof: the gates on E's checker (`n38 gates` on n40-kiln's
  plan) GREEN, the walk 10/10, the owned weapons printed at the start and after each zone, and the lesson frames from a reel.
- **rods40 (view, no data edit):** name the "white rods" on G0's frames first (CODE-MAP's candidates: the light shafts, pole lamps, dress33
  trusses at 8.7-11 m inside level 3's 12 m climbs, chains, edge lights). Rule: no decor without a collider inside the play volume (0.2-12 m
  above a floor, more than 0.3 m from a wall): Kit33Props / Fixtures33 / LightPlan33 skip or move it flush when rods40 is on. Proof: a scan
  per level 1-5 (count before, 0 after) and three before / after frame pairs.
- **cut40, level 6 out of the game:** the row cuts n35-finale from the campaign order (the lead's hook); the level select shows six slots,
  the sixth locked and unnamed; slots 7-10 and every "LEVEL 6..11" label gone from what the player sees; level 5's results read "CAMPAIGN
  COMPLETE". The level-6 files stay in the tree tonight (`--off40` still loads them); their `git rm` waits for GG's yes after he plays
  (D-4). Proof: LevelSelect29 (it reads `playable` from config) on the round-4 build: six slots, five playable, 0 NRE.
Owns SurvivalSim.Start40.cs, design.n40.{start40, kiln40, cut40, rods40}, gen-n40-kiln.cjs + Design/levels/n40/kiln*, UiLevelSelect26,
the results line, the decor filters in Kit33Props / Fixtures33 / LightPlan33.

**W1 the weapons (builder W1, ~210 min).** Asks 3, 4, 5 (the rule), 6.
- **charge40:** charge times longer: KESTREL fullSeconds 0.5 -> 0.9 s, THUNDERCLAP 0.7 -> 1.2 s (hypotheses: ULTRAKILL's Piercer 0.5 s,
  Destiny fusion rifles 0.53-0.97 s, Splatoon's Splat Charger 1.0 s; feel-web.md section 3). **The shotgun's charge already exists**
  (tap36: a press fires, a hold past 0.18 s charges, slug x3): give the THUNDERCLAP p39's hold rule (a hold fires nothing and charges; a
  click fires on release with INVERSUS's aim correction) **on the ground only**; in the air a press fires at once so the boost keeps its
  timing (D-6). Measure when the shot acts (L-055): the ground click's latency (p39's KESTREL: a 60 ms click fires 58 ms after the press;
  over ~100 ms needs the correction) and 0 ms added in the air; level 3's gates GREEN with the row on. The slug gets a look of its own (a
  heavier blast, a ring, a kick: one frame set).
- **The pistol in its rhythm:** every KESTREL rotation (the crosshair's quarter turn, the gun's 9 deg kick and 0.32 s settle, the NIGHT36
  hammer's 0.2 s re-cock) ends within 75 % of the fire interval (0.483 s -> <= 0.36 s), the charged shot's within 75 % of its cooldown;
  ease-out (no published rule, feel-web.md section 5: 75 % is a hypothesis). Proof: a frame log, shot -> each animation's end, inside the
  next shot's time. W1 sets the numbers in UnityTuning.Pistol40.cs; H1 wires the crosshair.
- **pump40, the shotgun's animation faster (view):** pumpDelay 0.45 -> ~0.2 s, pumpDuration 0.3 -> ~0.22 s, the kick spring 4.2 -> ~6 Hz
  (UnityTuning.Pump40.cs; the web's viewmodel numbers stay as they are). The fire interval is unchanged.
Owns Core/Guns/Charge35.cs (the shotgun's hold branch), ChargeShots27's slug look hooks, design.n40.charge40, Pistol39View's timings,
Viewmodel's pump hooks, UnityTuning.Pistol40.cs / Pump40.cs.

**H1 the arc (builder H1, ~180 min).** Ask 5 (the look): one curved bar under the reticle replaces hud38's three SlashBar cells (UiHud38.cs
:160-176) for every weapon: the KESTREL's and THUNDERCLAP's charge, the PULSE gauntlet's kill pips (0-5) and its 15 s blast cooldown, the
STITCHER where it is held (survival). It fills with the charge fraction (eased: fills that speed up near the end read faster,
feel-web.md section 4), drains through a cooldown, bone while filling, cyan at full with the 0.35 s white flash; Destiny 2 moved its
fusion-rifle charge meter into the reticle under the centre (a snippet). Start: a 90 deg arc, radius ~44 px, 7 px thick at 1080p (a label,
if any, >= 16 px). Also the KESTREL's crosshair spin re-timed to W1's numbers. Proof: frames of the arc at 0 %, 50 %, full and in a
cooldown on each weapon (one reel shared with W1); the old bars back with arc40 off. Owns UiHud38*.cs, UnityTuning.Hud40.cs, design.n40.arc40.

**R2 the ride (builder R2, ~230 min).** Asks 11, 12, 17. Owns Core/Mover.cs (inside the wallride35 branch only), Ride35.cs, Ride38.cs,
design.n40.ride40, and **the checker's ride move tonight** (Harness.N38.Gates.cs: an endless ride may cross a gap no ride crossed before,
L-042 / L-061).
- **First measure what GG feels** (L-063): the height lost over 3 s of a tap ride (ends at 1.2 s) and of a hold ride (sinks 0.1 m/s after
  a 0.35 s grip); ride38's walk deaths (2 -> 9 in NIGHT38's level-1 room a5; level 1 is now n38-foundry: re-measure there first).
- **ride40, the ride holds (D-9):** ride38's hold ride (no sink, the look pitch past +-15 deg steers up / down at up to 2.5 m/s, never past
  the wall's top) for every ride: it ends only when the player leaves (jump = the jump-off, crouch, steering away), the wall ends, or the
  feet land; a 60 s guard against a stuck state (the MIT Godot controller's "infinite run" ends the same way, feel-web.md section 2).
  Crisp: the attach settles in <= 0.1 s; camera roll ~7.5 deg (a student Titanfall-style build).
- **The attach asks for intent:** airborne at least ~1.0 m above the ground below, horizontal speed >= ~4 m/s, the move input toward or
  along the wall (dot >= 0.1), a wall at least ~2.0 m tall above the feet (risers and platform faces are shorter), attachReach 0.5 -> ~0.25
  m, a 0.2 s lockout on the same wall (feel-web.md section 1; each a hypothesis). GG once called the old attach "almost like you snap on
  to it" (w29): keep a deliberate jump at a wall a sure attach. Proof: a walk-bot count of attaches on surfaces under 2 m tall or with the
  ground within 1 m, levels 1-5 (before vs after: 0); THE GANTRY walk 10/10; every ON level's gates GREEN with the new ride move (the lead
  re-runs them all after R2 lands); verbs rows 9 / 11 / 44 PASS in the Player.

**L2 level 2 (builder L2, ~230 min).** Asks 13, 14. A new level 2, `n40-iceline` (the ice canal at dusk), written from a beat sheet first
(one job per room, a job tag on every solid, L-057; `Design/levels/n40/iceline.beats.md`; n38-iceline's j1-j4 sheet is a start), small
rooms with a movement line and a climb route for enemies (PRINCIPLES 4). The gauntlet's curriculum (teach -> practice -> twist -> test):
(1) TANK ALONE: a large enemy and nothing else: the punch staggers what guns barely scratch; (2) THE SHIELD: a WARDEN: punch the shield
(a 1 s stun), or get round it; (3) the swarm: swarmers: save the PULSE blast for the group; (4) the reach: a DRONE across a gap: the
gauntlet cannot reach, so the parry sends its yellow orb back; (5) the cooldown: a room that spends the blast early and punishes it with a
second group (manage the 15 s); (6) **the armoury**: the KESTREL returns on entering a zone (the row's `awards`), then a mixed room where
switching wins; (7) the rehearsal (a CHARGER: dodge, then punch its back); (8) B2's arena. iceline40's `startSet` is the gauntlet alone.
Proof: n36-jobs GREEN; gates on E's checker for any traversal gate; the walk bot 10/10 (L-045, with E3's combat plan for the three kinds,
L-013); the verbs reel on n40-iceline; a frame per room. Owns gen-n40-iceline.cjs, Design/levels/n40/iceline*, design.n40.iceline40,
Bot/Walk40Iceline.cs.

**B2 the brawler (builder B2, ~230 min).** Ask 16. THE STAG leaves level 2 with iceline40 (its code stays, D-8); a new boss in L2's last
room, working name THE ICEBREAKER (D-3), gauntlet-led, by boss_rules. Damage by range: ranged hits scale from x1 at <= 4 m to ~x0.2 beyond
12 m (the Marauder is open only at ~3-4.5 m; the Hideous Mass's shell ignores all but melee; V2 enrages at range); the gauntlet hits full
and builds stagger. Attacks (a start): the haymaker (heavy, >= 0.9 s glow, then ~1 s stuck: the punish); the slam (a shockwave ring: jump
or ride over it); a thrown ice block when the player stays far (YELLOW: parry it back for a stagger); a rush when the player is past ~15 m.
Phase 2 at 50 %: faster chains, the ice cracking into floes. A compact arena (~30-40 m) with ride walls on its rim and pillars to break
line (a new arena json). A jointed blocky model built in code (big fists, small head, hunched: enemies-web.md section 4) with a pose per
state (wind-up, strike, stuck, stagger, slam, phase). Length by boss_rules 7; the pistol from range alone takes >= 3x a mixed fight's time.
B2 does not edit Boss37.cs (B4 owns it tonight): it copies what it needs. Owns Core/Enemies/Brawl40.cs, SurvivalSim.Brawl40*.cs,
Brawl40View.cs, the arena json, design.n40.brawl40, Bot/Brawl40Pilot*.cs, its reel scenario.

**B4 THE LOOM (builder B4, ~230 min).** Asks 7-10. Owns Loom37*.cs and Boss37.cs (additive changes only: new states appended; logic open
tonight by GG's request), Loom37View.cs, NoRide39View.cs (its runtime switch), gen-n40-span.cjs + Design/levels/n40/span*, design.n40.loom40,
Bot/Loom37Pilot*.cs.
- **Level 4 is the grapple's (n40-span from n38-span):** the STITCHER's pickup, award, lesson and teach line [13] leave p1; p1 becomes a
  grapple room; o2's practice knee is torn by the grapple; loom40's `dropWeapons: ["nailgun"]` keeps it out of levels 4-5 (survival keeps
  it, D-7). The STITCHER leaves only with loom40, so the LOOM is never left needing nails nobody has.
- **The tear by the grapple:** a knee tears by latching and pulling away (the existing pull-away tear), no nails (Doom Eternal's Khan
  Maykr: hook + punch about six times); each tear staggers; the CYAN window takes every weapon.
- **Stays in the canyon** (boss_rules 4): clamp stations and body to o3's rect with a margin, restore a station after an anti-hide, cap
  the glide (36-40 m/s today) to a readable speed with an ease, the death fall lands on the canyon floor. Root-cause line first (frames
  outside the room over 10 seeds today), then 0.
- **Tells and animations:** every attack by boss_rules 1 (the spool YELLOW); a pose per state (wind-up, strike, recover, stagger, tear,
  reposition), not only the bob.
- **No-ride zones (D-12):** phase 1 marks about a third of the canyon's ride walls `noride`, phase 2 about two thirds, switched at the leg
  change behind a 1 s warning flash (a runtime switch of the per-run noride set; a grapple anchor always in reach); the NoRide39View look
  ON for these walls and seen on screen. Cues that push grapple, dodge and reposition: sweeps that clear the floor, an attack on the wall
  you ride, anchors that glow when a knee is open.

**B5 THE CROWN (builder B5, ~210 min).** Ask 20. Owns Crown37*.cs, Orbit37.cs, Crown37View*.cs, the boss-line generalisation (a new
BossLine40, reused by B2 and B4), gen-n40-spire.cjs + Design/levels/n40/spire*, design.n40.crown40, the crown pilots.
- **First the bugs GG hit:** from G0's crown37 frames (both pilots) list every defect with its line (the anti-hide snaps onto the player,
  Crown37.cs:407; no clamp).
- **Teach the pass** (boss_rules 5): an entry line ("FLY THROUGH THE RING: {dash} DASH + {jump} JUMP"), v10 THE RING's empty prompt
  filled, the ring lit cyan when the player's speed reaches 12 m/s, a gem counter, "BLOCKED" on gun hits outside the window, "OPEN:
  SHOOT IT" in the window.
- **Better attacks** by boss_rules 1-3; arena changes (in n40-spire) only where the next step is unclear. Gems (20 since boss39) tuned by
  boss_rules 7. Verbs row 47 stays.

**E3 the campaign roster and every enemy's look (builder E3, ~230 min).** Asks 15, 19. Owns Drone.cs, Warden.cs, Charger.cs (their
campaign rules behind foes40), design.n40.foes40, Bot/Foes40Combat.cs (the walk bot's answer to each: L-013), the TTK harness,
Game/EnemyViews*.cs, Enemy40View*.cs, UnityTuning.Models40.cs (models40).
- **foes40, three kinds into the campaign** (L2 places them; levels 3-5 keep their proven waves, D-5): numbers from enemies-web.md with
  their sources. The **DRONE** is the deflect enemy: a YELLOW orb (ULTRAKILL: yellow = parryable) after a 1.0 s glow (today 0.4 s),
  slower (~14 m/s start; the spitter's glob is 16 m/s), an approach at ~0.6x the husk's speed (ULTRAKILL's Stray 0.61x walk); a reflected
  orb kills or staggers it. The **WARDEN** is the gauntlet's: its punch-stun stays the answer, a flank the other. The **CHARGER** is the
  movement and shotgun enemy: jump or ride its charge, then its stunned back (x3). An attack token: at most two winding up at once (Doom
  2016's pool). Proof: a time-to-kill matrix kind x weapon (each kind's counter >= 1.5x faster than the others); the walk bot 10/10 on
  n40-iceline with Foes40Combat; one reel scenario lining up the three with their tells (frames).
- **models40, silhouettes** (enemies-web.md section 4: a different outline in black per kind, the defining part on head / shoulders /
  back, squares for tanks, wedges for fast or dangerous, the brightest value at chest height, a distance rim light against the dusk sky, a
  weak point as a separate box): the DRONE, WARDEN, CHARGER, then husk, swarmer, spitter, tank (cut from the end if late). A pose per
  state (move, wind-up = the tell, hit flinch, death); red-orange only for danger. Proof: a silhouette sheet from the lineup reel (each
  kind black at 30 m) that a fresh subagent with no labels names correctly for at least 6 of 7; the renderer count per model at most 2x
  today's (an EditMode test the lead runs).

**Lead L0.** G0; integration (cherry-pick, golden, fingerprint plain and `--off40`, compile, the touched tests); a build per round after
its last commit (L-038); every reel and Player check (builders never run one); frames of every view row before it turns on; LevelSelect29
after any round that touches a level view (L-043); after each reel's verdict, the looked-at frames copied to Logs/shots/n40 and the rest of
that reel's frames deleted (own output, CLAUDE.md rule 10: disk); the round-4 Player block; the test hour; the docs. The lead alone:
SurvivalSim*.cs other than builders' partials, SurvivalGame*.cs, Campaign24's core, Curriculum25, Unlocks26, the tape, FingerprintBaseline,
Walk29Pilot, FeelReel.N35.Verbs.cs, every build / reel / suite, git.
</cards>

<builders>
Nine builders in worktrees, launched the minute the shared commit is green (~0:30). NIGHT39 ran seven and NIGHT38 eight; the ninth is the
price of one builder per boss and per level (L-060). Rules in NIGHT40-BRIEF.md (NIGHT39-BRIEF's section 2): git merge port/stage3 first and
before every report; `dn-check-wt.sh` (the lead tests it once with a planted error, L-034); fingerprint and `--off40` EQUAL before each sim
commit; golden before each Core commit; never the Editor, a build or a Player; config scripts patch only their own row (L-049); reports
under 300 words naming the frames the lead should take; resumed by SendMessage (L-033). Cards are sized to end by the round-4 build
(4:20); round 4 is fixes from the Player only.

| Builder | Owns (details in its card) |
| --- | --- |
| **S1** start + level 3 + sweep | SurvivalSim.Start40.cs, design.n40.{start40,kiln40,cut40,rods40}, n40-kiln files, UiLevelSelect26, the results line, decor filters |
| **W1** weapons | Charge35.cs (shotgun hold), ChargeShots27 slug look, design.n40.charge40, Pistol39View timings, Viewmodel pump hooks, UnityTuning.Pistol40 / Pump40 |
| **H1** arc | UiHud38*.cs, UnityTuning.Hud40.cs, design.n40.arc40 |
| **R2** ride | Mover.cs (wallride35 branch), Ride35.cs, Ride38.cs, design.n40.ride40, Harness.N38.Gates.cs (the ride move) |
| **L2** level 2 | gen-n40-iceline.cjs, Design/levels/n40/iceline*, design.n40.iceline40, Bot/Walk40Iceline.cs |
| **B2** brawler | Core/Enemies/Brawl40.cs, SurvivalSim.Brawl40*.cs, Brawl40View.cs, its arena json, design.n40.brawl40, Bot/Brawl40Pilot*.cs |
| **B4** LOOM | Loom37*.cs, Boss37.cs (additive), Loom37View.cs, NoRide39View.cs, n40-span files, design.n40.loom40, Bot/Loom37Pilot*.cs |
| **B5** CROWN | Crown37*.cs, Orbit37.cs, Crown37View*.cs, BossLine40, n40-spire files, design.n40.crown40, crown pilots |
| **E3** roster + looks | Drone.cs, Warden.cs, Charger.cs, design.n40.foes40, Bot/Foes40Combat.cs, TTK harness, EnemyViews*.cs, Enemy40View*.cs, UnityTuning.Models40.cs |

**The spend limit** (L-003): it stops every agent at once. Every builder commits each finished slice; if agents stop with 429s, the lead
logs the time, keeps integrating and building alone from the cut order, and retries a builder at most every 30 minutes.
**Keep first:** start40, ride40, arc40 + charge40, the LOOM's clamp + loom40's STITCHER removal, the CROWN's teaching, cut40, kiln40.
**Cut in this order:** E3's old-kind models (husk, swarmer, spitter, tank keep theirs), rods40 beyond levels 1 and 3, B5's arena changes,
B4's extra poses, the slug's new look. L2 + B2 ship OFF together (n37-iceline and THE STAG stay) unless walked 10/10 and fought by both
pilots in the Player in round 4.
</builders>

<lean_rules>
- Golden at every Core commit; Mover.cs only inside the wallride35 branch (R2); JsMath; Mulberry32 seeded per run; whole-step timers.
- Numbers: gameplay in design.n40.<row> with a `_why` (both config copies byte-identical); render / UI in UnityTuning.*40.cs, each with a
  comment. Never edit design.n35-n39 rows or n37 / n38 level files in place: override in design.n40 and new n40 level ids.
- Caps (report short instead): builds 7; reels 38 (G0 4; rounds 1-3: S1 2, W1 + H1 3, R2 2, L2 3, B2 3, B4 3, B5 2, E3 2; round 4: verbs
  on levels 1-5 and the three bosses with both pilots, 11; retakes 3); Player checks (LevelSelect29, smoke) 5; suite 1 (batch, first in
  the test hour, >= 8 GB free memory, nothing beside it); builders 9; no build or reel while C: is under 10 GB free.
- Commits with explicit paths; never GG's four files (L-047: read a diff before any `git checkout --`); nothing deleted but the lead's own
  reel frames; no audio (event names only); Play mode only by the lead; close every Player by its PID (L-054).
- Docs with Write / Edit only (rule 9); every lead message ends with the status line (closing_reminder).
</lean_rules>

<tasks>
## U3-53.1 Start + G0 (0:00-0:30)
`date`; G0 (a)-(e), the shared commit first; NIGHT40-LOG.md opened with each line; launch the nine.
## U3-53.2 Build rounds (0:30-4:20): integrate at about 1:30, 2:30, 3:30
Round 1 (1:30): every builder's root-cause line and first slice (S1's grace, W1's charge numbers, R2's measures, L2's beat sheet, B2's
brain skeleton, B4's clamp, B5's defect list, E3's DRONE retune), build n40-r1, frames. Round 2 (2:30): L2 walkable, B2 fighting in the
harness, B4's grapple tear, H1's arc, R2's attach + the gates re-run; frames. Round 3 (3:30): every row's frames, one iteration each, rows ON.
## U3-53.3 Round 4 in the Player (4:20-5:00)
Build n40-r4 after the last commit; the verbs on levels 1-5 (the after); B2 / B4 / B5 with both pilots; LevelSelect29; last fixes; the
freeze commit at 5:00 (a new build only if code changed after r4).
## U3-53.4 Test hour (5:00-5:45)
Free memory checked, then the batch suite alone; golden; fingerprint `--off40` EQUAL; re-runs only for levels changed after r4; the
start-grace frames; NIGHT40-PROOF.md with every number from tonight's runs.
## U3-53.5 Close (5:45-6:00)
NIGHT40-LOG close table (G0, S1, W1, H1, R2, L2, B2, B4, B5, E3, L0: DONE or SHORT with a measured reason); NIGHT40-REPORT.md (GG's page:
his 22 asks, one row each: ON / OFF, the switch, how it was checked); FEEL-PLAYTEST.md (a 10-minute pad-first walk: level 3's start, the
arc, a wall ride, level 2 to its boss, THE LOOM, THE CROWN); DECIDE40 answers; HANDOFF; PORT-STATUS events; tag u3-n40. The memory
harvest may follow in a fresh session.
</tasks>

<leftovers>
1. Any card that closes SHORT (it carries, with its measured reason).
2. Level 6 rebuilt from scratch (GG: later), with THE RIVAL (DECIDE37 D-1); the level-6 files' `git rm` after GG's yes (D-4).
3. "heavy" given a real kind on levels 3 and 5 (D-5: the web calls the CHARGER "the heavy").
4. The three kinds placed on levels 3-5, each with that level's walk re-run 10/10.
5. The NIGHT39 rows still OFF: doors38, juice38, stag38; the LANCER (a ported sniper) as a fourth campaign kind.
</leftovers>

<closing_reminder>
- Feel is done on the screen (L-001); a boss is done in the Player with both pilots (L-029, L-058); a level is done when the walk bot
  clears it 10/10 and the verbs pass (L-045, L-052).
- `--off40` is NIGHT39's game: new level ids, override rows, never an in-place edit of an old level or row.
- Every lead message ends with `N40-STATUS: round <r> · builders <alive>/9 · rows on <n>/14 · <h:mm> / 6:00`.
</closing_reminder>

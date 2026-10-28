# NIGHT43.md: the cohesion night: GG's n42-r4 playthrough to level 3, thirty asks; foundations first (the ride, the wall-jump count, the boost combine, doors that open), then the levels, the enemies, the boss, the UI and the feel rebuilt on top, under written design rules (lead + nine builders; ~4 h build, ~1 h Player, close)

GG played n42-r4 to level 3 on 2026-10-16 and sent one long direction, verbatim with a row map in Research/2026-10-16-wave43/GG-REQUEST.md
(G1-G30). He asked for research ("ULTRAKILL, other boomer shooters, movement shooters, arena shooters, and relevant open-source projects"),
foundations before content ("fixing wall-running consistency before trying to tune every wall-running encounter"), written rules so we
"stop forgetting", and "significant actual building". Today's numbers: Research/2026-10-16-wave43/CODE-FACTS-A.md (movement, level 1, doors)
and CODE-FACTS-B.md (weapons, bosses, enemies, UI, level 3). Research with URLs: Research/2026-10-16-wave43/research/{movement-web,
combat-web, encounters-arenas-web, ui-art-web, bosses-web}.md. The rules every card follows: docs/DESIGN-RULES.md (the lead writes it during
the build from that research; it extends docs/PRINCIPLES.md) and DECIDE43.md (GG's calls, each a default).

Base: port/stage3 at the NIGHT43 shared commit a9da30f6 (on u3-n42 + 1623ae82). t0 = 16:00 (GG's message). Builders start ~16:50.

<goal_for_the_night>
Every one of GG's thirty asks is built and seen on a Player frame, or SHORT with a measured reason. The ride is smooth and forgiving and
its jumps never silently stop working; jump + shot boosts high in a forgiving window; a door always opens when its room is empty and a small
counter says how many are left; level 1's GANTRY is a lava wall-ride with dash gaps, THE FLUE carries you up, PIERCE teaches before it
spawns, the boss door is clean; THE WICK says what hurts it and when; the gauntlet swings at 1.25 s and parries orbs from farther; a
high-speed gauntlet hit is a FUSION you feel; the enemies are pills with faces; level 3 starts readable, teaches the boost small then big,
and has flyers, chargers and BURROWERS; the HUD, teach cards, shop and results have one visual language; and docs/DESIGN-RULES.md holds the
rules. Nothing GG had is lost.
</goal_for_the_night>

<method>
1. **Shape:** shared commit 16:20 (done: a9da30f6); builders 16:50-20:00; integration at ~18:00, ~19:00, ~20:00 (cherry-pick, gates, one
   Player look per round); round 4 in the Player 20:00-20:40 (verbs on levels 1-3, the smoke, each row's frames); freeze ~20:40; close ~21:10.
2. **Feel is measured and seen (L-001):** every row names its number (harness / telemetry) and its reel frames before it is built. A row
   turns ON only after the lead looked at its before / after pair and one iteration; builders never set `enabled: true`.
3. **Every change behind a switch; `--off43` / `-skygrindOff43` is NIGHT42's game.** Level edits go to n43-foundry / n43-iceline / n43-kiln
   (placeholders in the shared commit), swapped in by their level rows.
4. **Foundations first (GG):** M1's chimney43 / ride43 / boost43 and E1's rooms43 land in round 1 (by ~18:00); L1, K1 and B1 build on them
   (forced on in their own harness runs from the start: they read M1's and E1's numbers from the cards below, then re-check at round 2).
5. **Mover.cs is named tonight** (CLAUDE.md rule 3) for builder M1 only, behind chimney43 / ride43 / boost43; golden 0 mismatches and the 22
   replays exact at every Core commit.
6. **Nothing lost:** VERBS.md rows touched are marked (9-14 the ride and wall jumps; 6-7 slide / slam (B); 5 dash; 28 the gauntlet; 29-30 the
   THUNDERCLAP boost; 40 the parry); verbs on levels 1-3 at round 4.
7. **One builder per area, end to end;** builders never run the Editor or a Player; the lead cherry-picks (no `git merge`).
8. **GG's message outranks CLAUDE.md (org rules, then GG, then CLAUDE.md):** G21 rebuilds the enemy models (CLAUDE.md rule 7 said "the
   web's blocky models rebuilt as they are"; GG now asks for the overhaul: the silhouettes stay pills); G19's hot pink is a new FUSION colour
   (rule 8 kept magenta for RIVAL: the FUSION pink is a hotter, whiter pink in a burst shape, never a body colour; DECIDE43 D-3). Sound
   stays GG's (rule 7): event names only.
</method>

<cards>
**M1 movement (builder M1).** G3, G13, G2, G26. Owns Core/Mover.cs (behind chimney43 / ride43 / boost43 only), MovementMath.cs additions,
Ride35 / Ride40 hooks, Core/Boost42.cs + a new Core/Boost43.cs, Charge35.cs's THUNDERCLAP ground-press rule (additive), design.n43.{chimney43,
ride43, boost43}, SurvivalSim.N43.Move.cs, Harness.N43.Move.cs, FeelReel.N43.Move.cs.
- chimney43 (G13 / G2): today a pressed wall jump spends one of 3 per airtime, refilled only by a landing (CODE-FACTS-A A2); THE STACK's
  six-panel sections and any chimney run out and the press silently becomes the double jump. New rule (movement-web.md): a wall jump off a
  DIFFERENT wall (normal dot < 0.9, or > 1.5 m from the last wall-jump point) refills the count; the same wall still spends (no infinite
  climb on one face); a ride start on a new wall refills it too. Harness: a scripted chimney (two facing walls 4.5 m apart, 14 m tall): the
  height reached with presses at each wall, before / after; THE STACK's panel chain (n37 wick) climbed by presses.
- ride43 (G3): the ride's feel: (a) the attach: the entry probe swept 0.45 m (stay probe 0.12 m), the attachPull draw also on a tap ride;
  (b) the snap: wallgrip36's 1.5 m/s rise clamp is a hard stop: ease it (keep half the rise, then brake by gravity); (c) lostGrace 0.05 ->
  0.12 s; (d) **B after a ride**: within 0.3 s of leaving a wall B is not a slam (CODE-FACTS-A A3: a slam sets (0,-48,0) and kills every
  m/s): it is a drop that keeps the horizontal speed (a slam only after 0.3 s or when looking down > 45 deg); B on a ride stays the drop;
  (e) the ride40 intent with jump held needs no stick into the wall (moving along it is enough); (f) the 0.2 s lockout only for the same
  wall; (g) a dash ending against a wall attaches (DOOM Eternal's climb walls). Each sub-rule its own number with a _why. Harness: the
  share of scripted approaches (angles 10-60 deg, speeds 6-16 m/s) that attach, before / after; the m/s kept by a B press 0.1 s after a ride.
- boost43 (G26): (a) the combine window: a THUNDERCLAP press on the ground within 0.15 s BEFORE a jump fires on the jump's step in the air
  (today it is a ground tap: no boost, CODE-FACTS-B G26), and a shot within 0.15 s AFTER leaving the ground boosts in full; (b) a boost
  never gets halved by the jump-cut (releaseMult 0.5; Boost42Launch disarms it: make sure every boost path does); (c) boost42's lift by
  pitch (11 m/s at 40 deg or steeper) as boost43's default, with a measured choice of 11-12. Report the heights to the lead at once: one
  boost from a standing jump, boost + double jump, two boosts (K1 builds kb1's steps from them). Harness: the apex of jump + shot at offsets
  -0.15..+0.15 s, before / after; level 3's kb1b G1 gate with boost43 on.
- Proof reel `move43`: a chimney climb (5+ presses), a B press 0.1 s after a ride (the speed kept), a jump + shot boost (apex frame).

**L1 level 1 (builder L1).** G1, G2, G4, G11 (+ G9's rules on level 1). Owns Tools/night/gen-n43-foundry.cjs, Design/levels/n43-foundry*
(+ its plan), design.n43.foundry43, Harness.N43.Level.cs, FeelReel.N43.Foundry.cs. Builds on M1's rules (force chimney43 / ride43 on in
your harness runs) and E1's room fields (`afterLesson43`, `fallReturn43`, below).
- THE GANTRY (G1): rebuilt over LAVA (the pits' floors are lava: a fall costs health and returns you to the lip by E1's fallReturn43, never
  a long walk): readable, satisfying, still skill: the first span a single long wall (learn the hold), then alternating walls with the next
  wall in view and at most ~0.5 s of flight between walls at ride speed (9-13 m/s), walls overlapping >= 4 m, one DASH GAP (a missing
  wall the player dashes across, 6 m at 24 m/s) taught by a centre card ("{dash} ACROSS"), the lane lit cyan where it is rideable. Dash
  is taught in a1 before any room needs it (today's l1.dash line + its trench; check it shows).
- THE FLUE (G2): a chimney: two facing ride walls ~4.5 m apart and ~12 m tall, continuous (no 1 m overlaps; CODE-FACTS-A B6), so a bounce
  wall to wall carries you up (chimney43 refills each new wall) and a held ride up works too; the top deck leads forward only: the climb
  behind the deck is closed off when the room clears (a rail / shutter; G11), and the exit is lit.
- PIERCE (G4): no floating stand-ins: the lesson in order: enter -> "HOLD {jump} ON A WALL" -> the ride is performed -> the enemies arrive
  (a wave with `afterLesson43`, spawning with spawn33's tells from a believable place: rising out of the pit on a lift, or dropping from a
  hatch) -> "HOLD {altFire}: SLOW. {fire}: PIERCE". Its exit (d10) is a straight approach (B1 owns the door's look and the tunnel).
- G11: every cleared room's way back is closed or one-way (rooms whose exit is high get a drop gate; THE FLUE above); the next door is lit.
- Harness: the gates with the checker's margins on n43-foundry with chimney43 / ride43 on; the walk 10/10; the share of scripted gantry
  crossings that succeed (first try, second try). Proof reel `foundry43`: THE GANTRY from the lip and mid-chain, the dash gap, THE FLUE
  mid-climb, PIERCE's three beats, the door to THE STACK.

**W1 weapons (builder W1).** G17, G18, G19 (sim). Owns the gauntlet's cycle override, Core/Parry42.cs + a new Core/Parry43.cs, the
drone-orb / glob reflect volume, a new Core/Fusion43.cs, ImpactRules' fusion tier (additive), design.n43.{gauntlet43, parry43, fusion43},
SurvivalSim.N43.Weapons.cs, Harness.N43.Weapons.cs, FeelReel.N43.Weapons.cs. Combat.cs: one-line hooks only (M1 also touches it).
- gauntlet43 (G17): the swing 1.8 -> 1.25 s (the row holds 1.25); measured: press-to-next-swing in steps, DPS, THE ICEBREAKER's bot band
  re-run (must stay inside 40-70 s; report it), impact42's per-minute freeze cap still sane.
- parry43 (G18): today ~0.11 s from the eye point, no buffer on the gauntlet (CODE-FACTS-B G18). New: parry42's surface-to-body reach and
  mercy, plus a catch volume ahead of the player (ULTRAKILL: a 3 m sphere 3 m ahead; ours sized to the gauntlet: radius ~1.5 m centred
  ~2 m ahead of the eye along the aim), a 0.15 s early buffer on the gauntlet swing too, and a window measured in time-to-impact >= 0.2 s
  at the orb's 12 m/s. Measured: the share of scripted swings that reflect an orb fired from 8 m, by press time before impact, before / after.
- fusion43 (G19): a gauntlet hit or punch landed at high speed (player speed >= ~14 m/s, or during a dash / within 0.25 s after one, or a
  ride jump-off) is a FUSION: x2 damage (name your number), a knockback and a 0.6 s stagger, a hitstop tier above the gauntlet's (0.22 s;
  boss 0.28 s), and the event N43Events.Fusion with Fusion43Event filled (F1 draws it). A dash INTO an enemy with the gauntlet in hand
  should make the hit easy to land (e.g. the swing's reach +1 m while dashing). Measured: fusion share of gauntlet hits in a scripted
  dash-in, damage, the stagger.
- Proof reel `weapons43`: a DRONE orb reflected at ~3 m, a FUSION (dash into a husk + the gauntlet) at +0 / +0.1 s.

**F1 feel (builder F1).** G19 (view), G20. Owns Game/GunFx*, ImpactFrames (additive), CameraFeel / PostFx additions, Game/Input/Rumble.cs +
Haptics22View additions, a new Game/Fusion43View.cs, design.n43.feel43 (view row), UnityTuning.Feel43.cs, FeelReel.N43.Feel.cs.
- FUSION (G19): two forces combining: cyan sparks (the player's) and orange-red embers (the enemy's) pulled into one point, then a hot-pink
  / white core flash and a ring that blows out; a 2-3 frame crush, a camera kick and roll, a chroma kick, the HUD's word "FUSION" punched in
  (U1's style language: ask the lead for U1's font / colour tokens), a sharp rumble crack (low 1.0 / high 1.0 for ~0.12 s, then a fading
  thud). Built on combat-web.md's numbers (trauma^2 shake, +0.6 for an empowered impact).
- G20: gunshots and hits: bigger hit sparks per weapon, a hit-flash that reads; the THUNDERCLAP charged slug: a thick tracer, an
  explosion at the impact, the enemy thrown; the TORCH: a richer flame (the pool raised for it, a hot core + smoke, light on walls), a held
  rumble buzz while firing; rumble per weapon on fire (light) and on charge release (heavy); a boss hit while it is OPEN (N43BossState.open)
  gets the strongest hit read.
- Measured: the frame difference (screen change) per event vs NIGHT42 (L-001), the rumble log per event (Haptics22View.Log), the
  particle counts. Proof reel `feel43`: a FUSION at +0 / +0.05 / +0.15 s, a slug kill, the TORCH firing.

**U1 UI (builder U1).** G5 (view), G6, G7, G14 (view), G24. Owns UiHud33* (the health bar, the counter, the theme), a new UiTeach43 (the
centre card), UiShop33 / UiCampaignResults25 visuals, UiBossBar41 (its states), a backdrop blur (a new shader + view), UnityTuning.Ui43.cs,
design.n43.{hud43, counter43, teach43, screens43, bossbar43}, FeelReel.N43.Ui.cs.
- First write the HUD's visual language into Research/2026-10-16-wave43/U1.md (type scale, the three fonts' roles, colour tokens, panel
  shape, motion rules) from ui-art-web.md; send it to the lead early (F1 and A1 use its tokens; the lead folds it into DESIGN-RULES).
- hud43 (G6): the health bar rebuilt to look intentional (ui-art-web.md: a segmented bar, a 0.15 s after-image that drains, the frame
  pulsing on a hit, the number in the display font); the HUD's panels in one shape language; nothing filled-box (w33's "never again").
- counter43 (G5): a small "N LEFT" in a corner while a locked room fights (SurvivalSim.N43RoomLeft; -1 = hidden), ticking down with a
  small punch, gone when the door opens.
- teach43 (G7): the centre card (ULTRAKILL's HudMessage): the bound glyph + a few words, centred above the crosshair (never on it), opens in
  ~0.16 s, holds ~4-5 s or until the verb is done, once; it replaces teach35's top slot and text36's lower slot while on (the lessons'
  text unchanged).
- screens43 (G24): the shop and the results over a blurred, darkened game (ui-art-web.md: a Kawase blur of the last frame; a one-shot
  capture is enough), information in panels with a hierarchy (the title, the main number, the details), the palette used on purpose, a
  staggered entrance animation.
- bossbar43 (G14): the boss bar from SurvivalSim.N43BossState: HP with an after-image, the lock tick (LOCKED: the bar's colour and a short
  "SHOOT THROUGH A CRACK"-style hint), OPEN (the bar bright, a timer), STAGGER, the leg "1 / 3"; shown for the whole fight (B1 fills it).
- Proof reel `ui43`: a hit and a heal on the bar, the counter in a locked room down to 0, a centre card, the shop, the results, the boss bar
  in each state.

**E1 encounters (builder E1).** G5 (logic), G22, G4 (logic), G23, G19 (level 2 lesson). Owns Core/Campaign24.cs, SurvivalSim.Campaign24.cs,
Level36 / spawn33 additions, HuskBrain's watchdog, Husks' kind map ("heavy"), Tools/night/gen-n43-iceline.cjs + Design/levels/n43-iceline*,
design.n43.{rooms43, iceline43}, SurvivalSim.N43.Rooms.cs (incl. N43RoomLeftCount), Harness.N43.Rooms.cs, FeelReel.N43.Rooms.cs.
- **First slice (by ~17:40, L1 and K1 wait on the names):** two room fields, read only with rooms43 on:
  - a wave's `"afterLesson43": <lesson index>`: the wave waits until that lesson is done (then spawns with its spawn33 tells);
  - a room's `"fallReturn43": {"belowY": <y>, "to": [x, y, z], "damage": <hp>}`: feet below belowY inside the room -> back to `to`, the
    damage taken, a short fade (no death).
- rooms43 (G5): a room's door counts ONLY the enemies it spawned (today EnemiesAlive is the whole level; CODE-FACTS-A B9); an enemy out of
  its room's bounds, under its floor or in a dry pit for > 2 s ends (counted dead, no points); a verb lesson can never hold a door shut
  after the room's enemies are dead (a time cap, then the lesson is marked done); the top-up loop capped so the door is never held by
  re-spawns; N43RoomLeftCount answers the locked room's count (U1's counter); the event N43Events.RoomClear. A harness that clears every room
  of levels 1-3 with every kill path (pit knock, lava, stuck) and asserts each door opens.
- G22: "fail to move": the watchdog relocates a stuck enemy ON screen too (a short tell, then a step to the nearest free node) after ~2 s;
  the melee token ring reviewed (no enemy stands still > 3 s while the player is in reach); 'heavy' maps to a real heavier kind (name it;
  today a plain 40 HP husk, CODE-FACTS-B). Measured: max still-time per enemy over the walk bot on levels 1-3, before / after.
- iceline43 (G23 / G19): n43-iceline: two more CHARGER rehearsals before THE ICEBREAKER (a charger with a husk escort; a charger beside a
  wall to stun it into), and the gun-again room teaches dash + gauntlet: "{dash} INTO IT. {fire} THE GAUNTLET." (W1's FUSION).
- Proof reel `rooms43`: a locked room cleared to its door opening (the counter 3 -> 0), a pit-knocked husk ending, a level 2 rehearsal.

**K1 level 3 (builder K1).** G25, G26 (the teaching), G27, G28 (placement + THE RING). Owns Tools/night/gen-n43-kiln.cjs, Design/levels/
n43-kiln*, design.n43.kiln43, WyrmArena's room geometry, Harness.N43.Kiln.cs, FeelReel.N43.Kiln.cs.
- G25: kb1 rebuilt: no ramp without a reason (CODE-FACTS-B: the 13 m wedge lets you walk past the lesson), every piece with a gameplay
  reason written beside it in the generator; the path readable from the door (lit landings).
- G26: the boost taught small then big: three steps (heights from M1's boost43 report; today's estimate: ~2.5 m one jump, ~4.5 m jump +
  boost, ~7 m jump + boost + double jump), then kb1b's climb. Force boost43 on in your harness.
- G27: compositions by encounters-arenas-web.md's rules (introduce, reinforce, combine, escalate; one new role per room, <= 3 roles before
  level 4): DRONES enter level 3 (none today), chargers (the mini-ICEBREAKER language), A1's BURROWERS in two rooms (introduce alone, then
  combined); rosters by role, not HP.
- G28: THE KILN WYRM's ring (k5) redesigned as a place: its silhouette, light, a landmark, readable lava, enemy-free lanes (with A1's look43
  hints if they land).
- Harness: the gates (n38 gates) and the walk 10/10 on n43-kiln; the boost steps passed by a scripted player with boost43 on / off. Proof
  reel `kiln43`: kb1 from the door, a boost step, a burrower room, THE RING.

**A1 enemies' look and the BURROWER (builder A1).** G21, G23 (look), G28 (the enemy), G8 / G29 (look43). Owns Game/EnemyViews*, Enemy40View,
a new Game/Enemy43View*.cs, UnityTuning.Models43.cs, a new Core/Enemies/Burrower43.cs (+ its registration line in Husks' kind map: one line,
E1 owns the rest of that file), its view, design.n43.{burrow43, models43, look43}, SurvivalSim.N43.Burrow.cs, Harness.N43.Burrow.cs,
FeelReel.N43.Burrow.cs.
- models43 (G21 / G23): every kind rebuilt in the pill language (ui-art-web.md: class in the top third: a visor shape + one back part;
  colour, then silhouette, then detail): husk (a slit visor), swarmer, spitter (a round lens + a tank on the back), charger (a heavy brow
  visor, shoulder plates: the mini-ICEBREAKER), drone, warden (a shield with a frame), tank; orange / red / yellow kept with one value
  hierarchy (dark body, bright visor, yellow only where parryable); smooth meshes (capsules, lathed / procedural shapes), not boxes. A
  line-up frame of every kind. Keep the hit flash, dissolve and DANGER / PARRY roles readable.
- burrow43 (G28): the BURROWER: hunts the player's ground contact under the floor (a raised trail, dust), a 0.8 s tell under the player
  (a red-orange ring + rumble), then it bursts out (damage + a launch if you are on the spot) and stays exposed ~0.5 s (the window: a hit
  in it is a COUNTER: a stagger, x2 damage, N43Events.BurrowCounter), then dives. Airborne or on metal plates you are safe. HP enough to
  take a few shotgun blasts. Its look: a segmented orange / yellow body, a ringed mouth, readable from above. Numbers with _why; the
  events N43Events.BurrowTell / BurrowEmerge / BurrowCounter.
- look43 (G8 / G29): a lighting / composition kit for the levels NIGHT43 touches: lights that mark the way forward, hazard reads (lava
  glow), fixtures with a purpose; no clutter (DESIGN-RULES art). Last in your order.
- Proof reel `burrow43`: a BURROWER tell -> emerge -> counter; the enemy line-up (models43 on / off).

**B1 THE WICK (builder B1).** G13 (with M1), G14, G15, G16, G10. Owns Core/Enemies/Wick37*.cs, SurvivalSim.Wick37.cs, Juice38's boss entrance,
Doors38View's boss door look, the Wick views, design.n43.wick43, SurvivalSim.N43.Wick.cs (incl. N43BossStateFill), Harness.N43.Wick.cs,
FeelReel.N43.Wick.cs.
- G13: with chimney43 / ride43 forced on, THE STACK's panel chain climbs reliably by presses and by held rides (the bot's climb share
  before / after); the hot-wall throw and the 12.4 m/s shaft clamp reviewed (eased at the tunnel; no "brick wall" at the door).
- G14: N43BossStateFill for THE WICK: valid for the whole fight (today the bar is hidden when the body is off; CODE-FACTS-B), hp01, the leg
  lock (locked + lockAt), OPEN while the cyan stagger / a cracked plate takes full damage, the leg, a hint ("CRACK A PLATE: RETURN AN EMBER",
  "RIDE, HOLD {fire}: SHOOT THE CRACK"); chip damage visibly counts.
- G15: the reveal more cinematic (bosses-web.md: first intro <= 6-8 s, retries <= 1.5 s, in the world, the player keeps moving): a short
  framing (letterbox bars, the camera's look eased toward the rising WICK while the player keeps control, the shaft lights coming on in
  order), the name card, then play.
- G16: each leg change is a readable beat (the bar's leg tick, the slag drops, the WICK recoils, N43Events.BossLeg); the parry (EMBER)
  stays central; leg 2 / 3 each add one attack variation built on the same language (not more HP).
- G10: the boss door (d10) and its tunnel: no pink (the door's violet-white + the red / blue flicker read as a bug): a door in the boss
  language, the flicker without the magenta fringe, the tunnel wide enough to carry the speed in.
- Harness: n37 wick's bot band and the casual bot with wick43 on; Proof reel `wick43`: the reveal (3 frames), the bar LOCKED / OPEN /
  STAGGER, a leg change, the door approach.
</cards>

<builders>
| Builder | Owns |
| --- | --- |
| **M1** movement | Mover.cs (behind chimney43 / ride43 / boost43), MovementMath, Ride35/40 hooks, Boost42/43, Charge35's ground press |
| **L1** level 1 | gen-n43-foundry.cjs, Design/levels/n43-foundry*, foundry43 |
| **W1** weapons | gauntlet cycle, Parry42/43, the reflect volume, Fusion43, ImpactRules' fusion tier, gauntlet43 / parry43 / fusion43 |
| **F1** feel | GunFx*, ImpactFrames, CameraFeel / PostFx additions, Rumble / Haptics22View additions, Fusion43View, feel43 |
| **U1** UI | UiHud33*, UiTeach43, UiShop33 / results visuals, UiBossBar41, the blur, hud43 / counter43 / teach43 / screens43 / bossbar43 |
| **E1** encounters | Campaign24*, Level36 / spawn33 additions, HuskBrain's watchdog, Husks' kind map, n43-iceline, rooms43 / iceline43 |
| **K1** level 3 | gen-n43-kiln.cjs, Design/levels/n43-kiln*, WyrmArena's geometry, kiln43 |
| **A1** enemies' look | EnemyViews*, Enemy40View, Enemy43View*, Burrower43 (+ one kind-map line), models43 / burrow43 / look43 |
| **B1** THE WICK | Wick37*, SurvivalSim.Wick37, Juice38's boss entrance, Doors38View's boss door, wick43 |
| the lead | docs/DESIGN-RULES.md, PRINCIPLES / VERBS updates, DECIDE43, integration, the Editor, Players, reels, git |

**Hand-offs:** E1's room fields (first slice) -> L1, K1. M1's boost heights -> K1. U1's tokens (U1.md) -> F1, A1. W1's Fusion43Event -> F1.
B1's N43BossStateFill -> U1. A1's "burrower" kind -> K1 (K1 may place it in data before it exists: an unknown kind is a plain husk until A1
lands). Builders cannot message each other: report hand-offs; the lead passes them on.
**The spend limit (L-003):** commit every slice; on 429s the lead integrates and builds alone.
**Keep first if short:** M1 chimney43 + ride43 (d) the B slam; E1 the door logic + the counter; boost43's combine; L1's GANTRY and FLUE;
W1 gauntlet43 + parry43; B1's bar state; U1's counter + teach card + health bar; F1's FUSION; A1's models; K1's kb1; then the rest.
**Cut in this order:** look43, THE RING's redesign, A1's swarmer / warden models, screens43's results (keep the shop), B1's leg variations,
iceline43's second rehearsal.
</builders>

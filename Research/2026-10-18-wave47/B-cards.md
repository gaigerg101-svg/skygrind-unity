# NIGHT47 builder B: the cards (levels 2 and 3; G25-G35)

Written 2026-10-18 23:00-23:50 by builder B, round 0, before any code (NIGHT47.md section 5: "geometry and code only after PASS"). The lead
assembles docs/BOSSES47.md / docs/ROOMS.md from these; I edit no shared doc. Research lines are R-B1..R-B4 in `B.md` (this folder). Every
"today" number was read in the code or measured tonight (file:line, or the harness line); a number I plan is marked **plan**. Unity config =
`Assets/StreamingAssets/Design/unity-config.json` (UC), the web config = `PortExport/config.json` (PC).

## 0. Baselines (measured or read tonight, 22:53-23:30)

**Chargers in level 2 today (n46-iceline, the ON level):** all 4 are in g8 2.8 THE REHEARSAL, the 8th of 9 rooms before THE BRAWL:
waves [charger], [charger, husk, husk], [charger, charger]. No other level-2 room has one (Design/levels/n46/iceline.plan.json, read by
node). Level 3 has 2 (kb2 w3, kb3 w2). n47-iceline is n46-iceline with only its name changed (n47-shared.cjs:7, 49).

**The CHARGER today** (Core/Enemies/Charger.cs; PC:2236-2257; UC tells36 / foes40):
- wind-up 1.2 s (tells36 chargerTellS, UC:3873; the web's tellTime 0.8 overridden); **its line locks at the wind-up's START**
  (Charger.cs:265), so a walk sideways during the 1.2 s beats it: no late-dodge skill exists (measured: `n47b charger`, section 5's G26
  row: an early strafe and a late one both dodge 40 / 40).
- charge 22 m/s for 30 m, **no turning at all** (yaw = chargeYaw, Charger.cs:183-190); 25 damage + knock 14 / 6 within 1.5 m; a player
  1.76 m above its feet is missed.
- wall: stunned 2.0 s (foes40.charger.stunS, UC:5993); a charge that hits nothing still stuns 1.2 s; then recover 0.8 s, cooldown 3 s.
- damage: front x0.3, the back while stunned x3 (Charger.cs:110-117), a hitscan gun x1.5 during the wind-up (UC:1563-1568); 150 HP;
  knockbackScale 0.3.
- look (EnemyViews.Charger.cs): a box body ("a 1.35 x h x 0.8 x 1.2 box"), a box horn, a flat back plate that glows 0xffe14a x2.5 while
  stunned, a 1.6 x 30 m red-orange line on the floor during the tell; models40 / models43 add a plough wedge, horns and a chevron visor.

**THE ICEBREAKER today** (Core/Enemies/Brawl40.cs + Brawl40.N42.cs; UC:5689-5898, 6285-6352): pool 6000 x BossMath.Scale, 2 phases (at
0.5), rest 1.2 s (P1) / 0.55 s (P2). Attacks and wind-ups:

| attack | wind-up today | active | damage | counter today | punish today |
|---|---|---|---|---|---|
| HAYMAKER | 1.0 s (P2 0.9), tracks 70 % | 0.2 s, 2.6 m reach, 50 deg | 30 | dash out of the cone / jump | STUCK 1.2 s (x1.5) |
| SLAM | 0.9 s (P2 0.8) | ring 12 m/s to 20 m | 20 | be in the air | recover 0.8 s |
| BLOCK (yellow) | 0.75 s | an ice block at 20 m/s | 15 | punch it back: 150 dmg, 4 meter | recover 0.5 s |
| RUSH | **0.65 s**, locks at 60 % | 16 m/s up to 1.6 s | 25 | dash aside; a wall / pillar = STUCK 1.2 s | recover 0.4 s |
| SWEEP (n42) | 0.9 s | 0.4 s a spin (P2: two) | 22 | jump (P2 double jump) | dizzy 0.7 s |
| LEAP (n42) | 1.1 s, tracks 60 % | 0.55 s, 3.2 m ring | 28 | dash out of the circle | STUCK 1.0 s |
Damage: guns x1 within 4 m falling to x0.2 at 12 m; gauntlet / punch / parry x1; x1.5 STUCK, x2 WINDOW. Stagger meter 6 (P1) / 8 (P2): a
gauntlet hit drains 3, a punch 1; empty = the WINDOW 2.5 s. P2 floes: every 3 s the floe ahead of you cracks (1 s red, then water 4 s, 12 HP/s).
The view (Brawl40View*.cs): boxes; **no springs, no pose easing** (lean / head angles set per state instantly, Brawl40View.cs:270-317);
STUCK and WINDOW drawn in cyan 36e0ff (UnityTuning.Brawl40.cs:10-28); hits: rims white 0.08 s, gauntlet hits add ice chips and a 0.28 s crack.
Measured tonight (`n42brawl all --seeds 10 --gauntlet42`, 23:00): skilled bot p50 38.1 s / p90 47.1 s, 0 deaths; casual bot p50 53.7 s,
max 79.0 s, 1.2 deaths a fight, 170 health lost a fight. Tells seen: haymaker 0.90-1.00 s, sweep 0.90, slam 0.80-0.90, block 0.75, leap 1.10.
The human-model pilot (n47human) arrives ~23:45: the human baseline is its first number (not predicted here).

**THE KILN WYRM today:** section 3.0 below.

## 1. THE CHARGER foe card (G26; rows charger47 sim, chargerLook47 view)

| field | the card |
|---|---|
| Job | the level-2 role that punishes standing on a line: it asks for a late sidestep and a punish from behind, never a trade at the front. It is the mini-ICEBREAKER (DESIGN-RULES E-1, E-7): its wind-up, its lit line and its open back are the ICEBREAKER's RUSH in small, so the boss's language is learned 4 rooms early |
| Shape logic | the NIGHT43 pill body (E-2) carried low and forward on a squat stance, with ONE dominant shape: a heavy wedge PROW plate over the head and shoulders (triangles = danger, RocketBrush via NIGHT47 s10), two swept-back horn blades off the prow, a chevron brow visor (E-2's charger class; F's visor47 spec applies), and on the back a bank of 3 VENT slats (squares = the machine) that open when stunned. One silhouette at 30 m: a wedge with a hump. Palette: dark steel body, red-orange visor and prow edge; the vents dark in idle, red-orange glowing in the wind-up, **yellow while stunned (yellow = punch it, the parry language; the WICK's crack and the ICEBREAKER's window use the same yellow)** |
| Idle | a breathing spring on the prow (2 % at 0.3 Hz), a hoof scrape every 2-3 s (view, cosmetic seeded generator), the head tracks you with a lagging spring |
| Wind-up (plan 0.95 s; G26 asks 0.8-1.0) | 0-0.3 s: it drops 0.25 m and the prow dips 15 deg (anticipation), the vents ramp dim -> bright red-orange; 0.3-0.95 s: the lit line (1.6 m wide, to its charge distance or the first wall) sweeps WITH you (it tracks); **the last 0.25 s it locks**: the line stops, flashes brighter and its edge turns white, the prow snaps level. A sidestep in the last 0.25 s beats it; one before is followed (Helldivers' "dodge late, not early", ULTRAKILL Cerberus's last turn: R-B1) |
| Charge | 22 m/s (kept), turning at most **plan 35 deg/s** toward you (today 0): it cannot turn sharply (Pinky, Helldivers' widened turn arc), so a late dodge wins and an early one is punished. A body in its path: a husk / swarmer is burst (the launch36 burst event), a heavy (tank, warden) is knocked aside and the charger stops, stunned 1.2 s |
| Wall stun (weak spot) | 2.0 s (kept), the vents open yellow, the back takes x3 (kept); a missed charge skids 0.8 s with its back exposed (x1.5, vents half open: Helldivers' lengthened recovery) instead of today's 1.2 s full stun |
| Counters per verb (VERBS rows) | dash / slide (5, 6) in the lock: dodge; jump / wall ride (3, 9): it passes under (kept rule: feet 1.76 m up); KESTREL tap (25): x0.3 on the front, x1.5 in the wind-up (kept); **KESTREL charged pierce (27): full damage through the back and through every charger in its line (two lined up behind a pillar is the trick shot)**; gauntlet punch (28) on the front: x0.3 and it does not stop the charge (the armour shrugs: the G27 lever); **punch on the stunned back: launch36 (41) launches it, and a launched charger that hits another bursts it**; LT blast (28) pushes it off its line mid-charge (knock x0.3, kept) but deals x0.3 at the front; THUNDERCLAP (29, level 3) slug into the open back; parry (40): not parryable (red-orange: dodge only, E-4) |
| Escalation (plan, sub-switch `charger47.enrage`) | when one charger dies, every other charger in the room gets **one** faster wind-up (0.95 -> 0.8 s, never under E-4's 0.8) and its vents flare: kill order is a decision (ULTRAKILL Cerberus's enrage, R-B1). A room of three (the stampede) is where it matters |
| Death | the prow splits along its crease and falls forward, the vents vent a puff (view only; the sim's kill unchanged) |
| Frames (reel a247, level 2) | idle at 10 m; wind-up start; wind-up last 0.2 s (the locked white-edged line); mid-charge with a late dodge; wall stun with yellow vents (front 45 deg, back); the punch launch into a second charger (the burst); death; the 10 / 20 / 30 m grayscale silhouette (with F's visor sheet) |
| Rule ids | E-1, E-2, E-3, E-4, E-7, C-1, R-4, M-4 (a dash pressed in the lock keeps its meaning) |

Events (new, web-style, for docs/AUDIO-HOOKS.md): `chargerLock47` (the last 0.25 s), `chargerSkid47` (a miss), `chargerBurst47` (a body
burst by a charge), `chargerEnrage47`. Kept: chargerTell / chargerCharge / chargerStunned / chargerBlocked.

## 2. Level 2's CHARGER route (G25) and THE STAMPEDE (G27): n47-iceline (row iceline47; Tools/night/gen-n47-iceline.cjs from gen-n46-iceline.cjs)

**G25 the route (plan):** chargers before THE BRAWL go from 4 in 1 room (the 8th) to 10 in 4 rooms, the first in the first third alone.

| room (order) | today | n47 (plan) | why |
|---|---|---|---|
| g1 2.1 TANK ALONE (1st) | [tank] | [tank], then **[charger] alone** through the exit door (wave 2, after the tank dies), its first charge down the room's 20 m axis at the spawn | the first third, alone (G25). The room's law is "a slow body closing"; the charger is its fast twin. The two 5 m ride panels (x +-8..8.6, z -18..-5) are the walls it breaks on; the entry wall behind you is the other. **R-3 broken on purpose** (two new roles in one room): GG 2026-10-18 "needs to appear earlier and more frequently". Lesson (copy for F): "IT CAN'T TURN. {dash} LATE, THEN PUNCH ITS BACK." |
| g2 2.2 THE SHIELD (2nd) | [warden], [warden, husk x2] | + wave 3 **[charger, husk, husk]** | the charger with known fodder (R-2 reinforce); the room's two flank pillars (5 m, x +-9..9.6) break its charge; at most 3 roles at once |
| g5 2.5 THE COOLDOWN (5th) | 3 waves | wave 2 **+ 1 charger** with the swarm | a charge through the swarm bursts swarmers (the charger as your tool: bait it into the pack while the blast cools); 3 roles at once |
| g8 2.8 THE STAMPEDE (8th) | 4 chargers in 3 waves | rebuilt below: **6 chargers + husks + 2 spitters** | the mini challenge (G27), the boss's rehearsal |

**THE STAMPEDE: the arena card (DESIGN-RULES A-9), g8's rect kept (x -12..12, z -250..-220, 24 x 30 m, roof 9 m), doors kept.**

| field | THE ROUNDHOUSE |
|---|---|
| Title | THE ROUNDHOUSE (the banner "2.8 THE STAMPEDE") |
| Place fantasy | the ice line's engine shed: the line's ploughs were stabled here between two rows of iron columns, over a long inspection pit; the ploughs are gone and the CHARGERS (built from their blades) stable in their place; the line's crew (husks) and two spitters on the coaling gallery hold the shed |
| Spatial law | nave: one long hall, two column rows making three lanes (the chargers' straights), a gallery across the far end |
| Verb it celebrates | the late dodge into a column, then the punish from behind with the right tool (the KESTREL's pierce through two backs, the punch-launch of a stunned one into another) |
| The 30 s problem | Three chargers own the lanes while two spitters cover them from the far gallery: bait each charge into a column, punish the open back, and keep the gallery quiet with the KESTREL, because the glove cannot reach the gallery and the prow turns a punch to the face (no other room asks you to pick your weapon by which side of the enemy you stand on) |
| Molecule | D the door lip (1.0 m up, the whole hall in view) -> a one-way drop to the floor; three lanes W / C / E between the walls and the 4 kept columns (moved to a 2 x 3 grid, plan: x +-5, z -228 / -235 / -242); THE PIT along lane C (plan 3 x 10 m, 1.5 m deep, z -230..-240) with ramps at both ends; the GALLERY (plan 3 m deck, x -12..12, z -250..-247) with two end ramps (the spitters' and husks' climb route, GG NIGHT20); the earned shortcut: a wall ride on the E wall from the lip to the gallery's end (the ride panel kept from the level's language, cyan strip) |
| Landmark | THE TURNTABLE GATE: the far wall's big round engine door over the gallery, lit from behind (the exit), its plough-blade frame red-orange: seen from the lip, the only round shape in the room |
| Hero prop | THE PIT: it says what the shed was for, and in play a charger whose charge crosses it falls in and is STUCK (dazed, plan 3 s, today's blocked rule: it cannot jump) with its back up: the free punish (Pinky "can be led off ledges", R-B1); a player who falls in walks out by a ramp (time, not health) |
| Hazard both ways | the pit (above); no lava (level 2's fire is g7's grate) |
| State change | wave 3 (three chargers at once) opens the gallery's two side hatches for the spitters: the gallery lamps flip white -> red-orange (shift46's fixture rule, named for the view) |
| Light / colour story | the lip dark; the hall under three white panels (kept); the gallery under two red-orange lamps once wave 3 comes; the exit's cyan pool beyond the turntable gate (A-6) |
| Every-solid rule | 6 columns (2.5 m, 5 m tall): the walls the charges break on, and cover from the gallery; the pit: the charger trap and the one cut through lane C; the gallery + 2 ramps: the spitters' high ground (KESTREL range ~20-28 m from the lip, the glove out of reach), the walkers' climb; the lip: the entry view and the first safe stand; the E ride panel: the shortcut to the gallery. Nothing else |
| Tell-a-friend line | "Three chargers in an engine shed: you make them smash into the pillars, then shoot them in the back, and if you're clever you make one fall in the pit." |
| Cut list | today's g8 lane-end wave-1 spot (replaced by the lane C far end); nothing walked yet |

**THE STAMPEDE's flow plan (arena-flow-web.md Part D, 11 points; plan numbers, measured after the build):**
1. Entry: the lip 1.0 m up, 3 m deep at z -220..-223, the whole hall and wave 1 in view; a one-way drop; first foothold the first column
   pair 5 m in (z -228).
2. Tiers: the pit (-1.5), the floor (0), the lip (1.0), the gallery (3.0); the columns 5 m (cover, not a tier).
3. Loops: the ground ring round the pit (lane W -> gallery foot -> lane E -> the lip's foot), ~70 m; the air line: the E ride panel -> the
   gallery -> the W ramp down (not closed: a teaching-size room, A-7).
4. Outs: every column pair has a lane on each side; the gallery has two ramps; the pit two ramps; the lip one drop (the one one-way).
5. Cover: the 6 columns (5 m, full cover from the gallery) and two 0.9 m coal carts in lanes W / E (plan; one jump clears).
6. Hazard's job: the pit cuts lane C only (the strafe ring stays whole); it is where a charger is trapped.
7. Asymmetric sides: W = the plain lane and a cart; E = the ride panel and a cart; C = the pit; one landmark (the gate) from every tier.
8. Spawns (waveNodes43): w1 [charger] at the far end of lane C (0, 0, -245), in view from the lip (los check at build); w2 [charger x2,
   husk x2] chargers at the far ends of lanes W / E, husks from the gallery ramps; w3 [charger x3, spitter x2]: chargers from the far
   lane ends + the W wall hatch, spitters on the gallery (x +-8, z -248.5), never behind the lip.
9. Healing: no in-round kill healing (DECIDE46 N-9 kept); one health pickup on the gallery's middle (exposed: Part D 4).
10. Between waves: the gallery hatches open at w3 only.
11. Measure: `n46flow --level n47-iceline` (g8 row: dwell, stall, clear, nearest-enemy) vs n46-iceline; the walk 10 / 10; and the G27
    meter below.

**G27 meter (plan): gauntlet-only vs mixed in THE STAMPEDE.** In my harness (`Harness.N47.Brawl.cs`, command `n47b stampede`), the
human-model pilot (n47human, through its policy hook) clears g8 with policy `gauntlet` (the glove alone: punch + LT blast, dodges as the
pilot does) and with policy `mixed` (the glove on stunned backs, the KESTREL charged pierce on lined-up backs and on the gallery), 40
seeds each, rows charger47 + iceline47 ON. Done when: gauntlet-only takes >= 25 % longer OR >= 25 % more damage than mixed (the median);
the RED control: the same two policies in today's g8 (n46-iceline, rows off), where I expect the gap to be small (measured, not assumed).

## 3. THE ICEBREAKER boss card (G28-G30; rows brawl47 sim, brawlLook47 view)

| field | the card |
|---|---|
| Fantasy | the ice line's icebreaker, the plough-ship's prow walking: a brawler that breaks the frozen canal under you |
| Silhouette + palette | the CHARGER's family scaled up (one faction, one logic): a huge pill torso carried forward under a wedge PROW chest plate, two oversized rounded mitts with ice knuckle shards (triangles), a chevron visor under the prow, the same 3-slat VENT bank on its back. Palette: dark steel, frost-white plates (pale grey-white, **never cyan**: today's STUCK / WINDOW cyan 36e0ff moves to **yellow** ffd23a, the punch-it colour), red-orange tells and visor, yellow vents and cracks when OPEN |
| The arena's job | kept (brawl40-arena: 36 x 36 m, the floes in P2, the pillars a rush breaks on); P3 adds the CHARGER gates (below) |
| What GG likes / keeps | not stated by GG for this boss; kept: the punch-opened WINDOW (stagger meter), the yellow BLOCK parry (B-5: parry is the game's yellow), the floes |

**Phases (plan: three, at 66 % / 33 %; each pool change is a split, never HP added):**

| phase | rule | attacks (name: tell, length; counter; punish window; hit effect) | verb it stars |
|---|---|---|---|
| P1 (100-66 %) THE BOUT | teaches: one attack at a time, rest 1.2 s | HAYMAKER: shoulder pulls back, the fist's knuckles glow red-orange, 1.0 s, tracks 70 % then locks; dash out of the 50 deg cone; STUCK 1.2 s with the vents yellow; impact: ice shatter at the fist, a floor crack decal, heavy camera on a hit. SLAM: both mitts rise over the head, a red-orange ring painted at its feet, 0.9 s; jump; recover 0.8 s; impact: a frost ring with ice spikes along it. BLOCK: it tears an ice block off the floor, the block YELLOW, 0.9 s (today 0.75: raised to E-4's 0.8+); punch it back; a hit drains 4 meter; impact: a yellow-white flash, the block shatters on its chest (critical hitstop tier). PROW RUSH: **the CHARGER's wind-up** (it drops, vents red-orange, a lit line that tracks then locks in the last 0.25 s), **0.95 s (today 0.65: under E-4)**; dash late into a pillar's side; it breaks on the pillar: STUCK 1.2 s, the vents yellow; impact: the pillar cracks, ice debris | the punch (gauntlet) on the BLOCK and the STUCK back |
| transition 66 % (1.2 s) | the arena changes (B-2): it roars and stamps, the canal's ice cracks into floes (the P2 floes rule moves from 50 % to here), a banner line (copy for F: "IT'S BREAKING THE ICE."), a push-away ring | | |
| P2 (66-33 %) THE THAW | the floor is the threat: the floes crack ahead of you; adds the SWEEP and the LEAP | SWEEP: it plants a foot and winds its arm low, a red-orange arc on the floor at shin height, 0.9 s; jump (P2: double jump for two spins); dizzy 0.7 s; impact: an arc of ice spray along the arm's path. LEAP: it crouches, a red-orange disc tracks you 60 % then locks and pulses, 1.1 s; dash out of the disc; STUCK 1.0 s; impact: a ring + floe crack + dust. Plus P1's four with rest 0.8 s | the dodge on cracking floes (movement under a floor rule) |
| transition 33 % (1.2 s) | the CHARGER gates open (two hatches in the arena wall); a line ("THE HERD'S COMING.") | | |
| P3 (33-0 %) THE STAMPEDE | P1 remixed harder (section 3: combined, shorter gaps, tells >= 0.8 s): HAYMAKER -> SLAM / SWEEP chains (kept 50 % / 60 %), a DOUBLE PROW RUSH (two rushes, the second's tell 0.8 s and its line shown during the first), rest 0.55 s; **two CHARGERS** enter once from the gates (the level's lesson as the exam). **A CHARGER that hits the ICEBREAKER staggers it: 3 meter, as a gauntlet hit** (plan): bait a charger into the boss | everything: the late dodge, the punch, the KESTREL pierce on two backs |

**Length (G4's numbers, the human band):** median 90-150 s, p90 <= 180 s, min >= 60 s on 40 seeds of n47human. Levers in this order, each
measured on 10 seeds, then 40: the WINDOW 2.5 -> up to 3.5 s; STUCK x1.5 -> x2; the chip outside windows held >= 30 % of full (guns x0.2
at range today: raised to x0.3, section 3's chip rule); the meter drain (gauntlet 3 / punch 1) for more windows; P3's rests. **Pools only
go down.** If the human median is under 90 s, length comes from P3 (the chargers) and the transitions, never HP.

**G29 animation (brawlLook47, view only; the view reads the sim's state, no hit box or timing changes):** key poses per state (idle,
wind-up per attack, strike, STUCK, WINDOW, recover, phase roar) blended by NIGHT5's closed-form springs (David Rosen's few-poses-plus-
procedural approach, R-B3, Reported): anticipation (the wind-up pose sinks 0.3 m and pulls the striking side back), overshoot and settle
on the strike (the fist's spring overshoots ~15 %, settles in <= 0.25 s), follow-through (the prow plate, the knuckle shards and the vent
slats on lagging springs), a 60-100 ms flinch spring on every hit taken, an idle that breathes (chest 2 % at 0.25 Hz), a walk bob with foot
plants and the floor's frost puff. Never a linear lerp on a limb.

**G30 hit effects (brawlLook47):** each attack its own tier (uhiyama-lab's ladder, R-B3): light (the SWEEP's spray, the SLAM ring when it
misses: no shake), heavy (the HAYMAKER / LEAP / PROW RUSH impact: the floor crack, the ice burst, camera 3-5 deg when it hits the player,
1.5-2.5 deg when it lands within 4 m), critical (the BLOCK returned: the 1-2 frame yellow-white flash, the shatter, the sim's hitstop). The
player's hits on it: chip = small ice chips + the 80 ms rim flash (kept); window hits = a frost plate breaks off; **the armour sheds 4
plates at 80 / 60 / 40 / 20 %** so the damage shows on the body (B-1: damage always registers).

Events (new): `brawl47Phase3`, `brawl47Gates`, `brawl47ChargerHit` (a charger staggered it), `brawl47DoubleRush`, `brawl47PlateShed`.

**Frames (reel a247, x9 THE BRAWL):** per attack the wind-up start, the last 0.2 s, the hit, the impact (6 attacks x 4); each transition;
P3 with the chargers; a plate shed; before (rows off) and after, 1280x720 and 1920x1080.

## 4. THE KILN WYRM (G31-G35; rows wyrm47 sim, wyrmLook47 view, kiln47 level)

### 4.0 Baseline (read in the code tonight; UC = Design/unity-config.json, its StreamingAssets copy byte-identical)
The fight runs Wyrm.cs with n26 paths (Design/bosses/wyrm26.json), the cycle `coil, dive, lunge, loop, finale` (Wyrm.cs:186; the finale
only in P3, :578). Pool 7800 x 1.1 x 0.7 x 0.234 x 4 (n39) x 0.9 (wyrm46.poolScale) = **5058**; phases at 66 / 33 %. Damage: body x0.5,
head x2, the exposed core x3; outside a stagger every hit x0.8 (gunScale); the PULSE punch on the head in the last 0.4 s of a charge
staggers it (Wyrm31.cs:147): STAGGERED 8 s (wyrm46), every weapon x2.5, a slug x2 more, one hit capped at 15 % of the pool.

| attack | tell today (s) | what shows today (the boxes view; wormVisual OFF) | damage | counter |
|---|---|---|---|---|
| COIL (P3 SQUEEZE) | squeeze 0.8 | nothing named | contact (body 5, head 18) | leave the ring |
| DIVE | crack **0.6**, then dive 1.0 | a 5.2 m red-orange floor square, then a 4.4 m wall plate (WyrmView.cs:185-207); buried pieces hidden | 5 shards x 10 | the DIVE plates |
| LUNGE | 0.8 | a 3.2 m line | head 35 | sidestep |
| LOOP | **none** | none | the spit runs in it | none |
| SPIT (P2-P3) | **0.6** | nothing named | 10 x 8, every 3.2 / 2.2 s, a 2nd lane 1.0 s later | cover |
| CORE (P2-P3) | roar **0.6** | dim violet glow, then a red-orange pulse | (opens 3.5 / 4.5 s every 14 / 10 s) | shoot it x3 |
| AGGRO charge | **0.5** | a locked line | contact | punch the head (0.4 s window) |
| FEINT + TAIL SWEEP | **0.6** | nothing named | 20 within 9 m on the ground | jump |
| anti-camp spit | 1.0 | | 35 every 1 s | keep moving |
| FINALE (P3) | 1.0 | a 4.2 m circle at the gap | contact | leave by the gap |
| crumble at 50 % | 1.0 | | | |
**Seven of the eleven are under G31's 0.8 s** and five show nothing that says where. **The burrow (G34) is still in the code but rarely
seen**: the DIVE (Wyrm.cs:620-652, unchanged since NIGHT24's 8f1aebd5: "a floor crack, then it burrows", to y -10 under the floor and
out of the wall) sits second in the cycle, and an AGGRO charge restarts the interrupted pattern (`cycleIndex--`, Wyrm.cs:807; the recoil
too, :838); the COIL is ~297 m (~12 s at P1 speed) and the aggro cooldown 5 s with the player inside 14 m, so the cycle seldom gets past
the coil (an inference from the code: the DIVE's share of the fight is measured in slice 2, not yet). **The look GG means (G32):** the
NIGHT26 Tripo worm (5dd327ec) had the molten palette (Wyrm26.shader:15-18: glow (1, 0.07, 0.005), amber (1, 0.32, 0.02), hot (1, 0.88,
0.67), ash (0.01, 0.006, 0.005); UnityTuning.Wyrm26Fx.cs:16 0xff4a10 / 0xff9a28 / 0xfff2d6 / 0x1a1210); **GG turned it off on
2026-10-09 (ad187af6: "asked for the old worm and not the Tripo one")**; today's blocky worm is dark steel 0x3a3f4a with pale violet-white
trim (UnityTuning.Wyrm24.cs:10), red-orange only on the core / spit / marks. So GG wants the blocky worm's shape (10-09) in the molten
colours (10-18): the card does both. Measured tonight (`n46wyrm fight --level n46-kiln --seeds 10`, default rows, 22:56): killed 10 / 10,
**bot median 59.5 s** (32.6-148.6), deaths median 1, damage taken median 116, staggers median 2, STAGGERED share 91.2 %, crumble 10 / 10
(NIGHT46's 40-seed median was 38.3 s, NIGHT46-LOG.md:135: why tonight's 10 seeds differ is not checked). **No pilot boosts**: WyrmPilot31
uses the gauntlet / shotgun / revolver from the floor only (WyrmPilot31.cs:32), and no wyrm harness counts a boost: today's boost uses per
minute in this fight are not measured by anything (G33's baseline is built in slice 2, below).

**Slice 2 (measured 23:12, `n47b wyrm --level n46-kiln --seeds 10`, Tools/SimHarness/Harness.N47.Brawl.cs, rows off, WyrmPilot31):**
killed 10 / 10, median 58.6 s (6 of 10 seeds differ from n46wyrm's times: my meter has none of n46wyrm's death handling; not checked
further). Tells from the worm's own log: aggro 102x 0.50 s, crack 28x 0.60, dive 15x 1.00, finale 10x 1.00, finaleGap 9x 1.00, lunge 20x
0.80, roar 33x 0.60, spit 177x 0.60, squeeze 37x 0.80: **under 0.8 s: aggro, crack, roar, spit** (the tail sweep's 0.6 s is Wyrm31's, not
in this log). Pattern share of the fight's steps: **coil 60.4 %**, aggro 8.9, dive 7.5, finale 6.6, lunge 5.8, loop 4.0, recoil 2.8,
emerge 2.4, feint 1.6. **Burrows: median 1.08 a minute; 3 of 10 fights never burrow; 28 cracks told but only 15 dives told** (13 cut,
by an aggro charge: Wyrm.cs:807). THUNDERCLAP shots fired airborne by WyrmPilot31: median 1.08 a minute (incidental: its tail-sweep hops
while it fires; it never boosts on purpose). G34's RED line: 1.08 burrows / min and 7 / 10 fights; G33's RED line: the human pilot's
count with my policy in this arena (when n47human lands).

**Reach (measured 23:10, `n47b reach`, kb3's floor, rows off):** a jump 1.58 m; jump + double jump **2.88 m**; jump + one shot down at the
apex 4.12 m; jump + shot + double jump + shot **7.92 m** (kiln43's chain: 4.0 / 5.8 / 9.0 m, consistent).

### 4.1 THE KILN WYRM boss card
| field | the card |
|---|---|
| Fantasy | the kiln's furnace given a spine: a molten worm that swims through the firing floor |
| Silhouette + palette | **kept: the blocky segmented worm GG chose on 10-09** (the boxes, the pill head GG's NIGHT43 language), made cooler: each segment an ash-black block (0x1a1210) whose seams glow molten (glow 0xff4a10 at the joins, amber 0xff9a28 along the back ridge, white-hot 0xfff2d6 only in the mouth and on a damage flash), a back ridge of slag fins (triangles, danger) that lag on springs, the head a wedge jaw with an inner glow that breathes. Palette = the NIGHT26 molten yellow-red-orange (G32), on the 10-09 shape. The pale violet trim goes (the boundary colour is not the enemy's). Yellow only on the punch-it moment (below) |
| What GG likes / keeps | "feels fine overall": the fight's core (the punch-opened STAGGER, the slug in the window, the rim plates and perches, the crumble at half), its phases (66 / 33 %) and its length (the human band) are kept; what changes is what he named: the tells, the colours, the arena for the boost, the burrow, the anticipation |
| The arena's job | the floor is the worm's, the air is yours: every attack that owns the floor has an answer up (the boost, VERBS 29) |

**Per phase (plan; every tell 0.8-1.5 s and says WHERE (G31); tells never overlap (`resolveGap` kept, raised to cover the longer tells)):**

| phase | attack | tell (what, how long) | counter (verb) | punish window | hit effect |
|---|---|---|---|---|---|
| P1 (100-66 %) | AGGRO charge | the head lowers, its jaw glow ramps amber -> white, a lit line runs from it to you and locks, **0.9 s** (today 0.5); **the head turns yellow for the punch window** (0.4 -> 0.6 s, wyrm30 windowSeconds) | the PULSE punch on the head (28) | STAGGERED 8 s (kept), x2.5 | a molten splash from the jaw, heavy camera on a hit, the head's recoil spring |
| P1 | LUNGE | the body coils back (anticipation), a 3.2 m line glows along its path, **1.0 s** (0.8) | sidestep / dash (5) | | a floor scorch along the line |
| P1 | THE BURROW (the DIVE, G34) | **a crack opens in the floor where it will dive and glows, 1.2 s** (crack 0.6); it dives; **under the floor a glowing crack trail follows its buried path** (Diablos' trail, R-B2) and the floor over it shakes; **the breach: a 2.6 m crack circle glows under the player's position (tracks 60 % then locks), 1.2 s**, then it erupts with a molten burst, arcs 8-11 m up and over | get off the floor: the boost (29) to a perch or a slag island, or a dash (5) off the circle | **the arc: the back vent (the core) faces up at the top of the arc: x3 from above** (shot from a perch or mid-boost) | the floor bursts (debris, a molten ring), knock-up 7 on a hit, the eruption lights the room for 0.5 s |
| P1 | FEINT + TAIL SWEEP | the tail lifts and a red-orange arc paints 9 m on the floor, **0.9 s** (0.6) | jump (3) | 0.7 s while it re-coils | an ember arc along the floor |
| transition 66 % (1.2 s) | | it roars, the core vents flare, the room's floor seams glow (the light flip), a line (copy for F: "IT'S GOING UNDER."), push-away | | | |
| P2 (66-33 %) | + SPIT | the mouth glows and the lanes it will hit are painted on the floor as red-orange dashes, **0.9 s** (0.6) | cover / move off the lanes | | a molten spatter decal per hit, small shake |
| P2 | + CORE roar | the core vents open in steps (amber, 3 steps), **0.9 s** (0.6) | shoot the core from above (the boost to an island) | open 3.5 s (kept) | a vent burst |
| P2 | THE BURROW twice per cycle (the AGGRO no longer restarts it: plan) | as P1 | as P1 | as P1 | as P1 |
| transition 33 % (1.2 s) | | the crumble (kept, 1 s tell) + a line ("NOWHERE TO STAND.") | | | |
| P3 (33-0 %) | P1 + P2 remixed: SQUEEZE (ring painted on the floor, **1.0 s**), a DOUBLE BREACH (two circles, the second shown 0.6 s after the first: tells >= 0.8 s each), FINALE (kept, 1.0 s, the gap's circle) | | | | |

**Length:** the human band (median 90-150 s, p90 <= 180, min >= 60, 40 seeds of n47human). The longer tells and the breach arcs add
time; the arc's x3 from above and the yellow head window add damage chances: both measured, lever order: the STAGGER length (8 s kept), the
core's open time, the slug cap. Pools only go down.

**G32 frames:** the worm before / after at the emerge, mid-coil, the burrow trail, the breach, a damage flash; 1280x720 and 1920x1080.
Events (new): `wyrm47Crack`, `wyrm47Trail`, `wyrm47BreachTell`, `wyrm47Breach`, `wyrm47CoreFromAbove`, `wyrm47Glimpse`, `wyrm47Pass`,
`wyrm47Cameo`.

### 4.2 The arena for the boost (G33): wyrm47-arena.json (Tools/night/gen-n47-wyrm-arena.cjs from gen-n46-wyrm-arena.cjs; n47-kiln's k5)
NIGHT45 / NIGHT46's card THE KILN'S HEART is kept field for field (place, ring law, rim plates, perches, crumble, the KILN MOUTH, the
walk-in through d8); the changes, each with its reason:

| piece | change (plan) | why (the card line it serves) |
|---|---|---|
| THE SLAG ISLANDS (new, 3) | three 4 x 4 m slag plates hung on chains from the crucible at **6.0 m** over the inner ring (R 11-13, at 120 deg), no ramp, no ride wall: reached only by a boost from the floor or a 6 m perch, or a boost-hop between them (gap plan 8-10 m) | "heights only the boost clears" (G33), from the measured reach: 6.0 m is 3.1 m over the double jump's 2.88 m and 1.9 m over one shot's 4.12 m, 1.9 m under the two-shot chain's 7.92 m: only jump + shot + double jump + shot lands it from the floor |
| THE BREACH rule (sim, wyrm47) | the breach targets the floor (y < 2 m); a player on a perch, a plate >= 3 m or an island is skipped | the floor is the worm's, the air is yours; the burrow is the reason to boost |
| THE CORE from above (sim, wyrm47) | the core's x3 counts when the shooter's eye is above the core's top (the arc's apex 8-11 m); from below x1 | "a weak spot reachable from the air" (G33): the islands and the boost apex are the angle |
| the 1.2 m rim plates | kept, but **they no longer stop the breach** (it rises through 1.2 m) | today they are the safe answer to everything on the floor; tomorrow the safe answer is up |
| the 6 m perch (23) and the 9.5 m perches | kept; the gap from each 9.5 m perch to its nearest island plan 9-11 m (a boost-hop, not a jump) | the air loop (NIGHT46 Part D 3) gains the islands: rim perch -> island -> island -> rim perch, never the floor |
| the 13 m perches (crumble) | kept | the P2 -> P3 state change |
| no lava added | kept (WyrmArena is boxes only) | the hazard is the worm (Part D 6) |

**The flow plan (Part D, the deltas to NIGHT46's 11 points):** 2 tiers + the islands' 7.5 m; 3 the air loop rim perch -> 3 islands -> rim
perch (flight times per hop measured in slice 2, M-5: <= 0.5 s between surfaces at the boost's speed, or written SHORT); 4 every island two
outs (the next island, a drop to the floor or a perch); 5 no cover on the islands; 11 the meter below.

**G33 meter (plan):** in `Harness.N47.Brawl.cs` (`n47b wyrm boosts`), the human-model pilot (n47human) with my wyrm policy (a boost when a
breach tell is under it or the core is open above it; otherwise the pilot's default), 40 seeds, **today's arena + rows off (the RED
control)** vs **wyrm47 + kiln47 on**: boost uses per minute (a boost = a THUNDERCLAP shot fired airborne that applied the shotgun jump,
Combat.cs:992 ShotgunJump43) up >= 2x; plus the fight in the band. The same policy runs in both arenas, written before the arena changes.

### 4.3 The foreshadow beats in level 3 (G35): n47-kiln (Tools/night/gen-n47-kiln.cjs from gen-n46-kiln.cjs)
| beat | room | what (plan) | teaches | sim / view |
|---|---|---|---|---|
| 1 THE GLIMPSE | kb1 3.1 THE STEPS (the top of step 3, 9 m up) | a slot window in the far wall looks down into the kiln's core 40-60 m away: the worm's molten back arcs through a lava channel and is gone (once, on arrival at the top) | it exists, it is big, it lives in the fire | view only (a cosmetic worm on a scripted path, no collider), the `wyrm47Glimpse` event |
| 2 THE PASS | kb2 3.2 POUR HALL (36 x 44 m, the two lava pours) | between waves 1 and 2: the floor shakes, **the glowing crack trail** (the fight's burrow trail) runs across the hall from wall to wall under the player's feet, both pours surge 0.3 m as it passes beneath | the moving crack = the worm under you | view + camera shake; no damage (sim: a scripted event only) |
| 3 THE CAMEO | kb3 3.3 CASTING FLOOR (40 x 46 m, 18 m roof, the 8 m ladle) | after wave 1: **the breach tell** (the 2.6 m crack circle under the player, 1.2 s) and the head erupts through it (20 damage + knock-up, real), arcs, dives into the pour; then once more with the circle under the ladle's foot, and a lesson card (copy for F: "WHEN THE FLOOR GLOWS, GET UP. {jump} + {fire} DOWN.") | the boss's main tell and its answer (the boost up), without the fight | sim (wyrm47: the cameo hazard, its tell and damage) + view (the head and 6 segments) |
Done when (G35): frames of each beat, the walk 10 / 10 on n47-kiln, n46flow no worse past its band.

## 5. The PROOF plan (RED once, GREEN once: L-014; I never set enabled true)
| G | meter (mine unless named) | RED (before / row off) | GREEN (after / row on) | frames (the lead takes; reel phase) |
|---|---|---|---|---|
| G25 | `n47b route`: chargers per room before x9 on n47-iceline | 4 in 1 room, the first in room 8 (read tonight) | the first in g1 alone + >= 3 more rooms; the walk 10 / 10 (`n46wyrm walk`-style on n47-iceline) | a247: g1's charger from the spawn |
| G26 | `n47b charger`: a late dodge (pressed in the last 0.25 s) vs an early one (0.6 s before release), 40 seeds, scripted | **measured 23:18 (`n47b charger --seeds 40`, rows off, tells36's 1.2 s): still 40 / 40 hit; early (a strafe from 0.1 s into the wind-up) 0 / 40 hit; late (a strafe in the last 0.25 s) 0 / 40 hit**: both dodge, no timing skill exists (the seeds vary the distance 10-14 m only; the sandbox's open direction is the same each seed) | late dodges >= 90 %, early dodges <= 30 %; wind-up 0.8-1.0 s (steps); the stun 2.0 s; a charge through a husk bursts it | a247: idle, wind-up start, last 0.2 s, charge, wall stun, the launch burst, death; grayscale 10 / 20 / 30 m |
| G27 | `n47b stampede`: gauntlet-only vs mixed, n47human, 40 seeds | today's g8 (small gap expected: measured) | gauntlet-only >= 25 % longer or more damage (median) | a247: the lip view, mid-fight w3 |
| G28-G30 | n47human `--boss brawl` (40 seeds) + n42brawl tells | today's numbers (n42brawl above; n47human at ~23:45) | the human band; every tell >= 0.8 s (the rush 0.65 -> 0.95); the three phases | a247: per attack wind-up / last 0.2 s / hit / impact; transitions; P3 chargers; a plate shed |
| G31 | `n47b tells`: per attack the tell length in steps from the sim's bossTell marks | 7 of 11 under 0.8 s (table above; the log shows aggro 0.50, crack 0.60, roar 0.60, spit 0.60) | 11 / 11 in 0.8-1.5 s, none overlapping | a347: each tell at its start and its last 0.2 s |
| G32 | frames only | the steel / violet worm | the molten blocky worm | a347: emerge, coil, trail, breach, damage flash |
| G33 | `n47b wyrm boosts` (above) | today's arena, rows off | >= 2x boost uses per minute; the band held | a347: an island boost, a core hit from above |
| G34 | `n47b wyrm dives`: dives per minute and the share of fights with >= 2 dives | median 1.08 a minute, 7 / 10 fights see one, 13 of 28 cracks cut (n47b wyrm, 23:12) | >= 2 a minute (one per 30 s), 40 / 40 fights see one, no crack cut | a347: the dive crack, the trail, the breach |
| G35 | the walk + n46flow on n47-kiln; beat events fired 3 / 3 per walk | n46-kiln: 0 beats | 3 / 3 beats on 10 / 10 walks; n46flow no worse past its band | a347: each beat from the player's view |
Every code commit: golden 0 / 22 exact, fingerprint plain / --off46 / --off47 EQUAL, dn-check-wt 0, configs cmp.

## 5b. Round 1 as built (2026-10-19 00:00-01:10; these replace the plan's numbers where they differ)
- **G26 charger47:** wind-up 0.95 s, lock 0.25 s, turn **20 deg/s** (35 caught 8 of 20 late sidesteps), burst 1000 through launch36's path,
  heavy stun 1.2 s, enrage 0.8 s within 30 m. `n47b charger` (40 seeds, n46-iceline g1): rows off still / early / late hit 40 / 0 / 0;
  on 40 / 40 / 0; a husk in the line burst 0 / 40 off, 10 / 10 on. The missed charge's skid is NIGHT40's (stunned, back x3), not the
  card's half-open x1.5.
- **G25 / G27 n47-iceline:** chargers g1 1 (alone, wave 2), g2 1, g5 1 (its wave 2's two husks out: the 2-kinds-a-wave budget), g8 6 =
  9 in 4 rooms (was 4 in 1). THE ROUNDHOUSE built without the door lip (the spawn stands at the door), the gallery in two halves either side
  of the exit, the lamps white (one accent per room). Lessons (6 words max, LevelValidate): g1 "{dash} LATE: IT CAN'T TURN"; g8 "{dash} LATE,
  {fire} ITS BACK" / "BAIT ONE INTO THE PIT". n46flow 10 / 10 walks. The gauntlet-only vs mixed meter: not built yet.
- **G31 / G34 wyrm47:** every named tell 0.8-1.2 s except the head charge's (kept 0.5 s: at 0.9 s, fixed or tracking, the pilot's punch
  windows fell from 18 to 3-13 staggers in 10 fights); no charge inside the DIVE; STAGGERED 10 s, open x3.0, charges every 3.5 s (the
  tail). The breach under the player, the trail and the second dive are not built (advanceAfterAggro / diveAfterLunge trials measured and
  left at 0).
- **G28-G30 brawl47:** phases at 66 / 33 %, rush 0.95 s, block 0.9 s, rests 1.8 / 1.0 s, meter 9 / 12, chip x0.6, phase 3's two chargers and
  their drain. Human median 50.9 -> 71.6 s: still under the band. The look, animation and hit effects (brawlLook47) are not started.
- **G33:** THE SLAG ISLANDS at 6.0 m in n47-kiln (data); the breach rule and the boost policy (its meter) are round 2.

## 5c. Rounds 2-3 as built (2026-10-19 00:52-03:00; n47human 40 seeds unless named; median / p90 / min / killed)
- **THE ICEBREAKER (brawl47):** THE ICE ARMOUR (each phase starts armoured: x0.25 outside STUCK / WINDOW until the phase's first window,
  which lands x1; phase 3's armour is the herd's: a charger into the boss or the chargers dead break it): 91.8 / 105.2 / 70.0 / 40: IN.
  View: springs, yellow windows, four glowing frost plates while armoured, an ice burst at each 20 % of the bar.
- **THE KILN WYRM (wyrm47):** kept: tells 0.9-1.2 s (crack, spit, core, lunge, squeeze, sweep), the dive never cut, STAGGERED 10 s x3:
  122.6 / 256.9 / 45.6 / 38. Measured and left at 0: the head charge at 0.9 s (fixed: bot 3/10; tracked + window 0.6: 9/40), THE LEAN
  0.4 s before the 0.5 s tell (15/40; run into the tell: bot 7/10), a window cap (0.2: 214.2; 0.3: 156.9), a 3.5 s charge cooldown (245.5),
  spit 5 / head 12 (159.5, 33/40), the coil skip (189.2) and the coil cut at half (280.8, 21/40). The tail's hits (`--b47-tally`, slow
  fights): body contact 189, spit / shards 163, head 87, lunge 16. **With the boost policy (`--b47-boost`: a jump + THUNDERCLAP-down chain
  on a crack tell or an open core) the same fight is 96.7 / 159.6 / 46.8 / 39 (n46-kiln) and 97.1 / 160.2 / 46.8 / 40 (n47-kiln's
  islands): the boost is the answer; the p90 comes into the band, the min does not.** Boost chains a minute (jump2): 5.3 vs 5.6 (x1.06):
  the islands do not change what a fixed policy does (G33's 2x is not met that way).
- **THE STAMPEDE (G27):** the glove lands nothing on the prow, the KESTREL takes a free charger's back x3: g8 gauntlet-only 84.8 s / 5
  deaths vs mixed 46.0 s / 1 death (n46flow, 20 seeds); level 2's walk 10 / 10.
- **G35 beats (n47-kiln):** n47b beats: still 2 / 2 cameo hits, jumping 0 / 2; rows off none. The walk 10 / 10.

## 6. Hand-offs and calls for the lead
- F (copy47): my lesson / banner strings above are drafts against section 3's copy rules; F owns the final words and their system.
- F (visor47 / foes47): the CHARGER's chevron visor follows F's visor spec; I build the prow, horns, vents and pose.
- The human-model pilot: I need a policy hook for (a) the stampede's two policies and (b) the wyrm's boost policy; I write both in my own
  harness file once the pilot's header names the hook.
- R-3 is broken on purpose in g1 (two new roles: the tank and the charger), for GG's "appear earlier and more frequently".
- GG's two worm words disagree on their face (10-09 "the old worm, not the Tripo one"; 10-18 "the same yellow-red-orange colour theme as
  before"): my call is the 10-09 shape in the 10-18 colours; the lead may overrule.

# NIGHT47 builder L: cards (round 0; the lead assembles docs/BOSSES47.md / docs/ROOMS.md from them)

Rows: loom47 (sim), loomLook47 (view), span47 (level n46-span -> n47-span); crown47 (sim), crownLook47 (view), spire47 (level n46-spire ->
n47-spire). Research lines: Research/2026-10-18-wave47/L.md (cited as [L.md §n]). Baselines are L.md §2 (measured tonight). Rule ids:
DESIGN-RULES B-1..B-5, E-4, A-1..A-9, C-1/C-2/C-3, M-3, V-2, T-3. Numbers marked "ours" are first picks, tuned by the meters; "read" = the
code / config today. Pools only go down; nothing the player had is lost (VERBS rows named per change).

---

## 1. THE LOOM boss card (G36-G38, G40)

| field | THE LOOM (NIGHT47) |
|---|---|
| Fantasy (one line) | The riggers' cable loom, the machine that spun the span's cables, gone feral at the canyon's narrows: it hangs in its own frame over the drop and weaves anything that moves into its warp. |
| What GG said | "a much better design with colors that match its theme, far more fluid animations, and more varied attacks ... looks stiff, ugly, and doesn't incentivize movement; the arena isn't fun ... new arena, new design, new colors, new animations, new attacks, and proper multi-phase structure that encourages slow-motion grapple mechanics" |
| What stays (B-5, VERBS 46) | the six knees as moving grapple nodes (the hook IS the fight: Doom's "the hook is smart because it is selective" [L.md §1]); the pull-away tear (VERBS 46, unchanged); the YELLOW spool parry; the legs-are-places structure (a phase ends when its window closes, so the pool never gates it); the 13 existing event names |
| Silhouette | a SHUTTLE: one long horizontal body (8 m), pointed at both ends (triangles = danger), a brass spool drum at its middle (the hub the threads come off), six long two-segment legs braced on the arena walls (the feet plant: a spider read at 40 m), a harness of 12 taut warp threads fanning up from its back to the frame beam (the loom read: "it is strung in"). Distinct from THE WICK (a tall stack) and THE ICEBREAKER (a squat ram): it is wide and flat, seen from below |
| Palette (level 4's theme: iron, brass, amber lamps at dusk; ROOMS.md level 4 lamps are amber) | body: blackened iron (dark, the E-3 rule) with brass trim on the drum and the leg joints; its lamps (two per side, the eyes of the shuttle): amber at rest, red-orange in a RED tell, yellow in a YELLOW tell (the parry language, one colour says "now" [NIGHT47 §3]); the warp threads pale amber at rest, red-orange when an attack runs along them; the KNEES cyan (player-usable: grapple nodes); the seam on a hooked knee YELLOW (punch / shoot it now); the spinneret core CYAN only in the window. Phase 2 recolours (Gabriel [L.md §1]): the brass goes hot amber, the lamps red-orange; phase 3: the warp burns red-orange end to end, the drum glows from inside |
| Animation (G37; NIGHT47 §3 animation rules, [L.md §1 spider]) | code springs, view only (the sim's knee positions are the truth; the view never moves a hit box): the legs step procedurally: each foot keeps its plant on the wall until the knee is > 3 m from its rest, then steps in an arc (0.35 s, lift 1.5 m, overshoot 0.2 m, settle spring) with diagonal pairs alternating; the body = the feet's average + a 2.4 s breathing hover (read: bobPeriodS) with a 4 deg roll lag on every glide (secondary motion); every attack has anticipation (the body drops 0.8 m and pulls its legs in over the first 40 % of the tell), a strike (a 0.12 s snap) and an overshoot (0.3 s spring back); the warp threads sag and twang (a 2nd-order spring per thread, a pluck on every attack and every tear); a 60-100 ms flinch spring on every hit (the drum recoils from the hit's side); a torn leg falls limp and swings (pendulum spring), its thread lashes free |
| The arena's job | THE LOOM FRAME (section 3): no floor, four corner posts and the knees: the only way to reach a weak point is a hook, and every hook is a BEND. The arena changes with each phase because the boss cuts its own frame (Ueda: boss and arena designed together [L.md §1]) |
| The rule (all phases) | Hook a lit (cyan) knee: the latch starts a BEND (VERBS 22). Inside it the knee's SEAM shows YELLOW: shoot the seam (any gun, `seamDamage` total) OR pull away 7 m (the old tear, VERBS 46): it tears. Two tears (+ one parried spool in P2-P3) empty the meter: the spinneret drops 4 m toward you and opens CYAN for `windowS` 4.0 s (today 2.2 s: B-2 "longer than a combo plus the travel" [L.md §1 Birkhead]); every hit lands full there. Outside the window every hit chips at `chipScale` 0.30 (today 0.02: the spec's chip >= 30 %, "you can barely damage it") |
| The BEND rule (the star, G40) | loom47 refills the BEND meter on THE LOOM's right moves: a tear `bendOnTear` +0.5, a parried spool `bendOnParry` +0.5, each phase start `bendAtPhase` full (read today: the meter is full once a run and refills only on kills; THE LOOM's fight has no kills, so every bend after the first is a 1 s SHORT one: L.md §2). So every second knee hook is a FULL 2.5 s bend: the seam shot is made in slow motion, and the instant swap (VERBS 23) inside it is the way to switch to the gun that fits (the THUNDERCLAP up close, the KESTREL charge from the post). Witch Time's rule: the slow motion is earned by the right move and spent on the punish [L.md §1] |
| Length target (human band, NIGHT47 §1) | P1 30-40 s, P2 35-50 s, P3 25-45 s, two 1.5 s transitions: median 90-150 s, p90 <= 180 s, min >= 60 s (n47human, 40 seeds). Today's bots: skilled 48.7 s, casual 55.5 s (L.md §2); length comes from the road to each window and its 4 s, never from HP (the pool stays 750 x the scale or goes down) |
| Verb it stars | the BEND (VERBS 22-23) through the hook (VERBS 17, 18, 20); the parry (spools); the ride on the side walls (VERBS 9) between hooks |

**Per phase** (P = the brain's leg; tells 1.0-1.5 s, the sweep 2.0 s as today, heavy <= 2 s [L.md §1]; every tell moves differently; no two
tells at once (Boss37 rule 7, read)):

| phase | attacks (name: tell (what, how long) / counter (verb) / punish window / hit effect) | rule change | BEND uses (>= 3) | transition beat |
|---|---|---|---|---|
| **P1 THE FRAME** (teach: 3 attacks, generous) | **THREAD LASH** (kept): the near legs rear, a red-orange line glows from the drum to where you stand, 1.2 s / step off the line: dash, hook sideways (a latch = a bend: you watch it miss) / its near knee dips 1.5 m and stays lit 1.0 s after the strike / 30 dmg + an 8 m/s shove, red-orange spark line on the body. **SPOOL SHOT** (kept): the drum turns, a yellow spool swells at the spinneret, 1.2 s / punch it back (VERBS parry) / a returned spool staggers it 1.5 and fills the bend +0.5 / 25 dmg, a yellow burst; returned: a cyan streak, a drum flinch. **FLOOR SWEEP** (kept, sweep41): a red-orange decal line across the well's ledge level, 2.0 s / height: a post top (y 4 is over P1's topY 3), a ride, a hook / 1.5 s recover with its legs spread (all four live knees in reach) / 15 dmg + a lift | none: the frame stands; four posts; the knees of pair 1 lit | knee 1 (bend), knee 2 (bend, FULL after the first tear's +0.5), the window's whip onto the spinneret (a latch: bend) = 3 | the window closes: THE LOOM screams (a 1.5 s beat: the lamps flare red-orange, it lunges up 4 m, the warp twangs), cuts two of its own frame ropes: the two far posts fall into the void (a layer swap, between phases only: Titanfall's rule [NIGHT47 §3]); banner "It cut its frame." (4 words, >= 36 px, upper third; sentence case per F's docs/COPY47.md) |
| **P2 THE WEAVE** (the rule changes) | lash, spool, sweep (now topY 11: the posts are swept, the answer is a hook or a ride) plus **SHUTTLE DIVE** (new): the body rears, its pointed nose lamps go white-hot red-orange and a lit line is drawn on the warp from it to your perch, 1.3 s / hook away (any ring or knee: the latch's bend shows the dive pass) or ride off the wall / it sticks nose-down at the perch 1.5 s: its two low knees within 8 m and lit (the best tear of the fight) / 35 dmg + a knock off the perch (a fall = a lip return, -10). **WARP CAGE** (new): four warp threads rise out of the void round you and glow red-orange, tightening, 1.5 s / out over them: a hook or a jump-dash / the threads it used stay 4 s as cyan grapple lines (two extra nodes) leading to a knee / 25 dmg + a 1 s snare (slowed 50 %) | two posts gone: the near pair + the side walls' ride panels + the rings are the footing; the knees of pair 2 lit and 4 m higher (only a hook reaches them); a parried spool is now part of the meter (read today: legs 2-3 stagger 7.5) | knee 3, knee 4, a dive escape or the cage escape (each a latch) = 3-4 | the window closes: it drops 6 m and hangs from two legs, swinging (the swing: swingAmpM 7 m over 3.2 s, read from o2's anchor); the warp catches fire red-orange end to end; banner "It's falling. Finish it." (4 words) |
| **P3 THE UNRAVEL** (P1 remixed harder) | combined, shorter gaps (rest 2.5 -> 1.5 s), tells still >= 1.0 s: **BOBBIN RAIN** (new): the drum shakes, three spools drop in a line along your path: the middle one YELLOW, the outer two red-orange, 1.2 s / punch the yellow one back, dash past the red / the yellow return staggers 1.5 + bend +0.5 / 20 dmg each. LASH + SWEEP chained (a sweep's recover can be followed by a lash, never two tells at once). SHUTTLE DIVE from the swing (its arc is the swing's: readable) | it swings: its knees are FAST moving nodes (the hook needs a lead; the BEND makes it possible: hook, then aim the seam in slow motion); the near posts are swept every cycle (topY 11) | knee 5, knee 6, the final whip onto the spinneret = 3 | the last window: it falls into the canyon (kept); the finisher beat: a 0.4 s slow motion on the last hit (R4 list: "a boss finale beat") |

Attack count: 7 (THREAD LASH, SPOOL SHOT, FLOOR SWEEP, WEB WALL (kept as the camp answer: a sheet between the two near posts when you camp
one 4 s), SHUTTLE DIVE, WARP CAGE, BOBBIN RAIN): >= 6 (G38). Event names (new, web style, for GG's sounds): `loom47Dive` (stage tell /
strike / stick), `loom47Cage` (tell / snap / free), `loom47Bobbin` (tell / drop / parry), `loom47Seam` (open / hit / tear), `loom47Cut` (the
frame cut, P1 -> P2), `loom47Swing` (P2 -> P3).

Numbers (all in design.n47.loom47.numbers with a `_why`; ours, first picks): chipScale 0.30, windowS 4.0, coreDropM 4, seamDamage 60 (read,
PortExport/config.json weapons: the KESTREL tap 25, the THUNDERCLAP 8 pellets x 8 = 64: three taps or one close blast tear it inside a
2.5 s bend; later rows' damage multipliers on these guns: not checked), bendOnTear 0.5, bendOnParry 0.5,
bendAtPhase 1, dive {windup 1.3, active 1.0, stick 1.5, damage 35}, cage {windup 1.5, damage 25, snareS 1, lineS 4}, bobbin {windup 1.2,
damage 20, spacingM 4}, p3RestS 1.5. Not checked yet: the KESTREL / THUNDERCLAP damage per shot on a knee (round 1 reads them before the seam
number is fixed).

---

## 2. THE CROWN boss card (G41, G43 boss part)

| field | THE CROWN (NIGHT47) |
|---|---|
| Fantasy | The spire's crown: the ring of gem-set bell blades that topped the bell tower, torn loose in the quake; it hangs over the broken shaft, still ringing, and burns whoever climbs down to the bells. |
| What GG said | "needs an overhaul because it's not fun, the level doesn't make sense, and the flamethrower attack looks ugly, doesn't match the game's style, and lacks weight" |
| Why it is not fun (measured, L.md §2) | it is mostly falling: the casual pilot falls 10 times a fight (103 in 10) and each fall restarts the leg; guns say BLOCKED outside the window (chip 0); its one rule (pass through the ring) is the same move for 20 gems; there is no flame at all (the flame GG saw is the TORCH) |
| What stays (B-5, VERBS 8) | the pass: the jump-dash THROUGH the ring (VERBS 8; level 5's verb, taught v3-v10); the gems as the bar (20 x 100, boss39); the descent summit -> shaft -> plaza (the spire you go down); the YELLOW gem shard; the finisher (KESTREL charge / THUNDERCLAP slug through the opening); every existing event name |
| Silhouette | a CROWN, not a saw: a 6 m ring of eight tall gem blades standing up like a crown's points (triangles), tilted 20 deg toward you, a bell's clapper hanging from its centre (the flame's nozzle and its weak point in the window), turning slowly. Seen at 40 m: a spiked circle with a hanging weight |
| Palette (level 5's theme: stone, bronze bells, amber lamps: ROOMS.md v2 / v6) | the blades dark bronze with a gold edge; the gems red-gold (enemy family, warm), each gem's light dimming as it breaks; the clapper white-hot in a flame tell; red-orange tells, yellow shards; CYAN only on the opening in the window (player-usable: "go through / shoot here"). Phase 2: gold -> hot red-gold, the ring spins faster; phase 3: the blades glow red-orange from inside |
| Animation | springs (view only): the ring rocks like a struck bell after every pass (a damped 1.2 s swing of 8 deg), the clapper swings with its own lag (follow-through), anticipation before each attack (the ring rears back 10 deg over the first 40 % of the tell), a flinch on every hit, a broken gem blows out with a shatter (crown38 kept) and the ring dips 0.3 m |
| The arena's job | THE BROKEN SHAFT (section 4): wide footing per phase, the descent as the phase change, no restart on a fall |
| The rule (all phases) | Pass through the ring (a jump-dash, or in P2 a slam down through it): it CRACKS: a gem breaks (`passGems` 1, 2 with the TORCH burning: kept) and the ring opens CYAN for `windowS` 3.5 s: every gun lands full on the gems and the clapper. Outside the window guns chip at `chipScale` 0.30 (today 0: "BLOCKED"), the TORCH within its 7 m at 0.6 (level 5's weapon must count: R-1's 3 s verb) |
| Falls (the fun fix) | a fall never restarts the phase: the feet are caught on the next floor down (the shaft's gallery in P1-P2, the plaza in P3) for `fallHp` 5, and the CROWN follows down; the phase only moves on when its gems are gone (read today: a fall = the leg's checkpoint + fallDamage 5) |
| Length target | P1 30-45 s (7 gems), P2 30-45 s (7), P3 30-40 s (6 + the finisher), two 1.5 s beats: median 90-150 s, p90 <= 180, min >= 60 (n47human). Today: skilled 36.3 s, casual 58.4 s (L.md §2). Length from windows and phases; the pool stays 2000 or goes down |
| Verb it stars | the jump-dash through the ring (VERBS 8), the slam (VERBS 7, P2's dive pass), the parry (shards), the TORCH (level 5's weapon), the ride on the core's panels (VERBS 9) |

| phase | attacks (name: tell / counter / punish / hit effect) | rule change | transition beat |
|---|---|---|---|
| **P1 THE SUMMIT** (teach) | **BLADE RING** (kept): the blades flare red-orange and the ring spreads, 1.2 s / jump or drop under the cut at chest height / 0.3 s recover, ring flat: an easy pass / 25 dmg, a red-orange slash. **GEM SHARD** (kept): one gem flares yellow, 1.0 s -> 1.2 s (read today 1.0: under the 1.0-1.5 rule's middle) / punch it back: a socket shatters / 15 dmg. **CROWN FIRE** (new, the boss flame: section 5): the clapper glows white, the ring tips toward you, a red-orange line burns along the floor where the cone will sweep, 1.3 s / get over it (jump, jump-dash THROUGH the ring above the cone: the flame is under the ring) or behind a summit slab / it vents 1.5 s after the sweep: the ring hangs still and flat, the easiest pass of the fight / 8 dmg a tick at 0.25 s + a burn state 1.5 s | the summit slabs all stay (no orbit platforms needed to stand: today's orbits are P1's bonus route, not its floor) | the 7th gem: the summit cracks (1.5 s: the ring rises and tolls once, every gem flares, dust), the floor breaks in a ring around the core and you drop to the gallery (caught, no damage); banner "Down the shaft." (3 words) |
| **P2 THE SHAFT** (the rule changes) | **BEAM CUT** (kept, the gallery slab you head for drops: back in 6 s). **BELL TOLL** (new): the ring stops and rings: a gold ring of sound grows from it, 1.3 s / jump over the wave as it passes (it runs along the gallery at feet height) or be on a ride panel / after a toll the ring hangs 1.5 s in the shaft's middle / 20 dmg + a 6 m/s push outward. **FIRE SPIRAL** (new, CROWN FIRE's P2 form): the ring lies flat under you and pours its flame down round the core, 1.4 s / ride the core's panels above it, or drop through the ring after it passes / its flame trail leaves the ring flat beneath you: the dive pass (VERBS 7 slam through it) breaks 2 gems / 8 dmg a tick + burn | the ring lies FLAT in the shaft below you: you pass it by DROPPING through it (a slam through = 2 gems); the jump-dash pass still works; the gallery spiral is the floor, the core's ride panels the fast way down | the 14th gem: the gallery falls away floor by floor (1.5 s), you land on the plaza; the ring drops to the plaza and lands (landY, read); banner "Break THE CROWN." (3 words) |
| **P3 THE PLAZA** (P1 remixed harder) | BLADE RING + CROWN FIRE chained (a fire's vent can be followed by a blade ring: never two tells at once); **SHARD VOLLEY** (GEM SHARD x3, one yellow, two red, 1.2 s); **PLAZA DROP** (kept: the sector nearest you drops) | the ring on the ground, tall: the pass is a jump-dash through its upright opening; the plaza's sectors drop under the habit | the last gem: the finisher (kept: a KESTREL charge or a THUNDERCLAP slug through the opening), a 0.4 s slow motion on the hit |

Attack count: 8 (BLADE RING, GEM SHARD, BEAM CUT, LOW FIRE (kept, the air habit), PLAZA DROP, CROWN FIRE, BELL TOLL, FIRE SPIRAL; SHARD VOLLEY
is GEM SHARD's P3 form). New events: `crown47Fire` (tell / sweep / vent / burn), `crown47Toll` (tell / wave / pass), `crown47Spiral` (tell /
pour), `crown47Crack` (a pass opens it), `crown47Catch` (a fall caught on the floor below). Numbers in design.n47.crown47.numbers (ours):
chipScale 0.30, torchChip 0.6, windowS 3.5, fallHp 5, fire {windup 1.3, sweepS 1.5, arcDeg 90, reachM 9, tickS 0.25, tickDmg 8, burnS 1.5,
ventS 1.5}, toll {windup 1.3, speed 14, height 0.9, damage 20, push 6}, spiral {windup 1.4, pourS 2.0}, shardWindup 1.2, slamGems 2.

---

## 3. THE LOOM's new arena: THE LOOM FRAME (o3 in n47-span; A-9 card + flow plan; G39)

o3 is level 4's last room, so its rect may change (nothing after it moves). Today: 44 x 140 m, a linear canyon crossing (L.md §2). New: the
same width (x -39..5: the feet's 21 m span, read: footSpanX), 56 m long (z -250.5..-306.5): an 8 m start ledge + a 48 x 44 m well. The door
from o2 (d7: x -19..-15, z -250.5, a checkpoint; the room's spawn (-17, 0, -252.5): read, Design/levels/n46/span.plan.json) is unchanged. Generator: Tools/night/gen-n47-span.cjs
(from gen-n46-span.cjs; p1-p5 / o1 / o2 copied unchanged; the generator throws if any other room changes). Movement numbers it uses (read):
grapple range 40 m, 2 charges (one back per 4 s, tearRefill +1), a pull's arrival carries ~5-6 m past its ring (ROOMS.md n46-span), jump 1.5 m,
double 2.7 m, ride 10.45 m/s, the jump-dash ~21 m flat.

| field | THE LOOM FRAME |
|---|---|
| Title | THE LOOM FRAME (banner "4.10 THE NARROWS") |
| Place fantasy | The riggers' loom-house at the canyon's narrows, where the span's cables were spun on the great loom before they were strung; when the span was cut the loom tore free of its floor and now hangs in its own frame over the drop, its feet braced on the walls. |
| Spatial law | shaft (a frame over a void): four corner posts, two side walls, nothing in the middle but the machine |
| Verb it celebrates | the BEND: every hold that reaches a weak point is a hook, and every hook is a slow-motion beat |
| The 30 s problem | THE LOOM hangs over a well with no floor and its lit knees are the only holds near its seams: hook a knee, and in the slow motion shoot the seam before the lash lands; the four posts are your only rest and it sweeps them. (o1-o2: rehearse a pull / a tear with no enemy; p1-p5: other jobs, ROOMS.md) |
| Molecule | nodes: S the start ledge (the door), P1-P4 the corner posts (5 x 5 m tops at y 4), WW / WE the side walls' ride panels (cyan, y 2..12), R1-R6 rings on the frame beam and the walls (y 10-14), K1-K6 the knees (moving), C the spinneret (the window). One-way: S -> P1 / P2 by a hook only (the ledge is at the door's y 0, levelgen doors carry no height; the posts' tops are 4 m up, past the double jump's 2.7 m: the first hook is the commit); a post -> the ledge or the void's lip return is the way down; every knee -> anywhere (the BEND's release carries you). Two-way: post <-> post by the ride panels (ride + jump off) or a ring. The earned shortcut: knee to knee inside one BEND (hook the second knee before the slow motion ends: one bend, two tears) |
| Landmark | the FRAME BEAM: a roof-high iron beam across the far end (z -306) carrying the warp threads up to the machine, with the room's two amber lamps; from the door: THE LOOM in front of it, the warp fanning up |
| Hero prop | the HALF-WOVEN SPAN: a frayed cable bridge hanging from the machine's spinneret toward the far wall (it was weaving the next span): in play, P1's sweep rides along it and P2's WARP CAGE threads come off it |
| Hazard both ways | the void (fall: a lip return to the last post stood on, -10 health: the n46-span rule); no adds (a boss room): the hazard to the boss is its own: a torn leg's thread falls into the void and the machine sags (a knee lower, easier) |
| State change | P1 -> P2: it cuts its frame ropes: the far posts P3 / P4 fall into the void (a layer swap between phases); the lamps amber -> red-orange. P2 -> P3: it drops 6 m and swings; the warp burns red-orange |
| Light / colour story | the start ledge dark under the loom-house's roof lip (tight); the well open to the dusk sky; the two amber lamps on the frame beam the brightest points; cyan only on the knees, the rings, the ride strips; in P3 the burning warp is the light |
| Every-solid rule | the start ledge (the door, the first view, the commit drop); P1 / P2 (the near posts: the rest and the first hook's take-off; swept from P2 on); P3 / P4 (the far posts: P1's air loop's far end; they fall at P2: the arena shrinks forward); WW / WE ride panels (the post-to-post route and the dodge from a sweep); the frame beam (the landmark, the rings' hanger); six rings (the holds that are not the boss: the escape from a cage / a dive). Nothing else (no tower pairs, no anchor maze: cut) |
| Tell-a-friend line | "The loom hangs over a pit in its own frame; you hook its glowing knees, everything goes slow-mo, and you shoot the seam before it whips you." |
| Cut list | from o3 (today): the 140 m crossing and its three tower pairs (the fight traveled; GG: "the arena isn't fun"), the 13-anchor maze, the end ledge, the no-ride blink (the frame has only two ride panels, always on). From the walk / frames: round 1 |

| flow plan (Part D) | THE LOOM FRAME |
|---|---|
| 1 entry lip + drop in | no lip (the door is flush, y 0): the 8 m start ledge under the loom-house's roof lip shows THE LOOM (26 m ahead at y 18), the frame beam and all four posts in one view; the commit is the first hook up onto a near post (P1 / P2, 9 m ahead, 4 m up: the rings R1 / R2 hang over them, the pull's 5-6 m carry lands on the post); first foothold: the near posts |
| 2 tiers | 0 the start ledge; 4 the posts (4 x 25 m2; P1's sweep topY 3 is under them, P2-P3's topY 11 over them: read, design.n41.sweep41); the ride panels 2-12 m; the rings 10-14 m and the knees ~8-16 m (air); the void below (no floor) |
| 3a ground loop | none on purpose (a boss well: the posts are islands; "the hook is smart because it is selective" [L.md §1]); the loop is 3b |
| 3b air loop | P1 -> ride WW north (14 m, 1.4 s) -> jump off onto P3 -> hook R4 (the beam, ~12 m) -> P4 -> ride WE south -> P2 -> hook R2 -> P1: two rides + two hooks a lap (charges 2, one back per 4 s, a tear +1), never the void; each node two outs (a ride or a hook) |
| 3c the one-way | up only by a hook (the ledge -> a post, 4 m); down by a drop (a post -> the ledge, or a ride panel's end); at P1 -> P2 the far posts fall: the arena itself goes one way, forward |
| 3d two outs per node | S: P1 / P2. Each post: its ride panel / a ring (and the live knee). Ride panels: the far post / a knee. Rings: any post. Knees: anywhere (the bend's release) |
| 4 the pickup | one health on P3's top in P1 (the far post: bait across the well), gone when P3 falls; P2's health on the frame beam's ring R4 (a hook to it) |
| 5 cover | none on the posts (perches; Part D 4); the cover is the slow motion and the distance |
| 6 hazard's job | the void makes every reach a hook and every miss a lip return (-10); it never fills a loop (there is no floor loop) |
| 7 sides differ | west = the ride side (WW, the lash's side when you stand there), east = the hook side (more rings: R5 / R6 on the east wall), the far end = the beam (the landmark from every post) |
| 8 spawns | none (a boss room): THE LOOM's body glides inside the frame (loom40's clamp kept, margin 10 m); its stations from the level row (loom37.stations): P1 (-17, 18, -284), P2 (-17, 14, -280), P3 (-17, 12, -282) + the swing |
| 9 healing | P1's health on P3; P2's on R4; the tears give a grapple charge (read: tearRefill) |
| 10 between phases | the frame changes only at a phase's transition (posts fall at P1 -> P2; the drop + swing at P2 -> P3), never mid-phase |
| 11 the meter target | n47loom reach (the real Mover hooks each knee and ring from each post: every landing >= 2 m inside a post's edge); falls per fight <= half of today's casual (53 in 10 -> <= 27); the BEND >= 3 per phase for n47human (min over 40 seeds); n47human's band; the time on one post < 5 s (the sweep / web keep you moving); n46flow --level n47-span no worse past F1's band on p1-p5 (unchanged rooms) |

---

## 4. Level 5: the premise, the route, each room's job, THE CROWN's arena (G42)

**Premise (one paragraph, for the level start and docs/ROOMS.md):** THE SPIRE is the bell town's cathedral, built on the lip of a chasm,
its bell tower cut DOWN into the cliff below its far end (the spire of this town points into the abyss: its bells rang over the drop). The
quake broke it: the great bell fell through the nave (v2), the belfries' floors fell out (v3, v4, v7: you fly the gaps), the stair is falling
(v9), and the CROWN, the ring of gem blades that crowned the bell tower, tore loose and hangs in the open shaft at the cathedral's end, still
ringing. Its red-gold light rises out of the chasm. You walk the cathedral end to end toward that light, then go down the shaft after it.

**The readable route (the goal visible from the start and from each room; the wienie [L.md §1]):** THE CROWN'S LIGHT, a red-gold column of
light rising 300 m out of the shaft at the core's axis (-10, -289) (a view object, crownLook47; it brightens as you get closer and pulses
with the CROWN's toll). Every room's roof is broken over its exit third (the quake: a jagged slot, x -14..-6, the last third of the room's
length; the gate rooms keep their ceiling over the pit and open it over the landing only), so from the spawn and from each room's door,
looking ahead, the column stands in the slot above the far wall. Ray test: `n47spire view` (Tools/SimHarness/Harness.N47.Spire.cs, the world's collider): from
the eye (1.6 m) at v1's spawn (the start) and at v2..v10's entry spawns, the column's points every 10 m from y 10 to 300, inside the view
(bearing <= 53, |up| <= 37 deg) and clear: one point each. MEASURED today (n46-spire): **0 / 10** (every point in view is blocked: every room
is roofed), v11 the checker's control sees it (open sky). GREEN: 10 / 10 on n47-spire. Every exit door's
frame lit amber (A-6). Copy (F owns the strings; L proposes): the level start "THE SPIRE. Follow the light down." (6 words; sentence case per F's docs/COPY47.md, the name in capitals).

| room | its job in one line (the 30 s problem for fight rooms: ROOMS.md NIGHT46 cards stand) | NIGHT47 change |
|---|---|---|
| v1 THE LANDING | take the TORCH and see where you are going: the light through the broken vault ahead | the roof slot; the level's premise line |
| v2 THE NAVE / FALLEN BELL | the TORCH up close round the fallen bell (the bell fell from the tower you are walking to) | the roof slot over the apse (the hole the bell fell through is ALREADY "white roof panel" there: it becomes open sky) |
| v3 BELFRY I | learn to fly: one jump-dash over the fallen belfry floor | the slot over the landing only (the pit keeps its ceiling) |
| v4 THE STEP UP | fly onto a higher ledge | the same |
| v5 THE RINGERS' STAIR | a breather fight in the TORCH's lane | the slot |
| v6 THE ROPE CHOIR | fly between three heights and punch the drones' orbs back | the slot over the organ loft: the light behind the organ (its amber turns to the light's red-gold) |
| v7 THE LONG BELL | the exam at the jump-dash's limit | the slot over the landing |
| v8 THE VESTRY | rest, heal, and see the light close: the "picture spot" (Rogers [L.md §1]): the vestry's far wall has a tall window onto the shaft and the CROWN itself hanging in it | the window (view only: the CROWN's model at its reveal place) + the slot |
| v9 FALLING STONES | keep moving across stones that fall under you (the stair is collapsing) | the empty lesson gets a line (F: "The stones fall. Keep moving." 5 words); the slot |
| v10 THE RING | rehearse the pass: fly through a spinning ring | its far wall OPEN above the door: through the ring you see the CROWN's light: the rehearsal lines up with the fight |
| v11 THE SPIRE | go down the shaft after the CROWN (the boss card, section 2) | THE BROKEN SHAFT card below |

**v11 THE BROKEN SHAFT (A-9 card, the boss arena; geometry changes in n47-spire only, under spire47; the sim's rules under crown47):**

| field | THE BROKEN SHAFT |
|---|---|
| Title | THE BROKEN SHAFT (banner "5.11 THE SPIRE") |
| Place fantasy | the bell tower cut down into the cliff: its summit at the cathedral's floor, its stair spiralling down round the bell shaft's core to the bell plaza at the bottom; the CROWN hangs in it |
| Spatial law | shaft: three floors stacked round one core (the summit, the gallery spiral, the plaza) |
| Verb it celebrates | the jump-dash through the ring (level 5's verb), then the slam through it (P2) |
| The 30 s problem | The CROWN hangs over the shaft and burns the floor you stand on: fly through its ring to crack it open, then burn and shoot it before it closes; every phase it drags the fight one floor down. (v10: the ring with no enemy; v6: heights with drones) |
| Molecule | S the summit (y 0, today's four slabs + the bridge, all kept in P1), O the orbit platforms (P1's bonus air route, kept), G the gallery spiral (8 slabs, -30..-51), CP the core's ride panels (-50..-29), PL the plaza (y -60). One-way: S -> G (the summit's break), G -> PL (the gallery's fall). The earned shortcut: ride the core's panels down past the spiral |
| Landmark | the CROWN itself and its light column (the level's wienie: the same light) |
| Hero prop | the core column (the spire's spine): the thing the CROWN circles, the ride panels down |
| Hazard both ways | the drop between floors: a fall is CAUGHT on the floor below for 5 hp (crown47: no leg restart); the CROWN's falling pieces (beam cut, plaza drop) are its hazard to you |
| State change | P1 -> P2 the summit breaks; P2 -> P3 the gallery falls; the light goes gold -> red-gold -> red |
| Light / colour story | the summit under the open dusk sky; the shaft darker, lit by the CROWN's gems and its flame; the plaza lit red from the chasm below |
| Every-solid rule | as today (read: the summit slabs, 12 orbit platforms, 8 gallery slabs, the core, 8 plaza sectors) except: the summit's slabs stay for all of P1 (today it shatters at the reveal's end: L.md §2) and a CATCH floor under the orbits at the gallery's top (y -30) so a P1 fall lands on the gallery, not the leg's checkpoint |
| Tell-a-friend line | "A crown of blades hangs in a bell shaft; you jump-dash through it to crack it open, then it breathes fire and drags you down the tower floor by floor." |
| Cut list | the leg restart on a fall (measured: 103 falls in 10 casual fights); from the walk / frames: round 1 |

Flow plan (Part D, the boss shaft): 1 entry: the door v10 -> the summit's bridge (kept), the CROWN rising over the shaft 15 m ahead; 2 tiers:
summit 0 / orbits -8 / -15 / -22 / gallery -30..-51 / plaza -60; 3 loops: P1 the summit's ring round the core (ground) + the orbits (air);
P2 the spiral (one way down) + the core's panels; P3 the plaza ring (ground) + the sectors' drops; 4 perches: the orbit platforms (no cover);
5 cover: the summit's slabs' edges (CROWN FIRE's answer); 6 hazard: the drops (caught); 7 sides: the core splits every floor; 8 spawns: none;
9 healing: one health per phase on its floor (today: not checked); 10 the floor changes only at a phase's beat; 11 meters: falls per fight
(n47crown: today casual 10.3), the phase restarts per fight 0, n47human's band.

---

## 5. The boss flame spec (G43, boss part: CROWN FIRE and FIRE SPIRAL; the TORCH is F's)

NIGHT47 §3's flame spec, red-orange family (an enemy flame, V-2), with the layered build [L.md §1 fire]:
- **The sim** (crown47): a cone from the clapper (the nozzle), `reachM` 9, sweeping `arcDeg` 90 over `sweepS` 1.5; a tick of `tickDmg` 8 every
  `tickS` 0.25 to a player inside the cone and under the ring's plane (the flame is UNDER the ring: being above it is the answer); a hit sets
  `burnS` 1.5 (the view's burn state on the player: a red-orange rim on the HUD's health bar, never a full-screen tint). Its floor line (the
  tell) is exactly the cone's width at the floor (Riot: never more room than the attack gives [L.md §1]).
- **The tell (1.3 s):** the clapper heats white over the first 0.8 s (a glow ramp, a light growing 0 -> 2 on the summit), embers gather INTO
  the nozzle (inward particles), the ring tips 10 deg toward you (anticipation), and a red-orange line scorches along the floor where the cone
  will pass, drawn from its start edge to its end edge across the tell (it says where and which way).
- **The flame (view, crownLook47):** a mesh stream, not a particle pile (Company of Heroes [L.md §1]): a cone of 3 scrolling flipbook-like
  layers (posterized noise, time-offset), white-hot core at the nozzle -> yellow -> orange -> red-orange tips, additive; heat haze at the cone's
  edge (a distortion band, not over the reticle's centre box); embers (~60, stretched by speed, falling and bouncing on the floor); smoke at the
  tips (dark, slow, 1.5 s); a flickering point light on the floor and walls (a scripted slow curve, low amplitude: G11's lava rule); scorch
  decals where the cone passed (fade 4 s).
- **Weight:** a pressure kick on start (the ring recoils 0.4 m opposite the cone, a 0.15 s spring), a continuous low shudder on the ring while
  it pours, a lick on release (the cone's tip curls up and out over 0.25 s), then the vent (smoke from the blades' tips: the punish window says
  itself). Camera: a light trauma (+0.15) on the player only when a tick hits.
- **FIRE SPIRAL** (P2): the same layers poured straight down from the flat ring, sweeping round the core (one full turn over `pourS` 2.0 s).
- **Effects intensity / reduced motion:** every layer scales with EFFECTS INTENSITY; heat haze and the shudder off under reduced motion.
- Done when (G43 boss part): frames of the tell's start and its last 0.2 s, mid-sweep, the release lick, the scorch after; a contact sheet
  (-2 .. +30 frames from the first tick); a blind pair between two looks (the mesh-stream look vs a particle-cone look), 3 judges.

---

## 6. PROOF plan (RED once, GREEN once: L-014; frames named for the lead)

| G row | meter (RED = row off / today; GREEN = rows on) | frames (reel phase, level, moment) |
|---|---|---|
| G36 THE LOOM's design + colours | a grayscale silhouette at 10 / 20 / 40 m (the shuttle + legs read); palette hues logged (no cyan except knees / core / rings; no magenta) | a447, n47-span o3: the entry view (loomLook47 off = RED today, on = GREEN), P2 recolour, P3 burning warp |
| G37 fluid animation | the view's spring traces: foot steps per 10 s > 0, every attack's anticipation share of the tell 30-50 %, a flinch on 100 % of hits (logged by the view in the reel) | a447: a contact sheet of a lash (tell start .. strike +30 frames); a leg step sequence (6 frames); a hit flinch |
| G38 >= 6 attacks | `n47loom base --rows loom47 --level n47-span`: 7 attack ids seen over 40 seeds, every wind-up 1.0-1.5 s (sweep 2.0); RED: today 3 seen (web 0) | a447: each new attack's tell start + last 0.2 s (dive, cage, bobbin) |
| G39 the new arena | `n47loom reach` (new: every hook from each post lands >= 2 m inside); falls per fight <= 27 in 10 (casual; today 53); time on one post < 5 s; n46flow p1-p5 unchanged | a447: from the door (the frame, the machine, the posts), the P1 -> P2 posts falling, a hook into a BEND |
| G40 phases + the BEND star | n47human --boss loom (40 seeds): median 90-150, p90 <= 180, min >= 60; BEND starts >= 3 per phase (min over seeds; my harness counts bend34Start per leg under the pilot's hook) and FULL bends >= 1 per phase (today: 1 a fight); RED: n47human on today's fight (the lead's G0 baseline) | a447: a FULL bend on a knee with the yellow seam, the seam hit, the tear, each transition beat |
| G41 THE CROWN rebuilt | n47human --boss crown: the band; falls caught 100 % (0 phase restarts); chip share of dealt >= 15 % (today 0); RED: n47crown base today (skilled 36.3 s, casual 58.4 s, 103 falls / 10) | a547: each phase's entry, a pass + crack, the window, each transition |
| G42 level 5 makes sense | `n47spire view`: the light seen from v1-v10 10 / 10 (RED measured today: 0 / 10, the v11 control sees it); the walk 10 / 10 (`n46spire walk` on n47-spire); n46flow --level n47-spire no worse past F1's band | a547 (or a246's door frames on n47-spire): the spawn view, each door's view (the light in the slot), v8's window, v10 through the ring |
| G43 boss flame | crown47's tick hits logged (damage, burn); the floor line width = the cone width at the floor (harness) | a547: CROWN FIRE tell start, last 0.2 s, mid-sweep, the lick, the scorch; FIRE SPIRAL mid-pour; a contact sheet; a blind pair (2 looks) |

Order if short (one attempt each first, then depth): G40 (the BEND rule + the band), G38 (the new attacks), G41 (falls caught + chip + the
window), G42 (the slots + the light, the ray test), G39 (the arena), G43 (the flame sim, then its look), G36 / G37 (the looks), then frames.
Hand-offs (for the lead to pass on): F: the strings above (the level-5 start line, v9's lesson, the four banners), the burn-state rim on the
health bar (F's HUD), the reticle centre box rule for the heat haze; the human-model pilot: THE LOOM's policy (hook a lit knee, shoot the seam
inside the bend; the policy goes in my harness file through the pilot's hook once its header is read).

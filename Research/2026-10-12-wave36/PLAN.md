# The wave 36-38 overhaul plan (2026-10-12; draft for GG; decisions in DECIDE36.md)

Read REVIEW.md first. This plan answers GG's message point by point and changes HOW we build, not only what. All numbers below are
starting points to tune on Player frames; every gameplay number goes in config (CLAUDE.md rule 5), new ones in a Unity-side
`design.n36` block.

## 1. Direction: movement is the weapon (recommended; D-1)
GG asked whether to pivot to fists, explosions and 3D platforming. Recommendation: **pivot the centre of the game, keep the guns.**
Rule 1 (nothing lost) stays: every verb in docs/VERBS.md still works. What changes is what the game pays for.

- **The parry is the centre (GG: "an understanding of parry mechanics").**
  - Every enemy attack shows a tell for about 1-1.5 s: a body pose plus a colour flash. YELLOW means punch it (parry); RED means dodge it.
  - A parry freezes the screen 0.25 s, sends projectiles back as an explosion, heals, and refills one dash (ULTRAKILL: parry TrueStop 0.25, heals and refills).
  - Taught in L1 as a verb, never as a death tip.
- **Speed pays.**
  - A punch thrown while dashing, sliding, riding or pulled by the grapple LAUNCHES the enemy.
  - A launched enemy that hits another, or a wall, explodes (GG: "explosions").
  - Standing still pays nothing.
- **Heal by aggression.** A kill within about 4 m spills health (ULTRAKILL's blood rule; numbers in research/rea-ultrakill-combat.md). Health pickups stay (rule 1). Camping starves you.
- **Every gun does one movement thing too.**
  - KESTREL: an instant tap, and hold to charge.
  - THUNDERCLAP: its blast pushes you (a shotgun jump).
  - STITCHER: nails stick, then ZL detonates them.
  - Combos come from switching (QUICKDRAW and ARSENAL already exist).
- Rejected: a fists-only pivot. It throws away rule 1, the weapon-per-level progression GG asked for (w29, w30), and the switch combos (w33). Anger Foot's own lesson is that a kick-only game needs constant new variety (design-web.md). Kept as D-1's option B.

## 2. Feel fixes (night 36, first block; each a Player frame proof)
| fix | what | where |
|---|---|---|
| F1 the pistol fires on the press | A tap fires on the press, always. Holding past ~0.18 s starts a charge that fires on release as a second, charged shot (both verbs kept). CHARGING shows only once a real hold has started. | Core/Guns/Charge35.cs:114-185; UiRail34.Charge35.cs |
| F2 shots leave the gun | Nails, rockets and grenades are drawn from the muzzle and blend into the sim path over ~0.1 s. Tracers start at the post-kick muzzle, the same point as the flash. The slug draws one line, not 7 fake pellets. | ProjectileViews.cs, WeaponFx.cs:236-273 / :254-272, SurvivalGame.Render.cs:40/:103 |
| F3 crunch | 15-20 ms freeze on any body hit (within the 0.2 s/s budget); a stagger pose and visible knockback on enemies; a 0.1 s freeze per punch hit (ULTRAKILL). Gun parts move: slide, hammer, a 2-frame flash sprite, shell eject. | ImpactRules.cs, Viewmodel*, HitSpring.cs |
| F4 the fist | Replace the 10 cm block with a low-poly hand and forearm: knuckles, a lunge, a follow-through. | BlockyKit.Gauntlet, Viewmodel.cs:364-378, :842-853 |
| F5 the wall ride grips, not climbs | On attach, clamp upward speed to a small kick (~1.5 m/s). Keep the speed you arrived with along the wall. Grip near level, then a slow sink. Ease the camera roll in and out. Mark rideable walls with a cyan trim stripe. | Mover.cs:1023 and :467-475, inside the wallride35 branch only (named here, CLAUDE.md rule 3) |
| F6 text you can read | Teaching lines move to the lower middle (above the crosshair's lower third), 32 px at 1080p on a plate, apart from the room name. | UiHud33.cs:137-142, UnityTuning.Hud33.cs:86-95, UiTeach35.cs |

## 3. Levels: a new method (the biggest change)
GG: "you're really struggling to make detailed rooms that really incentivize mechanics." The fix is the method.

1. **Write every level from scratch, whole.** New files `n36-*.json`, made by generators that do not `require` any older generator. No n30-n35 room survives unless GG asks for it.
2. **A beat sheet before any geometry.** Each room gets:
   - one job: teach, practise, combine, test, rest or boss;
   - its verb;
   - the movement line drawn as a path;
   - where enemies enter, and their climb route;
   - what the player sees on entry;
   - the reward.
   The ease-in is fixed: a level opens with 10-20 s of no threat. A new enemy first appears alone, or as a pair in a tight space, before it is mixed with others (ULTRAKILL 0-1 / 3-1; design-web.md rule 3).
3. **Every prop has a job, or it doesn't exist.** Each solid carries a `job` tag: line, cover, climb, hazard, marker or reward. A checker fails any untagged prop and any lava that can't hurt an enemy (lava kills enemies too; launching enemies into it is the point).
4. **Metrics kit.** Rooms are built from verb modules sized from the movement numbers: ride wall pairs, slide gaps, dash gaps, grapple nodes, parry alleys, climb routes (n35-ride-distances.json is the start). No more stamping one RIDE HALL into every level.
5. **Smaller and fewer.** 5-7 rooms per level (about 40 in total, against 57 today), each 12-24 m (GG w28: "smaller and more action packed").
6. **Look before it ships.** Each room gets a top-down render, three Player frames from the player's eye (entry, middle, exit) and a critic agent's check against its beat sheet before it counts. Then the bot walk (L-041).

Levels (outline; each = one new verb + one weapon + a boss that is the exam; the weapon order stays as Curriculum25 has it):
| level | verb taught | the boss's exam |
|---|---|---|
| L1 FOUNDRY | move, jump, slide, wall ride; the parry (the glob) | THE LANTERN: ride the wall to reach it; parry its orbs back to crack it |
| L2 ICE LINE | the gauntlet, launch, skating | THE STAG: parry its charge at the antlers; it staggers; punch the open back |
| L3 KILN | THUNDERCLAP blast-jump, slide | THE KILN WYRM: blast-jump to its head; launch its spit back |
| L4 SPAN | grapple, STITCHER | THE LOOM: grapple to the marked joints; nail them; detonate |
| L5 SPIRE | the jump dash, height | THE CROWN: chase it up its own sectors |
| L6 FINALE | everything | THE RIVAL (magenta): a V2-style mirror that rides, grapples and parries like you (D-4) |

## 4. Bosses: one framework, then six fights
- **Walk-in, never a cut.** The boss arena is the level's last room, reached through a door and a short hall. The player sees the boss arrive (Gabriel's beam; V2 drops in). No `Respawn` into a far arena: this replaces Campaign24.cs:571-575 for the n36 levels.
- **One tell grammar.** Each attack has its own pose and a colour flash, with a wind-up of about 1-1.5 s (Birkhead: "beyond 2 seconds I start counting"; design-web.md rule 5). YELLOW = parry, RED = dodge. Each big attack is followed by a punish window long enough to reach the boss and land one full combo.
- **Three attacks, two phases.** At 50 % the boss is stunned and gets a free-hit window, then changes one thing. No added health: band 15-22 s for the bot, ~40 s for a person (GG w31-w32).
- **The weak point is reached through the level's verb.** It is marked on the model (a lit joint or core), never explained in text alone.
- **Models that read.**
  - Articulated low-poly bodies: a skeleton of parts with procedural animation (spring bones, IK legs), the dusk palette, an emissive core and rim.
  - A silhouette readable with no light, and a distinct pose for each attack.
  - Needs D-3: CLAUDE.md rule 7 says enemy models are "the web's blocky models rebuilt as they are", and GG's message asks for the opposite for bosses.
- **AI with intent.** Each boss keeps its distance band, moves around the arena's features, and punishes camping (an anti-camp attack aimed at the safest spot).

## ULTRAKILL reference numbers (research/rea-ultrakill-combat.md; decompiled file:line there; starting points, not copies)
| rule | ULTRAKILL | use here |
|---|---|---|
| revolver tap | instant on press; the charged piercer is on its own button (Fire2), fills in 0.57 s, fires on release | F1: the tap is always instant; the charge never delays it |
| shot visuals | the hit ray comes from the camera; the beam is drawn from `gunBarrel` to the hit point, flash at the barrel, same call as the anim trigger | F2 |
| nails | spawn 1 m in front of the camera, flash alternating barrels | F2 (we draw from the muzzle and blend) |
| parry buffer | a punch up to ~0.12 s early still parries | the parry core |
| telegraphs | Filth tracks you during the wind-up, then locks and opens the parry window; Malicious Face charges 2 s, locks aim at you + 0.5 s lead, fires 0.5 s later; Mindflayer previews its beam with a line | the tell grammar |
| anti-camp | Mindflayer teleports in after 10-15 s without melee; V2 enrages after ~6 s of you staying over 15 m away or out of sight | every boss's anti-camp attack |
| V2 | picks a weapon by range (shotgun ≤15 m, nails <25 m, revolver beyond); changes movement pattern every 2-5 s, with a colour cue; runs from you under 10 m | THE RIVAL (D-4) |
| arena flow | doors lock red on entry; enemies switch on 1 s later, 0.1 s apart, each with a spawn bubble and light; next wave 1 s after a wave dies; last kill 0.15 slow-mo; doors unlock 0.1 s apart | the room flow for n36 levels |
| walls | no upward motion from a wall; cling only while falling (>1 m/s) and pushing in; slide speed grows ~8 m/s each second | F5: grip, never climb |
| heal | blood heals on touch for 0.25 s after it spawns; 35 % of each hit becomes temporary max-HP loss | D-5 heal by aggression |

## 5. How we build (process)
- **Depth before breadth: three nights, not one (D-2).**
  - N36: the feel fixes F1-F6, the parry core, the speed launch, heal by aggression, the boss framework, then L1 + THE LANTERN rebuilt end to end. GG plays this slice before anything else is rebuilt.
  - N37: L2 + L3 with THE STAG and THE KILN WYRM.
  - N38: L4-L6 with THE LOOM, THE CROWN and THE RIVAL.
- **The close gate is eyes, not lines.** Each card closes on Player frames plus a short capture judged against its beat sheet or feel target (L-001), then the harness lines. The harness gates stay (golden, fingerprint, --off36, the verbs check).
- **Fewer builders, owning whole things.** One builder owns a level and its boss, end to end, including looking at it.

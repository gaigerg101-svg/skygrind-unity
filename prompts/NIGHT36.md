# NIGHT36.md: the overhaul, slice 1: movement is the weapon (lead + five builders, about 3 hours)

GG, 2026-10-12 ~16:00, after playing n35-r4 (verbatim and a row map: Research/2026-10-12-wave36/GG-REQUEST.md): "the guns and stuff don't
feel very satisfying"; "the pistol is not satisfying anymore"; "the crosshair fires normally but then the gun doesn't fire out the gun";
"the boss fights are awful ... you just teleport into these random rooms"; bosses are "ugly", "hard to understand what to do", and must
"encourage the mechanics of each level"; "more of a parry ... an understanding of parry mechanics"; "you're really struggling to make
detailed rooms that really incentivize mechanics"; "the tutorial text is way too high up"; level 2 starts with "an enemy running at you
without any explanation ... ease the player into it"; "random lava pits random pillars that serve no purpose"; the wall ride: "you kind
of hook onto a wall and you kind of just start sliding up it"; "I want a massive overhaul ... including how the core gameplay loop works
... really incentivizing much more movement".

GG decided (DECIDE36.md, ~16:30): D-1 **movement is the weapon** (the parry at the centre, speed launches enemies into explosions,
close kills heal; every gun kept); D-2 **depth first over three nights** (tonight: the feel fixes, the core loop, the boss framework,
and LEVEL 1 + THE LANTERN rebuilt end to end; GG plays that slice before anything else is rebuilt; N37 = L2-L3, N38 = L4-L6);
D-3 **new jointed boss models** (CLAUDE.md rule 7's "blocky models rebuilt as they are" is lifted for bosses only; ordinary enemies
keep the web's models). D-4..D-6 on their defaults.

Base: port/stage3 at u3-n35 (29c80815). Read first: CLAUDE.md, docs/PRINCIPLES.md, docs/VERBS.md, HANDOFF.md, NIGHT35-REPORT.md, and
Research/2026-10-12-wave36/: REVIEW.md (what is wrong, with file:line; READ IT ALL), PLAN.md (the plan and the ULTRAKILL numbers
table), DECIDE36.md, research/rea-ultrakill-combat.md (ULTRAKILL's revolver, parry, tells, V2, arena flow, walls, healing; decompiled
numbers only), research/design-web.md (cited design rules). t0 = the moment GG says go. Box 3:00, hard cap 3:20.

<goal_for_the_night>
GG starts level 1 and it feels like a different game. The KESTREL fires the instant he clicks and its shot leaves the barrel; hits
freeze and enemies stagger; his fist is a fist. A spitter's glob flashes yellow, he punches it, time catches, and it flies back and
explodes. He dash-punches a husk into another and both burst. Killing up close heals him. Walls grip and carry him; they never lift
him. The teaching text sits where he looks. Level 1 is new from the first room: each room does one job he can see, nothing in it is
there for no reason, and the first threat comes after a calm start. At the end he walks through a door into THE LANTERN's hall and
watches it arrive: a jointed thing with a lit core that he can only reach by riding the walls, and whose orbs he can parry back.
Nothing he had is gone: the verbs check says so in a Player.
</goal_for_the_night>

<method>
1. **Depth, not breadth.** Only level 1 and its boss are rebuilt tonight. No builder touches levels 2-6 except to keep them booting
   (they still play with tonight's feel and core rows on).
2. **Eyes close a card, not lines.** A card is DONE when the lead has looked at a RED / GREEN Player frame pair (or a capture) that
   shows the target in one frame (L-001), the card's harness check is GREEN, and one iteration on what the frames showed has landed.
   A harness GREEN alone is never DONE.
3. **Reference first.** Numbers start from PLAN.md's ULTRAKILL table and design-web.md, each with a `_why` naming its source; every
   tuning change is logged as a delta.
4. **Nothing lost.** docs/VERBS.md gains the new verbs (parry payoff, launch, heal by aggression); verbs36 (verbs35's reel, extended)
   performs every row in a Player at the freeze. Rows a decision changes on purpose (the KESTREL's tap, the wall ride's rise) are
   marked "changed by D-x / F-x" with their new check.
5. **Commit every slice; builders patch only their own config rows (L-049).**
</method>

<cards>
**F1 the guns feel like guns (builder F, ~120 min).** REVIEW.md root causes 1-3.
- `tap36`: a KESTREL tap fires ON THE PRESS, always. Holding past `design.n36.tap36.holdStartS` (start 0.18 s) starts the charge;
  release fires the charged shot as a second shot (both verbs kept; ULTRAKILL's tap is always instant, its charge never delays it).
  CHARGING shows only once a real hold has started (UiRail34.Charge35.cs:45-91; also stop the gauntlet BLAST slot's "CHARGING" when it
  is only refilling pips: HudWeapons30.cs:72-74, say "REFILL"). Same rule for THE THUNDERCLAP's charge. Core/Guns/Charge35.cs:114-185.
- `muzzle36` (view only): nails, rockets and grenades are DRAWN from the muzzle and blend onto the sim path over ~0.1 s
  (ProjectileViews.cs:66-112); hitscan tracers start at the post-kick muzzle, the same point as the flash (WeaponFx.cs:236-273 vs
  SurvivalGame.Render.cs:40 / :103); the slug draws one line, not 7 fake pellets (WeaponFx.cs:254-272).
- `crunch36`: a 15-20 ms freeze on every body hit (inside the 0.2 s/s budget; ImpactRules.cs:19-30), 0.1 s per punch hit (ULTRAKILL);
  a stagger pose and a visible knockback on enemies (HitSpring); gun parts move: the KESTREL's hammer and slide, a 2-frame flash
  sprite, a shell eject; THE THUNDERCLAP's pump.
- `fist36` (view): the 10 cm gauntlet block (BlockyKit.cs:568-581) becomes a low-poly hand and forearm in the gauntlet's colours:
  knuckles, a lunge forward on the punch, a follow-through, a parry pose.
- Check: harness (a tap's shot lands on the press step; a hold still charges and fires; the freeze per tier); Player frames: the tap's
  first frame (flash + kick + tracer from the barrel), a STITCHER volley from the barrel, a hit stagger, the fist mid-punch.

**P1 the core loop: parry, launch, heal (builder P, ~140 min).** PLAN.md section 1.
- `tells36`: every enemy attack that can hit the player gets a tell: a pose plus a colour flash, a wind-up of 1-1.5 s for melee and
  big shots (Birkhead; ULTRAKILL's Filth tracks during the wind-up, then locks). YELLOW = parryable, RED = dodge only. Projectiles
  that are parryable glow yellow. One tell per attack, never two overlapping.
- `parry36`: a punch that meets a yellow attack or projectile (with ULTRAKILL's ~0.12 s early buffer) is a PARRY: a 0.25 s freeze, a
  flash, projectiles return to their shooter and explode, a melee attacker staggers; the parry heals and refills one dash. Builds on
  the existing gauntlet parry (ZL PARRY, the glob punch-back); event `parry36` for GG's sound.
- `launch36`: a punch thrown while dashing, sliding, wall riding or pulled by the grapple LAUNCHES the enemy along the punch; a launched
  enemy that hits another enemy or a wall explodes (damage and radius in config; the explosion hurts enemies, never the player). Event
  `launch36Burst`.
- `heal36`: a kill within ~4 m spills health (ULTRAKILL: blood heals on touch for a short window; start 10 HP, radius 4 m); health
  pickups stay. Event `heal36`.
- Check: harness per rule (a glob parried returns and explodes; a dash-punch launches; a launched husk into a husk = two kills; a
  close kill heals, a far kill doesn't); Player frames of each (the yellow tell, the parry's freeze frame, the burst, the heal number).

**M1 walls grip, text you can read (builder M, ~90 min, then the bots).** REVIEW.md root causes 4 and 10.
- `wallgrip36` (Mover.cs, inside the wallride35 branch only: named here, CLAUDE.md rule 3): on attach, clamp upward speed to a small
  kick (`design.n36.wallgrip36.attachUpMax`, start 1.5 m/s) instead of keeping a jump's rise (Mover.cs:1023); keep the arrival speed
  along the wall; grip near level for a short time, then a slow sink (the tap ride's 0.15 gravity applied to a rising entry is the
  "slide up": Flow34.cs:109-116, Mover.cs:467-475); ease the camera roll in and out. ULTRAKILL: a wall never lifts you.
- Rideable walls are marked: a cyan trim stripe along every wall-ride panel (view; the colour rule: cyan = player-usable).
- `text36`: teaching lines and lessons move out of the top slot to the lower middle (about 62 % down), 32 px at 1080p on a plate,
  apart from the room name (UiHud33.cs:137-142, 219, 735-744; UnityTuning.Hud33.cs:86-95; UiTeach35.cs). The hud33 rule checker 0.
- Then: the walk bot and its pilots on the new level 1 (10/10, L-041), and the harness for the ride (no rise above 1.5 m on a jump
  entry; the old ride's length and chain speed numbers before / after).
- Check: frames: a jump into a ride wall at attach + 0.5 s + 1 s (height stays), the trim stripe, the text at its new place.

**L1 LEVEL 1, from scratch (builder L, ~150 min).** REVIEW.md root causes 7-8; PLAN.md section 3.
- A NEW file `Design/levels/n36/foundry.json` from a NEW generator `Tools/night/gen-n36-foundry.cjs` that `require`s no older
  generator (shared low-level helpers in a new `n36-lib.cjs` are fine). No n30-n35 room is copied.
- **Beat sheet first** (`Design/levels/n36/foundry.beats.md`, written and committed BEFORE geometry): 5-7 rooms, each 12-24 m; for each:
  its job (teach / practise / combine / test / rest / boss), its verb, the movement line as a path, where enemies enter and their climb
  route, what the player sees on entry, the reward. Order: a calm start (10-20 s, no threat: move, look, jump, slide), the KESTREL and
  its charge, the wall ride (practise over a floor, then over lava), the PARRY (one spitter alone in a tight space, its glob yellow; the
  text names the button), a combine room (ride + parry + launch), a rest, then a door and a short hall into THE LANTERN's hall.
- **Every solid has a `job`** (line / cover / climb / hazard / marker / reward) and a `why`; a checker (`Tools/night/n36-jobs.cjs`)
  fails any untagged solid, any lava an enemy can never be launched into, and any room without a climb route for enemies.
- New enemies appear alone or in a pair first; the arena flow follows ULTRAKILL's (PLAN.md table): doors lock red on entry, enemies
  switch on 1 s later, 0.1 s apart, each with a spawn bubble and light; the last kill slows time 0.15 for a moment; doors unlock.
- **Look before it counts:** a top-down render per room (n34-lead-render.cjs's way) and the lead's three Player frames per room (entry,
  middle, exit), checked against the beat sheet by L; the walk bot 10/10 (M).
- The campaign's level 1 swaps to n36-foundry behind `design.n36.level36` (Curriculum25 hook pre-placed by the lead); levels 2-6 unchanged.

**B1 the boss framework + THE LANTERN (builder B, ~150 min).** REVIEW.md root causes 5-6; PLAN.md section 4.
- `walkin36`: the boss fight happens in the level's last room, entered through a door and a short hall; NO `Respawn` into a far arena
  (the lead pre-places a gate at Campaign24.cs:571-575; THE LOOM already fights in its room: SurvivalSim.Boss30.cs:62-90). The reveal:
  doors lock, the boss arrives in view (a drop or a rise on a beam), its name card, then the fight. No cut.
- A shared `Boss36` shape (Core): three attacks, each with its own pose and a 1-1.5 s colour tell (YELLOW parry / RED dodge, P's
  `tells36` grammar), a punish window after each big attack long enough to reach it and land one full combo, a 50 % stun with a
  free-hit window, then one change; an anti-camp attack aimed at the safest spot after ~6 s of camping (V2 / Mindflayer: PLAN.md
  table). No added health: band 15-22 s for the bot (GG w31-w32).
- THE LANTERN rebuilt on it: its hall is built for the wall ride (L builds the hall to B's spec, by 1:00); its core is lit and reachable
  best from the walls (keep: full damage only while riding; the charged KESTREL from the wall breaks its shield); its orbs are YELLOW:
  parried back, they crack the shield. Three attacks: the orb fan (parry), the floor cycle to lava (ride), the panel sweep (dodge).
- `bossmodel36` (D-3): a jointed low-poly body (a hierarchy of parts, procedural animation: spring bones, a breathing idle, a pose per
  attack), the dusk palette, an emissive core and rim; readable as a silhouette with no light. Built in code from parts (no imported
  rig needed tonight); `Boss36View` with the LANTERN's model as the first.
- Check: harness (the walk-in starts the fight with no Respawn; the tells' timings; the band); Player frames: the reveal, each tell,
  a parried orb, the 50 % stun; THE LANTERN's time with every n36 row on.

**Lead L0 (~30 min inside rounds).** docs/VERBS.md's new rows before round 1; the shared commit; the frames of every card, looked at;
the REA / ILSpy reads a builder asks for (the decompile is in the session scratch folder; numbers only, cite file:line).
</cards>

<builders>
Five builders in worktrees (`isolation: "worktree"`), launched by 0:15 with this file and the wave-36 research set. Each: `git merge
port/stage3` first and before every report; dn-check-wt.sh on its OWN scratch dir; fingerprint and `--off36` EQUAL before each sim
commit; CoreHarness 0 mismatches before each Core commit; never the Editor, a build or a Player; reports under 300 words; resumed by
SendMessage.

| Builder | Owns |
| --- | --- |
| **F** gun feel | Core/Guns/Charge35.cs (tap36), UiRail34.Charge35.cs, HudWeapons30's slot label, WeaponFx.cs, ProjectileViews.cs, Viewmodel*.cs, BlockyKit.Gauntlet, ImpactRules.cs (freeze tiers), HitSpring.cs, its harness file |
| **P** core loop | Core/Combat.cs (the melee / parry path; nobody else edits Combat.cs tonight), new Core/Parry36.cs, Launch36.cs, Heal36.cs, the enemy attack tells in Core/Enemies (not Lantern*), their views' tell flash, its harness file |
| **M** walls + text + bots | Core/Mover.cs (wallride35 branch only), Core/Flow34.cs, the ride-wall trim view, UiHud33's message-slot placement + UiTeach35 + UnityTuning.Hud33, Game/Bot/* pilots, Harness.N36.Move.cs |
| **L** level 1 | Tools/night/gen-n36-foundry.cjs, n36-lib.cjs, n36-jobs.cjs, Design/levels/n36/*, the level36 config row, the lines harness for L1 |
| **B** bosses | new Core/Boss36.cs, Core/Enemies/Lantern* (the rebuild), new Game/Boss36View.cs + the LANTERN model, the walkin36 flow in its own new SurvivalSim.Boss36.cs partial, its harness file |

The lead alone: SurvivalSim*.cs other than builders' new partials, SurvivalGame*.cs, Campaign24.cs (the walkin36 gate), Curriculum25,
Program.cs, the tape, FingerprintBaseline, every build and reel, git. Overlaps decided now: P's tell grammar is shared by B (P lands
`tells36`'s shape by 0:50; B uses it); L builds THE LANTERN's hall to B's spec (B sends it by 0:40); M's text placement is the only
UiHud33 edit tonight.
</builders>

<lean_rules>
- Proof: method 2. Never edit a test to pass; a test that pins a state tonight's decisions change is updated with the reason logged.
- Golden at every Core commit; Mover.cs only inside the wallride35 branch (wallgrip36); JsMath, Mulberry32; whole-step timers.
- Config: gameplay numbers in design.n36.<row> with a `_why` naming the source; render / UI numbers in UnityTuning.*36.cs.
- Every n36 row behind N36Blocks; `--off36` / `-skygrindOff36` turns all off; tape format 19.
- Licences: copy only MIT / Apache / BSD / Unlicense / CC0, licence in the header, a port line in PORT-STATUS.md. ULTRAKILL's
  decompiled code is reference only (numbers and rule shapes, cited); its models and levels are never copied.
- Caps (report it short instead): builds 6; reel invocations 22; suite 1 (batch, at the freeze); builders 5.
- Memory (L-005, the n35 close): heavy jobs one at a time; check free memory before a build or a reel; identify a Player by PID and
  start time before closing it (L-054).
- Commits with explicit paths; never GG's four files; nothing deleted without GG.
</lean_rules>

<tasks>
## U3-49.1 Start (0:00-0:15)
`date`; the Editor gate; `git status`; the stray Player PID 115072 (started 13:33 by an older session) identified and closed only if it is
ours; docs/VERBS.md new rows; the shared commit (N36Blocks: tap36, crunch36, tells36, parry36, launch36, heal36, wallgrip36, level36,
walkin36, boss36; view rows muzzle36, fist36, text36, bossmodel36; tape 19; design.n36 rows off; `--off36`; the walkin36 gate in
Campaign24; the level36 hook in Curriculum25; reel groups PhaseN36F/P/M/L/B + verbs36; n36 rows forced off in old pinned tests); launch.
## U3-49.2 Build rounds (0:15-2:20): integrate at about 0:55, 1:35, 2:10
Each round: integrate, gates, ONE batch build, the round's reels, frames LOOKED AT, builders resumed with the exact lines.
Round 1: tap36 + muzzle36, tells36's shape, wallgrip36 + text36, L's beat sheet + first rooms, B's Boss36 shape + the hall spec.
Round 2: crunch36 + fist36, parry36 + launch36 + heal36, level 1 complete, THE LANTERN on Boss36 with walkin36, verbs36's first run.
Round 3: bossmodel36, iteration on every card's frames (at least one tuning pass each), the walk bot 10/10 on the new level 1.
## U3-49.3 Freeze (2:20) and verification (2:20-2:45)
fingerprint `--off36` EQUAL; golden; one batch suite; smoke; verbs36 all PASS (one Player per map); THE LANTERN in its band; level 1
played start to boss in a Player with every n36 row on (a capture of each room); NIGHT36-PROOF.md.
## U3-49.4 Close (2:45-3:00)
NIGHT36-LOG close table (F1, P1, M1, L1, B1, L0: DONE or SHORT with a measured reason), NIGHT36-REPORT (GG's page: the slice to play,
what to look for, frame pairs), FEEL-PLAYTEST (level 1 only: does it feel like a different game?), HANDOFF, PORT-STATUS events
(parry36, launch36Burst, heal36, boss36Reveal, the tells), DECIDE36, the memory harvest, tag u3-n36. Hard cap 3:20.
</tasks>

<leftovers>
1. Levels 2-3 + THE STAG + THE KILN WYRM on the n36 method and Boss36 (NIGHT37).
2. Levels 4-6 + THE LOOM, THE CROWN, THE RIVAL (NIGHT38; D-4).
3. Jointed models for ordinary enemies (rule 7 stays for them unless GG lifts it).
4. Hard damage (ULTRAKILL: 35 % of a hit as temporary max-HP loss) as heal36's partner, if heal36 makes the game too easy.
5. A pad-rumble pass for parries and launches.
</leftovers>

<closing_reminder>
- Only level 1 and THE LANTERN are rebuilt tonight; depth over breadth (D-2).
- A card is DONE only on a frame pair the lead looked at plus one iteration; a GREEN line alone is not done.
- Nothing GG had may be gone: verbs36 all PASS in the Player, or RED on the close table with why.
- Every lead message ends with `N36-STATUS: round <r> · builders <alive>/5 · verbs <pass>/<total> · <h:mm> / 3:20`.
</closing_reminder>

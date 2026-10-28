# NIGHT35.md: the fundamentals night (lead + five builders, about 3 hours)

GG, 2026-10-11 ~19:28, after playing n34 (verbatim and a row map: Research/2026-10-11-wave35/GG-REQUEST.md): "why we're removing abilities
... the pistol lost the ability to charge and fire same with the shotgun"; a picked-up weapon shows no model ("you need to see the
silhouette or the shotgun model"); "we're not following game principles ... that should be a core embedded into this project"; ULTRAKILL-style
text that teaches each mechanic in-level (wall run, a new weapon, fire, zoom); the grapple "shouldn't go limp ... it needs to take you to
wherever you fire" and unlock "like the 4th or 5th level"; no effect when wall riding + zooming ("a bluer overlay or some ... gamey glitchy
effect"); the healing item doesn't read as healing; the jump dash in later levels; "six levels total and we just have to polish them a
ton"; the lights look "crappy ... cheesy" with "weird little triangle displays"; the PS2 overlay look "and it really shows"; the UI "moving
around a little bit ... bouncier ... glitching out like a TV" ("I've explained this before"); wall riding "a lot more like Lucio from
Overwatch", with arenas, bosses and stages built on wall-running semi-platforming. "I feel like we're taking two steps forward and a step
backward." Use the skills, the reverse engineer included.

Base: port/stage3 at u3-n34 + the addendum (6733d8be). Read first: CLAUDE.md, docs/PRINCIPLES.md (new tonight: the rules every card is
checked against), HANDOFF.md, NIGHT34-REPORT.md, Research/2026-10-11-wave35/ (GG-REQUEST.md, research/audit.md: why each thing GG named
broke, with file:line; lucio.md; look.md; teach.md; RESEARCH.md: the numbers), and Research/2026-10-11-wave34/research/rea-ultrakill.md
(ULTRAKILL's own numbers, read with REA + ILSpy). t0 = the moment GG says go. Box 3:00, hard cap 3:20.

<goal_for_the_night>
GG plays level 1 and is taught, in the game, to fire, zoom and charge the KESTREL, and both the KESTREL and THE THUNDERCLAP charge again.
Every pickup shows what it is. The world looks like a PS2 game and the HUD wobbles like an old TV. A wall ride feels like Lucio's and
shows its own blue focus look when he zooms on it. The grapple yanks him straight to where he fired, from level 4. Six levels, each
teaching one thing. Nothing he had is gone: the VERBS check says so in a Player.
</goal_for_the_night>

<method>
1. **Nothing lost (PRINCIPLES 1, the night's first rule).** Before any new row lands, the lead writes docs/VERBS.md: every verb and
   weapon ability, its input, the row that owns it, the level it is available from, and how a reel proves it. Verbs tonight's DECIDE35
   defaults change on purpose are marked "changed by D-x" with the new behaviour (the KESTREL's hold, D-8; the hold-swing, D-9; the
   1.2 s ride, D-6; the whip / whip-readies-punch / the bend before level 4, D-2): their reel line checks the new behaviour. The
   `verbs35` reel performs every verb in a Player, one Player per map (L-046, L-051), and prints PASS / RED per verb. It runs in round 2
   and at the freeze; a RED at the freeze gets 15 minutes of fixing, then goes on the close table as RED with its reason.
2. **Reference first, then iterate (kept from w34; 0 tuning commits there).** Numbers start from the cited sources (RESEARCH.md table), are
   iterated at least once on Player frames, and every change is logged as a delta.
3. **A spec, a check, a frame.** Every row: its automated check and a RED (`-skygrindOff35`) / GREEN Player frame pair the lead looked at.
4. **Commit every slice; builders patch only their own config rows (L-049).**
</method>

<cards>
**W1 the weapons come back, and read at a glance (builder W, ~100 min).**
- P0: the KESTREL's and THE THUNDERCLAP's charge "lost" (audit.md item 1). The code path is intact (`Combat.cs:903`,
  `ChargeShots27.cs:155-205`; design.n31.charge and n33.pierce33 on), but in play it reads as gone: holding fire first auto-fires for
  0.4 s, then charges, then fires itself at 1.4 s; the only cue is two arcs on the reticle; the gun never reacts; the rail has no
  CHARGING state; then 4 s (KESTREL) / 6 s (THUNDERCLAP) of plain fire. Make it unmistakable and dependable:
  - the charge starts on the hold without the 0.4 s of auto-fire first (`charge35`: a press-and-hold charges, a tap fires; ULTRAKILL's
    revolver charges on its own input with no shots first), the release fires (a full charge held longer than `design.n35.charge35.holdMaxS`
    fires itself), a short fixed cooldown shown on the rail (W adds `UiRail34.Charge35.cs` as a partial; V does not touch UiRail34);
  - the gun visibly charges (a glow building on the barrel, a small shake, a cyan tip), the rail slot shows CHARGING with its fill, the
    reticle arcs stay; events `charge35Start` / `charge35Full` / `charge35Fire` for GG's sounds;
  - harness per gun (a hold reaches full, the charged shot's damage, a tap still fires a normal shot) and a Player reel on the campaign's
    level 1 (KESTREL) and level 3 (THUNDERCLAP), read from the campaign order (not hard-coded n34 ids: A may swap the files), in verbs35.
    DECIDE35 D-8: the trigger shape.
- Pickups (PRINCIPLES 3; audit.md items 2 and 4: today a weapon pickup is a plain spinning 0.6 m cyan cube in a glow pillar,
  `PropsView29.cs:145-178`, and the healing item the same cube in green): a weapon pickup shows that weapon's own mesh
  (`BlockyKit.ForWeapon` exists; SURVIVAL's wall buys already show it as a hologram, `WallBuys.cs:76, 151`), turning and bobbing, cyan
  rim, with its name on approach ("THUNDERCLAP"); a locked unlock shows the gun's silhouette (CoD's chalk outline). The health pickup gets
  a model that reads as health without a red cross (a protected emblem: research/teach.md section 2): a vial / heart capsule with a pulse
  and its name "MEND +N" on approach.
- Check: harness per gun; Player frames of the shotgun pickup, a wall buy, the health pickup, RED / GREEN.

**T1 the explainer: ULTRAKILL-style teaching text (builder T, ~100 min).**
- A `teach35` text kind on the EXISTING lesson system (Core/Lesson25.cs LessonDef already has rect, prompt and promptPad, and Campaign24's
  lessons already reach hud33's message slot, `UiHud33.cs:137`): rect, text with an `{action}` token, time 5 s, once per profile
  (Core/Profile.cs), the bound button resolved live through PromptText / PromptGlyphs / Glyphs.cs (pad glyph or key), the
  ULTRAKILL format: the verb in capitals, the button, at most one rule ("HOLD {fire} to CHARGE the KESTREL. Release to fire.").
  research/rea-ultrakill.md (HudMessage: trigger, bound key, 5 s, once) and teach.md section 1 are the spec.
- Content (one new idea per level after level 1, PRINCIPLES 4; level 1 teaches the base verbs room by room, as ULTRAKILL's 0-1): L1
  fire, zoom (LT), charge, slide, dash; L2 the Lucio wall ride + wall jump; L3 the THUNDERCLAP and its charge; L4 the grapple + the bend;
  L5 the jump dash + the whip; L6 none (the exam). Every new weapon's first pickup gets its own line (fire / its LT).
- Also: the `verbs35` reel (method 1), owned by T: one scripted performance per verb in VERBS.md, a PASS / RED line each.
- Check: the reel shows each level-1 text when its trigger is entered (frames); verbs35 all PASS on the round-3 build.

**M1 movement: Lucio wall ride + a grapple that yanks (builder M, ~110 min).**
- `wallride35` (Mover.cs branches only, golden exact off; research/lucio.md's spec): ride while jump is held with no 1.2 s cap (a long
  safety cap, logged); release jump = a jump-off boost whose height follows the look pitch; crouch drops off with no boost; a speed buff
  while riding and an exit boost so chains build speed; follow inside / outside corners and curved walls (pillars); a re-ride rule (not
  the same wall twice until landing or after a distance); a short late-jump window after leaving the wall. Entry speed gate lowered
  (logged). Numbers from lucio.md (each cited) in design.n35.wallride35.
- `grapple35` (audit.md item 3: holding the button past 0.2 s switches to NIGHT30's pendulum swing: the "limp"; `grapple32.tapNodesOnly`
  true refuses a tap on a bare wall, `GrappleTap31.cs:135-148`, so firing at a wall never takes you there; the arrival clamps to 12 m/s):
  the grapple pulls you straight to whatever you hit (a node, a wall, an enemy via the whip) at a constant speed (ULTRAKILL's whiplash: a
  constant pull, no swing: rea-ultrakill.md) and stops there; the hold no longer swings (the swing stays as a sub-switch, off); the rope
  view is taut. **The unlock:** NIGHT34's nodes in levels 1-3 made the grapple owned from level 1 (`SurvivalSim.Systems30.cs:200-204`
  owns it on any level with nodes): it is owned from level 4 only (the unlock rows), and with it the bend (bend34 fires on a latch);
  the nodes in levels 1-3 become dim locked rings (a tease) or are swapped for wall-ride lines (A decides per room with M). The auto-own
  line is `SurvivalSim.Systems30.cs:201` (a lead file): the shared commit pre-places a `grapple35` gate there.
- The bots (the red team's finding 1): Walk29Pilot's `SwingAlso34` hands rooms f5, f5b, f8, f9b, f10, i1a, i1b to Span30Pilot, which
  holds the grapple through the pendulum swing. M owns Walk29Pilot / Span30Pilot tonight: the span pilot uses grapple35's pull, the
  L1-3 rooms leave SwingAlso34 (the grapple is not owned there), and the walk bot is 10/10 again on every level. THE LOOM's reel and
  pilot (`FeelReel.N30.Bosses.cs:392-467`, LoomPilot32) and bend34's swing-latch rule (`SurvivalSim.Bend34.cs` header) are re-checked
  with grapple35.
- Order: wallride35 lands FIRST (by 0:45) so A can space rooms on its distances; grapple35 second.
- Check: harness per technique with numbers before / after (ride length on a 30 m wall, chain speed over three walls, a corner followed,
  the pull's path straightness and arrival); the verbs35 lines; THE CLIMB / walk bot still 10/10.

**V1 the look: PS2 that shows, lights that look like lights, a TV HUD (builder V, ~120 min).**
- PS2 pass: EXTEND our Crt29Pass / Look30Math (no package port tonight: shader errors cost a build; VNTG's maths, The Unlicense, may be
  copied with the licence in the header): internal resolution about 480 lines with point upscale (`pixelStep` > 0), colour compression
  32 levels (ULTRAKILL's default: rea-ultrakill.md), ordered dither, scanlines and chroma split that read at 1080p. Every number a
  UnityTuning setting. Today's pass is nearly invisible: design.n29.crt defaultStrength 0.35, pixelStep 0 (no pixelation), scanlines 0.12.
  Vertex snapping (world shaders, Assets/Shaders) and a settings page are leftovers.
- Lights (audit.md item 5: no triangle mesh exists at any light; the likely cause is Vfx33's untextured dust / spark particles,
  `Vfx33.cs:91-135`, or bloom on the bright lamp cores: the LEAD takes one frame pair on n34-r5 with Vfx33 off vs on at a lit room in the
  start block and hands V the cause, then V fixes it); fewer, stronger lights each inside an emissive fixture; bloom on
  fixtures only; one warm key + a cool fill per room (look.md section 2). Volumetric shafts (CristianQiu, MIT, Render Graph) only if the
  frames say it helps.
- The TV HUD: the HUD jiggles (a damped spring on hits / swaps), bounces on new text, and glitches briefly (a cyan / blue channel split +
  a scanline sweep on a bend or a level start; red only on damage: rule 8) via UIEffect and LitMotion (both already in the manifest), all in
  a NEW component on the canvas root (`UiTv35.cs`: no edits inside UiHud33's build code); readable at all times (text >= 16 px, the hud33
  rule checker 0).
- The wall-ride focus look: wall riding + zooming (slowAim27) gets a distinct cold-blue grade, edge chroma and a faint scanline glitch,
  strong enough to read in one frame (today's cue: vignette 0.25, chroma 0.15: invisible). Not the bend's grey: blue = focus, grey = bend.
- Check: Player frames RED / GREEN of a level-1 room, a lit room, the HUD on a hit (two frames), the wall-ride zoom.

**A1 six levels, polished, built on wall riding (builder A, ~110 min). GG: "six levels total and we just have to polish them a ton".**
- The campaign order is six levels (design.n35.campaign6; the lead pre-places the Curriculum25 / UiLevelSelect26 / LevelSel35Playable hooks
  and Lesson25Tests' order in the shared commit): L1 foundry, L2 iceline, L3 kiln, L4 span (the grapple), L5 spire (the jump dash), L6
  a SHORT finale (three wall-ride platforming rooms from the gen-n34 generators and one boss room; boss arenas are built in code,
  Core/Enemies/Crown32Arena.cs / Rink28Arena.cs, so L6 reuses a boss whose arena code exists and is not L5's; a boss rush is a leftover).
  The legacy levels after them leave the campaign order (they stay in the tree). A owns the unlock rows (M does not edit them).
- Polish first: levels 1-3 each get at least one Lucio semi-platforming room (wall chains facing alternate directions, gaps that need a
  ride, pillars to circle) on top of NIGHT34's lines; levels 4-5 get one if time (leftover). Spacings use the 1.2 s ride until M's
  wallride35 distances arrive (about 0:45), then are retuned.
- Each level's teach35 triggers placed where T's text needs them (A writes the rects, T the text).
- Check: lines34-style harness + the walk bot 10/10 on every changed level; LevelSelect29; every changed level boots in a Player (L-043).

**Lead L0 (~30 min inside rounds).** docs/VERBS.md before round 1 lands; the shared commit (N35Blocks, design.n35, `--off35`); the REA
pass on anything a builder asks (ULTRAKILL is decompiled in the scratch folder: never copy code, cite file:line); the N-11 boss check
(THE STAG with the bend) re-measured once the grapple moves to level 4.
</cards>

<builders>
Five builders in worktrees (`isolation: "worktree"`), launched by 0:15 with this file and the research set. Each: `git merge port/stage3`
first and before every report; dn-check-wt.sh on its OWN scratch dir (shared dirs gave phantom errors in w34); fingerprint and `--off35`
EQUAL before each sim commit; CoreHarness 0 mismatches before each Core commit; never the Editor, a build or a Player; reports under 300
words; resumed by SendMessage.

| Builder | Owns |
| --- | --- |
| **W** weapons + pickups | Core/Guns/* (ChargeShots27, Pierce33), Core/Combat.cs (the fire path; nobody else edits it tonight), Viewmodel charge pose, UiRail34's CHARGING state (one hunk), PropsView29 pickup cases, WallBuys, their harness file |
| **T** teaching text + verbs reel | Core/Lesson25.cs (the teach35 kind), Campaign24's lesson path (by a patch to the lead), the message slot's text path in UiHud33 / MessageSlot33 (T alone in UiHud33 tonight), PromptText / PromptGlyphs use, FeelReel.N35.Verbs.cs |
| **M** movement | Core/Mover.cs (wallride35 / grapple35 branches only), Core/Flow34.cs, SurvivalSim.Grapple31/32/Whip33 (the pull), Arcline21View (the rope), Game/Bot/Walk29Pilot.cs + Span30Pilot.cs + LoomPilot32.cs, Harness.N35.Move.cs |
| **V** look | PostFx.cs, Look.cs, Crt29*, Look30Math, Fixtures33.cs, LightPlan33.cs, Vfx33.cs, SlowAim27.cs, new UiTv35.cs, UnityTuning.Look35.cs |
| **A** levels | Tools/night/gen-n35-*.cjs, Design/levels/n35-*.json, the campaign order and unlock config rows, the lines harness |

The lead alone: SurvivalSim*.cs other than builders' new files, SurvivalGame*.cs, Program.cs, the tape, FingerprintBaseline, Curriculum25,
every build and reel, git. Overlaps decided now: T and V both touch UiHud33: T only the message slot's text path, V only motion /
effects in UiTv35.cs (no edits in UiHud33); W's rail state is a partial file; M sends wallride35 distances to A by 0:45.
</builders>

<lean_rules>
- Proof: a visual row is DONE only with a RED / GREEN Player frame pair the lead looked at; a gameplay row with a Player number; verbs35
  all PASS at the freeze. Tests verify, they don't define: never edit a test to pass (a test that pins a state the plan changes is
  updated with the reason logged, as D34-5).
- Golden at every Core commit; Mover.cs only inside wallride35 / grapple35 branches; JsMath, Mulberry32; whole-step timers.
- Config: gameplay numbers in design.n35.<row> with a _why naming the source; render / UI numbers in UnityTuning.*35.cs.
- Licences: copy only MIT / Apache / BSD / Unlicense / CC0, with the licence in the file header and a port line in PORT-STATUS.md;
  ULTRAKILL's decompiled code is reference only (numbers and rule shapes, cited).
- Caps (report it short instead): builds 6; reel invocations 22; suite 1 (batch, at the freeze); builders 5. Reel budget: round 1: 2
  (the lead's Vfx33 pair is on n34-r5 in the start block: 1) ; rounds 2-3: up to 4 each (one GREEN per landed card, one combined RED);
  verbs35 in round 2 and at the freeze, one Player per map (L1, L3, L4: 3 each); the freeze's level boots ride on verbs35 and LevelSelect29.
- The shared commit forces every n35 row off in the old tests that pin today's behaviour (GrappleTap, the 1.2 s ride, the order),
  as NIGHT27 did, so the one suite at the freeze is not spent on known reds.
- Heavy jobs one at a time; commits with explicit paths; never GG's four files; nothing deleted without GG.
</lean_rules>

<tasks>
## U3-48.1 Start (0:00-0:15)
`date`; the Editor gate; `git status`; the Vfx33 on / off frame pair on n34-r5 at a lit room (the "triangles" cause, for V); docs/VERBS.md
(from FEATURES.md + PORT-STATUS events: every verb with its input, row, level and check); the shared
commit (N35Blocks: charge35, wallride35, grapple35, teach35 if its text changes anything in the sim, campaign6 flow; tape 18; design.n35 rows off; `--off35`; hooks;
reel groups PhaseN35W/T/M/V/A + verbs35; the grapple35 gate at Systems30.cs:201; the campaign6 hooks in Curriculum25 / UiLevelSelect26 /
UnityTuning LevelSel35Playable and Lesson25Tests' order; n35 rows forced off in the old pinned tests); launch.
## U3-48.2 Build rounds (0:15-2:20): integrate at about 0:55, 1:35, 2:10
Each round: integrate, gates, ONE batch build, the round's reels (one GREEN per landed card + one combined RED), frames LOOKED AT, builders
resumed with the exact lines. Round 1: W's charge fix, T's text system on level 1, V's PS2 pass, M's wallride35, A's campaign6 order.
Round 2: grapple35 + the bots, pickups, lights, the TV HUD, A's wall-ride rooms, verbs35's first run. Round 3: level 6, the wall-ride
focus look, iteration on every card's frames (method 2: at least one tuning pass per visual card from the round-2 frames).
## U3-48.3 Freeze (2:20) and verification (2:20-2:45)
fingerprint --off35 EQUAL; golden; one batch suite; smoke; LevelSelect29 (six levels); verbs35 all PASS; bosses (the band, with the bend on
from level 4); frames of every card; NIGHT35-PROOF.md.
## U3-48.4 Close (2:45-3:00)
NIGHT35-LOG close table, NIGHT35-REPORT (GG's page: what came back, what's new, frame pairs, the verbs table), FEEL-PLAYTEST (level 1
first: does the text teach?), HANDOFF, PORT-STATUS events + ports, DECIDE35, the memory harvest (add the "nothing lost" lesson), tag u3-n35.
Hard cap 3:20.
</tasks>

<leftovers>
1. Vertex snapping on the world shaders (grid 160, ULTRAKILL's middle preset; VNTG / RetroPSX helpers).
2. Lucio rooms in levels 4-5; a boss rush in level 6.
3. A settings page for the PS2 look (ULTRAKILL's four steps: downscale, vertex warp, texture warp, colour compression).
4. The held-bumper weapon wheel that bends time while held (ULTRAKILL: hold 0.25 s, time 0.25).
5. Volumetric light shafts in lit rooms (CristianQiu, MIT).
6. Rising pitch on chained wall jumps is a sound cue: write the event name for GG (`wallride35Chain` with the chain count).
</leftovers>

<closing_reminder>
- Nothing GG had may be gone at the close: verbs35 all PASS in the Player, or the item is RED on the close table with why.
- Every visual card has a RED / GREEN frame pair the lead looked at, and one iteration on what the frames showed.
- Every lead message ends with `N35-STATUS: round <r> · builders <alive>/5 · verbs <pass>/<total> · <h:mm> / 3:20`.
</closing_reminder>

# NIGHT47.md: GG's playtest night. Every one of GG's 45 asks built, measured and shown on frames: THE WICK as a three-phase climb fight, the five bosses cut to 2-3 minutes for a human, the chargers' stampede, THE KILN WYRM foreshadowed and burrowing, THE LOOM and THE CROWN rebuilt, level 5 made to make sense, a heavier arsenal, a new HUD and words that sound human (lead + eight builders; at least 7 h, hard cap 8 h 30 min)

GG, 2026-10-18 (after playing the current build), verbatim. **This text outranks everything below it. Read it twice.**
> A first boss fight in the first stage is something I genuinely like a lot, but the wall‑jumping and upward movement still feel far too
> unforgiving, because if you try to wall jump or cling to the wall and climb upward, you simply can't make it consistently; even with a
> double jump you barely reach the next ledge, the timing window is too tight, and each gap between wall‑ride segments needs to be much
> easier to clear, especially early on, because right now the jumps are too high, too strict, and not fluid, and every attempt lowers your
> momentum slightly so you end up slipping further down each time, making it impossible to perform the sequence reliably, so the entire
> vertical climb needs to be redesigned to start much more forgiving at the bottom, with lower jumps, wider timing windows, and smoother
> transitions, gradually becoming slightly harder near the top but always remaining fully possible and fluid. I also want the fight
> structured into three phases where the first third plays out in a slightly modified arena layout, then after that initial phase the
> vertical platforms appear and force you to climb upward as part of the encounter, and then once you reach the top the third phase begins,
> which should resemble the first phase but be noticeably harder, with more aggressive patterns and clearer telegraphs. The boss's health is
> also way too high right now, you can barely damage it even while firing consistently, and these boss fights should last around two to
> three minutes maximum, so its HP needs to be reduced or its vulnerability windows expanded. The text that appears during the fight also
> needs to be much easier to read because it's currently tiny, unclear, and hard to understand, and the boss itself needs a few more attacks
> to make the fight feel dynamic. When you reach the top, the third phase should introduce a charge attack where the boss briefly cracks
> open, exposing a weak point, and the game needs to clearly communicate that moment with visual indicators so players know to hit it with
> melee; landing that hit should deal a big chunk of damage and send the player flying back in a dramatic, satisfying way. After completing
> the platforming section and reaching the top of the arena, there should be a translucent platform that fills the gap you climbed through
> so you don't fall back down, allowing the fight to continue smoothly. And beyond everything I'm describing, the entire encounter needs to
> be iterated on and improved even further, don't just follow my notes, push past them and refine every part of the design. There are also
> weird issues when running through the door, where it randomly turns pink or bugs out, and the lava lighting is inconsistent and messy, so
> those visual and lighting problems need to be cleaned up. The shield on the big shield enemy is positioned awkwardly and looks ugly, and
> the visors on enemies should be redesigned to look sharper and more angular, similar to Omega from Fortnite or other characters with cool
> pointy visors, and they should be slightly thicker and more like futuristic glasses. The text that appears when you pick up the gauntlet
> or start the game also sounds awkward and unnatural, it needs to be rewritten to feel like real game dialogue or clear, human‑sounding
> explanations. The left‑trigger special attack needs to be significantly more powerful, with a heavier feel, better particle effects, and
> a more dramatic impact, because right now it feels like a weak gust of wind with no weight. The small mini robots also look strange and
> need a better, more cohesive design. The health bar should be a single continuous bar instead of a series of tiny segments, glowing green
> and transitioning smoothly through gradients, green to lighter green to lime to yellow to orange to red, and if you have overheal above
> 100, it should shift into blue tones that deepen into purple as it increases. The enemy count in the top left needs to be larger and
> visually cleaner because the current tiny bars look bad. The charging enemy in the second stage needs to appear earlier and more
> frequently before the boss fight, and its design needs a major overhaul because it's too basic, lacks personality, and doesn't feel
> meaningful; there should be a mini challenge with three or more of them plus other enemies before the boss to encourage players to use
> more than just the gauntlet. The second boss also needs a better design, more fluid animations, and improved hit effects that match the
> intended impact, because right now the visuals don't align with the feel of the attacks. I like the punch effect, but it needs more color
> variation instead of just one pink tone, give it an aura with lighter and darker matching colors, and apply similar treatment to all
> enemies when punching; regular melee could be blue while the gauntlet punch stays pink. The shotgun boost feels great, but pressing left
> trigger with the shotgun looks weird and doesn't feel like a proper zoom‑in, and the angle is awkward, so that needs fixing. This entire
> prompt is meant to run for hours, six to eight hours minimum, so make sure you build all of this and go beyond it, thinking about
> additional mechanics, improvements, and new ideas. Remove the strange blue bar under the aim reticle and the charged attack indicator
> because it's unclear and misplaced. The worm boss feels fine overall, but its attacks need clearer telegraphs, and it needs the same
> yellow‑red‑orange color theme as before with a cooler design; its arena should be altered to incentivize shotgun boosting more, and it
> should burrow like the previous version. It also needs to appear earlier in the stage to help build anticipation and teach mechanics
> before the fight. The fourth level boss fight needs a much better design with colors that match its theme, far more fluid animations, and
> more varied attacks, because right now it looks stiff, ugly, and doesn't incentivize movement; the arena isn't fun and doesn't support the
> intended gameplay, so the boss likely needs a major overhaul, new arena, new design, new colors, new animations, new attacks, and proper
> multi‑phase structure that encourages slow‑motion grapple mechanics. The right‑bumper grapple should replace the dotted line with a single
> long glowing aura‑like line that feels powerful and stylish. The fifth level boss also needs an overhaul because it's not fun, the level
> doesn't make sense, and the flamethrower attack looks ugly, doesn't match the game's style, and lacks weight; all weapons need better
> effects, animations, and impact so they feel heavy and satisfying. I'll stop here, but please use the best possible methods available
> online to refine, redesign, and elevate all of this far beyond what I've described.

**GG will not answer during the night.** Every call is yours: write each one in `DECIDE47.md` (untracked) with the reason, then go on. If
GG writes during the night, his words come first. Base: `port/stage3` at `u3-n46` (check `git rev-parse u3-n46` equals HEAD 7fdd89bf; if
not, base on HEAD and log it). t0 = GG's go. The lead writes `date` on every log line. The play build stays `Builds/n46-r3` unless a new
build passes verbs37 on levels 1-5 and the smoke.

## 0. The one rule of tonight: nothing GG asked for is skipped

- Section 2 is **the ask ledger, G1-G45**: every request in GG's text, one row each, with its owner and its done-when. The ledger is
  copied to `Logs/n47/ASKS.md` at G0 and updated at every integration (state: not started / building / GREEN / SHORT).
- **A row closes only as GREEN** (its done-when measured and its Player frames looked at by the lead) **or SHORT** (two real attempts,
  the measured reason, the best attempt committed behind its row, and what the next night needs). "Not reached" is not a state. A row that
  is still "not started" at R3's integration gets the next free builder at once.
- **The cut order (section 8) never removes a G row.** It cuts depth (variants, polish, extra frames), never coverage.
- **"Go beyond" is a row too (G9, G44, G45).** It is built in R4 (section 6) after every other row has a first GREEN or SHORT.
- The close table in `NIGHT47-LOG.md` and GG's page `NIGHT47-REPORT.md` list all 45 rows with their state, frame path and how to switch
  each change off.

## 1. What GG's words change in the standing rules (order of authority: GG's message outranks CLAUDE.md; log each in DECIDE47)

| standing rule | tonight | why |
|---|---|---|
| CLAUDE.md rule 7, "GG owns enemy design" | **lifted for the enemies and bosses GG named**: visors (all enemies), the big shield enemy's shield, the small robots, the CHARGER, THE ICEBREAKER, THE KILN WYRM's look, THE LOOM, THE CROWN. New looks keep the NIGHT43 pill language (DECIDE43 D-4) and one shape logic for the whole faction. Sound stays GG's: no audio code, no AudioSource, no sound files; new events get web-style names in `docs/AUDIO-HOOKS.md`. | GG asked for each by name |
| DECIDE46 N-5, "nobody edits Mover.cs" | **lifted for one card: builder W's `climb47`** (G1, G2). Behind its sim row, off = NIGHT46's movement. The golden vectors and the 16 trajectory replays stay exact with the row off at every Core commit (CLAUDE.md rule 3); copy how ride40 / wallgrip36 kept them exact. | GG names the climb's timing and momentum, which only the Mover owns |
| bosses "one to two minutes, maybe three" (w38), bot band 30-60 s | **a human band: 90-150 s median, at most 180 s at p90, never under 60 s**, measured by the new human-model pilot (section 4), not the old bots. | GG: "barely damage it even while firing consistently ... two to three minutes maximum". The bots fight perfectly; GG does not. |
| arc40 / meter42 (GG's own w40 / w42 asks: one curved bar under the reticle) | **removed** (G19). PRINCIPLES 1 still holds: what the bar said (the charge level, FULL, the cooldown, the PULSE's kill pips, the BLAST's cooldown) moves onto the gun itself (a glow on the viewmodel) and the weapon panel. Nothing the player could read is lost. | GG reversed it; the info must not vanish with it |
| rule 8 palette | the gauntlet stays pink (DECIDE43 D-3 already accepted it; keep its hue measurably apart from RIVAL's magenta). Plain fist melee is blue in the cyan family. Enemy punches use the red-orange family. The health bar's red end is a danger meaning (allowed). Overheal blue to purple: the purple stays out of RIVAL's magenta hue range (write both hues in UnityTuning). | GG's colors, kept inside the language |
| feel rows OFF after a tied blind pair (NIGHT46's kicks) | **a GG ask is ON at the freeze when its gates are GREEN, verbs37 passes on levels 1-5, the smoke passes and the frames show the change.** Blind pairs choose between variants of an ask; they never veto the ask. | GG asked for these by name |
| "never add boss HP" (HANDOFF) | kept: every pool goes down or stays; length comes from windows, chip damage and phases, never from more HP. | |

Everything else in CLAUDE.md, docs/PRINCIPLES.md, docs/DESIGN-RULES.md and the NIGHT46 builder rules holds (section 7).

## 2. The ask ledger (G1-G45; owner in brackets; "frames" = Player frames before and after, named in the row's reel phase)

**THE WICK, level 1's boss (builder W)**
- **G1 The climb is forgiving at the bottom and fully possible at the top.** Done when: the sloppy pilot (section 4) clears each bottom-third
  segment first try in >= 95 % of 40 seeds and each top-third segment in >= 80 %; no segment needs an input inside a window under 150 ms;
  every gap is sized as a ratio of the measured reach (section 3); the climb takes 25-45 s for the sloppy pilot.
- **G2 No slipping further down each try.** Done when: a chain of five wall jumps on a climb wall gains the same height per jump (+-5 %), a
  failed jump never drops the player more than one segment (catch ledges), and a retry starts at full climb speed.
- **G3 Three phases.** P1 in a modified arena (the current one, changed); P2: the platforms appear at the transition and the climb IS the
  phase; P3 at the top, P1's kit remixed: faster, more combined, telegraphs clearer, not shorter. Done when: the boss card (section 5) and
  frames of each phase from the entry, the transition beat, and the top.
- **G4 2-3 minutes.** Done when: the human-model pilot's median kill is 90-150 s and its p90 is <= 180 s over 40 seeds, and 10 s of steady
  fire with the starting gun outside any window moves the bar by >= 5 % (it lands and it shows).
- **G5 Fight text easy to read** (W writes the words; U owns the system). Done when: every boss line meets section 3's text spec on the
  frames at 1280x720 and 1920x1080.
- **G6 A few more attacks.** At least 2 new ones, each with a tell, a counter and a punish window (section 3).
- **G7 P3's crack-open charge.** The boss charges, cracks open, a yellow weak point (yellow = punch it, the game's parry language) pulses for
  >= 2.5 s with world-space indicators; a melee hit takes 20-25 % of the bar and launches the player back 12-18 m with a short slow motion
  and a hard camera hit; the first time it shows a big "PUNCH THE CORE" line. Done when: frames of the crack, the hit, the launch, the
  landing; the human-model pilot lands it in >= 70 % of P3 openings.
- **G8 A translucent floor fills the shaft at the top.** It appears when the player reaches the top, it holds the player and enemies, it
  reads as a floor (a cyan-edged glass pane, player-usable), and no fall back down the shaft is possible in P3. Done when: frames + a 40-seed
  run with 0 falls into the shaft in P3.
- **G9 Push past the notes.** At least 3 ideas of W's own, researched, each behind its own row, shown on frames (R4).

**Rendering and light (builder E)**
- **G10 The door turns pink or bugs out when running through.** Done when: the cause is named with evidence and fixed, and 200 scripted door
  passes at sprint, slide and dash speed across levels 1-5 in a Player show 0 pink or broken frames (a pixel check for magenta and for NaN
  black / white blocks, plus the lead's look at a sample).
- **G11 The lava lighting is consistent.** Done when: one written lava light rule (section 3) applied on levels 1-5, and a walk past every
  lava field in a Player shows no light popping on or off (the visible light count and the luminance of a fixed probe point logged per frame:
  no jump over 15 % between frames that is not a scripted flare).

**Enemies (builder E, the CHARGER is builder I's)**
- **G12 The big shield enemy's shield** (the WARDEN; if GG's enemy is another one, fix that one too). Done when: the shield sits centred on
  the body's front at chest height, matches its collider, turns with the facing without lag or clipping; frames front / side / 45 degrees / in
  a fight.
- **G13 Visors: sharper, angular, pointy, a bit thicker, like futuristic glasses.** For every enemy with a visor. Done when: section 3's visor
  spec on every model, a grayscale silhouette sheet at 10 / 20 / 30 m, before and after.
- **G14 The small robots get a cohesive design.** Find which models GG means (frame every small enemy; if in doubt, redo all of them). Done
  when: one shape logic shared with the faction, the silhouette sheet, before and after.

**UI and words (builder U)**
- **G15 The gauntlet pickup and game start text sound human.** And every other player-facing string (teach cards, banners, pickups, level
  starts, boss lines). Done when: `docs/COPY47.md` lists every string before / after against section 3's copy rules, and frames of each.
- **G16 One continuous health bar with the green-to-red gradient.** Done when: section 3's bar spec, frames at 100 / 80 / 60 / 40 / 20 / 5 %.
- **G17 Overheal above 100 shifts blue, then deepens to purple.** If the game has no overheal today, U shows it and the lead decides the
  overheal sim row (section 6, `overheal47`). Done when: frames at 110 / 130 / max.
- **G18 The enemy count top left: larger and cleaner.** Done when: section 3's counter spec, frames with 12 / 3 / 1 / 0 left.
- **G19 Remove the blue bar under the reticle and the charged attack indicator.** Done when: frames show the reticle alone, and frames show
  the same information on the gun and the weapon panel for the KESTREL charge, the THUNDERCLAP charge, the PULSE pips and the BLAST cooldown.

**Weapons and player effects (builder X)**
- **G20 The gauntlet's LT blast: far more powerful, heavy, dramatic.** A sim row `pulse47` for its damage and push (numbers in config with a
  why) and section 3's blast effect. Done when: knockback distance at least 2x today's on a husk, the frames, and a blind pair of two
  variants for the look.
- **G21 The punch aura: lighter and darker matching tones; fist melee blue, gauntlet pink; enemies' punches get the same treatment** (the
  enemy side is builder E's, red-orange family). Done when: frames of each punch's start / hit / fade.
- **G22 The THUNDERCLAP's LT is a proper zoom with a natural angle.** Done when: section 3's zoom spec, frames at hip / mid-zoom / full zoom,
  and the boost still fires at once in the air (VERBS 29, air latency 0 ms).
- **G23 The grapple: one long glowing aura line instead of the dotted line.** (GG calls it the right bumper: check the pad bind and every
  prompt agree, and log what the bind is.) Done when: section 3's line spec, frames of the aim, the fire, the pull, the release, the BEND.
- **G24 Every weapon: better effects, animations and impact, heavy and satisfying.** The KESTREL (tap, charge), the THUNDERCLAP (shot, slug,
  boost), the PULSE (punch, blast), the TORCH, the fist, the swap. Done when: per weapon a contact sheet (fire -1 to +30 frames) before and
  after, section 3's weapon weight targets met, and a blind pair per weapon that picks the new one (3 judges; a tie keeps the new one ON
  because GG asked for it, and goes into the report).

**Level 2 (builder I)**
- **G25 The CHARGER appears earlier and more often before the boss.** Done when: the first CHARGER in the first third of level 2 alone in a
  teaching beat, then in at least 3 more rooms before the boss; the walk 10 / 10.
- **G26 The CHARGER's design: a major overhaul with personality.** A new look, a readable wind-up, a reason to exist (section 5's foe card).
  Done when: frames of the idle, the wind-up, the charge, the wall stun, the death; the counterplay works in the bot.
- **G27 A mini challenge before the boss: 3 or more CHARGERS plus other enemies, built so the gauntlet alone is not the best answer.**
  Done when: the room's card; in the human-model pilot, a gauntlet-only run takes measurably longer or more damage than a mixed run (logged
  numbers); frames from the lip and mid-fight.
- **G28 THE ICEBREAKER's design is better.** (a boss card, a new look)
- **G29 Its animations are fluid.** (section 3's animation rules)
- **G30 Its hit effects match each attack's impact.** Done when (G28-G30): frames of every attack's wind-up / hit / impact, the human band
  (G4's numbers), and the boss card answered.

**THE KILN WYRM, level 3's boss (builder K)**
- **G31 Clearer telegraphs.** Every attack gets a tell of 0.8-1.5 s that says where (a ground crack or a lit line where it will rise, a glow
  ramp). Done when: frames of each tell at its start and its last 0.2 s.
- **G32 The yellow-red-orange theme as before, a cooler design.** Find the version GG means in git (the NIGHT26 Tripo look, `design.n26.
  wormVisual`, and earlier looks), restore its molten yellow-red-orange, then make it cooler (section 5). Done when: frames before / after.
- **G33 The arena rewards the THUNDERCLAP's boost.** Heights and gaps only the boost clears, a weak spot reachable from the air. Done when:
  in the human-model pilot, boost uses per minute up by >= 2x against today's arena, and the card says why each piece exists.
- **G34 It burrows like the previous version.** Find the burrow in git (Wyrm.cs's DIVE: "a floor crack, then it burrows"), bring it back in
  the current fight. Done when: frames of the dive, the travel under the floor, the breach.
- **G35 It appears earlier in the stage to build anticipation and teach.** At least 3 beats before the fight: a glimpse, a pass under or
  through a room, and one room where it surfaces and teaches its main tell without the fight. Done when: frames of each beat; the walk 10 / 10.

**THE LOOM, level 4's boss (builder L)**
- **G36 A much better design, colors that match level 4's theme.**
- **G37 Far more fluid animations.**
- **G38 More varied attacks.** (at least 6 across the phases)
- **G39 A new arena that makes movement the answer.**
- **G40 A proper multi-phase fight that makes the slow-motion grapple (the BEND) the star.** Done when (G36-G40): the boss card and the arena
  card + flow plan pass the lead's gate, the human band, the BEND used >= 3 times per phase by the human-model pilot, frames per phase.

**THE CROWN and level 5 (builder C; the TORCH weapon is X's)**
- **G41 THE CROWN rebuilt to be fun.** (a boss card, new attacks and phases, the human band)
- **G42 Level 5 makes sense.** A one-paragraph premise, a route the player can read (the goal visible from the start and from each room), each
  room's job in one line; fixes to the rooms that break it. Done when: `docs/ROOMS.md`'s level 5 section, frames from each door, the walk 10 /
  10, n46flow no worse past its band.
- **G43 The flamethrower looks good, fits the style and has weight.** Both the TORCH (X) and any boss flame attack (C). Done when: section 3's
  flame spec, contact sheets, blind pairs.

**The whole night (the lead)**
- **G44 Hours of building, and beyond: new mechanics, improvements, ideas.** At least 7 h by `date`; R4 builds the idea list (section 6).
- **G45 Best methods from online research.** Every card cites its research lines (section 3's digest, plus each builder's own reading in
  `Research/2026-10-18-wave47/<builder>.md`, marked Verified when the page was read, Reported otherwise).

## 3. Targets and specs (ours unless marked; tune by the meter and the frames; log every miss)

**The climb (G1, G2; W; Mover via `climb47` only)**
- Measure first: with today's movement, record the max height gain of: a jump, a double jump, a wall jump, a wall jump chain, a ride
  jump-off at +45 degrees pitch, each from rest and from a ride (harness, then a Player trace). Write the table in W's notes.
- Gaps as ratios of the measured reach (the Titanfall 2 rule from NIGHT5: about 25 % slack on a safe jump): bottom third <= 55 % of the
  reach of the move the segment teaches, middle <= 70 %, top <= 80 %; no gap in the 0.8-1.1 band. Horizontal gaps the same way.
- `climb47` (sim row; each number in `design.n47.climb47` with a why): a wall jump off a climb wall SETS the vertical speed to at least its
  full value instead of adding to a falling one (the ratchet GG feels); a wall coyote of 0.15 s after leaving a climb wall (Celeste uses
  0.1 s for ledges and accepts wall jumps from a quarter tile away: Verified); a jump buffer of 0.15 s onto a wall; a wall-catch distance
  generous enough that a jump aimed 15 degrees off still attaches; the wall jump count refills on a climb ledge. Off = NIGHT46.
- Catch ledges so a miss costs one segment at most. Each segment's start shows the next hold in view. The look pitch rule (ride38) keeps
  working; VERBS rows 9, 10, 11, 14 and 44 get "changed by climb47" lines before the row lands.
- The sloppy pilot: jump presses jittered +-120 ms, look error +-12 degrees, one release in four late by 150 ms.

**Boss fights (G3, G4, G6, G7, G28-G41)**
- Phases: P1 teaches (2-3 attacks, generous tells), P2 changes the rule (an arena change at the transition, never mid-wave: Titanfall 2),
  P3 remixes P1 harder (combined attacks, shorter gaps, the tells still >= 0.5 s). A 1-1.5 s transition beat (banner, flash, push away).
  ULTRAKILL's Gabriel marks phase 2 at half health with a color change, a line and improved versions of the same attacks (Verified).
- Tells: wind-up 1-1.5 s is the sweet spot, 2 s at most for heavy attacks (combat designer, Game Developer: Verified). Each attack's tell
  looks and moves unlike the others; tells never overlap so they blur.
- Punish windows longer than one full combo plus the time to reach the boss (Verified, same article). Weak points obvious, exposed only in
  their window, appealing to hit rather than spiky (Verified). One color says "now": Doom Eternal's Marauder flashes green on the wind-up of
  the attack you punish (Verified); ours is yellow (the parry language).
- Damage always registers: chip damage outside windows >= 30 % of full, a hit flash on every hit, the bar's ghost trail; window damage
  full. Length from windows and phases, never from HP (pools only go down tonight).
- The human band: median 90-150 s, p90 <= 180 s, min >= 60 s, 40 seeds, the human-model pilot.
- Every attack lists: name, tell (what, how long), the counter (which verb), the punish window, the hit effect, the phase it is in.

**Animation (G29, G37, bosses and new foes)**
- Code-driven and spring-based: anticipation before every attack, overshoot and settle after, follow-through on parts (secondary motion on
  plates, tails, cables), eased curves (never linear lerps for limbs), a hit reaction on every hit (a 60-100 ms flinch spring), an idle that
  breathes. Frame-rate independent springs (NIGHT5's closed-form springs). Animation never changes the sim's hit boxes or timing: the view
  reads the sim's state.

**Hit feedback and weapon weight (G20-G24, G30, G43)**
- Layer order and numbers (uhiyama-lab, Verified): flash 80 ms; hitstop weak 0-30 ms, strong 50-80 ms, critical 100-150 ms; add flash, then
  particles, then shake, then hitstop, one at a time, toggling each to see what it adds; keep weak tiers restrained so heavy hits stand out.
  The sim's hitstop semantics stay; view-side holds only.
- Camera: the NIGHT5 springs and trauma (rotation only, trauma squared, decay ~1.5 / s). NIGHT46's kicks were built and judged "the same":
  tonight's must be visible on a contact sheet (peak camera punch light 0.5-1.0 deg, medium 1.5-2.5, heavy 3-5; settle <= 0.35 / 0.5 / 0.7 s).
- Muzzle 1-2 render frames; impact = sparks + a short light + a small mark; the kill gets a world-space flare in the player's family.
- Every effect scales with EFFECTS INTENSITY, is off under reduced motion, never covers the reticle's centre box (10 % of the screen), and
  never fills the screen (GG w30).
- The LT blast (G20): an 80-120 ms gather (inward particles, the glove brightening) that never delays the input's effect by more than the
  current blast; a 1-2 frame white-pink core flash; a shockwave ring 0 to the blast radius in 0.2 s with a screen-space distortion band;
  debris and dust; flung enemies tumble (view); hitstop 70-90 ms; trauma +0.5; FOV punch 2-3 deg; embers 0.5 s.
- The punch aura (G21): three tones of one hue per punch: a light core, a mid body, a darker rim, plus a soft halo; fist = cyan-blue
  family, gauntlet = pink family, enemies = red-orange family; the hit adds a ring in the same three tones.
- The THUNDERCLAP zoom (G22): world FOV eased to about 80 % of the base in 150 ms (keep the existing zoom code path: w33's world-FOV-only
  rule; the gun never slides to the centre); the gun rises to a shouldered pose from the lower right with its barrel on the reticle's line,
  no roll; the spread shown by the reticle tightening.
- The flame (G43): a cone of soft flipbook flames with a white-hot core at the nozzle, color by family (the TORCH: the player's cyan-blue
  plasma, white core to blue to violet tips, per rule 8; a boss flame: red-orange); heat haze at the cone's edge; embers; a flickering light
  on nearby surfaces; scorch marks; a pressure kick on start and a continuous low recoil while held; a lick on release. Doom Eternal treats
  its flame as a role in the loop (burning enemies drop armor: Verified), so give the TORCH's burn a readable state on enemies too.

**The grapple line (G23)**
- One continuous ribbon: an HDR white-cyan core that bloom picks up, a soft additive glow shell 3-4x its width, tapering from the hand to the
  hook. On fire: the line flies out with a decaying wave (2-3 waves, straight in 0.15 s). While pulling: a bright pulse runs hook to hand
  every 0.25 s. In the BEND: the line brightens and its wave slows with time. On release: it snaps back. The aim preview is a thin faint
  solid line to the valid target (no dots, no target diamond: w33).

**HUD and text (G5, G15-G19)**
- Text size: Xbox's guideline is 26 px body height on console and 18 px on PC at 1080p as launch minimums, with player scaling (Verified,
  XAG 101). SKYGRIND is pad-first for Switch and Steam Deck: boss and fight lines >= 36 px, teach cards >= 30 px, the counter >= 32 px, at
  1080p. A text size setting (XAG asks for scaling) in OPTIONS.
- Readability: a dark outline (2-3 px) and a soft shadow, no filled boxes (w33); the theme fonts, bold weights; at most 6 words per fight
  line; one message at a time, held 2.0 s + 0.3 s per word; upper third, never over the reticle.
- The health bar (G16, G17): one continuous fill, no segments; the color by health share, interpolated in a perceptual space (OKLab) through
  green, light green, lime, yellow, orange, red at 100 / 80 / 60 / 40 / 20 / 0 %; a soft glow in the bar's own color; a ghost trail on damage
  (a lighter segment that drains after 0.4 s); below 25 % a slow pulse and the number brightens (red and green alone fail for players with
  red-green color blindness, so the low state is also told by brightness, motion and the number). Overheal from 100 to the cap: blue, then
  deeper blue, then purple at the cap, drawn as a second layer over the full bar.
- The enemy counter (G18): a number plus a short word ("6 LEFT"), >= 32 px, top-left inside the safe area, one thin line under it that drains
  per kill, a short flash per kill, "LAST ONE" at 1, cleared state at 0. The tiny bars go.
- Copy rules (G15): second person, verb first, plain words, at most 10 words a line, the button glyph shown (pad first), a reason when it
  helps ("THE GAUNTLET. Punch with RT. Hold LT to blast."), no internal names, no jargon, read aloud it sounds like a person talking. Two
  variants for the gauntlet pickup and the game start; the lead picks.

**Enemy looks (G12-G14, G26)**
- Shape language (rocketbrush, Verified): triangles read as danger, squares as heavy, clean panels as technology; one dominant shape logic for
  the whole faction; check every model in grayscale, at distance and in motion.
- The visor (G13): an original design, not a copy of any character: one band about 1.3-1.6x today's height that wraps the head and sweeps
  back to points at the temples, a V dip at the bridge, two faceted planes meeting at a crease, an emissive red-orange slit with a bright top
  edge. It must read as "futuristic glasses" in the 30 m grayscale silhouette.
- The CHARGER (G26): Doom 2016's Pinky has an armored front, a soft back and sides, turns badly mid-charge, is stunned by a wall and can be
  led off a ledge (Verified). Ours: a heavy armored front plate (triangle shapes), a 0.8-1.0 s wind-up (it lowers, its vents glow, a lit
  line shows its path on the floor), a charge that cannot turn sharply, a wall stun of about 2 s with its back vents open (the weak spot), a
  counter for every verb: the slide or dash dodges it, the KESTREL's charged shot pierces the back, a punch on the stunned CHARGER launches it
  (launch36), a CHARGER that hits another enemy bursts it.

**Rendering (G10, G11)**
- Measured tonight: `Assets/Settings/PC_Renderer.asset` uses Forward+ (`m_RenderingMode: 2`), which has no per-object light limit but a
  per-camera visible light limit (Unity docs, Verified); the game finds 13 shaders with `Shader.Find`, and only 4 custom shaders (Enemy,
  Particle, BlobShadow, Motes) are in Always Included (some others may be in a Resources folder: check each).
- Pink is Unity's fallback for a shader it could not find or compile; a shader only found from code can be stripped from a build unless it is
  referenced or Always Included; a development build's console names the dropped shader (Verified). Hypotheses to test in this order, each
  with an A / B in a Player: (a) a shader found by `Shader.Find` that is not in the build, used when a door or room state changes; (b) the
  full-screen passes that react to speed (`TravelBlurFeature`, `Crt29Pass`); (c) NaN or Inf in HDR color spreading through bloom (clamp
  emissive, check for divides by zero in door / speed shaders); (d) a material swap on the door at the frame it opens. Fix the cause, not the
  symptom.
- The lava light rule (G11): the glow comes from the emissive surface; real lights are few, fixed per area (one per N m2 with one intensity
  and range per theme, set as Important or kept under the visible limit), never created per frame; any flicker is a scripted, slow, low
  amplitude curve; the same rule on levels 1-5.

## 4. The meters (build first: no tuning before the baselines; NIGHT5's lesson)

- **The human-model pilot** (lead, at G0, in `Tools/SimHarness/Harness.N47.Human.cs`; every boss builder uses the same one): the existing
  boss pilots plus human limits: hit share capped at 45 % hip-fire and 65 % zoomed, 250 ms reaction to a tell, one dodge in five late,
  damage taken, verbs used as a player would (the boss card names which). It reports kill time, damage taken, deaths, window hits, verbs used
  per minute. Baselines for all five bosses at G0 + 20 min, 40 seeds. Its first numbers must match GG's read ("you can barely damage it"):
  if the WICK dies in 40 s for this pilot, the pilot is wrong; fix the pilot before tuning the boss.
- **The sloppy climb pilot** (W): section 3's jitter, on the climb.
- **The flow meter** `n46flow` + `bash Tools/night/n46-flow-table.sh` (no level worse past its band) for every level change.
- **Frames**: one reel phase per builder in `Game/FeelReel.N47.<Name>.cs` (the lead pre-places the routing at G0, as NIGHT46's
  `FeelReel.N46.cs`); contact sheets for weapons (fire -2, 0, +1, +2, +3, +4, +6, +8, +12, +18, +30); 1280x720 and 1920x1080.
- **Blind pairs** (`Tools/night/n46-blind-pairs.cjs`): to choose between variants; three judges (fresh subagents shown only the two frames
  and one question).
- Everything is proved twice: RED once (before, or with the row off) and GREEN once (L-014).

## 5. Cards before code (the gate at ~1:15; the lead answers PASS or names the one line to fix)

- **Boss cards** (W, I, K, L, C) in `docs/BOSSES47.md`: the fantasy in one line, the silhouette and the palette, the arena's job, then per
  phase: the attacks (section 3's list per attack), the rule change, the verb it stars, the length target, the transition beat. The card cites
  its research lines. W's card keeps what GG likes about THE WICK (say what that is from the code and the frames) and changes the rest.
- **Arena cards + flow plans** (W's modified P1 arena and its top, I's stampede room, K's arena, L's new arena, C's level 5 rooms): the NIGHT46
  method (DESIGN-RULES A-9 card, arena-flow-web.md Part D's 11 points with numbers) in `docs/ROOMS.md` under "NIGHT47".
- **Foe cards** (I's CHARGER, E's shield enemy, the small robots, the visor sheet): job, shape logic, wind-up, weak spot, counters, frames.
- **The HUD spec** (U), **the copy inventory** (U: every string, before), **the FX spec** (X: per weapon the layers and numbers), **the
  render diagnosis plan** (E).
- Geometry and code only after PASS. A piece no card line asks for is cut.

## 6. The rows, the shared commit and the extras

- Rows as data (docs/NIGHT-ROWS.md): `design.n47.<row>`, sim rows with `"sim": true`, read by `rows.On(idXxx47)` in each builder's own
  `SurvivalSim.N47.<Name>.cs`; off = `-skygrindOff47` / `--off47` = NIGHT46's game. View rows have no sim bit. Numbers in each builder's own
  row with a `_why`, the two config copies byte-identical (`cmp`); render numbers in `UnityTuning.<Name>47.cs`.
- Planned rows (the lead places them all, off, at G0 with `Tools/night/n47-shared.cjs`, modeled on `n46-shared.cjs`; dry-run it once before
  the real run): W: `wick47` (sim), `climb47` (sim, Mover), `wickTop47` (sim: the floor), `wickLook47` (view); I: `charger47` (sim + view),
  `iceline47` (level), `brawl47` (sim), `brawlLook47` (view); K: `wyrm47` (sim: tells, burrow), `kiln47` (level: foreshadow beats, arena),
  `wyrmLook47` (view); L: `loom47` (sim), `span47` (level), `loomLook47` (view); C: `crown47` (sim), `spire47` (level), `crownLook47` (view);
  X: `pulse47` (sim: the blast numbers), `fx47` (view: every weapon), `grapple47` (view), `zoom47` (view); U: `hud47` (view), `copy47` (data),
  `overheal47` (sim, the lead's call); E: `visor47`, `foes47` (view: shield, small robots), `door47`, `lava47` (render fixes). Add a row with
  the lead's OK; never edit an older row or level file in place.
- Hooks the lead pre-places so no builder edits shared files: the boss phase hooks, the human pilot, the reel routing, a `Mover` hook for
  `climb47` (W alone writes in `Mover.cs`, behind the row), the HUD root slots U needs.
- **R4, the beyond round (G9, G44, G45).** After every G row has a first GREEN or SHORT, each builder builds up to 2 ideas of its own, each
  researched, each behind its own row, each with frames. The lead's starting list (take, improve or replace): the yellow crack language
  shared by all five bosses (one rule the player learns once); a boss finale beat (a short slow motion on the last hit, the arena reacting);
  a checkpoint at each phase start (retry46 extended to phases); `overheal47` (a kill spill above 100 up to a cap, decaying to 100 slowly:
  decay, not regen, so the w19 "no passive regen" rule holds); a text size and health colors option; enemy hit reactions that read the
  hit's direction; the CHARGER's stampede as a SURVIVAL event; boss names and one line of intro at the walk-in, big and readable.

## 7. Hard rules (CLAUDE.md and the NIGHT46 builder brief, restated; section 1's changes apply)

- **Git:** never commit to `master`, never push, never merge into master, never `gh auth switch`. Builders work in their own worktrees on
  branches from `port/stage3` at the shared commit; the lead cherry-picks. Explicit paths plus `.meta` files (a new `.meta`:
  `fileFormatVersion: 2`, a fresh 32-hex guid), never `git add -A` / `.`. One commit per slice. Never stage GG's four files
  (ProjectSettings/UnityConnectSettings.asset, ProjectSettings/EditorBuildSettings.asset, Assets/ProjectSettings/UIEffectProjectSettings.asset,
  Assets/Scenes/Arena.unity). Restore the TMP fallback font asset's Play-mode rewrite before a commit (CLAUDE.md rule 1). Night prompts stay
  untracked; `NIGHT47-LOG.md` is tracked. Trailer: `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- **Only the lead** drives the Unity Editor, builds, Players, reels, the suite and git on `port/stage3`. Builders use the CoreHarness, the
  SimHarness and `bash Tools/night/dn-check-wt.sh <their scratch dir>`, and name the frames they need.
- **Movement exact:** golden 0 mismatches and 22 replays exact at every Core commit (with `climb47` off); `fingerprint`, `--off46` and
  `--off47` EQUAL before each code commit.
- **Numbers** from config (`GameConfig`) only; Unity-only numbers in UnityTuning with a comment; patches by `.cjs` scripts written with the
  Write tool (never `node -e`, never a heredoc). **Randomness** only from `Mulberry32` in sim code; view randomness from a separate seeded
  cosmetic generator. Whole-step timers. No allocation per sim step. No Unity API in MonoBehaviour field initializers (L-035).
- **No audio.** Event names only. **Look:** cyan player-usable, red-orange enemies and danger, yellow parryable / punch it, magenta RIVAL,
  pale violet-white the boundary; text sizes per section 3; no effect over the whole screen.
- **Nothing lost (PRINCIPLES 1):** a change that touches a verb names its VERBS.md row; verbs37 on levels 1-5 after the rows go ON.
- **Docs** with the Write / Edit tools only; write only what was measured or read, otherwise "not checked".
- **The Editor:** `editor_focus` before Play checks; never recompile in Play mode; never `unity test`; one Play session or build at a time;
  batch-mode builds from `C:\Users\gaige\dev\SKYGRIND-unity-batch` when the live Editor stalls (NIGHT46). Close every Player by its PID.
- **Disk:** `df` at G0 and before each build; under 12 GB delete numbered frames after their verdicts (DECIDE46 D-1's rule); under 6 GB no
  new reels.

## 8. The clock (by `date`; the close table no earlier than 7:00; hard cap 8:30)

| time | the lead | builders |
|---|---|---|
| 0:00-0:35 | G0: read CLAUDE.md, HANDOFF, CORE, PRINCIPLES, DESIGN-RULES, NIGHT46-REPORT, DECIDE46, this file; `df`; the ledger to `Logs/n47/ASKS.md`; `n47-shared.cjs` dry run then real; the human-model pilot; spawn builders | (spawned at ~0:25) read sections 0-7 and their card |
| 0:35-1:15 | baselines: the human pilot on 5 bosses, the flow table, before-frames for every G row | cards, research notes, their own baselines |
| ~1:15 | **the card gate** (15 min) | fix the one line |
| 1:30-2:50 | R1 integrate + build `n47-r1` at 2:50, frames with rows ON in the build's copy | round 1 |
| 2:50-4:10 | R2 integrate + build `n47-r2` at 4:10, blind pairs | round 2 |
| 4:10-5:30 | R3 integrate + build `n47-r3` at 5:30; every row still "not started" gets a builder now | round 3 |
| 5:30-6:40 | R4 (the beyond round) + second attempts on SHORT rows; build `n47-r4` (the candidate) at 6:40 | round 4 |
| 6:40-7:40 | Player fixes only; the ON calls (section 1's rule); `n47-rows-on.cjs`; verbs37 levels 1-5; the suite; the smoke | fixes the lead names |
| 7:40 | freeze; tag `u3-n47` | |
| 7:40-8:30 | close: NIGHT47-LOG close table (all 45 rows), NIGHT47-REPORT, HANDOFF, DECIDE47, `wave-memory` (THREADS consolidated) | |

Each build: `bash Tools/night/n41-build.sh n47-rK`; rows ON in its copy with `node Tools/night/n46-build-rows.cjs n47-rK on n47.<row> ...`;
reels with `Tools/night/n47-reels.sh` (copy `n46-reels.sh` with NIGHT=n47 at G0).

**Cut order (depth only; no G row is ever dropped):** blind pairs beyond one per ask, then the second resolution's frames, then R4's second
idea per builder, then variants (one variant instead of two), then the n47 polish of looks beyond the spec. Never cut: the meters, the cards,
a first attempt at every G row, the ON calls, verbs37, the smoke, the close.

**The spend limit** has stopped every agent at once before (L-003): commit every slice; the lead writes a close skeleton and a HANDOFF note
at each build; on a 429 the lead goes on alone in this order: integrate what is committed, build, the ON calls, the smoke, the close, then
the G rows still open, one by one, in ledger order.

## 9. The builders (eight; each owns only its files; hand-offs go through the lead)

| id | owns | G rows | key files (read first) |
|---|---|---|---|
| W | THE WICK, the climb, the top floor, the WICK's arena and words | G1-G9 | Core/Enemies/Wick37*.cs, Wick37Stack.cs, Core/Mover.cs (climb47 only), n46-foundry's boss rooms, VERBS rows 9-14, 44 |
| I | level 2: the CHARGER (look and behavior), its teaching, the stampede room, THE ICEBREAKER | G25-G30 | Core/Enemies/Brawl40*.cs, Game/Brawl40View, EnemyViews.Charger.cs, SurvivalSim.Enemies19.cs, gen-n46-iceline.cjs |
| K | THE KILN WYRM: tells, theme, burrow, arena, foreshadowing | G31-G35 | Core/Enemies/Wyrm*.cs, WyrmArena.cs, Wyrm26.shader, Wyrm26Aura.shader, gen-n46-kiln.cjs, gen-n46-wyrm-arena.cjs, git log of Wyrm.cs |
| L | THE LOOM and its new arena | G36-G40 | Core/Enemies/Loom37*.cs, Loom30 / 31, gen-n46-span.cjs, the BEND (bend34), VERBS 17-23, 46 |
| C | THE CROWN, level 5's sense, the boss's flame | G41-G43 (boss) | Core/Enemies/Crown37*.cs, Crown32*.cs, Game/Crown37View.Crown40.cs, gen-n46-spire.cjs, docs/ROOMS.md level 5 |
| X | every weapon's feel, the LT blast, the punch aura (player side), the zoom, the grapple line, the TORCH | G20-G24, G43 (TORCH) | Game/Viewmodel*.cs, GunFx*.cs, Core/Guns/Pulse25.cs, PulseCharge27.cs, Torch*.cs, Arcline21View.Rope33.cs, Grapple31View.cs, Feel43.cs |
| U | the HUD, the text system, every string | G5 (system), G15-G19 | Game/UiHud33.cs, UiHud38*.cs (arc40, meter42), Hud33Layout.cs, BossLine40, UiTeach35, UnityTuning.Theme.cs |
| E | enemy looks (visors, shield, small robots, enemy punch aura), the door bug, the lava light | G10-G14, G21 (enemy side) | Game/EnemyViews*.cs, Enemy40View / 43View, UnityTuning.Models4*.cs, Doors38View.cs, DoorFrames29.cs, LavaView30.cs, Render/*.cs, Assets/Shaders |

Builder reports: under 300 words: each G row's state, every commit sha, every check with its measured number, before / after meter numbers,
the frames the lead must take, hand-offs. Last line: `N47-BUILDER <id>: round <r> · G rows GREEN <list> SHORT <list> · commits <shas>`.

## 10. Research digest (read tonight; Verified = the page was read, Reported = secondary; numbers that are ours are marked ours in section 3)

- Celeste's forgiveness (Maddy Thorson, Verified): coyote time, jump buffering, half gravity at the jump's peak while jump is held, corner
  correction on jumps and dashes, wall jumps from 2 px away (a quarter tile), super wall jumps from about 5 px, a stamina refund grace on a
  wall jump straight up. Coyote is 0.1 s in the source (NIGHT5, Verified).
- ULTRAKILL allows 3 wall jumps before landing, which resets them (Reported, a guide).
- Boss design from a combat designer (Game Developer, Verified): tells distinct and not overlapping; wind-ups 1-1.5 s, at most about 2 s;
  punish windows longer than one combo plus travel; weak points obvious, exposed only in their window, inviting not spiky; an arena that shows
  only what matters; usable objects clearly usable.
- Doom Eternal's Marauder (Doom Wiki, Verified): its eyes flash green on the wind-up of the axe lunge; a hit in the flash staggers it for a
  couple of seconds. One color, one moment, one answer.
- Doom 2016's Pinky (Doom Wiki, Verified): an armored front, a vulnerable back and sides, poor turning mid-charge, stunned by walls, can be
  led off ledges.
- ULTRAKILL's Gabriel (ULTRAKILL wiki, Verified): phase 2 at half health with a color change, a line, and improved versions of the same
  attacks.
- Hit feedback (uhiyama-lab, Verified): flash 80 ms, hitstop tiers 0-30 / 50-80 / 100-150 ms, layer order flash, particles, shake, hitstop;
  restraint on light hits; hitstop only on the first and last hit of a multi-hit.
- Shape language (RocketBrush, Verified): triangles danger, squares heavy, panels technology; one dominant shape logic per faction;
  grayscale, distance and motion tests.
- Text (Xbox Accessibility Guideline 101, Verified): 26 px console and 18 px PC minimum body height at 1080p, scalable by the player; an
  outline counts toward height only above 2:1 contrast.
- URP (Unity docs, Verified): Forward has a per-object light limit; Forward+ has none but limits lights visible per camera. Light popping in
  Forward comes from that per-object cap (bugnet, Verified).
- Pink materials (bugnet, Verified): the fallback for a shader that could not be found or compiled; runtime-only shaders get stripped unless
  referenced or Always Included; a development build's console names the shader.
- Doom Eternal's flame belch (Doom Wiki, Verified): a short burst that sets enemies burning; burning enemies drop armor, a role in the combat
  loop.
- Red-green color blindness is common, so a health color must not be the only signal (general accessibility knowledge; no page read
  tonight).
- Earlier research still valid: NIGHT5's springs, trauma shake, viewmodel weight and Titanfall map metrics; Research/2026-10-17-wave46/
  (arena flow: id's GDC 2018 talk; the flow metrics); Research/2026-10-12-wave37/BOSSES.md.

Sources: maddymakesgames.com/articles/celeste_and_forgiveness; gamepretty.com/ultrakill-abilities-tech-guide; gamedeveloper.com/design/
opinion-boss-design---tips-from-a-combat-designer; doomwiki.org/wiki/Marauder; doomwiki.org/wiki/Pinky_(Doom_2016); doomwiki.org/wiki/
Flame_belch; ultrakill.fandom.com/wiki/Gabriel,_Judge_of_Hell; uhiyama-lab.com/en/notes/unity/unity-game-feel-hit-feedback;
rocketbrush.com/blog/shape-language-in-game-character-design-how-to-make-characters-readable-and-consistent; devdocs.xbox.com
(XAG 101 text display); docs.unity3d.com/Manual/urp/rendering/forward-rendering-paths.html; bugnet.io (per-object light limit; pink materials
after build).

## 11. The close (all of it)

- `NIGHT47-LOG.md` close table: all 45 G rows (state, measured result, frame path, row, how to switch it off), the human band per boss
  before / after, the flow table, the suite, verbs37, the smoke.
- `NIGHT47-REPORT.md`, GG's page, in plain words: what changes for a player per level, each boss in one paragraph, the frames to look at,
  what is SHORT and why, and the play build path.
- HANDOFF.md (a new top block), DECIDE47.md, `docs/BOSSES47.md`, `docs/COPY47.md`, docs/ROOMS.md, VERBS.md lines, `wave-memory` with THREADS
  consolidated. Tag `u3-n47`. Nothing pushed, nothing merged. `git status` shows only GG's four files and untracked prompts.

## Kickoff (GG pastes this into a fresh Claude Code session in `C:\Users\gaige\dev\SKYGRIND-unity`)

```
/effort high
You are the lead of NIGHT47. Read C:\Users\gaige\dev\SKYGRIND-unity\NIGHT47.md whole, then CLAUDE.md, HANDOFF.md and docs/PRINCIPLES.md,
and run the night exactly as written. GG's words at the top outrank everything. I will not answer during the night: decide every call in
DECIDE47.md and keep going. Start with `date`.
```

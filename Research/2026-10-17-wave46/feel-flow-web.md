# Feel and flow in first-person shooters: web research (wave 46)

Written 2026-10-17 for GG's ask that the game "feel and flow better". Web research only: no game code was read or
changed, and nothing here was measured in SKYGRIND.

**How to read this file**

- Each finding gives the principle, the numbers the source gives, the source, whether the source was **opened** (I read
  the page or PDF text) or is a **search summary only** (I saw search-result text but not the page), and a way to check
  it automatically in SKYGRIND's 120 Hz sim with the bot.
- At 120 Hz, 1 step = 8.33 ms. Frame counts at 60 fps are converted as 1 frame = 2 steps.
- Under CLAUDE.md, every number here is a *reference* only. A number adopted from this file goes into
  `PortExport/config.json` or a Unity-side config file, never into code by hand.
- Constraints from this project's rules and memory, checked against each finding:
  - GG owns sound (CLAUDE.md rule 7).
  - Retired and never again: screen-border speed lines, passive in-round regen, the gun sliding to the centre on zoom,
    and any movement or crowd-control verb without a long cooldown.

Several items I could not open. They are named in the "Not opened" list at the end.

---

## 1. Movement feel

### F1. Coyote time: a jump still works a short time after leaving a ledge
- **Principle:** forgive jumps that are pressed just after the ground is gone. Thorson calls these tricks part of the
  "moment-to-moment experience", with "everything fudged a tiny bit in the player's favor".
- **Numbers:** Celeste's shipped player code has `JumpGraceTime = 0.1f` (0.1 s, which is 12 steps at 120 Hz). The same
  file also has `CeilingVarJumpGrace = .05f` and `WallSpeedRetentionTime = .06f`.
- **Sources:**
  - Maddy Thorson, "Celeste & Forgiveness", https://maddymakesgames.com/articles/celeste_and_forgiveness/ (opened).
  - Celeste `Player.cs`, https://raw.githubusercontent.com/NoelFB/Celeste/master/Source/Player/Player.cs (opened; I read
    the first 100 k characters only).
- **Automated check:** log every jump with `stepsSinceGrounded`. The bot walks off every ledge edge and presses jump at
  N = 1..20 steps after leaving the ground. Pass when every N up to the grace window jumps, and none past it does.
  Also count real coyote jumps per minute in bot runs: a count of zero means the window is too small to matter, or the
  levels never need it.

### F2. Input buffering: a press made just before landing fires on the landing step
- **Principle:** a jump (or dash or other action) pressed shortly before it becomes legal is cached and fires on the
  first legal step.
- **Numbers:**
  - Super Mario buffers the jump for 1-2 frames (2-4 steps at 120 Hz).
  - Braid buffers it for 0.23 s (about 28 steps).
  - Celeste buffers it too, but the length is defined in input code that I did not find. Not checked.
- **Sources:**
  - Pichlmair & Johansen, "Designing Game Feel: A Survey" (arXiv 2011.09201), https://ar5iv.arxiv.org/html/2011.09201
    (opened), section "button caching".
  - Thorson, F1 (opened).
- **Automated check:** a golden test feeds jump presses at landing − k steps, for k = 0..30, and asserts which ones
  fire. In bot runs, log "pressed but dropped" inputs (a press that produced no action within the window). A non-zero
  dropped-press rate on a pad is a direct measure of "the game ate my input".

### F3. Bunny-hop speed retention: no friction on the landing step
- **Principle:** in Quake / Source movement, ground friction runs only while grounded, and there is "a 1-frame window
  where friction is not applied when the player hits the ground". A jump on that frame keeps all horizontal speed.
  - Air acceleration caps only the projection of velocity onto the wish direction, not the total speed. So strafing
    while turning in the air can add speed.
  - Manual bhopping is hard because of that one-frame window. Auto-bhop (jump while held) is offered as the accessible
    version.
- **Numbers:** friction "approximate range 1-5". The article gives no `sv_airaccelerate` value.
- **Source:** Adrian Biagioli, "Bunnyhopping from the Programmer's Perspective",
  https://adrianb.io/2015/02/14/bunnyhop.html (opened).
- **SKYGRIND note:** `Mover.cs` is exact to the web and changes only when a task names it (rule 3). This finding is for
  designing a named task, not for an edit.
- **Automated check:** landing speed retention = horizontal speed 1 step after landing ÷ horizontal speed 1 step before
  it. Log a histogram of it per run, split by "jumped within k steps of landing" or not. A pad-first game wants the
  retention window wider than one step: compare the retention at k = 1, 3, 6 and 12 steps.

### F4. Slide-hop: crouch before landing, jump on contact (Titanfall 2)
- **Principle:** get airborne, press crouch shortly before landing, then jump right on touchdown. This "will maintain
  (and in some cases, increase) the Pilot's forward momentum over tremendous distances". You can turn corners while
  hopping without losing speed.
- **Numbers:** the wiki gives none. A student re-implementation lists a 0.2 s delay before clamping speed on a wall, and
  slide-boost fatigue that resets after 3 s. Those are the project's numbers, not Respawn's (search summary only).
- **Sources:**
  - Titanfall Wiki, "Slide Hop", https://titanfall.wiki.gg/wiki/Slide_Hop (opened).
  - UCSD CSE125 `TitanfallConstants.hpp`, https://cse125.ucsd.edu/2026/cse125g2/docs/TitanfallConstants_8hpp_source.html
    (search summary only).
  - Respawn's tech-test feedback post (redirected; **not opened**).
- **Automated check:** a "chain" metric, counting consecutive landings that kept at least 90% of their speed. Report the
  longest chain and the median chain per run. Then a bot persona that tries to slide-hop the whole level shows whether
  the levels allow chains at all (long flat runs, gentle slopes).

### F5. Slides last about one second on flat ground, and a slide-jump keeps more speed (Apex)
- **Numbers:**
  - On flat ground a sprinting player slides "for about a second before deceleration kicks in". On downhill slopes the
    slide lasts longer, and on steep ones it can accelerate.
  - Base sprint speed is 7.59 m/s.
  - Jumping out of a slide conserves momentum "for longer than jumping out of a sprint".
- **Design note (Respawn):** Apex kept Titanfall 2's knee-slide "so players could keep momentum on downhill terrain".
  Respawn dropped wall-running because of counterplay.
- **Sources:**
  - Apex Legends Wiki, "Sliding", https://apexlegends.wiki.gg/wiki/Sliding (opened).
  - SI, "Apex Legends making", https://videogames.si.com/features/apex-legends-making (search summary only).
- **Automated check:** per slide, log the steps until speed falls below sprint speed, on flat and on slopes. Compare the
  speed 0.5 s after a slide-jump with the speed 0.5 s after a sprint-jump: the slide-jump should be higher.

### F6. Restraint makes a movement tool feel smart (Doom Eternal dash)
- **Principle:** the dash has two charges and "regenerates quickly but only when you're on the ground". The meathook
  grabs only demons. "Because we don't give you everything, you make up that difference with skill" (Martin), and "the
  glue of the game is the little decisions" (Stratton).
- **Numbers:** two charges, regen only when grounded. The recharge time is not checked (the DoomWiki page returned 403).
- **Sources:**
  - PCGamesN, "Doom Eternal's restrained game design", https://www.pcgamesn.com/doom-eternal-intelligent-design
    (opened).
  - DoomWiki "Dash", https://doomwiki.org/wiki/Dash (**not opened**, 403).
- **Matches GG's rule:** movement and crowd-control verbs need long cooldowns ("every 15 seconds or even longer").
  Doom's version of restraint is charges plus grounded regen, not a long timer.
- **Automated check:**
  - The share of dashes spent in the air, and how often both charges are used inside 1 s.
  - "Ground touches between dash pairs": if the bot never runs dry, the limit is not shaping any decision.

### F7. Real-time response means under about 100 ms
- **Principle:** Swink defines game feel as "real-time control of virtual objects in a simulated space, with
  interactions emphasised by polish". Real time means a response cycle "of under 100 milliseconds".
- **Numbers:** 100 ms is 12 steps at 120 Hz. The survey names Swink's three thresholds ("impression of motion,
  perceived instantaneous response and continuity of response"), but I did not find their values. Not checked.
- **Sources:**
  - Liz England, review of Swink's *Game Feel*, https://lizengland.com/blog/review-game-feel-by-steve-swink/ (opened).
  - Pichlmair & Johansen (opened).
- **Automated check:** for every verb, measure the steps from the input to the first visible change: camera, weapon
  pose, or a view effect on screen, not the sim state. In the Player, measure the frames from input to the first changed
  pixel, with a capture. Any verb above 12 steps fails.

### F8. First-person camera comfort: camera at the eyes, no walking head bob, a fixed point at the centre (Mirror's Edge)
- **Principle:**
  - DICE removed the head bob of an earlier build, so the game is viewed "from your eyes and not your head".
  - It added a small white centre dot to fight simulation sickness, which players can turn off. The dot comes from
    ballet: dancers "focus on a certain object or spot on the floor" when spinning.
  - DICE also attached the reticle partly to the camera, steered by a distant focal point, because a centred reticle
    moved distractingly once the camera was animated. That point comes from a lead-developer interview (search summary
    only).
- **Numbers:** none given. FOV is not mentioned in the article I opened.
- **Sources:**
  - Engadget, "How Mirror's Edge fights simulation sickness",
    https://www.engadget.com/2008/07/17/how-mirrors-edge-fights-simulation-sickness/ (opened).
  - GDC Vault, "Creating First Person Movement for Mirror's Edge", https://gdcvault.com/play/1012171/ (**not opened**,
    login wall).
  - John Nesky, "50 Game Camera Mistakes" (GDC 2014; head bob is item #44), https://gdcvault.com/play/1020460/50-Camera
    (**not opened**).
- **Automated check:** log the camera's roll, pitch and position offsets that are not caused by input, per step. Report
  the RMS and the peak of that "camera motion the player didn't ask for" per run, and per verb (landing, slide,
  wall-ride). Pair it with GG's blind A/B (L-001).

### F9. FOV kick and camera tilt: no primary numbers found, and players turn them off
- **What I found:**
  - Apex has a separate "FOV Ability Scaling" setting that widens the view during movement abilities. Community guides
    recommend turning it off, because "no use for your screen to warp" (search summary only).
  - ULTRAKILL's options have a Camera Tilt toggle (on by default) and a Screenshake slider from 0 to 200%, defaulting to
    100% (search summary only).
  - I found **no primary source with numbers** for an FOV kick per m/s, a wall-run roll in degrees, or a landing dip.
    Not checked.
- **Sources:**
  - EA Help, "Best settings for Apex Legends on PC", https://help.ea.com/en/articles/apex-legends/best-settings-pc
    (search summary only).
  - ULTRAKILL wiki, Options, https://ultrakill.wiki.gg/wiki/HUD (search summary only).
- **SKYGRIND note:** screen-border speed lines are retired. Any speed FOV or tilt should be a config number with a
  settings slider, and judged blind (L-001).
- **Automated check:** log the FOV and the roll per step against speed. Assert that each effect is zero when its setting
  is zero (a comfort-mode test).

---

## 2. Combat feel

### F10. Hitstop scales with damage and has a cap (Smash)
- **Principle:** both attacker and victim freeze for the same time, and the freeze grows with damage.
- **Numbers (SSBWiki, at 60 fps):**
  - Ultimate's formula is ⌊(d·0.65 + 6)·h·e·s⌋ frames. A 15% hit is 15 frames: 250 ms, or 30 steps at 120 Hz.
  - The cap is 30 frames from Brawl on: 500 ms, or 60 steps. Melee capped at 20.
  - Electric hits get ×1.5.
- **Source:** SSBWiki, "Hitlag", https://www.ssbwiki.com/Hitlag (opened). The formulas are community-derived, not from
  Nintendo.
- **Automated check:** log every hitstop as (cause, damage, steps). Assert that the steps grow with damage, never pass
  the cap, and that two hitstops in one step don't stack past the cap.

### F11. How to make hitstop read (Sakurai)
- **Principle:**
  - The victim vibrates visually, but "keep the hurtbox locations static", so hits still register the same way.
  - Grounded victims shake side to side, airborne victims up and down, so feet don't clip into the floor.
  - The shake starts large and shrinks over the freeze, and is scaled to camera distance.
  - The attacker drifts slightly during the freeze, so a swing doesn't look frozen.
  - Sakurai keeps hitstop short in free-for-alls, because a frozen pair is a free target for a third player. With a
    horde, that maps to the player being frozen while other enemies keep acting.
  - The first flinch frame blends into the hurt pose over 4 hitstop frames.
- **Source:** Source Gaming's translation of Sakurai's Famitsu column vol. 490,
  https://sourcegaming.info/2015/11/11/thoughts-on-hitstop-sakurais-famitsu-column-vol-490-1/ (opened).
- **Automated check:** during hitstop, assert that the colliders (CoreGrid / box layers) haven't moved while the view
  mesh has. Log the damage the player takes from enemies that are not frozen during a player-side hitstop. That number
  should be near zero, or the freeze is costing the player health.

### F12. Every shot is a stack of small feedbacks (Nuclear Throne pistol)
- **Principle:** Jan Willem Nijman's breakdown of one pistol shot:
  - a sound;
  - a shell ejected 100-150° off the aim;
  - the bullet's first frame drawn as a circle (a fake muzzle flash);
  - a camera kick back 6 px from the aim point, and +4 screenshake that decays fast;
  - a weapon-sprite kick of 2 px that snaps back;
  - an impact effect;
  - on an enemy hit: a material sound, a 3 px/frame knockback, a get-hit animation (one white frame, then two frames of
    "big eyes"), and a freeze of about 10-20 ms.
- **Numbers:** a 10-20 ms freeze per hit is 1-2 steps. That is the small end, next to Smash's 250 ms (F10).
- **Sources:**
  - Infovore, "Making Game Feel" (quotes Nijman's Rock Paper Shotgun interview), https://infovore.org/?p=5275 (opened).
  - Nijman's talk "The Art of Screenshake" (**not opened**; I did not find the video page).
- **SKYGRIND note:** the sound layer is GG's. Every visual layer can be built and checked without audio.
- **Automated check:** a per-shot "feedback layer count": the number of distinct view channels that fire within 2 steps
  of a shot or hit (camera kick, weapon pose, flash, decal, enemy flash, enemy knockback, hitstop). Pass when each
  weapon's hit fires at least N channels. Capture a before/after frame pair as L-014's Player proof line.

### F13. Screen shake: trauma, squared or cubed, smooth noise, rotation only in 3D
- **Principle (Squirrel Eiserloh, GDC 2016):**
  - Keep a trauma value in [0, 1]. Damage and stress add to it (+0.2 or +0.5), and it falls linearly over time.
  - Shake = trauma² or trauma³. Trauma 0.30 / 0.60 / 0.90 gives 3% / 22% / 73% shake.
  - Use Perlin (smoothed) noise, not random values: it "works with pause and slow-motion" and is "more easily
    reproducible on replay".
  - **In 3D, use rotational shake only.** His slide marks translational shake in 3D as "VERY BAD".
  - Camera following: an asymptotic average, with weight 0.01 (slow), 0.1 (fast) or 0.5 (very fast) at 60 fps. At
    120 Hz a weight w becomes 1 − √(1 − w).
  - Up and down can use different weights.
- **Source:** Eiserloh, "Math for Game Programmers: Juicing Your Cameras With Math" (GDC 2016), slides
  http://www.mathforgameprogrammers.com/gdc2016/GDC2016_Eiserloh_Squirrel_JuicingYourCameras.pdf (opened, 62 pages).
- **SKYGRIND fit:** noise seeded per run fits rule 6. Camera effects are view-only, so the sim stays exact.
- **Automated check:**
  - Log trauma per step. Assert it is a pure function of sim events (a replay gives the same trauma curve).
  - Assert there is no camera translation from shake.
  - Report the time spent above trauma 0.5 per minute. Shake is "like salt", so a high value means the shake is
    constant and stops reading as feedback.

### F14. Recoil: the camera kicks up, and the horizontal part recovers faster than the vertical part
- **Principle (The Astronauts, Witchfire):**
  - Recoil kicks the camera upward for two reasons: realism, and so that "the speed and precision of repositioning the
    crosshair back onto the enemy" stays a skill.
  - "The gun resets horizontally a bit faster than vertically", so "the last split second of the recoil is purely
    vertical".
  - A small camera shake fires on each shot.
  - Muzzle smoke is shortened for play. Impact smoke lingers longer and is denser than muzzle smoke, so the two read
    apart.
  - Every ADS blur, vignette and depth-of-field effect can be turned off.
- **Numbers:** none given.
- **Source:** The Astronauts, "Making Gun Come to Life", https://www.theastronauts.com/2019/03/making-gun-come-to-life/
  (opened).
- **Automated check:** log the recoil pitch and yaw offsets per step after each shot. Assert that yaw reaches zero
  before pitch does, and that both return within the weapon's config window. On a pad, the bot's tracking error after a
  burst measures whether the recoil is fightable.

### F15. A taxonomy to audit against
- **Principle:** the survey sorts game-feel techniques into three domains: physicality (tuning), amplification (juice)
  and support (streamlining).
  - **Movement and actions:** gravity, terminal velocity, coyote time, i-frames, corner correction, button caching,
    assisted aiming.
  - **Event signification:** screen shake, knock-back / recoil, one-shot particles, cooldown visualisation, ragdoll,
    colour flash, impact markers, hit stop, audio, haptics.
  - **Time:** freeze frames, slow motion, bullet time, instant replay.
  - **Persistence:** trails, decals and debris, follow-through.
  - **Scene framing:** points of interest, a dynamic camera.
- **Numbers:** gravity in Super Meat Boy is about 41 m/s², and in Super Mario Bros. 91.28 m/s², against Earth's 9.807.
  Celeste's player controller is about 5,400 lines, "much of it devoted to player forgiveness".
- **Source:** Pichlmair & Johansen, "Designing Game Feel: A Survey", https://ar5iv.arxiv.org/html/2011.09201 (opened;
  the first 100 k of 116 k characters read).
- **Automated check:** a per-verb checklist table (verb × technique). In VERBS.md style, a cell is filled only when a
  Player proof line exists for it.

### F16. Enemy pain reactions escalate and change by weapon (Doom 2016)
- **Principle:**
  - Sustained damage escalates the reaction: a twitch, then a falter, then a stagger, which opens a glory kill.
  - The reactions differ by weapon: imps stop moving when hit by the assault rifle, and zombies stagger fast under the
    shotgun.
  - Glory kills make the player invulnerable while they play, and drop health (more from bigger demons). Chainsaw kills
    drop ammo.
  - An enemy may not start a new attack until its current one has finished.
- **Source:** Game Developer, "Cyber Demons: The AI of DOOM (2016)",
  https://www.gamedeveloper.com/design/cyber-demons-the-ai-of-doom-2016- (opened).
- **SKYGRIND note:** enemy design is GG's (rule 7), so this informs the reaction layer on the existing blocky models,
  not new enemies.
- **Automated check:** log a reaction per hit (none / twitch / falter / stagger). Report the share of hits with no
  visible reaction per enemy type. A high share means hits don't land visually ("kills not crunchy", from GG's NIGHT16
  notes in memory).

### F17. Enemies are chess pieces with readable weak points (Doom Eternal)
- **Principle:**
  - Threats are ranked by weak spots and counters. Shoot the Arachnotron's turret and it explodes. Hit a Mancubus arm
    and it explodes and staggers. Dodge the Pink Demon's charge, then shoot its tail. A grenade in a Cacodemon's mouth
    makes it flash, "inviting players to perform a Glory Kill".
  - Enemies are tuned so you "can't simply rush up and spam" the super shotgun.
  - The Mancubus punishes players who enter its personal space.
  - Martin calls the result "speed chess".
- **Sources:**
  - Shacknews, "Hell-Razer: The Making of Doom Eternal", page 4,
    https://www.shacknews.com/article/116635/hell-razer-the-making-of-doom-eternal?page=4 (opened).
  - Game Developer, "The aggressive resource management of Doom Eternal",
    https://gamedeveloper.com/design/the-aggressive-resource-management-of-i-doom-eternal-i- (opened).
- **Automated check:**
  - For each enemy type, log the share of kills that used its intended counter (weak point, parry, specific weapon).
  - Also log the weapon-switch rate per minute. If one weapon takes more than 70% of kills across all types, the chess
    is not happening. (70% is my threshold, not the source's.)

### F18. Pad aim assist has two parts: friction (slowdown) and magnetism (pull), both per weapon (Halo)
- **Principle:**
  - Halo's "magnetism", also called friction, is controller-only. It slows turning while an enemy is inside a radius
    around the reticle, and it partly tracks a strafing enemy by moving the camera.
  - Each weapon has its own radius, which shrinks when zoomed in line with the magnification.
  - The strength falls with distance and stops past a per-weapon range.
  - Some games use an outer radius with a weaker pull that ramps up to an inner radius.
  - Auto-aim (community name: "bullet magnetism") bends the shot toward the target inside the "red reticle range".
- **Numbers:** none published.
- **Sources:**
  - Halopedia, "Magnetism", https://halopedia.org/Magnetism (opened).
  - Halopedia, "Aim assist", https://halopedia.org/Aim-Assist (search summary only).
  - Nick Weihs (Insomniac), "Techniques for Building Aim Assist in Console Shooters" (GDC 2013), covering camera
    acceleration, deadzoning, magnetism, centering and friction:
    https://www.gdcvault.com/play/1017942/Techniques-for-Building-Aim-Assist (session page opened; the talk itself
    **not opened**).
- **Automated check:** a bot with a human-like pad model (stick noise, reaction delay) tracks a strafing target. Measure
  the time-on-target share and the time to acquire, with assist off, at default, and at maximum, per weapon. Assist is
  doing its job when the gap between bot skill levels narrows (F20).

### F19. Published aim-assist numbers from shipped games
- **The Finals, patch 1.4.1:**
  - "Zoom Snapping Time" cut from 0.3 s to 0.25 s, and its angular velocity capped to stop "rapid 90-degree turns".
  - "Camera Magnetism" cut from 50% to 35%.
  - Zoom snapping removed from the sniper, the revolver, the LH1 and all shotguns.
  - Source: Gfinity, https://www.gfinityesports.com/news/the-finals-aim-assist-nerfed (opened).
- **Apex Legends:**
  - Datamined strength is 0.6 on console and 0.4 for a controller on PC. Season 22 cut the PC controller strength by
    25% (reported as 0.4 to 0.3), and console crossplay into PC lobbies by 18-22%.
  - Sources: Upcomer, https://upcomer.com/apex-legends-aim-assist-for-controller-is-supposedly-stronger-on-consoles-than-pc ;
    esports.gg, https://esports.gg/news/apex-legends/apex-legends-aim-assist-getting-nerfed-on-pc-in-season-22 ;
    Beebom, https://beebom.com/apex-legends-aim-assist-nerfed/amp/ (all search summary only).
- **Overwatch console, patch 1.28:**
  - The old system "would decrease your controller's sensitivity input". The new "Ease In" runs from 0 to 1: 0 gives
    full strength at once, 1 a linear ramp, and the default 0.5 a curved ramp "that we've found works best with the
    widest range of heroes".
  - Source: Blizzard forums, https://us.forums.blizzard.com/en/overwatch/t/console-aim-assist-changes-patch-1-28/208185
    (opened).
- **Use:** these are starting bands for a config row (slowdown of about 35-50%, snap of 0.25 s or less, an ease-in
  curve), not values to copy.
- **Automated check:**
  - Assert that the assist never turns the camera faster than a capped angular velocity, with a unit test on the assist
    function.
  - Log the "snap" degrees per ADS press. A 90° snap is the exploit The Finals removed.

### F20. Aim assist done well narrows skill gaps without being noticed, and does not hurt learning
- **Findings across studies:**
  - Bullet magnetism and area cursor were the most effective techniques, and players favoured bullet magnetism.
  - Area cursor and target gravity "substantially reduced score differences between high- and low-skilled players".
  - Low to moderate sticky targets can go unnoticed.
  - Sticky targets have an "overshooting" problem: when the stickiness ends on leaving a target, aim jumps ahead.
  - Gutwin et al. found "no significant differences in performance gains between groups after assistance was removed".
  - Research implementations use an area-cursor radius of 10 px + 5 px × Level and a magnetism range of
    160 units × Level, for levels 1 to 10.
- **Source:** Kostic et al., "Aim Assistance in Video Games: A Review of Techniques for Accessibility and Player Support"
  (BISEC 2025), https://bisec.metropolitan.ac.rs/wp-content/uploads/2026/04/Rad-19-Aim-Assistance-in-Video-Games.pdf
  (opened).
- **Related:** a community Mycopunk mod fades the slowdown back in smoothly off-target, to avoid the snap
  (https://thunderstore.io/c/mycopunk/p/Coruscnium/AimAssist/v/1.0.1/, search summary only).
- **Automated check:**
  - Run two bot skill presets (aim noise high / low) with assist off and on. The assist "works" when the gap in
    kills-per-minute between them shrinks.
  - Log the stick-to-camera ratio on the step the reticle leaves a target. A spike there is the overshoot.

### F21. Visual sound cues: a cue channel that needs no audio work
- **Principle:** Fortnite's "Visualize Sound Effects" draws directional rings and icons for footsteps, gunfire and
  chests. Deaf reviewers rate it nearly ideal, and Epic turned it on by default for everyone. Hearing players use it
  too, which is a sign that directional threat cues help everyone.
- **Sources:**
  - Can I Play That, deaf review of Fortnite, https://caniplaythat.com/?p=2292 (search summary only).
  - GameSpot, https://www.gamespot.com/articles/how-to-use-visualize-audio-effects-in-fortnite/1100-6498314/ (search
    summary only).
- **SKYGRIND fit:** GG owns sound, but an off-screen threat indicator driven by the web's gameplay event names is UI.
  Event names stay unchanged under rule 7.
- **Automated check:** for every enemy attack that starts behind the player, log whether a cue was shown and how many
  steps before impact. Report "hits from unseen and uncued attackers" per run. Near zero is the goal.

### F22. Style meter: reward variety and punish getting hit (ULTRAKILL)
- **Numbers (ULTRAKILL wiki, Style):**
  - Ranks run from Destructive 200 to Chaotic 300, Brutal 400, Anarchic 500, Supreme 700, SSadistic 850,
    SSShitstorm 1000 and ULTRAKILL 1500.
  - Passive decay runs from 15/s (Destructive) to 120/s (ULTRAKILL).
  - Freshness per weapon variant: FRESH (7.5-10) ×1.5, USED (5-7.5) ×1.0, STALE (1-5) ×0.5, DULL (0-1) ×0.0. Each use
    costs that variant 0.7 freshness and gives +0.5 to the other weapons. Weapons not held regain 0.1/s.
  - Taking damage drops the meter one whole rank.
- **Healing (ULTRAKILL wiki, Health):**
  - Health comes from hitting enemies up close (punch 10-30, hitscan 10-30, parry 999).
  - "Hard damage" is 35% of each hit and lowers the health ceiling. It starts recovering after a 1 s timer, plus
    damage ÷ 20 s, and then drains back at 15/s. Higher style ranks speed up the recovery.
- **Sources:**
  - ULTRAKILL wiki, "Style", https://ultrakill.wiki.gg/wiki/Style (opened).
  - ULTRAKILL wiki, "Health", https://ultrakill.wiki.gg/wiki/Health (opened).
- **SKYGRIND note:** passive in-round regen is banned by GG. ULTRAKILL's healing is earned by aggression, not passive,
  but treat any healing row as a GG decision.
- **Automated check:** if SKYGRIND has or gets a style or variety meter, log per run:
  - the time-weighted mean rank;
  - the share of time at the top two ranks;
  - each weapon's share of style points.
  Freshness should push the bot (and players) to switch: the switch rate goes up when the row is ON.

---

## 3. Pacing and flow

### F23. Push-forward combat: resources come from aggression (Doom 2016 and Eternal)
- **Principle:**
  - "The solution to all the problems is to be aggressive." Passive play is punished, because the AI moves in.
  - Doom Eternal's loop: glory kill for health, flame belch for armour, chainsaw for ammo. "You just take what you need
    when you need it."
  - Doom 2016 dropped regenerating health, reloads and cover.
- **Sources:**
  - Game Developer, "The aggressive resource management of Doom Eternal" (opened).
  - GDC 2018, Loudy & Campbell, "Embracing Push Forward Combat in DOOM",
    https://gdcvault.com/play/1024940/Embracing-Push-Forward-Combat-in (session listing seen; talk **not opened**).
- **Automated check:**
  - Resource intake by source per minute (kills / melee / pickups / round-end). Push-forward holds when most intake
    comes from close actions.
  - "Time camping": the share of time inside a small radius with no enemy closer than X. Pair it with the damage taken
    while camping. If camping is safe, the design isn't pushing.

### F24. Limit how many enemies attack at once (Doom 2016 attack tokens)
- **Principle:**
  - Each attack type (melee, ranged, charged and so on) has a pool of tokens. A demon must take a token before it
    attacks and give it back after. "Each difficulty level has a different set of token counts for each attack type",
    and demons can steal tokens from each other.
  - Demon accuracy drops as the player moves faster, and higher difficulties compensate more for player movement.
  - Ranged enemies hold their distance. Cover is inverted: AI picks exposed spots near cover that stay visible to the
    player.
- **Numbers:** the article gives no token counts.
- **Source:** Game Developer, "Cyber Demons: The AI of DOOM (2016)" (opened).
- **Why it matters here:** this is how a horde stays readable. Many enemies on screen, few attacking at once, and speed
  rewarded with fewer hits.
- **Automated check:**
  - Log the attackers per step (enemies in an attack state aimed at the player). Report the max, the p95 and the
    histogram per round.
  - Log the hit rate against player speed buckets. Enemy hits should fall as speed rises: that is "movement is
    defence", checked in numbers.

### F25. Intensity curve: build up, sustain the peak 3-5 s, fade, relax 30-45 s (Left 4 Dead's Director)
- **Principle:**
  - "Constant, unchanging combat is fatiguing. Long periods of inactivity are boring." The Director estimates each
    survivor's intensity and tracks the maximum.
  - Intensity rises with damage taken (in proportion), incapacitation and ledge pulls, and when a nearby infected dies
    (inversely to distance). It decays toward zero, but not while infected are engaging.
  - The four phases:
    - **Build Up:** full threat until intensity crosses the peak threshold.
    - **Sustain Peak:** keep the full threat for **3-5 s** more.
    - **Peak Fade:** minimal threat until intensity decays, and the relax period waits "until a natural break in the
      action occurs".
    - **Relax:** minimal threat for **30-45 s**, or until the team has travelled far enough.
  - Bosses are not affected by this pacing.
  - Mobs come every 90-180 s on Normal, and 75% of mobs come from behind.
- **Source:** Michael Booth, "The AI Systems of Left 4 Dead" (Valve, AIIDE 2009), slides
  https://steamcdn-a.akamaihd.net/apps/valve/2009/ai_systems_of_l4d_mike_booth.pdf (opened).
- **Automated check:** this is the most directly portable model. Compute Booth's intensity from sim events per step
  (damage taken, near kills by distance, decay), and plot it over each round. Report:
  - the number of peaks;
  - the share of time in relax;
  - the longest stretch with no peak;
  - the longest stretch above the peak threshold.
  A wave that is flat-high is fatiguing. A wave with relax under about 10 s between peaks never lets the player breathe.

### F26. Loops inside loops: 3 s inside 30 s inside 3 min
- **Principle:**
  - Griesemer says "30 seconds of fun" is usually read backwards. It was "a 3-second loop inside of a 30-second loop
    inside of a 3-minute loop that is always different".
  - "No 30 second stretch of Halo is ever repeated; the missions are constantly changing the context on you."
  - The 30-second loop is varied by the environment, weapons, vehicles and enemy mixes.
- **Source:** Engadget, "Half-Minute Halo: An Interview with Jaime Griesemer",
  https://www.engadget.com/2011-07-14-half-minute-halo-an-interview-with-jaime-griesemer.html (opened).
- **Automated check:** cut each run into 30 s windows. For each window, log a context vector: the enemy-type mix, the
  room id, the weapon used, and the verbs used. Report the share of windows that repeat the window before them
  exactly. A high repeat share is "samey" in numbers.

### F27. CoD Zombies round scaling (fan-measured)
- **Numbers:**
  - Zombie health starts at 150 and gains +100 per round up to 950 at round 9-10, then ×1.1 per round (about 7,030 at
    round 30).
  - Zombies per round fit a quadratic, about 0.0842·R² + 0.1954·R + 22.05 (solo, accurate after round 10).
  - At most **24 zombies on the map** at once (World at War / Black Ops era). New ones spawn as others die.
  - These come from fan observation, not the game files.
- **Sources:**
  - callofdutyzombies.com forum threads, e.g. https://www.callofdutyzombies.com/topic/145097-2-players-6-more-zombies-30-or-not
    and https://callofdutyzombies.com/topic/144013-how-to-be-a-good-zombies-player (search summary only).
  - The CoD wiki's round page was **not opened**. The between-round delay and the spawn-delay formula are not checked.
- **Automated check:** log the alive-count per step and the total spawned per round. Assert the on-map cap from config,
  and report the steps with fewer than 3 enemies alive mid-round (a "dead air" proxy).

### F28. Flow: keep players in their own flow zone, and put choices inside the core action
- **Principle (Jenova Chen, "Flow in Games (and Everything Else)", CACM 50(4), 2007):**
  - Too much challenge brings anxiety, too little brings boredom, and hardcore and novice players have different flow
    zones.
  - Too many explicit choices break flow.
  - Chen's four steps: "Mix and match the components of Flow; keep the user's experience within the user's Flow Zone;
    offer adaptive choices, allowing different users to enjoy the Flow in their own way; and embed choices inside the
    core activities to ensure the Flow is never interrupted."
  - flOw did this by letting players dive between planes at will. The difficulty changed by player choice, below
    conscious thought.
- **Sources:**
  - Chen's paper PDF, https://khoury.northeastern.edu/~lieber/courses/csu670/f08/materials/p31-chen-flow-in-games.pdf
    (opened).
  - Wikipedia, "Flow (video game)", https://en.wikipedia.org/wiki/Flow_(video_game) (search summary only).
- **SKYGRIND fit:** a shop or menu that pauses the action between rounds is the "choice outside the core activity" that
  Chen warns about. Buys done on the move keep flow.
- **Automated check:**
  - Log the steps the player spends in any non-combat UI per round (menus, shops, pauses).
  - Log the optional difficulty choices made in-world (risk routes, elite pulls) per round. Chen's model wants the
    second high and the first low.

### F29. Valve: designs are hypotheses, playtests are experiments
- **Principle:**
  - "Game designs are hypotheses. Playtests are experiments. Evaluate designs off playtest results. Repeat."
  - Stat collection records deaths, level times and friendly fire, and is shown as heatmaps (TF2 Dustbowl death
    concentrations).
  - The warnings: "Averages hide extreme examples", stats "miss nuance (lacking context)", and you "can see 'illusory'
    patterns".
  - Biometrics (heart rate, skin conductance, eye tracking, EEG) are listed as arousal proxies.
- **Source:** Mike Ambinder, "Valve's Approach to Playtesting" (GDC 2009),
  https://cdn.fastly.steamstatic.com/apps/valve/2009/GDC2009_ValvesApproachToPlaytesting.pdf (opened).
- **Related, press only:** Valve's biofeedback build of L4D2 changed the Director's spawns by player arousal, and its
  post-playtest surveys rated it "twice as enjoyable". Source: PC Gamer / MCV coverage, e.g.
  https://mcvuk.com/?p=114442 (search summary only).
- **Automated check:** every feel row gets a stated hypothesis and a predicted metric change before it is built
  (L-001). Report the median *and* the p10 / p90 of every metric, never only the mean.

---

## 4. Measurable proxies of feel and flow (telemetry and bots)

### F30. A frustration detector from 1 s telemetry (IO Interactive, Kane & Lynch)
- **Principle:**
  - IO sampled every 1 s: position, avatar vector, camera vector, health, movement mode, crouch, and in-cover.
  - Frustrated players showed five signs together:
    1. repeated deaths in the same spot, or less progress each attempt;
    2. fewer kills each attempt;
    3. a faster pace, taking the same route each time;
    4. little or no use of abilities, pickups or the environment;
    5. the camera vector lining up with the movement direction ("the less interest in examining the environment").
  - The model fired when ≥ 2 deaths happened within 20 world units inside the window, the pace was above the session
    mean, and kills and pickups fell between deaths. "Frustration is not indicated by any single behavioral variable."
  - 6 of 22 testers matched, and all 6 confirmed in interviews that they had been frustrated in the flagged segments.
- **Source:** Drachen, Canossa & Sørensen, "Gameplay Metrics in Game User Research: Examples from the Trenches", ch. 14
  of *Game Analytics* (Springer 2013), https://cmps-people.ok.ubc.ca/bowenhui/game/readings/gameAnalyticsCh14.pdf
  (opened).
- **Automated check:** this can be implemented directly. Sample the sim every 120 steps, and flag windows that meet all
  the conditions. A bot can't feel frustrated, but the same pattern in bot runs (dying in one spot, taking the same
  route faster) marks a level spot that would frustrate people.

### F31. Death and kill heatmaps find the bad spots
- **Principle:**
  - Bungie published Halo 3 heatmaps of kills and deaths per map and per weapon in November 2007, its "Doppler Radar
    System of Death". They showed hot spots such as sniper perches on Guardian.
  - IO found that 119 of 253 deaths in one Kane & Lynch 2 session happened in a spawn area, a warning sign for map
    flow.
  - Typical map types: death maps, path (traffic) maps, dwell-time maps and drop-off maps.
- **Sources:**
  - Gigazine, https://gigazine.net/gsc_news/en/20071114_halo3_heatmaps/ (opened).
  - GamesRadar, "Where not to stand in Halo 3", https://gamesradar.com/where-not-to-stand-in-halo-3 (search summary
    only).
  - Drachen et al., F30 (opened).
  - Bugnet, "What is a gameplay heatmap", https://bugnet.io/blog/what-is-a-gameplay-heatmap (search summary only).
  - Wired, "Halo 3: How Microsoft Labs Invented a New Science of Play" (**not opened**: the fetch was blocked).
- **Automated check:** bin the bot's position (traffic), damage-taken spots, deaths, kills and "stalled" steps (F32)
  into a 2D grid per level, as PNGs in `Logs/shots/`. Cells nobody visits are dead space. Cells with high dwell and low
  speed are where flow breaks.

### F32. The stall and flow metric set (my synthesis, not from one source)
These are the proxies the sources above point at. Each can be computed from the deterministic sim per step, for a bot or
a recorded human run. The thresholds are starting guesses to set in config, not sourced numbers.

| Metric | Definition | What it flags | Source behind it |
|---|---|---|---|
| Stall share | share of in-combat steps with horizontal speed < 25% of walk speed | standing still and camping | F23, F30 (pace) |
| Longest stall | the longest run of stalled steps | dead ends, confusion | F31 |
| Airtime share | share of steps airborne | how much movement tech gets used | F3, F4 |
| Landing retention | median speed after / before landing | speed lost on landing | F3 |
| Chain length | consecutive landings keeping ≥ 90% of speed | slide-hop flow | F4 |
| Kill gap | median and max steps between kills in a round | dead air vs pressure | F25, F27 |
| Nearest-enemy distance | median and p10 / p90 per round | push-forward or kiting | F23 |
| Attackers at once | max and p95 enemies in an attack state | readable horde | F24 |
| Intensity curve | Booth intensity per step: peaks, relax share | the build-peak-relief shape | F25 |
| Context repeat | share of 30 s windows that repeat the window before | sameness | F26 |
| Dropped presses | inputs that fired no action within the buffer | eaten inputs | F2 |
| Input-to-view latency | steps from input to the first view change, per verb | sluggish verbs | F7 |
| Unseen hits | hits from off-screen attackers with no cue shown | unfair damage | F21 |
| Frustration windows | Drachen's five-sign pattern | hot spots of repeated failure | F30 |
| Weapon share | the most-used weapon's share of kills | "chess" collapsed into one gun | F17, F22 |
| Dead-end count | arrivals at a nav node whose only exit is back the way you came, without a kill | level flow | L4D "flow distance" (F25), F31 |
| Direction changes | heading changes of more than 90° per minute while not fighting | lost or wandering | F30 (route), F31 |

- **Dead ends and progress:** Valve's L4D computes a "flow distance" from the start safe room to every nav area, and
  uses it to place spawns at, behind or ahead of the team (F25, opened). The same field can score progress and dead
  ends per room in SKYGRIND's nodes.
- **Validating the metrics:** bots as "procedural personas" (MCTS agents with evolved utility functions) were found to
  capture human decision styles about as well as clones trained on human traces. That supports running several bot
  personas (aggressive, cautious, mover) instead of one.
  - Holmgård, Togelius et al., "Automated Playtesting with Procedural Personas through MCTS with Evolved
    Heuristics" (IEEE ToG 2019), https://arxiv.org/abs/1907.06570v1 (search summary only).
  - "Personas versus Clones for Player Decision Modeling",
    https://mail.antoniosliapis.com/papers/personas_versus_clones_for_player_decision_modeling.pdf (search summary
    only).
- **Coverage:** EA SEED reports that high state coverage finds more bugs, and that RL agents added to scripted bots
  raised coverage in Battlefield 2042 and Dead Space. Source: EA SEED, "Technical Challenges of Deploying Reinforcement
  Learning Agents for Game Testing in AAA Games" (CoG 2023), https://arxiv.org/abs/2307.11105v1 (search summary only).

---

## Not opened or not found (do not cite these as read)
- Jan Willem Nijman, "The Art of Screenshake": the video page was not found. Only Nijman's Nuclear Throne interview,
  quoted by Infovore, was read (F12).
- Jonasson & Purho, "Juice it or lose it" (Nordic Game Jam 2012): the talk and its list of techniques were not opened.
  The Juicy Break recreation page returned 404.
- Mark Brown (GMTK), "Secrets of Game Feel and Juice": only the Amara description, https://amara.org/v/C3BGA/ , was seen
  in search.
- Steve Swink, *Game Feel* (the book): not opened. Only reviews and the survey were read, so the three thresholds' values
  are not checked.
- GDC talks, all behind the Vault login: Weihs (aim assist), Mirror's Edge movement, Nesky's camera mistakes, and
  Loudy & Campbell (push-forward combat).
- Respawn's Titanfall 2 tech-test post (it redirected), DoomWiki "Dash" (403), Doom Fandom (402), Wired's Halo 3 article
  (blocked), and the CoD Zombies wiki round page.
- Celeste's input buffer length (it is in input code I did not find). ULTRAKILL's parry hitstop of 0.25 s: the wiki page
  failed (Anubis block, then 404), so 0.25 s comes from a search summary only.
- No primary numbers were found for FOV kick per speed, wall-run camera roll, or the landing dip (F9).
- Halo and Destiny aim-assist values: Bungie publishes none that I found.

# Feel research (web): wall ride, charge timing, charge arc, fire cadence

Wave 40 research, 2026-10-13. Web only (WebSearch / WebFetch). Every bullet ends with its URL. "(snippet)" means the
number came from the search engine's summary of that page and the page itself was not opened (it returned 402 / 403);
treat those as less certain. "Not found" means no source was found within the budget. Nothing here was measured in
SKYGRIND.

## 1. Wall-run attach rules

**Shipped games**
- Titanfall (1): base wall-run time 1.75 s; the Enhanced Parkour Kit doubles it to 3.5 s (snippet). Titanfall 2's own
  time limit: not found. https://titanfall.fandom.com/wiki/Enhanced_Parkour_Kit
- Titanfall 2 tech-test changes: Respawn made air speed and wall-run speed faster and made wall runs easier to chain for
  momentum. No numbers given. https://gamingbolt.com/titanfall-2-dev-to-increase-wallrunning-speed-reintroduce-titan-timer
- Apex Legends has no wall running on purpose. Daniel Z. Klein (Respawn): it would make "all the core aspects of Apex
  Legends become irrelevant", and TF2 players "very quickly burned out on" the "constant sugar high".
  https://www.charlieintel.com/apex-legends/respawn-dev-explains-why-apex-legends-will-never-have-wall-running-75098/
- Ghostrunner: the wall run starts automatically when you touch a flat wall while moving forward (snippet). Duration
  limit: not found. https://primagames.com/?p=314223
- ULTRAKILL has a wall cling, not a wall run. It triggers when you hold a movement direction toward a wall while falling.
  Cling "weight" rises the longer you cling, so you fall faster and faster up to the 100 u/s maximum fall speed. It resets
  on touching ground or an Enemy Step. A cling overrides your current fall speed when you first touch (or touch again).
  Wall jumps: at most 3 in the air. Slide 24 u/s; dash 49.5 u/s. https://ultrakill.wiki.gg/wiki/Movement
- ShatterRush: the run attaches automatically "when they are near a wall and not facing directly towards or away from
  it". An idle run falls off after 2 s. Run speed is 58.4 km/h (16.2 m/s). https://shatterrush.wiki.gg/wiki/Movement
- Dying Light 2: Tic Tac (160 stamina) gives a short wall run; Wall Combo (260) chains runs; Dart extends the run. Seconds:
  not found. https://upcomer.com/how-to-wall-run-in-dying-light-2-stay-human
- Mirror's Edge, Neon White, Severed Steel, Karlson: no duration, gravity or attach rules found. (No Neon White wall-run
  card was found either.) https://kb.speeddemosarchive.com/Mirror%27s_Edge

**Open-source and tutorial code**
- CSE125 2026 "Titanfall Movement System" (a student project): "Titanfall-inspired" constants in Quake units. It does
  not claim to have ripped them from TF2. https://cse125.ucsd.edu/2026/cse125g2/docs/TitanfallConstants_8hpp.html
  - Enter needs ground clearance of at least 50 u, horizontal speed of at least 200 u/s, and intent
    dot(wishDir, -wallNormal) of at least 0.1.
  - Stay needs dot of at least -0.26 and horizontal speed of at least 100 u/s.
  - Probe: a sphere cast of 35 u with radius 12 u.
  - Grip: 1.0 s of zero gravity, then gravity ramps from 0 to full over 2.0 s. Vertical speed decays exponentially with
    tau 0.45 s. Forced detach after 5.0 s on the same wall.
  - An "exiting wall" flag lasts 0.2 s. A blacklisted wall can be re-attached only at an inward speed of at least 100 u/s.
  - Camera roll is 7.5 deg, with a tilt interpolation speed of 10.
  - On entry: an upward kick of 440 u/s (ceiling 640). The face normal may rotate at most 90 deg per tick.
- Jeh3no Godot Advanced FP Controller (MIT):
  - Defaults: wallrun_time 3.5 s, infinite_wallrun_time = false, wallrun_fall_gravity_multiplier 0.006,
    time_bef_can_wallrun_again 0.2 s, wallrun_speed 18.
  - Exits: releasing forward input ("can_wallrun = false"), a floor raycast hit, losing both side rays, or the timer
    running out. https://raw.githubusercontent.com/Jeh3no/Godot-Advanced-State-Machine-First-Person-Controller/main/addons/JehenoAdvancedFirstPersonController/PlayerCharacter/StateMachine/player_character_script.gd
  - Exit code: https://raw.githubusercontent.com/Jeh3no/Godot-Advanced-State-Machine-First-Person-Controller/main/addons/JehenoAdvancedFirstPersonController/PlayerCharacter/StateMachine/wallrun_state_script.gd
- Karlson-movement wall-run fork (Dani's tutorial controller). No licence is shown.
  https://raw.githubusercontent.com/Legendized/Karlson-Movement-With-Wallrunning/main/PlayerMovement.cs
  - A surface counts as a wall only if |90 deg - angle(up, normal)| < 0.05 deg, so only exactly vertical surfaces.
  - While running it pushes up against gravity at 0.4 x g x wallRunGravity (default 1).
  - The run cancels 0.2 s after leaving the wall, and the player can run again after 0.1 s.
  - Camera roll is 10 deg via SmoothDamp. Entry impulse 20; escape force 600.
- The common Unity tutorial pattern: `AboveGround()` is a downward raycast of length minJumpHeight that must miss before
  a run can start, and wallCheckDistance rays look left and right.
  https://discussions.unity.com/t/how-to-make-the-first-person-wallrun-automatically-turn-on-curved-surfaces/1679362
  (snippet; the Medium tutorial with numbers returned 403: https://dude123code.medium.com/finally-a-good-wall-run-in-unity-4de42bcb7289)

**Which conditions stop accidental attaches on stairs and ledges** (my synthesis of the code above, not a sourced claim)
- Ground clearance (a down-ray of 50 u, or minJumpHeight, must miss): a step or a stair riser never qualifies.
- Minimum horizontal speed (200 u/s to enter): slowly climbing onto a platform does not attach.
- A near-vertical normal (Karlson within 0.05 deg; TMS only "side walls"): a stair's top faces and slopes are excluded.
- Input intent toward the wall (dot of at least 0.1) plus a detach threshold (-0.26): hugging a ledge wall while moving
  away does not attach.
- Re-attach lockout (0.2 s exit flag; a same-wall blacklist unless inward speed is at least 100 u/s): stops re-snapping
  after a jump.
- A "minimum wall height / length" check: not found in any source.

## 2. Infinite vs timed ride; gravity curve; camera

- Timed rides, with numbers:
  - TF1: 1.75 s (snippet). https://titanfall.fandom.com/wiki/Enhanced_Parkour_Kit
  - ShatterRush: 2 s idle. https://shatterrush.wiki.gg/wiki/Movement
  - Jeh3no: 3.5 s by default. https://raw.githubusercontent.com/Jeh3no/Godot-Advanced-State-Machine-First-Person-Controller/main/addons/JehenoAdvancedFirstPersonController/PlayerCharacter/StateMachine/player_character_script.gd
  - TMS: 5.0 s forced kickoff. https://cse125.ucsd.edu/2026/cse125g2/docs/TitanfallConstants_8hpp.html
- Infinite as an option: Jeh3no ships an `infinite_wallrun_time` toggle. When it is on, the run ends only on wall loss,
  floor contact or releasing forward. So "the ride ends when the wall ends or you stop holding forward" is the guard.
  https://raw.githubusercontent.com/Jeh3no/Godot-Advanced-State-Machine-First-Person-Controller/main/addons/JehenoAdvancedFirstPersonController/PlayerCharacter/StateMachine/wallrun_state_script.gd
- Speed as a guard: TMS drops the run below 100 u/s horizontal speed.
  https://cse125.ucsd.edu/2026/cse125g2/docs/TitanfallConstants_8hpp.html
- A shipped game with a confirmed never-decaying run: not found. (A search summary attributed "infinitely without losing
  height" to Ghostrunner; the source is actually a jam game, so it is discarded.) https://itch.io/post/11010310
- Gravity curves:
  - TMS: 0 g for 1.0 s, then a linear ramp to full g over 2.0 s.
    https://cse125.ucsd.edu/2026/cse125g2/docs/TitanfallConstants_8hpp.html
  - Jeh3no: a constant 0.006 x fall gravity.
    https://raw.githubusercontent.com/Jeh3no/Godot-Advanced-State-Machine-First-Person-Controller/main/addons/JehenoAdvancedFirstPersonController/PlayerCharacter/StateMachine/player_character_script.gd
  - Karlson fork: a constant 0.4 g counter-force.
    https://raw.githubusercontent.com/Legendized/Karlson-Movement-With-Wallrunning/main/PlayerMovement.cs
  - ULTRAKILL cling: fall speed grows the longer you cling, up to 100 u/s. https://ultrakill.wiki.gg/wiki/Movement
- Camera tilt and smoothing:
  - TMS: 7.5 deg, interpolation speed 10 (an exponential lerp, about 0.1 s time constant).
    https://cse125.ucsd.edu/2026/cse125g2/docs/TitanfallConstants_8hpp.html
  - Karlson fork: 10 deg, using SmoothDamp.
    https://raw.githubusercontent.com/Legendized/Karlson-Movement-With-Wallrunning/main/PlayerMovement.cs
  - Titanfall 2's real roll angle: not found. Measured attach / detach lerp times in any shipped game: not found.

## 3. Charge-shot timing

- ULTRAKILL Piercer revolver: the alt-fire charges over 0.5 s. "If released before then, the weapon winds back down to
  its default state without firing": no partial shot. https://ultrakill.wiki.gg/wiki/Revolver
- ULTRAKILL Railcannon: fires at once on swap; all variants share a 16 s cooldown. https://ultrakill.wiki.gg/wiki/Railcannon
- Titanfall charge rifle: fires only at full charge and discharges automatically the moment it reaches full (snippet;
  TF2 is "mostly unchanged"). Charge seconds: not found. https://titanfall.wiki.gg/wiki/Charge_Rifle
- Apex Legends charge rifle: a 0.85 s charge delay after the trigger press (snippet). https://apexlegends.fandom.com/wiki/Charge_Rifle
- Destiny 2 fusion rifles (the Renegades rescale), in ms: Rapid-Fire 500 -> 533, Adaptive / Aggressive 660 -> 667,
  Precision 780 -> 800, High Impact 960 -> 967. https://destinybulletin.com/news/weapon-archetype-changes-in-destiny-2-renegades-fusion-charge-times-shift-shotguns-get-pve-love-snipers-flinch-less-rocket-pulses-lose-reserves/
  - My inference, not a sourced claim: the new values are whole 30 Hz ticks (16, 20, 24 and 29 x 33.3 ms). That is the
    same idea as our "timers in whole steps".
- Splatoon:
  - Splat Charger: 1 s to full charge, or 3 s with an empty tank (charge rate 33.33 %). Partial charge deals 40-100
    damage; full charge deals 160 (snippet). https://splatoonwiki.org/wiki/Splat_Charger
  - E-liter 4K: 1.53 s to full. https://splatoonwiki.org/wiki/E-liter_4K
  - Heavy Splatling: 50 frames to the first charge ring, 75 frames in all (snippet). https://splatoonwiki.org/wiki/Heavy_Splatling
- Doom Eternal Ballista: the Destroyer Blade has a three-stage charge, can fire before full, and costs more ammo at higher
  stages. Rapid Chains cuts its charge time by 20 % (snippet). Seconds: not found. https://doomwiki.org/wiki/Ballista
- Halo CE plasma pistol: an overcharged shot costs about 2 s of overheat (snippet). Overcharge build time: not found.
  https://www.halopedia.org/Plasma_pistol
- Overwatch Widowmaker scoped charge: the snippet was garbled (it mentioned "0.5 s" and "1.08 per percent"). Not used:
  not verified. https://overwatch.fandom.com/wiki/Widowmaker
- Partial-charge rules seen, as a pattern:
  - All or nothing: the Piercer, the Titanfall charge rifle.
  - Scaled damage: the Splatoon chargers (40-100 partial vs 160 full).
  - Discrete stages: the Destroyer Blade (3 stages).

## 4. Charge-indicator UI near the reticle

- Destiny 2 Season 21 (2023): "Fusion Rifle and Sword reticles now both display charge information within their
  respective reticles", to make charge and guard energy easier to gauge. The goal was to stop relying on the weapon's
  audio and visual cues. https://mp1st.com/news/destiny-2-season-21-weapon-changes-detailed-updated-reticles-weapon-rotation-in-inspection-screen-and-more
- Destiny 2: the fusion rifle gets "a charge meter under the reticle"; hand cannons get a perk counter below the central
  reticle (snippets). This is the closest precedent to our "one arc under the reticle".
  https://www.bungie.net/7/en/News/article/season_21_weapon_preview
  https://www.sportskeeda.com/mmo/bungie-introduce-multiple-weapon-reticle-changes-destiny-2-season-deep
- Destiny 2: hip-fire reticles were rebuilt "so that they move more visibly at high FOV". Visibility at high FOV is a
  design constraint for any reticle meter. https://mp1st.com/news/destiny-2-season-21-weapon-changes-detailed-updated-reticles-weapon-rotation-in-inspection-screen-and-more
- ULTRAKILL Railcannon shows its cooldown three ways:
  - a lightning-bolt icon next to the weapon icon;
  - spinning screws on the gun that "reactivate one at a time" as the charge returns;
  - a hum whose pitch rises with the charge level.
  https://ultrakill.wiki.gg/wiki/Railcannon
- Game Creator Shooter docs: a "Radial Reloading Indicator" draws a wheel around the reticle while the weapon reloads
  (snippet). https://docs.gamecreator.io/shooter/ui/
- Progress-bar perception (Harrison et al., UIST 2007):
  - People do not perceive time linearly, and accelerating progress feels faster.
  - A bar that is slow early and fast late ("Fast Power") was preferred.
  - A follow-up with visual illusions made bars feel about 10 % faster (snippet).
  - What this suggests for us (inference): a fill that eases in (slow then fast), or a linear fill with a sharp flash at
    full, rather than an ease-out fill that looks "almost done" too early.
  https://www.chrisharrison.net/index.php/Research/ProgressBars
  https://www.figlab.com/research/2010/faster-progress-bars
- Peacocke, Teather et al. (GEM 2015) compared five ammo-display styles, both diegetic and HUD. Their findings were not
  checked. https://www.csit.carleton.ca/~rteather/pdfs/GEM2015_HUDs.pdf
- Not found with a source: a fill-direction convention, a full-charge flash or pulse duration, a colour change at full,
  and cooldown shown on the same element (beyond the Railcannon's on-gun screws).

## 5. Fire-animation cadence and reticle recovery

- Source SDK (HL2 .357): PrimaryAttack sets `m_flNextPrimaryAttack = curtime + 0.75` and calls
  `SendWeaponAnim(ACT_VM_PRIMARYATTACK)` on every shot. The view punch is (-8 pitch, ±2 yaw). The fire interval is
  hand-set; it is not read from the animation length.
  https://swarm.workshop.perforce.com/files/guest/knut_wikstrom/ValveSDKCodedlls/hl2_dll/weapon_357.cpp
- Source SDK machine gun: `while (m_flNextPrimaryAttack <= curtime) { m_flNextPrimaryAttack += fireRate; bullets++; }`.
  This is an accumulator, so the fire rate holds at low frame rates; the attack animation is re-sent on each fire call.
  https://swarm.workshop.perforce.com/files/guest/knut_wikstrom/ValveSDKCodegame_shared/hl2mp/weapon_hl2mpbase_machinegun.cpp
- Valve wiki: a gun fires while +attack is held and `m_flNextPrimaryAttack <= curtime`; GetFireRate() returns the seconds
  between bullets. https://developer.valvesoftware.com/wiki/Authoring_a_weapon_entity
- Unity forum advice: make the fire animation the same length as the fire delay, so it is back at rest when the next shot
  fires (snippet). https://discussions.unity.com/t/weapon-shoot-animation-speed-up-loop-time/592791
- CS:GO inaccuracy recovery:
  - Formula: newInaccuracy = inaccuracy * e^(-t / (log10(e) * RecoveryTime)), so it decays exponentially.
  - Recovery was later split into initial and final rates, so taps recover faster than sprays.
  - (snippet) https://www.steamanalyst.com/updates/recovery-time
- Recoil recovery as a set fraction of the fire interval: not found. A reticle rotation or spread animation timed to the
  fire rate: not found beyond the CS:GO recovery formula.

## Numbers table

| Item | Value | Source |
|---|---|---|
| TF1 wall-run time | 1.75 s (3.5 s with kit) | fandom (snippet) |
| ShatterRush idle run | 2 s; 16.2 m/s | shatterrush.wiki.gg |
| Jeh3no run time / re-run lockout | 3.5 s / 0.2 s; infinite toggle | GitHub (MIT) |
| TMS grip / gravity ramp / kickoff | 0 g for 1.0 s / 0 -> 1 g over 2.0 s / 5.0 s | cse125 |
| TMS enter: clearance, speed, intent dot | 50 u, 200 u/s, >= 0.1 | cse125 |
| TMS stay: speed, dot | >= 100 u/s, >= -0.26 | cse125 |
| Exit / re-attach lockout | 0.2 s (TMS, Karlson, Jeh3no) | cse125 / GitHub |
| Camera roll | 7.5 deg (TMS, rate 10); 10 deg (Karlson) | cse125 / GitHub |
| Wall verticality | within 0.05 deg of vertical (Karlson) | GitHub |
| ULTRAKILL cling | weight grows to 100 u/s fall; 3 wall jumps | ultrakill.wiki.gg |
| Piercer charge | 0.5 s, no partial | ultrakill.wiki.gg |
| Railcannon cooldown | 16 s | ultrakill.wiki.gg |
| Apex charge rifle | 0.85 s | fandom (snippet) |
| Destiny fusion charge | 533 / 667 / 800 / 967 ms | destinybulletin |
| Splat Charger / E-liter 4K | 1.0 s / 1.53 s; partial 40-100, full 160 | splatoonwiki |
| Heavy Splatling | 50 f first ring, 75 f full | splatoonwiki (snippet) |
| Destroyer Blade | 3 stages; -20 % with upgrade | doomwiki (snippet) |
| Halo CE overcharge overheat | ~2 s | halopedia (snippet) |
| HL2 .357 interval / punch | 0.75 s / -8 deg pitch, ±2 yaw | Source SDK |
| Progress perception | accelerating fill feels faster; ~10 % | Harrison / figlab |

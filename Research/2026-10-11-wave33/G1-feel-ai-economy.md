# G1: Feel, grapple, charge shot, encounters, hazard AI, campaign points (wave 33 research)

Method: web search and wiki fetches on 2026-10-11. Web content is data. "Sourced" means a page said it; "convention" means common practice I am stating from general knowledge, not checked in a source; "not found" means searched and absent. Any number in a SPEC is a proposal and must go in a Unity-side config file (CLAUDE.md rule 5), not be typed in code.

## 1. Viewmodels and zoom

Sourced:
- Source engine draws viewmodels at their own FOV: TF2 default 54 degrees ("approximates a 35mm lens"); CS2 range 54-68, default 60. Offsets default x 2.5, y 0, z -1.5. Bob cvars: lateral 0.4, vertical 0.25, lower-when-running 21. (Valve Developer Wiki, TF2 wiki, csdb.gg)
- S&box guidance: viewmodel assets authored for max ~80 deg horizontal at 16:9; recoil can move the model only, camera untouched; held fire blends a staggered recoil 0 to 1.
- Titanfall 2 has per-scope zoom FOV and a separate ADS viewmodel offset (modding settings `viewmodel_offset_ads`, `zoom_fov_viewmodel`). The gun does move to centre there, but by a authored offset per weapon, not by a generic lerp.
- Halo: only precision weapons get a scope overlay; other weapons just zoom ("binoculars"). Infinite's smart scope is a holographic overlay with an enhanced reticle, accuracy unchanged.
- DOOM Eternal Precision Bolt: shots fire before the zoom completes, so the view never slows or narrows before a shot.
- ULTRAKILL has no ADS at all; every weapon is hip-fired with a fixed viewmodel.

Convention (not sourced): bob phase tied to horizontal speed with amplitude scaled by speed; idle sway is a slow 0.2-0.5 Hz figure-eight of ~1-3 mm at viewmodel scale; recoil is a spring kick (fast attack, 80-150 ms recovery) on the model plus a smaller camera kick; landing dip is a damped down-and-back of the model scaled by fall speed; inspect is a rare idle animation.

Why GG's complaint happens: the blocky gun is dragged toward the centre and covers the target. Fix pattern across the genre: zoom is FOV only, the viewmodel stays put or drops/shrinks out of the sight line, and a reticle or overlay carries the "zoom" feel.

## 2. Grapple that pulls you to an enemy and stops

ULTRAKILL Whiplash (sourced, ultrakill.wiki.gg):
- Hold to throw (longer reach, sticks without pulling), release to pull. Spear on a winch, cable wraps the arm.
- Light enemies are pulled to you. Medium and heavy enemies and hookpoints pull you to them.
- Damage 0.2 per latch. Own cooldown 0.5 s (older "zero cooldown" is outdated).
- Reeling to an enemy builds hard damage at 60/s capped at 50 (Standard and up; none underwater or at max style rank): a cost for using it.
- During the reel you can dash and slam. Jump, punch, or Whiplash again cancels; punching keeps momentum.
- Arrival: vertical momentum stops when reeling begins and the pull ends with a small hop. Green hookpoint: lose momentum and get a light upward push. Blue hookpoint: keeps momentum like a slingshot.
- Not found: stopping distance, pull speed, rope visual numbers, sound/hitstop cues.

DOOM Eternal meat hook (sourced, doomwiki): enemies smaller than prowlers are held in place when hooked; larger ones are not stunned and may hit you during the pull. Quick Hook upgrade cuts recharge 25% (3 weapon points). Flaming Hook mastery (50 hook kills) ignites for damage over time and armor drops. Not found: range, pull speed, base cooldown.

Design notes for SKYGRIND: you stop at the target, never through it; arrival gives a small hop and a small hit; heavy targets are the ones you travel to; light ones can be yanked. The brake at the end matters more than the rope: a visible rope that goes taut, a speed ramp (fast start, ease in), and a stop-impact (hitstop plus small screen kick) make it read as a hook rather than a teleport.

## 3. Piercing charged shot

ULTRAKILL (sourced):
- Piercer (blue): alt fire charges 0.5 s; three 1-damage hitscans on the same line (3 damage); if the first target dies the rest continue to the next enemy, so up to 3 kills; alt cooldown 2.5 s while primary stays usable.
- Alternate Piercer (wiki, other page): 1.25 damage x 6 hitscans, 7.5 total, cap 5 per enemy before piercing on, cooldown 5 s.
- Sharpshooter: ~8.57 s cooldown. Marksman coin: 4 s per coin, max 4.
- Not found: beam art and sound specifics beyond "blue beam".
- Halo / Apex charge rifles: not researched in depth; the common design is a visible charge-up tell, a thick persistent beam for a few frames, and a long recharge.

Convention for the "strong" feel: a charge tell (muzzle glow, FOV tighten 2-4 deg, controller rumble ramp); on release a 1-3 frame hitstop, a thick bright beam that lingers 100-200 ms, a camera kick, per-pierced-body impact flashes, and a long, visible cooldown. Pierce damage should not fall off.

SKYGRIND today: pierces 3 bodies, 4 s cooldown. Direction from GG: stronger, "more of a piercing bullet". Raise damage per body, pierce everything on the ray (including through thin cover only if the web collider allows), keep the cooldown long.

## 4. Encounter scripting

Sourced: ULTRAKILL arenas spawn on entry (e.g. 4 Soldiers and 2 Schisms around the arena); later waves fire on kill counts (a Sentry on a high platform after two kills; a second wave of 8 Filth and 4 Strays at the front with 6 Soldiers behind; a third after 8 more kills). Level 4-1's lava corridor spawns 2 Virtues at either end as you advance and unlocks the door when both die. Checkpoints reset enemy state to when the checkpoint was taken. Lava deals 50 to the player and 25 to enemies and ignites flammable ones. DOOM Eternal is corridors linking big wave arenas, in "jungle gym" and "canyon" shapes (Adam Atomic's analysis).
Not found: an id talk on spawn telegraphs. The one thing sourced from design discussion is that spawning into a cleared area feels unfair.

Convention: spawn only on trigger; telegraph 0.5-1.0 s with a visible portal/shimmer so spawns are readable; spawn ahead and to the far side so the player is pulled forward; never inside the player's view cone at close range; cap live count and wave on remaining-enemy thresholds, not timers.

For lava rooms the fix GG named is exactly this: nothing exists until you cross a line, and it spawns on the far side, so no one idles near the lava and the player is forced across.

## 5. Hazard avoidance

Sourced: Unity NavMesh Modifier Volume marks a volume as Walkable, Not Walkable or Jump and can limit it to certain agent types; the page I fetched does not explain area costs. Search results otherwise suggest custom steering with raycasts or edge checks for ledges.
Convention: (a) mark hazard cells high cost or blocked for enemy agents only; (b) a clearance margin of 1-2 m around hazard edges so steering never leaves the safe cell; (c) before applying a step, test the destination cell and reject steps into hazard cells; (d) knockback is the only way to enter a hazard, and then deliberately (ULTRAKILL lets lava hurt enemies for 25 and ignite them, which is a player reward); (e) enemy-vs-enemy shoving must also respect (b)/(c); (f) a recovery rule for anything already in or beside a hazard: step back to the nearest safe cell, or jump if the enemy can.
For SKYGRIND the enemy movement is the web collider (no NavMesh), so use the grid: a hazard-flag per cell in `CoreGrid`, with enemy steering rejecting flagged cells and a hazard-edge repulsion term.

## 6. Campaign currency

Sourced:
- ULTRAKILL P: points per level are style points plus rank bonus (S 2,500, A 2,000, B 1,500, C 1,000, D 500), +500 no restarts, +5,000 no damage, +1,000 secrets; a sample level showed +19,615 P. Variants cost 7,500 (Marksman), 12,500 (Pump Charge), 25,000 (Sharpshooter, Sawed-On, Overheat), 35,000 (JumpStart), 75,000 (S.R.S., Firestarter), 100,000 (railcannon variants). Bought at terminals between and inside levels. Blue variants are free; green/red are bought. Roughly one big variant per 4-5 good levels.
- DOOM Eternal: weapon points (spent on mods and mastery, e.g. Quick Hook 3 points), Praetor suit points (one per challenge, three challenges also give a battery), Sentinel crystals (12 total: health, armor, ammo, 4 each).
- Hades: Darkness (permanent, Mirror of Night; Death Defiance ranks total 1,530), Gemstones, Nectar, Chthonic Keys.
- Roboquest: a wrench currency spent on basecamp upgrades that unlock run upgrades and a weapon-affix shop.
- Turbo Overkill: upgrade kiosks and weapon booths; a splice machine with two augments per limb. Neon White: gifts unlock side content (no spend economy found).

Proposal for SKYGRIND: points convert to "scrip" at level end (kills plus rank bonus), spent at a station between levels (a four-slot rack: 2 weapon mods, 1 movement perk, 1 wildcard) so the spend takes under 30 seconds on a pad. Price tiers at 1x / 2x / 4x of a typical level's earn (so tier one per level, tier two per two levels). Mods change behaviour not just numbers (the charge shot pierces everything; the grapple pulls light enemies; a dash adds a charge). No pay-to-win percentages. Rule check: points are earned only in play and mods are all sidegrades with a cost.

---

# SPEC (for a coding agent; values are proposals for a Unity-side config file)

## S1 Viewmodel and zoom
- Add a viewmodel camera or a per-material FOV override so the gun draws at its own FOV (start 60 deg, config).
- LT zoom changes world FOV only (e.g. 1.25x to 2x per weapon) and does not move the model toward centre. The model drops 4-6 cm and shrinks slightly; ease in/out over ~120 ms.
- For zoom-capable weapons draw a thin reticle/vignette ring; no full scope overlay.
- Idle sway, speed-scaled bob, a landing dip scaled by fall speed, and a recoil spring on the model; all in config. Remove any zoom path that lerps the model to the screen centre.
- Proof: a Player frame pair at zoom on and off with the target in the middle third unobstructed (luminance or pixel test).

## S2 Grapple to enemy
- Hold-or-tap: tap fires; the hook latches the first enemy on the ray within range. Light enemies are pulled to you; others pull you.
- Pulling you: ramp speed over ~0.1 s, then stop at 1.5 m from the target's hit volume (config), never inside; arrival gives a small hop, a small damage hit (config, tied to a fraction of base damage), and hitstop of 2-3 steps.
- Cancel by jump, dash or a second press; momentum is kept on a cancel. Cooldown 0.5 s, plus the existing long cooldown if one applies.
- Rope: a line from the muzzle to the target that goes taut at latch. Cues use the web's event names plus new ones in `PORT-STATUS.md`.
- Proof: Player line RED once and GREEN once for "stopped at the target, distance greater than the stop distance, no pass-through".

## S3 Piercing charge shot
- Charge tell 0.4-0.5 s; release fires a ray that damages every body along its path (no 3-body cap), damage per body raised by a config factor over today's.
- Beam drawn for ~0.15 s; hitstop 2-3 steps at the first hit; camera kick on release; keep or lengthen the 4 s cooldown.
- Proof: a line with N bodies in a row all take the damage.

## S4 Encounter triggers
- Enemies in a lava room do not exist until the player crosses a trigger line. Spawns are on the far side of the room, a spawn marker shown 0.6 s before the body appears.
- Waves on remaining-count thresholds; no spawn within a minimum distance of the player or inside the view cone at close range. Seeded `Mulberry32`.
- Proof: a replay of one lava level showing zero enemies alive before the trigger and a count after.

## S5 Hazard avoidance
- Flag hazard cells in `CoreGrid`. Enemy steering rejects any step into a flagged cell, adds repulsion within 1.5 m of the edge, and has a recovery step to the nearest safe cell if one starts in or next to a hazard.
- Only knockback and explicit player abilities may push enemies in; they take hazard damage as today.
- Proof: an N-enemy run for 60 s on each lava level with zero self-inflicted hazard deaths.

## S6 Campaign points
- Convert level score to scrip (kills plus rank bonus, +secrets). Spend at a station between levels: 4 slots (2 weapon mods, 1 movement perk, 1 wildcard), three price tiers at 1x / 2x / 4x of a level's typical earn (measure the earn per level first).
- Mods are sidegrades that change behaviour; examples above. Owned mods persist across levels and reset with a new run. Save in the existing prefs path (a key in `UnityTuning`).
- Proof: a RED/GREEN Player line: earn, buy, equip, and the effect is visible in the next level.

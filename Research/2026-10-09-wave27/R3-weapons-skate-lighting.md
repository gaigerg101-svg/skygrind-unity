# R3: Charge alt-fire, skate/ice feel, floor hazard and indoor look

Research date 2026-10-09. Web only. Tags: [read] = I fetched the page; [snippet] = search summary only; [judgement] = my
inference or an engineering suggestion, not a sourced fact. Several pages I wanted (Doom Eternal Blood Punch, Neon White,
Hades, SSX, Trackmania, Sunset Overdrive) were not retrieved. Doomwiki returned 403. Those are listed under "Not checked".
Search coverage for topic 2 was thin: no source gave FOV or roll numbers for a skating game. The numbers in the
SKYGRIND rules are therefore starting values to tune, not cited values.

---------------------------------------------------------------------------------------------------------------------

## Topic 1. Charge alt-fire with a long cooldown, and the PULSE gauntlet

### Prior art

**ULTRAKILL Marksman Revolver** [snippet] https://ultrakill.wiki.gg/wiki/Weapons , https://ultrakill.wiki.gg/wiki/Coins
- Primary: 0.5 s cooldown, damage 1 / 1.5 / 2 (base / limb / head).
- Alt fire is a coin toss, 4 s cooldown per coin. A coin shot by the revolver splits into extra hitscans (2 to 6 per coin,
  1.75 to 2.5 damage each). So the alt-fire is not a bigger shot. It is a setup that multiplies the normal shot.
- Lesson: a long-cooldown alt that is consumed by the normal fire keeps the normal loop central. [judgement]

**ULTRAKILL Pump Charge Shotgun** [snippet, page title only in results] https://ultrakill.wiki.gg/wiki/Pump_Charge_Shotgun
- Charge-on-pump shotgun with a detonate. No numbers retrieved.

**ULTRAKILL Feedbacker / Knuckleblaster** [read] https://ultrakill.wiki.gg/wiki/Arms
- Feedbacker: punch 1 damage, animation about 0.35 s, exhaustion cost 1 per punch. Parry flashes the screen, adds hitstop,
  refills stamina and health.
- Knuckleblaster: heavy punch 2.5 damage, animation about 0.87 s. Holding the key gives a close-range shockwave: 1 damage,
  launches light/medium enemies, deflects projectiles, animation about 1.9 s. Cost 1.5 exhaustion, which can go negative.
- Exhaustion model [snippet] https://ultrakill.wiki.gg/wiki/Whiplash : max 2, a punch needs at least 1, regen about 1.25
  per second, and a global cooldown that falls 2 per second. So the punch rate is gated by a recovering meter, not by a
  fixed timer. The player sees it as an arm icon meter on the HUD.
- Lesson: ULTRAKILL gates the heavy punch about once per 1 to 2 s, shorter than the 2.5 s asked for PULSE. The shockwave is
  the held variant of the same button, which matches GG's "LT shockwave".

**Titanfall 2 Charge Rifle** [read] https://titanfall.wiki.gg/wiki/Charge_Rifle ; [snippet] charge time 2.3 s
- Hold the trigger to charge and it fires only at full charge. 60 RPM in Titanfall 1. Orange charging beam is the visual
  indicator, and it reveals the player's position (a deliberate trade-off). Apex version builds damage over about 1 s.
- Lesson: a full-charge-or-nothing weapon is a different weapon, not an alt-fire. For SKYGRIND the normal shot must stay.

**Doom Eternal Blood Punch** [snippet] https://doomwiki.org/wiki/Blood_Punch (fetch failed, 403)
- Charged by glory kills: two glory kills give one charge, only one charge can be stored. No cooldown seconds retrieved.
- Lesson: a kill-fed charge that stores one use. This is the nearest precedent to "kills charge the shockwave".

**Others** [not checked]: Quake/Unreal alt-fires, Borderlands, Halo plasma pistol overcharge, Neon White, Hades. The
Halo plasma pistol overcharge is common knowledge (hold to overcharge, drains shields, brief lockout after) but I did not
fetch a source, so treat it as [judgement].

### How to keep it from replacing the normal fire loop [judgement, consistent with the ULTRAKILL data]
- Charge by holding the alt button, never by holding fire. Fire stays tap/hold as now.
- Charge is available only when the cooldown meter is full. Releasing early cancels with no cooldown or a small refund.
- Charged shot should be worth about 3 to 4 normal shots of damage but cost 4 to 6 normal shots of time (cooldown 8 to 15 s),
  so it is a burst tool for elites, not a DPS upgrade. ULTRAKILL's coin is the benchmark: 4 s cooldown for a roughly
  3 to 6x multiplier on a single revolver shot [snippet].
- GG's own rule (CORE.md): movement and crowd-control verbs need a cooldown of 15 s or more. A charge shot that
  knocks back or pierces a crowd falls under it. A pure single-target damage shot may be shorter (8 to 12 s).

### PULSE gauntlet and a kill-count charge meter
- Kill-fed charge is established by Blood Punch (2 glory kills per charge, 1 stored) [snippet]. Hades' and Neon White's
  versions were not checked.
- 5 kills per charge is a design choice, not a sourced typical value. Blood Punch's 2 per charge sets the low end; a
  round-based game with 20+ kills per round suggests 4 to 6. [judgement]
- Display ideas [judgement]: five pips around the crosshair or on the weapon model that fill cyan on each kill, whole ring
  pulses and the gauntlet glows when full. The weapon model itself (glove glow) is the best indicator because GG disliked
  mid-screen bars (CORE.md "stamina bar mid-screen really ugly").
- Swing every 2.5 s is slower than the Knuckleblaster punch (0.87 s) [read]. It must therefore hit hard (suggest 2.5 to
  3x pistol damage and a short knockback) or it will feel punished.

### What this means for SKYGRIND
1. Charge shots (pistol and shotgun) use the alt button (RT/LT per GG; the hold is the charge). Hold time to full charge:
   0.6 s pistol, 0.8 s shotgun. Releasing before 0.25 s fires nothing and costs nothing. Titanfall's 2.3 s [snippet] is
   too slow for a first-person run-and-gun; ULTRAKILL's coin has no charge at all. [judgement]
2. Cooldown after a charged shot: 10 s pistol, 14 s shotgun, in whole sim steps (1200 and 1680 at 120 Hz). Values go in
   config.json or a Unity-side config file (CLAUDE.md rule 5), not in code.
3. Damage: pistol charged shot 4x the normal shot, pierces up to 3 enemies. Shotgun charged blast 3x pellet total with a
   forward knockback. Starting values; verify with the headless balance judges.
4. Normal fire is never blocked while the charge cooldown runs. The cooldown only blocks the alt. Show the cooldown as a
   thin arc or ring around the crosshair plus a weapon-model glow returning from dim to cyan (not a bar mid-screen).
5. During the charge: weapon model glow ramps, FOV narrows 2 degrees (zoom feel), movement is not slowed.
6. PULSE gauntlet: swing every 2.5 s (300 steps), damage 3x pistol, short arc. Kill-fed charge: 5 kills fill five pips;
   when full, holding LT releases the shockwave (cooldown 15 s or longer per GG's rule) and a self-boost of about 0.4 s.
   Only kills count (not hits), as GG specified. Cap the stored charge at 1, like Blood Punch.
7. Fix the sim to the step: the charge timer, cooldown and kill counter all reset at run start and use `stepsFor`.

Sources: https://ultrakill.wiki.gg/wiki/Arms [read]; https://titanfall.wiki.gg/wiki/Charge_Rifle [read];
https://ultrakill.wiki.gg/wiki/Weapons , /wiki/Coins , /wiki/Whiplash , /wiki/Pump_Charge_Shotgun [snippet];
https://doomwiki.org/wiki/Blood_Punch [snippet].

---------------------------------------------------------------------------------------------------------------------

## Topic 2. Skate / ice feel: "dashing right to left, the entire floor"

### What I found
- Speed sensation comes from a wider FOV (more periphery) and from camera height. A wider FOV raises perceived speed
  [snippet] https://overtake.gg/threads/adjusting-fov-in-racing-sims.273396 . Moving the camera lower and forward also
  raises it. One search summary said that in an ice-skating game the camera had to sit closer to the ground to sell speed
  [snippet] https://www.shawnhargreaves.com/blog/vrrroom-whoosh.html (search result; I did not open it).
- Some racing games vary FOV with speed, quoted as 70 to 90 degrees [snippet, forum]. Treat it as indicative only.
- Skid and streak marks that get denser with speed make the player feel faster [snippet]. Ground-plane texture detail is
  what the eye reads for velocity, which is why "the entire floor" is the right target. [judgement]
- No fetched source gave roll-on-drift numbers. SSX, Jet Set Radio, Trackmania, Wipeout, Mirror's Edge, Tony Hawk,
  Sunset Overdrive and Sonic were not retrieved. [not checked]

### Open-source pieces
1. **Anime Speed Lines** [read] https://github.com/mirzabeig/anime-speed-lines
   - Licence: Unlicense (public domain). Safe to copy.
   - Post-processing effect: noise plus polar coordinates to draw radial speed lines around the screen centre. URP route is
     `CustomRenderPassFeature`; Unity 2021.3+.
   - Verdict for SKYGRIND: it is the screen-border speed-line frame that GG retired in w20 ("so forced ... that weird
     border"). Do NOT use it as the effect. It is useful only as a reference for writing a URP full-screen render feature.
2. **Simple Lazy Triplanar** [snippet] https://gitlab.com/s-ilent/simple-lazy-triplanar
   - Licence: MIT per the search summary (not verified on the repo page). Standard (built-in pipeline) based, so it would
     need porting to URP. Value here is the world-space (triplanar) mapping idea, not a drop-in.
3. I did not find a verified MIT/Apache/CC0 URP "floor streak" shader. The technique below is my own and carries no
   licence issue (write it fresh in the repo).

### Cheap URP floor-streak technique [judgement, standard shader maths]
Do it in world space in the floor material so the streaks belong to the floor, not the screen:
- Unlit or Lit-with-emission shader, or a Shader Graph with these nodes. Inputs: `_PlayerVel` (xz, set globally each
  frame with `Shader.SetGlobalVector`), `_Streak` (0..1 strength from speed).
- Streak coordinate: `p = worldPos.xz`. Take direction `d = normalize(_PlayerVel)`, perpendicular `n = (-d.y, d.x)`.
  Lines run along `d`, so use `u = dot(p, n)` for line index and `v = dot(p, d) + _Time.y * k` for scroll.
- `lineId = floor(u * density)`, `h = hash(lineId)`, streak mask = `smoothstep(w, 0, abs(frac(u * density) - 0.5))`
  multiplied by `step(h, _Streak)` (so more lines appear as speed rises) and a stretched noise along `v`
  (`noise(v * 0.15 + h * 50)`), so each line is a dash, not a continuous stripe.
- Parallax: add a second layer at `density * 0.5`, scroll speed `0.6x`, alpha 0.5. Two layers read as depth.
- Cost: about 10 ALU ops, one hash, no textures, no overdraw if it is part of the floor material. Fits mobile-class URP.
- Colour: cyan to pale violet-white, never red-orange (palette rule 8).
- The "dashing right to left" feel: the lines should run in the direction of travel relative to the camera, so a
  strafe makes the floor lines sweep across the screen horizontally. This falls out of using the player velocity.

### What this means for SKYGRIND
1. FOV: base 90 (or the player's setting). Add speed-based FOV, linear from 0 at normal run speed to +10 at 1.5x
   run speed, ease-in 0.15 s, ease-out 0.4 s. Cap total at base + 12. Starting values [judgement]; the "70 to 90" racing
   quote [snippet] confirms the technique but not the amount.
2. Camera roll on lateral velocity: roll = clamp(lateralSpeed / maxSpeed, -1, 1) * 4 degrees, smoothed with a 0.12 s
   time constant. 6 degrees on a skate slide. Starting values; stay at or below 8 degrees to avoid nausea. Roll must be
   cosmetic and not feed the sim (the sim stays bit-exact, rule 3).
3. Camera height: lower the eye by 0.05 to 0.08 m while sliding; confirm against the ice-camera-lower claim [snippet].
4. Floor streak shader as above on the ice floor material: density 2.5 lines per metre, line width 0.04, scroll factor
   `k = speed * 0.3`, strength rising from 0 at 30% of top speed to 1 at 100%.
5. Ice sparkle: world-space dust motes (GPU particles, 40 to 80 live) near the feet, velocity opposite to travel,
   lifetime 0.5 s, cyan-white. Carve spray: a short burst (6 to 10 particles) on a direction change above 60 degrees.
6. Blade trail: two thin additive trails behind the feet, 0.25 s lifetime, alpha fades with speed.
7. Wind: no audio (rule 7). Visual only: tiny camera shake, amplitude 0.002 rad at top speed, 8 to 12 Hz.
8. Do not add any screen-border frame or radial vignette streaks. World-space and floor streaks only (GG's rule).
9. Judge it per CORE L-001: capture two clips at matched speed, check that the floor-pixel motion (pixels per frame)
   rises at least 25% versus the plain floor.

Sources: https://github.com/mirzabeig/anime-speed-lines [read]; https://overtake.gg/threads/adjusting-fov-in-racing-sims.273396
[snippet]; https://www.shawnhargreaves.com/blog/vrrroom-whoosh.html [snippet]; https://gitlab.com/s-ilent/simple-lazy-triplanar
[snippet]; https://void1gaming.itch.io/free-grid-shader-for-unity-urp [snippet, scrolling grid shader; licence not checked].

---------------------------------------------------------------------------------------------------------------------

## Topic 3. Floor hazard that forces wall riding, and the indoor look

### (a) Temporary damaging floor

What the sources said:
- ULTRAKILL: hazards are zones that damage or kill and "set limits on available space". Lava deals 50 damage to the
  player and 25 to enemies per contact event and ignites flammable enemies [snippet]
  https://ultrakill.wiki.gg/wiki/Pit . A "Death Zone" deals a set damage and teleports the player to the nearest safe
  ground [snippet]. That teleport-to-safety rule is the opposite of what we want for a wall-ride forcing floor, but it is
  a useful fairness reference.
- Titanfall 2: marker warnings for hazards, and a red marker flashing on a hit [snippet]
  https://en.wikipedia.org/wiki/Titanfall_2 . I could not retrieve a source for an "Effect and Cause" floor hazard, so I
  do not claim one.
- Ghostrunner, Sonic, Mario, Super Meat Boy: not retrieved. [not checked]

Design reasoning [judgement]:
- Telegraph 1.5 to 2 s before the floor goes live: floor emissive pulses amber then red-orange, hum-free (no audio, rule
  7), with a ring that expands from the hazard origin. Use the player's wall-run kit as the safe route, so keep wall
  surfaces cool-coloured (cyan edge) during the hazard.
- Damage: tick based and low so it forces movement and does not kill from a single touch. Suggest 8 to 12 HP per
  second (a 100 HP player gives about 8 to 12 s of stand-on-floor survival), plus a visible 0.2 s hit flash. ULTRAKILL's
  50 per contact is a burst model for a game with much higher health numbers [snippet].
- Duration 6 to 10 s live, 15 to 25 s between events, never on the first two rounds. Never fire it in the same moment as
  another movement-taking effect.
- Read in dusk palette: red-orange glow floor with scrolling heat shimmer; edges that are safe (walls, pillars, platforms)
  keep the pale violet-white boundary tone. Cyan stays player-usable: light wall-run surfaces cyan to say "go here".
- Enemies must still be able to reach all floors (GG's rule): either enemies are immune to the hazard or the hazard
  skips the climb routes. Do not hurt enemies unless GG wants it (ULTRAKILL damages both [snippet]).

### (b) Baked lighting, roof, "almost indoors"

What I read [read] https://docs.unity3d.com/Manual/urp/probevolumes-concept.html (Unity 6.3):
- Adaptive Probe Volumes (APV) place Light Probe bricks (4x4x4 = 64 probes) by geometry density, bake indirect light,
  give per-pixel lighting, and the Baking Set holds one or more scenes. Lighting Scenarios can be baked and blended at
  runtime. Probe positions cannot be moved by hand; moving geometry after the bake breaks the data.
- The page says nothing about runtime-generated geometry. It implies the bake is for scene geometry that exists in the
  editor (the page does not state "static" outright).

What the snippets said:
- Lightmaps need baked UVs in `Mesh.uv2`, and meshes that use `uv2` for something else must use probes
  (`MeshRenderer.receiveGI`) [snippet] https://docs.unity3d.com/Manual/lighting-gi-uvs-real-time-lightmap.html . A mesh
  built at runtime from JSON has no baked lightmap data, because the bake runs in the Editor on scene objects.
- APV gives higher quality than Light Probe Groups and better directionality than lightmaps [snippet]
  https://unity.com/blog/engine-platform/new-ways-of-applying-global-illumination-in-unity-6 .

Options for runtime-generated geometry [judgement, built on the facts above]:
- A. Bake per level scene in the Editor. Needs the plan converted to real scene meshes ahead of time. Cost: a build step
  per map, large scene files, loses runtime variation. Best look, highest effort. Not recommended for a data-driven game.
- B. APV on a placeholder scene with pre-baked probes. Dynamic meshes can sample probes, but probes were baked against
  the geometry that existed, so a runtime-generated room that differs will leak or look wrong. The docs say moving
  geometry invalidates the data [read]. Only workable if a map's shape is fixed and baked once.
- C. Fake it (recommended): no GI bake at all.
  1. Ceiling: a flat ceiling quad or a generated roof slab at the plan's top height, dark violet-black, with a few cutout
     slots for a skylight (an "almost indoors" look needs a roof and one light source above).
  2. Vertex-colour ambient occlusion: when generating each mesh, darken vertices near wall-floor and wall-ceiling joins
     (value 0.55 to 0.75 within 0.6 m of a corner). Multiply the albedo by the vertex colour in the shader. Cost: one
     colour attribute, no UV2.
  3. Emissive strip lights: unlit or emissive-material boxes (cyan/pale violet) along ceiling edges and floor edges.
     Add a few real URP point lights (not shadowed) at strips near the player, limited to 4 per pixel and 8 per object
     in the URP asset, for local glow. Bloom picks the strips up.
  4. One baked reflection probe or just a cubemap in the Lighting Environment: set Environment Lighting to a flat
     colour or a gradient (dusk violet ambient). Reflection probes in an Editor-baked placeholder scene still work for
     dynamic meshes if the probe volume is large enough. Optionally render one probe at map load with
     `ReflectionProbe.RenderProbe` in Realtime mode, "On Awake" refresh only.
  5. Fog: low exponential fog tinted violet, density 0.01 to 0.02, to push the sense of enclosure.
  6. Dynamic objects (enemies, props) sample the ambient gradient only; fine for a blocky style.
- D. Open skylight option: keep the ceiling but add a skylight hole and a directional light for dusk, shadows on,
  one cascade (this is "indoors but you can see the sky").

Gotchas [judgement]:
- Never recompile or bake in Play mode (CLAUDE.md); APV baking and lightmapping are Editor-only.
- Progressive GPU lightmapper bakes are minutes per scene and the output lives in the scene, so each map plan would
  need its own scene. This conflicts with generating geometry from JSON.
- Light Probe Groups cannot be converted to APV [read].

### What this means for SKYGRIND
1. Take option C (vertex-colour AO + emissive strips + ceiling quad + gradient ambient + exponential fog). No baking, no
   scene-per-map. Cost: one generation pass in the mesh builder and one ceiling mesh per map plan.
2. Vertex AO numbers: 0.6 at inside corners, 0.8 along edges, 1.0 in open areas, falloff 0.6 m. Floor-to-wall joins
   only at first. Do not move any collider; the sim keeps using `ThreeOctree` (rule 4). This is render only.
3. Ceiling: top height = plan max height + 3 m, colour #1a1226-ish (dusk dark violet), with one skylight slot 20 to 30%
   of floor area (option D) for the "almost indoors" look. Enemies can still climb to every floor (rule from CORE.md).
4. Lights: 6 to 10 emissive strips per room, cyan for player-usable routes, pale violet-white at the boundary, red-orange
   only for hazards/enemies. Max 4 real point lights lit near the player; everything else is emission plus bloom.
5. Hazard floor: 1.5 s telegraph (amber pulse), 8 s live, 12 HP/s, 20 s spacing; wall surfaces stay cyan-edged. Hazard
   state changes go through gameplay events with web-style names (rule 7); numbers from a Unity-side config.
6. If GG later wants true GI, test APV on one fixed map first (option B), measure bake time and leak artefacts before
   committing the pipeline.

Sources: https://docs.unity3d.com/Manual/urp/probevolumes-concept.html [read];
https://docs.unity3d.com/Manual/urp/probevolumes-showandadjust.html [snippet];
https://docs.unity3d.com/Manual/lighting-gi-uvs-real-time-lightmap.html [snippet];
https://unity.com/blog/engine-platform/new-ways-of-applying-global-illumination-in-unity-6 [snippet];
https://ultrakill.wiki.gg/wiki/Pit [snippet]; https://en.wikipedia.org/wiki/Titanfall_2 [snippet].

---------------------------------------------------------------------------------------------------------------------

## Not checked (honest gaps)
- Doom Eternal Blood Punch cooldown seconds (doomwiki 403). Neon White, Hades, Quake/Unreal alt-fires, Borderlands,
  Halo plasma pistol: no source fetched.
- SSX, Jet Set Radio, Sunset Overdrive, Sonic, Trackmania, Wipeout, Mirror's Edge, Tony Hawk speed-feel numbers: no source.
- Ghostrunner, Super Meat Boy, Mario hazard timings: no source. No source for a Titanfall 2 floor hazard level.
- Licence of Simple Lazy Triplanar and of any scrolling-floor shader: only the search summary. Only the Unlicense
  repo was read. A second verified MIT/Apache/CC0 shader was not found in the time spent.
- All FOV, roll, damage and cooldown numbers in "What this means" are starting values from my own judgement, to be tuned
  with the measured-feel loop (L-001), not figures taken from the sources.

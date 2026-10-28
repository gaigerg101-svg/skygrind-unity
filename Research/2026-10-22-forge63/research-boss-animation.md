# Boss motion for code-driven primitive bodies: research (FORGE63)

Date: 2026-10-22. Web research only (about 27 tool calls, about 11 minutes). Nothing in the project was read or run.
Marks: **Verified (page)** = I opened the page and read the claim. **Verified (excerpt)** = the claim is in a search
engine excerpt of that URL; I did not open the page. **Inferred** = my reasoning or arithmetic, or the source page is
not pinned. "Not checked" = I looked and found nothing usable, or did not look.

## Summary: what to build, in priority order, with the numbers

1. **A telegraph pose driven by the sim's existing wind-up timer** (view only): the body pulls back or crouches over the wind-up, then snaps through. Give at least 15 frames of warning for an attack the player can learn and 30 for one they can't (critpoints; no frame rate is stated; at 60 fps that is 250 / 500 ms, at the 120 Hz sim 30 / 60 steps: Inferred). If an attack is faster than that, add a second cue, e.g. a flash.
2. **A flash at wind-up start, with a different shape or colour for each class of attack**: ULTRAKILL uses yellow for parryable and blue for unparryable, a 4-point spark for melee and an 8-point spark for ranged. In SKYGRIND the flash must stay inside the red-orange danger family (project rule 8). Code the two classes by spark shape or brightness, not by a new hue.
3. **One second-order follower per visual part** (t3ssel8r's f / zeta / r): `k1 = z/(pi f)`, `k2 = 1/(2 pi f)^2`, `k3 = r z/(2 pi f)`. r < 0 anticipates, r > 1 overshoots. The safe step is `sqrt(4 k2 + k1^2) - k1`, which is about 0.098 s at f=2, z=0.5 and 0.030 s at f=8, z=0.3. Both are far above the sim's 1/120 s = 0.0083 s (computed here). No source gave tuning ranges. Starting values to try (Inferred): f 1.5-3 Hz, z 0.4-0.7, r 1-2 for body parts that trail, and r of -0.5 to -1 for a deliberate anticipation dip.
4. **A squash on hit using an impulse envelope**: Quilez's `expImpulse(x,k) = h*exp(1-h)`, `h = k*x`, peaks at 1 when x = 1/k. Scale one axis by s and the other two by 1/sqrt(s) so the volume stays the same (the principle is from Wikipedia; the scale math is Inferred). For a peak at 40 ms use k = 25 /s (Inferred).
5. **Look-at and aim with lag** through Holden's critically damped spring, set by a halflife: `damping = 4 ln2 / halflife`. It settles as fast as possible without oscillating. No source gave a halflife for aim. Start at 0.08-0.2 s (Inferred).
6. **A worm or serpent body as a chain**: each segment is held at a fixed distance from the one ahead, plus an angle limit per joint so the body can't fold back on itself. If the segments are hitboxes, the chain must live in the sim, not the view (Inferred; rule 4).
7. **A stagger state with its own cue**: DOOM 2016 makes staggered demons glow, then changes the glow colour when the player is in glory-kill range. Sources disagree on the order, blue then orange or orange alone. Add a slump pose plus a part shake. The sim already has hitstop. Study numbers put hit-stop at about 0.1-0.2 s and slow motion at 0.2-0.4 s. Don't add a second freeze in the view.
8. **Idle breathing and sway**: a low-amplitude sine on height and scale. Mute it during a wind-up so the telegraph is the only motion on screen (Inferred; no source gave numbers).
9. **Guards**: cap every view offset so the drawn body never leaves its sim collider by more than a small margin. Draw each telegraph on or from the body that attacks. Never dim a telegraph or hide it in dust (the Hades II critique; the indie reviews of "hitbox larger than the visuals").
10. **Proof** (L-093 / L-108): a Player frame pair, wind-up versus neutral, that measures the pose displacement and the flash pixels. A component value is not proof.

## 1. Procedural techniques for code-driven bodies

### Second-order dynamics (t3ssel8r, "Giving Personality to Procedural Animations using Math", 2022)
- The video is at https://www.youtube.com/watch?v=KPoeNZZ6H4s. The ID comes from a mirror listing, https://yt.dorper.me/v/yt/KPoeNZZ6H4s. **Inferred** (I did not open the video). The listing's chapters cover second-order systems, implementation, stability and a conclusion. Its description cites semi-implicit Euler, Verlet and pole-zero matching. **Verified (excerpt)** of the mirror page.
- Hackaday's summary: the system uses a semi-implicit Euler solver, and three parameters control frequency, damping and initial response. With too large a timestep, the position "quickly explodes outwards", and the video uses eigenvalues to pick the step. https://hackaday.com/tag/game-design **Verified (excerpt)**.
- Exact code from the Rust crate `spring_motion`, which is "based on t3ssel8r's video". **Verified (page)**: https://docs.rs/spring_motion/latest/src/spring_motion/lib.rs.html
  ```
  k1 = z / (PI * f)
  k2 = 1 / (2 PI f)^2
  k3 = r * z / (2 PI f)
  position += velocity * dt
  accel = (target + target_velocity * k3 - position - velocity * k1) / k2
  velocity += accel * dt
  max_safe_dt = sqrt(4 k2 + k1^2) - k1      // the crate clamps dt to this
  ```
  The parameter docs: f is "the natural frequency", the speed at which the system responds and vibrates. zeta is the damping: 0 is undamped, 0 < zeta <= 1 is underdamped. r is "the initial response": "Negative: system anticipates motion by going in the opposite direction shortly", and "r > 1: system overshoots target at first". https://docs.rs/spring_motion **Verified (page)** for the crate's existence and the link to the video.
- Why it fits SKYGRIND: the follower takes the sim's position (and velocity, if the sim has one) as its target and produces only a drawn position. Feed it the interpolated render pose so it is never fed back into the sim (Inferred).
- Recommended f / zeta / r ranges: **not checked** (the video was not watched). The values in the summary are my own starting points (Inferred).

### Dampers and springs with a halflife (Daniel Holden, "Spring-Roll-Call")
URL for every claim in this subsection: https://theorangeduck.com/page/spring-roll-call **Verified (page)**.
- Exact damper: `x = lerp(x, g, 1 - exp(-(ln2 * dt) / halflife))`. The halflife is the time to close half the gap.
- `halflife_to_damping(h) = 4 ln2 / h`. The article calls the 4 "a bit of a fudge factor".
- `frequency_to_stiffness(f) = (2 pi f)^2`. The article calls this frequency "more like a pseudo-frequency", because damping also changes the rate of oscillation.
- `critical_spring_damper_exact` returns "as fast as possible without extra oscillation". It is the easiest spring to use because it has the fewest parameters. Holden uses springs for smoothing ("any noisy signal can be easily smoothed"), for character velocity, and for inertialization ("exactly the method we use for switching between animations").
- He warns that a spring "does not provide a fixed blend time and can easily overshoot". For a telegraph that must end on an exact step, use a timed curve, not a spring (Inferred).

### Frame-rate-independent lerp smoothing
- Freya Holmer's talk "Lerp smoothing is broken" (2024). Her post only announces the talk; the blog version was not finished. https://acegikmo.substack.com/p/lerp-smoothing-is-broken **Verified (page)**. A secondhand transcription of her formula is `a = b + (a - b) * exp(-decay * dt)`. **Inferred** (forum user's transcription, from a search excerpt; the video was not checked).
- An equivalent form: `value = lerp(target, value, exp2(-rate * dt))`. https://howlingmoonsoftware.com/improved-lerp-smoothing/ **Verified (excerpt)**.
- Her suggested decay range (1-25): **not checked**.

### Impulse and envelope shapes (Inigo Quilez)
- `expImpulse(x, k)`: `h = k*x; return h*exp(1.0-h)`. "Use k to control the stretching of the function"; its maximum of 1 "happens at exactly x = 1/k". Impulses are "great for triggering behaviours or making envelopes for music or animation". https://iquilezles.org/articles/functions **Verified (excerpt)**; the page itself returned 404 to my fetch.
- Use: x is the time since the hit, and the output scales the squash and the recoil offset (Inferred).

### Classical principles (the vocabulary for the poses)
URL for every claim in this subsection: https://en.wikipedia.org/wiki/Twelve_basic_principles_of_animation **Verified (page)**.
- Squash and stretch gives "a sense of weight and flexibility"; in realistic animation "an object's volume does not change when squashed or stretched". So scale (s, 1/sqrt(s), 1/sqrt(s)) (Inferred math).
- Anticipation "is used to prepare the audience for an action".
- Overlapping action is "the tendency for parts of the body to move at different rates". With primitives, give each capsule its own follower, with f lower the farther the capsule is from the root (Inferred).
- Exaggeration: motion that imitates reality perfectly "can look static and dull".
- Secondary action "gives a scene more life, and can help to support the main action".

### Chains for worm, serpent and leg segments
- A snake as a chain of points, each held at a fixed distance from its neighbour, plus a per-segment angle limit so the body can't fold back. A Pico-8 snake constrains each segment's centre to the radius of the segment before it, then limits the angles between segments, and credits a video by "argonaut" for most of its motion. https://www.lexaloffle.com/bbs/?pid=172833 **Verified (excerpt)**. Other itch.io devlog posts describe the same method: https://itch.io/post/13699597, https://itch.io/post/13703767 and https://itch.io/post/13722314 **Inferred** (which of the three says what is not pinned).
- A Verlet-style chain: the head leads, the body follows, and a tube mesh is rebuilt each frame. https://discourse.threejs.org/t/hiss-highness-procedural-snake-game-with-terrain-accurate-slithering-no-raycasts/88844 and https://tympanus.net/codrops/?p=108307 **Verified (excerpt)**; which of the two says what is not pinned.
- The argonaut video itself: **not checked**.

### A cheap stand-in for animation frames
- An itch.io developer: "particles, squash-stretch, and flashing indicators are my usual shortcut for avoiding animating a ton of 'anticipation' frames". https://itch.io/post/11673609 or https://itch.io/post/8070555 **Verified (excerpt)**; which of the two is not pinned.

## 2. Telegraphs that players read

- **Frame thresholds:** "the player has at least 15 frames to react" when the response can be anticipated, and "30 frames minimum if they don't know what to expect". "If there is something below these thresholds, signal it in another way outside these thresholds." Also: "Use clear audio and visual cues, double up on them if you can." No frame rate and no reaction-time reasoning are given. https://critpoints.net/2015/05/23/crafting-mechanics-and-bosses/ **Verified (page)**. The 60 fps and 120 Hz conversions in the summary are **Inferred**.
- **ULTRAKILL:** a yellow flash marks an attack that can be parried, and a blue flash one that cannot. Melee attacks get a "4-point parry spark", ranged attacks an "8-point parry spark". A parry makes the game "briefly freeze and flash the screen white". The page gives no durations. https://ultrakill.wiki.gg/wiki/Parry **Verified (page)**.
  - The Power's greatsword: each slash also launches two parryable yellow projectiles joined by an unparryable beam, so one attack mixes both classes. There is "a long period of vulnerability between the parry flash and the final swing". https://ultrakill.wiki.gg/wiki/Power **Verified (excerpt)**.
  - A 2022 player thread calls the blue flash inconsistent. This is one player's opinion. https://steamcommunity.com/app/1229490/discussions/2/3734079155590153286 **Verified (excerpt)**.
- **DOOM 2016:** demons stagger after a critical amount of damage, are briefly unable to fight back, and are highlighted. Sources describe a blue glow that turns orange when the player is in glory-kill range, but one guide says it flashes orange alone, so the sources disagree. Most glory kills begin with a short invulnerability. https://doomwiki.org/wiki/Glory_kill **Verified (excerpt)**; my fetch of doomwiki returned 403.
- **Elden Ring and Dark Souls:** "every attack must be telegraphed". Malenia's wind-up is brief but present; the floor collapses in the Bed of Chaos fight have no consistent visual tell. https://choostgames.com/blog/what-makes-a-good-boss-fight/ **Verified (excerpt)** (a developer blog, not FromSoftware).
- **Anatomy of an attack:** an attack opens with a build-up the player can read "to visually anticipate that an attack has begun". https://signalsandlight.substack.com/p/what-is-an-enemy-attack **Verified (excerpt)**. Each attack wants "a wind-up animation, a visual cue, an audio signal". https://bugnet.io/blog/how-to-design-enemy-attack-telegraphs **Verified (excerpt)**.
- **Deliberate over-telegraphing as a fake-out:** https://www.wayline.io/blog/the-art-of-the-fakeout-combat-design **Verified (excerpt)**. This is a late-boss tool only, after the honest version is learned (Inferred).
- **Hades, Hollow Knight, Furi:** no developer source on telegraph timing was found. **Not checked**. A review says Hades' attacks are "well telegraphed" except on some small foes; this is a review, not a measurement. https://gamingtrend.com/reviews/one-does-not-simply-walk-out-of-hades-hades-review/ **Verified (excerpt)**.
- **A GDC talk on boss telegraphs:** none was found in two searches. **Not checked** (search GDC Vault directly next time).

## 3. Hit reactions and stagger

- **Hit-stop duration:** a subjective study of 3D action games found the best hit-stop "approx. 0.1 to 0.2 seconds" and the best slow motion "approx. 0.2 to 0.4 seconds", in a scene where enemies are blown off screen. The search excerpt carried this; it is most likely the IEICE report at https://ken.ieice.org/ken/paper/20230315OCSt/eng/ (my fetch returned 405). **Inferred** attribution.
- **Practitioner advice:** hit-stop is "a few frames"; scale it by hit strength; freeze only the attacker and the target, not global time; use unscaled time. https://bugnet.io/blog/how-to-fix-hitstop-or-freeze-frame-feeling-off **Verified (excerpt)**. One Japanese dev blog's sweet spot is 0.05-0.1 s. **Inferred** (most likely https://uhiyama-lab.com/en/notes/unity/unity-game-feel-hit-feedback/; not opened).
- **White flash length:** one blog says 50-100 ms, but it is weakly sourced. https://eastondev.com/blog/ja/posts/dev/20260521-game-feedback-feel/ **Unverified**. No reliable source gave a flash length.
- **The stagger and break state:** DOOM 2016's highlight and colour change, as in section 2. DOOM Eternal's destructible demons (visible damage per part): **not checked** (doomwiki returned 403).
- **For SKYGRIND:** the sim already has hitstop (rule 4), so the view layer reads the sim's hitstop and stagger flags and adds only pose, shake and flash. During hitstop the view should freeze too, or the followers keep moving while the sim is frozen (Inferred).

## 4. Pitfalls

- **Telegraphs placed apart from the body, or misleading:** Hades II AOEs are "regions of the arena turning red usually independently of the boss's positions". Cerberus telegraphs on one side, yet "those shockwaves still reach fully behind" him. A wave is "intentionally obscured by the cloud of dust". A stomp's warning is hard to see because "the coloration before the explosion is dim". https://nex-3.com/blog/hades-ii-has-a-legibility-problem/ **Verified (page)**.
- **A hitbox bigger than the drawn body:** in indie boss fights, damage landed as if "the entire boss model and not just the sledgehammer" were the hitbox; another boss's hitbox was "way larger than the visuals would indicate"; one developer admitted their hitboxes are "a bit iffy". https://itch.io/post/11935834 and https://itch.io/post/11936033 **Verified (excerpt)**; which of the two says what is not pinned. In Hades, a miniboss's spin was called "unreasonably disjointed". https://steamcommunity.com/app/1145350/discussions/0/591769824782233841 **Verified (excerpt)**.
  - For SKYGRIND, the risk runs the other way: view-only overshoot and squash move the drawn body off the true sim collider. Cap the offset (e.g. at a fraction of the capsule radius), and keep the follower's r <= 2 on any part that can deal damage (Inferred).
- **Springs blow up when the timestep is too large** (Hackaday, above). A fixed 120 Hz with the crate's safe-step clamp avoids this (computed above). Even a critically damped spring can overshoot slightly in floating point, so implementations clamp near the target. **Inferred** attribution: the excerpt sat next to Unreal's FCriticalDamper page, https://dev.epicgames.com/documentation/unreal-engine/API/Plugins/GameplayCameras/FCriticalDamper.
- **Springs have no fixed blend time** (Holden, above). A telegraph timed by a spring will not end on the sim's attack step. Drive telegraphs from the sim's timer with an eased curve, and use springs only for the secondary motion around them (Inferred).
- **Wobble that reads as jank:** no source measured it. **Not checked**. My reasoning: idle sway, followers and shake all running during a wind-up drown the anticipation pose. Damp secondary motion (lower amplitude, higher zeta) while a telegraph is active, so the telegraph is the largest motion on screen (Inferred).
- **Colour coding the players doubt:** ULTRAKILL's blue flash drew a "flash inconsistent" thread (above), and the project's palette already reserves cyan for the player. Two classes of attack need two shapes or brightness levels within the red-orange family (Inferred from rule 8).

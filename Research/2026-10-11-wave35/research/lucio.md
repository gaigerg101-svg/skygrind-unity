# Lucio-style wall riding: mechanics, tech, open source, level design (wave 35 research)

Date: 2026-10-11. Web research only; every claim has a URL. "Not found" means searched and not found in this budget
(about 30 tool calls). Fandom, Liquipedia, Mobalytics, Dexerto-style guide pages were often blocked (402 / 403), so some
facts come from search-result excerpts of those pages; they are marked "(excerpt)".

## Answer

Lucio's Wall Ride is a passive, hold-to-ride move: jump at a wall and hold Jump, and he rides for as long as Jump is held,
with a movement speed buff while riding and a short speed boost when he leaves the wall; releasing Jump (by default since
the October 2020 patch) fires the wall jump boost, and Crouch drops off without it. Since May 2018 the ride follows outside
and inside corners without leaving the wall and is less likely to break along one surface, and he may re-land on the same
wall only if his leap took him far enough away. No ride time limit, no on-wall gravity value and no Overwatch 2 era
wall-ride change were found in official notes. The feel the owner wants comes from three things our ride lacks: no timer
(hold = stay), corner following, and a release jump that adds speed so hop-chains build momentum. Open source has no
Lucio clone; generic wall-run repos (MIT / Apache) exist but none documents curved-wall following. Level design sources
give principles (chain walls, read the route) but no published metrics.

## 1. Lucio's wall ride rules

- Start and hold: "jump at a nearby wall and hold the Jump key. Lucio will Wall Ride as long as it is held"; passive with a
  0.5 s cooldown (excerpt): https://overwatch.fandom.com/wiki/Wall_Ride and https://overwatch.fandom.com/wiki/L%C3%BAcio
- Time limit: none stated; "as long as it is held" (excerpt, same pages). An official numeric limit: not found.
- Gravity on the wall: not found (no official or datamined value). Players climb by riding while looking up or down and
  criss-cross jumping between walls, which the community calls intended: https://us.forums.blizzard.com/en/overwatch/t/lucio-wall-climb-mechanic-question-intended/720537
- Speed while riding: April 2017 live notes, "movement speed increased by 20% when wall riding" plus "a burst of speed when
  leaping off a wall" (March 2017 PTR had 30%) (excerpt): https://www.icy-veins.com/forums/topic/30073-patch-notes-for-april-11th-2017/
  and https://overwatch.blizzard.com/en-us/news/patch-notes/ptr/2017/03
- Exit boost: October 17 2017, "The speed boost that Lucio receives after completing a wall ride has been increased by
  65%", to compensate a bug fix that had slowed the ride: https://overwatch.blizzard.com/en-us/news/patch-notes/live/2017/10
- Later, Wall Ride "speed buff reduced from 40% to 30%" (late 2019): https://west-games.com/overwatch-lucio-changes/
  The wiki excerpt also gives "+30% movement speed buff" (excerpt): https://overwatch.fandom.com/wiki/Wall_Ride
- Jump-off numbers (excerpt, wiki): looking down and holding forward, the wall jump goes up at 4.12 m/s; looking up, 9.12
  m/s. Air speed is not decayed above 5.5 m/s unless air speed exceeds double ground speed (thresholds 11 / 13.76 / 17.6
  m/s by aura): https://overwatch.fandom.com/wiki/L%C3%BAcio . Not verified against a datamine.
- Corners and surfaces (May 3 2018 overhaul, quoted): "less likely to be interrupted along a single surface"; "Can now go
  around corners (both outside corners, and inside corners) without having to leave the wall"; "Can now land back on the
  same wall after leaping away, provided his leap takes him far enough away"; "Added a minimum time that the jump key must
  be held before Lucio can ride around corners": https://www.over.gg/8166/overwatch-patch-notes-for-may-3-2018-rialto-and-hero-tweaks
- Switching and curved walls: a community guide says curved walls count as walls facing a different direction, he cannot
  re-ride the same wall or a same-facing wall until he lands or rides a differently facing wall, and corners help climbing
  (excerpt): https://mobalytics.gg/overwatch/heroes/lucio-guide . It also claims "+120%" ride speed and a stackable +65%
  per wall jump, which conflicts with the patch notes above (the 65% was a buff to the exit boost); treat as unreliable.
- Release jump: October 20 2020, "By default, Lucio will now perform his wall jump boost immediately upon releasing Jump
  to leave the wall, instead of requiring a second Jump press"; "Lucio can now also drop out of wall rides by pressing
  Crouch, which will not trigger the automatic jump"; setting "Wall Jump On Release". October 23: crouch then releasing jump
  "within 0.2 seconds will provide a jump boost (Jump release timing is consistent with live server Jump re-press
  timing)": https://overwatch.blizzard.com/en-us/news/patch-notes/ptr/2020/10
- Late jump: a short window after detaching still allows a wall jump (excerpt): https://mobalytics.gg/overwatch/heroes/lucio-guide
- "Allow Backwards Wall Ride" setting lets him ride without facing the wall (excerpt, same page).
- Camera tilt: not found. Ceiling / slope limits: not found. Overwatch 2 "wall ride momentum" patch line: not found
  (searched OW2 season notes; only OW1-era lines came back).
- CTF: activating Wall Ride drops the flag (Feb 2018): https://us.forums.blizzard.com/en/overwatch/t/pc-overwatch-patch-notes-february-13-2018/24

## 2. Skill tech and why it feels good

- Wall jumping off a ridden wall, detach / reattach chains to go faster, late jumps around corners, corner climbing
  (excerpt): https://mobalytics.gg/overwatch/heroes/lucio-guide
- Criss-cross wall climbing between two walls: https://us.forums.blizzard.com/en/overwatch/t/lucio-wall-climb-mechanic-question-intended/720537
- Rollouts (practised wall-ride routes off spawn) and practice codes exist: https://dotesports.com/overwatch/news/best-custom-game-codes-to-practice-lucio-wall-riding-and-rollouts-in-overwatch-2
  (page blocked; title only) and a "floor is lava" route community: https://nyc1.lr.ggtyler.dev/r/LucioRollouts/comments/bcmagh/floor_is_lava_blizzard_world_tour
- Why it feels good: a sourced analysis of sound / flow was not found. What the rules imply (inference, not a source): no
  timer means the player, not a meter, ends the ride; the exit boost rewards leaving, so chains accelerate.

## 3. Open-source recreations

- A Lucio-specific repo: not found (GitHub search "lucio wall ride": 0 results): https://api.github.com/search/repositories?q=lucio+wall+ride
- Copyable licences, wall-run related (approach not documented in their READMEs; code not read in this budget):
  - JamesSayegh/Godot-Quake-wallrun-prototype, Apache-2.0, Godot 3, Quake air strafing plus wall run (closest to our
    movement): https://github.com/JamesSayegh/Godot-Quake-wallrun-prototype
  - Motta7000/unnamed-movement-fps, MIT, Godot 4.2 / 4.3, wall run only on marked (yellow) walls: https://github.com/Motta7000/unnamed-movement-fps
  - lastgeek/WallRunUE4, MIT, UE4 C++: https://github.com/lastgeek/WallRunUE4
  - AAScherbakov360/Advanced-FPS-movement.-Unity, Apache-2.0: https://github.com/AAScherbakov360/Advanced-FPS-movement.-Unity
  - ProjectBorealis/PBCharacterMovement, MIT, HL2 movement (no wall run listed): https://github.com/ProjectBorealis/PBCharacterMovement
- Reference only (no licence): ozata/unity-ultimate-titanfall-2-wallrun, Lahampsink/WallrunTutorial, Pavel-Konarik/ReplicatedMovementWallrun
  (listing): https://api.github.com/search/repositories?q=wallrun&sort=stars&per_page=15
- Surface-normal tracking, stick force and curved-wall following as documented techniques: not found in these repos.
  Note for us: rule 4 of CLAUDE.md bans PhysX for gameplay, so any approach must be rebuilt on our own collider rays anyway.

## 4. Level design for wall riding

- Titanfall 2: Respawn said some maps are "very heavily focused on chaining wall runs together": https://gamingbolt.com/titanfall-2-will-have-maps-that-will-heavily-focus-on-chaining-wall-runs
  ; walls read as possibilities for movement (search excerpt): https://www.gamesradar.com/titanfall-2-review
- Mirror's Edge: flow from wide corridors and placed obstacles; Runner Vision colours the route (search excerpt; source page
  now missing): https://lucasdiab.myportfolio.com/mirrors-edge-level-analysis
- Ghostrunner: levels chain wall runs, slides and grapple swings in quick succession: https://gamingtrend.com/a-speedrunners-dream-ghostrunner-review/
- Marked ride walls (a colour code) used by an open template: https://github.com/Motta7000/unnamed-movement-fps
- Overwatch map placement rules for wall ride, Neon White wall rules, numeric wall lengths / gaps / chain spacing: not found.

## A Lucio-style wall ride spec for our game

Every new number goes in a Unity-side config file (CLAUDE.md rule 5), never hand-typed. Proposed values are ours to tune,
not Blizzard's, unless cited.

1. Hold to ride, no timer: ride while Jump is held; drop the 1.2 s cap (or set it very long behind a switch). Basis: "as
   long as it is held": https://overwatch.fandom.com/wiki/Wall_Ride
2. Release jump: releasing Jump on the wall fires the wall jump with a speed boost; Crouch drops off with no boost; a 0.2 s
   crouch-then-release grace. Basis: https://overwatch.blizzard.com/en-us/news/patch-notes/ptr/2020/10
3. Ride speed buff plus exit boost: a percentage buff while riding (Lucio's history is 20 to 40 %) and a separate exit
   boost on leaving, so chains gain speed. Basis: https://overwatch.blizzard.com/en-us/news/patch-notes/live/2017/10 and
   https://west-games.com/overwatch-lucio-changes/
4. Jump direction from look pitch: looking up gives a much higher vertical exit than looking down (wiki 9.12 vs 4.12 m/s).
   Basis: https://overwatch.fandom.com/wiki/L%C3%BAcio
5. Follow corners: keep riding through inside and outside corners and along curved or faceted walls (re-sample the wall
   normal each step), with a minimum hold time before corner-following engages. Basis: https://www.over.gg/8166/overwatch-patch-notes-for-may-3-2018-rialto-and-hero-tweaks
6. Re-ride rule: the same wall (or same-facing wall) is allowed again only after landing, after riding a differently
   facing wall, or after leaping far enough away. Basis: the May 2018 notes above and https://mobalytics.gg/overwatch/heroes/lucio-guide
7. Late-jump window: a few steps after detaching still count as a wall jump (window length: ours, in whole 120 Hz steps).
   Basis: https://mobalytics.gg/overwatch/heroes/lucio-guide
8. Gravity on the wall: Lucio's value was not found; keep our 0.15 multiplier as the start point, consider a pitch-based
   climb (look up = rise) to match the climbing tech: https://us.forums.blizzard.com/en/overwatch/t/lucio-wall-climb-mechanic-question-intended/720537
9. Entry speed: Lucio has no documented minimum speed (not found); consider lowering our 8 m/s gate so a plain jump into a
   wall attaches. Basis: start rule above, https://overwatch.fandom.com/wiki/Wall_Ride
10. Short cooldown only: 0.5 s between rides (excerpt), not a long one; it is a traversal state, not a crowd-control verb.
    Basis: https://overwatch.fandom.com/wiki/Wall_Ride
11. Camera tilt: not found for Lucio; any tilt is our own feel choice and must be measured on screen (lesson L-001).
12. Arenas: build chains of ride walls with differently facing faces (so the re-ride rule allows a chain), corners and
    round pillars to follow, and mark ride walls with a colour (cyan is player-usable per our palette). Basis:
    https://gamingbolt.com/titanfall-2-will-have-maps-that-will-heavily-focus-on-chaining-wall-runs and
    https://github.com/Motta7000/unnamed-movement-fps
13. Boss fights: a ring of ride walls or pillars around the arena so the player can circle the boss off the floor;
    inference from the corner-following and pillar rules, no source found for boss-arena use.
14. Proof: one Player proof line per rule (L-014); the Movement-exact rule means any change to `Mover.cs` needs a task that
    names it and new golden vectors.

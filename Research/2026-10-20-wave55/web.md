# NIGHT55 A3 (a): web research. How shipped games and published talks solved tonight's problems

Written 2026-10-20 17:05-17:40 by the web researcher (read-only; skygrind-idea-lab, step 1 "Research"). The input was the lead's
8 items plus the top items of `critic-veteran.md`, the only critic file there at 17:31. `critic-gg.md` and `critic-first.md` did not
exist yet.

**How to read the marks:**
- **Verified** means I read the page, or downloaded and read the source file, this session.
- **Reported** means the claim comes from a search-result summary, or from another page that cites it, and I did not read it myself.
  Several wikis (doomwiki, breezewiki, zeldadungeon, Valve's wiki) and some press sites returned 403 / 402 / bot walls.
- A number appears only when a page gives it. Everything else is "not found" or "not checked".
- Searches used generic game-design terms only. Nothing from the repository went to the web.

**Not repeated here** (already in earlier research):
- the hitstop ladder and ULTRAKILL's hitstop values: wave42 `hitfeel-web.md`, wave43 `combat-web.md`
- boss intros (GabrielIntro.cs, HudMessage): wave43 `bosses-web.md`
- arena identity: wave45 `arena-identity.md`
- TF2 / L4D readability: wave43 `ui-art-web.md`

## 0. Our data first (read this session)

- **grade54:** a second Volume per boss (colour filter, two-tone split, saturation / contrast / exposure) and a 0.55 s turn. THE WICK
  and THE CROWN ship ungraded, because every grade put the frame's mean hue within 1-14 deg of the danger red (D-16). That was measured
  on the whole frame, red tells included.
- **move54's step smoothing** (`Assets/Scripts/Game/Move54.cs`, `Move54Step`; numbers in `Design/unity-config.json`):
  - a critically damped spring, omega 40 rad/s: 9 % of the held offset left at 0.1 s, 2 % at 0.15 s
  - stepMin 0.1 m, keepRate 4 m/s, maxOffset 0.4 m (= collision.stepHeight), resetJump 1 m, a gate of gateFrames
  - camera only; grounded at both ends, on the ground or sliding
  - **unmeasured:** the a354 run line had no step on it (NIGHT54-REPORT).
- **The lead's beyond list:** finish55, brief55, clear55, speedKill55 + chain55, ink55; eclipse55 "needs a post-grade tell pass".
- **The veteran critic's worst items:**
  - #1: THE WICK is a damage race. 6 % of its damage lands inside punish windows; 298 of 397 hits come from hot walls.
  - #2: kills are 3-frame pauses on undamaged pills.
  - #3: the ride pays no speed.
  - #4: rooms stall between waves.
  - #6: the danger hue is used as wallpaper.
  - #7: the style meter is a label, not a loop.
  - #8: tells are sentences, not body language.
  - #9: the FOV follows button presses, not speed.

---

## 1. A boss colour turn on a warm arena whose danger tells are red-orange (T-133; D-16; critic #6)

| source | what it says (numbers as given) | mark |
|---|---|---|
| Unity URP, Shadows Midtones Highlights: https://docs.unity3d.com/Manual/urp/Post-Processing-Shadows-Midtones-Highlights.html | a trackball (hue shift) and a lightness slider each for shadows, midtones and highlights; Shadow Limits Start / End and Highlight Limits Start / End set the transitions; a graph shows each range's contribution | Verified |
| the same override, defaults from an older URP package source (search result) | shadows end 0.3, highlights start 0.55, highlights end 1 | Reported |
| Unity URP, Split Toning: https://docs.unity3d.com/Manual/urp/Post-Processing-Split-Toning.html | Balance sets which tone dominates; "Value also changes the overall image brightness", so pick only hue and saturation | Verified |
| Unity URP, Color Curves: https://docs.unity3d.com/Manual/urp/Post-Processing-Color-Curves.html | Hue Vs Sat "adjusts saturation according to the input hue"; Lum Vs Sat "desaturates dark areas"; Hue Vs Hue remaps hues | Verified |
| Unity URP, injection points: https://docs.unity.cn/Manual/urp/customize/custom-pass-injection-points.html | AfterRenderingPostProcessing runs "after rendering post-processing effects but before the final blit" (the post includes colour grading) | Verified |
| Unity forum (search result) | a Render Objects feature on a layer can draw that layer twice (once in its normal pass); check it in the Frame Debugger | Reported |
| W3C WCAG 2.1, SC 1.4.11 Non-text Contrast: https://www.w3.org/WAI/WCAG21/Understanding/non-text-contrast.html | at least **3:1** against the adjacent colours; test "the least contrasting area"; ratio (L1 + 0.05) / (L2 + 0.05) | Verified |
| Xbox Accessibility Guideline 103: https://devdocs.xbox.com/gaming/accessibility/xbox-accessibility-guidelines/103 | "aim to have no cue depend on color alone"; colour content should also be "expressed through shape, pattern, icon, or text label" | Verified |
| DOOM Eternal boss tips (Bethesda): https://slayersclub.bethesda.net/en-AU/article/doom-eternal-boss-tips | the Marauder's "eyes briefly flash green" before the swing, "your window to strike"; the Gladiator: "shoot him when his eyes flash and you'll stagger him" | Verified |
| Splatoon 3 Splatfest: https://automaton-media.com/en/news/20221112-16667/ | night lighting gave every ink a neon glow, and blue ink made opponents and edges hard to see (a lighting change hid a gameplay colour) | Reported (I read only the headline) |
| Hyper Light Drifter, Alx Preston (Game Informer reprint): https://gameinformer.com/games/hyper_light_drifter/b/pc/archive/2014/03/14/developer-covers-key-aspects-hyper-light-drifter | "Each environment uses a striking and specific palette to highlight landmarks." | Verified |
| JoJo's palette switches: https://www.meer.com/en/106245-hirohiko-araki-and-color-without-rules ; https://ballstatedaily.com/article/2019/02/review-jojos-bizarre-adventure-vento-aureo-episode-19-white-album | Araki changes colours on purpose; recognition comes from structure, and colour carries mood. The anime is "infamous for switching color palettes for dramatic effect" | Reported |

**Methods for SKYGRIND:**

1. **Grade by tonal range, not the whole frame (THE WICK and THE CROWN).**
   - Use URP's Shadows Midtones Highlights instead of a full-frame filter. Push the shadows' trackball to violet-indigo and leave the
     midtones and highlights neutral.
   - Keep Shadow Limits low (the old default ends at 0.3). Then the bright emissive tell pixels sit in the untouched highlight band.
   - Add a Lum Vs Sat curve that desaturates only the darks. The walls go cool and grey while the emissives keep their saturation.
   - Make the turn's 0.55 s a **value** move (world exposure and shadow saturation down), never a hue rotation of the frame.
   - **Measure** on a454 frames, with pixels split into wall pixels and tell pixels:
     - the walls' mean hue is at least 30 deg from the danger hue (D-16's next try; the critic's line)
     - tell pixels move at most 5 deg in hue, graded vs raw
     - each tell against its adjacent wall reaches at least 3:1 luminance (WCAG 1.4.11's number)
     - RED with the row off.
2. **Draw the tells after the grade.** This is eclipse55's prerequisite, and it frees every boss grade at once.
   - Put the tell meshes on their own layer, drawn by a Render Objects feature at AfterRenderingPostProcessing. No grade, not even a
     black-and-white eclipse, can then move a tell.
   - Check in the Frame Debugger that the layer is not also drawn in the opaque pass.
   - Cost: after-post draws skip bloom and tonemapping, so the tells need their own glow. Decide whether they keep the PS2 treatment
     (V-6): it is not checked whether dither and colour steps run in post here.
   - **Measure:** tell-pixel RGB within 1/255 between a graded frame and an ungraded one while the world changes; RED with the feature
     off.
3. **Take the danger hue out of THE WICK's décor before grading** (V-1, V-2; critic #6).
   - The orange stripes become the level's accent at a lower value and saturation, or value-only stripes.
   - A hot wall shows its state only while it burns, as the most saturated thing in the frame.
   - This is the Marauder's green: the "act now" cue is a colour the arena never uses.
   - Add a second channel per XAG 103: a hot wall also changes pattern (scrolling heat bands), not only hue.
   - **Measure:** red-orange is at most 10 % of a454 frames with no hot wall (the critic's line); hot-wall hits are at most 30 % of hits
     (n47human).

## 2. Boss arena readability: "you just teleport into these random rooms ... hard to understand what to do"

| source | what it says | mark |
|---|---|---|
| Furi, Game Developer: https://www.gamedeveloper.com/design/furi-some-tough-design-decisions-striving-to-make-a-memorable-game | 3-4 minute walks between bosses (with an autowalk button) give "backstory and coaching" and build tension; each reveals "a visually unique environment teasing the upcoming boss personality" | Verified |
| DOOM Eternal boss tips (above) | the tell is the window: green eyes = strike now; "Attack too soon and the Marauder will deflect it with his shield"; the Gladiator banging his mace on his shield = a heavy follow-up | Verified |
| "Hades II Has a Legibility Problem": https://nex-3.com/blog/hades-ii-has-a-legibility-problem/ | in Hades I, melee hit a hitbox round the boss and ranged attacks flew out from it, so you read the body and set a rhythm. Hades II's red arena zones, "usually independently of the boss's positions", mean "the player is constrained by the areas the bosses allow them to exist in" | Verified (a critic's essay) |
| Psychonauts 2, Game Developer: https://www.gamedeveloper.com/marketing/using-a-modular-system-of-maneuvers-to-design-i-psychonauts-2-i-s-boss-fights-in-a-hurry | each attack "followed a pattern of telegraph, attack, and recovery"; bosses are maneuver chains; no timings given | Verified |
| ULTRAKILL, Parry: https://ultrakill.wiki.gg/wiki/Parry | "Yellow flashes indicate attacks that can be parried" (a 4-point spark for melee, 8-point for ranged); "Blue flashes indicate attacks that can't be parried"; a parry freezes the game, flashes white and fully restores stamina | Verified |
| Portal developer commentary: https://theportalwiki.com/wiki/Portal_developer_commentary | "without some serious prompting, players will rarely look up" (Jeep Barnett); "playtesters stubbornly refused to look up" (Bill Van Buren) | Verified (fan transcript) |

**Methods:**

1. **An approach room that previews the fight** (Furi).
   - The last space before each boss door shows the arena, or the boss's weak point, through an opening.
   - It lets the player perform the fight's one verb on a harmless object: ride the panel, punch the dummy core.
   - This turns "teleport into a random room" into "I saw where I was going". It fits brief55: the card's one verb line is the verb
     the player just did.
   - **Measure:** an entry frame with the boss or arena in view, and a telemetry line showing the verb fired before the door.
2. **Every attack is a maneuver row: tell, attack, recovery** (Psychonauts 2, DOOM).
   - The tell is on the body, at least 0.8 s for a big attack (E-4).
   - The recovery is the punish window, cued on the body in a hue the arena does not use. A core lit cyan fits cyan's meaning: "you can
     use this now".
   - The text sentence goes after the first 10 s (critic #8).
   - **Measure:** per attack, the tell-to-impact time from telemetry; at least 50 % of damage inside windows (critic #1); centre text on
     at most 10 % of frames after 10 s.
3. **Damage comes from the boss's body or from a state you can see, never from the arena as wallpaper** (the Hades I vs II essay).
   - Count the hits taken by source; walls at most 30 %.
   - For vertical bosses (THE WICK up a shaft, THE LOOM on a ceiling), Portal's "players will rarely look up" applies. Route the ride
     walls so they face the core, or raise the core into view on each leg.
   - **Measure:** the boss's body in at least 50 % of a454 frames (critic #1 / #5).

## 3. Kill crunch without audio (critic #2; speedKill55, chain55, ink55)

| source | what it says (numbers as given) | mark |
|---|---|---|
| Quake, QuakeC `player.qc` (id-Software/Quake on GitHub, qw-qc/player.qc; downloaded and read; GPL-2.0, so the method only, never the code) | the player gibs when health < -40 (in `PlayerDie`; each monster's file sets its own threshold, not read): 3 gibs and a head. Each gib starts at 0.5 x the inflictor's velocity + 25 along the hit direction, z 100 + 240 x random, x / y +-200, spin up to 600 deg/s per axis. Overkill tiers scale it: overkill under 50 x0.7, under 200 x2, else x10. Gibs live 10 + 10 x random s | Verified |
| ULTRAKILL, Health: https://ultrakill.wiki.gg/wiki/Health | close attacks heal: punch torso 10, limb 20, critical 30; Knuckleblaster gibbing 120, slam gibbing 120; Hard Damage is 35 % of the damage received | Verified |
| ULTRAKILL, Style: https://ultrakill.wiki.gg/wiki/Style | Double Kill = 2 kills within 0.5 s (+25); Triple Kill +50; MULTIKILL = more than 3 within 0.5 s (+100); Big Kill +100 / +125 | Verified |
| Vlambeer, "The Art of Screenshake" (Jan Willem Nijman), via https://infovore.org/?p=5275 and wave42 notes | per shot, the camera is kicked back from the aim point and 4 is added to the screenshake. A recreation's effect list includes Permanence, Camera Lerp, Screenshake, Player Knockback and Hit Pause (https://dkliao.itch.io/the-art-of-screenshake-recreation/devlog : 404 when I fetched it) | Reported |
| DOOM Eternal, destructible demons (Hugo Martin): https://bloody-disgusting.com/video-games/3515304/doom-eternal-creative-director-dishes-new-demon-destruction-system/ ; https://doomwiki.org/wiki/Destructible_demons | demons show the damage they take (flesh, bone, chunks); "purely visual flair" that still changes the feel; weak points disable attacks. In The Dark Ages the damage accounts for "distance, angle and overall damage" | Reported (both 403) |

**Methods:**

1. **Debris scaled by overkill** (Quake's tiers; critic #2's "at least 8 pieces lasting at least 1.5 s").
   - The chunk count and launch speed scale with the overkill as a share of max HP, and with the shot's direction and speed.
   - speedKill55 multiplies them by the player's speed.
   - Pieces persist 10-20 s, capped per room (Nijman's permanence; ink55).
   - **Measure:** debris visible 1.5 s after a kill (at least 8); a walk kill vs an 18 m/s kill frame pair (at least 2x); frame time.
2. **Damage states before death** (DOOM's destructible demons).
   - Two stages on the existing pill: a crack, then a piece knocked off, by HP fraction. It is a view change on existing models, not a
     new enemy design (rule 7), but show GG the pill with its cracks before shipping it.
   - **Measure:** changed pixels inside the body's box between the frame before a hit and the frame after.
3. **Escalate chains inside ULTRAKILL's 0.5 s window.**
   - Kills within 0.5 s of each other are one multikill. The hold grows per kill on C-2's ladder: the 1st 0.05 s, the 3rd and later at
     least 0.08 s, under killCap 0.155 s.
   - chain55's "3 kills in 1.5 s" is three times looser than ULTRAKILL's window. Pick one and log it in DECIDE55.
   - **Measure:** the a354 telemetry's holds per kill, and jitter at most 0.191.

## 4. Small, action-packed arenas with a movement line (critic #4 / #5; T-128, T-131)

| source | what it says (numbers as given) | mark |
|---|---|---|
| ULTRAKILL, The Cyber Grind: https://ultrakill.wiki.gg/wiki/The_Cyber_Grind | a 16x16 grid, each cell its own height (1 unit = half a cube's width); 38 official patterns shuffled per run; the grid re-patterns at each wave, with health, stamina and cooldowns reset before the spawns; jump pads per pattern run from 0 ("Skatepark") to 64 ("Pits"); common enemies spawn 0.1 s apart | Verified |
| DOOM (2016), Marty Stratton: https://www.gamewatcher.com/2015-07-07-doom-s-arenas-are-almost-like-skate-parks-according-to-exec-producer | "We refer to these arenas almost like skate parks", "improvising your way through this combat dance"; climb to high ground, fire down, drop when they close | Verified |
| DOOM Eternal making-of, Shacknews p8: https://shacknews.com/article/116635/hell-razer-the-making-of-doom-eternal?page=8 | "Doom Eternal's designers purposefully tie pacing to content"; "combat is another type of puzzle" | Verified |
| the same, p6 (search summary) | combat arenas and traversal puzzles (climbable walls, monkey bars) are building blocks rearranged in greybox | Reported |
| "Design of Doom Eternal" (adamatomic) | tall arenas come in two kinds: "jungle gyms" with distinct floors, and open "canyons" | Reported (403; cited in wave42) |
| Titanfall 2, GDC 2018: https://gdcvault.com/play/1025105/Designing-Unforgettable-Titanfall-Single-Player | "Early development was based on comfortable techniques and led to familiar results"; "action blocks", a rapid-prototyping technique | Verified (the abstract only) |
| The Level Design Book, circulation: https://book.leveldesignbook.com/process/layout/flow/circulation | "most maps use only 1-3 lanes"; lanes should differ from each other (length, what sits on them) | Verified |

**Methods:**

1. **The arena changes state between waves** (the Cyber Grind).
   - A combat room carries 2-3 authored patterns: blocks raised or lowered, pads moved. It re-patterns between waves behind a short
     tell.
   - Each pattern keeps the loop, a climb route for enemies and the ride line. This is A-9's "state change" field, and it gives a
     small room three 30-second problems.
   - **Measure:** the identity checks and the walk bot on each pattern; one entry frame per pattern.
2. **A block library with jobs** (DOOM Eternal's greybox blocks, Titanfall's action blocks).
   - 5-8 blocks, each with its reason string (A-1): a ride-wall pair at M-5 spacing; a pad with its landing; an enemy climb ramp; a
     hazard lip; a sightline breaker; a perch over the hazard for flyers (R-4).
   - New rooms are cut from blocks. Any block the walk never touches is cut (T-131: pieces with jobs, not only cuts).
3. **One to three lanes, and they differ** (LDB, plus Neon White in section 8).
   - The ride lane is the shortcut, never beside an equal walking lane. This is I2 (walk at least 1.3x the ride; T-128 left 7 rooms).
   - **Measure:** I2 per room, and the ride pilot at least 20 % faster than the walk pilot (critic #3).

## 5. Camera smoothing on stairs and step-downs (T-133: move54's step-down unmeasured)

| source | what it does (numbers as given) | mark |
|---|---|---|
| Source SDK 2013, `CBasePlayer::SmoothViewOnStairs`, `src/game/shared/baseplayer_shared.cpp` (ValveSoftware/source-sdk-2013 on GitHub; downloaded and read; Source SDK licence, so the method only) | "Handle view smoothing when going up or down stairs." It runs only when the ground entity's movetype is NONE ("Don't want to do this when the ground entity is moving the player"), when z changed, and when the view offset is unchanged (no crouch change). A constant slew of `150 * dir` units/s; the lag is clamped at `stepSize = 18.0f`; the eye's z adds (smoothed z - feet z); otherwise it resets. ConVar `smoothstairs` 1 | Verified |
| Quake, `WinQuake/view.c`, `V_CalcRefdef` (id-Software/Quake; read; GPL) | "smooth out stair step ups" only, and only when on the ground: `oldz += steptime * 80`; clamped to the feet; the lag clamped at 12 units; otherwise oldz = the feet | Verified |
| Valve wiki units (search summary) | 1 unit = 0.01905 m at map scale (16 units per foot); a player step is 18 units | Reported |

On those units, Source slews at about 2.9 m/s with its lag capped at about 0.34 m: a full step catches up in about 0.12 s.

**What this says about ours.**
- move54's shape matches Source's: camera only, a lag capped at one step height (maxOffset 0.4 m = stepHeight), grounded at both
  ends, and a catch-up of about 0.1 s.
- The differences:
  - Source slews at a constant rate; ours is a spring.
  - Source turns itself off on moving ground and on a view-height change. Whether move54 excludes moving platforms: not checked.
    Slides are included by design.
  - Quake never smooths a step down. Ours does, and that matters most: groundSnap drops 0.35 m in one step.

**Methods:**

1. **Measure before changing anything.** Build a354's step line on a real stair: 3-5 risers of 0.15-0.4 m, inside a354's own window
   (Move54ReelStepMin 0.15 to Max 0.4, from 4 m after the start to 3 m before the end), both up and down.
   - Print per frame: feet y, camera y, offset.
   - **GREEN:**
     - the largest one-frame camera change is at most a set share of the step (a354's existing line)
     - the offset is never above maxOffset across a run of risers (Source's clamp: no build-up on a staircase)
     - the offset is under 2 % 0.15 s after the last riser
   - **RED** with move54 off: a one-frame change equal to the whole step.
2. **Add Source's two gates if they are missing.** No smoothing while the ground is moving the player (a lift or a moving platform),
   and none on a frame where the eye height changes for another reason (a slide's height change). Both are view-only: rule 3 is safe.
3. **Keep the step-down smoothing** (Source does it; Quake does not). At 15 m/s the snap-down pop on step-downs is the one the player
   sees most.

## 6. Level décor colour that stays readable (decor54 "subtle on screen")

| source | what it says | mark |
|---|---|---|
| Mirror's Edge Catalyst, Runner's Vision (EA): https://www.ea.com/amp/news/runners-vision-in-mirrors-edge-catalyst | "Follow the Red": objects in front of the player are highlighted and a red trail guides; three settings (Off, Classic, Full View); off the route it "won't suggest a 180-degree turn" | Verified |
| Mirror's Edge (2008), Runner Vision (Wikipedia / reviews via search) | usable objects (pipes, springboard planks) are coloured red; "white things fade to red as you approach them" | Reported |
| DOOM Eternal, Hugo Martin: https://www.gamedeveloper.com/design/how-doom-eternal-owns-the-quot-contrivances-quot-of-video-games | on bright pickups: "That visual feedback, in a split second, tells you, 'You're doing the right thing.'"; "you need to be able to know from across the room, 'That's plasma ammo.'"; "You knew a health pack from across the room." | Verified |
| Hyper Light Drifter (section 1) | one palette per environment, chosen to highlight landmarks | Verified |
| Splatoon, GDC 2018 coverage: https://nintendowire.com/news/2018/03/21/splatoon-prototype-shown-off-gdc/ | neutral black-and-white characters were chosen to contrast with bright inks | Reported |

**Methods:**

1. **Approach-lit route strips** (Runner Vision). The ride wall's cyan strip stays dim at range and brightens as the player nears it
   or looks at it. decor54 can then stay quiet without losing the route, and the waist-high cyan dashes stop being permanent wallpaper.
   - **Measure:** the same ride wall at 20 m and at 6 m, with the strip's luminance ratio against the wall rising (target at least 3:1
     at 6 m).
2. **The "from across the room" test, per level** (Martin, plus WCAG's 3:1).
   - From a room's entry view at about 30 m, the ride strip, the pad and a pickup each reach at least 3:1 against what is next to them.
   - The level accent stays below that.
   - Run it beside the greyscale check (E-3).
3. **One accent per level, chosen to show the landmark** (HLD). The accent sits on A-6's landmark and the lit exit, not spread over
   every edge. It answers decor54's "brass edges nearly vanish into the walls": fewer accent pieces, and brighter ones.

## 7. A finisher or combo reward (finish55; critic #7 "the style meter is a label")

| source | what it says (numbers as given) | mark |
|---|---|---|
| ULTRAKILL, Style: https://ultrakill.wiki.gg/wiki/Style | the ranks D 200, C 300, B 400, A 500, S 700, SS 850, SSS 1000, U 1500 points; passive decay from 15/s (D) to 120/s (U). The rank speeds Hard Damage recovery (A x1.0, S x2.0, SS x2.5, SSS x3.0, U instant). Weapon freshness: FRESH (7.5-10) x1.5, USED x1.0, STALE (1-5) x0.5, DULL (0-1) x0; each attack takes 0.7 off that weapon and adds 0.5 to the others; holstered weapons regain 0.1/s. Taking damage drops one rank | Verified (the page warns that its rule text and table do not line up cleanly) |
| Bulletstorm (Wikipedia): https://en.wikipedia.org/wiki/Bulletstorm | "Points are used as currency at dropkits" for guns, ammo and upgrades; the leash and the kick / slide put enemies in slow motion to set up skillshots | Verified |
| Bulletstorm reviews (search) | repeated skillshots give diminished returns | Reported |
| Smash Bros, Finish Zoom: https://www.ssbwiki.com/Finish_Zoom | it fires when "the game calculates an attack will likely result in a game-winning KO"; it zooms on the impact point and briefly slows time over a red background (blue for Special Zoom); no frame numbers on the page | Verified (fan wiki) |
| DOOM Eternal, glory kill (doomwiki via search; wave42 notes) | invulnerable for the whole animation in Eternal (only at the start in 2016); glory kills always drop health; DOOM 2016's are "hundreds of milliseconds" (Robert Duffy) | Reported |

**Methods:**

1. **finish55 as a Finish Zoom.**
   - The sim knows the boss's HP before the last hit lands. When the hit will kill, zoom on the impact point and hold.
   - The background flash takes the boss palette's turn hue, never white (C-6).
   - **Measure:** the hold is at least 0.8 s of real time on a boss-death frame, with the turn's hue shift (the lead's line); RED off.
2. **Style that pays, through something the game already has** (Bulletstorm's points as currency; ULTRAKILL's rank-scaled recovery).
   - The rank multiplies the points you earn, or speeds heal36's spill.
   - Apply ULTRAKILL's freshness to verbs: repeat the same verb and its style falls (x1.5 / x1 / x0.5 / x0); vary them and it rises.
   - **Rename the resting rank.** "DULL" is ULTRAKILL's word for an over-used weapon, not a rank; the critic saw "D DULL" on screen
     during a ride chain.
   - **Measure:** a rank histogram per level run (at least 30 % of combat at B or above for a skilled pilot); one ride, wall jump and
     kill chain moves the rank one letter.
3. **execute55 (held back by the lead):** the sources give no stagger length in seconds (not found). If it is built later, DOOM 2016's
   "hundreds of milliseconds" is the only published bound on the animation.

## 8. A room that teaches a verb by itself (brief55, T-4, T-5)

| source | what it says | mark |
|---|---|---|
| Koichi Hayashida, Game Developer: https://www.gamedeveloper.com/design/the-secret-to-i-mario-i-level-design | the four steps: "First, you have to learn how to use that gameplay mechanic", then develop it, then "something crazy happens that makes you think about it in a way you weren't expecting", then "demonstrate ... what sort of mastery you've gained"; and "a clear concept in the beginning, and that's carried through absolutely all the way" | Verified |
| Portal developer commentary (section 2) | the first portal is placed so "players will invariably see themselves"; start in "a visually unique room with memorable objects" as a reference; fling is taught by "a pushed out concrete block above a pit with checkerboard concrete tile" and "slowly, step by step"; "fatigue set in if we didn't break up the more complicated, deliberately paced puzzles" with "a much simpler task under time pressure" | Verified |
| Half-Life 2, Ravenholm: https://wnhub.io/news/other/item-11774 | first a zombie already cut in half by a saw; then blades block the way; you pull one free with the gravity gun; a zombie appears; "In just 10 seconds, Valve taught the player that circular saws are great against zombies." No text | Verified (a commentator, not Valve) |
| Neon White, Vice: https://vice.com/en/article/neon-whites-developers-being-too-good-at-their-own-game-caused-real-problems | levels under a minute, often under 30 s. Silver = "the intended path with no optimizations and some mistakes"; Gold = no mistakes; Ace = "using the designed shortcut". The developers' times were too fast, so the ghost recorder added small errors "to simulate hesitations". Every level has "at least one shortcut that saved a significant amount of time" | Verified |

**Methods:**

1. **Show the result before the verb** (Ravenholm).
   - The room's entry view shows an enemy already beaten by the verb: a husk thrown onto a ledge by the pad, or one slammed at the end of
     a ride.
   - Then one obvious setup. Then the first live enemy placed so the verb is the natural answer. No text card.
   - **Measure:** the walk bot (and the first-time critic's read) fires the verb within 10 s of entering, with the text off.
2. **Four beats per verb across rooms** (kishōtenketsu).
   - Learn (safe, the small version, T-5), develop (bigger), twist (the verb used another way: RIDE KILL, a ride to escape a hazard),
     conclude (the boss, P-1).
   - Write the four beats as a column in docs/ROOMS.md per verb; a missing beat is a gap to build.
   - Portal's fatigue rule fits between beats: a simple timed task after a hard room.
3. **Self-confirming first use, at eye level** (Portal).
   - The first success changes the room where the player is already looking: a door opens, a light comes on (T-4's confirm).
   - Never above the player's eye line on a first use ("players will rarely look up").
4. **clear55's par from a ghost with hesitations** (Neon White).
   - The par comes from the n47human bot (aim and reaction delays), never from a best run.
   - Medals: silver = the intended path with mistakes, gold = no mistakes, ace = the room's shortcut. Every room needs one ride or pad
     shortcut that saves real time, which is I2 again.

---

## 9. The veteran critic's other top items

**V1. "The ride pays no speed" (critic #3).**
- Titanfall 2's wall-run numbers: no official source found. A student project's Titanfall-like constants (cse125.ucsd.edu, in Quake
  units, Reported) are not Respawn's numbers. Do not cite them as Titanfall's.
- If built, it is a sim row: Mover.cs only by a named task, golden 0 and 22 exact with it off (rule 3).
- **Measure** with the critic's line: the ride speed trace rises from entry toward a cap over about 1 s.

**V2. Waves stall (critic #4).**
- CoD Zombies keeps a cap of 24 zombies alive for one player (+6 per extra player) and spawns from the round's queue as each one dies
  (callofdutyzombies.com forum; Reported).
- Halo's encounter scripts wait on `ai_living_count` of an encounter (halomaps forum; Reported). Waiting for N > 0 instead of 0
  overlaps the waves.
- The Cyber Grind spawns common enemies 0.1 s apart (Verified, section 4).
- **Method:** start the next wave when at most 2 of the current wave remain, with spawn tells in view.
- **Measure:** the flow table: stall at most 15 %, door wait at most 5 s, dead time at most 8 s (the critic's lines).

**V3. The FOV follows button presses, not speed (critic #9).**
- GDQuest (https://www.gdquest.com/library/glossary/field_of_view, Verified): a wider FOV "make[s] the player feel like they're moving
  faster"; "Many games subtly increase the FOV when the player sprints or drives a vehicle"; 75-90 deg is common for FPS.
- Racing sims' "speed sensitivity" narrows the FOV at low speed and widens it at high speed (gtplanet forum; Reported).
- **Method:** FOV = base + k x max(0, horizontal speed - sprint), capped and eased. k and the cap go in UnityTuning with comments
  (rule 5). Keep LT's zoom as world FOV only (the w33 "never again").
- **Measure:** FOV against speed correlates at 0.9 or more on vf's telemetry; the FOV at 24 m/s is never below the 15 m/s ride's.

**V4. The style meter (critic #7):** see section 7, method 2.

## 10. The methods ranked for tonight (by what they unblock)

1. **Tells drawn after the grade** (1.2): one renderer feature frees THE WICK's and THE CROWN's grades and eclipse55, and makes "tells
   untouched" a pixel test.
2. **Shadows-only grade with a value turn** (1.1), measured on wall pixels vs tell pixels with WCAG's 3:1.
3. **The step-down measurement on a real stair** (5.1), plus Source's two gates (5.2).
4. **Debris by overkill and chains in a 0.5 s window** (3.1, 3.3), from Quake's tiers and ULTRAKILL's multikill.
5. **Tell / attack / recovery rows with the window on the body** (2.2) and the approach-room preview (2.1), for brief55.
6. **Freshness on verbs and a rank that pays** (7.2).
7. **Show the result before the verb** (8.1), and clear55's par from a ghost with hesitations (8.4).

## 11. Not found / not checked

- Numbers not found:
  - Titanfall 2's wall-run and slide-hop speeds (official)
  - DOOM's stagger length in seconds
  - the Smash Finish Zoom's slowdown factor
  - the frame counts of ULTRAKILL's parry flash
  - Hades' telegraph colour rules from Supergiant itself
- Sources found but not read:
  - an id Software or Supergiant primary source on keeping danger reds readable in a red level
  - the Art of DOOM Eternal book
- Whether ULTRAKILL tints its levels per layer: not checked.
- In this repository, not checked: whether move54 excludes moving platforms, and whether the PS2 treatment runs in post (it matters
  for 1.2).
- Licences: the Source SDK 2013 licence and Quake's GPL-2.0 allow reading only. No code from either goes into the project; the
  methods above are re-derived.

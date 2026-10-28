# Hit feel in fast FPS: web research (2026-10-14, wave 42)

Method: web search plus page fetches. Each claim carries its URL. "Not found" means I searched and no source gave it.
Two sources are low-grade blogs (marked [blog]); treat their numbers as ballpark. Two wiki pages (ULTRAKILL Hitstop, DOOM
Blood Punch / Meat hook / Glory kill) blocked direct fetch, so figures from them come from search-result summaries (marked [snippet]).

## 1. Hitstop and freeze frames

- ULTRAKILL has two hitstop tiers; time and music both stop. [snippet] Major = 0.25 s (parries and interruptions, punching a
  cannonball, shooting a rocket / Core Eject grenade with hitscan). Minor = "less than a quarter second" (Feedbacker punch hit,
  Knuckleblaster punch hit, a Piercer / Sharpshooter / Railcannon shot passing through an enemy, a rocket hitting an enemy,
  punching any punchable projectile). The minor value itself was NOT found. https://breezewiki.discard.no/ultrakill/wiki/Hitstop
- A successful ULTRAKILL parry = hitstop plus a bright white screen flash (the flash can be turned off in options).
  https://ultrakill.wiki.gg/wiki/Parry and https://ultrakill.wiki.gg/wiki/Arms
- Nuclear Throne (Vlambeer): the game freezes "about 10-20 milliseconds whenever you hit something". Same talk: kick the camera
  back 6 px and add 4 to screenshake per shot; hit enemy knockback 3 px per frame; enemy hit anim = 1 white frame, then 2 frames
  of "hurt, big eyes"; props stay after fights ("permanence"). https://infovore.org/?p=5275
- General guidance [blog]: hit stop weak 0-0.03 s, strong 0.05-0.08 s, critical 0.10-0.15 s; camera shake impulse weak 0.1-0.2,
  strong 0.3-0.5, critical 0.6-1.0; hit flash 0.08 s. https://uhiyama-lab.com/en/notes/unity/unity-game-feel-hit-feedback/
- Another [blog]: flash 50-100 ms (30 ms unnoticed, 150 ms blurry); hit pause 0.05-0.10 s; shake duration matches the flash;
  weak shake 2-3 px, strong 8-10 px; 20-30 particles, 0.5-1 s life. Layering order: sound + vibration at t0, flash +20 ms,
  particles +50 ms, floating text +100 ms. https://eastondev.com/blog/en/posts/dev/20260521-game-feedback-feel/
- Search summary of fighting-game practice: heavy attacks 6-10 frames full stop; 12-18 frames for boss supers; freezing 50-100 ms
  on a heavy hit reads as force; the point is layering several effects at one instant. (Weak source; aggregator.)
  https://gamineai.com/blog/pixel-art-combat-fx-hitstop-smear-frames-impact-timing-2026
- Swink's "Game Feel": input response should be no more than ~100 ms to feel comfortable (secondary citation).
  https://Www.wikipedia.org/wiki/Game_feel

## 2. Enemy flinch, stagger, knockback, gibs

- DOOM 2016/Eternal stagger: damaged demons enter a stagger state flagged by a colour highlight (blue in one source, orange in
  another for the glory-kill state); the stagger is the setup for a glory kill. Durations in seconds were not found.
  https://steamcommunity.com/app/782330/discussions/0/2143092024488011542 (search summary) and
  https://uxdesign.cc/the-user-experience-of-doom-eternal-af2ce5afa8f8 (visual damage feedback per body location)
- DOOM 2016 glory kills are deliberately short: CTO Robert Duffy: "strict time limits ... hundreds of milliseconds, just because
  you want to keep the player moving"; kills drop more health and supplies; "a hit of adrenaline every time" (Stratton).
  https://bethesda.net/en-EU/news/22-reasons-to-love-doom-part-1 . A search summary gave 600-700 ms; I could not confirm it on
  the page. DOOM Eternal's are longer: about 1.6 s (imp-size), 2.1-2.3 s (Revenant, Baron) per a guide summary; the Slayer is
  invulnerable for the whole animation. https://twinfinite.net/guides/doom-eternal-glory-kills/ and https://doomwiki.org/wiki/Glory_kill
- ULTRAKILL Knuckleblaster: 2.5 damage, "high knockback launches light/medium enemies", a downward punch splatters the enemy on the
  ground, small enemies killed "burst into a large shower of blood", some kills explode and give over 100 health.
  https://ultrakill.wiki.gg/wiki/Arms and https://ultrakill.fandom.com/wiki/Knuckleblaster
- ULTRAKILL Whiplash: light enemies are pulled to you, medium/heavy pull you to them (weight decides). Cooldown 0.5 s.
  https://ultrakill.wiki.gg/wiki/Arms
- Turbo Overkill reviews: enemies "explode with gibs and particles" in swarms; the chainsaw slide does more damage with more
  momentum; the same reviews complain that weak SOUND removed punch (visuals alone did not carry it).
  https://techraptor.net/gaming/reviews/turbo-overkill-review
- Prodeus: dismemberment (head blown off mid-air) singled out by reviewers as the payoff; no dev numbers found.
  https://bit-tech.net/reviews/gaming/pc/prodeus-preview/1/
- Rainbow Six Siege alpha removed hit markers (players used them to fish through smoke) and enlarged blood splatter as the
  replacement confirmation. https://www.ubisoft.com/en-us/game/rainbow-six/siege/news-updates/4ZXqtA5sxazw8Npy34Tsxw/closed-alpha-feedback

## 3. Melee: ULTRAKILL punches, DOOM Blood Punch

- Feedbacker: damage 1, punch animation ~0.35 s, cooldown 0-0.8 s via "punch exhaustion" (1 per punch). Knuckleblaster: damage
  2.5, anim ~0.87 s, cooldown 0.4-1.2 s, exhaustion 1.5 and may go negative; its shockwave deals 1 and deflects projectiles with
  harder timing than a parry. https://ultrakill.wiki.gg/wiki/Arms
- A melee parry deals 5-6 base damage depending on source (varies by enemy). https://ultrakill.wiki.gg/wiki/Parry (5), Arms (6)
- DOOM Eternal Blood Punch: charged by glory kills (two kills recharge it), AoE of two circles around the Slayer, kills lesser
  enemies, staggers bigger ones; killed enemies drop health. Exact damage/radius not found. [snippet]
  https://breezewiki.discard.no/doom/wiki/Blood_Punch

## 4. Parry window and forgiveness (ULTRAKILL)

- The window length in seconds/frames was NOT found in any source. What is documented are the forgiveness tools:
  - 1.03b: "Added 3 mercy frames between a projectile hitting the player and the damage happening, making projectile parrying
    slightly more lenient." Also changed the projectile parry from a raytrace into a hitbox, "a bit easier to punch at an angle".
    https://hakita.itch.io/ultrakill-prelude/devlog/98473/103b-patch-notes
  - Reward: full stamina, health restored up to the Hard Damage cap, a hitstop and flash. https://ultrakill.wiki.gg/wiki/Parry
  - Hard Damage: on Standard+ 35% of damage taken becomes hard damage; max health regrows at 14/s after a few seconds.
    https://ultrakill.fandom.com/wiki/Hard_Damage
- Design reading: the window is judged tight, but 3 frames at 60 fps = 50 ms of grace plus a wide hitbox plus a huge reward
  (heal, stamina, 0.25 s freeze) makes failing cheap and succeeding loud. (My inference from the above, not a source claim.)

## 5. Boost movement: how easy is the shotgun / rocket boost

- Quake: your own rocket/grenade splash does half damage to you (max 120 -> ~60); jumping straight up costs about 53 at most;
  the minimum self splash seen was 21. Knockback comes from any damage, explosions included. The push is free; the price is
  health. https://quake.speeddemosarchive.com/quake/qdq/articles/ZigZag/explosive.htm
- ULTRAKILL: pump-charge shotgun 3 pumps = 10 damage to enemies, 50 to you (a launch for 50 hp); parried pellet explosion
  (projectile boost) = 3.5 to enemies, 35 to you; Core Eject "Ultraboost" is described as hard (needs two weapons and timing).
  https://ultrakill.wiki.gg/wiki/Pump_Charge_Shotgun , https://ultrakill.wiki.gg/wiki/SSJ
- ULTRAKILL slam storage: momentum from slams is kept via slide-jumps "in a small window". https://gamepretty.com/ultrakill-advanced-movements-guide/
- DOOM Eternal Meat hook: the Super Shotgun's grapple pulls the Slayer to demons (traverse and close the distance). Cooldown and
  pull speed not found; Ballista not found. https://doomwiki.org/wiki/Meat_hook (search summary only)
- Dusk: air control is greater than Quake, bunnyhopping is forgiving. https://www.quakeworld.nu/blog/394/dusk-an-interview-with-david

## 6. Weapon readiness (does the next shot wait for the recoil animation)

- ULTRAKILL: switching between the Pump Charge and Core Eject shotguns skips their reload animations, so you can fire both back to
  back. Fire intervals: Core Eject / Sawed-On 1.25 s, Pump Charge 0.9 s, Pump alt fire 0.4 s. The cooldown is a number, not an
  animation lock. https://ultrakill.wiki.gg/wiki/Pump_Charge_Shotgun
- DOOM Eternal: weapon mod swaps can be animation-cancelled; patch 1.06 added quick switch. DOOM 2016 preview calls
  "flick-switching" "nearly instantaneous". https://doomwiki.org/wiki/Weapon_modification ,
  https://www.thesixthaxis.com/?p=362144 , https://bethesda.net/en-AU/news/inside-doom's-brutally-fast-single-player-campaign
- Takeaway across these: fire rate is a timer; switching is the cancel. No source says the next shot waits for visual recoil.

## 7. Style meter (ULTRAKILL) and health presentation

- 8 ranks, thresholds in points: Destructive 200, Chaotic 300, Brutal 400, Anarchic 500, Supreme 700, SSadistic 850, SSShitstorm
  1000, ULTRAKILL 1500. Base decay 15/s times a rank multiplier: 1, 1.25, 1.5, 2, 3, 4, 6, 8 (so 15 -> 120 per second).
  Colours: blue, green, yellow, orange, red, then SS ranks, gold at ULTRAKILL. https://ultrakill.wiki.gg/wiki/Style
  and https://www.wikipedia.com/wiki/Ultrakill
- What fills it: kill 30-45, parry 100, headshot 80 (150 vs bosses), big kill 100-125, chargeback 400, ground slam 60, air slam 160.
  Multipliers: movement up to 3x (rises 2.0/s while sliding/airborne, falls fast on the ground); weapon freshness 1.5x / 1.0 /
  0.5 / 0.0 (each use cuts the weapon's freshness 0.7, raises others 0.5); bosses 1.5x. https://ultrakill.wiki.gg/wiki/Style
- Display: rank letter and bar, a freshness bar below, bonus text popups ("PARRY", "PROJECTILE BOOST"...). How the bar animates
  (easing, flashes) was NOT found. https://steamcommunity.com/app/1229490/discussions/0/3906367962693957507
- Design intent: Hakita started from game feel above all, "What if DMC was an FPS?"; God Hand's loud, clipping audio was the model.
  https://intothebluesky.com/2020/12/19/interview-with-arsi-hakita-patala/ and the 80.lv piece
  https://80.lv/articles/ultrakill-devs-on-the-game-s-mechanics-npc-ai-and-early-access-experience/
- Health presentation: ULTRAKILL heals from blood in close combat and parries, with the hard-damage "dead" portion of the bar;
  DOOM drops health on glory kills (the kill itself is the health bar). No per-enemy health-bar numbers found.
  https://ultrakill.fandom.com/wiki/Hard_Damage

## 8. Not found / open

ULTRAKILL parry window in ms; minor hitstop value; style bar animation; DOOM stagger duration; Blood Punch numbers; Meat hook
cooldown; Ballista; Dusk / Prodeus / Turbo Overkill dev numbers on hitstop or shake (reviews only). Hit-marker research found no
rigorous study, only the R6 removal story.

## Ten concrete numbers to try

1. 0.25 s freeze + white flash on a parry or other "green" skill hit. Source: ULTRAKILL major hitstop, https://breezewiki.discard.no/ultrakill/wiki/Hitstop
2. 3 frames of mercy between a projectile touching the player and the damage landing, plus a hitbox (not ray) parry test.
   Source: https://hakita.itch.io/ultrakill-prelude/devlog/98473/103b-patch-notes
3. 10-20 ms (about 1-2 frames at 120 Hz) freeze on an ordinary bullet hit. Source: Nuclear Throne talk, https://infovore.org/?p=5275
4. 0.05-0.08 s freeze on a strong or melee hit, 0.10-0.15 s on a finisher [blog; start here, tune by eye].
   https://uhiyama-lab.com/en/notes/unity/unity-game-feel-hit-feedback/
5. 80 ms enemy hit flash (1 white frame first, then 2 frames of hurt pose). Sources: https://infovore.org/?p=5275 and the 0.08 s
   figure at https://uhiyama-lab.com/en/notes/unity/unity-game-feel-hit-feedback/
6. Shake 6 px camera kick + 4 units added per shot, decaying fast (scale to FOV). Source: https://infovore.org/?p=5275
7. Self-damage launch: 50 hp for a big shotgun launch, 35 hp for a parried pellet boost; Quake halves self splash (about 53 max).
   Sources: https://ultrakill.wiki.gg/wiki/Pump_Charge_Shotgun , https://quake.speeddemosarchive.com/quake/qdq/articles/ZigZag/explosive.htm
8. Style meter decay 15 points/s times 1 / 1.25 / 1.5 / 2 / 3 / 4 / 6 / 8 by rank, rank thresholds 200-1500; parry +100, kill 30-45,
   air slam +160. Source: https://ultrakill.wiki.gg/wiki/Style
9. Weapon freshness: 1.5x / 1.0x / 0.5x / 0.0x, -0.7 for the weapon just used, +0.5 for the others (anti-spam). Source: same Style page.
10. Execution/finisher animation of "hundreds of milliseconds" (DOOM 2016's stated rule, keep it under ~700 ms) with a reward drop
    (health) on completion. Source: https://bethesda.net/en-EU/news/22-reasons-to-love-doom-part-1 (the 600-700 ms figure is a
    search summary only).

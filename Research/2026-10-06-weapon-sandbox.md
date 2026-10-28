# Research 2026-10-06: the weapon sandbox (every gun has a job), combos, movement vs threat, pad and HUD

One read-only agent, about 15 minutes, for the next night's prompt. GG's ask: each weapon with its own purpose as in
ULTRAKILL, all balanced ("it's just which one you prefer"), combos, and strong but catchable movement. Today the farm's
kill share is shotgun 38-40 %, revolver 28-33 %, nailgun 27-29 %, everything else about 2 % (the brief).

Tags: **[read]** = read on the cited page today; **[snip]** = a search-result snippet only (the page wasn't opened or
returned no body); **(unverified)** = from memory or not confirmed; **[rec]** = this note's recommendation, not a sourced
number; **[repo]** = read in this repo today. Not repeated here (see the earlier notes): style ranks / decay / callouts
and hitstop (2026-10-05-overhaul.md §1), stamina models across games (§3), the controller / Deck / glyph basics and
weapon-wheel conventions (2026-10-04-controller.md §7), freshness-affects-score-not-damage (ZOMBIES-ULTRAKILL-RESEARCH.md
§3).

## Sources

- ULTRAKILL wiki (wiki.gg): Revolver https://ultrakill.wiki.gg/wiki/Revolver, Shotgun
  https://ultrakill.wiki.gg/wiki/Shotgun, Nailgun https://ultrakill.wiki.gg/wiki/Nailgun, Railcannon
  https://ultrakill.wiki.gg/wiki/Railcannon, Rocket Launcher https://ultrakill.wiki.gg/wiki/Rocket_Launcher, Style
  https://ultrakill.wiki.gg/wiki/Style, Movement https://ultrakill.wiki.gg/wiki/Movement, Health
  https://ultrakill.wiki.gg/wiki/Health, Parry https://ultrakill.wiki.gg/wiki/Parry, Virtue
  https://ultrakill.wiki.gg/wiki/Virtue, Mindflayer https://ultrakill.wiki.gg/wiki/Mindflayer, Schism
  https://ultrakill.wiki.gg/wiki/Schism, Options https://ultrakill.wiki.gg/wiki/Options
- DOOM: Game Developer, "Make me think, make me move"
  https://www.gamedeveloper.com/design/-make-me-think-make-me-move-new-i-doom-i-s-deceptively-simple-design; Shacknews,
  the making of DOOM Eternal https://www.shacknews.com/article/116635/hell-razer-the-making-of-doom-eternal?page=8;
  Thumbsticks' weak points https://www.thumbsticks.com/how-to-exploit-demon-weaknesses-doom-eternal/; PC Gamer on the
  Marauder https://www.pcgamer.com/doom-eternals-director-says-the-marauder-is-good-actually/ (body didn't load; [snip]);
  Shacknews controls https://www.shacknews.com/article/117015/doom-eternal-controls-and-keybindings
- Others: RoR2 proc coefficient https://riskofrain2.wiki.gg/wiki/Proc_Coefficient; Gunfire Reborn elements
  https://gunfirereborn.fandom.com/wiki/Elemental_Effect; Roboquest Elementalist https://roboquest.wiki.gg/wiki/Elementalist;
  Turbo Overkill review https://techraptor.net/gaming/reviews/turbo-overkill-review; Hades aspects
  https://www.rpgsite.net/feature/10254-hades-infernal-arms-weapon-aspects-guideevery-weapon-aspect-and-upgrade-unlock;
  Cruelty Squad review https://www.pcgamer.com/cruelty-squad-review/
- Pad and glyphs: Activision's MW movement post
  https://blog.activision.com/call-of-duty/2019-10/The-Basics-of-Call-of-Duty-Modern-Warfare-Movement; ISteamInput
  https://partner.steamgames.com/doc/api/isteaminput; Kenney Input Prompts https://kenney.nl/assets/input-prompts;
  Xelu's prompts https://thoseawesomeguys.com/prompts/

---

## 1. ULTRAKILL's arsenal

**The rule under all of it:** variants are **sidegrades**, never upgrades. Every variant of a slot shares the primary's
damage; only the alt-fire differs, and each alt-fire is a tool that sets up another weapon. [read, all five weapon pages]

**Revolver** (primary 1 dmg, 0.5 s, headshot x2, limb x1.5, all variants) [read: Revolver]
- **Piercer:** alt charges 0.5 s, 3 hitscans of 1, pierces, 2.5 s cooldown; head x2.5. Niche: lines of enemies.
- **Marksman:** 4 coins, 1 coin per 4 s. A hitscan on a coin ricochets to the nearest target, +1 damage per coin.
  Coins also reflect the Piercer's charge shot, the Electric rail (+0.5 x 4 hitscans), and boosted cores / rockets
  (+0.5 hitscan +1 explosive). Niche: the combo hub that makes every other hitscan better.
- **Sharpshooter:** alt is a bouncing beam; 0.3 s charge = 1 bounce, 2.5 s+ = 3; holds 3 shots, 6.66 s each; +1 hitscan
  per bounce; +2 hitscans off the first coin. Niche: bank shots, detonating projectiles mid-air.

**Shotgun** [read: Shotgun]
- **Core Eject:** 12 pellets x 0.25, 1.25 s. Alt throws a core (3.5 contact, 35 to yourself), 2 s cooldown; hold to
  throw further. **Shoot the core** with a revolver or rail: double radius and damage; an explosive hitscan makes a
  "core nuke" (4x radius). The Whiplash can hook a core back.
- **Pump Charge:** 10 x 0.25, 0.9 s. Pumps: 1 = 16 pellets, 2 = 24 wider, 3+ = an explosion (10 dmg, 50 to yourself)
  that can still shotgun-parry.
- **Sawed-On:** 12 x 0.25, 1.25 s. Alt: a tethered chainsaw, 0.25 per 0.05 s **and 10 HP healed per tick**; launched
  1.5 x 3 per enemy; 4 s recharge. Punch it to relaunch.
- **Shotgun parry:** a point-blank torso shot on a melee attack is a parry: more damage than a punch-parry, full heal,
  full stamina.
- **Swap-cancel:** variants have independent cooldowns, so switching variants skips the reload ("rapid-fire burst").

**Nailgun** (nails 0.185-0.205 each, 12.5-25/s) [read: Nailgun]
- **Attractor:** 25 nails/s; alt places up to 3 magnets (12 s each, 0.35 s apart) that pull nails, saws, rockets,
  chainsaws; a magnet holds 75 nails. Break the magnet (slam or hitscan): each released nail deals
  (embedded nails / 2) x nail damage: the **nail cloud / nailbomb**.
- **Overheat:** 25/s cold -> 12.5/s hot -> 8/s depleted; alt fires heatsinks, 62.5 nails/s for ~1 s, 2 charges, 8 s
  each; flaming nails ignite. Heat is shared across variants.
- **JumpStart:** alt is an electric cable: 10 shock, 20 u range, 5 s; a shocked enemy **with nails in it** sends a
  conductor aftershock of 2-6 (by 1-90 nails) to nearby grounded enemies and conductive objects (magnets, coins,
  rockets, drills).
- Freshness: nails count 0.1x, so a 25/s gun doesn't go stale in a second (Style, below).

**Railcannon** (all three share one charge: **16 s**; a lightning-bolt icon on the HUD) [read: Railcannon]
- **Electric:** 8 (2 x 4) hitscan, infinite pierce, hitstop on each enemy. Off a coin: +25 % first coin, +10 % each
  after. Triggers conductor aftershocks on nailed enemies. In water: aftershocks everything and 50 hard damage to you.
- **Screwdriver:** a drill, 3 on impact, then 0.0625 x 116 over ~7.5 s, bleeding "10 health worth of blood every tick"
  (a heal source); punching the drill dumps the remaining ticks +5. Drills chain conduction.
- **Malicious:** 2 hitscan + 6.25 explosion with **no falloff**; off a coin +0.5 / +1.25 first, +0.25 / +0.625 after.

**Rocket launcher** (3.5 direct, 7 on an airshot, 1 s) [read: Rocket Launcher]
- **Rocket riding:** each rocket weaker from the 4th on; resets on landing (riding speed 65 u/s, Movement page).
- **Freezeframe:** alt freezes **all** rockets, yours and enemies', up to 5 s, 10 s recharge; a rocket frozen 1 s+
  explodes "red" (charged) whatever it hits. **Stacking:** frozen rockets plus a magnet on one enemy = several
  simultaneous airshots.
- **S.R.S. cannon:** a cannonball, 2 uncharged / 5 at 25 %+ / 7 parried, bounces, its shockwave launches enemies up;
  8 s cooldown.
- **Firestarter:** a 3 s gasoline spray (0.5/tick), 8 s recharge; any explosion ignites it; burn lasts 6 s; 10 per tick
  to you if you stand in it.

**The canonical combos [read, the pages above]:** coin ricochet (any hitscan -> coin -> target, damage grows per coin);
rail + coin (Electric beam re-aimed and boosted); core eject + shoot (2x radius / damage, 4x with an explosive hitscan);
shotgun parry (point-blank on a melee wind-up); magnet nail cloud (nails gathered, magnet broken -> nailbomb);
conductor (nails make an enemy a battery for shock / rail); freezeframe stacking (rockets parked, released together,
homed by a magnet); rocket riding (movement from a weapon); airshot (rockets x2 on airborne enemies; the S.R.S. launches
them).

**Freshness (exact) [read: Style]**
- Scale 0-10 per variant. FRESH 7.5-10 = **x1.5**, USED 5-7.5 = **x1.0**, STALE 1-5 = **x0.5**, DULL 0-1 = **x0**,
  applied to **style points** only.
- Each attack: that variant -0.7; every other weapon (not same-slot variants) +0.5. Not held: +0.1/s.
- Worked example (not on the page): a revolver at 0.5 s drains 1.4/s: FRESH -> USED in ~1.8 s, STALE in ~3.6 s,
  DULL in ~6.4 s of nothing else. From DULL, passive recovery to FRESH takes ~65 s; ~13 attacks with other guns do it.
- Per-hit-type regen weights: alt-shotgun hits 3x, explosions 0.75x, pellets 0.15x, **nails / saws 0.1x**, screwdriver
  0.025x, fire 0x. Style bonuses also weight it (FULL IMPACT x8 ... KILL x0.3, multikill x0.1, QUICKDRAW x0).
- Floors: with one variant equipped a weapon can't drop below FRESH; two variants, USED; three, STALE (so a player with
  few tools isn't punished).
- Boss attacks drain 1.5x.

**Healing from blood [read: Health]:** health comes back only by damaging enemies **at close range** (the wiki gives no
radius; a fan source's "5 ft" is not official). Punch: torso 10, limb 20, crit 30; parry 999 (full, up to the hard
damage cap). Hard damage = 35 % of damage taken, blocks healing, starts after 1 s, drains at 15 HP/s (60/s at the top
rank). Why it forces aggression: the only heal is in the enemy's face, and getting hit raises the cap that blocks it.

**The parry [read: Parry]:** Feedbacker punch (projectiles and melee), shotgun point-blank (melee), the alt-shotgun
bludgeon. A parry freezes, flashes white, heals to the hard-damage cap, refills stamina, +PARRY. Tells: **yellow flash =
parryable** (4-point star melee, 8-point ranged), **blue = unparryable**. The window length isn't on the page.

## 2. DOOM Eternal's combat chess

- **The definition [read, Game Developer]:** "speed of movement; individuality of the demons; distinctiveness of the
  weapons; overall power of the player; and ... 'make me think, make me move'". Non-melee demons "hold their position"
  so the player initiates. "There's only one way to play Doom."
- **The resource loop [read, Shacknews p8]:** chainsaw weak enemies for ammo, glory kill for health, flame belch for
  armour. "If the AI and weapons are balanced, and there is a right way to play it, there is a path to success."
  Weapons are paired: the slow, heavy shotgun with the heavy cannon's stream ("one-two punch").
- **Weak points that ask for a tool [read, Thumbsticks]:**
  - Arachnotron: the turret is its primary attack; "hit the turret with whatever you've got" (precision bolt or sticky
    bomb, [snip] Shacknews) and it keeps only its secondary and swipe.
  - Mancubus: the two arm cannons; breaking them makes its attacks "basically harmless" (bolt, ballista, sticky [snip]).
  - Revenant: the shoulder rockets double as its jetpack: break them and it is grounded.
  - Cacodemon: "pop a grenade (or sticky bomb) into his mouth then ... a Glory Kill".
  - Carcass: plasma overloads its shield; Doom Hunter: plasma or blood punch breaks its shield; Cyber-Mancubus: blood
    punch strips armour; Whiplash: the ice bomb freezes her; Pinky: the tail, or one blood punch.
  - Marauder [snip, PC Gamer via search]: it "falters for a specific amount of time to allow for at least one shot with
    most weapons, but just long enough for two shots if the player swaps". The green flash is the window and the shield
    blocks everything else (unverified detail: not on a page read).
- **Low ammo forces the swap [snip, Steam / GameFAQs]:** about 24 shells and 13 rockets at max (unverified as exact);
  the stated point is "to use all the different weapons, and ... the chainsaw to get more ammo".
- **Quick-swap [snip, Steam guides]:** swapping skips a weapon's recovery; ballista -> super shotgun -> ballista is the
  Marauder answer; the precision bolt has an animation cancel. Speed is frame-rate bound; dedicated keys beat the wheel.
- **The lesson id took into The Dark Ages** (already in ZOMBIES-ULTRAKILL-RESEARCH.md §4): the Marauder's "such specific
  solutions" was hated; "fewer strings on the guitar". So: **an enemy may favour a tool, never require one.**

## 3. Other games that made every gun viable (2-3 lessons each)

- **Hades [read, rpgsite]:** each weapon is its own verb set (attack / special / cast); **aspects are sidegrades that
  change the verb**, not a bigger number: Arthur = slowest, hardest-hitting weapon, +50 HP, a 20 % damage-reduction aura
  you must stand in; Chiron = the special homes on the last foe hit; Beowulf / Hera load your cast into the weapon.
  Lesson: an upgrade that changes how a gun plays keeps every gun a preference.
- **Risk of Rain 2 [read via snip of the wiki page]:** every hit has a **proc coefficient** that scales on-hit chances
  and durations; fast multi-hit attacks get less (MUL-T's nailgun 0.6: a 25 % item procs 15 %). Lesson: status
  application per hit must scale down with fire rate, or the fastest gun owns every status.
- **Gunfire Reborn [snip, fandom]:** 3 elements, each a status (burn 20 % of the hit per s for 5 s; shock +10 % damage
  taken for 5 s; corrosion +50 % vs armour), and **pairs fuse**: burn + corrosion = Combustion (200 % instant, 100 % in
  5 m, stagger); burn + shock = Manipulation (enemies fight each other 5 s); shock + corrosion = Miasma (9 % max HP/s
  true damage). The Elemental Weave scroll: 100 % status chance after a swap. Lesson: a second gun of another family is
  the trigger for the big payoff.
- **Roboquest [snip, wiki]:** the Elementalist's Mantra cycles burn / shock / cryo every 10 s with +15 % to the current
  one; Elemental Resonance adds damage per status on a target. Lesson: a rotating favoured family nudges variety
  without forbidding a favourite.
- **Turbo Overkill [snip, TechRaptor]:** "each weapon has a use, along with pros and cons against certain enemy types";
  every gun has an alt-fire (the rocket launcher loads 4 at once); the chainsaw-leg slide does more damage the faster
  you go. Lesson: movement speed can be a damage stat, so fast players fight instead of kite.
- **Cruelty Squad [snip, PC Gamer]:** 20-plus mostly realistic guns; the ZKZ rifle's damage scales with your market
  holdings. Lesson: one gun tied to the economy (points banked) is a novelty slot, not a balance tool.

## 4. Movement vs threat: catching a fast player without slowing him

- **SKYGRIND already has ULTRAKILL's ratios [read: Movement; repo: unity-config.json, Mover config]:** ULTRAKILL walk
  16.5, slide 24, dash 49.5 u/s = **1 : 1.45 : 3.0**; SKYGRIND walk 8, slide 11.6, dash 24 m/s = **1 : 1.45 : 3.0**.
  ULTRAKILL: 3 stamina, dash 1, dash-jump 2, regen 0.7/s (1.05 Lenient, 1.4 Harmless), no regen while sliding, a parry
  refills all; wall jumps capped at 3 per air time. So the movement isn't too fast; the threats are too slow.
- **ULTRAKILL's threat toolbox [read]:**
  - **Virtue:** a marker follows you for ~3 s, stops on your spot, then a 30-damage beam (once per beam, lingers ~2 s).
    Enraged (after 5 attacks), the marker **predicts** your position. It punishes straight-line running and standing.
  - **Schism:** 6 linked orbs in a horizontal or vertical line (vertical when you're 5+ u above or below). On Brutal the
    first orb aims at you and **each next one leads more**. Jumping clears the horizontal line; the vertical one asks
    for a sidestep. It is "defenceless while airborne".
  - **Mindflayer:** 5 homing seekers (30 each; max speed 32.5 u/s Harmless, 48.75 Lenient, 87.75 Violent+), a sweeping
    35 beam, a 30 melee; **teleports away whenever it takes more than 6 damage**. It answers speed with homing and
    answers burst with a reset.
  - Parry tells (yellow / blue) let the fast player turn threats into heals.
- **DOOM Eternal [read / snip]:** 2 dash charges refilled on landing (2026-10-05-overhaul.md §3); the Whiplash is the
  fast hunter (frozen by the ice bomb); the Marauder punishes range with a ranged attack and closeness with a shotgun
  (unverified, not on a page read).
- **Patterns to steal [rec]:** markers that trail then commit (Virtue); progressive lead within one burst (Schism);
  homing with a capped turn rate and speed slightly above the player's sprint, below the dash (Mindflayer orbs vs the
  3x dash); area denial (puddles, beams that linger 2 s); an enemy that rewards being airborne (Schism's airshot) so
  air time is a choice; a reset on burst (teleport after X damage) so one-shot camping fails.

## 5. Controller conventions

- **CoD [read: Activision MW 2019 post]:** sprint = L3 while moving; tactical sprint = "click L3 twice and hold
  forward", the fastest movement, can't ADS without losing it; at MW 2019's reveal "only ... one tactical sprint per
  life" (later games recharge it; the Double Time perk doubles its duration [snip]; durations unverified). Slide =
  hold O while sprinting, with a hold / tap setting. Melee on R3 is the CoD default (unverified: not on a page read).
- **ULTRAKILL pad default [read: Options]:** move LS, **dodge (dash) = L3**, slide = A or R3, jump = B, fire RT, alt LT,
  punch X, previous variation RB, next variation RB+Y, last weapon d-pad up, change fist d-pad down, whiplash LB+RB,
  stats Select. (It reads oddly; other sources disagree, [snip] ludo.guide timed out: verify in game.)
- **DOOM Eternal pad default [read: Shacknews]:** fire RT, mod LT, equipment LB, switch weapon RB (hold = wheel),
  flame belch Y, chainsaw X, dash B, jump A, melee / glory kill R3, mod swap d-pad up, equipment swap d-pad left.
- **Menus [unverified]:** LB / RB switch top tabs and LT / RT switch sub-tabs on Xbox and PlayStation (Microsoft's
  "designing for Xbox and TV" guidance, https://learn.microsoft.com/windows/apps/design/devices/designing-for-tv, not
  opened today).
- **Glyphs:** ISteamInput `GetInputTypeForHandle` gives the real pad type; `TranslateActionOrigin` maps an origin to
  another pad type; `GetGlyphForActionOrigin` returns a Valve PNG path [read]. The newer `GetGlyphPNGForActionOrigin`
  takes a size (Small 32, Medium 128, Large 256) and style flags (Knockout, Light, Dark, NeutralColorABXY, SolidABXY)
  [snip; the docs page fetched showed only the legacy call]. Hysteresis: see 2026-10-04-controller.md §7.
- **Free glyph packs:**
  - **Kenney Input Prompts: CC0** [read]: 1,500 files; Xbox 360 / One / Series, PlayStation 1-5, Switch, Switch 2,
    Steam Deck, Steam Controller, keyboard, mouse; 64 x 64 tiles, spritesheets, fonts and vectors (v1.5a).
  - **Xelu's Free Controller & Key Prompts: CC0** ("completely free to use in any personal or commercial project")
    [read]; the device list is in its readme, not checked.
  - **Steam Deck glyphs:** use Steam's runtime art through the API above; redistributing Valve's glyph files in the
    build: not checked. Kenney covers the Deck under CC0.

## 6. ULTRAKILL's HUD

- **[read: Options]:** HUD types None / Standard (default) / Classic Color / Classic White. Standard: "player
  information HUD ... to the side of the screen with each HUD component nestled to each other in a rectangular
  formation". Classic: "locked to the bottom of the screen" with **numbers** for health and stamina. Toggles: weapon
  icon, arm icon, railcannon charge meter, style rank, style bonuses / multipliers, speedometer. Background opacity
  0-100 %. Crosshair surround thickness None / Thin / Medium / Thick / Extra Thicc. "Always On Top" keeps the HUD over
  the viewmodel.
- **From memory (unverified):** Standard sits bottom-left: a red HP bar over a blue 3-segment stamina bar, the
  railcannon charge as a small bar / bolt, the weapon icon in a square tinted by variant colour (blue / green / red),
  the arm icon beside it. The style meter is a panel on the right with the rank letter-word and a decay bar, and the
  bonus lines stacked under it. Crosshair: a small centre with optional HP / stamina arcs round it.
- **Why it reads cool, not cheap (inference):** variant colour as the only accent on each icon (one glance tells the
  loadout); chunky segmented bars with hard edges; one dark plate with no gradient; the centre kept nearly empty; big
  rank letters with a slam; information grouped into one block instead of scattered corners.

---

## What SKYGRIND could take

**The core diagnosis**
1. **Stop selling damage.** The FORGE (x1.8 plus the family bonus) and a box full of stronger guns are the CoD-Zombies
   engine: one gun gets much bigger, so you keep it. ULTRAKILL and Hades sell **sidegrades** (an alt-fire, a changed
   verb). Make the forge grant an alt-fire variant per gun, with damage unchanged [rec].
2. **Freshness must not touch damage.** Today's arsenal scales damage and points from x0.65 (freshMin) to x1.25
   (freshMax) [repo: config.json weapons.arsenal]. ULTRAKILL applies it to style only (§1); players call damage scaling
   "annoyingly inconsistent and punishing" (ZOMBIES-ULTRAKILL-RESEARCH.md §3). That keeps GG's "it's just which one you
   prefer" and still pays for rotation.
3. **The shotgun wins because it's a generalist.** Give each gun a range band and a job, so no gun is best everywhere.

**One-line niche per gun [rec]**
4. **KESTREL (revolver): the trigger.** Precision at any range (head x2), and its hit **detonates your own projectiles**
   (COMET rockets, BOUNCER grenades) like ULTRAKILL's core + shot: 2x radius and damage.
5. **THUNDERCLAP (shotgun): point blank only.** Full damage inside ~5 m, steep falloff after; a point-blank torso hit on
   a wind-up is a **parry** (heal + stamina refill), as ULTRAKILL's shotgun parry. 8 pellets x 8 stays.
6. **STITCHER (nailgun): the battery.** Low per-nail damage; nails **stay in** the target and make it conductive: a
   volt hit (ZAPPER, LONGWAVE) on a nailed enemy arcs to nearby enemies (ULTRAKILL conductor: 2-6 by 1-90 nails).
7. **COMET (rocket): crowds and lift.** Splash 4 m **launches** light enemies; an airborne enemy takes **x2 from any
   hit** (ULTRAKILL airshot 3.5 -> 7). Self-knockback is movement (rocket jump).
8. **LONGWAVE (rail): the line and the armour key.** Infinite pierce; the only gun that breaks fronts and plates
   outright; one long shared charge so it's an event, not a primary (ULTRAKILL's rail: 16 s).
9. **Keep:** TORCH (fire: area denial, puddles that COMET ignites, as the Firestarter), ZAPPER (volt chains vs
   SWARMERs), BOUNCER (blast: bank-shot grenade the KESTREL pops), VORTEX (gravity: gathers a crowd for COMET or
   THUNDERCLAP), TETHER WHIP (pulls an enemy or your own grenade / rocket to you, as the Whiplash hooks cores).
10. **Fold or cut:** RATTLER and CHOIR (kinetic, fast, overlap the STITCHER and the KESTREL), DISCUS (edge, overlaps
    THUNDERCLAP), PRISM (pierce, overlaps LONGWAVE). Their mechanics weren't read in code today (not checked): if one
    has a unique verb, it becomes another gun's alt-fire instead.

**Cross-gun combos [rec, each modelled on §1]**
11. **POP:** KESTREL hits a live COMET rocket or BOUNCER grenade -> double radius and damage (core eject + shoot).
12. **CONDUCTOR:** STITCHER nails, then a ZAPPER or LONGWAVE hit -> the shock arcs to every nailed enemy within a few
    metres, damage by nail count.
13. **LAUNCH + SNIPE:** COMET or THUNDERCLAP-uppercut launches; any hit on an airborne enemy x2 (airshot); the
    KESTREL headshot on a launched enemy is the style showpiece.
14. **GATHER + BURST:** VORTEX pulls a crowd -> COMET or THUNDERCLAP into the knot (the magnet + freezeframe idea).
15. **IGNITE:** TORCH coats; any blast (COMET, BOUNCER, a POP) ignites the coat for burn.
16. **SWAP-CANCEL:** the existing primed swap (+35 % first hit within 1 s, weapons.arsenal) becomes the DOOM
    ballista -> SSG rhythm: a swap skips the recovery of the gun you left. Keep the bonus; make the swap fast.

**Enemy traits that ask for a gun (favour, never require: The Dark Ages' lesson) [rec]**
17. **Plates / a shield front** (a HUSK variant): LONGWAVE breaks it, or flank it, or COMET-launch it.
18. **Weak points that disarm** (DOOM's turret, cannons, shoulder rockets): the SPITTER's glob sac; a KESTREL headshot
    pops it and the SPITTER can only melee. Its globs in flight are shootable / parryable (the yellow-tell rule).
19. **Swarms** (SWARMER): STITCHER / ZAPPER chains or a VORTEX + COMET knot; single-target guns waste time.
20. **Teleport-on-burst** (Mindflayer: away after more than 6 damage): an enemy that resets after a big hit rewards
    sustained or chained damage over one THUNDERCLAP blast.
21. **Pursuers that catch a fast player:** a Virtue-style marker that trails ~3 s then predicts; Schism-style bursts
    whose shots lead more each time; homing orbs faster than the 10.5 m/s sprint but slower than the 24 m/s boost;
    puddles (ACID) that deny the slide lanes. Each one is beaten by a dash, a parry, or a jump, not outrun by sprinting.

**Freshness proposal (style and points only, never damage) [rec]**
22. ULTRAKILL's table exactly: a 0-10 scale; FRESH >= 7.5 x1.5, USED >= 5 x1.0, STALE >= 1 x0.5, DULL < 1 **x0.25**
    (ULTRAKILL's x0 is the "you're meant to cycle everything" complaint; a floor keeps a favourite worth playing).
    Each gun's cost per shot = **1.4 x its interval in seconds** (KESTREL 0.4 s -> 0.56; THUNDERCLAP 0.9 s -> 1.26), so
    every gun goes FRESH -> STALE after ~3.6 s of continuous fire whatever its rate (ULTRAKILL's revolver pace); +0.5 to
    every other gun per shot; +0.1/s passive when holstered; with only one gun owned, the floor is FRESH (ULTRAKILL's
    floor rule), so the starting KESTREL never punishes a new player. The HUD shows the tier next to the weapon icon.

**Stamina proposal [rec]** (today [repo: Design/unity-config.json design.stamina]: bar 1.0 (+1 with VIGOR), boost
0.5, dash-jump +0.25, sprint 10.5 m/s at **0.4 bar/s**, refill 0.5 bar/s after 0.4 s (0.8 s if emptied), rewards: kill
+0.15, slam +0.25, parry +1)
23. **Sprint drain 0.2 bar/s** (5 s of sprint on a full bar instead of 2.5 s), so sprinting stops feeling like a
    tax; **passive refill down to 0.35 bar/s** after a 0.6 s delay (start with less: the web's 0.7 pips/s of 3 is
    0.23 bar/s, so 0.35 is still above the web).
24. **Earn it back in the fight (earn more):** split today's flat kill +0.15 by range: a kill within 6 m +0.25 (the
    blood-heal logic), beyond it +0.05; slam +0.25 only when the slam hits something; parry stays +1 (ULTRAKILL);
    VIGOR stays +1 bar. The fast player who fights stays fast; the one who kites runs dry.
25. **Measure, don't feel:** the night's checks should be the farm's kill share per gun (target: no gun above 25 %, at
    least 6 guns above 5 %), a style-per-minute delta with and without rotation, and the bot's time-to-hit for each new
    pursuer at sprint speed vs boost.

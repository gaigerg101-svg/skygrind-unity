# Weapons research: secondaries on gamepad, and unlock pacing

Method note: web only, 6 minute box. Figures below were read through a page summarizer (WebFetch), not by eye on the page, so treat each number as "reported by the cited wiki" and spot check before it goes into config. Items I could not find are listed at the end under "Not found". Some wiki hosts (doomwiki.org, fandom) returned 402/403, so DOOM Eternal numbers are thinner than ULTRAKILL's.

## Part 1. Secondary fire per weapon

### ULTRAKILL: every weapon slot has an alt fire, and the alt differs by variant

Structure: 5 base weapons, 3 variants each, 15 variants; revolver, shotgun and nailgun have alternate forms too. Source: https://ultrakill.fandom.com/wiki/Shotgun_(disambiguation) and https://ultrakill.wiki.gg/wiki/Weapons

Alt fire list as reported (https://ultrakill.wiki.gg/wiki/Weapons):
- Revolver: Piercer = piercing shot; Marksman = coin toss; Sharpshooter = piercing and ricocheting shot.
- Shotgun: Core Eject = core grenade; Pump Charge = pump/overpump; Sawed-On = chainsaw.
- Nailgun: Attractor = magnet; Overheat = heatsink dump; Jumpstart = electric cable.
- Railcannon: all three variants = zoom (Electric, Screwdriver, Malicious).
- Rocket launcher: Freezeframe = freezes rockets; S.R.S. Cannon = cannonball; Firestarter = gasoline.

Revolver alts (https://ultrakill.wiki.gg/wiki/Piercer_Revolver):
- Primary: 1 damage, 0.5 s cooldown; limb 1.5x, head 2x; hitscan.
- Piercer alt (hold to charge): charge takes about 0.5 s. Full charge released = blue beam, 3 total damage (1 damage x 3 hits). Then 2.5 s cooldown. Released before full charge: the shot cancels, nothing fires. So it is hold, release, and the early release is a safe abort (no cost reported).
- Sharpshooter alt (hold): at 0.3 s the shot bounces once; at 2.5 s or longer it bounces up to 3 times. Holds up to 3 charges, 6.66 s individual cooldown each. This is a charge-time ladder (more hold = more ricochets).
- Marksman alt: no charge, up to 4 coins, 4 s recharge per coin.

Coin (https://ultrakill.wiki.gg/wiki/Coin, reported via summarizer):
- A hitscan that hits a coin is reflected to the nearest target in line of sight.
- Damage: standard revolver +1 damage per coin (limb/head multipliers still apply). Piercer charged shot: +0.25 per subsequent coin on each hitscan. Sharpshooter: +1 hitscan from the first coin, nothing after.
- Split: shoot a coin at the peak of its arc (or 2+ s after the throw) and a standard shot splits into two; alt revolver shots add one hitscan and reset cooldowns.
- Target priority: other airborne coins, then cores/rockets/cannonballs, then interruptible projectiles, then enemy weak points, then any enemy point, then breakable glass.
- Coin lasts until it reflects a shot or 5 s airborne.

Cross-weapon combo, the clearest published example (https://ultrakill.wiki.gg/wiki/Core_Eject_Shotgun):
- Core Eject alt: core grenade, 3.5 damage (35 to the player), 2 s cooldown, "hold to charge distance" before firing.
- Shoot the core in the air with a non-explosive hitscan (standard revolver, alt revolver, Electric Railcannon): larger explosion, reported as 7 damage. Hit by another core, a rocket turned hitscan, or Malicious Railcannon: "extremely large" explosion. Explosion radius doubles for a non-explosive hitscan and doubles again for an explosive one.
- Net: shotgun alt (prime) + revolver (detonate) + coin (redirect) is a three weapon chain. Coins prioritise cores in their targeting list, which is what makes the chain work.

Pump Charge shotgun alt (https://ultrakill.wiki.gg/wiki/Pump_Charge_Shotgun): each pump (no cooldown, instant) raises pellets and spread. 0 pumps = 10 pellets at 0.25 each; 1 pump = 16 pellets; 2 pumps = 24 pellets, much wider spread; 3 pumps = explosion, 10 damage to enemies and 50 self damage. Primary fire cooldown is 0.9 s. Charge is stepwise (count presses) not time based, and the third step is a risk cliff (self damage) that doubles as a parry heal if timed on a melee attack.

### DOOM Eternal: mods are the secondary, one per weapon, on a fire-mode button

Reported, with limited numbers (https://www.thegamer.com/doom-eternal-weapon-mods-guide-upgrades-mastery/, https://doomwiki.org/wiki/Weapon_modification via search snippets, https://redbull.com/ca-en/best-weapon-mods-doom-eternal):
- Heavy Cannon, Precision Bolt: semi-auto sniper mode, tuned to pop weak points (Arachnotron turret, Mancubus cannons, Revenant rocket pods). Upgrade: reload 20% faster. Mastery: headshot kills trigger a blast that damages nearby demons; mastery requirement is 75 Precision Bolt headshot kills.
- Heavy Cannon, Micro Missiles: twin missile racks, minor homing. Upgrades: reload 35% faster (Quick Recharger), load time 75% shorter (Instant Loader). Mastery: fire indefinitely without reload.
- Combat Shotgun, Sticky Bombs: fires 3 bombs before reload; mastery 5. Upgrade: explosion 45% bigger (Bigger Boom). Strong against weak points.
- Plasma Rifle, Heat Blast: a heat gauge fills as you fire normally, then releases as a shockwave; three charge levels, the top level kills most enemies in one blast. Upgrade: +25% charge per shot. This is "charge by using the primary," not hold to charge.
- Rocket Launcher, Remote Detonate and Lock-On Burst: detonate mid flight; lock on then fire 3 rockets.
- Ballista, Arbalest: charged projectile that embeds then explodes; mastery gives instant recharge after a direct hit.
- Chaingun, Mobile Turret: highest DPS but limits mobility.
- Super Shotgun: single mod, the Meat Hook, already included on acquisition; Quick Hook upgrade cuts recharge 25%. Meat hook grabs enemies and slingshots you to them (https://www.fandom.com/articles/doom-eternal-weapons-gameplay-escalation via search snippet).
Pattern: each mod answers a specific enemy weak point or a movement need, and the mod has its own ammo or recharge so it never competes with primary ammo.

### Halo

- Pistol zoom: 2x magnification in CE, Reach, 4 and 5 (none reported for 2 and 3); Halo 3 scope does not work while dual wielding. https://www.halopedia.org/Magnum_(gameplay)
- Bungie's Jaime Griesemer is quoted (via search snippet) that the Battle Rifle began as a "save as" of the CE pistol because the pistol was overpowered. Lesson: a zoomed, accurate sidearm outshone the rest of the sandbox. https://www.halopedia.org/CE:Pistol
- Plasma Pistol overcharge: hold then release; EMP on hit drains shields and disables vehicles; the battery drains if the overcharge is held too long without firing. One search result claims a 4 second full charge for a Plasma Rifle secondary and that a miss is "very punishing" (unverified, forum/wiki aggregator). https://www.halopedia.org/Type-25_plasma_pistol/Gameplay (that URL 404ed on fetch; the claim came from the search snippet only).
- Boltshot: standard fire plus a charged burst. https://halo.fandom.com/wiki/File:Boltshot_hip.png (search snippet only).

### Destiny, Titanfall 2, Quake Champions

- Destiny 2 Linear Fusion Rifles share a Charge Time stat of 533 across models; perks (for example Liquid Coils) trade slower charge for more damage; some perks raise charge rate on kills. https://godroll.tv/615373993/eyes-unveiled (search snippet only; unit of 533 not defined by the source).
- Titanfall 2 Charge Rifle: hold the trigger to power a beam, long range, strong vs Titans. https://windowscentral.com/get-know-your-pilots-weapons-and-mods-titanfall-2 (search snippet only; no numbers or alt fire design info found).
- Quake Champions: official descriptions found list no secondary fire for Nailgun, Tri-Bolt or Rocket Launcher; the weapons are single mode. https://shacknews.com/article/99813/quake-champions-weapons-guide (snippet only). Useful as the contrast case: it relies on the champion's abilities, not weapon alts.

### What makes hold-to-charge feel good (what I could source)

Directly sourced:
- Early release should be safe or cheap. ULTRAKILL Piercer cancels with no shot if released before the 0.5 s charge completes (https://ultrakill.wiki.gg/wiki/Piercer_Revolver). Contrast with Plasma Pistol, where holding too long drains the charge (halopedia link above).
- Short, ladder style charge with visible steps: Sharpshooter 0.3 s = 1 bounce, 2.5 s = 3 bounces (https://ultrakill.wiki.gg/wiki/Piercer_Revolver). Doom Plasma Rifle Heat Blast has 3 named levels (https://doomwiki.org/wiki/Weapon_modification via snippet).
- Long cooldown after the payoff: Piercer 2.5 s, Sharpshooter 6.66 s per charge, Marksman coin 4 s per coin, Core Eject 2 s (same ULTRAKILL pages). The alt is a spend, not a spam.
- Charge limited in count (3 Sharpshooter charges, 4 coins, 3 sticky bombs) so the player banks them and spends them on a plan.
- Haptics: Returnal's alt fire is a trigger half pull to focus aim with a click, then full pull to enter alt fire; a unique haptic pulse tells you the alt fire has recharged; strongest vibration is reserved for the most powerful weapons (https://blog.playstation.com/2021/05/13/how-housemarque-created-returnals-immersive-dualsense-controller-effects/). Sony's DualSense guidance: adaptive triggers suit charging, loading and building energy (https://videogameschronicle.com/news/ps5-developers-reveal-how-their-games-use-dualsenses-adaptive-triggers-and-haptic-feedback, search snippet).

Not sourced (my inference, mark as opinion): release vs auto-fire at full charge. No source I found states a rule. ULTRAKILL requires release. Inference for a pad shooter with LT = secondary: a charge held on LT and fired with RT, or auto-fire at full on a short (under 0.6 s) charge, would avoid a dead input; test both blind.

### Cross weapon secondaries that combo (summary for design)

1. ULTRAKILL: coin (Marksman) redirects any hitscan, prioritises cores and rockets, adds +1 damage per coin on the standard revolver, splits shots at the arc peak. https://ultrakill.wiki.gg/wiki/Coin
2. ULTRAKILL: Core Eject core + any hitscan = bigger explosion (7 damage reported), radius doubles per hitscan type. https://ultrakill.wiki.gg/wiki/Core_Eject_Shotgun
3. Doom Eternal: sticky bomb and precision bolt both aimed at weak points, so shotgun and heavy cannon alts cover the same "break the weak point then finish" loop (https://redbull.com/ca-en/best-weapon-mods-doom-eternal).
4. Neon White: each card pairs a primary with a secondary movement/utility ability (Purify = machine gun + grenade; Elevate = sidearm + vertical boost; Godspeed = rifle + forward dash; Stomp = SMG + ground slam; Dominion = rocket launcher + grapple; Fireball = shotgun + air dash). Secondary is consumed by discarding the card. https://www.thegamer.com/neon-white-soul-cards-tips-tricks-guide/

## Part 2. Unlock pacing

### ULTRAKILL

- Start: revolver in the first level (0-1). Shotgun first unlocked in 0-3 DOUBLE DOWN, or 0-2 THE MEATGRINDER if the player beats a secret encounter. Nailgun in 1-1 HEART OF THE SUNRISE, or 1-2 if missed. Railcannon in 2-2 per the weapons page, but a search snippet put it in Layer 3 / Gluttony (conflict between sources; verify). https://ultrakill.wiki.gg/wiki/Weapons, https://ultrakill.fandom.com/wiki/Shotgun_(disambiguation), search snippet on earlyguides.com
- Variants are gated by purchase or later levels: Marksman 7,500 P, Pump Charge 12,500 P, Sharpshooter 25,000 P, Overheat 25,000 P, Jumpstart 35,000 P, Screwdriver and Malicious 100,000 P, Alternate revolvers after 1-4, alternate shotguns in 7-2, alternate nailguns in 4-4 (https://ultrakill.wiki.gg/wiki/Weapons, reported by summarizer and one entry for the revolver looked muddled). So earlier weapons stay relevant because the variant of each slot is a paid choice and every slot keeps its role.
- Level 0 and 1 emphasis: revolver's two firing modes and movement; the level text says the revolver teaches switching between the accurate shot and charged piercer (search snippet of https://cyberpost.co/what-is-the-first-mission-in-ultrakill/, weak source).
- Hakita's 1-1 Prelude devlog says the Nailgun arrived in a new level with a rebuild of pacing and enemy placement; it does not describe how the level teaches the gun. https://hakita.itch.io/ultrakill-prelude/devlog/154845/new-level-new-gun-reworked-prelude

### DOOM Eternal

- Starts with the shotgun and no infinite ammo pistol; the Super Shotgun is obtained in Cultist Base through a scripted neural link moment (search snippet of https://en.wikipedia.org/wiki/Doom_Eternal and https://doomwiki.org/wiki/Super_shotgun_(Doom_Eternal)).
- Teaching is through encounter design: the player must choose tools to exploit specific enemy weaknesses; no tutorial text needed (search snippet of the fandom escalation article, https://www.fandom.com/articles/doom-eternal-weapons-gameplay-escalation; page itself returned 402, so the claim is unverified).
- Mods unlock by finding mod bots; earlier weapons remain relevant because enemy weak points and resource economy (ammo, chainsaw, flame belch) force constant swapping. Unlock locations not found (TheGamer page says it does not list them).

### Neon White (best published on "right tool for the next encounter")

- Level design rules from the dev: the player always knows where to go next; always has the right tool for the next encounter; at least one non trivial shortcut; the shortcut cannot be hidden; the shortcut path must only use cards from the main path. https://www.gamedeveloper.com/business/-neon-white-and-designing-for-player-creativity
- New cards and mechanics are drip fed every few stages, cards placed right before they are needed; one example level (Dasher) exists to introduce blue demons that drop Godspeed and a chain of Godspeed dashes; each encounter is blocked out in isolation and then assembled, which creates shortcuts. https://www.gamedeveloper.com/road-to-igf-2023/-neon-white-and-designing-for-player-creativity (search snippet; the first gamedeveloper URL gave the rules above)
- Earlier cards stay relevant through slot limits: hold two different card types or up to three of the same kind; levels avoid asking for platforming, shooting and card management at once. https://www.gamedeveloper.com/business/-neon-white-and-designing-for-player-creativity
- Cards are stage specific pickups, not a loadout. https://www.thegamer.com/neon-white-soul-cards-tips-tricks-guide/

### Titanfall 2 campaign

- Titan loadouts are found as abandoned equipment in levels, typically just before a sequence that benefits from that loadout; roughly once per level the player gets in a Titan; eight loadouts total. https://titanfall.wiki.gg/Titanfall_2 and https://bit-tech.net/features/revisited-titanfall-2/1 (search snippets only, not fetched).

### Half-Life

- Half-Life 2: Ravenholm is described as the level that teaches the gravity gun is a weapon (ammo is scarce, saw blades and props reward using the tool). Source is a fan blog, low authority. https://gamertagmythras.com/blog/half-life-2/half-life-2-ravenholm-guide
- Crowbar first, 9mm pistol found early. https://giantbomb.com/wiki/Games/Half_Life_2

### Severed Steel

- Not found. Search returned only Firefight mode level unlocks and that the Flamethrower is a late unlock; nothing on per level weapon pacing.

### Design takeaways for a "pistol first, one weapon per level" ladder (derived from the above, mark as inference)

1. Place the new gun before the encounter that needs it (Neon White, Titanfall 2 loadouts).
2. Build one encounter around its secondary, not only its primary (Doom Eternal weak point enemies; ULTRAKILL cores need a hitscan).
3. Keep old weapons alive with slot or ammo pressure and with enemies that punish one tool (Doom, Neon White card limits).
4. Give each new secondary a combo partner with the pistol or the previous weapon (ULTRAKILL coin + revolver + core).

## Not found / unverified

- Titanfall 2 alt fire design for every weapon; no first party numbers.
- Destiny charge feel (haptics, tells); only a charge stat of 533 reported.
- Quake Champions secondaries (appears to have none).
- DOOM Eternal cooldown seconds and damage multipliers per mod; unlock missions.
- Severed Steel weapon pacing.
- Any source on release vs auto-fire at full charge as a design rule.
- Charge-time numbers for Halo Plasma Pistol (URL 404).
- ULTRAKILL railcannon unlock level conflicts between sources.

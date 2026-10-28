# Weapon swapping and the arsenal HUD (wave 34 research, 2026-10-11)

Web research only, about 30 tool calls. Every claim carries its source URL. Where a page could not be read
(doomwiki.org returned 403, ultrakill.fandom.com returned 402) the claim is marked "search snippet only".
"Not found" means the search did not turn it up, not that it does not exist.

## Answer

The games that make swapping a combo verb do three things: give each weapon (and each variant) its own cooldown
that keeps ticking while holstered, so swapping skips a reload rather than bypassing a cooldown (ULTRAKILL shotgun
variants, Doom Eternal Ballista / Super Shotgun); reward variety directly (ULTRAKILL's per-weapon freshness feeds the
style score); and keep swap input short and direct (number keys, same key cycles variants, a hold-wheel that slows
time for the pad). ULTRAKILL's own HUD does NOT show the full arsenal: its HUD shows only the selected weapon's icon,
tinted by variant colour, plus the railcannon charge meter and the freshness bar; the full set appears only in the
hold-to-open weapon wheel. So "an ULTRAKILL-style display that shows your full arsenal" is a new design that mixes
ULTRAKILL's colour-coded current-weapon icon with an always-on slot strip. For a pad-first 5-7 weapon game the
evidence points to: Y (or RB) for next / last weapon, a held bumper for a radial wheel with time slowed, a compact
always-visible slot strip with per-slot cooldown fills, and independent per-weapon cooldowns that tick while holstered.

## 1. ULTRAKILL

- HUD types: "Standard" places the HUD "to the side of the screen with each HUD component nestled to each other in a
  rectangular formation, angled slightly towards the center of the screen"; "Classic Color" / "Classic White" lock it
  "to the bottom of the screen" with numeric health and stamina. https://ultrakill.wiki.gg/wiki/HUD
- Weapon icon: an option "Displays the selected Weapon icon in HUD". The page describes only the current weapon, not
  the whole arsenal. https://ultrakill.wiki.gg/wiki/HUD and https://ultrakill.wiki.gg/wiki/Saves
- Variant colours: HUD colour options exist for Blue, Green, Red and Gold variations. https://ultrakill.wiki.gg/wiki/HUD
  Most weapons have Blue / Green / Red variants; arms run Blue, Red, Green, Gold and their colour cannot be customised.
  https://ultrakill.wiki.gg/wiki/Weapons and https://ultrakill.wiki.gg/wiki/Arms
- Charge meters: the HUD can show "the charge meter for the Railcannon"; railcannon variants share a 16 s cooldown,
  shown as a lightning-bolt meter, and the gun makes an electric noise when ready "even if it is not currently
  equipped" (search snippet of https://ultrakill.fandom.com/wiki/Electric_Railcannon). Style, movement multiplier and
  "Weapon Freshness" can also be shown. https://ultrakill.wiki.gg/wiki/HUD
- Weapon position: the view-model can be moved right / middle / left in settings. https://ultrakill.wiki.gg/wiki/Weapons
- Swapping: "On Swap to Already Drawn Weapon" chooses Next Variation, First Variation or Same Variation; "Remember Last
  Used Weapon Variation" (off by default) returns you to the variant you left; "Scroll Weapons With Mouse Wheel" (on by
  default) with "Weapon Scroll Type" Weapons / Variations / Both. https://ultrakill.wiki.gg/wiki/Saves
- Weapon wheel: holding "Previous Variation" (default Q) "brings up a weapon wheel around the player's reticle and
  slows down time"; pick with mouse or thumbstick and release. https://ultrakill.wiki.gg/wiki/Weapons
  On controller it opens from "Next Variation" on the Right Bumper. https://ultrakill.wiki.gg/wiki/Saves
- Default number-key layout and a "last weapon" bind: not found. Swap / draw time in seconds: not found.
- Swap cancelling: "Shotgun variants can be quickly swapped between to negate the weapon's reload animation, due to
  their independent cooldowns." Cooldowns: Core Eject 1.25 s, Pump Charge 0.9 s, Sawed-On 1.25 s.
  https://ultrakill.wiki.gg/wiki/Pump_Charge_Shotgun
- The price of the tech is freshness: "the quick depletion of weapon freshness on each Shotgun variant, which can
  severely affect Style." https://ultrakill.wiki.gg/wiki/Pump_Charge_Shotgun
- Why swapping is rewarded: a bar under the style meter shows how "Fresh" a weapon is; the fresher, the more style;
  using one weapon (and its variants) drops the multiplier until score goes static (search snippet of
  https://ultrakill.fandom.com/wiki/Style_Index ). Exact freshness state names and rates: not checked (page 402).
- Coins: 4 coins, recharging "4 seconds per coin, one coin at a time"; whether they recharge while holstered is not
  stated. https://ultrakill.wiki.gg/wiki/Coin
- No Switch port: not checked this round.

## 2. Doom Eternal

- Quick switch: Update 2.1 (1.06) notes say "Weapon Quick Switch has been enabled again in both Campaign and
  BATTLEMODE", after community feedback, so an earlier update had disabled it. https://www.thesixthaxis.com/?p=362144
  Why it was disabled and the exact patch: not found.
- The Super Shotgun's reload "can be mitigated by quick-switching to and from other weapons" (search snippet of
  https://doomwiki.org/wiki/Super_shotgun_(Doom_Eternal) , page 403).
- Weapon deploy times in seconds: not found.
- Weapon wheel: time slows while it is open but "the action is not completely paused".
  https://twinfinite.net/guides/doom-eternal-switch-weapons/ (search snippet). The exact factor: not found.
- The wheel slows the tick rate but not the frame rate, which players exploit with the meathook ("yeethooking")
  (search snippet of https://doomwiki.org/wiki/Meat_hook ).
- How id made swap combos the intended play (designer quotes): not found this round.
- Switch port: gyro aiming, judged responsive but needing tuning. https://vooks.net/doom-eternal-switch-review
  Its pad wheel layout on Switch: not found.

## 3. Quake, TF2, Titanfall 2, Halo, Splitgate

- Quake Live: switch time described as the current weapon's reload time plus 400 ms (community post, unverified).
  https://steamcommunity.com/app/282440/discussions/0/613937306688042774
- Quake 3 / Champions exact drop and raise ms: not found.
- TF2 standard deploy time: not found (the weapons overview page states none). https://wiki.teamfortress.com/wiki/Weapons
- Titanfall 2: a "Quick Swap" mod gives faster swap to that weapon (search snippet; source page not opened). Seconds:
  not found.
- Halo: Y has been the switch-weapon button across the series (CE, 2, 3, Infinite).
  https://halopedia.org/CE:Control_schemes and https://halopedia.org/H3:Control_schemes and
  https://www.shacknews.com/article/127720/halo-infinite-controls-and-keybindings
- Halo Infinite made drop-weapon ready-up time equal normal swap time (search snippet of
  https://gamespot.com/articles/halo-infinite-february-update-will-nerf-picking-up-and-dropping-weapons/1100-6511436/ ).
- Splitgate: Y / Triangle is "Next Weapon" on pad. https://sportskeeda.com/esports/the-best-controller-settings-splitgate
- Destiny 2 shipped then softened a weapon-swap delay after player pushback, a warning that swap-cancel nerfs feel bad.
  https://gamespot.com/articles/destiny-2s-new-weapon-swap-delay-will-be-dialed-back-soon/1100-6514009/
- ULTRAKILL on pad: RB opens the wheel (section 1). Doom Eternal on Switch: see section 2.

## 4. HUD patterns for "full arsenal visible, current highlighted"

- ULTRAKILL itself: current weapon only, colour-coded by variant, side-angled panel or bottom bar; the full set only
  in the wheel. https://ultrakill.wiki.gg/wiki/HUD and https://ultrakill.wiki.gg/wiki/Weapons
- Per-weapon readiness off the current weapon: ULTRAKILL gives the railcannon its own meter plus an audio cue when
  holstered (section 1). The community made mods for more per-weapon cooldown info, which suggests the base HUD
  under-reports it. https://thunderstore.io/c/ultrakill/p/daemon47/UltraCooldownInfo/ and
  https://thunderstore.io/c/ultrakill/p/Jade_Harley/Better_Weapon_HUDs/changelog
- The Game UI Database (55,000+ screenshots of HUDs, menus, health bars) is the place to compare strips vs wheels.
  https://aftermath.site/game-ui-database . A direct ULTRAKILL / Doom Eternal breakdown article there: not found.
- Bar vs wheel sizes and placements as measured numbers: not found.

## 5. Swap logic in code

- Independent per-weapon cooldowns are what makes swap cancelling work and what keeps it fair: swapping skips the
  current weapon's animation, but each weapon still owes its own cooldown. https://ultrakill.wiki.gg/wiki/Pump_Charge_Shotgun
- Shared cooldown as an anti-exploit: all railcannon variants share one 16 s cooldown, so variant cycling cannot skip
  it (search snippet of https://ultrakill.fandom.com/wiki/Electric_Railcannon ).
- Cooldowns ticking while holstered: the railcannon readies while unequipped (same snippet). Coins: not stated.
- Input buffering: an input buffer holds a press made slightly before an animation ends so it still fires.
  https://www.wayline.io/blog/unity-combat-input-buffer . A shooter-specific swap buffer spec: not found.
- Open-source code: a "Simple FPS Weapon System" (Godot, listed as MIT) https://godotengine.org/asset-library/asset/edit/18323
  and a Unity framework https://github.com/unitycoder/Unity-Hyper-FPS-FrameWork-Open-Source- ; licences and swap
  logic not checked. Nothing found that implements holstered cooldowns plus swap buffering.

## 6. Melee hitbox generosity

- Principle: players like their own hitbox smaller than drawn and the enemy's bigger; faster games can afford looser
  boxes; 1:1 model boxes give values "that fluctuate wildly between frames".
  https://critpoints.net/2015/05/20/why-shouldnt-hitboxes-match-perfectly/
- Sphere casts for melee and hitscan assist (ray first, sphere cast as fallback, snap to centre of mass) are a common
  pattern (search snippets; e.g. https://docs.page/soriox/TPS-Docs/classes/BP_ThirdPersonCharacter/Melee ).
- Halo lunge: when the target is close enough that the reticle turns red, melee lunges the player forward before the
  strike; sword lunge is far longer than a gun-butt's. Distances in metres: not found. https://www.halopedia.org/Lunge
  Magnetism slows turning near a target. https://www.halopedia.org/Magnetism
- Doom Eternal: melee lunge and glory-kill reach of 6-7 m for most demons in vanilla, per a mod's description
  (search snippet of https://nexusmods.com/doometernal/mods/1205 ). Blood Punch adds damage, range and AoE: two
  circular areas centred on the Slayer (search snippet of https://breezewiki.discard.no/doom/wiki/Blood_Punch ).
  Radius numbers: not found.
- ULTRAKILL Feedbacker: a 1-damage punch with a small cooldown, parries projectiles and coins (5 base damage on parry);
  Knuckleblaster: 2.5 damage, knockback, hold for a shockwave, cannot parry. https://ultrakill.wiki.gg/wiki/Arms
  Hitbox size or range numbers: not found.

## Recommended pattern for a pad-first 5-7 weapon game

1. Give every weapon its own cooldown that ticks while holstered, and let a swap skip only the current weapon's
   recovery animation, never another weapon's cooldown. That is ULTRAKILL's shotgun tech and why it stays fair.
   https://ultrakill.wiki.gg/wiki/Pump_Charge_Shotgun
2. Where a weapon has variants, share one cooldown across them so variant cycling cannot skip it (the railcannon's
   16 s). https://ultrakill.fandom.com/wiki/Electric_Railcannon (search snippet)
3. Reward the swap through the score, not only through DPS: a per-weapon freshness that drains with repeated use and
   recovers on others, shown under the style meter. https://ultrakill.fandom.com/wiki/Style_Index (search snippet)
4. Pad layout: Y for next weapon (Halo, Splitgate convention), and hold a bumper for a radial wheel around the
   reticle that slows time but does not pause it (ULTRAKILL on RB, Doom Eternal).
   https://sportskeeda.com/esports/the-best-controller-settings-splitgate , https://ultrakill.wiki.gg/wiki/Saves ,
   https://twinfinite.net/guides/doom-eternal-switch-weapons/
5. Run the wheel slowdown on the sim's own clock so a slowed tick rate cannot change movement physics, the
   bug Doom Eternal's meathook "yeethook" shows. https://doomwiki.org/wiki/Meat_hook (search snippet)
6. Offer ULTRAKILL's swap options: what pressing the drawn weapon's slot does (next / first / same variant) and
   whether a weapon remembers its last variant. https://ultrakill.wiki.gg/wiki/Saves
7. The arsenal HUD: keep ULTRAKILL's large current-weapon icon tinted by variant colour, and add (new, not
   ULTRAKILL) a compact always-on strip of all 5-7 slots with the current one enlarged and each slot's cooldown as a
   fill, because ULTRAKILL players added cooldown mods to see that.
   https://ultrakill.wiki.gg/wiki/HUD , https://thunderstore.io/c/ultrakill/p/daemon47/UltraCooldownInfo/
8. Give a holstered weapon becoming ready a one-shot cue (the railcannon's ready noise); here a visual pulse on its
   slot, since sound belongs to GG. https://ultrakill.fandom.com/wiki/Electric_Railcannon (search snippet)
9. Do not add a global swap delay to stop swap cancelling: Destiny 2's had to be dialed back.
   https://gamespot.com/articles/destiny-2s-new-weapon-swap-delay-will-be-dialed-back-soon/1100-6514009/
10. Melee: make the enemy hurtbox or the melee sphere cast bigger than the model, and add a short lunge toward a
    target in range. https://critpoints.net/2015/05/20/why-shouldnt-hitboxes-match-perfectly/ ,
    https://www.halopedia.org/Lunge

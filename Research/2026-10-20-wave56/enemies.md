# Enemies: threat, readability, encounters (wave 56 research)

2026-10-20, web research only (WebSearch / WebFetch), about 12 minutes. Covers behaviour and encounters, not enemy looks
(GG designs those).

**Verified** means I fetched the page and read the claim there. **Reported** means it comes from a search summary, a
third-party summary, or a page I could not fetch (403 / 402 / truncated).

## The sharp question

What makes horde and arena enemies feel threatening and readable at speed, and how do shipped games build encounters
from simple roles? Short answer from the sources:

- Limit how many enemies attack at once, and prefer attackers the player can see.
- Make ranged enemies stay visible.
- Escalate hits into a stagger the player can cash in.
- Pay the player for closing distance, with health, ammo and armour from kills.
- Build each wave from a points budget with role tiers.
- Start the next group when the player kills the current one, not on a timer.

## Methods

Each method gives the rule, its numbers, its source, and how to measure it in the bot harness or on a Player frame.
Every SKYGRIND number named below is a proposal for `config.json` or a Unity-side config file, never hand-typed.

### M1. Attack tokens per attack type (DOOM 2016)

- **Rule:** each attack type (melee, ranged, charge) has its own capped token pool. A demon must hold a token to attack
  and gives it back afterwards. A better-placed demon can take another demon's token, so the demons in front of the
  player are the ones attacking. Token counts are set per difficulty. No counts were published.
- **Source (Verified):** https://www.gamedeveloper.com/design/cyber-demons-the-ai-of-doom-2016- and
  https://80.lv/articles/cyber-demons-the-ai-of-doom
- **Source (Reported):** stealing is mostly an on-screen enemy taking an off-screen enemy's token, and higher
  difficulties have bigger pools. This comes from a devlog summary of the GDC talk:
  https://serfofcinder.itch.io/archetype-portal-to-hell/devlog/738495/enemy-ai-tech-talk
- **For SKYGRIND:** husk and swarmer share a melee pool; spitter and drone share a ranged pool; charger has a charge
  pool of 1. Pool sizes scale with the round, not the difficulty.
- **Measure:**
  - Log the tokens held per type at each step. The count never exceeds the pool.
  - At least 80 % of attack starts come from enemies inside the camera frustum, with an unoccluded ray.
  - Damage from off-screen attackers is under 20 % of damage taken.

### M2. Enemy accuracy drops as the player moves faster (DOOM 2016)

- **Rule:** a weighted distribution lowers demon accuracy as player speed rises. Higher difficulties compensate more.
  This is what makes moving fast feel like defence.
- **Source (Verified):** https://80.lv/articles/cyber-demons-the-ai-of-doom
- **Measure:**
  - Bucket spitter and drone hit rates by player speed (standing, running, sliding, dashing).
  - At dash speed the hit rate is half or less of the standing hit rate.
  - Draw the spread from the per-run `Mulberry32`, never `UnityEngine.Random`.

### M3. "Exposed cover": ranged enemies keep their distance and stay visible (DOOM 2016)

- **Rule:** imps, cacodemons and mancubi keep away from the player when they can. DOOM turned Rage's cover system
  around: enemies pick open spots near cover that give "maximum visibility to the player" instead of hiding.
- **Source (Verified):** https://www.gamedeveloper.com/design/cyber-demons-the-ai-of-doom-2016-
- **Measure:**
  - Spitters and drones are visible from the player camera (unoccluded ray) for at least 70 % of their alive time.
  - Their median distance to the player sits inside a config band. The band is ours; the sources give no number.

### M4. Pain escalates into a stagger; a finisher makes the player safe (DOOM 2016)

- **Rule:** damage climbs a ladder: twitch (slightly spoils the enemy's aim), falter or pushback (interrupts its
  behaviour), then stagger (a glory kill is possible). The player is invincible during a glory kill, and AI may not
  start a new attack until it ends.
- **Source (Verified):** https://www.gamedeveloper.com/design/cyber-demons-the-ai-of-doom-2016-
- **Measure:**
  - Attack starts during a finisher or execution window = 0.
  - Per role, log the time from first hit to stagger and how many falters come before it.
  - Every stagger has a visible state change on the Player frame, measured as a pixel delta on the enemy's
    screen box.

### M5. Each weapon gets a role-matched reaction (DOOM 2016)

- **Rule:** imps stop moving when hit by the assault rifle, and zombie fodder reaches stagger quickly from the shotgun.
  A perpetual minigun falter on the cacodemon was cut before launch because it was too strong.
- **Source (Verified):** https://www.gamedeveloper.com/design/cyber-demons-the-ai-of-doom-2016-
- **Measure:**
  - Build a weapon x enemy matrix of time-to-stagger and of "movement stopped" steps.
  - Each enemy has one weapon that is clearly best (at least 1.5x faster than its median weapon).
  - No pairing keeps an enemy locked out for more than its attack cycle.

### M6. Push-forward resource loop: kills are the pickups (DOOM 2016 / Eternal)

- **Rule:** glory kills drop health, chainsaw kills drop ammo, and the flame belch makes enemies drop armour. Pickups:
  - health +15 / +25 / +50
  - armour +10 / +30 / +50
  - "Enemies drop more health after a glory kill or if the player is below 50 health."
  - Chainsaw fuel: 1 to kill weak demons, up to 5 for big ones.
- **Source (Verified):** the push-forward framing, "instantly recover health and ammo while in a fight":
  https://www.gamedeveloper.com/design/how-doom-s-push-forward-design-cured-my-hoarder-syndrome
- **Source (Reported, doomwiki returned 403):** the numbers: https://doomwiki.org/wiki/Supplies_(Doom_Eternal)
- **Talk:** GDC 2018 "Embracing Push Forward Combat in DOOM" (Vault page not read):
  https://gdcvault.com/play/1024940/Embracing-Push-Forward-Combat-in
- **For SKYGRIND:** this fits GG's "no passive in-round regen" rule, because health comes from kills, not time.
- **Measure:**
  - Median distance to the nearest enemy while player HP is under 30 %. Push-forward means it falls; a retreat
    means it rises.
  - The share of in-round healing that came from kills within 5 m is at least 60 %.

### M7. A heavy's punish window is sized to the weapons (DOOM Eternal's Marauder)

- **Rule:** the Marauder falters long enough for at least one shot with most weapons, and just long enough for two
  shots if the player swaps weapons. It teaches weapon swapping, and it changes the player's priorities in a mixed arena.
- **Source (Reported, the page body was truncated):**
  https://www.pcgamer.com/uk/doom-eternals-director-says-the-marauder-is-good-actually/
- **For SKYGRIND:** apply this to the warden's shield drop and the charger's recovery.
- **Measure:** in steps, the vulnerable window is at least one shot interval of the slowest gun and less than two
  shot intervals of the fastest gun, or two with a swap.

### M8. A two-state attack tell: parryable or not (ULTRAKILL)

- **Rule:** a yellow flash plus a sound means the attack can be parried; a blue flash plus a different sound means it
  can't. A successful parry gives a short freeze and a white screen flash. Players say the windup flash matters more
  than the projectile's colour.
- **Source (Reported):** https://ultrakill.wiki.gg/wiki/Parry . GGRecon gives white / red instead
  (https://ggrecon.com/guides/ultrakill-tips), so the sources conflict.
- **For SKYGRIND:**
  - Yellow and blue break our palette (cyan is the player's, red-orange is danger). Encode the tell in the danger
    family by brightness, size or pulse.
  - The sound belongs to GG: fire a named event only.
- **Measure:**
  - Every enemy attack emits a windup event at least N steps (config) before its damage step.
  - On a Player frame, the tell's luminance contrast against the enemy body passes a threshold.
  - Bot parry success sits in a target band.

### M9. Build waves from a points budget with role tiers (ULTRAKILL Cyber Grind)

- **Rule:**
  - The budget starts at 10 points and grows by `3 + floor(wave/3)` each wave (wave 5 = 28, wave 10 = 55).
  - Costs: Filth 1, Stray 2, Drone 4, Schism 5, Streetcleaner 5, Malicious Face 13+5, Cerberus 16+2.
  - Uncommon enemies from wave 12, specials from wave 16, radiant (buffed) enemies from wave 25.
  - The Hideous Mass (the heavy) spawns first, and only while more than 70 points remain.
  - Each Hideous Mass or uncommon spawn blocks specials for 1 wave.
  - Commons spawn last, 0.1 s apart.
  - Each spawn prefab holds one enemy, with separate melee (n) and projectile (p) spawn points.
  - Full health and stamina, plus cooldown resets, at each wave start.
- **Source (Verified):** https://ultrakill.wiki.gg/wiki/Cyber_Grind
- **Measure:**
  - Log the budget spent per wave and the role mix (fodder / ranged / heavy).
  - At most 1 heavy per wave before a config round.
  - Ranged enemies only on ranged spawn points.
  - A heavy and a special never land in the same wave until late rounds.

### M10. Waves chain on kills, not timers, and spread around the player (ULTRAKILL campaign)

- **Rule:** in 1-1, two Drones appear at the door, and killing both summons four more, "one in each corner". Other
  arenas are cut into three spawn waves (7-3), and a heavy appears once the fodder is cleared (0-4).
- **Source (Reported, fandom returned 402 to the fetch):**
  https://ultrakill.fandom.com/wiki/1-1:_HEART_OF_THE_SUNRISE and
  https://ultrakill.fandom.com/wiki/7-3:_NO_SOUND,_NO_MEMORY
- **Measure:**
  - The gap from the last kill of group k to the first spawn of group k+1 is at most a config value (for example
    1.5 s).
  - A corner wave's spawn bearings around the player cover at least 3 of 4 quadrants.
  - At least 1 spawn per group lands inside the camera frustum, so the player sees the group arrive.

### M11. An active cap with refill (CoD Zombies)

- **Rule:** at most 24 zombies are active at once, and spawning pauses until some die. Per-round totals grow
  quadratically with the round. The figures come from fan analysis; the Black Ops 3 cap is said to come from reading
  the game's code.
- **Source (Reported):**
  https://expertbeacon.com/how-many-zombies-are-in-each-round-black-ops-3/ and
  https://www.callofdutyzombies.com/topic/145097-2-players-6-more-zombies-30-or-not
- **For SKYGRIND:** in small rooms the cap should scale with room area.
- **Measure:**
  - Active count never exceeds the cap.
  - The share of combat time spent at the cap is tracked, and stays at or under 50 %, or the room is under-built.

### M12. Territory, the 30-second loop and "good mistakes" (Halo)

- **Rule:**
  - Encounters follow a loop: enter a space, plan an approach, carry it out ("30 seconds of fun"), chained
    "200 or 300" times.
  - AI holds assigned territory, so opening fire doesn't bring "the whole room of enemies" down on the player.
  - AI assumes the player is still at the last known position.
  - "Good mistakes" give openings: a Hunter that misses its melee has a long recovery the player can flank.
- **Source (Verified):** https://gamedeveloper.com/game-platforms/in-depth-bungie-on-eight-years-of-i-halo-i-ai
- **Measure:**
  - Room fight length runs from the first enemy seen to the last kill. As a first band, the median per group is about
    30 s, and a room runs 2 to 3 groups. The band is my inference from Halo's loop; no source gives room lengths.
  - Fewer than 50 % of the room's enemies are alerted by the first shot.
  - A charger's recovery after a missed lunge is long enough for the bot to reach its flank.

### M13. Traversal that adapts at runtime (DOOM 2016)

- **Rule:** imps make short or long jumps from one jump animation adjusted at runtime. Hell Knights check the
  environment so their lunges don't clip geometry.
- **Source (Verified):** https://www.gamedeveloper.com/design/cyber-demons-the-ai-of-doom-2016-
- **No source found** for enemies flanking or climbing to every floor.
- **For SKYGRIND:** GG's rule "There always really needs to be a route that enemies can climb up" stays our own
  requirement.
- **Measure:**
  - Stand the bot on each floor level of a room. Every melee role reaches attack range within a config time (for
    example 8 s).
  - The share of player positions where no melee enemy can path to the player is 0.

### M14. Spawn location settings (DOOM SnapMap; partial)

- **Rule:** an encounter spawns at the activator's location or at the encounter object's location, inside a
  "Spawn Proximity" area. It fires "On Demon Spawned" and "On Encounter Percent Finished" signals. A per-encounter
  stagger override sets the health percentage at which a demon staggers.
- **Source (Verified):** https://wiki.bethesda.net/wiki/snapwiki/Doom/Single_Encounter/
- **Players:** they read DOOM Eternal spawns by "their distinctive spawning sound" and a colour. This is Reported, from
  a Steam thread: https://steamcommunity.com/app/782330/discussions/0/4211496988420357794
- **Not found:** any primary source on DOOM Eternal's portal-spawn design or on ULTRAKILL's spawn flash.
- **Measure:** see M10's frustum share. Also emit a spawn-tell event at least N steps before the enemy becomes active
  (config) and confirm it on a Player frame with a pixel delta at the spawn point.

## Not checked or not found

- The talk's own content: the GDC Vault page and its slides were not read (paywalled).
- DOOM's real token counts: none were published.
- Exact glory-kill drop amounts per enemy.
- The Left 4 Dead Director's relax / build-up / peak timings: only fan summaries came back, so it is left out.
- Halo attack slots: no source uses that term.
- Halo's "The Illusion of Intelligence" PDF: it would not decode through WebFetch.

# Feel research (wave 39): aim slow-mo, projectile parry, tap vs hold charge

Web research, 2026-10-13, about 6 minutes. Each claim carries its URL. "Not found" means no source turned up in this
pass, not that no number exists. Search summaries were cross-checked by fetching the page where marked "(fetched)".

## 1. Slow motion while aiming (wall run / grappled)

| Game | What is sourced | Number | URL |
|---|---|---|---|
| Titanfall 2 | No source found that Titanfall 2 slows time on ADS during a wall run. Its campaign's time mechanic is the "Effect and Cause" past/present shift, not slow-mo. | not found | https://en.wikipedia.org/wiki/Effect_and_Cause |
| Ghostrunner | Sensory Boost: hold Shift / LB to slow time and dash a short distance in the facing direction; held in the air it slows time "for a few seconds" and the player can strafe to dodge. | time scale: not found; duration "a few seconds" (no exact number) | https://godisageek.com/2020/10/ghostrunner-beginners-tips/ |
| Ghostrunner 2 | Sensory Boost costs stamina (a meter); the "Artificial Senses" upgrade makes it cost 25% less stamina. | 25% cost cut | https://store.epicgames.com/news/ghostrunner-2-the-best-skills-to-unlock-throughout-the-campaign |
| Max Payne | Bullet time slows the world for a brief period, standing or in a dive; producer Scott Miller's pitch was a button that shifts to slow motion "with the exception that Max could still aim in real time". | time scale: not found | https://www.timeextension.com/news/2023/03/max-payne-producer-reveals-how-bullet-time-mechanic-was-born ; https://en.wikipedia.org/wiki/Max_Payne_(video_game) |
| Mirror's Edge | Reaction Time temporarily slows gameplay so the player can time the next move without losing momentum. | time scale and duration: not found | https://en.wikipedia.org/wiki/Mirror%27s_Edge |
| Severed Steel | A slow-motion button drains a meter; the meter does not drain while diving, wall running or sliding; kills refill it; default bullet time is 10-20 s of meter. | 10-20 s meter; time scale: not found | https://bit-tech.net/reviews/severed-steel-review/1/ ; https://giantbomb.com/users/7052/articles/mooseys-not-so-hidden-gems-severed-steel |
| Neon White | not checked (no source found in this pass) | not found | none |

Takeaways backed by the sources above:
- Max Payne's design rule: the world slows, the player's aim does not (aim runs in real time). Source: timeextension link.
- Severed Steel ties the meter to movement: wall run / slide / dive spend nothing, which pays the player to keep moving
  while slowed. This is the closest shipped match to "slow-mo while aiming on a wall run". Source: bit-tech / giantbomb.
- Ghostrunner gates it with a stamina cost and lets the player reposition (strafe) inside the slow. Source: godisageek,
  Epic.
- Ramp-in / ramp-out times and exact time scales: not found for any of these games.

## 2. Projectile parry, hitstop and slow-mo

| Game / source | What is sourced | Number | URL |
|---|---|---|---|
| ULTRAKILL | A parry freezes time for a quarter second, classed as a "major" hitstop. | 0.25 s freeze | https://ultrakill.wiki.gg/wiki/Hitstop (search snippet; the page returned 404 / access denied when fetched, so the number is from the search index only) |
| ULTRAKILL | "Successfully parrying will cause the game to briefly freeze and flash the screen white, before healing the player fully"; parry restores stamina fully and health up to the hard-damage cap. (fetched) | parry window ms: not found; speed / damage multipliers: not found | https://ultrakill.wiki.gg/wiki/Parry |
| ULTRAKILL | Projectile boost: the player can parry their own shotgun pellet or cannonball; the parried pellet explodes on contact; the cannonball loses gravity and deals more damage; earns the "+ PROJECTILE BOOST" style bonus. | not found | https://ultrakill.wiki.gg/wiki/Parry ; https://gamerjournalist.com/how-to-parry-your-shotgun-in-ultrakill/ |
| Sekiro | Deflect window about 30 frames (0.5 s) by default, shrinking to about 7 frames if guard was released recently (anti-mash). Community-measured; another report gives 200 ms. | ~30 f / ~7 f (conflicting) | https://sekiroshadowsdietwice.wiki.fextralife.com/Deflection ; https://steamcommunity.com/app/814380/discussions/0/1746772719869502260 |
| Fighting games (general) | Hitstop scales with hit weight; one example: light / medium / heavy = 8 / 12 / 16 frames of hit/block stop. Parry and armour moves can change how much hitstop each side gets. | 8 / 12 / 16 f (at 60 fps: 133 / 200 / 267 ms) | https://slapcity.wiki.gg/wiki/Hitstop ; https://forums.ultra-combo.com/t/why-isnt-hitstop-duration-standardized/24838 |
| Street Fighter 2 / Smash | SF2 hitstop "froze the state of both characters for like 10 frames"; Smash vibrates characters during hitstop (horizontal on ground, vertical in air). Hitstop gives "the eyes a few frames to register and confirm it happened". (fetched) | ~10 f | https://critpoints.net/2017/05/17/hitstophitfreezehitlaghitpausehitshit/ |
| Sakurai (Smash) | Famitsu column on hitstop: both parties freeze briefly on a hit to sell the impact. | not found | https://sourcegaming.info/2015/11/11/thoughts-on-hitstop-sakurais-famitsu-column-vol-490-1/ |
| Furi, Hi-Fi Rush | not checked (no source found in this pass) | not found | none |

Takeaways backed by the sources above:
- ULTRAKILL's parry is a 0.25 s freeze + white flash + full heal; no slow-mo window after it is documented.
- Scale hitstop by weight (8/12/16 f pattern) and shake / vibrate the hit actor during the freeze (Smash).
- How projectile speed is shown so a parry can be read: not found for any of these games.

## 3. Tap vs hold on a charge weapon

| Game / source | What is sourced | Number | URL |
|---|---|---|---|
| ULTRAKILL Piercer revolver | Charge is on alt fire (separate button from primary); held to charge "over the course of half a second"; released early "the weapon winds back down to its default state without firing"; primary fire works normally during the 2.5 s alt cooldown. (fetched) | 0.5 s charge; early release = no shot; 2.5 s cooldown | https://ultrakill.wiki.gg/wiki/Piercer_Revolver |
| Hollow Knight nail arts | Hold attack about 1.3 s (cited as 1.35 s) to charge a nail art; Nailmaster's Glory cuts it to 0.75 s. The tap still swings the nail normally (fire on press), the hold charges for a nail art on release. | 1.35 s / 0.75 s | https://hollowknight.wiki.fextralife.com/Nailmaster's+Glory ; https://hollowknight.wiki.fextralife.com/Nail+Arts |
| Destiny 2 fusion rifles | Charge times by frame, rescaled in Renegades: Rapid-Fire 500 -> 533 ms, Adaptive / Aggressive 660 -> 667 ms, Precision 780 -> 800 ms, High-Impact 960 -> 967 ms (older Rapid-Fire 460 ms). Whether it fires on charge-complete or on release: not found in sources fetched. | 460-967 ms | https://destinybulletin.com/news/weapon-archetype-changes-in-destiny-2-renegades-fusion-charge-times-shift-shotguns-get-pve-love-snipers-flinch-less-rocket-pulses-lose-reserves/ |
| Halo plasma pistol | Hold the trigger a few seconds to gather an overcharge, release to fire one large bolt; overcharge then costs about 2 s of overheat (Halo CE). | charge "a few seconds"; ~2 s overheat | https://www.halopedia.org/Plasma_pistol |
| INVERSUS (Ryan Juckett, Game Developer) | Fire-on-release (needed to tell tap from charge) costs ~0.15 s of perceived lag (press-release time). Fix: on press, a brief parry/block window opens at once; on a quick release, the shot spawns shifted back along the lateral axis to undo the player's sideways drift, full correction 0-0.11 s, fading to 0 by 0.19 s. (fetched) | ~150 ms; 110 / 190 ms | https://www.gamedeveloper.com/design/reducing-perceived-attack-delay-in-inversus |
| Game Developer (designer input latency) | Casual players may need up to ~200 ms to deliberately choose hold over tap; there is often at least ~100 ms before an average player gets a finger off the button. | 100-200 ms | https://www.gamedeveloper.com/design/opinion-dealing-with-designer-input-latency |
| Unity Input System defaults | defaultTapTime 0.2 s; defaultSlowTapTime 0.5 s; defaultHoldTime 0.4 s; multiTapDelayTime 0.75 s; defaultButtonPressPoint 0.5. Tap = press and release within duration; SlowTap = hold at least duration then release; Hold = performs once held for duration. (fetched) | 200 / 400 / 500 ms | https://docs.unity3d.com/Packages/com.unity.inputsystem@1.8/api/UnityEngine.InputSystem.InputSettings.html ; https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/Interactions.html |
| Fighting-game negative edge | not checked (no source found in this pass) | not found | none |

Takeaways backed by the sources above:
- The cleanest shipped answer avoids the problem: ULTRAKILL puts the charge on a different button (alt fire), and an
  early release fires nothing, so a tap can never spend the charge shot.
- If one button must do both, the tap has to fire on release (INVERSUS); the sourced cost is ~150 ms, and the sourced
  fixes are an instant press-time effect (INVERSUS's parry) plus a spawn-position correction for the delay.
- Threshold numbers with a source: 200 ms tap ceiling (Unity default, and Game Developer's casual figure), 100 ms
  minimum release time for an average player, 400 ms Unity default hold.

## Not found in this pass

Time scales and ramp-in/out times for Ghostrunner, Max Payne, Mirror's Edge, Severed Steel and Neon White; any Titanfall 2
ADS slow-mo; ULTRAKILL parry window and projectile speed multipliers; Furi and Hi-Fi Rush parry numbers; how any game shows
projectile speed for parry reads; audio treatment of slow-mo; fighting-game negative-edge rules; Destiny fusion
fire-on-release vs fire-on-complete.

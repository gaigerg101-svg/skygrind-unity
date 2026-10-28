# M2: Boss balance and grapple limits (research, 2026-10-10)

Method: web search plus page fetches. Many wikis returned 402/403, so several numbers come from search snippets or a summarizing fetch; those are marked "unverified". "Inferred" = my reasoning, not a source. SKYGRIND weapon DPS was not read here ("not checked"); fill the formula from `config.json`.

## (1) Recommended spec

### Boss kill time
- **Target band: 45-75 s** for a competent player who hits most shots (GG: "like a minute"). A first-time player should still finish in 120 s or less. Hard cap: no boss over 150 s on the bot.
- **HP formula (inferred, standard DPS-budget method):** `HP = T_target x DPS_sustained x hit_rate x uptime`.
  - `DPS_sustained`: the best on-hand weapon for that boss, from `config.json`, charge and reload included.
  - `hit_rate`: 0.6 for pad aim.
  - `uptime`: fraction of the fight the boss is hittable, about 0.75.
  - Example: T = 60 s gives HP = 60 x DPS x 0.45.
  - Tune against the measured Player kill time, not the bot (L-029). Current bot times of 25 s (Lantern) and over 240 s (Wyrm) show the HP is not tied to one formula.
- Related method: Edwin Fan's HP formula uses a fixed damage window (DP15S), a "suboptimal" factor below 1, and a length multiplier (Game Developer).

### Invulnerability and damage rules
- **Max continuous invulnerable time: 1.5 s** (phase-change beat only). No timed "wait it out" invulnerability.
- **Shields and armor are a damage race, never a wall.** Armor still takes at least 25% chip damage from every weapon, so no hit ever reads as "0".
- A shield breaks by a damage meter and has a timeout of 6 s or less, then vanishes. This copies Diablo 4 Season 10, which replaced invulnerability with a 5 s shield at 1/3 and 2/3 HP that players can break to skip the stall (Icy Veins, via search snippet, unverified).
- Zero-damage zones ("can't take damage in areas") count as bugs: every collider on the boss accepts a hit.

### Hurtbox rules
- Hurtbox at least 1.3x the visual mesh. Add a large soft "aim-assist" capsule around the body for pad players.
- Body hit = 1.0x. Weak point = 2.5-3x. Staggered or open = 2.5x. Hits from any weapon count.
- Mandatory weak-point or window mechanics must also be reachable with every weapon (the Lantern shield must not depend on one pistol shot from one wall panel).

### Phases (60 s fight)
Two HP-gated phases at 100%, 55% and a short desperation beat at 20%, which makes three beats of about 20 s each. Each phase swaps the attack set and gives one 1.5 s stagger as feedback. This matches ULTRAKILL's 2-phase bosses with an even HP split (Gabriel 50+50, Minos Prime 65+65).

| Boss | Rule change |
|---|---|
| LANTERN | Shield: 3 pellet or nail hits OR 1 charged shot breaks it from anywhere. It breaks to a "burn" window of 8 s at 2.5x. Shield-up damage is 25%. |
| STAG | Body hits at 1.0x all the time. Wall-slam stun is 5 s at 2.5x. Punch adds 1.5 s of stagger. |
| KILN WYRM | Body takes 40% when the head is not punched. Punched head: 6 s exposed at 2.5x. |
| LOOM | Joints are weak points at 3x from any weapon. Body takes 50%. The nailgun gets no special exclusivity. |

### Anti-camping without invulnerability (inferred, plus the DOOM/ULTRAKILL patterns in table C)
- Give the boss area denial on a stationary player: homing missiles, a floor zone that grows when you stand still for 2 s, wall-pinning shockwaves.
- Boss relocates every 10-15 s, so most positions stay hittable and no spot stays safe.
- Reward moving: +25% damage while airborne or sliding. Keep the existing Wyrm/Lantern/Loom camp rules, but test them in the Player (T-39).

### Grapple spec (recommended)
- **2 charges, 6 s recharge each (one at a time).** A hard 1.0 s gap between pulls even with charges banked (the current 0.4 s allows chaining).
- Refund rules: a miss costs nothing (Halo: a failed connect triggers no cooldown); +1 charge on a non-boss kill. Landing does NOT refill.
- Range 30 m (inferred: Halo 8 m, Apex 21 m, but SKYGRIND pulls at 44 m/s). Keep the 12 m/s kept on arrival.
- The same anchor ring cannot be re-used within 3 s. Pulling onto a boss body is allowed once per 10 s.
- **Alternative A (Halo / Titanfall style):** 1 charge, 5 s cooldown, kills cut 2 s, and a successful pull into a punch refunds 50%.
- **Alternative B (Apex 2020 style):** distance-scaled cooldown, `cd = clamp(2 + 0.2 x metres pulled, 3, 10)` s. Short hops are cheap and long pulls cost more. This rewards skill and cuts face-plant spam.

## (2) Evidence

### A. Boss numbers
| Game / boss | Numbers | Source |
|---|---|---|
| ULTRAKILL Minos Prime | 130 HP (65+65). 6 / 12 stamina. Parry gives +5 stamina. 2 s stun at 0. No invulnerability documented. | ultrakill.wiki.gg/wiki/Minos_Prime |
| ULTRAKILL Sisyphus Prime | 200 HP (100+100). Stamina 6 / 10. Parry +5. 2 s stun. No i-frames. | ultrakill.wiki.gg/wiki/Sisyphus_Prime |
| ULTRAKILL Gabriel (Judge, Apostate) | 100 HP (50+50). Launched airborne at 50% (free damage window; Apostate takes half damage then). | ultrakill.wiki.gg/wiki/Gabriel |
| ULTRAKILL V2 | 40 HP first fight. 80 HP (50+30) second fight. No invulnerable periods documented. | ultrakill.wiki.gg/wiki/V2 |
| ULTRAKILL kill time | Not found. Whole level P-1 S-rank = 3:33 (Minos Prime plus other content), so this is not a boss time. | ultrakill.wiki.gg/wiki/P-1 |
| DOOM Eternal Marauder | One Steam thread estimates about 3200 HP (Nightmare) from damage maths. A search snippet claimed 5000 (unverified). Doom Eternal pages were 403/402. Eyes flash green before melee. A Super Shotgun or Ballista blast staggers it. The shield deflects early counterattacks. | steamcommunity.com/app/782330/discussions/0/3241971691951002029; slayersclub.bethesda.net/en-NZ/news/doom-eternal-boss-tips |
| DOOM Eternal Marauder design | Director: stagger long enough for one shot with most weapons, two if you swap. "Crystallizing everything about the game." | pcgamer.com (search snippet; article body not retrieved) |
| DOOM Eternal Doom Hunter | Break the regenerating shield (Plasma Rifle), then the sled. Homing missiles force dodging. HP not found. | slayersclub.bethesda.net/en-NZ/news/doom-eternal-boss-tips |
| DOOM Eternal Gladiator | Phase 1 shield; Phase 2 spinning mace. Stagger when the shield bangs. A reckless dash gives a counter opening. Shockwave pins the player against walls. HP not found. | same Slayers Club page |
| DOOM Eternal Icon of Sin | 8 targetable parts in 2 stages (armor, then flesh). A search snippet gave 45000 HP (unverified). | doomwiki.org/wiki/Icon_of_Sin_(Doom_Eternal) (search snippet) |
| Diablo 4 (design example) | Invulnerability phases replaced by a 5 s skippable shield at 1/3 and 2/3 HP. No phase can trigger before 10 s into the fight. | icy-veins.com (snippet, unverified); player criticism: "invuln phases where you can only wait ... boring bad design" (forum quotes in search results) |
| Hades | Boss design from myth-based behaviors (Hydra regenerating heads set the structure). No HP or timing data found. | digipen.edu/showcase/news/eduardo-gorinstein-delivers-challenge-and-fun-hades |
| HP formulas | Boss HP = sum(dp15s x suboptimal factor) x cycles. Another source says designers plan 3-5 min raid fights (MMO context, not FPS). | gamedeveloper.com/design/hitpoint-balancing-with-many-variables; search results |
| Not found | DOOM 2016, Titanfall 2 bosses, Returnal, Halo Infinite boss HP. No target kill-time source found for any FPS. | not searched deeply (time) |

Takeaway: ULTRAKILL bosses carry 100-200 HP against a player doing several damage per hit (inferred 20-30 hits), no timed invulnerability, and gate damage through stamina and parries (the player's action ends the window). That supports the "no waiting phases" rule above.

### B. Anti-camping (from the DOOM Eternal pages above, plus inference)
- Doom Hunter homing missiles "track from nearly any range", so standing still fails.
- Gladiator shockwave pins you between walls and its Phase 2 dash forces dashing.
- Marauder summons a wolf and fires ranged blasts from the axe.
- Minos Prime's stamina and parry loop forces the player into contact (ULTRAKILL wiki).
- All of these keep the boss damageable at most times (inferred from the pages: no invulnerable timers listed).

### C. Grapple and hook limits
| Game | Limits | Source |
|---|---|---|
| DOOM Eternal Meat Hook | Enemies only, never the environment. Fails on shielded Marauder, Riot soldier, and Gladiator shield/spin. Disengages on dash, double jump or knockback. Quick Hook upgrade = -25% recharge. Base seconds not found. | doomwiki.org (search results) |
| Titanfall 2 Grapple | Charge meter. Tricks and Treats patch: "one charge and recharges faster". Search snippet: 16.7 s to 10 s per charge (unverified). | titanfall.wiki.gg/wiki/Tricks_and_Treats |
| Apex Pathfinder | Launch 13 s, then 15 s (Apr 2020), then 35 s (May 2020 nerf), then distance-based 10-35 s (Oct 2020). Respawn considered a "drain energy as it goes" idea like Titanfall 2. Reason: faceplants stop being punished. Range 21 m. Wiki also lists 20 s flat plus 2 charges (June 2026 patch, per the wiki summary, unverified). | apexlegends.wiki.gg/wiki/Pathfinder; dexerto.com and gamespot.com (search snippets) |
| Halo Infinite Grappleshot | Range 8 m (Halopedia). Short cooldown, about 5 s. Quickshot upgrade -40% (about 3 s). No cooldown on a miss, a Kig-Yar stagger, or a finisher. Campaign unlimited, multiplayer 3-5 uses. | halopedia.org/Grappleshot; windowscentral.com (snippet) |
| ULTRAKILL Whiplash | 0.5 s cooldown, but pulling toward enemies (medium/heavy) drains health into "hard damage" at 60/s up to 50. Light enemies get pulled to you instead. Cancel by jump/punch keeps momentum. | ultrakill.wiki.gg/wiki/Whiplash |
| Sekiro | Fixed grapple points only (green circle when in reach). | sekiroshadowsdietwice.wiki.fextralife.com/Grappling+Hook |
| Spider-Man (Insomniac) | No cooldown on swings. Skill comes from physics and timing (zenith boost, late re-swing speed). | gamedeveloper.com (search result) |
| Ghostrunner, Karlson, Zelda Hookshot | Not found or not checked. Zelda is fixed targets only (general knowledge, no source fetched). | n/a |

Spam problems reported: Pathfinder was too dominant at 13-15 s, so devs went to 35 s, then to distance scaling to un-nerf low-skill use. Insight: the three fixes seen are (1) cooldown scaled to distance or power, (2) a cost on the action (ULTRAKILL hard damage), (3) a target restriction (DOOM, Sekiro). Charges plus a short minimum gap is the least punishing option for a movement shooter (inferred).

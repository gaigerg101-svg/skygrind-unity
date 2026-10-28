# R3: Rewards, reticle ability HUD, charged fist (web research, 2026-10-08)

Method: web search plus fetch only. No local code read. Rule used throughout: a number or fact with a URL is sourced;
anything marked "INFERENCE" is my design reasoning, not a finding; "not found" means searched and not located.
Gaps up front: no GDC talk transcript was found for Hades, Dead Cells or Returnal reward cadence (not found). Exact
reticle offsets in px for Apex / Titanfall 2 / Overwatch / Valorant / Destiny 2 / Ghostrunner were not found; no
public source gave them. Sizes in section B are therefore INFERENCE built from the sourced placement facts.

---------------------------------------------------------------------------------------------------------------------

## A. REWARD MOMENTS

### A1. What the references actually do (sourced)
- **CoD Zombies, Mystery Box**: costs 950 points in classic Zombies.
  https://www.sportskeeda.com/esports/what-order-buy-perks-black-ops-cold-war-zombies (perk prices) and
  https://strategywiki.org/wiki/Call_of_Duty:_World_at_War/Mystery_Box (box). Search summary also stated 950.
- **CoD Zombies, perks**: Jugger-Nog is 2500 points (health 100 -> 250). Cold War unifies perk cost by count owned: first
  2500, last 7000. https://www.sportskeeda.com/esports/what-order-buy-perks-black-ops-cold-war-zombies
  - Ratio, computed from those two numbers: a perk is about 2.6x a box spin; the 4th-perk tier is about 7.4x.
- **CoD Zombies, power-ups** (Max Ammo, Insta-Kill, Double Points, Nuke) drop randomly off killed enemies and give
  short-term buffs. In modern titles every power-up is in a cycle; once all have been given a new cycle starts, so the
  spread is even (anti-streak design).
  https://callofduty.fandom.com/wiki/Power-Ups and https://esports.gg/guides/call-of-duty/all-black-ops-6-zombies-power-ups-and-what-they-do
  - Max Ammo: refills all ammo for the player (and friendlies). Same sources.
  - Drop rate per round: not found.
- **Risk of Rain 2 chests**: Small chest base $25 (Common 79.2 %, Uncommon 19.8 %, Legendary 0.99 %); Large chest $50
  (Uncommon 80 %, Legendary 20 %); Legendary chest $400 guaranteed legendary, stage 4+ and extremely rare.
  https://riskofrain2.wiki.gg/wiki/Chest
  - Money costs scale exponentially with the difficulty coefficient, i.e. with time.
    https://riskofrain2.wiki.gg/wiki/Difficulty
  - Large/Legendary price is 2x / 16x the small chest. Computed from the wiki figures.
  - Page is flagged "under construction": stage scaling numbers not given there (not found).
- **Vampire Survivors chests**: dropped by bosses/champions. Common chest = +1 upgrade, epic = +3, legendary = +5;
  Luck raises tier odds. You cannot skip the animation of the 5-upgrade chest but you can skip 1 and 3. Evolution
  chests come from bosses at or after the 10-minute mark in most stages. If everything is maxed the chest pays
  money bags only.
  https://steamcommunity.com/app/1794680/discussions/0/3201493834358639726 and https://vampire-survivors.fandom.com/wiki/Evolution
  - Lesson (INFERENCE): the reward size is told by a visible tier before you open it, and the animation length
    scales with the tier.
- **Returnal**: Obolites are the common currency from kills; each biome has a Fabricator shop of four pods, the
  leftmost always a Silphium vial, the others artifacts/consumables/augments. Challenge rooms give 3 waves then a
  stat boost, a choice of high-powered weapons, silphium and resin. Health pickups at full health become Resin.
  https://www.techradar.com/how-to/returnal-resources-explained and https://www.gamerevolution.com/guides/679811-returnal-tips-guide
  - Notable: overflow converts into a permanent-ish stat instead of being wasted.
- **Doom Eternal**: Sentinel Crystals (12 total) each raise max health, armor or ammo by a fixed step (health +25 up
  to 200, armor +25 up to 150). Weapon points come from challenges, secrets and kills. Argent cells cap fixed at 60.
  https://doomwiki.org/wiki/Sentinel_crystal and https://doomwiki.org/wiki/Weapon_point
  - Praetor suit points come from Night Sentinel spirits or challenges. https://doomwiki.org/wiki/Praetor_suit
  - Resource refill is bound to actions, not menus: chainsaw kill = large ammo drops; glory kills charge Blood
    Punch / Sentinel Hammer. https://doomwiki.org/wiki/Chainsaw_(Doom_Eternal) and https://doomwiki.org/wiki/Glory_kill
- **ULTRAKILL secrets**: Soul Orbs, 5 per level, in out-of-the-way spots; first pickup gives a Secret style bonus
  and level end pays 1,000 P per new orb. Variant orbs are Blood (heal) and Dual Wield power-ups. Cumulative
  counts (10/25/50/100) unlock weapon colors. https://ultrakill.wiki.gg/wiki/Soul_Orb
  - The reward is found by moving through the level, not by opening a menu.
- **Dead Cells**: GDC / postmortem on the scroll-and-forge cadence not found. A Motion Twin designer discussing design
  tricks is referenced, no extractable numbers. https://www.pcgamesinsider.biz/indie-interview/66988/how-motion-twin-stopped-dead-cells-being-just-another-roguelike-pixel-art-metroidvania-game/
- **Hades boons / Darkness**: Darkness is one of three resource types and is spent on permanent stat upgrades (outside
  the run). Boon system was influenced by Dota 2 per Kasavin. GDC-grade cadence data not found.
  https://www.gamebanshee.com/news/121967-hades-interview.html and https://afkgaming.com/dota2/news/6052-hades-dev-shares-how-dota-2-influenced-their-game
- **Pick-1-of-3 critique** (Keith Burgun): the pause-and-pick-3 screen is "gambling dressed up as strategic
  decision-making", and with highly coupled options "the choice is obvious". He recommends putting power into the
  play space (upgrades on the map, gated by environmental challenge) or gradual accumulation of random powers.
  https://keithburgun.net/pick-1-of-3-is-a-missed-game-design-opportunity/
  - Slay the Spire discussion notes three options is standard and the game adds "choose from two" and "choose from
    many" types too. https://buymeacoffee.com/lunajavier/slay-spire-player-control
- Not found: a GDC talk on mid-run reward cadence for FPS/roguelite with figures. I did not invent one.

### A2. Recommended interval
- **Every 5 rounds as the main cadence, with a bigger one at round 10, 20, 30 (INFERENCE).**
  - Basis: CoD Zombies' own pillars sit on a power-up cycle (even spread, see above) and on a box/perk purchase
    economy, while the gambling-screen critique (Burgun) says menus on every beat dilute choice. Rarer = each one
    matters. Vampire Survivors ties its big payoff (5-upgrade chest, evolution) to a boss and a late clock, i.e. one
    per milestone, not per tick.
  - Between milestones keep reward flow *physical*: pickups from kills and secrets (Doom Eternal, ULTRAKILL model),
    no screen.
- Keep rounds 1-4 clean so the first reward at round 5 is a landmark. Round 5 should be small-big (a guaranteed
  spin), round 10 bigger (perk or weapon upgrade).

### A3. Eight reward types ranked for a speed shooter (INFERENCE, sources show the mechanic exists)
Ranking criterion: zero menu time, readable in under 1 s, does not break momentum, scales with the economy.
1. **Free Mystery Box spin, spawned as a physical glowing box on the next lane.** Existing mechanic with a known
   price (950). No menu: walk up, interact. https://strategywiki.org/wiki/Call_of_Duty:_World_at_War/Mystery_Box
2. **Guaranteed weapon drop (named, on a pedestal; no random).** RoR2 Legendary chest model: guaranteed tier.
   https://riskofrain2.wiki.gg/wiki/Chest
3. **Perk granted outright (one of two, or a fixed rotation).** Perk is the biggest ticket in Zombies (2500+).
   https://www.sportskeeda.com/esports/what-order-buy-perks-black-ops-cold-war-zombies
4. **One-time weapon upgrade (Pack-a-Punch style, applied to the held weapon, no screen).** Vampire Survivors "+1/+3/+5"
   tiers show tiers are legible. https://steamcommunity.com/app/1794680/discussions/0/3201493834358639726
5. **Full heal + max-HP step (+25).** Doom Eternal crystals use a fixed step. https://doomwiki.org/wiki/Sentinel_crystal
6. **Points lump sum (1000).** Only works if the points economy is the pacing lever; Returnal and ULTRAKILL both
   convert reward into currency. https://ultrakill.wiki.gg/wiki/Soul_Orb
7. **Max Ammo / full refill power-up.** Short-term, reliable, but tiny emotional size. https://callofduty.fandom.com/wiki/Power-Ups
8. **3-card percentage buff screen (the thing being replaced).** Burgun's critique: low coupling, obvious choice,
   gambling feel. https://keithburgun.net/pick-1-of-3-is-a-missed-game-design-opportunity/

### A4. Presenting a reward without stalling movement
- No pause. Spawn the reward as a **world object** with a pillar of light (Vampire Survivors chest animation shows
  players want a beat, but the skippable 1/3 chests prove they want an out). https://steamcommunity.com/app/1794680/discussions/0/3201493834358639726
- Time boxes (sourced principle): under 0.1 s feels instant, 1 s keeps flow, 10 s is attention limit.
  https://www.nngroup.com/videos/3-response-time-limits-interaction-design/
  - So: the pickup action to effect <= 0.1-0.3 s; any celebration banner <= 1 s; it never blocks input.
- UI animation guidance: 200-300 ms for small elements, up to 400-500 ms for large. https://valhead.com/?p=2978
- Gate the reward in space, not in menus (Burgun's alternative). https://keithburgun.net/pick-1-of-3-is-a-missed-game-design-opportunity/
- If a choice remains, the player chooses by walking: two pedestals, pick one by touching it. (INFERENCE, in line
  with Burgun's "put it in the play space").

### A5. Decision paralysis
- Max 2 options at a milestone, or 1 forced + 1 optional reroll-free swap. Burgun: three highly coupled options
  makes the choice obvious anyway, so extra cards add cost not value.
  https://keithburgun.net/pick-1-of-3-is-a-missed-game-design-opportunity/
- Make options differ in kind (weapon vs perk), not in percent, so the choice is a one-glance identity decision.
- Auto-select a default after a few seconds (INFERENCE); no timer visible if it adds stress.
- Show the choice as an icon + 3-word label; no stat tables.
- Slay the Spire's "choose from two" is a named type in the discussion above. https://buymeacoffee.com/lunajavier/slay-spire-player-control

### A6. Size relative to economy
- Anchors from sources: box 950; Jugger-Nog 2500; Cold War perk ladder 2500-7000; RoR2 chests 25 / 50 / 400.
  https://www.sportskeeda.com/esports/what-order-buy-perks-black-ops-cold-war-zombies and https://riskofrain2.wiki.gg/wiki/Chest
- Computed ratios: perk first tier = 2.6x box; RoR2 legendary chest = 16x small chest = 8x large chest.
- INFERENCE rule: milestone reward worth about **1x the best common purchase at round 5, 2-3x at round 10, a
  "legendary" 6-8x at 20+**. Points lump sum should equal about one box + one perk (roughly 3,000-3,500 at CoD
  numbers), not 100.
- RoR2 shows rewards must scale with time because costs do; a flat 1000 gets worthless. https://riskofrain2.wiki.gg/wiki/Difficulty

---------------------------------------------------------------------------------------------------------------------

## B. ABILITY / COOLDOWN HUD UNDER THE RETICLE

### B1. What real games do (sourced where found)
- **Halo Infinite**: equipment icon sits lower right next to the grenade icon. Cooldown = a band of light fills over
  the icon. Unavailable icon is darkened; end of cooldown = icon becomes bright again, plus a short sound.
  https://support.halowaypoint.com/hc/en-us/articles/26608497528596-Equipment-in-Halo-Infinite
- **Valorant**: ability icons bottom center beside armor/health; ultimate icon at the right of that cluster. Ult
  points charge per kill, death, spike action or orb (1 point each, per the sources).
  https://www.oneesports.gg/valorant/user-interface-valorant-hud/ and https://siege.gg/news/how-to-get-ultimate-points-in-valorant
- **Titanfall 2**: communicates through icons rather than color coding, with cooldown timers and progression bars as
  distinct icons; tactical ability shows its input. Some abilities hold two charges.
  https://caniplaythat.com/2016/12/01/disability-game-review-titanfall-2/ and https://titanfall.wiki.gg/wiki/Tactical_Ability
- **Apex Legends**: HUD shows persistent button prompts for weapons/abilities (can be turned off); damage marker on
  the crosshair (default X). Community asks for slimmer ability bars for teammates. Crosshair color customization
  was promised. https://forums.ea.com/discussions/apex-legends-feedback-en/consider-making-teammates-ultstactical-more-visible/13609655
  and https://upcomer.com/apex-legends-improves-colorblind-support-promises-crosshair-customization
- **ULTRAKILL**: stamina max 3, shown on the HUD; dash = 1, dash jump = 2; recharge 0.7/s on Standard (1.4 Harmless);
  no recharge while sliding; parry restores all. https://ultrakill.wiki.gg/wiki/Stamina
  - Flash-on-full: not documented (not found).
- **Ghostrunner**: dash with upgrades (extra charge, cooldown), four abilities with heavy cooldowns tied to a Focus
  meter. HUD geometry not found. https://www.gameshedge.com/?p=41145 and https://godisageek.com/2020/10/ghostrunner-beginners-tips/
- **Destiny 2**: grenade / melee / class ability each recharge on separate timers (grenade 0:58-3:02 depending on
  stat tier). Radial-vs-bar HUD geometry not found in sources.
  https://destinypedia.com/Grenade and https://www.bungie.net/en/News/Article/48838
- **Overwatch**: HUD geometry for the ability ring not found (search returned only WoW/custom-HUD pages). The
  color options on UI exist but do not cover ability effects.
  https://eu.forums.blizzard.com/en/overwatch/t/colour-blind-accessibility-issues/30890
- **Doom Eternal equipment icons**: placement and fill style not found in sources; equipment launcher + chainsaw
  fuel is the sourced mechanic. https://doomwiki.org/wiki/Chainsaw_(Doom_Eternal)

### B2. Recommended spec (INFERENCE, combining the sourced behaviors above)
Placement:
- Center-bottom of reticle: arc centered on the reticle's vertical axis, **26-48 px below the crosshair center at 1080p**
  (outside the spread ring, inside peripheral focus). No source gives px; this keeps it within ~2 deg of fixation.
  Reasoning: Apex and ULTRAKILL put persistent state at the periphery, but the designer's brief wants under-reticle,
  so keep it small and low-contrast when idle.
- Arc: 90-120 degree curve, **120-160 px wide, 4-6 px thick** at 1080p. Pill alternative: 96-140 x 6 px.
- Pips for charges: 8-10 px diameter, 6 px gap; max 3 (ULTRAKILL has 3 stamina; Titanfall 2 sometimes 2 charges).
  https://ultrakill.wiki.gg/wiki/Stamina and https://titanfall.wiki.gg/wiki/Tactical_Ability

Read-at-a-glance:
- Fill direction left to right, fill grows monotonically (Halo: band of light fills back up).
  https://support.halowaypoint.com/hc/en-us/articles/26608497528596-Equipment-in-Halo-Infinite
- Dim 35-45 % opacity while recharging, 100 % when ready (Halo darkens when unavailable, brightens at ready).
- **Ready flash**: a 100-150 ms white-to-color pulse plus 1.0 -> 1.15 -> 1.0 scale, total <= 250 ms. Basis: NN/g
  says <0.1 s feels instant and 200-300 ms is right for small elements. https://valhead.com/?p=2978 and
  https://www.nngroup.com/videos/3-response-time-limits-interaction-design/
- Idle-hide: arc fades to ~20 % when full and unused for 2 s; reappears on spend. (INFERENCE; keeps the reticle clean.)

Color rules:
- Use the project palette: cyan = usable/ready (player), a desaturated cool grey while charging; red-orange only for
  danger (e.g. cooldown denied flash) never for ready. Palette rules are from the project brief, not from a source.
- Never depend on color alone: Titanfall 2's HUD review credits icon-and-bar communication over color coding.
  https://caniplaythat.com/2016/12/01/disability-game-review-titanfall-2/
- Contrast: graphic UI parts need at least 3:1 against adjacent colors (WCAG 1.4.11). https://www.boia.org/WCAG2/CP/1.4.11
  - Applies to the arc vs. a bright sky or dusk fog; give the arc a 1-2 px dark outline.

Multiple abilities:
- **One cooldown, one arc. Two or three abilities: arc segments** (3 x 40 degree segments with 4 degree gaps) in
  fixed order left to right; **charges of one ability: pips**. Do not mix both on one ability.
- Distinguish by shape/icon glyph (tiny 12-14 px glyph under each segment) so it is not color alone.
- If dash is a stamina-style charge: pips (ULTRAKILL 3-segment precedent).
- Put the ultimate/rare ability off the reticle (Valorant keeps ult at the cluster edge; Halo keeps equipment lower
  right). https://www.oneesports.gg/valorant/user-interface-valorant-hud/

Accessibility:
- Offer a numeric/secs toggle and a size scale slider; Apex's persistent prompts are toggleable.
  https://ea.com/able/resources/apex-legends/pc/features
- Provide a non-visual cue option (project rule: no audio code, so use haptic/controller rumble only; INFERENCE).
- Provide a high-contrast mode (3:1 minimum) and a shape-redundant state (filled vs hollow pips).
- Reduce-motion option disables the scale pulse and keeps a static brighten.
- UI text >= 16 px at 1080p per the project rules (project CLAUDE.md, not a web source).

Motion timings summary (INFERENCE built on sourced bands):
- Ready flash 120 ms in / 130 ms out. Spend: instant drop (<100 ms). Fill: linear, no easing.
- Fade-out of idle arc 300-400 ms; fade-in 100 ms.
- Bands cited: <0.1 s instant, 200-300 ms small elements, 400-500 ms large. https://valhead.com/?p=2978

---------------------------------------------------------------------------------------------------------------------

## C. CHARGED MELEE FIST

### C1. Comparable mechanics (sourced)
- **ULTRAKILL Feedbacker**: default arm; fast punch that can parry; parried attacks deal 5 base damage; consumes
  1 stamina... (Feedbacker exhaustion 1, Knuckleblaster 1.5; exhaustion regenerates 1.25/s).
  https://ultrakill.wiki.gg/wiki/Arms and https://ultrakill.wiki.gg/wiki/Parry
  - **Knuckleblaster**: heavy damage, sends enemies flying, cannot parry; holding the melee key makes a shockwave
    that pushes back enemies and deflects projectiles. https://ultrakill.fandom.com/wiki/Knuckleblaster
  - Parry refills all stamina. https://ultrakill.wiki.gg/wiki/Stamina
  - Takeaway: a *skill-timed* punch gives a resource refill, which is the "charge by action" model.
- **Doomfist Rocket Punch** (Overwatch): hold to charge then release; max charge time 2.5 s; displaces 10-23 m in the
  facing direction and knocks the enemy back; cooldown reported 3-4 s depending on source; after mitigating 100+
  damage with Power Block the cooldown resets and the next punch is empowered; wall hit = up to 150 damage scaled
  by charge. https://icy-veins.com/overwatch/doomfist-abilities-strategy and https://www.invenglobal.com/overwatch/hero/doomfist
  and https://overwatch.blizzard.com/en-us/heroes/Doomfist (official text: "Damage increases if the enemy hits a wall").
  - Takeaway: continuous charge scales distance (10 to 23 m = 2.3x), with a cooldown guard; time-based, not kill-based.
- **Doom Eternal**: Blood Punch / Sentinel Hammer charge from glory kills; chainsaw requires fuel and kills drop ammo.
  https://doomwiki.org/wiki/Glory_kill and https://doomwiki.org/wiki/Chainsaw_(Doom_Eternal)
  - Takeaway: a charge fed by *melee kills* is exactly the designer's idea, shipped in a movement shooter.
- **Metal Gear Rising**: fuel cells fill by attacking with the blade; zandatsu on a weakened enemy refills health and
  fuel. https://destructoid.com/?p=119641 and https://www.engadget.com/2013-02-19-metal-gear-rising-revengeance-review-its-just-a-flesh-wound.html
  - Takeaway: hit-fed meter plus a skill-gated "finisher" that refills in one go.
- **Valorant ult points**: 1 per kill, 1 per death, 1 per spike action, 1 per orb. https://siege.gg/news/how-to-get-ultimate-points-in-valorant
  - Takeaway: discrete points, 'comeback' sources (death) keep losers charged.
- **Ghostrunner**: ability gates use a Focus meter that fills over time and upgrades add charges / speed.
  https://www.godisageek.com/?p=243240 ; Sensory boost upgrade grants a short boost on kills.
  https://www.gameshedge.com/?p=41145
- **Titanfall 2 melee, Hades, Viewfinder, Lethal Company**: no usable sourced detail on a charged melee
  found (not found). Titanfall 2 is covered above only for ability HUD.

### C2. Charge sources and rates (INFERENCE, informed by the pattern above)
- Sources in order of value: kill with the fist (largest), any kill within 1.5 s of a fist hit (chain), fist hit on an
  enemy (small), kill by other weapons (very small, so the gun still feeds the fist), taking no damage streak (none).
  - Doom Eternal feeds from glory kills; Valorant feeds from kills, deaths and objectives. Sources above.
- Starting numbers to test, not facts: fist hit 2 %, fist kill 8 %, other-weapon kill 3 %, multi-kill bonus +50 %.
  "Full charge = about 25-35 kills" matches the designer's "takes a lot of kills".
- Do not charge on time alone. Optional tiny floor 0.2 %/s so a stuck player can still reach tier 1 (INFERENCE).

### C3. Levels: 3 tiers versus continuous
- **3 tiers (recommended)**: readable, tunable, easy to telegraph; matches discrete pips (ULTRAKILL has 3 stamina, Valorant
  ult is discrete points). https://ultrakill.wiki.gg/wiki/Stamina
- Continuous (Doomfist model) gives smooth scaling but is harder to read and to test; Doomfist still caps it at a
  max charge and a cooldown. https://icy-veins.com/overwatch/doomfist-abilities-strategy
- Hybrid: continuous meter with 3 notches (33/66/100), boost scales within the notch only at the end.

### C4. Decay rules
- Options: none (stored), slow decay out of combat, decay on damage taken, reset on use.
- Sourced precedent: MGR fuel is spent when used; ULTRAKILL stamina simply refills; Doomfist's reset needs an action
  (mitigate 100 damage). https://icy-veins.com/overwatch/doomfist-abilities-strategy
- INFERENCE: lose one third of the charge when you take hit damage, none from time, full spend on flick (so
  charging is a risk/reward stake, not a bank). Decay while airborne: none (reward flow).

### C5. Telegraphing
- Weapon model: glow that grows with tier (emissive on knuckles), vent lines lighting (3 vents = 3 tiers); full
  charge = gauntlet shows a halo ring. Pure art direction, no source.
- HUD: reuse the reticle arc from section B as a 3-segment charge arc (consistent grammar). Flash on tier-up (same
  <=250 ms rules from B). Valorant icon 'fills as ult points land' is the precedent for per-point ticks.
  https://siege.gg/news/how-to-get-ultimate-points-in-valorant
- Doom Eternal shows the charge state on the weapon itself via equipment charge; precise visuals not found.

### C6. Boost size relative to a normal sprint/jump
- No sourced number from a comparable game for "flick" boost vs sprint. Closest: Doomfist punch travels 10-23 m,
  which is a deliberate dash-class move with a 3-4 s cooldown. https://icy-veins.com/overwatch/doomfist-abilities-strategy
- Proposed ratio (INFERENCE, to be tuned against the project's own jump distance J and sprint speed S):
  - Tier 1: about **0.5x J** extra horizontal reach or ~+25 % of S for one second.
  - Tier 2: about **1.0x J**.
  - Full: about **2.0-2.5x J** (the Doomfist span 10 to 23 m is a 2.3x ratio; used as a style precedent).
  - Full only reachable by a long kill streak, per the designer.
- Add a flick cooldown of at least a few seconds even at tier 1 (Doomfist 3-4 s; the project's own "long cooldown"
  rule from memory).

### C7. Pitfalls (INFERENCE unless noted)
- **Infinite loops**: kill -> charge -> flick -> reach next enemy -> kill. Fix: charge spent on use, cooldown, and
  kill-to-charge rate low enough that a full charge is a milestone. Cooldown reset conditions (Doomfist) need an
  expensive action. https://icy-veins.com/overwatch/doomfist-abilities-strategy
- **Wall/air-chain**: flick off a wall into another flick. Limit to once per airtime until grounded or wall touched.
- **Charge farming** on weak enemies: diminishing value per enemy type; elites give more.
- **Meter hidden when it matters**: telegraph on weapon and arc (C5).
- **Out-of-bounds launch**: full flick can cross map geometry; clamp using the project's own collider.
- **Opaque rules**: ULTRAKILL's exhaustion numbers (1 / 1.5, regen 1.25/s) are easy to read because they are tiny
  integers; keep tiers on round numbers. https://ultrakill.wiki.gg/wiki/Arms

### C8. Three variant designs
1. **GAUNTLET (3 tiers, spend-all).** Fist hit 2 %, fist kill 8 %, other kill 3 %. Tiers at 33/66/100 %. Flick spends
   the whole meter; boost = tier. Damage taken -33 %. Closest to the brief and to Doom Eternal's kill-fed melee.
   Safest to balance.
2. **KNUCKLEBLASTER-STYLE (hold-to-aim, continuous).** Hold melee to consume the meter into a Doomfist-style launch whose
   distance scales continuously between 0.5x and 2.5x J; max hold 2.5 s (Doomfist's cap). Meter keeps decaying 1 %/s
   out of combat. Best feel, hardest to test.
3. **PARRY-FED (ULTRAKILL / MGR hybrid).** Fist hits charge slowly; a *timed* hit (parry window or a hit on a staggered
   enemy, MGR's zandatsu idea) gives a big jump of charge or an instant tier. Skill-gated, so full charge is earned,
   not farmed. Parry resets full stamina in ULTRAKILL, so there is precedent for a single skill action granting a
   large refill. https://ultrakill.wiki.gg/wiki/Stamina and https://destructoid.com/?p=119641

---------------------------------------------------------------------------------------------------------------------

## Recommendations

### A. Rewards
- Replace the 3-card screen with a **milestone reward every 5 rounds**, upgraded at 10 / 20 / 30. Between milestones,
  rewards are world pickups only.
- Ranked list: free Mystery Box spin, guaranteed named weapon, perk, one-time weapon upgrade, full heal + max-HP step,
  1000-3000 points, Max Ammo, percentage cards last.
- Present as a glowing world object: no pause, interact in under 0.3 s, banner under 1 s, never blocks input.
- Offer at most 2 options, different in kind; default pick if ignored. Size reward at about 1x best common purchase
  (round 5), 2-3x (round 10), 6-8x (round 20+), using Zombies' 950 / 2500 / 7000 ladder as the yardstick.

### B. Reticle HUD
- One thin arc 26-48 px under the crosshair, ~140 x 5 px at 1080p, segments for different abilities, pips (8-10 px) for
  charges, never both on one ability.
- Dim while charging, full cyan at ready with a <=250 ms flash (120 ms in, 130 ms out), fade to ~20 % when idle.
- Add glyph or shape redundancy, 3:1 contrast with dark outline, a reduce-motion option and a numeric toggle.

### C. Fist
- Build variant 1 (GAUNTLET, 3 tiers) first; add the timed-hit bonus from variant 3 as a later layer.
- Charge from fist hits and kills (kills dominate), small gun-kill share, no time-only charge, spend all on flick,
  lose a third on damage.
- Boost ratios to test against jump J: 0.5x / 1.0x / 2.0-2.5x, flick cooldown a few seconds, one air flick until
  grounded; telegraph by gauntlet vents (3) and the same reticle arc.

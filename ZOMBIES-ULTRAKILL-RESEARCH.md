# ZOMBIES-ULTRAKILL-RESEARCH.md: CoD Zombies meets ULTRAKILL, from real-player data

Researched 2026-10-03 for GG: "want this game to be like cod zombies meets ultrakill as the main, but other games should
be inspired... pull from examples with real users and proven data". It builds on `DESIGN-RESEARCH.md` (Oct 2: combat,
horde pacing, retention) and doesn't repeat it.

**How it was researched:**
- Three research agents, about 215 distinct sources in total:
  - CoD Zombies (game scripts, Steam data, Treyarch statements)
  - ULTRAKILL and the movement shooters (wiki data, Steam leaderboards and achievements, developer interviews)
  - zombies-likes, ULTRAKILL-likes and the Steam market (sales estimates, Next Fest surveys, co-op data)
- A last-30-days pass over r/CODZombies, r/Ultrakill, r/boomershooters and r/indiegames, plus web supplements.
- SKYGRIND's own numbers checked against `PortExport/config.json` and the Unity code.

**Tags:**
- **[V]:** I checked it in this repo today.
- **[D]:** measured data: Steam API, achievements, leaderboards, SteamCharts, game scripts.
- **[Dev]:** a developer's own words.
- **[P]:** press or community reports.
- **[E]:** an estimate (Raijin / VG Insights sales models: order of magnitude only).
- **[U]:** unverified (seen only in a search summary).

## 1. Bottom line

**SKYGRIND is already most of "Zombies x ULTRAKILL" under the hood.** What it lacks is the part players can see, and
the Zombies rituals the web game never had.

**Already in [V]:**
- CoD's exact points: 10 per hit; 60 / 100 / 130 per kill (BO3's scripts give 60 / 70 / 100 / 130 [D]).
- Rounds, gates (doors) and wall buys.
- 10 perk machines.
- THE FORGE (Pack-a-Punch, 5000).
- Kill drops.
- ULTRAKILL's exact dash: 3 pips at 0.7/s, the Standard values [D].
- ULTRAKILL's exact freshness constants.
- A style meter whose rank multiplies points.
- A parry.
- Momentum.
- A gentle health curve: linear +8% per round from round 10 [V], so no CoD x1.1 sponges.

**What a player sees today is the browser game, rebuilt exactly.** That is why it "looks like the old stuff":
- **The plan so far:** PORT-PLAN.md rebuilds every web system first and puts the look last (N20).
- **NIGHT9's additions are invisible in a screenshot:** the oracle, determinism, and the arsenal's and momentum's
  numbers.

**What the data says to do next:**
1. **Build the Zombies rituals the web never had.** Cut nothing that works. These are the things players name when
   they say "Zombies":
   - the Mystery Box
   - the power switch before Pack-a-Punch
   - a wonder weapon
   - a recurring special round
   - a ladder of secrets
2. **Turn the ULTRAKILL half up where players feel it:**
   - style should drive survival as well as points (hard damage)
   - freshness should affect style and points, not damage
   - rounds should run on a growing point budget, Cyber Grind style
3. **Aim everything at a Next Fest demo.** The demo should be:
   - one map, played solo
   - a 20-40 minute first run with an ending
   - 6-8 guns, 4-5 enemy kinds and 1 boss
   - leaderboards
4. **Defer the modes the data doesn't support:** Duel vs a bot, Classic Arena, Trials. Online co-op is the biggest
   sales lever, but it belongs to the Early Access (EA) launch, not the demo.

**Section 6 is the proposed night order.** The next prompt is `NIGHT10.md`, rewritten today:
- the ARMORY guns (projectiles)
- the Mystery Box
- the ULTRAKILL alignment, behind flags

## 2. What players love and hate: the evidence

### 2.1 CoD Zombies

**1. The round loop is the product.**
- Vanguard launched with objective-only Zombies: no rounds, no Easter eggs, no wonder weapons.
  - Critics called it "lacking" [P].
  - Treyarch had to rebuild "round-based wave spawning... Door Buys, Wall Buys" [Dev, 2022].
- MWZ was called "DMZ with zombies" [P].
- BO6's return to round-based Zombies was "a return to form" (IGN and GameSpot both 8/10) [P].
- Black Ops III still matters 11 years on [D]:
  - 85.2% positive of 234,502 reviews
  - a 13,630 peak in September 2026
  - a top-40 seller in May 2026
- **In BO7, Zombies is "the only thing saving the game" [P]:**
  - Whole-game Steam score 37.7% positive [D]. Multiplayer and campaign complaints drive that.
  - Reviewers call round-based Zombies "exactly what the community asked for".
- **In the last 30 days, r/CODZombies' biggest posts were about classic round-based Zombies:**
  - a WaW/BO1 zombie-cap hack, 1,994 upvotes
  - Nacht der Untoten, 639
  - a full-size Mystery Box build, 111 upvotes and 49 comments
  - "best BO1 map", 42 comments

**2. The rituals matter more than their completion rates suggest.**
- Fewer than 2% of players saw Easter eggs before Cold War [Dev].
- Main-quest achievements: BO1 Moon 1.6%, Origins 3.4%, BO3 Origins remaster 9.7% [D].
- Mid-size goals reach many more players [D]:
  - Shadows of Evil's rituals, which open Pack-a-Punch: 26.4%
  - upgrade a Der Eisendrache bow (a wonder weapon): 15.6%
- Bow-upgrade guides have passed 5M views each [U].
- BO6's guided Directed Mode was welcomed [P].

**3. The Mystery Box and the economy.**
- Box: 950 points. Teddy-bear relocation odds by pull [P, wiki]:

  | Pull | Teddy chance |
  |---|---|
  | 1-3 | 0% |
  | 4-7 | 15% |
  | 8-12 | 30% |
  | after 12 | 50% |

- Pack-a-Punch: 5000 (BO1-BO3); 5k / 15k / 30k tiers (Cold War, BO6).
- Perk limit: 4 (classic); no limit from Cold War on; BO7's opt-in "Cursed" mode brings the limit of 4 back.
- Power-up drops [P, modding]: one when team points cross a threshold that grows x1.14 per drop, plus 2% per kill,
  at most 4 per round.
- Hellhound rounds: first at R5-7, then every 4-5 rounds; the last dog drops a Max Ammo [P, wiki].
- Exfil: Cold War at R10, then every 5 rounds; BO6 at R11 / 16 / 21 [P].

**4. What players hated [P]:**
- several currencies and crafting chores (salvage, rarity, armour crafting: "tedious")
- real money touching power (BO4 Elixirs, BO3 Mega GobbleGums)
- removing staples (Juggernog in BO4: a petition)
- sprawl and maps built only for 4 players on microphones (TranZit)
- nerfing how people choose to play (BO6's Directed farming nerf, reverted)
- health-sponge late rounds (BO6's mini-bosses)
- recycled maps (BO7 Paradox Junction: about 35k peak against February's 66k)

**5. Custom maps show what PC players value [D]:**
- BO3's Workshop has 5,115 maps.
- Lifetime subscribers for the top maps:

  | Map | Type | Subscribers |
  |---|---|---|
  | Town Reimagined | remake | 1.51M |
  | Rust | challenge arenas, a gun museum, bosses | 1.21M |
  | Leviathan | original: traps, buildables, a solo-able Easter egg | 1.13M |

- Novelty maps (Minecraft, Shrek) sit in the top 12.

**6. Session length [P]:** BO6 added solo pause and save-quit, a series first. Co-op pause was its most frequent
grievance.

### 2.2 ULTRAKILL and the movement shooters

**1. Endless waves plus high-skill movement keep players when the mode is reshuffled and ranked.**
- ULTRAKILL's Cyber Grind has five Steam leaderboards holding **1,642,047 entries**; Standard alone has 792,336 [D].
  That is about 15% of an estimated 5.3M owners [E].
- What the weaker wave modes managed [D]:
  - DOOM Eternal Horde Mode (3 fixed missions, extra lives): finish the first mission 5.8%, beat Horde 2.8%.
  - Turbo Overkill's endless mode: past wave 20, 1.2%.
- **How Cyber Grind works [wiki]:**
  - a full refill every wave
  - a 16x16 arena that reshuffles every wave (38 patterns)
  - a point budget for each wave that grows with the wave (10 + 3 + floor(wave / 3) per wave)
  - enemy classes unlock on set waves
  - a "radiant" tier from wave 25
  - a leaderboard per difficulty
- DUSK's developer on why his endless mode faded: "the difficulty flatlines" [Dev].

**2. Style drives ULTRAKILL's survival and economy.**
- **Hard damage:** 35% of each hit can't be healed until it drains. Its recovery speeds up with rank, and at
  ULTRAKILL rank it clears instantly [wiki].
- **Blood heal:** close hits heal, 10-30 per punch or shot by hit zone [wiki].
- **Parry:** heals in full (up to the hard-damage cap) and refills stamina.
- **Style buys variants:** style points convert 1:1 into P, which buys weapon variants at 7,500-100,000 P.
- Hakita: the style system exists "to get players out of the headspace of 'path of least resistance'" [Dev].
- A meter with no stakes gets ignored: "really easy to ignore it... because of how little effort it took to get S"
  [P].
- Reception and engagement [D]:
  - 97.3% positive of 245,547 reviews
  - all-time peak 71,785 concurrent players on the Fraud update, 25 Feb 2026
  - median playtime 5.4 h against an average of 24.3 h: a long tail who stay

**3. Freshness changes score only, never damage.** ULTRAKILL's values: Fresh x1.5, Used x1.0, Stale x0.5, Dull x0, on
style points [wiki]. Players still argue about it [P]:
- Against: "you're just meant to cycle through every weapon".
- For: "punish the player for overusing a single weapon".
- On scaling damage by freshness: "would make the game feel annoyingly inconsistent and punishing".

**4. id's lesson: depth, not complexity.**
- Eternal's loop: chainsaw for ammo, glory kills for health, flame belch for armor.
- The Marauder demanded "such specific solutions" and was its most hated part [P].
- For The Dark Ages, id cut it down:
  - Marty Stratton: "Some people thought it was too hard. I actually thought it was too complex."
  - "fewer strings on the guitar"
  - "We've streamlined the economy and the currencies."
- The Dark Ages ships 8 difficulty sliders, including the parry window and game speed [Dev, P].

**5. Campaign drop-off is steep after level 1 [D]:**

| Game | First unit | Second unit | Finish |
|---|---|---|---|
| Neon White | 87.1% | 70.6% | 29.0% |
| Severed Steel | 58.0% | 44.4% | 23.8% |
| DUSK | 44.6% | 25.3% | 20.5% |
| DOOM (2016) | 82.5% | n/c | 32.3% |

  So the first minutes decide most players.

**6. Juice numbers [wiki]:**
- Hitstop: a major hitstop of 0.25 s on a parry; the Jackhammer gives 0.2 / 0.5 s by speed.
- Options: screenshake 0-200%, a flash toggle, FOV 45-160, game speed 50-100%.
- Assisted ranks are marked, not hidden.
- Most music tracks switch between a calm half and a combat half.

### 2.3 Hybrids and the market

**1. Pure "CoD Zombies-like" indies have a modest ceiling, and feel decides their reviews.**
- **Sker Ritual ($19.99):**
  - 81-84% positive; peak 5,992; about 225k copies [D, E]
  - top complaints: clunky movement ("stuck on every corner"), netcode, sponges
  - round funnel from achievements: R20 72.1%, R35 16.9%, R50 5.8% [D]
- **Blood Reaver (EA, Apr 2026):** 159 reviews, criticised for one map [P].
- **Milo**, a solo-dev classic-Zombies homage, enters EA on **2026-10-29** [P]. Differentiate on movement and style,
  not nostalgia.
- The closest neon "zombie waves + mobility" rival: **Dead Rush** (EA, Apr 2026), which has 1-4 local co-op [P].
- A "roguelite FPS inspired by Black Ops Zombies and DOOM" drew 174 upvotes on r/boomershooters on 2026-09-13. There
  is appetite for the mix.

**2. ULTRAKILL-likes review 94-98% positive and usually sell 250k-800k [E]:**
- Mullet MadJack: 98% of 9.6k reviews. It added a free Boss Rush, with an ULTRAKILL crossover, after launch.
- Turbo Overkill, Severed Steel and Cultic sit in the same band.
- Score-only arena games are narrow: Hyper Demon about 142k [E].

**3. Co-op is the clearest sales multiplier:**
- VG Insights: co-op is 5% of Steam releases but 46% of copies sold.
- Online co-op roguelite FPS games lead the niche [Dev, D]:
  - Gunfire Reborn: 2.5M PC copies by 2022.
  - Far Far West: 500k+ in its first weekend; 435k wishlists before Next Fest.
  - Roboquest: about 733-881k [E].
- Only 45% of Roboquest players ever joined co-op [D], so solo must stand on its own.
- Offline-only co-op is about 2.5% of releases and falling, so split-screen alone has little market value.

**4. Horde games collapse on thin content and monetization [D, P]:**
- Killing Floor 3: 44-49% positive, peak down 75% in month two (battle pass and cash shop before the basics).
- Toxic Commando: 8 missions, "done in a single day", down 74% month on month.
- Zombie Army 4: base weapons locked behind DLC.
- Back 4 Blood: $59.99, and support ended after about 15 months.

**5. Next Fest gains track the wishlists a game brings in [Survey]:**
- February 2026 survey: median gain 806 wishlists.
- Pre-fest wishlists predict gains with a Spearman correlation of 0.825:

  | Pre-fest wishlists | Median gained |
  |---|---|
  | under 1k | 322 |
  | 10k-100k | 5,215 |

- Demos released a month or more before the fest did much better.
- Median demo playtime by success tier: Bronze 7 min, Gold 38, Diamond 65.
- Co-op demos convert players to wishlists worst.
- Valve: demo live 10 days early, with a feedback link on its main screen.

**6. The EA launch is the biggest revenue moment [GDCo]:**
- Only 20% of games earned more in their 1.0 month than in their EA launch month.
- ULTRAKILL, Roboquest, Gunfire Reborn, Hades and Far Far West all launched in EA.

## 3. SKYGRIND against the two pillars

**Already there:**

| Pillar | In SKYGRIND [V] | Source |
|---|---|---|
| Round loop, doors, wall buys | ✓ (5 zones, 4 wall guns since NIGHT10: THUNDERCLAP, STITCHER, COMET, LONGWAVE; 16 in the web). NIGHT11: the run's shape (see the missing table below, now mostly in) | web port |
| CoD points (10 / 60 / 100 / 130) | ✓ (no 70-point neck kill) | web port |
| Perks | ✓ 10 of the web's 15; no limit, each costs +10% per perk owned (`perks.stackTax`) | web port |
| Pack-a-Punch | ✓ THE FORGE, 5000, one tier | web port |
| Power-ups | ✓ 5 kinds, 1.5% per kill with a pity ramp (no threshold rule) | web port |
| Roguelite drafts | ✓ MODS (19 in the web) | web port |
| Style meter → points, regen, stamina | ✓ | web port |
| ULTRAKILL dash / freshness constants | ✓ exact | web port |
| Parry (+ perfect parry) | ✓ | web + NIGHT8 |
| Momentum: speed is damage, impact waves | ✓ | web port (NIGHT9) |
| A gentle late-round health curve | ✓ linear +8% per round from R10 | web config |
| No ammo | ✓ (ULTRAKILL-like) | web config |
| Enemy variety | husk, swarmer, spitter (7 kinds and 5 bosses in the web) | web port |

**Missing:**

| Pillar | Status | Source |
|---|---|---|
| Mystery Box | **in (NIGHT10, design.box ON):** 950, the CoD teddy ladder, refund and move, 3 spots, the pool is every ported gun (5 so far) | new design |
| Power switch before Pack-a-Punch | **in (NIGHT11, design.power ON):** a free hold-E switch on HEIGHTS' west skybridge, two gates deep (collider casts); it wakes the web's traps and THE PROTOCOL's relays; THE FORGE stays open from round 1 (the web; BO6's Liberty Falls) | new design (NIGHT11-RESEARCH A.1, C.3) |
| Traps | **in (NIGHT11, ported):** the web's four (SHOCK PLATE, CRUSHER, GRAVITY FAN, ARC PYLON), TRAPPED | web port |
| Wonder weapon | **missing** | new design |
| Recurring special round with a reward | **in (NIGHT11):** the web's objective rounds ported (HOLD 6, 14, 22 ...; GOLD RUSH 7, 11, 19 ...); the swarm rounds (8, 13, 18 ...) open with a banner and their last kill drops a power-up (design.zombies.swarmDrop ON: CoD's Max Ammo round) | web port + new design |
| Secrets / Easter eggs | **in (NIGHT11, ported):** THE PROTOCOL, the guided quest (four relays on the movement showpieces, the SURGE, every gun forged free, +2500) and its hidden fifth relay; the finishes it unlocks are not ported | web port |
| Exfil (a run ending) | **in (NIGHT11, design.exfil ON):** BO6's schedule (a radio at the spawn before rounds 11, 16, 21 ...), 90 s to the site, 35 kills (+10 per later exfil) in 85 s; it ends the run either way; RESULTS says EXFIL with the run's grade | new design (NIGHT11-RESEARCH A.3, C.2) |
| The run's grade and best moments | **in (NIGHT11, ported):** runMoments.js on RESULTS (S..D, the moments, the score line) | web port |
| Hard damage / blood heal | **in the LAB only (NIGHT11, lab.ultraHealth, off; D-18 stays GG's):** ULTRAKILL's 35% hard damage with its 1 s + damage / 20 s timer and rank-scaled drain, heals capped at max - hard, a blood heal per hit within 4 m, the parry to the cap | GG's call |
| Freshness on style / points only | **in (NIGHT10, design.ultra ON, D-17):** FRESH pays points and style, not damage; style pauses in breaks; a parry freezes 0.25 s and refills the dash (the web already refilled it) | GG's call |
| Round point budget and composition | partial (the director plans tiers; no visible budget) | design |
| Leaderboards per difficulty | **missing** (Steam U6) | engineering |
| Solo pause and save-quit | **missing** | engineering |
| Co-op | first slice of `Player[]` (NIGHT9); no second player in a run | U4 |
| A look that reads as new | **the web's look** (N20) | engineering + GG |

## 4. Ranked recommendations

Impact: H / M / L. Effort in Unity: S (an item), M (most of a night), L (several nights).

| # | Recommendation | Evidence | Imp / Eff | When |
|---|---|---|---|---|
| 1 | **The Mystery Box:** 950 points, CoD's teddy ladder (0 / 15 / 30 / 50%), the pool is every ported gun plus, later, the wonder weapon at low odds; it relocates between box spots; seeded per run (Mulberry32) | identity; r/CODZombies' most-engaged prop; Rust's gambler mechanic (1.21M subscribers) | H / S-M | N10 |
| 2 | **Port the ARMORY guns now** (STITCHER, COMET, LONGWAVE: projectiles): the box and the wall need a pool | demo target of 6-8 guns; the web has 18 designed | H / M | N10 |
| 3 | **ULTRAKILL alignment behind design flags:** freshness on style and points only; style decay pauses between rounds and when no enemy is near; a parry refills the dash pips; ULTRAKILL's parry hitstop (0.25 s) | ULTRAKILL's rules [wiki]; players on freshness and damage [P]; Hakita's "path of least resistance" | H / S | N10 (flags; GG decides the defaults, D-17) |
| 4 | **Hard damage and a close-kill blood heal**, rank-scaled drain (ULTRAKILL's 35%) | ULTRAKILL; DOOM's push-forward health [Dev] | H / S-M | N11 (GG's call, D-18) |
| 5 | **The run's shape:** a power switch that wakes THE FORGE (in zones 3-4, reachable by about R5-8); a recurring special round (e.g. swarmers at R5-7, then every 4-5) ending in a guaranteed drop; optional exfil at R11, then every 5 rounds | Shadows of Evil rituals 26.4% [D]; hellhound pacing; BO6 exfil; demo needs an ending | H / M | N11 |
| 6 | **A round point budget with composition pricing** (Cyber Grind): the budget grows each round, a surcharge per repeat of one kind, classes unlock on set rounds; no extra-life padding | Cyber Grind 1.64M entries vs Horde 2.8% [D]; DUSK's flatline | H / M | N11 (in the director, behind a flag) |
| 7 | **Leaderboards per difficulty, and achievements at R5 / 10 / 20 / 30** (Steam), in the demo | Cyber Grind boards; Sker Ritual's round funnel; Neon White hides boards until a first clear [Dev] | H / M | N16 (Steam night) |
| 8 | **A wonder weapon** from the box and a short build quest, upgradeable at THE FORGE; **a ladder of secrets** (hidden collectibles, a "song" egg) | bow upgrade 15.6% vs main egg ≤9.7% [D]; under 2% saw eggs [Dev] | H / M | N14 |
| 9 | **One currency (points).** THE FORGE tiers 5k / 15k / 30k; no salvage, rarity or crafting; no paid power ever | CoD's most consistent negatives [P]; KF3, ZA4 | H / S | standing rule |
| 10 | **Solo pause and save-quit** | BO6's first; co-op pause the top grievance | M / S | N11 |
| 11 | **A first-impression pass** (part of N20 pulled forward): the grade, sky, muzzle flash and tracers, gore and dissolve, decals | Sker Ritual's feel complaints; "looks like the old stuff" (GG) | H / M-L | N15 |
| 12 | **Online co-op (2-4) for the EA launch**; split-screen as the engineering step | 46% of copies from 5% of releases; co-op demos convert worst | H / L | after the demo (D-02) |
| 13 | **2-3 distinct maps by EA** | "Maps" is the genre's most-cited content complaint [P] | H / L | after the demo (GG) |
| 14 | **A difficulty ladder** (ULTRAKILL's 5 levels: enemy speed / HP, stamina regen 0.7 / 1.05 / 1.4) **and assist sliders** (parry window, game speed) | ULTRAKILL; Dark Ages sliders | M / S | N16 |

**Defer (the data doesn't support them before the demo):**
- **DUEL vs RIVAL:** no data.
- **CLASSIC ARENA and TRIALS:** Neon White's audience is level-based, not horde.
- **BOSS RUSH:** better as a free post-launch beat, as Mullet MadJack did.

**DAILY can ride on the seeded sim cheaply** once Steam lands.

## 5. Pitfalls (with the evidence)

1. **Replacing the round loop instead of adding to it** (Vanguard, MWZ).
2. **Systems pile-up.**
   - SKYGRIND already stacks style, freshness, momentum, swap combos, statuses, perks, THE FORGE and MODS.
   - Add the Zombies rituals without new currencies; teach one system at a time (BO6 "FAR TOO MUCH", id's "too
     complex").
3. **Freshness on damage** reads as inconsistent (ULTRAKILL players). With 2-3 weapon slots it bites harder.
4. **Hard-counter enemies** (the Marauder). Escalate by composition, not count or HP.
5. **Hitstop in co-op** freezes your partner too. Decide local vs shared before U4-02.
6. **Leaderboards shown before a first clear** demotivate (Neon White).
7. **Monetization before basics** (KF3), and **thin content at launch** (Toxic Commando, Blood Reaver).
8. **Self-sourced full heals get exploited** (ULTRAKILL nerfed the self-parry). Check the perfect parry.

## 6. The proposed night order (Survival first, to a Next Fest demo)

This replaces PORT-PLAN.md section 3's order **if GG agrees (D-19)**. Every web module still ports line for line
against the oracle; only the order and the additions change.

> **2026-10-03 (afternoon): D-19 decided on GG's delegation, and N11 revised by `NIGHT11-RESEARCH.md`.** The web game
> already has a run's shape (THE PROTOCOL, objective rounds, traps, the grade), so N11 ports those first, then adds the
> power switch (waking the relays and traps), the swarm round's drop and exfil; hard damage + blood heal go in the LAB
> (D-18 stays GG's); the round budget moves to N13 and save-and-quit to N12 or N16.

| Night | Theme | Main items |
|---|---|---|
| N10 | ARMORY guns + the box | projectiles; STITCHER, COMET, LONGWAVE (wall buys, homing); **the Mystery Box**; **ULTRAKILL alignment flags** |
| N11 | The run's shape | power switch → THE FORGE; recurring special round; optional exfil R11 +5; round budget and composition; hard damage and blood heal (D-18); solo pause and save-quit; the first POWER guns |
| N12 | POWER guns | the rest of guns.js; the hotbar and wheel; THE FORGE tiers 5k / 15k / 30k |
| N13 | The roster | charger, drone, lancer, warden; affixes; squads and flankers |
| N14 | Boss + wonder | BULWARK (and MONOLITH); a wonder weapon; the secrets ladder |
| N15 | First impression | the look pass pulled forward from N20; FX (decals, gore, dissolve); impactFx |
| N16 | Steam + demo | Steamworks.NET, leaderboards per difficulty, achievements, difficulty ladder and assists, onboarding prelude, a demo build (target: early December 2026) |
| N17+ | Melee, perks, mods, traps, ARCLINE; co-op (D-02); then the deferred modes | |

**Store page:** as soon as GG can (D-06). Wishlists before the fest decide the fest.

## 7. Decisions for GG

All of them are in HANDOFF.md section 5 too.

| ID | Question | Recommendation | Default if no answer |
|---|---|---|---|
| D-17 | ULTRAKILL alignment defaults: freshness on style and points only; style decay pauses between rounds; a parry refills the dash; parry hitstop 0.25 s | ON in Survival (your Oct 3 direction); the golden tests run with the flags off | ON |
| D-18 | Hard damage (35%, rank-scaled drain) and a close-kill blood heal, with regen kept slower | yes, behind a flag (reverses the web's 3.0 call) | off until you say |
| D-19 | The night order above (Survival first, demo-driven) instead of PORT-PLAN's parity order | adopt | NIGHT10 follows it; N11+ wait for your answer |
| D-20 | New Zombies design the web never had (Mystery Box, power switch, special round, exfil, wonder weapon, secrets) | yes; their numbers go in `Design/unity-config.json` with CoD's as the reference | the box ships in N10 behind a flag ON |
| D-21 | Defer DUEL, CLASSIC ARENA, TRIALS and BOSS RUSH until after the demo | yes | deferred |
| D-22 | A second and third map before EA | yes (GG's map design or a remake of a web arena) | none |

## 8. Key sources

The research reports had about 215 in total.

**CoD Zombies:**
- BO3 game scripts: bo3explorer.zeroy.com (`zombie_utility.gsc`, `_zm.gsc`).
- Steam review API and achievements for the CoD apps (311210, 212910, 42700, 1985810), the BO3 Workshop API.
- Treyarch: treyarch.com game-intel 2021 and 2022; charlieintel.com (Easter egg accessibility); gamesbeat.com (BO7).
- Press: GameRant and Dexerto on Vanguard / BO6 / BO7; game8.co; callofduty.fandom.com (Mystery Box, Power-Ups,
  Hellhound).

**ULTRAKILL and movement shooters:**
- ultrakill.wiki.gg (Style, Health, Parrying, The_Cyber_Grind, Movement, Difficulties).
- steamcommunity.com/stats/1229490/leaderboards.
- Into the Blue Sky interviews with Hakita (2020, 2021); 80.lv.
- Game Developer (DOOM Eternal combat Q&A); GamingBolt and Vice (Dark Ages).
- Steam achievements for Neon White, DUSK, Turbo Overkill, Severed Steel, DOOM, Roboquest.

**Market:**
- howtomarketagame.com (February 2026 Next Fest analysis, benchmarks).
- newsletter.gamediscover.co (wishlist conversion).
- gameworldobserver.com (the EA launch month).
- alineaanalytics.substack.com (co-op revenue); gamedevreports (VG Insights co-op share).
- steamcharts.com; raijin.gg [E].
- Steam pages and reviews: Sker Ritual, Killing Floor 3, Toxic Commando.
- partner.steamgames.com (Next Fest tips).

**Last 30 days:** `~/Documents/Last30Days/cod-zombies-and-ultrakill-what-players-love-and-hate-raw-v3.md` (Reddit
threads and web supplements).

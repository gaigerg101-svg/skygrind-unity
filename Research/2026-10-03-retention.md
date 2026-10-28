# research-retention.md: what keeps players coming back, and what SKYGRIND should build for it

Researched 2026-10-03 for GG. The question: what is at the core of the best-selling and most-played games, and which
features keep players coming back? This pass covers **retention and long-term engagement** only. Moment-to-moment feel
is another agent's, and so is Unity tech.

**Builds on (not repeated here):** `DESIGN-RESEARCH.md` (rows 14-17: a variety-only meta, the daily seed, the heat
sheet, shareable results), `ZOMBIES-ULTRAKILL-RESEARCH.md` (section 4: leaderboards, achievements, secrets,
monetization) and `NIGHT11-RESEARCH.md`. What this pass adds:
- achievement funnels from Steam's own global stats pages
- the 2024-2026 platform data (Steam Replay, Steam Best of 2025, GameAnalytics' PC benchmarks)
- the live-service cadence record: Helldivers 2, DRG, PoE, MH Wilds, Destiny 2
- a ranked list of retention features for the demo and Early Access

**Where SKYGRIND starts:**
- **Ported to Unity:** records (personal bests), the grade S..D, runMoments' score line and the Mulberry32 per-run seed.
- **Earnable in Unity:** only REACH ROUND 10.
- **The web has, not ported** (read in `skygrind/src/modes`):
  - `challenges.js`: 24 badges, 132 lines
  - `unlocks.js`: 6 flags, "variety, never stats"
  - `daily.js`: one counted try a day, a twist, a streak
  - `seeds.js`: seed codes
  - `mutators.js`: 4 mutators, unlocked by best round
  - `finishes.js`: gun recolours that unlock with trial medals
  - `ghosts.js` / `ghostCodec.js`
- **Planned:** `tasks.json` U6-01 (Steamworks.NET, leaderboards, achievements as contracts, cloud, Timeline).
- Google Drive has no SKYGRIND material (searched today), so the repos and the web are the sources.

**Tags:**
- **[W]:** a page I fetched (URL in the sources).
- **[S]:** a search-result snippet only, not opened.
- **[U]:** my inference or estimate.
- **[R]:** this repo or the web repo, read today.

**Effort** uses GG's scale for this pass: S = a few hours, M = a day, L = several days. This is not the night-sized
scale of the earlier research docs.

---

## 0. Bottom line

1. **Most playtime goes to old games that people keep coming back to, not to new ones.**
   - Only 14% of 2025's Steam playtime went to 2025 releases. 40% went to games 8 or more years old [W: GamingOnLinux /
     Steam Replay]. In 2024 the split was 15% and 37% [S].
   - In 2025 the median Steam user played just 4 different games [S: TheGamer / Steam Replay].
   - A new game wins by joining a player's short rotation and staying there.
2. **The Steam charts are social or replayable.**
   - Every game in Steam's 2025 Top Sellers Platinum tier (top 12 by revenue) has multiplayer or co-op [W: FullCleared;
     [U] for the count].
   - The Most Played Platinum tier includes **Left 4 Dead 2** (2009), **Peak** and **R.E.P.O.** [W].
   - A 16-year-old co-op horde shooter is still in Steam's top 12 by unique players. That is the best evidence that
     SKYGRIND's genre keeps players for years.
3. **The bar to clear on PC** [W: GameAnalytics 2026 via GameDevReports]:
   - D1 retention: 15-16% puts a game in the top quarter.
   - D30 retention: about 2.5% for the top 10%, about 10% for the top 1%.
   - Session length: median 18 min, top quarter over 30 min.
   - SKYGRIND's 20-30 minute run to exfil already fits a top-quarter session [U].
4. **Steam's own global achievement stats show the same retention funnel in every great roguelite (section 2):**
   - Over 90% of players reach a first milestone within minutes.
   - 47-73% get a first win.
   - About a third or more climb at least one difficulty rung.
   - 1-6% finish everything.
   - The ladder rungs keep 12-44% of buyers playing. They are the cheapest retention content there is.
5. **The recurring pillars, ranked by evidence and fit for SKYGRIND:**
   1. Runs that always pay something.
   2. Difficulty ladders that pay.
   3. Unlocks that add variety, shown as locks.
   4. Mastery tracks (CoD's camos).
   5. Shared-seed competition (daily or weekly, friends first).
   6. A ladder of secrets.
   7. Onboarding.
   8. Grade chasing.
   9. Clips and sharing.
   10. Build discovery.
   11. A content cadence without FOMO.
   12. Co-op.
   The most important finding about monetization is to avoid FOMO and pay to win.
6. **Content droughts kill games, and updates bring players back.**
   - Monster Hunter Wilds lost about 98% of its 1.38M Steam peak, blamed on "a lack of meaningful content" at endgame [S].
   - Destiny 2 fell 80-90% after Edge of Fate [S].
   - Helldivers 2's Escalation of Freedom update doubled its players (30k to 62.8k) in a day [S].
   - A Path of Exile league start goes from about 3k to 200k+ players a day [S].
   - For a solo developer, a **predictable** cadence of free updates beats a large one [U].
7. **SKYGRIND's biggest retention advantages:**
   - The web already designed most of the meta. Porting `challenges.js`, `unlocks.js`, `daily.js`, `mutators.js` and
     `finishes.js` is cheap.
   - The bit-exact deterministic sim makes daily seeds, verifiable leaderboards and ghosts nearly free. Few competitors
     can claim that [U].

---

## 1. The data: what sells and what gets played

| Dataset | Finding | Tag |
|---|---|---|
| Steam Best of 2025, **Top Sellers** Platinum (revenue, 1-12) | R.E.P.O., Call of Duty, Marvel Rivals, Battlefield 6, ARC Raiders, PUBG, Apex Legends, Schedule I, Borderlands 4, CS2, Dota 2, Monster Hunter Wilds | [W] FullCleared |
| Top Sellers Gold (13-24) | BG3, Expedition 33, Oblivion Remastered, KCD2, Split Fiction, FC 26, Cyberpunk 2077, Dead by Daylight, Nightreign, **Warframe**, RDR2, **Helldivers 2** | [W] |
| **Most Played** Platinum (unique players) | PUBG, **Peak**, GTA V Legacy, Marvel Rivals, Stardew Valley, Dota 2, Delta Force, Terraria, CS2, **R.E.P.O.**, **Left 4 Dead 2**, Apex | [W] |
| Top New Releases Platinum | ARC Raiders, Dune: Awakening, FC 26, BF6, MH Wilds, Civ VII, Nightreign, Oblivion R., Schedule I, Silksong, KCD2, Borderlands 4. TheGamer counts 9 of 12 as multiplayer | [W] TheGamer, FullCleared |
| Circana US 2025 (units + dollars) | 1. BF6, 2. NBA 2K26, 3. Borderlands 4, 4. MH Wilds, 5. BO7 ... 10. BO6, 13. Minecraft | [S] |
| Steam Replay 2025 | 14% of playtime on 2025 releases, 44% on games 1-7 years old, 40% on games 8+ years old | [W] |
| Steam Replay 2024 | 15% on new, 47% on 1-7 years old, 37% on 8+ (9% on new in 2023) | [S] |
| GameAnalytics 2026, 3,582 PC games | D1: top 25% 15-16%, top 1% 50-60%. D7: top 10% ~10%, top 1% >20%. D30: top 10% ~2.5%, top 1% ~10%. Session: median 18 min, top 25% >30 min, top 1% >100 min. Daily playtime median 32-33 min. DAU/MAU median 4-5%, top 10% 13-15% | [W] |
| GameDiscoverCo genre playtime | Roguelite and roguelike deckbuilder both have medians over 5.4 h. Slay the Spire 1: 26 h median, 74 h average over 10M players. Twin-stick shooter 2.2 h, metroidvania 3.5 h | [W] |
| Completion study, 1,616 Steam games | The median game is finished by 10% of players (mean 14%); about 2% of games are finished by over half | [S] |
| Friendslop 2025 | R.E.P.O. about 19M downloads and $152M; PEAK about 16.5M downloads and $98M; priced $8-20 | [S] AppMagic via search |
| BO6 Zombies | Over 480M hours played by early 2025 | [S] |
| Sales of SKYGRIND's references | Balatro 5M (Sep 2024) [S]; Megabonk 1M in 2 weeks (Unity, solo) [S]; Halls of Torment 1M (Oct 2024) [S]; Lethal Company est. 10M+ [S]; ULTRAKILL est. 2.5M-9.5M (the trackers disagree) [S] | |

**The theory behind the pillars:**
- **Self-determination theory.** Ryan, Rigby and Przybylski (2006) found that autonomy, competence and relatedness
  each predict game enjoyment independently [S].
- **Quantic Foundry's 12 motivations** (about 1.25M+ respondents) come in pairs: Destruction / Excitement, Competition /
  Community, Challenge / Strategy, Completion / Power, Fantasy / Story, Design / Discovery [S].
- **Where SKYGRIND sits** [U]:
  - **Strong:** Destruction / Excitement (hordes), Challenge (movement) and Power (the forge, perks).
  - **Weak:** Completion (nothing to collect), Competition / Community (no boards, no co-op) and Discovery (one hidden
    relay).
  - The retention work below fills those three gaps.

---

## 2. The funnel data: Steam global achievements (what share of owners reach each step)

| Game | First minutes | First "win" | Ladder rung 1 | Higher rung | Top tier / 100% | Source |
|---|---|---|---|---|---|---|
| **Vampire Survivors** (243 achievements) | Survive 1 min **97.8%**; Lv 5 97.2%; 10 min 93.5% | Survive 20 min 84.0% | - | - | Adventure complete 1.8%; lowest 1.7% | [W] |
| **Hades** | Clear Tartarus **81.8%** | First escape **46.7%** | (Heat has no achievement) | All keepsakes 17.4% | All companions 6.7%; max keepsakes 5.9% | [W] |
| **Balatro** | Reach Ante 4 **89.9%** | Win a run **71.6%** | Red Stake **43.8%** | Gold Stake **12.2%** | Collection 4.3%; Gold stickers on all jokers 0.8% | [W] |
| **Brotato** (179) | - | Win on Danger 0 **72.9%** | Danger 1 **61.0%**, 2 54.7%, 3 48.7% | Danger 4 42.3%, **Danger 5 36.5%** | Lowest 2.7% | [W] |
| **Risk of Rain 2** | First teleporter **86.8%** | Beat the game **52.2%** | - | Beat on Monsoon **23.7%**; masteries 10-17% | Lowest 1.5% | [W] |
| **Deep Rock Galactic** | First mission **92.2%** | First assignment 45.0% | Hazard 4 **34.2%** | Hazard 5 **11.7%**; Elite Deep Dive 7.5% | All four dwarves gold 1.9% | [W] |
| **Dead Cells** | First boss **72.3%** | Final boss **40.3%** | (Boss Cells not shown) | - | Lowest 0.8% | [W] |

**What the funnels say:**
1. **The first step is near-universal when the payoff is instant.** Vampire Survivors' first achievements land at 1 minute
   and at level 5, and each one is named after the item it unlocks ("Hollow Heart", "Wings", "Crown") [W]. In VS the
   achievement list is the unlock list [U from the names]. Over 97% of owners take that step.
2. **The first win lands between 47% and 73%.** Players who never reach a win churn [U]. A demo has to deliver a win, or
   an end state that feels like one, within its median 18 minutes (the Next Fest median in `DESIGN-RESEARCH.md`).
3. **Difficulty rungs hold a large share of buyers.**
   - Brotato keeps half its first-win players (36.5 / 72.9) all the way to its top Danger level.
   - About 61% of Balatro's winners clear Red Stake (43.8 / 71.6).
   - In DRG, 34.2% clear Hazard 4.
   - A rung re-uses every asset, so it is the cheapest retention per hour of development [U].
4. **The long tail is small but real.** 1-6% go for 100%. Those are the streamers, the guide writers and the
   leaderboard regulars [U].
5. **ULTRAKILL has no Steam achievements at all** [S: Steam discussions, Steam Hunters]. Its P-ranks, Prime Sanctums
   and Cyber Grind board are all in-game. SKYGRIND can keep both: Steam achievements mirroring in-game contracts
   (U6-01's plan) [U].

---

## 3. The pillars, one by one

Each pillar gives the evidence, what players love and hate, and the SKYGRIND application: what it is, how it fits
Zombies x ULTRAKILL, what it builds on, the effort and the risk.

### P1. A run that always pays: "every run counts" and the one-more-run loop

**Evidence:**
- **Hades.** Kasavin's design goal: "take the pain out of dying and having to restart"; "the moment of death isn't
  about rage-quitting" [W: Game Developer].
  - Dialogue reacts to how a run went, and relationships deepen across runs [W].
  - The team's guiding phrase was "every run counts" [S].
- **Vampire Survivors.** A stage lasts 30 min and the Reaper ends it. A meaningful run takes 15-25 min, and the gold
  earned buys permanent unlocks [S].
- **Risk of Rain 2.** The difficulty timer means "the player never really feels safe," which "can keep you playing for
  hours at a time" (Hopoo) [S].
- **Session benchmark:** median 18 min; the top quarter is over 30 min [W: GameAnalytics].

**Love and hate:**
- Players love a natural stopping point plus a reason to start again.
- They hate dead runs that pay nothing, long runs with no suspend (Returnal: `DESIGN-RESEARCH.md` row 12), and
  restart friction.

**SKYGRIND application: the PAYOFF strip** on DOWN and RESULTS.
- **What it shows:**
  - the grade
  - personal-best deltas
  - progress bars on the 2-3 nearest contracts (P3)
  - XP into each gun's mastery track (P4)
  - "NEXT UNLOCK: ..." with its lock icon
- **The run length:** exfil at R11 is the run's natural end. Target 20-30 minutes for a first exfil [U].
- **Fit:** CoD's end-of-game XP tally plus the ULTRAKILL rank screen.
- **Builds on:** RESULTS (grade, moments, score line), records.js and the restart on R.
- **Effort:** M, once contracts exist; S for PB deltas alone.
- **Risk:** low. Keep it to one screen, and show 3 bars at most (Hodent's 3-item limit, P7).

### P2. Difficulty ladders that pay (Hades Heat, Balatro stakes, DRG hazard, Brotato Danger)

**Evidence:**
- **The funnels in section 2:** Balatro win 71.6% then Red 43.8% then Gold 12.2%; Brotato D0 72.9% then D5 36.5%; DRG
  H4 34.2% then H5 11.7%; RoR2 Monsoon 23.7%.
- **Helldivers 2.** Super Helldive was one of the headline additions of Escalation of Freedom, the update that doubled
  the player count [S].
- **Destiny 2 shows the other side.** Edge of Fate's changes were called "punishing" and "ridiculously hard," and the
  player count fell 80%+ [S]. A ladder must be opt-in; raising the base difficulty is not a ladder.

**Love and hate:**
- Players love choosing their own pain and owning a badge that proves it.
- They hate stat-sponge rungs (more HP only) and rewards that don't change.

**SKYGRIND application: a HEAT ladder of 6-8 rungs.**
- **Unlocking:** a win (EXFIL) on rung N unlocks N+1.
- **Each rung adds one readable rule.** The web's mutators are a start: GLASS CANNON, SPEED DEMON, NO REGEN. Add later:
  denser composition, faster exfil timers, fewer box pulls. LOW GRAVITY stays a fun toggle, not a rung.
- **Each rung multiplies score and shows on the grade badge.**
- **Boards:** one per rung, or per group of rungs, to keep a small player base from splitting [U].
- **Fit:** CoD's Directed versus Standard modes plus ULTRAKILL's five difficulty levels.
- **Builds on:** `mutators.js` (125 lines, 4 mutators unlocked by best round) [R], the LAB's flags, the exfil win state
  and the grade.
- **Effort:** M for the ladder and stamping; L with new rung rules.
- **Risk:**
  - Rungs that only raise HP. The prior research's rule holds: escalate by composition.
  - The new numbers belong in `Design/unity-config.json`.
  - Enemy kinds are GG's design, so rungs reuse the existing kinds.

### P3. Unlocks that add variety, shown as locks (contracts, quests, blueprints)

**Evidence:**
- **Vampire Survivors:** 243 achievements that double as unlocks; the first ones land in minute 1 [W].
- **Halls of Torment:** traits unlock "only" through quests such as "wait 180 seconds before moving with the Archer";
  25 blessings, 1000+ traits; 1M copies [S].
- **Megabonk:** "Quests unlock automatically as you play," and a scarce Silver currency buys permanent content; 1M in
  two weeks, solo, in Unity [S].
- **Dead Cells:** blueprints found in runs join the drop pool for future runs; the restraint is "variety over raw
  power" [S].
- **Hades:** "many channels of permanent progression" [S].
- **Hodent (GDC 2016):** "Show the locks before giving the keys" [W].
- **Steam data:** games with more achievements show longer playtime, but not linearly [S].

**Love and hate:**
- Players love a new toy every run or two early on, and a checklist they can see.
- They hate stat grind. `DESIGN-RESEARCH.md` row 14 has the Gunfire Reborn, Witchfire and Hades II currency complaints.

**SKYGRIND application: a CONTRACTS board of 30-40 entries.**
- **Start from the web:** port `challenges.js` (24 badges with progress bars) and `unlocks.js` (6 flags) [R].
- **Every contract unlocks variety, never stats.** For example:
  - a locked MOD joins the draft pool (the web already locks 9 of 19 behind flags)
  - a gun joins the box pool
  - a mutator or heat rung
  - a gun finish
  - a starting rig (P12)
- **The cadence** [U, from the VS / Megabonk shape]:
  - the first unlock in under 5 minutes
  - one every 1-2 runs for the first 2 hours
  - then one every 3-5 runs
- **Mirror the contracts as Steam achievements** (U6-01).
- **Fit:** CoD's calling cards and challenges plus ULTRAKILL's challenge-per-level.
- **Builds on:** the event bus (every web event name is already emitted) and the PROTOCOL / exfil / forge events.
- **Effort:** M for the board and 24 ports; M more for the unlock hooks.
- **Risk:**
  - Gating the demo's fun behind locks. In the demo, unlock things that add to the fun and leave the core in.
  - Save data: use PlayerPrefs or a JSON file now and Steam Cloud later.

### P4. Mastery tracks (CoD camos and prestige; ULTRAKILL's palettes)

**Evidence:**
- **The BO6 Zombies camo ladder per gun** [W: TheGamer]:
  - nine military tiers at **100 / 200 / 300 / 400 / 600 / 800 / 1,000 / 1,500 / 2,000** critical kills
  - then 2 special camos
  - then Mystic Gold (10 rapid kills, 15 times), Opal (30 special zombies), Afterlife (20 kills without damage, 10 times;
    needs Opal on **33 guns**) and Nebula (10 elite kills)
- **How long it takes:** players report 5-7 months for Nebula [S]. Mastery camos have existed since CoD 4's gold (2007)
  [S].
- **BO6 Prestige:** 10 prestiges, then Prestige Master to level 1,000, shared across MP, Zombies and Warzone [S].
- **ULTRAKILL:** custom weapon palettes cost **1,000,000 P per weapon**, an in-game currency sink [S: wiki].
- **The web already has finishes:** gun finishes recolour "the neutral body tones ... never the family trim or the core
  gem; cosmetic only" (`finishes.js`) [R].

**Love and hate:**
- Players love visible proof of skill on their gun, and goals that make them try every weapon.
- They hate:
  - grinds so long that players exploit them (BO6's zipline immortality and boat-blocking) [S]
  - bad rewards at the top ("not worth your time" about BO6's level-1000 rewards) [S]
  - being forced to use weak guns

**SKYGRIND application: a MASTERY track per gun of 5-6 tiers.**
- **The challenges teach SKYGRIND's verbs:**
  - FRESH kills
  - air kills
  - SWAP kills
  - parry or slam kills
  - kills at S rank
  - a forged-gun tier
- **The rewards are finishes.** The last tier is an animated dusk finish (an emissive sweep).
- **Fit:** the camo grind becomes a tutorial for ULTRAKILL's style system. That is the cleanest single bridge between
  the two halves [U].
- **The counts:** tens to low hundreds per tier, not CoD's 2,000. With 6 guns in the demo (18 later), a track per gun
  stays short [U].
- **Builds on:** the arsenal's events (`primed`, `crossHit`, `swapKill`), `styleAward`, `rankChange`, the forge, the
  blocky kit's materials and `finishes.js`.
- **Effort:** M for the tracks and UI; S-M per finish material.
- **Risk:** the palette rules. Finishes must avoid enemy red-orange, RIVAL magenta and boundary violet. Keep them to
  the neutral tones, as `finishes.js` does [R].

### P5. Shared-seed competition: daily and weekly runs, async boards, ghosts

**Evidence:**
- **Spelunky's Daily** [W: Game Developer]: one attempt, the same seed for everyone, its own board.
  - "one small slip-up will cost you the entire day."
  - Players posted their runs so others could learn the same levels. The author couldn't remember "a year-old game
    being re-energised" like it.
- **DRG's weekly Deep Dive** [S]: seeded so "everyone will get the same one," rewards once per week, and players compare
  times and tactics.
- **Neon White** [S]: medals and friends' times stay **hidden until the first clear**; seeing impossible times first was
  "discouraging."
- **Friends versus global boards** [S: Chou, Crook]: friend and nearby-rank boards beat global ones. A global board
  motivates roughly the top 5%.

**Love and hate:**
- Players love a reason to launch today, and comparing with friends.
- They hate cheaters at the top, being ranked against the world's best on day one, and lost streaks.

**SKYGRIND application:**
1. **DAILY.** Port `daily.js` [R]:
   - the UTC date seed and its twist (one mutator, a first mod)
   - the first finished run counts; later runs are practice
   - the streak
2. **WEEKLY.** One fixed seed and rung for the week, DRG-style.
3. **Boards** (Steam leaderboards, U6-01):
   - friends first, then your ±5 global neighbours; never the global top on first view
   - shown after the player's first DAILY
4. **Ghosts later** (`ghosts.js`, `ghostCodec.js`).

- **Fit:** CoD's round-record bragging plus ULTRAKILL's Cyber Grind board.
- **Builds on:** the Mulberry32 per-run seed, `seeds.js` codes and **the bit-exact sim.** An entry can carry its input
  replay as Steam UGC so suspicious scores can be re-simulated [U: `DESIGN-RESEARCH.md` row 15 names
  AttachLeaderboardUGC].
- **Effort:** S-M for a local daily (the web's code is 63 lines); M for the Steam boards; L for ghosts and verification.
- **Risk:**
  - A local-only daily has no shared board (`seeds.js` says so [R]). It needs Steam.
  - The seed must fix the director's and box's streams, and it already does.

### P6. A ladder of secrets and community discovery

**Evidence:**
- **CoD Zombies.** In older games "less than two percent" saw the Easter eggs unless they watched YouTube. Treyarch made
  main quests accessible and kept side eggs obscure [S: Dexerto]. Die Maschine's quest was solved in under 4 hours on
  launch day [S].
- **ULTRAKILL.** Prime Sanctums unlock only after P-ranking every level of an act [S: wiki]. Secret levels are among its
  most-shared content (`ZOMBIES-ULTRAKILL-RESEARCH.md`).
- **Vampire Survivors.** Its secrets "were crafted out of necessity in order to keep up with the ravenous demand for new
  content" [S].
- **Lethal Company.** "A shared folklore of failure"; 2.4B TikTok views on its hashtag [S].

**Love and hate:**
- Players love "did you know..." moments that spread, and hunting together on launch week.
- They hate main content that is impossible to find without a wiki, and secrets that are only padding.

**SKYGRIND application: three tiers.**
1. **Guided.** THE PROTOCOL, ported, with its HUD line.
2. **Hidden.** The PROTOCOL's secret fifth relay (ported, finish not ported), plus 3-5 collectibles on the movement
   showpieces that reward the grapple and wall routes.
3. **Prime.** One hard secret gated by mastery, for example exfil at grade S or SS on heat 3. Its reward is a cosmetic
   or a new mode taste, such as a boss-rush door.

- **Fit:** CoD's egg culture plus ULTRAKILL's P-rank-gated sanctums.
- **Builds on:** `protocol.js`, `secretRelay`, the unlock flags and `finishes.js`.
- **Effort:** M per tier; L for a full main-quest finish.
- **Risk:**
  - Content cost on one map.
  - Secrets get solved in hours, which is fine: the spread is the point.
  - Write no lore that GG hasn't approved.

### P7. Onboarding and first-session retention

**Evidence:**
- **Hodent (GDC 2016)** [W]:
  - Warframe: "80% of all players ever kept playing after one hour"
  - at most "3 items to process at the same time" when learning
  - "Never punish the players WHILE THEY ARE JUST LEARNING"
  - learn by doing
- **First milestones:** VS survive 1 minute 97.8% versus DRG's first assignment 45.0% [W]. The first payoff has to be
  short.
- **Platform numbers:** session median 18 min [W]; Steam refunds are open under 2 h of play [S]; the Next Fest demo
  median is 18 min (prior doc).
- **The prior research's warning** (`ZOMBIES-ULTRAKILL-RESEARCH.md` pitfall 2): SKYGRIND stacks style, freshness,
  momentum, swap combos, statuses, perks, the forge, MODS, traps, the PROTOCOL and exfil.

**SKYGRIND application: a FIRST RUN script.**
- **Teach one system per round:**
  - R1: move and shoot
  - R2: dash, slide and style
  - R3: the wall buy and points
  - R5: perks
  - R6: power and the PROTOCOL
  - later: the forge and the box
- **The first contract unlock lands in round 1-2.**
- **No new system's failure is punished in its first round.**
- **Tips port** (`tips` in the web's Survival).
- **Effort:** M.
- **Risk:** a forced tutorial annoys returning players. Make it the default for a first run only, skippable.

### P8. Grade and rank chasing (ULTRAKILL's P-rank, Neon White's medals)

**Evidence:**
- **ULTRAKILL's P rank:** S in all three categories (time, kills, style) **without restarting** [S: wiki]. Prime
  Sanctums need all P-ranks of an act [S].
- **Neon White:** medals revealed progressively [S].
- **SKYGRIND has the base:** the grade S..D, best moments and the score line (runMoments.js, golden) [R].

**SKYGRIND application:**
- **A best grade per (rung, run end)**, shown on the title and RESULTS.
- **A "P" grade on an exfil:** S grade + SS peak style + no down.
- **A grade history strip** across the last 10 runs.
- **Fit:** it is the ULTRAKILL half's own retention loop.
- **Effort:** S.
- **Risk:** a grade that feels arbitrary. Show the formula's parts on RESULTS.

### P9. Clips, sharing and streamability

**Evidence:**
- **Friendslop and Lethal Company:** "precision-engineered to be exponentially more fun with friends"; viral via
  TikTok [S]. Balatro and VS grew through streams (`DESIGN-RESEARCH.md` row 17).
- **Steam Game Recording** is out of beta (Nov 2024). The **Timeline API** lets a game add instantaneous or range events
  with a clip priority (Standard / Featured) so Steam suggests clips [S: Steamworks ISteamTimeline].

**SKYGRIND application:**
- **Timeline events:**
  - multi-kills
  - SS rank reached
  - a perfect parry
  - the forge
  - a box wonder pull
  - exfil reach and outcome
  - each set Featured
- **A RESULTS build card:** gun, mods, perks, grade, heat and the seed code (`seeds.js`), with **RETRY SAME SEED**.
- **COPY SCORE LINE** (a web feature not yet ported) [R: PORT-STATUS].
- **Effort:** S for the Timeline hooks once Steamworks is in; S-M for the card.
- **Risk:** none of note. The Timeline hooks don't run without Steam.

### P10. Build variety and synergy discovery

**Evidence:**
- **Balatro:** joker pairs that redefine scoring [S].
- **RoR2:** no cap on stacks, so "every item you collect remains significant" [S].
- **RoR2's logbook:** collecting 10 log entries reaches 63.0% [W].
- **Halls of Torment:** reroll, augment and ban per run [S].
- **Balatro's collection:** completion 4.3% [W]. A codex is cheap long-tail content.

**SKYGRIND application:**
- **A CODEX:** guns, mods, perks, enemies and secrets, each with "first seen" and a short line. Unknown entries show as
  silhouettes (Hodent's locks).
- **NAMED COMBOS** when two mods or a perk and a gun interact, plus a combo award.
- **Banish and skip** in the MODS draft (`DESIGN-RESEARCH.md` row 8).
- **Builds on:** MODS (10 of 19), the forge's family bonus, the arsenal's CROSS / COLLAPSE / OVERLOAD.
- **Effort:** S-M.
- **Risk:** a codex with text GG hasn't approved. Keep the lines mechanical.

### P11. A content cadence without FOMO (seasons, passes, leagues)

**Evidence:**
- **Comebacks after updates:**
  - Helldivers 2: Escalation of Freedom took it from 30k to a 62.8k peak in one day; its Xbox launch took it from 146k
    to 390k in 7 minutes [S]
  - Path of Exile: every 3 months, from about 3k a day to 200k+, about 100k concurrent for the first month [S]
- **Collapses after droughts:**
  - MH Wilds: -98%, "a lack of meaningful content" [S]
  - Destiny 2: -80-90%, blamed on a content drought among other things [S]
- **Seasons without FOMO:**
  - **DRG:** six free seasons, all replayable at a Season Terminal, with progress kept; old cosmetics return to loot
    pools [W: Destructoid]
  - **Helldivers 2:** Warbonds never expire. Arrowhead: "the Warbonds are NOT a pay-to-win, FOMO-driven system" [S]
  - **Fortnite** is the opposite: about 75-100 h per pass over a 70-80 day season, with countdown timers [S, weak
    secondary sources]
- **Early Access:**
  - Hades II's plan was "a Major Update every few months," May 2024 to 1.0 in Sep 2025 [S]
  - An academic study found EA review scores don't correlate with update frequency, but 89% of EA games review equal or
    better during EA [S]

**SKYGRIND application: an EA roadmap of free major updates every 8-12 weeks.**
- **Each update brings** a map, a boss or a gun family, plus a contract set that **never expires**.
- **Optional "season" contract sets** stay selectable forever, DRG's model.
- **Announce only what is built** [U].
- **Fit:** CoD Zombies' DLC-map rhythm without the paywall.
- **Effort:** L, ongoing.
- **Risk:** solo burnout. One map in EA was the genre's top content complaint (prior doc), so the first update should be
  map 2.

### P12. Social play and player identity (co-op, classes, loadouts)

**Evidence:**
- **The charts:**
  - every 2025 Top Sellers Platinum game has multiplayer [W + U]
  - **L4D2** is in the Most Played Platinum tier, with about 20-40k concurrent in 2026 [S]
  - the DRG, Helldivers 2 and R.E.P.O. hits are co-op
- **Classes and characters:** DRG has 4 classes, Megabonk 20 characters, Halls of Torment its classes.
  `COOP-RESEARCH` and D-02 already put co-op after the demo.

**SKYGRIND application:**
- **For the demo: starting RIGS** unlocked by contracts. A rig is a starting gun, one perk discount and one starting
  MOD, giving identity without stats.
- **For EA: online co-op (2-4).** The deterministic sim makes lockstep or rollback more feasible than for most [U]. The
  Players work (NIGHT9) is the start.
- **Effort:** M for rigs; L for co-op.
- **Risk:** co-op is the largest scope item in the game. Rigs must not become power creep.

### Monetization (a constraint, not a feature)

**Evidence:**
- Players accept cosmetics and reject paid power (69% fine with cosmetic-only in one survey) [S, secondary].
- Buying functional advantages is disliked, and the buyers are respected less [S].
- Warframe lets players earn every item in play and trade premium currency between players [S].
- The Helldivers quote above.

**SKYGRIND:** premium, no MTX in EA, and any paid DLC is maps or cosmetics only. The prior doc's "one currency (points),
no paid power ever" stands. Effort: none.

---

## 4. What players hate: the anti-patterns, with evidence

| Anti-pattern | Evidence | SKYGRIND guard |
|---|---|---|
| FOMO timers and expiring passes | Fortnite's countdowns [S]; praise for DRG and Helldivers for avoiding them [W/S] | contracts never expire |
| Pay to win | Arrowhead's own statement; the survey data [S] | premium, no paid power |
| Grinds so long players exploit them | BO6 zipline / boat exploits [S]; Nebula takes 5-7 months [S] | mastery counts in the tens to hundreds |
| Rewards not worth the grind | BO6 level-1000 rewards "not worth your time" [S] | the top tier is the best-looking reward |
| Content drought, no endgame | MH Wilds -98% [S]; Destiny 2 -80-90% [S] | ladder + daily + weekly as an endgame from day one; 8-12 week cadence |
| Base difficulty raised for everyone | Destiny 2's "punishing" Edge of Fate [S] | difficulty only as opt-in rungs |
| Global boards before the first clear | Neon White [S]; the top-5% effect [S] | friends and neighbours first, shown after a first DAILY |
| Stat grind as the meta | `DESIGN-RESEARCH.md` row 14 | variety-only unlocks (the web's own rule) |
| Too many systems at once | Hodent's 3-item limit [W]; prior pitfall 2 | one system per round on a first run |

---

## 5. The ranked top 12 for the demo, then Early Access

Impact: H / M / L. Effort: S (hours), M (a day), L (several days). "Stage" says whether it belongs in the Next Fest
demo or in EA.

| # | Feature | Imp | Eff | Stage | Why (evidence) | Builds on |
|---|---|---|---|---|---|---|
| 1 | **CONTRACTS board (30-40) with variety unlocks, shown as locks**: port `challenges.js` (24) + `unlocks.js`; first unlock in under 5 min, then one per 1-2 runs for 2 h; mirrored as Steam achievements | H | M | demo | VS's first unlocks at 1 min reach 97.8% and its 243 achievements are its unlock list; Halls of Torment and Megabonk use quest unlocks; Hodent: "show the locks" | event bus, web modules, U6-01 |
| 2 | **The PAYOFF strip on DOWN / RESULTS**: grade, PB deltas, the 3 nearest contract bars, mastery XP, NEXT UNLOCK | H | S-M | demo | Hades: "every run counts"; the run ends at exfil inside the 18-30 min session band (GameAnalytics) | RESULTS, records, runMoments |
| 3 | **HEAT ladder (6-8 rungs)**: a win unlocks the next rung; one readable rule per rung; score multiplier; a badge on the grade | H | M | demo (3 rungs) then EA | Balatro Red 43.8% / Gold 12.2%; Brotato D5 36.5%; DRG H4 34.2%; Helldivers' Super Helldive drove its comeback | `mutators.js`, LAB flags, exfil win |
| 4 | **DAILY seed** (one counted try, twist, streak), then **WEEKLY**; Steam boards friends and neighbours first, shown after the first clear | H | S-M local, M Steam | demo (local daily), EA (boards) | Spelunky's Daily "re-energised" a year-old game; DRG's weekly Deep Dive is the same for everyone; Neon White; friends beat global | `daily.js`, `seeds.js`, Mulberry32, bit-exact sim |
| 5 | **First-run onboarding script**: one system per round, the first unlock in R1-2, no punishment while learning, tips | H | M | demo | Warframe 80% kept after 1 h; the 3-item limit (Hodent); DRG's first assignment only 45% versus VS's 97.8% first step | the round flow, banners, the tips port |
| 6 | **MASTERY track per gun** (5-6 tiers that teach FRESH, air, swap, parry and S-rank kills; finishes as rewards) | H | M | demo (2-3 tiers) then EA | CoD camo ladders (100 to 2,000 kills, then mastery camos) kept BO6 players for months; ULTRAKILL's 1M-P palettes | arsenal and style events, `finishes.js`, blocky kit |
| 7 | **Grade chase**: a best grade per rung, a "P" exfil (S + SS + no down), a history strip | M | S | demo | ULTRAKILL's P-rank without restarts gates its Prime Sanctums; Neon White's medals | grade S..D, runMoments |
| 8 | **Secrets ladder**: PROTOCOL (guided), then the hidden relay and 3-5 movement collectibles, then one Prime secret gated by an S-grade exfil on heat 3 | H | M-L | demo (tiers 1-2), EA (tier 3) | under 2% saw CoD's eggs until main quests were made accessible; Die Maschine solved in under 4 h (launch-week buzz); ULTRAKILL's sanctums | `protocol.js`, `secretRelay`, unlock flags |
| 9 | **Clips and sharing**: Steam Timeline events (Featured on SS, perfect parry, box wonder, exfil), a RESULTS build card, seed code, RETRY SAME SEED, COPY SCORE LINE | M | S | demo | Timeline API clip priorities; growth through streams and clips (Balatro, VS, Lethal Company's 2.4B TikTok views) | `seeds.js`, the score line, U6-01 |
| 10 | **CODEX and named combos**, plus banish / skip in drafts | M | S-M | EA | RoR2's 10-log step 63.0%; Balatro's collection 4.3% long tail; Halls of Torment's ban / reroll | MODS, forge, arsenal combos |
| 11 | **No-FOMO EA cadence**: a free major update every 8-12 weeks (map 2 first), each with a contract set that never expires; no MTX, no paid power | H | L (ongoing) | EA | Helldivers' doubling; PoE's 3k to 200k; MH Wilds' -98% and Destiny 2's -80-90% droughts; DRG's replayable seasons; Hades II's EA plan | the contract board (#1) |
| 12 | **Co-op (2-4), with starting RIGS for identity first** (rigs: M, in the demo) | H | L | EA | every 2025 Top Sellers Platinum game has multiplayer; L4D2, Peak and R.E.P.O. in the Most Played Platinum tier; friendslop revenue | Players (NIGHT9), deterministic sim, D-02 |

**Why this order** [U]:
- **#1-#5 are the demo's retention core.**
  - They turn an 18-minute demo session into a reason to come back on day 2 of the fest: a locked unlock, a daily, a
    rung.
  - They cost a few days in all, because the web already designed most of them.
- **#6-#9 add depth that streamers and reviewers name.**
- **#10-#12 decide Early Access longevity.**
  - Co-op has the highest ceiling of any feature here and the largest cost. It stays an EA item (D-02).

**Decisions this raises for GG** (proposals only; nothing changes without him):
- Heat-rung rules beyond the web's mutators touch balance.
- Rigs and mastery finishes touch the look's palette rules.
- Secret content and codex text are GG's to approve.
- Whether the demo carries a local daily before Steam boards exist.

---

## 6. Sources

**Platform and market data:**
- Steam Best of 2025 tiers: https://fullcleared.com/news/steams-best-of-2025-revealed-including-top-sellers/ [W]
- TheGamer on the 2025 list: https://www.thegamer.com/steam-valve-2025-best-sellers-list-multiplayer-titles/ [W]
- Steam Replay 2025: https://www.gamingonlinux.com/2025/12/steam-replay-is-live-and-notes-only-14-of-playtime-spent-by-all-steam-users-was-for-2025-releases/ [W]; https://www.thegamer.com/steam-playtime-2025-very-little-new-games/ [S]
- Steam Replay 2024: https://www.pcgamer.com/games/only-15-percent-of-all-steam-users-time-was-spent-playing-games-released-in-2024/ [S]
- Circana 2025: https://www.gamespot.com/gallery/2025-best-selling-games/2900-6281/ ; https://gamedevreports.substack.com/p/circana-the-us-gaming-market-in-december25 [S]
- GameAnalytics 2026 PC benchmarks: https://gamedevreports.substack.com/p/gameanalytics-mobile-and-pc-game ; https://www.gameanalytics.com/reports/2026-mobile-pc-gaming-benchmarks [W]
- GameDiscoverCo genre playtime: https://newsletter.gamediscover.co/p/which-game-genres-get-the-most-playtime [W]
- Completion rates (1,616 games): https://www.sciencedirect.com/science/article/abs/pii/S1875952118300181 [S]
- Friendslop revenue: https://appmagic.rocks/blog/friendslop-steam-games-2025 ; https://kotaku.com/steam-top-selling-2025-friendslop-rpgs-sales-2000654157 [S]

**Steam global achievement stats (all [W]):**
- Balatro: https://steamcommunity.com/stats/2379780/achievements
- Vampire Survivors: https://steamcommunity.com/stats/1794680/achievements
- Risk of Rain 2: https://steamcommunity.com/stats/632360/achievements
- Hades: https://steamcommunity.com/stats/1145360/achievements
- Brotato: https://steamcommunity.com/stats/1942280/achievements
- Deep Rock Galactic: https://steamcommunity.com/stats/548430/achievements
- Dead Cells: https://steamcommunity.com/stats/588650/achievements

**Developers and design:**
- Hades narrative rewards: https://www.gamedeveloper.com/design/how-supergiant-weaves-narrative-rewards-into-i-hades-i-cycle-of-perpetual-death [W]; https://gdconf.com/article/roguelikes-and-narrative-design-with-hades-creative-director-greg-kasavin-gdc-podcast-ep-16/ [S]
- Hodent, GDC 2016 onboarding: https://celiahodent.com/gamers-brain-ux-onboarding/ [W]
- Spelunky Daily: https://www.gamedeveloper.com/design/the-understated-genius-of-the-i-spelunky-i-daily-challenge [W]
- DRG seasons: https://www.destructoid.com/deep-rock-galactic-defeats-fomo-by-letting-you-play-old-seasons-whenever-wherever/ [W]
- DRG weekly Deep Dive: https://steamcommunity.com/app/548430/discussions/1/1631916887488274210/ [S]
- Helldivers 2 game master and comebacks:
  - https://kotaku.com/helldivers-2-galactic-war-game-master-joel-1851297376
  - https://game8.co/articles/latest/helldivers-2-escalation-of-freedom-update-doubles-player-count-after-downward-spiral
  - https://www.windowscentral.com/gaming/xbox/helldivers-2s-xbox-release-spiked-player-counts-so-high-it-caused-server-crash-issues
  - https://dotesports.com/helldivers-2/news/helldivers-2-players-praise-devs-for-not-falling-into-infamous-fomo-trap
  - all [S]
- Path of Exile leagues: https://www.escapistmagazine.com/path-of-exile-interview-chris-wilson-grinding-gear-games-10-year-anniversary-path-of-exile-2/ [S]
- MH Wilds decline: https://www.thegamer.com/monster-hunter-wilds-playercount-steam-nearly-1-percent-left/ ; https://www.gfinityesports.com/article/content-drought-causes-massive-player-drop-in-monster-hunter-wilds [S]
- Destiny 2 decline: https://gamerant.com/destiny-2-player-count-edge-of-fate-low/ ; https://thegamepost.com/dying-destiny-2-steam-player-count-edge-of-fate/ [S]
- Hades II EA cadence: https://www.supergiantgames.com/blog/hades2-early-access-is-here/ [S]
- EA empirical study: https://dl.acm.org/doi/10.1007/s10664-017-9531-3 [S]
- BO6 Zombies camos: https://www.thegamer.com/cod-bo6-zombies-weapon-camo-challenges-tips/ [W]; https://www.thegamer.com/call-of-duty-black-ops-6-blops6-zombies-youre-taking-camo-grind-too-seriously/ [S]
- BO6 Prestige / level 1000: https://www.pcgamesn.com/call-of-duty-black-ops-6/prestige-master-1000-rewards [S]
- BO6 480M hours: https://www.callofduty.com/uk/en/blog/2025/01/call-of-duty-black-ops-6-zombies-115-day-announcement [S]
- CoD Easter egg accessibility: https://www.dexerto.com/call-of-duty/why-have-treyarch-locked-all-major-call-of-duty-zombies-easter-eggs-185840/ [S]
- ULTRAKILL ranks, palettes and sanctums: https://ultrakill.wiki.gg/wiki/Ranking ; https://ultrakill.fandom.com/wiki/Weapon_customization [S]
- ULTRAKILL has no Steam achievements: https://steamhunters.com/apps/1229490/achievements [S]
- Neon White's leaderboards: https://www.vice.com/en/article/neon-whites-developers-being-too-good-at-their-own-game-caused-real-problems/ [S]
- Megabonk: https://www.gamedeveloper.com/business/indie-hit-megabonk-moves-over-a-million-copies-in-two-weeks ; https://en.wikipedia.org/wiki/Megabonk [S]
- Halls of Torment: https://en.wikipedia.org/wiki/Halls_of_Torment [S]
- Vampire Survivors' secrets: https://www.gamesradar.com/games/action/vampire-survivors-kicked-off-a-game-development-gold-rush-but-has-a-legitimately-new-genre-emerged-between-the-cash-ins/ [S]
- Vampire Survivors' run length: https://gamerant.com/vampire-survivors-30-minutes-reaper-boss-death-beat-kill/ [S]
- RoR2 design: https://www.gamedeveloper.com/design/how-moving-from-2d-to-3d-shaped-the-design-of-i-risk-of-rain-2-i- [S]
- Lethal Company: https://newsletter.gamediscover.co/p/what-can-we-learn-from-lethal-companys ; https://www.pushtotalk.gg/p/how-lethal-company-sold-10-million-copies [S]
- Balatro sales: https://www.playstack.com/news/balatro-5-million-copies-sold/ [S]
- L4D2 player counts: https://steamcharts.com/app/550 [S]
- Steam Timeline API: https://partner.steamgames.com/doc/api/ISteamTimeline [S]
- Steam refunds: https://www.gamesradar.com/platforms/pc-gaming/steam-changes-its-refund-policy-so-that-pre-order-early-access-playtime-now-counts-towards-the-2-hour-rule/ [S]

**Motivation research:**
- Self-determination theory: https://link.springer.com/article/10.1007/s11031-006-9051-8 [S]
- Quantic Foundry: https://quanticfoundry.com/ [S]
- Leaderboard design (secondary): https://yukaichou.com/advanced-gamification/how-to-design-effective-leaderboards-boosting-motivation-and-engagement/ ; https://adriancrook.com/how-leaderboards-impact-player-retention/ [S]
- Monetization attitudes (secondary, weak): https://theconversation.com/very-sneaky-tactics-we-asked-gamers-how-they-feel-about-monetisation-in-digital-gaming-173030 ; https://www.alibaba.com/product-insights/why-do-gamers-hate-microtransactions-understanding-the-pay-to-win-controversy.html [S]
- Warframe: https://fandomwire.com/warframes-monetization-policy-fully-embraces-the-free-to-play-model-unlike-destiny-2-making-it-more-accessible-to-players/ [S]

**Repo (read only, [R]):**
- `skygrind/src/modes/`: challenges.js, unlocks.js, daily.js, seeds.js, mutators.js, finishes.js, ghosts.js
- `skygrind/src/config.js` (challenges list, mutators)
- `SKYGRIND-unity/PORT-STATUS.md`, `DESIGN-RESEARCH.md`, `ZOMBIES-ULTRAKILL-RESEARCH.md`, `tasks.json` (U6-01)

**Not checked:**
- Fortnite's own battle pass data. Only weak secondary sources were found, tagged [S].
- Steam Replay's per-user stats beyond the median of 4 games.
- Boss Cell funnel rates for Dead Cells.
- Any Valorant, CS2 or Apex ranked-retention data. Not pursued: SKYGRIND is PvE.

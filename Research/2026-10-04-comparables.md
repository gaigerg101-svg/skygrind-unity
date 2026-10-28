# research-comparables.md: the games closest to SKYGRIND, what lasted, what faded, and what that means

Researched 2026-10-04 for GG. The question: which indie / AA shooters are closest to SKYGRIND, which ones lasted and
why, what players praise and hate, how they organise their screens, what their demos did, and what is new in the last
12 months. This pass builds on `Research/2026-10-03-retention.md` (funnels, pillars) and `ZOMBIES-ULTRAKILL-RESEARCH.md`
(sections 2.3, 4) and does not repeat them.

**Method and limits (read first):**
- Every number was read on **2026-10-04**. Five research agents gathered it in parallel; I spot-checked Gunfire Reborn,
  Sker Ritual and Killing Floor 3 against the live endpoints and the numbers matched exactly.
- **Reviews:** Steam's review API, all languages, all purchase types (`appreviews/APPID?json=1&language=all&purchase_type=all`).
  "Recent" is the store page's last-30-days line, which counts English reviews only (ULTRAKILL: 245,731 in the API,
  151,050 on the English store page).
- **Players:** steamcharts.com (all-time peak, last-30-days average). SteamDB blocked the fetches; where a SteamDB
  number surfaced in a snippet it is given in the notes.
- **Sales estimate:** Boxleiter, all-language reviews x30 to x60. GameDiscoverCo's own study gives a median of 38 for
  2020 launches and a range of 20-60, so the low end can be lower [W]. SteamSpy owner ranges are shown where they
  exist. SteamSpy has no data for apps from 2025 on, and it no longer tracks playtime (it returns 0).
- **Blocked:** HowLongToBeat, Reddit (old and new), SteamDB, Gamalytic and the Fandom wikis. The session's 200-search
  budget ran out. So: **no Reddit quotes** (Steam review text stands in), and playtime comes from snippets [S] or is
  "not found" (n/f).
- **Tags:** [W] fetched; [S] search snippet only; [U] my inference; n/v means not verified.

---

## 0. Bottom line (ranked for SKYGRIND)

1. **Feel and readability decide this genre's reviews.** The movement shooters sit at 89-98% positive (ULTRAKILL 97.3%
   of 245,731). The Zombies-likes that failed, failed on basics: Sker Ritual's most-voted negative is forced mouse
   acceleration, then mazes with "no minimap" and desync; KF3 (44.0% of 23,861) shipped a battle pass before text chat,
   10-30 minute shader compiles and a walk-to-a-terminal hub [W]. SKYGRIND's exact movement is the moat.
2. **Only content drops make a game grow.** ULTRAKILL is the one game here that grew after launch: monthly peaks of 712
   (EA) → 27,835 (Layer 7) → 22,928 (revamp) → 69,799 (Layer 8, Feb 2026), then a price rise to $29.99. Gunfire Reborn
   holds ~905 average five years after 1.0 on a season every ~4 months. Every single-release game faded to 20-200
   average; Sker Ritual (no map since Oct 2024) averages 23 [W].
3. **Horde and wave modes are what revive shooters, and they are SKYGRIND's core.** Boltgun's free Horde Mode lifted
   its monthly peak 57%; Echo Point Nova's Rogue Mode and Severed Steel's Rogue rework each set an all-time peak;
   Gunfire Reborn added a 30-minute horde mode; Trepang2 sells a Survival DLC [W].
4. **Price Early Access at $14.99-19.99.** The 2025-26 hits sit at $9.99-19.99 (Megabonk, DRG: Survivor, Mycopunk, Far
   Far West, Jump Space); the median hit price fell from $19.99 to $16.99 [W GDCo]. The $40 horde shooters faded (KF3,
   Toxic Commando, Painkiller with a 214 peak), and EA priced like a 1.0 cost even DRG (Rogue Core, 60.2%) [W].
5. **Every game that lasted has a difficulty ladder:** ULTRAKILL (5 tiers, ranks per tier), Gunfire Reborn
   (Reincarnation 1-10), Roboquest (Guardian I-IV), DRG: Survivor (Hazard 1-5), Witchfire (8 Torments) [W/S]. HEAT 1-3
   is the right start.
6. **Short sessions win; long runs with no save are a top complaint.** ULTRAKILL levels take 3-15 min, a DRG: Survivor
   run ~20 [W]. "30 minute runs would be much better" (Roboquest); "5 is too short, 10 is too long" (KF3); "No save
   system… Lose hours of playtime" (Sker) [W]. Keep the 20-30 minute exfil and ship save-and-quit (NIGHT13).
7. **Performance shows up in reviews.** KF3 is Deck Unsupported for "graphics performance"; Anger Foot "brought my 5090
   to its knees"; Rogue Core is "stutter city"; none of ULTRAKILL's 100 recent negatives mention performance [W].
   Shader warm-up and the 6.9 ms bench bar are product work, not polish.
8. **Steam Deck Verified is the genre norm.** All 10 movement shooters checked are Verified, as are Gunfire Reborn, DRG:
   Survivor, Witchfire, Deadzone: Rogue and Sulfur. Sker Ritual, Roboquest and RoR2 are only Playable (illegible text).
   1.7% of boomer-shooter reviews come from mostly-Deck players, against 0.22% for FPS [W GDCo].
9. **One run-setup screen, few meta screens, plain words.** Risk of Rain 2 picks survivor, loadout and difficulty in
   one lobby. The anti-patterns, from reviews: KF3's walk-to-a-computer hub (225 votes), CoD's "Every menu feels like a
   shopping mall", Gunfire's "catastrophic item/skill description", Witchfire's 10 stations and 4 currencies [W].
10. **A game gets one Next Fest.** February 2027: register by Jan 10, 2027; demos up a month or more early earned 2.5x
    the wishlists. June 2027: register by Apr 25. Momentum in the two weeks before predicts the result best (ρ 0.81).
    Median demo-to-wishlist conversion is 16-19%; co-op demos convert worst [W].
11. **Co-op is the ceiling, but not for the demo.** The 2025-26 hits are co-op PvE (Far Far West: 1.3M including keys in
    ~6 weeks), but Roboquest's 2-player cap is a top complaint and Mycopunk's solo players call it "co-op without the
    co-op" [W]. Design for 4; ship solo first.
12. **Players punish broken trust.** Deadzone: Rogue's most-voted review: "support is already dead for it 1 year later
    to make room for a sequel". AI-art complaints pull down Gunfire Reborn (recent 83%), Sker Ritual, Deadzone: Rogue
    and BO7 [W].

---

## 1. The comparables table

### 1a. Market numbers

All values are [W] as of 2026-10-04 unless tagged. Recent = English, last 30 days. Now = SteamCharts last-30-days
average.

| Game (type) | Release; EA → 1.0 (EA length) | Price (regular) | Reviews, all languages (% pos) | Recent | Peak (when) | Now | Sales estimate |
|---|---|---|---|---|---|---|---|
| **Sker Ritual** (Zombies-like) | EA 2022-10-06 → 1.0 2024-04-18 (18.5 mo) | $19.99 | 6,015 (81.5%) | 61% of 13 | 5,992 (Apr 2024) | 23 | 180-361k; SteamSpy 200-500k; "2 million players" on all platforms (publisher, Mar 2026) |
| **Killing Floor 3** (horde co-op) | 2025-07-24, no EA; delayed from Mar 2025 after its beta | $39.99 | 23,861 (44.0%) | 40% of 202 | 21,934 (Jul 2025) | 350 | 716k-1.43M; no official figure |
| **Zombie Army 4** (horde co-op) | Steam 2021-02-18 (2020 elsewhere, n/v) | $49.99 | 12,217 (86.2%) | 82% of 242 | 6,559 (Aug 2026, 90% off) | 454 | 367-733k; SteamSpy 500k-1M |
| **Toxic Commando** (horde co-op) | 2026-03-12 | $39.99 | 7,263 (73.3%) | ~60% (estimated from the histogram) | 8,684 (Mar 2026) | 176 | 218-436k; "disappointing" for the publisher |
| **CoD BO7 / BO6 Zombies** (reference) | BO6 2024-10-25; BO7 2025-11-14 | $69.99 | BO7 5,083 (37.7%); CoD HQ app 782,836 (58.6%) | BO7 ~35% (est.) | HQ app: 313,313 (Nov 2024) vs 91,752 (Nov 2025) | HQ 26,330 | BO7's EU launch week was over 50% below BO6's; BO6 had 50M players by Jul 2025 |
| **Blood Reaver** (Zombies-like) | EA 2026-04-15 | $14.99 | 198 (83.8%) | n/f | n/f | n/f | 6-12k [U] |
| **Roboquest** (FPS roguelite) | EA 2020-08 → 1.0 2023-11-07 (~39 mo) | $24.99 | 24,480 (95.3%) | 96% of 110 | 5,616 (Jan 2024) | 189 | 734k-1.47M; SteamSpy 500k-1M |
| **Gunfire Reborn** (FPS roguelite) | EA 2020-05-22 → 1.0 2021-11-17 (18 mo) | $19.99 | 103,768 (93.1%) | 83% of 156 | 35,002 (Jun 2021) | 905 | 3.1-6.2M; 2M announced at 1.0 |
| **Witchfire** (FPS roguelite) | Epic EA 2023-09; Steam EA 2024-09-23; 1.0 "late Q4 2026" (~39 mo) | $39.99 | 18,002 (91.3%) | 86% of 273 | 5,021 (Sep 2024) | 367 | 540k-1.08M on Steam; "over 500,000" [S] |
| **Deadlink** (FPS roguelite) | EA 2022-10-18 → 1.0 2023-07-27 (9 mo) | $24.99 | 3,673 (88.8%) | n/f | ~1,907 (2023, approx.) | <10 | 110-220k |
| **Deadzone: Rogue** (FPS roguelite) | EA 2025-04-29 → 1.0 2025-08-11 (3.5 mo) | $24.99 | 16,754 (82.4%) | 83% of 222 | 8,655 (Aug 2025) | 561 | 503k-1.0M; "nearly a million players" on all platforms |
| **Mycopunk** (co-op FPS) | EA 2025-07-10 → 1.0 2026-10-20 (~15 mo) | $14.99 | 4,740 (88.5%) | 80% of 56 | 2,397 (Jul 2025) | 89 | 142-284k |
| **Sulfur** (FPS roguelite) | EA 2024-10-28, still in EA | $24.99 | 10,517 (87.6%) | 90% of 391 | 2,561 (Nov 2024) | 858 (rising) | 316-631k; 52k units in its first ~3 weeks [W GDCo] |
| **ULTRAKILL** (movement) | EA 2020-09-03, still in EA (6 y 1 mo) | $29.99 (was $24.99 [S]) | 245,731 (97.3%) | 96% of 3,080 | 69,799 (Feb 2026, Layer 8) | 2,610 | 7.4-14.7M; SteamSpy 5-10M |
| **Neon White** (movement) | 2022-06-16 | $24.99 | 20,935 (97.9%) | 96% of 127 | 3,238 (Jun 2022) | 142 | 628k-1.26M; SteamSpy 200-500k |
| **Turbo Overkill** (movement) | EA 2022-04-22 → 1.0 2023-08-11 (16 mo) | $19.99 | 7,572 (93.9%) | 87% of 54 | 644 (Aug 2023) | 24 | 227-454k |
| **Mullet Madjack** (movement) | 2024-05-15 | $19.99 | 9,621 (97.7%) | 97% of 104 | 784 (May 2024) | 19 | 289-577k |
| **Severed Steel** (movement) | 2021-09-17 | $24.99 | 10,334 (95.4%) | 95% of 129 | 1,159 (Sep 2025, Rogue update) | 31 | 310-620k; SteamSpy 500k-1M |
| **Trepang2** (movement) | 2023-06-21 | $29.99 | 19,764 (92.7%) | 89% of 300 | 1,900 (Jun 2023) | 164 | 593k-1.19M |
| **Boltgun** (boomer shooter) | 2023-05-23 | $21.99 | 18,347 (89.5%) | 88% of 184 | 5,255 (May 2023) | 125 | 550k-1.1M |
| **Anger Foot** (movement) | 2024-07-11 | $19.99 | 4,150 (92.7%) | 83% of 74 | 1,072 (Jul 2024) | 28 | 125-249k |
| **Echo Point Nova** (movement, co-op) | 2024-09-24 | $24.99 | 8,054 (96.6%) | 93% of 94 | 1,111 (Mar 2025, Rogue Mode) | 37 | 242-483k |
| **Cultic** (boomer shooter) | 2022-10-13; Chapter 2 2025-09-18 | $9.99 + $9.99 | 8,759 (97.1%) | 92% of 85 | 836 (Sep 2025, Ch. 2) | 39 | 263-526k |
| **DRG: Survivor** (3D survivors-like) | EA 2024-02-14 → 1.0 2025-09-17 (19 mo) | $12.99 | 48,385 (86.2%) | 82% of 219 | 56,876 (Feb 2024) | 978 | 1.45-2.9M; 1M in about a month [S] |
| **Risk of Rain 2** (3D roguelite) | 1.0 2020-08-11 (EA start n/v) | $24.99 | 353,479 (93.8%) | 92% of 1,022 | 74,945 (Aug 2024) | 5,669 | 10.6-21.2M; SteamSpy 5-10M |
| **Megabonk** (3D survivors-like, solo dev, Unity) | 2025-09-18 | $9.99 | 106,732 (94.3%) | 88% of 1,109 | 116,969 (Oct 2025) | 3,149 | over 3M by Nov 2025 [W GDCo] |
| **Far Far West** (co-op FPS roguelite) | EA 2026-04-28 (a 12-month EA planned) | $19.99 | 38,389 (95.4%) | n/f | 47,031 | 2,577 (Sep) | 1.3M including keys by early June [W GDCo] |

**Anti-examples** [W unless tagged]:

| Game | Dates | Price | Reviews (% pos) | Peak → now | Stated reason |
|---|---|---|---|---|---|
| Spectre Divide (F2P 3v3) | 2024-09-03; shutdown announced 2025-03-13 | F2P | 19,692 (64.0%) | 26,232 → shut down | "not enough active players and incoming revenue"; over $35M raised, about 100 staff |
| Splitgate 2 | Jun 2025; back to beta Jul 2025 [S]; relaunched Dec 2025; development ended 2026-08-26 | F2P | 22,221 (63.7%) | 25,674 → 38 | "launched too early… rushed certain features" [S] |
| Hyper Light Breaker | EA 2025-01-14; development ended Oct 2025 [S] | $29.99 | 3,983 (52.4%) | 2,436 → ~0 | thin content for $30; "overshot the challenge" (secondary source) |
| FBC: Firebreak | 2025-06-17 | $19.99 now (launch price n/v) | 2,355 (64.0%) | 1,738 → 12 | "many players… leave within the first hour"; a €14.9M impairment |
| Painkiller (2025) | 2025-10-21 | $39.99 | 1,266 (55.0%) | 214 → 7 | "a dull co-op FPS in a boomer shooter's skin" [S] |
| DRG: Rogue Core | EA 2026-05-20 | $29.99 | 15,544 (60.2%) | 21,375 → ~480 | EA priced like the original; "upgrades are just stat modifiers"; "stutter city" |
| Highguard (F2P) | 2026-01-26; shut down 2026-03-12 | F2P | not available | ~97k → closed | "had not built a sustainable player base" |

### 1b. The product side

| Game | Team / engine | Co-op | Steam Deck | Update cadence (last 18-24 months) | Demo / Next Fest | Playtime |
|---|---|---|---|---|---|---|
| Sker Ritual | core of 5 / Unity | 1-4 online, crossplay | Playable (text) | maps Jul and Oct 2024; v1.3 Jul 2025 (voice chat, custom games); no map since | Jun 2022 Next Fest demo, one map [W] | n/f |
| Killing Floor 3 | n/f / UE5 | 1-6 | Unsupported (graphics performance) | a season every 3-4 months (Dec 2025, Mar 2026, Jul 2026) | closed beta Jan 2025, then the delay | n/f |
| Zombie Army 4 | Rebellion | 1-4 | Playable | none since Jul 2022 | n/f | n/f |
| Toxic Commando | Saber / Swarm | 1-4, bots | Playable (always online) | monthly patches; a map in Jun and a mode in Sep 2026 | Feb 2026 Next Fest demo, #8 [W GDCo] | n/f |
| Roboquest | RyseUp / UE4 | 1-2 | Playable (text) | 4 free updates in 2024; ended May 2025 | demo Jun 2020 [S] | runs 20-40 min; HLTB 9.3 / 22 / 44 h [S] |
| Gunfire Reborn | Duoyi / Unity | 1-4 | Verified | a season about every 4 months, plus a paid hero DLC each year | n/f | HLTB 13.5 / 40.5 / 99.7 h [S] |
| Witchfire | about 20-24 people [S] / UE4 | solo | Verified | a big update about every 6 months; patches every 1-2 weeks | none (Epic exclusive for its first year) | n/f |
| Deadzone: Rogue | n/f | 1-3, crossplay | Verified | Zone 4 Dec 2025; sequel announced May 2026 | Oct 2024 Next Fest demo; 150k+ wishlists before EA [S] | n/f |
| Mycopunk | first game / n/f | 1-4 | Unsupported | weekly devlogs; an update about every 2 months | demo at the reveal | n/f |
| Sulfur | n/f | solo | Verified | a named update every month | 2 demos in 2024 plus Oct 2024 Next Fest; 160k wishlists at launch [W GDCo] | n/f |
| ULTRAKILL | Hakita plus a small team / Unity | solo | Verified | one big layer every 1-2 years (Dec 2023, Feb 2025, Feb 2026) | Aug 2019 demo: Prelude plus 1-1, without the Cyber Grind [W] | levels 3-15 min; HLTB 6 / 15 / 47 h [S] |
| Neon White | small team / Unity | solo | Verified | none | Feb 2022 Next Fest | levels about 30 s |
| Turbo Overkill | about 2 people | solo | Verified | EA episodes, then 1.0 | Feb 2022 Next Fest | levels up to an hour (reviews) |
| Mullet Madjack | HAMMER95 / Unity | solo | Verified | a free boss rush Dec 2024 (the ULTRAKILL crossover) | Feb 2024 Next Fest; "560k wishlists in 6 months" [S, n/v] | about 2.5 h (one review) |
| Severed Steel | Greylock | solo | Verified | anniversary updates Sep 2025 and Sep 2026 | demo | campaign under 2 h (reviews) |
| Trepang2 | 1 → 4 people / UE4 | solo | Verified | DLC in 2023, including Survival Mode ($2.99) | demos from Dec 2019 | about 8 h |
| Boltgun | Auroch / UE4 | solo | Verified | DLC plus a free Horde Mode, Jun 2024; sequel announced May 2025 | n/f | n/f |
| Anger Foot | about 8 people / Unity | solo | Verified | n/f | Jun 2022 Next Fest demo (11 levels) | levels about 3 min |
| Echo Point Nova | Greylock | 1-4, friends only | Verified | Rogue Mode Mar 2025 | Jun 2023 demo | n/f |
| Cultic | solo developer / Unity | solo | Verified | Chapter 2 (paid) Sep 2025 | n/f | n/f |
| DRG: Survivor | Funday / Unity | solo | Verified | 3 DLCs; 1.0 Sep 2025 | n/f | runs about 20 min; HLTB 15 h main [S] |
| Risk of Rain 2 | Hopoo, then Gearbox | 1-4 | Playable (text) | DLC Aug 2024 (54.5% positive) and Nov 2025 (92.6%) | n/f | Drizzle: about 30 min to stage 6 [S] |
| Megabonk | solo developer / Unity | solo | Verified (#4 most played on Deck) | updates slowed after Feb 2026 | Next Fest demo (low-trust source) | HLTB ~4 h main [S] |

---

## 2. What separates the ones that lasted from the ones that faded

| Pattern | Lasted (evidence) | Faded (evidence) | For SKYGRIND [U] |
|---|---|---|---|
| **Dated, large content drops** | ULTRAKILL: every layer sets a new ceiling, and its quiet-month floor rose from ~1,700 to 2,554. Gunfire Reborn: seasons plus DLC; its Sep 2024 and Oct 2025 DLC months peaked at 19,126 and 14,596. Witchfire spikes to ~4,000 on each update | Sker Ritual: no map after Oct 2024, 23 now. Roboquest ended updates (May 2025): 189. Deadlink and Ziggurat 2: studios moved on, under 15 | EA cadence: a map or mode every 8-12 weeks |
| **Cadence can't save a disliked core** | - | KF3: each season bumps players, and they fall back within ~6 weeks; recent reviews still 40%. Mycopunk patches weekly yet fell from 1,262 to 89 | Fix feel and clarity before adding content |
| **Horde / rogue modes as revival** | Boltgun Horde (+57% peak); Echo Point Nova Rogue Mode and Severed Steel Rogue rework (all-time peaks); Gunfire Reborn's Spiritual Assault; Cultic's Survival Mode | Single-run campaigns with no mode (Neon White, Anger Foot, Turbo Overkill) all decayed after launch | SKYGRIND's Survival is the mode others add. Later modes: an endless arena, a boss rush |
| **Difficulty ladder** | ULTRAKILL 5 tiers; Gunfire Reincarnation 1-10; Roboquest Guardian I-IV; DRG: Survivor Hazard 1-5; RoR2 Eclipse; Witchfire Torments | Cultic: "to change the difficulty you have to start a new game"; Hyper Light Breaker "overshot the challenge" | HEAT 1-3 now, 6-8 in EA, one rule per rung |
| **Price and value** | $10-20 hits: Megabonk, DRG: Survivor, Far Far West, Gunfire Reborn | $40 with thin content: KF3, Toxic Commando, Painkiller. Hyper Light Breaker at $30. Mullet Madjack: "overpriced" after 2.5 h | EA at $14.99-19.99 [U] |
| **Co-op** | The long-lived big ones have 4-player co-op: Gunfire Reborn, RoR2. 2026 hits: Far Far West, Jump Space | Roboquest: "Only 2 player co-op, really?" Mycopunk: "Horrible if playing solo". Deadzone: Rogue: "good luck getting a game" | 4 players when it comes; solo balance must stand on its own |
| **Performance and tech basics** | ULTRAKILL: "runs well on modest hardware", no performance complaints | KF3: shaders take "30min", Deck Unsupported. Anger Foot CPU complaints. Rogue Core "stutter city". Witchfire "serious optimization is needed" | Shader warm-up; hold the bench bar; low-spec test |
| **Monetisation before basics** | ZA4: a complete game, no live service, still at 86% | KF3's battle pass came before text chat; Spectre Divide's $90 skins; Splitgate's $80 bundle; Toxic Commando's "FOMO battle pass"; CoD's "shopping mall" menus | Premium, no MTX (already the plan) |
| **The first hour** | Deadzone: Rogue: 85.4% finish the first mission | FBC: Firebreak: players "leave within the first hour". RoR2: "no tutorial, overwhelming". Turbo Overkill: 71.7% finish level 1, 49.9% level 3. Anger Foot: 40.3% beat the first boss | First-run script; easy first rounds; the first unlock fast |
| **EA length** | Length doesn't decide it: ULTRAKILL has been in EA 6 years and grows; Gunfire Reborn took 18 months | Deadzone: Rogue's 3.5-month EA, then a sequel announcement, brought backlash. Synthetik 2: 5 years in EA, 80.6%. GDCo: the median EA lasts 1 y 3 mo, and only 21% earned more at 1.0 than at EA launch | Treat the EA launch as the big revenue moment; never let it look abandoned |
| **Build variety vs stat sticks** | Roboquest: weapon mods feel "vastly different". ULTRAKILL's freshness rule rewards switching weapons | Rogue Core: "just stat modifiers". Deadzone: +5% / +10% is "mobile game design". DRG: Survivor: random upgrades "prevent you from upgrading a weapon fully" | Variety-only unlocks (the web's rule) |
| **Trust** | Roboquest ended updates openly ("no way we can continue to provide content for players with over 1000 hours") and kept 95% | Deadzone: Rogue's sequel review; AI-art complaints (Gunfire Reborn's recent 83%, Sker Ritual, Deadzone: Rogue, BO7) | No AI art; a public roadmap |

---

## 3. What players praise and hate, per game

Quotes come from the Steam review API (English; `filter=recent` or `all`, `review_type=positive|negative`; URL pattern
in section 8). Reddit could not be fetched, so there are no Reddit quotes. The quotes are near-verbatim excerpts.

**Sker Ritual: what a Zombies-like gets wrong** [W appreviews/1492070]
- **Praise:** "Cod zombies but better maybe?"; "beautiful and unique verity of monsters and bosses"; "great 20 dollar
  pickup"; "Brings me back to the OG zombies days" (Steam forum).
- **Hate:** the most-voted negative (50 votes): "forced mouse acceleration" makes "accurate aiming nearly impossible";
  "gunplay is lackluster, nothing has impact"; "bullet reg is non existent"; "pale imitation of CoD zombies" (21
  votes); "map layout is like a maze" with "no minimap"; "struggles significantly with guiding the player"; "de-sync
  city"; "No save system, Game crashes. Lose hours of playtime."; "blatantly used AI for its gods".
- **Critic** (PSU 7.5, [W]): upgrades "too expensive by tier 3", "bullet sponges", "consistent frame-rate issues".
- **The lesson:** CoD fans forgive a small amount of content, but not bad aim feel, unreadable maps, desync or AI art.

**Killing Floor 3** [W appreviews/1430190]
- **Hate** (all-time most voted): "feels like an early access mobile game" (742 votes); "no character customization"
  (621); "adding text chat… calling it a major update" (375); "to join a game, walk your virtual @$$ to computer"
  (225); "UI and matchmaking are an absolute downgrade" (190). Recent: "takes legit 30min compile shaders"; "Why are
  there a million kinds of damage?"; "Rng Gachaslop upgrade system". At launch (TheGamer [W]): "more important to have
  day one functional paid battle pass than a text chat".
- **Praise:** "making matches way more hectic and fun" (193 votes, after Deep Freeze cut Zed health and raised spawn
  rates); "Gunplay feels on point and less floaty than 2"; "movement mechanics like sliding and dashing".
- **Recovery is partial:** recent reviews are at 40%. Its "Long" mode (10 rounds and a boss) came from the top-voted
  survey item, "Longer Matches" [W].

**Black Ops 6 / 7 Zombies** [W appreviews/3606480, 1938090]
- **Praise:** "Zombies mode is the only thing keeping this game from being a total disaster"; "builds on BO6 zombies
  strengths"; "I love how they broke it up into modes"; "Cursed Mode… for more experienced zombies fans".
- **Hate:** "Every menu feels like a shopping mall"; "CoD HQ is pointless"; "another 90ish GB for zombies"; "The real
  issue is the Relics system"; AI-made calling cards.

**Roboquest: what made an FPS roguelite last** [W appreviews/692890]
- **Praise:** "Buttery smooth movement, great meta progression"; "Borderlands' wacky guns with DOOM-like combat"; "online
  co-op is really smooth"; weapon modifiers that feel "vastly different".
- **Hate:** "Only 2 player co-op, really?"; "Final boss only doable after 20 hours of grinding meta progression";
  "Locking gadgets behind non-combat gameplay"; "30 minute runs would be much better"; "No chat in co-op".

**Gunfire Reborn** [W appreviews/1217060]
- **Praise:** "constantly shown love and updates with new weapons"; "years of free updates"; "favorite to play with
  friends"; "70+ weapons, 200+ upgrades"; "easy to learn hard to master".
- **Hate:** "catastrophic item/skill description"; "translation is really, really bad"; "Undisclosed AI generated art";
  "3rd boss ruins whole game"; "matchmaking is broken". Players also find its hero-specific "Ascension" jargon
  confusing [S].

**ULTRAKILL: what makes a movement shooter stick** [W appreviews/1229490]
- **Praise:** "Snappy, fast, precise, and extremely rewarding"; "you can constantly find new ways to kill things";
  "p ranked every level, got on the leaderboard on the cyber grind and then grind cybergrind"; "the amount of freedom
  the game gives you is just crazy".
- **Hate** (rare): "holy visual clutter"; "Fraud is so HORRENDOUSLY balanced"; "the gameplay is VERY fast". Of 100
  recent negatives, 0 are about performance or motion sickness and 4 about balance.

**Neon White** [W appreviews/1533420]
- **Praise:** "The learning curve in this game is pretty addictive"; "tightly designed"; "movement feels smooth and
  intuitive".
- **Hate:** "game for speedrunners. I am not a speedrunner"; "way too much gift grinding for some dialogue slop"; "AWFUL
  CRT filter" with no toggle. Story and dialogue come up in 14 or more of 20 recent negatives.

**Others, one line each** [W]
- **Witchfire:** "Destiny gunplay feels so good" vs "difficulty comes from being underlevelled… tedious", "serious
  optimization is needed".
- **Deadzone: Rogue:** "gunplay and movement is spot on" vs "support is already dead…", "add a restart button".
- **Mycopunk:** praised for its upgrade grid vs "co-op without the co-op", "Horrible if playing solo".
- **Turbo Overkill:** "you can literally grapple hook your own rockets" vs "each level taking an hour or more", "vomit of
  colors".
- **Mullet Madjack:** "10 seconds to live" vs "finished it in 2.5 hours", "reuse of same levels".
- **Severed Steel:** "such a movement high" vs "Difficulty curve is all over the place".
- **Boltgun:** "movement and hits have weight" vs "Spongy enemies, boring same looking maps".
- **Anger Foot:** the group's performance complaints: "brought my 5090 to its knees", "bad cpu performance".
- **Echo Point Nova:** "best feeling movement… since Titanfall 2" vs "no timer or ranking system".
- **DRG: Survivor:** "quick 20 minute session" vs random upgrades blocking builds.
- **Megabonk:** "Insanely addictive" vs "the owner decided to… just ignore it".

**Performance and optimisation, across the table** [W]
- **Complaints:** KF3 (shader compiles, Deck Unsupported), Anger Foot (CPU), Witchfire, DRG: Rogue Core ("stutter
  city"), Sker Ritual ("frame-rate issues"), Trepang2 ("Drops mouse inputs"), Zombie Army 4 ("horrid frame drops").
- **No complaints:** ULTRAKILL, Neon White, Severed Steel.

---

## 4. How the player-facing game is organised

| Game | From boot to play | Run setup | Hub and meta | Session | What reviewers say about clarity |
|---|---|---|---|---|---|
| ULTRAKILL | menu → chapter select (average rank per layer) → level | difficulty screen: 5 tiers, each with a one-line description; STANDARD marked "recommended" for first-timers | level select with ranks; Cyber Grind (per-difficulty leaderboards, unlockable start waves); sandbox | levels 3-15 min | praised; assists marked with a blue background, not hidden |
| Neon White | mission board → level, one-key restart | none | medals (Bronze → Ace, hidden Red); information revealed step by step (friend times and ghost before global boards) | levels ~30 s | CRT filter and HUD with no toggles |
| Risk of Rain 2 | lobby | survivor, loadout (skills and skins) and difficulty on **one screen** | challenges unlock survivors and skills; logbook | Drizzle ~30 min to stage 6 [S] | "no tutorial", "can't toggle item descriptions" |
| DRG: Survivor | menu → run setup | class → class mod → biome → hazard (hazard last); 3-4 hazard-unlock challenges shown on the hazard screen [W] | classes unlock by rank (1 / 3 / 5 / 7) | ~20 min | menus fine; complaints aim at random upgrades |
| Roboquest | Basecamp hub | class + difficulty (Discovery → Guardian IV) | Basecamp upgrades for Wrenches; gadget unlocks through quests; Endless (solo save / resume) | 20-40 min | stat-heavy unlocks; non-combat quest gates |
| Gunfire Reborn | hero-select hub | hero + difficulty (Normal → Reincarnation 10); 8 optional "Bizarre Dream" toggles; daily challenges [S] | talent tree (Soul Essence); seasons; Spiritual Assault (a horde mode, unlocked at level 60) | Spiritual Assault: 30 min | jargon, poor wording, "difficulty scaling backwards" |
| Witchfire | Hermitorium hub, 10+ stations | expedition chosen at the Cliff of Sacrifice; Torments (8 + Alpha) | 4+ currencies; a garden that regrows in real time (15-30 h) | n/f | depth praised; "tedious" grind |
| Killing Floor 3 | walk your character to a terminal to join a match | specialist (locked to a perk until Oct 2025); 7 maps; 3 difficulties | battle pass; seasons | Standard ~5 waves (n/v); Long = 10 + a boss | "walk… to computer", "UI and matchmaking… downgrade", "million kinds of damage" |
| CoD BO6 / BO7 Zombies | CoD HQ launcher → game → Zombies → mode (Standard / Directed / Cursed) | augments and GobbleGums before the match; Relics as modifiers (BO7) | camos, prestige, store | sessions of hours ("4 hours straight") | "shopping mall" menus; launcher; 90 GB install |
| Sker Ritual | menu → map | map; custom games (Jul 2025) | "Miracle" perks from gods; "SUPER CHARGE" as its Pack-a-Punch; Sker Pass; paid menu backgrounds | n/f; no mid-run save | "struggles… guiding the player"; no minimap |

**What SKYGRIND's screens should look like** (proposals [U], each tied to the rows above)

1. **Title.** A first focus of **PLAY**, which starts Survival with the last setup in one press. Then SURVIVAL (setup),
   DAILY, CONTRACTS, MASTERY, RECORDS, SETTINGS, QUIT.
   - Not a walk-around hub (KF3's 225-vote complaint), not a launcher (CoD HQ; Steam's Deck guidance), no store tab.
   - The Game Accessibility Guidelines say a game should start "without the need to navigate through multiple levels of
     menus" [W].
2. **Run setup on one screen**, as in Risk of Rain 2's lobby, in DRG: Survivor's order with the hard choice last:
   1. map (in the demo, other maps shown locked: "in the full game")
   2. HEAT rung, each with a one-line rule, the base rung marked RECOMMENDED (ULTRAKILL's difficulty screen)
   3. the DAILY card (today's twist and whether today's counted try is used)
   4. a seed field folded under ADVANCED
   5. START
   - Show each locked rung's unlock condition on that screen, as DRG: Survivor does with its hazard challenges.
3. **Meta screens: at most three** (CONTRACTS, MASTERY, RECORDS), one currency or none.
   - Witchfire's 10 stations and 4 currencies read as grind; Gunfire Reborn's jargon is its top clarity complaint.
   - Every MOD, perk and contract in one plain line of text.
4. **In the run:** guidance to the next objective (Sker's "no minimap" complaint), instant restart (Neon White), and
   save-and-quit (Sker; Roboquest's Endless).
5. **RESULTS:** a time / kills / style breakdown as ULTRAKILL's rank does, then the PAYOFF strip and RETRY SAME SEED.
   Mark assisted runs instead of hiding them (ULTRAKILL).
6. **Onboarding:** teach inside the first round, as ULTRAKILL's 0-1 does (skippable on replay). Top demos had "no
   tutorials… or just a popup dialog that listed the controls" [W Zukowski]. SKYGRIND's first-run script (one lesson
   per round) fits this.
7. **Text size:** Xbox's accessibility guideline is 18 px at 1080p on PC [W]; Steam Deck needs text at least 9 px tall
   at 1280x800 [W]. CLAUDE.md's floor is 16 px. Whether to raise it is GG's call; the evidence points to 18 px.

---

## 5. Demo and Next Fest data

**Two corrections to earlier research** [W Zukowski]:
- The Bronze 7 / Silver 18 / Gold 38 / Diamond 65 minute tiers come from 2022 data (130 games), not Feb 2026.
- "18 minutes" is the Silver tier, not a median of all demos. In the same post, linear games' demos ran 14 min and
  replayable games' 18.5 min.

**Steam's dates and rules** [W partner.steamgames.com]

| Fest | Registration deadline | Demo live for press | Fest | Eligibility |
|---|---|---|---|---|
| Feb 2027 | **2027-01-10** | submit by Jan 25; press preview from Feb 11 | **Feb 22 - Mar 1** | release after Mar 1, 2027 |
| Jun 2027 | **2027-04-25** | submit by May 17; press preview from Jun 3 | **Jun 14 - 21** | release after the fest |

- The store page must be public. A game appears in **one** Next Fest only, and a demo of a game already on Steam can't
  enter. The demo is its own App ID with its own reviews.
- The wishlist notification email can be sent **once**, within two weeks of the first playable release.
- Steam recommends turning achievements off in demos. Shared cloud saves let progress carry over to the full game.

**Benchmarks** [W unless tagged]
- **Momentum beats the total** (June 2026): wishlists gained in the two weeks before the fest predict the result with
  Spearman ρ 0.81, against 0.76 for the base; games under 1k wishlists were "drowned out". Target 2,000+ before it.
- **Demo-to-wishlist conversion:** median 16.3% (Feb 2026), 19.3% (Jun 2026); "Do not make gameplay changes based on
  this data". The median Feb 2026 demo added ~11 followers and ~200 wishlists; the top 5% ~350 followers (~7k) [W GDCo].
- **Launch:** 7,000-10,000 wishlists reaches Popular Upcoming. First-week sales per wishlist: median 0.15x (0.10x over
  $10); Early Access 0.16 against 0.22 for full releases [W GDCo]. The Midnight Walkers (a zombie extraction shooter)
  converted 1.3% of 87k wishlists [W Alinea].
- **Keep the demo up:** after launch, games with a demo averaged an 89.83 review score against 84.22 without; pulling
  it is "like dipping your hook in the water then yanking it out". Put wishlist buttons on the title and quit screens.

**Genre cases** [W unless tagged]

| Game | Demo | Result |
|---|---|---|
| ULTRAKILL | Aug 28, 2019: the Prelude, 1-1, two secret levels; **no Cyber Grind, sandbox or cheats**; ended on a boss-arena tease | EA a year later; wishlists n/f |
| Sker Ritual | Jun 2022 Next Fest: **one map**, 3 guns, perks, buyable doors; "fairly fun and manageable playing alone" | EA Oct 2022; its 1.0 was the big moment (5,992 peak) |
| Anger Foot | Jun 2022 Next Fest: the first 11 levels, one boss, half the shoes | launched 2 years later |
| Mullet Madjack | Jan 24, 2024 demo (95% of 154 reviews); Feb 2024 Next Fest [S] | "560k wishlists after 6 months" [S, n/v] |
| Deadzone: Rogue | Oct 2024 Next Fest; "over 1,000 reviews in the demo" [S] | 150k+ wishlists before EA [S]; 100k+ players in week 1 |
| Sulfur | 2 demos (Mar and Jun 2024), plus Oct 2024 Next Fest | 160k wishlists at launch; 29k of its first 52k units bought by wishlisters; spikes after streamers |
| Far Far West | open playtest Oct-Nov 2025, then the Feb 2026 Next Fest | 435k wishlists before the fest, +101k during it; about 19% of wishlisters played the demo |
| Toxic Commando | Feb 2026 Next Fest (#8); the demo peaked at 5,831 [S] | 8,684 at launch, 176 now |
| SPRAWL zero | Jun 2026: the first 2 levels (30-45 min); a mid-fest update added mantling | n/f |
| FREERUNNERS (parkour) | an 8-10 min demo; median playtime 6 min | wishlists went from 420 to 755 |

**What a horde / movement demo should show** [W sources in brackets; the application is [U]]
- Into a round within seconds; controls as a popup at most [Zukowski 2024]. The first parry and the first style-rank
  pop well inside 10 minutes.
- An easy first round with a rigged good roll: "err on the side of easier", early synergies that feel great [GMTK].
- **One map, runs uncapped:** gate the depth instead (fewer perks, FORGE tier 1 only, other maps shown locked).
  Targets: a 30-90 min median (GMTK); Steam Page Analyzer suggests "20-30 min per run… 1 full loop"; How Many Dudes'
  uncapped demo had a 2 h 02 min median.
- Hold back the endless mode as ULTRAKILL did with the Cyber Grind. End on an exfil, a teaser and a wishlist screen.
- Deck-ready for the fest: default controller config, matching glyphs, legible text, 30 fps at 800p, no launcher
  [Steam]. Test on a weak PC [presskit.gg].
- Solo demo: co-op demos have "the lowest play-to-wishlist ratio" [Zukowski].

---

## 6. The last 12 months (Oct 2025 - Oct 2026)

All [W] unless tagged.
- **2025-10-21 Painkiller** ($39.99): 55.0%, peak 214. A co-op formula on a single-player brand.
- **2025-10-30 ARC Raiders** ($39.99): 81.7% of 417,948 (recent 54%); peak 465,097 (Jan 2026); 1.5M units in October.
  GameDiscoverCo credits its "sane social interactions" with random players.
- **2025-11-14 Black Ops 7:** the lowest year-end US ranking for a CoD since 2008 [W Wikipedia]. Zombies was its bright
  spot.
- **2025-11-18 RoR2: Alloyed Collective**: 92.6% positive, which redeemed Seekers of the Storm's 54.5%. Peak 43.6k.
- **2025-12-17 Splitgate: Arena Reloaded** relaunch: a 2,261 peak, falling to 38.
- **2026-01-26 Highguard:** about 97k peak, down 90% within a week, shut down 2026-03-12.
- **Feb 2026 Next Fest:** shooters at the top: Marathon #1 (143k CCU), Far Far West #5, Toxic Commando #8.
- **2026-03-05 Marathon** ($39.99): 76.5% of 64,434; peak 77,358; about 1,923 in Sep (down ~94%).
- **2026-03-12 Toxic Commando** ($39.99): 73.3%; 8,684 → 176. "always online", "lacking content".
- **2026-04-15 Blood Reaver** (EA, $14.99, a round-based Zombies-like with 4-player co-op): 83.8% of 198.
- **2026-04-28 Far Far West** (EA, $19.99): 95.4% of 38,389; peak 47,031; over 500k in under 2 weeks, 1.3M by early
  June. It is the team's third version of the game; they credit procedural "possibility space" and open development.
- **2026-05-12 Deadzone: Rogue 2** announced (demo 2026-10-08), which triggered the "support is already dead" review.
- **2026-05-20 DRG: Rogue Core** (EA, $29.99): 60.2% of 15,544. Complaints: price, stutter, stat upgrades.
- **2026-06 Next Fest:** EMPULSE (a 6v6 movement shooter from the Splitgate team) #5 most played. Venn counted 6
  shooters and 4 roguelites in the top 50.
- **2026-06-24 EMPULSE** ($9.99): 60.0%. Development ended 2026-08-26.
- **2026-10-20** Mycopunk 1.0 and **2026-10-29** Milo's EA (a co-op wave game) are coming [W store].
- **Still growing:** ULTRAKILL (69,799 in Feb 2026, then a price rise) and Sulfur (2026 average about double 2025's).

**What hit and what flopped, and why** [W GDCo; the synthesis is U]
- **Hit:** co-op PvE run games at $10-20, built in the open with playtests and Next Fest demos (Far Far West, Jump
  Space, Megabonk), and solo games that pay out in short sessions (Megabonk, DRG: Survivor). GameDiscoverCo's
  "sessionability" thesis: short runs, choices inside each run, a meta to grind.
- **Flopped:** $40 horde shooters with thin content (KF3, Toxic Commando, Painkiller); EA priced like a 1.0 (Rogue
  Core); free-to-play PvP from new IP (Spectre Divide, Splitgate 2, Highguard, EMPULSE), each killed by too few new
  players and server costs; a bad first hour (FBC: Firebreak).
- **Genre data** [W GDCo]: Roguelite is now the #1 tag among hit PC games (up 39 places since 2021); "co-op" appears in
  2.1% of new game descriptions (2025, from 1.4% in 2023); the median demo-peak to launch-peak CCU ratio is x3.01;
  52.2% of Feb 2026 Next Fest demos were Unity.

---

## 7. Ranked recommendations for SKYGRIND

Impact (Imp) and effort (Eff) are H / M / L [U]. "Stage" is the demo or Early Access.

| # | Recommendation | Evidence (section) | SKYGRIND hook | Imp / Eff | Stage |
|---|---|---|---|---|---|
| 1 | **Protect feel and readability first:** raw mouse input (no acceleration), honest hit registration, guidance to the next objective, a readable map | Sker's top negatives, KF3's, ULTRAKILL's praise (0, 3) | the exact Mover; a guidance light (first-run script) | H / M | demo |
| 2 | **No hitches:** shader warm-up at boot or load, the 6.9 ms worst-1% bar, a low-spec test PC | KF3's shaders and Deck Unsupported; Anger Foot; Rogue Core (2, 3) | the bench; D-34 | H / M | demo |
| 3 | **One-press PLAY and a one-screen run setup;** at most 3 meta screens; one plain line per item | RoR2's lobby; KF3's terminal; CoD's "shopping mall"; Gunfire's wording (4) | title, run setup, CONTRACTS, MASTERY | H / S-M | demo |
| 4 | **Plan the Next Fest backwards** (decision D-06, due Nov 1): pick **June 2027** unless a public demo and store page can be up by mid-December 2026 with 2,000+ wishlists climbing | one fest per game; early demos earned 2.5x; momentum ρ 0.81 (5) | U6 (Steamworks) | H / M | demo |
| 5 | **Demo content:** one map, uncapped runs, HEAT 1 and the DAILY; FORGE tier 1; other maps and the endless mode shown locked; end on exfil plus a wishlist screen; wishlist buttons on title, quit and RESULTS; solo | Sker's one-map demo; ULTRAKILL's held-back Cyber Grind; GMTK; Zukowski (5) | the run setup, the PAYOFF strip | H / M | demo |
| 6 | **Aim for Steam Deck Verified:** controller defaults, glyphs, legible text (raise the floor to 18 px at 1080p?), 30 fps at 800p | all movement shooters Verified; Sker / Roboquest / RoR2 lost it on text (1b, 4) | the 16 px rule (GG); input | M / M | demo |
| 7 | **Price Early Access at $14.99-19.99;** no MTX or pass; raise the price at 1.0 as ULTRAKILL and Mycopunk do | the median hit $16.99; $40 flops; Rogue Core (0, 2, 6) | the store page | H / S | EA |
| 8 | **A dated, public EA cadence:** a map, mode or HEAT rungs every 8-12 weeks; never announce a sequel during EA | ULTRAKILL's peaks; Gunfire's seasons; Witchfire's spikes; Deadzone's backlash (2) | the retention plan's #11 | H / L | EA |
| 9 | **Extend the ladder:** HEAT to 6-8 rungs with records per rung, assists marked not hidden | every laster has a ladder (2, 4) | HEAT 1-3, the grade | H / M | EA |
| 10 | **Short sessions:** keep the 20-30 min exfil; save-and-quit; maybe a shorter "quick exfil" contract | Roboquest's "30 minute runs"; KF3's "5 too short, 10 too long"; Sker's "no save" (3) | NIGHT13 save-and-quit | M / S | demo |
| 11 | **Variety, not stat sticks:** unlocks add guns, mods, twists; mastery gives finishes | Rogue Core, Deadzone, DRG: Survivor complaints (2) | CONTRACTS, MASTERY (already so) | M / S | both |
| 12 | **Co-op later, for 4 players, with solo tuned on its own** | Roboquest's 2-player cap; Mycopunk's solo complaints; the 2026 co-op hits; co-op demos convert worst (2, 5, 6) | D-02 | H / L | EA |
| 13 | **Later free modes:** an endless arena with boards, a boss rush, a crossover | Boltgun Horde; Echo Point Nova Rogue; Mullet Madjack's boss rush; the Cyber Grind (2) | the deterministic sim, ghosts | M / M | EA |
| 14 | **No AI art, and a visible roadmap** | Gunfire Reborn, Sker Ritual, Deadzone: Rogue, BO7 reviews (3) | GG's art direction; Steam's AI disclosure rules (not checked here) | M / S | both |

**Decisions this raises for GG:** D-06 (which fest); the EA price; the 16 px vs 18 px text floor; whether the demo
carries the DAILY. All are proposals only.

---

## 8. Sources

**Steam endpoints** (all [W], read 2026-10-04; APPID per the list below):
- Reviews: https://store.steampowered.com/appreviews/APPID?json=1&language=all&purchase_type=all&num_per_page=0 (summary) ; https://store.steampowered.com/appreviews/APPID?json=1&language=english&filter=recent&review_type=negative&num_per_page=20 (text; also `review_type=positive`, and `filter=all` with `day_range=365` or `3650`) ; https://store.steampowered.com/appreviewhistogram/APPID?l=english&review_score_preference=0 (1430190, 3606480, 2157830)
- Store: https://store.steampowered.com/app/APPID/ ; https://store.steampowered.com/api/appdetails?appids=APPID ; Deck: https://store.steampowered.com/saleaction/ajaxgetdeckappcompatibilityreport?nAppID=APPID
- Players and owners: https://steamcharts.com/app/APPID ; https://steamspy.com/api.php?request=appdetails&appid=APPID ; https://raijin.gg/app/APPID (third-party estimates, movement shooters)
- News and achievements: https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=APPID ; https://steamcommunity.com/app/APPID/allnews/ ; https://steamcommunity.com/stats/APPID/achievements (692890, 1492070, 1978590, 3228590, 1328350, 1164940)
- **App IDs.** Zombies and horde: Sker Ritual 1492070; KF3 1430190; ZA4 694280; Toxic Commando 2157830; BO7 3606480; CoD HQ 1938090; BO6 2933620; Blood Reaver 2906070; Dead Rush 4275970; Milo 2665020. FPS roguelites: Roboquest 692890; Gunfire Reborn 1217060; Witchfire 3156770; Deadlink 1676130; Deadzone: Rogue 3228590; Mycopunk 3247750; Sulfur 2124120; Ziggurat 2 1159560; Synthetik 528230; Synthetik 2 1471410. Movement: ULTRAKILL 1229490; Neon White 1533420; Turbo Overkill 1328350; Mullet Madjack 2111190; Severed Steel 1227690; Trepang2 1164940; Boltgun 2005010; Anger Foot 1978590; Echo Point Nova 1836730; Cultic 1684930. References: DRG: Survivor 2321470; RoR2 632360 (DLC 2306620, 2781620); Megabonk 3405340. Anti-examples: Spectre Divide 2641470; Splitgate 2918300; Hyper Light Breaker 1534840; FBC: Firebreak 2272540; Painkiller 2300120. Fetched for the trend section (name not recorded): 3124540, 1808500, 2605790, 3065800, 4323990, 3585460, 1597080, 4092940, 1757300.
- Steam's rules and guidance [W]: https://partner.steamgames.com/doc/marketing/upcoming_events/nextfest ; .../nextfest/feb_2027 ; .../nextfest/june_2027 ; https://partner.steamgames.com/doc/store/application/demos ; https://partner.steamgames.com/doc/steamdeck/compat ; https://partner.steamgames.com/doc/steamdeck/recommendations ; https://store.steampowered.com/steam_refunds/

**Market analysis (GameDiscoverCo, Zukowski and others)**
- GameDiscoverCo [W] (https://newsletter.gamediscover.co/p/...): how-that-game-sold-on-steam-using ; how-sker-rituals-10-release-redefined ; how-sulfur-sold-50k-real-fast-and ; how-far-far-west-sold-500k-in-less ; who-won-february-2026s-steam-next ; who-won-june-2026s-steam-next-fest ; who-won-october-2025s-steam-next ; who-won-june-2025s-steam-next-fest ; which-games-hit-big-in-steams-oct ; how-steams-personal-calendar-is-supercharging ; deep-dive-how-steam-followers-and ; steam-the-new-wishlists-to-first ; what-are-steam-decks-biggest-2026 ; does-your-steam-demo-ccu-predict ; how-have-the-genres-of-hit-pc-games ; will-big-walks-1m-sales-usher-in ; may-2026s-top-new-pc-and-console ; what-game-descriptions-tell-us-about ; who-won-steams-best-of-2025-revenue (mostly paywalled) ; what-happened-to-early-access-graduates ; revealed-the-top-new-pc-and-console-58f ; october-2025s-top-new-steam-and-console ; why-sessionability-radically-affects
- How To Market A Game [W] (https://howtomarketagame.com/...): 2026/07/14/games-that-used-momentum-for-steam-next-fest-success/ ; 2026/06/30/nobody-plays-demos-and-that-is-ok/ ; 2026/07/13/did-ai-slop-ruin-steam-next-fest-june-2026/ ; benchmarks/ ; 2022/09/26/how-many-wishlists-should-i-have-when-i-launch-my-game/ ; 2022/10/26/what-is-a-good-median-play-time-for-a-demo-benchmark/ ; 2025/03/26/benchmarks-how-many-wishlists-can-i-get-from-steam-next-fest/ ; 2026/04/13/making-sense-of-the-february-2026-steam-next-fest/ ; 2025/10/20/steam-next-fest-october-2025-checking-in-on-the-games-that-broke-through/ ; 2026/08/20/part-2-the-week-of-the-golden-age/ ; 2026/08/21/part-3-the-week-of-the-golden-age-how-many-dudes/ ; 2023/08/16/frequently-asked-questions-about-steam-next-fest/ ; 2026/02/02/should-you-keep-your-demo-up-post-launch/ ; 2021/05/10/keep-your-free-steam-demo-up-forever/ ; 2024/06/24/do-demos-help-earn-wishlists-steam-next-fest-june-2024/ ; 2025/08/26/the-demo-effect-from-7000-wishlists-to-42000/ ; snf/
- Others [W]: https://gamedevreports.substack.com/p/gamediscoverco-the-state-of-steam ; https://alineaanalytics.substack.com/p/wishlist-to-buyer-conversions-for ; https://gameworldobserver.com/2024/12/06/wishlist-to-sales-ratio-steam-gamediscoverco-benchmark ; https://recognizingpatterns.substack.com/p/far-far-west-didnt-win-steam-next ; https://www.venn-studios.com/research/next-fest-top-50/ ; https://www.gamingonlinux.com/2026/06/these-are-the-most-played-demos-of-steam-next-fest-june-2026/ ; https://gmtk.substack.com/p/how-to-make-a-great-steam-next-fest ; https://www.steampageanalyzer.com/blog/how-long-should-a-steam-demo-be ; https://presskit.gg/field-guides/how-to-build-game-demo ; https://gamedevproducer.com/posts/how-to-plan-a-game-demo-for-steam-next-fest/ ; https://www.cbgamedev.com/blog/freerunners-steam-next-fest-postmortem ; https://llamagriffin.com/Data/Steam%20Next%20Fest%20June%202026%20Report.html ; https://www.gamebrief.net/blog/sprawl-zero-demo-next-fest-2026 ; https://gameaccessibilityguidelines.com/allow-the-game-to-be-started-without-the-need-to-navigate-through-multiple-levels-of-menus/ ; https://learn.microsoft.com/en-us/gaming/accessibility/xbox-accessibility-guidelines/101

**Per-game articles, wikis and posts**
- Zombies and horde [W]: https://media.service.gov.wales/news/sker-ritual-levels-up-tides-of-terror-expands-wales-made-games-universe ; https://www.walesinteractive.com/news/categories/sker-ritual ; https://www.walesinteractive.com/post/sker-ritual-announcement-refreshed-editions ; https://www.walesinteractive.com/post/anniversary-update ; https://www.psu.com/reviews/sker-ritual-review-ps5/ ; https://steamcommunity.com/app/1492070/discussions/0/4352242595079193947/ ; https://www.charlieintel.com/games/sker-ritual-brings-round-based-cod-zombies-back-to-life-320640/ ; https://www.gamespew.com/2022/06/sker-ritual-preview/ ; https://en.wikipedia.org/wiki/Killing_Floor_3 ; https://playfront.de/en/killing-floor-3-sechs-monate-klare-lehren-und-eine-roadmap-fuer-2026/ ; https://game8.co/games/Killing-Floor-3/archives/538846 ; https://themagicrain.com/2026/03/killing-floor-3-releases-third-seasonal-update-operation-deep-freeze/ ; https://www.thegamer.com/killing-floor-3-launch-mixed-reviews-steam-battle-pass-crashes-performance/ ; https://www.gfinityesports.com/article/killing-floor-3-the-big-letdown-why-fans-are-unhappy-with-the-newest-release ; https://en.wikipedia.org/wiki/John_Carpenter%27s_Toxic_Commando ; https://www.focus-entmt.com/en/games/john-carpenters-toxic-commando ; https://en.wikipedia.org/wiki/Call_of_Duty:_Black_Ops_7 ; https://en.wikipedia.org/wiki/Call_of_Duty:_Black_Ops_6 ; https://tech4gamers.com/black-ops-7-steam-player-count-down-black-ops-6/
- FPS roguelites [W]: https://en.wikipedia.org/wiki/Roboquest ; https://en.wikipedia.org/wiki/Gunfire_Reborn ; https://en.wikipedia.org/wiki/Witchfire ; https://rogueliker.com/roboquest-ends-development/ ; https://theastronauts.com/2024/10/witchfire-steam-story/ ; https://gamegrin.com/news/gunfire-reborn-sells-one-million-copies/ ; https://player.one/gunfire-reborn-prepare-incoming-assault-latest-update-152388 ; https://witchfire.wiki.gg/wiki/Calamity ; https://witchfire.wiki.gg/wiki/Torments ; https://witchfire.wiki.gg/wiki/Hermitorium ; https://witchfire.wiki.gg/wiki/The_Revelations_Update ; https://steamcommunity.com/app/692890/discussions/1/4036976070310273415/ ; https://steamcommunity.com/app/692890/discussions/1/3941273339225027921/ ; https://www.pocketgamer.com/deadzone-rogue/available-via-steam-early-access/ ; https://www.gamingonlinux.com/2025/05/fight-off-a-planet-wide-fungal-catastrophe-in-the-upcoming-co-op-shooter-mycopunk/
- Movement shooters [W]: https://www.escapistmagazine.com/news-ultrakills-fraud-layer-update-skyrockets-playerbase-to-near-70k-peak-players/ ; https://games.gg/news/ultrakill-last-sale-before-price-increase/ ; https://en.wikipedia.org/wiki/Ultrakill ; https://ultrakill.wiki.gg/wiki/Difficulty ; https://ultrakill.wiki.gg/wiki/Style ; https://ultrakill.wiki.gg/wiki/Ranking ; https://ultrakill.wiki.gg/wiki/Levels ; https://ultrakill.wiki.gg/wiki/The_Cyber_Grind ; https://ultrakill.wiki.gg/wiki/Options ; https://ultrakill.wiki.gg/wiki/0-1:_Into_the_Fire ; https://ultrakill.wiki.gg/wiki/Demo ; https://en.wikipedia.org/wiki/Neon_White ; https://www.destructoid.com/neon-white-steam-next-fest-demo-pc-impressions/ ; https://www.vice.com/en/article/neon-whites-developers-being-too-good-at-their-own-game-caused-real-problems/ ; https://aiptcomics.com/2022/07/18/neon-white-ben-esposito-interview/ ; https://en.wikipedia.org/wiki/Mullet_Mad_Jack ; https://store.steampowered.com/app/2695350/MULLET_MAD_JACK_DEMO/ ; https://en.wikipedia.org/wiki/Trepang2 ; https://en.wikipedia.org/wiki/Warhammer_40,000:_Boltgun ; https://en.wikipedia.org/wiki/Anger_Foot ; https://www.thegamer.com/anger-foot-preview-devolver-digital/ ; https://en.wikipedia.org/wiki/Cultic_(video_game) ; https://www.pcgamesn.com/echo-point-nova/steam-update ; https://store.steampowered.com/app/2398180/Echo_Point_Nova_Demo/ ; https://steamcommunity.com/app/1328350/discussions/0/3189115678945454613/
- References and anti-examples [W]: https://en.wikipedia.org/wiki/Megabonk ; https://en.wikipedia.org/wiki/Deep_Rock_Galactic:_Survivor ; https://en.wikipedia.org/wiki/Highguard ; https://en.wikipedia.org/wiki/XDefiant ; https://www.gamedeveloper.com/business/mountaintop-studios-shutting-down-after-debut-shooter-spectre-divide-falls-short ; https://80.lv/articles/nate-mitchell-and-matt-hansen-on-the-death-of-spectre-divide-mountaintop-studios ; https://insider-gaming.com/1047-ends-empulse-splitgate-arena-reloaded-development/ ; https://www.indie-games.eu/hyper-light-breaker-failed-heres-why/ ; https://www.gamedeveloper.com/business/remedy-issues-profit-warning-with-fbc-firebreak-treading-water ; https://riskofrain2.wiki.gg/wiki/Difficulty ; https://deeprockgalactic.wiki.gg/wiki/Survivor:Biomes ; https://megabonk.org/guides/maps/ ; https://bonkmaster.com/guide

**Snippet only [S]:**
- GamesRadar, PC Gamer, Kotaku and Game Informer on Splitgate 2, Hyper Light Breaker, Spectre Divide's skins, Seekers of the Storm, Megabonk's sales and Roboquest's ending; Windows Central (Firebreak, headline only); PCGamesN's Painkiller preview; Gamepressure; Metacritic; fandom and gamerant pages on Gunfire Reborn's difficulty and Spiritual Assault and on Roboquest's Basecamp; the HowLongToBeat figures in 1b (via TrueAchievements and others).
- steamdb.info: ULTRAKILL 71,785; Spectre Divide 30,971; Toxic Commando demo 5,831 (https://steamdb.info/app/4354730/).
- https://www.gamesradar.com/games/fps/after-5-years-in-early-access-one-of-steams-best-rated-shooters-goes-on-sale-one-last-time-before-a-price-increase-dont-say-we-didnt-warn-you/ ; https://wnhub.io/news/stores-and-publishing/item-48385 ; https://www.savingcontent.com/2024/01/22/mullet-mad-jack-joins-steam-next-fest-with-first-demo-available-from-february-5th-to-the-12th/ ; https://www.savingcontent.com/2020/08/20/roboquest-a-fast-fps-roguelite-from-ryseup-studios-is-now-available-on-steam-early-access/ ; https://trepang2.fandom.com/wiki/Demo ; https://gameworldobserver.com/2024/10/21/witchfire-sold-enough-astronauts-independent-egs-deal ; https://www.walesinteractive.com/sker-ritual/all-about-sker ; https://www.capsulecomputers.com.au/2024/04/wales-interactive-celebrates-sker-ritual-as-biggest-ever-launch/ ; https://www.gematsu.com/2025/12/john-carpenters-toxic-commando-launches-march-12-2026 ; https://www.steampageanalyzer.com/blog/boxleiter-method-explained

**Blocked or failed:** howlongtobeat.com, old.reddit.com and reddit.com, steamdb.info, gamalytic.com (and its API),
pcgamingwiki.com, igdb.com, the Fandom wikis (402), ign.com, massivelyop.com (403), callofduty.fandom.com.

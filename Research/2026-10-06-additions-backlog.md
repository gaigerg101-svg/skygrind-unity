# Research 2026-10-06: the additions backlog (what proven horde, roguelite and boomer-shooter design adds next)

Written 2026-10-06 (evening) for GG's 8-hour overnight build: "a ton of real changes ... additions, iterations ...
decide based off the data, the game design, what's been proven for game design in making good games and how to iterate
them." Read-only research: nothing in the project was changed except this file.

**How it was made:** a skim of the repo's state (PORT-STATUS.md, tasks.json, HANDOFF.md, NIGHT19-REPORT.md,
DESIGN-PILLARS.md, IDENTITY.md, ZOMBIES-ULTRAKILL-RESEARCH.md, the QUEUE files, NIGHT20.md, the earlier Research docs,
`PortExport/config.json`, `Design/unity-config.json`, `Core/Director.cs`, the web's `src/enemies/`), the NIGHT17 farm
log recomputed, and about 30 web searches (wikis, GDC talks, interviews, press). It builds on the earlier research and
doesn't repeat it: `ZOMBIES-ULTRAKILL-RESEARCH.md` (CoD and ULTRAKILL evidence), `Research/2026-10-03-retention.md`
(retention data), `Research/2026-10-06-weapon-sandbox.md` (gun niches, combos, pursuers).

**Tags:** **[V]** checked in this repo today. **[S]** a cited source (wiki, GDC, interview, press). **[P]** community or
press report. **[U]** seen only in a search summary, not opened. **[rec]** my recommendation. Costs are for one Claude
builder: **S** under 1 h, **M** a 2-4 h block, **L** more than one block (the row says how to split it).

---

## 0. Bottom line

1. **The biggest hole is the boss beat.** The web designed five bosses on a schedule (BULWARK round 5, MONOLITH 10,
   RIVAL PRIME 15, ORBITER 20, HIVE 25; `config.bossKit.schedule`) and `Director.RoundType` already types rounds 5, 10,
   15 ... as `"boss"` [V `Core/Director.cs:182-193`], but no boss is ported, so those rounds play as ordinary rounds
   (the file's header: "a round the web gives other kinds or a boss plays with what exists"). Every comparable has this
   beat (CoD's hellhound and Panzer cadence, KF2's boss wave, Cyber Grind's escalation). BULWARK is a verb fight, not a
   sponge (ram it into a wall, break plates with armour-piercing hits, jump its rings), which is what players reward.
2. **"Nothing can catch you" (GG, NIGHT19) is still measured true:** THE CHASE's KITE bot survives 133.7 s at the
   round-10 roster against a 120 s target (S8 FAIL, NIGHT19-REPORT.md). Two proven fixes, both cheap: make health come
   from aggression (DOOM's push-forward loop, ULTRAKILL's blood; the LAB already has both, D-18 is GG's), and port the
   web's threats that punish straight-line kiting and perching (LANCER, the affixes, then WARDEN).
3. **Without audio from us, text must carry what CoD's announcer and music carry.** One slab at a time for each run beat
   (boss, hunt, last one, exfil, multi-kill) is the cheapest identity and pacing win on the list.
4. **The rhythm should be a heartbeat players can predict and plan for:** a special round every ~5 rounds (the swarm
   round exists; make it CoD's dog-round ritual), a boss on the 5s, an elite between them, and a "NEXT:" line in the
   break (Hades shows what's behind each door).
5. **Finish NIGHT19 before adding guns.** The sandbox's judges still FAIL (S1 4 dominated pairs, S2 COMET 75 %, S7
   1.15), and its fixes sit uncommitted in the builders' worktrees. FORGE tiers as sidegrades and named combos come
   next; a wonder weapon after the POWER projectile kinds exist.
6. **Measure first:** the farm still measures the bot (89 % -> 50 % of runs between rounds 6 and 7, the HOLD round; 0
   exfils in 100 runs; 24 % of earned points spent [V, recomputed from `Logs/farm-n17-b2/farm-normal-1.jsonl`]). The bot
   fixes (NIGHT20 U3-32.5) and a per-round pacing trace are prerequisites for half the "done when" lines below.

**The top 15 (ranked by value / cost, with GG's asks and the demo as tie-breaks):**

| # | Row | Cost |
|---|---|---|
| 1 | B1 Push-forward health as a measured A / B (close-kill heal + hard damage, LAB flags exist) | S |
| 2 | A1 BULWARK on round 5 (the web's boss and boss kit, ported as they are) | L (2 x M) |
| 3 | F1 The announcer: one slab per run beat (no audio, so text carries it) | S-M |
| 4 | C1 Kind debut cards (U3-02's NEW item) | S |
| 5 | F4 The tell floor: every big hit is telegraphed, checked by telemetry | S |
| 6 | A2 THE HUNT: the swarm round becomes CoD's dog-round ritual | S-M |
| 7 | C2 LANCER (the web's sniper: punishes kiting lines and perches) | M |
| 8 | C3 The affixes (VOLATILE, BLINKER, REGENERATING, SWIFT; SHIELDED from NIGHT19) | M |
| 9 | A4 THE ELITE: a named, affixed heavy mid-round every 3-5 rounds | S |
| 10 | B2 Named combos on screen (S4) | M |
| 11 | B3 THE FORGE in tiers, as sidegrades (5k / 15k / 30k) | M |
| 12 | A8 The NEXT line: the next rounds' types in the break | S |
| 13 | F2 SKYGRIND TIME (KF2's Zed Time, rare and earned) | S-M |
| 14 | D1 Trap combos: push or launch an enemy into a live trap | S |
| 15 | E1 The first fifteen minutes v2: teach each verb at its moment | M |

---

## 1. What already exists (the backlog skips these unless a row iterates them)

| Area | In the port today [V] | Gap this backlog uses |
|---|---|---|
| Round types | normal; HOLD 6, 14, 22 ...; GOLD RUSH 7, 11, 19 ...; swarm 8, 13, 18 ... with a banner and a forced power-up (`design.zombies.swarmDrop`); exfil at 11, 16, 21 ... | boss rounds (5, 10, 15 ...) are typed but empty; no elite / mini-boss beat; nothing previews the next round |
| Director | the web's directorMath (golden), tokens, camping / hunters, PEAK FADE (Booth's build-up / peak / fade / relax, `design.director`) | no per-round pacing trace in the farm (round seconds, phase seconds) |
| Enemies | HUSK, SWARMER, SPITTER, CHARGER (NIGHT19, `design.enemies.n19`); DRONE and SHIELDED in a worktree (7 DroneTests red by design) | LANCER, WARDEN, 4 affixes, 5 bosses (all designed in the web: `src/enemies/`, 5,818 LOC) |
| Guns | KESTREL (+ the gust), THUNDERCLAP, PULSE GAUNTLET, STITCHER, COMET, LONGWAVE; ZAPPER / BOUNCER in a worktree | niches / freshness / combos uncommitted; 6 POWER guns; no wonder weapon |
| Upgrades | perks (10 of 14), MODS drafts (10 of 19, offered on rounds 2, 4, 5, 7, 9, 10 ...), THE FORGE (one tier, x1.8; the sidegrade forge is a flag, off), the Mystery Box (CoD's teddy ladder), power-ups (5 kinds) | FORGE tiers; points sink late (the bot spends 24 %) |
| Map | Survival + DRYDOCK's greybox heart; traps (4), the power switch, THE PROTOCOL + its hidden relay; jump pads | NIGHT20 builds the rest of DRYDOCK (gantry hooks, the pier, every buyable); the ARCLINE has no buy (U3-03) |
| Meta | CONTRACTS (39), HEAT 1-3, DAILY, MASTERY, the P grade, PAYOFF, first-run onboarding (cards on rounds 1, 2, 3, 5, 6), recap, tape, ghost, autopilot / farm | onboarding never teaches the parry, the box, HOLD, MODS or exfil (QUEUE-first-fifteen.md, never run) |
| Feel | freezes, chunks, hit springs, camera feel, HUD in three tiers, named callouts and kill streaks (NIGHT17), combo callouts (NIGHT19), round ceremony, low-HP pulse | no run-beat announcer layer; no rare earned slow-mo |

**Pacing facts (the bot, so read as a lower bound) [V, recomputed today from 100 NIGHT17 farm runs]:** mean 76.6 s per
round (inflated by HOLD timeouts), median per run 49.1 s; a MODS pick every ~2 rounds (0.48 per round); 31 % of the
time above the DULL rank; 0 exfils; 24 % of earned points spent. So a decision already arrives every 60-90 s
(Brotato's shop every 20-60 s wave [S]); the backlog adds beats and threats, not more menus.

---

## 2. What the proven games teach (short, with sources)

### 2.1 Round structure and pacing
- **CoD Zombies' special round:** hellhounds first on round 5, 6 or 7, then every 4-7 rounds; the round is announced
  (fog, a voice line), and the last dog always drops a Max Ammo [S: callofdutyzombies.fandom.com/wiki/Hellhound,
  callofduty.fandom.com/wiki/Special_Round]. It is the most remembered rhythm in Zombies; SKYGRIND's swarm round has the
  drop but not the ritual.
- **CoD's mini-bosses between rounds:** Origins' Panzersoldat spawns at the start of round 8, then every 3-5 rounds,
  and never if its area is closed [S: callofduty.fandom.com/wiki/Panzersoldat]. Mob of the Dead's Brutus plays the
  same role. Late-round health-sponge mini-bosses (BO6) are among CoD players' complaints
  [ZOMBIES-ULTRAKILL-RESEARCH.md 2.1 item 4].
- **Left 4 Dead's director:** Build Up, Sustain Peak (3-5 s), Peak Fade, Relax (no wanderers, mobs or specials)
  [S: Booth, GDC 2009; left4dead.fandom.com/wiki/The_Director]. SKYGRIND has this (PEAK FADE); what it lacks is a
  measurement of it per round.
- **Killing Floor 2:** 4 / 7 / 10 waves plus a final boss wave; trader time 90 s on Normal, 60 s above
  [S: wiki.tripwireinteractive.com, Gameplay mechanics (KF2)]. The boss is the run's punctuation.
- **Deep Rock Galactic:** at most two Warnings (harder, raise the hazard bonus: Swarmageddon +20 %) and one Anomaly
  (neutral or helpful) per mission [S: deeprockgalactic.wiki.gg/wiki/Mutators]. Optional Machine Events (a 3-minute
  tower fight) pay an overclock [S: deeprockgalactic.wiki.gg/wiki/Machine_Events].
- **Back 4 Blood:** the director draws Corruption cards per level (1-2 early, 5-6 by Act 2) and players answer with
  their own cards [S: gamedeveloper.com "Behind Back 4 Blood's ingenious card system"; techraptor.net].
- **Risk of Rain 2:** difficulty is a clock (time x difficulty) times 1.15 per stage [S: riskofrain2.wiki.gg/wiki/
  Difficulty]; the Shrine of the Mountain is an opt-in harder boss for +1 item per shrine
  [S: riskofrain2.fandom.com/wiki/Shrine_of_the_Mountain]. Player-chosen risk is a proven intensity dial.
- **ULTRAKILL's Cyber Grind:** waves change the arena's layout; radiant (buffed) enemies from wave 25, a former boss as
  a regular enemy from wave 30 [S: ultrakill.wiki.gg/wiki/The_Cyber_Grind]. Escalate by composition, not HP.

### 2.2 Weapons and upgrades
- **Pack-a-Punch tiers:** BO6 charges 5,000 / 15,000 / 30,000 for three tiers [S: game8.co BO6]; BO3's Alternate Ammo
  Types are procs on a cooldown (Dead Wire 20 % per bullet, 5 s; Blast Furnace 15 %, 15 s; Thunder Wall 10 s)
  [S: callofduty.fandom.com/wiki/Alternate_Ammo_Type]. A tier can add a verb, not only damage.
- **Wonder weapons:** box-only, one per box move; in BO1 a wonder weapon has a 15 % chance to be in the box's list
  after round 5 and 20 % after round 10 [P: gamefaqs BO1 thread; S: callofduty.fandom.com/wiki/Ray_Gun]. Upgrading the
  Der Eisendrache bow reached 15.6 % of players against <= 9.7 % for main quests [ZOMBIES-ULTRAKILL-RESEARCH.md 2.1].
- **DOOM Eternal's loop:** chainsaw for ammo, glory kill for health, flame belch for armour: "push-forward combat"
  [S: gamedeveloper.com "The aggressive resource management of Doom Eternal"]. ULTRAKILL heals from close blood.
- **Vampire Survivors:** evolutions (a maxed weapon + its passive, opened from a chest) are a discovery engine
  [S: vampire.survivors.wiki/w/Evolution]; the chest reveal was built from slot-machine presentation by a developer
  from that industry [S: theconversation.com, 2023]. **Hades:** choose 1 of 3; Chaos gives a curse for a few
  encounters, then a lasting blessing [S: hades.fandom.com/wiki/Chaos/Boons_(Hades)].
- The sidegrade thesis (ULTRAKILL variants, Hades aspects, "stop selling damage") is in
  `Research/2026-10-06-weapon-sandbox.md`; not repeated here.

### 2.3 Enemies
- **Left 4 Dead's specials each counter one behaviour:** the Hunter punishes separation, the Smoker drags you out, the
  Boomer blinds and calls a horde [S: left4dead.fandom.com The Hunter / The Smoker / The Boomer]. Every SKYGRIND kind
  should name the behaviour it punishes (kiting lines, perching, standing still, one-gun play).
- **Gunfire Reborn:** enhanced enemies roll primary and secondary affixes from Elite difficulty; elites always drop a
  weapon, scroll or goblet [S: gunfirereborn.fandom.com/wiki/Difficulty, /Enemies]. Affixes multiply variety cheaply.
- **The Marauder (DOOM Eternal) is the failure case:** Vice called it the game's "biggest indulgence and greatest
  failing", SkillUp "objectively terrible" [S: en.wikipedia.org/wiki/Doom_Eternal]; its director defends it and blames
  its introduction [S: pcgamer.com]; The Dark Ages' successor aims to be "a little less frustrating" [S: pcgamer.com].
  Lesson: favour an answer, never require one; introduce every threat on purpose.
- **DOOM Eternal (tutorials on) stops time on a new demon's first encounter to name its weak point** [U: search
  summary]. That is the KIND debut card U3-02 already asks for.
- **The web already designed the roster this genre asks for** [V `PortExport/config.json`]: an exploder (VOLATILE),
  a blinker (BLINKER), a flyer (DRONE), a sniper (LANCER), a shield (WARDEN, SHIELDED), a charger (CHARGER, ported), a
  leaper (HIVE's queen) and five bosses. Porting them "as they are" is inside CLAUDE.md rule 7; new kinds are GG's.

### 2.4 The map and routes
- **CoD barriers:** 10 points per board, 6 boards a window, capped at 40 points a round, +50 a round to 490
  [S: callofduty.fandom.com/wiki/Points_(Zombies)]. It is early-round income and a reason to stand at a window: the
  opposite of SKYGRIND's pillar 1.
- **Training:** CoD's high-round skill is circling a horde on a route [S: steamcommunity guide "00Swash's Guide to
  Training"]. SKYGRIND's version (kiting) is too safe today (S8); the fix is threats and resources, not slower movement.
- **Secrets:** fewer than 2 % saw CoD's Easter eggs before Cold War; mid-size goals (rituals 26.4 %, bow 15.6 %) reach
  far more [ZOMBIES-ULTRAKILL-RESEARCH.md 2.1]. Roboquest hides keys and secret areas that pay meta crystals
  [S: store.epicgames.com Roboquest tips; roboquest.fandom.com/wiki/Basecamp].

### 2.5 Meta and onboarding
- Hades' God Mode: 20 % damage resistance, +2 % per death, capped at 80 % [S: hades.fandom.com/wiki/God_Mode].
- The retention data (VS's first unlock at 1 minute reached by 97.8 %, RoR2's logbook step 63 %, Hodent's three items,
  DRG's weekly, Spelunky's daily) is in `Research/2026-10-03-retention.md` sections 2-5.

### 2.6 Feel and juice
- **Hit-stop and shake:** Nijman's "Art of Screenshake" (a ~0.2 s sleep on a hit "gives the brain an opportunity to
  process") and Jonasson / Purho's "Juice it or lose it" (GDC 2012) [S]. SKYGRIND has these (NIGHT16-18).
- **KF2's Zed Time:** the world at 20 % speed for 3 s; 2.5-5 % per kill (more for close kills, headshots and explosive
  multi-kills), x2 after 30 s without one, x4 after 60 s; a kill during it resets it to 3 s
  [S: wiki.killingfloor2.com Mechanics; pcgamesn.com "made it in at the last minute"]. Rare, earned, and the series'
  signature.
- **Announcers:** Unreal Tournament's multi-kill ladder (DOUBLE, MULTI, MEGA, ULTRA, MONSTER KILL within 4 s of each
  other) and sprees at 5 / 10 / 15 / 20 / 25 kills [S: unreal.fandom.com Achievements_and_awards; strategywiki UT2004
  Terminology]. CoD voices every power-up and round change. SKYGRIND has no audio from us (rule 7), so the text slab is
  the announcer.

### 2.7 What has been shown to fail (apply to every row)
1. **Forced solutions** (the Marauder): every threat keeps at least two answers, checked by THE RANGE (every gun within
   2.5x the best on that enemy's cell).
2. **Damage sponges:** The Division's bosses with "a ridiculous amount of health"; The Division 2 promised "reasonably
   sized health bars" and new tanky enemies still drew complaints after Warlords [S: gamesradar.com, forbes.com 2020];
   BO6's mini-bosses [ZOMBIES-ULTRAKILL-RESEARCH.md]. Bosses here are windows and verbs, with a time-to-kill band.
3. **Replacing the round loop** (Vanguard's objective-only Zombies) [ZOMBIES-ULTRAKILL-RESEARCH.md 2.1].
4. **Systems pile-up** (BO6 "FAR TOO MUCH"): one new verb per feature; the onboarding teaches one per round.
5. **Currencies and chores** (BO6 salvage and rarity): points stay the only currency.
6. **Self-sourced heals get exploited** (ULTRAKILL's self-parry nerf): any heal row tests its cap.
7. **Freshness on damage** reads as inconsistent (ULTRAKILL players): NIGHT19's freshness moves it to points only.
8. **A rare effect made common stops being special** (KF2 ramps Zed Time's odds only after 30-60 s without one).

---

## 3. The ranked backlog

**Prerequisites (not ranked; do them first, they make the "done when" lines below honest):**

| ID | What | Why | Done when | Cost |
|---|---|---|---|---|
| P0-1 | Resume NIGHT19's worktrees: W (niches, ZAPPER / BOUNCER, freshness on points, AltUse, combos first pass) and E (DRONE, SHIELDED); fix the suite | uncommitted work rots; S1-S8 still FAIL | the suite green (1182 / 1192 at the close; 7 DroneTests red by design, 3 to fix); S1-S8 re-printed by `SimHarness range / chase / pfarm` | M |
| P0-2 | The bot tells the truth (NIGHT20 U3-32.5; QUEUE-first-fifteen U3-28.3): HOLD by the game's own ring test, shopping by route time, guns by damage at range, exfil called | the farm's round-6 cliff (89 % -> 50 %) and 0 exfils are the bot's; every farm "done when" needs this | 0 HOLD timeouts with the bot standing in the ring; >= 1 exfil call in each run that reaches round 11; `SimHarness newmap ... PASS` | M |
| P0-3 | A pacing trace: the farm writes seconds by round, the director's seconds in BUILD / PEAK / FADE / RELAX, and downs by round type; `farm-report.cjs` prints them | Booth's curve exists in code but isn't measured; rows A1-A8 are judged on it | a farm report with a pacing table (median seconds per round by type, RELAX share per round, downs per round type) | S |

**Columns:** Rank (global) | Name | Proven source | What the player sees and does | Why (**P**acing, **V**ariety,
**M**astery, **R**etention) | Done when (a judge number, a reel / scanner metric or a test) | Cost | Deps | Risk.
Standing for every row (CLAUDE.md): a design flag whose off state is tonight's game (the fingerprint EQUAL, golden 0
mismatches, 22 replays exact); new gameplay numbers in `Design/unity-config.json` with their why; randomness from the
run's Mulberry32; enemy kinds ported as the web has them.

### (a) Round structure and pacing

| Rank | Name | Proven source | Sees and does | Why | Done when | Cost | Deps | Risk |
|---|---|---|---|---|---|---|---|---|
| 2 | **A1 BULWARK on round 5** (port `bulwark.js` 659 LOC + `bossKit.js` 131 + `bossMath.js` 94 as they are) | CoD's boss / mini-boss cadence (hellhounds R5-7, Panzersoldat R8 + every 3-5); KF2's boss wave; anti-sponge lessons (The Division, BO6) | Round 5's card "BULWARK / ARMORED WALKER", a boss bar. Its RAM fills a DANGER line for 1.2 s: sidestep so it hits a wall, it's stunned 3 s with its back vent open. Plates take x0.1 until broken; COMET, LONGWAVE or any hit with momentum > 0.6 deal x3. STOMP rings to jump or dash over, a SWEEP BEAM to ride over. At 50 %: six husks from hatches, double rams. Standing still 4 s near one spot: it rams at once. "BULWARK DOWN", the web's reward table. | P: fills the empty round-5 slot (rounds 5 / 10 / 15 are typed "boss" and play normal today); M: dodge-into-wall and armour-piercing choices; R: the demo needs a boss (DESIGN-PILLARS D-38) | (1) bossMath golden vectors (bossForRound, bossScale) from a new port-export, 0 mismatches; (2) a BULWARK session test (wall ram -> 3 s stun; plate x0.1; AP x3; passivity ram at 4 s); (3) a new `SimHarness boss` judge, 10 seeds at round-5 HP: KESTREL-only median kill 60-150 s and a KESTREL-only win rate >= 80 % (no hard counter); with COMET or LONGWAVE >= 20 % faster (AP matters); STAND bot rammed within 4.5 s; (4) reel `boss5`: the ram line on screen >= 1.0 s before contact, the bar's text >= 18 px; (5) a smoke step "round 5 boss down" | L: block 1 = Core + kit + judge (M), block 2 = view, arena pen, bar, card (M) | an arena spot on each map (the web's Grinder pen on Survival; DRYDOCK needs one: NIGHT20); P0-2 (the bot must dodge rams or the farm stops at 5); CHARGER's ram / stun code to share | the pen must live in the static collider (no runtime collider edits); rule 7: built as the web's, GG can veto |
| 6 | **A2 THE HUNT: the swarm round as CoD's dog-round ritual** (iterates `design.zombies.swarmDrop`) | CoD hellhound rounds (fog + warning, spawns around you, last kill drops Max Ammo); DRG's Swarmageddon; L4D's horde | In the break before rounds 8, 13, 18: a 3 s count and the sky and fog shift toward a DANGER-tinted dusk; the slab "THE HUNT". The squads arrive through beams in a ring around the player (not out of view), one kind per hunt; "LAST ONE"; the drop (exists). | P: a predictable 5-round heartbeat with the boss on the 5s; V | a test: hunt spawns within the configured ring radius +/- 1 m and never inside the min distance; reel `hunt-open`: sky luma / hue shift above a threshold within the first 3 s (telemetry), the slab within 2 frames; farm (after P0-2): hunt rounds last <= 0.6x the median normal round of the same band and cause 10-25 % of downs in runs that reach them (a threat, not a wall) | S-M | P0-2, P0-3; F1 for the slab | spawning near a fast player: keep `spawner.telegraphTime` and a min distance (new numbers in `design.zombies.hunt`) |
| 9 | **A4 THE ELITE: a named mini-boss between bosses** | Panzersoldat / Brutus (R8, then every 3-5); Gunfire Reborn's elites (guaranteed drop); Cyber Grind's radiance | Mid-round from round 8 every 3-5 rounds (never on boss or hunt rounds): "ELITE INBOUND"; one CHARGER or WARDEN with two affixes, a name plate and a small bar; its kill always drops a power-up from the run's stream. | P: a peak inside the long middle (rounds 8-20); R: a reward you can see coming | a schedule test (first >= 8, gaps 3-5, seeded, skips boss / hunt rounds); farm: the elite is killed in >= 70 % of the rounds it appears and causes <= 15 % of downs; fingerprint EQUAL off | S | C3, C2 or CHARGER (exists), F1 | it's cadence over the web's kinds, not a new kind; GG can veto (rule 7) |
| 12 | **A8 The NEXT line** | Hades' doors show the next chamber's reward; L4D's build-up; CoD's round number as the clock | In the break, under the round's hero number: "NEXT: BOSS", "HUNT IN 2", "HOLD", "EXFIL OPEN" (<= 3 words), and five pips for the next rounds' types. | P: anticipation and planning (buy AP before BULWARK); M | a test: the line equals `Director.RoundType` for rounds +1..+5 over 50 seeds; scanner: <= 3 words, >= 18 px, outside the central 40 % (S14), shown only in breaks | S | none | spoils CoD's surprise; the hunt still has its build-up |
| 16 | **A3 THE BEACON: opt-in risk for a reward** | RoR2's Shrine of the Mountain; DRG's Machine Events; Hades' Chaos (pay now, gain later) | In some breaks a beacon stands at a seeded spot. Hold BUY: the next round adds an elite squad; clear it and pick a reward (a free MODS pick, a FORGE discount, points). Ignore it and nothing changes. | P: the player sets the intensity; M; R: a decision in the break | a test: opt-in only, the added squads = config, seeded spot; farm (bot takes it at random half the time): beacon rounds' downs <= 1.5x normal, the reward taken in >= 60 % of taken beacons; fingerprint EQUAL off | M | A4 / C3 (the harder content); F3 (the reveal) | pile-up: it reuses hold-BUY, the MODS draft and the slab, no new verb |
| 22 | **A5 Round twists from round 9** | DRG's Warnings / Anomalies (<= 2 + 1, a hazard bonus); B4B's Corruption cards; Cyber Grind | Every 4th ordinary round from 9 draws one twist named in the break: FOG, SWARMAGEDDON (a swarmer trickle), LETHAL (enemy damage and points x1.5), GOLDEN (drops x2). Each from systems that exist. | V inside a run; P in the middle rounds | a test: a seeded draw, never twice in a row, off on HEAT / DAILY twist rounds; farm: twist rounds' downs within 0.5-2x normal; the NEXT line and the slab show it | M | A8, F1 | pile-up (BO6); new numbers in `design.twists` |
| 26 | **A6 MONOLITH on round 10** (port `boss.js` 695 LOC) | as A1; Cyber Grind's layout change | A 12 m stone construct rising from THE GRINDER: slams with jumpable rings, 8-orb fans to parry into its core (200), kneels after every 3rd slam, the arena reshapes at 66 %, a floor that sinks at 33 %. | P, R (the second boss: the demo's ladder) | as A1 with its own session test and `SimHarness boss --boss monolith`; parried orbs >= 1 per kill in the judge's skilled run | L (2 x M) | A1's kit; the Grinder's reshape needs collider work | runtime geometry under the exact collider: the hardest port on the list |
| 27 | **A7 The round mix (composition budget)** | ULTRAKILL's Cyber Grind (composition and radiance, not HP; its leaderboard dwarfs DUSK's horde: ZOMBIES-ULTRAKILL-RESEARCH.md #6) | From round 10 the director's budget buys kinds, a repeat kind costs more, so rounds read as mixes, not more of the same. | V, P | from round 10, >= 3 kinds in 90 % of rounds (farm); directorMath golden 0 mismatches with the flag off | M | C2, C3, C4 | the director's golden; numbers are GG's to approve |

### (b) Weapons and upgrades

| Rank | Name | Proven source | Sees and does | Why | Done when | Cost | Deps | Risk |
|---|---|---|---|---|---|---|---|---|
| 1 | **B1 Push-forward health, as a measured A / B for GG** (LAB's CLOSE-KILL HEAL and ULTRA HEALTH, D-18) | DOOM's glory-kill health ("push-forward combat"); ULTRAKILL's blood heal and hard damage | HP comes back from close kills (blood within 4 m) and parries; a hard-damage segment on the vitals plate refills slowly; running away heals nothing. | M; fixes S8 from the resource side ("nothing can catch you"); fits pillar 2 | THE CHASE: S8 KITE survives <= 120 s at round 10 and <= 60 s at 15 (now 133.7 / 67.1); S9 FIGHT takes <= 50 % of STAND's damage per minute; THE PARITY FARM's median round within +/- 10 % of the flag off (not just harder); a test that no self-sourced heal passes the cap; fingerprint EQUAL off | S | none (THE CHASE and the LAB flags exist) | D-18 is GG's: ship it as the A / B he decides on, not a default |
| 10 | **B2 Named combos on screen** (finishes W's first pass) | ULTRAKILL (core eject + shot, airshots, conductor); DOOM Eternal's combat chess; `Research/2026-10-06-weapon-sandbox.md` 11-16 | POP (a KESTREL hit on your live rocket or grenade: 2x radius), LAUNCH + SNIPE (x2 on airborne), CONDUCTOR, GATHER + BURST, IGNITE; each lands with its slab and a style award. | M, V; GG: "it's just which one you prefer, and combos" | S4: >= 5 named combos, every reachable gun in >= 1, each >= 20 % better than the best single gun in its cell (THE RANGE pair mode); S7 1.3-2.0; reel `combo`: the slab within 2 frames | M | P0-1 | S1 / S2 regressions; pad aim on a moving rocket |
| 11 | **B3 THE FORGE in tiers, as sidegrades** | BO6 Pack-a-Punch 5k / 15k / 30k; BO3 Alternate Ammo Types (procs on cooldowns); ULTRAKILL variants; Hades aspects | FORGE I (5,000): the gun's alt-fire variant (weapons.js remainder: KESTREL pierce, THUNDERCLAP cone wave, compress + double blast). FORGE II (15,000): its family's status as a proc on a cooldown (shock, burn, pull, stagger: `forge.families`). FORGE III (30,000): a finish and a small flat mult, the same for every gun. | M, V; a points sink (the bot spends 24 % of what it earns); no gun falls off | S3: each gun at its tier within 1.6x the best in C7; S1 0 dominated pairs with forged guns; farm (after P0-2): points spent >= 50 % by round 15; tier-gate tests; fingerprint EQUAL off | M | P0-1 (the sidegrade forge's first pass), U3-14.5 variants | new numbers (`design.forge.tiers`); keep x1.4 flat so the forge stays a sidegrade |
| 19 | **B4 A wonder weapon** (box-only) | CoD's Ray Gun / Thundergun (one per box move; 15 % after round 5, 20 % after 10); the bow upgrade's 15.6 % | The box's jackpot: a box-only gun with a verb nobody else has (proposal: the web's VORTEX gravity well as its base) and a 3-step build to its upgraded form; a "WONDER" slab. | R (clips, the box's peak), M (a mid-size quest players finish) | a box test over 100k seeded pulls: 0 before round 5, 15 % +/- 1 pt rounds 5-9, 20 % +/- 1 pt from 10, never twice a box move; THE RANGE: best in the crowd cell but best in <= 35 % of cells (S2 holds); farm: owners' kill share <= 45 %; the quest completed by a scripted tape | L (2 x M) | U3-14.3 (vortex projectiles), B3 | becoming the one gun (NIGHT19's thesis); its name and look are GG's |
| 25 | **B5 TORCH and VORTEX** (the POWER guns the sandbox research keeps) | the web's guns.js; ULTRAKILL's Firestarter; `Research/2026-10-06-weapon-sandbox.md` 9 | TORCH coats and denies an area; VORTEX gathers a crowd. Both feed combos (IGNITE, GATHER + BURST). | V, M | S2: each is best or within 10 % of best in >= 2 cells; S4: IGNITE and GATHER + BURST pay >= 20 % | M each | U3-14.3, B2 | box pool dilution; the hotbar's slots |

### (c) Enemies (the web's kinds, ported as they are; new kinds are proposals for GG)

| Rank | Name | Proven source | Sees and does | Why | Done when | Cost | Deps | Risk |
|---|---|---|---|---|---|---|---|---|
| 4 | **C1 Kind debut cards** (U3-02's NEW item) | DOOM Eternal stops on a new demon to name its weak point [U]; the Marauder's lesson (introduce a threat on purpose); Hodent's one-at-a-time | The first time a kind or affix appears in a profile: a 1.2 s card, its name and one counter ("CHARGER: SIDESTEP THE LINE"), the world slowed on the Game layer; once per profile; never during a boss intro or with an enemy within 8 m. | P (each new kind becomes an event: swarmer 3, drone 4, spitter 6, charger 7, lancer 8, warden 9, affixes 8); onboarding | a test: once per kind per profile, gated as written; reel: <= 4 words, display text >= 32 px; a new-profile run to round 9 shows each card once; slowed time <= 1.5 s per card | S | the kinds (C2-C4) as they land | interrupting a fight: the 8 m gate |
| 7 | **C2 LANCER** (port `lancer.js` 186 LOC) | L4D's specials each punish one behaviour; ULTRAKILL's Virtue-style trailing marker (weapon-sandbox 4) | A thin aim beam from a perch that tracks you with a 0.45 s lag for 1.2 s, then fires (25); a dash, slide, jump or cover breaks it; it blinks to a new perch. From round 8, at most 6. | punishes straight-line kiting and perching (S8); gives KESTREL / LONGWAVE a long-range job (S2) | a session test on the web's numbers; THE CHASE at the round-10 roster with lancers: KITE takes >= 2x FIGHT-with-dashes' lancer damage per minute; reel: the beam on screen >= 1.0 s before every hit; THE RANGE: a "lancer at 40 m" cell where KESTREL or LONGWAVE is best | M | P0-1 (the kinds' plumbing CHARGER added); perch points per map | snipers frustrate in horde games: keep the web's cap and the tell |
| 8 | **C3 The affixes** (port `affixes.js` 240 LOC: VOLATILE, BLINKER, REGENERATING, SWIFT; SHIELDED from E) | Gunfire Reborn's affixed elites; ULTRAKILL's radiance; DRG | Pixel icons over heads from round 8 (20 % +2 % a round to 50 %, two affixes from 16 at 35 %, +40 points): VOLATILE bursts 3.5 m after 0.4 s on death, BLINKER jumps 5 m every 3.5 s, REGENERATING heals unless finished, SWIFT x1.35. | V (5 affixes x 6 kinds); M (target priority); S8 (SWIFT catches kiters) | a roll test against the config on the run's affix stream; reel: icons >= the legibility floor at 20 m; THE CHASE: SWIFT husks lower KITE's survival; farm: affixed kills 15-35 % from round 12 | M | P0-1 (SHIELDED) | VOLATILE vs the close-kill heal: an intended tension, with its tell |
| 20 | **C4 WARDEN** (port `warden.js` 161 LOC) | the Marauder lesson (many answers); DOOM's shield soldiers | A slow shield-bearer (120 deg front, a shield hit staggers you 1 s, a 0.8 s swing tell): flank it, launch it, parry its swing or pierce it. From round 9, at most 6. | M (positioning), V | THE RANGE warden cell: every reachable gun clears within 2.5x the best (no hard counter); a flanking bot clears >= 30 % faster than a frontal one; the tell floor holds | M | P0-1 | a stun chain that feels like the Marauder: cap stuns per 5 s |
| 29 | **C5 A climber, as a proposal for GG only** | L4D's Smoker / Jockey (deny separation); perches are today's kiting escape | A one-page proposal: an enemy that scales walls to deny perches, its counters and the CHASE number it targets. Nothing built. | answers "nothing can catch you" from above | the proposal written with its target metric; GG decides | S | none | rule 7: enemy design is GG's |

**Genre wish-list mapped to the web's designs:** exploder = VOLATILE; leaper = HIVE's queen (and navJump); shielded =
WARDEN / SHIELDED; flyer = DRONE (and ORBITER); sniper = LANCER; climber = none (C5).

### (d) The map and routes

| Rank | Name | Proven source | Sees and does | Why | Done when | Cost | Deps | Risk |
|---|---|---|---|---|---|---|---|---|
| 14 | **D1 Trap combos** | CoD's traps (a paid kill zone); environment-kill rewards in skill-shot shooters [rec] | Gust, COMET-launch or punch an enemy into a live SHOCK PLATE, CRUSHER, GRAVITY FAN or ARC PYLON: a named combo ("SENT") with style and points, credited to you. | M; gives the gust (GG's NIGHT19 ask) a job beyond escape | a test: an enemy pushed by you that dies in a trap within 2 s is credited to you; THE RANGE trap cell: push-into-trap clears >= 20 % faster than the best gun alone; S10 unchanged | S | traps placed on the map (exist); the gust | traps need the power on (fine) |
| 18 | **D2 The ARCLINE buy and the gantry's hooks** (U3-03) | the web's ARCLINE (swingMath exact in the Mover); grapple routes in movement shooters | A gadget on THE GANTRY's wall (the web's 5,500); crane hooks as nodes; swing routes and the enemy pull. | pillar 1 (movement is the moat); new routes | `SimHarness verbs`: a swing between two hooks measured; swingMath golden 0 mismatches, 16 / 16 replays exact; a smoke step that buys and swings | M | NIGHT20's GANTRY | rule 3: the rope is already in the Mover; no Mover change |
| 21 | **D3 A secrets ladder: five SIGNAL SHARDS** | ULTRAKILL's secrets; CoD's mid-size goals (15-26 %) beat main quests (<= 9.7 %); Roboquest's keys and secret areas | Five shards on DRYDOCK, each reachable only with a measured move (a wall-ride chain, a gust-jump, a swing); all five in one run open a reward (a free FORGE tier or a finish); kept in the profile. | R (discovery, talk), M | `SimHarness verbs`: each shard reachable by its move and not by walking or a plain jump; 0 shards visible from the spawn; profile and contract tests | M | NIGHT20's map, D2 | the map and the secret content are GG's to approve |
| 30 | **D4 Zone shift on boss rounds** | Cyber Grind's per-wave layout; the web's MONOLITH reshape | Part of a zone rises or sinks for a boss round. | V | the validator clean in both states; replays exact | L | A6 | runtime collider changes |
| 31 | **D5 Spawn vents (CoD barriers, adapted)** | CoD barriers: 10 points a board, capped per round | Seal the vents a round's squads enter by for points. | early income | not recommended unless GG asks | M | the director's spawn rules | it asks you to stand still (pillar 1); conflicts with out-of-view spawning |

### (e) The meta and onboarding

| Rank | Name | Proven source | Sees and does | Why | Done when | Cost | Deps | Risk |
|---|---|---|---|---|---|---|---|---|
| 15 | **E1 The first fifteen minutes v2** (QUEUE-first-fifteen U3-28.6, never run) | VS's first unlock at 1 min (97.8 %); Hodent; BO6's Directed Mode welcomed | Glyph + verb prompts at the moment of need, gone on use: PARRY when the first glob flies, BOX at the first 950 points, HOLD when the ring appears, MODS at the first draft, EXFIL at the radio; a first contract unlock inside 5 minutes. | R (day-1 retention); pillar 3 | a new-profile tape of rounds 1-7: each of 8 verbs prompted within 10 s of first need and cleared on first use (an event test); S12 (pad glyphs); <= 8 words; the first unlock <= 5 min in >= 90 % of 20 new-profile autopilot seeds | M | P0-2 | prompt spam: one at a time, after C1's cards |
| 23 | **E2 ASSIST (Hades' God Mode)** | Hades: 20 % damage resistance, +2 % a death, cap 80 % | SETTINGS > ASSIST: resistance that grows with each down; runs stamped (no records, like the LAB). | R (players who would quit); Steam Deck / pad players | tests (stamp, growth, cap); farm with ASSIST: median round >= +3 vs off; RESULTS shows the stamp | S | none | GG may prefer HEAT only; new numbers |
| 24 | **E3 A CODEX tab** | RoR2's logbook (63 % reach its 10-log step), Hades' and DOOM's codex | A tab inside WEAPONS (the meta stays at three screens): enemies met (with their debut card), combos found, shards found; locks as silhouettes. | R (collection), M | profile tests; scanner M9 / M10; the meta screen count unchanged | S-M | C1, B2, D3 | screen creep |
| 28 | **E4 A WEEKLY seed** | DRG's weekly Deep Dive; Spelunky's daily | One seed a week with a fixed HEAT rung and twist, counted once, beside the DAILY. | R | a seeds test (one per ISO week); RESULTS' badge | S | DAILY (exists) | thin without Steam boards (U6) |

### (f) Feel and juice

| Rank | Name | Proven source | Sees and does | Why | Done when | Cost | Deps | Risk |
|---|---|---|---|---|---|---|---|---|
| 3 | **F1 The announcer: one slab per run beat** | UT's multi-kill ladder (4 s window) and sprees; CoD's voiced round and power-up beats; ULTRAKILL's callouts | One slab at a time on the 12 deg slant, <= 3 words, with a priority queue: BOSS INBOUND, THE HUNT, ELITE INBOUND, LAST ONE, EXFIL OPEN, BULWARK DOWN, DOUBLE POINTS, MONSTER KILL. Reuses NIGHT17's callout and slam components. | P (beats read), identity, clips | a table of run events -> slabs (event names stay the web's, for GG's audio map); scanner on a scripted reel: each listed event's slab within 2 frames, 0 overlaps with the vitals and points plates, M9 0 pop-ins, M10 0 texts under 18 px; a strings test (<= 3 words) | S-M | none | "too much" (NIGHT17 reviewers): at most one slab per 1.5 s except boss beats |
| 5 | **F4 The tell floor** | the web's telegraph language; ULTRAKILL's attack flashes; the Marauder lesson | Every enemy hit of 15 or more is preceded by a DANGER tell (line, circle, glow) for >= 0.4 s, or its projectile is on screen >= 0.25 s. Violations go to GG as a list (his enemy design), not changed. | fairness: deaths read as the player's fault; WHAT GOT YOU shows the tell | a telemetry check over 10 seeded reel runs to round 10: 0 hits >= 15 without a tell for >= 0.4 s; the list of exceptions written | S | FeelProbe events | the web's husk swing may be shorter: report, don't tune |
| 13 | **F2 SKYGRIND TIME** | KF2's Zed Time (20 % speed, 3 s, 2.5-5 % per kill, ramps after 30 / 60 s) | Rarely, on a 3+ kill blast or a headshot kill at BOLD or above, the world drops to about 25 % for ~1.5 s (the look stays full speed), the screen leans toward INK, a slab. | crunch (GG: kills not crunchy); clip moments | telemetry over 10 seeded runs: 0.3-1.0 per combat minute, none within 20 s of another; reel: the time scale reaches its target within 1 frame and returns within 150 ms; `SimHarness impact` PASS (the freeze budget); SLOW-MO OFF disables it; fingerprint EQUAL | S-M | NIGHT16's time scale and SLOW-MO setting | motion comfort (the setting); co-op later (pitfall: hitstop in co-op) |
| 17 | **F3 The reward reveal** | VS's chest (slot-machine presentation); Hades' 1 of 3; CoD's box reveal (exists) | Boss, elite, hunt and beacon rewards after the round clears: a 0.8 s three-slot reveal, pick 1 of 3 (a power-up, a MODS pick, points). | R (reward salience), crunch | scanner: the reveal <= 1.0 s, M9, >= 18 px; a test: the three offers from the run's stream; pad navigation (S12) | S | A1 / A2 / A3 / A4 | slowing the break: skippable with one press |

---

## 4. The iteration loop for an 8-hour AI-built night (short)

Grounded in Valve's "designs are hypotheses, playtests are experiments" (Ambinder, GDC 2009), Cerny's "publishable
first playable" (DICE 2002), and this project's own record (NIGHT17-RETRO: "implemented was counted as felt";
NIGHT19: builders stopped by the spend limit with work uncommitted).

1. **0:00-0:30, baseline:** the gates, then the judges on the anchor build (`SimHarness range / chase / pfarm`, a farm
   with P0-3's pacing table, the reel). Every slice's "done when" compares against these numbers, never against memory.
2. **Pick 3-4 vertical slices, not 15 features.** A slice is player-visible end to end (rule, view, HUD, slab, test,
   judge) behind one flag. Write its judge before its code. Suggested slices from this backlog: (1) B1 + F4 + F1
   (threats and beats; small), (2) A1 BULWARK in two blocks, (3) C2 + C3 + C1 + A4 (the roster and its cards), (4) A2 +
   A8 + F3 (the rhythm).
3. **A playable Player build every 60-90 minutes** with the smoke run alone, the slice's judge and a reel. Each build
   ends in one of three calls, logged: keep (the number moved), iterate (one more round, with what to change), cut
   (flag off, the reason logged). The first slice is end to end by hour 2.
4. **Cut rule:** a slice that misses its judge after two iterations, or runs past 1.5x its box, is cut, not polished.
5. **Commit every green step at once** (tests-first commits red, then green), one feature per commit, with `.meta`
   files; builders by file ownership in worktrees, the lead alone drives the Editor and git (memory notes).
6. **Judge what players see by pairs:** two fresh blind reviewers per visual change, both orders (NIGHT18's method); a
   self-score never counts.
7. **Freeze 90 minutes before the end:** the final build, the gates (golden, replays, fingerprint, smoke, suite), the
   judges, a one-page report of measured numbers only. Keep a ranked spare-time list (this backlog's next rows) so a
   finished slice never leaves the night idle (NIGHT17 idled 75 minutes).

---

## 5. Sources

**Games and design (opened or summarised by search; [U] where only a summary was seen):**
- Left 4 Dead's director: [The Director (wiki)](https://left4dead.fandom.com/wiki/The_Director); Booth, "The AI
  Systems of Left 4 Dead", GDC 2009 ([text](https://www.readkong.com/page/the-ai-systems-of-left-4-dead-michael-booth-valve-9664541));
  [GDC Vault: From Counter-Strike to Left 4 Dead](https://gdcvault.com/play/1422/From-COUNTER-STRIKE-to-LEFT)
- L4D specials: [The Hunter](https://left4dead.fandom.com/wiki/The_Hunter), [The Smoker](https://left4dead.fandom.com/wiki/The_Smoker),
  [The Boomer](https://left4dead.fandom.com/wiki/The_Boomer)
- CoD Zombies: [Hellhound](https://callofdutyzombies.fandom.com/wiki/Hellhound), [Special Round](https://callofduty.fandom.com/wiki/Special_Round),
  [codzombiesguides: Hellhound](https://www.codzombiesguides.com/bestiary/hellhound/), [Panzersoldat](https://callofduty.fandom.com/wiki/Panzersoldat),
  [codzombiesguides: Panzersoldat](https://www.codzombiesguides.com/bestiary/panzersoldat/), [Pack-a-Punch Machine](https://callofduty.fandom.com/wiki/Pack-a-Punch_Machine),
  [game8: BO6 rarity vs Pack-a-Punch](https://game8.co/games/Call-of-Duty-Black-Ops-6/archives/472213),
  [Alternate Ammo Type](https://callofduty.fandom.com/wiki/Alternate_Ammo_Type), [Points (Zombies)](https://callofduty.fandom.com/wiki/Points_(Zombies)),
  [Ray Gun](https://callofduty.fandom.com/wiki/Ray_Gun), [gamefaqs: BO1 box odds](https://gamefaqs.gamespot.com/boards/960188-call-of-duty-black-ops/59529565) [P],
  [Steam guide: Training](https://steamcommunity.com/sharedfiles/filedetails/?id=666657246) [P]
- Killing Floor 2: [Mechanics (Zed Time)](https://wiki.killingfloor2.com/index.php?title=Mechanics_(Killing_Floor_2)),
  [PCGamesN on Zed Time](https://www.pcgamesn.com/killing-floor-2/killing-floor-2-zed-time-trigger),
  [Gameplay mechanics (waves, trader)](https://wiki.tripwireinteractive.com/index.php?title=Gameplay_mechanics_(Killing_Floor_2))
- Deep Rock Galactic: [Mutators](https://deeprockgalactic.wiki.gg/wiki/Mutators), [Machine Events](https://deeprockgalactic.wiki.gg/wiki/Machine_Events)
- Risk of Rain 2: [Difficulty](https://riskofrain2.wiki.gg/wiki/Difficulty), [Shrine of the Mountain](https://riskofrain2.fandom.com/wiki/Shrine_of_the_Mountain)
- Brotato: [Waves](https://brotato.wiki.spellsandguns.com/Waves)
- Vampire Survivors: [Evolution](https://vampire.survivors.wiki/w/Evolution), [The Conversation: gambling psychology](https://theconversation.com/vampire-survivors-how-developers-used-gambling-psychology-to-create-a-bafta-winning-game-203613)
- Hades: [Chaos boons](https://hades.fandom.com/wiki/Chaos/Boons_(Hades)), [God Mode](https://hades.fandom.com/wiki/God_Mode)
- ULTRAKILL: [The Cyber Grind](https://ultrakill.wiki.gg/wiki/The_Cyber_Grind)
- DOOM Eternal: [Game Developer: the aggressive resource management](https://www.gamedeveloper.com/design/the-aggressive-resource-management-of-i-doom-eternal-i-),
  [Wikipedia (Marauder reception)](https://en.wikipedia.org/wiki/Doom_Eternal), [PC Gamer: the director on the Marauder](https://www.pcgamer.com/doom-eternals-director-says-the-marauder-is-good-actually/),
  [PC Gamer: The Dark Ages' "new Marauder"](https://www.pcgamer.com/games/fps/doom-the-dark-ages-already-sneakily-revealed-its-new-marauder-and-the-devs-hope-hell-be-just-as-challenging-but-a-little-less-frustrating/);
  the new-demon tutorial pause [U: search summary only]
- Back 4 Blood: [Game Developer: the card system](https://www.gamedeveloper.com/design/behind-back-4-bloods-ingenious-card-system),
  [TechRaptor: Corruption cards](https://techraptor.net/gaming/guides/back-4-blood-corruption-cards-guide)
- Gunfire Reborn: [Difficulty](https://gunfirereborn.fandom.com/wiki/Difficulty), [Enemies](https://gunfirereborn.fandom.com/wiki/Enemies)
- Roboquest: [Epic Games Store: tips and secrets](https://store.epicgames.com/news/roboquest-become-a-robot-legend-with-these-tips-and-secrets?lang=en-US),
  [Basecamp](https://roboquest.fandom.com/wiki/Basecamp)
- Bullet sponges: [GamesRadar: The Division 2 director](https://www.gamesradar.com/the-division-2-director-explains-how-the-sequel-fixed-the-bullet-sponge-problem/),
  [Forbes: Division 2 after Warlords](https://www.forbes.com/sites/paultassi/2020/03/09/the-division-2-is-facing-old-foes-post-warlords-bullet-sponges-and-bad-loot/)
- Announcers: [Unreal: achievements and awards](https://unreal.fandom.com/wiki/Achievements_and_awards), [StrategyWiki: UT2004 terminology](https://strategywiki.org/wiki/Unreal_Tournament_2004/Terminology)
- Juice: [GDC Vault: Juice It or Lose It](https://www.gdcvault.com/play/1016487/juice-it-or-lose), [The Art of Screenshake](https://archive.org/details/the-art-of-screenshake)
- Process: [GDC Vault: Valve's Approach to Playtesting](https://www.gdcvault.com/play/1566/Valve-s-Approach-to-Playtesting)
  ([slides](https://media.gdcvault.com/gdc09/slides/GDC09_Ambinder_ValvesApproach.pdf)); Cerny's Method (DICE 2002;
  [Mark Cerny](https://en.wikipedia.org/wiki/Mark_Cerny))

**This repo [V]:** `PortExport/config.json` (`bossKit`, `bulwark`, `boss`, `lancer`, `warden`, `affixes`,
`director.rounds`, `forge`, `mods.draftRounds`); `Design/unity-config.json` (`design.zombies`, `design.enemies`,
`design.forge`, `design.freshness`); `Assets/Scripts/Core/Director.cs` (lines 1-20, 182-193); the web's
`src/enemies/*.js` line counts and headers (read only); `Logs/farm-n17-b2/farm-normal-1.jsonl` (100 runs, recomputed);
`NIGHT19-REPORT.md` section 2; `HANDOFF.md`; `ZOMBIES-ULTRAKILL-RESEARCH.md` 2.1 and 4-5;
`Research/2026-10-03-retention.md` 5; `Research/2026-10-06-weapon-sandbox.md`; `NIGHT17-RETRO.md` 2-3; `NIGHT20.md`;
`QUEUE-first-fifteen.md`; `tasks.json` (U3-02, U3-03, U3-14).

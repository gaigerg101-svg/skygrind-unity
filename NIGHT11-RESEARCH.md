# NIGHT11-RESEARCH.md: the run's shape (what to build next, and the evidence)

GG, 2026-10-03, after NIGHT10 closed: "great you decide on next wave but lets resrecah and come up with next detailed
prompt". Before that (2026-10-03, morning): "want this game to be like cod zombies meets ultrakill as the main, but
other games should be inspired". This file is the evidence behind `NIGHT11.md` (untracked). It builds on
`ZOMBIES-ULTRAKILL-RESEARCH.md` (sections 2.1 and 2.2 already hold the CoD and ULTRAKILL basics) and adds what the next
wave needs: our own code first, then two focused research passes.

**Tags:** [V] verified in the code or a command on 2026-10-03; [W] web source (URL in the sources list); [U] not
verified. Team material: a Drive search for "SKYGRIND" found nothing (2026-10-03); the repos are the material.

## 0. Our data first (what exists, what the web already built)

**The finding that changes the plan:** the frozen web game already has most of "the run's shape", and none of it is
ported. ZOMBIES-ULTRAKILL-RESEARCH section 3 lists "special round", "secrets" and "an ending" as missing because it
looked at the Unity build; the web has them [V]:

| Web module | Lines | What it does | Unity |
|---|---|---|---|
| `survival/objectives.js` | 136 | **Objective rounds** ("Zombies' special rounds; CoD's hardpoint"): HOLD (stand in a 4 m ring for 30 s in all; the round can't clear until it's held; points x1.5 inside; a bonus of 50 x round) and GOLD RUSH (kills pay x2; 3+ kills within 3 s pay 25 x chain). From round 6: HOLD on rounds 6, 14, 22, 26..., GOLD on 7, 11, 19, 27... (`director.rounds.objectiveFrom 6, objectiveEvery 4, holdOffset 2, goldOffset 3`; `config.objectives`). The kind comes from `directorMath.roundType` | not ported; `roundType` is not ported and has no vectors (the directorMath set records isBossRound / isSwarmRound / roundKinds only) |
| `survival/protocol.js` | 191 | **THE PROTOCOL**, the guided main quest (Treyarch: hidden eggs reach under 2% of players, so the steps are on screen): 4 relays on the map's movement showpieces (THE SKYBRIDGE past a 7 m gap, THE CHIMNEY, THE WALL RING, THE PINNACLE); they wake at round 6 (`protocol.wakeRound`); hold BUY to light one (its zone must be open); the next one's beacon stands over the map and the HUD names it; all four lit: a SURGE of elite squads joins the round; clearing it completes the PROTOCOL: every owned gun forged free, +2500 points, a cosmetic finish. A hidden fifth relay (THE ROOF) with no guide | not ported. The spots are in `PortExport/maps/survival.json` (`relaySpots`, `secretRelay`) |
| `survival/traps.js` | 250 | **Four traps**, hold E at a switch: SHOCK PLATE 750 (LANDING), CRUSHER 1000, GRAVITY FAN 900, ARC PYLON 1250 (SLAB GARDENS), 4.8-8 s on, 30-40 s cooldown; a trap kills a regular enemy outright (`traps.kill 99999`), hurts an elite, nicks a boss; each kill pays its points + the TRAPPED style bonus; traps never hurt you | not ported (`trapSpots` in the map export) |
| `modes/runMoments.js` | 90 | **The run's grade and best moments** on RESULTS: a grade S / A / B / C / D from the round (x0.5, full at round 25), the best style rank (x0.3) and accuracy (x0.2), bands S 0.85 / A 0.7 / B 0.55 / C 0.4 (`config.results.grade`); the biggest style award, the longest swing, the top speed, the biggest multi-kill | not ported |
| swarm rounds (`directorMath.isSwarmRound`) | - | all-Swarmer rounds 8, 13, 18, ... (`swarmEvery 5, swarmOffset 3, swarmFrom 8`), never on a boss round | **ported** (Director.cs); no reward at the end (the web has none) |
| boss rounds (`bossKit.schedule`) | - | 5 BULWARK, 10 MONOLITH, 15 RIVAL PRIME, 20 ORBITER, 25 HIVE; rotation from 30 | no boss ported: those rounds "play with what exists" (Director.cs header) |

**Hooks the ports need, checked in the Unity code [V]:**
- `Economy.SetBoost(name, mult)` and `Economy.Grant(amount, reason)` exist (GOLD RUSH, HOLD's points, bonuses).
- `Forge.Give(id, price = 0)` exists (the PROTOCOL's free forge).
- `Buyables` (Director.cs) takes items with `available` / `buy` (relays, trap switches), as the Mystery Box does.
- `Zones.IsOpen(id)`, `Zones.ZoneAt(Vec3)`, `ZoneMath.ZoneIndexAt` exist.
- **Missing:** a hold hook in `SurvivalRun` (a HOLD round must not clear until held: survival.js `holdRound`), a `banner`
  event the HUD shows (the web's protocol / objectives emit `banner {text}`; Ui.cs has a banner text but no listener for
  that event), `Director.AddSquads` for the surge (the director's rule for unported kinds: "plays with what exists").
- The surge's squads are chargers, lancers, wardens and spitters (`protocol.surge`); only the spitter is ported (the
  roster is the research's N13). Tonight's surge would field spitters (and whatever the director's rule fills in).

**Numbers already in the web's config that the run's shape reads [V]:** `survival.breakTime` 10, `firstBreakTime` 3,
`readyFrom` 3; enemies `6 + 3 x round`; HP +8% a round from round 10; `health.max` 100; `regen.delay` 4 s, `rate` 25 HP/s
(rank-scaled); the forge 5000; zones 750-2000; `powerUps` (kill drops; DOUBLE, OVERCLOCK, SHOCKWAVE, STASIS, REFUND).

**NIGHT10's debt [V, NIGHT10-LOG.md]:**
- The Player's managed allocation rose: +103,916 KB over the 60 s bench (16 collections) against +75,672 KB at u3-n9;
  the plans differed (Balanced vs Silent); reading the new code found no allocation; the cause is not measured. A
  Development Player profile (GC Allocated In Frame + the SimStep ProfilerMarkers) settles it.
- `Projectiles.Volley` and the darts' step are stubs (the RATTLER's special needs them, N12).
- A leftover branch `worktree-agent-ac8f3bcfc6870aa0f` (GG's call to delete).

**What GG said before (verbatim lines):** "cod zombies meets ultrakill as the main" (2026-10-03); "everything that was
built on the browser version ... style meter, better UI" (2026-10-02); D-17 ON, D-20 ON, D-18 off "tonight" (NIGHT10's
goal message); "you decide on next wave" (2026-10-03, after NIGHT10).

## A. CoD Zombies: the run's rituals (research pass 1, 2026-10-03)

One web-research agent, about 15 minutes, 70 sources. Reddit and the Fandom wikis blocked the fetcher, so player
reception comes from news coverage of community threads, Steam discussions, reviews and designer interviews.

**1. The power switch (the run's first goal).**
- Classic: one switch a few doors deep turns several systems on at once (perks, traps, teleporters, the way to
  Pack-a-Punch). Kino der Toten: 4 doors, 750 + 1000 + 1250 + 1250 = about 4,250 points; one guide throws it at
  round 3. Der Riese: "after around round 5" [W 5-7].
- After BO1 it split into small quests (Afterlife, Beast Mode, BO6 Terminus's three 500-point generators defended
  for 30 s each) [W 11, 16]. BO6's Liberty Falls has no power and no steps to Pack-a-Punch, and was praised for it:
  "there aren't a million moving parts" (Dexerto); Treyarch: "a really super popular map... very accessible"
  [W 23, 24]. BO7 brings switches back, one per area [W 20].
- Power can start the threat: Cold War's Megatons arrive about 2 rounds after power (round 15 if never); the Origins
  Panzer at round 8, gated on a door; Ascension's monkeys after the first perk [W 26-28].
- **For SKYGRIND:** THE FORGE already works from round 1, as Liberty Falls' Pack-a-Punch does, so power should gate
  something else (the agent's lesson, and ours: the web's traps and THE PROTOCOL are the classic power's systems).

**2. Special rounds.**
- Dog rounds: the first at a random round 5-7, then every 4-6; at most 2 dogs alive per player; fog and "Fetch me
  their souls!" warn of them (BO6: a music sting and green fog); **the last kill always drops a Max Ammo**, and BO6
  names them "Max Ammo rounds" (every 5-7 rounds on its maps) [W 1, 2, 29, 31, 32].
- The backlash is against bullet-sponge elites at high rounds (BO6's Manglers, capped in a patch; BO7 cut specials)
  [W 36-38], not against the special round itself.
- **For SKYGRIND:** the swarm rounds (8, 13, 18...) are its dog rounds already; what they lack is the reward and the
  warning.

**3. Exfil (the ending).**
- Cold War: offered from the end of round 10, then every 5, from a radio at the spawn; reach a site, clear a horde;
  failure shoots the chopper down and ends the match; the reward grows with the round [W 39-42]. The round-935 world
  record was certified by its exfil [W 47].
- BO6: rounds 11, 16, 21...; at round 11 about 1:25 to kill 35 zombies, the demand growing with the round; failure
  ends the run; solo high-round exfil read as a gear check ("very unlikely ... without a mutant injection or
  chopper") [W 43, 44, 48]. Frequent, safe exfils made Cold War feel "too easy" ("make it 10") [W 21].
- **For SKYGRIND:** an opt-in, fixed-interval climax that ends the run either way; scale it with the round; test solo.

**4. Pacing.** WaW's script: 24 alive at most, a 10 s break (SKYGRIND's own), solo counts 6 / 8 / 13 / 18 / 24 over
rounds 1-5 then 33 / 60 / 105 at 10 / 20 / 30, health +100 a round to round 9 then x1.1 [W 50-53]. Expert pace reaches
round 30 in about 29 minutes solo (Kino, BO1) [W 55]. SKYGRIND's own curve (6 + 3 x round, HP +8% from round 10) is
already gentler late.

**5. Pause and save-and-quit.** BO6 (Oct 2024), solo only, one slot: save after round 1, at full health, not while
downed or in an exfil; loading restores the start of the last completed round with weapons and perks; it autosaves
each round; pause capped at 15 minutes [W 59-61]. Coverage: "a feature players have been asking for since 2008"
[W 60]. Brotato resumes at the last shop [W 68].

## B. ULTRAKILL and the horde shooters (research pass 2, 2026-10-03)

One web-research agent, about 11 minutes, 45 sources (the official wiki ultrakill.wiki.gg first; fandom and some
wikis returned search summaries only).

**1. Hard damage.** 35% of each hit on Standard (0% on Harmless / Lenient) lowers the max you can heal to until it
drains. The first hit starts a 1 s timer, each hit adds damage / 20 s; then it drains at 15 / s, 40 / 50 / 60 at the
top three ranks, and is wiped at the top rank. Every heal, the parry included, fills only to max - hard damage
[W S1, S2]. Players split: "a punishment for not playing aggressively enough" vs "practically impossible" on Brutal
[W S7, S8].

**2. Blood heal.** A set amount per attack and hit spot, not a share of the damage: a punch 10 / 20 / 30 (torso / limb
/ weak point), revolver hits likewise, a nail 1, a pellet 3-30, a gib 120; the blood must reach you before it lands;
no published radius; patches fixed "healing range too small" and "can't heal while dashing"; self-parries don't heal;
an explosion's blood doesn't heal a player it hit [W S1, S4, S12-S15]. **ULTRAKILL has no passive regen.** With
SKYGRIND's 25 HP/s after 4 s, hard damage (draining in a second or two) only bites when hits stack; the blood heal is
what pays for staying close during the 4 s delay.

**3. Cyber Grind's budget.** Start 10, + (3 + floor(W / 3)) a wave (W5 28, W10 55, W30 about 245); each kind costs a
base + a surcharge per extra copy in the wave (fodder 0; Mindflayer 55 + 35; the Mirror Reaper's +99,999,999 is "one
per wave"); share rules (uncommons at most half the points, 10% reserved for commons); a full refill each wave
[W S5, S4, S20, S21]. **For SKYGRIND:** it pays off with many kinds; with three ported it has little to price. It
belongs with the roster (N13).

**4. DOOM.** Eternal: glory kills for health, the chainsaw for ammo, the flame belch for armor ("push-forward
combat"); Horde: 3 missions of 5 rounds, 3 extra lives. The Dark Ages: a parry refills melee charges [W S25-S33].

**5. Runs with an ending.** Killing Floor 3: 5 waves + a boss, about 20 minutes; Deep Rock: the race to the pod;
Brotato: 20 waves, 17.5 minutes of combat, an endless mode that keeps the win; Vampire Survivors: 30:00 [W S34-S41].
Next Fest: Steam sets no demo length; Zukowski's October 2022 sample median 18 minutes (top wishlist earners far
longer); Mark Brown: aim for a 30-90 minute median [W S43-S45].

## C. What the halves say together (ranked for SKYGRIND)

1. **Port the web's own run shape before inventing one.** THE PROTOCOL is the classic power quest's arc (a goal, a
   climax, a reward) built on SKYGRIND's movement showpieces; the objective rounds and swarm rounds are its special
   rounds; the traps are the classic power's traps; the grade is the reward screen. They are tuned, tested on the web
   and already in the map data (section 0). PORT-RULES' rule holds: port, don't improve.
2. **An ending: exfil, from round 11, every 5** (BO6's rounds), opt-in at the spawn, a short climax that ends the run
   either way, scaled with the round. It puts a 20-40 minute arc inside a demo (round 11 at SKYGRIND's pace: not
   measured; the smoke clears round 1 in 8 s with an aim bot).
3. **A power switch that turns the web's systems on:** the traps and THE PROTOCOL's relays wake when the power is
   on (classic power's "step change"), not THE FORGE (it stays open from round 1, as Liberty Falls' PaP does). Off by
   flag: the web's rules (traps always available, the PROTOCOL at round 6).
4. **The swarm round's reward and warning:** its last kill drops a power-up (CoD's Max Ammo; SKYGRIND has no ammo, so
   the web's kill-drop roll, forced), with a banner when it starts. The schedule stays the web's (8, 13, 18...).
5. **ULTRAKILL's health as an opt-in for now:** hard damage + the blood heal reverse the web's 3.0 removal of
   close-kill heals and change survival everywhere; GG kept D-18 off. The LAB is where such proposals go (D-03): a
   LAB toggle, off by default, so GG can A/B it in one click.
6. **Later:** the round budget with the roster (N13); save-and-quit (BO6's round-start restore) once the run's state
   is settled (N12 or the Steam night); the POWER guns (N12).

## D. The wave (NIGHT11) and why

GG delegated the choice ("you decide on next wave"). **D-19 decided: the research's order (Survival first, a Next
Fest demo), with section 0's correction:** NIGHT11 is "the run's shape", built from the web's own systems first, then
three small additions behind design flags.

| # | Item | Kind | Why it's here |
|---|---|---|---|
| 1 | Verify on arrival | - | the baseline |
| 2 | The allocation profile (NIGHT10's debt) | debt | the Player's managed growth rose 37% (cause not measured); a Development Player profile settles it before more systems land |
| 3 | Objective rounds (objectives.js, roundType) + the swarm round's drop | port + small design | the special rounds; CoD's Max Ammo reward |
| 4 | THE PROTOCOL (protocol.js) | port | the run's main goal and climax, on the movement showpieces |
| 5 | Traps (traps.js) | port | the Zombies ritual the power wakes |
| 6 | The power switch | design (flag) | the first goal: wakes the PROTOCOL's relays and the traps |
| 7 | Exfil | design (flag) | the ending |
| 8 | The run's grade (runMoments.js) | port | the reward screen (and exfil's) |
| 9 | ULTRA HEALTH, a LAB toggle (hard damage + blood heal) | design (LAB, off) | ULTRAKILL's half, without overriding D-18 |
| 10 | Close | - | the handoff loop |

**Not in NIGHT11, and where it goes:** the round budget (with the roster, N13), save-and-quit (N12 or the Steam
night), RATTLER / TORCH and the rest of guns.js (N12), the bosses (N14: the boss rounds keep "playing with what
exists").

## Sources

The full lists (70 and 45 sources, each with its URL and a fetched / snippet / not-verified tag) are the two research
notes; their key URLs:
- [W 5-7] Kino / Der Riese power: https://gamefaqs.gamespot.com/boards/960188-call-of-duty-black-ops/57125185 ;
  https://call-of-duty-nazi-zombies.fandom.com/wiki/Forum:Kino_der_Toten_Solo_Strategy
- [W 15, 16] Liberty Falls / Terminus PaP: https://www.powerpyx.com/cod-black-ops-6-liberty-falls-how-to-unlock-pack-a-punch/ ;
  https://www.powerpyx.com/cod-black-ops-6-terminus-how-to-unlock-pack-a-punch/
- [W 20] BO7 power switches: https://dotesports.com/call-of-duty/news/black-ops-7-ashes-of-the-damned-turn-on-power
- [W 21] Cold War "too easy": https://lv1gaming.com/call-of-duty-cold-war-zombies-is-too-easy-and-thats-a-problem/
- [W 23] Liberty Falls: https://www.dexerto.com/call-of-duty/black-ops-6s-liberty-falls-has-finally-fixed-zombies-biggest-problem-2966268/
- [W 24] Treyarch on BO7: https://gamesbeat.com/how-treyarch-designed-black-ops-7s-zombies-interview/
- [W 26-28] power and the threat clock: https://h-o-m-e.org/cold-war-zombies-special-and-elite-enemies/ ;
  https://callofduty.fandom.com/wiki/Panzersoldat ; https://callofduty.fandom.com/wiki/Space_Monkey
- [W 1, 2, 29, 31, 32] dog rounds: https://www.codzombiesguides.com/bestiary/hellhound/ ;
  https://www.ugx-mods.com/forum/scripting/50/tutorial-changing-max-dog-per-player/3018 ;
  https://callofduty.fandom.com/wiki/Special_Round
- [W 36-38] elite backlash: https://gamingintel.com/biggest-issues-black-ops-6-zombies/
- [W 39-44, 47, 48] exfil: https://www.treyarch.com/game-intel/2020/10/Black_Ops_Cold_War_Zombies_Gameplay_Guide ;
  https://game8.co/games/Call-of-Duty-Black-Ops-6/archives/481913 ;
  https://gamerant.com/bo6-cod-call-of-duty-black-ops-6-zombies-exfil-exfiltration-guide/ ;
  https://www.charlieintel.com/black-ops-cold-war/cold-war-player-sets-mauer-der-toten-zombies-exfil-world-record-after-five-days-123471/ ;
  https://steamcommunity.com/app/1938090/discussions/0/4693406768754078535/
- [W 50-55] pacing: https://pastebin.com/vLBtXbJr (WaW `_zombiemode.gsc` copy) ; https://zwr.gg/leaderboards/bo/30-speedrun/kino-der-toten/
- [W 59-61, 68] save and pause: https://www.gfinityesports.com/article/call-of-duty-black-ops-6-zombies-how-to-save-and-quit-or-pause-while-playing-solo ;
  https://www.dexerto.com/call-of-duty/black-ops-6-zombies-finally-adds-feature-players-have-been-asking-for-since-2008-2856214/ ;
  https://www.charlieintel.com/call-of-duty/black-ops-6-zombies-players-can-finally-save-and-quit-but-theres-a-catch-335380/
- [W S1, S2, S4, S5] ULTRAKILL: https://ultrakill.wiki.gg/wiki/Health ; https://ultrakill.wiki.gg/wiki/Difficulties ;
  https://ultrakill.wiki.gg/wiki/Development/Changelogs ; https://ultrakill.wiki.gg/wiki/The_Cyber_Grind
- [W S7, S8, S14, S21] hard damage reception, the self-parry patch, the Cyber Grind wishlist:
  https://steamcommunity.com/app/1229490/discussions/0/4693405793414987187/ ;
  https://hakita.itch.io/ultrakill-prelude/devlog/98473/103b-patch-notes ;
  https://github.com/Hypatech/ULTRAKILL-Speedrunner-Issues/issues/48
- [W S25-S33] DOOM: https://help.bethesda.net/app/answers/detail/a_id/49879/ ;
  https://bethesda.net/en-US/news/doom-eternal-update-6-66-release-notes ;
  https://slayersclub.bethesda.net/en-US/article/doom-the-dark-ages-advanced-tips
- [W S34-S45] run arcs and Next Fest: https://deeprockgalactic.wiki.gg/wiki/Drop_Pod ;
  https://brotato.wiki.spellsandguns.com/Waves ; https://partner.steamgames.com/doc/marketing/upcoming_events/nextfest ;
  https://howtomarketagame.com/2022/11/04/steam-next-fest-october-2022-how-did-games-perform/ ;
  https://gmtk.substack.com/p/how-to-make-a-great-steam-next-fest

# Wave 56 research: how style and speed PAY the player (gameplay)

Written 2026-10-20 by a research agent (WebSearch / WebFetch only, about 12 minutes).
Sharp question: how do ULTRAKILL, DOOM Eternal, Devil May Cry 5, Hi-Fi Rush and Titanfall 2 make the moment-to-moment
loop pay for style and speed (meters that feed back, weapon freshness, resource loops, chain escalation, the rank-change
moment)?

Labels: **Verified** = I fetched the page and read the number there. **Reported** = it came from a search-result summary
or a page I could not open (the wiki returned 402 / 403 / 404), so it needs one more check before it becomes a config value.
Fandom wikis (ultrakill.fandom.com, devilmaycry.fandom.com) returned HTTP 402; doomwiki.org returned 403.

SKYGRIND today (from the brief, not checked in code): a style rank meter D to S that pays nothing and sits at D most of
the time; a parry; a punch that launches; heal on close kill; RIDE KILL refunds a boost; kill hitstop 0.05 s (6 steps at
120 Hz).

## Mechanics (11)

1. **ULTRAKILL rank ladder: thresholds and per-rank decay.** 8 ranks, each a segment that stacks: Destructive 200,
   Chaotic 300, Brutal 400, Anarchic 500, Supreme 700, SSadistic 850, SSShitstorm 1000, ULTRAKILL 1500 points. Passive
   decay rises with rank: 15 / 18.75 / 22.5 / 30 / 45 / 60 / 90 / 120 points per second. The top is hard to hold; the bottom
   is easy to leave.
   https://ultrakill.wiki.gg/wiki/Style (Verified)
   *SKYGRIND measure:* a skilled pilot (the bot on a skilled persona) reaches rank A within 20 s of the first kill in a
   combat room; the share of combat time spent at D falls below 40 % (today "most of the time", not measured).

2. **ULTRAKILL: rank pays survivability.** Hard Damage (the part of lost health you cannot heal yet) recovers faster by
   rank: timer x2.0 and 40 hd/s at Supreme, x2.5 and 50/s at SSadistic, x3.0 and 60/s at SSShitstorm; at ULTRAKILL rank it
   resets to 0 and "will never increase" until you drop a rank. Below Supreme: normal timer, 15/s.
   https://ultrakill.wiki.gg/wiki/Hard_Damage (Verified)
   *SKYGRIND measure:* the close-kill heal per kill at rank A is at least 2x the heal at D, printed per kill in the
   Player proof line (kill-gated, so it is not the retired passive regen).

3. **ULTRAKILL: getting hit costs a rank.** Taking damage drops the style meter one rank and cuts level score by
   damage x 5; a checkpoint respawn resets the meter to 0.
   https://ultrakill.wiki.gg/wiki/Ranking (Verified)
   *SKYGRIND measure:* one enemy hit lowers the rank by exactly one letter within the same frame; the drop is visible
   (the letter changes) in the next rendered frame.

4. **ULTRAKILL weapon freshness.** Per weapon 0-10: FRESH 7.5-10 (style x1.5), USED 5-7.5 (x1.0), STALE 1-5 (x0.5),
   DULL 0-1 (x0.0). Each attack or bonus with a weapon takes 0.7 from it and gives 0.5 to every other weapon; unheld
   weapons regain 0.1 per second. Boss attacks drain x1.5.
   https://ultrakill.wiki.gg/wiki/Style (Verified)
   *SKYGRIND measure:* with freshness on, the most-used weapon's share of kills in a 3-minute bot run drops by at least
   15 points vs off, and weapon swaps per minute rise (paired runs, same seed).

5. **ULTRAKILL movement multiplier on style gain.** 1x to 3x: rises 2.0 per second while sliding or airborne, and falls
   1x every 0.1 s once you stop (3x to 1x in 0.2 s). Style against bosses x1.5.
   https://ultrakill.wiki.gg/wiki/Style (Verified)
   *SKYGRIND measure:* a kill made after 1 s airborne or sliding earns 3x the style of the same kill standing, shown as a
   multiplier tag beside the meter; standing still for 0.2 s returns it to 1x.

6. **ULTRAKILL style bonuses: named, big for skill.** Kill 30/45, Double Kill 25, Triple Kill 50, MULTIKILL 100, Parry
   100, Airshot 50, Headshot 80, Enraged 250, Air Slam 160, Rocket Return 100, Round Trip 300, Chargeback 400, Terminal
   Velocity 350, Juggle 20.
   https://ultrakill.wiki.gg/wiki/Style (Verified)
   *SKYGRIND measure:* a parry, a launched-enemy (punch) kill and a RIDE KILL each award at least 3x a plain kill, so one
   skill move moves the meter more than three plain kills.

7. **ULTRAKILL close-range healing and the parry as a full refill.** Healing comes from attacking "an enemy at close
   range": punch 10 / 20 / 30 (torso / limb / critical), hitscan 10 / 20 / 30 (body / limb / head), gib 120, and a parry is
   listed as 999 (full heal up to the Hard Damage limit) and refills stamina. A parry "briefly" freezes and flashes white.
   https://ultrakill.wiki.gg/wiki/Hard_Damage and https://ultrakill.wiki.gg/wiki/Parry (Verified)
   *SKYGRIND measure:* a parry refills the dash / boost stamina in the same step (stamina bar full on the next frame).

8. **ULTRAKILL hitstop tiers.** Major hitstops (parries, "green" style actions such as interruptions) freeze time for
   0.25 s; assist game-speed changes do not change hitstop length.
   https://breezewiki.discard.no/ultrakill/wiki/Hitstop (Reported: search summary; ultrakill.wiki.gg/wiki/Hitstop gave 404)
   *SKYGRIND measure:* a hitstop ladder in whole steps (120 Hz): plain kill 6 steps (0.05 s, today), chain kill / launched
   kill longer, parry 30 steps (0.25 s); each printed per event; judged blind and paired (L-001).

9. **DOOM Eternal: one verb per resource.** Chainsaw a demon for ammo, glory kill for health, flame belch for armor;
   Hugo Martin: "The solution to all the problems is to be aggressive." Numbers: chainsaw holds 3 fuel pips, one refills
   by itself in 20 s; fodder costs 1, heavies 3. Blood Punch takes 2 glory kills per charge, 1 charge stored.
   https://gamedeveloper.com/design/the-aggressive-resource-management-of-i-doom-eternal-i- (Verified: the loop and the
   quote) ; https://doomwiki.org/wiki/Chainsaw_(Doom_Eternal) and https://doomwiki.org/wiki/Blood_punch (Reported: 403)
   *SKYGRIND measure:* each resource the player can run short of has one aggressive verb that refills it, and a bot run
   with that verb disabled runs dry at least once per round (the verb is needed, not optional).

10. **Devil May Cry 5: rank drives the music and a hit costs tiers.** Ranks D to SSS; one enemy hit drops 1-2 tiers;
    repeating a move does not raise rank. The battle song adds "huge" choruses for players at the top ranks, and the
    composers "wrote additional transitional material for moments that would take us from one rank to the next."
    S grade per battle reported at about 5,500 style points.
    https://wccftech.com/devil-may-cry-5-interview/amp/ (Verified: choruses, transitions) ;
    https://shacknews.com/article/110322/stylish-points-and-style-ranks-in-devil-may-cry-5 and
    https://gamewith.net/devil-may-cry-5/article/show/8284 (Reported: the 1-2 tier drop, 5,500)
    *SKYGRIND measure:* none for music (GG owns sound: no audio code). The rank-up event keeps a stable name so GG's
    audio layer can hook it later; the event fires on the same step the letter changes.

11. **Hi-Fi Rush and Titanfall 2: multipliers for variety, a meter you spend.** Hi-Fi Rush: on-beat attacks hit
    harder; the world syncs to the beat; an S rank pays more Gears at the stage end; reported multipliers Jam Combo x2,
    Aerial Rave x1.5, Repeat x0.7, Ouch (hit) x0.8. Titanfall 2: the Titan meter fills slowly over time ("every 5
    seconds" in the tech test) and faster from kills, damage and objectives; boosts spend it (Ticks 65 %, Map Hack 70 %,
    Amped Weapons 80 %).
    https://gamingbolt.com/hi-fi-rush-guide-how-to-get-s-rank-and-farm-gears/amp (Verified: S pays more Gears, repeat
    penalty, no numbers) ; https://www.trueachievements.com/game/HiFi-Rush/walkthrough/3 (Reported: the x2 / x1.5 / x0.7 /
    x0.8, page 403) ; https://www.destructoid.com/titanfall-2s-tech-test-returns-tomorrow-with-improvements/ (Verified:
    every 5 s) ; https://primagames.com/tips/titanfall-2-how-use-boosts (Reported: boost costs)
    *SKYGRIND measure:* an "Ouch" rule (a hit costs a rank, item 3) and a "Repeat" rule (freshness, item 4) are the two
    that fit; a spendable meter is out of scope for 2 h.

## What the five games share

- The meter is **never just a score**: it changes a resource (ULTRAKILL healing, DOOM ammo / health / armor, Titanfall
  boosts, Hi-Fi Rush / DMC currency at the end) or the soundtrack (DMC, Hi-Fi Rush).
- **Two levers push style:** variety (freshness, repeat penalties) and risk (a hit costs a rank; top ranks decay fastest).
- **Skill moves are worth several kills** (ULTRAKILL parry 100 vs kill 30; Chargeback 400).
- **The biggest freeze is for the defensive skill move** (ULTRAKILL parry 0.25 s, 5x SKYGRIND's kill hitstop).

## Three proposals, ranked by (crunchy kills and speed) x (buildable in 2 h behind a switch)

Every new number below is a gameplay number the web never had: it goes in a Unity-side config file (CLAUDE.md rule 5),
not typed in code. Randomness is not involved. Colours: rank-up accents in cyan (player-usable), never magenta.

**P1. RANK PAYS (switch `rankPays56`).** The rank multiplies what SKYGRIND already pays: the close-kill heal and the
RIDE KILL boost refund scale by rank (for example D x1.0, C x1.25, B x1.5, A x2.0, S x2.5); a hit drops one rank
(ULTRAKILL / DMC); style gain gets ULTRAKILL's movement multiplier (1x to 3x at 2.0/s while airborne or sliding, back to
1x in 0.2 s grounded), so speed is what climbs the meter. Retune thresholds and per-rank decay (ULTRAKILL shape: the
top decays about 8x faster than the bottom).
Done when: the bot reaches A within 20 s of the first kill in a combat room; time at D under 40 % of combat time; heal
per close kill at A = 2x D in the proof line; one hit = one rank down on the same step. RED with the switch off, GREEN on.

**P2. THE CRUNCH LADDER (switch `crunch56`).** Hitstop escalates with the chain and the move, in whole 120 Hz steps:
plain kill 6 steps (today), 2nd and 3rd kill within the chain window 8 and 10 steps, launched-enemy kill and RIDE KILL
12 steps, parry 30 steps (ULTRAKILL's 0.25 s major tier). The rank-up shows in one frame: the letter swaps and scales
on the same rendered frame as the event, with a single-frame cyan edge flash on the letter only (no screen-border
effect: retired w20). Named style tags for parry / launch kill / RIDE KILL worth at least 3x a plain kill.
Done when: each event prints its hitstop steps and the frame of its HUD change (event step to visible change at most 1
frame); a blind paired A/B (L-001) picks ON as crunchier.

**P3. FRESHNESS (switch `fresh56`).** ULTRAKILL's per-weapon freshness 0-10 (-0.7 for the weapon used, +0.5 to the
others, +0.1/s while holstered) multiplies style gain x1.5 / x1.0 / x0.5 / x0.0, shown as a thin bar under the meter
(text 16 px or more). It pays swapping (RB scrolls weapons, GG's binding) and feeds P1.
Done when: on the same seed, the top weapon's kill share drops at least 15 points and swaps per minute rise vs OFF.
Ranked third: it pays variety more than crunch or speed, and needs P1 before the meter matters.

Not proposed: rank-driven music (DMC, Hi-Fi Rush). GG owns sound; the rank-up event name is kept so his layer can use it.

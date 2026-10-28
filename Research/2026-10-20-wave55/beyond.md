# NIGHT55 A4: the beyond list (the lead, skygrind-idea-lab's five lenses; 17:18-17:30)

Our data first: the verbs are docs/VERBS.md rows 1-47 + ride kill (EXT53), overheal (w47), parry / launch / heal36, the climb, the ring
pass, the thread tear. Ideas already in docs/memory/IDEAS.md are not proposed again (C-17 TRICK CHAIN, C-18 OVERDRIVE, C-14 THE DUSK
CLOCK, C-03 the ghost). GG's standing words (CORE.md): kills "crunchy", bosses where "you just teleport into these random rooms ...
hard to understand what to do", "detailed rooms that really incentivize mechanics", JoJo colour turns (NIGHT54), "build not just basic
things" (NIGHT55).

Rule check on every idea: no audio, no new enemy design, no web edits, no UI restyle of existing panels (NIGHT54 D-4 holds: new
elements only, in the existing type styles).

## The ideas (score: fit x3, first-10-min wow x3, buildable in ~2 h by one builder x2, measurable in a Player x2, minus risk 0-3; /50)

| # | lens | the player moment (their words) | builds on | switch | measured by | fit | wow | build | meas | risk | score |
|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | GG -> feature | "I killed the boss and the whole screen froze like a comic panel, with its name crossed out" | grade54's turn, the boss death event | `finish55` (view) | a frame at the boss death: the freeze >= 0.8 s real time, a panel border, the turn's hue shift >= 25 deg; RED off | 5 | 5 | 4 | 5 | 1 | 47 |
| 2 | steal (Neon White) | "the last kill of the room went slow and it told me my time against a par" | room clear events, the flow table's dwell per room | `clear55` (view + a config par per room) | a Player frame of the clear card at the last kill of a452's room; the slow-mo held 0.4-0.6 s; par from data, never typed | 5 | 4 | 4 | 5 | 1 | 45 |
| 3 | GG -> feature | "the boss room told me what to do before it started" | wickIntro47, boss walk-ins | `brief55` (view) | per boss a 2.5 s card with the name and ONE verb line in the pad glyph ("RIDE THE PANELS · {punch} THE CORE"), 40 px at 1080p; a frame per boss | 5 | 4 | 4 | 5 | 1 | 45 |
| 4 | systems x systems | "a kill at full speed blew the enemy apart twice as hard" | crunch blocks, the sim's speed | `speedKill55` (view) | crunch blocks per kill at >= 18 m/s vs walking: >= 2x; FOV kick printed; a frame pair | 4 | 4 | 5 | 5 | 0 | 44 |
| 5 | steal (ULTRAKILL blood) | "the room I fought in looks like I fought in it" | crunch, scorch51 | `ink55` (view) | dark-ink splats (not red-orange: rule 8) persist per room, capped; before / after frame of a cleared room; frame time | 4 | 4 | 4 | 5 | 1 | 42 |
| 6 | systems x systems | "three kills fast and time slowed with a colour punch" | kill events, hitstop's slow-mo, grade turn | `chain55` (view) | 3 kills inside 1.5 s fire one 0.35 s slow-mo + a 0.3 s hue punch; no more than one per 4 s; a frame | 4 | 4 | 5 | 4 | 1 | 41 |
| 7 | steal (Doom Eternal) | "a flashing staggered enemy, I punched it and it exploded into health" | heal36, launch, the punch | `execute55` (sim) | a staggered state at <= 15 % HP shown pale yellow (parryable family), a punch kills + heals; golden / fingerprints EQUAL off; harness counts | 5 | 5 | 2 | 4 | 3 | 40 |
| 8 | verbs | "landing from a big fall dips the camera and I feel the weight" | move54 | `land55` (view) | camera dip proportional to the fall speed, recovered in 0.2 s; a354-style trace | 3 | 3 | 5 | 5 | 0 | 38 |
| 9 | verbs (ULTRAKILL coin) | "I punched my own slug and it curved into a head" | the parry, the THUNDERCLAP slug | `slugParry55` (sim) | punch inside 0.2 s of the slug passing redirects it to the nearest head | 5 | 5 | 2 | 3 | 3 | 38 |
| 10 | wild | "phase 3 the world went black and white except me and the danger" | grade54 | `eclipse55` (view) | the graded world saturation < 0.1 while cyan / red-orange tells keep theirs (drawn after the grade) | 4 | 5 | 2 | 4 | 2 | 38 |
| 11 | GG -> feature | "the room's door only opens when I ride the wall to it" | levels, I3 teach verb | level data | the identity check I3 + the walk bot 10 / 10 | 5 | 3 | 2 | 4 | 2 | 36 |
| 12 | steal (Titanfall) | "the wall ride leaves a cyan line I can follow" | ride, the grapple ribbon | `rideTrail55` (view) | a ribbon on a ride frame; 2 s fade | 3 | 3 | 5 | 5 | 0 | 37 |
| 13 | wild | "the boss's arena lights went out one by one as its health dropped" | light54, the boss bar | `dim55` (view) | lights off per 25 % of the bar; a frame at each quarter | 4 | 4 | 4 | 4 | 1 | 39 |
| 14 | systems x systems | "my style rank shows on the boss bar's side" | style grades (survival), bosses | `style55` (view) | rank letter on boss frames | 3 | 3 | 3 | 4 | 1 | 31 |
| 15 | verbs | "slam from high up and the shockwave knocks the pack back" | slam | sim | knockback count | 3 | 4 | 2 | 4 | 3 | 31 |
| 16 | steal (Hades) | "each boss has its own coloured floor glyph under its attacks" | tells | view | glyph hue per boss | 3 | 3 | 3 | 4 | 1 | 31 |
| 17 | wild | "the camera rolls a little on a long wall ride" | move54 | view | roll deg trace | 3 | 3 | 5 | 5 | 1 | 37 |
| 18 | GG -> feature | "each level ends on a view of the next boss's arena" | campaign | view | a frame | 3 | 3 | 3 | 3 | 1 | 29 |

Losers (16, 18, 14, 15) go to docs/memory/IDEAS.md as `new` at the close.

## The slate's P2 picks (in order)
1. **finish55** (#1): the boss-kill freeze frame (JoJo / comic panel). Pure crunch for the bosses GG asked about most.
2. **brief55** (#3): the boss brief card (GG: "hard to understand what to do"). New element only; the UI styles unchanged.
3. **clear55** (#2): the room-clear beat with time vs par (gives every room an end and a pace).
4. **speedKill55** (#4) + **chain55** (#6): kills that pay movement (one builder, both view rows).
5. **ink55** (#5): rooms keep the fight on them.
Held back: execute55 (#7) and slugParry55 (#9) touch the sim and need two rounds; eclipse55 (#10) needs a post-grade tell pass.

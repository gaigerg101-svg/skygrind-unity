# Wave 40 forge notes (2026-10-13 22:00-, the wave-forge skill; builds nothing in the game)

## Stages
- S0: GG's words stored verbatim with a row map (GG-REQUEST.md). Branch: none made; the planning set is committed on port/stage3 as
  every forge since wave 36 (no `wave-40` branch: the lead's integration branch is port/stage3).
- S1 recall: HANDOFF (NIGHT39 block), NIGHT39-REPORT / LOG close table, DECIDE39, LESSONS L-001..L-064, PRINCIPLES, VERBS rows, OWNER
  (GG's w37 words: wall rides "stay on forever" -> ride38, built and left OFF). No screen pass: GG played n39-r4 tonight and his notes are
  the screen pass.
- S2 diverge: skipped as a separate pool. GG's request is 22 concrete asks; the ideas are inside the cards (the boost as a chained
  lesson, the armoury zone, the grapple tear, the ring that lights at 12 m/s, the three enemy roles). Logged as a deliberate skip.
- S3 research (3 web agents, parallel): research/boss-web.md, research/enemies-web.md, research/feel-web.md. Code (2 read-only
  explorers + the forge's spike): research/CODE-MAP.md.
- S4 converge: every ask maps to one card (GG-REQUEST.md's row map); P0 debt is small (NIGHT39's SHORT rows are OFF rows GG did not
  mention, carried as leftovers); ride38 (NIGHT38, OFF) is the base of R2.
- S5 spike: read Loom37.cs:415-440 (the anti-hide writes a station from the player's z, no clamp: confirmed) and design.n38.ride38 (the
  hold ride with no sink, the pitch steer: GG's w37 ask; OFF for a5 deaths): both premises confirmed in the code.
- S6 draft: NIGHT40.md (292 lines before the red-team, 305 after), DECIDE40.md.
- S7 attack: one fresh reviewer (files only) + the forge's dry run of G0's commands (every named script, file and flag exists;
  bossLine30 lives in Lantern30.cs only; LevelSelect29's expected count changes with cut40).

## Choices (each with the rejected alternative)
- **Ten builders** (one per boss, one per level, one per system) over seven: GG asked for "extensive construction" across 22 asks; NIGHT37
  (four bosses at once) and NIGHT38 (eight) finished; the spend-limit rule and a cut order cover the risk. Rejected: seven builders with
  B5 + B4 merged (two boss rewrites in one context missed L-060's shape).
- **The THUNDERCLAP's charge is surfaced, not invented:** the code already charges (tap36). Rejected: a second charge mechanic.
- **Level 3's start weapon (D-2):** the reason GG gave over his literal words, with the literal reading one config flip away and asked
  in the reply. Rejected: the literal reading as default (it would rebuild K4's level 3 opening around a delayed shotgun).
- **The start grace as both a move trigger and a timer** (GG: "a short delay or a required movement trigger"). Rejected: a trigger only
  (a player who stands still would wait forever).
- **The new enemies split sim (E3) / view (M3)** so the models of all seven kinds share one owner. Rejected: one enemy builder.
- **Level 6 removed with `git rm`** (GG: "deleted entirely"); the tag keeps it. Rejected: hiding it only.

## The red-team (one fresh reviewer, files only; 14 findings) and what changed
| # | finding | change |
|---|---|---|
| 1 | reel frames ~0.6 GB each: 28 reels cross the 10 GB floor at reel 13-15 | GG frees disk first (D-0 answered); the lead deletes its own reel frames after each verdict, keeping the looked-at ones |
| 2 | ten builders, ~40 merges, 270-min cards past the window | nine builders (M3's models folded into E3, whose sim work shrank, see 8); cards end by the round-4 build (4:20) |
| 3 | ~45-50 reels implied vs a cap of 28 | the round-1 "before" verbs dropped (NIGHT39's freeze verbs are the before); per-card reel quotas summing to 38 |
| 4 | B5 and M3 asked to run Players / a bench | the lead runs crown37 with both pilots in G0 and every reel; the bench gate replaced by a renderer count per model |
| 5 | in-place level edits and `git rm` break `--off40` | every level edit goes to a new n40 id swapped in by its row; rods40 is a view filter; level 6's `git rm` waits for GG |
| 6 | shared files with two owners; lead files cards need | one owner per file; the shared commit pre-places Curriculum25 swaps, N40StartSet, Campaign24's hold, Walk29Pilot lines; B4 owns Boss37.cs (additive), R2 the checker's ride move, E3 the walk bot's combat |
| 7 | builders launched before the shared commit exists | launch on the shared commit's green |
| 8 | the "new" enemies re-invent the ported DRONE / WARDEN / CHARGER; BULWARK clashes with a boss id | foes40 brings the ported three into the campaign, retuned (D-11) |
| 9 | the test hour does not fit | round 4 (4:20-5:00) runs the verbs and boss reels on n40-r4; the test hour is the suite first, then only re-runs |
| 10 | no checker owner for the endless ride | R2 owns the ride move; the lead re-runs every ON level's gates |
| 11 | the THUNDERCLAP's ground click fires on release (L-055) | W1 measures the ground latency against p39's 58 ms |
| 12 | the STITCHER and start sets on separate switches can leave an unwinnable LOOM or a lost pistol | the STITCHER leaves only with loom40; level 2's start set with iceline40; level 3's with kiln40 |
| 13 | the row map named cards that did not exist | fixed |
| 14 | level 3's start-weapon default against GG's literal words | GG answered before the go: "yes the pistol and gauntlet" (the shotgun first, D-2) |

## The forge experiment
forge experiment: a shared `<boss_rules>` block (movement-boss principles as eight checkable rules from the research) referenced by
three boss cards, instead of per-card prose; hypothesis: the boss cards close with fewer clarity defects (GG's "no idea what to do" /
"runs out of the map" class) and GG's next reaction names none of them; measure: NIGHT40-REPORT's B2 / B4 / B5 rows with rules 4 and 5
GREEN in the Player, and GG's next words on the three bosses.

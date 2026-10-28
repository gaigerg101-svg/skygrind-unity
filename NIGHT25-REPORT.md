# NIGHT25-REPORT.md: for GG (the teaching campaign; written 2026-10-08 evening, from NIGHT25-PROOF.md and NIGHT25-LOG.md)

Every "PASS" below is a bot or a scenario on a Development Player (or the headless sim), never a person. Nobody has played any of it yet.

## What you reported, and did each die on a Player?
| You said | Cause found | On a Player |
| --- | --- | --- |
| the arena is bugged out (worm over an empty sky) | the worm's room was collision only: nothing drew it | fixed: the room draws floor, walls and a light while the worm fights; pixel check lower third 0.051 (void) -> 0.439 (room); `wyrmvoid` PASS, and no worm starts outside the campaign |
| `ROUND 1 · 0 LEFT` over the worm | the survival round line shows in a campaign | hidden while a boss fights (lead's edit); seen gone in the frames |
| no health bar | no read for the worm | bar 'KILN WYRM', HP, phase mark, 2 pips, 24 px text: `wyrmbar` PASS |
| it does not die | the kill path was complete; each death reset the worm to full HP and there was no bar | a death now restores the worm to the START of its phase (0.29 -> 0.33); `wyrmdies` PASS: bossDown, pieces gone, LAYER CLEARED |
| melee while firing | NOT the sim: it runs both in one step. PUNCH sat on R3 under the aiming thumb | pad layout v5: PUNCH on RB, SPECIAL + ARCLINE on R3. `meleefire`: 8 shots + 5 punches in 3 s. PASS on the sim; the pad feel is yours |
| the PULSE builds by use, launches | built | `pulse` PASS: first notch after 7 landed hits, release launches 1.1 m (asked 1.1 m) |
| remove the shotgun shockwave | removed | `shockwave` PASS: 0 shockwave events on 3 LT presses, LT zooms x1.25 |
| aim the boss at me, a little harder | built | headless composed sim: player hits per minute 3.0 -> 5.5, kill 3:01 -> 3:57, shortest tell 0.60 s; Player `wyrmaim` PASS (aimed 13 / plain 67 spits, 2 lunge locks, 2 re-aims) |
| a tutorial, then ten levels | built | tutorial CLEARED by the bot in 42.2 s; levels 2-10 exist as plans |
| bosses | worm, BULWARK, MONOLITH, ELDER KILN WYRM, RIVAL PRIME | `bosses` on level 10's map: each has bar, floor + room, a kill with the real gun after HP cuts (40 / 87 / 33 / 38 / 58 s), a retry that restores its phase |

## Per row (what you can do, switch, number, what may feel worse)
- **PULSE** (`design.n25.pulseCharge`): hold the left click on the gauntlet, release to launch. 7 hits to the first notch; tiers launch 0.5 / 1.0 / 2.25 of a jump. Worse: unknown until you hold it; the other guns' kills no longer feed it.
- **Shockwave off** (`shockwaveOff`): the THUNDERCLAP's LT zooms; no shove. Worse: you lose a 15 s crowd-control verb.
- **Melee + fire** (`meleeFire`): RB punches while the gun cycles (layout v5). Worse: SPECIAL + ARCLINE move from RB to R3 (RB was SPECIAL in layout v4; Y still cycles weapons).
- **Aimed worm** (`wyrmAim`): spit leads you, the lunge locks onto you and re-aims once, 10-15 % less gap, phase-3 overlap. Numbers: hits per minute 5.5, kill 3:57 for the lag bot, about 2 bot deaths per fight. Worse: harder by design.
- **Tutorial** (`tutorial`): 8 areas, lessons in order, no fail state (a stage respawns you). 24 px prompts, 217 ms in / out.
- **Curriculum** (`curriculum`): levels 2-10 as plans, validate 0 errors; the bot's first-try clear rates are in `Design/levels/CURRICULUM.md`.
- **Boss rooms** (`bosses`): BULWARK and MONOLITH rooms, floor + light. **ELDER KILN WYRM** (boss `final`): four phases (75 / 50 / 25 %), tail shed, a spiral pattern, bigger room; the lag bot kills it in 5:31 mean (target 5:00-6:30) with 1-2 deaths. **RIVAL PRIME** (`bosses.rival`): the V2-style mirror duel, only magenta in the game; the bot kills it in 2:56 mean with 1-3 deaths. Not ported from the web's rival: slam, retreat diamond, rage flare, pillar HP, arcline copy, orb parry.
- **Hitch** (`hitchFix`): cold first PLAY 346 ms -> 247.8 -> 74.4 ms on a quiet machine (route prebuild N20StartRun 138 -> 1 ms, HUD wake 173 -> 5.7 ms). Still SHORT: two frames above 33 ms (SimStep 24 ms, R.World 20 ms). On a busy machine the tour read 197.5 ms: not comparable.
- **BIG PICK head**: round 5 reads 'BIG PICK · ROUND 5' (was ROUND 10).

## Not verified
- Everything by a person; no blind reviewers this night.
- `levelwalk` (Player bot, 235 s game-time budget): only level3, level4 and level8 LEVEL CLEARED; layer1, level5, 6, 7 and 10 stopped by the budget (the bot is slow in shield and fist rooms; the harness bot clears them).
- `campresults` (the results screen: focus on RETRY LEVEL, CONTINUE) FAILED on the final-code Player run (CONTINUE gave PendingMapId ''); the focus fix is pending (builder F), recheck on the final build.
- m9 pop-ins re-measure, the quiet hitch re-measure, the fingerprint record (`N25OnPath`) and the frozen close suite were pending when this was written (suite 2 on the integrated tree: 1760 / 1760, 97 / 97 golden, 22 / 22 replays).

## Questions that matter (DECIDE25; blank = the default acts)
- **D-1** What "pulse left click builds by use" means: default = the charge rises with its own hits, release launches, a little damage. Is that it?
- **D-4** The worm is level 2's boss (today's layer 1) with its bar; the tutorial has no boss. Keep?
- **Pad layout v5**: PUNCH on RB, SPECIAL + ARCLINE on R3. Does R3 as SPECIAL hurt?
- **ELDER / RIVAL knobs** (all in `design.n25.bosses.*`): ELDER `hpMult` 1.2, `phaseMult` 1.12, `spitDamageMult` 0.5 (the first guess, HP x2 and speed x1.25, killed the bot 0 of 3 in 14 min); RIVAL `hpMult` 1.8, burst 3 x 7, orbs 9. Raise `hpMult` to lengthen; each +0.1 on the ELDER is about 25 s.

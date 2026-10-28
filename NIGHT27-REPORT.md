# NIGHT27-REPORT.md: levels 1-3 rebuilt tight, two new bosses, the PULSE and the charge shots (for GG)

Written by the lead at 09:55 from NIGHT27-LOG.md. Nobody has played any of this by hand; every line below is a bot, a harness line or a Player reel.

## Outcome

- **Can you play levels 1-3 in order?** The path is built: TITLE > PLAY > CAMPAIGN > level 1 (the tutorial, ten stages, then THE LANTERN), level 2 (the pulse hall, the forced wall ride, the Glacier chase), level 3 (THE KILN, three tight rooms, then the worm). A bot on a Player cleared the tutorial in 148.5 s, walked layer1's three rooms in 81.4 s (links 8 s each, the worm's room entered at 102.7 s) and entered level 2's chase room at 13.4 s. The two new bosses were each killed on a Player by a bot (the Lantern at 87.9 s, the Glacier at 47.9 s). Level 2's chase was not walked from the level start to LAYER CLEARED in one go, and the worm was not fought by the walk. **Under 8 minutes in total is not verified** (tutorial about 2.5 min + level 2 about 1-2 min + level 3 about 1.7 min + the worm, which was 4:45 in NIGHT26).
- **The honest state of the build:** the game with every NIGHT27 block off passes the Player smoke (2 of 2 runs). With the rows on the smoke fails 7 to 21 steps (bot-driven steps and a tape replay that differs in two of the runs); the cause is not found (NIGHT27-LOG.md "Freeze and verification"). `chargeShots27` is OFF by default because of that. Play it, but expect the survival bots (autopilot) to behave differently under the new PULSE.
- **The suite:** 1807 tests, 1806 passed (the one red, `BotZeroAllocTests.FollowerStep_AlongAPlannedPath_AllocatesNothing`, passes alone 2 of 2).

## What each boss asks of you

- **THE LANTERN (level 1; verb WALL RIDE, weapon any gun, the PULSE to finish):** you can only hurt the face that looks at the wall you are riding (the shield is up in 2532 of 2532 not-riding frames and down in 1400 of 1400 riding frames in the Player reel; a 0.35 s grace after a ride). Orange orbs with a 0.6-0.8 s tell and a floor shape; three phases; in the last the wall segments retract and five small sentries fill the pips so you can finish with the PULSE. A pilot kills it on 10 of 10 seeds, median 83 s (target 75-100 s). It never stuns itself and never opens the window on a timer.
- **THE GLACIER (level 2; verb SKATE + PULSE LAUNCH, weapon the gauntlet only):** the original blocky skater on one blade ahead on the ice. When you close under 12 m a PULSE-ready flash opens a 1.5 s window; an LT blast that lands counts; a miss costs 20 m of gap, never health; three hits kill. The pilot kills it on 10 of 10 seeds (median 39 s, a floor set by the 15 s blast cooldown) and the Player reel in 47.9 s with 3 hits of 5 windows. The course is meant to curve in the view (code and numbers exist, not seen on a frame).

## The PULSE, the charge shots, the slow-mo, the skate feel, the roof

- **PULSE:** one slow heavy swing every 2.5 s (259 damage); KILLS (not hits) fill five pips on the glove; "PULSE READY" appears and LT blasts you forward (a 0.4 s boost) with a 15 s cooldown; the glove says why it is not ready ("NEED 5 KILLS", "COOLDOWN"). Harness and a Player reel on a virtual pad pass. **Why "isn't working" on your pad is not confirmed:** `Research/2026-10-09-wave27/PULSE-DIAGNOSIS.md` ranks the causes (silent gates, hits-not-kills with a third lost per hit taken, a reel that never used the pad path, trigger chatter at 0.35 now with a release threshold of 0.25).
- **Charge shots (OFF by default):** RT held past 0.4 s on the pistol (full at 0.6 s more) or the shotgun (0.8 s) fires one heavy shot on release (pistol 4.00x through three bodies, shotgun 2.74-3.00x), 10 s / 14 s cooldown; a tap fires at once. A new press restarts the hold (so a script that presses while holding keeps auto-fire). Flip `design.n27.chargeShots27.enabled` to turn it on.
- **Slow-mo on a wall-ride aim:** the world eases to 0.75 in 0.136 s while you ride and aim, with a vignette (0.45 to 0.70), chromatic aberration and a 4 degree FOV lift; look stays crisp; Time.timeScale never changes. Measured on the Player (steps per frame 1.48 vs 2.00). No blind comparison was made.
- **Skate feel on ice:** FOV +8.4 to +9.9 deg, camera roll 3.7 to 5.0 deg, floor streak strength 1.00 at 12 m/s, dust and blade trails; no screen-border lines. The floor-pixel motion gain was not measured.
- **Roof:** a ceiling over every room and link with cyan strips (60 strips, 888 AO quads; frame ms ON 15.19 vs OFF 16.36 with the player still and no enemies); the boss arenas have a skylight slot. A frame of layer1's casting floor reads as an indoor room. The sky gap above the wall tops is not closed.

## Per row (the close table in NIGHT27-LOG.md has the full lines)

8 DONE, 7 SHORT of 15: DONE: start, shared commit, wallLanes27 (wall route about 2x faster, computed), hazardFloor27 (12 HP/s only while live), pulse27, THE LANTERN, tutorial27, docs. SHORT: arenas27 (bot clears in 90-105 s, under the 120-180 s aim; no blind check), slowAim27 (no isolated 0.75 and no blind set), chargeShots27 (off), THE GLACIER (LAYER CLEARED and the curve not checked), skate27, roof27, the freeze checks (smoke with rows on; fingerprint on-path not recorded).

## Not verified (a person must play)

Everything on a pad; whether the PULSE LT works on your hardware; whether the Lantern makes you wall ride or lets you stand; the Glacier's curve and the skate feel; the roof and its sky gap; the pulse hall's melee (the bot median is 65 s with runs over 300 s); level lengths for a human; the survival bots under the new PULSE.

## Questions that matter (Research/2026-10-09-wave27/DECIDE27.md; defaults stand)

1. **Charge shots:** turn them on anyway (and fix the smoke later), or hold them until the smoke cause is found? (D-2)
2. **Which prompt keeps popping up on this machine** (the Windows Firewall alert or a Claude permission prompt)? It may be interrupting the Player runs.
3. **The Glacier "original" body (D-3):** a blocky skater on one blade is built; if you meant an earlier look, name it.

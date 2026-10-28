# NIGHT58 report for GG (2026-10-21, 13:59-16:1x MST; lead + three builders; tag u3-n58)

Your ask (13:45): "need to build for next 2 hours what else can be built and itterated upon". The night took NIGHT57's near misses and
level 3's two weak spots, and started level 4. Short version: one fix ships (yellow56: VERDICT_PENDING), the rest is measured and
written down for the next night, and NIGHT57's close is now really finished.

## First: NIGHT57's close had not finished
NIGHT57's report said seven rows were ON and tagged `u3-n57`. In the repo, four of them (level 3 as n57-kiln, wick55, grade55, colour56)
were still OFF and the tag did not exist; only the play build `Builds/n57-freeze` had them on in its own copy. Tonight turned them ON in
the repo with NIGHT57's measured reasons, ran the gates (all green) and made the tag. Nothing you played changes; the repo now matches it.

## What is ON in the play build Builds/n58-freeze
- Everything that was ON at NIGHT57 (the seven rows), unchanged.
- No new row is ON: yellow56 measured well but its critic died twice on network errors, so by the rule it stays OFF (one-line flip
  next night: `node Tools/night/n58-freeze-rows.cjs n56.yellow56=<reason>`). What it does: the leftover yellow that was not a parry tell (the foundry's lava-trench edge glow, a column's top plate,
  spire's walls) is held dim, so yellow on screen means "parry this". Measured in the Player: frames with more than 0.5 % non-parry
  yellow, foundry 6 / 6 -> 0 / 6, spire 6 / 6 -> 0 / 6. Off switch: `-skygrindNoYellow56`.

## Built tonight, OFF (with the number that holds each)
- **Level 3's exits** (the bot could not find the exit doors in kb2c and kb3; the harness had to move it on 14 / 15 times per 10 runs).
  The cause is found: the top tier and its ramp form one wall between the fight and the exit. The fix (two arches under the far bridge
  and the gallery, a spawn moved) cuts that to 3 / 1 / 0, but kb2c's room check fails (the player waits at its door 1.75 s, limit 0.5)
  and bot deaths rose 9 -> 13 over 30 runs. Held as a patch for the next night, not in the level.
- **Level 4 (THE SPAN)**: the card for a new room in the p2 link, THE GAP NEST, with the drone new to level 4, and its generator. No
  room built yet.
- **kill55** (hit marks): a reel now finds the frame where the white hit flash ends and probes the mark there: KILL55_PROBE.
- **press57** (punish standing still): measured on the new "chosen stillness" (the stick released, speed under 0.5 m/s): it does not
  move it by 5 points on any level (foundry 10.1 -> 7.7 %, iceline 20.0 -> 20.4 %, level 3 6.4 -> 5.9 %). It does cut the damage you take
  while moving. OFF.
- **rim56** (enemies brighter than the world): 79 % -> 77 % of enemies above the world's bright tenth (want 90 %): the fix dimmed a
  violet trim that the frame never showed. OFF.
- **The exit lamp** (a cyan frame + light on each level-3 door when its room clears): built, switched off (`UnityTuning.Exit58On`):
  no measurement of its brightness yet.

## PLAY notes (what to look at, 10 minutes)
1. Any level, a parry: the yellow on screen should only be the parry tells (orbs, halos). Foundry's lava edge is no longer yellow.
2. Level 3: the exits are as NIGHT57 shipped them; if you get stuck at kb2c's or kb3's far wall looking for the door, that is the cause
   above (the fix is held).

## Your calls
1. Level 3's exits: the held fix makes the exits findable but the bot dies more after it. Ship the arches (accept the deaths) or wait
   for the next night's spawn re-cut?
2. The judge: still a bot plus an evidence critic; your play is the veto.
3. NIGHT56's open calls still stand: g8's lava strip (T-136), the foothold rule (T-134).

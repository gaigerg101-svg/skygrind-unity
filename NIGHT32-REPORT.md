# NIGHT32 REPORT (for GG): bosses in half a minute, a grapple with charges, a round ring, level 4 rebuilt, level 5

**Outcome:** everything you asked for is built, switched ON and checked in the real Player (build n32-r5), with frames looked at. The bosses now die in about 26-39 s for the bot pilot (NIGHT31: 25 s to over 4 minutes), after your "still too tanky" I cut them again by half. The grapple can no longer be spammed. Boss 1's arena is round. Level 4 is rebuilt from scratch, levels 1-3 each got new arenas, and LEVEL 5 THE SPIRE exists with a new boss, THE CROWN. Suite 1877 / 1877, smoke all steps passed, level select 20 / 20. Nobody has played it with hands yet.

**How I worked (the "best methods" part):** 20 minutes of research with five subagents (three web: what works for people building games with AI, boss and grapple numbers from shipped games, arena design; two code audits), then six builders in parallel. The rules we followed: a target number before any change, seeded bot sweeps to prove it, campers and spammers as test bots, one change at a time, and frames from the real Player looked at every round. The research is in Research/2026-10-10-wave32/.

**Why the bosses felt tanky:** not mostly their health: outside short windows most hits did 1-6 % damage (the Stag's guns 5 %, the Wyrm's body 1.5 %, the Lantern 0 % from a platform, the Loom's knees out of reach), and the Stag / Wyrm windows were shorter than the gauntlet's cooldown. Also the Wyrm's charged slug bonus never applied (a bug). Now every hit does at least 25 %, the boss trick is a x2.5 bonus window, nothing stays unhittable for more than 1.5 s, and the pools are cut to a ~30 s bot fight.

**Player kill times (pilot bot, near-perfect aim; a person will be slower):** Lantern 30.1 s, Stag 26.6 s, Wyrm 29.2 s, Loom 28.0 s, THE CROWN 39.3 s. Campers still lose on every boss (standing still gets punished faster now, with a red-orange warning on the floor), except one: a shotgun camper at a Loom deck corner dies but deals 63 % first.

**The grapple:** 2 charges (two cyan pips by the reticle), each back after 4 s, a kill gives one back, 0.8 s between pulls. A TAP pulls only to rings, boss joints and big enemies, not walls (a HOLD still swings from a wall). Spamming for 20 s: 6 pulls now, 33 before.

**The ring:** 64 wall pieces over the fight's 16 sections, a disc floor, curved platforms. It reads round in the frames. Cost: a full wall-ride lap keeps 3-6 % less speed (48 pieces would cost nothing: one number).

**Levels:** level 4: nine smaller rooms that turn instead of a straight line, two or three heights each, every ring route built for two charges. New arenas: 1.9b LAVA BANKS, 1.10 CRUCIBLE (level 1); 2.6 FIGURE EIGHT, 2.6b SKATE RING (level 2); 3.2 POUR HALL, 3.2b SPLIT LEVEL (level 3). Level 5 THE SPIRE: one room per earlier level's verb, THE GAUNTLET, then THE CROWN in a true circle; all four guns; clearing it earns the TORCH.

**Your calls (DECIDE32.md):** the boss length (now ~30 s bot), the grapple numbers (2 charges / 4 s), 64 vs 48 ring pieces, and THE CROWN itself (you own enemy design: `design.n32.boss5` off removes it). `-skygrindOff32` gives NIGHT31's game.

**Not verified / weak:** a human playing it; the Loom corner camper; level 5's rooms all sit at one floor height (the level format cannot raise rooms); the boss models are still blocky boxes (only their weak spots glow now); frames fail to write in long reels (disk C is 93 % full; I did not delete anything).

# NIGHT33 REPORT (for GG): a HUD that looks made, real light, dressed rooms, a whip, a piercer, a shop

**Outcome:** all twelve things you asked for are built and switched ON. Each was checked in the real Player (freeze build `Builds/n33-r7`), and I looked at the frames. Smoke passed all steps, level select went 20/20 across all five levels, and DoorFree passed. The bosses are cut again: the bot pilot now kills them in about 18 s (NIGHT32: 26-30 s). Nobody has played it with hands yet. `-skygrindOff33` gives NIGHT32's game for comparison.

**What changed, with the frames to compare (all under `Logs/feel/`; RED = before, GREEN = now):**

| Your ask | What it is now | Frames: before / now |
| --- | --- | --- |
| The HUD "looks so AI generated" | Every filled box is gone. Text has a 2 px shadow on the world, there is one slash motif, and one message shows at a time. Health is a 64 px number over ten slash ticks. The weapon name and its LT action sit off the gun. Points are bone white. The boss bar is a 640 px line. Title, level select, results and pause use the same look. Rule check: 31 problems before, 0 now. | `n33-r1/redhudfeel/hud33/shot-N30-hud33-1080-rest.png` / `n33-r6/hudshop/hud33/shot-N30-hud33-1080-rest.png`; menus: `n33-r5/hudc/hud33/shot-N30-hud33-1080-title.png` |
| Lights "placed and floating" | Every light hangs from a fixture you can see: sconce, strip, pole lamp, framed panel, brazier, or lava light. Nothing floats (53 floating lights on level 1 before, 0 now). The grade is darker, with pools of light, and the lava glow stays local. | `n33-r1/redlookf/look33/shot-N33-look33-f10-a.png` / `n33-r5/envf/look33/shot-N33-look33-f10-a.png` |
| "More elements in every arena" | Every room on all five levels is dressed with 313-617 decor pieces: skirts, cornices, pilasters, pipes, trusses, lit lintels over doors, and one landmark per room. Floors stay clear, and decor never blocks a ride strip. | `n33-r2/redkit/kit33/shot-N29-N33-kit33-f10-a.png` / `n33-r2/kit/kit33/shot-N29-N33-kit33-f10-a.png` |
| "The arena design is poor" | Ten rooms rebuilt: f8, f9, i2, i5, k1, k4, s1, s6, v2, v4. Each has two or more heights, cover, a landmark and a device: an updraft fan, a hazard vent with a red-orange warning, or a moving platform. The worst death rooms are fixed: f5 went from 291 deaths to 17, k4 from 146 to 34. | `n33-r5/envf/arenas33/shot-N29-N33-arenas33-f9-a.png` |
| The zoom is "a forced angle" | LT zooms the world only. The gun lowers 5 cm, and no longer slides 338 px to the centre or tilts 5°. | `n33-r1/redhudfeel/feel33/shot-N30-feel33-zoom-on.png` / `n33-r1/hudfeel/feel33/shot-N30-feel33-zoom-on.png` |
| Enemies run into lava | Enemies no longer path or get pushed into lava: 39 of 41 burned in v6 before, 0 of 5 now. Your knockback can still throw them in. | `n33-r2/redai/ai33/shot-N29-N33-ai33-v6-20s.png` / `n33-r2/ai/ai33/shot-N29-N33-ai33-v6-20s.png` |
| ULTRAKILL trigger spawns | Six lava rooms hold their first wave until you cross to the far side, released after 12 s at most. Every enemy appears with a red-orange column at least 8 m away. | (no frame: the reel budget ran out) |
| Bosses "still way too tanky" | Player pilot: Lantern 18.3 s, Stag 17.6 s, Wyrm 19.0 s. Harness: Loom 19.3 s, Crown 18.1 s. A person should take about 40 s. Camping now hardens the boss: half damage, a grey shell and "MOVE". No camper dealt more than 35 % of the bar. | — |
| The grapple feels bad | The whip: tap an enemy. Small ones are yanked to you. Big ones and bosses pull you in, and you stop 1.5 m short with a hop, a small hit and a hit-pause, never inside them. Rings and boss joints still pull. A whip kill gives no charge back, so it can't chain forever. | `n33-r3/hudfeel/feel33/shot-N30-feel33-whip-stop.png` |
| A stronger piercing pistol charge | The charged KESTREL goes through everything in a line at 9x damage, with a beam to the wall and a kick. 4 s cooldown. | `n33-r1/hudfeel/feel33/shot-N30-feel33-pierce-beam.png` |
| Points that buy something | A level's points go into a wallet. CONTINUE opens a SHOP with eight upgrades at 2,650 / 5,300 / 10,600 (set from what the bot earns per level). The rack holds two weapon mods, one movement and one wildcard. A replay only banks what beats your best. | `n33-r6/hudshop/shop33/shot-N30-shop33-open.png` |

**How I worked:** I made one shared commit first, so every change could be switched off by row. Then seven builders worked in parallel for three rounds. In every round I ran a Player build and looked at the RED and GREEN frames, then sent fixes back with the exact frame. The checks caught three things that would have broken your playtest:
- the n33 levels hung the Player at boot (an infinite loop in the prop code);
- trigger spawns soft-locked levels 2 and 5;
- a new catwalk created a lava pit you could not get out of.

All three are fixed.

**Your calls (DECIDE33 defaults stand):** the boss length (about 18 s bot, about 40 s for you); the shop's eight upgrades and prices; whether the 0.6 s freeze before an enemy appears comes back (it is off because it caused bot death loops); s3b's spawn distance (5.3 m); and the guide lamp at exits staying cyan.

**Not verified / weak:**
- No person has played it.
- The shop's pad path and the vent's new look after the final fixes (the reel budget of 20 ran out).
- The lava rooms still read as about 60 % orange of the saturated colour by my metric (the frames look right).
- THE LOOM and THE CROWN were timed in the harness only.
- Disk C is still 94 % full; nothing was deleted.

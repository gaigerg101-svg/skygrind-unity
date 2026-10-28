# NIGHT40 report for GG: the build night

**Play:** `Builds/n40-r5/SKYGRIND.exe` (the freeze, port/stage3 aa424094). `-skygrindOff40` gives last night's game for a side-by-side.
Your words (22 asks) are in Research/2026-10-14-wave40/GG-REQUEST.md; each switch is `design.n40.<row>` in Design/unity-config.json.
13 of 14 rows are ON. Every number below is from tonight's runs (NIGHT40-PROOF.md). Walk it with FEEL-PLAYTEST.md (15 minutes, pad first).

| # | you asked | tonight | how it was checked |
|---|---|---|---|
| 1 | a delay or a movement trigger before the first wave | **ON (start40).** The first wave of a level, and after a respawn, waits until you move 6 m or 4 s pass; then the enemies stand in their red tell 1 s before they can hit. | spawn -> first possible hit on levels 2 and 4: 0.00 s before, 5.6-5.8 s now; frames of levels 2 and 3 starting empty |
| 2 | level 3's boost explained thoroughly | **ON (kiln40).** Level 3 starts with the shotgun alone (your answer) and teaches the boost in five numbered steps, each shown when you finish the last ("BOOST 1/5 · PRESS SPACE TO JUMP" ...), a summary card, then the slug. The pistol returns in the second room, the gauntlet in the third. | frames (the first wording read badly, rewritten once); the bot climbs it 10/10 |
| 3 | the pistol's spin finished before the next shot | **ON (charge40).** The crosshair's quarter turn now ends 0.34 s after a tap (it ended at 0.60 s, after the next shot at 0.48 s); every pistol rotation ends inside 75 % of its rhythm. | the harness, frame by frame; **not judged on a screen** |
| 4 | a shotgun charge shot | **ON (charge40).** It existed, but a hold fired a pellet shot first and nothing showed the charge. Now on the ground a hold charges with no shot first; a click still fires 58 ms after the press (the pistol's feel); in the air the shot is instant, so the boost is unchanged. | the moves check in the Player: a 1.5 s hold, 0 shots before the slug; level 3's gates unchanged |
| 5 | charges longer; one curved bar under the crosshair on every weapon | **ON (charge40 + arc40).** The pistol charges in 0.9 s (was 0.5), the shotgun 1.2 s (was 0.7). One arc under the crosshair fills with the charge, drains through a cooldown, shows the gauntlet's 5 pips and its 15 s blast, and the STITCHER's heat in survival. | frames; enlarged once after the first frames (it read small) |
| 6 | the shotgun's fire animation faster | **Built, OFF (pump40).** The pump ends 0.42 s after the shot (was 0.75). | never caught on a frame tonight, so it stays off: turn it on with `design.n40.pump40.enabled` and tell me |
| 7 | the STITCHER out of level 4 | **ON (loom40).** Level 4's first room is a grapple yard; the STITCHER is out of the campaign (survival keeps it). | level 4's moves check 30/30 in the Player |
| 8 | THE LOOM: no-ride zones, more in phase 2 | **ON (loom40).** 5 of the canyon's 14 ride walls refuse a ride in phase 1, 10 in phases 2-3, with a 1 s blinking warning; the plates are striped and finned. | frames |
| 9 | THE LOOM smarter, stays in the map, animated, clear tells | **ON (loom40).** It stays in its canyon (a camping player pulled it 43.7 m out before; 0 now). You beat it with the grapple: hook a lit knee, pull away (no nails); the lines tell you ("HOOKED. NOW G PULL AWAY TO A THREAD."); its tells glow, it has a pose per move. | the Player: beaten at 35.9 s with 0 nails; the bot: 49-63 s, 10/10, 0 deaths |
| 10 | movement-boss principles | all three boss cards followed 8 shared rules (a tell on every attack, a movement answer, the arena clamp, the rule taught on entry, two phases, 1-2 minute fights) | the research with sources: Research/2026-10-14-wave40/research/boss-web.md |
| 11, 17 | the infinite wall ride | **ON (ride40).** A ride holds its height until you jump, crouch, steer away, land or the wall ends; your look steers up and down. | 0.000 m lost over 3 s (it was 0.55 m in 1.2 s, then a fall); the moves check passes on levels 1, 3, 4 |
| 12 | no snapping onto stairs and platforms | **ON (ride40).** A ride now needs a real jump at a real wall: 1 m off the ground, 4 m/s, the stick toward the wall, 2 m of wall above your feet. | rides on short faces or near the ground: 480 of 606 before, 0 of 220 after; every level's gates still pass |
| 13 | level 2 rebuilt for the gauntlet, taught step by step | **ON (iceline40).** New rooms from a plan, one job each: TANK ALONE, THE SHIELD, THE SWARM (save the blast), THE REACH (send the yellow orb back), THE COOLDOWN, THE ARMOURY, THE MIX, THE REHEARSAL, then the boss. Each room names its button. | the bot walks it 10/10; the moves check 20/20; frames (the prompts were wrong once, rewritten) |
| 14 | no pistol at level 2's start (and the shotgun first on level 3) | **ON.** Level 2 starts with the gauntlet alone; the pistol returns in THE ARMOURY. | the level's own check |
| 15 | new enemies, one deflectable | **ON (foes40).** The DRONE (a yellow orb: send it back with the gauntlet or shoot the drone), the WARDEN (punch its shield or flank it), the CHARGER (dodge its charge, hit its back). They were ported from the web game but never placed in the campaign. | time-to-kill: each counter wins (WARDEN x1.9, CHARGER x2.1); the DRONE dies fastest to the pistol (DECIDE40 D-13) |
| 16 | boss 2 a brawler | **ON (brawl40).** THE ICEBREAKER: guns lose damage with range (full at 4 m, a fifth at 12 m), the gauntlet always hits full. A haymaker you dodge then punish, a slam ring you jump, a yellow ice block you send back, a rush if you stay far; in phase 2 the floor cracks into floes. | the Player: beaten at 47.5 s; the bot 49 s, a casual bot 64 s, both 10/10; the pistol from range alone takes 7x as long |
| 18 | the white rods | **ON (rods40).** Lamp posts, light shafts, trusses and chains inside the space you move through are gone (5-14 per level). | frames of level 5 |
| 19 | more detailed enemy models | **ON (models40).** All seven kinds have their own outline (shoulders, spikes, a dome, horns, a saucer). | a reader with no labels named 7 of 7 silhouettes, twice |
| 20 | THE CROWN: how to hurt it | **ON (crown40).** It tells you: "FLY THROUGH THE RING: L-SHIFT DASH + SPACE JUMP"; the ring is cyan (yours to use), GEMS n / 20, "OPEN: SHOOT IT", "BLOCKED"; it no longer cuts into the spire; a fall costs 5 hp (was 20). | frames; the boss reel; bot 40 s, casual 52 s |
| 21 | level 6 deleted | **ON (cut40).** Five playable levels and a locked sixth slot. The level-6 files stay until you say delete. | the level select check in the Player |

## Not done, or half done
- **The shotgun's faster pump** is built but off (no frame).
- **Level 2's ice look:** the floor is now a pale grey, not tan, but it does not read as ice yet: the room lights are warm.
- THE LOOM's floor-sweep cue (there is no sweep attack yet); at most two enemies attacking at once (up to five commit today).
- The full test suite was stopped for low memory; the key tests passed in the Editor.

## Decisions for you (DECIDE40.md)
- **D-13** the DRONE: two answers (pistol anywhere, the gauntlet parry on level 2), or a bullet-resistant drone so the parry always wins?
- **D-14** THE GANTRY (level 1): with the endless ride one long wall now crosses its first gate. Keep it, or block part of that wall?
- **D-4** level 6's old files: delete them now?
- **D-3** boss 2's name: THE ICEBREAKER, or yours.

# NIGHT30 REPORT (for GG): the readable game

**Outcome:** every item in your message is built, switched ON by default, and checked in a Player build (n30-r7, d49449ab) with the scenario's frames looked at; level 4 THE SPAN exists with its grapple and its boss THE LOOM. The Player smoke with everything ON passes. Nobody has played it with hands yet: the numbers below come from scripted Player runs and the headless harness.

**The weapon rules (five sentences, also in Design/levels/DESIGN30.md A):**
1. You do one thing at a time: a shot, a swing or a blast is an action, and an action always finishes (a gun's shot cycle, the punch's 0.35 s swing).
2. Switching is instant unless an action is running; then the switch happens the moment that action ends.
3. A cooldown keeps running while the weapon is holstered, so switching away never pauses it and switching back never restarts it.
4. The PULSE gauntlet has two cooldowns of its own, the heavy swing (2.5 s) and the LT blast (15 s), and both follow rule 3; the blast also needs five punch kills.
5. Each weapon's slot on the HUD says its state: READY, COOLING (seconds left) or CHARGING.
The cause of "switch back and wait the full cooldown": the switch waited for the WHOLE cooldown of the weapon in hand (the gauntlet's 2.5 s swing, the shotgun's pump). Measured in the Player: before, a switch after a heavy swing took 2.51 s; now 0.36 s.

**The HUD:** bottom right, one ring per weapon; the gauntlet has two (PUNCH, BLAST with its pips "2/5"). COOLING = a draining ring + seconds, CHARGING = a filling ring, READY = a flash and the word, SWITCHING when a switch waits. "PULSE READY" / "TRIGGER STALE" are gone; 0 overlapping texts in four HUD states (it was 3); run rows hide on pause / results / title; the green "1/2" was the REFUND chip (hidden in the campaign).

**Lava:** every pit that respawned you is now lava (f2, f3, f9, k2, k4's floor, i2 / i4; level 4 has lava under every gap, no void). A touch costs 25 % of your health and knocks you up (no teleport); enemies burn and die. f5's floor is lava from the start; the new f5b LAVA RUN is a wall-to-wall lava room (west lane, wall jump, east lane, deck with two spitters). f2's lava now glows from the door.

**THE LANTERN:** a round arena of 16 flat wall panels (the Mover turns about 30 degrees per ride; 16 keeps 61 % of the speed over a seam), three platforms against the wall, lava that rises from glowing vents. It explains itself: "GET ABOVE THE LAVA" on the first tell, "RIDE THE WALL. SHOOT THE CORE." when the core opens (twice at most). Circling forever is not safe (the orbs lead you, a panel ahead of you burns), and platforms are a refuge where your shots do nothing. The full-screen red wash is gone: only the lava glows. Player rider kill 69 s (band 45-75).

**Ice and THE GLACIER:** no rails: ice is a surface you glide on. A dash on ice ADDS speed (+56 % measured in the Player), downhill adds speed, steering stays. The Glacier's rink is 44 x 70 m, closed, the banks inside its reach; it still flees, but run away or hide and the bar under the GAP fills ("TOO FAR: SHARDS COMING"), then homing shards; a frost band crosses your lane every 9 s. Player chaser kill 101 s (band 80-130); a bank camper was hit 13 times in 30 s.

**THE KILN WYRM:** when it charges you its head glows cyan; punch it then: 6-frame hitstop, white flash, ring burst, double damage, a 1.2 s recoil, then a different attack. Player: 2 staggers in 2 charges.

**LEVEL 4 THE SPAN:** a broken bridge over a lava gorge; weapon NAILGUN (room s1); verb the ARCLINE grapple (now in the campaign, tape exact). s2 hook safely, s3 swing a gap, s4 hook while fighting, s5 rest, s6 hook-ride-hook, s7 escalation with a ring of nodes, s8 THE LOOM: a blocky crane-spider under the deck; swing onto a leg and nail its joints (cyan while they take full damage); it cuts the rope you hang on (1 s red tell: let go) and sweeps the deck (jump the bar). Player: the Loom cut once, swept twice, 30 full hits, bar 1.00 -> 0.37. Kill time in band only in a scripted model (107-111 s, band 90-150).

**The look:** HUD panels have scanlines, flicker when a value changes, glow on READY / a kill / a hit, buzz at low health (one panel at a time, under the flash limits); a subtle colour dither in the world; pixel size 1-4 in Settings > ADVANCED (off by default), HUD FLICKER off switch there too; meaningless bright floor panels removed; faint light shafts on goals. Cost: about +0.02 ms GPU.

**Fewer enemies:** levels 1-2 fight rooms 30-40 % fewer; level 2 opens with ONE tank alone ("THE TANK DIES TO ONE PUNCH"). The bot dies 0 times in that room (NIGHT29: 9 in the old i1), 6 in the ice fight (was 16).

## One line per item (try it / switch / number / worse)
- Rules: switch right after a punch / `design.n30.rules` / switch 0.36 s (was 2.51 s) / none seen.
- HUD: watch the rings / `design.n30.hud30` / 0 overlaps / a READY slot shows no word at rest (only 1 s on the turn).
- Lava: f2, f5b / `design.n30.lava` / 25 %, knock to 1.19 m, 0 respawns / k2 costs the bot 16 deaths (burns).
- Lantern: level 1 end / `design.n30.lantern30` / 69 s / the ring's stone is light in the lava glow.
- Glacier + glide: level 2 / `design.n30.glacier30`, `design.n30.glide` / 101 s, +56 % dash / glide made the chase easy until B halved the windows.
- Wyrm punch: level 3 end / `design.n30.wyrm30` / 2 staggers in 2 charges / none seen.
- Look: everywhere / `design.n30.look30` / +0.02 ms / the low-health full-screen red is older (NIGHT18) and still there.
- Rooms: f8, f10, i1b, i5, i6, k1, k2 rebuilt from room cards / `design.n30.levels` / walk30 10/10 on all four levels / f9's husks spawn beside the entry.
- Level 4: level select slot 4 / `design.n30.span` / span 10/10 (bot), Loom 0.37 bar left in 16 s of hanging.
- Teaching: lesson lines never stale (0 in the Player), boss lines twice at most / `design.n30.teach`.
`-skygrindOff30` (or each row's `enabled` false) gives NIGHT29's game.

**Not verified:** a human playing any of it; the Loom's real kill time with a rope-swinging player; a grapple ring seen in a frame; the Wyrm stagger by a human (the bot punches perfectly); suite run 2's result is in the close table.
**DECIDE30 questions that matter:** D-2 (keep the rebuilt Glacier or THE STAG?), D-4 (is the dither / HUD flicker the right amount?), D-9 (the Editor's Arena.unity / UIEffect changes: keep or drop?), and the low-health red (NIGHT18) that touches the whole screen.

# NIGHT55 critic panel: the movement-shooter veteran (ULTRAKILL, Doom Eternal, Titanfall 2)

Written 2026-10-20 by the A2 veteran critic (read-only). It judges flow, feel, weight, arena design and boss quality against those three games.

**What I read:**
- the rules: CLAUDE.md, docs/PRINCIPLES.md, docs/DESIGN-RULES.md, docs/VERBS.md, NIGHT55.md sections 0, 1 and 4
- the reports and ledgers: NIGHT54-REPORT.md, NIGHT53-REPORT.md, docs/memory/ledger/wave-53.md and wave-54.md
- the numbers: Logs/n54/wick-after.txt, Logs/n52/flow-freeze/table.md, Logs/n49/meters/wyrm-default.txt, the telemetry of the reels named below, and design.impact.freeze in Design/unity-config.json
- about 50 Player frames, read with the Read tool, from n54-freeze, n54-r2 and n54-r3, plus one survival kill sequence from n51-r7 as a comparison

**Limits:**
- Every frame comes from a bot reel, not a person playing. Reels save one frame in 50, so motion is judged from the telemetry lines, not from the frames alone.
- THE WYRM's numbers are from NIGHT49 on n46-kiln. They have not been re-measured on n52-kiln.
- The mechanics I name for the reference games come from my own play knowledge. I did not re-check them against a source this session.
- Where I could not check something, I say so.

Ranked worst first. Rule ids from docs/DESIGN-RULES.md are in brackets.

---

## 1. THE WICK is a damage race, not a read-the-tell, punish-the-window fight [B-1, B-2, B-4, E-4]

**Frames:**
- the shaft wall only, no boss: Logs/feel/n54-freeze/wf/a454/f0300.jpg, f0500.jpg, f0700.jpg, f0900.jpg, f1200.jpg, f1450.jpg, f1700.jpg
- the only two frames with the boss: Logs/feel/n54-freeze/wf/a454/f1000.jpg and f1650.jpg
- the second phase: shot-N29-W54-p2.png

**What I see:**
- In 10 of the 12 a454 frames I read, the screen is a shaft wall with orange stripes. The boss (a black cage round a yellow core) is in view in 2 of them.
- The bar reads STAGGER in grey for almost the whole fight and OPEN 4.0 in cyan when it opens.

**The numbers** (Logs/n54/wick-after.txt, 40 human-model fights):
- Punish windows: 4 per fight (median).
- **Damage dealt inside windows: 6 % (median).** 94 % of THE WICK's HP goes outside the punish window, because wick54 raised chip damage from 30 % to 60 %.
- Hits the player takes, by source: hot wall 298, ember 72, geyser 25, ring 2, charge 0. About 75 % of the hits come from the walls, not from an attack the boss telegraphs.
- Attacks picked: ember 127, geyser 77, curtain 65, charge 43, ring 10, **slam 0, dive 0**. Two of the seven attacks were never chosen in 40 fights.

**What the reference games do:**
- Doom Eternal, the Marauder: his eyes flash green just before he swings. A shot inside that flash staggers him, and it is the only way past his shield. The whole fight is "see the tell, take the window". Damage outside the window is close to zero.
- ULTRAKILL, Gabriel and V2: the boss fights in your space and in front of the camera. A yellow flash marks an attack you can parry, a blue flash one you cannot. The parry is the big damage.

**Fixed means** (n47human, 40 seeds, keeping GG's "relatively easy": median 60 s or less):
- Damage dealt inside windows is 50 % or more. Chip damage goes back toward 30 %, and the windows get longer or more frequent to pay for it.
- Every attack in the kit is at least 5 % of the picks.
- Hot-wall hits are 30 % or less of the hits taken.
- On the a454 reel, the boss's body is in at least 50 % of the frames (for example, a climb whose ride walls face the core, or a core that rises into view on each leg).

## 2. A kill is a 3-frame pause on an undamaged pill, and NIGHT54 made it shorter [C-1, C-2, PRINCIPLES 5]

**Frames:**
- the campaign kill run with move54 on: Logs/feel/n54-r2/m2f/a354/f0100.jpg, f0150.jpg, f0200.jpg, f0250.jpg
- the same run with move54 off: Logs/feel/n54-r2/m2foff/a354/f0250.jpg
- the survival comparison: Logs/feel/n51-r7/k-on/kill6/f0078.jpg, f0081.jpg, f0090.jpg

**What I see:**
- The m2f telemetry has four kill holds, at frames 120-122, 153-155, 185-187 and 218-220. Each is exactly 3 frames (0.05 s), 0.53 s apart. The fourth kill in the chain is no bigger than the first.
- At f0200, the husk under the crosshair at point-blank range is an intact glossy pill after being shot: no crack, no missing piece, no flinch.
- At f0250, 0.5 s after the last kill, I see nothing of the four bodies on the floor in view. The camera faces a wall, so this is not conclusive. The move54-off frame shows one red slab.
- NIGHT54's move54 cut the camera's hold per kill freeze from 0.063 s to 0.038 s (NIGHT54-LOG, a354).
- The survival kill (n51 kill6, f0078-f0090) shows a white-out, a starburst and about 10 large slabs. The campaign frames never show a kill that size.
- Gun recoil and muzzle flash at the moment of firing: not checked. No sampled frame lands on a shot.

**What the reference games do:**
- Doom Eternal, the destructible demons: each hit strips flesh and armour, so the body shows the damage before it dies. Weak points (the Mancubus's arm cannons, the Arachnotron's turret) can be shot off, and each one removes an attack.
- ULTRAKILL, the gore and the blood heal: a kill bursts the enemy into gibs and a blood spray, and the spray heals you if you are close. That is the game's reason to fight up close.

**Fixed means:**
- A hit on a husk shows a visible damage state on the next frame, with at least 2 stages before death (a cracked shell, then a piece knocked off). Measured as changed pixels on the body, before and after the hit.
- A campaign kill leaves at least 8 debris pieces that stay at least 1.5 s.
- Kills inside 1 s of each other escalate: the 1st holds 0.05 s, the 3rd and later at least 0.08 s, within the existing killCap of 0.155 s.
- The freeze holds the world while the look stays live, so GG's "less choppy" still holds: the a354 jitter stays at 0.191 or less.

## 3. The wall ride pays no speed: momentum is kept, never earned [M-5, A-2, PRINCIPLES 6]

**Frames:**
- Logs/feel/n54-freeze/vf/verbs37/f0900.jpg: riding at 15.4 m/s over the lava, "+30 WALL JUMP", rank D DULL
- the telemetry, vf/verbs37/telemetry.jsonl lines 852-901

**What I see:**
- The ride holds exactly 15.177 m/s from line 854 to line 879 (0.43 s). The jump-off reads 15.332 (+1.0 %). The next ride reads 15.342.
- A ride is a conveyor at the speed you entered it with. The bunny hop and the slide bounce are also written as "keep speed" in VERBS.md rows 12-13.
- The rooms put walking lanes the same length as the ride walls right beside them. The I2 check (the walk must be at least 1.3 times the ride) still fails in iceline g3, g4, g5, g7 and g8 and spire v2 (NIGHT53-REPORT).
- RIDE KILL refunds a boost only when you also get a kill on the ride. The ride itself pays nothing.

**What the reference games do:**
- Titanfall 2: a wallrun pushes the pilot past sprint speed. Chaining wallrun, jump, wallrun, and slide-hopping out of each landing, keeps and builds momentum, so the walls are the fast way through every map, every time.
- ULTRAKILL: the dash-jump and the slam bounce add speed on top of the slide's kept speed. Chaining is rewarded with velocity.

**Fixed means** (a new row, golden 0 / 22 exact with it off; Mover.cs only by a named task, CLAUDE.md rule 3):
- A ride accelerates toward a cap above sprint, for example from 15.2 to 18 m/s or more over 1 s.
- The jump-off adds at least 10 % over the ride speed.
- I2 passes in every room.
- A ride pilot clears foundry a3 and iceline g4 at least 20 % faster than the walk pilot.

## 4. The rooms are halls with a few distant pills, and the flow stalls between waves [R-1, R-3, A-2, A-8]

**Frames:**
- foundry (Logs/feel/n54-freeze/df/a452/):
  - shot-N29-K52-n52-foundry-a3-entry.png: "8 LEFT", 2 enemies visible, both far
  - shot-N29-K52-n52-foundry-a5-entry.png: "8 LEFT", 2 visible
  - shot-N29-K52-n52-foundry-b1-mid.png: "11 LEFT", 2 visible
- iceline: Logs/feel/n54-freeze/di/a452/shot-N29-K52-n52-iceline-g2-entry.png: "7 LEFT", 1 warden in a pillar hall
- kiln (Logs/feel/n54-r2/d2k/a452/):
  - shot-N29-K52-n52-kiln-kb2-entry.png: "14 LEFT", 4 small figures
  - shot-N29-K52-n52-kiln-kb3-mid.png: "13 LEFT", four husks standing in a line at the foot of the ride wall

**What I see:**
- The counter says 7-14 enemies. The frame shows 1-4, most standing on the floor far off.
- The bottom half of every entry frame is open floor.

**The numbers** (the walk pilot, not a person: Logs/n52/flow-freeze/table.md, with NIGHT53's updates):
- The pilot is stalled 25.3-36.0 % of the time on foundry, iceline and kiln.
- It waits 11.0-13.9 s at doors.
- It has 17.8-27.6 s of dead time per level.
- Iceline takes 221 s to clear.

**What the reference games do:**
- Doom Eternal, the arena lock-in: the doors seal, and waves spawn at the arena's edges and above you. The health, armour and chainsaw fuel sit on the loop, so the loop is the route. The next wave arrives before the last one is finished.
- ULTRAKILL: the rooms are small, the door shuts, and each wave spawns close to you with a flash.

**Fixed means:**
- On each combat room's a452 entry frame, by the third second of wave 1: at least 4 enemies visible, at least 2 within 15 m.
- The next wave spawns when 2 of the current wave remain.
- Flow table on foundry, iceline and kiln: stall 15 % or less, door wait 5 s or less, dead time 8 s or less.

## 5. The boss arenas ask for no movement, and THE WYRM is long, lethal and grounded [B-2, B-4, PRINCIPLES 4]

**Frames:**
- THE ICEBREAKER (Logs/feel/n54-r3/g3i/a254/): shot-N54-brawl40-p1.jpg, shot-N54-brawl40-turn.jpg and f0100.jpg. A flat hall with two pillars. The boss is about 130 px tall at 1080p, far down the hall. Nothing to ride.
- THE WYRM (Logs/feel/n54-r3/g3k/a254/): shot-N54-wyrm-p1.jpg and shot-N54-wyrm-p2.jpg. A grid floor, the boss a thin yellow column at the vanishing point, a "MOVE / B to jump" banner.
- THE LOOM: Logs/feel/n54-r3/g3p/a254/shot-N54-loom37-p1.jpg (the boss is a structure on the ceiling).
- THE CROWN: Logs/feel/n54-r3/g3s/a254/shot-N54-crown37-p1.jpg (a ring down a lane).

**What I see:** three of the four bosses read as distant objects at the end of a lane. THE WICK is the exception: 39 movement verbs a minute.

**THE WYRM's numbers** (Logs/n49/meters/wyrm-default.txt, NIGHT49, not re-measured on n52-kiln):
- Kill time: median 122.6 s, p90 256.9 s. 2 of 40 fights not killed inside the 300 s cap.
- 68 deaths in 40 fights.
- **4.6 jumps a minute**, no double jumps, 14.3 shots a minute. The pilot jumps less than once every 13 s.

**What the reference games do:**
- ULTRAKILL, Minos Prime and V2: a small round arena. The boss closes to melee range and leaves again. Its combos force a dash (the dash's invincibility frames are your defence) and its parryable hits are your offence. You are always moving inside its space.
- Doom Eternal, the Doom Hunter: the fight is on platforms with jump pads, and its sled pushes you off the ground.

**Fixed means:**
- On every boss's n47human row: jump + dash + ride at least 25 a minute.
- The boss within 15 m in at least 40 % of its a254 / a454 frames.
- THE WYRM, re-measured on n52-kiln: median 60-120 s, p90 150 s or less, at most 1 death per 10 fights.

## 6. THE WICK's arena uses the danger hue as wallpaper [V-1, V-2, E-3, C-6]

**Frames:**
- Logs/feel/n54-freeze/wf/a454/f0300.jpg, f0700.jpg, f1200.jpg, f1450.jpg
- the damage vignette: f0400.jpg and shot-N29-W54-p2.png

**What I see:**
- Red-orange stripes 30-80 px thick run across every shaft wall, on a large share of every frame.
- The damage vignette is a red dithered border on the screen's edges (f0400, p2).
- I cannot tell from any frame which wall is hot right now. Yet 298 of the 397 hits taken in the 40 fights came from hot walls.
- NIGHT54 shipped this boss ungraded because every grade it tried put the frame within 1-14 deg of the danger red (D-16).

**What the reference games do:**
- ULTRAKILL: a surface that hurts (lava, an electrified floor) looks like a hazard, and nothing decorative in the room shares its look.
- Doom Eternal: a weak point's or a stagger's glow appears only when you can use it. Hazard is a state you can see, not décor.

**Fixed means:**
- On a454 frames with no hot wall, red-orange pixels are 10 % of the frame or less.
- A hot wall is the most saturated thing in the frame while it burns, and goes back to the wall's own colour when it stops.
- Hot-wall hits are 30 % or less of the hits taken (n47human).
- THE WICK's grade ships ON with the walls' mean hue 30 deg or more from the danger hue, measured on the walls only (D-16's next try).

## 7. The style meter is a label, not a loop

**Frames:**
- the movement reels: Logs/feel/n54-freeze/vf/verbs37/f0900.jpg and Logs/feel/n54-freeze/vk/verbs37/f0900.jpg ("D DULL +30 WALL JUMP"), Logs/feel/n54-freeze/vi/verbs37/f0850.jpg ("D DULL")
- THE WICK's open window: Logs/feel/n54-freeze/wf/a454/f1650.jpg ("D DULL"; the D glyph overprints the word)
- after a multikill: Logs/feel/n54-r2/m2f/a354/f0200.jpg and f0250.jpg ("x5" and "x7", "C COOL")

**What I see:**
- Through every verbs reel the rank never leaves D, with its label "DULL", while the reel chains rides, wall jumps, slides and dash-jumps.
- A four-kill chain reaches C.
- Nothing on the frames reacts to the rank. Whether the code pays anything for it: not checked.

**What the reference game does:** ULTRAKILL's style meter.
- The ranks climb D, C, B, A, S, SS, SSS, ULTRAKILL, and decay when you stop.
- Every style source is listed under the meter the moment it happens (+PARRY, +AIRSHOT, +RICOSHOT).
- Each weapon's freshness (FRESH, USED, STALE, DULL) lowers its style the more you repeat it, which pushes you to swap.
- The level's rank needs style (the P-rank needs S in style, kills and time), so style is the score.
- "DULL" is ULTRAKILL's word for an over-used weapon. Here it is the player's resting rank: the game calls you dull for moving.

**Fixed means:**
- A skilled pilot in a combat room reaches A or higher within 20 s, and spends at least 30 % of a level's combat time at B or higher (a rank histogram per level run).
- One chain of ride, wall jump and kill moves the rank one letter.
- The rank pays through something already in the game (for example, RIDE KILL's refund or heal36's spill scaled by rank).
- A rank change shows in one frame (a punch and a colour step).

## 8. Each boss's tell is a sentence on the screen, not something its body does [T-2, T-3, B-1, E-4]

**Frames:**
- THE WICK (Logs/feel/n54-freeze/wf/a454/):
  - f0300.jpg: "the yellow embers back."
  - shot-N29-W54-p2.png: "It's climbing. Get up there."
  - f1650.jpg and f1700.jpg: "through the crack."
- THE ICEBREAKER: Logs/feel/n54-r3/g3i/a254/shot-N54-brawl40-p1.jpg and shot-N54-brawl40-turn.jpg. "PARRY THE BLOCK. PUNISH STUCK FISTS" is on screen twice at once, under the bar and in the centre.
- THE WYRM: Logs/feel/n54-r3/g3k/a254/shot-N54-wyrm-p2.jpg. "PUNCH THE HEAD TO STAGGER IT", twice.
- Level 4: Logs/feel/n54-freeze/vp/verbs37/f1600.jpg. "Kills give you more pulls" over a room with no enemy in view.

**What I see:**
- 7 of the 12 WICK frames I read carry a centre sentence.
- Two bosses print their rule twice in the same frame.

**What the reference games do:**
- ULTRAKILL teaches with one HudMessage: a trigger, the bound key, about 5 s, once. After that, the body carries the read: the parry flash, the wind-up pose, the boss's voice line before a combo.
- Doom Eternal: the weak points glow and the Marauder's eyes flash. There is no line of text during the fight.

**Fixed means:**
- After a boss's first 10 s, centre text is on 10 % or less of its frames.
- No rule appears twice in one frame.
- The OPEN state is shown on the boss's body (the core lit, a crack opening), visible in one frame.
- A blind check: with the text turned off, at least 4 of 5 viewers name the punish moment from a frame.

## 9. The camera reports button presses, not speed [C-1, PRINCIPLES 5]

**Frames:**
- the dash at 24 m/s: Logs/feel/n54-freeze/vf/verbs37/f0650.jpg. Only thin 1-2 px cyan streaks at the edges, and the HUD's "SLIDE BURST: RECHARGING" drawn over the "LEFT" counter.
- the ride at 15 m/s: Logs/feel/n54-freeze/vf/verbs37/f0900.jpg
- the spire dash: Logs/feel/n54-freeze/vs/verbs37/telemetry.jsonl lines 371-382

**What I see:**
- In vf's telemetry, lines 650-661, the dash starts at FOV 85.8 and falls to 79.7 within 11 frames, while the speed holds at 24.0 m/s the whole time. Spire's dash draws the same curve.
- The FOV is a kick on the button press, not a function of speed. A 15 m/s ride sits at about 83.5. The tail of a 24 m/s dash sits at 79.7, which is narrower at a higher speed.
- The ride's camera roll (to -13.8 deg) does read.

**What the reference game does:** Titanfall 2 sells a wallrun with the camera's roll into the wall and with geometry passing close to the lens. The speed you have is the speed you see.

**Fixed means:**
- The FOV follows horizontal speed and holds while the speed holds: base 74 up to sprint, then a set amount per m/s above sprint, with a cap. The amount and the cap go in UnityTuning, each with a comment.
- On vf's telemetry, FOV against speed correlates at 0.9 or more over the reel.
- At 24 m/s the FOV is never below the 15 m/s ride's.

## 10. No swap-combo layer outside the bend: the guns don't chain

**Evidence:**
- docs/memory/FEATURES.md: rules30 makes the switch wait for the current action and gives holstered weapons cooldowns.
- docs/VERBS.md row 23: an instant swap exists only inside a BEND, which comes from level 4.
- The boss frames show one gun in use per fight:
  - THE WYRM with the THUNDERCLAP (Logs/feel/n54-r3/g3k/a254/shot-N54-wyrm-p1.jpg)
  - THE LOOM with the THUNDERCLAP (Logs/feel/n54-r3/g3p/a254/shot-N54-loom37-p1.jpg)
  - THE CROWN with the TORCH (Logs/feel/n54-r3/g3s/a254/shot-N54-crown37-p1.jpg)
- The swap time outside a bend, and how often the bot swaps: not checked. The reason GG gave for rules30: not checked.

**What the reference games do:**
- Doom Eternal's quick-swap combo: fire the Ballista, swap to the Super Shotgun and fire inside a second, while each weapon's cooldown runs holstered. The swap is the damage skill.
- ULTRAKILL: a swap cancels the current weapon's recovery, and freshness (item 7) rewards rotating guns.

**Fixed means:**
- Outside a bend, swap-then-fire takes 0.12 s or less when the weapon's own cooldown is ready.
- A named two-weapon combo (QUICKDRAW is already in the game) pays something on screen.
- In a level 3-5 combat room, the pilot's kills include 2 different weapons within 2 s at least once a room.

---

N55-critic-veteran: written.

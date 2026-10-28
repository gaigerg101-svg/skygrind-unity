# NIGHT55 A2: the first-time player critic

Role: someone who has never seen SKYGRIND, judging what is unclear, unreadable or confusing in the first 10 minutes and in each boss.
Method: I judged Player frames first and read no night log or report. After writing the findings I grepped the source for three labels
I could not identify (STAGGER, SLIDE BURST, the LT word on the gauntlet). Pixel sizes come from coordinates read on the 1920x1080
frames ("px per character" = text width / character count). Colour and contrast figures are by eye and marked that way. No tool measured them.

Frames read (36, all 1920x1080 unless noted):
- Level 1 verbs: `Logs/feel/n54-freeze/vf/verbs37/` f0000, f0300, f0600, f0900, f1200, f1500
- Rooms: `Logs/feel/n54-freeze/df/a452/` shot-...-foundry-a3-entry, -a5-entry, -b1-entry; `di/a452/` shot-...-iceline-g2-entry, -g4-entry
- Levels 2-5: `vi/verbs37/f0000`, `vk/verbs37/f0000`, `vk/verbs37/f0600`, `vp/verbs37/f0700`, `vs/verbs37/f0000`
- THE WICK: `Logs/feel/n54-freeze/wf/a454/` f0050, f0150, f0600, f1200, shot-N29-W54-p1.png, shot-N29-W54-p2.png;
  `Logs/feel/n54-freeze2/gf/a254/` f0000, f0060, f0120, f0180
- THE ICEBREAKER `Logs/feel/n54-r3/g3i/a254/` shot-N54-brawl40-p1, -p2; THE WYRM `g3k/a254/` shot-N54-wyrm-p1, -p2;
  THE LOOM `g3p/a254/` shot-N54-loom37-p1, -p2; THE CROWN `g3s/a254/` f0200, shot-N54-crown37-p1;
  `Logs/feel/n54-freeze2/gs/a254/` shot-N54-crown37-turn, -p2
- First boot: no menu, title or boot frame exists under Logs/feel or Logs/shots from n50 on. The newest one is
  `Logs/feel/n33-r6/hudshop/hud33/shot-N30-hud33-720-title.png` (NIGHT33, 1280x720), read as a reference only.

Many more frames were requested and came back empty (the image request limit), so a454 has 6 of 38 frames read, not all 38.

---

## 1. No boss says what it wants, and the first one says the least

- **Frames:** `n54-freeze/wf/a454/f0050.jpg`, `f0150.jpg`, `f0600.jpg`, `f1200.jpg`, `shot-N29-W54-p1.png`, `shot-N29-W54-p2.png`;
  `n54-freeze2/gf/a254/f0060.jpg`, `f0120.jpg`, `f0180.jpg`; `n54-r3/g3p/a254/shot-N54-loom37-p1.jpg`, `-p2.jpg`;
  `n54-r3/g3s/a254/shot-N54-crown37-p1.jpg`, `n54-freeze2/gs/a254/shot-N54-crown37-p2.jpg`.
- **What a newcomer sees:** in THE WICK's first leg (6 frames), the only centre text is the boss's name, then "THE STACK", which is a
  place name. Nothing says "climb" or "ride the walls", and no button is shown. The bar stays full in all six leg-1 frames (LEG 1/3,
  red fill to the right end), so nothing tells me I am doing it wrong. The first hint, "It's climbing. Get up there." (p2), comes at
  LEG 2/3. It has no glyph and no arrow, and the boss is off screen. THE LOOM shows only "THE NARROWS", with no hint that you tear a
  knee with the grapple. THE CROWN shows only "THE SPIRE", with no hint that you jump-dash through the ring. Only THE ICEBREAKER
  ("PARRY THE BLOCK. PUNISH STUCK FISTS") and THE WYRM ("PUNCH THE HEAD TO STAGGER IT") name an action, and neither shows the button.
- **Rule broken:** PRINCIPLES 2 (a boss is the level's final exam of its verb, so the exam must name the verb); B-1 (LOCKED: what to
  do to open it); T-1 (the player's own bound glyph); T-3 (VERB IN CAPS + the bound input).
- **Fixed, on a frame:** in each boss's first 10 s of reel frames, at least 1 frame shows a centre card of the form
  "CLIMB [A-glyph] ride up, hit the core", with the verb in caps, the glyph at 1.2x cap height and at most 6 words. The card sits in
  the y≈275 slot for at least 4 s. THE WICK's card appears in leg 1, before the first stagger. If the bar has not moved after 20 s, a
  second line repeats the verb. Pass: 5 of 5 bosses, checked on frames.

## 2. The thing you must use is painted "danger"

- **Frames:** `n54-freeze/wf/a454/f0050.jpg`, `f0150.jpg`, `f1200.jpg`, `shot-N29-W54-p1.png` (THE WICK's climb walls);
  `n54-r3/g3s/a254/shot-N54-crown37-p1.jpg`, `f0200.jpg`, `n54-freeze2/gs/a254/shot-N54-crown37-turn.jpg` (THE CROWN's ring);
  `n54-r3/g3k/a254/shot-N54-wyrm-p1.jpg` (THE WYRM's floor grid).
- **What a newcomer sees:** THE WICK's shaft walls are banded with thick red-orange stripes, and I counted 7 to 9 bands per frame.
  The fight is about riding those walls, but I am taught that red-orange means "enemy or danger", so I stay off them. No cyan
  ride strip is on any WICK wall in these frames, while level 1's ride walls do have cyan strips (`vf/verbs37/f1200.jpg`). THE CROWN's
  ring, the thing you must fly through, is a red-orange ring with spikes about 1/3 of the screen wide. A spiked red ring says
  "don't touch". THE WYRM's whole floor is a glowing red grid, so I cannot tell whether the floor hurts.
- **Rule broken:** PRINCIPLES 3 (one colour per meaning); V-2 (cyan = the player can use, stand on or ride); V-3 (ridable walls carry
  the cyan edge strip); E-4 (red-orange = avoid).
- **Fixed, on a frame:** on THE WICK's frames, every wall the climb uses carries a cyan edge strip (hue 180-195°, ≥ 6 px tall at 1080p),
  and red-orange stays off ridable faces. The danger bands move to hazards only, or are recoloured to the arena's neutral. THE
  CROWN's ring has a cyan or white pass-through face; red-orange is kept only for the parts that hurt. THE WYRM's floor grid is
  recoloured unless the floor deals damage. A greyscale pass (E-3) puts the ring and the ride strips in the top value band.

## 3. The same screen gives two different buttons for parry

- **Frames:** `n54-freeze/di/a452/shot-N29-K52-n52-iceline-g4-entry.png` (card "Punch the yellow orb back with [ZR]", weapon panel
  "PULSE GAUNTLET / ZL PARRY"); `n54-freeze/di/a452/shot-N29-K52-n52-iceline-g2-entry.png` ("Punch the shield with [ZR]" + "ZL PARRY");
  `n54-freeze/vi/verbs37/f0000.jpg` ("[ZR]: one gauntlet swing drops a tank" + "ZL PARRY"); `n54-r3/g3i/a254/shot-N54-brawl40-p1.jpg`
  ("PARRY THE BLOCK" + "ZL PARRY").
- **What a newcomer sees:** the teaching card says parry with ZR, and the corner of the same frame says ZL is PARRY. I press one,
  it fails, and I don't know which one was the lie. THE ICEBREAKER's whole fight is "parry", and its frame shows only the ZL label.
  (Source: `HudWeapons30.cs:328` labels the gauntlet's LT "PARRY". I did not check which input actually parries in the build.)
- **Rule broken:** T-1 (the player's own bound glyph, one meaning); M-4 (one input, one meaning per state); U-1 (one visual language).
- **Fixed, on a frame:** on every frame with the gauntlet out, the parry glyph on the card matches the glyph in the weapon panel
  (a string compare in the reel: 0 mismatches). If two inputs both parry, the card says so ("[ZR] or [ZL]"). If the LT does
  something else, the panel names that action instead of PARRY.

## 4. Yellow, magenta and green turn up outside their meanings

- **Frames:** `n54-freeze/vk/verbs37/f0000.jpg`, `n54-freeze/vs/verbs37/f0000.jpg`, `n54-r3/g3p/a254/shot-N54-loom37-p1.jpg` (the
  THUNDERCLAP's yellow top plate, about 250x130 px, plus a yellow sight triangle on screen at all times);
  `n54-r3/g3s/a254/shot-N54-crown37-p1.jpg` (yellow gems, yellow TORCH plate); `n54-freeze/vf/verbs37/f1500.jpg` (yellow floor trims);
  `n54-freeze/vi/verbs37/f0000.jpg`, `n54-freeze/vp/verbs37/f0700.jpg` (magenta-pink ammo pips drawn across "ZL PARRY" / "ZL ZOOM");
  `n54-r3/g3k/a254/shot-N54-wyrm-p2.jpg`, `n54-r3/g3s/a254/f0200.jpg` (green ledge edges and a green see-through box).
- **What a newcomer sees:** level 2 teaches me that yellow means "punch it back". Then the biggest yellow thing on screen is my own
  shotgun. THE CROWN's gems are yellow squares, so do I parry them? The pink tick marks on the weapon panel strike through the
  words "PARRY" and "ZOOM", and pink is meant to mean "the rival". On THE WYRM and THE CROWN (n54-r3), the ledges you stand on are
  green, not cyan, so the "I can use this" colour changes from boss to boss. On n54-freeze2 THE CROWN's ledges are cyan again.
- **Rule broken:** C-5 (parryable = yellow, always); PRINCIPLES 3 and CLAUDE.md rule 8 (magenta is RIVAL only; cyan is player-usable);
  V-2.
- **Fixed, on a frame:** on the THUNDERCLAP and TORCH frames, saturated yellow pixels (hue 50-60°, S > 0.7) come only from parryable
  tells. The gun plates and sight move to the gun's neutral or cyan. Gems are cyan or white. The ammo pips are cyan or white and sit
  ≥ 4 px clear of the LT word (0 overlapping px). Player-usable ledge edges keep hue 180-195° on every boss grade: green edge pixels
  in the boss frames = 0.

## 5. "SLIDE BURST: RECHARGING" covers the enemy counter and names a thing never taught

- **Frames:** `n54-freeze/vf/verbs37/f0600.jpg` ("2 LEFT" visible as a ghost under the hint), `n54-freeze/vk/verbs37/f0600.jpg` (the
  counter gone), `n54-freeze/vp/verbs37/f0700.jpg` ("0 LEFT" ghosted under it).
- **What a newcomer sees:** the most useful number in a fight ("2 LEFT") is overwritten by a status about a "slide burst" that no
  card in these frames explains. The grey arc under the crosshair, on every frame, has no label. The source string calls it
  "THE LONG TICK UNDER THE CROSSHAIR" (`UnityTuning.FeelPolish.cs:49`), but the short form shown on screen does not say so.
- **Rule broken:** R-5 (a small counter shows how many are left); M-2 (a limit that bites is shown on screen, and it must be
  understandable); T-1 (taught where first needed); U-3 (hierarchy: the counter is the room's hero number).
- **Fixed, on a frame:** the room counter's box shares its rect with no other string (0 overlap px on every reel frame). The
  slide-charge state sits next to the arc it describes. The first time the burst is spent, one centre card teaches it once
  ("SLIDE BURST [glyph] refills 2 s after a slide"). After that, the arc alone shows it.

## 6. Teaching cards change size, glyph style and wording from level to level, and teach the wrong things twice

- **Frames:** `n54-freeze/vf/verbs37/f1200.jpg` ("Hold [B] on a wall to ride it": about 12 px per character);
  `n54-freeze/vp/verbs37/f0700.jpg` ("[R] to pull to the ring": 115 px for 19 characters, about 6 px per character, so ≈ 4 px per
  character at 720p, and no ring is in view); `n54-freeze/vk/verbs37/f0600.jpg` ("In the air, look straight down": about 6.7 px per
  character, no glyph); `n54-freeze/vk/verbs37/f0000.jpg` ("B to jump" as a plain cyan letter, not a glyph);
  `n54-r3/g3k/a254/shot-N54-wyrm-p1.jpg` ("MOVE / B to jump", the subline about 7 px per character, in a level-3 boss);
  `n54-freeze/vf/verbs37/f0000.jpg` ("THE FOUNDRY. Stay fast / [B] to jump, again in the air."); `vf/verbs37/f1500.jpg`
  ("[B] wall to wall, then [L] across").
- **What a newcomer sees:** jump is taught three times (level 1, level 3 again, then THE WYRM again). The slam has no button
  ("look straight down"). The grapple, the deepest verb, gets the smallest text in the game, shown while I face a blank wall. Glyphs
  come in four styles: a circle B, a bare cyan letter B, a square R, and a rounded ZR. "Wall to wall, then L across" names no verb.
  "Stay fast" is advice, not an instruction.
- **Rule broken:** T-1 (once, where first needed, the bound glyph); T-3 (VERB IN CAPS + input, ≤ 6 words); U-7 (≥ 16 px at 1080p);
  U-1 (one visual language); PRINCIPLES 2.
- **Fixed, on a frame:** every teaching card uses one template. Text measures 12 px ± 10 % per character at 1080p (≥ 8 px at 720p).
  It starts with VERB IN CAPS, includes one glyph in the shared glyph style, and has ≤ 6 words. Cards whose text contains "to jump"
  appear on level 1 only (0 on levels 3-5 and in bosses). The grapple card fires only when a grapple point is inside the centre 50 %
  of the view.

## 7. The boss bar's words read as states or as jargon

- **Frames:** `n54-freeze/wf/a454/f0050.jpg` (THE WICK, "STAGGER" under the bar while nothing is staggered);
  `n54-freeze/wf/a454/shot-N29-W54-p2.png` ("OPEN 4.0"); `n54-r3/g3p/a254/shot-N54-loom37-p1.jpg` ("LEG 1/3" on a boss that has legs);
  `n54-r3/g3s/a254/shot-N54-crown37-p1.jpg` ("GEMS 20/20" written inside the red fill: white on red-orange, by eye about 3:1);
  `n54-r3/g3k/a254/shot-N54-wyrm-p1.jpg` vs `-p2.jpg` (THE WYRM's bar is a 4 px line with a small "KILN WYRM" label in p1 and the
  full styled bar with "LEG 1/3" in p2).
- **What a newcomer sees:** "STAGGER" is on screen for the whole fight, so I think the boss is staggered and keep waiting for an
  opening. (Source: it is the name of the meter, `UnityTuning.Ui43.cs:122`. The meter itself is the thin white line under the fill.)
  "LEG 1/3" on THE LOOM reads like "hit leg 1 of 3". "OPEN 4.0" doesn't say whether that means seconds or hits. THE WYRM's bar
  changes shape between frames, and its name changes from "THE WYRM" to "KILN WYRM".
- **Rule broken:** B-1 (the bar tells the truth: the state and what to do); B-2 (phases read as beats); U-1.
- **Fixed, on a frame:** the state word shows only when it is true, as "LOCKED: <action>" / "OPEN 4 S" with a cyan countdown bar /
  "STAGGERED", and the meter label is dropped or shows only while the meter drains. "LEG" becomes "PHASE". "GEMS 20/20" sits outside
  the fill at ≥ 4.5:1 contrast. All 5 bosses use one bar template (name ≥ 28 px, fill ≥ 16 px tall), the same on every frame of a fight.

## 8. Place names take the instruction slot, and text covers the target

- **Frames:** `n54-freeze2/gf/a254/f0120.jpg` ("THE WICK" drawn across the boss's body); `n54-freeze2/gf/a254/f0180.jpg`
  ("THE STACK" drawn over THE WICK's glowing core, the thing you shoot); `n54-r3/g3p/a254/shot-N54-loom37-p1.jpg` (THE LOOM's body
  drawn through its own HP bar and "STAGGER" label, y≈110-180); `n54-freeze/wf/a454/f0150.jpg`, `n54-r3/g3s/a254/f0200.jpg`
  ("THE STACK", "THE SPIRE" in the slot where other bosses put their instruction).
- **What a newcomer sees:** I look at the centre for "what do I do" and read a room name. When the boss is in front of me, the label
  sits on its weak point, so I can't see the core I'm meant to hit. On THE LOOM, the HUD and the boss overlap so much that neither
  reads.
- **Rule broken:** T-2 (important text sits above the crosshair, never on it); B-3 (the intro is short); B-1.
- **Fixed, on a frame:** an arena name in a boss shows for ≤ 2 s at fight start, then clears. In the boss reel frames, no HUD text box
  intersects the boss's screen-space bounds (0 of N frames). The boss's start position keeps it below y = 200 at 1080p, clear of the bar.

## 9. A full-screen red wash hides every danger cue

- **Frames:** `n54-freeze/wf/a454/shot-N29-W54-p2.png` (by eye, most of the frame is red, with a noisy red vignette on all four edges);
  `n54-freeze2/gf/a254/f0060.jpg` (brown noise border); `n54-freeze/wf/a454/f0600.jpg` and `f1200.jpg` (an orange-red room with a
  red puddle and a red off-screen arrow).
- **What a newcomer sees:** during THE WICK's leg 2, everything is red. The big red beam, the red puddle and the red off-screen
  chevron are the cues that should say "avoid this", and they blend into a red screen. I cannot tell what hurts.
- **Rule broken:** U-6 (never a full-screen tint); NIGHT55 rubric line 4 (no effect over the whole screen); PRINCIPLES 3 (every
  state the player must react to has a cue they can see in one frame); E-3 (enemies and hazards are the most saturated warm objects).
- **Fixed, on a frame:** on the leg-2 frame, the centre 50 % of the frame keeps the mean luminance and saturation of the leg-1 frame
  (± 10 %). Red-orange pixels outside hazards, the boss and the HUD danger bar make up < 10 % of the frame (a hue mask on the reel
  frame). Any vignette stays within 8 % of each edge.

## 10. Numbers and words in the HUD with no legend

- **Frames:** every gameplay frame ("500" top right, unlabeled, 500 in every frame read except `wf/a454/shot-N29-W54-p2.png` = 550);
  `n54-freeze/vf/verbs37/f0600.jpg`, `f0900.jpg` ("D / DULL", "x3"); `n54-freeze/vf/verbs37/f1500.jpg` ("0 CLEAR");
  `n54-freeze/df/a452/shot-N29-K52-n52-foundry-b1-entry.png` (a red-topped crate with two red glyphs at the room's centre: is it
  hazard, cover or a buy?); `n54-freeze/df/a452/shot-N29-K52-n52-foundry-a3-entry.png` (white-cyan pill outlines inside the cyan pad
  beam: enemies spawning in the player's colour); `Logs/feel/n33-r6/hudshop/hud33/shot-N30-hud33-720-title.png` (NIGHT33 menu at 720p:
  "MODE", "ROOMS · DOORS · CHECKPOINTS · A BOSS" and "BEST GRADE D" about 11-12 px tall at 720p; no controls screen in the list).
- **What a newcomer sees:** "500" might be score, money or ammo; nothing says. A big "DULL" stays on screen while I'm learning, and
  nothing says what raises it. "0 CLEAR" reads as "zero cleared". A red box and a red slab show up with no behaviour shown. On the
  title screen (an old frame) there is no "controls" or "how to play" entry. There is no current first-boot frame at all, so the
  first 30 seconds of the game have no proof.
- **Rule broken:** U-3 (one hero number; the rest small and readable); U-7; PRINCIPLES 3 (a pickup or object shows what it is);
  E-6 (spawn tells) combined with V-2 (cyan = player-usable).
- **Fixed, on a frame:** "500" carries a word ("PTS" or a currency icon) at ≥ 16 px. The grade shows only after the first kill
  combo, or in a non-judging style. The counter reads "ROOM CLEAR". Every red-orange object in a level-1 room either hurts (and a
  frame shows that) or is recoloured. Spawn tells use red-orange, not white or cyan. A Player frame of the current title screen and
  the first 10 s of level 1 is captured each night at 1280x720, with all text ≥ 11 px at 720p.

---

Not checked: which input actually parries (ZR or ZL) in the build; the real health readout (every reel frame shows 9999); the
frame cadence of the reels (so "the bar stayed full for N seconds" is not given); the freeze2 grade on THE WYRM (only n54-r3
frames were read); a454 frames beyond the 6 listed.

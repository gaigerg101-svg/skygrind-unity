# critic-ggproxy (NIGHT55 A2): what GG keeps asking for and still cannot see on a frame

Role: the GG-proxy. Built only from GG's own words: docs/memory/OWNER.md, the quoted lines in docs/memory/CORE.md, the GG blocks at the
top of NIGHT47 / 48 / 49 / 50 / 52 / 53 / 54 / 55.md and EXT53.md, and Logs/n47/ASKS.md. Read-only; this is the only file written.

**What I actually looked at.** 26 Player frames rendered for me (the image tool's request limit dropped the other ~100 reads, so every
frame path below is one I saw), plus the reels' `telemetry.jsonl` (numbers marked "telemetry" are grep counts of those files). "By eye"
means my read of the frame, not a pixel measurement. Frames seen:
- `Logs/feel/n54-freeze/vk/verbs37/` f0100, f0500, f0950, f1400 (kiln); `vi/verbs37/f0700.jpg` (iceline)
- `Logs/feel/n54-freeze/wf/a454/` f1100, f1500 (THE WICK)
- `Logs/feel/n54-freeze/df/a452/` foundry-b1-entry, foundry-a5-mid; `dfoff/a452/` foundry-b1-entry; `di/a452/` iceline-g4-entry, iceline-g8-mid
- `Logs/feel/n54-freeze2/gf/a254/` f0200, shot-N54-wick-p1-raw, -p2, -turn; `gs/a254/f0060.jpg`
- `Logs/feel/n54-r3/` g3i brawl40-p1, -turn; g3k wyrm-p1, -turn, -p2; g3p loom37-p1, -p2; g3s crown37-p1-raw; l3off/a154 stack-on

**Not checked:** the climb, RIDE KILL's callout, anything in motion (flicker cannot be judged from stills), 1280x720 frames, the
verbs37 frames of foundry / span / spire (requested, not rendered).

---

## 1. The rooms still read as big symmetric halls you stand in; the first frames of a room are empty

- **GG:** "I'd like a lot of the rooms to be a lot smaller and more action packed like think about the design of Ultra Kilikow"
  (OWNER.md / CORE.md, 2026-10-09); "please dont just randomly plce stuff it needs intension and personality" (OWNER.md 2026-10-17,
  repeated in NIGHT52.md); "random lava pits random pillars that serve no purpose" (CORE.md, w35); "The rooms still don't incentivize
  wall-riding or creative use of the weapons; many of them feel very boring" (OWNER.md, w29); level 3 "just isn't good" (CORE.md, w38).
- **Frames:** `Logs/feel/n54-freeze/di/a452/shot-N29-K52-n52-iceline-g4-entry.png`, `.../di/a452/shot-N29-K52-n52-iceline-g8-mid.png`,
  `.../df/a452/shot-N29-K52-n52-foundry-b1-entry.png`, `Logs/feel/n54-freeze/vk/verbs37/f0100.jpg`, `f0500.jpg`, `f1400.jpg`.
- **On the frame:** g4 THE REACH from its door is a mirror by eye: a ramp left and a ramp right, a cyan pad column left and right, the
  exit centred between two white light posts, one drone 15 m+ away, "3 LEFT". NIGHT53-REPORT claims g4 is "no longer a mirror" on I1 59 %
  (threshold 60 %): the number passes by one point and the frame still reads mirrored. b1 THE YARD's entry: a long, deep hall, "11 LEFT",
  two small red husks (~40 px tall) at the far end, nothing within reach. g8 THE STAMPEDE, GG's own "mini challenge with three or more of
  them" (NIGHT47.md), mid frame: a blue corridor, one enemy, "10 LEFT". Kiln: f0100 a long empty corridor, f0500 a bare walled box,
  f1400 the camera on a blank purple wall. Telemetry: the on-screen enemy list is empty in 1733 / 1800 kiln verbs37 frames (96 %),
  1419 / 1800 spire, 1069 / 1800 foundry. verbs37 is a verb checklist, not a fight reel; that is the problem: no Player reel tonight
  shows a room's fight from entry to clear, so nobody has looked at "action packed".
- **Fixed looks like:** (a) the a452 entry frame of each re-cut room shows >= 2 live enemies >= 100 px tall (close range) and the far wall
  inside ~24 m; (b) the entry frame flipped left-right correlates < 0.6 in luma with itself (g4 today: matching ramps and columns);
  (c) a per-room fight reel, entry to clear, where an enemy is on screen in >= 60 % of frames; (d) g8's mid frame shows its 3+ chargers in
  one view.

## 2. THE WICK, the boss GG named in two prompts, ships with no palette, wrapped in danger red, with a floodlight on the crosshair

- **GG:** "A first boss fight in the first stage is something I genuinely like a lot ... the lava lighting is inconsistent and messy"
  (NIGHT47.md); "The first boss fight in particular has far too many lighting issues"; "each boss should have its own distinct color
  palette and lighting identity so the arena shifts in tone and atmosphere during the fight, similar to how JoJo's Bizarre Adventure uses
  dramatic color changes" (NIGHT54.md).
- **Frames:** `Logs/feel/n54-freeze2/gf/a254/shot-N54-wick-p1-raw.jpg`, `shot-N54-wick-p2.jpg`, `shot-N54-wick-turn.jpg`, `f0200.jpg`;
  `Logs/feel/n54-freeze/wf/a454/f1100.jpg`, `f1500.jpg`; `Logs/feel/n54-r3/l3off/a154/shot-N29-L54-stack-on.png`.
- **On the frame:** as shipped (grade off, D-16) THE STACK is olive-brown walls ringed by ~10 full-circle red-orange stripes that fill the
  top half of every frame: decoration in the enemy / danger hue (CLAUDE.md rule 8, V-2), and the reason every grade failed ("within 1-14
  deg of the danger red", NIGHT54-REPORT). The "turn" frame is the same room slightly darker: no JoJo moment. A white-hot pool of light
  sits on the floor directly under the reticle (around 960, 520-560) in p2, turn and f0200. In a454 f1100 the camera faces a striped wall
  at ~1 m with the boss off screen; in f1500 the boss is a sliver at the left edge. "Relatively easy" (median 45.5 s, 2 deaths) is the
  n47human sim; the Player pilot "fires no shots (punches only)" (NIGHT54-LOG 15:09), so no Player frame shows the easier fight.
- **Fixed looks like:** (a) red-orange pixels (hue 0-30 deg, S > 0.5) on the shaft walls <= 5 % of the a254 p1 frame outside a tell
  (today the stripes cover the top ~45 % by eye; not measured): stripes recoloured to a non-danger accent or lit only during a tell;
  (b) THE WICK graded ON with the wall-only danger distance >= 40 deg and the turn frame's mean hue >= 20 deg from p1; (c) in a 200 x 80 px
  box under the reticle: 0 clipped pixels (luma >= 250) and mean luma <= 1.5x the floor's mean; (d) a Player reel where the pilot shoots
  and THE WICK's rect is on screen in >= 70 % of frames.

## 3. The boss palettes that shipped are not what GG asked for, and the phase turn was never seen at a real phase change

- **GG:** the worm "needs the same yellow‑red‑orange color theme as before with a cooler design" (NIGHT47.md); "each boss should have its
  own distinct color palette ... so the arena shifts in tone and atmosphere during the fight" (NIGHT54.md).
- **Frames:** `Logs/feel/n54-r3/g3k/a254/shot-N54-wyrm-turn.jpg`, `-p1.jpg`, `-p2.jpg`; `g3p/a254/shot-N54-loom37-p1.jpg`, `-p2.jpg`;
  `g3i/a254/shot-N54-brawl40-turn.jpg`; `Logs/feel/n54-freeze2/gs/a254/f0060.jpg`.
- **On the frame:** THE WYRM's turn floods the arena acid green and its head in p2 is a green block (~1360-1520, 520-610): the opposite of
  "the same yellow-red-orange". On p1 the worm itself cannot be picked out (a yellow column and a green box); the only tell on screen is a
  text card, "MOVE / B to jump". THE LOOM's p1 and p2 are the same frame (same pose, both "LEG 1 / 3"); THE WYRM's p2 also says
  "LEG 1 / 3": NIGHT54-REPORT's "a colour turn at ... each phase change" was not captured at a phase change for either. THE ICEBREAKER's turn
  makes its body pale blue on a blue room: the boss in the player's cyan family and colder than its arena (E-3: enemies are the warmest
  objects on screen). THE CROWN ships ungraded (f0060: olive left wall, green right wall, red ceiling): the first and the last boss have no
  identity, 4 of 6.
- **Fixed looks like:** (a) THE WYRM's turn hue on wall pixels inside 15-55 deg, its head within 30 deg of its molten body; (b) for every
  boss a p2 frame whose telemetry reads "LEG 2 / n", with a printed p1 -> p2 mean-hue change >= 20 deg or value change >= 0.1; (c) every
  boss body >= 40 deg from cyan (186 deg) with saturation >= the arena's, measured on the boss rect; (d) THE CROWN graded ON, 0 frames
  "TOO CLOSE TO DANGER".

## 4. Not one kill on any frame of the shipped build; the score never moves; the style meter reads DULL in every frame

- **GG:** kills aren't "crunchy" (OWNER.md, w16); "hits and kills need to feel more satifying", the style meter "feel aweosme to combo"
  (OWNER.md, w41); "all weapons need better effects, animations, and impact so they feel heavy and satisfying" (NIGHT47.md).
- **Frames / telemetry:** points read "500" in 1800 / 1800 frames of each of the five verbs37 reels and of di a452, 1009 / 1009 of df a452
  (only THE WICK's a454 moves); foundry verbs37's events are land / slide / boost / pipsTick / shot / announcerSlab, no kill; the style rank
  text is "DULL" in 2772 / 2772 frames across the six n54-freeze reels that show it (e.g. `Logs/feel/n54-freeze/vk/verbs37/f0950.jpg`,
  `f1400.jpg`: "D / DULL").
- **What it means:** the play build has no Player frame of the thing GG has complained about since w16. NIGHT54's move54 cut the camera's
  hold on a gun kill 0.085 -> 0.05 s (40 % less) for smoothness, judged by a jitter number; no kill frame was looked at before or after.
- **Fixed looks like:** a kill reel (survival kill6 / kill12, per memory "reel kill coverage") on the build that ships, 6 kills per weapon,
  frames k-2..k+8 saved; on each: the points change within 2 frames, hitstop steps printed against the C-2 ladder (bullet 0-1, shotgun
  2-4, charged ~6, gauntlet 12-20), the kill burst >= 2 % of the frame; a 3-kill chain lifts the rank off D; the move54 kill hold judged in a
  blind pair against `-skygrindOff54` (if "off" is picked as the heavier kill 2 / 3, restore 0.085 s for kills).

## 5. "Strong color direction" in the decor: on and off cannot be told apart

- **GG:** "a lot of the decorative elements in several arenas are poorly executed and lack strong color direction" (NIGHT54.md); "the
  colors and stuff are way too bland The effects are inconsistent it all needs to be unified" (NIGHT49.md).
- **Frames:** `Logs/feel/n54-freeze/df/a452/shot-N29-K52-n52-foundry-b1-entry.png` vs
  `Logs/feel/n54-freeze/dfoff/a452/shot-N29-K52-n52-foundry-b1-entry.png`; `.../df/a452/shot-N29-K52-n52-foundry-a5-mid.png`.
- **On the frame:** side by side, b1 with decor54 on and off show me no difference: the same orange-brown walls, cyan wall dashes, cyan pad
  beams, red crate. NIGHT54 closed decor54 "DONE by its numbers; subtle on screen": its numbers count cyan pieces removed, not colour
  added. In a5-mid everything bright is cyan (pad beams, floor trims, wall dashes, a pillar face, the health cross) or red; no brass.
- **Fixed looks like:** per level, on vs off a452 entry frames: mean CIE76 dE over wall pixels >= 8; the level's accent (foundry brass ~40
  deg, iceline cobalt, kiln copper-green, ...) on >= 5 % of wall pixels at S >= 0.35; 3 fresh judges pick "on" as "has a colour direction"
  in both orders.

## 6. Lights still float: bare white rectangles hang in the ceilings

- **GG:** lights look "placed and floating" (OWNER.md, w32); the lights "look crappy they look cheesy ... weird little like triangle
  displays" (OWNER.md, w34); "the lighting and flickering bugs that happen constantly break immersion" (NIGHT54.md). PRINCIPLES 7 / V-5:
  lights hang from real fixtures.
- **Frames:** `Logs/feel/n54-freeze/df/a452/shot-N29-K52-n52-foundry-b1-entry.png` (white quad ~930-990, 105-140, under "THE YARD"),
  `.../df/a452/shot-N29-K52-n52-foundry-a5-mid.png` (~760-840, 195-235), `.../di/a452/shot-N29-K52-n52-iceline-g4-entry.png`
  (~925-995, 72-115).
- **On the frame:** in three rooms the brightest thing near the top is a flat, clipped-white rectangle with no housing, cable or rod,
  floating under a black ceiling. light54 (THE STACK's flicker 55 -> 2) fixed strobes; it did not touch these.
- **Fixed looks like:** in every a452 entry / mid frame each ceiling emissive has a visible fixture (a dark frame >= 4 px at 1080p and a
  hanger to the beam), clipped-white area per light <= 400 px, and 0 floating quads across the 18 rebuilt rooms' entry frames.

## 7. Boss fights still don't say what to do, and the words land on top of the boss

- **GG:** bosses "you just teleport into these random rooms ... hard to understand what to do" (CORE.md, w35); "The text that appears
  during the fight also needs to be much easier to read because it's currently tiny, unclear, and hard to understand" (NIGHT47.md).
- **Frames:** `Logs/feel/n54-freeze2/gf/a254/shot-N54-wick-p1-raw.jpg`, `f0200.jpg`; `Logs/feel/n54-r3/g3p/a254/shot-N54-loom37-p1.jpg`;
  `g3s/a254/shot-N54-crown37-p1-raw.jpg`; `g3i/a254/shot-N54-brawl40-p1.jpg`; `g3k/a254/shot-N54-wyrm-p1.jpg`, `-p2.jpg`.
- **On the frame:** the fight opens with the arena's name in 40 px white across the boss's body: "THE STACK" (telemetry rect 360-1560 x
  250-302, through THE WICK's yellow core; 79 / 208 frames of gf a254), "THE NARROWS" on THE LOOM, "THE SPIRE" on THE CROWN's ring: a
  place name, not an action. Under every bar a grey "STAGGER" sits while the boss is not staggered (WICK, ICEBREAKER, LOOM, CROWN p1: full
  bar, LEG 1); B-1 asks that line to say what to do. When the instruction does come it is printed twice at once (ICEBREAKER "PARRY THE
  BLOCK. PUNISH STUCK FISTS" centre and under the bar; WYRM "PUNCH THE HEAD TO STAGGER IT" the same). THE CROWN's "GEMS 20 / 20" (24 px)
  is drawn inside the red HP fill. THE WYRM's bar opens with its name at 16 px, alpha 0.42 (g3k telemetry line 41).
- **Fixed looks like:** in each a254's first 3 s, 0 px of any text box inside the boss's screen rect (the place name before the boss is
  revealed, or above the bar for <= 1 s); the state line empty or "LOCKED: verb + glyph" until a stagger; each instruction in one place at
  a time; GEMS text outside the HP fill (0 px overlap); the boss name >= 26 px and alpha >= 0.9 from the bar's first frame. GG on NIGHT54:
  "the UI looks good as it is, so don't touch that": these are placement and state bugs, not a restyle; keep the plates, fonts, colours.

## 8. Level text is still fragments, never leaves, and runs over the enemy counter

- **GG:** "The text that appears when you pick up the gauntlet or start the game also sounds awkward and unnatural, it needs to be
  rewritten to feel like real game dialogue" and "The enemy count in the top left needs to be larger and visually cleaner" (NIGHT47.md);
  "way too much instruction at the tsrat of the level" (OWNER.md, w41).
- **Frames:** `Logs/feel/n54-freeze/vk/verbs37/f0500.jpg`, `f0950.jpg`, `f1400.jpg`; `Logs/feel/n54-r3/l3off/a154/shot-N29-L54-stack-on.png`;
  `Logs/feel/n54-freeze/vi/verbs37/f0700.jpg`; `Logs/feel/n54-freeze/vk/verbs37/f0100.jpg`.
- **On the frame:** kiln's chip "In the air, look straight down" names no action and no button (T-3: verb + bound input); it is in 1199 /
  1800 kiln verbs37 frames (20 of 30 s), while the camera faces a wall or a violet void. A teach line is visible (alpha >= 0.3) in 1800 /
  1800 verbs37 frames on iceline, kiln and span (NIGHT52's hud52 claimed the card "shrinks to a chip after 6 s or the first kill": the chip
  then never leaves). THE WICK's fight line starts with a glyph and a lowercase half-sentence ("(R) the yellow embers back."; a454 also has
  " through the crack."). "B to jump" is still taught on level 3 (f0100). On iceline the toast "SLIDE BURST: RECHARGING" (24 px, rect
  72-427 x 64-90) is drawn over "2 LEFT", in 226 frames of each level's verbs37.
- **Fixed looks like:** every lesson line a whole sentence with its verb and its bound glyph, <= 6 words; on screen <= 6 s or until the verb
  is done (<= 360 frames per line in telemetry); no basic-jump line after level 1; no toast overlaps the counter plate (0 px).

## 9. Through the doors it's still "like dark"

- **GG:** doors "not visible now and you just run through it and it's like dark" (OWNER.md, w37); "weird issues when running through the
  door, where it randomly turns pink or bugs out" (NIGHT47.md).
- **Frame:** `Logs/feel/n54-freeze/vk/verbs37/f0950.jpg`.
- **On the frame:** the title plate says "DOOR"; the whole view is one flat dark violet with a single darker band across the middle: no floor
  line, no jambs, no exit; only the HUD and the gauntlet say the game is running. (The pink door, G10, was fixed and measured on NIGHT47;
  this is the "like dark" half.)
- **Fixed looks like:** across verbs37 x5, 0 frames with luma standard deviation < 0.03 (a flat screen); in door / link frames >= 3 visible
  geometry edges (floor, both jambs) and the next room's lit exit in view (A-6, A-8).

## 10. Enemies at fighting range are still red jelly beans, and screen-edge borders are back

- **GG:** "the visors on enemies should be redesigned to look sharper and more angular, similar to Omega from Fortnite ... slightly
  thicker and more like futuristic glasses"; "The small mini robots also look strange and need a better, more cohesive design"
  (NIGHT47.md); the dash lines "so forced ... not just that weird border that only appears" (OWNER.md, w19; CORE.md never again:
  "Screen-border speed lines").
- **Frames:** `Logs/feel/n54-freeze/df/a452/shot-N29-K52-n52-foundry-a5-mid.png`; `Logs/feel/n54-freeze/vi/verbs37/f0700.jpg`.
- **On the frame:** in a5-mid three enemies at roughly 8-15 m are red-orange blobs with a soft outline; one shows a faint yellow band, the
  others no visor at all (G13 SHORT since NIGHT48: "temple points not seen at 10 m"; G14's swarmer 16 x 22 px at 20 m). The same frame has
  a noisy red-orange band ~150-250 px deep on all four edges (a hit or low-health effect; which one not checked): an effect over the screen
  edge (NIGHT55 rubric line 4; U-6 "never a full-screen tint"). iceline f0700 at 24 m/s: both side edges wash white ~120 px deep while
  telemetry draws 13-16 streaks.
- **Fixed looks like:** on a452 mid frames every enemy >= 80 px tall shows a visor band >= 6 px tall at 1080p, value >= 0.25 off the body,
  its angular ends visible at 10 m (G13's done-when); hit / low-health feedback confined to the health plate (U-6): pixels within 200 px of
  the edges change luma <= 0.05 from the effect; at full speed the outer 150 px no brighter than the centre third (luma delta <= 0.1),
  speed carried by streaks in the world.

---

## The reports' claims against the frames (only the ones the frames contradict or cannot support)

| claim | source | the frame |
|---|---|---|
| g4 "no longer a mirror" (I1 59 %) | NIGHT53-REPORT | the entry frame reads mirrored (item 1) |
| decor54 DONE | NIGHT54-REPORT | b1 on / off indistinguishable (item 5) |
| a colour turn "at ... each phase change" | NIGHT54-REPORT | LOOM / WYRM "p2" frames are LEG 1 / 3 (item 3) |
| THE WICK "relatively easy" | NIGHT54-REPORT | sim only; the Player pilot punches only; boss off screen in a454 (item 2) |
| kill freeze camera hold 40 % less = smoother | NIGHT54-REPORT (move54) | no kill on any frame of the build (item 4) |
| the lesson card "shrinks to a chip after 6 s or the first kill" | NIGHT52-REPORT (hud52) | a teach chip in 1800 / 1800 frames on three levels (item 8) |
| "Nothing here was played by a human" | NIGHT52 / 53 / 54 reports | true on all three; the last recorded GG playtest is w47 (CORE.md) |

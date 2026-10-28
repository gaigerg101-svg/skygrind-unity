# GG proxy, FORGE72 start (2026-10-27): the ten worst things on build n71-polish8 (HEAD 646ccc818)

Read: docs/memory/OWNER.md (all 120 lines), docs/memory/CORE.md, the GG blocks of NIGHT55-61 (process asks only, no look
complaints), NIGHT54.md:6 and NIGHT63.md:5. Frames: the five sheets in `frames/`, plus these full frames under Logs/feel/n71-forge72/:
bk/boss63 wyrm-idle, -stagger, -windup; bp/boss63 crown37-idle, -tell, -windup; tp/tour65 spire-v6; tf/tour65 foundry-a3. Positions
are for 1920x1080 (tile positions say "tile"). "Asks" counts GG's dated quotes on the point. This list ranks defects that match his own
complaints; it says nothing about what looks right (L-105).

## The ten, worst first
**1. The boss rooms are still dark.** Wyrm idle: m42, d47 %. The ceiling (y 0-200) and the walls are brown-black; the only bright things are the
yellow pillar and the orange floor grid. Crown: d35-43 %, and a black grid ceiling fills the top 40 % of crown37-idle, crown37-tell and crown37-windup.
Level 5's rooms are dark too: spire v2 d30 %, v5 d23 %, v6 d29 %.
- Frames: bosses.jpg (all nine tiles); bk/boss63/shot-N30-B63-wyrm-idle.png; kiln-span-spire.jpg (the spire row).
- GG: "a lot of the room especially the boss rooms are very dark So we need to make sure to fix that" (2026-10-26, OWNER.md:118); "The first
  boss fight in particular has far too many lighting issues" (2026-10-20, OWNER.md:115); doors "like dark" (2026-10-13, OWNER.md:108). Asks: 3.
**2. Each level is still one tint, and the colours do not go together.** Spire v6 is olive green everywhere (floor, walls, ramps and
pillars are the same green, with no material contrast), and its walls carry dark-red stripes (x 0-560, y 230-440); red is supposed to mean danger. Iceline is blue-grey with
purple bands; the kiln is salmon and mauve with purple pillars; foundry is tan, teal and purple. The wyrm arena adds lime-green edge lines
(wyrm-windup x 550-800 y 325-350, top-right diagonal) that no other level uses.
- Frames: kiln-span-spire.jpg, iceline.jpg, foundry.jpg; tp/tour65/shot-N29-T65-n52-spire-v6-mid36.png; wyrm-windup.
- GG: "I asked previously about stuff related colors and animations and consistency and I just haven't seen it" (2026-10-26, OWNER.md:118);
  "effect colores everything doenst feel alligned" (2026-10-22, OWNER.md:120 / NIGHT63.md:5); "the colors and stuff are way too bland The
  effects are inconsistent it all needs to be unified" (2026-10-19, OWNER.md:116); "lack strong color direction" (2026-10-20, OWNER.md:115);
  "bland" (2026-10-05, OWNER.md:37). Asks: 5.
**3. In the crown fight the effects hide the boss, and their colours break the colour rules.** THE CROWN does not appear in any of the five crown frames.
Every time, a 700-900 px burst sits on the crosshair: cyan in idle (x 560-1320, y 450-1080), and orange-red with a pink ring in flinch, tell and windup
(x 600-1250, y 520-1000). The "ring" the text tells you to dash through is drawn in the danger colour. The tell adds a red cone that covers about a third of the frame
(x 450-1600). Behind all of it the arena is green, purple, black and orange at once.
- Frames: bosses.jpg (crown tiles); bp/boss63/shot-N30-B63-crown37-tell.png, -windup.png, -idle.png.
- GG: "each boss should have its own distinct color palette and lighting identity"; "the guns and effects during boss fights should follow
  proper color theory" (2026-10-20, OWNER.md:115); "effect colores everything doenst feel alligned" (2026-10-22, OWNER.md:120). Asks: 2.
**4. The bosses look like boxes.** THE KILN WYRM is a row of dull grey-green boxes at mid height (wyrm-windup x 0-1010, y 500-560) that blends
into the brown wall. Its "charging head" is a pale-green box about 90x60 px at the crosshair (wyrm-idle x 920-1010, y 505-570), and nothing shows that it
is charging. When staggered it becomes a translucent cyan cube with three pink rectangles (wyrm-stagger x 815-1120, y 385-690), and cyan is the
player's colour. A frame cannot show how rigidly the boss moves, but the model these frames show is box primitives.
- Frames: bosses.jpg (wyrm tiles); bk/boss63/shot-N30-B63-wyrm-idle.png, -windup.png, -stagger.png.
- GG: "boss animations are too rigid to start" (2026-10-22, NIGHT63.md:5); "colors and animations ... I just haven't seen it" (2026-10-26,
  OWNER.md:118); "I'm just still not a fan of a lot of the bosses" (2026-10-10, OWNER.md:77). Asks: 3.
**5. The boss text is wrong or unreadable.** In wyrm idle and flinch the text says "Swing the gauntlet LEFT CLICK at its charging head" while the weapon panel
shows THUNDERCLAP, and the line crosses the yellow pillar ("gauntle|t", x 780-800). Every crown frame shows a grey "STAGGER" under the bar during idle,
tell and windup, when the crown is not staggered. The orange beam cuts through the crown windup line ("throu|gh", x 1180-1300). In crown stagger, "OPEN, SHOOT IT"
is lost inside the cyan burst. The wyrm windup hint "PUNCH THE HEAD TO STAGGER IT" is grey and about 16 px tall.
- Frames: bosses.jpg; bk/boss63/shot-N30-B63-wyrm-idle.png; bp/boss63/shot-N30-B63-crown37-windup.png.
- GG: bosses "you just teleport into these random rooms ... hard to understand what to do" (2026-10-12, CORE.md:7); "there needs to be more
  text on the screen ... make it gamified and easy for the user to understand" (2026-10-26, OWNER.md:118). Asks: 2.
**6. The rooms do not tell you where to go.** 7 of the 19 room frames have a text line, and every one is a control hint ("Hold SPACE on walls to
ride", "Punch yellow globs back with F", "Yank an enemy with G"), never "go up" or "the exit is there". These have no line at all: foundry a2, a5, a8 and b1, iceline g3-g8,
all five kiln rooms, all three spire rooms.
- Frames: foundry.jpg, iceline.jpg, kiln-span-spire.jpg; tf/tour65/shot-N29-T65-n52-foundry-a3-mid36.png (the line at x 760-1160, y 150-200).
- GG: "there needs to be more text on the screen that explains like OK now you have to go up or whatever" (2026-10-26, OWNER.md:118);
  "There needs to be text in the game similar ultra kill" (2026-10-11, OWNER.md:101). His limit: "way too much instruction at the tsrat of the
  level" (2026-10-14, OWNER.md:110), so keep the lines short and show them in the moment. Asks: 2, plus the limit.
**7. The lights look placed and floating.** In the spire, cyan and green rings hang flat on the walls (v6 x 1630-1760 y 40-140; x 410-490 y 320-380).
Kiln kb2c and foundry a2 have small box lamps on bare walls. Flat white ceiling quads (iceline g4 and g6, span p1) and white strips beside every door
(foundry a2 and a8, iceline g1 and g6) look the same in every room.
- Frames: kiln-span-spire.jpg (spire row, kb2c, p1), iceline.jpg (g4, g6), foundry.jpg (a2, a8); the spire-v6 full frame.
- GG: lights look "placed and floating" (2026-10-11 ~10:00, OWNER.md:85); the lights "look crappy they look cheesy ... weird little like
  triangle displays" (2026-10-11 ~19:28, OWNER.md:105). Asks: 2.
**8. Decoration with no purpose: stripe bands and blocks.** Purple and dark-green horizontal bands run along every foundry wall (foundry-a3: a
purple band at x 380-1000, y 390-480, behind the enemy). Iceline has purple panels, the spire has red stripes, and the kiln has salmon blocks stacked on purple pillars.
None of them marks a route, a ride wall or a hazard.
- Frames: foundry.jpg, iceline.jpg, kiln-span-spire.jpg.
- GG: "decorative elements in several arenas are poorly executed and lack strong color direction" (2026-10-20, OWNER.md:115); "cluttered
  with detail for the sake of detail" (2026-10-11, OWNER.md:95); "random lava pits random pillars that serve no purpose" (2026-10-12,
  CORE.md:7); "more elements that are not just decoration" (2026-10-14, OWNER.md:110); "dont just randomly plce stuff it needs intension
  and personality" (2026-10-17, OWNER.md:112). Asks: 5.
**9. The rooms look ugly: blank walls fill the view.** In iceline g3, the right half of the frame (tile x 1560-1920) is one blank navy wall. In foundry a8, the left
40 % of the frame is a bare tan slab. In kiln kb2, purple pillars cut off half the view. Level 3 is still salmon and purple boxes.
- Frames: iceline.jpg (g3), foundry.jpg (a8), kiln-span-spire.jpg (kb2, kb3).
- GG: "other rooms and arenas that look ugly" (2026-10-26, OWNER.md:118); level 3 "just isn't good" (2026-10-13, OWNER.md:109). Asks: 2.
**10. A noise border and the TV overlay.** In wyrm-stagger and wyrm-windup, a red speckled noise frame 150-250 px deep eats all four edges at
9999 health. GG's own two screenshots show a red border at 45 and 31 health. Every frame has scanlines and a red/cyan fringe on edges
(foundry-a3 pillar edges; spire-v6 ramp edge).
- Frames: bosses.jpg (wyrm-stagger, wyrm-windup), gg-screens.jpg, any full frame.
- GG: "not just that weird border that only appears" (2026-10-06, OWNER.md:31); the PS2 overlay "and it really shows" (2026-10-11,
  OWNER.md:105). In favour: "UI elements that buzz, glow briefly, or flicker like an old TV" (2026-10-09, OWNER.md:70). Asks: 2 against, 1 for.

## What a still cannot judge (the night measures these another way)
- Jump height, gravity, ULTRAKILL movement "almost a one to one copy" (2026-10-26, OWNER.md:118); movement "less choppy" (2026-10-20,
  OWNER.md:115). Asks: 2. GG plays it, or a trace is compared against ULTRAKILL's numbers.
- Flicker and "textures overlapping that bug out" (2026-10-26, OWNER.md:118); "flickering bugs that happen constantly" (2026-10-20,
  OWNER.md:115). Asks: 2. Measure it frame to frame: per-pixel luma variance with a still camera, plus a z-fight scan per room.
- Boss motion "too rigid" (2026-10-22; 2026-10-26). Asks: 2. Measure pose changes over a clip.
- Kills and hits "crunchy" / "more satifying" (2026-10-05, OWNER.md:37; 2026-10-14, OWNER.md:110). Asks: 2.
- The wall ride's feel (OWNER.md:64, :71 2026-10-09; :106 2026-10-11; :110 2026-10-14; :111 2026-10-16; :118 2026-10-26 the roof). Asks: 6.
- Boss length "one to two minutes" (2026-10-13, OWNER.md:109); the UI "needs to be animated" (2026-10-14, OWNER.md:110). Use a clock and clips.

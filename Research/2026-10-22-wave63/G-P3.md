# NIGHT63 builder G: leftover 2, text61 (NIGHT62 P3) made judgeable on a two-run pair with the hint pinned

Branch `n63-P3` from port/stage3 d3336128 (it carries P2; V's B1 RED commit; nothing of P3 touches V's or B2's files). Builder G never ran the
Editor, a build, a Player or a reel. The row stays design.n61.text61 (OFF in config); nothing here turns it on.

## The red (read only, measured before any change)
- NIGHT62 P1's pair (s1on vs s1spot, a157 n57-kiln with -skygrindClearShots60, the same build): the SPOT named "the hint line under DOOR OPEN"
  ("Boost: SPACE, aim down, LEFT CLICK twice" vs "At the top, press LEFT CLICK again"; Logs/n62/spot/P1.txt), not the row.
- Why, from the two runs' telemetry (`Logs/feel/n62-p0/s1on|s1spot/a157/telemetry.jsonl`, the HUD's text by frame, read with a scratch
  script): the text timelines are identical in order but drift in time: 0 frames to the kb2 clear, then 1 frame (DOOR OPEN at f1026 vs f1025),
  9 frames by the kb3 entry (THE PRESS HALL at f1355 vs f1346, 0.15 s). In kb3 the sim's boost chain rotates through five lines on its own
  timers (the s1on log: "Jump SPACE when the floor glows", "Fire LEFT CLICK down to lift off", "Press SPACE and LEFT CLICK together", "At the
  top, press LEFT CLICK again", "Boost: SPACE, aim down, LEFT CLICK twice"), so a drift of a few frames puts a different line on the clear
  shot. The telemetry stops at 1800 frames (30 s), so the kb3 clear itself is not in it.
- NIGHT62 P3's pair (c3on-k vs t61off, a157 without clear shots): both kb2c entry frames show the title alone; the plain a157's rooms saw
  almost no lessons (one at the start), so text61 had nothing to act on and the SPOT said NOTHING OBVIOUS.

## What it adds (view only, reel only; no sim, config or row change)
- `Game/PinHint63.cs` (pure): `-skygrindPinHint63 [text]` acts only with `-skygrindReelOnly` on the command line (never in normal play). Its
  text is the argument after it (`|` separates lines, one per room in turn); without one (the last argument, or the next one is a switch) it is
  `UnityTuning.PinHint63Default` = "STEP 1 · IN THE AIR, LOOK STRAIGHT DOWN" (design.n43.kiln43.chain "look"), which brief55's teachMap words
  "Midair, aim down, then {fire}.", the instruction that stood under THE CLINKER BRIDGES' title on NIGHT60's red frame. The feed: nothing
  before the first room entry; at each "roomEnter" `PinHint63GapFrames` (1) empty frame (the card closes, as at a room change), then the room's
  line every frame.
- `Game/UiPinHint63.cs` + one hook line in `UiHud33.cs` Slot33 (after teach42): with the pin, the teaching slot's lesson line is the pinned
  line (through copy47 / brief55 and the lesson widget's resolver, exactly as a lesson's text is formed), its teach line none, no boss line.
  So what the centre card is ASKED to show depends only on the room entries; text61's gate (on) or its absence (off) decides what is drawn.
  The sim's lessons still run (the HUD stops drawing them; counted as "sim lines hidden").
- Proof (L-089), Player.log: `[pin63] hint pinned: <n> line(s) "<raw>" -> "<shown>" | ...` once; `[pin63] room <id> entered: "<shown>" after 1
  empty frame(s) | sim lines hidden so far <k> frames, fed <f> frames` per entry; `[pin63] text: blocks <n> | title ".." | card ".." | clear ".."`
  on each change of the centre text (the top slot on / off, the card's line, the ROOM CLEAR card on / off).
- `Tools/night/n63-p3-blocks.cjs` (the meter): at each `[reel] a157: <room> ... entry` line, the centre text blocks in effect (title, card,
  ROOM CLEAR); a summary per log: entries with two or more blocks k of n (doneWhen 1), ROOM CLEAR m of n (doneWhen 2), the card c of n; RED if
  no `[pin63] hint pinned` line. **Planted case:** `node Tools/night/n63-p3-blocks.cjs --selftest` PASS (5 planted entries, blocks 2, 3, 1, 1,
  0 -> "two or more 2 of 5, ROOM CLEAR 1 of 5, card 3 of 5"; an unpinned log says RED).

## What text61's pair should show (from the code and N63PinHintTests' pure run, NOT measured in a Player)
With the default (one line every room): text61 OFF shows the instruction under the title on every entry frame where the title still stands
(and the previous room's ROOM CLEAR with clear shots); text61 ON shows the title alone: in the first room the new line waits for the title
(Text61WaitS 3 s), in rooms 2-5 the same line again is withheld (once per run outside level 1), and roomEnter drops the ROOM CLEAR card.
N63PinHintTests' SPOT-pose test drives the real Text61Gate with the feed (title up 2.5 s, the frame 1.5 s after each entry): on 0 of 5 entry
frames with the instruction, off 5 of 5; shown 1, withheld 4. Honest limit: the default pin plants ONE instruction repeated in every room, so
rooms 2-5 judge text61's "no repeat" rule and room 1 its "wait for the title" rule; a per-room line (`-skygrindPinHint63 "A|B|C|D|E"`) judges
only the wait rule, and only on entry frames where the title still stands.

## Tests
`Assets/Tests/EditMode/Game/N63PinHintTests.cs` (4): the switch (reel only, text or the default, `|` lines), the feed (gap, line, counters, lines
in turn), two runs fed the same entries are identical frame by frame, the SPOT pose through Text61Gate. Run outside Unity on the dn-check
scratch build (a scratch runner, not the Unity test runner): RED 0 / 4 on `baced372`, GREEN 4 / 4 on `69d9c648`; N61TextTests 6 / 6 on the
same build.

## Shas (n63-P3, in order)
`baced372` (test-first, RED), `69d9c648` (the pin, the hook, the proof lines, the meter: GREEN), then this notes commit. Pick: `git
cherry-pick baced372^..<notes sha>`. dn-check 0 errors on both code commits (scratch deleted after each). `bash Tools/night/n63-gates.sh
--no-editor` on 69d9c648: all GREEN 23:27:25 (config identical; golden 0 failing, 0 mismatches, 22 replays exact; fingerprints plain and
--off57..--off63 EQUAL).

## Player steps for the lead
1. Build with text61 on in the copy: `node Tools/night/n46-build-rows.cjs <tag> on n61.text61` (the pin needs no row).
2. The two-run pair, same build, same pose, differing only by text61 (clear shots on, as NIGHT60's red run b8on had them, so the ROOM CLEAR
   card can stack too):
   `bash Tools/night/n63-reels.sh <tag> p3on,n57-kiln,a157,-skygrindClearShots60,-skygrindPinHint63 p3off,n57-kiln,a157,-skygrindClearShots60,-skygrindPinHint63,-skygrindNoText61`
3. The meter: `node Tools/night/n63-p3-blocks.cjs Logs/player/n47-reel-<tag>-p3on.log Logs/player/n47-reel-<tag>-p3off.log`. Want on:
   two or more blocks <= 1 of 5 and ROOM CLEAR 0 of 5; off: the stacked text on the same rooms (the pair's difference). Both logs must have the
   `[pin63] hint pinned` line and five `[pin63] room` lines (the pin fired on both sides).
4. Proof greps: `grep "\[pin63\]\|\[text61\]" Logs/player/n47-reel-<tag>-p3on.log` (on: one `[text61] shown`, then `withheld repeat` lines and
   `room clear dropped` lines; off: no `[text61]` line).
5. **SPOT pose:** a157 n57-kiln `shot-N29-L57-n57-kiln-kb2c-entry.png`, p3on vs p3off (THE CLINKER BRIDGES, the slate's P3 pose): off should
   show the title, "Midair, aim down, then LEFT CLICK." on the card and the previous room's ROOM CLEAR; on the title alone. If the meter shows
   kb2c's title gone by its entry frame on both sides, take the entry where the off side's blocks are highest (the meter prints each).
   P3 is exempt from the 2 % rule (NIGHT63.md rule 8: NIGHT62's debt rows).
6. Level 1 keeps its overlay (the slate's doneWhen 3): text61 passes everything on the first level; the pin is not needed for that check
   (a357 on n52-foundry, on vs off, as NIGHT61's notes).

## Not checked
- Anything in a Player: whether the pin fires, the frames, the meter's numbers, the SPOT.
- Whether the five entry frames still show the room's title at the shot (the title stands ~2-3 s; a slow wave delays the shot): rooms whose
  title has gone show text61's effect only through the card and ROOM CLEAR; the meter prints the title per entry.
- What else in the HUD can still differ between two runs: the top slot's EXPOSED / "Standing still! Move." line (the director's stillness
  timer) and enemy positions (NIGHT62: 15.3 m vs 15.2 m). The pin fixes the lesson line only.
- The meter attributes a change made on a shot's own frame to the next line (one frame's slack, said in UiPinHint63.cs).

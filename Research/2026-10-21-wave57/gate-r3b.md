# NIGHT57 gate r3b: E1 L3-rooms / kiln57 (a fresh critic, 2026-10-21 ~13:15)

Rubric: Research/2026-10-21-wave57/critic-gate.md. Brief: gate-r3b-brief.md. I did not read builder L's report except to find log
paths (L.md names only Logs/feel/n57-r3/summary47.txt and the harness command `n47b beats`). Every number below is from a log line I read
or from a read-only command I ran myself. My own command outputs are in the session scratchpad (`beats-n5x.txt`, `kb3-*.txt`, `kb2*-hold.txt`).
Nothing in the repo was written except this file. Every flow number is from the bot.

## E1 L3-rooms (kiln57: level 3 as n57-kiln)

### 1. Evidence real: PASS
- Build: `Builds/n57-freeze/SKYGRIND.exe` mtime 12:53. Commit 5e3d69c2 is dated 12:52. The build's copy
  `SKYGRIND_Data/StreamingAssets/Design/unity-config.json` line 12838 has `"kiln57": { ... "enabled": true, "levelFrom": "n52-kiln",
  "levelTo": "n57-kiln" }`. The freeze reels ran 12:54-13:01 (`Logs/feel/n57-freeze/summary47.txt` lines 1, 43, 254, 264).
- ON vs OFF: `== vk57 ... (map n57-kiln, scenario verbs37 ;` (line 1) vs `== vk52 ... (map n52-kiln, scenario verbs37 -skygrindOff57;`
  (line 43). The a157 / a452 lines print `map n57-kiln, level n57-kiln`.
- Bot files: `Logs/n57/L/flow-n57-kiln-r3d.txt` / `-B.txt` print `n46flow n57-kiln (loaded n57-kiln ...): 10/10 walks through kb2..kb3d`.
  The level's plan has the rooms `kb1 link, kb1b link, kb2 room, kb2b link, kb2c room, kb3 room, kb3c room, kb3d room, kb4 link, k5 boss`.
- The frames show what the numbers say: the a452 entry frames carry the cards THE CLINKER BRIDGES / THE PRESS HALL / THE TAP RUN /
  THE CRACKED FLOOR, and their wave 1 is visible.

### 2. MAIN: PASS
- **Combat rooms 3 -> 5. All 5 are new; at least 3 pass every room check, run by me.**
  - Room counts: n52-kiln walks `kb2..kb3` with 3 room rows (`Logs/n57/flow-base.txt` lines 54-59). n57-kiln walks `kb2..kb3d` with 5
    room rows (`flow-n57-kiln-r3d.txt` lines 2-8).
  - New: I compared the plan boxes. None of the 53 boxes in kb2 / kb2c / kb3 / kb3c / kb3d equals any box in
    `Design/levels/n52/kiln.plan.json`. The commits are 6e4c915b, fe810419, 9aea8857, 376b952d and 11c69a2a.
  - `python -I Tools/night/n57-l-room.py table`: R2 / R5 / R8 / IN VIEW / identity / verdict PASS for all 5 rooms. `IN VIEW PASS 5 / 5`,
    `I5 PASS 2`, `I7 PASS`.
  - The contract (`n52-room-check.py`). I re-ran all 14 items on three rooms:
    - kb2 `C 14/14 PASS`: hold probe 4 spots, none at 0 hits; dwell `2.30 -> 1.70 s`.
    - kb2c `C 14/14 PASS`: hold 3 spots, none at 0; dwell `1.60 -> 1.20 s`.
    - kb3 `C 14/14 PASS`: hold 3 spots, none at 0. Dwell `1.10 -> 1.85 s`, inside the 0.8 s band, from the contract's own per-room n46flow
      runs (seeds 1-10 / 11-20, `--off57` base), which I re-ran.
  - kb3c and kb3d: n56-room PASS, identity PASS and the computed items `C 12/12`. I did not re-run their dwell or hold items, and MAIN does
    not need them.
  - Walk: every room `(10/10)` on both seed sets (r3d and r3d-B).
- **Clear 92.2 s -> 168.2 s (seeds 1-10) / 171.9 s (seeds 11-20), bot, inside 150-240.** Source:
  `n46flow n57-kiln SUMMARY: GREEN 10/10; ... clear 168.2 s` and `-B`: `clear 171.9 s`, vs `n46flow n52-kiln SUMMARY: ... clear 92.2 s`.
  The level-time medians are 225.5 / 227.4 s, also inside the band.

### 3. GUARDS: PASS
- **Stall:** `stall 19.0 %` / `21.5 %` (n57) vs `25.5 %` (flow-base) / `26.8 %` (`Logs/n57/L/flow-n52-kiln-B.txt`, seeds 11-20,
  `--off57`). That is better on both seed sets.
- **Deaths:** `deaths 2` / `deaths 3` vs `deaths 8` / `deaths 11`. That is inside the flow band (8) and lower.
- **verbs37 level 3: 31 / 31** (`vk57`, line 39: `PASS: total 31/31 on n57-kiln; RED rows []`). See line 4.
- **The worm's beats fire.** I ran the harness myself (bot):
  - `n47b beats --level n57-kiln`: still `glimpse 1, pass 1, cameo breaches 2, cameo hits 2`; jump `glimpse 0, pass 0, cameo breaches 2,
    cameo hits 0`.
  - `n47b beats --level n52-kiln`: the same numbers.
  - The a157 `beats glimpse 0 pass 0 cameo 0` in every room is **the reel's room teleports, not a lost beat**:
    - The triggers need the feet 8 m over kb1's spawn (`glimpseY 8`), 6 s in kb2 (`passAfterS 6`) and kb3's cameo
      (SurvivalSim.N47.Kiln.cs lines 59, 67, 76).
    - a157 jumps DOORFREE to each fight room's entry and frames it once wave 1 settles.
    - The n52-kiln control printed the same zeros: `Logs/feel/n57-r2/summary47.txt` lines 287-289 (`== l1off ... n52-kiln ...
      -skygrindOff57`: kb2 / kb2c / kb3 `beats glimpse 0 pass 0 cameo 0`).
    - n57-r1 has no a157 run, and n57-r3 has only the n57-kiln side.

### 4. Nothing lost: PASS
- **vk57 vs r1's vk:** all 37 printed rows have the same verdict (23 PASS, 14 SKIP in each run; the reel's own total is 31/31 on both).
  - Freeze vk57: `total 31/31 on n57-kiln; RED rows []`, open floor kb2b.
  - r1 vk: `total 31/31 on n52-kiln; RED rows []`, open floor kb2b (n57-r1 line 85 block).
  - One difference: the wall-ride row uses an 18 m wall on n57 (`longest single ride 1.31 s on a 18 m wall`) vs 24 m on n52 (1.81 s).
    It is a PASS on both.
- **vk52 (`-skygrindOff57`) equals r1's n52-kiln result:** `total 31/31 on n52-kiln; RED rows []`, every row the same. The off side shows
  the older level.
- **r3's RED run (before D-16):** `RED: total 26/28 on n57-kiln; RED rows [40, 41]`, rows 25-27 `SKIP not owned at kiln's start`,
  `open floor kb2`. In it, row 40 reads `parry36 0 ... spitter ALIVE` and row 41 `launch36Burst 0`.
- **D-16 is a fair fix (it hides no lost verb):**
  - The diff (0c4e2286, FeelReel.N35.Verbs.cs +6 lines) only skips staging rooms that come before `FirstAwardRoom57`
    (`n40AwardRooms[0]`). It changes no game code path: the commit message reports fingerprints plain / --off56 / --off57 EQUAL.
  - On n57-kiln it moves the staging from kb2 to kb2b, the room where n52-kiln always staged.
  - A player on either level reaches kb2 before kb2b's KESTREL award. So "punch with the gauntlet in hand in kb2" is the same game state
    on both levels, and the old staging never tested it on n52 either.
  - Limit: the staged verbs run in kb2b (a kept n52 link), not inside the new rooms. Only the wall pick (18 m) comes from the new geometry.
    That is the same coverage n52-kiln had, not less.
- **n52-iceline 21/21 -> 29/29 is D-16, and nothing was lost:**
  - The staging moved `open floor g1` -> `open floor g6` (iceline's first award room).
  - Rows 24-27, 34 and 40-42 went `SKIP -> PASS`. No row went PASS -> SKIP or RED: I compared r1, r3 and the freeze row for row.
- **Other levels:** foundry 27/27 (a2), span 31/31 (p1) and spire 31/31 (v1) have identical rows and staging rooms in r1 and the freeze.
  D-16 moved staging only where a level has an award room after its first lane.

### 5. Eye-only lines (brief: a job per piece, wave 1 at the entry, red-orange only on enemies / danger, no whole-frame tint): PASS
I opened 10 frames from `Logs/feel/n57-freeze/l2on/a452/` and described each before reading its numbers:
- `kb2-entry`: a lane between a tall clay wall and a ramp, two husks and an orange blob ahead, a cyan pad, a cyan strip on the right wall.
  Wave 1 is in view; a452 reads alive 6, in view 6.
- `kb2-mid`: an enemy at point blank fills the centre, with the cyan pad beam on the left. The `red-orange 23.8 %` is that body, not a
  tint (the walls keep their colour).
- `kb2-high`: from the 4.5 m deck down the lane. Enemies are small below, a yellow parryable glow sits on one, an orange glowing strip lies
  on the floor (danger), and the cyan ledge edges are visible. THE STACK is the column at the left rear. Its job in the plan:
  `the entry's cover from the barrow run's spitters ... the loop's pivot near the door`.
- `kb2c-entry`: card THE CLINKER BRIDGES, a cyan pad beam, a cyan health cross, husks and an orange blob mid-floor, ledges with cyan edges.
  a452 reads 5 / 5.
- `kb3-entry`: card THE PRESS HALL. Dark floor-to-roof columns flank the nave (the drop presses), enemies are on the floor and on a ledge,
  the cyan pad is visible. a452 reads 6 / 6.
- `kb3-mid`: a cluster of husks on the cyan pad under large yellow glows. These match Tell36's yellow "punch it" halos and globs
  (Tell36View.cs lines 5-6), not a new colour.
- `kb3-high`: a cyan-edged ledge over a corridor of stacked clay walls, with two yellow floor discs (read as Tell36 halos from above; not
  identified for certain).
- `kb3c-entry`: card THE TAP RUN and the prompt "Midair, aim down, then LEFT CLICK" below it, with no overlap. A drone and husks stand
  around a cyan-edged block.
- `kb3d-entry`: card THE CRACKED FLOOR and the same prompt, a ramp, a cyan pad, two enemy groups visible.
- `kb3d-mid`: a glowing orange crack in the floor (danger), with husks at close range.

Verdicts on the eye items:
- **A job per piece:** R8 reports 0 untagged solids in each room, and every piece in kb2 / kb3 has a `job`. For example, each of kb3's
  four presses is `hero prop: a drop press (the charger's stun post)` ("side-step its charge into it: stunned 1.5 s, its back open").
- **Wave 1 at the entry:** every a452 entry frame shows enemies, and a157 reports `fight rooms framed 5/5 ... with no wave 0`.
- **Red-orange only on enemies and danger:** yes in all 10 frames (lava, the crack, enemies).
- **No whole-frame tint:** none seen.
- **Text:** none overlapping.
- Not checkable from stills: flashes per second. This row adds no effect.

**GATE L3-rooms: PASS**

### Seen, not gate lines (for the lead)
- **Wave 1 seen at the entry, through walls:**
  - a157 measures with the frustum plus a ray, and reads `kb3c ... wave1 6 | alive 6 | seen 3` and `kb3d ... wave1 5 | alive 5 | seen 2`.
    n56-room's entry-eye column agrees: 3.6 of 6 and 3.0 of 5. IN VIEW passes only from the foothold's flanks.
  - The n46flow geometry column `w1 in view` reads 0/4, 0/6 and 0/5 on kb2c / kb3 / kb3c, against 1/4 and 1/6 on n52.
  - NIGHT56's "spawns in view" goal is weaker at the door of the two last rooms.
- **The bot's whole-level run lingers more:**
  - `put at the next room ... 15` / `17` vs 3 / 4 on n52-kiln.
  - `stuck46 burrowers ended 28` / `28` vs 1 / 3.
  - The level-flow dwell is 18.8 / 16.8 s vs 5.7 / 3.5 s. kb3's level-flow dwell is `10.0` / `5.0` s vs n52 kb3's 1.9 / 1.1. The
    contract's per-room kb3 dwell passes (1.10 -> 1.85 s), so the extra time comes from the level context, probably the cameo / lesson
    room. The 225 s level time includes these put-forwards.
- **r3's `vk` on n52-kiln** read `RED: total 27/31 ... RED rows [17, 19, 22, 23] ... grapple owned True`. The freeze vk52 is clean
  (grapple owned False, 31/31). This is not this row, but the lead may want the cause named.

opinion (uncalibrated, decides nothing): the presses read as plain dark pillars in the entry frame. Their job (the charger's stun post)
does not show on screen without a charge in progress, which is the shape of GG's "random pillars that serve no purpose" complaint.

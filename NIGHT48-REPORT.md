# NIGHT48 report (for GG, 2026-10-19)

You asked for all 45 of your asks to be GREEN, and for the night to close early. It closed at 11:10 MST, after 3 h 31 min of building.
**Nothing was switched on for you to play.** The freeze did not run (no verbs37, no smoke, no batch suite on the close, no flow table),
so every new switch is still off. Play the n47-r3 build (`Builds/n47-r3`) as the last verified game. The n48-r6 build exists as a
candidate only.

## What is done and what you should look at first

1. **THE WICK's climb (G1).** Every top panel now has a lip shelf, and there are two extra top panels. The climb takes 27 s, the top third
   lands 82-90 % of first tries, and a human still finishes the boss in band (about 90-106 s). Frames: `Logs/feel/n48-r2/w-on/a148/`.
   Note: this is in the `climbTop48` row, which is OFF in config.
2. **The WICK's ride parry (G9).** Fixed a 19-second full-screen rainbow split that came from an open-boss hit. The hit now shows as a
   local star and rings. Frames: `Logs/feel/n48-r3/w-on/a148/shot-N29-W48-g9-rideparry-hit.png`.
3. **The Icebreaker's impacts (G30).** Slam, Block and Rush no longer wash the whole screen. Frames: `Logs/feel/n48-r3/b-ice/a248/`.
4. **The worm (G31, G33, G34).** The head-charge tell is 0.8 s and the worm's minimum fight length is now 64 s. Boosts that pay: 2.1x.
   Dives: 2.1-2.4 a minute. These are numbers, not frames, and the worm rows are OFF.
5. **THE LOOM (G36).** The hull is now a long banded spindle. Judges picked it over the old hull 3 to 0 in a blind test. Frames:
   `Logs/feel/n48-r4/l-loom/a448/shot-N29-L48-n48-long-30.png`.
6. **The warden's shield (G12).** The new plate, in its own orange with a dark rim, beat the old one 3 to 0 in a blind test.
   Pair: `Logs/n48/judge/r5-warden.png`.
7. **The LT blast (G20).** The lance won 2 to 1 over the ring. The lance is set in config, but its row is OFF.
8. **The CROWN's flame (G43).** The particle cone won 3 to 0 over the flame that ships. Its row is OFF, and its frames need retaking.
9. **The weapon kicks (G24).** On the latest reel every gun is in band and none is lighter than today. The weapon row is OFF.

## What is still SHORT, and why

- **G13 and G14 (small robots, visors):** the swarmer at 20 m is 16 x 22 px. It reads as a pill on legs at 3 to 10 m, but at 20 m it is
  too small to read. That is a scale question for you. The drone's new look lost its blind test three times, so the old drone ships.
- **G32 (the worm's body):** the molten body is still not in the shots. The beam and lava fill the frame.
- **G29, G35, G37 (motion strips and the glimpse frames):** taken and built, but I have not judged them yet.
- **G43 (the flame, boss part):** the cone won, but its row is off, and its frames need retaking after the camera fix.
- **G44 (6 hours of building):** not met. The night closed at 3 h 31 min at your request.
- **The LOOM's tips (D-34, D-37):** shots at the visible points now land, but the human boss band's minimum falls to 57 s. The switch
  stays off until the capsules are restricted to guns.

## What is decided but not switched on

Every row that won its test is in config with its switch OFF. Turning them on needs the freeze: verbs37 on levels 1-5, the smoke, the
batch suite, and the flow table. The next night's first step is those gates, then your call on each row.

## Where the detail is

- Close table, all 45 asks and the 8 R4 rows, each with its state and frame path: `NIGHT48-LOG.md` (top).
- Every decision and its reason: `DECIDE48.md` (untracked).
- Open items for the next night: `docs/memory/THREADS.md` (T-89 to T-102).
- Contact sheets and judge inputs: `Logs/n48/sheets/` and `Logs/n48/judge/` (gitignored).

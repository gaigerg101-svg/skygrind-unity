B15: PASS

Gate B15, NIGHT60. One slim critic, 2026-10-22 from about 04:38, read-only. The brief is gate-b15-brief.md. The rule is NIGHT57.md
section 0, clauses 1-5. I used 4 of the 6 images and ran 1 re-run (set C). Taste decided nothing below. The design question is GG's.

## The rule, clause by clause

- (1) MAIN met. kb3c put-ats on seeds 1-30 go 0 / 1 / 1 -> 0 / 0 / 0.
  - Before: the SUMMARY line "put at the next room ... N" in Logs/n60/L/b13/c1-A/B/C.txt reads 0 / 1 / 1. The two put-ats are both kb3c:
    `PUTAT60 n57-kiln seed 17 room kb3c` (c1-B) and `seed 27 room kb3c` (c1-C).
  - After: Logs/n60/L/b15/c1-A/B/C.txt read 0 / 0 / 0, with 0 PUTAT60 lines. NO PATH lines are 0 in all six files.
  - The cause is gone from the traces. Before, the bot was thrown into the air by the old pad and then stood at d6b's west jamb:
    - b13 c1-B: `(-9.1, 7.0, -247.2) Air`, then `(-7.5, -0.0, -253.0)` repeated.
    - b13 c1-C: `(-6.1, 2.2, -250.0) Air` and `(-10.4, 5.0, -247.7) Air`, then the same jamb point.
    - After, every kb3c trace walks through d6b on the ground, for example `(-6.9, 0.0, -254.9)` in c1-B and `(-7.1, 0.0, -256.8)` in c1-A.
  - Noise, by the letter of the rule: the baseline gap between seeds 1-10 and seeds 11-20 is |0 - 1| = 1. On seeds 11-20 the drop is 1 -> 0,
    which equals that gap and does not exceed it. The clause's other way to pass, "the entry's stated minimum", does hold: NIGHT60-BACKLOG.md
    B15 states "kb3c put-ats 0 on seeds 1-30", and the result is exactly that. Pooled over the 30 seeds the drop is 2 -> 0.
- (2) GUARDS: all hold.
  - kb3c deaths: 1 / 0 / 0 -> 1 / 0 / 0 (the kb3c room rows).
    - Before and after both have the same death line: `seed 9 24.0 s into kb3c: DEATH ... feet (10.0, -0.8, -237.8)` (b13 c1-A and b15 c1-A).
    - That spot is inside the tap run's lava (the pit rect x 7..11, z -245.5..-233.5, floorY -0.8 in kiln.plan.json), 10 m east of the new pad.
    - Only the seed-9 row after the death differs: fight 46.3 -> 42.3 s.
  - n57-kiln stall: 18.9 / 21.6 / 18.6 -> 18.8 / 18.8 / 17.9 %, all <= 23.65 (the SUMMARY lines).
  - Walks: 10/10 in all six files.
  - kb3c contract: 14/14 PASS (Logs/n60/L/contract/n57-kiln-b15/table.md and kb3c-check.txt).
    - Dwell 1.10 -> 0.35 s against a grace of 0.8. B14's c2 was 0.00 (n57-kiln-kb3c-c2/table.md).
    - Stall A 35.5 against 33.4 on B14's c2. That is +2.1 points, 6 % relative, inside the 10 % band.
    - Pads 2/2 (kb3c-pads.txt):
      - pad 6 (the blow pipe) at [0,2.5,-238.5] -> target [-9.5,4.5,-236]: launched, apex 7, landed [-9.51,4.5,-236], miss 0.01 m PASS.
      - pad 5 (the tap vent): landed [-1.01,2.5,-240.5], miss 0.01 m PASS, with the blow pipe in place. The probe steps the full sim
        (ArenaMap.CheckPads), so the blow pipe did not catch the tap vent's flight.
    - Caveat: kb3c-pads.txt names the builder's worktree level file. I did not hash that file. Its two pad lines match the shipped
      Design/levels/n57-kiln.json pads exactly (position, target, apexAbove).
  - Room table: PASS (Logs/n60/L/b15/room-c1.txt). The kb3c row is PASS, IN VIEW is 5 / 5, and I5 / I7 are PASS.
  - levelgen: 0 errors. Reports/LEVEL-n57-kiln-validate.md line 5 reads "errors: 0 (34 checks)", last written by 8419921a.
- (3) Nothing lost, and the gates are untouched.
  - verbs37 n57-kiln: `PASS: total 31/31 on n57-kiln; RED rows []`, NRE 0 (Logs/feel/n60-b7/summary47.txt lines 40 and 42, block vk15).
  - 8419921a touches no code. It changes level data, gen-n57-kiln.cjs, kiln.plan.json, docs/ROOMS.md, L.md, the validate report and logs,
    so golden and replays are not in play.
  - The lead re-made the --flow baseline in be31dac3. I did not check it.
- (4) Measured at HEAD on tonight's build, and the frames show what the numbers say.
  - The level is the one in the build. `git hash-object` of Builds/n60-b7/SKYGRIND_Data/StreamingAssets/Design/levels/n57-kiln.json gives
    a90cec34, which equals HEAD's and 8419921a's blob. The plain sha1 differs only because of CRLF.
  - a157 is GREEN (summary47.txt line 55). kb3c entry: seen 3 of wave1 6. Spawn: seen 1 of alive 6 (lines 51-52). These match
    Logs/n60/b1-summary47.txt lines 9-10 exactly. NRE 0 (line 57).
  - The frames below agree with the numbers. The pad is gone from d6b's mouth, and its beam now stands on the furnace's top edge.
- (5) Does not apply: the MAIN is a bot count, not a critic's preference.

## Re-run (one, as the brief names)

- Command: `dotnet run --project Tools/SimHarness -c Release -- n46flow --level n57-kiln --seeds 10 --first 21 --per-seed --trace --no-geo`.
  I ran it from the repo root at 04:41:58-04:43:16, exit 0, with no `--out`. The output is in the critic's scratchpad and not in the repo.
- It is byte-identical to Logs/n60/L/b15/c1-C.txt except the wall-time line (53 s here, 72 s there):
  - 0 PUTAT60, 0 NO PATH, "put at the next room ... 0"
  - SUMMARY GREEN 10/10, stall 17.9 %, deaths 0, falls 0
  - kb3c row: stall 20.8, maxStall 0.8, dwell 0.9, deaths 0, clear 21.2 (10/10)
- Against c1-C before (B13's level), the after file drops seed 27's PUTAT60 and its 15 trace lines. The kb3c row goes from stall 21.9,
  dwell 3.2, clear 23.3 to stall 20.8, dwell 0.9, clear 21.2.
- `git status --short` after the re-run: the run added or changed no entries.
  - Three entries left the list: NIGHT60-BACKLOG.md, docs/memory/LESSONS.md and gate-b15-brief.md. My first snapshot raced the lead's
    ff4e162f (04:37:39), which committed them.
  - The lead's be31dac3 (04:42:53) touched only Logs/n60/flow-base*.txt.
- For the lead: a Player (PID 84980, n60-b7, the v1off-iceline a357 reel) was running during my ~80 s re-run. n46flow is deterministic,
  and the byte-equal output confirms that. Any frame-timing number in that reel shared the CPU with it. I did not check whether that reel
  measures timing.

## The design question for GG (said, not decided)

Does the moved pad keep its job, and is "a climb from the furnace top" a fair replacement for "the loop's far end turned into a climb"?

- The job as the card states it, a launch onto the fettlers' walk, is kept, as measured: it lands at 4.5 m, 0.01 m off its target.
- The route changed:
  - Before, the pad sat on the floor (0 m) at the loop's far west end by d6b. One launch took you from the floor to the high tier.
  - Now it is on the furnace (2.5 m), 1 m in from the north-east corner of the deck x -5..1, z -243.5..-237.5. A pad route from the floor
    takes two steps: the tap vent or the furnace ramp first, then the blow pipe.
  - The floor's only one-step way up to the walk is now the walk ramp, which the plan calls "the slow way up".
  - The loop's far end (the west lane by d6b) has no upward line any more.
  - PRINCIPLES rule 1: the verb is kept, but that floor-to-high-tier route at the far end is not.
- ROOMS.md's kb3c Molecule row had already drifted from the build before B15:
  - It says "the blow pipe from the far west corner onto the furnace", and lists "the furnace top (4.5 m), the fettlers' walk (2.5 m)".
  - The plan builds the furnace at 2.5 m and the walk at 4.5 m, so the row's heights are swapped. The pre-B15 line has the same swap.
  - B15 only added "(as built: onto the fettlers' walk; NIGHT60 B15: from the furnace top's north-east corner, off d6b's lines)".
  - So neither the old card nor the new one matches the original "far west corner onto the furnace".
- The furnace stack's own why says "it cuts the furnace top's line to the fettlers' walk". The blow pipe now launches from that top to
  the walk.
  - Its straight ground track, computed from the plan and not simulated, passes about 1.9-2.3 m north of the stack's north face.
  - So the stack still covers the top's south-west part. Whether a launch line from the top to the walk goes against the stack's job is
    for GG.
- Wording checks, not defects:
  - The plan's why says the pad is "2 m from the furnace ramp's top". From the plan, the ramp's top edge (x -4..-1 at z -243.5) is
    5.1 m away. The tap vent's measured landing is 2.24 m away (centre to centre).
  - The pad's trigger radius against a person walking across the furnace top was not checked.
- The backlog's own alternative: "the fix moves or turns the pad, its job kept". A floor spot or a different aim in the west lane, off
  d6b's lines, would keep "the loop's far end". Nobody has built or measured one.

## Frames (4 of 6)

The brief's caveat is partly wrong. The spawn pose looks straight down the west lane at d6b's mouth. The entry pose shows the new pad's
beam, but not the furnace top's surface.

- Logs/feel/n60-b5/a13/a157/shot-N29-L57-n57-kiln-kb3c-spawn.png (before):
  - From (-6.5, -234) at yaw 0, the old blow pipe's disc and its cyan beam stand at the crosshair.
  - They sit at the foot of d6b's cyan-gridded doorway at the lane's end. This is the defect seen directly: a pad on the line out of the room.
- Logs/feel/n60-b7/a15/a157/shot-N29-L57-n57-kiln-kb3c-spawn.png (after):
  - The same pose. The pad and beam are gone and the lane to d6b is clear. Everything else in the frame matches.
  - The new pad is off-frame. By the plan it sits about 55 deg right of the view axis, past the 53.3 deg half-FOV, and no beam shows at
    the right edge.
- Logs/feel/n60-b5/a13/a157/shot-N29-L57-n57-kiln-kb3c-entry.png (before): there is no beam over the furnace block at frame centre. I could
  not pick out the old pad in this pose.
- Logs/feel/n60-b7/a15/a157/shot-N29-L57-n57-kiln-kb3c-entry.png (after):
  - A tall cyan beam rises from the furnace's top edge at frame centre (x ~925-990 px), with the pad disc faintly visible on the edge.
    That fits the pad at x 0, straight ahead of d6.
  - It stands just left of wave 1's drone. The a157 seen count is unchanged (3 / 6).
  - Cyan is a player-usable hue (CLAUDE.md rule 8). Whether a bright centre beam at the room's first look helps, as a signpost to the
    climb, or competes with the tells is GG's eye. No meter here measured it.
- None of the frames shows the furnace top's surface or a launch in flight. The launch is proven only by pad52.

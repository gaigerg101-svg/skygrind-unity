# NIGHT63 builder D: the review's S2 and S4 (Logs/n63/review-n63.md)

Branch n63-RV-D, from port/stage3 a86453e8. These notes are in their own file so they never clash with D.md at a pick.

## Commits, in order
Cherry-pick `4a8880d8^..<this notes sha>`.

1. `4a8880d8` **The tests for both, committed RED.** Outside Unity: 5 failed, 15 passed.
2. `db9ba03f` **S2, GREEN.**
3. `683c70e6` **S4, GREEN.**
4. This notes commit.

**Checks:**
- Outside Unity: 46 of 46 (every pure test in my scratch runner).
- dn-check 0 errors at each commit.
- Gates all GREEN 01:51:14.
- Not run: the Editor and a Player.

## S2: colours63's second scan on the wall clock
- **The read:** the second scan used `Time.realtimeSinceStartup`. decor54's second pass counts `FeelClock.UnscaledDelta` from the map's
  first frame (Decor54.cs:214). That is the reel's fixed clock while a reel runs, and 0 in a held frame. On a slow Player the wall clock
  runs ahead, so the scan could come before the pass it waits for, and two runs could differ.
- **The fix:** `Colours63Math.ScanDue(scans, frames, since)`, with `since` summed from `FeelClock.UnscaledDelta`, decor54's own clock.
  - The second scan always comes Colours63SecondScanS (3.5 s) on that clock: 0.5 s after decor54's 3 s pass, at any frame rate.
  - The first scan keeps its frame count (Colours63FirstScanFrames after the seen frame).
  - A held frame neither counts nor scans.
- **The tests:**
  - The rule at frame times 1/60, 1/144 and 1/30, and on a jittery clock.
  - The hook: no `realtimeSinceStartup`, and decor54 still on `FeelClock.UnscaledDelta`. If decor54 changes its clock, the test says so.

## S4: one name, one frame
**The read:** with `-skygrindSameRun63` the held ON frame replaced the shot's PNG. Shot29's logged stats (top band, cyan, red-orange,
mid luma) and the sidecars a helper wrote before the yield still described the shot's own frame. Examples are lamp60's door box and
colour56's rects. The held ON frame differed from the shot's own frame by 86k-193k px on b5 (the toggle lines' "held on vs the shot's
frame").

**Chosen: write the held ON frame beside the original, not re-measure.**
- The held ON frame now goes to `<side>-on/<file>` (SameRun63Math.OnDir), beside `<side>-off/<file>`. `<side>/<file>` stays the reel's
  own frame, as in a run without the option.
- **Why:** a re-measure fixes Shot29's four numbers only. Every helper's sidecar would need its own re-measure, and a new helper would
  silently get it wrong. Moving the held frame fixes them all in one place. The pair stays two held frames under one name, so tools that
  pair two folders by file name work unchanged.

**The proof line** now names both halves: `... | on <side>-on/... | off <side>-off/...`. The option line and the same63 summary say
where.

**What changes for the tools:** the pair's ON half is `<side>-on/X`, no longer `<side>/X`.
- `n63-spot-pair.cjs`, `n62-gun-yellow.py --pair` and `n63-c1-meter.py --pair` take the paths given: pass `<side>-on/...`.
- V's `n63-sheet.py` derives the off twin from the ON path (`<side>` → `<side>-off`). Give it `--off <side>-off/...`, or V adds a rule:
  a side ending in "-on" twins with the same side ending in "-off". Not edited here: it is V's file.
- Frames from before this change (b1-b7) keep the old layout: the ON half is in `<side>/`.

## S3 (G's, the table's owner's line, sent to the lead for G)
The table's bands are danger 345-35 and parry 45-70 (player 160-205, rival 285-335), in design.n63.colours63.numbers.bands. These are the
same in n63-c1-meter.py and n63-c1-audit.cjs. BossCue63 / BossTell63 should read them from Colours63Math.From(cfg), not type 345-30 /
38-62.

## Not checked
- A Player run of either fix.
  - S2: the scan's frame against decor54's pass in a reel's log.
  - S4: a reel with the option writing `<side>-on/`.
- The Editor run of N63ColoursTests and N63SameRunGameTests.

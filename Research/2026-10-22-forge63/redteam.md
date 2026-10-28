# FORGE63 red team: NIGHT63.md, GOAL.txt, slate-seed.json, builder-brief.md (2026-10-22 ~22:20, read only, nothing run)

Ranked by the run time each would waste. "Checked" names the file:line or command read. Nothing here was run in a Player or the Editor.

1. **CRITICAL (conditional, up to ~4 h lost): NIGHT62's clock blocks NIGHT63's start.**
   Line: NIGHT63.md 34-36 (rule 1: "never edit `Logs/clock.json` (only `stop` at the end)"), 55-56 (bearings step 1).
   Problem: `Logs/clock.json` is NIGHT62's right now: night n62, t0 21:46:11, staleMin 360, not stopped. `n63-clock.cjs start` refuses while a
   fresh clock exists, and it judges freshness by the OLD file's staleMin (n63-clock.cjs 26-29), so it refuses until 03:46 on 10-23. Meanwhile
   the guard enforces n62's bands: past 23:46 (n62's CAP) no build, reel set, smoke or test run starts (guard.cjs 251), and no agent starts
   past 23:16 (guard.cjs 300-301). NIGHT62's close does run `n62-clock.cjs stop` (NIGHT62.md 171), but its own escape (CAP plus a committed
   report) can end without it. NIGHT63 offers no way out and forbids editing the file. The same goes for `git tag -l u3-n62` "(must print)":
   only u3-n60 and u3-n61 exist now.
   Fix: in bearings step 1, add: "run `node Tools/night/n63-clock.cjs` first. If it prints `clock n62` and not stopped, run
   `node Tools/night/n62-clock.cjs stop`, log the line, then `start`. If u3-n62 is missing, log it under Blocked and go on (never tag NIGHT62's
   work)."
   Checked: `cat Logs/clock.json`; n63-clock.cjs 15-35; guard.cjs 85-95, 243-253, 298-302; `git tag -l "u3-n6*"`.

2. **CRITICAL: D1's same-run toggle does not fit how the rows it must judge are read, so P1', P2 and P3' can't get a pair from it as written.**
   Line: NIGHT63.md 74-75 and 80-83 (D1), 101-102 (P1' / P3' re-SPOT in b1); the slate's P2 / P3 doneWhen ("same-run pair").
   Problem: D1 is specified as a reel option that holds the sim, switches "that row's view off", captures, then restores. The rows don't
   work that way:
   - `Text61On()` is "read once per config" (UiText61.cs 30-39).
   - `BossRule61.On` is "called at a boss's rise, never per frame" (BossRule61.cs 36-37).
   - text61 is a stateful gate. `t61Gate` has a history, and `roomEnter` drops the clear card (UiText61.cs 44-55).

   So flipping the row at the capture frame changes nothing for P2: the line has already risen, which gives 0 px and NOTHING OBVIOUS. P3's
   OFF state (stacked text) is a history the toggle can't recreate, so its "off >= 3 of 5 in the same run" can't be measured. Two smaller
   faults:
   - The planted no-op "0 changed px" also fails if anything driven by unscaled time moves between the two captures: shader time, particles,
     TMP.
   - n60-v-lamp.py, which P1's OWN GAIN needs, takes two directories, `<on a157 dir> <off a157 dir>` (n60-v-lamp.py 163-167), not
     one run's interleaved frames.

   Fix:
   - Scope D1 to rows that are read every frame. Have `NightRows.ViewOn` consult a live override (`NightRows.ReelOverride`) that the reel
     sets, and have each tested view re-read it every frame: exitFrame61, bossMotion63, bossTell63, colours63.
   - For UI rows decided at an event, define OFF as "the same frame with the row's UI root hidden". That is valid for P2, which needs a
     line or no line.
   - Judge P3 on a two-run pair with the hint pinned (a reel argument that fixes the hint line) and say so in the slate, or drop P3 tonight.
   - Write the off captures to `<side>-off/` with the same file names, so n60-v-lamp.py, n62-gun-yellow.py --pair and n63-spot-pair.cjs
     work unchanged.
   - Hold every unscaled-time clock across the two captures.
   - Add to D1's doneWhen: "the toggle changes the frame for each of the rows it claims (one planted case per row)".

   Checked: UiText61.cs 28-70; BossRule61.cs 30-48; NightRows.cs 199-202; n60-v-lamp.py 163-167.

3. **HIGH: leftover #1 (P8, MapChoiceTests in the live Editor) can wedge the Editor that every build needs.**
   Line: NIGHT63.md 112-114.
   Problem: memory records that "Game M ... wedged the Pipeline (2026-10-21)" and "MapChoiceTests (606 s TIMEOUT)". Every build goes
   through the live Editor (n41-build.sh: `unity command build`). NIGHT61 lost every build and reel after the Pipeline went silent (CORE,
   L-118). "Only between builds" still allows it after b3, and a wedge there kills the freeze build, the reels and the smoke. The /goal is
   then met only by Blocked lines.
   Fix: drop P8 from tonight. Or rank it last, allow it only once, right after b1 (never after 4:00), behind a watchdog: `timeout 60 bash
   Tools/unity-loop.sh status` must answer 10 minutes later, or no Editor test runs again tonight and the lead logs it.
   Checked: n41-build.sh 1-25; MEMORY.md "Editor suite batches"; CORE.md (w61 state).

4. **HIGH: rule 8's 2 % share would have rejected NIGHT62's only PASS, and it clashes with the debt rows' own doneWhen.**
   Line: NIGHT63.md 49-51 (rule 8), 169-170; the slate's P1 / P2 / P3 "visible" rule; DECISIONS D-6.
   Problem:
   - P5 (gunYellow61) passed its critic on 20,439 changed px (NIGHT62-LOG.md, 21:57). A 1920x1080 frame has 2,073,600 px, so that is
     0.99 %: under the 2 % bar.
   - P2's line is capped at 8 words with em 36-40 px. Its glyph pixels are likely 0.5-1.5 % of the frame (an estimate, not measured).
   - "A bigger change" for P2 means more words (which breaks its doneWhen) or a backing panel (filled HUD boxes are in CORE's "Never again",
     w33).
   - `n62-gun-yellow.py --pair` prints a changed-px count, not a share (n62-gun-yellow.py 63). Every check is a hand division.

   The rule would cost each debt row a wasted round trip to its builder.
   Fix:
   - Measure the share inside the slice's named rect: the UI rect of the row, or the boss's screen rect (`--rect`). Use the full-frame 2 %
     only for B1, B2, B3 and C1.
   - Exempt P1, P2 and P3 (their doneWhen predate the rule).
   - Write the threshold as px: 2 % = 41,472 px at 1920x1080.
   - Re-base D-6's hypothesis on that.

   Checked: NIGHT62-LOG.md P5 line; n62-gun-yellow.py 6-11, 50-64; the slate's P2 doneWhen.

5. **HIGH (sizing): three fixed builds two hours apart, while a build takes ~2 min, strand picks and the one "go back once" round.**
   Line: NIGHT63.md 100, 137-143 (the clock table), 51 ("goes back to its builder ... once").
   Problem: NIGHT62 measured a build at 1:45 (21:48:40-21:50:25) and verbs37 x5 at 5:08. A pick that lands at 1:40 waits until 3:30. A
   slice under 2 % at b2 can come back only at b3, the last chance.

   Likely delivery (estimate, not measured):
   - D1 by about 1:30.
   - P2 by about 1:15, then B2 by b2.
   - B1 (five view architectures x six behaviours, plus a new reel; see 8) about 3:00-3:30.
   - C1 (the audit, routing, a meter with planted cases, >= 50 % less off-palette) and B3 (V's second slice) both hit b3 or end SHORT.

   Three builders can't land all six slices with a fair judgement on this cadence.
   Fix: build on demand. A build starts whenever a pick's gates are GREEN and no Player runs. b1 / b2 / b3 become "no later than" times;
   keep the 6:30 freeze of picks. Re-order V to B1 then leftovers, and give B3 to D after C1 (it shares C1's table; see 7).
   Checked: NIGHT62-LOG.md lines 19-27; the slate's B1 / C1 doneWhen.

6. **MEDIUM-HIGH: B1 and B2 build the same wind-up in the same five view files with two builders, and B2 may already be met.**
   Line: NIGHT63.md 84-99; builder-brief.md "The slices" (V: B1, G: B2); the slate's B1 / B2 spotPose.
   Problem:
   - Both drive a visual from the sim's attack state, in the same files: Wick37View*, Crown37View*, Brawl40View*, Ice41View, WyrmView and
     Boss37Kit.View.cs. That makes cherry-pick conflicts between V and G.
   - B1's SPOT pose ("mid wind-up") and B2's ("inside a cue") are the same frame. With both rows on, neither pair isolates one row.
   - The sim's tells already run 0.5-1.5 s (unity-config.json: tell 0.55-1.2, attackTellMinSeconds 0.8, telegraphSeconds 1.5). B2's ">= 300
     ms" may already hold for most attacks, which leaves B2 little to show.

   Fix:
   - A1 commits one shared, read-only `BossAttackClock63` (attack id, phase, steps to hit, read from the sim) that both builders use.
   - V owns the per-view hooks; G's cue goes in its own file, attached by a one-line hook pre-placed in A1 (L-049).
   - Rewrite B2's doneWhen as "the attacks whose measured cue lead is under 300 ms, or whose cue is not in the danger hue".
   - SPOT B1 at a flinch or the stagger slump, and B2 at a cue, each with only its own row on in that build (n46-build-rows on / off).

   Checked: `ls Assets/Scripts/Game` (the boss view files); `grep tell Design/unity-config.json`.

7. **MEDIUM-HIGH: B3 and C1 share one row, design.n63.colours63, across builders V and D.**
   Line: NIGHT63.md 98-99 and 104-106; the slate's B3 / C1 "row"; builder-brief.md (V: "coordinated with D through the lead").
   Problem:
   - `n63-freeze-rows.cjs` flips by row: a PASS on either slice turns on the other's unjudged changes (n63-freeze-rows.cjs 19-23).
   - Every SPOT pair for either slice carries both slices' changes, so the critic can't tell them apart.
   - Two builders write the same config block in two worktrees.

   Fix: in A1 split the row into design.n63.bossColours63 (B3) and design.n63.colours63 (C1). B3 reads C1's role table, so B3 starts only
   after C1's table commit is picked, or B3 moves to D after C1.
   Checked: n63-freeze-rows.cjs 17-24; slate-seed.json B3 / C1.

8. **MEDIUM: B1's (and B2's) proof reel does not exist and is not budgeted.**
   Line: NIGHT63.md 91-93 ("a boss reel prints each part's offset ... and the wind-up's start before each hit"); the slate's B1 / B2 doneWhen.
   Problem: `boss61` (FeelReel.N61G.cs 28-116) shoots the fight start (the rise at 1.0 s, at 1920 and 1280 res). B1 needs, on five bosses:
   attacks, hits on the boss (the flinch), a stagger (the slump) and idle, with times on FeelClock. That is a new scenario, with a bot that
   survives and lands hits per boss. Neither the brief nor the clock budgets it.
   Fix: make it V's first deliverable, before the layer: a `boss63` scenario that extends Boss61Scenario. It holds 20-30 s after the rise,
   fires at the weak point, and prints `[boss63] attack <id> windup <ms> hit <ms>` and `flinch` / `stagger` lines. Budget it 40 min. G reuses
   it for B2.
   Checked: FeelReel.N61G.cs; FeelReel.N41.cs 21-29.

9. **MEDIUM: M1 can't be measured as written.**
   Line: NIGHT63.md 102-103; the slate's M1 doneWhen ("position and yaw jumps").
   Problem:
   - verbs37 telemetry's `cam` holds fov, roll, pitch, height, x, y and z, with no yaw: `grep -c yaw` gives 0 on
     Logs/feel/n62-freeze/vk/verbs37/telemetry.jsonl.
   - The reels run on FeelClock with exactly 2 sim steps per frame (`"steps":2`, fps 60, simHz 120). The frame pacing and interpolation
     jitter GG calls "choppy" can't appear in a fixed-clock reel.

   M1 would report a number that can't show the complaint.
   Fix: M1 measures position and pitch, plus heading from the x/z path, and labels the result "fixed-clock camera path; real frame pacing not
   checked". Or it reads frame times from a `-skygrindBench 60` Player run (no FeelClock) for the pacing half. The doneWhen says which.
   Checked: head of that telemetry.jsonl, line 900's `cam`.

10. **LOW-MEDIUM: the report's "gg-proxy's ranking on the freeze frames" can't happen.**
    Line: NIGHT63.md 163-164 (report item 1), 142 (gg-proxy at 6:30), 34-35 (no new agent in the freeze).
    Problem: the freeze frames are made at close step (d), after 7:00, and the guard refuses any agent past freezeMin (guard.cjs 300-301).
    Fix: report item 1 cites gg-proxy's 6:30 ranking on b3's frames and says the freeze frames were not ranked (or ranks them by the
    lead's own read, labelled as such).
    Checked: guard.cjs 298-302; NIGHT63.md section 9.

11. **LOW: the agent cap counts five but the plan runs six.**
    Line: NIGHT63.md 43-44 and 131.
    Problem: three builders plus two GATE slots is five, and "the spotter runs in the foreground" is a sixth live agent whenever both GATE
    slots are busy. The memory crash is why the cap exists (L-128).
    Fix: "at most five agents: three builders plus two slots shared by GATEs, gg-proxy and the spotter". A SPOT takes a free slot; SPOT
    first means that row's GATE has not started yet.
    Checked: NIGHT63.md rules 5 and 7, section 5.

12. **LOW: yellow's role is undefined.**
    Line: the slate's B2 doneWhen ("a parryable attack keeps yellow56's parry tell"); NIGHT63.md 105-106 (C1 routes "the yellow edge
    lines"); CLAUDE.md rule 8 (four roles, no yellow).
    Problem: yellow56 is OFF (CORE: w60 "OFF: yellow56"), so the tell B2 promises to keep is not in the game. C1's role table has no yellow
    role, yet P5 removed the guns' yellow to keep yellow for parry. C1 and B2 can each decide this differently.
    Fix: A1 logs one D-id in DECIDE63 ("parry = yellow role in the table, used only by the parry tell" or "no yellow role"), and the role
    table carries it. B2's line becomes "a parryable attack's parry tell is untouched (whether its row is on or off)".
    Checked: CORE.md state line; CLAUDE.md rule 8; NIGHT62-LOG.md P5.

**The weakest slice: P3' (one line of text at a time, re-judged).** It is the most likely to give GG nothing to see:
- Its off state is a history: text61 is a gate decided once per config that drops the clear card at roomEnter, so the same-run toggle can't
  produce it (finding 2).
- Its red pose (n60-b8, stacked text) did not recur on NIGHT62's frames, and that SPOT said NOTHING OBVIOUS (NIGHT62-LOG.md 22:03:18).
- One fewer centre line is probably under rule 8's 2 % (finding 4).
- Even when it passes, the change is an absence.

Runner-up: B3. It is V's second slice behind the largest slice, shares a row with C1 (finding 7), and comes due at b3 at the earliest
(finding 5). Fix: drop P3' or judge it on a pinned-hint two-run pair, and spend the time on B1's reel (finding 8).

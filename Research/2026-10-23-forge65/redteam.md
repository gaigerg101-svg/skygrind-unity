# FORGE65 red team (2026-10-23, read-only; tree fa4082a5). Most severe first. Each: problem / evidence / consequence / smallest fix.

1. **The GATE window cannot hold what is put in it, and it sits where the usage limit hits.**
   Evidence: NIGHT65.md:143-151 and the clock table (:165) put a SPOT per room, one critic per builder, slot sets, the regenerate, the
   flip and rc1 (verbs37 x5 + smoke, ~15 min) into 5:10-6:00, all through ONE review slot (rule 5); a critic takes 17-23 min (L-109);
   NIGHT63's run hit the usage limit at 5:23 (CORE). Consequence: 6 SPOTs + 2 critics alone run to ~6:15; "no GATE or flip after 6:00"
   leaves the row OFF; the play card (row on vs -skygrindOff65) then compares old with old. Fix: drop R4; GATE each builder's set as soon
   as its R3 review lands (~4:10-5:00); rc1 by 5:30; and ALWAYS make a play copy with the row and every latest room on
   (n46-build-rows on a copy, labelled "unreviewed rooms") so GG's hands judge the night's work even with 0 PASS.

2. **T's start and T1's deadline contradict; the a9 finish is a hidden trap.**
   Evidence: T "branches from A1 once it lands" (NIGHT65.md:79) and A1 runs 0:10-0:50 (:80, :160), yet T1 is due "by 0:50" (:107,
   slate T1, builder-brief.md:49). R and S cannot run their sim loop without gen-n65-foundry.cjs and n65-room-check.py. And
   n52-foundry.json is not plan + levelgen: gen-n52-foundry.cjs --finish re-applies a9's n47 / n48 keys (wick47, frames47, climbTop48)
   and sets data.name "n46-foundry" (gen-n52-foundry.cjs:7-10, 429-473); T1's spec omits it, so `--check` fails on "campaign".
   Consequence: T1 ~1:30, R1 ~2:15, R3 ~4:40, collides with item 1. tour65 (a C# Player path) is first run at b1 with no fallback.
   Fix: spawn T at 0:05 from HEAD (T1 touches no A1 file; the lead cherry-picks); tell T to copy a9's row from n52-foundry.json after
   levelgen; reuse gen-n57-kiln.cjs:80-93 (swap/place). Fallback written: no tour65 at b1 -> reviewers get a157 + frames36 poses.

3. **"Replace a slot" is undefined for the room row and the top-level keys.**
   Evidence: T1 lists boxes, ramps, pads, pits, props, lights, nodes, waves (slate-seed.json:12). Measured on the plan: boxes / ramps /
   pads / pits key on `zone`; all 65 lights and 8 props key on `room`; nothing crosses its rect. The rows carry ~20 more keys: spawn,
   exit, height, roof, proofs (validator), lessons / lessons31, frames36 / frames52, highNodes, waveNodes43, standIns (a2 has NO nodes;
   Campaign24), fallReturn43 + afterLesson43 (b2, a8; Campaign24, UiHud33.Fall43), flight38 (b2), reachFrom / routeVia (a8;
   LevelValidate24, Director20). Top-level teach35 holds lesson rects inside a2 (l1.fire, l1.charge) and a3 (l1.zoom) (Lesson25). grapple
   is left out (0 today; a flight room may add anchors). Consequence: stale proofs -> levelgen ERROR, or lost lessons / fall return
   (PRINCIPLES rule 1). Fix: the room file is the whole row + its pieces + its teach35 entries + grapple; the assembler prints a per-key
   keep / replace / drop-with-reason table and refuses an unlisted key.

4. **The use flag lives in the file the PASS binds; any one room flips the level.**
   Evidence: builders set `"use": true` each round (NIGHT65.md:137); n65-slot-set then sets use after a PASS (:55, :110). A write to the
   room file after passSha makes n65-freeze-rows refuse (n65-freeze-rows.cjs:57-60); a FAILed room keeps use true and ships. The flip
   matches `String(s.row).includes("design." + key)` (:47) and every room slice's row contains "design.n65.foundry65", so one room PASS
   flips the row; LV's doneWhen (verbs37 27/27, NRE 0, flow --off65) is never required. Fix: ship flags in a lead-owned
   Design/levels/n65/slots65.json written only by n65-slot-set; round builds pass `--slots=` (as gen-n57-kiln.cjs:12, 60); room rows say
   "slot a2 of n65.foundry65"; LV needs its own verdict-LV.md.

5. **The walk bot drives the OLD rooms' scripts inside the new ones; b2's guard is vacuous.**
   Evidence: Walk43Foundry.cs:279 runs when BaseId is n43-foundry (n65 -> n52 -> ... -> n43 holds); b2's Chain43 gates at lz 45.5-57
   (:283-299) and a8's Pierce43 (lip at lz 18.2, perch, P2; :310, :324+) are the old geometry. D-8 keeps the ids on purpose. n46flow does
   not measure links and puts a stuck pilot at the next room after max(30 s, 3 x par) (Harness.N46.Flow.cs:253-256); flow-base's
   foundry rows are a2..a8 without b2 (Logs/n64/flow-base.txt:4-9). Consequence: "clears 10/10" fails a8 for the bot's reason, or a broken
   gantry passes. Fix in A1: a room-row opt-out (e.g. `bot43: false` -> the generic autopilot) or write a8 / b2's guard as "not
   applicable: scripted pilot"; for b2 print the put-at count on b2 (old vs new) as its crossing guard.

6. **The re-banded contract's command is the wrong tool.**
   Evidence: n52-room-check.py takes `<plan.json> <room> [--report --hold --flow-before --flow]` (n52-room-check.py:5-7, 21-22); the loop
   runs `n65-room-check.py n65-foundry room <id>` (NIGHT65.md:130, builder-brief.md:34), which is n59-l-room.py's CLI (R2 / R5 / R8 /
   IN VIEW / I1-I7, not the contract). The contract's dwell / hold items come from n59-l-contract.sh, which hard-codes n52-room-check.py
   and wants slots<NN>.json and openings<NN>.json. "Height by the card" is not computable. Consequence: round 1's loop errors or checks the
   wrong thing. Fix (A1): two named commands, `n59-l-room.py n65-foundry room <id>` and a copied n65-l-contract.sh calling
   n65-room-check.py; write slots65.json (base room per slot) and openings65.json; height band = the room file's declared height.

7. **b2 (104 m), a2 (14 x 20) and a8 (14 x 22) repeat the GAP NEST corner.**
   Evidence: plan rects (b2 [-10,-288,10,-184], a2 [-7,-55,7,-35], a8 [-7,-364,7,-342]); doors d1-d10 fix every rect. The re-band's long
   side is 14-32 m (D-4): b2 fails item 1 by construction and needs 52 pieces for 2.5 / 100 m2. None of research-rooms.md's archetypes
   fits a2, a8 or b2 (A 20 x 30, B 14 x 32, C 18 x 18, D 24 x 24, E 26 x 26; :152-183). L-112 / history.md section 3 rule 6 (w58-w60).
   Fix: the card gate rejects any card whose archetype does not fit the rect; b2 judged as a traversal (verbs37's 25-30 m wall and 14 m
   lane plus the crossing), exempt from item 1; or swap b2 for a4 PARRY (leftover 2).

8. **Half the slots have no red frame, and the cards are due before the red set runs.**
   Evidence: a157 frames only kind-room rooms with frames52.entry (FeelReel.N57.Level.cs:4, 58, 116); in the plan only a3, a5, b1 have
   frames52; a2 / a8 lack it, b2 is a link. tour65's fallback is frames52.entry too (slate T2). Cards are due 0:05-0:45 (NIGHT65.md:115)
   but the red set runs after A1 (:89), and the brief says to open the red frames first (builder-brief.md:12-13). Consequence: no SPOT
   pair for 3 of 6 rooms; cards written blind. Fix: fallback to frames36.mid (every room has it); cards use Logs/feel/n64-freeze/vf
   (37 kept frames) + plan PNGs now; the lead's sim red at 0:05.

9. **The room's MAIN is a count any rebuild meets, and every judge reads stills.**
   Evidence: MAIN = a verb line "the validator's routes and a tour65 frame show" + pieces / props with jobs above the old room's
   (slate-seed.json:111). The old a2 has 1 box, 1 ramp, 0 props; a8 / b2 have 0 props (measured). The validator's routes are walk routes
   (LevelValidate24), not a ride or boost line. gg-proxy, the design reviewer, the SPOT and the critic all read frames: corners (a) (c)
   (f) and rule 7 (history.md:63-68, 82-83). The forge's own route-pilot MAIN (history.md:136-139) was dropped. A whole-room rebuild makes
   the SPOT name "a change" every time: it measures nothing. Fix: add measured play-like lines per room against its red: n46flow air % and
   STYLE (flow-base a2 air 73.9 %, a8 33.6 %), hold52 --pads touches, and on ride rooms the ride pilot (`n53ridekill ride`, GREEN on a3).
   Say "the verb cannot be skipped: not checked" until a pilot shows it.

10. **The reviewers hold a veto they have used every night.**
    Evidence: GATE-ready = "no BLOCKING finding left from both reviewers" (NIGHT65.md:144); gg-proxy called the rooms "empty boxes" three
    nights running (Logs/n64/gg-proxy.md, cited at NIGHT65.md:19-21); nothing says what happens to a BLOCKING finding after R3.
    Consequence: 0 GATE-ready rooms, nothing ships, the "four rooms PASS" bar fails on judge taste. Fix: BLOCKING must name a piece and a
    rule or a GG quote and be fixable in one round; after R3 the critic decides with the open findings listed.

11. **The disk runs out before the close, and the /goal accepts that as done.**
    Evidence: 14 GB free (df); the guard refuses builds, reels AND smokes under 12 GB (guard.cjs:27, 239-242). Planned writes: n65-red as
    a byte copy (n64-freeze is 382 MB), b1-b4 (383 MB each, keep two), rc1, the freeze, three new worktrees (an agent worktree measured
    191 MB) plus three SimHarness builds; the live reel side is pruned only after it ends (n65-prune.sh:14-19). 232 worktrees are
    registered; Builds/ holds 79 GB in 255 entries (du), so GG's one call on old builds frees far more than the night needs. Consequence: under 12 GB near rc1 / the close; GOAL.txt then lets "each failure logged under Blocked" close a night with
    no verified freeze. Fix: GG's yes on DECIDE65's deletions before t0, or branch A-lean under 16 GB; a directory junction instead of
    the n65-red copy; keep only the last b-build; the minimum close's play build must be a verified RC.

12. **The builders mostly wait, and D-9 would hide it.**
    Evidence: builders must run `n65-gates.sh --no-editor` each round (builder-brief.md:37-38): golden + 10 fingerprint dotnet runs, none
    of which reads level data (code-scout.md:115-120; n65-gates.sh:30-35); three builders run it in parallel. During build, reels and two
    sequential reviews (~45-50 of each ~70 min round) they wait. D-9 counts only the minutes tagged BUILD / CHECK. Consequence: about 75 min
    of building per builder all night, reported as "70 % building". Fix: gates only when C# changes; builders start round n+1 from their own
    sim outputs while the reviews run; D-9 over wall-clock.

13. **Level 1 loses its colour grade when the row flips.**
    Evidence: design.n49.look49.numbers.levels names the literal "n52-foundry" (Design/unity-config.json:8168), matched exactly
    (PostFx.cs:491-520); A1 adds only Look52TierLevels (NIGHT65.md:84). Consequence: every room, kept ones included, changes grade:
    the SPOT and gg-proxy see a recolour, and PRINCIPLES rule 1 is broken. Fix: A1 adds "n65-foundry" to look49's list (both copies) +
    one test; grep the config for every literal "n52-foundry" (also :395 maps.list).

14. **The /goal mixes rules into its met condition.**
    Evidence: GOAL.txt states rules ("Every rebuilt room ... gets at least three rounds", "A room enters the level only with ...") inside
    the "Met when" text, and a small model judges it. n65-clock.cjs reports phase NONE once the clock is stale (staleMin 600, :13, :21), so
    "prints phase CAP" cannot appear after a stop longer than 10 h. Consequence: a room with two rounds can read as "goal not met" and keep
    the Stop hook running past 7:00. Fix: a "Rules (not conditions)" heading; "a room short of three rounds is reported SHORT"; the CAP
    branch also accepts "phase NONE (stale)".

15. **The seed band in the GUARD has no red.**
    Evidence: GUARD "deaths and stall within the old room's seed band (seeds 1-10 vs 11-20)" (slate-seed.json:112); the red step runs only
    `--seeds 10` (NIGHT65.md:91-92). Fix: run the red with the second seed set too (as n59-l-contract.sh does), at 0:05, sim only.

Not checked: whether levelgen of today's n52 plan reproduces n52-foundry.json byte for byte (I ran no levelgen); tour65's real run time.

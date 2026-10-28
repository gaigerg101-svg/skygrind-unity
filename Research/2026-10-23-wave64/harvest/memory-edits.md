# w64 harvest draft: proposed edits to docs/memory (builder G, NIGHT64 leftover 1, written 11:29-11:35 from the state at 11:29-11:32)

Sources read: NIGHT64-LOG.md (167 lines at 11:29), Logs/n64/slate.json (`node Tools/night/n64-slate.cjs`: C4 short, U1 measuring, B3 building,
L1 pass, L2 todo), Logs/n64/verdict-C4.md, verdict-L1.md, spot/C4.txt, spot/U1.txt, c4-door-b1.txt, u1-dup-b1.txt, rc.json (rc0 only),
DECIDE64.md (D-1), Logs/player/n47-reel-n64-b1-{uk,ukoff,ul,ui,uioff}.log (the `[order64]` quit lines), Research/2026-10-23-forge64/DECISIONS.md
(D-3, D-6, D-8). The sealed eye63 mapping and the chop output's direction were not read. Everything marked "(pending: the close fills it)" is
for the lead at the close. CORE.md is the lead's (not drafted here). Apply as exact row edits; each file's line count after is given.

## THREADS.md (60 lines now; 60 after)
1. Line 1 (the header): append `, w64 2026-10-23 (harvested at NIGHT64's close)`.
2. **T-153** (ONE COLOUR LANGUAGE): in "last number" append: `; w64 C4 goal64 SHORT: THE CROWN's goal column 342 -> 130 DANGER px in spire's
   door boxes on n64-b1, BOUNDARY 0: Crt29's TV fringe puts a red edge on every thin bright line, so "0 danger px" is unreachable
   (Logs/n64/c4-door-b1.txt, verdict-C4.md, DECIDE64 D-1); B3 (pending: the close fills it)`. In "next approach" append: `; GG: the fringe on
   thin lines and the column's place (DECIDE64 D-1 a-c); a thin-line bar measured on a planted line first (L-147)`.
3. **T-152** (THE CAMERA'S CHOP): in "last number" append: `; w64 L1: the blind A / B kit built and PASS (Builds/n64-abx, n64-aby from A1
   d86ba168, one config line apart; verbs37 n52-foundry 27 / 27 each; view only 0.90 mm; Logs/n64/verdict-L1.md; the card in NIGHT64-REPORT.md
   section 2)`. Next approach becomes: `GG plays X and Y and answers in DECIDE64; only then open Logs/n64/eye63-ab.mapping.txt; (a) the VSYNC-on
   bench (NIGHT64 leftover 4, pending: the close fills it)`.
4. **T-104** absorbs **T-115**: T-104's id cell becomes `T-104 (+ T-116, T-108's look49, T-112, T-121, T-115)`; append to its item `; T-115
   (w50): the full before / after sheet (G6, with the charge and the kit) never built (n52-sheet.py makes sheets)`. Delete T-115's row (one
   line freed for item 5).
5. **Only if U1 or L2 ends SHORT** (pending: the close fills it), add one row in the freed line:
   `| T-155 | THE BOSS TEXT (w64): <U1 order64 / L2 fightLine64, the one(s) SHORT> | w64 | <the GATE's number and verdict file> | <the critic's
   named cause> |`. If both ship, add nothing (the line stays free). Known so far: U1 on b1, row on, duplicate shown 0 / 0 / 0 (uk wyrm 398
   would, ul loom 206, ui icebreaker 1186) and orders shown equal on and off; the telemetry meter reads ui 2 frames (two order changes, 1 each)
   while the in-game counter reads 0: not explained (Logs/n64/u1-dup-b1.txt, n47-reel-n64-b1-ui.log).
6. No thread closes outright tonight (pending: the close fills it: T-150's D-4 stays GG's; T-154 unchanged).

## FEATURES.md (133 lines now; 136 after)
Add after the NIGHT63 rows (status cells pending where marked):
```
| THE EXIT THAT READS DANGER (C4: THE CROWN's goal column in the colour table's goal role, read with Colours63Math.From; the window shafts keep their red-gold) | Game/Goal64Math.N64D.cs, Route47View.cs (Goal64Column); Tools/night/n64-c4-door.py (STRICT / CORE, the TV fringe printed apart, 11 planted cases); N64GoalTests | design.n64.goal64 (view; -skygrindOff64, -skygrindNoGoal64) | w64 | OFF, SHORT (critic FAIL, Logs/n64/verdict-C4.md; DECIDE64 D-1) | not recorded | n64-b1 same-run a157 n52-spire: the column's DANGER px 342 -> 130 (fringe 87), BOUNDARY 0, 0 / 3 boxes (Logs/n64/c4-door-b1.txt); SPOT "pale white in A and clearly red-orange in B" |
| ONE ORDER AT A TIME (U1: bossbar43's hint not drawn on a frame hud47's fight line draws the same order; else as before, so it returns to the bar after the line) | Game/Order64.N64G.cs, UiOrder64.N64G.cs, one line in UiHud47.Boss.cs Fight47Late; Tools/night/n64-u1-dup.cjs (telemetry meter, planted); N64OrderTests | design.n64.order64 (view; -skygrindOff64, -skygrindNoOrder64) | w64 | (pending: the close fills it) | not recorded | the in-game `[order64]` counter: n64-b1 uk wyrm would-duplicate 398 / shown 0 vs ukoff 399 / 399; ui 1186 / 0 vs uioff 1184 / 1184; orders shown equal on and off; SPOT "A shows a second, smaller ... line under the boss health bar" (A = off) |
| THE FIGHT LINE IS THE FIGHT'S (L2: a room title the fight line draws while the boss bar is up is not drawn that frame, the queue untouched; held and posted to the message slot once after the fight) | Game/FightLine64.N64G.cs, UiFightLine64.N64G.cs, one line in UiHud33.cs RenderHud33; Tools/night/n64-l2-titles.cjs (telemetry + level rooms, planted); N64FightLineTests | design.n64.fightLine64 (view; -skygrindOff64, -skygrindNoFightLine64) | w64 | (pending: the close fills it) | not recorded | red on n63-b12 boss63: a room title as the fight line mid-fight WICK 170, CROWN 170, LOOM 170 of 1529 frames; (pending: b2's `[fight64]` numbers) |
```
Edit **THE EASED EYE** row's status cell to: `OFF, BLOCKED (measure MET; GG's blind A / B: w64 L1's kit, Builds/n64-abx / n64-aby, PASS, Logs/n64/verdict-L1.md)`.

## LESSONS.md (150 lines now; 149 after: three rows out, two in)
1. **Delete L-080's row**: merged into L-104 since w60 (L-104's text already says "L-080 merged here").
2. **Replace L-119's row** (L-034 and L-125 merged into it; delete both their rows):
```
| L-119 | Trust a dn-check only from its own scratch folder: one folder per builder and per lead check (never a shared name such as `dn`), deleted after each run (about 681 MB; a worktree about 165 MB); run `dn-check-wt.sh` (plain copies), never `dn-check.sh` (it registers a git worktree, and removing one is GG's call); and plant an error once a night, since a checker that always prints 0 is worse than none (L-034 and L-125 merged here) | w30: dn-check-wt.sh's sed ate "error CS", 4 builders reported 0 while the Editor broke (302c1e8d); w61: C: 8.3 -> 5.1 GB in 1 h 40 min with four scratches, 4.5-4.8 GB after delete-after-check (DECIDE61 D-12); the guard refused `git worktree remove --force` on the lead's dn-check scratch; w64: builders D and G both used `scratchpad/dn` the same minute, G's GREEN read 3384 false errors (the folder emptied mid-build), 0 in its own folder (NIGHT64-LOG "Picks") | lead, builders, disk, gates | confirmed | w30, w61, w64 |
```
3. **Add L-147** (new):
```
| L-147 | A thin bright line cannot meet "0 px of a hue" under the TV pass: Crt29's one-virtual-pixel red / blue split (UnityTuning.Look35FlatChromaPx, over the whole world after post) gives every thin line a red left edge, a green-white core and a blue right edge whatever its colour; before a slate bar counts a thin line's hue, measure the fringe on a planted line of the target colour in a Player frame, and judge the line's core (or draw it after the TV pass) | w64 C4: the goal column 342 -> 130 DANGER px on n64-b1 (fringe 87, off it 43), BOUNDARY 0, the critic's column sample red 3 px / orange / core hue 150 / blue; FAIL on a bar no colour could meet (Logs/n64/verdict-C4.md, c4-door-b1.txt; DECIDE64 D-1); builder D predicted it from NIGHT63's colours63 preview (1015 of 1069 DANGER px were the fringe) | forge, look, meters | new | w64 |
```
4. **Add L-148** (new):
```
| L-148 | When the first slices land early, build their GATE at once and move the late slice to the next build: a GATE kept for its planned slot idles the Player while the clock runs | w64: U1 and C4 reported 11:13 / 11:14, 30-40 min before their deadlines (slices pinned to file:line by the forge's code scout); b1 started 11:17:40 (0:26), 39 min before the plan's 1:05 slot, both SPOTs by 11:20:49 and C4's critic verdict by 11:23:34 (the files' mtimes); B3 moved to b2 (NIGHT64-LOG "A lead's call"; NIGHT64.md section 6) | lead, forge, planning | new | w64 |
```
5. **Confirmations** (append to each row's "confirmed" cell):
   - L-127: `, w64 (C4's red measured on rc0 by the lead's red set before any slice code; U1's and L2's reds read from n63-b12's boss63 telemetry before their GREEN)`
   - L-128: `, w64 (a gate run launched at 14.7 GB commit free, read only after the launch; the read moved into the same command, before the job: NIGHT64-LOG "Memory, a slip")`
   - L-130: `, w64 (C4's and U1's SPOT pairs first on n64-b1, both named their change: spot/C4.txt, spot/U1.txt)`
   - L-132: `, w64 (L1's critic 11:11 -> 11:13:47, C4's 11:21 -> 11:23:34: the verdicts' mtimes)`
   - L-134: `, w64 (every SPOT pair same-run: C4, U1)`
   - L-145: `, w64 (C4 SHORT by its critic's FAIL on the frames, not by D's own meter, which had predicted it)`
   (pending: the close fills it: B3's tuning loop, L2, rc1 and the close's length may add or confirm more.)

## OWNER.md (124 lines, already over its 120 cap: a consolidation is due, not drafted here)
Add the ask under its table, verbatim: `| 2026-10-23 ~08:30 | (after NIGHT63's close, no play; NIGHT64.md line 3) "is prompt fully finsihed or do we
need to handoff and continue, create handoff and use that, resrecah, follow best methods and create next prompt that will go for 3 hours" |`.
GG's eye63 answer (DECIDE64), if it comes, goes there and in ledger/wave-64.md's Reaction line.

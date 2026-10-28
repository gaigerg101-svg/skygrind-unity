# Wave 57 (NIGHT57, SHIP AND DEEPEN; 2026-10-21 09:34 - 14:3x MST; tags u3-n57-r1 (b34438ce), u3-n57)

**Ask (GG 2026-10-21, verbatim in NIGHT57.md and OWNER.md):** "i cant play it now so i need you ti figure out, study game deisgn check
doucments and see where we are missing, use skills, look at github"; then, to the lead's four questions, "1. you decide  2. yes  3. yes
4. will do write pormpt now" and "start it immediatley, building" [reading: the shipping rule is the lead's call; enemy ATTACK behaviour
may change; level 3 rebuilt from scratch; GG restarts the Editor].

**Before the run:** the Editor's Pipeline was wedged (a run_tests stuck at "Executing IPostBuildCleanup for:
Unity.PerformanceTesting.Editor.TestRunBuilder"; an orphaned n41-suite runner); GG restarted it. Gap analysis
(Research/2026-10-21-wave57/GAPS.md + research/design.md + research/github.md): levels 3-5 had 3 combat rooms each, the roster fell back at
level 4, ~12 built rows OFF, no judge checked against GG.

**Prompt shape:** NIGHT57.md (~260 lines, untracked), red-teamed by a fresh reviewer before t0 (3 critical findings fixed: n57-kiln
unlisted in maps.list, kiln47's beats keyed on N52LevelFrom, the checkers hard-coded n52); 4 h min / 5 h cap; lead + 3 builders (L level 3,
G enemies / kills, V colour / near misses) + one batched critic per build; builds r1 (10:36), r2 (11:36), r3 (12:23), freeze (12:53).
Forge experiment: the shipping rule (MAIN beats the baseline beyond noise, GUARDS within band, nothing lost, the critic checks evidence).

**Numbers (bot unless it says Player):** level 3 n57-kiln: combat rooms 3 -> 5 (all new), clear 92.2 -> 168.2 / 171.9 s, stall 25.5 ->
19.0 %, deaths 8 -> 2; wyrm55 deaths per fight 1.70 -> 0.80, median 122.6 -> 86.8 s; tokens56 attacks on screen 44.7 -> 85.0 %; wick55
damage in windows 6 -> 57 %; grade55 CROWN tells 0.68 -> 0.99 (Player); colour56 key / accent 22 -> 36 / 36 frames, world over tells
3 / 36, greyscale held 25 / 36 (Player); brief55 overlap 1 -> 0 frames (Player). Freeze: verbs37 27 / 29 / 31 / 31 / 31 (n57-kiln 31 / 31),
the smoke DONE (232.3 s, both planted steals took focus).

**Shipped ON (7):** kiln57, wyrm55, tokens56, brief55, wick55, grade55, colour56 (DECIDE57 D-9..D-17). **OFF:** press57 (MAIN missed),
kill55 (marks 0 %: the next frame is white flash), rim56, yellow56; rank56 retired. Forge experiment: 7 ON / 11 gated (NIGHT56: 1 / ~12).

**SHORT / not checked:** a person riding onto kiln57's 4.5 m tiers; kiln57's door exits (the harness moved the bot on 15 / 17 times vs 3 / 4);
in view at the door kb3c 3 / 6, kb3d 2 / 5; Game M's hang (after the tag, X1); the judge calibration (FAIL; frames unequal).

**What went wrong:** the disk ran out mid-r2 (35 -> 8.7 GB: reel frames 13.4 + 8.8 GB): r2's later reels silently refused; fixed with
n57-prune.sh after every reel batch. Two checkers read a row wrong and were fixed by the builder whose row they judged (the teach checker on
keyboard; verbs37's staging before kiln's award room, D-16): each fix was reviewed by a fresh critic.

**Closed threads:** T-137 (N36TestBaseline SetUpFixture forced heal36 off for every Game test). T-135 partly (brief55, grade55 CROWN,
colour56, wyrm55 shipped; kill55, rim56, yellow56 remain).

**Lessons:** L-103 (a baseline-relative shipping rule ships built work: 7 / 11 vs 1 / ~12), L-104 (prune reel frames after every batch: a
reel set writes ~10 GB and n47-reels.sh refuses under 10 GB without failing the batch), L-105 (a stills judge does not predict GG: calibrate
on played reels), L-106 (when a builder's checker change makes its own row pass, a fresh critic reviews the diff).

**Reaction:** (none yet)

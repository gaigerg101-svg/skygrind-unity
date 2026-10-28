# Wave 53 (NIGHT53, THE SECOND PASS): the rooms made places, the polish SHORTs (planned 2026-10-20 07:05-07:28; run 07:39-~10:5x)

Written by the forge (wave-forge, a 20-minute box). The close replaces the status lines below (NIGHT53-LOG.md's close table, NIGHT53-REPORT.md).
Planning set: NIGHT53.md (untracked), Research/2026-10-20-wave53/RESEARCH.md.

**Ask (GG, 2026-10-20 ~07:05, verbatim in NIGHT53.md and OWNER.md):** another 3.5 h (minimum 2.5 h) of polish, build and iterate with
NIGHT52's method; research proven data, high-star GitHub projects, experts and the newest model's practice; the prompt as a loop and a goal.

**Prompt shape:** lead + 2 builders (K rooms: kb3, p1, a3; U polish: T-122, T-111, T-120, T-116, a HUD kill pop with the installed
UIEffect / LitMotion); 3.5 h cap, 2.5 h minimum; rooms edited in place in the n52 plans (no new level ids), U's items behind
design.n53.polish53 (-skygrindOff53); P0 45 min (the shared commit and tools, builders spawned at ~0:12, T-126 at its source, the identity
checks I1 / I4 / I5 / I7 with planted failures, a plan-only baseline); the loop 0:45-2:00; freeze 2:00-3:00; close 3:00-3:30; kickoff +
/goal (1,202 chars) + /loop (self-paced, one cycle per iteration, stops itself at 3:30 or the tag). A fresh red-team reviewer's 12 findings
were applied before delivery (P0 and the freeze too short, tools to create, I6 / I7 not computable, the /goal-/loop wakeup race).

**Forge experiment:** the run is a /loop of fixed-shape cycles with a cycle ledger (pick, change, check, keep or revert, minutes) instead
of phase blocks. Hypothesis: a building share >= 75 % (NIGHT52: 68 %) and >= 4 kept cycles in the loop's 75 minutes. Measure: n53-share.py on
NIGHT53-LOG.md and the cycle rows counted.

**Research used (RESEARCH.md):** no primary source gives arena metrics in metres (A); the identity checks come from DOOM's push-forward
and Lazarus Labs loop (A-1, A-2), Halo's "no 30-second stretch repeated" (A-5) and the Level Design Book's openings (A-8); DotRecast (zlib)
as the boxed fallback for path lengths (B-1); UIEffect / LitMotion (MIT, already installed) for the HUD pop (B-2, B-4); /goal reads only the
conversation, /loop self-paced ends by ScheduleWakeup stop, worktrees branch from main (C-1..C-3).

**Shipped (play build Builds/n53-freeze, d2ec09ad; tag u3-n53):** rooms edited in place: kiln kb3, span p1, foundry a3 (builder K), iceline g4,
g3, g7's pad (the lead); polish53 ON with the kill pop, the scorch core, iceline's grade (T-122 / T-111 off: DECIDE53 D-15); the identity
checks Tools/night/n53-identity.py (I1-I5, I7; planted FAIL each; I2 / I3 on the level file's navGrid, never on lava); T-126 fixed at its source.
**Numbers:** identity room FAIL items 33 -> 27 (Logs/n53/identity-u3n52.md -> identity-freeze.md; kb3 and p1 all PASS); I5 level FAILs
2 -> 0; I7 1 -> 1 (foundry). verbs37 on n53-freeze 27 / 21 / 31 / 31 / 31 (kiln +1). Flow: iceline clear 246 -> 221 s (cross gone), foundry
dwell 16.8 -> 13.9 s (still a cross), iceline stall +1.8 points (new cross), span stall 15.6 -> 14.9 % (cross). The full suite / smoke: the
close table in NIGHT53-LOG.md.
**SHORT:** K (a3 I1 69 %; p1 / a3 / kb3's last cuts unreviewed), L (g4 / g3 I2 FAIL; g2's mirror not cut, D-14), U (T-122, T-111 off),
R (the 09:00 reviews: none of g2 / g3 / g4 / kb3 would satisfy GG; D-13).
**Forge experiment, scored:** building share 53 % by n53-share.py (target >= 75 %: MISSED; NIGHT52 68 %); kept loop cycles 4 (q1 I2 / I3,
g4, g3, g7's pad: MET). The fixed-shape cycle kept rooms moving, but each room cycle spent most of its minutes in checks (the contract's
dotnet jobs, the reels); the log's CHECK lines outweighed BUILD.
**Reaction:** none yet.

**EXT53 (GG 2026-10-20 ~10:44, verbatim in OWNER.md: "continue this prompt for another 2 hours ... minimum 2 hours, hard cap at 3 hours ...
clean up fix bugs and get creative"; 10:47-12:52).** Shape: 4 builders (M mirrors, N RIDE KILL, B bugs, K2 rooms) + A5 after the freeze;
the lead integrated, ran every proof, fixed the level-report errors tonight's edits left, moved 5 pads with a search tool; the full batch suite
run before the freeze build so the reels ran alone. Shipped (Builds/e53-freeze2, 0687ddbc = the freeze + A5's foundry a5; tag u3-n53x): RIDE KILL ON, firstShot ON,
6 rooms re-cut, 5 pads, the allocation flake and T-122 fixed. Numbers: identity FAIL items 27 -> 11 (9 rooms all PASS, with a5); suite 2258 / 2258 at the freeze commit;
verbs37 27 / 21 / 31 / 31 / 31 with RIDE KILL on; smoke DONE. SHORT: T-111, T-125's Player cause, the reviewers' "cuts, few added
pieces with jobs" (D-22). Lessons L-093..L-095. Reaction: none yet.

## Closed threads (moved from THREADS.md, consolidate 2026-10-20)
- T-126 (w52) Lesson25Tests.TheOrder_FollowsTheBlocks LEAKED STATICS (NightRows.ForceOffFrom = 46 and N38 / N40-N45 ForceOff without a
  restore; N52SharedTests' ViewOn RED, 2237 / 2238 at bee5387f; the victim pinned in 907dc105): CLOSED NIGHT53 (d748867d): the test
  restores every static it sets; the Editor pair RED unpinned, GREEN fixed.
- T-122 (w52) THE MUZZLE LIGHT ON A LONG FRAME (guns52's 40 ms light stretched by a first-shot hitch: n52-freeze KESTREL 134 ms over 3 frames
  at a 22 ms frame time): CLOSED at EXT53: the THUNDERCLAP's first shot built fx47's sprite sheets in its frame; firstShot ON (the sheets
  prewarmed), 5.5-9.0 ms. T-130's other parts stay open in THREADS.md.
- The AutopilotTests / ZeroAllocTests allocation flake (NIGHT14-NIGHT53; the NIGHT53 close's suite SHORT): CLOSED at EXT53: the tests'
  GC.Alloc recorder, fixed in the tests.
- Kiln's short teach line: answered at EXT53 (the queue rule, not a bug).

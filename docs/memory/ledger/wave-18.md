# Wave 18: feel in motion, measured (2026-10-06 08:22 to ~14:00, branch port/stage3, box 5.5 to 6 h, tag u3-n18)

**Ask:** GG (after NIGHT17, 2026-10-06 ~8 AM): "follow the best available methods ... analyse the results ... what did
not get completed or built at all ... create a prompt that will show real tangible improvements" (NIGHT18.md header).
GG had not played NIGHT17's build. NIGHT17-RETRO: reviewers 25 / 23 / 27 / 27 of 50; kill crunch, movement and motion
rows flat at 2. Task U3-31.

**Shipped** (final build 7; NIGHT18-REPORT; NIGHT18-LOG close table):
- The feel harness, built first: FeelReel, FeelProbe, FeelClock, FeelAB, Tools/feel-metrics.py (M1-M12).
- The kill at every range: slabs, white kill marker, freeze ladder, dark disc.
- The hit spring (EnemyViews.Hit.cs, HitSpring.cs); camera feel (boost / sprint / slide kicks, strafe roll, land dip).
- HUD and menu motion.
- The backlog: flourish 3fd4133, glint b689132, spawn ring ce32e9d, low-HP pulse efa0653, HUD STYLE ATTACHED dab6fb8,
  per-gun recoil 5ec11cf.
- 99 commits `u3-n17..u3-n18` (git rev-list).

**Numbers (NIGHT17 -> final; NIGHT18-REPORT §2-3):**
- Metrics PASS 56 -> 104 of 125.
- M1 1.97 -> 3.05; M3 0.73 / 1.05 -> 1.76 / 1.59 body heights; M5 2.7 -> 12.6 px; M8 18 / 18 -> 0 frames in the middle.
- Bench p99 17.66 / 34.32 -> 15.62 / 13.42 ms.
- Blind final pair (tonight's side picked): kills 8 / 8, hit 2 / 2, movement 7 / 10, HUD 5 / 8, menus 8 / 10.
- Rubric 34, 35 of 50 (NIGHT17: 25, 23, 27, 27).
- Suite 1062 / 1062; golden 22 exact; fingerprint EQUAL; smoke DONE.

**Close: 6 DONE / 4 SHORT** (NIGHT18-LOG close table: DONE U3-31.1, .2, .4, .5, .9, .10).
**SHORT:**
- .3 kill: horde5 "too much" (spark confetti) after the 4:50 freeze.
- .6 HUD: blind 5 / 8 < 6; roundend pops over the banner.
- .7 menus: 2 menu pop-ins (M9); the RESULTS grade card over POINTS.
- .8 rounds: a fourth round didn't fit; builds 2, 4, 5 spent on the reel's input path.

**Prompt shape:** NIGHT18.md 569 lines; GG's switches (4 rows), contract, 10 items, the harness spec first; box 5.5-6 h;
builders 5 (X kill, R hit, M movement, H HUD, N menus + the metrics tool) + the lead. Build share: not checked. Forge
experiment: "an effect is done when the screen measurably changes, and blind judges pick it over the old build in both
orders"; fewer, deeper items. Outcome: the best DONE ratio of the three waves and every judged row up.

**What went wrong in the run:**
- Three of seven builds lost to the reel's input path (a pad's trigger fired shots; a stale mouse delta).
- Another project's jobs held the CPU at 100 % at the start (NIGHT18-LOG close U3-31.1).

**Lessons added or confirmed:** L-001, L-002 (the reviewers' speed-line preference didn't match GG later), L-016, L-017,
L-018.

**Reaction** (GG's next request, NIGHT19.md "What GG asked for", 2026-10-06; verbatim in OWNER.md): "The stamina bar
should not be just in the middle of the screen, it's really ugly ... Don't be afraid to copy and pull assets, because a
lot of the UI and effects and animations still look a little cheesy." His later words on this wave's speed lines: "just
so forced" (NIGHT20.md).

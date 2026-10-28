# DECIDE55: the calls the lead made without GG (NIGHT55, the catch-up and push night; 2026-10-20 from 17:05 MST)

Each call: what was decided, the alternative rejected, why. GG can reverse any of them; the row it names turns it off.

- **D-1 The clock.** t0 = 17:05:42 (the first `date`). Phase A to ~17:55, builds ~18:50 / ~19:45 / ~20:35, freeze 20:35-21:05, the
  close table no earlier than 21:05:42, hard cap 21:35:42. Base u3-n54 = HEAD 40548069 (equal: nothing newer).
- **D-2 The researchers start with the open items the lead already knows** (NIGHT54's not-done list, THREADS T-127..T-133, GG's
  standing asks) instead of waiting for the auditor and the critics: A1-A3 run at the same time, and the researchers fold in the critics'
  top items if those files exist when they finish. Rejected: A3 after A1 + A2 (it would push the slate past 0:50).
- **D-3 The A0 smoke runs with a gamepad attached** ("HID-compliant game controller" present at 17:07). Unplugging or disabling the
  device is a change to GG's machine: not done. A pad-step failure with the pad present is read as T-73 / T-114, not as the build.
- **D-4 Rooms are edited in place** (Design/levels/n52/*.plan.json), as NIGHT53 and EXT53 did, not behind a design.n55 row: level
  files have no row switch, and the comparison side is `Builds/n54-freeze2` (tag u3-n54). `--off55` therefore gives NIGHT54's game
  except for builder R's room cuts; the report says so.
- **D-5 Five builders by area, each owning its rows and files** (L-049): B bosses (grade55, wick55, bossText55, brief55), C camera
  (fov55, step55, ssao55, ride55), K kills (kill55, finish55, speed55), R rooms (the mirrors in place, decor55, clear55), S smoke and
  reel harness (no row). Rows from Logs/n55/rows-def.json, placed off by Tools/night/n55-shared.cjs. Rejected: one builder per
  OPEN line (more builders than five, more overlap in SurvivalGame*.cs).
- **D-6 ride55 (the wall ride earns speed) is a sim row with Mover.cs named** by slate entry P1-ride, attempted only after C's P0
  entries. The critic (veteran #3) asked for it; GG's movement rule (PRINCIPLES 6) holds: bit-exact with the row off, THE STACK's
  climb walls keep NIGHT54's rule. It ships ON only if verbs37 x5 and THE WICK's climb numbers hold.
- **D-7 THE WICK's chip damage and windows (wick55) are encounter numbers, not attacks.** NIGHT54 D-6 pinned each attack's own
  numbers (GG: "keeping all the attacks exactly the same"); wick55 may change the window share, the chip rate and the hot wall's
  touch, never an attack's id, kind, timings, damage, radius or weight. Rejected: re-weighting slam / dive picks (an attack change).
- **D-8 ride55 is not built: P1-ride is SHORT by decision.** GG 2026-10-13 said of the ride "you gain speed too quickly"
  (docs/memory/OWNER.md:109; ride39 caps the chain at 16 m/s). The veteran critic's "the ride earns speed" (#3) reverses his call;
  it goes to the report for GG. Supersedes D-6. Rejected: a softer acceleration under the cap (15.2 -> 16 m/s would not read).
- **D-9 The pre-mortem's guards are adopted** (Logs/n55/premortem.md): the sharper checks go into each slate entry's notes and the
  critics check them; kill holds keep move54's 0.12 s cap; brief55 moves from B to K; Tools/night/n55-prune.sh (n54-prune.sh refuses
  n55 tags); the freeze smoke gets its own slot ~20:48-21:00 with no agent running, idle >= 60 s, and one n54-freeze2 control run on a
  failure; last builder commit 20:25, r3 picked by 20:35.
- **D-11 The night closed past the hard cap, without a freeze.** The usage limit stopped the lead and all five builders at 19:33
  (2 h 28 min); it reset at 22:00, past the 4 h 30 cap (21:35:42). GG's cap holds: no more building; the close was written at
  22:0x. Nothing ships ON, no tag u3-n55, the play build stays n54-freeze2. Measured-GREEN entries that never met a fresh critic
  close SHORT (NIGHT55.md section 2: only a critic's pass flips `passes`). Rejected: an hour of r2 past the cap to reach a freeze.
- **D-12 The freeze ran after the close (22:04-22:14), verification only.** Building stays stopped (D-11); a freeze builds what is on
  port/stage3 with every n55 row off and checks it (verbs37 x5, the smoke). Both passed, so by NIGHT55.md section 3 the play build is
  `Builds/n55-freeze` (= NIGHT54's game + iceline g7 / g5 re-cut + the smoke guard, which runs only under -skygrindSmoke) and the tag
  u3-n55 marks it. Rejected: leaving n54-freeze2 as the play build (the rule says the freeze build ships when its checks pass).
- **D-10 NIGHT55.md's "G9 extras still OFF" is wrong** (the auditor, OPEN O-13): wickRideParry47, wickIntro47 and wickFinale47 have
  been ON since NIGHT49 (5f212064). That line is closed with no work.

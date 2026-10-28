# Wave 60 (NIGHT60, the OVERNIGHT RUN; 2026-10-21 23:05:36 - 2026-10-22 ~07:50 MST; tag u3-n60)

**Ask (GG 2026-10-21 ~22:3x, verbatim in NIGHT60.md):** "dont follow this, take this but make it reated to this project, plan next night
prompt using this as a base, output should be next copty pastable goal pormpt" [reading: related, copy-pastable, prompt]. The base was the
"Overnight Run" template GG pasted with it; NIGHT60.md maps it onto this project (DECIDE60 D-1..D-9).

**Prompt shape:** NIGHT60.md (175 lines, untracked) + NIGHT60-BACKLOG.md (B1-B11 seeded, MAIN / GUARDS fixed before any number; B12-B29 added
in the run); an ~8 h unattended run, hard cap 8 h 30 (D-7), t0 the run's first `date` (D-10); the lead + at most two builders (L, V) + one
slim critic per row as its Player evidence lands (D-4, L-109); the check command n60-gates.sh (D-5: config, golden, fingerprints plain /
--off57..--off60, the Editor; `--flow` the five levels against Logs/n60/flow-base.txt). Forge: the red team's 10 findings applied (D-9).

**Shipped (ON by NIGHT57.md section 0's rule, each with a one-row critic PASS):**
- kill55 (B2; the cause: the enemy x-ray, queue 3005, drew over the marks, 2010; cd05c630 / 3524e84e / 284a4783; critic gate-b2.md; ON
  01:03-01:18 with n60-freeze-rows.cjs; flow --off60 EQUAL, Logs/n60/gates-kill55-flow.txt).
- Level 3 data (kiln57 already ON): B13 kb2c's respawn faces the fight (33ef339f, gate-b13.md), B14 kb3c's tap-hood pocket opened + the tap
  run's south end (612530f0 / bdb2ec0b, gate-b14.md), B15 the blow pipe off d6b's exit lines (8419921a, gate-b15.md).
- rim56 (B7 attempt 3: the string course takes the accent dim, the lit gain 2.205 -> 1.1; critic PASS gate-b7.md; ON ba80cfee 06:13, flow
  EQUAL); the review's computed terminator collapse measured instead: body V spread 0.50 -> 0.52 (D-14). focus60 (B9 / B18: a Player now
  pauses on a focus loss; critic PASS gate-b18.md; ON f923bf57 06:39, flow EQUAL; the smoke with two steals DONE).
- Tools: n60-gates / n60-reels / n60-prune (a1e6216c); a157 `-skygrindSpawnShots60` (B1) and `-skygrindClearShots60` (B8);
  n46-flow-geo.cjs's w1 in view off the spawn's yaw (B12); n60-v-yellow.py's fold bound (B16); the spitter / swarmer probe reporters and
  ProbeMaxEnemies 8 -> 32 (B3); n60-build-num.cjs (B3); n60-v-lamp.py (B8); FocusProbe60 + n60-focus-probe.sh + n60-focus-player.ps1 (B9).
- FREEZE: Builds/n60-freeze (ed5624dd): gates GREEN 07:07:42, verbs37 27 / 29 / 31 / 31 / 31, the smoke DONE 227.9 s (two steals took);
  Game suite 1006 / 1006 but M: MapChoiceTests TIMEOUT 606 s (T-147), 9 M classes unrun.

**Numbers (before -> after, source):**
- kill55 marks at the flash end 0.46 % (NIGHT59) -> 15.29 % ON vs 0.42 % OFF / 0.44 % old draw (a258 n60-b2; NIGHT60-LOG B2); the mark's
  centre V 0.76 -> 0.03; verbs37 x5 with kill55 ON 27 / 29 / 31 / 31 / 31.
- B13: wave 1 seen from kb2c's spawn 0 -> 5 of 5 (a157 n60-b5), n46-flow-geo w1 in view 1 / 4 -> 4 / 4, spawn31 PASS (Logs/n60/L/b13/).
- B14: kb3c NO PATH lines 1012 / 1591 / 2171 -> 0 / 0 / 0, put at the next room 8 -> 1 over seeds 1-30, kb3c deaths 8 -> 0 (Logs/n60/L/b14/).
- B15: kb3c put-ats 0 / 1 / 1 -> 0 / 0 / 0 (Logs/n60/L/b15/); kb3c contract 14/14 after each of B13-B15.
- B7 rim56: bodies over the world's p90 75.7 % off -> 90.1 % on (Logs/n60/b7a3-rimpath.txt; want >= 90); world over the tells 3 / 40 =.
- B9: the Player never paused on a focus loss (Cursor.lockState stays Locked; Logs/n60/b9/focus-n60-b7-green.txt) -> focus60 ON DONE True,
  -skygrindNoFocus60 False, planted False (Logs/n60/b9/focus-n60-b8-*.txt).
- B4 level 4 THE GAP NEST: contract 11/14 -> 12/14 (the slot split; Logs/n60/L/contract/n58-span-p2/table.md).
- B10 iceline idle > 20 %: 18.4 / 20.2 % -> 15.6 / 18.4 % with idle60 (branch n60-b10 only, D-13; Logs/n60/L/b10/).

**SHORT / blocked (measured reasons):**
- B3 yellow56 (blocked, 3 attempts): sKeep 0.85 / 0.75 / 0.65: MAIN 0 / 0 / 7 of 12, halo kept 59.6 / 64.7 / 82.2 % (guard 80): the tell and
  spire's world yellow share hue / S / V; next: a tells mask (NIGHT60-PROPOSALS.md P1, L).
- B4 / B5 level 4: the contract's ride wall (the card) and the 10-16 m height under drones over a 10 m deck: GG's calls; span58 OFF (D-8).
- B8 lamp60 (OFF after 2 attempts, ~3 h): the lamp's own gain 1.02-1.14x; MAIN 4 / 5 rooms ON with the off side already 2 / 5
  (Logs/n60/b8a2-lamp.txt); next: a brighter / larger lamp or a relative MAIN.
- B10 idle60 (branch only): MAIN missed, deaths guard set A 6 -> 9; the wardens / chargers hold the heavy token still.
- B24 the PULSE pips' pink (hue 332, 2 deg outside the RIVAL arc): GG's call. B17: cause noted, no change (a near body's honest AABB).
- B21: T-91's 240 s drone cap not reproduced on n52-kiln (4 x 40 seeds); the caps left are a burrower 5.6 m under kb2c's floor (L-b21.md).

**Estimate to actual (sizes S <= 30 / M <= 90 min; from NIGHT60-LOG's times):** B1 S 19 min, B6 S 9, B12 S 14, B16 S 5, B21 S 12 (S at 0.2-0.6x);
B4 M 71 min (0.8x, SHORT), B14 M 64 min built / 106 to DONE (0.7 / 1.2x), B10 M 38 (0.4x, missed), B13 ~25 min built (DONE after the stop),
B3 M 110 (1.2x, blocked), B9 M ~75 min active over two attempts (0.8x), B8 M ~3 h over two attempts (2x, blocked), B7 M ~4 h 10 over three
attempts net of the stop (2.8x). Idle: the usage limit stopped every worker ~03:00-04:01 (61 min; L-003 / L-098: the slices were
committed, the builders resumed by SendMessage).

**What went wrong:** the usage-limit stop (above); L-113 (n60-prune.sh skipped hyphenated sides: C: 9.7 GB); L-115 (the focus probe's steal
timed off a Player log that lands 6-14 s late: three runs read 0 losses); a Player never paused on a focus loss (EditMode-only since w56).

**Closed threads:** T-138 (focus60 ON: a Player pauses on a focus loss), T-98 (a148 on the pad: "[RT] through the crack.", B23), T-91 (not reproduced, B21), T-67 (the n46flow pilot walks the gantry: flow-base n52-foundry GREEN 10/10), T-130 (a
pointer row: its parts live in T-104 / T-105 / T-120); merged for the 60-line cap: T-79 into T-132, T-112 into T-104, T-117 into T-93.

**Reaction:** (none yet)

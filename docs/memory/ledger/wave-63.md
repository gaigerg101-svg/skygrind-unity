# Wave 63 (NIGHT63, THE BOSS NIGHT; 2026-10-22/23, t0 22:36:01, an 8-hour box, 7 minimum: FREEZE 05:36:01, CAP 06:36:01; tag u3-n63 on the close commit, the close run at CAP after the usage limit's stop)

**Ask (GG 2026-10-22 ~22:11, FORGE63, during NIGHT62's close):** "plan out next long detail night prompt 8 hours 7 minimum, follow detailed
methods we used and pull form online to get best cresuolts, be honets and critcal on where we need to improve, i would lead towards bosses,
effect colores everything doenst feel alligned, boss animations are too rigid to start, this is one of many many things the night prompt
hsould focus on, create it now, 30 mins to build" (NIGHT63.md lines 3-5). Led with the bosses (motion, tells, colours), one colour table,
and NIGHT62's debt (P1 exitFrame61, P2 bossRule61 on time).

**Prompt shape:** NIGHT63.md (189 lines, untracked) + the /goal (Research/2026-10-22-forge63/GOAL.txt); BUILD 0:00-7:00, the close 7:00-8:00
(the guard's clock rule); slate 8 seeded, 14 by the end (P3, C2, S7, T63, EYE63, EXIT63 from the leftovers and DECIDE calls); builders D,
V, G, at most five agents (L-128); 13 builds (b1 at 0:34, b13 at 5:14). Forge experiment (FORGE63 D-6, "visible by design": a named SPOT
pose with >= 41,472 px on its same-run pair, for B1-B3 and C1): B1 met it at last (wick stagger 52,428 on b10), B2 787,061; B3 and C1
went SHORT; B1's first SPOTs named the glow and the floor, not the pose (L-139). Leftovers outside the bar: C2 590,004, T63 11,949 px.

**Shipped (ON, each by a GATE PASS):** bossRule61 (P2, 65bc7cce), text61 (P3, f2b0c261), nearFade63 (C2, 02c70321, round 2), text63 (T63,
a53bcae4, round 2), bossMotion63 (B1, 393bdc8d, round 2), bossTell63 (B2, d391c6ef). The slate: 6 PASS, 7 SHORT (D1, P1, C1, M1, S7, B3,
EXIT63), 1 BLOCKED (EYE63) (Logs/n63/slate.json).

**Numbers (source):**
- P2: boss61 in from 0.47 s on all five, alpha 1.00 at 1.0 s (n63-b1 summary47.txt; NIGHT62: in at 2.45 s).
- P3: two or more centre blocks on kiln entries 0 / 5 ON vs 4 / 5 OFF (n63-b3, verdict-P3.md).
- B1 (verdict-B1.md, b10): rms 0 -> > 0 in 23 / 23 s on all five; flinches drawn 49 / 49 on a plain crown run; wick stagger 52,428 px;
  SPOT "tipped over sideways and slumped low" (spot/B1.txt).
- B2 (verdict-B2.md, b10): spit lead 225 ms "short" -> 425-492 "ok"; crownFire hue 41 "off-hue" -> hue 10 "ok"; every other attack's
  lines identical before and after.
- T63 (verdict-T63.md, b10): "(LATE" 4 -> 0 in verbs37 x5; kiln's three restatements withheld, the step lines and spire's TORCH line shown.
- C2 (verdict-C2.md): 590,004 px at 0.5 m (b7); THE ICEBREAKER at 1.7 m 936,071 (b6).
- eye63 64 of 64 events halved, view only 1.00 mm (b8); exit63 own gain 0 / 5 (b8); C1 2 / 5 levels; S7b 468,517 px.
- verbs37 x5 27 / 29 / 31 / 31 / 31, RED none, on b1-b6, b8, b10-b13; NRE 0 in every reel b2-b13; gates GREEN at every pick (flow EQUAL
  after each flip). The fresh review: must-fix 0, should-fix 8, all fixed or probed (Logs/n63/review-n63.md). The usage limit stopped
  the session 05:23-07:41 (L-003): the close ran at CAP; after it, n63-freeze (from the tag) verbs37 x5 as above, the smoke "DONE: all
  steps passed", flow "gates: all GREEN 08:17:31" (D-16); gg-proxy after (36 frames): no level frame better, 11 / 18 bosses unreadable.

**SHORT (reasons):** D1 (planted no-op 0-16 px, plant 100-111, not exact); P1 (own gain 1 / 5 after a brighter round, reverted, D-6); M1
(two GATE FAILs on inferred causes); C1 (2 / 5; iceline / spire lit surfaces, D-9); S7 (the blind spotter saw "columns" twice); B3 (round
2: the wyrm's OPEN head spring green, hue 158.5; THE ICEBREAKER 0 px); EXIT63 (own gain 0 / 5). EYE63 BLOCKED: a camera ease has no still
frame to SPOT; GG's blind A / B (D-10).

**Estimate to actual:** b1 at 0:34 (planned by 2:00), b5 2:17, b9 3:40, b12 ~5:02; GATEs 2-5 min; four rows needed a round 2 (B1, B3, C2,
T63); three PASS there.

**What went wrong:**
- The reel prune skipped capitalised side names (Logs/feel/n63-b4 11 GB; L-140).
- B2's meter misjudged three times; the cue it named was the strongest riser, not the cue (L-136).
- B1's pose hid under bigger glows (L-139).
- T63 lost two lessons silently, and its doneWhen named a list the run's order decided; S7's doneWhen kept a parts list through a redesign
  (L-144).
- The lead called B3 SHORT on the builder's meter before its round 2 (L-145).
- Another session's pytest took commit free to 1.0 GB (03:07-03:09; L-146).

**Agents:** builders D, V, G; gg-proxy start; GATEs (skygrind-critic): M1 x2, P2, P3, D1, C2 x2, S7, T63 x2, B1 x2, B2, B3 x2; the fresh
review (skygrind-reviewer); a skygrind-spotter per SPOT pair.

**Lessons added or confirmed:** added L-134..L-146; confirmed L-001, L-006, L-049, L-068, L-091, L-123, L-128, L-129, L-130, L-132.

**Closed threads:** T-146 (the w60 review's four should-fix items built: w61 B1; w63 7214fdd9, 01c3e6d8, 1b76fb8e; review S6 54569d37).
T-74 merged into T-60, T-142 into T-153. T-148 moved: bossRule61 and text61 ON.

**Reaction:** no play recorded (GG's last play 2026-10-20, OWNER.md).

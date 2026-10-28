# Wave 65 (NIGHT65, THE FLIGHT FOUNDRY; 2026-10-23, t0 13:58:02, 7 hours: BUILD 0:00-6:15, FREEZE 6:15, CAP 7:00; tag u3-n65)

**Ask (GG 2026-10-23 ~13:08, verbatim, NIGHT65.md line 3):** "ok we need to plan detaile dbuild prompt for next 7 hours 6.5 min 7 hard stop.
follow same methods on finding what to focus on, dont run into corners or repeat past mistakes, i wnat real progress and itterations and
not just small polishes". The forge FORGE65 chose the rooms (gg-proxy's #1 three nights): level 1's six fight rooms rebuilt from scratch.

**Prompt shape:** NIGHT65.md (206 lines, untracked) + GOAL.txt (Research/2026-10-23-forge65/); slate 10 (T1, T2, a2, a3, a5, b1, a8, a4, LV,
PC); builders T (tools), R (a2, a3, a5), S (b1, a8, a4) + one review slot (four agents at most); cards before geometry, a card gate, three
reviewed rounds, a GATE per builder's set; the forge experiment D-9 (building share >= 70 %).

**Branch C all night:** C: 13.2 GB at t0 (the plan needed 16-20 GB for a Player), 8.7 GB at 3:54 (about 1.5 GB of the fall outside the
night's files); the guard refused builds, reels and smokes under 12 GB and refused removing even this night's worktrees ("GG's call").
No Player ran (DECIDE65 D-1, D-2, D-6).

**Shipped:** no game change: design.n65.foundry65 (a level row) stays OFF; Design/levels/n65/slots65.json all old. T1 PASS (the assembler and
the slot tool); T2 SHORT (tour65: Player lines not checked). The rooms reach GG through Builds/n65-play-latest (D-7, unreviewed).

**Numbers (source):**
- The card gate (Logs/n65/cardgate*.md): 6 / 6 FAIL the first pass; a2, b1 PASS the second, a5 the third; a3, a8, a4 into round 1 by D-4.
- The red (Logs/n65/red/summary.md): the walk bot's DULL 54-88 % in five rooms; the skip test RED 6 / 6 (T's verb pilots).
- Round 3 (Logs/n65/r3b): verb pilots GREEN 6 / 6; skip --verbs65 GREEN a5, b1, a4; room checks PASS 5 / 6 (the red 1 / 6); air hand-offs
  a2 1 (red 0), a3 2 (3), a4 1 (0), a5 2 (4), b1 3 (3), a8 2 (2).
- The GATE (Logs/n65/verdict-<id>.md): 0 PASS, 6 FAIL, SIM-SIDE FAIL: the cards' MAIN aims on the walk bot's DULL unmet (a2 48.7 / 50.3
  vs < 18.6 / 21.1; b1 48.5 / 57.3 vs <= 40; a4 87.1 / 84.0 vs <= 50); guards (a2 stall, a4 deaths, b1 stall); a8 skip RED; a5 SPOT "mostly
  empty". Both critics: a NOT CHECKED Player line caps a room at UNSURE.
- The fix round (D-8): a4 deaths 1 / 1 -> 0 / 0; b1 launch from the rail GREEN, skip GREEN 22.7 vs 37.0 s; the skip fix (T): a4's GREEN was
  not real (its refusal never fired) -> RED; a5 and b1 GREEN with 4 / 5 seeds exercised.
- The density round (D-9, 4:30-4:40) and r5 (Logs/n65/r5): every guard held; nothing in the skip results moved. gg-proxy on the final rooms
  (Logs/n65/gg-proxy-final.md): would GG keep it over the old room: b1 YES; a3, a5 UNSURE; a2, a8, a4 NO; his first words predicted: "The
  rooms still don't incentivize wall-riding or creative use of the weapons" (2026-10-09). a5's pour never took a body (the wave-2 pilot).
- D-9 (Logs/n65/d9-final.txt): 188 added, 129 moved, 106 dropped over 49 commits: 75 % added or moved (the hypothesis >= 70 %: MET).
- The integrated-diff review (Logs/n65/review-n65.md): 0 must-fix; row off = NIGHT64's game; u3-n64's code boots n65-foundry (read).

**SHORT:** all six rooms (no GATE PASS: the card MAIN instrument, guards, skips; no Player); LV (the row: no slot new, no Player); T2.

**What went wrong:** the disk (not freed before t0; the night could not see the game); the cards' MAIN on the walk bot's STYLE (it never
uses a verb); three card passes cost geometry 30 minutes; the round's skip ran the plain pilot once (r3); the round script hung on `| head`;
five agents alive for a minute (stopped); my log ran ahead of the clock twice (corrected).

**What went right:** T's eight tools (assembler, slot tool, bot opt-out, iso65, tour65, verb pilots + verb-aware autopilot, air65, the GATE
pack) all landed early with proofs; the reviewers' findings were precise and the builders answered them by name; the verbs went from 1 / 6
to 6 / 6 working in the rooms' own geometry; every critic re-ran numbers itself; nothing touched Core; the gates GREEN at every pick.

**Lessons added or confirmed:** new L-150 (a card's MAIN needs an instrument that performs the verb); L-089 extended and confirmed (a
sim A / B's refused run must print that it refused something); L-104 confirmed (the disk before t0) with L-113 merged into it; L-149
confirmed (LESSONS.md, 150 lines).

**Threads:** T-127 (the rooms) extended with level 1's rebuild and its next approach (GATE on a Player build); T-149 (the disk) raised.

**Reaction:** (none yet; GG's play of Builds/n65-play-latest goes here and in OWNER.md)

# Wave 52 (NIGHT52, THE ARENA NIGHT): the combat rooms rebuilt to a numbered contract (2026-10-19 22:29 - 2026-10-20 ~06:1x MST, port/stage3; tag u3-n52)

Sources: NIGHT52-LOG.md (close table at the top), NIGHT52-REPORT.md, DECIDE52.md D-1..D-31, Tools/night/n52-rows.json, docs/ROOMS.md.

**Ask (GG, 2026-10-19 ~22:00, NIGHT52.md):** plan an 8 h night, "be honest and critical on where we still need to improve and polish",
compare open-source projects, roleplay; all related to building and iterating. GG did not answer during the run (every call in DECIDE52).

**Prompt shape:** lead + 3 builders (K kiln + span, F foundry + spire, U polish: hud52 / skin52 / guns52 / look52); 8 h, hard cap 8 h,
minimum 7 h; P0 baselines + a shared commit (rows design.n52.*, placeholders n52-*); THE ROOM CONTRACT (NIGHT52.md section 2: 20-32 m,
>= 2.5 pieces / 100 m2, 3 planes walkers reach, a 17 m ride, a pad, health, spawn rules, dwell no worse, the hold probe, 10/10 clears);
two persona reviewers per level (an ULTRAKILL P-rank runner, "GG" from OWNER.md); the room loop from 4:30; quiet freeze; STOP POINT every
30 min. Building share 66 % (target >= 70 %; Tools/night/n52-share.py).

**Shipped (all ON at the freeze, `-skygrindOff52` = NIGHT51's game; Builds/n52-freeze at bee5387f):**
- 18 rooms pass the contract (Logs/n52/contract-table-freeze.md): kiln kb2 0.44 -> 2.53, kb2c 0.60 -> 2.50, kb3 0.33 -> 3.08; foundry a3
  0.48 -> 2.69, a5 1.04 -> 2.81, b1 0.42 -> 2.97; iceline g2 1.19 -> 2.53, g3 1.10 -> 2.53, g4 0.83 -> 2.64 (types exempt), g5 0.77 -> 2.54,
  g7 0.52 -> 2.73, g8 1.81 -> 2.58; spire v2 1.67 -> 2.73, v5 0.95 -> 2.56, v6 1.02 -> 2.60; span p1 1.39 -> 2.60, p3 1.56 -> 2.64, p5 1.70
  -> 2.61. Jump pads 0 -> 37 (kiln 5, foundry 7, iceline 15, spire 6, span 4).
- hud52 (ink-panel HUD, the lesson chip, the top plates, teach lines that wait for their verb), skin52 (the enemy surface), guns52 (machined
  KESTREL / THUNDERCLAP), look52 (the bone health plus, the thin cyan ring, foundry's ledge faces).
- Tools: n52-contract-all.sh (the whole contract per room, one table), hold52 (the hold / camping probe), n52-room-check.py, n52-room-flow.sh
  (isolated per-room flows), n52-flow-table.sh, n52-freeze.cjs, n52-sheet.py (before / after sheet).

**Numbers on the freeze build:** verbs37 27 / 21 / 30 / 31 / 31 (foundry / iceline / kiln / span / spire; NIGHT51 26 / 20 / 25 / 30 / 30),
RED none, 33 Player reels NRE 0; a452 GREEN x5; golden 22 / 22 exact; fingerprints plain / --off50 / --off52 EQUAL; flow table 20/20 walks
x5 with kiln better on every column and four crosses (foundry dwell 12.1 -> 16.8 s, iceline clear 220 -> 246 s, spire / span stall +3-4
points: T-124); the full batch suite at 907dc105: 2238 / 2238 passed (Logs/n52-close-tests.xml; the first run at bee5387f was 2237 / 2238: a test leak, T-126, pinned test-only); the smoke on n52-freeze: DONE, all steps passed (227.7 s, 0 NRE) with the Bluetooth "Pro Controller" present.

**SHORT (with reasons):** U row: the scorch core (fallback NIGHT51's mark, T-120), iceline's grade 3/4 (gradeLevels [], T-116; span met 4/4 with no lift),
spire's tiers RED (foundry only, T-121), the kill wash's f3-f4 peak (T-111), guns52's muzzle-light timing on a long first-shot frame
(T-122). Not rebuilt: foundry a8 (its walk is scripted, D-26), iceline g1 / g6, foundry a2 / a4, the links.

**What the run taught (LESSONS L-086..L-090):** the contract computed per room made "done" checkable (18 rooms vs 0 the night before), but
the persona reviews of the late rooms still found first-minute bugs the numbers missed (pads on walk lines, a pad in front of a respawn,
a ride into a dead end) and design sameness ("none would satisfy GG as built"); TaskStop orphans; a proof reel must show its input fired.

**Reaction:** <none yet: GG has not played n52-freeze>.

## Closed threads (moved from THREADS.md, consolidate 2026-10-20)
- T-119 (w52) KILN kb3 THE MOULD LINE: CLOSED at NIGHT52 03:57 (b919501c): the door dwell was the walk pilot backing away from the pack along
  the door's axis from a spawn 7 m out (hold52 --dwelltrace); the retry spawn moved off the axis to (-5.5, 0, -202): kb3 14/14, 0.33 -> 3.08.
- T-106 (w49) THE FLOW OF n48-foundry: met at NIGHT52 P0: n52-flow-table.sh takes level ids as arguments; n48-foundry walks 20/20, stall
  29.2 %, dwell 12.1 s, dead 30.2 s, clear 142.5 s (NIGHT52-LOG 22:35-22:37; Logs/n52/flow-p0/table.md).
- T-109 (w49) THE FULL SUITE after the two test fixes (4c5b99bd): met: the full batch suite at 907dc105 (4c5b99bd is its ancestor, git
  merge-base) 2238 / 2238 (Logs/n52-close-tests.xml).
- T-101 (w48) G44, the next night's length: GG's call, made: NIGHT52 "plan an 8 h night" (this page's Ask); NIGHT53 3.5 h and EXT53 2 h
  (ledger/wave-53.md).

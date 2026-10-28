# Wave 49 (NIGHT49): one look for the whole game (2026-10-19, port/stage3; CLOSED 13:56 MST, t0 11:44; tag u3-n49)

Written by the forge and the NIGHT49 lead. The close entry replaces the status lines below at the NIGHT49 close (NIGHT49-LOG.md's
close table, NIGHT49-REPORT.md). Planning set: Research/2026-10-19-wave49/ (commit e66b056a; FORGE-NOTES.md has the stages and the
reviewer's 15 fixes).

**Ask (GG, 2026-10-19 ~11:20, verbatim in NIGHT49.md):** the charge bar under the reticle comes back ("I don't know why it's gone");
"the colors and stuff are way too bland"; "the effects are inconsistent it all needs to be unified"; "very inspired by Ultra Kill and
Doom".

**Forge experiment (one change):** every look row's acceptance is a frame-meter number with a measured baseline AND a 3-judge blind
pair, set before the run, instead of "the lead looks at the frames" alone.
- Hypothesis: fewer look rows close on a builder's say-so or on one viewer's taste; fewer looks reversed by GG later.
- Measure (NIGHT49-REPORT): the share of the look rows (G1-G6) closed with both a meter number and a pair result (target 100 %);
  later, how many of those GG reverses after playing.
- In-run state at 12:41: G1 (charge) has both (a149 meter GREEN; blind pairs ran and arc40 won 3-0 twice, so the look is not kept).
  G2-G5 have meters (a349, a449, a249) but their pairs have not run yet (they run after round 2). G6 has no row yet.
- The method held as a rule: the first pair set was staged from a wrong crop (black frames; the judges' TIE votes were discarded)
  and the first key was inverted (fixed; sha-checked). A pair needs its two images checked for content before the judges see them.

**Close (13:56):** the forge experiment scored: 5 of 6 look rows (G1-G5) closed with both a meter and a pair; G6 was not built (target
100 %, missed by one row). Shipped: lights49 ON (G2, GREEN on the tie) plus the P0 rows (six NIGHT47 / NIGHT48 rows). SHORT: G1 charge
(arc40 won three pairs), G3 / G4 grade (look49 OFF: won or tied 2 of 5), G5 kit (fx49 OFF: the kill's wash). Flow table walks 20/20.
Source: NIGHT49-LOG.md's close table, NIGHT49-REPORT.md, DECIDE49.md D-19 and D-20.

## Closed threads (moved from THREADS.md, consolidate 2026-10-20)
- T-89 (w48) THE FREEZE NOT RUN: met at the NIGHT49 freeze: verbs37 26 / 20 / 25 / 30 / 30 on the freeze build (RED rows [], NRE 0), the
  smoke DONE (237.5 s, NRE 0), the suite on a9b1442d 2206 / 2208 (both failures fixed in 4c5b99bd), the n48 rows climbTop48, foes48,
  loomHull48 ON (NIGHT49-LOG close table). Its n48 flow part went to T-106 and the suite re-run to T-109 (both closed in ledger/wave-52.md).
- T-88 (w47) RowsBaseline at 46 and the n47 flow table: met: the baseline flipped to OnPathThrough 48 (c16a86c9, fingerprints EQUAL plain /
  --off47 / --off48 / --off49); n47-flow-table.sh at 4c5b99bd walks 20/20 on all five levels (NIGHT49-LOG close table).
- T-110 (w49) THE BLIND-PAIR PROTOCOL: a rule, not open work: a pair's images are checked for content (not black) and the key by sha before
  judges see them; two pair sets were discarded this night (black crops; an inverted key line, fixed). This page's forge experiment holds it.

# NIGHT50 raw judge votes (the lead's record; each vote as the judge gave it, in the order the judge saw the pair)

Each row: pair id; the order-o1 votes (judges J1, J2, J3); the order-o2 votes (J1, J2, J3). The key for each pair is in
Logs/n50/judge-keys/<pair>.json (A and B mean the o1 or o2 layout; the key maps them to old and new). Reproduce a verdict with:
`python -I Tools/night/n50-pair.py tally <pair> <o1 J1> <o1 J2> <o1 J3> <o2 J1> <o2 J2> <o2 J3>`.
Votes are from fresh judge subagents, one call per (judge, order). Rubric: Tools/night/n50-rubric.md.

| pair | o1 (J1 J2 J3) | o2 (J1 J2 J3) | tally result |
| --- | --- | --- | --- |
| cal-1 (same frame) | TIE TIE TIE | TIE TIE TIE | TIE |
| cal-2 (6 px shift) | TIE TIE TIE | B TIE TIE | TIE |
| cal-3 (sharp vs blur) | A A A | B B B | old (the sharp frame) 3-0 |
| rej-1 (H: arc40 vs c3) | A A A | A B A | TIE |
| rej-2 (L: lights, kiln) | TIE TIE TIE | TIE TIE TIE | TIE |
| rej-3 (V: grade, kiln) | B B B | B A A | new 2-0 |
| rej-4 (X: kit on vs off) | B B B | A A A | new 3-0 |
| rej-5 (V: grade, spire) | A A A | A A A | TIE |
| rej-6 (V: grade, foundry) | B B B | B B B | TIE |
| rej-7 (V: grade, span) | A A A | A A A | TIE |
| rej-8 (V: grade, iceline) | B A B | A A A | new 2-0 (J2 TIE) |
| g1-charge (re-placed bar vs NIGHT46) | B B B | A A A | new 3-0 |

Notes:
- rej-8's order-o1 votes are B, A, B. An earlier tally was run with B, B, A (a transcription error in the lead's first tally); the result
  (new 2-0) is the same, and this table is the record.
- The verdict per judge is the pick only when both orders name the same frame; otherwise TIE (n50-pair.py).
- The rule of the result: a side wins with at least two judges and more judges for it than against it (n50-pair.py tally).

# I4 fix round, (a) (b) (d) (builder T, NIGHT68; Logs/n68/verdict-I4.md P1, P2, P4, P5)

All of these change how a verdict is read; no measurement changes. Every run line and ratio is computed as before.

- **(a) P1:** skip v2's exit code follows its M2 VERB line (seeds 1+), or its M2 FRESH line (seeds 21+); it no longer follows M1. M1 is
  still printed.
  - `Tools/night/n65-verbs.sh` counts GREEN / UNSURE by M2 VERB / M2 FRESH. It falls back to M1 only when the output has neither, as an
    old skip's would.
  - The GATE pack takes a pre-run's code from its M2 FRESH line. Its file header now says so.
- **P5:** on fresh seeds (`--seed 21` and up) the M2 VERB line is not printed. Only M2 FRESH is printed there, with the leak count shown
  but not judged: the ROOM list's fresh test is the median and the exercised count.
- **(b) P2, the leak:** an exercised seed whose refused-in or walker run still used the verb fails M2 VERB. The line names the seeds, and
  each seed's line ends `| LEAK: the refused run used the verb`. The check order is UNSURE (exercised), the bar, the refusal, the leak,
  STYLE, then the ride band.
- **(d) P4:** `n68-red-bars.cjs` puts a red with fewer than 70 % of its seeds exercised (14 / 20) at the floor bar 1.25 and does not use
  its ratio. The line says "bar at the floor 1.25, red exercised N/20", and `--write` sets `redNote` on the slate.
  - A pooled ratio of "-" (nothing exercised) is read, not MISSING.

Evidence (Logs/n68/I4fix/):
- `g7-seed8-leak.txt`: g7 melee only, seed 8. M1 PASS (ratio 1.60), but M2 VERB FAIL: "the refused run used the verb on 1 exercised seed(s)
  (8)". The seed line ends `LEAK`. Exit 1, where it was 0 under M1.
- `n65-verbs-count.txt`: the same case through n65-verbs.sh: "GREEN 0, RED 1". Under M1 it would have counted GREEN.
- `v2-fresh-21-22.txt`: seeds 21..22 print M1 and M2 FRESH only, with no M2 VERB line. Exit 3 follows M2 FRESH UNSURE.
- `gate-taken.txt` (+ folder): `--fresh Logs/n68/I4/fresh-v2.txt` taken. Its code is 3, from that file's M2 FRESH UNSURE line.
- `red-bars-under14.txt`, two planted copies of the dry red's skip, both with the pooled ratio set to 1.40:
  - Exercised 15 / 20: mainBar 1.6.
  - Exercised 9 / 20: mainBar 1.25, "(bar at the floor 1.25, red exercised 9/20: under 14 exercised, the ratio not used)".

Not checked: (c) (the fresh pre-run tied to its level file) is the lead's last item, not in this commit.

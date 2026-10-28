# DECIDE70 (NIGHT70, 2026-10-25; the lead's calls that are GG's to confirm)

Written at 00:03 (1:19 into the night). Each call names the measurement that raised it. None of them changes a rule that was already measured; each is a question for GG or for the forge.

## Q-1 Which run the launch target counts (D-4 / D-5)
- The card's launch target (kb3 >= 6, v2 >= 4 launches per verb-aware run) is read on the PLAIN verb-aware run (T's Z3 design, the slate's words: "the card's launch target stays the plain run's").
- Measured on the Z3 harness (NIGHT70-LOG.md, 00:01): kb3 d2z reads M2 VERB PASS 1.39 on the chosen runs (the committed run chosen on 5 of 20 seeds), but the plain run alone is 1.32 with 4 launches a run (the committed run: 22). It fails the plain launch target only.
- The question: should a candidate's launch count be the chosen run's (the verb side the M2 line uses), or stay the plain run's? The current rule keeps the plain run, so kb3 is SHORT tonight. A change would be a rule change made after the measurement: not made by the lead.

## Q-2 The Z3 bar's floor (the critic's P4)
- The bar is max(1.25 + (redZ3 - redPlain), redZ3 + 0.20), the formula in NIGHT70.md section 5. With a red that falls against the plain run, 1.25 + change can sit under 1.25 (the critic's example: red 1.00 against plain 1.10 gives 1.20). Not changed tonight (a bar is not loosened after a measurement). Written down for GG: keep the formula, or add a 1.25 floor.

## Q-3 The committed pilot's "launchable" definition (T's narrowing, not picked)
- T first narrowed "launchable" to hp at or under the burst's 60 damage (a body the launch kills). The slate's literal rule holds for chargers too; the literal is the default, and the narrowed rule is the option `--commit-burst-cap` (off by default). The literal runs on kb3 made 26-34 launches a run but ran 49-79 s against the plain 20-40 s, so the committed run was never chosen on kb3. The option's runs were the same on v2.
- Whether the committed pilot should hold fire on chargers is a question about the verb's design (GG's enemy and verb calls); not decided tonight.

## Q-4 The slide rooms' M3 (the critic's caveat on Z1)
- A launch+slide room's M3 now reads the plain-launch chart (n70-throws.sh never passes --slide). Not in this night's GATE rooms; the slide rooms' M3 is for the forge to re-check before any slide room is screened.

## Q-5 The night's room result
- If Q-1 is not changed, kb3 and v2 are SHORT at the close with their numbers (NIGHT70-LOG.md, 00:00-00:01). The night's aim "one room PASSes its GATE on a2" is not met by the current rule.

## Q-6 The fresh test's bar with the committed run (the integrated-diff review's major finding, Logs/n70/review-night.md:1)
- The GATE's fresh test (M2 FRESH, seeds 21-30) takes each seed's faster of the plain and committed runs (Harness.N65.Verb.cs:1308) and still reads against a flat 1.25 (:1398). The main bar gets the gain taken off (n70-red-bars.cjs); the fresh bar does not. So a fresh set reads easier than the main bar intends, and n70-next.cjs would mark `ok fresh` on it.
- Not changed tonight: no room reached a GATE, so no GATE result depends on it; and a bar is not changed after a measurement. Decision for GG / the forge: give the fresh test the same gain-adjusted bar as the main test, or keep it flat and say so in the GATE.

## Q-7 Minor print and tool findings (review, findings 2-8; none changes a verdict tonight)
- The bar formula can sit under 1.25 (Q-2); the red's harness is not checked against the round's (the t0-harness reds red-kiln/kb3d, red-kiln-n68/*, red-spire-n68/v2 carry no committed run); `n65-verbs.sh` looks for a chart beside the level file when run without `LEVEL_FILE` (a false "no throw chart"); a chart's level and sha are not checked by the M3 reader; the launchend chart's lava flag reads only while a body is alive (a lava death may print "in lava no"); a flight that ends in the same step as a charted one can print its end and distance (no "(shared step)" mark); `held70` is reset on no early return (no live path found; not traced).

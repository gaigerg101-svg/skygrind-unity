# Wave 49 forge notes (2026-10-19 ~11:20-11:45, the lead's session after NIGHT48's close)

## The stages
- S0: GG-REQUEST.md (verbatim; G1-G6; the charge bar logged as a reversal of 2026-10-18's G19).
- S1: LOOK-NOW.md + look-now.png (the lead's screen pass on n48-r5 Player frames, one per level).
- S2: IDEAS-POOL.md (24 cards through the lenses).
- S3: ULTRAKILL.md, DOOM.md, UNIFY.md (three web-only agents, every claim Verified or Reported with its URL) + CODE-FACTS.md (a
  read-only code map, file:line).
- S4: SHORTLIST.md (8 picks scored; 6 in, the style meter to leftovers, dither cut: a full-screen shader, DECIDE47 D-13).
- S5: SPIKE.md (the look meter: the premise that "bland" can be measured holds; the baseline per level).
- S6: NIGHT49.md (untracked, at the repo root, like every night prompt).
- S7: a fresh reviewer with the files only; its findings and the fixes below.

## The forge experiment (one change)
- **Change:** every look task's acceptance is a frame-meter number with a measured baseline (from the spike) AND a 3-judge blind pair,
  set before the run, instead of "the lead looks at the frames" alone.
- **Hypothesis:** fewer look rows close on a builder's say-so or on one viewer's taste; fewer looks reversed by GG later.
- **Measure (NIGHT49-REPORT):** the share of the look rows (G1-G6) closed with both a meter number and a pair result (target 100 %);
  later, how many of those GG reverses after playing.

## The reviewer's findings and the fixes (S7: 15 findings, all fixed in NIGHT49.md; 200 lines, /goal 1,085 characters)
1. Integration windows of 20 min could not hold picks + build + ~18 serial Players -> 40 min each, one reel script per round.
2. The freeze (build + verbs37 + smoke + suite + flow) needs 26-34 min and the batch copy is shared -> a 30-min quiet freeze at 3:10.
3. ~45 judges, not ~30 -> one blind sheet per area (H, V, L, X: 12 judges) + at most 6 re-pairs.
4. The smoke, the suite and the flow table beside four busy builders (memory: focus; L-005) -> run once, builders stopped.
5. P0 waited on builder F's drone split -> the lead's one-line gate in the shared commit.
6. Wrong fact: LightKit29 maps white -> the boundary colour (1219 fixtures), green / amber their own; fixtures carry no "usable" mark
   -> the fact corrected; L's first task defines the test (within 1.5 m of a pad, grapple node or ride surface in the level JSON).
7. F's enemy rim already exists (Enemy.shader `_RimColor`) and a948 has no wall or boss -> F dropped; builder L takes the level lights.
8. H's BEND lives in UiRail34 (not UiHud38); the STITCHER heat was in arc40 -> H owns UiRail34; heat is one of the sources.
9. Four colour tables contradicted "unified" -> one palette: Look49's role tokens seeded by the lead at G0; the rest hold sizes.
10. Level hues collided with role hues (amber / magma = danger, gold = parry) -> accents >= 20 deg from every role hue, below the roles'
    saturation.
11. "Centre box" meant two things -> the RETICLE box (18 px) and the WASH box (the middle 20 %, 384 x 216 px) defined in pixels.
12. The freeze mechanics were ambiguous -> OnPathThrough 47 -> 48 in its own commit; n48-rows-on.cjs handles design.n47 + design.n48 with
    --dry; the bar for view rows (frames / pair) vs sim and numbers rows (the meter on n49-r0 + verbs37); stampede47 needs its frame first.
13. The brief copy "n48 -> n49" was incomplete -> every substitution listed.
14. The /goal could become unmeetable (OFF rows are neither GREEN nor SHORT) -> "GREEN, SHORT, or OFF with its reason"; the close starts
    by 3:40; the close table so far at every STOP POINT.
15. The weakest feature was foes49 (GG never asked; rule 7: GG owns enemy design; the drone lost three pairs) -> cut; style49 also cut
    (style42 is already ON as a rank-letter style meter, unity-config.json:6404-6407).

## Forge follow-up (2026-10-19 12:41, after the reviewer's pass)
- The 15 fixes were checked against NIGHT49.md, not taken on trust. Three were still wrong in the prompt and are corrected there:
  the drone gate's name (`foes48drone` -> `drone49`, DECIDE49 D-2: NightRows.ViewOn reads the night from the row's name), the
  rows-on script's name (`n48-rows-on.cjs` -> `n49-rows-on.cjs`, the generic one that serves P0 and the quiet freeze), and the G0 row
  that named the old gate. Nothing else in the prompt changed; the /goal is 1,084 characters.
- The forge experiment in flight (in-run, not yet scored): G1 closed with both a meter number and a pair result, and the pair
  REJECTED both candidates (arc40 won 3-0 twice); G2-G5 have their meters and their pairs are still to run. The measure is taken at the
  close (NIGHT49-REPORT), and the status is in docs/memory/ledger/wave-49.md.
- Lesson for the next forge: a blind pair's inputs get checked for content before the judges see them (two of the six judges' TIEs
  were black frames; my crop window missed the 240x240 frames H had already cut, and the key line was inverted).

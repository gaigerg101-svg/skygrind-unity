# Wave 20: GG's playtest, then a build night (2026-10-06 22:06 to 2026-10-07 ~06:30, branch port/stage3, box 8 to 8.5 h, tag u3-n20)

**Ask:** GG played NIGHT19's build and asked for six fixes (the shockwave on the shotgun with a 15 s+ cooldown, zoom on
most guns, RB scrolls weapons, regen only at the round's end, open-source speed lines instead of the border, enemies that
reach every floor), then "a lot of building ... so many additions, well beyond the things I'm asking" (NIGHT20.md "What
GG said"; OWNER.md). Task U3-34.

**Shipped** (each behind a design block, `-skygrindOff20` = NIGHT19's game; NIGHT20-REPORT §1-2):
- THUNDERCLAP shockwave, 18 s, one charge (design.shockwave); the KESTREL gust retired.
- Zoom on the KESTREL / STITCHER / LONGWAVE / ZAPPER (design.zoom).
- Pad v3: RB next weapon, hold = wheel (pad.n20).
- Regen STRICT (regen.n20).
- AIR world streaks + boost blur (SpeedLines20.cs).
- Enemy routes: drops, leap, climb links, ladders (design.routes).
- NIGHT19's ZAPPER / BOUNCER / DRONE merged.
- 22 additions rows (design.adds.<row>): LANCER, WARDEN, affixes, HUNT + ELITE, twists, BULWARK (off), announcer, NEXT,
  debut cards, reveal, threat bar, CODEX, SKYGRIND TIME, combos + SENT, FORGE tiers, TORCH + VORTEX, BEACON, first
  fifteen v2, ASSIST, pad hint card, CONTROLS glyphs, WEEKLY, WHAT GOT YOU (NIGHT20-LOG additions table).
- ~231 commits `u3-n19..u3-n20` (git rev-list); judges THE CLIMB and THE MARATHON; FingerprintBaselineN20 (560a4e0).

**Numbers (A = NIGHT19's game -> B = final 560a4e0; NIGHT20-REPORT §3):**
- S1 4 -> 0; S3 10.13 x -> 1.51 x; S7 1.15 -> 1.92; S8 133.7 / 67.1 -> 81.2 / 44.2 s; S9 15 -> 18 %.
- CLIMB R1 Survival 477 -> 0, DRYDOCK 55 -> 4; R2 never -> 8.7 s; R3 3 -> 1 of 10.
- MARATHON M-1 6.0 -> 10.0; M-4 0.81 -> 0.61 (target 0.15-0.4).
- M8 v2 L1-L5 PASS; FEEL-METRICS 193 / 197 PASS rows, 25 / 26 FAIL rows (M9 pop-ins on new screens).
- Blind final four: 26 of 56 judgements (46 %) to tonight; speed lines 3 of 4 reviewers; shockwave 4 of 4.
- Suite 1569 / 1569; golden 39 sets 0 mismatches; fingerprint EQUAL; smoke alone 209.1 s DONE (HANDOFF.md).
- Bench p99 14.26 / 14.54 -> 17.36 / 114.51 ms: not comparable (another session's jobs).

**Close: 3 DONE / 9 SHORT** (DONE U3-34.1, .2, .4; NIGHT20-LOG close table; tasks.json U3-34 rows).
**SHORT** (measured reasons, NIGHT20-LOG close table):
- .3 shockwave: farm uses per round not measured (time).
- .5 regen: M-4 0.61, R1 / R2 FAIL; BLOOD PRICE / MEND are GG's call.
- .6 routes: CLIMB R3 1 / 10; DRYDOCK crow's nest R1 4 needs a map edit.
- .7 speed: L6 cost unmeasurable (bench cap, contaminated runs).
- .8 guns: S2 / S4 (SWAP-CANCEL 19 %) / S6 (shotgun 60 %) unmet.
- .9 additions: most rows EditMode only; BULWARK off; ASSIST +3 unmet.
- .10 not cheap: S15 46 % < 70 %, magenta rank square.
- .11 loop: round 2's reviewers lost to the spend stop.
- .12 close: PARITY FARM and free farm stopped (3-5 x slow).

**Prompt shape:** NIGHT20.md 772 lines; GG's words, switches table (11 rows), contract, 12 items, clock, reviewer and
builder briefs; box 8-8.5 h; caps on builds, reels, farm batches, bench runs (4), reviewers; builders 6 (G, P, E, N, H, V),
then 4 fresh (EN, G2, HV, P2). Build share: not checked. Forge experiment: GG's playtest fixes first, then a wave of
proven additions from a research backlog (`Research/2026-10-06-additions-backlog.md`), each row behind its own switch.
Outcome: 22 rows landed, but only 3 measured on a Player, and DONE fell to 3 of 12.

**What went wrong in the run:**
- Spend-limit stop ~01:05-01:52: every agent ended; three tests-first commits stayed red until finished.
- The bench and the final farm ran beside another session's jobs.
- The magenta defect was found at the freeze.

**Lessons added or confirmed:** L-001, L-002, L-003, L-004, L-005, L-006, L-007, L-008, L-009, L-010, L-011, L-012,
L-013, L-014, L-015, L-016, L-018, L-019, L-020, L-021, L-022.

**Reaction:** GG's next requests (2026-10-07) were about the process, not the build; verbatim in OWNER.md "Wants" (the two
2026-10-07 entries): "... I WANT BUILDING new fetaures that match and ideas, innovate, resreach ..." and "... mainly
building so I want to see notable changes a knot changes as in cutting things but additions innovations ...". No
verbatim reaction to playing NIGHT20's build is recorded yet.

# Wave 19: every gun has a job (2026-10-06 16:05 to ~21:30, branch port/stage3, box 5 to 5.5 h, tag u3-n19, cut short)

**Ask:** after NIGHT18's build GG asked for L3 sprint and R3 melee on a pad, bumpers that switch options, a wind gust on
"the first weapon" as a movement option, balance so something can catch you, stamina that starts low, the stamina bar out
of the middle, weapons that are sidegrades with combos ("not ... Zombies, where you just pick up the strongest weapon"),
and pulled assets because the UI is "a little cheesy" (NIGHT19.md "What GG asked for"; OWNER.md). Task U3-33.

**Shipped** (final build code 8a33313; NIGHT19-REPORT §1):
- Pad v2 (L3 sprint toggle, R3 punch / parry, LB boost, RB special + grapple) with the v1 migration.
- PromptText Kenney glyphs (Xbox / PS / Switch); LB / RB tabs (design.pad.n19).
- The KESTREL gust on LT (design.gust): later reversed by GG.
- design.stamina.n19 (0.3 bar/s); the CHARGER (design.enemies.n19).
- Vitals pips, role tags, combo callouts, role cards; the gust's look, textured sparks, horde5's confetti cut.
- Judges THE RANGE, THE CHASE, THE PARITY FARM (Game/Bot/Sandbox*.cs).
- 45 commits `u3-n18..u3-n19` (git rev-list).
- Not in: W's niche retune, ZAPPER / BOUNCER, combos, freshness, the DRONE: left in the builders' worktrees, rescued on
  branches 9687752 and ad1e828 and merged in w20 (HANDOFF.md NIGHT19 block).

**Numbers (NIGHT18 b0 -> final; NIGHT19-REPORT §2, NIGHT19-LOG close):**
- S1 4 -> 4 dominated pairs; S2 COMET 75 % -> 75 %; S7 1.16 -> 1.15; S8 128.2 / 169.0 -> 133.7 / 67.1 s.
- S10 gust lift +2.62 m (above the rocket jump's +1.12 m); S12 3 -> 0 keyboard prompts on the pad; S14 sprint 5 -> 1,
  boost 4 -> 0.
- Suite 1182 / 1192 (7 DroneTests red by design, 3 tape tests); golden 22 exact, 0 mismatches; fingerprint EQUAL; smoke
  alone 190.2 s DONE.
- Bench p99 19.55 / 100.61 ms: not comparable (ran beside the suite).

**Close: 3 passing / 7 short** (tasks.json U3-33.1, .2, .10 passing; .3-.9 short). NIGHT19-LOG has no close table (it
ends "NEXT: the close table").
**SHORT:** every builder item, for one reason: the spend-limit stop at ~17:27 ended all five builders; no rounds, no
blind reviewers, no beyond lists (NIGHT19-LOG "The stop").

**Prompt shape:** NIGHT19.md 791 lines; GG's words, a switches table (10 rows), contract, 10 items, three judges spec'd
first; box 5-5.5 h; builders 5 (P, W, E, H, V) in worktrees from d0705d9. Build share: not checked. Forge experiment:
headless balance judges (seconds to clear + 0.05 x HP) built and hand-checked before any balance change. Outcome: the
judges worked (hand checks AGREE) but the balance work never landed.

**What went wrong in the run:**
- The account's monthly spend limit (HTTP 429) at 1:22 in: all builders ended; the lead idle 17:27-20:55.
- A tests-first commit without its implementation left 7 tests red.
- The bench ran while the Editor ran the suite (a sequencing slip).

**Lessons added or confirmed:** L-003, L-005, L-008 (the gust misread), L-009, L-011, L-012.

**Reaction** (GG after playing this build, 2026-10-06 evening; NIGHT20.md "What GG said", verbatim in OWNER.md): "You
messed up: the left was supposed to be for the first option and that was supposed to do the shockwave, not the pistol.
... I need a lot of building; I'm not seeing enough building."

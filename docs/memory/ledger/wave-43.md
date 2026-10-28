# Wave 43 (NIGHT43): the cohesion night (2026-10-16 16:00-~20:00)

Written by the NIGHT43 lead at its close (a short ledger; w40-w42 still need a full wave-memory harvest).

- **Ask:** GG's long direction after playing n42-r4 to level 3 (Research/2026-10-16-wave43/GG-REQUEST.md, G1-G30): "a major leap in
  quality rather than a collection of tiny patches", foundations first, written design rules, research, real building.
- **Shape:** lead + 9 builders (M1 movement, L1 level 1, W1 weapons, F1 feel, U1 UI, E1 encounters, K1 level 3, A1 enemies' look + the
  BURROWER, B1 THE WICK); two read-only fact sweeps (CODE-FACTS-A / B) and a research agent (5 files) before the shared commit a9da30f6
  (16:18); docs/DESIGN-RULES.md and docs/ROOMS.md written by the lead during the build. The spend limit stopped every builder 17:15-18:50.
- **Causes found:** the wall-jump count refilled only by a landing; doors counted the whole level's enemies and pit bodies never died;
  PIERCE's floating stand-ins; THE FLUE's 1 m overlaps; B after a ride a slam to (0,-48,0); a ground shot before the jump fired on the
  ground and the jump release halved boosts; the orb parry ~0.11 s from the eye; the boss door's violet + red / blue flicker; THE WICK's
  climb pilot turned its stick off the wall (T-59's harness cause); the charged release's muzzle sphere filling the screen cyan.
- **Shipped ON (freeze 19:54, build n43-r5, tag u3-n43):** 22 rows (chimney43, ride43, boost43, foundry43, iceline43, kiln43, gauntlet43,
  parry43, fusion43, slug43, torch43, rooms43, burrow43, wick43; views hud43, counter43, teach43, screens43, bossbar43, feel43, models43,
  look43).
- **Numbers:** verbs 26/26, 20/20, 25/25, 30/30, 30/30; the smoke PASS; doors 17 -> 64/64 rooms; THE FLUE held 45 -> 87.5 %, taps 0 -> 75 %;
  boost 1.5-5 -> 6.3-8 m; parry earliest 1.94 -> 5.61 m; FUSION 955 damage at 22.8 m/s in the Player.
- **SHORT:** THE RING's geometry, T-59 in the Player, the move43 reel's pilot, L1's walk bot, the whole suite (T-65..T-71).
- **Lessons (to fold into LESSONS.md):** a new sim row can kill an old verb indirectly (fusion43's x2 killed launch36's body: run verbs on
  every level after the freeze rows go ON); builders' harness helpers need a builder prefix (N43Arg clash); a builder's "sync" commit can
  duplicate JSON keys; idle builders take the next card at once (F1 finished in 22 min and took look43); the spend limit again (L-003).
- **Reaction:** none yet.

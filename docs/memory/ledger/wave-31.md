# Wave 31 (NIGHT31): forge notes (the harvest after the run fills the rest)

- Forge: 2026-10-10, about 09:55-10:15 (GG's box: 20 minutes). Planning set: NIGHT31.md (the NIGHT30 lead's draft kept as
  NIGHT31.draft.md), Research/2026-10-10-wave31/ (GG-REQUEST, RESEARCH, R1-R3, SPIKE, IDEAS-POOL).
- GG's reaction to NIGHT30 (verbatim in GG-REQUEST.md): bosses still not liked; the grapple does not pull you in; the
  level-2 boss can be camped; the charge shot is gone; LT must do something on every weapon; pistol-only level 1 and one
  weapon per level (asked twice now). DECIDE30 D-2 / D-4 / D-9 unanswered.
- forge experiment: every boss must pass a CAMP TEST in the Player (four scripted campers: centre, edge, highest platform,
  farthest point) before it is DONE; hypothesis: GG stops reporting "you can camp" (said in w20, w29, w30); measure: the
  camp table in NIGHT31-PROOF.md and GG's next reaction.
- Spike (SPIKE.md): the pull-in grapple can come from the Mover's existing ZIP mode set by the owner's `aimHook`; Mover.cs
  untouched. Not run, only read.

## The run (2026-10-10 10:18-~16:00, lead + builders G, W, B, L; paused ~12:55-14:51 by the usage limit)
- Built (all behind design.n31 rows, all ON at the close; -skygrindOff31 = NIGHT30): the pull-in grapple (a Core input
  latch `GrappleTap31` outside Mover.cs sets the zip on a tap; 44 m/s; arrival carry capped at 12 m/s + a 5 m/s pop; the
  cyan diamond mark; the Loom hang), the charge shot back (auto-release 0.5 s past full fixed NIGHT27's smoke failure;
  KESTREL pierce 4 s cooldown, THUNDERCLAP slug 6 s), LT always does something (NAILGUN zoom / detonate, the gauntlet
  parry that returns boss projectiles as `parry31`), one weapon per level (KESTREL; + gauntlet L2; + THUNDERCLAP L3;
  + NAILGUN L4: the lead changed the forge's L2/L3 order), THE STAG (level 2's new boss), the Wyrm / Lantern / Loom rules,
  the spawn rule (32 violations -> 0), the edge-only low-health red, the stale lesson fix, the f8 stall fix.
- Measured: see NIGHT31-PROOF.md. The forge experiment (camp test in the Player) ran: n31-r4 THE STAG 3/5 (harness 5/5).
- Forge decisions that held: the spike (no Mover edit for the pull) held; the reviewer's 10 design fixes (section R) were
  all used. One that did not: the forge's weapon order (the levels were built for the other order).
- Process: four builders by file ownership, 3-5 rounds each by SendMessage (L-033 held); the lead integrated 9 times,
  every gate green each time; the biggest finds came from builders' harness checks (L's pull31 and walk30, B's reel
  timing), not from the plan.

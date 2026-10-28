# a2: open findings (BLOCKING lines, verbatim; the cardgates' a2 verdict lines)

## r1/review-gg.md

BLOCKING: THE MARK (a floor square) / "you're encouraged to just start shooting immediately and I don't want that gameplay" (2026-10-13) / move THE MARK onto THE PRESS's 2.2 m top (a double jump up) and set THE RACK's diagonal again from there, so the room's one shot comes from a jump to high ground, not from standing on a floor tile.

## r1/review-ld.md

**BLOCKING: NONE.** The design is coherent for its job. The MAIN miss and the stall guard break are the lead's call on the meter, not a
geometry fault I can name.

## cardgate-R2.md

a2: PASS
- First-gate BLOCKING (1), MAIN not beating the red: **FIXED.**
  - DULL: the aim is "< 18.6 % on A and < 21.1 % on B", against the red's 21.1 / 23.6 (red/a2 flow-A / flow-B, the STYLE room rows). That beats
    each set by 2.5 pts, the stated band (the red's own A-B spread).
  - Kill gap median: "< 2.8 / < 3.7 s", against the red's 2.8 / 3.7 (the flow-A / flow-B SUMMARY lines). This is an added line, strictly
    below the red.
  - The top rank "BOLD" moved to a guard.
- First-gate BLOCKING (2), THE CARTRIDGE STACK not screening wave 2: **FIXED.**
  - The new nodes are (-6.5, 0, -50.5) and (-6.5, 0, -51.6). From THE MARK's eye (4.9, 1.6, -41.5):
    - node 1 meets the stack's east face (x -4.5) at t 0.825, z -48.92, inside z -50..-48.5. The body ray is at y 1.10 and the head ray at 1.60,
      both under the 1.8 m top. The ray then leaves through the north face at x -5.87, inside the box the whole way.
    - node 2 meets the east face at z -49.83, also inside, under the top for both rays.
  - Both are blocked.
  - Campaign24.cs:913 takes a wave's nodes from waveNodes43[w], else r.nodes (not highNodes). So wave 2, from `nodes`, does spawn at these
    two.
  - From the mark they are 14.5 / 15.2 m away, at least minSpawnDist 12.

## cardgate.md

a2: FAIL the card's MAIN does not beat the red: the red already meets DULL <= 23.6 % on both sets (21.1 / 23.6) and the top rank BOLD; THE CARTRIDGE STACK screens neither wave-2 node from THE MARK
- BLOCKING: MAIN, the STYLE line / rule 7 ("MAIN numbers set against the red, never looser"; L-100: a threshold the red already meets is not
  beaten) / The red already meets "DULL at most 23.6 % on each set": 21.1 % on A and 23.6 % on B (Logs/n65/red/summary.md). On set A the aim is
  2.5 pts looser than the red. The top rank "BOLD or higher" is the red's own (BOLD / BOLD). So the card's only beaten lines are checker items
  (R5, IN VIEW), not measured play. Fix: set DULL below the red on each set (e.g. < 21.1 / < 23.6, beyond a stated noise band), or name another
  play number the red fails.
- BLOCKING: THE CARTRIDGE STACK / rule 10, every piece has a job; GG 2026-10-12 "random pillars that serve no purpose" / Its job is "screens
  wave 2's west nodes from the mark". My ray test from THE MARK (4.9, 1.6, -41.5) to (-6, 0, -52.5) and to (-6, 0, -46) misses the stack
  [-7..-4.5] x [-50..-48.5] (and the press) for both the body and the head. Both nodes are in plain view from the mark, so the 1.8 m box does not do
  its job. Fix: move the nodes into the stack's shadow as seen from the mark (north-west of it, on the far side of the line mark -> stack),
  or move the stack onto those lines.

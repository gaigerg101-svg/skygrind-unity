# Wave 35 (NIGHT35): the fundamentals night (planned 2026-10-11 19:28-~19:58 by the forge; not run yet)

- **Ask (GG 2026-10-11 ~19:28, after playing n34; verbatim in Research/2026-10-11-wave35/GG-REQUEST.md):** the pistol and shotgun "lost"
  charge-and-fire; pickups and the healing item don't show what they are; game principles embedded in the project; ULTRAKILL-style
  teaching text; a grapple that yanks you where you fire, unlocked at level 4-5; a visible wall-ride + zoom effect; six levels polished;
  better lights (no "triangle displays"); the PS2 look that shows; a TV-like moving HUD; Lucio wall riding with wall-ride arenas.
- **Forge findings (research/audit.md):** the charge code is intact and on, but unreadable in play (0.4 s of auto-fire first, auto-release
  at 1.4 s, a 2-arc reticle cue only, no gun reaction, no rail state; no Player check since w33); pickups are plain spinning cubes; the
  grapple "limp" is NIGHT30's hold-to-swing, bare-wall taps are refused (grapple32.tapNodesOnly), and NIGHT34's nodes made it owned from
  level 1; the PS2 pass is nearly invisible (design.n29.crt strength 0.35, pixelStep 0); the wall-ride zoom cue is faint (slowAim27).
- **Prompt shape:** NIGHT35.md ~180 lines; box 3:00, cap 3:20; lead + five builders (W weapons + pickups, T teaching text + verbs reel,
  M Lucio wall ride + grapple, V PS2 + lights + TV HUD + focus look, A six levels with wall-ride rooms); caps builds 6, reels 16, suite 1.
- forge experiment: a no-regression gate (docs/VERBS.md + the verbs35 Player reel run in rounds 1 and 3 and at the freeze; docs/PRINCIPLES.md
  imported by CLAUDE.md); hypothesis: GG's next reaction names no lost or unreadable mechanic; measure: verbs35 RED count at the freeze
  and the number of "lost / can't tell" items in GG's next message.
- **Shipped / Measured / Reaction:** not yet.

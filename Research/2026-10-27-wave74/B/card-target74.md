# B2 target74 card (builder B, NIGHT74): an open weak point off screen is named and pointed at

Row design.n74.target74 (OFF in the repo; ON: `-skygrindRows72,target74:1`). View only (no sim step, tape bit or fingerprint). Written from the code at t0b + B1 (not run).
gg-proxy close #3: THE CROWN says "OPEN 2.9" with no target in the frame. GG 2026-10-12: "hard to understand what to do".

## What it draws
- While the boss bar reads OPEN (UiBossBar43: the tag "OPEN 2.9") and the open weak point is NOT on the screen: the tag becomes "OPEN 2.9 - ABOVE" (BELOW / LEFT / RIGHT / BEHIND; 4 words, the bar's own face, 26 px with hud47) and a cyan chevron (two 40 x 9 px bars meeting at a tip, 1.5 Hz pulse, 96 px in from the edge of the HUD layer) points along the point's camera-local bearing.
- On screen, row off, or any other mode: nothing drawn, the tag exactly as before.
- The weak point is BossAttackClock63's `weak` of the live boss in its window (phase Stagger): THE WICK's core, THE ICEBREAKER's torso, THE WYRM's head / core, THE LOOM's spinneret, THE CROWN's ring (its top blade). Cyan on a boss means "shoot here" (rule W, weak74): the chevron points at the one cyan thing.
- Files: Game/Target74.cs (rule, maths, status), Game/UiTarget74.cs (Ui partial: tag, chevron), hooks in Game/UiBossBar43.cs (3 lines), numbers in UnityTuning.N74B.cs (inset 96, arms 40 x 9 px, half angle 42, 1.5 Hz, text floor 22), tests N74BTargetTests.

## Round 2 (the lead's B2 spot: "a small cyan corner bracket")
Round 1's two 40 x 9 px strokes at 42 degrees are an 84-degree angle: aimed diagonally, a right angle on a frame's edge is a bracket (Logs/n74/pairs/B2/on.png: 45 px at the top left). Round 2 draws an ARROW: a 76 px shaft behind the tip and two 64 px head arms at 36 degrees (72 open), all 16 px thick, each bar with a 5 px dark outline (the HUD's shadow ink) behind it: about 130 px long and 100 px across, readable on the olive lattice and the pale wall alike. Same position (tip 96 px in from the edge), same pulse (1.5 Hz), same bearing; UnityTuning.N74B.cs N74TargetArmPx / ShaftPx / ThickPx / HalfAngleDeg / OutlinePx / OutlineAlpha; test Arrow_BoldEnoughToBeAnArrowNotACornerBracket. Not seen: a Player frame of it.

## The proof (the lead's Player steps)
- The Player log line, once per direction shown (L-014): `[target74] dir=BEHIND boss crown37 | tag "OPEN 2.9 - BEHIND" at 26 px (min 22), 4 words | chevron at -864,0 pointing 180 deg | row on`. Nothing with the row off.
- boss63 has no natural off-screen frame (the reel's camera looks AT the weak point), so the stagger frames can be planted: `-skygrindTarget74Away=<deg>` (a boss63 option, FeelReel.N63V.cs: the stagger frames look that many degrees right of the weak point; 0 without it). 130 gives BEHIND; 75 gives RIGHT or LEFT; 0 is the unplanted reel (on screen: nothing drawn).
  `bash Tools/night/n74-reels.sh <tag> tgon,n52-spire,boss63,-skygrindRows72,target74:1,-skygrindTarget74Away=130 tgoff,n52-spire,boss63,-skygrindTarget74Away=130` (one Player each; the OFF side prints no `[target74] dir=` line).
- Every boss63 shot carries `[weak74] ... | target74 row on|off, weak point <on screen (id)|off screen DIR (id)|no open weak point> |` (Weak74.ShotLine), so the OFF frame of a pair names what the ON frame drew; the frame to pair is `shot-N30-B63-<boss>-stagger.png`.
- Meter: n74-pair.cjs on the stagger frames; the chevron is a cyan edge shape (n74-cyan.py counts it as cyan outside the weak box: expected, and it is the point).

## Not checked
Nothing run in Unity (no Editor, reel or Player): the chevron's geometry (two arms with their pivot at the tip, rotated about it) is worked out from RectTransform's rules and the maths is tested in N74BTargetTests (written, not run); the look of the chevron on a real frame is not seen; the tag's width at 26 px beside "LEG 1 / 3" is not seen (the tag box is 200 px wide with overflow; the hint is hidden under hud47).

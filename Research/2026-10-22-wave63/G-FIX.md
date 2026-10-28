# NIGHT63 builder G: the two open points after T63 and B2 passed (branch n63-FIX from port/stage3 d391c6ef)

Builder G never ran the Editor, a build, a Player or a reel.

## 1. T63c: text63's stale queue and its double withholding (DECIDE63 D-14 points 1-3; Logs/n63/verdict-T63.md round 2)
**The red (read from the code and b10):**
- The roomEnter handler ended the old ROOM CLEAR card but left text63's queue as it was.
- PassQueued returned the slot's line while a boss bar was up and left the queue untouched. A lesson that waited in one room could therefore
  show in the next room, or after a fight.
- On b10 vk, Midair was logged withheld twice: vk:102 Queued, then vk:104 Taught once Jump showed with Midair still in the slot.
  PassQueued had reset `lastWithheld` on release.

**The change (behind text63 only).** text61's Pass and its log lines are unchanged.
- `Text61Gate.DropWaiting(why)` drops every waiting line with its own `Dropped` fate. The Ui calls it on roomEnter (`left its room, entering
  <id>`) and logs `[text63] dropped: <why>: <line> (technique '<k>')`.
- PassQueued drops the queue when a boss bar comes up (`a boss fight began`) and still passes the fight's lines.
- A withheld line is counted and logged once a run (`withheldOnce`).
- `Text63Left` prints `[text63] nothing still waiting when <why>` when the queue is empty: the critic's point 2, that path had never printed in a
  Player. It prints only with text61 and text63 on.

**Tests (N63TextTests):** RED f84bbdcb (8 / 11 outside Unity). It reproduces the b10 double withholding and a lesson shown after a fight.
GREEN 99af17be (11 / 11; N61TextTests 6 / 6). Gates `--no-editor` GREEN 03:28:52.

**The T63 doneWhen on kiln (not checked in a Player).** b10 vk's Jump waited at vk:88 and showed at vk:103. No room change is logged between
those lines; the verbs reel stands at one lane (FeelReel.N35.Verbs.cs Open35). If a room change ever fell inside a wait, the line would now
read `[text63] dropped: left its room ...` instead of showing. b12's vk log will show it.

## 2. Read-only: "STAGGERED: HOLD LEFT CLICK, SLUG IT" during the spit's wind-up (B2 critic point 3, D-15 point 4)
**The frame and its log:** Logs/feel/n63-b10/b2ak-on/boss63/shot-N30-B63-wyrm-tell.png; Logs/player/n47-reel-n63-b10-b2ak.log:
- 162: `stagger 18000 staggered`
- 169: `[tell63] cue wyrm spit | level 0.45 at wind-up 0.01`
- 170: `stagger end 23600 after 5600`
- 177: `attack spit windup 23592 hit 24492`

**The cause: the sim's state, not a view bug.**
- **The HUD's hint is the sim's.** `SurvivalSim.N43.Bosses.cs:73` reads `bool stag = wyrm31 != null && wyrm31.Staggered;`. Line 81 picks
  `s.hint = dying ? null : stag ? b43WyrmStagger : ...`. `b43WyrmStagger` is config's `"wyrmStagger": "STAGGERED: HOLD {fire}, SLUG IT"`
  (Design/unity-config.json:6829). `Wyrm31.Staggered` is `staggerLeft > 0` (Core/Enemies/Wyrm31.cs:123). The prompt shows exactly while the
  sim's stagger lasts; no view keeps it alive.
- **The reel's "stagger end 23600" is not the sim's stagger ending.** It is the boss63 clock leaving its Stagger phase:
  `BossAttackClock63.FromWyrm`, BossAttackClock63.cs:198-245. NIGHT63's B1 fix round (D-6) made a pending tell outrank the stagger, so once the
  spit's tell is logged (wind-up 23592) the clock reads Windup while `Wyrm31.Staggered` is still true. The reel prints "stagger end" when its
  phase leaves Stagger (FeelReel.N63V.cs:203).
- The same file's comment records n63-b2's spit "inside an 18.0-24.3 s stagger". The sim's stagger outlives the spit's wind-up start, and the
  HUD reads it.
- **Why the worm spits while staggered:** the wyrm's attack log (Wyrm, wyrm24) does not read `Wyrm31.Staggered`. The sim reads it only for the
  hint (N43.Bosses.cs:73), the window's share (N47.Wyrm.cs:140) and the dive (N48.Brawl.cs:158). So a staggered worm keeps its tells.

**No fix.** It is not a view bug, and whether a staggered worm should spit is enemy design (GG's, CLAUDE.md rule 7). For DECIDE63: on the
wyrm the player is told "SLUG IT" during a spit that will hit them. The options are GG's: the worm holds its tells while staggered (a sim
change), or the hint gives way to a dodge cue while a tell is pending (a view change, not built).

## Shas (n63-FIX, in order)
f84bbdcb (RED), 99af17be (GREEN), then this notes commit. Pick: `git cherry-pick f84bbdcb^..<notes sha>`.

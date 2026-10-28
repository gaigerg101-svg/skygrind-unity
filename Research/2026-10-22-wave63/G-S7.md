# NIGHT63 builder G: leftover 6 = NIGHT61's S7, kiln's presses read as machines (design.n63.press63, OFF)

Branch `n63-S7` from port/stage3 91f998f8. Builder G never ran the Editor, a build, a Player or a reel.

## The red (read only)
- gg-proxy at NIGHT63's start (Logs/n63/gg-proxy-start.md item 1) and NIGHT62-REPORT section 1 item 1: "kiln's presses still plain blocks";
  NIGHT61's slate S7 (Logs/n61/slate.json): the N57 critic, "the presses read as plain dark pillars ... the shape of GG's random pillars that
  serve no purpose".
- Frames: `Logs/feel/n62-p0/t61off/a157/shot-N29-L57-n57-kiln-kb3-entry.png` (THE PRESS HALL's entry: three presses in view, the nearest on
  the left about 280 x 760 px: dark boxes with the hall's maroon and blue bands, nothing that says machine) and `Logs/feel/n62-freeze/vk/
  verbs37/f1000.jpg` (THE PRESS HALL, a dark block dead ahead).
- What they are (Design/levels/n57/kiln.plan.json, kept in the level's `campaign.boxes`): four boxes of zone kb3 whose job is "hero prop: a
  drop press (the charger's stun post)", 1.5 x 2-2.85 x 14 m, floor to roof (collision kind wall). They are the charger's stun posts: they do
  not hurt the player.

## What it adds (view only: no collider, no sim box, no nav, no flow change)
- `Game/Press63Math.cs` (pure) and `Game/Press63.cs` (self-installed like Fixture61: no hook line in any other file). On a map whose campaign
  boxes name drop presses, the first frame the row is on, each press gets, round its unchanged box:
  - **the frame:** four corner rails (0.24 m, 0.12 m proud of the box) from the floor to a crossbeam at 9.5 m (0.9 m tall, overhanging 0.18 m);
  - **the piston:** a chrome strip (36 % of each face's width) down every face, plinth to beam;
  - **the ram head:** a 1.1 m steel block round the column (0.24 m proud) riding up and down on its own clock (`HeadY` on the FeelClock: a
    1.7 s eased lift, a 0.8 s hold up top, a 0.22 s drop with gravity's square, a 0.5 s dwell on the plinth; each press 0.83 s out of phase,
    so the hall stamps in turn); it never enters the plinth or the beam;
  - **the plinth:** 1.0 m tall, its faces striped (parallelograms 0.32 m wide leaning 0.32 m) in bone and dark steel. **No red-orange** (a press
    does not hurt the player: CLAUDE.md rule 8) and **no yellow** (the parry tell's only: DECIDE63 D-1); N63PressTests classes every colour with
    the boss63 cue meter's hue classes;
  - **the lamp:** a housing on the crossbeam's face toward the hall's middle with a lens (the neon material) lit while the head waits up top,
    dark otherwise. It is an emissive lens in a fixture, not a scene light (no Light component: the hall's lighting is unchanged).
- Materials: the stone material's vertex colours for the machine (what Fixture61's mounts use; L-096: the stone shader reads vertex colour), the
  neon material's `_BaseColor` (a MaterialPropertyBlock) for the lens.
- **The switch, every frame** (D1's contract): NightRows.ViewOn(cfg, "press63"), -skygrindOff63 or older, -skygrindNoPress63. Off: the root
  hidden; nothing is built until the row is first on (NIGHT62's hall exactly with the row off from the start).
- **The row:** `node Tools/night/n63-g-rows.cjs` wrote design.n63.press63 (a view row: _why, enabled false, enabled_why, numbers {}) as
  design.n63's last key in both config copies, byte-identical; the script asserts nothing else changed. On a pick onto a tree whose design.n63
  gained rows since: take HEAD's two config copies and run the script again (L-065).
- **Proof (L-089):** one Player.log line per map built: `[press63] presses on n57-kiln: 4 dressed (campaign boxes with job 'hero prop: a drop
  press'), parts <n>, stripes <n>, heads 4 on the FeelClock | their boxes, colliders and nav untouched`; `Press63.Dressed / HeadsMoved /
  LampsLit` for a reel.
- Numbers: `UnityTuning.N63GS7.cs` (Unity-only view geometry and colours, one comment each).

## Tests
`Assets/Tests/EditMode/Game/N63PressTests.cs` (5): the row (a view row, the checker clean, off under -skygrindOff63), which boxes, every part
on its press, the head's clock (bounded, periodic, the drop faster than the lift, the lamp lit only up top, presses out of phase, never into
the plinth or the beam), the colours (no danger hue, no parry yellow). Outside Unity on the dn-check scratch build (a scratch runner, not the
Unity test runner): RED 1 / 5 on `4463ab94` (the colours test passed on the placeholder), GREEN 5 / 5 on `34aacf74`.

## Shas (n63-S7, in order)
`4463ab94` (test-first, RED), `34aacf74` (the presses, the row script and the row in both config copies: GREEN), then this notes commit.
Pick: `git cherry-pick 4463ab94^..<notes sha>`. dn-check 0 errors on both code commits (scratch deleted after each). `bash
Tools/night/n63-gates.sh --no-editor` on 34aacf74 (with the row in both config copies): all GREEN 00:23:16 (config copies identical; golden 0
failing, 0 mismatches, 22 replays exact; fingerprints plain and --off57..--off63 EQUAL). The flow gate (--flow) was not run by me.

## Player steps for the lead
1. Build with the row on: `node Tools/night/n46-build-rows.cjs <tag> on n63.press63`.
2. **SPOT pose:** a157 n57-kiln `shot-N29-L57-n57-kiln-kb3-entry.png` (THE PRESS HALL's entry: three presses in view, the nearest about 280 x
   760 px), on vs off on a same-run pair (D1: the row is read every frame), else two runs `p7on,n57-kiln,a157` and
   `p7off,n57-kiln,a157,-skygrindNoPress63`. Visible change: the dark pillars become presses (rails, a chrome piston strip, a ram head, a
   striped plinth). Changed px: `python -I Tools/night/n62-gun-yellow.py --pair <on> <off> --rect 0,0,1,1`, want >= 41,472 (not measured; the
   nearest press's chrome strip alone is about 100 x 600 px on that frame by its size). Second pose: verbs37 vk `f1000` (THE PRESS HALL) on
   the same build.
3. Proof: `grep "\[press63\]" Logs/player/n47-reel-<tag>-p7on.log` (one line, "4 dressed"); the off side none.
4. Colliders unchanged: `bash Tools/night/n63-gates.sh --flow` (the sim never reads the row: n46flow n57-kiln byte-equal) and verbs37 n57-kiln
   31 / 31 on the build.

## Not checked
- Anything in a Player: the look, whether a press part z-fights the hall's own bands (the rails and strips stand 0.06-0.12 m proud), the
  changed px, the head's motion on screen, the lamp.
- Whether the stripes read as warning stripes at the reel's distance (bone on dark steel, no red-orange by rule 8).
- n52-kiln (or any other level) has no box with that job: nothing is drawn there (the proof line says "none").

## S7b: the fix round after S7's GATE FAIL (branch n63-S7b from port/stage3 d13257ab)
**The red (Logs/n63/verdict-S7.md; the b5 s7k pair, read by eye):** the numbers passed (292,689 px), the visible rule failed: the blind SPOT
saw "tall, multi-part columns in tan and dark grey with black-and-tan hazard stripes". The critic's reasons from the frames: the chrome
strip rendered dull lavender (49, 48, 76); the near press's side faces tan like the hall's walls; the old pillar's rose and blue bands between
the rails; the 4th press's head a stray steel-blue slab; the lens a tiny blob; the bone stripe hue 36. My reading of the cause: the boxes are
1.5 x 2-2.85 x 14 m, floor to roof, so a frame hugging the box is still a column; the parts were the stone material, lit by the hall's dim
warm light (B8C4D0 came out at about a tenth of its value), so no part could be bright; the working parts sat at 9.5 m, near or above the
frame's top for the nearest press.

**The change (view only, same row; Press63Math.cs, Press63.cs, UnityTuning.N63GS7.cs):** a press's silhouette at the eye: bed 0-1.1 m (0.28 m
out, a lit top line), a near-black throat round the column from the bed to the crown (the gap's back; it also hides the old bands), four
bright guide columns at the corners, the ram head (1.0 m, 0.26 m out, a bright die at its bottom) riding 1.13 -> 3.25 m (raised: a 2.15 m
gap over the bed), a bright rod down each face from the head to the crown, the crown 4.4-5.8 m (0.3 m out, the widest) with the lamp (a
0.72 x 0.5 m lens) toward the hall, a dark stack to the roof. No stripes. Every part UNLIT (FxDraw.Opaque per colour): its value is the hex.
Every part stands out less than body.capsuleRadius (0.35 m; max 0.345 m, the lens), so the eye never enters a part of the box it collides
with. The colliders, the box and the nav are unchanged (no collider, CoreGrid or nav call in the patch).

**Tests (N63PressTests, test-first):** RED 0caedc16 (3 / 8 outside Unity), GREEN 9479f5b2 (8 / 8): the silhouette (a bed, a >= 1.8 m gap under
the raised head, the crown the widest, the gap's middle within 1 m of body.eyeHeight); the old pillar covered floor to roof (sampled 2 cm out
every 10 cm on two presses, both lamp sides) and every part, moving or not, within body.capsuleRadius; the piston (four rods from the head's
top to the crown, in front of the throat, behind the crown and the stack) and the value contrast by sRGB luma (throat <= 0.08; rod +0.6,
guide columns +0.5, head +0.3 over it; the die +0.15 over the head; the bed's top line +0.15; the stack <= 0.12); the lamp (lens >= 0.6 x 0.4
m on the crown's face toward the hall; lit >= 0.85; off +0.15 over its housing); the clock as before; no warm hue at all (sat >= 0.1: hue
outside 330-75), never danger, parry, cyan or magenta. The RED commit replaces two of S7's own assertions (the old part list, the striped
plinth): a redesign the GATE asked for, stated in the commit.

**Player steps for the lead:** the same as S7: build with `n63.press63` on; the SPOT pose a157 n57-kiln `shot-N29-L57-n57-kiln-kb3-entry.png`
on a same-run pair (`-skygrindSameRun63 n63.press63`); the proof line `[press63] presses on n57-kiln: 4 dressed ... colours <n> (unlit)`;
verbs37 x5. What a spotter should see: four presses, each a dark bed, a black opening with a grey ram head and bright rods and columns, a
heavy dark crown with a square lamp; the near-left one fills the left third. **Not checked:** anything in a Player (whether the unlit parts
look pasted on beside the lit hall, the 4 cm throat against the box's own faces (z-fighting), the head's motion, the px on the pose, the
fog on unlit parts at 25 m); whether the head is up or down at the SPOT frame's moment (the four presses are 0.83 s apart in a 3.22 s cycle,
so at least one shows its gap open in any frame: arithmetic, not checked).

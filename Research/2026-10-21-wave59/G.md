# NIGHT59 builder G: G1 kill55's marks after the flash

No Editor, Player, build or reel was run (builder rule). Every number below was read from existing frames or a planted copy of one.

## 1. The planted probe (commit 9383429a, `Tools/night/n59-g-probe.py`)

Files read: `Logs/feel/n58-freezek/{g1on,g1off}/a258/shot-N55-K-{hit-before,hit-next,flash-end,hit-after}.jpg` (MAIN repo; the
n58-freezek a258 pair, kill55 on / `-skygrindNoKill55`). Command: `python -I Tools/night/n59-g-probe.py Logs/feel/n58-freezek`.

| a258 frames (n58-freezek) | on | off |
|---|---|---|
| MAIN replica at the flash end (K55Marks: changed and V < 0.12, share of the body) | 0.62 % (reel line 0.69 %) | 0.23 % (reel line 0.41 %) |
| the same at +0.5 s (hit-after) | 0.10 % | 0.15 % |
| probe V at the mark's centre (7x7 px at 958, 551), before -> flash end | 0.74 -> 0.52 | 0.73 -> 0.52 |
| probe V at the mark's centre, before -> +0.5 s | 0.74 -> 0.64 | 0.73 -> 0.64 |
| the mark's visible footprint (on darker than off by > 0.12), flash end / +0.5 s | 3.34 / 2.91 % of the rect, median V 0.28 / 0.29 | |
| PLANTED: the off flash-end frame with the mark painted at its own colours (hole r 30 px) | | 16.35 % GREEN (unplanted 0.23 % RED) |

My replica's body rect is estimated from the flash's white on the hit's next frame, so it reads 0.62 / 0.23 % where the reel read
0.69 / 0.41 %.

- **The cause is that the mark is hidden. The colour is not being lifted.** At the mark's drawn centre, the on frame and the off frame
  read the same V: 0.52 / 0.52 at the flash end and 0.64 / 0.64 at +0.5 s. If fog or the grade were lifting a drawn mark, the on frame
  would still differ from the off frame there. Only crack strips beyond the centre show: about 3 % of the rect, median V 0.28.
- **Fog cannot be the cause.** `fog.near` is 45 m (config). The a258 victim fills a 113 px wide rect for its 0.8 m width, so it is a
  few metres away (the exact distance was not checked).
- **The grade does not lift black.** The frame's 1st percentile V is 0.008 on both sides.
- **The hole sits under the pill's parts in front of the torso.** The decal is placed on `ShellR` + `Kill55MarkLift` (0.045 m). The
  hit-before crop shows a round part over the torso at the crosshair.
- **The measure can see a mark.** A mark drawn at its own colours reads 16.35 % (want >= 5 %).

## 2. The draw (commit dbe62f7a)

- `Assets/Shaders/Resources/Kill59Mark.shader`, a new unlit shader:
  - **The depth pull.** Each vertex moves toward the camera along its own view ray by `Kill59MarkPull` (0.3 m). The mark keeps its
    pixels but wins the depth test against the husk's own parts. Anything nearer than 0.3 m still covers it.
  - **Fog and the near plane.** There is no fog. The pull stops `Kill59MarkNearest` (0.35 m) past the camera.
  - **The facing cut.** A mark whose shell normal turns away from the camera is not drawn (`Kill59MarkFacing` 0.05).
- `Kill55Damage.Ready` builds the marks on this shader. It falls back to NIGHT58's body copies under `-skygrindOldMarks59` (the same
  build's A / B) or when the shader is missing from the build.
- The marks stay behind kill55: `design.n55.kill55`, still OFF in the repo, and `-skygrindNoKill55`.
- a258's input line now names the draw: "by Kill59Mark (NIGHT59: pulled, no fog)" or "the body copies (NIGHT58)".
- The numbers are Unity-only, in `UnityTuning.N59G.cs` with comments. There are no gameplay numbers. The web has no hit marks.
- Checks: `dn-check-wt` 0 errors. Fingerprints, plain and `--off58`, EQUAL after each commit.

## 3. Not checked, and what the lead measures

- **MAIN after the change: not checked** (no Player). Reels on the rows-on build copy:
  - `g1on,n52-foundry,a258`
  - `g1off,n52-foundry,a258,-skygrindNoKill55`
  - `g1old,n52-foundry,a258,-skygrindOldMarks59`
  - the ladder: `k1on,n52-foundry,a355` / `k1off,n52-foundry,a355,-skygrindNoKill55`

  Then `python -I Tools/night/n59-g-probe.py Logs/feel/<tag> g1on g1off <x> <y>` (and `g1on g1old <x> <y>`). x and y are the px from
  the new a258 line ("the mark's centre at px").
- **Want:**
  - the a258 line's MARKS AT THE FLASH END >= 5 %;
  - the probe's centre V on the on side well under the off side's.
- **Risk:**
  - **A mark showing through.** A 0.3 m pull can show a mark through a part that stands nearer than the shell by more than 0.3 m, or
    over another enemy's edge within 0.3 m.
  - **A part that stands off further.** If a part stands off the profile by more than 0.3 m, the centre stays hidden. Then raise
    `Kill59MarkPull`.
- **The C-6 flash budget is unchanged.** No flash was added.

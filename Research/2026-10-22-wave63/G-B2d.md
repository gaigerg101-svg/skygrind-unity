# NIGHT63 builder G: B2d, crownFire's cue (branch n63-B2d from port/stage3 ab8ade3e)

Builder G never ran the Editor, a build, a Player or a reel. Read: Logs/player/n47-reel-n63-b9-b2{p,k,bp,bk}.log, n47-reel-n63-b7-b2p.log,
Logs/feel/n63-b9/b2k-on/boss63/shot-N30-B63-wyrm-tell.png.

## The lead's three points, read from the logs
1. **The wyrm's spit is fixed** (as the lead read it): after `cue wyrm spit start 24000 hit 24492 lead 492 hue 10 sat 0.92 ... danger yes ...
   verdict ok objects 2 obj KILN WYRM/tell63 cue`.
2. **crownFire is still off-hue after the fix.** Before and after alike: `cue crown37 crownFire start 9950 hit 11233 lead 1283 hue 41 sat 0.14 ...
   verdict off-hue objects 31 obj THE CROWN/crown clapper`, and `tell frame crown37 crownFire ... recoloured 0, column drawn`.
   - The clapper's fire colour is Crown47WhiteHex 0xFFF4DC (hue 41, saturation 0.14) at gain 2.4 (Crown37View.Look47.cs:109-110).
   - The meter takes the strongest riser's hue. The clapper rises to 2.4; the column reaches 0.6 (Tell36Red x BossTell63Gain x level 1).
   - The late pull left the clapper because ToDanger returned at its first test: saturation 0.14 is under Boss63CueMinSat (0.5), a colour with
     no hue the meter reads. The 35-45 arc was never reached.
3. **The crown's tell pair is not 0 px on b9.** b2p's toggle lines: shot 1 windup 0, shot 2 idle 0, shot 3 flinch 0, **shot 4 tell 766,631**,
   shot 5 stagger 0 (b7's tell: 766,708). The 0 the lead read is shot 1's: the windup shot comes before crownFire's wind-up, when the column is not
   drawn. The wyrm's tell pair (8 px) is a pose: its held ON frame (b2k-on shot-N30-B63-wyrm-tell.png) faces a dark block at close range, and the
   column is out of view. The column drew: the cue line's strongest riser is "KILN WYRM/tell63 cue".

## The change (BossTell63.ToDanger, UnityTuning.N63G.cs; only on listed attacks, through the late pass)
A white-hot warm glow on a listed attack's wind-up is pulled to the danger hue at its brightness when all three hold:
- pale: saturation under Boss63CueMinSat;
- tinted warm: saturation >= BossTell63WarmMinSat 0.05, and a hue from the table's danger start to its parry start (345-45);
- HDR-hot: level >= BossTell63HotLevel 1.0 (a lit surface stays under 1).

The saturated rule is unchanged (35-45; danger and parry never). Never pulled: a warm white below 1, a pure white, a pale yellow in the parry
band, the boundary's violet-white, a pale cyan. The fix list (wyrm:spit, crown37:crownFire) is unchanged; attacks off the list are untouched.

## Tests
RED aad13f07: N63BossTellTests 8 / 9 outside Unity. GREEN 9d3e74f0: 9 / 9; N63BossCueTests 13 / 13. dn-check 0 errors; gates `--no-editor` all
GREEN 02:30:29.

## Player steps for the lead
On b10, the same boss63 reels on spire with `-skygrindNoBossMotion63 -skygrindSameRun63 n63.bossTell63`:
- **Want:** `[tell63] recolour crown37 crownFire: <n> renderers, ...` with n >= 1, and `cue crown37 crownFire ... danger yes ... verdict ok`.
- **The rest unchanged:** bladeRing, beamCut and lowFire stay `danger yes`; gemShard stays `parry yes`.

## Not checked
- Anything in a Player.
- Whether other pale-hot parts under the crown's root also turn red during crownFire (the recolour line counts them).
- How the red clapper looks.
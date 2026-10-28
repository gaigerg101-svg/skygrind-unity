# NIGHT63 builder G: B2b, B2's fix round (design.n63.bossTell63, OFF)

Branch `n63-B2b` from port/stage3 2a5a1345 (b5's pick). Builder G never ran the Editor, a build, a Player or a reel.

## The red (read only: `grep -h "^\[boss63\] cue" Logs/player/n47-reel-n63-b4-b63?B.log`, bossTell63 off; b63?A.log on)
- The before and after end lines were identical on all five bosses, because the column drew on every non-parry attack and the meter's verdict
  had two faults of its own:
  1. **A yellow riser counted as parryable.** THE CROWN's beamCut x2 and lowFire (sim kind red: dodge only) read `hue 39 ... parry yes danger no
     obj crownLook47/crown point edge`: a parry-yellow cue on attacks that cannot be parried (DECIDE63 D-1), yet the end line said "off the
     danger hue 0" (and "parry drawn 6 of 1"). Parryable must be the sim's kind.
  2. **An ambient wave read as a cue.** THE KILN WYRM's spit: `hue 32 ... obj KILN WYRM/trim 9` is the molten seam wave (WyrmView:
     Wyrm47AmberHex x Wyrm47SeamGain 2.4 on a sine of Wyrm47WaveRate 3 rad/s, about 2.1 s, always on); its `tell 742` is the body's red heat on
     the same wave. The 250 ms reference cannot hold a 2.1 s cycle. With the column on, the strongest riser was still the seam (the column at
     0.6 x level cannot out-rise a 2.4 x seam), so `danger no` stayed.

## What changed
- **The meter, made honest** (`3a7a9557` RED, `b4e4fa0d` GREEN; the lead's approval (a)):
  - `BossCue63.Verdict(c, kind)`: Short (no danger- or parry-hue tell >= 300 ms) / OffHue (a cue found, its strongest riser not in the danger band,
    the SIM's kind not yellow) / Ok; printed on every cue line as `verdict ok | short | off-hue` and used for the end line's lists.
  - The reference reaches back to the previous attack's hit, at most `Boss63CueRefLongMs` 2.2 s and never under 250 ms: one ambient cycle is in
    it; a cue re-lit after the previous hit still rises. The frame buffer 3.0 -> 4.5 s for it.
  - Planted cases (N63BossCueTests): a red attack with a parry-yellow cue is OffHue (yellow kind: Ok; the wyrm's "?": OffHue); a 2.1 s ambient
    wave is no cue with the long reference and IS one with the short (the old false positive, kept as a check); a cue re-lit after the previous
    hit is a cue.
  - **Expect the before to change** on the next before set: the spit's amber was the wave, so with the long reference it may read fine, or show
    its real cue; the crown's 39 deg point edge may also be ambient. That is the measurement being honest, not a fix.
- **The fix list as data, computed from the rule** (`53d05927`): `Tools/night/n63-b2-fixlist.cjs` reads the per-attack cue lines of before logs
  and applies the same rule (mirrored; `--selftest` PASS), `--write` puts the list into `design.n63.bossTell63.numbers.fix` (+ `fix_why`) in
  both config copies, byte-identical, asserting nothing else changed. Run on n63-b4's five before logs: **wyrm:spit (hue 32, kind ?),
  crown37:beamCut (hue 39 x2, kind red), crown37:lowFire (hue 39, kind red)**; every other attack fine and untouched (the slate). It cannot be
  computed live in the game: the measure needs the fix off. **Re-run the tool on the next before set** (`node Tools/night/n63-b2-fixlist.cjs
  Logs/player/n47-reel-<tag>-<before sides>.log --write`) and commit its write.
- **The fix, only on listed attacks** (`0ffb0fac` RED, `53d05927` GREEN; approval (c)):
  - `BossTell63.Wants(boss, attack, kind, FixList(cfg))`: listed and eligible (never a parryable attack, never the wyrm's punch window).
  - The danger column through the listed wind-up (from its first step), as before.
  - **`BossTell63Late`** (self-installed, execution order 32000, Update and LateUpdate: after the views draw and before the reel reads, and
    after Crown37View's LateUpdate): during a listed wind-up, each renderer (its MaterialPropertyBlock `_BaseColor`) and light under the boss
    view's own root whose colour is a warm off-hue glow (`BossTell63.ToDanger`: saturated, a hue above the danger band's top 30 deg and up to the
    parry band's top 62 deg) is drawn in Tell36Red's hue at the same brightness; when the wind-up ends each goes back (unless its view drew it
    again). So the wyrm's amber seams and the crown's yellow-ish point edges go red-orange while a listed attack winds up: the cue the player
    reads is the danger hue, and the meter's strongest riser should follow it. The column is never pulled; the record is cleared the frame the
    fix stops (the row off: D1's off half is clean).
  - Proof lines: `[tell63] cue <boss> <attack> | level ..`, `[tell63] skip <boss> <attack> (not on the fix list: already in time and in the
    danger hue | parryable | BossTell63Skip)`, `[tell63] recolour <boss> <attack>: <n> renderers, <m> lights to the danger hue`.
- **The SPOT frame** (approval (d)): FeelReel.N63G takes `shot-N30-B63-<boss>-tell.png` at 70 % of the first listed wind-up per boss (the list
  is read with the row on or off, so both sides frame the same pose; Shot30 adds D1's same-run off half) and prints `[boss63] tell frame <boss>
  <attack> at wind-up <w>: ... | bossTell63 on|off, recoloured <n>, column drawn|not drawn`.

## Tests
N63BossCueTests 12 / 12 and N63BossTellTests 7 / 7 outside Unity on the dn-check scratch build (a scratch runner, not the Unity test runner):
meter RED 10 / 12 on 3a7a9557, GREEN on b4e4fa0d; fix RED 4 / 7 on 0ffb0fac, GREEN on 53d05927. dn-check 0 errors on each code commit
(scratch deleted after each). `bash Tools/night/n63-gates.sh --no-editor` on 53d05927 (with the fix list in both config copies): all GREEN
00:52:59 (config copies identical; golden 0 failing, 0 mismatches, 22 replays exact; fingerprints plain and --off57..--off63 EQUAL).

## Shas (n63-B2b, in order)
`3a7a9557` (meter RED), `b4e4fa0d` (meter GREEN), `0ffb0fac` (fix RED), `53d05927` (fix GREEN, the list in both config copies), then this
notes commit. Pick: `git cherry-pick 3a7a9557^..<notes sha>`. If the config conflicts on a pick, take HEAD's copies and re-run
`node Tools/night/n63-b2-fixlist.cjs <the b4 before logs> --write` (L-065).

## Player steps for the lead
1. Build: `node Tools/night/n46-build-rows.cjs <tag> on n63.bossTell63`.
2. The pair on THE KILN WYRM and THE CROWN (bossMotion63 off on both sides): `bash Tools/night/n63-reels.sh <tag>
   t63kA,n57-kiln,boss63,-skygrindNoBossMotion63 t63kB,n57-kiln,boss63,-skygrindNoBossMotion63,-skygrindNoBossTell63
   t63pA,n52-spire,boss63,-skygrindNoBossMotion63 t63pB,n52-spire,boss63,-skygrindNoBossMotion63,-skygrindNoBossTell63` (and the other three
   bosses' A / B for "attacks already fine are untouched": no `[tell63] cue` line there, their cue lines equal on A and B).
3. Greps: `grep -h "^\[boss63\] cue\|^\[boss63\] tell frame\|^\[tell63\]" Logs/player/n47-reel-<tag>-t63*.log`. Want on A: `[tell63] cue wyrm
   spit`, `[tell63] recolour wyrm spit: <n> renderers ...` (n > 0), the spit's cue line `danger yes` / `verdict ok`; the same for the crown's
   beamCut / lowFire; `[tell63] skip ... (not on the fix list ...)` for the rest. On B: the corrected before (verdict per attack); then re-run
   the fix-list tool on the B logs and compare with the committed list.
4. **SPOT poses:** `shot-N30-B63-wyrm-tell.png` (the spit's wind-up at 70 %: the column round the head, the seams red-orange instead of amber) and
   `shot-N30-B63-crown37-tell.png` (beamCut: the point edges red-orange instead of yellow-ish), on vs off on a same-run pair (D1) else A vs B.
   Changed px: `python -I Tools/night/n62-gun-yellow.py --pair <on> <off> --rect 0,0,1,1`, want >= 41,472 (not measured).

## Not checked
- Anything in a Player: whether the recolour shows on screen (the views' MaterialPropertyBlock `_BaseColor` on the additive neon material: the
  path Brawl40View's glow uses), the changed px, the after numbers.
- Whether a view sets some of these colours only on a change (then the restore path puts them back; a view that writes the colour through
  another property than `_BaseColor` is not touched).
- The wyrm's spit and shed have no tell kind in the sim: treated as dodge-only.

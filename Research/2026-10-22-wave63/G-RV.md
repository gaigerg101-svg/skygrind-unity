# NIGHT63 builder G: the review's S3 and S7 (Logs/n63/review-n63.md), branch n63-RVG from port/stage3 fe55804c

Builder G never ran the Editor, a build, a Player or a reel.

## The red (read from the code and the config)
- **S3:** three hue band sets. The cue meter and the tell's late pull (BossCue63, BossTell63.ToDanger) and the fix-list tool used
  UnityTuning.N63G.cs's danger 345-30 and parry 38-62. The colour table (design.n63.colours63.numbers.bands, which C1's meter and audit use)
  says danger 345-35 and parry 45-70. The wyrm's amber #FF9A28 is hue 31.8: danger for C1, off-hue for B2.
- **S7:** BossTell63.ToDanger pulled every saturated hue in (30, 62] to red during a listed wind-up. That range covers the whole parry band, so
  the wyrm's punch window #FFD23A (hue 46) would turn red-orange if it opened during a listed spit wind-up. Whether they overlap was not checked
  (the review's own note).

## The change
- `BossCue63.Use(cfg)` reads the bands from the colour table through `Colours63Math.From`, cached per config object. `BossCue63.Bands` is the set
  in use. DangerHue and ParryHue use `InArc`, the same rule as RoleOf. The boss63 reel hands it the game's config at each measured strike and at
  the end line, which now names the bands ("bands danger 345-35 parry 45-70 (colours63's table)"). BossTell63.Draw hands it the config every frame.
  UnityTuning.N63G.cs no longer types any bands.
- `BossTell63.ToDanger` pulls only the hues strictly between the table's danger band and its parry band (35-45 now). Danger and parry hues are
  left alone.
- `Tools/night/n63-b2-fixlist.cjs` reads the same table and recomputes each line's danger from its printed hue (whole degrees) and saturation. The
  tell stays as the line printed it.
- Re-run with `--write` on n63-b6's five before logs: the list is unchanged. wyrm:spit is short (no riser at all). crown37:crownFire is hue 41
  at saturation 0.14: not danger under either set. In both config copies only `fix_why`'s text changed. The fixture has the same lines.

## Tests (test-first)
RED is 1e46e1b6 (outside Unity: N63BossCueTests 12 / 13, N63BossTellTests 6 / 8). GREEN is 6adf14c1 (13 / 13, 8 / 8; N63ColoursTests 10 / 10 and
N63BossColoursTests 7 / 7 unchanged). What they pin:
- the bands equal the table's;
- the amber is danger, a 66 deg yellow is parry, 39 deg is neither;
- at every half degree, saturated, the meter's class and RoleOf agree on danger and parry;
- the pull takes 36.5, 39 and 41 deg, and leaves the amber, the parry yellow, #FFD23A and the spool alone;
- the fix-list rule test no longer lists the 32 deg spit line.

A `[SetUpFixture]` (N63BandsBaseline) hands every Game test the repo config's table.

Changed assertions:
- The RED commit changes two of B2b's own: the amber and the parry yellow were pulled.
- The GREEN commit moves one sample colour: 0xFFC23C (41 deg) becomes 0xFFD23C (46 deg). It was chosen inside the old parry band; the rule it
  pins is unchanged.

N63BossMotionTests and N63BossClockTests need the engine (ECall outside Unity): not run by me.

## Checks
dn-check 0 errors; `--selftest` PASS; gates `--no-editor` all GREEN 02:07:33. On a pick conflict in the config copies: take HEAD's two copies
and re-run the tool with `--write` on the same five logs (L-065).

## Not checked
- Anything in a Player: the reel's end line with the bands, and the pull leaving a punch window yellow.
- Whether a real before set measured under the new bands changes the list. The b6 lines carry the tell computed under the old bands; a wider
  danger band could make an earlier riser count as a tell.
- bossColours63's `seams_why` still says "hue 36". That is builder D's row text (the review's aside), not changed here.

# NIGHT39 proof (the lead; every number from a run tonight; sources: NIGHT39-LOG.md)

Freeze: `92eb280a` (port/stage3), build `Builds/n39-r4/SKYGRIND.exe` (Succeeded, 0 errors). Rows ON: p39, ride39, slow39, boss39 (design.n39);
hud38, crown38, foundry38, kiln38, span38, spire38 (design.n38). OFF: doors38, juice38, iceline38, finale38, stag38, ride38, and ride39's
noRideLook.

## Sim parity (at the freeze commit)
- Golden (CoreHarness): 39 vector sets / 22 replays, 0 mismatches.
- Fingerprint (SimHarness): EQUAL plain and `--off39`; EQUAL at every merge tonight (V, R1, F1, P1, E, K4, B1).
- Editor compile: clean (`compilationFailed` false, 0 console errors) at every merge. G0 (d): the 13:01 NullReference entries did not
  recur in any compile tonight (not reproduced; cause not found).

## The gates (the lead's own `n38 gates` runs; E's checker with dash-then-jump, chain release, blast, blast + wall jump, room limits)
| level | result | notes |
|---|---|---|
| L1 n38-foundry | GREEN 2/2 (with ride39 on) | THE GANTRY; walk 10/10 with ride39 (R1) |
| L3 n38-kiln | GREEN 4/4 (+34 % each) | 12 m climbs; walk 10/10 (K4) |
| L4 n38-span | GREEN 3/3 (+31 %) | LIMIT pistolOnly in p2 / p4; walk 10/10 (E) |
| L5 n38-spire | GREEN 4/4 (with ride39 on) | room limits; walk v1-v10 (C2, NIGHT38) |
| L2 n38-iceline | G2 / G3 GREEN; G1 RED (no skate-only gap on flat ice) | OFF: GG's call |
| L6 n38-finale | x6 +15 % RED (35 cheats); PASS with a what-if pistolOnly | OFF: GG's call |

## Player checks (campaign boot: `reel.sh` + `-skygrindMap <id>` + `-skygrindReelOnly <scenario>`)
- n39-r2 (rows on in the build's copy): verbs37 n38-foundry 26/26 (rows 9 / 11 with ride39, 25 / 27 with p39); n38-kiln 29/30 (row 13:
  the reel's 0.02 s press window under one frame on the low preset, K4); n38-spire 26/30 (the grapple rows: no node on the level).
- n39-r3: slowworld PASS with slow39 (own share 0.30) and with `-skygrindOff39` (0.75); crown37 GREEN with boss39 + crown38; verbs37
  n38-span 30/30.
- n39-r4 (the freeze, the shipped config): verbs37 n38-foundry 26/26, n37-iceline 30/30, n38-kiln 29/30 (row 13: the reel's frame
  window), n38-span 30/30, n38-spire 26/30 (the grapple rows: no node), n35-finale 30/30.
- 0 NullReferenceException in every Player log tonight.

## Harness checks by row
- p39: `n39pistol` GREEN (added latency = the click length: 40-140 ms clicks fire 42-142 ms after the press); `n39reels` 8/8; Pistol39Tests 4/4.
- ride39: chain speed wall by wall 9.2 / 13.2 / 16.0 / 16.1 / 16.1 / 16.2 m/s (was to 23.6); Ride39Tests 3/3.
- slow39: `n39slow all` GREEN (signal 100 %; latch 0.258 sim s -> 0.717 real s; the parry window on the first frame after the freeze).
- boss39 (bot, 10 seeds, all killed): WICK 44.7 s, STAG 37.9, KILN WYRM 43.0 (in wyrm39-arena), LOOM 45.4, CROWN 36.5, Stag31 37.5-44.4.
- juice38's damage static (V): a 3 s lava stand 6 bursts -> 1; Juice39HurtTests 3/3.
- AutopilotTests 9/9, MapChoiceTests 50/50 (split per map) in the Editor.

## Not proven
- The freeze batch suite: stopped by Claude Code for low memory (it started at 6.7 GB free, under the night's 8 GB rule; three NIGHT38
  Players from 13:10 were still running and were closed afterwards). Not restarted.
- On screen: p39's heavier / cannon look, the no-ride texture, a locked doorway, the boss entrance juice, the parry and whip slow.
- A person's play.

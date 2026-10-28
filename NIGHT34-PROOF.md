# NIGHT34-PROOF: what was measured tonight (every number from tonight's runs; sources in NIGHT34-LOG.md)

Freeze a88fb8d6 (2026-10-11 16:53), play build `Builds/n34-r4/SKYGRIND.exe`. Rows ON: bend34 (split), bendLook, rail34, nomark34,
lines34, calm34, flow34, reach34, air34, combo34. RED = the same build with `-skygrindOff34` (NIGHT33's game).

## Gates (at every Core commit and at the freeze)
- `fingerprint` EQUAL, `fingerprint --off34` EQUAL, `fingerprint --off33` EQUAL (the composed fingerprint runs with every NIGHT34 block
  off: FingerprintBaseline.N34OnPath false).
- CoreHarness: 39 vector sets (48094 records), 22 replays (22 exact), 0 mismatches; Mover.cs edited only inside flow34 branches.
- dn-check 0; Editor compile 0 errors; SharedTests 63/63, TapeTests 14/14, Bend34Tests 8/8, Air34Tests 3/3, Rail34LayoutTests 7/7,
  RunSnapshotTests 8/8, Lesson25Tests 10/10.
- Suite 1 (9ababdfc, batch): 1935 / 1936; the one red was Lesson25Tests' campaign-order test (NIGHT33's order with lines34 on), fixed
  at the freeze (D34-9). Suite 2 (a88fb8d6, the freeze, batch): **1936 / 1936 passed**, 0 failed.

## B1 bend34 (the grapple BEND)
- Harness `bend34` GREEN: every latch bends 20/20 (global and split); 20 latches with no kills: exactly 1 FULL bend; global share
  0.250 on all 69 hold steps; split: the world stepped 836 of 3332 steps inside bends (1 in 3.99), enemies moved on 0 held steps
  (6211 moves on world steps) while the player moved on 2496 / 2496; a swap inside a bend lands 0 steps after the press (15 outside
  and with the row off: NIGHT33's timeline); tape replays EQUAL 5/5 (global and split).
- Player n34-r1 (global): latched a node, share min 0.250, swap steps [0, 0, 0] (RED [15, 1, 13]).
- Player n34-r2 (split, the default): world held on 148 of 173 steps inside, the loop's share 1.000, swaps [0, 0, 0], 1 kill inside,
  the meter 1.00 -> 0.00 -> 0.25 after the kill. Frames looked at: f3 kill (grey world, red burst, crawling husks).
- Not measured: the bend's length in game seconds in the Player (the reel's "real length" is wall time while it writes frames).
- combo34: harness GREEN (3 kills, 2 then 3 guns: BEND BREAKER +150, TRIPLE BEND +250); Player n34-r3: "BEND BREAKER" shown.
- air34: harness GREEN: the same kill airborne vs grounded: damage 25 = 25; points 90 -> 270; style 63 -> 156 (x3).

## H1 bendLook, rail34, nomark34
- Player hud34 (n34-r1, n34-r3): hud33 rule problems 0 at 1080 and 720; 5 slots + hand; 4/4 swaps moved the highlight; the READY
  pulse 1/1; the bend look 1/1 (depth, grade, sky, rail brighter); the grade deeper per kill and restored 1/1; boss bar beside the rail.
- Frames looked at: the bend grade GREEN vs RED (the sky grey-brown, walls desaturated, husks / beams / cyan saturated); the rail crop
  GREEN vs RED; nomark at a wall RED (the diamond) vs GREEN (none); nomark aimed at a node on n34-foundry (the ring, no diamond).

## A1 lines34, calm34
- Measured distances: running jump 6.93 m, slide-hop 12.79 m / hop, dash 6.07 m, wall ride 9.60 m (1.20 s), grapple reach 40 m.
- calm34: dressing rows foundry 612 -> 118, iceline 543 -> 142, kiln 313 -> 68, span 538 -> 139, spire 576 -> 155 (73-81 % fewer;
  no row with a collider cut).
- lines34 (harness): n34-kiln 4/4 GREEN; n34-iceline 4/6 (i2 lane 11.25 m, i5 12.0 m: their own pilot scripts, no nodes);
  n34-foundry 3/7 (f5 13.0 m, f5b 13.75 m, f9 15.0 m: lava-lesson lanes; f7 the 4 m slide corridor, one floor). NIGHT33's files: 0
  nodes in every room. Walk bot 10/10 on n34-foundry, n34-kiln, n34-iceline; n33-span / n33-spire walk unchanged (10/10, same medians).
- Player n34-r3: n34-foundry and n34-kiln booted, 2 fight rooms entered each, 6 frames each; RED n33-foundry booted.
- span and spire: not rebuilt (span with nodes walked 8/10: SHORT); they keep NIGHT33's rooms with the calm pass.

## M1 flow34, reach34, latency34
- flow34 harness PASS: wall coyote 0 -> 12 of 16 late presses; wall buffer 0 -> 10 of 14 early presses; zip carry -1.57 -> 10.40
  m/s; ridePhase: height over a full ride -2.50 -> -0.50 m; lurch: heading 0.25 s after a 90 degree input change 13.5 -> 62.0
  degrees (speed kept 102.8 -> 90.6 %). The buffered-landing no-friction jump already existed (100 % -> 100 %).
- Player n34-r3 flow34: BHOP 3 hops (2 buffered) 100 / 100 / 100 % speed kept; ZIP 7.76 m/s sideways after the release.
- reach34 harness PASS: at surface + 2.95 m the swing and blast miss with the row off and hit with it on; a hit at the margin damages
  the worm and staggers it (0 -> 1).
- latency34: every verb's first sim change is on the press step (0) with flow34 off and on; nothing late.

## Freeze checks on n34-r4
- Smoke (`-skygrindSmoke`): `209.2s DONE: all steps passed` (round 1 cleared in 8 s, 9 kills, 1835 points; the relays 4/4; the HOLD
  ring 3600 / 3600; THE PROTOCOL in 33 s; the exfil radio called), 0 exceptions.
- LevelSelect29: `DONE 20 presses, 20 PASS, 0 FAIL` (n34-foundry, n34-iceline, n34-kiln, n33-span, n33-spire among them), 0 exceptions.
- DoorFree29: PASS on n34-foundry (14 rooms, open before 1, after 14), 0 exceptions.
- bend34 (split): latched, tier full, swaps [0, 0, 0], 6 kills inside, world held on 216 of 261 steps; the reel marked itself RED on
  "ends 0": the six kills kept extending the bend past the reel's capture window (the end is proven in r1, r2 and the harness).
- lines34 on n34-iceline: booted, i1a / i1b entered, 6 frames, calm34 keeps 142 / 543 (73.8 % cut).
- Bosses in the Player (bot pilot, GREEN vs RED `-skygrindOff34`):

| boss | GREEN (rows on) | RED (NIGHT33) | band 15-22 s |
|---|---|---|---|
| THE LANTERN | 21.4 s | 18.3 s | in |
| THE STAG | **10.1 s** | 17.6 s | **under** |
| THE KILN WYRM | 18.6 s | 19.0 s | in |
| THE LOOM | 21.3 s (3 pilot deaths, bar kept) | not run | in (T-45) |
| THE CROWN | 19.6 s | not run | in (T-45) |

  The Stag's drop to 10.1 s is most likely the bend: the pilot grapples, and inside a split bend the boss acts at a quarter speed while
  the pilot fires at full speed. The harness pilots ran without the bend (M's reach34 --flow: stag 19.6 s). Not tuned tonight
  (DECIDE34 N-11). 139 / 142 frames failed to write in the GREEN / RED boss reels alike (T-42's long-reel frame loss, not new).

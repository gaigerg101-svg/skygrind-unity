# NIGHT60 builder V: B2 kill55's marks near-black

No Editor, Player, build or reel was run (builder rule). Every number below was read from NIGHT59's existing a258 frames
(`Logs/feel/n59-b2/{g1on,g1off,g1old}/a258/shot-N55-K-*.jpg`, MAIN repo) or from a planted copy of one.

## 1. The cause (commit 1: `Tools/night/n60-v-mark.py`, output `Logs/n60/V/mark-cause.txt` and `mark-cause-on-old.txt`)

Command: `python -I Tools/night/n60-v-mark.py <MAIN repo>/Logs/feel/n59-b2` (and `... g1on g1old`).

**The enemy x-ray paints over the marks.** `EnemyViews.MakeXray` draws each husk a second time with `SKYGRIND/Enemy`'s
`SRPDefaultUnlit` pass. That pass uses ZTest Greater, ZWrite Off and normal blending, at queue Transparent + `EnemyXrayQueueOffset`
(3005). It is on whenever the player's setting `xray` is on, and the default is true. The pass draws the body wherever something
NEARER than the body hides it. The marks write depth in front of the body: NIGHT58 lifts them 0.045 m and NIGHT59's `Kill59Mark`
pulls them 0.3 m. They draw in the opaque queue (Geometry+10), so the x-ray comes after them and lays `aim.xray.color` #FF4A1C at
`aim.xray.alpha` 0.35 (more toward the silhouette) over every mark pixel.

| | colour | V |
|---|---|---|
| the hole, its own colour (UnityTuning.Kill55HoleHex) | #070303 | 0.03 |
| the hole under the x-ray, model (linear blend, a 0.35, before the post) | #A02B0F | 0.63 |
| the frame's mark pixels at +0.5 s (g1on darker than g1off by > 0.12: 6.06 % of the rect) | median #860802 | 0.53 |
| the same at the flash end (5.89 % of the rect; the flash's grey still in the mix) | median #501811 | 0.31 |
| the reel's line at +0.5 s, the changed pixels | H 3 S 0.99 | 0.55 |

- **The model and the frame agree.** The model's composite has V 0.63 before the post. The frame shows V 0.53, which matches the
  tone map (linear 0.35 -> about 0.27). The hue is the x-ray's red, not the mark's. The post itself keeps black black: the frame's
  1st percentile V is 0.008.
- **This explains NIGHT59's probe.** The mark's centre read the same V on and off (0.53 / 0.52). In the off frame, the hit's impact
  cube stands in front of the body, so the x-ray tints it the same dark red. In my read of the crops it is a dark red square on the
  crosshair.
- **This explains NIGHT57 G3's read** ("the hole drawn in a dark red near the body's own hue"). The precedent is foes48's shield,
  which draws after the x-ray for the same reason (`UnityTuning.Shield48QueueAfterXray`).

**Planted cases** (L-111: at the colour the draw really shows). Each takes the off flash-end frame and plants the mark in
n59-g-probe.py's shape (the hole disc and the crack's strips), then runs the reel's MAIN replica on it.

| planted | MAIN replica | verdict |
|---|---|---|
| (a) at the x-ray composite (today's draw) | 0.10 % (on / old pair: 0.52 %) | RED, as the reel's 0.46 % |
| (b) at the marks' own colours (a draw after the x-ray) | 15.74 % (on / old pair: 16.86 %) | GREEN, >= 5 % |
| (c) unplanted | 0.10 % (0.52 %) | RED |

The meter prints `meter OK` only when (a) is RED, (b) is GREEN and (c) is RED.

## 2. The change (commit 2 the test, commit 3 the fix)

- **Commit 2, the test.** `N60MarkTests.Marks_DrawAfterTheEnemyXray_InTheirOwnColour` builds a mark through
  `Kill55Damage.Params59`, which is now public with nothing else changed. It wants:
  - the mark's render queue past Transparent + `EnemyXrayQueueOffset` (3005);
  - its `_BaseColor` equal to the linear colour it was given, with alpha 1.

  On commit 2 the material keeps the shader's Geometry+10 (2010), so the test is RED.
- **Commit 3, the fix.**
  - `Params59` sets the queue to Transparent + `EnemyXrayQueueOffset` + `Kill60MarkQueueAfterXray` (3006). That is a NEW Unity-only
    number in `UnityTuning.N60V.cs`, with its comment. The marks then draw after the x-ray, keep their depth pull and their own
    colours, and the x-ray's tint under them is overwritten.
  - The shader file is unchanged except for one comment.
  - It applies only with kill55 on (`design.n55.kill55`, OFF in the repo).
  - `-skygrindNoKill55` (no marks) and `-skygrindOldMarks59` (NIGHT58's body copies, opaque queue) keep their meanings.
  - a258's input line now prints `mark queue <q> vs the enemy x-ray's 3005 (x-ray setting on|off; NIGHT60 B2)`, so the reel shows
    the input fired.
- **No colour number changed.** The colours were already near-black. The draw order was the gap.

## 3. Not checked, and what the lead measures

- **The MAIN after the change: not checked** (no Player).
- **Reels.** Build the Player with kill55 ON in the build's copy only, then run
  `bash Tools/night/n60-reels.sh <tag> <spec> ...` with these specs:
  - `v2on,n52-foundry,a258`
  - `v2off,n52-foundry,a258,-skygrindNoKill55`
  - `v2old,n52-foundry,a258,-skygrindOldMarks59`
  - the ladder: `v2kon,n52-foundry,a355` / `v2koff,n52-foundry,a355,-skygrindNoKill55`
- **What each must print:**
  - **v2on:**
    - `mark queue 3006 vs the enemy x-ray's 3005 (x-ray setting on`
    - MARKS AT THE FLASH END >= 5 %. The planted model predicts about 15 %, if nothing nearer than the 0.3 m pull covers the hole.
    - `ladder ok`
  - **v2old:** `mark queue 2010`, and its MAIN about 0.5 % (NIGHT59's).
  - **v2off:** `marks laid 0` and its MAIN about 0.5 %.
  - **a355 v2kon:** `ladder ok`.
  - **The probe at the mark's centre** (`python -I Tools/night/n59-g-probe.py Logs/feel/<tag> v2on v2off <x> <y>`): the on
    side's flash-end V well under the off side's. NIGHT59 read 0.53 / 0.52.
- **The meter on the new frames** (`python -I Tools/night/n60-v-mark.py Logs/feel/<tag> v2on v2off <x> <y>`): section 2's mark
  pixels should now read V < 0.12 for most of them. NIGHT59's read 12.9 %.
- **The guard.** Run `python -I Tools/night/n56-colour.py Logs/feel/<tag>/v2on/a258` and the same on `v2off/a258` and `v2old/a258`.
  The TOTAL lines' cover / sky / sat must be equal within the off-vs-old noise. The marks sit inside the enemy rects, which the
  world region excludes.
- **verbs37 x5 equal.** This is a render-only change with no Core state; the fingerprints are EQUAL.
- **Risks the critic should look at:**
  - **Hit sparks behind the mark.** Transparent effects in queues up to 3005 (the hit sparks and KillConfirm at Transparent + 0..2)
    write no depth. Where one stands in front of a mark, the mark now draws over it, so the sparks near the hole sit behind it for
    their few frames. Frames to read: shot-N55-K-hit-next.jpg and shot-N55-K-flash-end.jpg, on vs old.
  - **Anything nearer than the 0.3 m pull still covers the mark.** This is unchanged from NIGHT59.

# B7 rim56's trims

No Editor, Player, build or reel was run. Every number below is from NIGHT58's r1 a357 frames
(`Logs/feel/n58-r1/{v1on,v1off}-<level>/a357`, MAIN repo, rim56 on vs off at the same poses) or a planted copy of them.

## B7.1 The cause (commit 1: `Tools/night/n60-v-rimpath.py`, output `Logs/n60/V/rimpath-n58-r1.txt`)

Command: `python -I Tools/night/n60-v-rimpath.py <MAIN repo>/Logs/feel/n58-r1`. It reproduces the r1 MAIN exactly with
n56-colour.py's own `measure()`: bodies over the env p90 off 78 / 99 (78.8 %), on 75 / 98 (76.5 %).

**Cause 1: rim56's accent dim never reaches the string course, and the string course is most of the accent on screen.**
- **The two draw paths.**
  - decor54's pieces (wall-top rims, floor trims, door frames, roof strips, prop collars) are coloured by `Decor54.Recolour`. That
    calls `Colour56.Clamp`, which calls `Rim58.AccentMax`, so rim56's x0.45 lands on them.
  - decor55's string course ("Decor55 band", 2.70-3.02 m up every wall face; foundry: 318 segments, 1089 m) is coloured by
    `Decor55.Recolour`. That is its own copy and never calls `Colour56.Clamp`, so neither colour57's max channel nor rim58's dim
    reaches it. Its material is set once at the map's build (`m.SetColor("_BaseColor", c)` on a neon copy).
- **On the frame (foundry).**
  - 85-97 % of the accent-hue pixels per shot are the same on and off, within 0.03.
  - My map of them (scratch, not committed) shows the long violet bands along the walls the same on and off.
  - Only thin pieces (a door lintel, the wall-top rim lines) went darker; a5-entry's darker ones read V on / off 0.44.
  - The env p90 V is the same on and off: 0.729 / 0.729 on a3-entry, for example.
- **Planted case** (the ON frames with the unreached accent pixels scaled by k, measured by `measure()`):

  | | x0.45 | x0.60 | x0.75 |
  |---|---|---|---|
  | foundry | 14 / 14 | 14 / 14 | 13 / 14 (on: 6 / 14) |
  | kiln | 30 / 36 | 30 / 36 | 30 / 36 (on: 26 / 36) |
  | **total** | **87 / 98 (88.8 %)** | **87 / 98 (88.8 %)** | **86 / 98 (87.8 %)** |

  Iceline, span and spire don't move. The V1 cover check holds: 38 / 40 at every k, as unplanted.

**Cause 2: rim56's lit gain washes out the brightest bodies, which then fail on saturation.**
- **Iceline (NIGHT57's gain 2.21, NIGHT56's 1.50).** Off 17 / 17 → on 13 / 17. The env p90 stays (V 0.475 / 0.471,
  S 0.653). The g2 bodies go from S 0.72-0.86 off to S 0.53-0.63 on, at V 0.83-0.93, which is under the env's S 0.65.
- **Kiln.** kb3-entry: S 0.70 / 0.67 off → 0.66 / 0.64 on. kb3c-entry: 0.65 → 0.57.
- **Not addressed here.** This is the lit gain's own number. No frame here can plant a different gain, so it is the next step: see
  section B7.3.

**The probe gap (B3's ac964137).** a357 runs the campaign waves, which include swarmers and spitters. NIGHT59's foundry
a5-entry spitter had no rect. Until ac964137 their bodies were not counted in "enemies in view". Their warm pixels sat in the world
but inside the role band (350-35), so the env p90 hardly saw them; their acid sac, if outside the role hues, did count as env.
- **The count changes.** On a build with ac964137, every spitter or swarmer in view with at least 15 % warm pixels joins the MAIN's
  count.
- **Both counts can't be printed from the telemetry.** It records no enemy kind, so the counts with and without them can only come
  from two builds: one with ac964137 and one without.

## B7.2 The change (commit 2 the test, commit 3 the fix)

- **Commit 2, the test.** `N60RimTests` pins two things:
  - with the row on, the band colour's largest channel is x `UnityTuning.Rim60BandScale` (0.45), with hue, saturation and alpha
    kept;
  - with the row off, it is exactly as it came.

  `Rim60.N60V.cs` is pre-placed with a `Band` that returns its input, so the row-on test is RED there.
- **Commit 3, the fix.**
  - `Rim60.Band` scales the colour while rim56 is on, using `Rim60.On`: rim56's row, not `-skygrindNoRim56`, not `-skygrindOff58`,
    not `-skygrindOff60`.
  - Decor55's band build takes it in one hook line: `Rim60.Band(Recolour(...), Rim60.On(cfg))`.
  - The `[decor55]` Player log line now ends `rim56's accent dim on the band x0.45 (NIGHT60 B7)`, or `... no (NIGHT60 B7)`. That
    is the input proof.
  - The number is new: `Rim60BandScale` 0.45, in `UnityTuning.N60V.cs` with its comment. It equals NIGHT58's Rim58AccentScale,
    which was measured on the frame at 0.44 on the pieces it reached.
  - With the row off (the repo), nothing changes.
- **Cause 2 (the lit gain) is not changed.**

## B7.3 Not checked, and what the lead measures

- **The MAIN after the change: not checked** (no Player).
- **Build:** rim56 ON in the build's copy. Run `bash Tools/night/n60-reels.sh <tag> <spec> ...` with these sides, named so
  n60-v-rimpath.py reads them:
  - `v1on-foundry,n52-foundry,a357` and `v1off-foundry,n52-foundry,a357,-skygrindNoRim56`
  - the same pair for `iceline` (n52-iceline), `kiln` (n57-kiln), `span` (n52-span) and `spire` (n52-spire)
- **Input proof:** `grep "\[decor55\]" Logs/player/n47-reel-<tag>-v1on-foundry.log` must end `rim56's accent dim on the band x0.45
  (NIGHT60 B7)`. The v1off logs must end `no (NIGHT60 B7)`.
- **MAIN** (as written): `python -I Tools/night/n56-colour.py Logs/feel/<tag>/v1on-<level>/a357 --kinds entry,mid` for each of the
  five levels. Sum the TOTAL lines' "body over env p90" across them; the MAIN wants >= 90 %.
  - The same in one command: `python -I Tools/night/n60-v-rimpath.py Logs/feel/<tag>`. Its TOTAL line is n56-colour's count, with
    on and off side by side.
  - The planted estimate is about 87 / 98 (88.8 %). Iceline's lit-gain washout (cause 2) is still in it. So the MAIN may still be
    short, and by how much is the number to read.
- **Reached check:** the same `n60-v-rimpath.py` lines. The "unreached (same on / off)" accent share should fall from NIGHT58's
  85-97 % on foundry, and the env p90 V on should sit under off on foundry and kiln.
- **Checks to keep** (B7 names no GUARD):
  - V1 cover from the same n56-colour lines. NIGHT58 on: 38 / 40.
  - The greyscale guard: `python -I Tools/night/n57-v-cvd.py grey Logs/feel/<tag>/v1on-<level>/a357`.
  - verbs37 x5 equal. This change touches no sim, and the fingerprints are EQUAL.
- **Next step for cause 2.** NIGHT57's lit gain (Rim57Gain 1.47 x Rim56LitGain 1.5 = 2.21) washes the brightest bodies under the
  env's S p90 on iceline (S 0.53-0.63) and kiln. A lower gain while rim56 is on needs a Player scan. It is a Unity-only constant, so
  each value needs a build; n60-build-num.cjs does not reach it.

## B7.4 Attempt 2: the lit gain (cause 2)

**The estimate.** Commit 1: `Tools/night/n60-v-rimgain.py`, outputs `Logs/n60/V/rimgain-n58-r1.txt` (no band fix) and
`rimgain-n58-r1-band45.txt` (with B7's band fix planted).

How the estimate works:
- It uses NIGHT58's r1 a357 pairs at the same pose: rim56 on (lit gain 2.205) vs off (skin52's own, 1.0).
- The planted frame for a gain g is the ON frame, with the pixels inside each enemy rect mixed in linear light between the off and on
  render at t = (g - 1) / (2.205 - 1). The band fix is planted as in B7.1.
- It is measured by n56-colour.py's own `measure()`.
- **Planted check first:** g 2.205 reproduces the ON frames' own count on every shot: GREEN.
- **Limits.** t moves all of rim56's body change together (the lit gain, the shade and the near rim). The post chain between the two
  renders is not modelled. It is uncalibrated (L-108: tonight's B3 halo simulation over-read by 11-14 points).

Bodies over the env p90 (the MAIN's measure), all five levels:

| lit gain | with the band fix x0.45 | without the band fix |
|---|---|---|
| 2.205 (now) | 87 / 98 (88.8 %) | 75 / 98 (76.5 %) |
| 1.8 | 88 / 99 (88.9 %) | 75 / 99 |
| 1.7 | 89 / 99 (89.9 %) | |
| 1.6 | 89 / 99 (89.9 %) | 75 / 99 |
| 1.5 | 89 / 99 (89.9 %) | 74 / 99 |
| 1.4 | 89 / 99 (89.9 %) | |
| **1.3** | **91 / 99 (91.9 %)** | 76 / 99 |
| 1.0 | 91 / 98 (92.9 %) | 77 / 98 |

Notes on the table:
- **Per level at 1.3, with the band fix:** foundry 13 / 14, iceline 17 / 17, kiln 32 / 37, span 14 / 16, spire 15 / 15.
- **Iceline g2's four bodies** all pass from g 1.3. At 2.205 two of them sit at S 0.53-0.63.
- **Lowering the gain alone** (no band fix) only trades iceline for foundry (6 -> 3 / 14).
- **What still fails at 1.3:** 7 bodies, on saturation, mostly on kiln and span. 5 of the failures remain even at 1.0, where the env's
  S p90 sits at 0.69-0.70.
- V1 cover stays 38 / 40 at every gain.

**The value:** 1.3, with 1.0 as the fallback (no lit gain from rim56: 91 / 98 estimated, foundry 12 / 14).

**The change.**
- **Commit 2, the test.** `N60RimTests.LitGain_RowOn_UnderNight57s_RowOff_AsItCame` is RED there: the placeholder returns 2.205.
- **Commit 3, the fix.**
  - `Rim60.LitGain` returns `UnityTuning.Rim60LitGain` (1.3) while `Rim60.On`, under the same switches as the band.
  - `Rim56.LateUpdate` takes it in one hook line. The shadow side (Rim57Gain x Rim56Shade*) and the near rim are unchanged.
  - The reels' "the lit side's gain written" now reads 1.30 with the row on: the input proof.
  - **Fallback:** set `Rim60LitGain` to 1.0f and rebuild.

**What the lead runs.** The same reels as B7.3 (a357, five levels, `v1on-<lvl>` / `v1off-<lvl>` with `-skygrindNoRim56`, rim56 ON in
the build's copy), and the same commands.
- The a357 / a358 lines must read `the lit side's gain written 1.30`.
- `n60-v-rimpath.py`'s TOTAL is the MAIN.
- **My estimate of the MAIN with both fixes: 91 / 99 (91.9 %). It is uncalibrated.** The band estimate alone said 88.8 %; the lead's
  Player number calibrates it.

## B7.5 Attempt 3: the guard "world over the tells" (iceline g2-entry)

**The Player result** (the lead, n60-b5 at gain 1.3 with the band fix):
- MAIN met: 128 / 141 (90.8 %).
- The guard "world over the tells" went 3 / 40 OFF → 4 / 40 ON.

**The cause** (`Tools/night/n56-colour.py` on `Logs/feel/n60-b5/v1{on,off}-iceline/a357`; `Tools/night/n60-v-tells.py`, outputs
`Logs/n60/V/tells-iceline-g2-entry-b{4,5}.txt`):
- **The frame that flips is iceline g2-entry.**
- **The world side does not move.** The world's S p99.5 is 0.65 on and off, held at iceline's cap (cap57's 0.65).
- **The tells move.** n56-colour's tells are the enemy rects' pixels in the danger (5-35) or parry (38-70) hue with S >= 0.35 and
  V >= 0.25, judged by their median S:
  - off 0.706 → on 0.646, under the world's 0.654;
  - 65-68 % of them are bright pixels in the 38-70 band (V 0.996), whose S falls 0.675 → 0.604 on;
  - the median hue moves 42.6 → 50.3.
- **It is the lit gain.** On n60-b4 (the same band fix, gain 2.205) the same frame reads tells S 0.502 (H 60). So the tells' S falls
  with the gain (2.205: 0.502, 1.3: 0.646, off: 0.706), and the lit gain whitens the bodies' and tells' bright parts.
- **The band dim plays no part.** It is outside the enemy rects, and the world S p99.5 is the same on and off.
- **Kiln's three FAILs (kb2-mid, kb2c-entry, kb2c-mid) fail with rim56 off too.** Tells 0.55-0.63 against the world's 0.70, at every
  gain.

**The estimate** (`Tools/night/n60-v-rimgain2.py`, output `Logs/n60/V/rimgain2-n60-b4-b5.txt`):
- **Method.** Two Player builds differ only in the lit gain. For a gain g, the enemy rects' pixels move along the gain axis in linear
  light. The planted check (g 1.3 reproduces n60-b5's own 128 / 141 and 4 / 40) is GREEN.
- **Below 1.3 it extrapolates.** It is uncalibrated.

| gain | bodies over the env p90 | world over the tells | g2-entry tells' S vs world 0.654 |
|---|---|---|---|
| 1.3 | 128 / 141 (90.8 %) | 4 / 40 | 0.646 FAIL |
| 1.2 | 131 / 141 (92.9 %) | 3 / 40 | 0.669 |
| 1.1 | 130 / 141 (92.2 %) | 3 / 40 | 0.694 |
| 1.0 | 130 / 141 (92.2 %) | 3 / 40 | 0.720 |

**The value:** 1.1. It is the margin on the guard (0.04) with the MAIN at 92.2 %. Fallback: 1.0. Not 1.2: its margin on the guard is
only 0.015, inside what an extrapolation can miss.

**The change.**
- **Commit 2, the test.** `N60RimTests.LitGain_KeepsTheTellsOverTheWorldsCap` wants the gain at or under 1.1 and not under skin52's
  1.0. It is RED at 1.3.
- **Commit 3, the fix.** `UnityTuning.Rim60LitGain` goes 1.3 → 1.1 in `UnityTuning.N60V.cs`, with the attempt-3 comment. Nothing else
  changes.

**What the lead runs.** The same a357 reels as B7.3 on a build with this commit and rim56 ON in its copy.
- The reel lines must read `the lit side's gain written 1.10`.
- **MAIN:** `n60-v-rimpath.py`'s TOTAL. My estimate is 130 / 141, and it is uncalibrated.
- **Guard:** n56-colour.py's "V1 GUARD count: world out-saturates the tells (sat FAIL)" summed over the five levels, against the OFF
  side's 3 / 40.

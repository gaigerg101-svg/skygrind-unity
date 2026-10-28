# THE HARVEST (FORGE72 D-2): built view rows that are OFF, that answer GG's words (off-rows.txt, off-why.txt; 2026-10-27)

Each row's `enabled_why` says it is OFF "until" a gate (a critic, a SPOT of 41,472 px, GG's keep) that never ran. None says it failed.
Judge them on the H train: one build (n72-h0) after the shared commit, then the pairs below. Every row is OFF in the repo's config, so a
pair is the ON side with `-skygrindRows72 <row>:1+<row>:1...` (D-3) against the OFF side with the plain config, in the same build.
Same-run pairs (`-skygrindSameRun63`) work only for the seven rows in FrameRows (colours63, goal64, voice71, words71, wyrm71, cyan71,
kicks71); the others are two sides with the reel's fixed frames (L-156).

Pass rule D-1: a SPOT names a change, the lead's look finds no new defect, and the H train's ONE combined guard run (every passing row
`:1` together) holds. Then each row flips, one commit each, by about 1:30.

A row whose pair shows a defect goes to its owner's list as "fix, then ON", and the owner starts that rework only then. A row with no
visible change stays OFF with the pair's number.

Precedence: harvest rows flip first, as built. New rows build on top: body72 on wyrm71, btext72 on voice71 / words71 / bossText55,
spring72 on hitzone47 / models41.

| group | rows (design path) | GG's words it answers | pair (level, scenario) | owner if it needs work |
|---|---|---|---|---|
| H1 colour | n63.colours63, n64.goal64, n56.yellow56, n55.decor55 | "colors ... I just haven't seen it" (2026-10-26); "effects are inconsistent it all needs to be unified" (2026-10-19) | tour65 on n52-spire and n52-foundry; boss63 n52-spire | C |
| H2 light | n61.fixture61, n55.ssao55 | lights "placed and floating" (2026-10-11); "flickers" (2026-10-26) | tour65 n52-iceline; ssao55 also on L's flicker meter | L |
| H3 exits | n63.exit63 (alone: exitFrame61 / lamp60 / doors38 are its older variants, OFF) | doors "not visible now and you just run through it and it's like dark" (2026-10-13); "OK now you have to go up" (2026-10-26) | a157 n57-kiln with -skygrindClearShots60 (the clear shots) | N |
| H4 boss | n71.wyrm71, n71.voice71, n71.words71, n71.cyan71, n55.bossText55 | "boss animations are too rigid" (2026-10-22); "hard to understand what to do" (2026-10-12) | boss63 n57-kiln and n52-spire | B |
| H5 hits | n47.hitzone47, n41.models41, n71.kicks71 | "hits and kills need to feel more satifying" (2026-10-14); "kills not crunchy" (2026-10-05) | the kill phases kill12 / headshot12 (`-skygrindReelOnly`; memory reel-kill-coverage) | B |
| H6 finish | n55.finish55 (finale47 overlaps it and stays OFF) | the boss kill (GG 2026-10-20: JoJo-style colour turns) | bosskill72 (B fills the pre-placed phase) | B |
| H7 camera | n55.step55 only (cam72 supersedes eye63 and fov55: they drive the same eye height and FOV; red team 12) | movement "cleaner, more fluid, and less choppy" (2026-10-20) | not a still: verbs37 telemetry, read by N with cam72 | N |
| H8 small | n42.reticle42, n40.pump40, n41.select41 | the reticle "spin faster" (2026-10-14); the shotgun's animation "sped up" (NIGHT40) | verbs37 frames (reticle, pump); the level select frame (select41) | B (reticle, pump), N (select41) |

Not harvested tonight: n38.doors38 / n61.exitFrame61 / n60.lamp60 (superseded by exit63); n63.press63 (kiln presses; C may take it as a
leftover); n71.wash71 (0.005 over its bar; L leftover); n71.glow71 (RED); n45.look45, n46.shift46 (old, unmeasured; leftovers); every sim
row (a tape bit; not a player switch); n55.speed55 (slow motion on kills: GG asked for slow motion only on ride + zoom); n38.juice38 /
n41.juice41 (a full-screen flicker: GG asked for fewer flickers).

Rows older than NIGHT46 (reticle42, pump40, select41, models41) do not go through NightRows.ViewOn (its night check starts at 46).
`-skygrindRows72` sets the loaded config's value, so it covers them only if they read the config after Rows72 runs (in Awake, right after
the load). The lead greps each one's read site before its pair. Any row it cannot switch at run time needs a cross-build pair (two builds,
the same pose) or stays OFF tonight.

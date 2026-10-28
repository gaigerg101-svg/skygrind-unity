# Wave 41 (NIGHT41): the finish night (2026-10-14 09:06-15:04, port/stage3, box 6 h, freeze df379618 at 13:28)

Harvested by NIGHT45 C1 (2026-10-17) from NIGHT41-LOG.md, NIGHT41-REPORT.md, Research/2026-10-14-wave41/GG-REQUEST.md and INVENTORY.md.

**Ask:** "ok lets make a prompt related to things that were not done or still partial, expand upon that much more and think of other
things to imporve and make better" (2026-10-14 ~08:40; GG had not played n40-r5). The forge's INVENTORY: 28 items (A1-A14, B1-B4,
C1-C10): NIGHT40's SHORTs, the old OFF rows (doors38 / juice38 / stag38), NIGHT39's leftovers, and new items tied to GG's earlier words.

**Shipped (13 / 18 rows ON, build n41-r7):** juice41On, doors41On, pump41On (the NIGHT38 / NIGHT40 rows turned on through n41 rows, the old
rows untouched), entrance41, bars41 (J2 / T2); look41 (K2: room light and the sun by theme); sweep41, brawlLook41 (B8); tokens41,
heavy41, kiln41 (E6); spire41, foundry41 (R5); the level 6 plan (L6: beat sheet THE MIRROR, greybox n41-finale, RIVAL.md). Shared commit
3fa2c92a (N41Blocks 8 bits, tape 24, N41BaseId, pre-placed hooks). Per-frame reel telemetry (T2).

**Numbers (source NIGHT41-LOG.md):** time above two wind-ups 3.14 % -> 0 % (tokens41); the CHARGER's hold 0.19 -> 0.34 s + 0.14 s hitstop
(heavy41); L5 verbs 26/30 -> 30/30 (spire41); THE GANTRY b2-g1 "+156 % -> +32 % ride chain" (foundry41); THE LOOM's sweep: stay 10/10
51.6 s, casual 58.6 s; level 2 sun B/R 0.795 -> 1.390; aim41's harness hits / s THE LOOM 0.63 -> 3.17, THE ICEBREAKER 0.43 -> 1.89;
the suite Core 1095 / 1095, Game A-L 527 / 528; the smoke PASS to round 11; bench 3 runs (indicative, beside the Editor) p99 ~10.7-11.1 ms,
models41 no measurable cost.

**SHORT:** aim41 OFF (no Player frame of a pad lock on a weak point); select41 OFF (built, LevelSelect29 PASS, no frame looked at);
models41 OFF (the husk's arm unreadable at 12 m); A11 verbs row 13 RED on n41-kiln (a reel limit: the Player lands a ground jump
11.60 -> 9.43 m/s where the harness passes; T-61); the suite's Game M / N past run_tests' 600 s, P-Z not run; THE WICK's fight reel RED
(T-59, since NIGHT37); finale41 OFF by D-3 (GG reacts to the plan first).

**Prompt shape:** NIGHT41.md 208 lines; G0 0:00-0:30, rounds 1-3, round 4 in the Player 4:20-5:00, the freeze and a test hour; caps 7
builds / 50 reels (used 7 / 49); eight builders (T2, J2, K2, B8, E6, P3, R5, L6); forge experiment: an INVENTORY of every carried item
with a fate each (28 fates in the log).

**What went wrong in the run:** build n41-r6 failed twice: a builder's new .cs was picked 4 s after the gates' compile, the Editor answered
`up_to_date`, and the Player's script layout differed (fixed by ImportAsset(ForceUpdate) + RequestScriptCompilation); the suite's stop at
14:43 did not take (pkill matched nothing) and its calls overlapped the bench; the Game M / N batches timed out.

**Lessons added or confirmed:** L-067 (never pick while the gates' compile runs; force the import of a new file before a build); L-068
(run_tests' filter is ONE substring: batch by class prefix, keep each batch under 600 s); L-062 confirmed (the four n41 ids registered with
placeholder files: no reel booted SURVIVAL); L-005 confirmed (the bench beside the Editor's leftover test run: indicative only).

**Reaction (GG played n41-r7, 2026-10-14 ~18:35; Research/2026-10-14-wave42/GG-REQUEST.md):** level 1 "waallride section ... awful and
relaly difficult and not fun"; "pressing a near a wall you fly bcak so fast"; DRONE parry "relaly tight ... you miss depsite felling like
you hit it"; "shoutgun jumping is too difficult"; "way too much instruction at the tsrat of the level"; "hits and kills need to feel more
satifying" (OWNER.md, w41 row).

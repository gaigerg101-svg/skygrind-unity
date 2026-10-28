# Wave 40 (NIGHT40): the build night (2026-10-13 23:11 to 2026-10-14 ~03:30, port/stage3, box 6 h, closed at ~4:20 elapsed)

Harvested by NIGHT45 C1 (2026-10-17) from NIGHT40-LOG.md, NIGHT40-REPORT.md, Research/2026-10-14-wave40/GG-REQUEST.md.

**Ask:** GG's 22 asks after playing n39-r4 (Research/2026-10-14-wave40/GG-REQUEST.md, 2026-10-13 ~22:00): a delay before a level's first
wave; level 3's boost explained thoroughly; the pistol's spin finished before the next shot; a shotgun charge; longer charges and one
curved bar; the shotgun's fire animation faster; the STITCHER out of level 4; THE LOOM with no-ride zones, smarter, in its map; the
infinite wall ride; no snapping onto stairs; level 2 overhauled for the gauntlet (no pistol at its start); the second boss "completely
redesigned into a brawler-style encounter"; new enemy types with more detailed models (GG's words outranked CLAUDE.md rule 7, logged).

**Shipped (13 / 14 rows ON, freeze aa424094, build n40-r5):** start40, kiln40, rods40, cut40 (S1); charge40 (W1); arc40 (H1); ride40 (R2);
iceline40 (L2: n40-iceline from a beat sheet); brawl40 (B2: THE ICEBREAKER); loom40 (B4); crown40 (B5); foes40 + models40 (E3: the DRONE,
WARDEN and CHARGER on campaign levels, models for seven kinds). Shared commit 4fa44abe (N40Blocks 10 bits, tape 23).

**Numbers (before -> after, source NIGHT40-LOG.md / REPORT):** spawn -> first possible hit 0.00 -> 5.6-5.8 s (levels 2, 4); a held ride lost
0.547 m in 1.2 s then fell -> 0.000 m over 3 s; rides on short / low faces 480 / 606 -> 0 / 220; THE ICEBREAKER beaten in the Player at
47.5 s (bot p50 48.6 s, casual 63.8 s); THE LOOM pulled 43.7 m out of o3 -> 0, beaten in the Player at 35.9 s with 0 nails; THE CROWN's
ring in the core 9.9 % -> 0 %, falls 20 -> 5 hp; rods in the play volume 5-14 per level -> 0; blind silhouette naming 7 / 7 twice; the
crosshair quarter turn 0.60 -> 0.34 s (harness only); verbs 26/26, 20/20, 24/25, 30/30, 26/30 (L5 rows 17/19/22/23: no grapple node);
the smoke PASS 204.1 s.

**SHORT:** pump40 OFF (built, the pump 0.75 -> 0.42 s, never on a frame: no telemetry trigger for the volley); the ice look (a pale grey
floor, not ice); THE LOOM's floor-sweep cue (no sweep attack exists); E3's two-attacker cap (up to 5 committed, > 2 for 3.5 % of time);
the batch suite stopped by Claude Code for low memory (no results); the n40 ids missing from design.maps.list at G0 (three r1 reels booted
SURVIVAL, fixed 01:00); Lesson25TextTests' 7-word prompt added by B4 f7878a50 (T-62, found by NIGHT41's suite).

**Prompt shape:** NIGHT40.md 305 lines; G0 shared commit, then rounds 1-4, round 4 in the Player, a test hour, the close; caps 7 builds /
38 reels (used 6 / 28); nine builders (S1, W1, H1, R2, L2, B2, B4, B5, E3); every row off until its frame; forge experiment: one builder
per GG ask cluster, rows judged ON only on a Player frame.

**What went wrong in the run:** the permission classifier blocked builders' `git merge port/stage3` (the lead cherry-picks from then on);
the maps.list miss at G0; the batch suite killed for low memory; disk 18 GB at G0 until GG freed space (128 GB by 23:34).

**Lessons added or confirmed:** L-065 (register a new level id everywhere the campaign loads levels), L-066 (builders cannot merge: the
lead cherry-picks), L-056 confirmed (the verbs reel's ride wall vs ride40's intent rule was the reel's pick, not a lost verb).

**Reaction:** none to n40-r5 itself; GG's next message (2026-10-14 ~08:40, Research/2026-10-14-wave41/GG-REQUEST.md) asked for a prompt
on "things that were not done or still partial" without having played it.

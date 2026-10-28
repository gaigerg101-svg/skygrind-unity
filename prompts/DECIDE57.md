# DECIDE57 (NIGHT57: SHIP AND DEEPEN; decisions with their reasons and the alternative rejected; the lead adds D-7 on)

## D-1 The shipping rule (GG 2026-10-21: "1. you decide")
A row ships ON when its MAIN number beats the A0 baseline, its GUARD numbers are no worse than the baseline beyond their band, nothing is
lost, the gates are EQUAL off, and the batched critic confirms the evidence (NIGHT57.md section 0). GG's play is the veto (a PLAY note and
an off switch per ON row). Reason: NIGHT55-56 shipped 2 of ~38 entries under "every critic line passes"; near misses on non-guard lines
(2.4 px for one frame) held whole features off (GAPS.md 1.2). Rejected: keep the old rule (the shelf grows, GG sees nothing new); ship
everything built (no guard against a regression).

## D-2 Enemy attack behaviour may change (GG: "2. yes")
press57 and tokens56 change how EXISTING enemies attack (a delayed aimed attack with a ground tell; aim vs the player's speed; who may
attack). Looks, models and new enemy kinds stay GG's (CLAUDE.md rule 7). Rejected: a new enemy kind for the pressure (GG's design).

## D-3 Level 3 rebuilt from scratch (GG: "3. yes")
n57-kiln: 5-6 combat rooms from A-9 cards; links, the start and k5 THE KILN'S HEART may be kept; THE KILN WYRM's AI unchanged (GG
2026-10-13 "keep the worm AI the same"). Behind the row kiln57 (off = n52-kiln). Ships with >= 3 new rooms that pass; fewer: OFF, SHORT.
Rejected: patch kb2 / kb2c / kb3 again (GG 2026-10-09 "complete arena overhaul ... from scratch"; T-131: patches break mirrors).

## D-4 The Editor (GG: "4. will do")
GG restarts it before the run. If it does not answer at A0: never kill it; batch-mode builds and suite from SKYGRIND-unity-batch.

## D-5 The judge is calibrated before it judges taste (P0-calib)
The GG stand-in critic is scored on GG's real verdicts (n35 vs n37; n41-r7 / n42-r4's complaints). Failing it, the gate critic checks
evidence and eye-only lines only. Reason: research/design.md lesson 12 (bots and vision models are not fun meters; 43.4 % glitch recall);
GAPS.md section 4. Rejected: keep the uncalibrated persona as the taste judge.

## D-6 Three builders, 4-5 h
L (level 3), G (enemy pressure, kills, G's shelf), V (colour budget, V's shelf); the lead owns P0, builds, reels, critics. Reason: the
usage limit stopped NIGHT55 (five builders) at 2 h 28 min and NIGHT56 (three) at 3 h 12 min; the checkpoint tag after r1 is the fallback.
Rejected: a fourth builder for the tools (the Game M hang and the rooms table are the lead's leftovers).

## D-7 No n57-r0 (09:5x, the lead)
NIGHT57.md's clock names an n57-r0 build with every shelf row on. Skipped: every shelf row but wyrm55 is being changed by G or V tonight,
so r0's numbers would be stale by r1; wyrm55 (a sim row nobody changes) is measured at HEAD in the harness (40 seeds + seeds 41-60 on /
off for the band). The row-on side of every other shelf row is measured on r1 / r2 after its fix. Rejected: r0 anyway (a Player run and
a critic batch spent on versions about to change).

## D-8 P0-calib's frames (09:5x, the lead)
The frames are stills of different rooms (n35-r2's 9 level frames vs n37-r1's level36 walk; no shared scenario exists between n35 and n37),
and GG judged by playing. The test is weaker than planned and its report says so; a PASS still means the judge's taste agrees with GG's on
the one paired verdict we have. Part B's match words were fixed before the run (Logs/n57/calib-key.json).

## D-9 wyrm55 ON (09:43, the lead; P0-shelf, section 0's rule)
MAIN deaths per fight 1.70 -> 0.80 (seeds 1-40), 1.50 -> 0.65 (41-60); the off sets differ by 0.20: beyond noise. GUARDS killed, median,
p90 all better (NIGHT57-LOG 09:43). The fastest kill (27.0 s) is a near miss under GG's 1-2 min, not a named guard: reported, not
blocking. ON in the r1 build copy, then in the repo's config at the freeze if the r1 critic confirms the evidence. Rejected: hold it for
the fastest-kill line (the median, which GG plays, sits inside his band).

## D-10 tokens56 ON (10:20, the lead; G1, section 0's rule; G's commit 14430e73)
MAIN attack starts inside the sim cone 44.7 -> 85.0 % pooled (bot; seeds 1-10 and 1-20 both 85.0; off seed band 0.5 points). GUARDS:
deaths 21 / 9 / 8 / 0 / 0 -> 13 / 7 / 8 / 0 / 0 (better); clear time at 20 seeds +7.0 / +1.7 / +3.3 / +1.7 / +1.1 % (all <= +10 %). At 10
seeds iceline +11.3 % and spire +10.3 %, inside the baseline's own seeds 1-10 vs 11-20 spread (8.6 % / 16.8 %): the 20-seed reading is
the guard (more data, the same rule). ON in the r1 copy; the critic checks the logs (rows forced on: tokens56). Rejected: block on the
10-seed reading (the baseline's own noise is larger than the gap).

## D-11 wick55 ON, pending the r2 critic (11:2x; G4, G's ab3f973b, Logs/n57/g4-wick-*.txt)
MAIN damage dealt in the windows 6 -> 57 % on both seed sets (bot). GUARD deaths: wick54's off fight 2 (seeds 1-40) and 9 (41-80); on
7 and 4: pooled 11 / 11 over 80 seeds (NIGHT56's "2 -> 7" read seeds 1-40 only, the low end of the off side's own spread). Median 45.5 /
47.6 -> 57.5 / 54.6 s, inside the bot band 30-60 s. T-59 (the Player pilot cannot climb the stack) stays a pilot thread. Rejected: hold it
on NIGHT56's 40-seed reading.

## D-12 rank56 RETIRED (11:2x; G4)
MAIN DULL share 50.5 / 67.8 / 25.9 / 13.2 / 12.0 -> 50.6 / 67.4 / 26.0 / 13.2 / 12.0 % (bot): unmoved; the ride-kill refund pays 0.00 at
every rank. The row stays off and goes to docs/RETIRED.md at the close. T-137 (its RowOff test failing in the Editor) explained: the
N36TestBaseline SetUpFixture forces N36Blocks off for every Game test, so heal36 was off; G's fixed test passes 3/3 under `--fixture36`.
Rejected: keep it for GG to try (it pays nothing to try).

## D-14 brief55 ON, pending the critic's check of the checker change (11:4x)
Tonight's reels ran on keyboard (no pad connected: "device":"kbm"; NIGHT56's on a pad). n55k-teach.cjs counted "LEFT CLICK" as two words
and not as a glyph; V's 6c2054ef joins the key names (one glyph = one word) and changes nothing else (the 6-word, 360-frame, basic-jump
and 0 px rules unchanged). On n57-r1 kiln with the fixed checker: brief55 ON 6 lines, 0 bad, 0 overlap frames (GREEN); every n55+ row
off (vkoff55, NIGHT54's text) 3 bad (an 8-word line, a line without a glyph, the basic jump after level 1) and 99 overlap frames (42.1 px):
the checker still fails real misses. MAIN beats the baseline; GUARD 1 -> 0 frames (-skygrindOff57 vs on). Because a builder changed the
checker its own row is read by, the r2 critic reviews the checker diff. Rejected: keep the old checker (it fails every keyboard line).

## D-15 r3 now, with everything; the missing r2 reels run on it (12:1x)
The disk ran out during r2's reels (8.7 GB; n47-reels.sh refuses under 10 GB), so kill55, grade55, the colour r2 fix and verbs37 on
kiln never ran on r2. Rather than re-reel r2, r3 is built at once with every builder commit picked (G's idle fix and a257 framing too),
and the full reel set runs on it. L's slot C (a new kb3) joins the freeze build only if it passes its room checks and the harness guards;
the freeze's own verbs37 x5 and a157 are its Player evidence. Rejected: re-reel r2 then build r3 at 12:40 (two reel sets do not fit before
the 13:04 freeze; the disk would run out again). Prune after every reel batch (n57-prune.sh).

## D-16 verbs37's staging waits for the level's first award room (12:5x, the lead)
r3's verbs37 on n57-kiln: RED rows 40 / 41 and rows 25-27 PASS -> SKIP. Builder L traced it (Research/2026-10-21-wave57/L.md): Open35
stages every ground verb at the first room whose spawn has a 14 m lane; n57's new kb2 has a 21 m lane, so it staged there, BEFORE kb2b's
KESTREL award (design.n40.kiln40.awards), and rows 40 / 41 punched with the gauntlet in hand (Combat.cs drops that punch by design). A
player walks kb1 -> kb1b -> kb2 -> kb2b and holds the KESTREL before any later room. Fix: Open35 skips rooms before the level's first
award room (SurvivalSim.FirstAwardRoom57); with no award room, as before; n52-kiln stages at kb2b as before (its kb2 has no 14 m lane).
The freeze build's verbs37 on n57-kiln and n52-kiln is the evidence, and the critic reviews this diff. Rejected: move kb2's spawn to fit
the reel (bends the room to the tool); ship kiln57 OFF without trying (the night's headline, diagnosed as the tool's).

## D-17 the r3 verdicts (13:1x; Research/2026-10-21-wave57/gate-r3.md)
- grade55 ON: THE CROWN tells kept 495 / 502 (0.99; NIGHT56 0.68); THE WICK GREEN, ride cue 13 % cyan. b2on's RED items (walls near
  danger at the turn 0.39, the off run 0.31; spire's walls barely change) are not guards and the baseline fails them too (L-100).
- colour56 ON: MAIN 36 / 36 (off 22 / 36); greyscale 25 / 36 (off 25 / 35): held after r2's luminance fix; world over tells 3 / 36;
  deuteranopia 36 / 36.
- wick55 ON (bot): MAIN windows 6 -> 57 %; deaths 11 / 11 pooled. The critic's caveat, kept in the report: part of the jump is the row
  counting the bare core as a window; on one measure the chip share falls 51 -> 43-44 % (still better).
- kill55 OFF: MARKS NEXT FRAME 0.00 % on and off (the next frame is 97 % white flash); the a354 runs never fire kill55, so its jitter
  guard did not test the row. rim56 OFF (enemies above the world's p90 on 75 %, foundry 29 %; target 90 %). yellow56 OFF (a real non-
  parry yellow frame on foundry a5).
Rejected: kill55 ON on the chain ladder alone (its MAIN is marks AND the ladder).

## D-13 colour56 stays OFF after r1 (11:1x)
MAIN (measured fallback) PASS 36 / 36 vs 22 / 36; world over the tells 7 / 36 (NIGHT56's 32 / 36); deuteranopia 4 / 4; but the greyscale
guard falls 25 / 35 (colour56 off) -> 18 / 36: the keys lift the world's values above the enemies and hazards. V gets one attempt at r2.

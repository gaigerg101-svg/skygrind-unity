# NIGHT35 proof (the fundamentals night)

Freeze: 2be19aef (port/stage3), build `Builds/n35-r4` (every design.n35 row ON). RED = the same build with `-skygrindOff35`.
Reels: 22 of 22 invocations used (n34-r5 1, n35-r1 4, n35-r2 5, n35-r3 7, n35-r4 5). Builds 4 of 6. Every number below was printed by a
harness or a Player; "not checked" where it was not.

## Gates at the freeze
- fingerprint EQUAL; `--off35` EQUAL; `--off34` EQUAL; CoreHarness 39 vector sets 0 mismatches, 22 / 22 trajectories exact; dn-check 0;
  Editor compile 0 errors. Every Core commit tonight passed the same gates (builders' and the lead's lines in each commit message).
- Batch suite at the freeze (2be19aef, 21 min): **1943 / 1945**. Failed, not diagnosed: `BotZeroAllocTests.BotBrainStep_ChaseEngageEvadeRecover_AllocatesNothing` (3 GC samples over 1800 steps, want 0) and `MapChoiceTests.EveryListedMap_LoadsAndPlays600Steps` (over its 180 s timeout). Both are load-sensitive and ran with ~4 GB of 64 free; whether the new n35 levels slow MapChoice is not checked. NIGHT34's freeze suite was 1936 / 1936.
- Smoke on n35-r4: NOT OBTAINED. The Player quit about 10 s after boot with no `[smoke]` line and hung in cleanup (closed by its PID);
  not diagnosed.
- LevelSelect29 on n35-r4: not run (time). The six-level order is proven by `campaign35` ALL GREEN (harness) and the levels35 reel
  booting n35-finale as place 6 of 6 (n35-r2); n35-foundry and n35-kiln booted in the n35-r4 verbs35 runs, n35-iceline was not booted
  in a Player tonight.

## verbs35 (docs/VERBS.md; one Player per map)
- L1 n35-foundry (n35-r4): **23 / 23 PASS** (every performed row PASS; SKIPs with their reason).
- L3 n35-kiln (n35-r4): **25 / 25 PASS**.
- L4 n33-span: rows 1-12 PASS on n35-r4 (recovered from the Player's own log: the lead closed this Player by mistake while reading
  memory use, so its total line is missing); rows 13-37 from n35-r3 (before the freeze fixes): 13 PASS, 20 PASS, 24-26 PASS, 28 PASS,
  29 PASS, 34 PASS; **17 / 18 / 19 / 22 / 23 RED on n35-r3** (the grapple never attached in the Player: the cause was found and fixed
  in 9bb3322e, harness `n35move pull` "press x3" RED before / PASS after, but NOT re-run in a Player at the freeze: the reel cap);
  27 / 30 RED on n35-r3 (fixed in c87b574c; L1 / L3 row 27 PASS on n35-r4 with 0 normal shots before the charged shot; row 30, the
  THUNDERCLAP, not re-run in a Player).

## Cards
- **charge35** (W): Player n35-r1 GREEN: a hold fires 0 shots, charge35 Start / Full / Fire 1 / 1 / 1; the hands' glow 0.50 -> 1.00,
  the tip 11.0 -> 25.6 mm; the rail CHARGING -> CHARGED. RED (-skygrindOff35): 1 shot while held, no charge35 events. Harness
  n35charge GREEN (KESTREL x9.00, THUNDERCLAP x2.22, a tap = one shot). Frames looked at.
- **pickup35** (W): Player n35-r3 PASS: THUNDERCLAP label 39 px near / 23 px at 12 m; MEND +40. RED: NIGHT34's cubes. Frames looked at
  (r2: the pillar hid the gun and the vial; r3: the gun on a floor ring with its own light, the cyan vial).
- **teach35** (T): Player n35-r1 GREEN: 5 / 5 level-1 lines in their rooms, 24 px, 5.02 s each, the second run on the same profile 0.
  RED: no line. Frame looked at ("HOLD ZR to CHARGE. Release to fire.").
- **wallride35** (M): harness: a 30 m wall 9.95 m / 1.20 s -> 28.95 m / 2.79 s; three-wall chain end 16.2 -> 20.7 m/s; corners 98 /
  90 / 360 deg. Player n35-r4 verbs35 row 9: longest single ride 1.89 s (L1, 25 m wall), 1.56 s (L3), 1.64 s (L4) vs NIGHT34's 1.2 s
  cap; row 11 jump-off 17.20 m/s vs 11.71 on the wall. A person still climbs a shaft with tapped wall jumps (5.46 m, 6 WallJumps, same
  as before).
- **grapple35** (M): harness: a wall tap refused -> arrives 0.70 s at 44 m/s, 0.00 m off the line; a held press no longer swings.
  Player: the hook bites and pulls (M's grapple35 reel: the arrival event fired; the path was blocked by geometry at the reel's spot);
  the latch-cancel bug the r3 Player found is fixed but not re-run in a Player. Owned from level 4: L1 / L3 rows 17-23 PASS (not owned).
- **look35 / lights35 / tv35 / ridelook35** (V): Player n35-r4 look35 PASS, all four GREEN: equal neighbours 10.3 % (RED) -> 61.9 %
  (480 lines), grain 0.012; the lit room's mid luminance 0.188 (r1) -> 0.313 (r4, the key pool); the HUD kicked 1.3 px on a hit, a
  glitch on the hit; the ride + zoom frame focus 1.00, blue 52.8 %. Frames looked at (RED / GREEN on r1, GREEN on r3 / r4).
- **campaign6** (A): harness campaign35 ALL GREEN (six levels, L6 starts with all five guns, the grapple owned from L4); walk 10 / 10
  on n35-foundry / iceline / kiln / finale with every row ON; levels35 PASS on n35-finale (n35-r2: 3 ride rooms, 9 frames).
- **N-11 bosses** (lead, harness bosses33, 5 seeds): --off35 all in band (lantern 20.3, stag 19.6, wyrm 20.5, loom 21.0, crown 18.3 s);
  n35 on: lantern 13.9 s (under the 15 s floor: the KESTREL's charge returns in 2.5 s), crown 16.0, the others unchanged; every camp
  test PASS. DECIDE35 D-15. The Player boss reel was not run (cap).

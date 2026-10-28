# Wave 42 (NIGHT42): the feel night (2026-10-14 19:00 to 2026-10-15 00:35, port/stage3, box 6 h, freeze a523f935 at 23:53)

Harvested by NIGHT45 C1 (2026-10-17) from NIGHT42-LOG.md, NIGHT42-REPORT.md, Research/2026-10-14-wave42/GG-REQUEST.md, DECIDE42.md.

**Ask:** GG's fifteen asks G1-G15 after playing n41-r7 (2026-10-14 ~18:35): THE GANTRY's wall-ride section "awful", the fling near a wall,
the gauntlet 0.7 s faster, the DRONE-orb parry, the shotgun boost easier, less instruction at a level's start, the reticle, hits and
kills more satisfying, held fire auto-starting the next charge, a context skill "because we are forgetting things", arena elements "not
just decoration", the style meter and health bar, THE ICEBREAKER harder, a bigger gauntlet hit.

**Shipped (10 rows ON, build n42-r4):** wallfling42 (M1); foundry42 (L1: THE GANTRY forgiving in n42-foundry); gauntlet42 (2.5 -> 1.8 s),
charge42 (W1); impact42 + kills42 (F1); meter42, style42, health42 (U1); brawl42 (B1: SWEEP + LEAP, phase 2 double sweep); the
night-context skill (X1, installed at the close). Shared commit af1b680a (N42Blocks 9 bits, tape 25, N42BaseId).

**Numbers (source NIGHT42-LOG.md close table):** a wall press's back push 8.00 -> 0.00 m/s; a person riding each GANTRY wall 40.4 -> 100 %;
held shots after a charge 7 -> 1; THUNDERCLAP hit freeze 58 -> 85 ms, gauntlet on a husk 83 -> 143 ms, a tank 123 -> 183 ms; the meter's
wrong fill 44 -> 0 and false FULL 301 -> 0 over 754 frames; THE ICEBREAKER skilled 48.6 -> 55.9 s (46.2 with gauntlet42), no HP added;
the suite Game 998 / 999 and Core 1095 / 1095 (after dd37246a): the first whole suite to finish since NIGHT40; the smoke PASS 174 s;
verbs 26/26, 20/20, 24/25 (row 13: T-61), 30/30, 30/30.

**SHORT:** boost42 OFF (8 m climbs 3.8 -> 22.3 % at lift 11, but level 3's G1 only +10 %: the kb1b roof; D-9); parry42 OFF (the swing
inside 2.5 m 11/16 -> 16/16, the punch window 217 -> 558 ms, never on a frame; D-10); reticle42 OFF (spin at +0.2 s 0.23 -> 0.25: not
shown faster); teach42 OFF (2 lines in 60 s with and without it; D-11); arena42 OFF (the grinder and cover placed in b1, never on a
frame; D-12); THE WICK's fight reel RED (T-59); the wave-memory harvest for 40-42 not run (time).

**Prompt shape:** NIGHT42.md 198 lines; G0 with a read-only CODE-FACTS sweep before the shared commit; integrations ~22:00 and ~23:00, round
4 in the Player, a test hour; cap 50 reels (used 38 + the smoke), 4 builds; eight builders (M1, L1, W1, F1, U1, B1, K1, X1); forge
experiment: count OFF rows whose only reason is "unseen" (2 vs NIGHT41's 3).

**What went wrong in the run:** the session's connection dropped 19:15-20:23; the Editor crashed on its own at 20:01 (D3D12 device
removed, in Play mode) and was reopened at ~20:40, so the shared commit landed at 20:47 (planned 19:30); RAM 1.6 of 63 GB free at G0
(OneDrive.Sync 14.9 GB, not ours); a real pad plugged in (L-046).

**Lessons added or confirmed:** L-069 (a fact sweep before the shared commit pays: CODE-FACTS named the causes the cards fixed); L-068
confirmed (the split batches: every batch under 600 s, M / N 209 / 209); L-001 confirmed (five rows stayed OFF because no frame showed
them); L-070 (a row that adds a sim field must add it to RunSnapshot's Fields or Skipped: RunSnapshotTests caught wallfling42's).

**Reaction (GG played n42-r4 to level 3, 2026-10-16 ~16:00; Research/2026-10-16-wave43/GG-REQUEST.md G1-G30):** "treat this as a
challenge to make a major leap in quality rather than a collection of tiny patches"; wall riding "clunky and buggy ... Fix this at the
movement-system level rather than designing around a broken mechanic"; "all enemies ... destroyed but the exit door still fails to
open"; the UI "too generic and cheap" (OWNER.md, 2026-10-16 row).

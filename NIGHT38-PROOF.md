# NIGHT38 proof (written in NIGHT39 as C1 row 11, from NIGHT38-LOG.md's records; nothing re-measured here)

Freeze: `01c5c032` (port/stage3), build `Builds/n38-r1/SKYGRIND.exe`, tag `u3-n38` at `07dfb1de`. Rows ON at the freeze: foundry38,
span38, spire38. Everything else in design.n38 OFF.

## Sim parity (every integration, the lead)
- Golden (CoreHarness): 39 vector sets / 22 replays, 0 mismatches at the shared commit (4bd19270) and after M's Mover change (ride38).
- Fingerprint (SimHarness): EQUAL plain and `--off38` after every merge (E, M, H, O2, S2, C2, F, K3, E's rounds 2-4, C2 round 4).
- Editor compile: clean (`compilationFailed` false) after every merge; one compile (13:00) left NullReference entries in the Editor console
  (SurvivalGame.Update / Look.C, `cfg` null, play mode stopped): not reproduced in the Player; open as NIGHT39 C1 row 10.

## The gates (E's checker, `n38 gates` / `n38 limits`)
| level | result at the close | notes |
|---|---|---|
| L1 n38-foundry | 2/2 PASS (+32 %, +26 %) | 3.6 m ceilings, 37 m gaps; walk 3/3 |
| L2 n38-iceline | G2 +20 %, G3 +15 % PASS; G1 INFO | G1 a skate gap the box world cannot judge |
| L3 n38-kiln | 0/4 RED | the PULSE blast and the dash jump beat every gate |
| L4 n38-span | 3/3 PASS | grapple; walk 10/10 |
| L5 n38-spire | 4/4 PASS with room limits | `pistolOnly` measured to remove the blast (`n38 limits`); walk v1-v10 5/5 |
| L6 n38-finale | 0/4 | the exam level owns every verb (GG's call) |

## Player checks (n38-r1)
- Smoke (`-skygrindSmoke`): DONE, all steps passed, 0 NullReferenceException.
- Level boots with `-skygrindMap n38-*` alone: landed on the SURVIVAL attract (0 errors) - NOT a level boot. The campaign boot needs a reel
  (`-skygrindReelOnly`): NIGHT39 G0 (a).
- Verbs: not run.

## Tests
- Batch EditMode suite at 01c5c032: 1982 / 1985. Lesson25Tests.TheOrder_FollowsTheBlocks fixed in the close (NIGHT38 forced off in the
  order pins; 10/10). Open: AutopilotTests.PerStep_AllocatesNothing (1 allocation over 17,748 fight steps), MapChoiceTests timeout (180 s).
- Builders' EditMode tests run by the lead: Ride38Tests 5/5, Hud38Tests 6/6, Juice38Tests 5/5, N38SharedTests 3/3.

## Not proven
Frames of doors38, hud38, juice38, stag38, loom38, crown38 and the level rooms; ride38 on (a5 deaths 2 -> 9 with it on); a person's play.

# NIGHT37 proof (freeze e6902b00, Player build Builds/n37-r7, 2026-10-12 23:35-00:10)

Every line below was measured on the freeze commit or read from the n37-r7 Player logs (Logs/feel/n37-r7/summary.txt,
Logs/player/n37-reel-n37-r7-*.log). "Not checked" says so.

## Gates at the freeze (e6902b00)
- dn-check 0 errors; Editor compile 0 errors.
- fingerprint EQUAL; `--off37` EQUAL; `--off36` EQUAL.
- golden: 39 vector sets (48094 records), 0 mismatches; 22 trajectory replays, 22 exact.
- Every design.n37 row ON: boss37, wick37, stag37, loom37, crown37. FingerprintBaseline.N37OnPath stays false.
- Harness:
  - `boss37 kit` GREEN (11 rules).
  - `stag37 all` GREEN: band 10/10, p50 20.9 s, p90 24.8 s, 0.0 % still damage; `stag37 casual` 10/10 killed, p50 27.0 s, max 37.0 s.
  - `loom37 all` GREEN: band 10/10, p50 21.6 s, p90 21.8 s, 0.3 % still; camp: sever at 4.00 / 3.00 s, web wall in leg 2.
  - `crown37 all` GREEN: band 10/10, p50 21.93 s, p90 23.02 s, 0.0 % still; every attack check PASS; every leg met a cut and a blade
    ring in all 10 seeds.
  - `wick37 all` RED only on the strict per-gap rule (a look-level leap can catch the next panel from 2.4 m of 8 m, +0.26 m; looking up:
    7.2 m, +2.45 m), accepted as the person-first trade-off. Climb 10/10 to the cap (median 21.0 s); band (D-7) PASS: 10/10 killed,
    total p50 34.3 s, from the cap p50 15.4 s, 0 % still.
  - NIGHT36 intact: `n36tap`, `n36core`, `boss36 all` GREEN.
- Batch suite: pre-freeze at f37ebc4a 1964 / 1966 (both failures fixed in e6902b00: Lesson25Tests 10/10, Lesson25TextTests 6/6 in the
  Editor). The freeze suite at e6902b00 was started at 23:50 and NOT completed: Claude Code stopped its shell for critically low system
  memory, and the lead closed its batch Unity (pid 159272, identified by command line and start time; free memory 4.5 GB -> 17 GB) ~5 min
  into a ~12 min run rather than risk the live Editor. Not re-run (the notice said not to). The freeze differs from the pre-freeze suite's
  f37ebc4a by W's reel / view commits and the lead's two test fixes (checked in the Editor).

## Player reels on n37-r7 (8 invocations; 24 / 26 for the night). 0 NullReference, 0 exceptions in all 8 Player logs.
| reel | result | measured |
|---|---|---|
| wick37 (level 1) | space GREEN; fight GREEN | CAMPAIGN run; the begin moved the feet 0.000 m; ledge 1 at 4.8 s; panels 3-14 ridden; leg 3 (the cap) reached; tells flare 1.40, ember 1.20, dive 1.00, slam 1.00 s; 3 parries, 3 cracks, 1 pierce, 1 window; NOT killed in the Player run (the reel's fight line passes on leg 3 + the tells) |
| stag37 (level 2) | RED | killed in 24.2 s of fight; tells shard 1.00, chunk 1.20, kick back 1.00, charge 1.50 s; 2 leg punches, 2 chunk parries, 2 windows, the charge parried; gauntlet in hand; RED only because no floe dropped in this run (floes dropped 0; n37-r4 showed 5 with the RED and the down frames) |
| loom37 (level 4) | GREEN | CAMPAIGN run; tears 6 (pull 4, LT 2, reel puppets 0); first window 5.2 s; leg 3; killed at 21.7 s; lash 1.20 s, spool 1.20 s; cuts 10; the canyon frames in room o3 |
| crown37 (level 5) | GREEN | CAMPAIGN run; tells bladeRing 1.20, beamCut 1.40, gemShard 1.00, lowFire 1.00 s; passes 6 (the reel's passes are puppets: the look, not the timing); 2 windows; legs 2-3; the finisher; killed; blade 1 hit, shard 1 hit taken |
| verbs37 n37-foundry (L1) | 27 / 28 | RED row 43: the reel's walk from a8 never entered the stack a9 (the wick37 reel proves the walk-in: feet moved 0.000 m) |
| verbs37 n37-iceline (L2) | 30 / 30 | |
| verbs37 n37-span (L4) | 32 / 32 | |
| verbs37 n37-spire (L5) | 32 / 33 | RED row 15 jump pad: 0 JumpPad events with the pad awake (K: the same 0 on n33-spire with --off37: not a NIGHT37 loss; the headless probe launches 3 / 3) |
| levels 3 and 6 (n35-kiln, n35-finale) | boot, 0 NRE | measured on n37-r5 (27/29, 28/31; their REDs judged reel artifacts by K and fixed in f37ebc4a; not re-run on n37-r7) |

## No verb lost (PRINCIPLES 1)
K judged every RED verbs37 row from n37-r5 with a headless probe and NIGHT36's levels as the control (`verbs37probe <level> --off37`):
each was the check's setup, none a lost verb (NIGHT37-LOG.md "Round 3"). After the fixes: L2 30/30 and L4 32/32 in the Player.

## Frames the lead looked at (n37-r1..r7)
- THE CROWN: arena-bridge / arena-orbit (cyan-edged fragments orbiting the spire core), the window (the ring all cyan), the pass (the
  TORCH lit cyan mid-pass over orbiting pieces), tell-gemShard (orange in r1 -> yellow after C's fix).
- THE STAG: the slipstream streak behind the running stag, the antler charge's YELLOW antlers, the window (on its side, cyan bands), the
  floes frame (gauntlet in hand, a RED floe strip) on r4.
- THE LOOM: the reveal (the body slung on six legs between towers, cyan knees), the tear (mid-air over the canyon between cyan ride
  walls, nails in a glowing knee, legs hanging), the kill (the body falling), the rehearsal text "NAIL THE KNEE. G PULL AWAY.".
- THE WICK: space-up (the spiral of cyan-edged panels, red-orange hot seams), r6 the beige drum with a hidden core -> r7 the flare
  lighting the whole shaft red and the stall's core blazing cyan-white; the r7 window frame faces the shaft wall, not the boss (the
  reel's aim).

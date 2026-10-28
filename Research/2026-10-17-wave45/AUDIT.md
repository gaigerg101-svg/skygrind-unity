# NIGHT45 forge audit (2026-10-17): the arenas, the fluff, the repeated mistakes (read-only; the lead's agent)

## A. The arenas (27 fight rooms; each piece's `why` read from Design/levels/<id>.json)
Cross-cutting: (1) A-6 is uniform: Look43View puts the same two cyan pylons beside every room's exit (Look43View.cs:226-258,
Look43Math.cs:132); only k5's KILN MOUTH is a room's own landmark; a4 and the boss rooms get none. (2) A-5 half built: Lava30 kills
enemies in every level, but only a3, a5, kb2, kb3 are designed around it; levels 2, 4, 5 have no fight-room hazard. (3) The top-level `_why`
of all five campaign files still holds NIGHT15's DRYDOCK hub text.

| level | room | size | roles | A-1 reasons | verdict |
|---|---|---|---|---|---|
| 1 | a2 KESTREL | 14x20x7 | fod | 1/1 | OK (teaching) |
| 1 | a3 RIDE | 16x26x9 | fod | 4/5 | OK (lava both ways) |
| 1 | a4 PARRY | 8x19x6 | rng | 1/1 | OK, no landmark |
| 1 | a5 CRUCIBLE | 20x24x10 | fod / rng+fod | 5/6 | OK-old (same 30 s problem as b1) |
| 1 | **b1 THE YARD** | 36x40x12 | fod / rng+fod | 5/8 | **WEAK** (3 reasonless arena42 pieces, OFF row; a5's problem) |
| 1 | a8 PIERCE | 14x22x9 | fod | 7/7 | REBUILT NIGHT43 |
| 1 | a9 THE STACK | 28x28x50 | boss | in code | OK |
| 2 | g1 TANK ALONE | 22x26x9 | hvy | 2/2 | OK (bare) |
| 2 | g2 THE SHIELD | 24x28x9 | shd+fod | 7/7 | OK |
| 2 | g3 THE SWARM | 26x28x9 | fod (8 swarmers) | 4/4 | WEAK-ish (two blocks) |
| 2 | g4 THE REACH | 24x30x10 | fly | 6/6 | OK (the canal and bridge: level 2's best identity) |
| 2 | g5 THE COOLDOWN | 26x30x9 | fod / shd+fly | 5/5 | OK |
| 2 | g6 THE ARMOURY | 14x18x6 | fod | 2/2 | OK (reward room) |
| 2 | **g7 THE MIX** | 30x32x10 | hvy+rng / fod+rng | 5/5 | **WEAK** (16 bodies, 5 generic pieces, no hazard) |
| 2 | g8 THE REHEARSAL | 24x30x9 | chg | 4/4 | OK (pillars stun the charger) |
| 2 | x9 THE BRAWL | 38x38x14 | boss | 1/41 lines | OK-old |
| 3 | kb2 POUR HALL | 36x44x16 | fod / fly / chg | 7/9 | OK (pours both ways) |
| 3 | kb2c THE ASH BED | 28x30x12 | bur | 5/5 | REBUILT NIGHT43 |
| 3 | kb3 CASTING FLOOR | 40x46x18 | rng / bur / fly | 6/7 | OK |
| 3 | **k5 THE RING** | 26x26x12 | boss | **0/76 props** | **WEAK** (geometry SHORT T-65) |
| 4 | p1 HOOK YARD | 32x36x12 | fod / rng | 3/3 | WEAK |
| 4 | **p3 KNOT YARD** | 40x40x14 | fod / rng | 3/3 | **WEAK** (1,600 m2, 3 pieces) |
| 4 | **p5 SPOOL HALL** | 36x36x14 | fod / rng | 3/3 | **WEAK** (p3's template) |
| 4 | o3 THE CANYON | 44x140x30 | boss | 7/7 | OK |
| 5 | **v2 THE NAVE** | 30x38x14 | fod / rng / "heavy" | 7/7 | **WEAK** (template; "heavy" a plain husk there) |
| 5 | v6 THE CHOIR | 34x42x16 | fod / heavy+rng+fly | 8/8 | WEAK-ish (v2's layout) |
| 5 | v11 THE SPIRE | 68x68x12 | boss | 9/9 | OK |
ROOMS.md has no rows for levels 4-5; levels 1 / 3 rows still say "not checked" in places.

## B. Fluff and old stuff (J5)
1. Root untracked: 60 files, 3.2 MB (NIGHT7-44.md, 7 drafts, 7 BRIEFs, PROMPT-FORGE / PROMPT-STOPS, 4 QUEUE, `Logs$TAG-tests.log` /
   `.xml` 1.9 MB stray). night-context reads the newest NIGHT<n>.md from the root; 494 tracked files cite NIGHTnn.md by name (comments). The
   prompts are the only copy: archive (commit into a folder), do not delete; the two Logs$TAG files are safe to delete.
2. Disk: Logs/ 220 GB (mostly Logs/feel's numbered frames, not split by night), Builds/ 34 GB (n15-n34 21 GB measured; n35-n44 ~12 GB est.).
3. Design rows OFF 3+ nights, superseded: balance32 (empty since w32), ride38 (D-9's infinite ride; reading), doors38 / juice38 (shipped
   via doors41On / juice41On), stag38 (THE STAG left the campaign), pump40, boost42 / parry42 (boost43 / parry43 ON). GG's open calls:
   iceline38, finale38, finale41, arena42, teach42, reticle42; unseen: aim41, juice41, select41, models41 (T-60).
4. Level files: none unreferenced (every n29-n42 id is a base of the off-switch chain): NOT fluff. Conditional: level06-level10 (9.4 MB)
   only if GG drops the deep off switches. `Design/levels/n2x-n43/<level>.json` intermediates: 107 MB tracked, written only by regen scripts
   (the plan.json files are what later generators read): the safest big cut, but it needs a regen check (not tonight).
5. Per-night scaffolding: NIGHT42's shared commit 39 files, NIGHT43's 52, NIGHT44's 5 (view only); each sim night adds N4xBlocks,
   SurvivalSim.N4x, FeelReel.N4x, N4xView, N4xSharedTests and edits Curriculum25, the previous SurvivalSim.N4x, FeelReel.N27, N29Fixes,
   PlayerChecks, SurvivalSim(.Run), four Tape files (a format bump each night), FingerprintBaseline, Program.cs, both configs, and every
   older shared test (format number). Totals: 23 N*Blocks, 52 SurvivalSim.N*, 117 FeelReel.N*, 151 Harness.N* files.
6. Tools/night: 147 n29-n37 + 82 n38-n42 scripts; live: n41-build, n41-reels (called by n43 / n44-reels), n42-suite, n43-gates,
   n39-build-rows, n43-rows-on, smoke, reel, integrate; superseded gates / reels / shared-patch / config one-offs.

## C. Repeated mistakes (NIGHT42-44 logs, LESSONS to L-064; w40-w44 never harvested)
- The spend limit / a dropped connection stopped every agent at once: w19, w20, w42, w43.
- The whole suite not run before the freeze: w43, w44 (w42 ran it after the freeze).
- Reel pilot / input-path faults read as feature failures: the parry press (w42, w43 r1), move43 (w43, w44), wick37 (w37-w44), foundry43's
  placement (w43).
- Rows built but never on a frame, left OFF: aim41, models41, select41 (w41); parry42, arena42 (w42); the TORCH (w44).
- dn-check / the build at 0 while the Editor compile broke: w42; a new file needing a forced import: w44.
- A sync commit duplicating rows: w43; harness helper names colliding: w43.
- A new row breaking an old verb or a boss indirectly: fusion43 -> launch36, ride43(e) -> the casual WICK (w43).
- Gates not re-proven when the checker grew: w42 (wallfling42), the kb1b roof (w42, w43).
- The smoke failing for the machine (focus / the pad): w42, w44.
- The Editor left in Play mode or crashing at the start: w42, w43.
- Known reds carried for nights: T-62 (w40-w44).
- Levels patched on copied plans; reasonless pieces: w36, w43.
- Movement rows ON on harness numbers alone: w43 (Player proof still RED w44).

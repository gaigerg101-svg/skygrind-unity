# FORGE72 spike and dry run: facts read or measured in the forge (2026-10-27 08:15-09:30 MST, HEAD 646ccc818)

## S1 The game today, on Player frames (build n71-polish8 = HEAD's code; hard-linked as Builds/n71-forge72)
Command (08:39-08:46, 7 sides, 0 NREs): `bash Tools/night/n71-reels.sh n71-forge72 tf,n52-foundry,tour65 ti,n52-iceline,tour65
tk,n57-kiln,tour65 ts,n52-span,tour65 tp,n52-spire,tour65 bk,n57-kiln,boss63 bp,n52-spire,boss63`. Summary: Logs/feel/n71-forge72/summary47.txt.
Sheets and luma (`python -I Research/2026-10-27-forge72/sheet.py <out.jpg> <shots>`; mean luma 0-255, dark = share of pixels < 30):
| where | mean | dark < 30 |
|---|---|---|
| foundry a2-b1 (6 rooms) | 66.6-82.1 | 7.4-15.0 % |
| iceline g1-g8 (8 rooms) | 58.6-84.8 | 5.5-12.5 % |
| kiln kb2-kb3d (5 rooms) | 72.4-78.8 | 4.9-7.3 % |
| span p1, p3, p5 | 64.1-80.0 | 5.9-12.0 % |
| spire v2, v5, v6 | 57.7-66.3 | **23.0-30.2 %** |
| THE KILN WYRM (boss63: idle, tell, windup, flinch, stagger) | 42.2-66.1 | **8.7-47.1 %** (idle 47.1) |
| THE CROWN (boss63) | 52.2-66.1 | **22.6-43.4 %** |
| GG's own screenshots 2026-10-26 19:25 (before Bright71; 45 and 31 health) | 42.9-46.2 | 33.1-33.6 % |
gg-proxy.md ranks the ten worst things on these frames (dark bosses; one tint per level; effects that hide THE CROWN; box bosses; wrong
boss text; no "go here" line; floating lights; purposeless stripes; blank walls; the red noise border).

## S2 Flicker premise (scout-lighting.md 1a): Stone.shader already follows the Forward+ contract
Assets/Shaders/Stone.shader:37-44, 201-224: `multi_compile _ _CLUSTER_LIGHT_LOOP`, `GetNormalizedScreenSpaceUV(i.positionCS)` in the
fragment (as URP Lit), LIGHT_LOOP_BEGIN / END, the directional loop under USE_CLUSTER_LIGHT_LOOP, multi_compile_fog + MixFog. So the
scout's rank-1 cause is unlikely. Left to measure (L's meter): SSAO noise without TAA (n55.ssao55 exists, OFF), the film grain
(config quality.presets.*.grain 0.035 / 0.045), bloom on thin cyan strips, the CRT pass and its fringe, the red noise border (gg-proxy #10),
coplanar boxes (Bright71.Grow staggers 0.002-0.006 m since 407cf1874). SMAA only on the High preset (PostFx.cs:414-419); TAA is out (the
camera stack; scout-lighting 1g).

## S3 The 1.3 RIDE roof is still in the rebuilt level 1
Design/levels/n52/foundry.plan.json a3 `roof:false` (0e3f95ea5), but Design/levels/n65/foundry.plan.json a3 `roof:true`, and the n65 files
(Design/levels/n65/foundry.json, Assets/StreamingAssets/Design/levels/n65-foundry.json) date from 2026-10-24 01:53. gen-n65-foundry.cjs
reads the n52 plan (`PLAN_IN = "Design/levels/n52/foundry.plan.json"`, line 42). GG's build had foundry65 ON (PLAY71), so the roof he
saw was most likely n65's. Before n65.foundry65 goes ON: regenerate, then check a3 has no roof slab.
- Second trap: an "old" slot takes its row from the n52 room export, and Design/levels/n65/rooms-n52/a3.json still reads `"roof":true`
  (exported at 73b88cbd4, before the roof edit). So `--slots=b1,a8` alone would rebuild the roof. First re-export a3:
  `node Tools/night/gen-n65-foundry.cjs --export a3`. It rewrites rooms-n52/a3.json, and rooms/a3.json only when that is missing or with
  --force, so never pass --force. Then `--check`, then `--run --slots=b1,a8`.
- GG's play build's level files (sha1, first 10): n65-foundry bd59e6a856 = Logs/n71/play/foundry3/ (slots b1, a8); n67-iceline
  348b0b1c76 = Logs/n71/play/iceline4/ (g4); n67-kiln cfc4a7a260 = Logs/n71/play/kiln3/ (kb2, kb3, kb3c, kb3d); n67-spire 0390bf6182
  = Logs/n71/play/spire2/ (v6). The repo's copies are the all-old assemblies (slots65 / slots67 every slot "old"). The gen logs are
  Logs/n71/play/<L>-gen.txt. A regenerated n67 level whose sha1 equals the play build's file is GG's level, proven; foundry differs by
  the roof.

## S4 One build can switch any view row: NightRows.ReelOverride exists, and the config is loaded once
- Assets/Scripts/Core/NightRows.cs:213-221: ViewOn reads ReelOverride first (NIGHT63 D1; set today only by the same-run reel).
- Only one GameConfig.Load in the game (SurvivalGame.Init.cs:25; Night14's bot nav loads its own). GameConfig.Get caches by path
  (GameConfig.cs:64-73), so overrides are applied right after Load, before any read, or the cache is cleared.
- `-skygrindSameRun63 <night.row>` (FeelReel.N63Same.cs) gives same-instant pairs for rows in SameRun63Math.FrameRows (colours63, goal64,
  the n71 rows ...).

## S5 Movement facts (scout-movement.md; PortExport/config.json)
- ground {walkSpeed 8, accel 10, friction 6, stopSpeed 2.5}, air {accel 1.5}: Quake-style (Mover.cs:1365-1384, MovementMath.Accelerate /
  ApplyFriction). jump.doubleJumps 1, doubleJumpMult 0.9: with ultra71's 5.07 m jump, a double jump adds about 4.1 m, so about 9.2 m from
  the floor before wall rides. Room ceilings are about 10 m (the 1.3 RIDE slab was at 10 m).
- verbs37 row 12 (bunny hop) is RED on iceline (2 hops) and kiln with ultra71; span, spire and foundry pass (Logs/feel/n71-polish8,
  n71-polish4). The close's player check needs every side PASS, so the reel window is P0.

## S6 Timings measured (for the trains)
- A Player build: about 1 minute incremental (Logs/n72-fix/build-polish8.txt: 21:12:42 to 21:13:31).
- The smoke: 227.8 s (Logs/player/n71-polish8-smoke.log). A verbs37 side: about 1 minute; tour65 per level 9-55 s; boss63 34-38 s.
- A reel set writes frames before the prune: C: dipped 52 -> 28 GB during kiln's tour, back to 48 GB after. C: free at 08:55: 52 GB.

## S7 Commands run in the forge (2026-10-27)
`bash Tools/night/n71-reels.sh ...` (above); `python -I Research/2026-10-27-forge72/sheet.py ...`; `node Research/2026-10-27-forge72/off-rows.cjs`;
`node Research/2026-10-27-forge72/off-why.cjs`; the n72 tools and their self-tests (redteam.md lists the dry run).

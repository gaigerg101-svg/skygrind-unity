# FORGE74 spike (2026-10-27 21:50-21:58; HEAD e73c3fe1d, the n73-play build = n73-freeze at 5ad6c89bf)

## S1 GG's play-build rooms are already in the repo's n67 level files
sha1 (first 10) of Assets/StreamingAssets/Design/levels/<f>.json in the repo against Builds/n71-play's copy:
| level file | repo | n71-play | same |
|---|---|---|---|
| n65-foundry | 3366a7eab4 | bd59e6a856 | no: R0 regenerates it (b1, a8, the 1.3 roof gone; FORGE72 spike S3) |
| n67-iceline | 348b0b1c76 | 348b0b1c76 | yes (g4) |
| n67-kiln | cfc4a7a260 | cfc4a7a260 | yes (kb2, kb3, kb3c, kb3d) |
| n67-spire | 0390bf6182 | 0390bf6182 | yes (v6) |
The four rows are OFF in Design/unity-config.json (`design.n65.foundry65`, `design.n67.{iceline,kiln,spire}67`, enabled false). The
slots files (slots65.json, n67/<L>/slots67.json) read every slot "old": the rooms are in the level files (NIGHT72-REPORT section 8's ROW
lines, "by list mode"), so a slots file is bookkeeping, not what the level loads.

## S2 verbs37 with each level row ON, on today's code
`cp -al Builds/n73-play Builds/n74-spike`, then
`bash Tools/night/n74-reels.sh n74-spike vi,n67-iceline,verbs37,-skygrindRows72,iceline67:1 vk,n67-kiln,verbs37,-skygrindRows72,kiln67:1
vp,n67-spire,verbs37,-skygrindRows72,spire67:1 vn,n65-foundry,verbs37,-skygrindRows72,foundry65:1` (21:53-21:57, Logs/feel/n74-spike/summary47.txt):
- n67-iceline 29/29 PASS, RED rows []
- n67-kiln 30/30 PASS, RED rows []
- n67-spire 31/31 PASS, RED rows []
- n65-foundry 27/27 PASS, RED rows [] (the repo's file, not GG's; R0 changes it, so LV-foundry65 re-runs this side)
NRE 0 on each side. Not run here: the smoke with the rows ON, the tour65 pairs, the spotter (the night's LV slices).
Note: n74-spike is a forge build (a hard-link copy) and counts toward n74-caps' builds; the night may delete it (the forge's own output).

## S3 The night's tools, run in the forge
- `node Tools/night/n74-tools.cjs` wrote n74-clock.cjs, n74-reels.sh, n74-prune.sh, n74-pair.cjs, n74-slate-set.cjs, n74-row-add.cjs,
  n74-caps.cjs, n74-rows.cjs.
- `node Tools/night/n74-rows.cjs --dry`: "would add design.n74 (6 rows, all OFF) to both copies and 6 FrameRows" (rows74.json; the FrameRows
  anchor is `"core73" };` in SameRun63Math.N63D.cs).
- `node Tools/night/n74-clock.cjs` (before start): printed the stopped n73 clock and C: 427 GB free.
- `node Tools/night/n74-caps.cjs`: "caps n74: builds 0/12, reel sides 0/260, smokes 0/12, spotter runs 0/60".
- `bash Tools/night/n74-reels.sh n74-spike ...`: S2 above; it pruned to every 50th frame at the end.
- `node Tools/night/n72-slate.cjs Research/2026-10-27-forge74/slate-seed.json`: "slate: 20 slices: ... 20 todo", exit 0.
- n72-close-check.cjs takes `--night n74` (line 55) and reads its windows from Logs/clock.json's capMin (start >= cap-60, stop in
  [cap-30, cap)); for an 8:00 cap: the close starts at 7:00 or later, the clock stops 7:30-8:00. Not run with --night n74 (no n74 files yet).

## S4 Not proven in the forge
- The room pipeline's red-first commands (forge71 brief R2) were not run tonight; they ran at NIGHT71 (Logs/n71/).
- weak74's meter needs the weak point's screen box in the reel log: B adds the line; not checked that boss63 prints it today.
- pan74 (L1's reel phase) does not exist yet; L writes it into the FeelReel.N74 partial the shared commit pre-places.

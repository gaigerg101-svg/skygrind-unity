# NIGHT56 gate r5, part A (LEVELS): the evidence for the batched critic (the lead's, 05:1x)

The final gate of the night, split in two (NIGHT56.md section 6: more than 30 images); part B judges gameplay, colour and bosses. Build
`Builds/n56-r5` from port/stage3 b6ab1f10: every builder commit of the night is in it (L's last 2fc22326). Its copy's config has ON:
design.n55 grade55, wick55, step55, decor55, kill55, brief55, clear55, wyrm55; design.n56 rank56, colour56, rim56, yellow56, tokens56. Rooms
are level data: they change on both sides. All paths are under `C:\Users\gaige\dev\SKYGRIND-unity\`. Reel lines:
`Logs/feel/n56-r5/summary47.txt`; full lines `Logs/player/n47-reel-n56-r5-<side>.log`. Earlier builds' evidence: `Logs/feel/n56-r1` .. `n56-r4`.
The r2 verdicts: `Research/2026-10-20-wave56/gate-r2.md` (judge r5 fresh; say where r5 changed a verdict). Rubric and output:
`Research/2026-10-20-wave56/critic-gate.md`. Write `Research/2026-10-20-wave56/gate-r5a.md`. At most 30 images (16 listed).
The slates: `Logs/n56/slate.json`, `Logs/n55/slate.json`. The lead's bot runs on HEAD b6ab1f10 (rows off): `Logs/n56/bot-r5/` (room table +
planted, identity, n46flow x10 with the style / tokens columns), summary `Logs/n56/bot-r5.out`; baselines `Logs/n56/flow-base.out`,
`Logs/n56/identity-base.txt`. Bot numbers are labelled bot. This is each entry's last attempt: a FAIL closes it SHORT.

## A1. L1 the room checker (attempt 2)
- doneWhen: the slate. r2 FAILED line 1: rooms with no foothold printed "beside the foothold" and counted as PASS.
- Evidence: `Logs/n56/bot-r5/room-table.txt`, `Logs/n56/bot-r5/room-planted.txt`; you may run `python -I Tools/night/n56-room.py planted`
  and `... table`. The fix: no foothold -> IN VIEW "not measured"; "beside" = within 10 m of the foothold box (a proposed number: no source
  names one). No frames.

## A2. L2 spawns in view (attempt 2)
- doneWhen: the slate (in view >= 15 / 18; I7 PASS x5; a156 agrees on >= 80 % of foundry / iceline / kiln rooms; stall <= 25 % on foundry /
  iceline / kiln (bot); no clear time worse than flow-base by > 10 %; verbs37 x5 nothing lost). r2 FAILED only the stall line. Note that
  L1's fix now makes the strict in-view count lower (13 rooms have no foothold): read both counts the table prints.
- Evidence: the room table; a156 sides l5f / l5i / l5k / l5s / l5v and `python -I Tools/night/n56-room.py agree
  Logs/player/n47-reel-n56-r5-l5f.log Logs/player/n47-reel-n56-r5-l5i.log Logs/player/n47-reel-n56-r5-l5k.log`; flow `Logs/n56/bot-r5/flow.out`
  SUMMARY lines vs `Logs/n56/flow-base.out`; verbs37 sides vf / vi / vk / vs / vp on r5.
- Frames (6): r5 vs r1 (NIGHT55's rooms) at the entry camera: `Logs/feel/n56-r5/l5f/a156/shot-N29-L56-n52-foundry-a3-entry.png` vs
  `Logs/feel/n56-r1/d1off/a452/shot-N29-K52-n52-foundry-a3-entry.png`; `.../n56-r5/l5i/a156/shot-N29-L56-n52-iceline-g2-entry.png` vs
  `.../n56-r1/d2off/a452/shot-N29-K52-n52-iceline-g2-entry.png`; `.../n56-r5/l5k/a156/shot-N29-L56-n52-kiln-kb2-entry.png` vs
  `.../n56-r1/d3off/a452/shot-N29-K52-n52-kiln-kb2-entry.png`.

## A3. L3 the seven failing rooms (a3, g3, g4, g8, v2, kb2, p5) by ADDING pieces with jobs
- doneWhen: identity FAIL rooms 7 -> <= 2; each added piece carries its job in the plan and a bot contact count > 0; contract 14/14 per room;
  walk bot 10/10 per touched level; the critic's GG-persona line per room.
- Evidence: `Logs/n56/identity-base.txt` (7 FAIL rooms) vs `Logs/n56/bot-r5/identity.txt`; the builder's work per room is in
  `Research/2026-10-20-wave56/L.md` (what was added: p5's pad moved 1 m, g8's THE ASH PIT, kb2's lip in and out; a3 / g3 / g4 / v2 nothing
  built: read why there); contract prints for g8 / p5 are not in the lead's evidence unless `Logs/n53/contract-*.md` holds them (say so).
- Frames (4): the two rooms changed, r5 vs before L3 (r2): `Logs/feel/n56-r5/l5i/a156/shot-N29-L56-n52-iceline-g8-entry.png` vs
  `Logs/feel/n56-r2/l2i/a156/shot-N29-L56-n52-iceline-g8-entry.png`; `Logs/feel/n56-r5/l5s/a156/shot-N29-L56-n52-span-p5-entry.png` vs
  `Logs/feel/n56-r2/l2s/a156/shot-N29-L56-n52-span-p5-entry.png`. Give the GG-persona line for each of the 7 rooms (for the unbuilt ones:
  what GG would still see).

## A4. S1-doors (= NIGHT55 P1-doorDark): doors and links lit
- doneWhen: verbs37 x5 0 frames with world luma std < 0.03 (n55-r-flat.py); door / link frames show >= 3 geometry edges and the next room
  lit (a frame per level); verbs37 nothing lost.
- Evidence: run `python -I Tools/night/n55-r-flat.py Logs/feel/n56-r5/vf/verbs37 Logs/feel/n56-r5/vi/verbs37 Logs/feel/n56-r5/vk/verbs37
  Logs/feel/n56-r5/vs/verbs37 Logs/feel/n56-r5/vp/verbs37` (the kept frames: every 50th) and the same on n56-r1 (the before: 7 flat).
- Frames (2): the flattest r5 frame the meter names on iceline and on spire (open the two it prints; if none is flat, open
  `Logs/feel/n56-r5/vi/verbs37/f0900.jpg` and `Logs/feel/n56-r5/vp/verbs37/f1550.jpg`).

## A5. P2-clear (clear55, attempt 2)
- doneWhen: Logs/n55/slate.json. r2 FAILED (2, 3): the off side still drew the card at alpha 1.00.
- Evidence: sides c55i (on) / c55ioff (off, iceline) and c55foff (off, foundry) on r5: the `a455:` lines (`cards N`); r2's on sides for the
  other levels (`Logs/feel/n56-r2/c55f`, `c55k`, `c55s`, `c55v`).
- Frames (2): `Logs/feel/n56-r5/c55i/a455/` one hold frame, `Logs/feel/n56-r5/c55ioff/a455/` its card frame if one exists (list the folder).

## A6. P0-g7g5 (NIGHT55: iceline g7 / g5 re-cuts, attempt 2)
- doneWhen: Logs/n55/slate.json. r2 FAILED (1, 2, 5): the contract 14/14 not evidenced; the pieces not visible in the frames.
- Evidence: `Logs/n56/contract-g7g5.txt` (the lead's run 02:59: g7 14/14 PASS, g5 14/14 PASS, identity PASS); `Logs/n56/bot-r5/identity.txt`;
  verbs37 vi on r5; the pieces and their jobs: `Research/2026-10-20-wave55/R.md`.
- Frames (2): `Logs/feel/n56-r5/v5i/a356/g7/shot-N56-n52-iceline-g7-mid.png`, `Logs/feel/n56-r5/v5i/a356/g5/shot-N56-n52-iceline-g5-mid.png`.

## A7. P0-mirrors (NIGHT55)
- doneWhen: Logs/n55/slate.json (identity table FAIL items <= 7; every re-cut room adds >= 1 piece with a job; verbs37 on the touched levels
  nothing lost; walk bot 10 / 10; a452 GREEN on each touched level). Evidence: as A3 and the r5 verbs37 / a156 sides. No frames.

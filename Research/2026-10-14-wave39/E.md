# NIGHT39 builder E (E2: the checker on call)

Owner: Tools/SimHarness/Harness.N38.Gates.cs, Design/levels/envelopes38.json, and Design/levels/n38/GATES38.md (the format notes).

## Slice 1: the ice judge (C1 row 4; n38-iceline's G1)

Before this slice the checker's box world had no ice. Now:
- The checker builds the plan's `surfaces` into the test world, and the real sim runs them: Surfaces26 for the friction and Glide30
  for the glide.
- A takeoff on ice adds the pumped skate run-up to every ground try.
- `verb: skate` is judged against a DRY control, the same geometry with the ice taken out.
- New options: `--gate` (tag a solid from the command line) and `--room-limit` (a what-if LIMIT).
- The selftest has a new ice case (expected RED, caught by a dry-control cheat). The selftest is GREEN.
- Iceline's G2 and G3 are unchanged (+20 % / +15 %, best other -11 %).

G1 measured (`--gate box10:skate:14,0,-134:14,0,-164:53:j3-g1`): 24 m gap, 53 m ice lane.

| case | skate margin | best skate reach | best without the skate | verdict |
| --- | --- | --- | --- | --- |
| as built | +85 % | 57.27 m (dashjump+pulse, pumped) | 57.28 m (the same try, dry) | RED: 706 cheats (dash jump, blast) |
| what-if j3 `pistolOnly` (no blast) | +40 % | 37.45 m | 37.47 m (dash jump + double, dry) | RED: 487 cheats (dash jump) |

Why it fails:
- The pumped lane reaches the lip at about 17 m/s (16.88 m/s measured on the 12 m pump stop).
- That is under the dash's own 24 m/s, so the dash jump at the lip is the same jump on ice or off it.
- On flat ice no skate-only gap exists at any length with today's glide numbers (design.n30.glide: decay 0.12 / s, dashAdd 7, softCap
  30; a boost costs 0.5 of a 1.0 bar).
- For DECIDE39's carried row, this measures "G1 is a flow beat" unless GG answers first. A judged skate gate needs a glide change, or
  a downhill ice run (not measured).

## Slice 2: the two-pull grapple (n38-finale x6)

The grapple is now flown with the game's own hook, pull, arrival, 2 charges and 0.8 s gap. Each try fires one or two hooks at the
nodes, the gate solid, the far wall or the roof. Details are in GATES38.md, "The grapple, flown".

- The selftest has 2 new cases and stays GREEN (10 / 10):
  - A planted good gate (40 m, two nodes): PASS, +33 %, 2 hooks.
  - A planted bare gate (no node, no roof): RED. It crosses only at +7 %, under 10 %.
- **x6 (44.5 m):**
  - The grapple crosses with +15 % (node 2 at 8 m, then the end wall behind the deck; the farthest hook 26.86 m).
  - It is RED on 35 cheats: the THUNDERCLAP boost x3 and the dash jump + blast.
  - With a what-if `pistolOnly` on x6: PASS (best other -16 %).
- Finale as a whole: still 0 / 4. x3, x4 and x7 are RED on the boost, the blast and 1 line-of-sight grapple each.
- **n38-span (span38 ON):** all 3 grapple gates are RED on this checker and on the pre-NIGHT39 one (dd013f7b). The cause is the
  same 15 / 25 / 15 `boost x3 p-70 + double@apex` cheats, the ledge sequence NIGHT38 added in round 4. The flown grapple crosses
  each at +31 %. This is a level owner's or lead's call: a ceiling over each gap, or a room LIMIT.
- No change on foundry (+32 / +26 %), spire (+39 / +37 %) or iceline G2 / G3 (+20 / +15 %).

## Round 2

**n38-span fixed (54bf1af1).**
- p2 and p4 now carry the LIMIT pistolOnly (banner "PULL ACROSS").
- `n38 limits n38-span p2` / `p4` now also tries the grapple. Inside each room: blast 0, boost 0, RopeFire 1.
- `n38 gates` is GREEN 3/3: +31 % each, with no other try crossing. It is also GREEN with `--ride38`.
- `n38span walk` is 10/10 seeds.

**The blast + wall-jump family is in the checker.**
- Selftest 11/11 GREEN.
- n38-kiln is RED 0/4. Boost margins +49 / +69 / +55 / +49 %, but the blast + wall jump lands on every gate (24 / 29 / 29 / 10
  tries; the feet reach 11.31 m over the take-off).
- Iceline stays GREEN (+20 / +15 %).

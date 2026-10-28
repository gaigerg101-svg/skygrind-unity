# NIGHT33.md (draft by the NIGHT32 lead, 2026-10-10): leftovers of NIGHT32, in priority order

Base: port/stage3 at the NIGHT32 close (tag u3-n32). Read first: NIGHT32-REPORT.md, NIGHT32-LOG.md, NIGHT32-PROOF.md, DECIDE32.md, Design/levels/DESIGN32.md, Research/2026-10-10-wave32/. GG's playtest of NIGHT32 (FEEL-PLAYTEST.md) comes first and outranks this list.

1. **GG's verdict on the boss length.** NIGHT32 cut every boss to a pilot kill of about 26-39 s in the Player after GG's "bosses still too tanky!" (20:10). If GG still finds them long, the next lever is the pools under design.n32.bosses32 / boss5 (one number each); if too short, the windows. The camp margin is thin at 30 s fights (B: the Stag's centre camper reaches 41 % of the bar in the harness): the anti-camp rules (b1586400 / 2a022c9c) carry it.
2. **The hit-size rule (1.3x the model)** fails by design on the Stag (1.0: a bigger hit volume let a bank-top camper win) and on the Lantern (0.91, in Lantern27.cs); the Loom is 1.25 (1.33 with Tools/night/n32b-loomjoint.cjs, not applied). Decide per boss with the camp table.
3. **Level 5's format gaps (F):** rooms cannot be raised (no floor-height key), every door is cut in the north wall, closed rooms hide the next room. A LEVEL-FORMAT row for a room's floor height and side doors would let THE SPIRE climb for real.
4. **Bot-only issues:** the foundry seed-5 stall in f5 (Walk29Pilot.cs:185 lava rescue only jumps when grounded; A's suggested fix untested); Span30Pilot / Chain31Pilot / FeelReel.N31.Grapple.cs assume travel goes north (S listed the lines); grapple31 E's script aims at walls the tap rule now refuses.
5. **THE CROWN's reel rule** is RED by a strict "every tell frame drawn" check (one-frame view lag) while the fight passes; relax to "drawn within one frame".
6. **Frame writes fail in long reels** (hundreds per reel; disk C 93 % full, Logs/feel ~88 GB plus NIGHT32's): ask GG before deleting old reel folders.
7. NIGHT32.draft.md (NIGHT31's leftovers) items not covered tonight: a real Stag-charge tell before the fight (i5 / i6), the grapple's 0.2 s tap window (a guess), the pad Player line for the reel button.

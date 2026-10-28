# NIGHT31.md (draft by the NIGHT30 lead, 2026-10-09 22:40): the leftovers of NIGHT30, in priority order

Base: branch port/stage3 at tag u3-n30 (the NIGHT30 close). Read first: NIGHT30-REPORT.md, NIGHT30-LOG.md, NIGHT30-PROOF.md, Design/levels/DESIGN30.md (sections B-I), HANDOFF.md (NIGHT30 block), DECIDE30.md. GG has not played NIGHT30: his playtest notes (FEEL-PLAYTEST.md) come first and outrank this list.

## Leftovers (each with the measured state at the NIGHT30 close)
1. **THE LOOM in the Player:** the fight rises, draws, rings its joints cyan and shows its line in the Player (r6 frame), but the Player reel never proved CUT (the reel's code-attached rope was not "hanging": B's fix may have landed after the freeze; see the close table) and nobody has measured a Player kill time; the harness kill time comes from a scripted model (median 107-111 s, band 90-150), not a rope-swinging pilot. FLYERS (the third attack) is not built (cut by NIGHT30's cut list).
2. **f9 (and every room): enemies spawn beside the entry.** r5 / r6 frames of f9 show husks filling the view's edges at the spawn. Rule: a wave spawn at least 6 m from the room's spawn and out of the first view cone, read by the validator.
3. **The full-frame red at low health** is NIGHT18's PostFx vignette (`PostFx.cs`, colour = danger, intensity toward 1) plus two Ui.cs vignettes: GG asked that effects not touch the whole screen. Limit it to the edges (behind a row; GG decides).
4. **Grapple node rings:** `GrappleNodes30` draws a cyan ring per node; no Player frame has shown one in view yet (s2's node is above the entry's view). A frame aimed at a node; the old `Arcline21View` cubes still draw in campaign.
5. **Item 9's later iterations:** the rooms other than f8 / f10 / i1b / i5 / i6 / k1 / k2 got one pass (the lava and the counts) and no redesign; f5b's 6.0 m ledge is unreachable (the east lane tops out ~4.4 m); i5's rails stay 2.2 m (1.2 m let a player walk past the blocks). A `hero` row in the plans so the view's accent light is the room's hero, not the brightest amber socket.
6. **Look:** vertex snap and stone texture noise not built (first on the cut list); heat shimmer not built; the quantise strength 0.35 is a first pick (GG's eye); the bench of the look's cost: see the close table.
7. **Pad reels:** menus30 / weaponhud30 passed on r5 and failed on r6 on pad presses only (the machine in use); run the pad scenarios with nobody at the machine.
8. **Wyrm feints** (the reviewer: a stand-still puncher wins every charge) and a Player kill time for the Wyrm with wyrm30 on (harness lag bot median 3:21, one seed 4:22).
9. **Level card boss column:** the level select shows NAILGUN for level 4 but no boss on any card (V: a new column; GG decides).
10. **The DECIDE30 rows** stand at their defaults unless GG answers (D-1 the rules, D-2 Glacier kept, D-3 THE SPAN, D-4 the look, D-9 the Editor's Arena.unity / UIEffect changes).

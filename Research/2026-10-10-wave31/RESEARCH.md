# RESEARCH index (wave 31 forge, 2026-10-10)

Three web-only research agents (about 2-3 minutes each, in parallel) and one read-only code scout. Every outside claim in
R1-R3 carries its URL; several rest on fan wikis or search summaries because the page fetch failed, and the files say so.
Numbers read through a page summariser: spot-check before they go into config.

| file | question | the line the prompt uses |
|---|---|---|
| R1-bosses.md | how the best 3D arena bosses stop camping and force movement and weapon variety (18 patterns, 5 failure modes) | a stillness / distance timer + a distance-keyed kit (V2); two damage states that need different weapons (Icon of Sin, Marauder, Metroid Prime); a half-health change that moves the safe spot (V2's walls); teach the tell with a smaller enemy (Dead Cells); failure: a boss that ignores the level's toolkit (Titanfall 2's Richter and Kane) |
| R2-grapple.md | what makes a first-person grapple pull you in | a rope only stops you moving away; pull games set velocity toward the anchor (ULTRAKILL whiplash 60 u/s, from a wiki mirror), release on arrival with momentum kept, cancel into another verb, and show the target before the press (Apex ring, Sekiro marker); a 10-bullet spec in section 4 |
| R3-weapons.md | an alt on every weapon, charge-shot feel, one weapon per level | ULTRAKILL piercer about 0.5 s charge and 2.5 s cooldown, a safe early cancel; coin / core cross-weapon combos; Halo's pistol zoom; Neon White places a new card just before it is needed |
| SPIKE.md | can the grapple pull without editing Mover.cs | yes in principle: the Mover's ZIP mode (34 m/s, release at 2.6 m, speed kept) is set from the owner's `aimHook`; a node hook with the zip set is a pull-in |
| IDEAS-POOL.md | 25 cards scored by owner fit, feasibility, novelty | top: the camp test (6), pull-in grapple, magnetism, charge shot, LT everywhere, one weapon per level, the weapon-gated weak point (5 each) |

Code facts (the scout, 2026-10-10, file:line in NIGHT31.md's facts block): the charge shot (`ChargeShots27.cs`) is built
and switched off since NIGHT27 because the Player smoke failed with it on (cause not found); runs start with the KESTREL and
the gauntlet, levels 1-2 award nothing; the THUNDERCLAP's shockwave was removed at GG's word in NIGHT25 (its LT zooms
1.25 x); the gauntlet's LT works only after five punch kills.

# L6: the PULSE's own gun glow (T-113; design.n71.glow71, a view row; OFF in the repo, ON in the play build)

- **The change, plainly:** the PULSE gauntlet now shows its charge on the gun like the KESTREL and THUNDERCLAP do: four cyan steps along the top of the gauntlet light with its pips while the BLAST is ready, with a white flash at full.
- **Why it was missing:** NIGHT50's steps ran along the muzzle line, inside the forearm block, so 0 % and 75 % read alike (DECIDE50 D-8; n50-r1 a150 glow-gauntlet-0 / -75). Now they sit on the body's top (the body mesh's bounds' top + lift), from the back cap toward the knuckles.
- **Off flag:** `-skygrindNoGlow71` (also `-skygrindOff71` or older; gunglow50 off). ON: a play build made with `n71.glow71`.
- **Reel (proof, the lead):** the a150 reel's gunglow50 pass already frames the PULSE at 0 / 25 / 50 / 75 / 100 % (forced levels): `bash Tools/night/n71-reels.sh <tag> g71on,n52-foundry,a150 g71off,n52-foundry,a150,-skygrindNoGlow71`. GREEN: `gunglow50 ok: KESTREL THUNDERCLAP PULSE`, the PULSE line `lit k/4`, luma rising each step, flash 2 frames, the gun still. RED (off): `gunglow50 PULSE: NO STEPS on this rig`, `not ok: PULSE`. The frames `H50-glow-gauntlet-<pct>` are the SPOT pair.
- **Colour:** the player's cyan (Look49UsableHex), as the other guns' steps; no HUD element changes (the PULSE pips untouched).
- **Numbers:** design.n71.glow71.numbers segW 0.036, segH 0.01, lift 0.002, span 0.8, gapShare 0.3 (m, rig space); re-apply with `node Research/2026-10-26-wave71/F/L6/glow71-config.cjs`. Not checked: how the steps sit on screen (no Player run by F).
- **Gates:** with the row off: gates-off.txt. With it on: EQUAL by construction (the sim harness compiles no view). EditMode: N71GlowTests (3).

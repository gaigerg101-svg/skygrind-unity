# DECIDE28: questions for GG on the Glacier rebuild (defaults stand until he answers)

| id | question | default taken |
| --- | --- | --- |
| D-1 | A new Mover multiplier `groundAccelMult` (start 0.4 on ice) so steering on ice is slidy? (`Mover.cs` is frozen; only GG may open it) | NO: the rink uses ice friction 0.25 (try 0.15) + the 28 degree banks + the slide for the skating feel |
| D-2 | The pistol and the shotgun are allowed on level 2 to crack the boss's ice glaze (the gauntlet still does all the damage)? | YES (the alternative is the simpler Zamboni Lap with no glaze) |
| D-3 | Full design (about 135 s, three phases) or the simple version first? | BOTH IN ORDER: the simple lap fight (about 75 s) is built and verified first, then the glaze, the island door and the phases on top of it |
| D-4 | The worm | back to the blocky old look (done: `design.n26.wormVisual` off); the Tripo view code stays on file |
| D-5 | Jump pads | rebuilt as a raised glowing disc with chevrons, a rising shimmer and a launch flare (view only) |

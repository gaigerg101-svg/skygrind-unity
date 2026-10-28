# NIGHT58 builder V: rim56 on foundry, yellow56 on foundry a5, the exit lamps (V1, V2, V3)

Builder V, worktree `worktree-agent-ab86390bb1963e824` from the shared commit 7f88487c. No Player, Editor, build or reel was run by V. Every
"after" below is SIMULATED on frames that already exist (said so on its line), or "not measured". After every commit
`dotnet run --project Tools/SimHarness -c Release -- fingerprint` (and `--off57`, `--off58`) printed `EQUAL to the baseline`, and
`bash Tools/night/dn-check-wt.sh` printed 0 errors before each code commit. No file under `Assets/Scripts/Core` changed.

## V1 rim56 on foundry (d330bab6)
**Cause (Tools/night/n58-v-rimcause.py on n57-r3 v1on-foundry / v1none-foundry a357).** foundry's environment p90 V (0.68-0.71) is set by
colour56's violet ACCENT (hue 260-270, the foundry accent 263), not by lit walls. Of the env pixels at or over that p90, 43-94 % per shot are in
that hue bin, and 55-95 % of them sit in the upper half of the frame. With colour56 off (v1none), the same rooms' env p90 V is 0.48-0.67. The
bodies are V 0.59-0.70 with S 0.80-0.95, so the S half already passes. They are short by V 0.007-0.21. rim56's gain lifts a body only +0.03 to
+0.11 (on vs off, the same shots).
**Built:** while rim56 is on, Colour56.Clamp's accent max channel is x0.45 (UnityTuning.Rim58AccentScale; Rim58.N58V.cs; one hook line in
Colour56.cs). There are also one-row off switches, `-skygrindNoRim56` and `-skygrindNoYellow56` (Rim56.On / Yellow56.On), so the off side no
longer needs `-skygrindOff56`.
**Simulated (n58-v-rimsim.py, the meter's own measure; x1.00 reproduces 58/77):** with the accent's value x0.75, body over env p90 goes
58/77 (75 %) -> 70/77 (91 %). colour56's MAIN stays 36/36 at x0.70.
**Measured by the lead on r1 (n58-r1 foundry a357, rim56 forced on): 3/14 off -> 6/14 on (43 %). The simulation over-read.** Re-read with
n58-v-rimcause.py: the env p90 V is IDENTICAL on and off on every shot (0.698 / 0.698, 0.706 / 0.706, 0.686 / 0.686). So the accent dim never
reached the frame. Either the violet trims are not drawn through Colour56.Clamp (decor55's string course, or pieces recoloured by another
path), or Clamp ran before rim56 read on for the map. The +3 bodies are the gain's lift (+0.05 to +0.23). Not checked which; the next step is
a358's `accent ... on N pieces` count on a Player. If N is 0, find the path that draws the 263 trims.

## V2 yellow56 on foundry a5 (208b7f99)
**What was left (Tools/night/n58-v-yellow.py on n57-r3 foundry a5-entry, 0.88 %):**
- the lava trench's edge glow: 10340 px, H 49, S 0.64, V 0.81;
- a husk's parry halo: 2336 px, H 53, S 0.99, V 0.90 (a parry tell: kept);
- the column top plate: 1472 px, H 48, S 0.56, V 0.82.

**Built:** the dim-yellow bounds go from vFull 0.50 / vNone 0.75 to 0.84 / 0.89. These are config numbers
(`design.n56.yellow56.numbers.dimYellow58`, written by Tools/night/n58-v-rows.cjs with n58-v-sets.cjs) that Cap57Pass reads through
Rim58.Yellow58 (one hook line). There is no shader change. `-skygrindOff58` keeps NIGHT57's bounds.

**The meter (L-106: a NEW checker, n56-colour.py unchanged):** n58-v-yellow.py folds an orb's fringe into the orb. The rule: yellow within
24 px (at 1080p, Euclidean) of an orb found by n56-colour.py's own shape rule, connected to the orb through yellow. Planted cases GREEN 4/4:
- an orb with its ring: 0 % left;
- an orb plus a far strip: the strip is counted;
- a strip alone: counted, FAIL;
- a strip running into the ring: only its far part is counted.

**Simulated (n58-v-yellow.py --sim, on the n57-r3 a357 ON stills):**
- NIGHT57's bounds: 1/36 frames over 0.5 % (foundry a5-entry, 0.85 %).
- 0.84 / 0.89: 0/36 frames over (worst 0.36 %, iceline g7-mid).
- GUARD: the orbs' share on spire v5-mid is 17.39 % before and after.
- Not measured in a Player.

## V3 the exit lamps on n57-kiln (cd504292; switched off in the follow-up commit)
Exit58.N58V.cs is a NEW view file. It reads n57-kiln's own campaign.doors[] / rooms[], so no lamp entries are needed in L's plan. Each gated
door (Zones locks it, the clear opens it), except a door into a boss room, gets two things:
- a cyan frame: posts and lintel 0.22 m thick, 0.15 m outside the opening, additive, colors.usable x3.2;
- a cyan point light (4, 6 m range) 1.5 m back in the room being left.

The lamp is dark while the door is locked. It rises over 0.35 s when the door opens: one rise, no flash. Its line goes into a358 (Exit58.Line).
**MAIN not measured:** no command printed the exit's luma vs the room's median at the clear frame before the freeze. a358 has no door rect at
the clear frame, and a157's format was not read. So UnityTuning.Exit58On = false ships it built but OFF (it has no config row: D-12). To
measure it, set the flag true in a copy, and capture each room's clear frame with the door's screen rect: the luma inside the rect vs the
frame's median, on vs `-skygrindNoExit58`.

## Seeded leftover (spire f1550's dark door): not attempted.

## Files touched outside V's own (one line each)
- `Assets/Scripts/Game/Colour56.cs`: `float mc = Rim58.AccentMax(g.cfg, ...)` (Clamp).
- `Assets/Scripts/Game/Render/Cap57Pass.cs`: `Yellow = Rim58.Yellow58(cfg, new Vector4(...))`.
- `Assets/Scripts/Game/Rim56.cs` / `Yellow56.cs` (V's N56 files): `!Rim58.No("-skygrindNoRim56")` / `!Rim58.No("-skygrindNoYellow56")` in On.

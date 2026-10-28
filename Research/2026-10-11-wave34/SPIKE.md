# Forge spike, wave 34: can the grapple's slow motion keep the player at full speed? (read, 10 minutes, 2026-10-11)

Premise to test: GG wants to "switch weapons in real time" inside a grapple slow motion while "the world bends around you". The research
(research/slowmo.md section 1) says the proven pattern is asymmetric: the world slows, the player aims and acts at full speed (F.E.A.R.,
Max Payne). What does SKYGRIND's loop allow?

## What the code has (read, with file:line)
- **A global slow path exists and is safe.** `SurvivalGame.cs:313-343`: the impact layer's share (`ImpactBeginFrame`) scales the frame's dt
  into the accumulator; the sim still steps at 120 Hz, fewer steps per real second; hitstop is a share of 0. `SurvivalGame.SlowAim27.cs`
  (wall-ride + aim eases the share to 0.75) and `sim.Lantern29SlowScale` (a sim-owned eased value read by the loop) are two precedents.
  The header says it: "the tape and the fingerprint never see it, and Time.timeScale is never touched. Look input reads the real frame
  delta". Camera look is applied per render frame (`SurvivalGame.Render.cs:34`, `input.yaw`), so look stays real time under any share.
- **What the global path cannot do:** the player's own timers (the 0.25 s swap, `PortExport/config.json` weapons `swapTime`; fire
  cooldowns; the Mover) slow with the world, so "real time swaps" need a sim rule that knows the bend (instant swap while bending).
- **The split path looks feasible.** `SurvivalSim.Run.cs:284-420` `Step()` already runs a player block first (input frames, grapple /
  whip latches, `mover.Step`, devices move, assist, `loadout.Step`, the N19 / N20 / N21 arsenal hooks, regen, buyables) and a world block
  after (`projectiles.Step`, `brain.Step`, `husks.Step`, the N19-N33 hooks, `brain.Separate`, the lava guard), then style and power-ups.
  A bend can run the player block every step and the world block on steps where `bendStep % N == 0` (whole steps, deterministic, on the
  tape as the bend's start step). Note `brain.Step` sits inside the player block today (before the loadout): it moves to the world side.

## Risks the run must handle (named in the prompt)
1. **Mixed hooks:** `N31WeaponsStep`, `N33WhipStep`, `N33PierceStep`, `N25CombatStep`, `N24FistStep` act for the player but sit in the
   world block; each must be classified (player or world) in one table in `Core/Bend34.cs` with a test per hook.
2. **Player projectiles** (rockets, grenades) live in `projectiles.Step` with the enemies'. Proposal: they slow with the world (a
   Max Payne bullet read). Decided in the card, not by the builder.
3. **Render:** enemies stepped 1 in N need their own interpolation alpha or they stutter at 30 Hz (research/slowmo.md section 4).
4. **Fingerprint:** with bend34 off the Step must be byte-identical (the split is a branch around the world block, not a reorder).
   The `brain.Step` move happens only inside the bend branch.

Verdict: build the global path first (one slice, about 40 minutes, playable), then the split behind `design.n34.bend34.split`. The lead
picks the default from Player frames and the bend34 reel's numbers.

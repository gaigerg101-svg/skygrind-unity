# SPIKE (wave 31 forge, 2026-10-10 ~10:03): can the grapple PULL without editing Mover.cs?

Read, not run. Lines from `Assets/Scripts/Core/Mover.cs` at u3-n30.

- The Mover already has a ZIP mode (NIGHT21, ported from the web's mover.js): `rope.zip` true means "straight at the anchor
  at `arcline.zip.reelSpeed`, the rope taut the whole way" (Mover.cs:662-670), released on arrival
  (`dist < arcline.zip.arrive` or after `arcline.zip.maxTime`, Mover.cs:605-608) with the velocity KEPT
  (`ReleaseRope("zip")` -> `SwingMath.ReleaseVelocity(velocity, risingBoost, ...)`, Mover.cs:497).
- Config today (PortExport/config.json:2059): `zip.reelSpeed` 34, `zip.arrive` 2.6, `zip.maxTime` 1.2.
- `zip` is set only from the owner's `aimHook` callback (`Func<InputFrame, GrappleTarget?>`, Mover.cs:154; the target's
  `zip` field, Mover.cs:537), which lives OUTSIDE Mover.cs (the ARCLINE wiring, `Core/Arcline21.cs` / `GrappleAim.cs`).
  In Survival it is true only for a big hooked enemy. A node hook is a swing: a rope that only stops you moving away.
  That matches GG's "It doesn't pull you in" and R2 section 3 ("a rope only stops you moving away; it does not move you").
- A `reel` button exists (Mover.cs:36, `input.reel.held` shortens the rope in a swing, Mover.cs:648; bound in
  `InputReader.cs:50`, listed in `Gamepad.cs:221`). Not checked: whether `reel` has a pad binding in the campaign. If it
  has none, a pad player can never shorten the rope.

**Premise:** a PULL-IN grapple to nodes (and surfaces) is an `aimHook` change (return the node with `zip = true` on a tap;
keep the swing on a hold) plus a design row, with Mover.cs untouched (CLAUDE.md rule 3). The zip speed is a web number
(34 m/s); if a different node speed is wanted it needs a Unity-side config row read by the owner, never a Mover edit:
the run must check whether `ArclineConfig` can be swapped per hook without touching the golden path (golden vectors run
with `aimHook` null, so they cannot see it). Risk left for the run: the zip ignores the swing's collision sub-step
order? No: the zip path runs inside `StepSwing`'s sub-steps (Mover.cs:660-684), so it collides like the swing.

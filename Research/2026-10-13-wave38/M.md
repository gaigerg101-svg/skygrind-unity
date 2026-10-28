# Builder M notes (NIGHT38): ride38 and noride

## ride38 (design.n38.ride38; Core/Ride38.cs; Mover.cs branches marked "NIGHT38 ride38", inside wallride35's)
- Only a HOLD ride (jump held at the entry). Once a rising entry has braked (wallride35 / wallgrip36's brake), the vertical speed is the look
  pitch's drift with no gravity: 0 within +-15 deg (holdSink 0), linear to +-2.5 m/s at +-45 deg, clamped past it.
- A rise stops at the wall's top: the feet never pass top - 1.45 m (the capsule's upper contact sample, body.capsuleHeight - capsuleRadius),
  measured 1.3 cm under it on a 10 m wall (`n38ride hold` row "rise"). The 8 s safety cap is replaced by a 60 s guard (reason "safety").
- The timed (tap) ride is bit for bit NIGHT37's (`n38ride hold` row "tap").
- For gate design (builder E's envelopes): a held ride on a wall of height H can put the feet anywhere from the floor to H - 1.45 m, at any
  point along the wall, before the jump-off. A ride wall's height is therefore a launch-height envelope, not only a surface.

## noride (the level format)
- A campaign box (json `campaign.boxes[]`, with `min` / `max`) tagged `"noride": true` is a wall no ride starts on (wallride35's entry and
  its near-miss attach) and no ride turns onto at a corner (the ride ends "corner"). Read with `Ride38.NoRideFrom(mapJson)`, handed to the
  Mover with `Mover.SetNoRide38(boxes)` (the sim does it at each run start while any NIGHT38 block is on: SurvivalSim.N38.Ride.cs).
- E's gate checker: build the Mover, then `m.SetNoRide38(Ride38.NoRideFrom(level json))`.
- It blocks RIDES only: the web's wall jump (a press near a wall) still works off a noride wall, so a noride gate must still be checked for
  wall-jump chains.
- Needs wallride35 on (the game's config has it on); with wallride35 off the flag is ignored.
- The look (dull, ridged, no cyan trim): RideTrim36 already trims only `ride: true` boxes; a noride box should not be tagged `ride`.

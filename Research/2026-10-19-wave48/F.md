# NIGHT48 builder F: research notes (round 1)

Each line says Verified (I read the page, the code, or measured it tonight) or Reported (a search summary; the page itself not read).
The NIGHT47 cards (Research/2026-10-18-wave47/F-cards.md) stand; these lines are what round 1 added.

## Web
- R1 (Reported: search summary, the pages not read): procedural multi-leg gaits step a leg when its foot has drifted a set distance from
  its target, lift through a midpoint, and stagger which legs may move together (diagonal pairs) so the legs never move in unison.
  Sources: Digital Production, "Spider Walker automates multi-leg animation" (2026-08-28), https://digitalproduction.com/2026/08/28/spider-walker-automates-multi-leg-animation/ ;
  The Rookies, "Spider Procedural Animation", https://www.therookies.co/projects/33667 . Used for: G14's gait (foes48, Mesh43.Legs48): the
  phase follows the DRAWN distance (Legs48StrideM a cycle), diagonal pairs half a cycle apart, the swinging leg lifts; a still body eases to rest.

## Code read tonight (Verified)
- C1 (Verified, EnemyViews.Shield47.cs / UnityTuning.F47.cs): foes47's arm is a 0.35 x 1.3 m box turned 62 deg forward about its own
  centre (0.55, 1.15, 0.38): its far end reaches z ~0.95 m, past the shield's front face (0.71 m at x 0.55). Measured by `n48feel shield`:
  4 / 8 of its corners out the front at rest.
- C2 (Verified, same files): foes47's DOWN pose is a Slerp from Euler(-kick, 0, 0) to Euler(-22, -90, 0) with a Lerp of the position: the
  path passes THROUGH the body (n48feel shield: 81-96 of 190 shield points inside the body's drawn capsule at a 0.25-0.5 down share; 4
  inside and the bottom 0.04 m under the floor when down).
- C3 (Verified, Mesh43.Visor47.cs): visor47 builds the angular band for the four patch visors only; Eyes3 (swarmer), Ball (spitter) and
  Lenses3 (drone) return null and keep their shape: 3 of 7 kinds had no band and no temple points.
- C4 (Verified, EnemyViews.cs SwarmerView): foes47's "scuttle" is the legs part's x scale breathing at 6 Hz by drawAge, standing or running.
- C5 (Verified, Fx47View.cs + UnityTuning.Fx20.cs KenneyRingMid 0.63, band 0.54-0.72): the blast's aim ring's inner edge is 0.857 x its
  radius; with Fx47AimRingClear 0.12 it is 0.103 x its distance, inside the reticle box's +-0.134 half-width (10 % of a 16:9 frame at a 74 deg
  vertical fov); and it is drawn on the aim at the press (aim0) while the blast's own kick (3.5 deg) lifts the view after the draw.
- C6 (Verified, Tell36View.cs): a launch36 burst shell is capped at Fx47ShellMaxShare x its distance (~20 deg) and sits where the thrown
  bodies meet, on the aim line: n47-r3's f18 / f30 wash frames had shells 1-2 (Logs/player/n47-reel-n47-r3-a747s.log).
- C7 (Verified, FeelReel.N47.Fx.cs Sheets47More): a747's charged-shot sheets gated on ChargeShots27 (chargeShots27 off in the config), so the
  KESTREL charge and the slug were never measured; the live charge is charge35 / charge40 (docs/VERBS.md rows 27 / 30: hold, release fires).
- C8 (Verified, Core/AimAssist.cs header + PortExport/config.json `aimAssist: "snap"`): the default SNAP assist pulls the look onto the
  body's aim point (the head inside the inner half of the cone, else centre mass) before the shot. hitzone47 reads its zone from the hit
  point: an aim at the legs or the side lands on the centre or the head. n47-r3 a747 (Verified, its log): ON zones read head 0, legs none,
  side none.

- C9 (Verified, FeelReel.cs Capture + FeelProbe.Record.cs RecordFrame + UnityTuning.FeelAB.cs ProbeMaxFrames 1800): the reel's JPG
  recorder names each frame f = FeelProbe.FrameCount - 1, and RecordFrame stops counting at 1800 frames; every later frame is written to the
  same f1799.jpg on the thread pool, and the overlapping writes of that one file throw (failed++). Matched on every n47 / n48 reel log:
  reels under 1800 frames 0 failed (1080, 1110, 1577, 1745), over it some (1920: 1, 3178: 49, 5740: 110).
- C10 (Verified, n48-r1 a748 log + the frame X48-blast-A-f4): the ON passes' only wash frame (f4) had no halo and no shell; the frame shows
  the PULSE punch aura (Fx47View OnMeleeHit: three-tone spheres + a 2.5x halo disc) on the 3 m body's hit point, in the reticle's box.

## Measured tonight (Verified, my harness)
- M1: `n48feel shield` (Tools/SimHarness/Harness.N48.Feel.cs): foes47 RED (above); foes48 every pose 0 points inside the body / the
  pauldrons / the helmet, the arm 0 / 8 out the front; up centred x 0.00 m, y 0.56 h, body clearance 0.106 m; down x 1.03 m, the bottom
  0.03 m over the floor, clearance 0.219 m.

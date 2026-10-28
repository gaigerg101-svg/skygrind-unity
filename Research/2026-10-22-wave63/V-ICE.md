# NIGHT63 builder V: THE ICEBREAKER covers the screen when the player is close (branch n63-ICE from port/stage3 at 2a5a1345)

The lead's real-play bug. Its own rule line tells the player "Get close and swing the gauntlet {fire}".

## The red (read only)
- On b1-b4, boss63 on n52-iceline with bossMotion63 off: the flinch and stagger frames are black but for the HUD and the gauntlet, e.g.
  `Logs/feel/n63-b4/b63iS-off/boss63/shot-N30-B63-brawl40-stagger.png` (opened: a near-uniform dark navy-violet screen).
- The cause, read from the reel's lines, the telemetry and Brawl40View.cs (not from a Player probe):
  - The first stagger, "stuck" at 4583 ms, follows a `brawlLeap` whose landing tracks the player.
  - Nothing in the sim keeps the player out of the boss's body. After the landing the camera sits about 1.7 m from its centre (the reel's
    line; the telemetry's camera at z 269.35, pitched 12.97 deg up at the centre).
  - The view's stuck pose leans the torso 50 deg toward the player. Worked out from its numbers, the torso's centre is about 0.9 m from the
    eye at eye height and the hump's about 0.46 m. The eye is against or inside them.
  - They are dark violet (Brawl40BodyHex 0x2a2438 / 0x1a1622): the screen's colour.
  - It is not a near-plane fault or a back face.
- The parts draw with the Stone / URP Lit material, which has no dissolve, so C2's rule (the enemy shader's dissolve) cannot reach them as it is.

## The fix (view only, behind design.n63.nearFade63, the same rule as C2: an enemy never fills the screen from inside)
- `NearFade63.Hide`: the eye is within `numbers.bossHideM` (0.6 m) of a part's world bounds (inside: 0 m).
- `NearFade63.HideNear`, every frame: a part at the eye stops drawing (`Renderer.forceRenderingOff`: the component stays enabled and its
  bounds live, so it is measured again next frame); a hidden part the eye has left draws again; with the row off every part it hid draws
  again and its set empties; a part its own view turned off is never marked.
- The hook, one line at the end of `Brawl40View.Render`: `NearHide63` over the body's parts only. Those are bodyRoot's (the legs, the leaning
  torso and hump, the head, the rims, the frost, the cracks) and the two arms and two fists. Never the arena or the fight's effects.
  - The row is read every frame. No allocation per frame (the lists are reused, the roots cached).
- The number by `node Tools/night/n63-v-c2-rows.cjs --numbers`: replaces only design.n63.nearFade63.numbers, both copies byte-identical,
  nothing else changed, the row OFF.
- The lead's optional item: the boss63 reel skips a D1 held frame whole (`FeelClock.Holding`). No damage event, flinch frame, pose, rms,
  stagger or G's measure is read on it, so b4's Crown "drawn off" artefact cannot recur.

## Shas
`bc28b290` (test first, RED: N63NearHideTests), `da89169f` (the rule, the helper, the number, the hook), `6a7ad448` (boss63 skips a held
frame), then this notes commit. dn-check 0 errors at every commit.

## The SPOT pose and the Player steps
- The SPOT pose: b63i's `shot-N30-B63-brawl40-stagger.png` (the stuck stagger after the leap), on a same-run pair toggling n63.nearFade63:
  `node Tools/night/n46-build-rows.cjs <tag> on n63.nearFade63`, then the b63i side with `-skygrindSameRun63 n63.nearFade63`.
  - Off: the black screen. On: the torso and hump (the parts at the eye) gone, the room, the legs, the head, the arms and the stuck fist
    drawn.
  - Expected far above 41,472 px (the off frame is nearly all one dark colour): not measured.
- Grep: the boss63 line for the Icebreaker; `Brawl40View.NearHidden63` is the count of parts hidden on a frame, for a reel if one is wanted.
- Not checked:
  - Any Player frame.
  - Whether `Renderer.bounds` stays live with `forceRenderingOff` in this Unity (I believe so; N63NearHideTests' HideNear test checks it on
    real renderers in the Editor).
  - Whether 0.6 m hides too much or too little of the leaning torso. The frame will show whether the hump stays at the eye's edge.
  - Other bosses: only the Icebreaker's view calls it. The helper is shared if the Wick, the Loom or the Crown ever reach the eye.

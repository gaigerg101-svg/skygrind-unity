# Worm boss visuals (builder V, NIGHT26)

Written before the code, from reading Game/WyrmView.cs, Core/Enemies/Wyrm.cs and the three FBX files. "Measured" means a number I computed from the files;
"not checked" means it needs the Editor or a Player.

## What is drawn today
- `WyrmView` (Game) draws THE KILN WYRM as 26 pieces: the head, `design.n24.wyrm.body.pieces` = 24 body boxes, the tail. Each piece is two
  `MeshRenderer`s (a dark box and a pale trim plate, 52 renderers), a core sphere, 120 shot boxes and 6 mark boxes, all unit cubes / spheres on the
  `FxDraw` neon (URP Unlit) material. One draw per renderer, no instancing across pieces.
- The pieces are the sim's own boxes (`Wyrm.boxes[i]`: centre `c`, frame `f / r / u`, half sizes `hr / hu / hf`; `prevCenters` one step back for the
  render interpolation; `buried[i]` hides a piece under the floor). The sim keeps a ring buffer of the head's positions and puts piece i at the entry
  `i x spacingSteps` back, so the body already follows the head's exact trail. `design.n24.wyrm.body.spacingSteps` = 13 steps (2.4 m at 22 m/s); each
  box is as long as the gap to its neighbour plus `overlap` (0.3 m), so a lunge stretches the boxes and leaves no hole. Head 3 x 3 x 4 m, body 2.5 m
  tapering to 1.2 m, the tail 0.6 of that.
- Not touched: the core sphere (the exposed weak point), the spit and shards, the marked spots, the room.

## The FBX files (measured by parsing the binary FBX, Tools-free script in the scratchpad)
- Every piece is Z-up raw with a node `Lcl Rotation` of -90 about X (upright after import), pivot at its feet; units are metres.
- Head: 0.54 wide, 0.86 tall, 0.98 long. Side silhouette: the tube comes in high at -Z and curves down to the jaws at +Z; the lowest vertices (jaw teeth)
  sit on the +Z side. It faces +Z.
- Body: 0.98 along X, 0.53 along Z, 0.74 tall. The side view (Z-Y) is a ring: the tube runs along X, not Z. NIGHT26 C1's `WormBossSetup` stacked it along
  the head's axis (Z), so its hoops would have been placed side by side, not end to end: the "gap" risk was real.
- Tail: 0.28 x 0.62 x 0.98, runs along Z; many vertices (the thick, curled end with barbs) at -Z, few (the thin tip, one vertex at the end) at +Z.
- NIGHT26 C1's chain used the head's mean-vertex "heavy end", which is the jaw side here (mean z +0.05): the chain would have left through the jaws.
- The albedo textures paint the seams, the eyes and the body orbs saturated red (r ~1, g ~0.15) and some patches pale yellow: the emission mask is read
  from the albedo (r - 1.6 g), so no extra texture is needed. The Player shader reads the albedo only (the Tripo normal, roughness and metallic maps are not used there; the prefab's URP Lit materials still use them).

## The plan
1. `WormBossSetup` bakes each piece into one frame (+Z forward, +Y up, centred, quarter turn for the body, half turn for the tail when the vertices say so)
   into `Assets/Resources/WormBossParts.asset` (meshes, textures, both shaders, bounds sizes), with the head's jaw mask in the vertex colour; the prefab is
   rebuilt from the baked meshes (head at the origin facing +Z, chain along -Z). The lead runs the menu item and checks the Console note and the prefab.
2. `WyrmVisual26` (Game) draws head / body (one instanced call for all body pieces) / tail from the sim's boxes: position `c`, rotation `LookRotation(f, u)`,
   scale: Z = box length / mesh length (no gaps), X and Y between that and the box width (`WyrmFxWidthBlend`). Three draws, three materials (one shader).
3. `Shaders/Wyrm26.shader`: soft-lit albedo, HDR red-orange emission on the red mask, per-instance `_Glow`, `_Jaw`, `_Hue`, `_Hot` (white-hot), `_Burn`.
   `Shaders/Wyrm26Aura.shader`: additive shell / halo / flat (embers).
4. `WyrmGlow26` (pure functions, EditMode tests): idle wave, coil pulse (rate rises to the release), release peak, telegraph per attack kind, trail, impact,
   damage flash, HP dimming, death flare and head-to-tail burn-out. The view fills the state from the sim's public state and the `bossTell` / `bossHit`
   events; the hit piece is found by the aim ray against the same boxes (no sim change). One getter was added to `Wyrm.cs`: `PathLength` (read only).
5. Fallback: `design.n26.wormVisual.enabled` off, or no parts asset: the blocky boxes as before.
6. Reels `wormcoil` and `wormattack` print readings; `wormcoil` ends with the frame-cost helper (effects on vs off, 24 husks).

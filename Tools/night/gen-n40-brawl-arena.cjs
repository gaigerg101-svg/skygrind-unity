// NIGHT40 builder B2: THE ICEBREAKER's arena (design.n40.brawl40.arena) as WyrmArena boxes (half, height, playerSpawn, collision.boxes),
// ARENA-LOCAL (the floor's centre is 0, 0, 0; the sim offsets it to the boss room row's brawl40.centre). Writes Design/maps/brawl40-arena.json
// and its StreamingAssets copy (byte-identical). Run from the repo root (or a worktree): node Tools/night/gen-n40-brawl-arena.cjs
// The shape (NIGHT40.md B2: "a compact arena (~30-40 m) with ride walls on its rim and pillars to break line"):
//   - the ice floor 36 x 36 m (half 18), 1 m thick under y 0;
//   - the rim: four 8 m walls 1 m thick (ride walls: R2's ride40 asks a wall >= 2 m tall), the +z wall split by the ENTRY GAP (4 m wide,
//     x -2..2) where the level's door meets it (n40-iceline's x9 is entered from +z walking -z, as every iceline room); the sim closes the gap
//     with the slab (kind "slab": not in this file's boxes, the sim builds it from `slab`) walkin36.lockSeconds after the walk-in;
//   - four PILLARS (2.4 x 2.4 m, 7 m tall) on the diagonals at +-8 m: line breakers for the pistol at range, a wall to ride round in a pinch,
//     and the haymaker's lunge stops on them (the clamp pushes the body out). No pillar within 6 m of the spawn or of the centre.
const fs = require("fs");
const path = require("path");
const root = path.resolve(__dirname, "..", "..");
const half = 18, wallH = 8, wallT = 1, gap = 2, pillarHalf = 1.2, pillarH = 7, pillarAt = 8;
const boxes = [];
const box = (min, max, kind, job) => boxes.push({ min, max, kind, job });
box([-half, -1, -half], [half, 0, half], "floor", "the ice: the whole fight on one level");
box([-half - wallT, 0, -half - wallT], [half + wallT, wallH, -half], "wall", "rim north: a ride wall (8 m)");
box([-half - wallT, 0, -half], [-half, wallH, half], "wall", "rim west: a ride wall (8 m)");
box([half, 0, -half], [half + wallT, wallH, half], "wall", "rim east: a ride wall (8 m)");
box([-half - wallT, 0, half], [-gap, wallH, half + wallT], "wall", "rim south, west of the entry gap: a ride wall (8 m)");
box([gap, 0, half], [half + wallT, wallH, half + wallT], "wall", "rim south, east of the entry gap: a ride wall (8 m)");
for (const sx of [-1, 1])
  for (const sz of [-1, 1])
    box([sx * pillarAt - pillarHalf, 0, sz * pillarAt - pillarHalf], [sx * pillarAt + pillarHalf, pillarH, sz * pillarAt + pillarHalf], "pillar",
      "a pillar: breaks the line of a gun from range; the haymaker's lunge stops on it");
const arena = {
  _why: "THE ICEBREAKER's arena (NIGHT40 builder B2; Tools/night/gen-n40-brawl-arena.cjs; design.n40.brawl40.arena). Arena-local: the floor's centre at 0, 0, 0. Built as world layer brawl40Arena at n40-iceline x9's brawl40.centre (SurvivalSim.Brawl40.cs). 36 x 36 m of ice, 8 m ride walls on the rim, the entry gap (x -2..2) on +z closed by the slab after the walk-in, four 7 m pillars at +-8 m on the diagonals.",
  half,
  height: wallH,
  playerSpawn: { x: 0, y: 0, z: half - 2, yawDeg: 0 },
  bossSpawn: { x: 0, y: 0, z: -6 },
  slab: { min: [-gap, 0, half], max: [gap, wallH, half + wallT] },
  collision: { boxes },
};
const text = JSON.stringify(arena, null, 1) + "\n";
const out = path.join(root, "Design", "maps", "brawl40-arena.json");
const copy = path.join(root, "Assets", "StreamingAssets", "Design", "maps", "brawl40-arena.json");
fs.writeFileSync(out, text);
fs.writeFileSync(copy, text);
console.log(`brawl40-arena.json: ${boxes.length} boxes (half ${half}, walls ${wallH} m, pillars ${pillarH} m at +-${pillarAt}); copied to StreamingAssets`);

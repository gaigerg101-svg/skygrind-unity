// D. Arena data: every map loaded by the real arena (src/world/arena.js), with its collider boxes and wedges, the
// static collision triangles taken out of the built three.js Octree, the core grid and its preset heights, spawn
// points and the rest of the map data, the Survival zone parts, and the nav grid built the way main.js builds it. The
// Spire also gets the Duel nav graph built the way src/nav/navWorker.js builds it (core locked to 'towers').
// Run: node port-export/scripts/maps.mjs
import { BoxGeometry, Scene } from 'three';
import { createArena } from '../../src/world/arena.js';
import { CORE_PRESETS, presetHeights } from '../../src/world/coreGrid.js';
import { LINK, LINK_NAMES, createNavGrid } from '../../src/ai/navGrid.js';
import { buildNavGraph } from '../../src/nav/navGraph.js';
import { resolveZones, zoneIndexAt } from '../../src/survival/zoneMath.js';
import { mulberry32 } from '../../src/enemies/spawnMath.js';
import { config, enc, writeJson } from './lib.mjs';

const MAPS = ['spire', 'training', 'survival'];
const EXTRA_PRESETS = ['platforms', 'pen', 'pillars']; // boss arenas, outside the random rotation (coreGrid.js)

/** Every distinct triangle stored in a three.js Octree (a triangle can sit in several leaves: dedupe by identity). */
function octreeTriangles(octree) {
  const seen = new Set();
  const flat = [];
  (function walk(node) {
    for (const t of node.triangles) {
      if (seen.has(t)) continue;
      seen.add(t);
      flat.push(t.a.x, t.a.y, t.a.z, t.b.x, t.b.y, t.b.z, t.c.x, t.c.y, t.c.z);
    }
    for (const s of node.subTrees) walk(s);
  })(octree);
  return { count: seen.size, flat, bounds: { min: octree.bounds.min.toArray(), max: octree.bounds.max.toArray() } };
}

/** A winding-sensitive, rotation-invariant key for a triangle (float32-rounded, as the geometry buffers store it). */
function triKey(p) {
  const s = p.map((v) => v.map((c) => Math.fround(c).toFixed(4)).join(','));
  return [s.join('|'), [s[1], s[2], s[0]].join('|'), [s[2], s[0], s[1]].join('|')].sort()[0];
}

/**
 * The box triangles the Octree does not hold. arena.js feeds every box through BoxGeometry into Octree.fromGraphNode;
 * three's Octree.calcBox() pads only the min corner of its bounds (-0.01), so a face lying exactly on a max plane can
 * fall outside every child box when the tree splits and is dropped. Found by building the same BoxGeometry triangles
 * (with multiplicity) and matching them against what the Octree kept.
 */
function droppedBoxTriangles(boxes, kept) {
  const have = new Map();
  const f = kept.flat;
  for (let i = 0; i < f.length; i += 9) {
    const k = triKey([[f[i], f[i + 1], f[i + 2]], [f[i + 3], f[i + 4], f[i + 5]], [f[i + 6], f[i + 7], f[i + 8]]]);
    have.set(k, (have.get(k) ?? 0) + 1);
  }
  const dropped = [];
  for (const b of boxes) {
    const w = b.max[0] - b.min[0];
    const h = b.max[1] - b.min[1];
    const d = b.max[2] - b.min[2];
    const p = new BoxGeometry(w, h, d).translate(b.min[0] + w / 2, b.min[1] + h / 2, b.min[2] + d / 2).toNonIndexed().getAttribute('position');
    for (let i = 0; i < p.count; i += 3) {
      const tri = [0, 1, 2].map((j) => [p.getX(i + j), p.getY(i + j), p.getZ(i + j)]);
      const k = triKey(tri);
      const n = have.get(k) ?? 0;
      if (n > 0) have.set(k, n - 1);
      else dropped.push({ box: b, triangle: tri });
    }
  }
  const max = kept.bounds.max;
  const onMaxPlane = (tri) => [0, 1, 2].some((axis) => tri.every((v) => Math.abs(v[axis] - max[axis]) < 1e-6));
  return { count: dropped.length, allOnMaxPlanes: dropped.every((d) => onMaxPlane(d.triangle)), list: dropped };
}

function navGridExport(arena, tagAt) {
  const t0 = performance.now();
  const grid = createNavGrid(config, { getCollider: () => arena.octree.geometry, tagAt }).build(arena.map.floorSize, { sync: true });
  const ms = performance.now() - t0;
  const s = grid.snapshot;
  const n = s.count;
  const x = new Float64Array(n);
  const z = new Float64Array(n);
  for (let i = 0; i < n; i++) {
    x[i] = grid.colX(s.col[i]);
    z[i] = grid.colZ(s.col[i]);
  }
  const linkCounts = LINK_NAMES.map((_, k) => s.linkKind.reduce((c, v) => c + (v === k ? 1 : 0), 0));
  return {
    source: 'createNavGrid(config, {getCollider: () => arena.octree.geometry, tagAt}).build(map.floorSize, {sync: true}) (src/ai/navGrid.js)',
    buildMs: Math.round(ms),
    cell: config.ai.grid.cell,
    size: grid.size,
    origin: -arena.map.floorSize / 2,
    columnCenter: 'column c: x = origin + (c % size + 0.5) * cell, z = origin + (floor(c / size) + 0.5) * cell',
    count: n,
    linkKinds: LINK,
    linkCounts: Object.fromEntries(LINK_NAMES.map((k, i) => [k, linkCounts[i]])),
    nodes: {
      note: 'parallel arrays, one entry per node (index = node id). x, z: the column centre; y: floor height; clear: headroom to the next surface above (m); tag: zone index (-1: none); edge: 1 when a floor is missing under one side of the body; col: column index; blocked: all 0 at build (locked zones are masked at run time by tag)',
      x: Array.from(x), z: Array.from(z), y: Array.from(s.y), clear: Array.from(s.clear), tag: Array.from(s.tag), edge: Array.from(s.edge), col: Array.from(s.col), blocked: Array.from(s.blocked),
    },
    links: {
      note: 'CSR: node i\'s links are linkStart[i] .. linkStart[i + 1] - 1; linkTo = target node, linkKind = linkKinds value, linkCost = m-equivalent cost',
      linkStart: Array.from(s.linkStart), linkTo: Array.from(s.linkTo), linkKind: Array.from(s.linkKind), linkCost: Array.from(s.linkCost),
    },
  };
}

function exportMap(name) {
  const arena = createArena(new Scene(), config);
  const map = arena.load(name);
  const half = map.floorSize / 2;
  const floor = { min: [-half, -config.core.floorThickness, -half], max: [half, 0, half], kind: 'floor' };
  const tris = octreeTriangles(arena.octree.octree);
  const dropped = droppedBoxTriangles([floor, ...map.boxes, ...map.barriers], tris);

  const core = map.core
    ? {
      spec: map.core,
      note: 'n x n square pillars from (x0, z0), cellSize m each; heights row-major, row 0 at the min-z edge (presetHeights, src/world/coreGrid.js). A pillar is a box [x0 + k*cellSize, 0, z0 + r*cellSize] .. [+cellSize, h, +cellSize] with 10 triangles (top + 4 sides); pillars with h <= config.core.minHeight do not collide. arena.load() sets "flat".',
      presets: Object.fromEntries([...CORE_PRESETS, ...EXTRA_PRESETS].map((p) => {
        const rng = mulberry32(1);
        return [p, presetHeights(p, map.core.cells, config.core.maxHeight, config.core.heightStep, rng)];
      })),
      randomNote: '"random" is drawn with mulberry32(1) (the arena\'s rng before any arena.seed() call); a run reseeds it',
    }
    : null;

  let zoneParts = null;
  let tagAt = null;
  if (map.zones) {
    const parts = resolveZones(map.zones, map.zoneHoles ?? []);
    zoneParts = { source: 'resolveZones(map.zones, map.zoneHoles) (src/survival/zoneMath.js)', zoneIds: map.zones.map((z) => z.id), startZone: map.startZone, parts };
    // survival/zones.js tagAt(x, y, z) = zoneIndexAt(parts, x, y + 1, z) (the air 1 m over the floor point).
    tagAt = (x, y, z) => zoneIndexAt(parts, x, y + 1, z);
  }
  const navGrid = navGridExport(arena, tagAt);
  const gridMs = navGrid.buildMs;
  delete navGrid.buildMs; // timings go to the console only, so a rerun writes identical files
  navGrid.tagAt = tagAt ? 'zoneIndexAt(zoneParts.parts, x, y + 1, z), as survival/zones.js tagAt' : 'none (every tag is -1), as main.js when no zones are loaded';
  navGrid.corePreset = 'flat';

  let duelNavGraph = null;
  let duelMs = 0;
  if (name === 'spire') {
    const t0 = performance.now();
    arena.setCore(config.core.duelPreset);
    const grid = createNavGrid(config, { getCollider: () => arena.octree.geometry }).build(arena.map.floorSize, { sync: true });
    const graph = buildNavGraph(arena.map, arena.octree, config, arena.core, grid);
    duelNavGraph = {
      source: 'src/nav/navWorker.js body: arena.load("spire"), arena.setCore(config.core.duelPreset), createNavGrid(...).build(...), buildNavGraph(map, collider, config, core, grid)',
      corePreset: config.core.duelPreset,
      nodes: graph.nodes,
      edges: graph.edges,
      stats: graph.stats,
      note: 'edges[i] = the out-edges of node i: {to, cost (s), kind, ...}',
    };
    duelMs = Math.round(performance.now() - t0);
  }

  const out = {
    name,
    generatedBy: 'port-export/scripts/maps.mjs',
    source: 'createArena(new Scene(), config).load(name) (src/world/arena.js) over src/world/maps/' + name + '.js',
    floorSize: map.floorSize,
    killPlaneY: map.killPlaneY,
    collision: {
      note: 'The static collider arena.js builds: one box per solid ([floor, ...map.boxes]) plus map.barriers (invisible walls and caps), and one 20° wedge prism per map.wedges entry. Boxes are {min: [x, y, z], max: [x, y, z], kind}. The core pillars are a separate live collider (see core). Survival adds locked zones as solid boxes at run time (the zone parts of every locked zone).',
      boxes: [floor, ...map.boxes],
      barriers: map.barriers,
      wedges: map.wedges,
      wedgeNote: 'a wedge rises `rise` m over `length` m along a = (-sin yaw, -cos yaw) from its low-end centre (x, z); width across. Faces: slope, back (vertical, at `length`), two sides (arena.js wedgeGeometry)',
      triangles: {
        note: 'every distinct triangle in the built static Octree (boxes, barriers, wedges; not the core), flat [ax, ay, az, bx, by, bz, cx, cy, cz] per triangle, in the Octree\'s order; coordinates come from Float32 geometry buffers',
        count: tris.count,
        octreeBounds: tris.bounds,
        flat: tris.flat,
      },
      droppedByOctree: {
        note: 'box triangles (from the same BoxGeometry arena.js uses) that the built Octree does not hold, so they never collide in the game. three/addons/math/Octree.js calcBox() pads only the min corner of the bounds, so a face exactly on a max plane of octreeBounds can fall outside every child box during split(). allOnMaxPlanes: every dropped triangle lies in the plane x, y or z = octreeBounds.max',
        count: dropped.count,
        allOnMaxPlanes: dropped.allOnMaxPlanes,
        list: dropped.list,
      },
    },
    core,
    spawns: { playerSpawn: map.playerSpawn, duelSpawns: map.duelSpawns, ground: map.spawns?.ground, high: map.spawns?.high },
    pads: map.pads,
    padNote: 'a pad with a target launches with padLaunchVelocity(from, target, apexAbove, g, fallGravityMult); without one straight up to config.jumpPad.apexHeight (arena.checkPads)',
    grappleNodes: map.grappleNodes ?? [],
    zoneParts,
    navGrid,
    duelNavGraph,
    data: map,
    dataNote: 'the whole map object arena.load() returned (the level data, including everything above)',
  };
  const file = writeJson(`maps/${name}.json`, enc(out));
  return { name, bytes: file.bytes, triangles: tris.count, dropped: dropped.count, droppedOnMax: dropped.allOnMaxPlanes, nodes: navGrid.count, links: navGrid.links.linkTo.length, gridMs, graph: duelNavGraph ? `${duelNavGraph.nodes.length} nodes, ${duelMs} ms` : null };
}

export function exportMaps() {
  return MAPS.map(exportMap);
}

if (process.argv[1]?.endsWith('maps.mjs')) {
  for (const r of exportMaps()) {
    console.log(`maps/${r.name}.json: ${(r.bytes / 1e6).toFixed(2)} MB, ${r.triangles} triangles (${r.dropped} box triangles dropped by the Octree, all on its max planes: ${r.droppedOnMax}), nav grid ${r.nodes} nodes / ${r.links} links (${r.gridMs} ms)${r.graph ? `, duel nav graph ${r.graph}` : ''}`);
  }
}

// NIGHT14 U3-27.4 (SKYGRIND-unity agent N): maps/spire-duel-navgrid.json, the Spire's nav grid with the core locked to
// config.core.duelPreset ('towers'): the grid src/nav/navWorker.js hands buildNavGraph (maps/spire.json's navGrid is
// the 'flat' one, so the duel graph can't be rebuilt from it). The shape of maps.mjs navGridExport (nodes y / tag / col,
// the CSR links). It also builds the graph on it and prints its stats, which must equal spire.json duelNavGraph.stats
// (flights 38554, gridLinks tried 307 / added 266).
// Run from a port-export folder of the web repo: node port-export/scripts/navgrid-duel.mjs
// (NIGHT14 ran a copy with absolute imports from a scratch folder: the web repo is read only.)
import { Scene } from 'three';
import { config, writeJson } from './lib.mjs';
import { createArena } from '../../src/world/arena.js';
import { LINK, LINK_NAMES, createNavGrid } from '../../src/ai/navGrid.js';
import { buildNavGraph } from '../../src/nav/navGraph.js';

const arena = createArena(new Scene(), config);
arena.load('spire');
arena.setCore(config.core.duelPreset);
const grid = createNavGrid(config, { getCollider: () => arena.octree.geometry }).build(arena.map.floorSize, { sync: true });
const s = grid.snapshot;
const linkCounts = LINK_NAMES.map((_, k) => s.linkKind.reduce((c, v) => c + (v === k ? 1 : 0), 0));
const out = {
  source: 'navWorker.js: arena.load("spire"), arena.setCore(config.core.duelPreset), createNavGrid(config, {getCollider: () => arena.octree.geometry}).build(map.floorSize, {sync: true}) (src/ai/navGrid.js)',
  cell: config.ai.grid.cell,
  size: grid.size,
  origin: -arena.map.floorSize / 2,
  columnCenter: 'column c: x = origin + (c % size + 0.5) * cell, z = origin + (floor(c / size) + 0.5) * cell',
  count: s.count,
  linkKinds: LINK,
  linkCounts: Object.fromEntries(LINK_NAMES.map((k, i) => [k, linkCounts[i]])),
  nodes: {
    note: 'parallel arrays, one entry per node (index = node id): y, tag, col (x, z: the column centre)',
    y: Array.from(s.y), tag: Array.from(s.tag), col: Array.from(s.col),
  },
  links: {
    note: 'CSR: node i\'s links are linkStart[i] .. linkStart[i + 1] - 1',
    linkStart: Array.from(s.linkStart), linkTo: Array.from(s.linkTo), linkKind: Array.from(s.linkKind), linkCost: Array.from(s.linkCost),
  },
  tagAt: 'none (every tag is -1)',
  corePreset: config.core.duelPreset,
};
const graph = buildNavGraph(arena.map, arena.octree, config, arena.core, grid);
writeJson('maps/spire-duel-navgrid.json', out);
console.log(`maps/spire-duel-navgrid.json: ${s.count} nodes, ${s.linkTo.length} links ${JSON.stringify(out.linkCounts)}; graph ${graph.nodes.length} nodes, stats ${JSON.stringify(graph.stats)}`);

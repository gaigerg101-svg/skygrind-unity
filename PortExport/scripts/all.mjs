// Regenerates everything in port-export/ from the current src/ (A config, B vectors, C trajectories, D maps).
// Run from the repo root: node port-export/scripts/all.mjs
import { exportConfig } from './export-config.mjs';
import { exportVectors } from './vectors.mjs';
import { exportTrajectories } from './trajectories.mjs';
import { exportMaps } from './maps.mjs';

const t0 = performance.now();
const c = exportConfig();
console.log(`A  config.json ${c.json.bytes} B (JSON round trip exact: ${c.roundTrip}), config.notes.md ${c.rows} rows (${c.unresolved} unresolved)`);
for (const [name, count] of Object.entries(exportVectors())) console.log(`B  vectors/${name}.json: ${count} records`);
for (const r of exportTrajectories()) console.log(`C  trajectories/${r.name}.json: ${r.steps} steps, states ${r.states.join('/')}`);
for (const r of exportMaps()) {
  console.log(`D  maps/${r.name}.json: ${r.triangles} Octree triangles (${r.dropped} box triangles dropped, all on max planes: ${r.droppedOnMax}), nav grid ${r.nodes} nodes / ${r.links} links${r.graph ? `, duel nav graph ${r.graph}` : ''}`);
}
console.log(`done in ${((performance.now() - t0) / 1000).toFixed(1)} s`);

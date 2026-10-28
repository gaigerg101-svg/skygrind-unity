// NIGHT72 builder C (slate C3 decor72): the mark list of a level, offline. Mirrors Decor72.cs's classification (UnityTuning.N72C.cs's reaches)
// on the level json's own data, so the per-level counts the Player prints ("[decor72] <level> ...") can be checked against this file's.
// A band segment (Decor55's string course / Colour56's frieze: a 4 m piece of a wall box's face) has a JOB when it is
//   RIDE   : on a wall box that is a ride panel (campaign.boxes with ride: true; on a level with no tag, a note / why naming a ride panel or wall),
//   ROUTE  : within ROUTE_M (xz) of a door's rect (campaign.doors[].rect = [x0, z0, x1, z1]),
//   HAZARD : within HAZARD_M (xz) of a pit's rect (campaign.pits[].rect) or a lava rect (lava[].rect),
// else it has none and decor72 hides it. Usage:
//   node Tools/night/n72-c-marks.cjs <level json> [...]      e.g. Assets/StreamingAssets/Design/levels/n52-spire.json
// The numbers below are copied from UnityTuning.N72C.cs (Decor72RouteReachM / Decor72HazardReachM / Decor72Segment): keep them equal.
const fs = require('fs');
const ROUTE_M = 4, HAZARD_M = 2, SEG = 4, MIN_LEN = 1.0;
const RIDE_NOTE = /ride (panel|wall)/i;

function rectDist(px, pz, r) { // r = [x0, z0, x1, z1]
  const dx = Math.max(r[0] - px, 0, px - r[2]), dz = Math.max(r[1] - pz, 0, pz - r[3]);
  return Math.hypot(dx, dz);
}

function analyse(file) {
  const j = JSON.parse(fs.readFileSync(file, 'utf8'));
  const boxes = (j.data && j.data.boxes) || [];
  const camp = j.campaign || {};
  const cb = camp.boxes || [];
  const tagged = cb.some(b => b.ride === true);
  const ridePanels = cb.filter(b => b.kind === 'wall' && (b.ride === true || (!tagged && RIDE_NOTE.test((b.note || '') + ' ' + (b.why || ''))))).filter(b => b.min && b.max);
  const doors = (camp.doors || []).map(d => d.rect).filter(Boolean);
  const hazards = (camp.pits || []).map(p => p.rect).filter(Boolean).concat((j.lava || []).map(l => l.rect).filter(Boolean));
  const eps = 0.6;
  const isRide = b => ridePanels.some(p => b.min[0] >= p.min[0] - eps && b.max[0] <= p.max[0] + eps && b.min[1] >= p.min[1] - eps && b.max[1] <= p.max[1] + eps && b.min[2] >= p.min[2] - eps && b.max[2] <= p.max[2] + eps);
  const out = { level: j.name || file, walls: 0, faces: 0, segments: 0, ride: 0, route: 0, hazard: 0, none: 0, rideBoxes: 0, doors: doors.length, hazards: hazards.length };
  for (const b of boxes) {
    if (b.kind !== 'wall') continue;
    out.walls++;
    const ride = isRide(b);
    if (ride) out.rideBoxes++;
    for (let f = 0; f < 4; f++) {
      const alongZ = f < 2;
      const nx = f === 0 ? 1 : f === 1 ? -1 : 0, nz = f === 2 ? 1 : f === 3 ? -1 : 0;
      const fixedC = f === 0 ? b.max[0] : f === 1 ? b.min[0] : f === 2 ? b.max[2] : b.min[2];
      const a0 = alongZ ? b.min[2] : b.min[0], a1 = alongZ ? b.max[2] : b.max[0];
      const len = a1 - a0;
      if (len < MIN_LEN) continue;
      out.faces++;
      const n = Math.max(1, Math.ceil(len / SEG)), seg = len / n;
      for (let i = 0; i < n; i++) {
        const mid = a0 + seg * i + seg / 2;
        const px = alongZ ? fixedC + nx * 0.35 : mid, pz = alongZ ? mid : fixedC + nz * 0.35;
        out.segments++;
        if (ride) { out.ride++; continue; }
        if (doors.some(r => rectDist(px, pz, r) <= ROUTE_M)) { out.route++; continue; }
        if (hazards.some(r => rectDist(px, pz, r) <= HAZARD_M)) { out.hazard++; continue; }
        out.none++;
      }
    }
  }
  return out;
}

for (const f of process.argv.slice(2)) {
  const o = analyse(f);
  const kept = o.ride + o.route + o.hazard;
  console.log(`${o.level}: wall boxes ${o.walls} (ride panels ${o.rideBoxes}), faces ${o.faces}, segments ${o.segments}; marks: ride ${o.ride}, route ${o.route} (${o.doors} doors), hazard ${o.hazard} (${o.hazards} pits / lava); kept ${kept} (${o.segments ? Math.round(100 * kept / o.segments) : 0} %), no job ${o.none}`);
}

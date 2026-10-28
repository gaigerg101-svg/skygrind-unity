// NIGHT46 builder A3: line of sight from an eye to body centres in a BUILT level json (collision.boxes + wedges), sampled at 5 cm (the ramps approximated conservatively).
// usage: node los.cjs <level.json> <eyeX> <eyeZ> <eyeFeetY> "x,y,z;x,y,z;..."   (targets are feet; centre = feet + 1.0; eye = feet + 1.6)
// prints per target: distance, bearing from -z (deg), LOS yes / the first blocking box.
const fs = require("fs");
const j = JSON.parse(fs.readFileSync(process.argv[2], "utf8"));
const ex = +process.argv[3], ez = +process.argv[4], ey = +process.argv[5] + 1.6;
const targets = process.argv[6].split(";").map((s) => s.split(",").map(Number));
const boxes = j.collision.boxes.filter((b) => b.kind !== "floor");
const wedges = j.collision.wedges || [];
function inWedge(w, x, y, z) {
  // a ramp: footprint length along yaw (deg: 0 = toward -z?), width across; rise from 0 at its start to `rise` at its end. Approximate: test both
  // orientations' footprints and use the max height (conservative: blocks more).
  const c = Math.cos((w.yaw || 0) * Math.PI / 180), s = Math.sin((w.yaw || 0) * Math.PI / 180);
  const dx = x - w.x, dz = z - w.z;
  const u = dx * s + dz * c, v = dx * c - dz * s; // u along, v across (guess)
  for (const [uu, vv] of [[u, v], [-u, v], [v, u], [-v, u]]) {
    if (Math.abs(vv) <= w.width / 2 && uu >= -w.length / 2 && uu <= w.length / 2) {
      const h = w.rise * (uu + w.length / 2) / w.length;
      if (y <= Math.max(h, w.rise - h) && y >= 0) return true; // conservative
    }
  }
  return false;
}
for (const t of targets) {
  const tx = t[0], ty = t[1] + 1.0, tz = t[2];
  const d = Math.hypot(tx - ex, tz - ez);
  const bearing = Math.atan2(tx - ex, -(tz - ez)) * 180 / Math.PI;
  const n = Math.ceil(Math.hypot(d, ty - ey) / 0.05);
  let hit = null;
  for (let i = 1; i < n && !hit; i++) {
    const a = i / n, x = ex + (tx - ex) * a, y = ey + (ty - ey) * a, z = ez + (tz - ez) * a;
    for (const b of boxes) if (x > b.min[0] && x < b.max[0] && y > b.min[1] && y < b.max[1] && z > b.min[2] && z < b.max[2]) { hit = "box " + JSON.stringify(b.min) + "-" + JSON.stringify(b.max) + " " + (b.kind || ""); break; }
    if (!hit) for (const w of wedges) if (inWedge(w, x, y, z)) { hit = "wedge " + JSON.stringify(w); break; }
  }
  console.log(`[${t}] d ${d.toFixed(1)} m, bearing ${bearing.toFixed(1)} deg${Math.abs(bearing) > 53 ? " (OUT of view)" : ""}: ${hit ? "NO LOS (" + hit + ")" : "LOS yes"}`);
}

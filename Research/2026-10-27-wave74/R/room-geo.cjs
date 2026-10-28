// NIGHT74 R: a room file's geometry on one screen (read only). Usage: node Research/2026-10-27-wave74/R/room-geo.cjs <file.json> [<file.json> ...]
// Boxes / ramps / pads / pits / tunnels with their extents and tops, the spawn / exit / waves / nodes, the floor planes (distinct tops of walkable pieces)
// and the cover classes (a free-standing solid's height: high >= 1.75, low 1.0-1.25, anything else listed as "between").
const fs = require("fs");
const fmt = (a) => "[" + a.map((v) => Math.round(v * 100) / 100).join(", ") + "]";
for (const file of process.argv.slice(2)) {
  const f = JSON.parse(fs.readFileSync(file, "utf8"));
  const r = f.row;
  console.log("== " + file + ": " + r.id + " " + r.name + " rect " + fmt(r.rect) + " height " + r.height + " roof " + r.roof + " par " + r.par);
  console.log("  spawn " + JSON.stringify(r.spawn) + " exit " + fmt(r.exit) + " waves " + JSON.stringify(r.waves));
  const tops = new Map();
  for (const b of f.boxes || []) {
    const h = b.max[1] - b.min[1], w = b.max[0] - b.min[0], d = b.max[2] - b.min[2];
    const cls = b.kind === "deck" ? "deck" : h >= 1.75 ? "HIGH" : h >= 1.0 && h <= 1.25 ? "low" : "between";
    console.log("  box " + b.kind + " " + fmt(b.min) + ".." + fmt(b.max) + "  " + w.toFixed(1) + " x " + d.toFixed(1) + " x " + h.toFixed(1) + " [" + cls + "] " + (b.job || "") + (b.ride ? " RIDE" : ""));
    if (b.kind === "deck" || (b.max[1] - b.min[1]) < 4) tops.set(b.max[1], (tops.get(b.max[1]) || 0) + 1);
  }
  for (const q of f.ramps || []) console.log("  ramp " + fmt(q.from) + " -> " + fmt(q.to) + " w " + q.width + " rise " + q.rise + " " + (q.job || ""));
  for (const q of f.pads || []) console.log("  pad at " + fmt(q.at) + " -> " + fmt(q.target) + " apex+" + q.apexAbove + " " + (q.job || ""));
  for (const q of f.pits || []) console.log("  pit rect " + fmt(q.rect) + " floorY " + q.floorY + (q.lava ? " LAVA" : "") + " " + (q.job || ""));
  for (const q of f.tunnels || []) console.log("  tunnel rect " + fmt(q.rect) + " ceiling " + q.ceiling);
  if (r.waveNodes43) r.waveNodes43.forEach((w, i) => console.log("  wave " + (i + 1) + " nodes " + w.map(fmt).join(" ")));
  if (r.standIns) console.log("  standIns " + r.standIns.map((s) => fmt(s.at)).join(" "));
  console.log("  tops of walkable pieces: " + [...tops.keys()].sort((a, b) => a - b).join(", ") + " (floor 0)");
}

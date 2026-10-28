// NIGHT74 R: what a room file changed against n52's room (read only). Usage: node Research/2026-10-27-wave74/R/room-diff.cjs <id> [<id> ...]
// Prints, for each id: the row keys that differ (waves, par, lessons, nodes ...), the piece counts per list and the floor planes of the boxes.
const fs = require("fs");
const j = (x) => String(JSON.stringify(x));
for (const id of process.argv.slice(2)) {
  const n = JSON.parse(fs.readFileSync("Design/levels/n65/rooms/" + id + ".json", "utf8"));
  const o = JSON.parse(fs.readFileSync("Design/levels/n65/rooms-n52/" + id + ".json", "utf8"));
  console.log("== " + id + " " + n.row.name);
  const keys = [...new Set(Object.keys(n.row).concat(Object.keys(o.row)))];
  for (const k of keys) {
    if (j(n.row[k]) === j(o.row[k])) continue;
    if (/_why$|^frames|^line65|^proofs|^card65/.test(k)) { console.log("  ~ " + k + " differs (doc / tour key)"); continue; }
    console.log("  * " + k + ": new " + j(n.row[k]).slice(0, 150) + " | old " + j(o.row[k]).slice(0, 150));
  }
  for (const k of ["boxes", "ramps", "pads", "grapple", "pits", "tunnels", "lights", "props", "teach35"]) {
    const a = (n[k] || []).length, b = (o[k] || []).length;
    console.log("  " + k + ": new " + a + ", old " + b);
  }
}

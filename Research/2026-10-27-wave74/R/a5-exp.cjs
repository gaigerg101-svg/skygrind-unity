// NIGHT74 R: variants of a5's room file for the guards (diagnosis; the winner is written into rooms/a5.json by hand with the Edit tool).
// Usage: node Research/2026-10-27-wave74/R/a5-exp.cjs <name[+name]> <in.json> <out.json>
const fs = require("fs");
const [name, inp, out] = process.argv.slice(2);
const f = JSON.parse(fs.readFileSync(inp, "utf8"));
const r = f.row;
const E = {
  base() {},
  // the old room had three pads, the NIGHT65 file one (the guard "nothing lost: pads at least the red's"): two with jobs
  pads2() {
    f.pads.push({ zone: "a5", at: [3.5, 2.5, -142.8], target: [-8.4, 0, -133], apexAbove: 2, job: "pad",
      why: "THE LADLE RETURN (NIGHT74 R): from THE GANTRY's west end back over THE POUR onto THE WEST RIM: the way off the high ground that is not the drop into the rail lane, and it closes the pad route (apron, pour pad, gantry, return pad, rim, apron)" });
    f.pads.push({ zone: "a5", at: [4.5, 0, -119.3], target: [3, 3, -128], apexAbove: 1.5, job: "pad",
      why: "THE BLOCK KICK (NIGHT74 R): from the south floor's east side onto THE TAP BLOCK's 3 m top, over INGOT S: a player-only high ground over THE APRON and THE POUR's lip (walkers cannot climb 3 m), where the slide-launch's targets stand below and the spitter's gantry is level across the pour" });
  },
  pad2only() { E.pads2(); f.pads.pop(); },
  // the first try's I4 FAIL (a pad within 2 m of a proof walk line): the return pad on the gantry's east end, the lift on the apron
  pads2b() {
    f.pads.push({ zone: "a5", at: [8, 2.5, -141.8], target: [-3, 0, -132.4], apexAbove: 2, job: "pad",
      why: "THE LADLE RETURN (NIGHT74 R): from THE GANTRY's east end back over THE POUR onto THE APRON, 3 m north of THE TAP BEAM's mouth: the way off the high ground that is not the drop into the rail lane, a second pass at wave 2's apron holders, and it closes the pad route (apron, pour pad, gantry, return pad, apron)" });
    f.pads.push({ zone: "a5", at: [0.5, 0, -131.3], target: [2.5, 3, -128], apexAbove: 1.5, job: "pad",
      why: "THE APRON LIFT (NIGHT74 R): from THE APRON onto THE TAP BLOCK's 3 m top: after the slide-launch a short way up to a player-only high ground over THE APRON and THE POUR's lip (walkers cannot climb 3 m), 2.7 m east of the slide's axis so a slide out of the slot never rides it" });
  },
};
for (const n of name.split("+")) {
  if (!E[n]) { console.error("unknown experiment " + n + " (" + Object.keys(E).join(", ") + ")"); process.exit(2); }
  E[n]();
}
fs.writeFileSync(out, JSON.stringify(f, null, 1) + "\n");
console.log("wrote " + out + " (" + name + ")");

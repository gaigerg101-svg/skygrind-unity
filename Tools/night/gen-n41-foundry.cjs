// NIGHT41 builder R5 (foundry41; NIGHT41.md R5, INVENTORY A12, DECIDE41 D-2 = DECIDE40 D-14): level 1 as n41-foundry. It is n38-foundry's plan
// (Design/levels/n38/n38-foundry.plan.json, written by Tools/night/gen-n36-foundry.cjs --n38, which this file does not run or edit) copied whole,
// with NIGHT41's change applied on the copy, so `--off41` keeps n38-foundry bit for bit:
//   1. the id `n41-foundry` and its _why
//   2. a NO-RIDE BAND on THE GANTRY's (b2) long west ride wall W1, gate by gate: NIGHT40 R2's checker (`n38 gates --ride40`) measured gate b2-g1
//      crossed by ONE endless ride along W1 (the climb up to the ceiling, then the release: +156 %), so the chain of walls (W1 -> W2, faces
//      alternating) was optional. W1 keeps its ride panel from its south end to RIDE_TO m past the gate's lip; the rest of its length becomes a
//      `noride` wall (the existing level-format flag, Design/levels/n38/GATES38.md: the Mover never starts or turns a ride on it, the look is dull
//      with no cyan trim; levelgen hands it to the sim's boxes as n38-span's liners). W2 (from 20 m past the lip) is untouched, so a ride on W1
//      that jumps across to W2 still crosses: the chain is needed again.
// The band's length comes from the checker (`n38 gates Design/levels/n41/foundry.plan.json --ride40`): RIDE_TO is the farthest W1 end at which no
// single-wall try crosses and the ride chain still does (N41F_RIDE_TO overrides it for a probe; N41F_GATES picks the gates, default both).
// Run from the repo (or worktree) root: bash Tools/night/n41-foundry-regen.sh
const path = require("path");
const fs = require("fs");

const ROOT = path.join(__dirname, "../..");
const SRC = path.join(ROOT, "Design/levels/n38/n38-foundry.plan.json");
const OUT = path.join(ROOT, "Design/levels/n41/foundry.plan.json");
/// m past the gate's lip where W1's ride panel ends (the rest of W1 is the no-ride band): measured, see the header
const RIDE_TO = Number(process.env.N41F_RIDE_TO || 22);
/// m the band stands out from W1's face into the lane (a buttress; see the band below); N41F_PROUD overrides it for a probe
const PROUD = Number(process.env.N41F_PROUD || 0.6);
/// m: the side-wall liners' thickness (proud of the room's wall), as the ride panels' 0.6 m (n36-lib panel)
const LINER = 0.6;
const GATES = (process.env.N41F_GATES || "b2-g1,b2-g2").split(",");
/// PROBES ONLY (never the shipped file; the script refuses to write the plan with them unless N41F_OUT names another file): N41F_NO_BAND=1 keeps
/// W1 whole; N41F_NO_W2=1 takes each gate's W2 out, so the checker answers "does W1 ALONE cross?" (the chain is needed when it does not)
const NO_BAND = process.env.N41F_NO_BAND === "1", NO_W2 = process.env.N41F_NO_W2 === "1";
const OUT_FILE = process.env.N41F_OUT ? path.resolve(ROOT, process.env.N41F_OUT) : OUT;
if ((NO_BAND || NO_W2) && OUT_FILE === OUT) throw new Error("a probe (N41F_NO_BAND / N41F_NO_W2) must write to N41F_OUT, not the shipped plan");

const plan = JSON.parse(fs.readFileSync(SRC, "utf8"));
if (plan.id !== "n38-foundry") throw new Error(`unexpected source id ${plan.id}`);
plan.id = "n41-foundry";
plan._why = "NIGHT41 builder R5 (design.n41.foundry41 swaps n38-foundry for this file while the row is on): Tools/night/gen-n41-foundry.cjs copies "
  + `n38-foundry's plan whole and changes only: THE GANTRY's west ride wall W1 at gates ${GATES.join(" / ")} rides to ${RIDE_TO} m past the lip, the rest `
  + "of it a noride band, so the endless ride needs the chain of walls again (DECIDE41 D-2). n38-foundry's _why: " + plan._why;

const b2 = plan.rooms.find((r) => r.id === "b2");
if (!b2) throw new Error("room b2 (THE GANTRY) missing");
const changed = [];
for (const gid of GATES) {
  const land = plan.boxes.find((b) => b.zone === "b2" && b.gate && b.gate.id === gid);
  if (!land) throw new Error(`gate ${gid} missing`);
  const lipZ = land.gate.from[2] - 0.5; // the plan's `from` is 0.5 m short of the lip (n38f-foundry-rooms.cjs: lz0 - 0.5), the travel toward -z
  const landZ = land.max[2]; // the landing's near edge
  // W1: the west ride panel (its east face) whose span overlaps this gate's pit
  const wi = plan.boxes.findIndex((b) => b.zone === "b2" && b.ride && b.ride36 && b.ride36.face === "E" && b.max[2] > landZ && b.min[2] < lipZ);
  if (wi < 0) throw new Error(`gate ${gid}: W1 missing`);
  const w1 = plan.boxes[wi];
  if (NO_W2) {
    const w2i = plan.boxes.findIndex((b) => b.zone === "b2" && b.ride && b.ride36 && b.ride36.face === "W" && b.max[2] > landZ && b.min[2] < lipZ);
    if (w2i < 0) throw new Error(`gate ${gid}: W2 missing`);
    plan.boxes.splice(w2i, 1);
    changed.push(`${gid}: PROBE W2 out`);
  }
  if (NO_BAND) { changed.push(`${gid}: PROBE no band`); continue; }
  const cutZ = lipZ - RIDE_TO; // world z where the ride panel ends
  if (!(cutZ < w1.max[2] && cutZ > w1.min[2])) throw new Error(`gate ${gid}: the cut z ${cutZ} is not inside W1 (${w1.min[2]}..${w1.max[2]})`);
  const farZ = w1.min[2];
  const bandLen = +(cutZ - farZ).toFixed(2);
  // the ride panel: W1 from its south end (z max) to the cut
  w1.min = [w1.min[0], w1.min[1], cutZ];
  const lz = w1.ride36.lz;
  w1.ride36 = Object.assign({}, w1.ride36, { lz: [lz[0], +(lz[1] - bandLen).toFixed(2)] });
  w1.why = w1.why + ` NIGHT41 R5 (foundry41): it ends ${RIDE_TO} m past the lip; the noride band carries on to its old end.`;
  w1.note = w1.why;
  // the band: the same slab, noride, no ride tag (dull, no cyan trim)
  const why = `gate ${gid}'s NO-RIDE BAND (NIGHT41 R5, foundry41; DECIDE41 D-2): W1's last ${bandLen} m (from ${RIDE_TO} m past the lip to its old end), `
    + "dull, ridged, no cyan: the endless ride crossed the gate on W1 alone (NIGHT40 R2's checker, +156 %); here the ride must jump across to W2 (the chain)";
  // the band STANDS PROUD of W1's face by PROUD m (a buttress), so its south end is a corner the ride meets: the Mover's noride rule ends a ride at a
  // corner onto a noride face and starts none on it (Mover.cs Ride35Turn / the entry); a band flush with W1's face would be ridden straight on
  // (measured: n38 gates --ride40 on a probe with W2 out, a flush 4 m band left W1 alone crossing at +31 % / +26 %, as with no band)
  // ... on BOTH faces: W1's back (west) face is a wall too (measured: with only the east face proud, a ride on W1's back face crossed alone, +31 %)
  const bx0 = w1.min[0] - PROUD, bx1 = w1.max[0] + PROUD;
  const band = { zone: "b2", kind: "wall", min: [bx0, w1.min[1], farZ], max: [bx1, w1.max[1], cutZ], job: "line", why: why + `; it stands ${PROUD} m proud of both of W1's faces (a buttress: a ride on either face meets its end face as a corner)`, noride: true };
  band.note = band.why;
  plan.boxes.splice(wi + 1, 0, band);
  // the room's own side walls over the pit: a LINER noride on each (n38-span's FLIGHT liners, the same flag), proud of the wall by LINER m so a ride
  // along the room's wall from the takeoff floor meets its end as a corner. Measured (`n38 gates --ride40 --trace` on a probe with W2 out and the band
  // in): a try that dropped into pit 1 started a ride on the room's WEST wall at y -2.8 and the endless ride climbed out along it onto the landing
  // (+31 % / +26 %): one ride on one wall, the chain skipped.
  const [rx0, , rx1] = b2.rect;
  for (const [a, b, side] of [[rx0, rx0 + LINER, "west"], [rx1 - LINER, rx1, "east"]]) {
    const lw = `gate ${gid}'s ${side} NORIDE LINER (NIGHT41 R5, foundry41): the room's ${side} wall over the pit, pit floor to the roof, dull, no cyan: `
      + "the endless ride along the room's own wall crossed the gate alone (measured); only the chain of ride walls crosses";
    plan.boxes.push({ zone: "b2", kind: "wall", min: [a, land.min[1], landZ], max: [b, 14, lipZ], job: "line", why: lw, note: lw, noride: true });
  }
  changed.push(`${gid}: W1 rides z ${w1.max[2]}..${cutZ} (${RIDE_TO} m past the lip at ${lipZ}), noride band z ${cutZ}..${farZ} (${bandLen} m)`);
}
b2.foundry41_why = "R5 (foundry41): W1's no-ride band at each gate: " + changed.join("; ");

fs.mkdirSync(path.dirname(OUT_FILE), { recursive: true });
fs.writeFileSync(OUT_FILE, JSON.stringify(plan, null, 1) + "\n");
console.log("wrote", path.relative(ROOT, OUT_FILE), "|", changed.join(" | "));

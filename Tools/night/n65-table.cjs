// NIGHT65 (the lead's): the per-room table of one sim round against the red (branch C: the numbers a review and a GATE read). For each slot
// room it reads <dir>/<id>/{room-check,flow-A,flow-B,pads,ride}.txt (Tools/night/n65-red.sh's files) and the same files under the red
// folder, and prints one markdown row per room: the room check's verdict line, air % A / B, stall % A / B, STYLE's DULL % A / B and the
// top rank, deaths A / B, clear s A / B, the pads PASS / total, the ride pilot (a3) - each "round (red)".
//   node Tools/night/n65-table.cjs <dir> [<reddir>=Logs/n65/red]
const fs = require("fs");
const path = require("path");

const dir = process.argv[2];
const red = process.argv[3] || "Logs/n65/red";
if (!dir) { console.log("usage: node Tools/night/n65-table.cjs <dir> [<reddir>]"); process.exit(1); }
const ROOMS = ["a2", "a3", "a5", "b1", "a8", "a4"];

function read(d, id, f) {
  const p = path.join(d, id, f);
  return fs.existsSync(p) ? fs.readFileSync(p, "utf8") : null;
}
function flow(d, id, s) {
  const t = read(d, id, "flow-" + s + ".txt");
  if (!t) return null;
  const sum = (t.match(/SUMMARY: (.*)/) || [])[1] || "";
  const num = (re) => { const m = sum.match(re); return m ? m[1] : "?"; };
  const lines = t.split(/\r?\n/), hi = lines.findIndex((l) => /^\s+style\s+kind\s+\|/.test(l));
  const st = (hi >= 0 ? lines.slice(hi + 1).find((l) => new RegExp("^\\s+" + id + "\\s+room\\s+\\|").test(l)) : "") || "";
  const sm = st.match(/\|\s*([0-9.]+)\s+([0-9.]+)\s+([0-9.]+)\s+([0-9.]+)\s+([0-9.]+)\s+([0-9.]+)\s*\|\s*(.*)$/);
  return {
    verdict: (sum.match(/^(\w+ \d+\/\d+)/) || [])[1] || "?",
    air: num(/air ([0-9.]+) %/), stall: num(/stall ([0-9.]+) %/), deaths: num(/deaths (\d+)/), clear: num(/clear ([0-9.]+) s/),
    dull: sm ? sm[1] : "?", top: sm ? sm[7].trim() : "?",
  };
}
function check(d, id) {
  const t = read(d, id, "room-check.txt");
  if (!t) return "?";
  return ((t.match(new RegExp("^ROOM " + id + ": (.*)$", "m")) || [])[1] || "?").trim();
}
function pads(d, id) {
  const t = read(d, id, "pads.txt");
  if (!t) return "?";
  const all = (t.match(/^pad52 .* (PASS|FAIL)\s*$/gm) || []);
  if (/no jump pad inside/.test(t) && !all.length) return "none";
  return all.filter((l) => / PASS\s*$/.test(l)).length + " / " + all.length;
}
function ride(d, id) {
  const t = read(d, id, "ride.txt");
  if (!t) return "";
  return ((t.match(/n53ridekill ride: (\w+)/) || [])[1] || "?");
}
const both = (a, b) => a + " (" + b + ")";
console.log("| room | room check (red) | air % A / B (red) | stall % A / B (red) | DULL % A / B (red) | top rank A (red) | deaths A / B (red) | clear s A / B (red) | pads (red) | ride (red) |");
console.log("|---|---|---|---|---|---|---|---|---|---|");
for (const id of ROOMS) {
  const A = flow(dir, id, "A"), B = flow(dir, id, "B"), rA = flow(red, id, "A"), rB = flow(red, id, "B");
  if (!A || !B) { console.log("| " + id + " | not run |  |  |  |  |  |  |  |  |"); continue; }
  const ab = (k) => A[k] + " / " + B[k] + " (" + (rA ? rA[k] : "?") + " / " + (rB ? rB[k] : "?") + ")";
  console.log("| " + id + " | " + both(check(dir, id), check(red, id)) + " | " + ab("air") + " | " + ab("stall") + " | " + ab("dull") + " | " +
    both(A.top, rA ? rA.top : "?") + " | " + ab("deaths") + " | " + ab("clear") + " | " + both(pads(dir, id), pads(red, id)) + " | " +
    (id === "a3" ? both(ride(dir, id), ride(red, id)) : "") + " |");
  if (A.verdict !== "GREEN 10/10" || B.verdict !== "GREEN 10/10") console.log("|  | n46flow: " + A.verdict + " / " + B.verdict + " (a clear short of 10 / 10 is a GUARD failure) |  |  |  |  |  |  |  |  |");
}

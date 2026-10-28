// NIGHT70 (the lead's; FORGE66's copy of n70-table.cjs, its red team's findings 1 and 3: the reds under Logs/n70, the slot rooms from
// slots65.json, the verb-aware flow V's stall column, the ride pilot in b2 too): the per-room table of one sim round against the red (branch C: the numbers a review and a GATE read). For each slot
// room it reads <dir>/<id>/{room-check,flow-A,flow-B,pads,ride}.txt (Tools/night/n70-red.sh's files) and the same files under the red
// folder, and prints one markdown row per room: the room check's verdict line, air % A / B, stall % A / B, STYLE's DULL % A / B and the
// top rank, deaths A / B, clear s A / B, the pads PASS / total, the ride pilot (a3) - each "round (red)".
//   node Tools/night/n70-table.cjs <dir> [<reddir>=Logs/n70/red]
const fs = require("fs");
const path = require("path");

const dir = process.argv[2];
if (!dir) { console.log("usage: node Tools/night/n70-table.cjs <dir> [<reddir>]"); process.exit(1); }
// NIGHT70 T1b (slate T1b line 1): the level of the round: L67, else the round folder's assembly record's level (gen-slots.cjs writes it); an
// L67 level's red is Logs/n70/red-<L> (NIGHT70.md bearings step 6), level 1's Logs/n70/red; the assembly is any <L>.assembly.json in <dir>
const asmFile = fs.existsSync(dir) ? fs.readdirSync(dir).filter((f) => /\.assembly\.json$/.test(f)).map((f) => path.join(dir, f))[0] : null;
const asmLevel = asmFile ? (JSON.parse(fs.readFileSync(asmFile, "utf8")).level || null) : null;
const L67 = (process.env.L67 && process.env.L67 !== "foundry" ? process.env.L67 : null) || (asmLevel && asmLevel !== "foundry" ? asmLevel : null);
const red = process.argv[3] || (L67 ? "Logs/n70/red-" + L67 : "Logs/n70/red");
// NIGHT70 T1c fix 2 (Logs/n70/verdict-T1c-r2.md): the rooms are the ROUND's, never the tracked slots65.json read now (a slot written while a
// round runs added a "not run" row): $ROOMS as n70-round.sh exported it, else the round folder's own assembly record (foundry.assembly.json,
// gen --out), else the round folder's room sub-folders that hold a room check
function roundRooms() {
  if (process.env.ROOMS && process.env.ROOMS.trim()) return { rooms: process.env.ROOMS.trim().split(/\s+/), from: "$ROOMS (the round's)" };
  const asm = asmFile || path.join(dir, "foundry.assembly.json");
  if (fs.existsSync(asm)) { const a = JSON.parse(fs.readFileSync(asm, "utf8")); if (Array.isArray(a.rooms) && a.rooms.length) return { rooms: a.rooms.map((r) => r.id), from: asm.replace(/\\/g, "/") }; }
  const subs = fs.readdirSync(dir).filter((d) => fs.existsSync(path.join(dir, d, "room-check.txt"))).sort();
  return { rooms: subs, from: dir.replace(/\\/g, "/") + "/<room>/room-check.txt" };
}
const RR = roundRooms();
const ROOMS = RR.rooms;

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
console.log("The round's rooms: " + ROOMS.join(" ") + " (from " + RR.from + "); level " + (L67 || "foundry") + "; the red " + red + (fs.existsSync(red) ? "" : " (MISSING: every red column reads ?)") + "\n");
console.log("| room | room check (red) | air % A / B (red) | stall % A / B (red) | stall % V (red) | DULL % A / B (red) | top rank A (red) | deaths A / B (red) | clear s A / B (red) | pads (red) | ride (red) |");
console.log("|---|---|---|---|---|---|---|---|---|---|---|");
for (const id of ROOMS) {
  const A = flow(dir, id, "A"), B = flow(dir, id, "B"), rA = flow(red, id, "A"), rB = flow(red, id, "B"), V = flow(dir, id, "V"), rV = flow(red, id, "V");
  if (!A || !B) { console.log("| " + id + " | not run |  |  |  |  |  |  |  |  |  |"); continue; }
  const ab = (k) => A[k] + " / " + B[k] + " (" + (rA ? rA[k] : "?") + " / " + (rB ? rB[k] : "?") + ")";
  console.log("| " + id + " | " + both(check(dir, id), check(red, id)) + " | " + ab("air") + " | " + ab("stall") + " | " + (V ? V.stall : "?") + " (" + (rV ? rV.stall : "?") + ") | " + ab("dull") + " | " +
    both(A.top, rA ? rA.top : "?") + " | " + ab("deaths") + " | " + ab("clear") + " | " + both(pads(dir, id), pads(red, id)) + " | " +
    (id === "a3" || id === "b2" || read(dir, id, "ride.txt") || read(red, id, "ride.txt") ? both(ride(dir, id), ride(red, id)) : "") + " |");   // NIGHT70 T1b: wherever a ride.txt exists
  if (A.verdict !== "GREEN 10/10" || B.verdict !== "GREEN 10/10") console.log("|  | n46flow: " + A.verdict + " / " + B.verdict + " (a clear short of 10 / 10 is a GUARD failure) |  |  |  |  |  |  |  |  |  |");
}

// FORGE72: every design row with enabled false, with the first 230 characters of its _why, into off-rows.txt (the harvest table's input).
//   node Research/2026-10-27-forge72/off-rows.cjs
const fs = require("fs");
const path = require("path");
const root = path.resolve(__dirname, "..", "..");
const d = JSON.parse(fs.readFileSync(path.join(root, "Design", "unity-config.json"), "utf8")).design;
const out = [];
for (const [k, v] of Object.entries(d)) {
  if (!v || typeof v !== "object") continue;
  for (const [r, x] of Object.entries(v)) {
    if (x && typeof x === "object" && x.enabled === false) {
      const w = String(x._why || x._note || x._what || "").replace(/\s+/g, " ");
      out.push(k + "." + r + " :: " + w.slice(0, 230));
    }
  }
}
fs.writeFileSync(path.join(__dirname, "off-rows.txt"), out.join("\n") + "\n");
console.log(out.length + " OFF rows");

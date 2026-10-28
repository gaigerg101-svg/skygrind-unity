// NIGHT52 the room loop (the lead's): copies n47's plans of the loop levels to Design/levels/n52/<level>.plan.json with only `id` changed
// (n47-<level> -> n52-<level>) and a `_why` line in front; refuses to overwrite. The rooms are then edited by the loop's own patch scripts.
//   node Tools/night/n52-loop-plans.cjs iceline spire span
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
for (const lv of process.argv.slice(2)) {
  const src = path.join(ROOT, "Design/levels/n47/" + lv + ".plan.json"), out = path.join(ROOT, "Design/levels/n52/" + lv + ".plan.json");
  if (fs.existsSync(out)) throw new Error(out + " exists");
  const text = fs.readFileSync(src, "utf8");
  const plan = JSON.parse(text);
  if (plan.id !== "n47-" + lv) throw new Error(src + ": id " + plan.id);
  plan.id = "n52-" + lv;
  plan._why = "NIGHT52 the room loop (design.n52." + lv + "52 swaps n47-" + lv + " for n52-" + lv + " while the row is on; NIGHT52.md section 5): n47's plan with the loop's rebuilt rooms (THE ROOM CONTRACT, section 2). Before it: " + plan._why;
  fs.mkdirSync(path.dirname(out), { recursive: true });
  fs.writeFileSync(out, JSON.stringify(plan, null, 1) + "\n");
  console.log("wrote " + path.relative(ROOT, out));
}

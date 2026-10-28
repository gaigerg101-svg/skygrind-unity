// NIGHT53 (the lead's): marks or adds a Tools/night/n53-queue.json item.
//   node Tools/night/n53-queue.cjs mark <id> <done|reverted|short|open> "<note: commit, log time>"
//   node Tools/night/n53-queue.cjs add <id> <owner> "<what>" [--after <id>]
//   node Tools/night/n53-queue.cjs list            (the open items, in order)
const fs = require("fs");
const path = require("path");
const file = path.join(__dirname, "n53-queue.json");
const d = JSON.parse(fs.readFileSync(file, "utf8"));
const [cmd, id, a, b, ...rest] = process.argv.slice(2);
if (cmd === "mark") {
  const it = d.items.find((x) => x.id === id);
  if (!it || !/^(done|reverted|short|open)$/.test(a || "")) throw new Error("usage: mark <id> <done|reverted|short|open> <note>");
  it.state = a;
  if (b) it.note = b;
} else if (cmd === "add") {
  if (d.items.some((x) => x.id === id) || !a || !b) throw new Error("usage: add <new id> <owner> <what> [--after <id>]");
  const item = { id, owner: a, state: "open", what: b };
  const ai = process.argv.indexOf("--after");
  const at = ai > 0 ? d.items.findIndex((x) => x.id === process.argv[ai + 1]) : -1;
  if (ai > 0 && at < 0) throw new Error("no item " + process.argv[ai + 1]);
  if (at >= 0) d.items.splice(at + 1, 0, item); else d.items.push(item);
} else if (cmd !== "list") throw new Error("usage: mark | add | list");
if (cmd !== "list") fs.writeFileSync(file, JSON.stringify(d, null, 2) + "\n");
for (const x of d.items) if (x.state === "open") console.log(x.id + " [" + x.owner + "] " + x.what.slice(0, 150));

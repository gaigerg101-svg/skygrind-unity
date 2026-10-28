// NIGHT53 the close (the lead's): puts the close table (Tools/night/n53-close.py's output, saved to Logs/n53/close-table.md) at the top
// of NIGHT53-LOG.md under a "## Close table" heading with the time, replacing an earlier one if present. The STOP POINT lines stay below.
//   python -I Tools/night/n53-close.py > Logs/n53/close-table.md && node Tools/night/n53-close-insert.cjs HH:MM
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const at = process.argv[2];
if (!/^\d\d:\d\d$/.test(at || "")) throw new Error("usage: n53-close-insert.cjs HH:MM");
const table = fs.readFileSync(path.join(ROOT, "Logs/n53/close-table.md"), "utf8").replace(/\r\n/g, "\n").trim();
if (!table.startsWith("| row |")) throw new Error("Logs/n53/close-table.md is not n53-close.py's table");
const logPath = path.join(ROOT, "NIGHT53-LOG.md");
let log = fs.readFileSync(logPath, "utf8");
const eol = log.includes("\r\n") ? "\r\n" : "\n";
const lines = log.split(/\r?\n/);
let start = lines.findIndex((l) => l.startsWith("## Close table"));
if (start >= 0) {
  let end = start + 1;
  while (end < lines.length && !lines[end].startsWith("**STOP POINT")) end++;
  lines.splice(start, end - start);
}
const block = ["## Close table (" + at + ", Tools/night/n53-close.py over Tools/night/n53-rows.json)", "", ...table.split("\n"), ""];
lines.splice(2, 0, ...block);
fs.writeFileSync(logPath, lines.join(eol));
console.log("close table inserted at the top of NIGHT53-LOG.md (" + table.split("\n").length + " lines)");

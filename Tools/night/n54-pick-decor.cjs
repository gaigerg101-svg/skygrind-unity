// NIGHT54 (the lead): resolves builder D's config conflict at the pick (D adds design.n54.decor54 after move54; M changed move54's numbers on
// the adjacent lines). Takes HEAD's config (every other row as integrated) and inserts D's decor54 object exactly as D's branch tip has it,
// right after move54, keeping the file's 2-space formatting and its line endings. Asserts nothing else changes; writes both copies.
//   node Tools/night/n54-pick-decor.cjs <D's tip commit>
const fs = require("fs");
const { execSync } = require("child_process");
const tip = process.argv[2];
if (!tip) throw new Error("usage: n54-pick-decor.cjs <commit>");
const head = execSync("git show HEAD:Design/unity-config.json", { encoding: "utf8", maxBuffer: 1 << 28 });
const theirs = JSON.parse(execSync("git show " + tip + ":Design/unity-config.json", { encoding: "utf8", maxBuffer: 1 << 28 }));
const decor = theirs.design.n54.decor54;
if (!decor) throw new Error("no decor54 at " + tip);
const d = JSON.parse(head);
if (d.design.n54.decor54) throw new Error("HEAD already has decor54");
const eol = head.includes("\r\n") ? "\r\n" : "\n";
const lines = head.split(eol);
const start = lines.findIndex((l) => l === '    "n54": {');
if (start < 0) throw new Error("no n54 block");
const mv = lines.findIndex((l, i) => i > start && l === '      "move54": {');
if (mv < 0) throw new Error("no move54 row");
const end = lines.findIndex((l, i) => i > mv && (l === "      }" || l === "      },"));
if (end < 0) throw new Error("move54's end not found");
const block = JSON.stringify(decor, null, 2).split("\n").map((l, i) => (i === 0 ? '      "decor54": ' + l : "      " + l));
const out = lines.slice(0, end).concat([lines[end].replace(/,?$/, ",")], block.map((l, i) => (i === block.length - 1 && lines[end] === "      }," ? l + "," : l)), lines.slice(end + 1)).join(eol);
const back = JSON.parse(out);
if (JSON.stringify(back.design.n54.decor54) !== JSON.stringify(decor)) throw new Error("decor54 did not round-trip");
const a = JSON.parse(out); delete a.design.n54.decor54;
if (JSON.stringify(a) !== JSON.stringify(d)) throw new Error("something beyond decor54 changed");
fs.writeFileSync("Design/unity-config.json", out);
fs.writeFileSync("Assets/StreamingAssets/Design/unity-config.json", out);
console.log("decor54 inserted after move54 (" + Object.keys(decor.numbers || {}).length + " number keys); both copies written");

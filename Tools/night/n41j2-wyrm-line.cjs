// NIGHT41 builder J2 (the lead's n41-r3 note: THE KILN WYRM's beat had no rule line, Entrance41Probe Line=''): adds
// design.n41.entrance41.lines.wyrm (its bossRise id is "wyrm") and extends lines_why; design.n41.entrance41 ONLY (L-049). The line is from
// Core/Enemies/Wyrm.cs's damage rule (one HP pool: a body hit x1, the head x2, the exposed core x3). Copies the config to
// Assets/StreamingAssets/Design/ (byte-identical before and after). Refuses to run twice. Run from the worktree root.
const fs = require("fs");
const path = require("path");
const root = path.resolve(__dirname, "../..") + "/";
const file = root + "Design/unity-config.json";
const copy = root + "Assets/StreamingAssets/Design/unity-config.json";
let raw = fs.readFileSync(file, "utf8");
if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ before the patch");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const d0 = JSON.parse(raw);
const row = d0.design.n41.entrance41;
if (!row || !row.lines) throw new Error("entrance41.lines missing");
if (row.lines.wyrm) throw new Error("lines.wyrm already present");
const line = "SHOOT ITS HEAD. THE OPEN CORE HITS x3";

const brawl = JSON.stringify("brawl40") + ": " + JSON.stringify(row.lines.brawl40);
const i = raw.indexOf(brawl, raw.indexOf('"entrance41": {'));
if (i < 0) throw new Error("lines.brawl40 not found");
const lineStart = raw.lastIndexOf(eol, i) + eol.length;
const ind = raw.slice(lineStart, i);
raw = raw.slice(0, i + brawl.length) + "," + eol + ind + JSON.stringify("wyrm") + ": " + JSON.stringify(line) + raw.slice(i + brawl.length);

const oldWhy = JSON.stringify(row.lines_why);
const newWhy = JSON.stringify(row.lines_why + " wyrm (THE KILN WYRM, n41-r3): written from Wyrm.cs's damage rule (one pool: body x1, head x2, the exposed core x3).");
const j = raw.indexOf('"lines_why": ' + oldWhy, raw.indexOf('"entrance41": {'));
if (j < 0) throw new Error("lines_why not found");
raw = raw.slice(0, j) + '"lines_why": ' + newWhy + raw.slice(j + ('"lines_why": ' + oldWhy).length);

const d1 = JSON.parse(raw);
if (d1.design.n41.entrance41.lines.wyrm !== line) throw new Error("wyrm line mismatch");
delete d1.design.n41.entrance41.lines.wyrm;
d1.design.n41.entrance41.lines_why = row.lines_why;
if (JSON.stringify(d1) !== JSON.stringify(d0)) throw new Error("something else changed");
fs.writeFileSync(file, raw);
fs.copyFileSync(file, copy);
console.log("entrance41.lines.wyrm = " + line);

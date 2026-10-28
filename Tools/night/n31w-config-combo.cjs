// NIGHT31 builder W: adds design.n31.charge.comboNails (item 2c, the cross-weapon combo, its own sub-switch inside n31.charge) to
// Design/unity-config.json. The web has no charge shot and no combo: a Unity-side row (CLAUDE.md rule 5). Run from the repo (or worktree) root:
//   node Tools/night/n31w-config-combo.cjs     (refuses to run twice; inserts after the charge block's meleeCancels_why line)
const fs = require("fs");
const path = require("path");
const file = path.resolve(__dirname, "..", "..", "Design", "unity-config.json");
const raw = fs.readFileSync(file, "utf8");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const d = JSON.parse(raw);
const c = d.design && d.design.n31 && d.design.n31.charge;
if (!c || c.meleeCancels_why === undefined) throw new Error("design.n31.charge has no numbers yet (run n31w-config-charge.cjs first)");
if (c.comboNails) throw new Error("design.n31.charge.comboNails already exists");
const anchor = '        "meleeCancels_why": ' + JSON.stringify(c.meleeCancels_why) + "," + eol;
const at = raw.indexOf(anchor);
if (at < 0 || raw.indexOf(anchor, at + 1) >= 0) throw new Error("the meleeCancels_why line is not where it was expected (once)");
const combo = {
  _why: "Item 2c (DESIGN31 A, the cross-weapon combo; R3: ULTRAKILL's coin and core): a charged KESTREL shot that passes through a body carrying stuck NAILGUN nails detonates them at mult x the nails' detonate damage (the nails go off before the shot's own hit, so they also go off on a kill). Its own switch inside n31.charge (cut first); 1.5 is a guess.",
  enabled: true,
  mult: 1.5,
};
const text = JSON.stringify(combo, null, 2).split("\n").map((l, i) => (i === 0 ? l : "        " + l)).join(eol);
const out = raw.slice(0, at + anchor.length) + '        "comboNails": ' + text + "," + eol + raw.slice(at + anchor.length);
fs.writeFileSync(file, out);
const back = JSON.parse(fs.readFileSync(file, "utf8")).design.n31.charge.comboNails;
console.log("design.n31.charge.comboNails: enabled " + back.enabled + ", mult " + back.mult);

// NIGHT31 builder W: adds design.n31.charge.shotgun.slug (round 2: the THUNDERCLAP's charged shot as the one-line SLUG of DESIGN31 A) to Design/unity-config.json.
// Unity-side (the web has no charge shot). Run from the repo (or worktree) root: node Tools/night/n31w-config-slug.cjs   (refuses to run twice)
const fs = require("fs");
const path = require("path");
const file = path.resolve(__dirname, "..", "..", "Design", "unity-config.json");
const raw = fs.readFileSync(file, "utf8");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const d = JSON.parse(raw);
const sg = d.design && d.design.n31 && d.design.n31.charge && d.design.n31.charge.shotgun;
if (!sg) throw new Error("design.n31.charge.shotgun missing");
if (sg.slug !== undefined) throw new Error("design.n31.charge.shotgun.slug already exists");
const anchor = '          "knockMult": 2,' + eol + '          "cooldownSeconds": 6' + eol;
const at = raw.indexOf(anchor);
if (at < 0 || raw.indexOf(anchor, at + 1) >= 0) throw new Error("the shotgun block's tail is not where it was expected (once)");
const add = '          "knockMult": 2,' + eol + '          "cooldownSeconds": 6,' + eol +
  '          "slug": true,' + eol +
  '          "slug_why": ' + JSON.stringify("DESIGN31 A: the charged THUNDERCLAP is a SLUG: one pellet line along the aim (no spread), the first body before a wall takes a whole volley's damage (pellets x pellet damage) x damageMult (3) with the forward knock x knockMult (husks only, R7). false: NIGHT27's charged pellet volley.") + eol;
fs.writeFileSync(file, raw.slice(0, at) + add + raw.slice(at + anchor.length));
console.log("design.n31.charge.shotgun.slug: " + JSON.parse(fs.readFileSync(file, "utf8")).design.n31.charge.shotgun.slug);

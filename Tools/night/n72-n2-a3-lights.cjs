// NIGHT72 builder N (the lead's call, 2026-10-27: "a roofless a3 keeps no ceiling light", GG 2026-10-11 lights "placed and floating"): the three kind-"ceiling" lights of level 1's a3 (a3-L1, a3-L2, a3-L4, y 9.95)
// hung from the roof slab 0e3f95ea5 removed; with nothing above them they float (levelgen's lights row read them "not flush to a surface"). N0c took them out of n65-foundry (gen-n65-foundry.cjs); this takes
// them out of the n52 plan itself, so the shipped n52-foundry (the `foundry65:0` side) loses them too, in place, like n72-n2-a3-ring.cjs. Then: levelgen of the n52 plan,
// `node Tools/night/gen-n52-foundry.cjs --finish`, `node Tools/night/gen-n65-foundry.cjs --export a3` and `--run --slots=b1,a8`.
// Idempotent; refuses a plan whose a3 is not the roofless 1.3 RIDE, or whose a3-L1 / L2 / L4 are not the ceiling lights it knows.
//   node Tools/night/n72-n2-a3-lights.cjs
"use strict";
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN = "Design/levels/n52/foundry.plan.json";
const IDS = ["a3-L1", "a3-L2", "a3-L4"];

const f = path.join(ROOT, PLAN);
const text = fs.readFileSync(f, "utf8");
const eol = text.includes("\r\n") ? "\r\n" : "\n";
const fmt = (o) => JSON.stringify(o, null, 1).replace(/\n/g, eol) + eol;
const plan = JSON.parse(text);
if (fmt(plan) !== text) throw new Error(PLAN + " does not round-trip in its form; not written");
const a3 = plan.rooms.find((r) => r.id === "a3");
if (!a3 || a3.name !== "1.3 RIDE" || a3.roof !== false) throw new Error("a3 is not the roofless 1.3 RIDE: not written");
const mine = plan.lights.filter((l) => IDS.includes(l.id));
if (mine.length === 0) { console.log("a3-L1 / L2 / L4 are already out of " + PLAN + ": nothing to do"); process.exit(0); }
if (mine.length !== IDS.length || !mine.every((l) => l.room === "a3" && l.kind === "ceiling" && l.at[1] > 9.9)) throw new Error("a3-L1 / L2 / L4 are not the three ceiling lights at 9.95 m: not written");
const before = plan.lights.length;
plan.lights = plan.lights.filter((l) => !IDS.includes(l.id));
a3.lights_why = "NIGHT72 N2 (Tools/night/n72-n2-a3-lights.cjs, the lead's call): a3 has no roof (0e3f95ea5), so its three ceiling lights a3-L1, a3-L2 and a3-L4 (y 9.95, normal down) had nothing above them: out, a roofless a3 keeps no ceiling light.";
fs.writeFileSync(f, fmt(plan));
console.log("lights " + before + " -> " + plan.lights.length + " in " + PLAN + " (" + IDS.join(", ") + " out; lights_why added to a3)");

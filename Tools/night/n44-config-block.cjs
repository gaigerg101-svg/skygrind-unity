// NIGHT44 shared commit (the lead; a one-hour night: GG 2026-10-16 ~20:47 "start now, you only have one hour max"): adds design.n44 with
// the one view row vis44 (V2: the FUSION streaks, the TORCH cone, the stale card), off, to both config copies. No sim rows tonight (K2's
// RING and B2's reveals are cut for the hour), so no N44Blocks and no tape change. Refuses to run twice.
// Run: node Tools/night/n44-config-block.cjs
const fs = require("fs");
const root = "C:/Users/gaige/dev/SKYGRIND-unity/";
const file = root + "Design/unity-config.json", copy = root + "Assets/StreamingAssets/Design/unity-config.json";
let raw = fs.readFileSync(file, "utf8");
if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ");
const d = JSON.parse(raw);
if (d.design.n44) throw new Error("design.n44 already exists");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const tail = eol + "    }" + eol + "  }" + eol + "}";
if (!raw.endsWith(tail)) throw new Error("unexpected config ending");
const n44 = {
  _why: "NIGHT44, the finishing night cut to one hour (GG 2026-10-16 ~20:47: 'start now, you only have one hour max'; NIGHT44.md; Research/2026-10-16-wave44/). One view row: vis44 (N44View; no sim bit). -skygrindOff44 (and every older off switch) draws NIGHT43's views.",
  vis44: { _why: "Builder V2 (H2): the FUSION's streaks tapered with a hot-pink / white core (pending DECIDE43 D-3), the TORCH's dense cone from the muzzle, the stale centre card dropped on a room change (T-69). View only.", enabled: false, enabled_why: "Off until the lead has looked at its vis44 off / on frames (NIGHT44.md method 2)." },
};
const block = JSON.stringify(n44, null, 2).split("\n").map((l, i) => (i === 0 ? l : "    " + l)).join(eol);
const out = raw.slice(0, raw.length - tail.length) + eol + "    }," + eol + "    \"n44\": " + block + eol + "  }" + eol + "}";
JSON.parse(out);
fs.writeFileSync(file, out);
fs.copyFileSync(file, copy);
console.log("design.n44.vis44 added (off)");

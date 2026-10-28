// NIGHT52 (the lead's; DECIDE52 D-17): one more view row, design.n52.look52 (off), for builder U's round 5: the world reads the GG-persona
// review asked for (D-16): the health pickup's silhouette ("the healing item don't show what they are", OWNER.md) and the grapple ring /
// rope colours (a red-orange rope line breaks rule 8: red-orange is enemies and danger only). Both config copies; N52SharedTests' view rows.
//   node Tools/night/n52-row-look52.cjs   (refuses to run twice)
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const files = ["Design/unity-config.json", "Assets/StreamingAssets/Design/unity-config.json"].map((p) => path.join(ROOT, p));
const raw = fs.readFileSync(files[0], "utf8");
if (raw !== fs.readFileSync(files[1], "utf8")) throw new Error("the two config copies differ");
const d = JSON.parse(raw);
if (!d.design.n52 || d.design.n52.look52) throw new Error("design.n52.look52 exists or design.n52 is missing (already run?)");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const anchor = '        "numbers": {}' + eol + "      }" + eol + "    }" + eol + "  }" + eol + "}";
if (!raw.endsWith(anchor)) throw new Error("unexpected config ending (design.n52's last row must end the file)");
const row = {
  _why: "View row (NightRows.ViewOn(cfg, \"look52\")). Builder U (round 5; DECIDE52 D-16 / D-17, the GG-persona review): the health pickup reads as health at 15 m (its silhouette, not a cyan canister under small text) and the grapple rings / ropes keep rule 8's colours (no red-orange on a player-usable rope). Off = NIGHT51's pickups and rings.",
  enabled: false,
  enabled_why: d.design.n52.hud52.enabled_why,
  numbers: {},
};
const block = JSON.stringify(row, null, 2).split("\n").map((l, i) => (i === 0 ? l : "      " + l)).join(eol);
const out = raw.slice(0, raw.length - anchor.length) + '        "numbers": {}' + eol + "      }," + eol + '      "look52": ' + block + eol + "    }" + eol + "  }" + eol + "}";
const back = JSON.parse(out);
if (JSON.stringify(back.design.n52.look52) !== JSON.stringify(row)) throw new Error("look52 did not round-trip");
const a = JSON.parse(JSON.stringify(back)); delete a.design.n52.look52;
if (JSON.stringify(a) !== JSON.stringify(d)) throw new Error("something else changed");
for (const f of files) fs.writeFileSync(f, out);
const test = path.join(ROOT, "Assets/Tests/EditMode/N52SharedTests.cs");
let t = fs.readFileSync(test, "utf8");
const old = 'static readonly string[] ViewRows = { "hud52", "skin52", "guns52" };';
if (t.split(old).length !== 2) throw new Error("N52SharedTests' ViewRows anchor");
t = t.replace(old, 'static readonly string[] ViewRows = { "hud52", "skin52", "guns52", "look52" }; // look52: n52-row-look52.cjs (D-17)');
fs.writeFileSync(test, t);
console.log("design.n52.look52 added (off), both copies; N52SharedTests' view rows");

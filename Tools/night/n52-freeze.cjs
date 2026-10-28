// NIGHT52 the freeze (the lead's; NIGHT52.md section 7: "rows ON by their checks"): turns the named design.n52 rows ON with their reasons, in
// both config copies, and teaches the older shared tests that a later night's ON level row may swap a slot again: N47SharedTests and
// N48SharedTests' no-switch branches compare the slot with N52Chain.Final(levelTo) (the level a later ON row turns it into), the helper
// added to N52SharedTests.cs. With every n52 row off, Final(x) == x: the tests read as before.
//   node Tools/night/n52-freeze.cjs [--dry] <row>="<reason>" ...      e.g. kiln52="K: kb2 / kb2c / kb3 pass the contract; verbs37 30/30"
// Refuses an unknown row or a row already ON; asserts every anchor once before writing.
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const dry = process.argv.includes("--dry");
const pairs = process.argv.slice(2).filter((a) => a !== "--dry").map((a) => { const i = a.indexOf("="); return [a.slice(0, i), a.slice(i + 1)]; });
if (!pairs.length || pairs.some(([r, why]) => !r || !why)) throw new Error('usage: n52-freeze.cjs [--dry] <row>="<reason>" ...');
const writes = [];
const cfgFiles = ["Design/unity-config.json", "Assets/StreamingAssets/Design/unity-config.json"];
const raw = fs.readFileSync(path.join(ROOT, cfgFiles[0]), "utf8");
if (raw !== fs.readFileSync(path.join(ROOT, cfgFiles[1]), "utf8")) throw new Error("the two config copies differ");
const d = JSON.parse(raw);
let out = raw;
for (const [row, why] of pairs) {
  const r = d.design.n52[row];
  if (!r) throw new Error("no design.n52." + row);
  if (r.enabled) throw new Error(row + " is already ON");
  // the row's own block: its "enabled": false and its enabled_why, found after its key, once
  const key = '"' + row + '": {';
  const at = out.indexOf(key, out.indexOf('"n52": {'));
  if (at < 0 || out.indexOf(key, at + 1) >= 0 && out.indexOf(key, at + 1) < out.length) {
    if (at < 0) throw new Error(row + ": its block was not found");
  }
  const en = out.indexOf('"enabled": false', at), ew = out.indexOf('"enabled_why": "', at);
  const own = out.indexOf('"_why": "', at), next = out.indexOf('"_why": "', own + 1); // the row's own _why, then the next row's
  if (en < 0 || ew < 0 || (next > 0 && (en > next || ew > next))) throw new Error(row + ": enabled / enabled_why not inside its block");
  const ewEnd = out.indexOf('"', ew + '"enabled_why": "'.length);
  const newWhy = ("ON at the NIGHT52 freeze: " + why).replace(/\\/g, "\\\\").replace(/"/g, '\\"');
  out = out.slice(0, ew) + '"enabled_why": "' + newWhy + out.slice(ewEnd);
  const en2 = out.indexOf('"enabled": false', at);
  out = out.slice(0, en2) + '"enabled": true' + out.slice(en2 + '"enabled": false'.length);
}
const back = JSON.parse(out);
for (const [row] of pairs) if (back.design.n52[row].enabled !== true || !back.design.n52[row].enabled_why.startsWith("ON")) throw new Error(row + " did not flip");
const a = JSON.parse(JSON.stringify(back)), b = JSON.parse(JSON.stringify(d));
for (const [row] of pairs) { a.design.n52[row].enabled = b.design.n52[row].enabled = null; a.design.n52[row].enabled_why = b.design.n52[row].enabled_why = null; }
if (JSON.stringify(a) !== JSON.stringify(b)) throw new Error("something else changed in the config");
for (const f of cfgFiles) writes.push([f, out]);

function patch(file, pairs2) {
  let s = fs.readFileSync(path.join(ROOT, file), "utf8");
  const crlf = s.includes("\r\n");
  if (crlf) s = s.replace(/\r\n/g, "\n");
  for (const [x] of pairs2) if (s.split(x).length - 1 !== 1) throw new Error(file + ": anchor found " + (s.split(x).length - 1) + " times: " + x.slice(0, 90));
  for (const [x, y] of pairs2) s = s.replace(x, () => y);
  if (crlf) s = s.replace(/\n/g, "\r\n");
  writes.push([file, s]);
}
const t52 = fs.readFileSync(path.join(ROOT, "Assets/Tests/EditMode/N52SharedTests.cs"), "utf8");
if (!t52.includes("class N52Chain")) {
  patch("Assets/Tests/EditMode/N52SharedTests.cs", [[
    "namespace Skygrind.Core.Tests\n{\n    public class N52SharedTests\n",
    "namespace Skygrind.Core.Tests\n{\n" +
    "    /// <summary>NIGHT52 freeze (n52-freeze.cjs): the level a levelTo id ends up as in the campaign order once later nights' ON level rows\n" +
    "    /// (under the off switch) swap it in turn. With every later row off it returns the id itself.</summary>\n" +
    "    internal static class N52Chain\n    {\n" +
    "        public static string Final(GameConfig c, string id)\n        {\n" +
    "            var design = PortData.Obj(c.root[\"design\"]);\n" +
    "            for (int hops = 0; hops < 16 && id != null; hops++)\n            {\n" +
    "                string next = null;\n" +
    "                foreach (KeyValuePair<string, object> kv in design)\n                {\n" +
    "                    if (kv.Key.Length < 2 || kv.Key[0] != 'n' || !int.TryParse(kv.Key.Substring(1), out int nn) || nn >= NightRows.ForceOffFrom) continue;\n" +
    "                    if (!(kv.Value is Dictionary<string, object> night)) continue;\n" +
    "                    foreach (KeyValuePair<string, object> r in night)\n" +
    "                        if (r.Value is Dictionary<string, object> row && row.TryGetValue(\"levelFrom\", out object f) && f as string == id\n" +
    "                            && row.TryGetValue(\"enabled\", out object e) && e is bool on && on && row.TryGetValue(\"levelTo\", out object t) && t is string ts) { next = ts; break; }\n" +
    "                    if (next != null) break;\n                }\n" +
    "                if (next == null) return id;\n                id = next;\n            }\n            return id;\n        }\n    }\n\n" +
    "    public class N52SharedTests\n",
  ]]);
}
patch("Assets/Tests/EditMode/N47SharedTests.cs", [[
  'Assert.That(at >= 0 && on[at] == c.Str("design.n47." + row + ".levelTo"), Is.True,',
  'Assert.That(at >= 0 && on[at] == N52Chain.Final(c, c.Str("design.n47." + row + ".levelTo")), Is.True, // NIGHT52 freeze: a later ON row may swap the slot again\n                            ',
]]);
patch("Assets/Tests/EditMode/N48SharedTests.cs", [[
  'Assert.That(at >= 0 && on[at] == c.Str("design.n48." + row + ".levelTo"), Is.True,',
  'Assert.That(at >= 0 && on[at] == N52Chain.Final(c, c.Str("design.n48." + row + ".levelTo")), Is.True, // NIGHT52 freeze: a later ON row may swap the slot again\n                            ',
]]);
if (dry) { for (const [p] of writes) console.log("would write " + p); console.log("DRY RUN: rows " + pairs.map(([r]) => r).join(", ") + "; every anchor found once"); process.exit(0); }
for (const [p, s] of writes) fs.writeFileSync(path.join(ROOT, p), s);
console.log("NIGHT52 freeze: ON " + pairs.map(([r]) => r).join(", ") + "; both config copies; N47 / N48 shared tests follow later swaps (N52Chain)");

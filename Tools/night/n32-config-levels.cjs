// NIGHT32 shared commit 2 (the lead): the level rows' keys Curriculum25.N32Levels reads (empty: no swap, no level 5 until a builder fills them).
// Run: node Tools/night/n32-config-levels.cjs   (refuses to run twice); then bash Tools/night/n24-sync.sh
const fs = require("fs");
const file = "C:/Users/gaige/dev/SKYGRIND-unity/Design/unity-config.json";
let s = fs.readFileSync(file, "utf8");
const eol = s.includes("\r\n") ? "\r\n" : "\n";
if (s.includes("\"levelSwap\"")) throw new Error("levelSwap already present");
function after(anchorRow, lines) {
  // the row's `"enabled": false` line inside design.n32.<row>
  const at = s.indexOf("\"" + anchorRow + "\": {", s.indexOf("\"n32\": {"));
  if (at < 0) throw new Error("row not found " + anchorRow);
  const en = s.indexOf("\"enabled\": false", at);
  const lineEnd = s.indexOf(eol, en);
  const indent = s.slice(s.lastIndexOf(eol, en) + eol.length, en);
  s = s.slice(0, en) + "\"enabled\": false," + eol + lines.map((l) => indent + l).join("," + eol) + s.slice(en + "\"enabled\": false".length);
}
after("arenas32", ["\"levelSwap_why\": \"Curriculum25.N32Levels: while this row is on, each level id here is swapped for its rebuilt file ({old id: new id}); builder A fills it (e.g. n30-foundry: n32-foundry).\"", "\"levelSwap\": {}"]);
after("span32", ["\"levelSwap_why\": \"Curriculum25.N32Levels: while this row is on, n30-span is swapped for builder S's rebuilt level 4 ({old id: new id}).\"", "\"levelSwap\": {}"]);
after("level5", ["\"id_why\": \"Curriculum25.N32Levels: while this row is on, this level id is inserted right after `after` (level 4) in the campaign order; builder F names it.\"", "\"id\": \"\"", "\"after\": \"n30-span\""]);
fs.writeFileSync(file, s);
const d = JSON.parse(fs.readFileSync(file, "utf8"));
console.log(JSON.stringify({ a: d.design.n32.arenas32.levelSwap, s: d.design.n32.span32.levelSwap, f: [d.design.n32.level5.id, d.design.n32.level5.after] }));

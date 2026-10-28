// NIGHT38 builder O2: level 4's delivery rows for n38-span, as the n37 levels did (git show 8880e8b7): the maps list row (design.maps.list,
// campaignOnly), the unlock rows (design.n31.unlocks.levels.rows and Design/levels/unlocks.json: level 4 earns the STITCHER, the nailgun), then the
// copies under Assets/StreamingAssets (byte-identical). Text inserts right after n37-span's own rows (L-049: no other row is touched); a second run
// changes nothing. `--on` also turns design.n38.span38 on (only when its gates are GREEN: builder E's `n38 gates`).
// Run, from the repo (or worktree) root:  node Tools/night/n38o-config.cjs [--on]
const fs = require("fs");
const CFG = "Design/unity-config.json", CFG_COPY = "Assets/StreamingAssets/Design/unity-config.json";
const UNL = "Design/levels/unlocks.json", UNL_COPY = "Assets/StreamingAssets/Design/levels/unlocks.json";
const NOTE = "LEVEL 4 THE SPAN rebuilt for flight (NIGHT38 O2, design.n38.span38 swaps n37-span for it): five new rooms (open roofed arenas, two flight rooms of grapple gates) and NIGHT37's two rehearsal rooms and THE CANYON. The STITCHER (also a pickup in room p1), as n37-span.";

function insertAfter(text, anchor, add, what) {
  const i = text.indexOf(anchor);
  if (i < 0) throw new Error(`${what}: anchor not found`);
  return text.slice(0, i + anchor.length) + add + text.slice(i + anchor.length);
}

let cfg = fs.readFileSync(CFG, "utf8");
const eol = cfg.includes("\r\n") ? "\r\n" : "\n";
if (!cfg.includes('"id": "n38-span"')) {
  // 1. the maps list: one line after n37-span's
  const m37 = '{ "id": "n37-span", "label": "LEVEL 4 THE SPAN", "file": "Design/levels/n37-span.json", "campaignOnly": true },';
  cfg = insertAfter(cfg, m37, eol + '        { "id": "n38-span", "label": "LEVEL 4 THE SPAN", "file": "Design/levels/n38-span.json", "campaignOnly": true },', "maps.list");
  // 2. the unlock row: a block after n37-span's (design.n31.unlocks.levels.rows)
  const u37 = '"id": "n37-span",' + eol + '              "earns": "nailgun",';
  const at = cfg.indexOf(u37);
  if (at < 0) throw new Error("unlocks: n37-span's row not found");
  const close = cfg.indexOf("}", at);
  const block = "," + eol + "            {" + eol + '              "n": 4,' + eol + '              "id": "n38-span",' + eol + '              "earns": "nailgun",' + eol
    + '              "note": ' + JSON.stringify(NOTE) + eol + "            }";
  cfg = cfg.slice(0, close + 1) + block + cfg.slice(close + 1);
  console.log("config: maps.list + unlocks rows for n38-span");
} else console.log("config: n38-span rows already there");
if (process.argv.includes("--on")) {
  const key = '"span38": {';
  const i = cfg.indexOf(key);
  const j = cfg.indexOf('"enabled": false', i);
  const k = cfg.indexOf("}", i);
  if (i < 0) throw new Error("design.n38.span38 not found");
  if (j > 0 && j < k) { cfg = cfg.slice(0, j) + '"enabled": true' + cfg.slice(j + '"enabled": false'.length); console.log("design.n38.span38: enabled true"); }
  else console.log("design.n38.span38: already on");
}
const parsed = JSON.parse(cfg);
if (!parsed.design.maps.list.some((m) => m.id === "n38-span")) throw new Error("maps.list row missing after the patch");
fs.writeFileSync(CFG, cfg);
fs.copyFileSync(CFG, CFG_COPY);

let unl = fs.readFileSync(UNL, "utf8");
if (!unl.includes('"id": "n38-span"')) {
  const line37 = unl.split(/\r?\n/).find((l) => l.includes('"id": "n37-span"'));
  if (!line37) throw new Error("unlocks.json: n37-span's row not found");
  const ueol = unl.includes("\r\n") ? "\r\n" : "\n";
  unl = insertAfter(unl, line37, ueol + '    { "n": 4, "id": "n38-span", "earns": "nailgun", "note": ' + JSON.stringify(NOTE) + " },", "unlocks.json");
  JSON.parse(unl);
  fs.writeFileSync(UNL, unl);
  console.log("unlocks.json: n38-span row");
} else console.log("unlocks.json: n38-span row already there");
fs.copyFileSync(UNL, UNL_COPY);
for (const [a, b] of [[CFG, CFG_COPY], [UNL, UNL_COPY]]) if (!fs.readFileSync(a).equals(fs.readFileSync(b))) throw new Error(`copy mismatch: ${b}`);
console.log("copies identical");

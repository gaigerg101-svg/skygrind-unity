// NIGHT67 A1 (the lead runs it; the shared commit, L-049 / L-065; builder T's T1a, slate T1a line 2; Tools/night/n65-a1.cjs's recipe for three
// levels; FORGE67b code-scout.md section 3 #3, redteam.md 1 / 11 / 13). Reads the level table Tools/night/levels67.json (iceline, kiln, spire) and:
//   1. design.n67 as design's last key in Design/unity-config.json and its byte copy Assets/StreamingAssets/Design/unity-config.json: the block's
//      _why and three LEVEL rows iceline67 / kiln67 / spire67 {levelFrom n52-iceline / n57-kiln / n52-spire, levelTo n67-iceline / n67-kiln /
//      n67-spire, enabled false} (data, no tape bit: NightRows.SwapLevels swaps a row's level in when it is on, NightRows.BaseId reads n67-<L>
//      as its base for older systems: the loadout, the awards, the unlocks);
//   2. a maps.list row for each new id right after its base's row (an unlisted id boots the arena: N65SharedTests.cs:40);
//   3. look49's levels list (design.n49.look49.numbers.levels): each new id right after its base when the base is listed (n67-spire);
//   4. Assets/Scripts/Game/UnityTuning.N53.cs: Polish53IcelineLevels gains "n67-iceline" (iceline's grade is a code list: polish53 is ON, so a
//      rebuilt ice line would lose its grade: PRINCIPLES rule 1);
//   5. the placeholder levels Design/levels/n67-<L>.json and their Player copies: the base level byte for byte with its name and _why changed
//      (gen-slots.cjs --level <L> --run remakes it from the slots);
//   6. per level Design/levels/n67/<L>/slots67.json with every slot old (redteam.md 1: the close-check reads it from the shared commit on), the
//      exports Design/levels/n67/<L>/rooms-base/<id>.json and rooms/<id>.json (gen-slots.cjs --export), and the plan step with that slots file
//      (Design/levels/n67/<L>.plan.json, <L>.assembly.json and openings67.json: n59-l-room.py n67-<L> reads them; no dotnet).
// Asserts nothing else changed in the config (the parsed file minus the changes equals the old one: keys, values and order), the copies are
// byte-identical, the code edit is that one list, each placeholder differs from its base only in name and _why. Run once; a second run is refused.
//   node Tools/night/n67-a1.cjs [--dry]
//   node Tools/night/n67-a1.cjs --verify   (after the run, read only: the shared commit's facts, N65SharedTests' as a node check: each row exists,
//                                          is OFF, chains to its base (BaseId), is listed once in maps.list with its file in both copies, look49 and
//                                          Polish53 hold the ids, every slots67.json is all old over the level's slots, every placeholder is its
//                                          base apart from name / _why, the config copies identical)
const fs = require("fs");
const path = require("path");
const cp = require("child_process");

process.on("uncaughtException", (e) => { console.error("n67-a1: REFUSED: " + e.message); process.exit(1); }); // a refusal is one line, exit 1
const args = process.argv.slice(2);
const dry = args.includes("--dry");
const root = path.join(__dirname, "..", "..");
const abs = (p) => path.join(root, p);
const CFG = ["Design/unity-config.json", "Assets/StreamingAssets/Design/unity-config.json"];
const TUNING = "Assets/Scripts/Game/UnityTuning.N53.cs";
const TABLE = JSON.parse(fs.readFileSync(abs("Tools/night/levels67.json"), "utf8"));
const NAMES = ["iceline", "kiln", "spire"];
const LV = NAMES.map((n) => Object.assign({ name: n, key: n + "67" }, TABLE[n]));
for (const l of LV) if (!l.id || !l.base || l.row !== "design.n67." + l.key) throw new Error("levels67.json: " + l.name + " has no id / base, or its row is not design.n67." + l.key);

const BLOCK = {
  _why: "NIGHT67 (THE AREAS, NIGHT67.md; GG 2026-10-24: 'area deisgn need t be better in alot of levels thats the mian focus'; GG 2026-10-09: 'every single level needs a complete arena overhaul'): this night's design rows. iceline67, kiln67 and spire67 are LEVEL rows (data, no tape bit: NightRows.SwapLevels swaps each one's level in while it is on, NightRows.BaseId reads n67-<level> as its base level for older systems). Each is OFF until its own slice LV passes (Logs/n67/slate.json LV) with a skygrind-critic PASS bound to the sha it judged, flipped by Tools/night/n67-freeze-rows.cjs. The web has no such rows: its levels were single maps.",
};
for (const l of LV) {
  BLOCK[l.key] = {
    _why: "Level row (the lead's A1, Tools/night/n67-a1.cjs; builders R and S fill its rooms): " + l.label + " rebuilt as areas as " + l.id + " by Tools/night/gen-slots.cjs --level " + l.name + " (Tools/night/levels67.json): " + l.slots.length + " fight-room slots (" + l.slots.join(", ") + "), each holding " + l.base + "'s room until its rebuilt room passes its GATE (" + l.slotsFile + ", written only by Tools/night/n65-slot-set.cjs --level=" + l.name + "); every link, the boss room and every door kept bit for bit. On, the campaign order's " + l.base + " becomes " + l.id + ". Off: -skygrindOff67 (or older). The web has no such row.",
    enabled: false,
    enabled_why: "Off until slate LV passes (Logs/n67/slate.json LV, NIGHT67.md section 4 step 5): " + l.slotsFile + " holds at least one room set new by n65-slot-set.cjs and every other slot equals " + l.base + "'s room; levelgen --walk no ERROR beyond " + l.base + "'s own; verbs37 " + l.id + " on its GATE build; -skygrindOff67 boots " + l.base + "; gates GREEN; then LV's own skygrind-critic PASS, flipped by Tools/night/n67-freeze-rows.cjs.",
    levelFrom: l.base,
    levelTo: l.id,
  };
}
const mapRow = (id, label) => '{"id": "' + id + '", "label": "' + label + '", "file": "Design/levels/' + id + '.json", "campaignOnly": true},';
const PLACE_WHY = (l) => "NIGHT67 A1 placeholder (Tools/night/n67-a1.cjs: " + l.base + ".json byte for byte with its name and this _why changed; Tools/night/gen-slots.cjs --level " + l.name + " --run remakes it from " + l.slotsFile + "): ";
const POLISH_OLD = 'Polish53IcelineLevels = { "n47-iceline", "n52-iceline" };';
const POLISH_NEW = 'Polish53IcelineLevels = { "n47-iceline", "n52-iceline", "n67-iceline" };';
const POLISH_NOTE_OLD = "// THE ICE LINE's map ids (lower case): look52's grade list takes them while icelineGrade is on";
const POLISH_NOTE_NEW = "// THE ICE LINE's map ids (lower case): look52's grade list takes them while icelineGrade is on (NIGHT67 A1: n67-iceline, row design.n67.iceline67, Tools/night/n67-a1.cjs)";
const slotsText = (l) => {
  const s = { _why: "NIGHT67 (the lead's; written by Tools/night/n67-a1.cjs with every slot old, then only by Tools/night/n65-slot-set.cjs --level=" + l.name + " --slots=" + l.slotsFile + "): which of " + l.id + "'s fight-room slots ship their rebuilt room file (new, bound to its GATE PASS's sha and the file's blob there) and which keep " + l.base + "'s room (old). gen-slots.cjs --level " + l.name + " --slots=" + l.slotsFile + " assembles the shipped level from it.", slots: {} };
  for (const id of l.slots) s.slots[id] = { use: "old", why: "the night's initial slot set (n67-a1.cjs): " + l.base + "'s room until the rebuilt room PASSes its GATE" };
  return JSON.stringify(s, null, 1) + "\n";
};
function placeholder(l) {
  const lraw = fs.readFileSync(abs(l.baseFile), "utf8");
  const lb = JSON.parse(lraw);
  const head = '{"_why":' + JSON.stringify(lb._why) + ',"name":' + JSON.stringify(lb.name) + ",";
  if (!lraw.startsWith(head)) throw new Error(l.baseFile + " does not start with its _why and name as JSON.stringify writes them (the placeholder's text edit would be unsafe)");
  const lout = '{"_why":' + JSON.stringify(PLACE_WHY(l) + lb._why) + ',"name":' + JSON.stringify(l.id) + "," + lraw.slice(head.length);
  const la = JSON.parse(lout);
  if (la.name !== l.id || la.data.name !== lb.data.name) throw new Error(l.id + ": the placeholder's names are wrong");
  la.name = lb.name; la._why = lb._why;
  if (JSON.stringify(la) !== JSON.stringify(lb)) throw new Error(l.id + ": the placeholder differs from " + l.base + " beyond its name and _why");
  return lout;
}
const NightOf = (key) => { const m = /^n(\d+)$/.exec(key || ""); return m ? Number(m[1]) : -1; };

if (args.includes("--verify")) { verify(); process.exit(0); }

// ---------------------------------------------------------------- the run
const raw = fs.readFileSync(abs(CFG[0]), "utf8");
if (raw !== fs.readFileSync(abs(CFG[1]), "utf8")) throw new Error("the two config copies differ");
const before = JSON.parse(raw);
if (before.design.n67) throw new Error("design.n67 already exists (already run?)");
for (const l of LV) for (const f of ["Design/levels/" + l.id + ".json", "Assets/StreamingAssets/Design/levels/" + l.id + ".json", l.slotsFile]) if (fs.existsSync(abs(f))) throw new Error(f + " already exists (already run?)");
const last = Object.keys(before.design).pop();
if (NightOf(last) >= 67) throw new Error("design's last key is " + last + " (expected an older night)");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";

// 2. the maps.list rows: each base row once, on its own line; the new row after it with the same indent
const lines = raw.split(eol);
for (const l of LV) {
  const old = mapRow(l.base, l.label);
  const mi = lines.map((x, i) => (x.trim() === old ? i : -1)).filter((i) => i >= 0);
  if (mi.length !== 1) throw new Error(l.base + "'s maps.list row found " + mi.length + " times (expected 1): " + old);
  const indent = lines[mi[0]].slice(0, lines[mi[0]].indexOf("{"));
  lines.splice(mi[0] + 1, 0, indent + mapRow(l.id, l.label));
}
let out = lines.join(eol);

// 3. look49's levels list: each new id after its base when the base is listed
const lookOld = before.design.n49.look49.numbers.levels;
const lookNew = lookOld.slice();
for (const l of LV) { const i = lookNew.indexOf(l.base); if (i >= 0) lookNew.splice(i + 1, 0, l.id); }
const fmt = (a) => '"levels": [' + a.map((x) => JSON.stringify(x)).join(", ") + "]";
if (out.split(fmt(lookOld)).length !== 2) throw new Error("look49's levels list found " + (out.split(fmt(lookOld)).length - 1) + " times (expected 1): " + fmt(lookOld));
out = out.replace(fmt(lookOld), fmt(lookNew));

// 1. design.n67 as design's last key
const tail = eol + "    }" + eol + "  }" + eol + "}";
const tailEnd = out.endsWith(tail) ? tail : out.endsWith(tail + eol) ? tail + eol : null;
if (!tailEnd) throw new Error("unexpected config ending");
const block = JSON.stringify(BLOCK, null, 2).split("\n").map((x, i) => (i === 0 ? x : "    " + x)).join(eol);
out = out.slice(0, out.length - tailEnd.length) + eol + "    }," + eol + "    \"n67\": " + block + eol + "  }" + eol + "}" + tailEnd.slice(tail.length);

// asserts: only these changes
const after = JSON.parse(out);
const dk = Object.keys(after.design);
if (dk[dk.length - 1] !== "n67") throw new Error("n67 is not design's last key");
if (JSON.stringify(after.design.n67) !== JSON.stringify(BLOCK)) throw new Error("design.n67 not written as the script says");
const list = after.design.maps.list;
for (const l of LV) {
  const at = list.findIndex((m) => m.id === l.id);
  if (at < 1 || list[at - 1].id !== l.base || list.filter((m) => m.id === l.id).length !== 1) throw new Error("the maps.list row for " + l.id + " is not once, right after " + l.base + "'s");
  if (JSON.stringify(list[at]) !== JSON.stringify({ id: l.id, label: l.label, file: "Design/levels/" + l.id + ".json", campaignOnly: true })) throw new Error("the maps.list row for " + l.id + " differs");
}
if (JSON.stringify(after.design.n49.look49.numbers.levels) !== JSON.stringify(lookNew)) throw new Error("look49's levels list: " + after.design.n49.look49.numbers.levels.join(","));
delete after.design.n67;
for (const l of LV) list.splice(list.findIndex((m) => m.id === l.id), 1);
after.design.n49.look49.numbers.levels = lookOld;
if (JSON.stringify(after) !== JSON.stringify(before)) throw new Error("something outside the changes changed (keys, values or order)");

// 4. the code list
const traw = fs.readFileSync(abs(TUNING), "utf8");
if (traw.split(POLISH_OLD).length !== 2) throw new Error(TUNING + ": the Polish53IcelineLevels line found " + (traw.split(POLISH_OLD).length - 1) + " times (expected 1)");
if (traw.split(POLISH_NOTE_OLD).length !== 2) throw new Error(TUNING + ": its comment found " + (traw.split(POLISH_NOTE_OLD).length - 1) + " times (expected 1)");
const tout = traw.replace(POLISH_OLD, POLISH_NEW).replace(POLISH_NOTE_OLD, POLISH_NOTE_NEW);
const tl0 = traw.split(/\r?\n/), tl1 = tout.split(/\r?\n/);
if (tl0.length !== tl1.length || tl0.filter((x, i) => x !== tl1[i]).length !== 1) throw new Error(TUNING + ": the edit is not one line");

// 5. the placeholders
const places = LV.map((l) => [l, placeholder(l)]);

if (dry) {
  console.log("n67-a1 --dry: design.n67 (" + LV.map((l) => l.key).join(", ") + ") after design." + last + "; maps.list rows " + LV.map((l) => l.id).join(", ") + "; look49 " + JSON.stringify(lookNew) + "; " + TUNING + " Polish53IcelineLevels + n67-iceline; placeholders " + places.length + " (" + places.map(([l, t]) => l.id + " " + t.length + " chars").join(", ") + "); slots67.json x" + LV.length + " all old");
  process.exit(0);
}
for (const f of CFG) fs.writeFileSync(abs(f), out);
if (fs.readFileSync(abs(CFG[0]), "utf8") !== fs.readFileSync(abs(CFG[1]), "utf8")) throw new Error("the copies differ after the write");
fs.writeFileSync(abs(TUNING), tout);
for (const [l, t] of places) for (const f of ["Design/levels/" + l.id + ".json", "Assets/StreamingAssets/Design/levels/" + l.id + ".json"]) fs.writeFileSync(abs(f), t);
console.log("n67-a1: design.n67 (" + LV.map((l) => l.key + " " + l.base + " -> " + l.id + " off").join("; ") + ") after design." + last + "; maps.list rows after their bases; look49's levels " + JSON.stringify(lookNew) + " (both config copies byte-identical; nothing else changed); " + TUNING + ": Polish53IcelineLevels + \"n67-iceline\" (one line); placeholders " + places.map(([l]) => "Design/levels/" + l.id + ".json").join(", ") + " and their Player copies (each base's data.name kept)");
// 6. per level: the slots file (every slot old), the exports, the plan step with that slots file (no dotnet)
for (const l of LV) {
  fs.mkdirSync(path.dirname(abs(l.slotsFile)), { recursive: true });
  fs.writeFileSync(abs(l.slotsFile), slotsText(l));
  for (const step of [["--export"], ["--slots=" + l.slotsFile]]) {
    const r = cp.spawnSync(process.execPath, [path.join(__dirname, "gen-slots.cjs"), "--level", l.name].concat(step), { cwd: root, encoding: "utf8" });
    const said = ((r.stdout || "") + (r.stderr || "")).split(/\r?\n/).filter(Boolean);
    if (r.status !== 0) throw new Error("gen-slots.cjs --level " + l.name + " " + step.join(" ") + " failed: " + said.slice(-3).join(" | "));
    console.log("n67-a1: " + l.name + " " + step.join(" ") + ": " + (step[0] === "--export" ? said.length + " room(s) exported (" + l.slots.join(" ") + ")" : said.filter((x) => /^gen-slots|^openings/.test(x)).join(" | ")));
  }
  console.log("n67-a1: " + l.slotsFile + " written, every slot old (" + l.slots.join(" ") + ")");
}
verify();

// ---------------------------------------------------------------- --verify: the shared commit's facts (read only)
function verify() {
  let bad = 0;
  const say = (ok, m) => { console.log((ok ? "PASS " : "FAIL ") + m); if (!ok) bad++; };
  const c0 = fs.readFileSync(abs(CFG[0]), "utf8"), c1 = fs.readFileSync(abs(CFG[1]), "utf8");
  say(c0 === c1, "the two config copies are byte-identical (" + c0.length + " chars)");
  const cfg = JSON.parse(c0);
  const rows = [];
  for (const nk of Object.keys(cfg.design)) { const nn = NightOf(nk); const b = cfg.design[nk]; if (nn < 46 || !b || typeof b !== "object") continue; for (const r of Object.keys(b)) { const v = b[r]; if (v && typeof v === "object" && typeof v.levelFrom === "string" && typeof v.levelTo === "string") rows.push({ night: nn, name: r, from: v.levelFrom, to: v.levelTo, enabled: v.enabled === true }); } }
  const baseId = (id) => { for (let h = 0; h < 16; h++) { const r = rows.find((x) => x.to === id); if (!r || r.from === id) return id; id = r.from; } return id; };
  const files = (id) => ["Design/levels/" + id + ".json", "Assets/StreamingAssets/Design/levels/" + id + ".json"];
  for (const l of LV) {
    const r = (cfg.design.n67 || {})[l.key];
    say(!!r && r.enabled === false && r.levelFrom === l.base && r.levelTo === l.id, l.row + ": " + (r ? "levelFrom " + r.levelFrom + ", levelTo " + r.levelTo + ", enabled " + r.enabled : "missing"));
    say(baseId(l.id) === baseId(l.base), "BaseId(" + l.id + ") = " + baseId(l.id) + " = BaseId(" + l.base + ") (the older systems read it as its base: loadout, awards, unlocks)");
    const m = cfg.design.maps.list.filter((x) => x.id === l.id);
    const mi = cfg.design.maps.list.findIndex((x) => x.id === l.id);
    say(m.length === 1 && cfg.design.maps.list[mi - 1].id === l.base && m[0].file === "Design/levels/" + l.id + ".json", "maps.list: " + l.id + " listed once, right after " + l.base + ", file " + (m[0] || {}).file);
    const fx = files(l.id).map((f) => fs.existsSync(abs(f)));
    say(fx.every(Boolean) && fs.readFileSync(abs(files(l.id)[0]), "utf8") === fs.readFileSync(abs(files(l.id)[1]), "utf8"), l.id + ": both level copies exist and are byte-identical");
    if (fx.every(Boolean)) {
      const a = JSON.parse(fs.readFileSync(abs(files(l.id)[0]), "utf8")), b = JSON.parse(fs.readFileSync(abs(l.baseFile), "utf8"));
      const placeholderLike = a._why.startsWith("NIGHT67 A1 placeholder");
      const strip = (o) => Object.assign({}, o, { name: "", _why: "", generatedBy: "" });
      say(a.name === l.id && JSON.stringify(strip(a)) === JSON.stringify(strip(b)), l.id + ": name " + a.name + ", " + (placeholderLike ? "the A1 placeholder: " : "assembled (" + String(a.generatedBy).slice(0, 60) + "): ") + (JSON.stringify(strip(a)) === JSON.stringify(strip(b)) ? "equal to " + l.base + " apart from name / _why / generatedBy" : "DIFFERS from " + l.base + " (a rebuilt slot is in: read its slots file)"));
    }
    let sl = null;
    try { sl = JSON.parse(fs.readFileSync(abs(l.slotsFile), "utf8")); } catch (e) { sl = null; }
    const uses = sl ? l.slots.map((id) => id + " " + ((sl.slots || {})[id] || { use: "old (no entry)" }).use) : [];
    const extra = sl ? Object.keys(sl.slots || {}).filter((id) => !l.slots.includes(id)) : [];
    say(!!sl && extra.length === 0 && l.slots.every((id) => (sl.slots || {})[id] && ["old", "new"].includes(sl.slots[id].use)), l.slotsFile + ": " + (sl ? uses.join(", ") + (extra.length ? "; NOT SLOTS: " + extra.join(" ") : "") : "missing"));
    const ex = l.slots.filter((id) => fs.existsSync(abs(l.roomsBase + "/" + id + ".json")) && fs.existsSync(abs(l.rooms + "/" + id + ".json")));
    say(ex.length === l.slots.length, l.name + ": rooms-base / rooms exported for " + ex.length + " / " + l.slots.length + " slots");
    const lookHas = (cfg.design.n49.look49.numbers.levels || []).includes(l.base);
    if (lookHas) { const lv = cfg.design.n49.look49.numbers.levels; say(lv.indexOf(l.id) === lv.indexOf(l.base) + 1, "look49's levels: " + l.id + " right after " + l.base + " (" + lv.join(", ") + ")"); }
  }
  const t = fs.readFileSync(abs(TUNING), "utf8");
  say(t.includes(POLISH_NEW), TUNING + ": Polish53IcelineLevels holds n67-iceline");
  const on = rows.filter((r) => r.night === 67 && r.enabled);
  say(on.length === 0, "no design.n67 level row is ON (the campaign order is unchanged: SwapLevels skips an off row)" + (on.length ? ": ON " + on.map((r) => r.name).join(", ") : ""));
  console.log("n67-a1 --verify: " + (bad ? bad + " FAIL" : "all PASS"));
  if (bad) process.exit(1);
}

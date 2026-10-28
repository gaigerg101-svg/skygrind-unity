// NIGHT60 builder V (B3 c): sets ONE number in a BUILD's config copy only, so the lead can scan a number (yellow56's sKeep) in the Player without
// rebuilding. The repo's two copies (Design/ and Assets/StreamingAssets/Design/unity-config.json) are never touched.
//   node Tools/night/n60-build-num.cjs <tag> <dotted.config.path> <number>
//     e.g. node Tools/night/n60-build-num.cjs n60-b3 design.n56.yellow56.numbers.dimYellow58.sKeep 0.75
//     file: Builds/<tag>/SKYGRIND_Data/StreamingAssets/Design/unity-config.json (the path is walked from the file's root: "design." first)
//   node Tools/night/n60-build-num.cjs --selftest   (a temp copy of Design/unity-config.json: one number set, nothing else changed; an unknown
//     path, a non-number target and a non-number value refused)
// Refuses: a missing file, an unknown path (it prints the deepest key that exists), a target that is not already a number, a value that is
// not a finite number. The edit is in the text (only the value's characters change: the file's layout, key order and line ends stay), then
// the whole file is parsed again and must equal the old one with only that number changed. The first edit keeps the build's original as
// unity-config.json.orig next to it (restore: copy it back).
const fs = require("fs");
const path = require("path");
const os = require("os");

const root = path.join(__dirname, "..", "..");

/** The [start, end) of the value at `keys` in the JSON text, or the deepest prefix found when it is missing. */
function spanOf(text, keys) {
  let i = 0;
  const ws = () => { while (i < text.length && " \t\r\n".includes(text[i])) i++; };
  const str = () => {
    const s = i;
    i++;
    while (text[i] !== '"') i += text[i] === "\\" ? 2 : 1;
    i++;
    return JSON.parse(text.slice(s, i));
  };
  let found = null, deepest = [];
  function value(p) {
    ws();
    const s = i;
    const c = text[i];
    if (c === "{") {
      i++;
      ws();
      if (text[i] === "}") { i++; return [s, i]; }
      for (;;) {
        ws();
        const k = str();
        ws();
        i++; // ':'
        const q = p.concat([k]);
        const match = q.length <= keys.length && q.every((x, j) => x === keys[j]);
        if (match && q.length > deepest.length) deepest = q;
        const sp = value(q);
        if (match && q.length === keys.length) found = sp;
        ws();
        if (text[i] === ",") { i++; continue; }
        i++; // '}'
        return [s, i];
      }
    }
    if (c === "[") {
      i++;
      ws();
      if (text[i] === "]") { i++; return [s, i]; }
      for (let n = 0; ; n++) {
        value(p.concat([String(n)]));
        ws();
        if (text[i] === ",") { i++; continue; }
        i++; // ']'
        return [s, i];
      }
    }
    if (c === '"') { str(); return [s, i]; }
    while (i < text.length && !",}] \t\r\n".includes(text[i])) i++;
    return [s, i];
  }
  value([]);
  return { found, deepest };
}

function setNumber(file, dotted, valueText) {
  if (!fs.existsSync(file)) throw new Error("no such file: " + file);
  const v = Number(valueText);
  if (valueText === undefined || String(valueText).trim() === "" || !Number.isFinite(v)) throw new Error("not a finite number: " + valueText);
  const raw = fs.readFileSync(file, "utf8");
  const before = JSON.parse(raw);
  const keys = dotted.split(".");
  let o = before;
  for (const k of keys) {
    if (o === null || typeof o !== "object" || !(k in o)) {
      const { deepest } = spanOf(raw, keys);
      throw new Error("unknown path " + dotted + " (the deepest key that exists: " + (deepest.join(".") || "(none)") + ")");
    }
    o = o[k];
  }
  if (typeof o !== "number") throw new Error(dotted + " is not a number in the file (it is " + JSON.stringify(o).slice(0, 60) + "): refused");
  const { found } = spanOf(raw, keys);
  if (!found) throw new Error("the value's text was not found for " + dotted);
  const out = raw.slice(0, found[0]) + JSON.stringify(v) + raw.slice(found[1]);
  const after = JSON.parse(out);
  let t = before;
  for (const k of keys.slice(0, -1)) t = t[k];
  const old = t[keys[keys.length - 1]];
  t[keys[keys.length - 1]] = v;
  if (JSON.stringify(after) !== JSON.stringify(before)) throw new Error("something besides " + dotted + " would change: refused");
  const orig = file + ".orig";
  if (!fs.existsSync(orig)) fs.writeFileSync(orig, raw);
  fs.writeFileSync(file, out);
  return { old, v };
}

function selftest() {
  const src = path.join(root, "Design", "unity-config.json");
  const dir = fs.mkdtempSync(path.join(os.tmpdir(), "n60-build-num-"));
  const file = path.join(dir, "unity-config.json");
  fs.copyFileSync(src, file);
  const raw = fs.readFileSync(file, "utf8");
  const p = "design.n56.yellow56.numbers.dimYellow58.sKeep";
  let bad = 0;
  const r = setNumber(file, p, "0.75");
  const out = fs.readFileSync(file, "utf8");
  const changedLines = raw.split("\n").filter((l, k) => l !== out.split("\n")[k]).length;
  const ok1 = r.v === 0.75 && JSON.parse(out).design.n56.yellow56.numbers.dimYellow58.sKeep === 0.75 && changedLines === 1 && out.length === raw.length - String(r.old).length + 4;
  console.log("selftest set " + p + " " + r.old + " -> 0.75: " + changedLines + " line changed, the rest byte-equal -> " + (ok1 ? "as planted" : "WRONG"));
  bad += ok1 ? 0 : 1;
  for (const [what, args] of [["an unknown path", ["design.n56.yellow56.numbers.dimYellow58.sKeepTypo", "0.7"]], ["a non-number target", ["design.n56.yellow56.numbers.dimYellow58.sKeep_why", "0.7"]], ["a non-number value", [p, "abc"]]]) {
    let refused = false;
    try { setNumber(file, args[0], args[1]); } catch (e) { refused = true; console.log("selftest " + what + ": refused (" + e.message.slice(0, 110) + ") -> as planted"); }
    if (!refused) { console.log("selftest " + what + ": NOT refused -> WRONG"); bad++; }
  }
  const unchanged = fs.readFileSync(file, "utf8") === out;
  console.log("selftest the refused calls left the file as it was: " + (unchanged ? "as planted" : "WRONG"));
  bad += unchanged ? 0 : 1;
  fs.rmSync(dir, { recursive: true, force: true });
  console.log("SELFTEST " + (bad ? "RED (" + bad + " wrong)" : "GREEN"));
  return bad ? 1 : 0;
}

function main() {
  const a = process.argv.slice(2);
  if (a[0] === "--selftest") return selftest();
  if (a.length !== 3) {
    console.error("usage: node Tools/night/n60-build-num.cjs <tag> <dotted.config.path> <number> | --selftest");
    return 2;
  }
  const [tag, dotted, num] = a;
  if (!/^[A-Za-z0-9._-]+$/.test(tag)) throw new Error("a tag is letters, digits, '.', '_' and '-' only: " + tag);
  const file = path.join(root, "Builds", tag, "SKYGRIND_Data", "StreamingAssets", "Design", "unity-config.json");
  const r = setNumber(file, dotted, num);
  console.log("n60-build-num: " + path.relative(root, file) + " " + dotted + " " + r.old + " -> " + r.v + " (nothing else changed; the original kept as unity-config.json.orig)");
  return 0;
}

try {
  process.exitCode = main();
} catch (e) {
  console.error("n60-build-num: REFUSED: " + e.message);
  process.exitCode = 1;
}

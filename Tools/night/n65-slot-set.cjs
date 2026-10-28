// NIGHT65 builder T (T1; NIGHT65.md section 0 rule 8, section 4 step 4; redteam.md 4): the ONLY writer of Design/levels/n65/slots65.json, the
// lead's file that says which slots of n65-foundry ship their rebuilt room. A slot goes "new" only after its room's GATE PASS, and only when the
// room file is the one the critic judged: the slate slice <room> (Logs/n65/slate.json) must hold passes true, a passSha, a verdict file whose
// first line is "<room>: PASS", and Design/levels/n65/rooms/<room>.json must be unchanged since passSha (committed there, no later commit and
// no working-tree edit touches it). slots65.json then records the passSha and the file's blob at it; gen-n65-foundry.cjs --slots=<slots65.json>
// re-checks the file against that sha at every assembly (a change after the PASS is refused: GATE it again).
//   node Tools/night/n65-slot-set.cjs <room> new [--slate=<path>] [--slots=<path>] [--dry]
//   node Tools/night/n65-slot-set.cjs <room> old <reason>   (back to n52's room: a FAIL, a roll-back)
//   node Tools/night/n65-slot-set.cjs --print                (the slot set)
//   node Tools/night/n65-slot-set.cjs --selftest             (the planted cases: a PASS sets the slot; a changed room file is refused; a slice
//                                                             with no PASS is refused; scratch slate and slots files, the room file restored)
const fs = require("fs");
const path = require("path");
const os = require("os");
const cp = require("child_process");
const ROOT = path.join(__dirname, "../..");
// NIGHT66 T2: b2 THE GANTRY is the seventh slot (gen-n65-foundry.cjs SLOTS); a slots65.json written before it has no b2 entry: gen reads a
// missing slot as old, and `n65-slot-set.cjs b2 old <reason>` writes it
// NIGHT67 T1b (slate T1b; FORGE67b redteam.md 13): any level of Tools/night/levels67.json. `--level=<L>` takes that level's SLOTS, room files
// (Design/levels/n67/<L>/rooms/) and slots file (Design/levels/n67/<L>/slots67.json: never slots65.json); `--slots=<file>` alone names the level
// by its file (the table's slotsFile); both given must agree. An unknown option, `--slots <file>` with a space, and a room that is not a slot
// of that file's level (or not a key of an existing slots file) are refused. With neither, level 1 as before (slots65.json).
//   node Tools/night/n65-slot-set.cjs <room> new --level=<L> --slots=Design/levels/n67/<L>/slots67.json --slate=Logs/n67/slate.json
let SLOTS = ["a2", "a3", "a4", "a5", "b1", "b2", "a8"];
const args = process.argv.slice(2);
const OPTS = ["--slots", "--slate", "--dry", "--print", "--selftest", "--level"];
const opt = (name, d) => { const a = args.find((x) => x.startsWith(name + "=")); return a ? a.slice(name.length + 1) : d; };
const git = (a) => cp.execFileSync("git", a, { cwd: ROOT, encoding: "utf8", maxBuffer: 64 << 20, stdio: ["ignore", "pipe", "pipe"] }).trim();
let ROOMS_DIR = "Design/levels/n65/rooms", DEFAULT_SLOTS = "Design/levels/n65/slots65.json", LEVEL_NAME = "foundry", LEVEL_ID = "n65-foundry", BASE = "n52-foundry", GEN = ["gen-n65-foundry.cjs"];
const roomPath = (id) => ROOMS_DIR + "/" + id + ".json";
const abs = (p) => (path.isAbsolute(p) ? p : path.join(ROOT, p));
const norm = (p) => path.relative(ROOT, abs(p)).replace(/\\/g, "/");
// the level: --level=<L>, else the level whose slotsFile is --slots=<file>, else level 1; refuses an unknown option or `--slots <file>`
function setLevel() {
  for (const a of args) if (a.startsWith("--") && !OPTS.includes(a.split("=")[0])) throw new Error("unknown option " + a + " (known: " + OPTS.join(" ") + "; values with '=')");
  for (const a of ["--slots", "--slate", "--level"]) if (args.includes(a)) throw new Error(a + " needs '=' (" + a + "=<value>): '" + a + " <value>' would make the value a positional and fall back to the default");
  const table = JSON.parse(fs.readFileSync(path.join(ROOT, "Tools/night/levels67.json"), "utf8"));
  const lv = opt("--level", null), sf = opt("--slots", null);
  let name = lv;
  if (lv && (!table[lv] || lv.startsWith("_"))) throw new Error("--level=" + lv + ": not in Tools/night/levels67.json (" + Object.keys(table).filter((k) => !k.startsWith("_")).join(", ") + ")");
  if (sf) {
    const byFile = Object.keys(table).filter((k) => !k.startsWith("_") && norm(table[k].slotsFile) === norm(sf));
    if (lv && byFile.length && byFile[0] !== lv) throw new Error("--slots=" + sf + " is level " + byFile[0] + "'s, not --level=" + lv + "'s (" + table[lv].slotsFile + ")");
    if (!lv && byFile.length) name = byFile[0];
    if (!lv && !byFile.length && !/(^|\/)slots65\.json$/.test(norm(sf)) && !args.includes("--selftest")) throw new Error("--slots=" + sf + " is no level's slots file in Tools/night/levels67.json: give --level=<L> too");
  }
  if (!name) return;
  const t = table[name];
  SLOTS = t.slots.slice(); ROOMS_DIR = t.rooms; DEFAULT_SLOTS = t.slotsFile; LEVEL_NAME = name; LEVEL_ID = t.id; BASE = t.base;
  GEN = name === "foundry" ? ["gen-n65-foundry.cjs"] : ["gen-slots.cjs", "--level", name];
}
// a room this tool may set: a slot of the level, and a key of the slots file when the file exists
function checkRoom(id, s, file) {
  if (!SLOTS.includes(id)) throw new Error(id + " is not a slot of " + LEVEL_ID + " (" + SLOTS.join(", ") + "; " + file + ")");
  if (fs.existsSync(abs(file)) && !(id in (s.slots || {}))) throw new Error(id + " is not in " + file + " (its slots: " + Object.keys(s.slots || {}).join(", ") + ")");
}

function readSlots(file) {
  if (fs.existsSync(abs(file))) return JSON.parse(fs.readFileSync(abs(file), "utf8"));
  const s = { _why: "NIGHT65 (the lead's; written only by Tools/night/n65-slot-set.cjs): which of " + LEVEL_ID + "'s fight-room slots ship their rebuilt room file (new, bound to its GATE PASS's sha and the file's blob there) and which keep " + BASE + "'s room (old). " + GEN.join(" ") + " --slots=" + file + " assembles the shipped level from it.", slots: {} };
  for (const id of SLOTS) s.slots[id] = { use: "old", why: BASE + "'s room until the rebuilt room PASSes its GATE" };
  return s;
}
function writeSlots(file, s, dry) {
  const text = JSON.stringify(s, null, 1) + "\n";
  if (dry) { console.log("(dry) would write " + file + ":\n" + text); return; }
  fs.mkdirSync(path.dirname(abs(file)), { recursive: true });
  fs.writeFileSync(abs(file), text);
}

// the checks; returns the slot entry or throws with the refusal
function passEntry(id, slatePath) {
  let slate;
  try { slate = JSON.parse(fs.readFileSync(abs(slatePath), "utf8")); } catch (e) { throw new Error("cannot read the slate " + slatePath + ": " + e.message); }
  const s = (Array.isArray(slate) ? slate : slate.slices || []).find((x) => x.id === id);
  if (!s) throw new Error(id + ": no slate slice " + id + " in " + slatePath);
  if (s.passes !== true) throw new Error(id + ": the slate slice has passes " + JSON.stringify(s.passes) + ": a slot goes new only after its GATE PASS");
  if (!s.passSha) throw new Error(id + ": the slate slice has no passSha (the sha the critic judged)");
  const first = s.verdict && fs.existsSync(abs(s.verdict)) ? fs.readFileSync(abs(s.verdict), "utf8").split(/\r?\n/)[0].trim() : "";
  if (first !== id + ": PASS") throw new Error(id + ": the verdict file " + JSON.stringify(s.verdict || null) + " does not start '" + id + ": PASS'");
  const file = roomPath(id);
  const sha = git(["rev-parse", "--verify", String(s.passSha) + "^{commit}"]);
  let blob;
  try { blob = git(["rev-parse", sha + ":" + file]); } catch (e) { throw new Error(id + ": " + file + " is not in passSha " + s.passSha); }
  // unchanged since passSha: no commit after it and no working-tree edit (git diff <sha> compares the sha with the working tree)
  const changed = git(["diff", "--name-only", sha, "--", file]);
  if (changed) throw new Error(id + ": " + file + " changed since its PASS at " + s.passSha + " (refused: GATE it again, or set passSha to the judged sha)");
  if (Array.isArray(s.passFiles) && !s.passFiles.map((f) => String(f).replace(/\\/g, "/")).includes(file)) console.log("WARN " + id + ": passFiles does not list " + file);
  return { use: "new", passSha: String(s.passSha), commit: sha, blob, verdict: s.verdict, set: new Date().toISOString() };
}

function main() {
  setLevel();
  const slotsFile = opt("--slots", DEFAULT_SLOTS);
  const slatePath = opt("--slate", "Logs/n65/slate.json");
  const dry = args.includes("--dry");
  if (args.includes("--print")) { const s = readSlots(slotsFile); console.log("n65-slot-set --print: " + LEVEL_ID + " (" + slotsFile + ")"); for (const id of SLOTS) console.log(id + ": " + (s.slots[id] ? JSON.stringify(s.slots[id]) : "old (no entry: gen reads a missing slot as old)")); return; }
  const [id, use, ...why] = args.filter((a) => !a.startsWith("--"));
  if (!["new", "old"].includes(use)) throw new Error("usage: n65-slot-set.cjs <" + SLOTS.join("|") + "> new|old [reason] [--level=<L>] [--slots=<file>] [--slate=<file>]");
  const s = readSlots(slotsFile);
  checkRoom(id, s, slotsFile);
  if (use === "new") s.slots[id] = passEntry(id, slatePath);
  else {
    const reason = why.join(" ").trim();
    if (reason.length < 12) throw new Error(id + " old: give the reason (a FAIL's verdict, a roll-back)");
    s.slots[id] = { use: "old", why: reason, set: new Date().toISOString() };
  }
  writeSlots(slotsFile, s, dry);
  console.log("n65-slot-set: " + id + " " + use + (use === "new" ? " (PASS " + s.slots[id].passSha + ", blob " + s.slots[id].blob.slice(0, 12) + ")" : "") + " -> " + slotsFile + "; slots: " + SLOTS.map((k) => k + " " + (s.slots[k] ? s.slots[k].use : "old (no entry)")).join(", "));
}

function selftest() {
  // planted cases on scratch slate / slots files; the room file is a2's committed file at HEAD (the case "changed" edits it and restores it)
  const tmp = fs.mkdtempSync(path.join(os.tmpdir(), "n65slot-"));
  const file = roomPath("a2");
  const head = git(["rev-parse", "HEAD"]);
  const ok = [];
  const orig = fs.readFileSync(abs(file));
  try {
    git(["rev-parse", head + ":" + file]); // a2's file must be committed at HEAD for the case
    if (git(["diff", "--name-only", head, "--", file])) throw new Error(file + " has a working-tree change: commit it before the selftest");
    const verdict = path.join(tmp, "verdict-a2.md");
    fs.writeFileSync(verdict, "a2: PASS\n(the selftest's planted verdict; the first line exactly as n65-freeze-rows.cjs reads it)\n");
    const slate = (passes) => { const p = path.join(tmp, "slate-" + passes + ".json"); fs.writeFileSync(p, JSON.stringify({ slices: [{ id: "a2", passes, passSha: head, passFiles: [file], verdict }] })); return p; };
    const slots = path.join(tmp, "slots65.json");
    const run = (slatePath) => { const s = readSlots(slots); s.slots.a2 = passEntry("a2", slatePath); writeSlots(slots, s, false); return s; };
    // 1. a PASS on an unchanged file sets the slot new, bound to the sha and the blob
    const s1 = run(slate(true));
    ok.push(["a PASS with the room file unchanged since passSha sets a2 new", s1.slots.a2.use === "new" && s1.slots.a2.blob === git(["rev-parse", head + ":" + file])]);
    // 2. PLANTED: the room file changed after the PASS (one box appended in the working tree) is refused
    const planted = JSON.parse(orig.toString("utf8"));
    planted.boxes.push({ zone: "a2", kind: "hull", min: [0, 0, -45], max: [1, 2, -44], job: "cover: planted", why: "the selftest's planted change" });
    fs.writeFileSync(abs(file), JSON.stringify(planted, null, 1) + "\n");
    let refused = "";
    try { run(slate(true)); } catch (e) { refused = e.message; }
    ok.push(["PLANTED a2.json changed after its PASS: refused (" + refused.slice(0, 140) + ")", /changed since its PASS/.test(refused)]);
    fs.writeFileSync(abs(file), orig);
    // 3. a slice with no PASS is refused
    let refused2 = "";
    try { run(slate(false)); } catch (e) { refused2 = e.message; }
    ok.push(["a slice with passes false: refused (" + refused2.slice(0, 100) + ")", /passes false/.test(refused2)]);
    // 4. the assembler refuses a slots65.json whose new slot's file changed (the same planted edit)
    fs.writeFileSync(abs(file), JSON.stringify(planted, null, 1) + "\n");
    const r = cp.spawnSync(process.execPath, [path.join(__dirname, "gen-n65-foundry.cjs"), "--slots=" + slots, "--dry-plan"], { cwd: ROOT, encoding: "utf8" });
    fs.writeFileSync(abs(file), orig);
    ok.push(["PLANTED gen-n65-foundry --slots=<that slots65.json> with a2.json changed: refused", r.status !== 0 && /changed since its PASS/.test(r.stderr + r.stdout)]);
    // 5. and takes it once the file is the judged one again
    const r2 = cp.spawnSync(process.execPath, [path.join(__dirname, "gen-n65-foundry.cjs"), "--slots=" + slots, "--dry-plan"], { cwd: ROOT, encoding: "utf8" });
    ok.push(["gen-n65-foundry --slots=<that slots65.json> with a2.json as judged: assembles (a2 new)", r2.status === 0 && /slot a2: new/.test(r2.stdout)]);
    // NIGHT67 T1b: the levels of levels67.json (the CLI, as the lead runs it)
    const cli = (a) => { const r = cp.spawnSync(process.execPath, [__filename].concat(a), { cwd: ROOT, encoding: "utf8" }); return { code: r.status, out: (r.stdout || "") + (r.stderr || "") }; };
    const kilnSlots = "Design/levels/n67/kiln/slots67.json";
    const c6 = cli(["kb3", "old", "the selftest's planted reason", "--level=kiln", "--slots=" + path.join(tmp, "x.json"), "--bogus"]);
    ok.push(["PLANTED an unknown option --bogus: refused (" + c6.out.trim().slice(0, 110) + ")", c6.code !== 0 && /unknown option --bogus/.test(c6.out)]);
    const c7 = cli(["kb3", "old", "the selftest's planted reason", "--slots", kilnSlots]);
    ok.push(["PLANTED '--slots <file>' with a space: refused (" + c7.out.trim().slice(0, 110) + ")", c7.code !== 0 && /needs '='/.test(c7.out)]);
    const s8 = path.join(tmp, "kiln-slots67.json");
    const c8 = cli(["g7", "old", "the selftest's planted reason", "--level=kiln", "--slots=" + s8]);
    ok.push(["PLANTED g7 (iceline's) with --level=kiln: refused (" + c8.out.trim().slice(0, 110) + ")", c8.code !== 0 && /not a slot of n67-kiln/.test(c8.out) && !fs.existsSync(s8)]);
    const c9 = cli(["kb3", "old", "the selftest's planted reason", "--level=kiln", "--slots=Design/levels/n65/slots65.json"]);
    ok.push(["PLANTED --level=kiln with level 1's slots65.json: refused (" + c9.out.trim().slice(0, 110) + ")", c9.code !== 0 && /is level foundry's/.test(c9.out)]);
    const c9b = cli(["kb3", "old", "the selftest's planted reason", "--slots=" + path.join(tmp, "nobody.json")]);
    ok.push(["PLANTED --slots=<a file no level owns> with no --level: refused, never a fall-back (" + c9b.out.trim().slice(0, 110) + ")", c9b.code !== 0 && /no level's slots file/.test(c9b.out)]);
    const s10 = path.join(tmp, "kiln-no-kb3.json");
    fs.writeFileSync(s10, JSON.stringify({ _why: "planted", slots: { kb2: { use: "old" }, kb2c: { use: "old" } } }, null, 1) + "\n");
    const c10 = cli(["kb3", "old", "the selftest's planted reason", "--level=kiln", "--slots=" + s10]);
    ok.push(["PLANTED kb3 against a kiln slots file without kb3: refused (" + c10.out.trim().slice(0, 110) + ")", c10.code !== 0 && /is not in /.test(c10.out)]);
    // a kiln PASS: kb3's room file committed at HEAD (the shared commit's export), a scratch slate, a scratch slots file -> kb3 new; gen-slots assembles it
    const kb3 = "Design/levels/n67/kiln/rooms/kb3.json";
    let committed = true;
    try { git(["rev-parse", head + ":" + kb3]); } catch (e) { committed = false; }
    if (!committed || git(["diff", "--name-only", head, "--", kb3])) ok.push(["NOT CHECKED: a kiln PASS (" + kb3 + " is not committed unchanged at HEAD in this tree: run again after the shared commit's sync)", null]);
    else {
      const v = path.join(tmp, "verdict-kb3.md");
      fs.writeFileSync(v, "kb3: PASS\n(the selftest's planted verdict)\n");
      const sl = path.join(tmp, "slate-kb3.json");
      fs.writeFileSync(sl, JSON.stringify({ slices: [{ id: "kb3", passes: true, passSha: head, passFiles: [kb3], verdict: v }] }));
      const s11 = path.join(tmp, "kiln-slots67.json");
      fs.copyFileSync(abs(kilnSlots), s11);
      const c11 = cli(["kb3", "new", "--level=kiln", "--slots=" + s11, "--slate=" + sl]);
      const w = JSON.parse(fs.readFileSync(s11, "utf8"));
      ok.push(["--level=kiln: a PASS sets kb3 new in the given slots file only (" + c11.out.trim().slice(0, 90) + ")", c11.code === 0 && w.slots.kb3.use === "new" && JSON.parse(fs.readFileSync(abs(kilnSlots), "utf8")).slots.kb3.use === "old"]);
      const g11 = cp.spawnSync(process.execPath, [path.join(__dirname, "gen-slots.cjs"), "--level", "kiln", "--slots=" + s11, "--dry-plan"], { cwd: ROOT, encoding: "utf8" });
      ok.push(["gen-slots --level kiln --slots=<that file> --dry-plan: assembles (kb3 new)", g11.status === 0 && /slot kb3: new/.test(g11.stdout)]);
    }
  } finally {
    fs.writeFileSync(abs(file), orig);
    fs.rmSync(tmp, { recursive: true, force: true });
  }
  if (git(["diff", "--name-only", head, "--", file])) { console.log("FAIL the room file was not restored"); process.exit(1); }
  for (const [name, pass] of ok) console.log((pass === null ? "" : pass ? "PASS " : "FAIL ") + name);
  const bad = ok.filter(([, p]) => p === false).length;
  console.log(bad ? "n65-slot-set selftest: " + bad + " FAIL" : "n65-slot-set selftest: PLANTED GREEN (" + ok.length + " cases; " + file + " restored, unchanged since HEAD)");
  process.exit(bad ? 1 : 0);
}

try { if (args.includes("--selftest")) selftest(); else main(); } catch (e) { console.error("n65-slot-set: REFUSED: " + e.message); process.exit(1); }

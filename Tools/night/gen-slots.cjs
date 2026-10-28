// NIGHT67 builder T (T1a; slate T1a; FORGE67b code-scout.md sections 2-3, DECISIONS D-5 / R-5, redteam.md 5 / 6): ANY level assembled from ROOM
// FILES in SLOTS, driven by the level table Tools/night/levels67.json (gen-n65-foundry.cjs, level 1's assembler, parameterised). The base level,
// its plan and its generator are never written here, so a level row off loads the base level bit for bit. Every fight room of the base plan
// (kind room) is a SLOT holding a ROOM FILE: the whole room row (every key), its zone-keyed pieces (boxes / ramps / pads / grapple / pits /
// tunnels), its room-keyed pieces (lights / props / surfaces, the level's own list in the table). Every other room (KEPT: links, the boss), every
// door and every level key is the base's bit for bit (asserted on the plan and again on the finished level).
// The three bugs a copy of gen-n65-foundry.cjs would ship (code-scout.md section 2), fixed here:
//   1. levelgen ERROR rows: the base level's own (by check name, levels67.json baseErrors: spire's v11 pit recovery) pass; only a row BEYOND them
//      refuses --run / --finish (gen-n65 refused any, so n52-spire could never pass);
//   2. the openings overlay: <dir>/openings<NN>.json's level key is SEEDED with the base level's openings (n57's for kiln), then a new slot's
//      `opening` overrides it (n59-l-room.py merges n52's openings53.json with the prefix's file only);
//   3. the surfaces list (iceline g4's ice floor) is carried with its room (--check refuses a plan list the table does not name).
// Files per level <L> (levels67.json; n67's layout, code-scout.md section 3):
//   Design/levels/n67/<L>/rooms-base/<id>.json  the base room exported exactly (never edited: --check re-exports and compares)
//   Design/levels/n67/<L>/rooms/<id>.json       the builder's room file (R / S edit only these); it starts as the export
//   Design/levels/n67/<L>/slots67.json          the lead's: per slot "old" or "new" + the PASS's sha (written by n65-slot-set.cjs --level=<L>)
//   Design/levels/n67/<L>.plan.json, <L>.json (levelgen's), <L>.assembly.json, openings67.json (one key per level), the level
//   Design/levels/n67-<L>.json and its StreamingAssets copy; the levelgen report Reports/LEVEL-n67-<L>-validate.md
// Run (from the tree's root; <L> = iceline | kiln | spire | foundry, or L67=<L> in the environment):
//   node Tools/night/gen-slots.cjs --level <L> [--slots=latest | --slots=old | --slots=g7,g8 | --slots=<slots67.json>] [--table] [--dry-plan]
//       the PLAN (as gen-n65-foundry.cjs: latest = every slot's rooms/<id>.json, old = every rooms-base/<id>.json, ids = those latest, a file =
//       the shipped set, a "new" slot's file must be the one its PASS bound); prints the per-key table of every rebuilt slot; refuses an unlisted
//       row key (KNOWN below: an `area67` block is refused by name), an unlisted file key, a drop with no reason, a locked key changed (id, kind,
//       rect), a piece keyed to another room, a new piece with no job / why
//   node Tools/night/gen-slots.cjs --level <L> --run [--slots=...] [--out=<dir>] [--no-build]   plan -> levelgen --walk -> the finish; a levelgen
//       ERROR row beyond the base's refuses BEFORE the finish (the shipped files' sha256 printed before and after); --out=<dir>: every file goes to
//       <dir> (<L>.plan.json, <L>.json, <L>.assembly.json, openings<NN>.json, LEVEL-<id>-validate.md, <id>.json), no tracked file is written
//   node Tools/night/gen-slots.cjs --level <L> --finish [--src=<levelgen file>]
//   node Tools/night/gen-slots.cjs --level <L> --check    every slot old (through the room-file path): the plan equals the base plan apart from id /
//       _why, levelgen + the finish equals the base level byte for byte apart from its names (name, _why, generatedBy), the base ERROR rows are
//       the table's; then the planted cases (one box moved 1 m prints the difference; an unknown row key, area67, a drop with no reason, an unknown
//       file key, a piece keyed to another room, a rect widened, a new piece with no job: each refused; verb65 / line65 accepted; an opening from a
//       latest slot overrides the seed; a rebuilt room changes only its slot through levelgen; a spawn lifted 40 m is refused beyond the base's
//       rows). Writes nothing of the lead's (a scratch folder, deleted after)
//   node Tools/night/gen-slots.cjs --level foundry --cross-n65 [--slots=old|latest]   gen-n65-foundry.cjs and this tool on level 1 into two scratch
//       folders: the plans byte for byte (and, with --run, the levels apart from names)
//   node Tools/night/gen-slots.cjs --level <L> --export [ids] [--force]   rooms-base/<id>.json and, when missing (or --force), rooms/<id>.json
//   node Tools/night/gen-slots.cjs --level <L> --info[=<field>]   the level's resolved table as JSON (or one field; a list space-joined), for the
//       round tools' shell scripts (slots, id, base, planOut, levelOut, roomsDir, slotsFile, row, ...)
// The plan step needs no dotnet; --run / --check build the SimHarness once (dotnet build, never `dotnet run`) and run its DLL under a timeout.
const fs = require("fs");
const path = require("path");
const os = require("os");
const cp = require("child_process");
const crypto = require("crypto");
const ROOT = path.join(__dirname, "../..");
const TABLE_FILE = "Tools/night/levels67.json";
const DLL = "Temp/SimHarness/bin/Release/net10.0/SimHarness.dll";
const LEVELGEN_TIMEOUT_MS = 900000; // a levelgen --walk runs ~10-60 s; 15 min is a hang

const rel = (p) => (path.isAbsolute(p) ? p : path.join(ROOT, p));
const read = (p) => fs.readFileSync(rel(p), "utf8");
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const eolOf = (t) => (t.includes("\r\n") ? "\r\n" : "\n");
const isDoc = (k) => /(_why|Why)$/.test(k);
const sha256 = (p) => crypto.createHash("sha256").update(fs.readFileSync(rel(p))).digest("hex");

// ---------------------------------------------------------------- the arguments (an unknown option is refused by name)
const argv = process.argv.slice(2);
const OPTS = ["--level", "--slots", "--table", "--dry-plan", "--run", "--out", "--no-build", "--finish", "--src", "--check", "--cross-n65", "--export", "--force", "--info", "--room-file", "--plant-error"];
let LEVEL = process.env.L67 || null;
const args = [];
for (let i = 0; i < argv.length; i++) {
  const a = argv[i];
  if (a === "--level") { LEVEL = argv[++i]; continue; }
  if (a.startsWith("--level=")) { LEVEL = a.slice(8); continue; }
  args.push(a);
}
const flag = (f) => args.includes(f);
const opt = (name) => { const a = args.find((x) => x.startsWith(name + "=")); return a ? a.slice(name.length + 1) : null; };
function refuseUnknownOptions() {
  for (const a of args) if (a.startsWith("--") && !OPTS.includes(a.split("=")[0])) throw new Error("unknown option " + a + " (known: " + OPTS.join(" ") + ")");
}

// ---------------------------------------------------------------- the level, from the table
const TABLE = JSON.parse(read(TABLE_FILE));
const LEVELS = Object.keys(TABLE).filter((k) => !k.startsWith("_"));
function levelOf(name) {
  if (!name) throw new Error("no level: --level <" + LEVELS.join("|") + "> (or L67=<L>)");
  const t = TABLE[name];
  if (!t || name.startsWith("_")) throw new Error("--level " + name + ": not in " + TABLE_FILE + " (" + LEVELS.join(", ") + ")");
  return t;
}
let T, ID, PX, PLAN_IN, BASE_FILE, DIR, ROOMS, OLD, SLOTS_FILE, SLOTS, ZONE_KEYED, ROOM_KEYED, LISTS, FILE_KEYS, OUT_DIR;
let PLAN_OUT, GEN_OUT, USED, OPENINGS_OUT, REPORT, OUTS, TRACKED_OUTS;
function setLevel(name, outDir) {
  T = levelOf(name);
  ID = T.id; PX = ID.split("-")[0];
  PLAN_IN = T.planIn; BASE_FILE = T.baseFile; DIR = T.dir; ROOMS = T.rooms; OLD = T.roomsBase; SLOTS_FILE = T.slotsFile; SLOTS = T.slots;
  ZONE_KEYED = T.zoneKeyed; ROOM_KEYED = T.roomKeyed.concat(T.extraLists || []);
  LISTS = ZONE_KEYED.concat(ROOM_KEYED);
  FILE_KEYS = ["_why", "room", "row"].concat(LISTS, ["drop", "opening", "opening_why"]);
  OUT_DIR = outDir ? outDir.replace(/\\/g, "/").replace(/\/$/, "") : null;
  const D = OUT_DIR || DIR;
  PLAN_OUT = D + "/" + name + ".plan.json"; GEN_OUT = D + "/" + name + ".json"; USED = D + "/" + name + ".assembly.json";
  OPENINGS_OUT = D + "/" + T.openings.file;
  TRACKED_OUTS = ["Design/levels/" + ID + ".json", "Assets/StreamingAssets/Design/levels/" + ID + ".json"];
  OUTS = OUT_DIR ? [OUT_DIR + "/" + ID + ".json"] : TRACKED_OUTS;
  REPORT = OUT_DIR ? OUT_DIR + "/LEVEL-" + ID + "-validate.md" : "Reports/LEVEL-" + ID + "-validate.md";
}
const zoneOwned = (k) => ZONE_KEYED.includes(k);
const owner = (k, o) => (zoneOwned(k) ? o.zone : o.room);

// Every room-row key a level has carried (gen-n65-foundry.cjs's KNOWN, NIGHT24-NIGHT65; code-scout.md section 2: it accepts every fight-room
// row key of the other four plans). A key not listed is refused: a new key needs its reader named here first. Doc keys (ending _why or Why) are
// always allowed. NIGHT67 (redteam.md 6): `area67` is NOT a key (research.md's proposal; the lint infers from the geometry): refused by name.
const KNOWN = ["_note", "afterLesson43", "arena36", "arenas33", "award", "blastGap", "boss", "botMedian", "brawl40", "bridge36", "bridges45", "card57",
  "climbTop48", "copied38", "crown32", "crumble31", "d3", "dashGap", "deckBreak31", "devices33", "exit", "fall37", "fallReturn43", "flight38", "frames36",
  "frames47", "frames52", "gapLz31", "grapple", "grappleNodes", "hallRoute", "hazard", "height", "highNodes", "hoop37", "id", "identity45", "identity46",
  "job36", "kind", "kneeTarget37", "lantern36", "lessons", "lessons31", "limit", "limitReason", "lines34", "loom37", "loom40NoRide", "loom47Posts",
  "loomLegs", "loomRoom37", "loomSwing37", "meter38", "name", "nodes", "noride38", "owns38", "pacer37", "par", "parCap", "pickup", "pickup31", "proofs",
  "props45", "props46", "pull38", "reachFrom", "rect", "respawnY", "ride35", "rims36", "ringRoute32", "roles43", "roof", "routeVia", "run38", "shortcuts",
  "sink31", "spawn", "spire37", "stag37", "standIns", "standIns31", "standIns36", "tops43", "verb36", "wallRoute", "waveNodes43", "waves", "waves31",
  "wick37", "wick47",
  // NIGHT65: frames65, line65 (the card's verb line, [[x, y, z], ...]), bot43 (false: the walk bot's old id scripts skip the room), card65, verb65
  // (the room's verb for the verb-aware pilot: launch | launch+slide | pierce | ride | parry | slide; Harness.N65.VerbPilot.cs)
  "frames65", "line65", "bot43", "card65", "verb65"];
const LOCKED = ["id", "kind", "rect"]; // the doors fix every rect; the id is the slot; the kind a fight room
const OPENINGS = ["vista", "ambushed", "ambusher", "held"]; // openings53.json's four (NIGHT53 I7)

// ---------------------------------------------------------------- the export: one slot's room out of the base plan, exactly
function exportRoom(plan, id) {
  const row = plan.rooms.find((r) => r.id === id);
  if (!row) throw new Error("no room " + id + " in " + PLAN_IN);
  const f = { _why: "NIGHT67 T1a (Tools/night/gen-slots.cjs --level " + LEVEL + " --export): " + T.base + "'s room " + id + " (" + row.name + ") exactly as " + PLAN_IN + " has it: the row (every key), its zone-keyed pieces (" + ZONE_KEYED.join(", ") + "), its room-keyed pieces (" + ROOM_KEYED.join(", ") + "). The slot's starting point: rebuild it here (every new piece with a job and a why), give the row bot43: false, verb65 (the slate's verb) and line65 (the verb's line, 3+ waypoints); never an area67 block (refused). A key the row drops needs a reason in drop{}. The room's opening: top-level \"opening\": \"vista\" | \"ambushed\" | \"ambusher\" | \"held\" (and \"opening_why\"); left out, the room keeps " + T.base + "'s.", room: id, row };
  for (const k of LISTS) f[k] = (plan[k] || []).filter((o) => owner(k, o) === id);
  f.drop = {};
  return f;
}
// one line per piece / per row key, so a builder's diff is a line per change (gen-n65-foundry.cjs's roomText)
function roomText(f) {
  const lines = ["{"];
  const keys = Object.keys(f);
  keys.forEach((k, i) => {
    const v = f[k], comma = i < keys.length - 1 ? "," : "";
    if (Array.isArray(v)) {
      if (!v.length) lines.push(" " + JSON.stringify(k) + ": []" + comma);
      else { lines.push(" " + JSON.stringify(k) + ": ["); v.forEach((o, j) => lines.push("  " + JSON.stringify(o) + (j < v.length - 1 ? "," : ""))); lines.push(" ]" + comma); }
    } else if (v && typeof v === "object") {
      const ks = Object.keys(v);
      if (!ks.length) lines.push(" " + JSON.stringify(k) + ": {}" + comma);
      else { lines.push(" " + JSON.stringify(k) + ": {"); ks.forEach((kk, j) => lines.push("  " + JSON.stringify(kk) + ": " + JSON.stringify(v[kk]) + (j < ks.length - 1 ? "," : ""))); lines.push(" }" + comma); }
    } else lines.push(" " + JSON.stringify(k) + ": " + JSON.stringify(v) + comma);
  });
  lines.push("}");
  return lines.join("\n") + "\n";
}

// ---------------------------------------------------------------- the slot set: which file each slot takes
function slotSet(spec) {
  const out = {};
  const latest = (id) => ({ use: "latest", file: ROOMS + "/" + id + ".json" });
  const old = (id) => ({ use: "old", file: OLD + "/" + id + ".json" });
  if (spec === "latest") for (const id of SLOTS) out[id] = latest(id);
  else if (spec === "old") for (const id of SLOTS) out[id] = old(id);
  else if (/\.json$/.test(spec)) {
    const s = JSON.parse(read(spec));
    for (const k of Object.keys(s.slots || {})) if (!SLOTS.includes(k)) throw new Error(spec + ": slot " + k + " is not a slot of " + LEVEL + " (" + SLOTS.join(", ") + ")");
    for (const id of SLOTS) {
      const e = (s.slots || {})[id] || { use: "old" };
      if (e.use === "old") { out[id] = old(id); continue; }
      if (e.use !== "new") throw new Error(spec + ": slot " + id + " use '" + e.use + "' (old or new only)");
      // a PASS binds to its sha (NIGHT67.md rule 8): the file shipped is the one the critic judged, or the slot is refused
      const file = ROOMS + "/" + id + ".json";
      let atPass;
      try { atPass = cp.execFileSync("git", ["show", e.passSha + ":" + file], { cwd: ROOT, encoding: "utf8", maxBuffer: 64 << 20 }); } catch (err) { throw new Error(spec + ": slot " + id + ": " + file + " is not in its passSha " + e.passSha); }
      if (!same(JSON.parse(atPass), JSON.parse(read(file)))) throw new Error(spec + ": slot " + id + ": " + file + " changed since its PASS at " + e.passSha + ": GATE it again, or `node Tools/night/n65-slot-set.cjs " + id + " old --level=" + LEVEL + " --slots=" + SLOTS_FILE + "`");
      out[id] = { use: "new", file, passSha: e.passSha };
    }
  } else {
    const ids = spec.split(",").filter(Boolean);
    for (const id of ids) if (!SLOTS.includes(id)) throw new Error("--slots=" + spec + ": " + id + " is not a slot of " + LEVEL + " (" + SLOTS.join(", ") + ")");
    for (const id of SLOTS) out[id] = ids.includes(id) ? latest(id) : old(id);
  }
  return out;
}

// ---------------------------------------------------------------- one room file checked against the base room: the per-key table, or a refusal
function checkRoom(id, f, base, where) {
  const fail = (m) => { throw new Error(where + ": " + m); };
  for (const k of Object.keys(f)) if (!FILE_KEYS.includes(k)) fail("unlisted file key '" + k + "' (a " + LEVEL + " room file holds " + FILE_KEYS.join(", ") + ")");
  if (f.room !== id) fail("room '" + f.room + "', expected " + id);
  const row = f.row;
  if (!row || typeof row !== "object") fail("no row");
  const drop = f.drop || {};
  const table = [];
  if ("area67" in row) fail("unlisted row key 'area67' (refused: research.md's area67 block is a proposal, not a room field; the lint infers from the geometry: redteam.md 6)");
  for (const k of Object.keys(row)) if (!KNOWN.includes(k) && !isDoc(k)) fail("unlisted row key '" + k + "' (refused: name its reader in gen-slots.cjs KNOWN first)");
  for (const k of LOCKED) if (!same(row[k], base.row[k])) fail("row." + k + " " + JSON.stringify(row[k]) + " is the slot's (" + JSON.stringify(base.row[k]) + "): the doors fix it");
  for (const k of Object.keys(drop)) {
    if (k in row) fail("drop." + k + " but the row still has " + k);
    if (!(k in base.row)) fail("drop." + k + ": " + T.base + "'s row has no " + k);
    if (String(drop[k] || "").trim().length < 12) fail("drop." + k + ": give the reason (who read it, why it can go)");
  }
  for (const k of Object.keys(base.row)) if (!(k in row)) {
    if (!(k in drop)) fail("row key '" + k + "' dropped with no reason (add drop." + k + ": \"<reason>\"; nothing lost, PRINCIPLES 1)");
    table.push([k, "drop", drop[k]]);
  }
  for (const k of Object.keys(row)) table.push([k, !(k in base.row) ? "add" : same(row[k], base.row[k]) ? "keep" : "replace", ""]);
  for (const k of LISTS) {
    const arr = f[k] || [];
    if (!Array.isArray(arr)) fail(k + " is not a list");
    for (const o of arr) if (owner(k, o) !== id) fail(k + ": a row keyed to '" + owner(k, o) + "', not " + id + ": " + JSON.stringify(o).slice(0, 160));
  }
  if ("opening" in f && !OPENINGS.includes(f.opening)) fail("opening '" + f.opening + "' (one of " + OPENINGS.join(", ") + "; leave it out to keep " + T.base + "'s)");
  if ("opening_why" in f && !("opening" in f)) fail("opening_why with no opening");
  const edited = !same(stripWhy(f), stripWhy(base));
  // every NEW or changed piece carries a job and a why (the room checker counts them); the base's own pieces, kept as they were, pass as they are
  for (const k of ZONE_KEYED.concat(["props"])) {
    if (!LISTS.includes(k)) continue;
    const old = new Set((base[k] || []).map((o) => JSON.stringify(o)));
    for (const o of f[k] || []) if (!old.has(JSON.stringify(o)) && !(String(o.job || "").trim() && String(o.why || "").trim())) fail(k + ": a new piece without job / why: " + JSON.stringify(o).slice(0, 160));
  }
  return { table, edited };
}
const stripWhy = (f) => { const c = Object.assign({}, f, { _why: "" }); delete c.opening; delete c.opening_why; return c; };
function withNotes(f) {
  let n = 0;
  for (const k of ZONE_KEYED) for (const o of f[k] || []) if (o.why && o.note === undefined) { o.note = o.why; n++; }
  return n;
}
const roomFile = (p) => JSON.parse(read(p));

// ---------------------------------------------------------------- the assembly: the base plan with each slot's file in place
function assemble(plan0, files, opts) {
  const plan = JSON.parse(JSON.stringify(plan0));
  const report = [];
  for (const id of SLOTS) {
    const f = files[id];
    const base = exportRoom(plan0, id);
    const { table, edited } = checkRoom(id, f.data, base, f.file);
    const notes = withNotes(f.data);
    const i = plan.rooms.findIndex((r) => r.id === id);
    plan.rooms[i] = f.data.row;
    for (const k of LISTS) {
      const mine = (o) => owner(k, o) === id;
      const fresh = f.data[k] || [];
      const arr = plan[k] || [];
      if (same(arr.filter(mine), fresh)) continue;
      const at = arr.findIndex(mine);
      const rest = arr.filter((o) => !mine(o));
      rest.splice(at < 0 ? rest.length : at, 0, ...fresh);
      plan[k] = rest;
    }
    const counts = LISTS.map((k) => k + " " + base[k].length + (base[k].length !== (f.data[k] || []).length || !same(base[k], f.data[k] || []) ? " -> " + (f.data[k] || []).length + (same(base[k], f.data[k] || []) ? "" : "*") : "")).join(", ");
    const tally = (a) => table.filter((t) => t[1] === a).map((t) => t[0]);
    report.push({ id, use: f.use, file: f.file, edited, table, counts, notes, keep: tally("keep"), replace: tally("replace"), add: tally("add"), drop: tally("drop"), verb65: f.data.row.verb65 || null });
    if (edited && f.data.row.bot43 !== false) report[report.length - 1].warn = "WARN: rebuilt but the row has no bot43: false (the walk bot's old id scripts would drive the new geometry)";
  }
  // the guards: nothing outside the slots changed
  for (const r of plan0.rooms) if (!SLOTS.includes(r.id) && !same(r, plan.rooms.find((q) => q.id === r.id))) throw new Error("room changed: " + r.id);
  if (!same(plan0.rooms.map((r) => r.id), plan.rooms.map((r) => r.id))) throw new Error("room order changed");
  for (const k of LISTS) {
    const outside = (list) => (list || []).filter((o) => !SLOTS.includes(owner(k, o)));
    if (!same(outside(plan0[k]), outside(plan[k]))) throw new Error(k + ": a row outside the slots changed");
  }
  if (!same(plan0.doors, plan.doors)) throw new Error("doors changed (the lead's: the doors fix every rect)");
  for (const k of Object.keys(plan0)) if (!["_why", "id", "rooms"].concat(LISTS).includes(k) && !same(plan0[k], plan[k])) throw new Error(k + " changed");
  plan.id = opts.id || ID;
  plan._why = T.planWhy + ": " + T.base + "'s plan with " + SLOTS.length + " slots from room files (" + report.map((r) => r.id + " " + r.use + (r.edited ? " (rebuilt)" : " (= " + T.base.split("-")[0] + ")")).join(", ") + "). Before it: " + plan0._why;
  return { plan, report };
}
function printReport(report, all) {
  for (const r of report) {
    const head = "  slot " + r.id + ": " + r.use + " " + r.file + (r.passSha ? " (PASS " + r.passSha + ")" : "") + (r.edited ? ": REBUILT" : ": " + T.base + "'s room exactly");
    console.log(head + "; " + r.counts + (r.notes ? "; note = why filled on " + r.notes : "") + (r.verb65 ? "; verb65 " + r.verb65 : ""));
    if (!r.edited && !all) continue;
    console.log("    row keys: keep " + r.keep.length + ", replace " + r.replace.length + ", add " + r.add.length + ", drop " + r.drop.length);
    for (const [k, a, why] of r.table) if (all || a !== "keep") console.log("    " + (a + "       ").slice(0, 8) + k + (why ? ": " + why : ""));
    if (r.warn) console.log("    " + r.warn);
  }
}
function loadFiles(set) {
  const files = {};
  for (const id of SLOTS) {
    const s = set[id];
    if (!fs.existsSync(rel(s.file))) throw new Error("slot " + id + ": no " + s.file + " (run --export first)");
    files[id] = Object.assign({}, s, { data: roomFile(s.file) });
  }
  return files;
}
// the openings: the level's key SEEDED with the base level's openings (bug 2), then a slot that is not old and whose file sets `opening`
// overrides its room; other levels' keys in the same file are kept (n67's openings67.json holds one key per level); no timestamp
function openingsOf(files, prev) {
  const seed = T.openings.seed ? (JSON.parse(read(T.openings.seed))[T.openings.key] || {}) : {};
  const o = prev && typeof prev === "object" ? JSON.parse(JSON.stringify(prev)) : {};
  o._why = T.openings._why || "NIGHT67 (generated by Tools/night/gen-slots.cjs from the room files in their slots; the lead's, never committed by a builder): one key per level (" + LEVELS.filter((l) => TABLE[l].openings.file === T.openings.file).join(", ") + "): the level's openings SEEDED with its base level's (" + LEVELS.filter((l) => TABLE[l].openings.file === T.openings.file).map((l) => l + " " + (TABLE[l].openings.seed || "none")).join(", ") + "), then each slot that is not old and whose room file sets `opening` (vista / ambushed / ambusher / held) overrides its room. Overlaid by Tools/night/n59-l-room.py on Design/levels/n52/openings53.json.";
  const lv = Object.assign({}, seed);
  const cards = Object.assign({}, o._cards || {});
  for (const id of SLOTS) delete cards[id];
  for (const id of SLOTS) {
    const f = files[id];
    if (f.use === "old" || !("opening" in f.data)) continue;
    lv[id] = f.data.opening;
    if (f.data.opening_why) cards[id] = f.data.opening_why;
  }
  const out = { _why: o._why };
  for (const k of Object.keys(o)) if (k !== "_why" && k !== "_cards") out[k] = o[k];
  out[T.openings.key] = lv;
  out._cards = cards;
  return out;
}
function writePlanFile(plan, report, spec, files) {
  fs.mkdirSync(rel(path.dirname(PLAN_OUT)), { recursive: true });
  const prev = !OUT_DIR && fs.existsSync(rel(OPENINGS_OUT)) ? JSON.parse(read(OPENINGS_OUT)) : null;
  fs.writeFileSync(rel(OPENINGS_OUT), JSON.stringify(openingsOf(files, prev), null, 1) + "\n");
  const text = read(PLAN_IN);
  const eol = eolOf(text);
  fs.writeFileSync(rel(PLAN_OUT), JSON.stringify(plan, null, 1).replace(/\n/g, eol) + eol);
  fs.writeFileSync(rel(USED), JSON.stringify({ _why: "NIGHT67: what the last gen-slots.cjs --level " + LEVEL + " plan step assembled (read by --finish for generatedBy; the lead's, never committed by a builder)", level: LEVEL, slots: spec, plan: PLAN_OUT, at: new Date().toISOString(), rooms: report.map((r) => ({ id: r.id, use: r.use, file: r.file, rebuilt: r.edited, passSha: r.passSha || null, verb65: r.verb65 })) }, null, 1) + "\n");
}

// ---------------------------------------------------------------- the finish: the level from levelgen's file
// foundry-a9 (level 1): gen-n52-foundry.cjs lines 429-473 as gen-n65-foundry.cjs keeps them (a9's n47 / n48 keys, data.name n46-foundry)
function finishFoundryA9(lvl) {
  lvl.data.name = "n46-foundry";
  const rooms = lvl.campaign.rooms;
  const a9 = rooms.find((r) => r.id === "a9");
  if (!a9 || a9.boss !== "wick" || !a9.wick37) throw new Error("a9 (THE STACK, wick) missing");
  const c = a9.wick37.centre;
  a9.job36 = "boss";
  a9.verb36 = "P1 the parry on the floor, P2 the climb (ride, leap), P3 the punch at the open core (the exam)";
  a9.par = 150;
  a9.par_why = "NIGHT47 W: the human band's top (NIGHT47.md section 3: median 90-150 s), was 22 (the bot's).";
  a9.wick47 = {
    _why: "NIGHT47 W (rows wick47 / climb47 / wickTop47; W-cards.md section 1): THE WICK in three phases in this shaft: P1 THE FLOOR (the panels folded into the wall, 3 slag vents in the 9 m floor), P2 THE CLIMB (at 55 % the steps and the panels come out of the wall: 5 plain STEPS 1.4 m apart (a jump, no ride: round 3), then 8 ride panels from ledge 1 (the last step, 7 m) rising 2.5 / 2.8 m, ledge 2 at 16.2 m, the net), P3 THE CAP at 26.3 m (the glass slides over the shaft; the crack-open charge). The numbers are config, not this row: design.n47.wick47.numbers (the stack, the vents, the attacks) and design.n47.climb47.numbers; this row is the room's note for the reels and docs/ROOMS.md.",
    phases: ["P1 THE FLOOR", "P2 THE CLIMB", "P3 THE CAP"],
  };
  const at = (x, y, z, yaw, pitch) => [c[0] + x, c[1] + y, c[2] + z, yaw, pitch];
  a9.frames47 = {
    _why: "NIGHT47 W: reel a147's spots (the lead's frames): RED on n47-r1 with the rows off, GREEN with them on.",
    p1Entry: at(0, 0, 10.5, 0, 0.15),
    p1Floor: at(0, 0, 5, 0, 0.35),
    p2Climb: at(-7.5, 1.45, 0, 180, 0.3),
    capEdge: at(0, 26.3, 11, 0, 0.1),
    p3Cap: at(0, 26.3, 6, 0, 0.1),
  };
  a9.climbTop48 = {
    _why: "NIGHT48 W (level row climbTop48; G1): THE STACK's top third gets a LIP at the far end of each top-third panel (after panels 4, 5 and 6 of n47's stack): a catch shelf a little under the rider's feet at the panel's end, against the wall, so a release held past the panel's end lands on it and a short hop rides the next panel. The numbers are config, not this row: design.n48.climbTop48.numbers (lipAfter, lipOver, lipArcDeg, lipStartDeg, lipDepth); SurvivalSim.N48.Wick.cs reads this key's presence only.",
    lips: true,
  };
}
function finishLevel(lvl, base, usedLine) {
  if (!lvl.data || !lvl.campaign) throw new Error("not a levelgen level file");
  if (T.finish === "foundry-a9") finishFoundryA9(lvl);
  else if (T.finish === "data-name") lvl.data.name = base.data.name; // the older systems' name the base level carries (levelgen writes the plan id)
  else throw new Error("finish '" + T.finish + "' unknown (" + TABLE_FILE + ": data-name | foundry-a9)");
  // the guards against the base level (read only): every kept room bit-equal, the room order, the doors, every campaign row outside the slots
  const rooms = lvl.campaign.rooms, brooms = base.campaign.rooms;
  if (!same(rooms.map((r) => r.id), brooms.map((r) => r.id))) throw new Error("room order changed");
  for (const r of brooms) if (!SLOTS.includes(r.id) && !same(r, rooms.find((q) => q.id === r.id))) throw new Error("kept room changed: " + r.id + " is not " + T.base + "'s bit for bit");
  if (!same(lvl.campaign.doors, base.campaign.doors)) throw new Error("doors changed");
  const own = (o) => o && typeof o === "object" && SLOTS.includes(o.zone || o.room);
  for (const k of new Set(Object.keys(base.campaign).concat(Object.keys(lvl.campaign)))) {
    if (k === "rooms" || k === "doors") continue;
    const a = lvl.campaign[k], b = base.campaign[k];
    if (Array.isArray(a) || Array.isArray(b)) { if (!same((a || []).filter((o) => !own(o)), (b || []).filter((o) => !own(o)))) throw new Error("campaign." + k + ": a row outside the slots differs from " + T.base + "'s"); }
    else if (!same(a, b)) throw new Error("campaign." + k + " differs from " + T.base + "'s");
  }
  lvl.name = ID;
  lvl._why = T.planWhy + " (" + T.row + " swaps " + T.base + " for this file while the row is on; NIGHT67.md THE AREAS: fight rooms rebuilt as areas from cards in played rounds, GG 2026-10-24 'area deisgn need t be better in alot of levels'): levelgen of " + PLAN_OUT + " (" + T.base + "'s plan; " + usedLine + "), then the finish (" + T.finish + "). " + T.base + "'s _why: " + base._why;
  lvl.generatedBy = "Tools/night/gen-slots.cjs --level " + LEVEL + " (" + usedLine + "; plan " + PLAN_OUT + " -> levelgen -> --finish); " + lvl.generatedBy;
  return lvl;
}
function serLike(raw) {
  const eol = eolOf(raw);
  const compact = !raw.trimEnd().includes("\n");
  return (o) => (compact ? JSON.stringify(o) + (raw.endsWith("\n") ? eol : "") : JSON.stringify(o, null, 1).replace(/\n/g, eol) + eol);
}
function usedLineOf() {
  if (!fs.existsSync(rel(USED))) return "slots: not recorded";
  const u = JSON.parse(read(USED));
  return "slots " + u.slots + ": " + u.rooms.map((r) => r.id + " " + r.use + (r.rebuilt ? " rebuilt" : "") + (r.passSha ? " @" + r.passSha : "")).join(", ");
}

// ---------------------------------------------------------------- the levelgen report's ERROR rows, against the base level's own (bug 1)
function errorRows(reportText) {
  return reportText.split(/\r?\n/).filter((l) => l.includes("| ERROR |")).map((l) => ({ name: l.split("|")[1].trim(), text: l.trim() }));
}
function beyondBase(rows) { return rows.filter((r) => !T.baseErrors.includes(r.name)); }

function finish() {
  const src = opt("--src") || GEN_OUT;
  if (!fs.existsSync(rel(REPORT))) throw new Error("--finish: no levelgen report " + REPORT + " for " + src + " (run --run)");
  const rows = errorRows(read(REPORT)), beyond = beyondBase(rows);
  if (beyond.length) throw new Error("--finish: the levelgen report " + REPORT + " lists " + beyond.length + " ERROR row(s) beyond " + T.base + "'s own (" + beyond[0].text.slice(0, 200) + "): the level is not shipped");
  if (!opt("--src") && fs.statSync(rel(REPORT)).mtimeMs + 1 < fs.statSync(rel(src)).mtimeMs) throw new Error("--finish: " + src + " is newer than its report " + REPORT + " (re-run levelgen: --run)");
  const lvl = JSON.parse(read(src));
  if (lvl.name !== ID) throw new Error(src + ": name " + lvl.name + ", expected " + ID + " (levelgen of " + PLAN_OUT + ")");
  const baseRaw = read(BASE_FILE);
  finishLevel(lvl, JSON.parse(baseRaw), usedLineOf());
  const text = serLike(baseRaw)(lvl);
  for (const o of OUTS) { fs.mkdirSync(path.dirname(rel(o)), { recursive: true }); fs.writeFileSync(rel(o), text); }
  if (OUTS.length > 1 && read(OUTS[0]) !== read(OUTS[1])) throw new Error("copies differ");
  console.log(ID + " --finish: " + src + " -> " + OUTS.join(", ") + " (" + text.length + " chars" + (OUTS.length > 1 ? ", both copies identical" : "") + "); kept rooms bit-equal to " + T.base + "'s; ERROR rows " + rows.length + " (" + (rows.length ? rows.map((r) => r.name).join("; ") + ": all " + T.base + "'s own" : "none") + "); " + usedLineOf());
}
function shipShas() { return TRACKED_OUTS.map((o) => o + " " + (fs.existsSync(rel(o)) ? sha256(o) : "missing")).join(", "); }

// ---------------------------------------------------------------- levelgen (the SimHarness DLL, built once per process; never `dotnet run`)
let built = false;
function ensureBuilt() {
  if (built || flag("--no-build")) { built = true; return; }
  const r = cp.spawnSync("dotnet", ["build", "Tools/SimHarness", "-c", "Release", "-nologo", "-v", "q"], { cwd: ROOT, encoding: "utf8", maxBuffer: 64 << 20, timeout: LEVELGEN_TIMEOUT_MS });
  if (r.status !== 0) throw new Error("dotnet build Tools/SimHarness failed (exit " + r.status + "): " + ((r.stdout || "") + (r.stderr || "")).split(/\r?\n/).filter((l) => /error/.test(l)).slice(0, 5).join(" | "));
  built = true;
}
function levelgen(planPath, outPath, reportPath) {
  ensureBuilt();
  const t0 = Date.now();
  const r = cp.spawnSync("dotnet", [DLL, "levelgen", planPath, outPath, "--walk", "--report", rel(reportPath)], { cwd: ROOT, encoding: "utf8", maxBuffer: 64 << 20, timeout: LEVELGEN_TIMEOUT_MS });
  const all = (r.stdout || "") + (r.stderr || "");
  const keep = all.split(/\r?\n/).filter((l) => /ERROR|error CS|plan checks|level report|FAIL|problem|  ground|  air|bot walk|^levelgen /.test(l)).map((l) => l.slice(0, 300));
  const rep = all.match(/level report: (\d+) errors?/);
  const rows = fs.existsSync(rel(reportPath)) ? errorRows(fs.readFileSync(rel(reportPath), "utf8")) : [];
  return { code: r.status, lines: keep, errors: rep ? Number(rep[1]) : null, rows, all, secs: Math.round((Date.now() - t0) / 1000), timedOut: r.error && r.error.code === "ETIMEDOUT" };
}

// ---------------------------------------------------------------- the diff of two level files, names aside
const NAMES = ["name", "_why", "generatedBy"];
function levelDiff(a, b) {
  const out = [];
  for (const k of new Set([...Object.keys(a), ...Object.keys(b)])) if (!NAMES.includes(k) && k !== "campaign" && !same(a[k], b[k])) out.push("top: " + k);
  const ca = a.campaign || {}, cb = b.campaign || {};
  for (const k of new Set([...Object.keys(ca), ...Object.keys(cb)])) if (k !== "rooms" && !same(ca[k], cb[k])) {
    const la = ca[k] || [], lb = cb[k] || [];
    if (Array.isArray(la) && Array.isArray(lb)) {
      const sa = new Set(la.map((o) => JSON.stringify(o))), sb = new Set(lb.map((o) => JSON.stringify(o)));
      const gone = la.filter((o) => !sb.has(JSON.stringify(o))), added = lb.filter((o) => !sa.has(JSON.stringify(o)));
      out.push("campaign." + k + ": " + gone.length + " row(s) only in the base, " + added.length + " only in the new" + (gone.length + added.length <= 4 ? " (" + gone.map((o) => "base " + JSON.stringify(o).slice(0, 140)).concat(added.map((o) => "new " + JSON.stringify(o).slice(0, 140))).join("; ") + ")" : ""));
    } else out.push("campaign." + k);
  }
  const ra = new Map((ca.rooms || []).map((r) => [r.id, r])), rb = new Map((cb.rooms || []).map((r) => [r.id, r]));
  for (const id of new Set([...ra.keys(), ...rb.keys()])) {
    const x = ra.get(id) || {}, y = rb.get(id) || {};
    const d = [...new Set([...Object.keys(x), ...Object.keys(y)])].filter((k) => !same(x[k], y[k]));
    if (d.length) out.push("campaign.rooms " + id + ": " + d.join(", "));
  }
  return out;
}

// a planted piece's spot: on a 1 m grid inside the rect (3 m in from the walls and doors), the one farthest from every [x, y, z] the row
// names anywhere (spawn, nodes, waves, lessons) and from every box's footprint
function freeSpot(row, boxes) {
  const pts = [];
  (function walk(o) { if (Array.isArray(o)) { if (o.length >= 3 && o.slice(0, 3).every((v) => typeof v === "number")) pts.push([o[0], o[2]]); else o.forEach(walk); } else if (o && typeof o === "object") Object.keys(o).forEach((k) => { if (k !== "rect") walk(o[k]); }); })(row);
  const r = row.rect;
  let best = [(r[0] + r[2]) / 2, (r[1] + r[3]) / 2], bestD = -1;
  for (let x = r[0] + 3; x <= r[2] - 3; x += 1) for (let z = r[1] + 3; z <= r[3] - 3; z += 1) {
    let d = Infinity;
    for (const p of pts) d = Math.min(d, Math.hypot(p[0] - x, p[1] - z));
    for (const b of boxes || []) { const dx = Math.max(b.min[0] - x, 0, x - b.max[0]), dz = Math.max(b.min[2] - z, 0, z - b.max[2]); d = Math.min(d, Math.hypot(dx, dz)); }
    if (d > bestD) { bestD = d; best = [x, z]; }
  }
  return best;
}

// ---------------------------------------------------------------- --check
function check() {
  const plan0 = JSON.parse(read(PLAN_IN));
  const baseRaw = read(BASE_FILE);
  const base = JSON.parse(baseRaw);
  let bad = 0;
  const say = (ok, m) => { console.log((ok ? "PASS " : "FAIL ") + m); if (!ok) bad++; };
  console.log("gen-slots --check --level " + LEVEL + ": base " + T.base + " (" + BASE_FILE + ", plan " + PLAN_IN + "), new id " + ID + ", slots " + SLOTS.join(" ") + ", kept " + plan0.rooms.filter((r) => !SLOTS.includes(r.id)).map((r) => r.id + ":" + r.kind).join(" "));
  // 0. the table against the plan: the slots are every fight room (kind room) of the base plan; every piece list the plan has is named
  const fight = plan0.rooms.filter((r) => r.kind === "room").map((r) => r.id);
  say(T.finish === "foundry-a9" ? SLOTS.every((s) => plan0.rooms.some((r) => r.id === s)) && fight.every((s) => SLOTS.includes(s)) : same(fight.slice().sort(), SLOTS.slice().sort()), "the slots " + SLOTS.join(" ") + " are " + (T.finish === "foundry-a9" ? "every fight room (+ level 1's link b2)" : "every fight room (kind room)") + " of " + PLAN_IN + " (" + fight.join(" ") + ")");
  const unnamed = Object.keys(plan0).filter((k) => Array.isArray(plan0[k]) && !["rooms", "doors", "owns38"].includes(k) && !LISTS.includes(k));
  say(unnamed.length === 0, "every piece list of the plan is carried (" + LISTS.filter((k) => k in plan0).map((k) => k + "[" + plan0[k].length + "]").join(" ") + ")" + (unnamed.length ? ": NOT NAMED " + unnamed.join(", ") : ""));
  const orphan = [];
  for (const k of LISTS) for (const o of plan0[k] || []) if (!plan0.rooms.some((r) => r.id === owner(k, o))) orphan.push(k + " keyed '" + owner(k, o) + "'");
  say(orphan.length === 0, "every piece is keyed to a room of the plan" + (orphan.length ? ": " + orphan.slice(0, 5).join("; ") : ""));
  // 1. the exports are the base's rooms exactly (re-exported now); the builders' files: the base's or rebuilt
  for (const id of SLOTS) {
    const e = exportRoom(plan0, id);
    const p = OLD + "/" + id + ".json";
    say(fs.existsSync(rel(p)) && same(stripWhy(roomFile(p)), stripWhy(e)), p + " is " + T.base + "'s " + id + " exactly (re-exported from " + PLAN_IN + "; " + LISTS.map((k) => k + " " + e[k].length).join(", ") + ")");
    const q = ROOMS + "/" + id + ".json";
    console.log("     " + q + ": " + (!fs.existsSync(rel(q)) ? "missing" : same(stripWhy(roomFile(q)), stripWhy(e)) ? T.base + "'s room (not rebuilt yet)" : "REBUILT (differs from " + T.base + "'s room)"));
  }
  for (const k of ROOM_KEYED.filter((k) => k !== "lights" && k !== "props")) {
    const per = SLOTS.map((id) => id + " " + (plan0[k] || []).filter((o) => o.room === id).length).join(", ");
    console.log("     the " + k + " list carried with its rooms: " + per + " (plan total " + (plan0[k] || []).length + ")");
  }
  const tmp = fs.mkdtempSync(path.join(os.tmpdir(), "gen-slots-check-"));
  const runs = [];
  try {
    const CHECK_ID = ID + "-check";
    const oldFiles = loadFiles(slotSet("old"));
    const A = assemble(plan0, oldFiles, { id: CHECK_ID });
    const strip = (p) => Object.assign({}, p, { id: "", _why: "" });
    say(same(strip(A.plan), strip(plan0)), "every slot old: the assembled plan equals " + PLAN_IN + " apart from id / _why");
    // the openings: every slot old -> the level's key is the base's (the seed); n59-l-room.py's merge then gives every room the base's opening
    const seed = T.openings.seed ? (JSON.parse(read(T.openings.seed))[T.openings.key] || {}) : {};
    const opOld = openingsOf(oldFiles, null)[T.openings.key];
    say(same(opOld, seed), "every slot old: " + T.openings.file + " " + T.openings.key + " = " + (T.openings.seed || "nothing (no seed)") + "'s " + JSON.stringify(seed));
    const lv = (plan, tag) => {
      const pp = path.join(tmp, tag + ".plan.json"), lp = path.join(tmp, tag + ".json"), rp = path.join(tmp, tag + ".md");
      fs.writeFileSync(pp, JSON.stringify(plan, null, 1) + "\n");
      const g = levelgen(pp, lp, rp);
      runs.push(g);
      if (g.errors === null || !fs.existsSync(lp)) throw new Error("levelgen failed (" + tag + ", " + g.secs + " s" + (g.timedOut ? ", TIMEOUT" : "") + "): " + g.all.slice(-800));
      const l = JSON.parse(fs.readFileSync(lp, "utf8"));
      l.name = ID; // levelgen named it after the scratch plan id
      finishLevel(l, base, "slots " + tag);
      return { l, g };
    };
    const O = lv(A.plan, "old");
    const diffs = levelDiff(base, O.l);
    say(diffs.length === 0, "every slot old: levelgen --walk (" + O.g.errors + " ERROR, " + O.g.secs + " s) + the finish (" + T.finish + "): the level equals " + BASE_FILE + " apart from its names (" + NAMES.join(", ") + ")" + (diffs.length ? ": " + diffs.join("; ").slice(0, 900) : ""));
    const named = Object.assign({}, O.l, { name: base.name, _why: base._why, generatedBy: base.generatedBy });
    say(serLike(baseRaw)(named) === baseRaw, "every slot old: with " + T.base + "'s name / _why / generatedBy the bytes ARE " + BASE_FILE + " (" + baseRaw.length + " chars)");
    const oldNames = O.g.rows.map((r) => r.name);
    say(same(oldNames.slice().sort(), T.baseErrors.slice().sort()), "every slot old: the levelgen ERROR rows are " + T.base + "'s own, as " + TABLE_FILE + " lists them (" + (oldNames.length ? oldNames.join("; ") : "none") + ")");
    // planted: the first slot with boxes, one box moved 1 m east -> the difference printed
    const P = SLOTS.find((id) => oldFiles[id].data.boxes.length > 0);
    const Q = SLOTS.find((id) => id !== P);
    const P1 = loadFiles(slotSet("old"));
    const b0 = P1[P].data.boxes[0];
    P1[P].data.boxes[0] = Object.assign({}, b0, { min: [b0.min[0] + 1, b0.min[1], b0.min[2]], max: [b0.max[0] + 1, b0.max[1], b0.max[2]] });
    const B = assemble(plan0, P1, { id: CHECK_ID });
    const PB = lv(B.plan, "planted");
    const pd = levelDiff(base, PB.l);
    say(pd.length > 0, "PLANTED one " + P + " box moved 1 m east (levelgen --walk " + PB.g.errors + " ERROR, " + PB.g.secs + " s): the difference prints (" + pd.length + " item(s)):");
    for (const d of pd) console.log("       " + d.slice(0, 400));
    // planted refusals (no levelgen): each by its name
    const refuse = (name, mut, want) => {
      const F = loadFiles(slotSet("old"));
      mut(F);
      try { assemble(plan0, F, { id: CHECK_ID }); say(false, "PLANTED " + name + ": NOT refused"); } catch (e) { say(!want || e.message.includes(want), "PLANTED " + name + ": refused (" + e.message.slice(0, 240) + ")" + (want && !e.message.includes(want) ? " but not by '" + want + "'" : "")); }
    };
    const dropKey = Object.keys(oldFiles[Q].data.row).find((k) => !LOCKED.includes(k) && !isDoc(k));
    refuse("an unknown row key " + P + ".row.spinners67", (F) => { F[P].data.row.spinners67 = [1]; }, "'spinners67'");
    refuse("an area67 block in " + Q + "'s row (research.md's proposal)", (F) => { F[Q].data.row.area67 = { high: [0, 4, 0] }; }, "'area67'");
    refuse(Q + " drops its " + dropKey + " with no reason", (F) => { delete F[Q].data.row[dropKey]; }, "'" + dropKey + "'");
    refuse("an unknown file key " + P + ".extras", (F) => { F[P].data.extras = []; }, "'extras'");
    refuse(P + "'s box keyed to zone " + Q, (F) => { F[P].data.boxes[0] = Object.assign({}, F[P].data.boxes[0], { zone: Q }); }, "keyed to '" + Q + "'");
    refuse(Q + "'s rect widened 1 m", (F) => { const r = F[Q].data.row.rect; F[Q].data.row.rect = [r[0] - 1, r[1], r[2], r[3]]; }, "row.rect");
    refuse("a new " + P + " box with no job / why", (F) => { const r = F[P].data.row.rect; F[P].data.boxes.push({ zone: P, kind: "hull", min: [(r[0] + r[2]) / 2, 0, (r[1] + r[3]) / 2], max: [(r[0] + r[2]) / 2 + 1, 2, (r[1] + r[3]) / 2 + 1] }); }, "without job / why");
    refuse(P + "'s opening 'sideways'", (F) => { F[P].data.opening = "sideways"; }, "'sideways'");
    // accepted: verb65 + line65 on a row (the brief's step 4: a rebuilt room carries both)
    {
      const F = loadFiles(slotSet("old"));
      const r = F[P].data.row.rect;
      F[P].data.row.verb65 = "ride"; F[P].data.row.line65 = [[r[0] + 2, 0, r[1] + 2], [(r[0] + r[2]) / 2, 3, (r[1] + r[3]) / 2], [r[2] - 2, 0, r[3] - 2]];
      F[P].use = "latest";
      let ok = true, msg = "";
      try { const X = assemble(plan0, F, { id: CHECK_ID }); const row = X.plan.rooms.find((q) => q.id === P); ok = row.verb65 === "ride" && row.line65.length === 3; msg = "the plan's " + P + " row carries verb65 " + row.verb65 + ", line65 " + row.line65.length + " waypoints"; } catch (e) { ok = false; msg = "refused: " + e.message.slice(0, 200); }
      say(ok, "PLANTED " + P + ".row verb65 ride + line65: accepted (" + msg + ")");
    }
    // the opening: from an old slot nothing beyond the seed; from a latest / new slot it overrides the seed for that room
    const P3 = loadFiles(slotSet("old"));
    P3[P].data.opening = "ambusher";
    P3[P].data.opening_why = "the --check's planted card line";
    assemble(plan0, P3, { id: CHECK_ID });
    const o3old = openingsOf(P3, null)[T.openings.key];
    P3[P].use = "latest";
    const o3new = openingsOf(P3, null)[T.openings.key];
    say(same(o3old, seed) && same(o3new, Object.assign({}, seed, { [P]: "ambusher" })), "PLANTED " + P + ".opening ambusher: from an old slot " + T.openings.key + " stays the seed, from a latest / new slot " + JSON.stringify(o3new));
    // other levels' keys in the shared openings file are kept
    const o3keep = openingsOf(P3, { _why: "x", other: { z1: "held" }, _cards: { z1: "a card" } });
    say(same(o3keep.other, { z1: "held" }) && o3keep._cards.z1 === "a card" && o3keep._cards[P] === "the --check's planted card line", "PLANTED a shared " + T.openings.file + " with another level's key: kept (" + JSON.stringify(o3keep.other) + "), " + P + "'s card added");
    // planted: a room rebuilt (one pillar added with a job, bot43 false) replaces only its slot; the pillar stands on the free spot farthest
    // from every point the row names (spawn, nodes, waves' nodes, lessons) and every box (a pillar on an encounter node is levelgen's ERROR)
    const P2 = loadFiles(slotSet("old"));
    const [cx, cz] = freeSpot(P2[P].data.row, P2[P].data.boxes);
    P2[P].data.boxes.push({ zone: P, kind: "hull", min: [cx - 0.5, 0, cz - 0.5], max: [cx + 0.5, 10, cz + 0.5], job: "cover: planted", why: "the --check's planted pillar" });
    P2[P].data.row.bot43 = false;
    const C = assemble(plan0, P2, { id: CHECK_ID });
    const outside = LISTS.every((k) => same((plan0[k] || []).filter((o) => owner(k, o) !== P), (C.plan[k] || []).filter((o) => owner(k, o) !== P)));
    const rowsElse = plan0.rooms.every((r) => r.id === P || same(r, C.plan.rooms.find((q) => q.id === r.id)));
    say(outside && rowsElse && C.plan.boxes.filter((b) => b.zone === P).length === plan0.boxes.filter((b) => b.zone === P).length + 1, "PLANTED " + P + " rebuilt (one pillar added, bot43 false): it appears in the assembled plan (" + P + " boxes " + plan0.boxes.filter((b) => b.zone === P).length + " -> " + C.plan.boxes.filter((b) => b.zone === P).length + ") and only " + P + "'s slot changed");
    printReport(C.report.filter((r) => r.id === P), false);
    const PC = lv(C.plan, "planted-" + P);
    const ca = base.campaign, cb = PC.l.campaign;
    const foreign = [];
    for (const k of Object.keys(ca)) if (Array.isArray(ca[k]) && k !== "rooms") {
      const sa = new Set(ca[k].map((o) => JSON.stringify(o))), sb = new Set((cb[k] || []).map((o) => JSON.stringify(o)));
      for (const o of ca[k].filter((o) => !sb.has(JSON.stringify(o))).concat((cb[k] || []).filter((o) => !sa.has(JSON.stringify(o))))) if ((o.zone || o.room) !== P) foreign.push(k + " " + JSON.stringify(o).slice(0, 100));
    }
    for (const r of ca.rooms) if (r.id !== P && !same(r, cb.rooms.find((q) => q.id === r.id))) foreign.push("room " + r.id);
    const cd = levelDiff(base, PC.l);
    const beyondC = beyondBase(PC.g.rows);
    say(beyondC.length === 0 && foreign.length === 0 && cd.length > 0, "PLANTED " + P + " rebuilt, levelgen --walk " + PC.g.errors + " ERROR (" + (beyondC.length ? "BEYOND the base's: " + beyondC.map((r) => r.name).join("; ") : "none beyond the base's") + ") + the finish: every campaign row that differs from " + BASE_FILE + " is " + P + "'s (" + (foreign.length ? "FOREIGN: " + foreign.join("; ") : "none foreign") + "); the difference: " + cd.join("; ").slice(0, 600));
    // planted: P's spawn lifted 40 m (off the nav grid) -> a levelgen ERROR row beyond the base's -> refused (the base's own rows pass)
    const P4 = loadFiles(slotSet("old"));
    const D = assemble(plan0, P4, { id: CHECK_ID });
    const sp = D.plan.rooms.find((r) => r.id === P);
    sp.spawn = Object.assign({}, sp.spawn, { at: [sp.spawn.at[0], sp.spawn.at[1] + 40, sp.spawn.at[2]] });
    const PD = (() => { const pp = path.join(tmp, "lift.plan.json"), lp = path.join(tmp, "lift.json"), rp = path.join(tmp, "lift.md"); fs.writeFileSync(pp, JSON.stringify(D.plan, null, 1) + "\n"); const g = levelgen(pp, lp, rp); runs.push(g); return g; })();
    const beyondD = beyondBase(PD.rows);
    say(beyondD.length > 0 && PD.rows.filter((r) => T.baseErrors.includes(r.name)).length === T.baseErrors.length, "PLANTED " + P + "'s spawn lifted 40 m: levelgen " + PD.errors + " ERROR, " + beyondD.length + " beyond " + T.base + "'s own (" + beyondD.map((r) => r.name).join("; ").slice(0, 200) + ": --run / --finish refuse it); the base's own " + T.baseErrors.length + " row(s) still listed and allowed");
  } finally {
    fs.rmSync(tmp, { recursive: true, force: true });
  }
  if (fs.existsSync(rel(TRACKED_OUTS[0])) && fs.existsSync(rel(TRACKED_OUTS[1]))) {
    const a = read(TRACKED_OUTS[0]), b = read(TRACKED_OUTS[1]);
    const d = levelDiff(base, JSON.parse(a));
    console.log("     " + TRACKED_OUTS[0] + " now: copies " + (a === b ? "identical" : "DIFFER") + "; against " + BASE_FILE + " apart from names: " + (d.length ? d.length + " item(s): " + d.join("; ").slice(0, 600) : "equal") + " (" + (JSON.parse(a).generatedBy || "").split(";")[0].slice(0, 120) + ")");
  }
  console.log("gen-slots --check --level " + LEVEL + ": " + (bad ? bad + " FAIL" : "all PASS") + " (levelgen runs: " + runs.length + ", " + runs.map((r) => r.secs + " s").join(" / ") + ")");
  process.exit(bad ? 1 : 0);
}

// ---------------------------------------------------------------- --cross-n65: level 1 through gen-n65-foundry.cjs and through this tool
function crossN65() {
  if (LEVEL !== "foundry") throw new Error("--cross-n65 is level 1's (--level foundry)");
  const spec = opt("--slots") || "old";
  const tmp = fs.mkdtempSync(path.join(os.tmpdir(), "gen-slots-cross-"));
  const a = path.join(tmp, "n65").replace(/\\/g, "/"), b = path.join(tmp, "slots").replace(/\\/g, "/");
  let bad = 0;
  const say = (ok, m) => { console.log((ok ? "PASS " : "FAIL ") + m); if (!ok) bad++; };
  try {
    const run = flag("--run") ? ["--run"] : [];
    const r1 = cp.spawnSync("node", ["Tools/night/gen-n65-foundry.cjs", "--slots=" + spec, "--out=" + a].concat(run), { cwd: ROOT, encoding: "utf8", maxBuffer: 64 << 20, timeout: LEVELGEN_TIMEOUT_MS });
    const r2 = cp.spawnSync("node", ["Tools/night/gen-slots.cjs", "--level", "foundry", "--slots=" + spec, "--out=" + b].concat(run), { cwd: ROOT, encoding: "utf8", maxBuffer: 64 << 20, timeout: LEVELGEN_TIMEOUT_MS });
    console.log("gen-n65-foundry.cjs --slots=" + spec + " --out=<a>" + (run.length ? " --run" : "") + ": exit " + r1.status + "; " + ((r1.stdout || "") + (r1.stderr || "")).split(/\r?\n/).filter((l) => /^gen-n65|^openings|levelgen --walk|REFUSED/.test(l)).join(" | ").slice(0, 400));
    console.log("gen-slots.cjs --level foundry --slots=" + spec + " --out=<b>" + (run.length ? " --run" : "") + ": exit " + r2.status + "; " + ((r2.stdout || "") + (r2.stderr || "")).split(/\r?\n/).filter((l) => /^gen-slots|^openings|levelgen --walk|REFUSED/.test(l)).join(" | ").slice(0, 400));
    const pa = fs.readFileSync(a + "/foundry.plan.json", "utf8"), pb = fs.readFileSync(b + "/foundry.plan.json", "utf8");
    say(r1.status === 0 && r2.status === 0 && pa === pb, "level 1 --slots=" + spec + ": the plans are byte for byte the same (" + pa.length + " / " + pb.length + " chars; sha256 " + sha256(a + "/foundry.plan.json").slice(0, 16) + " / " + sha256(b + "/foundry.plan.json").slice(0, 16) + ")");
    const oa = JSON.parse(fs.readFileSync(a + "/openings65.json", "utf8")), ob = JSON.parse(fs.readFileSync(b + "/openings65.json", "utf8"));
    say(same(oa, ob), "level 1 --slots=" + spec + ": openings65.json the same (" + JSON.stringify(oa.foundry) + ")");
    if (run.length) {
      const la = JSON.parse(fs.readFileSync(a + "/n65-foundry.json", "utf8")), lb = JSON.parse(fs.readFileSync(b + "/n65-foundry.json", "utf8"));
      const d = levelDiff(la, lb);
      say(d.length === 0 && la.name === lb.name, "level 1 --slots=" + spec + " --run: the levels n65-foundry.json equal apart from _why / generatedBy (" + (d.length ? d.join("; ").slice(0, 400) : "0 differences") + ")");
    }
  } finally { fs.rmSync(tmp, { recursive: true, force: true }); }
  console.log("gen-slots --cross-n65 --slots=" + spec + ": " + (bad ? bad + " FAIL" : "all PASS"));
  process.exit(bad ? 1 : 0);
}

// ---------------------------------------------------------------- --info: the resolved table, for the round tools' shell scripts
function info() {
  const plan0 = JSON.parse(read(PLAN_IN));
  const o = { level: LEVEL, id: ID, base: T.base, baseFile: BASE_FILE, planIn: PLAN_IN, px: PX, dir: DIR, roomsDir: ROOMS, roomsBaseDir: OLD, slotsFile: SLOTS_FILE, slots: SLOTS, kept: plan0.rooms.filter((r) => !SLOTS.includes(r.id)).map((r) => r.id), planOut: PLAN_OUT, genOut: GEN_OUT, assembly: USED, openings: OPENINGS_OUT, report: REPORT, levelOut: OUTS[0], outs: OUTS, row: T.row, label: T.label, finish: T.finish, baseErrors: T.baseErrors, planName: LEVEL + ".plan.json", levelName: ID + ".json", assemblyName: LEVEL + ".assembly.json", openingsName: T.openings.file, reportName: "LEVEL-" + ID + "-validate.md" };
  const f = opt("--info");
  if (!f) { console.log(JSON.stringify(o, null, 1)); return; }
  if (!(f in o)) throw new Error("--info=" + f + ": no such field (" + Object.keys(o).join(", ") + ")");
  console.log(Array.isArray(o[f]) ? o[f].join(" ") : String(o[f]));
}

// ---------------------------------------------------------------- main (a refusal prints one line and exits 1)
try { main(); } catch (e) { console.error("gen-slots: REFUSED: " + e.message); process.exit(1); }
function main() {
  refuseUnknownOptions();
  setLevel(LEVEL, opt("--out"));
  if (flag("--info") || opt("--info")) return info();
  if (flag("--check")) return check();
  if (flag("--cross-n65")) return crossN65();
  if (flag("--finish")) return finish();
  if (flag("--export")) {
    const plan0 = JSON.parse(read(PLAN_IN));
    const ids = args.filter((a) => !a.startsWith("--"));
    fs.mkdirSync(rel(ROOMS), { recursive: true });
    fs.mkdirSync(rel(OLD), { recursive: true });
    for (const id of ids.length ? ids : SLOTS) {
      if (!SLOTS.includes(id)) throw new Error(id + " is not a slot of " + LEVEL + " (" + SLOTS.join(", ") + ")");
      const t = roomText(exportRoom(plan0, id));
      fs.writeFileSync(rel(OLD + "/" + id + ".json"), t);
      const q = ROOMS + "/" + id + ".json";
      const wrote = !fs.existsSync(rel(q)) || flag("--force");
      if (wrote) fs.writeFileSync(rel(q), t);
      if (!same(JSON.parse(t), exportRoom(plan0, id))) throw new Error("export round trip failed: " + id);
      console.log("export " + LEVEL + " " + id + ": " + OLD + "/" + id + ".json" + (wrote ? " and " + q : " (" + q + " kept: a builder's file)") + " (" + t.length + " chars)");
    }
    return;
  }
  const spec = opt("--slots") || (fs.existsSync(rel(SLOTS_FILE)) ? SLOTS_FILE : "old");
  const plan0 = JSON.parse(read(PLAN_IN));
  const set = slotSet(spec);
  const rfo = opt("--room-file");
  if (rfo) {
    const c = rfo.indexOf(":", 2); // a Windows path's drive colon is not the separator
    const rid = rfo.slice(0, c), rpath = rfo.slice(c + 1);
    if (!SLOTS.includes(rid)) throw new Error("--room-file: " + rid + " is not a slot of " + LEVEL);
    if (!OUT_DIR) throw new Error("--room-file needs --out=<a scratch dir> (a planted room never reaches the tracked level files)");
    set[rid] = { use: "latest", file: rpath, passSha: null };
    console.log("gen-slots: --room-file: slot " + rid + " reads " + rpath);
  }
  const files = loadFiles(set);
  const { plan, report } = assemble(plan0, files, { id: ID });
  for (const r of report) r.passSha = set[r.id].passSha;
  if (flag("--plant-error")) {
    if (!flag("--run") || !OUT_DIR) throw new Error("--plant-error needs --run and --out=<a scratch dir> (the planted plan and level are written only there)");
    if (/^(Design|Assets|Reports)\//.test(path.relative(ROOT, rel(OUT_DIR)).replace(/\\/g, "/") + "/")) throw new Error("--plant-error: --out=" + OUT_DIR + " is inside a tracked level folder");
    const s0 = plan.rooms.find((r) => r.id === SLOTS[0]);
    s0.spawn = Object.assign({}, s0.spawn, { at: [s0.spawn.at[0], s0.spawn.at[1] + 40, s0.spawn.at[2]] });
    console.log("gen-slots: --plant-error: PLANTED " + SLOTS[0] + " spawn at " + JSON.stringify(s0.spawn.at) + " (40 m over its floor: off the nav grid)");
  }
  const dry = flag("--dry-plan");
  if (!dry) writePlanFile(plan, report, spec, files);
  const ops = openingsOf(files, null)[T.openings.key];
  console.log("openings: " + (dry ? "(not written) " : OPENINGS_OUT + " ") + T.openings.key + " " + (Object.keys(ops).length ? Object.keys(ops).map((k) => k + " " + ops[k]).join(", ") : "none") + " (seed " + (T.openings.seed || "none") + ")");
  console.log("gen-slots --level " + LEVEL + ": " + PLAN_IN + " -> " + (dry ? "(--dry-plan: not written)" : PLAN_OUT) + " (slots " + spec + "; rebuilt: " + (report.filter((r) => r.edited).map((r) => r.id).join(", ") || "none") + "); every other room, door, piece and key " + T.base + "'s bit for bit");
  printReport(report, flag("--table"));
  if (flag("--run")) {
    const before = shipShas();
    console.log("shipped before: " + before);
    const g = levelgen(PLAN_OUT, GEN_OUT, REPORT);
    for (const l of g.lines) console.log("  " + l);
    if (g.errors === null || !fs.existsSync(rel(GEN_OUT))) { console.log("levelgen failed (exit " + g.code + (g.timedOut ? ", TIMEOUT" : "") + "): " + g.all.slice(-800)); console.log("shipped after:  " + shipShas() + " (NOT written)"); process.exit(1); }
    const beyond = beyondBase(g.rows);
    if (beyond.length) {
      const after = shipShas();
      console.log("levelgen --walk: " + g.errors + " ERROR, " + beyond.length + " beyond " + T.base + "'s own (RED: " + beyond.map((r) => r.text.slice(0, 200)).join(" || ") + "): REFUSED, the shipped files NOT written");
      console.log("shipped after:  " + after + (after === before ? " (byte-identical)" : " (CHANGED: a bug)"));
      process.exit(1);
    }
    finish();
    console.log("levelgen --walk: " + g.errors + " ERROR (" + (g.rows.length ? g.rows.map((r) => r.name).join("; ") + ": " + T.base + "'s own" : "none") + "; " + g.secs + " s)");
    const after = shipShas();
    console.log("shipped after:  " + after + (OUT_DIR ? (after === before ? " (byte-identical: --out writes only " + OUT_DIR + ")" : " (CHANGED under --out: a bug)") : ""));
    if (OUT_DIR) console.log("the round's level: " + OUTS[0] + " " + sha256(OUTS[0]) + "; plan " + PLAN_OUT + ", report " + REPORT + ", openings " + OPENINGS_OUT);
  }
}

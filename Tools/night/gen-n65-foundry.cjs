// NIGHT65 builder T (T1; NIGHT65.md section 3; FORGE65 DECISIONS D-6, redteam.md 2-5): LEVEL 1 as n65-foundry, assembled from ROOM FILES in SLOTS.
// Level 1's geometry is Design/levels/n52/foundry.plan.json (read only: n52-foundry, its plan and its generator are never written here, so the
// row foundry65 off loads n52-foundry bit for bit). Six fight-room SLOTS (a2 KESTREL, a3 RIDE, a4 PARRY, a5 CRUCIBLE, b1 THE YARD, a8 PIERCE)
// each hold a ROOM FILE: the whole room row (every key), its zone-keyed boxes / ramps / pads / grapple / pits / tunnels, its room-keyed lights /
// props and its teach35 entries (redteam.md 3). Every other room (a1, a6, a7, a9), every door and every level key is n52's bit for bit.
// NIGHT66 T2: a seventh slot, b2 THE GANTRY (a link, n52's room as it is until GG's own message names it; its export rooms-n52/b2.json).
//   Design/levels/n65/rooms-n52/<id>.json   the n52 room exported exactly (never edited: --check re-exports and compares)
//   Design/levels/n65/rooms/<id>.json       the builder's room file (R / S edit only these); it starts as the export
//   Design/levels/n65/slots65.json          the lead's: per slot "old" or "new" + the PASS's sha (written only by n65-slot-set.cjs)
//   Design/levels/n65/openings65.json       written by the plan step from the room files: a room file's optional top-level
//       "opening": "vista" | "ambushed" | "ambusher" | "held" (and "opening_why": its card line) in a slot that is not old; n59-l-room.py
//       n65-foundry overlays it on n52's openings53.json, so a room with no opening (or an old slot) keeps n52's
// Run (from the tree's root):
//   node Tools/night/gen-n65-foundry.cjs [--slots=latest | --slots=old | --slots=a2,a5 | --slots=<slots65.json>] [--table]   the PLAN
//       latest: every slot's rooms/<id>.json (the round builds); old: every slot rooms-n52/<id>.json (= n52's rooms); a2,a5: those latest,
//       the rest old; a file: slots65.json (the shipped level; a "new" slot's file must be the one its PASS bound, else refused).
//       Default: Design/levels/n65/slots65.json when it exists, else old. Prints the per-key keep / replace / add / drop table of every
//       slot whose file differs from n52's room (--table: of every slot) and refuses an unlisted key (KNOWN below) or a drop with no reason.
//   dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n65/foundry.plan.json Design/levels/n65/foundry.json --walk
//   node Tools/night/gen-n65-foundry.cjs --finish   the LEVEL: a9's n47 / n48 keys re-applied exactly as gen-n52-foundry.cjs does (lines
//       429-473: wick47, frames47, climbTop48; data.name "n46-foundry"), asserted bit-equal to n52-foundry's a9; both copies written
//   node Tools/night/gen-n65-foundry.cjs --run [--slots=...]   the three steps above in one (the builders' sim loop); NIGHT66 T1a: a levelgen
//       ERROR (or a failed levelgen) exits 1 BEFORE the finish, so the shipped files are never written from a level with errors (their sha256
//       printed before and after); --plant-error (the proof only) lifts a2's spawn off the nav grid in the assembled plan
//   --out=<dir> (NIGHT66 T1c; red team 11): with --run (or the plan step alone), every file goes to <dir>: foundry.plan.json, foundry.json,
//       foundry.assembly.json, openings65.json, LEVEL-n65-foundry-validate.md (levelgen --report) and the level n65-foundry.json (one copy);
//       no tracked file is written (the tracked copies' sha256 printed before and after). Give a path relative to the tree (Logs/n66/<tag>)
//       or a Windows absolute one (C:/...): a Git Bash /c/... path is not one node resolves. --plant-error needs --run and --out, outside
//       Design / Assets / Reports. --finish refuses a level whose report (Reports/LEVEL-n65-foundry-validate.md, or <dir>'s) is missing,
//       lists an ERROR row, or is older than the level (T1a's critic, Logs/n66/verdict-T1a.md finding 1)
//   node Tools/night/gen-n65-foundry.cjs --check    with every slot old (through the room-file path): the assembled plan equals n52's plan and
//       the assembled level equals n52-foundry.json byte for byte apart from its names; then the planted cases (a one-box change prints the
//       difference, an unknown key and a drop with no reason are refused, a changed room replaces only its slot). Writes nothing of the
//       lead's: its two levelgen runs use the plan id n65-check in a scratch folder (deleted, with their report, after).
//   node Tools/night/gen-n65-foundry.cjs --export [ids] [--force]   writes rooms-n52/<id>.json and, when missing (or --force), rooms/<id>.json
// Reuses gen-n57-kiln.cjs's slot recipe (its :12 --slots=, :60 the NEW list, :80-93 swap / place) and gen-n52-foundry.cjs's guards and finish.
const fs = require("fs");
const path = require("path");
const os = require("os");
const cp = require("child_process");
const ROOT = path.join(__dirname, "../..");
const PLAN_IN = "Design/levels/n52/foundry.plan.json";
const N52 = "Design/levels/n52-foundry.json";
const DIR = "Design/levels/n65";
const ROOMS = DIR + "/rooms", OLD = DIR + "/rooms-n52";
const SLOTS_FILE = DIR + "/slots65.json";
let PLAN_OUT = DIR + "/foundry.plan.json", GEN_OUT = DIR + "/foundry.json", USED = DIR + "/foundry.assembly.json";
let OUTS = ["Design/levels/n65-foundry.json", "Assets/StreamingAssets/Design/levels/n65-foundry.json"];
// NIGHT66 T1c: the levelgen report (levelgen's own default path; --out moves it) and --out=<dir> (red team 11, code-scout.md section 1):
// every file a run writes goes to <dir> (the plan foundry.plan.json, levelgen's foundry.json and LEVEL-n65-foundry-validate.md, the
// assembly foundry.assembly.json, openings65.json and the level n65-foundry.json, one copy), never a tracked file
let REPORT = "Reports/LEVEL-n65-foundry-validate.md";
const OUT_DIR = (() => { const a = process.argv.slice(2).find((x) => x.startsWith("--out=")); return a ? a.slice(6).replace(/\\/g, "/").replace(/\/$/, "") : null; })();
const ID = "n65-foundry";
// NIGHT66 T2 (slate T2; FORGE66 redteam.md finding 7): b2 THE GANTRY, kind link, is the seventh slot (use old: n52's room exactly; it is
// rebuilt only on GG's own word). The plan's room order: a2 a3 a4 a5 b1 b2 ... a8
const SLOTS = ["a2", "a3", "a4", "a5", "b1", "b2", "a8"];
const KEPT = ["a1", "a6", "a7", "a9"]; // NIGHT65.md "Kept bit for bit" (b2 left it for its slot, NIGHT66 T2)
const ZONE_KEYED = ["boxes", "ramps", "pads", "grapple", "pits", "tunnels"];
const ROOM_KEYED = ["lights", "props"];
const LISTS = ZONE_KEYED.concat(ROOM_KEYED, ["teach35"]);
// opening / opening_why (optional): the room's opening for the checker (vista / ambushed / ambusher / held) and its card line; an old slot
// writes nothing, so it keeps n52's (Design/levels/n52/openings53.json)
const FILE_KEYS = ["_why", "room", "row"].concat(LISTS, ["drop", "opening", "opening_why"]);
let OPENINGS_OUT = DIR + "/openings65.json";
if (OUT_DIR) {
  PLAN_OUT = OUT_DIR + "/foundry.plan.json"; GEN_OUT = OUT_DIR + "/foundry.json"; USED = OUT_DIR + "/foundry.assembly.json";
  OPENINGS_OUT = OUT_DIR + "/openings65.json"; OUTS = [OUT_DIR + "/n65-foundry.json"]; REPORT = OUT_DIR + "/LEVEL-n65-foundry-validate.md";
}
// Every room-row key a level has carried (the union over Design/levels' plans and level files, NIGHT24-NIGHT64) plus NIGHT65's own. A key
// not listed is refused: a new key needs its reader named here first (redteam.md 3: an unread key is a lost lesson or a stale proof).
// Doc keys (ending _why or Why) are always allowed: nothing reads them.
const KNOWN = ["_note", "afterLesson43", "arena36", "arenas33", "award", "blastGap", "boss", "botMedian", "brawl40", "bridge36", "bridges45", "card57",
  "climbTop48", "copied38", "crown32", "crumble31", "d3", "dashGap", "deckBreak31", "devices33", "exit", "fall37", "fallReturn43", "flight38", "frames36",
  "frames47", "frames52", "gapLz31", "grapple", "grappleNodes", "hallRoute", "hazard", "height", "highNodes", "hoop37", "id", "identity45", "identity46",
  "job36", "kind", "kneeTarget37", "lantern36", "lessons", "lessons31", "limit", "limitReason", "lines34", "loom37", "loom40NoRide", "loom47Posts",
  "loomLegs", "loomRoom37", "loomSwing37", "meter38", "name", "nodes", "noride38", "owns38", "pacer37", "par", "parCap", "pickup", "pickup31", "proofs",
  "props45", "props46", "pull38", "reachFrom", "rect", "respawnY", "ride35", "rims36", "ringRoute32", "roles43", "roof", "routeVia", "run38", "shortcuts",
  "sink31", "spawn", "spire37", "stag37", "standIns", "standIns31", "standIns36", "tops43", "verb36", "wallRoute", "waveNodes43", "waves", "waves31",
  "wick37", "wick47",
  // NIGHT65: frames65 (tour65's card poses: name -> [x, y, z, yawDeg, pitchRad]), line65 (the card's verb line: [[x, y, z], ...] waypoints, tour65),
  // bot43 (false: Walk36 / Walk38Foundry / Walk43Foundry skip their id scripts in this room, the generic autopilot runs), card65 (the card's path),
  // verb65 (the room's verb for the verb-aware pilot: launch | launch+slide | pierce | ride | parry | slide; read by Tools/SimHarness/
  // Harness.N65.VerbPilot.cs with n46flow --verbs65 and n65verb skip --verbs65; nothing in the game reads it)
  "frames65", "line65", "bot43", "card65", "verb65"];
// The keys a slot cannot change: the doors d1-d10 fix every rect (redteam.md 7; the card fits the rect), the id is the slot, the kind a fight room
const LOCKED = ["id", "kind", "rect"];
// NIGHT72 N0 (D-4; GG 2026-10-26 "the mini roof ... remove it", commit 0e3f95ea5): level 1's room a3 (1.3 RIDE) has NO roof slab (n52's plan: a3 roof
// false). levelgen's room standard (LevelValidate29) wants `roof: true` on every non-boss room, so a roofless a3 reads exactly one ERROR row: closedRoom
// for a3. That row, by name and only while the plan's a3 row says roof false, is allowed (as levels67.json baseErrors for spire's pit recovery): any
// other ERROR row still refuses --run and --finish. The three a3 ceiling lights (a3-L1, a3-L2, a3-L4, y 9.95, "kind": "ceiling") hung from the slab;
// with no slab nothing is above them (the lights row read them "not flush to a surface"), and the lead's call (GG 2026-10-11: lights "placed and
// floating") is that a roofless a3 keeps no ceiling light: assemble() takes them out of the n65 plan while a3's row says roof false (a separate
// commit, so it reverts alone; `--keep-ceiling-lights` keeps them, and --check's equal-to-n52 proofs run that way).
const ROOFLESS_ROOM = "a3", ROOFLESS_LIGHTS = ["a3-L1", "a3-L2", "a3-L4"];
// the ERROR rows of a levelgen report: [{check, items: ["a3-L1: not flush ...", ...]}] (a row's detail is "; "-joined)
function reportErrorRows(text) {
  return text.split(/\r?\n/).filter((l) => l.includes("| ERROR |")).map((l) => {
    const c = l.split("|").map((s) => s.trim());
    return { check: c[1], items: (c[3] || "").split("; ").map((s) => s.trim()).filter(Boolean), line: l };
  });
}
// the rows beyond the roofless a3's own: [] means every ERROR row is allowed (or there is none)
function unexpectedErrorRows(text, rooflessA3) {
  const bad = [];
  for (const r of reportErrorRows(text)) {
    if (rooflessA3 && r.check === "closedRoom" && r.items.length === 1 && r.items[0] === ROOFLESS_ROOM + ": no `roof: true`") continue;
    // (the lights row of a3-L1 / L2 / L4 is NOT allowed: the plan no longer carries them; --keep-ceiling-lights is --check's own proof mode)
    bad.push(r.line.slice(0, 300));
  }
  return bad;
}
const isDoc = (k) => /(_why|Why)$/.test(k);
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
// true when the plan file at `p` has room a3 with `roof: false` (the roofless 1.3 RIDE); false when the file is missing or a3 has a roof
const rooflessPlan = (p) => { try { const r = JSON.parse(fs.readFileSync(path.isAbsolute(p) ? p : path.join(ROOT, p), "utf8")).rooms.find((x) => x.id === ROOFLESS_ROOM); return !!r && r.roof === false; } catch (e) { return false; } };
const rel = (p) => (path.isAbsolute(p) ? p : path.join(ROOT, p));
const read = (p) => fs.readFileSync(rel(p), "utf8");
const eolOf = (t) => (t.includes("\r\n") ? "\r\n" : "\n");
const owner = (k, o) => (ZONE_KEYED.includes(k) ? o.zone : o.room);
const args = process.argv.slice(2);
const flag = (f) => args.includes(f);
const opt = (name) => { const a = args.find((x) => x.startsWith(name + "=")); return a ? a.slice(name.length + 1) : null; };

// ---------------------------------------------------------------- the export: one slot's room out of n52's plan, exactly
function exportRoom(plan, id) {
  const row = plan.rooms.find((r) => r.id === id);
  if (!row) throw new Error("no room " + id + " in " + PLAN_IN);
  const f = { _why: "NIGHT65 T1 (Tools/night/gen-n65-foundry.cjs --export): n52-foundry's room " + id + " (" + row.name + ") exactly as " + PLAN_IN + " has it: the row (every key), its zone-keyed pieces, its room-keyed lights / props, its teach35 entries. The slot's starting point: rebuild it here (every piece with a job and a why), give the row bot43: false (the walk bot's old id scripts drive the old geometry), add frames65 / line65 for tour65. A key the row drops needs a reason in drop{}. The room's opening for the checker: add top-level \"opening\": \"vista\" | \"ambushed\" | \"ambusher\" | \"held\" (and \"opening_why\": its card line); left out, the room keeps n52's (openings53.json).", room: id, row };
  for (const k of LISTS) f[k] = (plan[k] || []).filter((o) => (k === "teach35" ? o.room : owner(k, o)) === id);
  f.drop = {};
  return f;
}
// one line per piece / per row key, so a builder's diff is a line per change (standard JSON: any reader parses it)
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
    for (const id of SLOTS) {
      const e = (s.slots || {})[id] || { use: "old" };
      if (e.use === "old") { out[id] = old(id); continue; }
      if (e.use !== "new") throw new Error(spec + ": slot " + id + " use '" + e.use + "' (old or new only)");
      // a PASS binds to its sha (NIGHT65.md rule 8): the file shipped is the one the critic judged, or the slot is refused
      const file = ROOMS + "/" + id + ".json";
      let atPass;
      try { atPass = cp.execFileSync("git", ["show", e.passSha + ":" + file], { cwd: ROOT, encoding: "utf8", maxBuffer: 64 << 20 }); } catch (err) { throw new Error(spec + ": slot " + id + ": " + file + " is not in its passSha " + e.passSha); }
      if (!same(JSON.parse(atPass), JSON.parse(read(file)))) throw new Error(spec + ": slot " + id + ": " + file + " changed since its PASS at " + e.passSha + ": GATE it again, or `node Tools/night/n65-slot-set.cjs " + id + " old`");
      out[id] = { use: "new", file, passSha: e.passSha };
    }
  } else {
    const ids = spec.split(",").filter(Boolean);
    for (const id of ids) if (!SLOTS.includes(id)) throw new Error("--slots=" + spec + ": " + id + " is not a slot (" + SLOTS.join(", ") + ")");
    for (const id of SLOTS) out[id] = ids.includes(id) ? latest(id) : old(id);
  }
  return out;
}

// ---------------------------------------------------------------- one room file checked against n52's room: the per-key table, or a refusal
function checkRoom(id, f, base, where) {
  const fail = (m) => { throw new Error(where + ": " + m); };
  for (const k of Object.keys(f)) if (!FILE_KEYS.includes(k)) fail("unlisted file key '" + k + "' (a room file holds " + FILE_KEYS.join(", ") + ")");
  if (f.room !== id) fail("room '" + f.room + "', expected " + id);
  const row = f.row;
  if (!row || typeof row !== "object") fail("no row");
  const drop = f.drop || {};
  const table = [];
  for (const k of Object.keys(row)) if (!KNOWN.includes(k) && !isDoc(k)) fail("unlisted row key '" + k + "' (refused: name its reader in gen-n65-foundry.cjs KNOWN first)");
  for (const k of LOCKED) if (!same(row[k], base.row[k])) fail("row." + k + " " + JSON.stringify(row[k]) + " is the slot's (" + JSON.stringify(base.row[k]) + "): the doors fix it");
  for (const k of Object.keys(drop)) {
    if (k in row) fail("drop." + k + " but the row still has " + k);
    if (!(k in base.row)) fail("drop." + k + ": n52's row has no " + k);
    if (String(drop[k] || "").trim().length < 12) fail("drop." + k + ": give the reason (who read it, why it can go)");
  }
  for (const k of Object.keys(base.row)) if (!(k in row)) {
    if (!(k in drop)) fail("row key '" + k + "' dropped with no reason (add drop." + k + ": \"<reason>\"; nothing lost, PRINCIPLES 1)");
    table.push([k, "drop", drop[k]]);
  }
  for (const k of Object.keys(row)) table.push([k, !(k in base.row) ? "add" : same(row[k], base.row[k]) ? "keep" : "replace", ""]);
  // the pieces: every one keyed to this room
  for (const k of LISTS) {
    const arr = f[k] || [];
    if (!Array.isArray(arr)) fail(k + " is not a list");
    for (const o of arr) if ((k === "teach35" ? o.room : owner(k, o)) !== id) fail(k + ": a row keyed to '" + (k === "teach35" ? o.room : owner(k, o)) + "', not " + id + ": " + JSON.stringify(o).slice(0, 160));
  }
  // the room's opening (the checker's IN VIEW / I7: n59-l-room.py overlays Design/levels/n65/openings65.json on n52's openings53.json)
  if ("opening" in f && !OPENINGS.includes(f.opening)) fail("opening '" + f.opening + "' (one of " + OPENINGS.join(", ") + "; leave it out to keep n52's)");
  if ("opening_why" in f && !("opening" in f)) fail("opening_why with no opening");
  const edited = !same(stripWhy(f), stripWhy(base));
  // every NEW or changed piece carries a job and a why (A-1 / R8: the room checker counts them); n52's own pieces, kept as they were, pass as
  // they are; note = why when a builder leaves note out
  for (const k of ZONE_KEYED.concat(["props"])) {
    const old = new Set((base[k] || []).map((o) => JSON.stringify(o)));
    for (const o of f[k] || []) if (!old.has(JSON.stringify(o)) && !(String(o.job || "").trim() && String(o.why || "").trim())) fail(k + ": a new piece without job / why (R8): " + JSON.stringify(o).slice(0, 160));
  }
  return { table, edited };
}
const OPENINGS = ["vista", "ambushed", "ambusher", "held"]; // openings53.json's four (NIGHT53 I7)
// the file's geometry and row (its _why and its opening are notes for the checker, not a rebuild)
const stripWhy = (f) => { const c = Object.assign({}, f, { _why: "" }); delete c.opening; delete c.opening_why; return c; };
// the plan's convention (gen-n52-foundry.cjs `piece`): note = why
function withNotes(f) {
  let n = 0;
  for (const k of ZONE_KEYED) for (const o of f[k] || []) if (o.why && o.note === undefined) { o.note = o.why; n++; }
  return n;
}
const roomFile = (p) => JSON.parse(read(p));

// ---------------------------------------------------------------- the assembly: n52's plan with each slot's file in place
// A list a file leaves exactly as n52 had it is not touched (a8's boxes and lights are not contiguous in n52's lists); a changed list goes in at
// the place its room's first row had (gen-n57-kiln.cjs swap). Every room, door, piece and key outside the slots is asserted n52's.
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
      const mine = (o) => (k === "teach35" ? o.room : owner(k, o)) === id;
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
    report.push({ id, use: f.use, file: f.file, edited, table, counts, notes, keep: tally("keep"), replace: tally("replace"), add: tally("add"), drop: tally("drop") });
    if (edited && f.data.row.bot43 !== false) report[report.length - 1].warn = "WARN: rebuilt but the row has no bot43: false (the walk bot's old id scripts would drive the new geometry)";
  }
  // the guards (gen-n52-foundry.cjs writePlan): nothing outside the slots changed
  for (const r of plan0.rooms) if (!SLOTS.includes(r.id) && !same(r, plan.rooms.find((q) => q.id === r.id))) throw new Error("room changed: " + r.id);
  if (!same(plan0.rooms.map((r) => r.id), plan.rooms.map((r) => r.id))) throw new Error("room order changed");
  for (const k of LISTS) {
    const outside = (list) => (list || []).filter((o) => !SLOTS.includes(k === "teach35" ? o.room : owner(k, o)));
    if (!same(outside(plan0[k]), outside(plan[k]))) throw new Error(k + ": a row outside the slots changed");
  }
  if (!same(plan0.doors, plan.doors)) throw new Error("doors changed (the lead's: the doors fix every rect)");
  for (const k of Object.keys(plan0)) if (!["_why", "id", "rooms"].concat(LISTS).includes(k) && !same(plan0[k], plan[k])) throw new Error(k + " changed");
  // NIGHT72 N0c (the lead's call, GG 2026-10-11 lights "placed and floating"): a roofless a3 keeps no ceiling light (see ROOFLESS_ROOM above)
  const a3row = plan.rooms.find((r) => r.id === ROOFLESS_ROOM);
  report.droppedLights = [];
  if (a3row && a3row.roof === false && !opts.keepCeilingLights) {
    const gone = (plan.lights || []).filter((l) => l.room === ROOFLESS_ROOM && l.kind === "ceiling" && ROOFLESS_LIGHTS.includes(l.id));
    plan.lights = plan.lights.filter((l) => !gone.includes(l));
    report.droppedLights = gone.map((l) => l.id);
  }
  plan.id = opts.id || ID;
  plan._why = "NIGHT65 (row foundry65; Tools/night/gen-n65-foundry.cjs): n52-foundry's plan with " + SLOTS.length + " slots from room files (" + report.map((r) => r.id + " " + r.use + (r.edited ? " (rebuilt)" : " (= n52)")).join(", ") + "). Before it: " + plan0._why;
  return { plan, report };
}
function printReport(report, all) {
  if (report.droppedLights && report.droppedLights.length) console.log("  a3 is roofless (roof false): its ceiling lights " + report.droppedLights.join(", ") + " are out of the plan (nothing above them; NIGHT72 N0c, the lead's call)");
  for (const r of report) {
    const head = "  slot " + r.id + ": " + r.use + " " + r.file + (r.passSha ? " (PASS " + r.passSha + ")" : "") + (r.edited ? ": REBUILT" : ": n52's room exactly");
    console.log(head + "; " + r.counts + (r.notes ? "; note = why filled on " + r.notes : ""));
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
// openings65.json from the room files in their slots: a slot that is not old and whose file sets `opening` writes it (n59-l-room.py overlays
// it on n52's openings53.json, so every other room keeps n52's opening); no timestamp, so an unchanged set writes the same bytes
function openingsOf(files) {
  const o = { _why: "NIGHT65 (generated by Tools/night/gen-n65-foundry.cjs from the room files in their slots; the lead's, never committed by a builder): n65-foundry's rooms whose room file sets an opening (vista / ambushed / ambusher / held), overlaid by Tools/night/n59-l-room.py on Design/levels/n52/openings53.json. A slot that holds n52's room (old), or a file with no opening, writes nothing here and keeps n52's.", foundry: {}, _cards: {} };
  for (const id of SLOTS) {
    const f = files[id];
    if (f.use === "old" || !("opening" in f.data)) continue;
    o.foundry[id] = f.data.opening;
    if (f.data.opening_why) o._cards[id] = f.data.opening_why;
  }
  return o;
}
function writePlanFile(plan, report, spec, files) {
  fs.mkdirSync(rel(path.dirname(PLAN_OUT)), { recursive: true });
  fs.writeFileSync(rel(OPENINGS_OUT), JSON.stringify(openingsOf(files), null, 1) + "\n");
  const text = read(PLAN_IN);
  const eol = eolOf(text);
  const out = JSON.stringify(plan, null, 1).replace(/\n/g, eol) + eol;
  fs.writeFileSync(rel(PLAN_OUT), out);
  fs.writeFileSync(rel(USED), JSON.stringify({ _why: "NIGHT65: what the last gen-n65-foundry.cjs plan step assembled (read by --finish for generatedBy; the lead's, never committed by a builder)", slots: spec, plan: PLAN_OUT, at: new Date().toISOString(), rooms: report.map((r) => ({ id: r.id, use: r.use, file: r.file, rebuilt: r.edited, passSha: r.passSha || null })) }, null, 1) + "\n");
}

// ---------------------------------------------------------------- the finish (gen-n52-foundry.cjs lines 429-473, kept): the level from levelgen's file
function finishLevel(lvl, n52, usedLine) {
  if (!lvl.data || !lvl.campaign) throw new Error("not a levelgen level file");
  lvl.data.name = "n46-foundry"; // the older systems' name for level 1, as n46 / n47 / n48 / n52-foundry carry it (MapModelJson reads the outer name first)
  // ---- gen-n47-foundry.cjs's a9 (copied by gen-n52-foundry.cjs: the same keys in the same order, so a9 comes out bit-equal) ----
  const rooms = lvl.campaign.rooms;
  const a9 = rooms.find((r) => r.id === "a9");
  if (!a9 || a9.boss !== "wick" || !a9.wick37) throw new Error("a9 (THE STACK, wick) missing");
  const c = a9.wick37.centre; // [0, 0, -382]
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
  // ---- gen-n48-foundry.cjs's a9 (copied) ----
  a9.climbTop48 = {
    _why: "NIGHT48 W (level row climbTop48; G1): THE STACK's top third gets a LIP at the far end of each top-third panel (after panels 4, 5 and 6 of n47's stack): a catch shelf a little under the rider's feet at the panel's end, against the wall, so a release held past the panel's end lands on it and a short hop rides the next panel. The numbers are config, not this row: design.n48.climbTop48.numbers (lipAfter, lipOver, lipArcDeg, lipStartDeg, lipDepth); SurvivalSim.N48.Wick.cs reads this key's presence only.",
    lips: true,
  };
  // ---- the guards against n52-foundry.json (read only): a9 and every kept room bit-equal ----
  const b9 = n52.campaign.rooms.find((r) => r.id === "a9");
  if (!same(a9, b9)) throw new Error("a9 is not n52-foundry's a9 bit for bit");
  if (!same(rooms.map((r) => r.id), n52.campaign.rooms.map((r) => r.id))) throw new Error("room order changed");
  for (const id of KEPT) if (!same(rooms.find((r) => r.id === id), n52.campaign.rooms.find((r) => r.id === id))) throw new Error("kept room changed: " + id);
  if (!same(lvl.campaign.doors, n52.campaign.doors)) throw new Error("doors changed");
  const own = (o) => SLOTS.includes(o.zone || o.room);
  for (const k of ["lights", "props", "pits", "boxes", "tunnels", "teach35"]) {
    const a = (lvl.campaign[k] || []).filter((o) => !own(o)), b = (n52.campaign[k] || []).filter((o) => !own(o));
    if (!same(a, b)) throw new Error("campaign." + k + ": a row outside the slots differs from n52-foundry's");
  }
  lvl.name = ID;
  lvl._why = "NIGHT65 (design.n65.foundry65 swaps n52-foundry for this file while the row is on; NIGHT65.md: level 1's six fight rooms rebuilt from cards in played rounds, GG 2026-10-09 'every single level needs a complete arena overhaul'): Tools/night/gen-n65-foundry.cjs, levelgen of " + PLAN_OUT + " (n52-foundry's plan; " + usedLine + "), then a9's n47 / n48 data re-applied bit-equal. n52-foundry's _why: " + n52._why;
  lvl.generatedBy = "Tools/night/gen-n65-foundry.cjs (" + usedLine + "; plan " + PLAN_OUT + " -> levelgen -> --finish); " + lvl.generatedBy;
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
function finish() {
  const src = opt("--src") || GEN_OUT;
  // NIGHT66 T1c (T1a's critic, Logs/n66/verdict-T1a.md finding 1): the finish never ships a level whose levelgen report lists an ERROR row,
  // nor one with no report, nor one written after its report (a stale report says nothing about it)
  if (!fs.existsSync(rel(REPORT))) throw new Error("--finish: no levelgen report " + REPORT + " for " + src + " (run --run)");
  // NIGHT72 N0: the roofless a3's own two rows (closedRoom a3, lights a3-L1 / L2 / L4) are allowed while the plan's a3 says roof false
  const errRows = unexpectedErrorRows(read(REPORT), rooflessPlan(PLAN_OUT));
  if (errRows.length) throw new Error("--finish: the levelgen report " + REPORT + " lists " + errRows.length + " ERROR row(s) beyond the roofless a3's (" + errRows[0].slice(0, 160) + "): the level is not shipped");
  if (!opt("--src") && fs.statSync(rel(REPORT)).mtimeMs + 1 < fs.statSync(rel(src)).mtimeMs) throw new Error("--finish: " + src + " is newer than its report " + REPORT + " (re-run levelgen: --run)");
  const lvl = JSON.parse(read(src));
  if (lvl.name !== ID) throw new Error(src + ": name " + lvl.name + ", expected " + ID + " (levelgen of " + PLAN_OUT + ")");
  const n52raw = read(N52);
  finishLevel(lvl, JSON.parse(n52raw), usedLineOf());
  const text = serLike(n52raw)(lvl);
  for (const o of OUTS) { fs.mkdirSync(path.dirname(rel(o)), { recursive: true }); fs.writeFileSync(rel(o), text); }
  if (OUTS.length > 1 && read(OUTS[0]) !== read(OUTS[1])) throw new Error("copies differ");
  console.log("n65-foundry --finish: " + src + " -> " + OUTS.join(", ") + " (" + text.length + " chars, both copies identical); a9 bit-equal to n52-foundry's; kept rooms " + KEPT.join(" ") + " bit-equal;" + usedLineOf());
}

// NIGHT66 T1a: the sha256 of the two shipped copies (OUTS), "missing" for an absent one
function shipShas() {
  const crypto = require("crypto");
  // the two tracked copies (NIGHT66 T1c: whatever --out says)
  return ["Design/levels/n65-foundry.json", "Assets/StreamingAssets/Design/levels/n65-foundry.json"].map((o) => o + " " + (fs.existsSync(rel(o)) ? crypto.createHash("sha256").update(fs.readFileSync(rel(o))).digest("hex") : "missing")).join(", ");
}

// ---------------------------------------------------------------- levelgen (SimHarness): plan -> level
function levelgen(planPath, outPath, reportPath) {
  // NIGHT66 T1c: reportPath (levelgen --report) keeps the level report beside a --out level, out of the tracked Reports/
  const extra = reportPath ? ["--report", rel(reportPath)] : [];
  const r = cp.spawnSync("dotnet", ["run", "--project", "Tools/SimHarness", "-c", "Release", "--", "levelgen", planPath, outPath, "--walk"].concat(extra), { cwd: ROOT, encoding: "utf8", maxBuffer: 64 << 20 });
  const all = (r.stdout || "") + (r.stderr || "");
  const keep = all.split(/\r?\n/).filter((l) => /ERROR|error CS|plan checks|level report|FAIL|problem|  ground|  air|bot walk|^levelgen /.test(l)).map((l) => l.slice(0, 300));
  const rep = all.match(/level report: (\d+) errors?/);
  return { code: r.status, lines: keep, errors: rep ? Number(rep[1]) : null, all };
}

// ---------------------------------------------------------------- the diff of two level files, names aside (n56-l-leveldiff.py's shape)
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
      out.push("campaign." + k + ": " + gone.length + " row(s) only in n52, " + added.length + " only in the new" + (gone.length + added.length <= 4 ? " (" + gone.map((o) => "n52 " + JSON.stringify(o).slice(0, 140)).concat(added.map((o) => "new " + JSON.stringify(o).slice(0, 140))).join("; ") + ")" : ""));
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

// ---------------------------------------------------------------- --check
function check() {
  const plan0 = JSON.parse(read(PLAN_IN));
  const n52raw = read(N52);
  const n52 = JSON.parse(n52raw);
  let bad = 0;
  const say = (ok, m) => { console.log((ok ? "PASS " : "FAIL ") + m); if (!ok) bad++; };
  // 1. the exports are n52's rooms exactly (re-exported now); the builders' files: n52's or rebuilt
  for (const id of SLOTS) {
    const e = exportRoom(plan0, id);
    const p = OLD + "/" + id + ".json";
    say(fs.existsSync(rel(p)) && same(roomFile(p), e), p + " is n52's " + id + " exactly (re-exported from " + PLAN_IN + ")");
    const q = ROOMS + "/" + id + ".json";
    console.log("     " + q + ": " + (!fs.existsSync(rel(q)) ? "missing" : same(stripWhy(roomFile(q)), stripWhy(e)) ? "n52's room (not rebuilt yet)" : "REBUILT (differs from n52's room)"));
  }
  // 2. every slot old, through the room-file path: the plan equals n52's apart from id / _why
  const tmp = fs.mkdtempSync(path.join(os.tmpdir(), "n65check-"));
  const runs = [];
  try {
    const oldFiles = loadFiles(slotSet("old"));
    const A = assemble(plan0, oldFiles, { id: "n65-check", keepCeilingLights: true });
    const strip = (p) => Object.assign({}, p, { id: "", _why: "" });
    say(same(strip(A.plan), strip(plan0)), "every slot old: the assembled plan equals " + PLAN_IN + " apart from id / _why");
    // 3. levelgen + the finish: the level equals n52-foundry.json byte for byte once its names are n52's
    const lv = (plan, tag) => {
      const pp = path.join(tmp, tag + ".plan.json"), lp = path.join(tmp, tag + ".json");
      fs.writeFileSync(pp, JSON.stringify(plan, null, 1) + "\n");
      const g = levelgen(pp, lp);
      runs.push(g);
      if (g.errors === null || !fs.existsSync(lp)) throw new Error("levelgen failed (" + tag + "): " + g.all.slice(-800));
      const l = JSON.parse(fs.readFileSync(lp, "utf8"));
      l.name = ID; // levelgen named it after the scratch plan id
      finishLevel(l, n52, "slots " + tag);
      return { l, g };
    };
    const O = lv(A.plan, "old");
    const diffs = levelDiff(n52, O.l);
    say(diffs.length === 0, "every slot old: levelgen --walk (" + O.g.errors + " ERROR) + the finish: the level equals " + N52 + " apart from its names (" + NAMES.join(", ") + ")" + (diffs.length ? ": " + diffs.join("; ") : ""));
    const named = Object.assign({}, O.l, { name: n52.name, _why: n52._why, generatedBy: n52.generatedBy });
    // the keys' order: rebuild in n52's order (Object.assign keeps O.l's, which is levelgen's: the same)
    say(serLike(n52raw)(named) === n52raw, "every slot old: with n52's name / _why / generatedBy the bytes ARE " + N52 + " (" + n52raw.length + " chars)");
    // 4. planted: one box of a2 moved 1 m east -> the difference printed
    const P1 = loadFiles(slotSet("old"));
    P1.a2.data.boxes[0] = Object.assign({}, P1.a2.data.boxes[0], { min: [P1.a2.data.boxes[0].min[0] + 1, P1.a2.data.boxes[0].min[1], P1.a2.data.boxes[0].min[2]], max: [P1.a2.data.boxes[0].max[0] + 1, P1.a2.data.boxes[0].max[1], P1.a2.data.boxes[0].max[2]] });
    const B = assemble(plan0, P1, { id: "n65-check", keepCeilingLights: true });
    const PB = lv(B.plan, "planted");
    const pd = levelDiff(n52, PB.l);
    say(pd.length > 0, "PLANTED one a2 box moved 1 m east (levelgen --walk " + PB.g.errors + " ERROR): the difference prints (" + pd.length + " item(s)):");
    for (const d of pd) console.log("       " + d);
    // 5. planted: an unknown row key, a drop with no reason, a piece keyed to another room, a rect moved: each refused
    const refuse = (name, mut) => {
      const F = loadFiles(slotSet("old"));
      mut(F);
      try { assemble(plan0, F, { id: "n65-check", keepCeilingLights: true }); say(false, "PLANTED " + name + ": NOT refused"); } catch (e) { say(true, "PLANTED " + name + ": refused (" + e.message.slice(0, 220) + ")"); }
    };
    refuse("an unknown row key a5.row.spinners65", (F) => { F.a5.data.row.spinners65 = [1]; });
    refuse("a8 drops its lessons with no reason", (F) => { delete F.a8.data.row.lessons; });
    refuse("an unknown file key b1.extras", (F) => { F.b1.data.extras = []; });
    refuse("a3's box keyed to zone a4", (F) => { F.a3.data.boxes[0] = Object.assign({}, F.a3.data.boxes[0], { zone: "a4" }); });
    refuse("a4's rect widened", (F) => { F.a4.data.row.rect = [-5, -108, 5, -89]; });
    refuse("a rebuilt a2 box with no job / why", (F) => { F.a2.data.boxes.push({ zone: "a2", kind: "hull", min: [0, 0, -45], max: [1, 2, -44] }); });
    refuse("a2's opening 'sideways'", (F) => { F.a2.data.opening = "sideways"; });
    // the opening: a file's `opening` reaches openings65.json from a slot that is not old; an old slot writes nothing (n52's opening kept)
    const P3 = loadFiles(slotSet("old"));
    P3.a2.data.opening = "vista";
    P3.a2.data.opening_why = "the --check's planted card line";
    assemble(plan0, P3, { id: "n65-check", keepCeilingLights: true });
    const opOld = openingsOf(P3).foundry;
    P3.a2.use = "latest";
    const opNew = openingsOf(P3).foundry;
    say(same(opOld, {}) && same(opNew, { a2: "vista" }), "PLANTED a2.opening vista: from an old slot openings65.json gets nothing (n52's kept), from a latest / new slot it gets {\"a2\": \"vista\"}");
    // 5b. NIGHT72 N0: the roofless a3's allow-list itself (no levelgen): its own row (closedRoom a3) passes only while a3's plan says roof false; anything else refuses
    const repOf = (rows) => rows.map((r) => "| " + r[0] + " | ERROR | " + r[1] + " |").join("\n");
    const ownRows = repOf([["closedRoom", "a3: no `roof: true`"]]);
    say(unexpectedErrorRows(ownRows, true).length === 0 && unexpectedErrorRows(ownRows, false).length === 1, "PLANTED the roofless a3's own ERROR row (closedRoom a3): allowed with a3 roof false, refused when a3 has a roof");
    say([["lights", "a3-L1: not flush to a surface: no surface behind it"], ["lights", "a3-L3: not flush to a surface: no surface behind it"], ["closedRoom", "a4: no `roof: true`"], ["pit recovery", "a3: stuck"]].every((r) => unexpectedErrorRows(repOf([r]), true).length === 1), "PLANTED an a3 light's row (the ceiling lights are out of the plan, so none is expected), another room's closedRoom, another check's ERROR: each refused even with a3 roof false");
    // 5c. NIGHT72 N0c (the lead's call): a roofless a3 keeps no ceiling light; every other light, in every room, is n52's bit for bit
    const gone = assemble(plan0, loadFiles(slotSet("old")), { id: "n65-check" }), kept = assemble(plan0, loadFiles(slotSet("old")), { id: "n65-check", keepCeilingLights: true });
    // (NIGHT72, the lead's call: n52's own plan lost the three ceiling lights too, n72-n2-a3-lights.cjs: then n52 has none left to drop and the two plans' lights are equal; the planted case below puts them back)
    const isCeil72 = (l) => l.room === ROOFLESS_ROOM && ROOFLESS_LIGHTS.includes(l.id);
    const wantLights = plan0.lights.filter((l) => !isCeil72(l));
    say(gone.plan.lights.length === wantLights.length && gone.plan.lights.every((l) => wantLights.some((m) => same(m, l))) && !gone.plan.lights.some(isCeil72) && same(kept.plan.lights, plan0.lights),
      "a3 roof false: none of the ceiling lights " + ROOFLESS_LIGHTS.join(", ") + " is in the n65 plan (n52's plan has " + plan0.lights.length + " lights" + (plan0.lights.some(isCeil72) ? ", they come out" : ", none of them") + "; n65's " + gone.plan.lights.length + "), every other light is n52's bit for bit; --keep-ceiling-lights keeps all of n52's " + plan0.lights.length);
    const FL = loadFiles(slotSet("old"));
    for (const id of ROOFLESS_LIGHTS) FL.a3.data.lights.push({ id, room: ROOFLESS_ROOM, kind: "ceiling", at: [0, 9.95, -70], normal: [0, -1, 0], size: [3, 3, 0.1], color: "white", range: 9, intensity: 1 });
    const planted = assemble(plan0, FL, { id: "n65-check" });
    say(same(planted.report.droppedLights, ROOFLESS_LIGHTS) && !planted.plan.lights.some(isCeil72), "PLANTED the three ceiling lights put back in a3's room file: " + planted.report.droppedLights.length + " taken out (" + planted.report.droppedLights.join(", ") + ")");
    const FR = loadFiles(slotSet("old")); FR.a3.data.row.roof = true; // a3's room file with a roof again
    say(assemble(plan0, FR, { id: "n65-check" }).report.droppedLights.length === 0, "PLANTED a3's room file with roof true: no light is taken out");
    // 6. planted: a room rebuilt (a5 gains a pillar, drops standIns-free keys with reasons) replaces only its slot
    const P2 = loadFiles(slotSet("old"));
    P2.a5.data.boxes.push({ zone: "a5", kind: "hull", min: [-0.5, 0, -125.5], max: [0.5, 10, -124.5], job: "cover: planted", why: "the --check's planted pillar" });
    P2.a5.data.row.bot43 = false;
    const C = assemble(plan0, P2, { id: "n65-check", keepCeilingLights: true });
    const outside = LISTS.every((k) => same((plan0[k] || []).filter((o) => (k === "teach35" ? o.room : owner(k, o)) !== "a5"), (C.plan[k] || []).filter((o) => (k === "teach35" ? o.room : owner(k, o)) !== "a5")));
    const rowsElse = plan0.rooms.every((r) => r.id === "a5" || same(r, C.plan.rooms.find((q) => q.id === r.id)));
    say(outside && rowsElse && C.plan.boxes.filter((b) => b.zone === "a5").length === plan0.boxes.filter((b) => b.zone === "a5").length + 1, "PLANTED a5 rebuilt (one pillar added, bot43 false): only a5's slot changed (every other room row, piece, light, prop and teach35 entry bit-equal)");
    printReport(C.report.filter((r) => r.id === "a5"), false);
    // ... and through levelgen + the finish: every campaign row that differs from n52-foundry's is a5's, levelgen 0 ERROR
    const PC = lv(C.plan, "planted-a5");
    const ca = n52.campaign, cb = PC.l.campaign;
    const foreign = [];
    for (const k of Object.keys(ca)) if (Array.isArray(ca[k]) && k !== "rooms") {
      const sa = new Set(ca[k].map((o) => JSON.stringify(o))), sb = new Set((cb[k] || []).map((o) => JSON.stringify(o)));
      for (const o of ca[k].filter((o) => !sb.has(JSON.stringify(o))).concat((cb[k] || []).filter((o) => !sa.has(JSON.stringify(o))))) if ((o.zone || o.room) !== "a5") foreign.push(k + " " + JSON.stringify(o).slice(0, 100));
    }
    for (const r of ca.rooms) if (r.id !== "a5" && !same(r, cb.rooms.find((q) => q.id === r.id))) foreign.push("room " + r.id);
    const cd = levelDiff(n52, PC.l);
    // NIGHT72 N0: the baseline is the all-old level's own ERROR count (the roofless a3's two rows: 2; 0 when a3 has a roof), not a flat 0
    say(PC.g.errors === O.g.errors && foreign.length === 0 && cd.length > 0, "PLANTED a5 rebuilt, levelgen --walk " + PC.g.errors + " ERROR (the all-old level's own: " + O.g.errors + ") + the finish: every campaign row that differs from " + N52 + " is a5's (" + (foreign.length ? "FOREIGN: " + foreign.join("; ") : "none foreign") + "); the difference: " + cd.join("; ").slice(0, 700));
  } finally {
    fs.rmSync(tmp, { recursive: true, force: true });
    for (const r of ["Reports/LEVEL-n65-check-validate.md"]) fs.rmSync(rel(r), { force: true });
  }
  // 7. the generated level the lead's tree holds now (if any), against n52-foundry apart from names
  if (fs.existsSync(rel(OUTS[0])) && fs.existsSync(rel(OUTS[1]))) {
    const a = read(OUTS[0]), b = read(OUTS[1]);
    const d = levelDiff(n52, JSON.parse(a));
    console.log("     " + OUTS[0] + " now: copies " + (a === b ? "identical" : "DIFFER") + "; against " + N52 + " apart from names: " + (d.length ? d.length + " item(s): " + d.join("; ").slice(0, 600) : "equal") + " (" + (JSON.parse(a).generatedBy || "").split(";")[0] + ")");
  }
  console.log("gen-n65-foundry --check: " + (bad ? bad + " FAIL" : "all PASS") + " (levelgen runs: " + runs.length + ")");
  process.exit(bad ? 1 : 0);
}

// ---------------------------------------------------------------- main (a refusal prints one line and exits 1)
try { main(); } catch (e) { console.error("gen-n65-foundry: REFUSED: " + e.message); process.exit(1); }
function main() {
if (flag("--check")) check();
else if (flag("--finish")) finish();
else if (flag("--export")) {
  const plan0 = JSON.parse(read(PLAN_IN));
  const ids = args.filter((a) => !a.startsWith("--"));
  fs.mkdirSync(rel(ROOMS), { recursive: true });
  fs.mkdirSync(rel(OLD), { recursive: true });
  for (const id of ids.length ? ids : SLOTS) {
    if (!SLOTS.includes(id)) throw new Error(id + " is not a slot");
    const t = roomText(exportRoom(plan0, id));
    fs.writeFileSync(rel(OLD + "/" + id + ".json"), t);
    const q = ROOMS + "/" + id + ".json";
    const wrote = !fs.existsSync(rel(q)) || flag("--force");
    if (wrote) fs.writeFileSync(rel(q), t);
    if (!same(JSON.parse(t), exportRoom(plan0, id))) throw new Error("export round trip failed: " + id);
    console.log("export " + id + ": " + OLD + "/" + id + ".json" + (wrote ? " and " + q : " (" + q + " kept: a builder's file)") + " (" + t.length + " chars)");
  }
} else {
  const spec = opt("--slots") || (fs.existsSync(rel(SLOTS_FILE)) ? SLOTS_FILE : "old");
  const plan0 = JSON.parse(read(PLAN_IN));
  const set = slotSet(spec);
  // NIGHT66 T1c: --room-file=<id>:<path> (a planted case's room file, e.g. n66-deco.cjs's crates): that slot reads the file given; only with
  // --out (a planted room never reaches the tracked level files)
  const rfo = opt("--room-file");
  if (rfo) {
    const c = rfo.indexOf(":", 2); // a Windows path's drive colon is not the separator
    const rid = rfo.slice(0, c), rpath = rfo.slice(c + 1);
    if (!SLOTS.includes(rid)) throw new Error("--room-file: " + rid + " is not a slot");
    if (!OUT_DIR) throw new Error("--room-file needs --out=<a scratch dir> (a planted room never reaches the tracked level files)");
    set[rid] = { use: "latest", file: rpath, passSha: null };
    console.log("gen-n65-foundry: --room-file: slot " + rid + " reads " + rpath);
  }
  const files = loadFiles(set);
  const { plan, report } = assemble(plan0, files, { id: ID, keepCeilingLights: flag("--keep-ceiling-lights") });
  for (const r of report) r.passSha = set[r.id].passSha;
  // NIGHT66 T (T1a): --plant-error, the safe --run's proof only: a2's spawn lifted 40 m into the air, so levelgen's level report reads an ERROR
  // ("spots on the nav grid": no grid node under a2 spawn) and --run must refuse before it writes the shipped files
  if (flag("--plant-error")) {
    // NIGHT66 T1c (T1a's critic, Logs/n66/verdict-T1a.md finding 1): the planted plan and level never land in a tracked file
    if (!flag("--run") || !OUT_DIR) throw new Error("--plant-error needs --run and --out=<a scratch dir> (the planted plan and level are written only there)");
    if (/^(Design|Assets|Reports)\//.test(path.relative(ROOT, rel(OUT_DIR)).replace(/\\/g, "/") + "/")) throw new Error("--plant-error: --out=" + OUT_DIR + " is inside a tracked level folder");
    const a2 = plan.rooms.find((r) => r.id === "a2");
    a2.spawn = Object.assign({}, a2.spawn, { at: [a2.spawn.at[0], a2.spawn.at[1] + 40, a2.spawn.at[2]] });
    console.log("gen-n65-foundry: --plant-error: PLANTED a2 spawn at " + JSON.stringify(a2.spawn.at) + " (40 m over its floor: off the nav grid)");
  }
  const dry = flag("--dry-plan"); // assemble and print, write nothing (n65-slot-set.cjs --selftest)
  if (!dry) writePlanFile(plan, report, spec, files);
  const ops = openingsOf(files).foundry;
  console.log("openings: " + (dry ? "(not written) " : OPENINGS_OUT + " ") + (Object.keys(ops).length ? Object.keys(ops).map((k) => k + " " + ops[k]).join(", ") : "none set (every slot keeps n52's opening, openings53.json)"));
  console.log("gen-n65-foundry: " + PLAN_IN + " -> " + (dry ? "(--dry-plan: not written)" : PLAN_OUT) + " (slots " + spec + "; rebuilt: " + (report.filter((r) => r.edited).map((r) => r.id).join(", ") || "none") + "); every other room, door, piece and key n52's bit for bit");
  printReport(report, flag("--table"));
  if (flag("--run")) {
    const before = shipShas();
    console.log("shipped before: " + before);
    const g = levelgen(PLAN_OUT, GEN_OUT, OUT_DIR ? REPORT : null);
    for (const l of g.lines) console.log("  " + l);
    // levelgen exits 1 on any "plan checks" problem; n52's own plan has one (ground spawn 22 against box 88 in b2, the red's), so the gate is the
    // level report's ERROR count (the doneWhen's "levelgen --walk 0 ERROR") and the file written
    // NIGHT66 T (T1a; code-scout.md section 1's new finding): both refusals come BEFORE finish(), so a level with a levelgen ERROR never reaches
    // the shipped files (Design/levels/n65-foundry.json and its StreamingAssets copy); the sha256 of both printed before and after
    if (g.errors === null || !fs.existsSync(rel(GEN_OUT))) { console.log("levelgen failed (exit " + g.code + "): " + g.all.slice(-800)); console.log("shipped after:  " + shipShas() + " (NOT written)"); process.exit(1); }
    // NIGHT72 N0: only the roofless a3's own rows (closedRoom a3, lights a3-L1 / L2 / L4) may stand; any other ERROR row refuses
    const beyond = g.errors ? (fs.existsSync(rel(REPORT)) ? unexpectedErrorRows(read(REPORT), rooflessPlan(PLAN_OUT)) : ["the levelgen report " + REPORT + " is missing"]) : [];
    if (g.errors && beyond.length === 0) console.log("levelgen --walk: " + g.errors + " ERROR row(s), all the roofless a3's own (closedRoom a3: no `roof: true`): allowed (NIGHT72 N0)");
    if (beyond.length) {
      const after = shipShas();
      for (const b of beyond) console.log("  beyond the roofless a3's: " + b);
      console.log("levelgen --walk: " + g.errors + " ERROR (RED): REFUSED, the shipped files NOT written");
      console.log("shipped after:  " + after + (after === before ? " (byte-identical)" : " (CHANGED: a bug)"));
      process.exit(1);
    }
    finish();
    console.log("levelgen --walk: " + (g.errors ? g.errors + " ERROR row(s), all the roofless a3's own (allowed, NIGHT72 N0)" : "0 ERROR"));
    console.log("shipped after:  " + shipShas() + (OUT_DIR ? (shipShas() === before ? " (byte-identical: --out writes only " + OUT_DIR + ")" : " (CHANGED under --out: a bug)") : ""));
    if (OUT_DIR) console.log("the round's level: " + OUTS[0] + " " + require("crypto").createHash("sha256").update(fs.readFileSync(rel(OUTS[0]))).digest("hex") + "; plan " + PLAN_OUT + ", report " + REPORT + ", openings " + OPENINGS_OUT);
  }
}
}

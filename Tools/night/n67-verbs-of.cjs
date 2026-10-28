// NIGHT67 builder T (T1b; slate T1b line 1; FORGE67b redteam.md 6 / 8): each room's verb for a round, from the slate SNAPSHOT (the round's
// Logs/n67/<tag>/slate-at-start.json: a slate write during a round changes nothing) against the room row's verb65 in the round's level file.
// Per room one line `verb65 <room> <row verb65|-> (slate <verb|->): <row|plant|MISMATCH|none>`:
//   row       the row carries verb65 and it equals the slate's verb (a rebuilt room: the pilots read the row)
//   plant     the row has no verb65 (a base room: a red, an old slot) and the slate names a verb: skip v2 and n46flow --verbs65 get
//             `--plant-verb65 <room>:<verb>` (VerbPilot.cs: the plant also sets bot43 false in memory)
//   MISMATCH  the row's verb65 differs from the slate's (or the slate has none): the round refuses (a verb change re-runs its red first)
//   none      neither names a verb (no verb run for the room)
//   node Tools/night/n67-verbs-of.cjs <slate.json> <level.json> <room> [<room> ...]            the lines; exit 1 on a MISMATCH
//   node Tools/night/n67-verbs-of.cjs <slate.json> <level.json> <rooms...> --plants            `g7:ride,g8:pierce` (the plant rooms), or nothing
//   node Tools/night/n67-verbs-of.cjs <slate.json> <level.json> <rooms...> --verb=<v>          the rooms whose verb is <v>, comma-joined
//   node Tools/night/n67-verbs-of.cjs <slate.json> <level.json> <rooms...> --rooms-of=<L>      (rooms ignored) the slate's rooms of level <L>
const fs = require("fs");
const args = process.argv.slice(2);
const opts = args.filter((a) => a.startsWith("--")), pos = args.filter((a) => !a.startsWith("--"));
const [slatePath, levelPath, ...rooms] = pos;
if (!slatePath) { console.error("usage: n67-verbs-of.cjs <slate.json> <level.json> <room>... [--plants | --verb=<v> | --rooms-of=<L>]"); process.exit(2); }
const KNOWN = ["--plants", "--verb", "--rooms-of"];
for (const o of opts) if (!KNOWN.includes(o.split("=")[0])) { console.error("n67-verbs-of: REFUSED: unknown option " + o); process.exit(2); }
const opt = (n) => { const a = opts.find((x) => x.startsWith(n + "=")); return a ? a.slice(n.length + 1) : null; };
const slate = JSON.parse(fs.readFileSync(slatePath, "utf8"));
const slices = Array.isArray(slate) ? slate : slate.slices || [];
if (opt("--rooms-of")) { console.log(slices.filter((s) => s.level === opt("--rooms-of")).map((s) => s.id).join(" ")); process.exit(0); }
const level = JSON.parse(fs.readFileSync(levelPath, "utf8"));
const rows = new Map(((level.campaign || {}).rooms || []).map((r) => [r.id, r]));
const out = [];
for (const id of rooms) {
  const row = rows.get(id);
  if (!row) { out.push({ id, row: "-", slate: "-", act: "MISMATCH", why: "no room " + id + " in " + levelPath }); continue; }
  const s = slices.find((x) => x.id === id);
  const sv = s && s.verb ? String(s.verb) : null, rv = row.verb65 ? String(row.verb65) : null;
  const act = rv && sv ? (rv === sv ? "row" : "MISMATCH") : rv && !sv ? "MISMATCH" : !rv && sv ? "plant" : "none";
  out.push({ id, row: rv || "-", slate: sv || "-", act, verb: rv || sv, why: act === "MISMATCH" ? (sv ? "the room file's verb65 differs from the slate's verb" : "the slate names no verb for a room whose file has verb65") : "" });
}
if (opts.includes("--plants")) { console.log(out.filter((o) => o.act === "plant").map((o) => o.id + ":" + o.slate).join(",")); process.exit(0); }
if (opt("--verb")) { console.log(out.filter((o) => o.act !== "MISMATCH" && o.verb === opt("--verb")).map((o) => o.id).join(",")); process.exit(0); }
for (const o of out) console.log("verb65 " + o.id + " " + o.row + " (slate " + o.slate + "): " + o.act + (o.why ? " (" + o.why + ")" : ""));
process.exit(out.some((o) => o.act === "MISMATCH") ? 1 : 0);

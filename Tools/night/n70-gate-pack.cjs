// NIGHT70 T1c (THE AREAS): L67=<iceline|kiln|spire> (or --l67) packs a room of that level: the round's saved <id>.json / <L>.plan.json, the
// red Logs/n70/red-<L>, the area card (card.md), room-unchanged.txt (the room file against the round's record <round>/room-files/<id>.json,
// else the file at the round's sha: FALLBACK, said; a difference REFUSED), room-file-at-round.json (the copy deco.txt judges), area.txt (M1),
// deco.txt (M4, re-run on that copy), levelgen-report.md (the base level's own ERROR rows first), verbs65-skip.txt (NIGHT70 I4: the fresh
// seeds 21-30 on the round's slate-at-start.json, run here, or the round's pre-run taken by `--fresh <file>` only when its header's room-file
// sha256 equals the round's record <round>/room-files/<id>.json and its command ran `--seed 21 --seeds 10`), the slate's launch M3 beside each launch verdict; the verb65 check against the round's slate snapshot. Every
// GATE piece not found (card, red side, verbs, table row, room diff, air, the fresh skip, the level review of THIS round with a '## <id>'
// section) is listed in the README's FIRST line "MISSING: ..." and the pack exits 3 (else "COMPLETE", exit 0). Level 1 (L67 unset): below.
// NIGHT70 (FORGE66's copy of NIGHT65 builder T's n70-gate-pack.cjs; its red team's findings 1 and 11: Logs/n70, the reds Logs/n70/red and
// Logs/n70/red65, the room's verb plan in place of its card, the skip at 10 seeds, the round's saved level and plan when present, the slot
// rooms from slots65.json). The GATE evidence pack (NIGHT65.md section 4 step 4, rule 9 "reviewers see evidence, not the story"): one room's folder
// Logs/n70/gate/<id>/ holding everything its GATE critic and blind spotter read, and nothing else. Read only on the tree: it writes the pack
// folder and the spot key, never Design/levels (iso pictures it must draw go to a temporary folder, deleted after).
//   bash Tools/night/n70-gate-pack.sh <id> <round-tag> [--logs Logs/n70] [--plan Design/levels/n65/foundry.plan.json]
//        [--level Design/levels/n65-foundry.json] [--out Logs/n70/gate/<id>] [--key Logs/n70/gate/keys/<id>-spot-key.txt] [--sha <sha>]
//        [--blind Tools/night/n65-blind.py] [--no-skip] [--fresh <round's pre-run of skip v2 on seeds 21-30>]
//   NIGHT70 I4 (+ fix (c)): --fresh <file> is taken in place of the fresh run only when the file holds skip v2's own lines (printed by
//   `n65verb skip ... --room-file <round>/room-files/<id>.json`), anchored at a line's start: `# room-file sha256: <hex>` equal to the sha256
//   (line endings aside) of <round>/room-files/<id>.json, `# level sha256: <hex>` equal to that of the level file this pack runs (the round's
//   saved level), and its header `n65verb skip v2: ... room <id>, ... seeds 21..30 (--seeds 10)`; otherwise it is not taken (said, with the
//   reason) and the fresh skip runs here
// The pack:
//   table.md         the room's row of <logs>/<round>/table.md (which carries the red in brackets), with its header
//   verbs.md         the room's verb-pilot and skip lines: the round's (<logs>/<round>/verbs.md, verbs-*.txt) against the red's
//                    (Logs/n70/red/verbs-*.txt, Logs/n70/red65/verbs-*.txt); with verb65 on the room's row of the level, a
//                    fresh `n65verb skip --seed 21 --seeds 10 --bars Logs/n70/slate.json` (skip v2 on the GATE's fresh seeds 21-30, never
//                    tuned on: NIGHT70 I4; its M2 FRESH line is the ROOM list's fresh test, median >= 1.25 with >= 7 of 10 exercised)
//                    on the level (saved as verbs65-skip.txt, its touch table and launch36 kills kept for n70-deco.cjs)
//   air.md           n65-air.cjs on the round's plan against the red's plan (Design/levels/n52/foundry.plan.json)
//   spot-A.png / spot-B.png   the round's iso65 picture and the red's, both through n65-blind.py; A / B by a fair coin seeded from the room
//                    id and the sha (sha256 of "<id>:<sha>", its first hex digit even: A = the round); the mapping goes to the key file,
//                    OUTSIDE the pack (the spotter never sees it)
//   plan.md          the room's verb plan at the sha (Research/2026-10-23-wave66/plans/<id>.md)
//   room-diff.txt    git diff of the room file from n52's export (rooms-n52/<id>.json) to the round's (rooms/<id>.json), both at the sha
//   checks.txt       the round's room-check, flow A / B / V (the room's rows, SUMMARY, STYLE), pads and ride against both reds'
//   open-findings.md the room's BLOCKING lines from every <logs>/r*/review-*.md and the cardgate files (<logs>/cardgate*.md), verbatim
//   README.md        each file with its command and the sha
const fs = require("fs");
const path = require("path");
const os = require("os");
const cp = require("child_process");
const crypto = require("crypto");
const THROWS = require("./n70-throws-sum.cjs"); // NIGHT70 Z1: the throw chart's one reader (M3, DECIDE70 D-3)
const ROOT = path.join(__dirname, "../..");
// NIGHT70 T1c: a refusal (or a failure) is one line and exit 1; a pack folder the refusal left empty is removed (no stale pack)
process.on("uncaughtException", (e) => { console.error("n70-gate-pack: " + (/^REFUSED/.test(e.message) ? "" : "FAILED: ") + e.message); try { if (typeof OUT === "string" && fs.existsSync(OUT) && fs.readdirSync(OUT).length === 0) fs.rmdirSync(OUT); } catch (_) { /* nothing */ } process.exit(1); });
const args = process.argv.slice(2);
const pos = args.filter((a, i) => !a.startsWith("--") && !(i > 0 && args[i - 1].startsWith("--") && !["--no-skip"].includes(args[i - 1])));
const opt = (k, d) => { const i = args.indexOf(k); return i >= 0 && i + 1 < args.length ? args[i + 1] : d; };
const [id, round] = pos;
if (!id || !round) { console.error("usage: n70-gate-pack.sh <id> <round-tag> [--logs ...] [--plan ...] [--level ...] [--out ...] [--key ...] [--sha ...] [--blind ...] [--no-skip]"); process.exit(2); }
const abs = (p) => (path.isAbsolute(p) ? p : path.join(ROOT, p));
const LOGS = abs(opt("--logs", "Logs/n70")); let LEVEL = opt("--level", "Design/levels/n65-foundry.json");
let PLAN = opt("--plan", null), planHow = "--plan";
const OUT = abs(opt("--out", "Logs/n70/gate/" + id)), KEY = abs(opt("--key", "Logs/n70/gate/keys/" + id + "-spot-key.txt"));
const BLIND = opt("--blind", "Tools/night/n65-blind.py");
// NIGHT70 T1c (slate T1c line 1): L67=<iceline|kiln|spire> (or --l67 <L>): the level of Tools/night/levels67.json. Its round's saved level
// (<round>/<id>.json) and plan (<round>/<L>.plan.json) are read (n70-round.sh always saves them: no scratch assembly for an L67 level), the
// red is Logs/n70/red-<L> (NIGHT70.md bearings step 6) and its plan the base level's plan, the card Research/2026-10-24-wave67/cards/<id>.md,
// the room diff rooms-base/<id>.json -> rooms/<id>.json; the round's area-<id>.txt, deco-<id>.txt and levelgen report are copied in (the
// ROOM list's GATE: M1 area, M4 deco, the levelgen ERROR guard); a room whose row has no verb65, or one that differs from the slate's verb,
// is REFUSED before anything runs (FORGE67b redteam.md 6).
const L67 = (opt("--l67", null) || process.env.L67 || "").replace(/^foundry$/, "") || null;
const T67 = L67 ? JSON.parse(fs.readFileSync(path.join(ROOT, "Tools/night/levels67.json"), "utf8"))[L67] : null;
if (L67 && (!T67 || L67.startsWith("_"))) { console.error("n70-gate-pack: L67=" + L67 + " is not a level of Tools/night/levels67.json"); process.exit(2); }
const RED_PLAN = T67 ? T67.planIn : "Design/levels/n52/foundry.plan.json";
const REDS = T67 ? ["red-" + L67] : ["red", "red65"];
const git = (a) => cp.execFileSync("git", a, { cwd: ROOT, encoding: "utf8", maxBuffer: 64 << 20, stdio: ["ignore", "pipe", "pipe"] }); // NIGHT70 T1c: git's own "fatal:" stays in the error (a missing card is said in card.md)
const read = (p) => (fs.existsSync(p) ? fs.readFileSync(p, "utf8").replace(/\r\n/g, "\n") : null);
const RD = path.join(LOGS, round);
if (!fs.existsSync(RD)) { console.error("n70-gate-pack: no round folder " + RD); process.exit(1); }
// the sha: --sha, else the round's assembly header (gen-latest.txt "# sha: x"), else HEAD
let sha = opt("--sha", null), shaFrom = "--sha";
if (!sha) { const g = read(path.join(RD, "gen-latest.txt")); const m = g && /^# sha: ([0-9a-f]{7,40})/m.exec(g); if (m) { sha = m[1]; shaFrom = round + "/gen-latest.txt's header"; } }
if (!sha) { sha = git(["rev-parse", "--short=8", "HEAD"]).trim(); shaFrom = "HEAD (no " + round + "/gen-latest.txt sha)"; }
fs.mkdirSync(OUT, { recursive: true });
const tmp = fs.mkdtempSync(path.join(os.tmpdir(), "n65gate-"));
// NIGHT70 T1c fix (verdict-T1c.md problem 2): every piece the GATE needs that comes back not found is listed here; the README's FIRST line
// then reads "MISSING: ..." and the pack exits 3 (the files are still written, so the critic sees what is there)
const MISSING = [];
const sha256n = (text) => crypto.createHash("sha256").update(String(text).replace(/\r\n/g, "\n")).digest("hex"); // line endings aside
const readme = [];
const note = (file, cmd, extra) => readme.push(`- **${file}**: ${cmd}${extra ? " " + extra : ""}`);
const write = (file, text) => fs.writeFileSync(path.join(OUT, file), text.endsWith("\n") ? text : text + "\n");
const run = (cmd, a, o) => { const r = cp.spawnSync(cmd, a, Object.assign({ cwd: ROOT, encoding: "utf8", maxBuffer: 64 << 20 }, o || {})); return { code: r.status, out: ((r.stdout || "") + (r.stderr || "")).replace(/\r\n/g, "\n") }; };
const roomLine = (l) => new RegExp("(^|[^a-z0-9])" + id + "([^a-z0-9]|$)").test(l);
let SCRATCH = null; // the scratch tree of the round's assembly (no --plan)
// FORGE66 red team 11: the round's own saved level and plan (n70-round.sh copies them) unless --level / --plan name others
const SAVED_LEVEL = path.join(RD, (T67 ? T67.id : "n65-foundry") + ".json"), SAVED_PLAN = path.join(RD, (L67 || "foundry") + ".plan.json");
if (T67 && (!fs.existsSync(SAVED_LEVEL) || !fs.existsSync(SAVED_PLAN)) && !args.includes("--level")) { console.error("n70-gate-pack: L67=" + L67 + ": the round " + RD + " lacks its saved level or plan (" + path.basename(SAVED_LEVEL) + ", " + path.basename(SAVED_PLAN) + "): run the round with n70-round.sh"); process.exit(1); }
let LEVEL_GIVEN = args.includes("--level");
if (!LEVEL_GIVEN && fs.existsSync(SAVED_LEVEL)) { LEVEL = SAVED_LEVEL; LEVEL_GIVEN = true; }
if (!PLAN && fs.existsSync(SAVED_PLAN)) { PLAN = SAVED_PLAN; planHow = "the round's saved plan (" + path.relative(ROOT, SAVED_PLAN).replace(/\\/g, "/") + ")"; }

try {
  // ---- the round's plan: --plan, else assembled from the room files AT THE SHA in a scratch tree (the assembler, n52's plan and the room
  // files as git shows them there; --slots=latest; the scratch tree is deleted after: Design/levels is never written)
  if (!PLAN) {
    const st = path.join(tmp, "tree");
    SCRATCH = st;
    // + the config pair the harness reads beside its working folder, and n52's level (the finish's guard), for a scratch level when needed
    const files = ["Tools/night/gen-n65-foundry.cjs", "Design/levels/n52/foundry.plan.json", "PortExport/config.json", "Design/unity-config.json", "Design/levels/n52-foundry.json"];
    for (const r of Object.keys(JSON.parse(git(["show", sha + ":Design/levels/n65/slots65.json"])).slots)) files.push("Design/levels/n65/rooms/" + r + ".json", "Design/levels/n65/rooms-n52/" + r + ".json");
    for (const f of files) { fs.mkdirSync(path.join(st, path.dirname(f)), { recursive: true }); fs.writeFileSync(path.join(st, f), git(["show", sha + ":" + f])); }
    const r = run(process.execPath, [path.join(st, "Tools/night/gen-n65-foundry.cjs"), "--slots=latest"], { cwd: st });
    if (r.code !== 0) throw new Error("the scratch assembly at " + sha + " failed: " + r.out.slice(-600));
    PLAN = path.join(st, "Design/levels/n65/foundry.plan.json");
    planHow = "assembled at " + sha + " (`gen-n65-foundry.cjs --slots=latest` on the room files git shows there, in a scratch tree deleted after)";
  }
  // ---- (0) NIGHT70 T1c: the room's verb65 (the round's plan row) against the slate's verb (the night's slate, or --slate): refused on a
  // mismatch, and for an L67 level on a missing verb65 (a rebuilt room carries verb65 = the slate's verb: the ROOM list)
  {
    const MAINV = path.resolve(cp.execFileSync("git", ["rev-parse", "--git-common-dir"], { cwd: ROOT, encoding: "utf8" }).trim().replace(/^(?!\w:|\/)/, ROOT + "/"), "..");
    const siv = args.indexOf("--slate");
    // NIGHT70 T1c round 2 (verdict-T1c-r2.md A5): the ROUND's slate snapshot, as the skip's bar reads it (--slate overrides; the live slate
    // only when the round kept no snapshot, said in the line)
    const SNAPV = path.join(RD, "slate-at-start.json");
    const SL = siv >= 0 && args[siv + 1] ? path.resolve(args[siv + 1]) : fs.existsSync(SNAPV) ? SNAPV : path.join(MAINV, "Logs/n70/slate.json");
    const slv = fs.existsSync(SL) ? (JSON.parse(fs.readFileSync(SL, "utf8")).slices || []).find((x) => x.id === id) : null;
    const rowv = ((JSON.parse(fs.readFileSync(abs(PLAN), "utf8")).rooms || []).find((x) => x.id === id) || {}).verb65 || null;
    const sv = slv && slv.verb ? String(slv.verb) : null;
    const line = "verb65 " + id + " " + (rowv || "-") + " (slate " + (sv || "-") + ", " + SL.replace(/\\/g, "/") + ")";
    if (rowv && sv && rowv !== sv) throw new Error("REFUSED: " + line + ": the room file's verb65 differs from the slate's verb (a GATE judges the slate's verb; a verb change re-runs the red first)");
    if (T67 && !rowv) throw new Error("REFUSED: " + line + ": the round's room has no verb65 (a rebuilt room carries verb65 = the slate's verb: the ROOM list)");
    if (rowv && !sv) throw new Error("REFUSED: " + line + ": the slate names no verb for " + id);
    readme.push("- **verb65**: " + line + ": equal");
    console.log("n70-gate-pack " + id + ": " + line + ": equal");
  }
  // ---- (0b) NIGHT70 T1c fix (verdict-T1c.md problems 1 and 4): the room file unchanged since the round. The round's record
  // (<round>/room-files/<id>.json, written by n70-round.sh at its start), else (a round run before the record existed) the room file at the
  // round's sha (FALLBACK, said); against the room file now (the working tree): a difference is REFUSED with both hashes. The pack's deco then
  // judges that recorded file, never the working tree.
  let ROUND_ROOM = null;
  {
    const roomRel = (T67 ? T67.rooms : "Design/levels/n65/rooms") + "/" + id + ".json";
    const now = fs.existsSync(abs(roomRel)) ? sha256n(fs.readFileSync(abs(roomRel), "utf8")) : null;
    const rec = path.join(RD, "room-files", id + ".json");
    let then = null, from = "";
    if (fs.existsSync(rec)) { then = sha256n(fs.readFileSync(rec, "utf8")); from = round + "/room-files/" + id + ".json (the round's own record)"; ROUND_ROOM = rec; }
    else {
      try { const txt = git(["show", sha + ":" + roomRel]); then = sha256n(txt); ROUND_ROOM = path.join(tmp, "round-" + id + ".json"); fs.writeFileSync(ROUND_ROOM, txt); from = "git show " + sha + ":" + roomRel + " (FALLBACK: " + round + " kept no room-files record; the round's sha from " + shaFrom + ")"; }
      catch (e) { then = null; }
    }
    if (then === null) throw new Error("REFUSED: no record of the room file " + round + " ran (no " + round + "/room-files/" + id + ".json and no " + roomRel + " at " + sha + ")");
    const line = "the room file " + roomRel + ": the round's " + then.slice(0, 16) + " (" + from + "), now " + (now ? now.slice(0, 16) : "MISSING");
    if (now !== then) throw new Error("REFUSED: " + line + ": the room file changed since the round (a GATE judges the round's file: run a new round, or restore the file)");
    write("room-unchanged.txt", "# NIGHT70 GATE: the room file unchanged since the round (sha256 with line endings aside)\n" + line + ": UNCHANGED\n");
    // NIGHT70 T1c round 2 (verdict-T1c-r2.md A6): the round's room file kept in the pack (room-file-at-round.json): deco judges this copy
    fs.copyFileSync(ROUND_ROOM, path.join(OUT, "room-file-at-round.json")); ROUND_ROOM = path.join(OUT, "room-file-at-round.json");
    readme.push("- **room-file-at-round.json**: the room file the round ran (" + from + "): deco.txt judges this copy");
    readme.push("- **room-unchanged.txt**: " + line + ": UNCHANGED");
    console.log("n70-gate-pack " + id + ": " + line + ": UNCHANGED");
  }
  // ---- (1) table.md
  {
    const t = read(path.join(RD, "table.md"));
    let body;
    if (!t) { body = `not found: ${round}/table.md (the round's table is not written yet)`; MISSING.push("table: no " + round + "/table.md"); }
    else {
      const lines = t.split("\n");
      const head = lines.filter((l) => /^\| room /.test(l) || /^\|---/.test(l)).slice(0, 2);
      const row = lines.filter((l) => l.startsWith("| " + id + " "));
      if (!row.length) MISSING.push("table: no row for " + id + " in " + round + "/table.md");
      body = (row.length ? head.concat(row) : [`not found: no row for ${id} in ${round}/table.md`]).join("\n");
    }
    write("table.md", `# ${id}: the round's row (${round}/table.md), the red in brackets\n\n${body}\n`);
    note("table.md", `the header and the \`| ${id} \` row of ${path.relative(ROOT, path.join(RD, "table.md")).replace(/\\/g, "/")}`);
  }
  // ---- (2) verbs.md (+ a fresh --verbs65 skip when the level's row has verb65)
  {
    // the room's n65verb header line ("room <id> ..." / "room <id>, ..."), its seed lines (a skip run's) and its verdict line
    // NIGHT70 Z1: a launch verdict line carries the M3 of the chart passed in (the round's <round>/throws-<id>.txt; the red's
    // Logs/n70/throws-<id>-shipped.txt, the bearings' copy of the forge's chart); `where` names it
    const pick = (text, chart, where) => {
      if (!text) return [];
      const ls = text.split("\n"), out = [];
      let mine = false;
      // NIGHT70 T1b: skip v2's lines too: its header "n65verb skip v2: level ...", its seed lines "  seed N verb-aware: / refused-in: /
      // walker: / => / touches", its "  runs:" line, the verdicts "n65verb skip: M1 ... / M2 ... / totals ..." (the M1 line ends the run's
      // block only after M2, totals, the touch and launchkill lines that follow it)
      for (const l of ls) {
        if (/^n65verb [a-z]+( v2)?: level /.test(l)) { mine = new RegExp("room " + id + "[ ,]").test(l); if (mine) out.push(l); continue; }
        if (/^n65verb skip( touch| launchkill)?: (M2 |totals |room )/.test(l)) { if (new RegExp("room " + id + "[ ,|]").test(l)) out.push(l); continue; }
        if (/^n65verb [a-z]+( --[a-z-]+)*: (GREEN|RED|UNSURE|M1 )/.test(l)) { if (mine || new RegExp("room " + id + "[ ,|]").test(l)) out.push(l + launchM3(l, chart, THROWS, where)); mine = false; continue; } // NIGHT70 T1c: also the ride pilot's `n65verb ride --shoot: GREEN|RED`
        if (mine && /^  (seed \d+[: ]|NOTE|runs: )/.test(l)) out.push(l);
      }
      return out;
    };
    // NIGHT70 T1c fix (verdict-T1c.md problem 6): the slate's launch M3 is "in a pit yes" or "in lava yes" on the launched body, not the
    // pilot's GREEN: printed beside every launch verdict
    // NIGHT70 Y2 (slate Y2; scout.md X1; NIGHT68 r2: kb3's RED line "launch36 0 ... in a pit YES" read PASS): M3 PASSes only on the pilot's
    // `: GREEN` line with `launch36 [1-9]` and "in a pit YES" or "in lava YES" (the probe sets them from the launched body only)
    // NIGHT70 Z1 (slate Z1; DECIDE70 D-3; NIGHT69 a1 read kb3 and v2 FAIL on seed 1 while their 20-seed charts hold 14 / 20 and 20 / 20):
    // M3 is the room's THROW CHART (<round>/throws-<id>.txt, Tools/night/n70-throws.sh), re-counted by Tools/night/n70-throws-sum.cjs (T,
    // passed in): a candidate is a GREEN probe with launch36 1+ AND a pit / lava end or a burst killing >= 2 incl. the flier (a RED probe's
    // burst never counts); PASS at >= 10 of 20; no chart: FAIL "no throw chart". The one probe's own pit / lava reading is printed beside it.
    function launchM3(l, chart, T, where) {
      if (!/^n65verb launch( --[a-z-]+)*: (GREEN|RED)/.test(l)) return "";
      const room = (/\| room (\S+) \|/.exec(l) || [])[1] || null;
      const v = T.m3(chart, room, where || "the round's throws-" + (room || "<id>") + ".txt");
      const pit = /in a pit YES/.test(l), lava = /in lava YES/.test(l), b = (/bursts \d+ \(hits \d+, kills \d+\)/.exec(l) || ["no bursts printed"])[0];
      return "   || M3 (the slate's launch M3, DECIDE70 D-3: the room's 20-seed throw chart, PASS at >= 10 / 20 candidates): " + (v.pass ? "PASS" : "FAIL") + " (" + v.why + ")" + (v.line ? " [" + v.line + "]" : "") + " (this probe said: " + b + ", in a pit " + (pit ? "yes" : "no") + ", in lava " + (lava ? "yes" : "no") + ")";
    }
    // the round's verbs-*.txt (each pilot's own output, the seed lines kept); verbs.md (their digest) only when no .txt names the room
    const roundLines = [];
    const RCHART = path.join(RD, "throws-" + id + ".txt"), rchart = read(RCHART), rwhere = path.relative(ROOT, RCHART).replace(/\\/g, "/");
    const SCHART = path.join(LOGS, "throws-" + id + "-shipped.txt"), schart = read(SCHART), swhere = path.relative(ROOT, SCHART).replace(/\\/g, "/");
    for (const f of fs.readdirSync(RD).filter((f) => /^verbs.*\.txt$/.test(f)).sort()) { const p = pick(read(path.join(RD, f)), rchart, rwhere); if (p.length) roundLines.push(`from ${round}/${f}:`, ...p.map((l) => "    " + l)); }
    if (!roundLines.length) { const p = pick(read(path.join(RD, "verbs.md")), rchart, rwhere); if (p.length) roundLines.push(`from ${round}/verbs.md:`, ...p.map((l) => "    " + l)); }
    // FORGE66: both reds' own verb files (n70-round.sh red --shipped / red65)
    const redLines = [];
    for (const rn of REDS) {
      const TD = path.join(LOGS, rn);
      const own = path.join(TD, id, "skip.txt"); // NIGHT70 T1c: the bearings' red skip (Logs/n70/red-<L>/<id>/skip.txt)
      if (fs.existsSync(own)) { const p = pick(read(own), schart, swhere); if (p.length) redLines.push(`from ${path.relative(ROOT, own).replace(/\\/g, "/")}:`, ...p.map((l) => "    " + l)); }
      for (const f of (fs.existsSync(TD) ? fs.readdirSync(TD).filter((f) => /^verbs.*\.txt$/.test(f)).sort() : [])) { const p = pick(read(path.join(TD, f)), schart, swhere); if (p.length) redLines.push(`from ${path.relative(ROOT, TD).replace(/\\/g, "/")}/${f}:`, ...p.map((l) => "    " + l)); }
    }
    // the round's row (its plan at the sha): verb65? Then the skip runs on the round's own level: --level when given, else levelgen + the
    // finish of the scratch plan inside the scratch tree (the harness reads the sha's config beside it; nothing outside the scratch is written)
    let verb65 = null, levelBy = "", levelFile = LEVEL, levelCwd = ROOT;
    { const pr = (JSON.parse(fs.readFileSync(abs(PLAN), "utf8")).rooms || []).find((x) => x.id === id); if (pr && pr.verb65) verb65 = pr.verb65; }
    let skip = "";
    if (verb65 && !args.includes("--no-skip")) {
      if (!LEVEL_GIVEN && SCRATCH) {
        const g = run("dotnet", ["run", "--project", path.join(ROOT, "Tools/SimHarness"), "-c", "Release", "--", "levelgen", "Design/levels/n65/foundry.plan.json", "Design/levels/n65/foundry.json", "--walk"], { cwd: SCRATCH });
        const fz = run(process.execPath, [path.join(SCRATCH, "Tools/night/gen-n65-foundry.cjs"), "--finish"], { cwd: SCRATCH });
        if (fz.code !== 0) throw new Error("the scratch level at " + sha + ": levelgen exit " + g.code + " / finish: " + (g.out + fz.out).slice(-600));
        levelFile = "Design/levels/n65-foundry.json"; levelCwd = SCRATCH;
      }
      const lv = read(path.isAbsolute(levelFile) ? levelFile : path.join(levelCwd, levelFile));
      if (lv) levelBy = String(JSON.parse(lv).generatedBy || "").split(";")[0];
      const where = levelCwd === ROOT ? levelFile : "the scratch level of the round's assembly at " + sha;
      // NIGHT70 T1b: skip v2 against the room's mainBar in the night's slate (the main tree's Logs/n70/slate.json, read wherever the level is)
      // NIGHT70 T1b fix (Logs/n70/verdict-T1b.md problems 2 and 10): the MAIN tree's slate (git's common dir, whichever tree runs this), and a
      // room whose mainBar is null or missing is refused: a GATE never reads M1 against the 1.25 fallback
      const MAIN = path.resolve(cp.execFileSync("git", ["rev-parse", "--git-common-dir"], { cwd: ROOT, encoding: "utf8" }).trim().replace(/^(?!\w:|\/)/, ROOT + "/"), "..");
      const si = args.indexOf("--slate"); // the planted case's slate (a copy with a null mainBar); default the main tree's
      // NIGHT70 T1c fix (verdict-T1c.md problem 5): the ROUND's slate snapshot (<round>/slate-at-start.json: the mainBar the round ran) unless
      // --slate names another; the live slate only when the round kept none (said in the file)
      const SNAP = path.join(RD, "slate-at-start.json");
      const BARS = si >= 0 && args[si + 1] ? path.resolve(args[si + 1]) : fs.existsSync(SNAP) ? SNAP : path.join(MAIN, "Logs/n70/slate.json");
      const barsFrom = si >= 0 && args[si + 1] ? "--slate" : fs.existsSync(SNAP) ? "the round's slate snapshot" : "the LIVE slate (the round kept no slate-at-start.json)";
      const DLLP = path.join(ROOT, "Temp/SimHarness/bin/Release/net10.0/SimHarness.dll");
      // NIGHT70 T1c round 2 (verdict-T1c-r2.md A4): the DLL carries no build commit, so the line says plainly what is known: HEAD at pack
      // time and the DLL's mtime, read before AND after the run (a rebuild while it ran is said, and is a MISSING piece)
      const mt = () => (fs.existsSync(DLLP) ? fs.statSync(DLLP).mtime.toISOString().replace("T", " ").slice(0, 19) + " UTC" : "MISSING");
      const mt0 = mt();
      let build = "the DLL's build commit is not knowable (the DLL carries none): HEAD at pack time " + git(["rev-parse", "--short=8", "HEAD"]).trim() + ", the DLL built " + mt0 + " (run as built: no build here)";
      if (!fs.existsSync(BARS)) throw new Error("no " + BARS + ": skip v2's --bars (the room's mainBar) is the night's slate");
      const sl = (JSON.parse(fs.readFileSync(BARS, "utf8")).slices || []).find((x) => x.id === id);
      if (!sl || typeof sl.mainBar !== "number") throw new Error(id + "'s mainBar in " + BARS + " is " + (sl ? JSON.stringify(sl.mainBar) : "missing (no slice)") + ": the GATE's M1 needs the room's own bar (n70-slate-set.cjs " + id + " mainBar=<x>)");
      // NIGHT70 I4 (slate I4 line 2; FORGE67 red team 2 and 9): the GATE's fresh seeds 21-30 (never tuned on: the round's own skip ran 1-20),
      // the built DLL run directly under a 900 s timeout (`dotnet run`'s child outlives a killed host; NIGHT66's a8 hang); or the round's
      // pre-run (--fresh <file>), taken only when its room-file sha256 equals the round's record and its command ran --seed 21 --seeds 10
      const FRESH = opt("--fresh", null);
      const freshArgs = [path.join(ROOT, "Temp/SimHarness/bin/Release/net10.0/SimHarness.dll"), "n65verb", "skip", "--level", levelFile, "--room", id, "--seed", "21", "--seeds", "10", "--bars", BARS];
      let r = null, freshHow = "run here", freshHead = "";
      let freshCmd = `dotnet Temp/SimHarness/bin/Release/net10.0/SimHarness.dll n65verb skip --level <${where}> --room ${id} --seed 21 --seeds 10 (seeds 21-30, timeout 900 s) --bars ${BARS.replace(/\\/g, "/")} (skip v2)`;
      if (FRESH) {
        const ft = read(abs(FRESH));
        const rec = path.join(RD, "room-files", id + ".json");
        const want = fs.existsSync(rec) ? sha256n(fs.readFileSync(rec, "utf8")) : null;
        // NIGHT70 I4 fix (c) (verdict-I4.md P3): the lines skip v2 itself prints, anchored: `# room-file sha256: <hex>` (--room-file),
        // `# level sha256: <hex>` (the level it played) equal to the level file this pack would run, its own header's room and seed set
        const m = ft && /^# room-file sha256: ([0-9a-f]{64})/m.exec(ft);
        const lm = ft && /^# level sha256: ([0-9a-f]{64})/m.exec(ft);
        const lvPath = path.isAbsolute(levelFile) ? levelFile : path.join(levelCwd, levelFile);
        const lwant = fs.existsSync(lvPath) ? sha256n(fs.readFileSync(lvPath, "utf8")) : null;
        const hdr = ft && new RegExp("^n65verb skip v2: level .*, room " + id + ", .*seeds 21\\.\\.30 \\(--seeds 10\\)", "m").test(ft);
        const why = !ft ? "no such file" : !want ? "the round kept no room-files record " + round + "/room-files/" + id + ".json" : !m ? "no `# room-file sha256:` line in it" : m[1] !== want ? "its room-file sha256 " + m[1].slice(0, 16) + " differs from the round's record " + want.slice(0, 16)
          : !lm ? "no `# level sha256:` line in it (a pre-run from before I4 (c))" : !lwant ? "the level file " + lvPath.replace(/\\/g, "/") + " is missing" : lm[1] !== lwant ? "its level sha256 " + lm[1].slice(0, 16) + " differs from the level this pack runs (" + lwant.slice(0, 16) + ", " + lvPath.replace(/\\/g, "/") + ")"
          : !hdr ? "its skip v2 header is not room " + id + " on seeds 21..30 (--seeds 10)" : null;
        if (why) { freshHow = "--fresh " + FRESH + " NOT taken (" + why + "): the fresh skip run here"; console.log("n70-gate-pack " + id + ": " + freshHow); }
        else {
          // NIGHT70 I4 fix (verdict-I4.md P1): the taken pre-run's code from its M2 FRESH line (the fresh test), as the run-here skip's exit
          r = { code: /^n65verb skip: M2 FRESH PASS/m.test(ft) ? 0 : /^n65verb skip: M2 FRESH FAIL/m.test(ft) ? 1 : /^n65verb skip: M2 FRESH UNSURE/m.test(ft) ? 3 : 2, out: ft };
          freshHow = "--fresh " + FRESH + " taken: its room-file sha256 " + want.slice(0, 16) + " equals the round's record (" + round + "/room-files/" + id + ".json) and its level sha256 " + lwant.slice(0, 16) + " the level this pack runs";
          freshCmd = "the round's pre-run " + FRESH.replace(/\\/g, "/") + " (taken by --fresh; its own header lines follow)";
          freshHead = ft.split("\n").filter((l) => /^# /.test(l)).map((l) => "# pre-run " + l.slice(2)).join("\n") + "\n";
          console.log("n70-gate-pack " + id + ": " + freshHow);
        }
      }
      if (!r) { console.log("n70-gate-pack " + id + ": command: dotnet " + freshArgs.map((a) => a.replace(/\\/g, "/")).join(" ")); r = run("dotnet", freshArgs, { cwd: levelCwd, timeout: 900000 }); }
      const kept = r.out.split("\n").filter((l) => /^n65verb skip|^  seed|^  NOTE|^  runs: /.test(l)).join("\n");
      const mt1 = mt();
      if (!freshHead && mt1 !== mt0) { build += "; the DLL CHANGED while the skip ran (" + mt0 + " -> " + mt1 + "): not one build"; MISSING.push("skip 21-30: the DLL was rebuilt while it ran"); }
      // NIGHT70 T1c round 2 (verdict-T1c-r2.md A2): a refused, timed-out or crashed fresh skip is a missing piece; NIGHT70 I4: and one that
      // did not run seeds 21..30 (its header line) or printed no M2 FRESH line
      if (r.code === null || r.code === 2 || !/^n65verb skip: M1 (PASS|FAIL|UNSURE)/m.test(r.out)) MISSING.push("skip 21-30: " + (r.code === null ? "TIMEOUT or killed" : r.code === 2 ? "REFUSED (exit 2) or no M1 line" : "no M1 line (exit " + r.code + ")"));
      else if (!/seeds 21\.\.30 \(--seeds 10\)/.test(r.out) || !/^n65verb skip: M2 FRESH (PASS|FAIL|UNSURE)/m.test(r.out)) MISSING.push("skip 21-30: the skip did not run seeds 21..30 or printed no M2 FRESH line");
      write("verbs65-skip.txt", `# command: ${freshCmd}\n${freshHead}# fresh: ${freshHow}\n# sha: ${sha}; the level: ${levelBy}\n# bars: ${barsFrom} (${BARS.replace(/\\/g, "/")}), mainBar ${sl.mainBar} (the M1 line's bar; the ROOM list's fresh test is the M2 FRESH line: median >= 1.25, >= 7 of 10 exercised)\n# build: ${freshHead ? "the pre-run's (its header above)" : build}\n# exit: ${r.code} (the M2 FRESH line: 0 PASS, 1 FAIL, 3 UNSURE, 2 refused or none)\n\n${kept}\n`);
      skip = kept;
      note("verbs65-skip.txt", "`" + freshCmd + "`" + ` (skip v2 on the fresh seeds 21-30; ${freshHow}; the round's row's verb65: ${verb65})`);
    }
    if (!roundLines.length) MISSING.push("verbs: no n65verb line for " + id + " in " + round);
    if (!redLines.length) MISSING.push("verbs: the red's lines (" + REDS.map((x) => "Logs/n70/" + x).join(", ") + ")");
    if (/^launch/.test(verb65 || "") || roundLines.some((l) => /n65verb launch( --[a-z-]+)*: (GREEN|RED)/.test(l))) {
      const v = THROWS.m3(rchart, id, rwhere);
      write("throws.txt", rchart != null ? rchart : "not found: " + rwhere + " (the round's throw chart: bash Tools/night/n70-throws.sh <the round's level> " + id + " " + rwhere + " 20)\n");
      note("throws.txt", "the round's throw chart " + rwhere + " (Tools/night/n70-throws.sh, 20 probe seeds); M3 (DECIDE70 D-3, n70-throws-sum.cjs): " + (v.pass ? "PASS" : "FAIL") + " (" + v.why + ")");
      if (rchart == null) MISSING.push("M3: no throw chart " + rwhere);
    }
    write("verbs.md", `# ${id}: the verb pilots and the skip test\n\nThe round (${round}):\n\n${roundLines.length ? roundLines.join("\n") : "    not found: no n65verb line for " + id + " in " + round + "/verbs*.md / .txt"}\n\nThe reds (${T67 ? "Logs/n70/red-" + L67 + ": " + T67.base + "'s room" : "Logs/n70/red: the shipped slots, n52-foundry's rooms; Logs/n70/red65: NIGHT65's final rooms"}):\n\n${redLines.length ? redLines.join("\n") : "    not found"}\n\nThe round's row (its plan at ${sha}): verb65 ${verb65 || "none"}${verb65 ? (skip ? "; the --verbs65 skip on the round's level (verbs65-skip.txt):\n\n" + skip.split("\n").map((l) => "    " + l).join("\n") : " (--no-skip: not run)") : " (no --verbs65 skip run)"}\n`);
    note("verbs.md", `the \`n65verb\` header, verdict and seed lines naming ${id} from ${round}/verbs*.md / .txt and Logs/n70/red / red65 verbs-*.txt`);
  }
  // ---- (3) air.md
  {
    const a = run(process.execPath, [path.join(__dirname, "n65-air.cjs"), PLAN, id]);
    const b = run(process.execPath, [path.join(__dirname, "n65-air.cjs"), RED_PLAN, id]);
    // NIGHT70 T1c round 2 (verdict-T1c-r2.md A2): an air run that failed is a missing piece
    if (a.code !== 0 || /Error|EXCEPTION|REFUSED/.test(a.out)) MISSING.push("air: n65-air.cjs on the round's plan exit " + a.code + (/(Error|EXCEPTION|REFUSED)[^\n]*/.exec(a.out) || [""])[0].slice(0, 100));
    if (b.code !== 0 || /Error|EXCEPTION|REFUSED/.test(b.out)) MISSING.push("air: n65-air.cjs on the red's plan exit " + b.code);
    const shown = (t) => t.split(tmp.replace(/\\/g, "/")).join("<scratch>").split(tmp).join("<scratch>");
    write("air.md", `# ${id}: air hand-offs (Tools/night/n65-air.cjs)\n\n## The round's plan (${planHow})\n\n\`\`\`\n${shown(a.out.trim())}\n\`\`\`\n\n## The red's plan (${RED_PLAN})\n\n\`\`\`\n${b.out.trim()}\n\`\`\`\n`);
    note("air.md", `\`node Tools/night/n65-air.cjs <the round's plan> ${id}\` and \`node Tools/night/n65-air.cjs ${RED_PLAN} ${id}\``);
  }
  // ---- (4) spot-A.png / spot-B.png and the key (outside the pack)
  {
    // both drawn fresh, the same way: --no-card-marks (n52's rooms carry no line65 / frames65, so those marks alone would name the rebuild)
    const iso = (planFile, tag, label) => {
      const o = path.join(tmp, tag + ".png");
      const r = run(process.execPath, [path.join(__dirname, "n65-iso.cjs"), planFile, id, o, "--no-card-marks"]);
      if (r.code !== 0) throw new Error("n65-iso " + planFile + " " + id + ": " + r.out);
      return { file: o, how: "`node Tools/night/n65-iso.cjs " + label + " " + id + " <tmp> --no-card-marks`" };
    };
    const mine = iso(PLAN, "round", "<the round's plan>"), red = iso(RED_PLAN, "red", RED_PLAN);
    const h = crypto.createHash("sha256").update(id + ":" + sha).digest("hex");
    const roundIsA = parseInt(h[0], 16) % 2 === 0;
    const [A, B] = roundIsA ? [mine, red] : [red, mine];
    for (const [src, dst] of [[A.file, "spot-A.png"], [B.file, "spot-B.png"]]) {
      const r = run("python", ["-I", BLIND, src, path.join(OUT, dst)]);
      if (r.code !== 0 || !fs.existsSync(path.join(OUT, dst))) throw new Error("n65-blind " + src + ": " + r.out);
    }
    fs.mkdirSync(path.dirname(KEY), { recursive: true });
    fs.writeFileSync(KEY, `# NIGHT70 GATE spot key for ${id} (NOT for the spotter): sha256("${id}:${sha}") = ${h.slice(0, 16)}..., first hex digit ${h[0]} (${roundIsA ? "even" : "odd"})\nA = ${roundIsA ? "the round (" + round + ")" : "the red (" + (T67 ? T67.base : "n52-foundry") + ")"}: ${A.how}\nB = ${roundIsA ? "the red (" + (T67 ? T67.base : "n52-foundry") + ")" : "the round (" + round + ")"}: ${B.how}\n`);
    note("spot-A.png / spot-B.png", `the round's and the red's iso65 pictures, both drawn now with --no-card-marks (no line65 / frames65), through \`python -I ${BLIND} <in> <out>\`; A / B by sha256("${id}:${sha}")'s first hex digit; the key is ${path.relative(ROOT, KEY).replace(/\\/g, "/")} (outside this folder)`);
  }
  // ---- (5) card.md, room-diff.txt, checks.txt, open-findings.md
  {
    let card;
    const cardPath = T67 ? "Research/2026-10-24-wave67/cards/" + id + ".md" : "Research/2026-10-23-wave66/plans/" + id + ".md"; // NIGHT70 T1c: the area card
    try { card = git(["show", sha + ":" + cardPath]); } catch (e) { card = "not found: " + cardPath + " at " + sha + "\n"; }
    if (/^not found: /.test(card)) MISSING.push((T67 ? "card" : "plan") + ": " + cardPath + " at " + sha);
    write(T67 ? "card.md" : "plan.md", card);
    note(T67 ? "card.md" : "plan.md", `\`git show ${sha}:${cardPath}\``);
  }
  {
    let diff;
    try {
      const a = "rooms-base-" + id + ".json", b = "rooms-" + id + "-" + round + ".json";
      fs.writeFileSync(path.join(tmp, a), git(["show", sha + ":" + (T67 ? T67.roomsBase : "Design/levels/n65/rooms-n52") + "/" + id + ".json"]));
      fs.writeFileSync(path.join(tmp, b), git(["show", sha + ":" + (T67 ? T67.rooms : "Design/levels/n65/rooms") + "/" + id + ".json"]));
      diff = run("git", ["-c", "core.autocrlf=false", "-c", "core.safecrlf=false", "diff", "--no-index", "--no-color", a, b], { cwd: tmp }).out;
    } catch (e) { diff = "not found: " + e.message.split("\n")[0]; }
    // NIGHT70 T1c round 2 (verdict-T1c-r2.md A2): no diff (the files not found) is a missing piece; an EMPTY diff means the round's room is the
    // base's room unchanged: said in the file (a rebuilt room's GATE needs a change: M4)
    if (/^not found: /.test(diff)) MISSING.push("room-diff: " + diff.slice(0, 120));
    else if (!diff.trim()) { diff = "EMPTY: the round's room file equals the base's export (no change to judge)\n"; MISSING.push("room-diff: empty (the room equals the base's export)"); }
    write("room-diff.txt", `# git diff --no-index of ${sha}:${T67 ? T67.roomsBase : "Design/levels/n65/rooms-n52"}/${id}.json (the base's export) against ${sha}:${T67 ? T67.rooms : "Design/levels/n65/rooms"}/${id}.json (the round's)\n\n${diff}`);
    note("room-diff.txt", `\`git diff --no-index\` of the two files as \`git show ${sha}:...\` prints them`);
  }
  {
    const sect = [];
    const pickFlow = (t) => (t || "").split("\n").filter((l) => /^#|^n46flow: seeds|SUMMARY|STYLE|^  style|^  room /.test(l) || new RegExp("^\\s*" + id + "\\s").test(l));
    const redDirs = T67 ? [["the red (" + T67.base + "'s room: Logs/n70/red-" + L67 + ")", path.join(LOGS, "red-" + L67, id)]] : [["the red (the shipped slots: n52-foundry's rooms)", path.join(LOGS, "red", id)], ["the red65 (NIGHT65's final rooms)", path.join(LOGS, "red65", id)]];
    for (const [tag, dir] of [["the round (" + round + ")", path.join(RD, id)]].concat(redDirs)) {
      sect.push(`==== ${tag}: ${path.relative(ROOT, dir).replace(/\\/g, "/")}`);
      for (const f of ["room-check.txt", "flow-A.txt", "flow-B.txt", "flow-V.txt", "pads.txt", "ride.txt"]) {
        const t = read(path.join(dir, f));
        if (t === null) { if (f !== "ride.txt") { sect.push(`-- ${f}: not found`); MISSING.push("checks: " + path.relative(ROOT, path.join(dir, f)).replace(/\\/g, "/")); } continue; }
        sect.push(`-- ${f}`);
        sect.push(...(f.startsWith("flow") ? pickFlow(t) : t.trimEnd().split("\n")));
      }
      sect.push("");
    }
    write("checks.txt", sect.join("\n"));
    note("checks.txt", `${round}/${id}/ and ${redDirs.map((d) => path.relative(ROOT, d[1]).replace(/\\/g, "/")).join(", ")}: room-check.txt, flow-A / flow-B / flow-V.txt (their headers, the ${id} rows, SUMMARY, STYLE), pads.txt, ride.txt`);
  }
  {
    const files = [];
    // NIGHT70 T1c round 2 (verdict-T1c-r2.md A1): the level review of THAT round only: <logs>/<round>/review-*.md, or the round number's
    // <logs>/r<n>/review-level.md (NIGHT70.md section 4 step 2); a review of another round never counts
    const rn = (/^(r\d+)/.exec(round) || [])[1];
    for (const d of [round].concat(rn && rn !== round ? [rn] : [])) { const dd = path.join(LOGS, d); if (fs.existsSync(dd) && fs.statSync(dd).isDirectory()) for (const f of fs.readdirSync(dd).filter((f) => /^review-.*\.md$/.test(f)).sort()) files.push(d + "/" + f); }
    for (const f of fs.readdirSync(LOGS).filter((f) => /^cardgate.*\.md$/.test(f)).sort()) files.push(f);
    const out = [`# ${id}: open findings (BLOCKING lines, verbatim; the cardgates' ${id} verdict lines)`, ""];
    for (const f of files) {
      const ls = read(path.join(LOGS, f)).split("\n");
      const got = [];
      let inRoom = false, level = 0, grab = false;
      for (let i = 0; i < ls.length; i++) {
        const l = ls[i], h = /^(#+)\s+(.*)$/.exec(l);
        if (h) { const lv = h[1].length; if (inRoom && lv <= level) inRoom = false; if (new RegExp("^" + id + "([^a-z0-9]|$)").test(h[2])) { inRoom = true; level = lv; } grab = false; continue; }
        if (new RegExp("^" + id + ": ").test(l)) { got.push(l); continue; } // a cardgate's verdict line
        if (!inRoom) continue;
        // a BLOCKING line and its continuation: the wrapped lines that follow (indented), or, after a bare "**BLOCKING:**", its whole block,
        // up to a blank line, a heading or a rule
        if (/BLOCKING/.test(l)) { got.push(l); grab = /^\*\*BLOCKING:?\*\*\s*$/.test(l.trim()) || /BLOCKING:\s*$/.test(l.trim()) ? "block" : "wrap"; continue; }
        if (grab) {
          if (l.trim() === "" || /^---/.test(l)) grab = false;
          else if (grab === "block" || /^\s+\S/.test(l) || !/^(- |\* |\d+\. )/.test(l)) got.push(l);
          else grab = false;
        }
      }
      if (got.length) out.push(`## ${f}`, "", ...got, "");
    }
    // NIGHT70 T1c round 2 (verdict-T1c-r2.md A1): the level review is a GATE piece ("that round read by a level review"): a review file of
    // this round with a `## <id>` section, else MISSING (exit 3)
    const reviews = files.filter((x) => /review-/.test(x));
    const reviewed = reviews.filter((x) => new RegExp("^##\\s+" + id + "([^a-z0-9]|$)", "m").test(read(path.join(LOGS, x)) || ""));
    if (!reviewed.length) {
      MISSING.push("level review: no review of " + round + " (" + path.relative(ROOT, path.join(LOGS, round)).replace(/\\/g, "/") + "/review-*.md" + (rn && rn !== round ? " or " + path.relative(ROOT, path.join(LOGS, rn)).replace(/\\/g, "/") + "/review-level.md" : "") + ") has a '## " + id + "' section" + (reviews.length ? " (read: " + reviews.join(", ") + ")" : " (none exists)"));
      out.push("MISSING: no level review of " + round + " has a '## " + id + "' section" + (reviews.length ? " (read: " + reviews.join(", ") + ")" : " (no review file of the round exists: NIGHT70.md section 4 step 2, the lead writes it before the GATE)"));
    } else if (out.length === 2) out.push("the level review " + reviewed.join(", ") + " has a '## " + id + "' section and no BLOCKING line for " + id);
    write("open-findings.md", out.join("\n"));
    note("open-findings.md", `the BLOCKING lines (and a bare "**BLOCKING:**" block's list) inside the ${id} section of ${files.length ? files.join(", ") : "(no review or cardgate file found for " + round + ")"}, and every line starting "${id}: "`);
  }
  // ---- (5b) NIGHT70 T1c: the round's M1 (area-<id>.txt), M4 (deco-<id>.txt) and levelgen report (the ERROR guard), copied as they are
  for (const [src, dst, what] of [["area-" + id + ".txt", "area.txt", "M1 AREA (n70-area.cjs on the round's folder and its touches)"], ["levelgen-report.md", "levelgen-report.md", "the round's levelgen report (ERROR rows: no more than the base level's own)"]]) {
    let t = read(path.join(RD, src));
    if (t === null) MISSING.push(dst + ": " + round + "/" + src);
    // NIGHT70 T1c fix (verdict-T1c.md problem 2): the base level's own ERROR rows (levels67.json baseErrors, proven by gen-slots --check)
    if (t !== null && dst === "levelgen-report.md" && T67) {
      const rows = t.split("\n").filter((l) => l.includes("| ERROR |")).map((l) => l.split("|")[1].trim());
      const beyond = rows.filter((n) => !T67.baseErrors.includes(n));
      t = "# the guard: ERROR rows no more than the base level's own. " + T67.base + "'s own (Tools/night/levels67.json baseErrors, proven by gen-slots.cjs --check): " + T67.baseErrors.length + (T67.baseErrors.length ? " (" + T67.baseErrors.join("; ") + ")" : "") + "; this round's: " + rows.length + "; beyond the base's: " + beyond.length + (beyond.length ? " (" + beyond.join("; ") + ")" : "") + "\n\n" + t;
    }
    write(dst, t === null ? "not found: " + round + "/" + src + " (the round wrote none)\n" : t);
    note(dst, (t === null ? "NOT FOUND: " : "") + round + "/" + src + ": " + what);
  }
  // M4: the decoration checker re-run on the ROUND's room file (the record above), the round's touches (verdict-T1c.md problem 4)
  {
    const touches = path.join(RD, "verbs-skip.txt");
    const d = run(process.execPath, [path.join(__dirname, "n70-deco.cjs"), id, "--file", ROUND_ROOM, "--touches", touches].concat(T67 ? ["--level", L67] : ["--base", sha]));
    const ok = /^n70-deco: (PASS|FAIL) \|/m.test(d.out);
    if (!ok) MISSING.push("deco: n70-deco.cjs gave no verdict (" + d.out.trim().split("\n").pop().slice(0, 160) + ")");
    write("deco.txt", "# command: node Tools/night/n70-deco.cjs " + id + " --file <the round's room file: " + path.relative(ROOT, ROUND_ROOM).replace(/\\/g, "/") + "> --touches " + path.relative(ROOT, touches).replace(/\\/g, "/") + (T67 ? " --level " + L67 : " --base " + sha) + "\n\n" + d.out);
    note("deco.txt", "M4: n70-deco.cjs on the round's room file (" + path.relative(ROOT, ROUND_ROOM).replace(/\\/g, "/") + "), the round's touches; its base the room file's first commit");
  }
  // ---- (6) README.md
  write("README.md", (MISSING.length ? "MISSING: " + MISSING.length + " piece(s) the GATE needs came back not found: " + MISSING.join("; ") + "\n\n" : "COMPLETE: every piece the GATE names was found\n\n") + `# GATE pack: ${id}, round ${round}\n\nWritten by \`bash Tools/night/n70-gate-pack.sh ${id} ${round}\` at sha ${sha} (from ${shaFrom}), ${new Date().toISOString().slice(0, 16).replace("T", " ")} UTC. The round's plan: ${planHow}; the level for the --verbs65 skip: ${LEVEL_GIVEN ? LEVEL : "the scratch level of that assembly (levelgen + the finish inside the scratch tree)"}; the logs: ${path.relative(ROOT, LOGS).replace(/\\/g, "/") || LOGS}. Nothing in this folder names which picture is the old room.\n\n${readme.join("\n")}\n`);
  if (MISSING.length) { console.log("n70-gate-pack " + id + " " + round + ": MISSING " + MISSING.length + ": " + MISSING.join("; ")); process.exitCode = 3; }
  console.log(`n70-gate-pack ${id} ${round}: ${path.relative(ROOT, OUT).replace(/\\/g, "/")} (${fs.readdirSync(OUT).length} files: ${fs.readdirSync(OUT).sort().join(", ")}); key ${path.relative(ROOT, KEY).replace(/\\/g, "/")}; sha ${sha}`);
} finally {
  fs.rmSync(tmp, { recursive: true, force: true });
}

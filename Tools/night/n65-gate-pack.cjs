// NIGHT65 builder T (the GATE evidence pack; NIGHT65.md section 4 step 4, rule 9 "reviewers see evidence, not the story"): one room's folder
// Logs/n65/gate/<id>/ holding everything its GATE critic and blind spotter read, and nothing else. Read only on the tree: it writes the pack
// folder and the spot key, never Design/levels (iso pictures it must draw go to a temporary folder, deleted after).
//   bash Tools/night/n65-gate-pack.sh <id> <round-tag> [--logs Logs/n65] [--plan Design/levels/n65/foundry.plan.json]
//        [--level Design/levels/n65-foundry.json] [--out Logs/n65/gate/<id>] [--key Logs/n65/gate/keys/<id>-spot-key.txt] [--sha <sha>]
//        [--blind Tools/night/n65-blind.py] [--no-skip]
// The pack:
//   table.md         the room's row of <logs>/<round>/table.md (which carries the red in brackets), with its header
//   verbs.md         the room's verb-pilot and skip lines: the round's (<logs>/<round>/verbs.md, verbs-*.txt) against the red's
//                    (Research/2026-10-23-wave65/T/verb-*-red.txt, verb-skip-verbs65-red.txt); with verb65 on the room's row of the level, a
//                    fresh `n65verb skip --verbs65` on the level (saved as verbs65-skip.txt)
//   air.md           n65-air.cjs on the round's plan against the red's plan (Design/levels/n52/foundry.plan.json)
//   spot-A.png / spot-B.png   the round's iso65 picture and the red's, both through n65-blind.py; A / B by a fair coin seeded from the room
//                    id and the sha (sha256 of "<id>:<sha>", its first hex digit even: A = the round); the mapping goes to the key file,
//                    OUTSIDE the pack (the spotter never sees it)
//   card.md          the room's card at the sha (Research/2026-10-23-wave65/cards/<id>.md)
//   room-diff.txt    git diff of the room file from n52's export (rooms-n52/<id>.json) to the round's (rooms/<id>.json), both at the sha
//   checks.txt       the round's room-check, flow A / B (the room's rows, SUMMARY, STYLE), pads and ride against the red's
//   open-findings.md the room's BLOCKING lines from every <logs>/r*/review-*.md and the cardgate files (<logs>/cardgate*.md), verbatim
//   README.md        each file with its command and the sha
const fs = require("fs");
const path = require("path");
const os = require("os");
const cp = require("child_process");
const crypto = require("crypto");
const ROOT = path.join(__dirname, "../..");
const args = process.argv.slice(2);
const pos = args.filter((a, i) => !a.startsWith("--") && !(i > 0 && args[i - 1].startsWith("--") && !["--no-skip"].includes(args[i - 1])));
const opt = (k, d) => { const i = args.indexOf(k); return i >= 0 && i + 1 < args.length ? args[i + 1] : d; };
const [id, round] = pos;
if (!id || !round) { console.error("usage: n65-gate-pack.sh <id> <round-tag> [--logs ...] [--plan ...] [--level ...] [--out ...] [--key ...] [--sha ...] [--blind ...] [--no-skip]"); process.exit(2); }
const abs = (p) => (path.isAbsolute(p) ? p : path.join(ROOT, p));
const LOGS = abs(opt("--logs", "Logs/n65")), LEVEL = opt("--level", "Design/levels/n65-foundry.json");
let PLAN = opt("--plan", null), planHow = "--plan";
const OUT = abs(opt("--out", "Logs/n65/gate/" + id)), KEY = abs(opt("--key", "Logs/n65/gate/keys/" + id + "-spot-key.txt"));
const BLIND = opt("--blind", "Tools/night/n65-blind.py"), RED_PLAN = "Design/levels/n52/foundry.plan.json";
const git = (a) => cp.execFileSync("git", a, { cwd: ROOT, encoding: "utf8", maxBuffer: 64 << 20 });
const read = (p) => (fs.existsSync(p) ? fs.readFileSync(p, "utf8").replace(/\r\n/g, "\n") : null);
const RD = path.join(LOGS, round);
if (!fs.existsSync(RD)) { console.error("n65-gate-pack: no round folder " + RD); process.exit(1); }
// the sha: --sha, else the round's assembly header (gen-latest.txt "# sha: x"), else HEAD
let sha = opt("--sha", null), shaFrom = "--sha";
if (!sha) { const g = read(path.join(RD, "gen-latest.txt")); const m = g && /^# sha: ([0-9a-f]{7,40})/m.exec(g); if (m) { sha = m[1]; shaFrom = round + "/gen-latest.txt's header"; } }
if (!sha) { sha = git(["rev-parse", "--short=8", "HEAD"]).trim(); shaFrom = "HEAD (no " + round + "/gen-latest.txt sha)"; }
fs.mkdirSync(OUT, { recursive: true });
const tmp = fs.mkdtempSync(path.join(os.tmpdir(), "n65gate-"));
const readme = [];
const note = (file, cmd, extra) => readme.push(`- **${file}**: ${cmd}${extra ? " " + extra : ""}`);
const write = (file, text) => fs.writeFileSync(path.join(OUT, file), text.endsWith("\n") ? text : text + "\n");
const run = (cmd, a, o) => { const r = cp.spawnSync(cmd, a, Object.assign({ cwd: ROOT, encoding: "utf8", maxBuffer: 64 << 20 }, o || {})); return { code: r.status, out: ((r.stdout || "") + (r.stderr || "")).replace(/\r\n/g, "\n") }; };
const roomLine = (l) => new RegExp("(^|[^a-z0-9])" + id + "([^a-z0-9]|$)").test(l);
let SCRATCH = null; // the scratch tree of the round's assembly (no --plan)
const LEVEL_GIVEN = args.includes("--level");

try {
  // ---- the round's plan: --plan, else assembled from the room files AT THE SHA in a scratch tree (the assembler, n52's plan and the room
  // files as git shows them there; --slots=latest; the scratch tree is deleted after: Design/levels is never written)
  if (!PLAN) {
    const st = path.join(tmp, "tree");
    SCRATCH = st;
    // + the config pair the harness reads beside its working folder, and n52's level (the finish's guard), for a scratch level when needed
    const files = ["Tools/night/gen-n65-foundry.cjs", "Design/levels/n52/foundry.plan.json", "PortExport/config.json", "Design/unity-config.json", "Design/levels/n52-foundry.json"];
    for (const r of ["a2", "a3", "a4", "a5", "b1", "a8"]) files.push("Design/levels/n65/rooms/" + r + ".json", "Design/levels/n65/rooms-n52/" + r + ".json");
    for (const f of files) { fs.mkdirSync(path.join(st, path.dirname(f)), { recursive: true }); fs.writeFileSync(path.join(st, f), git(["show", sha + ":" + f])); }
    const r = run(process.execPath, [path.join(st, "Tools/night/gen-n65-foundry.cjs"), "--slots=latest"], { cwd: st });
    if (r.code !== 0) throw new Error("the scratch assembly at " + sha + " failed: " + r.out.slice(-600));
    PLAN = path.join(st, "Design/levels/n65/foundry.plan.json");
    planHow = "assembled at " + sha + " (`gen-n65-foundry.cjs --slots=latest` on the room files git shows there, in a scratch tree deleted after)";
  }
  // ---- (1) table.md
  {
    const t = read(path.join(RD, "table.md"));
    let body;
    if (!t) body = `not found: ${round}/table.md (the round's table is not written yet)`;
    else {
      const lines = t.split("\n");
      const head = lines.filter((l) => /^\| room /.test(l) || /^\|---/.test(l)).slice(0, 2);
      const row = lines.filter((l) => l.startsWith("| " + id + " "));
      body = (row.length ? head.concat(row) : [`not found: no row for ${id} in ${round}/table.md`]).join("\n");
    }
    write("table.md", `# ${id}: the round's row (${round}/table.md), the red in brackets\n\n${body}\n`);
    note("table.md", `the header and the \`| ${id} \` row of ${path.relative(ROOT, path.join(RD, "table.md")).replace(/\\/g, "/")}`);
  }
  // ---- (2) verbs.md (+ a fresh --verbs65 skip when the level's row has verb65)
  {
    // the room's n65verb header line ("room <id> ..." / "room <id>, ..."), its seed lines (a skip run's) and its verdict line
    const pick = (text) => {
      if (!text) return [];
      const ls = text.split("\n"), out = [];
      let mine = false;
      for (const l of ls) {
        if (/^n65verb [a-z]+: level /.test(l)) { mine = new RegExp("room " + id + "[ ,]").test(l); if (mine) out.push(l); continue; }
        if (/^n65verb [a-z]+: (GREEN|RED)/.test(l)) { if (mine || new RegExp("room " + id + "[ ,|]").test(l)) out.push(l); mine = false; continue; }
        if (mine && /^  (seed \d+:|NOTE)/.test(l)) out.push(l);
      }
      return out;
    };
    // the round's verbs-*.txt (each pilot's own output, the seed lines kept); verbs.md (their digest) only when no .txt names the room
    const roundLines = [];
    for (const f of fs.readdirSync(RD).filter((f) => /^verbs.*\.txt$/.test(f)).sort()) { const p = pick(read(path.join(RD, f))); if (p.length) roundLines.push(`from ${round}/${f}:`, ...p.map((l) => "    " + l)); }
    if (!roundLines.length) { const p = pick(read(path.join(RD, "verbs.md"))); if (p.length) roundLines.push(`from ${round}/verbs.md:`, ...p.map((l) => "    " + l)); }
    const TD = abs("Research/2026-10-23-wave65/T");
    const redFiles = fs.existsSync(TD) ? fs.readdirSync(TD).filter((f) => /^verb-.*-red\.txt$/.test(f)).sort() : [];
    const redLines = [];
    for (const f of redFiles) { const p = pick(read(path.join(TD, f))); if (p.length) redLines.push(`from Research/2026-10-23-wave65/T/${f}:`, ...p.map((l) => "    " + l)); }
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
      const r = run("dotnet", ["run", "--project", path.join(ROOT, "Tools/SimHarness"), "-c", "Release", "--", "n65verb", "skip", "--level", levelFile, "--room", id, "--seeds", "5", "--verbs65"], { cwd: levelCwd });
      const kept = r.out.split("\n").filter((l) => /^n65verb skip|^  seed|^  NOTE/.test(l)).join("\n");
      write("verbs65-skip.txt", `# command: dotnet run --project Tools/SimHarness -c Release -- n65verb skip --level <${where}> --room ${id} --seeds 5 --verbs65\n# sha: ${sha}; the level: ${levelBy}\n# exit: ${r.code}\n\n${kept}\n`);
      skip = kept;
      note("verbs65-skip.txt", `\`dotnet run --project Tools/SimHarness -c Release -- n65verb skip --level <${where}> --room ${id} --seeds 5 --verbs65\` (the round's row's verb65: ${verb65})`);
    }
    write("verbs.md", `# ${id}: the verb pilots and the skip test\n\nThe round (${round}):\n\n${roundLines.length ? roundLines.join("\n") : "    not found: no n65verb line for " + id + " in " + round + "/verbs*.md / .txt"}\n\nThe red (n52-foundry):\n\n${redLines.length ? redLines.join("\n") : "    not found"}\n\nThe round's row (its plan at ${sha}): verb65 ${verb65 || "none"}${verb65 ? (skip ? "; the --verbs65 skip on the round's level (verbs65-skip.txt):\n\n" + skip.split("\n").map((l) => "    " + l).join("\n") : " (--no-skip: not run)") : " (no --verbs65 skip run)"}\n`);
    note("verbs.md", `the \`n65verb\` header, verdict and seed lines naming ${id} from ${round}/verbs*.md / .txt and Research/2026-10-23-wave65/T/verb-*-red.txt`);
  }
  // ---- (3) air.md
  {
    const a = run(process.execPath, [path.join(__dirname, "n65-air.cjs"), PLAN, id]);
    const b = run(process.execPath, [path.join(__dirname, "n65-air.cjs"), RED_PLAN, id]);
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
    fs.writeFileSync(KEY, `# NIGHT65 GATE spot key for ${id} (NOT for the spotter): sha256("${id}:${sha}") = ${h.slice(0, 16)}..., first hex digit ${h[0]} (${roundIsA ? "even" : "odd"})\nA = ${roundIsA ? "the round (" + round + ")" : "the red (n52-foundry)"}: ${A.how}\nB = ${roundIsA ? "the red (n52-foundry)" : "the round (" + round + ")"}: ${B.how}\n`);
    note("spot-A.png / spot-B.png", `the round's and the red's iso65 pictures, both drawn now with --no-card-marks (no line65 / frames65), through \`python -I ${BLIND} <in> <out>\`; A / B by sha256("${id}:${sha}")'s first hex digit; the key is ${path.relative(ROOT, KEY).replace(/\\/g, "/")} (outside this folder)`);
  }
  // ---- (5) card.md, room-diff.txt, checks.txt, open-findings.md
  {
    let card;
    try { card = git(["show", sha + ":Research/2026-10-23-wave65/cards/" + id + ".md"]); } catch (e) { card = "not found: Research/2026-10-23-wave65/cards/" + id + ".md at " + sha + "\n"; }
    write("card.md", card);
    note("card.md", `\`git show ${sha}:Research/2026-10-23-wave65/cards/${id}.md\``);
  }
  {
    let diff;
    try {
      const a = "rooms-n52-" + id + ".json", b = "rooms-" + id + "-" + round + ".json";
      fs.writeFileSync(path.join(tmp, a), git(["show", sha + ":Design/levels/n65/rooms-n52/" + id + ".json"]));
      fs.writeFileSync(path.join(tmp, b), git(["show", sha + ":Design/levels/n65/rooms/" + id + ".json"]));
      diff = run("git", ["diff", "--no-index", "--no-color", a, b], { cwd: tmp }).out;
    } catch (e) { diff = "not found: " + e.message.split("\n")[0]; }
    write("room-diff.txt", `# git diff --no-index of ${sha}:Design/levels/n65/rooms-n52/${id}.json (n52's export) against ${sha}:Design/levels/n65/rooms/${id}.json (the round's)\n\n${diff}`);
    note("room-diff.txt", `\`git diff --no-index\` of the two files as \`git show ${sha}:...\` prints them`);
  }
  {
    const sect = [];
    const pickFlow = (t) => (t || "").split("\n").filter((l) => /^#|^n46flow: seeds|SUMMARY|STYLE|^  style|^  room /.test(l) || new RegExp("^\\s*" + id + "\\s").test(l));
    for (const [tag, dir] of [["the round (" + round + ")", path.join(RD, id)], ["the red (n52-foundry)", path.join(LOGS, "red", id)]]) {
      sect.push(`==== ${tag}: ${path.relative(ROOT, dir).replace(/\\/g, "/")}`);
      for (const f of ["room-check.txt", "flow-A.txt", "flow-B.txt", "pads.txt", "ride.txt"]) {
        const t = read(path.join(dir, f));
        if (t === null) { if (f !== "ride.txt") sect.push(`-- ${f}: not found`); continue; }
        sect.push(`-- ${f}`);
        sect.push(...(f.startsWith("flow") ? pickFlow(t) : t.trimEnd().split("\n")));
      }
      sect.push("");
    }
    write("checks.txt", sect.join("\n"));
    note("checks.txt", `${round}/${id}/ and red/${id}/: room-check.txt, flow-A / flow-B.txt (their headers, the ${id} rows, SUMMARY, STYLE), pads.txt, ride.txt`);
  }
  {
    const files = [];
    // every round folder (r1, r2, r3, r3b, r4 ...: NIGHT65's GATE critic found r3b's reviews skipped by the old /^r\d+$/)
    for (const d of fs.readdirSync(LOGS).filter((f) => /^r\d+[a-z0-9]*$/.test(f) && fs.statSync(path.join(LOGS, f)).isDirectory()).sort()) for (const f of fs.readdirSync(path.join(LOGS, d)).filter((f) => /^review-.*\.md$/.test(f)).sort()) files.push(d + "/" + f);
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
    if (out.length === 2) out.push("none found");
    write("open-findings.md", out.join("\n"));
    note("open-findings.md", `the BLOCKING lines (and a bare "**BLOCKING:**" block's list) inside the ${id} section of ${files.join(", ")}, and every line starting "${id}: "`);
  }
  // ---- (6) README.md
  write("README.md", `# GATE pack: ${id}, round ${round}\n\nWritten by \`bash Tools/night/n65-gate-pack.sh ${id} ${round}\` at sha ${sha} (from ${shaFrom}), ${new Date().toISOString().slice(0, 16).replace("T", " ")} UTC. The round's plan: ${planHow}; the level for the --verbs65 skip: ${LEVEL_GIVEN ? LEVEL : "the scratch level of that assembly (levelgen + the finish inside the scratch tree)"}; the logs: ${path.relative(ROOT, LOGS).replace(/\\/g, "/") || LOGS}. Nothing in this folder names which picture is the old room.\n\n${readme.join("\n")}\n`);
  console.log(`n65-gate-pack ${id} ${round}: ${path.relative(ROOT, OUT).replace(/\\/g, "/")} (${fs.readdirSync(OUT).length} files: ${fs.readdirSync(OUT).sort().join(", ")}); key ${path.relative(ROOT, KEY).replace(/\\/g, "/")}; sha ${sha}`);
} finally {
  fs.rmSync(tmp, { recursive: true, force: true });
}

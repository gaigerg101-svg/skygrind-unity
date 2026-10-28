// NIGHT65 builder T (L-149: "save every output a verdict cites the moment it prints, the command, the sha and the time at its top"): runs one
// command and writes its whole output to a file with that header, then prints the output's last lines. No shell: the command and its arguments
// are passed as they are (a bash script: `bash Tools/night/x.sh ...`).
//   node Tools/night/n65-save.cjs <out.txt> [--tail N] -- <command> [args ...]
const fs = require("fs");
const path = require("path");
const cp = require("child_process");
const ROOT = path.join(__dirname, "../..");
const argv = process.argv.slice(2);
const sep = argv.indexOf("--");
if (sep < 1 || sep === argv.length - 1) { console.error("usage: n65-save.cjs <out.txt> [--tail N] -- <command> [args ...]"); process.exit(2); }
const out = argv[0];
const ti = argv.indexOf("--tail");
const tail = ti >= 0 && ti < sep ? Number(argv[ti + 1]) : 12;
const cmd = argv.slice(sep + 1);
const git = (a) => { try { return cp.execFileSync("git", a, { cwd: ROOT, encoding: "utf8" }).trim(); } catch (e) { return "?"; } };
const sha = git(["rev-parse", "--short=8", "HEAD"]);
const dirty = git(["status", "--short", "--untracked-files=no"]);
const stamp = () => new Date().toLocaleString("sv-SE", { hour12: false });
const t0 = stamp();
const r = cp.spawnSync(cmd[0], cmd.slice(1), { cwd: ROOT, encoding: "utf8", maxBuffer: 256 << 20 });
const body = (r.stdout || "") + (r.stderr ? (r.stdout ? "\n" : "") + r.stderr : "") + (r.error ? "\nspawn error: " + r.error.message : "");
const head = ["# command: " + cmd.join(" "), "# sha: " + sha + " (branch " + git(["rev-parse", "--abbrev-ref", "HEAD"]) + (dirty ? "; tracked changes uncommitted: " + dirty.split(/\r?\n/).length + " file(s)" : "; clean tracked tree") + ")", "# time: " + t0 + " -> " + stamp() + " (local)", "# exit: " + r.status, ""].join("\n");
fs.mkdirSync(path.dirname(path.resolve(ROOT, out)), { recursive: true });
fs.writeFileSync(path.resolve(ROOT, out), head + "\n" + body.replace(/\r\n/g, "\n"));
const lines = body.replace(/\r\n/g, "\n").trimEnd().split("\n");
console.log("saved " + out + " (" + lines.length + " lines, exit " + r.status + ")");
for (const l of lines.slice(-tail)) console.log("  " + l.slice(0, 400));
process.exit(r.status === null ? 1 : r.status);

// NIGHT34 builder M (design.n34.flow34): the run snapshot's slot for the Mover's new field `flow34` (Core/Flow34.cs State: the run's flow rules,
// the wall coyote's clock and normal, the zip carry), for the lead (Core/RunSnapshot.cs is not builder M's file). RunSnapshotTests checks that
// MoverState.Fields + Skipped cover every Mover field, so without this patch that test is red on builder M's commit.
//   1. MoverState.Fields: "flow34" after "stamina" (saved whole, as moveBalance / stamina are: a struct of bools, ints, doubles and Vec3s).
//   2. OptionalSinceNight34 = { "flow34" } and the Load skip: a save written before NIGHT34 has no flow34 and keeps what StartRun set.
// Each anchor must occur exactly once (asserted before any write); a file already patched is refused.
// Run: node Tools/night/n34m-snapshot-patch.cjs [repoRoot] [--dry]
const fs = require("fs");
const path = require("path");
const args = process.argv.slice(2);
const dry = args.includes("--dry");
const root = args.find((a) => !a.startsWith("--")) || path.join(__dirname, "..", "..");
const file = path.join(root, "Assets/Scripts/Core/RunSnapshot.cs");
const raw = fs.readFileSync(file, "utf8");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
if (raw.includes("OptionalSinceNight34")) throw new Error("RunSnapshot.cs: already patched (OptionalSinceNight34 found)");
const edits = [
  [
    '            "stamina", // NIGHT17 U3-30.4 (builder M): the run\'s design.stamina rules, the bar and its clocks (the refill\'s delay, the sprint)',
    '            "stamina", // NIGHT17 U3-30.4 (builder M): the run\'s design.stamina rules, the bar and its clocks (the refill\'s delay, the sprint)\n' +
      '            "flow34", // NIGHT34 M1 (builder M): the run\'s design.n34.flow34 rules and clocks (the wall coyote, the zip carry; Core/Flow34.cs)',
  ],
  [
    '        public static readonly string[] OptionalSinceNight17 = { "stamina" };',
    '        public static readonly string[] OptionalSinceNight17 = { "stamina" };\n\n' +
      "        /// <summary>NIGHT34 M1: a field a NIGHT33 save never wrote. Absent, it keeps what StartRun set (the run's design.n34.flow34 rules,\n" +
      "        /// the clocks off), so an older save resumes.</summary>\n" +
      '        public static readonly string[] OptionalSinceNight34 = { "flow34" };',
  ],
  [
    "if (!r.Has(f.Name) && (Array.IndexOf(OptionalSinceNight16, f.Name) >= 0 || Array.IndexOf(OptionalSinceNight17, f.Name) >= 0)) continue;",
    "if (!r.Has(f.Name) && (Array.IndexOf(OptionalSinceNight16, f.Name) >= 0 || Array.IndexOf(OptionalSinceNight17, f.Name) >= 0 || Array.IndexOf(OptionalSinceNight34, f.Name) >= 0)) continue;",
  ],
];
for (const [from] of edits) {
  const n = raw.split(from.split("\n").join(eol)).length - 1;
  if (n !== 1) throw new Error(`RunSnapshot.cs: anchor found ${n} times (want 1): ${from.slice(0, 120)}`);
}
let out = raw;
for (const [from, to] of edits) out = out.replace(from.split("\n").join(eol), () => to.split("\n").join(eol));
console.log(`Assets/Scripts/Core/RunSnapshot.cs: ${edits.length} edit(s)${dry ? " (dry run, not written)" : ""}`);
if (!dry) fs.writeFileSync(file, out);

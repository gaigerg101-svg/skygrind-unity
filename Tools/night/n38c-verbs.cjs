// NIGHT38 builder C2 (the lead's note 3): marks docs/VERBS.md's rows that spire38 changes INSIDE level 5's flight rooms (flight38 rows of n38-spire:
// BELFRY I-III, THE LONG BELL) as "changed by spire38", each with its check (Harness spire38 meter / gates). Exact-string edits; idempotent.
// Run: node Tools/night/n38c-verbs.cjs
const fs = require("fs");
const path = require("path");
const F = path.join(__dirname, "../../docs/VERBS.md");
let t = fs.readFileSync(F, "utf8");
const TAG = "**changed by spire38";
const edits = [
  ["| 4 | double jump | A in the air / Space | web | 1 | a `DoubleJump` event |",
    "| 4 | double jump | A in the air / Space | web; **changed by spire38 (NIGHT38, n38-spire's flight rooms only)**: once the first dash since a gate's pad has ended, the double jump is gone until a pad or the ground outside (design.n38.spire38.meter.oneDash) | 1 | a `DoubleJump` event; **spire38 check** (harness `spire38 gates`): in a gate room no jump + dash + double line lands past the gap (control, meter off: it lands 3.4-4.4 m past) |"],
  ["| 5 | dash | B or RB-less dash bind / Shift-dash (design.move) | web | 1 | a `Dash` event and > 4 m covered in 0.3 s |",
    "| 5 | dash | B or RB-less dash bind / Shift-dash (design.move) | web; **changed by spire38 (NIGHT38, n38-spire's flight rooms only)**: a gate's pad holds the bar at one jump-dash (0.75 + 0.05), the bar does not refill there, and the first dash since the pad spends it (design.n38.spire38.meter) | 1 | a `Dash` event and > 4 m covered in 0.3 s; **spire38 check** (harness `spire38 meter`): the pad's bar 0.80, 0.00 after the jump-dash and 2 s on the landing, 0.80 on the next pad; the control (off) refills to 1.00 |"],
  ["| 8 | jump dash (dash-jump) | dash then jump / same | web | 5 (taught) | a `DashJump` or `BoostJump` event |",
    "| 8 | jump dash (dash-jump) | dash then jump / same | web; **changed by spire38 (NIGHT38, n38-spire's flight rooms only)**: one per gate pad (the gate meter); BELFRY I teaches it on its gate | 5 (taught) | a `DashJump` or `BoostJump` event; **spire38 check** (harness `spire38 gates`): from each gate's pad the dash-then-jump lands 6.75 m past the near edge (margin >= 37.5 %) |"],
  ["| 17 | grapple pull to a node | LB tap / E (design.n21.arcline, GrappleTap31) | n21 arcline, n31 grapplePull, n32 grapple32; **changed by D-2**: owned from level 4 only (`grapple35`) |",
    "| 17 | grapple pull to a node | LB tap / E (design.n21.arcline, GrappleTap31) | n21 arcline, n31 grapplePull, n32 grapple32; **changed by D-2**: owned from level 4 only (`grapple35`); **changed by spire38 (NIGHT38, n38-spire's flight rooms only)**: the grapple is off while the feet are in a gate room (design.n38.spire38.meter.noHook), back at its edge; **spire38 check** (harness `spire38 meter`): ropeEnabled false in BELFRY I, true in THE NAVE |"],
  ["| 18 | grapple pull to a bare wall | LB tap at a wall | **changed by grapple35**: pulls straight to the hit point (NIGHT32 tapNodesOnly refused it) |",
    "| 18 | grapple pull to a bare wall | LB tap at a wall | **changed by grapple35**: pulls straight to the hit point (NIGHT32 tapNodesOnly refused it); **changed by spire38**: off in n38-spire's gate rooms, as row 17 |"],
  ["| 29 | THUNDERCLAP fire | RT / LMB | web shotgun |",
    "| 29 | THUNDERCLAP fire | RT / LMB | web shotgun; **changed by spire38 (NIGHT38, n38-spire's gate rooms only)**: the room LIMIT `pistolOnly` (Campaign24's, banner \"ONE JUMP-DASH PER PAD\") takes the THUNDERCLAP (and its mid-air boost) and the TORCH for the room; the owned list comes back at the door (Campaign24's limit rule); **spire38 check**: the room rows carry `limit: [\"pistolOnly\"]` (Design/levels/n38-spire.json) and Campaign24's restore is its own tested rule; the Player check is the lead's |"],
];
let n = 0;
for (const [a, b] of edits) {
  if (t.includes(b)) continue;
  const i = t.indexOf(a);
  if (i < 0 || t.indexOf(a, i + 1) >= 0) throw new Error("row not found exactly once: " + a.slice(0, 40));
  t = t.slice(0, i) + b + t.slice(i + a.length);
  n++;
}
fs.writeFileSync(F, t);
console.log(`VERBS.md: ${n} row(s) marked; rows with "${TAG}": ${(t.match(/\*\*changed by spire38/g) || []).length}`);

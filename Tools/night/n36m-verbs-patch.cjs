// NIGHT36 builder M: patches FeelReel.N35.Verbs.cs so the verbs35 scenario also serves verbs36 (FeelReel.N36.Move.cs): the tag of its lines, rows 9 / 25 /
// 27 / 30 as docs/VERBS.md changed them tonight (wallgrip36's rise, tap36's press step, tap36's 1 normal + 1 charged), and the NIGHT36 rows 40-43 before the
// close. Each replacement must match exactly once. Run: node Tools/night/n36m-verbs-patch.cjs
const fs = require("fs");
const path = require("path");
const file = path.join(__dirname, "..", "..", "Assets", "Scripts", "Game", "FeelReel.N35.Verbs.cs");
let s = fs.readFileSync(file, "utf8");
const eol = s.includes("\r\n") ? "\r\n" : "\n";
function rep(a, b) {
  a = a.replace(/\n/g, eol);
  b = b.replace(/\n/g, eol);
  const n = s.split(a).length - 1;
  if (n !== 1) throw new Error("expected 1 match, got " + n + ": " + a.slice(0, 80));
  s = s.replace(a, () => b);
}

// the tag
rep(`            Debug.Log("[reel] verbs35: " + line);`, `            Debug.Log("[reel] " + verbsTag + ": " + line);`);
rep(`            Debug.Log("[reel] verbs35: start " + pc);`, `            Debug.Log("[reel] " + verbsTag + ": start " + pc);`);
rep(`            Debug.Log($"[reel] verbs35: total {v.pass}/{v.performed}");`, `            Debug.Log($"[reel] {verbsTag}: total {v.pass}/{v.performed}");`);
// the verb names of rows 40-43
rep(`            foreach (var r in Verbs35Rows) if (r.n == n) verb = r.verb;`, `            foreach (var r in Verbs35Rows) if (r.n == n) verb = r.verb;
            foreach (var r in Verbs36Table) if (r.n == n) verb = r.verb; // NIGHT36 M (FeelReel.N36.Move.cs)`);
// row 9: the rise above each ride's attach point (wallgrip36)
rep(`            public double rideStartT = double.NaN, rideEndT = double.NaN, rideSpeed, wallJumpSpeed, slideSpeed;`,
  `            public double rideStartT = double.NaN, rideEndT = double.NaN, rideSpeed, wallJumpSpeed, slideSpeed;
            /// <summary>NIGHT36 M (wallgrip36, VERBS.md row 9): the current ride's attach height and the most any ride rose above its attach point.</summary>
            public double rideAttachY = double.NaN, rideRise;
            /// <summary>NIGHT36 M (tap36, VERBS.md row 25): the sim step of the last "fire" event.</summary>
            public int lastFireStep = -1;`);
rep(`if (e.type == MoverEventType.WallRideStart) { if (double.IsNaN(v.rideStartT)) v.rideSpeed = e.speed;`,
  `if (e.type == MoverEventType.WallRideStart) { v.rideAttachY = m.position.y; if (double.IsNaN(v.rideStartT)) v.rideSpeed = e.speed;`);
rep(`                    if (t < 0.05) m.velocity = new Vec3(wAlong.x * 11 - wNormal.x * 0.5, m.velocity.y, wAlong.z * 11 - wNormal.z * 0.5);`,
  `                    if (t < 0.05) m.velocity = new Vec3(wAlong.x * 11 - wNormal.x * 0.5, m.velocity.y, wAlong.z * 11 - wNormal.z * 0.5);
                    if (m.state == MoveState.WallRide && !double.IsNaN(v.rideAttachY)) v.rideRise = Math.Max(v.rideRise, m.position.y - v.rideAttachY); // NIGHT36 M (wallgrip36)`);
rep(`                if (!sim.n35.wallride35) Verb35(v, 9, "RED", "not landed (wallride35 off, D-6): " + rm);
                else Verb35(v, 9, rode && rideS > 1.2 ? "PASS" : "RED", rm);`,
  `                bool grip36 = sim.n36.wallgrip36; // NIGHT36 M (VERBS.md row 9, changed by F-5): a jump into the wall rises at most 1.5 m above the attach point
                rm += $"; the most a ride rose above its attach point {v.rideRise.ToString("0.00", ci)} m (wallgrip36 {grip36}{(grip36 ? ": at most 1.5" : "")})";
                if (!sim.n35.wallride35) Verb35(v, 9, "RED", "not landed (wallride35 off, D-6): " + rm);
                else Verb35(v, 9, rode && rideS > 1.2 && (!grip36 || v.rideRise <= 1.5) ? "PASS" : "RED", rm);`);
// row 25: the press step (tap36)
rep(`            Action<object> onFire = o => { string gun = o as string ?? "?"; v.fires[gun] = v.F(gun) + 1; };`,
  `            Action<object> onFire = o => { string gun = o as string ?? "?"; v.fires[gun] = v.F(gun) + 1; v.lastFireStep = sim.simStep; };`);
rep(`                int k0 = v.F("revolver");
                yield return Do35(g, 0.4, t => g.forcedCombat = new CombatInput { weapon = -1, fire = new Btn { pressed = t < 0.02, held = t < 0.03 } }, wallEnd);
                Verb35(v, 25, v.F("revolver") > k0 ? "PASS" : "RED", $"KESTREL shots on one tap {v.F("revolver") - k0}");`,
  `                int k0 = v.F("revolver");
                // NIGHT36 M (VERBS.md row 25, changed by F-1): a 0.1 s click (a real click: 50-100 ms); with tap36 the shot comes on the press step (the first
                // step of the frame that pressed), charge35 alone fires it on the release (12+ steps later)
                int pressStep = -1, fireStep0 = v.lastFireStep;
                yield return Do35(g, 0.4, t => { if (pressStep < 0) pressStep = sim.simStep; g.forcedCombat = new CombatInput { weapon = -1, fire = new Btn { pressed = t < 0.02, held = t < 0.1 } }; }, wallEnd);
                bool tapLive = sim.charge35 != null && sim.charge35.tap36;
                int lag = v.lastFireStep != fireStep0 && pressStep >= 0 ? v.lastFireStep - pressStep : -1;
                string tm = $"KESTREL shots on one 0.1 s click {v.F("revolver") - k0}, the first {(lag < 0 ? "-" : lag.ToString(ci))} sim steps after the press frame (tap36 {tapLive}{(tapLive ? ": on the press, at most 2" : "")})";
                Verb35(v, 25, v.F("revolver") > k0 && (!tapLive || (lag >= 0 && lag <= 2)) ? "PASS" : "RED", tm);`);
// rows 27 / 30: tap36's 1 normal + 1 charged
rep(`            else Verb35(v, row, fired == 1 && starts == 1 && before == 0 ? "PASS" : "RED", m);`,
  `            else
            {
                int wantBefore = g.sim.charge35 != null && g.sim.charge35.tap36 ? 1 : 0; // NIGHT36 M (VERBS.md row 27, changed by F-1): tap36's press fires one normal shot first
                Verb35(v, row, fired == 1 && starts == 1 && before == wantBefore ? "PASS" : "RED", m + $" (want {wantBefore} normal before: tap36 {wantBefore == 1})");
            }`);
// rows 40-43 before the close (verbs36 only)
rep(`            Verb35(v, 37, "SKIP", "SURVIVAL wall buys / the shop's own reel (VERBS.md: not performed)");
`,
  `            Verb35(v, 37, "SKIP", "SURVIVAL wall buys / the shop's own reel (VERBS.md: not performed)");
            if (verbsTag == "verbs36") yield return Verbs36Rows(g, v, open, openYaw, wallEnd); // NIGHT36 M: VERBS.md rows 40-43 (FeelReel.N36.Move.cs)
`);
fs.writeFileSync(file, s);
console.log("FeelReel.N35.Verbs.cs patched");

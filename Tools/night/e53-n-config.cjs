// EXT53 builder N (row N, RIDE KILL): adds design.n53.rideKill53 (a sim row, OFF) after polish53, in Design/unity-config.json and its byte copy
// Assets/StreamingAssets/Design/unity-config.json.
//   node Tools/night/e53-n-config.cjs [rootDir] [--dry]   (refuses to run twice; asserts its anchor exactly once BEFORE writing anything)
// The web has no such numbers (it has no reward for a wall ride): they live in the Unity design config (CLAUDE.md rule 5).
const fs = require("fs");
const dry = process.argv.includes("--dry");
const rootArg = process.argv.slice(2).find((a) => !a.startsWith("--"));
const root = (rootArg || process.cwd()).replace(/\\/g, "/").replace(/\/?$/, "/");
const file = root + "Design/unity-config.json", copy = root + "Assets/StreamingAssets/Design/unity-config.json";
const raw = fs.readFileSync(file, "utf8");
if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ");
const d = JSON.parse(raw);
if (!d.design.n53 || !d.design.n53.polish53) throw new Error("design.n53.polish53 is missing");
if (d.design.n53.rideKill53) throw new Error("design.n53.rideKill53 already exists (already run?)");
if (Object.keys(d.design).pop() !== "n53") throw new Error("design.n53 must be the last key of design");
if (Object.keys(d.design.n53).pop() !== "polish53") throw new Error("polish53 must be the last key of design.n53");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
// the anchor: the end of polish53's numbers, polish53, design.n53, design, the root
const tail = eol + "        }" + eol + "      }" + eol + "    }" + eol + "  }" + eol + "}";
const count = raw.split(tail).length - 1;
if (count !== 1 || !raw.endsWith(tail)) throw new Error("the anchor (the config's ending) was found " + count + " time(s) / not at the end");
const row = {
  sim: true,
  _why: "EXT53 builder N (row N; GG w35, 2026-10-12, docs/memory/OWNER.md: \"The rooms still don't incentivize wall-riding or creative use of the weapons\"): RIDE KILL. A player's kill landed while riding a wall (Mover state WallRide), or within numbers.graceS after the ride (the step the ride ended counts as a riding step), pays the rider back numbers.charges boosts (a dash charge: charges x design.stamina.boost.cost of the bar with the bar on, charges x dash.pipCost pips without; capped at the max), on top of the kill's own refill (design.stamina.rewards / n19), and emits `rideKill53` (payload RideKill53Event: the HUD's RIDE KILL callout, GG's sounds). The verb pair is Titanfall 2's wall-run gunplay; the payout is ULTRAKILL / DOOM Eternal's resource-for-aggression rule that design.stamina.rewards already follows. Core/RideKill53.cs (the rule), SurvivalSim.N53.RideKill.cs (the sim), UiRideKill53.cs (the callout). The web has none of these numbers (no reward for a wall ride).",
  enabled: false,
  enabled_why: "Off until the lead's a253 Player proof prints GREEN with the row on and RED with -skygrindOff53 (EXT53 row N, Tools/night/e53-rows.json) and verbs37 loses nothing; golden 22/22 exact and the fingerprints EQUAL plain / --off52 / --off53 with the row in config (measured by builder N).",
  numbers: {
    graceS: 0.4,
    graceS_why: "s after the ride's last riding step a kill still counts, as whole steps (MovementMath.StepsFor: 48 at 120 Hz). It covers the jump-off shot (VERBS row 11, the jump-off boost) and a shot fired on the wall that lands just after (a KESTREL charge, a short rocket); ride43's lost-wall grace and the flow34 coyote / buffer windows are 0.1-0.15 s, a rocket flies 30 m/s (12 m in 0.4 s), so 0.4 s keeps it a ride verb, not a 'rode recently' buff.",
    charges: 1,
    charges_why: "boosts paid back per ride kill: the bar's 1 x design.stamina.boost.cost 0.5 (half the 1.0 bar, one full boost), the pips' 1 x dash.pipCost. One keeps a ride chain going (ride, kill, boost to the next wall) without refilling the whole bar on one shot (the parry's design.stamina.rewards.parry 1 is the whole bar); a multi-kill on the wall pays each kill, capped at the bar's max."
  }
};
const block = JSON.stringify(row, null, 2).split("\n").map((l, i) => (i === 0 ? l : "      " + l)).join(eol);
const out = raw.slice(0, raw.length - tail.length) + eol + "        }" + eol + "      }," + eol + "      \"rideKill53\": " + block + eol + "    }" + eol + "  }" + eol + "}";
const back = JSON.parse(out);
if (JSON.stringify(back.design.n53.rideKill53) !== JSON.stringify(row)) throw new Error("rideKill53 did not round-trip");
const a = JSON.parse(out);
delete a.design.n53.rideKill53;
if (JSON.stringify(a) !== JSON.stringify(d)) throw new Error("something beyond design.n53.rideKill53 changed");
if (dry) { console.log("e53-n-config: 1 row planned (dry run, nothing written): design.n53.rideKill53, " + (out.length - raw.length) + " bytes, 2 files"); process.exit(0); }
fs.writeFileSync(file, out);
fs.writeFileSync(copy, out);
console.log("e53-n-config: design.n53.rideKill53 written (enabled false) to Design/unity-config.json and its StreamingAssets copy");

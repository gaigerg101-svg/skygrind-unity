// NIGHT52 the room loop, fix 4 (the lead's): g7's retry spawn 3.5 m further in. In the chained level walk (n52-flow-table.sh, Logs/n52/flow-mid)
// iceline's level dwell rose 13.9 -> 18.5 s, g7 carrying most of it (5.4 -> 10.1 s on seeds 1-10, with 7 deaths): every retry puts the
// player back at the room's spawn, 2 m inside the door, inside the 4 m dwell zone, so a death re-starts the fight camped at the door. The
// retry point moves to the quay's middle (0, 0, -189.5): 5.5 m from the door wall, between the bunkers' fronts and the mooring posts (builder F
// did the same in a5). The entry itself is unchanged (the door, the waves, the nodes); the frames52 entry spot stays at the door.
// Asserts fix 3's state. Run: node Tools/night/n52-loop-fix4.cjs, then bash Tools/night/n52-loop-regen.sh iceline g7
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN = path.join(ROOT, "Design/levels/n52/iceline.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
const g7 = plan.rooms.find((r) => r.id === "g7");
if (!/Fix 3/.test(g7.loop52_why || "") || JSON.stringify(g7.spawn.at) !== "[0,0,-186]") throw new Error("g7 is not fix 3's (already run?)");
g7.spawn = { at: [0, 0, -189.5], yawDeg: 0 };
g7.spawn_why = "NIGHT52 fix 4: the retry point on the quay's middle, 5.5 m from the door wall (outside the door's 4 m band): a death never restarts the fight camped at the door";
g7.loop52_why += " Fix 4 (n52-loop-fix4.cjs): the retry spawn 3.5 m in.";
for (let i = 0; i < plan.rooms.length; i++) if (plan.rooms[i].id !== "g7" && JSON.stringify(plan.rooms[i]) !== JSON.stringify(before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
for (const k of Object.keys(before)) if (k !== "rooms" && JSON.stringify(before[k]) !== JSON.stringify(plan[k])) throw new Error(k + " changed");
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("n52-loop-fix4: g7's retry spawn at (0, 0, -189.5)");

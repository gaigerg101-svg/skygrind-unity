// NIGHT32 (for the lead; written by builder C, NOT applied by C): builder C's lines in lead-owned files: THE CROWN takes level 5's boss room (F's plan: room v9, plan boss id
// "crown") when design.n32.boss5 is on. With the row off Crown32Begin returns false and BossRoomSystem("crown") is null (the room falls back as any unknown boss id does today).
// The view (Game/Crown32View.cs) installs itself (RuntimeInitializeOnLoadMethod, as GrappleNodes30): no SurvivalGame line is needed.
// Run once from the main repo: node Tools/night/n32-crown-wire.cjs
const fs = require("fs");
const root = "C:/Users/gaige/dev/SKYGRIND-unity/";
function patch(file, pairs) {
  let s = fs.readFileSync(root + file, "utf8");
  const crlf = s.includes("\r\n");
  if (crlf) s = s.replace(/\r\n/g, "\n");
  for (const [a, b] of pairs) {
    const n = s.split(a).length - 1;
    if (n !== 1) throw new Error(file + ": anchor found " + n + " times: " + a.slice(0, 80));
    s = s.replace(a, b);
  }
  if (crlf) s = s.replace(/\n/g, "\r\n");
  fs.writeFileSync(root + file, s);
  console.log("patched " + file);
}

patch("Assets/Scripts/Game/SurvivalSim.Boss25.cs", [
  // BossRoomSystem: the plan id "crown" reads THE CROWN with the row on (BossRoomDefeated goes through it)
  ["public IBossSystem BossRoomSystem(string id) => id == \"loom\" ? (IBossSystem)loom30 : ",
   "public IBossSystem BossRoomSystem(string id) => id == \"crown\" ? (n32.boss5 ? (IBossSystem)crown32 : null) : id == \"loom\" ? (IBossSystem)loom30 : "],
  // BossRoomBegin: start (or retry) THE CROWN
  ["            if (id == \"loom\") return Loom30Begin(out spawn, out yawDeg); // NIGHT30 B",
   "            if (id == \"crown\") return Crown32Begin(out spawn, out yawDeg); // NIGHT32 C: THE CROWN, LEVEL 5's boss (SurvivalSim.Crown32.cs; false with design.n32.boss5 off)\n            if (id == \"loom\") return Loom30Begin(out spawn, out yawDeg); // NIGHT30 B"],
]);

// NIGHT36 (the lead, for builder B): BossRoomSystem asks B's N36BossSystem first, so BossRoomDefeated and every reader of the room's
// boss see THE LANTERN on Boss36 while boss36 owns the fight. Run once: node Tools/night/n36-bosssystem-hook.cjs
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
  ["        public IBossSystem BossRoomSystem(string id) => id == \"crown\"",
   "        public IBossSystem BossRoomSystem(string id)\n        {\n            IBossSystem b36 = null;\n            N36BossSystem(id, ref b36); // NIGHT36 B: boss36's boss for this id while it owns the fight (SurvivalSim.Boss36.cs)\n            return b36 ?? BossRoomSystem25(id);\n        }\n\n        IBossSystem BossRoomSystem25(string id) => id == \"crown\""],
]);
patch("Assets/Scripts/Game/SurvivalSim.N36.cs", [
  ["        partial void N36BossBegin(string id, ref bool handled, ref bool ok, ref Vec3 spawn, ref double yawDeg); // NIGHT36 B: first in BossRoomBegin\n",
   "        partial void N36BossBegin(string id, ref bool handled, ref bool ok, ref Vec3 spawn, ref double yawDeg); // NIGHT36 B: first in BossRoomBegin\n" +
   "        partial void N36BossSystem(string id, ref IBossSystem boss); // NIGHT36 B: first in BossRoomSystem (set boss to own the id)\n"],
]);

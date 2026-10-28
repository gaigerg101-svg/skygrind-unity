// NIGHT32 (the lead): level 5's TORCH is a LEVEL award (the last room's clear, Campaign24's level unlock), not a room-1 award: the
// unlocks31 checks (room 1) expect no award for n32-spire. Run once: node Tools/night/n32-lead-lists2.cjs
const fs = require("fs");
const root = "C:/Users/gaige/dev/SKYGRIND-unity/";
for (const f of ["Assets/Scripts/Game/FeelReel.N31.Weapons.cs", "Tools/SimHarness/Harness.N31.Weapons.cs"]) {
  let s = fs.readFileSync(root + f, "utf8");
  const a = '"nailgun" }, "torch")';
  const n = s.split(a).length - 1;
  if (n !== 1) throw new Error(f + ": anchor found " + n + " times");
  s = s.replace(a, '"nailgun" }, (string)null) /* the TORCH comes with the level clear, not in room 1 */');
  fs.writeFileSync(root + f, s);
  console.log("patched " + f);
}

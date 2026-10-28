// NIGHT32 (the lead): the typed level lists in older reel / harness files learn tonight's level ids (A2 risk 1; A, S, F named the lines):
// the weapon rooms rooms31 frames, the unlocks31 reel's start sets and awards, the unlocks31 harness's level list.
// Run once: node Tools/night/n32-lead-lists.cjs
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

patch("Assets/Scripts/Game/FeelReel.N31.Levels.cs", [
  ["            { \"n30-span\", new[] { \"s1\" } },\n        };",
   "            { \"n30-span\", new[] { \"s1\" } },\n" +
   "            { \"n32-foundry\", new[] { \"f8\", \"f9\" } }, // NIGHT32: the rebuilt levels keep their weapon rooms (A, S); level 5 starts with all four (F)\n" +
   "            { \"n32-kiln\", new[] { \"k1\" } },\n" +
   "            { \"n32-span\", new[] { \"s1\" } },\n" +
   "        };"],
]);

patch("Assets/Scripts/Game/FeelReel.N31.Weapons.cs", [
  ["                [\"n30-span\"] = (new[] { \"revolver\", \"gauntlet\", \"shotgun\" }, \"nailgun\"),\n",
   "                [\"n30-span\"] = (new[] { \"revolver\", \"gauntlet\", \"shotgun\" }, \"nailgun\"),\n" +
   "                [\"n32-foundry\"] = (new[] { \"revolver\" }, null), // NIGHT32: the swapped levels (design.n32.arenas32 / span32) and LEVEL 5 (level5)\n" +
   "                [\"n32-kiln\"] = (new[] { \"revolver\", \"gauntlet\" }, \"shotgun\"),\n" +
   "                [\"n32-span\"] = (new[] { \"revolver\", \"gauntlet\", \"shotgun\" }, \"nailgun\"),\n" +
   "                [\"n32-spire\"] = (new[] { \"revolver\", \"gauntlet\", \"shotgun\", \"nailgun\" }, \"torch\"),\n"],
]);

patch("Tools/SimHarness/Harness.N31.Weapons.cs", [
  ["            (\"Design/levels/n30-span.json\", new[] { \"revolver\", \"gauntlet\", \"shotgun\" }, \"nailgun\"),\n        };",
   "            (\"Design/levels/n30-span.json\", new[] { \"revolver\", \"gauntlet\", \"shotgun\" }, \"nailgun\"),\n" +
   "            (\"Design/levels/n32-foundry.json\", new[] { \"revolver\" }, (string)null), // NIGHT32: the swapped levels and LEVEL 5\n" +
   "            (\"Design/levels/n32-kiln.json\", new[] { \"revolver\", \"gauntlet\" }, \"shotgun\"),\n" +
   "            (\"Design/levels/n32-span.json\", new[] { \"revolver\", \"gauntlet\", \"shotgun\" }, \"nailgun\"),\n" +
   "            (\"Design/levels/n32-spire.json\", new[] { \"revolver\", \"gauntlet\", \"shotgun\", \"nailgun\" }, \"torch\"),\n" +
   "        };"],
]);

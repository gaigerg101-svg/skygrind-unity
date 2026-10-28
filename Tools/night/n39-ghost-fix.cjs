// NIGHT39 G0 (b) (the lead): the ghost weapon on level 3. n35-kiln's room k1 awards "thunderclap" (a display name), Campaign24.cs:850-853
// hands r.award to Loadout.AddWeapon, which adds ANY string (Combat.cs:943), and Combat fires by weapon id ("revolver", "shotgun",
// "nailgun", ...): an owned "thunderclap" is a slot that never fires (GG 2026-10-13: "another weapon that doesn't do anything and acts like
// a ghost weapon"). The fix is the id: "thunderclap" -> "shotgun" in n35-kiln (both copies). A data fix, deliberately not behind --off39
// (it is a bug; NIGHT39-LOG records the exception). Older kiln files (n29-n34) are not in the campaign order and stay as they are.
// Run once: node Tools/night/n39-ghost-fix.cjs
const fs = require("fs");
const root = "C:/Users/gaige/dev/SKYGRIND-unity/";
for (const f of ["Design/levels/n35-kiln.json", "Assets/StreamingAssets/Design/levels/n35-kiln.json"]) {
  const s = fs.readFileSync(root + f, "utf8");
  const n = s.split('"award":"thunderclap"').length - 1;
  if (n !== 1) throw new Error(f + ": expected one award thunderclap, found " + n);
  const out = s.replace('"award":"thunderclap"', '"award":"shotgun"');
  JSON.parse(out);
  fs.writeFileSync(root + f, out);
  console.log("fixed " + f);
}

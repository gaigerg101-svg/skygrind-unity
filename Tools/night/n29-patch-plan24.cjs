// NIGHT29 (builder R): patch Assets/Editor/Maps/LevelPlan24.cs for the room standard (LevelPlan29): per-room wall height, extra boxes, extra campaign keys.
const fs = require("fs");
const p = "Assets/Editor/Maps/LevelPlan24.cs";
let t = fs.readFileSync(p, "utf8");
const NL = String.fromCharCode(10), CR = String.fromCharCode(13);
const eol = t.includes(CR + NL) ? CR + NL : NL;
function rep(a, b) { a = a.split(NL).join(eol); b = b.split(NL).join(eol); if (!t.includes(a)) throw new Error("missing: " + a); t = t.replace(a, () => b); }
rep(
  "foreach (R2 p in pieces) if (!p.Empty) walls.Add(Box(p.x0, 0, p.z0, p.x1, wallH, p.z1, $\"wall of {S(O(L(plan[\"rooms\"])[i]), \"id\")}\"));",
  "foreach (R2 p in pieces) if (!p.Empty) walls.Add(Box(p.x0, 0, p.z0, p.x1, LevelPlan29.RoomHeight(O(L(plan[\"rooms\"])[i]), wallH), p.z1, $\"wall of {S(O(L(plan[\"rooms\"])[i]), \"id\")}\")); // NIGHT29: a room's own `height`");
rep(
  "list.Add(new Dictionary<string, object> { [\"zone\"] = S(t, \"zone\"), [\"kind\"] = \"hull\", [\"min\"] = Arr(r.x0, c, r.z0), [\"max\"] = Arr(r.x1, c + 2, r.z1), [\"note\"] = \"tunnel ceiling\" });\n            }\n            return list;",
  "list.Add(new Dictionary<string, object> { [\"zone\"] = S(t, \"zone\"), [\"kind\"] = \"hull\", [\"min\"] = Arr(r.x0, c, r.z0), [\"max\"] = Arr(r.x1, c + 2, r.z1), [\"note\"] = \"tunnel ceiling\" });\n            }\n            list.AddRange(LevelPlan29.ExtraBoxes(plan, WallThick)); // NIGHT29: roofs, door lintels, collider props (only for a plan with \"n29\": true)\n            return list;");
rep(
  "foreach (string key in new[] { \"layer\", \"rank\", \"weapons\", \"links\", \"pits\", \"tunnels\", \"surfaces\", \"n27\", \"wallHeight\" }) if (plan.ContainsKey(key)) campaign[key] = plan[key];",
  "foreach (string key in new[] { \"layer\", \"rank\", \"weapons\", \"links\", \"pits\", \"tunnels\", \"surfaces\", \"n27\", \"wallHeight\" }) if (plan.ContainsKey(key)) campaign[key] = plan[key];\n            foreach (string key in LevelPlan29.CampaignKeys) if (plan.ContainsKey(key)) campaign[key] = plan[key]; // NIGHT29: n29, lights, props");
fs.writeFileSync(p, t);
console.log("patched", p);

// NIGHT40 builder B4: writes design.n40.loom40 (its row ONLY, L-049) in Design/unity-config.json from the object below, then copies the file to
// Assets/StreamingAssets/Design/unity-config.json (the two stay byte-identical). Idempotent: run it after each change to the row below.
// Run from the worktree root: node Tools/night/n40-loom40-row.cjs
const fs = require("fs");
const path = require("path");
const root = path.resolve(__dirname, "..", "..");
const file = path.join(root, "Design", "unity-config.json");
const copy = path.join(root, "Assets", "StreamingAssets", "Design", "unity-config.json");
const raw = fs.readFileSync(file, "utf8");
if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ before the patch");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const d = JSON.parse(raw);
const old = d.design.n40.loom40;
if (!old) throw new Error("design.n40.loom40 missing");

const row = {
  _why: old._why,
  enabled: old.enabled,
  enabled_why: old.enabled_why,
  levelFrom: old.levelFrom,
  levelTo: old.levelTo,
  dropWeapons: old.dropWeapons,
  dropWeapons_why: old.dropWeapons_why,
  marginM: 10,
  marginM_why: "m: THE LOOM's stations and body centre stay inside THE CANYON's (o3) floor plan minus this (boss_rules 4). Its feet reach 9.8 m along the canyon from the centre (kneeSpreadZ 7 x 1.4) and its knees 9 m across (kneeSpanX), so 10 keeps the whole machine inside. MEASURED before (n40loom stay, loom40 off): a camper in o2 dragged the body 43.7 m out of o3.",
  glideMax: 18,
  glideMax_why: "m/s: the top speed of THE LOOM's glide between stations and onto a camper (stationSpeed's 36-40 m/s crossed a 36 m station step in one second: a jump cut, GG 'often runs out of the map'). 18 m/s is about the player's dash speed: a move the eye follows. Ours.",
  glideAccel: 24,
  glideAccel_why: "m/s2: the glide eases up at this and brakes onto its goal (v <= sqrt(2 a d)): 0.75 s to full speed, a 36 m station step in ~2.75 s. Ours.",
  homeS: 6,
  homeS_why: "s after the last anti-hide before the leg's station goes back to its own place (NIGHT39 never put it back; the MMO leash: return home when the chase ends). Longer than the hide rule's 3 s (design.n37.boss37.hideSeconds), so a player still hiding keeps it over them.",
  legHoldMaxS: 3,
  legHoldMaxS_why: "s: a leg change is held open (no attacks) until the body arrives at its new station, at most this past the change's own 1 s (design.n37.loom37.kit.legChangeSeconds): the glide is now ~2.75 s, and the leg change is the phase's transition (boss_rules 6: 1-2 s announced). The old 36 m/s glide existed to fit the 1 s change (stationSpeed_why).",
  floorY: -24,
  floorY_why: "m: THE CANYON's void floor (n38-span / n40-span o3: the floor boxes' bottom, design.n37.loom37.fallY_why 'the void floor at -24'): THE LOOM's death fall stops with its feet on it (footDropY above it). MEASURED before: the fall went 27.6 m under it.",
  lineS: 5,
  lineS_why: "s each boss line stands (Core/BossLine40, B5's: boss_rules 5 'the damage rule is taught on entry'): teach35's showS (design.n35.teach35.showS 5).",
  lineRule: "HOOK A LIT KNEE {grapple}. THEN {grapple} PULL AWAY.",
  lineRule_why: "THE LOOM's damage rule on entry (the lead's line, NIGHT40 round 2): the grapple hooks a lit knee (it latches), a pull to another thread tears it (Core/Enemies/Loom37.N40.cs Latch40).",
  lineLatch: "HOOKED. NOW {grapple} PULL AWAY TO A THREAD.",
  lineLatch_why: "shown on the first two latches of a fight: the second half of the rule, at the moment it applies.",
  lineOpen: "OPEN: SHOOT THE CORE. EVERY GUN COUNTS.",
  lineOpen_why: "shown on the first CYAN window: every weapon lands in full there (Boss37 Scale: Open -> 1).",
  noRideWarnS: 1,
  noRideWarnS_why: "s the canyon's next no-ride walls blink before they refuse a ride (D-12: 'switched at the leg change behind a 1 s warning flash').",
  practiceBackS: 3,
  practiceBackS_why: "s after o2's practice knee tears before it is back (a rehearsal: try again at once). Ours.",
  spoolTurnDeg: 90,
  spoolTurnDeg_why: "deg/s a live spool turns toward the player's chest, its speed kept (boss_rules 2: its answer is the parry, a dash past it still works). MEASURED round 3 (n40loom stay --on): with straight spools a strafing pilot was never close enough to parry, and legs 2-3 need one parry (design.n39.boss39 loom stagger 7.5): the leg waited 19 s on lashes until the pilot died.",
  tearHeal: 20,
  tearHeal_why: "health a torn knee gives back (Doom Eternal's blood punch: the aggressive move refills). MEASURED round 3: the deaths were falls (-20 each, design.n37.loom37.fallCostHp) plus a lash (-30); the tear is the fight's right move.",
};
for (const k of Object.keys(old)) if (!(k in row)) throw new Error("the row lost key " + k);

// find the row's text: `      "loom40": {` .. the matching `      }` at the same indent
const startKey = eol + '      "loom40": {';
const at = raw.indexOf(startKey);
if (at < 0) throw new Error("loom40 row not found");
const endKey = eol + "      }";
const end = raw.indexOf(endKey, at + startKey.length);
if (end < 0) throw new Error("loom40 row end not found");
const block = JSON.stringify(row, null, 2).split("\n").map((l, i) => (i === 0 ? l : "      " + l)).join(eol);
const out = raw.slice(0, at) + eol + '      "loom40": ' + block + raw.slice(end + endKey.length);
const check = JSON.parse(out);
const others = JSON.parse(raw);
delete others.design.n40.loom40;
const others2 = JSON.parse(out);
delete others2.design.n40.loom40;
if (JSON.stringify(others) !== JSON.stringify(others2)) throw new Error("the patch touched more than design.n40.loom40");
fs.writeFileSync(file, out);
fs.copyFileSync(file, copy);
console.log("design.n40.loom40: " + Object.keys(check.design.n40.loom40).filter((k) => !k.endsWith("_why")).join(", "));

// NIGHT42 builder W1: adds W1's numbers to ITS OWN rows only (design.n42.parry42 / charge42 / reticle42; gauntlet42's swingSeconds came in the
// shared commit), as text inserted after each row's "enabled_why" line (L-049: no reformat of the file), then copies the config to
// Assets/StreamingAssets/Design/ (the two stay byte-identical). Idempotent: a key already in its row is left alone. Never touches "enabled".
// Run from the worktree root: node Tools/night/n42w1-config.cjs
const fs = require("fs");
const path = require("path");
const root = path.resolve(__dirname, "..", "..") + "/";
const file = root + "Design/unity-config.json";
const copy = root + "Assets/StreamingAssets/Design/unity-config.json";
let raw = fs.readFileSync(file, "utf8");
if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ before the patch");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";

const rows = {
  parry42: [
    ["reachMult", 1.5, "DECIDE42 D-3: the orb's reach 1.5x orb.reflectRange (2 m -> 3 m), measured as a volume: the orb's surface to the body's axis (eye down to the capsule's lower sphere), not the eye's point (ULTRAKILL 1.03b: 'changed from a raytrace into a hitbox', hitfeel-web.md section 4)."],
    ["mercyS", 0.1, "ULTRAKILL 1.03b: '3 mercy frames between a projectile hitting the player and the damage happening' (50 ms at 60 fps); doubled to 0.1 s (12 steps) because GG misses 'despite feeling like you hit it': a press in the same rendered frame as the touch at 30-60 fps plus input latency still parries. The orb waits at the contact point meanwhile."],
    ["orbBufferS", 0.24, "DECIDE42 D-3: parry36's early buffer (0.12 s) doubled for orbs only: a punch or a gauntlet swing that sent nothing back still parries an orb that comes into reach within 0.24 s (an orb at 12 m/s covers 2.9 m: about the new reach)."],
  ],
  charge42: [
    ["cannonTurnS", 0.46, "G8 (GG: 'complete animation before you can preform necxt attack'): after a charged shot (KESTREL or THUNDERCLAP) the gun's next shot waits this long, the reticle's cannon turn (spin + snap; reticle42 draws it in this time). 0.46 s is inside the KESTREL's tap interval (0.483 s, design.n39.p39 x1.2), so a tap right after the cannon loses at most nothing of its rhythm; NIGHT41's turn was 1.82 s and a tap fired through it (CODE-FACTS G10 / G8)."],
  ],
};

for (const [row, keys] of Object.entries(rows)) {
  const start = raw.indexOf('"' + row + '": {');
  if (start < 0) throw new Error("row not found: " + row);
  const end = raw.indexOf("}", start);
  let body = raw.slice(start, end);
  const ew = body.indexOf('"enabled_why":');
  if (ew < 0) throw new Error("no enabled_why in " + row);
  const lineStart = raw.lastIndexOf(eol, start + ew) + eol.length;
  const indent = raw.slice(lineStart, start + ew);
  // the end of the row's last key line (the line before the closing brace)
  const closeLine = raw.lastIndexOf(eol, end);
  let insert = "";
  for (const [k, v, why] of keys) {
    if (body.includes('"' + k + '":')) continue;
    insert += "," + eol + indent + JSON.stringify(k) + ": " + JSON.stringify(v) + "," + eol + indent + JSON.stringify(k + "_why") + ": " + JSON.stringify(why);
  }
  if (!insert) continue;
  raw = raw.slice(0, closeLine) + insert + raw.slice(closeLine);
}
JSON.parse(raw); // still valid
fs.writeFileSync(file, raw);
fs.writeFileSync(copy, raw);
const d = JSON.parse(raw).design.n42;
for (const r of Object.keys(rows)) console.log(r, JSON.stringify(Object.fromEntries(Object.entries(d[r]).filter(([k]) => !k.endsWith("_why")))));

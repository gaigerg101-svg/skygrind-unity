// NIGHT30 (the lead): resolves a Design/unity-config.json cherry-pick conflict where HEAD only flipped a row's "enabled" and the
// picked commit added keys after it: keep HEAD's enabled value, take the picked side's added keys.
const fs = require("fs");
const f = "C:/Users/gaige/dev/SKYGRIND-unity/Design/unity-config.json";
let s = fs.readFileSync(f, "utf8");
const nl = s.includes("\r\n") ? "\r\n" : "\n";
let n = 0;
for (;;) {
  const a = s.indexOf("<<<<<<< "), m = s.indexOf("=======", a), z = s.indexOf(">>>>>>> ", m);
  if (a < 0) break;
  const ours = s.slice(s.indexOf("\n", a) + 1, m).replace(/\r?\n$/, "");
  const theirs = s.slice(s.indexOf("\n", m) + 1, z).replace(/\r?\n$/, "");
  const en = ours.match(/"enabled": (true|false)/);
  if (!en || ours.trim().split(/\r?\n/).length !== 1) throw new Error("unexpected HEAD side: " + ours.slice(0, 120));
  const merged = theirs.replace(/"enabled": (true|false)/, '"enabled": ' + en[1]);
  s = s.slice(0, a) + merged + nl + s.slice(s.indexOf("\n", z) + 1);
  n++;
}
JSON.parse(s);
fs.writeFileSync(f, s);
console.log("config merged, blocks: " + n);

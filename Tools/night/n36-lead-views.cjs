// NIGHT36 (the lead): B's Boss36View in the boss views' init / render (SurvivalGame.Boss26.cs), and L's last-kill slow in the loop's
// time layer (SurvivalGame.Impact.cs, after SlowAimFrame). Run once: node Tools/night/n36-lead-views.cjs
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
patch("Assets/Scripts/Game/SurvivalGame.Boss26.cs", [
  ["            hulkView = new HulkView(this);\n",
   "            hulkView = new HulkView(this);\n            Boss36View.Current = new Boss36View(this); // NIGHT36 B: THE LANTERN on Boss36 (bossmodel36, the hall)\n"],
  ["            hulkView?.Render(alpha);\n",
   "            hulkView?.Render(alpha);\n            Boss36View.Current?.Render(alpha); // NIGHT36 B\n"],
]);
patch("Assets/Scripts/Game/SurvivalGame.Impact.cs", [
  ["            share = SlowAimFrame(frameDt, share);",
   "            share = Arena36SlowFrame(frameDt, share); // NIGHT36 L (the lead: SurvivalGame.Arena36.cs): an arena room's last kill slows the world a moment\n            share = SlowAimFrame(frameDt, share);"],
]);

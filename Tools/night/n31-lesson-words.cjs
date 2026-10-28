// NIGHT31 (the lead, the freeze suite): two lessons31 lines were 7 words (Lesson25TextTests: at most 6). Shortens them in the
// generators, the plans and the generated levels (every copy, so a regen keeps them). Run once: node Tools/night/n31-lesson-words.cjs
const fs = require("fs");
const root = "C:/Users/gaige/dev/SKYGRIND-unity/";
const pairs = [
  ["RIDE THE WEST WALL OVER THE LAVA.", "RIDE THE WEST WALL OVER LAVA."],
  ["IT CHARGES. GLIDE SIDEWAYS. PUNCH ITS BACK.", "IT CHARGES. DODGE. PUNCH ITS BACK."],
];
const files = [
  "Design/levels/n30/foundry.json", "Design/levels/n30/foundry.plan.json", "Design/levels/n30-foundry.json", "Tools/night/gen-n30-foundry.cjs",
  "Design/levels/n30/iceline.json", "Design/levels/n30/iceline.plan.json", "Design/levels/n30-iceline.json", "Tools/night/gen-n30-iceline.cjs",
];
for (const f of files) {
  let s = fs.readFileSync(root + f, "utf8");
  let n = 0;
  for (const [a, b] of pairs) { const k = s.split(a).length - 1; n += k; s = s.split(a).join(b); }
  fs.writeFileSync(root + f, s);
  console.log(f + ": " + n + " replaced");
}

// NIGHT33 builder R, slice 1 (run once): makes Tools/night/gen-n33-<name>.cjs as COPIES of the five NIGHT32 generators with the level id n33-<name> and the
// output Design/levels/n33/<name>.plan.json. Every room id, pickup, boss room, lesson and spawn rule stays the n32 generator's. Each replacement asserts its anchor
// exactly once. Refuses to overwrite an existing gen-n33 file. Run from the repo (worktree) root: node Tools/night/n33r-copy-gens.cjs
const fs = require("fs");

function once(t, a, b, file) {
  const n = t.split(a).length - 1;
  if (n !== 1) throw new Error(`${file}: anchor found ${n} times: ${a}`);
  return t.replace(a, b);
}

const HEAD = (name) => `// NIGHT33 builder R (DESIGN33 card R): a copy of gen-n32-${name}.cjs with the level id n33-${name} and the output Design/levels/n33/${name}.plan.json
// (design.n33.kit33 swaps n32-${name} for n33-${name}). R reshapes rooms here; builder K's dressing pass (Tools/night/dress33.cjs) runs on the plan after it.
// Run: bash Tools/night/n33-regen.sh ${name}
`;

const jobs = {
  foundry: [
    ['const OUT = "Design/levels/n32/foundry.plan.json";', 'const OUT = "Design/levels/n33/foundry.plan.json";'],
    ['this.plan.id = "n32-foundry";', 'this.plan.id = "n33-foundry";'],
    ['this.plan._why = "NIGHT32 builder A', 'this.plan._why = "NIGHT33 builder R (design.n33.kit33 swaps n32-foundry for this file): Tools/night/gen-n33-foundry.cjs = gen-n32-foundry.cjs. NIGHT32 builder A'],
  ],
  iceline: [
    ['const OUT = "Design/levels/n32/iceline.plan.json";', 'const OUT = "Design/levels/n33/iceline.plan.json";'],
    ['this.plan.id = "n32-iceline";', 'this.plan.id = "n33-iceline";'],
    ['this.plan._why = "NIGHT32 builder A', 'this.plan._why = "NIGHT33 builder R (design.n33.kit33 swaps n32-iceline for this file): Tools/night/gen-n33-iceline.cjs = gen-n32-iceline.cjs. NIGHT32 builder A'],
  ],
  kiln: [
    ['const OUT = "Design/levels/n32/kiln.plan.json";', 'const OUT = "Design/levels/n33/kiln.plan.json";'],
    ['this.plan.id = "n32-kiln";', 'this.plan.id = "n33-kiln";'],
    ['this.plan._why = "NIGHT32 builder A', 'this.plan._why = "NIGHT33 builder R (design.n33.kit33 swaps n32-kiln for this file): Tools/night/gen-n33-kiln.cjs = gen-n32-kiln.cjs. NIGHT32 builder A'],
  ],
  span: [
    ['const L = new Level("n32-span", "LEVEL 4 THE SPAN",\n', 'const L = new Level("n33-span", "LEVEL 4 THE SPAN",\n  "NIGHT33 builder R (design.n33.kit33 swaps n32-span for this file): Tools/night/gen-n33-span.cjs = gen-n32-span.cjs. " +\n'],
    ['L.write("Design/levels/n32/span.plan.json");', 'L.write("Design/levels/n33/span.plan.json");'],
  ],
  spire: [
    ['const L = new Level("n32-spire", "LEVEL 5 THE SPIRE",\n', 'const L = new Level("n33-spire", "LEVEL 5 THE SPIRE",\n  "NIGHT33 builder R (design.n33.kit33 swaps n32-spire for this file): Tools/night/gen-n33-spire.cjs = gen-n32-spire.cjs. " +\n'],
    ['L.write("Design/levels/n32/spire.plan.json");', 'L.write("Design/levels/n33/spire.plan.json");'],
  ],
};

for (const [name, reps] of Object.entries(jobs)) {
  const src = `Tools/night/gen-n32-${name}.cjs`, dst = `Tools/night/gen-n33-${name}.cjs`;
  if (fs.existsSync(dst)) throw new Error(`${dst} exists: refusing to overwrite`);
  let t = fs.readFileSync(src, "utf8").replace(/\r\n/g, "\n");
  for (const [a, b] of reps) t = once(t, a, b, src);
  fs.writeFileSync(dst, HEAD(name) + t);
  console.log("wrote", dst);
}

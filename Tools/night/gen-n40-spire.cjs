// NIGHT40 builder B5 (crown40; NIGHT40.md B5, method 2: level edits go to a NEW level id): level 5 as n40-spire. It is n38-spire's plan
// (Design/levels/n38/spire.plan.json, written by Tools/night/gen-n38-spire.cjs, which this file does not run or edit) copied whole, with
// NIGHT40's changes applied on the copy, so `--off40` keeps n38-spire bit for bit:
//   1. the id `n40-spire` and its _why
//   2. v10 THE RING's empty lesson prompt filled with design.n40.crown40.line.ringPrompt ("{dash} + {jump}: THROUGH THE RING": the pass the
//      CROWN's fight asks for, with the same two buttons as its entry line; six words, the room-lesson limit). The lesson is NIGHT37's
//      `reached` rect past the gap: it clears when he lands across. The widget resolves {dash} / {jump} to the bound buttons.
// Web coordinates (x east, y up, z south). Run from the repo (or worktree) root: bash Tools/night/n40-spire-regen.sh
const path = require("path");
const fs = require("fs");

const ROOT = path.join(__dirname, "../..");
const SRC = path.join(ROOT, "Design/levels/n38/spire.plan.json");
const OUT = path.join(ROOT, "Design/levels/n40/spire.plan.json");
const CFG = JSON.parse(fs.readFileSync(path.join(ROOT, "Design/unity-config.json"), "utf8"));
const LINE = CFG.design.n40.crown40.line;
if (!LINE || !LINE.ringPrompt) throw new Error("design.n40.crown40.line.ringPrompt missing (Tools/night/n40-crown40-config.cjs writes it)");

const plan = JSON.parse(fs.readFileSync(SRC, "utf8"));
if (plan.id !== "n38-spire") throw new Error(`unexpected source id ${plan.id}`);
plan.id = "n40-spire";
plan._why = "NIGHT40 builder B5 (design.n40.crown40 swaps n38-spire for this file while the row is on): Tools/night/gen-n40-spire.cjs copies "
  + "n38-spire's plan whole and changes only: v10 THE RING's lesson prompt (the pass taught with the CROWN's buttons). n38-spire's _why: " + plan._why;

const room = (id) => {
  const r = plan.rooms.find((x) => x.id === id);
  if (!r) throw new Error(`room ${id} missing`);
  return r;
};

// 2. v10 THE RING: the prompt (both lesson lists: NIGHT25's and NIGHT31's)
const v10 = room("v10");
for (const list of [v10.lessons, v10.lessons31]) {
  if (!Array.isArray(list) || list.length !== 1) throw new Error("v10's lessons are not NIGHT37's single reached rect");
  list[0].prompt = LINE.ringPrompt;
  list[0].promptPad = LINE.ringPrompt;
}
v10.lesson40_why = "B5: v10's prompt was empty since NIGHT37 (nothing on screen said what the ring is for); now the pass with the same two buttons as the CROWN's entry line (design.n40.crown40.line.ringPrompt, six words: the room-lesson limit), so the fight's rule is rehearsed here first.";

fs.mkdirSync(path.dirname(OUT), { recursive: true });
fs.writeFileSync(OUT, JSON.stringify(plan, null, 1) + "\n");
console.log("wrote", path.relative(ROOT, OUT), plan.rooms.length, "rooms; v10 prompt:", JSON.stringify(LINE.ringPrompt));
